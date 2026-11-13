# Layer 0-C — Knowledge Edge Pairs (002)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-002-001 — AI vocabulary gap edges (comprehensive)

AI/LLM vocabulary → gap (zero predicates):
adapt (AI sense) | agent | agentic | autonomous (AI sense) | chain-of-thought | context window | embedding | evaluate (ML) | few-shot | fine-tune | foundation model | grounding | hallucination | knowledge cutoff | LLM | model | multi-agent | orchestrate (AI verb) | persona (AI) | prompt | prompt engineering | RAG (Retrieval-Augmented Generation) | reasoning trace | skill (AI sense) | system prompt | temperature (ML) | token | tool-calling | tool-use | train (ML) | vector database | vector embedding

---

## KE-002-002 — Code verb edges (override AI object)

When code verbs appear WITH AI vocabulary:
implement → IMPLEMENT (code-writing verb wins over AI object)
build → IMPLEMENT
create → IMPLEMENT
develop → IMPLEMENT
write (code/tests) → IMPLEMENT or TEST depending on context
design → ARCHITECTURE
architect → ARCHITECTURE
structure → ARCHITECTURE
integrate → API_CONTRACT
document → DOCUMENT
sanitize + security context → SECURITY

Rule: Predicate is determined by the ACTION verb, not the AI vocabulary in the object/target.

---

## KE-002-003 — "adapt" gap edges

"adapt" in AI/agent sense → gap:
- "adapt the agent" → gap
- "adapt the model" → gap
- "adapt the workflow" (AI context) → gap
- "self-adapting" / "adaptive agent" → gap

Contrast — "adapt" with code verb:
- "implement adaptive routing" → IMPLEMENT (IMPLEMENT fires on "implement")
- "refactor the adaptive module" → IMPLEMENT (refactor is code verb)

Rule: "adapt" alone (AI adaptation) → gap. Code verbs in same clause override.
