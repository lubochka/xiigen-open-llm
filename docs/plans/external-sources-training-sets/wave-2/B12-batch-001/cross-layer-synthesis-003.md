# Cross-Layer Synthesis (003)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## B12-CLS-003 — Full layer-by-layer trace: ARCHITECTURE + API_CONTRACT + DOCUMENT

### Request

"Design the service interface for the event bus and specify the API contract."

---

### Layer 0-A: Compiler

Predicate detection step by step:

Token analysis:
  "design" → ARCHITECTURE fires (design = system design verb, Core Rule 5)
  "service interface"
    → "interface" with "design" present → ARCHITECTURE fires (system design context)
    → "interface" in eventual context with "API contract" → API_CONTRACT fires (API context)
    → Both contexts apply simultaneously → BOTH fire
  "event bus" — integration component; contributes to API context
  "specify" → DOCUMENT fires (specification verb)
  "API" → API_CONTRACT fires (core API term)
  "contract" → API_CONTRACT fires (Core Rule 2: always fires regardless of modifier)

Disambiguation applied (Rule B12-R03 for "interface"):
  "design" present → ARCHITECTURE fires on "interface"
  "API contract" present → API_CONTRACT fires on "interface"
  Both signals simultaneously → BOTH predicates fire from "interface" term

CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
Confidence: 0.93
TransitionType_hint: "documentation" (no "write/implement/build")

---

### Layer 0-B: Router

Routing decisions (one per predicate):
  1. ARCHITECTURE → Phase 3.1 → ArchitectureFlow31 (DecisionCount: 1)
  2. API_CONTRACT → Phase 3.3 → APIContractFlow33 (DecisionCount: 2)
  3. DOCUMENT → Phase 2.1 → DocumentationFlow21 (DecisionCount: 3)

PrimarySubPhase: "3.3"
TotalDecisionCount: 3
TransitionType: "documentation"
FlowOrder: ["ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21"]
FlowOrder rationale: Architecture first (provides component context) → API contract second → documentation last

---

### Layer 0-C: Knowledge Edge (rules applied)

Rule B12-R03 (interface disambiguation):
  "design" present → ARCHITECTURE fires on "interface" ✓
  "API contract" present → API_CONTRACT fires on "interface" ✓
  Both fires simultaneously — no suppression ✓

Rule B12-R04 (design always = ARCHITECTURE):
  "design" → ARCHITECTURE fires ✓
  "design" does NOT fire IMPLEMENT ✓

Rule B12-R05 (design + API = both):
  "design" + "API contract" → ARCHITECTURE + API_CONTRACT ✓

Rule B12-R01 (contract always):
  "API contract" → "contract" → API_CONTRACT fires ✓

Rule B12-R10 (TransitionType):
  No "write/implement/build" → documentation TransitionType ✓

---

### Layer 0-D: Flow Factory

Factory for [ARCHITECTURE, API_CONTRACT, DOCUMENT]:
  FlowType: "probationary_accepted" (three-predicate with ARCHITECTURE pattern has limited training)
  SelectedFlows:
    - ArchitectureFlow31 (3.1) for ARCHITECTURE
    - APIContractFlow33 (3.3) for API_CONTRACT
    - DocumentationFlow21 (2.1) for DOCUMENT
  FlowOrder: ["ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21"]
  TransitionType: "documentation"

---

### Layer 1: Transition Orchestrator

ExecutionPlan:
  Phase1_Sequential: ArchitectureFlow31
    - Inputs: user_request, event bus system context
    - Outputs: service component map, API surface requirements, event bus topology
    - Must complete first (APIContractFlow33 depends on its outputs)
  Phase2_Sequential: APIContractFlow33
    - Inputs: ArchitectureFlow31.outputs.api_surface + ArchitectureFlow31.outputs.component_map
    - Outputs: event bus API contract specification, interface definitions
  Phase3_Sequential: DocumentationFlow21
    - Inputs: ArchitectureFlow31.outputs + APIContractFlow33.outputs
    - Outputs: event bus architecture and API documentation
  QACheckpoint: after DocumentationFlow21

Full sequential chain — no parallelism (each depends on previous).
TransitionType: "documentation"

---

### Layer 2: Nine-Node Flow (all three flows, summary)

ArchitectureFlow31 (3.1):
  N09 output: event-bus-component-map.md, api-surface-requirements.md

APIContractFlow33 (3.3):
  N01: receives architecture outputs
  N09 output: event-bus-api-contract.yaml (publish/consume endpoint definitions)

DocumentationFlow21 (2.1):
  N01: receives both architecture and API contract outputs
  N09 output: event-bus-developer-guide.md

GateB for APIContractFlow33: PASS — "event-bus-api-contract.yaml defines publish and consume endpoints"

---

### Teaching Layer

Rules exercised:
  B12-R03: "interface" fires BOTH ARCHITECTURE and API_CONTRACT (dual context) ✓
  B12-R04: "design" → ARCHITECTURE (system design verb) ✓
  B12-R05: "design" + "API" → ARCHITECTURE + API_CONTRACT ✓
  B12-R01: "contract" → API_CONTRACT always ✓
  Orchestrator ordering: ArchitectureFlow31 FIRST → APIContractFlow33 SECOND → DocumentationFlow21 THIRD ✓

Artifacts: 4-5 files
  1. event-bus-component-map.md (from ArchitectureFlow31)
  2. api-surface-requirements.md (from ArchitectureFlow31)
  3. event-bus-api-contract.yaml (from APIContractFlow33)
  4. event-bus-developer-guide.md (from DocumentationFlow21)
