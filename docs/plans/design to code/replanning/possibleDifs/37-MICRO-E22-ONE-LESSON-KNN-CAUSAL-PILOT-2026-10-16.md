# MICRO-N37 — short domain causal pilot: E22 KNN 0→1→2 lesson, majority threshold, replacement, empty-query

**Status 16.10.2026:** executable proposal + source-equivalent JS fixture/proxy; **native .NET / live consumer / external teacher / independent CODEX judgement NOT_RUN**. Aim: verify *learning utility*, not pass count. Source under audit [PredicateDatasetRouter.cs](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/22.AgentAlgorithms/Context/PredicateDatasetRouter.cs); source finding [N37](37-NEW-E22-ONE-PAIR-KNN-VOTE-CLIFF-AND-ONLINE-REPLACE-2026-10-16.md). Target recipient: E22 `AgentAlgorithms.PredicateDatasetRouter` ONLY; avoid proxy≠native equivalence claims.

## MICRO-N37.A — one-factor `k` denominator (same example set, same ranking)

- **Baseline A:** immutable source `Predict` with configured `k=3` and cutoff `votes > 3/2`.
- **Enhanced B:** only change *denominator at voting* to `effectiveK = neighbours.Length`, `votes > effectiveK/2`. Do **not** change tokenizer, label function, Jaccard, sorted examples, candidate pool, data, model, threshold for `n>=3`, or code outside a branch/test harness. For `n=0` use same source fallback in A/B; test alternate abstain only in MICRO-N37.C.
- **Training snapshots:** n=0; n=1 (one BugFix); n=2 (two independently authored BugFix cases); n=3 (three BugFix). `Train(snapshot)` calls always pass the **full** accepted set, never only delta. For each, exact `queryPredicates` of first example [101,105], plus source-disjoint queries in a separate blind panel.
- **Analytic predictions** with k3: n0 `all six true`; n1 `all six false`; n2 BugFix `true`; n3 BugFix `true`. With B and n1, BugFix `true`; with n2, BugFix `true`. Negative-control bucket `Tests=false` on BugFix exact query. For `n=3+` same labels/ordered neighbors A and B must match exactly.
- **Domain panel:** 12–24 independent, frozen, source-family-disjoint examples: 8–12 C# (method-body null handling, no-op namespace period, two-method counterexamples), 4–6 mixed Orchestration+Tests/PLAN, 4–6 heldout negative/unknown (empty predicates and unrelated snippets). Provide real original files and external semantic gold before scoring; auto-label/string `NeedsNullCheck` is **not gold**.
- **Success:** source-level cliff reproduced n=1, and on blind domain cases B improves **per-bucket macro-F1 / critical misroute / retained old-law** with no unacceptable false-positive increase. Record exact numerator/denominator, confusion by bucket, abstention rate and source family, paired difference and latency. If only exact train recall improves, status `SOURCE_BUG_CONFIRMED_QUALITY_GAIN_UNKNOWN`; never generalize from n=1 to 360-case E22 historical run.
- **Falsifier:** n=1 already predicts its positive bucket under actual current C# source or a different caller bypasses `Predict`; then source interpretation/consumer assumption must be revisited before change.

## MICRO-N37.B — replacement vs cumulative retention (different axis, separate run)

- Seed router with **three** distinct correct BugFix lessons (IDs `bugfix_1..3`), freeze exact old queries, train baseline `Train(all3)`.
- New accepted lesson `test_1` (independent label `Tests`), compare (A) **incorrect incremental caller pattern** `Train([test_1])` versus (B) **complete rebuild contract** `Train(all3 + [test_1])`; same k=3, same query vectors, same frozen gold. Do not simultaneously change denominator.
- A is source-guaranteed to remove three BugFix examples because `Train` calls `Clear`. B retains them; retention **on current queries may still be sensitive to tie breaking** and must be measured.
- **Decision:** If all real callers pass full snapshots, classify `NOT_A_PRODUCT_BUG/CONTRACT_CLARIFICATION`; if an online caller passes delta, classify `CONNECTED_REGRESSION_RISK`, then require explicit `AddLesson` with immutable pair ID or full-state rebuild and transactional acceptance. Any actual code modification requires separate approval/work order.

## MICRO-N37.C — empty evidence / corpus-order control

- Keep n≥6 corpus fixed with 3 BugFix + 3 Tests. Execute `queryPredicates=[]` and permute *only* training order across two controlled runs; compare predictions. All nonempty rows have Jaccard distance 1 to empty query, so top3 depends on corpus order. This **shows lack of information**, not an objective semantic “correct label”.
- Compare existing A (always predict) with B (explicit `NO_EVIDENCE` when query has no usable predicates, either safe abstain or fallback). Score wrong confident route vs abstentions on sealed unseen C#/PLAN/Tests cases, not simply percent abstained.
- Control: `queryPredicates=[]` with an **empty-predicate training row** (union=0 => distance0), to verify distinction `no predicates` vs absence of any candidate. Semantics of truly empty source must be defined before accepting anything.

## MICRO-N37.D — consumer availability gate and validity

First trace actual production path: `source request → predicate extraction → PredicateDatasetRouter.Train/Predict → chosen dataset → downstream learned Unit/Flow or real user output`; write exact SHA, call-site, one accepted lesson manifest, model/index state and cold reopen. The already known [E24](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E24AttributedRouter.cs#L32-L92) trains router but never uses its prediction for the measured route, so cannot supply the consumer proof. If no ordinary consumer, **stop at experiment-local classification: no claim of RP01 product gain**.

**Historical positive control:** E22 archive Tier3, 450 total examples, 360 train / 90 val, baseline 0.4709, KNN 0.5646 macro-F1 ([primary saved JSON](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json)). Do not recompute it as a one-pair experiment, do not change label definitions to force a PASS. Verify no regression at 360 examples only **if** candidate enters actual E22 code.

**Evidence protocol:** source-proxy code/JSON, intended C# switch point, exact dataset IDs/split manifest, offline blind gold, paired per-case before/after and cold restore SHA (if supported), verdict `IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE/NO_CONSUMER`. No Windows full CI as a science quality metric. A deterministic vector vote difference is a logical bug/feature; usefulness requires independently graded ordinary output.

**Roles if this becomes a real DPO/learning trial:** human selection = **CODEX + Astra 6 UltraCode**; both final chosen/rejected = **approved external open Chinese model**; exact pair and final output quality separately evaluated by **CODEX + Astra 6 UltraCode**. This router-label microexperiment generates **no DPO**; no teacher or CODEX independent evaluation is claimed. Review: source/plan self-review only, not independent review.

### Run / artifacts

The minimal bounded JS arithmetic reproduction is [37-MICRO-E22-ONE-LESSON-VOTE-PROXY.mjs](37-MICRO-E22-ONE-LESSON-VOTE-PROXY.mjs) with fixed [fixture](37-MICRO-E22-ONE-LESSON-KNN-FIXTURE.json). Execute in Node 20+: `node 37-MICRO-E22-ONE-LESSON-VOTE-PROXY.mjs`. It intentionally implements only `PredicateDatasetRouter` array ranking, Jaccard and majority vote; it does **not** execute native C# or measure semantic learning. The bounded result is [receipt](37-MICRO-E22-ONE-LESSON-SOURCE-RECEIPT.json).

### Executor instructions

Read applicable `AGENTS.md` plus `docs/skills/architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `experiment-design-SKILL.md`, `claim-verification-SKILL.md`, `prompt-patch-authoring-SKILL.md` and `human-readable-architecture-audit-SKILL.md`. Work only on MICRO-N37 after independent review; no alteration of protected core research directory; do not change native code as part of this planning package. Preserve source pin and declaration of any updated exact head.
