# Row14.2 DPO Rows Only - No Training

Status: `ROW14_2_DPO_ROWS_PASS`

This packet creates Row14.2 DPO row evidence only. It does not train, continue-train, mutate checkpoints, update locators, promote, run UI, or perform git actions.

## Scope

- Target machine: `SkillSelectionMachine`
- Target adapter: `SkillSelectionMachine.PromptContextTrainingAdapter`
- Target skill: `client-adaptive-learning-loop`
- Row count: 2 usable DPO rows
- Intended future use: Row14.3 can use these rows for continue-training from existing checkpoint after Row14.2 review.
- Current flags: `trainingPerformed=false`, `checkpointMutation=false`, `locatorUpdate=false`, `promotion=false`, `uiRun=false`

## Source Boundary

The rows are `source-disjoint` from Row13 held-out/proxy rows. They do not copy Row13 held-out or proxy prompt text, row ids, paths, filenames, source refs, pair ids, or candidate order.

Row13 target skills avoided as Row14.2 targets:

- `e2e-test-matrix`
- `no-product-decisions`
- `output-skepticism`
- `self-verification`

The Row14.2 rows are `universal-only`: they teach the general SkillSelection boundary for adaptive lifecycle continue-training from an existing saved state. They do not encode host UI, repository path, vendor behavior, product readiness, or locator mechanics.

## Row Design

| Row id | Chosen | Rejected near miss | Quarantine decision |
|---|---|---|---|
| `ROW14_2_TRAIN_CLIENT_ADAPTIVE_LEARNING_LOOP_001` | `client-adaptive-learning-loop` | `planning-dpo-authoring` | keep for Row14.2 review then Row14.3 continuation input; `quarantine=false` |
| `ROW14_2_TRAIN_CLIENT_ADAPTIVE_LEARNING_LOOP_002` | `client-adaptive-learning-loop` | `ai-model-selection` | keep for Row14.2 review then Row14.3 continuation input; `quarantine=false` |

Each row includes chosen/rejected decision objects, available skills, prompt/task text, source-disjoint note, universal-only rationale, no vendor output note, and quarantine decision.

## Held-Out Probe

Planned Row14.3 held-out probe: `ROW14_3_HELDOUT_CLIENT_ADAPTIVE_LEARNING_LOOP_CONTINUE_001`.

The held-out probe is not copied into the JSONL training rows. It remains eval-only planning text in the summary JSON so Row14.3 can test whether continue-training added a new scenario without retraining from scratch.

## Vendor And Privacy

`no vendor output`: the rows are local synthetic evidence authored from project skill metadata. They contain no copied or distilled vendor output, no external AI answer, and no raw private prompt text.

## Next

Next smallest work order: Row14.2 review, then Row14.3 continue-training/export.
