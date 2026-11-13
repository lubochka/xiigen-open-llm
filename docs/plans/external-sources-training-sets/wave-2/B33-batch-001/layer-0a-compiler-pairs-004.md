# Layer 0-A — Compiler Pairs (004)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-004-001 — gap: "agentic workflow"

A: "Design an agentic workflow for document processing."
B: ["ARCHITECTURE"] — "design" fires ARCHITECTURE. "agentic workflow" is the object (AI vocabulary), but "design" is the trigger.
Note: "design" overrides the AI object → ARCHITECTURE fires.
CompiledPredicateIds: ["ARCHITECTURE"]

---

## C0A-004-002 — gap: "prompt engineering" alone

A: "Do better prompt engineering for the classification model."
B: [] — "do better" is not a code-writing verb. "prompt engineering" → gap. No predicate.
CompiledPredicateIds: []

---

## C0A-004-003 — gap: "fine-tune the model"

A: "Fine-tune the LLM on customer support data."
B: [] — "fine-tune" in ML sense → gap vocabulary. Not a code-writing verb for software predicates.
CompiledPredicateIds: []

---

## C0A-004-004 — IMPLEMENT fires: "write the agent orchestration code"

A: "Write the agent orchestration code for the multi-step workflow."
B: ["IMPLEMENT"] — "write" (code-writing verb) fires IMPLEMENT. Orchestration context is object, not trigger.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-004-005 — gap: "AI agent" as subject, vague statement

A: "Our AI agent is not performing well on edge cases."
B: [] — observation statement without predicate-triggering verb. No predicate fires.
CompiledPredicateIds: []
