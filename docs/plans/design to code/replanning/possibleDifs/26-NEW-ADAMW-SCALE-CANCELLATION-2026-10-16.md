# N26-B — Пропущенное следствие TeachingVector: AdamW гасит глобальное масштабирование градиента

**Дата:** 2026-10-16. **Источник:** llm_mvp_core@b0afc08dc0df110b10ed5ed38b518906665c16f8; **статус:** CODE+MATHEMATICAL PROXY, native .NET и quality-training НЕ выполнены. Новое по отношению к DIF-02 (вектор сведён к количеству активных координат) и N16-02 (нулевой вес становится 0.01): этот документ показывает, что **даже оставшийся scalar может не дать ожидаемой разницы в шагах LoRA**.

## Суть и доказательства

В [DpoTrainer.StepVectorCore L861–904](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/11.DPO/DpoTrainer.cs#L861-L904) gradMargin умножен на scaledWeight = normalizedPairWeight × VectorSignal(teachingVector). [VectorSignal L1159–1166](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/11.DPO/DpoTrainer.cs#L1159-L1166) считает ненулевые элементы и выдаёт коэффициент [1,2]. [ApplyOptimizer L1218–1225](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/11.DPO/DpoTrainer.cs#L1218-L1225) передаёт LoRA parameters в собственный AdamW.

[AdamW.Step L72–106](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/04.TrainingLoop/Optimizers/AdamW.cs#L72-L106) реализует m, v, bias correction и update: w -= lr*mHat/(sqrt(vHat)+eps), отдельно decoupled weight decay.

При первом optimizer step и одном примере, если g заменить на 2g, получится:

~~~text
Δw(g)  = -lr*g/(|g|+eps)
Δw(2g) = -lr*2g/(2|g|+eps)
~~~

То есть для |g| намного больше eps обе поправки почти равны — адаптивная нормализация практически отменяет задуманный удвоенный вес. Это **не баг AdamW**, а несоответствие выбранного способа кодировать силу обучения назначению вектора. При различных коэффициентах у *нескольких* примеров в одном батче направление суммы может измениться: там отмена не гарантируется. Она также не полная при почти нулевых градиентах, изменяющемся масштабе между шагами и ненулевой истории оптимизатора.

### Проверенный численный proxy без модели

Для lr=1e-5, eps=1e-8, w0=0, zero moments: g=0.01 → Δw=-0.0000099999900; 2g=0.02 → Δw=-0.0000099999950. Разница модуля всего около 0.00005%, а не 100%. При g=1e-8 уже ~33%: eps ограничивает инвариантность. Цифры пересчитаны локально по формуле AdamW, **не через .NET и не на LoRA**. В дополнение [DpoTrainer metrics L909–935](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/11.DPO/DpoTrainer.cs#L909-L935): Loss scaledWeight-aware, но RewardMargin/Accuracy weighted только normalizedPairWeight — это отдельный риск неверной интерпретации.

## Небольшой эксперимент MICRO-N26-B (не 500 DPO)

**L0, 0 обучения.** Создать отдельный одноразовый console test c РЕАЛЬНЫМ AdamW.Step и одним Parameter, w=0, тремя fresh optimizer instances и фиксированными beta/eps/lr: A:g=0.01; B:g=0.02; C:g=-0.01. Одно обновление каждому, записать ratio |ΔB|/|ΔA|, mHat, vHat, float rounding. Primary: ratio, ожидается близкое к 1, C должен сменить знак. Если иначе — исследовать настоящий optimizer path, а не менять ожидания.

**L1, только если L0 подтвердился.** Две копии одного frozen tiny Student+reference; одна разрешённая НОВАЯ DPO пара, одинаковые exact-v2 token IDs/response masks/seed; A TeachingVector activeSlots=0, B activeSlots=60, один StepVector на каждой fresh копии, никаких повторных применений frozen RP01-pairs. Контроль C: две разнонаправленные пары с разными activeSlots (проверяет относительное, а не глобальное взвешивание). Записать vectorSignal, ||gradient||, ||ΔLoRA||, cosine обновлений, chosen/rejected likelihood и одинаковые free outputs.

**Диагностическая primary metric:** отношение ||ΔLoRA_B||/||ΔLoRA_A||. **Отдельная quality metric:** доля верных предметных ответов на новом source-disjoint holdout до/после. Если качество неразличимо, вывод SCALE-CANCELLATION может быть подтверждён, а LEARNING-GAIN остаётся NOT_PROVED. Не переписывать существующий frozen научный verdict и не запускать Windows CI, если достаточно одного локального native теста.

### Почему важно для RP01, но не доказано причиной RP01

Сначала измерить реальные activeSlots каждого RP01 DPO vector batch и факт вызова этого StepVector path. У RP01 случайная B0, domain mismatch и недообучение могут целиком объяснять нулевые free outputs. Точное объяснение нельзя получать из одного optimizer ratio. Не менять learning rate, rank или веса вектора до наблюдения consumer quality.

**Отдельный смысл:** DIF-02 показывает утрату семантических координат TeachingVector; N26-B доказывает **математическую почти-инвариантность AdamW к оставшемуся общему positive scalar**. Исследовать направление градиента и relative per-example priority вместо обещания «сильнее учим» только за счёт абсолютного масштаба.

Связано с [DIF-02](01-PRIORITIZED-DIFFERENCES.md), [N16-02](16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md), [RP01 code audit](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md).

**Вердикт:** подтверждено как source-level и математическое расхождение; настоящий прирост/регрессия обучения не измерен; независимое review PENDING.
