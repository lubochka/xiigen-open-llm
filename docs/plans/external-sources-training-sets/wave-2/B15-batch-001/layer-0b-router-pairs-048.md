# Layer 0-B — Router Pairs (048)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-048-001 — Full pipeline routing for KYCService (037–048 reference case)

A: "Create KYCService with a KYC domain model, wire up the identity verification API, secure the document upload, write identity check tests, and document the verification flow."
B: Flow: ArchitectureFlow31 → APIContractFlow33 → SecurityAuditFlow → ImplementationFlow41 → TestingFlow51 ‖ DocumentationFlow21 | DecisionCount: 6
Ordering confirmed: 3.1 → 3.3 → security → 4.1 → 5.1 ‖ 2.1

---

## B15-router-048-002 — Full pipeline routing for AnonymizationService (IMPLEMENT+SECURITY reference)

A: "Build AnonymizationService, protect the raw data endpoint, and encrypt the PII mapping table."
B: Flow: SecurityAuditFlow → ImplementationFlow41 | DecisionCount: 2
Two SECURITY triggers → deduplication → one SecurityAuditFlow routing decision.

---

## B15-router-048-003 — TransitionType propagation confirmed for 037–048 routing

TransitionType: refactoring is passed only to ImplementationFlow41 factory call.
All other flow factory calls (ArchitectureFlow31, APIContractFlow33, SecurityAuditFlow, TestingFlow51, DocumentationFlow21) receive no TransitionType metadata.

---

## B15-router-048-004 — "refactor" verb — routing unchanged, TransitionType changes

"refactor" → IMPLEMENT → ImplementationFlow41 (same flow as feature_addition).
TransitionType: refactoring modifies ImplementationFlow41 internal behavior (backward-compatibility required).
The routing decision (which flow is selected) is identical for refactoring and feature_addition.

---

## B15-router-048-005 — Anti-pattern: routing SECURITY after IMPLEMENT

Anti-pattern confirmed across all 037–048 services:
SecurityAuditFlow must always precede ImplementationFlow41 when SECURITY is present.
Correct order: SecurityAuditFlow → ImplementationFlow41. Wrong order: ImplementationFlow41 → SecurityAuditFlow.
