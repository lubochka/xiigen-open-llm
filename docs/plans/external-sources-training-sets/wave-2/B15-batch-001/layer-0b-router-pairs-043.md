# Layer 0-B — Router Pairs (043)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-043-001 — AnonymizationService: IMPLEMENT routing

A: "Build an AnonymizationService to mask PII fields before export."
B: Flow: ImplementationFlow41 | DecisionCount: 1

---

## B15-router-043-002 — AnonymizationService: IMPLEMENT+SECURITY routing (single trigger)

A: "Build AnonymizationService and protect the raw data access endpoint."
B: Flow: SecurityAuditFlow → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-043-003 — AnonymizationService: IMPLEMENT+SECURITY routing (two triggers, deduplication)

A: "Build AnonymizationService, protect the raw data endpoint, and encrypt the PII mapping table."
B: Flow: SecurityAuditFlow → ImplementationFlow41 | DecisionCount: 2
Two SECURITY triggers (protect + encrypt) → deduplication → one SecurityAuditFlow call.

---

## B15-router-043-004 — AnonymizationService: IMPLEMENT+ARCHITECTURE routing

A: "Build AnonymizationService with an anonymization domain model covering all PII field categories."
B: Flow: ArchitectureFlow31 → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-043-005 — NearestWrong scores confirmed for 037–048 range

| Flow | NearestWrong | Score |
|------|-------------|-------|
| ImplementationFlow41 | 4.2 Integration | 0.40 |
| ArchitectureFlow31 | APIContractFlow33 | 0.38 |
| APIContractFlow33 | ArchitectureFlow31 | 0.36 |
| SecurityAuditFlow | APIContractFlow33 | 0.35 |
| TestingFlow51 | E2E Flow 5.3 | 0.42 |
| DocumentationFlow21 | APIContractFlow33 | 0.33 |
