# Layer 0-D — Flow Factory Pairs (002)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B38-0D-002-001 — deterministic_generic: 7.1 (QA alone, no_contribution)

FlowType: deterministic_generic
Scenario: ReleaseSignOffCollection (sign-off scenario, no IMPLEMENT)
PredicateSet: ["QA"]

Factory output:
{
  "flow_id": "ReleaseGateFlow71",
  "flow_type": "deterministic_generic",
  "phase": "7.1",
  "predicate": "QA",
  "transition_type": "no_contribution",
  "gate_b_required": false,
  "entry_state": "release_gate_pending",
  "exit_state": "release_gate_closed"
}

---

## B38-0D-002-002 — deterministic_generic properties

deterministic_generic flows in B38:
- Selected when: PredicateSet contains only QA (no IMPLEMENT → no probationary consideration needed)
- Gate B: not required for deterministic flows
- Always executes: no confidence threshold needed
- Output: predictable, repeatable
- TransitionType: no_contribution

When does deterministic_generic apply for B38?
- Any QA-only scenario: sign-off, approval, verify-only, audit-only
- DOCUMENT-only scenario (if no QA, which is rare)
- QA+DOCUMENT, QA+SECURITY (no IMPLEMENT)

---

## B38-0D-002-003 — deterministic_generic vs probationary: selection criteria

Select deterministic_generic when:
- Flow pattern is well-established (many prior successful executions)
- Predicate set does not contain novel combinations
- No Gate B evaluation required
- All N01-N09 paths have been validated

Select probationary when:
- Flow candidate is new or recently promoted
- Gate B confidence threshold must be verified
- Marked as probationary in flow registry

In B38 batch-001, ReleaseGateFlow71 is treated as deterministic_generic for all QA-only scenarios.

---

## B38-0D-002-004 — Factory map for no_contribution flows

| FlowId | Type | Phase | Predicate | TransitionType |
|---|---|---|---|---|
| ReleaseGateFlow71 | deterministic_generic | 7.1 | QA | no_contribution |
| DocumentationFlow | deterministic_generic | 2.1 | DOCUMENT | no_contribution |
| SecurityAuditFlow | deterministic_generic | 3.4 | SECURITY | no_contribution |

All three are deterministic_generic and produce no_contribution when IMPLEMENT is absent from the predicate set.
