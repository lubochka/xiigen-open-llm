# Layer 0-C — Knowledge-Edge Pairs (050)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-050-001 — MapComponent: "evaluate" gap confirmed

```
source_id: xiigen-b01-0c-050-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A: user_request: "Evaluate map clustering algorithms to determine the best approach for 50,000 location pins."
B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  knowledge_edge_reason: "'evaluate', 'clustering algorithms', 'location pins' all gap vocabulary"
  matched_predicate_count: 0
  capability_gap_id: "GAP-B01-050-evaluate-algorithms"
  capability_gap_description: "'evaluate' is an assessment verb not in any predicate lexicon; 'clustering', 'algorithm', 'location pins' are domain-specific terms not in any lexicon"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  next_flow_id: "knowledge-edge-missing-flow"
  FinalHumanResult: null

---

## B01-0C-050-002 — MapComponent: "integrate" gap reinforcement

```
source_id: xiigen-b01-0c-050-002
source_license_state: first_party
pair_kind: knowledge_edge
```

"integrate" gap behavior (reinforced from round-2):
  "integrate the mapping API service" → ARCHITECTURE fires on "API" and "service".
  "integrate" itself does NOT fire IMPLEMENT.
  IMPLEMENT fires only on "implement" (separate verb in the same sentence).
  
Correct parsing: "Integrate the API and implement the pipeline"
  → ARCHITECTURE fires on "API"; IMPLEMENT fires on "implement"; "integrate" contributes nothing.
  
Error to avoid: treating "integrate" as equivalent to "implement".
  "Integrate X" → ARCHITECTURE may fire if X contains structural terms.
  "Integrate X" alone → depends entirely on X, not on "integrate".
