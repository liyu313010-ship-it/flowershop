# D 盘主项目业务修复与发布说明

主项目：`D:\flowershop`。修复基线：`85513562`。保留 D 盘现有界面、地址组件、视频/聊天及支付宝处理代码；没有用 E 盘目录整套覆盖。

## 修复后的规则

- 购物车和结算共用 `POST /api/Orders/quote`。默认普通配送满 299 元免运费，否则 15 元；加急配送另加 20 元，具体以服务端配置及报价为准。
- 优惠券预览与下单共用资格、有效期、门槛和总量检查。无效券明确报错，不静默生成原价订单。
- 请求编号和内容摘要持久保存。相同请求重试只生成同一订单；金额变化要求重新报价确认。
- 下单采用事务、按用户及商品加行锁，并条件扣库存。取消在同一事务中返还库存、返券、写入轨迹。
- 货到付款保持 pending/unpaid，不调用管理员改状态接口，也没有在线支付期限。
- 用户确认收货使用专门接口，不能调用任意状态管理接口。所有管理入口和超时任务共用状态规则。
- 发货要求真实填写物流公司和运单号，移除自动编造运单号。交易订单不再直接物理删除。
- 历史商品和地址使用下单快照，修正电话字段及取消失败时的提示。
- 订单、库存变化统一失效相关缓存；前缀缓存删除实际生效。
- 封禁或降权递增 token 版本；鉴权核对当前状态和角色。现有旧 token 可能要求重新登录。
- 用户收货积分只奖励一次；不再自动重试有副作用的写请求。

## 本机私密配置

原 D 盘的配置完整保留在 `backend/HuanyuFlowerShop/appsettings.Local.json`，已加入 Git 忽略，并禁止本机 publish 自动打包。程序按“通用配置 → 本机配置 → 环境变量”的顺序读取。

受版本控制的 `appsettings.json` 不含数据库密码、JWT 私钥或支付宝私钥。部署时由服务器环境文件提供数据库/JWT 配置；支付宝原配置通过 GitHub production 环境的加密 secret 注入发布包，不进入公开源码。

未使用的 AutoMapper 依赖已移除。支付宝 SDK 仍保留原实现，其旧包存在兼容性告警；本次没有声称第三方付款已实测成功。

## 数据库

新增迁移 `20261101093454_DMainOrderBusinessConsistency`，只增加缺少的业务字段和请求编号唯一索引，保留已有表、字段及数据，兼容线上比 D 盘模型更新的表结构。

现有数据库应用增量迁移，部署前由现有安装流程备份数据库和 uploads。全新空库可以执行：

```powershell
dotnet run --project backend\HuanyuFlowerShop -- --initialize-empty-database
```

只有没有任何表的数据库允许初始化；已有业务库会被拒绝。

## 验证

```powershell
$env:NUGET_PACKAGES='E:\Cache\nuget'
$env:DOTNET_CLI_HOME='D:\Cache\flowershop-dotnet'
$env:TEMP='D:\Cache\Temp'
$env:TMP=$env:TEMP
dotnet test HuanyuFlowerShop.sln
```

9 项真实 MySQL/HTTP 检查需要独立测试 MySQL 监听 `127.0.0.1:13316`，并设置 `FLOWERSHOP_TEST_MYSQL`。未设置时这些测试会明确跳过；测试为每个场景创建随机测试数据库，结束后清理，拒绝业务主机/端口。

本次实际验证结果保存在 `D:\Cache\flowershop-repair-20261101`；E 盘副本的测试结果不作为 D 盘验收证据。

发布使用 D 盘源码快照，原 Gitee 远程保持不变。公开发布提交连接到 GitHub 已有公开历史，不推送含本机旧配置的 D 盘历史。部署完成还须检查公网页面、资源和 readiness，构建通过不等于上线成功。
