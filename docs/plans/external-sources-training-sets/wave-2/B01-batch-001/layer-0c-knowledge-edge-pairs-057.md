# Layer 0-C — Knowledge-Edge Pairs (057)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-057-001 — RatingStars: "gaming the system" confirmed gap

```
source_id: xiigen-b01-0c-057-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A: user_request: "Users are gaming the rating system — investigate fake review submissions and propose mitigation."
B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  knowledge_edge_reason: "'gaming', 'investigate', 'fake review', 'propose mitigation' all gap vocabulary"
  matched_predicate_count: 0
  capability_gap_id: "GAP-B01-057-gaming-reviews"
  capability_gap_description: "Trust and abuse vocabulary ('gaming', 'fake') not in any predicate lexicon; investigation verbs ('investigate', 'propose') not in any predicate lexicon"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  next_flow_id: "knowledge-edge-missing-flow"
  FinalHumanResult: null

---

## B01-0C-057-002 — Trust/abuse domain gap vocabulary

```
source_id: xiigen-b01-0c-057-002
source_license_state: first_party
pair_kind: knowledge_edge
```

Trust and content moderation gap terms confirmed:

| Term | Why it does NOT fire |
|---|---|
| gaming (the system) | Abuse pattern; not in any lexicon |
| fake review | Content quality noun; not in any lexicon |
| spam | Content quality noun; not in any lexicon |
| moderation | Process noun; not in any lexicon |
| flagging | Process verb/noun; not in any lexicon |
| escalation | Process noun; not in any lexicon |
| mitigation | Strategy noun; not in any lexicon |
| abuse | Trust term; not in any lexicon |
| bot | Technical noun; not in any lexicon |

Note: "document comment moderation use cases" → DOCUMENT fires on "document" and "use cases".
"Moderation" itself does NOT fire DOCUMENT — only "document" and "use cases" fire.
