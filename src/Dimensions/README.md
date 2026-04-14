# Dimensions - Terraria 代理服务器

Dimensions 是一个基于 TrProtocol 的 Terraria 游戏代理服务器，支持多服务器切换、跨服传送等功能。

## 功能特性

- **多服务器支持**: 支持配置多个 Terraria 服务器，玩家可以通过命令切换
- **无缝切换**: 在不同服务器之间切换时保持玩家状态
- **自定义数据包**: 实现了 DimensionUpdate 自定义数据包用于服务器间通信
- **SSC (Server-Side Character) 支持**: 正确处理服务器端角色数据
- **实体同步清理**: 在切换服务器时清理 NPC、投射物、物品等实体
- **命令系统**: 支持 /server 命令切换服务器

## 项目结构

```
Dimensions/
├── Program.cs              # 程序入口点
├── Listener.cs             # TCP 连接监听器
├── Logger.cs               # 日志记录器
├── Serializers.cs          # 数据包序列化器
├── Models/
│   ├── Config.cs           # 配置模型
│   └── Server.cs           # 服务器配置模型
├── Packets/
│   └── DimensionUpdate.cs  # 自定义数据包
└── Core/
    ├── Client.cs           # 客户端连接管理
    ├── ClientHandler.cs    # 客户端处理器基类
    ├── PacketClient.cs     # 数据包客户端
    ├── PacketReceiveArgs.cs # 数据包接收参数
    ├── Tunnel.cs           # 数据包隧道
    ├── CommandHandler.cs   # 命令处理器
    ├── ConnectionHandler.cs # 连接处理器
    ├── CustomPacketHandler.cs # 自定义数据包处理器
    ├── NpcHandler.cs       # NPC 处理器
    ├── ProjectileHandler.cs # 投射物处理器
    ├── PlayerHandler.cs    # 玩家处理器
    ├── ItemHandler.cs      # 物品处理器
    ├── PylonHandler.cs     # 传送塔处理器
    ├── SSCHandler.cs       # SSC 处理器
    ├── MobileDebugHandler.cs # 移动端调试处理器
    └── GlobalTracker.cs    # 全局客户端追踪器
```

## 快速开始

### 1. 配置

创建 `config.json` 文件：

```json
{
  "listenPort": 7654,
  "sendDimensionPacket": false,
  "protocolVersion": "Terraria319",
  "servers": [
    {
      "name": "main",
      "serverIP": "127.0.0.1",
      "serverPort": 7777
    },
    {
      "name": "pvp",
      "serverIP": "127.0.0.1",
      "serverPort": 7778
    }
  ]
}
```

### 2. 运行

```bash
dotnet run
```

### 3. 连接

在游戏中连接 `localhost:7654` (或配置的监听端口)

## 命令

| 命令 | 描述 |
|------|------|
| `/server` | 显示可用服务器列表 |
| `/server <name>` | 切换到指定服务器 |
| `/server list` | 显示可用服务器列表 |

## 架构说明

### 数据包流转

```
客户端 <-> PacketClient <-> Tunnel <-> PacketClient <-> 服务端
```

### 核心组件

#### Client
管理单个客户端连接，包含：
- 客户端连接 (`_client`)
- 服务端连接 (`_serverConnection`)
- 数据包隧道 (`c2s`, `s2c`)
- 处理器列表 (`handlers`)

#### Tunnel
负责在两个 PacketClient 之间转发数据包：
- C2S (Client to Server): 客户端到服务端
- S2C (Server to Client): 服务端到客户端

#### PacketClient
封装 TcpClient，提供数据包级别的读写：
- 使用 PacketSerializer 序列化/反序列化数据包
- 支持自定义 DimensionUpdate 数据包
- 处理 ISideSpecific 接口

#### ClientHandler
处理器基类，提供以下事件：
- `OnCommonPacket`: 通用数据包处理
- `OnS2CPacket`: 服务端到客户端数据包处理
- `OnC2SPacket`: 客户端到服务端数据包处理
- `OnCleaning`: 清理事件（切换服务器时）

### 自定义数据包

DimensionUpdate 是一个自定义数据包，用于：
- 传递客户端真实 IP 地址
- 请求服务器切换
- 自定义服务器连接

数据包结构：
```
MessageID: Unused67 (67)
SubType: SubMessageID
  - ClientAddress = 1
  - ChangeSever = 2
  - ChangeCustomizedServer = 3
  - OnlineInfoRequest = 4
  - OnlineInfoResponse = 5
Content: string
Port: ushort (可选，取决于 SubType)
```

## 处理器说明

| 处理器 | 功能 |
|--------|------|
| CommandHandler | 处理 /server 命令 |
| ConnectionHandler | 处理连接初始化、玩家同步 |
| CustomPacketHandler | 处理 DimensionUpdate 数据包 |
| NpcHandler | 追踪 NPC 状态，切换时清理 |
| ProjectileHandler | 追踪投射物状态，切换时清理 |
| PlayerHandler | 追踪其他玩家状态，切换时清理 |
| ItemHandler | 追踪物品状态，切换时清理 |
| PylonHandler | 追踪传送塔状态，切换时清理 |
| SSCHandler | 处理服务器端角色数据 |

## 依赖

- .NET 10.0
- TrProtocol (自定义协议库)
- Newtonsoft.Json
- XNA Framework (Terraria 依赖)

## 相关文档

- [架构文档](docs/ARCHITECTURE.md)
- [配置文档](docs/CONFIG.md)
