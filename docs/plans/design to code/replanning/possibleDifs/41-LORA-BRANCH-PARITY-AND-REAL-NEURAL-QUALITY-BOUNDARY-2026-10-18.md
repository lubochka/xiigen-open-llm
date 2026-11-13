# N41-N1 — LoRA в PrepareToGPL и текущем Student не эквивалентны: не искать успех переводческой памяти в старом адаптере

18.10.2026. **SOURCE-COMPARISON / HYPOTHESIS / NO NATIVE RUN**. Цель: отделить реальную смену LoRA/градиентного алгоритма от архитектуры исторической learned memory. Не объявлять доказанным ни дефект current q/v, ни превосходство старого LoRA.

## 1. Verified source differences

| Проверка | PrepareToGPL | current codex/dpo-implementation |
| --- | --- | --- |
| Владелец адаптера | [LoraLinear.cs L15–60](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/10.LoRA/LoraLinear.cs#L15-L60) — Module над Linear, RegisterModule base/dropout и RegisterParameter A/B, base frozen | [LoraLinear.cs L13–56](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/10.LoRA/LoraLinear.cs#L13-L56) — Module + IAttentionProjectionAdapter, адаптерный slot и собственные параметры |
| Путь подключения | [LoraApplicator.cs L12–62](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/10.LoRA/LoraApplicator.cs#L12-L62) — Apply перечисляет reflection Linear и возвращает список новых LoraLinear, комментарий прямо утверждает: **base model forward unchanged; adapters applied externally** | [LoraApplicator.cs L401–446](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/10.LoRA/LoraApplicator.cs#L401-L446), [L889–912](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/10.LoRA/LoraApplicator.cs#L889-L912) — model mutation lease, PopulateAdapterPair for typed projection; отдельные seeded policy/reference/fresh modes |
| Dropout train/eval | [old L45–61](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/10.LoRA/LoraLinear.cs#L45-L61) — Forward вызывает dropout(training: true) без переключателя | [current L106–123](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/10.LoRA/LoraLinear.cs#L106-L123) — Forward(x) выбирает training:false, явная версия Forward(x, training) |
| DPO objective | [old DpoTrainer.cs L36–74](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/11.DPO/DpoTrainer.cs#L36-L74) — ручная DPO оптимизация relative chosen/rejected и AdamW | [current DpoTrainer.cs L650–710](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L650-L710) — final policy/reference BatchLogProbs, objective admission, backward, optimizer/publishing guards |

**Следствие по коду:** старый LoRA Applicator один сам по себе НЕ гарантирует, что прямой LlmModel.Forward вообще вызовет адаптер. Текущий проект явно адресует model projection slots. Но по одному исходнику Applicator нельзя утверждать, что не существовал иной legacy caller, вручную применявший список. Для сравнения нужена **фактическая трасса** модели, а не предположение.

Исторический переводческий ExecutorUnit был **побайтно одинаковым** между ветками и обучается выбранным source→target в exact/phrase/KNN памяти без neural update. Положительный переводческий опыт [P125](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md) менял только память, **neural instances=0**. Поэтому гипотеза «откатить LoRA к PrepareToGPL и получить этот исторический переводческий gain» причинно не обоснована.

## 2. Быстрый двухуровневый LoRA parity pilot

**Домен:** сначала синтетическая next-token preference, затем профиль EN→RU (только после успешного unit sanity); отдельно C# method documentation. **Получатель:** одна реальная neural policy с LoRA, frozen reference; ни слова о переводческой памяти здесь.

**LEVEL 0 / источник истины:** 2×2 matrix old vs current implementation и attached vs detached adapter, один небольшой compatible forward без обучения. B=0 init должен давать одинаковые logits base+adapter; после заданного ненулевого B или одной tiny legal optimizer step сравнить logits у **реально вызываемого LlmModel.Forward**, lora_a/lora_b checksum, target slots, forward invocation. При old external-only caller вызов неизменной базовой модели — ожидаемый риск, а не заранее измеренный исход. Для dropout >0 отдельно train/eval seeded repeated forward; при dropout=0 отрицательный контроль идентичного вывода. Нельзя мешать разные RNG и vocab.

**LEVEL 1 / навык:** только если текущий DPO owner принимает lawful pair и доступны совместимые checkpoint/tokenizer, фиксировать маленький TRAIN (8–16 genuine meaningful chosen/rejected), tiny known ability, exact same policy base, response masks, LR, rank, alpha, update count, reference; (A) DPO-only random B0, (B) chosen-only warm SFT then DPO при одинаковой суммарной compute budget; (C) pretrained-compatible base если имеется подтверждённый provenance, тогда сравнение отдельно. Регистрировать pair train margin/CE, per-token grad norm, changed logits, free answer на двух разных hidden semantic task families. При неработающем tiny train overfit 500 DPO не запускать.

**Measure:** delta chosen log odds, masked gradient numerical-vs-analytic, actual answer correct/all, critical semantic violations, wall time/update, tokens/sec, resident RAM/VRAM, LoRA adapter tensors and full/trainable neural layers, executed Unit/Flow graph if present. Не использовать «positive margin» как единственную метрику. Label-shuffle и exact-B0 forward — negative controls.

## 3. Связь с уже известным и novelty limit

[RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) уже предлагает cold-start vs SFT и gradient checks; **не выдавать их за впервые найденные гипотезы**. Здесь новое конкретное cross-branch source отличие — **external-only vs typed model-attached adapters** и legacy forced training dropout. Проводить такой опыт только для neural owner и реального task; для E09 AST kNN, E26 contextual dictionary, E43 replay и P125 memory training LoRA не является получателем улучшения.

Все операции продуктового кода и native experiments пока НЕ выполнялись; protected core/docs/plans/replanning-possible-diffs не изменять.
