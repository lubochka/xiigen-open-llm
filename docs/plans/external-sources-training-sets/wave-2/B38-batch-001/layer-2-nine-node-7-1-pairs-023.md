# Layer 2 — Nine-Node 7.1 Pairs (023)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## B38-L2-023-001 — ProbationaryRejected: QA+TEST release testing flow rejected

Scenario: ProbationaryRejected
Input: "Verify the release testing criteria and sign off on the release candidate."
N01 Predicates: ["QA", "DOCUMENT"]
TransitionType: no_contribution (attempted via probationary flow; fallback used)

The probationary scenario:
A new flow candidate "ReleaseTestingFlow71-candidate" was evaluated.
Gate B evaluation:
  - Nodes covered: 6/9
  - Minimum confidence threshold: 0.88
  - Actual confidence: 0.71 (below threshold)
  - Gate B result: FAIL

Because Gate B failed, the probationary flow is REJECTED.
Fallback: universal-nine-node-development-flow-7.1 (deterministic_generic) is used.

SelectedFlowId: "universal-nine-node-development-flow-7.1"
TrustState: "deterministic_generic"
FallbackUsed: true
SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

N09 Result (via fallback flow):
  - TransitionType: no_contribution
  - Files: [release_testing_criteria.md, sign_off_register.md]
  - FileCount: 2
  - ResultState: release_candidate_signed_off
  - DecisionCount: 2

---

## B38-L2-023-002 — v6-02 corruption vs correct probationary handling

CORRUPT behavior (v6-02 corruption):
  SelectedFlowId: "ReleaseTestingFlow71-candidate"
  TrustState: "probationary"
  FallbackUsed: false
  Gate B passed: false  ← WRONG: flow accepted despite Gate B failure

CORRECT behavior:
  SelectedFlowId: "universal-nine-node-development-flow-7.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

Rule: If gate_b_passed == false, the probationary flow MUST be rejected. No exceptions.
The fallback (deterministic_generic) MUST be used. The result is still produced — via the fallback.
