# Layer 0-D — Flow Factory Pairs (011)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-011-001 — DOCUMENT sub-phase routing: DocumentationFlow (Phase 2)

```
source_id: xiigen-b12-0d-011-001
flow_type: deterministic_generic
transition_type: documentation
phase: 2
sub_phase: 2.1
pair_kind: factory_selection
```

A:
  run_id: "B12-0D-011-001"
  CompiledPredicateIds: ["DOCUMENT"]
  sub_phase_hint: "2.1"
  request: "Define the database schema for the inventory table."

B:
  FlowFactoryResult:
    FlowType: "deterministic_generic"
    TransitionType: "documentation"
    SelectedFlows:
      - { FlowId: "DocumentationFlow21", SubPhase: "2.1", Predicate: "DOCUMENT", Order: 1 }
    FlowOrder: ["DocumentationFlow21"]
    PrimarySubPhase: "2.1"
    FactoryReason: "Single DOCUMENT predicate. Routes to Phase 2.1 documentation sub-phase.
    No API_CONTRACT → no 3.3 routing. Database schema without API co-occurrence = pure documentation."

---

## B12-0D-011-002 — DOCUMENT secondary routing: when API_CONTRACT is primary

```
source_id: xiigen-b12-0d-011-002
flow_type: deterministic_generic
transition_type: documentation
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"

B:
  FlowFactoryResult:
    SelectedFlows:
      - { FlowId: "APIContractFlow33", SubPhase: "3.3", Predicate: "API_CONTRACT", Order: 1 }
      - { FlowId: "DocumentationFlow21", SubPhase: "2.1", Predicate: "DOCUMENT", Order: 2 }
    FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]
    PrimarySubPhase: "3.3"
    FactoryReason: "API_CONTRACT primary → APIContractFlow33. DOCUMENT secondary → DocumentationFlow21.
    DocumentationFlow21 runs after APIContractFlow33 to capture the contract documentation."

Note: DocumentationFlow21 position in FlowOrder:
  When API_CONTRACT is present: DocumentationFlow21 runs AFTER APIContractFlow33
  When API_CONTRACT is absent: DocumentationFlow21 runs FIRST (only flow)
  When ARCHITECTURE is present: DocumentationFlow21 runs LAST (after architecture and API contract)

---

## B12-0D-011-003 — DocumentationFlow21 vs DocumentationFlow24

```
source_id: xiigen-b12-0d-011-003
pair_kind: factory_reference
```

DocumentationFlow21 (Phase 2.1) — primary documentation routing:
  Used when: DOCUMENT fires as primary or secondary predicate
  Context: standard documentation sub-phase entry
  Most common: "define", "specify", "document", "describe", "annotate"

DocumentationFlow24 (Phase 2.4) — alternate documentation routing:
  Used when: DOCUMENT fires in secondary documentation context
  Context: when updating/annotating existing documentation artifacts
  Less common in B12

Standard B12 DOCUMENT routing:
  All DOCUMENT predicates in B12 → DocumentationFlow21 (2.1)
  No B12 test cases currently use DocumentationFlow24

The DocumentationFlow21 choice for B12 is consistent across all predicate combinations:
  [API_CONTRACT, DOCUMENT] → DocumentationFlow21
  [ARCHITECTURE, API_CONTRACT, DOCUMENT] → DocumentationFlow21
  [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY] → DocumentationFlow21
  All B12 DOCUMENT routings → Phase 2.1 via DocumentationFlow21

---

## B12-0D-011-004 — Complete DocumentationFlow routing scenarios in B12

```
source_id: xiigen-b12-0d-011-004
pair_kind: factory_reference
```

All B12 scenarios where DocumentationFlow21 appears in FlowOrder:

Scenario 1: [DOCUMENT] alone
  FlowOrder: [DocumentationFlow21]
  Position: first and only flow
  SubPhase: 2.1

Scenario 2: [API_CONTRACT, DOCUMENT]
  FlowOrder: [APIContractFlow33, DocumentationFlow21]
  Position: second flow (after API contract)
  SubPhase: 2.1

Scenario 3: [ARCHITECTURE, API_CONTRACT, DOCUMENT]
  FlowOrder: [ArchitectureFlow31, APIContractFlow33, DocumentationFlow21]
  Position: third flow (after architecture and API contract)
  SubPhase: 2.1

Scenario 4: [API_CONTRACT, DOCUMENT, SECURITY]
  FlowOrder: [APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34]
  Position: second flow (parallel with SecurityAuditFlow34)
  SubPhase: 2.1

Scenario 5: [IMPLEMENT, API_CONTRACT, DOCUMENT]
  FlowOrder: [ImplementationFlow43, APIContractFlow33, DocumentationFlow21]
  Position: third flow (last)
  SubPhase: 2.1

In all cases, DocumentationFlow21 routes DOCUMENT predicate to Phase 2.1.
The position in FlowOrder varies by predicate combination, but the sub-phase is always 2.1.
