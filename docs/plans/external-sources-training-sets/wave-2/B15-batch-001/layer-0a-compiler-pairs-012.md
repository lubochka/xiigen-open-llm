# Layer 0-A — Compiler Pairs (012)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: compiler

---

## B15-compiler-012-001 — "implement" idempotency → IMPLEMENT

A: "Implement idempotent order submission handling to prevent duplicate orders."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition

---

## B15-compiler-012-002 — ARCHITECTURE+IMPLEMENT+TEST+DOCUMENT+QA: five

A: "Architect the order service domain, build the core services, write tests, document the API, and QA."
B: Predicates: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT", "QA"] | TransitionType: feature_addition

---

## B15-compiler-012-003 — "implement" + "integrate" + "write tests": three

A: "Implement the shipping label generator, integrate with the carrier label API, and write unit tests."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST"] | TransitionType: feature_addition

---

## B15-compiler-012-004 — v6 corruption: "model" not caught as ARCHITECTURE

v6 corrupt: "Build the order service and the domain model" → Predicates: ["IMPLEMENT"]
Correct: Predicates: ["IMPLEMENT", "ARCHITECTURE"]
Error type: v6-02. "domain model" → ARCHITECTURE. "model" in domain model fires ARCHITECTURE even when embedded in an IMPLEMENT request.

---

## B15-compiler-012-005 — IMPLEMENT+DOCUMENT: complete B15 pattern summary

Key B15 predicate rules:
- build/implement/create/scaffold/bootstrap/add (code artifact)/write (code)/extend → IMPLEMENT
- "contract" (any modifier) → API_CONTRACT
- "domain model" / "domain model" / "architecture" → ARCHITECTURE
- integrate/wire up/connect → API_CONTRACT
- secure/harden/protect → SECURITY
- write/create tests → TEST
- document/spec out → DOCUMENT
- QA (abbreviation) → QA
