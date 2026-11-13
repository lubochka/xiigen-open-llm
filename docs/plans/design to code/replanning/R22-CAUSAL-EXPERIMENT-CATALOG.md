# R22 — причинные эксперименты гибридного обучения

**13.10.2026 · RCOR22-R2 · исправленный проект экспериментальной программы.**

Нужно выяснить, какое изменение действительно помогает системе решать новые задачи и сколько оно стоит на ПК. Сначала связываем один настоящий урок с существующим Student и обычным ответом. Одновременно готовим независимые предметные сравнения: распознавание домена, представление смысла, соседство, связи и выбор пути. Не ждём, пока все слайсы или все исследователи получат DONE.

**Статусы:** `EDITORIAL_REPAIR / DESIGN_CANDIDATE / DESIGN_BINDING_REQUIRED / RUNTIME_BINDING_OPEN / LEARNING_NOT_MEASURED / INDEPENDENT_ASTRA_REVIEW_NOT_PERFORMED`. Этот документ не запускает обучение, модели, тесты или Windows/Actions. Примеры ниже — `DOCUMENT_EXAMPLE`, не Teacher corpus и не результаты Student. Незавершённые проектные решения не называются «только runtime IDs».

Предыдущая редакция сохранена в Git history: planning commit `e96a727053f6153455e979bf76ab4714d8728aa2`, этот путь, blob `548fc1bb88744fb9405c274ef450b5a282cc80c6`. Текущая редакция заменяет её действующие указания, сохраняя E1, C01–C11 и предметные обязательства. Исторические source receipts остаются в [SOURCES](results/R22/SOURCES.csv); исправления объяснены в [FINDINGS](results/R22/FINDINGS.md).

## 0. Полученные смысловые изменения, а не ещё одна очередь

| Прочитанный источник на P=e96a727053f6153455e979bf76ab4714d8728aa2 | Что R22 принимает и действительно меняет |
|---|---|
| [R21 FINDINGS](results/R21/FINDINGS.md), blob `ee3329f6ea4e9310e9b3e8e14c86acd43fbe89da`; [DOMAIN-EXPERIMENT-RECIPES §9](DOMAIN-EXPERIMENT-RECIPES.md) | R21 **опубликован**, а не отсутствует. C# — первый Student; EN→RU — второй структурно другой предмет. Семь паспортов — проектная программа, не готовые корпуса/ordinary endpoints. E2 может идти независимо от E1. |
| [R18, редакция 2](R18-FIRST-LEARNING-IMPLEMENTATION-WORK-ORDER.md), blob `cca00c57bce7d0f66e5dc0e25d4c74ebc95d5660`, §§0–7 | Повторно используем A–D; `owner-input-verbatim-v1`, versioned material/objective, shared generation facade. Все learning arms исполняют один исправленный pipeline. G5=consumer, G6=environment/review; предварительные условия отделены от последующих наблюдений. |
| [R13 FINDINGS](results/R13/FINDINGS.md), blob `bcc820e0615f2b86fbdfa8cf0764220c334abd7a`, §§2–7 | E1 recipient уже условно выбран. Нужны отдельно prospective effect/noise/N/budget decisions и отдельно actual runtime bindings. Isolated utility, promotion и ordinary adoption — разные verdicts. Общий REPAIR_REQUIRED не запрещает независимую подготовку. |
| [JOIN](PARALLEL-R01-R03-JOIN.md), [PARALLEL20](PARALLEL-R01-R20-CONTRACT.md), [COMMON](run-prompts/COMMON.md) | R01–R20 имеют независимые старты; R10 получает девять исследовательских snapshots. Shared writes последовательны. R22 не переписывает sibling results и не объявляет их завершёнными. |
| [Предыдущие раунды](previous%20rounds%20thinking%20results.md), opening/R07 correction | S04-only и чтение названий RAG не whole-project audit. Сохраняем исходные решения, указываем фактическую глубину чтения и остаток; не считаем новый prompt выполненным исследованием. |

P=`lubochka/xiigen-planning:llm_plan3`; D=`llm mvp/design to code session`; G=`llm mvp/dpo legal build`; F=`llm mvp/llm dynamic flow`; V=`llm mvp/visual studio & visual studio code continue`; K=`lubochka/llm_mvp_core:codex/dpo-implementation`. K HEAD повторно наблюдён как `3c7f5f8c7809936e25579603c96ea6df8598fc40`. Это не разрешение откатывать ветки. Перед зависимым действием проверить нужные blobs, не сбрасывать работу из-за чужого RUN-LOG.

Прямой маршрут данного repair: D/original task → WorkLeft-RAG-MASTER-INDEX → WorkLeft-02 → current R18/R13/R21 → назначенный эксперимент. Более широкие MECH/RESEARCH-03, F/V/G и code traces старого R22 — **унаследованные/атрибутированные**, пока соответствующий span не прочитан новым исполнителем. Полного обхода библиотеки здесь не заявлено. Сохраняются original/task/goal, WorkLeft/LeftToWork, RAG MD/NDJSON, S00–S10/extras, DPO/DynamicGraph, DTC и IDE. Семь предметов не заменяют всю продуктовую программу.

## 1. Что именно обучается

| Роль | Детерминированная часть | Отдельное обучаемое состояние и наблюдаемый результат |
|---|---|---|
| **ILearningUnit** | Domain/input-vector/output schemas, допустимые типы, stable input/result association IDs | Mapping/ranker/adapter конкретного Unit. Typed many→many должен сохранять смысл и соответствие входов/выходов, в том числе при reordered/partial results. |
| **ILearningFlow** | Порты, OutputRefs resolution, lifetime/version и законность коррекции | Relation/correction state между Units. При frozen Units изменённая связь должна улучшить downstream input/result. |
| **ILearningOrchestrator** | Dependencies, права, допустимый порядок, cancellation и общий бюджет | Policy выбора lawful async combinations/ветвей. Измеряем конечное решение и стоимость всех запущенных ветвей, не только победителя. |
| **ILearningDynamicGraph** | Типы и порядок паттернов, допустимые ребра, semantic OrderedPatternIds | Higher-order relation/pattern state. Проверяем новые композиции знакомых примитивов, не запоминание целого маршрута. |

Train остаётся в существующей ILearningMachine infrastructure; Graph — L5 specialization, не шестой уровень и не четыре новых trainers. Имена ILogical*/Machine сопоставлять поведением, не создавать API из опечаток original task. Сохраняются TensorCore/shared base/project LoRA, StackDetector-before-normalization, semantic compression, BM25+kNN, positive/negative/warning memory, final arbiter, обязательные skills вне top-k и source-bound ограничения helpers.

Для каждого claim обязателен путь: **domain/law → representation → lawful candidates → learned owner/state/signal → output/association → ordinary consumer → вмешательство/контроль → independent outcome → полная цена**. Отсутствующий actual owner/caller — конкретный OPEN seam. E1 не доказывает обучение Flow, Graph, mapper или kNN. Детерминированный forward над выученными весами может быть обученной функцией; случайность сама по себе не обучение. Правильный deterministic ответ может быть chosen.

## 2. Как модель получает задание

[run-prompts/R22.md](run-prompts/R22.md) — действующий вход. Dispatcher собирает **содержимое**, а не список ссылок: COMMON один раз для planning + полный numbered prompt + этот общий контракт + ОДНА полная Cxx/E1 card + актуальный own checkpoint + необходимые primary/code/skill excerpts. Пользователь не склеивает книги. У ребёнка нет самостоятельных GitHub/shared-file прав; он возвращает packet parent.

**Режим обязателен.** Сейчас PLANNING: только назначенные документы R22. Для будущего implementation/read-only inference/teacher/update/adoption нужны конкретные отдельные права; нельзя одновременно передать запрет всех core effects из planning COMMON и приказ исполнить optimizer. GitHub write permission не training grant. Не выдумывать CLI, model/profile, checkpoint, parameter keys, корпус, доступную память или фактические model IDs.

**Общий полный child contract:** CURRENT TASK; NOT THE TASK; mode/effect authority; original requirement; current→target mechanism; exact context and RAG route; loaded skills; actual/proposed owner/state/caller; hypothesis; one changed factor/frozen factors; law/family/splits; material and state versions; good/bad/edge; output; documentary acceptance; future outcome/stop/recovery; allowed/forbidden files/actions; source/read-depth/limitations; actual invocation count/identity or NONE. В каждом Cxx ниже содержательная часть этого договора заполнена. Перед вызовом dispatcher подставляет прочитанные фрагменты и настоящие bindings; пока их нет, это design task, не RUN_READY.

**Skill packs** — K/docs/skills, полные filenames:
- **S-ARCH:** `trainable-architecture-decision-SKILL.md` — role/state/authority и полнота; `code-examination-SKILL.md` — caller/consumer и полный affected-file read перед code diff.
- **S-PLAN:** `phase-planning-SKILL.md`, `plan-review-SKILL.md` — scope, exact-candidate review, design versus execution dependencies.
- **S-PROMPT:** `prompt-patch-authoring-SKILL.md`, `output-readability-gate-SKILL.md` — конкретный WRONG/CORRECT, понятный результат и границы evidence.
- При разработке измерений адресно загрузить `experiment-design-SKILL.md` и `improvement-measurement-SKILL.md`; существование ссылки не считать загрузкой. Исторические blanket 30/50-pass/coverage правила не отменяют текущие 25 assignments и запрет новых тестов для R22.

**Context packs** дополняют, не заменяют primary:
- **Q1 material/Student:** D/original task; WorkLeft-RAG-MASTER→WorkLeft-02; R18 §§3–6; R13 §§3–8; K `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`, `11.DPO/DpoTrainer.cs`, `11.DPO/Models/DpoBatch.cs`, `01.Tokenizer/BpeTokenizer.cs`, `10.LoRA/LoraApplicator.cs`, `03.TransformerModel/LlmModel.cs`; actual execution/adoption callers из R18.
- **Q2 domains/neighbors:** R21 recipes §9, R04/R06/R08 own findings с их limitations, R12/R13 domain/representation/kNN sections; source laws для выбранного предмета; K `00.Core/LogicalContracts.cs`, `00.Core/LearningMachineContracts.cs`, `06.VectorSearch`, `08.GraphRag`, `17.DynamicDomains`, `26.TrainableRouter` и найденный actual consumer. Directory — маршрут, не прочитанный implementation.
- **Q3 connections:** original roles; WorkLeft MECH-01/RESEARCH-03 по вопросу; full relevant R07/R12 result; R03 route в F/V/G до primary authority; K Logical/LearningMachine contracts, `15.OrchestratorModel`, actual Flow/OutputRefs/graph selection/preload/execution. Не объявлять перечисленный маршрут новым самостоятельным чтением.
- **Q4 examination/cost:** R13 current findings, R17 evidence policy, R18 §§5–6, R21 §9; sufficient R09/R23 cost evidence и R24 ordinary-product requirements. Missing sibling result ограничивает зависимый binding, не весь R22.

Перед patch читать полный меняемый code file и callers/consumers на actual ref. Старый snippet не даёт code-change readiness. Перед плановой записью использовать fresh SHA; при конфликте сохранить чужую дельту и перечитать затронутые sections. После записи remote readback.

## 3. Первый опыт: E1-CODEDOC-CONDITIONAL-DPO-v2

### 3.1. Заполненная предметная задача

**Промпт исполнителю:** «Определи, помогает ли один новый разрешённый DPO урок существующему CodeDocumentation Student точнее объяснять C# nullable/control-flow на других source families. Используй R18 A exact material, B response objective/installed LoRA, C ordinary facade и D измерение. Не создавай trainer и не начинай с глобального tokenizer/Graph rewrite. До эффекта зафиксируй допустимый baseline, сравнимые conditions, profile и права; затем исполни только разрешённую стадию §3.4. Верни фактический результат, включая отсутствие эффекта, без подгонки уроков, рубрики или повторного gradient». Skills S-ARCH/S-PLAN/S-PROMPT; context Q1+Q4.

**Вход/выход:** exact OwnerInput `x` с предметными условиями, существующий server-owned Student state → обычный typed ответ с RequestId/association и фактической model identity. Выбранный framing R18 — `owner-input-verbatim-v1`: скрытое metadata не считается conditioning. Сохранить routing API; stages/run_id не выдавать за сгенерированный ответ. Proposed facade и methods из R18 остаются proposed до реализации.

**Предметные страты:** nullable reference access; nullable value returns/chains; coalescing counterfactual inputs; parentheses/getter-exception boundaries. Ранее освоенные CodeDocumentation families — retention. Поддерживаемая C# version и source law фиксируются; nullable warning не runtime guarantee.

**Пример:** для `int? Size(string? s) => s?.Length;` существенный факт — null при null input. Ответ «возвращает 0» содержательно хуже. `s?.Length ?? 0` — ДРУГОЙ x, не отрицательная сторона прежней пары. Две верные формулировки могут быть TIE. Это пример автора плана, не допущенная Teacher-пара. Края: UNKNOWN source, empty result, valid alternative, значимое отличие после 480 символов, changed code version, interrupted attempt.

### 3.2. Контроль, который отделяет исправление pipeline от обучения

- **O (необязательная историческая диагностика):** старый pipeline/старые weights. O→T смешивает изменение кода и обучение и НЕ является основным learning comparison.
- **P/N:** допустимый исходный state, accepted либо явно разрешённый clean baseline, уже исполняемый исправленным v2 pipeline. Именно N — baseline для causal Δ.
- **C0:** read-only экземпляр того же N, без optimizer, с той же доступной memory/context/cache policy; контроль hidden mutation и noise.
- **T/U:** одно разрешённое применение новой пары от N, тот же v2 pipeline, tokenizer/framing, corpus, mapper, k/radius, ordinary generation/evaluator settings; сохранённый и вновь открытый trial.

Основной Δ=N→U; C0 не второй trained clone. Не запускать один admitted pair ещё раз в A/B клонах. Сравнение material/objective fixes — отдельный mechanism вопрос. Предпочтительная пара, исправленная подготовка, changed weights и Student utility имеют разные результаты.

### 3.3. Смысл DPO, получатель и восстановление

CODEX в фактически разрешённой конфигурации GPT-6 Astra UltraCode выполняет человеческий выбор учебного case. Разрешённая внешняя открытая китайская модель создаёт ОБЕ финальные стороны при одном x. CODEX отдельно оценивает exact final pair/context: preference, pair usability, law reasons и TIE/BOTH_BAD/UNJUDGEABLE. Weak Student answer — возможный разрешённый контекст, не автоматически rejected. Judge не переписывает стороны под прежним authorship. Изменённые bytes/context требуют новой оценки и действующего допуска. Mathematical reference не judge; post-update evaluator не Teacher.

В выбранной R18 v2 для каждой response: `s_theta(y|x)=sum_t log p_theta(y_t | x,y_<t)` по actual response target mask. DPO: `L=-log sigmoid(beta*((s_theta(y+|x)-s_ref(y+|x))-(s_theta(y-|x)-s_ref(y-|x))))`. Forward и backward используют одну reduction/маску/scale; prompt остаётся conditioning, padding не target. Public average scorer не заменяется глобально. Framing, segmented BPE, real stop token/length policy и версии берутся из R18, не выводятся из последнего vocab ID.

Это objective для выбранного probabilistic Student. Произвольный kNN distance не объявлять log probability; scorer/ranker target для других блоков определить отдельно. Zero policy/reference initial margin не означает плохую пару. Equivalent raw sides, meaningful contrast destroyed by preparation и zero initial margin различны.

Только actual installed project-LoRA key allowlist входит в optimizer; base/reference frozen и storage-independent. LoRA сама не tokenizer/ResultProjection; нулевой update отдельной матрицы не автоматический дефект. Фактические keys, optimizer membership, режимы/RNG и objective continuation проверяются до mutation. Не менять beta/LR/rank по экзамену. Нет silent Compact/whitespace/480 cap или empty-token [0] вместо смысла. Refuse несовместимый материал до effect.

Одна допустимая новая pair identity → не более одного разрешённого update. Persist attempt/parent/trial, полное state/recovery. UNKNOWN outcome → существующий recovery, не teacher retry/новый UUID/второй gradient. Restore включает weights/adapters/optimizer/counters/RNG и связанные versioned index/cache state. `NO_TRAINING_NEEDED` допустим при достаточном baseline по действующей policy; это ноль произошедших уроков.

### 3.4. G1–G6 и стадии — без круговых prerequisites

**Используются значения R18 v2, а не прежняя перестановка G5/G6 в R22.**

| Группа | До соответствующего эффекта | После эффекта |
|---|---|---|
| G1 baseline | Model/domain/project/config/tokenizer; accepted selection или clean missing + explicit cold-start authority | Loaded verified state receipt. Не изобретать accepted manifest для initial root. |
| G2 recipient/state | Actual trainable keys, frozen/ref separation, optimizer continuation, journal/recovery | Actual update и state delta. Не требовать выполненный gradient для допуска самого gradient. |
| G3 effects | Реальные разрешённые profiles/grants/rights/expiry и соответствующий call budget | Request IDs/returned bytes после author call; отдельная exact assessment до optimizer. Future request ID не prerequisite authoring. |
| G4 examination | Family partitions, law/rubric/N/effect/noise/retention/repeat/stop rules до candidate outcomes | Scores/uncertainty/verdict. Missing design rule не «только runtime ID». |
| G5 consumer | Implemented shared input/facade; permissible read-only baseline/isolated trial; parent ordinary behavior до update | G5-post: cold accepted/retained adoption после verdict. Его не требуют до G5-pre. |
| G6 environment/review | Actual permitted host, conservative whole-PC allocation, applicable native property и required exact-candidate review | Measured peak/cost и независимые actual receipts после соответствующих действий. |

Порядок: planning/code design → отдельно разрешённая read-only inspection → ordinary baseline → отдельно разрешённые authoring/assessment → одна mutation → isolated examination → authorized accept/retain → cold ordinary adoption → отдельная remote/native release. Preconditions привязаны к стадии. A/C design не ждут будущие scores; authoring не ждёт собственный response ID; mutation не ждёт post-adoption; E2 preparation не ждёт E1 DONE. Existing required checks не отключаются, но новый Windows run/coverage не learning gate.

### 3.5. Конечный проект измерения, а не «метрики определят потом»

**Действующее approved noise/quality profile S04 имеет приоритет.** Его existence/values пока не подтверждены этим R22. Ниже — конкретный **PILOT-P12 proposal**, не измеренный корпус, не allocation и не owner-approved product threshold. R13/R18-D выбирает/согласует его ДО candidate outcomes либо приносит уже утверждённый профиль; R23 согласует фактический resource cap. Это bounded design-binding task, а не разрешение выбирать правила после результата.

Предлагаемый первый pilot: 12 независимых C# source families, по 3 на каждую из четырёх страт §3.1; в каждой 2 случая, ordinary и boundary. Итого 24 случая, не 24 независимые семьи. Один заранее фиксированный generation seed/settings для P,C0,T: 72 outputs. Четыре заранее выбранных boundary cases получают ещё два seeds во всех трёх arms: ещё 24 outputs, **максимум 96 ordinary generations**, без повторного обучения. Если budget не допускает это, profile меняется/утверждается до candidate outcomes; урезанный sample не наследует более широкий claim.

Учебный material — одна новая семья вне панели; максимум один authoring attempt и один admitted update в этом pilot. TIE/invalid/no lawful pair завершает попытку без автоматического нового вызова. Teacher selection, author, pair judge, post-update evaluator и любая adjudication получают отдельные finite call/token/cost limits из actual grant; 96 — только предел ordinary generations, не скрытый общий API budget. Финальный независимый examiner имеет отдельный scope/grant.

Первичная величина: доля случаев, где **все заранее выписанные essential source claims верны, нет критической law error и ответ соответствует requested output**. Сначала усреднить внутри family, затем равно по families; missing/abstain/length-limited outputs видимы в denominator и отдельно по причинам. Вторичные: missing/false claims, mapping errors, latency/resource и retention. Legitimate unknown/OOD задания оцениваются по собственной объявленной rubric, а не исключаются после результата.

Pilot reports paired family deltas, wins/ties/losses и разброс повторов; он **не подтверждает** семь доменов, population mastery, надёжный p95 SLA или 95% product goal. Небольшой положительный сигнал не превращается в статистически подтверждённое улучшение. Общие verdicts сохраняются; при недостаточной точности — INCONCLUSIVE, с отдельно описанным exploratory signal.

**Отдельный proposed confirmatory профиль, не prerequisite подготовки pilot:** одна первичная гипотеза; не менее 40 новых независимых target families; frozen family-level paired analysis, например stratified cluster bootstrap 10 000 resamples и 95% interval; practically meaningful target +5 percentage points; lower effect bound >0; critical-law violations недопустимы; protected retention margin и реальный cost envelope фиксируются owner до просмотра trial. Эти числа — проверяемое предложение R22, НЕ научно гарантированная достаточность N и НЕ найденные требования владельца. Baseline/noise-driven power/precision assessment может потребовать иной N/метод; degenerate interval/малое число независимых families не доказательство certainty. До выбора actual retention margin, meaningful effect, precision и budget статус остаётся DESIGN_BINDING_REQUIRED. Одна конечная analysis look; adaptive intermediate panels — operational, не untouched confirmation.

**Acceptance данного design task:** есть выбранный prospective профиль со всеми policy fields и rationale, либо точный owner-bound decision residue, не пустое «R24 решит». Нельзя назвать эксперимент execution-ready по одному этому предложению. Дальняя source goal 95%+ сохраняется, но не становится coverage/условием первого урока.

## 4. C00 — сквозное сравнение комбинации deterministic + learning

**Новый contrast `R22:C00` дополняет, не перенумеровывает C01–C11.** Он не дополнительный R26 и не команда запустить все arms до первого урока.

**Prompt:** «На одном выбранном law-defined domain и одинаковых доступных сведениях/бюджете сравни D — фиксированное решение/выбор, L — обученную policy без необязательного deterministic solver/corrector, H — их комбинацию. Общие права, типы, hard laws и final arbiter обязательны во ВСЕХ arms. H+Graph — только отдельное обоснованное расширение, если higher-order consumer существует. Назови реализованные версии и недоступные arms. Не подменяй отсутствующий baseline заведомо слабой игрушкой». Context Q2/Q3/Q4, skills S-ARCH/S-PLAN.

Learned-only здесь означает отсутствие исследуемого **решающего shortcut**, а не отсутствие детерминированных вычислений/guardrails. Когда removal меняет допустимое пространство, это architecture-level comparison с явно ограниченной атрибуцией, не чистый single-factor эффект. Не считать расходы на подготовку/обучение L/H бесплатными; separately report marginal inference и cumulative learning cost.

**Output/examination:** lawful end-task quality, path validity/relative regret, per-domain retention, time-to-first-useful-signal, p50/p95 с N, simultaneous RAM/VRAM и persistent growth; quality–cost frontier. **Good:** H даёт тот же lawful quality дешевле или больше quality в том же бюджете. **Bad:** H получает teacher answers/больше candidates, а D — нет. **Edge:** D уже optimal; L arm отсутствует; oracle metadata; дорогое rejected branch; safety removal. **Falsifier:** H не лучше matched lawful alternative или проигрыш скрыт средним. Missing arm ограничивает whole-hybrid superiority claim, не блокирует C03/C07. Owner: R22 experiment design + actual mechanism owner; R23 resource receipt.

## 5. C01–C11 — полные содержательные карточки

Все карточки наследуют §2 и применимые §3.4/§7; dispatcher включает это содержанием. Чтение одной карточки без её laws/state/skills/refs не model-ready invocation. Outputs идут в существующие R22 carriers; будущие raw experiment artifacts только в отдельно разрешённый owner scope.

### C01 — domain mapping независимо от solver · E2

**Context/skills:** Q2+Q4; S-ARCH/S-PLAN/S-PROMPT; R21 §9.3 и actual StackDetector/domain selector consumer. **Prompt:** «Раздели subject, intent, language, stack, project и Student identity. При frozen solver/representation/context сравни predicted domain с adjudicated oracle-domain diagnostic на тех же независимых cases; включи mixed/OOD/unknown. Единственное вмешательство — routing metadata. Oracle не выдавай за production feature и не обучай по closed labels». **State/owner:** actual mapper/routing policy; если update owner отсутствует, только diagnostic, не обученный mapper.

**Output/examination:** confusion/abstention по доменам; допустимость маршрута и final quality; oracle gap; wrong-domain harm и полная routing cost. **Good:** корректный C# context отличает generic/nullability от похожего TS. **Bad:** solver стал лучше при неизменном mapper — заявлено mapper learning. **Edges:** C#+SQL запрос требует нескольких typed results; неизвестная версия; законные несколько labels. **Falsifier:** oracle/predicted одинаково ошибаются — исследовать solver/representation, не объявлять найденной проблему mapping. Independent family sources и fixed label policy обязательны.

### C02 — domain-sensitive tokens/vector representation · E2, R04:E01

**Context/skills:** Q2, S-ARCH/S-PROMPT; raw/canonical/TeachingVector/BPE/trainer/output channels R04 и R21 laws. **Prompt:** «На том же source corpus сравни current generic representation с конкретной law-preserving версией. Укажи coordinate meaning, typed associations, tokenizer/codebook version и compatible consumer. Перестрой index отдельно для каждой геометрии и учти цену. Не сравнивай cosine между несовместимыми spaces. Правка lossless material E1 — correctness repair, не доказательство learned encoder». **Owner:** actual encoder/projector и downstream Unit; learned target только при state+signal.

**Output:** semantic-loss/roundtrip examples, raw→vector→neighbor→answer trace, held-out law quality, lawful-neighbor recall при независимом reference и reindex/migration footprint. **Good:** отрицание, число и `?` меняют предметный смысл и сохраняются. **Bad:** vector dimension одинаков, значит semantics совместимы. **Edges:** Unicode/OOV, significant whitespace, same surface/different meaning, version collision, newly composed outputs. **Falsifier:** выросла similarity, но не meaningful downstream accuracy. Consumer frozen совместимым способом; невозможность совместимости — design residue, не скрытая смена solver.

### C03 — features/metric/reranker при фиксированном k · E2

**Context/skills:** Q2, S-ARCH/S-PLAN; R06 scorer/eligibility/neighbor-use evidence и actual callable scorer. **Prompt:** «Для reranker зафиксируй common candidate pool, corpus, law filter и k; меняй только fixed versus trained scoring. Назови objective, update recipient и доказуемо используемые scores. Для отдельной learned-geometry гипотезы держи corpus/k постоянными, но получи exact candidates заново в каждой геометрии: фиксировать прежние nearest IDs и одновременно заявлять новый nearest-neighbor search нельзя». Это C03-rerank/C03-geometry subcases, не два выполненных вызова.

Кандидат score: `s_d(q,n;theta)=sum_j w_j(d,q;theta)*phi_j(q,n)`, после hard eligibility. Domain laws нельзя превращать в мягкий штраф, который можно перевесить similarity. Query-dependent score не автоматически metric для индекса: valid broad retrieval → rerank; measure proposal recall separately. **Output:** actual state/signal, ranking flips, law errors/accuracy/full cost. **Good:** warning меняет выбор, lawful deterministic candidate остаётся доступен. **Bad:** manual weights названы обучением. **Edges:** nonmetric score, sparse domain, empty pool, stale index. **Falsifier:** изменился score, но ordinary output не использует новое ранжирование. Owner — actual scorer и его Unit consumer, не новый trainer.

### C04 — dynamic k/radius при неизменном score · E2

**Context/skills:** Q2+Q4, S-ARCH/S-PLAN. **Prompt:** «Freeze representation, corpus/eligibility, ranking и solver. Сравни fixed k с domain/query policy выбора k/radius по ambiguity/density/diversity/cost. Задай finite allowed actions, stopping и empty-pool fallback до outcomes. Различай learned policy и ручную эвристику». Candidate action: допустимое `(k,radius,stop)` под hard cap; реальный cap и допустимые actions принадлежат runtime/resource owner, не берутся из желания получить gain.

**Output:** chosen k/radius, exposed/actually consumed neighbors, accuracy/law/latency и память. **Good:** неоднозначный JOIN получает несколько различающихся lawful examples. **Bad:** вместе с k изменили encoder/corpus. **Edges:** warning-only hits, ties, zero lawful neighbors, rare domain/PC pressure. **Falsifier:** больше retrieval cost, но first-hit consumer выдаёт тот же ответ; выбрать более дешёвый comparator, а не заявить dynamic learning. Owner — actual policy+neighbor consumer; для learned claim нужен отдельный update signal, не просто вычисление k по формуле.

### C05 — exact search versus ANN · E2

**Context/skills:** Q2+Q4, S-ARCH/S-PLAN; actual exact/index APIs. **Prompt:** «Одинаковые corpus, lawful eligibility, representation, valid distance, k и downstream scorer/aggregation. Exhaustive reference ищет по всему eligible pool; ANN получает собственный candidate set. Сравни эти sets и конечные ответы. Не заставляй обе arms использовать один и тот же retrieved set: это скроет ошибки retrieval». При nonmetric rerank separately compare broad ANN proposal recall с exhaustive score reference; ANN distance и reranker score не смешиваются.

**Output:** neighbor-ID recall с tie-aware reference, downstream law quality, query/build/rebuild time, peak RAM и storage. **Good:** approximate выигрывает по цене при допустимом quality loss. **Bad:** новый corpus только в ANN arm или oracle filtering только у baseline. **Edges:** distance ties, tiny pool, stale index, eligible filtering после ANN теряет rare lawful neighbor. **Falsifier:** rare-domain harm превосходит экономию. Owner — actual index and ordinary caller. ANN не называется exact; native/release evidence только для конкретной используемой реализации.

### C06 — использование соседей: top-1 versus aggregation · E2

**Context/skills:** Q2+Q3, S-ARCH/S-PROMPT; KnnLogicalUnit/typed output consumer, R04 associations. **Prompt:** «Получив один frozen ordered neighbor set, меняй только top-1 output versus явно заданную multi-neighbor correction/aggregation. Укажи conflicts, positive/negative/warning semantics, output type и association IDs. Наличие k hits не доказательство того, что consumer использует их». Learned aggregator требует actual state/signal; deterministic aggregation может быть baseline.

**Output:** какие payload реально участвовали в ответе, lawful composition/accuracy/false claims, latency и memory. **Good:** два согласованных source spans объясняют две стороны метода. **Bad:** объединены несовместимые SQL schema/версии. **Edges:** TIE, duplicate evidence, cancelled Unit, missing OutputRefs, many→many, правильный top-1. **Falsifier:** recall вырос, а final quality нет либо появились ложные утверждения. Owner — current typed consumer и resolver, не новый framework.

### C07 — Unit и Flow: раздельный credit assignment · E3

**Context/skills:** Q3+Q4, S-ARCH/S-PLAN; stable IDs, OutputRefs lifetime/version и real Flow correction consumer. **Prompt:** «Freeze Unit outputs/state; измени только learned Flow correction и покажи получение правильного downstream input. Обратный контроль — Unit changed при fixed Flow. 2×2 U0F0/U1F0/U0F1/U1F1 допустим только с lawful state lineage/resources/no-repeat, не обязателен до первого опыта». Disjoint training families не автоматически exchangeable arms; confounding явно ограничивает conclusion.

**Output:** input→result associations, resolver payload/type/version, end-task outcome и correction costs; named signal recipient. **Good:** reordered два ответа сохраняют input IDs и правильно попадают в destination ports. **Bad:** positional Zip или score update без actual corrected input. **Edges:** N→M/empty outputs, missing/stale references, partial failure, cancellation. **Falsifier:** Flow state изменён, downstream ответ не изменился или изменился из-за Unit retraining. Owner — существующий Flow/Machine update seam; не создавать отдельный тренер Flow по названию.

### C08 — learned asynchronous Orchestrator · E3

**Context/skills:** Q3+Q4, S-ARCH/S-PLAN; actual dependency/route/budget consumer. **Prompt:** «Freeze Units/Flow и lawful candidate opportunities, dependency DAG и общий budget. Сравни fixed lawful scheduling/selection с learned branch/order policy. Запиши все launched/completed/cancelled/failed branches и причину final choice. Одинаковая возможность получить кандидата не означает бесплатное выполнение всех candidates». Asynchronous races не должны менять semantic association/order.

**Output:** valid final path, quality, latency и total work; regret к enumerated lawful alternatives для маленьких случаев либо best observed under fixed budget для больших. **Good:** выбран быстрее достижимый lawful результат без пропуска обязательного шага. **Bad:** cheapest/first response назван best quality или не посчитаны проигравшие ветви. **Edges:** mandatory slow branch, duplicate/late output, timeout, side effects, fan-in. **Falsifier:** выигрыш latency оплачен недопустимой quality/total-cost потерей. Owner — actual orchestration policy and selection caller; async method не доказательство learned route.

### C09 — DynamicGraph higher-order transfer · E4

**Context/skills:** Q3+Q4, S-ARCH/S-PLAN; actual graph state/update/preload/edge-authority/selected-target execution, R03 primary lineage. **Prompt:** «Freeze familiar primitives и lower policies. Измени только разрешённое relation/pattern state; сравни flat/per-primitive reference с higher-order graph на unseen structural compositions. Split делай по source/graph structure до rename/variants. Сохрани semantic OrderedPatternIds, law/type rules и final arbiter». NOT_ACTIVATED training не превращать в готовую Graph learning capability.

**Output:** unseen-composition success, path validity, retention на primitives, lookup/preload/state costs и ordinary selected-target evidence. **Good:** знакомые шаги законно сочетаются в ранее невиданном порядке зависимостей. **Bad:** сохранён тот же маршрут под другими node names. **Edges:** cycles, missing payload resolver, stale graph after adapter/index change, contradictory domain conditions. **Falsifier:** gain исчез на новой структуре. Owner — actual L5/Machine infrastructure, не sixth tier/trainer. Доступная flat baseline сохраняется; неподдержанный higher-order arm — bounded implementation seam.

### C10 — LoRA и semantic result projection: два разных вопроса · E1/E2

**Context/skills:** Q1+Q2, S-ARCH/S-PLAN/S-PROMPT; installed keys, typed source/target space, ordinary consumer. **Prompt:** «C10-LoRA использует N→U E1 с одним lawful update и тем же tokenizer/solver pipeline. Докажи actual recipient и cold ordinary use. C10-Projection отдельно сравнивает named P_domain с compatible frozen projector: source coordinates→typed target/codebook/decoder→обычный consumer. Назови objective/labels/allowed matrix, несовместимости и reindex/rollback». Один эксперимент не меняет оба механизма незаметно.

**Output:** semantic decoding/law validity и downstream quality, retention, adapter/projection/index migration cost. **Good:** learned projection сохраняет отрицание/число и выдаёт правильный typed token, используемый следующим Unit. **Bad:** VectorSignal масштабировал gradient или adapter hash изменился — объявлен новый tokenizer. **Edges:** wrong codebook same dimension, base key leak, old optimizer/objective, zero initial gradient одной LoRA matrix, incompatible index. **Falsifier:** эффект только в assessment-specific path/на known training family или semantic output потерян. Owner — trainer+ordinary consumer для LoRA; отдельный найденный projection owner для P_domain.

### C11 — DPO utility, memory control и Builder · E1 / R21-L5

**Context/skills:** Q1+Q4, R21 Builder B0/B1, S-ARCH/S-PLAN/S-PROMPT. **Prompt:** «Раздели качество пары, admission, actual update и Student utility. Сравни N/C0/U при одинаковой memory/context. Дополнительный read-only memory-only/prompt-only comparator нужен лишь для различения retrieval и weights effect. Builder сравнивает curator-chosen и Builder-proposed НОВЫЕ законные уроки при одинаковой source-policy/cost и downstream Student examination; pair JSON validity не главный endpoint». Историческое Chinese обеих sides правило остаётся; Builder не получает нового авторского grant этим опытом. Self-authoring допускается только действующим source-bound контрактом.

**Output:** chosen/rejected meaning/provenance/material verdict, admitted/attempted lesson efficiency, effect на независимом Student, retention и стоимость выбора/author/judge/update/eval. **Good:** пара качественная, utility не выросла — честное INCONCLUSIVE, исследуется recipient/control. **Bad:** repeated pair под новым UUID; deliberately silly negative; Builder сам выставляет себе итоговую полезность. **Edges:** BOTH_BAD/TIE, оба raw ответа дают одинаковые tokens, Student уже знает ответ, harmful/false feedback, unjudgeable source. **Falsifier:** больше валидных пар без улучшения Student или gain полностью объяснён memory-only. Disjoint curriculum allocation не называется randomized causal comparison без соответствующего дизайна.

## 6. Домены: конкретный смысл, функции соседства и независимый экзамен

Owner учебной программы — [R21 §9](DOMAIN-EXPERIMENT-RECIPES.md). Следующие law/feature/kNN идеи — **проектные кандидаты**, не «найденные лучшие функции». Для каждого domain фиксировать rights/version/source family, allowed answer set, learned owner, actual ordinary caller, typed associations, score/space/index versions. Hard constraints отделены от soft style; корпус и install readiness не подразумеваются.

| Предмет | Что нельзя потерять в tokens/IR | Кандидат lawful-neighbor policy | Good / hard negative / independent edge |
|---|---|---|---|
| EN↔RU | Direction, NEG(scope), quantity/unit, BEFORE/AFTER, modality, entities/placeholders | Same direction/law family; semantic score + slot agreement; diversity при ambiguity, learned k отдельно | «не отправлять до одобрения» против потери отрицания; unseen topics и reverse direction отдельной стратой; несколько верных переводов |
| Orchestration | Typed dependencies, effects, partial order, deadline/cancellation | Structural dependency features, warnings против unlawful order; score cost/quality лишь внутри eligible set | prerequisite раньше mutation; быстрый план пропускает обязательный шаг; rare fan-in/failure family |
| Figma→HTML/CSS | Hierarchy, geometry, component identity, constraints, breakpoints/order | Compatible component/layout features; diversity по responsive structures | корректный mobile/desktop layout; похожая картинка ломает structure/accessibility; unseen design family, viewport variants не независимые sources |
| C# documentation | Syntax/types/generics, nullable, control/data flow, exceptions, exact spans/version | Compatible language/source semantics; rerank по branch/contract similarity | null против 0; async cancellation; same syntax с иным getter side effect |
| Angular documentation | Framework version, lifecycle, signals/observables, binding and types | Version eligibility до similarity; rare lifecycle candidates с warnings | объяснение реально применимого lifecycle; перенос несовместимого API; unseen project/version |
| SQL documentation | Dialect/schema, keys/cardinality, NULL logic, aggregation/order/duplicates | Dialect/schema eligibility; join/null features и diverse lawful neighbors | LEFT JOIN preserves unmatched row; WHERE filter меняет смысл; unseen schema/dialect, duplicate-sensitive family |
| DPO Builder | Frozen x/context, law/weakness, final-pair provenance/identity, preference usability | Only permitted lesson sources; novelty/diversity; closed answers исключены из memory | meaningful lawful pair; trivial negative/forced TIE; unseen weakness и independent Student gain, не self-score |

Второй предмет после C# для breadth — **EN→RU**, а не ещё один code paraphrase. Его подготовка и E2 сравнения независимы от первого Student update; real translator/adapter/data/grants ещё привязываются. DTC/IDE, другие языки/intents/code/stored procedures/S00–S10/extras сохраняются с owners/triggers. Для нового real-world domain добавлять его реальные законы/единицы/ограничения, а не переносить универсальный cosine. Не заявлять coverage всех направлений по этим семи строкам.

## 7. Взаимодействия, bulk, эффективность и verdict

**Минимальные interactions:** representation×neighbor policy; Unit×Flow; Orchestrator×Graph. Difference-in-differences относится к одному заранее выбранному outcome при comparable states/panels. Four-arm дизайн только когда допустимы все четыре состояния/истории/ресурсы. Если не допустим — bounded observational contrast с ограничением causal claim. Не требуется Cartesian matrix до первого урока. Не складывать отдельные gains как доказательство synergy.

**Best path:** множество допустимых решений и source-based partial preference, не один эталонный path. Маленькие конечные cases — enumerated lawful optimum в объявленном пространстве; большие — best observed under fixed search budget. Указать comparator coverage и quality/cost regret, а не global optimum. Law-valid alternative не ошибка из-за несовпадения с reference string.

**Bulk:** окно последовательных НОВЫХ admitted lessons, не replay mini-batch. Один E1 урок — одно наблюдение, не доказательство bulk learning. Для каждого окна считать attempted/admitted/applied/accepted/rolled-back; B0 frozen initial, предыдущий accepted и candidate на сопоставимых панелях. Within-bulk panel/rubric constant; across-bulk stable operational anchor + новые family strata отдельно. Reused operational panel не untouched certificate. Teacher/lesson-selector/retrieval/training не получают sealed answers. Новый panel/rubric требует versioned series/bridge comparison обеих states, не склейку процентов.

**User feedback:** сохранить исходный answer/state; проверить rights/consent, source truth, conflicts, domain и novelty; admission новой пары отдельно; ложная поправка не truth. UNKNOWN previous mutation сначала recover; отдельный new pair не обход unresolved history. Измерять retention, false-feedback sensitivity, graph/index/adapter coherence и disk growth после каждого принятого окна.

**Метрики:** law-valid task accuracy/essential claims; per-domain/mixed/OOD mapping; representation semantic loss; kNN recall/ranking/actual neighbor use; typed associations/Flow correction; lawful path/Graph transfer; pair usability отдельно от learned utility. Primary metric/denominator/family weighting/noise/effect/retention/allowed looks фиксируются до outcome. p50/p95 всегда с N и cold/warm settings; малый sample не SLA.

**Полная цена:** wall time от разрешённого input до qualified outcome плюс phase times; teacher/selector/pair judge/Student/evaluator location/calls/tokens/cost; failed/rejected/cancelled branches; restore/reindex/reload. Simultaneous peak процесса/дерева: base/policy/reference/evaluation copies, LoRA, optimizer/gradients/activations/logits/KV, tokenizer/encoder, index/graph/cache и временные snapshots. Disk: retained checkpoints, corpus, index, journal и migration overlap. Не складывать разные по времени component peaks в measured peak. Cost per lawful lesson и per independent family improved сообщать только с реальным ненулевым denominator. Quality-at-budget/quality–time–memory frontier, не только размер adapter. Local Student с внешним Teacher/examiner не объявлять полностью offline autonomous learning.

**Решения:** IMPROVE — valid practically meaningful benefit по frozen rule при law/retention/cost guards; REGRESS — valid unacceptable harm; INCONCLUSIVE — insufficient/noisy/limited evidence; INVALID_EVIDENCE — contamination/material/state/comparison failure в конкретном scope; NOT_MEASURED — не запускалось. Не менять rubric ради зелёного результата. Isolated valid gain может сосуществовать с `ADOPTION_NOT_ESTABLISHED`: promotion и cold serving receipts отдельны. Не стирать valid isolated observation из-за delivery failure, но не заявлять user-delivered improvement. Negative experiment не закрывает требование улучшить продукт.

## 8. Очередность и конечная передача

**Ближайший путь:** R18 A/B/C минимальная реализация в отдельных правах; R22/R13 выбирают frozen examination profile; R21 готовит lawful C# и EN→RU families; R23 задаёт whole-PC allocation. Эти preparations идут по собственным prerequisites, не whole R01–R20 DONE. Нет blanket «сначала E1, затем можно исследовать C01–C11».

При bound E1 — одно informative observation. Material invalid → exact seam repair; no-update тоже улучшился → context/noise confound; update есть, ordinary state прежнее → consumer/adoption; no gain при valid conditions → следующий различающий Cxx, не повтор пары. При E1 blocker разрешённый owner может **до outcomes** выбрать более доступный E2 по expected information/effort; это не замена нейронного урока заявлением о learned routing. E3/E4 запускаются по собственным owner/state/consumer/permission prerequisites.

R22 передаёт R13 выбранный/предложенный quantitative profile и specific design residue; R18 — exact mechanism/consumer condition, не новый trainer; R21 — source-family/second-domain receipts; R23 — full phase/state/cost ledger; R24 — ordinary DTC/IDE user journeys и product output evidence, не весь незавершённый statistical design; R25 — exact candidate + counterexamples и независимое review поручение. Ни одна передача не повышает sibling status.

**Документная проверка:** против каждого expectation имеется C00/C01–C11/E1, actual/proposed owner, lawful control, concrete good/bad/edge и full-cost falsifier; G5/G6 совпадают с R18; R21 не missing; no general E1-before-E2 gate; pipeline-corrected N→U; no hidden replay/closed-panel leak; design/runtime/observation/review раздельны. Самопроверка автора не independent Astra. Повторный исследователь продолжает точный residue, не создаёт новый общий audit или R26.

**Independent examiner packet (будущий отдельный вызов):** exact final R22/R18/R13 bytes/diff, original role/WorkLeft authority, lawful domain cases, candidate controls, raw protected output refs/cost/counterevidence, proposed versus approved profile и список непривязанных states. Задача examiner — опровергнуть causal attribution, semantic loss, leakage, no-repeat violation, hidden resource cost и circular prerequisites; вернуть scope-bound ACCEPT/NEEDS_FIX с конкретными refs. Model alias в плане не actual invocation; текущий independent Astra review **NOT_PERFORMED**.

## 9. Научные опоры и границы

DPO objective: Rafailov et al., [Direct Preference Optimization](https://arxiv.org/abs/2305.18290). LoRA parameter adaptation: Hu et al., [LoRA](https://arxiv.org/abs/2106.09685). Adaptive holdout risk: Dwork et al., [Generalization in Adaptive Data Analysis and Holdout Reuse](https://arxiv.org/abs/1506.02629). Это первичные методические основания, не доказательство эффективности XIIGEN, не права на конкретный provider и не готовая domain metric. Числа pilot/confirmation выше — помеченные проектные предложения, не результаты этих статей.

**Фактически здесь исправлены документы R22.** Продуктовый код, datasets, weights, workflows и чужие results не менялись; DPO generation, training, inference, независимая модельная экспертиза и измерение PC/quality не выполнялись.
