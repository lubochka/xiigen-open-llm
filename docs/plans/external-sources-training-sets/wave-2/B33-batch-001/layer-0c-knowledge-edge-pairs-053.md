# Layer 0-C — Knowledge Edge Pairs (053)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-053-001 — Llama + embedding model combination rules

| Input | Result |
|-------|--------|
| "Llama generates text; embedding model encodes queries" | [] → gap-ai-ml-model-001 |
| "Implement an orchestrator that calls Llama for generation and embedding model for retrieval" | IMPLEMENT |
| "Integrate the Llama API with the embedding model service" | API_CONTRACT |

---

## KE-053-002 — Semantic search + knowledge graph enrichment rules

| Input | Result |
|-------|--------|
| "Semantic search results enriched with knowledge graph relationships" | [] → gap-ai-ml-model-001 |
| "Build the semantic search module and integrate with knowledge graph REST API" | IMPLEMENT + API_CONTRACT |
| "Write tests for semantic search service to validate relevance scores" | TEST |

---

## KE-053-003 — "Orchestrator" as software artifact vs. AI gap term

Disambiguation rule:
- "Orchestrate the LLM calls" (AI behavior description, no code verb) → [] → gap-ai-ml-model-001.
- "Implement an orchestrator that calls Llama" (software artifact with code verb) → IMPLEMENT.
- "Orchestrator" as a noun in object position with "implement" → IMPLEMENT.
- "Orchestrate" as primary imperative verb without code verb context → [] → gap (from earlier B33 rounds).

---

## KE-053-004 — IMPLEMENT + API_CONTRACT: "build ... and integrate" pattern

Pattern analysis:
"Build the semantic search module and integrate it with the knowledge graph REST API" → [IMPLEMENT, API_CONTRACT].
Step 1: "build" fires IMPLEMENT on "semantic search module".
Step 2: "integrate" fires API_CONTRACT on "knowledge graph REST API".
Both fire independently. No collapsing permitted.

---

## KE-053-005 — TEST fires from "write tests for" with semantic search object

Rule: "Write tests for the semantic search service" → TEST.
"Semantic search service" in object position does not change the predicate.
TEST fires because "write tests for" is a test-writing form.
This is the same as: "write tests for the tokenizer", "write tests for the embedding service".
