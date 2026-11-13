# Layer 0-A — Compiler Pairs (007)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-007-001 — gap: "skill routing"

A: "The agent skill routing is choosing the wrong skill for escalation queries."
B: [] — observation statement; "skill routing" (AI sense) → gap. No predicate fires.
CompiledPredicateIds: []

---

## C0A-007-002 — IMPLEMENT fires: "create a skill"

A: "Create a new escalation skill for the support agent."
B: ["IMPLEMENT"] — "create" fires IMPLEMENT. "skill" in AI sense is object; code-writing verb determines predicate.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-007-003 — gap: "agent persona"

A: "Define the agent persona for the sales assistant."
B: [] — "define" + "agent persona" → AI configuration gap. "define" does not fire ARCHITECTURE here (persona is AI behavior, not structural vocabulary).
CompiledPredicateIds: []

---

## C0A-007-004 — gap: "context window"

A: "The context window is too small for the multi-turn conversation agent."
B: [] — "context window" → LLM/AI gap vocabulary. No predicate fires.
CompiledPredicateIds: []

---

## C0A-007-005 — SECURITY fires: "validate agent inputs"

A: "Validate and sanitize all inputs to the AI agent endpoint to prevent prompt injection."
B: ["SECURITY"] — "validate and sanitize inputs", "prevent prompt injection" → SECURITY (input validation for security). AI context does not suppress SECURITY.
CompiledPredicateIds: ["SECURITY"]
