# DesignToCode-clean: первичные neural receipts и новая условная проверка backward SIMD

Дата исследования: 19 октября 2026. Область: DPO/LoRA/BPE/TensorCore; только чтение исходников, сравнение сохранённых JSON и арифметика формул. **Native model, обучение, inference и внешние модели не запускались.** Текущая реализация не изменялась.

| Снимок | SHA |
|---|---|
| Указанная пользователем успешная ветка `DesignToCode-clean` | `e346b88a3cc1fcce48aefa626bfc7b00509eefd6` |
| `codex/dpo-implementation` | `358af351e22a92a3f7938d1f9221d5aeb7ddb053` |
| Начальная planning-версия для дедупликации | `b0308fbaa783758104987e6544d057062ccf6fc3` |

## 1. Что поставить в главу восстановления

**В clean действительно сохранены положительные neural preference receipts.** Их надо сохранить как самостоятельную контрольную линию. Они имеют разные знаменатели, а значение «1.000» не означает 100% самостоятельных ответов.

В этом learner `Accuracy` считает долю пар с положительным **reference-relative** margin: насколько preference policy сдвинулась к chosen относительно reference. Это также не равнозначно абсолютному превосходству вероятности chosen над rejected. В `StepMany` train-метрики рассчитаны на forward до update данной пачки; возвращение metrics после optimizer не превращает их в повторную оценку final checkpoint. [Первичная формула и счётчик в clean, 176–230][legacy-accuracy].

| Первичное свидетельство из clean | Что удалось заново прочитать | Что можно утверждать |
|---|---|---|
| V210, 11 мая | 134 пары, 9 minibatches, `FinalAccuracy=.8333333135`, `FinalRewardMargin=.0057025589` | Положительная сохранённая train preference метрика последней пачки. Последняя пачка содержит 6 пар; это 5/6, не 83.3% от 134 и не heldout. |
| Base-wiki repeat10, 15 мая | 500 строк, 32 пачки; последняя содержит 4 пары и имеет `Accuracy=1.000`, margin .023747 | В этом финальном train minibatch записаны 4/4 положительных предпочтений. Полная таблица присутствует; 1.000 не относится ко всем 500 строкам. |
| Wiki query-planning 023→025, 19 мая | 50/50 **точно одинаковых raw triples** `(a,chosen,rejected)`; по сохранённым pair diagnostics 49/50→49/50, средний margin .002350332304→.002355672860 | Узкий положительный сдвиг среднего reference-relative preference на той же панели. Новых положительных пар 0, потерянных 0; собственные свободные ответы в этих diagnostics не измерены. |

[V210 JSON][v210]; [repeat10 таблица, 6–15 и 19–51][repeat]; [назначение FinalAccuracy из Last(), 133–154][legacy-last]; [summary 023][sum023]; [summary 025][sum025]; [raw batch023][raw023]; [raw batch025][raw025]; [diagnostics023][diag023]; [diagnostics025][diag025].

Повторная сверка 023→025 сделана по полным текстам, с сохранением регистра и переводов строк. PairId разных runs не использован как идентичность урока: он только соединяет строку с её собственным сохранённым diagnostic. SHA256 канонического множества общих triples: `51000f71cf90475acc429dcc44fc7ea23ae6a924c4c89b81d49b36027c73c073`. Источники относятся к 19 мая, но SHA исполнявшейся тогда сборки в этих receipts не записан. Наличие файлов в октябрьском clean не делает октябрьский код кодом майского запуска. Веса этих runs в данном проходе не загружались и не пересчитывались.

**Приоритетный восстановительный опыт REC-CLEAN-NEURAL-01, известная линия P01/P03, не новая гипотеза:** восстановить отдельно ровно сохранённую пару policy/reference соответствующей серии, её model config, encoder и панель; воспроизвести ту же preference-метрику. Затем оценить потребителя, который должен отвечать пользователю, на отдельной предметной панели. Только если этот consumer действительно использует обученный checkpoint, можно переходить к сопоставлению полезного ответа до/после. Смысловой проектный planner, таблица Unit и neural DPO policy — разные возможные получатели. Найденное в предыдущем аудите изменение исторического output head остаётся отдельным известным основанием для head-vs-q/v опыта, а не основанием откатывать исправленный autograd или переносить старые role labels.

## 2. Что уже совпало с clean: не выдавать за новое утраченное обучение

- **BPE совпадает целиком:** в обеих ветках blob `d7c5e69308a32bf832ec4b087097a35b26f51d64`. Нынешние EOS/byte-budget/segmentation риски нельзя приписать изменению между этими двумя версиями данного файла. [Clean BPE][bpe-old], [current BPE][bpe-new].
- **LoRA forward совпадает по алгоритму:** `base(x)+(alpha/r)·((dropout(x)Aᵀ)Bᵀ)`; A имеет Kaiming initialization, в конструкторе с `RandomSeeds` — seeded вариант; B=0, q/v projection обращается к attached adapter. Изменения самого `LoraLinear` здесь касаются capture/restore RNG и идентичности потока. [Clean initialization/forward, 73–116][lora-old], [current initialization/forward, 88–126][lora-new].
- **Численный body attention совпадает.** Дельта файла — дополнительные методы RNG в интерфейсе adapter. Reshape→RoPE→GQA→mask→softmax→PV сохранён. [Clean attention][attention-old], [current attention][attention-new].
- **В AdamW сама формула update совпадает;** добавлено переносимое/восстановимое состояние и проверки. Это не устранение возможности domain interference и не доказательство ошибки восстановления. [Clean AdamW, 41–89][adam-old], [current AdamW, 75–107][adam-new].
- Известные темы не переименованы в новые: random B0/head recipient; общий префикс пары; FP32-суммы log-probability DLT-2; q-LoRA для одного causal key DLT-3; масштаб A при одинаковом forward; mixed legacy/exact masks; nonfinite metadata; Inf second moment; sparse AdamW age; ненулевая dropout-последовательность; SFT warmup/response masking.

## 3. N-ADJOINT-01 — изменился порядок FP32-редукции именно обратного MatMul

**Класс:** новый конкретный clean→current численный seam; гипотеза влияния на качество, **не доказанный дефект SIMD и не доказанная причина RP01**. Это уточнение общих P10 (composed gradient) / P110 (стоимость scalar/SIMD) с точной нынешней call-chain.

### Что сейчас и что было

В clean градиент левого операнда `MatMul(a,b)` суммирует произведения последовательно по `column`:

`gradA[...] += upstream[...] * b[...]`.

В current этот участок заменён на `SimdOps.MatVecRowUnpublished`. Он берёт блоки размером `Vector<float>.Count`, выполняет `Vector.Dot` для каждого блока, затем суммирует уже округлённые результаты блоков. Градиент правого операнда остаётся скалярным. **Прямой MatMul уже использовал этот SIMD helper в обеих версиях; найденная дельта относится именно к backward gradA.** [Clean TensorOps, 650–672][grad-old]; [current TensorOps, 650–678][grad-new]; [обе версии SIMD helper, 99–141][simd].

Маршрут действителен для neural learner: `DpoTrainer.StepVectorCore` → два policy Forward → `BackwardLogProbs` → `logits.Backward` → MatMul backward в head/attention/FFN/LoRA → gradients разрешённых A/B → AdamW. `Step` и `StepMany` также достигают `BackwardLogProbs`; фактический внешний RP01 caller надо сохранить в receipt опыта. Локальная замена gradient по промежуточному входу может изменять более ранние A/B; она не тождественна проверке только веса конечного output head. [DPO вызовы, 882–927][dpo-chain]; [Step/StepMany, 649–750][dpo-many]; [seed backward, 1072–1121][dpo-backward]; [LoRA forward][lora-new].

Формулы в вещественной арифметике одинаковы. FP32 сложение не ассоциативно, поэтому при взаимном сокращении вкладов разные порядки могут давать разные остатки. AdamW имеет epsilon `1e-8`; разность малого градиента иногда сильнее отражается на шаге, чем видно по абсолютной величине самого gradient. Это возможная связь с качеством, которую надо проверить на фактических градиентах и moments.

### Что вычислено здесь, и чего это не доказывает

В арифметическом примере для ширин 4, 8 и 16 взяты два блока. Первый содержит 1 и нули; второй — −1, `2^-26` и нули. Все произведения конечны. Для табличного первого шага AdamW выбраны `beta1=.9`, `beta2=.95`, `epsilon=1e-8`, `learning_rate=1e-5`, `m=v=weight=0`; это параметры иллюстрации, а не утверждение о фактических moments/настройках RP01.

| Величина | Результат вычисления формулы |
|---|---:|
| Точная сумма и последовательная FP32-сумма clean | `1.4901161193847656e-8` |
| Округлённые по блокам FP32-суммы | 0 |
| Первый формульный AdamW decrement при fresh m=v=weight=0 и scalar gradient | `5.984122708468931e-6` |
| Тот же decrement при block-rounded gradient 0 | 0 |
| Контроль одинаковых положительных степеней двойки | Обе суммы совпадают |

Это **арифметика формул**, а не запуск .NET `Vector.Dot`, JIT, настоящего q/v gradient или обучения. Реализация внутренней редукции Vector.Dot на целевой машине не измерялась. Малый пример показывает, почему одного `maxAbsError` без optimizer-последствия недостаточно; он не доказывает, что такие остатки встречались в RP01 или что scalar всегда точнее SIMD. Ненулевые исходные moments могут уменьшить, усилить или поменять такой эффект.

## 4. Малый опыт EXP-ADJOINT-01: проверка clean-поведения без отката всех исправлений

### Этап A — сначала существующее состояние, 0 обучающих updates

В будущей отдельно исполняемой диагностике снять operands и upstream для выбранных реально достигнутых MatMul.backward из одного действительного разрешённого DPO batch. Выбрать состояние с ненулевой LoRA B; при fresh B=0 нулевой A-gradient ожидаем и не является дефектом. Сохранить shapes, порядки осей, dtype, .NET version, CPU SIMD flags, `Vector<float>.Count`, policy/reference/tokenizer/config hashes и уже существующие optimizer moments.

В одноразовой памяти оценить три одинаковых contractions:

1. A — current managed SIMD backward.
2. B — только scalar gradA reduction из clean.
3. R — независимая FP64 reference с теми же исходными FP32 operands.

Forward, градиент правого операнда, все остальные операторы, данные и состояние остаются одинаковыми. Сначала сравнить две реализации именно с R: B не считается правильнее только из-за исторического имени.

**Измерения:** относительная L2-ошибка к R, max absolute error, число значимых sign flips, разница полного A/B-gradient и эффективного LoRA update при одних и тех же existing moments. Для sign flips заранее отсечь близкие к нулю координаты, например `|g_R| >= max(1e-7, 1e-3·max|g_R|)`; полный распределительный отчёт и список исключённых координат сохранить. Отдельно показать epsilon-normalized влияние меньших координат на AdamW; не выдавать их количество за semantic risk. Измерить время contraction.

Если current не менее точен относительно R, влияние на effective update мало и нет достигнутых нестабильных координат, гипотезу не продвигать к предметному обучению. Сама побитовая разница не означает ущерба.

### Этап B — только при установленном действительном trigger

Два одинаковых состояния текущего learner: current kernel против единственного экспериментального замещения gradA на clean scalar. Выбирается **один домен, 2 урока, 4 новых source-disjoint случая и 2 retention случая**, а не оба домена как обязательная матрица. Одинаковые разрешённые pairs, order, actual optimizer steps, m/v, LR, beta, reference, LoRA, tokenizer, decoding. Не запускать большой прогон500 и Windows CI для этой развилки.

| Домен | Небольшая закрытая панель | Основной предметный результат |
|---|---|---|
| C# documentation/trace | 2 TRAIN-пары на один выбранный контраст null/exception/order; 4 новых случая на перенос контраста и 2 retention случая | Собственное объяснение правильной ветки, условия исключения и порядка побочных эффектов; критические неверные утверждения отдельно |
| EN→RU | 2 TRAIN-пары на один выбранный контраст отрицания/роли/порядка; 4 новые комбинации и 2 retention перевода | Сохранение агента, объекта, отрицания, модальности; собственный ответ через обычный consumer |

Для выбранного домена использовать ровно тот путь, который действительно задействует Transformer/LoRA. Чистый kNN baseline без этой contraction служит отрицательным контролем применимости. Одна и та же панель из 6 запросов повторяется до обучения, после него и после cold restore: это 36 ответов для двух плеч, либо 30 при доказанно одном общем начальном baseline. Если исходно правильных ответов нет, retention-показатель не определён; не называть повторный неправильный ответ удержанием способности. Raw ответы и единая rubric сохраняются. Loss, reward margin, веса и скорость — диагностические величины, отдельно от полезного ответа. Расширение панели допустимо только после конкретного различающего сигнала.

**Фальсификаторы:** фактический training runtime не содержит изменённого kernel; нет различия градиента сверх заявленного допуска; current ближе к reference; начальные states расходятся; качество не улучшается либо появляются retention/critical ошибки. При отсутствии предметного улучшения статус `NO_OBSERVED_QUALITY_GAIN`, даже если градиенты и веса различаются.

**Что менять при подтверждении:** внутренний `TensorOps.MatMul` backward в `00.TensorCore`, с заранее выбранным более точным reduction algorithm либо контролируемым fallback для установленного режима. Не создавать новый semantic Unit. Формат A/B state, tokenizer и checkpoint topology не меняется. Продолжение с тем же checkpoint технически возможно, но траектория оптимизации становится иной: изменение numerical-kernel/version должно войти в receipt продолжения. Сохранить быстродействие как отдельный компромисс и не заменять весь код clean.

## 5. Новизна, скилы, границы ответственности

Поиск выполнен в зафиксированных planning possibleDifs и read-only core исследованиях; использованы запросы `MatVecRowUnpublished`, `SIMD`, `backward.*simd`, `scalar.*MatMul`, `Vector.Dot`, `порядок суммирования`. Найдены общий [P110 scalar/SIMD forward benchmark][p110], [P10 составная производная][p10] и [source review41][review41], который описывает управляемый backend. Точного выделенного clean→current gradA-контраста с проверкой existing AdamW update в просмотренном корпусе не найдено. Поэтому статус — **новое конкретное уточнение**, а не претензия на новую тему всей численной точности.

Прочитаны проектные `code-examination-SKILL.md` и `prompt-patch-authoring-SKILL.md`: call chain до формирования вывода, один bottleneck на сравнение, отрицательный контроль и предметная проверка. Раздел выше задаёт будущий опыт; не выдаёт план за выполненное исправление.

**Роли будущего DPO:** человеческую роль выбора случая выполняет CODEX с Astra 6 UltraCode; внешняя разрешённая открытая китайская модель генерирует обе стороны chosen/rejected; качество пары независимо оценивает CODEX с Astra 6 UltraCode. Архивные пары здесь только доказательства истории. Текущий анализ и формульная арифметика не объявляются фактическим вызовом этого пресета, внешнего teacher или запуском quality evaluation.

Приложения пакета: `source-integrity.json` (38/38 API Git blob SHA), `arithmetic-and-retained-receipts.json`, `read_only_evidence.py`. Скрипт переносим: `python evidence/dpo/read_only_evidence.py --source-root /path/to/clean-checkout --manifest /path/to/source-integrity.json --output /tmp/dpo-evidence.json`. Он читает только сохранённые данные, проверяет каждый реально прочитанный первичный файл по pinned Git blob SHA/SHA256/числу байт, не запускает .NET и не меняет checkout. Два исходных batch JSON имеют UTF-16LE BOM; проверка идёт до декодирования. Source copies — рабочая проверка, а не предложение дублировать весь исходный код в planning.

[v210]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/experiments-v2/INTERNAL-TRAIN-READY-V191/results/train-ready-v210-good-external-plus-internal-guide-full-20260511-014712/internal-train-ready-dpo-summary.json#L11-L23
[repeat]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/experiments-v2/INTERNAL-TRAIN-READY-V191/results/base-wiki-session-01-batch-004-repeat10-500-20260515-1100/internal-train-ready-dpo-summary.md#L6-L51
[legacy-last]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/experiments-v2/INTERNAL-TRAIN-READY-V191/runner/InternalTrainReadyDpoRunner/Program.cs#L133-L154
[legacy-accuracy]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/11.DPO/DpoTrainer.cs#L176-L230
[bpe-old]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/01.Tokenizer/BpeTokenizer.cs
[bpe-new]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/01.Tokenizer/BpeTokenizer.cs
[lora-old]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/10.LoRA/LoraLinear.cs#L73-L116
[lora-new]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/10.LoRA/LoraLinear.cs#L88-L126
[attention-old]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/03.TransformerModel/Layers/CausalSelfAttention.cs#L130-L342
[attention-new]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/03.TransformerModel/Layers/CausalSelfAttention.cs#L136-L348
[adam-old]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/04.TrainingLoop/Optimizers/AdamW.cs#L41-L89
[adam-new]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/04.TrainingLoop/Optimizers/AdamW.cs#L75-L107
[grad-old]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/00.TensorCore/Core/TensorOps.cs#L650-L672
[grad-new]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/00.TensorCore/Core/TensorOps.cs#L650-L678
[simd]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/00.TensorCore/Core/SimdOps.cs#L99-L141
[dpo-chain]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/11.DPO/DpoTrainer.cs#L882-L927
[dpo-many]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/11.DPO/DpoTrainer.cs#L649-L750
[dpo-backward]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/11.DPO/DpoTrainer.cs#L1072-L1121
[p110]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/experimets/cards/P110-numeric-matrix-cost.md#L16-L29
[p10]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/experimets/cards/P10-composed-gradient.md
[review41]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/41-current-dpo-gradient-chain-source-review.md

[raw023]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/batch-023-run-output.json
[sum023]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-mixed-confirmation-023-project-wiki-query-planning-flow-20260519174947/project-wiki-dpo-metric-summary.json#L8-L22
[diag023]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-mixed-confirmation-023-project-wiki-query-planning-flow-20260519174947/dpo-pair-diagnostics.jsonl

[raw025]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/batch-025-run-output.json
[sum025]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-promotion-confirmation-025-project-wiki-query-planning-flow-20260519175259/project-wiki-dpo-metric-summary.json#L8-L22
[diag025]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-promotion-confirmation-025-project-wiki-query-planning-flow-20260519175259/dpo-pair-diagnostics.jsonl

