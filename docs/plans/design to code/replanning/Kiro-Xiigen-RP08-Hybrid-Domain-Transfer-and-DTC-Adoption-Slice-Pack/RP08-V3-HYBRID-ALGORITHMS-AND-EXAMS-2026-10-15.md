# RP08 V3 · Real hybrid algorithm, law constraints and causal bulk exams
**SPECIFICATION/EXAM PLAN — not existing API declarations or measured outcomes.** Verify actual K interface signatures before mapping these conceptual fields; do not paste the schemas below as new public contracts or fork RP02/RP03/RP04 engines.

## 1. Exact hybrid decision sequence
1. **Eligibility:** collect domain candidate(s), intent, version/stack/dialect, actor/rights, source family, source law and privacy/split. UNKNOWN/MIXED is allowed. Hard rights, version, semantic validity and effect authority are deterministic; reject or abstain before effects. A technically useful but forbidden candidate has utility **ineligible**, not a low similarity penalty.
2. **Law representation:** subject-specific typed IR/token fields preserve the distinguishing operation: grammatical antecedent+agreement+polarity (D1), async dependency/effect status (D2), DOM geometry/viewport (D3), C# AST/operator/nullability (D4), Angular signal dependency and version (D5), SQL three-valued logic/join cardinality (D6), DPO exact pair+provenance (D7). Maintain `law_schema_id`, `coordinate_id`, `tokenizer_id`, version, negative/warning polarity, provenance. Domain-aware semantic projection is *not* Student BPE ID, and LoRA parameter recipient is *not* a vector coordinate.
3. **Retrieval:** restrict to training-visible lawful candidate corpus with compatible rights/source/version/split, then use vector/lexical/pattern evidence. `k`, metric, index ef, candidate budget, domain uncertainty and learned rerank are separate controls. Score can suggest but NEVER override source-law refusal. Preserve *all* eligible output IDs, relation and explanation; multiple neighbors are useful only if consumer can choose safely among them.
4. **Deterministic + learned cooperation:** `ILearningUnit` domain + input vector + output schema, many inputs→many typed results retaining provenance; `ILearningFlow` identity-preserving corrections on relationships, frozen Unit control when Flow learns; `ILearningOrchestrator` async lawful candidate paths, cancellation journal, budget, effects; `ILearningDynamicGraph` higher-order order-2/3 relation motifs and selected trainable preference; existing `ILearningMachine` owns updates. Final arbiter distinguishes answer/warning/refusal and may reject any learned suggestion violating law.
5. **Feedback and update:** after consent, chronology and no-leak split, compare positive/negative answer preferences with domain law, source authority and true same-x counterfactual. CODEX requested Astra 6 UltraCode chooses the case; admitted Chinese author writes both final sides; separate CODEX Astra 6 UltraCode judges unchanged pair. TIE/BOTH_BAD/UNJUDGEABLE stops. Existing DPO objective must use correct response mask/reference/frozen base/allowed LoRA recipient and canonical one-pair journal. Cold selected state and actual consumer are evidence; optimizer count is not quality.
6. **Continuous learning:** predict→log before knowing feedback→validate rights/law/split/conflict→teach only novel allowed state→reconcile durable side effects after crash→run independent old/new domain exam→promote or rollback with explicit parent. No accidental cross-tenant transfer.

## 2. Adaptive kNN: hypotheses, not an invented proved formula
A domain-conditioned controller can search eligible candidates in bounded expansions `k=1→2→4→8...` **only** within declared maxK/latency/CPU/VRAM constraints, stopping when legal alternatives/uncertainty/conflict and estimated gain justify it. This is a **proposal**; learned k requires a named trainable policy, input features, target credit and actual updated policy version. Fixed heuristic expansions are deterministic, not trained. Compare:
- A0 deterministic law-only/no learned retrieval;
- A1 actual top1 baseline;
- A2 same index/eligible pool, top-k consumed with lawful typing and warning separation;
- A3 identical eligible pool and k, actual learned reranking state N/U;
- A4 domain/uncertainty/budget-aware learned k N/U (separate from A3);
- A5 same eligible corpus, exact-vs-ANN recall/quality/cost with controlled index parameters;
- A6 Unit/Flow/E3/E4 single-state ablations;
- A7 Student response-only LoRA one-pair N/U.
Do NOT compare A1 old code to A4 new code+fresh model and attribute all gain to learned k. Query-dependent lexical normalization, changes to corpus stats, ANN recall, rights filtering and abstention must be reported as distinct mediators.

## 3. Law-sensitive examples — public illustration ONLY, NOT DPO training records
| Domain | GOOD and why | BAD and why | EDGE and action |
|---|---|---|---|
| D1 EN→RU | `The company approved it; it is ready` with explicit feminine company referent and compatible adjectival gender; polarity preserved | selects masculine agreement from higher cosine or drops `not` while claiming fluent translation | antecedent ambiguous: UNKNOWN/clarify, never fabricate gender |
| D2 asynchronous workflow | validated approved A before send(A), cancellation respected and charged | cheaper send(A) before approval or double-dispatch after timeout | unknown committed effect: journal and reconcile |
| D3 DTC HTML/CSS | same identified element fits 360px and 1280px with assets rights | attractive 1280px only but 360px overflow; wrong source node relation | font/asset unavailable: partial permitted result not invented screenshot |
| D4 C# | distinguish `s?.Length` (nullable) from `s?.Length ?? 0` (coalesced) in actual project AST | raw near-neighbor says both yield 0 on null | missing `s` declaration or language version: abstain/ask |
| D5 Angular | conditional computed signal dependencies tied to actual Angular version | treats signal as always read when branch excludes it | component/template unavailable ⇒ unknown |
| D6 SQL | ON predicate under LEFT JOIN preserves unmatched left rows with correct NULL semantics | WHERE moved predicate suppresses null-extended rows while claiming equivalent | schema/dialect unknown ⇒ ask, use three-valued logic |
| D7 DPO | chosen/rejected differ on a lawful actionable defect under same x, both admitted author receipts | rejected copied from Student then falsely attributed to Chinese; both outputs equal quality | TIE/BOTH_BAD/REJECTED_BETTER quarantine; no silent flip |

Negative/warning neighbor is diagnostic evidence, **not an executable answer**. Same grammatical surface with changed referent, operator or nullable law must retain meaningful distinct token coordinates. A nearest-neighbor string matching score is not evidence of lawful reasoning.

## 4. Frozen bulk experiment before looking at U outputs
Use independent source-family IDs; train/dev/operational-choice/final sealed confirmation are distinct, including RAG/cache/derived paraphrase lineage. Baseline **B0**; B1 after one eligible unique new pair/policy update; B2 from different source family and old-domain retention, including no-feedback path. Single-factor arms: optional O old code, N corrected code+**no** learned-state change, U **same corrected code**+one state mutation. Fixed task corpus/hardware/solver/index/decoder/license/source-law/budget. If hardware or corpus changes, start a different experiment ID; don't pool.

For each domain × arm × bulk, record `family_id; source/law_version; heldout seal; domain prediction; admissible law outcomes; candidate IDs; warning/positive/negative polarity; path and enumerated feasible set; output-to-input association; state writer/parent/version/hash; cold reader; answered/abstained/refused/timeout; correctness; quality by blinded rater; wall p50/p95; CPU time, process-tree peak RAM/VRAM, disk/index size, inference/training cost, failed/cancelled branch and external author/judge cost; n and uncertainty`. Do not use sum of per-process peak RAM as actual simultaneous peak.

**Report metrics explicitly** (defined before trial): `law_violation_count / eligible_n`; `useful_correct_n / total_enrolled_n` including refusals; `correct_given_answered` alongside abstention/coverage; `domain_mapping_accuracy` on independently labeled domains; `chosen/rejected admissibility and true preference proportion` with ties; `path_regret = achieved_cost - least_cost_enumerated_lawful_path` ONLY within enumerated feasible paths; paired `U−N` task success and resource deltas; old-domain retention after B2. Enrolled n must include refused and failed runs. Predeclare critical-law tolerance (normally zero for safety critical) and practical benefit threshold and stopping rule; tiny n = exploratory not certified. Time-to-first-**lawful-useful** output matters more than test-run duration.

## 5. Falsification and repair cycle
- Code fixed but state same ⇒ ENGINEERING_GAIN, not LEARNING_GAIN. Search larger k but output `hits[0]` ⇒ NO_MULTI_NEIGHBOR_EFFECT.
- State changed but cold real consumer reads previous version ⇒ NO_ADOPTION; repair selected-state binding, not scoring rubric.
- U better on TRAIN, equal/worse on unrelated families ⇒ overfit/contamination, no promotion; examine DPO pair, representation and mapping.
- Law error in D1 negation, D4 nullability, D6 SQL three-valued join, or unauthorized effect ⇒ HOLD irrespective of averaged utility.
- Accuracy up but PC cost exceeds declared user hardware or time budget ⇒ report tradeoff and try bounded exact-candidate/cached-statistics, not invented speedup.
- Cannot reproduce rights/labels, author/judge roles, frozen arm identity or total cost ⇒ INVALID_EVIDENCE; produce small missing-evidence work order, not PASS.
- Native host unavailable or Astra not invoked ⇒ CODE_READY/SCIENCE_ONLY and REVIEW_NOT_PERFORMED, not fake DTC adoption/reviewer pass.

**One-line success criterion:** actual permissible new-domain and retained old-domain user tasks improve in matched heldout N→U, trace reaches the accepted cold ordinary user path, and full PC costs remain within a predeclared budget; otherwise we have a useful partial finding, not a completed RP08.
