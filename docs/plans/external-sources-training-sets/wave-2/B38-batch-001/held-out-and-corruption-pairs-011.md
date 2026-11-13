# Held-Out and Corruption Pairs (011)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## B38-HO-011-001 — v6-01 corruption: QA suppressed when "checklist" context

Corruption type: v6-01
Input: "Review the deployment checklist and approve the release."

Correct result:
- "Review" → QA
- "deployment" → GAP
- "checklist" → GAP
- "approve" → QA
- PredicateSet: ["QA"]
- CapabilityGapWarnings: ["deployment", "checklist"]
- ResultState: routed to 7.1

v6-01 corrupted result:
- Compiler sees "checklist" → applies suppression: "this is checklist work, not a review"
- QA suppressed
- PredicateSet: [] (WRONG)
- ResultState: knowledge_edge (WRONG)

Gate B assertion that would catch v6-01:
If input contains "review" or "approve" AND ResultState == "knowledge_edge" → v6-01 detected

Explanation:
Gap vocabulary NEVER suppresses predicates. "checklist" is gap vocabulary. It produces a CapabilityGapWarning. The QA predicate from "review"/"approve" still fires. The predicate set is ["QA"], not [].

---

## B38-HO-011-002 — v6-02 corruption: probationary flow accepted despite Gate B failure

Corruption type: v6-02
Scenario: ReleaseGateFlow71_v2 (probationary) selected for "Approve the service release."
Gate B score: 0.58 (below threshold 0.85)

Correct behavior:
- Gate B check: 0.58 < 0.85 → FAIL
- Fallback: ReleaseGateFlow71 (deterministic_generic) executes
- Result state: probationary_rejected; output from fallback

v6-02 corrupted behavior:
- Gate B check: 0.58 < 0.85 → FAIL recorded
- (Bug) Probationary flow executes anyway
- ReleaseGateFlow71_v2 output used as result
- gate_b_passed = false in output (detectable sign of corruption)

Gate B assertion that would catch v6-02:
Any output where flow_type contains "probationary" AND gate_b_passed = false → v6-02 detected

---

## B38-HO-011-003 — v6-03 corruption: QA+IMPLEMENT routing collapsed to single 7.1

Corruption type: v6-03
Input: "Audit the build artifacts and approve the payment service release."
Correct: ["QA","IMPLEMENT"] → 4.1 + 7.1; DecisionCount = 2; TransitionType = feature_addition

v6-03 corrupted result:
- Router collapses ["QA","IMPLEMENT"] → ["QA"]
- Routing: 7.1 only; DecisionCount = 1 (WRONG)
- TransitionType = no_contribution (WRONG — should be feature_addition)
- BackendServiceFlow41 not executed (WRONG)

Gate B assertion that would catch v6-03:
If input contains "build" or "service" or "pipeline" AND IMPLEMENT not in phase_routing → v6-03 detected

Explanation:
Rule 15: each predicate produces one independent routing decision. No predicate absorbs another. "build" and "service" both fire IMPLEMENT. IMPLEMENT routes to 4.1. This cannot be collapsed into the QA routing at 7.1.
