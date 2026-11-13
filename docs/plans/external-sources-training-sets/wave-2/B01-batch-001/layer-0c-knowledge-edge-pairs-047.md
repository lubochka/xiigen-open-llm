# Layer 0-C — Knowledge-Edge Pairs (047)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-047-001 — ResizablePanel: "resolve" gap confirmed

```
source_id: xiigen-b01-0c-047-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A: user_request: "The resizable panel feels sluggish during resize — investigate and resolve the performance issue."
B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  knowledge_edge_reason: "'sluggish', 'investigate', 'resolve', 'performance issue' are all gap vocabulary"
  matched_predicate_count: 0
  capability_gap_id: "GAP-B01-047-resolve-performance"
  capability_gap_description: "'resolve' not in IMPLEMENT lexicon; 'investigate' not in any lexicon; performance diagnosis language not in any predicate lexicon"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  next_flow_id: "knowledge-edge-missing-flow"
  FinalHumanResult: null

---

## B01-0C-047-002 — New gap verbs confirmed in round-3 (files 037-060)

```
source_id: xiigen-b01-0c-047-002
source_license_state: first_party
pair_kind: knowledge_edge
```

Gap verbs newly confirmed in round-3:

| Verb | Context where tested | Why it does NOT fire IMPLEMENT |
|---|---|---|
| improve | accordion animation (037) | Maintenance verb |
| refactor | dropdown logic (038) | Transformation verb |
| optimize | skeleton rendering (041), scroll (044) | Engineering quality verb |
| resolve | panel performance (047) | Fix/repair verb |
| evaluate | map algorithms (050) | Assessment verb |
| make | video autoplay (053) | Colloquial construction verb |
| scale | share service (059) | Infrastructure operation verb |
| investigate | panel sluggish (047) | Diagnosis verb |
| analyze | audio formats (054) | Analysis verb |
| profile | editor latency (055) | Performance analysis verb |

All gap. None fire IMPLEMENT. None fire any other predicate.
