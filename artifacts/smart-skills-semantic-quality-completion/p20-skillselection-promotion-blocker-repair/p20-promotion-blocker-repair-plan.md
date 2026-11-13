# P20 SkillSelection promotion blocker repair gate

Этот артефакт не продвигает модель в runtime/common path. Он фиксирует, какие доказательства нужны, чтобы потом безопасно решить вопрос о promotion. Сейчас правильное решение: не копировать P17E candidate в `models/skill-selection/skill-selection-v1`.

## Что P19 уже доказал

P19 сравнил текущую common-модель SkillSelection и P17E production-export candidate.

P19 показал четыре важные вещи:

1. P17E candidate существует как lifecycle/export artifact, но сам не заявляет readiness.
2. Runtime/common loader сейчас смотрит на `models/skill-selection/skill-selection-v1/manifest.json` и `models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json`, а не на P17E artifact directory.
3. Candidate меньше текущей common-модели: 19 trained pair ids против 900, 17 prompt keys против 75, 17711 bytes против 1309772 bytes.
4. Candidate manifest говорит `accuracyAtExport=0` и `semanticQualityClaimed=false`; locator говорит `runtime_wired=false`, `common_model_path_updated=false`, `final_model_path_updated=false`.

По-человечески: P17E доказал, что export/import lifecycle работает для маленького candidate. Он не доказал, что этот candidate лучше или хотя бы не хуже текущей common-модели.

## Почему слепое копирование P17E candidate запрещено

Слепое копирование P17E candidate в common runtime path перетрёт уже существующий common checkpoint. Это небезопасно, потому что P17E candidate:

- меньше по объёму обученного состояния;
- имеет нулевую export accuracy;
- сам не заявляет semantic quality;
- не подключён к runtime locator;
- не прошёл compatibility/non-regression против текущей common-модели;
- не имеет rollback/load proof;
- не прошёл fresh-load через `XiigenExtensionApiService`.

Такое копирование создало бы видимость promotion, но не доказало бы качество. Это ровно тот тип ошибки, который P19 должен был остановить.

## Repair path по каждому blocker

| Blocker | Current evidence | Required evidence | Exact artifact/test |
|---|---|---|---|
| `candidate_manifest_accuracy_zero` | P17E manifest: `accuracyAtExport=0` | Promotion candidate должен иметь positive eval report с ненулевыми metric fields и сравнением с existing common | future compatibility report + focused test row in this P20 checklist |
| `candidate_semantic_quality_false` | P17E manifest: `semanticQualityClaimed=false` | Semantic quality может быть заявлена только после source-disjoint eval, ablation, fresh-load, continue-training and API/firewall checks | future no-overclaim gate and manifest rewrite probe |
| `locator_artifact_only_not_runtime_wired` | P17E locator: `runtime_wired=false` | Locator rewrite должен явно показать candidate path, common path strategy, rollback path, and whether runtime uses the new path | future truthful locator rewrite artifact |
| `candidate_would_overwrite_larger_common_asset` | existing common checkpoint is much larger | Compatibility/non-regression must prove candidate does not regress current common behavior, or promotion must be side-by-side/versioned | future compatibility/non-regression test |
| `candidate_profile_coverage_smaller` | candidate has fewer trained ids and profile rows | Missing sections must be explained: either filled with training data or kept out of promotion | future coverage comparison report |
| missing rollback/load proof | P19 only says no promotion happened | Promotion must have backup of previous common path and load proof after rollback | future rollback/load proof test |
| missing fresh-load-through-runtime | Candidate was imported/exported, not loaded through runtime common loader | `XiigenExtensionApiService` must fresh-load the promoted path and return honest response boundaries | future runtime fresh-load probe |
| missing API/firewall rerun | P17F was before any promotion | After any promotion candidate rewrite, API/firewall must rerun to ensure no unsupported readiness claim leaks | future API/firewall rerun report |

## Gate checklist before any later promotion

1. Compatibility and non-regression: compare existing common model and candidate on the same source-disjoint scenarios.
2. Rollback and load proof: backup current common checkpoint, load candidate, rollback, and prove the old path loads again.
3. Fresh-load through `XiigenExtensionApiService`: runtime loader must read the promoted or versioned path from disk, not only a test artifact.
4. Truthful manifest and locator rewrite: manifest and locator must reflect exactly what is wired and what remains not-ready.
5. API/firewall rerun: unsupported claims must stay blocked; supported claims must have positive evidence.
6. No-overclaim: semantic quality, final readiness, full API readiness, host readiness, and dashboard readiness stay false unless their own gates prove them.
7. State and documentation sync: state, reports, and human docs must say the same scoped truth.

## Что P20 доказывает

P20 доказывает только это:

- P19 blockers перенесены в executable repair checklist.
- Каждый blocker имеет current evidence, required repair, forbidden shortcut, and required test/probe.
- P20 не меняет runtime/common model files.
- P20 не заявляет SkillSelection readiness, semantic quality, final model readiness, full API readiness, dashboard readiness, or host runtime readiness.

## Что P20 не доказывает

P20 не доказывает, что SkillSelection semantic quality готова. P20 не запускает training, не мутирует checkpoint, не продвигает model, не меняет `models/skill-selection/skill-selection-v1`, and не проводит runtime promotion.
