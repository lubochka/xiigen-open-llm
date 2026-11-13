# 158. После Q011: что действительно перенесено, проверено и осталось исправить

4 ноября 2026. Проверенный core: `45bd4e371f511e29c053f8117d61f43365abb460`, planning: `9cd53517311be504a89f0e04cdcea0afb011bf8c`. Это адресное продолжение аудитов [149](codex-149-post131-transfer-and-experiment-audit-2026-11-03.md), [154](codex-154-postq003-artifact-and-transfer-audit-2026-11-03.md), [153](codex-153-p115-p127-transfer-boundaries-2026-11-03.md) и [157](codex-157-postq003-executor-context-repair-2026-11-04.md), с учётом опубликованных Q004–Q011. [План исправлений 159](codex-159-postq011-addressed-repair-plan-2026-11-04.md), [покрытие карточек 160](codex-160-postq011-card-coverage-2026-11-04.md), [договор доказательств 51](../experimets/codex-51-postq011-evidence-boundaries-2026-11-04.md).

## 1. Вывод для решения

Восстановление опытов существенно продвинулось. Оно уточнило научные выводы и вернуло исходные Git-объекты, но не закрыло продуктовую приёмку переносов. Конкретная новая связь с продуктом — Q007: внешний проверяющий отвергает неполный и численно недопустимый optimizer state, тогда как сам `AdamW.TryImportPortableState` принимает его. Это подтверждено сохранённым нативным наблюдением и нынешним исходником. Исправление должно сохранить законный свежий и разреженный optimizer state.

Другие основные остатки уже имели владельцев: происхождение CAS-конфликта, поздняя отмена Evaluate, привязка DPO к реальному mutable recipient, lineage с одинаковым content и разными законными IDs, null/empty Output перед fingerprint, связь P127/P131 с фактическим payload/output. Они перепроверены по текущим файлам; новая параллельная очередь не создана. Исправленные BOM/hash, late-cancel training и существующие механизмы cache/framing не объявляются отсутствующими.

По сохранённой науке: P03 действительно ухудшил B с 2/4 до 1/4; P04/P06/P08/P09/P10 не получили новых правильных ответов на своей панели, хотя отдельные вероятности и численные свойства улучшились. У P127 и P131 положительные результаты сохранены и независимо пересчитаны. Ошибка проверяющего не равна ложности всех записанных им результатов.

Навигационная сверка реестра различает **114 базовых Q/R-работ и7 поздних адресов115–121, всего121 уникальный work ID**. Все121 строки сохранены. Это полный счёт адресов, а не повторный аудит исполнения всех121 реализаций.

**Очереди:** 131/131 исходных результатов записано. В отдельной последовательной проверке сохранённых опытов приняты Q001–Q011, 11/131; остаётся 120, Q012 ещё не назначен. Продуктовый порядок по [реестру](01-registry.md#executor-next) остаётся Q01→Q02→Q03→Q06; Q04 приостановлен, Q05 зависит от него. Q02 здесь — PAIR migration, а не optimizer continuation.

## 2. Что лично проверено и где проходит предел

| Вид проверки | Сделано в этом аудите | Чего она не доказывает |
|---|---|---|
| Актуальность источников | GitHub branches/trees и exact blob bytes; [183 источника переноса](../../../../experiments-v2/_audits/postq011-2026-11-04/transfer-source-manifest.json), исходные карточки и сохранённые материалы Q002–Q011 | Не полное построчное чтение всех файлов репозитория |
| Продуктовые цепи | Caller→guard/importer→mutable state/publication; адресные tests и отсутствие нужных контролей | Не свежий C# PASS и не проверка всех 114 реализаций заново |
| Численные данные | Собственный [воспроизводимый пересчёт](../../../../experiments-v2/_audits/postq011-2026-11-04/codex-audit-saved.py), [вывод](../../../../experiments-v2/_audits/postq011-2026-11-04/codex-saved-checks.json): панели P03–P06, ресурсы P08/P09,21 504 Float32-пары P09, objective chain P10 | Не новый Forward/Generate/Train; не восстановление отсутствующих logits или moments |
| Сохранённые проверяющие | Q008 honest→exit0, порча scored position→exit2; Q011 honest→0, порча производной→2, дополнительный scope-probe ниже | Не исполнение нативных C#/PowerShell finalizers |
| P127/P131 | Независимый [JSON-пересчёт](../../../../experiments-v2/_audits/postq011-2026-11-04/transfer-saved-recount.json), повторён основным аудитором тем же открытым [скриптом](../../../../experiments-v2/_audits/postq011-2026-11-04/transfer-saved-recount.py) | Не новая работа native Flow или ordinary product consumer |
| Binary inventory | Обновлены все 3502 пути прежнего фиксированного inventory через 137 корневых tree IDs и 7 изменившихся деревьев | Git identity не равна материализованным LFS bytes или native Load |

`dotnet`, `pwsh`, `powershell`, `mono`, `csc`, `mcs` не обнаружены в проверенных PATH/стандартных местах. Поэтому **fresh native build/test/CLI/Load — UNRUN**, новых model calls и training updates —0. Исторический native receipt Q007 прочитан, но не присвоен этой сессии. Текущие GitHub `.bin` выдаются как LFS pointers; свежего получения backing payloads и массовой загрузки не было. Квитанции прежнего LFS refetch сохранены как доказательства их авторов.

Сохранённый завершённый пакет содержит 5 Node subprocess checks. Во время подготовки пакет повторялся; ранний вызов Q011 с неверным именем `derivative-check.json` завершился ENOENT до проверки, после чего использован действительный `derivative-comparison.json`. Это ошибка подготовки, а не отрицательный научный контроль. Нулевые native/model counts относятся ко всем этим чтениям.

Применены project skills архитектуры, планирования и review, code-examination, experiment-design, prompt-patch-authoring, improvement-measurement и человеческого изложения. Большие AGENTS/CLAUDE/skills читались по применимым разделам; полного перечитывания сотен килобайт каждого файла не заявляем. Независимый исследователь проверил продуктовые цепи; основной аудитор проверил его доказательства, численные результаты и конечные документы. [Подробный разбор исходника](../../../../experiments-v2/_audits/postq011-2026-11-04/transfer-findings.md) содержит методы, строки, SHA и контрпримеры.

## 3. Главный продуктовый разрыв: guard проверяющего и настоящий импорт

### 3.1. Что произошло в Q007

В [native-result-002.json](../../../../experiments-v2/RP01-MICRO/P0601/results/codex-review-2026-11-04/native-result-002.json) девять требуемых случаев прошли **диагностическую** приёмку. Для `optimizer-empty` и `optimizer-nan` одновременно записаны `NativeAccepted=true`, `FullStateAccepted=false` и изменение optimizer digest. Внешний guard сначала защищает отдельный объект; затем диагностический вызов на другом объекте намеренно показывает, что продуктовый importer принимает эти входы. Общий `accepted=true` означает, что диагностика сработала как ожидалось.

Текущий [AdamW.cs](../../../../04.TrainingLoop/Optimizers/AdamW.cs), `TryImportPortableState`, проверяет supplied keys/shapes/lengths, собирает detached dictionary и заменяет `_state`/`_step`. Проверки NaN/Infinity и отрицательного второго момента нет. Empty Entries проходит пустой цикл. Путь `DpoTrainer.TryRestoreOptimizerState` действительно вызывает этот importer; project guard проверяет supplied bindings, а trusted proof связывает bytes, что само по себе не проверяет численную допустимость исходного state. Здесь **не** утверждается обход уже выданного proof подменой bytes.

Численный дефект и полнота пакета — две обязанности. NaN/Infinity в m/v и v<0 недопустимы для формулы AdamW. Для Q007 Step401 trusted contract ожидает четыре набора моментов, поэтому пустой пакет неполон. Но [AdamWOptimizerStateRecoveryTests](../../../../Tests/AdamWOptimizerStateRecoveryTests.cs) законно принимает empty Step0 и sparse state одного параметра при двух bindings. Глобальный запрет пустого состояния либо требование moments для каждого параметра модели сломали бы этот договор.

**Ремонт:** intrinsic numerical validation до swap; completeness по внешнему trusted expected initialized keyset конкретного continuation. Проверить отказ actual importer и actual DpoTrainer consumer с неизменностью старого состояния. Успешность только внешнего reader guard недостаточна. Владелец — существующий Q04/FIX-MC и state contracts02/101/P12; пауза Q04 сохраняется. Подробная матрица —159§2.

### 3.2. Что уже доказано полезного

Q007 восстановил два original checkpoint и нативно загрузил состояния A/B в историческом reader:16 weight arrays,4 пары moments,Step401 и RNG сверены с независимым разбором. Всего 19 учитываемых попыток чтения с первым неудачным запуском, при лимите 20. Новых учебных обновлений этот reader не делал. Обычные ответы после fresh load не измерялись. Исторический опыт сделал два настоящих updates400→401; оба плеча изменили 896 LoRA-координат и сохранили 12 frozen arrays. Одинаковый ответ 2/4 и одинаковые конечные states согласуются с тем, что `VectorSignal` учитывает число занятых slots, а не их знак/смысл.

## 4. Состояние старых продуктовых исправлений на нынешнем source

| Остаток / действующий владелец | Подтверждённая причина и проверка, которой ещё не хватает |
|---|---|
| Q01 /135-R01-CONCURRENCY | `TranslationMachine.TryPublishVersion` теряет `observed` одного CAS, а diagnostic перечитывает поздний CurrentVersion. При B→проигрыш A→публикация C запись A может назвать C победителем вместо B. Нужен детерминированный трёхписательский порядок и возврат observed snapshot |
| Тот же Q01, Evaluate | После `BeforeMeasuredAccuracyPublishBarrier` отсутствует финальная проверка cancellation перед CAS. Отмена в barrier при отсутствии другого writer может опубликовать accuracy/version. Поздняя отмена **Train уже защищена**, её сохранить |
| Q01, ordinary-store/new process | Store и durable candidate уже написаны; требуется их обычное composition wiring и восстановление именно проигравшего staged candidate в новом процессе. Исправление BOM/hash из `b4af9eef70f` и опубликованные 36/36 с R17 не переоткрывать |
| R128-02 /T121-OWNER/BINDING/SHARING | Content hash checkpoint + binding names не различают два mutable recipient с одинаковыми bytes. Отдельные capability ledgers, ownerless real issuer и admitted constructor без recipient оставляют обход договорённой привязки. Проверять настоящие objects/issuer и reissue, а не искусственно разные строки |
| R128-03 /P112 | Выбор первого SourceRowId по content отвергает второй законный ID одной семьи; Dev/Test policy flags объединены. Нужны lawful duplicate positives в обоих порядках, точный `FORBIDDEN_CONTENT_OVERLAP` и независимая матрица Dev/Test content/family |
| R28/R117-04 /P115 | `DescribeOutput(null)` и `DescribeOutput("")` сливаются до правильного framed hash; обычный producer принимает этот Output. Helper-тест с недопустимым null Text не покрывает этот путь. Cache/framing уже существуют |
| Q127/105§5 и R154-D | В опыте обучается `KnnLogicalUnit<string,OrderedRoute>` с input-only positional encoding; обычный consumer использует другой тип. Label-only checks не связывают route с ordered payload. Сначала saved checker/authority, затем назначенное решение о реальном consumer |
| R131-04/R154-C | P131 checker не читает native-flow, а согласованная подмена actual+length в raw может пройти. Historical producer DLL ошибочно сравнивается с новой verifier DLL; список old requiredGates тоже связан с current. Перед новым binding нужен отдельный producer archive/schema; child должен получить абсолютный `/evidence` root |

Это адресное подтверждение открытых остатков, не новое разбиение всех 114 работ на «готово/не готово». [Матрица 159](codex-159-postq011-addressed-repair-plan-2026-11-04.md#existing-owners) сохраняет прежние владельцы и назначенные условия приёмки.

## 5. Проверка опытов Q002–Q011: точность, исполнение и цена

Источники таблицы — опубликованные отчёты по точным ссылкам; где выполнен новый независимый пересчёт, он указан. Исторический update не становится новым расходом этого аудита; общая стартовая подготовка и повторные/recovery стадии учитываются отдельно.

| QA / опыт | Что подтверждено и что исправлено в толковании | Граница переноса |
|---|---|---|
| Q002 /P01 | [22 файла восстановлены](../../../../experiments-v2/RP01-MICRO/P0101/results/binary-repair-2026-11-03/repair-report.md). Ранние 800SFT и recovery800SFT+8DPO — разные оплаченные стадии; другие попытки не полностью сведены. A/B2/4→2/4, новых правильных/потерянных ответов 0 | Нельзя считать repair обучением; native Load не подтверждён, NOT_GRANTED |
| Q003 /P02 | [27 файлов восстановлены](../../../../experiments-v2/RP01-MICRO/P0201/results/binary-repair-2026-11-03/codex-repair-report.md). Исходные 4SFT+8DPO и recovery4SFT+8DPO дают 24 известных updates; поздний identity readback0. A/B2/4→2/4, TRAIN1/2 и EXAM1/2 | Reference сохранён; пригодность полного continuation и native Load отдельно |
| Q004 /P03 | [Поправка точности](../../../../experiments-v2/RP01-MICRO/P0301/results/codex-review-2026-11-03/codex-report.md), независимо подтверждена: A2/4→2/4; B2/4→1/4, потерян E2, gains0. T1 NLL1,347864→1,117266, но T2/E2 хуже. Единственный предъявленный урок T1; loss по 21 next-token позициям не response-only loss | Полного final weights/moments/RNG/logits/token IDs нет в опубликованном bundle. Их нельзя восстановить из scalar/hash или U+FFFD; повтор Train не ремонт |
| Q005 /P04 | [Восемь checkpoint](../../../../experiments-v2/RP01-MICRO/P0401/results/codex-review-2026-11-04/codex-report.md),8DPO updates. A/B2/4→2/4,0 gains/losses. Независимо подтверждены 5 ухудшений gold NLL. Разные rejected у плеч делают chosen>rejected несопоставимым показателем общей accuracy | Победитель не установлен; последний pre-update margin не final. Native Load и полный конечный optimizer/RNG/logits не подтверждены |
| Q006 /P05 | [30 восстановлений P0501/P05Q01](../../../../experiments-v2/RP01-MICRO/P0501/results/codex-review-2026-11-04/codex-report.md). Исторические 8DPO+400SFT, donor400 отдельно. P0501 TRAIN0/4,EXAM0/2,полное retention0/2 до/после. P05Q01 strict/semantic0/6 на всех 21 панелях, UNQUALIFIED; A/B/EXAM не запускались | Sum/mean уже используют response mask. Контраст длины сам по себе не доказывает дефект DPO. Native Load UNRUN, HOLD |
| Q007 /P06 | [Исторический native reader и state](../../../../experiments-v2/RP01-MICRO/P0601/results/codex-review-2026-11-04/codex-report.md); см.§3.2. A/B2/4→2/4, T2/E1/E2 NLL хуже; источник движения весов установлен узко | Guard/importer gap открыт, post-load ordinary answers UNRUN; HOLD |
| Q008 /P07 | [Один исторический BPE Train](../../../../experiments-v2/RP01-MICRO/P0701/results/codex-review-2026-11-04/codex-report.md) на `ab ab ab ab`, vocab257,merge(97,98)→256. Для(a,b): joint[256] имеет 0 pure-response targets, segmented[97,98] —1; контроли(a,c)/(x,y) сохраняются. Новый saved checker0/2 подтверждён | Это обученный tokenizer и boundary effect, не приобретённый ответный навык, не RU/HE coverage. Защита Validate уже была; native82/83 UNRUN |
| Q009 /P08 | [4SFT и sampler](../../../../experiments-v2/RP01-MICRO/P0801/results/codex-review-2026-11-04/codex-report.md). Ответы 2/4→2/4; gold NLL лучше 4/4, но E1 gold-minus-opposite margin хуже. На T1 support2→1, P(gold)0,86879754→1 при том же ответе | Сужение support не означает новый правильный ответ; sampler дороже по всем 4 строкам, см.§6. NativeR30 UNRUN, HOLD |
| Q010 /P09 | [4SFT и forward parity](../../../../experiments-v2/RP01-MICRO/P0901/results/codex-review-2026-11-04/codex-report.md). Независимо пересчитаны 4×21×256=21 504 Float32 пары,84 argmax,различий 0. Ответы 2/4→2/4; золотая NLL лучше 4/4, E1 margin хуже | Это full forward против **empty-cache prefill**, не incremental decode. Ускорение и постоянная цена native call не установлены; нынешние native tests UNRUN |
| Q011 /P10 | [Один DPO update0→1](../../../../experiments-v2/RP01-MICRO/P1001/results/codex-review-2026-11-04/codex-report.md), fresh AdamW, donor400 отдельно.6 сравнений производной из 12 loss evaluations; max abs error0,000026932268, запас по abs tolerance3,713018. Ответы 2/4→2/4, NLL T1/E1 лучше,T2/E2 хуже | Second moment — проверка согласованности, не независимая производная. Два coordinates×3epsilon не доказывают весь backward; current native gradient tests UNRUN, HOLD |

В P03–P06 повторный пересчёт использовал настоящий полный сохранённый ответ и gold, независимо выведенный из арифметики prompt; готовые `Correct` сверялись, а не принимались на веру. У P05 допустимы оба заранее назначенных ответа, но строка с верным первым символом и неверным хвостом не стала правильной.

### 5.1. Что положительного сохраняется у P127/P131

Обе попытки P127: TRAIN9/10→10/10,EXAM5/6→6/6. `p2` — новый верный TRAIN, `h2` — новый верный EXAM. В 68 сохранённых строках каждой попытки route и ordered payload согласованы. Это подтверждение данной выборки. Оно не исполняет нынешний finalizer и не доказывает интеграцию в другой обычный consumer.

P131:128 raw rows,128 unique keys,128/128 совпадений с сохранённым native output по actual/verdict/flow/metrics/warnings/adapter/confidence/state version. N0/4→4/4 и 4/4 после reader; P8/8 сохранены. H0/4,X0/4 остаются ограничениями общего переноса. Согласованная подмена raw ещё должна отвергаться новым checker по первичному native output, при сохранении честного исторического positive.

## 6. Стоимость: какие числа нельзя превращать в ускорение

Для Q009 повторно пересчитаны медианы сохранённых block medians из [sampler receipt](../../../../experiments-v2/RP01-MICRO/P0801/results/attempt-0001/sampler-resource-0001/receipt.json):

| Строка | Sampler A→B, нс | B/A по этим медианам | Выделения A→B, байт |
|---|---:|---:|---:|
| T1 |94 250→251 850|2,672|10 120→25 936|
| T2 |19 600→38 200|1,949|10 120→25 872|
| E1 |22 150→47 850|2,160|10 120→25 872|
| E2 |20 600→44 300|2,150|10 120→25 872|

Это **отношение медиан**, не процент среднего paired delta из старого receipt: формулы нельзя смешивать. Наблюдался и прогрев внутри серий, поэтому результат относится к этой измерительной процедуре. Цена всего Generate здесь не измерена. Улучшение правила выбора support может быть полезно при принятой цене, но слово «ускорение» этими числами не поддерживается.

Для Q010 широкие окна 100,1104→35,3144 мс выглядят быстрее, а медианы четырёх actual calls равны 7,0148→7,2893 мс. В A есть выброс 65,4134 мс на T2. Это один записанный прогон без оценки устойчивости; ни ускорение, ни устойчивая регрессия 3,91% не установлены. Разность allocation91 712 байт включает неодинаковую обвязку. Деление на 4 даёт 22 928, но не измеренную постоянную стоимость native call. [Уже опубликованная поправка r28](../../../../experiments-v2/RP01-MICRO/P0901/results/attempt-0001/report-corrections/r28/resource-boundary.json) сохраняется; assert22 928 добавлять нельзя.

## 7. Дополнительные границы проверяющего и runner

### 7.1. Q011: численная проекция и вся цепь objective

Нынешний saved checker честно назван `VALID_SAVED_PROJECTION`. Он пересчитывает finite difference из `loss_plus/loss_minus`, связывает moments и 6 сравнений и проверяет панель. Но сохранённые `chosen_log_probability`, `rejected_log_probability`, `margin`, `loss`, `objective_derivative` на FD-ветке проверяются на finiteness; все переходы между ними не пересчитываются.

На отдельной копии изменено только первое `chosen_log_probability` на+0,1. Исходный checker по-прежнему вернул 0. Независимая проекция `logP→reference-adjusted float margin→softplus loss/derivative` обнаружила 1 несогласованную строку из 12. На честном оригинале все 12 цепей согласованы при abs tolerance1e−6. [Исполняемое свидетельство](../../../../experiments-v2/_audits/postq011-2026-11-04/codex-saved-checks.json), `saved_checkers`, сохраняет оба исхода.

Это **ограничение области проверки**, не доказательство неправильного записанного градиента. В будущую полную приёмку P10/P11/P86/P87 нужно добавить связи objective и point-in-time, не переименовывать нынешний ограниченный checker в полный verifier. Дериват из second moment остаётся consistency check. Историческая symmetry — полусумма±margin; для худшей пары её модуль 0,000361317, не удвоенная величина.

### 7.2. Напечатанный deadline не прекращает процесс

В текущих `P0701/Run-Experiment.ps1:92–130` и `P1001/Run-Experiment.ps1:99–137` вызовы build/exe синхронны, Stopwatch считывается после завершения, а пределы 600/1200 только добавляются в log. В этих двух wrappers нет механизма прекращения process tree по deadline. Это проверка исходника; зависание или нарушение исторического фактического времени не наблюдалось. Старые успешные результаты сохраняются.

Для будущего назначенного runner repair обязательны bounded child, termination/reap, nonzero terminal и сохранённые partial logs; отрицательный timeout проверяется искусственным child без модели. `runtime_deadline_seconds` в JSON не заменяет это исполнение. Владелец — существующий 03-test-and-execution-contract и соответствующий runner, без нового PRIMARY; см.159§5.

## 8. Сохранность после Q011

Фиксированный universe3502 из 154 обновлён по текущим Git trees:130 из 137 directory roots не изменились; в 7 изменившихся деревьях проверены все относящиеся к inventory пути. [Projection и authority](../../../../experiments-v2/_audits/postq011-2026-11-04/codex-current-tree-projection.json), [пересчёт и 43 дельты](../../../../experiments-v2/_audits/postq011-2026-11-04/codex-saved-checks.json).

| Категория Git identity | После Q003 | После Q011 |
|---|---:|---:|
| Равна original |49|92|
| Изменённый ранее существовавший path сохраняет identity187 |3401|3358|
| Иная identity |19|19|
| Добавленные пути исходной дельты |33|33|
| Всего |3502|3502|

Новые 43 равенства original: P0401—8,P0501—8,P05Q01—22,P0601—2,P0801—1,P0901—1,P1001—1. Они включают уже опубликованные repairs авторов Q005–Q011; эта сессия binaries не восстанавливала. У 19 иных identities прежний аудит доказал pointers к conflict text; новый массовый payload read не заявляется. У двух shared S1 P11/P12 корни и известные расхождения не изменились: применим точный readback/repair из 157, владельцы 29-D3 и 29-W1/R42. Это не отменяет исправность ранее принятых алгоритмов.

## 9. Как теперь передавать задание

В поручении указывать один существующий owner и действующий статус, затем конкретный source/attempt, фактический consumer, сохраняемый положительный случай, собственный отрицательный контроль и ожидаемый внешний исход. [Договор 51](../experimets/codex-51-postq011-evidence-boundaries-2026-11-04.md) задаёт краткий формат. [159](codex-159-postq011-addressed-repair-plan-2026-11-04.md) назначает точные изменения и проверяемые признаки; [160](codex-160-postq011-card-coverage-2026-11-04.md) показывает, какие карточки усилены и почему.

В этом пакете исправлены документация, адресные задания и проверяемая evidence. Production, существующие tests/runners, данные, gold, старые raw и бинарные файлы не изменены. Поэтому открытые дефекты кода остаются открытыми до своего назначенного исполнения. Публикация документов не является научной победой или native acceptance.
