# Layer 0-B — Router Pairs (039)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-039-001 — KYCService: IMPLEMENT routing

A: "Create a KYCService to verify customer identity documents."
B: Flow: ImplementationFlow41 | DecisionCount: 1 | NearestWrong: 4.2 Integration (0.40)

---

## B15-router-039-002 — KYCService: IMPLEMENT+SECURITY routing

A: "Create KYCService and secure the identity verification endpoints."
B: Flow: SecurityAuditFlow → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-039-003 — KYCService: IMPLEMENT+API_CONTRACT routing

A: "Create KYCService and wire up the third-party identity verification API."
B: Flow: APIContractFlow33 → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-039-004 — KYCService: IMPLEMENT+SECURITY+TEST routing

A: "Create KYCService, harden the document upload endpoint, and write tests for all identity checks."
B: Flow: SecurityAuditFlow → ImplementationFlow41 → TestingFlow51 | DecisionCount: 3
Ordering: SECURITY → IMPLEMENT → TEST

---

## B15-router-039-005 — SecurityAuditFlow position in multi-predicate routing confirmed

SecurityAuditFlow always routes BEFORE ImplementationFlow41 when SECURITY is present.
SecurityAuditFlow NearestWrong: APIContractFlow33 (3.3) at 0.35.
Anti-pattern: placing SecurityAuditFlow after ImplementationFlow41.
