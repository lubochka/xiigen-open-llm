# Layer 0-C — Knowledge-Edge Pairs (059)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-059-001 — SocialShare: "scale" gap confirmed + API incident framing

```
source_id: xiigen-b01-0c-059-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A: user_request: "The sharing button went viral and broke our API rate limit — scale the service to handle the spike."
B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  knowledge_edge_reason: "'scale', 'viral', 'broke', 'spike' are gap vocabulary; 'API' appears but incident framing prevents ARCHITECTURE fire"
  matched_predicate_count: 0
  capability_gap_id: "GAP-B01-059-scale-incident"
  capability_gap_description: "'scale' is an infrastructure operation verb not in any predicate lexicon; incident/operations framing ('broke', 'spike', 'viral') not in any lexicon; ARCHITECTURE requires structural design verbs, not incident verbs"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  next_flow_id: "knowledge-edge-missing-flow"
  FinalHumanResult: null

---

## B01-0C-059-002 — When "API" does NOT fire ARCHITECTURE

```
source_id: xiigen-b01-0c-059-002
source_license_state: first_party
pair_kind: knowledge_edge
```

Edge case: "API" in incident/operational context:
  "scale the service to handle API load" → "scale" is gap verb; operations context → KNOWLEDGE_EDGE.
  "our API broke" → incident reporting, not architecture design → KNOWLEDGE_EDGE.
  "monitor the API" → "monitor" is gap verb → KNOWLEDGE_EDGE.
  
When "API" DOES fire ARCHITECTURE:
  "define the API schema" → "schema" fires ARCHITECTURE; "API" reinforces.
  "implement the API service" → ARCHITECTURE fires on "API" + "service"; IMPLEMENT fires on "implement".
  "design the API architecture" → ARCHITECTURE fires on "API" + "architecture".
  
Rule: ARCHITECTURE requires structural design context for "API" to fire.
Incident, monitoring, and scaling operations are not structural design → gap.
