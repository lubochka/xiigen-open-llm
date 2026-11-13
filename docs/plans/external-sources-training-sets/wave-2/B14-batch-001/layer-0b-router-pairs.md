# Layer 0-B — Router Pairs
# Batch: B14-batch-001
# Scenario: B14 — DevOps cloud infrastructure, sub-phase 3.5
# Contract: PromptRoutingInputRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRouterResult>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: router

---

## Pair B14B1-WDA-router-001 — ARCHITECTURE → 3.5

```
source_id: xiigen-b14-router-001
transition_type: feature_addition
phase: 3
sub_phase: 3.5
pair_kind: router
```

A:
  compiled_predicate_ids: ["ARCHITECTURE"]
  compiled_text: "Design the cloud infrastructure architecture for the microservices deployment layer."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      sub_phase_label: "Infrastructure and DevOps Architecture"
      confidence: 0.92
      routing_note: "'design' + 'infrastructure architecture' + 'layer' → ARCHITECTURE → 3.5 (cloud/infra context)"

  TransitionType: feature_addition
  PhaseId: 3
  SubPhaseId: 3.5
  CapabilityGapWarnings:
    - "gap-devops-cloud-001: 'cloud', 'microservices' are gap vocabulary"

  Sub-phase disambiguation: ARCHITECTURE routes to 3.5 (Infrastructure and DevOps Architecture)
  when the context involves infrastructure, cloud, deployment layers, or network topology.
  ARCHITECTURE routes to 3.1 (System Architecture) for application-level design (service
  boundaries, domain architecture). The "infrastructure" and "deployment layer" vocabulary
  signals 3.5.

---

## Pair B14B1-WDA-router-002 — ARCHITECTURE + IMPLEMENT → 3.5 + 4.1

```
source_id: xiigen-b14-router-002
transition_type: feature_addition
phase: 3
sub_phase: 3.5
pair_kind: router
```

A:
  compiled_predicate_ids: ["ARCHITECTURE", "IMPLEMENT"]
  compiled_text: "Architect the service mesh layer and implement the network policy components for the Kubernetes cluster."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      sub_phase_label: "Infrastructure and DevOps Architecture"
      confidence: 0.92
      routing_note: "'architect' + 'layer' + 'components' → ARCHITECTURE → 3.5 (network policy = infra)"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      sub_phase_label: "Service and Logic Implementation"
      confidence: 0.92
      routing_note: "'implement' + 'components' → IMPLEMENT → 4.1"

  TransitionType: feature_addition
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 4.1]

---

## Pair B14B1-WDA-router-003 — ARCHITECTURE + DOCUMENT → 3.5 + 6.2

```
source_id: xiigen-b14-router-003
transition_type: documentation
phase: 3
sub_phase: 3.5
pair_kind: router
```

A:
  compiled_predicate_ids: ["ARCHITECTURE", "DOCUMENT"]
  compiled_text: "Document the cloud architecture blueprint and diagram the infrastructure components and their dependencies."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      sub_phase_label: "Infrastructure and DevOps Architecture"
      confidence: 0.92
      routing_note: "'architecture' + 'blueprint' + 'diagram' + 'infrastructure components' → ARCHITECTURE → 3.5"
    - predicate: DOCUMENT
      sub_phase: 6.2
      sub_phase_label: "Documentation and Knowledge Base"
      confidence: 0.92
      routing_note: "'document' → DOCUMENT → 6.2"

  TransitionType: documentation
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 6.2]
