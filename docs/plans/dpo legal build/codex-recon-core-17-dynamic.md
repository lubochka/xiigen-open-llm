# Рекон: 17.DynamicDomains — прецедент динамической структуры (граф, формируемый на лету)

corpus: core · slug: core-17-dynamic · relevance: medium

## Что это и где (точные ключевые пути)

Проект 17.DynamicDomains — это маленькая чистая библиотека графовых алгоритмов над
топологией суб-фаз. Один реальный исходник (всё остальное в каталоге — артефакты bin/obj):

- `C:/Projects/llm_mvp_core/17.DynamicDomains/DynamicDomainGraph.cs` — весь код домена.
- `C:/Projects/llm_mvp_core/17.DynamicDomains/DynamicDomains.csproj` — нет ProjectReference,
  нет зависимостей; «чистый C#», только System/LINQ через ImplicitUsings.

Содержимое `DynamicDomainGraph.cs`:
- `record DynamicDomainTransition(string FromId, string ToId)` — одно ребро графа.
- `static class DomainGraphClosure` — транзитивное замыкание: `HasAnyDependency`,
  `BuildPredecessorMap` (DFS через `ReachableTargets` + `BuildAdjacency`).
- `static class OrchestratorExpander` — `FindBridgeSubPhases` (BFS-кратчайший путь `FindPath`,
  берёт промежуточные «мостовые» узлы между маршрутизированными суб-фазами).

Реальные потребители (вне `.claude/worktrees`):
- `C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs`
  (`using DynamicDomains;`): строки ~531, ~552 (`HasAnyDependency`/`BuildPredecessorMap` для
  порядка суб-фаз), ~778 (`FindBridgeSubPhases` для вставки мостов), ~825 `BuildDynamicDomainTransitions()`.
- `C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E49OrchestratorExpander.cs`,
  `E50DomainGraphClosure.cs` — эксперименты E49/E50 (это автономные replica-эксперименты,
  свой Jaccard-код; класс 17 они НЕ импортируют, лишь моделируют ту же идею).
- `C:/Projects/llm_mvp_core/Tests/OrchestratorModelTests.cs` — тесты.

## Как устроено / работает (кратко)

Главное: 17 не хранит и не «создаёт» домены. «Динамическая структура на лету» формируется
В ДВА ШАГА вне 17:
1) Реестр топологии `ILogicalTopologyRegistry`
   (`C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/LogicalTopologyRegistry.cs`,
   record `LogicalTransition(TransitionId, FromTopologyKind, FromTopologyId, ToTopologyKind, ToTopologyId, ...)`)
   мутируется в рантайме юнитами `Add*TopologyMutationUnit`
   (`AddDomainTopologyMutationUnit`, `AddPhaseTopologyMutationUnit`, `AddSubPhaseTopologyMutationUnit`,
   `AddTransitionTopologyMutationUnit`). То есть домены/фазы/рёбра добавляются динамически в реестр.
2) `AdaptiveLogicalOrchestrator.BuildDynamicDomainTransitions()` читает `_topology.ListTransitions()`,
   фильтрует пары `sub-phase -> sub-phase` и проецирует их в `DynamicDomainTransition[]`. Затем 17
   по этому набору рёбер: (а) решает, есть ли зависимость между маршрутизированными суб-фазами
   (тогда переключает порядок выполнения), (б) находит «мостовые» суб-фазы и доставляет их в маршрут.

Итог: 17 — это чистый детерминированный граф-калькулятор (замыкание + кратчайший путь) над уже
готовым множеством рёбер. Прецедент «динамики» здесь = реестр топологии редактируется в рантайме
(mutation-юнитами), а 17 пересчитывает зависимости/порядок/мосты при каждом вызове.

E49/E50 показывают идею «как домены связываются автоматически»: ребро добавляется, если
Jaccard-перекрытие профилей предикатов двух доменов > 0.35; E50 — что граф «стабилизируется»
(edge_delta < 5%) после добавления N доменов. Это пока ТОЛЬКО эксперименты-симуляции, в
рантайм-граф 17 этот вывод рёбер не подключён.

## Релевантность к ILearningDynamicGraph (DPO / обучение / граф / оркестрация / измерение / teacher-provenance)

- ГРАФ / ДИНАМИКА: 17 — готовый, проверенный прецедент «динамической структуры»: узлы/рёбра
  (`DynamicDomainTransition`), транзитивное замыкание, поиск мостов, перестройка порядка
  выполнения на лету из изменяемого реестра. Хорошая опора для топологии узлов/рёбер
  ILearningDynamicGraph. Связка «mutation-units меняют реестр -> граф-калькулятор пересчитывает»
  — рабочий шаблон fan-in/fan-out поверх рёбер.
- ОРКЕСТРАЦИЯ: точка авторитета — `AdaptiveLogicalOrchestrator` (ILogicalOrchestrator). Именно он
  владеет финальным решением о порядке/мостах; 17 — его детерминированный помощник.
- ИЗМЕРЕНИЕ: E49 даёт метрики (`dependency_accuracy`, `graph_precision`), E50 —
  (`final_edge_delta`, `closure_round`). Это готовый словарь метрик для оценки качества
  авто-построения рёбер, если ILearningDynamicGraph будет учить предсказание зависимостей.
- DPO / ОБУЧЕНИЕ: в 17 обучения НЕТ. Это критично: текущий вывод рёбер (где он есть, в E49/E50)
  — детерминированный Jaccard-порог; для ILearningDynamicGraph это кандидат на ЗАМЕНУ обучаемым
  юнитом, а не на использование как финального авторитета (см. классификацию ниже).
- TEACHER-PROVENANCE: в 17 нет учителя/дистилляции/внешних источников — поле неприменимо (N/A).
  Профили предикатов в E49/E50 — захардкоженные синтетические массивы (`KnownDomains`/`NewDomains`),
  не из легального teacher-датасета; для обучения их нужно строить заново из разрешённых источников.

## Классификация (по правилам trainable-architecture-decision-SKILL.md)

`DomainGraphClosure` и `OrchestratorExpander` (в 17):
- Уровень архитектуры: НЕ обучаемый уровень. Это `OK_INTERNAL_DETAIL` — внутренний
  детерминированный хелпер (граф-замыкание DFS + BFS-кратчайший путь над заданными рёбрами).
- Почему: по скилу (Decision Procedure п.5-6 и раздел Verdicts) граф/обход без обученного
  состояния, без training data/checkpoint/lifecycle — это implementation detail внутри
  разрешённого уровня. Владеющий уровень — `ILogicalOrchestrator` (`AdaptiveLogicalOrchestrator`).
  Семантического/обучаемого утверждения 17 не делает -> shortcut-нарушения здесь нет, метка честная.
- Замечание по Graph-Style Gate: финальный авторитет на рёбрах сейчас детерминированный (топология
  из реестра + чистый обход). Это допустимо, пока на этих рёбрах НЕ навешивается «семантический/
  обученный» выбор. Если ILearningDynamicGraph объявит выбор рёбер/зависимостей обучаемым, то
  любой Jaccard-порог как финальный решатель = `FORBIDDEN_RUNTIME_SHORTCUT`, нужен обучаемый
  `ILogicalUnit` (вход: профили/контекст узлов без утечки индекса/источника; выход: ребро/вес),
  а детерминированный обход остаётся eval-only/baseline.

E49 `E49OrchestratorExpander` / E50 `E50DomainGraphClosure` (в 35.ExperimentsV2):
- Классификация: `KEEP_FOR_EVAL_ONLY` / `BASELINE_MUST_BE_LABELLED`. Jaccard-перекрытие предикатов
  с порогом 0.35 — детерминированный baseline-предиктор рёбер, не обучаемый юнит.
- Почему: нет typed trained state, нет checkpoint/manifest/export-import/locator, нет fresh-load/
  continue-training; данные синтетические и захардкоженные. По скилу это baseline/eval, а не
  семантическая архитектура. Для ILearningDynamicGraph — кандидат на REWRITE_TO_DPO (учить
  предсказание зависимости домена обучаемым юнитом), Jaccard оставить как baseline для ablation.

`Add*TopologyMutationUnit` (контекст «формирования на лету», в 21):
- Это уже `ILogicalUnit` (например `AddTransitionTopologyMutationUnit : ILogicalUnit<AddTransitionCommand, LogicalTransitionEntry>`),
  но честно `Confidence = 1.0`, детерминированная мутация реестра — `OK_INTERNAL_DETAIL`/
  обёртка-детерминированный юнит, не обучаемая семантика. Это и есть механизм «динамической
  структуры на лету» (правка реестра в рантайме), без обучения.

## Пробелы / что доисследовать

- Кто и когда вызывает `Add*TopologyMutationUnit` в рантайме (источник динамики): не прослежена
  цепочка инициатора мутаций топологии — нужно для прецедента «на лету» в плане ILearningDynamicGraph.
- Дублирование/дрейф: E49/E50 содержат собственную копию Jaccard-логики, не используя классы 17.
  Для плана решить: единый обучаемый предиктор рёбер vs две детерминированные реализации.
- Тесты `Tests/OrchestratorModelTests.cs` (упоминают DynamicDomain) не прочитаны построчно —
  при планировании зафиксировать, какие именно инварианты графа/порядка/мостов закреплены тестами.
- `ILogicalTopologyRegistry` прочитан только в части `LogicalTransition`/`ListTransitions`/
  `AddOrUpdateTransition`; домен/фаза/суб-фаза (Add*Domain/Phase/SubPhase) и `PredicateIds`/`SourceIds`
  на рёбрах не разобраны — потенциальные признаки для обучаемого предиктора зависимостей.
- Связь 17 с уровнями ILearning* за пределами `AdaptiveLogicalOrchestrator` не найдена;
  подтвердить, что 17 не используется как скрытый авторитет где-то ещё.
