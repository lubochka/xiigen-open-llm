# Layer 0-B — Router Pairs (009)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-009-001 — [UI, DOCUMENT] → 1.1 + 2.1: design system routing

```
source_id: xiigen-b01-0b-009-001
source_title: XIIGen B01 router-009 — [UI, DOCUMENT] design system routes to 1.1 and 2.1
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-009-001]
  DocumentId: "B01-0a-009-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-009-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Onboarding UI design system documentation with typography, color, and spacing"
  CompiledText: "Create the design system documentation for the onboarding UI — define the typography, color tokens, and spacing scale."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-009-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.42
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-009-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.35
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-009-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.29
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 3

Routing rationale:
  UI (design-system, ui, typography, color, spacing) → 1.1.
  DOCUMENT (documentation, define) → 2.1.
  IMPLEMENT (create) → 4.1.
  The design system documentation is simultaneously a UX definition artifact
  (1.1 — screen-level visual system), a requirements document (2.1 — tokens
  specified as requirements), and an implementation task (4.1 — token system
  must be built). Three routing decisions are correct.

---

## B01-0B-009-002 — "theme" fires UI → 1.1 routing

```
source_id: xiigen-b01-0b-009-002
source_title: XIIGen B01 router-009 — "theme" routes to 1.1 via UI predicate
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-009-002]
  DocumentId: "B01-0a-009-002-prompt"
  CompiledText: "Define the dark and light theme tokens for the onboarding screens and update the color palette."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-009-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.39
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-009-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.32
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  UI (theme, screen, color) → 1.1. DOCUMENT (define) → 2.1. Two decisions.
  "theme" fires UI and routes to 1.1. It is not a gap vocabulary term in B01.

---

## B01-0B-009-003 — design system full token set → 1.1 + 2.1

```
source_id: xiigen-b01-0b-009-003
source_title: XIIGen B01 router-009 — design system full token inventory routing
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-009-003]
  DocumentId: "B01-0a-009-003-prompt"
  CompiledText: "Document the complete design system component library: typography scale, color system, spacing tokens, theme variants, and layout grid."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-009-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.44
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-009-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  UI fires on seven terms (design-system, component, typography, color, spacing,
  theme, layout). DOCUMENT fires on one term (document). Despite the heavy UI
  signal, DOCUMENT still independently routes to 2.1. The model must not suppress
  DOCUMENT even when UI evidence is dominant.
