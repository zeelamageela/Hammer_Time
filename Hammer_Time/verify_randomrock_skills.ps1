# Final Verification - RandomRockPlacement Skill System
# Ensures all old skill references are gone and new system is in place

$filePath = "Assets\Scripts\RandomRockPlacerment.cs"
$content = Get-Content $filePath -Raw

Write-Host "`n=== FINAL VERIFICATION ===" -ForegroundColor Cyan
Write-Host "File: $filePath`n" -ForegroundColor Cyan

# Check for OLD skill system (should be 0 except in comments)
Write-Host "OLD Skill System References:" -ForegroundColor Yellow
$oldDraw = ([regex]::Matches($content, "(?<!//.*)(\.drawAccuracy)")).Count
$oldGuard = ([regex]::Matches($content, "(?<!//.*)(\.guardAccuracy)")).Count  
$oldTakeout = ([regex]::Matches($content, "(?<!//.*)(\.takeOutAccuracy)")).Count

Write-Host "  .drawAccuracy    : $oldDraw" -ForegroundColor $(if ($oldDraw -eq 0) { "Green" } else { "Red" })
Write-Host "  .guardAccuracy   : $oldGuard" -ForegroundColor $(if ($oldGuard -eq 0) { "Green" } else { "Red" })
Write-Host "  .takeOutAccuracy : $oldTakeout" -ForegroundColor $(if ($oldTakeout -eq 0) { "Green" } else { "Red" })

# Check for NEW skill system (should be > 0)
Write-Host "`nNEW Skill System References:" -ForegroundColor Yellow
$newWeight = ([regex]::Matches($content, "weightAccuracy")).Count
$newAim = ([regex]::Matches($content, "aimAccuracy")).Count  
$newFinesse = ([regex]::Matches($content, "finesseAccuracy")).Count

Write-Host "  weightAccuracy   : $newWeight" -ForegroundColor $(if ($newWeight -gt 0) { "Green" } else { "Red" })
Write-Host "  aimAccuracy      : $newAim" -ForegroundColor $(if ($newAim -gt 0) { "Green" } else { "Red" })
Write-Host "  finesseAccuracy  : $newFinesse" -ForegroundColor $(if ($newFinesse -gt 0) { "Green" } else { "Red" })

# Check for proper usage patterns
Write-Host "`nUsage Patterns:" -ForegroundColor Yellow
$getValueCalls = ([regex]::Matches($content, "\.GetValue\(\)")).Count
$combinedAccuracy = ([regex]::Matches($content, "combinedAccuracy")).Count
$skillCombos = ([regex]::Matches($content, "\* 0\.[357]0?f")).Count

Write-Host "  .GetValue() calls        : $getValueCalls" -ForegroundColor Green
Write-Host "  combinedAccuracy vars    : $combinedAccuracy" -ForegroundColor Green
Write-Host "  Skill combinations (%)   : $skillCombos" -ForegroundColor Green

# Overall status
Write-Host "`n=== OVERALL STATUS ===" -ForegroundColor Cyan
$passed = ($oldDraw -eq 0) -and ($oldGuard -eq 0) -and ($oldTakeout -eq 0) -and ($newWeight -gt 0) -and ($newAim -gt 0) -and ($newFinesse -gt 0)

if ($passed) {
    Write-Host "? ALL CHECKS PASSED!" -ForegroundColor Green
    Write-Host "   - Old skill system: REMOVED" -ForegroundColor Green
    Write-Host "   - New skill system: IN PLACE" -ForegroundColor Green
    Write-Host "   - Ready for testing!" -ForegroundColor Green
} else {
    Write-Host "? SOME CHECKS FAILED" -ForegroundColor Red
    if ($oldDraw -gt 0 -or $oldGuard -gt 0 -or $oldTakeout -gt 0) {
        Write-Host "   - Old skill references still exist!" -ForegroundColor Red
    }
    if ($newWeight -eq 0 -or $newAim -eq 0 -or $newFinesse -eq 0) {
        Write-Host "   - New skill system not fully implemented!" -ForegroundColor Red
    }
}

Write-Host ""
