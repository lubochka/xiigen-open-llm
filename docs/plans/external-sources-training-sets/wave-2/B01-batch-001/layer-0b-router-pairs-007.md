# Layer 0-B — Router Pairs (007)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-007-001 — [UI, ARCHITECTURE] → 1.1 + 3.1: wireframe and design routing

```
source_id: xiigen-b01-0b-007-001
source_title: XIIGen B01 router-007 — [UI, ARCHITECTURE] routes to 1.1 and 3.1
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: documentation
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-007-001]
  DocumentId: "B01-0a-007-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-007-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Mobile checkout wireframe and prototype with interactive components"
  CompiledText: "Design the wireframes and prototype screens for the mobile checkout experience with interactive components."
  CompiledPredicateIds: ["UI", "ARCHITECTURE"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-007-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.44
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-007-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.31
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  UI (wireframe, prototype, screen, interaction, component) → 1.1.
  ARCHITECTURE (design + structural noun) → 3.1 (System Architecture).
  TransitionType: documentation — ARCHITECTURE is present but IMPLEMENT is absent.
  When ARCHITECTURE fires without IMPLEMENT, the TransitionType is documentation
  (the architect produces a design artifact, not implementation code).
  Two routing decisions; no IMPLEMENT routing to 4.1.

---

## B01-0B-007-002 — ARCHITECTURE without IMPLEMENT → documentation TransitionType

```
source_id: xiigen-b01-0b-007-002
source_title: XIIGen B01 router-007 — ARCHITECTURE alone: documentation TransitionType
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: documentation
phase: 3
sub_phase: 3.1
task_type: system-architecture
coverage_status: covered
pair_kind: router
```

A:
  DocumentId: "B01-0b-007-002-prompt"
  CompiledText: "Design the system architecture for the SaaS onboarding module including component communication patterns."
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0b-007-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.29
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  ARCHITECTURE alone routes to 3.1. No UI terms present; 1.1 is NOT routed.
  "component communication patterns" — "component" is in UI lexicon, so if
  present in original text, UI would fire. Here the CompiledPredicateIds from
  the compiler show only ["ARCHITECTURE"]; the router accepts the compiler output
  without re-evaluating the raw text. The router trusts the compiler.
  TransitionType: documentation (ARCHITECTURE without IMPLEMENT).

---

## B01-0B-007-003 — "design" presence and ARCHITECTURE firing rule

```
source_id: xiigen-b01-0b-007-003
source_title: XIIGen B01 router-007 — "design" + structural noun = ARCHITECTURE
source_license_state: first_party
pair_kind: router
coverage_status: covered
```

Rule annotation (supplements pairs 001-002):

  ARCHITECTURE fires when:
    Case 1: "design" + "architecture" both appear in the request
    Case 2: "design" + a structural noun (wireframe, system, flow, pattern, schema,
      module, infrastructure, pipeline, diagram) appears in the request

  ARCHITECTURE does NOT fire when:
    "design" appears with only UI nouns and no structural noun
    Example: "design the button color" → UI fires on "button" and "color";
      ARCHITECTURE does NOT fire because "color" is not a structural noun.
    Example: "design the form layout" → UI fires on "form" and "layout";
      ARCHITECTURE may or may not fire depending on whether "layout" is treated
      as structural. Conservative interpretation: "layout" alone is UI; pair
      with "architecture" or "system" for ARCHITECTURE to fire.

  Summary: The safest ARCHITECTURE trigger is "design" + "architecture" or
  "design" + "system" together. Single-word "design" without a structural noun
  partner is ambiguous and should lean toward not firing ARCHITECTURE unless
  the structural noun is clearly present.
