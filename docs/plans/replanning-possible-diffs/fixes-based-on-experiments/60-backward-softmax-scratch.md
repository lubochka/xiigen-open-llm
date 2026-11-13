# FIX-BWS — один временный массив softmax на обратный проход DPO

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q58](01-registry.md#work-fix-bws). Для общего включения действует измеренное ограничение этой строки. Возврат к работе — по записанному условию для её домена. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

## Уточнение допуска по показателям — 3 ноября 2026

**FIX-BWS: Приостановить общий перенос.** Вошёл по малой экономии выделений. Это не разрешение принять значительный рост пика.

- Точность: 66/66 учебных токенов сохранены.
- Полезный результат: Медиана выделений на обновление 1 879 189 192,8 → 1 879 024 291,2 Б (−0,00878%).
- Цена и ограничения: Медиана пика 1 378 791 424 → 1 598 201 856 Б (+15,91%); ускорение не установлено.
- [Полное сравнение и статус кода](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#benefit-fix-bws).

Этот вывод относится к названным ID, а не ко всем механизмам документа. Старый текст плана и опыта сохранён как доказательство, но его общий статус «к переносу / разрешён» не переопределяет это уточнение. Ближайшее действие — разбор уже сохранённых данных и конкретного регресса; новое обучение, повтор PRIMARY и переключение product default этим дополнением не назначаются. Для отдельного логического ремонта назвать нарушенный контракт и сохранить отрицательные контроли, а не переносить весь эксперимент под словом «bugfix».


<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски CORE:** Kernel parity проверить на назначенном реальном learning пути: независимые tensors/gradients/updates и ненулевой правильный readout. Отрицательный контроль обнаруживает возврат ошибки, положительный сохраняет lawful путь; isolated0→0 не качество обучения.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Для ресурсного переноса сравнивать actual native операции на том же обученном состоянии и в одинаковом окне. Managed allocations, peak и CPU различаются; прогрев/дрейф показываются по блокам. P08 продемонстрировал полезный gold-mass эффект при allocations+155,65%: наличие quality gain и кода в продукте не отменяет HOLD по памяти. Константа счётчика не является наблюдением.

Для FIX-BWS исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Стоимость относится к точно названному окну. Контрпример P09: разность allocations четырёх arm windows, делённая на4, включает создание входов/cache/receipts и не является постоянной стоимостью одного Forward. Для этой оптимизации указать размер работы, warmup, что внутри/снаружи счётчиков и одинаковую обвязку A/B; не закреплять точные bytes по одному такому среднему. Проверять существующие tests и дополнять их learned fixture, не объявлять отсутствующими по одному отчёту.

Применимость: FIX-BWS. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Полный ресурсный бюджет обязан включать обязательные диагностические вызовы и неудачные попытки, с отдельными единицами model/layer и optimizer-call/effective-update. Контрпример P11: 40 model Forward основной части +19 контрольных =59; 8 optimizer calls дают лишь4 изменения весов. Oracle/FD, заявленный обязательным, должен влиять на принятие результата, а не только выводиться в JSON. Равенство старому алгоритму подтверждать независимой проверкой, не комментарием в исходнике.

Применимость: FIX-BWS. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

**Допуск на27 октября — HOLD для общего переноса.** Шесть сохранённых `BWS01/results-01/verdict.json::runs` дают median peak A`1 378 791 424`→B`1 598 201 856`bytes, **+15,91324%**. Успех allocations −0,008775% остаётся измеренным, но не снимает правило [69](69-transfer-admission-and-priority-2026-10-27.md) о более5% не-CPU ухудшении. Открытый вопрос — безопасный профиль, а не отсутствие положительной оси. Не выдавать overlap диапазонов за отсутствие ухудшения; причинность peak не доказана. Ниже сохранён проект реализации, не безусловная команда перенести. Повторных запусков этим дополнением не назначено; сверка — [82](82-positive-transfer-delta-audit-2026-10-27.md).

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-BWS

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**M — метрика привязана к фактической стадии обучения.** Для обучающего loss/reward записать состояние, на котором он вычислен, отдельно от номера опубликованных после update весов. У DpoTrainer последняя StepMetrics — pre-update; не называть её оценкой конечного checkpoint. Пример P02: B уже стартовал с margin 0,002773 после SFT; отношение последних значений ×3,22 не равно отношению приростов DPO. Любое сравнение приобретения/ремонта использует собственное before каждого этапа и одинаковый reader. DPO Accuracy=(reward_margin>0) означает улучшение относительно reference, а не правильный ответ: T2 имеет policy chosen−rejected=−0,1827326 при положительном reward. TRAIN/EXAM correctness и сохранённые logP остаются отдельными полями. Если эта карточка сравнивает математически равные оптимизации, сохранить её A/B equality; не требовать искусственного роста точности и не добавлять post-update Forward, когда нужное чтение уже предусмотрено.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

25 октября 2026. **План переноса, без реализации и новых запусков.** Основание: успешная ось allocations опыта №65 `BACKWARD-SCRATCH-QUALIFIED-01 / BWS01`, commit `7c75b495f1d`. Прочитанный source HEAD `51c19ba2de3`; native получатель ещё содержит прежнюю реализацию. Следующий ID — **FIX-IGC**, по [реестру](01-registry.md). Исполнитель использует модель своей сессии по умолчанию. Тема обучения — документация кода; задача — описание порядка действий C#-метода; получатель — native Transformer с q/v LoRA, механизм — DPO.

## Измеренное улучшение

Источники: [report](../../../../experiments-v2/RP01-MICRO/BWS01/results-01/report.md), [verdict](../../../../experiments-v2/RP01-MICRO/BWS01/results-01/verdict.json), шесть `run-*/arm-result.json` в том же `results-01`, [Bws01Bridge.cs](../../../../experiments-v2/RP01-MICRO/BWS01/Bws01Bridge.cs), [source selection](../../../../experiments-v2/RP01-MICRO/BWS01/Sources.DPO.props). Bridge компилирует `35.ExperimentsV2/Experiments/Rp01Micro/N62-R2`, поэтому исторический успех не означает уже внедрённый native код.

Порядок шести процессов — A B B A B A, по 10 DPO updates. Во всех raw совпадают start/final state hashes, step records, before/after panels и optimizer step 10; обе TRAIN строки сохраняют 33/33 правильных response tokens, actual parameter state меняется. Это сверка сохранённых полей, не независимое сравнение всех бинарных checkpoint/moments. Ненулевой learned-before отличает этот опыт от неквалифицированного старого №05.

| Ось | A → B | Вывод |
|---|---|---|
| Медиана allocated bytes/update | 1 879 189 192.8 → 1 879 024 291.2 | −164 901.6 bytes, около −0.008775%; полезное измеренное улучшение |
| Диапазоны allocations/update | A: 1 879 184 859.2–1 879 189 677.6; B: 1 879 024 252–1 879 029 264 | Три наблюдения на руку, диапазоны не пересеклись; это не универсальный доверительный интервал |
| Медиана wall/update, ms | 15 709.24657 → 15 456.85482 | Диапазоны пересекаются; причинное ускорение не установлено |
| Медиана CPU/update, ms | 12 857.8125 → 12 442.1875 | То же ограничение |
| Медиана process peak, bytes | 1 378 791 424 → 1 598 201 856 | Ухудшение, не экономия RAM процесса |

Chosen NLL D-T1: 0.0001682954→0.0001711616, D-T2: 0.0001697296→0.0001762845 в обеих руках — небольшое ухудшение. Разность A/B по обучению не показана и не требуется для успеха ресурсной оси при квалифицированном обновлении. EXAM/free generation в этом опыте не измерялись.

## Единственная правка и математическое условие

Получатель — [11.DPO/DpoTrainer.cs:1072](../../../../11.DPO/DpoTrainer.cs), private `BackwardLogProbs(PolicyLogProbGraph,float)`. Перенести `var expVals = new float[v]` из цикла активных позиций непосредственно после `if (cnt <= 0f) return` и вычисления scale. Других изменений алгоритма нет. Точный исторический diff — [A](../../../../35.ExperimentsV2/Experiments/Rp01Micro/N62-R2/A/DpoTrainer.cs) → [B](../../../../35.ExperimentsV2/Experiments/Rp01Micro/N62-R2/B/DpoTrainer.cs).

Массив локален **одному вызову**, не trainer, process, chosen/rejected pair или thread pool. Не применять ArrayPool, static cache, shared buffer, stackalloc или сохранённую таблицу вероятностей. `maxVal` и `expSum` по-прежнему заново считаются для каждой строки; все v элементов переписываются до чтения. Пропуск masked positions, sum/mean scaling, fractional mask, знак производной, `+=` в logits.Grad и конечный `logits.Backward(logits.Grad)` сохраняются. На zero-mask пути softmax buffer не создаётся; существующее создание zeros-like logits.Grad до этого guard остаётся прежним.

Для строки с целевым y производная равна `upstream × mask × (oneHot(y)−softmax(logits))`, дополнительно делённая на cnt только при mean. Временный массив не входит в граф и полностью записан до использования, поэтому reuse не меняет эту формулу, порядок арифметики или gradient. При n активных позициях одна сторона сохраняет `(n−1) × array_size(v)` bytes, если n>0; при n=0 ничего не меняется. Вычислительная сложность остаётся O(nv), одновременно живой scratch — O(v).

Для двух сторон по33 target positions и V629 оценка на использованном x64 runtime: `2 × 32 × align8(24 + 4×629) = 162816 bytes/update`. Header/alignment — допущение конкретного runtime, не часть математического доказательства семантики. Разница с измеренными164901.6 составляет2085.6 bytes; отдельной трассировки для её причин нет. Нельзя объявлять остаток точно «заголовками»: заголовки уже учтены в формуле. Измерялось окно целых StepVector с публикацией, не изолированный private метод.

Это отдельный фактор от FIX-OSR58 (экспорт optimizer metadata), FIX-FBC54 (bit comparison), FIX-NH36 (scored LM-head rows), FIX-IGC61 (initial gradient copy) и FIX-AGC41 (поступление gradient в TensorOps). Их не внедрять вместе и не складывать проценты как измеренный общий результат.

## Набор и восстановление

Единственный donor: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1/`. `S0.weights.bin`: 47 899 519 bytes, SHA `a3cad45680c0953ffd03ca52a32d80f0afa5547015e493907af6811cf85cd9c5`. Полные config/tokenizer hashes, native APIs и ограничения — [54§4](54-frozen-base-check-scope.md). H512/V629/C2048/L4/Q8/KV2/FF1408, q/v r8/alpha16/dropout0. **И policy, и reference загружают S0** после штатного topology/adapter setup, как в Bws01Bridge; это намеренно новый DPO reference anchor, не незаметное изменение профиля с B0 reference.

TRAIN: только D-T1/D-T2 из `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BWS01/train.jsonl`, SHA `abb353b31fd5c973fe2e84efbb91e465bf9027a90e870903f36ca66718c78a41`. Перенести неизменные bytes в назначенный будущий fixture `Tests/Fixtures/ExperimentFixes/v1/Bws/train.jsonl`; `manifest.json` закрепляет donor/config/tokenizer/material identities. Не смешивать дополнительные темы и не брать EXAM как уроки.

DPO optimizer fresh step0 в каждой руке; SFT producer moments/RNG не импортируются. Начальные native random streams идентичны A/B; зафиксировать typed snapshot/identity при создании trainer. Это явно новая DPO-эпоха, а не восстановление отсутствующей истории. Перед измерением требуется 33/33 на каждой строке. Missing/hash mismatch/неверный framing → адресная диагностика fixture, не повтор1000SFT и не предложение пользователю выбрать переобучение.

## Native тесты и ремонт ожиданий

Будущие файлы: `Tests/ExperimentFixes/BackwardSoftmaxScratchContractTests.cs`, `BackwardSoftmaxScratchTransferTests.cs`; namespace `LlmFromScratch.Tests.ExperimentFixes`. Контрактные tests через один test-only reflection helper вызывают существующие private `PolicyLogProbGraph(Logits,Ids,Mask,LogProbability,SumTargets)` и `BackwardLogProbs`; helper не копирует learner и одинаков в A/B. Не открывать private API продукту ради тестов.

| Метод | Проверка и расход на стадию |
|---|---|
| `MaskedRowsMatchAnalyticGradient(bool sumTargets)` —2 Theory cases | Малые T4/V3 logits; строки `[log2,0,0]`, `[0,log2,0]`, `[0,0,log2]`, финальная строка не scored; ids `[0,0,1,2]`, mask `[0,1,0.25,0]`, upstream2. Проверить ожидаемые softmax `[.5,.25,.25]` / `[.25,.5,.25]`, scale sum или `/1.25`, masked row zero. По1root Backward; no model/optimizer |
| `ZeroMaskPreservesNoSoftmaxWork` | Все mask=0, logits.Grad остаётся zeros-like;0root Backward. Наличие guard перед scratch проверяется также в точном product diff; тест не утверждает отсутствия любых allocations |
| `IndependentCallsPreserveTheirGradients` | Два отдельных graph/разные logits; сравнить первые grad bytes до/после второго вызова и analytic values.2root Backward; не выдавать это за стресс-тест гонок. Local-buffer lifetime дополнительно проверяется по diff |

Сохранить существующий `LlmFromScratch.Tests.DpoTrainerTests.DpoTrainer_StepUsesEachPolicyTrainingGraphExactlyOnce` ([тело](../../../../Tests/DpoTrainerTests.cs)): один tiny Step/стадию,4model forwards/2backward/1optimizer. Он проверяет реальное употребление training graphs; не подменяет qualified H512 learning.

Новый Fact `BackwardSoftmaxScratchTransferTests.NativeQualifiedLearning_RecordsArm` использует **native** DpoTrainer и полный путь `StepVector→BackwardLogProbs→Tensor.Backward→AdamW→adapters`. Падающие tests сейчас не запускались и не установлены. При будущем падении ремонтировать setup/expected лишних allocations; не менять gradient tolerances, число actual updates, mask/zero-mask поведение, mutation admission или входы ради зелёного результата.

## Один будущий native A/B и готовый порядок

Exact contract filter общего runner (один process/стадию, пять cases с раскрытой Theory):

```text
FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.BackwardSoftmaxScratchContractTests.MaskedRowsMatchAnalyticGradient|FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.BackwardSoftmaxScratchContractTests.ZeroMaskPreservesNoSoftmaxWork|FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.BackwardSoftmaxScratchContractTests.IndependentCallsPreserveTheirGradients|FullyQualifiedName=LlmFromScratch.Tests.DpoTrainerTests.DpoTrainer_StepUsesEachPolicyTrainingGraphExactlyOnce
```

Рабочий каталог `C:/Projects/llm_mvp_core`; common runner — планируемый `Tests.Support/ExperimentTransfer/Run-NativeAB.ps1`, состояния — [02](02-learning-state-and-fixtures.md), навыки/русский отчёт/wait — [03](03-test-and-execution-contract.md). Общий loader/scorer из54/58 переиспользуется с отдельным PlanId; не создавать ещё один learner проект. Source copies N62-R2 не могут подменить B assembly.

1. При старом product code добавить общий fixture/tests и exact discovery; закрепить `$baselineCommit`. Baseline запускается до переноса массива. Все A receipts завершить.
2. В том же checkout изменить только private метод выше, закрепить `$candidateCommit`; Candidate выполняет B, используя уже завершённое A. Чужой code не reset/checkout, результаты не очищать.
3. Каждый learning process делает10DPOupdates, чередуя D-T1/D-T2, из одного назначенного S0. Основной бюджет A+B:20updates,80training forwards,40backward,16before/after scoring forwards;0SFT/acquisition/Generate/teacher. Контракты отдельно добавляют2tiny Step,8model forwards,4backward и8leaf root Backward. Итого22optimizer applications; не скрывать tiny setup за числом20.
4. На стадию1Rebuild/1discovery,1contract process (4новых cases +1existing),1learning process. Всего2Rebuild,2discovery,4test processes,12cases. Полные filters формируются по указанным namespace+method; learning: `FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.BackwardSoftmaxScratchTransferTests.NativeQualifiedLearning_RecordsArm`. Discovery обязана найти6cases/стадию, а не0.

```powershell
Set-Location C:/Projects/llm_mvp_core
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-BWS -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit
# Только после native diff; A уже сохранено.
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-BWS -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit -CandidateCommit $candidateCommit
```

Это будущие команды после реализации общего runner, не заявление о существующей готовой команде. Output: `artifacts/fbe/FIX-BWS/attempt-0001/{contracts,learning}/{A,B}/`; immutable intent, per-stage binding/source/runtime/native DLL hashes и terminal receipts по03. В measured window —10полных StepVector с одинаковой публикацией/logging; restore/scoring/compare вне окна. Final weights/checkpoint, portable optimizer и typed RNG сохранить native writers и прочитать без нового fit, как54/58. Acceptance: действительное изменение adapters, optimizer0→10, unchanged base/reference, равные A/B losses/weights/moments/RNG и33/33retention. Полезное уменьшение allocations достаточно на своей оси, ухудшения NLL/wall/CPU/peak показываются рядом. Не требовать ровно164902bytes и не повторять серию до удобного знака.

Этот kernel внутри code-documentation handler не меняет prompt→domain. [20-E2E](20-prompt-domain-e2e.md) может прочитать готовый native receipt без нового H512 load/fit/запроса; wrong dispatch не чинится scratch buffer. При запуске ждать тот же handle штатным blocking wait до terminal/deadline, без busy polling, второго monitor и придуманной работы. Ошибка форматирования чинится по сохранённому raw. **STOP/NEXT Любы выполняется сразу**, без «ещё одного» исправления старой карточки. В текущей сессии всё это остаётся планом.
