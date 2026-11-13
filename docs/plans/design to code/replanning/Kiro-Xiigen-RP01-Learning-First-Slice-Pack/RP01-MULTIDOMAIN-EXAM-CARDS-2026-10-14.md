# RP01 — domain-law exams instead of code coverage

**2026-10-14 · PRECOMMIT TEMPLATE / NO MEASURED RESULT.** Use with [five-pass dispatch](RP01-FIVE-PASS-LEARNING-EXPEDITION-2026-10-14.md), [RP01 scientific examination](LEARNING-EXAMINATION.md), R13/R21/R22 and the independent RP09 examiner. A domain-specific case is not a unit test. Report domain law/version, meaning and end-task correctness, actual path to the best legal solution, uncertainty and complete local-PC resources. Do not report a gained ability without executing the lawful ordinary consumer.

## Operating principles

1. Domain = allowed input/output, versioned hard laws, constraints, intended task, alternative solution paths, rights, source families, actual evaluator, **and normal product consumer**. Unknown/mixed domain has its own abstention/compose branch; do not force C# because a code fence is present.
2. **Do not merge mechanisms:** raw BPE token ids, domain AST/lexical features, embeddings, semantic projection, LoRA parameters, kNN metric/k and policy, typed Unit/Flow, async Orchestrator and higher-order DynamicGraph are distinct learning or fixed states. A trained LoRA adapter is not automatically a correct vector token. RP02/RP18 must prove typed projection/decoder and downstream consumer separately.
3. Hard domain constraints are deterministic vetoes; learned modules choose/rank **within** allowed candidates. Compare a fixed legal solver against the learnable path at equal budget; a fast illegal answer does not score as progress.
4. The first RP01 E1 **requires only full CodeDocumentation C# precommit**. SQL and EN→RU are parallel shadow law exams owned by RP02/RP09; they must not delay one Student lesson, and they are NOT evidence of cross-domain Student gain unless corresponding real consumer is available and actually receives the changed state.
5. Evaluation unit = disjoint **source family**, never token, paraphrase, related prompt or candidate beam. Split train/dev/sealed BEFORE paraphrasing and Teacher calls. Closed exam material may never enter retrieval, Teacher, CODEX lesson selection or feedback. Precommit R13 sample size, meaningful effect, paired uncertainty/CI, resource ceiling, regret and stopping before seeing trial answers.
6. E1 comparison always P parent, C0 same repaired implementation but zero model update, T same implementation plus one authorized admitted DPO update. P→C0 is software correction; C0→T is learning. Keep tokenizer/config/decoder/source families/rubric and resources matched. Old-code P versus new-code T alone is invalid causality.
7. Track actual model delta, checkpoint restore and cold ordinary path separately from end-task correctness. A valid judged pair with unchanged Student behavior is an INCONCLUSIVE lesson, **not failed tests** and not quality gain.

## Exam card 1 — C# CodeDocumentation (FULL E1 precommit)

**Law/version:** C# nullable and lifted operator semantics, null-conditional versus null-coalescing, precedence, types, evaluation order, throwing properties/getters and actual language version.

**GOOD positive:** With int? value = null; int result = value ?? 0; the explanation identifies 0 because ?? supplies fallback. With string? s = null; int? length = s?.Length; it identifies null, not 0, because ?. short-circuits. **BAD negative:** claim that ?. automatically returns the default numeric 0. **Why it is wrong:** it collapses a lifted nullable result into a coalescing expression, changing the meaning. **EDGE:** nullable value type, property throwing, invalid member access, non-nullable compilation, operator overload, C# version conflict; answer must disambiguate applicability, not confidently improvise.

**Family separation:** training on one nullable construct does not permit simple renamed paraphrase as holdout. Withhold independent AST/value-type/control-flow family and separately hard-negative traps. **Score:** source-grounded essential claim accuracy, false-law critical errors, justified abstain and ordinary generated text. **Comparison:** P/C0/T on genuine CodeDocumentation Student and new-process /api/code-documentation/generate. **Owner:** RP01.02A→02B→03A; independent reviewer RP09. **First lesson prerequisite:** yes, this exam alone.

## Exam card 2 — SQL NULL and dialect (NON-BLOCKING shadow)

**Law/version:** three-valued NULL logic, JOIN cardinality, bag semantics vs DISTINCT, concrete dialect. **GOOD:** WHERE col IS NULL tests missing values, while col = NULL ordinarily evaluates UNKNOWN in WHERE. **BAD:** claim WHERE col = NULL finds NULL values. **Why:** it ignores SQL three-valued logic. **EDGE:** dialect-specific null-safe equality (e.g. IS NOT DISTINCT FROM), outer join duplicates, collation, older version/config semantics; require dialect and a safe query oracle.

**Family separation:** withhold different schema/join graph, dialect and query AST, not same query with renamed columns. **Score:** legal executable equivalence on consented fixture if actually run; otherwise independent semantics review clearly labelled NOT_EXECUTED. **Consumer:** actual SQL-capable normal product path only; no forced C# generation. **Owner:** RP02/RP09. **First lesson prerequisite:** NO.

## Exam card 3 — English ↔ Russian translation (NON-BLOCKING shadow)

**Law/version:** source propositional meaning, negation, direction, modality, condition scope, numbers and named entities. **GOOD:** “Do not delete the file unless it is approved” preserves prohibition with an exception in Russian (e.g. “Не удаляйте файл без разрешения”, given appropriate context). **BAD:** “Удалите файл, если разрешения нет”. **Why:** reverses permission/prohibition. **EDGE:** double negative, only-if versus unless, aspect, culture/case, ambiguous pronoun and specialized term; abstain/clarify if objectively underdetermined.

**Family separation:** independent source intentions, not translations of the same underlying propositions distributed between train and heldout. **Score:** proposition/negation preservation, direction, entity continuity, justified abstain — not fluency alone. **Consumer:** actual EN↔RU user path with own state identity, if available. **Owner:** RP02/RP09. **First lesson prerequisite:** NO.

## Exam card 4 — Angular (optional second technical shadow)

**Law/version:** Angular version bound to template typing, signals/Observable behavior, component lifecycle, API deprecations, browser/SSR. **GOOD:** answer specifies version and how code compiles/behaves under that version. **BAD:** outdated lifecycle/API treated as current on every version. **EDGE:** SSR/hydration, missing import, Angular major version conflict. **Owner:** RP02/RP09. This is an independent contract; it must not replace missing required SQL or translation passports.

## Exam card 5 — Mixed and unsupported request

**Example:** “Explain why SQL WHERE col = NULL is wrong, then translate that explanation to Russian while preserving negation.” **GOOD:** recognize two domains with compatible constraints or hand off explicitly; preserve SQL semantics and translation polarity. **BAD:** force CodeDocumentation C# label because snippets contain code; route to wrong tokenizer/LoRA silently. **EDGE:** source rights not clear, unknown dialect, version conflict, no consumer, two domain laws incompatible → explicit UNSUPPORTED/ABSTAIN/NEEDS_CONTEXT; include in denominator by declared policy, do not drop.

## Algorithm examination matrix — measurement at actual owner, not tests

| Algorithm and owner | Causal intervention / controls | Evidence of useful mechanism | Falsifier / deterministic guard |
|---|---|---|---|
| DPO Student RP01 | Same software + parent P / no-update C0 / single judged update T | C# heldout family quality, hard-law error, cost and cold ordinary output | C0=T, no changed LoRA keys, leaked holdout; guard exact bytes/mask/rights and replay only if altered |
| Domain mapper RP02 | oracle-domain vs predicted-domain vs decoy, fixed solution engine | lawful routing by family, mixed-domain abstain and end-task success | wrong dialect/version; check schema parser only if edited |
| Domain vector/semantic projection RP02+RP18 | raw BPE vs versioned typed features + projection, compatible reindex, frozen solver | law preservation, correct decoded fields, downstream task ability, bytes | negation/nullness lost or only cosine improved; typed serialization guard if edited |
| Query-adaptive kNN RP02 | k=1 vs true multi-hit fusion vs adaptive k/rerank on same lawful memory; exact then ANN | effective neighbour count/fusion changes final legal answer, recall/latency | Search(k) followed only by hits[0], illegal cross-domain retrieval; owner-focused check |
| ILearningUnit/ILearningFlow RP03 | frozen typed many→many Unit, then corrected Flow state vs fixed mapping | legal typed outputs and OutputRefs meaningfully improved | learned link corrupts type/output; typed-port guard if edited |
| ILearningOrchestrator RP04 | fixed lawful async route vs learned scheduler, same fixed graph | correct dependency/order, solution path regret, latency and budget | illegal asynchronous side effects; cancel/side-effect guard if edited |
| ILearningDynamicGraph RP04 | frozen lower-order graph vs separately learned higher-order pattern | unseen multi-step path improvement, cost-accuracy Pareto | simply static DAG or memorization; graph-version/state identity |
| Hybrid PC RP05+ | deterministic constraints + learned planner/Student/kNN vs matched-budget fixed and learning-only ablations | same or higher lawful success per full PC second/MB and old-domain retention | regression or hidden teacher/index/shadow costs |

**No numeric claims from a plan:** R13 signs off N, minimum relevant effect, evaluator independence and 95%-product-goal interpretation. Teacher/judge licenses, paid API/PC budget, dataset rights and local checkpoint must be actually authorized. If none are available: outcome BLOCKED / NOT_MEASURED, not fabricated zero or 100%.

## Bulk-to-bulk DPO and resource ledger

For every allowed B0/B1/B2 boundary record unique new pair count versus attempted candidates versus optimizer steps; pair content/parent/current LoRA delta; current training/dev/sealed hash and no-repeat state; per-family legal accuracy, critical-law failures and worst-domain retention; p50/p95, CPU/peak RAM/VRAM/disk/time, all teacher/judge/cold-start/reindex/retry costs; checkpoint and normal consumer. Apply only new judged pairs; no hidden replay or leaking earlier sealed families to new teaching.

**Example evidence record (illustrative only, NOT a measurement):**
~~~yaml
experiment_id: REQUIRE_REAL_R13_ID
domain: CSharpCodeDocumentation
law_version: REQUIRE_PRIMARY_SOURCE
source_family_id: REQUIRE_DISJOINT_FROZEN_ID
training_overlap: false
rights: REQUIRE_EXAM_RIGHTS
consumer: VERIFY_EXISTING_ORDINARY_CALLER
parent_P: VERIFY_EXISTING_CHECKPOINT
control_C0: SAME_CODE_NO_WEIGHT_UPDATE
trial_T: SAME_CODE_NEW_ADMITTED_PAIR_ONCE
rubric: CLAIM_LEVEL_WITH_CRITICAL_LAW_VETO
N_effect_uncertainty_stop: PRECOMMIT_WITH_R13_OR_BLOCK
PC_budget: PRECOMMIT_OR_BLOCK
actual_result: NOT_MEASURED
actual_Astra_review: NOT_PERFORMED
~~~

**Outcome labels:** IMPROVE / REGRESS / INCONCLUSIVE / INVALID_EVIDENCE / NOT_MEASURED / NOT_SUPPORTED_DOMAIN; separately KEEP_PARENT / PROMOTE_WITH_GRANT / ROLLBACK / NOT_ATTEMPTED. A passing build or Windows CI is not a domain exam. Local PC capacity is a measurement target, not a claim from the available specs.
