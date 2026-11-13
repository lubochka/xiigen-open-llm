# Q5 Pre-Commit Review/Fix Cycle 2 of 4

Status: FAIL

Created at UTC: 2026-06-10T22:27:44.7354281Z

Branch: `codex/chat-gpt-5.5`

Scope: second independent pre-commit review/fix cycle for accepted Q5 SkillSelection changes only. No staging, commit, push, broad tests, host/plugin/runtime work, dashboard/API work, Q6/Q7/Q8 work, external AI/tools, or model training was run.

## Review Focus

- Trainable evidence, Direct Preference Optimization data quality, no-hidden-determinism, and hash/reference consistency.
- Whether Q5 PASS depends mainly on trainable SkillSelection state or on production code gating.
- Whether source-disjoint rows support dashboard/user-journey relationships without held-out answer-key leakage.
- Whether candidate-position and first-position controls remain degraded.
- Whether final claims keep `semantic_quality_claimed=false`.

## Finding

### F1 - Deterministic UI allow/deny gate is required to keep held-out negative rows passing

`43.SmartSkillsDomain/SkillSelectionMachine.cs` now contains `IsPromptContextCandidateAllowed`, which special-cases `ui-testing` and `user-journey-testing` with prompt/host keyword checks. The checkpoint does include source-disjoint prompt-context rows for dashboard/user-journey behavior, but a static simulation of the same prompt-context scoring showed that the production gate removes extra `ui-testing` selections from held-out rows:

- `ss-heldout-003-external-tool-ban`: without the gate, `ui-testing` is selected in addition to the expected skills.
- `ss-heldout-008-over-select-all-control`: without the gate, `ui-testing` is selected in addition to the expected skills.
- `ss-heldout-012-implementation-reality`: without the gate, `ui-testing` is selected in addition to the expected skills.

That means the current Q5 held-out PASS depends partly on a hardcoded production skill gate instead of learned negative/rejection behavior. This violates the cycle-2 no-hidden-determinism focus. I did not apply a code/model repair in this review slice because the honest repair is to teach or encode negative/rejection evidence in the trainable state and then remove or downgrade the hardcoded production gate; a quick report-only or claim-only edit would hide the same problem.

## Evidence

- Branch: `codex/chat-gpt-5.5`.
- Full files read before review/edit: `AGENTS.md`, `CLAUDE.md`, Q5 plan sections, `STATE.json`, cycle 1 report, `SkillSelectionMachine.cs`, model/training manifests, checkpoint, corpus, held-out set, Q5 reports/artifacts.
- Focused Q5 probe report remains PASS as currently implemented: 24/24 exact selected matches, 24/24 expected rejected present, 12/12 permutation-stable rows, 0/24 first-position baseline exact matches, 0/24 checkpoint-position baseline exact matches, `semantic_quality_claimed=false`.
- Candidate-position controls remain degraded: first-position baseline 0/24 and checkpoint-position baseline 0/24.
- Leakage/source-disjoint check: held-out rows 12; held-out `training_allowed=true` rows 0; eligible corpus/held-out overlap 0; eligible supplemental/held-out overlap 0; historical supplemental held-out answer-key overlaps remain 12 ineligible audit-only rows.
- Source-disjoint dashboard/user-journey bridge rows exist in eligible corpus: `skill-selection-q5-04r-dashboard-journey-bridge-001` and `skill-selection-q5-04r-dashboard-journey-bridge-002`.
- Prompt-context checkpoint profiles for `ui-testing` and `user-journey-testing` include those bridge rows as `IPromptContextTraining` source pairs.
- Hash/reference consistency check passed for current checkpoint, manifest, runner report, model index, corpus pointer, corpus snapshot, and training manifest:
  - checkpoint `190cd688cdb248cc4b711b4e490fa35ae2c9b2c21ec45254c0859ecb1f8a4998`
  - model manifest `31dc4f413acebd725aecc433abc7fe047bde2c15cda27c3fc8e06784f106674f`
  - runner report `93d05a748187e66e31a252c138ddf9be406d56cd195bcc8c59318b68eccbdadb`
  - model index `0661b233b4a449d7427ca9cf6d00625a8fb91f681027722d51c3070be1869119`
  - corpus snapshot `31c83e19c22d5201717ca60b324494c79e842f6d1039bc08884d28b5dd90f413`
  - corpus pointer `f349c37694aefc9ff6ad96f1efbb812d12141cf63de0f1b0506241c6d0da950c`
  - training manifest `a63fb2601c0a8077a5d052f4563910b21e2b8abbed5b36ce4401eccc3d888fc1`
- Targeted overclaim check: reviewed Q5/model/data artifacts keep `semantic_quality_claimed=false`; no host/runtime/dashboard/API completion claim was accepted.

## Fixes Applied

No production code, model, or training-data repair was applied in cycle 2. This is a failing pre-commit review with an actionable repair boundary.

## Exact Next Action

Run `Q5_PROMPT_CONTEXT_NEGATIVE_REJECTION_REPAIR` before cycle 3: repair SkillSelection prompt-context training/model behavior so held-out negative rows do not need hardcoded `ui-testing` / `user-journey-testing` production allow/deny logic, then rerun the focused candidate-position probe and rerun Q5 pre-commit review cycle 2. Do not stage, commit, push, run broad tests, or start Q6/Q7/Q8.
