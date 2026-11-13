# Layer 0-C — Knowledge-Edge Pairs (042)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-042-001 — EmptyState: "enhance" gap confirmed again

```
source_id: xiigen-b01-0c-042-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A: user_request: "Enhance empty state messaging to feel more friendly and encourage user action."
B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  knowledge_edge_reason: "'enhance' not in IMPLEMENT lexicon; 'messaging', 'friendly', 'encourage' are gap vocabulary"
  matched_predicate_count: 0
  capability_gap_id: "GAP-B01-042-enhance-messaging"
  capability_gap_description: "'enhance' confirmed gap verb (first seen round-2 file 036); UX quality adjectives ('friendly') and persuasion verbs ('encourage') not in any lexicon"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  next_flow_id: "knowledge-edge-missing-flow"
  FinalHumanResult: null

---

## B01-0C-042-002 — "write user stories" — DOCUMENT+IMPLEMENT special rule context

```
source_id: xiigen-b01-0c-042-002
source_license_state: first_party
pair_kind: knowledge_edge
```

Special rule: "write user stories" fires BOTH DOCUMENT and IMPLEMENT.

Parsing breakdown:
  "write" → IMPLEMENT (code artifact verb)
  "user stories" → DOCUMENT (narrative/story predicate term)
  Result: CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]

This is NOT a knowledge-edge case — it routes to 2.1 + 4.1.

Contrast with knowledge-edge case:
  "write user stories for empty state behavior" — DOCUMENT+IMPLEMENT both fire.
  "improve the empty state messaging" — KNOWLEDGE_EDGE (no predicates).
  Key: the presence/absence of a predicate verb is the deciding factor.
