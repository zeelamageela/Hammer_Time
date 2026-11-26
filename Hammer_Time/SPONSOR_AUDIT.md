# Sponsor system audit (first pass, Nov 25 2026)

**Acted on (Nov 25):** sponsors are money only (stat bonuses, opponent modifiers and their sliders no longer used), one availability rule per tier,
result bonuses wired, contracts count down when a tournament is played. The current rules are in `Assets/Scripts/Tourny/SponsorRules.cs`; what follows
describes the code before that change.

Read-only look at the code. Nothing here was run, so anything marked "check" is a question for the game, not a finding. Decision so far: **scrap the
sponsor stat bonuses**; keep the system under review for when and where sponsors appear.

## Where it lives

| Piece | File | What it does |
|---|---|---|
| Card data | `Tourny/Card_Sponsor.cs`, `Tourny/Card.cs`, `Tourny/PowerUpList.cs` | One sponsor: name, id, `cost` (a weekly amount, see below), `duration` in weeks (50+ = permanent), six stat bonuses (`draw/guard/takeOut/sweepStrength/sweepEnduro/sweepCohesion`), six opponent modifiers (`opp*`), a `signCondition` + `signConditionValue` (when it becomes available), a `bonusCondition` + `bonus` (a cash bonus), `bonusStart`. Three pools: local, national, international. |
| Logic + screen | `Tourny/SponsorManager.cs` (875 lines) | `SetUp()` shuffles the pools, restores saved ids, decides which cards are active / played / available, draws the four active slots and the replace panel; `BuyCard` swaps a card in and changes `costPerWeek`; `PreviewPoints` / `UnPreviewPoints` add or remove a card's stats from `cm.modStats` and `cm.oppStats`. |
| Save | `Tourny/CareerManager.cs` | `cardSponsorIDList`, `activeCardIDList`, `activeCardLengthList`, `playedCardIDList` (saved and loaded); active card durations are counted down each week; `pUpM.SetUp()` runs after a load. |
| Weekly money | `TeamMenu.cs` | `cm.costPerWeek` = player wages + the sponsors' `pm.costPerWeek`. |
| Hub | `Tourny/TournySelector.cs`, `Card_Select.cs`, `TeamMenu.cs` | The Sponsors panel and the team screen's use of `PreviewPoints`. |

## Findings

1. **Sponsor stat bonuses do nothing in play.** `PreviewPoints` only changes `cm.modStats`, which is a Team-screen display total (see TRAINING_DESIGN.md,
   "What each stat actually does"). `SkillBonus` (gear + training) does not include it. So the six bonus sliders on the sponsor cards are decoration today.
   Scrapping them costs nothing in play; the sliders and the Team screen totals are the only things to tidy.
2. **Opponent modifiers (`opp*`) might do something.** `TeamManager` adds `gsp.oppStats` to the AI team's sweepers (strength, endurance, cohesion).
   Check where `gsp.oppStats` is filled from (`GameManager` creates a fresh one at line 218; whether the career's `cm.oppStats` is copied in needs
   confirming). If it is, sponsor debuffs on opponents do reach the AI sweepers, and the draw/guard/take-out ones do not (the AI's shot skills come from its
   team stats, not from this). That would make the "opponent" half of the system the only live part.
3. **`modStats` / `oppStats` are running totals that get re-added** every time the hub opens (preview then un-preview), which is why they drift. The
   code comments already say "DON'T call tm.PreviewPoints()". If the stat bonuses are scrapped, `PreviewPoints`/`UnPreviewPoints` and the sliders can go,
   and the equipment side can stop touching `modStats` too (gear now uses `equipmentBonus`).
4. **Availability rules are the interesting part.** `signCondition` is one of `wins`, `earnings`, `provQual`, `week`, `None` (compared against the
   career record, earnings, a qualification flag, or the week). That is the "when and where sponsors come available" you want to tune. Things to check:
   - `signConditionValue` is a string on `Card_Sponsor`, but the comparisons read like numbers: check the real field type on `Card`.
   - The `week` rule is `value < cm.week`, so a sponsor shows up after that week, never on it.
   - `wins` uses `cm.record.x`, a lifetime total; once enough wins are banked every wins-gated sponsor is available for the rest of the career.
   - The three pools (local, national, international) are one shuffled list; there is no rule tying a pool to a tournament level or a place on the map.
5. **Money is the one live effect.** `cost` is applied to `costPerWeek` when a card is bought or replaced, so sponsors act as weekly income (or
   expense). `bonus` / `bonusCondition` / `bonusStart` (a payout for winning games or a title) have no reader in the files I searched: check.
6. **Duration handling has gaps.** A card with `duration > 0` is active; the "ongoing" text branch (`duration > 50`) is unreachable in `SetUp` because
   the earlier `duration > 0` branch catches it first. Expired cards are replaced by `emptySponsorCard` but stay in `playedCards`.
7. **Size and risk.** 875 lines of UI code hand-copying twelve slider values per card into several arrays and panels, repeated four times. Very easy to break.

## If the stat bonuses go

Keep: the card as a *weekly money* and *story* thing (name, image, `cost`, duration, availability rules, bonus payouts if wired).
Remove: the six bonus fields and six `opp*` fields on the data, the sliders on the card UI, `PreviewPoints`/`UnPreviewPoints`, and the sponsors'
writes to `modStats`/`oppStats`.
Decide first whether the opponent modifiers stay (finding 2). They are the only part that might change a match.

## Questions for you

- Do sponsors stay as a money system (weekly income, bonuses for results) with no stats at all?
- What should open the sponsors up: week, wins, a qualification, a place on the map (local, national, international)? One rule per pool would be simpler
  than the current mix.
- Do you want opponent debuffs from sponsors, or none?
