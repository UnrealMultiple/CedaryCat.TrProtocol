# 配置文档

## 配置文件

配置文件位于 `config.json`。

## 配置项

### port

代理服务器监听端口。

```json
{
  "port": 7654
}
```

### log_level

日志输出等级。

```json
{
  "log_level": "Info"
}
```

| 等级 | 说明 |
|------|------|
| `Debug` | 调试信息（最详细）|
| `Info` | 一般信息 |
| `Warning` | 警告信息 |
| `Error` | 错误信息（最简略）|

### servers

目标 Terraria 服务器列表。

```json
{
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

| 字段 | 说明 |
|------|------|
| `name` | 服务器名称（用于切换命令）|
| `host` | 服务器地址 |
| `port` | 服务器端口 |

## 完整配置示例

```json
{
  "port": 7654,
  "log_level": "Info",
  "servers": [
    {
      "name": "lobby",
      "host": "127.0.0.1",
      "port": 7777
    },
    {
      "name": "survival",
      "host": "127.0.0.1",
      "port": 7778
    },
    {
      "name": "creative",
      "host": "127.0.0.1",
      "port": 7779
    }
  ]
}
```

## 服务器切换

玩家在游戏中使用命令切换服务器：

```
/server              - 显示服务器列表
/server <name>       - 切换到指定服务器
```

## 日志等级说明

日志等级从低到高：
- `Debug` - 显示所有日志
- `Info` - 显示 Info、Warning、Error
- `Warning` - 显示 Warning、Error
- `Error` - 只显示 Error
