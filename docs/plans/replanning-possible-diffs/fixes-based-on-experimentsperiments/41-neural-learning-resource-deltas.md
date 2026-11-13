# 41. Перенос измеренных нейронных изменений и границы DPO context

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q33](../fixes-based-on-experiments/01-registry.md#work-fix-rpe), [R07](../fixes-based-on-experiments/01-registry.md#work-fix-cs), [R09](../fixes-based-on-experiments/01-registry.md#work-fix-mb). В документе несколько работ. У каждой строки собственное назначение и условие окончания. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](../fixes-based-on-experiments/03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](../fixes-based-on-experiments/01-registry.md#executor-next).

**Действующие поручения по открытым переносам** — [Q35](#q35-min) и [Q32](#q32-min). Для них брать указанный в соответствующем разделе один учебный шаг. В общих таблицах ниже записаны дозы и результаты исходных сравнений для справки. Остальные работы этого сборника сохраняют свои области.


## Уточнение допуска по показателям — 3 ноября 2026

**FIX-MB: Приостановить общий перенос по пограничному таймингу.** Экономия доказана, но нельзя округлить +5,07% до допустимых 5%. Нужна квалификация сохранённых точных замеров; без нового запуска в этой сессии.

- Точность: Учебные/экзаменационные ответы и состояние A/B совпали.
- Полезный результат: Выделения 13 642 496 → 12 782 336 Б (−6,31%).
- Цена и ограничения: Время 74,021 → 77,774 мс (+5,07%): чуть выше границы 5%, по округлённому отчёту.
- [Полное сравнение и статус кода](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#benefit-fix-mb).

**FIX-RPE: Приостановить общий перенос.** Вошёл по малой экономии; рост времени обучения >5% не покрывается исключением для CPU.

- Точность: Учебных 1/2 → 1/2, экзаменационных 1/2 → 1/2; конечное состояние одинаково.
- Полезный результат: Построения 4 → 1; выделения 13 640 016 → 13 623 456 Б (−0,1214%).
- Цена и ограничения: Время 93,571 → 110,826 мс (+18,44%); удерживается новый кеш 1 344 Б; CPU 78,1 → 109,4 мс.
- [Полное сравнение и статус кода](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#benefit-fix-rpe).

Этот вывод относится к названным ID, а не ко всем механизмам документа. Старый текст плана и опыта сохранён как доказательство, но его общий статус «к переносу / разрешён» не переопределяет это уточнение. Ближайшее действие — разбор уже сохранённых данных и конкретного регресса; новое обучение, повтор PRIMARY и переключение product default этим дополнением не назначаются. Для отдельного логического ремонта назвать нарушенный контракт и сохранить отрицательные контроли, а не переносить весь эксперимент под словом «bugfix».


<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски CORE/LEARN:** Kernel parity проверить на назначенном реальном learning пути: независимые tensors/gradients/updates и ненулевой правильный readout. Отрицательный контроль обнаруживает возврат ошибки, положительный сохраняет lawful путь; isolated0→0 не качество обучения. Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Для ресурсного переноса сравнивать actual native операции на том же обученном состоянии и в одинаковом окне. Managed allocations, peak и CPU различаются; прогрев/дрейф показываются по блокам. P08 продемонстрировал полезный gold-mass эффект при allocations+155,65%: наличие quality gain и кода в продукте не отменяет HOLD по памяти. Константа счётчика не является наблюдением.

Для FIX-SC, FIX-CS, FIX-AC, FIX-MB, FIX-RPE, FIX-SEXP исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](../fixes-based-on-experiments/92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Стоимость относится к точно названному окну. Контрпример P09: разность allocations четырёх arm windows, делённая на4, включает создание входов/cache/receipts и не является постоянной стоимостью одного Forward. Для этой оптимизации указать размер работы, warmup, что внутри/снаружи счётчиков и одинаковую обвязку A/B; не закреплять точные bytes по одному такому среднему. Проверять существующие tests и дополнять их learned fixture, не объявлять отсутствующими по одному отчёту.

Применимость: FIX-SC, FIX-CS, FIX-AC, FIX-MB, FIX-RPE, FIX-SEXP. Основание и адресные проверки — [план85](../fixes-based-on-experiments/85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](../fixes-based-on-experiments/03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Полный ресурсный бюджет обязан включать обязательные диагностические вызовы и неудачные попытки, с отдельными единицами model/layer и optimizer-call/effective-update. Контрпример P11: 40 model Forward основной части +19 контрольных =59; 8 optimizer calls дают лишь4 изменения весов. Oracle/FD, заявленный обязательным, должен влиять на принятие результата, а не только выводиться в JSON. Равенство старому алгоритму подтверждать независимой проверкой, не комментарием в исходнике.

Применимость: FIX-SC, FIX-CS, FIX-AC, FIX-MB, FIX-RPE, FIX-SEXP. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](../fixes-based-on-experiments/88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-SC, FIX-CS, FIX-AC, FIX-MB, FIX-RPE, FIX-SEXP

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE/LEARN по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](../fixes-based-on-experiments/76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](../fixes-based-on-experiments/77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**M — метрика привязана к фактической стадии обучения.** Для обучающего loss/reward записать состояние, на котором он вычислен, отдельно от номера опубликованных после update весов. У DpoTrainer последняя StepMetrics — pre-update; не называть её оценкой конечного checkpoint. Пример P02: B уже стартовал с margin 0,002773 после SFT; отношение последних значений ×3,22 не равно отношению приростов DPO. Любое сравнение приобретения/ремонта использует собственное before каждого этапа и одинаковый reader. DPO Accuracy=(reward_margin>0) означает улучшение относительно reference, а не правильный ответ: T2 имеет policy chosen−rejected=−0,1827326 при положительном reward. TRAIN/EXAM correctness и сохранённые logP остаются отдельными полями. Если эта карточка сравнивает математически равные оптимизации, сохранить её A/B equality; не требовать искусственного роста точности и не добавлять post-update Forward, когда нужное чтение уже предусмотрено.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Адресный контроль шести внешних владельцев, 27 октября 2026.** Здесь находятся FIX-SC/CS/AC/MB/RPE/SEXP; перенесённые AGC/FBM/RRD/ATS/SSR имеют собственные актуальные owner-карточки. Закрытие одного tensor helper не закрывает эти шесть строк.

| ID | Различающий контроль и граница результата |
|---|---|
| FIX-SC | Полный IDs/mask carrier сохраняет последний target при input T−1. Удаление только последнего scored target должно нарушить уже назначенный ScTensorContracts, даже если общий digest входа прежний. A admission refusal означает0 завершённых updates; B success —200. Обработчик, выставляющий success до StepVector, неприемлем. Не сравнивать стоимость A0 и B200 как ускорение |
| FIX-CS | Сравнивать все назначенные logits/градиенты, включая раннюю причинную строку; совпадение только последней позиции может скрыть неправильную маску. Counter/shape helper не заменяет собственный H64 native training receipt |
| FIX-AC | Scratch должен полностью перезаписываться перед каждым чтением softmax. На уже назначенных разных строках прежнее содержимое scratch не влияет на результат; одинаковый input дважды не выявляет stale values. Сохранять actual native softmax consumer и цену удалённой копии |
| FIX-MB | Не смешивать backward Mean с forward RRD. Неверное распределение upstream между строками должно менять независимый scalar gradient; сравнение конечных full weights/optimizer/RNG — с A, а не только B/B. Доказанный byte parity опубликованных файлов не равен выполненному fresh restore |
| FIX-RPE | Идентичность cached training table включает параметры и фактически запрошенные позиции; другой context/позиция не получает прежнюю строку. Уже назначенные inference/fallback контракты остаются отдельными; экономия другого cache не приписывается RPE |
| FIX-SEXP | Контроль аналитического gradient и числа вычислений Exp относится к BackwardLoss, а не к loss Forward. H16 donor3/40 является частичным навыком; сохранение0/2 целых ответов не доказывает полное усвоение. Сравнение digest весов не заменяет optimizer1000→1004 и typed RNG |

Полный numerical oracle использует имена/формы/битовые массивы parameters, optimizer entries/moments/step и все typed RNG entries по правилам [68, U1/U2](../fixes-based-on-experiments/68-transfer-repair-followup-plan-2026-10-27.md), не ToString или агрегированную loss. Назначенные donors, budgets и native admission §3/§5 сохраняются. Счётчик completed изменяется после успешного native вызова; сообщения о попытке не означают update. Проверки comparator работают с копиями уже снятых данных, без дополнительных обучения/Forward. Примитивные contracts без Train допустимы как unit-проверки, но не как самостоятельный новый эксперимент.

Дата: 23 октября 2026. Фаза этого документа — **READ_ONLY_RESEARCH + планирование**. Здесь назначены будущие изменения и проверки; код, тесты, обучение, сборки и исторические hosts этим поручением не запускались. Писался только этот Markdown в новом каталоге `fixes-based-on-experimentsperiments`. Старый каталог `fixes-based-on-experiments` не изменён.

Включены одиннадцать отдельных ID: **FIX-AGC, FIX-FBM, FIX-RD, FIX-AT, FIX-SILU, FIX-CS, FIX-MB, FIX-RPE, FIX-AC, FIX-SEXP, FIX-SC**. Первые десять относятся к ресурсам действительного обучения; FIX-SC — к согласованию длины входа и next-token targets. Малый полезный результат допустим, рост accuracy и минимальный процент выигрыша не вводятся. Timing одного упорядоченного A/B остаётся наблюдением, а не доказанным причинным ускорением. Каждый ID переносится отдельно, с собственным A до изменения и B после; эффекты разных ID не складываются.

## 1. Дедупликация, статус и порядок

Сверены README и строки [старого реестра](../fixes-based-on-experiments/01-registry.md), целевые главы [контракта переноса](../fixes-based-on-experiments/03-test-and-execution-contract.md), исходные symbols и сохранённые receipts ниже. Старые FIX-LC, FIX-TS, FIX-GB, FIX-NH и FIX-RMS уже имеют планы 13/28/29/36/30. Они здесь не описываются заново: FIX-FBM не заменяет FIX-RMS, FIX-MB не заменяет FIX-RD, FIX-CS не меняет gradB из FIX-GB, FIX-SC не повторяет selection строк головы из FIX-NH. Старые NEURAL-FROZEN-TRANSPOSE и N62 слабые/неполные попытки не используются вместо новых квалифицированных RD/AT/AGC/FBM receipts.

В общем реестре FIX-SC имеет приоритет разблокирования обучения. Внутри ресурсной группы рекомендуемый порядок: AGC → FBM → RD → AT → MB → RPE → SILU → CS → AC → SEXP. Это порядок изоляции зависимостей и рисков, не разрешение выполнить программу. Каждый A фиксирует реально выбранный product commit с уже принятыми предыдущими изменениями; B отличается только текущим фактором. Исторические байтовые дельты нельзя обещать после наложения других оптимизаций.

Поздний SEXP receipt23 октября обновляет именно основание включения: старые 01-registry:84 и 04-evidence-and-novelty:44 фиксировали отсутствие измеренного выигрыша/невыполненность на прежнем срезе. Это не существующий implementation plan и не основание исключить новый результат 39.6467→27.2809ms. Старые документы сохранены; **FIX-SEXP supersedes прежнее exclusion по позднему результату**, не переименовывает FIX-SILU или FIX-AC.

Решение по admission:

| ID | Достаточное основание для плана | Ограничение |
|---|---|---|
| FIX-AGC, FBM, RD, AT, MB, RPE | Измерено уменьшение allocations при настоящих updates; сохранены заданные учебные/численные endpoints | Нет нового доказательства обобщения, универсальной величины выигрыша или production transfer |
| FIX-SILU, CS, AC, SEXP | Измерено положительное wall/CPU наблюдение, есть точная сокращаемая работа и численная parity | Причинный размер временного эффекта не установлен; SILU и SEXP увеличивают allocations; AC — особенно малый эффект |
| FIX-SC | B допускает корректный shifted boundary, меняет четыре adapter tensors; TRAIN1 correct tokens3/49→4/49 и terminal NLL ниже | Полные ответы и terminal exact остаются 0/2; response NLL и один rank ухудшаются. Это частичный учебный сигнал и ремонт capacity, не освоение задачи |

## 2. Первичные измерения: полезное и регрессии рядом

Все числа ниже прочитаны из уже сохранённых отчётов/JSON; это не результаты текущего поручения. Запуск был один на вариант, без рандомизированного повторного timing. `allocatedBytes` — суммарно выделенные байты в указанном окне, не retained memory и не working-set peak.

| ID / первичный receipt | A → B в одном сопоставимом окне | Не скрывать рядом |
|---|---|---|
| AGC: [report](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AGC1/results/report.md), [comparison](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AGC1/results/attempt-0001/comparison.json), [A metrics](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AGC1/results/attempt-0001/A/metrics.json), [B metrics](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AGC1/results/attempt-0001/B/metrics.json) | Выделения внутри step1,847,691,280→1,761,735,608: **−85,955,672 байта**, −4.652%; wall−2590.3569ms, CPU−859.375ms | GC0/1/2 каждый+4; retained после шага+28,461,768 байт.424 первых буфера/21,492,528float дают расчёт 85,970,112 байт; он близок, но не равен измерению |
| FBM: [report](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/FBM1/results/report.md), [comparison](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/FBM1/results/comparison.json), [A](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/FBM1/results/A/metrics.json), [B](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/FBM1/results/B/metrics.json) | Выделения внутри step1,847,680,544→1,837,788,968: **−9,891,576 байт**, −0.535%;40 вызовов/2,478,080 элементов карты | wall+201.296ms, CPU+1640.625ms, GC+2/+2/+3. trainingAllocatedBytes имеет другое окно и дельту−9,989,776; не подменять им step |
| RD: [report](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001/results/report.md), [A](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001/results/A/code-sign.json), [B](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001/results/B/code-sign.json) | Выделения 13,640,016→12,984,816: **−655,200 байт**, −4.804%; wall156.5045→105.9158ms, CPU140.625→109.375ms | peak41,893,888→42,139,648,+245,760 байт; GC2/1/0 прежние |
| AT: [report](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AT01/a001/results/report.md), [comparison](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AT01/a001/results/comparison.json) | TRAIN13,640,016→12,457,296 байт: **−1,182,720**, −8.671%; AFTER-readout8,203,240→7,558,120 | TRAIN wall119.3876→123.6514ms, CPU93.75→125ms. Выигрыш чтения после обучения учитывается отдельно |
| SILU: [report](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1/report.md), [raw](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1/raw.json) | wall22.1668→19.9589ms: **−2.2079ms**; убраны 14,784 повторных Exp | Выделения 13,639,760→13,698,992: **+59,232 байта**; CPU15.625ms прежний |
| CS: [report](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/a001/results/report.md), [comparison](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/a001/results/comparison.json) | wall110.3624→85.2964ms: **−25.066ms**; вычисленные score cells7,056→3,696 | Выделения 13,642,776 байт, CPU93.75ms и GC2/0/0 прежние; перенос на KV-cache не доказан |
| MB: [report](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MB01/a001/results/report.md), [result](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MB01/a001/results/result.json) | Выделения 13,642,496→12,782,336: **−860,160 байт**; убраны 21,504 массива из трёх int | wall74.0209→77.7742ms; CPU78.125ms и GC2/0/0 прежние |
| RPE: [report](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RPE01/a001/results/report.md), [A](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RPE01/a001/results/A/code-sign.json), [B](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RPE01/a001/results/B/code-sign.json) | Выделения 13,640,016→13,623,456: **−16,560 байт**, около−0.12%; построения 4→1, повторные чтения 0→3 | wall93.5708→110.8264ms, CPU78.125→109.375ms; кэш удерживает 1,344 байта. Малый выигрыш не округлять до нуля |
| AC: [report](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AC01/a001/results/report.md), [result](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AC01/a001/results/result.json) | wall71.1384→69.2569ms: **−1.8815ms**; CPU78.125→62.5ms | Выделения 13,642,496 байт прежние. NO_GAIN в отчёте опирался на время соседнего MB; это не повторный сопоставимый замер шума AC |
| SEXP: [report](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SEXP01/results/attempt-0001/report.md:80), [result](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SEXP01/results/attempt-0001/result.json), [study](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SEXP01/results/attempt-0001/study.json) | Четыре updates:39.6467→27.2809ms: **−12.3658ms**; CPU46.875→31.25ms; каждый B-step короче | Выделения 6,230,688→6,238,192: **+7,504 байта**. Загрузка 289.0267→133.912ms указывает на различие холодных затрат. Даже последние два шага не независимое доказательство причинного ускорения |

AGC/FBM сохранили конечные адаптеры, файл весов, step loss/margin, норму изменения и TRAIN-панель. RD/AT/MB/AC сравнивают веса, optimizer, losses и logits; SILU —896 обучаемых float,4loss и 1024logits каждого конца; CS — также logits всех позиций. Эти результаты копий исходников обосновывают будущий перенос, но не являются прохождением тестов продукта.

## 3. Точные данные, обученный старт и расход

Новые проверки используют сборки реального продукта из C:/Projects/llm_mvp_core. Старые src/A и src/B — только происхождение доказательств. Готовый старт восстанавливается, а обучение производителя не повторяется.

### H512: AGC и FBM

Донор: C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-postfix-one/v1. Файлы:bundle.json,generation.json,tokenizer.json,data.source.jsonl,data.material.json,authority.json,source-manifest.json,preparation.json,S0.weights.bin,S0.optimizer.bin,S0.rng.json.

[Generation](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-postfix-one/v1/generation.json) фиксирует V629/C2048/H512/L4/Q8/KV2/FF1408,RoPE10000,RMS1e−5;LoRA rank8/alpha16/dropout0/q_proj,v_proj;base/adapter seed20261014. Tokenizer/material bytes сохраняются без повторного BPE.

TRAIN — simulation-csharp-doc-f06-008:121chosen IDs,75prompt,46response targets. Исторически 46/46 до/после; это воспроизведение строки, не независимая генерация. Policy восстанавливает SFT1000; reference остаётся исходным frozenB0. **DPO начинает новый optimizer0**; SFT moments не импортируются. Файлы SFT optimizer/RNG — происхождение донора, не вымышленный DPO continuation.

Композиция AGC/FBM: B0 через исходные guards→policyS0→точный material→новые recipient/profile/content admission/lease/capability→[CreateForProjectLora](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:450). Это частные тестовые полномочия, не canonical parent. LR1e−5 и native default AdamW прежние. RNG задаётся одинаково из generation при dropout0; отсутствующий DPO snapshot не объявляется восстановленным.

На руку:1StepVector,4 учебных Forward,2Backward,2TRAIN-readout Forward. Пара: **2updates /12Forward /4Backward /0Generate /0 общих updates**. Step loss/margin измерены до update внутри StepVector; не называть их новым post-update loss и не добавлять ради него вызовов.

### H64: RD, AT, MB, RPE, SILU, CS, AC

V256/C64/H64/L1/Q4/KV2/FF176,RoPE10000,RMS1e−5;rank4/alpha8/dropout0/q,v;base62001/adapter62002. TRAIN: «C# result sign: 9-4 =»→«+» и «C# result sign: 4-9 =»→«-». EXAM: «C# result sign: 8-3 =»→«+» и «C# result sign: 3-8 =»→«-». Между prompt и answer **нет LF**;22UTF8-байта дают 21 сдвинутую позицию full-sequence SFT. Такой EXAM не доказывает общее правило арифметики.

| ID | Каталог под C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/ | RNG |
|---|---|---|
|RD,MB,AC|h64-rms-row/v1|rms-row-reduce-probe-v1|
|AT|h64-attn-transpose/v1|attention-transpose-probe-v1|
|RPE|h64-rope-table/v1|rope-training-table-probe-v1|
|SILU|h64-silu/v1|frozen-transpose-probe-v1|
|CS|h64-causal-scores/v1|frozen-transpose-probe-v1|

Набор 11 файлов тот же. Точные topology/LoRA SHA берутся из **назначенного** authority.json. Совпавшее имя stream не объединяет SILU/CS: topology соответственно c8d4a2f9fc2a338c39618bd8f76146c799d99c5385b2e2629dce1bb2d19403e2 и 8f58ee5df35f811e786804b8fef417d36461117404c24d8301af61548aa8e0ad.

Preparation зафиксировал 400updates,T1 W0false→S0true. TRAIN обычно 1/2 и совместим с constant baseline; ресурсный PASS не становится 2/2. Typed validation получает FullCheckpointLoraTopology(4,new[]{"q_proj","v_proj"}). Восстановить keyed AdamW moments/step400 и RNG этой семьи; не искать другого донора, не обнулять optimizer и не повторять 400updates.

[Trainer67](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:67) выполняет trainingForward117,loss/backward/AdamW135. Beta1=.9,beta2=.95,eps1e−8,decay0,LR1e−3,clip1,accum1,warmup0. Порядок T1,T2,T1,T2; [RD96](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001/Program.cs:96), [SILU787](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/Program.cs:787).

На руку 4updates+4BEFORE+4AFTER,optimizer400→404. Пара: **8updates /24Forward /8Backward /0Generate /0 общих updates**. Сохранение/восстановление проверяет уже измеренные logits и восстановленные массивы без новых Forward. Чистая ресурсная правка сохраняет численные результаты фикстуры.

### H16: SEXP

[Study](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SEXP01/results/attempt-0001/study.json) закрепляет PA01/repos/B и PA01/results/accepted-parent-handoff.json под C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO. SftAcceptedParent.PrepareStartLatest выбирает уже существующий accepted ordinal2. [Модуль 197–215](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/host/SftAcceptedParent.cs:197):H16/C64/L1/Q2/KV1/FF32/V256,rank2alpha4dropout0q,v,weights/moments/RNG,optimizer1000. Это частный accepted parent, не продуктовый canonical. Новый PA01/1000updates не назначены.

LR3e−4,totalSteps2000,warmup0,clip1,accum1,decay0;AdamW восстановленный. [Строки 79–85](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SEXP01/host/Program.cs:79): «// file: One.cs\nstatic int One()=>1;»,Two→2;EXAM Three→3,Four→4;CONTROL «// file: Pos.cs\nstatic bool Pos(int n)=>n>0;». Здесь \n — один LF, без EOS/добавочного LF. Длины 36/36/40/38/44 байта. Четыре updates:One,Two,One,Two,35 сдвинутых позиций/V256.

Начальный logicalstateSHA db44f1a437653299b345dc83c85523aad1ba1c55d3285865fe92501e5e78e386;3/40partial,full0/2. Пара: **8updates /28Forward /8Backward /0Generate /0DPO /0teacher /0tokenizer-training /0 подготовки**; на руку 4 учебных и 5+5 диагностических Forward,optimizer1000→1004. NLL4.766696420→4.765654100 одинаков в A/B;3/40 и 0/2 прежние. Полное освоение открыто; прежние подготовка PA01 и failedentries — происхождение, не разрешение повторить их.


## 4. Продуктовые изменения, формулы и точные тесты

Все пути относятся к C:/Projects/llm_mvp_core. Ниже назначены новые методы в Tests/ExperimentFixes/NeuralResourceContractTests.cs. Каждый ID имеет один Fact с Trait("PlanId","FIX-…") и Trait("Case","Contract"), который последовательно выполняет конечные подслучаи. Перечисленные формы, оси и подслучаи конечны; **Model.Forward, обучение и optimizer updates запрещены**. Лимит числа tensor operations не выдумывается: циклы выполняют ровно назначенные данные. У SILU дополнительно 14 скалярных вычислений для конечной разности. Существующие тесты служат источниками требований; их нынешнее падение или прохождение не утверждается.

### FIX-AGC — первая копия градиента

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE/LEARN по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

[AccumulateGrad984](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:984) вызывает new Tensor(gradData.ToArray(),shape), а [TensorStorage23](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorStorage.cs:23) опять копирует данные. Заменить только эту ветвь на new Tensor(gradData,shape); защитная копия остаётся. Последующие +=, порядок сложения и отмена прежние. Не присваивать внешний массив напрямую.

Формула первого накопления g:=d, следующего g_i:=g_i+d_i. AgcContracts:5 подслучаев — независимость первого grad[.25,−.5,1,−2] от последующей мутации входа; второе добавление[1,1,−1,−1]→[1.25,.5,0,−3]; граф x*x+x при x[−2,0,3]→grad[−3,1,7]; frozen input; pre-cancel. Сохранить требования [TensorCore237](C:/Projects/llm_mvp_core/Tests/TensorCoreTests.cs:237) и [ZeroGrad282](C:/Projects/llm_mvp_core/Tests/TensorCoreTests.cs:282). Native путь: DPO→autograd→AccumulateGrad→AdamW. H512-пара§3;4×число float объясняет payload, не задаёт platform-specific assertion.

### FIX-FBM — карта broadcast только при backward

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE/LEARN по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

[MultiplyBroadcast148–214](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:148). До выделения mappedBIndices вычислить a.RequiresGrad||b.RequiresGrad; карта существует только при true. Прямые индексы/умножения не менять. Frozen weight с дифференцируемой activation требует карту. Координатную оптимизацию FIX-RMS сюда не включать.

y_i=a_i*b_map(i); gradA_i=u_i*b_map(i), gradB_j=Σu_i*a_i по прежнему порядку. FbmContracts:10 подслучаев — формы[2,3,4]×[2,3,1] и×[4], четыре комбинации RequiresGrad для каждой; incompatible shape;pre-cancel. Значения−.7+.037*i, независимое аналитическое gold. Сохранить [PerRowScale696](C:/Projects/llm_mvp_core/Tests/LoraTests.cs:696) и gamma746. Измеренный потребитель — замороженный H512reference, а не произвольный training:false.

### FIX-RD — последовательная редукция последней оси

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE/LEARN по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

[SumAlongAxis1020](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:1020), потребитель [RmsNorm](C:/Projects/llm_mvp_core/03.TransformerModel/Layers/RmsNorm.cs). Для положительной последней оси contiguous storage суммировать rowBase+k в старом порядке float; остальные оси оставить прежними. KeepDims,shape,Mean division,epsilon,cancellation неизменны; SIMD/reassociation исключены.

sum_r=(((0+x[rD])+x[rD+1])+…). RdContracts:12 подслучаев —[2,3,4],[7],[2,5],[2,1]×keepDims true/false;axis0;negative last axis;invalid axis;pre-cancel. Gold — независимая последовательная сумма. Сохранить [Mean202](C:/Projects/llm_mvp_core/Tests/TensorCoreTests.cs:202), [RmsNorm35](C:/Projects/llm_mvp_core/Tests/TransformerModelTests.cs:35), [backward795](C:/Projects/llm_mvp_core/Tests/LoraTests.cs:795).655200 байт относится к измеренным 3RMS/update×21 строка×64 ширина и конкретному CLR.

### FIX-AT — транспонирование через strides

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE/LEARN по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

[Transpose710](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:710): identity и 2D-путь сохранить; generic rank вычисляет strides один раз, не выделяя FlatToIndices на элемент. Выход остаётся владеющим tensor, не view. Backward — обратная перестановка. dstIndex=Σcoord*dstStride,srcIndex=ΣswappedCoord*srcStride.

AtContracts:28 подслучаев — все i≤j rank3[2,3,4] (6) и rank5[1,2,3,2,2] (15);два negative-axis эквивалента;две invalid axis;pre-cancel;неизменность входа;attention roundtrip. Последний использует [Lora349–398](C:/Projects/llm_mvp_core/Tests/LoraTests.cs:349):B2/S3/H2/D4,x=−.7+.037*i,upstream=.013*(i+1)−.21. H64AT-family, отдельно TRAIN и AFTER-readout.

### FIX-MB — Mean backward без координат

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE/LEARN по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

[BroadcastGradAlongAxis1049](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:1049). Для last-axis keepDims с положительной шириной брать gradOut[i/axisSize]*scale; сохранить порядок накопления и generic fallback. RD forward не менять. Производная mean: dL/dx[rD+c]=dL/dmean[r]/D.

MbContracts:7 подслучаев —[2,3,4],upstream[−.6,.2,1.1,−.4,.8,.3] при нулевом и прежнем grad.1;axis0;axis1;keepDimsfalse;singleton;pre-cancel. Gold аналитическое. Сохранить Mean202/RmsNorm795. Донор h64-rms-row;21,504 массивов относится только к измеренному графу.

### FIX-RPE — ограниченный кэш частот обучения

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE/LEARN по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

[GetFrequencies31](C:/Projects/llm_mvp_core/03.TransformerModel/Layers/RotaryEmbedding.cs:31), [attention148](C:/Projects/llm_mvp_core/03.TransformerModel/Layers/CausalSelfAttention.cs:148), [diff](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RPE01/a001/one-factor.diff). Внутренний training-only accessor:offset0,1≤seqLen≤64. Public API,inference,KV/offset прежние. На экземпляр одна неизменяемая запись(seqLen,cos,sin);theta/dim уже неизменяемы. Атомарная публикация под lock, замена не мутирует таблицу старого графа; наружу изменяемый cached tensor не выдавать.

Частота определяется seqLen/theta/dim/offset; разные ключи не объединять. RpeContracts:9 подслучаев —1/21/64;21→22→21 с backward старого графа;65;offset1;invalid length;pre-cancel;два детерминированно чередующихся потребителя. Сохранить [offset14](C:/Projects/llm_mvp_core/Tests/RotaryEmbeddingPositionOffsetTests.cs:14), [rotary508](C:/Projects/llm_mvp_core/Tests/LoraTests.cs:508),odd dimension573. Retained cache — цена; inference-выигрыш не заявляется.

### FIX-SILU — знаменатель только при безопасном владении

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE/LEARN по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

[Silu1132](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:1132), [SwiGLU43](C:/Projects/llm_mvp_core/03.TransformerModel/Layers/SwiGluFeedForward.cs:43), [TransformerBlock46](C:/Projects/llm_mvp_core/03.TransformerModel/TransformerBlock.cs:46). Forward x/(1+exp(−x)); кэшировать denominator, backward s=1/denom и прежнее s+x*s*(1−s), без reassociation.

[Tensor.Data16](C:/Projects/llm_mvp_core/00.TensorCore/Core/Tensor.cs:16) изменяем. Поэтому старый public overload сохранить. Новый overload(Tensor,CancellationToken,bool reuseForwardDenominator),defaultfalse; training передать из TransformerBlock в SwiGLU, opt-in только для частного gate projection при training&&RequiresGrad и без мутации между forward/backward. Inference/cached/direct callers прежние; static experiment flag не переносить.

SiluContracts:6 подслучаев —stable-input parity;existing gradient;legacy mutation после forward;frozen;inference-false overload;pre-cancel. Gold/upstream из [Silu964](C:/Projects/llm_mvp_core/Tests/LoraTests.cs:964),x[−3.1,−1.2,−.35,0,.42,1.7,3.4]. Численный перебор ровно 14 дополнительных scalar-Silu,epsilon1e−3,tolerance2e−6/4e−4/4e−3. [Model parity930](C:/Projects/llm_mvp_core/Tests/LoraTests.cs:930) — требование, не назначенный дополнительный прогон.

### FIX-CS — только причинно видимые scores

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE/LEARN по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

[ScaledDotProduct263](C:/Projects/llm_mvp_core/03.TransformerModel/Layers/CausalSelfAttention.cs:263), [diff](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/one-factor.diff). Узкий causal-score kernel проверяет rank4,batch/head/inner dims и квадратность; сохраняет SIMD dot и порядок суммирования каждой видимой ячейки. Вычисляются j≤row; верхние нули прежняя mask превращает в−Infinity. Scale/mask/softmax, GQA и rectangular WithPast294 прежние.

Это не общий MatMul: backward допустим при нулевом upstream выше диагонали после маски. Узкий именованный public causal API допустим; не выдумывать IVT/assembly names. CsContracts:8 подслучаев —B1/H4/T1,3,21/D16;B2/H2/T5/D4;верхний upstream через mask;bad heads;nonsquare;bad rank. x=−.2+.007*i, сравнить fullMatMul→mask и causal→mask,probabilities/context/gradQ,K без модели. Сохранить [mask13](C:/Projects/llm_mvp_core/Tests/TensorOpsCausalMaskWithPastTests.cs:13), [backward639](C:/Projects/llm_mvp_core/Tests/LoraTests.cs:639). Модельный finite-difference878 здесь не запускается. Расчёт 4×4×21²→4×4×21×22/2 касается score cells, не всех FLOP модели.

<a id="q32-min"></a>
### FIX-AC — Q32. Убрать предварительную копию Softmax

В [TensorOps.Softmax](../../../../00.TensorCore/Core/TensorOps.cs) для непрерывной последней оси максимум и экспоненту читать из входа. Результат нормировать в собственном выходном массиве. Сохранить обычную ветвь других осей, градиент и отмену. Предварительная полная запись входа в выходную память в выбранной ветви больше не нужна.

**Почему переносим во вторую очередь.** В AC01 время составило 71,1384 → 69,2569 мс (−2,64%), выделения остались 13 642 496 байт. Ответы исходных плеч совпали. Разброс контрольного времени 2,883 мс превышает наблюдаемый выигрыш 1,8815 мс. Карточка обещает устранить лишнюю копию. Устойчивость выигрыша во времени пока неизвестна.

Взять [состояние AC01 после 400 обновлений](../../../../experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1) и урок T1 из его `data.source.jsonl`. Прочитать знак T1, выполнить один обычный шаг обучения внимания и снова прочитать знак. Достаточны конечная потеря, работоспособный backward и сохранённый выученный ответ. В корректной правке ответы могут совпадать со старыми. Удаление предварительного копирования видно в самой ветви. Прежние проверки Softmax можно использовать при падении. Время 71,1384 → 69,2569 мс — результат AC01 с разбросом больше разницы. Для перехода по реестру не требуются новый бенчмарк, десять новых тестов или побитовое равенство результатов обучения. [Опыт AC01](../../../../experiments-v2/RP01-MICRO/AC01/a001/results/report.md) · [Данные](../fixes-based-on-experiments/codex-158-learning-sets-transfer-coverage-2026-11-04.md#open-transfer-data).

<a id="q35-min"></a>
### FIX-SEXP — Q35. Использовать ту же экспоненту при вычислении градиента

В [Trainer.BackwardLoss](../../../../04.TrainingLoop/Trainer.cs) один раз на строку посчитать `exp(logit − max)` для каждого элемента. Записать значения в один рабочий массив на вызов, использовать их и для знаменателя, и при вычислении градиента. Для следующей строки заполнить массив заново. Сохранить масштаб, target и накопление градиента.

**Почему переносим.** SEXP01 на четырёх обновлениях уменьшил число вычислений экспоненты с 71 680 до 35 840. Измеренное время составило 39,6467 → 27,2809 мс, выделения выросли на 0,12%. Верных токенов осталось 3/40, полных ответов 0/2 в обоих плечах.

**Как подготовить старт.** В рабочем PA01/B у `000002/weights.bin` на момент сверки 43 499 байт. Манифест требует 43 500. Остальные четыре файла из списка `payloads` в этом каталоге совпали с манифестом. Для своего прогона скопировать `experiments-v2/RP01-MICRO/PA01/repos/B` в каталог попытки как `B`. В копии заменить только `training-artifacts/models/code-documentation/sft/rp01-ingress-h16/v1/checkpoints/000002/weights.bin` байтами локального объекта `.git/lfs/objects/ba/8e/ba8ec8c20250bc431da4bf9993ff22c508422cd7224b5c57cec33ce7b98c6e0e` из корня core. Передать корень этой копии в `SftAcceptedParent.PrepareStartLatest` с исходным `accepted-parent-handoff.json`. Если рабочий файл уже совпадает с этим SHA, загрузчик может взять исходный B сразу.

Восстановить принятый родительский шаг 1000 из [PA01](../../../../experiments-v2/RP01-MICRO/PA01/results/accepted-parent-handoff.json) через тот же загрузчик, что в [учебном примере SEXP01](../../../../experiments-v2/RP01-MICRO/SEXP01/host/Program.cs). Взять один урок `// file: One.cs` с ответом `static int One()=>1;`. Один SFT-шаг должен пройти по изменённому backward, дать конечную потерю и сдвинуть step оптимизатора на один. Рядом записать число верных токенов ответа до и после. В исходном опыте было 3/40 по панели и 0/2 полных ответов. Уменьшение числа вычислений подтверждено тем опытом, нового правильного полного ответа от этой оптимизации не требуется. При неверном градиенте проверить повторную запись буфера для каждой строки, target и scale. После адресной проверки перейти к Q34. [Первичный результат](../../../../experiments-v2/RP01-MICRO/SEXP01/results/attempt-0001/result.json) · [Данные](../fixes-based-on-experiments/codex-158-learning-sets-transfer-coverage-2026-11-04.md#open-transfer-data).

## 5. FIX-SC — последний target и вместимость входа

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE/LEARN по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

[Отчёт 152](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/results/attempt-0001/report.ru.md:152), [compareA](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/results/attempt-0001/compare/A/arm-result.json), [compareB](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/results/attempt-0001/compare/B/arm-result.json), [prepareB](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/results/attempt-0001/prepare/B/arm-result.json), [calibrateA](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/results/attempt-0001/calibrate/A/arm-result.json). Использован completed attempt-0001, не CRLF/failed roots.

Исторически T65 не входит в C64, а inputprefix64 предсказывает все 64targets. B200updates изменили 4adapter tensors; TRAIN1correct3/49→4/49,first20:2→3; terminalNLL улучшился на.027169/.004342. Fullresponse0/2,terminal exact0/2 прежние; двухкандидатная accuracy1/2 — constant baseline. ResponseNLL хуже на.000151/.000713,rank124→125 хуже,EXAMcorrecttokens0. Старый полный расход **400updates/1672Forward/808Backward** включал CalibrateA200; этот диагностический расход заново не назначается.

Единый фактор требует двух согласованных правок:

1. [DpoExactPairMaterial.Validate94–129](C:/Projects/llm_mvp_core/11.DPO/Models/DpoExactPairMaterial.cs:94): для exactT≥2 проверить T−1≤min(policyC,referenceC). Digest/admission/IDs/masks/tokenbounds прежние. T>C+1 отклоняется; не обрезать targets и не заменять input capacity числом scored labels.
2. [DpoTrainer1007](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1007): exactpolicy/reference получают prefix[0..T−1), граф хранит полные IDs/masks. Общий TargetRowCount проверяет logitrows=T−1; scoring/Backward используют target[i+1],responseMask[i+1] и прежнюю sum-normalization. Legacy nonexact сохраняет fullinput; последний target не исчезает.

Causal attention делает последний полный input ненужным для предыдущих logits. Исторические A/Bdiff — SC01/src/A/DPO и src/B/DPO в этих двух файлах. Helper проверяет rank до Dimensions[1]; unsafeordering не переносить. FIX-PD/NH одновременно в этот diff не добавлять.

Данные: [control](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/fixture-control.jsonl), [boundary](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/fixture-boundary.jsonl), по 2 строки. Prompt «// file: One.cs\n»/Two; controlchosen «static int One()=>1;\n// expected return value: 1»/Two→2; boundary меняет return на numeric. Rejected меняет последний 1↔2. JSONbytes/LF сохраняются, vocab256/no merges/noEOS; T64control/T65boundary.

[LoadModels421–453](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/src/Common/ContinuousLearning/Sc01ExperimentBridge.cs:421):H16/C64/L1/Q2/KV1/FF32/V256,rank2alpha4dropout0q,v,base777,LoRA20261020,stream restore-minimal-set. Донор C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/trained-weights.bin,43,500 байт; logicalstateSHA db44f1a437653299b345dc83c85523aad1ba1c55d3285865fe92501e5e78e386. FileSHA фиксирован savedrequest/prepare; не путать его с logicalhash. Донор SFT1000 частичен:3/40,full0/2; одинаковые policy/reference weights,referencefrozen,freshDPOoptimizer0. Нет SFTmomentimport/fakeDPOlineage.

### Минимальная будущая проверка без повторной калибровки

Fact ScTensorContracts: 6 конечных подслучаев без модели: TargetRowCount(T/T−1); последний gold отличается от предпоследнего;sum против mean;нулевая mask;несовместимая shape;legacy full. Фиксированный delegate вызывает настоящие private scoring/backward helpers на искусственных logits; seam одинаков A/B.

Fact ScLearning: тот же донор и 4readout — boundaryOne,boundaryTwo,EXAM «// file: Three.cs\nstatic int Three()=>3;» и «// file: Four.cs\nstatic int Four()=>4;». Каждое чтение — один policy Forward на prefix без последнего token и response mask. Это одинаковый observer A/B: он читает T65 через input64, хотя старый training admission его отклоняет. BEFORE/AFTER4+4 на руку; full/partial/terminal exact/rank/NLL берутся из тех же logits.

До updates один численный control **только controlOne** на руку: policy chosen/rejected и reference chosen/rejected —4Forward,2policy Backward,0updates. Сравнить полные A и shifted B градиенты с tolerance1e−6 (исторический max3e−7); затем очистить gradients и проверить неизменность weights/optimizer/RNG. Control не обучать. CalibrateA200 не запускать.

A делает одну попытку admission boundary: ожидается отказ до Forward/update; AFTER читает исходное состояние. B выполняет ровно 200StepVector,One/Two попеременно, native SC настройки/LR прежние, без добора. Overflow/digest/legacy guards проверяются без модельных вызовов. Это новый авторский конечный план, не уже измеренный сокращённый replay.

| Стадия | Updates | Forward | Backward |
|---|---:|---:|---:|
| A:control+4before/4after+ожидаемый refusal |0|12|2|
| B:тот же control/readout+200×4F/2B |200|812|402|
| **Полный будущий FIX-SC** |**200**|**824**|**404**|

Generate/teacher/tokenizer-training/подготовка=0. Новые Forward ради report/save/load запрещены. Прогноз 3/49→4/49 не становится обязательным gold; полный навык при 0/2 остаётся открытым. Время A0/B200 нельзя сравнивать как ускорение.


## 6. Будущие файлы и однозначный запуск выбранного ID

Обязателен [контракт 02](02-native-transfer-and-test-repair.md) вместе с [каноническим A/B](../fixes-based-on-experiments/03-test-and-execution-contract.md#native-ab-transfer). Рабочий каталог — C:/Projects/llm_mvp_core, без worktree и копии продукта. Будущий исполнитель использует модель своей сессии по умолчанию, без override и вложенного AI. Отчёт пишется по-русски. Исследователь сейчас программу не запускает.

| Будущий путь под canonical root | Назначение |
|---|---|
| Tests/ExperimentFixes/NeuralResourceContractTests.cs | Десять отдельных Facts из§4, без вызовов модели |
| Tests/ExperimentFixes/DpoShiftedContextContractTests.cs | ScTensorContracts и проверки отказов из§5 |
| Tests/ExperimentFixes/NeuralResourceLearningTests.cs | Одиннадцать Facts, по одному ID, точный расход§3/§5 |
| Tests/ExperimentFixes/NeuralResourceFixture.cs | Чтение закреплённых байтов, typed validation, восстановление, композиция полномочий и результаты; без копии learner/kernel |
| Tests/Fixtures/ExperimentFixes/v1/NeuralResourceDeltas/ID/manifest.json | Точные исходные paths/SHA256, профиль, данные, RNG/optimizer, бюджет; одинаковый файл в A/B |
| Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 | Явный dispatcher11ID, Baseline/Candidate, один запуск стадии |
| Tests.Support/ExperimentTransfer/Compare-NativeAB.ps1 | Чтение результатов и проверка identity/parity/counters/regressions; без вызовов модели |

Эти новые файлы **не созданы**. Проект: [Tests/LlmFromScratch.Tests.csproj](C:/Projects/llm_mvp_core/Tests/LlmFromScratch.Tests.csproj), Release/net9.0. Каждый Fact имеет Trait("PlanId","FIX-…") и Trait("Case","Contract") либо "Learning". Фильтр строго **PlanId=FIX-AGC&Case=Contract**, затем **PlanId=FIX-AGC&Case=Learning**; меняется только назначенный ID. Перед запуском dispatcher требует ровно один обнаруженный Fact на каждый фильтр. Ноль или два и более — ошибка без запуска. Никакой Theory с непрозрачным ID, wildcard класса или запуска всей сборки.

| ID | Contract Fact / подслучаи | Learning Fact / полный native расход A+B |
|---|---|---|
| FIX-AGC | AgcContracts /5 | AgcLearning:2U/12F/4B |
| FIX-FBM | FbmContracts /10 | FbmLearning:2U/12F/4B |
| FIX-RD | RdContracts /12 | RdLearning:8U/24F/8B |
| FIX-AT | AtContracts /28 | AtLearning:8U/24F/8B |
| FIX-MB | MbContracts /7 | MbLearning:8U/24F/8B |
| FIX-RPE | RpeContracts /9 | RpeLearning:8U/24F/8B |
| FIX-SILU | SiluContracts /6 и 14scalar probes | SiluLearning:8U/24F/8B |
| FIX-CS | CsContracts /8 | CsLearning:8U/24F/8B |
| FIX-AC | AcContracts /10 | AcLearning:8U/24F/8B |
| FIX-SEXP | SexpContracts /6 | SexpLearning:8U/28F/8B |
| FIX-SC | ScTensorContracts /6 | ScLearning:200U/824F/404B |

Всего 107 конечных подслучаев в полном списке для одного варианта. Запускается только выбранная строка в каждой из двух стадий A/B; только SILU дополнительно имеет 14 скалярных вычислений для конечной разности. Произвольный лимит операций внутри фиксированных tensor loops не вводится. Это не Model.Forward. Дорогие LoraTests878/930, DpoObjective195, TrainerTests71 **не назначены здесь к запуску**; они использованы как требования. Широкий regression suite требует отдельного явного конечного списка и расхода, а не бесплатного добавления.

[Native DpoObjective195](C:/Projects/llm_mvp_core/Tests/DpoObjectiveConsistencyTests.cs:195) подтверждает реальный StepVector и проверки allowed keys/reference. Test capability действительна только в test assembly. H512 использует original B0 guards→policyS0→material→recipient/profile→private admission/lease→CreateForProjectLora. Донор не передаёт лицензию. Full-checkpoint validation получает rank8/H512, rank4/H64, rank2/H16 и q/v; optimizer восстанавливается по реальным именам параметров, а не порядку обхода.

До A создаются общие harness/fixtures, фиксируется baselineCommit; production factor ещё старый. Порядок: build/discovery, Contract Fact, Learning A. Новый контракт может дать ожидаемый fail A; существующий fail скрывать нельзя. Затем один product diff, candidateCommit и те же проверки B. B-only API разрешается ленивым конкретным delegate/reflection; его отсутствие в A фиксируется, а не ломает компиляцию A.

Проектируемые команды **не выполнены**. Сначала требуется реализовать указанное расширение harness:

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-AGC -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit
# Только после отдельно разрешённого product diff:
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-AGC -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit -CandidateCommit $candidateCommit
~~~

Остальные десять ID используют ту же CLI с назначенным PlanId. Лимит 1800 секунд на стадию — авторский конечный предел, а не доказанное достаточное время. Нормальный wait своего процесса не ограничивается двумя чтениями; yield/handle не означает завершения. Не обещать уведомление без реального ожидания, не создавать фоновый monitor, постороннюю работу или новый attempt. По STOP завершить только свой процесс согласно 02; по прямому NEXT оставить цель открытой и не дописывать старые отчёты/проверки.

### Владение стадиями A и B

Назначен C:/Projects/llm_mvp_core/artifacts/fbe/ID/attempt-0001/{A,B}. Имена: source.json, fixture.json, calls.json, before.json, steps.jsonl, after.json, costs.json, state-write-status.json, S1.weights.bin, S1.optimizer.bin, S1.rng.json, completed.json. Это будущие пути.

**Завершённый A — обязательный вход B, а не причина отказать всему attempt.** Разрешено читать и переиспользовать проверенный completed receipt той же стадии, но не исполнять её повторно. Живой owner handle той же стадии ожидается; UNKNOWN диагностируется тем же владельцем. Чужая identity, несовместимые manifest/commit или незавершённые артефакты без подтверждённого владельца запрещают новый запуск. Их нельзя обойти attempt-0002 или новым корнем. B получает отдельный stage claim, привязанный к completed A и тем же fixture/harness hashes; A не перезаписывается.

Сначала захватить доступные terminal weights/optimizer/RNG и уже измеренные показания; сохранить независимыми CreateNew с отдельными ошибками; затем typed validation/readback; completed.json последним. Ошибка validation весов не должна лишить доступных moments/RNG. Для восстановления отчёта Train/Forward не повторяются.

## 7. Уровни проверок и изменение старых тестов

Unit подтверждает формулу, индексы, отмену и владение массивами. Native integration — реальную цепь learner→model/autograd→optimizer→сохранение/восстановление→чтение тех же показаний. Это E2E **внутри механизма обучения**, а не userprompt→domainrouter→ответ. Последний остаётся отдельным [планом 20](../fixes-based-on-experiments/20-prompt-domain-e2e.md); эти resource fixtures не требуют новых router fits.

Существующие тесты не объявлены падающими; их fixed gold и отрицательные случаи сохраняются. В FIX-SC меняется только допустимость exact T=C+1 с проверкой последнего target; legacy/overflow/digest/frozen state обязательны. В прочих факторах смена арифметического порядка — другой фактор, не повод ослабить tolerance.

Время и allocations не становятся жёсткими платформенными unit assertions. Полезные и отрицательные оси показываются отдельно. Full response, partial token и NLL не взаимозаменяемы. Ресурсное сохранение узкого навыка допустимо, но полное приобретение ответа этим не закрывается.

## 8. Охват и непроверенное

Адресно прочитаны TensorOps148–214/710–852/984–1065/1132–1163, Tensor16–70, TensorStorage1–27, RotaryEmbedding1–84, SwiGLU1–53, CausalSelfAttention130–168/263–320, TransformerBlock calls, Trainer67/117/135/389–426, DpoTrainer995/1007–1120/CreateForProjectLora, точные SC diff двух файлов. Тела existing tests: Lora349–398/696–745/795–830/930–1003, DpoObjective195–225. Другие имена найдены адресно; полное чтение или исполнение их не заявлено.

Прочитаны отчёты/целевые разделы и structured raw одиннадцати кандидатов; SEXP report1–166/result/study/one-factor.diff — позднее дополнение. Бинарные checkpoint не загружались в модель; ABI не проверялся запуском. Pins: AGC/FBM883496bca18113bf37e0afe683c75b05a0075048; RD/AT/MB/AC66947785d1318f7cbbc88118f3156cdc75001e16; RPE0b76ab950d82534f5904005dda096545505ae307; SILU/SEXPfb9175cc752d41e1cb525e0609b7c96140940217; CS26332500f76ea8345256c73ecfd44c0023b7e883. Это не будущие baseline commits. Parent inventory называл ff6ed114; worker не публиковал Git.

TARGETED_READ skills под C:/Projects/llm_mvp_core/docs/skills: architect-behavior-classifier26–52, planning-session-startup26–50, phase-planning26–50, plan-review40–65, code-examination41–67, generated-code-review54–78, prompt-patch-authoring63–100, output-readability-gate1–6, human-communication-GLOSSARY5–25. Это не полный reload. Исторические указания — evidence; текущая фаза определяет полномочия. Таблицы не «циклы проверки». ContextMode чтение не завершилось, использован ограниченный файловый анализ; усечённые outputs не выданы за полное чтение.

Не исполнены: одиннадцать переносов, build/discovery новых tests, native A/B, широкая регрессия, переносимость timing, полное прикладное E2E и освоение ответов. Никакая будущая проверка не помечена PASS.
