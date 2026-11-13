# Layer 0-C — Knowledge-Edge Pairs (049)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-049-001 — ChartsAndGraphs: "explain" confirmed gap

```
source_id: xiigen-b01-0c-049-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A: user_request: "Explain how SVG chart rendering works and why canvas-based charts perform better for large datasets."
B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  knowledge_edge_reason: "'explain' not in any predicate lexicon; 'SVG', 'rendering', 'canvas' are gap terms"
  matched_predicate_count: 0
  capability_gap_id: "GAP-B01-049-explain-rendering"
  capability_gap_description: "Educational/explanatory verbs ('explain', 'describe how') not in any predicate lexicon; technical implementation nouns ('SVG', 'canvas', 'rendering') not in any lexicon"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  next_flow_id: "knowledge-edge-missing-flow"
  FinalHumanResult: null

---

## B01-0C-049-002 — ARCHITECTURE fires on "schema" alone but not "define"

```
source_id: xiigen-b01-0c-049-002
source_license_state: first_party
pair_kind: knowledge_edge
```

Key parsing rule for ARCHITECTURE:
  "Define the chart API schema" → ARCHITECTURE fires on "API" and "schema".
  "define" alone does NOT fire ARCHITECTURE (gap verb).
  "API" alone fires ARCHITECTURE (structural term).
  "schema" alone fires ARCHITECTURE (structural term).
  
Contrast:
  "Define the chart color palette" → "define" is gap; "color" is UI; "palette" NOT in any lexicon.
  Result: CompiledPredicateIds: ["UI"] (only "color" fires).
  
Rule: "define" is a gap verb but ARCHITECTURE noun terms fire independently of the verb.
ARCHITECTURE fires on the presence of structural nouns, not on the verb.
