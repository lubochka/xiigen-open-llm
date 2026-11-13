# FIX-AGC — принять первый градиент с одной защитной копией

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q30](01-registry.md#work-fix-agc). Для общего включения действует измеренное ограничение этой строки. Возврат к работе — по записанному условию для её домена. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

## Уточнение допуска по показателям — 3 ноября 2026

**FIX-AGC: Приостановить общий перенос до квалификации памяти.** Нельзя скрывать +6,74% за уменьшившимся пиком. Куча до/после GC и суммарные выделения — разные величины.

- Точность: 46/46 учебных токенов, один точный учебный ответ сохранены; экзамен не измерялся.
- Полезный результат: Выделения 1 847 691 280 → 1 761 735 608 Б (−4,65%); время 15 965,7 → 13 375,3 мс (−16,23%).
- Цена и ограничения: Куча сразу после шага 422 111 616 → 450 573 384 Б (+6,74%); после полной сборки +0,00157%. GC 110/10/4 → 114/14/8.
- [Полное сравнение и статус кода](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#benefit-fix-agc).

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

Для FIX-AGC исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Стоимость относится к точно названному окну. Контрпример P09: разность allocations четырёх arm windows, делённая на4, включает создание входов/cache/receipts и не является постоянной стоимостью одного Forward. Для этой оптимизации указать размер работы, warmup, что внутри/снаружи счётчиков и одинаковую обвязку A/B; не закреплять точные bytes по одному такому среднему. Проверять существующие tests и дополнять их learned fixture, не объявлять отсутствующими по одному отчёту.

Применимость: FIX-AGC. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Полный ресурсный бюджет обязан включать обязательные диагностические вызовы и неудачные попытки, с отдельными единицами model/layer и optimizer-call/effective-update. Контрпример P11: 40 model Forward основной части +19 контрольных =59; 8 optimizer calls дают лишь4 изменения весов. Oracle/FD, заявленный обязательным, должен влиять на принятие результата, а не только выводиться в JSON. Равенство старому алгоритму подтверждать независимой проверкой, не комментарием в исходнике.

Применимость: FIX-AGC. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-AGC

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**M — метрика привязана к фактической стадии обучения.** Для обучающего loss/reward записать состояние, на котором он вычислен, отдельно от номера опубликованных после update весов. У DpoTrainer последняя StepMetrics — pre-update; не называть её оценкой конечного checkpoint. Пример P02: B уже стартовал с margin 0,002773 после SFT; отношение последних значений ×3,22 не равно отношению приростов DPO. Любое сравнение приобретения/ремонта использует собственное before каждого этапа и одинаковый reader. DPO Accuracy=(reward_margin>0) означает улучшение относительно reference, а не правильный ответ: T2 имеет policy chosen−rejected=−0,1827326 при положительном reward. TRAIN/EXAM correctness и сохранённые logP остаются отдельными полями. Если эта карточка сравнивает математически равные оптимизации, сохранить её A/B equality; не требовать искусственного роста точности и не добавлять post-update Forward, когда нужное чтение уже предусмотрено.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Уточнение после аудита 27 октября 2026:** владелец этой карточки — FIX-AGC. При первом накоплении градиента защитная копия должна принадлежать TensorStorage; изменение исходного массива вклада после Accumulate не меняет накопленный gradient. При повторном вкладе остаётся сложение, а не замена. Проверять независимыми значениями: первый вклад `[1,2]`, второй `[3,4]` дают `[4,6]`; изменение первого входного массива после передачи не меняет этот итог. Для назначенного DPO сохранить полный state oracle и прежний бюджет, не ограничиваться loss. CS/MB/RPE описаны в [прежнем41-neural](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) и уточнены в57; это другие владельцы, несмотря на одинаковый номер файла. Действует [03§24](03-test-and-execution-contract.md#transfer-audit-20261027). Это уточнение плана; новые проверки не запускались.

Дата: 22 октября 2026, UTC. Статус: **план кандидата, production и tests ещё не изменены**. Основание — завершённый GRAD-ACCUM-COPY-01 / AGC1, не старый LF-03 с неусвоенным ответом. Проверенный срез исходников и evidence: [ebaf7760a36d43172bd88317ff469edfd09a615b](https://github.com/lubochka/llm_mvp_core/commit/ebaf7760a36d43172bd88317ff469edfd09a615b). Порядок и следующий ID задаёт [реестр](01-registry.md); собственный номер файла не задаёт очередь.

**Решение:** убрать один внешний `ToArray()` при первом поступлении градиента. Конструктор тензора по-прежнему владеет отдельной копией. Исторический опыт сохранил уже усвоенные 46/46 токенов ответа и тот же результат одного DPO-обновления, выделив на 85 955 672 байта меньше. Это успех по расходу памяти настоящего обучения; точность не выросла. Перенос остаётся отдельным решением пользователя.

## 1. Доказательства и новизна

| Источник на проверенном срезе | Что проверяется |
|---|---|
| [Отчёт AGC1](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/AGC1/results/report.md) | Фактор, назначенный профиль, acquisition общего старта и ограничения |
| [Raw A](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/AGC1/results/attempt-0001/A/metrics.json), [raw B](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/AGC1/results/attempt-0001/B/metrics.json) | `steps[0]`, `learningSignal`, `firstGradient`, `finalWeights`: фактический переход и его цена |
| [Паспорт копий](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/AGC1/source-copy-manifest.json) | Историческая A/B-копия TensorOps; её native перенос ещё не выполнен |
| [Готовый общий старт](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-postfix-one/v1/bundle.json) | COMPLETE, `learning_qualified=true`, приобретён `simulation-csharp-doc-f06-008`; producer1000 уже оплачен |

Прежние планы 29/30 меняют другие участки TensorOps. LF-03 относится к начальной копии в `Tensor.Backward(initialGradient)`, а этот фактор — к каждому первому поступлению в `TensorOps.AccumulateGrad`. Не переносить LF-03 попутно и не называть два места одной правкой. Данный план закрывает отсутствовавшие native integration/tests/fixture instructions для нового положительного AGC1.

| Измерение | A | B | Вывод |
|---|---:|---:|---|
| Allocated bytes одного StepVector | 1 847 691 280 | 1 761 735 608 | −85 955 672, −4,652% |
| Teacher-forced TRAIN correct before→after | 46/46→46/46 | 46/46→46/46 | Усвоенный ответ сохранён; это не свободная генерация |
| TRAIN mean NLL before→after | 0,000013250273→0,000013182895 | То же | Изменение одинаково |
| DPO optimizer | 0→1 | 0→1 | По одному реальному обновлению |
| Adapter delta L2 | 0,00225587145855641 | То же | 53 248 значений в 16 массивах |
| GC0/1/2 | 110/10/4 | 114/14/8 | По +4; ухудшение сохранено |
| Retained heap сразу после шага | 422 111 616 | 450 573 384 | +28 461 768 байт |
| Retained heap после полной GC | 99 599 592 | 99 601 152 | +1 560 байт |
| Wall / CPU шага | 15 965,676 / 13 328,125 ms | 13 375,3191 / 12 468,75 ms | Одно наблюдение; причинное ускорение не установлено |

Интервал таблицы — **`steps[0]`**, не более широкий `costs.training*`. В raw A/B одинаковы loss, reward margin, carrier, адаптерный digest и объявленный digest конечного weight-файла. `comparison.json` при данной проверке доступен как LFS pointer; его payload и бинарные веса здесь не скачивались. Поэтому это сверка сохранённых raw и их хэшей, не новая проверка бинарных байтов. До будущего native запуска donor проверяется реальными bytes/hash, а не указателем LFS.

**Любое измеренное улучшение полезного показателя — успех по этому показателю и профилю, без требования роста accuracy.** Один C#-урок не доказывает обобщения по доменам, скорости многошагового обучения или пользы LoRA вне этой панели. Улучшение времени/пика на единственном холодном процессе не получает универсального обещания.

## 2. Где менять код и почему обучение зависит от него

Получатель — [00.TensorCore/Core/TensorOps.cs, AccumulateGrad, строки 984–999](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/00.TensorCore/Core/TensorOps.cs#L984-L999). Единственная замена:

~~~csharp
// Было, только ветвь target.Grad == null:
target.Grad = new Tensor(gradToAdd.Data.ToArray(), gradToAdd.Shape);
// Должно стать:
target.Grad = new Tensor(gradToAdd.Data, gradToAdd.Shape);
~~~

Оставить без изменений early return по `RequiresGrad`, ветвь `+=`, порядок суммирования, shape и существующую семантику ошибок. [Tensor(float[],TensorShape)](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/00.TensorCore/Core/Tensor.cs#L37-L46) вызывает [TensorStorage(float[])](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/00.TensorCore/Core/TensorStorage.cs#L21-L24), который выполняет `data.ToArray()`. Их менять нельзя: именно они сохраняют владение и изоляцию градиента. Не убирать копию в `Tensor.Backward`, не менять конструктор на shared storage, не вводить новый feature flag.

Реальный путь: `DpoTrainer.StepVector` → policy forward → backward выбранной/отвергнутой стороны → TensorOps backward closures → `AccumulateGrad` → native optimizer → адаптеры. Исторически достигнуты **424 первых поступления / 21 492 528 float** за один update, поэтому измеряемое место действительно участвует в обучении. Native learning test ниже использует этот же продуктовый путь.

### Математическое основание

Если вклады в параметр равны g₁,…,gₘ, градиент обязан быть Σgᵢ. Первый вклад сохраняется как независимая копия C(g₁); последующие прибавляются в прежнем порядке. Для копии массива выполняется:

\[
C(C(g_1))=C(g_1)\quad\text{по значениям},\qquad
\operatorname{storage}(C(g_1))\ne\operatorname{storage}(g_1).
\]

В A внешний `ToArray` и конструктор создают две последовательные копии. В B конструктор создаёт одну: значения и изоляция те же, число float-копирований меньше. Это доказательство численного и ownership-контракта при неизменённом конструкторе; оно не доказывает ускорение GC.

Общая опора для композиции backward — [Proposition 9.4, chain rule](https://github.com/lubochka/xiigen-planning/blob/d7b1704fd681fe0855afeb08ca5234433b5f9f6c/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0312-0317-ch09-motor-taxonomy-and-related-work.md#proposition-9-4). Здесь локальные производные и порядок накопления вкладов не меняются. Свойство двух копий массива доказано выше отдельно по native конструкторам; книга не является доказательством ownership или измеренных −4,652%.

Снимаемый payload по трассе: `4 × 21 492 528 = 85 970 112` байт. Измеренное уменьшение 85 955 672 отличается на 14 440 байт, около 0,017%. Заголовки массивов, окружение и GC учитываются отдельно; расчёт не подставляется вместо замера. Условие доказательства проверяют тест изоляции первого вклада и тест двух путей в один leaf.

## 3. Точный donor, данные и восстановление

Планируемый общий test helper: `Tests/ExperimentFixes/PostfixDpoStateFixture.cs`; паспорта выбранного фактора: `Tests/Fixtures/ExperimentFixes/v1/GradAccumCopy01/manifest.json`. Helper не содержит копий TensorOps/Trainer и не производит S0. Для [FIX-FBM](42-frozen-broadcast-map.md) он может быть тем же проверенным файлом; каждый фактор использует свою A/B-пару, а не результаты другого фактора.

Единственный donor — `experiments-v2/RP01-MICRO/_shared-learned-starts/h512-postfix-one/v1/`:

| Файл | SHA-256 | Bytes |
|---|---|---:|
| S0.weights.bin | `9da6b8ae5562481815f342b440e5adbee3942fd36a1c71aa68c37a2d9ff49496` | 47 899 519 |
| S0.optimizer.bin — исторические SFT-моменты, **не импортировать в DPO** | `a0d03e00f549ce8566c5d8c39399087c6538020e5052f29f7371c2f9fb9799e9` | 426 788 |
| S0.rng.json | `f819248e615f50e41c22db16d5fac5116809857d195d3b9d149c9b6ec580ba9b` | 1 573 |
| generation.json | `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40` | 1 520 |
| tokenizer.json | `e51c91d85ec486863f1862e49b378fa9f34873ee31a993114ba41995f38e5066` | 20 867 |
| data.source.jsonl | `5b265faaa2046a49449de0f5a44423a28df65bcf79b2c5fabc55e7d6796ba1f6` | 1 274 |
| data.material.json | `697aaa26b6b3de6fe01fde6e301868096e41877635df0e5f5d7074f01be6a5b4` | 12 343 |

Тему назвать **документация**, профиль C# postfix increment. TRAIN — единственная исходная строка `simulation-csharp-doc-f06-008`; prompt буквально:

~~~text
C# 12 / .NET 8. Explain this code's observable behavior in one sentence.
int x=10; int y=x++; System.Console.Write(y);
~~~

Chosen — `It writes 10 because postfix increment assigns the old value to y.`; rejected — `It writes 11 because postfix increment assigns the new value to y.`. Использовать исходный JSONL и его metadata, не перепечатывать новый dataset с иной provenance. Источник simulation, runtime verification C# в его metadata false; новый тест не превращает его в legal-teacher corpus. EXAM не назначен, новых DPO не генерировать.

Из generation/authority: V629/C2048/H512/L4/Q8/KV2/FF1408, q/v-LoRA rank8/alpha16/dropout0, base/root seed20261014, streams `rp01-policy` / `rp01-reference`. Сначала два независимых native `CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart` из B0, reference целиком freeze; только policy загрузить S0 после проверки `ValidateFullCheckpointArtifact` с topology q/v/rank8 и восстановить RNG. B0 — `training-artifacts/models/csharp-code-documentation/ilearning-unit/rp01-csharp-small/v1/checkpoints/000001/weights.bin`, weights SHA `a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911`, manifest SHA `c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d`; parent kind InitialBaselineNoUpdate, 55 parameter entries/39 base.

**Точная привязка native START к B0.** У существующего `PrepareStart` нет аргумента «открой step000001»: [service:99–132](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointStartService.cs#L99-L132) вызывает `LoadLatestCompatible`, который выбирает максимальный совместимый step ([loader:161–240](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/14.ContinuousLearning/Training/RepositoryOwnedModelCheckpointLoader.cs#L161-L240)). Поэтому имя B0 в плане не считается достаточной настройкой loader. Общий `PostfixDpoStateFixture` обязан выполнить следующие действия **до S0 import и любых Forward**:

1. После проверки SHA frozen `generation.json` построить существующий публичный `CodeDocumentationRepositoryOwnedModelCheckpointStartRequest` ([record:22–34](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointStartService.cs#L22-L34)). `RepositoryRoot=C:/Projects/llm_mvp_core`; slugs строго `csharp-code-documentation` / `ilearning-unit` / `rp01-csharp-small` / `v1`. `ModelConfig`, `LoraConfig`, `AdapterSeedAuthority`, `BaseInitializationSeed` и все шесть полей `RuntimeCompatibility` берутся из этого проверенного `generation.json`, без чтения текущей изменяемой app-конфигурации и без placeholder hashes. Tokenizer — уже назначенный `tokenizer.json` с проверенным SHA, без BPE Train. `MaximumCheckpointBytes` — прежний default native request. `ColdStartPolicy=RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied`.
2. Дважды независимо вызвать `PrepareStart(request, cancellationToken, initialOnly:false)`. Здесь **нельзя** использовать `initialOnly:true`: [loader:118–127](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/14.ContinuousLearning/Training/RepositoryOwnedModelCheckpointLoader.cs#L118-L127) при этом отвергает любое существующее checkpoint candidate, включая B0. Этот flag означает initial control без сохранённого состояния, а не выбор InitialBaselineNoUpdate.
3. У **каждого** успешного результата потребовать nonnull `ParentManifest`, `Step==1`, `ParentCheckpoint==null`, `DpoApplication==InitialBaselineNoUpdate`, точные `ManifestSha256`, `WeightsSha256` и `RepositoryRelativeWeightsPath` B0 выше, `ColdStartReason==null`,55 checkpoint entries/39 base entries. Сохранить обе квитанции выбора и actual native assembly hashes; совпавшие compatibility labels не заменяют происхождение текущих DLL. Два результата должны содержать разные model instances и независимые parameter buffers.
4. Только после этих проверок полностью freeze reference, загрузить S0 в policy и восстановить RNG по договору ниже. Если loader выбрал более поздний совместимый checkpoint либо canonical B0 недоступен, вернуть test-fixture defect `POSTFIX_B0_NOT_SELECTED` с фактическими manifest/step, **0 Forward/Train**. Это планируемая метка harness, не новый product error code. Не выбирать другой baseline, не менять manifests/model tree, не копировать его во временный root, не использовать cold start и не придумывать отсутствующий exact-step API.

Это сохраняет ту же модель B0, которую проверял исторический `AgcExperimentBridge.LoadModels`, и делает выбор проверяемым на текущем native API. Новый source/assembly fingerprint записывается отдельно; старые runtime-compatibility поля donor не выдаются за новый измеренный hash сборки.

RNG topology `f220de4520adbec3ddb8c23eabb0d485f6bb052f0c0dc9a50a8a5d9c3448cbfb`, LoRA configuration `4df9bab2d5115d8368c3f36fdf6c4f31ba8ed43bb65200c41a5c563795d02c35`. Materializer — native `LogicalDpoPreparedBatchMaterializer.Prepare`; exact carrier prompt75/chosen121/rejected118/padded121,46 response targets, digest `f7083c1cb18d7469779568ae09e23521449540f5351b3a764f64aeef88a55327`.

DPO optimizer свежий **0→1**, не SFT1000→1001; reference остаётся B0. Native `ProductionOptimizerSettings` сохраняются, включая LR1e−5/weightDecay0.01; beta/epsilon и KL beta0.1 записать из native конфигурации в receipt, не переопределять ради совпадения исторического результата. Один `StepVector([batch],1,timing)`; `data.material.json.order` описывает **прошлый producer1000**, его не выполнять.

Отсутствие donor/B0/payload или несовместимый hash — именованный setup defect до Forward; искать другой checkpoint, уменьшать модель или скрыто повторять producer запрещено. Большие файлы не дублируются в каждый fixture.

## 4. Постоянные тесты на основе эксперимента

Новые, пока **не существующие** файлы:

- `Tests/ExperimentFixes/GradientAccumulationLearningTests.cs`: один Fact `RestoredPostfixDpo_PreservesLearningState`;
- `Tests/ExperimentFixes/GradientAccumulationContractTests.cs`: три Fact ниже;
- helper/manifest из§3; namespace новых классов `LlmFromScratch.Tests.ExperimentFixes`.

Learning Fact восстанавливает назначенные policy/reference; снимает **один** teacher-forced TRAIN readout before (46/46), делает один native StepVector, снимает **один** after. Сохраняет predictions/NLL/ranks, 16 named adapter arrays, frozen/reference hashes, optimizer/RNG и timing; квалификация требует ненулевого изменения adapters, unchanged base/reference, optimizer1 и retained46/46. Ни inference-only parity, ни изменение весов без приобретённого before не заменяют этого теста.

Доступ к native DPO — существующий test-only шаблон [DpoStepVectorTimingTests:212–245](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/Tests/DpoStepVectorTimingTests.cs#L212-L245): `TestLease`, `DpoMutationAdmissionCapability.Issue`, `DpoProjectLoraRecipient.Capture`, `DpoExecutionProfile.Capture`, `CreateForProjectLora`. Не копировать historical issuer, не добавлять public bypass/friend assembly. Этот тест не доказывает product admission и ничего не публикует в canonical model store.

Измерение — ровно один native StepVector, включая его штатную policy/metrics publication; restore/readout/hash/export и внешние observers вне окна. Использовать `GC.GetTotalAllocatedBytes(precise:true)` на изолированном test process; закрепить scope. В historical host были дополнительные callbacks публикации optimizer/RNG и diagnostics; native A/B сравниваются между собой, не обязаны повторить абсолютное число байтов старого host. Сериализовать готовые native optimizer/RNG после окна без дополнительного Forward. Канонические имена policy/metrics — `DpoTrainer.BuildPolicyCheckpointFileName(1)` / `BuildMetricsFileName(1)` внутри arm-owned `state/dpo`.

**Бюджет одной руки:** 1 update,4 training forwards (2reference+2policy),2 backward,2 TRAIN diagnostic forwards; producer/SFT/EXAM/Generate/external0. A+B:2updates,8training forwards,4backward,4diagnostic. Один receipt на руку; остальные assertions и отчёты читают его без обучения.

Новые contract Facts используют native internal `AccumulateGrad` через уже существующий [InternalsVisibleTo](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/00.TensorCore/AssemblyInfo.cs), без новой product API:

| Новый Fact | Вход / независимый expected |
|---|---|
| `FirstContribution_IsOwnedAndBitPreserved` | target shape[4],RequiresGrad=true; incoming `[1f,-0f,float.PositiveInfinity,float.NaN]`. После первого вызова побитово сравнить4значения через SingleToInt32Bits; изменить incoming[0]=99, target.Grad[1]=7 — исходные буферы не влияют друг на друга. Проверить `NotSame` массивов. Это ownership, не quality claim |
| `RepeatedContribution_AccumulatesWithoutAliasing` | target[3],incoming1 `[1,-2,3]`,incoming2 `[4,5,-6]`: после двух вызовов `[5,3,-3]`; incoming1/2 не изменились. После target.ZeroGrad и ещё incoming1 — `[1,-2,3]`, ветвь существующего градиента сохранена |
| `FrozenTarget_DoesNotAcquireGradient` | target[3],RequiresGrad=false,incoming `[1,2,3]`; Grad остаётся null, incoming не изменяется |

Контрактные случаи не запускают модель/optimizer, но защищают именно метод настоящего обучения. Nonfinite input не отправляется в TRAIN; он проверяет побитовое копирование, а не допустимость нечислового градиента оптимизатором.

## 5. Существующие тесты и точный разбор падений

Свежие падения **не запускались и не установлены**. Сохранить assertions/tolerances и выполнить до/после один точный filter из этих пяти names (префикс `FullyQualifiedName=LlmFromScratch.Tests.`; объединение `|`):

| Method | Действие |
|---|---|
| `TensorCoreTests.Backward_AddOp_GradPassedToBothInputs` | Сохранить оба unit gradients |
| `TensorCoreTests.Backward_MultiplyOp_GradIsOtherOperand` | Сохранить untouched frozen input |
| `TensorCoreTests.Backward_MatMul_GradMatchesPyTorchReference` | Сохранить аналитический oracle |
| `TensorCoreTests.ZeroGrad_ClearsAccumulatedGradient` | Очистка остаётся действительной перед следующим накоплением |
| `LoraTests.LoraAttention_AllProjectionBGradientsMatchFiniteDifferenceThroughFinalNorm` | Сохранить настоящий путь q/v/attention/final norm и finite differences |

Если A зелёный, B красный — исправить alias/ветвь/непреднамеренное изменение конструктора; expected под B не менять. Если A уже красный, записать конкретный test ID/stack и классификацию [03§2](03-test-and-execution-contract.md); не чинить весь solution. Единственная заранее описанная build prerequisite — [02§10](02-learning-state-and-fixtures.md#native-build-prerequisite), одинаковая в A/B. Старые тесты не удалять и не Skip; отсутствие найденного нового Fact — setup error, не PASS.

## 6. Две native стадии, сохранение и E2E

Исполнитель расширяет **планируемый**, пока не созданный `Tests.Support/ExperimentTransfer/Run-NativeAB.ps1` по [03§9](03-test-and-execution-contract.md#native-ab-transfer) ровно записью FIX-AGC: output `artifacts/fbe/FIX-AGC/attempt-0001`, fixture§3, five existing / three contracts / one learning Fact на руку. Common harness commit создаётся **до** product diff; его SHA — `$baselineCommit`. Native A завершается в canonical `C:/Projects/llm_mvp_core`, branch `codex/dpo-implementation`, затем единственная product-замена/review/commit даёт `$candidateCommit`. Не создавать копий repo/worktrees, не делать checkout/reset/stash.

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-AGC -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit
# После законченного A и отдельно назначенного product diff/review/commit:
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-AGC -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit -CandidateCommit $candidateCommit
~~~

На stage — один Rebuild Tests/net9.0/Release, discovery точных IDs, затем existing→contracts→learning; **три test processes на stage**, два learning processes всего. Новый learning exact filter — `FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.GradientAccumulationLearningTests.RestoredPostfixDpo_PreservesLearningState`; contract class filter заканчивается `.GradientAccumulationContractTests.`. DLL Location/SHA/MVID для native TensorOps/DpoTrainer/LlmModel и SHA source/fixture сохраняются на каждую stage; B не принимает stale A DLL. Learning deadline15min/process, build15min/stage; это wall guard, не дополнительные updates.

A terminal receipts сохраняются до product edit. B читает именно их; уже завершённый A не считается поводом повторить fit. `Compare-NativeAB.ps1` читает состояния и создаёт comparison/report без model calls. Сравнение требует одного runtime/ISA, точной parity данных/градиентного результата/optimizer, всех ресурсных осей и фиксированного числа updates. Процент исторического выигрыша не CI threshold; measured native `B<A` подтверждает перенос расхода. Отсутствие выигрыша записывается, повтор ради удобного числа не назначен.

E2E из [20](20-prompt-domain-e2e.md#resource-transfer-e2e) использует прежний learned selector и настоящий выбранный documentation recipient; эта карточка не назначает router training. Состояние postfix Student не является состоянием маршрутизатора. После kernel transfer прежняя проверка recipient/state isolation читает готовые результаты; дополнительных neural updates0. Маршрутизацию не объявлять улучшенной из-за расхода TensorOps.

## 7. Готовый промпт и обязательное завершение

~~~text
Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/41-grad-accum-copy.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.

Выполни только отдельно назначенный FIX-AGC. Сейчас карточка — план, не запуск.
Default model текущей сессии, отчёты по-русски: report.md и report-for-user.md.
Skills в docs/skills: architect-behavior-classifier,phase-planning,plan-review,
code-examination,test-integrity,prompt-patch-authoring (все -SKILL.md),
human-communication-GLOSSARY.md; AGENTS/CLAUDE и общие02/03.
Один фактор: внешний ToArray в первом AccumulateGrad; TensorStorage копирует всегда.
S0 только h512-postfix-one/v1; referenceB0,freshDPO0; producer1000 не повторять.
A до product edit,B после,один canonical checkout; budgets/paths даны выше.
Ни runtime experiment switch,ни copied training assembly,ни обход product authority.
Любое измеренное полезное улучшение — успех по оси/профилю,accuracy расти не обязана;
покажи GC/heap regressions и границу teacher forcing. 0→0 не успешное обучение.
Один process/handle,жди terminal штатным wait в том же ходе; никакого blind polling,
повторного launch или конца раунда вместо ожидания. Интервалы ожидания≤60секунд.
При STOP/NEXT немедленно прекрати предыдущийID: не дописывай report/tests/commit/cleanup.
NEXT читай поID из01; старые experiments/results сохраняй.
Новых DPO здесь нет. Если отдельно потребуются: генерирует внешняя открытая китайская
модель; выбор в роли человека и оценку качества делает CODEX с astra 6 ultracode.
Не выдавай default executor за исполненную независимую оценку этой моделью.
~~~

Отчёт отдельно показывает `execution_status`, `learning_validity`, успех каждой полезной метрики, regressions и оставшиеся ограничения. Текущая работа проверила документы/исходники/raw; native tests, restore и обучение этого плана ещё не выполнены.
