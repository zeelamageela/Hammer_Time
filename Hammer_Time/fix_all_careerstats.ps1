# Comprehensive fix for all CareerStats references across the codebase
# Renames: drawAccuracy ? weightAccuracy, takeOutAccuracy ? aimAccuracy, guardAccuracy ? finesseAccuracy

$files = @(
    "Assets\Scripts\RandomRockPlacerment.cs",
    "Assets\Scripts\EndMenu.cs",
    "Assets\Scripts\EquipmentManager.cs",
    "Assets\Scripts\QuickTestGame.cs",
    "Assets\Scripts\TeamMenu.cs",
    "Assets\Scripts\Tourny\CareerManager.cs"
)

foreach ($file in $files) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        
        # Replace CareerStats property references (no .GetValue() - these are int properties)
        $content = $content -replace '\.drawAccuracy', '.weightAccuracy'
        $content = $content -replace '\.takeOutAccuracy', '.aimAccuracy'
        $content = $content -replace '\.guardAccuracy', '.finesseAccuracy'
        
        # Also fix any variable names that use these
        $content = $content -replace '\bdrawAccuracy\b', 'weightAccuracy'
        $content = $content -replace '\btakeOutAccuracy\b', 'aimAccuracy'
        $content = $content -replace '\bguardAccuracy\b', 'finesseAccuracy'
        
        # Special case: variable names in code
        $content = $content -replace '\bdrawAccu\b', 'weightAccu'
        $content = $content -replace '\bguardAccu\b', 'finesseAccu'
        $content = $content -replace '\btakeOutAccu\b', 'aimAccu'
        
        Set-Content -Path $file -Value $content
        Write-Host "? Fixed $file" -ForegroundColor Green
    } else {
        Write-Host "? File not found: $file" -ForegroundColor Red
    }
}

Write-Host "`n? All Career Stats references updated!" -ForegroundColor Cyan
