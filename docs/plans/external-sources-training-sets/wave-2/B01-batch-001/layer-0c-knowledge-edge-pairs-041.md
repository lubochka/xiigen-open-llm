# Layer 0-C — Knowledge-Edge Pairs (041)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-041-001 — LoadingSkeleton: "optimize" gap confirmed

```
source_id: xiigen-b01-0c-041-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A: user_request: "Optimize skeleton screen rendering performance and reduce layout shift during loading."
B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  knowledge_edge_reason: "'optimize' is gap verb; 'rendering performance', 'layout shift', 'loading' are gap terms"
  matched_predicate_count: 0
  capability_gap_id: "GAP-B01-041-optimize-rendering"
  capability_gap_description: "'optimize' not in IMPLEMENT lexicon (confirmed gap); 'rendering performance' not in any lexicon; 'layout shift' (Cumulative Layout Shift) is a web performance metric not in any lexicon"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  next_flow_id: "knowledge-edge-missing-flow"
  FinalHumanResult: null

---

## B01-0C-041-002 — "skeleton screen" split: context dependency

```
source_id: xiigen-b01-0c-041-002
source_license_state: first_party
pair_kind: knowledge_edge
```

"Skeleton screen" parsing rule:
  "skeleton screen" → "screen" fires UI (lexicon hit); "skeleton" does NOT contribute.
  "loading skeleton" → neither "loading" nor "skeleton" fires UI; requires co-occurring "component" or "widget".
  "skeleton component" → "component" fires UI.
  "render skeleton" → neither "render" nor "skeleton" fires UI or IMPLEMENT.

This split is not an error — it follows strict lexicon matching:
  "screen" IS in UI lexicon; "skeleton" is NOT.
  Two different surface forms ("skeleton screen" vs "loading skeleton") behave differently.
