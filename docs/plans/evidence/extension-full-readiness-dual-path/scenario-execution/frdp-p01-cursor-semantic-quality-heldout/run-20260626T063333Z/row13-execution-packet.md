# Row13 Execution Packet - frdp-p01-cursor-semantic-quality-heldout

Run id: `run-20260626T063333Z`

Generated at UTC: `2026-06-26T06:33:33.2108822Z`

Bounded sub-agent scope: Row13 step 13.1 only. No delegation. No git actions. No production code edits. No live Cursor UI. No checkpoint mutation, promotion, locator update, training, or product-readiness claim.

## Status

`ROW13_1_NEEDS_FIX`

Reason: an existing automated/core held-out comparison candidate was found, but it is not safe to run under this work order because its test writes to `artifacts/smart-skills-semantic-quality-completion/p25-skillselection-runtime-holdout-comparison`, outside the allowed Row13 evidence folder. The narrower scoring guard exists but is not a held-out semantic-quality evaluation, so it cannot satisfy Row13.1.

## Numbered Row13 Steps

1. Row13.1: automated/core held-out semantic-quality evaluation, no UI.
   - Intended proof: run a safe existing automated/core held-out semantic-quality evaluation for Cursor Row13 using universal orchestration/skill selection behavior, not project-stack-specific behavior.
   - Current result: NEEDS_FIX.
   - No automated/core held-out evaluation command was run in this packet.
   - No metrics were produced in this packet.
   - What was inspected: board Row13, plan quality matrix, execution-state references, Phase 03 quality scoring guard output, P24/P25 held-out test code, and eval-only runner contracts.

2. Row13.2: hidden deterministic shortcut audit / no memorization / no candidate-position/source-id leakage.
   - Required later proof: exact prompt/source/path/title/source-id masking, nearest-neighbour check, candidate-order shuffle, pair-id/source-id leakage audit, and no-trained-state or common-baseline control.
   - Current result: not run.

3. Row13.3: real Cursor UI duplicate scenario, same prompt family, proof of visible answer.
   - Required later proof: same prompt family as Row13.1, Cursor signed-in/session preflight, explicit safe UI scenario script, visible answer evidence, and comparison to Path A automated/core output.
   - Current result: not run.

4. Row13.4: review + board update.
   - Required later proof: reviewer packet, board row update, joined-status downgrade or acceptance with exact evidence refs, and no-overclaim locks.
   - Current result: not run.

## Harness Assessment

Candidate found:

`dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~SmartSkillsSemanticQualityP25RuntimeHoldoutComparisonTests" --no-restore --nologo --logger "console;verbosity=normal"`

Decision: not run.

Why it is relevant: the test name and code show it compares common and candidate SkillSelection checkpoints on accepted P24 runtime held-out rows using `SkillSelectionMachine.DecideFromTrainedState`; it records leakage controls and keeps semantic quality, promotion, training, and checkpoint mutation claims false.

Why it is unsafe for this Row13.1 work order: the test hard-codes its output directory under `artifacts/smart-skills-semantic-quality-completion/p25-skillselection-runtime-holdout-comparison`, which is outside the allowed write scope for this packet.

Related setup candidate:

`dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~SmartSkillsSemanticQualityP24RuntimeHoldoutRowsTests" --no-restore --nologo --logger "console;verbosity=normal"`

Decision: not run.

Why: P24 creates accepted redacted runtime held-out rows, but also writes under `artifacts/smart-skills-semantic-quality-completion/p24-skillselection-runtime-holdout-rows`, outside the allowed Row13 folder.

Guard-only candidate:

`dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQualityScoringTests" --no-restore --nologo --logger "console;verbosity=normal"`

Decision: not run.

Why: Phase 03 evidence shows this guard previously passed 9 tests, but it is guard-only, not a held-out semantic-quality evaluation. Rerunning it would not satisfy Row13.1.

Configurable runner candidate:

`ExtensionEvalOnlyQualityRunner`

Decision: not run.

Why: it has configurable input/output directory support, but no existing safe CLI or approved Row13 held-out input JSONL was found during this packet. Creating a new runner or new eval fixture would exceed Row13.1's "run existing safe harness if available" scope.

## Exact Next Narrow Work Order

Create or expose a safe Row13 automated/core harness entry point that:

1. Uses existing P24/P25 held-out SkillSelection comparison logic or an equivalent approved core runner.
2. Accepts an output directory parameter and writes only under `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-semantic-quality-heldout/<new-run-id>/`.
3. Uses only universal-only prompts/rules: universal orchestration and skill-selection behavior, not project-stack-specific behavior.
4. Runs no UI, no training, no checkpoint mutation, no promotion, no locator update, no external AI, and no network.
5. Captures command, exit code, scenario count, common/candidate pass counts, leakage-control results, model file hash before/after, and all false claim-lock fields.
6. Produces `semantic quality improvement not claimed` and keeps token savings, product readiness, checkpoint promotion, locator update, and training readiness false.

## Claims Locked False

- semantic quality improvement not claimed
- token savings not claimed
- product readiness not claimed
- checkpoint promotion not claimed
- locator update not claimed
- training readiness not claimed
- learnability not claimed
- live Cursor UI proof not claimed
- full extension readiness not claimed

## Required Phrase Readback

- Row13.1
- held-out
- hidden deterministic shortcut
- real Cursor UI duplicate
- universal-only
- semantic quality improvement not claimed

## Evidence Boundary

The evidence supports only: Row13 execution packet created, existing held-out harness candidates assessed, and a NEEDS_FIX packet recorded because no safe existing held-out automated/core harness could be run within the allowed write scope.

This packet does not prove semantic quality improvement, token savings, product readiness, checkpoint promotion, locator update, or training readiness.
