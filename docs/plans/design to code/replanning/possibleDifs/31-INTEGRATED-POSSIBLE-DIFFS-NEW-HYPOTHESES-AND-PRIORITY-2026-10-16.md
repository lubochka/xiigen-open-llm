# RP01 / N31 — дополнение к общему реестру: история успешного обучения, зоны риска и два новых falsifier

**16.10.2026.** Цель этого документа — оставить одну ясную карту для следующего исполнителя и связать новые N31-R1/R2 с уже собранными независимыми аудитами. **Не** переписывать 200+ других документов и не выдавать reindex за новый баг. Snapshot primary code: `llm_mvp_core@79082340f01b744cdf08185616f2bd818dbd06b8`; planning branch `llm_plan3`. Статус: `READ_ONLY_AUDIT + EXECUTED_JS_PROXY + PLANNING_FILES_CREATED`; `NO_NATIVE_TRAINING_RUN`, `NO_INDEPENDENT_REVIEW`.

## Главная человеческая диагностика

**RP01 не обязательно реализует тот же механизм, которым раньше удавалось обучать систему.** В ранних успешных/обнадёживающих запусках обучался не только Student: работали структурные представления, дискретная память «source→correctOutput», проверяемые правила, kNN, подбор кандидатов и replay. В RP01 свободный ответ проверял прежде всего `random B0→exact DPO→q/v LoRA→TextGenerator`; 50 updates на домен изменили параметры, но не показали улучшения свободных ответов. Следовательно, прежде чем переписывать DPO или делать ещё 500 пар, нужно **установить, какой именно learned state был получателем каждого успешного старого урока и кто потреблял его при правильном ответе**.

Это **гипотеза структурного расхождения** с прямыми частичными source-подтверждениями, а не уже доказанная конкретная причинная ошибка в RP01. [Общий прежний арбитраж](00-MASTER-SYNTHESIS-AND-CONFLICTS.md), [source & branch lineage](26-INDEPENDENT-FULL-GAP-REGISTER-AND-HISTORICAL-LEARNING-LINEAGE-2026-10-16.md), [15 осей из N29](29-CONSOLIDATED-EVIDENCE-ROUTING-AND-NOVELTY-REGISTER-2026-10-16.md), [новый узкий N31](31-NEW-REPLAY-COUNT-CLIFF-AND-PSEUDOSTRUCTURE-2026-10-16.md).

## 1. Что именно показали опубликованные успешные и неуспешные эксперименты

| Программа / branch | Историческое опубликованное наблюдение | Что в действительности доказывает | Что НЕ доказывает |
| --- | --- | --- | --- |
| RP01 C#500, `codex/dpo-implementation` | 500 pairs / 50 AdamW; 16 LoRA tensors changed; free `0/3→0/3`; margin `+0.0012275314` | Реальный параметрический update; итоговый маленький open panel не улучшился | Нельзя говорить «DPO не работает всегда»; n=3 и узкий decoder, cold base |
| RP01 EN→RU500 после C# | ещё 500 pairs / 50 updates; free `0/4→0/4`; margin `+0.0001100063` | Продолжение меняло state, не доказало полезный навык | Не проверяет грамматическую память раннего `TranslationMachine` |
| E09 structural KNN, historical | `top1 .2413→.3507`, 4492 train pairs; `k=5` | Исторически получен source artifact с показателями эффекта AST retrieval | Baseline и enhanced на **разных validation subsets**, нужен paired rerun |
| E34v2 BM25+KNN | `hit@1 .7222→.7778` | Записан result для fusion retrieval | Baseline/enhanced на разных частях val; alpha optimised on another subset; не RP01 Student |
| E43 replay | retention `.3226→.3978` | Исторический replay улучшил measured bigram retention в том run | Не DPO LoRA и не доказательство новых continual Units |
| Ранние `InternalTrainReadyDpoRunner` из `DesignToCode` | Реальный optimizer/checkpoints: V210 134 pairs / 9 batches / train accuracy `.8333`; repeat10 повторял 50 пар десять раз и дошёл до `1.0` train minibatch accuracy | Были **настоящие** DPO training runs в соседней ветке, не только игрушечные kNN | Не frozen unseen free-response, другой encoder/config/датасет |
| Старый hybrid `TranslationMachine` | Обучение `SemanticTokenizerUnit/ExecutorUnit/VerifierUnit` записывает exact-source map, phrase+word map, three-stage KNN, verifier rules | Есть самостоятельный обучаемый **не-весовой** механизм, подходящий для локальных правил | Не означает, что RP01 Student потребляет тот же state |
| Узкий `TranslationMemorySourceLaw` contrast | wrong accepted `4→0`, correct `2→3`, coverage `6/6→3/6` | Детерминированный закон может снизить ошибки **ценой отказов** | Не learned kNN и не прирост neural quality |

Основные источники: [RP01 RECOVERY](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md); [historic table](02-HISTORICAL-EXPERIMENTS.md); [real adjacent-branch DPO receipts](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md); [E43 run JSON](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json); [WorkLeft RAG-02](../../09.09.26WorkLeft-RAG-02-design-to-code.md), [WorkLeft RAG-03](../../09.09.26WorkLeft-RAG-03-merge-and-branches.md). *RAG описывает provenance и старые decisions, но не заменяет actual run/commit.*

## 2. Где конкретно искать все семейства расхождений, не дублируя работу других агентов

Здесь `C` = установленное различие/свойство по source, `H` = причинный эффект для качества пока гипотеза, `N` = узкая новая работа этого дополнения. `P0` — проверять сначала для соответствующей фактической цепочки; не означает, что её уже доказали причиной RP01.

| Ось | Риск / механизм утраты качества | Приоритет / существующий evidence owner |
| --- | --- | --- |
| **1. Получатель обучения** | Neural weights vs trained correction maps / exact memory / learned Unit / orchestration graph смешаны под одним словом «учится» | **P0-C/H:** [00 master](00-MASTER-SYNTHESIS-AND-CONFLICTS.md), [26 lineage](26-INDEPENDENT-FULL-GAP-REGISTER-AND-HISTORICAL-LEARNING-LINEAGE-2026-10-16.md) |
| **2. Pretraining/cold start** | Random B0 `vocab629`, 4 layers, без исходной компетенции; preference-only DPO может не обучить язык | **P0-C/H:** [RP01 audit](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [REC03](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/recovery/RP01.REC03-CAPABILITY.md) |
| **3. DPO material/labels** | Teacher-generated chosen/rejected, wrong rubric, tie, source quality, dataset family leaks, masks/exact v2; legacy translation-hash DPO — **другой** путь | **P0:** [N16 visible-pair collapse](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md), [DPO data/objective](02-DPO-DATA-OBJECTIVE-GRADIENT.md) |
| **4. DPO objective/step** | Weak preference margin vs free answer; 50 updates ≠ 500 optimizer steps; gradient and exact response mask audit | **P0:** [REC02](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/recovery/RP01.REC02-DPO-GRADIENT.md), [28 master](28-DELTA-MASTER-AUDIT-ALL-AXES-HISTORY-AND-NOVEL-CAUSAL-GATES-2026-10-16.md) |
| **5. LoRA recipient/scope** | q/v-only, rank/alpha/scale, wrong parameter topology, frozen reference, quantization incompatibility | **P0 conditional:** [28 master](28-DELTA-MASTER-AUDIT-ALL-AXES-HISTORY-AND-NOVEL-CAUSAL-GATES-2026-10-16.md), [RP01 audit](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) |
| **6. BPE and vector tokenization** | Small vocab, collision/misaligned token IDs, Unicode/whitespace/order, decoder EOS, embedding from incompatible checkpoint | **P0:** [N16 hash](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md), [N21 BPE drift](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md) |
| **7. Semantic law features** | Positional syntax, AST, nullability, negation scope, POS/role and direction must not be replaced by bag-only signatures | **P0 for C#/SQL/lang:** [12 matrix→KNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [27 historical split](27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md) |
| **8. KNN recall/ANN** | Index graph may lose reachable neighbor; top1/first admissible candidate vs constraint-aware ranking; query/index encoder drift | **P1-C/H:** [N28 graph reachability](28-NEW-HNSW-REVERSE-PRUNE-AND-LOST-LESSON-REACHABILITY-2026-10-16.md), [N16 encoder drift](16-LIVE-ENCODER-INDEX-VERSION-DRIFT.md) |
| **9. KNN lexical/context** | Stopwords remove role/negation, source similarity asymmetry, BM25 denominator depends on candidate pool | **P1:** [N26 role polarity](26-NEW-DIFFERENCES-ROLE-POLARITY-AND-DIRECTION-METRIC-2026-10-16.md), [N20 fusion](20-LESSON-ADDITION-RERANKER-NONLOCAL-QUALITY-GAPS-2026-10-16.md) |
| **10. DPO → Unit construction** | Wrong `BuildRejectedCandidate`, predicted word alignment, rejected-annotation contaminating chosen key, lack of real law-oriented labels | **P1:** [N21 structured](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md), [N29 teacher annotation](29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md) |
| **11. Multi-Unit consistency** | Trained tokenizer and executor use different featurizer; exact `|PAIR` mismatch; direction/language key, tag loss, word map overrides | **P1:** [N28 train/infer drift](28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT-2026-10-16.md), [N26 tags](26-NEW-SEMANTIC-LANGUAGE-KEY-AND-RULETAG-PARITY-2026-10-16.md) |
| **12. Verifier/negative-law** | Required terms as `Contains` can accept opposite semantics; context role swapped; cross-language rule leak and false positives | **P1:** [N23 verifier conflict](23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE-2026-10-16.md), [N28 issue parser](28-NEW-VERIFIER-ISSUE-ROLE-PARSER-AND-CONTEXT-LEAK-2026-10-16.md) |
| **13. Checkpoint/restore** | Policy vs reference/optimizer RNG, metadata-only export, Units/kNN manifest state, tokenizer version mismatch and cold reload | **P0 if restore boundary:** [28 master](28-DELTA-MASTER-AUDIT-ALL-AXES-HISTORY-AND-NOVEL-CAUSAL-GATES-2026-10-16.md), [N22 state](22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md) |
| **14. Replay/retention** | Up to three prior manifests per domain, mismatch between DPO and KNN histories, lost transitive examples | **P1:** [N27 replay aperture](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md) |
| **15. Replay relevance count boundary** | With `n<=topK`, enabled relevance scorer **not run**; when `n>topK` it suddenly runs, same correct previous lesson may change place | **P1-N31/C-H:** [N31-R1](31-NEW-REPLAY-COUNT-CLIFF-AND-PSEUDOSTRUCTURE-2026-10-16.md), [native protocol](31-MICRO-REPLAY-RANKING-NATIVE-CAUSAL-PROTOCOL-2026-10-16.md) |
| **16. Replay pseudostructure** | The purported 20% `pattern` weight is literally reused token-Jaccard; no word order / AST constraint in replay | **P1-N31/C-H, domain conditioned:** [N31-R2](31-NEW-REPLAY-COUNT-CLIFF-AND-PSEUDOSTRUCTURE-2026-10-16.md) |
| **17. Product routing** | Consumer may ignore new KNN/Units or training-specific law; learned path and actual product output can diverge | **P0:** [28 product gate](28-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-DIVERGENCE-2026-10-16.md), [18 ProductHost bypass](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md) |
| **18. Evaluation illusion** | `AccuracyAfter` measures coverage or numeric proxy, wrong direction/rubric, count pass ≠ free-response semantic correctness | **P0:** [26 direction bias](26-NEW-DIFFERENCES-ROLE-POLARITY-AND-DIRECTION-METRIC-2026-10-16.md), [N28 Unit eval](28-NEW-N28-UNIT-HELDOUT-OBJECTIVE-NOT-LIVE-QUALITY-2026-10-16.md) |
| **19. Branch/lineage** | Successful behavior may still be in `DesignToCode`/`language_translate`; code merged ≠ behavior integrated; compare authorship, config, active caller, actual result | **P0:** [11 real older DPO](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md), [WorkLeft RAG-03](../../09.09.26WorkLeft-RAG-03-merge-and-branches.md) |

**Не выдавать этот указатель за полный текстовый grep всех файлов:** перечислены ведущие 19 семейств, а их подгипотезы и доказательства живут в существующих `possibleDifs` / `RAG`. Владелец конкретного пункта может его опровергнуть после парного native опыта. Дедуп важнее количества гипотез.

## 3. Доменная политика — полезное различие для одного домена может вредить другому

| Функция | C# / Design→Code | SQL | EN↔RU / другие языки | Универсальные домены |
| --- | --- | --- | --- | --- |
| AST/order/roles | Высокая потребность: методы, типы, control-flow, markup tree | Высокая: join placement, NULL, scope | Только при валидном POS/dep-parser, морфологическом и direction контексте | Не включать без сигнала |
| KNN structural feature | Узлы+связи с версией языка | SQL AST и dialect | POS/lemma/согласование, близкие знаковые различия | lexical+vector baseline |
| DPO vs discrete memory | DPO может учить style; Unit — точные правила | Unit/law guards часто критичны | Domain phrase+word maps могут давать качество без генеративного Student | Выбирать recipient по типу знания |
| Replay | Domain-law/subphase budget, retention критических исключений | Law families по типам NULL/JOIN | Direction + subject/negation/gender partition | TopK по topic без AST может быть нормально |
| Метрика | Compile/semantic clauses/negative law; не только string match | Logical query behavior/cardinality/NULL | Semantic adequacy, wrong accepted, coverage & abstentions | Domain-specific Q_law на unseen |

## 4. Самый короткий порядок дальнейшего исполнения — без новой бюрократической программы

**A. Распутать RP01 напрямую.** Использовать уже существующие [REC01–REC05](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/README.md): REC01 отвечает за call graph, REC02 — 8–32 exact DPO proof, REC03 — cold-start/SFT vs DPO-only. Только после получения на маленьком примере фактического сигнала переходить к 500. Этим не заменить старые KNN эксперименты: параллельно хранить U/G branch отдельно.

**B. Проверить N31 как independent non-neural delta.** Прямой source показывает count cliff + псевдоструктурный вес. Один уже [выполненный JS proxy](31-MICRO-REPLAY-SCORE-AND-ORDER-PROXY-RESULT.json), следующий [нативный one-factor протокол](31-MICRO-REPLAY-RANKING-NATIVE-CAUSAL-PROTOCOL-2026-10-16.md): `3× A=current vs B=forceScore`, затем `4× A=doubleJaccard vs B=structure`. Никакой новой expensive DPO кампании.

**C. Только после native rank signal — связь с learned Unit и ordinary consumer.** Измерить actual `replaySelectedId/knnCorpusId/dpoReplayId/flowId/output` + сохранение после cold reload. Иначе устранение условной низкоуровневой странности не даёт пользы.

**D. Итог — на новых семействах доменных законов.** 12–20 source-disjoint запросов на каждой целевой области, `fullyCorrect`, `Q_law`, `wrongAccepted`, `abstained`, `criticalRegression` и реальные `CPU/RAM/wall time`; фиксировать и ухудшение. Семантические оценки/рубики заморозить до выдачи.

## 5. Границы разрешённого сейчас и человеко-понятный результат

- Созданы **только** новые файлы `N31` в `xiigen-planning` указанного каталога. Продуктовый код, студенческие веса, исходные исторические данные и закрытый каталог в `llm_mvp_core` **не редактировались** через этот аудит.
- JS результат `N31` показывает конструктивную возможность смены выбора на **синтетических** данных, не «выученный студент стал лучше».
- Проверку отдельным CODEX/Astra reviewer, native C# .NET и blind semantic exam **не выполняли**, статус `DRAFT_PENDING_INDEPENDENT_REVIEW`.
- Способ исполнения и проверяемый return packet: [N31 native micro work order](31-MICRO-REPLAY-RANKING-NATIVE-CAUSAL-PROTOCOL-2026-10-16.md); не просить новую теорию или новый план R01–R25.
