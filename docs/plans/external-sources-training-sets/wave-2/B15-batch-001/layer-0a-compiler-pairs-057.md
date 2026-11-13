# Layer 0-A — Compiler Pairs (057)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-057-001 — RetryService: IMPLEMENT from "implement"

A: "Implement a RetryService to handle transient failures with exponential backoff."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Confidence: 0.97
"implement" → IMPLEMENT.

---

## B15-compiler-057-002 — RetryService: IMPLEMENT+ARCHITECTURE on "retry model"

A: "Implement RetryService with a retry domain model covering policy configuration."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
"implement" → IMPLEMENT. "retry domain model" → ARCHITECTURE.

---

## B15-compiler-057-003 — RetryService: IMPLEMENT+TEST on "write tests"

A: "Implement RetryService and write tests for all retry policy configurations."
B: Predicates: ["IMPLEMENT", "TEST"] | TransitionType: feature_addition
"implement" → IMPLEMENT. "write tests" → TEST.

---

## B15-compiler-057-004 — RetryService: IMPLEMENT+DOCUMENT on "document"

A: "Implement RetryService and document the exponential backoff configuration parameters."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | TransitionType: feature_addition
"implement" → IMPLEMENT. "document" → DOCUMENT.

---

## B15-compiler-057-005 — "retry" is a domain noun and also an IMPLEMENT trigger verb

"retry" as a noun (e.g., "retry policy") does not fire predicates.
"retry" as a verb describing code artifact creation fires IMPLEMENT: "Retry failed operations with backoff" → IMPLEMENT.
Context determines whether "retry" is a noun or verb in the request.
