# Layer 0-C — Knowledge-Edge Pairs (046)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-046-001 — DragDropList: "automate" gap confirmed

```
source_id: xiigen-b01-0c-046-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A: user_request: "Automate drag-and-drop interaction testing across all supported browser environments."
B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  knowledge_edge_reason: "'automate' confirmed gap verb; 'drag-and-drop' confirmed gap term; 'browser environments' gap"
  matched_predicate_count: 0
  capability_gap_id: "GAP-B01-046-automate-dnd-testing"
  capability_gap_description: "'automate' not in IMPLEMENT lexicon (confirmed round-2 file 036); 'drag-and-drop' interaction terms not in any lexicon; 'browser environments' not in any lexicon"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  next_flow_id: "knowledge-edge-missing-flow"
  FinalHumanResult: null

---

## B01-0C-046-002 — DragDropList: drag/drop gap vocabulary cluster

```
source_id: xiigen-b01-0c-046-002
source_license_state: first_party
pair_kind: knowledge_edge
```

Drag-and-drop gap vocabulary confirmed in file 046:

| Term | Predicate it resembles | Why it does NOT fire |
|---|---|---|
| drag | UI (interaction) | Specific gesture noun; not in UI lexicon |
| drop | UI (interaction) | Specific gesture noun; not in UI lexicon |
| drag handle | UI | Compound; neither part in UI lexicon |
| sortable | UI | Adjective; not in UI lexicon |
| reorder | any | Verb; not in any predicate lexicon |
| drop zone | UI | Compound; not in UI lexicon |
| drag-and-drop | UI | Hyphenated compound; not in UI lexicon |

Exception: "drag-and-drop" + UI term → "create a drag-and-drop widget" → "widget" fires UI.
