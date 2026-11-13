
# RP01 possibleDifs — единая карта причин потери качества и исторической успешной обучаемости, дополнение N35

**16 октября 2026 · обзор актуального planning llm_plan3, core codex/dpo-implementation@216a1f3d6af3f40ceef081dc168b07e4ce3934f9 и доступных sibling DesignToCode/language_translate источников.** Формат: кто реально учится → где теряется сигнал → предыдущий владелец evidence → короткий опыт → домены. Это **консолидирующий навигатор**, а не 29 доказанных причин провала. Новый источник и конкретные причинные гипотезы N35 выделены отдельно. NATIVE_NOT_RUN; CODEX independent Astra judge NOT_CALLED.

## 1. Самый важный факт RP01: действительная мутация параметров ≠ полезная обученность

[RP01 recovery и фактические receipts](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) описывают две последовательные демонстрации:

- **C#500:** 500 записей из 25 template families, 50 AdamW updates, 16 LoRA tensors изменились, 39 base tensors не изменились, но independent free-answer 0/3→0/3.
- **EN→RU500:** еще 500 пар, еще 50 updates, LoRA снова менялась, free-answer 0/4→0/4.
- Обе панели крайне малы и не доказывают нулевой способности на других задачах. Они доказывают только отсутствие наблюдаемого улучшения на именно этих проверенных free outputs. Preference margin, token logits, сохраненный checkpoint, число тестов и GOOD labels сами по себе не равны полезному языковому навыку.
- Исходная B0 — **random 629-token byte BPE vocabulary, hiddenDim512, 4-layer Transformer**, а не pretrained model. Первое препятствие — проверить, способен ли Student вообще генерировать предметно осмысленные ответы до DPO; без этого новые 500 chosen/rejected могут повышать относительные вероятности и оставлять ответы бессмысленными.
- **Ролевой контракт:** выбор учебных задач (human role) делает **CODEX с Astra 6 UltraCode**. Окончательные chosen И rejected генерирует **разрешенная внешняя открытая китайская модель**. Отдельный CODEX с Astra 6 UltraCode независимо проверяет точную DPO пару, отдельно оценивает actual outputs Student. Указанные в [LEARNING-EXAMINATION](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/LEARNING-EXAMINATION.md) Astra-simulated teacher pairs имеют честную метку simulation, **не** provenance внешней модели. Не исправлять ее подменой происхождения в отчете.

## 2. Двадцать шесть классов уже найденных различий; разделить по recipient и evidence

Это **индекс**, повторяющий известные 26 осей из [N30](30-MASTER-QUALITY-LOSS-COVERAGE-AND-SOURCE-LINEAGE-2026-10-16.md), [N33](33-MASTER-DELTA-RP01-HISTORY-RECIPIENTS-AND-FAST-CAUSAL-ORDER-2026-10-16.md), [N32](32-INCREMENTAL-NOVELTY-REGISTER-AND-DECISION-ORDER-2026-10-16.md) без притворства, что все пробелы появились впервые сейчас. Для каждого нужна исходная строка и actual consumer trace.

| Условный класс | Где может исчезнуть польза | Уже имеющееся доказательство / источник | Домен и минимальный фальсификатор |
| --- | --- | --- | --- |
| R01 P0 | Случайная B0 без исходной способности языка/кода; DPO не заменяет SFT | [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [N33 autograd](33-NEW-AUTOGRAD-EMBEDDING-BIAS-AND-SUPERVISED-WARMUP-GAPS-2026-10-16.md) | C#/RU/SQL: SFT chosen-only tiny warmup затем SAME DPO vs DPO-only |
| R02 P0 | Teacher author: simulated/programmatic contrast вместо внешней модели, недостаток реальной сложности | [02 DPO](02-DPO-DATA-OBJECTIVE-GRADIENT.md), RP01 recovery | Любой DPO: проверить external Chinese receipts, CODEX selection, judge |
| R03 P0 | 500 строк имеют 25/20 семейств, а не 500 независимых source families; слабые rejected и shortcut | [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [N32 same prompt](32-NEW-RP01-SAME-PROMPT-CONTRAST-ADMISSION-2026-10-16.md) | C#/RU: train/heldout по независимым семействам, law vs surface |
| R04 P0 | DPO prompt/response mask, sum-vs-mean, sign/beta/ref и backprop | [03 gradients](03-DPO-PAIR-AND-GRADIENT.md) | Тот же tiny 8–32 pair overfit + finite differences |
| R05 P1 | Vector DPO TeachingVector семантические координаты сворачиваются в число ненулевых slots/weight normalization | [N26 AdamW](26-NEW-ADAMW-SCALE-CANCELLATION-2026-10-16.md), [N33 ingress](33-NOVEL-DPO-VECTOR-INGRESS-NONFINITE-ADMISSION-2026-10-16.md) | Только StepVector: signed swap, finite check, same gradient; не direct StepMany |
| R06 P1 | LoRA q/v target/rank/scale/B=0 limit, отсутствие полноценной адаптации нужного decoder | [03 LoRA](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) | Neural: rank8 vs target-module control, same heldout |
| R07 P0 | BPE ids и tokenizer drift, неизвестная EOS семантика, lexical coverage; exact DPO vs legacy bridge | [21 BPE](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md), [16 bridge](16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE-2026-10-16.md) | Tokenizer owner specific; paired exact IDs and lost-tail diagnostics |
| R08 P1 | Stop/EOS/generation length, KV cache vs full logits; encoder logits не гарантируют свободный answer | [26 stop](26-INDEPENDENT-STOP-TARGET-AND-SAMPLER-2026-10-16.md), [N33 Unicode](33-NEW-UTF8-DECODER-CUTOFF-2026-10-16.md) | C#/RU: same checkpoint, first8/16/32/64 and complete output; honest stop |
| R09 P0 | Restore/adoption model/base/LoRA/optimizer/RNG vs historical parent, wrong serving checkpoint | [06 checkpoint](06-CHECKPOINT-RESTORE-ADOPTION.md), [N27 cache alias](27-NEW-MUTABLE-TRANSLATION-CHECKPOINT-CACHE-ALIAS-2026-10-16.md) | Cold new process; actual user answer bit parity + adopted ID |
| R10 P0 | DPO pair → typed many-to-many Units loses content, source POS/AST and rejected law | [05 Units](05-UNITS-FLOW-GRAPH-DETERMINISM.md), [N21 source](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md) | Translation/code: same lesson → Unit owner state → normal Flow |
| R11 P0 | Silent unknown recipient/dispatch, generic learner accepted with no actually trained Unit | [17 silent dispatch](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md) | Trace UnitId/lessonId/count and final learned recipient |
| R12 P1 | KNN query/index version, scope, source law, namespace/language direction and normalization collisions | [N28 tokenizer key](28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT-2026-10-16.md), [N30 direction](30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md) | Translation RU/EN/HE: same valid memory, changed direction/query |
| R13 P1 | HNSW partial Add, reverse prune, invalid vectors, ANN top-k not same as exact | [N30 HNSW](30-NEW-HNSW-FAILED-ADD-PARTIAL-MEMORY-2026-10-16.md), [N28 prune](28-NEW-HNSW-REVERSE-PRUNE-AND-LOST-LESSON-REACHABILITY-2026-10-16.md) | Retrieval: exact brute-force recall@k vs HNSW |
| R14 P1 | AST, POS, syntax roles, order/negation features absent/incorrectly projected | [12 matrix misread](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [N23 E26](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md) | AST C#; POS/negation EN→RU; SQL parse-tree separate |
| R15 P1 | Positive/negative/warning evidence conflicts; negative-only verifier, safe abstain vs false success | [N27 verifier](27-NEW-UNIT-HELDOUT-VERIFIER-TOKENIZER-METRIC-BIAS-2026-10-16.md), [N21](21-NEW-GAPS-SYMBOL-BLIND-LESSON-ADMISSION-AND-DIRECTION-MISROUTING-2026-10-16.md) | Translation + DSL; wrong accepted per actual user case |
| R16 P1 | Phrase composition greedy; contractions/operators erased; KNN first matching source too coarse | [N30 composition](30-INDEPENDENT-NOVEL-COMPOSITION-CONTRACTION-AND-OPERATOR-GAPS-2026-10-16.md) | EN→RU can't/can; C# ->; SQL operators, no global ban |
| R17 P1 | Replay/learning aperture: old knowledge lost after new lesson, stale training cursor | [07 replay](07-CONTINUAL-REPLAY.md), [N27 aperture](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md) | Same old-law heldout before/after new-domain training |
| R18 P1 | Retrieval candidate ranking/scale is nonlocal to newly added lesson, stale count, extra duplicates | [20 nonlocal](20-LESSON-ADDITION-RERANKER-NONLOCAL-QUALITY-GAPS-2026-10-16.md), [N34 alias](34-NEW-ONE-LESSON-DOUBLE-INSERT-UNDER-ALIAS-SIGNATURE-2026-10-16.md) | Translator top-k, 1 lesson vs duplicate alias |
| R19 P1 | KNN exact/phrase/word mapping and Verifier may preempt learned candidate | [22 KNN state](22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md) | Translation: force no exact-source before comparing |
| R20 P1 | Graph/Flow Orchestrator composed answer may bypass actually learned Unit and call deterministic rules | [18 bypass](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md), [05 Units](05-UNITS-FLOW-GRAPH-DETERMINISM.md) | Trace first user input through actual ordinary consumer |
| R21 P1 | Rejected-law annotation contaminates positive KNN key, false negatives and silent Unit closure | [29 rejected](29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md) | Translation: same chosen/rejected lesson, fresh query, no oracle metadata |
| R22 P1 | Decoder/normalization Unicode/script, byte completion and language casing | [N28 Unicode](28-NEW-UNICODE-CANONICAL-KEY-AND-SPANISH-N-FALSE-MOJIBAKE-2026-10-16.md), [N33 UTF8](33-NEW-UTF8-DECODER-CUTOFF-2026-10-16.md) | RU/HE/ES: NFC/NFD + multilingual byte control |
| R23 P1 | Dataset weighting/replay stratification and class imbalance, misleading experiment «failed» | [02 historical](02-HISTORICAL-EXPERIMENTS.md), [N33 E44/E45](33-NEW-E44-E45-PAIR-TARGET-CONFOUND-2026-10-16.md) | Domain-conditioned class-balanced same-panel eval |
| R24 P0 | Leakage: benchmark uses oracle fields, same source/training families; changed subset between variants | [02 historical](02-HISTORICAL-EXPERIMENTS.md), [12 E07-E09](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) | E09/E34: same exact validation files and measured metric |
| R25 P1 | Wrong eval: Unit scores/markers vs semantic lawful user outputs, false ready/pass | [N28 product heldout](28-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-DIVERGENCE-2026-10-16.md), [N33 macro](33-NOVEL-UNIT-SCORE-AND-STUDENT-LENGTH-PARITY-2026-10-16.md) | Real generated paired blind law judge and critical error |
| R26 P0 | Inference adapter/adoption/domain-router policy mismatches ordinary user context | [RP01 current code map](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md), [N30 master](30-MASTER-QUALITY-LOSS-COVERAGE-AND-SOURCE-LINEAGE-2026-10-16.md) | Same checkpoint, raw teacher independent consumer, cold adoption |

Ссылки на owners не являются доказательством каждого quality impact. C=source code condition, R=historical published run, H=causal quality not yet proven, P=source-only mathematical proxy. Не считать одинаковые аргументы новыми находками.

## 3. Три действительно отдельные новые границы N35 в другом recipient GraphRAG

[Новый source аудит](35-NEW-GRAPHRAG-EMBEDDING-PREFIX-METADATA-AND-HIDDEN-FALLBACK-2026-10-16.md) и [трехчастный малый causal-experiment](35-MICRO-GRAPHRAG-PREFIX-METADATA-AND-FALLBACK-DOMAIN-EXPERIMENTS-2026-10-16.md):

- **N35-G1** — EmbeddingModel при обычной nonzero ветке видит лишь первые maxLen BPE IDs, где maxLen512 только значение по умолчанию; GraphRag.Indexer и Local/GlobalSearch теряют semantic suffix. **Отдельно от** Transformer-KNN N23 с явными 64 tokens. Conditional normal vector path source-confirmed, performance H.
- **N35-G2** — Indexer.BuildNodeText возвращает только RawText, если он есть: Signature и Name исключаются из embedding material; отдельные map-метаданные не возвращают их в вектор. Новая representational identity loss до ANN; **не N28 context packing**, которое теряет уже найденный результат. Source-confirmed, quality H.
- **N35-G3** — при почти нулевом hidden embedding norm происходит непрозрачная замена на tokenID%dimension bucket по **полному** тексту (а не token truncation). Может скрыть, что learned encoder недееспособен, но actual zero events и quality не измерялись. Эта ветка делает невозможным простое утверждение G1 «при одинаковом prefix embeddings всегда равны»: fallback может нарушить эту эквивалентность.

Риск доменный: GraphRAG C#/SQL/dialect/DTC по metadata; long-context C#/SQL/translation/DTC по G1; hidden deterministic fallback для всех GraphRAG retrieval domains, только если он срабатывает. Новые N35 эффекты **не объясняют напрямую** текущий offline RP01 Student 0/3 или 0/4 без независимого доказательства GraphRAG call chain.

## 4. Не терять фактические исторические успехи и прежние неправильные интерпретации

| Реальное историческое evidence | Что установлено | Чего не установлено |
| --- | --- | --- |
| [E09 AST-KNN run 01.05](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) | Reported top1 .2413→.3507 (+10.94 процентного пункта) для C# AST-neighbor experiment | **Разные validation panels!** Не доказано matched causal gain, и не доказано, что RP01 DPO получил этот AST |
| [E07 failed matrix → E09 code comparison](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) | E07 tier рассказывал про AST parent/scope features, а фактический key был BPEId/AstNodeType/AstDepth; BPE index не совпадал с character-offset map | «AST parent вреден» не доказано; E07→E09 буквально тот же алгоритм не установлен |
| [E26 contextual POS-dictionary](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-003.json) | .40→1.00 на 15 контекстных случаях; положительный локальный semantic signal | Не универсальное повышение kNN accuracy для C#/SQL |
| [E43 replay](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json) | Retention .3226→.3978 на отдельном bigram/replay опыте | Не перенесено автоматически в RP01 LoRA continuation |
| [E14 Tier3](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-05-01-002.json) | .8824→.8676: отрицательный larger-panel результат и сильный дисбаланс | Не опровержение всех AST/POS/KNN функций |
| [DesignToCode V210 actual DPO run](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode/experiments-v2/INTERNAL-TRAIN-READY-V191/results/train-ready-v210-good-external-plus-internal-guide-full-20260511-014712/internal-train-ready-dpo-summary.json) | 134 pairs, 9 batches, train final accuracy 0.83333, final margin +0.00570256, saved checkpoints (старая ветка) | НЕ blind free answer; там иной tokenizer (stable-hashed words), архитектура/context96 и dataset |

Исторические успехи сохранять в исследовательском реестре, не переписывать отрицательные E07/E14/E44. Если результат возник в другой функции (перенос feature из матрицы в KNN), необходимо воспроизвести сравнение при SAME source family, SAME input and SAME oracle, а не перенести победный процент из другого bench.

## 5. Порядок пяти самых дешевых доказательных опытов

| Очередность | Эксперимент и изолируемый фактор | Граница результата |
| --- | --- | --- |
| **P0-1 neural** | random B0 + 8–32 verified lawful pairs: SFT warmup vs no-SFT; затем абсолютно те же DPO steps и independent free exam | Проверит предпосылку компетенции, не GraphRAG |
| **P0-2 neural** | exact DPO tiny finite differences: response-mask/sign/ref and same-train overfit; один фактор за раз | Gradient/loss sanity, не consumer gain |
| **P0-3 neural** | frozen parent vs saved/cold restored vs actual ordinary Student decoder; first-token logits and same new query | Restore/adoption/decoder attribution |
| **P1-1 Units/KNN** | E07 feature/offset misalignment vs Roslyn spans, E09 same-panel AST KNN; отдельно E26 POS and order/negation translation | Доменные laws; no pooled universal KNN switch |
| **P1-2 GraphRAG** | [MICRO-N35 G1/G2/G3](35-MICRO-GRAPHRAG-PREFIX-METADATA-AND-FALLBACK-DOMAIN-EXPERIMENTS-2026-10-16.md) после proof consumer, 4–8 graph docs, 8–12 blind queries | Source → actual ANN → final law, fallback branch |

Для любого эксперимента: frozen B0 + independent TRAIN/DEV/SEALED family IDs, true law not exact phrase, no answer-source leakage, equal budget, unmodified teacher/model identity, same frozen questions A/B, source/commit hashes, final output evidence. Только actual improved independent final law без critical regressions можно записать OBSERVED_SMALL_GAIN; если улучшился margin или ANN recall, но не конечный ответ — SIGNAL_ONLY. Если 8–12 matched cases не дали прироста — NO_OBSERVED_GAIN, не «провал всей архитектуры».

## 6. Что реально завершено в данном документе

**Сделано:** прочитан действующий RP01 recovery, кодовые пути core DpoTrainer/LoraLinear/EmbeddingModel/GraphRAG, sample RAG Master Index, источники E09/E26/E14/E43 и DesignToCode V210, проверены существующие N23/N28/N32/N33/N34 аудит-owners; создан новый N35 и быстрые falsifiers. **Не сделано:** полный постраничный обход всех RAG файлов и соседних веток, all-262-file independent semantic review, native compilation/training/GraphRAG quality run, CODEX Astra independent judging, запретный core protected folder write (не выполнялась). Это **полезный ограниченный аудит**, не сертификат исчерпывающей полноты.

**Последующая независимая рецензия CODEX/Astra 6 UltraCode:** сверить каждую ссылку/line и точный recipient, найти дубли относительно 262 existing possibleDifs, дополнить verified source list, провести не более одного нового микротеста на выбранную причинную ось; отделить docs publication от product implementation. Результаты публиковать только с candidate-bound review SHA.
