# N33-D — новая гипотеза: один NaN-grad распространяется на ВСЕ trainable grads при clipping

<!-- AUDIT-2026-11-09-3161:START -->
## Уточнение численной приёмки — 9 ноября 2026

Исторический поиск причины NaN ниже остаётся привязанным к своему срезу; он не доказывает причину позднего P48. В core `31612135c97e15be64e1b2a4a21efd3ffd58e443` порядок pre-step gradients ProbeNN исправлен, а [P48/P49](https://github.com/lubochka/llm_mvp_core/blob/31612135c97e15be64e1b2a4a21efd3ffd58e443/docs/plans/replanning-possible-diffs/experimets/cards/P48-weighting-curriculum.md) имеют отдельные сохранённые результаты. Не переоткрывать принятый backward и не называть состоявшиеся нечисловые updates «обучение не вызывалось».

Для нового сравнительного измерителя конечность каждого участвующего плеча должна менять машинный verdict. Чистый отрицательный контроль: NaN только у одного плеча при сохранении правильных ответов другого — всё сравнение непригодно. Сохранить attempted/completed updates, первое наблюдённое нечисловое состояние и область наблюдения; последнее испорченное состояние само по себе не устанавливает номер первого отказа. Недостающие измерения остаются неизвестными.

Конечный loss всё ещё может не соответствовать применяемой производной: ограниченный `-log(max(p,1e-7))` ProbeNN — конкретный пример, требующий отдельного независимого насыщенного oracle [C04](https://github.com/lubochka/llm_mvp_core/blob/31612135c97e15be64e1b2a4a21efd3ffd58e443/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/105-addressed-repair-plan-2026-10-30.md). Такой остаток не исправляется clipping/NaN-guard и не опровергает правильную производную обычной cross-entropy. Сохраняются положительные finite-контроли, исходная доза и границы условных trainer-проверок ниже; новое обучение ради пересчёта отчёта не назначено.
<!-- AUDIT-2026-11-09-3161:END -->

**16.10.2026 · CODE+FLOAT-SEMANTICS PROVEN / NATIVE_NOT_RUN / REAL_DATA_UNKNOWN.** Исходный `llm_mvp_core@32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7`; core protected possible-diffs **READ ONLY**. Это не прежний [N30 nonfinite KNN input](30-NEW-VECTOR-ALIAS-AND-NONFINITE-KNN-INGRESS-2026-10-16.md) и не [N33-C nonfinite DPO metadata](33-NOVEL-DPO-VECTOR-INGRESS-NONFINITE-ADMISSION-2026-10-16.md). Здесь новый механизм **распространяет одну невалидную производную на другие, прежде корректные, параметры через общий norm**.

## Доказательство

1. [TensorOps.ClipGradNorm L1627–1647](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/00.TensorCore/Core/TensorOps.cs#L1627-L1647) суммирует `totalNormSq += g*g` по всем trainable параметрам, берёт `Sqrt`, затем только `if(totalNorm<=maxNorm) return`; дальше каждый `p.Grad.Data[i] *= maxNorm/(totalNorm+1e-6)`.
2. **Контрпример прямо из float-алгебры:** два параметра, `grad0=[float.NaN]`, `grad1=[1f]`, `maxNorm=1`. Тогда `totalNormSq=NaN`, `totalNorm=NaN`; условие `NaN<=1` ложно; `scale=NaN`; **обе** производные становятся `NaN`, хотя второй параметр был чистым. Для `grad0=+Infinity` масштаб `1/(+Infinity)=0`; `Infinity*0=NaN`, прочие конечные grad обнуляются. Здесь нет softmax, dataset, tokenizer или model-dependent допущений.
3. [Trainer.TrainSupervisedNextTokenStep L125–145](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/04.TrainingLoop/Trainer.cs#L125-L145) вызывает `ClipGradNorm`, затем `optimizer.Step`. Проверка конечности *весов* случается **после** optimizer; при отказе она чистит gradients, но в показанном method scope **не восстанавливает веса и optimizer moments**. Значит нельзя считать ошибку полностью атомарной. Фактическое обнаружение ошибки зависит от того, появился ли NaN gradient.
4. [Trainer.RunPretrainingCoreAsync L223–254](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/04.TrainingLoop/Trainer.cs#L223-L254) проверяет конечность scalar **loss до backward**, затем clip→optimizer→`CompletedSteps++` без показанной post-step finite parameter validation; [L262–283](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/04.TrainingLoop/Trainer.cs#L262-L283) запускает checkpoint saver и catch/Faulted только при выброшенном исключении. Поэтому **условно** неконечные параметры могут попасть в checkpoint даже если исходный loss был finite. Не утверждать, что опубликованный checkpoint действительно содержит NaN; ни dataset, ни run не исполнены.
5. [AdamW.Step L74–107](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/04.TrainingLoop/Optimizers/AdamW.cs#L74-L107) суммирует градиенты в `m/v` и не проверяет finite, а [CaptureState/RestoreState L113–175](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/04.TrainingLoop/Optimizers/AdamW.cs#L113-L175) не выполняет automatic rollback; блок возврата state отдельный и должен быть явно вызван.

## Область и отношение к RP01

- **P0 safety для любого нового native SFT/pretraining** на случайной B0, включая восстановление нужной обучаемости; математическая уязвимость общая для C#/ENRU/SQL и других нейронных доменов.
- **P2 гипотеза о фактическом RP01 C#500/ENRU500 0/3 и 0/4:** не найдено доказательства NaN в train gradients; защищённый `DpoTrainer` имеет иные обёртки step/rollback. Не обвинять DPO-only pipeline на основании кода другого trainer.
- Не путать `float.IsFinite(loss)` с `all gradients are finite`, а `Faulted` с `rollback succeeded`.

## MICRO-N33-D: ≤ один нативный тест ядра + один условный тренер-тест

```csharp
using TensorCore.Core;
using TensorCore.Layers;
var a = new Parameter(new Tensor(new float[] { 10f }, new TensorShape(1)));
var b = new Parameter(new Tensor(new float[] { 10f }, new TensorShape(1)));
a.Value.Grad = new Tensor(new float[] { float.NaN }, new TensorShape(1));
b.Value.Grad = new Tensor(new float[] { 1f }, new TensorShape(1));
TensorOps.ClipGradNorm(new [] { a, b }, maxNorm: 1f);
Console.WriteLine($"NaN propagated? {float.IsNaN(b.Grad!.Data[0])}");
```

**A=SOURCE:** `b.Grad` становится `NaN`. **B=ONE_DIFF_GUARD:** ограниченная функция, проверяющая `maxNorm>0 && IsFinite(maxNorm)` и все gradient coordinates BEFORE вычисления norm и BEFORE optimizer; invalid throws domain-specific `NONFINITE_GRADIENT_BEFORE_CLIP` без изменения других gradient buffers. Primary metric `b.Grad remained 1f`, no optimizer step, no model/optimizer delta. Controls: (C) grads `[3,4]` при maxNorm5 — корректная норма, unchanged; (D) finite `[3,4]` maxNorm1 — scale 1/5 within FP tolerance; (E) +Infinity — rejection like NaN. **Короткий native .NET source test, не новая CI матрица.**

**Conditional Step T1:** до любой реальной SFT серии сохранить base/AdamW portable state; в disposable tiny model вызвать один controlled invalid gradient through pre-optimizer seam, сравнить A=current vs B=guard+rollback (если нужен) по `SHA(params), SHA(optimizer), CompletedSteps, checkpointFiles` и error category. Проверка должна гарантировать отказ ДО mutation; не создавать poisoned artifact на рабочем пути. Если нет безопасного hook/injection, достаточно T0 и source-review transaction; не изобретать permission bypass. Только после этого измерить независимую 8–16 case SFT способность [MICRO-N33-A](33-MICRO-AUTOGRAD-REACHABILITY-AND-SFT-EXPERIMENT-2026-10-16.md).

**Критерий и границы доказательства:** если T0 не воспроизводится на actual .NET, N33-D `DISCONFIRMED` по runtime snapshot. Если T0 подтверждён, есть **алгоритмическая уязвимость**, но её вклад в ухудшение качества остаётся `UNKNOWN` до следа nonfinite train input и paired heldout. Не восстанавливать предыдущие checkpoint под новым названием «лучше обученная модель» без нативного compare.

## Роли/доказательства

Для реального DPO CODEX **Astra 6 UltraCode** выбирает урок, **внешняя открытая китайская модель** формирует обе стороны chosen/rejected, отдельный CODEX **Astra 6 UltraCode** оценивает exact pair и independently — реальный ответ Student. Настоящая source/float проверка *не* потребляет такую пару и не представляет собой исполнение этих ролей. Native test, actual DPO and Student training: `NOT_RUN`.

**Verdict:** `N33-D CODE_CONFIRMED_NONFINITE_CLIP_CASCADE`; `SFT_STATE_RISK_CONDITIONAL`; `QUALITY_IMPACT_UNKNOWN`. Один короткий falsifier перед следующими 500 парами.
