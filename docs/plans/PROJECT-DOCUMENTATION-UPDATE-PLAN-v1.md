# PROJECT-DOCUMENTATION-UPDATE-PLAN-v1

Статус: план обновления документации, без реализации документации и без изменений production-кода.  
Ветка на момент создания: `codex/chat-gpt-5.5`.  
Область: все существующие проекты `llm_mvp_core` и три репозитория smart-skills extensions.

Этот план нужен для того, чтобы по каждому проекту появилась документация, которую можно читать как карту системы: что это за проект, какие данные через него проходят, какие алгоритмы реально работают, где они находятся в коде, и что ещё является заглушкой или инфраструктурой.

## 0. Что попросила Луба

Нужно сделать план обновления документации на уровне каждого проекта.

Документация по каждому проекту должна:

1. Чётко и человеческим языком объяснять, как проект работает.
2. Покрывать существующие проекты из `C:\Projects\llm_mvp_core`.
3. Покрывать extension-репозитории:
   - `C:\Projects\claude-xiigen-smart-skills`
   - `C:\Projects\cursor-xiigen-smart-skills`
   - `C:\Projects\codex-xiigen-smart-skills`
4. Обязательно содержать первые три части в формате, который уже закреплён для планов:
   - простая UML-диаграмма, которую можно импортировать в draw.io;
   - описание алгоритмов без кода;
   - подробное объяснение алгоритмов с короткими важными цитатами кода и точными ссылками на реальные файлы.

План должен не просто сказать “добавить документацию”, а задать проверяемый процесс: что читать, что писать, как проверять, и почему документ нельзя принять, если он снова получился как техническая заглушка.

## 1. Что уже есть и почему этого недостаточно

В репозитории уже есть полезные обзорные файлы:

| Файл | Для чего использовать | Почему не хватает |
|---|---|---|
| `docs/SUBPROJECT-MAP.md` | Карта проектов, статусы, ключевые файлы | Это краткий справочник, а не объяснение работы каждого проекта |
| `docs/ARCHITECTURE.md` | Общая архитектура и крупные потоки | Есть общие диаграммы, но нет глубины по каждому проекту |
| `docs/COMPONENT-INTERFACES.md` | Интерфейсы, методы, контракты | Слишком технический справочник; не объясняет алгоритмы человечески |
| `AGENTS.md`, `CLAUDE.md` | Правила качества документации | Это правила, а не документация проектов |
| README в extension-репозиториях | Текущий статус host-пакетов | README короткие и местами старые; они не объясняют реальный код entrypoint-ов |

Вывод: существующие файлы становятся источниками для новой документации, но не заменяют её.

## 2. Единый формат документации для каждого проекта

Каждый core-проект должен получить отдельный файл:

```text
docs/projects/<project-id>-<project-name>.md
```

Каждый extension-репозиторий должен получить файл:

```text
docs/PROJECT-DOC.md
```

README каждого проекта должен оставаться коротким: что это за проект, как его запускать, и где лежит подробная документация.

### 2.1 Часть 1 - простая UML для draw.io

В каждом документе должна быть простая диаграмма Mermaid или PlantUML, которую можно импортировать в draw.io.

Диаграмма должна показывать:

- главные классы или компоненты проекта;
- какие данные входят;
- какие данные выходят;
- какие соседние проекты используются;
- где trainable unit, где deterministic infrastructure, где host-thin-client, где честная заглушка;
- короткую заметку на русском для каждого узла.

После диаграммы должна быть таблица симуляций по связям.

Пример строки:

| Связь | Пример входа | Что происходит | Ожидаемый результат | Что запрещено |
|---|---|---|---|---|
| `Cursor extension -> XiigenExtensionRequest` | Пользователь просит подобрать скилы | Host собирает request и отправляет в core | Core возвращает `XiigenExtensionResponse` | Host не выбирает скилы сам |

### 2.2 Часть 2 - алгоритмы без кода

Для каждого важного алгоритма нужно написать:

- человеческое название алгоритма;
- точный класс;
- точный метод или функцию;
- какие параметры метод принимает;
- пошаговое описание, что метод делает;
- какие есть ветки поведения;
- что этот алгоритм не делает;
- статус: обучаемая логика, deterministic infrastructure, bootstrap, honest scaffold, legacy.

Недостаточно:

> `SkillSelectionMachine` выбирает скилы.

Достаточно:

> Алгоритм: выбор скилов из обученного состояния.  
> Класс: `SkillSelectionMachine`.  
> Метод: `DecideFromTrainedState(SkillSelectionRequest request)`.  
> Параметры: prompt, project root, available skill ids, host facts, source entry id.  
> Шаги: нормализует ключ промпта, читает веса из checkpoint, строит evidence features, оценивает candidate skills, возвращает selected/rejected skills и confidence.  
> Не делает: не принимает ранжирование от host как финальную semantic authority.

### 2.3 Часть 3 - важные цитаты кода и объяснение

После описания алгоритма нужны короткие цитаты кода с точными ссылками на реальные файлы.

Каждая цитата должна объяснять:

- почему этот код важен;
- какие данные входят;
- какие данные выходят;
- какую ветку поведения цитата доказывает;
- чего цитата не доказывает.

Правила:

- цитаты короткие;
- указывать файл и строку;
- читать файл перед цитированием;
- не ссылаться на “будущий код”;
- если это заглушка, честно писать “это заглушка”;
- если это обучаемая логика, рядом должны быть ссылки на checkpoint, manifest, locator, fresh-load или continue evidence, если такие claim-ы делаются.

## 3. Проверки качества документации

Каждый проектный документ должен пройти эти проверки.

| Проверка | Что должно быть видно |
|---|---|
| Человеческий язык | Русское объяснение без голых shortcodes и без “прочитай код сам” |
| UML | Диаграмма импортируется в draw.io, у узлов есть объяснения |
| Симуляции | Каждая важная связь в UML имеет пример входа и ожидаемый результат |
| Алгоритмы | Есть класс, метод, параметры и реальные шаги |
| Code refs | Цитаты короткие, точные, из текущего кода |
| Честность статуса | Заглушки, scaffolds и partial implementation названы прямо |
| Anti-stub | JSON/PowerShell/report/smoke не выдаются за реализацию кода |
| Anti-determinism | BM25, KNN, regex, dictionaries описаны как infrastructure/bootstrap, если они не являются обучаемой semantic authority |
| Testability | Каждая ветка алгоритма и UML-связь связана с тестом, QA или честным “нет теста, нужен work order” |
| Drift check | Документ сверяется с `docs/ARCHITECTURE.md`, `docs/SUBPROJECT-MAP.md`, `docs/COMPONENT-INTERFACES.md` |

## 4. Какие проекты покрывает план

### 4.1 Основные проекты `llm_mvp_core`

| ID | Проект | Приоритет | Что обязательно объяснить |
|---|---|---|---|
| D00-A | `00.Core` | Высокий | `ILearningMachine`, `ILogicalUnit`, `ILogicalFlow`, `ILogicalOrchestrator`, `OperationResult` |
| D00-B | `00.TensorCore` | Средний | Tensor ops, autograd, что это инфраструктура, а не продуктовая логика |
| D01 | `01.Tokenizer` | Средний | BPE tokenization |
| D02 | `02.DatasetPipeline` | Средний | Загрузка данных и batching |
| D03 | `03.TransformerModel` | Средний | Transformer blocks и поток данных внутри модели |
| D04 | `04.TrainingLoop` | Средний | Training loop, loss tracking |
| D05 | `05.Embeddings` | Средний | Embedding layer |
| D06 | `06.VectorSearch` | Средний | Vector/KNN search как infrastructure |
| D07 | `07.GraphEngine` | Средний | Symbol graph, PageRank |
| D08 | `08.GraphRag` | Средний | Graph RAG retrieval |
| D09 | `09.Inference` | Средний | TextGenerator, sampler |
| D10 | `10.LoRA` | Средний | Low-Rank Adaptation |
| D11 | `11.DPO` | Высокий | DPO training, chosen/rejected, near-miss negatives |
| D12-A | `12.SearchEngine` | Средний | BM25/KNN search как deterministic infrastructure |
| D12-B | `12.SemanticCompiler` | Низкий | Исторический/superseded статус |
| D13 | `13.CodeOptimizer` | Средний | Roslyn analysis and optimization |
| D14-A | `14.MachineLearning` | Средний | Classic ML algorithms |
| D14-B | `14.ContinuousLearning` | Высокий | Feedback, DPO pairs, LoRA adaptation, registry |
| D15 | `15.OrchestratorModel` | Высокий | User task -> decomposition -> route -> quality checks |
| D16 | `16.AutonomousLearning` | Средний | Automated training cycle |
| D17 | `17.DynamicDomains` | Низкий | Planned/placeholder honesty |
| D18 | `18.CapabilityTraining` | Высокий | Capability DPO training pipeline |
| D21-A | `21.AgentInfrastructure` | Высокий | Agent loop, storage, file editor, tool registry |
| D21-B | `21.AgentInfrastructure.Desktop` | Средний | Desktop surface |
| D22-A | `22.AgentAlgorithms` | Высокий | Decomposer, multi-agent orchestration, dependency graph |
| D22-B | `22.AgentAlgorithms.Desktop` | Средний | Desktop algorithm surface |
| D23-A | `23.OssEquivalents` | Средний | MCP, Roslyn analyzer, schema |
| D23-B | `23.OssEquivalents.Desktop` | Низкий | Desktop equivalent surface |
| D23-C | `23.SemanticCompilerAdapter` | Средний | Adapter boundary |
| D24 | `24.Compression` | Средний | Semantic compression levels, partial status |
| D25 | `25.ModelServer` | Высокий | Model repository, manifests, locators, checkpoints |
| D26 | `26.TrainableRouter` | Высокий | Trainable routing flow |
| D27-A | `27.TranslationDomain` | Очень высокий | TranslationMachine, training, inference, asset lifecycle |
| D27-B | `27.TranslationDomain.Tools` | Высокий | CLI export/import/publish/load/continue |
| D28 | `28.SemanticNlp` | Средний | NLP utilities |
| D29 | `29.SemanticCompiler` | Высокий | SemanticCompiler pipeline E01-E27 |
| D33 | `33.MatrixAlgorithms` | Средний | Matrix algorithms for training |
| D34 | `34.Experiments` | Низкий | Historical experiments |
| D35 | `35.ExperimentsV2` | Средний | Experiment runner and evidence |
| D36 | `36.LibraryComparison` | Низкий | OSS comparison and benchmarks |
| D42 | `42.ProjectWikiDomain` | Очень высокий | Project wiki, documentation graph, OrchestrationPlanMachine, anti-runtime-detector rules |
| D43-A | `43.LearningLocalWikiData` | Высокий | Local wiki data, training runners, hooks |
| D43-A-tools | `43.LearningLocalWikiData/tools/*` | Высокий | PluginHostInvocationHarness, ProjectWikiTrainingRunner, WikiContextHook |
| D43-B | `43.SmartSkillsDomain` | Очень высокий | SkillSelectionMachine, PromptToTask scaffold, OrchestrationReasoning scaffold, extension API |
| DTests | `Tests` | Высокий | Какие тесты что доказывают |
| DInspector-A | `LlmInspector.Core` | Средний | Inspector core |
| DInspector-B | `LlmInspector` | Средний | Inspector app/UI |
| DExpRunners | `experiments-v2/*/runner/*` | Низкий/группой | Группировать runner-ы, если они не активный product path |

### 4.2 Extension-репозитории

| ID | Репозиторий | Приоритет | Что обязательно объяснить |
|---|---|---|---|
| E-Claude | `C:\Projects\claude-xiigen-smart-skills` | Очень высокий | Реальный Claude entrypoint, request to core, debug trace |
| E-Cursor | `C:\Projects\cursor-xiigen-smart-skills` | Очень высокий | VS Code/Cursor command, request to core, QA path |
| E-Codex | `C:\Projects\codex-xiigen-smart-skills` | Очень высокий | Codex plugin/app entrypoint, request to core, debug trace |

В каждом extension-документе отдельно объяснить:

- какой файл является настоящим entrypoint;
- как prompt пользователя превращается в `XiigenExtensionRequest`;
- что именно отправляется в `llm_mvp_core`;
- что именно возвращается;
- как debug setting показывает `xiigen <method>`, `input`, `output`;
- почему JSON/PowerShell/log files являются evidence, а не заменой plugin-кода;
- что host никогда не делает: semantic skill ranking, DPO generation, training, private corpus handling, final semantic decisions.

## 5. Где должна лежать документация

В core repo:

```text
docs/projects/
  PROJECT-DOCUMENTATION-INDEX.md
  PROJECT-DOCUMENTATION-TEMPLATE.md
  _status/coverage-matrix.json
  00-core.md
  00-tensor-core.md
  01-tokenizer.md
  ...
  43-smart-skills-domain.md
  tests.md
  llm-inspector.md
  experiments-v2-runners.md
```

В extension repo:

```text
C:\Projects\claude-xiigen-smart-skills\docs\PROJECT-DOC.md
C:\Projects\cursor-xiigen-smart-skills\docs\PROJECT-DOC.md
C:\Projects\codex-xiigen-smart-skills\docs\PROJECT-DOC.md
```

## 6. Фазы выполнения

Фазы ниже не разрешают production-code changes. Это только документация, README links и coverage/status files.

### D0 - общий шаблон и индекс

Цель: создать стандарт, по которому потом пишутся все документы.

Файлы:

- `docs/projects/PROJECT-DOCUMENTATION-TEMPLATE.md`
- `docs/projects/PROJECT-DOCUMENTATION-INDEX.md`
- `docs/projects/_status/coverage-matrix.json`

Gate:

- в coverage matrix есть каждая строка из раздела 4;
- template требует UML, алгоритмы без кода, code quotations;
- template требует method names, parameters, branch simulations, test mapping.

### D1 - foundation stack

Проекты:

- `00.Core`
- `00.TensorCore`
- `01.Tokenizer`
- `02.DatasetPipeline`
- `03.TransformerModel`
- `04.TrainingLoop`
- `05.Embeddings`
- `09.Inference`
- `33.MatrixAlgorithms`

Gate:

- `00.Core` объясняет five-level learning hierarchy;
- ясно написано, что training живёт только на `ILearningMachine`;
- tensor/model docs не прячут продуктовую логику внутри math infrastructure.

### D2 - retrieval, search, graph, RAG, compression

Проекты:

- `06.VectorSearch`
- `07.GraphEngine`
- `08.GraphRag`
- `12.SearchEngine`
- `24.Compression`

Gate:

- BM25, KNN, PageRank, graph retrieval названы infrastructure/bootstrap;
- есть раздел “что это не доказывает”;
- нет claim-а “это обучаемое reasoning”, если это retrieval.

### D3 - DPO, LoRA, continuous learning, model server

Проекты:

- `10.LoRA`
- `11.DPO`
- `14.MachineLearning`
- `14.ContinuousLearning`
- `16.AutonomousLearning`
- `18.CapabilityTraining`
- `25.ModelServer`
- `26.TrainableRouter`

Gate:

- DPO объясняет chosen/rejected и near-miss negatives;
- model server объясняет manifest, checkpoint, locator, import/export, fresh-load, continue-training;
- continuous learning объясняет common/adaptive split.

### D4 - agent and orchestration

Проекты:

- `15.OrchestratorModel`
- `21.AgentInfrastructure`
- `21.AgentInfrastructure.Desktop`
- `22.AgentAlgorithms`
- `22.AgentAlgorithms.Desktop`
- `23.OssEquivalents`
- `23.OssEquivalents.Desktop`
- `23.SemanticCompilerAdapter`
- `29.SemanticCompiler`

Gate:

- каждая ветка orchestration имеет simulation row;
- user interrupt / authority boundary документирован для agent-facing проектов;
- deterministic pipeline steps не названы trainable decisions.

### D5 - trainable product domains

Проекты:

- `27.TranslationDomain`
- `27.TranslationDomain.Tools`
- `42.ProjectWikiDomain`
- `43.LearningLocalWikiData`
- `43.LearningLocalWikiData/tools/ProjectWikiTrainingRunner`
- `43.LearningLocalWikiData/tools/PluginHostInvocationHarness`
- `43.LearningLocalWikiData/tools/WikiContextHook`
- `43.SmartSkillsDomain`

Gate:

- `27.TranslationDomain` показывает training, inference, export/import, continue-training, checkpoint guard;
- `42.ProjectWikiDomain` разделяет documentation graph, project wiki, OrchestrationPlanMachine, build-time audit helpers;
- `43.SmartSkillsDomain` честно различает trained `SkillSelectionMachine`, skeleton `PromptToTaskMachine`, skeleton `OrchestrationReasoningMachine`, host API boundary;
- ни одна skeleton-машина не описана как trained feature.

### D6 - experiments and inspection

Проекты:

- `34.Experiments`
- `35.ExperimentsV2`
- `36.LibraryComparison`
- `28.SemanticNlp`
- `LlmInspector.Core`
- `LlmInspector`
- grouped `experiments-v2/*/runner/*`

Gate:

- active/historical/generated runner статусы разделены;
- runner-ы сгруппированы, если они не являются активным product path;
- inspector docs объясняют input/output и core/UI split.

### D7 - extension repo docs

Репозитории:

- `C:\Projects\claude-xiigen-smart-skills`
- `C:\Projects\cursor-xiigen-smart-skills`
- `C:\Projects\codex-xiigen-smart-skills`

Gate:

- в каждом repo есть `docs/PROJECT-DOC.md`;
- README ссылается на него;
- real source entrypoint назван;
- request/response flow до `llm_mvp_core` показан;
- debug behavior описан;
- JSON/PS1 evidence не выдаётся за plugin implementation;
- code quotations взяты из реального JS/TS/PS1 entrypoint, а не из отчётов.

### D8 - navigation and final review

Файлы:

- `docs/projects/PROJECT-DOCUMENTATION-INDEX.md`
- `docs/ARCHITECTURE.md`
- `docs/SUBPROJECT-MAP.md`
- README links в extension repo

Gate:

- index ссылается на все project docs;
- `SUBPROJECT-MAP.md` указывает на новые docs;
- `ARCHITECTURE.md` больше не пытается вместить все детали, а ведёт в per-project docs;
- coverage matrix говорит: done / not applicable / repair required по каждому проекту.

## 7. Шаблон bounded work order

```text
DOC-WO-XX: <project>
Max active time: 15 minutes
Mode: documentation-only

Allowed files:
  - docs/projects/<project>.md
  - docs/projects/_status/coverage-matrix.json
  - README link if needed

Mandatory reads:
  - AGENTS.md
  - CLAUDE.md
  - docs/projects/PROJECT-DOCUMENTATION-TEMPLATE.md
  - docs/SUBPROJECT-MAP.md row for this project
  - docs/ARCHITECTURE.md relevant section
  - docs/COMPONENT-INTERFACES.md relevant section
  - every source file quoted in the doc

Required skills/guides:
  - implementation-doc-uml-SKILL.md
  - implementation-doc-algorithms-SKILL.md
  - implementation-doc-code-refs-SKILL.md
  - implementation-doc-status-SKILL.md
  - implementation-doc-hierarchy-SKILL.md when ILearningMachine/ILogical* appears

Deliverable:
  - Russian human-readable project document
  - coverage matrix row update

Gate:
  - UML has notes and simulations
  - algorithms name class + method + parameters
  - code quotes are exact and current
  - stubs and deterministic helpers are labelled honestly
  - test/QA mapping exists or gap is explicit
```

## 8. Как архитектор проверяет каждый документ

1. Можно ли понять проект без чтения кода первым шагом?
2. Импортируется ли UML в draw.io?
3. Есть ли у каждой стрелки пример входа и ожидаемый результат?
4. Для каждого алгоритма есть class + method + parameters?
5. Описан ли сам алгоритм, а не только “что он должен делать”?
6. Цитаты кода короткие и объяснены?
7. Документ описывает current implementation, не future wish?
8. Заглушки и partial status честно названы?
9. Deterministic helpers не спрятаны как trainable logic?
10. Есть ли связь с тестами, QA или честный documentation gap?

Если хотя бы один пункт не проходит, документ не принят. Следующий шаг - documentation repair, а не implementation.

## 9. Completion matrix

| Требование | Evidence | Статус |
|---|---|---|
| Все core-проекты покрыты | Section 4.1 + coverage matrix | Planned |
| Все extension repo покрыты | Section 4.2 + coverage matrix | Planned |
| Человеческий русский язык | Template + review gate | Planned |
| UML для draw.io | Part 1 | Planned |
| No-code algorithms | Part 2 | Planned |
| Code quotations + explanation | Part 3 | Planned |
| Только фактическая реализация | Code quote rule + stub honesty | Planned |
| Нет hidden deterministic shortcut | D2/D5 gates | Planned |
| Documentation follows code | Mandatory source reads | Planned |
| Test/QA mapping | Review gate | Planned |

## 10. Что этот план не делает

План не:

- меняет production code;
- тренирует модели;
- запускает broad tests;
- меняет runtime extension behavior;
- заявляет, что существующая документация уже полная;
- принимает JSON/PowerShell/smoke reports как замену объяснению реального кода.

## 11. Первый шаг после принятия

Начать D0:

1. Создать `docs/projects/PROJECT-DOCUMENTATION-TEMPLATE.md`.
2. Создать `docs/projects/PROJECT-DOCUMENTATION-INDEX.md`.
3. Создать `docs/projects/_status/coverage-matrix.json`.
4. Заполнить matrix всеми проектами и extension repo из раздела 4.

До review D0 нельзя начинать массовое написание project docs: сначала нужно доказать, что template сам не допускает старую ошибку “UML слишком high-level, алгоритмов нет, цитаты кода не объясняют поведение”.
