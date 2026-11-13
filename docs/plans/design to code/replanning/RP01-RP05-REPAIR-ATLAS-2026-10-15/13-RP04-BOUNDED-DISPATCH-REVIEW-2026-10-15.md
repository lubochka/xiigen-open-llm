# RP04 — bounded dispatch: fresh source findings, 15 October 2026

Status: **SOURCE_REVIEW / PLANNING_DRAFT; not product implementation, training, independent review or measured improvement.** This supplement extends existing RP04 F22–F30 rather than rerunning R01–R25. It is not four new serial tasks. Existing task owners absorb only the finding relevant to their next action.

## Source snapshot and scope

Planning baseline inspected: `lubochka/xiigen-planning@d507493cb54a60bfff9e48e9bab5435e9c3d85bf`; core inspected: `lubochka/llm_mvp_core@a564772ab713aae071b27c513a9bad5a1a775a5e`. Existing RP04 manifest has **12 physical task cards**, V6. Older references to 16 RP04 cards are not this snapshot. F17–F30 already describe unactivated lifecycle, configuration-versus-structural learning, partial outcomes, flattening, null streams and fixed-pool limitations. Those are retained, not counted as newly discovered defects.

Directly inspected core sources:

- [AxisSweepCombinationStrategy.cs](https://github.com/lubochka/llm_mvp_core/blob/a564772ab713aae071b27c513a9bad5a1a775a5e/21.AgentInfrastructure/Logical/AxisSweepCombinationStrategy.cs#L35-L96), blob `9720de08246190fe9696115f78d9cba77178c94d`.
- [BoundedCombinationGraphCandidateRunner.cs](https://github.com/lubochka/llm_mvp_core/blob/a564772ab713aae071b27c513a9bad5a1a775a5e/21.AgentInfrastructure/Logical/BoundedCombinationGraphCandidateRunner.cs), blob `8099c2c68d3977f61ddf15ada77b89451f4a499a`; inspected ranges 1–330, 425–535 and 600–760.
- [DynamicGraphMachine.cs](https://github.com/lubochka/llm_mvp_core/blob/a564772ab713aae071b27c513a9bad5a1a775a5e/15.OrchestratorModel/Logical/DynamicGraphMachine.cs#L21-L77), blob `8149717b04d442e6e89bb61ab34fb608201ef369`: training/evaluation/export/import remain explicit NOT_ACTIVATED in this class. This reconfirms an existing finding; it does not prove every other owner is inactive.

## RP04-BD01 — the cap is applied to a lexicographic prefix, not domain-diverse candidates

**NEW SOURCE REFINEMENT OF F28/F30; runtime quality impact NOT_MEASURED.** AxisSweep sorts distinct node IDs and edge keys ordinally, iterates node → edge → four flag profiles, and returns immediately when `plans.Count == request.CombinationCount` (lines 45–94). The request permits 2–8 combinations. Thus with nodes A/Z, edges E1/E2 and a cap of 4, all four candidates belong to A/E1. With cap 8, the first node can occupy the entire pool. A later ranker cannot recover Z from an already truncated pool; shuffling input arrays does not fix the ordinal sort.

**Required repair:** RP04.00A records where candidate proposals are cut; RP04.01A distinguishes proposal budget, admitted pool and execution budget. Keep the existing deterministic prefix as a named baseline. Where the claim is domain/path coverage, place a bounded lawful domain-aware proposal allocation BEFORE the truncation, or report the precise unavailable candidates. Reuse RP02 domain-law/kNN evidence; do not invent a second tokenizer or claim that increasing k alone solves proposal starvation. A first valid configuration-preference lesson from the existing pool is still useful and need not wait for this extension.

**GOOD:** an A/Z controlled domain case records eligible axes, emitted IDs, budget and whether the independently lawful target is reachable. **BAD:** four A/E1 flag variants reported as four-domain or multi-route coverage. **EDGE:** fewer lawful candidates than the budget: emit fewer with explicit reason, never pad with forbidden or duplicate plans. If IDs are opaque labels, perform an identity-renaming challenge while preserving all semantic descriptors; changing names alone must not be credited as domain learning.

**Examination/falsifier:** source-family-disjoint fresh-pool examination measures lawful-target-in-pool/N and lawful final output/N separately from conditional ranking. If a verified upstream shortlist already ensures the needed domain diversity at this cap, narrow the finding to the local strategy and preserve that upstream proof. **Engineering minimum:** no new product tests for documentary/no-change work; only one focused budget/uniqueness/admission check if that deterministic boundary actually changes.

## RP04-BD02 — learned ordering must not destroy the runner's ordinal and semantic identities

**NEW VERIFIED INTEGRATION CONSTRAINT; not an existing observed learner failure.** `ValidateAndCopyPlans` requires `plan.Index == index` at each list position and independently requires unique `CombinationId` and `(NodeId, EdgeOrderKey, Flags)` (lines 425–484). AxisSweep derives CombinationId from the canonical tuple, not the eventual list position. `RunPlanAsync` normalizes child Index to zero while retaining CombinationId and uses the outer ordinal in TraceRef (lines 198–239).

**Required repair:** RP04.01B must distinguish `dispatch_index` from stable combination identity. When lawful learned ordering changes [A(index0), B(index1)] to [B,A], assign dispatch indices [0,1] WITHOUT changing B/A CombinationIds or their canonical plan signatures. Keep a map of original stable ID → dispatch index → output/feedback → update ID. Do not label or journal preferences by array position. Reordering alone is not evidence of structural graph learning.

**GOOD:** B becomes index0 but its content-derived ID, lawful flags and feedback target remain B. **BAD:** reorder objects with their old indices (runner rejects), or regenerate identities from new positions (feedback may attach to a different plan). **EDGE:** equal scores use a stable declared tie-break; no preference label is manufactured for a tie. A changed semantic flag is a new plan identity, not merely a new ordinal.

**Examination/falsifier:** one cold C0/U same-input pair must show stable ID → numeric score → actual dispatch ordinal → typed final output. If no list ordering is changed, record NOT_APPLICABLE rather than inventing an edit. **Smallest engineering check when changed:** one two-plan reorder plus existing duplicate rejection; no test-coverage expansion.

## RP04-BD03 — the configured timeout is not a demonstrated end-to-end deadline

**NEW VERIFIED LOCAL STRUCTURE; unbounded-delay impact is CONDITIONAL.** The runner calls synchronous strategy enumeration and validates/copies plans BEFORE creating `ExecutionCancellationScope` (roughly lines 72–122). Its batch stopwatch starts later, around line 149. On cancellation it awaits active children (lines 172–180); `AwaitActiveAfterCancellationAsync` awaits each task without a separate drain deadline (lines 655–670). Consequently this source alone does not establish that the caller returns by the configured timeout, especially when enumeration is slow or a child ignores cancellation. No such hang was executed in this review, and this is NOT evidence about the cause of earlier Windows runs.

**Required repair:** RP04.01A/03B separate total ordinary-request time, proposal/admission time, execution time and cancellation-drain time. RP04.00B/04A trace the actual child cancellation/effect owner before promising PC responsiveness. A cancellation token requests cooperation; returning early or `Task.WhenAny` does not prove the underlying work stopped. For an unsafe/uncooperative child, use an existing proven isolation/termination owner or mark the path unsuitable for a bounded-effect experiment. Never abandon a side-effecting task and launch a duplicate under a fresh journal ID.

**GOOD:** cooperative, read-only bounded first lesson with actual stop/drain receipt and all costs counted. **BAD:** batch ElapsedMs called end-to-end latency, TIMEOUT called resource release, or removing the drain wait while allowing untracked work to continue. **EDGE:** cancellation races with durable completion: resolve the same attempt, preserve its effect/state ownership, do not retrain or retry blindly.

**Examination/falsifier:** confirm an actual supported caller supplies a tighter deadline and safely owns termination, or record the limitation. Run only a narrow bounded cooperative-cancellation probe if the edited deterministic boundary needs it. Do not deliberately run a hanging in-process experiment. This item is NOT a mandatory cancellation-framework rewrite before a safe first parameter update.

## RP04-BD04 — post-result ranking is not the same intervention as learned dispatch savings

**NEW SOURCE REFINEMENT OF F25/F30; no speedup claimed.** The bounded runner fills available concurrency slots, then launches the next enumerated plan on each completion until all supplied plans are exhausted or cancellation occurs (lines 130–170). This inspected loop has no learned quality stop predicate. A consumer that reranks completed outcomes can improve final-answer choice while paying for all candidate executions. Merely changing final rank or list order cannot establish lower total work when the same full set is still executed.

**Required repair:** RP04.01B/01C label the actual intervention: CONFIG_PREF, RESULT_RANK, DISPATCH_ORDER, DISPATCH_SUBSET or learned stopping. Put the selected-state read at the actual decision point and record exactly which work it can avoid. The earliest genuine preference update remains valuable; do not delay it for a streaming implementation. A later dispatch-subset/stopping claim needs its own bounded experiment, actual cooperative cancellation and lawful fallback. Response-DPO and route/motif pairwise objectives remain different mechanisms.

**GOOD:** same-pool result-ranker experiment claims improved lawful answer selection, with unchanged/all-candidate execution cost reported honestly. **BAD:** a better winner score or winner-only latency reported as less work/compact PC learning. **EDGE:** earlier completion but equal total CPU work: report latency and total work separately, including all losers and drain time.

**Examination/falsifier:** instrument actual normal launch counts and state-reader position. If an upstream learned strategy already selects a subset before this runner, cite it and assess that strategy; do not attribute its effect to post-result sorting. RP04.03B compares fixed-pool rank quality separately from matched-budget end-to-end dispatch efficiency. No new product tests for exam design.

## Roles and acceptance retained

CODEX with **astra 6 ultracode** performs the delegated human case/choice selection. An authorized external open Chinese model authors BOTH final positive and negative response-DPO candidates. A separate CODEX invocation with **astra 6 ultracode** assesses their exact unchanged quality/preferences. No model calls have been made by writing this document. Observed route/motif preference is not automatically standard response DPO. No cancelled, missing or unobserved outcome becomes an observed negative.

Publication/author review is not independent Astra acceptance. Product code, learned state, domain experiments and PC measurements are unchanged by this supplement. Original WorkLeft one-use/provenance/rollback obligations, domain-law authority, RP02 representation/kNN and RP03 typed Unit/Flow ownership remain binding. Integration into selected physical cards is recorded separately; this initial source publication does not itself claim those cards have already changed.
