# Layer 0-A — Compiler Pairs (003)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-003-001 — gap: "RAG pipeline"

A: "Build a RAG (Retrieval-Augmented Generation) pipeline for the knowledge base."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "RAG pipeline" is the object, not a trigger. Predicate fires on code verb.
Wait — B33 teaching point 4 says: "Development action verbs (build/implement/design) fire predicates even when the object is AI vocabulary."
So: B: ["IMPLEMENT"] — "build" → IMPLEMENT.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-003-002 — gap: "chain-of-thought prompting"

A: "Use chain-of-thought prompting in the support classifier."
B: [] — "use" is not a code-writing verb. "chain-of-thought prompting" → gap. No predicate fires.
CompiledPredicateIds: []

---

## C0A-003-003 — gap: "autonomous agent"

A: "Make the support bot more autonomous."
B: [] — "make more autonomous" is vague; "autonomous" (AI sense) → gap. No predicate fires.
CompiledPredicateIds: []

---

## C0A-003-004 — gap: "tool-calling"

A: "Enable tool-calling for the sales agent."
B: [] — "enable" is operational gap; "tool-calling" → gap vocabulary. No predicate fires.
CompiledPredicateIds: []

---

## C0A-003-005 — API_CONTRACT fires on "integrate" even with AI context

A: "Integrate the AI agent with the CRM API."
B: ["API_CONTRACT"] — "integrate" fires API_CONTRACT in any context. AI context does not change the predicate.
CompiledPredicateIds: ["API_CONTRACT"]
