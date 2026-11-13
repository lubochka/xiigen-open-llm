# FIX-FBM — не строить backward-карту для полностью замороженного broadcast

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q31](01-registry.md#work-fix-fbm). Механизм уже существует. Для этого поручения достаточно названного ниже наблюдения на работающем пути. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

<a id="q31-next"></a>
## Q31. Проверить один шаг на уже обученном состоянии

Оптимизация `MultiplyBroadcast` для двух замороженных входов принята в `4a727793306`. Осталось проверить её на обычном DPO-шаге с [готовым состоянием postfix](../../../../experiments-v2/RP01-MICRO/_shared-learned-starts/h512-postfix-one/v1). Брать единственную исходную пару FBM1 из раздела о доноре. Правильный ответ говорит, что постфиксный инкремент присваивает прежнее значение 10, ошибочный — новое значение 11. Восстановить принятый старт и прочитать предпочтение до и после одного шага, записать потерю, шаг и сохранность ответа. Стартовый DPO-оптимизатор у этого сценария идёт 0 → 1. [Существующие проверки ветвей и градиента](../../../../Tests/ExperimentFixes/FrozenBroadcastContractTests.cs). Список фактически найденных тестов даёт средство запуска. Расхождение исторических чисел 13 и 14 само по себе не требует написания дополнительного теста. Обширная первоначальная программа ниже остаётся описанием опыта.

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

Для FIX-FBM исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Стоимость относится к точно названному окну. Контрпример P09: разность allocations четырёх arm windows, делённая на4, включает создание входов/cache/receipts и не является постоянной стоимостью одного Forward. Для этой оптимизации указать размер работы, warmup, что внутри/снаружи счётчиков и одинаковую обвязку A/B; не закреплять точные bytes по одному такому среднему. Проверять существующие tests и дополнять их learned fixture, не объявлять отсутствующими по одному отчёту.

Применимость: FIX-FBM. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Полный ресурсный бюджет обязан включать обязательные диагностические вызовы и неудачные попытки, с отдельными единицами model/layer и optimizer-call/effective-update. Контрпример P11: 40 model Forward основной части +19 контрольных =59; 8 optimizer calls дают лишь4 изменения весов. Oracle/FD, заявленный обязательным, должен влиять на принятие результата, а не только выводиться в JSON. Равенство старому алгоритму подтверждать независимой проверкой, не комментарием в исходнике.

Применимость: FIX-FBM. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-FBM

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**M — метрика привязана к фактической стадии обучения.** Для обучающего loss/reward записать состояние, на котором он вычислен, отдельно от номера опубликованных после update весов. У DpoTrainer последняя StepMetrics — pre-update; не называть её оценкой конечного checkpoint. Пример P02: B уже стартовал с margin 0,002773 после SFT; отношение последних значений ×3,22 не равно отношению приростов DPO. Любое сравнение приобретения/ремонта использует собственное before каждого этапа и одинаковый reader. DPO Accuracy=(reward_margin>0) означает улучшение относительно reference, а не правильный ответ: T2 имеет policy chosen−rejected=−0,1827326 при положительном reward. TRAIN/EXAM correctness и сохранённые logP остаются отдельными полями. Если эта карточка сравнивает математически равные оптимизации, сохранить её A/B equality; не требовать искусственного роста точности и не добавлять post-update Forward, когда нужное чтение уже предусмотрено.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Уточнение после аудита 27 октября 2026:** зафиксировать guard **оба RequiresGrad=false**, unchanged numeric result и неизменную градиентную ветвь при хотя бы одном обучаемом входе. Проверка только fully-frozen случая пропустит потерю градиента смешанной пары. Приёмка должна читать actual native consumer и полное состояние после назначенного DPO, а не только loss либо fixture shape. Здесь нет reference-score cache: guard-before-cache и eviction из FIX-LC нельзя механически превращать в новое требование к FBM. Historical allocation gain и рост CPU/wall/GC показываются раздельно. Бюджет владельца сохраняется, ресурсный успех другой реализации не присваивается. Действует [03§24](03-test-and-execution-contract.md#transfer-audit-20261027). Это уточнение плана; новые проверки не запускались.

Дата: 22 октября 2026, UTC. Статус: **план кандидата; production, тесты и новые прогоны ещё не выполнены**. Основание — положительный FROZEN-BROADCAST-MAP-01 / FBM1. Проверенный срез: [ebaf7760a36d43172bd88317ff469edfd09a615b](https://github.com/lubochka/llm_mvp_core/commit/ebaf7760a36d43172bd88317ff469edfd09a615b). Порядок и следующий ID берутся только из [реестра](01-registry.md).

**Решение:** в `TensorOps.MultiplyBroadcast` пропускать создание `mappedBIndices`, когда **оба** входа имеют `RequiresGrad=false`. Значения умножения, shape, cancellation и существующая градиентная ветвь сохраняются. В настоящем одном DPO-обновлении исторического опыта это уменьшило allocation на **9 891 576 байт, 0,535%**, сохранив приобретённые 46/46 токенов ответа и те же адаптеры. Wall time, CPU и число GC при этом выросли; это кандидат по памяти обучения, а не установленное ускорение.

## 1. Основание, ограничения и новизна

| Доказательство на закреплённом SHA | Назначение |
|---|---|
| [FBM1 report](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/FBM1/results/report.md) | Исторический фактор, условие freeze, acquisition и ограничения |
| [Raw A](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/FBM1/results/A/metrics.json), [raw B](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/FBM1/results/B/metrics.json) | `steps[0]`, `learningSignal`, `frozenBroadcast`, `finalWeights` |
| [Source manifest](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/FBM1/source-manifest.json) | Опыт использовал изолированный host; исходная база `883496bca18113bf37e0afe683c75b05a0075048` не равна выполненному native переносу |
| [Готовый h512 donor](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-postfix-one/v1/bundle.json) | COMPLETE и `learning_qualified=true`, producer1000 уже завершён |

| Показатель одного StepVector | A | B | Интерпретация |
|---|---:|---:|---|
| Allocated bytes | 1 847 680 544 | 1 837 788 968 | −9 891 576, −0,535% |
| Teacher-forced TRAIN correct before→after | 46/46→46/46 | 46/46→46/46 | Приобретённый ответ сохранён |
| Mean NLL before→after | 0,000013250273→0,000013182895 | То же | Одинаковое изменение |
| Optimizer step | 0→1 | 0→1 | Настоящее DPO-обновление |
| Adapter delta L2 | 0,00225587145855641 | То же | 16 массивов, 53 248 значений |
| Wall ms | 10 357,8352 | 10 559,1312 | +201,296; ухудшение |
| CPU ms | 8 890,625 | 10 531,25 | +1 640,625; ухудшение |
| GC0/1/2 | 108/7/4 | 110/9/7 | +2/+2/+3 |
| Retained heap после шага | 622 051 000 | 534 046 256 | −88 004 744; наблюдение одного процесса |
| Retained heap после полной GC | 99 654 872 | 99 605 200 | −49 672 |
| Peak working set | 986 337 280 | 728 317 952 | Пик всего процесса; отдельное причинное снижение не установлено |

Нужный интервал — **`steps[0]`**, не расширенный `costs.training*`. В raw совпадают DPO loss `0.000019336709`, reward margin `11.076641`, carrier и объявленные конечные hashes: adapters `ac1e9e5945d8b362ba011bdc26f2fdb22a91691214e5a989de6c9b278e1ed997`, weights `8cbe9ccbbdbed652536c18bb81e1d2cc2c3d59982b997d8c1d7dfaa0b1b93c0e`. Эти StepVector loss/reward относятся к вычислениям шага до применения обновления; нового post-update DPO-loss в опыте нет. `comparison.json` при проверке доступен как LFS pointer; сверены полные A/B metrics, а не payload comparison или бинарные веса. Нативный запуск обязан проверить реальные bytes donor по SHA.

**Любое измеренное улучшение полезного показателя — успех именно по нему и этому профилю; роста accuracy не требуется.** Здесь найден успех по allocated bytes настоящего обучения. Нет доказательства улучшения генерации, обобщения, latency или пика на другом домене. 46/46 — один teacher-forced TRAIN, а не независимый EXAM.

План [30 / FIX-RMS](30-broadcast-coordinate-allocation.md) убирает другую стоимость: создание координат для broadcast. FIX-FBM убирает только ненужную backward-карту полностью frozen вызова. Не объединять факторы в одной A/B-паре и не складывать проценты. Если FIX-RMS уже принят в baseline, он должен быть одинаков в A/B и отражён в source passport; градиентную ветвь не переписывать попутно. [FIX-AGC](41-grad-accum-copy.md) использует тот же donor, но меняет другой метод. Его результат не заменяет FBM native-проверку.

## 2. Точный код, связь с обучением и математическое основание

Получатель — [00.TensorCore/Core/TensorOps.cs:148–214, MultiplyBroadcast](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/00.TensorCore/Core/TensorOps.cs#L148-L214). Сейчас `new int[data.Length]` создаётся после проверки совместимости форм, независимо от `RequiresGrad`, и сохраняет индекс B для каждого элемента A. Карта читается только из backward closure, которая вообще создаётся лишь при `a.RequiresGrad || b.RequiresGrad`.

Узкий diff: после существующих shape-guards и выделения `data`, **до** выделения карты добавить ветвь `if (!a.RequiresGrad && !b.RequiresGrad)`. В ней повторить тот же scalar forward: `flat` в прежнем порядке, `FlatToIndices`, тот же right-aligned `rankOffset`, прежнее вычисление `bFlat`, затем `data[flat] = a.Data[flat] * b.Data[bFlat]`. Сохранить проверку cancellation при `(flat & 4095) == 0`. Вернуть `new Tensor(data, a.Shape)` без карты, children и backward callback. Существующая ветвь с `mappedBIndices`, накоплением `aGrad`/`bGrad` и `AccumulateGrad` остаётся без изменений. `TensorStorage` продолжает владеть копией; общую память, runtime experiment switch и новую публичную API не вводить.

Не переносить условие на «оба leaf frozen» или «reference model»: решение определяется **фактическими** `RequiresGrad` входных тензоров. Если хотя бы один вход требует градиент, карта остаётся. Замороженный коэффициент может участвовать в backward другого входа; эту ветвь нельзя отключать. Shape errors проверяются до обоих путей, поэтому fast path не принимает ранее недопустимый broadcast.

Native потребитель — [RmsNorm.Forward:29–40](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/03.TransformerModel/Layers/RmsNorm.cs#L29-L40): squared→Mean→RSqrt, затем два MultiplyBroadcast. Он входит в policy/reference forward `DpoTrainer.StepVector` и в SFT. В историческом DPO-обновлении было 72 broadcast-вызова; **40 полностью frozen**, вместе 2 478 080 элементов. Удаляемые массивы находились именно внутри измеренного обучения. Frozen forward остаётся вычислением значений для loss/метрик; не требуется обучение самих этих промежуточных массивов, чтобы их стоимость влияла на шаг.

Обозначим right-aligned отображение индексов A в B через φ. Поэлементно:

\[
y_i=a_i b_{\varphi(i)},\qquad
\frac{\partial L}{\partial a_i}=g_i b_{\varphi(i)},\qquad
\frac{\partial L}{\partial b_j}=\sum_{\varphi(i)=j}g_i a_i.
\]

Карта хранит φ(i), а не параметры модели. Когда оба входа frozen, ни один из двух градиентов не запрошен графом: сохранённая карта не имеет читателя. Значение y вычисляется теми же операциями и в том же порядке. Если downstream trainable-узел использует y как константу, его градиент зависит от сохранённых значений y; они не меняются. Доказательство перестаёт применяться, если хотя бы один вход trainable, поэтому guard — необходимая часть изменения, а не эвристика.

Правило композиции локальных производных подтверждено [Proposition 9.4](https://github.com/lubochka/xiigen-planning/blob/d7b1704fd681fe0855afeb08ca5234433b5f9f6c/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0312-0317-ch09-motor-taxonomy-and-related-work.md#proposition-9-4). Отсутствие читателя карты при двух frozen input установлено отдельно по native closure выше. Книга не доказывает этот guard или runtime-выигрыш; их проверяют код, контрактные случаи и сохранённый опыт.

Предсказанный удаляемый payload `4 × 2 478 080 = 9 912 320` байт. Замер −9 891 576 отличается на 20 744 байта, около 0,21%; это независимая проверка механизма, не замена runtime-метрики. Контракты ниже проверяют обе стороны guard, rank alignment и сохранение backward. Выгода архитектурно возможна в разных темах при наличии frozen-вызовов; её величина зависит от shapes и графа. Для AST/KNN/порядка слов отдельного улучшения не доказано.

## 3. Один общий S0 и точные входы

Планируемый helper — **тот же** `Tests/ExperimentFixes/PostfixDpoStateFixture.cs`, чей полный договор восстановления задан в [FIX-AGC §3](41-grad-accum-copy.md#3-точный-donor-данные-и-восстановление). Если он уже создан для AGC, переиспользовать его файл и API, а не создавать второй producer/loader. Manifest этой карточки — `Tests/Fixtures/ExperimentFixes/v1/FrozenBroadcastMap01/manifest.json`. Все пути считаются от `C:/Projects/llm_mvp_core`; helper и manifest ещё предстоит реализовать.

Единственный donor: `experiments-v2/RP01-MICRO/_shared-learned-starts/h512-postfix-one/v1/`:

| Файл | SHA-256 |
|---|---|
| S0.weights.bin | `9da6b8ae5562481815f342b440e5adbee3942fd36a1c71aa68c37a2d9ff49496` |
| S0.rng.json | `f819248e615f50e41c22db16d5fac5116809857d195d3b9d149c9b6ec580ba9b` |
| generation.json | `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40` |
| tokenizer.json | `e51c91d85ec486863f1862e49b378fa9f34873ee31a993114ba41995f38e5066` |
| data.source.jsonl | `5b265faaa2046a49449de0f5a44423a28df65bcf79b2c5fabc55e7d6796ba1f6` |
| data.material.json | `697aaa26b6b3de6fe01fde6e301868096e41877635df0e5f5d7074f01be6a5b4` |

Настройки: V629/C2048/H512/L4/Q8/KV2/FF1408, q/v-LoRA rank8/alpha16/dropout0, base/root seed20261014, `rp01-policy`/`rp01-reference`. Два независимых native `CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart` из B0; reference остаётся B0 и frozen, policy после topology-validation загружает S0 и его RNG. B0 weights — `training-artifacts/models/csharp-code-documentation/ilearning-unit/rp01-csharp-small/v1/checkpoints/000001/weights.bin`, SHA `a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911`, manifest SHA `c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d`. RNG topology `f220de4520adbec3ddb8c23eabb0d485f6bb052f0c0dc9a50a8a5d9c3448cbfb`, LoRA identity `4df9bab2d5115d8368c3f36fdf6c4f31ba8ed43bb65200c41a5c563795d02c35`.

**B0 выбирается по полному START-контракту41§3, а не по названию donor.** Shared helper строит существующий `CodeDocumentationRepositoryOwnedModelCheckpointStartRequest` из проверенного frozen `generation.json`, задаёт canonical root/slugs, все реальные compatibility fields и `ColdStartPolicy.Denied`, вызывает дважды `PrepareStart(..., initialOnly:false)`. На текущем native API service использует latest-compatible discovery; поэтому оба возвращённых `ParentManifest` должны явно пройти Step1/ParentCheckpoint=null/InitialBaselineNoUpdate, pinned path/manifest SHA/weights SHA и55entries/39base до S0 import и Forward. Более поздний выбранный checkpoint — `POSTFIX_B0_NOT_SELECTED`, без обучения или подбора другого baseline. `initialOnly:true` не подходит: [loader:118–127](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/14.ContinuousLearning/Training/RepositoryOwnedModelCheckpointLoader.cs#L118-L127) отвергает **любой** существующий candidate, включая B0. Нового product API, временного model tree и обхода native loader нет; все проверки выполняются в уже назначенном restore, добавляя0Forward/Train.

Тема **документация**, профиль C# postfix increment. Одна исходная JSONL-строка `simulation-csharp-doc-f06-008`; exact prompt:

~~~text
C# 12 / .NET 8. Explain this code's observable behavior in one sentence.
int x=10; int y=x++; System.Console.Write(y);
~~~

Chosen `It writes 10 because postfix increment assigns the old value to y.`; rejected `It writes 11 because postfix increment assigns the new value to y.`. Источник simulation, C# runtime verification false; сохранить provenance, не называть legal-teacher набором. Carrier через native `LogicalDpoPreparedBatchMaterializer.Prepare`: prompt75/chosen121/rejected118/padded121,46 response targets, SHA `f7083c1cb18d7469779568ae09e23521449540f5351b3a764f64aeef88a55327`.

**DPO optimizer свежий 0→1**, native `ProductionOptimizerSettings` LR1e−5/weightDecay0.01, KL beta0.1, остальные параметры фиксируются из native config без настройки под результат. Исторические SFT1000-моменты `S0.optimizer.bin` **не импортировать**; `data.material.json.order` с 1000 шагами описывает producer, его не выполнять. Только один `StepVector([batch],1,timing)` на руку. Если donor/B0/LFS payload отсутствует или hash не совпал, записать конкретный setup defect до Forward. Запрещены поиск другой модели, новый bootstrap, уменьшение модели и замена reference на S0.

## 4. Тесты непосредственно из механизма опыта

Планируемые файлы: `Tests/ExperimentFixes/FrozenBroadcastLearningTests.cs`, `Tests/ExperimentFixes/FrozenBroadcastContractTests.cs`, общий helper и manifest из§3. Namespace `LlmFromScratch.Tests.ExperimentFixes`. Это будущие файлы; настоящая публикация карточки не утверждает их существование.

Один learning Fact **`RestoredPostfixDpo_PreservesLearningState`**: независимые policy/reference из donor, один teacher-forced TRAIN before, один native StepVector, один after. Передать native DPO через существующий test-only lease/capability/recipient pattern [DpoStepVectorTimingTests:212–245](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/Tests/DpoStepVectorTimingTests.cs#L212-L245). Не копировать исторический issuer и не ослаблять production admission.

Квалификация: before46/46, after46/46, optimizer0→1, ненулевой delta адаптеров, unchanged base/reference. A/B обязаны иметь один carrier, одинаковые raw predicted IDs/ranks/NLL/loss, named adapter values и состояние optimizer/RNG. Результат отвечает за сохранение приобретённого ответа и стоимости update; он не объявляет acquisition нового правила или обобщение. Новый post-update DPO-forward ради ещё одной loss-метрики не назначен.

Область замера — один StepVector с его штатной publication; restore/readout/hash/export вне окна. Снимать allocated bytes через `GC.GetTotalAllocatedBytes(precise:true)` в отдельном test process, wall/CPU/GC/heap/peak working set рядом. Historical host имел дополнительную диагностику/callbacks; native A/B сравниваются друг с другом, не с абсолютным allocation исторического host. Экспорт текущих optimizer/RNG после окна не должен запускать модель. Native policy/metrics filenames получать через `DpoTrainer.BuildPolicyCheckpointFileName(1)` / `BuildMetricsFileName(1)` в arm-owned `state/dpo`, не изобретать несовместимые RNG-имена.

**Бюджет одной руки:** 1 DPO-update,4 training forwards (2reference+2policy),2 backward,2 диагностических TRAIN forwards. A+B:2updates,8training forwards,4backward,4diagnostic. Producer/SFT/EXAM/Generate/teacher вызовов0. Один learning receipt на руку; повтор assertions/report читает его без обучения.

Новые contract tests — **9 cases**, без model/optimizer:

| Название | Условие и независимый oracle |
|---|---|
| `FrozenValues_PreserveDataAndStorage` — Fact | A[2,3]=[1,2,3,4,5,6], B[3]=[0.5,−1,2], оба frozen. Выход[0.5,−2,6,2,−5,12], RequiresGrad=false; после мутации входных массивов выход прежний |
| `FrozenRankOffset_PreservesRightAlignment` — Fact | A[2,2,3]=1..12, B[2,1]=[2,3]. Выход[2,4,6,12,15,18,14,16,18,30,33,36] |
| `RequiresGradCombinations_PreserveBackward` — Theory,4cases | Входы первого Fact, flags FF/TF/FT/TT. При хотя бы одном true upstream[1..6]: dA=[0.5,−2,6,2,−5,12], dB=[17,29,45]. Grad возникает только у запрошенных входов. FF проверяет отсутствие trainable result/leaf grads без Backward |
| `FrozenInvalidShapes_PreserveErrors` — Theory,2cases | A[2,3],B[1,2,3] — rank violation; A[2,3],B[2,2] — несовместимые dimensions. Оба frozen; прежний ArgumentException |
| `PreCancelledFrozenBroadcast_Throws` — Fact | A[4097],B[1],оба frozen, заранее отменённый token. OperationCanceledException до первого умножения; cancellation из fast path не удалён |

Контракты проверяют native `MultiplyBroadcast` напрямую. Они не выделяются в самостоятельный «эксперимент без обучения» и не заменяют learning Fact; это защита конкретного кода из настоящего training path.

## 5. Существующие тесты и что делать с падением

**Существующих свежих падений здесь не воспроизводили.** До/после сохранить следующие четыре native проверки, их assertions и исходные tolerances:

| Полное имя после `LlmFromScratch.Tests.` | Решение |
|---|---|
| `LoraTests.TensorOps_MultiplyBroadcastBackwardReducesPerRowScale` | Сохранить reference/finite-difference oracle; защищает trainable B reduction |
| `LoraTests.TensorOps_MultiplyBroadcastBackwardReducesGammaAcrossRows` | Сохранить суммирование по строкам и gamma gradient |
| `LoraTests.RmsNorm_BackwardMatchesReferenceForInputAndGamma` | Сохранить оба аналитических градиента нормализации |
| `LoraTests.LoraAttention_AllProjectionBGradientsMatchFiniteDifferenceThroughFinalNorm` | Сохранить связь q/v и конечной нормы; не ослаблять tolerances |

Источник — [LoraTests:696–920](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/Tests/LoraTests.cs#L696-L920). Никакой existing test не должен ожидать факт выделения карты при frozen input: публичный контракт — значения/gradients/ошибки. Если B ломает один из этих тестов, чинить guard/индексацию/cancellation в назначенном методе, а не expected. Если A не запускается из-за native build prerequisites, применять только одинаковый A/B порядок [02§10](02-learning-state-and-fixtures.md#native-build-prerequisite); отдельно зафиксировать root cause и не объявлять baseline PASS.

Exact existing filter — OR четырёх полных имён с оператором `FullyQualifiedName=`. Contract filter `FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.FrozenBroadcastContractTests.`; learning exact filter `FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.FrozenBroadcastLearningTests.RestoredPostfixDpo_PreservesLearningState`. Discovery должен показать **4existing +9contracts +1learning=14 cases** на каждой стороне; отсутствие или лишние случаи — ошибка фильтра, не повод запускать весь suite. Эти четыре existing теста используют маленькие native operations/finite difference и не назначают DPO/SFT updates.

## 6. Native A до изменения, B после; команды и артефакты

Следовать [03§9 native-ab-transfer](03-test-and-execution-contract.md#native-ab-transfer): один checkout `C:/Projects/llm_mvp_core`, branch `codex/dpo-implementation`, sequential native product A/B. `Tests.Support/ExperimentTransfer/Run-NativeAB.ps1` и `Compare-NativeAB.ps1` — **планируемые общие helpers**, не существующие команды на текущем SHA. Сначала реализовать их контракт и одинаковые tests/fixture/helper на исходном production; зафиксировать `$baselineCommit`. Выполнить A, сохранить terminal receipt. Только затем изменить назначенную ветвь `MultiplyBroadcast`, сделать `$candidateCommit`, выполнить B. Не создавать временный checkout или переключатель алгоритма.

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-FBM -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit
~~~

После завершённого A и отдельного product commit:

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-FBM -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit -CandidateCommit $candidateCommit
~~~

Для каждого stage runner делает один `dotnet build Tests/LlmFromScratch.Tests.csproj -c Release -t:Rebuild`, один discovery назначенных14cases, затем **existing → contracts → learning**, три отдельных test processes, все `-c Release --no-build`. Итого2Rebuild/2discovery/6test processes; новая модель обучается лишь в двух назначенных learning Facts. A/B tests/helpers/fixtures/csproj одинаковы; допустимый product diff — только `TensorOps.MultiplyBroadcast`. В B проверить загруженный native TensorCore/TransformerModel/DPO DLL Location/SHA/MVID и source SHA; stale A assembly — build defect, не parity.

Watchdog:15min на Rebuild,15min на learning process,5min на каждый existing/contract process. Deadline не добавляет updates; незавершённый stage остаётся именованным unfinished/failed, без auto retry и без необоснованного объявления процесса убитым.

Output: `artifacts/fbe/FIX-FBM/attempt-0001/`; subpaths `baseline/A`, `after/B`, `contracts/A`, `contracts/B`, `learning/A`, `learning/B`. До работы CreateNew `intent.json`; B отдельно создаёт `candidate-binding.json`, связывая точные SHA и исходный intent. В arm: `ownership.json`, `status.json`, TRX/stdout, `native-assemblies.json`, `input-manifest.json`, `before.json`, `after.json`, `train-cost.json`, `adapter-delta.json`, optimizer/RNG snapshots, completed/not_reached counters. После B read-only comparer делает `comparison.json` и русский `report.md`; новый Forward ради оформления отчёта запрещён. При уже существующем output прочитать ownership/status: живой handle ждать; terminal использовать; незавершённый чужой/утерянный owner оформить defect. Не удалять, не перетирать и не назначать самовольно attempt-0002.

Проход кандидата требует сохранности learned state/контрактов и измеренного allocation advantage **native** B. Не делать flaky CI assertion «B обязательно быстрее на0,535%»: процент исторический, timing не подтверждён. Если свежая native пара не показывает полезного улучшения, сохранить факты и статус «native benefit not reproduced», без новых автоматических прогонов; исторический выигрыш не стирать. Любое другое полезное измеренное улучшение также фиксируется по своей оси вместе с ухудшениями.

## 7. Связь с prompt→domain и границы применения

FBM не меняет router/KNN, выбор домена, authority или вердикт. В [FIX-E2E](20-prompt-domain-e2e.md) его native TensorCore может стать частью уже выбранного документационного learner. Подключать только после решения о переносе; использовать прежний route/назначенный learned state и evidence, не добавлять новый TRAIN смешанных тем. Если целевой route выбирает другой профиль или граф вообще не имеет fully frozen broadcast, величина выигрыша FBM неизвестна. Проверка маршрутизации не заменяет измеренное DPO-обучение данной карточки. Новый независимый router/E2E запуск этой карточкой не назначен.

## 8. Короткий промпт исполнителю и завершение

~~~text
Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/42-frozen-broadcast-map.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.

ID FIX-FBM; русский report: artifacts/fbe/FIX-FBM/attempt-0001/report.md.
Исполняющая модель — default текущей сессии. Прочитай AGENTS/CLAUDE и02/03.
Skills в docs/skills: architect-behavior-classifier-SKILL.md,
trainable-architecture-decision-SKILL.md,phase-planning-SKILL.md,
plan-review-SKILL.md,code-examination-SKILL.md,test-integrity-SKILL.md,
experiment-design-SKILL.md,prompt-patch-authoring-SKILL.md,
human-communication-GLOSSARY.md; остальные обязательные роли —03§4.
Задача — только выбранный ID и условия настоящей карточки; не расширяй scope.
S0=h512-postfix-one/v1; referenceB0;freshDPO0→1;0bootstrap/новыхDPO.
Один canonical root C:/Projects/llm_mvp_core; A до product diff,B после.
Меняется лишь карта fully frozen MultiplyBroadcast; gradients остаются прежними.
Любое измеренное полезное улучшение — успех по оси/профилю,accuracy расти не обязана.
Покажи GC/time/CPU regressions; 46/46 — teacher-forced TRAIN, не EXAM.
Жди тот же process/handle до terminal штатными wait-tools в этом же ходе;
интервалы≤60секунд, никаких blind polling/новых watcher loops/повторных запусков.
Не заканчивай ход вместо ожидания. STOP/NEXT немедленно прекращает прежний ID:
запрещено дописывать его report/tests/commit/cleanup. NEXT только из реестра01.
Старые experiments/results не удаляй. Отчёт строится из raw, без повторного Train.
Если отдельно назначат новые DPO: генератор — внешняя открытая китайская модель;
отбор в роли человека и quality оценка — CODEX с astra 6 ultracode.
Не выдавай default executor за уже выполненную оценку этой моделью.
~~~

Ожидание `exec` session выполняется через `write_stdin` того же session_id; yielded functions-cell — через `functions.wait` того же cell_id, только если он действительно выдан. Не подменять это немедленным чтением ещё не готовых файлов. Лимиты, исключения и режим terminal/deadline — [03§5](03-test-and-execution-contract.md). Текущая карточка создана по проверенным источникам и raw; её native restore, тесты и обучение **не запускались**.
