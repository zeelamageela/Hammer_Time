$content = Get-Content 'Assets\Scripts\RockManager.cs' -Raw
$content = $content -replace 'RandomRockPlacement', 'RandomRockPlacerment'
Set-Content 'Assets\Scripts\RockManager.cs' -Value $content
Write-Host "Reverted to RandomRockPlacerment" -ForegroundColor Green
