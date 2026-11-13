# Layer 0-C — Knowledge-Edge Pairs (059)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-059-001 — VIPAccess: gap verbs

```
source_id: xiigen-b06-0c-059-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-059-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for VIPAccess:
    "Grant VIP customers access to the flash sale 24 hours before general release." → KNOWLEDGE_EDGE (grant = operational)
    "Unlock exclusive products for Platinum tier members." → KNOWLEDGE_EDGE (unlock = operational)
    "Revoke VIP access when the membership expires." → KNOWLEDGE_EDGE (revoke = operational)
    "Redirect VIP customers to the dedicated support queue." → KNOWLEDGE_EDGE (redirect = operational)
  Teaching: "VIP customers", "flash sale", "exclusive products", "Platinum tier members" are domain nouns.

---

## B06-0C-059-002 — VIPAccess: boundary cases

```
source_id: xiigen-b06-0c-059-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-059-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for VIPAccess:
    "Define the VIP access requirements." → NOT knowledge-edge (define, requirements fire DOCUMENT)
    "Grant access per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Build the VIP access gate." → NOT knowledge-edge (build fires IMPLEMENT)
    "Grant VIP customers early access." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-059-003 — VIPAccess: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-059-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-059-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for VIPAccess:
    gap: "grant/unlock/revoke/redirect/enable VIP access"
    spec: "define/describe/specify/document VIP access requirements"
  Teaching: "VIP access", "early sale window", "exclusive product unlock" are domain nouns — never fire predicates alone.
