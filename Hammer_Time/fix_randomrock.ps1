# Fix RandomRockPlacement.cs corruption
$content = Get-Content 'Assets\Scripts\RandomRockPlacerment.cs' -Raw
$content = $content -replace '^-NoNewline\s*', ''
Set-Content 'Assets\Scripts\RandomRockPlacerment.cs' -Value $content
Write-Host "Fixed RandomRockPlacement.cs" -ForegroundColor Green
