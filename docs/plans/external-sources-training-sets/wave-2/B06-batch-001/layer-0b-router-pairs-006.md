# Layer 0-B — Router Pairs (006)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: router

---

## Method note

File 006 covers [DOCUMENT, IMPLEMENT] → 2.1 + 4.1 for feature narrative with "write".

---

## B06-0B-006-001 — [DOCUMENT, IMPLEMENT] → 2.1 + 4.1 feature narrative

```
source_id: xiigen-b06-0b-006-001
source_title: XIIGen B06 router — DOCUMENT+IMPLEMENT to 2.1+4.1 feature narrative
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B06-0a-006-001]
  DocumentId: "B06-0a-006-001-prompt"
  CompiledText: "Write the feature narrative for the subscription management epic and define the capability requirements."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B06-0a-006-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B06-0a-006-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  DOCUMENT ("narrative", "epic", "define", "capability", "requirements") → 2.1.
  IMPLEMENT ("write") → 4.1.
  DecisionCount: 2.

---

## B06-0B-006-002 — RequirementsAuthoringFlow21 is the 2.1 flow

```
source_id: xiigen-b06-0b-006-002
source_title: XIIGen B06 router — RequirementsAuthoringFlow21 for 2.1
source_license_state: first_party
pair_kind: router
```

Teaching note — Layer 2 flow for sub-phase 2.1:

  Sub-phase 2.1 (Business Requirements and Use Cases) is served by:
    FlowId: RequirementsAuthoringFlow21

  This is the nine-node flow (B06 Core Rule 18). Its nodes are:
    N01: DomainSplitterUnit
    N02: DomainTranslatorUnit
    N03: SchemePlannerUnit
    N04: DomainProcessorUnit
    N05: ApplierUnit
    N06: ResultTranslatorUnit
    N07: ArbiterSelectorUnit
    N08: ArbiterUnit
    N09: FormatterUnit

  When the router produces a 2.1 routing decision, the flow factory
  instantiates RequirementsAuthoringFlow21 for that decision.
  When the router also produces a 4.1 routing decision, the flow factory
  instantiates ImplementationFlow41 for that decision.
  Both flows execute independently.

---

## B06-0B-006-003 — 4.1 routing: ImplementationFlow41

```
source_id: xiigen-b06-0b-006-003
source_title: XIIGen B06 router — 4.1 routing ImplementationFlow41
source_license_state: first_party
pair_kind: router
```

Teaching note — Sub-phase 4.1 (Backend Services and Business Logic):

  When IMPLEMENT fires and routes to 4.1, the flow factory instantiates:
    FlowId: ImplementationFlow41

  This flow is outside the scope of B06 training (B06 covers sub-phase 2.1
  primary and 4.1 secondary). The 4.1 routing decision is produced and the
  flow factory selection records it, but the nine-node pairs for 4.1 execution
  are covered in B06's companion batch (B04-batch-001 or similar).

  In B06 training pairs, 4.1 routing decisions are always present in router
  outputs when IMPLEMENT fires, but the 4.1 flow execution is out-of-scope.
  The model must still produce the 4.1 routing decision — not producing it
  is the v6-03 corruption (result collapse).
