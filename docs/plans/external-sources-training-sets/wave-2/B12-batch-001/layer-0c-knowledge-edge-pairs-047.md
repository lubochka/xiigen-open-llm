# Layer 0-C — Knowledge Edge Pairs (047)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-047-001 — Rule: "auto-generating" alone is NOT IMPLEMENT; construction verb required

```
source_id: xiigen-b12-0c-047-001
pair_kind: knowledge_edge
```

### Auto-generate distinction from construction verb

"Define the API documentation generation contract for auto-producing reference docs from OpenAPI specs":
  NOT IMPLEMENT: "auto-producing" is not a construction verb
  DOCUMENT: "define"
  API_CONTRACT: "api" + "contract" + "openapi"

"Auto-generate", "auto-produce", "auto-create" — these describe an automated process,
but in the absence of an explicit construction verb (build/implement/write/create/develop/scaffold),
IMPLEMENT does NOT fire.

IMPLEMENT fires from the vocabulary of the request author's action:
  "Define the contract for auto-generating docs" → the author is DEFINING (documentation verb)
  "Build the auto-generator" → the author is BUILDING (construction verb → IMPLEMENT fires)

The "auto-" prefix describes what the tool does, not what the author is doing.

---

## B12-0C-047-002 — Rule: "build the API doc generator and write HTML pages" fires IMPLEMENT + API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0c-047-002
pair_kind: knowledge_edge
```

### Build + write in documentation generator context

"Build the API documentation generator that reads OpenAPI specs and writes HTML reference pages":
  IMPLEMENT: "build" + "writes" (two construction verbs → one IMPLEMENT predicate)
  API_CONTRACT: "api" + "openapi"
  DOCUMENT: "documentation" + "reference pages"

"Writes HTML reference pages" fires IMPLEMENT via "writes" (code-writing verb) AND fires DOCUMENT
via "documentation"/"reference pages". Both IMPLEMENT and DOCUMENT fire simultaneously.
Neither suppresses the other.

---

## B12-0C-047-003 — Rule: "integrate the API doc generator" = API_CONTRACT + DOCUMENT (integrate ≠ IMPLEMENT)

```
source_id: xiigen-b12-0c-047-003
pair_kind: knowledge_edge
```

### Integrate verb in documentation context

"Integrate the API documentation generator with the CI pipeline to publish docs on each release":
  API_CONTRACT: "integrate" (with API co-occurrence) + "api"
  DOCUMENT: "documentation"
  NOT IMPLEMENT: "integrate" is not a construction verb (B12-R17)
  NOT IMPLEMENT for "publish": "publish" is a release verb (B12-R21)

Both "integrate" and "publish" are non-IMPLEMENT verbs. Only explicit construction verbs fire IMPLEMENT.
The presence of "documentation" in the object fires DOCUMENT independently.
