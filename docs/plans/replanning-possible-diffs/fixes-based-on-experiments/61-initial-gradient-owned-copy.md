# FIX-IGC — убрать внешнюю копию начального градиента, сохранить владение данными

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q28](01-registry.md#work-fix-igc). Для общего включения действует измеренное ограничение этой строки. Возврат к работе — по записанному условию для её домена. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

## Уточнение допуска по показателям — 3 ноября 2026

**FIX-IGC: Приостановить общий перенос.** Экономия есть, но утверждение «без значительного ухудшения» сохранёнными замерами не поддерживается.

- Точность: 66/66 учебных токенов сохранены.
- Полезный результат: Медиана выделений на обновление 1 879 191 087,2 → 1 878 565 130,4 Б (−0,03331%).
- Цена и ограничения: Пик 1 333 448 704 → 1 451 937 792 Б (+8,89%); время 124 313,6508 → 148 449,0313 мс (+19,41%), диапазоны пересекаются; CPU +9,20%.
- [Полное сравнение и статус кода](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#benefit-fix-igc).

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

Для FIX-IGC исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Стоимость относится к точно названному окну. Контрпример P09: разность allocations четырёх arm windows, делённая на4, включает создание входов/cache/receipts и не является постоянной стоимостью одного Forward. Для этой оптимизации указать размер работы, warmup, что внутри/снаружи счётчиков и одинаковую обвязку A/B; не закреплять точные bytes по одному такому среднему. Проверять существующие tests и дополнять их learned fixture, не объявлять отсутствующими по одному отчёту.

Применимость: FIX-IGC. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Полный ресурсный бюджет обязан включать обязательные диагностические вызовы и неудачные попытки, с отдельными единицами model/layer и optimizer-call/effective-update. Контрпример P11: 40 model Forward основной части +19 контрольных =59; 8 optimizer calls дают лишь4 изменения весов. Oracle/FD, заявленный обязательным, должен влиять на принятие результата, а не только выводиться в JSON. Равенство старому алгоритму подтверждать независимой проверкой, не комментарием в исходнике.

Применимость: FIX-IGC. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-IGC

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**M — метрика привязана к фактической стадии обучения.** Для обучающего loss/reward записать состояние, на котором он вычислен, отдельно от номера опубликованных после update весов. У DpoTrainer последняя StepMetrics — pre-update; не называть её оценкой конечного checkpoint. Пример P02: B уже стартовал с margin 0,002773 после SFT; отношение последних значений ×3,22 не равно отношению приростов DPO. Любое сравнение приобретения/ремонта использует собственное before каждого этапа и одинаковый reader. DPO Accuracy=(reward_margin>0) означает улучшение относительно reference, а не правильный ответ: T2 имеет policy chosen−rejected=−0,1827326 при положительном reward. TRAIN/EXAM correctness и сохранённые logP остаются отдельными полями. Если эта карточка сравнивает математически равные оптимизации, сохранить её A/B equality; не требовать искусственного роста точности и не добавлять post-update Forward, когда нужное чтение уже предусмотрено.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

25 октября 2026. **План, не реализация.** Основание — успешная ресурсная ось №66 `INITIAL-GRADIENT-COPY-QUALIFIED-01 / LFG01`, commit `a410e4534a4`; source проверен на `51c19ba2de3`. Следующий ID — **FIX-LFT** из [реестра](01-registry.md). Исполнитель — модель текущей сессии по умолчанию; рабочий каталог `C:/Projects/llm_mvp_core`. Тема/профиль/получатель — документация C# / D-T1,D-T2 / native Transformer q/v LoRA, реальный DPO.

## Что измерено

[Report](../../../../experiments-v2/RP01-MICRO/LFG01/results-01/report.md), [verdict](../../../../experiments-v2/RP01-MICRO/LFG01/results-01/verdict.json), шесть `run-*/arm-result.json`, [Lfg01Bridge](../../../../experiments-v2/RP01-MICRO/LFG01/Lfg01Bridge.cs) и [Sources.TensorCore.props](../../../../experiments-v2/RP01-MICRO/LFG01/Sources.TensorCore.props) связывают A/B с копиями старого `LF-03/{A,B}/Core/Tensor.cs`. Порядок A B B A B A,10DPOupdates/run. Совпали raw start/final parameter hashes,10step records, panels и optimizerStep10; каждая TRAIN строка33/33до/после, actual state изменился. Полного независимого byte compare всех исторических optimizer/checkpoint здесь не было.

| Метрика | A → B | Вывод |
|---|---|---|
| Медиана allocated bytes/update | 1 879 191 087.2 → 1 878 565 130.4 | −625956.8bytes, примерно−0.03331%; успешная полезная ось |
| Диапазоны allocated | A1 879 187 220–1 879 193 823.2; B1 878 564 620–1 878 568 100 | Три точки/руку, диапазоны не пересекаются |
| Медиана wall/update,ms |12 431.36508→14 844.90313| Наблюдаемое ухудшение; перекрывающиеся диапазоны не дают причинного процента |
| Медиана CPU/update,ms |10 376.5625→11 331.25| Ухудшение наблюдения |
| Медиана process peak,bytes |1 333 448 704→1 451 937 792| Peak вырос; allocated bytes не равны живой памяти |

Chosen NLL D-T1/D-T2 слегка ухудшился одинаково в A/B: `.0001682954→.0001711616`, `.0001697296→.0001762845`. EXAM/free generation не исследовались. Новый квалифицированный опыт не делает прежний cold LF-03 с0→0валидным задним числом.

## Точный native перенос и доказательство

[00.TensorCore/Core/Tensor.cs:94](../../../../00.TensorCore/Core/Tensor.cs), `Backward(Tensor initialGradient)`: заменить только

```csharp
Grad = new Tensor(initialGradient.Data.ToArray(), initialGradient.Shape);
```

на

```csharp
Grad = new Tensor(initialGradient.Data, initialGradient.Shape);
```

[Tensor constructor](../../../../00.TensorCore/Core/Tensor.cs) передаёт данные в [TensorStorage(float[])](../../../../00.TensorCore/Core/TensorStorage.cs), который **по-прежнему делает собственный `data.ToArray()`**. Присваивание `Grad=initialGradient`, удаление storage-copy, изменение Shape, DFS, BackwardFunc, accumulate/zero semantics не входит в diff. Место вызова в реальном обучении — [DpoTrainer.BackwardLogProbs](../../../../11.DPO/DpoTrainer.cs), завершающееся `logits.Backward(logits.Grad)`.

Если C — owned copy массива x, старая цепочка C(C(x)) и новая C(x) дают одинаковые bytes без alias с x. Конструктор синхронен; изменение внешнего x после возврата не влияет на Grad. Для совпавшего `initialGradient == old Grad` RHS полностью создаётся до присваивания нового Grad — старые bytes также копируются безопасно. Это не поддержка конкурентной внешней записи в массив во время copy; такой гарантии не было и раньше.

Для DPO две стороны логитов `[1,124,629]`, на каждую убран один float[]: `4×124×629=311984` payload bytes, с принятой для этого runtime24-byte header —312008; два массива дают624016. Наблюдаемая медианная разность625956.8отличается на1940.8bytes. Причина остатка не аттестована отдельной allocation trace; не приписывать его точно неизвестным объектам. Размер/header зависит от runtime; формула объясняет порядок и масштаб, не даёт exact CI threshold. Временная сложность остаётся O(TV), а копирований становится одно вместо двух.

**Не дубликат [FIX-AGC41](41-grad-accum-copy.md):** тот план убирает внешнюю копию в первом `TensorOps.AccumulateGrad`, этот — только seed в `Tensor.Backward(initialGradient)`. В baseline/candidate каждого фактора остальные алгоритмы одинаковы. Также не включать scratch60 или optimizer58 попутно. Сумма экономий04–07 из отчёта66 — арифметика раздельных опытов; совместного A/B четырёх факторов не было, поэтому «вместе доказано0.065%» не переносится в планы.

## Общий обученный старт и артефакты

Профиль полностью совпадает с [60-BWS](60-backward-softmax-scratch.md): готовый `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1/S0.weights.bin`, SHA `a3cad45680c0953ffd03ca52a32d80f0afa5547015e493907af6811cf85cd9c5`,47899519bytes; **policy и reference=S0**, DPO optimizer fresh0, одинаковые fresh native random streams. SFT moments/RNG producer не импортировать; acquisition не повторять. Native model/optimizer/RNG APIs и hashes —54§4 и58§5.

Исходные две строки — `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LFG01/train.jsonl`, SHA `abb353b31fd5c973fe2e84efbb91e465bf9027a90e870903f36ca66718c78a41`; будущий fixture `Tests/Fixtures/ExperimentFixes/v1/Igc/{train.jsonl,manifest.json}` сохраняет bytes/config/material identities. Состав и framing не расширяются; новая тема/EXAM не добавляются. Before обязан быть33/33по каждой строке, иначе остановка с конкретной fixture/load причиной, а не0→0результат или новый1000-step producer.

Output нового native переноса — `artifacts/fbe/FIX-IGC/attempt-0001/{contracts,learning}/{A,B}/`. Завершённые A/B и checkpoints читаются повторно, не перезаписываются и не обучаются заново ради сериализации отчёта.

## Тесты, сохраняющие математический и ownership контракт

Новые файлы: `Tests/ExperimentFixes/InitialGradientCopyContractTests.cs`, `InitialGradientCopyTransferTests.cs`; namespace `LlmFromScratch.Tests.ExperimentFixes`. Одинаковые tests компилируются на исходной и исправленной native библиотеке, новых product API или reflection не требуется.

| Новый Fact | Что проверять |
|---|---|
| `CallerMutationAfterBackwardDoesNotChangeOwnedGradient` | Leaf root и seed; после1Backward изменить seed.Data. Root.Grad bytes не меняются, массивы не одной identity |
| `OwnedGradientMutationDoesNotChangeCaller` | После1Backward изменить root.Grad.Data; seed bytes прежние |
| `ExistingGradientCanBeUsedAsSeedWithoutAlias` | Передать `root.Backward(root.Grad)` со заранее заданным Grad; новый Grad содержит те же values, не alias старого;1Backward |
| `WrongShapeRejectsBeforeReplacingGradient` | Shape mismatch даёт прежний ArgumentException, старый Grad identity/bytes сохранены;0Backward graph traversal |
| `SeededGraphKeepsDerivative` | x=[2], y=Scale(x,3), seed=[5] →y.Grad=[5],x.Grad=[15];1Backward,0optimizer |

Сохранить три existing [TensorCoreTests](../../../../Tests/TensorCoreTests.cs): `Backward_AddOp_GradPassedToBothInputs`, `Backward_MultiplyOp_GradIsOtherOperand`, `Backward_MatMul_GradMatchesPyTorchReference`. Они дают3unseeded Backward/стадию,0model forward/optimizer и не заменяют нового seeded ownership покрытия.

Не заявлять сейчас найденных падающих tests: они не запускались. В будущем падающий alias/gradient/shape тест — регрессия, не повод ослабить oracle. Меняется только expectation лишнего allocation, если такой test действительно существует; он должен проверять отдельное owned storage и фактический ресурсный выигрыш, а не две копии как обязательное поведение.

## Конечный native A/B и связь с E2E

Exact contract filter общего runner (один process/стадию, восемь Facts):

```text
FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.InitialGradientCopyContractTests.CallerMutationAfterBackwardDoesNotChangeOwnedGradient|FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.InitialGradientCopyContractTests.OwnedGradientMutationDoesNotChangeCaller|FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.InitialGradientCopyContractTests.ExistingGradientCanBeUsedAsSeedWithoutAlias|FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.InitialGradientCopyContractTests.WrongShapeRejectsBeforeReplacingGradient|FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.InitialGradientCopyContractTests.SeededGraphKeepsDerivative|FullyQualifiedName=LlmFromScratch.Tests.TensorCoreTests.Backward_AddOp_GradPassedToBothInputs|FullyQualifiedName=LlmFromScratch.Tests.TensorCoreTests.Backward_MultiplyOp_GradIsOtherOperand|FullyQualifiedName=LlmFromScratch.Tests.TensorCoreTests.Backward_MatMul_GradMatchesPyTorchReference
```

Готовые команды будущего исполнения; SHA закрепляются в описанном ниже порядке common/factor, значения не выбираются из чужого результата:

```powershell
Set-Location C:/Projects/llm_mvp_core
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-IGC -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit
# Только после native diff; завершённое A не повторяется.
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-IGC -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit -CandidateCommit $candidateCommit
```

Общий будущий `Tests.Support/ExperimentTransfer/Run-NativeAB.ps1` по [03](03-test-and-execution-contract.md) получает PlanId `FIX-IGC`. Сначала при старом native code закрепляются общие tests/fixtures и `$baselineCommit`; завершается Baseline. Затем ровно указанный однострочный diff в том же checkout, `$candidateCommit`, Candidate с готовым A. Никаких worktrees/reset/copy learner. Stage/SourceRoot/BaselineCommit/CandidateCommit и команды зафиксированы выше.

Learning filter: `FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.InitialGradientCopyTransferTests.NativeQualifiedLearning_RecordsArm`. Он выполняет10native StepVector/arm из общего S0, поочерёдно D-T1/D-T2. У каждой стадии contract process:5новых +3existing facts, затем1learning Fact. Итого2Rebuild,2discovery,4test processes,18test-case executions; discovery проверяет9cases/стадию,0tests —ошибка.

Общий расход A+B:20DPOupdates/optimizer applications,80training forwards,40training backward,16scoring forwards; contracts добавляют14tiny Tensor.Backward,0optimizer и0model calls. Acquisition/SFT/Generate/teacher=0. Все diagnostic/readback/formatter операции идут по готовым artifacts, вне measured10-step окна с неизменной publication/logging.

Сопоставлять A/B final parameter bits, полные portable optimizer states, typed RNG, все10step records и33/33retention; actual adapters должны измениться, frozen base/reference — нет. Записать native assembly/source hashes, runtime, формы seed и resource deltas. Final checkpoint уже создан learner; native optimizer/RNG writers используются вне окна, без нового update. Полезный allocation gain признаётся успехом оси, а wall/CPU/peak/NLL ухудшения остаются рядом. Если в новом native A/B gain не подтвердился — так и записать, без автоматического добора повторов.

В [20-E2E](20-prompt-domain-e2e.md) это вычисление уже выбранного neural handler; готовый receipt не доказывает качество selector. Новых routing queries, H512 loads и обучения ради этой связи нет. Точные skills/русский отчёт —03. После запуска использовать blocking wait исходного handle; не завершать ход вместо ожидания, не опрашивать файл часто и не создавать ненужную работу. Ошибка отчёта не повод повторять обучение. **STOP/NEXT — немедленно**, переход только по stable ID реестра; старые результаты не удалять. Сейчас реализуется только этот документ.
