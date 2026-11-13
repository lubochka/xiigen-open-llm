# План наведения порядка в обучении project-local wiki

Статус: архитектурный план.
Дата: 2026-05-24.
Режим: архитектор.

Этот документ не говорит, что обучение закончено.
Этот документ не разрешает запускать новые батчи.
Этот документ нужен, чтобы остановить хаос вокруг wiki-обучения и дать
исполнителю точную, проверяемую схему работы.

Пока этот план не принят, executor запрещён.

Запрещено до принятия плана:

- запускать обучение;
- запускать новые батчи;
- запускать тесты только из-за изменения плана;
- менять ветку;
- коммитить и пушить этот план как завершённую фазу;
- продолжать от любого checkpoint;
- говорить, что задача готова.

Разрешено до принятия плана:

- читать документы;
- искать противоречия;
- править архитектурный план;
- уточнять границы между кодом, данными, весами и проектными wiki;
- описывать будущие действия executor-а.

## 0. Governing documents and source of truth

Этот план не должен жить отдельно от остальных правил проекта.

Перед любым будущим исполнением executor обязан читать:

```text
C:\Projects\llm_mvp_core\AGENTS.md
C:\Projects\llm_mvp_core\CLAUDE.md
C:\Projects\llm_mvp_core\docs\SESSION-START-PROMPT.md
C:\Projects\llm_mvp_core\42.ProjectWikiDomain\README.md
C:\Projects\llm_mvp_core\43.LearningLocalWikiData\PLAN-PROJECT-WIKI-CROSS-STACK-ADVANCED-GAP-CLOSURE-v1.md
C:\Projects\llm_mvp_core\43.LearningLocalWikiData\PLAN-PROJECT-WIKI-TRAINING-ASSET-REGISTRY-v1.md
```

Дополнительные protocol documents из `orchestrator extension` остаются
архитектурными источниками смысла:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\EXECUTOR-STAGE-PROMPT-v2.md
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\EXECUTOR-DISCIPLINE-RULES-v1.md
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md
```

Если эти документы противоречат этому плану, executor не должен сам выбирать
удобную версию и не должен превращать противоречие в преждевременную остановку.
Он обязан применить иерархию источников: текущая команда Luba, `AGENTS.md`,
`CLAUDE.md`, cross-stack plan, этот registry plan, затем внешние protocol
documents. Противоречие записывается в active `STATE.json`; работа продолжается,
если иерархия и document RAG дают безопасное недеструктивное действие. Только
destructive action, wrong-branch action или отсутствие безопасного действия
может стать `operator_input_required`.

Source of truth для learning assets:

```text
C:\Projects\llm_mvp_core\43.LearningLocalWikiData
```

Source of truth для runtime logic:

```text
C:\Projects\llm_mvp_core\42.ProjectWikiDomain
```

Source of truth для model weights:

```text
IModelRepository
```

Ветка не должна меняться автоматически. Если executor находится не на той ветке,
это blocker, а не повод запускать `git checkout`.

## 1. Главная цель

Мы обучаем не одну локальную wiki вручную.

Главная цель: XIIGen должен уметь сам создавать весь набор обучаемых юнитов,
которые нужны для project-local wiki любого подходящего проекта.

Правильная цепочка:

```text
запрос пользователя
  -> понять, что нужна project-local wiki
  -> определить, какие wiki-юниты нужны
  -> выбрать, какой юнит слабый сейчас
  -> построить 50 валидных строк обучения для этого юнита
  -> обучить юнит
  -> проверить held-out набор
  -> сохранить метрики, веса и манифест
  -> зарегистрировать юнит в проектной архитектуре
  -> импортировать или загрузить юнит через locator
  -> проверить на реальных проектах как validation target
  -> решить следующий слабый юнит
```

Локальная wiki конкретного проекта является результатом и проверочной
поверхностью.
Обучаемая способность, которая должна жить в `llm_mvp_core`, это умение
построить такую wiki-систему для проекта.

## 2. Роли и запреты

### 2.1. Architect

Архитектор:

- формулирует цель;
- разделяет код, данные, веса и project-local evidence;
- проектирует registry;
- определяет gate-ы;
- ищет противоречия;
- пишет план.

Архитектор не запускает обучение, не запускает тесты, не коммитит, не пушит и
не меняет ветку.

### 2.2. Executor

Executor появляется только после принятия плана.

Executor:

- создаёт файлы и код, указанные в плане;
- запускает только те проверки, которые прямо относятся к изменённому типу
  файлов;
- не принимает архитектурные решения сам;
- не говорит `готово`, если gate не пройден;
- не превращает ответ на вопрос о статусе в остановку, если следующий шаг
  известен;
- если Gate B не пройден и gap actionable, продолжает следующий пункт плана.

### 2.3. Training runner

Training runner:

- обучает конкретный юнит;
- пишет реальные метрики;
- сохраняет отчёт;
- сохраняет checkpoint или ссылку на него;
- не решает, что архитектурно делать дальше.

### 2.4. Governance editor

Governance editor:

- правит правила, промпты и настройки;
- проверяет текст, JSON и противоречия;
- не запускает `dotnet test`, если не менялся исполняемый код.

## 3. Почему нельзя просто продолжать обучение

Сейчас есть много батчей, отчётов и checkpoint-ов. Они перенесены внутрь
проекта:

```text
C:\Projects\llm_mvp_core\43.LearningLocalWikiData
```

Одного переноса недостаточно.

Перед новым обучением нужно для каждого batch-а знать:

- какой юнит он обучал;
- какой статус он получил;
- какие метрики были реально сохранены;
- есть ли train и held-out наборы;
- есть ли runner report;
- есть ли checkpoint;
- можно ли продолжать обучение от этого checkpoint;
- можно ли использовать batch как baseline;
- или его можно хранить только как историю ошибки.

Если этого не сделать, следующий запуск может продолжить обучение от плохого
checkpoint или принять rejected batch как улучшение.

## 4. Что где живёт

### 4.1. `42.ProjectWikiDomain`

Здесь живёт логика.

```text
C:\Projects\llm_mvp_core\42.ProjectWikiDomain
```

Тут должны быть:

- обучаемые wiki-юниты;
- оркестратор обучения;
- генераторы пар для Direct Preference Optimization;
- training runners;
- проверка метрик;
- логика выбора следующего слабого юнита;
- логика публикации манифестов;
- flow adapters для подключения к `15.OrchestratorModel`.

Тут не должны лежать огромные папки с результатами прогонов.

### 4.2. `43.LearningLocalWikiData`

Здесь живут данные обучения, отчёты и история прогонов.

```text
C:\Projects\llm_mvp_core\43.LearningLocalWikiData
```

Тут должны быть:

- batch registry;
- схемы registry;
- training batch metadata;
- runner reports;
- phase reports;
- held-out evaluation;
- resource reports;
- accepted/rejected history;
- ссылки на checkpoints;
- небольшие canonical examples, если они нужны для проверки.

Это не runtime-логика.
Это архив и источник правды о том, что реально обучалось и какие результаты
были получены.

`43.LearningLocalWikiData` не должен владеть runtime-кодом. Он владеет
данными, отчётами, registry и историей решений.

### 4.3. `IModelRepository`

Это обязательная секция. Без неё обученный юнит не является частью проекта.

`IModelRepository` отвечает за реальные веса модели и checkpoint-и.

В обычном git должны храниться:

- манифест юнита;
- ссылка на checkpoint;
- checksum;
- версия юнита;
- версия базовой модели;
- сведения о том, какой runner report принял checkpoint;
- import/export contract;
- locator key.

В обычном git не должны храниться бесконтрольно:

- случайные большие веса без манифеста;
- checkpoint без метрик;
- checkpoint без статуса accepted/rejected;
- локальные временные результаты runner-а.

Если веса хранятся через Git Large File Storage, это должно быть явно записано
в манифесте. Если веса хранятся во внешнем artifact storage, в git хранится
только ссылка и checksum.

Минимальный манифест обученного юнита:

```json
{
  "unit_name": "ProjectWikiQueryPlanningFlow",
  "unit_version": "v1",
  "base_model": "xiigen-small-doc-wiki-base",
  "checkpoint_ref": "model-repository://project-wiki/query-planning/v1/checkpoint-025",
  "checkpoint_checksum": "sha256:...",
  "runner_report_path": "43.LearningLocalWikiData/runner-reports/...",
  "accepted_batch_id": "project-wiki-query-planning-025",
  "import_contract": "ProjectWikiQueryPlanningManifest",
  "export_contract": "GeneratedCapabilityCheckpointManifest",
  "locator_key": "project-wiki.query-planning",
  "status": "accepted"
}
```

Gate для этой секции:

- у каждого accepted юнита есть манифест;
- в манифесте есть checkpoint reference;
- checkpoint reference указывает на существующий artifact или Git LFS объект;
- есть runner report, который принял этот checkpoint;
- есть locator key;
- есть import/export contract.

Если хотя бы одного пункта нет, юнит не считается встроенным.

### 4.3.1. Manifest ownership

В git можно хранить manifest.
В git нельзя хранить смысл "модель готова" без проверяемой ссылки на веса.

Минимальные файлы для accepted unit:

```text
43.LearningLocalWikiData/manifests/<unit>/<version>/model-manifest.json
43.LearningLocalWikiData/reports/<unit>/<batch-id>/runner-report.json
43.LearningLocalWikiData/registry/PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json
```

Если checkpoint хранится через Git Large File Storage, manifest должен
содержать object id или checksum. Если checkpoint хранится через
`IModelRepository`, manifest должен содержать repository key.

### 4.4. `.xiigen/wiki-domain` внутри проекта клиента

Внутри конкретного проекта должна лежать только локальная информация этого
проекта:

```text
C:\Projects\<project>\.xiigen\wiki-domain
```

Там могут быть:

- evidence graph;
- документация проекта;
- связи между функциями;
- локальный manifest;
- ссылка на project-local adapter;
- cache для локальной wiki.

Там не должна жить общая logic обучения XIIGen.

## 5. Регистрация обучаемого юнита в системе

Обучение само по себе не завершает задачу.

Юнит считается частью проекта только после всей цепочки:

```text
train
  -> validate
  -> publish manifest
  -> publish checkpoint reference
  -> register FlowAdapter
  -> register locator entry
  -> import/load through locator
  -> verify route to unit
```

Обязательные integration points:

- flow adapter в `15.OrchestratorModel/Logical/`;
- публикация checkpoint через `25.ModelServer`;
- доступ через `IRemoteUnitLocator`;
- запись в `43.LearningLocalWikiData` registry;
- runner report с реальными метриками.

Gate регистрации:

- adapter зарегистрирован в `LogicalOrchestratorAssistant`;
- locator может найти юнит по `locator_key`;
- manifest загружается без ручного знания пути;
- training orchestrator видит этот юнит как доступный;
- rejected checkpoint не может быть выбран как continuation source.

## 6. Canonical units

Используем один список имён, без альтернативных названий:

```text
ProjectWikiRequestRouterUnit
StackAndVersionClassifierUnit
CodeTraceBuilderUnit
EntityImpactMapperUnit
DocumentationGeneratorUnit
NegativeDpoGenerator
ProjectWikiQueryPlanningFlow
WikiContextRoutingUnit
ProjectWikiAnswerComposer
ProjectAnswerVerifier
WikiTraceAggregator
ProjectWikiTrainingOrchestrator
```

`WikiTraceAggregator` не просто пишет отчёт.
Он помогает системе решить, какой юнит тренировать следующим.

После плохого результата человек не должен вручную гадать:

```text
кого тренировать теперь?
```

Система должна видеть:

- какой юнит провалился;
- какой тип ошибки повторяется;
- какой batch нужен дальше;
- можно ли продолжать;
- или нужно остановить blind training.

## 7. Registry для training assets

Нужно создать единый registry:

```text
C:\Projects\llm_mvp_core\43.LearningLocalWikiData\PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json
```

И схему:

```text
C:\Projects\llm_mvp_core\43.LearningLocalWikiData\PROJECT-WIKI-TRAINING-ASSET-REGISTRY.schema.json
```

### 7.1. Обязательные поля registry entry

Каждый batch обязан иметь запись:

```json
{
  "batch_id": "project-wiki-query-planning-025",
  "unit_name": "ProjectWikiQueryPlanningFlow",
  "unit_version": "v1",
  "status": "accepted",
  "status_reason": "held-out gate passed and combo did not regress",
  "train_rows": 50,
  "heldout_rows": 50,
  "train_path": "43.LearningLocalWikiData/...",
  "heldout_path": "43.LearningLocalWikiData/...",
  "runner_report_path": "43.LearningLocalWikiData/...",
  "metrics": {
    "accuracy": 0.92,
    "reward_margin": 0.031,
    "loss": 0.68,
    "cpu_ms": 1200,
    "ram_mb": 350,
    "elapsed_ms": 5000
  },
  "checkpoint_ref": "model-repository://...",
  "checkpoint_allowed_as_continuation": true,
  "allowed_next_action": "may_use_as_baseline",
  "source_authority": "runner_report",
  "import_export_status": "manifest_published",
  "locator_status": "registered",
  "artifact_integrity": "verified",
  "git_commit": "UNKNOWN",
  "artifact_checksum": "sha256:...",
  "lfs_oid": "UNKNOWN",
  "source_run_id": "2026-05-20-project-wiki-query-planning-promotion-025",
  "discovered_from": "43.LearningLocalWikiData/runner-metrics/...",
  "review_status": "machine_classified_pending_human_review",
  "created_at": "2026-05-24T00:00:00Z"
}
```

Если метрика отсутствует, она должна быть `UNKNOWN`, а не выдумана.

### 7.2. Status: accepted

Batch получает `accepted`, только если:

- есть train rows;
- есть held-out rows;
- есть runner report;
- runner report содержит реальные метрики;
- held-out accuracy проходит порог конкретного юнита;
- reward margin положительный, если этот runner его считает;
- combo run не регрессировал baseline, если combo run применим;
- checkpoint опубликован или явно не нужен для этого unit type;
- нет hard-gate нарушений;
- registry entry содержит allowed next action.

Allowed next actions:

```text
may_use_as_baseline
may_use_as_replay_source
may_use_checkpoint_as_continuation
may_publish_manifest
```

### 7.3. Status: rejected

Batch получает `rejected`, если:

- отрицательный reward margin;
- held-out accuracy ниже порога;
- combo run ухудшил baseline;
- rejected side в Direct Preference Optimization слабый или двусмысленный;
- есть chosen == rejected;
- есть missing `[WRONG: ...]`;
- есть mojibake;
- есть wrong project evidence;
- checkpoint создаёт деградацию;
- runner report явно rejected.

Allowed next action:

```text
preserve_as_negative_history
do_not_continue_from_checkpoint
use_for_failure_analysis_only
```

Rejected checkpoint нельзя использовать как continuation source.

### 7.4. Status: experimental_no_authority

Batch получает `experimental_no_authority`, если:

- нет полного runner report;
- нет held-out набора;
- непонятно, какой unit обучался;
- batch был диагностическим;
- метрики были получены не runner-ом;
- source authority неясен;
- checkpoint есть, но неизвестно, принят ли он.

Allowed next action:

```text
preserve_trace_only
requires_adjudication_before_use
```

### 7.5. Status: blocked

Batch получает `blocked`, если:

- повреждены файлы;
- отсутствует train или held-out path;
- отсутствует runner report, но batch помечен accepted;
- checkpoint path битый;
- batch конфликтует с другим batch id;
- registry не может определить unit name.

Allowed next action:

```text
repair_metadata_before_use
```

## 7.6. Training block flag

До завершения registry validation должен существовать явный blocking state:

```json
{
  "training_continuation_blocked_until_registry_validated": true,
  "reason": "existing artifacts are not classified yet"
}
```

Ни один future runner не должен начинать новый batch, если этот flag равен
`true`.

## 8. Алгоритм миграции существующих artifacts

Executor позже должен выполнять миграцию строго так:

1. Прочитать текущую ветку и не менять её.
2. Прочитать список artifacts в `43.LearningLocalWikiData`.
3. Найти все train/held-out файлы.
4. Найти все runner reports.
5. Найти все checkpoints.
6. Найти все manifest files.
7. Сгруппировать artifacts по unit name и batch id.
8. Если один artifact подходит к двум batch id, поставить обе группы в
   `blocked`.
9. Для каждой группы построить registry entry.
10. Если группа неполная, поставить `blocked` или `experimental_no_authority`.
11. Если runner report rejected, поставить `rejected`.
12. Если runner report passed и все gate-ы выполнены, поставить `accepted`.
13. Записать registry.
14. Записать artifact integrity report.
15. Записать summary report миграции.
16. Не запускать обучение в этой же фазе.

Gate миграции:

- каждый найденный batch имеет ровно одну registry entry;
- нет accepted entry без runner report;
- нет accepted entry без allowed next action;
- нет continuation source из rejected batch;
- все missing metrics записаны как `UNKNOWN`;
- каждый checkpoint имеет checksum, object id или repository key;
- каждый manifest связан с ровно одним accepted batch или помечен blocked;
- migration summary показывает counts по `accepted`, `rejected`,
  `experimental_no_authority`, `blocked`.

## 8.1. Artifact integrity audit

Миграция должна отдельно проверить целостность переноса.

Нужен файл:

```text
C:\Projects\llm_mvp_core\43.LearningLocalWikiData\ARTIFACT-INTEGRITY-AUDIT.json
```

Он должен содержать:

- сколько файлов найдено;
- сколько training rows найдено;
- сколько held-out rows найдено;
- сколько runner reports найдено;
- сколько checkpoints найдено;
- сколько manifests найдено;
- сколько файлов невозможно классифицировать;
- какие artifacts отсутствуют для accepted candidates;
- какие checkpoints не имеют manifest;
- какие reports не имеют batch rows;
- какие batch rows не имеют report.

Gate audit:

- ни один accepted batch не имеет missing artifact;
- ни один rejected checkpoint не помечен continuation source;
- все unknown artifacts перечислены явно;
- audit report сохранён до любого нового обучения.

## 8.2. Migration report format

Cleanup/migration report не является training report.

Он должен показывать:

```text
scope: cleanup_migration
training_started: false
new_batches_created: 0
accuracy: UNKNOWN unless read from runner report
reward_margin: UNKNOWN unless read from runner report
loss: UNKNOWN unless read from runner report
cpu_ms: migration process value if measured
ram_mb: migration process value if measured
elapsed_ms: migration process value if measured
accepted_count: N
rejected_count: N
experimental_no_authority_count: N
blocked_count: N
next_allowed_action: validate_registry_or_fix_blocked_entries
```

Нельзя писать training report для cleanup phase.
Если runner-emitted metric отсутствует, значение должно быть `UNKNOWN`.

## 9. Правильный training loop после миграции

После принятия registry обучение должно идти не вручную, а через loop:

```text
цель
  -> прочитать registry
  -> прочитать слабые места по WikiTraceAggregator
  -> выбрать weak unit
  -> построить 50 строк
  -> обучить
  -> проверить held-out
  -> записать runner report
  -> обновить registry
  -> опубликовать manifest, если gate принят
  -> зарегистрировать unit, если он стал publishable
  -> решить следующий unit
```

Этим управляет:

```text
ProjectWikiTrainingOrchestrator
```

Он должен:

- принять цель обучения;
- выбрать wiki-юнит;
- выбрать batch strategy;
- запустить runner;
- прочитать реальные метрики из report artifact;
- записать результат в registry;
- не говорить `done`, если gate не пройден;
- перейти к следующему юниту, если это разрешено.

## 10. Использование тестовых проектов

Проекты:

```text
C:\Projects\wizGameserver
C:\Projects\GenieDynamicServer
C:\Projects\geniesoftGameMVPServer
C:\Projects\genietasks
C:\Projects\askapediaweb
```

используются как validation target, а не как generic training source.

На них нужно проверять:

- строится ли evidence graph;
- определяется ли stack и версия .NET/C#;
- строится ли bottom-up documentation;
- строится ли call graph;
- строится ли value-flow;
- умеет ли query planner разбивать вопрос на серию wiki search queries;
- умеет ли answer composer отвечать с evidence ids;
- умеет ли verifier ловить missing evidence;
- умеет ли система работать со stored procedures, если они есть;
- умеет ли система отвечать на developer impact questions, например:
  "я хочу добавить поле в entity, где оно пройдёт и что затронет?"

Если проект клиентский или нет явного разрешения на обучение, его код нельзя
использовать как generic training source.

## 11. Что нельзя делать

Нельзя:

- продолжать обучение от rejected checkpoint;
- считать папку `runner-metrics` доказательством качества без registry;
- считать зелёные unit tests training report;
- использовать проект клиента как generic training source без разрешения;
- складывать общую training logic в `.xiigen/wiki-domain`;
- говорить `готово`, если есть файлы, но нет пройденных gate-ов;
- запускать новые batch-и, если непонятно, от какого состояния продолжаем;
- показывать accuracy, CPU, RAM или time без сохранённого runner report;
- называть deterministic validation обучением модели.

## 12. Что должен сделать executor после принятия плана

Только после принятия плана executor получает задачи:

1. Создать registry schema.
2. Создать registry importer.
3. Просканировать `43.LearningLocalWikiData`.
4. Создать `PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json`.
5. Создать migration summary report.
6. Добавить чтение registry в `ProjectWikiTrainingOrchestrator`.
7. Добавить использование registry в `WikiTraceAggregator`.
8. Добавить защиту от continuation from rejected checkpoint.
9. Добавить проверку import/export manifest для accepted units.
10. Добавить регистрацию publishable units через flow adapter и locator.
11. Добавить protection flag
    `training_continuation_blocked_until_registry_validated`.
12. Добавить artifact integrity audit.
13. Добавить cleanup/migration report.
14. Добавить next-training-action report, который называет следующий слабый unit
    и стратегию следующего 50-row batch-а без запуска обучения.

Executor не должен в этой фазе запускать новые training batch-и.

## 13. Gate B для завершения cleanup/migration phase

Фаза cleanup/migration считается завершённой только если:

- существует registry schema;
- существует registry;
- все найденные batch artifacts имеют статус;
- каждый accepted batch имеет runner report;
- каждый accepted checkpoint имеет manifest или artifact reference;
- rejected checkpoints запрещены как continuation source;
- artifact integrity audit сохранён;
- cleanup/migration report сохранён;
- blocking flag снят только после успешной registry validation;
- `ProjectWikiTrainingOrchestrator` читает registry;
- `WikiTraceAggregator` может назвать следующий weak unit;
- существует `reports/PROJECT-WIKI-NEXT-TRAINING-ACTION.json`;
- next-training-action report содержит `training_started: false` и
  `new_batches_created: 0`;
- next-training-action report называет actionable target unit, evidence batches
  и конкретную 50-row batch strategy;
- migration report содержит counts по всем статусам;
- нет `UNKNOWN` в статусе batch-а, только в отдельных метриках;
- executor не запустил новые batch-и во время cleanup.

Проверочная команда Gate B:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File 43.LearningLocalWikiData\tools\Assert-ProjectWikiTrainingAssetRegistryGate.ps1
```

Команда должна вывести:

```text
PROJECT_WIKI_TRAINING_ASSET_REGISTRY_GATE_B_PASSED
```

Если любой пункт не выполнен, фаза не завершена.

Если фаза не завершена, но следующий пункт плана известен, executor не должен
останавливаться на отчёте. Он должен использовать отчёт только как внутреннюю
проверку и продолжить выполнение следующего пункта.

## 13.1. Future checks for executor phase

Когда план будет принят и executor начнёт реализацию, нужны проверки именно
этого слоя, а не случайный full test ritual.

Будущие проверки должны покрыть:

- registry loader читает все entries;
- accepted entry без runner report отклоняется;
- rejected checkpoint нельзя выбрать как continuation source;
- missing metric остаётся `UNKNOWN`;
- artifact integrity audit находит dangling checkpoint;
- `ProjectWikiTrainingOrchestrator` читает registry перед выбором unit;
- `WikiTraceAggregator` выбирает следующий weak unit из registry и traces;
- publishable unit имеет manifest и locator key.

Эти проверки становятся задачей executor-а только после принятия этого
архитектурного плана.

## 13.2. Dirty worktree and unrelated files

Executor не должен чинить хаос через откат чужих изменений.

Правило:

- перед будущей реализацией записать текущий branch и dirty files;
- не трогать unrelated files;
- не добавлять `.claude/settings.local.json` в commit случайно;
- не менять ветку без явного указания Luba;
- если нужная работа зависит от другой ветки, остановиться как architect и
  описать branch conflict.

## 14. Ожидаемый результат этапа

После выполнения этого плана у нас должно быть:

- понятное место для всех wiki learning artifacts;
- единый registry accepted/rejected/experimental/blocked batch-ей;
- запрет случайного continuation от плохого checkpoint;
- разделение между logic, learning data, model weights и project-local evidence;
- понятный import/export path для обучаемых юнитов;
- регистрация юнитов через project infrastructure;
- возможность продолжать обучение через orchestration loop;
- меньше false positive reports;
- понятный ответ на вопрос: "что именно тренируем дальше и почему?"

Только после этого имеет смысл запускать следующие batch-и.

## 15. Человеческий словарь

Чтобы executor не писал нечитаемые отчёты, используем такие простые значения:

- batch: пакет из 50 строк обучения или проверки;
- held-out: проверочный набор, который модель не видит во время обучения;
- checkpoint: снимок обученного состояния модели;
- manifest: паспорт юнита и ссылка на его веса;
- registry: список всех batch-ей и их статусов;
- accepted: можно использовать дальше;
- rejected: нельзя продолжать от этого checkpoint;
- experimental: хранить как след, но не использовать для продвижения;
- blocked: сначала починить metadata или missing files;
- runner report: сохранённый отчёт реального запуска обучения или проверки.
