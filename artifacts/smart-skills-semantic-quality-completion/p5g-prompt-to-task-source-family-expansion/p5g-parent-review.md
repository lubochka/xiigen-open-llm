# P5G parent review - PromptToTask source-family expansion

## Verdict

PASS_AFTER_REPAIR_NOT_TRAINING_READY.

P5G is accepted only as PromptToTask source-family expansion for later DPO authoring review. It is not a final DPO dataset, not a training set, not an eval set, and not semantic-quality evidence.

## Review pass 1 - candidate coverage

Checked 8 PromptToTask source-family rows.

Result:
- 6 rows are `candidate_for_authoring_review`;
- 1 row is `evidence_only`;
- 1 row is `quarantine`;
- all rows keep `training_ready=false`;
- the six accepted candidates are independent from the two existing P5E PromptToTask families.

Accepted scope: PromptToTask now has enough candidate-family width for a later authoring/split phase, after parent review and DPO materialization.

## Review pass 2 - required future DPO shape

Checked every row for the fields needed before future DPO authoring.

Result:
- every row has a Russian universal PromptToTask boundary;
- every row states what private/project/stack details were removed;
- every row explains independence from existing P5E PromptToTask families;
- every row has a future chosen shape;
- every row has a future rejected near miss.

Accepted scope: these rows are source-family candidates, not final DPO rows.

## Review pass 3 - shortcut controls and raw-private firewall

Checked required ablation controls and self-check.

Result:
- every row includes controls for exact prompt lookup;
- every row includes filename/path/title shortcut control;
- every row includes keyword-only control;
- every row includes source-family-id shortcut control;
- every row includes nearest-only/retrieval-only control;
- no raw private prompt text is copied.

Accepted scope: future authoring must still build actual DPO rows and run ablation probes later.

## Review pass 4 - stable ids and human readability

Parent review found a Markdown defect: stable status ids were partially translated.

Repair accepted:
- `candidate_for_authoring_review`, `evidence_only`, and `quarantine` are preserved as exact ids;
- human explanations remain in Russian;
- self-check records `stable_machine_ids_preserved_in_markdown=true`.

Accepted scope: the report is readable by a human while preserving machine ids needed by later automation.

## Review pass 5 - no-overclaim lifecycle

Checked P5G self-check and artifacts.

Result:
- `training_allowed=false`;
- `eval_allowed=false`;
- `semantic_quality_claimed=false`;
- `training_ready_rows=0`;
- no tests/build/training/eval ran;
- no code/checkpoint/model/locator changed;
- no commit/push was done by the subagent.

Accepted scope: commit/push is allowed only for P5G artifacts + state + this parent review.

## Next action after phase commit

After P5G is committed and pushed, continue with bounded subagent DPO materialization for PromptToTask from the six accepted source-family candidates. Do not run training/eval in that phase.
