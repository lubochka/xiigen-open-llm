# N33-C — DPO teaching vector/weight: нечисловой ingress не отклоняется до optimizer

**16.10.2026 · SOURCE-VERIFIED CONDITIONAL GAP / NATIVE-NOT-RUN / NOT_DIRECT_RP01_CAUSE.** Новый отдельный от [N16 zero-weight](16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md), [N26 AdamW scale](26-NEW-ADAMW-SCALE-CANCELLATION-2026-10-16.md), [N30 nonfinite KNN vectors](30-NEW-VECTOR-ALIAS-AND-NONFINITE-KNN-INGRESS-2026-10-16.md) случай. Источник `llm_mvp_core@32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7`.

## Доказуемый кодовый путь

1. Публичный [DpoVectorBatch](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/11.DPO/DpoTrainer.cs#L3864-L3864) принимает `float[] TeachingVector`, `float Weight=1` **без границ и finite restrictions**.
2. [DpoTrainer.StepVectorCore L840–849](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/11.DPO/DpoTrainer.cs#L840-L849) копирует массив и вес, затем `RequireVectorPairAdmissions`; [L1174–1182](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/11.DPO/DpoTrainer.cs#L1174-L1182) проверяет только `RequirePairAdmission(vectorBatch.Batch)`. `NaN`/`±Infinity` в metadata не проверяются до permit.
3. [L859–896](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/11.DPO/DpoTrainer.cs#L859-L896) применяет `Math.Max(0.01f, Weight)` к нормировке: NaN/±Infinity может дать nonfinite `totalWeight`, `vectorWeight`, `gradMargin` и повлиять на `BackwardLogProbs`; **конкретный commit/fail поведения публикации не исполнялся**. Не утверждать, что persistence гарантированно примет NaN: downstream rollback может отказать.
4. [VectorSignal L1159–1166](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/11.DPO/DpoTrainer.cs#L1159-L1166) считает только `Count(abs(value)>1e-6)`. При `TeachingVector=[NaN, 1]` NaN может быть **молча признан неактивным измерением**, вместо отказа от непригодного обучающего признака. При `Infinity` он будет «активным» наравне с конечным знаком. Это потеря семантической валидности входа, **не обязательно численное отравление весов** в последнем варианте.

**Причинная граница:** [RP01 `LlmIngredientLogicalUnits.cs` передаёт `Weight=1f`](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1736-L1760); source доказал дефект общего интерфейса для будущих curriculum/negative weighting, **но не обнаружил ни одной нечисловой пары в опубликованном C#500/ENRU500**. В профиле RP01 это низкий P2; при входе model-produced sparse/projection vector в новом домене может стать P1.

## MICRO-N33-C · одно маленькое доказательство, без training run

**L0:** взять одну законно допущенную exact-v2 DPO-пару (не менять frozen experiment; можно тестовый fixture), создать два `DpoVectorBatch` с identical `Batch`:
- A: `Weight=1f, TeachingVector=[0,1]` — valid control.
- B: `Weight=float.NaN, TeachingVector=[0,1]` — invalid weight.
- C: `Weight=1f, TeachingVector=[float.NaN,1]` — invalid vector.
- D: `Weight=float.PositiveInfinity, TeachingVector=[0,1]` — invalid weight.
- E: `Weight=1f, TeachingVector=[float.PositiveInfinity,1]` — invalid vector.

Запустить как изолированный native admission probe **с mutation capability = absent**, без реального разрешённого изменения модели. Ожидаемый безопасный результат в исправленном интерфейсе: для B–E `DPO_VECTOR_WEIGHT_NONFINITE`/`DPO_TEACHING_VECTOR_NONFINITE` до mutation permit, optimizer и публикации; A доходит до обычного permit gate. На текущем source эти типы не отбрасывает `RequireVectorPairAdmissions`; проследить реальный thrown error, не угадывать его. Мера — `rejected_before_mutation`, сериализованный штамп и стоп-причина, **НЕ free accuracy**.

**L1, только если возник реально достижимый дефект и есть безопасный тестовый recipient:** при одном и том же approved seed/optimizer положительный control A vs инвалидные B–E, убедиться, что invalid не оставляют никаких Δparams/checkpoint/optimizer moments. Одного fail-before-permit теста достаточно для условного интерфейсного дефекта. Никаких новых 500 пар. Если invalid уже блокируется в caller — документировать тот же контракт там; не вносить вторую, конфликтующую нормализацию. Negative controls `Weight=0` (отдельный контракт N16) и `vector=[-1,1]` (sign currently ignored by N02) должны оставаться **отдельными** вопросами.

**Минимальная предлагаемая инварианта:** до права менять модель `float.IsFinite(Weight) && Weight>=0`; все `TeachingVector` finite, семантическая schema/version проверены. Нужно согласовать, действительно ли Weight0 исключает вклад или минимальный коэффициент является намеренным: это не решается проверкой finite. Не принимать metadata `NaN` как «нулевое знание».

## Независимая DPO quality/roles

Роль человеческого выбора — **CODEX на Astra 6 UltraCode**. Оба `chosen` и `rejected` целевого production DPO пишет **внешняя открытая китайская модель**; отдельно CODEX на Astra 6 UltraCode судит точную пару, отдельно оценивает actual Student outputs. Не выдавать simulation/Astra dev pair за Chinese output. Контроль finite проверяет безопасный вход **после** законного допуска и семантической оценки; не заменяет grader. Этим документом ни одна модель, judge или Student не вызывались.

**Вердикт:** `C_SOURCE_NONFINITE_ADMISSION_HOLE`, `P2_NON_RP01`, `RUNTIME_AND_QUALITY_NOT_RUN`. Требуется только целевой microprobe; по этому условному seam нельзя переписывать исходную причину RP01.
