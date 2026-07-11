#!/usr/bin/env bash
set -Eeuo pipefail

SOURCE_DIR="/opt/flowershop-source"
APP_DIR="/opt/flowershop-app"
WEB_DIR="/var/www/flowershop"
ENV_FILE="/etc/flowershop.env"
DEPLOY_REF="${DEPLOY_REF:-main}"

echo "[deploy] Updating source from GitHub ref: $DEPLOY_REF..."
cd "$SOURCE_DIR"
git -c safe.directory="$SOURCE_DIR" fetch --prune origin \
  "+${DEPLOY_REF}:refs/remotes/origin/${DEPLOY_REF}"
git -c safe.directory="$SOURCE_DIR" reset --hard "origin/${DEPLOY_REF}"

echo "[deploy] Building Vue frontend..."
cd "$SOURCE_DIR/frontend"
npm ci --legacy-peer-deps
npm run build

echo "[deploy] Publishing ASP.NET backend..."
cd "$SOURCE_DIR/backend/HuanyuFlowerShop"
dotnet publish HuanyuFlowerShop.csproj -c Release -o "$APP_DIR.next"

echo "[deploy] Applying database migrations..."
set -a
source "$ENV_FILE"
set +a
dotnet tool restore
dotnet ef database update --project HuanyuFlowerShop.csproj

echo "[deploy] Replacing application files..."
rm -rf "$APP_DIR.previous"
if [ -d "$APP_DIR" ]; then mv "$APP_DIR" "$APP_DIR.previous"; fi
mv "$APP_DIR.next" "$APP_DIR"
rm -rf "$APP_DIR/uploads"
ln -s /var/lib/flowershop/uploads "$APP_DIR/uploads"
chown -R www-data:www-data /var/lib/flowershop/uploads
install -d "$WEB_DIR"
rsync -a --delete "$SOURCE_DIR/frontend/dist/" "$WEB_DIR/"

echo "[deploy] Restarting services..."
systemctl restart flowershop
nginx -t
systemctl reload nginx

curl --fail --silent --show-error --retry 10 --retry-delay 2 \
  http://127.0.0.1:5002/api/Categories >/dev/null
echo "[deploy] Deployment completed successfully."
