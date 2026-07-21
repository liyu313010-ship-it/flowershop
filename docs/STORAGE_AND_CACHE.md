# 文件存储与缓存说明

## 生产数据放在哪里

| 内容 | 生产位置 | 是否持久化 | 说明 |
| --- | --- | --- | --- |
| 用户、订单、商品、聊天消息和附件元数据 | MariaDB `huanyuflowershop` | 是 | 数据库默认位于服务器 `/var/lib/mysql` |
| 聊天图片与文件 | `/var/lib/flowershop/uploads/chat-attachments/YYYY/MM/` | 是 | 应用目录中的 `uploads` 是指向该目录的符号链接 |
| 用户头像 | `/var/lib/flowershop/uploads/avatars/` | 是 | 通过 `/uploads/avatars/` 公开读取 |
| 商品上传图片 | `/var/lib/flowershop/uploads/products/` | 是 | 通过 `/uploads/products/` 公开读取 |
| 前端构建文件 | `/var/www/flowershop/` | 随发布替换 | 哈希资源会保留旧版本，避免已打开页面失效 |
| 生产备份 | `/var/backups/flowershop/<时间>/` | 是 | 每次发布前备份数据库和 uploads，保留最近 7 份 |

聊天附件不是“缓存”，而是客服业务记录。数据库只保存原文件名、内部存储名、MIME 类型、大小和消息归属，文件本体保存在持久化目录中，不写入数据库，也不通过 SignalR 传输。

## 哪些内容属于缓存

- `IMemoryCache` 和请求限流状态保存在后端进程内存中，服务重启后自动清空。
- 前端登录令牌和少量用户状态保存在浏览器 `localStorage`，聊天文件不会写入 `localStorage`。
- 图片预览通过浏览器内存中的 Blob URL 展示，聊天组件关闭时立即释放。
- 附件接口返回 `Cache-Control: private, no-store`，浏览器和 Nginx 都不会持久缓存聊天附件。
- `node_modules`、Vite `dist`、.NET `bin/obj` 是开发或构建产物，不是生产业务数据；GitHub Actions runner 执行结束后会销毁临时构建环境。

## 附件安全规则

- 单文件默认最大 10MB，可通过 `Storage:ChatAttachments:MaxFileSizeBytes` 调整，最高 19MB，为 Nginx 的 20MB 请求限制预留 multipart 开销。
- 支持 JPG、PNG、GIF、WebP、PDF、TXT、Word 和 Excel。
- 后端同时校验扩展名和文件头，不信任浏览器上传的 MIME 类型。
- 附件目录不会映射为公共静态目录，只能通过 `/api/chat/messages/{id}/attachment` 读取。
- 下载接口验证 JWT、会话归属和管理员角色，普通用户不能读取其他用户的附件。
- 服务端使用随机内部文件名，原文件名只用于界面展示和下载。

## 容量与维护

有效聊天附件默认作为客服记录保留，不会被当作临时缓存自动删除。建议定期检查：

```bash
du -sh /var/lib/flowershop/uploads
find /var/lib/flowershop/uploads/chat-attachments -type f | wc -l
du -sh /var/backups/flowershop
```

若要设置自动过期，必须先确定售后、退款、纠纷举证和隐私合规所需的保留期限，再增加按数据库消息记录清理文件的定时任务；不要直接用 `find -delete`，否则会留下指向不存在文件的聊天消息。
