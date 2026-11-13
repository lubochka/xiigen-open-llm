# Layer 0-D — Flow Factory Pairs (028)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B38-0D-028-001 — ChangelogAutomation flow factory: QA+IMPLEMENT+SECURITY

Scenario: ChangelogServiceSecurityApproval
Input: "Audit the security of the changelog generation service and approve the release."
PredicateSet: ["QA","IMPLEMENT","SECURITY"]

Flows created:
1. BackendServiceFlow41 (Phase 4.1, IMPLEMENT, order 1)
2. SecurityAuditFlow (Phase 3.4, SECURITY, order 2)
3. ReleaseGateFlow71 (Phase 7.1, QA, order 3 — last)

TransitionType: feature_addition
FlowCount: 3

---

## B38-0D-028-002 — MigrationGuide flow factory: IMPLEMENT+DOCUMENT (no QA)

Scenario: MigrationServiceDocumentation
Input: "Build the migration service and document the migration guide criteria."
PredicateSet: ["IMPLEMENT","DOCUMENT"]

Flows created:
1. BackendServiceFlow41 (Phase 4.1, IMPLEMENT, order 1)
2. DocumentationFlow (Phase 2.1, DOCUMENT, order 2)

Note: No QA trigger — ReleaseGateFlow71 not created. QA-last rule applies only when QA is present.
TransitionType: feature_addition
FlowCount: 2

---

## B38-0D-028-003 — SecurityAdvisory flow factory: DOCUMENT+SECURITY (no QA)

Scenario: SecurityAdvisoryDocumentation
Input: "Document the security advisory for the v7 release patch."
PredicateSet: ["DOCUMENT","SECURITY"]

Flows created:
1. DocumentationFlow (Phase 2.1, DOCUMENT, order 1)
2. SecurityAuditFlow (Phase 3.4, SECURITY, order 2)

Note: No QA trigger — ReleaseGateFlow71 not created.
TransitionType: no_contribution
FlowCount: 2

---

## B38-0D-028-004 — CVENotification flow factory: QA+IMPLEMENT+DOCUMENT+SECURITY

Scenario: CVEPatchServiceFullGate
Input: "Build the patch service, audit the CVE, document the security criteria, and approve the fix."
PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]

Flows created:
1. BackendServiceFlow41 (Phase 4.1, IMPLEMENT, order 1)
2. DocumentationFlow (Phase 2.1, DOCUMENT, order 2)
3. SecurityAuditFlow (Phase 3.4, SECURITY, order 3)
4. ReleaseGateFlow71 (Phase 7.1, QA, order 4 — last)

TransitionType: feature_addition
FlowCount: 4
