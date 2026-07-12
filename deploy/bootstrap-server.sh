#!/usr/bin/env bash
set -Eeuo pipefail

PUBLIC_IP="${PUBLIC_IP:-8.148.9.196}"
DEPLOY_PUBLIC_KEY="${DEPLOY_PUBLIC_KEY:-}"
SOURCE_DIR="/opt/flowershop-source"

if [ "$(id -u)" -ne 0 ]; then
  echo "Run this script as root." >&2
  exit 1
fi
if [ -z "$DEPLOY_PUBLIC_KEY" ]; then
  echo "Set DEPLOY_PUBLIC_KEY to the dedicated GitHub Actions SSH public key." >&2
  exit 1
fi

echo "[bootstrap] Installing system packages..."
apt-get update
DEBIAN_FRONTEND=noninteractive apt-get install -y \
  ca-certificates curl git mariadb-server nodejs npm nginx openssl rsync wget
if ! command -v dotnet >/dev/null || ! dotnet --list-sdks | grep -q '^9\.'; then
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  bash /tmp/dotnet-install.sh --channel 9.0 --install-dir /usr/share/dotnet
  ln -sfn /usr/share/dotnet/dotnet /usr/bin/dotnet
fi

echo "[bootstrap] Ensuring 2 GiB swap..."
if [ "$(swapon --show --noheadings | wc -l)" -eq 0 ]; then
  fallocate -l 2G /swapfile
  chmod 600 /swapfile
  mkswap /swapfile
  swapon /swapfile
  grep -q '^/swapfile ' /etc/fstab || echo '/swapfile none swap sw 0 0' >> /etc/fstab
fi

echo "[bootstrap] Creating restricted deployment account..."
id deploy >/dev/null 2>&1 || useradd --create-home --shell /bin/bash deploy
# Keep password authentication impossible while allowing SSH public-key login.
# Ubuntu's useradd creates a locked account (leading `!` in /etc/shadow), and
# sshd rejects even a valid authorized key for such an account.
usermod --password '*' deploy
install -d -o deploy -g deploy -m 700 /home/deploy/.ssh
printf '%s\n' "$DEPLOY_PUBLIC_KEY" > /home/deploy/.ssh/authorized_keys
chown deploy:deploy /home/deploy/.ssh/authorized_keys
chmod 600 /home/deploy/.ssh/authorized_keys
install -o root -g root -m 755 "$SOURCE_DIR/deploy/deploy-flowershop.sh" \
  /usr/local/sbin/deploy-flowershop
printf '%s\n' 'deploy ALL=(root) NOPASSWD: /usr/local/sbin/deploy-flowershop' \
  > /etc/sudoers.d/flowershop-deploy
chmod 440 /etc/sudoers.d/flowershop-deploy
visudo -cf /etc/sudoers.d/flowershop-deploy

echo "[bootstrap] Creating database and secrets..."
systemctl enable --now mariadb
DB_PASS="$(openssl rand -hex 18)"
JWT_KEY="$(openssl rand -hex 32)"
mysql <<SQL
CREATE DATABASE IF NOT EXISTS \`huanyuflowershop\` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER IF NOT EXISTS 'flowershop'@'localhost' IDENTIFIED BY '${DB_PASS}';
ALTER USER 'flowershop'@'localhost' IDENTIFIED BY '${DB_PASS}';
GRANT ALL PRIVILEGES ON \`huanyuflowershop\`.* TO 'flowershop'@'localhost';
FLUSH PRIVILEGES;
SQL
cat > /etc/flowershop.env <<ENV
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://127.0.0.1:5002
ConnectionStrings__DefaultConnection='Server=localhost;Database=huanyuflowershop;User=flowershop;Password=${DB_PASS};AllowUserVariables=True;AllowPublicKeyRetrieval=True;SslMode=Preferred'
JwtSettings__SecretKey='${JWT_KEY}'
SecuritySettings__AllowOrigins__0='http://${PUBLIC_IP}:8080'
FLOWERSHOP_DB_NAME='huanyuflowershop'
FLOWERSHOP_DB_USER='flowershop'
FLOWERSHOP_DB_PASSWORD='${DB_PASS}'
ENV
chmod 600 /etc/flowershop.env

install -d -o www-data -g www-data /opt/flowershop-app /var/www/flowershop \
  /var/lib/flowershop/uploads

cat > /etc/systemd/system/flowershop.service <<'UNIT'
[Unit]
Description=Huanyu Flower Shop API
After=network.target mariadb.service
Requires=mariadb.service

[Service]
WorkingDirectory=/opt/flowershop-app
ExecStart=/usr/bin/dotnet /opt/flowershop-app/HuanyuFlowerShop.dll
EnvironmentFile=/etc/flowershop.env
User=www-data
Group=www-data
Restart=always
RestartSec=5

[Install]
WantedBy=multi-user.target
UNIT
systemctl daemon-reload
systemctl enable flowershop

cat > /etc/nginx/sites-available/flowershop <<'NGINX'
server {
    listen 8080;
    listen [::]:8080;
    server_name _;
    root /var/www/flowershop;
    index index.html;
    client_max_body_size 20m;

    location /api/ {
        proxy_pass http://127.0.0.1:5002;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
    location /uploads/ { proxy_pass http://127.0.0.1:5002; }
    location / { try_files $uri $uri/ /index.html; }
}
NGINX
ln -sfn /etc/nginx/sites-available/flowershop /etc/nginx/sites-enabled/flowershop
nginx -t

echo "[bootstrap] Running the first deployment..."
/usr/local/sbin/deploy-flowershop
echo "Open http://${PUBLIC_IP}:8080 after allowing TCP port 8080 in Alibaba Cloud."
