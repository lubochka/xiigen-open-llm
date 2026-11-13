# Layer 0-C — Knowledge-Edge Pairs (044)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-044-001 — InfiniteScroll: "scroll" confirmed gap in all positions

```
source_id: xiigen-b01-0c-044-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A: user_request: "Optimize infinite scroll performance by using virtual rendering and throttling scroll events."
B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  knowledge_edge_reason: "'optimize', 'scroll', 'virtual rendering', 'throttling' all gap vocabulary"
  matched_predicate_count: 0
  capability_gap_id: "GAP-B01-044-optimize-scroll"
  capability_gap_description: "'scroll' confirmed gap in all positions (verb: 'scroll down'; noun: 'scroll events'; adjective: 'infinite scroll'); 'throttling' is a web API term not in any lexicon"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  next_flow_id: "knowledge-edge-missing-flow"
  FinalHumanResult: null

---

## B01-0C-044-002 — InfiniteScroll: "scroll" gap breadth

```
source_id: xiigen-b01-0c-044-002
source_license_state: first_party
pair_kind: knowledge_edge
```

Gap term "scroll" confirmed in all grammatical positions:
  noun: "scroll events", "scroll position", "scroll container" → NO predicate fires
  verb: "scroll to bottom", "scroll the list" → NO predicate fires
  adjective: "infinite scroll", "virtual scroll" → NO predicate fires
  compound: "scroll bar" → NO predicate fires (neither "scroll" nor "bar" in UI lexicon)

Exception: "scroll" co-occurring with UI lexicon term:
  "create a scroll component" → "component" fires UI; "scroll" does not contribute.
  "build scrollable screen" → "screen" fires UI; "scrollable" does not contribute.

Rule: "scroll" never directly fires any predicate regardless of position.
