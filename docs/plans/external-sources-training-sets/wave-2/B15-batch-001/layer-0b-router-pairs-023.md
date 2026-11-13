# Layer 0-B — Router Pairs (023)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-023-001 — TaxService IMPLEMENT → ImplementationFlow41

A: Predicates: ["IMPLEMENT"] | Request: "Implement the tax calculation service."
Router:
  - PredictedSubPhaseId: "4.1" | Confidence: 0.92 | DecisionCount: 1

---

## B15-router-023-002 — IMPLEMENT+ARCHITECTURE → two flows (TaxService domain model)

A: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | Request: "Build the tax service and design the domain model for tax jurisdiction rules."
Router:
  - ArchitectureFlow31 | ImplementationFlow41
  DecisionCount: 2

---

## B15-router-023-003 — ARCHITECTURE+IMPLEMENT → two flows (architecture verb first)

A: Predicates: ["ARCHITECTURE", "IMPLEMENT"] | Request: "Architect the tax calculation engine and implement the rate lookup logic."
Router:
  - ArchitectureFlow31 | ImplementationFlow41
  DecisionCount: 2 | Note: Same flows regardless of which predicate is listed first in request — routing is by predicate, not request order.

---

## B15-router-023-004 — ARCHITECTURE+API_CONTRACT+IMPLEMENT+DOCUMENT → four flows

A: Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "DOCUMENT"] | Request: "Architect the tax service domain model, define the tax API contracts, implement the calculation engine, and document the tax rules."
Router:
  - ArchitectureFlow31 | APIContractFlow33 | ImplementationFlow41 | DocumentationFlow21
  DecisionCount: 4 | Ordering: 3.1 → 3.3 → 4.1 ‖ 2.1

---

## B15-router-023-005 — ARCHITECTURE+API_CONTRACT+IMPLEMENT+SECURITY+TEST+DOCUMENT → six flows

A: Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT"] | Request: "Design tax domain model, define tax API contracts, build tax service, secure the tax data endpoints, write tax calculation tests, and document the tax rules."
Router:
  - ArchitectureFlow31 | APIContractFlow33 | SecurityAuditFlow | ImplementationFlow41 | TestingFlow51 | DocumentationFlow21
  DecisionCount: 6 | QA absent (not explicitly requested).
