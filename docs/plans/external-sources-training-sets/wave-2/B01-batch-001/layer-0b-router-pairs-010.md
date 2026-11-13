# Layer 0-B — Router Pairs (010)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-010-001 — [UI, ARCHITECTURE, IMPLEMENT, DOCUMENT] → four routing decisions

```
source_id: xiigen-b01-0b-010-001
source_title: XIIGen B01 router-010 — four-predicate four routing decisions
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-010-001]
  DocumentId: "B01-0a-010-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-010-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Onboarding screen flow: architecture design, form component implementation, requirements documentation, and screen interaction mapping"
  CompiledText: "Design the architecture for the onboarding screen flow, implement the form components, document the user requirements, and map all screen interactions."
  CompiledPredicateIds: ["UI", "ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-010-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.43
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-010-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.30
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-010-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.32
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-010-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.35
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 4

Routing rationale:
  UI (screen, form, component, interaction) → 1.1.
  ARCHITECTURE (design, architecture) → 3.1.
  IMPLEMENT (implement) → 4.1.
  DOCUMENT (document, requirement) → 2.1.
  Four predicates → four independent routing decisions. DecisionCount must be 4.
  TransitionType: feature_addition (IMPLEMENT is present).
  The router must not collapse any two decisions into one.

---

## B01-0B-010-002 — four-predicate routing: second variant

```
source_id: xiigen-b01-0b-010-002
source_title: XIIGen B01 router-010 — four-predicate second variant routing
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-010-002]
  DocumentId: "B01-0a-010-002-prompt"
  CompiledText: "Build the full product listing screen architecture, implement the responsive component library, write the functional specification for each page, and define the interaction model for all UI elements."
  CompiledPredicateIds: ["UI", "ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-010-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.44
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-010-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.29
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-010-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.31
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-010-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.36
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 4

---

## B01-0B-010-003 — complete B01 routing decision table

```
source_id: xiigen-b01-0b-010-003
source_title: XIIGen B01 router-010 — complete routing table for all predicate combinations
source_license_state: first_party
pair_kind: router
coverage_status: covered
```

Complete B01 routing decision table:

| Predicates fired | Routing decisions | Sub-phases | TransitionType |
|---|---|---|---|
| [UI] | 1 | 1.1 | feature_addition |
| [DOCUMENT] | 1 | 2.1 | documentation |
| [IMPLEMENT] | 1 | 4.1 | feature_addition |
| [ARCHITECTURE] | 1 | 3.1 | documentation |
| [UI, DOCUMENT] | 2 | 1.1 + 2.1 | feature_addition |
| [UI, IMPLEMENT] | 2 | 1.1 + 4.1 | feature_addition |
| [UI, ARCHITECTURE] | 2 | 1.1 + 3.1 | documentation |
| [DOCUMENT, IMPLEMENT] | 2 | 2.1 + 4.1 | feature_addition |
| [ARCHITECTURE, IMPLEMENT] | 2 | 3.1 + 4.1 | feature_addition |
| [UI, DOCUMENT, IMPLEMENT] | 3 | 1.1 + 2.1 + 4.1 | feature_addition |
| [UI, ARCHITECTURE, IMPLEMENT] | 3 | 1.1 + 3.1 + 4.1 | feature_addition |
| [UI, ARCHITECTURE, DOCUMENT] | 3 | 1.1 + 3.1 + 2.1 | documentation |
| [UI, ARCHITECTURE, IMPLEMENT, DOCUMENT] | 4 | 1.1 + 3.1 + 4.1 + 2.1 | feature_addition |
| [] | 0 | none (knowledge-edge) | unknown |

TransitionType rule: feature_addition when IMPLEMENT is present;
documentation when IMPLEMENT is absent and at least one of UI, ARCHITECTURE, or
DOCUMENT is present.
