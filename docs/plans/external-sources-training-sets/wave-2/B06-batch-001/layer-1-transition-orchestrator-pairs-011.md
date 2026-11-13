# Layer 1 — Transition Orchestrator Pairs (011)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 011 covers Phase 2 flow routing patterns: how each Phase 2 sub-phase is reached.

---

## B06-T1-011-001 — Phase 2 sub-phase routing patterns

```
source_id: xiigen-b06-t1-011-001
source_title: XIIGen B06 T1 — Phase 2 sub-phase routing patterns
source_license_state: first_party
pair_kind: transition_orchestrator
```

Phase 2 sub-phase routing patterns in TransitionOrchestratorUnit:

  Sub-phase 2.1 (Business Requirements and Use Cases):
    Owner predicate: DOCUMENT
    Primary routing signals: requirements, use-case, acceptance criteria,
      business rules, functional, behavior, story, narrative, feature-brief,
      capability, epics
    Flow: RequirementsAuthoringFlow21
    Example request: "Define the use cases and acceptance criteria"

  Sub-phase 2.2 (Technical Requirements):
    Owner predicate: DOCUMENT
    Primary routing signals: technical requirements, data model, API contract,
      schema, interface spec, data structure, entity definition
    Flow: TechnicalRequirementsFlow22
    Example request: "Specify the data model for the order entity"
    Note: B06 does not cover 2.2 deeply (B07 batch)

  Sub-phase 2.3 (Non-Functional Requirements):
    Owner predicate: DOCUMENT
    Primary routing signals: performance, scalability, availability,
      non-functional, SLA, reliability, load, latency
    Flow: NonFunctionalRequirementsFlow23
    Example request: "Define the performance requirements and SLA"
    Note: B06 does not cover 2.3 deeply (B08 batch)

  Sub-phase 2.4 (Stakeholder and External Requirements):
    Owner predicate: DOCUMENT
    Primary routing signals: stakeholder, external, partner, compliance,
      regulatory, vendor, legal, audit
    Flow: StakeholderRequirementsFlow24
    Example request: "Document stakeholder expectations for external partners"
    Note: B06 touches 2.4 via held-out HO-003 and router-pairs-005

---

## B06-T1-011-002 — Cross-phase routing: Phase 2 + Phase 3 + Phase 4 + Phase 1

```
source_id: xiigen-b06-t1-011-002
source_title: XIIGen B06 T1 — cross-phase routing patterns
source_license_state: first_party
pair_kind: transition_orchestrator
```

Cross-phase routing patterns in B06:

  Single phase (common in B06):
    DOCUMENT → Phase 2 (2.1 or 2.4)
    ARCHITECTURE → Phase 3 (3.1)
    IMPLEMENT → Phase 4 (4.1)
    UI → Phase 1 (1.1)

  Dual phase (common in B06 dual-predicate cases):
    DOCUMENT + IMPLEMENT → Phase 2 + Phase 4
    DOCUMENT + UI → Phase 2 + Phase 1
    DOCUMENT + ARCHITECTURE → Phase 2 + Phase 3

  Triple phase:
    DOCUMENT + IMPLEMENT + UI → Phase 2 + Phase 4 + Phase 1
    ARCHITECTURE + DOCUMENT + IMPLEMENT → Phase 3 + Phase 2 + Phase 4

  Quad phase (full coverage):
    ARCHITECTURE + DOCUMENT + IMPLEMENT + UI → Phase 3 + Phase 2 + Phase 4 + Phase 1

  Rule: Phase ordering in transitions is not imposed by the orchestrator.
  The factory queues flows with 2.1 first (requirements before implementation).

---

## B06-T1-011-003 — Transition chain integrity

```
source_id: xiigen-b06-t1-011-003
source_title: XIIGen B06 T1 — transition chain integrity
source_license_state: first_party
pair_kind: transition_orchestrator
```

Teaching note — transition chain integrity:

  The TransitionOrchestratorUnit must maintain chain integrity:
    CompilerOutput.DocumentId → referenced in RouterOutput.DocumentId
    RouterOutput.RoutingDecisions → referenced in OrchestratorOutput.EvidenceIds
    OrchestratorOutput.TransitionId → referenced in FlowFactory input

  Chain integrity check:
    - Every TransitionId must reference a valid compiler document (via EvidenceIds)
    - Every routing decision must reference a valid sub-phase target
    - No transition may be produced without a corresponding routing decision

  Chain integrity failure symptoms:
    - TransitionId with no matching compiler evidence → chain break
    - Routing decision without corresponding transition → orphaned decision
    - Transition with wrong SubPhaseId → routing–transition mismatch

  The PairParserUnit (Layer 3) validates chain integrity across all pairs
  in a batch and reports ChainIntegrityCheck: "passed" or "failed".
