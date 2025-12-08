# 花艺电商项目 - Gitee部署与使用指南

## 1. 项目概述

这是一个基于 ASP.NET Core 9.0 和 Vue 3 + Vite 的花艺电商项目，包含完整的前后端功能，支持商品管理、购物车、订单管理、用户认证等功能。

## 2. 技术栈

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

## 3. 部署到Gitee详细步骤

### 3.1 环境准备

#### 3.1.1 安装Git

- **Windows**：下载并安装 [Git for Windows](https://gitforwindows.org/)
- **Mac**：使用 Homebrew 安装 `brew install git`
- **Linux**：使用包管理器安装，如 Ubuntu: `sudo apt install git`

#### 3.1.2 安装Node.js（前端开发）

- 下载并安装 [Node.js](https://nodejs.org/)
- 版本要求：Node.js 16+，npm 8+

#### 3.1.3 安装.NET SDK（后端开发）

- 下载并安装 [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)

### 3.2 本地仓库初始化

#### 3.2.1 创建项目目录

```bash
# 创建项目目录
mkdir flowershop
cd flowershop
```

#### 3.2.2 初始化Git仓库

```bash
git init
```

#### 3.2.3 创建.gitignore文件

```bash
# 创建.gitignore文件
cat > .gitignore << 'EOF'
# ASP.NET Core
**/bin/
**/obj/
*.user
*.suo
*.userosscache
*.sln.docstates
.vs/

# Entity Framework Core
*.db
*.sqlite
**/Migrations/*
!**/Migrations/*.cs

# Logs
logs
*.log
npm-debug.log*
yarn-debug.log*
yarn-error.log*
pnpm-debug.log*
lerna-debug.log*

# Node.js dependencies
node_modules/
*.node
*.pem

# Vue/Vite
.DS_Store
*.local
/dist/
*.local
vite.config.js.timestamp-*

# Element Plus
.element-plus/

# Environment variables
.env
.env.local
.env.*.local

# Editor directories and files
.vscode/*
!.vscode/extensions.json
.idea
*.suo
*.ntvs*
*.njsproj
*.sln
*.sw?

# Database
*.db
*.mdf
*.ldf

# Temp files
*.tmp
*.temp

# OS generated files
Thumbs.db
.DS_Store
.DS_Store?
._*
.Spotlight-V100
.Trashes
ehthumbs.db

# Uploads
**/uploads/

# Build outputs
*.dll
*.exe
*.pdb
*.deps.json
*.runtimeconfig.json
*.runtimeconfig.dev.json

# Documentation
*.md
!README.md
!README.*.md
EOF
```

### 3.3 代码提交

#### 3.3.1 添加项目文件

将前后端代码复制到项目目录中，然后执行以下命令：

```bash
# 添加所有文件到暂存区
git add .

# 查看添加状态
git status
```

#### 3.3.2 创建初始提交

```bash
git commit -m "Initial commit"
```

### 3.4 Gitee仓库创建与关联

#### 3.4.1 在Gitee上创建仓库

1. 登录 [Gitee](https://gitee.com/)
2. 点击右上角的「+」按钮，选择「新建仓库」
3. 填写仓库信息：
   - 仓库名称：flowershop
   - 仓库描述：花艺电商项目
   - 选择公开或私有
   - 点击「创建仓库」

#### 3.4.2 关联本地仓库与Gitee仓库

```bash
# 添加远程仓库
git remote add origin https://gitee.com/L-I-Y-U/flowershop.git

# 查看远程仓库
git remote -v
```

### 3.5 代码推送

#### 3.5.1 HTTPS方式推送（需要输入密码或访问令牌）

```bash
git push -u origin main
```

#### 3.5.2 SSH方式推送（推荐，更便捷安全）

##### 3.5.2.1 生成SSH密钥对

```bash
# 生成SSH密钥对
ssh-keygen -t ed25519 -C "your-email@example.com" -f "~/.ssh/id_ed25519" -N ""
```

##### 3.5.2.2 查看SSH公钥

```bash
# Windows
Get-Content -Path "~/.ssh/id_ed25519.pub"

# Mac/Linux
cat ~/.ssh/id_ed25519.pub
```

##### 3.5.2.3 将SSH公钥添加到Gitee

1. 登录Gitee
2. 点击右上角头像 → 「设置」→ 「安全设置」→ 「SSH 公钥」
3. 点击「添加公钥」
4. 粘贴生成的公钥内容
5. 设置公钥名称，点击「确定」保存

##### 3.5.2.4 使用SSH方式推送代码

```bash
# 修改远程仓库为SSH协议
git remote set-url origin git@gitee.com:L-I-Y-U/flowershop.git

# 推送代码
git push -u origin main
```

## 4. 访问Gitee代码

### 4.1 HTTPS方式克隆

```bash
git clone https://gitee.com/L-I-Y-U/flowershop.git
```

### 4.2 SSH方式克隆

```bash
git clone git@gitee.com:L-I-Y-U/flowershop.git
```

### 4.3 在线浏览

直接访问 [Gitee仓库地址](https://gitee.com/L-I-Y-U/flowershop) 即可在线浏览代码。

## 5. 修改代码后上传到Gitee

### 5.1 基本步骤（手动上传）

#### 5.1.1 查看修改状态

```bash
git status
```

#### 5.1.2 添加修改到暂存区

```bash
# 添加所有修改
git add .

# 或添加指定文件
git add 文件名
```

#### 5.1.3 提交修改

```bash
git commit -m "提交说明"  # 提交说明要清晰描述修改内容
```

#### 5.1.4 推送到Gitee

```bash
git push origin main
```

### 5.2 进阶：自动化上传方法

#### 5.2.1 使用Git客户端自动推送

- **GitHub Desktop**：可视化界面，提交后可直接点击"Push origin"按钮
- **SourceTree**：支持多种Git操作，包括自动推送
- **VS Code内置Git**：提交后右下角会提示推送，点击即可

#### 5.2.2 配置Git钩子（自动推送）

**创建post-commit钩子**：

```bash
# 进入.git/hooks目录
cd .git/hooks

# 创建post-commit文件
cat > post-commit << 'EOF'
#!/bin/bash
echo "自动推送代码到Gitee..."
git push origin main
EOF

# 赋予执行权限
chmod +x post-commit
```

#### 5.2.3 使用Gitee Actions（CI/CD自动化）

**创建Gitee Actions配置文件**：

在项目根目录创建 `.github/workflows/main.yml`（Gitee兼容GitHub Actions格式）

```yaml
name: Auto Push to Gitee

on:
  push:
    branches: [ main ]

jobs:
  push-to-gitee:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
      with:
        fetch-depth: 0
    
    - name: Push to Gitee
      uses: ad-m/github-push-action@master
      with:
        repository: git@gitee.com:L-I-Y-U/flowershop.git
        ssh-key: ${{ secrets.GITEE_SSH_KEY }}
        branch: main
```

**配置步骤**：

1. 生成新的SSH密钥对
2. 将私钥添加到Gitee仓库的"Settings" → "Secrets and variables" → "Actions"中，命名为`GITEE_SSH_KEY`
3. 将公钥添加到Gitee账户

## 6. 从Gitee更新本地代码

当远程仓库有新的提交时，使用以下命令更新本地代码：

```bash
git pull origin main
```

## 7. 常用Git命令速查

| 命令 | 功能 |
|------|------|
| `git init` | 初始化Git仓库 |
| `git status` | 查看仓库状态 |
| `git add .` | 添加所有修改到暂存区 |
| `git commit -m "message"` | 提交修改 |
| `git push origin main` | 推送到远程仓库 |
| `git pull origin main` | 从远程仓库拉取更新 |
| `git remote add origin 仓库地址` | 添加远程仓库 |
| `git remote -v` | 查看远程仓库 |
| `git log` | 查看提交历史 |
| `git branch` | 查看本地分支 |
| `git checkout -b 分支名` | 创建并切换到新分支 |
| `git checkout 分支名` | 切换到指定分支 |
| `git merge 分支名` | 合并分支 |
| `git clone 仓库地址` | 克隆仓库 |

## 8. 最佳实践

### 8.1 代码提交规范

- **提交信息清晰**：使用简洁明了的语言描述修改内容
- **每次提交一个功能点**：避免一次提交多个不相关的功能
- **使用英文或中文**：保持一致性，推荐使用中文描述
- **使用动词开头**：如「修复」、「添加」、「更新」等

### 8.2 分支管理

- **main分支**：稳定版本，用于部署生产环境
- **dev分支**：开发分支，用于集成各个功能分支
- **feature分支**：功能分支，用于开发新功能
- **bugfix分支**：修复分支，用于修复bug

### 8.3 协作开发

- **定期拉取更新**：避免本地代码与远程仓库差距过大
- **解决冲突**：推送前先拉取，解决冲突后再推送
- **代码审查**：使用Gitee Pull Request功能进行代码审查
- **issue管理**：使用Gitee Issue功能管理任务和bug

### 8.4 安全注意事项

- **保护敏感信息**：不要将密码、API密钥等敏感信息提交到代码仓库
- **使用环境变量**：将敏感配置放在环境变量中，通过配置文件引用
- **定期更新依赖**：及时更新依赖包，修复安全漏洞

## 9. 常见问题与解决方案

### 9.1 推送失败：认证失败

**问题**：`remote: Incorrect username or password (access token)`

**解决方案**：

1. 检查用户名和密码是否正确
2. 使用访问令牌替代密码
3. 检查SSH密钥是否正确添加到Gitee

### 9.2 推送失败：主机验证失败

**问题**：`Host key verification failed`

**解决方案**：

```bash
# 将Gitee主机添加到known_hosts
ssh-keyscan -t ed25519 gitee.com >> ~/.ssh/known_hosts
```

### 9.3 克隆失败：仓库不存在

**问题**：`fatal: repository 'https://gitee.com/username/repo.git/' not found`

**解决方案**：

1. 检查仓库URL是否正确
2. 检查仓库是否存在
3. 检查是否有访问权限

### 9.4 提交失败：工作区有未提交的修改

**问题**：`error: cannot pull with rebase: Your index contains uncommitted changes.`

**解决方案**：

```bash
# 暂存当前修改
git stash

# 拉取更新
git pull origin main

# 恢复暂存的修改
git stash pop
```

## 10. 总结

本指南详细介绍了花艺电商项目从部署到Gitee的完整流程，包括：

- 项目概述和技术栈
- 部署到Gitee的详细步骤
- 访问Gitee代码的方法
- 修改后上传到Gitee的步骤
- 自动化上传方法
- 常用Git命令
- 最佳实践和注意事项
- 常见问题与解决方案

通过本指南，你可以轻松完成项目的Gitee部署和代码管理，提高开发效率和协作质量。

---

**更新时间**：2025-12-08
**版本**：v1.0
**作者**：花艺电商项目团队