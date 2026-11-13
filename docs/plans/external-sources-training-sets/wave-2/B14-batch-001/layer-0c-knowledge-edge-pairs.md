# Layer 0-C — Knowledge Edge Pairs
# Batch: B14-batch-001
# Scenario: B14 — DevOps cloud infrastructure, sub-phase 3.5
# Contract: KnowledgeEdgeHandlerUnit : ILogicalUnit<SubPhaseRouterResult, CapabilityGapSignal>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: knowledge_edge

---

## Pair B14B1-WDA-edge-001 — "scale" alone does not fire ARCHITECTURE

```
source_id: xiigen-b14-edge-001
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: knowledge_edge
```

A:
  user_request: "Scale the application to handle ten times the current traffic."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  PhaseId: null
  SubPhaseId: null
  SourceStatus: "knowledge_edge"
  Confidence: 0.70

  CapabilityGapSignal:
    gap_id: "gap-devops-scaling-001"
    gap_vocabulary: ["scale", "ten times", "current traffic"]
    DomainId: "software-development"
    SCHEDULE_CAPABILITY_LEARNING: true

  Predicate evidence:
    "scale" — not in any predicate lexicon (scale ≠ ARCHITECTURE ≠ IMPLEMENT)
    "application" — not in any predicate (application as noun)
    "ten times the current traffic" — operational metric; no predicate
    Result: zero predicates → knowledge_edge

  Critical boundary: "scale" is a frequent gap term in DevOps contexts. ARCHITECTURE
  fires on: design, architect, structure, layer, component, diagram, blueprint, plan,
  pattern, framework. "Scale" as an operational objective has no predicate.

---

## Pair B14B1-WDA-edge-002 — cloud platform names are gap vocabulary

```
source_id: xiigen-b14-edge-002
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: knowledge_edge
```

A:
  user_request: "Set up AWS ECS (Elastic Container Service) for the containerized microservices."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  PhaseId: null
  SubPhaseId: null
  SourceStatus: "knowledge_edge"
  Confidence: 0.70

  CapabilityGapSignal:
    gap_id: "gap-devops-cloud-001"
    gap_vocabulary: ["AWS ECS", "ECS", "containerized", "microservices", "set up"]
    DomainId: "software-development"
    SCHEDULE_CAPABILITY_LEARNING: true

  Predicate evidence:
    "AWS" — gap (cloud provider name)
    "ECS" — gap (service name abbreviation)
    "set up" — gap (not in IMPLEMENT lexicon)
    "containerized" — gap (container technology vocabulary)
    "microservices" — gap (architectural style term, not in predicate lexicon)
    Result: zero predicates → knowledge_edge

  Cloud platform names (AWS, Azure, GCP, ECS, EKS, S3, Lambda) are all gap vocabulary.
  Predicates fire on design/implement/structure action verbs, not on platform names.

---

## Pair B14B1-WDA-edge-003 — "infrastructure as code" vocabulary: IMPLEMENT fires

```
source_id: xiigen-b14-edge-003
transition_type: feature_addition
phase: 4
sub_phase: 4.1
coverage_status: ready
pair_kind: knowledge_edge
```

A:
  user_request: "Write the Terraform modules to provision the VPC (Virtual Private Cloud) and networking infrastructure."

B:
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  TransitionType: feature_addition
  PhaseId: 4
  SubPhaseId: 4.1
  SourceStatus: "ready"
  Confidence: 0.92

  Predicate evidence:
    "write" → IMPLEMENT (write is an IMPLEMENT action verb)
    "modules" → IMPLEMENT (module as a software/code unit)
    "Terraform" — gap (IaC tool name; not in predicate lexicon)
    "provision" — gap (DevOps operation term)
    "VPC" — gap (networking abbreviation)
    "networking infrastructure" — ARCHITECTURE fires on "infrastructure" in a structural context?
      "networking infrastructure" → ARCHITECTURE via "infrastructure" as a structural noun.
    Result: ["IMPLEMENT", "ARCHITECTURE"]
    CapabilityGapWarnings:
      - "gap-devops-cloud-001: 'Terraform', 'provision', 'VPC' are gap vocabulary"

  Teaching: "Write modules" fires IMPLEMENT via write + module. "Networking infrastructure"
  fires ARCHITECTURE via "infrastructure" as a structural/architectural noun. Tool names
  (Terraform) are gap but don't suppress the predicate that fires on the action vocabulary.
