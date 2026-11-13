# Row13.1 Quality Failure Diagnosis

Status: `ROW13_1_DIAGNOSIS_NEEDS_TRAINING`

Generated at UTC: `2026-06-26T06:53:17Z`

Sub-agent: `Row13.1-bounded-subagent`

## Human Meaning

Row13.1 automated/core held-out quality is `0/4` because neither the common model nor the candidate model selected the expected universal-only skill for any of the four accepted P24 held-out rows. This is not product readiness evidence, not a token-savings result, not promotion evidence, and not a semantic-success claim.

## Root Causes

Harness bug:

- Causal for 0/4: no.
- Finding: the existing wrapper command output says `ROW13_1_FIX_PASS` even though `commonPassCount=0` and `candidatePassCount=0`. That is a reporting/status wording defect. It did not cause the selector decisions.

Trained state missing or not loaded:

- Causal for candidate: yes.
- Finding: the candidate manifest and checkpoint exist, and `SkillSelectionMachine.LoadCheckpointAsync` validates the checkpoint. The candidate has trained metadata, but it has no applicable profile coverage for the four expected held-out skill ids and `candidatePositionWeights` is empty. Runtime therefore returns no selected skills with `NO_TRAINED_STATE_AVAILABLE`.

Common model held-out failure:

- Causal for common: yes.
- Finding: common produces decisions but selects wrong alternatives on all four rows:
  - expected `e2e-test-matrix`, selected `implementation-integrity`, `test-integrity`, `plan-review`
  - expected `no-product-decisions`, selected `claim-verification`
  - expected `output-skepticism`, selected `verification-before-completion`, `plan-review`
  - expected `self-verification`, selected `test-integrity`

Expected scenarios too strict or invalid:

- Invalid scenario found: no.
- Finding: the P24 rows are marked `runtimeInvocationReady`, `notUsedForTraining`, `commonTrainingAllowed=false`, source/pair disjoint, universal-only, and leakage controls blocked. They are strict because current checkpoints do not contain or learn the expected held-out skill ids, but the inspected metadata does not support invalidating them.

Deterministic shortcut or leakage:

- Shortcut leakage found: no in the inspected Row13 evidence.
- Finding: the existing report shows exact prompt, source id, path, filename, candidate order, and pair id authority blocked for all four rows, with candidate-order invariance for both common and candidate.

## Coverage Finding

Expected held-out skill ids:

- `e2e-test-matrix`
- `no-product-decisions`
- `output-skepticism`
- `self-verification`

The inspected common and candidate checkpoints do not contain those expected ids in exact hyphenated form or simple underscore form. The candidate checkpoint has only 5 `skillSelection` profiles and 2 `promptContext` profiles, with 19 trained pair ids and 17 trained prompt keys.

## Boundary

No training was performed. No checkpoint was mutated. No locator or promotion state was changed. No live Cursor UI was run. No git action was performed.

No code or harness fix was applied because a local code/wrapper patch cannot honestly make the current checkpoints select held-out skills that are absent from trained semantic profile coverage. The wrapper status wording should be cleaned up in a later harness-reporting work item, but the Row13.1 quality failure itself needs new trained state.

## Exact Next Training Work Order

Requested artifact type: training/DPO candidate state plus new held-out evidence.

Fresh Luba approval is required before any training, checkpoint write, locator change, promotion, commit, or push.

Allowed writes when approved:

- a new bounded training artifact folder
- a new candidate checkpoint/export artifact folder
- a new Row13.1 evidence folder under `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-semantic-quality-heldout/`

Forbidden writes:

- do not mutate `models/skill-selection/skill-selection-v1`
- do not overwrite user/common models
- do not update locators or promotion state
- do not train on the P24 held-out rows themselves

Tasks:

1. Build an approved, source-disjoint training/DPO set that teaches canonical hyphenated skill ids for `e2e-test-matrix`, `no-product-decisions`, `output-skepticism`, and `self-verification`, with hard negatives matching the Row13.1 alternatives.
2. Train or continue-train the SkillSelection semantic units/final-selection state using only approved training rows and preserve P24 rows as held-out evaluation only.
3. Validate that exported candidate state contains canonical hyphenated skill ids for the target universal-only skills before held-out evaluation.
4. Rerun Row13.1 automated/core held-out evaluation into a new evidence folder and parse all generated JSON.
5. Report `heldOutCount`, `commonPassCount`, `candidatePassCount`, leakage-control counts, and model/checkpoint hash before/after with no readiness, token-savings, promotion, or semantic-success overclaim.

Target gate: `candidatePassCount` should reach `4/4` on the accepted P24 held-out rows before any quality success claim is considered; common baseline should remain separately reported.
