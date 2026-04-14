# RealmNexus

一个基于 .NET 10 的高性能 Terraria 游戏代理服务器，支持多服务器切换、自定义数据包处理和插件化架构。

## 特性

- **高性能网络处理**: 使用 `System.IO.Pipelines` 和 `Channel` 实现异步 I/O
- **服务器切换**: 支持玩家在不同 Terraria 服务器间无缝切换
- **自定义数据包**: 可扩展的自定义数据包系统
- **Handler 插件架构**: 基于接口的模块化设计，支持自动注册和依赖注入
- **完整的生命周期**: 连接、断开、服务器切换等事件支持
- **日志等级控制**: 可配置的日志输出级别

## 快速开始

### 配置

编辑 `config.json`:

```json
{
  "port": 7654,
  "log_level": "info",
  "servers": [
    {
      "name": "main",
      "host": "127.0.0.1",
      "port": 7777
    },
    {
      "name": "pvp",
      "host": "127.0.0.1",
      "port": 7778
    }
  ]
}
```

### 运行

```bash
dotnet run
```

## 项目结构

```
RealmNexus/
├── Core/
│   ├── Handlers/          # 数据包处理器
│   ├── PacketPipe.cs      # 网络管道
│   ├── RealmClient.cs     # 客户端连接管理
│   └── PacketHandlerManager.cs  # Handler 管理
├── Logging/               # 日志系统
├── Models/                # 数据模型
├── Packets/               # 自定义数据包
└── ProxyServer.cs         # 主服务器
```

## 创建自定义 Handler

```csharp
public class MyHandler(RealmClient client, ILogger logger) 
    : PacketHandlerBase<SyncPlayer>(client, logger)
{
    protected override void HandleC2S(SyncPlayer packet, PacketInterceptArgs args)
    {
        // 处理客户端到服务器的包
        Logger.LogInfo("MyHandler", $"玩家: {packet.Name}");
    }

    protected override void HandleS2C(SyncPlayer packet, PacketInterceptArgs args)
    {
        // 处理服务器到客户端的包
    }

    public override void OnServerChanging()
    {
        // 服务器切换前清理
    }
}
```

## 自定义数据包

```csharp
public struct MyPacket : ICustomPacket
{
    public MessageID Type => MessageID.Unused68;
    public int Data { get; set; }

    public unsafe void ReadContent(ref void* ptr, void* end_ptr)
    {
        Data = Unsafe.ReadUnaligned<int>(ptr);
        ptr = (byte*)ptr + sizeof(int);
    }

    public unsafe void WriteContent(ref void* ptr)
    {
        Unsafe.WriteUnaligned(ptr, Data);
        ptr = (byte*)ptr + sizeof(int);
    }
}
```

## 许可证

MIT License
