# Layer 0-C — Knowledge-Edge Pairs (037)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-037-001 — AccordionPanel: animation gap confirmed

```
source_id: xiigen-b01-0c-037-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A: user_request: "Improve the accordion animation speed and transition timing for smoother user experience."
B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  knowledge_edge_reason: "No predicate lexicon terms present; 'improve', 'animation', 'transition timing' are all gap vocabulary"
  matched_predicate_count: 0
  capability_gap_id: "GAP-B01-037-animation-improve"
  capability_gap_description: "Animation control verbs ('improve', 'optimize', 'tune') and animation nouns ('animation', 'transition timing') are not in any predicate lexicon"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  next_flow_id: "knowledge-edge-missing-flow"
  FinalHumanResult: null

---

## B01-0C-037-002 — "improve" reinforcement: gap verb confirmed across contexts

```
source_id: xiigen-b01-0c-037-002
source_license_state: first_party
pair_kind: knowledge_edge
```

Gap verb "improve" behavior:
  First confirmed: round-2 file 036 (gap from UI+IMPLEMENT family).
  Confirmed again in: 037, 041 (optimize), 046 (automate), 047 (resolve).
  Rule: "improve" does NOT fire IMPLEMENT regardless of context.
  Counterexample (common error): "improve the component" — STILL no IMPLEMENT fire.
  Correct: "build an improved component" — "build" fires IMPLEMENT; "improve" does not.
