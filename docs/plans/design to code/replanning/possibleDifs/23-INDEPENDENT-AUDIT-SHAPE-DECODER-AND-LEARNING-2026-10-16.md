# 23 — Дополнительный независимый RP01-аудит: векторная форма, декодер, граница обучения

Дата 16.10.2026. SOURCE-VERIFIED local behaviour / source-equivalent analytic proxy / RP01 causal impact NOT VERIFIED / independent CODEX–Astra 6 UltraCode review NOT RUN.

**Pinned core:** [0eccbdc3c4efb90adf1d699806cbfa9f5610998d](https://github.com/lubochka/llm_mvp_core/tree/0eccbdc3c4efb90adf1d699806cbfa9f5610998d). **Planning inspected:** llm_plan3@98d45907b4bcbc1ee889019029074a6e9b984fe0; другие агенты одновременно добавляют исследования. Инвентаризированы 97 первых файлов possibleDifs и 11 core studies, детально прочитаны главные тематические отчёты и исходный код; не заявляется, что построчно прочитан весь RAG-архив.

## Вывод для человека

Исторически работала не только LoRA: предметный KNN, накопленная обучением память правил/слов/контекстов, Flow и маршрутизация; результаты из ветки language_translate и текущих experiments-v2 являются настоящими *функциональными* результатами на ограниченных панелях. Часть ранних негативных экспериментов была впоследствии реализована как успешные KNN-функции. В частности, [E40V2 kNN versus production constant TrustScorer](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/10-historical-function-transfer-and-learning.md) документирует 79 TRAIN / 21 VALID, baseline .6667, enhanced 1.0, а последующий production scorer изменил обучаемые labels+vectors на формулу с фиксированными коэффициентами. Реальная линия «неудачная матрица → KNN» от владельца проекта остаётся заданием на точное commit-подтверждение; не объявляем E40V2 именно тем донором.

[11 E51A/E51B source audit](11-E51-KNN-ORACLE-SPLIT-AND-POSITIVE-MECHANISMS-2026-10-16.md) установил, что 13/13 validation E51A повторяли CodeContext из TRAIN; метрика predicate overlap 0.205→1 не доказывает обобщение на неизвестный код. E51B использовал сохранённые oracle значения. Ни одна из этих оговорок не отменяет существование полезных исторических алгоритмов; она требует source-family-disjoint проверки реального потребителя.

В RP01 random B0 (629 BPE tokens, 4 layers) оптимизировался через response-exact DPO и q/v LoRA, но это не гарантирует приобретения языка/грамматики. В [DpoTrainer.StepVectorCore](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/11.DPO/DpoTrainer.cs#L832-L943) действительно выполняются policy/reference forwards, backward и AdamW; loss и changed weights не свидетельствуют о новой самостоятельной правильной генерации. Отдельный Student Generator не автоматически применяет старую trained memory / KNN / Flow.

## N23-A — НОВЫЙ source-level механизм: silently truncated cosine при разных размерностях

[Distances.CosineSimilarity L10–21](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/06.VectorSearch/Distances.cs#L10-L21) вычисляет dot, na и nb в цикле до длины первого массива без требования равенства размерностей. Если query [1] сравнивать с indexed [1,100], score=1, хотя правильный full query [1,0] даёт примерно 0.0099995. При query длиннее indexed происходит доступ вне границ массива. [BruteForceIndex.Search](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/06.VectorSearch/BruteForceIndex.cs#L20-L30) использует тот же метод, так что «exact oracle» сам некорректен при этом нарушении. [HnswIndex.Add/Load/Search](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/06.VectorSearch/HnswIndex.cs#L49-L151) не связывает vector dimension и encoder SHA с загрузкой индекса.

**Counterexample (PROXY, не .NET):** learned memory с двумя метками: old_wrong [1,100] → WRONG; learned_correct [0.8,0] → CORRECT. Full query [1,0] выбирает CORRECT (score1 против .0099995), короткий query [1] оценивает оба в 1 и при stable insertion order BruteForce возвращает WRONG. Новые знания *существуют*, но несовместимое представление делает их недоступными и выдаёт ложную уверенность. JS source-equivalent proxy исполнен, source SHA/результат сохранены в соседних файлах.

**Important negative control:** [KnnLogicalUnit.Vectorize L265–299](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L265-L299) явно использует один _vectorDimensions при train/query. Реальный mismatch RP01 в этом пути пока НЕ НАЙДЕН. Отдельно уже известен [encoder-fingerprint drift](16-LIVE-ENCODER-INDEX-VERSION-DRIFT.md): он возникает даже при одинаковой длине и не является тем же дефектом.

**Falsifier:** найти реальный consumer и его index build/load, записать фактические query/index dimensions+SHA. Если все совпадают, исключить N23-A как причину отрицательного RP01 exam. Патч допустим после evidence: fail-closed на разную размерность, persist dimension+encoder fingerprint, rebuild из pinned encoder; не «лечить» молчаливым обрезанием.

## N23-B — совпавшее независимое подтверждение, НЕ новый приоритет

[Sampler.Sample L19–39](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/09.Inference/Sampler.cs#L19-L39): ветка topK>0 выбирается до else-if topP<1, поэтому при положительной temperature topP игнорируется, если включён topK. Это уже независимо замечено [параллельным агентом: 23.04](23-INDEPENDENT-LANGUAGE-ROUTING-POS-AND-CROSS-CLAUSE-GAPS-2026-10-16.md); мы лишь воспроизвели численный toy. При logits [5,4,0], topK=2, topP=.6 второй токен остаётся с probability .26894 вместо нуля при topP. RP01 exam с temperature0/topK0/topP1 не затронут.

## N23-C — новая условная диагностика: ordinary decoder скрывает неизвестные token IDs

[BpeTokenizer.Decode L85–92](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/01.Tokenizer/BpeTokenizer.cs#L85-L92) для отсутствующего ID дописывает пустой массив байтов. [TextGenerator.GenerateCore L188–190,226–246](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/09.Inference/TextGenerator.cs#L173-L249) проверяет id на допустимость только в detailed-mode. При model/tokenizer vocabulary drift обычный Generate может незаметно потерять часть сгенерированных токенов; подробный RP01 путь должен отказаться. Потребителя с несовместимыми manifests не показано, поэтому только P2 conditional.

## Матрица остальных факторов: уже задокументированы, НЕ выдавать за новые

| Mechanism | Existing primary investigations |
|---|---|
| random B0, SFT warm start, teacher/student quality | [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [cold-start mini](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md) |
| DPO pair semantic validity, exact masks, response-sum, chosen/rejected token collision | [02-DPO](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [16-pair collapse](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md) |
| q/v-only LoRA, optimizer/beta, frozen reference, weight floor, dropout | [03-LoRA](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [16-DPO](16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md) |
| BPE retrain/order/platform newline, vector tier balance and tokenizer SHA | [21-BPE](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md), [16-KNN](16-KNN-TIER-BALANCE-CAUSAL-MICROEXPERIMENT.md) |
| fake EOS, MAX length, prompt framing, decoder and KV-cache parity | [10-EOS](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md), [16-KV](16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md) |
| KNN duplicate ID/label, metadata type erasure, ignored domain scope, HNSW RNG | [14-KNN](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md) |
| ANN asymmetric backedge after pruning: ALREADY PROPOSED by other agent | [16-independent section9](16-INDEPENDENT-NEW-GAPS-KNN-CORRECTIONS-AND-UNIT-TRUTH-2026-10-16.md) |
| self-labelled semantic Unit, law address mismatch, novel lessons filtered | [21-structured](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md) |
| late KNN correction, phrase-map dominates KNN, translator direction, target mixing | [16-independent](16-INDEPENDENT-NEW-GAPS-KNN-CORRECTIONS-AND-UNIT-TRUTH-2026-10-16.md) |
| false-positive learned accuracy, training acceptance without actual Unit mutation | [21-scaffold](21-INDEPENDENT-CLAUSE-DROPOUT-WHITESPACE-AND-LEGACY-SCAFFOLD-2026-10-16.md), [17-dispatch](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md) |
| consumer bypass of KNN, restore/optimizer/random state, scoring without hidden gold | [18-host](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md), [06-restore](06-CHECKPOINT-RESTORE-ADOPTION.md), [13-parity](13-CAUSAL-LEARNING-PARITY-PROTOCOL.md) |
| matrix→kNN evidence, sibling branches, E51 source leakage | [12-MATRIX](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [11-E51](11-E51-KNN-ORACLE-SPLIT-AND-POSITIVE-MECHANISMS-2026-10-16.md) |

## Teaching ownership — нельзя менять роли задним числом

**Human selector: CODEX with Astra 6 UltraCode. External licensed/open Chinese model: author of BOTH DPO chosen and rejected. Independent CODEX/Astra 6 UltraCode: exact-pair QUALITY judge.** Separate CODEX assessment for real Student outputs. [RP01 simulation input](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L58-L75) marks deterministic generator sides; RP01 manifest says no local Chinese calls in this loop. Those 500-pair exploratory simulations are NOT evidence of an actual Chinese-teacher authoring chain. No such external calls or independent Astra judgement were executed in this audit.

## Priorities and gate

P0: reuse existing [cold-start](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md) and [Unit-vs-Student](11-EXP01-UNIT-MEMORY-IMPACT.md) causal plans with source-disjoint blind law cases. P1 conditional: N23-A run caller dimension inventory then minimal native .NET probe. P2: N23-B/C read-only decoder tests; close for greedy strict RP01 unless real contrary usage is found. [Full experiment protocol](23-MICRO-SHAPE-AND-SAMPLER-CAUSAL-PILOT.md).

**Review status**: independent plan/code review by CODEX/Astra not performed; no native .NET Student learning quality measured; no changes to product code; citations to archived metrics ≠ new runs.
