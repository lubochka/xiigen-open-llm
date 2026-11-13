# Layer 0-B — Router Pairs (037)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-037-001 — CustomerProfileService: IMPLEMENT routing

A: "Build a CustomerProfileService that stores and retrieves customer data."
B: Flow: ImplementationFlow41 | DecisionCount: 1 | NearestWrong: 4.2 Integration (0.40)

---

## B15-router-037-002 — CustomerProfileService: IMPLEMENT+SECURITY routing

A: "Build CustomerProfileService and secure the customer PII endpoints."
B: Flow: SecurityAuditFlow → ImplementationFlow41 | DecisionCount: 2 | Ordering: SECURITY before IMPLEMENT

---

## B15-router-037-003 — CustomerProfileService: IMPLEMENT+ARCHITECTURE routing

A: "Build CustomerProfileService with a profile domain model."
B: Flow: ArchitectureFlow31 → ImplementationFlow41 | DecisionCount: 2 | Ordering: ARCHITECTURE (3.1) before IMPLEMENT (4.1)

---

## B15-router-037-004 — CustomerProfileService: IMPLEMENT+API_CONTRACT routing

A: "Build CustomerProfileService and integrate with the identity provider."
B: Flow: APIContractFlow33 → ImplementationFlow41 | DecisionCount: 2 | Ordering: API_CONTRACT (3.3) before IMPLEMENT (4.1)

---

## B15-router-037-005 — Routing order confirmed for 037–048 services

All 037–048 services follow the same routing order:
ARCHITECTURE (3.1) → API_CONTRACT (3.3) → SECURITY → IMPLEMENT (4.1) → TEST (5.1) → DOCUMENT (2.1) → QA (7.x)
TransitionType propagated unchanged to all routing decisions.
