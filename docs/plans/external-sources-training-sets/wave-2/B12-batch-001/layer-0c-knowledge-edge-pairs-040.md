# Layer 0-C — Knowledge Edge Pairs (040)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-040-001 — Rule: "versioned endpoint" is a domain qualifier; endpoint fires API_CONTRACT

```
source_id: xiigen-b12-0c-040-001
pair_kind: knowledge_edge
```

### Rule B12-R30: Versioned endpoint qualifier

"Versioned endpoint", "versioned API", "multi-version API" — the "versioned" and "multi-version"
prefixes are domain qualifiers (B12-R25). They do NOT independently fire any predicate.
API_CONTRACT fires via the endpoint/api/contract/rest/route vocabulary.

Examples:
  "Define the versioned endpoint contract" → "endpoint" + "contract" → API_CONTRACT; "define" → DOCUMENT
  "Build the versioned endpoint /api/v2/users" → "build" → IMPLEMENT; "endpoint" + "api" → API_CONTRACT
  "Versioned API routing" → alone, no predicate without explicit API vocabulary

"Versioned" is a lifecycle label, not an API trigger. "Endpoint" is the API trigger.

---

## B12-0C-040-002 — Rule: "upgrade" does NOT fire IMPLEMENT unless a construction verb is also present

```
source_id: xiigen-b12-0c-040-002
pair_kind: knowledge_edge
```

### Upgrade vs construction verb distinction

"Upgrade" describes a migration/lifecycle event. It does NOT fire IMPLEMENT.
Only explicit construction verbs (implement, build, create, write, develop, scaffold) fire IMPLEMENT.

Examples:
  "Define the versioned endpoint contract for the REST API v2 upgrade" → API_CONTRACT + DOCUMENT
    "upgrade" → not IMPLEMENT (lifecycle term)
    "define" → DOCUMENT
    "rest" + "api" + "endpoint" + "contract" → API_CONTRACT

  "Upgrade and implement the versioned endpoint" → IMPLEMENT + API_CONTRACT
    "implement" is explicit → IMPLEMENT fires
    "endpoint" + "api" → API_CONTRACT fires

Corrupt (v6-01): treating "upgrade" as IMPLEMENT.

---

## B12-0C-040-003 — Rule: "design the versioned API strategy" fires ARCHITECTURE + API_CONTRACT (no DOCUMENT unless documentation verb present)

```
source_id: xiigen-b12-0c-040-003
pair_kind: knowledge_edge
```

### Design verb boundary in versioned API context

"Design the versioned API strategy with header-based and URI-based version negotiation" fires:
  ARCHITECTURE: "design" (unconditional)
  API_CONTRACT: "api"
  NOT DOCUMENT: no specification/documentation verb present

Contrast:
  "Design and specify the versioned API strategy" → ARCHITECTURE + API_CONTRACT + DOCUMENT
    "specify" provides the DOCUMENT trigger

"Design" alone is NOT a DOCUMENT trigger. DOCUMENT requires an explicit documentation verb
(define, specify, document, describe, update-as-specification).
