# Layer 0-C — Knowledge-Edge Pairs (036)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-036-001 — Round-2 gap vocabulary complete list

```
source_id: xiigen-b01-0c-036-r2-gap-list
source_license_state: first_party
pair_kind: knowledge_edge
```

Gap vocabulary introduced or reinforced in round-2 (files 025–036):

| Term | Predicate it resembles | Why it does NOT fire |
|---|---|---|
| breadcrumb | UI | Domain noun; not in UI lexicon |
| tooltip | UI | Domain noun; not in UI lexicon |
| tab (alone) | UI | Domain noun; not in UI lexicon |
| gallery | UI | Domain noun; not in UI lexicon |
| pagination (alone) | UI | Domain noun; not in UI lexicon |
| popup | UI | "modal" is in lexicon; "popup" is not |
| thumbnail | UI | Domain noun; not in any lexicon |
| hero (alone) | UI | Domain noun; not in UI lexicon |
| dark mode | UI | Compound; "mode" not in lexicon |
| animate / animation | any | Not in any predicate lexicon |
| scroll | any | Not in any predicate lexicon |
| hover | any | Not in any predicate lexicon |
| lazy loading | any | Not in any predicate lexicon |
| enhance | IMPLEMENT | Gap verb; not in IMPLEMENT lexicon |
| integrate | IMPLEMENT | Gap verb; confirmed from file 024 |
| automate | IMPLEMENT | Gap verb; confirmed from file 024 |
| allow | IMPLEMENT | Gap verb; confirmed from file 024 |
| run | IMPLEMENT | Gap verb; confirmed from file 024 |
| card sorting | UI | UX research methodology; not in any lexicon |
| usability | any | Not in any predicate lexicon |

---

## B01-0C-036-002 — Knowledge-edge result path fields (complete)

```
source_id: xiigen-b01-0c-036-ke-fields
source_license_state: first_party
pair_kind: knowledge_edge
```

Required fields when OverallVerdict is "KNOWLEDGE_EDGE":
  knowledge_edge_reason: [description of what gap vocabulary was present]
  matched_predicate_count: 0
  capability_gap_id: [generated gap identifier]
  capability_gap_description: [description of missing lexicon coverage]
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  next_flow_id: "knowledge-edge-missing-flow"
  FinalHumanResult: null

Prohibited: any sub-phase assignment when knowledge-edge fires.
  KnowledgeEdge: true always paired with SubPhaseId: null.
