# Final comprehensive fix for all remaining CareerStats references

$files = @(
    "Assets\Scripts\Tourny\SponsorManager.cs",
    "Assets\Scripts\Tourny\TournyManager.cs",
    "Assets\Scripts\XPManager.cs"
)

foreach ($file in $files) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        
        # Replace CareerStats property references
        $content = $content -replace '\.drawAccuracy', '.weightAccuracy'
        $content = $content -replace '\.takeOutAccuracy', '.aimAccuracy'
        $content = $content -replace '\.guardAccuracy', '.finesseAccuracy'
        
        # Fix variable names
        $content = $content -replace '\bdrawAccuracy\b', 'weightAccuracy'
        $content = $content -replace '\btakeOutAccuracy\b', 'aimAccuracy'
        $content = $content -replace '\bguardAccuracy\b', 'finesseAccuracy'
        
        Set-Content -Path $file -Value $content
        Write-Host "? Fixed $file" -ForegroundColor Green
    }
}

# Fix CareerStatsData in save data
$file = "Assets\Scripts\Tourny\SaveData\CareerSaveData.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw
    $content = $content -replace 'public int drawAccuracy;', 'public int weightAccuracy;   // Y-axis accuracy (distance/weight control)'
    $content = $content -replace 'public int guardAccuracy;', 'public int finesseAccuracy;   // Complex shot bonus (finesse techniques)'
    $content = $content -replace 'public int takeOutAccuracy;', 'public int aimAccuracy;       // X-axis accuracy (lateral positioning)'
    Set-Content -Path $file -Value $content
    Write-Host "? Fixed CareerStatsData in $file" -ForegroundColor Green
}

# Fix RockManager.cs typo
$file = "Assets\Scripts\RockManager.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw
    $content = $content -replace 'RandomRockPlacerment', 'RandomRockPlacement'
    Set-Content -Path $file -Value $content
    Write-Host "? Fixed typo in $file" -ForegroundColor Green
}

Write-Host "`n? All remaining references updated!" -ForegroundColor Cyan
