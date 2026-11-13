# Layer 0-B — Router Pairs (011)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: router

---

## B38-0B-011-001 — v6-03 corruption: [QA, IMPLEMENT] collapsed to single 7.1 decision

Corruption type: v6-03
Input: "Review the order service implementation and approve the release."
Correct PredicateSet: ["QA", "IMPLEMENT"]

Correct routing:
1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41) — order 1
2. QA → Phase 7.1 (ReleaseGateFlow71) — order 2 (last)
DecisionCount: 2
TransitionType: feature_addition

v6-03 corrupted routing:
- Router sees [QA, IMPLEMENT] and collapses to highest-phase predicate
- Only QA routed → Phase 7.1
- IMPLEMENT routing to 4.1 dropped
- DecisionCount: 1 (WRONG — should be 2)
- TransitionType: no_contribution (WRONG — should be feature_addition)

Detection: Input contains "service" (IMPLEMENT trigger). If DecisionCount = 1 and input has a service/build/pipeline word, v6-03 corruption is likely.

---

## B38-0B-011-002 — v6-03 effect on downstream nodes

When v6-03 collapses [QA, IMPLEMENT] to [QA]:
- N05 (flow execution): only ReleaseGateFlow71 runs; BackendServiceFlow41 never invoked
- N07 (output): only 2 files produced instead of 3
- N09 (result): TransitionType = no_contribution instead of feature_addition
- N09 (files): service_review.md missing from output

The 4.1 routing decision is silently dropped. No error is raised. The output looks superficially complete (release gate ran) but is missing the IMPLEMENT-phase artifacts.

---

## B38-0B-011-003 — v6-03 correction procedure

To correct v6-03:
1. Rerun compiler on original input
2. Verify predicate set contains all fired predicates (check for "service", "build", "pipeline" words)
3. Send full predicate set ["QA", "IMPLEMENT"] to router
4. Router must create one routing decision per predicate
5. DecisionCount must equal len(predicate_set) = 2
6. Execute both flows: BackendServiceFlow41 first, ReleaseGateFlow71 second

The fundamental rule (Rule 15): two predicates → two independent routing decisions. No predicate absorbs another. No collapse.

---

## B38-0B-011-004 — Summary: all v6 routing corruptions

v6-01 (Compiler level): QA suppressed when gap vocabulary present
- Router receives: [] instead of ["QA"]
- Incorrect ResultState: knowledge_edge instead of 7.1 routing
- Fix: compiler must not suppress predicates due to gap co-occurrence

v6-02 (Orchestrator level): probationary flow proceeds despite Gate B failure
- Router selects probationary flow candidate
- Orchestrator executes it without Gate B verification
- Fix: Gate B check is mandatory before any probationary flow execution

v6-03 (Router level): multi-predicate collapsed to single decision
- Router receives: ["QA", "IMPLEMENT"]
- Router collapses to: ["QA"] only
- DecisionCount under-counted; IMPLEMENT phase skipped
- Fix: Router must issue one decision per predicate; no collapse logic permitted
