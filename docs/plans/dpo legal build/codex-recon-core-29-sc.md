# Codex Recon — 29.SemanticCompiler (OperationResult SC29)

Вопрос рекона: семантический компилятор (OperationResult SC29) — что компилирует и как.
Роль: рекон-аналитик (только чтение + запись этого файла). Это evidence для ПЛАНА ILearningDynamicGraph, не разрешение на реализацию.

## Что это и где (точные ключевые пути)

`29.SemanticCompiler` — отдельная сборка `SemanticCompiler29` со СВОИМ локальным
`OperationResult<T>` (НЕ `AgentInfrastructure.OperationResult<T>`; cross-assembly использует `using SC29 = SemanticCompiler29;`).
Назначение: сжать сырой текст промпта/кода в компактные «логические предикаты» + домен + язык стека + коэффициент сжатия.

Главные пути:
- `C:/Projects/llm_mvp_core/29.SemanticCompiler/Core/Core.cs` — `OperationResult<T>` (SC29), `BaseService`, контракты `ICompilerStorage`/`ICompilerEventLogger`, `enum Domain { CODE_Q, BUG, PLAN, ARCH, REFACTOR, UNKNOWN }` (D-04), записи `PipelineInput`, `PipelineOutput`.
- `C:/Projects/llm_mvp_core/29.SemanticCompiler/Fabric/Interfaces.cs` — фабричные интерфейсы: `ISemanticCompiler29.Compile(PipelineInput) -> OperationResult<PipelineOutput>`, плюс `IStackDetector29`, `IRouteClassifier29`, `ISemanticNormalizer29`, `IFamilyCollapser29`, `ILogicExtractor29`.
- `C:/Projects/llm_mvp_core/29.SemanticCompiler/Pipeline/SemanticCompilerPipeline.cs` — единственная реализация `ISemanticCompiler29`; оркестрирует 6 стадий.
- `C:/Projects/llm_mvp_core/29.SemanticCompiler/Routing/RouteClassifier.cs` — классификатор домена (keyword + predicate-vote + KNN-метод).
- `C:/Projects/llm_mvp_core/29.SemanticCompiler/Routing/StackDetector.cs` — детектор языка по маркер-строкам.
- `C:/Projects/llm_mvp_core/29.SemanticCompiler/Development/DevelopmentPromptCompiler.cs` — обёртка для dev-промптов (project 21): домен + лексикон dev-предикатов + декомпозиция на сегменты.
- `C:/Projects/llm_mvp_core/29.SemanticCompiler/Translation/PatternClassifier.cs` — классификатор порядка слов (SVO/SOV/VSO/IMPERATIVE) для перевода.
- Прочие стадии (бегло, не читались целиком): `Compression/AstTokenizer.cs`, `SemanticNormalizer.cs`, `FamilyCollapser.cs`, `LogicExtractor.cs`, `RoleAssigner.cs`; `Planning/Decomposer.cs`, `DependencyGraph.cs`, `StopSignalDetector.cs`; `Translation/ContextualDictionary.cs`, `PatternReorderer.cs`; `Beautification/TemplateBeautifier.cs`.

## Как устроено / работает (кратко)

`SemanticCompilerPipeline.Compile(PipelineInput)` (Core.cs `PipelineInput(Text, FilePath?)`) выполняет фиксированную последовательность (порядок обязателен, E20 Rule A):
1. `AstTokenizer.Tokenize` — токены AST, регистр сохранён.
2. `StackDetector.Detect(rawText)` — язык по сырому тексту ДО нормализации.
3. `RouteClassifier.Classify(tokens)` — домен.
4. `SemanticNormalizer.Normalize` — нижний регистр, стемминг, стоп-слова.
5. `FamilyCollapser.Collapse` — схлопывание словоформ к корню.
6. `LogicExtractor.Extract` — предикаты вида `VALIDATE_OP(X)`, `NULL_CHECK(Y)` (VocabSize=22).
Результат: `OperationResult<PipelineOutput>.Ok(new PipelineOutput(domain, predicates, stack, compression))`, где `compression = originalWordCount / predicateCount`. Пустой вход -> `Fail("EMPTY_INPUT")`.

Механизмы по сути детерминированные:
- `StackDetector`: считает совпадения строк-маркеров (`TypeScriptMarkers`, `CSharpKeywordMarkers`+`CSharpStructuralMarkers`, `PythonMarkers`), порог `MinMarkers=2`, выбор по max-счёту (`CountMatches`).
- `RouteClassifier.Classify(string[])` runtime-путь: голосование по статической `PredicateDomainMap`, при отсутствии — keyword-fallback по статической `KeywordMap` с жёстким приоритетом BUG>REFACTOR>PLAN>ARCH>CODE_Q (`ClassifyByKeywords`).
- `DevelopmentPromptCompiler`: статический `DevelopmentPredicateLexicon` (substring-match терминов) + правила по домену/предикатам -> множество dev-предикатов; декомпозиция через `Decomposer` с fallback по маркерам " and then "/" after that " и т.п.
- `PatternClassifier`: эвристика POS по двум статическим спискам (`ActionWords`/`StructureWords`) -> метка порядка слов.

## Релевантность к ILearningDynamicGraph (DPO/обучение/граф/оркестрация/измерение/teacher-provenance)

- Кандидат на роль предобработчика/feature-source: `Compile(...)` даёт типизированный компактный сигнал (domain, predicates[], stack, dev-predicate ids), пригодный как ВХОД к узлам графа (`ILearningUnit`/`ILearningFlow`) и как стабильный фичевектор для DPO-пар.
- Оркестрация: `SemanticCompilerPipeline` — фиксированная статическая цепочка из 6 стадий. Это НЕ `ILogicalOrchestrator`/`ILogicalFlow` (нет `RunAsync`/`ExecuteAsync`-контрактов уровней, нет fan-out/fan-in, нет async-графа). Для ILearningDynamicGraph это пример детерминированной композиции стадий, которую при оборачивании надо явно позиционировать как фичевыработку, а не как обучаемую маршрутизацию по рёбрам.
- Измерение: `CompressionRatio` — полезная числовая метрика входа (диагностика/feature), не метрика обучения (не accuracy/loss/reward-margin на held-out).
- Обучаемость/lifecycle — ОТСУТСТВУЕТ как у обучаемого узла: нет checkpoint/manifest/export-import/locator/fresh-load/continue-training. `RouteClassifier.Train(...)` пишет только в in-memory `_train` (теряется при перезапуске); `Classify(int[])` при пустом `_train` -> `Fail("NOT_TRAINED")`. То есть «обучение» здесь — заполнение списка в памяти, а не персистируемая модель.
- Teacher/provenance: teacher-выходов/distillation тут нет; гейт Legal Teacher неприменим к этому коду (N/A). Источник предикатов — собственные статические лексиконы/маркеры, не внешний LLM-teacher.
- Граф-рёбра/скрытые шорткаты: при включении 29 в ILearningDynamicGraph каждое ребро, где сейчас стоит `RouteClassifier`/`StackDetector`/`PatternClassifier`/`DevelopmentPredicateLexicon`, обязано иметь явный владельца авторитета. Если решение домена/маршрута на ребре остаётся за keyword/lexicon/KNN-памятью без обученного узла — это `FORBIDDEN_RUNTIME_SHORTCUT` (правило Graph-Style Reconstruction Gate в trainable-architecture-decision-SKILL.md), если не помечено явно как eval-only/helper/baseline.

## Классификация механизмов (по trainable-architecture-decision-SKILL.md / generated-code-review-SKILL.md Layer 5A)

Аллоу-уровни (ILearningMachine / ILogicalOrchestrator / ILogicalFlow / ILogicalUnit / ILearningMachinePair) — НЕ найдено: ни один тип в 29 их не реализует; используются только локальные фабричные интерфейсы `I*29` и `BaseService` сборки SC29.

- `SemanticCompilerPipeline` (`ISemanticCompiler29`): внутренний детерминированный конвейер-композиция стадий. НЕ orchestrator/flow по иерархии (нет уровневых контрактов, нет обучения). Классификация: внутренний хелпер/feature-pipeline. Для графа — `OK_INTERNAL_DETAIL` только если честно помечен как детерминированная предобработка; претензия «семантический/обучаемый» -> `REPORT_CLAIM_TOO_STRONG`.
- `RouteClassifier` (классификатор/роутер + KNN `JaccardDist`): runtime-путь `Classify(string[])` = статические `PredicateDomainMap`+`KeywordMap` (keyword/dictionary matching). Это «classifier/router/regex-keyword» — по скилу НЕ архитектурный паттерн, только internal detail. KNN-путь `Classify(int[])` есть, но без персистентного состояния и без lifecycle-доказательств -> KNN как standalone-авторитет недопустим (skill: «standalone KNN/retrieval как вся обучаемая архитектура» = нарушение). Вердикт сейчас: `OK_INTERNAL_DETAIL`/`BASELINE_MUST_BE_LABELLED`; как «обученный роутер домена» -> `NEEDS_ILOGICALUNIT_WRAPPER` + lifecycle.
- `StackDetector`: счётчик строковых маркеров (`CountMatches`) с порогом. Детерминированный детектор. Вердикт: `OK_INTERNAL_DETAIL` (helper) — не обучаемый, не авторитет для семантической претензии.
- `DevelopmentPromptCompiler.ExtractDevelopmentPredicates`: substring-lexicon + ручные if-правила по домену/предикатам. Детерминированный keyword/pattern-matcher. Вердикт: `OK_INTERNAL_DETAIL`/`BASELINE_MUST_BE_LABELLED`; как «обучаемая разметка dev-намерения» -> `NEEDS_ILOGICALUNIT_WRAPPER`.
- `PatternClassifier`: POS-эвристика по двум статическим спискам слов. Детерминированный классификатор. Вердикт: `OK_INTERNAL_DETAIL` (helper).
- `OperationResult<T>` (SC29), `BaseService`, `PipelineInput/Output`, `Domain` — инфраструктура/типы, не подлежат классификации уровней (N/A).

Итог классификации: 29.SemanticCompiler — детерминированный слой выработки фич/предикатов и доменной разметки на статических словарях, маркерах и эвристиках; «обучение» (`RouteClassifier.Train`) — только in-memory, без персистируемой модели и lifecycle. Любая claim «обучаемый/семантический узел графа» от этого кода = `REPORT_CLAIM_TOO_STRONG` до оборачивания в аллоу-уровень с доказательствами жизненного цикла.

## Пробелы / что доисследовать

- Не читались целиком (только бегло по Glob, потолок 3-8 файлов): `LogicExtractor.cs` (как именно строится словарь из 22 предикатов и есть ли там скрытое состояние), `SemanticNormalizer.cs`, `FamilyCollapser.cs`, `AstTokenizer.cs`, `RoleAssigner.cs`, `Decomposer.cs`, `DependencyGraph.cs`, `StopSignalDetector.cs`, `ContextualDictionary.cs`, `PatternReorderer.cs`, `TemplateBeautifier.cs` — проверить, нет ли где обучаемого/персистируемого состояния помимо `RouteClassifier._train`.
- Где и как 29 потребляется графом/оркестрацией: подтвердить вызовы `ISemanticCompiler29`/`DevelopmentPromptCompiler` из 15.OrchestratorModel, 16.AutonomousLearning, 21, 42 (grep по `ISemanticCompiler29`, `DevelopmentPromptCompiler`) — чтобы знать реальные рёбра графа и где стоит детерминированный авторитет.
- Кто (если кто-то) вызывает `RouteClassifier.Train(...)` / `Classify(int[])` в проде и существует ли путь сохранения весов — сейчас следов персистентности нет; подтвердить отсутствие checkpoint/manifest.
- Связь `PipelineOutput.Predicates` -> DPO: годятся ли предикаты как стабильный фичевектор для пар (риск утечки сигналов источника/пути при формировании пар) — отдельная проверка при проектировании датасета.
