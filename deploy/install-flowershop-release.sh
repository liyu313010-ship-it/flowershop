#!/usr/bin/env bash
set -Eeuo pipefail

ARCHIVE="/home/deploy/flowershop-release.tgz"
APP_DIR="/opt/flowershop-app"
WEB_DIR="/var/www/flowershop"
UPLOAD_DIR="/var/lib/flowershop/uploads"
ENV_FILE="/etc/flowershop.env"
LOCK_FILE="/run/lock/flowershop-deploy.lock"

exec 9>"$LOCK_FILE"
if ! flock -n 9; then
  echo "Another flower shop deployment is already running." >&2
  exit 1
fi

if [ ! -s "$ARCHIVE" ]; then
  echo "Release archive is missing: $ARCHIVE" >&2
  exit 1
fi

if tar -tzf "$ARCHIVE" | grep -Eq '(^/|(^|/)\.\.(/|$))'; then
  echo "Release archive contains an unsafe path." >&2
  exit 1
fi

STAGING="$(mktemp -d /opt/flowershop-release.XXXXXX)"
cleanup() { rm -rf "$STAGING"; }
trap cleanup EXIT
tar -xzf "$ARCHIVE" --no-same-owner -C "$STAGING"

test -f "$STAGING/backend/HuanyuFlowerShop.dll"
test -f "$STAGING/frontend/index.html"
source "$ENV_FILE"

# 每次发布、尤其是执行迁移前都备份数据库和上传文件，保留最近 7 份。
BACKUP_DIR="/var/backups/flowershop/$(date +%Y%m%d-%H%M%S)"
install -d -m 700 "$BACKUP_DIR"
echo "Backing up the current production database and uploads to $BACKUP_DIR..."
mariadb-dump --protocol=socket \
  --user="$FLOWERSHOP_DB_USER" --password="$FLOWERSHOP_DB_PASSWORD" \
  --single-transaction --routines --triggers \
  "$FLOWERSHOP_DB_NAME" > "$BACKUP_DIR/database.sql"
install -d "$BACKUP_DIR/uploads"
rsync -a "$UPLOAD_DIR/" "$BACKUP_DIR/uploads/"
find /var/backups/flowershop -mindepth 1 -maxdepth 1 -type d -printf '%T@ %p\n' \
  | sort -nr | tail -n +8 | cut -d' ' -f2- | xargs -r rm -rf

APP_NEXT="${APP_DIR}.next"
WEB_NEXT="${WEB_DIR}.next"
APP_PREVIOUS="${APP_DIR}.previous"
WEB_PREVIOUS="${WEB_DIR}.previous"
rm -rf "$APP_NEXT" "$WEB_NEXT"
install -d "$APP_NEXT" "$WEB_NEXT" "$UPLOAD_DIR"
cp -a "$STAGING/backend/." "$APP_NEXT/"
cp -a "$STAGING/frontend/." "$WEB_NEXT/"
rm -rf "$APP_NEXT/uploads"
ln -s "$UPLOAD_DIR" "$APP_NEXT/uploads"
chown -R www-data:www-data "$UPLOAD_DIR"

echo "Applying pending database migration scripts..."
shopt -s nullglob
for migration_file in "$STAGING"/migrations/*.sql; do
  migration_id="$(basename "$migration_file" .sql)"
  applied="$(mariadb --protocol=socket \
    --user="$FLOWERSHOP_DB_USER" --password="$FLOWERSHOP_DB_PASSWORD" \
    "$FLOWERSHOP_DB_NAME" --skip-column-names --batch \
    --execute="SELECT COUNT(*) FROM __EFMigrationsHistory WHERE MigrationId='${migration_id}';")"
  if [ "$applied" = "0" ]; then
    echo "Applying migration: $migration_id"
    mariadb --protocol=socket \
      --user="$FLOWERSHOP_DB_USER" --password="$FLOWERSHOP_DB_PASSWORD" \
      "$FLOWERSHOP_DB_NAME" < "$migration_file"
  fi
done

if [ -d "$STAGING/uploads" ]; then
  echo "Restoring legacy uploaded media..."
  rsync -a "$STAGING/uploads/" "$UPLOAD_DIR/"
  chown -R www-data:www-data "$UPLOAD_DIR"
fi

rm -rf "$APP_PREVIOUS" "$WEB_PREVIOUS"
systemctl stop flowershop
if [ -d "$APP_DIR" ]; then mv "$APP_DIR" "$APP_PREVIOUS"; fi
if [ -d "$WEB_DIR" ]; then mv "$WEB_DIR" "$WEB_PREVIOUS"; fi
mv "$APP_NEXT" "$APP_DIR"
mv "$WEB_NEXT" "$WEB_DIR"

systemctl start flowershop
nginx -t
systemctl reload nginx

healthy=false
for _ in $(seq 1 30); do
  if curl --fail --silent http://127.0.0.1:5002/health/live >/dev/null \
    && curl --fail --silent http://127.0.0.1:5002/health/ready >/dev/null \
    && curl --fail --silent http://127.0.0.1:5002/api/Categories >/dev/null; then
    healthy=true
    break
  fi
  sleep 2
done

if [ "$healthy" != true ]; then
  echo "New release failed its health check; rolling back application files." >&2
  systemctl stop flowershop || true
  rm -rf "$APP_DIR" "$WEB_DIR"
  if [ -d "$APP_PREVIOUS" ]; then mv "$APP_PREVIOUS" "$APP_DIR"; fi
  if [ -d "$WEB_PREVIOUS" ]; then mv "$WEB_PREVIOUS" "$WEB_DIR"; fi
  systemctl start flowershop
  systemctl reload nginx
  exit 1
fi

rm -f "$ARCHIVE"
echo "Release installed successfully; API health check passed."
