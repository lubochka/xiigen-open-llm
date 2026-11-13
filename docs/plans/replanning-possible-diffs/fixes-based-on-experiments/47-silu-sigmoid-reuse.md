# FIX-SSR — повторное использование знаменателя SiLU только в обучающем проходе

<a id="q34-min"></a>
## Q34. Один учебный шаг с сохранённым знаменателем SiLU

**Почему переносим.** В SILU01 время уменьшилось с 22,1668 до 19,9589 мс (−9,96%). Выделения выросли на 0,43%, CPU сохранился. На учебной паре и на двух отложенных вопросах точность осталась 1/2. Результат относится к обучающему проходу этой модели. [Отчёт SILU01](../../../../experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1/report.md).

После Q35 передать существующий признак `training` от `TransformerBlock.Forward` через `SwiGLUFeedForward` в `TensorOps.Silu`. В учебном вызове сохранить уже вычисленный знаменатель `1 + exp(-x)` до обратного прохода и использовать его в производной. В прямом проходе оставить исходное деление. Вызов с KV-кешем и прежние перегрузки используют прежний путь. Буфер принадлежит одному вычислению и освобождается вместе с его графом.

Восстановить [h64-silu/v1](../../../../experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1). Взять один урок T+ `C# result sign: 9-4 =` → `+`. Прочитать ответ до и после одного обычного SFT-шага. Проверить, что шаг дошёл до обратного прохода через сохранённое значение и завершился с конечной потерей. Записать, сохранился ли выученный знак. Не требовать нового процента ускорения или совпадения всех весов. При потере уже выученного ответа проверить время жизни буфера и передачу `training`. После целевого прогона и устранения связанного с правкой падения перейти к Q32. [Исходный опыт](../../../../experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1/report.md) · [Применимость наборов](codex-158-learning-sets-transfer-coverage-2026-11-04.md#open-transfer-data).

Ниже сохранены исходная архитектура, данные и полная методика опыта. Для нынешнего переноса объём задаёт этот раздел.

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски CORE:** Kernel parity проверить на назначенном реальном learning пути: независимые tensors/gradients/updates и ненулевой правильный readout. Отрицательный контроль обнаруживает возврат ошибки, положительный сохраняет lawful путь; isolated0→0 не качество обучения.

**Ожидания тестов:** собственные данные/positive/negative этой карточки и названный ordinary caller имеют приоритет над примерами120. Пример другого семейства не добавляет fixture, Train или новый API. Полный набор outer/nested Train/BPE/Grammar, readouts, restore и этапов остаётся в существующем бюджете. Структурный guard сTrain0 не объявляется новым обучением.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Для ресурсного переноса сравнивать actual native операции на том же обученном состоянии и в одинаковом окне. Managed allocations, peak и CPU различаются; прогрев/дрейф показываются по блокам. P08 продемонстрировал полезный gold-mass эффект при allocations+155,65%: наличие quality gain и кода в продукте не отменяет HOLD по памяти. Константа счётчика не является наблюдением.

Для FIX-SSR исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Стоимость относится к точно названному окну. Контрпример P09: разность allocations четырёх arm windows, делённая на4, включает создание входов/cache/receipts и не является постоянной стоимостью одного Forward. Для этой оптимизации указать размер работы, warmup, что внутри/снаружи счётчиков и одинаковую обвязку A/B; не закреплять точные bytes по одному такому среднему. Проверять существующие tests и дополнять их learned fixture, не объявлять отсутствующими по одному отчёту.

Применимость: FIX-SSR. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Полный ресурсный бюджет обязан включать обязательные диагностические вызовы и неудачные попытки, с отдельными единицами model/layer и optimizer-call/effective-update. Контрпример P11: 40 model Forward основной части +19 контрольных =59; 8 optimizer calls дают лишь4 изменения весов. Oracle/FD, заявленный обязательным, должен влиять на принятие результата, а не только выводиться в JSON. Равенство старому алгоритму подтверждать независимой проверкой, не комментарием в исходнике.

Применимость: FIX-SSR. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-SSR

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**M — метрика привязана к фактической стадии обучения.** Для обучающего loss/reward записать состояние, на котором он вычислен, отдельно от номера опубликованных после update весов. У DpoTrainer последняя StepMetrics — pre-update; не называть её оценкой конечного checkpoint. Пример P02: B уже стартовал с margin 0,002773 после SFT; отношение последних значений ×3,22 не равно отношению приростов DPO. Любое сравнение приобретения/ремонта использует собственное before каждого этапа и одинаковый reader. DPO Accuracy=(reward_margin>0) означает улучшение относительно reference, а не правильный ответ: T2 имеет policy chosen−rejected=−0,1827326 при положительном reward. TRAIN/EXAM correctness и сохранённые logP остаются отдельными полями. Если эта карточка сравнивает математически равные оптимизации, сохранить её A/B equality; не требовать искусственного роста точности и не добавлять post-update Forward, когда нужное чтение уже предусмотрено.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

Дата: 22 октября 2026, UTC. **Статус: ограниченный кандидат переноса, только план.** Основание — завершённый **SILU-SIGMOID-REUSE-01 / SILU01-R1**. Код и свидетельства сверены на [ebaf7760a36d43172bd88317ff469edfd09a615b](https://github.com/lubochka/llm_mvp_core/commit/ebaf7760a36d43172bd88317ff469edfd09a615b). В этой документной сессии обучение, изменение продукта и запуск тестов не выполнялись.

[Реестр и следующий ID](01-registry.md) · [состояния](02-learning-state-and-fixtures.md) · [исполнение](03-test-and-execution-contract.md#native-ab-transfer) · [доказательства и новизна](04-evidence-and-novelty.md)

## 1. Измеренный выигрыш с его ценой

В прямом проходе SiLU вычисляет знаменатель \(d=1+\exp(-x)\) и результат \(x/d\); текущий backward повторно вычисляет ту же экспоненту. SILU01 сохранил **знаменатель**, затем использовал \(1/d\) в неизменной формуле производной. Измерено меньшее время четырёх настоящих SFT-обновлений из уже обученного состояния при побитовом равенстве результатов. Цена — дополнительные **59 232 байта** выделений. Один короткий прогон не устанавливает причинный процент ускорения.

| Показатель исходного опыта | A | B | Вывод |
|---|---:|---:|---|
| Wall четырёх SFT-обновлений | 22,1668 ms | 19,9589 ms | −2,2079 ms, −9,96%; наблюдаемый полезный выигрыш |
| Выделено за TRAIN | 13 639 760 B | 13 698 992 B | **+59 232 B**, +0,43%; регрессия обязательна в отчёте |
| CPU | 15,625 ms | 15,625 ms | Равенство одного отсчёта, не точная CPU-оценка |
| GC0/1/2 | 2/0/0 | 2/0/0 | Без изменения |
| Массивы знаменателей / payload | 0 / 0 B | 4 / 59 136 B | Четыре массива по 14 784 B; заголовки добавили 96 B |
| Максимальный cache payload одного forward | 0 B | 14 784 B | Сохраняется на время жизни соответствующего графа |
| Четыре loss и 896 trainable values | — | Побитово равны A | В обеих руках обучаемое состояние действительно меняется |
| Portable optimizer | 7 384 B | 7 384 B, тот же digest | В обеих руках optimizer step 400→404 |
| 1024 BEFORE + 1024 AFTER logits | — | Побитово равны A | Те же 1/2 TRAIN и 1/2 EXAM |
| Peak working set | 55 828 480 B | 55 828 480 B | Общий максимум процесса, не отдельный peak каждой руки |

Первичные источники: [raw.json](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1/raw.json), [costs.csv](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1/costs.csv), [per-row.csv](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1/per-row.csv), [отчёт](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1/report.md), [factor.patch](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/SILU01/factor.patch). Source pin опыта: `fb9175cc752d41e1cb525e0609b7c96140940217`; tree `e448640bd1f2fe2c65adcc0b5ffcb02e7f5539e6`, 36 файлов и один фактор в TensorOps. В этой документной проверке прочитаны сами raw arms и parity.

**Почему успех допускается:** полезное измеренное уменьшение wall уже есть; рост точности для успеха по этой оси не требуется. **Почему статус ограничен:** A, затем B выполнены в одном процессе, порядок фиксирован, повторов нет. Разница 2,2079 ms на 14 784 убранных Exp даёт около 149 ns/Exp; сам отчёт признаёт это завышенной оценкой механической цены. Нельзя заявить устойчивые −9,96%, ускорение inference или перенос на все модели. Математическая опора — сохранение forward/backward при установленном времени жизни входа.

Старт исторически подготовлен один раз за 400 шагов: приобрёлся правильный ответ T+, до этого обе TRAIN-строки были неверны. После четырёх новых обновлений дополнительного усвоенного ответа нет; сохраняются 1/2 TRAIN и 1/2 EXAM. Это не доказывает общее правило знака, но сравнение имеет настоящее обученное «до» и реальное продолжение обучения.

**Новизна:** FIX-ATS/45 меняет адресацию attention, FIX-RRD/44 — SumAlongAxis, FIX-RMS/30 — MultiplyBroadcast. Здесь меняется вычисление производной нелинейности и время жизни её временного массива. Соседние kernel factors не включаются в эту A/B-пару; их проценты не суммируются.

## 2. Точный native recipient и отличие от source-copy

В исходном [RunArm, строки 762–814](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/SILU01/Program.cs#L762-L814) экспериментальный переключатель включался **только на четырёх training updates B**. На BEFORE и AFTER readout он был выключен у обеих рук. При этом raw содержит `silu_graph_nodes=4` на каждой панели readout: `RequiresGrad=true` бывает и при чтении модели с прикреплёнными LoRA.

Следовательно, правило «кэшировать всегда при x.RequiresGrad» расширило бы расход памяти inference за пределы измеренного опыта. **Выбран явный training-only opt-in** через существующий флаг `training`. Старые публичные перегрузки и inference с KV cache остаются без этого кэша. Это план переноса измеренного фактора на настоящий consumer, а не новый измеренный результат. Сохранение отсутствия denominator array не обещает полного равенства native allocations старого и нового overload: расходы closure/вызова также попадают в отдельные readout metrics.

| Файл и существующий участок | Будущее изменение |
|---|---|
| [00.TensorCore/Core/TensorOps.cs:1129–1163](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/00.TensorCore/Core/TensorOps.cs#L1129-L1163) | Добавить `public static Tensor Silu(Tensor x, bool cacheBackwardDenominator, CancellationToken cancellationToken)`. Старые `Silu(x)` и `Silu(x, token)` делегируют с `false`. Массив создаётся только при флаге `true` **и** `x.RequiresGrad` |
| [03.TransformerModel/Layers/SwiGLUFeedForward.cs:37–53](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/03.TransformerModel/Layers/SwiGLUFeedForward.cs#L37-L53) | Добавить `Forward(Tensor x, bool training, CancellationToken cancellationToken)`. Он передаёт `training` в новый SiLU opt-in. Обе прежние перегрузки делегируют с `false` |
| [03.TransformerModel/TransformerBlock.cs:46–65](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/03.TransformerModel/TransformerBlock.cs#L46-L65) | В обычном Forward передать уже имеющийся `training` в новую перегрузку FFN. Меняется только этот вызов |
| [TransformerBlock.ForwardWithCache:68–95](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/03.TransformerModel/TransformerBlock.cs#L68-L95) | Сохранить вызов прежней FFN-перегрузки без кэша знаменателей |
| [LlmModel.cs:219–259](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/03.TransformerModel/LlmModel.cs#L219-L259) | Forward и ForwardHidden уже передают `training` в блок; product edit не нужен |
| [Trainer.cs:67–142](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/04.TrainingLoop/Trainer.cs#L67-L142) | Consumer вызывает `model.Forward(training: true)`, затем BackwardLoss и только после него optimizer.Step; product edit не нужен |

Разрешены **ровно три product paths**: TensorOps.cs, SwiGLUFeedForward.cs, TransformerBlock.cs. Конструктор `SwiGLUFeedForward(ModelConfig cfg)` и его регистрация gate/up/down modules сохраняются. Не добавляются поля ModelConfig, checkpoint schema, параметры модели, global switch, AsyncLocal mode или новый model registry. Флаг обозначает фазу native вызова, а не руку, gold, domain или результат теста; он не выбирается по `XIIGEN_FBE_ARM`.

В новой SiLU-перегрузке сохранить отдельный локальный `float[]? denominators` для каждого result. При выключенном флаге или `RequiresGrad=false` выполняется прежний путь. При включённом кэше:

1. В прежнем цикле вычислить `float d = 1f + MathF.Exp(-x.Data[i])`, затем именно `x.Data[i] / d`, и сохранить `d`. Не заменять деление умножением на sigmoid.
2. Closure этого result читает собственный массив: `sigmoid = 1f / denominators[i]`.
3. Сохранить порядок `upstream * sigmoid * (1f + x.Data[i] * (1f - sigmoid))`, Children/RequiresGrad, AccumulateGrad, форму и cancellation checks.
4. После освобождения графа массив не удерживается постоянным хранилищем. Контракт opt-in требует неизменности входного activation между forward и backward. Array pool, другая формула SiLU и повторное обучение стартового состояния в этот фактор не входят.

Обычные training-вызовы других learners также проходят через этот блок, поэтому математический контракт важен шире SFT. Однако DPO-расходы и качество других доменов здесь не измерены: карточка не объявляет для них процент выигрыша и не запускает их автоматически. Приоритет и Next определяет только [реестр 01](01-registry.md).

## 3. Производная, время жизни входа и floating point

Для неизменённого \(x\):

\[
d(x)=1+\exp(-x),\quad s(x)=1/d(x),\quad
f(x)=x/d(x),\quad f'(x)=s(x)\,[1+x(1-s(x))].
\]

Сохранённый в forward **d** равен float-знаменателю, который исходный backward повторно вычислял для того же x. Остальные операции и их порядок сохраняются; побитовое равенство измерено на runtime опыта. Восстановление sigmoid как `(x/d)/x` при x=0 даёт 0/0, а замена forward на `x*s` может изменить округление. Обе замены исключены.

Общая опора chain rule: [Proposition 9.4](https://github.com/lubochka/xiigen-planning/blob/d7b1704fd681fe0855afeb08ca5234433b5f9f6c/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0312-0317-ch09-motor-taxonomy-and-related-work.md#proposition-9-4), [Proposition 9.6 и FFN pullback](https://github.com/lubochka/xiigen-planning/blob/d7b1704fd681fe0855afeb08ca5234433b5f9f6c/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#proposition-9-6), [заметка 87 о производной smooth activation](https://github.com/lubochka/xiigen-planning/blob/d7b1704fd681fe0855afeb08ca5234433b5f9f6c/llm%20mvp/math-theory/giansalvo-transformer-vol1/notes/87-feedforward-gates-and-layernorm-motors.md). Формула SiLU выше выведена здесь самостоятельно. Эти источники не доказывают runtime-ускорение и не позволяют заменить производную SiLU бинарной ReLU-маской.

### Почему нельзя молча предположить неизменность input для любого caller

[Tensor.Data](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/00.TensorCore/Core/Tensor.cs#L15-L22) возвращает изменяемый массив. При его изменении между forward и backward сохранённый d будет от старого x, а другой множитель формулы — от нового. Доказательство эквивалентности тогда не действует.

- Старые перегрузки сохраняют прежнее чтение актуального input при backward. Это ограниченная совместимость mutable API, а не утверждение о математической корректности такого изменённого графа.
- Новый opt-in требует стабильного входа. Выбранный native путь использует свежий tensor проекции gate; последующие Multiply и Linear не меняют его Data. [Trainer:117–135](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/04.TrainingLoop/Trainer.cs#L117-L135) вызывает backward до optimizer.Step. Это проверяемое условие данного синхронного пути, не гарантия для любого caller.
- Тест двух одновременно живых графов в §5 защищает от общего массива и чужого denominator. Static cache, dictionary по Tensor и pooling здесь не вводятся.
- Если при native review обнаружен новый caller с `training=true`, изменяющий вход до backward, его нельзя скрыто включить в этот перенос. Для него оставляется прежний режим; точный call site записывается как ограничение интеграции. Основной Trainer не заменяется копией learner.

Цена кэша — \(4N\) payload bytes на живой activation из N элементов плюс заголовок массива. В опыте N=21×176=3696: 14 784 B на forward; четыре updates дают 59 136 B payload и 96 B заголовков. Убирается одна Exp на элемент backward. Forward Exp остаётся; чтение без backward не получает от этого математической экономии.

## 4. Готовое S0, данные и native восстановление

Единственный donor: [`experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1`](https://github.com/lubochka/llm_mvp_core/tree/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1). Контракты: [bundle](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1/bundle.json), [generation](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1/generation.json), [data.material](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1/data.material.json), [authority](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1/authority.json).

| Payload | Bytes | SHA-256 |
|---|---:|---|
| S0.weights.bin | 320620 | `8721dd57b11fa61b11acf37c337611129891a1d3096ae0392a89455d6d58fdec` |
| S0.optimizer.bin | 7384 | `539d11e925bbff676d9e5969d8c7bfb4326ff93aa1b93f934e8ff9bbbafc15d0` |
| S0.rng.json | 765 | `13237c7cb87e6d20ddb3df4115c76cb2fd516ea310c06e328e1d0a4e0c5eb781` |
| data.source.jsonl | 126 | `7c05b6c5cf13f3596c09343236d2f8da41d03fbd17c4ec81a4307f9f6fe50f79` |

Bundle digest: `e2119091142292b32aa457e7ac6691302b55e38f9f0ca97ecb8bde07a2bab4ee`. Topology: `c8d4a2f9fc2a338c39618bd8f76146c799d99c5385b2e2629dce1bb2d19403e2`; LoRA identity: `53ac9cf859c32cef6d478e08a9fc4f7ab4258700eb3208a77463f214ace02ee4`. Stream называется **`frozen-transpose-probe-v1`** — это фактическое имя именно этого family. Не переименовывать его и не заменять соседним attention-transpose donor: совпадение весов не означает совпадения identities.

Loader повторяет только восстановление из [Program.cs:716–759](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/SILU01/Program.cs#L716-L759):

- `LlmModel` под base seed 62001; vocab=256, context=64, hidden=64, layers=1, heads=4, KV heads=2, FFN=176, RoPE=10000, RMS ε=1e−5.
- q/v LoRA: rank=4, alpha=8, dropout=0, seed=62002, названный stream. `LoraApplicator.ApplySeeded` выполняется **до** LoadCheckpoint; загрузка весов не создаёт топологию.
- Четыре обучаемых массива и двенадцать frozen. Bindings — `model.GetCheckpointParameterEntries().Select(entry => new AdamWParameterBinding(entry.Key, entry.Parameter))`, без угадывания порядка.
- AdamW: β1=.9, β2=.95, weightDecay=0, eps=1e−8; portable import должен вернуть true, сохранённый step=400. RNG восстанавливается с точными topology/LoRA identities.
- Trainer: batch=1, seq=64, warmup=0, total=400, maxLR=minLR=.001, gradClip=1, gradAccum=1, weightDecay=0. TotalSteps=400 сохранён из опыта намеренно; постоянный LR исключает дрейф расписания при продолжении optimizer step 400→404.

| Роль | ID | Полный prompt | Ответ / byte ID |
|---|---|---|---|
| TRAIN | T+ | `C# result sign: 9-4 =` | `+` / 43 |
| TRAIN | T- | `C# result sign: 4-9 =` | `-` / 45 |
| EXAM | E+ | `C# result sign: 8-3 =` | `+` / 43 |
| EXAM | E- | `C# result sign: 3-8 =` | `-` / 45 |

TRAIN sequence — prompt+answer, UTF-8 byte IDs без BPE, EOS и добавленного разделителя: 22 tokens, 21 shifted target. Порядок четырёх обновлений: T+, T-, T+, T-; weight=1. EXAM не обучается и не выбирает checkpoint или число шагов. Будущий паспорт `Tests/Fixtures/ExperimentFixes/v1/SiluSigmoidReuse01/manifest.json` фиксирует все inputs/config/gold и ссылки на исходные байты; бинарники не дублируются.

До первого model call проверяются payload/hash, отсутствие LFS pointer вместо данных, named topology, portable import и RNG identity. При отсутствующем входе назвать точный файл и остановить эту руку **до обучения**. Нельзя запускать 400-step producer или использовать другой family. A и B загружают собственные экземпляры из одного S0; историческая подготовка сейчас стоит ноль обновлений.

## 5. Расширенные проверки из исходного плана

Следующие файлы **запланированы и пока не реализованы этой карточкой**:

| Path | Назначение |
|---|---|
| `Tests/ExperimentFixes/SiluReuseContractTests.cs` | Два legacy Facts и пять candidate-only Facts |
| `Tests/ExperimentFixes/SiluReuseLearningTests.cs` | Один Fact `RestoredSft_TrainingOnlyCachePreservesState` |
| `Tests/ExperimentFixes/SiluReuseFixture.cs` | Native S0 loader, фиксированная панель, состояния и receipts |
| `Tests/Fixtures/ExperimentFixes/v1/SiluSigmoidReuse01/manifest.json` | Хеши, конфигурация, независимый gold |
| `Tests.Support/ExperimentTransfer/Run-NativeAB.ps1` / `Compare-NativeAB.ps1` | Расширение будущего общего runner для FIX-SSR; это не learner |

Namespace: `LlmFromScratch.Tests.ExperimentFixes`. **Общий test harness должен компилироваться на A**, где новых перегрузок ещё нет. Candidate-only Facts получают MethodInfo лениво **внутри выбранного Fact** по точным сигнатурам из §2. Нельзя ссылаться на новую перегрузку статически или вызывать поиск в type initializer при discovery A. На B отсутствие нужного метода — ошибка, не Skip. Reflection только вызывает реальную native перегрузку из её assembly; основной learning Fact использует прежний Trainer API.

### Два legacy Facts в обеих руках

Во всех gradient cases входные Tensor создаются с `requiresGrad: true`; исключение явно указано в FrozenAndFalseModes. Для обеих перегрузок используются отдельные входы и графы, чтобы накопленный Grad не влиял на сравнение.

- `LegacyOverloads_MatchScalarForwardAndDerivative`: обе прежние перегрузки SiLU; x=[−8, −2, −.5, 0, .5, 2, 8], upstream=[1, −2, .25, 3, −.75, 2, −1]. Независимые скалярные f и f′, форма и pullback через public `Backward(Tensor)`. Expected forward вычисляется делением, а не x·sigmoid.
- `LegacyOverloads_PreserveLiveInputBackwardCompatibility`: старый forward на x=[−1, 0, 1], затем явное изменение x.Data до backward. Проверить прежнюю производную от актуального x. Это тест совместимости старого mutable API, **не** доказательство корректности изменённого графа. Новый training opt-in здесь не вызывается.

### Пять candidate-only Facts

1. `TrainingCache_MatchesIndependentDerivative`: новая сигнатура с `cacheBackwardDenominator=true`, те же конечные inputs/upstream. Сравнить pullback с независимым scalar gold и центральной конечной разностью double f при ε=1e−4, absolute gradient tolerance=2e−3. Forward bits и backward на неизменённом входе должны совпасть со старым API. Отрицательные значения и нуль сохраняются в наборе.
2. `TrainingCache_BelongsToEachForwardGraph`: два живых графа на x1=[−2, 0, 2] и x2=[3, −1, .5]; backward в обратном порядке с собственным upstream каждого. Каждый результат равен своему независимому gold. Это проверяет срок жизни и отсутствие общего cache.
3. `FrozenAndFalseModes_PreserveLegacyBehavior`: flag=false при RequiresGrad=true; flag=true при RequiresGrad=false. Forward равен legacy, RequiresGrad соответствует входу, frozen input не получает gradient. Source review подтверждает отсутствие denominator array в обоих выключенных режимах. Этот контракт не выдаётся за resource benchmark.
4. `TrainingCache_PreCancellationPreservesInput`: token отменён до операции; исходные bytes неизменны, result не возвращён. Старую частоту проверок в forward сохранить; нового cancellable backward API не вводить.
5. `SwiGlu_TrainingOnlyOverloadPreservesForwardAndBackward`: один native `SwiGLUFeedForward(new ModelConfig { HiddenDim = 4, FfnHiddenDim = 8 })`, созданный внутри `Initializers.UseDeterministicSeed(62003UL)`; два раздельных input tensor формы [1,2,4] с данными [−1, −.5, 0, .5, 1, 1.5, −2, 2] и upstream [.5, −1, 1.5, 2, −.25, .75, −.5, 1]. Прежняя перегрузка (false) и новая с training=true дают одинаковые forward values, input gradients и parameter gradients после раздельного ZeroGrad. Не использовать один input с накопленным Grad. Это два чистых FFN forward/backward, без optimizer и fit. Тип должен принадлежать native TransformerModel, а не копии из test project.

**Основной learning Fact:** native Trainer + LlmModel + TensorOps + LoRA; один restore на руку, четыре BEFORE readouts с `model.Forward(training: false)`, четыре SFT-обновления с `training: true` через Trainer, четыре AFTER readouts с false. B выбирает новый путь самим native training flag; экспериментального переключателя нет. Сохранить 16×256 итоговых logits обеих рук, loss/rank/exact всех четырёх строк, четыре training losses на руку, final weights/optimizer/RNG, TRAIN wall/CPU/allocations/GC и отдельные readout resources.

Главные контракты: уже усвоенный ненулевой T+ до updates, действительная запись trainable parameters, четыре updates и optimizer step=404, frozen без изменений, побитовое равенство A/B outputs и состояния при одинаковом runtime/ISA. Gold 43/45 не заменяется фактическим top-1. Одинаковые 1/2 не называются изученным правилом знака. Нельзя требовать снижения allocations: измеренная цена кэша положительна. Будущие native ресурсы заранее не обещаются; единственный speed run не становится статистически устойчивым эффектом.

**Бюджет:** две руки × четыре SFT = **8 updates, 168 shifted targets**, 16 основных readout forwards; producer=0, DPO=0, teacher=0. Candidate-only contracts не вызывают Trainer/optimizer; SwiGLU guard делает только два FFN forward/backward без изменения весов. Existing guard из §6 также не обучает модель. A: 4 test executions = 1 existing + 2 legacy + 1 learning. B: 9 = 1 existing + 2 legacy + 5 candidate + 1 learning. Итого **13 executions в шести процессах** (existing/contracts/learning на руку), две Rebuild/discovery. Readout metrics не расширяют training budget.

Terminal receipts, final weights, portable optimizer и RNG сохраняются в `artifacts/fbe/FIX-SSR/attempt-0001/learning/{A|B}/state/`. Compare и отчёт читают эти данные без нового fit. Donor не перезаписывается. Последующее E2E использует сохранённое состояние и отдельно назначенный readout, а не повторяет 400-step подготовку.

## 6. Существующий тест и адресный ремонт падений

Свежих результатов тестов нет. Существующий consumer smoke — [Tests/TransformerModelTests.cs:352–362](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/Tests/TransformerModelTests.cs#L352-L362), `LlmFromScratch.Tests.TransformerModelTests.TransformerBlock_Forward_OutputShape_Unchanged`. Он вызывает старый `block.Forward(x)`, то есть training=false. **Expected shape сохраняется**; Fact выполняется до и после будущего изменения. В прочитанном TensorCoreTests.cs специального SiLU gradient coverage нет; его добавляют новые Facts, а не замена unrelated assertions.

| Возможное будущее падение | Адресный ремонт | Недопустимая подмена |
|---|---|---|
| Неверная производная на zero/negative | В TensorOps восстановить denominator, формулу f′ и порядок операций; проверить отсутствие s=f/x | Удалить нуль, взять expected из actual, без основания ослабить tolerance |
| Legacy/inference получил cache или изменённый output | Старые SiLU/FFN перегрузки передают false; ForwardWithCache сохраняет прежний вызов | Считать RequiresGrad эквивалентом training, убрать negative guard |
| Training consumer не вызывает новый overload | Передать существующий training из TransformerBlock в FFN и SiLU; проверить B-DLL после Rebuild | Вызвать копию SiLU в тесте или product switch по arm |
| Два графа влияют друг на друга | Локальный массив в closure каждого result; исправить время жизни | Static cache, pooling или подмена состояния reflection |
| Restore отказал либо дал другой S0 | Точный family, topology, RNG, LoRA до загрузки и portable AdamW mapping | Повторить 400 steps или взять похожий donor |
| Speed gain не повторился | Записать native результат со всеми метриками; сохранить ограниченный статус исторического успеха | Повторы сверх бюджета или объявление паритета качества ускорением |

Сборочная предпосылка [02§10](02-learning-state-and-fixtures.md#native-build-prerequisite) выполняется до первого Tests build/discovery одинаково для A/B. Ссылка Tests→35 подтверждена исходниками, но не является свежим сообщением компилятора. Архивный Rp01Micro исключается из 35 только по описанному там условному Compile Remove; Tests и native компоненты не исключаются, весь solution ради этого ID не ремонтируется.

## 7. Исходное двухстадийное сравнение

Рабочий корень — **`C:/Projects/llm_mvp_core`**, ветка — `codex/dpo-implementation`. Применяется [lifecycle 03§9](03-test-and-execution-contract.md#native-ab-transfer). Allowlist для FIX-SSR содержит **три product files из §2**: прежнего TensorOps-only правила недостаточно для выбранного native wiring.

**Baseline:** общий harness commit без product edit → Rebuild Tests Release → discovery четырёх выбранных случаев → existing A → legacy contracts A → learning A → terminal baseline-stage. **Candidate:** после завершения A — три назначенных product изменения, review и commit → Rebuild → discovery девяти случаев → existing B → legacy/candidate contracts B → learning B → comparison. Другие планы, families и исторический SILU01 host не запускаются.

Команды после реализации этих **будущих** helpers:

```powershell
Set-Location C:/Projects/llm_mvp_core
$ssrBaselineCommit = (git rev-parse HEAD).Trim()
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-SSR -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $ssrBaselineCommit
```

После полностью завершённого A и отдельно назначенного product diff:

```powershell
$ssrCandidateCommit = (git rev-parse HEAD).Trim()
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-SSR -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $ssrBaselineCommit -CandidateCommit $ssrCandidateCommit
```

Runner пока **запланирован**. Для FIX-SSR он назначает exact existing FQN из §6; два legacy FQN из §5 с prefix `LlmFromScratch.Tests.ExperimentFixes.SiluReuseContractTests.`; в B добавляет пять названных candidate Facts. Main filter: `FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.SiluReuseLearningTests.RestoredSft_TrainingOnlyCachePreservesState`. Каждый процесс вызывает `dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter <назначенный-filter>` с отдельным TRX/results path. Discovery сверяет точные методы; ноль обнаруженных тестов — ошибка. На A новая перегрузка не требуется ни при компиляции, ни при статической инициализации discovery.

Output root: `artifacts/fbe/FIX-SSR/attempt-0001`; отдельные каталоги `baseline/A`, `contracts/A`, `learning/A`, `after/B`, `contracts/B`, `learning/B`. Baseline intent создаётся через CreateNew с `candidateCommit:null`; B пишет отдельный candidate-binding с digest terminal A. Фиксируются canonical HEAD/ancestry, common input hashes, native assembly names/locations/SHA/MVID для TensorCore, TransformerModel, TrainingLoop, LoRA, а также runtime/ISA/config. Helpers и fixed inputs между руками не меняются. Совпадение пути DLL не подтверждает загрузку B-кода.

Completed A — вход B, не причина повторить A. Worktree, checkout/reset/stash и чужой cleanup не используются. Занятый attempt сначала читается; его не удаляют и не увеличивают автоматически. Каждый finally сохраняет actual counters, not_reached и failure до throw. Deadline — 5 минут на test process и 15 минут на Rebuild, без автоматического replay. Живой run наблюдается по исходному handle. Compare/report допускают исправление представления тех же raw с явной новой ревизией summary, без нового обучения.

## 8. Историческая постановка полного сравнения

Фактор работает внутри получателя обучения; выбор domain/flow/Unit принадлежит [FIX-E2E](20-prompt-domain-e2e.md). Новый training bool не получает semantic route authority. Назначенное E2E использует то же сохранённое обученное состояние и native identity, передаёт настоящий prompt правильному домену и ссылается на SSR receipt как на свойство kernel в его учебном пути. Четыре прямых model readouts не объявляются полной цепочкой prompt→domain. Дополнительное обучение router и автоматическое включение всех доменов здесь не назначены.

> Исторический текст от 02.11.2026. Действующий объём дан в [Q34](#q34-min). Ранее требовалось: прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/47-silu-sigmoid-reuse.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.
>
> **Будущее задание: только FIX-SSR после отдельного назначения внедрения.** Прочитай AGENTS.md, CLAUDE.md, эту карточку, 03§1–9 и 02§10. Skills: docs/skills/architect-behavior-classifier-SKILL.md, trainable-architecture-decision-SKILL.md, intent-to-plan-SKILL.md, phase-planning-SKILL.md, plan-review-SKILL.md, generated-code-review-SKILL.md, test-integrity-SKILL.md, prompt-patch-authoring-SKILL.md, human-communication-GLOSSARY.md. Модель исполнителя — фактическая текущая модель сессии по умолчанию, без override.
>
> NAMED_CHECK SSR-TRAINING-SCOPE: WRONG — cache только по RequiresGrad или замена division на x·sigmoid; CORRECT — training-only opt-in с native caller, локальным denominator и старыми uncached перегрузками. Бюджет — ровно 8 SFT и 16 основных readouts, producer=0. Используй h64-silu/v1 без bootstrap. Русские report.md и report-for-user.md, comparison.json и status.json пишутся в точный output root §7. Покажи speed observation рядом с дополнительными allocations и всеми ошибками EXAM. Любое измеренное улучшение полезного показателя считается успехом по этой оси и профилю; остальные изменения, включая ухудшения, обязательны. Это не автоматический ProductReady.
>
> CODEX с astra 6 ultracode выполняет роль человека в выборе и оценку качества DPO; внешняя открытая китайская модель генерирует DPO только при отдельной потребности. Здесь новых пар нет. Для одного запуска сохраняй один handle: exec_command.session_id → write_stdin с пустыми chars; yielded functions.exec.cell_id → functions.wait; другие tools — по их реальной схеме. Ожидай порциями ≤60 s до terminal в том же ходе; не заканчивай ответом «жду», не делай blind polling и не запускай вторую копию. Точный процесс и его terminal receipt важнее предположения об отказе по отсутствующему файлу.
>
> Прямой STOP/NEXT немедленно прекращает работу над прежним ID, включая отчёт, commit и cleanup. Следующий ID берётся из текущей строки FIX-SSR в реестре 01, а не из старого порядкового номера опыта. Не продолжать предыдущий эксперимент после такого указания. Existing results не удалять. Документная публикация не назначает product integration, новые прогоны или broad CI.

Первоначальный расширенный план требовал точного native training-only пути, проверенного времени жизни input, численного parity, реального optimizer step 400→404, сохранения старых API и readout behavior, а также полной таблицы ресурсов. Оно не объявляет, что модель выучила арифметику или что единичный процент ускорения гарантированно перенесётся в production.
