# Layer 0-C — Knowledge-Edge Pairs (004)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-004-001 — "prototype" IS in UI lexicon (B01 context vs B32 gap)

```
source_id: xiigen-b01-0c-004-001
source_title: XIIGen B01 knowledge-edge-004 — prototype is in UI lexicon in B01
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
pair_kind: knowledge_edge
coverage_status: covered
```

A:
  DocumentId: "B01-0c-004-001-prompt"
  CompiledText: "Create interactive prototypes for all the onboarding screens."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate analysis:
  "prototype" (via "prototypes") → UI fires (B01 UI lexicon includes prototype)
  "screen" (via "screens") → UI fires
  "interactive" → maps to "interaction" in UI lexicon → UI fires
  "create" → IMPLEMENT fires
  Result: ["UI", "IMPLEMENT"] — NOT knowledge-edge

B:
  FlowId: "universal-nine-node-development-flow-1.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    knowledge_edge_reason: "null"
    matched_predicate_count: "2"
    recommended_action: "EXECUTE_FLOW"
  Warnings: []

---

## B01-0C-004-002 — B32 vs B01 "prototype" contrast

```
source_id: xiigen-b01-0c-004-002
source_title: XIIGen B01 knowledge-edge-004 — prototype: B32 gap vs B01 UI lexicon member
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

B32 context (vibe coding scenario):
  In B32, "prototype" was treated as gap vocabulary because:
  - The B32 scenario focused on vibe coding (rapid AI-assisted implementation)
  - B32's registered flows covered implementation and testing phases
  - "prototype" in B32 implied a rapid throwaway build, which had no registered flow
  - Result in B32: "Build a prototype" → CompiledPredicateIds: [] → knowledge-edge
    with gap ID assigned

B01 context (SaaS onboarding UX scenario):
  In B01, "prototype" is explicitly in the UI predicate lexicon because:
  - The B01 scenario covers sub-phase 1.2 (Wireframes and Prototypes)
  - Prototyping is a standard UX deliverable in onboarding screen design
  - B01 has a registered flow that handles prototype screen design (ScreenInventoryFlow11)
  - Result in B01: "Build a prototype" → CompiledPredicateIds: ["UI", "IMPLEMENT"]

Rule: Predicate membership is scenario-specific. "prototype" is in the UI lexicon
for ALL B01 sub-scenarios. Do not apply B32 gap treatment to B01 prototype requests.

---

## B01-0C-004-003 — prototype routing consequence in B01

```
source_id: xiigen-b01-0c-004-003
source_title: XIIGen B01 knowledge-edge-004 — prototype routing in B01
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

A:
  DocumentId: "B01-0c-004-003-prompt"
  CompiledText: "Design a clickable prototype for the onboarding wizard with full screen transitions."
  CompiledPredicateIds: ["UI", "ARCHITECTURE"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate analysis:
  "prototype" → UI fires (B01 UI lexicon)
  "screen" (via "screen transitions") → UI fires
  "design" + structural context ("prototype" can be structural) → ARCHITECTURE may fire
  "transitions" → not explicitly in UI lexicon; "interaction" is; "transitions" is
    adjacent. Conservative: only explicitly listed terms fire. ARCHITECTURE fires
    on "design" here due to the structural framing.
  Result: ["UI", "ARCHITECTURE"]

B:
  FlowId: "universal-nine-node-development-flow-1.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    knowledge_edge_reason: "null"
    matched_predicate_count: "2"
    recommended_action: "EXECUTE_FLOW"
    routing_summary: "UI → 1.1; ARCHITECTURE → 3.1"
  Warnings: []

This contrasts with B32 where "Design a prototype" would have been knowledge-edge.
In B01, it routes normally to 1.1 + 3.1.
