# FIX-BR — явный критерий исполнимых слияний при обучении BPE

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q39](01-registry.md#work-fix-br). Для общего включения действует измеренное ограничение этой строки. Возврат к работе — по записанному условию для её домена. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

## Уточнение допуска по показателям — 3 ноября 2026

**FIX-BR: Не допускать общий режим.** Вошёл по лямбдам; другой профиль значительно проиграл. Нельзя выдать один выигравший профиль за универсальный перенос.

- Точность: Текст восстанавливается точно в A и B; точность модели не проверялась.
- Полезный результат: Лямбды: 12 → 9 токенов (−25%); учебный корпус 24 → 23 (−4,17%).
- Цена и ограничения: Комментарий 5 → 9 токенов (+80%); вся экзаменационная панель 20 → 21 (+5%, ровно порог).
- [Полное сравнение и статус кода](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#benefit-fix-br).

Этот вывод относится к названным ID, а не ко всем механизмам документа. Старый текст плана и опыта сохранён как доказательство, но его общий статус «к переносу / разрешён» не переопределяет это уточнение. Ближайшее действие — разбор уже сохранённых данных и конкретного регресса; новое обучение, повтор PRIMARY и переключение product default этим дополнением не назначаются. Для отдельного логического ремонта назвать нарушенный контракт и сохранить отрицательные контроли, а не переносить весь эксперимент под словом «bugfix».


<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски BPE:** У BpeTokenizer.Train отдельно requested/achieved/merges/reason и настоящий читатель outcome; законный roundtrip сохраняется, shortfall не скрывается. Все вложенные и twin BPE fits входят в полный бюджет; больший request не доказывает больший достигнутый vocab.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->
<!-- POST102-AUDIT-2026-11-02:START -->
## Действующее уточнение после P102 — Результат Train и downstream capacity

**Проверка `POST102-FIX-BR`. WRONG.** Requested size равен достигнутому vocab, либо одинаковые vocab с разными запросами составляют состоявшийся A/B factor.

**CORRECT.** Outcome с достигнутым размером/причиной уже реализован; проверять его actual consumer. Критерий merge и эффект downstream длины — разные вопросы. Learned tokenizer state подписывается вместе с model V.

**Адресная приёмка.** Корпус с исчерпанными парами показывает shortfall без silent success; actual gold token length проверяет decoder cap. Не назначать заново реализованный return outcome, не добавлять скрытый второй BPETrain для oracle. Unknown token/UTF-8 policy остаются R08, не чинятся диапазонной подрезкой словаря.

Применять [единый контекст40](../experimets/40-effective-executor-context-2026-11-02.md) и [адресный план121](121-post102-addressed-repair-plan-2026-11-02.md): актуальные роли навыков, один рабочий оценщик, ordinary consumer и реакция на test/report failure. Это ограничивает вывод и усиливает приёмку; исходные factor, доза, fixed panel и порядок сохраняются. Исправление saved evidence не является новой PRIMARY, а наличие этого блока не разрешает запуск. Исторические указания ниже действуют только в своей исходной области; при противоречии бюджета/фактора требуется адресное решение до исполнения.
<!-- POST102-AUDIT-2026-11-02:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Проверять actual vocabulary/merges, выученные в этом fixture, и полученные ими IDs на входе назначенного downstream learner/reader. Подстановка архивных token IDs вместо выхода нового tokenizer доказывает только downstream component. Время/allocations измеряются своим окном, placeholder не считается measured; отсутствие сравнительного peak не записывается как0.

Для FIX-BR исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Экономия обучения токенизатора проверяется вместе с результатом выученных merges/IDs и исходным учебным объёмом. Не смешивать число представленных повторов с числом уникальных фрагментов или token positions; у каждого показателя свой знаменатель. Контроль использует выученный tokenizer и тот же реальный downstream reader, а не только constructor. Ресурсное окно включает заранее названные стадии, без приписывания writer allocations обучающему алгоритму.

Применимость: FIX-BR. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Сохранить назначенный реальный этап обучения tokenizer и его веса повторов; до/после сравнивать одни и те же уникальные строки и неизменный gold. Число вызовов Train, обработанных фрагментов и реально изменённых merges — разные счётчики. Принятие включает качество downstream обученной модели и все обязательные контроли, а не только неизменный общий процент или успешный выход runner.

Применимость: FIX-BR. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

**Применение нового критерия допуска, 27 октября 2026.** «Успех по оси» ниже означает наблюдённую дельту, а не разрешение общего внедрения. Lambda12→9 даёт25% улучшения, но comment5→9 —80% ухудшения; такой общий профиль не проходит правило69. Общий EXAM20→21 ухудшается ровно на5%, его нельзя ошибочно назвать>5%; запрещающим известным срезом здесь остаётся comment. Opt-in lambda — только ограниченный кандидат с собственными метриками, без переноса новой цели в legacy KNN320/default Train. State/ordered merges сохраняются и читаются собственным codec; при restore нельзя обучить новый tokenizer на fixture и выдать его за сохранённый. Планы четырнадцати успешных Train и одного rejected call не расширяются.

**Уточнение исполнения, 27 октября 2026.** Legacy objective и новый opt-in сравниваются с разными заранее закреплёнными oracle. Недопустимо пересчитать golden новым алгоритмом и назвать это совместимостью старого Train. Улучшение lambda не скрывает ухудшение comment/всего EXAM: сохранить все заданные группы в отчёте. При наличии FIX-BPE/BW держать их одинаковыми в обеих руках; иначе меняется более одного фактора и исходное основание BR не изолировано.

Общие примеры правильной и ошибочной проверки — [контракт03§24](03-test-and-execution-contract.md#transfer-audit-20261027). Это дополнение плана; новые проверки сейчас не запускались.

ID: **FIX-BR**. Статус: **кандидат ограниченного переноса; native реализация и тесты ещё не выполнены**. Основание — завершённый **BPE-REALIZED-MERGE-COUNT-01 / BR01**, впервые опубликованный на `70062c090a55d7350e5eadb2606dd3f07953692f`. Тема — язык программирования / документация кода; lambda, comment и neutral — три профиля одной темы. Новый критерий даёт полезное измеренное сокращение lambda и TRAIN, одновременно ухудшая comment и весь EXAM. Поэтому план вводит **явный training opt-in**, сохраняющий прежний двухаргументный `Train` и всех его callers.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности для этого не обязателен. Все остальные дельты, включая ухудшения, показываются рядом.** Здесь обучается словарь и таблица BPE merges, а не ответы нейросети. Сжатие не объявляется точностью KNN или качеством генерации. Текущая задача — документация, без build/test/Train и без изменения продукта.

## 1. Первичные доказательства и граница успеха

Проверенные источники на одном immutable commit:

- [A.json](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/experiments-v2/RP01-MICRO/BR01/r/0001/A.json), [B.json](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/experiments-v2/RP01-MICRO/BR01/r/0001/B.json) — полные per-arm метрики, TRAIN/EXAM IDs, no-update control и terminal outcome.
- [Program.cs](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/experiments-v2/RP01-MICRO/BR01/Program.cs), [source-copy-manifest.json](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/experiments-v2/RP01-MICRO/BR01/source-copy-manifest.json) и [intent.jsonl](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/experiments-v2/RP01-MICRO/BR01/r/0001/intent.jsonl) — настоящий Train, один фактор CountPairs, завершённые A/B.
- [fixture.json](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/experiments-v2/RP01-MICRO/BR01/in/fixture.json), [A-initial-state.json](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/experiments-v2/RP01-MICRO/BR01/r/0001/A-initial-state.json), [A-final-state.json](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/experiments-v2/RP01-MICRO/BR01/r/0001/A-final-state.json), [B-final-state.json](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/experiments-v2/RP01-MICRO/BR01/r/0001/B-final-state.json) — прочитаны полные JSON, проверены bytes/hash.
- [Технический отчёт](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/experiments-v2/RP01-MICRO/BR01/r/0001/report.md) — контекст и ограничения. `comparison.json` в доступном Git tree — LFS pointer; его payload не выдаётся за прочитанный. Таблица ниже пересчитана по A/B, а не по недоступной сводке.

Historical source — `883496bca18113bf37e0afe683c75b05a0075048`. Native `01.Tokenizer/BpeTokenizer.cs` на70062c имеет тот же blob `d7c5e69308a32bf832ec4b087097a35b26f51d64`; RegexSplitter — `ad3eed613fb90777af20ed07b6f8340b8c16bfd9`. A неизменён, B отличается только телом CountPairs. Native проект и product caller в опыте не собирались: это source-copy свидетельство, которое будущий перенос обязан проверить на `01.Tokenizer/Tokenizer.csproj`.

| Показатель | BEFORE в обеих руках | A: частота соседств | B: исполнимые замены |
|---|---:|---:|---:|
| Vocab / learned merges | 256 / 0 | 257 / 1 | 257 / 1 |
| Первая learned pair | отсутствует | `(47,47)→256`, `//` | `(61,62)→256`, `=>` |
| TRAIN token count | 27 | 24 | 23 |
| Lambda E1–E3, сумма tokens | 12 | 12 | 9 |
| Comment E4, tokens | 9 | 5 | 9 |
| Neutral E5, tokens | 3 | 3 | 3 |
| Весь EXAM, tokens | 24 | 20 | 21 |
| Roundtrip всех строк | точный | точный | точный |
| No-update control | byte-only S0 | тот же S0 | тот же S0 |
| Allocated bytes внутри Train | неприменимо | 26 176 | 26 176 |
| Train wall, мс | неприменимо | 5,4786 | 4,4362 |
| Train CPU, мс | неприменимо | 15,625 | 15,625 |

Выигрыш B относительно A: lambda **−3 токена / −25%**, TRAIN **−1 токен**. Ухудшения: comment **+4 токена**, EXAM в целом **+1 токен**. CPU равен одному грубому шагу таймера; один холодный A→B запуск не доказывает ускорение. Gen0/1/2 равны0, peak не измерен. Не переносить результат на vocab320, перевод, весь C# corpus или downstream accuracy. Новое обучение действительно было: состояние 256/0→257/1, новые IDs используются в Encode, контроль без Train не меняется; это не «правильных ответов0→0».

Новизна относительно [FIX-BPE](12-bpe-no-match-copy.md) и [FIX-BW](31-bpe-weighted-chunks.md): те оптимизируют ресурсы с **сохранением** прежнего выбора merges. FIX-BR сознательно меняет учебный objective. Их legacy overlap oracle не исправляют под BR; новый режим проверяется отдельно. Если оптимизации уже приняты, они одинаковы в контролях, а BR не запускает их перенос заново.

## 2. Где и как менять продукт

Область product diff — только [01.Tokenizer/BpeTokenizer.cs](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/01.Tokenizer/BpeTokenizer.cs): `Train:42–68`, `CountPairs:526–538` и объявление небольшого enum в том же namespace. `RegexSplitter`, `MergeChunk:550–566`, `ApplyMerges`, Encode/Decode и canonical Save/Load остаются прежними.

1. Добавить `public enum BpePairCountObjective { OverlappingFrequency = 0, RealizedReplacements = 1 }` и **отдельный** overload `Train(string text, int vocabSize, BpePairCountObjective objective)`. Существующий точный `Train(string,int)` сохраняется и вызывает legacy objective. Optional третий параметр вместо старого overload не подходит: это ломает reflection/API identity. Не выбирать режим из текста, имени домена, текущих частот или HTTP metadata.
2. До сброса `_vocab`/`_merges` проверить enum; неизвестное значение отвергать `ArgumentOutOfRangeException` с `ParamName="objective"`. Действующий vocab guard сохраняется. Политика относится к конкретному вызову Train; Load не угадывает её, последующий двухаргументный Train опять выбирает legacy и, как сегодня, начинает обучение словаря заново.
3. Передавать objective в private CountPairs. Legacy считает все соседние позиции. Realized для self-pair `(x,x)` считает нечётные соседние рёбра каждого непрерывного run: `selfEdges=0` в начале каждого chunk; при self-pair увеличить, чётный пропустить; при non-self сбросить. Для `(x,y), x≠y` прежний счёт сохраняется. Первое появление ключа, порядок chunks и MaxBy ties не меняются.
4. Если FIX-BW уже принят, тот же подсчёт выполняется внутри каждого weighted representative; каждое учтённое вхождение прибавляет его Multiplicity. Сброс run между representatives обязателен. Это композиция двух явно назначенных правил, не разрешение вернуть duplicated chunks или потерять multiplicity. Если BW не принят, вес равен1.
5. `_merges`, nextToken, stop condition и MergeAll сохраняют прежний смысл. В сохранённом tokenizer достаточно упорядоченных merges и vocab; inference не нуждается в training objective. Формат canonical state и его limits не меняются. Существующие model checkpoints не мигрируются и не переобучаются.

Настоящий downstream caller — [KnnLogicalUnit.Train:136–153 / Vectorize:266–282](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L136-L153): он учит BPE320, затем кодирует examples для HNSW и запрос для Execute. **Этот caller сохраняет двухаргументный legacy Train**, поскольку положительный опыт BR257 не доказывает пользу нового objective в его пространстве векторов. План готовит явный native механизм для принятого code-documentation профиля257; включение его в KNN320, production routing или иную тему не является частью этого кандидата. Так измеренная польза сохраняется без глобального переноса известной comment-регрессии.

## 3. Математическое подкрепление и ограничения

Обоснование выведено из конкретных [CountPairs / MergeChunk](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/01.Tokenizer/BpeTokenizer.cs#L526-L566), не из общего обещания, что «новый BPE лучше».

Для пары `(x,y)` с разными IDs два соседних вхождения не могут перекрываться: общий средний token потребовал бы `x=y`. Поэтому число её соседних вхождений совпадает с числом замен MergeChunk. Для run из L одинаковых IDs прежняя частота равна `L−1`, а неперекрывающихся замен ровно `floor(L/2)`. Greedy MergeChunk слева направо реализует именно их. Для одного шага:

~~~text
realized(x,y) = sum по chunks числа замен MergeChunk для данной пары;
realized(x,x) = sum по runs floor(L_run / 2);
длина после шага = длина до шага − realized(selected_pair).
~~~

При одинаковых входных chunks выбор максимального realized даёт максимальное **немедленное** сокращение этой обучающей последовательности за один merge. Для weighted chunks каждое слагаемое умножается на Multiplicity. Это не теорема глобально оптимального словаря на несколько шагов и не гарантия сжатия EXAM: после первого шага состояния расходятся. В BR01 `///////` даёт legacy6 против realized3, четыре `=>` дают4 в обоих критериях; поэтому A выбирает `//`, B — `=>`. На отложенном длинном comment A закономерно лучше.

Тесты §5 защищают чётные/нечётные run, сброс между run/chunks, non-self пары, ties и повторное применение к уже выученным IDs. Математика обосновывает точность подсчёта, но не расширяет измеренный домен и не заменяет native acquisition/readout.

## 4. Точные данные, начальное состояние и сохранение результата

Будущая fixture: `Tests/Fixtures/ExperimentFixes/v1/BpeRealizedMerge01/`. Скопировать **bytes** `in/fixture.json` и `r/0001/A-initial-state.json` из pinned BR01 в `fixture.json` и `initial-tokenizer.json`; raw A/B хранить как evidence отдельно от исполняемых expectations. Static `manifest.json` содержит источник, следующие hashes, literal corpus/expected IDs и бюджет; будущие baseline/candidate SHA находятся в runtime receipts, а не меняют tracked fixture после A.

| Identity | Значение |
|---|---|
| fixture bytes / SHA256 | 458 / `979c0820a3681360e1f75027a0ab40bbe859cfd116010da914bd76559ea6435e` |
| Corpus bytes / SHA256 | 27 / `b07ec7b8dbd7f319c2997554312f70853ba0cd10264dd3e329b33424284d684c` |
| Initial canonical bytes / SHA256 | 3261 / `3bba3bc0d53fb07a5de4d28839f14c2b296b8034de37e5d561b123a9dcf95bf4` |
| Legacy after SHA256 | `040851bc349e2c6cdb8e4e1089f989009d81901eed51aec867a0671f9e15a272` |
| Realized after SHA256 | `d93e193d77ec6e6f55e1086a998e8bd30b723f645e759a805c1310d683902db2` |

TRAIN — `string.Join("\n", train_lines)`, UTF8 без BOM и без завершающего LF; `Environment.NewLine` здесь запрещён. Пять строк: `///////`, `x=>x`, `y=>y`, `z=>z`, `w=>w`. Vocab target **257**. EXAM не входит в TRAIN:

| ID / профиль | Text | Legacy after IDs | Realized after IDs |
|---|---|---|---|
| E1 lambda | `a=>a` | `[97,61,62,97]` | `[97,256,97]` |
| E2 lambda | `b=>b` | `[98,61,62,98]` | `[98,256,98]` |
| E3 lambda | `c=>c` | `[99,61,62,99]` | `[99,256,99]` |
| E4 comment | `/////////` | `[256,256,256,256,47]` | `[47,47,47,47,47,47,47,47,47]` |
| E5 neutral | `q+r` | `[113,43,114]` | `[113,43,114]` |

Основные captures **загружают готовое одинаковое byte-only S0**, по два отдельных native `Load` для learner и no-update control; пустой `Train("",256)` заново не нужен. Это начальная память токенизатора, не «почти обученная нейросеть». Готовый after нельзя подставить вместо единственного содержательного Train: тогда изменённый writer не проверяется. Каждый capture делает один Train257, сохраняет canonical `tokenizer.json`, receipt и фактические IDs; fresh Load этого after используется для cold readout без повторного Train. Source/raw/donors read-only. Полученные файлы затем используются сравнивателем и E2E metadata, без нового обучения ради отчёта.

## 5. Постоянные native tests и минимальный бюджет

Планируемые файлы:

- `Tests/ExperimentFixes/BpeRealizedMergeLearningTests.cs`;
- `Tests/ExperimentFixes/BpeRealizedMergeContractTests.cs`;
- `Tests/ExperimentFixes/BpeMergeObjectiveInvoker.cs` — только разрешение native overload, без копии алгоритма;
- общий `Tests.Support/ExperimentTransfer/Run-NativeAB.ps1`, отдельная матрица `PlanId=FIX-BR`.

Два learning Fact в namespace `LlmFromScratch.Tests.ExperimentFixes`:

1. `BpeRealizedMergeLearningTests.LegacyProfile_PreservesLearnedDictionary` — выполняется один раз на A и один раз на B, всегда точный двухаргументный Train. Это контроль неизменности default при появлении opt-in.
2. `BpeRealizedMergeLearningTests.RealizedProfile_LearnsOneMergeAndReportsAllProfiles` — только B, один вызов нового overload с `RealizedReplacements`. Отсутствующий native API — ошибка, fallback к legacy запрещён.

Общий test source компилируется на A: enum/новый overload разрешаются **лениво внутри** B-only Facts/Theory, по имени `Tokenizer.BpePairCountObjective` и точной сигнатуре `(string,int,enum)`. Discovery A не ищет новый API. До измерения helper строит `Action<BpeTokenizer,string,int>` через expression-call native метода с постоянным enum; legacy использует такой же delegate. Разрешение reflection/компиляция delegate не входят в timed Train. ARM отвечает только за provenance; метод и requested objective заданы literal test case, а не угадываются по результату.

Каждый основной capture: проверить bytes/S0/source → Load learner и отдельный control → по5 EXAM Encode/Decode обоих +1 TRAIN Encode learner → ровно один measured Train257 learner → те же20 совокупных before/after EXAM пар и второй TRAIN Encode → Save after → fresh Load →5 cold EXAM пар. Control не получает Train. Все assertions читают эту же квитанцию. Проверить приобретение 256/0→257/1, exact pair/IDs/canonical hash, точный Decode и неизменный control; вывести все пять профилей, не выбрать только удачные lambda. В A-legacy и B-legacy всё состояние совпадает; B-realized совпадает с собственным pinned gold, **не** с A.

Три основных процесса вместе: **3 содержательных Train, 0 byte-init Train, 75 EXAM Encode/Decode-пар, 6 TRAIN Encode, 3 final Save и 9 Load**. Reflection delegates вызывают настоящий `Tokenizer.BpeTokenizer` из native project. Перед первым Load/Train сохранить AssemblyName/Location/SHA256/MVID и source/build provenance; archived BR01 Runner не принимается за native B.

Контрактный B-only Theory `BpeRealizedMergeContractTests.RealizedCounts_MatchIndependentMergeOracle` содержит ровно семь cases. Здесь `\n` означает один LF в C# literal. Новый tokenizer на case получает один nonempty Train с явным Realized, затем Save для чтения merges и один Encode/Decode corpus; expected не вычислять тем же CountPairs.

| Case | Corpus | Vocab | Expected merges по порядку |
|---|---|---:|---|
| even-run | `aaaa\nbb\nbb\nbb` | 257 | `(98,98)→256` |
| odd-run | `aaaaa\nbb\nbb\nbb` | 257 | `(98,98)→256` |
| reset-within-chunk | `bc\nbc\naaXaaYaa` | 257 | `(97,97)→256` |
| reset-between-chunks | `bc\nbc\naa\naa\naa` | 257 | `(97,97)→256` |
| distinct-pair | `abab` | 257 | `(97,98)→256` |
| first-tie | `ab\ncd` | 257 | `(97,98)→256` |
| learned-self-id | `aaaaaaaa` | 258 | `(97,97)→256`, затем `(256,256)→257`; Encode=`[257,257]` |

Это математические boundary tests будущей реализации, **DESIGNED_NOT_MEASURED**; они не добавляют исторических успехов или новых тем. Seven cases =7 Train,8 learned merges,7 roundtrip-пар. Восьмой B-only Fact `InvalidObjective_RejectsBeforeChangingLearnedState` загружает уже сохранённый `learning/B/realized/tokenizer.json`, вызывает новый Train с enum99, требует `ArgumentOutOfRangeException("objective")` и прежний state hash. Один rejected API call, **0 успешных fits**, один дополнительный Load; не обучать donor для этого Fact.

### Существующие проверки и ремонт падений

Назначить только два existing Fact до и после product diff, с полными prefix `FullyQualifiedName=LlmFromScratch.Tests.`:

| Method | Зачем / как разбирать падение |
|---|---|
| `TokenizerTests.SaveAndStateFingerprintShareCanonicalCompleteSerialization` | Сохранить complete canonical bytes/hash и stream rewind. B-only mismatch default означает ошибку делегирования objective/порядка merges; чинить Train, не сериализатор или expected hash |
| `KnnLogicalUnitTests.KnnLogicalUnit_TrainsWithBpeAndHnswThenPredictsNearestOutput` | Сохранить реальный legacy BPE320→Vectorize→HNSW→Execute, исходные две labelled строки, k1/dim32, SECURITY/evidence/confidence. Не включать Realized в caller и не менять output, k или predicate features ради PASS |

Источники: [TokenizerTests.cs](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/Tests/TokenizerTests.cs), [KnnLogicalUnitTests.cs](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/Tests/KnnLogicalUnitTests.cs). Эти четыре executions дают4 дополнительных BPE Train, включая2 прежних KNN fits/4 labelled rows и2 Execute. Их данные не подмешиваются в BR corpus. Счётчики их внутренних Encode сохраняются отдельно от новых82 пар и6 TRAIN Encode; отсутствие native counter помечается `not_exposed`, не выдуманным нулём.

Полный назначенный расход: **14 успешных BPE Train =3 captures+7 boundary+4 existing; ещё1 отвергнутый API call, 0 updates в нём**. Из14 нет новых пустых initialization. В новых тестах82 EXAM/contract roundtrip-пары +6 TRAIN Encode; existing операции отдельно. Две project rebuild/discovery,6 test processes: existingA, legacyA, existingB, legacyB, realizedB, contractsB. Discovery A=3 выбранных cases, B=12 (2existing+2learning+7Theory+1invalid). Никаких SFT/DPO/LoRA updates или model API calls.

Свежих падений эта сессия не запускала. При A failure зафиксировать baseline defect. При B-only ошибка exception/enum — проверять guard **до** reset; неправильный `//` в Realized — run reset/count/ties; неправильное cold состояние — Save/rewind/Load/provenance. Не удалять assertions, не делать Skip и не повышать vocab/число уроков до желаемого результата. Исторический overlap-контракт FIX-BW остаётся legacy и не переписывается под новый objective.

## 6. Точное исполнение двух стадий

Только canonical `C:/Projects/llm_mvp_core`, ветка `codex/dpo-implementation`. Future output: `artifacts/fbe/FIX-BR/attempt-0001/`; основные receipts — `learning/A/legacy`, `learning/B/legacy`, `learning/B/realized`; contracts — `contracts/B`. [03§9](03-test-and-execution-contract.md#native-ab-transfer) задаёт общий immutable intent / completed baseline-stage / candidate-binding, Rebuild, process ownership и provenance, **но не его GB/RMS/BW filters/budget**. FIX-BR имеет собственные три captures и B-only contracts, заданные здесь; raw сравнивает `A-legacy↔B-legacy` и `A-legacy↔B-realized` как две разные проверки.

После отдельного назначения FIX-BR создать common harness/fixtures commit на прежнем продукте, включив обязательную [предпосылку Tests build,02§10](02-learning-state-and-fixtures.md#native-build-prerequisite) одинаково для обеих стадий. Его SHA — `$baselineCommit`. Затем:

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-BR -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit
~~~

Baseline: один `dotnet build Tests/LlmFromScratch.Tests.csproj -c Release -t:Rebuild`, exact discovery3cases → existingA → legacyA → `BASELINE_COMPLETE`. После этого назначенный product diff, review и candidate commit в том же checkout; common tests/fixtures/SDK/ISA/config неизменны. Requested objective каждого capture заранее записан в одном static manifest; он не подбирается по measured output. SHA нового commit — `$candidateCommit`:

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-BR -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit -CandidateCommit $candidateCommit
~~~

Candidate: Rebuild → exact discovery12cases → existingB → legacyB → realizedB → contractsB → comparison/report. A не повторяется. Learning filters — точный `FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.` плюс каждое имя выше; existing filter — два FQN через `|`; B contract filter — `FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.BpeRealizedMergeContractTests.`. Каждый dotnet test использует `-c Release --no-build`, свой TRX/output и последовательный запуск.

Runner явно задаёт `XIIGEN_FBE_SOURCE_ROOT`, `XIIGEN_FBE_OUTPUT`, `XIIGEN_FBE_ARM`, `XIIGEN_FBE_PAIR_ROOT`; root fixtures вычисляется относительно SourceRoot, invalid-objective Fact читает ровно completed B-realized по PairRoot. В timed interval только delegate Train; parse/hash/Save/Load/readout вне него. Отчёт содержит обе пары сравнений, цены всех профилей и границу single cold process; исторический процент скорости не становится CI threshold.

Deadline5min/test process,15min/Rebuild. Найти в актуальном каталоге инструментов настоящий continuation/wait **до запуска**. При returned session/task handle ждать тот же handle bounded вызовами≤60секунд до terminal/deadline; output с задержкой не доказывает потерю процесса. Не заканчивать ход обещанием «доложу позже», не создавать polling loops/дублирующие waiters и не запускать второй fit. Failed/partial receipts сохраняются; formatter-only repair читает готовые raw. Completed stage возвращает старые квитанции, live stage продолжает наблюдаться. Новый attempt, удаление старого output, worktree/checkout/reset/branch switch не назначены.

## 7. E2E, отчёт и промпт исполнителю

[FIX-E2E](20-prompt-domain-e2e.md#resource-transfer-e2e) сохраняет prompt→learned topic→registered native receiver→actual response→cold restore. BR не обучал outer selector и не добавляет тему/keyword-dispatch. У существующего receiver с tokenizer сравнивается **его собственная** state identity до/после restore; нельзя подменить его BPE320 нашим BR257 или из prompt выбрать training objective. Если recipient не использует BPE — `not_applicable`. BR after читается только там, где принят именно этот профиль, без нового Train; существующий KNN320 остаётся legacy. Дополнение к E2E добавляет0 запросов/0fits и не выдаёт tokenizer compression за измеренный успех маршрутизации.

Русские `report.md` и `report-for-user.md` в PairRoot, формат [03§8](03-test-and-execution-contract.md): source/data/assembly identity; что изменено; BEFORE/AFTER по каждому capture; A-legacy/B-legacy parity; all-profile A/B deltas; actual budget; failure/not_reached; граница и кандидат решения. Не повторять Train ради summary. Сохранённое улучшение по конкретному профилю признаётся успехом даже при неизменной accuracy; comment/total regression не скрывать.

~~~text
Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/40-bpe-realized-merge-objective.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.

Выполни только отдельно назначенный FIX-BR по40-bpe-realized-merge-objective.md.
Текущий этап до назначения — только планирование. Модель текущей сессии default, без override.
Skills из docs/skills/: architect-behavior-classifier-SKILL.md, phase-planning-SKILL.md,
plan-review-SKILL.md, generated-code-review-SKILL.md, prompt-patch-authoring-SKILL.md,
human-communication-GLOSSARY.md, output-readability-gate-SKILL.md; также общие роли03§4.
Сначала текущие AGENTS.md/CLAUDE.md; не угадывай другое имя/папку skills.
Canonical root=C:/Projects/llm_mvp_core; branch=codex/dpo-implementation.
Harness=Tests.Support/ExperimentTransfer/Run-NativeAB.ps1; PlanId=FIX-BR.
Fixture=Tests/Fixtures/ExperimentFixes/v1/BpeRealizedMerge01; output=artifacts/fbe/FIX-BR/attempt-0001.
Baseline и Candidate запускаются точными командами§6. Product diff только01.Tokenizer/BpeTokenizer.cs.
Новый objective явный; default legacy, KNN320 и learned outer routing не переключать.
3 основных captures,7 boundary Train,4 existing Train; ещё1 rejected call без fit. Повторы запрещены.
Используй сохранённый S0/after по§4; не переучивай донор и не подставляй after вместо writer-test.
Любое измеренное улучшение полезного показателя — успех для показателя/домена; accuracy расти не обязана.
Покажи все ухудшения, особенно comment/total EXAM. Отчёты по-русски, точные paths в§7.
Жди тот же process handle до terminal; не заканчивай ход вместо ожидания, не запускай polling loop или новый fit.
STOP/NEXT Любы действует немедленно: прекрати старую задачу, не дописывай её отчёт/код/commit/cleanup.
Следующий ID бери из01-registry.md, а не из старого номера опыта. Старые эксперименты не удалять.
~~~

Ни положительный BR01, ни эта карточка не разрешают автоматическое внедрение всего пакета. Решение о переносе принимает пользователь; нынешнее поручение разрешает публикацию плана.
