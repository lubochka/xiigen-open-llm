# Layer 0-B — Router Pairs (003)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-003-001 — Zero predicates → GapHandler (AI vocabulary alone)

AI vocabulary that always routes to GapHandler (zero predicates):
- "adapt the agent" → GapHandler
- "autonomous agent" → GapHandler
- "LLM prompt engineering" → GapHandler
- "RAG pipeline optimization" (without build/implement verb) → GapHandler
- "fine-tune the model" → GapHandler
- "tool-calling behavior" → GapHandler
- "chain-of-thought prompting" → GapHandler
- "token budget" → GapHandler
- "agent persona" → GapHandler
- "context window" → GapHandler
- "hallucination reduction" (without build verb) → GapHandler
- "agentic workflow" (without design verb) → GapHandler

---

## C0B-003-002 — IMPLEMENT routes to 4.1 even with AI object

A: ["IMPLEMENT"] | "Build the RAG retrieval module" → ImplementationFlow41 (4.1)
A: ["IMPLEMENT"] | "Create an intent classifier" → ImplementationFlow41 (4.1)
A: ["IMPLEMENT"] | "Write the agent routing logic" → ImplementationFlow41 (4.1)
Rule: Code-writing verbs + AI object → IMPLEMENT (4.1) regardless of AI vocabulary in object.

---

## C0B-003-003 — TEST routes to 5.1 or 5.3 even with AI object

A: ["TEST"] | "Write unit tests for the intent classifier" → UnitTestingFlow51 (5.1)
A: ["TEST"] | "Write integration tests for the agent API" → E2ETestingFlow53 (5.3)
Rule: Test-writing verbs + AI object → TEST (5.1 or 5.3 by sub-phase rules). AI context is object, not trigger.

---

## C0B-003-004 — DOCUMENT routes to DocumentationFlow62 with AI object

A: ["DOCUMENT"] | "Document the AI agent API schema" → DocumentationFlow62
Rule: Documentation verb + AI object → DOCUMENT → DocumentationFlow62.

---

## C0B-003-005 — v6-02 corruption pattern: gap → IMPLEMENT (wrong)

A: [] | "Adapt the agent behavior for new use cases"
Correct: GapHandler
Corrupt: ["IMPLEMENT"] ← v6-02 ("adapt" incorrectly mapped to IMPLEMENT)
Teaching: "adapt" (AI sense) is gap. IMPLEMENT requires code-writing verbs.
