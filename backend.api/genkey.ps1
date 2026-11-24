param(
    [string]$Project = ""
)

$ErrorActionPreference = "Stop"

# Default to the folder that contains this script (Backend.API).
if ([string]::IsNullOrWhiteSpace($Project)) {
    $Project = $PSScriptRoot
}

$projectFile = if (Test-Path -LiteralPath $Project -PathType Container) {
    Get-ChildItem -LiteralPath $Project -Filter "*.csproj" |
        Select-Object -First 1 -ExpandProperty FullName
} else {
    $Project
}

if (-not $projectFile -or -not (Test-Path -LiteralPath $projectFile)) {
    throw "Unable to find a .csproj under '$Project'."
}

Write-Host "Using project: $projectFile"

# Ensure User Secrets are initialized for this project.
dotnet user-secrets list --project $projectFile 1>$null 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Initializing User Secrets ..."
    dotnet user-secrets init --project $projectFile
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to initialize User Secrets."
    }
}

$secrets = dotnet user-secrets list --project $projectFile
if ($LASTEXITCODE -ne 0) {
    throw "Unable to read User Secrets. Check the project and its UserSecretsId."
}

if ($secrets | Where-Object { $_ -match '^JwtConfig:Key\s*=' }) {
    Write-Host "JWT key already exists: no change."
}
else {
    $bytes = [byte[]]::new(32)
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()

    try {
        $rng.GetBytes($bytes)
    }
    finally {
        $rng.Dispose()
    }

    $key = [Convert]::ToBase64String($bytes)

    dotnet user-secrets set "JwtConfig:Key" $key --project $projectFile
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to store the JWT key."
    }

    Write-Host "Local JWT key created and stored."
}
