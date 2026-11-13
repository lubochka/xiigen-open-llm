# N35 — Сводная причинная карта RP01 и исторического обучения: кому пришёл урок и дошёл ли до ответа

**16.10.2026 · исследование исходников, RAG и соседних веток; версия `llm_plan3`.** Этот файл — дополнительный указатель на доказательства, **не объявляет 259 уже написанных документов новыми** и не подменяет историю новым run. Сопоставлены master-файлы `possibleDifs`, read-only `llm_mvp_core/docs/plans/replanning-possible-diffs`, исходный [алгоритм DPO](../../Алгоритм%20создания%20ДПО.md), [original task](../../original%20task.md), RP01 и код из `codex/dpo-implementation`. Ветка `DesignToCode` и `language_translate` используются как **исторические источники**, не как активная конфигурация. Все новые документы в данном пакете — **только в целевом possibleDifs**.

## 1. В чём действительно задача

Плохой ответ после обучения может означать как минимум шесть *разных* отказов:

```text
  Источник → реальная разрешённая DPO-пара → model-visible IDs/contrast
       ↘                                   ↘
   ошибка admissibility                  DPO → q/v-LoRA → neural policy → TextGenerator
       ↓                                   ↓
   разложение DPO на Tokenizer/Executor/Verifier (не backprop)
       ↓
   обучаемые Units / contextual word map / KNN / verifier / graph
       ↓
   replay + checkpoint + restore нужного получателя
       ↓
   зарегистрированный ordinary user flow / fallback / law guards
       ↓
   независимый незнакомый вопрос → предметно правильный ответ?
```

**Точное архитектурное разграничение:** Original design описывает `ILearningUnit` (domain-aware many-to-many), `ILearningFlow` (композиция и коррекция), `ILearningOrchestrator` (асинхронные цепочки) и `ILearningDynamicGraph` (динамическая связь паттернов); runtime `ILearningMachine` управляет обучением. Не отождествлять эти роли с `DpoTrainer`, `KnnLogicalUnit`, `TranslationMachine` или `TextGenerator`. Генератор текста не читает автоматически KNN/Unit память, а Unit-updates не доказывают neural gradient.

RP01 [frozen diagnosis](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) показывает: **C#500, 25 семейств × 20, 50 optimizer updates**, 16 изменённых LoRA tensors при frozen base 39; preference margin вырос на `0.0012275314`, но **free 0/3→0/3**. ENRU500 ещё 50 steps, margin `+0.0001100063`, **free 0/4→0/4**. Не называть эти числа «обучение восстановлено»; метрики для иных Unit не складывать с free answers Student.

## 2. All-axis register: механизм → что могло исчезнуть → какой один эксперимент

Легенда: **C** — поведение видно в исходниках; **R** — опубликованный исторический result, без нашей репликации; **H** — качество *возможно*, не измерено; **P** — proxy/бумажный вывод. `P0` — сначала блокеры собственного Student, `P1` — learned Unit/KNN, `P2` — масштабирование после causal gain.

| Ось | Приоритет/recipient | Источник evidence/owner, не переоткрываем | Быстрый различающий опыт |
|---|---|---|---|
| A01 Нулевая language base/random B0, слишком малый vocab/context | P0 Neural | [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [B0/SFT](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md) | Замороженные 8–32 sentences: может ли B0 научиться next-token и породить один новый ответ? |
| A02 DPO chosen/rejected teacher provenance, качество двух сторон, same-prompt contrast | P0 DPO | [DPO objective](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [same prompt](32-NEW-RP01-SAME-PROMPT-CONTRAST-ADMISSION-2026-10-16.md), [input-to-generation](27-RP01-DPO-COMMON-PREFIX-GENERATION-GAP-2026-10-16.md) | Сравнить каждый actual masked response span, independent wrong law, first differing IDs |
| A03 Response-token masks, BOS/EOS, prompt budget, choice clipping/erasure | P0 DPO | [translation model-visible collapse](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md), [token erasure](16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE-2026-10-16.md), [EOS](26-INDEPENDENT-STOP-TARGET-AND-SAMPLER-2026-10-16.md) | Print exact IDs/masks chosen/rejected, nonzero gradient on distinguishing token only |
| A04 LoRA q/v adapters frozen base, rank/scale, optimizer/reference gradient | P0 Student | [LoRA](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [autograd](33-NEW-AUTOGRAD-EMBEDDING-BIAS-AND-SUPERVISED-WARMUP-GAPS-2026-10-16.md), [AdamW](26-NEW-ADAMW-SCALE-CANCELLATION-2026-10-16.md) | Finite-difference loss and weight delta for one tiny legal batch; check missing gradient paths |
| A05 Tokenizer build/train/save/reload, BPE offsets/order, OOV and Unicode | P0/P1 neural/encoder | [BPE drift](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md), [UTF8](33-NEW-UTF8-DECODER-CUTOFF-2026-10-16.md), [Unicode](28-NEW-UNICODE-CANONICAL-KEY-AND-SPANISH-N-FALSE-MOJIBAKE-2026-10-16.md), [pretrain source](34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md) | Identical token/roundtrip SHA cold vs warm; language-specific minimal contrast |
| A06 Decoder/sampling/termination, source-vs-output length, saved model consumption | P0 inference | [answer boundary](26-INDEPENDENT-STOP-TARGET-AND-SAMPLER-2026-10-16.md), [length parity](33-NOVEL-UNIT-SCORE-AND-STUDENT-LENGTH-PARITY-2026-10-16.md) | Same checkpoint/input, greedy output at fixed EOS/limit, exact first-token trace |
| A07 Explicit mathematical learning vs deterministic self-label / templates / law-gates | P0/P1 | [N21 structured](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md), [N27 self-scoring](27-NEW-UNIT-HELDOUT-VERIFIER-TOKENIZER-METRIC-BIAS-2026-10-16.md), [consumer bypass](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md) | Train/no-train/shuffled-label, compare same ordinary caller; standalone fixture PASS is not enough |
| A08 DPO chosen→typed sub-units, wrong rejected lesson, mapping/origin loss | P1 Units | [structured builder source](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L77-L145), [N29](29-NEW-FIRST-TOKEN-PSEUDOALIGNMENT-ORDER-CONTAMINATION-2026-10-16.md), [feedback](29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md) | Replay same admissible pair; compare raw teacher law vs three recipient outputs |
| A09 History E26 POS left/right → current lemma/concept map, morphology | P1 RU/EN translation | [E26 transfer](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md), [POS pilot](25-MICRO-E26-POS-CONTEXT-CAUSAL-EXPERIMENT-2026-10-16.md) | one-factor POS-neighbor keys, heldout same lemma/different law; not C# AST |
| A10 History E07 AST matrix vs E09 KNN target and feature profile | P1 C# / structural code | [E07→E09 proof limits](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [pair split](30-NEW-E05-E07-STRUCTURAL-EXPERIMENT-VALIDITY-2026-10-16.md), [source](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/Experiments/E09StructuralKnn.cs) | *same-file, same-label, same-val-set*: AST rich KNN versus hashed baseline |
| A11 KNN index identity: hashed BPE/POS, k actual, role/order/negative score | P1 translation/code/DSL | [generic KnnLogicalUnit](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L215-L299), [KNN scope](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md), [N26 polarity](26-NEW-DIFFERENCES-ROLE-POLARITY-AND-DIRECTION-METRIC-2026-10-16.md) | freeze hits, only add domain-specific order/AST/polarity features, blind law-negative panel |
| A12 Translation KNN source guard/consumer; FIFO / duplicate alias / latest correction | P1 Translation Unit | [N16 FIFO](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md), [N34 duplicate](34-NEW-ONE-LESSON-DOUBLE-INSERT-UNDER-ALIAS-SIGNATURE-2026-10-16.md), [direction](30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md) | one newer lawful lesson; verify it appears inside k and wins real `TrainedTranslationAdapter`, not just `Count` |
| A13 HNSW insert/reverse-prune / mutable index / wrong or unreachable neighbor | P1 ANN only | [reverse prune](28-NEW-HNSW-REVERSE-PRUNE-AND-LOST-LESSON-REACHABILITY-2026-10-16.md), [failed add](30-NEW-HNSW-FAILED-ADD-PARTIAL-MEMORY-2026-10-16.md), [proxy](30-MICRO-VECTOR-INDEX-STATE-AND-QUALITY-2026-10-16.md) | Brute-force exact neighbor oracle vs ANN and source-law correctness |
| A14 Confidence + verifier learned requirements, language/domain identity, false gate | P1 translation/graph | [Verifier accumulation](23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE-2026-10-16.md), [N27](27-NEW-UNIT-HELDOUT-VERIFIER-TOKENIZER-METRIC-BIAS-2026-10-16.md), [N28](28-NEW-VERIFIER-ISSUE-ROLE-PARSER-AND-CONTEXT-LEAK-2026-10-16.md) | accepted correct / accepted wrong / abstention; preserve older lawful translations |
| A15 Replay selection, model parent/version, restore of units and nearest index | P1 continual learning | [N27 replay](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md), [state identity](22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md), [N33 HNSW RNG](33-NEW-HNSW-RNG-CONTINUATION-AND-COUPLING-2026-10-16.md) | old/new laws before, after train, after cold restore; same one ordinary consumer |
| A16 Dataset breadth, law/source-family split, heldout-only metric, target contamination | P0 evidence | [RP01 data validity](27-RP01-BATCH-STRATIFICATION-CONTRACT-GAP-2026-10-16.md), [E35/36 RNG](23-NEW-E35-E36-COUPLED-RNG-AND-FALSE-KNN-PASS-2026-10-16.md), [E44/E45](33-NEW-E44-E45-PAIR-TARGET-CONFOUND-2026-10-16.md) | Group-disjoint paired correct-law gold; same denominator + null/negative controls |
| **N35-CVG** Loss-only 2-window early stop, `maxSteps=5`, default window=1 | **P1 generated-capability DPO ONLY (new check)** | [N35 source audit](35-NEW-GENERATED-CAPABILITY-DPO-LOSS-PLATEAU-STOP-2026-10-16.md) `DefaultLogicalCapabilityTrainer → LogicalDpoTrainingUnit → DpoConvergenceMonitor` | [5-update paired micro](35-MICRO-DPO-CONVERGENCE-VS-DOMAIN-QUALITY-2026-10-16.md): `window=1` vs `window=5`, frozen answers; **NOT_RUN** |

**Не называть все строки новыми дефектами:** почти все — существующие доказательства других агентов; N35 — добавленный здесь отдельно ограниченный гипотетический learning-quality seam. У каждой строки возможны противоположные исходы. Устранение источниковых нарушений integrity — отдельная обязанность, но не доказательство semantic gain.

## 3. Исторические данные, которые важно НЕ потерять

| Источник / дата | Что реально записано | Граница интерпретации |
|---|---|---|
| [E07 Tier2 structural context / 01.05.2026](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/results/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-2026-05-01-002.json) | baseline `.1502` → enhanced `.1310`; FAILED | Его `Train` **не использовал** заявленные новые `AstParentType/FileId/ScopeDepth` в decision key; BPE ordinal↔AST char offsets нарушает корректный feature alignment. FAILED не опровергает AST-контекст. |
| [E09 Tier1/30.04, Tier2/01.05](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md) | Tier1 `.3368→.3472` при пороге PASS не принят; Tier2 `.2413→.3507` (+10.94 п.п.) | В Tier2 **baseline и KNN оценивались на разных подмножествах validation**, переназначение kind label может различаться по файлам. Верная feature story — candidate, не причинное доказательство. |
| [E14 Context→Action Tier2/Tier3](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) | Tier2 `.75→1.00` на **4 val**; Tier3 `.8824→.8676` на **68 val** | Контекстный nearest neighbor подходит не каждому источнику/объёму. Одно small PASS не лицензия на универсальный kNN. |
| [E26 Contextual Dictionary](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md) | `.40→1.00`, всего **15 preset contexts**, 6 hard | Демонстрация POS-context disambiguation, не доказанное обучение обобщающего нейронного словаря; нынешний Unit теряет часть left/right POS контекста. |
| [E43 Replay Buffer](34-RP01-GAP-AND-EVIDENCE-CROSSWALK-2026-10-16.md) | retention `.3226→.3978` в своём эксперименте | Положительное свидетельство replay *там*, не сохранения RP01 Student/selected Unit после cold restarts. |
| [DesignToCode V210 / repeat10](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md) | Настоящий `DpoTrainer.StepMany`, checkpoint, V210 134 пары/9 batches и train accuracy `.8333`; repeat10 иной сценарий с высокими train metrics | **Реальная тренировка была**, но ни один из этих процентов сам по себе не показывает independent semantic answer gain. Tokenizer, prefix/material, model version не равны RP01 B0 byte-BPE629. |
| [TranslationMachine translation-v1](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md) | `rows_trained=1`, `runtime_product_accuracy=0.3333`, есть non-exact nearest-source transfer; `passes_promotion_gate=false` | Это узкий transfer memory, не DPO LoRA Student generation и не доказательство product-ready качества. |
| [E13B historical heldout](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md) | 20 training items 100% matched, 10 unseen expressions: **0% matched, 100% abstained** | Безопасный отказ ≠ способность генерировать новый верный ответ. |

**Уточнение к запросу о матрице → KNN.** Объяснение вполне может состоять в разных **местах включения одного типа структуры**: исторический E07 не измерял обещанный набор AST features, E09 извлекал AST vector непосредственно из Roslyn и искал соседей. Однако буквальная цепь *тот же код/та же модель была перенесена и дала выигрыш* не подтверждена одним traced commit. Это две разные задачи и методики; нельзя объявить прежнюю отрицательную интерпретацию истиной без paired ablation.

## 4. Где feature улучшение может быть локальным, а не универсальным

| Domain panel | Релевантная структура | Когда не надо форсировать |
|---|---|---|
| C# design-to-code, Roslyn AST, build/unit completion | AST kind + parent + method scope + ordered operators/edges + identifiers; paired compile checks | В разговорном RU→EN AST C# не добавляет смысла |
| Natural translation RU↔EN | POS left/right, agreement/gender, negation scope, clause/role order; KNN source/target compatibility | Жёсткая AST вида C# и английский stopword-list не универсальны; имена/API должны сохраняться |
| Hebrew↔English / other scripts | native language POS/morphology, Unicode normalization, target script mix, clitic boundaries | Cyrillic-only/Latin-only gate EN_RU нельзя применять без новой языковой политики |
| Diagram/DSL/control flow | ordered edge/flow roles, legal transitions, graph path and source law, feedback provenance | Bag-of-words и POS русского не сохраняют направление стрелки/условия |
| Domain-independent generated capability | positive/negative learned law, actual recipient and cold consumer state, replay, valid early stop | `generated-capability` loss STOP не относится к `TranslationMachine` или RP01 Student без доказанной цепочки |

## 5. Исполнение без очередного месяца тестов

1. **Read-only E0:** выбрать **один настоящий пользовательский ошибочный запрос**, построить `raw source→admitted pair SHA→tokenized visible contrast→recipient id→mutable state→checkpoint hash→ordinary user caller→answer→independent law`. В отчёте обозначать `MISSING_LINK`, а не закрывать gap зелёным тестом.
2. **Neural P0:** использовать [RP01 tiny cold-start SFT/DPO](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md) + [N33 autograd](33-MICRO-AUTOGRAD-WARMUP-AND-LEARNING-IMPACT-2026-10-16.md). Сначала 8–32 примера/один закон и проверка *нового свободного ответа*. Если Student не способен породить полезный токен из warm base, добавление KNN не подтверждает neural repair.
3. **Unit/KNN P1:** один позитивный historical mechanism и одна текущая деградация на **source-family-disjoint** панели: C# — AST E09 и его одинаковая validation; RU/EN — E26 POS/контекст и обычный trained adapter; graph — edge-order. A/B меняет **одну** feature/eligibility функцию, сохраняет данные, law-labels, index memory.
4. **Generated-capability (только если actual caller):** [N35-MICRO-CVG-01](35-MICRO-DPO-CONVERGENCE-VS-DOMAIN-QUALITY-2026-10-16.md), `window1` vs `window5`, `max5`. STOP-only gain измерить отдельно от DPO/LoRA bug.
5. **E0→E4 decision packet:** `source commit → why signal can train → which state actually changed → blind heldout wins/losses → prior-law regressions → cold consumer reproducibility → resource cost → adopt/reject/no evidence`. Прекращать при невалидном эксперименте, не переписывать Gold под model outputs. Windows CI gate нужен позже для совместимости/поставки, а не для объяснения обучаемости.

### Уровни доказательств и решения

- **CONFIRMED BY SOURCE:** прочитан метод и его caller; ещё не значит «ошибка пользователя воспроизведена».
- **HISTORICAL SAVED RESULT:** опубликованные файлы с цифрами; непарные сравнения/fixture bias отдельно.
- **PROXY:** логически следует из методов, но native learner не вызывался.
- **CAUSAL QUALITY GAIN:** однофакторный fresh A/B через **тот же ordinary consumer**, frozen blind law panel, separate source-family split, cold restored same result, независимое суждение по смыслу и учёт регрессий.
- **CURRENT VERDICT:** RP01 neural useful gain **не подтверждён**; исторические kNN/POS/real DPO тренировочные результаты **сохранены как ограниченные улики**, новые N35 hypotheses **не исполнялись native**. Правдоподобие или красивый график train-loss не поднимать до доказанной модели.

**Правила происхождения DPO:** пользовательский исходный контракт в [original task](../../original%20task.md) предусматривает открытые разрешённые модели для генерации финальных учебных пар; фактические model id, лицензия, service terms, provider, request id обязательны. При новом корпусе обе `chosen`/`rejected` стороны создаёт разрешённая внешняя open model, CODEX/Astra 6 UltraCode выполняет human case selection и независимую проверку. Никакие new teacher API calls и third-party judge в этом аудите не осуществлялись.

**Неприкосновенно:** `lubochka/llm_mvp_core@codex/dpo-implementation/docs/plans/replanning-possible-diffs/**`; core product code/old experimental receipts/старые фиксированные RP01 оценки не менялись. **Review verdict:** `DRAFT_PENDING_INDEPENDENT_SOURCE_AND_NATIVE_REVIEW`.
