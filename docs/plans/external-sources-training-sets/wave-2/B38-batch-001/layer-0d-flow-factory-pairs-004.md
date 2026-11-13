# Layer 0-D — Flow Factory Pairs (004)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B38-0D-004-001 — trusted_learned: 7.1 promoted flow

FlowType: trusted_learned
Scenario: ReleaseGateFlow71 (promoted from probationary after sustained success)
PredicateSet: ["QA"] or ["QA", "IMPLEMENT"]

Factory output:
{
  "flow_id": "ReleaseGateFlow71",
  "flow_type": "trusted_learned",
  "phase": "7.1",
  "promotion_source": "probationary_accepted",
  "promotion_count": 12,
  "gate_b_required": false,
  "entry_state": "trusted_active",
  "exit_state": "trusted_complete"
}

---

## B38-0D-004-002 — trusted_learned properties

trusted_learned flows in B38:
- Promoted from probationary after repeated successful Gate B passes
- Gate B no longer required at execution time (trust established)
- Operates identically to deterministic_generic from a routing perspective
- Distinguished in flow registry: trusted_learned vs deterministic_generic tracks origin
- Carries promotion history metadata

---

## B38-0D-004-003 — Flow type hierarchy for B38

probationary → probationary_accepted → trusted_learned
deterministic_generic (original validated flows, not promoted from probationary)

At routing time:
- trusted_learned and deterministic_generic behave identically
- The distinction is in the registry record (origin tracking)
- TransitionType, phase routing, and output are the same

---

## B38-0D-004-004 — Trusted flows in four-predicate scenarios

When all four predicates fire ["QA","IMPLEMENT","DOCUMENT","SECURITY"]:
- Each predicate's flow has its own flow type status
- ReleaseGateFlow71 (QA) may be trusted_learned
- BackendServiceFlow41 (IMPLEMENT) may be deterministic_generic
- DocumentationFlow (DOCUMENT) may be deterministic_generic
- SecurityAuditFlow (SECURITY) may be deterministic_generic

Each flow's type is independent. A multi-predicate scenario may combine flows of different types. The overall scenario is only probationary_rejected if any active probationary flow fails Gate B.
