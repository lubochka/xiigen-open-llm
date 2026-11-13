# RP01 — источники, архитектурные роли и функциональная проверка

Редакция 15.10.2026. **Source/requirement map, не новая очередь исполнения.** [Dispatcher](RP01-EXECUTE-NOW.md), [прочитанный code snapshot](RP01-CURRENT-CODE-MAP.md), [научная методика](LEARNING-EXAMINATION.md). Исторические code findings 13–14 октября — кандидаты для сверки, не приказ заново исправлять нынешний код.

## Где искать исходное требование

P=`lubochka/xiigen-planning:llm_plan3`; K=`lubochka/llm_mvp_core:codex/dpo-implementation`. В P: D=`llm mvp/design to code session`; G=`llm mvp/dpo legal build`; F=`llm mvp/llm dynamic flow`; V=`llm mvp/visual studio & visual studio code continue`. G/F/V links нельзя разрешать относительно D по умолчанию.

| Source family | Предметное использование | Ограничение чтения |
|---|---|---|
| D original task/task prompt/goal/Алгоритм создания ДПО/Изменения в плане работы | Исходные функции, человеческая цель, DPO и delivery | Читать consequential раздел своей задачи; не объявлять все файлы прочитанными по списку |
| D WorkLeft-02/MECH-01/RESEARCH-03 и другие WorkLeft/LeftToWork | Существующие training/DPO решения, gaps, state/rights/algorithm conformance | Research baseline переиспользуется; старый status не автоматически current fact |
| D WorkLeft-RAG MASTER/usage/MD+NDJSON/DELTAS | Навигация до конкретного G/F/V/K primary | INDEX_ONLY отличается от PRIMARY_READ; фиксировать настоящий span и его следствие |
| S00–S10/Original-Slice-Queue, S06 LEARNING-ROLE-CONTRACT, extras | Не потерять роли, DTC/IDE и другие product expectations | Whole-program remaining source debt остаётся у Rxx owners, не перед каждым E1 |
| G original-task, plan-01/02/03/04/06/07/09 и DPO guides | Unit/Flow/orchestration/graph/DPO/continual первичные решения | Проверять actual path/версию; July labels не универсальные October grants |
| F DECISIONS-LOCKED/ARBITER-KNN-AUTHORITY-AMENDMENT/DYNAMIC-FLOW design | Domain laws, budget, pre-generation memory и post-generation acceptance | Similarity threshold не универсальная probability; failed memory сохраняет negative/warning роль |
| V RAG-06/LeftToWork-CONTINUE/PLAN-VS-VSCODE-CONTINUE | Ordinary user/IDE/session/state delivery | Не считать mock/internal evaluator настоящим IDE/ordinary consumer |
| D previous rounds/join/R01–R25 results/R18/R22/R23/R24/R25 | Переиспользовать исследования, causal comparisons и owner constraints | Не выполнять их заново и не менять чужие statuses этим RP01 repair |
| K actual instructions/skills/code/interfaces/callers/consumers | Что действительно работает и где вмешательство | Читать нужный current source до проверки/патча; source trace не runtime receipt |

## Четыре уровня — требования, а не четыре новых trainer

| Пользовательская роль | Обязательная функция | Соответствие текущему коду и probe/экзамен |
|---|---|---|
| ILearningUnit | Domain + input-vector schema + requested output schema; typed many→many с ассоциациями и query semantics | Actual `ILogicalUnit<TInput,TOutput>`/request/result; проверить payload/resolver/identity, а не список IDs. Отсутствующий IQueryable bridge — явный RP03 gap |
| ILearningFlow | Отношения Units и обучаемая коррекция output→следующий input | Named correction state/owner; fixed Unit vs changed Flow, downstream law-correct result и сохранённые associations |
| ILearningOrchestrator | Async input→output через допустимые dependencies/combinations | Safe deterministic action space отдельно от learned branch/order policy; все async branch costs и side effects учитываются |
| ILearningDynamicGraph | Отношения паттернов разных порядков и перенос на новые композиции | Named higher-order learned state и actual selection consumer; static closure/число edges не доказывает обучение |

Прочитанный `K/00.Core/LearningMachineContracts.cs` размещает Train на ILearningMachine, не на каждом Unit/Flow. `LogicalContracts.cs` возвращает Task/OperationResult/LogicalUnitResult, не обещает готовый IQueryable API. Не переименовывать existing interfaces по опечатке и не объявлять requested semantics уже реализованными без bridge/consumer evidence. [Подробный Unit/Flow handoff](tasks/execution/RP01.04C.md), [Orchestrator/Graph](tasks/execution/RP01.04D.md).

## Каждое функциональное ожидание имеет механизм и способ проверки

| Ожидание пользователя | Задания/владелец | Как отличить действительный результат от видимости |
|---|---|---|
| Быстро достичь первого настоящего Student урока | 00B, 02A/02B, conditional B0, 03A | Existing state reuse; один train-and-hold; actual optimizer/journal/allowed tensor evidence отдельно от gain |
| CODEX выбирает, Chinese создаёт positive и negative, CODEX оценивает | S/A/J | Actual отдельные назначения CODEX Astra 6 UltraCode; обе стороны Chinese; exact bytes; meaningful law preference; TIE/BOTH_BAD/UNJUDGEABLE не forced PASS |
| DPO не теряет semantic contrast | Conditional 01A/01B | Правильные raw/framing/response targets и response-relative objective в actual trainer; полезность далее независимый T2 |
| Правильно определить domain/intent/stack/version | 04A→RP02 | Oracle-vs-inferred diagnostic, mixed/unknown strata, confusion и downstream path errors |
| Предметные tokens/vectors и LoRA projection | 04A→RP02 | Schema/feature/coordinate/version→actual consumer; LoRA state не автоматически semantic codec; heldout law preservation/collisions/downstream quality |
| Dynamic kNN учитывает законы, POS/AST/referents | 04B→RP02 | Evidence roles + context law + learned reranking + contextual k + typed fusion; fixed vs learned/dynamic, exact vs ANN, actual output, не hits count |
| Сочетание deterministic и learned механизмов полезно | 04E и соответствующие RP02–RP05/RP09 | One-factor contrast, затем Student×policy 2×2; hard guards неизменны; deterministic lawful baseline может быть лучше |
| Typed Unit many→many и Flow correction | 04C→RP03 | Multiple/reordered/partial outputs сохраняют relations; fixed Unit/changed Flow действительно меняет нужный downstream input |
| Асинхронный Orchestrator и higher-order Graph | 04D→RP04 | Learned state/signal/update/consumer; unseen compositions и все branch costs; no silent retry of irreversible effect |
| Учиться от новых user inputs без забывания | 03B→RP05/RP09 | Проверенный feedback, новые pair/source families, supported descendant owner, chronological before/after и old-domain retention |
| Компактная система действительно работает на PC | 04E/03B→PC owner | Whole concurrent process tree, base/reference/optimizer/index/cache/disk, latency/external/failure cost; не размер adapter |
| Обычный пользователь получает принятый результат | T3.P/T3.C, conditional 01C | Same held weights published, new ordinary process served identity/output; source-only path и remote pointer не заменяют adoption |
| C#, SQL и EN→RU, плюс исходные DTC/Angular/IDE задачи | 02A/04A–E/05A + original slice strategy | Раздельные domain-law exams/real consumers; missing owner/consumer остаётся явным unresolved gap, не закрывается C# E1 |

Матрица обеспечивает traceability **плана**, не утверждает реализованность всех функций. Получатель handoff должен быть конкретным actual task/owner; если он не найден, это вывод данного handoff и ограничение claim, а не разрешение придумать RP номер.

## Прямо прочитанные дополнительные источники этой редакции

На K snapshot `97e4af3eadd3981320f698d104134a01984a2542`:

- `tools/Rp01RawDpoLesson/README.md`: actual author/reopen/train-and-hold/publish-held route и source/rights boundaries.
- `26.TrainableRouter/Training/CodeDocumentationScientificTrainingLifecycle.cs`: intent before effect, hold, publish-only, exact raw answers и scientific/native verdict validation. Publication reuses saved Trial, не делает optimizer/generation.
- `CodeDocumentationScientificTrainingPolicy.cs` и `25.ModelServer/ContinuousLearning/CodeDocumentationScientificTrainingConfiguration.cs`: strict bindings, existing 12×5/+2/critical=0 и startup hashes; operator trust не равен cryptographic model attestation.
- `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs` целиком: token/predicate count tiers, safeK≥1, output из hits[0], остальные evidence IDs, confidence warning. Это один example-memory consumer, не вся next-token/grammar система.
- Начальные substantive `00.Core/LogicalContracts.cs`/`LearningMachineContracts.cs`: actual execution contracts и machine-level training boundary.

Primary F/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md подтверждает сохранение negative/warning memory и two-level arbiter authority. Прочитаны relevant WorkLeft-02/RAG-01 sections и previous rounds/join, не все WorkLeft/G/F/V документы. По assigned source spans tasks уточняют ещё не закрытые вопросы.

## Математика и репрезентации

[01B](tasks/execution/RP01.01B.md) содержит standard DPO objective/sign/weight interpretation со ссылкой на первичную работу; выбранная alternative objective требует явного contract. [01A](tasks/execution/RP01.01A.md) различает raw identity и tokenization boundaries: joint BPE не обязана совпадать с раздельной конкатенацией encodings. Не менять tokens/framing/side bytes после J ради фикстуры.

E2 цепь: raw→stack/domain/version/context→semantic representation→rights-compatible positive/negative/warning memory→context-law output candidates→trained ranking/contextual k→typed consumer→final arbiter. Ошибочный пример может быть ценным warning, но не accepted output. Domain/policy изменения не смешиваются с Student-only C0→Trial; separate interaction измеряется явно.

## История и граница выполненного

Полные прежние source anchors/13 октября observations доступны [в immutable snapshot](https://github.com/lubochka/xiigen-planning/blob/de19e6f25264c7afc353345253d2e249b449b080/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/SOURCE-AND-ARCHITECTURE.md), unique originals не удалены. Current source важнее historical bug label; original requirement важнее convenience-refactor.

Изменены только planning документы. Реальные learning, CODEX/Chinese calls, PC experiments, независимая review acceptance и ordinary adoption не выполнены этой редакцией. No full-repository audit или весь R01–R25 source closure не заявляются.
