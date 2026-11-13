# 03 — LoRA, исходная база, BPE и разрыв векторных пространств

<!-- POSTQ011-2026-11-04:START -->
## POSTQ011 — Обученный tokenizer и действительный получатель вектора

Q008 подтверждает learned BPE merge(97,98)→256 на ASCII: joint(a,b) даёт 0 pure-response targets, segmented —1. Это граница materialized tokens/mask, а не ответное умение модели или языковая аттестация RU/HE. Существующую Validate сохранить; single scored-position mutation должна отказать пересчётом actualInput/Target/Mask. Владельцы 82/83 не получают нового PRIMARY.

Q007 vector effect ограничен числом занятых slots: sign/index смысла не поступают в scalarVectorSignal. Одинаковые states A/B при равной плотности не являются доказательством переноса семантики в LoRA. В project DPO равные checkpointbytes не равны одной mutable recipient identity; проверять реальный issuer/recipient/reissuedcapability по R128-02, сохраняя законный sharing. NaN moments и lost required state ловятся продуктовым admission, а не только reader guard. Fresh/sparse состояния остаются допустимыми в своём договоре.

Основание: [единый разбор](codex-2026-11-04-POSTQ011-TRANSFER-AND-EXPERIMENT-AUDIT.md), [core158](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-158-postq011-transfer-and-experiment-audit-2026-11-04.md) и [план 159](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-159-postq011-addressed-repair-plan-2026-11-04.md). В передаваемое поручение включить собственные положительный/отрицательный случаи, actual consumer и предел доказательства. Исходные результаты, данные, бюджет и очередь сохранены; текущая правка документная.
<!-- POSTQ011-2026-11-04:END -->


<!-- POSTQ003-2026-11-03:START -->
## Уточнение приёмки 3 ноября 2026 — COORDINATE-AND-PAYLOAD-IDENTITY

Идентичность checkpoint включает фактический payload и coordinate/tokenizer schema, не только имя LoRA-файла. Shared P11/P12 S1.weights не соответствуют manifest; восстановление точного original принадлежит29-D3/29-W1, исторические code fixes сохраняются. Положительный контроль: свой полный совместимый bundle. Отрицательные: чужое содержимое того же размера или пересоздание manifest под повреждённый state. Optimizer не признавать повреждённым без отдельного доказательства.

Основание и точные владельцы — [аудит после Q003](codex-2026-11-03-POSTQ003-ARTIFACT-AND-TRANSFER-AUDIT.md). В передаваемое поручение включить назначенные state/attempt/path/authority, положительный и отрицательный случаи и ожидаемую причину отказа. Исходный ниже паспорт сохраняется как история; уточнение не запускает новый опыт, не меняет gold/budget и не отменяет доказанные прежние результаты. Нативная реализация и приёмка требуют назначенного исполнения.
<!-- POSTQ003-2026-11-03:END -->

**Метод:** source→actual tensor/object→consumer; при отсутствии cold semantic comparison указано HYPOTHESIS, не FAILURE.

## DIF-301 (P0). RP01 обучал предпочтения на случайной B0, а не на предобученной языковой основе
[Манифест](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/config/code-documentation/generation-v1.json#L7-L48) задаёт 629 vocab, 2048 context, 4 слоя, hidden512, 8 heads, rank8 alpha16 dropout0, только q_proj/v_proj. [RP01 current review](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/RP01-LEARNING-MECHANISM-REVIEW-2026-10-16.md#L15-L29) прямо описывает B0 как randomly initialized untrained. Это **наиболее вероятное архитектурное объяснение**: DPO учит относительным предпочтениям с имеющимся распределением; без пригодной генеративной основы и языкового словаря такая маломощная адаптация может менять pair log-likelihood, не производя полезный текст. Но отсутствие прироста на короткой панели не доказывает невозможность всех tiny-model recipes.
**Проверка:** отдельно (а) capability baseline до DPO на scripted C#/EN-RU и small general language, (б) frozen valid pretrained tiny base, если права и PC budget допускают, (в) same LoRA/data/teacher on both, (г) **не менять B0 в середине уже выданного эксперимента**.

## DIF-302 (P1). Target-modules не эквивалентны достаточной выразительности
[LoRA definition и forward](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/10.LoRA/LoraLinear.cs#L15-L60) и [Forward code](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/10.LoRA/LoraLinear.cs#L107-L125) реализуют ожидаемую формулу: output = Base(x) + alpha/r × (dropout(x)Aᵀ)Bᵀ; B изначально нулевая; A инициализируется Kaiming. По внешнему чтению **нельзя утверждать, что формула LoRA сломана**. Ограничение: при q_proj/v_proj only, rank8, обучаемые параметры воздействуют на attention, но не напрямую на token embedding, output lm-head, MLP/FFN.
**Непроверенные причины:** слабое влияние на первые токены, неподходящее место вставки, отсутствующий target layer в фактическом generation forward, edge-effects TensorCore backward через registered adapters, init RNG/grad norm, premature stopping при temp0, capacity vs domain complexity.
**Точный probe:** перехват только названных 16 tensors, Δpolicy/chosen vs rejected, один forward identical input before/after и knockout adapter. Сравнение rank/target-space как следующий (отдельный) controlled experiment; **не** менять одновременно base, vocab и LoRA rank.

## DIF-303 (P0). LoRA и semantic/vector tokenization — два независимых преобразования
[RP18 architecture source](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP18-Semantic-Transposition-and-LoRA-Vector-Alignment-Slice-Pack/ARCHITECTURE-AND-CONTRACTS.md#L12-L39) прямо различает:
- generator-LoRA меняет network activations/logits;
- tokenizer определяет байтовые/целочисленные IDs;
- semantic feature encoder несёт область действия закона, отрицание, роли и relation;
- codebook/quantizer, если нужен, имеет отдельную версию и декодер;
- HNSW/kNN index хранит конкретное пространство, которое надо пересчитать при смене encoder;
- typed Unit/Flow consumer не обязан читать ни LoRA state, ни её скрытое представление.
**CODE + CONTRACT_DIFF:** текущий [DpoTrainer.VectorSignal](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L1159-L1166) не обучает и не материализует semantic output vector; [KnnLogicalUnit](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L265-L300) строит свой hashed histogram из отдельно trained BPE. Простого «DPO checkpoint → LoRA → vector token → unit» на основании этих двух методов **нет**.
**Дальнейшая трассировка:** найти точный runtime producer/decoder/consumer per domain и связать vector space ID, tokenizer state SHA, adapter hash, domain lawVersion, cardinality, unit schema, Index ID. Без доказательств отсутствия по ВСЕМ другим классам нельзя объявлять глобальную transposition implementation missing.

## DIF-304 (P1). BPE емкость и RU/HE coverage существенно отличаются от старого требования
[Tokenizer core](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/01.Tokenizer/BpeTokenizer.cs#L38-L82) обучается на byte sequences и merge tables; [RP01 config](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/config/code-documentation/generation-v1.json#L7-L10) задаёт 629 tokens. [Переводческий guide 11.05](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/language%20translate%20learning/TRANSLATION-TOKENIZER-GUIDE-v1.md#L26-L88) ранее требовал coverage EN≥97%, RU≥95%, HE≥90%, допускает словарь 64k–100k (как **вариант масштаба**, не гарантию успеха).
**Гипотеза:** RU морфология разбита на многочисленные bytes и long target sequences, что увеличивает difficulty and context pressure. Но маленький словарь допустим для tiny research, если измеренный token coverage и task utility достаточны; числа 64k нельзя навязать prototype без PC cost.
**Быстрая проверка:** токены/Unicode char для каждого domain, fraction single-byte fallback, examples cut at context2048, fraction response targets, p50/p95, split of RU/EN; avoid changing tokenizer after checkpoint without remap/reinit.

## DIF-305 (P1). Возможная неоднозначность EOS в BPE
[Tokenizer.EosToken](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/01.Tokenizer/BpeTokenizer.cs#L36-L40) вычисляется как *vocab.Count−1*. [Train](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/01.Tokenizer/BpeTokenizer.cs#L42-L68) создаёт 256 байтов и добавляет merge tokens последовательными IDs. Из этих строк не видно отдельного reserved EOS ID, и last learned merge **может быть обычной подстрокой**. Если tokenizer artifact не резервирует/переназначает этот ID, во время Generate токен обычного содержимого может интерпретироваться как EOS.
**Статус:** CONDITIONAL_HYPOTHESIS, не воспроизведённая ошибка; проверить saved tokenizer artifact/schema, actual Generate stopping, end-token storage, roundtrip of EOS, декодирование обычной подстроки с последним merge ID. **Не** менять id в existing checkpoint без миграции.

## DIF-306 (P1). Tokenizer и vectorization KNN обучаются на разных алфавитах
[KnnLogicalUnit.Train](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L136-L153) тренирует локальную BPE vocabSize320 на входах конкретного unit. Student имеет vocab629. **Это не один embedding space.** ID 259 или cosine v32 одной модели не сопоставим с ID 259 другой, если merge-table не идентична; даже при общей длине вектора координаты отличаются. Возможны несовместимости train/readback при обновлении tokenizer state KNN.
**Проверка:** serialize tokenizer SHA, compare token mapping and vectorization IDs of same strings across train/inference and cold reload; запрещено переиспользовать индекс с другим encoder/version. Не утверждать, что такие пространства уже смешиваются: конкретный illegal call-site пока OPEN.

## DIF-307 (P1). Обучение ANN без извлечения смысла
[KNN vectorizer](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L265-L299) хранит частотные BPE mod buckets и хеш предикатов mod buckets. Нет собственной trainable embedding projection; нормировка L2 не создаёт семантики. Это не значит, что KNN вообще не учится — он **запоминает примеры и распределение labels**; но другой класс способности по сравнению с neural representation learning.
**Проверка:** semantic-equivalent paraphrases vs lexical close but opposite action; annotated law-differences with identical candidate pool; measure collisions and downstream acceptance.

## DIF-308 (P1). LoRA check/reconstruction не завершены одной проверкой веса
[RP01 recovery](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/RP01-LEARNING-MECHANISM-REVIEW-2026-10-16.md#L31-L50) описывает verified saved source C50, policy/reference imports and global optimizer/RNG continuation; это positive evidence. Отдельная неизвестная величина: полное equality functional outputs cold load vs immediate inference across tokenizer/adapter/settings/decoder/retained context. Для каждого loaded model требовать comparison одинакового pair score и output, plus identity, without recomputing training.

## Сценарии minimal contrast
(A) Случайная B0 vs технически допустимая pretrained B0 только после исходного DPO-result review; (B) fixed model, compare raw-BPE/hybrid semantic encoder for KNN; (C) fixed data/model/decoder, LoRA on/off cold generation; (D) same vector dimensions but different feature bases — forcibly reject as incompatible until normalized agreement proven; (E) RU/HE punctuation, negation and action tokens.
