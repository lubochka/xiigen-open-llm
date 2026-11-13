# Layer 0-C — Knowledge-Edge Pairs (053)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-053-001 — VideoPlayer: "make" gap confirmed

```
source_id: xiigen-b01-0c-053-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A: user_request: "Make video autoplay on mobile without user gesture and handle muted fallback gracefully."
B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  knowledge_edge_reason: "'make' not in IMPLEMENT lexicon; 'autoplay', 'mobile', 'gesture', 'muted fallback' all gap"
  matched_predicate_count: 0
  capability_gap_id: "GAP-B01-053-make-autoplay"
  capability_gap_description: "'make' is colloquial construction verb not in IMPLEMENT lexicon; 'autoplay' is browser behavior term; 'mobile' is platform noun not in any predicate lexicon"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  next_flow_id: "knowledge-edge-missing-flow"
  FinalHumanResult: null

---

## B01-0C-053-002 — "make" vs "create" distinction

```
source_id: xiigen-b01-0c-053-002
source_license_state: first_party
pair_kind: knowledge_edge
```

"make" vs "create" critical distinction:
  "create a video player" → IMPLEMENT fires on "create".
  "make a video player" → IMPLEMENT does NOT fire; "make" is gap.
  "build a player" → IMPLEMENT fires on "build".
  "make the player" → IMPLEMENT does NOT fire.

Common learner error: treating "make" as synonym for "create".
They are semantic equivalents in English but "make" is NOT in the IMPLEMENT lexicon.

Complete IMPLEMENT verb list (confirmed):
  build, implement, create, scaffold, write (code), add (code), develop.
  That is the complete list. "make", "produce", "generate", "construct" are NOT in the list.
