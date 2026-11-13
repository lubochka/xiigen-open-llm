# EXPERIMENT-RESULTS-MAP.md — E01-E50 validated results

Last updated: 2026-05-01 (59 runs including V2 variants — 30 PASSED, 5 SCALE_UP, 27 FAILED)

## Validated for production use

| Experiment | Component | Delta | Use in |
|---|---|---|---|
| E01 | AstTokenizer | +93% perplexity | 12.SemanticCompiler Compression/ |
| E03 | N-gram (bigram→trigram) model | +64% perplexity | 12.SemanticCompiler Compression/ |
| E04 | BM25 (Best Match 25) search | +100% recall@3 | 22 AstRelevanceRanker |
| E05 | Syntax filter | +30% top1_accuracy | 12.SemanticCompiler Compression/ |
| E08 | SyntaxRoles | +56% top1_accuracy | 22 TaskDecomposer |
| E09 | K-NN (K-Nearest Neighbours) structural | +45% top1_accuracy | 22 AstRelevanceRanker |
| E10 | Regex prep | +26% parse speed | 09.Inference |
| E11 | Step speed | +54% ms_per_step | 09.Inference, 22 ContextWindowManager |
| E12 | Matrix optimization | +43% benchmark_ms | 33.MatrixAlgorithms |
| E15 | Semantic tokenizer | +10% perplexity | 12.SemanticCompiler Compression/ |
| E16 | Synonym collapse | +8% perplexity | 12.SemanticCompiler Compression/ |
| E17 | Logic predicates | +97% perplexity | 12.SemanticCompiler Compression/ |
| E18 | PatternClassifier | +15% top1_accuracy, 95% domain | 12 RouteClassifier, 22 |
| E19 | Complexity router | +18% binary_accuracy | 12 RouteClassifier |
| E21 | BoundaryDetect | +143% boundary_accuracy | 22 TaskDecomposer |
| E23 | TemplateBeautifier | +833% template_match_rate | 12 Beautification/ |
| E25 | PatternClassifier (SVO/SOV/VSO) | +155% top1_accuracy | 12 Translation/ |
| E26 | ContextualDictionary | +150% disambiguation_accuracy | 12 Translation/ |
| E27 | PatternReorderer | +250% exact_match_rate | 12 Translation/ |
| E32 | EpisodeDecomposer | +11% count_accuracy | 15 OrchestratorModel |
| E34V2 | Hybrid BM25+K-NN matcher | +8% hit_at_1 | 22 AstRelevanceRanker |
| E35 | DependencyDetector | +30% dependency_accuracy | 22 AgentOrchestrator, 15 |
| E38V2 | SkillRetriever (fair K=5 baseline) | +100%, 86% load reduction | 22 ContextWindowManager |
| E38 | SkillRetriever (precision_at_5_fair) | +100% | 22 ContextWindowManager |
| E39 | QualityOrchestrator | +20% check_f1 | 22 QualityOrchestrator |
| E40V2 | TrustScorer (StratifiedSplit) | +50% trust_accuracy | 22 TrustScorer |
| E41 | IdleDetector | +100% idle_accuracy | 16 AutonomousLearning |
| E43 | ReplayBuffer 70/30 | +23% retention_accuracy | 16 AutonomousLearning |
| E49 | OrchestratorExpander | +44% dependency_accuracy | 17 DynamicDomains, 22 |
| E50 | DomainGraphClosure | +75% final_edge_delta | 17 DynamicDomains |

## Scale-up required (needs more data — SCALE_UP status)

| Experiment | Reason |
|---|---|
| E14 | ContextAction K-NN: data too separable at Tier 3 |
| E20 | StackDetector: synthetic data 100% trivial at Tier 3 |

## Failed — architectural finding

| Experiment | Why |
|---|---|
| E06 | PageRank MRR (Mean Reciprocal Rank): -22% — BM25 simpler and better |
| E07 | Structural context token: -13% — adds noise to K-NN vectors |
| E13 | Method K-NN: 0% — methods too short to embed meaningfully |
| E22 | DatasetRouter: +1% macro_f1 — only 3 languages in Tier 3, needs TS+PY |
| E24 | AttributedRouter: -37% — noise ratio increases with attribution |
| E28-E31 | ContinuousLearning: bigram (B-gram) cannot show gradient signal |
| E33 | StopSignalDetector: 0% after self-healing fix — trivially solvable data |
| E36 | FeedbackCorrector: 0% — pattern matching too rigid |
| E37 | MergerSynthesizer: 0% — all candidates equal quality |
| E40 | TrustScorer: 0% — seed-42 collapse; fixed in E40V2 with StratifiedSplit |
| E42/V2 | DiskManager: knowledge_retention=1.00 baseline, no room to improve |
| E44/V2 | DatasetWeighter: -98% with ×10 constant; needs gradient proxy |
| E45/V2 | CurriculumOrganizer: 0% — curriculum order indistinguishable at Tier 2 |
| E46 | ConfidenceGate: 0% — data too separable, both thresholds achieve 1.00 |
| E47 | ModelHubSearcher: -8% — exact tag match beats BM25 on overlapping queries |
| E48 | DomainBootstrapper: 0% — warm start = cold start at Tier 2 |

## Key architectural findings

- **E20 CRITICAL:** StackDetector MUST run before E15 normalization — markers destroyed otherwise
- **E44 CRITICAL:** max_weight = total_examples / (num_classes × rare_examples) — constant ×10 caused forgetting
- **E40:** StratifiedSplit required for all experiments with fixed random seed
- **E34V2:** BM25 + K-NN fusion beats either alone — D-10 in DECISIONS-LOCKED.md
- **E33:** Self-healing distribution can flip PASSED→FAILED if both arms reach 1.00 trivially

## Пометка 31.10.2026 — память переходов E28 разделена по проекту

Перенос измеренного результата опыта 72 «условие проекта в памяти переходов»
(отчёт `experiments-v2/RP01-MICRO/P7201/results/attempt-0001/report.md`:
экзамен 3 из 4 → 4 из 4, контроль уроков 9 из 12 → 12 из 12, приобретено 4
строки, потеряно ноль, цена 46 байт состояния и 1 224 байта выделений на
обучении). Строки таблиц выше не менялись.

В `35.ExperimentsV2/Experiments/E28ProjectAdaptor.cs` изменены три вещи:

1. извлечённые пары предикатов несут имя своего проекта — раньше
   принадлежность файла проекту терялась до построения памяти;
2. ключ памяти переходов стал составным «проект плюс текущий предикат» —
   общий ключ складывал наблюдения двух проектов в одну запись и на запрос
   одного проекта возвращал дословно закон другого;
3. при отсутствии записи ответ больше не совпадает с законной меткой 0
   («проверка на пустое»): признак «запись найдена» возвращается отдельно,
   поэтому отказ отличим от предсказания.

Следствие для показателя самого опыта E28: «настроенная» память проекта A
больше не отвечает на запросы проекта B его чужим законом, а уходит на
запасной путь и получает закон своего проекта, поэтому прежняя разница между
плечами на этом входе исчезает. Прежняя разница и создавалась этим
столкновением ключей. Назначение реального признака проекта независимо от
порядка списка файлов — отдельный подслучай карточки P72 («проверка label
construction»), он не измерялся и этим переносом не выполнен.

Постоянное покрытие: `Tests/ExperimentFixes/ProjectKeyedTransitionMemoryTests.cs`.

## Пометка 31.10.2026 — текущее действие вернулось в адрес памяти перехода

Перенос измеренного результата опыта «адрес памяти кэша предсказания следующего
предиката» (номер опыта P47-PRIMARY-01, подслучай P47.1; отчёт
`experiments-v2/RP01-MICRO/P4701/results/attempt-0001/report.md`: экзамен
4 из 4 → 4 из 4, учебные контроли 8 из 12 → 12 из 12, разрушительных коллизий
адресов 4 → 0, потерянных верных строк ноль). Строки таблиц выше не менялись.

Что было не так. Адресом памяти служил только портрет файла — доли видов
действий в этом файле. Портрет один на весь файл, а вопросов к файлу много:
«что идёт после проверки на пустую ссылку?» и «что идёт после возврата
значения?» попадали в одну строку памяти. Записывалось несколько ответов, а
отвечала всегда самая ранняя запись, потому что при равной похожести чтение
оставляет ту, которую встретило раньше. Остальные записи физически лежали в
памяти, но до них никогда не доходила очередь.

В `35.ExperimentsV2/Experiments/E30PredicateCache.cs` изменено одно:

1. адрес памяти строится теперь на КАЖДУЮ пару «текущее действие и следующее»,
   а не один на файл: к портрету файла (22 разряда) добавлен блок текущего
   действия (22 разряда), всего 44 разряда. Предсказываемое действие в адрес не
   входит ни в каком виде, иначе память читала бы ответ из самого вопроса;
2. ширина блока — 22, а не 6 из набора опыта: в опыте данные содержали только
   номера 0-5, а разборчик кода нумерует предикаты от 0 до 21. Блок шириной 6
   молча терял бы старшие номера. План прямо запрещает переносить «6» как
   универсальный размер словаря предикатов продукта;
3. номер текущего действия вне диапазона 0-21 оставляет блок нулевым, но
   считается и попадает в заметки результата опыта отдельным предупреждением —
   молча такие пары не отбрасываются.

Порог похожести 0.85, мера расстояния, сам `PredicateCache` (запись, чтение,
сборка портрета файла) и общий механизм приближённого поиска соседей не
менялись: файл `35.ExperimentsV2/Infrastructure/PredicateCache.cs` остался
побайтово прежним.

Статус переноса — **CONDITIONALLY_PRODUCT_BINDING**, словами: это исправление
экспериментального потребителя. Переход к продукту требует назначить реального
потребителя и показать его цепочку обучение → адрес → поиск → ответ. Пока такого
потребителя не найдено, нельзя писать «все механизмы поиска по соседям
исправлены». Из названия файла в проекте 21 продуктовый дефект не следует.

Чего перенос не доказывает. Новый адрес нельзя выдавать за продолжение старой
памяти с тем же идентификатором схемы: при смене схемы нужна согласованная
перекодировка сохранённых уроков либо явно несовместимый формат, и опыт P47 не
доказал загрузку этой структуры в новом процессе (сохранения состояния у этой
памяти нет вовсе). Межпроектная изоляция не доказана: похожесть чужих адресов
0.833333 при пороге 0.85, то есть разделение держится на пороге, а не на
устройстве адреса. Собственный показатель опыта E30 на настоящем наборе файлов
после этой правки не перезапускался, поэтому направление его изменения не
измерено.

След для прежних доказательств: опыт P47 подключает этот файл напрямую по его
продуктовому пути и сверяет его отпечаток перед прогоном. Отпечаток изменился
(было `291784f0af4fce68…`, стало `fdbf17d530779f9b…`), поэтому повторный прогон
попытки attempt-0001 на этой сверке остановится — это ожидаемо, а записанные
результаты той попытки остаются историческим доказательством. Файлы опыта не
менялись.

Постоянное покрытие: `Tests/PredicateTransitionAddressLearningTests.cs` (девять
проверок на точных данных опыта плюс проверка подключения в настоящем пути
продукта). Журналы четырёх прогонов — в
`experiments-v2/_runners/FIX-E30-PREDICATE-ADDRESS-attempt-0001/`.
