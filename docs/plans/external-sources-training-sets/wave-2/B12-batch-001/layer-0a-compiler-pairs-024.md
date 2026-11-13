# Layer 0-A — Compiler Pairs (024)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-024 — Complete predicate rule reference for B12 API_CONTRACT

```
source_id: xiigen-b12-0a-024
pair_kind: compiler_reference
```

### API_CONTRACT Trigger Lexicon (complete)

Every term in this list fires API_CONTRACT when present in a request:

| Term | Notes |
|------|-------|
| api | Core trigger — fires always |
| endpoint | Core trigger — fires always |
| contract | Core trigger — fires always regardless of modifier |
| request | Fires when in API context (with api/rest/endpoint co-occurrence) |
| response | Fires when in API context |
| schema | Fires ONLY with API co-occurrence (api/rest/graphql/openapi/swagger/grpc/webhook) |
| openapi | Core trigger — fires always |
| swagger | Core trigger — fires always |
| rest | Core trigger — fires always |
| graphql | Core trigger — fires always |
| grpc | Core trigger — fires always |
| webhook | Core trigger — fires always |
| interface | Fires API_CONTRACT in API context; fires ARCHITECTURE in system design context |
| route | Core trigger — fires always |
| payload | Does NOT fire API_CONTRACT alone; serves as modifier |
| spec | Fires DOCUMENT in API context; does NOT fire API_CONTRACT alone |
| idempotent | Fires API_CONTRACT (API contract concept) |
| integration | Fires API_CONTRACT ONLY with API co-occurrence |

---

### IMPLEMENT Triggers (code-creation verbs)

| Term | Fires IMPLEMENT? |
|------|-----------------|
| implement | YES |
| build | YES |
| create | YES |
| write | YES (code verb — "write the schema", "write the spec") |
| develop | YES |
| scaffold | YES |
| update | NO (maintenance verb) |
| define | NO (specification verb) |
| specify | NO (specification verb) |
| document | NO (documentation verb) |
| integrate | NO (per Core Rule 17) |
| publish | NO (release verb) |

---

### "contract" modifier table — all fire API_CONTRACT

| Phrase | API_CONTRACT fires? | Additional predicates |
|--------|---------------------|-----------------------|
| data contract | YES | — |
| service contract | YES | — |
| message contract | YES | — |
| event contract | YES | — |
| webhook contract | YES | + webhook also fires |
| API contract | YES | + api also fires |
| REST contract | YES | + rest also fires |
| gRPC contract | YES | + grpc also fires |
| GraphQL contract | YES | + graphql also fires |
| versioning contract | YES | — |
| interface contract | YES | + interface also fires |
| idempotent contract | YES | + idempotent also fires |
| payload contract | YES | — |
| response contract | YES | + response fires |

---

### "schema" co-occurrence table

| Phrase | API_CONTRACT fires? | Reason |
|--------|---------------------|--------|
| API schema | YES | "api" is co-occurrence |
| REST schema | YES | "rest" is co-occurrence |
| GraphQL schema | YES | "graphql" is co-occurrence |
| OpenAPI schema | YES | "openapi" is co-occurrence |
| gRPC schema | YES | "grpc" is co-occurrence |
| webhook schema | YES | "webhook" is co-occurrence |
| database schema | NO | "database" is NOT an API co-occurrence |
| data model schema | NO | no API co-occurrence |
| table schema | NO | no API co-occurrence |
| JSON schema (alone) | NO | "json" alone is not in API co-occurrence list |
| JSON schema (in API context) | YES | when api/rest/graphql also in request |

---

### "interface" disambiguation table

| Context | API_CONTRACT fires? | ARCHITECTURE fires? |
|---------|---------------------|---------------------|
| "REST API interface" | YES | NO (no "design" verb) |
| "gRPC interface definition" | YES | NO |
| "design the service interface" | NO direct API_CONTRACT | YES (if no API terms) |
| "design the service interface and API contract" | YES | YES (both fire) |
| "interface" + "design" + "api" | YES | YES (dual-fire) |

---

### TransitionType decision table

| Predicates | IMPLEMENT present? | TransitionType |
|-----------|-------------------|----------------|
| API_CONTRACT alone | NO | documentation |
| API_CONTRACT + DOCUMENT | NO | documentation |
| API_CONTRACT + SECURITY | NO | documentation |
| API_CONTRACT + ARCHITECTURE | NO | documentation |
| IMPLEMENT + API_CONTRACT | YES | feature_addition |
| IMPLEMENT + API_CONTRACT + DOCUMENT | YES | feature_addition |
| IMPLEMENT + API_CONTRACT + SECURITY | YES | feature_addition |

Rule: TransitionType = feature_addition if and only if IMPLEMENT fires. Otherwise: documentation.

---

### SECURITY trigger note

"authentication" fires SECURITY ALWAYS — Core Rule 6 is unconditional.
No context suppresses SECURITY when "authentication" is present.
This includes: REST API doc context, OpenAPI spec context, webhook spec context, gRPC context.

### DomainId note

All B12 pairs: DomainId = "software-development" (Core Rule 24)

### GapHandler note

No GapHandler in B12 (Core Rule 22) — API_CONTRACT is always covered by sub-phase 3.3.
