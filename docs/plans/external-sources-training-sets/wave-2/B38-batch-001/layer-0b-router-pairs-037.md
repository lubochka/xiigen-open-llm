# Layer 0-B — Router Pairs (037)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-037-001 — ChangelogAutomation routing: knowledge_edge

Input: "Generate the changelog automatically from commit messages."
- PredicateSet: []
- Routing: no flows created
- ResultState: knowledge_edge

---

## B38-0B-037-002 — ChangelogAutomation routing: QA alone

Input: "Review the auto-generated changelog and approve it for release."
- PredicateSet: ["QA"]
- Routing: QA → Phase 7.1 (ReleaseGateFlow71)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-037-003 — ChangelogAutomation routing: IMPLEMENT alone

Input: "Build the changelog automation pipeline for the release service."
- PredicateSet: ["IMPLEMENT"]
- Routing: IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
- DecisionCount: 1
- TransitionType: feature_addition

---

## B38-0B-037-004 — ChangelogAutomation routing: QA+DOCUMENT

Input: "Document the changelog entry criteria and validate the output format."
- PredicateSet: ["QA","DOCUMENT"]
- Routing:
  1. DOCUMENT → Phase 2.1 (DocumentationFlow)
  2. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 2
- TransitionType: no_contribution

---

## B38-0B-037-005 — ChangelogAutomation routing: QA+IMPLEMENT+SECURITY

Input: "Audit the security of the changelog generation service and approve the release."
- PredicateSet: ["QA","IMPLEMENT","SECURITY"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. SECURITY → Phase 3.4 (SecurityAuditFlow)
  3. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 3
- TransitionType: feature_addition
