# N38 — RP01: карта расхождений, успешно обучавшихся соседних механизмов и новых условных разрывов

**16 октября 2026 | INDEPENDENT ADDENDUM TO EXISTING possibleDifs | DOCUMENTARY AUDIT, NOT SCIENTIFIC CLOSURE.**

## Зачем этот документ

RP01 не подтвердил улучшение свободного ответа после настоящего изменения LoRA; но исторические E09/E14/E22 показывали, что **другие, неравноценные получатели обучения** действительно могут выигрывать в отдельных задачах. Нужно локализовать место, где содержательный урок перестаёт влиять на **обычный пользовательский ответ**, не объявляя сотни существующих отдельных заметок «новыми открытиями».

**Точки фиксации:**
- planning: xiigen-planning, llm_plan3, design to code session/replanning/possibleDifs; прежний пакет [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md).
- core: read-only llm_mvp_core, codex/dpo-implementation @ [aa284ba2](https://github.com/lubochka/llm_mvp_core/tree/aa284ba21790c5d7601328997cd6174c85cee878). Документы docs/plans/replanning-possible-diffs читались **без записи**.
- ранее существовавшие карты [N35](35-MASTER-RP01-DIVERGENCE-HISTORICAL-SUCCESSES-AND-CAUSAL-ORDER-2026-10-16.md), [N37](37-RP01-MASTER-CROSSWALK-POSITIVE-HISTORY-NOVELTY-AND-FAST-DECISIONS-2026-10-16.md) и [core 74](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/74-helper-master-reconciliation-and-research-priorities.md) остаются канонической предыдущей работой. Нынешний N38 добавляет только два узких candidate seams и приоритеты.

**Четыре разных recipient:**
1. Neural Student: DPO → signed gradient → AdamW → q/v LoRA → Student checkpoint → TextGenerator.
2. Structured TranslationMachine: admitted translated lesson → SemanticTokenizerUnit / ExecutorUnit / QualityVerifierUnit → KNN, phrases, word maps → TrainedTranslationAdapter → ordinary translation.
3. Learned routing/memory: E09/E14/E22, HNSW, semantic compiler, routing-profile; результат KNN label **не обязан менять веса Student**.
4. Deterministic laws/graph/policy: полезный stop/abstain/guard **не является новым нейронным знанием**.

## 1. Наблюдаемая регрессия RP01: не подменять метрику

По [RP01 Recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md): C#500 (25 families×20, 50 updates, 16 LoRA tensors изменились, free correctness 0/3→0/3); EN→RU500 (20 families×25, ещё 50 updates, free correctness 0/4→0/4); C# retention 0/3→0/3. Входная B0 — случайная, 629 BPE tokens, hidden512, 4 layers. Не доказано, что B0 вообще умела формировать фразу до DPO. **Мутация весов доказана отчётами; улучшение пользовательского навыка — нет**. Размер независимых панелей очень мал, поэтому нельзя утверждать «ни в одном домене обучение не работает».

Нынешний снимок документов **не заменяет** проверку реальных private checkpoint/logits/dataset SHA в native среде; новых .NET student calls, китайского teacher или независимого CODEX/Astra judge в N38 НЕ БЫЛО.

## 2. История: успех не равен универсальному преимуществу KNN

| Эксперимент/ветка | Первичный источник | Исторический результат и реальная граница |
| --- | --- | --- |
| E09 Structural KNN, DesignToCode Tier1 | [run 2026-04-30](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/experiments-v2/E09-STRUCTURAL-KNN/results/run-2026-04-30-001.md) | 0.3368→0.3472, +3.1% относительного прироста; **FAILED** только порог 5%, а не «KNN ничего не дал». Ограниченный tokenizer-domain, требует matched rerun |
| E09 Structural KNN, Tier2 | [run 2026-05-01](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) | 0.2413→0.3507 PASSED, но [N37 causal review](37-RP01-CAUSAL-DELTA-HISTORICAL-E22-AND-RECIPIENT-PRIORITY-2026-10-16.md) документирует несопоставимый baseline/validation slice; не переносить +45% на product |
| E14 Context-Action KNN, маленький опыт | [run 2026-05-01-001](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-05-01-001.json) | 0.75→1.00 на **4 validation files**, действие на структурном C# code context |
| E14 Context-Action KNN, крупнее | [run 2026-05-01-002](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-05-01-002.json) | 0.8824→0.8676, хуже; редкие REFACTOR_EXTRACT/ADD_LOOP, imbalance; доменные фичи без стратификации могут вредить |
| E22 Dataset Router | [run 2026-05-07](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json) | macro-F1 0.4709→0.5646, train360/val90 PASSED. Но per-dataset: Generic=1.00, CSharp=.99, BugFix=.82, Orchestration=.57, **TypeScript=.00, Tests=.00**. Не переносить router из сильных классов в слабые |
| Языковые соседние ветки | [language_translate](https://github.com/lubochka/llm_mvp_core/tree/language_translate), [DesignToCode](https://github.com/lubochka/llm_mvp_core/tree/DesignToCode), [external_domains](https://github.com/lubochka/llm_mvp_core/tree/external_domains) | Данные/реализации следует сверять по исходным SHAs, type of learned state, ordinary consumer и heldout; наличие PASS в другом recipient не означает Student free answer PASS |

**Проверять не статус эксперимента, а механизм, sign of gain, знаменатель, train/validation leakage, right domain, learned state reached consumer.** Пользовательское историческое наблюдение о неправильно интерпретированных «неудачах» как матричных и удачном переносе в KNN — рабочее исследовательское основание, но без конкретной source→implementation→native outcome цепочки нельзя каждый такой перенос объявлять доказанным.

## 3. Карта возможных пробелов: причина → существующий owner → решающий опыт

**C = code condition verified in prior docs; H = causal hypothesis; Q = product-quality gain not measured here.** Строки уже исследованных агентов не считать N38 «новыми».

| ID | Что могло быть утрачено | Existing evidence / owner | Где проверять (domain / recipient) |
| --- | --- | --- | --- |
| R01 | Random B0 не умеет язык до DPO; относительный objective не создаёт baseline competence | [RP01](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [N33 warmup](33-NEW-AUTOGRAD-EMBEDDING-BIAS-AND-SUPERVISED-WARMUP-GAPS-2026-10-16.md) | P0 Neural C#, ENRU: tiny SFT warmup vs DPO-only, new heldout |
| R02 | DPO author/provenance: simulation вместо допущенного external teacher, слабые rejected | [N35](35-MASTER-RP01-DIVERGENCE-HISTORICAL-SUCCESSES-AND-CAUSAL-ORDER-2026-10-16.md), [RP01](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) | P0 Neural: source lineage и CODEX judgment, а не 500 счетчик |
| R03 | Дубли 25/20 families, family contamination и shallow shortcut | [N27 prefix](27-RP01-DPO-COMMON-PREFIX-GENERATION-GAP-2026-10-16.md), [N32 same prompt](32-NEW-RP01-SAME-PROMPT-CONTRAST-ADMISSION-2026-10-16.md) | C#, ENRU: family-disjoint gold |
| R04 | Wrong DPO contrast, same prompt, response mask, sign/beta/reference, autograd unreachable | [DPO gradient](03-DPO-PAIR-AND-GRADIENT.md), [core 41](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/41-current-dpo-gradient-chain-source-review.md) | Neural 8–32 one-step finite difference, source-bound |
| R05 | TeachingVector semantic signs/coords collapse to active-slot count | [core 30](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/30-teaching-vector-adamw-and-current-recipient.md) | StepVector only, signed/permute pair controls; not generic KNN |
| R06 | LoRA target q/v, low rank, frozen embedding/head/FFN; cold start expressivity | [N33](33-NEW-AUTOGRAD-EMBEDDING-BIAS-AND-SUPERVISED-WARMUP-GAPS-2026-10-16.md), [N35](35-MASTER-RP01-DIVERGENCE-HISTORICAL-SUCCESSES-AND-CAUSAL-ORDER-2026-10-16.md) | Neural compare one axis per arm |
| R07 | BPE canonical-vs-legacy, hashed tokens, truncation and semantic collision | [N16 bridge](16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE-2026-10-16.md), [N21](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md) | Translation alternative bridge, NOT direct RP01 exact-v2 |
| R08 | Decoder prompt/prefix/EOS/length: learned conditional answer not freely reachable | [N27 prefix](27-RP01-DPO-COMMON-PREFIX-GENERATION-GAP-2026-10-16.md), [N10 stop](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md) | Neural C#/ENRU, paired fixed model inference |
| R09 | Checkpoint parent, tokenizer, optimizer/RNG, actual serving identity, cold load | [core 19](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/19-unit-state-identity-and-partial-learning.md), [N27 cache](27-NEW-MUTABLE-TRANSLATION-CHECKPOINT-CACHE-ALIAS-2026-10-16.md) | All; process restart, logit/answer parity |
| R10 | DPO → typed Unit loses source roles, pair contrast, rejected law, mapping multiplicity | [N21 structured](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md), [N27 role](27-NEW-ROLE-BLIND-MORPHOLOGY-AND-QUOTE-NEGATIVE-COLLAPSE-2026-10-16.md) | ENRU/RUEN, verbs/gender, code AST |
| R11 | Wrong UnitId silently skipped, pretend accuracy, missing owner from ordinary Flow | [N17](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md), [N21](21-INDEPENDENT-CLAUSE-DROPOUT-WHITESPACE-AND-LEGACY-SCAFFOLD-2026-10-16.md) | Structured Unit, one-law receipt |
| R12 | E09/KNN signature collisions, broad structural fallback, label/file-kind variance | [N35 E09](35-NEW-E09-SIBLING-FEATURE-INVISIBLE-IN-STRUCTURAL-KNN-2026-10-16.md), [N37 KNN](37-NEW-KNN-SIGNATURE-GUARD-MONOTONICITY-CONTAINMENT-AND-CASCADE-2026-10-16.md) | C# code AST vs plain sentences, domain gating |
| R13 | E14 action-label imbalance, incomplete rare classes | [E14 large source](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-05-01-002.json), [N35 rare class](35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md) | C# refactor, Tests, action classification |
| R14 | E22 learned router useful only some domains; zero TypeScript/Tests | [E22 run](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json), [N37 pilot](37-MICRO-E22-ONE-LESSON-KNN-CAUSAL-PILOT-2026-10-16.md) | Domain router: separate per-category confusion/abstain |
| R15 | Word order and syntax structure: learned phrase/POS vs bag of words; reordering not universally useful | [N30 phrase](30-INDEPENDENT-NOVEL-COMPOSITION-CONTRACTION-AND-OPERATOR-GAPS-2026-10-16.md), [N27 role](27-NEW-ROLE-BLIND-MORPHOLOGY-AND-QUOTE-NEGATIVE-COLLAPSE-2026-10-16.md) | ENRU/HEEN/SQL/AST; DO NOT force same feature on numeric domains |
| R16 | Learned phrase/word map drops unseen modifiers/negation and preempts better KNN | [N21 phrase](21-INDEPENDENT-CLAUSE-DROPOUT-WHITESPACE-AND-LEGACY-SCAFFOLD-2026-10-16.md), [N35 negation confidence](35-NEW-NEGATION-OMISSION-WORDMAP-TO-099-CONFIDENCE-2026-10-16.md) | Translation/code critical not/no/never and operators |
| R17 | Verifier learned AND conflicts, wrong src/context parse, lexical polarity blind and false high confidence | [N23](23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE-2026-10-16.md), [N28](28-NEW-VERIFIER-ISSUE-ROLE-PARSER-AND-CONTEXT-LEAK-2026-10-16.md) | Structured Translation; correct/wrong/abstain separately |
| R18 | Unit checkpoint alias/partial training, reused incorrect state, idempotence/noop | [N23 identity](23-MICRO-UNIT-IDENTITY-AND-ATOMICITY-2026-10-16.md), [N37 alias](37-NEW-KNN-UNIT-MUTABLE-OUTPUT-ALIAS-2026-10-16.md) | Cold restore, repeated law + genuinely new law |
| R19 | HNSW random state/neighbor prunes, memory reachability vs collection count | [N28](28-NEW-HNSW-REVERSE-PRUNE-AND-LOST-LESSON-REACHABILITY-2026-10-16.md), [N33](33-NEW-HNSW-RNG-CONTINUATION-AND-COUPLING-2026-10-16.md) | Large memory, high-dimensional domain, after restore |
| R20 | GraphRAG seed/metadata truncation and fallback overshadow learned retrieval | [N35](35-NEW-GRAPHRAG-EMBEDDING-PREFIX-METADATA-AND-HIDDEN-FALLBACK-2026-10-16.md), [N36](36-NEW-GRAPHRAG-SEEDLESS-PAGERANK-FALSE-EVIDENCE-2026-10-16.md) | Graph/Knowledge domains, not direct language Student |
| R21 | Feedback to wrong recipient, training history counted despite no live effect | [N27 replay](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md), [core 75](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/75-translation-feedback-learning-consumer.md) | Continual feedback; before/after live consumer |
| R22 | Evaluation biased to negative-only verifier, synthetic labels and family leak | [N27](27-NEW-UNIT-HELDOUT-VERIFIER-TOKENIZER-METRIC-BIAS-2026-10-16.md), [N23](23-MICRO-VERIFIER-CAUSAL-LEARNING-EXPERIMENT-2026-10-16.md) | All: balanced blind gold with abstention |
| **N38-A (NEW)** | Legacy fractional mask: forward weights by maskVal, backward weights by 1 per nonzero position | [new source audit](38-NEW-LEGACY-FRACTIONAL-RESPONSE-MASK-GRADIENT-MISMATCH-2026-10-16.md), [numerical receipt](38-MICRO-DPO-MASK-PROXY-RESULT.json) | Legacy-only / prospective weighted token DPO; exact RP01 protected |
| **N38-B (NEW)** | Generic StepMany can mix legacy mean and exact sum objective under one update without homogeneous gate | [new source audit](38-NEW-MIXED-EXACT-LEGACY-DPO-OBJECTIVE-IN-SAME-STEP-2026-10-16.md) | Generic permitted training only; current project-LoRA RP01 execution profile rejects legacy |

**Итого:** 22 прежних категорий — **ссылки на владельцев**; 2 новых условных code seams N38. Таблица не доказывает, что любой из 24 пунктов вызывает нынешние 0/3 или 0/4. Особо важно: **N38-A/B не исправлять прежде P0 B0, actual curriculum, reachability и native training evidence**.

## 4. Функции по доменам: не строить одну «лучшую KNN» на всё

| Домен / задача | Возможные полезные признаки (только candidate) | Избегать без доказательств |
| --- | --- | --- |
| C#/DesignToCode | AST node parent/child, method signature, call graph, action labels, declaration order, operator polarity, framework version; E14 context action | считать случайные lexical overlaps подтверждением корректного refactor; копировать E09 tokenizer gain как C# law gain |
| EN↔RU/HE↔EN | language direction, lemma/POS, syntactic role, local phrase order, gender agreement, negation/quantifier, source span coverage | считать order-invariant bag-of-words равносильной фразе; применять POS англ. языка к ивриту |
| SQL/DSL | AST-operator/alias/parentheses/binding, join direction, predicate scope, exact identifier identity | нормализовать != в =, склеивать токены через стрелки, игнорировать порядок JOIN |
| Generic router E22 | observed dataset id, family/label imbalance, abstain/margin calibration; per-domain F1 | глобально деплоить по одному macro-F1 при TypeScript=0/Tests=0 |
| Plain text fuzzy retrieval | semantic vector, token overlap, topic, recency, bounded KNN recall | применять строгий AST gate к предложениям без AST |
| Numeric rules/finance | operand/sign/units/threshold, symbolic equality, bound checks | приносить dependency-POS heuristics как универсальное улучшение |

## 5. Кратчайшая очередь решений

1. **P0 direct Student:** frozen B0 vs tiny chosen-only supervised warmup vs DPO-only и warmup→DPO; first-token/free answer/domain gold по C# и ENRU. Не тратить неделю на Windows Actions.
2. **P0 Unit/KNN separate:** E09/E14/E22 как доноры только после frozen source/label/split и независимого *ordinary consumer* теста. Первый опыт — одна lawful lesson + ее source-disjoint следствие и отрицательный law.
3. **P0 exact provenance / checkpoint:** один маленький cold process restore and real model/text generation parity; удостовериться, что исполнитель учит того Student, которого позже экзаменует.
4. **P1 domain-specific:** order/POS/AST KNN arm против простого vector-only arm на ENRU и C#, отдельные panels SQL/DSL при наличии independent gold.
5. **P2 N38:** native finite difference с fractional legacy masks и mixed-objective homogeneity; если соответствующие callers не достигнуты — задокументировать LATENT_NOT_REACHED и НЕ трогать RP01.

**Готовый минимальный protocol:** [N38 micro experiments](38-MICRO-MULTIDOMAIN-LEARNING-DPO-KNN-AND-UNIT-EXPERIMENTS-2026-10-16.md).

## 6. Инструкции ролям и принцип доказательств

Источник принятия решений — цель владельца: более быстрое реальное обучение, не test-count, Windows build cycles или подгонка тестов к метрикам. Skills были прочитаны из core docs/skills: architectural-decision-testing, phase-planning, plan-review, code-examination, generated-code-review, prompt-patch-authoring, claim-as-hypothesis, experiment-design, human-readable-architecture-audit, output-readability-gate.

- **Architect** выбирает один проверяемый recipient/consumer и запрещает переписывать уже успешный механизм без head-to-head.
- **Planner** строит маленькие source-family-disjoint cases, фиксирует train/heldout/negative control, бюджет и stop.
- **Independent plan reviewer** проверяет идентичность сравниваемых источников и intervention, предотвращает ref/lesson/gold leakage.
- **Executor** использует CODEX / Astra 6 UltraCode там, где его роль прямо разрешена, но **N38 не утверждает вызов модели**; для реальных новых финальных DPO chosen/rejected автором является только допущенная внешняя открытая китайская модель, независимая CODEX оценка фиксирует точную пару и реальный Student output. Simulation не переименовывать в external.
- **Code reviewer** требует реальные source paths и run receipts, проверяет отсутствие mutation train/production в exploratory audit.
- **Human communication** — русское объяснение влияния на пользовательский закон; явные статусы SOURCE, NATIVE, DOMAIN_QUALITY.

Никаких правок llm_mvp_core/docs/plans/replanning-possible-diffs/** и никаких ретроспективных изменений существовавших E09/E14/E22/RP01 артефактов. Публикация новых заметок — в planning/possibleDifs только.