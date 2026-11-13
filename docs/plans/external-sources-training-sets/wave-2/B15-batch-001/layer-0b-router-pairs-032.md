# Layer 0-B — Router Pairs (032)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-032-001 — CreditService: IMPLEMENT single route

A: Predicates: ["IMPLEMENT"] from "Implement CreditService."
B: Route: ImplementationFlow41 (sub-phase 4.1) | DecisionCount: 1

---

## B15-router-032-002 — CreditService: IMPLEMENT+API_CONTRACT routing

A: Predicates: ["IMPLEMENT", "API_CONTRACT"] from "Implement CreditService + API contracts."
B: Routes: APIContractFlow33 (3.3) → ImplementationFlow41 (4.1) | DecisionCount: 2

---

## B15-router-032-003 — CreditService: IMPLEMENT+ARCHITECTURE routing

A: Predicates: ["IMPLEMENT", "ARCHITECTURE"] from "Implement CreditService + architect credit ledger model."
B: Routes: ArchitectureFlow31 (3.1) → ImplementationFlow41 (4.1) | DecisionCount: 2

---

## B15-router-032-004 — CreditService: IMPLEMENT+SECURITY+DOCUMENT three routes

A: Predicates: ["IMPLEMENT", "SECURITY", "DOCUMENT"] from "Implement + secure endpoints + document policy."
B: Routes: SecurityAuditFlow → ImplementationFlow41 (4.1) ‖ DocumentationFlow21 (2.1) | DecisionCount: 3
DOCUMENT parallel with 4.1 after security completes.

---

## B15-router-032-005 — Router: all QA routes are last

When QA is present: QAGatingFlow71 (sub-phase 7.1) or ReleaseGateFlow73 (sub-phase 7.3) always placed last.
Example: ["IMPLEMENT", "API_CONTRACT", "QA"] → APIContractFlow33 (3.3) → ImplementationFlow41 (4.1) → QAGatingFlow71 (7.1)
QA absolute last ordering cannot be changed by any other ordering rule.
