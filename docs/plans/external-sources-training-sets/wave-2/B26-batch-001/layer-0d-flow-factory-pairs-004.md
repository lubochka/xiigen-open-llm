# Layer 0-D — Flow-Factory Pairs (004)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F004-001 — SECURITY → 6.3 (XSS scan)
Input: "Scan the CustomerFeedback form for XSS vulnerabilities."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
  Factory: FlowFactory.Create("SECURITY", "6.3") → SecurityAuditFlow63

## F004-002 — SECURITY+QA+AUTHORIZATION → 3 flows
Input: "QA the RBAC implementation, audit privilege escalation paths, verify admin routes are locked."
  CompiledPredicateIds: ["QA", "SECURITY", "AUTHORIZATION", "TEST"] | SubPhases: [7.3, 6.3, 6.3, 5.1]
  Factory: FlowFactory.CreateMulti(["QA","SECURITY","AUTHORIZATION","TEST"]) → 4 flows

## F004-003 — SECURITY+IMPLEMENT (CSP headers)
Input: "Implement Content Security Policy headers on the CustomerPortal to block XSS."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"] | SubPhases: [5.2, 4.1]
  Factory: FlowFactory.CreateMulti(["SECURITY","IMPLEMENT"]) → [SecurityTestingFlow52, FeatureImplementationFlow41]
  Note: CSP implementation = enforcement context → 5.2 not 6.3

## F004-004 — SECURITY+DOCUMENT (threat model)
Input: "Create a threat model for the new DataIngestionPipeline."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "DOCUMENT"]
  Evidence: "create" → IMPLEMENT; "threat model" → SECURITY; "document" implicit
  Factory: FlowFactory.CreateMulti(["SECURITY","IMPLEMENT"]) → [SecurityAuditFlow63, FeatureImplementationFlow41]

## F004-005 — SECURITY+QA+IMPLEMENT+TEST+DOCUMENT → 5 flows (max complexity)
Input: "Implement OWASP Top 10 mitigations, write security tests, QA the controls, audit exposure, and document findings."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "TEST", "QA", "DOCUMENT"] | SubPhases: [6.3/5.2, 4.1, 5.1, 7.3, 6.2]
  Factory: FlowFactory.CreateMulti → 5 flows | ExecutionMode: sequential
