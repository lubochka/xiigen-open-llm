# Layer 0-B — Router Pairs (002)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: router

---

## B38-0B-002-001 — [QA] alone routes to 7.1 only

PredicateSet: ["QA"]
Scenario: ReleaseSignOffCollection
Input: "Collect the sign-offs for the release and confirm the launch is approved."

Routing decisions:
1. QA → Phase 7.1 (Release and Deployment Automation) → ReleaseGateFlow71

RoutingTable:
| Predicate | Target Phase | Flow | Order |
|---|---|---|---|
| QA | 7.1 | ReleaseGateFlow71 | 1 (only) |

DecisionCount: 1
TransitionType: no_contribution
OutputFiles: [sign_off_register.md, release_gate_report.md]
DomainId: software-development

Note: "launch" produced a CapabilityGapWarning but did NOT generate a routing decision. Gap warnings are metadata, not routing entries.

---

## B38-0B-002-002 — Single QA routing: sign-off register scenario

PredicateSet: ["QA"]
Scenario: Post-release sign-off collection

Routing:
- Phase 7.1 only
- Flow: ReleaseGateFlow71 (deterministic_generic)
- N09 state: SignOffRegister written; release gate closed

What does NOT happen:
- No routing to Phase 4 (no IMPLEMENT)
- No routing to Phase 2 (no DOCUMENT)
- No routing to Phase 3 (no SECURITY)
- No GapHandler invoked (QA fired, so not knowledge_edge)

---

## B38-0B-002-003 — Single QA: ordering rule trivially satisfied (only one predicate)

When PredicateSet = ["QA"], the QA ordering rule (QA last) is satisfied trivially because there is only one predicate. No ordering calculation needed.

If a v6-03 corruption were to add phantom IMPLEMENT to this result, the incorrect output would be:
- Routing to 7.1 + 4.1
- DecisionCount: 2
- TransitionType: feature_addition

The correct output:
- Routing to 7.1 only
- DecisionCount: 1
- TransitionType: no_contribution

Detection method: check that "service", "build", "pipeline", or "implementation" actually appears in the input before accepting IMPLEMENT in the predicate set.

---

## B38-0B-002-004 — QA alone: no contribution path explanation

TransitionType = no_contribution means:
- The QA predicate fires and produces a quality gate check
- No new code is authored (no IMPLEMENT)
- No new documentation is written (no DOCUMENT)
- The contribution to the codebase is a review/approval record only
- N09 node output: 2 files (gate report + sign-off register)

This is expected and correct for pure sign-off scenarios in B38. The absence of feature_addition is not a failure condition — it is the correct classification for sign-off-only flows.
