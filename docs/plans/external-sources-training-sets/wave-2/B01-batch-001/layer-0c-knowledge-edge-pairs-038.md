# Layer 0-C — Knowledge-Edge Pairs (038)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-038-001 — DropdownMenu: "refactor" gap confirmed

```
source_id: xiigen-b01-0c-038-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A: user_request: "Refactor the dropdown selection logic to improve reusability and reduce bundle size."
B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  knowledge_edge_reason: "No predicate lexicon terms; 'refactor', 'reusability', 'bundle size' are all gap vocabulary"
  matched_predicate_count: 0
  capability_gap_id: "GAP-B01-038-refactor"
  capability_gap_description: "'refactor' is a common development verb not in IMPLEMENT lexicon; engineering quality terms ('reusability', 'bundle size') not in any lexicon"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  next_flow_id: "knowledge-edge-missing-flow"
  FinalHumanResult: null

---

## B01-0C-038-002 — "refactor" vs "implement": critical distinction

```
source_id: xiigen-b01-0c-038-002
source_license_state: first_party
pair_kind: knowledge_edge
```

Gap verb "refactor" vs IMPLEMENT verbs:

| Verb | Fires IMPLEMENT? | Fires anything? |
|---|---|---|
| build | YES | YES |
| create | YES | YES |
| implement | YES | YES |
| scaffold | YES | YES |
| write (code) | YES | YES |
| add (code) | YES | YES |
| develop | YES | YES |
| refactor | NO | NO |
| rewrite | NO | NO |
| restructure | NO | NO |
| migrate | NO | NO |
| optimize | NO | NO |
| improve | NO | NO |

Rule: Only explicit creation/construction verbs fire IMPLEMENT.
Transformation/maintenance verbs are gap vocabulary.
