# Layer 0-D — Flow Factory Pairs (006)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-006-001 — ARCHITECTURE + API_CONTRACT factory: ArchitectureFlow31 + APIContractFlow33

```
source_id: xiigen-b12-0d-006-001
flow_type: deterministic_generic
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: factory_selection
```

A:
  run_id: "B12-0D-006-001"
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"
  request: "Design the REST API architecture for the admin panel and document the route structure."

B:
  FlowType: "deterministic_generic"
  SelectedFlows:
    - predicate: "ARCHITECTURE"
      flow: "ArchitectureFlow31"
      sub_phase: "3.1"
    - predicate: "API_CONTRACT"
      flow: "APIContractFlow33"
      sub_phase: "3.3"
    - predicate: "DOCUMENT"
      flow: "DocumentationFlow21"
      sub_phase: "2.1"
  PrimaryFlow: "APIContractFlow33"
  TransitionType: "documentation"
  FlowOrder: ["ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21"]
  FactoryReason: "Three predicates → three flows. ARCHITECTURE → ArchitectureFlow31.
  API_CONTRACT → APIContractFlow33 (primary for B12). DOCUMENT → DocumentationFlow21."

---

## B12-0D-006-002 — Factory logic for multi-predicate ARCHITECTURE + API_CONTRACT

```
source_id: xiigen-b12-0d-006-002
flow_type: deterministic_generic
pair_kind: factory_selection
```

Multi-flow factory selection logic for [ARCHITECTURE, API_CONTRACT, DOCUMENT]:

Step 1: Scan CompiledPredicateIds for each predicate
Step 2: For each predicate, query flow registry:
  - ARCHITECTURE → registry returns "ArchitectureFlow31" for sub-phase 3.1
  - API_CONTRACT → registry returns "APIContractFlow33" for sub-phase 3.3
  - DOCUMENT → registry returns "DocumentationFlow21" for sub-phase 2.1
Step 3: Check learned flow availability (trusted_learned or probationary_accepted)
  - For [ARCHITECTURE, API_CONTRACT, DOCUMENT] three-predicate pattern: deterministic_generic
  - No learned combination flow registered for this three-predicate pattern
Step 4: Compose FlowOrder:
  - Architecture first (planning before API contract work)
  - API_CONTRACT second (primary B12 flow)
  - Documentation last (captures decisions from prior flows)
Step 5: TransitionType = documentation (no IMPLEMENT predicate)

Result: three separate flows composed into ordered FlowOrder list.

---

## B12-0D-006-003 — ArchitectureFlow31 + APIContractFlow33 interaction

```
source_id: xiigen-b12-0d-006-003
flow_type: deterministic_generic
pair_kind: factory_selection
```

How ArchitectureFlow31 and APIContractFlow33 interact in sequence:

ArchitectureFlow31 (Phase 3.1) outputs:
  - Service component map
  - Boundary definitions
  - API surface requirements (what endpoints are needed)

APIContractFlow33 (Phase 3.3) receives ArchitectureFlow31 outputs and produces:
  - Specific endpoint definitions (using API surface requirements from 3.1)
  - Request/response schemas
  - Route patterns matching the architecture

This input-output relationship is why ArchitectureFlow31 runs first in FlowOrder.
APIContractFlow33 uses the architectural decisions to produce the specific API contracts.

---

## B12-0D-006-004 — Factory output when ARCHITECTURE fires: complete structure

```
source_id: xiigen-b12-0d-006-004
flow_type: deterministic_generic
pair_kind: factory_selection
```

Complete factory output structure for [ARCHITECTURE, API_CONTRACT, DOCUMENT]:

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"

B:
  FlowFactoryResult:
    FlowType: "deterministic_generic"
    TransitionType: "documentation"
    TotalFlows: 3
    PrimarySubPhase: "3.3"
    SelectedFlows:
      - { FlowId: "ArchitectureFlow31", SubPhase: "3.1", Predicate: "ARCHITECTURE", Order: 1 }
      - { FlowId: "APIContractFlow33", SubPhase: "3.3", Predicate: "API_CONTRACT", Order: 2 }
      - { FlowId: "DocumentationFlow21", SubPhase: "2.1", Predicate: "DOCUMENT", Order: 3 }
    FlowOrder: ["ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21"]
    ParallelEligible: []  (all three are sequential in this case)
    FactoryNotes: "Architecture must complete before API contract. Documentation after both."
