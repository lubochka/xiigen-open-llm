# Layer 0-B — Router Pairs (038)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-038-001 — ReleaseNoteTemplate routing: DOCUMENT alone

Input: "Create a release note template for all future releases."
- PredicateSet: ["DOCUMENT"]
- Routing: DOCUMENT → Phase 2.1 (DocumentationFlow)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-038-002 — ReleaseNoteTemplate routing: QA alone

Input: "Approve the release note template before distribution."
- PredicateSet: ["QA"]
- Routing: QA → Phase 7.1 (ReleaseGateFlow71)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-038-003 — ReleaseNoteTemplate routing: IMPLEMENT alone

Input: "Build the release note template service for the documentation pipeline."
- PredicateSet: ["IMPLEMENT"]
- Routing: IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
- DecisionCount: 1
- TransitionType: feature_addition

---

## B38-0B-038-004 — ReleaseNoteTemplate routing: QA+DOCUMENT

Input: "Document the release note entry criteria and sign off the template."
- PredicateSet: ["QA","DOCUMENT"]
- Routing:
  1. DOCUMENT → Phase 2.1 (DocumentationFlow)
  2. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 2
- TransitionType: no_contribution

---

## B38-0B-038-005 — ReleaseNoteTemplate routing: QA+IMPLEMENT+DOCUMENT

Input: "Review the release note template against acceptance criteria, build the template service, and approve for release."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 3
- TransitionType: feature_addition
