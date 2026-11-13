# N26 — единая архитектурная развязка RP01: что сломано, что не перенесено, что пока только подозреваем

**Дата: 2026-10-16 · Область:** llm_mvp_core@b0afc08dc0df110b10ed5ed38b518906665c16f8, planning/llm_plan3; авторская сверка источников и предыдущего каталога. **Статус:** DOCUMENTARY READBACK; не новый .NET model run, не CODEX/Astra judge; новых подтверждений real learning gain нет. Эта заметка — карта входов, а не замена находок других агентов, и не разрешает вносить изменения в core/docs/plans/replanning-possible-diffs.

## 1. Главная дифференциальная диагностика

**Три эффекта, которые при планировании были смешаны:**

1. *Знания в изменяемых LoRA весах:* DPO меняет численную policy и может увеличивать выбранный pair margin. Он не равен готовой языковой компетенции.
2. *Знания в обучаемой дискретной памяти/функциях:* E09/Roslyn AST, E13B formula lookup, E26 word/context, E36 correction, KNN/phrase/exact maps, replay, Unit/Flow. Могут улучшать ответ без изменений LoRA и могут обучаться **на своих состояниях**.
3. *Необучаемые правила качества/структуры:* E27 sentence reorder, law gates, verifier, статические проверки. Могут повышать utility, но их эффект нельзя записывать на счёт neural learning.

Реальный **RP01 isolated simulator** оценивает главным образом (1), тогда как старые «хорошие» результаты относились ко всем трём видам. [Сводка текущего RP01](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [архитектурный Master](00-MASTER-SYNTHESIS-AND-CONFLICTS.md), [прямая сверка нынешних вызовов](10-INDEPENDENT-CODE-PATH-RECONCILIATION-2026-10-16.md). C# free0/3→0/3 и ENRU free0/4→0/4 **нельзя ретроспективно переименовать в успех**.

## 2. Куда смотреть за каждым классом возможных различий

| Тракт / владелец | Доказательства, уже собранные другими аудитами | Реальная граница | Быстрый разделяющий шаг |
|---|---|---|---|
| Источник данных и DPO-пары | [02-DPO](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [03-DPO](03-DPO-PAIR-AND-GRADIENT.md), [bridge token loss](16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE-2026-10-16.md) | Некорректность пары, weak teacher, loss в model-visible токенах; старый bridge ≠ RP01 exact-v2 | Хеш bytes→token IDs→response mask двух contrasting pairs; один law verifier |
| DPO loss / backward / sampler | [current/legacy divergence](10-INDEPENDENT-CODE-PATH-RECONCILIATION-2026-10-16.md), [independent decoder audit](23-INDEPENDENT-AUDIT-SHAPE-DECODER-AND-LEARNING-2026-10-16.md) | Градиент может быть верным по формуле и бесполезным по смыслу | 1 tiny directional finite-difference + exact-logprobs |
| LoRA base / cold start / head | [03-LoRA](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [04-Base](04-BASE-LORA-CHECKPOINT.md), [cold-start micro](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md) | B0 random; trainable q/v не даёт гарантии способности переводить | 8–32 new chosen-only warm-up vs DPO-only, independent free output |
| **TeachingVector→optimizer, новая находка N26-B** | [исходник+математика+микро](26-NEW-ADAMW-SCALE-CANCELLATION-2026-10-16.md) | Даже scalar после потери семантических координат может почти исчезнуть в AdamW | [1-step actual optimizer](26-MICRO-EXPERIMENT-NATIVE-ADAMW-AND-LEARNED-E27-2026-10-16.md), затем quality только если значимо |
| Tokenizer / vector / code semantic | [03](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [legacy mask](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md), [BPE drift](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md) | BPE vs semantic vectors; BOS/EOS, OOV, identity и code AST должны иметь стабильные версии | Roundtrip+offsets+source-law pairs, frozen checkpoint tokenizer |
| KNN representation / retrieval | [matrix→KNN recheck](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [KNN gaps](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md), [fusion](20-NEW-GAP-NONLOCAL-FUSION-AND-BM25-SCALING-2026-10-16.md) | E07 reported tiers не меняли key; E09 reported +10.94pp сравнивал разные panels; generic kNN loss of roles/sequence | Один source-disjoint same-panel A/B Roslyn structural→law-aware kNN |
| Learning Unit и переводческие maps | [17-silent-dispatch](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md), [21-structured](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md), [E26 POS](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md) | Наличие результата TrainBatch не равно состоянию выученной карты, Output не равно вызвать consumer | Digest learned state + incoming heldout + actual execute trace |
| **E27 translation structure, новая находка N26-A** | [исходник/семь примеров](26-NEW-E27-STRUCTURAL-REORDER-COVERAGE-LOSS-2026-10-16.md) | Старый E27 SOV/VSO/etc → новый лишь adjacent check/null swap | [paired learn+reorder mini](26-MICRO-EXPERIMENT-NATIVE-ADAMW-AND-LEARNED-E27-2026-10-16.md) |
| Flow / Graph / deterministic gating | [05-unit flow](05-UNITS-FLOW-GRAPH-DETERMINISM.md), [06-units](06-UNITS-FLOWS-GRAPH.md), [ProductHost bypass](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md) | Learned Unit может существовать, но результат не достигает ordinary response; STATIC gates могут дать ложный gain | 3-arm frozen Student / Unit memory / Student+Unit, with route evidence |
| Model restore / checkpoint/adoption | [06-restore](06-CHECKPOINT-RESTORE-ADOPTION.md), [10-translation](10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md) | Checkpoint сохранён ≠ тот же live consumer использовал правильную версию и tokenizer | Save→cold-reopen→SHA state→paired ordinary answer |
| Continual / multi-domain / feedback | [07-replay](07-CONTINUAL-REPLAY.md), [E36](24-NEW-E36-FEEDBACK-KNN-SPLIT-AND-ROUTING-2026-10-16.md) | 0→0 old-domain performance не retention; feedback may be duplicated/filtered | 3-arm same old+new family-disjoint paired retention после появления навыка |
| Evaluation, historical branches | [11-sibling branch successes](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md), [historical caveats](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md), [E35/E36 RNG](23-NEW-E35-E36-COUPLED-RNG-AND-FALSE-KNN-PASS-2026-10-16.md) | Другая модель, val denominator, leakage, dummy rule accuracy, frozen train margin ≠ free utility | Primary replay на одном тестовом наборе, честный denominator и provenance |

**Важное пояснение к «неудачной матрице, затем успешному KNN»:** [12-MATRIX-TO-KNN-EXPERIMENT-MISREADS](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) уже нашёл неправильно привязанные E07 AST features и реально положительную E09 KNN систему, но **не обнаружил direct commit transfer одного и того же алгоритма**. Назвать этот переезд доказанным конкретным рефакторингом пока нельзя. Сохраняем оба разных факта, не исправляя историю ради удобного narrative.

## 3. Приоритеты запуска по критерию learning gain / стоимость

**Первый приоритет — реальная способность Student:** использовать существующий [cold-start mini protocol](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md) на новой семье и одинаковом independent heldout. До больших 500-парных тренировок показать хотя бы tiny learnable overfit + *новый осмысленный ответ*. Это проверяет главную вероятную предпосылку RP01.

**Второй приоритет — parity старой функции и нынешнего consumer:** [микро E27 с обученными word mappings](26-MICRO-EXPERIMENT-NATIVE-ADAMW-AND-LEARNED-E27-2026-10-16.md) и [memory-versus-Student micro](19-MICRO-CONSUMER-01-QUICK-LEARNING-QUALITY-PILOT.md). Их качество относится к **гибридной системе**, не приписывать все улучшения нейронным весам.

**Третий приоритет — teaching-vector/optimizer invariance:** один native Parameter + AdamW.Step без модели, <10 scalar updates. Дальнейший tiny DPO только если есть причина считать этот вектор полезным. Это маленький необязательный technical gate, не замена двух quality проверок.

## 4. Правила, чтобы опять не потратить недели на зелёные Windows runs

Для каждого нового опыта только одна причина различия между A/B. До старта: точный source/model/data/tokenizer SHA, обученная сущность (вес/память/правило), функция обычного consumer, закон/ответ и blind family-disjoint holdout. После: изменённые **параметры/память**, правильные **новые ответы**, route trace и время/ресурсы — четыре независимые строки. Keep old frozen results immutable. Всё неизвестное помечать UNKNOWN, всё проверенное только на proxy — PROXY, а не native result. Не ослаблять security/license/admission checks; отказаться от всего CI не означает отказаться от одного короткого проверяемого source/quality regression.

## 5. Источники требований выше данного каталога

[WorkLeft RAG Master](../../09.09.26WorkLeft-RAG-MASTER-INDEX.md); [WorkLeft teacher roles](../../09.09.26WorkLeft-02-teaching-chinese-model.md); [Original algorithm: DPO creation and trainable Units](../../Алгоритм%20создания%20ДПО.md); [RP01 Source/Architecture](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/SOURCE-AND-ARCHITECTURE.md); [domain/flow requirements](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md). Source→implementation→actual consumer must be reconstructed per domain. One code name or PASS receipt does not establish behavioral equivalence.

**Итог:** два дополнительных source-level seams N26-A/N26-B задокументированы; настоящая причинность плохого RP01 learning ещё не установлена. Уже существующие 100+ соседних файлов покрывают многие другие классы, но их findings нельзя арифметически складывать в один «доказанный root cause». 
