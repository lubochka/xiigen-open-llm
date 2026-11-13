# Layer 0-C — Knowledge Edge Pairs (044)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-044-001 — Rule: "tenant-scoped" is a domain qualifier (B12-R25 extended)

```
source_id: xiigen-b12-0c-044-001
pair_kind: knowledge_edge
```

### Tenant qualifier rule

"Tenant-scoped", "multi-tenant", "tenant isolation", "per-tenant" — these are domain qualifiers.
They describe architectural patterns applied to APIs but do NOT independently fire predicates.

Examples:
  "Define the tenant-scoped API contract" → "api" + "contract" → API_CONTRACT (not from "tenant-scoped")
  "Implement tenant isolation in the REST API endpoints" → "implement" → IMPLEMENT; "rest" + "api" + "endpoints" → API_CONTRACT
  "Multi-tenant SaaS API" → "api" → API_CONTRACT (the "multi-tenant SaaS" part is qualifier only)

B12-R25 generalization: domain qualifiers never independently fire predicates. The predicate fires
from the API vocabulary that accompanies the qualifier.

---

## B12-0C-044-002 — Rule: "authentication" fires SECURITY in tenant-scoped context (Core Rule 6 unconditional)

```
source_id: xiigen-b12-0c-044-002
pair_kind: knowledge_edge
```

### Core Rule 6 in multi-tenant context

"Define the tenant-scoped API contract with authentication and tenant-level authorization policies" fires:
  DOCUMENT: "define"
  API_CONTRACT: "api" + "contract"
  SECURITY: "authentication" (Core Rule 6: unconditional — even in tenant/authorization context)

The presence of "tenant-level authorization" does NOT suppress SECURITY. "Authentication" always
fires SECURITY regardless of the surrounding tenant/authorization context.

Teaching: Core Rule 6 is absolute. The classifier must not demote "authentication" to an
authorization context term that avoids SECURITY.

---

## B12-0C-044-003 — Rule: "expose the tenant API contract" fires API_CONTRACT only (no SECURITY from "tenant")

```
source_id: xiigen-b12-0c-044-003
pair_kind: knowledge_edge
```

### Tenant qualifier does not add SECURITY

"Expose the tenant-scoped API contract to external partners" fires:
  API_CONTRACT: "expose" + "api" + "contract"
  NOT SECURITY: "tenant-scoped" is a qualifier; no authentication/oauth keyword present

Corrupt (v6-02 variant): treating "tenant-scoped" as implying authentication → incorrectly adding SECURITY.
The rule: SECURITY fires from explicit authentication/oauth keywords, not from implied security contexts.
"Tenant scoping" is an isolation pattern, not an authentication mechanism keyword.
