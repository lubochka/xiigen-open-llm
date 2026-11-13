# N58 — Паспорта коротких экспериментов для предварительного согласования человеком

**Дата:** 2026-10-19 · **Статус каждого: DRAFT / DESIGN_ONLY / NOT_RUN / HUMAN_APPROVAL_REQUIRED.** Этот документ — **НЕ разрешение** запускать native обучение, исправлять продукт или менять `llm_mvp_core/docs/plans/replanning-possible-diffs/**`. Все возможные результаты ниже — **ожидаемые условные различения**, а не измерения. Исходные промпты каждого **нового** эксперимента приведены **полностью и дословно** ниже; исторический оригинальный запрос/режим запуска восстанавливается из указанного receipt и **не подменяется** этим новым промптом.

## Общий набор скилов и точный human gate

**Загрузить по source path, не по пересказу**:
- [Architect](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/human-readable-architecture-audit-SKILL.md), [Trainable architecture](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/trainable-architecture-decision-SKILL.md), [code examination](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/code-examination-SKILL.md).
- [Plan review](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/plan-review-SKILL.md), [experiment design](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/experiment-design-SKILL.md), [claim verification](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/claim-verification-SKILL.md).
- [Prompt authoring](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/prompt-patch-authoring-SKILL.md), [reference/source map](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/reference-inventory-and-source-map-SKILL.md), [DPO triple](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/dpo-triple-format-SKILL.md), [human communication](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/human-communication-GLOSSARY.md).

**Отдельный skill override по карточкам** уточнён в каждом опыте. Нельзя сказать `skills loaded` без списка файлов и read receipt.

**Единый технический PRE-FLIGHT:** указать exact commit SHA для planning и core, проверенный source/blob SHA для кандидата, исполняемую версию harness/native DLL/model binary, config, checkpoint SHA, schema и способ создания **независимых одинаковых стартовых состояний**; source/corpus/train/checkpoint/tokenizer/encoder/index/optimizer/RNG/reference hashes; полный TRAIN source IDs и EXAM IDs, подписанный до изменения ground truth. Для native ML прямо назвать `policy`, `reference`, `tokenizer`, `generation consumer`, seeds, topK/topP/EOS, exact model ID/config; без pin → **BLOCKED_INPUT**, никакого подстановочного имени или случайного B0 вместо historical state. Для Unit-only: записать **`neural model: N/A; layers: N/A; trainable state: named Unit`**. Нужен explicit user APPROVED со scope конкретного ID; до него только чтение и подготовка.

**Одинаковые 4 состояния для каждого experiment:** `A0` старый механизм **до** учебных воздействий, `A1` тот же A после неизменённого/sham или одинакового TRAIN, `B0` один альтернативный механизм **до**, `B1` альтернативный после того же TRAIN; если меняется только selector/metric/restore без обучения — `updates=0`, A0=A1/B0=B1 по состоянию, compare A vs B и не выдавать за учебный gain. Frozen blind EXAM для обоих плеч; оценка real raw output как `correct/all`, `correct/answered`, `wrong accepted/all`, coverage, errors by semantic obligation и сохранение старых 4 ответов; отдельно latency, true CPU vs wall, process peak RSS, GPU VRAM, checkpoint bytes, trainable parameters/layers, Units/Flow, route+neighbor+verifier evidence. Label family/group split: TEST не входит в train, не менять gold. Масштаб **2 урока + 4 EXAM** default; N58-01 использует полный **8+24** для честного исторического воспроизведения, а `2+4` как отдельный tiny check. Бюджет не является гарантией обнаружения эффекта. N/A и UNKNOWN указывать буквально, а не нулём.

## N58-01 — EN→RU: положительный донор P125, восстановление обычного Flow, вклад phrase mapping

**Предмет:** перевод, профиль EN→RU. **Получатель:** `executor-en_ru` памяти `TranslationMachine` (exact/phrase/KNN), обычный `GetOrchestratorAsync().RunAsync`. **Нейронная модель:** **НЕТ, N/A**; 0 LoRA updates. **Точное имя runtime:** `TranslationMachine → ExecutorUnit → TrainedTranslationAdapter → TranslationRoutingFlow`. Историческая привязка: [native P125 receipt](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md), core code `7be28a0d24937e3100af8ffa7701518e1022745c`, historical Release `27.TranslationDomain.dll` SHA256 `395995130c266d23a2f6ac1b2da9e90fc4544d64524b5eb8ed38221e85182683` **только для frozen historical receipt**, а не утверждение о сегодняшнем DLL.

**Первичные, подлинные данные:** [TRAIN JSON](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/train.json) — 8 уже опубликованных `S032` action/object уроков с explicit `PhraseMappings`; [EXAM inputs 24](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/eval-inputs.json) и [blind gold](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/panel-gold.json). Не заменять ручной реконструкцией: `validate the file → проверь файл`, `delete the cache → удали кэш` — только иллюстративные **части**. Historical control C shuffled validate/delete, 13/24; positive B 19/24. Негативные `do not delete the file`, `did you delete the file` должны быть оценены как неверные при утрате polarity/question.

**A/B, только одно воздействие в каждом отдельном запуске:** (R1) frozen воспроизведение неизменного donor (A0/A1 no train, B0/B1 8 lessons) — **reproduction**, не новый механизм; (R2) два обученных плеч с одинаковыми lessons и index, отличающиеся **только передачей `phrase_mappings`**, на новой frozen 4-item панели — **causal factor**. Нельзя смешивать R1 и R2 или назвать R1 вкладом phrase map; для R2 источник берётся из существующего корректного input schema.

**Ожидаемая проверка, НЕ прогноз точного числа:** R1 должен показать, воспроизводится ли 0/24→19/24 на том же native state/панели; иной результат — честный regression/identity problem. R2 показывает отдельно, влияет ли `PhraseMappings` на настоящие ordinary composition, не ухудшая отрицание. Исторические данные **не являются** B1 для новой копии.

**Карточки для сверки:** [P125](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P125-executor-memory-learning.md), [P25](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P25-cold-restore.md), P54, P59. **Специализированный скилл:** `learning-path-audit-SKILL.md` + `learning-loop-closure-SKILL.md` + полный общий набор.

**Дословный новый executor prompt:**

> N58-01; роль: исполнитель воспроизведения, НЕ автор новых gold. До любого native запуска покажи мне pinned core/planning SHA, historical vs runnable DLL/checkpoint SHA, полный список skills, реальный JSON 8 TRAIN и EXAM 24 без gold в runtime. Получатель исключительно TranslationMachine.executor-en_ru, никакой LoRA/Student. Предложи R1 и R2 как отдельные A0/A1/B0/B1 с единственным различием. Не запускай пока я явно не напишу APPROVED N58-01 R1 или R2. После разрешения сохрани каждый raw Flow output/adapter/verification, counts, wrong accepted, benchmark compare, SHA и ресурсы; оставь protected folder нетронутой. Если donor нельзя восстановить, ответ BLOCKED_INPUT, не запускай альтернативный опыт.

**Human gate:** `PENDING_APPROVAL`. Если SHA DLL / schema / actual historical files не совпадают — `NOT_REPRODUCIBLE` без адаптации ответа под тест.

## N58-02 — Student DPO: способность базы, сохранность пары и генерация — РАЗНЫЕ контрасты

**Предметные профили:** документация C# и перевод EN→RU на отдельных, НЕ объединённых панелях. **Модель (наименование и топология из audit):** локальный `LlmModel` RP01 code-documentation Student, **случайно инициализированный B0, pretraining НЕ доказан**; `config/code-documentation/generation-v1.json`: `vocabSize=629, hiddenDim=512, layers=4, contextLen=2048`; DPO `Beta=0.1`, `AdamW default lr=1e-5`, q/v-LoRA `rank=8, alpha=16, dropout=0` ([RP01 audit](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md)). **Не заявлять совпадения** с отдельным `TranslationDpoTextFeaturizer` default `512 vocab/64 context`. Для фактического запуска **обязательно назвать exact policy checkpoint file/hash, frozen reference SHA и tokenizer artifact SHA**; пока **UNKNOWN / GATE BLOCKED**. Reference/model bytes нельзя заменить выдуманным checkpoint.

**Исходные источники:** [C# simulation 500 и ENRU simulation 500](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) как историческая сводка; исполнимый tiny TRAIN берёт **2 уже admitted выбранных контраста** из конкретных source JSONL `training-data/dpo/csharp-code-documentation/simulation-csharp-500-v1` или `training-data/dpo/translation-en-ru/simulation-enru-500-v1`, с exact `pair_id/family_id`, без повторов и без TEST-family. Если такие две реально различимые пары не отобраны/не проходят admission — `BLOCKED_DATA`, а не самодельный teacher gold. Проверочные C#: оператор `x-2` против `x+2`, различие expression body и block body, nullable branches; EN→RU: `do not delete` vs `delete`, роли и порядок. Эти строки — **семантические обязательства**, не готовый corpus до фиксации exact raw pairs. EXAM — четыре новых case IDs/families на профиль; blind evaluator оценивает смысл, не score label.

**Независимые гипотезы, НЕ смешивать в один A/B:** (C-only, 0 updates) P07 materializer/encoder/model-visible IDs + response masks/EOS/common prefix, модель не нужна; (N1, native) P02 DPO-only vs same-base short SFT warmup→DPO, separately equal-DPO and equal-TOTAL compute budgets; (N2, после N1) P01 q/v targets vs разрешённый иной recipient, отдельно rank/alpha/dropout. P10 finite-difference проверяется компонентно **до** интерпретации quality; не заявлять, что все P07/P01/P02 выполнены одной карточкой.

**A0/A1/B0/B1 и ожидаемое:** native N1 обе базовые копии равны до уроков, в A те же 2 DPO updates, в B разрешённые 2 warmup + те же DPO updates; B использует больше compute, второй **отдельный** matched-total-cost comparison обязателен для вывода об эффективности. Главный observable — **собственный свободный ответ TextGenerator после real restore**, case-by-case; loss/margin/16 changed tensors — вторичные. 0/4→0/4 = **NO_OBSERVED_QUALITY_GAIN**, а не «успешно обучилась».

**Карточки:** P01/P02/P04/P07/P09/P10/P14/P18/P81/P112. **Специализированные скилы:** `dcm-dpo-construction-GUIDE.md`, `dcm-training-GUIDE.md`, `training-data-gap-audit-SKILL.md`, `data-flow-integrity-SKILL.md` + общий набор.

**Дословный новый executor prompt:**

> N58-02; сначала только source-backed review. Верни exact ID, weights/checkpoint SHA, tokenizer corpus/vocab SHA, config, frozen reference hash, model layers/trainables, полные 2 TRAIN пары с происхождением chosen/rejected и admission, четыре заранее запечатанных TEST-family cases, system/user prompt и generation EOS/topK/topP. Не используй другой tokenizer с тем же VocabSize. Выбери ровно ОДИН P07(C), P02(N1) или P01(N2) и опиши A0/A1/B0/B1; P02 требует отдельного equal-total-budget. Загрузи и перечисли все названные skills. До моего APPROVED N58-02 <subcase> не запускай Student, gradient, Windows workflow или внешнего teacher. После одобрения предъяви реальные free answers, masks/tokens/gradient при необходимости, true ресурсы и независимый blind verdict без изменения gold. Если policy/ref/encoder неизвестен — BLOCKED_INPUT.

**Human gate:** `PENDING_MODEL_SHA + PENDING_APPROVAL`.

## N58-03 — C# Structural kNN: ordinal аргумента вместо потери различия

**Предмет:** структура программ C#, задача **предсказать следующий Roslyn AST-kind**. **Точный получатель:** `StructuralVectorExtractor → StructuralKnnPredictor.Train/Predict` из [N57 source review](57-ARGUMENT-AND-VERIFIER-LEARNING-GAPS-2026-10-19/01-structural-argument-gap.md), потенциально через `SemanticCompilerFacade` лишь при включённом `UseStructuralKnn=true`. **Нейронная модель:** **N/A**, обучаемая память kNN, не LoRA. **Donor sibling:** `language_translate` original branch source blob фактически тот же алгоритм (по N57), НЕ свидетельство, что fix уже присутствовал в gold.

**Начальные данные:** два реально parsed и признанных Roslyn TRAIN transitions в двух разных C# файлах, где `Pair(a,b)` различает первый/последний аргумент; четыре новые позиции в **других файлах** со сменой имён/внешних форм, заранее определённый реальный next-kind из traversal (не произвольная label `left/right`). Первоначальная программа пример: `void F(){ Pair(a,b); return; }` как **семантический sketch**, actual valid compilable C# fixtures и exact Roslyn version/hash до запуска извлечь из N57 и закрепить; если next-kind не различается — закрыть hypothesis, не исправлять gold. Source input/style и formatter одинаковые.

**Единственное B:** к прежним feature dimensions добавить `ArgumentOrdinal` у потомка `ArgumentSyntax` внутри `ArgumentListSyntax`, сравнение `0.3` как предварительный явно фиксированный вес. Старые поля, corpus, distance прочих координат, k/codec, routing, label vote не менять. **A0/B0** до 2 lessons на четырёх source-disjoint TEST; **A1/B1** после 2 matched real `Train`, тот же actual `Predict`. Проверить, что реальные противоположные позиции имеют одинаковые старые признаки, но разные `ArgumentOrdinal`. Затем при наличии живого facade caller отдельный readout `PredictedNextTokens`; без caller нельзя утверждать RP01 product quality effect.

**Ожидаемый различающий результат:** B должен менять actual feature identity/ranking у контрастных узлов; повышение correct/4 на новом code shape возможно, **не гарантировано**. Даже если B увеличил next-kind exact, не приписывать ему способность писать документацию. Эту находку **уже открыл N57** — N58 только проверяет полноту паспорта.

**Protected refs:** P26/P27/P82/P83/P84/P91; N57 отдельного exact подслучая ArgumentOrdinal пока не связал с запускаемым P. **Спецскилы:** `code-examination-SKILL.md`, `architectural-decision-testing-SKILL.md`, `learning-path-audit-SKILL.md` + общий набор.

**Дословный новый executor prompt:**

> N58-03: никакого продукта и обучения до approval. Назови Roslyn assembly/version, current и donor StructuralKnn blob SHA, четыре точные пары TRAIN/EXAM с исходными C# файлами и вычисленным РЕАЛЬНЫМ traversal next-kind; сначала покажи без запуска изменения, как отличаются feature vector и роли ArgumentSyntax/ArgumentListSyntax. Единственное B — ArgumentOrdinal плюс заранее фиксированный вклад 0.3. Старые fields/routing/labels не изменять. Предъяви A0/A1/B0/B1 native Predict с raw ranked neighbors и correct/4 по прежде frozen EXAM, если я дам APPROVED N58-03. При неустановленном обычном consumer пометь PRODUCT_NOT_REACHABLE. Не меняй protected каталог.

**Human gate:** `PENDING_ROSLYN_FIXTURE/PENDING_APPROVAL`.

## N58-04 — RU→EN: learned verifier marker вместо ошибочного «прошёл проверку»

**Предмет:** перевод, профиль RU→EN, задача сохранить manner. **Получатель:** `TranslationStructuredBatchBuilder → QualityVerifierUnit.TrainOnExample/Verify`, поверх обычного adapter **только после** component-level доказательства. **Нейронная модель:** N/A. **Состояние:** два независимых исходно пустых `QualityVerifierUnit`; идентичный текущий source/checkpoint, версию/sha зафиксировать. **Источник нового механизма уже N57**, [подлинная карточка N57](57-ARGUMENT-AND-VERIFIER-LEARNING-GAPS-2026-10-19/02-verifier-marker-gap.md).

**Полные учебные данные (illustrative N57 fixture, НЕ approved teacher DPO):**
- TRAIN T1: `Он тихо открыл дверь`, chosen `He quietly opened the door`, rejected `He opened the door`;
- TRAIN T2: `Она быстро собрала сумку`, chosen `She quickly packed the bag`, rejected `She packed the bag`;
- Builder создаёт именно **verifier lesson** `Pass=false`, Issues `expected 'quietly'/'quickly' for translated quality marker` для **ошибочного candidate**; не переворачивать выбранную DPO сторону и не изменять score напрямую.
- EXAM E1/E2: `Он тихо прочитал письмо` с candidate `He quietly read the letter` (pass) / `He read the letter` (fail);
- EXAM E3/E4: `Она быстро написала записку` с `She quickly wrote the note` (pass) / `She wrote the note` (fail).

**A:** verifier `LooksLikeMannerMarker` текущая испорченная кириллица; **B:** добавить один нормальный кириллический suffix `о` в **эту функцию**, остальное без изменений. A0/B0 четыре `Verify` до lesson; A1/B1 после одинаковых 2 lessons; записать marker rules count/checkpoint, route/verdict, exact issue, true vs false, correct/all, wrong accepted/all, warm/cold. Ничего не менять в POS tokenizer или lesson input одновременно.

**Ожидание из N57 source-equivalent моделирования, не факт native:** исходное A0=2/4, A1=2/4, B0=2/4, B1=4/4 на указанной панели; **при реальном запуске возможен другой результат**, отметить его без переписывания expected. За пределами этих двух известных marker-слов правила не доказаны. Дополнительный ordinary Flow — отдельный consumer proof, потому что phrase override может bypass verifier.

**Protected refs:** P52/P56/P57/P58; точного кириллического `LooksLikeMannerMarker` вмешательства не найдено в обзоре N57, но не доказано exhaustive absence. **Спецскилы:** `translation-domain-reality-check-SKILL.md`, `learning-signal-capture-SKILL.md`, `test-integrity-SKILL.md` + общий набор.

**Дословный новый executor prompt:**

> N58-04: до запуска предъяви полные два RU→EN structured verifier lesson DTO и четыре EXAM candidate DTO выше; покажи actual source helper, current SHA и boundary BuildBatch→TrainOnExample→Verify. Используй два чистых verifier с одинаковыми состояниями; только B включает распознавание кириллического suffix «о», никакой POS/tokenizer поправки и никаких новых translation pairs. Зафиксируй sample provenance как illustrative approved trial, не external teacher. До моего APPROVED N58-04 не запускай native C#. После approval приведи 16 raw Verify results, learned marker maps, false accepts, checkpoint bytes и обновление ordinary Flow **отдельно если reachable**. Protected docs read-only.

**Human gate:** `PENDING_STRUCTURED_INPUT/PENDING_APPROVAL`.

## N58-05 — новая составная гипотеза H58-01: конфликтные уроки с разными IDs, но общим адресом памяти

**Статус новизны:** **HYPOTHESIS / NEW COMPOSITION TO CHECK**, а не заявление, что другие агенты никогда её не рассматривали. Простые элементы *уже* P51.1 (`TopicDomain`), P53.4 (`ExampleId`), P54 (phrase) и N56 (routing). **Не найдено завершённого paired native A/B именно с `distinct ExampleId + same normalized key + opposite lesson insertion order + identical ordinary consumer` в просмотренных картах.** Перед добавлением новой protected P требуется дополнительная semantic deduplication.

**Предмет:** перевод EN→RU с разными темами слов. **Получатель:** `ExecutorUnit` exact+phrase maps/kNN, завершающий `TrainedTranslationAdapter`. **Нейронная модель:** N/A; память Unit. **Код:** [assign last value exact source](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L413-L416), [phrase update](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L754-L759), [consumer ignores TopicDomain](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L331).

**Начальные данные — предложенная контролируемая fixture, требует подтверждения применимости и независимого gold до запуска:** две structurally valid, deliberately conflicting source lessons with **distinct** `ExampleId`, `SourceId`, `BatchId`, `HeldOut=false` and accepted `UnitId=executor-en_ru`, одна и та же нормализованная source phrase `the bank`: topic `finance` target `банк`, topic `river` target `берег`. *Важно*: слово `bank` без контекста действительно неоднозначно; решить смысл могут только `TopicDomain` или полный disambiguating sentence. До проверки normalizer и lawful record schema эта synthetic пара — **не подлинное обучение из рабочего corpus**, только изолированный stress test. Дополнительный вход `the account is open` / `the river is open` — NEGATIVE independent statements, gold обязан быть утверждён человеком, без заранее обещанной эквивалентности.

**A:** два допустимых урока в порядке finance→river; **B:** те же два идентичных учебных DTO в порядке river→finance, одинаковая изначальная machine и SHA, обе версии добросовестно допускают distinct ExampleId; zero-change C — только одно значение ключа в обоих порядках. Это тест **order sensitivity** фиксированной архитектуры, не A/B исправления. После подтверждения (отдельное вмешательство C) поменять только domain-specific namespace ключа (P51.1) при неизменном порядке, не смешивать C с order-of-lessons. EXAM 4 заранее frozen requests: `the bank` topic finance и topic river, плюс два same-key casing/whitespace перефразирования со стандартным и отличающимся TopicDomain; **если нет независимого определимого gold на паре `the bank`, измерять не accuracy перевода, а identity/collision/order condition**, добавив однозначный контекст для семантической панели. Прямой product success только после independently valid semantic oracle, без transfer claim для неоднозначного слова.

**A0/B0 до обучения:** empty maps, same ordinary routing. **A1/B1 после:** compare map key/value/checkpoint (коллизия), raw Flow outputs, chosen channel, correct/all лишь для валидного gold, loss старого урока, false-accepts. Новая гипотеза поддержана, если *при обеих допустимых lessons и одинаковом state* меняется ответ из-за порядка и не учитывается тема; противоположный результат опровергает на выбранной панели. Предлагаемая диагностическая формулировка без переноса метрик: «перезапись address памяти, не слабый gradient».

**Protected refs:** P51.1, P53.4, P54.1/P54.3, P125; **coverage PARTIAL / compound evidence OPEN**. **Спецскилы:** `data-flow-integrity-SKILL.md`, `learning-path-audit-SKILL.md`, `negative-example-patching-SKILL.md` + общий набор.

**Дословный новый executor prompt:**

> N58-05: сначала предоставь source-backed proof, что два distinct identity урока с одинаковым normalized SourcePhrase но разным TopicDomain могут быть допущены в Executor без изменения protected files. Назови точный supported JSON DTO, фактический TopicDomain carrier, key before/after, два независимых cloned initial checkpoints, source IDs и четыре frozen EXAM с семантически недвусмысленным gold. Если пара «the bank» неоднозначна и тема не является legit explicit context — ограничь результат фактом collision, не заявляй semantic accuracy. Сравни A и B с противоположными порядками при неизменных DTO, затем отдельно предложи P51.1 domain-key как второй эксперимент. Укажи все skills и блокирующие SHA. Только после моего APPROVED N58-05 выполняй native 2+4; покажи before/after selected memory entry/Flow output и сохранение прежнего урока. Protected folder read-only, Windows CI и 500-pair retrain не запускай.

**Human gate:** `PENDING_LEGAL_DATA_ORACLE/PENDING_APPROVAL`.

## Финальный план контроля до запуска любого ID

| Gate | Требуется в подписываемом pre-registration | Решение сейчас |
|---|---|---|
| G1 | Exact PIN core/planning, binary/model/checkpoint/index/tokenizer/encoder sha, имя runtime/Student; отсутствие у Unit-only модели явно N/A | **OPEN** |
| G2 | Исходный промпт дословно, skills как exact paths и read receipts | **DRAFT PROVIDED**; исполнитель ещё не подтвердил |
| G3 | Full raw TRAIN DTO/source IDs/heldout case IDs + frozen semantic rubric и role/negation/operator edges | **OPEN для новых native runs** |
| G4 | Один factor на опыт, четкие A0/A1/B0/B1, same final consumer, no oracle, negative controls, native vs proxy | **PLAN READY FOR REVIEW, NOT EXECUTION** |
| G5 | Permission code/sandbox/human approval, отдельно от исследования; запрет protected subtree | **PENDING APPROVAL** |
| G6 | Протокол true memory/process metrics, elapsed/CPU отдельные; counts denominators, wrong accepted, cold restore и независимый blind reviewer | **SPECIFIED, NOT MEASURED** |

**Любой неподтверждённый G1–G3 означает `BLOCKED_INPUT`, а не разрешение импровизировать начальное состояние, промпт, промаркированные DPO-пары или ожидаемый ответ.** Этот документ создан для проверки человеком ДО работ. Никакие новые эксперименты не запускались и никаких результатов качества не получено.

## H58-02/H58-03 — дополнительные утверждаемые паспорта

[**Branch-specific DPO gradient-sign и LoRA forward/dropout паспорта**](58-SIBLING-DPO-LORA-SOURCE-DELTAS-2026-10-19.md) содержат отдельные source proofs, исходные prompts, skills, модели, synthetic/preexisting data boundaries, expected discriminator и предварительный gate. **Они не входят в пять основных ID**, это ещё два альтернативных условных опыта, **NOT_RUN / approval required**.
