# FIX-JO — повторно использовать настройки JSON внутри настоящего обучения переводу

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски TRANSLATION:** Сверить builder→learned memory→обычный tokenizer/executor/verifier. Истинный negative и marker-only, exact topic/direction и нормализованные RuleTags не смешиваются; отрицательный смысловой контраст и сохранённый правильный перевод оцениваются раздельно, без подмены gold.

**Ожидания тестов:** собственные данные/positive/negative этой карточки и названный ordinary caller имеют приоритет над примерами120. Пример другого семейства не добавляет fixture, Train или новый API. Полный набор outer/nested Train/BPE/Grammar, readouts, restore и этапов остаётся в существующем бюджете. Структурный guard сTrain0 не объявляется новым обучением.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Для ресурсного переноса сравнивать actual native операции на том же обученном состоянии и в одинаковом окне. Managed allocations, peak и CPU различаются; прогрев/дрейф показываются по блокам. P08 продемонстрировал полезный gold-mass эффект при allocations+155,65%: наличие quality gain и кода в продукте не отменяет HOLD по памяти. Константа счётчика не является наблюдением.

Для FIX-JO исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Стоимость относится к точно названному окну. Контрпример P09: разность allocations четырёх arm windows, делённая на4, включает создание входов/cache/receipts и не является постоянной стоимостью одного Forward. Для этой оптимизации указать размер работы, warmup, что внутри/снаружи счётчиков и одинаковую обвязку A/B; не закреплять точные bytes по одному такому среднему. Проверять существующие tests и дополнять их learned fixture, не объявлять отсутствующими по одному отчёту.

Применимость: FIX-JO. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Полный ресурсный бюджет обязан включать обязательные диагностические вызовы и неудачные попытки, с отдельными единицами model/layer и optimizer-call/effective-update. Контрпример P11: 40 model Forward основной части +19 контрольных =59; 8 optimizer calls дают лишь4 изменения весов. Oracle/FD, заявленный обязательным, должен влиять на принятие результата, а не только выводиться в JSON. Равенство старому алгоритму подтверждать независимой проверкой, не комментарием в исходнике.

Применимость: FIX-JO. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-JO

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** TRANSLATION по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**K — владение допущенным состоянием и восстановление.** Показать, какой объект хранит допущенные параметры, примеры, конфигурацию или ключ кэша; интерфейс IReadOnlyList сам по себе не доказывает неизменность. Контроль после создания/допуска: изменение исходного caller-owned массива или внешнего представления не меняет захваченную запись незаметно. Законное обновление/смена model identity вызывает предусмотренное обновление или инвалидирование кэша, а не запрет обучения. Если reader сохраняется/восстанавливается, сравнить поведение обычного consumer по сохранённым данным и настройкам: reconstruction с default не является эквивалентным restore opt-in режима. Ошибка из KDP: catalogue был проверен, затем изменён через возвращённый массив; schema не содержала catalogue. Для этой карточки проверять её собственный carrier, не добавлять чужой KNN-механизм ради теста. Если изменяемого carrier/сохранения нет, отметить это явно в evidence и не выдумывать лишний runtime.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

ID: **FIX-JO**. Основание: **JSON-OPTIONS-REUSE-01**, историческая карточка опыта №42, результат `experiments-v2/RP01-MICRO/JO1/a001/result/`. Номер этого плана 46 не меняет номер старого опыта. Сверка исходников и evidence — commit [`ebaf7760a36d43172bd88317ff469edfd09a615b`](https://github.com/lubochka/llm_mvp_core/commit/ebaf7760a36d43172bd88317ff469edfd09a615b). Статус: **кандидат переноса измеренного улучшения; текущая работа документная, native исправление и новые тесты ещё не выполнены**. Приоритет внутри своей очереди высокий: полезный расход обучения уменьшился, содержимое усвоенной памяти и ответы сохранены, механизм имеет прямое объяснение. Порядок запуска и следующий ID задаёт [реестр](01-registry.md).

## 1. Положительное основание и границы вывода

В исходном `ExecutorUnit.TrainOnExample` каждый урок создаёт новые `JsonSerializerOptions` и `JsonStringEnumConverter`, затем разбирает ими `ExecutorInput` и `ExecutorOutput`. В опыте B ровно эта настройка стала общей для вызовов того же метода. Данные, порядок восьми уроков, native логика записи KNN/точных/фразовых соответствий и consumer не менялись.

Первичные источники: [report.md](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/JO1/a001/result/report.md), [A/training.json](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/JO1/a001/result/A/training.json), [B/training.json](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/JO1/a001/result/B/training.json), [score-summary.json](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/JO1/a001/result/score-summary.json), [one-factor.diff](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/JO1/a001/one-factor.diff), [manifest.json](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/JO1/a001/manifest.json).

| Назначенная метрика | A | B | Что доказано этим сравнением |
|---|---:|---:|---|
| Выделено managed bytes за один настоящий Train, текущий поток | 873 496 | **334 336** | **−539 160 Б, −61,7%** в этом парном прогоне |
| Выделено managed bytes процессом в том же окне | 873 496 | **334 336** | Подтверждающий счётчик; не размер удержанной кучи |
| Обычные правильные ответы до→после | 0/24→19/24 | 0/24→19/24 | Оба плеча реально приобрели ненулевой полезный результат |
| После обучения: отвечено / ошибочно принято / честные краевые отказы | 22 / 3 / 2 | 22 / 3 / 2 | Качество и известные ограничения сохранены |
| Exact / phrase / KNN entries до→после | 0→8 / 0→16 / 0→24 | То же | Изменена именно память исполнителя EN→RU |
| Wall time одного Train | 142,5145 мс | 37,2915 мс | Наблюдение; причинность ускорения не установлена |
| CPU одного Train | 93,75 мс | 46,875 мс | Наблюдение; причинность ускорения не установлена |
| GC по поколениям 0/1/2 | 0/0/0 | 0/0/0 | Изменений нет |
| Peak working set за жизнь процесса | 55 914 496 Б | 57 618 432 Б | **Ухудшение +1 703 936 Б**, показывать рядом |

Исторический расход — **два отдельных холодных процесса, 2 Train/16 предъявлений, 96 ordinary Flow calls, 0 neural updates, 0 внешних моделей, 0 cold-load, без повторов**. Порядок A→B фиксирован; это не статистическая серия и не доказательство универсального процента экономии. Process lifetime allocation `61 787 120→61 272 144 Б` не подменяет назначенное окно Train. Общая RAM компьютера, GPU и стоимость модели-исполнителя здесь не измерялись.

Состояния и ответы проверяются по первичным данным, отдельно от headline отчёта. [A/state-receipts](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/JO1/a001/result/A/state-receipts.json) и [B/state-receipts](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/JO1/a001/result/B/state-receipts.json) дают одинаковый canonical initial/before `4eaf4c3c37dc7576f96f9aedb5dd902e5883984504519186a394ed7b2cfc0e42` и trained/after `a6faf7013d08051f0268555c5ecd38a00203c1a2e0c5ec4047415a900d423f11`. Полные [A/trained.checkpoint.json](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/JO1/a001/result/A/trained.checkpoint.json) и [B/trained.checkpoint.json](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/JO1/a001/result/B/trained.checkpoint.json) независимо сверены структурно: после исключения только четырёх видов служебных полей из§5 все поля равны. Размеры исходных файлов 8932/8930 Б различаются; это **равенство канонического состояния, не равенство исходных bytes**.

По [A/raw-outputs](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/JO1/a001/result/A/raw-outputs.json) и [B/raw-outputs](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/JO1/a001/result/B/raw-outputs.json) независимо сверены все 48 before/after пары по `success,errorCode,actual,verdict,selectedFlow,adapterId,metrics,warnings`: различий нет. Агрегированный `comparison.json` через использованный GitHub read возвращал LFS pointer, поэтому его полный payload не выдаётся за независимо прочитанный. Метки `exactOptimizationSupported=true` и `SUPPORTED_IN_THIS_PAIRED_RUN` опубликованы в отчёте; raw проверки выше дают отдельное основание для bounded вывода.

**`LearningBatchResult.AccuracyBefore/AccuracyAfter=0` и `RewardMargin=0` не являются оценкой этой панели.** В текущем `TranslationMachine.MeasureUnitCoverage` обязательный список содержит tokenizer, executor-ru_en и verifier; executor-only EN→RU batch не попадает в этот список ([source:674–684](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationMachine.cs#L674)). Для этого опыта качество установлено отдельным scorer над 24 настоящими ответами. Нельзя написать «обучение 0→0» по этим служебным полям или менять их ради красивого отчёта.

Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен. Здесь полезная ось — allocations внутри обучения, при сохранённом ненулевом учебном результате. Все ухудшения и ошибки ответа остаются рядом. Это основание для кандидата, решение о полном внедрении остаётся за пользователем.

## 2. Точный native ремонт и участие в процессе обучения

Единственный product file — **`27.TranslationDomain/TranslationTrainedUnits.cs`**, класс `ExecutorUnit`, метод [TrainOnExample:404–456](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationTrainedUnits.cs#L404). Добавить внутри класса следующий private holder и заменить только создание `opts` на его получение, после прежнего `if (example.HeldOut) return;`:

~~~csharp
private static class TrainingJsonPolicy
{
    internal static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    static TrainingJsonPolicy() { }
}

// В ExecutorUnit.TrainOnExample, после HeldOut guard:
var opts = TrainingJsonPolicy.Options;
~~~

Сохранить текущие defaults `JsonSerializerOptions`, тип converter, порядок и аргументы двух `Deserialize`, null/held-out guards и все последующие записи. Не применять holder в `SemanticTokenizerUnit.TrainOnExample`, `QualityVerifierUnit.TrainOnExample`, `ExecutorUnit.MeasureAccuracy`, checkpoint serializer или reader. Не вводить глобальную JSON policy, source generation, новые resolver/converter options либо предварительный `MakeReadOnly`: это другой фактор. Назначенный explicit static constructor обеспечивает ленивую первую инициализацию по обращению к этому типу; holder не читают из setup, constructor машины, before readout, других методов и тестового warmup.

| Участок native пути | Конкретный код | Почему он входит в проверку |
|---|---|---|
| Доставка учебного batch | [TranslationMachine.TrainStructuredBatchAsync:89–145](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationMachine.cs#L89) | Реальный цикл выбирает trainer по `UnitId`, вызывает `TrainOnExample`, записывает identities/history/version |
| Чтение урока | `ExecutorUnit.TrainOnExample:408–410` | Здесь создаётся лишняя policy и разбираются именно `InputJson`/`ChosenOutputJson`; оптимизация участвует в каждой записи обучения |
| Изменение памяти | `TrainOnExample:413–455` | Exact source map, три KNN signatures, word/surface/phrase maps; их значения должны остаться прежними |
| Потребитель результата | `GetOrchestratorAsync().RunAsync → InlineTranslationOrchestrator → TranslationRoutingFlow.ExecuteAsync → TrainedTranslationAdapter → tokenizer/executor/verifier` | Обычный пользовательский запрос реально использует усвоенную память; оценка не ограничена JSON parse benchmark |
| Сохранение состояния | [ExportCheckpoint:261–300](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationMachine.cs#L261), [SaveCheckpointAsync:552–606](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationMachine.cs#L552), [LoadCheckpointAsync:611–650](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationMachine.cs#L611) | Сохраняется реальная обученная память для последующего read-only reuse; placeholder ExportAsync/ImportAsync не подходят |

Обучаемый механизм — память `executor-en_ru`, тема перевода EN→RU, technical. Это не LoRA, градиентный DPO или новое semantic authority. Предлагаемое исправление — внутренняя реализация существующего Unit, без нового Unit/Flow/public API. Policy не содержит учебных данных и не экспортируется в checkpoint, поэтому миграция состояния и переобучение донора не требуются. При откате только этой product правки сохранённые модели остаются читаемыми.

## 3. Математическое объяснение и область применимости

Пусть `n` — число непустых принятых executor-уроков. В каждом разбираются два фиксированных DTO типа. Исходный код создаёт `n` options/converter instances; B создаёт один holder при первом обращении в процессе. Если `C` — стоимость создания policy и подготовки её metadata для этих DTO, а `P_i` и `W_i` — разбор содержимого и запись памяти урока, то модель затрат имеет вид:

\[
A(n)\approx nC+\sum_{i=1}^{n}(P_i+W_i),\qquad
B(n)\approx C+\sum_{i=1}^{n}(P_i+W_i).
\]

Это объяснение по коду, не результат нового профилирования. Для 8 уроков число конструирований самой options policy по коду уменьшается 8→1; стоимость чтения JSON и усвоения данных остаётся. Подготовка одной и той же metadata амортизируется: её повторная часть с ростом `n` перестаёт расти на каждый урок. Из формулы не следует ровно 61,7% для другого batch, размера строк, версии runtime или уже прогретого процесса. При одном уроке выигрыш на числе policy может отсутствовать.

[Официальная документация System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/configure-options) рекомендует reuse одинаково настроенной `JsonSerializerOptions`: metadata cache принадлежит instance, повторное использование безопасно для этой cache, после первого serialization/deserialization options становится неизменяемой. Копирование options не копирует её metadata cache. [Документация C# static constructors](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-constructors) описывает однократную синхронизированную инициализацию типа и отсутствие `BeforeFieldInit` при явном static constructor. Поэтому расходы первого создания holder включаются в первый измеряемый Train, а не прячутся в setup.

`static readonly` запрещает переназначить ссылку, но сам по себе не делает объект глубоко неизменяемым. Здесь настройки полностью задаются до публикации private holder, после этого код их не мутирует; первый Deserialize замораживает policy. Новый доступ к ней разрешён только внутри указанного Train. **Это не доказательство потокобезопасности обучения:** словари Unit и KNN остаются изменяемыми, конкурентные Train в этой карточке не назначены. Возможное совместное использование одинаковой policy разными executor directions не доказывает улучшение качества или стоимости иных доменов. Измеренный профиль — только EN→RU technical.

## 4. Данные: сохранить исходный P125 и не дублировать план 19

[TEST-TMEM /19](19-translation-memory-coverage.md) уже назначает acquisition, target-permutation и cold restore для памяти перевода. FIX-JO добавляет **новый product factor и измерение расходов writer**, которых в 19 нет. Полный TEST-TMEM, его C-arm,24 cold calls и исторический P25 gate здесь не запускаются. Его данные и scorer используются совместно, вместо второй трактовки тех же 24 примеров.

Общий fixture root остаётся **`Tests/Fixtures/ExperimentFixes/v1/TranslationMemory/`**; будущий scorer — **`Tests/ExperimentFixes/TranslationMemoryPanelScorer.cs`** из 19. Если эти запланированные файлы ещё не реализованы, создать один раз нужные этому переносу файлы в общем harness commit, без автоматического исполнения 19. Отдельный JO passport — **`Tests/Fixtures/ExperimentFixes/v1/TrainingJsonOptions01/manifest.json`**. Он ссылается на общие файлы, не изменяет их содержимое и не переписывает provenance старого опыта.

Источники таблицы — каталог `docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/` в source commit `66947785d1318f7cbbc88118f3156cdc75001e16`; fingerprints зафиксированы JO manifest. План 19 ссылается на эти же данные в более раннем commit `2297566a5c07b564031337c2b5d4187ca10ca729`. Проверять bytes/blob, а не предполагать тождество по имени.

| Общий файл | Git blob источника | SHA256 исходных bytes |
|---|---|---|
| `training-batch-B.json` | `0c2a3d9a0da31b7277c0d215fb79ddebda49aa6c` | `2bf441780a1c97af03c64541eaea46cd8882764f88e2e4a143a2b4ed9b7a4e44` |
| `eval-inputs.json` | `585bf30cad84b57b2e1f3fe8c00f171f2348c313` | `0a44c8ecdb5fb2a1ad8d83fbe4da21dc5d2f54bb1ef270f2586fc96b5af9f880` |
| `panel-gold.json` | `af73d89cfd5b42f954380437bc5596b46dae9498` | `cc6b122dc8aa42fad9e30cd90a277dbcfb48926213e028a326d55e1648a36614` |
| Исходный `score.cjs` для проверки переноса scorer | `e8b20d471667da397e8d5c59ac63b4e7f947540b` | `9d091163a9d528a25355ed6e260b669ec9ab6719d8c1a81ca999eaf7a3c2d1a6` |

TRAIN — ровно 8 executor-only примеров, `BatchId=P125-enru-memory-01-B`, `DomainTag=TranslationDomain`, `UnitId=executor-en_ru`, `SourceId=simulation`, все `HeldOut=false`, исходные signatures и 16 `PhraseMappings` в исходном порядке. `BuildGeneralBatch` нельзя использовать взамен: он создал бы уроки tokenizer/verifier и изменил изучаемый механизм. Запрещены новые пары, дополнение отрицательных примеров, смена направления/темы и исправление исходных targets в этом факторе.

| Группа EXAM | Фиксированный состав | Исторический результат после обучения |
|---|---|---|
| `x01–x08` | validate the file; delete the cache; copy the report; move the config; upload the image; download the package; save the draft; archive the log |8/8; это воспроизведение исходных уроков |
| `n01–n03` | validate a file; delete a cache; copy a report |3/3; близкие варианты тех же семейств |
| `ev01–ev08` | delete the file; validate the cache; move the report; copy the config; download the image; upload the package; archive the draft; save the log |8/8; комбинации знакомых частей |
| `cc01–cc05` | do not delete the file; delete two files; validate the file and the cache; did you delete the file; delete the database |0/5 верных переводов; честные отказы cc02/cc05, ошибочно приняты cc01/cc03/cc04 |

Смешения предметных тем нет: одна тема перевода, несколько классов проверки. Это маленькая synthetic панель памяти, не независимые языковые семейства и не тест всех переводов. Неправильные ответы cc01/03/04 не превращаются в желаемый gold ради parity. Quality report отдельно отмечает потерю отрицания, второго объекта/связки, вопросительности.

Scorer переносит ровно `normalize/assess` из указанного `score.cjs`: lowercase, ё→е, только исходное обрезание краевой пунктуации и сведение whitespace; затем frozen accepted phrases и action/object/polarity/quantity/second-object/question predicates. `answered` требует native success, verdict `TRANSLATED` и отсутствие bootstrap/fallback. До переноса scorer проверить его на уже сохранённых raw/scores, **0 новых Train/Flow**. Gold читает только assessor после завершения всех 96 native outputs A/B; learner и передающий ему request helper gold не открывают. Списки плеч толькоA/B — отсутствиеC не меняет rubric.

## 5. Один native Fact на плечо: настоящее обучение и один сохранённый результат

Новый файл — **`Tests/ExperimentFixes/TrainingJsonOptionsTransferTests.cs`**, namespace `LlmFromScratch.Tests.ExperimentFixes`, класс `TrainingJsonOptionsTransferTests`, ровно один Fact:

**`OneEightLessonBatch_PreservesLearningAndMeasuresWriterCost`**.

Общий test-only reader/receipt helper — **`Tests/ExperimentFixes/TrainingJsonOptionsFixture.cs`**. Это новые запланированные файлы, не существующие API. Использовать обычный native ProjectReference на project27 через Tests; никакого `Compile Include` исходников 27, копированного learner или experimental DLL. До model call проверить `typeof(TranslationMachine).Assembly` и `typeof(ExecutorUnit).Assembly`: реальные native AssemblyName/Location/SHA/MVID, имя — из evaluation native csproj. Загрузка JO source-copy host или прежних A/B DLL считается ошибкой происхождения, не valid native run.

Каждый A/B Fact выполняется один раз в отдельном свежем OS test process, без других test methods в этом процессе. Последовательность внутри Fact:

1. Проверить fixture hashes, exact8rows/24cases и stage ownership; загрузить TRAIN и EXAM без gold. Создать одну свежую `TranslationMachine`, сохранить `state/initial.checkpoint.json` и полный receipt. Готовое историческое состояние 19/24 **не подменяет этот старт**: здесь измеряется стоимость writer, которую restore пропустил бы.
2. Выполнить 24 ordinary before calls через `GetOrchestratorAsync().RunAsync` с `ScenarioId=TranslationRoutingFlow.ScenarioId`, metadata `source_language=en`, `target_language=ru`, `topic_domain=technical`; сохранить все поля ответов. Затем `state/before.checkpoint.json`. Никакого Train для проверки «готовности»; expected before —0/24 полезных переводов.
3. В одном узком окне ровно один `TrainStructuredBatchAsync(training-batch-B)` и 8 предъявлений. Портировать измерение из [JO host Program:54–86](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/JSON-OPTIONS-REUSE-01/host/Program.cs#L54): до/после `GC.GetAllocatedBytesForCurrentThread`, `GC.GetTotalAllocatedBytes(true)`, `Stopwatch.GetTimestamp`, `Process.TotalProcessorTime`, `GC.CollectionCount(0..2)`, managed thread ID. JSON file IO, DTO setup, snapshots, output writing, reflection и report — за пределами окна. Не прогревать Train/holder, не добавлять forced GC и повторы.
4. Текущий native метод возвращает уже завершённый Task. Проверить `pending.IsCompleted` до `GetAwaiter().GetResult`; одинаковый managed thread обязателен для thread allocation сравнения. Если контракт стал async, запись показать как `measurement_contract_changed`, сохранить consumed calls; нельзя блокирующим ожиданием на другом потоке или новым fit «починить» цифры. Process allocation в testhost может включать стороннюю активность runtime; primary attribution — защищённое thread window, process/CPU/wall — отдельные поля.
5. Сохранить `training.json` со всеми raw counters и результатом writer: success, rows8, promotedUnitIds ровно `[executor-en_ru]`; затем `state/trained.checkpoint.json`. В B только теперь разрешена reflection-проверка private `TrainingJsonPolicy.Options`: поле существует, type не имеет `BeforeFieldInit`, options уже `IsReadOnly`; не запускать дополнительную сериализацию или Train ради этой проверки. A не требует ещё отсутствующий holder. Это contract assertion сохранности policy, не самостоятельное доказательство обучения.
6. На **том же** обученном instance выполнить 24 ordinary after calls, затем `state/after.checkpoint.json`. Assertions writer state, чужих owners и read-only inference используют эти четыре snapshots и 48 outputs; повторные checkpoints/queries/fit ради каждого assertion не нужны. Сохранить полный `receipt.json`, включая фактические вызовы, native identity и завершённость.
7. A Fact заканчивает сохранением `A_RAW_COMPLETE_PAIR_QUALITY_PENDING`: проверены native writer и изменение памяти, но ещё не объявлена точность. B Fact после сохранения своих 48 outputs читает completed A и только теперь передаёт все 96 outputs и frozen gold чистому `TranslationMemoryPanelScorer`. Этот вызов выполняет assertions приобретения 0→19/24 в каждой руке, semantic/state parity и пишет `pair-quality.json`; новых Train/Flow нет. Shared helper компилируется одинаково A/B; условие stage управляет только доступностью законченной пары, не product algorithm. Финальный Compare читает эти же raw/оценки и проверяет completeness, cost deltas и статусы; неудачная учебная проверка даёт nonzero/failed stage, а не зелёный summary. Если B не состоялся, качество пары остаётся `not_evaluated`, цифры из истории не подставляются.

Одна попытка содержит два независимых fresh writer acquisition A/B, потому что изменяется цена записи. **После этих фиксированных двух fit все новые lifecycle/readout/report проверки используют сохранённые after states и outputs, без повторного обучения.** Нет автоматического donor builder, C-arm, второго 24-query reload leg или вызова всего TEST-TMEM. Отсутствующий fixture — конкретный `missing_or_incompatible_fixture`, а не разрешение придумать другой batch.

### Четыре JSON состояния и точная канонизация

В каждом плече сохранить `initial`, `before`, `trained`, `after` native checkpoint. Raw bytes/SHA/размер сохраняются независимо от canonical сравнения. Из canonical исключить **только** `ExportedAtUtc` и `TrainingHistory[].CpuMs`, `.PeakRamMb`, `.ElapsedMs` — в disk schema это `exported_at_utc`, `training_history[].cpu_ms/peak_ram_mb/elapsed_ms`. Портировать установленное правило [JO Snapshot:145–173](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/JSON-OPTIONS-REUSE-01/host/Program.cs#L145): timestamp→UnixEpoch и эти cost поля→0 в отдельной canonical копии. Оригинальный snapshot не переписывается.

Нельзя исключать version, accuracy telemetry, flags, batch IDs, promoted units, identities, KNN entries/order, direction, token/word/surface/phrase maps, verifier, tokenizer или Tatoeba state. Сравнение устанавливает `A.initial=B.initial`, `A.trained=B.trained`, внутри каждого плеча `initial=before`, `trained=after`, но `before.memory!=trained.memory`. Чужие owners — semantic tokenizer, quality verifier, все executor directions кроме EN_RU — полное равенство до/после. Ориентир состояния: exact0→8, phrases0→16, KNN0→24, word/surface0→0, identities0→8, history0→1, version0.1.0→0.1.1. Это assertions конкретного fixture, не глобальная схема всех моделей.

Ordinary output equality сопоставляет 48 пары по ID/phase и полям `success,errorCode,actual,verdict,selectedFlow,adapterId,metrics,warnings`. Arm, phase placement, request IDs и elapsed/allocations — отдельный operational слой, не часть семантического equality; confidence внутри metrics не исключать. Невыданные native selectedTrainingIds честно сохранить как `not_exposed`, не выводить provenance соседа из ожидаемого gold. В итогах показать отдельно before/after A/B, ошибки/отказы и state parity. Исторический ожидаемый replay —0→19/24; если текущий native A его не воспроизводит, зафиксировать конкретный drift, не заменять gold, не увеличивать lessons и не объявлять A пригодным по служебному accuracy. Нулевая точность после Train не является учебным результатом.

### Где хранить state для последующего reuse

Корень результата — **`artifacts/fbe/FIX-JO/attempt-0001/`**. Назначенные файлы основной пары: `learning/A/` и `learning/B/`, внутри `training.json`, `raw-outputs.json`, `state/*.checkpoint.json`, `state-receipts.json`, `receipt.json`, `status.json`, `provenance.json`. Готовый native donor после принятого переноса — **`learning/B/state/after.checkpoint.json`**; его паспорт — `learning/B/provenance.json`, digest и совместимость перечисляются также в `comparison.json`.

Паспорт содержит source baseline/candidate SHA, actual native DLL hashes, fixture/source blobs, TRAIN/EXAM identities, направление EN_RU, topic technical, writer=ExecutorUnit, exact canonical exclusions, state schema/version, raw/canonical SHA, observed19/24 и три known wrong accepts, all counters и parent fixture. Optimizer/neural weights/RNG schedule здесь `not_applicable`, не выдуманная модель. Policy state не переносится вместе с моделью. После завершения пары checkpoint и receipts доступны read-only; последующий отдельно назначенный consumer может использовать `TranslationMachine.LoadCheckpointAsync` с 0 Train и собственным явным бюджетом чтений. Это место reuse указано заранее, исполнитель его не ищет.

19 может впоследствии читать этот паспорт/готовый donor в явно назначенной проверке совместимости, без нового acquisition. **Его существующий P25 historical cold gate и `input-checkpoint.json` не заменяются молча новым JO donor**. FIX-JO не выполняет этот cold gate второй раз и не расширяет 19budget; текущие assertions lifecycle уже читают готовые 4 snapshots. Другой direction/topic не получает этот checkpoint по совпавшему имени модели.

## 6. Существующие тесты: точный filter, бюджет и ремонт падений

Свежие падения не установлены: нынешняя работа не запускала Tests. Назначенный минимальный набор совместимости состоит ровно из двух существующих Facts в `Tests/TranslationDomainTests.cs`:

| Полное имя метода после `LlmFromScratch.Tests.TranslationDomainTests.` | Что сохранять | Расход одного исполнения |
|---|---|---|
| [`ExecutorUnit_EnRuWordMap_ComposesCyrillicTranslationWithoutBootstrap`](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/Tests/TranslationDomainTests.cs#L407) | Урок с enum `SemanticConcept.UNKNOWN` и WordMappings; `good morning friend→доброе утро друг`, без bootstrap |1 Train/1 lesson/1ordinary Flow |
| [`ExecutorUnit_EnRuPhraseMapping_WinsOverOrdinaryKnn`](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/Tests/TranslationDomainTests.cs#L911) | Phrase mapping для `red house is empty` выигрывает у train-only KNN примера про blue house; expected `красный дом пустой`, без `синий` |1 Train/1 lesson/1ordinary Flow |

Эти тесты оставляют свои маленькие, разные datasets, не смешивая их с P125. Они запускаются в отдельном compatibility process до основного Fact в каждом stage; их предварительные Train не прогревают static policy в свежем learning process. Общая цена existing checks A+B: **4 Train/4lesson/4 Flow**. Они не объявляются независимым экспериментальным улучшением и не дублируют полный lifecycle19.

При падении после переноса сначала сравнить тот же ID с baseline. Deserialize/enum/null regression исправлять восстановлением той же policy и converter в назначенном `TrainOnExample`, сохраняя negative control и expected text. Если phrase уступает unrelated KNN, исправить ошибочную интеграцию writer в пределах данного diff; не переименовать ожидаемый ответ и не поменять TRAIN. Изменение options не должно менять семантические expected значения, поэтому массовое обновление snapshots/19→actual запрещено. Ошибку provenance/hash/dataset связывать с конкретным loader перед Train; она не считается выигрышем модели. Падение вне выбранных методов фиксируется отдельно, без незапрошенного ремонта solution.

Сохранить остальные существующие checkpoint/schema/continued-training/source-copy/negative-KNN тесты; эта карточка их не удаляет, не помечает Skip/xfail и не запускает весь класс автоматически. Build prerequisite — только [02§10](02-learning-state-and-fixtures.md#native-build-prerequisite): устранение включённых в 35 архивных source copies одинаково A/B, если этот evaluated Compile конфликт действительно существует. Native project27 и test assertions остаются в сборке.

## 7. Канонический запуск: один checkout, четыре test processes, конечный бюджет

Исполнение наследует [03§9](03-test-and-execution-contract.md#native-ab-transfer) и добавляет точную ветвь **`PlanId=FIX-JO`** в будущие `Tests.Support/ExperimentTransfer/Run-NativeAB.ps1` и `Compare-NativeAB.ps1`. Эти helper/API **планируемые**, а не доступные сегодня команды. SourceRoot единственный — `C:/Projects/llm_mvp_core`, branch `codex/dpo-implementation`. Не создавать worktree, sibling checkout, source-copy host и не запускать повторно исторический JO `run.py`. Исторический опыт использовал 9 копий native исходников на `66947785...`; это evidence, а не новая product сборка.

Сначала подготовить общий harness/fixture/scorer commit A без product holder, зафиксировать `$baselineCommit`, выполнить Baseline до любых product edits. Затем в том же checkout вне runner внести ровно holder+opts line в `TranslationTrainedUnits.cs`, review/commit, зафиксировать `$candidateCommit`, выполнить Candidate. Runner не делает checkout/reset/restore/stash/branch switch. Он проверяет canonical root/branch/HEAD, чистоту назначенных tracked inputs, A ancestor B, равенство test/helper/fixture bytes и однокомпонентный разрешённый product diff. Никакой runtime ARM switch не выбирает реализацию: `XIIGEN_FBE_ARM` служит только provenance/выходам/назначенным assertions.

| Стадия | Отдельный процесс | Train / предъявления | Ordinary Flow |
|---|---|---:|---:|
| A existing | Exact filter из 2 existing Facts |2 /2 |2 |
| A learning | Только `OneEightLessonBatch_PreservesLearningAndMeasuresWriterCost` |1 /8 |48:24before+24after |
| B existing | Тот же exact filter |2 /2 |2 |
| B learning | Тот же один Fact, свежий process |1 /8 |48:24before+24after |
| Compare / report / scorer | Чтение завершённых raw, не learner |0 /0 |0 |
| **Общий максимум** | **4 test processes, 2 native project Rebuild, 2 discovery** | **6 /20** | **100** |

Основной transfer бюджет отдельно: **2 Train/16 lessons/96 ordinary outputs**. Остальные 4/4/4 — явно названная совместимость. Нет cold child, дополнительных restore queries, C-arm, neural updates, внешних teacher calls, warmup fit и скрытых repeats. Порядок: **A Rebuild→discovery3→existingA→learningA; product diff/review/commit; B Rebuild→discovery3→existingB→learningB→compare**. Одна discovery стадии должна найти ровно 2 existing+1 new, не ноль и не целый класс. После stage Rebuild все test invocations с `-c Release --no-build`; ОС PID learning отличается от compatibility и другого плеча. Read-only перенос scorer по сохранённым данным не запускает дополнительные tests/Flow.

Exact existing filter — объединение двух FQN в§6 через `|`. Exact learning filter:

~~~text
FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.TrainingJsonOptionsTransferTests.OneEightLessonBatch_PreservesLearningAndMeasuresWriterCost
~~~

Runner задаёт уже предусмотренные test-only `XIIGEN_FBE_SOURCE_ROOT`, `XIIGEN_FBE_OUTPUT`, `XIIGEN_FBE_ARM`, `XIIGEN_FBE_PAIR_ROOT`; learning output — `learning/A` или `learning/B`, compatibility — `existing/A` или `existing/B`. Каждая команда имеет отдельные TRX/stdout/terminal receipt и один handle. Общий `attempt-0001` claim/ownership создаётся до первой содержательной операции; существующая completed попытка читается, частичная сохраняется с фактическими counters. Каталог не удалять и suffix автоматически не увеличивать.

Форма будущих команд после реализации общего harness и назначения переноса:

~~~powershell
Set-Location C:/Projects/llm_mvp_core
$baselineCommit = (git rev-parse HEAD).Trim()
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-JO -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit
~~~

После завершённого A, единственного product diff и его commit:

~~~powershell
$candidateCommit = (git rev-parse HEAD).Trim()
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-JO -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit -CandidateCommit $candidateCommit
~~~

Внутренние test commands используют точно назначенный filter, `--logger "trx;LogFileName=selected.trx" --results-directory <свой stageOutput>`. Срок —5 минут на test process,15 минут на Rebuild; это watchdog, не разрешение добавлять уроки. Истечение срока фиксируется по реальному процессу, а не выводится из пустого log. Native assembly identity записывается до и после writer; старый DLL из кэша не принимается как B.

В finally сохранить terminal/failure receipt, уже сделанные Train/Flow и доступные snapshots/raw перед возвратом ошибки xUnit. Невыполненные шаги — `not_reached`, а не 0correct как измеренное «после». Если содержательный fit уже выполнен, ошибка formatter/scorer/ссылки исправляется чтением этих же raw, с новой явно обозначенной revision отчёта; повторный fit не назначается. После A-only или частичного B нельзя выводить экономию по отсутствующей стороне.

## 8. Критерии переноса, E2E и результаты для решения пользователя

Три вывода показываются отдельно. **Исполнение:** terminal stages, точный budget/native origin. **Валидность обучения:** реальные 8 уроков, same-thread measured writer, ненулевые правильные ordinary ответы после и полная state/readout связь. **Полезный эффект:** дельта выбранных cost metrics вместе со всеми качественными и ресурсными изменениями. Нулевой учебный результат после не заменяется ростом margin/количества записей.

Для именно семантически нейтрального FIX-JO ожидаются полное A/B canonical state/output equality и меньшие writer allocations. Процент 61,7 не становится CI threshold; сравнивать фактические native bytes. Если полезно улучшилась другая метрика, это успех по ней с её ограничениями, но такой результат отдельно отличается от подтверждения исходной allocation-гипотезы. Time/CPU при одной холодной точке по-прежнему observations без устойчивой causal speedup декларации. Известные 3 wrong accepts нельзя скрывать общим `parity=true`; они остаются ограничением готовности перевода. Новый semantic regression запрещает назвать этот фактор прозрачной оптимизацией, даже если RAM меньше.

Обычный domain consumer в основном Fact действительно выполняет translation Flow, но explicit scenario/direction **не доказывает learned prompt→domain routing**. [20 FIX-E2E](20-prompt-domain-e2e.md) уже задаёт собственный RU→EN general receiver и другой набор. FIX-JO не меняет его selector, данные, checkpoint или бюджет; по 20 добавлено 0 вызовов. [49 FIX-KR](49-router-learned-key-normalization.md) имеет отдельное положительное основание для reader aliases и также не соединяется автоматически с этим EN→RU receiver. Полный внешний маршрутизатор и этот технический переводчик могут соединяться только по конкретно назначенному receiver contract/данным, не по совпадению слова «перевод» в prompt. Здесь зафиксирован уже реально измеренный путь использования учебного результата и место готового after donor.

Итоговые будущие файлы в `artifacts/fbe/FIX-JO/attempt-0001/`: `comparison.json`, `report.md`, **`report-for-user.md` на русском**, `status.json`, ссылки на A/B raw и state passports. Порядок отчёта: что училось и где изменено → before/after0/24 → фактический результат каждого плеча → все 24 поимённые ответы/ошибки → raw/canonical state и неизменность owners → полезные cost deltas и ухудшения → scope/доказательность/незавершённые стадии → кандидат ремонта. `required-changes`/реестр связываются с **FIX-JO** и этим evidence, без объявления измеренным будущего native исполнения. Старые эксперименты, их результаты и карточки не удаляются.

## 9. Точный промпт будущему исполнителю

> Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/46-training-json-options-reuse.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.
>
> Выполни только отдельно назначенный перенос **FIX-JO** по этой карточке и 03§9. Модель исполнителя — фактическая default текущей сессии, без самовольной смены model/reasoning. Обучаемая система — native TranslationMachine memory EN→RU technical; нейронная конфигурация неприменима. Исторические данные заморожены; новые DPO/teacher вызовы не назначены. Роли CODEX выбора/проверки DPO сохраняют указанное пользователем `astra 6 ultracode`; это не разрешение создавать здесь новые пары или оценивать собственную генерацию.
>
> Сначала прочитай актуальные AGENTS.md/CLAUDE.md, эту карточку,02/03 и строку **FIX-JO** реестра. Skills относительно `docs/skills/`: `architect-behavior-classifier-SKILL.md`, `trainable-architecture-decision-SKILL.md`, `intent-to-plan-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `generated-code-review-SKILL.md`, `test-integrity-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-communication-GLOSSARY.md`, `human-readable-architecture-audit-SKILL.md`. Объяснение и два итоговых отчёта — по-русски; точные paths даны в§8. Не ищи другой host, модель, corpus, язык или место сохранения.
>
> WRONG: заменить измерение обучения parse-only benchmark, загрузить обученный 19/24 checkpoint вместо измеряемого writer, прогреть holder до окна, запустить целиком TEST-TMEM либо по одному fit на assertion, исправить verifier/reader вместе с opts, изменить gold ради зелёного теста. CORRECT: ровно 2 fresh native writer arms по 8 уроков,96 обычных outputs; после fit — reuse snapshots/raw;2 existing Facts отдельно добавляют 4 Train/4 урока/4 Flow. Один product factor — private TrainingJsonPolicy с explicit cctor и reuse только в ExecutorUnit.TrainOnExample.
>
> Любое измеренное улучшение полезной оси — успех этой оси и профиля, рост accuracy не обязателен; показывай все ухудшения рядом. Не подменяй фактическое обучение служебным accuracy0 или чистой инфраструктурой. Ремонт loader/receipt/test допускается только по конкретному пути, который мешает назначенному writer/readout, в пределах§6–7; не продолжай чужой кандидат и не расширяй бюджет.
>
> Один запуск — один исходный handle. При exec_command session_id жди `write_stdin` с пустыми chars; при `functions.exec` ответе `Script running with cell ID ...` — `functions.wait` с тем же cell_id. Если доступен иной инструмент, сначала прочитай его схему и используй штатный documented wait. Ожидание порциями≤60 секунд продолжается **в том же ходе** до terminal/deadline, с краткими сообщениями о ходе. Не объявляй сбой по отсутствию файла, не relaunch, не создавай polling loop/второй waiter/summary work ради ожидания и не заканчивай ход фразой «жду». Неподтверждённые цифры не публикуй.
>
> **STOP/NEXT имеют немедленный приоритет.** При прямом STOP прекрати действия; при NEXT перейди только к названному ID либо следующему ID текущего реестра. Запрещено продолжать прежнюю карточку, дописывать её отчёт, tests, commit или cleanup после такого указания; сообщи только уже подтверждённое состояние. Старые номера из цитат не выбирают следующий опыт. Существующие результаты не удаляй и не перезапускай ради завершения прежнего отчёта. В отсутствие STOP/NEXT доведи назначенный живой запуск до terminal тем же штатным ожиданием и запиши результат без повторного обучения.

