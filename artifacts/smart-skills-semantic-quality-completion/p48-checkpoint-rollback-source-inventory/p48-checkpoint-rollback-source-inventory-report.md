# P48 checkpoint / rollback / source inventory

Статус: `SUBAGENT_DONE_PENDING_PARENT_REVIEW`.

Scope: clean-minimal, local-repo inventory only. Training, eval runner, checkpoint rewrite/mutation, host runtime, external AI/network, git stage/commit/push/switch were not run.

## Что уже известно для будущего approval packet

P47/P45 уже называют обязательные поля будущей авторизации:

- `exact_luba_approval_quote`: сейчас `NONE`; для будущего training/eval/checkpoint mutation нужен свежий явный approval.
- `training_authorized_now`: сейчас `false`.
- `eval_authorized_now`: сейчас `false`.
- `checkpoint_mutation_authorized_now`: сейчас `false`.
- `candidate_dataset_refs`: P31 materialized candidates, 12 rows, sha256 `3ff9b59ae601e6dee9f1741528f3c49018ea65ff84eca76d5f77a125d1ca88a6`; P27 dataset card, 8 rows, sha256 `6433774c85110843e7e9bfd14ffec549d636de1b43811823402e52aed7f7e8cf`.
- `heldout_refs`: P36 eval fixtures, 36 rows, sha256 `fa8419a529781cedbfabbb9048553d5fd2833f6b71149b333107c42a0b60250e`; P31 dashboard scenarios, 6 rows, sha256 `b3c8ae7a27ed53430937716af6d506b0646ec6829f47f4bd959a4f96dc393ad5`.
- `preflight_refs`: P41 source-disjoint guard exists, P36 fixture count is 36 with 12 rows per machine, but it is parse/consistency guard only and not eval execution.
- `metrics_plan` and `ablation_plan`: required before semantic-quality/model-readiness claims; not executed.
- `rollback_plan`: required before mutation; design-only so far.

## Existing model / locator candidates found

PromptToTaskMachine:

- Current repo model checkpoint candidate: `models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json`.
- Manifest: `models/prompt-to-task/prompt-to-task-v1/manifest.json`.
- Checkpoint sha256: `7ccaeaabc3724d92cefde4a435053bf178a1540d69566609aab25842ca47e7db`.
- Manifest sha256: `ff1bd46e21725a578b2e080c3e6f725d31934d3582f99a0f8b19fb79d77637ab`.
- Locator: `models/index.json` entry `prompt-to-task-v1`.
- Training refs already recorded in manifest: 12 rows total, 8 train, 4 held-out, `training_allowed=false` in split/report evidence; `semanticQualityClaimed=false`.

OrchestrationReasoningMachine:

- Current repo model checkpoint candidate: `models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json`.
- Manifest: `models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json`.
- Checkpoint sha256: `dcd7d02b3f8fb9302123995bbf2780f9b5b00d653c8fef6ebba3084f7360d254`.
- Manifest sha256: `3e70931d3f313c71c65aa951de326d04442c017dbad2702673881a0023a57479`.
- Locator: `models/index.json` entry `orchestration-reasoning-v1`.
- Important ambiguity: `models/orchestration-reasoning/orchestration-reasoning-v1/checkpoint.json` also exists with sha256 `f66f256156bbc4ba278851f75a2ecd644160adbf676db29118640519e3c9805b`, and older OR-04B locator reports refer to it. It must not be silently treated as rollback or target without a resolver artifact.

SkillSelectionMachine:

- Current repo model checkpoint candidate: `models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json`.
- Manifest: `models/skill-selection/skill-selection-v1/manifest.json`.
- Checkpoint sha256: `737a5fad7c82851728a529dfc5890c22b4154cede7e639403a9e135d821e56ef`.
- Manifest sha256: `7a9e517f5d821a47030f7817c8d1abde83261785b378d5789e8f47b3a16eb0fe`.
- Locator: `models/index.json` entry `skill-selection-v1`.
- Manifest says `semantic_quality_claimed=false`, `host_runtime_claimed=false`, `adaptive_user_model_claimed=false`, `external_ai_used=false`, `exact_prompt_leakage=0`, `route_mode=candidate_position_scaffold_not_final_semantic_authority`.

Common locator:

- `models/index.json`, sha256 `aed50c43f15d854dee1caccbb99505b81a99b9003bfffff4a5b98fa006189595`, contains locator entries for PromptToTask, OrchestrationReasoning, SkillSelection and unrelated models.

## Source / license gate inventory

P27:

- `p27-repair-dataset-candidate.jsonl`: 8 rows, sha256 `e62eb13ecd3268b963f2d81a2883fd37bd985517be2225d90dec4e439c9d38d2`.
- `p27-source-disjoint-quarantine-ledger.jsonl`: 8 rows, sha256 `f727830e64ec1511854dae0afb5c3e1fd86495e191b7670de75e960c0ef60939`.
- Gate status: `commonTrainingAllowed=false`, `candidateTrainingAllowed=false`, `notUsedForTrainingYet=true`.
- License/source status: rows are local-private holdout-derived repair candidates and require explicit Luba approval before common reuse, plus license/source-family review, redaction review, and an independent source-disjoint split.

P31:

- `p31-materialized-dpo-candidates.jsonl`: 12 rows, sha256 `3ff9b59ae601e6dee9f1741528f3c49018ea65ff84eca76d5f77a125d1ca88a6`.
- `p31-source-disjoint-split-manifest.json`: rowCount 12, `sourceFamiliesAreDisjoint=true`, `commonTrainingAllowed=false`, design only.
- `p31-dashboard-qa-scenarios.jsonl`: 6 rows, sha256 `b3c8ae7a27ed53430937716af6d506b0646ec6829f47f4bd959a4f96dc393ad5`.

P36 / P41:

- `p36-eval-fixtures.jsonl`: 36 rows, sha256 `fa8419a529781cedbfabbb9048553d5fd2833f6b71149b333107c42a0b60250e`.
- P36 counts: 12 PromptToTask, 12 OrchestrationReasoning, 12 SkillSelection.
- P36 status: fixture rows only, `trainingRun=false`, `evalRun=false`, `checkpointMutated=false`, `semanticQualityClaimed=false`.
- P41 status: parse/consistency guard only, no model/machine call, no eval run, no checkpoint mutation.

P44 / P45 / P47:

- P44 says source/license gate is required and target checkpoint readiness is not ready for promotion.
- P45 dry-run denies training/eval/checkpoint mutation because source/license and checkpoint refs are missing.
- P47 repeats the same hard stop: `target_checkpoint_ref=NONE`, `rollback_checkpoint_ref=NONE`, `source_license_gate.current_status=missing_for_training`.

## Missing fields

Still missing before any future training/eval/checkpoint mutation:

- Fresh exact Luba approval quote for the future action.
- Exact operation scope: which machine(s), training vs eval vs checkpoint mutation, and whether common model update is intended.
- `target_checkpoint_ref` per machine: checkpoint path, manifest path, locator ref, sha256, source dataset sha256, and intended write semantics.
- `rollback_checkpoint_ref` per target: previous checkpoint path, manifest path, locator ref, sha256, rollback activation steps, and who/what may activate rollback.
- Source/license gate artifact that classifies every candidate source family with `license_or_approval_source`, `common_training_eligible`, `user_local_only`, `quarantine_reason`, `dataset_checksum`, and `heldout_checksum`.
- Disjoint train/eval binding for the exact future dataset, not just historical split evidence.
- Approval-safe mapping from P27/P31/P36 rows to future train/eval roles.
- OR checkpoint resolver that disambiguates `checkpoint.json` vs `orchestration-reasoning-v1-checkpoint.json`.

## Can target_checkpoint_ref and rollback_checkpoint_ref be safely named now?

No.

Existing repo assets can be named as current checkpoint/manifest/locator candidates, but P48 cannot safely promote them into future `target_checkpoint_ref` or `rollback_checkpoint_ref` because P47 requires an exact future approval packet and P45/P47 explicitly record both fields as `NONE`. Naming them now would silently choose a mutation target and rollback baseline without current authorization, without per-machine operation scope, and without source/license gate closure.

For SkillSelection, the current published asset is a plausible rollback candidate only after a future parent-owned binding artifact says the intended target is SkillSelection common-model mutation and accepts `models/skill-selection/skill-selection-v1/*` as rollback. For PromptToTask and OrchestrationReasoning the same rule applies. For OrchestrationReasoning there is also an extra ambiguity because two checkpoint files exist under the same model directory.

## Next non-mutating artifact required

The next artifact should be a parent-owned checkpoint target/rollback/source-license binding dossier. It should not run training/eval/checkpoint mutation. It should bind:

- exact machine list;
- exact current asset candidate per machine;
- exact target vs rollback semantics;
- source/license gate verdict for P27/P31/P36 and any training-data refs;
- OR checkpoint filename resolver;
- explicit statement that target/rollback fields remain inactive until fresh Luba approval.

