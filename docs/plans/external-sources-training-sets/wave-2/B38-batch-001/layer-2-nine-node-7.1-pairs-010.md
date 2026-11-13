# Layer 2 — Nine-Node 7.1 Pairs (010)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## B38-L2-010-001 — FullReleaseGatePipeline probationary_rejected: candidate fails Gate B

Scenario: FullReleaseGatePipeline (probationary candidate attempt — rejected)
Input: "Review the payment service, validate release criteria, security audit, and approve go-live."
N01 Predicates: ["QA", "IMPLEMENT", "DOCUMENT", "SECURITY"]

Gate B evaluation:
- Flow candidate: FullReleaseGateFlow71_v2 (probationary — new four-predicate composite flow)
- Gate B score: 0.71
- Gate B threshold: 0.85
- Gate B result: FAIL

Nine-node trace (probationary_rejected path):
N01: predicates = ["QA", "IMPLEMENT", "DOCUMENT", "SECURITY"]
N02: FullReleaseGateFlow71_v2 (probationary) selected as candidate
N03: Gate B evaluation begins
N04: Gate B score computed: 0.71 (below threshold 0.85)
N05: Gate B FAIL detected; probationary flow REJECTED
N06: Fallback selected: individual deterministic_generic flows for each predicate
N07: Fallback executes: [BackendServiceFlow41, DocumentationFlow, SecurityAuditFlow, ReleaseGateFlow71]
N08: All four flows execute in canonical order: IMPLEMENT → DOCUMENT → SECURITY → QA
N09 Result:
  - FlowType: probationary_rejected (fallback executed as deterministic_generic)
  - TransitionType: feature_addition
  - Files: [service_review.md, release_criteria.md, security_report.md, release_gate_decision.md, compliance_check.md, sign_off_register.md]
  - FileCount: 6
  - ResultState: full_release_gate_complete_via_fallback
  - DecisionCount: 4

---

## B38-L2-010-002 — probationary_rejected path explanation

The probationary candidate (FullReleaseGateFlow71_v2) was a composite flow attempting to handle all four predicates in a single integrated flow. Gate B failed because the composite flow's confidence score was below threshold.

The fallback is to run each predicate's deterministic_generic flow independently.
This produces the correct output (same files, same routing), just from validated flows rather than the unvalidated composite.

The result_state is "probationary_rejected" but the output is still complete.

---

## B38-L2-010-003 — Why probationary_rejected is not a failure

probationary_rejected means:
- The new composite flow was not ready for production
- The existing deterministic flows handled the request correctly
- Output is correct and complete
- The user receives valid results

probationary_rejected IS a failure for:
- The new flow's promotion tracking (it was not promoted)
- The Gate B record (failure logged)

probationary_rejected is NOT a failure for:
- The user's request (handled by fallback)
- The output quality (same as deterministic_generic)
