# Hammer Time: launch list

Last updated Nov 27 2026. Tick things off here; detail lives in `SESSION_HANDOFF.md`, `AUDIT.md` and the design docs.

## 0. Blockers to decide or fix first

- [ ] **Ice features only exist in development builds.** Venue Ice, Adjustable Line (the ICE button) and AI Reads Ice sit behind tester checkboxes. A release build has none of them. Decide which ship, and move those into the game proper.
- [ ] **Win-loss records and standings are trustworthy** (section 1).

## 1. Career truth: records, standings, fields

- [ ] Check every way a result is recorded: played, simulated, round robin, Page playoff, single knockout, quit and resume. (Step 2, Nov 27: results are now copied back into the league, history is matched by team id, and the tour list shares the league's teams. Needs play testing; single knockout not checked.)
- [ ] A playoff game counted twice: fixed for the Page playoff (Nov 27, found by reading the code; confirm by playing). Single knockout still to check.
- [x] Standings show season wins and losses (the tour table used a record that was never updated).
- [ ] Event records are stored by borrowing the season win-loss fields and adding the old totals back at the end. Confirm this survives quitting mid-event.
- [ ] Provincial standings (ranked by earnings) and tour standings (points) are accurate after each event.
- [ ] Simulated results are believable (stronger teams win more; the player's team is rated on the same scale as the others).
- [ ] **Standings in the profile** (built Nov 27, check the layout): tap the player's name, then Standings; tap the name again to go back.
- [ ] Only teams in the player's events gain wins and money; the rest of the league stands still.
- [ ] Sponsor bonuses count in the profile earnings but not on the money list.
- [ ] Profile rank display may read the wrong list and fail to open (`AUDIT.md` A-11).
- [ ] **Right teams in each event.** Today, as the code reads: tour events take the first teams in the tour list, so the same field each time; everything else, championships included, draws at random from the whole league.
    - **The World Championship** (16 teams): the year's top money earners. (User's rule.)
    - **The Tour Championship** (6 teams): the year's top tour points. (User's rule.)
    - Club nights: lower-ranked teams. Local tournaments: a mix. Qualifiers: teams not yet qualified. World Tour and international events: the best. (Suggested; user agreed Nov 27.)
- [ ] **Bug:** missing out on the money cut closes the Tour Championship instead of the World Championship (`TournySelector.SetActiveTournies`, `if (champQual) ... else tourChampionship.complete = true`, in both copies).
- Confirmed Nov 27: the Tour Championship also requires the qualifier. `provQual` in code is really the **tour** qualifier.
- [ ] Rename `provQual` to match.
- [ ] **Entry requirements** (under discussion): some events open only to teams above an earnings or rank mark; the same rule picks the opponents.
- [ ] Team strength grows sensibly across a season and matches event level.

## 2. Features to build

- [ ] **Club nights** (built, uncommitted): play test, add the user's list, decide on the three existing ones. Remove the unused cash-game code and `Tourny_Home_2` afterwards.
- [ ] **A club night every week** at your club (built Nov 27, play test): you and 3 clubmates, from week 2. When a week has three other events, the third is held over.
- [x] No club night in championship weeks, unless none of the championships is affordable (decided Nov 27).
- [ ] **Club path** (decided Nov 27): at career start pick a club (or type a hometown) and a nationality. Club wins qualify you for the Club Championship mid-season, then Nationals, then the World Championship.
    - Five championships (confirmed Nov 27): Club (your hometown club), National (best in your country), World (the national champions), Players' (top money from all over), Tour (best after the tour circuit).
    - Fields (Nov 27): Club 4, National 12, Players' 8, Tour 6, World to confirm (we have 14 countries). Order after the regular season: National, Tour, Players', World; Club is mid-season.
    - Missing a championship does not end the season: it is played out without the player (needs event simulation). Missing the Club Championship means no Nationals or Worlds that season.
    - [ ] Set a week-by-week calendar; recycle trophy art for the new championships.
    - Your club has 8 other teams. Countries: Canada, United States, Japan, South Korea, Norway, Switzerland, Scotland, Denmark, Sweden, Finland, China, England, Italy, Germany (14). **World Championship: 8 teams** (assumed: you plus the 7 strongest other national teams).
    - Names: `TEAM_NAMES.csv` (118 surnames, approved Nov 27) and `CHARACTERS.csv` (the 40 characters and their countries, reviewed Nov 27). Neither is in the game yet; the renamed characters need swapping in too. Show the user every name list before it is published.
    - Clubmates: 8 characters eligible for the home country (that country's own first); fill the rest with local surnames. There is no "anywhere" group. Characters are the rivals and tour faces. League stays at 32 unless the user asks for 48.
    - Proposed 32 teams: 16 home (9 in your club, 7 champions of other clubs), 16 foreign (at least one per country). Tour 16: 6 home, 10 foreign. Foreign championships are not simulated.
    - [ ] **Step 1, league** (committed Nov 27): new careers get 32 teams with a country, club and tour place; saved with the career. Older careers keep their old league. `Career_Menu` has the hometown field, a flag slider for nationality and the colour slider connected (flags in `Assets/Resources/Flags`). Play test.
    - [ ] Until a way onto the tour is built, the player counts as one of its 16 teams.
    - [ ] **Hometown is the club** (decided Nov 27): the player types it or uses the generator, which fills name, team name and hometown (not colour or country). The other 7 clubs come from the same town list.
    - [x] `TOWNS.csv` (8 per country, approved Nov 27) is in the game; the generator picks a town from the chosen country. The club audit's suggestions are applied.
    - [ ] The World Championship and season endings need rework.
- [ ] **Practice session** before an event: optional, about four rocks, ICE button after each.
- [ ] **Daily puzzle:** set situations, one shot first, aim line with hit arrows by default, per-puzzle requirements, solvability check, attempts and streak.
- [ ] **Career intro:** alternate takes the injured skip's two rocks in last year's final; a miss still moves on; skippable. Remove the old tutorial. Teach guards and sweeping with tips in the first event.
- [ ] **Main menu:** Career, Daily Puzzle, Options.
- [ ] **Skins games** on the calendar: hammer needs two, the other team steals, otherwise carry over.
- [ ] **Game Center awards:** pick them, create them in App Store Connect, add the code.

## 3. Ice

- [ ] ICE button shows when it should on a phone: after your shot, before it, during the other team's turn; never blocks a throw or a sweep.
- [ ] The menu and "last time here" dialog fit every screen size.
- [ ] Reads carry across ends, games, seasons and quit-and-resume.
- [ ] AI Reads Ice play test: it should miss early and settle down.
- [ ] Decide: how much ice reading matters; sliders or one-tap "faster / slower, more / less curl" after a shot.
- [ ] Later: ice-reading skill; worn paths.

## 4. Built, needs a play test

- [ ] Timing readout: unswept throws land inside the expected ranges.
- [ ] Hit arrows and warning line; pull-back circles; aim fuzz look on a phone.
- [ ] Training, cohesion, consistency, gear and money-only sponsors over a career.
- [ ] Sweep fatigue; AI sweeper restraint on guards; the 10% sweep boost.
- [ ] Delivery, shooter animation, camera, AI backswing, Game Pace on all three settings.
- [ ] AI takeouts and frozen rocks under the new collisions.
- [ ] Free Guard Zone: chain hits and end-of-end cases.
- [ ] Equipment shop over several weeks (owned items and colours stay).
- [ ] Full EditMode test run inside Unity.

## 5. Decisions waiting

- [ ] Sweep effect only while sweeping, and tap window scaled with pace.
- [ ] Rate the player's team for simulations from the same skill measure the matches use.
- [ ] Tune training and gear prices against weekly income.

## 6. Open findings from the earlier audit

From `AUDIT.md`; not re-checked since the physics rebuild.

- [ ] A-3: negative cash can soft-lock the career screen (dialogue error on game over).
- [ ] B-3: "New career" has no confirmation.
- [ ] S-2: waits in the game loop with no timeout can softlock.
- [ ] S-5: Quick Test ships in release builds.
- [ ] S-6: the high score file can be wiped.
- [ ] S-7, S-11, S-12: teardown errors, per-frame lookups, unchecked lookups.
- [ ] A-6, A-10: calendar progression is fragile; sponsor list may run out in long careers.
- [ ] L-1: a full save every turn (check for hitches on older phones).

## 7. Polish and art

- [ ] Final Tactics and On-Ice training icons; tier art.
- [ ] Broom animations by equipment tier (setup half done).
- [ ] Real-time slide animation needs more frames.

## 8. Release prep (suggested)

- [ ] Remove dead scenes and code: Triple Knockout, old tutorial, cash games, hidden legacy buttons, unused ads package.
- [ ] Release build on a real phone with no tester options: new career to season end.
- [ ] Saves from the current App Store version still load after updating.
- [ ] Performance, battery and app size.
- [ ] Version number, release notes, screenshots, privacy answers.
- [ ] Push to GitHub.

## 9. The audits

Each one: read the code, write down findings, fix, then confirm by playing.

1. [ ] Career records, standings and event fields
2. [ ] Money: fees, prizes, sponsors, gear, game over
3. [ ] Saving: quit and resume at every screen; updating from the old version
4. [ ] Match rules and scoring: hammer, extra ends, Free Guard Zone
5. [ ] AI and sweeping
6. [ ] Every screen on a small phone, a large phone and a tablet
7. [ ] Release build and App Store readiness

## Parked

Tutorial branching and menu tutorials (likely replaced by the career intro), a separate AI "tap" shot, flick shot mode (hidden).
