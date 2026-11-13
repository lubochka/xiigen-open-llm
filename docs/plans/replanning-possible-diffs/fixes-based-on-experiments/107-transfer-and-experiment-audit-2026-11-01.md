# 107. Аудит исполнения переносов и точности опытов — 1 ноября 2026

## Результат

В очереди102 принято **6 DONE / 2 PARTIAL / 1 OPEN / 1 CONDITIONAL**. Новые коммиты действительно закрыли default ownership KNN, владение памятью RouteClassifier, неверный backward ProbeNN и несколько узких переносов переводчика. Их повторная реализация не назначается. Однако «8 из10 закрыто» в103 считает наличие реализации достаточным: R6 сохраняет два пробела договора запуска, R7 — ошибку постоянной ничьей. Наличие отдельных evaluator и launch claim полезно, но исходная приёмка этих строк ещё не выполнена полностью. [S103], [S102], [S30], [SB], [SB-limit]

Текущий пакет содержит аудит, [адресный план108](108-addressed-repair-plan-2026-11-01.md), [карту усиления карточек109](109-card-hardening-and-coverage-2026-11-01.md), пересчёт сохранённых наблюдений и source probes. Он не объявляет будущие продуктовые исправления уже выполненными. Научная очередь и её NEXT остаются у execution index.

**Проверенные срезы:** core `790228c256168a336b01abae4387e28391048c0c`, ветка `codex/dpo-implementation`; planning `1bec1b8907a87dc2f768bb950a488732ff925a00`, ветка `llm_plan3`. Перед публикацией повторно сверяются удалённые вершины. Аудит учитывает перечисленные владельцем коммиты30–31 октября, включая P81; старая локальная вершина не используется как доказательство отсутствия исправления.

## 1. Как проверено и что означает статус

Проверены исходники владельцев и непосредственных потребителей, изменения коммитов, назначенные карточки, сохранённые JSON/ответы/логи и границы положительных контролей. Применены навыки из `docs/skills`: архитектурная классификация, intent-to-plan/phase-planning, code-examination, plan-review/plan-self-validation, prompt-patch-authoring и human-readable-architecture-audit. Сначала отделялся детерминированный дефект преобразования/проверки от гипотезы о качестве обучения; затем прослеживался путь от входа до потребителя.

Один отдельный reviewer проверил продуктовых владельцев; root проверил его находки, runner и экспериментальные числа. Это не полная независимая повторная реализация всех опытов. **.NET и PowerShell здесь не запускались; нового обучения0; восстановления двоичных checkpoints в свежем процессе0.** Сохранённые чужие test receipts ниже — readback, а не результаты нового тестового прогона.

[recompute_evidence.py][ER] выполнил113 арифметических/структурных проверок над26 входными файлами: пересчёт P67/P68/P72/P74–P81 и явно обозначенные аналоги P30/R6. [product_source_probes.py][EP] независимо проверяет логические следствия пяти текущих C# owners. Оба скрипта используют Python standard library, не вызывают learner. SHA-256 и Git blob hashes прочитанных файлов сохранены рядом с результатами. Число113 относится к проверкам этого reader, не к113 native tests и не к113 научным подтверждениям.

Термины: **DONE** — принято конкретное требование очереди в проверенной границе; **PARTIAL** — часть механизма работает, остаётся именованный контрпример; **OPEN** — обещанный путь не закрыт; **CONDITIONAL** — ещё требуется отдельное эмпирическое назначение. Исполненный отрицательный/неразличимый опыт сохраняет свой статус исполнения, не превращается в «не запускался» или «всё неудачно».

## 2. Сверка действующей очереди102

| ID | Статус | Основание и остаток |
|---|---|---|
| R1 | DONE | Normalized example ID guard принят прежним аудитом; новых оснований переоткрывать его нет. Это не guard дублей другого TranslationKnnStore. |
| R2 | DONE | `e10c305b9b59`: default неизвестного mutable answer требует явной snapshot policy; сохранённая приёмка41/41. Дисковый checkpoint из этого не следует. |
| R3 | DONE | Dataset router владеет копиями входных features/labels. Соседний RouteClassifier теперь тоже исправлен отдельным коммитом. |
| R4 | DONE | Отказ новой router batch не очищает принятую память; valid replacement сохранён. Это не транзакция TranslationMachine. |
| R5 | DONE | RNG comparison идёт по полям/entries/cursors. Прежняя принятая строка не переоткрывается. |
| R6 | PARTIAL | `087ed2b9cea` добавил claim, caller binding и проверки зависимостей;27/27 сохранённых controls. Не покрыты imports только reference projects и legacy terminal без нового claim при continue; A02 ниже. |
| R7 | PARTIAL | Evaluators P30/P40 реализованы. P30 всё ещё считает отсутствие строгого dominance доказательством crossing; A03. Работающий P40 не назначается заново. |
| R8 | DONE | Hash-bound effective reader P31 уже существует; прежняя коррекция6/21/4/25 читается через него. |
| R9 | OPEN | `29.SemanticCompilerPipeline.Compile` вызывает строковую keyword-перегрузку; trained int-overload остаётся вне этого ordinary path; A05. |
| R10 | CONDITIONAL | Только оставшийся различимый эмпирический вопрос после релевантного ремонта. Этот аудит не выдаёт разрешение повторять завершённые Train. |

R1/R3/R4/R5/R8 здесь сохраняют принятую приёмку104; повтор полного прежнего native набора не заявляется. R2/R6/R7/R9 сверены с текущими владельцами. Поэтому эта таблица честнее и старого5/3/1/1, и позднего8/0/1/1. [S102], [S103], [S104], [SK], [SR], [SB], [S30]

## 3. Что действительно перенесено

| Перенос | Что подтверждено | Граница |
|---|---|---|
| Default mutable KNN | Policy resolver отказывает до построения памяти;41 сохранённых PASS. | Explicit Shared остаётся явным выбором; persistence — отдельный контракт. [SK] |
| RouteClassifier ownership | Полная `ownedRows` строится до `_train = ownedRows`; массивы скопированы. | Не решает empty inference и wiring `Compile`. [SR] |
| ProbeNN backward | `dH` и `dEmb` вычисляются до изменения W2/W1; сохранённая finite-difference приёмка: max absolute error2.826×10⁻⁷. | Проверена производная маленькой сети, не обещано качество предметной задачи. [SNN], [SNN-r] |
| Точная память перевода | Составной topic/source key, writer/reader/import; адресная приёмка8/8,3 адреса для3 уроков, fresh-load4/4. | P51 закрыт в узком scope ключа; same-key revision P67 и fallback isolation — отдельные вопросы. [SE], [ST], [SE-r] |
| EN→RU `can't` | Source-side polarity guard KNN; адресные3/3, cold→trained→reload0/4→4/4→4/4. | Не доказана вся cascade, любые языки, цитаты и многосоставное отрицание. [SL], [ST], [SL-r] |
| Синтаксически нечитаемая batch row | Preflight всех известных trainable JSON до live mutation; адресные3/3, rejected-state hashes совпадают. | Синтаксис не равен typed validity и поздней атомарности; A01. [SM], [SM-r] |
| WikiTrace pair | `pair.A`, `Chosen`, `Rejected` реально дошли до metric conversion; сохранены50 combined model vectors вместо5 шаблонов. | Комбинированная уникальность не доказывает prompt-only различимость или улучшение learned model. [SW] |
| DataLoader stride и хвост | Default stride=sequence length; явный1 сохраняет старое сравнение; tail-aligned window достигает конца shard. Сохранено135 PASS,2 SKIP,0 FAIL. | В P68 измерен охват и небольшой loss gain, не доказано ускорение wall time. [SD], [SD-r] |
| Память формул и доля отказов | Unknown→`EXPRESSION_NOT_IN_MEMORY`; E13B различает `exact_match`, `answered_not_matched`, `expression_not_seen`, `execution_failed`; abstention считается прямо. | Прежний defective metric не делает все nonmatches отказами. Сохранённые адресные suites8/8,7/7,7/7; родственные47/47 и85/85. [SF], [SF-e], [SF-r] |
| E28 project key | `(Project,Current)` и found flag действительно есть; legal label0 отделена от no entry. | Исправлен experimental owner, не доказан ProductHost transfer; текущий `Run` имеет A04. [S28] |
| E30 current action | В адрес добавлен current predicate, а не next/gold;44 координаты, сохранённые9/9. | Native experimental cache исправлен; ordinary product consumer и его эффект этим не доказаны. [S30cache] |

## 4. Оставшиеся ошибки и конкретное последствие

### A01 · P1 · Частичная атомарность TranslationMachine

В `ValidateStructuredBatchRowsReadable` выполняется `JsonDocument.Parse(json)`. Typed `JsonSerializer.Deserialize<ExecutorInput>` находится в trainer, а `MeasureHeldOutAccuracy` — после записи учебных строк. Валидный JSON `42` проходит синтаксический preflight, но не является ExecutorInput. После первой законной строки вторая такая строка может отказать уже после изменения живой памяти. Отдельный late failure — нечитаемый held-out после успешной train row: preflight held-out пропускает. [SM], [ST]

Это source-confirmed незакрытая граница транзакции, **не новый native воспроизведённый прогон**. Синтаксический guard P53 работает и остаётся принятым. Исправление108/RF01: typed staged runtime целиком, поздняя проверка в нём, одна публикация. Распространено на P53, state/restore и orchestration cards; проверять state сразу после отказа, до repair-fit.

### A02 · P1 · R6: неполный scope build inputs и legacy continuation

`Get-RunBindingSourceClosure` обходит ProjectReference, но `Get-RunBindingRealImportGraph -ProjectPath $fullProject` вызывается один раз для главного проекта. `.props/.targets`, подключённые только reference project, не входят в этот preprocess. Сам отчёт исполнения C01, пункт6, прямо признаёт ограничение. Менять только такой import всё ещё можно без изменения записанного root import graph. [SB], [SB-limit]

`Test-RunBindingArchivedStatusAdmits` проверяет JSON parse и равенство SHA копии/live status; содержание стадии обучения не читает. `ContinuationAuthorized` пропускает terminal markers. Если у исторической завершённой попытки ещё нет **нового** launch claim, admission идёт в `CreateNew`; дальнейший путь P50/P52 предусматривает полный learner executable при прохождении остальных gates. На проверенном SHA оба полных script сейчас останавливает независимый source-pin DRIFT до build: это отдельно сохранено в `runner-pin-readback.json`. Поэтому доказан дефект admission, а реальный повтор обучения через весь текущий script не заявляется. Нынешний `process_started=true` claim, live/unknown owner и чужой архив защищены правильно; контрпример относится к terminal без нового claim. Проверен source path и Python state analogue с положительными guards, не реальный повтор обучения. [SB], [S50run], [S52run], [ER]

Приёмка27/27 сохраняется в своей границе. Исправление108/RF02: migrate/deny legacy terminal для учебного запуска; отчёт достраивать reader-only; imports вычислять для всех фактически собранных проектов и конфигураций. Запрет повторного Train не обходится новым child ID.

### A03 · P1 · P30 принимает постоянную ничью за информативный фактор

В `AlphaPanelEvaluator`: `crossoverFree = sameWinner && positiveMargins`, затем `withCrossover = rowVerdicts.Count - crossoverFree`. Кандидаты A=(1,1), B=(1,1) дают постоянную ничью при любом alpha и того же winner A по ordinal tie-break. Margins0, поэтому нынешняя формула насчитывает «настоящее пересечение» и `INFORMATIVE_FACTOR_PANEL`. Это логическая ошибка дополнения множества: отсутствие доказанного строгого dominance не доказывает crossing. [S30]

Source analogue воспроизвёл false-positive и два controls: строгий dominance действительно неразличим, реальное пересечение меняет winner. Исправление108/RF03 вычисляет фактическое изменение назначенного observable на назначенных alpha, отдельно отмечает tie/unknown evidence. Существование evaluator больше не закрывает R7. Нового обучения для ремонта не требуется. [ER]

### A04 · P1 · Ключ E28 исправлен, но `E28.Run` не измеряет адаптацию

Текущий `genericPairs` строится из `projectA.Concat(testFiles)`: экзамен вошёл в обучение общей модели. `adaptedModel` обучен только ключам project A, а все testFiles получили project B. Поэтому на всём экзамене enhanced вынужден читать тот же generic fallback. Кроме того, predicates сначала соединяются между файлами и лишь потом `Zip`: возникают ложные переходы через file/project boundary. [S28]

Source-equivalent fixture, входом которого являются уже извлечённые predicates, даёт0 adapted hits/5 экзаменационных пар,5/5 test pairs внутри generic training,5 межфайловых edges/11. Это demonstrator алгоритма, не измерение нативного C# corpus. Историческая planning-находка N37 уже описывала leakage и cross-file; новый project key их не устранил и сделал zero treatment reachability явно проверяемой. [EP], [PN37]

Исправление108/RF04: настоящий ProjectId независимо от split role; TRAIN/EXAM по source files внутри назначения; пары отдельно по файлам; nonzero adapted hits; negative control выключает improved lookup и меняет заранее назначенную строку. **P72 3/4→4/4 остаётся полезным измерением коллизии ключа** на своём host, а не исправленным `E28.Run` целиком и не доказательством ProductHost gain.

### A05 · P1 · Соседний маршрутизатор и ordinary consumer

`29.SemanticCompilerPipeline.Compile` зовёт `Classify(string[])`; trained memory читается `Classify(int[])`. Подключение learned route R9 всё ещё отсутствует на названном пути. Другой `23.SemanticCompilerAdapter` не объявляется сломанным или отсутствующим: это другой consumer и ему нужна собственная трасса. [SP], [SR]

В learned int-overload29 нет empty guard. Для k1 и `[1]→PLAN`, `[2]→ARCH` пустой запрос одинаково далёк от обоих; перестановка lessons меняет ответ PLAN↔ARCH. Python analogue подтверждает порядок выбора. Исправленный `22.PredicateDatasetRouter` здесь не переоткрывается. Сохранённая собственность массивов29 тоже не отменяется. [SR], [EP]

Исправление108/RF05: typed no-evidence и назначенный downstream fallback; один public raw request до/после controlled learned-label intervention, реальная schema и instance identity. Проверка одной внутренней перегрузки не закрывает `Compile`.

### A06 · P1 · Два различных дефекта TranslationKnnStore остаются

1. `AddPair` делает append; один lesson добавляется под full/prefix/structure. При совпадении подписей он занимает несколько мест k. Исправленный normalized ID в другом KNN не исправляет этого owner. P60 доказал12→8 memberships и3→4 уникальных соседей, но ordinary exact source route маскировал answer gain. [SKNN], [ST], [P60]
2. `if (full.Count > 0) return full;` завершает tier search до executor target compatibility. Полный tier с английским ответом EN→RU может быть целиком отвергнут позднее, а допустимый русский prefix уже не читается. P64 показывает component gain; public panel2/2 в обоих плечах идёт через exact path. Source polarity P55 — другой guard. [SKNN], [ST], [P64]

RF06 задаёт stable lesson identity до cap и eligibility до завершения tier. Совпадение текста не заменяет identity: разные lessons и same-ID revision должны иметь явную политику.

### A07 · P2 · Пустые контроли и нулевой знаменатель нельзя считать успешной проверкой

В P79 `positions_with_empty_mask_in_both_arms=0`, `identical_answers=0`, `violations=[]`, но `passed=true`. На50 назначенных позициях контроль не задействован; корректный status **UNEXERCISED**, а вывод об одинаковых масках/ответах на основной панели остаётся действительным. Это не повод автоматически менять панель и переобучать модель. [P79-c], [ER]

В P80 правильное определение retention использует `rawPresent ∩ normalized` в числителе и raw-present в знаменателе. На45 строках75/75=1; потерь0. В основном наборе нет raw-empty строк. `MarkerReportArmB` выдаёт строкам n/a, но для aggregate `denominator==0` возвращает числовой0. Будущий wholly-empty input должен получать null+UNMEASURED, а не0% качества. Также меняется определение числителя: «поправлен только знаменатель» — слишком узкая формулировка, хотя на данной панели числители равны75. [P80-src], [P80-m], [ER]

P77 runtime manifest имеет четыре `ФАЙЛ_НЕ_НАЙДЕН` для локальных seam paths; pre-build `source-pins.txt` содержит их хеши. Это расхождение двух writers provenance, а не доказательство отсутствия исходников или ложности3/3 и0/3. Ремонт reader должен сверить старые hashes/bytes и дать одну эффективную source map, без нового fit. [P77-pin], [P77-pre]

### A08 · P1 для общего допуска · широкая приёмка переводчика остаётся красной

| Сохранённый runner | Before | After | Адресные новые tests |
|---|---:|---:|---:|
| FIX-P53-BATCH-JSON |313 PASS,2 FAIL/315|313 PASS,2 FAIL/315|3/3|
| FIX-P55-SOURCE-POLARITY |313 PASS,2 FAIL/315|313 PASS,2 FAIL/315|3/3|
| FIX-P51-TOPIC |301 PASS,1 FAIL/302|301 PASS,1 FAIL/302|8/8|

Первое падение: `TranslationDpoTrainerBridgeApiBoundaryTests.TrainWithStepMany_HasNoUndocumentedPublicDirectTrainerMutationOverload`, ожидается2 overloads, найдено3. Второе: `TranslationDpoTrainEvaluateRunnerTests...UnsafeParentsFailBeforeTraining("corrupt")`, ожидается `CANDIDATE_INVALID`, получен `MANIFEST_INVALID`. [SE-r], [SL-r], [SM-r]

Оба присутствовали до переносов: это **не доказанная новая регрессия** topic/polarity/preflight. Но они запрещают объявлять весь затронутый gate зелёным. RF07 требует решить API/admission contract и проверить, что corrupt fixture доходит до заявленного gate, сохранив no-mutation; менять expected ради green нельзя.

### A09 · P1/P2 · Долги сохранения состояния и HNSW остаются отдельными

План105/C05 требовал actual typed state у P50/P52. Current C01 не реализует это автоматически: launch manifest и output hashes не являются сериализацией learner. Для доступных бинарных файлов через GitHub connector встречаются LFS pointers; этот аудит не материализовал payload и не исполнял fresh-process restore. Это **не утверждение, что payload потерян**, и не разрешение молча заменить его новой моделью. [S105], [SB-limit]

Current `HnswIndex.Load` проверяет коллекции и размеры, затем присваивает `_entryPoint = data.EntryPoint`, не проверяя topology bounds. Старый counterexample104 (one-node index, EntryPoint99) остаётся применимым к этому source; native повтор здесь не заявляется. Сохранённые length/ownership guards остаются полезными. RF09 переносит actual typed persistence и load-topology admission раздельно. [SH], [S104]

## 5. Что именно показали поздние эксперименты

Каждая строка относится к своему appointed host/state/panel. Малая полезная ось не обнуляется из-за отсутствия полного product gain; нулевой gain также не переименовывается в обучение. Raw и исторические reports не переписаны.

| Опыт | Подтверждённое наблюдение | Корректная граница и следующий вопрос |
|---|---|---|
| [P55][P55] | Полярность: primary3/4→4/4, wrongAccepted1→0. | Узкая law-проверка; поздний native `can't` перенос принят отдельно. |
| [P56][P56] | Whole-token verifier primary3/4→4/4, edge2/4→4/4. | Actual5 уроков/42 checks против1/14; native tokenizer signature и controlled signature различаются. Не заявлять весь Flow исправленным. |
| [P57][P57] | Semantic scorer не изменил вердикты на actual exact-copy answers. | Нет observed semantic gain; scorer sensitivity на искусственной панели — другая ось. |
| [P58][P58] | После feedback оба memory состояния совпадают,1/8 против1/8; cold0/8. | Feedback дошёл до lesson, но различия acquired state не возникло. Supplemental fits не подменяют исходные плечи. |
| [P59][P59] | Новый lesson доступен после reload, старый panel19/24 сохранён. | Actual80 Flow calls против54; exact metric не доказывает сохранность politeness. |
| [P60][P60] | Membership12→8, unique top-k3→4. | Gain доступности соседей; exact bypass не позволяет заявить answer gain. |
| [P61][P61] | Неизвестный хвост теряется в адресе; добавление хвоста различает уроки. | Experimental writer не равен исправленному product writer; OOV/коллизия/ordinary path раздельно. |
| [P62][P62] | Лишний signature tag опускает admission threshold и допускает чужой ответ. | Проверять monotonicity на unknown source и реальном reader, не только знакомый exact answer. |
| [P63][P63] | Симметричная source мера закрывает один gate, но cascade всё ещё пускает чужую оговорку. | Полезный component check не закрывает end-to-end law. |
| [P64][P64] | Eligibility внутри tier находит допустимый lower candidate; ordinary2/2 в обоих плечах. | Различить candidate gain и masked public outcome; A06. |
| [P65][P65] | History similarity отбрасывает противоположный смысл; manual passport разрешает contrast. | Это назначенный oracle допуска, не наученный semantic deduplicator. |
| [P66][P66] | Jaccard0.75 у negation contrast; фильтр убирает смысловую проверку. | Сравниваются report/admission policy на тех же6 predictions, не новое обучение. |
| [P67][P67] | A2/2, B1/2; reload сохраняет оба исхода; confidence0.9900 у правильного и неверного. | Проблема revision order под тем же адресом, не устраняется topic key сама по себе. |
| [P68][P68] |4 updates/arm; input positions7→16; loss6.74650749→6.73708502 (B−A=−0.00942246). | Coverage gain2.286×; wall45.1→66.3ms не доказывает ускорение. Дополнительный exam dataset build раскрыт. |
| [P69][P69] | Длинные источники с одинаковым64-token prefix схлопываются; short controls различимы. | Сохранённые wrong retrieval доказывают вред данной потери контекста; обобщённый новый encoder ещё не выбран. |
| [P70][P70] | Owner-aware memory2/4→4/4. | Перестановка полей восстанавливает известные mapping; это не unseen generalization. Native conversion transfer — доставка полей, не измеренный DPO gain. |
| [P71][P71] | Оба алфавита1/2, как constant baseline; разные причины неразличимости. | В одном плечe input alias, в другом candidate words не кодируются. Rebuild readings — reconstruction без Train, не новая независимая попытка. |
| [P72][P72] | EXAM3/4→4/4, train controls9/12→12/12;4 gained rows,0 lost. | Exact-key disambiguation; unknown-project gate одинаково предписан. Не переносить на A04. |
| [P73][P73] | Neural conditioning EXAM.75→1 и train.75→1,2400 updates/arm. | Равная размерность не означает бесплатность; timings/allocations показывать отдельно. Это probe learner, не ProductHost. |
| [P74][P74] | Coverage9/12=.75 неизменно; lawful answers4/4→3/4; confidence.7483 не отличает wrong. | Фальсификация proxy как гарантии retention; после corruption обучение0. |
| [P75][P75] | Historical support violations3/10, current0/10; один historical сбой через TextGenerator. | Всего24 sampler calls с4 greedy baseline. Support law ≠ sign accuracy; нового sampler fix здесь не требуется. |
| [P76][P76] | Replacement chars5→2; удалены3 bytes; strict valid0/4→2/4. | Полная метка0/4 в обоих после назначенного truncation;400 SFT updates. Interior damage остаётся видимым. |
| [P77][P77] | Original3/3 без LOW; отдельная rare LOW0/3. | Нельзя вычитать как−100pp падение модели; модель одна, знаменатели разные. Source map A07. |
| [P78][P78] | window1:3updates/3monitor calls; window5:5updates/1call; correct2→2 оба. | Native cadence, не moving-average window. Monitor calls восстановлены по losses; live hook не заявлен. |
| [P79][P79] | Маски и ответы совпали:46/50 оба, blocked0, divergent0. | Structural leakage существует, effect на этой панели не проявился. Empty-mask control UNEXERCISED. |
| [P80][P80] |75 сохранённых markers:75/270=.27778 против75/75=1; потерь0. | Исправление определённости метрики, не обучение; zero-aggregate edge A07. |
| [P81][P81] |d(T1)=1.89036894, d(T2)=+0.17209435, contrast1.71827459. Preference3/6; actual2/6. | Оба правильных знака не получены;2 actual successes были уже до4 updates. Зависимость от prompt измерена, приобретение двухстороннего навыка не доказано. |

P78 source trace: `DefaultLogicalCapabilityTrainer.ConvergenceMonitor` делегирует `DpoConvergenceMonitor`; `_windowSteps` там увеличивает счётчик. `LogicalDpoTrainingUnit` сам зовёт monitor только `step % windowSteps == 0`. При max=window=5 будет одно сравнимое наблюдение; это не проверка среднего за5 шагов. P81 logits пересчитаны reader, argmax проверен отдельно от restricted +/- preference. [SCVG], [SCVG-call], [ER]

## 6. Как ошибки перестанут воспроизводиться в заданиях

В карточках рядом с исходным рецептом добавлены конкретные positive/negative controls: late typed/held-out failure; old terminal/no claim; reference-only import; постоянная ничья; нулевая представленность класса; неиспользованное improved shoulder; source/file boundaries; prompt-only view отдельно от targets; отказ/неверный ответ/ошибка исполнения; same-key revision; различие coverage и correctness; знак каждой preference пары. Полная карта и hashes старого/нового тела —109 и accompanying JSON.

Приёмка экспериментального host остаётся соразмерной его задаче: seam/copy допустим, если фактор и граница вывода честны. Ordinary product consumer обязателен **для заявления о переносе в продукт**, не для каждого дешёвого опыта. Уже измеренный raw не «исправляется» новым fit; пустой контроль помечается незадействованным, а новое назначение требуется только когда ответ на оставшийся вопрос действительно нужен.

## 7. Ограничения покрытия

Это адресный аудит актуальных переносов, серии55–81 и связанных будущих карточек. Он не объявляет все561 entries planning/possibleDifs или всё solution глубоко проверенными. Архивные title/статусы сохранены с явным действующим дополнением. C05 persistence не закрыт одним присутствием LFS pointer, и отсутствие локального payload не объявлено потерей на сервере. Нативная приёмка новых контрпримеров остаётся в108; текущий audit code проверяет только свою указанную арифметику и source logic.

## Источники закреплённого среза

[S102]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/102-late-transfer-repair-plan-2026-10-29.md
[S103]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/103-transfer-queue-commit-mapping-2026-10-30.md
[S104]: 104-transfer-and-experiment-audit-2026-10-30.md
[S105]: 105-addressed-repair-plan-2026-10-30.md
[S30]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P3001/verdict/AlphaPanelEvaluator.cs
[SB]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/experiments-v2/_runners/run-binding/New-RunBinding.ps1
[SB-limit]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/experiments-v2/_runners/C01-RUN-BINDING-attempt-0001/not-proven.md
[S50run]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P5001/Run-Experiment.ps1
[S52run]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P5201/Run-Experiment.ps1
[SK]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/21.AgentInfrastructure/Logical/IKnnAnswerSnapshot.cs
[SR]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/29.SemanticCompiler/Routing/RouteClassifier.cs
[SP]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/29.SemanticCompiler/Pipeline/SemanticCompilerPipeline.cs
[SNN]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/35.ExperimentsV2/Core/ProbeNeuralNetwork.cs
[SNN-r]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/experiments-v2/_runners/C04-PROBENN-BACKWARD-attempt-0001/gradient-receipt-attempt-02-AFTER-FIX-green.json
[SE]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/27.TranslationDomain/TranslationExactSourceKey.cs
[ST]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/27.TranslationDomain/TranslationTrainedUnits.cs
[SM]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/27.TranslationDomain/TranslationMachine.cs
[SKNN]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/27.TranslationDomain/TranslationUnitContracts.cs
[SL]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/27.TranslationDomain/TranslationMemorySourceLaw.cs
[SW]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/42.ProjectWikiDomain/WikiTraceAggregatorTrainingPipeline.cs
[SD]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/02.DatasetPipeline/DataLoader.cs
[SD-r]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/experiments-v2/_runners/TRANSFER-DATALOADER-STRIDE-attempt-0001/step-07-final-state-all-affected-checks.log
[SF]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/21.AgentInfrastructure/Logical/ExpressionCompletionUnit.cs
[SF-e]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/35.ExperimentsV2/Experiments/E13BHeldOutValidation.cs
[SF-r]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/_runners/FIX-ABSTAIN-REASON-IN-SUCCESS-ENVELOPE-attempt-0001
[SE-r]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/_runners/FIX-P51-TOPIC-attempt-0001
[SL-r]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/_runners/FIX-P55-SOURCE-POLARITY-attempt-0001
[SM-r]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/_runners/FIX-P53-BATCH-JSON-attempt-0001
[STests]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/Tests
[S28]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/35.ExperimentsV2/Experiments/E28ProjectAdaptor.cs
[S30cache]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/35.ExperimentsV2/Experiments/E30PredicateCache.cs
[SH]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/06.VectorSearch/HnswIndex.cs
[SCVG]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/11.DPO/DpoConvergenceMonitor.cs
[SCVG-call]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/18.CapabilityTraining/LlmIngredientLogicalUnits.cs
[PN37]: https://github.com/lubochka/xiigen-planning/blob/1bec1b8907a87dc2f768bb950a488732ff925a00/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/37-NEW-E28-PROJECT-ADAPTOR-TEST-LEAKAGE-AND-CROSSFILE-2026-10-16.md
[ER]: ../../../../experiments-v2/_audits/2026-11-01-transfer-audit/recompute_evidence.py
[EP]: ../../../../experiments-v2/_audits/2026-11-01-transfer-audit/product_source_probes.py
[P55]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P5501/results/attempt-0001
[P56]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P5601/results/attempt-0001
[P57]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P5701/results/attempt-0001
[P58]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P5801/results/attempt-0001
[P59]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P5901/results/attempt-0001
[P60]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P6001/results/attempt-0001
[P61]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P6101/results/attempt-0001
[P62]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P6201/results/attempt-0001
[P63]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P6301/results/attempt-0001
[P64]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P6401/results/attempt-0001
[P65]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P6501/results/attempt-0001
[P66]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P6601/results/attempt-0001
[P67]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P6701/results/attempt-0001
[P68]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P6801/results/attempt-0001
[P69]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P6901/results/attempt-0001
[P70]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P7001/results/attempt-0001
[P71]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P7101/results/attempt-0001
[P72]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P7201/results/attempt-0001
[P73]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P7301/results/attempt-0001
[P74]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P7401/results/attempt-0001
[P75]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P7501/results/attempt-0001
[P76]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P7601/results/attempt-0001
[P77]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P7701/results/attempt-0001
[P78]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P7801/results/attempt-0001
[P79]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P7901/results/attempt-0001
[P80]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P8001/results/attempt-0001
[P81]: https://github.com/lubochka/llm_mvp_core/tree/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P8101/results/attempt-0001
[P79-c]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P7901/results/attempt-0001/controls.json
[P80-src]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P8001/Program.cs
[P80-m]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P8001/results/attempt-0001/metrics.json
[P77-pin]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P7701/results/attempt-0001/source-pins-runtime.json
[P77-pre]: https://github.com/lubochka/llm_mvp_core/blob/790228c256168a336b01abae4387e28391048c0c/experiments-v2/RP01-MICRO/P7701/results/attempt-0001/source-pins.txt
