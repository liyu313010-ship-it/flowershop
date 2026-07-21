# 生产部署清单

1. 复制 `.env.production.example` 为 `.env.production`，生成随机密码和 JWT/Webhook 密钥。
2. 配置真实域名、DNS 和 HTTPS 证书。生产环境不能使用示例密钥。
3. 先备份现有数据库，再执行审批后的 EF migration：
   `dotnet ef database update --project backend/HuanyuFlowerShop/HuanyuFlowerShop.csproj`
4. 启动服务：`docker compose --env-file .env.production -f docker-compose.production.yml up -d --build`
5. 验证：访问 `/health/live`、`/health/ready`，再执行注册、登录、下单、货到付款、后台发货和确认收货流程。
6. 每日运行 `scripts/backup-mysql.ps1`，并定期做恢复演练。
7. `Payment__Mode=disabled` 时只开放货到付款；接入真实支付前必须配置商户密钥、回调域名、Webhook 签名密钥和退款对账流程。
8. 上传文件、聊天附件和缓存位置见 `docs/STORAGE_AND_CACHE.md`。生产附件必须保存在 `/var/lib/flowershop/uploads`，不要写入版本发布目录。
