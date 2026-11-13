# Нейронное обучение: текущая реализация и новый узкий кандидат

Аудитор: `/root/training_new_gaps`. Срез core: `0b76ab950d82534f5904005dda096545505ae307`; planning: `4b950599a04388941f56b41e634233ebb7dc17c2`. Выполнено чтение исходников и существующих карточек; native обучение, Python-прокси, сборка и продуктовые правки не выполнялись.

## Что в прежних находках уже изменилось

1. Знак градиента DPO и дробный mask множитель присутствуют в текущем `DpoTrainer.BackwardLogProbs`: [`DpoTrainer.cs:1071–1128`](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/11.DPO/DpoTrainer.cs#L1071-L1128). `targetScale = scale * mask.Data[i+1]`; `(oh-sm)*targetScale`. Предлагать повторное исправление старого знака или маски как нового дефекта этого среза нельзя.
2. Exact материал использует response mask и суммарный log probability; legacy material использует другую маску/reduction: [`DpoTrainer.cs:1002–1068`](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/11.DPO/DpoTrainer.cs#L1002-L1068). Поэтому старый вывод об objective относится к конкретному входному пути, не всему DPO.
3. LoRA-код действительно возвращает base(x)+alpha/r*B(A(dropout(x))), а параметры адаптеров доступны отдельно: [`LoraLinear.cs:110–131`](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/10.LoRA/LoraLinear.cs#L110-L131). Наличие формулы не доказывает положительный навык; с другой стороны, сохранённое улучшение NLL не следует обесценивать из-за нулевого exact.
4. Копии BPE и frozen transpose ещё **не перенесены в исходники**. [`BpeTokenizer.cs:540–566`](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/01.Tokenizer/BpeTokenizer.cs#L540-L566) создаёт новый список в каждом MergeChunk, включая отсутствие пары. [`Linear.cs:45–50`](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/00.TensorCore/Layers/Linear.cs#L45-L50) каждый раз транспонирует веса. Это уже измеренные кандидаты других опытов, а не новые гипотезы.
5. Условный first-step warmup отказ известен ранее: default `WarmupSteps=100`, scheduler.GetLr(0)=0, SFT отвергает LR≤0. Но штатная фабрика и сохранённый малый host задают WarmupSteps0; новый блокер текущего RP01 этим не установлен. Ссылка на уже известное: `61-LEARNING-UNBLOCK-AND-EXPERIMENT-AUDIT-2026-10-20/11-NEURAL-EXPERIMENT-REVIEW.md`.

## Что не является новой гипотезой

Прочитаны текущие карточки `N62-neural-update-costs` (R1/R2/E-OPT-STEP/LF-03), `N61-dpo-reference-reuse`, `NEURAL-FROZEN-TRANSPOSE-01`, `RMS-COORD-ALLOC-01`, соседние N-HEAD/GRADB объяснения. Не предлагаются повторно: перенос сравнения frozen base на spans, expVals scratch, копия initialGradient, metadata optimizer step вместо экспорта моментов, кэш reference logprobs, кеш транспонированных frozen matrices, устранение координатных массивов RMS, selected LM-head rows, обрезка PAD.

## Новый кандидат: повторное вычисление неизменных позиционных таблиц внутри уроков

**Рабочий ID: ROPE-TRAIN-TABLE-01. Статус: source-confirmed repeated computation; effect NOT_RUN.**

На каждом реальном forward внимание снова строит таблицы косинусов/синусов, хотя длина урока, размер головы и theta часто одинаковы на соседних шагах. Это расход внутри обучения. Гипотеза: ограниченное повторное использование тех же таблиц только в training path уменьшит allocated bytes учебного перехода при неизменных loss/learned state/readouts. Удерживаемая память станет больше; её требуется показать рядом. Ни ускорение, ни влияние на нулевые ответы RP01 ещё не измерены.

### Где это участвует в обучении

1. [`Trainer.TrainSupervisedNextTokenStep:102–145`](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/04.TrainingLoop/Trainer.cs#L102-L145) → `LlmModel.Forward(training:true)` → loss/backward/AdamW → q/v-LoRA state.
2. [`LlmModel.Forward:219–242`](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/03.TransformerModel/LlmModel.cs#L219-L242) проходит каждый TransformerBlock; [`TransformerBlock.Forward:46–64`](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/03.TransformerModel/TransformerBlock.cs#L46-L64) передаёт `training` в attention.
3. [`CausalSelfAttention.Forward:146–153`](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/03.TransformerModel/Layers/CausalSelfAttention.cs#L146-L153) вызывает `_rope.GetFrequencies(seq, cancellationToken)` и применяет таблицы к Q и K.
4. [`RotaryEmbedding.GetFrequencies:50–72`](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/03.TransformerModel/Layers/RotaryEmbedding.cs#L50-L72) каждый раз создаёт `frequencyData`, Tensor frequencies, `positionData`, Tensor positions, outer, cos и sin. `_dim` и `_theta` private readonly: [14–21](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/03.TransformerModel/Layers/RotaryEmbedding.cs#L14-L21).
5. [`TensorOps.ApplyRotaryEmbedding:1277–1363`](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/00.TensorCore/Core/TensorOps.cs#L1277-L1363) читает cos/sin в forward и backward, не пишет в них и не регистрирует их как обучаемые Children. Частоты здесь не learned state. Улучшение касается стоимости усвоения q/v параметрами.
6. Тот же механизм вызывается из [`DpoTrainer.ForwardPolicyLogProbs:1018–1030`](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/11.DPO/DpoTrainer.cs#L1018-L1030). Малый опыт ниже измеряет SFT/LoRA, **DPO updates=0**; эффект DPO не приписывается ему автоматически.

### Почему этот механизм отличается от уже описанных

Reference-cache хранит оценки сторон DPO; frozen-transpose хранит другое представление весов; RMS меняет адресацию broadcast. Здесь только константные позиционные cos/sin, зависящие от `(dim, theta, seqLen, positionOffset)`. Постоянство этих значений уже установлено в [protected47, строки31–32](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/47-current-attention-backprop-and-position-contract.md#L31-L32); оно не объявляется новым открытием. Новая гипотеза — однозаписочное повторное использование внутри настоящего TRAIN и измерение его стоимости при том же обучении. Старые материалы изучают backward/численную корректность, offset KV-cache, нечётный headDim, сохранение позиций при PAD-crop и конфигурацию; эти факторы не совпадают с предлагаемым A/B.

Дедуп повторён после материализации **всех734 Markdown** из pinned `possibleDifs@4b950599a04388941f56b41e634233ebb7dc17c2` по manifest, пропущенных Markdown0. Сканированы также330 локальных Markdown защищённого core каталога, включая все153 карточки. Поиск `\bRoPE\b|RotaryEmbedding|GetFrequencies|inv_freq|тригоном` дал17 planning и12 protected файлов; проверены совпадения и дополнительные русские/английские формулировки cache/frequency/position/table/cos/sin/reuse/precompute. Точного прежнего вмешательства training table reuse или такого эксперимента не найдено. Машинный перечень, pinned blobs и строки совпадений — [novelty.json](rope-novelty.json). Это поиск и адресное чтение совпадений, не последовательное прочтение всех734 документов и не доказательство отсутствия идеи во всех Git-ветках. Сканирование может пропустить иную формулировку; родительская проверка новизны остаётся полезной.

### Предметная применимость

Первый профиль: предметная тема программирование C#, задача определить знак целочисленного вычитания; actual learner — один LlmModel с q/v-LoRA. Механизм сам по себе не привязан к C# и может работать в переводе/документации при этом же Transformer, но эти домены не измерены этим рецептом. KNN/словарные Units без этого Transformer результата не получают. Синтаксис/порядок предложения не меняются: формулы, абсолютные позиции, theta и маски остаются исходными. Смещение KV-cache при генерации специально не затрагивается.

Подробная карточка с назначенным host/данными/бюджетом/командой — [06-ROPE-TRAIN-TABLE-01.md](06-ROPE-TRAIN-TABLE-01.md) рядом. Реализация host должна быть выполнена только по будущему явному поручению. В текущем проходе пишутся документы.
