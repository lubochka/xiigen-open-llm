# Layer 0-C — Knowledge-Edge Pairs (055)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-055-001 — CodeEditor: performance diagnosis gap confirmed

```
source_id: xiigen-b01-0c-055-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A: user_request: "The editor is too slow to type in — profile the keystroke handler and reduce latency."
B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  knowledge_edge_reason: "'profile', 'keystroke handler', 'latency', 'too slow' all gap vocabulary"
  matched_predicate_count: 0
  capability_gap_id: "GAP-B01-055-profile-latency"
  capability_gap_description: "Performance diagnosis verbs ('profile') and metrics ('latency') not in any predicate lexicon; 'keystroke handler' is technical implementation term not in any lexicon"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  next_flow_id: "knowledge-edge-missing-flow"
  FinalHumanResult: null

---

## B01-0C-055-002 — CodeEditor domain gap terms

```
source_id: xiigen-b01-0c-055-002
source_license_state: first_party
pair_kind: knowledge_edge
```

Code editor domain gap terms confirmed:

| Term | Why it does NOT fire |
|---|---|
| syntax highlighting | Domain feature; not in any lexicon |
| auto-complete | Domain feature; not in any lexicon |
| line numbers | Domain feature; not in any lexicon |
| keystroke | Domain noun; not in any lexicon |
| latency | Performance metric; not in any lexicon |
| code folding | Domain feature; not in any lexicon |
| bracket matching | Domain feature; not in any lexicon |
| linting | Domain feature; not in any lexicon |

UI fires on: "component", "theme", "color", "design-system" (from code editor design contexts).
IMPLEMENT fires on creation verbs ("implement", "create", "build").
All code-editor-specific features are gap nouns — rely on co-occurring UI/IMPLEMENT terms.
