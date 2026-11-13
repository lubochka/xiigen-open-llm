# RP04 — concrete hybrid algorithms, typed seams and state ownership

## 0. Contract of intervention
The path is: raw input → permitted domain/law passport (RP02 or frozen placeholder) → typed many→many Unit candidates (RP03 or existing ILogicalUnit/IDynamicGraphNode) → typed Flow corrections/OutputRefs → **Orchestrator legal async plans + learned choice (E3)** → **DynamicGraph admitted higher-order relation selection (E4)** → final deterministic arbiter → ordinary user output. Actual serving order of E3/E4 can be interleaved; this is a responsibility graph, not a mandatory fixed call order.

For each learned claim bind: objective/signal → parameters/state owner → exact update → versioned checkpoint → cold ordinary consumer → independent causal outcome. If a current edge Unit owns the learned choice, use its owner; do not create a competing ILearningMachine.

## 1. E3: split eligibility from preference
**Input:** (request_id, tenant/context, domain+intent confidence, law_ids, law_version, vector/coordinate_version, typed candidates/OutputRefs, Flow relation_state_id, graph_snapshot_id, allowed effects and consent, time/memory/token budget, cancellation scope, policy_snapshot_id).
**Deterministic admissibility A(x):** only candidate combinations whose every transition is type/port/coordinate/version compatible, dependency order acyclic, provenance authorized, side effects permitted, budgets finite, and required arbiters reachable. Invalid, mixed/unknown or legally disallowed proposals yield a visible gap/abstention or seek clarification; no score may grant authority.

**Combination construction:** bounded beam of K admissible unique plans of ordered/fan-out/fan-in Units and Flow ports. Count MaxCombinations planned separately from MaxParallelCombinationRuns actually in flight; both bounded by active budget. Include deterministic canonical fast path as a candidate and let it win if simpler/better. Never launch two non-idempotent side effects to explore counterfactuals; use replay/sandbox or proxy observation there.

**Trainable policy:** start with smallest candidate-level contextual ranker or calibrated linear contextual bandit using versioned features:
- domain/intent confidence and law-admissibility (mask, not positive bonus);
- predicted downstream usefulness and source association;
- relevant RP02 kNN warning/positive/negative memory within same law/embedding schema;
- candidate Unit/Flow relation compatibility and historical acceptance by actual arbiters;
- expected completion latency, critical-path length, concurrency peak, bytes/tokens, failure risk;
- unknown-knowledge edge and need for clarification.
Learning target: order eligible paths / choose finite depth and permitted stop policy, not judge correctness. Preference signal from independently accepted user outcome / arbiter + cost rubric. Record opportunities, propensity (where reliable), feedback uncertainty and skipped paths; offline unobserved outcome is UNKNOWN, not negative. Use conservative/no-update fallback for sparse domains. Different domains may have different feature normalizers or policy heads, not automatically different global tensors.

**Decision rule, conceptual:** eligible = deterministic_law_filter(candidates); ranked = route_policy(context, eligible, state_v); execute top permitted plan(s) within budget; receive typed result/branch trace; arbiter may reject, request repair or accept; store all reasons and total costs. A learned score is NEVER a permission, validity oracle or final arbiter. Scheduler owns retries, cancellation propagation, branch lifetimes and fair CPU/resource allocation.

**Credit assignment:** record per-branch value/cost; do not assign entire final reward to all launched branches. Preserve against confounding: freeze Unit/Flow/kNN/retrieved pool, hold graph state, swap only E3 route weights. For known law-defined exact solutions compare route regret to best *lawful available* alternative under the same resources; approximate optima must be labelled oracle-bounded, not absolute.

## 2. E4: higher-order pattern grammar and learning
**Input:** versioned graph snapshot with node identity, relation/type/domain, directional/order semantics, candidate evidence, legal provenance, authority, RP03 OutputRefs/Flow schema and RP02 coordinate version. Do not encode only a bag of edges.

**Order-2:** relation between admissible typed edges or transitions (e.g., source validation → domain reasoning → correction). **Order-3:** composition of typed order-2 patterns with a lawful fan-in, short return-loop or branch dependency. Each pattern = pattern_id, order, constituent node/edge IDs, directed port map, domains+law ids, typed input/output schema versions, preconditions, output association semantics, evidence origin and relation confidence/state snapshot id. Pattern structural fingerprint canonicalizes valid equivalent identifiers/order-insensitive subsets without erasing sequence, negation, units or actual authority. Hold out structural families, not only text rephrasings.

**Deterministic admission:** validate type/coordinate/version, law, source provenance, cycles/termination, owner/consent, memory/time budget, arbiter and no hidden target actions. Pattern builder may propose; final admission is deterministic and independently reviewed. The D-15 arbiter remains last authority.

**Trainable choice:** small policy or existing learned edge authority over admitted higher-order patterns; goal = improved success on unseen combinations of familiar primitives, with total state and search costs. Feature set: ordered typed relation fingerprint, domain law, RP02 similarity only within compatible schema, prior corrections, confidence and uncertainty, dynamic kNN warnings, score of subgraphs, PC and latency estimate. Training signal is not equal to link count; compare good/bad patterns and real downstream accepted or rejected user outcomes, with explicit no-update control and time-matched compute.

**L5 boundary:** first verify whether existing K/15.OrchestratorModel/Logical/DynamicGraphMachine.cs scaffold should own new train/eval/export/import, or whether the currently installed learned edge Unit owns policy weights and machine only coordinates state. Choose exactly ONE writer and ONE frozen serving snapshot per relation policy. Do not change public interface just to make names symmetrical. If no lawful owner identified, stop that specific product patch with source-bound missing-owner evidence, while continuing independent E3.

**Graph state lifecycle:** parent → immutable candidate train → checkpoint/pattern journal + parameter checksum + domain law and schema versions → independent evaluation → deterministic acceptance or rollback → atomic serving pointer → cold reload. Include replay provenance/consumed pair IDs, optimizer state and RNG if trained, retention probe, topology revision and source consent. Stale imported patterns are quarantined instead of silently migrated between vector coordinate systems.

## 3. Candidate discriminators and consumer semantics
Proposed *internal* payloads only; names are illustrative until exact caller/callee trace validates:
- RP04RouteProposal: plan_id, input_id, allowed_actions, dependency_DAG, predicted_cost, eligibility_reasons, policy_version.
- RP04BranchReceipt: plan_id, branch_id, parent_context, resolved_output_ref, start/end/cancel/fail, CPU/GPU/memory/tokens, outcome, assigned_arbiters.
- RP04PatternRelation: order (2 or 3), typed constituent edges, law/schema/coordinate versions, evidence/structural-family fingerprint, learned_strength, provenance.
- RP04LearningReceipt: parent_hash, candidate_hash, policy_owner, signal_origin, unique_update_id, checkpoint, observed no-update delta, end_user_consumer.
These are candidate shapes, **not requirements to introduce four new public types**. Reuse LogicalContracts and existing DynamicGraph projection/result envelopes with a minimal compatible adapter, if sufficient. No IQueryable placeholder without materialized requested-type payload and association.

## 4. Deterministic versus learned authority table
| Concern | Fixed/verified | May learn | Prohibited proxy |
|---|---|---|---|
| Types and OutputRefs | schema, lifetime, version, resolver, cardinality | ranking lawful matches | index-Zip assumptions |
| Domain law | constraints, source version, consent | domain uncertainty and weighting within legal candidates | score overriding law |
| Route | dependency DAG, bounded async, permissions, cancellation, cost trace | choose/order/stop among admissible plans | fastest branch always correct |
| kNN | same-version pool admissibility, exact/ANN comparison | query/domain adaptive k and scoring | universal fixed k equals learned |
| Pattern | constituent order, type, allowed higher-order edges | preference/transfer on new legal compositions | more nodes means smarter |
| Arbiter | final ACCEPT/REJECT/REWORK or UNKNOWN | choice of which arbiter to ask within fixed rules | budget or Chinese model overriding rejection |
| Storage | idempotence, atomic candidate/promote/rollback, no-repeat | policy weight update | new SHA equals trained |

## 5. Exact DPO compatibility (do not shortcut roles)
For permitted preference lessons, CODEX GPT-6 Astra UltraCode plays the **human case selector** from an allowed frozen teaching pool. The approved **external open Chinese model** creates BOTH final chosen and rejected strings under the SAME frozen prompt/context; preserve exact bytes, model license, provenance and unique case key. A SEPARATE CODEX GPT-6 Astra UltraCode examination judges the Chinese-authored exact final pair and can return TIE/BOTH_BAD/UNJUDGEABLE. The rejected side must be plausible and meaningfully worse under explicit domain laws; no automatic reuse of the Student answer as final negative; no edit after pair judgment without rejudging. Reference policy for DPO stays frozen and is NOT CODEX. These roles apply even when R04 route/graph candidate contrast suggests a pair; route trace is evidence, not a substitute for authored DPO response strings. Later trainer only consumes admitted unique pairs by an explicitly declared recipient; target parameters can be route/graph preference head only if the optimizer and format actually reach them, else do not claim learned route policy.

## 6. How RP04 differs from adjacent slices
- RP01 E1 Student DPO optimizer/LoRA/baseline is separate, runnable without E3/E4.
- RP02 E2 domain law, semantic representation, query-adaptive kNN must supply versioned profile or a frozen legal fixture; RP04 does not retokenize across domains.
- RP03 E3 typed Unit many→many and Flow correction state supplies payload associations; RP04 freezes those mechanisms in its own experiments.
- RP05+ carries broad PC continual feedback/native DTC+IDE deployment. RP04 records compatibility and actual minimal serving path only; no silent release-claim.

## 7. Minimum runnable split and safe recovery
E3 baseline: existing lawful fixed route → same candidates scored by versioned ranker → one real cold-run result; freeze other layers, compare accuracy/time/branches/cost. E4 baseline: graph snapshot of primitives/order-1 → add only admitted order-2/3 relation preference, leave E3 fixed → unseen composition panel and cold-run output. An unknown candidate stays DRAFT, error/cancel preserves trace and disallows promotion. Non-improvement means keep previous state and continue other independent lane with explicit evidence.
