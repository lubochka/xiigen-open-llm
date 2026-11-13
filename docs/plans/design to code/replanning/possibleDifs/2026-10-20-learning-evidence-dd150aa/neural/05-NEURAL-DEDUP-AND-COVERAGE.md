# Neural audit: что новое, что уже покрыто и что нельзя считать исправлением

Срез core: **dd150aa5216e42b8a8bba14955337398ec5f8397**. Planning comparison: **0ea8f0b0094e234ffe7734adfa95e2a9e53203f5**. Метод: чтение исходников и строгая дедупликация по полному материализованному possibleDifs и protected cards. Native training и full tests не запускались. Protected folder не изменялась.

## Новая узкая гипотеза

Только **DPO-SHIFTED-CONTEXT-01** выделяется в самостоятельную новую карточку: полный carrier/targets остаются, input отделяется как T−1, что позволяет проверить обучение на meaningful последнем target65 приContext64. См. [findings](01-DPO-SHIFTED-CONTEXT-01.md), [card](02-DPO-SHIFTED-CONTEXT-01-CARD.md), [prompt](03-SC01-EXECUTOR-PROMPT.md), [fixture](04-SC01-FIXTURE.md).

Состояние **PREPARATION_REQUIRED / эффект не измерен**. Это уточнение next-token execution, выведенное из уже известной PAD границы, а не открытие самого факта shifted loss. Предыдущие агенты правильно указали, что простое crop L−1 теряет последний target; новая карточка специально разделяет input tensor и полный target carrier.

## Отсечённые повторы и устаревшие предположения

| Проверенная тема | Результат чтения и основание | Решение |
|---|---|---|
| «LoRA не участвует в forward» | Текущий LoraLinear возвращает base(x)+scaled B(A(dropout(x))) с TensorOps graph. [LoraLinear112–125](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/10.LoRA/LoraLinear.cs#L112-L125); attention использует adapter path q/v [CausalSelfAttention129–150](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/03.TransformerModel/Layers/CausalSelfAttention.cs#L129-L150). | Старые branch defects нельзя автоматически переносить на current SHA. Не новая гипотеза без runtime counterexample. |
| «DPO обучает prompt вместо response» | В exact route TargetMask берёт response masks; материал строит их по PromptLength. [DpoTrainer1002–1016](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/DpoTrainer.cs#L1002-L1016), [material37–46](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/Models/DpoExactPairMaterial.cs#L37-L46). | Legacy mean/full objective — отдельный известный путь; current exact уже исправлен. |
| Неверный знак log-softmax gradient | Current backward использует oneHot−softmax, а StepVector задаёт противоположные chosen/rejected upstream. [1072–1120](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/DpoTrainer.cs#L1072-L1120), [891–905](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/DpoTrainer.cs#L891-L905). | Не переоткрывать устранённый generic bug как current cause. |
| Неподходящая initial base, q/v vs output head, короткий causal context | Уже подробно описаны PREPARETOGPL-DPO-LORA-TOKENS и последующие audits. В частности DLT-3 разбирает q-LoRA при единственном ключе, DLT-2 — FP32 cancellation. [existing DLT](https://github.com/lubochka/xiigen-planning/blob/0ea8f0b0094e234ffe7734adfa95e2a9e53203f5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/PREPARETOGPL-DPO-LORA-TOKENS-2026-10-18.md). | Известные гипотезы; не новая карточка. |
| DPO computes pad / LM head на лишних строках | Уже E-DPO-VALID-LENGTH/PAD и N-HEAD. [PAD](https://github.com/lubochka/xiigen-planning/blob/0ea8f0b0094e234ffe7734adfa95e2a9e53203f5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/61-LEARNING-UNBLOCK-AND-EXPERIMENT-AUDIT-2026-10-20/03-DPO-PAD-IMPLEMENTATION-BLUEPRINT.md), [HEAD](https://github.com/lubochka/xiigen-planning/blob/0ea8f0b0094e234ffe7734adfa95e2a9e53203f5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/61-LEARNING-UNBLOCK-AND-EXPERIMENT-AUDIT-2026-10-20/02-NEURAL-CURRENT-STATE-AND-NOVEL-HEAD.md). | Новый SC01 не смешивается с per-side PAD и selected-head rows. |
| Tensor copies/gradient allocations/MatMul locality | Double backward copy уже LF-03; AccumulateGrad copy, GRADB locality, RMS/Softmax/RoPE costs присутствуют в предыдущих audits. [LF audit](https://github.com/lubochka/xiigen-planning/blob/0ea8f0b0094e234ffe7734adfa95e2a9e53203f5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-signal-and-cost-audit/01-new-findings-and-small-experiments.md). | Не выдавать повтор resource-factor за новый. Положительные старые resource metrics сохраняются. |
| SFT optimizer/session reset | Known E-SFT-MOMENT-CONTINUITY/P119, не новая находка. Для SC01 недопустима другая ошибка — назвать SFT moment donor доказанной exact-DPO continuation. Gate [DpoTrainer533–560](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/DpoTrainer.cs#L533-L560). | Новый опыт фиксирует одинаковый warm weights + явно новый DPO optimizer. |
| Одна длинная SFT row блокирует весь вызов | Source-confirmed в IncrementalTrainer до LoRA/DPO/SFT: [385–419](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/14.ContinuousLearning/Training/IncrementalTrainer.cs#L385-L419). | Это существующий N63-INGRESS-LENGTH. Текущий результат/очередь сверяет общий аудит; повторять №08 не надо. |
| Checkpoint restore невозможен вообще | Уже сохранён настоящий tiny donor: weights+optimizer save1000 → fresh restore → 200 SFT updates; [Program74–129](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs#L74-L129), [result](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/restore-minimal-set.json#L71-L81). | Отсутствие canonical promotion не равно отсутствию сохранённого обученного состояния. LFS payload access проверяется отдельно, без ложного «мы восстановили». |
| Online train metrics объявлены after | StepVector metrics считаются на forward до ApplyOptimizer; [879–927](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/DpoTrainer.cs#L879-L927). | Известная измерительная ловушка. SC01 требует explicit before/after scorer на соответствующих весах. |

## Покрытие P07: конкретный подслучай, не ещё одно «новое открытие»

Соседний reviewer проверил protected cards: P07.2 рассматривает separate segment boundaries, P07.5 — mismatch raw OwnerInput/TRAIN template; E-DPO-SFT-ORDER и E-SFT-RESPONSE-MASK **фиксируют** различие SFT Prompt+LF+Response и DPO segmented framing, а меняют только stage order либо mask. Источники на том же SHA:

- [P07-materialized-token-contrast.md](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/cards/P07-materialized-token-contrast.md#L145-L166).
- [E-DPO-SFT-ORDER.md](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/cards/E-DPO-SFT-ORDER.md#L55-L65).
- [E-SFT-RESPONSE-MASK.md](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/cards/E-SFT-RESPONSE-MASK.md#L55-L65).

Следовательно, наличие этих карточек не является измеренным A/B именно **cross-stage framing alignment**. Но это подслучай известного P07, а не новая общая гипотеза. Автоматически объявить production дефект тоже нельзя: default IncrementalTrainer DPO route может быть legacy, а exact selector ограничен code-documentation. Сначала нужен конкретный same-model переход SFT→exact-DPO/serve с доказанным carrier, а не сравнение двух несвязанных API. Самостоятельный запуск здесь не назначается.

SC01 специально исключает этот confound: vocab256 без merges, prompt уже включает LF, full decoded sequence matches donor prefix; меняется только input/target extent.

## Граница полноты

Этот пакет проверяет neural execution на current core и новизну относительно доступного корпуса planning. Он не утверждает, что исчерпал все соседние ветки, доказал причину ухудшения RP01 или воспроизвёл прежние успехи. Исторические KNN/branch результаты и полный reconciliation очереди представлены соседними аудитами общего пакета.

Все новые артефакты здесь — документы. Никаких новых measured learning numbers, производственных изменений, удаления старых карточек, скрытых повторных запусков или записи в protected folder не было.

