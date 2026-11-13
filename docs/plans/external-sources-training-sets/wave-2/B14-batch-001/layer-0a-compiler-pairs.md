# Layer 0-A — Compiler Pairs
# Batch: B14-batch-001
# Scenario: B14 — DevOps cloud infrastructure — cloud architecture design, sub-phase 3.5
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: compiler

---

## Pair B14B1-WDA-compiler-001 — ARCHITECTURE: cloud infrastructure design

```
source_id: xiigen-b14-compiler-001
transition_type: feature_addition
phase: 3
sub_phase: 3.5
coverage_status: ready
pair_kind: compiler
```

A:
  user_request: "Design the cloud infrastructure architecture for the microservices deployment layer."

B:
  DocumentId: "B14-compiler-001-prompt"
  DocumentTitle: "Cloud infrastructure architecture for microservices deployment"
  CompiledText: "Design the cloud infrastructure architecture for the microservices deployment layer."
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"

  Predicate evidence:
    "design" → ARCHITECTURE
    "infrastructure architecture" → ARCHITECTURE fires on "architecture"
    "microservices" — gap (microservices as an architectural style; not in predicate lexicon)
    "deployment layer" → ARCHITECTURE fires on "layer"
    "cloud" — gap (cloud platform vocabulary)
    Result: ["ARCHITECTURE"] via design + architecture + layer

  Gap warnings:
    - "gap-devops-cloud-001: 'cloud', 'microservices', 'deployment layer' in DevOps context are gap vocabulary"

---

## Pair B14B1-WDA-compiler-002 — ARCHITECTURE + IMPLEMENT: infrastructure code

```
source_id: xiigen-b14-compiler-002
transition_type: feature_addition
phase: 3
sub_phase: 3.5
coverage_status: ready
pair_kind: compiler
```

A:
  user_request: "Architect the service mesh layer and implement the network policy components for the Kubernetes cluster."

B:
  DocumentId: "B14-compiler-002-prompt"
  DocumentTitle: "Service mesh architecture and Kubernetes network policy implementation"
  CompiledText: "Architect the service mesh layer and implement the network policy components for the Kubernetes cluster."
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  SourceStatus: "ready"

  Predicate evidence:
    "architect" → ARCHITECTURE (verb form)
    "service mesh layer" → ARCHITECTURE fires on "layer"; "service" → IMPLEMENT? 
      "service mesh" is an architectural term — "layer" reinforces ARCHITECTURE
    "implement" → IMPLEMENT
    "network policy components" → ARCHITECTURE fires on "components"; IMPLEMENT on "implement"
    "Kubernetes" — gap (platform name)
    "cluster" — gap
    Result: ["ARCHITECTURE", "IMPLEMENT"]

---

## Pair B14B1-WDA-compiler-003 — ARCHITECTURE + DOCUMENT: infrastructure design doc

```
source_id: xiigen-b14-compiler-003
transition_type: documentation
phase: 3
sub_phase: 3.5
coverage_status: ready
pair_kind: compiler
```

A:
  user_request: "Document the cloud architecture blueprint and diagram the infrastructure components and their dependencies."

B:
  DocumentId: "B14-compiler-003-prompt"
  DocumentTitle: "Cloud architecture blueprint documentation and infrastructure diagram"
  CompiledText: "Document the cloud architecture blueprint and diagram the infrastructure components and their dependencies."
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
  SourceStatus: "ready"

  Predicate evidence:
    "document" → DOCUMENT
    "cloud architecture" → ARCHITECTURE fires on "architecture"
    "blueprint" → ARCHITECTURE fires on "blueprint"
    "diagram" → ARCHITECTURE fires on "diagram"
    "infrastructure components" → ARCHITECTURE fires on "components"
    "dependencies" — ARCHITECTURE? In infrastructure context: "dependencies" as structural
      relationships → ARCHITECTURE (component dependencies = architectural relationships)
    "cloud" — gap
    Result: ["ARCHITECTURE", "DOCUMENT"]
