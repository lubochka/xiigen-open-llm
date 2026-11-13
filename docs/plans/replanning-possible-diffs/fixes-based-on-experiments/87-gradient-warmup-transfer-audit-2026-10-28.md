# 87. Аудит P05, P10–P12: приёмка результатов и первый шаг SFT

28 октября 2026. Фаза **READ_ONLY_RESEARCH + Markdown**. Срез HEAD: `b47df140494fbcb0389c3aebb314c74bbeeef9d6`. Прочитаны исходники, сохранённые результаты, изменения коммитов и существующие тесты. В этой работе не запускались сборки, тесты, обучение или inference; код, веса и старые результаты не менялись. Реализация следующих исправлений — [план88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md); применимость к остальным владельцам — [89](89-gradient-warmup-card-coverage-2026-10-28.md).

Различие стартовых состояний существенно: P10 использует fresh DPO AdamW0, P11 — fresh embedding AdamW0; **P12 импортирует SFT moments и AdamW step400**, а отдельный scheduler counter начинает с0 (`P1201/Program.cs:465–479`). Восстановление P12 как fresh optimizer изменило бы второй фактор. Именно это несоответствие найдено работником `/root/p08_transfer_audit` в первой редакции плана88 и исправлено до передачи исполнителю.

## Что внесено в действующий код

| Коммит / опыт | Подтверждённый результат | Решение о переносе |
|---|---|---|
| `cb3c9dc172d4f7ead06e30c2e2836a1908a255af`, P05 receipt | Изменены действующий writer опыта, общий для host/tests builder, тесты и отдельная корректировка старого receipt | Основной дефект «сдвиг весов означает квалифицированное обучение» исправлен для сохранённого P05. Ниже — новые непокрытые входные/статусные случаи |
| `27d296aaa155263597f524d84287aa5b68eec422`, P10 | Один native DPO update; производные двух q/v координат согласовались с finite difference на трёх epsilon | Нового исправления DPO не требуется. Результат поддерживает перенос проверки в existing test suite, а не замену работающей производной |
| `a680a8453b3615097f0abdcbe538baae5d777be3`, P11 | Native embedding backward работает; абляция разрывает связь. Изменены17 достижимых строк и1088 координат | Native B уже существует. Embedding-only обучение не допускается как общий quality default: E2 потерян, ответы стали постоянными |
| `f44195d55ea681eeaf3e215e4d13293f0207fe2a`, P12 | Прямой SFT Trainer с WarmupSteps=4 отказывает на первом LR=0; экспериментальный override только step0 даёт четыре updates | Кандидат — точечное согласование первого SFT шага с его допустимым LR. Общий сдвиг scheduler на step+1 не измерен |

P10/P11/P12 коммиты добавляют experiment hosts/receipts и записи реестра; нового native исправления этих механизмов в них нет. Очередь Q011→Q014 фиксирует выполнение/навигацию, не готовность продукта. P09 и исправления P02 уже разобраны в [84](84-reporting-sampling-cache-transfer-audit-2026-10-27.md); новые перечисленные коммиты не закрывают их остатки [85](85-reporting-sampling-cache-repair-plan-2026-10-27.md).

## P05: основной дефект исправлен, проверка целостности панели неполна

Источники: [builder](../../../../experiments-v2/RP01-MICRO/P0501/src/P0501LearningQualificationReport.cs), [Program](../../../../experiments-v2/RP01-MICRO/P0501/Program.cs), [новые тесты](../../../../Tests/P0501LearningQualificationReportTests.cs), [отдельный corrected receipt](../../../../experiments-v2/RP01-MICRO/P0501/results/attempt-0001/audit-correction-79/corrected-receipt.json), [объяснение](../../../../experiments-v2/RP01-MICRO/P0501/results/attempt-0001/audit-correction-79/README.md).

Подтверждённый ремонт: writer вызывает `P0501LearningQualificationReport.Build`, выводит отдельную квалификацию групп, а не только `Delta.MaxAbsolute>0`. Сохранённый опыт честно имеет8 настоящих updates, нулевые ответы прежнего полного reader и `INVALID_UNQUALIFIED_BEFORE / INVALID_ZERO_TO_ZERO`; цель OPEN. Восстановление первого знака из уже записанной строки отделено от освоения слов positive/negative; новых model calls для него нет. Это закрывает соответствующую **основную** часть ремонта80, не саму учебную цель P05.

**R29-Q1 — одинаковый размер не гарантирует ту же панель.** Builder:257–259 проверяет только равенство before/after counts. `BuildGroups` строит dictionary только для before, затем обходит after; сверяет наличие ID и gold, но не уникальность after ID, равенство множеств ID или Kind. Статический контрпример: before `[T1 correct,T2 wrong]`, after `[T1 correct,T1 correct]`. Размер2 сохранён, оба поиска T1 успешны, одинаковый gold проходит; писатель получит2/2, потеряв T2. Изменение Kind при том же ID также меняет квалификацию удержания без отказа. В текущих сохранённых данных такое повреждение не установлено; найден пропуск в fail-closed контракте и его тестах.

**R29-Q2 — годное сравнение не равно закрытой учебной цели.** Builder:288–300 вычисляет `realTraining` отдельно, но условие `goalClosed` не использует ни его, ни terminal outcome. Два плеча с ненулевыми корректными ответами до/после, CompletedUpdates=0 и одинаковыми weights удовлетворяют условию закрытия цели. В receipt одновременно возможны `real_training_performed=false` и `CLOSED_FOR_NAMED_CONSUMER`. Аналогично не исключён незавершённый executionStatus. Это логический контрпример по исходнику, не новый запуск и не утверждение о неправильности уже исправленного P05 receipt.

Ремонт должен сохранять различие: сравнение1→0 может быть валидным измерением регрессии. Нельзя требовать улучшения, чтобы признать данные валидными, или скрывать неудачное обучение за INVALID. Требуются отдельные completeness, learning-performed, behavioral qualification, hypothesis и transfer-admission.

## P10: подтверждённый native градиент и неточная числовая формулировка

Источники: [Program](../../../../experiments-v2/RP01-MICRO/P1001/Program.cs), [derivative comparison](../../../../experiments-v2/RP01-MICRO/P1001/results/attempt-0001/derivative-comparison.json), [metrics](../../../../experiments-v2/RP01-MICRO/P1001/results/attempt-0001/metrics.json), [report](../../../../experiments-v2/RP01-MICRO/P1001/results/attempt-0001/report.md).

Факты: native StepVector на T1, fresh DPO AdamW0→1, reference и frozen base неизменны; donor SFT optimizer400 не импортирован в новую DPO эпоху. Бюджет36 model Forward,4 Backward (2training+2control),1update,8generation соблюдён по записанным счётчикам. T1 верен до/после; accuracy2/4. T1 margin1,861236→1,874308 — полезный локальный сдвиг, не доказательство общего улучшения DPO или всего autograd.

Математическая связь: при нулевых начальных моментах после первого AdamW шага `m1=(1−β1)g`, `v1=(1−β2)g²`. Native путь не делает clipping; после снятия vector multiplier1,0833334 recovered gradient сопоставим с central difference того же unscaled scalar. Проверка второго момента поддерживает согласованность optimizer state, но не является второй независимой finite-difference программой. Две заранее назначенные координаты и однотокенный response не доказывают все веса, многотокенные маски или все схемы нормировки.

**R29-D1 — агрегаты отчёта должны происходить из raw.** Повторная арифметика по шести сохранённым сравнениям даёт max absolute error `0,000026932268261364856`; минимальный запас к absolute tolerance1e−4 — `3,7130181175`, а не «не менее четырёх раз». Фраза коммита «2,6e−5» тоже округляет вниз. Все6 сравнений действительно проходят порог; исправляется описание запаса, не verdict согласия. Никаких новых нейронных вычислений для этого не требуется.

## P11: работающий backward, неприменённый контроль и скрытый общий бюджет

Источники: [native Embedding](../../../../00.TensorCore/Layers/Embedding.cs), [Program](../../../../experiments-v2/RP01-MICRO/P1101/Program.cs), [arms](../../../../experiments-v2/RP01-MICRO/P1101/results/attempt-0001/arms.json), [metrics](../../../../experiments-v2/RP01-MICRO/P1101/results/attempt-0001/metrics.json), [report](../../../../experiments-v2/RP01-MICRO/P1101/results/attempt-0001/report.md), [oracle](../../../../experiments-v2/RP01-MICRO/P1101/src/ScatterAddOracle.cs).

Native `Embedding.cs:59–77` связывает выход с weight и суммирует по одинаковым token IDs. Формула `∂L/∂W[r,c]=Σ(position:id=r) upstream[position,c]` объясняет17 достижимых строк и нулевые отсутствующие строки. Оракул дал0 расхождений из16 384; finite difference9/9 с отдельным допуском relative0,15 или absolute2e−4. B — нынешний продукт; A — намеренная detached абляция, не текущая поломка embedding.

**R29-G1 — вычисленный контроль не управляет решением.** Program:626–673 получает `differencesAgreed`; :822–847 формирует hypothesis/exit, не включая это значение. Поэтому контрпример8/9 вместо9/9 не помешал бы успешному завершению при прежних остальных guards. Также guardPanelAUnchanged и результаты factor proof должны быть явно распределены между обязательными gates и informational diagnostics. В текущем raw finite difference9/9, поэтому сам сохранённый математический результат не опровергнут. Исправлять terminal reducer и его тесты, а не рабочую формулу Embedding.

**R29-G2 — прежняя ошибка квалификации повторена.** Program:825–827 объявляет `VALID_FOR_NAMED_UNIT` из изменения таблицы и сохранности frozen base, не из реально прочитанной панели. Отдельно признавать structural gradient reachability, actual training и качество consumer. Во фактическом B E1 стал верным, E2 стал неверным; все четыре ответа теперь `+`. Общие2/4 скрывают потерю прежнего E2 и совпадают с constant-answer baseline2/4. Это валидно наблюдённая регрессия предметного поведения, а не повод вычеркнуть успешную проверку scatter-add. Для интеграции embedding-only политики потеря E2 существенна; общего quality допуска нет.

**R29-B1 — основной счёт не включает обязательные controls.** Program:691–693 и metrics.budget.held_exactly проверяют8training Forward,16generation и16scoring, но исключают19control model Forward/1Backward и3layer Forward/1Backward. Полный наблюдённый счёт: **59 model Forward**,9model Backward, отдельно3embedding Forward/1Backward. Из8 вызовов optimizer только B имеет4 ненулевых изменения таблицы; у A4 обращения/шага, но0 effective parameter updates. Записанные «8updates» нельзя без единицы выдавать за8 изменений весов. Контроли не скрыты в raw, однако общий budget gate их не защищает. Пределы и aggregate writer нужно исправить без повторения обучения.

Старый первичный паспорт P11 называет exact response masks, хотя фактический SFT loss full-sequence; смешивает общие q/v настройки с исключением embedding-only; обещает отдельный FD tolerance «ниже», не задавая этот численный допуск в PRIMARY. Исторический FD результат сохраняется с фактическим порогом, не получает задним числом preregistered label. В новом рецепте эти сведения задаёт автор до запуска.

## P12: проверен точечный первый шаг, не глобальный перенос расписания

Первичные строки, scope и проверка native hashes — [пакет исследователя](reviews/2026-10-28-warmup-first-update-source-audit.md). Native targets: [Trainer](../../../../04.TrainingLoop/Trainer.cs), [scheduler](../../../../04.TrainingLoop/Schedulers/CosineWithWarmup.cs), [IncrementalTrainingSession](../../../../14.ContinuousLearning/Training/IncrementalTrainingSession.cs), [отчёт P12](../../../../experiments-v2/RP01-MICRO/P1201/results/attempt-0001/report.md).

Trainer:117 выполняет Forward,125BackwardLoss,126clipping и только128–132 читает LR и отвергает0. Scheduler:28–35 при warmup>0 возвращает0 на step0. Поэтому прямой supervised путь с WarmupSteps=4 делает дорогую работу и отказывает до AdamW. Плечо A:1Forward/1Backward/0updates; B:4/4/4 после override **только step0** на0,00025. Это измеренное устранение отказа первого урока; сравнение доз несимметрично, потому что A остановился. Называть B более эффективным обучением при равном числе updates нельзя.

**R29-W1 — предлагаемый перенос шире измеренного.** Report предлагает `(step+1)` во всём scheduler или `GetLr(step+1)` в SFT. Оба сдвигают все warmup шаги. Проверенный B оставляет step1 и далее исходными. Точный экспериментальный ряд при W4: `[0,00025;0,00025;0,0005;0,00075]`; global shift даёт другой ряд. Для общего сдвига нет измеренного A/B доказательства. Сначала планировать измеренное исключение первого прямого supervised шага и независимую раннюю проверку невалидного LR.

**R29-W2 — область настоящего consumer.** `IncrementalTrainingSession.cs:403–407` уже принудительно задаёт WarmupSteps=0, поэтому P12 не доказывает отказ обычного continuous-learning пути. Pretraining Trainer тоже нельзя отождествлять с этим SFT: он может выполнить AdamW при LR0, обновив moments/step без сдвига weights. Фраза «урок выброшен целиком» относится к отказавшему прямому supervised вызову, не ко всем путям обучения. Сохранить низкоуровневые проверки невалидного LR; не чинить успех теста удалением guard.

**R29-W3 — восстановление должно иметь артефакт.** В P12 attempt нет сохранённого S1 weights/optimizer/RNG, только панели и hashes/сводки. Hash не заменяет checkpoint. План не может предписать «восстановить этот S1» без существующего файла. Ремонт отчёта делается по raw без model calls; будущий native runtime regression либо использует отдельно подтверждённый checkpoint, либо явно повторяет ограниченную исходную учебную программу, сохраняя её state для следующих тестов.

В B точность остаётся2/4; E1 margin−0,47885656→−0,49445748 (ухудшение по модулю≈3,258%), P(gold) уменьшается≈0,276%. Это ниже порога5%, но показывается рядом. Раздельные ресурсы A/B не измерены, whole-process peak/alloc нельзя объявить регрессией или отсутствием регрессии B. CPU исключён из запрета по правилу69. Приоритет — точечный логический ремонт прямого SFT и диагностики; общий warmup default не допущен.

## Граница аудита и дальнейшая работа

Положительные результаты P10/P11 сохраняются как доказательства работающего продукта и основания усилить его тесты; новый алгоритмический FIX им не придумывается. В отчётах различаются validity сравнения, useful metric gain и product admission. Карточки остальных владельцев получают требования фактических gates, полного бюджета, стабильной панели и точного потребителя; это не повторный source-аудит каждого из62 ID.

Локальная рабочая копия содержит чужие незакоммиченные изменения; они сохранены. Документные дополнения не означают, что перечисленные repairs уже внесены в C# или проверены runtime. Новые эксперименты/исправления выполняются только после прямого поручения соответствующей стадии.
