# Layer 0-C — Knowledge Edge Pairs (057)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-057-001 — Rule: "publish the SDK" fires API_CONTRACT only; NOT IMPLEMENT (B12-R21 extended)

```
source_id: xiigen-b12-0c-057-001
pair_kind: knowledge_edge
```

### Publish as release verb in SDK context

"Publish the auto-generated SDK to the partner developer portal alongside the API contract":
  API_CONTRACT: "publish" (release verb) + "api" + "contract"
  NOT IMPLEMENT: "publish" is a release verb per B12-R21
  NOT DOCUMENT: no documentation verb

"Publish" in SDK context is the same as "publish" in API context: it is a release/deployment
action. Even though publishing an SDK implies engineering work, "publish" alone is classified
as a release verb in B12. Only explicit construction verbs fire IMPLEMENT.

Contrast:
  "Publish the SDK" → API_CONTRACT (publish = release verb)
  "Build the SDK" → IMPLEMENT + API_CONTRACT (build = construction verb)

---

## B12-0C-057-002 — Rule: "build the TypeScript SDK from the REST API contract and write the client wrapper classes" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0c-057-002
pair_kind: knowledge_edge
```

### Build + write in SDK generation context

"Build the TypeScript SDK from the REST API contract and write the client wrapper classes":
  IMPLEMENT: "build" + "write" (two construction verbs → one IMPLEMENT predicate)
  API_CONTRACT: "rest" + "api" + "contract"
  NOT DOCUMENT: no documentation verb ("wrapper classes" is a code artifact description, not documentation)

"Client wrapper classes" are code artifacts. "Write the wrapper classes" = construction verb + code artifact.
This fires IMPLEMENT. DOCUMENT does NOT fire because no documentation term is present.

Contrast:
  "Write the OpenAPI specification" → IMPLEMENT + API_CONTRACT + DOCUMENT ("specification" fires DOCUMENT)
  "Write the client wrapper classes" → IMPLEMENT + API_CONTRACT (no documentation term)

---

## B12-0C-057-003 — Rule: "integrate SDK generation and document the API specification versioning contract"

```
source_id: xiigen-b12-0c-057-003
pair_kind: knowledge_edge
```

### Integrate + document: two predicates from two verb types

"Integrate SDK generation into the CI pipeline and document the API specification versioning contract":
  API_CONTRACT: "integrate" (with API co-occurrence) + "api" + "contract"
  DOCUMENT: "specification" + "document"
  NOT IMPLEMENT: "integrate" is not a construction verb

"Specification" fires DOCUMENT independently from the verb. Even without an explicit documentation
verb, "specification" (spec) is a DOCUMENT trigger.

Final result: [API_CONTRACT, DOCUMENT]. Routes = [3.3, 2.1]. DecisionCount = 2.
