# Layer 0-B — Router Pairs (007)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: router

---

## B38-0B-007-001 — [QA, DOCUMENT] → 7.1 + 2.1 (acceptance criteria)

PredicateSet: ["QA", "DOCUMENT"]
Scenario: AcceptanceCriteriaVerification
Input: "Verify the acceptance criteria are met and sign off on the feature release."

Routing decisions:
1. DOCUMENT → Phase 2.1 (DocumentationFlow)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

RoutingTable:
| Predicate | Target Phase | Flow | Order |
|---|---|---|---|
| DOCUMENT | 2.1 | DocumentationFlow | 1 |
| QA | 7.1 | ReleaseGateFlow71 | 2 (last) |

DecisionCount: 2
TransitionType: no_contribution
CapabilityGapWarnings: []
OutputFiles: [acceptance_criteria_doc.md, release_gate_report.md]
DomainId: software-development

---

## B38-0B-007-002 — Why acceptance criteria routes to 2.1, not 7.1

"Acceptance criteria" fires DOCUMENT. DOCUMENT always routes to Phase 2 (Rule 14).
In Phase 2.1 specifically: technical requirements and specification documents.
Acceptance criteria is a technical specification artifact → Phase 2.1.

Phase 7.1 (Release gate) handles the approval decision (QA predicate).
Phase 2.1 handles the criteria documentation artifact (DOCUMENT predicate).

They are complementary: 2.1 produces the criteria document; 7.1 verifies it for release.

---

## B38-0B-007-003 — [QA, DOCUMENT] scenarios: all route the same way

All inputs that fire [QA, DOCUMENT] route identically:
- Acceptance criteria → DOCUMENT → 2.1
- Release criteria → DOCUMENT → 2.1
- Exit criteria → DOCUMENT → 2.1
- Completion criteria → DOCUMENT → 2.1
- Readiness criteria → DOCUMENT → 2.1

Plus QA from any QA-lexicon term → 7.1.

The criteria qualifier does not change the routing. The routing destination is always 2.1 for DOCUMENT in B38.

---

## B38-0B-007-004 — no_contribution for [QA, DOCUMENT] explained

TransitionType = no_contribution for [QA, DOCUMENT] because:
1. QA → no_contribution (quality gate, not code authoring)
2. DOCUMENT → documentation (also not a code feature addition)

Neither predicate implies new code is being written. The "contribution" tracked in TransitionType refers to code/feature contribution, not documentation contribution. A documentation update routes to Phase 2 but does not count as feature_addition in TransitionType terms.

feature_addition requires IMPLEMENT in the predicate set.
