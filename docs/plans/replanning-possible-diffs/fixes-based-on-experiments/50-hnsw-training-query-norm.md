# FIX-HNQ — убрать повторное вычисление нормы запроса внутри обучения HNSW

<!-- transfer-entry-reload -->
**Вход исполнителя.** [R14](01-registry.md#work-fix-hnq). Для общего включения действует измеренное ограничение этой строки. Возврат к работе — по записанному условию для её домена. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

## Уточнение допуска по показателям — 3 ноября 2026

**FIX-HNQ: Не допускать общий режим; разделить профили.** Вошёл по тёплому окну. Противоположный результат основного замера нельзя скрывать и нельзя смешивать разные окна.

- Точность: Новые ответы в A и B 0/4 → 4/4.
- Полезный результат: Тёплое обучение 100,7398 → 83,0755 мс (−17,53%); CPU 109,375 → 78,125 мс.
- Цена и ограничения: Основной замер 3,1809 → 3,9071 мс (+22,83%); выделения +56 Б; вложенный тёплый Add медленнее.
- [Полное сравнение и статус кода](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#benefit-fix-hnq).

Этот вывод относится к названным ID, а не ко всем механизмам документа. Старый текст плана и опыта сохранён как доказательство, но его общий статус «к переносу / разрешён» не переопределяет это уточнение. Ближайшее действие — разбор уже сохранённых данных и конкретного регресса; новое обучение, повтор PRIMARY и переключение product default этим дополнением не назначаются. Для отдельного логического ремонта назвать нарушенный контракт и сохранить отрицательные контроли, а не переносить весь эксперимент под словом «bugfix».


<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски STATE:** Проверить полный typed learned payload и применимые RNG/tokenizer/index/maps, состояние сразу после вмешательства и отдельный fresh-load только по назначенному API. Положительный старый ответ сохраняется; внешняя мутация/чужая identity должна обнаруживаться. Count/hash сами не доказывают восстановление.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->


**Адресное усиление 29.10.2026:** [97, R31-QD](97-learned-knn-state-integrity-transfer-plan-2026-10-29.md) использует уже описанный здесь общий `TrainWithIndexFactory` для доступа к тому же реально обученному индексу. Вторую фабрику/обходной Search не создавать. Нормирование не разрешает сравнение векторов разных размерностей: короткий запрос обязан отвергаться до расстояния, затем тот же экземпляр сохраняет 12/12 обычных ответов. Это отдельное основание P18, не дополнительный замер ускорения HNQ.

<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Для ресурсного переноса сравнивать actual native операции на том же обученном состоянии и в одинаковом окне. Managed allocations, peak и CPU различаются; прогрев/дрейф показываются по блокам. P08 продемонстрировал полезный gold-mass эффект при allocations+155,65%: наличие quality gain и кода в продукте не отменяет HOLD по памяти. Константа счётчика не является наблюдением.

Для FIX-HNQ исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Стоимость относится к точно названному окну. Контрпример P09: разность allocations четырёх arm windows, делённая на4, включает создание входов/cache/receipts и не является постоянной стоимостью одного Forward. Для этой оптимизации указать размер работы, warmup, что внутри/снаружи счётчиков и одинаковую обвязку A/B; не закреплять точные bytes по одному такому среднему. Проверять существующие tests и дополнять их learned fixture, не объявлять отсутствующими по одному отчёту.

Применимость: FIX-HNQ. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Полный ресурсный бюджет обязан включать обязательные диагностические вызовы и неудачные попытки, с отдельными единицами model/layer и optimizer-call/effective-update. Контрпример P11: 40 model Forward основной части +19 контрольных =59; 8 optimizer calls дают лишь4 изменения весов. Oracle/FD, заявленный обязательным, должен влиять на принятие результата, а не только выводиться в JSON. Равенство старому алгоритму подтверждать независимой проверкой, не комментарием в исходнике.

Применимость: FIX-HNQ. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-HNQ

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** STATE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**K — владение допущенным состоянием и восстановление.** Показать, какой объект хранит допущенные параметры, примеры, конфигурацию или ключ кэша; интерфейс IReadOnlyList сам по себе не доказывает неизменность. Контроль после создания/допуска: изменение исходного caller-owned массива или внешнего представления не меняет захваченную запись незаметно. Законное обновление/смена model identity вызывает предусмотренное обновление или инвалидирование кэша, а не запрет обучения. Если reader сохраняется/восстанавливается, сравнить поведение обычного consumer по сохранённым данным и настройкам: reconstruction с default не является эквивалентным restore opt-in режима. Ошибка из KDP: catalogue был проверен, затем изменён через возвращённый массив; schema не содержала catalogue. Для этой карточки проверять её собственный carrier, не добавлять чужой KNN-механизм ради теста. Если изменяемого carrier/сохранения нет, отметить это явно в evidence и не выдумывать лишний runtime.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Ограничение ресурсного вывода, 27 октября 2026.** Main Train wall3.1809→3.9071 ms (+22.83%) — не CPU и не разрешённое исключение роста CPU. Поэтому warm-окно и сокращение norm calculations56→14 не квалифицируют общий перенос с отсутствием>5% регрессий. Full-profile rollout остаётся HOLD/CONDITIONAL; отдельный warm профиль может оцениваться только своими сопоставимыми metrics, с сохранением main результата. +56 allocated bytes записываются вместе с исходным знаменателем, не автоматически как допустимые≤5%. Существующие acquired/retained outputs, confidence bits и seeded-seam controls остаются; новый resource snapshot не даёт права пересоздать HNSW с другим RNG или подменить trained graph ради красивого времени.

**Усиление после R4:** реальные4старых+4новых урока и owner-input cases уже внесены. Плохая проверка — тот же query дважды и только конечный selected ID: она пропускает stale norm и score drift. Хорошая — меняет query и отдельно сохранённый vector, сравнивает score/confidence и сохранение старых плюс усвоение новых ответов. Cached norm живёт в одном обходе/запросе, не в глобальном mutable состоянии. Не заменять .745/.963 наблюдения ожиданием1.0 ради зелёного теста. Число вычислений56→14 — вывод об операциях, не измеренный CPU; reported allocations+56B показывать как ухудшение. Timing без контроля порядка не превращать в обещанный процент ускорения. Бюджет существующих двух learning stages не расширять. [67](67-transfer-repair-followup-audit-2026-10-27.md), [03§25](03-test-and-execution-contract.md#repair-followup-20261027).

**Уточнение исполнения, 27 октября 2026.** Код внесён в `a37d0b9f52a`. Helper cosine parity и несколько Search не доказывают путь нового урока через укладку и back-pruning. Использовать назначенную HNQ панель: обученное дробление/вектор→индекс, 4→8 уроков, прежние и новые полные ответы, graph/state parity. Норму вычислять один раз на конкретный traversal, не сохранять между вызовами или по адресу изменяемого массива. Старый cosine и ручные значения остаются отдельными oracle.

Общие примеры правильной и ошибочной проверки — [контракт03§24](03-test-and-execution-contract.md#transfer-audit-20261027). Это дополнение плана; новые проверки сейчас не запускались.

Дата: 22 октября 2026, UTC. ID: **FIX-HNQ**. Основание: **HNSW-QUERY-NORM-REUSE-01 / HNQ01/run01**. Статус: **ограниченный кандидат по сокращению арифметической работы обучения**. Эта публикация создаёт план; production, новые тесты и обучение не запускались. Проверенный код — core [ebaf7760a36d43172bd88317ff469edfd09a615b](https://github.com/lubochka/llm_mvp_core/commit/ebaf7760a36d43172bd88317ff469edfd09a615b). Следующий ID задаёт только [реестр 01](01-registry.md); наследуется [контракт 03](03-test-and-execution-contract.md#native-ab-transfer).

## 1. Почему кандидат допускается и что не доказано

Во время настоящего `KnnLogicalUnit.Train` каждый урок добавляется в HNSW. Три метода обхода сравнивают неизменный в пределах вызова запрос с несколькими векторами. Исходный cosine заново вычисляет сумму квадратов запроса при каждом сравнении; B считает её один раз на вызов метода обхода. Это работает внутри построения обученной памяти, не в постороннем benchmark.

| Ось исходного опыта | A | B | Граница вывода |
|---|---:|---:|---|
| Main: построения суммы q² | 56 | 14 | −42, −75% этого компонента; **выведено из измеренных calls и source**, не измерение CPU-инструкций |
| Main: элементы этих сумм | 3584 | 896 | −2688; не −75% всей работы learner |
| Main Train wall | 3,1809 ms | 3,9071 ms | **+0,7262 ms**, +22,83% |
| Main Train allocations | 590 816 B | 590 872 B | **+56 B** |
| Main вложенные Add wall / allocations | 0,6745 ms / 15 592 B | 0,8039 ms / 15 648 B | Обе оси хуже; время уже входит в Train |
| Warm Train wall / CPU | 100,7398 / 109,375 ms | 83,0755 / 78,125 ms | Наблюдаемый выигрыш первого fit; cold/JIT и фиксированный порядок A→B не позволяют приписать его фактору |
| Warm вложенные Add wall | 24,9038 ms | 26,3246 ms | Хуже, несмотря на лучший полный Train |
| Warm Train allocations | 370 984 B | 371 016 B | +32 B |
| Warm: построения q² / элементы | 14 / 896 | 8 / 512 | Это отдельная панель, её нельзя заменить main числами |
| Old exact / new exact, warm→main | 4/4→4/4; 0/4→4/4 | То же | Обучение действительно усваивает четыре новых ответа |
| Main near / composition / edge | 3/3; 0/8; 0/5 | То же | Обобщение на составные и краевые запросы не показано |

Первичка: [A/A.json](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/HNQ01/results/A/A.json), [B/B.json](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/HNQ01/results/B/B.json), [metrics.json, включая warm](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/HNQ01/results/metrics.json), [report](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/HNQ01/results/report.md), [исторический candidate.md](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/HNQ01/results/candidate.md). Исходный source pin — `fb9175cc752d41e1cb525e0609b7c96140940217`.

Историческое `NOT_SUPPORTED_FOR_MEASURED_AXIS` относится к main wall/allocations и сохраняется. Новый план признаёт отдельный полезный показатель — меньше повторной арифметики в действительном обучении — по правилу пользователя «любое измеренное улучшение полезной оси». Raw честно помечает `derivedIsArithmeticProxyNotCpuTime:true`: непосредственно измерены 56 cosine calls и 14 query-method calls, затем точный source связывает их с числом сумм. Это сильнее свободной гипотезы, но не доказательство скорости. Main CPU=0/0 означает предел разрешения счётчика, а не отсутствие работы. Первый Add, не затронутый фактором, также сдвинул время; это дополнительная причина осторожности, не доказательство причины всех колебаний.

Сохранённые индексы побитово одинаковы A/B: warm `8513b90d12d0c0fa509b81f5d47b6dbf878b1e7bc05fa8e81fb6e3cdc9adf73e`, main `f99a03825ff16f17754e3909f64a4b4085563890471659345aee5c101d8cdb5f`. По 24 ответа каждой панели совпадают вместе с confidence bits/evidence/warnings. +56 B согласуется с дополнительным captured полем в семи SelectNeighbours. Граф из восьми узлов не проверяет обратную подрезку связей и concurrency.

Новизна: FIX-ATS/RRD/RMS относятся к tensor kernels; FIX-HNQ меняет повторную норму dense query в `06.VectorSearch`. BPE merge objective, другие KNN distance functions и маршрутизация сюда не добавляются.

## 2. Native путь и единственный содержательный фактор

| Участок на проверенном SHA | Будущая дельта |
|---|---|
| [KnnLogicalUnit.Train:103–197](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L103-L197) | Настоящий learner: BPE Train → Vectorize → index.Add. Содержательная логика обучения сохраняется |
| [HnswIndex.Add:49–96](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/06.VectorSearch/HnswIndex.cs#L49-L96) | Уже вызывает GreedySearch, SearchLayer и SelectNeighbours, включая обратную подрезку; алгоритм добавления не переписывать |
| [GreedySearch:164–182, SearchLayer:190–223, SelectNeighbours:225–231](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/06.VectorSearch/HnswIndex.cs#L164-L231) | Один `QuerySquaredNorm(query)` в начале каждого вызова; передать результат во все пять cosine call sites |
| [Distances.CosineSimilarity](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/06.VectorSearch/Distances.cs#L10-L22) | Публичный метод оставить прежним; он независимый legacy oracle |
| [KnnLogicalUnit.ExecuteAsync:201–262](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L201-L262) | Реальный consumer обученных tokenizer/index/examples. Не копировать его в Tests |

Новые private helpers в HnswIndex: `QuerySquaredNorm(float[] query)` и `CosineWithQueryNorm(float[] query, float[] candidate, float querySquaredNorm)`. Первый суммирует `q[i]*q[i]` в float и прежнем порядке. Второй оставляет dot и candidate norm в float, затем **тот же** `MathF.Sqrt(na) * MathF.Sqrt(nb)` и `denom < 1e-9f ? 0f : dot / denom`. Не кэшировать sqrt вместо суммы, не менять precision/SIMD, ширины поиска, tie order или zero guard. Norm не сохраняется между вызовами или в checkpoint. Product factor B меняет только названные участки `06.VectorSearch/HnswIndex.cs`.

## 3. Явный общий RNG seam до A

Сейчас `HnswIndex._rng` — private static readonly `new Random()`; seeded native API нет. Source-copy заменял его на seed7331. **Переносить этот seed в production default нельзя.** Для native A/B назначен один общий seam, одинаковый до A и после B:

1. Сохранить публичный `HnswIndex(int m=16, int efConstruction=200, int efSearch=50)`. Он делегирует новому **internal** `HnswIndex(int m, int efConstruction, int efSearch, Random levelRandom)` с прежним static `_rng`. Новый readonly instance field `_levelRandom` используется только RandomLevel; переданный null отвергается. Старые callers и Load продолжают использовать тот же default RNG.
2. `06.VectorSearch/Properties/AssemblyInfo.cs` получает только `InternalsVisibleTo("LlmFromScratch.Tests")`. В `21.AgentInfrastructure/AgentInfrastructure.csproj` такой friend **уже существует**; его не дублировать.
3. Сохранить public `KnnLogicalUnit<TInput,TOutput>.Train` со всеми прежними параметрами/defaults. Общий текущий body перенести в единственный private `TrainCore(..., Func<HnswIndex> indexFactory)`. Public Train передаёт `static () => new HnswIndex(8,64,32)`. Новый internal `TrainWithIndexFactory` имеет те же восемь обязательных аргументов и девятый `Func<HnswIndex> indexFactory`; он вызывает этот же TrainCore. Ни copied learner, ни второй body не появляются. Новая factory проверяется на null до обучения; остальные guards и operation envelope сохраняются.
4. В learning process создаётся **один `new Random(7331)` на руку**. Factory создаёт индексы m=8, efConstruction=64, efSearch=32, передавая тот же Random сначала warm4, затем main8. Между панелями RNG **не сбрасывается**. Existing/contract tests запускаются отдельными процессами и не расходуют этот stream.

Для точных counters общий Hnsw seam также задаёт internal `Action<string,int>? TrainingTraceObserver`, по умолчанию null. События: `AddBegin`/`AddEnd` в try/finally Add; `QueryMethod` с длиной query в начале трёх методов; `Cosine` перед каждым из пяти сравнений. Общий private `MeasuredCosine` в A лишь сообщает событие и вызывает неизменный Distances.CosineSimilarity; B передаёт готовую norm в его расширенную private форму и затем в CosineWithQueryNorm. Test collector считает только внутри Add; observer не меняет candidates, RNG или выход алгоритма. До замера создаётся collector; запись per-Add receipt выполняется после снятия интервала. Это измерительная предпосылка A/B, не отдельное улучшение и не постоянная telemetry production. Будущие ресурсы включают одинаковые probes обеих рук; исторические проценты не становятся expected. A/B изолирует norm-factor поверх общего seam и не измеряет суммарную цену всех служебных изменений против исходного кода без seam. Это ограничение и дополнительные instance fields указываются в отчёте; бесплатная интеграция не обещается.

Общий harness commit затрагивает эти два native source files, один friend file, Tests/helpers/fixtures и условную build-предпосылку02. Candidate commit содержит **только норм-фактор HnswIndex**, а не seam заново. Старые public API, default RNG, Save/Load schema и tokenizer алгоритм сохраняются. Обе стадии используют native assemblies; arm metadata не выбирает алгоритм.

## 4. Математика и ограничения домена

Для неизменного q и кандидата v:

\[
a=\sum_i q_i^2,\quad b=\sum_i v_i^2,\quad c=\sum_iq_iv_i,\quad
\operatorname{cos}(q,v)=c/(\sqrt a\sqrt b).
\]

При N сравнений одного q сумма a не зависит от v. Перенос её вычисления до цикла сохраняет порядок слагаемых внутри a, b и c. В реализации остаётся прежний порог знаменателя; равенство ответов и float bits подтверждено на исходном runtime. На main trace C=56 cosine calls и Q=14 query-method calls: работа query norm меняется с C·64 на Q·64. Это −75% **компонента нормы запроса**, а не wall, FLOPS всего Train или расхода BPE.

Условие — q не меняется до окончания данного метода. Native Train передаёт свежие Vectorize arrays и синхронно завершает Add; concurrent mutation сохранённых массивов этим опытом не проверяется. После завершения Search пользователь вправе изменить query и вызвать Search снова: новая норма должна вычислиться заново. Для обратной подрезки query может быть `_vectors[nb]`; никакого предположения о его единичной норме не вводится. Это применимо к cosine dense-vector пути разных доменов, но измеренное обучение — только EN→RU технические указания. AST/order-aware distance, точность перевода на новые конструкции и E2E dispatch не следуют из тождества.

## 5. Точные данные, две минимальные подготовки и повторное чтение

Полного сохранённого KnnLogicalUnit в HNQ01 нет: index JSON не содержит BPE tokenizer и таблицу ответов. Поэтому здесь оправданы **две минимальные Train на руку**: warm на первых четырёх уроках, затем **новая Train на всех восьми**, не «добавить ещё четыре». Сохранённый graph не подаётся как будто он полный обученный Unit.

Единственный источник данных: [`docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/`](https://github.com/lubochka/llm_mvp_core/tree/ebaf7760a36d43172bd88317ff469edfd09a615b/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01), точные входы [manifest HNQ01](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/HNQ01/harness/manifest.json). Будущий fixture `Tests/Fixtures/ExperimentFixes/v1/HnswQueryNorm01/` содержит byte-exact копии и manifest с source pin, Git blob и SHA-256:

| Файл | Git blob |
|---|---|
| training-batch-B.json | `0c2a3d9a0da31b7277c0d215fb79ddebda49aa6c` |
| eval-inputs.json | `585bf30cad84b57b2e1f3fe8c00f171f2348c313` |
| panel-gold.json | `af73d89cfd5b42f954380437bc5596b46dae9498` |

| ID | TRAIN source | Target |
|---|---|---|
| tr01 | validate the file | проверь файл |
| tr02 | delete the cache | удали кэш |
| tr03 | copy the report | скопируй отчёт |
| tr04 | move the config | перемести конфигурацию |
| tr05 | upload the image | загрузи изображение |
| tr06 | download the package | скачай пакет |
| tr07 | save the draft | сохрани черновик |
| tr08 | archive the log | архивируй журнал |

Настройки: UnitId=`hnq01-enru`, role=`translation-executor`, domain=`TranslationDomain`, dimensions=64, k=3, confidenceFloor=.70; BPE vocab=320. Parse examples в исходном порядке; проверить sourceId=simulation, heldOut=false, unique ID и буквальные source/target. Нативные training examples имеют PredicateIds=[], DomainTags=["translation"], EvidenceIds=["P125-enru-memory-01:"+id]. Text selector — `s => s`. В request: runId=`run01`, те же unit/role/domain, source из eval, `new LogicalExecutionFlags()`, пустой metadata. Gold не подаётся в request. Все 24 eval случая берутся из файла, без подбора удобной панели; scorer читает accepted variants и исходную normalization после сохранения outputs.

На каждой панели выполнить Train один раз, 24 ExecuteAsync, сохранить состояние, восстановить свежий экземпляр и прочитать **только x01,x04,x05,x08** для restore parity с уже сохранённым output этой панели. Это не новый fit. У warm x05/x08 ещё не выучены; сравниваются outputs restore, не выдуманная правильность. Warm должен иметь четыре разных правильных oldExact, main — четыре новых правильных exact; 0→0 не считается обучением.

### Полный read-only bundle без новой production checkpoint schema

Будущий test helper `HnswQueryNormFixture.cs` сохраняет под `learning/<arm>/state/{warm4|main8}/`:

- `tokenizer.json` через существующий BpeTokenizer.Save;
- `index.json` через существующий HnswIndex.Save;
- `examples.json`: полные LogicalKnnTrainingExample<string,string> из обученной `_examples`;
- `unit.json`: исходный Manifest, identity, dimensions/k/floor и назначение selector=string-identity-v1; `bundle.json` — hashes и native assembly identities.

Только test helper читает точные private project fields `_tokenizer`, `_index`, `_examples` через reflection. Для restore он вызывает BpeTokenizer.Load, HnswIndex.Load и существующий единственный private KnnLogicalUnit constructor с **11** параметрами: string unitId, string role, string domain, BpeTokenizer, HnswIndex, Dictionary<string,LogicalKnnTrainingExample<string,string>>, Func<string,string>, int dimensions, int k, double floor, LogicalKnnModelManifest. Поле/тип/сигнатура должны совпасть; иначе конкретный setup failure, без поиска другой фабрики и без Train. Это fixture adapter к настоящему native consumer, не копия Execute/Vectorize и не public checkpoint API. Доступ к private полям Random запрещён.

Снимок пригоден для дальнейших **чтений** этого string→string Unit; продолжение Add/Train по нему не обещано, RNG continuation не сохранён. После этого назначения отчёты и уже назначенное E2E читают bundle/receipts; не повторяют warm/main подготовку. Missing/hash mismatch блокирует соответствующий input, не разрешает новый attempt.

## 6. Native тесты, реальные existing assertions и полный бюджет

Будущие файлы в namespace `LlmFromScratch.Tests.ExperimentFixes`:

- `Tests/ExperimentFixes/HnswQueryNormLearningTests.cs`: один Fact `NativeTraining_ReducesRepeatedNormWorkAndPreservesMemory`.
- `Tests/ExperimentFixes/HnswQueryNormContractTests.cs`: три candidate-only Facts ниже.
- `Tests/ExperimentFixes/HnswQueryNormFixture.cs`: loader/collector/bundle/scorer; без learner copy.

Learning Fact выполняет две панели §5, фиксирует реальные Train/Add/Execute/cosine/query-method counters, исходы всех строк, confidence bits, index/tokenizer/examples hashes, manifest и wall/CPU/allocations отдельно warm/main. Derived norm fields всегда названы source-derived; scorer не выдаёт их за CPU. Manifest parity исключает только TrainingTimeMs и ManagedAllocationBytes, которые показываются отдельными дельтами; остальные поля, output strings, confidence bits, evidence, warnings и payload state должны совпасть A/B. Общие assertions A проверяют валидное усвоение и целостность; улучшение norm-компонента и A/B parity проверяет Compare после двух terminal receipts. Main wall/allocations могут ухудшиться и должны остаться в отчёте.

B-only Facts получают новые private helpers через точный lazy MethodInfo **внутри Fact**; common test source не обращается к отсутствующему B-методу при компиляции или discovery A. Missing method на B — ошибка, не Skip.

1. `CachedNormCosine_MatchesLegacyOnFixedVectors`: шесть пар q/v: [1,2,3]/[1,2,3], [1,0]/[−1,0], [1,0]/[0,1], [0,0]/[1,0], [1e−6,0]/[1e−6,0], [−2,.5,3]/[.25,−1,2]. Прежний Distances.CosineSimilarity — bitwise oracle при одном runtime; independently проверить знак/zero/порог по формуле §4. Шесть вычислений новой нормы/косинуса, шесть legacy cosine; без Add/Train.
2. `SeparateSearches_RecomputeQueryNorm`: native seeded index с a=[1,0], b=[0,1]; два Add. Query=[1,.1] даёт a; после изменения того же массива на [.1,2] следующий Search даёт b и новый правильный score. Всего два Search; не кэшировать norm по объекту между вызовами.
3. `SeededSeam_PreservesLegacyPublicApiAndDefaultRandom`: обе public signatures неизменны, default ctor сохраняет reference к старому static RNG, internal ctor — к переданному Random. Проверка project fields только read-only; ноль Add/Train/Search. Это совместимость общего seam, не отдельная измеренная победа.

Три exact existing methods запускаются до A и после B, assertions не меняются:

| Существующий FQN после `LlmFromScratch.Tests.` | Расход одного запуска |
|---|---|
| [KnnLogicalUnitTests.KnnLogicalUnit_TrainsWithBpeAndHnswThenPredictsNearestOutput](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/Tests/KnnLogicalUnitTests.cs#L14-L57) | 1 Train, 1 BPE Train, 2 Add, 1 Execute; ожидается SECURITY |
| [VectorSearchTests.Hnsw_SaveLoad_PreservesResults](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/Tests/VectorSearchTests.cs#L55-L69) | 2 Add, 1 Search, 1 index Save/Load; ожидается a |
| [VectorSearchTests.CosineSimilarity_IdenticalVectors_ReturnsOne](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/Tests/VectorSearchTests.cs#L71-L76) | 1 legacy cosine, без обучения |

Свежих падений здесь не измеряли. При regression исправлять пять HNSW call sites, norm scope/float order/threshold либо допущенный лишний diff; не менять expected SECURITY/a/one, не снимать zero/negative case. При несовпавшем графе сначала проверить один RNG7331 между warm/main и одинаковые source/data hashes; не менять seed ради PASS. Сломанный restore исправляется в test bundle adapter по точным native типам, без нового fit. Подготовка [02§10](02-learning-state-and-fixtures.md#native-build-prerequisite) одинакова A/B.

**Полный расход назначения:** основная A/B-пара — 4 Unit Train, 4 BPE Train, 24 Add, 112 Execute (96 основных +16 restore), по 4 index Save/Load и BPE Save/Load. Existing добавляют 2 Train/2 BPE Train, 8 Add, 2 Execute, 2 прямых Search и 2 index Save/Load. Candidate contracts добавляют 2 Add и 2 прямых Search. Итого **6 Unit Train, 6 BPE Train, 34 Add, 114 Execute, 4 прямых Search, 6 index Save/Load, 4 BPE Save/Load**; DPO/SFT/teacher=0. Восстановление ничего не добавляет в index. A:3 existing+1 learning=4 cases; B:3 existing+3 contracts+1 learning=7. Всего11 cases, пять test processes, две Rebuild/discovery. Вложенные cosine calls показываются отдельно и не считаются дополнительными Train.

## 7. Точные стадии и исполнение

Единственный root: `C:/Projects/llm_mvp_core`, ветка `codex/dpo-implementation`. Будущий `Tests.Support/ExperimentTransfer/Run-NativeAB.ps1` расширяется PlanId=FIX-HNQ. Общий harness §3/fixture/collector/test source фиксируется **до A**. A: Rebuild Tests Release → discovery4 → existing → learning → terminal baseline-stage. Затем один product norm factor/review/commit. B: Rebuild → discovery7 → existing → contracts → learning → compare. Никакого checkout/worktree/копии или повторения A.

```powershell
Set-Location C:/Projects/llm_mvp_core
$hnqBaselineCommit = (git rev-parse HEAD).Trim()
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-HNQ -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $hnqBaselineCommit
```

Только после завершённого A и candidate commit:

```powershell
$hnqCandidateCommit = (git rev-parse HEAD).Trim()
if ($hnqCandidateCommit -eq $hnqBaselineCommit) { throw "CANDIDATE_EQUALS_BASELINE" }
git merge-base --is-ancestor $hnqBaselineCommit $hnqCandidateCommit
if ($LASTEXITCODE -ne 0) { throw "BASELINE_IS_NOT_CANDIDATE_ANCESTOR" }
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-HNQ -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $hnqBaselineCommit -CandidateCommit $hnqCandidateCommit
```

Runner **пока не реализован**. Existing filter — OR трёх полных FQN таблицы. Learning filter: `FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.HnswQueryNormLearningTests.NativeTraining_ReducesRepeatedNormWorkAndPreservesMemory`; B contracts: `FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.HnswQueryNormContractTests.`. Каждый test process — `dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter <exact-filter>` с отдельным TRX/results. Discovery zero — ошибка. Root/HEAD/branch, common harness/fixture hashes, terminal A и B-only diff проверяются по03§9; native VectorSearch/AgentInfrastructure/Tokenizer DLL Location/SHA/MVID фиксируются после отдельного Rebuild каждой стадии.

Output: `artifacts/fbe/FIX-HNQ/attempt-0001/`, подкаталоги `baseline/A`, `learning/A`, `after/B`, `contracts/B`, `learning/B`; состояния — §5. A intent immutable с candidate:null; B пишет candidate-binding отдельно. Чужие/completed результаты не удаляются. Main process deadline5мин, Rebuild15мин; actual counters/not_reached сохраняются в finally. Ни formatter failure, ни отсутствие нового файла сами по себе не назначают replay.

## 8. E2E и готовое поручение

Это обучение получателя EN→RU, а не выбор domain. В [FIX-E2E](20-prompt-domain-e2e.md) внешний learned selector сохраняет ответственность за domain/role/typed handoff; HNQ bundle может быть read-only receiver fixture только при совместимом назначенном контракте. Он не заменяет существующий receiver автоматически и не увеличивает budget20. Direct Execute панели не называется полным prompt→domain E2E.

> **Назначен только FIX-HNQ после отдельного решения о переносе.** Модель — текущая default сессии без override; отчёт на русском. Прочитай AGENTS.md, CLAUDE.md, 50, 01, 02§10, 03§1–9; skills в docs/skills: architect-behavior-classifier-SKILL.md, trainable-architecture-decision-SKILL.md, phase-planning-SKILL.md, plan-review-SKILL.md, code-examination-SKILL.md, test-integrity-SKILL.md, prompt-patch-authoring-SKILL.md, human-communication-GLOSSARY.md.
>
> NAMED_CHECK HNQ-TRACE-AXIS: WRONG — назвать 56→14 измеренным CPU ускорением или заменить default RNG на7331; CORRECT — exact source-derived norm work внутри native Train, явный общий RNG seam, сохранённые default API и все ресурсные регрессии рядом. Используй только данные и бюджет этой карточки. После единственных warm4/main8 сохраняй полный read-only bundle; не переучивай для отчёта. `report.md`, `report-for-user.md`, `comparison.json`, `status.json` — в точном output root §7. Любое полезное измеренное улучшение — успех своей оси/профиля; это не автоматический ProductReady. Никаких новых DPO: при отдельной потребности генератор — внешняя открытая китайская модель, роль человека при выборе и оценку качества выполняет CODEX с astra 6 ultracode.
>
> Один запуск — один handle: exec_command.session_id → write_stdin с пустыми chars, yielded functions.exec.cell_id → functions.wait, другой tool — его документированный wait. Ждать порциями ≤60 s до terminal в том же ходе; не завершать «жду», не делать blind polling, relaunch или новый attempt. Прямой STOP/NEXT немедленно прекращает прежний ID, включая старый report/commit/cleanup. Следующий stable ID читать из строки FIX-HNQ текущего реестра, не из старого номера опыта. Старые эксперименты сохранять. Документная публикация не разрешает сейчас внедрение, обучение или full CI.
