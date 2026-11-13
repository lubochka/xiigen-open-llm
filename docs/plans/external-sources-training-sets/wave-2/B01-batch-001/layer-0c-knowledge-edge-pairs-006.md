# Layer 0-C — Knowledge-Edge Pairs (006)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-006-001 — "design" + structural noun → ARCHITECTURE fires

```
source_id: xiigen-b01-0c-006-001
source_title: XIIGen B01 knowledge-edge-006 — "design" + structural noun = ARCHITECTURE
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
pair_kind: knowledge_edge
coverage_status: covered
```

A:
  DocumentId: "B01-0c-006-001-prompt"
  CompiledText: "Design the architecture of the onboarding screen routing system."
  CompiledPredicateIds: ["UI", "ARCHITECTURE"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate analysis:
  "design" + "architecture" → ARCHITECTURE fires (both terms co-present)
  "screen" → UI fires (UI lexicon)
  Result: ["UI", "ARCHITECTURE"]

B:
  FlowId: "universal-nine-node-development-flow-1.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    knowledge_edge_reason: "null"
    matched_predicate_count: "2"
    routing_summary: "UI → 1.1; ARCHITECTURE → 3.1"
    recommended_action: "EXECUTE_FLOW"
  Warnings: []

ARCHITECTURE trigger rules in B01:

  Trigger 1: "design" + "architecture" both present → ARCHITECTURE fires
  Trigger 2: "design" + system noun (system, module, pipeline, schema, infrastructure)
             → ARCHITECTURE fires
  Trigger 3: "architecture" alone (without "design") → ARCHITECTURE fires
  Trigger 4: "design" + UI noun only → ARCHITECTURE does NOT fire
             (e.g., "design the button" → only UI fires)

---

## B01-0C-006-002 — "design" + UI noun: ARCHITECTURE does NOT fire

```
source_id: xiigen-b01-0c-006-002
source_title: XIIGen B01 knowledge-edge-006 — "design" + UI noun: no ARCHITECTURE
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

A:
  DocumentId: "B01-0c-006-002-prompt"
  CompiledText: "Design the form input styles for the onboarding screens."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate analysis:
  "design" + "form" (UI noun, not structural) → ARCHITECTURE does NOT fire
  "form" → UI fires
  "input" → UI fires
  "screen" (via "screens") → UI fires
  Result: ["UI"] only — ARCHITECTURE absent because "design" + "form"/"input"/"screen"
  are UI terms, not structural/architectural nouns

B:
  FlowId: "universal-nine-node-development-flow-1.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    knowledge_edge_reason: "null"
    matched_predicate_count: "1"
    recommended_action: "EXECUTE_FLOW"
  Warnings: []

---

## B01-0C-006-003 — "architecture" alone fires ARCHITECTURE

```
source_id: xiigen-b01-0c-006-003
source_title: XIIGen B01 knowledge-edge-006 — "architecture" alone fires ARCHITECTURE
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

A:
  DocumentId: "B01-0c-006-003-prompt"
  CompiledText: "Document the service architecture for the SaaS onboarding module."
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate analysis:
  "architecture" alone → ARCHITECTURE fires (no "design" required when "architecture"
    is explicitly present)
  "document" → DOCUMENT fires
  No UI terms present
  Result: ["ARCHITECTURE", "DOCUMENT"]
  Routes to: 3.1 (ARCHITECTURE) + 2.1 (DOCUMENT)
  TransitionType: documentation (IMPLEMENT absent)

B:
  FlowId: "universal-nine-node-development-flow-3.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    knowledge_edge_reason: "null"
    matched_predicate_count: "2"
    routing_summary: "ARCHITECTURE → 3.1; DOCUMENT → 2.1"
    recommended_action: "EXECUTE_FLOW"
  Warnings: []
