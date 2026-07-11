#!/usr/bin/env bash
set -Eeuo pipefail

SOURCE_DIR="/opt/flowershop-source"
APP_DIR="/opt/flowershop-app"
WEB_DIR="/var/www/flowershop"
ENV_FILE="/etc/flowershop.env"
DEPLOY_REF="${DEPLOY_REF:-main}"

echo "[deploy] Updating source from GitHub ref: $DEPLOY_REF..."
cd "$SOURCE_DIR"
git -c safe.directory="$SOURCE_DIR" fetch origin \
  "+${DEPLOY_REF}:refs/remotes/origin/${DEPLOY_REF}"
git -c safe.directory="$SOURCE_DIR" reset --hard "origin/${DEPLOY_REF}"

echo "[deploy] Building Vue frontend..."
cd "$SOURCE_DIR/frontend"
npm ci --legacy-peer-deps
npm run build

echo "[deploy] Publishing ASP.NET backend..."
cd "$SOURCE_DIR/backend/HuanyuFlowerShop"
rm -rf "$APP_DIR.next"
dotnet publish HuanyuFlowerShop.csproj -c Release -o "$APP_DIR.next"

echo "[deploy] Applying database migrations..."
set -a
source "$ENV_FILE"
set +a
dotnet tool restore

HISTORY_EXISTS="$(mariadb --protocol=socket \
  --user="$FLOWERSHOP_DB_USER" --password="$FLOWERSHOP_DB_PASSWORD" \
  --skip-column-names --batch \
  --execute="SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='${FLOWERSHOP_DB_NAME}' AND table_name='__EFMigrationsHistory';")"

if [ "$HISTORY_EXISTS" = "0" ]; then
  echo "[deploy] Creating a baseline schema for the new database..."
  SCHEMA_FILE="$(mktemp)"
  dotnet ef dbcontext script --project HuanyuFlowerShop.csproj --output "$SCHEMA_FILE"
  mariadb --protocol=socket \
    --user="$FLOWERSHOP_DB_USER" --password="$FLOWERSHOP_DB_PASSWORD" \
    "$FLOWERSHOP_DB_NAME" < "$SCHEMA_FILE"
  rm -f "$SCHEMA_FILE"

  mariadb --protocol=socket \
    --user="$FLOWERSHOP_DB_USER" --password="$FLOWERSHOP_DB_PASSWORD" \
    "$FLOWERSHOP_DB_NAME" <<'SQL'
CREATE TABLE `__EFMigrationsHistory` (
  `MigrationId` varchar(150) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL,
  CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
);
SQL
  while IFS= read -r migration_id; do
    mariadb --protocol=socket \
      --user="$FLOWERSHOP_DB_USER" --password="$FLOWERSHOP_DB_PASSWORD" \
      "$FLOWERSHOP_DB_NAME" \
      --execute="INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('${migration_id}', '9.0.17');"
  done < <(dotnet ef migrations list --project HuanyuFlowerShop.csproj --no-connect | grep -E '^[0-9]{14}_')
fi

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
