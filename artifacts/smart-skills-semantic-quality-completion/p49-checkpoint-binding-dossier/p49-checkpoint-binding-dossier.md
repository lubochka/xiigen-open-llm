# P49 - checkpoint target/rollback/source-license binding dossier

Статус: non-mutating binding dossier. Этот документ ничего не разрешает запускать или переписывать. Он только фиксирует, какие текущие checkpoint-кандидаты существуют в репозитории, что именно может быть названо rollback-кандидатом в будущем, какие target-поля остаются `INACTIVE/NONE`, и какие source/license решения всё ещё требуют свежего явного одобрения Luba.

## Граница P49

В P49 не запускались training, eval runner, checkpoint rewrite/mutation, host runtime, external AI/network, git stage/commit/push/switch. Production code и tests не редактировались. Проверка P49 может быть только статической: чтение локальных файлов, сверка путей/sha256, JSON/JSONL parse.

До P49 P48 уже нашёл текущие asset-кандидаты, но оставил target/rollback refs как небезопасные для именования без отдельной привязки. После P49 смысл такой: текущие checkpoint-файлы можно точно указать как `current_asset_candidate` и как возможный `rollback_candidate_ref`, но они не становятся active rollback/target до свежего approval, где Luba выбирает точную машину и операцию.

## Точный список машин

Будущий approval packet может говорить только о следующем машинном списке, без скрытого расширения:

- `PromptToTaskMachine`
- `OrchestrationReasoningMachine`
- `SkillSelectionMachine`

Любая будущая операция обязана явно выбрать одну или несколько машин из этого списка и назвать operation type. Отсутствие выбора означает `target_ref=INACTIVE/NONE` и `rollback_ref=INACTIVE/NONE`.

## Текущие asset-кандидаты

| Machine | Current checkpoint candidate | Manifest | Locator ref | Checkpoint sha256 | Manifest sha256 | Semantic quality claim |
|---|---|---|---|---|---|---|
| `PromptToTaskMachine` | `models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json` | `models/prompt-to-task/prompt-to-task-v1/manifest.json` | `models/index.json#model_id=prompt-to-task-v1` | `7ccaeaabc3724d92cefde4a435053bf178a1540d69566609aab25842ca47e7db` | `ff1bd46e21725a578b2e080c3e6f725d31934d3582f99a0f8b19fb79d77637ab` | `false` |
| `OrchestrationReasoningMachine` | `models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json` | `models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json` | `models/index.json#model_id=orchestration-reasoning-v1` | `dcd7d02b3f8fb9302123995bbf2780f9b5b00d653c8fef6ebba3084f7360d254` | `3e70931d3f313c71c65aa951de326d04442c017dbad2702673881a0023a57479` | `false` |
| `SkillSelectionMachine` | `models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json` | `models/skill-selection/skill-selection-v1/manifest.json` | `models/index.json#model_id=skill-selection-v1` | `737a5fad7c82851728a529dfc5890c22b4154cede7e639403a9e135d821e56ef` | `7a9e517f5d821a47030f7817c8d1abde83261785b378d5789e8f47b3a16eb0fe` | `false` |

Общий locator file: `models/index.json`, sha256 `aed50c43f15d854dee1caccbb99505b81a99b9003bfffff4a5b98fa006189595`.

## Target semantics

Target ref сейчас остаётся `INACTIVE/NONE` для всех трёх машин. P49 не выбирает target checkpoint и не даёт разрешение на rewrite.

Если будущий approval разрешит checkpoint mutation для конкретной машины, target должен быть новым staged checkpoint path. Нельзя молча перезаписать текущий repo checkpoint. Минимальный смысл будущего target:

- `target_checkpoint_path`: новый staged path, созданный под будущий approval, не один из текущих current/rollback paths;
- `target_manifest_path`: новый staged manifest или явно будущий manifest update, связанный с target checkpoint;
- `target_locator_ref`: locator update допускается только после будущих fresh-load, continue-training, metrics/ablation gates;
- `created_from_dataset_sha256`: checksum разрешённого dataset/ref пакета;
- `approval_quote`: свежая фраза Luba, явно разрешающая named operation now.

До заполнения этих полей target остаётся `INACTIVE/NONE`.

## Rollback semantics

Текущий repo checkpoint может быть rollback-кандидатом, но только после будущего approval, который выбирает exact machine и operation. P49 не активирует rollback и не меняет locator.

Rollback-кандидаты:

| Machine | Rollback candidate checkpoint | Manifest | Locator ref | Checkpoint sha256 |
|---|---|---|---|---|
| `PromptToTaskMachine` | `models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json` | `models/prompt-to-task/prompt-to-task-v1/manifest.json` | `models/index.json#model_id=prompt-to-task-v1` | `7ccaeaabc3724d92cefde4a435053bf178a1540d69566609aab25842ca47e7db` |
| `OrchestrationReasoningMachine` | `models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json` | `models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json` | `models/index.json#model_id=orchestration-reasoning-v1` | `dcd7d02b3f8fb9302123995bbf2780f9b5b00d653c8fef6ebba3084f7360d254` |
| `SkillSelectionMachine` | `models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json` | `models/skill-selection/skill-selection-v1/manifest.json` | `models/index.json#model_id=skill-selection-v1` | `737a5fad7c82851728a529dfc5890c22b4154cede7e639403a9e135d821e56ef` |

Rollback activation in future must say: exact machine, operation being rolled back, previous locator ref, staged target ref being rejected/restored from, rollback checkpoint sha256, manifest sha256, and post-rollback verification. P49 does not perform any of that.

## OrchestrationReasoning resolver

Для `OrchestrationReasoningMachine` есть два local checkpoint-файла:

- Locator-current: `models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json`, sha256 `dcd7d02b3f8fb9302123995bbf2780f9b5b00d653c8fef6ebba3084f7360d254`.
- Alternate historical candidate: `models/orchestration-reasoning/orchestration-reasoning-v1/checkpoint.json`, sha256 `f66f256156bbc4ba278851f75a2ecd644160adbf676db29118640519e3c9805b`.

`models/index.json` и `models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json` сейчас указывают на `orchestration-reasoning-v1-checkpoint.json`. Поэтому это locator-current candidate. `checkpoint.json` не является current locator target; это historical/alternate candidate, который нельзя silently использовать как target или rollback без будущего явного выбора.

Будущий approval должен выбрать одно из двух:

- использовать locator-current как rollback candidate перед staged mutation; или
- отдельно исследовать/выбрать alternate historical candidate, указав почему он нужен, какой locator будет затронут, и какие проверки доказывают его допустимость.

Если approval молчит о resolver choice, действует безопасное значение: locator-current rollback candidate only, alternate remains evidence-only.

## Source/license binding

P49 связывает статусы, но не меняет разрешения. Common training сейчас не разрешён ни одним из этих артефактов.

| Phase | Current status | Common training allowed now | Что нужно до common reuse/training/eval |
|---|---|---:|---|
| P27 | `p27-skillselection-repair-dataset-candidate`: 8 local-private repair candidate rows; `TRAINING_NOT_AUTHORIZED_IN_P27`; not used for training yet. Candidate rows: `artifacts/smart-skills-semantic-quality-completion/p27-skillselection-repair-dataset-candidate/p27-repair-dataset-candidate.jsonl`, sha256 `e62eb13ecd3268b963f2d81a2883fd37bd985517be2225d90dec4e439c9d38d2`. Dataset card: sha256 `6433774c85110843e7e9bfd14ffec549d636de1b43811823402e52aed7f7e8cf`. | false | Fresh Luba approval for common reuse, license/source-family review, redaction review, and source-disjoint split independent from P24/P25/P26 holdout lineage. |
| P31 | `p31-materialized-dpo-candidates.jsonl`: 12 materialized SkillSelection DPO candidates, all `common_training_allowed=false`, `training_allowed_in_p31=false`, `not_used_for_training_yet=true`; sha256 `3ff9b59ae601e6dee9f1741528f3c49018ea65ff84eca76d5f77a125d1ca88a6`. Dashboard QA scenarios: 6 rows, sha256 `b3c8ae7a27ed53430937716af6d506b0646ec6829f47f4bd959a4f96dc393ad5`. | false | License/source-family approval for each source family, redaction review, anti-leakage review, and future approval that distinguishes train/eval/dashboard-only roles. |
| P36 | `p36-eval-fixtures.jsonl`: 36 eval fixture rows, 12 per machine; `training_allowed=false`, `eval_execution_allowed=false`, `not_executed_yet=true`; sha256 `fa8419a529781cedbfabbb9048553d5fd2833f6b71149b333107c42a0b60250e`. | false | Future eval approval, source-disjoint family review, runner scope approval, and firewall that eval fixture material is not training material. |
| P41 | Source-disjoint pre-eval guard only; current report says fixture_count 36, no train/eval family overlap, `can_proceed_to_eval_runner=false`, `model_or_machine_called=false`; report sha256 `5812cd40a7a808bc48539e3741fec133af331e2ec0de315efb002517b3fb6121`. | false | Future approval to proceed to eval runner, plus guard must still pass after any source/license changes. P41 itself grants no training/eval/checkpoint permission. |
| P45 | Authorization manifest dry-run; `exact_luba_approval_quote=NONE`, `training_authorized_now=false`, `eval_authorized_now=false`, `checkpoint_mutation_authorized_now=false`, `target_checkpoint=NONE`, `rollback_checkpoint=NONE`; manifest sha256 `2729a928706398f82f9fbbb41993c531c1980ea694098fea049c5cd4b659d507`; dry-run report sha256 `a1adcd3ac31e3db96db46eb6b8503a79e4981773e8287850b9f42dd6801b28cb`. | false | Fill source_license_gate, target/rollback refs, metrics plan, and fresh approval. Dry-run denial must remain denial until those fields are real. |
| P47 | Disabled authorization packet; `exact_luba_approval_quote=NONE`; target/rollback refs required but current value is `NONE`; source license gate status `missing_for_training`; packet sha256 `62b2e708871a504430e3130dbc4c8717419368240d5610f7e35b7ae21d2091ac`. | false | Fresh Luba approval, exact operation/machine list, source/license artifact, target staged checkpoint path, rollback candidate ref, metrics thresholds, ablation pass conditions. |

## Approval packet template

This template is deliberately disabled. Concrete fields are filled where P48/P49 knows them; placeholders remain only where fresh Luba approval or future staged output is required.

```json
{
  "approval_quote": "<fresh Luba quote explicitly authorizing named operation now>",
  "operation_type": "<training|eval|checkpoint_mutation|rollback>",
  "machine_selection": ["<PromptToTaskMachine|OrchestrationReasoningMachine|SkillSelectionMachine>"],
  "current_assets": {
    "PromptToTaskMachine": {
      "checkpoint_path": "models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json",
      "manifest_path": "models/prompt-to-task/prompt-to-task-v1/manifest.json",
      "locator_ref": "models/index.json#model_id=prompt-to-task-v1",
      "checkpoint_sha256": "7ccaeaabc3724d92cefde4a435053bf178a1540d69566609aab25842ca47e7db"
    },
    "OrchestrationReasoningMachine": {
      "checkpoint_path": "models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json",
      "manifest_path": "models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json",
      "locator_ref": "models/index.json#model_id=orchestration-reasoning-v1",
      "checkpoint_sha256": "dcd7d02b3f8fb9302123995bbf2780f9b5b00d653c8fef6ebba3084f7360d254",
      "alternate_historical_checkpoint_path": "models/orchestration-reasoning/orchestration-reasoning-v1/checkpoint.json",
      "alternate_historical_checkpoint_sha256": "f66f256156bbc4ba278851f75a2ecd644160adbf676db29118640519e3c9805b"
    },
    "SkillSelectionMachine": {
      "checkpoint_path": "models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json",
      "manifest_path": "models/skill-selection/skill-selection-v1/manifest.json",
      "locator_ref": "models/index.json#model_id=skill-selection-v1",
      "checkpoint_sha256": "737a5fad7c82851728a529dfc5890c22b4154cede7e639403a9e135d821e56ef"
    }
  },
  "target_checkpoint_ref": {
    "current_value": "INACTIVE/NONE",
    "future_required_path": "<new staged checkpoint path, not an overwrite of current repo checkpoint>"
  },
  "rollback_checkpoint_ref": {
    "current_value": "INACTIVE/NONE",
    "future_candidate_must_be_selected_from_current_assets": true
  },
  "source_license_gate": {
    "current_status": "missing_for_training",
    "common_training_allowed_now": false,
    "future_required_artifact": "<source/license/redaction/source-family review artifact>"
  }
}
```

## Forbidden claim firewall

P49 is not semantic quality. P49 is not model readiness. P49 is not trained-model readiness. P49 is not a training run, not an eval run, not a checkpoint mutation, not host runtime evidence, and not a promotion. It only removes hidden architecture ambiguity before a future approval packet.

## Parent review expectation

Parent should review P49 for exactness and parseability. If accepted, parent may perform the repository commit/push boundary for P49 artifacts/state according to parent protocol. No P50 or training/eval/checkpoint action is derived from this work order.
