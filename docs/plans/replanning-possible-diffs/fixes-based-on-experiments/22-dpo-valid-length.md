# FIX-PD — исполнять exact DPO сторону до её действительной длины

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q25](01-registry.md#work-fix-pd). Для общего включения действует измеренное ограничение этой строки. Возврат к работе — по записанному условию для её домена. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

## Уточнение допуска по показателям — 3 ноября 2026

**FIX-PD: Приостановить общий перенос.** Вошёл из-за уменьшения работы и выделений. Пик памяти превышает допустимый регресс; выигрыш времени его не отменяет.

- Точность: Учебные ответы 33/33 и 69/69 сохранены в A и B. Свободная генерация не измерялась.
- Полезный результат: Выделения 4 455 460 456 → 4 110 381 856 Б (−7,75%); время 36 605 → 17 934 мс (−51,01%).
- Цена и ограничения: Пик памяти 1 348 935 680 → 1 714 892 800 Б (+27,13%). Конечные веса не побитово равны; NLL немного хуже.
- [Полное сравнение и статус кода](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#benefit-fix-pd).

Этот вывод относится к названным ID, а не ко всем механизмам документа. Старый текст плана и опыта сохранён как доказательство, но его общий статус «к переносу / разрешён» не переопределяет это уточнение. Ближайшее действие — разбор уже сохранённых данных и конкретного регресса; новое обучение, повтор PRIMARY и переключение product default этим дополнением не назначаются. Для отдельного логического ремонта назвать нарушенный контракт и сохранить отрицательные контроли, а не переносить весь эксперимент под словом «bugfix».


<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски LEARN/TOK:** Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно. Разделить unknown ID, EOS и допустимость UTF-8 bytes. Lawful многобайтовый поток сохраняется, неизвестный ID/незавершённый terminal не исчезает молча; проверить ordinary caller и обе назначенные greedy/stochastic ветви, а не только новый overload.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

В учебной проверке actual logits/vector/mask от восстановленного состояния должны попадать в назначенный trainer/reader; canned tensor допускается только в отдельно названном component test. Нулевой optimizer step delta и неизменные веса не доказывают отсутствие Backward. Квитанция различает measured/structural/unknown; эту границу P08 consumer уже пришлось исправлять. Предусмотренное контрактом чтение reference cache или выученной памяти разрешено: проверяется фактический hit/miss и привязка к правильному state/key. Запрет касается подмены проверяемого производителя заранее подготовленным oracle, а не самого механизма FIX-LC.

Для FIX-PD исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Для метрики DPO указывать единицу pair/step/panel, веса и фазу. Контрпример одного шага с margins [+1,-3]: доля положительных пар 0,5, признак положительного среднего 0; совпадение этих счётов на исходном опыте не делает их одной метрикой. Проверка вызывает полный writer с metric_definitions, а не только внутренний builder. Неизмеренное время SFT — null, не0; исторические receipts не переписывать.

Применимость: FIX-PD. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Проверять по отдельности optimizer step, scheduler step и число реально применённых обновлений. Контрпример P12: импортированные моменты AdamW400 совместимы с новой эпохой scheduler0; P10, напротив, использует fresh DPO0. Не переносить правило fresh/import из соседнего опыта. После отказа сверять веса, моменты, RNG и исполненные Forward/Backward; отчёт о правильных ответах без назначенного обучения не закрывает учебную цель.

Применимость: FIX-PD. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

**Дополнение P07, 27 октября:** [83 — связанный learned-BPE тест](83-learned-bpe-boundary-transfer-addendum.md) принадлежит этому владельцу. Existing segmentation test уже обучает BPE; усилить его независимыми source bytes, точной маской и causal target row на одном snapshot, затем restore. Это отдельная малая tokenizer fixture, не повтор H512/PD01. Product CreateBatch/Validate уже защищают separate encoding и exact masks; новая runtime-проверка не назначена. ASCII результат P07 положителен для сохранения target; новые RU/HE guards ещё NOT_RUN. Основания и ограничения — [82](82-positive-transfer-delta-audit-2026-10-27.md).

<!-- BEGIN audit-79-followup -->
## Уточнение после последующих исправлений и P03–P05

При переносе valid-length сохранить response targets и отдельно контракт генерации: scored positions, produced token IDs, stop reason и полный правильный ответ не взаимозаменяемы. Контроль P05: однознаковый donor нельзя оценить принудительным8-token продолжением. Равенство mask/score не доказывает EOS/consumer. Для sum/mean reference использует ту же нормировку, derivative — тот же denominator; не чинить objective ради одного сырого length penalty.

Основания — [79](79-followup-transfers-and-learning-audit-2026-10-27.md), адресный план — [80](80-followup-repair-plan-2026-10-27.md). Это изменение требований и статуса по прочитанным источникам; неприменённые проверки не объявляются пройденными. Собственные бюджеты и исторические результаты карточки сохраняются.
<!-- END audit-79-followup -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-PD

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** LEARN/TOK по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**M — метрика привязана к фактической стадии обучения.** Для обучающего loss/reward записать состояние, на котором он вычислен, отдельно от номера опубликованных после update весов. У DpoTrainer последняя StepMetrics — pre-update; не называть её оценкой конечного checkpoint. Пример P02: B уже стартовал с margin 0,002773 после SFT; отношение последних значений ×3,22 не равно отношению приростов DPO. Любое сравнение приобретения/ремонта использует собственное before каждого этапа и одинаковый reader. DPO Accuracy=(reward_margin>0) означает улучшение относительно reference, а не правильный ответ: T2 имеет policy chosen−rejected=−0,1827326 при положительном reward. TRAIN/EXAM correctness и сохранённые logP остаются отдельными полями. Если эта карточка сравнивает математически равные оптимизации, сохранить её A/B equality; не требовать искусственного роста точности и не добавлять post-update Forward, когда нужное чтение уже предусмотрено.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Контроль достигнутого этапа, 27 октября 2026.** Для назначенных двух StepVector/arm отдельно сохранять attempted и completed. Отказ до первого native update не может получить label «после двух DPO», completed2 или measured after; после одного успешного update completed1, после следующего отказа — partial. Проверять это в общем контроллере вызова/receipt с подставным throwing callback, а не только сериализацией заранее составленного JSON: ограничение аналогично [68, U8](68-transfer-repair-followup-plan-2026-10-27.md). Эти contract-подслучаи не выполняют модель и не прибавляют третьего update. Валидное learning сравнение по-прежнему требует обоих actual updates, обученного S0, fresh DPO0→2 и заданного dropout0; nonzero-dropout continuation остаётся отдельным padded контрактом.

Статус: **новый план ограниченного кандидата; внедрение не назначено**. Дополнение от 2026-10-21 UTC. Основание — `E-DPO-VALID-LENGTH` / PD01, `attempt-0001`, код и raw на `4e993a33c9396f155a4aaeb42170e0251f34aa77`. Старый пакет `8ea3e78c039971288b12ecf2657c8e9f6804e98a` не заменяется. Новые build/test/Train здесь не выполнялись.

Обязательны [реестр](01-registry.md), [fixture/state](02-learning-state-and-fixtures.md), [контракт исполнения](03-test-and-execution-contract.md). Следующий ID по предлагаемому порядку — **FIX-LC**; перенос LC не следует автоматически из назначения PD.

## 1. Что улучшилось и для какого обучения

Тема — документация кода; профиль — описание вычитания аргументов C#-метода английским предложением, с разными длинами chosen/rejected. Получатель — q/v LoRA `rp01-csharp-small/v1`, H512. Настоящие DPO forward/backward исполняют замаскированный конец короткой стороны на общей длине обеих сторон. Опыт убрал только этот конец после проверки exact carrier и сохранил измеренный учебный сигнал в численной погрешности.

Первичные [report](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/PD01/results/attempt-0001/report.md), [pair-summary](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/PD01/results/attempt-0001/pair-summary.json), [A metrics](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/PD01/results/attempt-0001/A/metrics.json), [B metrics](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/PD01/results/attempt-0001/B/metrics.json), [inputs](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/PD01/results/attempt-0001/inputs.json). Это прочитанные измерения, не новый результат плана.

| Измерение | A padded | B valid length | Выигрыш или ограничение |
|---|---:|---:|---|
| PAD1 chosen/rejected execution length | 148/148 | 124/148 | Убран24-token хвост chosen |
| PAD2 chosen/rejected execution length | 160/160 | 160/124 | Убран36-token хвост rejected |
| Side-token positions одного предъявления двух пар | 616 | 556 | −60, или9.7402597%; **не сумма всех Forward** |
| Выделенные bytes в учебной фазе | 4455460456 | 4110381856 | −345078600, или7.7450715% |
| Wall time | 36605.1957 мс | 17933.5587 мс | −51.008%; причинное устойчивое ускорение не установлено |
| Peak working set | 1348935680 | 1714892800 | **+365957120 байт**, около27.129% |
| CPU time | 43578.125 мс | 26765.625 мс | Одиночное наблюдение, не гарантированный процент |
| PAD1 TRAIN correct до→после | 33/33→33/33 | 33/33→33/33 | Обученное состояние удержано |
| PAD2 TRAIN correct до→после | 69/69→69/69 | 69/69→69/69 | Обученное состояние удержано |
| PAD1 response NLL до→после | 0.00018515321→0.0001853122 | то же | Небольшое ухудшение в обоих |
| PAD2 response NLL до→после | 0.00027638255→0.00027691975 | то же | Небольшое ухудшение в обоих |

На шаге1 loss A5.8157034/B5.8157063, margin A−4.404846/B−4.404848. На шаге2 loss5.563154e−13 и margin28.492544 совпали. Active logits и нормы градиентов16bindings уложились в заранее заданную границу `abs(a-b)<=1e-6+1e-4*max(abs(a),abs(b))`. L2 изменения adapter weights: A0.003873390491646422/B0.0038733904915202923; max|delta| одинаков1.6927719116210938e−05. **Финальные веса не равны побитово.** Допуск численной эквивалентности шага не ослабляет SHA-проверки сериализации и загрузки.

Исторически каждый arm работал отдельным процессом, порядок A→B, без перестановки/прогрева/повторов. Не измерены held-out EXAM, свободная генерация, batch>1, nonzero dropout, отдельная целая пара с равными длинами. Полная длинная сторона каждого measured pair — только локальный отрицательный контроль. Любой полезный измеренный ресурсный выигрыш признаётся без требования роста accuracy; регрессия peak остаётся рядом.

## 2. Где менять продукт, сохранив смысл DPO

В [PD01 B DpoTrainer:1041–1084](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/PD01/B/DpoTrainer.cs#L1041-L1084) менялись два wrapper и два prefix helper. [Bridge:487–585](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/PD01/Common/ContinuousLearning/PD01ExperimentBridge.cs#L487-L585) загружает обученный S0, создаёт native recipient/profile/admission, затем выполняет `StepVector`; это real optimizer updates, не измерение отдельно нарезанных массивов.

| Native место | План изменения |
|---|---|
| [DpoTrainer.CaptureBatch:991–1001](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/11.DPO/DpoTrainer.cs#L991-L1001) | Проверять исходный полный exact carrier, capacity и execution profile как сейчас. Не переносить validation на укороченную копию |
| [BatchLogProbs / ForwardPolicyBatchLogProbs:1007–1017](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/11.DPO/DpoTrainer.cs#L1007-L1017) | После CaptureBatch выбрать read-only execution view стороны: IDs и соответствующую target mask длиной Lside; оба wrapper пользуются одним helper |
| [StepVectorCore:845–922](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/11.DPO/DpoTrainer.cs#L845-L922) | Сохранить admission/permit, objective и instrumented forward/backward/optimizer boundaries. Уменьшается размер каждого допустимого Forward, не число вызовов |
| [DpoDataset / DpoExactPairMaterial](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/11.DPO/DpoDataset.cs) | Не менять source material, tokenizer framing, digest, chosen/rejected lengths, response-sum objective или capacity |
| `DpoProjectLoraRecipient` / internal `CreateForProjectLora` в том же DpoTrainer | Передать immutable eligibility по проверенной topology/LoRA config и узкий execution mode для контрольного A; не делать caller-флагом обход validation |

Реализация — internal helper `DpoValidLengthExecutionView` в новом `11.DPO/DpoValidLengthExecutionView.cs`; он возвращает пару Tensor IDs/mask и длину, не мутирует batch. При Lside==carrier width возвращается прежний view. При Lside<width создаётся собственный prefix [0,Lside), с полным причинным prompt. Не вырезать prompt, первую/последнюю response цель и внутренние нули. TargetMask выбирается native способом; SUM логарифмов response остаётся SUM, не mean. Выравнивание носителя по max length остаётся частью идентичности и допуска.

**Первый перенос ограничен** exact material, проверенным project LoRA recipient, rank2/batch1 tensor, 2≤Lside≤carrier width и нулевыми всеми активными dropout в policy/reference, включая LoRA. Для проверки native A используется internal mode `Padded`; B — `ValidLengthWhenEligible`. После отдельного выбора FIX-PD последний становится режимом только этой допустимой области. Public legacy constructors и неподходящие варианты остаются padded. Nonzero/неустановленный dropout → padded, без попытки «восстановить» RNG вручную. Dropout потребляет случайность по числу элементов; сокращение формы может поменять весь следующий поток и не покрыто опытом.

Execution mode не меняет material/objective version; это численная реализация тех же входов. Но mode и runtime trainer identity пишутся в новом test receipt/production compatibility fingerprint. Существующий точный resume через несовместимый trainer fingerprint по-прежнему отказывает; старые continuation hashes не перепечатываются ради доступа. Нельзя обещать bitwise совпадение нового обучения со старой padded траекторией.

Связь с [FIX-LC](13-dpo-reference-cache.md): базовый PD A/B имеет cache disabled в обоих плечах, как исходный PD01. Если LC уже отдельно принят, его internal capacity0 фиксирует этот контроль. При будущей выбранной композиции LC+PD cache key по прежнему exact digest/profile/reference, hit не вызывает Forward; misses получают допустимый execution view. Новый combined gain не выводится сложением исторических процентов. Эта карточка не запускает второй H512 опыт двух факторов.

## 3. Точный общий старт и учебный материал

Источник — [h512-pad-segmented/v1/bundle.json](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-pad-segmented/v1/bundle.json), полный root `experiments-v2/RP01-MICRO/_shared-learned-starts/h512-pad-segmented/v1/`. Исторический producer1000SFT steps дал PAD1 0/33→33/33, PAD2 0/69→69/69. Его не повторять.

| Файл | Bytes | SHA256 |
|---|---:|---|
| S0.weights.bin | 47899519 | `f404f8eedc6f03edd60103d13d05440083088990fbb728630b750b5902f03987` |
| S0.optimizer.bin, preparation only | 426788 | `a7dd7d747f71bff858e073d92cf7fdf7d803db94d9d16330d004bab51e3b1136` |
| S0.rng.json | 1573 | `f819248e615f50e41c22db16d5fac5116809857d195d3b9d149c9b6ec580ba9b` |
| data.source.jsonl | 724 | `b8dc0bc759983ef45b960b2ea6cb684db698dbfd1c3b38982931bd90906e98c9` |
| data.material.json | 17461 | `668afba066a3600d6ea1ee58226f6a0fcf0a5f0448628537e001fb86faf75f63` |

В raw `sharedStart.startWeightsSha256=578a2915015381f21a204d140bd7836b4a4e5d23375b04b9d216c67a8bae248d`. Это **digest массива adapter values после load**, вычисленный [bridge:494–495](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/PD01/Common/ContinuousLearning/PD01ExperimentBridge.cs#L494-L495), а не hash файла `S0.weights.bin`. Проверить оба значения в их собственных областях; нельзя заменить file SHA на578a.

Модель/config/tokenizer те же идентичности, что в [02](02-learning-state-and-fixtures.md) и FIX-MC: `rp01-csharp-small/v1`, V629/C2048/H512/L4/Q8/KV2/FF1408, q/v rank8/alpha16/dropout0. Frozen reference — отдельная canonical B0 загрузка: `training-artifacts/models/csharp-code-documentation/ilearning-unit/rp01-csharp-small/v1/checkpoints/000001/weights.bin`, weights SHA `a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911`, manifest SHA `c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d`. [Authority](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-pad-segmented/v1/authority.json) не содержит transferable admission/lease/capability. Native test recipient/lease создаются отдельно существующим lawful test path; manifest не подделывается.

DPO optimizer **свежий step0**, после двух updates step2 в обоих плечах. S0.optimizer.bin — SFT подготовка, **не импортируется** в DpoTrainer. Production DPO settings `DefaultLearningRate=1e-5`, weightDecay=.01,beta1=.9,beta2=.95,epsilon1e-8 сохраняются. Тест не зависит от внедрения FIX-MC.

Будущий passport `Tests/Fixtures/ExperimentFixes/v1/Pd01/manifest.json`; исходные PAD1→PAD2 bytes/material копируются без изменения. Exact segmented framing без разделителя: prompt91tokens; PAD1 chosen124/rejected148, response33/57; PAD2 chosen160/rejected124, response69/33. Не заменять этим SFT prompt+LF carrier семейства dt-joint. Новых тем/EXAM нет. Missing LFS/full bytes/hash/qualification → `missing_or_incompatible_fixture`, без bootstrap/подбора другого старта.

## 4. Какие существующие тесты сохранить и как добавить учебное покрытие

Свежие падения не установлены. Baseline ниже выполняется будущим исполнителем до изменения; тест, который не запускался, не объявляется красным.

| Существующий тест | Решение о будущих assertions |
|---|---|
| `DpoExactPairMaterialTests.Masks_IncludeFirstAndLastResponseExcludePromptAndRightPadding` | Исходные masks не меняются; оставить expected |
| `DpoExactPairMaterialTests.Capacity_ExactBoundaryAcceptedAndBothModelLimitsEnforced` | Проверяется полная исходная capacity; нельзя принять ранее недопустимый материал после обрезки |
| `DpoObjectiveConsistencyTests.ExactCarrier_DigestCommitsTheNewObjectiveWhileMaterialAndFramingStayV2` | Digest/material/framing/objective identity неизменны |
| `DpoObjectiveConsistencyTests.LegacyJsonAndPublicAverage_AreUnchanged` | Legacy API и mean objective не переключаются |
| `DpoExactPairMaterialTests.ResponseSumAndBackward_MatchIndependentFiniteDifferencesWithPromptAndPaddingExcluded` | Сохранить независимый finite-difference oracle; новая view не подменяет expected logits своей же реализацией |
| `DpoStepVectorTimingTests.TimedExactStepVector_MatchesUntimedMetricsWeightsOptimizerRandomAndLegacyJson` | Сравнивать timed/untimed **одного execution mode**; их bitwise parity не ослабляется из-за historical padded-versus-prefix различий |
| `DpoTrainerOptimizerStateHandoffTests.DropoutContinuationMatchesUninterruptedNextUpdateForPolicyOptimizerAndRng` | Сохранить nonzero dropout padded continuation и точные hashes; это не допустимая numerical A/B relaxation |

Планируемые новые `Tests/ExperimentFixes/DpoValidLengthLearningTests.cs`, `DpoValidLengthExecutionContractTests.cs`. Главный `NativeTwoPairAblation_PreservesLearningSignalAndRemovesPadding` один раз создаёт A/B по этой fixture. Оба — native DpoTrainer с разным internal execution mode, независимые model/optimizer/RNG, одинаковое обучение PAD1→PAD2; B не experimental source copy. Observer только снимает уже вычисленные lengths/active targets/logits/16gradient norms до update, без второго forward.

Assertions одного receipt: TRAIN before33/33 и69/69; source weights equality; active targets equality; A/B lengths148/148→124/148 и160/160→160/124; response SUM/loss/margin/logits/gradient norms в указанном tolerance; frozen base/reference не меняются; фактические2updates и одинаковое число forward/backward. Сохранить after NLL, whole exact, alloc/wall/peak; не требовать точного historical процента или peak improvement. Test receipt объясняет scope: один test process с независимыми экземплярами, allocations измерены отдельно вокруг каждого двухшагового arm; это не точное воспроизведение historical process timing. Рост полезной метрики фиксируется отдельно от regressions;0→0 не проходит квалификацию.

Companions на tiny H8/L1/Q2/KV1/FF16/V256/C16,rank2,seed17 и byte tokenizer без Train: (1) exact equal-length prompt=a,chosen=bc,rejected=de — identity view и A/B parity; (2) exact unequal prompt=a,chosen=b,rejected=cd с dropout0.1 — оба режима выбирают padded и сохраняют RNG continuation; (3) legacy тех же строк — прежний padded/mean путь. По одному DPO update на A/B каждого case: **6tiny updates,24training Forward,12Backward**,0Generate. Invalid length/mask/capacity, batch-shape и borrowed mutable input проверяются до update,0training. Предварительная validation по-прежнему вправе отклонить неподдерживаемую batch-форму; fallback не делает invalid material допустимым. Контракты не объявляются новым доказательством качества.

## 5. Команды, бюджет и промпт исполнения

Будущие команды после отдельного выбора FIX-PD, из `C:/Projects/llm_mvp_core`, Release/net9.0:

```powershell
Set-Location C:/Projects/llm_mvp_core
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName~DpoExactPairMaterialTests.Masks_IncludeFirstAndLastResponseExcludePromptAndRightPadding|FullyQualifiedName~DpoExactPairMaterialTests.Capacity_ExactBoundaryAcceptedAndBothModelLimitsEnforced|FullyQualifiedName~DpoObjectiveConsistencyTests.ExactCarrier_DigestCommitsTheNewObjectiveWhileMaterialAndFramingStayV2|FullyQualifiedName~DpoObjectiveConsistencyTests.LegacyJsonAndPublicAverage_AreUnchanged|FullyQualifiedName~DpoExactPairMaterialTests.ResponseSumAndBackward_MatchIndependentFiniteDifferencesWithPromptAndPaddingExcluded|FullyQualifiedName~DpoStepVectorTimingTests.TimedExactStepVector_MatchesUntimedMetricsWeightsOptimizerRandomAndLegacyJson|FullyQualifiedName~DpoTrainerOptimizerStateHandoffTests.DropoutContinuationMatchesUninterruptedNextUpdateForPolicyOptimizerAndRng" --logger "trx;LogFileName=baseline.trx" --results-directory artifacts/fbe/FIX-PD/attempt-0001/baseline
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName~DpoValidLengthExecutionContractTests" --logger "trx;LogFileName=contracts.trx" --results-directory artifacts/fbe/FIX-PD/attempt-0001/tests/contracts
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName~DpoValidLengthLearningTests.NativeTwoPairAblation_PreservesLearningSignalAndRemovesPadding" --logger "trx;LogFileName=learning.trx" --results-directory artifacts/fbe/FIX-PD/attempt-0001/tests/learning
```

После изменения один раз повторить existing filter в `tests/existing`, logger `existing.trx`. Existing tiny/finite-difference tests имеют отдельный расход по своим исходникам; не записывать их как0 и не превращать в повтор главной H512 пары. Новые classes сначала реализуются;0discovered не PASS. Назначение не даёт права чинить посторонние тесты solution.

Главный бюджет: **4DPO updates,16training Forward,8Backward,8TRAIN diagnostic Forward** на пару,0SFT preparation,0Generate,0EXAM,0external calls. Это24model Forward всего для обучения и TRAIN readout;60/616positions не превращать в ложный общий счётчик. Сумма с новыми companions отдельно:10updates (4H512+6tiny),40training Forward,20Backward,8TRAIN diagnostic Forward; расходы existing baseline/regression показываются отдельной строкой. Один receipt `artifacts/fbe/FIX-PD/attempt-0001/resources/pair.json`, русский `report.md`. Главный предел15минут; missing artifact/timeout не разрешает новый bootstrap, скрытый retry или ростsteps.

**Промпт исполнителю.** Полный действующий контекст: карточка C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/22-dpo-valid-length.md; свежее полное чтение AGENTS/CLAUDE/skills — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md (§1); роли — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md (§2); применимость и разбор падений — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткий список ниже дополнительный. Выполняется только прямо назначенный ID/этап; документальное поручение не разрешает запуск. PRIMARY, собственные примеры, данные, команды и бюджет полной карточки сохраняются. Модель текущей сессии по умолчанию, без override. Прочитать AGENTS/CLAUDE, skills03 и `planning-session-startup-SKILL.md`, `code-examination-SKILL.md`, `improvement-measurement-SKILL.md`. Сделать только FIX-PD. WRONG: обрезать материал до admission, убрать prompt, менять response SUM, применить prefix к dropout>0, ослабить persistence hashes, сложить выигрыши LC+PD. CORRECT: immutable execution view после проверки, narrow eligibility, один native A/B и отдельные tiny guards, сохранённые peak/NLL regressions. Любое измеренное полезное улучшение признавать по оси/профилю, не обещая универсальности.

Ожидать живую операцию штатным wait того же handle порциями≤60секунд; не завершать ход вместо ожидания, не запускать повтор и не строить polling loop. **Прямой STOP/NEXT немедленно прекращает предыдущую работу, включая её отчёт, тесты, commit и cleanup.** Исторические цитаты STOP не запускают остановку текущей задачи. Next берётся по ID из01, а прямое назначение пользователя важнее очереди.
