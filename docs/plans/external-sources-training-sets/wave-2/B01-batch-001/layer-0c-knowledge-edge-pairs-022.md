# Layer 0-C — Knowledge-Edge Pairs (022)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note (022)

Unroutable cases: inputs where ALL predicate signals are gap vocabulary
and the system correctly produces KNOWLEDGE_EDGE with no sub-phase routing.

---

## B01-0C-022-001 — unroutable: "dark mode" — no predicate terms

```
source_id: xiigen-b01-0c-022-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Support dark mode across all product surfaces."
  CompiledPredicateIds: []
  Note: "dark mode" — "mode" and "dark" not in UI lexicon.
  "support" NOT in IMPLEMENT. "surfaces" not in any lexicon.

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics:
    knowledge_edge_reason: "no_predicate_match"
    capability_gap_id: "gap-dark-mode-001"
    capability_gap_description: "'dark mode' has no registered predicate. 'surfaces' is not a UI term."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"

---

## B01-0C-022-002 — unroutable: "A/B test" — UX research without UI terms

```
source_id: xiigen-b01-0c-022-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Run an A/B test comparing two checkout flows to determine which has higher conversion."
  CompiledPredicateIds: []
  Note: "A/B test", "checkout flows", "conversion" — none are predicate terms.
  "run" NOT in IMPLEMENT.

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics:
    knowledge_edge_reason: "no_predicate_match"
    capability_gap_id: "gap-ab-test-001"
    capability_gap_description: "A/B testing methodology. No UI, DOCUMENT, IMPLEMENT, or ARCHITECTURE terms present."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"

---

## B01-0C-022-003 — unroutable: "brand guidelines" — no predicate

```
source_id: xiigen-b01-0c-022-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Align the brand guidelines across all team materials."
  CompiledPredicateIds: []
  Note: "brand guidelines", "align", "team materials" — no predicate terms.

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics:
    knowledge_edge_reason: "no_predicate_match"
    capability_gap_id: "gap-brand-guidelines-001"
    capability_gap_description: "Brand guidelines task has no registered development predicate."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
