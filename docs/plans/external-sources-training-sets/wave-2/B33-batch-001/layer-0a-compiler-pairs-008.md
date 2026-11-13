# Layer 0-A — Compiler Pairs (008)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-008-001 — gap: "hallucination guard"

A: "Add a hallucination guard to the LLM output pipeline."
B: ["IMPLEMENT"] — "add a hallucination guard" in code context → IMPLEMENT (feature addition to software pipeline).
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-008-002 — gap: "temperature setting"

A: "Lower the temperature setting for the classification model."
B: [] — "lower the temperature" → AI model configuration gap. "lower" is not a code-writing verb here.
CompiledPredicateIds: []

---

## C0A-008-003 — gap: "agent framework"

A: "Evaluate different AI agent frameworks for the project."
B: [] — "evaluate" is observational/analytical gap. "agent frameworks" → AI vocabulary gap.
CompiledPredicateIds: []

---

## C0A-008-004 — IMPLEMENT+TEST fires: build + test AI feature

A: "Implement the intent classification module and write unit tests for the routing logic."
B: ["IMPLEMENT", "TEST"] — "implement" → IMPLEMENT; "write unit tests" → TEST (5.1). AI vocabulary is object, not trigger.
CompiledPredicateIds: ["IMPLEMENT", "TEST"]

---

## C0A-008-005 — gap: "model version"

A: "Switch from GPT-3.5 to GPT-4 for the support agent."
B: [] — "switch model version" → AI configuration gap. No code-writing verb.
CompiledPredicateIds: []
