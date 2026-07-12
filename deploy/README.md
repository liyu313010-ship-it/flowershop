# Automated production deployment

Every push to `main` runs `.github/workflows/deploy.yml`. GitHub's runner builds
the Vue and ASP.NET applications, generates per-migration SQL files, uploads one
release archive, and invokes `/usr/local/sbin/install-flowershop-release` through
the restricted deployment user's allowlisted sudo command. The small production
server does not run npm, Vite, or dotnet publish during routine deployments.

Required GitHub production environment secrets:

- `SERVER_HOST`
- `SERVER_PORT`
- `SERVER_USER`
- `SERVER_SSH_PRIVATE_KEY`

The one-time server bootstrap installs the root-owned release installer,
configures the deployment user, application environment, systemd unit, MariaDB
database, persistent upload directory, and Nginx port 8080 site. Releases use a
deployment lock, staged directory swaps, health verification, and file rollback.
