# Layer 0-B — Router Pairs (005)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: router

---

## B15-router-005-001 — TEST routing: 5.1 vs 5.3

"Write unit tests for the pricing algorithm" → UnitTestingFlow51 (5.1: unit + algorithm = pure function)
"Write E2E tests for the checkout flow" → E2ETestingFlow53 (5.3: E2E + user flow)
"Write integration tests for the order API" → E2ETestingFlow53 (5.3: integration)

---

## B15-router-005-002 — QA routing: 7.1 vs 7.3

"QA the order service before release to staging" → ReleaseGateFlow71 (7.1: staging/release)
"QA the order feature before shipping" → QAGatingFlow73 (7.3: feature acceptance)
"QA sign-off on the payment module" → QAGatingFlow73 (7.3: feature gate)

---

## B15-router-005-003 — ARCHITECTURE sub-phase: 3.1 vs 3.5

"Design the order service architecture" → ArchitectureFlow31 (3.1: application-level)
"Design the infrastructure for the order processing cluster" → ArchitectureFlow35 (3.5: infrastructure)

---

## B15-router-005-004 — IMPLEMENT+DOCUMENT+QA routing

A: "Build the order report service, document the report API, and QA before launch."
Router: Predicates = ["IMPLEMENT", "DOCUMENT", "QA"]
Routing: ImplementationFlow41 ‖ DocumentationFlow → QAGatingFlow73 (QA after both complete)

---

## B15-router-005-005 — v6 corruption: API_CONTRACT invented flow

v6 corrupt: "Build order service with data contracts" → Predicates: ["IMPLEMENT"] only
Correct: ["IMPLEMENT", "API_CONTRACT"] → ImplementationFlow41 + APIContractFlow33
Error: "data contracts" → API_CONTRACT always fires on "contract" keyword.
