# Layer 0-A — Compiler Pairs (011)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: compiler

---

## B15-compiler-011-001 — "implement" + service bus → IMPLEMENT

A: "Implement the order event dispatcher on the service bus."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition

---

## B15-compiler-011-002 — IMPLEMENT+SECURITY+AUTHORIZATION: two security predicates

A: "Build the order admin service, secure sensitive order data, and add role-based access for managers."
B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; secure sensitive data → SECURITY; add role-based access → AUTHORIZATION.

---

## B15-compiler-011-003 — "implement" + TEST (E2E): order workflow

A: "Implement the order confirmation flow and write E2E tests for the complete order journey."
B: Predicates: ["IMPLEMENT", "TEST"] | TransitionType: feature_addition
TEST routes to: E2ETestingFlow53 (E2E + complete journey → 5.3)

---

## B15-compiler-011-004 — Deduplication: three IMPLEMENT triggers → one predicate

A: "Build the order service, implement the fulfillment handler, and create the shipment tracker."
B: Predicates: ["IMPLEMENT"] (three triggers → deduplicated to one)
Evidence: build + implement + create all fire IMPLEMENT; deduplication → single predicate.

---

## B15-compiler-011-005 — IMPLEMENT+API_CONTRACT+SECURITY+TEST+DOCUMENT+QA: six

A: "Build the checkout service, integrate with PayPal, secure payment data, write tests, document the API, and QA before launch."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT", "QA"] | TransitionType: feature_addition
QA last.
