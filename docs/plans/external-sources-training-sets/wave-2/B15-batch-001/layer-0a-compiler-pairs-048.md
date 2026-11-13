# Layer 0-A — Compiler Pairs (048)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-048-001 — IMPLEMENT+SECURITY complete reference for 037–048 range

All IMPLEMENT+SECURITY verified triggers across services 037–048:
- CustomerProfileService: "secure the PII endpoints" → SECURITY
- KYCService: "secure/harden document upload" → SECURITY
- ComplianceService: "secure audit log endpoints" → SECURITY
- ConsentService: "protect consent record modification" → SECURITY
- DataRetentionService: "encrypt archived data store" → SECURITY
- AnonymizationService: "protect raw data access / encrypt PII mapping" → SECURITY
- ExportService: "secure export download endpoint" → SECURITY
- ImportService: "authenticate import source connections" → SECURITY
- MigrationService: "encrypt data in transit" → SECURITY
- SyncService: "secure sync trigger endpoint" → SECURITY

---

## B15-compiler-048-002 — TransitionType inheritance for 037–048 range

All services in 037–048: TransitionType = feature_addition when IMPLEMENT trigger verb is not "refactor".
TransitionType = refactoring when "refactor" is the trigger verb (overrides all others).
SECURITY does not change TransitionType — SECURITY predicates always inherit the TransitionType from IMPLEMENT.

---

## B15-compiler-048-003 — "verify" fires IMPLEMENT, not SECURITY

"verify" in context of identity/compliance operations is an IMPLEMENT trigger verb.
"Verify customer identity documents" → IMPLEMENT (feature_addition).
SECURITY fires from: secure, harden, protect, encrypt, authenticate — not from "verify".

---

## B15-compiler-048-004 — Deduplication across 037–048 IMPLEMENT+SECURITY combos

If a request contains both "secure" and "harden" targeting the same service, only one SECURITY predicate is emitted.
Example: "Build KYCService, secure the identity verification endpoint, and harden the document upload endpoint" → ["IMPLEMENT", "SECURITY"] — not two SECURITY entries.

---

## B15-compiler-048-005 — Complete non-trigger reference for 037–048 domain nouns

Non-trigger domain nouns confirmed across 037–048:
"customer profile", "address", "KYC", "compliance", "consent", "GDPR", "retention", "anonymization", "export", "import", "migration", "sync", "PII" — none fire predicates when used as nouns alone.
