# Layer 1 — Transition Orchestrator Pairs (007)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 007 covers DOCUMENT routing to multiple sub-phases (2.1 + 2.4): orchestration handles both.

---

## B06-T1-007-001 — DOCUMENT → 2.1 + 2.4 orchestration

```
source_id: xiigen-b06-t1-007-001
source_title: XIIGen B06 T1 — DOCUMENT 2.1 and 2.4 orchestration
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: documentation
phase: 2
sub_phase: 2.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B06-t1-007-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Describe the checkout flow requirements and document the stakeholder expectations for each user type."
  CompiledPredicateIds: ["DOCUMENT"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], SubPhaseSignals: ["requirements"]
    - PredictedSubPhaseId: "2.4", EvidencePredicates: ["DOCUMENT"], SubPhaseSignals: ["stakeholder"]

B:
  Transitions:
    - TransitionId: "transition-B06-t1-007-001a-2.1"
      TransitionType: "documentation"
      PhaseId: "2"
      SubPhaseId: "2.1"
      DomainId: "software-development"
      PredicateIds: ["DOCUMENT"]
      Metadata:
        sub_phase_signal_basis: "requirements, describe"
        primary_intent: "document"
        domain_signal: "business_requirements"

    - TransitionId: "transition-B06-t1-007-001b-2.4"
      TransitionType: "documentation"
      PhaseId: "2"
      SubPhaseId: "2.4"
      DomainId: "software-development"
      PredicateIds: ["DOCUMENT"]
      Metadata:
        sub_phase_signal_basis: "stakeholder"
        primary_intent: "document"
        domain_signal: "stakeholder_requirements"

  TransitionCount: 2
  GlobalTransitionType: "documentation"

Rationale:
  One predicate (DOCUMENT), two sub-phase routing decisions (2.1, 2.4),
  two transitions. No IMPLEMENT or UI → documentation TransitionType.
  Both transitions carry the same predicate (DOCUMENT) but different sub-phase signals.

---

## B06-T1-007-002 — 2.4 transition: StakeholderRequirementsFlow24

```
source_id: xiigen-b06-t1-007-002
source_title: XIIGen B06 T1 — 2.4 transition StakeholderRequirementsFlow24
source_license_state: first_party
pair_kind: transition_orchestrator
```

Teaching note — Sub-phase 2.4 transition context:

  When the TransitionOrchestratorUnit produces a 2.4 transition:
    - The flow factory instantiates StakeholderRequirementsFlow24
    - The nine-node chain for 2.4 focuses on stakeholder and external
      requirements documentation
    - The 2.4 flow runs independently from the 2.1 flow
    - Both flows receive the same UserRequest in their A input

  2.4 flow scope (for B06 context):
    - Stakeholder requirements from external partners
    - Compliance and regulatory requirements
    - SLA (Service-Level Agreement) requirements from enterprise customers
    - Partner API (Application Programming Interface) requirements
    - Vendor constraints

  B06 does not deeply cover the 2.4 nine-node execution. 2.4 execution
  is out-of-scope for B06 but the transition and factory selection for 2.4
  are covered in B06 training pairs.
