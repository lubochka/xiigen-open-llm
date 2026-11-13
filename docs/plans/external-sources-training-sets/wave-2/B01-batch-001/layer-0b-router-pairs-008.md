# Layer 0-B — Router Pairs (008)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-008-001 — [UI, DOCUMENT] → 1.1 + 2.1: accessibility audit routing

```
source_id: xiigen-b01-0b-008-001
source_title: XIIGen B01 router-008 — [UI, DOCUMENT] for accessibility audit
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-008-001]
  DocumentId: "B01-0a-008-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-008-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Onboarding screen accessibility audit and WCAG compliance requirements"
  CompiledText: "Audit the accessibility of all onboarding screens and document the WCAG compliance requirements."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-008-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-008-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.34
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  UI (accessibility, screen) → 1.1. The accessibility sub-phase (1.4) might seem
  like a candidate, but the Phase 1 router routes all UI-predicate signals to 1.1
  as the primary sub-phase for screen mapping and flow definition. Sub-phase 1.4
  would be a specific routing target only if a separate ACCESSIBILITY predicate
  existed; it does not. All UI signals route to 1.1 in Phase 1.
  DOCUMENT (document, requirement) → 2.1. The WCAG (Web Content Accessibility
  Guidelines — standards for web accessibility) compliance requirements document
  is a business requirements artifact.

---

## B01-0B-008-002 — "accessibility" fires UI: routing confirmed

```
source_id: xiigen-b01-0b-008-002
source_title: XIIGen B01 router-008 — accessibility alone routes to 1.1 via UI predicate
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-008-002]
  DocumentId: "B01-0a-008-002-prompt"
  CompiledText: "Review the accessibility compliance of the new checkout flow and flag any issues."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-008-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  "accessibility" fires UI and routes to 1.1. No DOCUMENT terms present;
  no 2.1 routing. "compliance", "review", "flag", "issues" — none fire any
  predicate. Single routing decision to 1.1.

---

## B01-0B-008-003 — Full accessibility + screen + document routing

```
source_id: xiigen-b01-0b-008-003
source_title: XIIGen B01 router-008 — accessibility + screen + document → two decisions
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-008-003]
  DocumentId: "B01-0a-008-003-prompt"
  CompiledText: "Map all onboarding screens for accessibility issues, define remediation criteria, and describe the expected behavior after fixes."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-008-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-008-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.33
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  UI (screen, accessibility) → 1.1. DOCUMENT (define, criteria, describe, behavior) → 2.1.
  Two independent decisions. The remediation criteria and behavior descriptions
  are requirements artifacts (2.1). The screen mapping for accessibility issues
  is a UX definition task (1.1).
