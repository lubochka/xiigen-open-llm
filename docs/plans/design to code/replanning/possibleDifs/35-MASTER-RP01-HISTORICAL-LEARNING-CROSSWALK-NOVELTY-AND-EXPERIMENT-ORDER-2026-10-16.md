# N35 MASTER — RP01 × доказанное историческое обучение × получатель × новые гипотезы

**Дата:** 16.10.2026. **Проверенные ветки:** llm_mvp_core/codex/dpo-implementation@216a1f3d6af3f40ceef081dc168b07e4ce3934f9; соседние language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9 и DesignToCode@8f20ceb8a9bfbadab2ad309226c47dd76e714548 (выборочно на конкретных файлах); xiigen-planning/llm_plan3. **Статус: SOURCE_AUDIT_AND_HISTORICAL_RECEIPT_RECONCILIATION + NEW_N35_SOURCE_HYPOTHESES + EXECUTED_SYNTHETIC_SHADOW; NATIVE_DOTNET_STUDENT_LEARNING_NOT_RUN; independent CODEX/Astra review NOT_RUN.**

**Запрет соблюдён:** в llm_mvp_core/docs/plans/replanning-possible-diffs НЕ производились изменения. Этот документ и дочерние материалы добавлены исключительно в xiigen-planning/replanning/possibleDifs; отчёты других агентов/замороженные результаты не переписаны.

## 1. Ответ человеку: сначала доказать, какого именно ученика сравниваем

Сегодня под общим словом «обучение» объединены несовместимые по метрикам механизмы:

| Что меняется от одного урока | Кто фактически читает выученное | Что можно объявлять успехом |
| --- | --- | --- |
| **Neural DPO + LoRA q/v**: adapter tensors на случайной B0 | Student TextGenerator, autoregressive free response | Независимый правильный *семантический* ответ в том же consumer после restore; изменение весов/логитов недостаточно |
| **KNN / StructuralKnn E09**: corpus (AST vector → следующий вид AST) | KNN predictor; дальнейший user-facing consumer требует caller-trace | Рост correctness@1 на *той же* unseen AST-панели, без утечки файлов |
| **TranslationExecutor Unit**: exact-source, phrase, word maps и KNN pairs | TrainedTranslationAdapter → Executor → Verifier → bootstrap либо translation | Верный перевод нового источника после cold restore, зафиксированный маршрут exact/phrase/word/KNN |
| **Embedding encoder + vector index**: hidden embeddings/HNSW, возможно metric learning | EmbeddingModel→ANN→KNN ingredient Unit | Верно найденный и принятый law в обычном вызове, индекс пересчитан под версию encoder |
| **Graph/Flow/Deterministic checks**: routing, safety law, fallback | ProductHost / Logical Machine / Flow | Evidence честно помечено RULE_GAIN или ROUTING_GAIN; не называть изменением обученных neural weights |

**Замороженное RP01 наблюдение:** [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) и [causal pack](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md): C#500 — 25 смысловых шаблонов ×20, 50 updates, LoRA changed, свободная генерация 0/3→0/3; EN→RU500 — 20 категорий ×25, ещё 50 updates, LoRA changed, 0/4→0/4. B0 случайная, не прошла предварительное языковое обучение. Нельзя превращать эти результаты задним числом в результат работы KNN или назвать DPO математически сломанной без finite-difference proof.

## 2. Три новые различающие гипотезы этого прохода

| ID | Source-confirmed seam | Сфера и статус | Контраст за один маленький запуск |
| --- | --- | --- | --- |
| **N35-A** | [E09 StructuralKnn](35-NEW-E09-SIBLING-FEATURE-INVISIBLE-IN-STRUCTURAL-KNN-2026-10-16.md): **SiblingCount** сохраняется в structural vector, но не участвует в DistanceTo; FileId также игнорируется, однако file-invariance может быть полезна | C#/DesignToCode AST KNN, **P1 conditional**; не RP01 neural | Та же память/k и 8–16 C# unseen source contexts, DistanceTo original vs only sibling penalty; оценка gold next-kind и регрессий |
| **N35-B** | [StratifiedSplit singleton](35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md): единственный пример класса всегда train, нулевой val; E44 при secVal<=1 подменяет security metric общей | E40/E44 и редкие законы SQL/security/language, **P1 для доверия к eval** | Native group counts 1/2/3/5, далее frozen consumer на добавленной независимой sealed rare-law панели; отдельный matched one-extra-lesson experiment |
| **N35-C** | [Sentence embedding](35-NEW-EMBEDDING-POOL-NORMALIZE-TRAINABILITY-CUT-2026-10-16.md): Pool, L2Normalize и float[] EmbedBatch **трижды отсоединяют gradient**; даже ремонт Embedding.Forward не создаст обучаемый metric-learning API | Conditional P2 для намеренного обучаемого vector encoder; current inference float[] сам по себе корректен | Tiny Pool gradient expected 0.5/token, L2 norm derivative 0.128/-0.096, потом frozen heldout contrastive retrieval with versioned ANN |

[EXECUTED JS SHADOW receipt — только арифметика формул, не .NET](35-MICRO-SOURCE-SHADOW-RECEIPT-2026-10-16.json): для N35-A при изменении только SiblingCount distance=0→0 (original) и 0/0.3 (тестовый penalty); для N35-B rare singleton val=0; для N35-C finite-difference 0.128/-0.096. Это **не подтверждение улучшения обучения** и не заменяет нативный эксперимент.

**Ограничение новизны:** проверены существующие N16/N26/N30/N32/N33/N34 отчёты и соответствующие исходники; нельзя утверждать, что во всех непроиндексированных документах мира таких наблюдений никогда не было. N35 отличается точным source seam / условием входа / фальсификатором, а не громким названием.

## 3. Crosswalk всех главных осей возможной потери качества — без переписывания чужих исследований

| Ось | Имеющийся evidence / статус | Проверяемое несоответствие и применимость |
| --- | --- | --- |
| P0-01 **Стартовая способность Student** | [RP01 learning recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) | Случайная B0 + preference-only DPO не обязана породить словарь/декодирование. 8–32 chosen-only tiny SFT vs DPO-only; не 500 ещё раз |
| P0-02 **Автор и независимое качество DPO** | [DPO evidence](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [same-prompt admission](32-NEW-RP01-SAME-PROMPT-CONTRAST-ADMISSION-2026-10-16.md) | Реальный teacher generation обеих сторон, causal rejected (не просто стиль), CODEX human choice + independent CODEX grading; источник/provenance/license verified |
| P0-03 **DPO objective, response mask, reference** | [DPO pair gradient](03-DPO-PAIR-AND-GRADIENT.md), [exact-gradient core source-only](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/41-current-dpo-gradient-chain-source-review.md) | Tiny directional numerical derivative; q/v trainability; shared prefix, unequal response length, chosen/rejected length bias; не объявлять баг до falsifier |
| P0-04 **LoRA capacity/gradient/LR** | [LoRA and vector](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [N33 autograd](33-NEW-AUTOGRAD-EMBEDDING-BIAS-AND-SUPERVISED-WARMUP-GAPS-2026-10-16.md) | rank/alpha/target_modules/scheduler/grad clip; frozen B0; отдельная ошибка SFT warmup LR=0, но она не путь RP01 DPO |
| P0-05 **BPE/token ID/stop/cache** | [BPE drift](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md), [tokenizer evaluation](23-MICRO-TOKENIZER-LEARNING-QUALITY-PILOT.md), [inference stop](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md) | Save→Load tokenizer hash, response span, EOS/pad, OOV, language morphology, truncation; обычный free-answer вместо teacher-forced метрики |
| P0-06 **DPO → subunits: не потерять закон** | [translation DPO bridge](16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE-2026-10-16.md), [structured Units](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md), [unit identity](23-NEW-UNIT-CASE-IDENTITY-AND-NONATOMIC-TRAINING-2026-10-16.md) | chosen/rejected должны сохраняться как корректная per-unit задача; проверить SourceId, UnitId, semantic signature и фактический consumption; 3 subunits не равны 3 независимым законам |
| P0-07 **Фактический путь обученного ответа** | [ProductHost bypass](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md), [logical Units/Flows](06-UNITS-FLOWS-GRAPH.md) | Graph/route может выбирать deterministic bootstrap или другой Unit; измерить final response ID и provenance до/после |
| P1-08 **KNN exact→prefix→structure reachability** | [ordinary KNN direction gating](30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md) | Обученные pairs записаны, но ordinary source без распознанного concept может вообще не обращаться к KNN в части направлений |
| P1-09 **KNN recency + alias duplicate** | [FIFO old correction](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md), [N34 same-key insertion](34-NEW-ONE-LESSON-DOUBLE-INSERT-UNDER-ALIAS-SIGNATURE-2026-10-16.md) | Новое preferred correction недоступно при Take(k); одно занятие может занять два слота; false confidence/coverage |
| P1-10 **KNN scope и отрицательный перенос** | [direction-shared store](30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md), [topic loss](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md) | Раздельные домены, языковые направления, environment/version и target script; не смешивать SQL и prose словарь |
| P1-11 **Векторные признаки и ANN** | [encoder-index drift](16-LIVE-ENCODER-INDEX-VERSION-DRIFT.md), [nonfinite ingress](30-NEW-VECTOR-ALIAS-AND-NONFINITE-KNN-INGRESS-2026-10-16.md), [N35-C](35-NEW-EMBEDDING-POOL-NORMALIZE-TRAINABILITY-CUT-2026-10-16.md) | Обновлённый encoder со старым индексом, hashed BPE bags теряют порядок, pool cannot backprop, ANN recall vs exact |
| P1-12 **AST и порядок токенов** | [E07 misalignment](30-NEW-E05-E07-STRUCTURAL-EXPERIMENT-VALIDITY-2026-10-16.md), [E09 move](26-NEW-E09-GIT-PRODUCTION-TARGET-AND-TIE-DRIFT-2026-10-16.md), [N35-A](35-NEW-E09-SIBLING-FEATURE-INVISIBLE-IN-STRUCTURAL-KNN-2026-10-16.md) | Token index≠char span; E07 tiers 2–5 только названия; E09 label codec и tie changed; sibling count не влияет на distance |
| P1-13 **Синтаксические роли, отрицание, оператор** | [N29 first-token mapping](29-NEW-FIRST-TOKEN-PSEUDOALIGNMENT-ORDER-CONTAMINATION-2026-10-16.md), [N30 phrase/contract](30-INDEPENDENT-NOVEL-COMPOSITION-CONTRACTION-AND-OPERATOR-GAPS-2026-10-16.md) | Word order, субъект/объект, negation scope, SQL NULL, C# operator; per-domain parser и verifier, не глобальный lexical Jaccard |
| P1-14 **Отличать правила от обученных весов** | [law contrast RP01](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [hidden deterministic Units](05-UNITS-FLOW-GRAPH-DETERMINISM.md) | Rule precision improvement может сопровождаться падением coverage/abstain; отдельно neural, memory, routing, guard |
| P1-15 **Checkpoint + optimizer/ANN state** | [checkpoint adoption](06-CHECKPOINT-RESTORE-ADOPTION.md), [HNSW RNG](33-NEW-HNSW-RNG-CONTINUATION-AND-COUPLING-2026-10-16.md), [cache alias](27-NEW-MUTABLE-TRANSLATION-CHECKPOINT-CACHE-ALIAS-2026-10-16.md) | Save/load parity weights, optimizer moments, training step/cursors, tokenizer SHA, KNN and routing/teacher journal; cold ordinary consumer |
| P1-16 **Evaluation split/replay/statistics** | [E51 oracle split](11-E51-KNN-ORACLE-SPLIT-AND-POSITIVE-MECHANISMS-2026-10-16.md), [E44/E45 confound](33-NEW-E44-E45-PAIR-TARGET-CONFOUND-2026-10-16.md), [N35-B](35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md) | Нельзя приравнивать “PASS” на leaked oracle/unequal validation panels к generalization; rare classes не попадут в val |
| P2-17 **Corpus index/graph cross-model composition** | [KNN hybrid retrieval](04-KNN-HYBRID-RETRIEVAL.md), [BM25 fusion nonlocal](20-NEW-GAP-NONLOCAL-FUSION-AND-BM25-SCALING-2026-10-16.md) | Добавление третьего урока меняет rank первых двух; retrieval first-hit не равен law-conditioned rerank; Graph/Flow must consume learned answer |

**Не следует** из таблицы, что каждая ось вызвала RP01 0/3 или что дефект наблюдался в каждом домене. Сила доказательств смешанная: source-confirmed code paths, исторические receipts, proxy и hypotheses. Детальные первоисточники сохранены в указанных документах.

## 4. Восстановление успешных и «неудачных» экспериментов, без ошибочного переноса выводов

| Исторический механизм | Данные и честная интерпретация | Целевой перенос |
| --- | --- | --- |
| **E07 structural ngram** | Архивные T1 0.1531→0.1462, T2 0.1502→0.1310, отрицательные результаты при неверном BPE→AST alignment и tier no-op [N30 source](30-NEW-E05-E07-STRUCTURAL-EXPERIMENT-VALIDITY-2026-10-16.md) | Не объявлять AST плохой идеей. Честно исправить alignment; оценить на equal validation. |
| **E09 AST KNN** | Архивный 0.2413→0.3507, однако denominator/panel baseline vs enhanced различались; [N26 подтверждён коммит переноса E09→production 27ae0326](26-NEW-E09-GIT-PRODUCTION-TARGET-AND-TIE-DRIFT-2026-10-16.md) | Сохранить реальные structural features, проверять N35-A, label codec, true file boundary, k. **Не** выдавать Git-коммит E09 за доказательство конкретного “failed matrix donor”. |
| **E14 ContextAction KNN** | T2 0.75→1.00 на 4 val, T3 0.8824→0.8676 на 68 val [matrix/KNN audit](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) | Перенос по размеру корпуса/labels неоднородный; law/distribution-aware scoring, macro label accuracy. |
| **E26 contextual word dictionary** | Положительные локальные RU_EN контексты (сообщается 0.40→1.00 на 15), но positional pseudoalignment и word-map overwrites допускают отрицательный перенос [N29](29-NEW-FIRST-TOKEN-PSEUDOALIGNMENT-ORDER-CONTAMINATION-2026-10-16.md) | Проверить parser role + word order для RU/EN; не распространять на HE/SQL без соответствующего codec. |
| **E44/E45 neural weighting/curriculum** | Неудачные архивные эксперименты содержат *две меняющиеся переменные*: baseline/shadow учат разные пары вход→цель [N33 E44/E45](33-NEW-E44-E45-PAIR-TARGET-CONFOUND-2026-10-16.md) | Не отвергать weighting/curriculum целиком. Сначала одинаковые переходы, затем менять один фактор. Редкие labels N35-B. |
| **Матрица→KNN** | [N12 provenance](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md) и [N12 misreads](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md): минимум три “matrix” значения — ILearningUnit, ngram E07 и вычислительный E12. Конкретный failed-matrix donor→successful KNN function commit **не найден** | Вести отдельный OPEN_PROVENANCE item: искать SHA пары diff, экспериментальные JSON, алгоритмический смысл, не восстанавливать историю догадкой. |

Проверяющий обязан искать реальные receipt/commit не только на текущем codex/dpo-implementation, но и в DesignToCode/language_translate и иных ветках через git history. Для каждой подтверждённой «удачи» хранить точную train/val panel, scorer, domain, sample count, checkpoint, consumer и retest under heldout.

## 5. Приоритетная короткая программа вместо ещё одной кампании 500 DPO

| Order | Эксперимент | Что менять | Условия продолжения |
| --- | --- | --- | --- |
| **0 / READ-ONLY** | Реконструкция актуального caller graph Student, Translation Unit, KNN, ProductHost, checkpoint; evidence links и SHA | Ничего | Есть конкретный train→persist→cold consumer→answer path; иначе классифицировать НЕАКТИВНО, не чинить вслепую |
| **1 / NEURAL** | 8–32 honest pairs: exact DPO gradient finite difference, prompt/response masks, LoRA weight/logit delta vs free generation; warmed SFT vs DPO-only | Одна переменная на contrast; same tokenizer B0/seed/steps | Student учится на train через teacher-forced NLL и затем ordinary output; если нет — STOP, не масштабировать на 500 |
| **2 / DPO QUALITY** | 4–8 semantic law families C# / ENRU / SQL, pairs authored by externally allowed open Chinese model; CODEX/Astra independent grading | only quality of chosen/rejected or independent family (fixed training budget) | Good/bad actually differ in truth, not form; provenance & license receipts |
| **3 / AST-KNN** | [N35-A](35-NEW-E09-SIBLING-FEATURE-INVISIBLE-IN-STRUCTURAL-KNN-2026-10-16.md): synthetic discriminating vector, затем real Roslyn withheld nodes | DistanceTo uses sibling count only | Same-source feature matters on unseen C# laws; no regression without sibling effect |
| **4 / EVAL-COVERAGE** | [N35-B](35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md): rare labels 1/2/3/5 and sealed heldout | First diagnostic only; then new independent train lesson | Accuracy without rare laws marked UNKNOWN; compare paired answers on same sealed cases |
| **5 / VECTOR TRAINING** | [N35-C](35-NEW-EMBEDDING-POOL-NORMALIZE-TRAINABILITY-CUT-2026-10-16.md): mean + normalize finite difference, then metric-learning pair with rebuilt ANN | A=stop-gradient, B=differentiable path; same learner model + corpus | Native gradient and *semantic recall* improve, no old-law losses |
| **6 / COLD PARITY** | Save/reopen and replay of all trainable state; check reply from actual ordinary request | cold restart only | Same tokenizer/encoder/index identities; no hidden bootstrap fallbacks |

**Minimal domain coverage:** C#: null, exception, operator, argument/AST role; EN→RU: negation, pronoun/coreference, word order, morphology; SQL: IS NULL, = NULL in code vs explanation, dialect-specific syntax; HE/ES language directions only where there is real tokenizer/domain data. Не применять C# AST штраф к языковому KNN и не применять стоп-слова English к SQL operators.

**Метрики:** before/after paired correctness by law, critical errors, over-refusal, partial/abstain, sample & family diversity, train accuracy vs independently heldout accuracy, token losses, consumed UnitId and adapter ID, actual checkpoint reopen, wall/CPU/RAM, complete native log SHA. Не считать улучшением одно изменившееся deterministic rule, веса без семантического gain или перекрытие неверных ответов abstain без учёта coverage.

## 6. Фиксированный контракт ролей, экспертизы и доказательств

1. **Human selector = CODEX с моделью Astra 6 UltraCode**: выбирает конкретный lesson, domain/law, input и допустимый independent heldout. Не подменять человеческую роль реальным пользователем или детерминистическим сортировщиком.
2. **Автор final chosen И final rejected = внешняя разрешённая открытая китайская модель.** Зафиксировать конкретные provider/model/version/license/terms and request IDs; нельзя выдавать ASTRA_SYNTHETIC_DEV или ручную детерминистическую пару за её ответ.
3. **Independent DPO pair quality judge = CODEX с Astra 6 UltraCode**: проверка каждой стороны, фактического контраста, свободы от leakage, смысловой truth и доменного правила **до train**.
4. **Independent actual learned-output judge = CODEX с Astra 6 UltraCode**, blind к arm/epoch; не оценивать только собственным deterministic evaluator или той же китайской моделью, написавшей пары.
5. **Architect/plan/review/code/prompt/human communication:** применены [проектные skills](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills) для раздельных hypotheses, explicit caller/owner, minimal same-panel experiments и review acceptance. У данного исследования нет отметки об исполненном автономном Astra review; этот контракт адресован следующему исполнителю.

**Минимальный evidence record** каждого следующего эксперимента: experiment_id; hypothesis_id; core SHA; planning SHA; branch; domain; law_id; source_family/train_or_holdout; selector_actor/version; external teacher model/provider/license; chosen_sha; rejected_sha; DPO judge version/verdict; recipient UnitId/adapterKey/modelId; tokenizer/embedding/index version; optimizer/scheduler/seed; checkpoint_id; exact consumer route; native stdout; frozen scoring rubric; per-case before/after; independent judge verdict; errors/fallbacks; elapsed_ms; status: SOURCE_ONLY | JS_PROXY | NATIVE_MECHANISM_PASS | HELDOUT_GAIN | COLD_CONSUMER_GAIN | FAIL/BLOCKED.

**Не подменять неизвестное:** текущий DOC даёт доказательства конкретных алгоритмических различий и точные falsifiers, но не заявляет, что все ранние ветки целиком прочитаны, что N35 fixes уже внедрены, либо что RP01 root cause установлен. Первые настоящие learning verdict появятся только после нативных paired runs и CODEX/Astra проверки.

## 7. Навигация в RAG и источники

- [Исходный «Алгоритм создания ДПО»](../../Алгоритм%20создания%20ДПО.md): модель ILearningMachine, many-to-many Unit, параллельные combinations и человеко-понятный план.
- [WorkLeft MASTER RAG](../../09.09.26WorkLeft-RAG-MASTER-INDEX.md): checkpoints, student identity, teacher authorship, journal exactly-once и evidence vs status.
- [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md).
- [Историческая positive/negative lineage](26-INDEPENDENT-FULL-GAP-REGISTER-AND-HISTORICAL-LEARNING-LINEAGE-2026-10-16.md).
- [Общий N33 реестр качества](33-MASTER-CROSSWALK-QUALITY-LOSS-AND-NOVEL-N33-2026-10-16.md).
- [N35 исходная новая AST гипотеза](35-NEW-E09-SIBLING-FEATURE-INVISIBLE-IN-STRUCTURAL-KNN-2026-10-16.md).
- [N35 редкая validation](35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md).
- [N35 gradient-ready embedding](35-NEW-EMBEDDING-POOL-NORMALIZE-TRAINABILITY-CUT-2026-10-16.md).
- [N35 выполненный локальный JS proxy с чётким уровнем доказательств](35-MICRO-SOURCE-SHADOW-RECEIPT-2026-10-16.json).
