# P35. Дизайн checkpoint rewrite manifest перед любым изменением checkpoint

Статус: это только дизайн manifest. В P35 checkpoint не переписывался, model files не трогались, fresh-load и continue-training не запускались.

## Зачем нужен manifest

Checkpoint нельзя переписывать просто потому, что появились DPO rows или artifact validation. Перед mutation нужен manifest, который объясняет:

- что именно будет переписано;
- какие source rows и checksums использованы;
- какие gates уже прошли;
- как откатиться;
- как загрузить user/cache state первым;
- как fallback-load common repo checkpoint;
- как export/import/share работает для пользовательской ноги;
- как continue-training продолжает существующий checkpoint, а не начинает с нуля.

## Простая UML-карта для draw.io

Импорт в draw.io: Arrange -> Insert -> Advanced -> Mermaid.

```mermaid
classDiagram
    class "User adaptive checkpoint" {
      +user_cache_path
      +backup_path
      +export_package
      +share_package
    }
    class "Repo common checkpoint" {
      +repo_checkpoint_path
      +manifest_ref
      +locator_ref
      +upgrade_version
    }
    class "Checkpoint rewrite manifest" {
      +manifest_id
      +machine
      +source_pointers
      +training_dataset_checksum
      +eval_fixture_checksum
      +ablation_report_ref
      +fresh_load_probe_ref
      +continue_training_probe_ref
      +rollback_ref
    }
    class "Future training/eval runner" {
      +load existing checkpoint
      +apply authorized rows
      +write candidate checkpoint
      +run source-disjoint eval
      +run ablations
    }
    class "Readiness firewall" {
      +requires metrics
      +requires fresh-load
      +requires continue-training
      +rejects exact memory shortcut
    }

    "User adaptive checkpoint" --> "Future training/eval runner" : load first when user state exists
    "Repo common checkpoint" --> "Future training/eval runner" : fallback when no user state
    "Future training/eval runner" --> "Checkpoint rewrite manifest" : writes candidate manifest after authorized mutation
    "Checkpoint rewrite manifest" --> "Readiness firewall" : evidence must satisfy gates
    "Readiness firewall" --> "Repo common checkpoint" : allow repo update only after all gates pass
    "Readiness firewall" --> "User adaptive checkpoint" : allow user backup/export/import/share only after lifecycle probe
```

## Manifest fields required before rewrite

| Field | Human meaning | Why required |
|---|---|---|
| `manifest_id` | stable id for this candidate rewrite | so rollback and audit can point to one manifest |
| `machine` | `PromptToTask`, `OrchestrationReasoning`, or `SkillSelection` | so rows cannot accidentally train the wrong sphere |
| `checkpoint_scope` | `common_repo` or `user_adaptive` | common and user legs have different permission and privacy rules |
| `source_pointers` | hashed source refs, no raw private text | so training can be resumed without leaking raw content |
| `training_dataset_checksum` | checksum of exact train-ready rows | prevents hidden row changes |
| `eval_fixture_checksum` | checksum of held-out fixtures | proves eval is source-disjoint from training data |
| `ablation_report_ref` | future report proving shortcuts fail | prevents path/id/node/stack shortcut masquerade |
| `dashboard_api_qa_ref` | dashboard/API examples and checks | proves results can be inspected by humans |
| `fresh_load_probe_ref` | future probe from empty process/state | proves checkpoint can be loaded after export/import |
| `continue_training_probe_ref` | future probe that resumes existing checkpoint | proves we did not retrain from zero |
| `backup_ref` | snapshot before mutation | allows rollback |
| `export_import_ref` | package/import proof | proves checkpoint is portable |
| `share_policy` | what can be shared and with whom | protects user adaptive data |
| `semantic_quality_gate` | exact metrics and thresholds | prevents "checkpoint exists" from becoming quality claim |

## Lifecycle required before mutation

1. Load user/cache checkpoint first if this is a user-adaptive machine.
2. If user/cache is absent, load repo/common checkpoint as fallback.
3. Before mutation, write backup metadata.
4. Apply only authorized train-ready rows.
5. Save candidate checkpoint under a new candidate id.
6. Export candidate checkpoint package.
7. Import it into a clean location.
8. Run fresh-load probe on the imported checkpoint.
9. Run continue-training probe from that imported checkpoint.
10. Run source-disjoint eval and shortcut ablations.
11. Only then may a later phase decide whether repo/common or user-adaptive manifest is accepted.

## What remains missing after P35

- No train-ready common dataset is authorized.
- No eval fixtures are materialized as runner input yet.
- No checkpoint candidate exists.
- No fresh-load probe result exists.
- No continue-training probe result exists.
- No numeric semantic metrics exist.

Therefore P35 cannot authorize checkpoint rewrite or semantic quality. It only defines what the rewrite manifest must contain when a later phase is authorized.
