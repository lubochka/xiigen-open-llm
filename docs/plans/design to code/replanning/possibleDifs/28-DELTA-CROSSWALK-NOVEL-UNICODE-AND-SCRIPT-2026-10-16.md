# RP01 possibleDifs: доказанная дельта N28, родословная и куда смотреть все оси

**16.10.2026 · incremental source audit.** Branch `xiigen-planning/llm_plan3`, core `d5c2409b24292d361593b8262b0c54997b15f539`. Защищённый `llm_mvp_core/docs/plans/replanning-possible-diffs` только READ, без изменений. Модели/расширенный C# тренинг, оценку Astra не запускали.

RP01 scientific Student получил DPO/LoRA update, но C#500 free `0/3→0/3`, ENRU500 `0/4→0/4` — [сохранённый отчёт](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md). Это **не** отменяет успешность других исторических retrieval/translation Units с другими recipients.

## Исторические результаты, с границами

- [E14 contextual KNN 30.04](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-04-30-001.json): top1 0.75→1.00, **4 val cases**; узкий позитив, не neural text gain.
- [E26 context dictionary 01.05](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-001.json): 0.40→1.00 на 15 cases; [N23 audit](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md) отметил заранее встроенный словарь, не переносить как доказательство обученности.
- [E43 replay 01.05](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json): retention 0.3226→0.3978, **исторический** recipient.
- [E09 structural kNN 01.05](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json): reported 0.2413→0.3507, **но сравнение baseline/knn с неодинаковыми val denominators**, см. [00 audit](00-MASTER-SYNTHESIS-AND-CONFLICTS.md); нельзя считать causal gain.

## Карта семейств причин (существующие owners, без переписывания)

| Ось | Проверенные/исторические досье | Новая дельта |
|---|---|---|
| Random B0, SFT precondition, DPO-mask/gradient, LoRA q/v/rank, BPE tokens | [DPO/gradient](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [LoRA](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [RP01 diagnostic](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md) | **не исследовались новым N28**; ничего не объявлять исправленным |
| AST, порядок, E09/E26, matrix→KNN и другие ветки | [matrix-KNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [branch archaeology](11-BRANCH-ARCHAEOLOGY-AND-LEARNING-PROOFS.md), [full historical crosswalk](27-RP01-FULL-CAUSAL-CROSSWALK-POSITIVE-HISTORY-2026-10-16.md) | не считать ранний FAILED приговором новой KNN функции |
| KNN feature space, eligibility, top1, history, HNSW, replay | [14 ID/predicates](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md), [20 non-local rerank](20-LESSON-ADDITION-RERANKER-NONLOCAL-QUALITY-GAPS-2026-10-16.md), [27 replay](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md) | [N28-U](28-NEW-UNICODE-CANONICAL-KEY-AND-SPANISH-N-FALSE-MOJIBAKE-2026-10-16.md): Unicode NFC/NFD semantic alias, false Ñ mojibake (новое) |
| DPO→Units, structure, flow, verifier, hardcoded arbitration | [21 Unit](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md), [28 feature-key drift](28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT-2026-10-16.md), [23 verifier](23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE-2026-10-16.md) | [N28-S](28-NEW-ENRU-MIXED-SCRIPT-PROTECTED-IDENTIFIER-REJECTION-2026-10-16.md): KNN strict target ≠ PhraseMap target (новое) |
| Cold restore, ordinary consumer, quality/metrics | [06 checkpoint](06-CHECKPOINT-RESTORE-ADOPTION.md), [18 consumer bypass](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md), [28 metrics](28-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-DIVERGENCE-2026-10-16.md) | каждую новую гипотезу мерить **на actual ordinary output после cold reopen**, не по весам |

**Дедупликация:** операторная потеря уже найдена; русский `RU/ru` уже найден; direct upstream corrected tokenizer vs Executor key mismatch уже найден другим N28. Уникальный инкремент этой сессии **три конкретных кодовых шва**: `NFD sí→si`, `uppercase Ñ→mojibake`, `KNN WordMap vs PhraseMap script filters`. Это новизна среди просмотренных документов, **не** обещание абсолютной уникальности на всём диске. `DesignToCode` сохраняет те же кодовые условия — проблемы унаследованы, не proven regressions in RP01.

## Что можно выполнить быстро

[Три однофакторных микропробы](28-MICRO-UNICODE-AND-MIXED-SCRIPT-LEARNING-EXPERIMENTS-2026-10-16.md); [исполненный source-only proxy](28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY.mjs) и [результат](28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY-RESULT.json). **SOURCE_ONLY_PASS ≠ LEARNING_GAIN**. Настоящий исполнитель подтверждает source-family disjoint correctness, false-accepts, retention, cold restore и отдельную оценку CODEX на Astra 6 UltraCode.

**Непереопределяемые роли:** CODEX / Astra 6 UltraCode выполняет human choice; разрешённая внешняя открытая китайская модель создаёт обе DPO стороны chosen/rejected; CODEX / Astra 6 UltraCode независимо оценивает пары и ответ Student. Здесь роли НЕ исполнялись.
