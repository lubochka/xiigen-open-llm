# Layer 0-C — Knowledge-Edge Pairs (052)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-052-001 — FileUploadWidget: ARCHITECTURE fires on structural nouns, not verbs

```
source_id: xiigen-b01-0c-052-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A: user_request: "Define the file upload service API schema including endpoint, file size limits, and supported MIME types."
B:
  CompiledPredicateIds: ["ARCHITECTURE"] (not KNOWLEDGE_EDGE)
  Routing: 3.1 | TransitionType: documentation

Key rule applied:
  "define" is gap verb → does NOT fire IMPLEMENT or ARCHITECTURE by itself.
  "service" IS in ARCHITECTURE lexicon → fires ARCHITECTURE.
  "API" IS in ARCHITECTURE lexicon → fires ARCHITECTURE.
  "schema" IS in ARCHITECTURE lexicon → fires ARCHITECTURE.
  Three ARCHITECTURE hits → CompiledPredicateIds: ["ARCHITECTURE"].
  ARCHITECTURE fires because of structural noun terms, not because of "define".

---

## B01-0C-052-002 — MIME types, endpoints, file size limits: all gap terms

```
source_id: xiigen-b01-0c-052-002
source_license_state: first_party
pair_kind: knowledge_edge
```

File upload domain gap terms confirmed:

| Term | Why it does NOT fire |
|---|---|
| MIME type | Domain-specific noun; not in any lexicon |
| endpoint | Could suggest ARCHITECTURE but not in lexicon |
| file size limit | Domain constraint; not in any lexicon |
| progress indicator | Domain noun; not in any lexicon |
| drag-and-drop zone | Domain noun; "zone" not in any lexicon |
| multi-file | Adjective; not in any lexicon |
| chunked upload | Domain pattern; not in any lexicon |
| resumable upload | Domain pattern; not in any lexicon |

Note: "endpoint" is NOT in ARCHITECTURE lexicon despite being infrastructure-related.
ARCHITECTURE fires on: architecture, system-design, service, API, database, schema, integration, infrastructure, pipeline, module, dependency.
