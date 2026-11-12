[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-Za-z0-9-]{1,39}$')]
    [string]$GitHubUsername
)

$ErrorActionPreference = 'Stop'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'Install .NET SDK 10 to run this development-time restore check.'
}

$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$solutionPath = Join-Path $projectRoot 'DirectoryService.sln'
$nugetConfigPath = Join-Path $projectRoot 'nuget.config'
$runPath = Join-Path $projectRoot ('artifacts/nuget-restore-check/' + [guid]::NewGuid().ToString('N'))
$packagesPath = Join-Path $runPath 'packages'

$credentialVariable = 'NuGetPackageSourceCredentials_github'
$environmentNames = @($credentialVariable, 'NUGET_HTTP_CACHE_PATH', 'NUGET_PLUGINS_CACHE_PATH')
$previousEnvironment = @{}
foreach ($environmentName in $environmentNames) {
    $previousEnvironment[$environmentName] = [Environment]::GetEnvironmentVariable($environmentName, 'Process')
}

$secureToken = $null
$plainToken = $null
$tokenPointer = [IntPtr]::Zero

try {
    $secureToken = Read-Host 'GitHub PAT classic with read:packages (hidden input)' -AsSecureString
    if ($secureToken.Length -eq 0) {
        throw 'A non-empty GitHub token is required.'
    }

    $tokenPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureToken)
    $plainToken = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($tokenPointer)
    if ($plainToken -match '[;\r\n]' -or [string]::IsNullOrWhiteSpace($plainToken)) {
        throw 'The token contains invalid whitespace or credential separators.'
    }

    [Environment]::SetEnvironmentVariable(
        $credentialVariable,
        "Username=$GitHubUsername;Password=$plainToken;ValidAuthenticationTypes=Basic",
        'Process'
    )
    [Environment]::SetEnvironmentVariable('NUGET_HTTP_CACHE_PATH', (Join-Path $runPath 'http-cache'), 'Process')
    [Environment]::SetEnvironmentVariable('NUGET_PLUGINS_CACHE_PATH', (Join-Path $runPath 'plugin-cache'), 'Process')

    New-Item -ItemType Directory -Path $runPath -Force | Out-Null
    Write-Host "Checking restore with a fresh package directory: $packagesPath"

    $restoreArguments = @(
        'restore', $solutionPath,
        '--configfile', $nugetConfigPath,
        '--packages', $packagesPath,
        '--artifacts-path', $runPath,
        '--no-http-cache',
        '--force',
        '--verbosity', 'minimal',
        '-p:RestoreFallbackFolders='
    )

    & dotnet @restoreArguments
    if ($LASTEXITCODE -ne 0) {
        throw "Restore failed with exit code $LASTEXITCODE. Check the NuGet error above."
    }

    Write-Host 'Restore succeeded with the repository config and a fresh package directory.' -ForegroundColor Green
}
finally {
    foreach ($environmentName in $environmentNames) {
        [Environment]::SetEnvironmentVariable($environmentName, $previousEnvironment[$environmentName], 'Process')
    }
    if ($tokenPointer -ne [IntPtr]::Zero) {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($tokenPointer)
    }
    if ($null -ne $secureToken) {
        $secureToken.Dispose()
    }
    $plainToken = $null
}
