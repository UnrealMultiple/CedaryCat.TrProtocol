# TrProtocol

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](../LICENSE)

TrProtocol 是一个用于 Terraria 游戏的协议库和工具集，包含数据包序列化、代理服务器和测试工具。

## 项目结构

```
src/
├── TrProtocol/                      # 核心协议库
├── TrProtocol.Shared/               # 共享库
├── TrProtocol.SerializerGenerator/  # 源代码生成器
├── Dimensions/                      # Terraria 代理服务器
└── TrProtocol.TestAgent/            # 测试代理工具
```

## 模块说明

### TrProtocol (核心协议库)

Terraria 协议的核心实现：
- 完整的数据包定义
- 高效的序列化/反序列化
- 支持 Terraria 1.4.4.9 (Protocol 319)

### Dimensions (代理服务器)

基于 TrProtocol 的 Terraria 代理服务器：
- 多服务器切换
- SSC 支持
- 实体同步管理
- /server 命令

详细文档：[Dimensions/README.md](Dimensions/README.md)

## 快速开始

### 构建项目

```bash
cd src
dotnet build TrProtocol.slnx
```

### 运行 Dimensions

```bash
cd src/Dimensions
dotnet run
```

## 技术特性

- **高性能**: 使用不安全代码和源代码生成器
- **类型安全**: 完整的强类型数据包定义
- **可扩展**: 插件式处理器架构

## 许可证

[GNU General Public License v3.0](../LICENSE)
