# 架构设计文档

## 整体架构

```
┌─────────────────────────────────────────────────────────────┐
│                        ProxyServer                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │   Listener  │  │ Config      │  │   PacketHandler     │  │
│  │   (Port)    │  │             │  │   Manager           │  │
│  └──────┬──────┘  └─────────────┘  └─────────────────────┘  │
└─────────┼───────────────────────────────────────────────────┘
          │ Accept
          ▼
┌─────────────────────────────────────────────────────────────┐
│                        RealmClient                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │  C2S Pipe   │  │  S2C Pipe   │  │  PacketHandler      │  │
│  │ (Client→Server)│ │(Server→Client)│  │  Manager            │  │
│  └──────┬──────┘  └──────┬──────┘  └─────────────────────┘  │
└─────────┼────────────────┼───────────────────────────────────┘
          │                │
          ▼                ▼
┌─────────────────┐  ┌─────────────────┐
│   Terraria      │  │   Terraria      │
│   Client        │  │   Server        │
└─────────────────┘  └─────────────────┘
```

## 核心组件

### 1. ProxyServer

- **职责**: 监听客户端连接，管理服务器配置
- **关键方法**:
  - `StartAsync()`: 启动监听
  - `StopAsync()`: 停止服务器

### 2. RealmClient

- **职责**: 管理单个客户端连接，处理服务器切换
- **关键方法**:
  - `RunAsync()`: 运行客户端连接
  - `ChangeServerAsync()`: 切换服务器
  - `SendPacketToServerAsync()`: 发送包到服务器
  - `SendPacketToClientAsync()`: 发送包到客户端

### 3. PacketPipe

- **职责**: 处理网络 I/O，解析和转发数据包
- **特性**:
  - 使用 `System.IO.Pipelines` 高性能读取
  - 支持自定义数据包解析
  - 避免装箱操作

### 4. PacketHandlerManager

- **职责**: 管理所有 Handler，处理包拦截
- **特性**:
  - 自动反射注册 Handler
  - 拓扑排序解决依赖
  - 对象池复用 `PacketInterceptArgs`

### 5. IPacketHandler

- **职责**: 处理特定类型的数据包
- **生命周期**:
  - `OnConnected()`: 连接建立
  - `OnDisconnected()`: 连接断开
  - `OnServerChanging()`: 开始切换服务器
  - `OnServerChanged()`: 服务器切换完成

## 数据流

### 客户端到服务器 (C2S)

```
Terraria Client → PacketPipe (C2S) → Handler.OnC2S → HandlerManager → Server
```

### 服务器到客户端 (S2C)

```
Terraria Server → PacketPipe (S2C) → Handler.OnS2C → HandlerManager → Client
```

## 自定义数据包流程

```
Raw Data → PacketPipe.DeserializePacket → CustomPacketRegistry → ICustomPacket → Handler
```

## 服务器切换流程

```
1. ChangeServerAsync() 调用
2. OnServerChanging() 触发 (清理状态)
3. 断开旧服务器连接
4. 连接新服务器
5. OnServerChanged() 触发 (恢复状态)
6. 重新发送 ClientHello 等初始化包
```

## 性能优化

1. **ArrayPool**: 缓冲区复用
2. **ObjectPool**: `PacketInterceptArgs` 复用
3. **ReaderWriterLockSlim**: 并发控制
4. **Channel**: 异步消息队列
5. **避免装箱**: 泛型处理替代 object
