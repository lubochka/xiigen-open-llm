# Layer 0-C — Knowledge-Edge Pairs (040)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-040-001 — ProgressBar: "track" and "percentage" gap confirmed

```
source_id: xiigen-b01-0c-040-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A: user_request: "Track progress across wizard steps and show percentage completion to the user."
B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  knowledge_edge_reason: "'track', 'progress', 'wizard steps', 'percentage' are all gap vocabulary"
  matched_predicate_count: 0
  capability_gap_id: "GAP-B01-040-track-progress"
  capability_gap_description: "'track' is a monitoring/analytics verb not in any predicate lexicon; 'progress' as domain noun not in UI lexicon; 'wizard steps' not in any lexicon"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  next_flow_id: "knowledge-edge-missing-flow"
  FinalHumanResult: null

---

## B01-0C-040-002 — Progress-related gap vocabulary list

```
source_id: xiigen-b01-0c-040-002
source_license_state: first_party
pair_kind: knowledge_edge
```

Gap vocabulary cluster: progress/status domain terms

| Term | Why it does NOT fire |
|---|---|
| progress | Domain noun; not in UI lexicon |
| percentage | Domain noun; not in any lexicon |
| completion | Domain noun; not in any lexicon |
| track | Monitoring verb; not in any predicate lexicon |
| wizard steps | Multi-word domain noun; not in any lexicon |
| step (alone) | Domain noun; not in any lexicon |
| status | Domain noun; not in any lexicon |
| milestone | Domain noun; not in any lexicon |

Key rule: "progress bar component" → "component" fires UI; "track progress" → no predicate fires.
