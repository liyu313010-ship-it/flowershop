# Automated production deployment

Every push to `main` runs `.github/workflows/deploy.yml`. The workflow connects
to the server as a restricted deployment user and invokes
`/usr/local/sbin/deploy-flowershop` through its single allowed sudo command.

Required GitHub production environment secrets:

- `SERVER_HOST`
- `SERVER_PORT`
- `SERVER_USER`
- `SERVER_SSH_PRIVATE_KEY`

The one-time server bootstrap installs the tracked `deploy-flowershop.sh` as a
root-owned command, configures the deployment user, application environment,
systemd unit, MariaDB database, persistent upload directory, and Nginx port
8080 site.
