# Codex Recon — 22.AgentAlgorithms: детерминистические алгоритмы (KNN/Jaccard/BM25)

Рекон для плана ILearningDynamicGraph. Только evidence, не разрешение на реализацию.
Скилы просмотрены: `docs/skills/trainable-architecture-decision-SKILL.md` (правила
уровней + KNN-нота), `docs/HOW-TO-USE-SKILLS.md` (найден). Прочитано 8 файлов из
области (защита от перегруза), остальное помечено в «Пробелы».

## Что это и где (точные ключевые пути)

Проект `22.AgentAlgorithms` — это algorithm-слой агента (контекст, ранжирование,
маршрутизация, выбор скилов). Релевантные под вопрос файлы:

- `C:/Projects/llm_mvp_core/22.AgentAlgorithms/Skills/SkillRetriever.cs` — Jaccard-KNN выбор скилов (E38V2).
- `C:/Projects/llm_mvp_core/22.AgentAlgorithms/Context/KnnSkillFilter.cs` — top-K фильтр скилов через `ISemanticSearchProvider` + confidence-порог.
- `C:/Projects/llm_mvp_core/22.AgentAlgorithms/Context/HybridRanker.cs` — BM25 + KNN + pattern fusion (E34V2), фиксированные веса.
- `C:/Projects/llm_mvp_core/22.AgentAlgorithms/Context/AstRelevanceRanker.cs` — `ISemanticSearchProvider` через BM25 по AST-токенам (Roslyn, SIMD).
- `C:/Projects/llm_mvp_core/22.AgentAlgorithms/Context/SemanticRanker.cs` — обёртка-сортировщик поверх `ISemanticSearchProvider`.
- `C:/Projects/llm_mvp_core/22.AgentAlgorithms/Context/PredicateDatasetRouter.cs` — KNN-голосование по predicate-векторам (E22), маршрут в dataset-бакеты.
- `C:/Projects/llm_mvp_core/22.AgentAlgorithms/Routing/RouteClassifier.cs` — доменный классификатор по ключевым словам (делегирует в `SemanticCompiler29.RouteClassifier.ClassifyByKeywords`), confidence = захардкоженные 0.95/0.2.
- `C:/Projects/llm_mvp_core/22.AgentAlgorithms/Indexing/RepoMapBuilder.cs` — карта символов, упрощённый in-degree (НЕ итеративный PageRank).
- `C:/Projects/llm_mvp_core/22.AgentAlgorithms/Fabric/Interfaces.cs` — контракты слоя (`ISemanticSearchProvider`, `ISkillFilter`, `ITaskDecomposer` и т.д.).

## Как устроено / работает (кратко)

- **SkillRetriever** — токенизирует задачу через `FamilyCollapser.CollapseString`,
  считает `JaccardSim` query-тегов и условия каждого скила, сортирует по убыванию,
  при `maxScore < 0.40` отдаёт fallback (always-on / категория). Каталог скилов —
  статический `SkillTriggerLibrary.All` (хардкод-список строк).
- **KnnSkillFilter** — зовёт `_search.SearchAsync`, считает confidence = topScore /
  суммаScore; если ниже порога 0.40 → возвращает всех кандидатов (fallback), иначе
  фильтрует по top-K id. Сама «семантика» делегирована провайдеру.
- **HybridRanker** — токенизация (regex + стоп-слова E15 + суффиксный стеммер),
  BM25 (K1=1.5, B=0.75) + нормированный внешний KnnScore + pattern-score
  (SVO/SOV/IMPERATIVE по таблицам слов); линейная смесь с фикс. весами 0.45/0.35/0.20.
- **AstRelevanceRanker** — единственная реальная реализация `ISemanticSearchProvider`
  в этом проекте: индексирует AST-kind частоты (Roslyn), BM25 + IDF, SIMD-путь при
  большом словаре. Всё локально, без HTTP.
- **SemanticRanker** — берёт scores у провайдера, переписывает `Score` чанков и
  сортирует. Логики обучения нет.
- **PredicateDatasetRouter** — `Train()` просто складывает примеры в `List`,
  `Predict()` берёт k ближайших по `JaccardDistance` и голосует большинством по
  бакетам. `AutoLabel()` — чистые `Contains`-эвристики (`namespace`, `throw`, `[fact]`).
  Состояние НЕ персистентно: нет checkpoint/export/import/manifest/locator.
- **RouteClassifier** — обёртка над keyword-классификатором SemanticCompiler29;
  confidence не вычисляется из данных, а присваивается константой.
- **RepoMapBuilder** — Roslyn парсит методы, строит callee→callers, нормированный
  in-degree (комментарий явно: «not iterative PageRank»).

## Релевантность к ILearningDynamicGraph (DPO/обучение/граф/оркестрация/измерение/teacher-provenance)

- **Кандидаты-ретриверы внутри обучаемого уровня.** По KNN-ноте скила KNN/BM25/Jaccard
  допустимы как *компонент* модели / memory-слой / candidate-retriever ВНУТРИ
  разрешённого уровня. Для ILearningDynamicGraph эти классы — естественные
  candidate-generation / feature-source / eval-baseline кирпичи на рёбрах графа,
  но НЕ финальная обучаемая авторитетность.
- **Готовый baseline для измерения.** BM25-ранкер (AstRelevanceNRanker) и Jaccard-KNN
  (SkillRetriever) дают детерминированную точку отсчёта, против которой можно мерить
  обучаемый юнит (ablation / no-shortcut gate из скила).
- **DPO/teacher-provenance:** в проекте 22 НЕТ генерации DPO-пар, чекпойнтов, teacher-
  вызовов или внешних AI. Слова «DPO» встречаются только в строках-комментариях
  (`SkillRetriever.cs:86`, `InternalFirstAiSupport.cs:227`) — это не код обучения и не
  teacher-provenance. Legal-teacher gate здесь не задевается.
- **Граф/оркестрация:** реальной graph-edge авторитетности (fan-in/fan-out,
  multi-output edge) в этих файлах нет; маршрутизация (`RouteClassifier`,
  `PredicateDatasetRouter`) — точечные детерминированные решения, пригодные как
  диагностический/feature-вход на рёбрах, не как trainable-узел.

## Классификация (уровень ИЛИ внутренний детерминистический хелпер, со ссылкой на правило скила)

Ни один класс в 22.AgentAlgorithms НЕ реализует `ILearningMachine` /
`ILogicalOrchestrator` / `ILogicalFlow` / `ILogicalUnit` / `ILearningMachinePair`
(grep по этим типам = 0 совпадений в коде; только текст «DPO» в 2 комментариях).
Следовательно по Decision Procedure скила все они — `internal detail only`.

| Класс/файл | Механизм | Уровень? | Классификация (правило скила) |
|---|---|---|---|
| SkillRetriever (`Skills/SkillRetriever.cs`) | Jaccard-KNN + статический каталог + порог 0.40 | нет | Внутренний детерминированный ретривер/baseline. Скил: «standalone KNN/retrieval не есть вся обучаемая архитектура» + «scorer/selector/KNN — implementation detail». Verdict `OK_INTERNAL_DETAIL` / `BASELINE_MUST_BE_LABELLED`. Допустим как candidate-retriever внутри обучаемого уровня; запрещён как финальная авторитетность выбора скила без обёртки. |
| KnnSkillFilter (`Context/KnnSkillFilter.cs`) | confidence-порог поверх провайдера | нет | Внутренний детерминированный фильтр. «KNN audit-triggered, не forbidden». `OK_INTERNAL_DETAIL`. |
| HybridRanker (`Context/HybridRanker.cs`) | BM25+KNN+pattern, фикс. веса | нет | Детерминированный fusion-scorer. Скил: scorer = implementation detail. `OK_MODEL_COMPONENT` (как feature/score внутри уровня) / `BASELINE_MUST_BE_LABELLED`. Фиксированные веса = НЕ обучение. |
| AstRelevanceRanker (`Context/AstRelevanceRanker.cs`) | BM25/IDF по AST-токенам | нет | Детерминированный retrieval-провайдер (реализация `ISemanticSearchProvider`). Идеальный candidate-retriever/eval-baseline внутри уровня. `OK_MODEL_COMPONENT` при честной метке. |
| SemanticRanker (`Context/SemanticRanker.cs`) | сортировка по scores провайдера | нет | Тонкая детерминированная обёртка. `OK_INTERNAL_DETAIL`. Имя «Semantic» НЕ означает обучаемую семантику — потенциальный over-claim. |
| PredicateDatasetRouter (`Context/PredicateDatasetRouter.cs`) | KNN-голосование + Contains-AutoLabel | нет | `Train()`/`Predict()` есть, но состояние эфемерное (List в памяти), без checkpoint/export/import/locator/fresh-load. По lifecycle-чеклисту скила — НЕ обучаемый юнит. Внутренний KNN-baseline-роутер. `OK_INTERNAL_DETAIL` как роутер-кандидат; `REPORT_CLAIM_TOO_STRONG`, если назвать «trained router». `AutoLabel` — детерминированная разметка, годна лишь для corpus-labeling/fixture (скил: deterministic helpers только для labeling/audit/fixture/eval). |
| RouteClassifier (`Routing/RouteClassifier.cs`) | keyword-классификатор + константный confidence | нет | Внутренний детерминированный классификатор. Скил: classifier/router = implementation detail. `OK_INTERNAL_DETAIL`. Захардкоженный confidence 0.95/0.2 — кандидат на `REPORT_CLAIM_TOO_STRONG` (Minimum Breadth: confidence floors). |
| RepoMapBuilder (`Indexing/RepoMapBuilder.cs`) | in-degree по call-графу | нет | Детерминированный структурный индекс. `OK_INTERNAL_DETAIL`. Честно помечен «not iterative PageRank». |

Итог: вся область 22.AgentAlgorithms по этому вопросу — **детерминированные
хелперы / eval-baseline / candidate-retrievers**, НЕ обучаемая авторитетность.
Для ILearningDynamicGraph они должны входить в обучаемый уровень с явной меткой
(eval-only / helper / candidate-retriever / baseline) и закрываться ablation /
no-shortcut тестом на каждом ребре (Graph-Style Reconstruction Gate скила).

## Пробелы / что доисследовать

- **Не прочитаны** (по лимиту 3–8 файлов; вероятная релевантность к графу/оркестрации):
  `MultiAgent/AgentOrchestrator.cs`, `MultiAgent/DependencyGraph.cs`,
  `Pipeline/AgentPipeline.cs`, `Pipeline/InternalFirstAiSupport.cs` (упоминает
  «DPO-ready learning signal» в комментарии — проверить, не маскируется ли обучение),
  `Comparison/LlmJudge.cs` + `AgentComparisonHarness.cs` (LLM-judge → возможный
  teacher/provenance вопрос), `Loop/ReactAgentLoop.cs`, `Tasks/*Decomposer.cs`,
  `Context/AstPackedContextBuilder.cs`, `Context/AstRelevanceRanker`-смежные ranker'ы.
- **Где живёт `FamilyCollapser` / `SkillTriggerLibrary` источник** (для SkillRetriever) —
  вне 22; уточнить, чтобы понять, можно ли каталог скилов сделать обучаемым.
- **`SemanticCompiler29.RouteClassifier.ClassifyByKeywords`** — реальная логика домена
  лежит в проекте 29; для графа маршрутизации проверять там.
- **Связь с разрешёнными уровнями** (15.OrchestratorModel / 00.Core
  `LearningMachineContracts`) — в 22 их нет; нужно подтвердить, что обёртки
  ILogicalUnit для этих ретриверов планируются в 15/00, а не в 22.
- **HOW-TO-USE-SKILLS.md** просмотрен только на наличие; детальная сверка каталога
  скилов не делалась (вне под-задачи).
