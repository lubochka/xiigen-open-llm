# Layer 0-C — Knowledge-Edge Pairs (041)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-041-001 — MembershipTiers: gap verbs

```
source_id: xiigen-b06-0c-041-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-041-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for MembershipTiers:
    "Upgrade a customer to Gold tier when they reach 500 points." → KNOWLEDGE_EDGE (upgrade = operational)
    "Downgrade a customer to Bronze after inactivity." → KNOWLEDGE_EDGE (downgrade = operational)
    "Check the current tier of the customer." → KNOWLEDGE_EDGE (check = operational)
    "Assign tier benefits based on membership level." → KNOWLEDGE_EDGE (assign = operational)
  Teaching: "membership tier", "Gold", "Bronze", "points", "tier benefits" are domain nouns.

---

## B06-0C-041-002 — MembershipTiers: boundary cases

```
source_id: xiigen-b06-0c-041-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-041-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for MembershipTiers:
    "Define the membership tier requirements." → NOT knowledge-edge (define, requirements fire DOCUMENT)
    "Upgrade members per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Create the tier evaluation service." → NOT knowledge-edge (create fires IMPLEMENT)
    "Upgrade members to the next tier." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-041-003 — MembershipTiers: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-041-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-041-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for MembershipTiers:
    gap: "upgrade/downgrade/check/assign/evaluate tier membership"
    spec: "define/describe/specify/document membership tier requirements"
  Teaching: tier names (Bronze, Silver, Gold, Platinum) are domain labels — never fire predicates.
