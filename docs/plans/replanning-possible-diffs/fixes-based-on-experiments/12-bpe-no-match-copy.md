# FIX-BPE / BPE-NO-MATCH-COPY-01 — перенос экономии памяти внутри обучения токенизатора

<!-- transfer-entry-reload -->
**Вход исполнителя.** [R11](01-registry.md#work-fix-bpe). Механизм уже существует. Для этого поручения достаточно названного ниже наблюдения на работающем пути. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

<a id="r11-next"></a>
## R11. Измерить, сколько памяти экономит уже перенесённая ветвь

В [MergeChunk](../../../../01.Tokenizer/BpeTokenizer.cs) при отсутствии пары возвращается исходный список. В [BpeNoMatchCopyTests](../../../../Tests/BpeNoMatchCopyTests.cs) уже проверяются слияния, выученный словарь и чтение после сохранения. Остался один вопрос — уменьшаются ли выделения **за Train** при сегодняшнем способе учитывать повторяющиеся фрагменты. Активный объём заканчивается этим измерением.

1. Взять только профиль C# из [сохранённого входа](../experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/B/code-doc.json). Поля `train_strings` (5 строк), `corpus_repeat_count` (8), `target_vocabulary` (320) и `exam_strings` (4 строки) уже записаны. Соединить учебные строки по порядку через LF, без завершающего LF. Экзаменационные строки идут только на чтение. В этом опыте учится сам BPE.
2. В рабочей копии исполнитель приёмки уже добавил `BpeNoMatchCopyTests.TrainingTheSavedCodeProfileReportsHowManyBytesItAllocates`. Использовать этот метод из локальной попытки `artifacts/fbe/R11-BPE-NOMATCH-ALLOCATIONS/attempt-0001`. При работе в чистой копии дождаться публикации этой попытки её исполнителем. Он измеряет выделенные байты вокруг обычного Train. Для обеих версий использовать этот корпус и один и тот же тест. В контроле заменить только возврат списка при отсутствии пары на копирование тех же элементов. В проверяемой версии использовать обычный `BpeTokenizer.Train`. Взвешивание повторов и прочие условия в обоих вариантах одинаковы. Для контроля использовать локальную временную подмену в этом же методе и вернуть его до завершения; параллельную сборку из этого дерева в это окно не запускать. Не переносить в продукт вторую реализацию токенизатора.
3. Одна содержательная тренировка на вариант. В одном и том же синхронном окне вокруг `Train` записать разность `GC.GetAllocatedBytesForCurrentThread()`; сборку корпуса, проверку и вывод отчёта вести за пределами окна. После обучения записать число слияний, размеры словаря и число строк, где `Decode(Encode(text))` вернул исходный текст. На старой панели это 64 слияния, словарь 320, точное чтение 4/4 у обеих версий. Для пары, которая встретилась, сохраняется обычное слияние. Для отсутствующей пары копия меняет расход памяти при тех же токенах.
4. Из корня core запустить `dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter FullyQualifiedName~TrainingTheSavedCodeProfileReportsHowManyBytesItAllocates`. Для сохранения чисел добавить к каждому запуску `--logger "trx;LogFileName=bpe.trx" --results-directory <каталог-варианта>`, подставив разные каталоги для контроля и переноса. В отчёт выписать имя измерившего теста, обе величины, разность и результат декодирования. При несовпадении сначала проверить корпус, версию ветви и границы измерения. После результата перейти к [очереди](01-registry.md#executor-next).

В исходном опыте C# потребовал 3 904 664 → 1 126 744 Б за обучение. CPU вырос с 31,25 до 46,875 мс. Второй профиль EN→RU отдельно дал 2 892 824 → 893 656 Б, восстановление EXAM 4/4 и тот же CPU. Сегодняшний замер сообщает свои числа. Исторические указания ниже про два новых проекта, все профили, полный повтор и неизменность каждого байта относятся к первоначальному плану. [Сохранённые результаты обоих профилей](../experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/report.md).

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски BPE:** У BpeTokenizer.Train отдельно requested/achieved/merges/reason и настоящий читатель outcome; законный roundtrip сохраняется, shortfall не скрывается. Все вложенные и twin BPE fits входят в полный бюджет; больший request не доказывает больший достигнутый vocab.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Проверять actual vocabulary/merges, выученные в этом fixture, и полученные ими IDs на входе назначенного downstream learner/reader. Подстановка архивных token IDs вместо выхода нового tokenizer доказывает только downstream component. Время/allocations измеряются своим окном, placeholder не считается measured; отсутствие сравнительного peak не записывается как0.

Для FIX-BPE исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Экономия обучения токенизатора проверяется вместе с результатом выученных merges/IDs и исходным учебным объёмом. Не смешивать число представленных повторов с числом уникальных фрагментов или token positions; у каждого показателя свой знаменатель. Контроль использует выученный tokenizer и тот же реальный downstream reader, а не только constructor. Ресурсное окно включает заранее названные стадии, без приписывания writer allocations обучающему алгоритму.

Применимость: FIX-BPE. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Сохранить назначенный реальный этап обучения tokenizer и его веса повторов; до/после сравнивать одни и те же уникальные строки и неизменный gold. Число вызовов Train, обработанных фрагментов и реально изменённых merges — разные счётчики. Принятие включает качество downstream обученной модели и все обязательные контроли, а не только неизменный общий процент или успешный выход runner.

Применимость: FIX-BPE. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

**Адресное усиление после R4, 27 октября:** [FU-04/67](67-transfer-repair-followup-audit-2026-10-27.md), [U4/68](68-transfer-repair-followup-plan-2026-10-27.md). Старые ordered hashes/IDs уже закреплены; ресурсная ось no-match остаётся открытой. `BpeWeightedChunkTests` с отношением8×/1× защищает BW, не этот фактор: одинаковая лишняя копия в обеих руках может улучшить отношение. Хорошая проверка требует того же объекта списка на no-match и правильного нового результата на match; затем измеряет native Train с одинаковым BW в обеих руках. Плохая — только roundtrip/hash или ratio соседнего опыта. Expected берётся из старого tokenizer, не второго candidate. Comparator не добавляет Train; native расход остаётся в рецепте этой карточки. Не объявлять полную готовность по прежней фразе66 «ничего открытого».

**Уточнение исполнения, 27 октября 2026.** Код внесён в `f317cd7ee32`; повторно переносить его не нужно. `BpeNoMatchCopyTests` сравнивает два обучения новой реализацией: это детерминизм, а не равенство старому алгоритму. Закрепить полный старый tokenizer и ordered merges из назначенного опыта, сравнить token IDs на обеих панелях и проверить, что изменение публичного результата Encode не меняет последующий ответ. Экономию копий подтверждать на настоящем Train; round-trip сам по себе её не доказывает.

Общие примеры правильной и ошибочной проверки — [контракт03§24](03-test-and-execution-contract.md#transfer-audit-20261027). Это дополнение плана; новые проверки сейчас не запускались.

Статус: **план кандидата**, 21 октября 2026. Новый код и новые тесты ещё не написаны, запусков в этой документной сессии не было. Основание — уже выполненный `BPE-NO-MATCH-COPY-01`. Проверенный текущий код: `4f35cb71b3db482bfe7ebe611c652c78d89d5db1`; исторический код опыта: `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`.

Порядок исполнения задаёт [реестр](01-registry.md). Обязательные общие условия: [состояние и наборы](02-learning-state-and-fixtures.md), [тестирование и исполнение](03-test-and-execution-contract.md). Исполнитель использует свою модель по умолчанию. Этот план требует выполнения только после отдельного решения пользователя о переносе.

## Зачем переносить и что нового в плане

При обучении BPE выбранная пара встречается лишь в небольшой части кусков текста. Сейчас каждый вызов `MergeChunk` выделяет новый список и копирует весь кусок даже при отсутствии пары. Удачный опыт убрал эти копии, сохранив выученные слияния и закодированные ответы. Перенос нужен для уменьшения стоимости реального обучения словаря; улучшение точности перевода или ответов KNN этим опытом не доказано.

В [реестре кандидатов](../required-changes.md#bpe-no-match-copy-01) уже есть результат и адрес изменения. Здесь добавлены отсутствовавшие решения: граница владения возвращаемым списком, неизменяемый тестовый набор, конкретные продуктовые тесты, правила исправления возможных падений и проверка вызова из обучаемой единицы. Это не новая карточка и не повтор старого опыта.

## Измеренное основание

Первичные источники: [A/code-doc](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/A/code-doc.json), [B/code-doc](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/B/code-doc.json), [A/en-ru](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/A/en-ru.json), [B/en-ru](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/B/en-ru.json), [отчёт с ограничениями](../experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/report.md).

| Показатель | C# / документация | EN→RU |
|---|---:|---:|
| Выделено байт за Train: A → B | 3 904 664 → 1 126 744 | 2 892 824 → 893 656 |
| Экономия | 2 777 920 байт, 71,1% | 1 999 168 байт, 69,1% |
| Среднее число токенов EXAM: до → после | 43 → 21,75 в обоих плечах | 48 → 21,75 в обоих плечах |
| Выученный словарь / слияния | 256/0 → 320/64 | 256/0 → 320/64 |
| Точное восстановление EXAM | 4/4 до и после | 4/4 до и после |
| CPU: A → B | 31,25 → 46,875 мс | 46,875 → 46,875 мс |
| Кусков без пары в B | 41 152 из 42 432 | 28 424 из 30 144 |

Хэш конечного состояния совпал между A и B внутри каждой темы. Успех относится к выделениям памяти во время обучения; рост точности для этого не нужен. Рост CPU на первой теме показан рядом. Время и пик памяти одного процесса — наблюдения без доказанной причинности: был один A→B, без перестановки порядка. Нулевые A counters означают отсутствие дополнительной B-инструментации, а не отсутствие копий в A. Не усреднять два домена в один учебный набор.

## Точки переноса и точный алгоритм

Путь обучения: [`BpeTokenizer.Train`](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/01.Tokenizer/BpeTokenizer.cs#L42) → `CountPairs` → `MergeAll` → [`MergeChunk`](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/01.Tokenizer/BpeTokenizer.cs#L548). Продуктовый потребитель — [`KnnLogicalUnit<TInput,TOutput>.Train`](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L136): обучает BPE на текстах уроков, затем этим токенизатором строит векторы для HNSW. Изменение непосредственно участвует в обучении.

Единственный продуктовый файл первой правки: `01.Tokenizer/BpeTokenizer.cs`. Изменить только тело `MergeChunk`:

1. Начать проход без нового списка. Найти первое совпадение выбранной пары слева направо.
2. Если совпадения нет, вернуть входной `ids` без записи в него.
3. При первом совпадении создать `List<int>(ids.Count)`, скопировать неизменённый префикс, добавить `newToken`, продолжить прежний левосторонний проход с шагом два на слиянии.
4. Сохранить семантику неперекрывающихся слияний: для четырёх одинаковых токенов выполнить два слияния; для трёх — одно и оставить хвост. Не менять выбор пары, разрешение ничьих, порядок `_merges`, UTF-8, словарь или сериализацию.

Владение: `MergeChunk` не мутирует вход при любом исходе. Список без совпадения может быть тем же объектом; текущие `MergeAll`, `ApplyMerges` и `Encode` лишь заменяют ссылку/читают его. Кэша между вызовами нет. Не заменять `TensorStorage`, KNN, HNSW, числовые векторы или learning policy ради этой оптимизации. Если при переносе выявится новый caller, который мутирует возвращённый список в предположении отдельной копии, сначала зафиксировать этот caller и исправить его владение; не скрывать разницу переписыванием ожидаемых ответов.

## Набор и сохранённый результат для теста

Создать **при реализации** два неизменяемых fixture-файла: `Tests/Fixtures/ExperimentFixes/v1/BpeNoMatchCopy01/code-doc.json` и `en-ru.json`. Перенести из соответствующего raw A только `train_strings`, `corpus_repeat_count`, `train_sha256`, `exam_strings`, `before`, `after`, `state_before_sha256`, `state_after_sha256`, `target_vocabulary` и ссылку на источник. Значения не генерировать заново. Сборка корпуса: точные строки ×8 и LF из исходного raw; до Train проверить UTF-8 SHA256.

| Тема | SHA256 TRAIN | SHA256 состояния после |
|---|---|---|
| code-doc | `4525d08f34536fa86c97f0123ad1140585838496afba4f4a483bede8a863bccb` | `9910827ac8c1eb65e75081fcb593115328ad44b092bc22dd2e5ff87779d5b9d8` |
| en-ru | `a6774ee12d42c83fe1f40275184dea9c7d224df7c333e804784b3bc030cdde9a` | `4b35570ece5bde6ad5461928a7fbe4e078ded7a2d0eaa836bd2490a352b16824` |

Минимальный учебный переход на одну тему: новый BPE → `Train("",256)` → четыре BEFORE-readout → `Train(corpus,320)` → четыре AFTER-readout. Один readout означает один Encode и один Decode. На тему: восемь readout и два Train, из них один содержательный. Для обеих тем — 4 Train, 16 Encode и 16 Decode. Сборка корпуса — `string.Join("\n", trainStrings repeated eight times)`, без завершающего LF; длины 2303/2615 UTF-8 bytes и SHA сверены по raw. EXAM никогда не добавлять в TRAIN; строка повторов — отдельный явно обозначенный контроль состава, а не доказательство далёкого обобщения. Проверить каждый массив token IDs, `ComputeStateSha256()`, 64 слияния, точный Decode. Базовая модель, LoRA, SFT и DPO в этом тесте не нужны: учащийся механизм — BPE.

Сохранённый tokenizer после Train нужен как независимый golden и для reload; начинать сам тест изменения Train с уже обученного токенизатора нельзя, иначе изменяемый learning writer не будет выполнен. Это обоснованное исключение из повторного использования обученного старта. Исторические результаты не перезаписывать.

## Тесты и возможные падения

Ниже — план будущих тестов, а не утверждение, что они уже написаны или что текущий тестовый набор падает. В этой сессии build/test не запускались.

| Файл / тест | Что проверять или изменить при переносе |
|---|---|
| Новый `Tests/TokenizerLearningTransferTests.cs`, `BpeNoMatchCopy_PreservesBothLearningTransitions` | Два `[Theory]` case по отдельным fixture; BEFORE/AFTER, точные IDs/Decode и canonical hash из таблицы. Вызывает настоящий проект Tokenizer, без linked копии файла как основного тестируемого пути. |
| Тот же файл, `BpeNoMatchCopy_PreservesPrefixOverlapAndUnmatchedTail` | Через реальные Train/Encode на малых строках подтвердить первое совпадение после префикса, отсутствие пары, пару в начале/конце, нечётные повторы и Unicode. Результаты фиксировать из независимого ручного разбора/исторической A, не вычислять новым MergeChunk. |
| Тот же файл, `BpeNoMatchCopy_DoesNotMutatePriorEncodedResults` | На **том же экземпляре** tokenizer закодировать строку и сохранить returned List и его копию; выполнить другие Encode, проверить неизменность первого результата. Затем изменить один элемент публичного returned List и снова Encode исходную строку: новые IDs должны совпасть с golden, а не с изменённым списком. Это проверка независимости публичных результатов; не объявлять её прямой проверкой идентичности private MergeChunk List. Изолированный другой tokenizer для проверки aliasing недостаточен. |
| `Tests/TokenizerTests.cs`, `SaveAndStateFingerprintShareCanonicalCompleteSerialization`, `SaveLoad_PreservesVocab` | Сохранить строгие проверки canonical bytes, SHA и reload. Несовпадение после правки — ошибка продукта, а не повод обновить golden. |
| `Tests/TokenizerEdgeCaseTests.cs`, `Encode_LongerTextAfterTraining_ProducesFewerTokensThanBytes`, `SaveLoad_EncodesIdenticallyAfterReload` | Сохранить проверки реального обучения и reload; добавить точные fixture лишь в новый файл, не заменить старые сценарии. |
| `Tests/KnnLogicalUnitTests.cs`, `KnnLogicalUnit_TrainsWithBpeAndHnswThenPredictsNearestOutput` | Оставить как проверку настоящего downstream Train и Unit. Её один exact-query не объявлять независимым качеством на EXAM. |

Ресурсный приёмочный замер оформить **при отдельно назначенной реализации** в новых `Tests.Support/BpeNoMatchCopy01/Runner.csproj` и `Tests.Support/BpeNoMatchCopy01/Program.cs`. Это уточнение будущих путей: существующие файлы или опыты не перемещаются. Прежний планируемый адрес внутри проекта 35 не используется, поскольку SDK glob родительского `35.ExperimentsV2/ExperimentsV2.csproj` включил бы новый Program и baseline sources в другую сборку. [Общая предпосылка, 02§10](02-learning-state-and-fixtures.md#native-build-prerequisite) исключает только архивные Rp01Micro, поэтому размещать новый runner под проектом 35 и расширять Remove на него не следует. Новый короткий путь находится вне проектов 35 и Tests; product Theory остаётся в `Tests/TokenizerLearningTransferTests.cs`.

Два будущих проекта имеют `TargetFramework=net9.0`, `ImplicitUsings=enable`, `Nullable=enable` и `EnableDefaultCompileItems=false`. Их точный состав:

| Будущий файл | Состав сборки |
|---|---|
| `Tests.Support/BpeNoMatchCopy01/Runner.csproj` | `OutputType=Exe`, `AssemblyName=Xiigen.Fbe.BpeNoMatchCopy01.Runner`; единственный `Compile Include="Program.cs"`; `ProjectReference Include="../../01.Tokenizer/Tokenizer.csproj"` для B и `ProjectReference Include="Baseline/BpeNoMatchCopyBaseline.csproj"` для A |
| `Tests.Support/BpeNoMatchCopy01/Baseline/BpeNoMatchCopyBaseline.csproj` | `OutputType=Library`, `AssemblyName=Xiigen.Fbe.BpeNoMatchCopy01.Baseline`; ровно `Compile Include="BpeTokenizer.cs"` и `Compile Include="RegexSplitter.cs"`; нет ProjectReference на проект 35 или native Tokenizer |
| `Tests.Support/BpeNoMatchCopy01/Baseline/BpeTokenizer.cs`, `RegexSplitter.cs` | Исходные тексты обоих файлов из `01.Tokenizer/` на `4f35cb71b3db482bfe7ebe611c652c78d89d5db1`, с единственной заменой `namespace Tokenizer;` на `namespace Xiigen.Fbe.BpeNoMatchCopy01.Baseline;`; алгоритмы, using, serialization, Train и MergeChunk не переписываются |

`RegexSplitter` обязателен: исходный BPE вызывает его в [Train/Encode:53,76](https://github.com/lubochka/llm_mvp_core/blob/7eab1172bb3787eed9c1d966931a856df95f4976/01.Tokenizer/BpeTokenizer.cs#L53-L76), а [отдельный RegexSplitter](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/01.Tokenizer/RegexSplitter.cs) принадлежит тому же namespace. Его отсутствие в baseline assembly или использование текущей изменённой копии было бы другим baseline. Проверка текстового diff каждого baseline source допускает только назначенную namespace-замену. Сохранить source commit, original file SHA-256/Git blob, namespace transformation и отдельный SHA-256 фактических compiled source bytes; эти две группы hashes нельзя объявлять одинаковыми. Записать manifest в `Tests.Support/BpeNoMatchCopy01/Baseline/source-manifest.json` до запуска и скопировать ссылку/hash в resource receipts.

Runner выбирает A/B только по `--arm`; B создаёт настоящий `Tokenizer.BpeTokenizer`, A — `Xiigen.Fbe.BpeNoMatchCopy01.Baseline.BpeTokenizer`. Не ссылаться на baseline из Tests/35 и не добавлять рекурсивный `Compile Include`; runner не компилирует файлы Baseline повторно. До первого Train в каждом из уже назначенных четырёх процессов записать AssemblyName/Location/SHA256 выбранного типа, SHA source-manifest и текущего product commit. Для B ожидается библиотека `Tokenizer`; для A — `Xiigen.Fbe.BpeNoMatchCopy01.Baseline`. Эта проверка использует тот же процесс, не добавляет readout или обучение.

После создания будущих проектов и до четырёх процессов сделать только evaluation их состава, без targets/restore:

```powershell
dotnet msbuild Tests.Support/BpeNoMatchCopy01/Runner.csproj -nologo -p:Configuration=Release -getProperty:AssemblyName -getItem:Compile,ProjectReference
dotnet msbuild Tests.Support/BpeNoMatchCopy01/Baseline/BpeNoMatchCopyBaseline.csproj -nologo -p:Configuration=Release -getProperty:AssemblyName -getItem:Compile,ProjectReference
```

Сохранить выводы как `artifacts/fbe/FIX-BPE/attempt-0001/build/bpe-runner-items.json` и `bpe-baseline-items.json`, CreateNew. Требовать ровно назначенные прямые Compile и ProjectReference, не включать generated targets. Не запускать отдельный build/Train ради этой проверки: `dotnet run` ниже соберёт свой назначенный host. Общий Tests prerequisite по документу 02 применяется к отдельной Tests-команде, не превращает эти resource процессы в новый запуск проекта 35. Все перечисленные файлы **планируются**, сейчас runner не создан и команды не выполнялись. Аргументы runner фиксированы: `--arm A|B --domain code-doc|en-ru --fixture <один из двух назначенных fixture> --out <назначенный каталог плеча>`.



**Сводный бюджет переноса.** Указанные выше 4 Train / 16 Encode / 16 Decode относятся только к двум product Theory cases. Каждый из четырёх ресурсных A/B×domain процессов выполняет тот же минимальный переход: один пустой Train256 и один содержательный Train320, четыре before и четыре after readout. Ресурсная часть добавляет 8 Train (4 пустых + 4 содержательных), 32 Encode и 32 Decode. Вместе основные product+resource проверки: **12 Train (6 пустых + 6 содержательных), 48 Encode и 48 Decode**. Это шесть содержательных обучений BPE, а не двенадцать нейронных запусков. Корпусы/число слияний не расширяются; ресурсные процессы не повторяют работу друг друга. Malformed/alias/малые chunk companions и существующий regression suite имеют отдельные test receipts и не выдаются за часть этого учебного A/B. Save/load проверки используют уже полученное состояние: повторный Train ради восстановления запрещён. Если конкретный test method требует дополнительных вызовов, их счёт указывается до запуска; они не скрываются внутри 48 readout.

После реализации запустить ровно четыре отдельных процесса в порядке A/code-doc, B/code-doc, A/en-ru, B/en-ru с одинаковым `Release` и runtime. Измерять только содержательный Train, сохранять выделенные bytes и полные learning readouts. Корень результата — строго `artifacts/fbe/FIX-BPE/attempt-0001/resources/`; четыре каталога плеч ниже назначены заранее. CreateNew без autosuffix: если каталог соответствующего плеча уже существует, читать его результат и исходный handle, не запускать вновь, не удалять и не выбирать attempt-0002. Старый consumed experiment не запускать. Полностью назначенные будущие команды из корня репозитория:

```powershell
dotnet run --project Tests.Support/BpeNoMatchCopy01/Runner.csproj -c Release -- --arm A --domain code-doc --fixture Tests/Fixtures/ExperimentFixes/v1/BpeNoMatchCopy01/code-doc.json --out artifacts/fbe/FIX-BPE/attempt-0001/resources/code-doc/A
dotnet run --project Tests.Support/BpeNoMatchCopy01/Runner.csproj -c Release -- --arm B --domain code-doc --fixture Tests/Fixtures/ExperimentFixes/v1/BpeNoMatchCopy01/code-doc.json --out artifacts/fbe/FIX-BPE/attempt-0001/resources/code-doc/B
dotnet run --project Tests.Support/BpeNoMatchCopy01/Runner.csproj -c Release -- --arm A --domain en-ru --fixture Tests/Fixtures/ExperimentFixes/v1/BpeNoMatchCopy01/en-ru.json --out artifacts/fbe/FIX-BPE/attempt-0001/resources/en-ru/A
dotnet run --project Tests.Support/BpeNoMatchCopy01/Runner.csproj -c Release -- --arm B --domain en-ru --fixture Tests/Fixtures/ExperimentFixes/v1/BpeNoMatchCopy01/en-ru.json --out artifacts/fbe/FIX-BPE/attempt-0001/resources/en-ru/B
```

Это новая проверка переноса в настоящий продукт после назначения реализации. Сравнение исторического Windows raw с новым другим runtime не выдавать за строгое A/B. В обычном CI не требовать буквальных исторических миллисекунд/процентов. Корректность состояния и Encode — строгая; полезный ресурсный результат считать по измеренному направлению изменения при одинаковых данных/runtime, без требования роста точности.

Диагностика падения: если поменялись IDs/hash/порядок merges — исправлять MergeChunk. Если fixture hash не совпадает — исправлять загрузку bytes/LF, не корпус. Если падает только временной порог — убрать недоказанный временной oracle и оставить измерение в receipt; исходную потерю времени явно показать. Если тест строился на отдельной копии native файла — перевести именно проверяемый B путь на `ProjectReference`, сохранив baseline только в сравнительном стенде.

Команды из корня репозитория, **после реализации**:

```powershell
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName~TokenizerLearningTransferTests|FullyQualifiedName~TokenizerTests|FullyQualifiedName~TokenizerEdgeCaseTests|FullyQualifiedName~KnnLogicalUnitTests" --logger "trx;LogFileName=bpe-transfer.trx" --results-directory artifacts/fbe/FIX-BPE/attempt-0001/tests
```

Если tests-каталог уже существует, сначала прочитать существующий результат и handle, а не перезаписать TRX или назначить другой attempt. Ожидаемый результат: все выбранные тесты прошли; учебные A/B данные совпадают по каждой строке; каждый новый тест реально обнаруживается `--list-tests` после его добавления. Нулевой список выбранных тестов — ошибка назначения. Полную Windows/solution цепочку запускать только если конкретный изменённый контракт требует её по общему плану, не как предварительное условие признания уже измеренного выигрыша.

## Архив первоначального поручения

Для текущей приёмки исполнитель берёт [R11 и её команду](#r11-next).

> Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/12-bpe-no-match-copy.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.
>
> Выполни только принятый план `12-bpe-no-match-copy.md` в `codex/dpo-implementation`. Прочитай общий контракт03, состояние02, реестр01 и проектные `docs/skills/intent-to-plan-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `test-integrity-SKILL.md`, `human-communication-GLOSSARY.md`. Модель — твоя текущая по умолчанию. Измени только перечисленные продуктовый метод и тесты; исходные эксперименты/результаты не удаляй и не переобозначай. Работай с раздельными TRAIN/EXAM и двумя темами. Отчёт на русском: механизм до/после, точные данные, источник, команды/exit, полезный показатель, все ухудшения, что не доказано. Любое измеренное улучшение полезного показателя внутри обучения — успех по этому показателю и теме; роста точности не требуй. Живой процесс жди штатным wait по его исходному handle; не запускай повтор из-за отсутствия лога. Прямая команда STOP немедленно прекращает работу. При прямом NEXT немедленно передай краткий фактический статус и перейди к названному ID; запрещено продолжать прежний опыт, дописывать его отчёт, чинить прошлые тесты или запускать его заново. Следующий ID брать только из реестра, не из старого порядкового номера. DPO здесь не генерируется; если отдельное принятое задание потребует его, внешняя открытая китайская модель создаёт chosen/rejected, CODEX с `astra 6 ultracode` выполняет человеческий выбор и оценку качества; не приписывай такую оценку текущему тесту.



## Разделение с поздним FIX-BW

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** BPE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

[FIX-BW](31-bpe-weighted-chunks.md) по завершённому BW01 добавлен следующим ID после FIX-BPE. Его фактор — weighted representatives в Train/CountPairs/MergeAll, а здесь остаётся только no-match copy в MergeChunk. При отдельно выбранном совместном переносе уже принятая правка одинаково присутствует в обеих руках следующего A/B; не склеивать resource проценты и не заменять один опыт другим. Собственные fixtures, runner, бюджет и assertions этой карточки сохраняются. Для точного следующего ID использовать01; это уточнение не назначает запуск BW01 или реализацию обоих кандидатов сразу.
