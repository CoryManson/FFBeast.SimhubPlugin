# Build script for FFBeast SimHub Plugin and Console App
# Usage: .\build.ps1 [-Plugin] [-Console] [-All] [-Clean]

param(
    [switch]$Plugin,
    [switch]$Console,
    [switch]$All,
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

# Colors for output
function Write-Success { param($msg) Write-Host $msg -ForegroundColor Green }
function Write-Info { param($msg) Write-Host $msg -ForegroundColor Cyan }
function Write-Warning { param($msg) Write-Host $msg -ForegroundColor Yellow }
function Write-Error { param($msg) Write-Host $msg -ForegroundColor Red }

# Banner
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Host "  FFBeast Wheelbase Control - Build Script" -ForegroundColor Magenta
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Host ""

# Check if at least one option is selected
if (-not ($Plugin -or $Console -or $All)) {
    Write-Warning "No build target specified. Building both by default."
    $All = $true
}

# Set working directory to script location
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath
Write-Info "Working directory: $scriptPath"
Write-Host ""

# Check for SimHub installation path (only if building plugin)
if ($Plugin -or $All) {
    if (-not $env:SIMHUB_INSTALL_PATH) {
        Write-Warning "SIMHUB_INSTALL_PATH environment variable not set!"
        $defaultPath = "C:\Program Files (x86)\SimHub\"
        if (Test-Path $defaultPath) {
            Write-Warning "Using default path: $defaultPath"
            $env:SIMHUB_INSTALL_PATH = $defaultPath
        } else {
            Write-Error "SimHub not found at default location."
            Write-Host "Please set SIMHUB_INSTALL_PATH environment variable:"
            Write-Host '[System.Environment]::SetEnvironmentVariable("SIMHUB_INSTALL_PATH", "C:\Program Files (x86)\SimHub\", "User")' -ForegroundColor Gray
            exit 1
        }
    } else {
        Write-Success "✓ SIMHUB_INSTALL_PATH: $env:SIMHUB_INSTALL_PATH"
    }
    Write-Host ""
}

# Clean if requested
if ($Clean) {
    Write-Info "Cleaning build directories..."
    if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
    if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
    Write-Success "✓ Clean completed"
    Write-Host ""
}

# Build SimHub Plugin
if ($Plugin -or $All) {
    Write-Info "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    Write-Info "  Building SimHub Plugin..."
    Write-Info "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    try {
        $output = dotnet build FFBeast.SimHubPlugin.csproj -c Release --nologo 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Success "✓ SimHub Plugin build successful!"
            
            # Check if files exist in output
            $dllPath = "bin\Release\FFBeast.SimHubPlugin.dll"
            if (Test-Path $dllPath) {
                $fileInfo = Get-Item $dllPath
                Write-Host "  → Output: $dllPath" -ForegroundColor Gray
                Write-Host "  → Size: $([math]::Round($fileInfo.Length/1KB, 2)) KB" -ForegroundColor Gray
                Write-Host "  → Modified: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
                
                # Check if copied to SimHub
                $targetPath = Join-Path $env:SIMHUB_INSTALL_PATH "FFBeast.SimHubPlugin.dll"
                if (Test-Path $targetPath) {
                    Write-Success "  ✓ DLL copied to SimHub directory"
                } else {
                    Write-Warning "  ⚠ DLL not found in SimHub directory"
                    Write-Host "  Manual copy command:" -ForegroundColor Gray
                    Write-Host "  Copy-Item '$dllPath' -Destination '$env:SIMHUB_INSTALL_PATH'" -ForegroundColor DarkGray
                }
            }
            Write-Host ""
        } else {
            Write-Error "✗ SimHub Plugin build failed!"
            Write-Host $output -ForegroundColor Red
            exit 1
        }
    } catch {
        Write-Error "✗ Build error: $_"
        exit 1
    }
}

# Build Console App
if ($Console -or $All) {
    Write-Info "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    Write-Info "  Building Console Application..."
    Write-Info "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    try {
        $output = dotnet build FFBeastRecenter.csproj -c Release --nologo 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Success "✓ Console Application build successful!"
            
            $exePath = "bin\Release\net8.0\FFBeastRecenter.exe"
            if (Test-Path $exePath) {
                $fileInfo = Get-Item $exePath
                Write-Host "  → Output: $exePath" -ForegroundColor Gray
                Write-Host "  → Size: $([math]::Round($fileInfo.Length/1KB, 2)) KB" -ForegroundColor Gray
                Write-Host "  → Modified: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
                
                Write-Host "`n  To run:" -ForegroundColor Gray
                Write-Host "  .\\$exePath" -ForegroundColor DarkGray
            }
            Write-Host ""
        } else {
            Write-Error "✗ Console Application build failed!"
            Write-Host $output -ForegroundColor Red
            exit 1
        }
    } catch {
        Write-Error "✗ Build error: $_"
        exit 1
    }
}

# Summary
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Success "  Build Complete!"
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Host ""

if ($Plugin -or $All) {
    Write-Host "📦 SimHub Plugin:" -ForegroundColor Yellow
    Write-Host "   • Restart SimHub to load the plugin" -ForegroundColor White
    Write-Host "   • Go to Settings → Additional Plugins" -ForegroundColor White
    Write-Host "   • Look for 'FFBeast Wheelbase Control'" -ForegroundColor White
    Write-Host ""
}

if ($Console -or $All) {
    Write-Host "🖥️  Console App:" -ForegroundColor Yellow
    Write-Host "   • Run: .\\bin\\Release\\net8.0\\FFBeastRecenter.exe" -ForegroundColor White
    Write-Host "   • Configure key in config.json" -ForegroundColor White
    Write-Host ""
}

Write-Host "📚 Documentation:" -ForegroundColor Cyan
Write-Host "   • README.md - Full documentation" -ForegroundColor White
Write-Host "   • QUICKSTART.md - Quick setup guide" -ForegroundColor White
Write-Host "   • BUILD.md - Detailed build instructions" -ForegroundColor White
Write-Host ""

Write-Success "Happy racing! 🏎️"
