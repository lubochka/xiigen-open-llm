# Сверка всех карточек опытов с коммитами

Дата: 27 октября 2026. Ветка `codex/dpo-implementation`.

Это ответ на требование пройти **полный** список карточек, а не агрегаты.
Для каждой карточки указано, исполнительская она или справочная; для каждой
исполнительской — узел прогона и коммит, которым его результаты впервые
попали в ветку. Таблица построена из каталога карточек, содержимого узлов и
истории ветки, а не из памяти.

## Счёт

| | |
|---|---:|
| карточек всего | 272 |
| справочных (номер P не является номером запуска) | 125 |
| исполнительских | 147 |
| исполнительских с узлом прогона и коммитом | 147 |
| исполнительских без прогона | 0 |

Справочные карточки запускать нечего: в них самих написано «справочная
карточка … номер P не является номером запуска», и незаполненными остаются
подслучай, данные, модель, токенизатор, команда и место вывода.

## Исполнительские карточки

| карточка | узел прогона | коммит | дата |
|---|---|---|---|
| AST-SIB-CACHE-01 | `AST01` | `d2ec9b30916` | 2026-10-22 |
| ATTN-CAUSAL-SCORES-01 | `CS01` | `f4d6627aa3f` | 2026-10-23 |
| ATTN-SOFTMAX-COPY-01 | `AC01` | `ff6ed114e49` | 2026-10-23 |
| ATTN-TRANSPOSE-STRIDES-01 | `AT01` | `2245937c6c9` | 2026-10-22 |
| BACKWARD-SCRATCH-QUALIFIED-01 | `BWS01` | `7c75b495f1d` | 2026-10-23 |
| BPE-NO-MATCH-COPY-01 | `N-KU-01` | `341f1c730b2` | 2026-10-23 |
| BPE-REALIZED-MERGE-COUNT-01 | `BR01` | `70062c090a5` | 2026-10-22 |
| BPE-WEIGHTED-CHUNKS-01 | `BW01` | `7a9fa58d551` | 2026-10-22 |
| COLLISION-ALARM-BAND-DENSITY-01 | `CDG28` | `996a8c27402` | 2026-10-24 |
| COLLISION-ALTERNATION-LONG-WINDOW-01 | `CDG42` | `064b9c5b490` | 2026-10-25 |
| COLLISION-ANSWER-LOSS-MONITOR-01 | `CDG27` | `03b6379842b` | 2026-10-24 |
| COLLISION-BAD-BATCH-AT-END-01 | `CDG17` | `7608b8304c1` | 2026-10-24 |
| COLLISION-BAD-LESSON-POSITION-01 | `CDG16` | `aa0c73f2e7a` | 2026-10-24 |
| COLLISION-BLAST-RADIUS-01 | `CDB02` | `edead968037` | 2026-10-23 |
| COLLISION-CARD-RATE-01 | `CDG24` | `770262ec5fa` | 2026-10-24 |
| COLLISION-CARD-RATE-DOSE-01 | `CDG25` | `731cc29d97c` | 2026-10-24 |
| COLLISION-CLIFF-AND-LATE-BUDGET-01 | `CDG08` | `25628bd1581` | 2026-10-25 |
| COLLISION-COMBINED-REPAIR-01 | `CDG31` | `459a13673be` | 2026-10-24 |
| COLLISION-CONTAMINATION-RATE-01 | `CDG14` | `29c49f5e4e7` | 2026-10-24 |
| COLLISION-CROSS-REPAIR-SIMILARITY-01 | `CDG36` | `ea7d90f5cc4` | 2026-10-24 |
| COLLISION-CYCLE-LIMIT-01 | `CDL01` | `74f93272c9b` | 2026-10-23 |
| COLLISION-CYCLES-01 | `CDC01` | `ebbed8aca64` | 2026-10-23 |
| COLLISION-DAMAGE-DISTANCE-01 | `CDG03` | `13341cb7e03` | 2026-10-23 |
| COLLISION-DETECTION-DELAY-COST-01 | `CDG07` | `10a20f5e69e` | 2026-10-24 |
| COLLISION-DOSE-01 | `CDR01` | `d839922f8fb` | 2026-10-23 |
| COLLISION-DPO-ASYMPTOTE-01 | `CDG33` | `05a6db57947` | 2026-10-24 |
| COLLISION-DPO-STEPS-CURVE-01 | `CDG22` | `25628bd1581` | 2026-10-25 |
| COLLISION-DPO-VS-SFT-RECOVERY-01 | `CDG20` | `25628bd1581` | 2026-10-25 |
| COLLISION-ERROR-SHAPE-01 | `CDG37` | `dc02b9da161` | 2026-10-24 |
| COLLISION-ERROR-SHAPE-02 | `CDG38` | `e5b5e21c275` | 2026-10-24 |
| COLLISION-FRAGILITY-01 | `CDF01` | `08ad859b5c1` | 2026-10-23 |
| COLLISION-HANDOVER-POINT-01 | `CDG32` | `9b01d812875` | 2026-10-24 |
| COLLISION-INTERLEAVE-01 | `CDI01` | `2e0ad0bb0d8` | 2026-10-23 |
| COLLISION-LATE-REGRESSION-SET-01 | `CDG09` | `25628bd1581` | 2026-10-25 |
| COLLISION-LESSON-RATE-01 | `CDG23` | `c794be0536b` | 2026-10-24 |
| COLLISION-MIXTURE-REPAIR-01 | `CDG30` | `9814954ebf5` | 2026-10-24 |
| COLLISION-NUMBNESS-CAUSE-01 | `CDN01` | `e756cfd8fdc` | 2026-10-23 |
| COLLISION-PLAIN-FORGETTING-CONTROL-01 | `CDF02` | `1608da1b864` | 2026-10-23 |
| COLLISION-POST-BATCH-RECOVERY-01 | `CDG18` | `b6dfda242e1` | 2026-10-24 |
| COLLISION-PREVENTION-01 | `CDP01` | `25628bd1581` | 2026-10-25 |
| COLLISION-RECALIBRATION-01 | `CDR02` | `25628bd1581` | 2026-10-25 |
| COLLISION-RECOVERY-BREADTH-01 | `CDG19` | `c318b5bec5a` | 2026-10-24 |
| COLLISION-RECOVERY-BY-PIT-DEPTH-01 | `CDG39` | `c1e8f5bd41b` | 2026-10-24 |
| COLLISION-RECOVERY-BY-PIT-DEPTH-02 | `CDG40` | `718d129b187` | 2026-10-24 |
| COLLISION-RECOVERY-BY-PIT-DEPTH-03 | `CDG41` | `1f75f2e4d3d` | 2026-10-24 |
| COLLISION-REGRESSION-SET-REPAIR-01 | `CDG01` | `609710f430f` | 2026-10-23 |
| COLLISION-REPAIR-01 | `CDB01` | `6d68f3f63eb` | 2026-10-23 |
| COLLISION-REPAIR-AGE-VS-SAME-ROW-PRACTICE-01 | `CDG44` | `1e65b066f19` | 2026-10-25 |
| COLLISION-REPAIR-AGE-VS-WINDOW-01 | `CDG43` | `893f34a37d2` | 2026-10-25 |
| COLLISION-REPAIR-BUDGET-GROWTH-01 | `CDG02` | `25628bd1581` | 2026-10-25 |
| COLLISION-REPAIR-DISCRETE-READOUT-01 | `CDG65` | `905d552b425` | 2026-10-26 |
| COLLISION-REPAIR-FOREIGN-COST-VS-FACT-01 | `CDG45` | `9853ee75fb8` | 2026-10-25 |
| COLLISION-REPAIR-FOREIGN-RECENCY-01 | `CDG46` | `f33c4f97203` | 2026-10-25 |
| COLLISION-REPAIR-FOREIGN-TAIL-01 | `CDG49` | `32316896284` | 2026-10-25 |
| COLLISION-REPAIR-FOREIGN-TAIL-LENGTH-01 | `CDG50` | `4d8530365c3` | 2026-10-25 |
| COLLISION-REPAIR-FOREIGN-TAIL-MIDDLE-01 | `CDG51` | `0550370a673` | 2026-10-25 |
| COLLISION-REPAIR-FULL-SPEED-GAP-SMOOTHNESS-01 | `CDG58` | `726b33aea95` | 2026-10-25 |
| COLLISION-REPAIR-GAP-CONTENT-FOREIGN-OR-OWN-01 | `CDG53` | `8be2749aff7` | 2026-10-25 |
| COLLISION-REPAIR-GAP-COST-UPDATES-OR-MOVEMENT-01 | `CDG54` | `51c19ba2de3` | 2026-10-25 |
| COLLISION-REPAIR-GAP-LENGTH-STRUCTURE-01 | `CDG59` | `06693bee6e7` | 2026-10-25 |
| COLLISION-REPAIR-GAP-MOVEMENT-CURVE-01 | `CDG55` | `a7c06d46445` | 2026-10-25 |
| COLLISION-REPAIR-HORIZON-BY-DENSITY-01 | `CDG29` | `d3c3500e8f3` | 2026-10-24 |
| COLLISION-REPAIR-INTERLEAVE-GRAIN-01 | `CDG62` | `2066cf432b1` | 2026-10-25 |
| COLLISION-REPAIR-KNOWN-ANSWER-VALIDATION-01 | `CDG66` | `ab1e9a1585b` | 2026-10-26 |
| COLLISION-REPAIR-LAST-PRACTICE-ROW-01 | `CDG48` | `eeb91ad92ca` | 2026-10-25 |
| COLLISION-REPAIR-LENGTH-RATE-OR-CEILING-01 | `CDG60` | `b818d0301ac` | 2026-10-25 |
| COLLISION-REPAIR-MASSED-VERSUS-SPACED-01 | `CDG61` | `8585d6c7739` | 2026-10-25 |
| COLLISION-REPAIR-MEASURE-ARBITRATION-01 | `CDG68` | `a23026550e6` | 2026-10-26 |
| COLLISION-REPAIR-MULTI-PAIR-READOUT-01 | `CDG64` | `bdff342f3d9` | 2026-10-26 |
| COLLISION-REPAIR-NAME-VERSUS-RULE-01 | `CDG67` | `b917116b1b8` | 2026-10-26 |
| COLLISION-REPAIR-OTHER-ROW-01 | `CDG34` | `223b77e27e0` | 2026-10-24 |
| COLLISION-REPAIR-OWN-ROW-DOSE-01 | `CDG47` | `5f1d5be2f34` | 2026-10-25 |
| COLLISION-REPAIR-PRACTICE-DISTANCE-OR-POSITION-01 | `CDG52` | `12844a98b49` | 2026-10-25 |
| COLLISION-REPAIR-SHORT-SLOW-GAP-01 | `CDG56` | `88ba5133d4f` | 2026-10-25 |
| COLLISION-REPAIR-SLOW-GAP-PIT-01 | `CDG57` | `84052700694` | 2026-10-25 |
| COLLISION-REPAIR-SPILLOVER-CAUSE-01 | `CDG04` | `9e6289d0875` | 2026-10-23 |
| COLLISION-REPAIR-STABLE-READOUT-01 | `CDG63` | `c62acfd85fe` | 2026-10-25 |
| COLLISION-REPLICATION-OTHER-ROW-01 | `CDG12` | `5f7e3e047f9` | 2026-10-24 |
| COLLISION-REPLICATION-THIRD-ROW-01 | `CDG13` | `5851230f621` | 2026-10-24 |
| COLLISION-SINGLE-BAD-LESSON-01 | `CDG15` | `fde1e56ea30` | 2026-10-24 |
| COLLISION-SOURCE-CUTOFF-01 | `CDG05` | `f52a1da8fd1` | 2026-10-23 |
| COLLISION-SOURCE-CUTOFF-ISOLATED-01 | `CDG06` | `25628bd1581` | 2026-10-25 |
| COLLISION-TEN-DPO-STEPS-01 | `CDG21` | `25628bd1581` | 2026-10-25 |
| COLLISION-TIMING-VS-BREADTH-01 | `CDG10` | `e949fb0aa4b` | 2026-10-24 |
| COLLISION-TWO-DAMAGED-ROWS-01 | `CDG35` | `3cf1bac3dbf` | 2026-10-24 |
| COLLISION-UNRELATED-TRAINING-HEALING-01 | `CDG26` | `bd046e74701` | 2026-10-24 |
| COLLISION-WEIGHTED-REPAIR-01 | `CDG11` | `98933c78359` | 2026-10-24 |
| DPO-SHIFTED-CONTEXT-01 | `SC01` | `899cd1fc15b` | 2026-10-22 |
| DPO-TERMINAL-TARGET-QUALIFIED-01 | `SC02` | `8c1ae3b73fb` | 2026-10-23 |
| DPO-TERMINAL-TARGET-QUALIFIED-02 | `DQ01` | `6f9ccc3712a` | 2026-10-25 |
| DPO-TOKENIZER-SNAPSHOT-01 | `TS01` | `b1f41cae621` | 2026-10-22 |
| E-DPO-SFT-ORDER | `OR01` | `fd12a0ca4db` | 2026-10-22 |
| E-DPO-VALID-LENGTH | `PD01` | `4e993a33c93` | 2026-10-22 |
| E-SFT-MOMENT-CONTINUITY | `MC01` | `38dd9fed12c` | 2026-10-21 |
| E-SFT-RESPONSE-MASK | `MS01` | `f9cd4029d35` | 2026-10-21 |
| FILE-STATIC-RECEIVER-01 | `FSR01` | `e1225147aa1` | 2026-10-23 |
| FROZEN-BASE-CHECK-ORDER-CONTROL-01 | `FBQ02` | `3d9c40fd00e` | 2026-10-23 |
| FROZEN-BASE-CHECK-QUALIFIED-01 | `FBQ01` | `2f1e07175b8` | 2026-10-23 |
| FROZEN-BASE-CHECK-REPEATS-01 | `FBQ04` | `770febf019a` | 2026-10-23 |
| FROZEN-BASE-CHECK-SHARE-01 | `FBQ03` | `755a8cd2ada` | 2026-10-23 |
| FROZEN-BROADCAST-MAP-01 | `FBM1` | `cba27d86048` | 2026-10-22 |
| GRAD-ACCUM-COPY-01 | `AGC1` | `0a33a6e39dd` | 2026-10-22 |
| GRADB-LOCALITY-01 | `KGB` | `6f076240567` | 2026-10-22 |
| HIERARCHICAL-LOCAL-HISTORY-01 | `HCL01` | `38c05f2e9a0` | 2026-10-23 |
| HNSW-QUERY-NORM-REUSE-01 | `HNQ01` | `25628bd1581` | 2026-10-25 |
| IDENTITY-TRAIN-INDEX-01 | `IDX01` | `25628bd1581` | 2026-10-25 |
| INITIAL-GRADIENT-COPY-QUALIFIED-01 | `LFG01` | `a410e4534a4` | 2026-10-23 |
| JSON-OPTIONS-REUSE-01 | `JO1` | `cd02d3efb65` | 2026-10-22 |
| KH-COST-01 | `N-KU-01` | `341f1c730b2` | 2026-10-23 |
| KH-EXT-01 | `KHE` | `c8b0d8a029c` | 2026-10-22 |
| KN-ROUTER-LEARNED-KEY-01 | `KR01` | `ebaf7760a36` | 2026-10-23 |
| KNN-DOMAIN-CONSTRUCTOR-ROUTE-01 | `KDR01` | `f1309ffb219` | 2026-10-26 |
| KNN-DOMAIN-PRINCIPLES-01 | `KDP01` | `e585a3290e5` | 2026-10-26 |
| KNN-DOMAIN-PRINCIPLES-01-fixtures | `KDP01` | `e585a3290e5` | 2026-10-26 |
| METHOD-KNN-TARGET-SCOPE-01 | `MKSC01` | `3acae42f7ef` | 2026-10-22 |
| N-HEAD-01 | `NH01` | `885745ba5c9` | 2026-10-22 |
| N-KU-01-do-loop-label | `N-KU-01` | `341f1c730b2` | 2026-10-23 |
| N60-e1-verifier-admission | `N60-E1-VERIFIER-ADMISSION` | `26332500f76` | 2026-10-21 |
| N61-dpo-reference-reuse | `N61-REF-CACHE` | `0cf3634ce03` | 2026-10-21 |
| N61-mode1-retention-and-identity | `N61-MODE1` | `4f35cb71b3d` | 2026-10-21 |
| N62-neural-update-costs | `N62-R1 / N62-R2 / E-OPT-STEP / LF-03` | `883496bca18, 66947785d13, 6e3e8864933, dd150aa5216` | 2026-10-20 |
| N62-training-context | `T-TRAIN-CONTEXT-01` | `2297566a5c0` | 2026-10-21 |
| N63-INGRESS-LENGTH-LEARNED-01 | `N63IL` | `c58d08bcc22` | 2026-10-23 |
| N63-correction-collision | `N63C` | `256f603e700` | 2026-10-21 |
| N63-ingress-length | `N63I` | `dd150aa5216` | 2026-10-20 |
| N63-replay-capacity | `N63R` | `fb9175cc752` | 2026-10-20 |
| NEURAL-FROZEN-TRANSPOSE-01 | `LEARN-SANITY-PROBE` | `0b76ab950d8` | 2026-10-20 |
| NEURAL-FROZEN-TRANSPOSE-RETRAINED-01 | `FTR01` | `f12e190b47f` | 2026-10-23 |
| NG01-null-guard-label | `NG01` | `70062c090a5` | 2026-10-22 |
| OPTIMIZER-STEP-READ-QUALIFIED-01 | `OPT01` | `40242e7c6aa` | 2026-10-23 |
| PARENT-ADOPTION-01 | `PA01` | `1acbfeeb949` | 2026-10-23 |
| POS-QUERY-SET-ONCE-01 | `PQ01` | `a87fef91ba5` | 2026-10-23 |
| RMS-COORD-ALLOC-01 | `RMS01` | `7a9fa58d551` | 2026-10-22 |
| RMS-MEAN-BACKWARD-01 | `MB01` | `ed0112636e9` | 2026-10-23 |
| RMS-ROW-REDUCE-01 | `RD01` | `b1a756d25c1` | 2026-10-22 |
| ROPE-TRAIN-TABLE-01 | `RPE01` | `d2ec9b30916` | 2026-10-22 |
| SAVED-STATE-PANEL-0407-01 | `PNL0407` | `59659598ebd` | 2026-10-23 |
| SEPARATOR-FORMAT-MISMATCH-01 | `SEP01` | `b6c0fff7d57` | 2026-10-25 |
| SFT-EXP-REUSE-01 | `SEXP01` | `8734c4e7de7` | 2026-10-23 |
| SILU-SIGMOID-REUSE-01 | `SILU01` | `fbf250a43d2` | 2026-10-22 |
| SVD-DIRECTED-TARGET-01 | `SVD01` | `d75f9aa43d0` | 2026-10-23 |
| VT-APOSTROPHE-01 | `QVA01` | `916a4ffb5db` | 2026-10-22 |
| WIKI-FEATURE-KEYS-01 | `WK01` | `31b8888a8af` | 2026-10-22 |
| WIKI-FEATURES-ONCE-01 | `WC01` | `1fa9bac16ff` | 2026-10-22 |
| WIKI-MARGIN-01 | `WM01` | `43cb1c40a76` | 2026-10-21 |
| WIKI-MARGIN-BALANCED-01 | `WM02` | `79fa85aee4b` | 2026-10-23 |
| WIKI-TAXONOMY-01 | `WT01` | `43cb1c40a76` | 2026-10-21 |

## Справочные карточки

Запуску не подлежат; перечислены для полноты счёта.

`P01-trainable-recipient`, `P02-warm-base`, `P03-historical-score-lineage`, `P04-preference-contrast`, `P05-response-length-objective`, `P06-teaching-vector-adamw`, `P07-materialized-token-contrast`, `P08-answer-boundary`, `P09-cached-forward`, `P10-composed-gradient`, `P100-topk-ties`, `P101-learning-budget`, `P102-teacher-forced-decoding`, `P103-stack-before-normalization`, `P104-law-eligibility`, `P105-frozen-reference`, `P106-corrected-semantic-target`, `P107-lawful-neighbor-aggregation`, `P108-metadata-carrier`, `P109-gold-validity`, `P11-autograd-reachability`, `P110-numeric-matrix-cost`, `P111-reward-credit`, `P112-family-disjoint-evaluation`, `P113-feature-energy`, `P114-zero-weight-pair`, `P115-bm25-statistics-cache`, `P116-e43-file-boundary`, `P117-translation-direction-memory`, `P118-projectwiki-content`, `P119-learning-dynamics`, `P12-optimizer-boundaries`, `P120-semantic-compression-laws`, `P121-project-adapter-selection`, `P122-root-channel-unicode-digits-operators`, `P123-design-conversion-critic-evidence`, `P124-p3-fixed-discriminator-contract`, `P125-executor-memory-learning`, `P13-finite-ingress`, `P14-teacher-provenance`, `P15-retention-replay`, `P16-semantic-order-roles`, `P17-tokenizer-refit`, `P18-state-identity`, `P19-mutable-output`, `P20-hnsw-reachability`, `P21-hnsw-ingress-atomicity`, `P22-hnsw-rng-continuation`, `P23-retrieval-fusion`, `P24-replay-selection`, `P25-cold-restore`, `P26-structural-features`, `P27-structural-codec`, `P28-morphology-family`, `P29-formula-memory`, `P30-learned-fusion-alpha`, `P31-source-trust`, `P32-skill-selection`, `P33-domain-classifier`, `P34-router-labels`, `P35-actual-neighbor-votes`, `P36-incremental-router`, `P37-empty-router-query`, `P38-learned-segmentation`, `P39-oracle-output-separation`, `P40-router-feedback`, `P41-stop-and-split`, `P42-checker-arbiter`, `P43-contextual-pos`, `P44-template-projection`, `P45-learned-reordering`, `P46-pair-value`, `P47-file-vector-context`, `P48-weighting-curriculum`, `P49-probe-warm-start`, `P50-graph-dependencies`, `P51-translation-context`, `P52-semantic-annotation`, `P53-batch-atomicity`, `P54-phrase-composition`, `P55-translation-laws`, `P56-verifier-context`, `P57-semantic-measurement`, `P58-feedback-learning`, `P59-correction-visibility`, `P60-signature-duplicate`, `P61-unknown-tail`, `P62-signature-monotonicity`, `P63-source-containment`, `P64-tier-postfilter`, `P65-history-admission`, `P66-heldout-contrast`, `P67-correction-identity`, `P68-pretrain-pipeline`, `P69-graphrag-context`, `P70-wikitrace-context`, `P71-perplexity-alphabet`, `P72-project-conditioning`, `P73-domain-conditioning`, `P74-memory-eviction`, `P75-sampler-residual`, `P76-utf8-boundary`, `P77-rare-class-support`, `P78-loss-plateau`, `P79-grammar-leakage`, `P80-marker-denominator`, `P81-input-conditioning`, `P82-file-split`, `P83-expression-body`, `P84-lambda-family`, `P85-composite-obligations`, `P86-fractional-response-mask`, `P87-mixed-objectives`, `P88-unit-context-consumption`, `P89-density-stop`, `P90-tagged-search`, `P91-method-pair-split`, `P92-dpo-memory-flow-bridge`, `P93-confidence-abstention`, `P94-typed-many-results`, `P95-learned-flow-edge`, `P96-async-orchestration`, `P97-learned-graph-order`, `P98-tokenizer-capacity`, `P99-unknown-token-id`

## Что эта сверка не утверждает

Она говорит только, что у каждой исполнительской карточки есть сохранённый
прогон и коммит, которым он опубликован. Она **не** говорит, что каждый
результат положителен, что каждая учебная цель достигнута и что каждый
перенос доказан. Позиции, которые аудит 23.10 пометил как требующие
исправленной постановки, разобраны отдельно, и их исходы — включая
отрицательные — записаны в их собственных отчётах.
