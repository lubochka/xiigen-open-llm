# Layer 0-C — Knowledge-Edge Pairs (015)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 015 covers LoyaltyProgram knowledge-edge boundaries — three pairs.

---

## B06-0C-015-001 — Loyalty: "narrative" fires DOCUMENT; "promote" does not

```
source_id: xiigen-b06-0c-015-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-015-001-kedge"
  CompiledText: "Loyalty: narrative fires DOCUMENT; promote does not"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Document the narrative for loyalty program checkout integration"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — document fires DOCUMENT, narrative fires DOCUMENT"
    - request: "Promote loyalty program enrollment during checkout"
      predicates: []
      result: "KNOWLEDGE_EDGE — promote is not in any predicate lexicon"
      capability_gap_id: "gap-loyalty-promote-001"
    - request: "Define the loyalty requirements"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge"

---

## B06-0C-015-002 — Loyalty: "stakeholder" → DOCUMENT + 2.4 route; "manage" → gap

```
source_id: xiigen-b06-0c-015-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-015-002-kedge"
  CompiledText: "Loyalty: stakeholder fires DOCUMENT; manage is gap"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Describe stakeholder expectations for loyalty checkout integration"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — describe fires DOCUMENT, stakeholder fires DOCUMENT; routes to 2.1+2.4"
    - request: "Manage the loyalty program integration project"
      predicates: []
      result: "KNOWLEDGE_EDGE — manage is not a predicate term; project management is a gap"
      capability_gap_id: "gap-loyalty-project-manage-001"

---

## B06-0C-015-003 — Loyalty: "plan" vs "specify" boundary

```
source_id: xiigen-b06-0c-015-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-015-003-kedge"
  CompiledText: "Loyalty: specify fires DOCUMENT; plan does not"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Specify the feature-brief for loyalty rewards checkout"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — specify fires DOCUMENT, feature-brief fires DOCUMENT"
    - request: "Plan the loyalty program rollout for the checkout redesign"
      predicates: []
      result: "KNOWLEDGE_EDGE — plan is not a predicate term; rollout planning is a gap"
      capability_gap_id: "gap-loyalty-rollout-plan-001"
    - request: "Plan and specify the loyalty requirements"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — specify fires DOCUMENT; plan does not fire but specify does"
