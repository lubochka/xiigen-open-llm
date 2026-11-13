# Codex Recon — Core Assets (чекпойнты / манифесты / датасеты / отчёты)

Корпус: `core` · slug: `core-assets` · РЕКОН для плана ILearningDynamicGraph.
Только обзор реально найденных ассетов; код не исполнялся, git не трогался.

## Что это и где (точные ключевые пути)

Реестр и публикованные модели — `C:/Projects/llm_mvp_core/models`:
- `models/index.json` — корневой реестр (5 моделей: orchestration-reasoning-v1, orchestration-v1, prompt-to-task-v1, skill-selection-v1, translation-v1). Поля: model_id, domain, domain_tag, manifest_path, checkpoint_path, *_sha256, learned_pattern_count, training_row_count, held_out_row_count, held_out_accuracy, `semantic_quality_claimed`, `host_runtime_claimed`, `adaptive_work`, `external_ai_used`, `locator_proof_type`, published_at_utc.
- Каждая модель = тройка файлов в `models/<domain>/<id>/`: `manifest.json` + `<id>-checkpoint.json` (+ часто `runner-report.json`). Примеры:
  - `models/skill-selection/skill-selection-v1/{manifest,skill-selection-v1-checkpoint,runner-report}.json`
  - `models/orchestration-reasoning/orchestration-reasoning-v1/{manifest,orchestration-reasoning-v1-checkpoint,checkpoint}.json` (есть оба: `checkpoint.json` и `<id>-checkpoint.json` — возможный дубль)
  - `models/orchestration/orchestration-v1/`, `models/translation/translation-v1/`, `models/prompt-to-task/prompt-to-task-v1/`

Тренировочные данные — `C:/Projects/llm_mvp_core/training-data`:
- `training-data/orchestration/orchestration-v1/`: `corpus.jsonl.gz` (сжатый DPO-корпус), `corpus-pointer.json` (sha256+compression), `training-manifest.json` (linked_model, total_pairs=352, mode1/2/3 split train/eval).
- `training-data/translation/translation-v1/`: `corpus.jsonl.gz`, `training-manifest.json`.
- `training-data/skill-selection/skill-selection-v1/` — манифестом упомянуты `training-manifest.json`, `corpus-pointer.json`, `corpus-snapshot.jsonl` (надо подтвердить наличие на диске).
- Расширенные семантические датасеты под `training-data/smart-skills/semantic-quality/<...>/`: `*-dpo.jsonl`, `split.json`, `schema.json`, `dataset-card.json` (на них ссылается манифест orchestration-reasoning).

Артефакты прогонов/отчёты — `C:/Projects/llm_mvp_core/artifacts` (НЕ `reports/`):
- Покомпонентные `*-corpus.jsonl` и `*-corpus-summary.json` (orchestration-reasoning, prompt-to-task, prompt-plan-paired).
- DPO-конвейер: `*-dpo-schema-report.json`, `*-dpo-finalized-metadata.jsonl`, `*-dpo-finalization-report.json`, `dpo-train-readiness-report.json`.
- Аудиты: `deterministic-sweep-report.json`, `corpus-audit-*.json`, `external-tool-approval-report.json`, `*-asset-lifecycle-report.json`.
- Хосты/границы: `host-boundary-{claude,cursor,codex}.json`, `debug-trace-*.json`, `phase40-*-anti-kitchen-*`, `phase40-cross-host-prompt-suite-gate-report.json`.
- `artifacts/phase37/`, `artifacts/phase38/`: логи раннеров (`*.stdout/stderr.log`), checkpoint-бэкапы, `orchestration-loop-report-cycles-001-029.json`, `continue-report.json` (continue-training пробы).
- `data/alice.txt`, `data/` — похоже на песочницу/несвязанные данные.
- Каталог `reports/` и `datasets/` на верхнем уровне НЕ найдены (отчёты живут в `artifacts/`, датасеты — в `training-data/`).

## Как устроено / работает (кратко)

- Один шаблон ассета на модель: manifest → ссылается на checkpoint + runner-report + training-data (по ref и sha256). Целостность держится цепочкой sha256 (checkpoint_sha256 / manifest_sha256 / manifest_checkpoint_sha256), в index есть история ремонтов хэшей (`metadata_hash_repair`).
- Два формата манифеста сосуществуют: snake_case schema_version=2 (orchestration-v1) и PascalCase schemaVersion=1 (orchestration-reasoning-v1) — нет единой схемы.
- Checkpoint orchestration-v1 содержит `flow_grounding_knn[]` — массив DPO-пар (pair_id, mode, prompt, chosen{flow_ref, plan_text}, rejected_text, expected_route_mode/boundary_source/ml_generated/intent_kind, is_held_out). То есть KNN-память пар уже сериализуется в чекпойнт.
- runner-report (skill-selection) даёт `semantic_domain_matrix` на 11 юнитов: domain, machine, training_adapter, dpo_row_shape, checkpoint_section, held_out_fixture, `trainable_state_present`, `deterministic_final_authority=false`, `runtime_semantic_quality_claimed=false`. Это контрактная карта юнит↔адаптер↔DPO-строка↔секция чекпойнта.
- Честность статусов выдержана: почти везде `semantic_quality_claimed=false`, `host_runtime_claimed=false`, `external_ai_used=false`, `op45_no_deterministic_masquerade=true` — то есть это «PASS_AS_HONEST_UNTRAINED_SCAFFOLD», а не «PASS_AS_TRAINED_FEATURE».
- DPO-корпус с пометками `rejected_text: "...deterministic lookup instead of learned grounding"` учит модель предпочитать обучаемый путь детерминированному (анти-shortcut сигнал прямо в данных).

## Релевантность к ILearningDynamicGraph (DPO / обучение / граф / оркестрация / измерение / teacher-provenance)

- DPO-конвейер уже есть «end-to-end» по форматам: corpus.jsonl(.gz) → corpus-pointer/training-manifest → checkpoint(KNN-пары) → manifest → index.json → runner-report. Это готовый прототип жизненного цикла, который план авто-построения DPO для ILearningUnit/Flow/Orchestrator должен переиспользовать, а не изобретать.
- Граф/оркестрация: `flow_grounding_knn` + `expected_route_mode`/`expected_boundary_source` — это по сути обучаемые рёбра маршрутизации (узел prompt → выбранный flow/route). Прямо ложится на Graph-Style/DynamicGraph Reconstruction Gate скила (узлы/рёбра/контракты/кто владеет финальной властью на ребре).
- Измерение: held_out_row_count/held_out_accuracy в index, `held_out_fixture` per-unit, mode1/2/3 train-eval split в training-manifest, `dpo-train-readiness-report.json`, `deterministic-sweep-report.json`, `continue-report.json` — готовые места для метрик качества и ablation/no-shortcut проверок графа.
- Teacher / provenance / Legal Teacher Gate: `external_ai_used=false`, `rawPrivateTextCopied=false`, `commonOrAdaptive: "common"`, `external-tool-approval-report.json`. Это согласуется с Legal Teacher/Provenance Gate. ОДНАКО провенанс-полей по teacher_access_mode/hosting_provider/model_id/model_license/provider_terms_checked в манифестах НЕ видно — для DPO-генерации учителем это пробел (см. ниже).
- Common vs adaptive split: модели маркированы `adaptive_work=false`/`commonOrAdaptive:common`; есть `phase40-common-adaptive-split-gate-report.json` — основа для R5 (общие модели в repo, адаптивные на стороне юзера).

## Классификация (для кода/ассетов; ссылки на правила скила)

Эти ассеты — не код, а сериализованное состояние и отчёты, поэтому классифицирую то, на что они указывают, по `trainable-architecture-decision-SKILL.md`:

- Чекпойнты/манифесты/index — артефакты жизненного цикла уровня `ILearningMachine` (lifecycle checklist: load user→repo fallback, export/import, manifest, checkpoint, locator, continue-training). loader_type `SkillSelectionMachine`, machineId `orchestration-reasoning-machine` подтверждают, что владелец — Machine-уровень. Вердикт по форме: `OK_ALLOWED_LEVEL` (как scaffold-эвиденс), не семантическое качество.
- `flow_grounding_knn` в чекпойнте: KNN-память пар. По KNN-note скила это допустимый компонент модели/памяти ВНУТРИ allowed-level, а не самостоятельная архитектура → `OK_MODEL_COMPONENT`, при условии что финальная власть не у голого KNN-lookup. В данных это и заявлено (`deterministic_final_authority=false`, rejected учит против lookup). На уровне плана ILearningDynamicGraph каждое такое ребро обязано назвать владельца финальной власти (Graph Reconstruction Gate).
- `route_mode: "candidate_position_scaffold_not_final_semantic_authority"` (skill-selection): это прямое признание, что позиция кандидата — НЕ финальная семантическая власть. Совпадает с примером скила про `_candidatePositionWeights` → `CandidateOrderLeakageGuardUnit` (order-leakage guard, diagnostic only). Классификация: внутренний детерминистический хелпер/диагностика, не trainable-архитектура; держать помеченным.
- `semantic_domain_matrix` (11 «semantic units»): заявлены как `s14_contract_catalog_not_runtime_semantic_quality` — то есть контрактный каталог, а НЕ доказанное рантайм-качество. По двух-PASS правилу это `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`. Нельзя трактовать как `PASS_AS_TRAINED_FEATURE`.
- Датасеты (`*-dpo.jsonl`, corpus.jsonl.gz): по Required Dataset Table — кандидаты `KEEP_FOR_TRAINING`/`REWRITE_TO_DPO`; присутствие `rejected_text` про deterministic lookup = правильный анти-shortcut сигнал. Финальный вердикт по строкам не дан — нужен просмотр содержимого (см. пробелы).

## Пробелы / что доисследовать

- Не открыты тела `*.jsonl(.gz)` корпусов: реальная схема DPO-строк (chosen/rejected/reason/leakage-поля), happen ли target-label/source-path leakage — критично для авто-построения DPO в плане. Особенно `source_path: "training-bundle://..."` в чекпойнте — проверить, не утекает ли провенанс в обучающий сигнал.
- Teacher provenance отсутствует в манифестах: нет teacher_access_mode/hosting_provider/model_id/model_license/provider_terms_checked. Для Legal Teacher Gate плана это обязательные поля — сейчас покрыты только косвенно (`external_ai_used=false`). Доисследовать `external-tool-approval-report.json` и dataset-card.json.
- Дубль чекпойнта у orchestration-reasoning (`checkpoint.json` vs `orchestration-reasoning-v1-checkpoint.json`) — уточнить, какой канонический (manifest ссылается на `<id>-checkpoint.json`).
- Несогласованность схем манифестов (snake vs Pascal, schema_version 1 vs 2) — план должен зафиксировать единую схему манифеста/индекса.
- `index.json` неполный по полям: orchestration-v1 и translation-v1 не имеют checkpoint_sha256/locator_proof_type, которые есть у других → проверить целостность/локатор для них.
- Наличие на диске `training-data/skill-selection/.../corpus-snapshot.jsonl` и `training-manifest.json` подтвердить (упомянуты только в manifest/runner-report).
- Связь ассетов с реальными классами `ILearningMachine`/`IModelRepository`/`IRemoteUnitLocator` (15.OrchestratorModel, 25.ModelServer) НЕ прослежена в этом реконе — нужен отдельный код-рекон, чтобы привязать форматы к коду для плана DynamicGraph.
- Каталоги верхнего уровня `reports/`, `datasets/` отсутствуют; если план их предполагает — это решение по структуре, а не существующий ассет.
