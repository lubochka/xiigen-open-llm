# Диагностические карточки: обязательное обучение и проверка TTQ02

Дата: 2026-10-27. Worker: `/root/repair_numerical_audit`. Фаза — READ_ONLY_RESEARCH + назначенные Markdown. В этой работе не запускались build, tests, inference, training или experiment hosts; прочитаны исходники, git и сохранённые результаты. Программы ниже — будущие задания, не выполненные опыты.

В десяти новых активных P-паспортах разрешение на нулевой учебный бюджет заменено настоящим native обучением с конечными дозами. С учётом исторического P25 alias, ошибочно допускавшего пропуск новой учебной программы, затронуто11 разрешений:10 новых PRIMARY и1 старый alias. [Документ36](../36-diagnostic-learning-programmes.md) содержит общую таблицу рецептов, учёт всех Forward, admission chain и отрицательные контроли. Это изменение планов; результаты нового обучения ещё не получены.

## Что прочитано и что именно исправлено

Исходный исследованный срез: commit `846144464a856cd3f118bcfc0165c8117b06c167`, содержащий TTQ02. При чтении checkout сдвинулся чужими коммитами через `25fa8a1aca58612010dcb9caa3e646930a49bda0` к `75107af930a1d4c55d60fa8a4ef7c01643f13cb0`. Worker не делал staging/commit/push. Исторические формулировки далее относятся к исходному срезу, а не приписываются уже исправленному файлу.

Сначала целиком прочитан [активный executor](../prompts/P-SERIES-CONTINUE-executor.md), затем51 строка [контракта34](../34-P-experiment-execution-contract.md). На исходном срезе executor:18 говорил «Для reader после обучения новый Train не нужен», контракт34:30 — «Для reader-only опыта достаточно доказанно обученного S0». Прямое уточнение Любы требует настоящего обучения в каждой новой программе; старый trained checkpoint может быть стартом. Родитель исправляет эти общие поверхности, индекс и TTQ-карточку; этот worker их не редактировал.

В каталоге найдены125 P-карточек. По всем125 прочитаны верхние паспортные поля/результаты целевого поиска исключений; полностью и адресно разобраны верхние паспорта десяти строк ниже, донорские recipes и указанные native источники. Это не полное чтение каждого исторического приложения и не125 независимых экспериментальных проверок. AGENTS/CLAUDE и применимые repo skills читались адресно; полного reload всех больших документов не заявляю.

| ID / семейство | Прежнее разрешение в активном паспорте, строка14 исходного среза | Новая учебная программа и точный бюджет | Что этим ещё не доказано |
|---|---|---|---|
| P08 / граница sampling | 0 updates; запрет общего before/after learning loop | H64-RD: T1,T2,T1,T2, AdamW400→404, LR.001;12 Forward/4 Backward/12 samples | Качество sampling не следует из изменения support |
| P09 / cache/full | 0 updates; только12 reader Forward | H64-RD;≤20 Forward/4 Backward, cache/full на общем S1 | Совпадение reader не доказывает обобщение |
| P75 / residual | Updates0,4prefill | H64-RD;12 Forward/4 Backward/12 samples | Controlled residual draw не оценивает реальную частоту |
| P81 / input conditioning | Updates0; native reader «без обучения» | H64-RD;≤20 Forward/4 Backward;4 before и6 after prompts | Один обученный знак не доказывает двустороннюю зависимость от input |
| P99 / unknown ID | Updates0; только4prefill и16decode | H64-RD;12 Forward/4 Backward/16decode attempts | Fault ID256 введён экспериментально; естественная генерация не заявляется |
| P100 / ties | Updates0;4Forward/12samples | H64-RD;12 Forward/4 Backward/12samples | При отсутствии ties в actual logits эффект на actual answers не доказан |
| P10 / composed gradient | 0 updates; FD без optimizer | Native DPO T1, fresh AdamW0→1;≤36 Forward/4 Backward (2train+2control)/12FD evaluations | FD доказывает производную конкретного scalar, а не semantic gain |
| P14 / per-side provenance | 0 updates; roundtrip/materialization без consumption | Native StepVector на actual материализованной T2 pair;≤2updates/4Backward/20Forward, B update обязателен | Сохранность provenance и качество модели имеют разные verdicts |
| P57 / semantic measurement | 0Train/0Load/0Flow; immutable old B-after outputs | Original P125 native B Train:1call/8lessons/48Flow/104scorer verdicts | Улучшение scorer не называется улучшением модели |
| P102 / teacher forcing | Updates0; только restore/read | Original AGC1 single DPO update из H512 S0;≤192Forward/2Backward;4rollouts≤46tokens | Teacher-forced accuracy не заменяет собственный ответ |

В карточках заменены сами действующие budget/start/native paragraphs; это не только верхняя оговорка. Historicals ниже PRIMARY сохраняются как история, а не zero-training разрешение. При readback у всех десяти паспортов есть ссылка36; прежние формы `0 updates`, `Updates0`, `0 Train/0Load`, запрет before/after, `без новых updates` и `Только restore/read` в этих активных секциях отсутствуют. Этот текстовый readback подтверждает контракт документа, не готовность runtime host.

**Дополнение P25, владелец правки — родитель.** Старый P25-enru-cold-01 делал0Train/24Flow. Его исторический readback сохраняется; использовать этот alias как разрешение пропустить новую программу нельзя. Родитель назначает [P25-LEARNED-RESTORE-02](../cards/P25-cold-restore.md), отдельный Q125 без перенумерации прежних124: тот же MEMORY-B writer,1Train/8lessons,24cold-before+24afterTrain+save/load fresh+24afterRestore=72Flow. Neural updates неприменимы; historical19/24 не повышается и не заменяет новый результат. Этот worker P25/index не редактировал; строка фиксирует согласованную новую постановку, не её выполнение.

## Оригинальные источники, совместимость и смысл обучения

| Source / recipe | Подтверждено чтением | Применение и граница |
|---|---|---|
| [RD01 Program.cs:118](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001/Program.cs:118), :141–142, :325–354 | H64 SFT400 preparation, acquisition predicate false→true; original continuation4, LR.001, T1/T2, AdamW .9/.95/1e-8 decay0, total404 | Шесть reader-карточек продолжают проверенный bundle без новых400 bootstrap; restored moments400 и RNG нужны, fresh AdamW0 меняет программу |
| [AGC1 bridge:525](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AGC1/Common/ContinuousLearning/AgcExperimentBridge.cs:525), :548; [DpoTrainer:363](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AGC1/Common/DPO/DpoTrainer.cs:363) | Fresh DPO optimizer0, single StepVector, LR1e-5, decay.01; SFT moments не импортируются | P10/P14 используют этот native учебный механизм в новой явно назначенной H64 постановке; это не существовавший оригинальный H64 DPO run. P102 продолжает original H512 |
| [AGC1 bridge:1007](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AGC1/Common/ContinuousLearning/AgcExperimentBridge.cs:1007), :456 | Original reference остаётся canonical B0; только policy загружается S0 | P102 сохраняет это различие. Общий reference S0 изменил бы DPO loss; такая подмена устранена в окончательном рецепте |
| [AGC1 bridge:760](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AGC1/Common/ContinuousLearning/AgcExperimentBridge.cs:760), :484–548 | Rich pairs→governance→content authority→exact material; recipient/profile; server-owned lease; mutation capability; CreateForProjectLora→StepVector | В36 записана вся штатная цепочка. Public constructor без capability не равен доступному native пути. P14 сохраняет actual LocalModel rejected, не копирует обе AGC1 DeterministicGenerator константы |
| [P125 harness:48](../results/2026-10-18/P125-enru-memory-01/harness/Program.cs:48), :61–76 | Один B TrainStructuredBatchAsync,8rows, promoted owner, ordinary Flow; это реальное обучение памяти | P57 повторяет только нужный B trajectory из пустого state. Neural optimizer/LR неприменимы; старый snapshot остаётся свидетельством исторического обучения |

Restore достаточен для доказательства происхождения стартового состояния, для отдельной проверки сохранности checkpoint и для исторического readback. В новой экспериментальной программе он недостаточен вместо назначенного обучения. H64 preparation доказывает приобретённый T1, а не T2/EXAM; H512 preparation — один postfix lesson, а не x20. Если такого evidence нет или bundle не совпал, нельзя закрыть строку random 0→0: нужен повтор оригинальной подготовки с прежним acquisition gate, отдельным бюджетом и разрешением на запуск.

В других просмотренных паспортных полях слово «frozen» или neural updates0 не автоматически означает отсутствие обучения:

| Семейство/ID | Почему не включено в десять zero-training исключений |
|---|---|
| P23/P69/P92/P115 | Random/frozen neural encoder сочетается с настоящим native memory fit; это обучение памяти, а не нейросети |
| P56 | Обучаемое правило; neural optimizer не является его writer |
| P62 | Два урока уже входят в новую программу, дальнейшее чтение не обязано снова обучать |
| P76 / P110 | Общая учебная часть400 / 2400updates имеется; ноль дополнительных шагов в reader не обнуляет её |
| P86 | Исторический component был без обучения; новый primary задаёт1update/arm |
| P95/P97 | Два native fits по2rows; отдельные frozen units не отменяют fit |
| P123/P124 | Native KNN fit является учебным механизмом; scaffold diagnostics отдельно |
| P125 и исторический P25 readback | P125 B/C имеют реальные native lessons; поздний Load/Flow0Train читает уже обученную память. Исторический результат не стирается. Но новый P25 нельзя закрыть старым zero-training alias: ему назначен отдельный P25-LEARNED-RESTORE-02 с настоящим Train |

Эта таблица — разбор найденных формулировок, не заявление о достаточности качества всех остальных115 программ.

## TTQ02 / Q001: что подтверждает commit8461444

Прочитаны local git show, [host](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TTQ02/Program.cs), [report](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TTQ02/results/attempt-0001/report.md), metrics.json, training-progress.json, per-position.json, before-correction records и run.log. Сопоставлены оригинальные [N63CollisionProbe:117–150](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs:117). Сведения о выполнении относятся к сохранённому чужому run; worker его не воспроизводил.

| Проверяемое утверждение | Source/raw | Узкий вывод |
|---|---|---|
| Повторилось реальное original обучение | TTQ host:349–389; original N63:117–150: train IDs Encode(prompt+LF+chosen), LR.003, batch1, seq2048, max60, checkEvery10, AdamW .9/.95/.01/1e-8, clip1;60 loss entries и6 progress records | Код вызывает native Trainer и сохранённые записи согласованы с60updates. Нового независимого runtime доказательства здесь нет |
| Stop predicate исправлен к оригиналу | Original проверяет exact DPO ChosenIds/response mask; TTQ:379–389 exact read; before-correction:10losses,1progress, T1=21/23 | Joint training framing и exact DPO exam различны; joint23/23 не означает exact23/23. Исправленный run заканчивается по60 limit, поскольку exact22/23 |
| Непустое изменение на TRAIN | Exact per-position after:22 correct из23 против0 before, terminal true; loss7.759556→.00084102585 | Настоящее частичное усвоение учебной строки; полного correct response23/23 нет |
| Перенос и контроль | Same-rule-other-name0→0/29, unrelated0→2/21; обоим terminal false | 0→0 не является retention/transfer. Квалификация terminal≥2rows не пройдена:1/3 |
| Индексация reader | В18 сохранённых panels (3states×2views×3rows) сумма correct равна stored correct_tokens; predicting_row=target_index−1; ровно1terminal каждой строки | Внутренняя согласованность raw подтверждена разбором JSON; logits заново не вычислялись |
| Совпадение обученного и saved_auth | Digest `9dc0eb81b270357869b17513bc483387e8b24367dbbab193a4699bb5fc9f3804`; exact/joint per-position counts совпадают | Совпала описанная ниже проекция параметров по записи run; это не full checkpoint/optimizer/RNG byte identity |

**TTQ-F1, P2 — неполный счётчик стоимости.** [Program.cs:260](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TTQ02/Program.cs:260) увеличивает `forwards` только в ReadRow; [Trainer.cs:117](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:117) выполняет ещё Forward/Backward/optimizer для каждого TrainSupervisedNextTokenStep. Поэтому corrected run:24 diagnostic +60 training =84total Forward,60Backward,60updates. Сохранённый before-correction:19diagnostic +10training =29Forward,10Backward/updates. Два сохранённых результата вместе:113Forward,70Backward/updates; не путать с одним corrected run. Report:95 и commit «24 прямых прохода» без области счётчика недосчитывают обучение.

Логический контрпример без запуска: увеличить maxUpdates при неизменном числе ReadRow; cost training растёт, поле forwards_performed не включает её. Existing oracle проверяет позиции и совпадение значений, не учёт вызовов Trainer. Будущая правка host/report: отдельные diagnosticForward,trainingForward,totalForward,backward,updates с sum invariant; сохранять attempt identity и отдельный ledger повторов. Приёмка: текущие сохранённые записи объясняются84 и29, а24 остаётся честным diagnostic count. Product source менять для этого не требуется; чужой TTQ host/report сейчас не редактировался.

**TTQ-F2, P2 — «побайтовое состояние» шире StateDigest.** [Program.cs:185–195](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TTQ02/Program.cs:185) пишет подряд только float Data из GetCheckpointParameterEntries. В digest нет parameter names/shapes/RequiresGrad, optimizer moments/step, RNG, tokenizer/config. Report:32/117/130 и commit расширяют его до state byte identity.

Логический контрпример без запуска: два объекта с одинаковыми parameter Data и разными AdamW moments либо RNG cursor имеют одинаковый StateDigest. Existing oracle сравнивает именно эту проекцию и panel scores, поэтому такой вариант не ловит. Будущая правка: назвать поле parameter_data_digest и ограничить вывод этим сравнением; для отдельного full-state claim сохранять и сравнивать именованные tensors/metadata, optimizer schema+keys+step+moments, RNG entries и bundle identities. Приёмка: либо узкая честная формулировка без full-state claim, либо независимые сравнимые артефакты всех названных компонентов. Текущий fresh trained checkpoint/optimizer/RNG bundle в result inventory не предоставлен; их независимое byte comparison здесь не выполнено.

Дополнительное уточнение отчёта: строки129/142 «запущен один раз» могут описывать corrected launch, но рядом сохранён отдельный before-correction учебный результат. Следует явно назвать эту область и два observed training outcomes; не стирать первый и не считать70updates бюджетом единственного corrected run.

Правильный итог Q001: результат записан, обучение действительно присутствует в сохранённых evidence, qualification FAIL1/3. Это не незапущенный опыт и не PASS. Новый zero-training TTQ passport требовал исправления; родитель владеет им и очередью.

## Передача и границы

Worker изменил только10 перечисленных P-карточек,36 и этот review; product/runner/dataset/weights не менял, runtime не запускал. Parent редактирует общие contracts/index/prompts, TTQ-card и общий аудит. Никаких125 completed cycles, независимых reruns или all-state proof этот пакет не заявляет.

Открытая будущая работа: реализовать назначенные hosts внутри H с реальным native writer/admission, выполнить только при прямом разрешении, сохранить полные учебные/reader receipts и принять отрицательные результаты без добора дозы. После простого изменения Markdown execution status остаётся SHALL_IMPLEMENT, а acquisition/retention — непроверенными для новых программ.

