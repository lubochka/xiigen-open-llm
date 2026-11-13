# Layer 0-B — Router Pairs (045)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-045-001 — InternalAnnouncement routing: DOCUMENT alone

Input: "Document the internal announcement for the v6 release."
- PredicateSet: ["DOCUMENT"]
- Routing: DOCUMENT → Phase 2.1 (DocumentationFlow)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-045-002 — InternalAnnouncement routing: knowledge_edge

Input: "Post the internal announcement on the company wiki."
- PredicateSet: []
- Routing: no flows created
- ResultState: knowledge_edge

---

## B38-0B-045-003 — InternalAnnouncement routing: QA alone

Input: "Review the internal release announcement and approve it before distribution."
- PredicateSet: ["QA"]
- CapabilityGapWarnings: ["distribution"]
- Routing: QA → Phase 7.1 (ReleaseGateFlow71)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-045-004 — InternalAnnouncement routing: QA+DOCUMENT

Input: "Document the announcement release criteria and sign off the internal notice."
- PredicateSet: ["QA","DOCUMENT"]
- Routing:
  1. DOCUMENT → Phase 2.1 (DocumentationFlow)
  2. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 2
- TransitionType: no_contribution

---

## B38-0B-045-005 — InternalAnnouncement routing: IMPLEMENT+DOCUMENT (no QA)

Input: "Build the internal announcement service and document the announcement template criteria."
- PredicateSet: ["IMPLEMENT","DOCUMENT"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
- DecisionCount: 2
- TransitionType: feature_addition
