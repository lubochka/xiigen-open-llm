# Layer 0-C — Knowledge Edge Pairs (047)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-047-001 — Embedding model gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| embedding model (standalone) | No code verb | gap-ai-ml-model-001 |
| embedding model client (with "build") | Code verb present | IMPLEMENT fires |
| embedding model API (with "integrate") | "integrate" present | API_CONTRACT fires |
| embedding pipeline (with "build") | Code verb present | IMPLEMENT fires |

---

## KE-047-002 — Vector database gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| vector database (standalone) | No code verb | gap-ai-ml-model-001 |
| vector database connector (with "implement") | Code verb present | IMPLEMENT fires |
| vector database endpoint (with "secure") | "secure" present | SECURITY fires |
| vector database API (with "integrate") | "integrate" present | API_CONTRACT fires |

---

## KE-047-003 — Embedding model vs. vector database: both gap-ai-ml-model-001

Rule: Both "embedding model" and "vector database" belong to gap-ai-ml-model-001.
They frequently appear together (model produces vectors, database stores them).
"embedding model stores vectors in vector database" → [] → gap-ai-ml-model-001 (one event).
Code verb on either → predicates fire; gap skipped.

---

## KE-047-004 — CRUD (create, read, update, delete) in object does not fire additional predicates

Rule: CRUD operations described in the object position do not fire additional predicates.
"implement a vector database connector that abstracts CRUD operations" → IMPLEMENT only.
CRUD is an acronym used as object context; it does not trigger additional predicates.

---

## KE-047-005 — Embedding model client is IMPLEMENT, embedding model is gap

Distinction:
- "embedding model" = AI model type → gap (no code verb).
- "embedding model client" = software artifact → IMPLEMENT when "build/implement" present.
- "embedding model API" = interface → API_CONTRACT when "integrate" present.
The noun that follows determines whether it's a software artifact or an AI term.
