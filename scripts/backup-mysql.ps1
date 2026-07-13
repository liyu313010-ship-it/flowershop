param(
    [string]$Container = "flowershop-db-1",
    [string]$Database = $env:MYSQL_DATABASE,
    [string]$User = $env:MYSQL_USER,
    [string]$Password = $env:MYSQL_PASSWORD,
    [string]$OutputDirectory = "./backups"
)

$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($Database) -or [string]::IsNullOrWhiteSpace($User) -or [string]::IsNullOrWhiteSpace($Password)) {
    throw "MYSQL_DATABASE, MYSQL_USER and MYSQL_PASSWORD must be set"
}

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$file = Join-Path $OutputDirectory ("flowershop-{0:yyyyMMdd-HHmmss}.sql.gz" -f (Get-Date))
docker exec $Container mysqldump --single-transaction --routines --triggers --hex-blob -u$User -p$Password $Database | gzip > $file
Write-Host "Backup created: $file"
