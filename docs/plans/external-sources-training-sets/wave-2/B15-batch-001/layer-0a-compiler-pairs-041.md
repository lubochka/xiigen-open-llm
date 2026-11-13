# Layer 0-A — Compiler Pairs (041)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-041-001 — ConsentService: IMPLEMENT from "implement"

A: "Implement a ConsentService to manage user consent preferences."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Confidence: 0.97
"implement" → IMPLEMENT.

---

## B15-compiler-041-002 — ConsentService: IMPLEMENT+SECURITY on "protect"

A: "Implement ConsentService and protect the consent record modification endpoint."
B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
"implement" → IMPLEMENT. "protect" → SECURITY.

---

## B15-compiler-041-003 — ConsentService: IMPLEMENT+ARCHITECTURE on "consent model"

A: "Implement ConsentService with a consent domain model tracking opt-in states."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
"implement" → IMPLEMENT. "consent domain model" → ARCHITECTURE.

---

## B15-compiler-041-004 — ConsentService: IMPLEMENT+DOCUMENT on "spec out"

A: "Implement ConsentService and spec out the consent change event schema."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | TransitionType: feature_addition
"implement" → IMPLEMENT. "spec out" → DOCUMENT.

---

## B15-compiler-041-005 — "consent" and "GDPR (General Data Protection Regulation)" are domain nouns

"consent", "GDPR", "opt-in", "opt-out", "preference" are domain nouns.
None fire any predicate. IMPLEMENT fires only from action verbs.
