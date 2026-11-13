# FIX-N63R — сохранить доступный урок домена в общей памяти повторов

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q21](01-registry.md#work-fix-n63r). Для общего включения действует измеренное ограничение этой строки. Возврат к работе — по записанному условию для её домена. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

**Профилактика приёмки, не снятие запрета общего переноса:** [дополнение48](../experimets/codex-48-post128-executor-verification-2026-11-03.md) передать в отдельно назначенное QA. Показать acquired/retained/lost по IDs и фактическую дозу каждой стороны; одинаковая суммарная точность не исключает потери старых ответов. Честная потеря — научный результат и ограничение допуска; missing/ложная запись — процедурный дефект. Не исправлять gold по actual, не доводить buffer replay до фиктивного DPO, не восстанавливать S0 перед измерением потери. Старые результаты и действующий отказ переноса сохранены.

## Уточнение допуска по показателям — 3 ноября 2026

**FIX-N63R: Не допускать общий перенос.** Вошёл по удержанию одного старого урока. Есть существенные потери других уроков и несопоставимая доза обучения.

- Точность: Старый A07: обученное начало 22/49; после A 2/49, B 15/49 (+26,53 п.п. A→B), C 4/49.
- Полезный результат: Удержание выбранного старого урока заметно лучше.
- Цена и ограничения: Новый N01 22 → 18 верных токенов (−18,18%), N02 28 → 25 (−10,71%), старый A01 2 → 1. Выделения 5 863 987 776 → 12 124 886 728 Б (+106,77%); время 12 217,55 → 28 327,05 мс (+131,86%). 800 против 1200 обновлений.
- [Полное сравнение и статус кода](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#benefit-fix-n63r).

Этот вывод относится к названным ID, а не ко всем механизмам документа. Старый текст плана и опыта сохранён как доказательство, но его общий статус «к переносу / разрешён» не переопределяет это уточнение. Ближайшее действие — разбор уже сохранённых данных и конкретного регресса; новое обучение, повтор PRIMARY и переключение product default этим дополнением не назначаются. Для отдельного логического ремонта назвать нарушенный контракт и сохранить отрицательные контроли, а не переносить весь эксперимент под словом «bugfix».


<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [120-executor-context-and-test-repair-contract](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски REPLAY/CONTINUITY:** WRONG: корректный Sample или вызов нового learner доказывает продолжение старого навыка. CORRECT: named donor → независимые A/B/C weights+keyed optimizer → фактически переданные replay rows → Trainer → те же32 назначенных qualification/final readouts. Consumer-five отдельно: sessionFactory возвращает восстановленный H32, окончательная LoRA preparation сохраняет его adapters, PrepareBatch/TrainPreparedAsync передаёт6строк;22before/after readouts относятся к этому же owner. Смена/обнуление state перед after делает retention недоказанным. Удержанный A07/EO02 — частичная полезная ось; остальные потери не скрывать. Никаких дополнительных Forward при reload, bootstrap или внешних mutation fixtures.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
**Согласование POST102:** [40-effective-executor-context](../experimets/40-effective-executor-context-2026-11-02.md) обязателен для конкретного назначения вместе с полным стеком договора120. [120-post102-transfer-and-executor-audit](https://github.com/lubochka/llm_mvp_core/blob/d18b716e4a855f6dd94415104f48999876108a51/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-post102-transfer-and-executor-audit-2026-11-02.md) — отдельный источник фактов, [121-post102-addressed-repair-plan](https://github.com/lubochka/llm_mvp_core/blob/d18b716e4a855f6dd94415104f48999876108a51/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/121-post102-addressed-repair-plan-2026-11-02.md) — будущие адресные ремонты. Приёмка использует actual caller и тот же callable/options, что calibration. Номер документа, новый объект или nonzero exit не разрешают повтор paid update: фиксировать owner/checkpoint/stage, после сохранённого S1 восстанавливать только назначенные чтения/отчёт с additionalTrain0. Неизвестная стадия — HOLD, не новый entry suffix. Mechanism N/A не смешивать с неизмеренным существующим state.
<!-- EXECUTOR-CONTEXT-120:END -->
<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

**Сравнение replay должно продолжать один lineage памяти.** P103 показал, как два независимых результата Train ошибочно назвали забыванием. Здесь явно записать identity S0, операцию продолжения, S1 и состав replay; если продолжения в API нет, отметить CONTRACT_ABSENT, не подменять его новой фабрикой singleton.

**Наблюдение:** полный ответ каждой прежней и новой строки, правильные шаги/термины, target owner, state hashes. Количество записей или длина результата не заменяет удержание. Испорченный прежний ответ той же длины обязан падать у того же evaluator; неправильный routing owner не получает успех от общей суммы правильных строк.

**Восстановление:** Save→новый объект→Load→обычный consumer, без повторного replay-training. Если меняется несколько learned components, частичный отказ загрузки оставляет прежнее опубликованное состояние. Разделить цену нового урока, replay и независимых диагностических fits, включая неудачные попытки. Исторические выводы исправлять адресной correction receipt, не переписывая raw и не обучая ради нужного отчёта.

Основание: [аудит 123](codex-123-post107-transfer-and-experiment-audit-2026-11-02.md), [план 124](codex-124-post107-addressed-repair-plan-2026-11-02.md), [контекст 41](../experimets/codex-41-post107-executor-checks-2026-11-02.md). Это уточнение приёмки будущего переноса; прежние результаты и назначенная доза сохраняются.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

В учебной проверке actual logits/vector/mask от восстановленного состояния должны попадать в назначенный trainer/reader; canned tensor допускается только в отдельно названном component test. Нулевой optimizer step delta и неизменные веса не доказывают отсутствие Backward. Квитанция различает measured/structural/unknown; эту границу P08 consumer уже пришлось исправлять. Предусмотренное контрактом чтение reference cache или выученной памяти разрешено: проверяется фактический hit/miss и привязка к правильному state/key. Запрет касается подмены проверяемого производителя заранее подготовленным oracle, а не самого механизма FIX-LC.

Для FIX-N63R исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Для метрики DPO указывать единицу pair/step/panel, веса и фазу. Контрпример одного шага с margins [+1,-3]: доля положительных пар 0,5, признак положительного среднего 0; совпадение этих счётов на исходном опыте не делает их одной метрикой. Проверка вызывает полный writer с metric_definitions, а не только внутренний builder. Неизмеренное время SFT — null, не0; исторические receipts не переписывать.

Применимость: FIX-N63R. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Проверять по отдельности optimizer step, scheduler step и число реально применённых обновлений. Контрпример P12: импортированные моменты AdamW400 совместимы с новой эпохой scheduler0; P10, напротив, использует fresh DPO0. Не переносить правило fresh/import из соседнего опыта. После отказа сверять веса, моменты, RNG и исполненные Forward/Backward; отчёт о правильных ответах без назначенного обучения не закрывает учебную цель.

Применимость: FIX-N63R. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-79-followup -->
## Уточнение после последующих исправлений и P03–P05

Retention сначала квалифицируется на сохранённом исходном reader. Изменение maxTokens/EOS между donor и проверкой — INVALID_UNQUALIFIED_BEFORE, а не forgetting. Сохранять обе записи: raw текущего consumer и отдельную оценку прежнего; не смешивать их в среднем. Новый replay-цикл не нужен для исправления ошибочного считывателя, если исходные наблюдения уже записаны.

Основания — [79](79-followup-transfers-and-learning-audit-2026-10-27.md), адресный план — [80](80-followup-repair-plan-2026-10-27.md). Это изменение требований и статуса по прочитанным источникам; неприменённые проверки не объявляются пройденными. Собственные бюджеты и исторические результаты карточки сохраняются.
<!-- END audit-79-followup -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-N63R

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** LEARN/STATE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**M — метрика привязана к фактической стадии обучения.** Для обучающего loss/reward записать состояние, на котором он вычислен, отдельно от номера опубликованных после update весов. У DpoTrainer последняя StepMetrics — pre-update; не называть её оценкой конечного checkpoint. Пример P02: B уже стартовал с margin 0,002773 после SFT; отношение последних значений ×3,22 не равно отношению приростов DPO. Любое сравнение приобретения/ремонта использует собственное before каждого этапа и одинаковый reader. DPO Accuracy=(reward_margin>0) означает улучшение относительно reference, а не правильный ответ: T2 имеет policy chosen−rejected=−0,1827326 при положительном reward. TRAIN/EXAM correctness и сохранённые logP остаются отдельными полями. Если эта карточка сравнивает математически равные оптимизации, сохранить её A/B equality; не требовать искусственного роста точности и не добавлять post-update Forward, когда нужное чтение уже предусмотрено.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Различимость воздействия и сохранённого навыка, 27 октября 2026.** Наличие CODE_Q в буфере проверяется вместе с его фактическим участием в native SFT и поштучными old/new responses. Буфер с правильными IDs, чьи уроки не дошли до Trainer, не проходит тест. Сохранять actual число завершённых updates, суммарные shifted/response token exposures и вес каждого урока по рукам: равные число шагов или размер replay buffer не означают одинаковую учебную дозу. Исходный неуравненный token budget не выдаётся за изолированное доказательство политики при одинаковой дозе.

После окончательной LoRA preparation снимается learned-before; повторное обнуление adapters/moments, repair-Train перед оценкой и замена acquired gold новым stale target запрещены. Уже назначенные consumer-five и учебная панель остаются раздельными; добавлять новую matched-dose руку или повышать бюджет здесь не назначено. Частичное удержание токенов при неправильных целых ответах не закрывает full-answer retention и не квалифицирует общий rollout при регрессиях других строк.

**Уточнение исполнения, 27 октября 2026.** Проверять не только размер replay-списка, но идентичность сохранённого урока каждой назначенной темы до его передачи настоящему learner. После нового урока фиксировать удержание сразу, без предварительного повторного предъявления забытого gold: такой replay скрыл бы исследуемую потерю. В retention readout один неизменный правильный target; количество replay entries и успешное чтение файла не заменяют ненулевой учебный результат. Отрицательные контроли и budget этой карточки сохраняются.

Общие примеры правильной и ошибочной проверки — [контракт03§24](03-test-and-execution-contract.md#transfer-audit-20261027). Это дополнение плана; новые проверки сейчас не запускались.

Статус: **условный кандидат по измеренному удержанию части токенов; не готовность к выпуску**. Дата: 21.10.2026. Источники закреплены на `4f35cb71b3db482bfe7ebe611c652c78d89d5db1`. Этот документ планирует будущие изменения и проверки; код, обучение и тесты в данной сессии не выполнялись. Следующий ID и зависимости берутся только из [реестра](01-registry.md). Обязательны [состояние и fixtures](02-learning-state-and-fixtures.md) и [контракт исполнения](03-test-and-execution-contract.md).

## 1. Измеренное основание и точная граница успеха

Основание — **N63R-PROBE**, а не прежняя девятишаговая карточная попытка N63-AR-CAP с `0→0`. Probe сначала приобрёл частичный навык на A01/A07 и сохранил веса с moments. Затем общий FIFO вытеснил CODE_Q из буфера после добавления BUG; экспериментальная политика оставила один урок CODE_Q, который реально участвовал в последующих SFT-обновлениях. Это полезное удержание токенов, а не только проверка состава списка. [Программа](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N63R-PROBE/Program.cs#L77), [raw](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N63R-PROBE/results/n63r-probe.json#L7952).

Все числа ниже — правильные response-токены при подаче правильного префикса. Целый ответ неверен **во всех показанных состояниях и строках**; свободная генерация не измерена.

| Строка | Обученное «до» | A: FIFO, без повтора | B: сохранён A07 | C: без повтора, столько же updates, сколько B |
|---|---:|---:|---:|---:|
| Старый A07, 49 токенов | 22 | 2 | **15** | 4 |
| Старый A01, 43 токена | 23 | 2 | **1 — хуже A/C** | 2 |
| Новый N01, 26 токенов | 0 | 22 | **18 — хуже A/C** | 24 |
| Новый N02, 31 токен | 0 | 28 | **25 — хуже A/C** | 28 |
| EXAM EO01, 45 токенов | 6 | 1 | **0 — хуже A/C** | 1 |
| EXAM EO02, 49 токенов | 15 | 1 | **15** | 3 |
| EXAM EN01, 35 токенов | 1 | 4 | **1 — хуже A/C** | 2 |
| EXAM EN02, 29 токенов | 0 | 3 | **1 — хуже A** | 1 |

У A07 response NLL после A/B/C: `5.588743 / 3.593689 / 5.751105`; до было `3.252996`. B теряет меньше A, но всё равно хуже собственного «до». У EO02 B сохраняет 15/49. Это успех по этим показателям и теме **объяснение поведения C# / CODE_Q**. Из него нельзя вывести сохранение всех прежних уроков, успешность всех новых уроков или пригодность для иных доменов. BUG здесь создаёт нагрузку на общий буфер и не входит в обучение CODE_Q. [Полные per-row readouts](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N63R-PROBE/results/n63r-probe.json#L7979).

| Стоимость продолжения | A | B | C |
|---|---:|---:|---:|
| Updates | 800 | 1200 | 1200 |
| Предъявленные input tokens | 63 600 | 118 800 | 95 400 |
| Выделено байт | 5 863 987 776 | 12 124 886 728 | 8 795 992 464 |
| Wall, мс | 12 217.5506 | 28 327.0492 | 17 791.1449 |
| CPU, мс | 10 406.25 | 22 125 | 16 484.375 |

B получил на 50% больше updates, на 86.79% больше input tokens, на 106.77% больше allocations и на 131.86% больше wall, чем A. C уравнивает только updates; число tokens и содержание обучения остаются разными. Один процесс с порядком A→B→C не доказывает причинное ухудшение времени от одного лишь способа вытеснения. Не называть этот опыт сравнением при равном compute budget. [Сырые arms](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N63R-PROBE/results/n63r-probe.json#L46).

Результат уже учтён в [required-changes](../required-changes.md). Новый слой здесь: конкретный продуктовый метод, поведение при числе доменов больше capacity, корректное продолжение из донора, действующий порог MinExamples и тесты настоящего потребителя. Старые эксперименты, их ограничения, source IDs и raw сохраняются.

## 2. Куда переносить и что именно менять

Точка изменения — [`16.AutonomousLearning/Training/ReplayBuffer.cs::Add`](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/16.AutonomousLearning/Training/ReplayBuffer.cs#L32). Сейчас при переполнении удаляется общий старый префикс. Будущий вариант:

1. После добавления строки при переполнении посчитать число строк каждого `Domain` с `StringComparer.Ordinal`.
2. Найти первую по порядку вставки строку домена, у которого больше одной строки; удалить её.
3. Если все домены представлены единственной строкой, удалить самую старую строку общего FIFO. Capacity остаётся жёстким пределом: когда доменов больше мест, сохранить каждый невозможно.
4. Повторить только пока размер больше capacity. Не менять `DefaultMaxCapacity=4096`, минимум capacity1, `ReplayRatio=.3`, seed/RNG, порядок возвращаемых строк, фильтр домена и случайную выборку `Sample`.

Шаги1–2 соответствуют [экспериментальному B](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/35.ExperimentsV2/Experiments/Rp01Micro/N63R/ReplayBuffer.B.cs#L44). Шаг3 — явно новое поведение для производственного общего буфера: исходный опыт ограничен CODE_Q/BUG и бросал исключение при отсутствии кандидата. Этот exception и список `AllowedDomains` **не переносить** в универсальный продукт. Fallback покрывается контрактным тестом и не получает чужого доказательства качества обучения. При capacity2 и исторических четырёх вставках он не меняет measured case.

Реальный потребитель: [`AutonomousTrainer.PrepareBatch`](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/16.AutonomousLearning/Training/AutonomousTrainer.cs#L69) → `ReplayBuffer.Sample` → manifest → [`TrainPreparedAsync`](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/16.AutonomousLearning/Training/AutonomousTrainer.cs#L113) → `IIncrementalTrainer.TrainAsync` → настоящие SFT-обновления. Новые строки добавляются в буфер только после успешного artifact/provenance результата. В первой правке `AutonomousTrainer` не требует смены алгоритма; тест подтверждает существующий путь. `14.ContinuousLearning/Training/AutonomousLearningReplayMixer` — другой механизм и не является получателем этого исправления.

**Порог `AutonomousTrainer.MinExamples=5` сохраняется.** Исходные N01/N02 — две строки; передать их прямо в `AutonomousTrainer` и ожидать обучения нельзя. `PrepareBatch` законно вернёт `INSUFFICIENT_EXAMPLES` до replay. Нельзя понизить gate до2, размножить две строки до5 или назвать mocked call успешным обучением. [Guard](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/16.AutonomousLearning/Training/AutonomousTrainer.cs#L77).

## 3. Два раздельных набора будущих тестов

Корень — **`Tests/Fixtures/ExperimentFixes/v1/N63R/`**; запланированы `manifest.json`, `native-retention.json`, `consumer-five.json`. Создавать их только на этапе реализации. Существующий исходник: [`experiments-v2/RP01-MICRO/N63R/fixtures/input.json`](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N63R/fixtures/input.json). Точные bytes и hashes закрепляются по общему контракту02.

### 3.1. Учебный regression: прежние две новые строки

Состав неизменен: старые TRAIN A01/A07; новые TRAIN N01/N02; BUG B01/B02 используются только для вытеснения в общей памяти; EXAM EO01/EO02/EN01/EN02. Четыре CODE_Q TRAIN readouts и четыре EXAM readouts сериализуются раздельно. Вставки буфера строго A01,A07,B01,B02, capacity2, seed42. A-baseline даёт B01,B02 и Sample(N01,N02); B-product даёт A07,B02 и Sample(N01,N02,A07). В B вызывается настоящий `AutonomousLearning.ReplayBuffer`, а не копия B из эксперимента; A — ограниченная неизменная FIFO baseline-копия закреплённого `Add`, отделённая от тестируемого продукта.

Применяется прямой нативный learner, как в [experiments-v2/RP01-MICRO/N63R/NativeLearner.cs](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N63R/NativeLearner.cs#L113): `TrainingLoop.Trainer.TrainSupervisedNextTokenStep`, `LlmModel.LoadCheckpoint`, `AdamW.TryImportPortableState`. Именно этот helper подключает [N63R-PROBE/Probe.csproj](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N63R-PROBE/Probe.csproj#L20); в `35.ExperimentsV2/Experiments/Rp01Micro/N63R/` находятся только две копии ReplayBuffer, не learner. Новый helper `Tests/ExperimentFixes/N63RNativeFixture.cs` только составляет эти вызовы; не копирует optimizer, gradient или LoRA. Этот слой намеренно не вызывает `AutonomousTrainer`, поэтому не обходит его MinExamples.

Модель — H32: vocab256/context256/layers1/heads4/kvheads2/ffn64; BPE с256 byte-токенами без merges. LoRA q_proj/v_proj, rank8/alpha16/dropout0, seed42/stream `n63-ar-cap`. Framing `prompt + LF + response`, максимум257 полных tokens, без обрезки; response targets начинаются с `Encode(prompt+LF).Count-1`. Config и scorer берутся из указанного NativeLearner, response gold — из fixture, не из ответа модели.

Для native regression **и** consumer явно создать одинаковый исходный `TrainingConfig`: `BatchSize=1`, `SeqLen=256`, `WarmupSteps=0`, `TotalSteps=1000`, `MaxLr=.001f`, `MinLr=.001f`, `GradAccumSteps=1`, `GradClipNorm=1f`, `WeightDecay=0f`. AdamW создавать с `beta1=.9f`, `beta2=.95f`, `weightDecay=0f`, `eps=1e-8f` и затем импортировать сохранённые keyed moments/step. `TotalSteps` — параметр расписания тренера, а не число вызовов consumer; его будущий бюджет остаётся6 SFT updates. Эти значения взяты из [N63R-PROBE/Program.cs:46–62](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N63R-PROBE/Program.cs#L46), не из defaults текущего продукта.

Доноры: `experiments-v2/RP01-MICRO/N63R-PROBE/results/wa-trained.bin` и `wa-trained-optimizer.bin`; дополнительный источник базовой идентичности — `w0-base.bin` из той же папки. Сначала поставить исходную topology, затем загрузить полные weights, проверить keyed binding и импортировать исходные moments/step. Три независимых объекта A/B/C получают одинаковые bytes; они не делят mutable model/optimizer. Проверить старое «до» A01=23/43, A07=22/49 и панель из raw. Не подставлять LC H512, не обнулять optimizer и не учить заново1000+400 preparatory steps. Снимок RNG probe отдельно не сохранён: для dropout0 фиксируются одинаковые вновь создаваемые RNG, и это не объявляется доказательством точного продолжения ненулевого dropout.

Бюджет будущей адресной проверки: A800, B1200, C1200 SFT updates в историческом порядке строк; всего3200, DPO0/Generate0/внешние модели0. Qualification readout общего старта8 forward, после A/B/C по8 — всего32 readout-forward; preparatory updates0. Это известный ограниченный учебный переход, а не доказанный оптимальный минимум. Ни сокращать до прежних9 шагов с0→0, ни автоматически расширять бюджет нельзя. Если донор отсутствует/несовместим, статус `missing_or_incompatible_fixture`, без скрытого bootstrap.

У будущего теста три вывода отдельно: строгие buffer/domain invariants; реальные до/после readouts; измеренный сравнительный retention и его цена. Проверять направление сохранённого эффекта A07 и EO02 против A/C при matched runtime, выводить все восемь строк и полные exact flags. Исторические float values не превращать в универсальные численные константы CI. Потеря полезного эффекта не лечится изменением gold, seed или состава EXAM. Одного совпадения Sample недостаточно для PASS учебной части.

### 3.2. Настоящий consumer: пять новых CODE_Q строк

`consumer-five.json` — **новый авторский набор проверки соединения**, не изменение старого опыта и не новое уже измеренное доказательство качества. Порядок TRAIN: прежние N01,N02 и следующие три уникальные строки; все Domain=`CODE_Q`, weight1, capturedAt=`2026-10-20T00:00:00Z`.

| ID | Точный prompt | Точный response |
|---|---|---|
| N03 | `Explain in unchecked C#: int F(int x){return x-4;}` | `Returns unchecked x minus four.` |
| N04 | `Explain in unchecked C#: int F(int x){return x*3;}` | `Returns unchecked three times x.` |
| N05 | `Explain in unchecked C#: int F(int x){return x^1;}` | `Returns x with its least significant bit toggled.` |

Тема одна — объяснение поведения C#; BUG не входит в TRAIN. EXAM остаётся четырьмя исходными строками. Никакого подбора дополнительных строк исполнителем. Два старых урока используются как readout обученного донора; A07 дополнительно приходит из ReplayBuffer. После исторических четырёх вставок `PrepareBatch` получает5 новых и1 допустимый replay, всего6, поскольку `min(round(5*.3/.7), eligible1)=1`.

Исторические `simulation:n63:<id>` непригодны для product manifest. Только в **новых fixture-объектах** назначить каждому ID `TrainingSourceProvenance.CreateSourceId("fixes-based-on-experiments/FIX-N63R/v1/" + id, prompt + "\n" + response)`. Сохранить historical ID отдельным metadata-полем; старый input.json не править. Это нужно именно для [`PrepareBatch`→`BuildManifest`](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/14.ContinuousLearning/Core/TrainingProvenance.cs#L71), который отвергает noncanonical/duplicate IDs. Manifest покрывает реальные six rows, не только пять новых.

Consumer-test использует настоящий `AutonomousTrainer` и настоящий `ContinuousLearning.IncrementalTrainer` с публичным factory-конструктором: `new IncrementalTrainer(storage, sessionFactory, adapterDir: consumerOutput, loraConfig: donorConfig)`, где `donorConfig = new LoraConfig { Rank=8, Alpha=16f, Dropout=0f, TargetModules=["q_proj", "v_proj"] }`; `DpoPairs=[]`. Параметр `loraConfig` нельзя опускать: [production default Dropout=.05](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/14.ContinuousLearning/Training/IncrementalTrainer.cs#L821) несовместим с восстановленной topology dropout0. Новый test-owned `IIncrementalTrainingSessionFactory` в `N63RNativeFixture.cs` возвращает независимый `IncrementalTrainingSession` с восстановленным H32, tokenizer, Trainer с указанным выше TrainingConfig и AdamW с указанными коэффициентами и импортированными moments. Хэши/ModelIdentity вычисляются из прочитанных bytes/config, `CheckpointStart` не притворяется canonical repository parent. Это легальный локальный тест инъекции, не публикация донора в production registry. `consumerOutput` — `artifacts/fbe/FIX-N63R/attempt-0001/resources/consumer-adapters`; каталог не используется для результатов других проверок.

Перед первым SFT проверить, что повторный `LoraApplicator.Apply` в [`IncrementalTrainer:505`](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/14.ContinuousLearning/Training/IncrementalTrainer.cs#L505) сохраняет совместимые установленные adapters. Их повторное использование предусмотрено [`LoraApplicator.BuildPlan`](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/10.LoRA/LoraApplicator.cs#L747); не объявлять существующий код переинициализацией без evidence. Нужны ненулевые before readouts после окончательной подготовки topology, а не до неё.

Один вызов `TrainPreparedAsync` передаёт6 действительных строк в [`IncrementalTrainer` supervised loop](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/14.ContinuousLearning/Training/IncrementalTrainer.cs#L644):6 SFT updates, DPO0; не400 повторных consumer cycles. Записать before/after для7 CODE_Q TRAIN readouts (A01,A07,N01–N05) и4 EXAM, то есть22 readout-forward. Проверить ненулевой сохранённый навык, реальные adapter changes при неизменной базе и полный returned provenance. При повторной загрузке adapter в свежую совместимую модель сравнить named keys/shapes/value bits и frozen base hash без дополнительных Forward: это отдельная проверка сохранения состояния, не ещё один readout или новое доказательство качества. Бюджет22 readout относится только к before/after обучения; reload добавляет0. Число вызовов/сохранённый файл сами по себе не являются учебным результатом. Эта проверка устанавливает подключение consumer и сохранение контракта, не переносит прежние15/49 на новый пятистрочный curriculum.

## 4. Существующие и новые тесты

Текущие падения не установлены. Адресные существующие тесты находятся в [`Tests/AutonomousLearningTests.cs`](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/Tests/AutonomousLearningTests.cs#L294), именно project16. `AutonomousLearningReplayMixerTests` проверяет project14 и не заменяет их.

| Тест / группа | План переноса |
|---|---|
| `AutonomousLearning_ReplayBuffer_MixRatio_Is70New30Old`, `EmptyBuffer_Returns100PercentNew`, `NeverMixesAnotherAdapterDomain` | Сохранить formula, new-first order и запрет смешивать домены. В именах таблицы опущен повторяющийся prefix `AutonomousLearning_ReplayBuffer_` у последних двух методов. |
| `AutonomousLearning_ReplayBuffer_CapsHistoryAtConfiguredCapacity` | Существующий one-domain сценарий по-прежнему оставляет последние3 строки: ожидание `[old-p2,old-p3,old-p4]` не менять. Добавить отдельный mixed-domain case. |
| `AutonomousLearning_ReplayBuffer_MinimumCapacityIsOne` | Сохранить минимальную capacity и последний one-domain урок; добавить all-unique-domain fallback case, не делать unbounded buffer. |
| `AutonomousLearning_AutonomousTrainer_MinimumExamplesNotMet_ReturnsFail` | Сохранить строгое `INSUFFICIENT_EXAMPLES` для2 строк и неизменность буфера. Этот тест не «мешает эксперименту». |
| `AutonomousLearning_TrainPrepared_RejectsManifestNotBoundToFrozenBatch`, `AutonomousLearning_AutonomousTrainer_MixedNewDomainsFailBeforeReplayOrTraining`, `AutonomousLearning_AutonomousTrainer_ForwardsCancellationToken` | Сохранить manifest/domain/cancellation boundaries; canonical IDs исправляются в новой fixture, не ослаблением guards. |
| Новый `Tests/AutonomousReplayRetentionLearningTests.cs` | `ReservedDomainLesson_ReachesNativeTrainingAndRetainsMeasuredPartialSkill` — основной A/B/C. `FiveNewRows_ReachNativeIncrementalTrainerWithOnlySameDomainReplay` — настоящий consumer выше. |
| Тот же новый файл, контрактные tests | `Overflow_RemovesOldestFromRepeatedDomain`; `AllUniqueDomains_FallsBackToBoundedFifo`; `TwoNewRows_KeepInsufficientExamplesGuard`; `HistoricalSourceIds_AreRejectedAndCanonicalFixtureIdsAreAccepted`. Явно не считать эти четыре теста четырьмя опытами обучения. |

Если после изменения тест ожидал общий FIFO для **нескольких** доменов, разделить сценарии и вывести новый expected из явной последовательности вставок. Не переписывать observed actual в gold и не удалять one-domain/negative assertions. Если падают ratio, manifest, MinExamples, base freeze или actual readouts — чинить перенос либо оставить кандидат невключённым, не ослаблять tests.

## 5. Ограниченные шаги будущей работы и команды

После назначения пользователем: (1) проверить fixtures/доноры и текущий адресный baseline; (2) изменить только `ReplayBuffer.Add` и добавить private helper подсчёта/выбора; (3) добавить указанные fixtures и native helper; (4) выполнить контрактные tests, один учебный A/B/C и один пятистрочный consumer; (5) сохранить русский отчёт с каждой регрессией и ресурсной ценой, провести назначенный review. Это не разрешение запускать старый `Run-Probe.ps1`, изменять прочие retention policies или глобальную Windows/solution цепочку.

Команды **после реализации** из `C:/Projects/llm_mvp_core`:

```powershell
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName~AutonomousReplayRetentionLearningTests|FullyQualifiedName~AutonomousLearningTests.AutonomousLearning_ReplayBuffer|FullyQualifiedName~AutonomousLearningTests.AutonomousLearning_AutonomousTrainer|FullyQualifiedName~AutonomousLearningTests.AutonomousLearning_TrainPrepared_RejectsManifestNotBoundToFrozenBatch" --logger "trx;LogFileName=FIX-N63R.trx" --results-directory artifacts/fbe/FIX-N63R/attempt-0001/tests
```

Before/after панели и расходы сохранять в `artifacts/fbe/FIX-N63R/attempt-0001/resources/native-retention.json` и `consumer-five.json`, отчёт — в `artifacts/fbe/FIX-N63R/attempt-0001/report.md`. Нельзя удалять существующий каталог или выбирать autosuffix вместо чтения результата/handle. Общий учебный бюджет назначения:3200+6 SFT updates без повторной подготовки; отдельные guard-tests без обновлений. Не считать current old failed run затратой нового теста и не забывать уже оплаченные1000+400 в описании исторического probe. Сохранённый partial gain остаётся кандидатом даже при отсутствии абсолютного роста accuracy; непройденные release gates остаются видимыми.

## 6. Готовый промпт исполнителю

> Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/16-replay-domain-retention.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.
>
> Выполни только назначенный FIX-N63R из реестра01 после решения о переносе. Используй текущую модель сессии по умолчанию. Прочитай AGENTS.md, CLAUDE.md, общий контракт03 и skills `docs/skills/architect-behavior-classifier-SKILL.md`, `trainable-architecture-decision-SKILL.md`, `intent-to-plan-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `test-integrity-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`. Перенеси только описанную политику Add; Sample, ratio и MinExamples5 сохраняются. Не повторяй consumed probe, не удаляй старые эксперименты, не придумывай новый домен/набор/модель, не загружай LC H512 вместо N63R H32. Частичное удержание15/49 — полезный результат; полные ответы false и регрессии других строк показывай рядом. Исполняй native learning и пятистрочный consumer раздельно; mock counts не называй обучением. Source IDs канонизируй только в новой fixture. При несовместимом доноре не запускай bootstrap. Отчёт на русском по контракту03: exact sources, до/после, все строки, updates/tokens/time/memory, reload, что доказано и что не доказано. Живой процесс жди штатным wait исходного handle, не завершай ход вместо ожидания и не создавай polling-loop/повторный launch. При STOP немедленно прекращай действия. При NEXT немедленно переходи к указанному ID: запрещено дописывать прежний отчёт, исправлять прежние тесты, коммитить или снова запускать прежнюю карточку под предлогом завершения. Следующий ID берётся только из реестра. Новые DPO здесь не генерируются; роли внешнего учителя/выбора/оценки не присваиваются SFT-fixture.
