# FIX-LFT — повторно использовать транспонированную замороженную матрицу при обучении

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q29](01-registry.md#work-fix-lft). Для общего включения действует измеренное ограничение этой строки. Возврат к работе — по записанному условию для её домена. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

## Уточнение допуска по показателям — 3 ноября 2026

**FIX-LFT: Приостановить общий перенос по времени обучения.** Вошёл по памяти. CPU одинаков, но время обучения по часам ухудшилось >5%; один короткий прогон не устанавливает причину.

- Точность: После подготовки 2/2 учебных; после окна A и B 2/2. Экзамен 1/2, прироста не показано.
- Полезный результат: Выделения 13 631 888 → 12 124 496 Б (−11,06%).
- Цена и ограничения: Время 16,4268 → 17,9350 мс (+9,18%). Пик +0,70%; новый кеш 249 856 Б.
- [Полное сравнение и статус кода](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#benefit-fix-lft).

Этот вывод относится к названным ID, а не ко всем механизмам документа. Старый текст плана и опыта сохранён как доказательство, но его общий статус «к переносу / разрешён» не переопределяет это уточнение. Ближайшее действие — разбор уже сохранённых данных и конкретного регресса; новое обучение, повтор PRIMARY и переключение product default этим дополнением не назначаются. Для отдельного логического ремонта назвать нарушенный контракт и сохранить отрицательные контроли, а не переносить весь эксперимент под словом «bugfix».


<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски CORE/LEARN:** Kernel parity проверить на назначенном реальном learning пути: независимые tensors/gradients/updates и ненулевой правильный readout. Отрицательный контроль обнаруживает возврат ошибки, положительный сохраняет lawful путь; isolated0→0 не качество обучения. Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Для ресурсного переноса сравнивать actual native операции на том же обученном состоянии и в одинаковом окне. Managed allocations, peak и CPU различаются; прогрев/дрейф показываются по блокам. P08 продемонстрировал полезный gold-mass эффект при allocations+155,65%: наличие quality gain и кода в продукте не отменяет HOLD по памяти. Константа счётчика не является наблюдением.

Для FIX-LFT исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Стоимость относится к точно названному окну. Контрпример P09: разность allocations четырёх arm windows, делённая на4, включает создание входов/cache/receipts и не является постоянной стоимостью одного Forward. Для этой оптимизации указать размер работы, warmup, что внутри/снаружи счётчиков и одинаковую обвязку A/B; не закреплять точные bytes по одному такому среднему. Проверять существующие tests и дополнять их learned fixture, не объявлять отсутствующими по одному отчёту.

Применимость: FIX-LFT. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Полный ресурсный бюджет обязан включать обязательные диагностические вызовы и неудачные попытки, с отдельными единицами model/layer и optimizer-call/effective-update. Контрпример P11: 40 model Forward основной части +19 контрольных =59; 8 optimizer calls дают лишь4 изменения весов. Oracle/FD, заявленный обязательным, должен влиять на принятие результата, а не только выводиться в JSON. Равенство старому алгоритму подтверждать независимой проверкой, не комментарием в исходнике.

Применимость: FIX-LFT. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-LFT

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE/LEARN по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**K — владение допущенным состоянием и восстановление.** Показать, какой объект хранит допущенные параметры, примеры, конфигурацию или ключ кэша; интерфейс IReadOnlyList сам по себе не доказывает неизменность. Контроль после создания/допуска: изменение исходного caller-owned массива или внешнего представления не меняет захваченную запись незаметно. Законное обновление/смена model identity вызывает предусмотренное обновление или инвалидирование кэша, а не запрет обучения. Если reader сохраняется/восстанавливается, сравнить поведение обычного consumer по сохранённым данным и настройкам: reconstruction с default не является эквивалентным restore opt-in режима. Ошибка из KDP: catalogue был проверен, затем изменён через возвращённый массив; schema не содержала catalogue. Для этой карточки проверять её собственный carrier, не добавлять чужой KNN-механизм ради теста. Если изменяемого carrier/сохранения нет, отметить это явно в evidence и не выдумывать лишний runtime.

**M — метрика привязана к фактической стадии обучения.** Для обучающего loss/reward записать состояние, на котором он вычислен, отдельно от номера опубликованных после update весов. У DpoTrainer последняя StepMetrics — pre-update; не называть её оценкой конечного checkpoint. Пример P02: B уже стартовал с margin 0,002773 после SFT; отношение последних значений ×3,22 не равно отношению приростов DPO. Любое сравнение приобретения/ремонта использует собственное before каждого этапа и одинаковый reader. DPO Accuracy=(reward_margin>0) означает улучшение относительно reference, а не правильный ответ: T2 имеет policy chosen−rejected=−0,1827326 при положительном reward. TRAIN/EXAM correctness и сохранённые logP остаются отдельными полями. Если эта карточка сравнивает математически равные оптимизации, сохранить её A/B equality; не требовать искусственного роста точности и не добавлять post-update Forward, когда нужное чтение уже предусмотрено.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Уточнение исполнения, 27 октября 2026.** Проверка повторов на одной матрице пропустит ошибочный глобальный cache. Использовать уже назначенный lifecycle: завершить owner, изменить допустимым способом базу, открыть новый owner и сравнить с uncached oracle; затем cold-load прежний full state. Два одновременно живых graph проходят backward в обратном порядке и не делят перезаписываемый scratch. Сброс moments или повторные250updates после restore запрещены; primitive/learned call budgets §7 сохраняются.

Общие примеры правильной и ошибочной проверки — [контракт03§24](03-test-and-execution-contract.md#transfer-audit-20261027). Это дополнение плана; новые проверки сейчас не запускались.

Статус: **план, не внедрение**. Основание — квалифицированный повтор №02 `NEURAL-FROZEN-TRANSPOSE-RETRAINED-01 / FTR01`, опубликованный в `f12e190b47`. Это новый кандидат: прежнее исключение cold LFT01 не учитывало этот повтор. Следующий ID — **FIX-AGC**, порядок задан [реестром](01-registry.md). Рабочий каталог будущего исполнителя: `C:/Projects/llm_mvp_core`; модель исполнителя — настроенная по умолчанию, без принудительной замены.

## 1. Измеренное основание и ограничения

Первичные файлы: [A.json](../../../../experiments-v2/RP01-MICRO/FTR01/results-01/A.json), [B.json](../../../../experiments-v2/RP01-MICRO/FTR01/results-01/B.json), [verdict.json](../../../../experiments-v2/RP01-MICRO/FTR01/results-01/verdict.json), [report.md](../../../../experiments-v2/RP01-MICRO/FTR01/results-01/report.md). Исполнение — [Program.cs:83](../../../../experiments-v2/RP01-MICRO/FTR01/Program.cs#L83); его [Runner.csproj](../../../../experiments-v2/RP01-MICRO/FTR01/Runner.csproj) компилирует копии LFT01. Это доказательство фактора в эксперименте, а не уже внедрённого native исправления.

Оба плеча приобрели TRAIN `0/2→2/2` за 250 native SFT updates; четыре измеряемых updates сохранили `2/2`. TRAIN NLL `2.3840828715→2.3331911820` одинаков в A/B. EXAM остаётся `1/2`, обе предсказанные метки `-`: освоение общего правила знака не доказано. Совпали acquisition loss stream, стартовые weights/optimizer/readouts, четыре последующих loss, конечные weights/optimizer/readouts; frozen hashes не менялись, trainable weights менялись.

| Ось за четыре updates | A: переставлять каждый раз | B: кэш | Вывод |
|---|---:|---:|---|
| Выделения, bytes | 13 631 888 | 12 124 496 | −1 507 392, около −11.06%: полезный измеренный успех |
| Удерживаемые bytes кэша | 0 | 249 856 | Дополнительная живая память |
| Cache misses / hits | 0 / 0 | 8 / 24 | По одной исходной перестановке восьми матриц, затем reuse |
| Wall, ms | 16.4268 | 17.9350 | Ухудшение в этом коротком наблюдении |
| CPU, ms | 15.625 | 15.625 | Измеренного улучшения нет |
| Process peak, bytes | 45 010 944 | 45 326 336 | +315 392; peak всего процесса, не только окна |

GC counts одинаковы. Не превращать allocations в «память процесса снизилась» или в обещание ускорения. Историческое наблюдение после 400 updates с TRAIN1/2 не доказывает оптимальность остановки на250: это не сравнение двух одинаково аттестованных S0 в данном A/B.

## 2. Где переносить и почему одного RequiresGrad недостаточно

Native получатели:

- [00.TensorCore/Layers/Linear.cs](../../../../00.TensorCore/Layers/Linear.cs), `Forward(Tensor, CancellationToken)`: сейчас каждый вызов делает `TensorOps.Transpose(Weight.Value,0,1,...)` и затем MatMul.
- [10.LoRA/LoraLinear.cs:112](../../../../10.LoRA/LoraLinear.cs#L112): base projection вызывает этот Linear. Перестановки trainable A/B LoRA **не кэшировать** между updates.
- [03.TransformerModel/LlmModel.cs](../../../../03.TransformerModel/LlmModel.cs): владение моделью, загрузка base/adapter checkpoint и жизненный цикл модельного scope.
- [04.TrainingLoop/Trainer.cs:67](../../../../04.TrainingLoop/Trainer.cs#L67), `TrainSupervisedNextTokenStep`: потребитель scope при серии SFT updates. DPO подключается только последующим отдельным переносом с собственным доказательством; не расширять этот бюджет на него.

`Parameter.Value` имеет getter, но Tensor хранит изменяемые значения. `RequiresGrad=false` запрещает вычисление градиента, **не запись весов при LoadWeights или через storage**. Кэш по объектной ссылке и этому флагу без lifetime-контракта неверен.

Выбранная граница переноса: **явный scope неизменной базы для принадлежащей trainer модели**, включаемый только между загрузкой общего S0 и завершением назначенных четырёх SFT updates. Обычный `Linear.Forward` вне scope сохраняет текущую семантику. Не добавлять глобальные static counters/flags из экспериментальной копии в product.

1. Добавить internal `FrozenLinearTransposeScope` в `00.TensorCore/Layers/FrozenLinearTransposeScope.cs`: `Open(IEnumerable<Linear>)`, `Dispose()`. Каждый выбранный Linear получает внутреннюю ссылку на этот scope; `Linear.Forward` запрашивает его detached transpose. Таблица владеет записью по identity Linear, только для замороженного allowlist; создание ленивое. Отмена до публикации записи не оставляет частичный tensor. В `00.TensorCore/Layers/Module.cs` добавить internal обход зарегистрированных модулей с устранением дублей по object identity; он нужен для всех восьми frozen Linear, включая FFN/head, а не только q/v. Не обходить модель через reflection в product.
2. В `03.TransformerModel/LlmModel.cs` добавить internal `ExecuteWithFrozenBaseTransposes<T>(Func<T> action)`. Он под `_adapterMutationSync` проверяет отсутствие активного scope, отмечает `_frozenBaseTrainingActive`, обходит Linear, открывает scope, вызывает action и в finally снимает scope/флаг. В `04.TrainingLoop/Trainer.cs` добавить конкретный owner серии `TrainSupervisedNextTokenSteps(LlmModel model, AdamW optimizer, IReadOnlyList<SupervisedTokenStep> steps)`; новый public readonly DTO `TrainingLoop.SupervisedTokenStep(IReadOnlyList<int> tokenIds, float weight)` хранит token IDs и weight, batch возвращает `float[]` loss в исходном порядке. Owner вызывает указанную операцию модели один раз и прежний `TrainSupervisedNextTokenStep` для каждого элемента. Это SFT consumer нового пути; прежний одиночный API остаётся без кэша. Для межсборочного доступа добавить ровно `InternalsVisibleTo("TransformerModel")` в `00.TensorCore/AssemblyInfo.cs` и `InternalsVisibleTo("TrainingLoop")` рядом с существующими friends LlmModel; перед применением сверить AssemblyName этих native csproj. Не делать scope public и не разрешать новую global friend ко всем сборкам.
3. До входа закончены load/attachment. Явный active-scope guard отказывает **до первой записи** в обоих overload `LoadCheckpoint`, обоих `LoadAdapterCheckpoint` и `ExecuteAdapterMutation` (он покрывает attach/rebind/unfreeze через mutation lease). Учесть native private readers, которые вызываются этими entrypoints: guard стоит перед чтением с последующим copy, не после него. Реентерабельность C# lock не обходит этот guard: callback того же потока получает отказ; другой поток сериализуется и может загрузить только после Dispose. Новых merge/reset entrypoints этим переносом не вводится. Прямое изменение `RequiresGrad=true` у конкретной матрицы заставляет Linear удалить её cache entry и использовать свежий Transpose; это проверяется отдельно. Не хэшировать и не сравнивать все значения базы на каждом Linear call. На hit проверяются только identity/layout/RequiresGrad/allowlist по§7.1; неизменность значений внутри серии обеспечивается контрактом исключительного владения и guards штатных entrypoints.
4. Новый batch owner требует исключительного владения моделью и базой на всё время вызова. Raw внешние записи в публичный Tensor.Data внутри серии не поддерживаются — в том числе последовательные записи между Forward, а не только concurrent writes. Один lock не предотвращает такие записи, и metadata stamp не обнаруживает изменение значений; вызывающий код, который не обеспечивает исключительное владение, использует старый одиночный API. Это ограничение API, а не предположение, что Tensor стал immutable. Scope нельзя выдать наружу и продолжить после возврата owner.
5. Dispose очищает ссылки; следующий независимый учебный scope, загрузка другого checkpoint, другой экземпляр модели или trainable base не наследуют старые transpose. В кэше нет autograd history, optimizer moments или logits. Bias и LoRA вычисляются как прежде. Вне нового owner Linear остаётся свежим; нельзя автоматически включить кэш всем callers по `RequiresGrad=false`.

Такой ownership — обязательная часть будущего diff. Если при реализации окажется, что узкий owner не покрывает запись базы, сохранить default path и отметить неперенесённую часть. Не объявлять экспериментальную экономию доказательством безопасности глобального кэша.

**Граница нового измерения:** scope, owner, обход Linear и guard entrypoints — необходимые части будущей native реализации. Их суммарная стоимость историческим FTR01 не измерена. В A/B окно обязательно включает вход/выход owner, misses и guards; исторические −1 507 392 B не обещаются этому варианту автоматически. Не добавлять per-hit full-base bit validation или persistent cache из другого замысла: это другой фактор.

Опубликованный detached transpose неизменяем. Dispose удаляет ссылки владельца, но не меняет storage, который ещё нужен созданному graph. Исключительное владение моделью на время серии — явная предпосылка нового API; raw внешние записи в mutable Tensor.Data внутри серии не поддерживаются. Старый одиночный API остаётся доступным без кэша.

## 3. Математическое основание

Для постоянной матрицы W каждое обращение использует одно и то же Wᵀ. Материализация перестановки не меняет чисел и порядок MatMul; повторное чтение того же detached Wᵀ сохраняет forward и градиент по входу. Градиент по W отсутствует только в выбранном frozen режиме. Для изменяемой LoRA A/B условие не выполняется.

При k обращениях к матрице m×n перенос заменяет k копирований по mn элементов одним копированием и хранением mn элементов. По восьми матрицам FTR01 сумма полезного payload —249856bytes; три повторных обращения дают минимум749568bytes устранённых payload-копий. Измеренная разница1507392bytes больше: в ней также структуры/временные allocations реализации Transpose. Нельзя вывести всё измеренное число из одной формулы payload. Равенство арифметики поддерживает приоритет кандидата, но не доказывает уменьшение wall/peak.

Область применимости — модели с многократно используемой неизменной базой, включая этот LoRA SFT профиль. В данном опыте тема — C# result sign; никаких выводов о domain router, AST, POS или переводе из него не следует.

## 4. Точное состояние и минимальный будущий тест

Фиксированная конфигурация из raw: byte BPE без merges/EOS, V256/C64/H64/L1/Q4/KV2/FF176, RoPE10000/RMS1e−5; base seed62001, LoRA seed62002, stream `frozen-transpose-probe-v1`, q/v r4/alpha8/dropout0. Это **модель под тестом**, не выбор модели агента.

TRAIN строго две строки: `C# result sign: 9-4 =`→`+`; `C# result sign: 4-9 =`→`-`. EXAM: `8-3`→`+`, `3-8`→`-` с тем же framing. Материализация последовательностей и Observe — из указанного Program, без изменения пробелов и токенов. EXAM не участвует в acquisition или выборе остановки.

В `results-01` есть JSON/hashes/logs, **нет сохранённого полного S0**. `CaptureState()` в Program сохраняет снимок в памяти для хэша; хэш не восстанавливает moments. Поэтому запрещено писать, что готовый FTR donor уже существует, или подменять его weights-only состоянием другого опыта.

Назначить единственное будущее хранилище: `C:/Projects/llm_mvp_core/Tests/Fixtures/ExperimentFixes/v1/FTR01/S0/`. Файлы будущего bundle: `model.bin`, `optimizer.bin`, `rng.json`, `tokenizer.json`, `manifest.json`; manifest хранит recipe, named weights, shapes, dtype, source identity, SHA каждого файла, optimizer step250/moments, data cursor (следующая T1), readouts TRAIN2/2 и EXAM1/2. Модель: `LlmModel.SaveCheckpoint/LoadCheckpoint` после создания совпадающих topology/adapters. AdamW: `ExportPortableState(named bindings).ToBytes()` → `AdamWPortableState.FromBytes(...)` → `TryImportPortableState` с **четырьмя именованными trainable bindings новой модели**; отказ импорта не заменяется свежим optimizer. Owner-bound `CaptureState/RestoreState` не переносится между экземплярами. RNG: `LlmModel.CaptureRandomState(topologySha, loraIdentitySha,250)` → typed `LlmModelRandomStateSnapshot` JSON → `RestoreRandomState(snapshot, identity)`. Identity SHA берутся из построенной canonical topology/LoRA config этого профиля и записываются в manifest; не использовать hashes H512. Dropout0 не разрешает выдумать historical RNG: это новый один раз сохранённый native bundle с собственным receipt.

Сначала прочитать этот **конкретный** bundle. Если он уже пригоден, никаких250новых updates. Если отсутствует, отдельный заранее назначенный producer внутри разрешённой будущей реализации делает **одну** подготовку ровно250updates, с cache disabled, LR.001, AdamW(.9,.95,decay0,eps1e−8), clip1, batch1, accumulation1, T1/T2 попеременно. Его единственный process получает CreateNew ownership и terminal receipt; завершить и закрепить bundle/hashes **до фиксации baseline input commit и intent**. Candidate никогда не запускает producer. Сохранить полный S0; проверить2/2 и совместимость. Если не выполнено — `fixture_not_qualified`, не продолжать до3000 и не менять LR/seed. Первичный эксперимент допускал3000, но это не бюджет нового переноса. Непригодный уже существующий bundle не перезаписывается и не заменяется молча новой подготовкой.

Конкретный будущий producer: `Tests/ExperimentFixes/FtrLearnedStateProducerTests.cs`, метод `CreateQualifiedS0Once`, namespace `LlmFromScratch.Tests.ExperimentFixes`. Общий `Run-NativeAB.ps1` получает только дляFIX-LFT стадию `PrepareFixture`: она проверяет назначенный путь, возвращает пригодный completed bundle без build/Train, а при отсутствии выполняет один Rebuild, exact discovery и один process с фильтром `FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.FtrLearnedStateProducerTests.CreateQualifiedS0Once`. Ownership — `artifacts/fbe/FIX-LFT/prepare-0001/`; повторный вход читает receipt или ждёт его handle. Producer не включается в learning/existing filters и не исполняется вB. После него фиксируются bundle/input hashes и baseline commit; его build не подменяет обязательный baseline Rebuild.

Затем независимые A/B восстанавливают **один и тот же** полный S0, включая AdamW step/moments. По4SFT updates T1,T2,T1,T2 с теми же параметрами; A — исходный native code без scope, B — новый native owner/scope. Одна новая подготовка максимум250, измерение8updates, никакого DPO/Generate/внешнего учителя. При готовом S0 весь training budget —8updates. Pre/post readout в каждом плече —4TRAIN/EXAM prompts до и4после, суммарно16 scoring forwards. Producer имеет4 cold и4 terminal readouts; при фиксированных250updates промежуточные panels каждые25 не назначаются. Это минимальный новый test recipe, не replay всех historical diagnostics. Контрактные assertions повторно читают raw без Train; четыре дополнительных learned restore/oracle forwards явно учтены§7.3.

Сохранять `C:/Projects/llm_mvp_core/artifacts/fbe/FIX-LFT/attempt-0001/` по общему harness: `intent.json`, `{contracts,learning}/{A,B}/`, `comparison.json`, `report.md`. Четыре B-only contracts имеют отдельные receipts, в A отмечены `not_applicable`, не false PASS. Не создавать заранее runtime-owned output, если native writer требует CreateNew. Existing complete A/B читаются, живой process ожидается; artifacts не удаляются ради rerun. Измеряемое окно включает вход/выход scope и первоначальные misses, не включает подготовку и diagnostic forwards. Паритет losses/weights/optimizer/readouts обязателен, вместе с2/2→2/2 и параметрическими updates. Полезное снижение allocations достаточно для успеха этой оси; любые wall/CPU/peak ухудшения показать рядом.

## 5. Тесты и ремонт ожиданий

Новые будущие тесты в `Tests/ExperimentFixes/FrozenLinearTransposeTests.cs`, namespace `LlmFromScratch.Tests.ExperimentFixes`:

| Тест | Проверяемое свойство |
|---|---|
| `Scope_ReusesFrozenTransposeAndPreservesInputGradient` | B-only:2fresh Linear forwards +2scoped forwards,4Backwards,0optimizer. Сравнить output/input gradient; второй scoped call —hit |
| `TrainableWeightAndLoraUpdates_BypassFrozenCache` | B-only:2Linear и2LoraLinear forwards с прямой контролируемой записью trainable matrices между ними;0Backward/optimizer. Ни base trainable, ни A/B LoRA не застывают |
| `LoadAndUnfreeze_InvalidateOrRejectBeforeMutation` | B-only после learning receipt:4readouts восстановленного final B по§7.4 и2Linear forwards при unfreeze;0Backward/optimizer. Reentrant LoadCheckpoint/LoadAdapterCheckpoint/ExecuteAdapterMutation внутри scope отклонены до записи; base меняется только вне scope, после Dispose разрешён restore |
| `DisposeCancellationAndSeparateModels_DoNotShareCache` | B-only:4Linear forwards, включая один cancelled до materialization;0Backward/optimizer. Нет глобального entry, partial tensor или reuse другой модели |
| `NativeSft_FromQualifiedS0_PreservesTrajectory` | Описанные4updates/arm, паритет native A/B, корректность2/2 и ресурсные readouts |

В первом новом scope-тесте закрепить прямоугольные данные: X shape[2,3] = [−0.4,0.2,0.7,0.1,−0.3,0.9], W shape[2,3] = [0.3,−0.2,0.5,−0.6,0.4,0.1], bias=[0.05,−0.07], upstream G shape[2,2] = [0.2,−0.3,0.7,0.4]. Независимый scalar oracle вычисляет XWᵀ+b и GW; сверять с fresh и scoped путями с допуском `1e−6+1e−5·abs(expected)`, сохраняя exact A/B state assertions учебного теста. Перед каждым из четырёх сравнений использовать независимый X или обнулить его gradient: накопление предыдущего backward не должно подменить GW. Oracle не вызывает новый Linear/модель и не добавляет Forward/Backward к уже назначенным четырём. Проверка одного и того же ошибочного результата двух путей недостаточна.

Сохранить действующие `Tests/LoraTests.cs`: `LoraLinear_Construction_DoesNotMutateBase`, `LoraApplicator_ValidApplyFreezesBaseAndOwnsOnlyCanonicalAdapterParameters`, `LoraAttention_AllProjectionBGradientsMatchFiniteDifferenceThroughFinalNorm`, `LlmModel_BaseOnlyCheckpointWritesVersionTwoAndRoundTrips`, `LlmModel_LegacyLoadIntoAttachedModelPreservesAdapterValues`. Различать уже существующие mutation/restore проверки и новый учебный тест; не запускать весь LoraTests дополнительно ради этого фактора.

Если падает проверка числа Transpose calls, заменить лишь устаревшее ожидание повторной работы на проверку one-miss + reuse **внутри scope**. Вне scope старый контракт сохраняется. Не ослаблять числовые, finite-difference, base-immutability, cancellation и checkpoint assertions. Измерение allocations не оформлять точным machine-independent требованием −1507392; постоянный тест проверяет отсутствие повторной материализации, native A/B отчёт — фактическую полезную ось.

## 6. Исполнение после отдельного разрешения реализации

Общий lifecycle, навыки, русский отчёт, native assembly provenance, точная discovery и способ ожидания — [03](03-test-and-execution-contract.md); применяются без поиска альтернативных проектов. Использовать уже спланированный общий `Tests.Support/ExperimentTransfer/Run-NativeAB.ps1`, а не создавать ещё один runner. Расширить его конкретным PlanId `FIX-LFT`, SourceRoot и двумя измеряемыми стадиями Baseline/Candidate и условной стадией PrepareFixture. Этот entry ещё должен быть реализован; команды ниже **не являются существующей готовой возможностью**. `$baselineCommit` и `$candidateCommit` — полные проверенные SHA стадий по03§9, не значения, выбранные моделью произвольно:

```powershell
Set-Location C:/Projects/llm_mvp_core
# Только при отсутствии полного пригодного S0; до фиксации $baselineCommit.
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-LFT -Stage PrepareFixture -SourceRoot C:/Projects/llm_mvp_core
# Закрепить общие tests/fixture inputs и получить $baselineCommit по03§9.
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-LFT -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit
# После native diff, в том же checkout; A повторно не выполнять.
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-LFT -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit -CandidateCommit $candidateCommit
```

Один targeted Rebuild/discovery на стадию. **A:** пять перечисленных existing методов и learning Fact со старым4-step вызовом. **B:** те же5existing, затем learning Fact через новый batch owner, затем четыре новых scope contracts. A выполняет два последовательных exact-filter запуска — existing5, затем learning; B три — existing5, learning, затем contracts4. Каждый Fact исполняется один раз; не полагаться на порядок xUnit. Restore contract читает final B только после terminal learning receipt. Test source общий до baseline; новые API вызываются через узкий lazy reflection bridge только в B, их отсутствие в A не ломает compilation/discovery. Нельзя заранее вносить cache scope в baseline ради компиляции тестов. Learning Fact каждой стадии пишет только свою квитанцию; Compare читает их после terminal B.

Полный дополнительный расход existing5:0optimizer updates, finite-difference метод даёт28модельных Forward +4Backward на стадию, всего56/8. Четыре новых B-contracts:14Linear/LoraLinear calls (один отменён до расчёта),4learned-model forwards,4primitive Backward,0optimizer; вложенные primitive calls обычных model forwards отдельно не считать как новые модельные прогоны. Основной learning —8updates/8native backward,16scoring forwards, плюс необязательный **однократный** producer250updates/250backward,4cold и4terminal readouts. Полные суммы и разделение model/primitive Backward —§7.3. Сохранять категории отдельно; не включать эти tests в resource окно. Discovery проверяет exact method names, не wildcard всего LoraTests. Не подключать experiment-copy DLL вместо native и не повторять fit ради formatter/restore assertions.

Процессы без producer:2Rebuild,2discovery,5test processes (A:existing затем learning; B:existing затем learning затем contracts с чтением final B),16test-case executions:6вA и10вB. При действительно отсутствующемS0 добавить **один** producer Rebuild/discovery/test process и один producer case, а не скрыто увеличивать каждый arm. Подготовка не измеряет полезный фактор и не получает самостоятельного успеха.

При запуске оставаться в задаче до terminal state. Использовать доступное blocking wait исходного handle, а не частый опрос файла, второй monitor или придуманную параллельную работу. Если инструмент возвращает session ID, ждать именно её штатным wait; истечение одного ожидания не означает смерть процесса. Перед восстановлением/повтором проверить исходный handle, exit и сохранённые artifacts. Исправление formatter/пути не требует нового обучения. Прямое указание Любы остановиться или перейти к другому ID выполняется сразу; старый опыт не продолжается под видом отчётности. Эти правила не разрешают запуск сейчас: текущий этап — только планирование.


**STOP/NEXT:** немедленно прекратить прежнюю карточку; запрещено сначала дописывать её отчёт, чинить тесты, делать commit/cleanup или последний запуск. При NEXT перейти к точному ID из актуального реестра. Исторические цитаты не являются живой командой. Default-модель исполнителя, русский отчёт и bounded wait исходного handle≤60секунд — по03; новый process вместо ещё живого не создавать.

## 7. Недостающие условия защищённого native переноса

Этот раздел дополняет выбранный в§2 **internal scope + пакетный Trainer owner**. Второго public scope и новой цепочки forward overloads нет. Включены уточнения из `82860557802f4f5cf3065010011c50056d07dc25`: исключительное владение неизменной базой, guards штатных writers и metadata stamp; per-hit full-base bit scan не входит в этот фактор. Историческая экономия не доказывает стоимость нового native owner.

### 7.1. Entry, исключительное владение и старые графы

Entry связывается с экземпляром Linear, reference `Weight.Value`, reference storage/Data, обеими dimensions и strides, согласованной row-major длиной. Dimensions/strides сохраняются как собственные metadata значения, а не изменяемые ссылки. Кэш применим только к rank2 frozen W из allowlist с поддерживаемой contiguous раскладкой. На hit проверяются этот stamp и RequiresGrad, без чтения каждого float базы, hash или сравнения с transpose. Равенство длины не доказывает равенство формы. При неподдерживаемой layout действует прежний native путь, а не новая интерпретация storage.

При несовпадении reference, shape/strides, RequiresGrad или allowlist удалить entry и использовать свежий результат прежнего Transpose; malformed shape сохраняет прежнюю ошибку. Новая entry публикуется только после полностью успешного построения. Старый detached tensor не изменять in-place, не переиспользовать его storage как scratch и не возвращать в pool при Dispose: живой предыдущий graph сохраняет своё содержимое для backward. Отмена проверяется на входе и штатным Transpose при materialization; partial entry не публикуется. Trainable W и LoRA A/B не кэшируются.

Внутри действующего owner значения выбранных frozen matrices не меняются. Raw внешние записи между Forward нарушают этот контракт и не получают обещания обнаружения; обычный single-step API остаётся путём для callers без такого владения. Штатные LoadCheckpoint/LoadAdapterCheckpoint/ExecuteAdapterMutation внутри owner запрещены до записи. После Dispose разрешены загрузка и изменение базы, а следующий scope начинает с пустого кэша. Native A/B включает owner, обход Linear, misses, metadata checks и guards; исторические −1507392allocated bytes и +1.5082ms не обещаются. Если полезная ось не подтверждается, записать результат; не отключать штатные guards и не добавлять per-hit scan как второй неиспытанный фактор.

### 7.2. Реальные сборки и единственный owner

Нативные проекты — [TensorCore.csproj](../../../../00.TensorCore/TensorCore.csproj), [TransformerModel.csproj](../../../../03.TransformerModel/TransformerModel.csproj), [TrainingLoop.csproj](../../../../04.TrainingLoop/TrainingLoop.csproj). В них нет собственного переопределения AssemblyName; preflight проверяет evaluated names TensorCore/TransformerModel/TrainingLoop. В TensorCore [AssemblyInfo.cs](../../../../00.TensorCore/AssemblyInfo.cs) уже есть friend Tests; в LlmModel — LoRA и Tests. Сохранить их, добавить только два точных friend из§2. Не делать scope public ради доступа trainer и не создавать reflection traversal в product.

Обход зарегистрированных modules по identity включает восемь base Linear профиля L1: q/k/v/o, gate/up/down, lm_head. Новый public `Trainer.TrainSupervisedNextTokenSteps` открывает **один** internal model scope вокруг четырёх прежних `TrainSupervisedNextTokenStep`; возвращает четыре loss без повторного Forward. Его фиксированный список steps создаётся до owner. Single-step API и inference вне scope сохраняют старую семантику. Active-scope guard в load/mutation entrypoints обязателен даже под reentrant lock.

Этот native batch owner не требует внедрения FIX-MC. Связывание с server-owned SFT transaction выполняется только после отдельно принятого MC и в границах уже назначенного E2E; оно не добавляется скрыто к LFT diff или бюджету. Владелец DPO не подключается. Scope/gradient tests и native SFT capture проверяют именно новый Trainer owner; они не доказывают ещё не выполненное server wiring.

### 7.3. Один donor, сериализация, scheduler и полный бюджет

Единственный адрес — `Tests/Fixtures/ExperimentFixes/v1/FTR01/S0/` из§4. Единственный producer — `FtrLearnedStateProducerTests.CreateQualifiedS0Once` и стадия `PrepareFixture` общего runner из§4/6. Отдельный Prepare-FrozenTransposeStart.ps1 не создаётся. Producer вызывается только при отсутствии bundle, до baseline input commit/intent; Candidate никогда его не повторяет. Иная папка или непригодный существующий bundle не разрешают молчаливый новый fit. Historical FTR01 хранит hashes/JSON результатов, а не уже доступный полный donor.

Формат optimizer — `AdamW.ExportPortableState(canonicalBindings).ToBytes()` с native `FromBytes/TryImportPortableState` после final adapter attachment; owner-bound state и JSON float moments не заменяют переносимое состояние. `rng.json` в§4 — **typed JSON snapshot**, а не файл strict binary writer. Не вызывать `RepositoryOwnedDpoRandomStateFile.WriteCreateNew` с именем rng.json: тот API требует своё `BuildFileName(step)`. Сохранять `JsonSerializer.SerializeToUtf8Bytes(snapshot)` и читать `JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(bytes)` с одинаковыми default options, проверкой null и наличия всех обязательных полей. Native carrier [LlmModelRandomState.cs](../../../../03.TransformerModel/Randomness/LlmModelRandomState.cs) — публичный positional record; такое восстановление и IReadOnlyList поддерживаются [constructor contract](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/immutability) и [таблицей коллекций System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/supported-types#systemcollectionsgeneric-namespace).

Round-trip сравнивает FormatVersion/Algorithm/TopologySha256/LoraConfigurationSha256/RandomStateStep по значениям и `Entries.SequenceEqual`: record equality всего Snapshot не доказывает содержимое IReadOnlyList после десериализации. Каждый entry сохраняет AdapterKey/StreamDomain/InitializationSeed/DropoutSeed/ConsumedCursor; три последних поля остаются ulong, без JS Number/double. Fixture отдельно требует `RandomStateStep==250`: native RestoreRandomState проверяет лишь неотрицательность шага. Затем вызвать `RestoreRandomState(snapshot,new LlmModelRandomStateIdentity(topologySha,loraSha))`, который проверяет identity и полный slot mapping до изменения cursors. Этот carrier содержит RNG адаптеров; он не заменяет optimizer, trainer counter или data cursor. Manifest связывает actual file SHA, config/topology/LoRA identity, четыре trainable binding keys, optimizer step250, cursor и readouts. Dropout0 не заменяет сохранение state.

Historical producer TrainingConfig: batch1/seq64, warmup0, **TotalSteps3000**, minLR=maxLR=.001, clip1, accumulation1, decay0. Внешний бюджет строго250updates: TotalSteps3000 не разрешает продолжение. Measured Trainer имеет **TotalSteps4**, supervised counter0 и восстановленный AdamW step250. LR постоянный. Нельзя подменять trainer counter optimizer step или брать scheduler H64-404. После четырёх измеряемых updates полный final A/B bundle имеет AdamW step254 и RNG snapshot RandomStateStep254; restore contract читает эти254, тогда как проверка250 выше относится только к исходному S0. Счётчик нового measured Trainer при этом4.

| Категория | Model.Forward | Model Backward | Primitive Forward attempts / Backward | Optimizer updates |
|---|---:|---:|---:|---:|
| Один необязательный producer:4cold+250training+4qualified | 258 | 250 | Вложенные в model отдельно не прибавлять | 250 |
| Main A+B:4updates и8scoring на плечо | 24 | 8 | Вложенные отдельно не прибавлять | 8 |
| Пять existing Facts на A и B | 56 | 8 | Вложенные отдельно не прибавлять | 0 |
| Четыре новых B-contracts, включая learned restore/oracle | 4 | 0 | 14 /4; один Forward отменён до расчёта | 0 |
| **При готовом donor** | **84** | **16** | **14 /4** | **8** |
| **Максимум при отсутствии donor** | **342** | **266** | **14 /4** | **258** |

Existing finite-difference Fact действительно делает28Forward+4Backward на stage: B sizes q4/k2/v2/o4, по два численных Forward на элемент и по одному analytic Forward/Backward на projection. Остальные четыре selected existing Facts не вызывают Model.Forward/Train. Это вывод по телам [LoraTests.cs](../../../../Tests/LoraTests.cs), не новый запуск. Metadata stamp и scalar-oracle assertions не добавляют Forward. Никаких других seeds, panels, repeats или повышения дозы.

### 7.4. Точный test map, baseline compatibility и восстановление

Learning и четыре contract Facts из §5 находятся в `LlmFromScratch.Tests.ExperimentFixes.FrozenLinearTransposeTests`; producer назначен отдельно в §4. Общий harness и test source создаются до baseline. Никаких прямых ссылок из общего source на ещё не существующие DTO/методы: узкий **lazy reflection bridge** разрешает в B только `TrainingLoop.Trainer.TrainSupervisedNextTokenSteps`, `TrainingLoop.SupervisedTokenStep` и внутренние scope seams для contract tests. A выполняет прежние четыре single-step calls, B требует batch API до обучения и вызывает его ровно один раз; отсутствие API в B —FAIL, не тихий fallback. Arm label выбирает ожидаемый source contract/output, не алгоритм learner. Tests уже имеют friend native assemblies, но это не разрешает compile-time ссылку на отсутствующий A API.

Learning exact filter: `FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.FrozenLinearTransposeTests.NativeSft_FromQualifiedS0_PreservesTrajectory`; discovery ровно1. Затем Candidate exact filters четырёх named contract Facts§5; общего class filter, повторяющего learning, нет. Existing5 запускаются ровно по названиям§5, finite-difference не добавляется вторично. Перед сравнением требуется terminal learning receipt, TRAIN2/2 до и после,4actual updates/arm, равенство trajectories и full state.

`Scope_ReusesFrozenTransposeAndPreservesInputGradient` использует данные и scalar oracle§5 в предусмотренных4primitive Forward/Backward. Для каждого Forward — независимый X с RequiresGrad=true. Два scoped графа одновременно живы; после их Forward закрыть scope и выполнить их backward в обратном порядке, проверяя неизменность сохранённого detached storage и GW. Это те же четыре Backward, без дополнительного вызова ради Dispose. В load/unfreeze Fact предусмотрены2Linear calls и **четыре** model readout, все с `training:true`, без Backward/update, после основного B receipt:

1. Загрузить независимую модель из full final B; внутри нового internal owner выполнить первый Forward на одной TRAIN строке.
2. Завершить первый owner. Только вне любого scope изменить один заранее выбранный конечный float frozen Weight у allowlisted Linear и сохранить его новые bits для проверки. Открыть новый owner с пустым кэшем, выполнить второй Forward на той же TRAIN строке и закрыть этот scope. Первый lookup каждой выбранной Linear — miss, новая entry не заимствует storage старого scope; это проверяется на тех же calls, без дополнительного Forward. Изменение базы внутри owner не выполняется и не считается поддерживаемым сценарием.
3. На тех же изменённых weights вне scope сделать третий, uncached Forward; logits должны совпасть со вторым. Это oracle свежего scope после изменения базы, не проверка обнаружения raw записи внутри серии, не пятый вызов и не требование изменения top1.
4. Вне scope восстановить тот же full final B штатным LoadCheckpoint и восстановить его optimizer/RNG state254; открыть новый scope и сделать четвёртый Forward на той же строке. Logits должны совпасть с первым; base bits — с исходным final B.

Reentrant load/mutation guard проверяется внутри первого owner до записи, без дополнительного Forward; unchanged base/checkpoint bits сравниваются напрямую. Shape/stride/reference/RequiresGrad/allowlist проверяются непосредственно на metadata entry-validation predicate без дополнительных model runs. Content-bit predicate и сценарий raw внешней записи внутри owner из этого плана исключены. Resource window уже закрыт, основной receipt не переписывается. Cancelled primitive call входит в14attempts, успешных primitive Forward13. Formatter/report/restore assertions не вызывают повторный fit.

**Промпт FIX-LFT:** Полный действующий контекст: карточка C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/55-frozen-linear-transpose-cache.md; свежее полное чтение AGENTS/CLAUDE/skills — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md (§1); роли — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md (§2); применимость и разбор падений — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткий список ниже дополнительный. Выполняется только прямо назначенный ID/этап; документальное поручение не разрешает запуск. PRIMARY, собственные примеры, данные, команды и бюджет полной карточки сохраняются. модель исполнителя — default сессии; навыки architect/phase-planning/plan-review/code-examination/test-integrity/prompt-patch-authoring/human-communication — по03. Один checkout, один выбранный LFT, один donor§4/7, русский отчёт `artifacts/fbe/FIX-LFT/attempt-0001/report.md`. Сохрани internal batch owner с исключительным владением, guards штатных entrypoints, immutable graph storage и восемь base получателей. Per-hit full-base bit scan и persistent cache не добавляй; исторический процент не обещай после включения native owner/guards. Бюджет§7.3 окончателен, producer максимум один раз250. Любое измеренное полезное улучшение признаётся по своей оси рядом с регрессиями. Жди исходный живой handle штатным wait≤60s до terminal в том же ходе; второй monitor/повторный run не создавай. Прямой STOP/NEXT немедленно прекращает старый ID, включая отчёт/commit/cleanup. Следующий ID берётся только из01, автоматического продолжения нет. Старые experiments/results не удалять.
