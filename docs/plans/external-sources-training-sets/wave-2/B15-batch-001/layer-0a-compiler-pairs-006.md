# Layer 0-A — Compiler Pairs (006)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: compiler

---

## B15-compiler-006-001 — "extend" + feature → IMPLEMENT

A: "Extend the order service to support multi-currency order totals."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: extend + service feature → IMPLEMENT (extending software functionality).

---

## B15-compiler-006-002 — "implement" + ARCHITECTURE (service layer design)

A: "Implement the order service and design the service layer architecture."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; design the service layer architecture → ARCHITECTURE.

---

## B15-compiler-006-003 — "create" + repository → IMPLEMENT

A: "Create the order repository with CRUD (Create/Read/Update/Delete) operations and pagination."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: create + repository → IMPLEMENT. Repository = software component (data access layer).

---

## B15-compiler-006-004 — "wire up" → API_CONTRACT

A: "Build the order handler and wire up the external tax calculation API."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; "wire up external API" → API_CONTRACT (wire up = external integration verb).

---

## B15-compiler-006-005 — IMPLEMENT+TEST+DOCUMENT+QA: four predicates

A: "Build the refund processing service, write integration tests, document the refund policy API, and QA the refund workflow."
B: Predicates: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; write integration tests → TEST (5.3); document → DOCUMENT; QA → QA.
