# Независимая проверка нейронных дополнений

20.10.2026. Проверены три файла `audit_work/dpo_lora/NEURAL-CURRENT-STATE-AND-NOVEL-HEAD.md`, `N60-PAD-ORIGINAL-CODE-BLUEPRINT-AND-PASSPORT.md`, `ACTIVE-NEURAL-EXPERIMENT-SEMANTIC-REVIEW.md`. Проверка — чтение предложений, адресное сопоставление original source и сверка байтов документированного JSONL. Native runtime, tensor/operator probes, обучения или генерации **не было**.

**Решение:** документы пригодны для доказательной публикации; критической ошибки в предложенном разграничении PAD/HEAD, сдвиге target, сохранении prompt или числе обновлений не найдено. Native implementation ещё не существует и не считается проверенной. Ниже два явных инварианта будущей реализации, которые стоит сохранить рядом с blueprint, чтобы правильный замысел не потерялся при исполнении.

Code pin: `bf9b543e149ef197da85a767821d98c9b67916d2`; актуальные protected карточки и policy — `f5dd532045716495a2c7668573250e5d7cc46810`. По прочитанному compare эти neural source файлы между pins не изменены.

## 1. Проверенные обязательные свойства

| Проверка | Решение и evidence |
|---|---|
| PAD не выкидывает prompt | **Верно.** Предлагается own-length prefix IDs/mask после full carrier validation; это весь prompt и весь response короткой стороны. [Exact material L37–46](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/Models/DpoExactPairMaterial.cs#L37-L46) задаёт именно эти длины |
| HEAD сохраняет Transformer context | **Верно.** Выбор строк предложен после полного ForwardHidden/final norm, до прежнего LM-head. [LlmModel L219–265](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/03.TransformerModel/LlmModel.cs#L219-L265) подтверждает separable seam |
| Target сдвинут на один | **Верно в замысле.** Current [DPO L1044–1050](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L1044-L1050) использует logits row i для ids[i+1] и mask[i+1]. HEAD сохраняет явный original-target mapping; предупреждает не использовать новый R как прежнее T |
| Последний target PAD не теряется | **Верно.** Blueprint запрещает Length−1 crop: retained IDs/mask длины L включают target L−1. Full causal input имеет L строк; последняя оцениваемая logit row L−2 |
| Existing Gather нельзя применять как differentiable select | **Верно.** [TensorOps.Gather L1477–1490](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/00.TensorCore/Core/TensorOps.cs#L1477-L1490) возвращает новый Tensor без graph edge. Это риск предложенного изменения; current DPO его сейчас не использует |
| PAD и HEAD — разные факторы | **Верно.** PAD меняет длину hidden/attention вычисления короткой стороны; HEAD оставляет прежний общий T Transformer и уменьшает только число выходных проекций. В каждом паспорте другой фактор явно выключен |
| Reference/policy роли и counts | **Верно.** [StepVector L875–905](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L875-L905): 2reference+2policy forward и2policy backward на пару; optimizer1 на minibatch [L917–923](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L917-L923) |
| При четырёх batch1 updates | **Верно:** 4 StepVector/optimizer,16 forwards,8 backwards суммарно A+B. Это не28 уроков. Generate0 позволяет ресурсный вывод, answer accuracy NOT_MEASURED |
| ORDER6 updates | **Верно, не8.** [Latest23 L176–180](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L176-L180) задаёт один DPO batch из двух пар +2SFT updates на плечо: 2+4=6. DPO forwards16/backwards8 относятся к четырём pair presentations |
| Измерение gradients после возврата | **Корректно запрещено как parity proof.** Current ZeroGrad сразу после optimizer; [timing type L14–27](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoStepVectorTiming.cs#L14-L27) содержит scalar times/counts, не gradients |
| Ресурсная сопоставимость | **Учтено:** копирование B входит в его цену; разные свежие процессы для per-arm peak, build/load/JIT отдельно, nested phases не складываются. Raw timings forward helpers включают likelihood reduction, ApplyOptimizer включает recipient/parameter validation; это не чистые kernel times |
| Policy20.10 | **Учтено.** Нет повторного личного approval каждогоSHA, PREPARABLE не blanketBLOCKED; consumer/restore допускаются при точной зависимости без лишнего нового Train |

## 2. NR-01 — явная граница HEAD rows для будущего исполнителя

Это **уточнение уже правильного замысла**, не новая ошибка кода. В точной форме полезно записать:

Пусть `p = ExactMaterial.PromptLength`, `L = ChosenLength` либо `RejectedLength`, `T = SequenceLength`. Current exact маска положительна на target positions `p … L−1`; по [material validation L120–130](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/Models/DpoExactPairMaterial.cs#L120-L130) p≥1, response непуст.

- Selected hidden rows для HEAD: **p−1 … L−2**.
- Соответствующие targets: **ids[p] … ids[L−1]**.
- Первая выбранная hidden row — **последняя позиция prompt**, предсказывающая первый response token.
- Число scored rows R=L−p. Последняя response hidden row L−1 не нужна для likelihood наблюдённых targets; prompt hidden states целиком продолжают участвовать в Transformer attention/backward.
- Полный Transformer HEAD всё равно получает T токенов. PAD не включается.
- При общем sparse варианте хранить отсортированный selected-row index и исходный target index/weight для каждой строки; не менять `mask[i+1]` на `mask[i]`, не делать цикл до R−1 и не терять последнюю цель.

Документ HEAD уже говорит `mask[i+1]` и запрещает старый `i<t−1` на сокращённых logits. Выше только снимается частая двусмысленность выражения «response hidden rows».

## 3. NR-02 — отсутствие graph edge нельзя принять за законный нулевой эффект

Это **обязательный инвариант реализации**, не требование отдельного тестового проекта. В нынешнем коде [Tensor.Backward L74–103](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/00.TensorCore/Core/Tensor.cs#L74-L103) проходит `Children` и вызывает `BackwardFunc`. Новый row-select должен сохранить RequiresGrad и parent edge на full hidden tensor, а backward — накопить значения по исходным индексам:

`gradHidden[rows[j], h] += gradSelected[j, h]`.

Формат/границы/порядок индексов фиксируются, массив индексов не должен измениться между forward и backward. Нельзя заново собрать detached hidden Tensor из Data и ожидать, что optimizer получит прежний градиент. Реальная downstream Linear использует MatMul, [Linear L45–60](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/00.TensorCore/Layers/Linear.cs#L45-L60); правильная цепь должна соединять selected rows обратно с теми же q/v LoRA.

В PAD паспорте предложено `abs(a-b) <= 1e−5 + 1e−4*max(abs(a),abs(b))`. **Одна такая tolerance не доказывает наличие autograd:** нулевой и маленький градиент могут удовлетворить абсолютному порогу. Поэтому одновременно нужны source-проверка graph wiring и сопоставимые actual gradient observations в самих заявленных updates, либо честное GRADIENT_PARITY_NOT_MEASURED. Нельзя объявить detached backward допустимым «zero parameter delta» только потому, что AdamW вызван или counter увеличился.

Это не требование всегда получать ненулевую дельту: при корректном material, intact graph, настоящем optimizer и прошедших integrity checks нулевой эффект по-прежнему допустим. Не добавляются лишние Train, новый synthetic operator fixture или accuracy gate. Тolerance остаётся заранее выбранным численным критерием, а не заменой структурного контракта.

## 4. PAD byte passport независимо подтверждён

Встроенные две JSONL строки прочитаны из fenced block, к каждой добавлен указанный LF, кодировкаUTF8 безBOM. Суммарно **958 bytes**, SHA256 **47f8fbc80dd9bdbb92f31c95dce6273385b5e236dfecfdb7aedcbb81b7a54af7** — совпало с паспортом. Оба JSON объекта синтаксически валидны; поля соответствуют видимому payload.

Это проверка документационных bytes, **не BPE materialization** и не существование runtime dataset. Prompt по147 символов, стороны PAD1 по53/102 и PAD2 по118/53 символа; эти символьные числа не объявляются BPE длинами. Реальную неравную chosen/rejected length ещё должен установить прежний materializer без изменения tokenizer и carrier.

## 5. Новизна и предел результата

Дополнительно выполнен текстовый поиск по corpus possibleDifs/protected зафиксированных snapshots на LM-head/output projection/selected rows/masked logits. Близкие найденные темы — обучать саму head вместо q/v, DPO/SFT mask, BPE vocabulary remap и PAD. Тождественная гипотеза «тот же полный Transformer, LM-head только selected scored rows» в просмотренных совпадениях не найдена. Поэтому **NEW_WITHIN_REVIEWED_CORPUS** обоснованно как ограниченная corpus-новизна. Научный мировой приоритет или причина RP01 accuracy этим не доказаны.

PAD остаётся известным N60, новый ID ему не присваивается. Заявленная полезность HEAD/PAD — пока исследовательская стоимость реального обучения, не измеренное улучшение. Документы верно сохраняют любые фактические полезные показатели и регрессии отдельно; численной победы, model recovery или готовой micro CLI здесь нет.

