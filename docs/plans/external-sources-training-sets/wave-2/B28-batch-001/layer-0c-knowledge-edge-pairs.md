# Layer 0-C — Knowledge Edge Pairs
# Batch: B28-batch-001
# Scenario: B28 — QA automation DevOps, sub-phase 7.1
# Contract: KnowledgeEdgeHandlerUnit : ILogicalUnit<SubPhaseRouterResult, CapabilityGapSignal>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: knowledge_edge

---

## Pair B28B1-WDA-edge-001 — CI/CD vocabulary is gap

```
source_id: xiigen-b28-edge-001
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: knowledge_edge
```

A:
  user_request: "Set up the CI/CD pipeline for continuous deployment to the staging environment."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  PhaseId: null
  SubPhaseId: null
  SourceStatus: "knowledge_edge"
  Confidence: 0.70

  CapabilityGapSignal:
    gap_id: "gap-devops-cicd-001"
    gap_vocabulary: ["CI/CD", "continuous deployment", "staging environment", "set up"]
    DomainId: "software-development"
    SCHEDULE_CAPABILITY_LEARNING: true

  Predicate evidence:
    "CI/CD" — not in any predicate lexicon (DevOps abbreviation)
    "continuous deployment" — gap (DevOps vocabulary)
    "staging environment" — gap
    "set up" — gap (not in IMPLEMENT lexicon)
    "pipeline" — ARCHITECTURE? Only with design/structure action verb — "set up" carries no predicate
    Result: zero predicates → knowledge_edge

---

## Pair B28B1-WDA-edge-002 — "deploy" does not fire any predicate

```
source_id: xiigen-b28-edge-002
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: knowledge_edge
```

A:
  user_request: "Deploy the application to the production environment."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  PhaseId: null
  SubPhaseId: null
  SourceStatus: "knowledge_edge"
  Confidence: 0.70

  CapabilityGapSignal:
    gap_id: "gap-devops-deployment-001"
    gap_vocabulary: ["deploy", "production environment"]
    DomainId: "software-development"
    SCHEDULE_CAPABILITY_LEARNING: true

  Predicate evidence:
    "deploy" — not in any predicate lexicon (deploy ≠ IMPLEMENT ≠ QA)
    "production environment" — gap
    "application" — not in any predicate (application as noun, not action)
    Result: zero predicates → knowledge_edge

  DevOps deployment vocabulary (deploy, rollout, provision, push to production) is entirely
  gap. It is distinct from the IMPLEMENT lexicon (build, create, implement) and from
  the QA lexicon (QA literal abbreviation).

---

## Pair B28B1-WDA-edge-003 — "monitor" is gap vocabulary

```
source_id: xiigen-b28-edge-003
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: knowledge_edge
```

A:
  user_request: "Monitor the production deployment and alert on error rate spikes."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  PhaseId: null
  SubPhaseId: null
  SourceStatus: "knowledge_edge"
  Confidence: 0.70

  CapabilityGapSignal:
    gap_id: "gap-devops-monitoring-001"
    gap_vocabulary: ["monitor", "production deployment", "alert", "error rate spikes"]
    DomainId: "software-development"
    SCHEDULE_CAPABILITY_LEARNING: true

  Predicate evidence:
    "monitor" — not in any predicate lexicon
    "alert" — not in any predicate lexicon
    "error rate" — not in any predicate
    "spikes" — not in any predicate
    Result: zero predicates → knowledge_edge

  DevOps observability vocabulary (monitor, alert, metrics, dashboards, SLO/SLA, error rate)
  is gap across all predicate lexicons.
