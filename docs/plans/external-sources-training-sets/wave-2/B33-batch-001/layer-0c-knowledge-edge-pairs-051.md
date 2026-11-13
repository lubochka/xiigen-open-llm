# Layer 0-C — Knowledge Edge Pairs (051)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-051-001 — AI agent + multi-agent combined gap rules

Rule: Both "AI agent" and "multi-agent" belong to gap-ai-ml-model-001.
Combined request without code verb → one SCHEDULE_CAPABILITY_LEARNING → gap-ai-ml-model-001.
"AI agents and multi-agent systems share memory through a common context store" → [] → gap-ai-ml-model-001.

---

## KE-051-002 — AI agent tool dispatcher: implementation pattern

"AI agent tool dispatcher" is a software artifact when built:
- "Implement the AI agent tool dispatcher" → IMPLEMENT
- "Build an AI agent tool dispatcher" → IMPLEMENT
- "AI agent tool dispatcher routes calls" (descriptive) → [] → gap-ai-ml-model-001
The action verb determines the outcome.

---

## KE-051-003 — Knowledge graph + semantic search: IMPLEMENT vs. gap distinction

| Input | Result |
|-------|--------|
| "Knowledge graph enrichment improves semantic search recall" | [] → gap-ai-ml-model-001 |
| "Build the knowledge graph adapter" | IMPLEMENT |
| "Write tests for the semantic search relevance" | TEST |
| "Integrate the knowledge graph API with semantic search" | API_CONTRACT |

---

## KE-051-004 — IMPLEMENT+TEST: build then test pattern for round-3

Pattern: "Build [AI artifact] and write [tests] for [AI artifact]" → [IMPLEMENT, TEST].
"Build the knowledge graph adapter and write integration tests for the query layer" → [IMPLEMENT, TEST].
Standard ordering: IMPLEMENT before TEST (rule 3).

---

## KE-051-005 — DOCUMENT fires on AI architecture documentation

Rule: "document the [AI] architecture" → DOCUMENT.
"Document the AI agent architecture including tool registry and memory management design" → DOCUMENT.
Even though "architecture" appears in the object, the verb "document" fires DOCUMENT, not ARCHITECTURE.
ARCHITECTURE fires only when "design/architect" is the primary verb + structural noun in object.
