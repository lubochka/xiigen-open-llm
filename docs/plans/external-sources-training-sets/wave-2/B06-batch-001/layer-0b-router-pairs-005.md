# Layer 0-B — Router Pairs (005)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: router

---

## Method note

File 005 covers [DOCUMENT] → 2.1 + 2.4 dual routing from stakeholder signals.
One predicate, two routing decisions. DecisionCount > compiled predicate count.

---

## B06-0B-005-001 — [DOCUMENT] → 2.1 + 2.4 stakeholder dual routing

```
source_id: xiigen-b06-0b-005-001
source_title: XIIGen B06 router — DOCUMENT to 2.1 and 2.4 stakeholder dual
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: documentation
phase: 2
sub_phase: 2.1
task_type: requirements-and-business-rules
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B06-0a-005-001]
  DocumentId: "B06-0a-005-001-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/005-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout flow requirements and stakeholder expectations"
  CompiledText: "Describe the checkout flow requirements and document the stakeholder expectations for each user type."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B06-0a-005-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.55
      EvidencePredicates: ["DOCUMENT"]
      SubPhaseSignals: ["requirements", "describe"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B06-0a-005-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.4"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.55
      EvidencePredicates: ["DOCUMENT"]
      SubPhaseSignals: ["stakeholder"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  One compiled predicate (DOCUMENT) produces two routing decisions because
  both 2.1 and 2.4 sub-phase signals are present in the compiled text.
  2.1 signal: "requirements", "describe" → Business Requirements
  2.4 signal: "stakeholder" → Stakeholder and External Requirements
  Both sub-phases are legitimate targets for this DOCUMENT request.
  NearestWrongSubPhaseScore 0.55 is elevated because the two routing decisions
  are themselves "nearest competitors" to each other.
  The 2.4 decision has slightly lower confidence (0.88) because "stakeholder"
  is the sole 2.4 signal while 2.1 has two signals.

---

## B06-0B-005-002 — v6-03 corruption: [DOCUMENT] → 2.1 only (wrong)

```
source_id: xiigen-b06-0b-005-002
source_title: XIIGen B06 router — v6-03 corruption single routing when dual expected
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A: [Same as B06-0B-005-001]
  CompiledText: "Describe the checkout flow requirements and document the stakeholder expectations for each user type."
  CompiledPredicateIds: ["DOCUMENT"]

B-corrupt (v6-03):
  RoutingDecisions:
    - DocumentId: "B06-0a-005-001-prompt"
      PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false

  DecisionCount: 1

Corruption type: Corruption-v6-03 — routing to single sub-phase.
Why harmful: Sub-phase 2.4 (Stakeholder and External Requirements) is never
  selected. Stakeholder expectations for each user type are not documented as
  external requirements. Business analyst and compliance team requirements
  derived from stakeholder interviews are not captured in the 2.4 artifact.
Fix: Router must inspect all sub-phase signals within DOCUMENT evidence
  and produce a routing decision for each matching sub-phase.

---

## B06-0B-005-003 — 2.1 vs 2.4 routing decision structure comparison

```
source_id: xiigen-b06-0b-005-003
source_title: XIIGen B06 router — 2.1 vs 2.4 routing decision structure
source_license_state: first_party
pair_kind: router
```

Teaching note — 2.1 vs 2.4 routing decision comparison:

  2.1 decision:
    PredictedPhaseId: "2"
    PredictedSubPhaseId: "2.1"
    EvidencePredicates: ["DOCUMENT"]
    SubPhaseSignals: ["requirements", "use-case", "behavior", "criteria", "story"]

  2.4 decision:
    PredictedPhaseId: "2"
    PredictedSubPhaseId: "2.4"
    EvidencePredicates: ["DOCUMENT"]
    SubPhaseSignals: ["stakeholder", "external", "partner", "compliance", "regulatory"]

  Both decisions share the same predicate (DOCUMENT) but are differentiated by
  SubPhaseSignals. Both are valid, non-competing routing decisions from one predicate.
  They will each instantiate different flow factories:
    2.1 → RequirementsAuthoringFlow21
    2.4 → StakeholderRequirementsFlow24
