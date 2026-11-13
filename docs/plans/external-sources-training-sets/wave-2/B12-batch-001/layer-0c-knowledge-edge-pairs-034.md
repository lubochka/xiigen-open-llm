# Layer 0-C — Knowledge Edge Pairs (034)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-034-001 — API_CONTRACT + SECURITY + DOCUMENT triple: complete predicate fire

```
source_id: xiigen-b12-0c-034-001
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the API contract and security requirements for the payment gateway authentication endpoints."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  Rule applications:
    Rule B12-R01: "api" + "contract" + "endpoints" → API_CONTRACT ✓
    Rule B12-R04: "define" → DOCUMENT ✓
    Rule B12-R06: "authentication" → SECURITY unconditional ✓
  Key: Three predicates fire from a single request. "security requirements" alone does NOT fire
  SECURITY — "security" as a general term is not the same as "authentication". However, "authentication"
  present in the same request fires SECURITY unconditionally.
  DecisionCount: 3 (api_contract→3.3, document→2.1, security→3.4)

---

## B12-0C-034-002 — Tenant-scoped API: domain qualifier does not change API_CONTRACT behavior

```
source_id: xiigen-b12-0c-034-002
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the tenant-scoped API contract for the multi-tenant SaaS platform."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Rule applications:
    Rule B12-R01: "api" + "contract" → API_CONTRACT ✓
    Rule B12-R04: "define" → DOCUMENT ✓
  Key: "tenant-scoped" is a domain qualifier. It does NOT suppress or change API_CONTRACT.
  "multi-tenant" is an architectural concept. It does NOT fire ARCHITECTURE because there is no "design" verb.
  "SaaS" is a business model term. It does NOT fire any predicate independently.
  DomainId remains "software-development" for all B12 pairs (Core Rule 24).

---

## B12-0C-034-003 — Backward compatible API: "upgrade" is not IMPLEMENT; "compatible" does not add predicates

```
source_id: xiigen-b12-0c-034-003
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the backward compatible API contract for the user service upgrade from v1 to v2."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Rule applications:
    Rule B12-R04: "define" → DOCUMENT ✓
    Rule B12-R01: "api" + "contract" → API_CONTRACT ✓
  Key: "upgrade" in this context = specifying the upgrade path, not building code.
  "upgrade from v1 to v2" is a version increment description, not a construction verb.
  Compare: "implement the v2 upgrade for the user service" → IMPLEMENT fires.
  "backward compatible" is a contract qualifier, not a predicate trigger.
  DecisionCount = 2 (not 3; no IMPLEMENT from "upgrade").
