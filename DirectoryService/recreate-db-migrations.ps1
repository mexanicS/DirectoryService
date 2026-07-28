param(
    [string]$MigrationName = "Initial",
    [switch]$KeepMigrations = $false,
    [switch]$KeepDatabase = $false
)

$ErrorActionPreference = "Stop"

$infrastructureProject = "src/DirectoryService.Infrastructure"
$presentationProject = "src/DirectoryService.Presentation"
$migrationsPath = "$infrastructureProject/Migrations"

Write-Host "`n==========================================" -ForegroundColor Cyan
Write-Host "   Starting Database & Migration Reset    " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# 1. Drop Database
if (-not $KeepDatabase) {
    Write-Host "--> Dropping database..." -ForegroundColor Yellow
    dotnet ef database drop --force --project $infrastructureProject --startup-project $presentationProject
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to drop the database."
    }
    Write-Host "Database dropped successfully." -ForegroundColor Green
}

# 2. Clear previous migrations
if (-not $KeepMigrations) {
    if (Test-Path $migrationsPath) {
        Write-Host "--> Deleting existing migrations in $migrationsPath..." -ForegroundColor Yellow
        Remove-Item -Path $migrationsPath -Recurse -Force
        Write-Host "Migrations folder deleted." -ForegroundColor Green
    } else {
        Write-Host "--> No existing migrations folder found to delete." -ForegroundColor Gray
    }
}

# 3. Create new migration
Write-Host "--> Creating a new migration: '$MigrationName'..." -ForegroundColor Yellow
dotnet ef migrations add $MigrationName --project $infrastructureProject --startup-project $presentationProject
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create migration '$MigrationName'."
}
Write-Host "Migration '$MigrationName' created successfully." -ForegroundColor Green

# 4. Update Database
Write-Host "--> Applying migrations to the database..." -ForegroundColor Yellow
dotnet ef database update --project $infrastructureProject --startup-project $presentationProject
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to update database."
}

Write-Host "==========================================" -ForegroundColor Green
Write-Host " Database & migrations reset successfully! " -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
