# Cross-Layer Synthesis (003)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## S003 — End-to-end trace: Gap path — "Set up Kubernetes for production"

### Layer 0-A
Input: "Set up Kubernetes for our production workloads."
  "set up" → gap | "Kubernetes" → gap (platform name) | "production workloads" → gap
  CompiledPredicateIds: [] → gap

### Layer 0-B
  No predicates. Router: GapHandler path.
  SCHEDULE_CAPABILITY_LEARNING emitted to subproject 16 (AutonomousLearningPipeline (16)).
  DomainId: "software-development"

### Layer 0-D
  FlowFactory.CreateGap() → GapFlow | No InfrastructureArchitectureFlow35

### Layer 1
  No TransitionType assigned (gap path)

### Layer 2
  GapHandler: "The request specifies a platform configuration action. Please provide a design objective: design the cluster architecture, architect the namespace topology, or define the pod scheduling strategy."

### Layer 3
  Teaching: "set up" + platform name = operational gap. Neither ARCHITECTURE nor IMPLEMENT fires without design verbs.

### Cross-layer invariant
  Gap path emits SCHEDULE_CAPABILITY_LEARNING. No flow instantiated. GapHandler returns clarification.

---

## S003-B — Contrast: "Design the Kubernetes cluster architecture"

### Layer 0-A
  "design" → ARCHITECTURE | "Kubernetes cluster architecture" → ARCHITECTURE ("cluster", "architecture")
  CompiledPredicateIds: ["ARCHITECTURE"]
  SubPhase: 3.5 (infra vocabulary: Kubernetes, cluster)
  Teaching: Adding "design" + "architecture" transforms the operational gap into a routable ARCHITECTURE request.

