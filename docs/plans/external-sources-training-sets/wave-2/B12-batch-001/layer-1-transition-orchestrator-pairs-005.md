# Layer 1 — Transition Orchestrator Pairs (005)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-005-001 — ARCHITECTURE + API_CONTRACT + DOCUMENT: ARCHITECTURE first

```
source_id: xiigen-b12-l1-005-001
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: transition_orchestrator
```

A:
  run_id: "B12-L1-005-001"
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  FlowOrder: ["ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21"]
  user_request: "Design the REST API architecture for the admin panel and document the route structure."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    PrimarySubPhase: "3.3"
    TotalFlows: 3
    ExecutionPlan:
      Phase1_Sequential:
        - Flow: "ArchitectureFlow31"
          SubPhase: "3.1"
          Inputs: ["user_request", "system_context", "admin_panel_requirements"]
          Outputs: ["component map", "service boundaries", "API surface requirements", "route list"]
          Note: "Must complete first — provides route list needed by APIContractFlow33"
      Phase2_Sequential:
        - Flow: "APIContractFlow33"
          SubPhase: "3.3"
          Inputs: ["ArchitectureFlow31.outputs.route_list", "ArchitectureFlow31.outputs.api_surface"]
          Outputs: ["REST endpoint contracts", "route specification", "request/response schemas"]
          Note: "Consumes architecture output — must run after ArchitectureFlow31"
      Phase3_Sequential:
        - Flow: "DocumentationFlow21"
          SubPhase: "2.1"
          Inputs: ["APIContractFlow33.outputs.contracts", "ArchitectureFlow31.outputs.component_map"]
          Outputs: ["Admin API documentation", "route guide"]
          Note: "Captures all design decisions from both prior flows"
    QACheckpoint: "after DocumentationFlow21 completes"
    EstimatedOutputs: 4-6 files

Orchestrator note: Full sequential chain for ARCHITECTURE + API_CONTRACT + DOCUMENT.
Each flow uses the outputs of the preceding flow. No parallelism possible in this pattern.
ARCHITECTURE must run first because API contract work depends on architectural decisions.

---

## B12-L1-005-002 — Why ARCHITECTURE runs first: dependency chain

```
source_id: xiigen-b12-l1-005-002
pair_kind: transition_orchestrator
```

Dependency chain for [ARCHITECTURE, API_CONTRACT, DOCUMENT]:

ArchitectureFlow31 produces:
  - Component list (which services/modules exist)
  - API surface requirements (what APIs are needed)
  - Route list (what URL patterns are needed)
  - Security requirements at architectural level

APIContractFlow33 consumes ArchitectureFlow31 outputs:
  - Uses route list to define specific endpoint contracts
  - Uses component list to determine request/response types
  - Uses security requirements to include auth specifications

DocumentationFlow21 consumes both:
  - Uses APIContractFlow33 output for API contract documentation
  - Uses ArchitectureFlow31 output for architectural context

This creates a strict dependency chain:
  ArchitectureFlow31 → APIContractFlow33 → DocumentationFlow21
  (each depends on the previous; no parallelism)

---

## B12-L1-005-003 — ARCHITECTURE + API_CONTRACT + DOCUMENT outputs

```
source_id: xiigen-b12-l1-005-003
pair_kind: transition_orchestrator
```

Expected outputs for [ARCHITECTURE, API_CONTRACT, DOCUMENT] documentation:

From ArchitectureFlow31 (3.1):
  - Component diagram or description
  - Service boundary definitions
  - API surface specification (high-level)

From APIContractFlow33 (3.3):
  - OpenAPI specification file (detailed endpoint contracts)
  - Route map document
  - Request/response schema definitions

From DocumentationFlow21 (2.1):
  - Developer guide integrating architecture and API contract
  - API reference documentation

Total estimated files: 4-6
TransitionType: documentation (no new code created, architecture and contract documented)

---

## B12-L1-005-004 — Three-flow sequential vs two-flow sequential vs two-flow parallel

```
source_id: xiigen-b12-l1-005-004
pair_kind: transition_orchestrator
```

Execution model comparison:

Two-flow sequential [API_CONTRACT, DOCUMENT]:
  1. APIContractFlow33
  2. DocumentationFlow21 (depends on step 1)
  Total time: 2 units
  
Two-flow parallel [IMPLEMENT, API_CONTRACT]:
  1. ImplementationFlow43 (parallel with APIContractFlow33)
  1. APIContractFlow33 (parallel with ImplementationFlow43)
  Total time: 1 unit
  
Three-flow sequential [ARCHITECTURE, API_CONTRACT, DOCUMENT]:
  1. ArchitectureFlow31
  2. APIContractFlow33 (depends on step 1)
  3. DocumentationFlow21 (depends on step 2)
  Total time: 3 units

Three-flow with mixed [API_CONTRACT, DOCUMENT, SECURITY]:
  1. APIContractFlow33 (sequential)
  2. DocumentationFlow21 (parallel) + SecurityAuditFlow34 (parallel)
  Total time: 2 units

Decision factor for parallelism: can two flows run without depending on each other's output?
  YES → parallel execution
  NO → sequential execution
