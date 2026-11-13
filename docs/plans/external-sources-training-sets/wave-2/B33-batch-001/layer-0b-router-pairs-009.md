# Layer 0-B — Router Pairs (009)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-009-001 — Gap: AI agent vocabulary summary routing

Gap vocabulary (always → GapHandler regardless of context):
adapt / agentic / autonomous (AI sense) / chain-of-thought / context window / embedding / evaluate (ML) / few-shot / fine-tune / grounding / hallucination / knowledge cutoff / LLM / multi-agent (as noun/adjective) / orchestrate (as AI verb) / persona (AI) / prompt / prompt engineering / RAG / skill (AI sense) / system prompt / temperature (ML) / token / tool-calling / tool-use / train (ML sense) / vector

---

## C0B-009-002 — Code verbs override AI vocabulary in object

When these code verbs are present, predicates fire regardless of AI vocabulary in object:
implement → IMPLEMENT | build → IMPLEMENT | create → IMPLEMENT | develop → IMPLEMENT | write (code) → IMPLEMENT
design / architect / structure → ARCHITECTURE
integrate → API_CONTRACT
document → DOCUMENT
write tests / add tests / create tests → TEST
secure / sanitize / validate (security context) → SECURITY

---

## C0B-009-003 — Ambiguity: "add" in AI vs code context

"add few-shot examples to the prompt" → [] (prompt engineering gap — adding to prompt, not code)
"add a caching layer to the RAG pipeline" → ["IMPLEMENT"] (adding software feature to code)
"add input validation to the agent endpoint" → ["SECURITY", "IMPLEMENT"] (code security feature)
Rule: "add" fires predicates when adding a software artifact; gap when adding to AI/prompt context.

---

## C0B-009-004 — TransitionType in AI context

IMPLEMENT (AI feature) → feature_addition (same as any implementation)
ARCHITECTURE (AI system design) → documentation (no IMPLEMENT co-present)
ARCHITECTURE+IMPLEMENT → feature_addition

---

## C0B-009-005 — Gap: "AI agent" alone in question

A: [] | "What AI agent should we use for this task?"
B: GapHandler
Reason: Advisory/analytical question about AI tooling → gap. No predicate fires on questions about AI choice.
