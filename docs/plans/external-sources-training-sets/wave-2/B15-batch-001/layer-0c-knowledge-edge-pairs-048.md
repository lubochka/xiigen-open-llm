# Layer 0-C — Knowledge-Edge Pairs (048)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-048-001 — Complete ARCHITECTURE trigger reference for 037–048 range

| Service | Confirmed ARCHITECTURE triggers |
|---------|--------------------------------|
| CustomerProfileService | "profile domain model", "customer profile model", "bounded context" |
| AddressValidationService | "address domain model", "address validation model" |
| KYCService | "KYC domain model", "identity verification model", "identity state machine" + architect verb |
| ComplianceService | "compliance domain model", "regulatory model" |
| ConsentService | "consent domain model", "consent state machine" + architect verb |
| DataRetentionService | "data lifecycle domain model", "retention policy model" |
| AnonymizationService | "anonymization domain model", "PII field category model" |
| ExportService | "export domain model", "export format model" |
| ImportService | "import domain model", "data ingestion model" |
| MigrationService | "migration domain model", "entity mapping model" |
| SyncService | "sync domain model", "conflict resolution model" |

---

## B15-kedge-048-002 — Complete API_CONTRACT trigger reference for 037–048 range

"integrate with [X]", "wire up [X] API", "connect to [X]" → API_CONTRACT for all 037–048 services.
"[service] contracts", "data contracts", "API contracts" → API_CONTRACT.
"define [X] contracts" → API_CONTRACT + DOCUMENT (simultaneous dual-predicate from one clause).

---

## B15-kedge-048-003 — "define" fires both API_CONTRACT and DOCUMENT (confirmed 037–048)

"define the [X] contracts" → API_CONTRACT (from "contracts") + DOCUMENT (from "define").
This dual-predicate rule applies to all 037–048 services when "define ... contracts" pattern is present.

---

## B15-kedge-048-004 — Spurious predicate risks for 037–048 range (v6-02 inverse)

| Trigger | Spurious Predicate Risk | Correct Result |
|---------|------------------------|----------------|
| "enforce compliance rules" | SECURITY incorrectly added | IMPLEMENT only |
| "GDPR requirements" | SECURITY incorrectly added | IMPLEMENT only |
| "profile management" | ARCHITECTURE incorrectly added | IMPLEMENT only |
| "import process" | ARCHITECTURE incorrectly added | IMPLEMENT only |

---

## B15-kedge-048-005 — "domain model" pattern is necessary and sufficient for ARCHITECTURE

The "[X] domain model" pattern (where [X] is any domain noun) is sufficient to fire ARCHITECTURE.
No other domain noun combination fires ARCHITECTURE without explicit "domain model", "bounded context", or "architect" verb.
Confirmed across all 037–048 services.
