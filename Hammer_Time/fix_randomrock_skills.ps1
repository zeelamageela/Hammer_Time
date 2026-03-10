# Fix RandomRockPlacement skill system references
# This script converts old skill system to new skill system

$filePath = "Assets\Scripts\RandomRockPlacerment.cs"
$content = Get-Content $filePath -Raw

Write-Host "Starting skill system conversion in RandomRockPlacement.cs..." -ForegroundColor Cyan

# Count occurrences before
$drawCount = ([regex]::Matches($content, "\.drawAccuracy\.GetValue\(\)")).Count
$guardCount = ([regex]::Matches($content, "\.guardAccuracy\.GetValue\(\)")).Count  
$takeOutCount = ([regex]::Matches($content, "\.takeOutAccuracy\.GetValue\(\)")).Count

Write-Host "Found:" -ForegroundColor Yellow
Write-Host "  - drawAccuracy: $drawCount" -ForegroundColor Yellow
Write-Host "  - guardAccuracy: $guardCount" -ForegroundColor Yellow
Write-Host "  - takeOutAccuracy: $takeOutCount" -ForegroundColor Yellow

# TAKEOUT ACCURACY (Aim 50% + Weight 50%)
# Replace all takeOutAccuracy with combined aim+weight
$content = $content -replace 'activeCharStats\.takeOutAccuracy\.GetValue\(\)', '(int)((activeCharStats.aimAccuracy.GetValue() * 0.5f) + (activeCharStats.weightAccuracy.GetValue() * 0.5f))'

# GUARD ACCURACY (Weight 50% + Aim 50%)  
# Replace all guardAccuracy with combined weight+aim
$content = $content -replace 'activeCharStats\.guardAccuracy\.GetValue\(\)', '(int)((activeCharStats.weightAccuracy.GetValue() * 0.5f) + (activeCharStats.aimAccuracy.GetValue() * 0.5f))'

# DRAW ACCURACY (Weight 50% + Aim 50%)
# Replace all drawAccuracy with combined weight+aim
$content = $content -replace 'activeCharStats\.drawAccuracy\.GetValue\(\)', '(int)((activeCharStats.weightAccuracy.GetValue() * 0.5f) + (activeCharStats.aimAccuracy.GetValue() * 0.5f))'

# CASE 0 in ShotSelector - uses drawAccuracy directly in math
$content = $content -replace '\(1\.5f - \(0\.01f \* activeCharStats\.drawAccuracy\.GetValue\(\)\)\)', '(1.5f - (0.01f * ((activeCharStats.weightAccuracy.GetValue() * 0.5f) + (activeCharStats.aimAccuracy.GetValue() * 0.5f))))'

# CASE 3 in ShotSelector - uses guardAccuracy in range
$content = $content -replace '\(Random\.Range\(0f, 1\.5f - \(0\.01f \* activeCharStats\.guardAccuracy\.GetValue\(\)\)\)\)', '(Random.Range(0f, 1.5f - (0.01f * ((activeCharStats.weightAccuracy.GetValue() * 0.5f) + (activeCharStats.aimAccuracy.GetValue() * 0.5f)))))'

# CASE 4 in ShotSelector - takeout uses takeOutAccuracy in math
$content = $content -replace '\(Random\.insideUnitCircle \* \(1\.5f - \(0\.005f \* activeCharStats\.takeOutAccuracy\.GetValue\(\)\)\)\)', '(Random.insideUnitCircle * (1.5f - (0.005f * ((activeCharStats.aimAccuracy.GetValue() * 0.5f) + (activeCharStats.weightAccuracy.GetValue() * 0.5f)))))'
$content = $content -replace '\(Random\.insideUnitCircle \* \(1\.5f - \(0\.01f \* activeCharStats\.takeOutAccuracy\.GetValue\(\)\)\)\)', '(Random.insideUnitCircle * (1.5f - (0.01f * ((activeCharStats.aimAccuracy.GetValue() * 0.5f) + (activeCharStats.weightAccuracy.GetValue() * 0.5f)))))'

# CASE 5 in ShotSelector - freeze uses drawAccuracy (should use finesse+weight)
# Freeze skill checks
$content = $content -replace 'SkillCheck\("Freeze", activeCharStats\.drawAccuracy\.GetValue\(\)\)', 'SkillCheck("Freeze", (int)((activeCharStats.finesseAccuracy.GetValue() * 0.7f) + (activeCharStats.weightAccuracy.GetValue() * 0.3f)))'
$content = $content -replace 'SkillCheck\("Freeze", activeCharStats\.takeOutAccuracy\.GetValue\(\)\)', 'SkillCheck("Freeze", (int)((activeCharStats.finesseAccuracy.GetValue() * 0.7f) + (activeCharStats.weightAccuracy.GetValue() * 0.3f)))'

# Freeze math in case 5
$content = $content -replace '\(0\.5f - \(0\.005f \* activeCharStats\.drawAccuracy\.GetValue\(\)\)\)', '(0.5f - (0.005f * ((activeCharStats.finesseAccuracy.GetValue() * 0.7f) + (activeCharStats.weightAccuracy.GetValue() * 0.3f))))'
$content = $content -replace '\(2f - \(0\.01f \* activeCharStats\.drawAccuracy\.GetValue\(\)\)\)', '(2f - (0.01f * ((activeCharStats.finesseAccuracy.GetValue() * 0.7f) + (activeCharStats.weightAccuracy.GetValue() * 0.3f))))'
$content = $content -replace '0\.5f - \(0\.005f \* activeCharStats\.drawAccuracy\.GetValue\(\)\)', '0.5f - (0.005f * ((activeCharStats.finesseAccuracy.GetValue() * 0.7f) + (activeCharStats.weightAccuracy.GetValue() * 0.3f)))'
$content = $content -replace '1\.5f - \(0\.005f \* activeCharStats\.drawAccuracy\.GetValue\(\)\)', '1.5f - (0.005f * ((activeCharStats.finesseAccuracy.GetValue() * 0.7f) + (activeCharStats.weightAccuracy.GetValue() * 0.3f)))'
$content = $content -replace '0\.1f \* activeCharStats\.drawAccuracy\.GetValue\(\)', '0.1f * ((activeCharStats.finesseAccuracy.GetValue() * 0.7f) + (activeCharStats.weightAccuracy.GetValue() * 0.3f))'

# CASE 6 in ShotSelector - uses guardAccuracy 
$content = $content -replace '\(Random\.Range\(0f, 1\.5f - \(0\.01f \* activeCharStats\.guardAccuracy\.GetValue\(\)\)\)\)', '(Random.Range(0f, 1.5f - (0.01f * ((activeCharStats.weightAccuracy.GetValue() * 0.5f) + (activeCharStats.aimAccuracy.GetValue() * 0.5f)))))'

# Save the file
$content | Set-Content $filePath -NoNewline

Write-Host "`nConversion complete!" -ForegroundColor Green
Write-Host "File updated: $filePath" -ForegroundColor Green

# Verify
$newContent = Get-Content $filePath -Raw
$drawCountAfter = ([regex]::Matches($newContent, "\.drawAccuracy\.GetValue\(\)")).Count
$guardCountAfter = ([regex]::Matches($newContent, "\.guardAccuracy\.GetValue\(\)")).Count  
$takeOutCountAfter = ([regex]::Matches($newContent, "\.takeOutAccuracy\.GetValue\(\)")).Count

Write-Host "`nRemaining old references:" -ForegroundColor Cyan
Write-Host "  - drawAccuracy: $drawCountAfter" -ForegroundColor $(if ($drawCountAfter -eq 0) { "Green" } else { "Red" })
Write-Host "  - guardAccuracy: $guardCountAfter" -ForegroundColor $(if ($guardCountAfter -eq 0) { "Green" } else { "Red" })
Write-Host "  - takeOutAccuracy: $takeOutCountAfter" -ForegroundColor $(if ($takeOutCountAfter -eq 0) { "Green" } else { "Red" })

if ($drawCountAfter -gt 0 -or $guardCountAfter -gt 0 -or $takeOutCountAfter -gt 0) {
    Write-Host "`nWARNING: Some old skill references remain! Manual review needed." -ForegroundColor Red
} else {
    Write-Host "`nSUCCESS: All old skill references converted!" -ForegroundColor Green
}
