# Dimensions 架构文档

## 概述

Dimensions 是一个 Terraria 代理服务器，采用分层架构设计，实现了客户端与服务端之间的透明代理，同时提供服务器切换、状态保持等高级功能。

## 架构图

```
┌─────────────────────────────────────────────────────────────────┐
│                         客户端 (Terraria)                        │
└───────────────────────────┬─────────────────────────────────────┘
                            │ TCP
┌───────────────────────────▼─────────────────────────────────────┐
│                         Listener                                │
│                    (TCP 连接监听)                                │
└───────────────────────────┬─────────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────────┐
│                          Client                                 │
│              (客户端连接管理、处理器协调)                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │  PacketClient│  │   Tunnel    │  │      ClientHandler      │  │
│  │  (客户端连接)│  │ (数据包隧道) │  │      (处理器链)          │  │
│  └──────┬──────┘  └──────┬──────┘  └─────────────────────────┘  │
│         │                │                                       │
│         └────────────────┘                                       │
│                   │                                              │
│         ┌─────────▼──────────┐                                   │
│         │   PacketClient     │                                   │
│         │   (服务端连接)      │                                   │
│         └──────────┬─────────┘                                   │
└────────────────────┼────────────────────────────────────────────┘
                     │ TCP
┌────────────────────▼────────────────────────────────────────────┐
│                      服务端 (Terraria)                           │
└─────────────────────────────────────────────────────────────────┘
```

## 核心模块

### 1. 网络层

#### Listener
- **职责**: 监听 TCP 连接，接受客户端连接
- **关键方法**:
  - `ListenThread()`: 主监听循环
  - `OnAcceptClient()`: 处理新连接

#### PacketClient
- **职责**: 封装 TcpClient，提供数据包级别的读写
- **关键特性**:
  - 使用 PacketSerializer 进行序列化/反序列化
  - 支持自定义 DimensionUpdate 数据包
  - 处理 ISideSpecific 接口（用于 C2S/S2C 字段区分）
- **关键方法**:
  - `Send(INetPacket)`: 发送数据包
  - `Send(DimensionUpdate)`: 发送自定义数据包
  - `Receive()`: 接收数据包
  - `ListenThread()`: 监听线程

#### Tunnel
- **职责**: 在两个 PacketClient 之间转发数据包
- **事件**:
  - `OnReceive`: 接收到数据包时触发
  - `OnError`: 发生错误时触发
  - `OnClose`: 连接关闭时触发
- **方向**:
  - C2S: 客户端到服务端
  - S2C: 服务端到客户端

### 2. 业务逻辑层

#### Client
- **职责**: 管理单个客户端连接的完整生命周期
- **核心功能**:
  - 维护客户端和服务端连接
  - 管理数据包隧道
  - 协调各个处理器
  - 处理服务器切换
- **关键方法**:
  - `TunnelTo(Server)`: 连接到指定服务器
  - `ChangeServer(Server)`: 切换服务器
  - `SendClient/SendServer`: 发送数据包
  - `Disconnect`: 断开连接

#### ClientHandler (抽象基类)
- **职责**: 定义处理器接口
- **事件方法**:
  - `OnCommonPacket`: 通用数据包处理
  - `OnS2CPacket`: 服务端到客户端数据包
  - `OnC2SPacket`: 客户端到服务端数据包
  - `OnCleaning`: 清理事件（切换服务器时）

### 3. 处理器层

| 处理器 | 职责 | 关键数据 |
|--------|------|----------|
| CommandHandler | 处理 /server 命令 | - |
| ConnectionHandler | 处理连接初始化 | SyncPlayer, WorldData |
| CustomPacketHandler | 处理 DimensionUpdate | DimensionUpdate |
| NpcHandler | NPC 状态追踪 | activeNpc[200] |
| ProjectileHandler | 投射物追踪 | projOwner[1000] |
| PlayerHandler | 其他玩家追踪 | activePlayers[254] |
| ItemHandler | 物品追踪 | activeItem[401] |
| PylonHandler | 传送塔追踪 | activePylon[9] |
| SSCHandler | SSC 数据处理 | equipments, syncPlayer |

### 4. 数据包层

#### DimensionUpdate (自定义数据包)
```csharp
MessageID: Unused67 (67)
SubType: SubMessageID
  - ClientAddress = 1        // 传递客户端 IP
  - ChangeSever = 2          // 切换服务器
  - ChangeCustomizedServer = 3 // 自定义服务器
  - OnlineInfoRequest = 4    // 在线信息请求
  - OnlineInfoResponse = 5   // 在线信息响应
Content: string
Port: ushort (条件字段)
```

## 数据流

### 连接建立流程

```
1. Listener 接受 TCP 连接
2. 创建 Client 实例
3. Client 接收 ClientHello
4. 创建到默认服务器的连接
5. 建立 C2S 和 S2C 隧道
6. 注册并启动所有处理器
7. 开始数据包转发
```

### 服务器切换流程

```
1. 接收到 /server 命令
2. CommandHandler 解析目标服务器
3. Client.ChangeServer() 被调用
4. 触发所有处理器的 OnCleaning()
   - 清理 NPC、投射物、物品等实体
5. 断开当前服务端连接
6. 连接到新服务器
7. 重新建立隧道
8. 恢复玩家状态（如果是非 SSC）
```

### 数据包处理流程

```
1. PacketClient 接收数据包
2. Tunnel 触发 OnReceive 事件
3. Client 分发到各个处理器
   - OnC2SPacket (客户端到服务端)
   - OnS2CPacket (服务端到客户端)
   - OnCommonPacket (双向)
4. 如果 Handled = true，停止转发
5. 否则通过 Tunnel 转发到目标
```

## 关键技术点

### 1. TrProtocol 集成

Dimensions 基于 TrProtocol 构建，适配了以下特性：

- **INetPacket 接口**: 所有数据包实现此接口
- **ISideSpecific 接口**: 区分 C2S 和 S2C 字段
- **IExtraData 接口**: 处理额外数据（如 SyncNPC.ExtraData）
- **源生成器**: TrProtocol 使用源生成器生成序列化代码

### 2. 实体清理机制

切换服务器时，需要清理客户端显示的实体：

```csharp
// NpcHandler.OnCleaning 示例
for (short i = 0; i < maxNPC; ++i)
{
    if (activeNpc[i])
    {
        Parent.SendClient(new SyncNPC
        {
            NPCSlot = i,
            Bit3 = 1,  // 删除标志
            ExtraData = []  // 必须设置，避免 null
        });
        activeNpc[i] = false;
    }
}
```

### 3. SSC 处理

SSCHandler 处理服务器端角色数据：

- **非 SSC 服务器**: 保存玩家装备、属性到本地
- **SSC 服务器**: 使用服务器保存的角色数据
- **切换时**: 如果是从 SSC 切换到非 SSC，恢复本地保存的数据

### 4. 自定义数据包处理

DimensionUpdate 需要特殊处理：

```csharp
// 序列化
if (packetId == (byte)MessageID.Unused67)
{
    // 手动反序列化
    var dimensionUpdate = new DimensionUpdate();
    dimensionUpdate.ReadContent(ref ptr, end);
    packet = dimensionUpdate;
}
else
{
    // 使用 PacketSerializer
    packet = serializer.Deserialize(br);
}
```

## 扩展指南

### 添加新处理器

1. 创建继承自 `ClientHandler` 的类
2. 重写需要的事件方法
3. 在 `Client.RegisterHandlers()` 中注册

```csharp
public class MyHandler : ClientHandler
{
    public override void OnS2CPacket(PacketReceiveArgs args)
    {
        if (args.Packet is MyPacket packet)
        {
            // 处理数据包
            args.Handled = true; // 如果不需要转发
        }
    }
}
```

### 添加新命令

在 `CommandHandler.OnC2SPacket` 中添加：

```csharp
if (textC2S.Text.StartsWith("/mycommand"))
{
    // 处理命令
    Parent.SendChatMessage("响应消息");
    args.Handled = true;
}
```

## 性能考虑

1. **线程模型**: 每个客户端连接使用独立的 Task 处理
2. **数据包队列**: 使用 BlockingCollection 作为数据包队列
3. **实体追踪**: 使用数组/字典追踪实体状态，避免频繁创建对象
4. **零拷贝**: 尽可能使用 Span<T> 和 Memory<T> 减少拷贝

## 安全考虑

1. **输入验证**: 所有外部输入都经过验证
2. **异常处理**: 所有网络操作都有 try-catch 保护
3. **资源释放**: 连接关闭时正确释放资源
4. **命令限制**: 命令处理器设置 Handled 标志防止滥用
