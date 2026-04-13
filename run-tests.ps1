# Parallel Test Execution Helper Script for PowerShell
# Usage: .\run-tests.ps1 -Category Movement
# Example: .\run-tests.ps1 -Category Movement

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Movement", "Attack", "Enemy", "Life", "Item", "UI")]
    [string]$Category,
    
    [Parameter(Mandatory=$false)]
    [switch]$All
)

# Get the project root directory
$ProjectDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Find Unity installation
$UnityPaths = @(
    "C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe",
    "$env:PROGRAMFILES\Unity\Hub\Editor\*\Editor\Unity.exe"
)

$UnityPath = $null
foreach ($path in $UnityPaths) {
    $resolved = Resolve-Path $path -ErrorAction SilentlyContinue
    if ($resolved) {
        $UnityPath = $resolved[0].Path
        break
    }
}

if (-not $UnityPath) {
    Write-Error "Unity installation not found. Please specify the path manually."
    exit 1
}

Write-Host "Using Unity: $UnityPath" -ForegroundColor Cyan

# Function to run a single test category
function Run-TestCategory {
    param([string]$TestCategory)
    
    Write-Host "`nRunning $TestCategory tests..." -ForegroundColor Green
    Write-Host "Command: & `"$UnityPath`" -projectPath `"$ProjectDir`" -runTests -testMode playmode -testCategory $TestCategory -logFile -" -ForegroundColor Gray
    
    & "$UnityPath" -projectPath "$ProjectDir" -runTests -testMode playmode -testCategory $TestCategory -logFile -
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "$TestCategory tests completed successfully" -ForegroundColor Green
    } else {
        Write-Host "$TestCategory tests failed with exit code $LASTEXITCODE" -ForegroundColor Red
        return $false
    }
    return $true
}

# Main execution
if ($All) {
    Write-Host "Running ALL test categories in sequence..." -ForegroundColor Cyan
    $categories = @("Movement", "Attack", "Enemy", "Life", "Item", "UI")
    $failed = @()
    
    foreach ($cat in $categories) {
        if (-not (Run-TestCategory $cat)) {
            $failed += $cat
        }
    }
    
    Write-Host "`n========== Test Summary ==========" -ForegroundColor Cyan
    if ($failed.Count -eq 0) {
        Write-Host "✅ All test categories passed!" -ForegroundColor Green
    } else {
        Write-Host "❌ The following categories failed:" -ForegroundColor Red
        $failed | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
        exit 1
    }
} elseif ($Category) {
    Run-TestCategory $Category
} else {
    Write-Host @"
Parallel Test Execution Script for GAM301

Usage:
  .\run-tests.ps1 -Category <CategoryName>    # Run a specific test category
  .\run-tests.ps1 -All                        # Run all categories in sequence

Available Categories:
  - Movement (TC1-TC10):    10 tests
  - Attack (TC11-TC21):      11 tests
  - Enemy (TC22-TC28):       7 tests
  - Life (TC29-TC35):        7 tests
  - Item (TC36-TC45):        10 tests
  - UI (TC46-TC57):          12 tests

Examples:
  .\run-tests.ps1 -Category Movement          # Run only Movement tests
  .\run-tests.ps1 -Category Attack            # Run only Attack tests
  .\run-tests.ps1 -All                        # Run all categories sequentially

For parallel execution:
  1. Open GitHub Actions -> parallel-tests workflow
  2. Or run individual categories in separate terminals manually
  3. Or see PARALLEL_TESTING_GUIDE.md for detailed options
"@
}
# Full local test suite
.\run-tests.ps1 -All
