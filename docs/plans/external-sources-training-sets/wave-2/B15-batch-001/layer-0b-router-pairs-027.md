# Layer 0-B — Router Pairs (027)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-027-001 — InvoiceService: IMPLEMENT+DOCUMENT routing

A: Predicates: ["IMPLEMENT", "DOCUMENT"] from "Implement InvoiceService + document workflow."
B: Routes: ImplementationFlow41 (4.1) ‖ DocumentationFlow21 (2.1) | DOCUMENT parallel-eligible with IMPLEMENT | DecisionCount: 2

---

## B15-router-027-002 — InvoiceService: IMPLEMENT+API_CONTRACT routing

A: Predicates: ["IMPLEMENT", "API_CONTRACT"] from "Implement InvoiceService + data contracts."
B: Routes: APIContractFlow33 (3.3) → ImplementationFlow41 (4.1) | Ordering: 3.3 before 4.1 | DecisionCount: 2

---

## B15-router-027-003 — InvoiceService: IMPLEMENT+SECURITY routing

A: Predicates: ["IMPLEMENT", "SECURITY"] from "Implement InvoiceService + encrypt storage."
B: Routes: SecurityAuditFlow → ImplementationFlow41 (4.1) | Ordering: security before implementation | DecisionCount: 2

---

## B15-router-027-004 — DOCUMENT parallel eligibility rule

DOCUMENT routes to DocumentationFlow21 (sub-phase 2.1).
DocumentationFlow21 is parallel-eligible with ImplementationFlow41 (4.1) and TestingFlow51 (5.1).
It is NOT parallel-eligible with ArchitectureFlow31 (3.1) — architecture must complete before documentation.

---

## B15-router-027-005 — InvoiceService: full four-predicate route

A: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "DOCUMENT"].
B: Routes: SecurityAuditFlow → APIContractFlow33 (3.3) → ImplementationFlow41 (4.1) ‖ DocumentationFlow21 (2.1) | DecisionCount: 4
DOCUMENT runs in parallel with 4.1 after security and contract flows complete.
