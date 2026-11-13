# Layer 0-A — Compiler Pairs (038)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-038-001 — AddressValidationService: IMPLEMENT from "implement"

A: "Implement an AddressValidationService to normalize and verify customer addresses."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Confidence: 0.97
"implement" → IMPLEMENT.

---

## B15-compiler-038-002 — AddressValidationService: IMPLEMENT+API_CONTRACT on "integrate"

A: "Implement AddressValidationService and integrate with the postal verification API (Application Programming Interface)."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
"implement" → IMPLEMENT. "integrate" → API_CONTRACT.

---

## B15-compiler-038-003 — AddressValidationService: IMPLEMENT+ARCHITECTURE on "address model"

A: "Implement AddressValidationService with an address domain model."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
"implement" → IMPLEMENT. "address domain model" → ARCHITECTURE.

---

## B15-compiler-038-004 — AddressValidationService: IMPLEMENT+TEST on "write tests"

A: "Implement AddressValidationService and write unit tests for all validation rules."
B: Predicates: ["IMPLEMENT", "TEST"] | TransitionType: feature_addition
"implement" → IMPLEMENT. "write unit tests" → TEST.

---

## B15-compiler-038-005 — "address" is a domain noun — does not fire predicates

"address", "postal", "zip code", "region" are domain nouns.
None fire any predicate. IMPLEMENT fires only from action verbs (build/implement/create etc.).
v6-01 risk: treating "address" as a trigger for any predicate and returning [] is incorrect.
