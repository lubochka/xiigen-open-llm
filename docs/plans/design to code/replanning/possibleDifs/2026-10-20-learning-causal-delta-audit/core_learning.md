# Нейронное обучение: независимая проверка, сохранённые успехи и один новый фактор

Дата: 20 октября 2026. **Текущая работа — только чтение исходников и сохранённых результатов, расчёт по JSON и документация.** Новые build, тесты, обучение и переносы в основной код не выполнялись. Защищённый каталог `llm_mvp_core/docs/plans/replanning-possible-diffs` в GitHub не изменялся. Локальные `core_snapshot` и `branch_snapshots` содержат прочитанные копии для исследования.

**Главный результат:** `RMS-COORD-ALLOC-01` — единственная новая нейронная гипотеза этого дополнения. Она касается стоимости массива координат на каждый элемент RMS broadcast **внутри настоящего SFT/LoRA update**. Это не доказанное объяснение плохих ответов RP01 и не измеренный успех. Остальные рассмотренные идеи признаны известными или уже исправленными; новые номера для них не создаются.

## 1. Источники и границы проверки

Основной source pin — core [`ab583032562d6656de9dd5ca6fa0a325fd4b1f30`][core-pin]. Последующее сравнение с [`65d03a73a76810a0425a41c33d3c7eeaf4cdbc48`][core-late] содержит 156 изменённых/добавленных Markdown-файлов и не содержит изменений runtime-кода. Это позволяет сохранить один pinned native source в новом рецепте; последние документарные изменения рассмотрены отдельно.

Для нейронного аудита получены все 60 файлов директорий TensorCore, Tokenizer, TransformerModel, TrainingLoop, Inference, LoRA и DPO. Их размер и Git blob SHA сверены с remote manifest. Четыре дополнительные зависимости Core/DatasetPipeline также получены точно; новый малый host использовал бы явно перечисленные **36 source paths** из [manifest](RMS-COORD-ALLOC-01-SOURCE-MANIFEST.json), а не произвольно выбранный набор проекта. Большие файлы изучены по computational regions и связанным callers; получение файла не объявлено полным аудитом всех его governance/restore ветвей.

Исторические pins [`DesignToCode@8f20ceb8…`][design] и [`language_translate@f6032aff…`][translate] проверены адресно по `TensorOps`, `CausalSelfAttention`, `RotaryEmbedding`, `LoraLinear`. Их восемь полученных файлов также сверены по Git blob SHA. Split-half RoPE convention сохранился; прежние attention helpers создавали новые Tensor без нужного backward, а current использует дифференцируемые операции. Это контрдоказательство к безусловному откату старых слоёв, а не новое объяснение деградации или полная реконструкция исторического checkpoint.

Применены прочитанные навыки code-examination, claim-verification и architect-behavior-classifier; текст и паспорт согласованы с human-communication glossary, правилами планирования/проверки плана и кода общего аудита. Прямые требования пользователя об экспериментальных копиях, полезных показателях и documentation-only определяют границы работы.

## 2. Что текущий DPO/LoRA действительно делает

| Проверенный участок | Source-факт | Значение для диагноза |
|---|---|---|
| Exact objective | [`DpoExactPairMaterial:19–46`][material] задаёт exact response material; [`ComputeTargetLogProbs:1036–1068`][scores] использует shifted targets и mask, возвращая sum для exact и weighted mean для legacy. [`ExactObjective:1131–1141`][objective] вычисляет устойчивый softplus и его производную. | Нельзя приписывать всем нынешним режимам full-prompt mean или старую нестабильную loss. Соответствие форме DPO не доказывает полезный предметный результат. |
| Настоящий update | [`Step:650–697`][step] хранит policy graphs для chosen/rejected, передаёт противоположные коэффициенты в BackwardLogProbs и вызывает ApplyOptimizer. [`BackwardLogProbs:1083–1120`][backward] применяет `(oneHot-softmax) × upstream × mask` и вызывает `logits.Backward`. | Это действительное нейронное обучение. `DpoMetrics` вычисляются до optimizer и являются online train measure, не post-update heldout оценкой. |
| LoRA forward и инициализация | [`LoraLinear:92–125`][lora] создаёт случайную A и нулевую B; forward равен `base(x) + (alpha/r) × (dropout(x) Aᵀ) Bᵀ`. | При свежей B=0 первый gradient A закономерно нулевой по правилу цепи. Первый gradient B при этом может быть ненулевым. Это не доказательство разрыва autograd; сохранённый обученный C50 нельзя считать fresh B=0. |
| Исправленная embedding | [`Embedding.Forward:43–79`][embedding] хранит ID и weight child, backward суммирует repeated-ID вклады в таблицу при RequiresGrad. | Исторический обрыв уже исправлен. Нельзя заново поручать тот же repair или применять его как причину frozen-base RP01 без проверки обучаемого получателя. |
| Дробная маска | [`BackwardLogProbs:1083–1116`][backward] суммирует значения mask для legacy denominator и умножает вклад каждой позиции на её mask. | Исправление присутствует. Fractional legacy опыт не является доказательством дефекта обычного binary exact RP01. |

Проверка здесь адресная: она не аттестует автоматически все optimizer-resume, tokenizer identity, full checkpoint topology и последующие Unit/Flow consumers. Историческое успешное обучение другого получателя — например, памяти Executor — должно сохраняться отдельно от генерации этой нейросетью.

## 3. RP01 B0 и «500»: не смешивать знаменатели

Прочитанный [первичный плановый разбор RP01][rp01] различает случайную неподготовленную B0, 500 учебных пар и 50 optimizer updates. В нём сохранены C# free `0/3→0/3` и EN→RU free `0/4→0/4`, одновременно с изменением LoRA и небольшими положительными margin deltas. **Это не accuracy `0/500`: 500 — размер TRAIN, а не знаменатель экзамена.** Эти кампании здесь не перезапускались и их ответы независимо не пересчитывались; ссылка указывает происхождение контекста.

Отсутствие свободных правильных ответов в этих маленьких панелях не отменяет действительное изменение параметров, а изменение параметров не заменяет правильный ответ. Холодный старт, получатель обучения, objective, token/context бюджет и реальный consumer остаются известными причинными осями. Новый RMS фактор выделяет стоимость того же обучения и не заменяет эти содержательные вопросы.

## 4. Сохранённые полезные результаты при точности 0/24

Для этого дополнения заново прочитаны **12 первичных JSON receipts**: before/after `training-steps.json`, `cost.json`, `score-summary.json` для `embedding-sft8-01` и `dpo-mask-live8-01` на `ab583032…`. Полученные bytes сверены по Git blob SHA; суммы и средние пересчитаны десятичной арифметикой, без новых Student calls. [Машиночитаемый пересчёт](neural-history-recalculation.json) фиксирует числа и их области.

| Показатель | Embedding SFT8, A→B | DPO fractional-mask8, A→B |
|---|---:|---:|
| Завершённых native updates | 8→8 | 8→8 |
| Правильные / все ответы | 0/24→0/24 | 0/24→0/24 |
| Успешная native генерация | 24/24→24/24 | 24/24→24/24 |
| Средний online pre-step loss | 7.0169490625→6.9877417 | 0.69314783875→0.693147945 |
| Сумма времени восьми учебных шагов, мс | 24505.3504→24391.5929 | 36113.9900→34532.0568 |
| Полный harness, мс | 142083.4058→149435.7635 | 155791.2765→159018.7596 |
| CPU всего процесса, мс | 141593.75→149609.375 | 154890.625→157968.75 |
| Peak working set процесса, байт | 1737625600→1826639872 | 1958367232→2199101440 |
| Cumulative allocated bytes за жизнь процесса | 175607527112→175687971504 | 184322779688→184322688336 |

Первичные пары источников: EMB [шаги A][emb-steps-a]/[B][emb-steps-b], [cost A][emb-cost-a]/[B][emb-cost-b], [score A][emb-score-a]/[B][emb-score-b]; DPO [шаги A][dpo-steps-a]/[B][dpo-steps-b], [cost A][dpo-cost-a]/[B][dpo-cost-b], [score A][dpo-score-a]/[B][dpo-score-b].

У EMB уже первый B update меняет 18944 embedding-координаты и создаёт optimizer state, тогда как в A их изменение равно0; receipt weightDecay=0. Средний учебный loss уменьшился на **0.0292073625**. Это полезный измеренный результат процесса обучения, даже при прежних 0/24. В DPO каждый шаг сохраняет неизменность base/reference, а first-moment hashes демонстрируют участие реального updater; его средний loss, наоборот, ухудшился на **0.00000010625**. Эти изменения не следует скрывать округлением обоих значений до0.693.

Оба сохранённых сравнения показывают меньшую сумму elapsed времени восьми шагов, но большие total harness, CPU и process peak. У DPO также на91352байта меньше cumulative process allocations; у EMB на80444392байта больше. Последняя строка **не** является TRAIN-only allocation, retained memory или peak RAM. По первичным отчётам таймер EMB включает step receipt I/O, а DPO Step — native checkpoint publication. Единичный ресурсный выигрыш признаётся локальным успехом этой метрики; устойчивый speedup без повторных измерений не заявляется. Все ухудшения показаны рядом. Эти полезные исправления и их метрики уже описаны в [соседнем новом пакете кандидатов][positive-existing]; это подтверждение известных результатов, не новые гипотезы.

## 5. Единственная новая гипотеза: RMS-COORD-ALLOC-01

Исходная цепочка: [`Trainer.TrainSupervisedNextTokenStep:102–145`][sft] → [`LlmModel.Forward:219–242`][model] и [`TransformerBlock.Forward:46–64`][block] → [`RmsNorm.Forward:29–40`][rms] → [`TensorOps.MultiplyBroadcast:168–184`][broadcast] → native backward → AdamW → LoRA state → EXAM readout.

RmsNorm вызывает MultiplyBroadcast дважды: RMS scale `[B,T,1]` и gamma `[H]`. Внутри mapping loop каждый элемент проходит [`FlatToIndices:1008–1017`][coords], где исходник создаёт `new int[shape.Rank]`. Предлагается вычислять **тот же** индекс B целочисленными division/remainder справа налево, не создавая этот промежуточный массив. Сохраняются `mappedBIndices`, float arithmetic order, graph и backward. Это отдельный ресурсный фактор, не возвращение старого detached RMS и не замена смыслового алгоритма обучения.

Гипотеза: уменьшаются allocated bytes и, возможно, цена фактических учебных updates. Математическое совпадение mapping выведено по исходникам; runtime equality и ресурсный эффект **не измерены**. Тема панели — язык программирования C#; задача — определить знак целочисленного вычитания. Структура нормализации сама по себе не использует AST или порядок русских слов; широта возможного ресурсного эффекта не выдаётся за доказанный эффект в иных доменах. DPO проходит через тот же RMS, но в данном паспорте DPO steps=0.

Готовы исключительно документы:

- [Самостоятельная карточка](RMS-COORD-ALLOC-01-CARD.md): точные source paths/SHA, один diff, native модель и seeds, literal TRAIN/EXAM,4 SFT updates/arm, измерители, области расходов, отчёт и критерий успеха.
- [Промпт исполнителю](RMS-COORD-ALLOC-01-EXECUTOR-PROMPT.md): тот же default agent/model, русский отчёт, конкретный короткий Windows attempt, запрет продлевать предыдущую задачу после STOP/NEXT.
- [Literal source manifest](RMS-COORD-ALLOC-01-SOURCE-MANIFEST.json):36 конкретных файлов с текущими blob SHA; старый pin62bcc — только источник состава, текущий native pinab583032 указан отдельно.
- [Независимый review](rms-independent-review.md): `PASS_DOCUMENTED_DESIGN_NOT_RUN`. Исправлены конфликт предварительно созданного results, legacy runner args/meta, manifest до окончательного run.py, unknown counts при timeout и относительные ссылки отчёта.

Рецепт даёт новый no-arg runner только для будущего отдельно порученного исполнения: `C:/xg/RMS01/aNNN`, один tiny build, два процесса A/B, no retry и no deletion. Подготовка не создаёт results/bin/obj; run.py готовится до окончательных checksums; runner эксклюзивно создаёт results. Experiment-only switch/fixtures разрешены в лабораторной копии, не в production. Ни runner, ни экспериментальная копия метода **не созданы** этим аудитом. Текущий статус — `DESIGN_DOCUMENTED_HOST_NOT_MATERIALIZED_NOT_RUN`.

## 6. Проверка новизны и честное покрытие protected

Основной поиск охватил **678** manifest-listed planning Markdown на `f73d229c…`, **301** protected Markdown на `ab583032…` —92 корневых и209 в `experimets`. Дополнительно прочитаны18 новых planning Markdown на `a868f46ee3695728406173f8cd945e278ddb0def`, две новые N63 карточки core65d03a73, а добавленный текст всех156 изменённых Markdown из core compare проверен отдельным поиском. Итого999 полнотекстовых Markdown плюс один агрегат156 patches. Собственный новый RMS пакет исключён из поиска. Это полнота текстового поиска в названных snapshots, не заявление ручного чтения каждого документа или отсутствия идеи в иных ветках.

Все запросы, количество совпадений и точные строки сохранены в [RMS novelty search](RMS-COORD-ALLOC-01-NOVELTY-SEARCH.json):

| Запрос, регистр игнорируется | Совпадения | Смысловой разбор |
|---|---:|---|
| `FlatToIndices\|mappedBIndices\|RMS-COORD\|RMS-INDEX` | 0 | Точное описание исследуемого механизма и ID не найдено. |
| `MultiplyBroadcast` | 7 | Три planning и четыре protected mentions описывают текущий сохранённый autograd/правильное суммирование broadcast-gradient. Они не предлагают устранить массив координат на элемент. |
| RmsNorm/broadcast рядом с alloc/memory/cost/выдел/массив; русские «массив координат»/«индексный массив» | 3 | Restoration identity с RmsNormEps, HNSW ingress и scalar TeachingVector — другие механизмы. |
| RmsNorm/RMSNorm и нормализация рядом со стоимостью/массивами | 69 | Широкий контекст просмотрен адресно; найдены kept fixes, shape-условия, gradient composition и config provenance. Нового per-element coordinate cost протокола нет. |

Ближайшие исходные документы рассмотрены содержательно: protected [`41-current-dpo-gradient-chain-source-review.md:62–83`][prior-rms], [N62 neural update costs][n62], [P10 composed gradient][p10]; planning [N-SHAPE-01/02][shape], [новый GRADB-LOCALITY][gradb-known]. Первые сохраняют правильный autograd; N-SHAPE-01 относится к `Mean(keepDims:false)`, которого обычный RMS не вызывает; GRADB меняет порядок обхода **MatMul backward**, а RMS фактор меняет вычисление broadcast-индекса во **forward**. Поэтому RMS признан отдельным источником потенциальной цены native обучения, но пока **не покрытым существующим protected экспериментом**. Запрет менять protected соблюдён: новый паспорт добавляется снаружи, в possibleDifs, как кандидат эксперимента.

| Другой рассмотренный кандидат | Решение |
|---|---|
| `BackwardLogProbs` softmax scratch `new float[v]` на позицию | Уже OCT20-R2/LF-02 и N62; не новая карточка. |
| Tensor.Backward `initialGradient.ToArray` плюс TensorStorage copy | Уже LF-03/N62; не новая карточка. |
| Float response sums и cancellation policy-reference margin | Уже PG-09/DLT-2; нынешнее предложение double accumulator — refinement, не новая гипотеза. |
| Frozen Linear transpose/reuse | Уже NEURAL-FROZEN-TRANSPOSE-01. Его host — источник рецепта, cache-factor исключён из RMS A/B. |
| Warmup first-step0, Embedding/bias detachment, fractional mask | Уже описаны; current Embedding/mask исправлены. Caller-specific warmup нельзя объявлять общим active blocker. |
| Новые GRADB-LOCALITY/KNN cost из concurrent planning | Прочитаны и учтены как существующие отдельные кандидаты, не переименованы в собственную находку. |

Новый native ресурсный опыт допустим лишь при выполнении настоящих update. Любое измеренное улучшение полезного показателя считается успехом по этому показателю и данной теме/задаче; accuracy gain не обязателен. Неудачная сборка или timeout без receipt — технический/partial исход, не нулевая предметная точность. После STOP немедленно прекратить работу; после NEXT не продолжать предыдущий опыт ради отчёта, проверки или коммита. Старые опыты и результаты сохраняются.

[core-pin]: https://github.com/lubochka/llm_mvp_core/commit/ab583032562d6656de9dd5ca6fa0a325fd4b1f30
[core-late]: https://github.com/lubochka/llm_mvp_core/compare/ab583032562d6656de9dd5ca6fa0a325fd4b1f30...65d03a73a76810a0425a41c33d3c7eeaf4cdbc48
[design]: https://github.com/lubochka/llm_mvp_core/tree/8f20ceb8a9bfbadab2ad309226c47dd76e714548
[translate]: https://github.com/lubochka/llm_mvp_core/tree/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9
[material]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/11.DPO/Models/DpoExactPairMaterial.cs#L19-L46
[scores]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/11.DPO/DpoTrainer.cs#L1036-L1068
[objective]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/11.DPO/DpoTrainer.cs#L1131-L1141
[step]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/11.DPO/DpoTrainer.cs#L650-L697
[backward]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/11.DPO/DpoTrainer.cs#L1083-L1120
[lora]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/10.LoRA/LoraLinear.cs#L92-L125
[embedding]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/00.TensorCore/Layers/Embedding.cs#L43-L79
[rp01]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md
[emb-steps-a]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/before/training-steps.json
[emb-steps-b]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/after/training-steps.json
[emb-cost-a]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/before/cost.json
[emb-cost-b]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/after/cost.json
[emb-score-a]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/before/score-summary.json
[emb-score-b]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/after/score-summary.json
[dpo-steps-a]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/before/training-steps.json
[dpo-steps-b]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/after/training-steps.json
[dpo-cost-a]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/before/cost.json
[dpo-cost-b]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/after/cost.json
[dpo-score-a]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/before/score-summary.json
[dpo-score-b]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/after/score-summary.json
[positive-existing]: https://github.com/lubochka/xiigen-planning/tree/a868f46ee3695728406173f8cd945e278ddb0def/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-candidates-and-causal-gaps
[sft]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/04.TrainingLoop/Trainer.cs#L102-L145
[model]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/03.TransformerModel/LlmModel.cs#L219-L242
[block]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/03.TransformerModel/TransformerBlock.cs#L46-L64
[rms]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/03.TransformerModel/Layers/RmsNorm.cs#L29-L40
[broadcast]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/00.TensorCore/Core/TensorOps.cs#L168-L184
[coords]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/00.TensorCore/Core/TensorOps.cs#L1008-L1017
[prior-rms]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/41-current-dpo-gradient-chain-source-review.md#L62-L83
[n62]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md
[p10]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/P10-composed-gradient.md
[shape]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/51-LOCAL-WIKI-SQL-KNN-TENSOR-RECOVERY-2026-10-19/04-TENSOR-CONDITIONAL-GAPS.md
[gradb-known]: https://github.com/lubochka/xiigen-planning/blob/a868f46ee3695728406173f8cd945e278ddb0def/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-candidates-and-causal-gaps/new-gradb-locality-card.md
