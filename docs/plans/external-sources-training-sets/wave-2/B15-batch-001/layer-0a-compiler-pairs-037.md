# Layer 0-A — Compiler Pairs (037)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-037-001 — CustomerProfileService: IMPLEMENT from "build"

A: "Build a CustomerProfileService that stores and retrieves customer data."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Confidence: 0.97
"build" → IMPLEMENT. "CustomerProfileService" is a domain noun — does not fire any predicate.

---

## B15-compiler-037-002 — CustomerProfileService: IMPLEMENT+ARCHITECTURE on "profile model"

A: "Build CustomerProfileService with a profile domain model."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
"build" → IMPLEMENT. "profile domain model" → ARCHITECTURE ("domain model" pattern fires regardless of domain noun prefix).

---

## B15-compiler-037-003 — CustomerProfileService: IMPLEMENT+SECURITY on "secure"

A: "Build CustomerProfileService and secure the customer PII (Personally Identifiable Information) endpoints."
B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
"build" → IMPLEMENT. "secure" → SECURITY.

---

## B15-compiler-037-004 — CustomerProfileService: IMPLEMENT+API_CONTRACT on "integrate"

A: "Build CustomerProfileService and integrate with the identity provider."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
"build" → IMPLEMENT. "integrate" → API_CONTRACT.

---

## B15-compiler-037-005 — CustomerProfileService: refactoring TransitionType

A: "Refactor CustomerProfileService to split read and write models."
B: Predicates: ["IMPLEMENT"] | TransitionType: refactoring
"refactor" → IMPLEMENT. TransitionType: refactoring ("refactor" wins over feature_addition).
