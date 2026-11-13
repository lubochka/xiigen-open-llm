# Layer 0-B — Router Pairs (044)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-044-001 — ClientCommunication routing: DOCUMENT alone

Input: "Document the client communication plan for the v5 release."
- PredicateSet: ["DOCUMENT"]
- Routing: DOCUMENT → Phase 2.1 (DocumentationFlow)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-044-002 — ClientCommunication routing: knowledge_edge

Input: "Send the client communication email about the v5 release."
- PredicateSet: []
- Routing: no flows created
- ResultState: knowledge_edge

---

## B38-0B-044-003 — ClientCommunication routing: QA alone

Input: "Review the client communication template and approve it for release distribution."
- PredicateSet: ["QA"]
- CapabilityGapWarnings: ["distribution"]
- Routing: QA → Phase 7.1 (ReleaseGateFlow71)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-044-004 — ClientCommunication routing: QA+DOCUMENT

Input: "Document the communication release criteria and sign off the client notice."
- PredicateSet: ["QA","DOCUMENT"]
- Routing:
  1. DOCUMENT → Phase 2.1 (DocumentationFlow)
  2. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 2
- TransitionType: no_contribution

---

## B38-0B-044-005 — ClientCommunication routing: QA+IMPLEMENT+DOCUMENT

Input: "Build the client notification service, document the communication criteria, and validate the messaging."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 3
- TransitionType: feature_addition
