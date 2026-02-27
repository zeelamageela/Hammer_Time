# PowerShell script to fix PlayoffManager.cs opponent finding bug
# Run this script from the workspace root directory

$file = "Assets\Scripts\Tourny\PlayoffManager.cs"

# Read the file
$content = Get-Content $file -Raw

# Define the old code to replace (exact match with tabs)
$oldCode = @"
	for (int i = 0; i < tm.teams.Length; i++)
	{
		if (tm.teams[i].player)
			playerTeam = i;
	}
	for (int i = 0; i < tm.teams.Length; i++)
	{
		if (tm.teams[i].name == tm.teams[playerTeam].nextOpp)
			oppTeam = i;
}
Debug.Log("OppTeam is " + oppTeam);
"@

# Define the new code
$newCode = @"
	for (int i = 0; i < tm.teams.Length; i++)
	{
		if (tm.teams[i].player)
			playerTeam = i;
	}
	
	// CRITICAL FIX: Find opponent using game scores (more reliable than nextOpp after loading)
	// When returning from a completed game, nextOpp hasn't been set yet by SetPlayoffs()
	oppTeam = -1;
	
	if (playerTeam >= 0 && playerTeam < tm.teams.Length)
	{
		string playerTeamName = tm.teams[playerTeam].name;
		
		// Use the game that was just played to determine opponent
		if (playerTeamName == gsp.redTeamName)
		{
			// Player was red, opponent was yellow
			for (int i = 0; i < tm.teams.Length; i++)
			{
				if (tm.teams[i].name == gsp.yellowTeamName)
				{
					oppTeam = i;
					Debug.Log(`$"[LoadAndAdvancePlayoffs] Found opponent via game scores: {tm.teams[i].name} (red/yellow)");
					break;
				}
			}
		}
		else if (playerTeamName == gsp.yellowTeamName)
		{
			// Player was yellow, opponent was red
			for (int i = 0; i < tm.teams.Length; i++)
			{
				if (tm.teams[i].name == gsp.redTeamName)
				{
					oppTeam = i;
					Debug.Log(`$"[LoadAndAdvancePlayoffs] Found opponent via game scores: {tm.teams[i].name} (yellow/red)");
					break;
				}
			}
		}
	}
	
	if (oppTeam < 0)
	{
		Debug.LogError(`$"[LoadAndAdvancePlayoffs] CRITICAL: Could not find opponent team!");
		Debug.LogError(`$"  playerTeam={playerTeam}, playerName={tm.teams[playerTeam]?.name}");
		Debug.LogError(`$"  gsp.redTeamName={gsp.redTeamName}, gsp.yellowTeamName={gsp.yellowTeamName}");
		Debug.LogError(`$"  Skipping match processing to prevent crash!");
		playoffRound++;
		SetPlayoffs();
		return;
	}
	
Debug.Log("OppTeam is " + oppTeam);
"@

# Perform the replacement
if ($content -match [regex]::Escape($oldCode))
{
    Write-Host "? Found the code to replace" -ForegroundColor Green
    $content = $content.Replace($oldCode, $newCode)
    
    # Write back to file
    Set-Content -Path $file -Value $content -NoNewline
    Write-Host "? Successfully applied fix to PlayoffManager.cs!" -ForegroundColor Green
    Write-Host ""
    Write-Host "The fix has been applied. The code will now:" -ForegroundColor Cyan
    Write-Host "  - Use gsp.redTeamName and gsp.yellowTeamName to find opponent" -ForegroundColor Cyan
    Write-Host "  - Add error handling to prevent crashes" -ForegroundColor Cyan
    Write-Host "  - Gracefully handle cases where opponent can't be found" -ForegroundColor Cyan
}
else
{
    Write-Host "? Could not find the exact code to replace" -ForegroundColor Red
    Write-Host "The file may have already been modified or has different formatting" -ForegroundColor Yellow
}
