# Layer 0-C — Knowledge-Edge Pairs (052)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-052-001 — InstallmentPlan: gap verbs

```
source_id: xiigen-b06-0c-052-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-052-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for InstallmentPlan:
    "Schedule monthly installment charges for the customer." → KNOWLEDGE_EDGE (schedule = operational)
    "Charge the next installment on the due date." → KNOWLEDGE_EDGE (charge = operational)
    "Apply a late fee when the installment is missed." → KNOWLEDGE_EDGE (apply = operational)
    "Cancel the installment plan after two missed payments." → KNOWLEDGE_EDGE (cancel = operational)
  Teaching: "monthly installment charges", "installment", "late fee", "missed payments" are domain nouns.

---

## B06-0C-052-002 — InstallmentPlan: boundary cases

```
source_id: xiigen-b06-0c-052-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-052-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for InstallmentPlan:
    "Define the installment plan requirements." → NOT knowledge-edge (define, requirements fire DOCUMENT)
    "Schedule installments per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Create the installment scheduler." → NOT knowledge-edge (create fires IMPLEMENT)
    "Schedule monthly installment charges." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-052-003 — InstallmentPlan: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-052-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-052-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for InstallmentPlan:
    gap: "schedule/charge/apply/cancel/process installment"
    spec: "define/describe/specify/document installment plan requirements"
  Teaching: "BNPL", "installment", "interest-free period", "missed payment" are domain nouns — never fire predicates alone.
