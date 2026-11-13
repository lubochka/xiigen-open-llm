# Layer 0-C — Knowledge Edge Pairs (048)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-048-001 — Semantic search gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| semantic search (standalone) | No code verb | gap-ai-ml-model-001 |
| semantic search service (with "build") | Code verb present | IMPLEMENT fires |
| semantic search (with "write tests for") | "write tests" present | TEST fires |
| semantic search API (with "integrate") | "integrate" present | API_CONTRACT fires |

---

## KE-048-002 — Knowledge graph gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| knowledge graph (standalone) | No code verb | gap-ai-ml-model-001 |
| knowledge graph query adapter (with "implement") | Code verb present | IMPLEMENT fires |
| knowledge graph schema (with "document") | "document" present | DOCUMENT fires |
| knowledge graph API (with "integrate") | "integrate" present | API_CONTRACT fires |

---

## KE-048-003 — BM25 (Best Match 25) in object does not change predicate

Rule: BM25, TF-IDF (term frequency–inverse document frequency), and other retrieval algorithms used as object context do not fire predicates.
"Build a semantic search service that combines embedding lookup with BM25 reranking" → IMPLEMENT (only "build" fires).
BM25 is a retrieval algorithm name; it is object context.

---

## KE-048-004 — AI+IMPLEMENT mixed: semantic search + multiple verbs

Pattern for IMPLEMENT+API_CONTRACT mixed request with semantic search:
"Build the semantic search layer … integrate with the knowledge graph API" → [IMPLEMENT, API_CONTRACT].
- "build" → IMPLEMENT on "semantic search layer"
- "integrate" → API_CONTRACT on "knowledge graph API"
Both verbs fire independently. No collapsing.

---

## KE-048-005 — SPARQL (SPARQL protocol and RDF query language) is implementation detail, not gap

Rule: SPARQL is a query language — a software implementation detail, not an AI gap term.
"Implement a knowledge graph query adapter that translates natural language to SPARQL" → IMPLEMENT.
SPARQL in the object does not change the predicate. It's part of the software artifact description.
