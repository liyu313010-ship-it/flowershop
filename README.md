# 花艺电商项目

## 项目概述

这是一个基于 ASP.NET Core 9.0 和 Vue 3 + Vite 的花艺电商项目，包含完整的前后端功能。

## 技术栈

### 后端
- ASP.NET Core 9.0
- MySQL 数据库
- Entity Framework Core
- JWT 认证与授权
- RESTful API 设计

### 前端
- Vue 3
- Vite
- Pinia 状态管理
- Vue Router
- Axios
- Element Plus UI 组件库
- Tailwind CSS

## 项目结构

```
├── backend/          # 后端代码
│   └── HuanyuFlowerShop/  # ASP.NET Core 项目
├── frontend/         # 前端代码
│   ├── public/       # 静态资源
│   └── src/          # 源代码
├── .gitignore        # Git 忽略文件
└── README.md         # 项目说明文档
```

## 快速开始

### 后端启动

```bash
# 进入后端目录
cd backend/HuanyuFlowerShop

# 恢复依赖并启动 API
dotnet restore
dotnet run
```

### 前端启动

```bash
# 进入前端目录
cd frontend

# 安装依赖
npm install

# 启动开发服务器
npm run dev
```

## 数据库配置

1. 确保 MySQL 服务已启动
2. 创建数据库 `huanyu_flowershop`
3. 修改 `backend/HuanyuFlowerShop/appsettings.json` 中的数据库连接字符串
4. 运行数据库迁移

```bash
# 进入后端目录
cd backend/HuanyuFlowerShop

# 首次使用时恢复仓库工具
dotnet tool restore

# 运行数据库迁移
dotnet ef database update
```

## API 文档

API 文档可通过 Swagger 访问：
- 开发环境：`http://localhost:5002/swagger`
- 存活检查：`/health/live`
- 就绪检查（包含数据库）：`/health/ready`

## 部署说明

### 后端部署

```bash
# 进入后端目录
cd backend/HuanyuFlowerShop

# 构建发布版本
dotnet publish -c Release -o ./publish

# 运行发布版本
dotnet ./publish/HuanyuFlowerShop.dll
```

### 前端部署

```bash
# 进入前端目录
cd frontend

# 构建生产版本
npm run build

# 将 dist 目录部署到 Web 服务器
```

## 授权说明

本项目采用 JWT 认证机制，用户登录后会获取到一个 JWT token，后续请求需要在 Authorization 头中携带该 token。

## 生产环境说明

生产环境必须通过密钥管理服务注入数据库连接、JWT 密钥、CORS 来源和支付回调密钥，禁止把真实密钥写入仓库。在线支付在配置 `Payment:Mode` 和对应商户参数前保持关闭；没有商户号、密钥和 HTTPS 回调地址时请使用货到付款。部署脚本会在迁移前创建数据库备份，并在应用健康检查失败时回滚应用文件。

## 许可证

MIT License
