# Layer 0-B — Router Pairs (039)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-039-001 — MigrationGuide routing: DOCUMENT alone

Input: "Document the migration guide for upgrading from v1 to v2."
- PredicateSet: ["DOCUMENT"]
- Routing: DOCUMENT → Phase 2.1 (DocumentationFlow)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-039-002 — MigrationGuide routing: QA+DOCUMENT

Input: "Review the migration guide, verify it covers all exit criteria, and sign off."
- PredicateSet: ["QA","DOCUMENT"]
- Routing:
  1. DOCUMENT → Phase 2.1 (DocumentationFlow)
  2. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 2
- TransitionType: no_contribution

---

## B38-0B-039-003 — MigrationGuide routing: QA+SECURITY

Input: "Audit the migration guide for security implications and approve the migration plan."
- PredicateSet: ["QA","SECURITY"]
- Routing:
  1. SECURITY → Phase 3.4 (SecurityAuditFlow)
  2. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 2
- TransitionType: no_contribution

---

## B38-0B-039-004 — MigrationGuide routing: IMPLEMENT+DOCUMENT (no QA)

Input: "Build the migration service and document the migration guide criteria for the release."
- PredicateSet: ["IMPLEMENT","DOCUMENT"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
- DecisionCount: 2
- TransitionType: feature_addition
- Note: No QA trigger in this input — QA-last rule applies only when QA is present.

---

## B38-0B-039-005 — MigrationGuide routing: QA+IMPLEMENT+DOCUMENT+SECURITY

Input: "Review the migration service, audit security of the migration path, document exit criteria, and approve the release."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. SECURITY → Phase 3.4 (SecurityAuditFlow)
  4. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 4
- TransitionType: feature_addition
