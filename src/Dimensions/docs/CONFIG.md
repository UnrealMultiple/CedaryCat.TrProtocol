# Dimensions 配置文档

## 配置文件

Dimensions 使用 JSON 格式的配置文件 `config.json`。

## 配置示例

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
    },
    {
      "name": "building",
      "serverIP": "192.168.1.100",
      "serverPort": 7779
    }
  ]
}
```

## 配置项说明

### 根配置

| 配置项 | 类型 | 必填 | 默认值 | 说明 |
|--------|------|------|--------|------|
| `listenPort` | ushort | 否 | 7654 | Dimensions 监听的端口号 |
| `sendDimensionPacket` | bool | 否 | false | 是否发送 DimensionUpdate 数据包 |
| `protocolVersion` | string | 否 | "Terraria319" | 协议版本号 |
| `servers` | array | 是 | - | 服务器列表 |

### 服务器配置

| 配置项 | 类型 | 必填 | 默认值 | 说明 |
|--------|------|------|--------|------|
| `name` | string | 是 | - | 服务器名称（用于 /server 命令） |
| `serverIP` | string | 是 | - | 服务器 IP 地址 |
| `serverPort` | ushort | 是 | - | 服务器端口号 |

## 详细说明

### listenPort

Dimensions 监听的端口号，玩家需要连接到这个端口。

```json
"listenPort": 7654
```

**注意事项**:
- 确保端口未被其他程序占用
- 防火墙需要允许此端口的入站连接
- 如果使用云服务器，需要在安全组中开放此端口

### sendDimensionPacket

控制是否发送 DimensionUpdate 自定义数据包。

```json
"sendDimensionPacket": false
```

**使用场景**:
- `false`: 标准模式，不发送自定义数据包
- `true`: 向服务端发送客户端真实 IP 等信息

**注意**: 服务端需要支持 DimensionUpdate 数据包才能使用此功能。

### protocolVersion

Terraria 协议版本号。

```json
"protocolVersion": "Terraria319"
```

**常见版本**:
- Terraria 1.4.4.9: Terraria319
- 其他版本请参考 Terraria 更新日志

**注意**: 版本号必须与客户端和服务端匹配，否则无法连接。

### servers

服务器列表，定义玩家可以切换的目标服务器。

```json
"servers": [
  {
    "name": "main",
    "serverIP": "127.0.0.1",
    "serverPort": 7777
  }
]
```

#### name

服务器名称，用于 `/server` 命令。

**命名规则**:
- 不能包含空格
- 建议使用小写字母
- 名称唯一

**示例**:
- `main` - 主服务器
- `pvp` - PVP 服务器
- `building` - 建筑服务器

#### serverIP

服务器的 IP 地址。

**支持格式**:
- IPv4: `127.0.0.1`, `192.168.1.100`
- 域名: `terraria.example.com` (需要 DNS 解析)

**注意**:
- 如果是本机服务器，使用 `127.0.0.1`
- 如果是局域网服务器，使用内网 IP
- 如果是公网服务器，使用公网 IP 或域名

#### serverPort

服务器的端口号。

**常见端口**:
- 7777 - Terraria 默认端口
- 7778, 7779 - 其他常用端口

**注意**:
- 确保端口与目标服务器实际监听端口一致
- 不同服务器可以使用不同端口

## 配置场景示例

### 场景 1: 本地测试

所有服务器都在本地运行：

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
      "name": "test",
      "serverIP": "127.0.0.1",
      "serverPort": 7778
    }
  ]
}
```

### 场景 2: 局域网服务器

服务器在局域网内的不同机器上：

```json
{
  "listenPort": 7654,
  "sendDimensionPacket": false,
  "protocolVersion": "Terraria319",
  "servers": [
    {
      "name": "main",
      "serverIP": "192.168.1.10",
      "serverPort": 7777
    },
    {
      "name": "pvp",
      "serverIP": "192.168.1.11",
      "serverPort": 7777
    }
  ]
}
```

### 场景 3: 公网服务器

使用公网服务器：

```json
{
  "listenPort": 7654,
  "sendDimensionPacket": false,
  "protocolVersion": "Terraria319",
  "servers": [
    {
      "name": "main",
      "serverIP": "123.45.67.89",
      "serverPort": 7777
    },
    {
      "name": "asia",
      "serverIP": "asia.terraria.example.com",
      "serverPort": 7777
    }
  ]
}
```

### 场景 4: 混合配置

本地和远程服务器混合：

```json
{
  "listenPort": 7654,
  "sendDimensionPacket": false,
  "protocolVersion": "Terraria319",
  "servers": [
    {
      "name": "local",
      "serverIP": "127.0.0.1",
      "serverPort": 7777
    },
    {
      "name": "remote",
      "serverIP": "123.45.67.89",
      "serverPort": 7777
    }
  ]
}
```

## 故障排除

### 无法连接到 Dimensions

**检查清单**:
1. `listenPort` 是否被占用
2. 防火墙是否允许端口
3. 云服务器安全组是否开放端口

### 无法连接到目标服务器

**检查清单**:
1. `serverIP` 和 `serverPort` 是否正确
2. 目标服务器是否在线
3. 网络是否连通（尝试 ping）

### 协议版本不匹配

**错误表现**: 客户端显示版本不匹配

**解决方案**:
1. 检查 `protocolVersion` 是否与客户端一致
2. 更新 Dimensions 到支持新版本

### 服务器切换失败

**检查清单**:
1. 服务器名称是否正确
2. 目标服务器是否在线
3. 检查 Dimensions 日志获取详细错误

## 高级配置

### 多实例配置

可以在同一台机器上运行多个 Dimensions 实例：

**实例 1** (`config1.json`):
```json
{
  "listenPort": 7654,
  "servers": [...]
}
```

**实例 2** (`config2.json`):
```json
{
  "listenPort": 7655,
  "servers": [...]
}
```

启动时指定配置文件：
```bash
dotnet run --config=config2.json
```

### 动态配置重载

（未来功能）支持在不重启的情况下重载配置。

## 配置验证

启动时会自动验证配置：

```
[Config] 协议版本号: Terraria319
[Config] 侦听端口: 7654
[Config] 远程服务器: main, pvp, building
```

如果配置有误，会显示错误信息：

```
[Config] 错误: 未找到配置文件 config.json
```

## 相关文档

- [README.md](../README.md) - 项目概览
- [ARCHITECTURE.md](ARCHITECTURE.md) - 架构文档
