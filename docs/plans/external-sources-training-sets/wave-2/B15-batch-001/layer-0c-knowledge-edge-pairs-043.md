# Layer 0-C — Knowledge-Edge Pairs (043)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-043-001 — AnonymizationService: ARCHITECTURE triggers

Confirmed ARCHITECTURE triggers:
- "anonymization domain model" → ARCHITECTURE
- "PII field category model" → ARCHITECTURE
- "bounded context" (anonymization bounded context) → ARCHITECTURE

---

## B15-kedge-043-002 — AnonymizationService: SECURITY triggers (two triggers, one predicate)

Confirmed SECURITY triggers (both fire SECURITY; deduplication applies):
- "protect the raw data access endpoint" → SECURITY
- "encrypt the PII mapping table" → SECURITY
Both triggers present → one SECURITY predicate (deduplication).

---

## B15-kedge-043-003 — AnonymizationService: TEST trigger

"write tests for PII masking correctness" → TEST.
"write unit tests for all anonymization rules" → TEST.

---

## B15-kedge-043-004 — "mask" and "redact" are IMPLEMENT trigger verbs

"mask" and "redact" as code artifact creation verbs fire IMPLEMENT.
"Mask PII fields before export" → IMPLEMENT. "Redact sensitive fields in audit logs" → IMPLEMENT.

---

## B15-kedge-043-005 — SECURITY deduplication rule confirmed for AnonymizationService

Multiple SECURITY triggers in one request → single SECURITY predicate → single SecurityAuditFlow call.
This rule applies to ALL services in the B15 scenario — not just AnonymizationService.
Anti-pattern: emitting two separate SECURITY entries for "protect" and "encrypt" in the same request.
