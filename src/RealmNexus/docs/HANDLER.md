# Handler 开发指南

## 概述

Handler 是 RealmNexus 的核心扩展机制，用于拦截和处理特定的数据包。

## 创建 Handler

### 1. 继承 PacketHandlerBase<T>

```csharp
using RealmNexus.Core;
using TrProtocol.NetPackets;

public class MyHandler(RealmClient client, ILogger logger) 
    : PacketHandlerBase<SyncPlayer>(client, logger)
{
    protected override void HandleC2S(SyncPlayer packet, PacketInterceptArgs args)
    {
        // 处理 C2S 包
    }

    protected override void HandleS2C(SyncPlayer packet, PacketInterceptArgs args)
    {
        // 处理 S2C 包
    }
}
```

### 2. 继承 PacketHandlerBase (多包处理)

```csharp
public class MultiHandler(RealmClient client, ILogger logger) 
    : PacketHandlerBase(client, logger)
{
    public override void OnC2S(PacketInterceptArgs args)
    {
        if (args.Packet is SyncPlayer player)
        {
            // 处理 SyncPlayer
        }
        else if (args.Packet is ClientHello hello)
        {
            // 处理 ClientHello
        }
    }
}
```

### 3. 处理自定义数据包

```csharp
public class CustomHandler(RealmClient client, ILogger logger) 
    : CustomPacketHandlerBase<DimensionUpdate>(client, logger)
{
    protected override void HandleC2S(DimensionUpdate packet, PacketInterceptArgs args)
    {
        // 处理自定义包
    }
}
```

## 拦截包

```csharp
protected override void HandleC2S(SyncPlayer packet, PacketInterceptArgs args)
{
    // 阻止包继续转发
    args.Handled = true;
    
    // 修改包内容
    packet.Name = "NewName";
    
    // 发送新包
    _ = Client.SendPacketToServerAsync(new OtherPacket());
}
```

## 生命周期事件

```csharp
public class LifecycleHandler(RealmClient client, ILogger logger) 
    : PacketHandlerBase(client, logger)
{
    public override void OnConnected()
    {
        // 连接建立时
        Logger.LogInfo("LifecycleHandler", "客户端已连接");
    }

    public override void OnDisconnected()
    {
        // 连接断开时
        Logger.LogInfo("LifecycleHandler", "客户端已断开");
    }

    public override void OnServerChanging()
    {
        // 切换服务器前（清理状态）
        _savedData.Clear();
    }

    public override void OnServerChanged()
    {
        // 切换服务器后（恢复状态）
        _ = Client.SendPacketToServerAsync(_savedHello);
    }
}
```

## Handler 依赖注入

```csharp
public class DependentHandler(
    RealmClient client, 
    ILogger logger,
    SyncPlayerHandler syncPlayerHandler)  // 依赖其他 Handler
    : PacketHandlerBase(client, logger)
{
    public override void OnC2S(PacketInterceptArgs args)
    {
        var playerName = syncPlayerHandler.PlayerName;
        // ...
    }
}
```

依赖会自动解析，只需在构造函数中声明即可。

## 最佳实践

1. **单一职责**: 每个 Handler 只处理一种类型的包
2. **状态管理**: 使用生命周期事件管理状态
3. **异常处理**: 避免在 Handler 中抛出异常
4. **异步操作**: 使用 `_ =` 丢弃 Task 避免阻塞

## 内置 Handler

| Handler | 功能 |
|---------|------|
| `ConnectionHandler` | 连接状态管理 |
| `SyncPlayerHandler` | 玩家信息同步 |
| `ClientHelloHandler` | 客户端握手 |
| `SSCHandler` | 服务器角色模式处理 |
| `ItemHandler` | 物品状态跟踪 |
| `NpcHandler` | NPC 状态跟踪 |
| `ProjectileHandler` | 弹幕状态跟踪 |
| `ChatCommandHandler` | 聊天命令处理 |
| `DimensionUpdatePacketHandler` | 自定义数据包处理 |
