# Layer 0-A — Compiler Pairs (058)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-058-001 — DeadLetterService: IMPLEMENT from "create"

A: "Create a DeadLetterService to capture and replay failed event messages."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Confidence: 0.97
"create" → IMPLEMENT.

---

## B15-compiler-058-002 — DeadLetterService: IMPLEMENT+API_CONTRACT on "integrate"

A: "Create DeadLetterService and integrate with the DLQ (Dead Letter Queue) API."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
"create" → IMPLEMENT. "integrate" → API_CONTRACT.

---

## B15-compiler-058-003 — DeadLetterService: IMPLEMENT+TEST on "write tests"

A: "Create DeadLetterService and write integration tests for message capture and replay."
B: Predicates: ["IMPLEMENT", "TEST"] | TransitionType: feature_addition
"create" → IMPLEMENT. "write integration tests" → TEST.

---

## B15-compiler-058-004 — DeadLetterService: IMPLEMENT+DOCUMENT on "document"

A: "Create DeadLetterService and document the replay strategy and failure classification schema."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | TransitionType: feature_addition
"create" → IMPLEMENT. "document" → DOCUMENT.

---

## B15-compiler-058-005 — "capture" and "replay" are IMPLEMENT trigger verbs

"capture" and "replay" as code artifact creation verbs fire IMPLEMENT.
"Capture failed messages to the DLQ" → IMPLEMENT. "Replay dead-letter messages for reprocessing" → IMPLEMENT.
