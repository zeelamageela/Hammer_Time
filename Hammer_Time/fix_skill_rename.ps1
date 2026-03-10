# PowerShell script to rename skill properties from drawAccuracy/guardAccuracy/takeOutAccuracy
# to weightAccuracy/aimAccuracy/finesseAccuracy

# Fix RandomRockPlacement CareerStats references (int properties, no .GetValue())
$filePath = "Assets\Scripts\RandomRockPlacerment.cs"
$content = Get-Content $filePath -Raw
$content = $content -replace 'cm\.cStats\.weightAccuracy\.GetValue\(\)', 'cm.cStats.weightAccuracy'
Set-Content -Path $filePath -Value $content -NoNewline
Write-Host "? Fixed CareerStats references in $filePath" -ForegroundColor Green

# Fix TrajectoryLine CareerStats references
$filePath = "Assets\Scripts\UI\TrajectoryLine.cs"
$content = Get-Content $filePath -Raw
$content = $content -replace 'cm\.cStats\.aimAccuracy\.GetValue\(\)', 'cm.cStats.aimAccuracy'
Set-Content -Path $filePath -Value $content -NoNewline
Write-Host "? Fixed CareerStats references in $filePath" -ForegroundColor Green

Write-Host "All CareerStats references updated!" -ForegroundColor Cyan

