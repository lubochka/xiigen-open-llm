# 31. Учить BPE на взвешенных представителях одинаковых фрагментов

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски BPE:** У BpeTokenizer.Train отдельно requested/achieved/merges/reason и настоящий читатель outcome; законный roundtrip сохраняется, shortfall не скрывается. Все вложенные и twin BPE fits входят в полный бюджет; больший request не доказывает больший достигнутый vocab.

**Ожидания тестов:** собственные данные/positive/negative этой карточки и названный ordinary caller имеют приоритет над примерами120. Пример другого семейства не добавляет fixture, Train или новый API. Полный набор outer/nested Train/BPE/Grammar, readouts, restore и этапов остаётся в существующем бюджете. Структурный guard сTrain0 не объявляется новым обучением.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Проверять actual vocabulary/merges, выученные в этом fixture, и полученные ими IDs на входе назначенного downstream learner/reader. Подстановка архивных token IDs вместо выхода нового tokenizer доказывает только downstream component. Время/allocations измеряются своим окном, placeholder не считается measured; отсутствие сравнительного peak не записывается как0.

Для FIX-BW исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Экономия обучения токенизатора проверяется вместе с результатом выученных merges/IDs и исходным учебным объёмом. Не смешивать число представленных повторов с числом уникальных фрагментов или token positions; у каждого показателя свой знаменатель. Контроль использует выученный tokenizer и тот же реальный downstream reader, а не только constructor. Ресурсное окно включает заранее названные стадии, без приписывания writer allocations обучающему алгоритму.

Применимость: FIX-BW. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Сохранить назначенный реальный этап обучения tokenizer и его веса повторов; до/после сравнивать одни и те же уникальные строки и неизменный gold. Число вызовов Train, обработанных фрагментов и реально изменённых merges — разные счётчики. Принятие включает качество downstream обученной модели и все обязательные контроли, а не только неизменный общий процент или успешный выход runner.

Применимость: FIX-BW. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

**Усиление после R4:** независимые old-state hashes и ресурсный ratio repeated/unique защищают собственный фактор BW; [FU-04/67](67-transfer-repair-followup-audit-2026-10-27.md) запрещает переносить это закрытие на BPE no-match. В обеих руках BW-проверки FIX-BPE одинаков. Плохие реализации: вес каждого представителя=1; lexical sort вместо first occurrence; пропуск последнего фрагмента. Они должны отличаться от закреплённых ordered merges/IDs, даже если roundtrip ещё работает. Цена измеряется в реальном Train; копирование одного и того же корпуса для8× не считается новым смысловым доменом. Не выводить CPU из allocation ratio, показывать оба показателя отдельно. Старые learned gold сохраняются, budget не расширяется повторным обучением ради expected. [Общий контракт03§25](03-test-and-execution-contract.md#repair-followup-20261027).

**Уточнение исполнения, 27 октября 2026.** Код внесён в `8565b6cdb32` поверх FIX-BPE. Повтор текста ×8 и два одинаковых candidate fit проверяют полезные свойства, но не доказывают полный legacy-parity. Сверять ordered merges, vocab bytes, fingerprint и exact EXAM IDs с закреплённым A; частоты и first-occurrence tie — отдельные контроли. Для этого фактора FIX-BPE должен присутствовать одинаково в A/B. Уникальный corpus остаётся отдельной строкой затрат, а проценты двух BPE-оптимизаций не складываются.

Общие примеры правильной и ошибочной проверки — [контракт03§24](03-test-and-execution-contract.md#transfer-audit-20261027). Это дополнение плана; новые проверки сейчас не запускались.

ID: **FIX-BW**. Статус: **план переноса BPE-WEIGHTED-CHUNKS-01 / BW01**, только документация по `7a9fa58d55127ae88e93cd5d71c24a579f730ca3`. Два раздельных профиля: **документация программ / C#** и **перевод / EN→RU**. Учится словарь BPE, а не переводчик или router. Порядок — после FIX-BPE; следующий ID читается в [реестре](01-registry.md).

## 1. Измеренное основание и его границы

Четыре первичных receipts: [A-code-doc](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/BW01/results/attempt-0001/A-code-doc.json), [B-code-doc](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/BW01/results/attempt-0001/B-code-doc.json), [A-en-ru](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/BW01/results/attempt-0001/A-en-ru.json), [B-en-ru](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/BW01/results/attempt-0001/B-en-ru.json). Дополнительные источники — [report](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/BW01/results/attempt-0001/report.md), [one-factor.diff](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/BW01/one-factor.diff), [Program](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/BW01/Program.cs).

| Показатель single Train | C# A→B | EN→RU A→B |
|---|---:|---:|
| Allocated bytes | **3 926 152→767 328** (−3 158 824, ≈−80,46%) | **2 907 816→567 672** (−2 340 144, ≈−80,48%) |
| Wall, ms | 36,1466→9,6570 | 34,8632→8,3162 |
| CPU, ms | 31,25→0 | 31,25→15,625 |
| Pair positions visited | 46 992→5 011 | 60 584→4 632 |
| MergeChunk calls | 42 432→2 368 | 30 144→1 728 |
| Вхождений / уникальных chunks | 663 / 37 | 471 / 27 |
| Peak working set, bytes | 37 277 696→33 964 032 | 36 302 848→33 566 720 |
| Vocabulary / learned merges, до→после в каждой руке | 256/0→320/64 | 256/0→320/64 |
| Сумма EXAM token counts, до→после в каждой руке | 172→87 | 192→87 |

До/после состояния и все EXAM IDs/roundtrip в A/B совпадают. Zero CPU — дискретность счётчика короткого процесса, не отсутствие работы. GC0/1/2=0 в обоих профилях. Измеренных ухудшений в этих двух receipts не обнаружено, но корпус намеренно повторный (5 строк×8). Для почти уникального corpus ресурсный выигрыш не установлен. Исторические A/B содержали одинаковые счётчики; их cost не равен чистому неинструментированному production Train. Изменение compression — реальный результат обучения словаря, не улучшение перевода, понимания C# или HNSW поиска.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и профилю, роста accuracy не требуется.** Здесь успешен расход ресурсов настоящего BPE Train с одинаковым приобретённым состоянием. Все будущие ухудшения также показываются; универсальная польза по всем доменам не заявляется.

На проверенном срезе `comparison.json` доступен как Git LFS pointer, а `status.json` не содержит полных records; у fixture осталось историческое `DESIGNED_NOT_RUN`. Поэтому числа выше выведены из четырёх полных arm JSON, не из недоступной summary. Завершённые Train не повторять из-за дефекта финального контроллера; старые metadata/raw не переписывать.

Новизна относительно FIX-BPE: тот убирает лишнюю копию внутри `MergeChunk` при отсутствии пары. Этот план уменьшает число представленных повторов **с сохранением их веса** в Train/CountPairs/MergeAll. Это разные факторы. Если FIX-BPE уже принят, его MergeChunk одинаков в A/B FIX-BW; если не принят, остаётся старым в обеих руках. Проценты −71% и −80% нельзя складывать, перемножать или считать независимыми при совместном переносе.

## 2. Точный код и настоящий потребитель

Product diff ограничен [01.Tokenizer/BpeTokenizer.cs](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/01.Tokenizer/BpeTokenizer.cs): `Train:42–68`, `CountPairs:521+`, `MergeAll:536+` и private `WeightedChunk`. `MergeChunk`, Encode/Decode, Save/Load/ComputeStateSha256, RegexSplitter и public API не меняются.

Предписанная реализация:

1. Перебирать прежний `RegexSplitter.Split(text)` в исходном порядке. `Dictionary<string,WeightedChunk>(StringComparer.Ordinal)` объединяет только **идентичные исходные строки**. Отдельный List хранит представителей в порядке первого появления; у представителя TokenIds, Multiplicity, FirstOccurrenceOrdinal.
2. При первом вхождении создать прежние UTF8 byte IDs, weight1; при повторе увеличить Multiplicity. Не нормализовать строку/Unicode/регистр и не сортировать.
3. В CountPairs пройти каждую соседнюю позицию представителя, **включая перекрывающиеся пары**, и прибавить Multiplicity к прежнему `int` счётчику. Не считать пару один раз на distinct chunk и не менять overflow semantics отдельным фактором.
4. Выбор `MaxBy`, ранний stop без пар, назначение nextToken, словарь/merges остаются прежними. First-occurrence order сохраняет порядок первого добавления пар и прежнее разрешение равных частот. Повторный одинаковый chunk не мог раньше внести новую пару; после одинаковых merges это свойство сохраняется.
5. MergeAll вызывает неизменённый MergeChunk один раз на представителя, заменяет TokenIds, сохраняет weight и порядок. Не объединять заново chunks по совпавшим post-merge IDs, не делать общий static cache.

В native [KnnLogicalUnit.Train:103–169](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L103-L169) этот BPE обучается на текстах examples до Vectorize/HnswIndex; ExecuteAsync кодирует запрос тем же выученным tokenizer. Это конкретная связь подготовки с обучением и результатом. Не менять vectorDimensions, k, HNSW, class labels или predicate features.

KnnLogicalUnit использует `Environment.NewLine`, исторический BW — LF. Не объявлять его corpus fingerprint идентичным BW. Это отдельный native consumer regression из §5, с собственными двумя examples. DPO canonical tokenizer loader также получает неизменный сериализованный словарь; pass загрузки сам по себе не доказательство обучения.

## 3. Данные, сохранённое состояние и два независимых профиля

Future fixture root: `Tests/Fixtures/ExperimentFixes/v1/BpeWeightedChunks01/`: exact `fixture.json`, `manifest.json`, четыре historical arm JSON. Источник [BW01/in/fixture.json](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/BW01/in/fixture.json). Loader использует только назначенные текстовые поля и рецепт; старый status не превращает выполненные arm receipts в несуществующие.

Каждый corpus: строки T1…T5 повторить **весь блок восемь раз**, соединить LF, без завершающего LF, UTF8 без BOM. Домены в одном Train не смешивать.

### C# / code-doc

~~~text
int Add(int a, int b) => a + b; // return the sum
int Subtract(int a, int b) => a - b; // return the difference
int Multiply(int a, int b) => a * b; // return the product
string Echo(string value) => value; // return the same text
// identifiers: alpha beta gamma delta aaaa aaaaa aaaaaa
~~~

EXAM (четыре строки, не входят в Train):

~~~text
int Add(int left, int right) => left + right; // return the sum
int Subtract(int left, int right) => left - right; // return the difference
aaaa aaaaa aaaaaa
// value: Ω + ё
~~~

### Перевод / en-ru

~~~text
Dana opened the door. => Дана открыла дверь.
Dana did not open the door. => Дана не открыла дверь.
Mira closed the window. => Мира закрыла окно.
Mira did not close the window. => Мира не закрыла окно.
book books door doors окно окна аааа ааааа
~~~

EXAM:

~~~text
Nora opened the window. => Нора открыла окно.
Nora did not open the window. => Нора не открыла окно.
аааа ааааа aaaa aaaaa
Ёж увидел окно; Ω.
~~~

| Identity | C# | EN→RU |
|---|---|---|
| Corpus bytes | 2303 | 2615 |
| Corpus SHA256 | `4525d08f34536fa86c97f0123ad1140585838496afba4f4a483bede8a863bccb` | `a6774ee12d42c83fe1f40275184dea9c7d224df7c333e804784b3bc030cdde9a` |
| Learned state SHA after64merges | `9910827ac8c1eb65e75081fcb593115328ad44b092bc22dd2e5ff87779d5b9d8` | `4b35570ece5bde6ad5461928a7fbe4e078ded7a2d0eaa836bd2490a352b16824` |
| EXAM token counts after | [36,36,5,10] | [23,25,17,22] |

Initial byte-vocabulary state SHA везде `3bba3bc0d53fb07a5de4d28839f14c2b296b8034de37e5d561b123a9dcf95bf4`. Один `Train("",256)` создаёт его; затем один `Train(corpus,320)` учит64merges. Этот cold writer-test не заменять загрузкой готового after-tokenizer: тогда изменённый Train вообще не будет проверен. Нейронная модель/LoRA/optimizer не используются, готовый S0 им не нужен.

После каждой руки сохранить canonical `tokenizer.json` в её output, SHA/merge list/EXAM IDs в receipt. Fresh tokenizer загружается из этих **уже созданных bytes** для cold-readout без Train; последующие проверки используют тот же файл. Источник и старые результаты read-only. Не создавать новую систему model checkpoints ради BPE.

## 4. Постоянное учебное покрытие на native BPE

Планируемые файлы: `Tests/ExperimentFixes/BpeWeightedChunksLearningTests.cs`, `BpeWeightedChunksContractTests.cs`; общий runner03. Два основных Fact:

- `CodeDocumentation_NativeTrain_PreservesLearnedState`;
- `EnglishToRussian_NativeTrain_PreservesLearnedState`.

Каждый Fact запускается отдельным test process для одного arm/profile. Один source harness используется последовательно в canonical `C:/Projects/llm_mvp_core` на `codex/dpo-implementation`; B обязан происходить из обычного проекта `01.Tokenizer/Tokenizer.csproj`, не archived BW runner. Сначала Baseline выполняет A/code-doc и A/en-ru; после сохранения всех A receipts и назначенного product diff/review/commit Candidate выполняет B/code-doc и B/en-ru. Четыре основных процесса и их учебный бюджет сохраняются; A после изменения кода не повторяется.

В Fact: validate corpus/hash → byte init → четыре before Encode/Decode → **один** измеряемый native Train320 → четыре after Encode/Decode → canonical Save → fresh Load → четыре cold Encode/Decode. Проверить 256→320/0→64, exact bytes/roundtrip, все IDs и canonical state hash, список merges; A/B состояния точно совпадают. Gold — исходные bytes и закреплённые historical IDs/hash, не результат собственного CountPairs. Поштучный token count не заменяет проверку IDs.

Allocation на текущем потоке/wall/CPU/GC только вокруг Train320; parse/hash/readout/Save/Load вне измерения. Peak явно process-wide. Не переносить experiment counters в production; pairPositions/calls остаются историческим объяснением. Resource gate не фиксирует −80% для CI: новый receipt сообщает фактические deltas, а обязательный permanent oracle сохраняет acquired dictionary. Если на общей базе с FIX-BPE дополнительный allocation gain отсутствует, зафиксировать это, не повторять до успеха.

Основные четыре processes: **4 содержательных Train + 4 byte-init Train**, 32 before/after Encode/Decode-пары +16 cold-пар =48, 4 Save/4 Load, 0 neural/Generate. Всё состояние одного Fact используется всеми assertions; split assertions не запускают новый fit.

## 5. Контрактные границы и существующие падающие тесты

Шесть маленьких Theory cases в `BpeWeightedChunksContractTests.WeightedFrequencyAndTieOrder_MatchesIndependentOracle`, по одному native Train/case/arm. Здесь `\n` — C# escape одного LF в строковом литерале, не два буквальных символа backslash+n.

| Case | Corpus | Target vocab | Независимый expected learned pair |
|---|---|---:|---|
| first-ab | `ab\ncd` | 257 | token256=(97,98) |
| first-cd | `cd\nab` | 257 | token256=(99,100) |
| multiplicity | `ab\ncd\ncd` | 257 | token256=(99,100), повтор нельзя выбросить |
| overlap | `aaaa\nbb\nbb\nbb` | 257 | token256=(97,97): три перекрывающихся aa против трёх bb, первый aa |
| near-unique | `ab\ncd\nef` | 258 | token256=(97,98), token257=(99,100) |
| unicode | `ёё\nΩΩ\nёё` | 257 | token256=(209,145), UTF8 ё; normalization отсутствует |

По каждому case также exact roundtrip. Это проверка семантики Train для границ переноса, не новый ресурсный эксперимент и не основание обещать выигрыш на unique корпусах. **12 дополнительных маленьких Train** в двух руках включены в общий бюджет.

Назначенный baseline/after filter содержит ровно следующие три существующих Fact namespace `LlmFromScratch.Tests`:

| Method | Что сохраняется / как ремонтировать |
|---|---|
| `TokenizerTests.SaveAndStateFingerprintShareCanonicalCompleteSerialization` | Полные canonical bytes и state hash. При падении искать изменение learned merges/порядка; не менять expected fingerprint под actual |
| `TokenizerEdgeCaseTests.SaveLoad_EncodesIdenticallyAfterReload` | IDs до/после Load без повторного Train |
| `KnnLogicalUnitTests.KnnLogicalUnit_TrainsWithBpeAndHnswThenPredictsNearestOutput` | Настоящие BPE fit→HNSW→Execute: ровно исходные две labelled строки, k1/dim32, SECURITY output и evidence |

Последний существующий test — native consumer smoke, не доказательство общего prompt→domain E2E или улучшения nearest-neighbour accuracy. Сохранить исходные `jwt oauth authorization token` / `layout screen accessibility design`, predicates и query. Не подмешивать их в два BW corpus.

Эти три tests вызывают по одному BPE Train каждый: baseline3 + after3 = **6 дополнительных Train**; у Knn два Train/HNSW fit и два Execute суммарно. Вместе с основными8 и контрактными12 весь назначенный расход — **26 BPE Train calls** (из них4 пустые byte init), 0 SFT/DPO/neural training. Other internal Encode/Decode existing tests остаются их прежними операциями и сохраняются отдельно от48 основных readout-пар. Не запускать весь Tokenizer/Knn suite без конкретного дополнительного риска.

Существующие tests канонической загрузки `DesignConversionDpoCanonicalBpeTokenizerArtifactTests` сохраняются; добавлять их broad run не требуется, если serialization не изменена и два назначенных serialization controls зелёные. Свежие падения сейчас не измерялись. Если A падает — baseline defect с точным diagnostic; B-only mismatch — чинить multiplicity/order/mapping в назначенном Train, не loader limits, encoding format, HNSW k или assertion. Нет удаления, Skip и замены expected текущим actual.

## 6. Путь будущего исполнения и E2E

A и B последовательно используют единственный `C:/Projects/llm_mvp_core` на ветке `codex/dpo-implementation`; output `artifacts/fbe/FIX-BW/attempt-0001/`, основные receipts в `learning/A/code-doc`, `learning/B/code-doc`, `learning/A/en-ru`, `learning/B/en-ru`. Helper/runner и порядок двух стадий определяет [03 §9](03-test-and-execution-contract.md#native-ab-transfer), вне архивного проекта35. Worktree, sibling/copy, checkout/reset и переключение ветки запрещены. Ранее запланированный `Tests.Support/BpeNoMatchCopy01` FIX-BPE не заменяется и не запускается здесь.

Сначала на прежнем native алгоритме фиксируются одинаковые tests/helpers/fixtures и runner; их commit записывается как `$baselineCommit`. Первая команда не принимает ещё не созданный candidate:

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-BW -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit
~~~

Baseline выполняет existing tests → шесть contract cases A → learning A/code-doc → learning A/en-ru. После terminal A receipts человек или назначенный исполнитель в том же корне вносит только weighted-chunks product diff, проводит review и фиксирует `$candidateCommit`; test/helper/fixture bytes прежние. Затем:

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-BW -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit -CandidateCommit $candidateCommit
~~~

Candidate выполняет existing tests → шесть contract cases B → learning B/code-doc → learning B/en-ru и сравнивает готовые receipts без повторного A. Каждая стадия выполняет `Rebuild` назначенного Tests проекта до `--no-build`; native Tokenizer assembly Location/SHA-256/MVID и source SHA записываются отдельно по03§9. Отсутствие завершённого A блокирует Candidate без нового Train; старую DLL нельзя принять за B.

SHA получены common-harness/candidate этапом03. Все процессы и26Train из§4–5 сохранены; изменено только размещение A перед product edit, B после. Существующий attempt не удаляется и не заменяется automatic retry/cleanup. Learning deadline5минут на основной process, максимум4; штатный wait на одном handle до terminal/deadline, без окончания хода вместо ожидания. Нет новых запусков исторического `run-pair`/контроллера ради получения summary.

[FIX-E2E §9](20-prompt-domain-e2e.md#resource-transfer-e2e) сохраняет learned routing и registered native receiver. Tokenizer memory не становится classifier домена. Совместно выбранная интеграция добавляет только идентичность tokenizer/native receiver и отсутствие чужого Train к существующим requests, 0 дополнительных training updates/тем; основной BPE A/B не повторяется внутри E2E.

## 7. Промпт исполнителю

~~~text
Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/31-bpe-weighted-chunks.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.

Выполни только отдельно назначенный FIX-BW. Текущая стадия без назначения — документация.
Модель текущая default без override; report.md/report-for-user.md по-русски.
Прочитай AGENTS/CLAUDE, 02/03, эту карточку; docs/skills:
architect-behavior-classifier-SKILL.md, phase-planning-SKILL.md, plan-review-SKILL.md,
code-examination-SKILL.md, test-integrity-SKILL.md, bug-to-tests-SKILL.md,
prompt-patch-authoring-SKILL.md, human-communication-GLOSSARY.md.
WRONG: убрать дубликаты без веса, сортировать пары, слить темы, изменить RegexSplitter,
подмешать FIX-BPE в одно плечо, снова запустить consumed BW01 из-за отсутствия summary.
CORRECT: native Train/CountPairs/MergeAll, ordinal weighted originals, first-occurrence ties,
две точные corpus fixtures, learned-state parity, reused Save/Load и конечный бюджет26Train.
Один canonical root/branch: Stage Baseline сохраняет обе A-панели до product edit;
после review/commit Stage Candidate выполняет только B и читает готовые A receipts.
Каждая стадия Rebuild и native DLL SHA/MVID; no worktree/checkout/reset/branch switch.
Любой measured useful gain — успех по метрике/профилю, рост accuracy необязателен;
покажи все regressions и не называй compression точностью перевода или routing.
Нулевые новые merges/нет приобретённого состояния не выдавай за successful learning.
Один process/handle, wait в том же ходе; никакого polling loop, повторного launch или autoattempt.
STOP/NEXT сразу прекращает прежнюю задачу: нельзя заканчивать её отчёт/tests/commit/cleanup.
NEXT — точный ID из01. Старые файлы, эксперименты и receipts сохраняются.
~~~

Отчёт03 содержит exact corpus и loaded assembly hashes, количество fits/merges, before→after token IDs/roundtrip, canonical restore, все resource deltas и результаты настоящего Knn consumer. В этой документной сессии tests, fits и код продукта не изменялись/не запускались.

