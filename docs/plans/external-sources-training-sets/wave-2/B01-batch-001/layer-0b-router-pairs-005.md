# Layer 0-B — Router Pairs (005)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-005-001 — [DOCUMENT] → 2.1: functional behavior only, no UI

```
source_id: xiigen-b01-0b-005-001
source_title: XIIGen B01 router-005 — [DOCUMENT] routes to 2.1 only; no 1.1 routing
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: documentation
phase: 2
sub_phase: 2.1
task_type: business-requirements
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-005-001]
  DocumentId: "B01-0a-005-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-005-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Onboarding wizard functional behavior and user journey requirements"
  CompiledText: "Describe the functional behavior of the onboarding wizard and document the user journey requirements."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-005-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  DOCUMENT is the sole predicate. DOCUMENT routes to sub-phase 2.1 (Business
  Requirements and Use Cases). There is NO routing to 1.1 because no UI predicate
  fired. "Onboarding wizard" contains no UI lexicon terms. The router must not
  infer a 1.1 routing from the UX subject matter — only explicit predicate matches
  determine routing. DecisionCount: 1.

---

## B01-0B-005-002 — [DOCUMENT] → 2.1: password reset requirements only

```
source_id: xiigen-b01-0b-005-002
source_title: XIIGen B01 router-005 — [DOCUMENT] routes to 2.1 for requirements document
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: documentation
phase: 2
sub_phase: 2.1
task_type: business-requirements
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-005-003]
  DocumentId: "B01-0a-005-003-prompt"
  CompiledText: "Define the business requirements for the password reset feature and describe each step's acceptance criteria."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-005-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.35
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-005-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.28
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  DOCUMENT (define, requirement, describe, criteria) → 2.1. IMPLEMENT (feature) → 4.1.
  No UI terms are present in this request; sub-phase 1.1 is NOT routed.
  The router must not add a 1.1 decision because no UI predicate exists.

---

## B01-0B-005-003 — [DOCUMENT] alone: key rule — no UI routing inferred from UX context

```
source_id: xiigen-b01-0b-005-003
source_title: XIIGen B01 router-005 — [DOCUMENT] routing rule: no context-based inference
source_license_state: first_party
pair_kind: router
coverage_status: covered
```

Rule annotation (supplements pairs 001-002):

  The router MUST NOT infer a routing decision from the subject matter of the
  request. It routes solely based on the CompiledPredicateIds provided by the
  compiler. Subject-matter inference is a Corruption-v6-01 variant.

  Examples of incorrect inference to avoid:
    Request about "onboarding wizard" → router MUST NOT add 1.1 routing
    Request about "password reset flow" → router MUST NOT add 1.1 routing
    Request about "checkout experience" → router MUST NOT add 1.1 routing

  The router asks only: "Which predicates fired?" and routes each one to its
  registered sub-phase. If no predicate fired, KnowledgeEdge: true is the
  correct result.

  Sub-phase ownership in B01 context:
    1.1 — Screen Flow and UX Definition        → owned by: UI predicate
    2.1 — Business Requirements and Use Cases  → owned by: DOCUMENT predicate
    3.1 — System Architecture                  → owned by: ARCHITECTURE predicate
    4.1 — Implementation                       → owned by: IMPLEMENT predicate
    2.4 — Stakeholder Requirements             → owned by: DOCUMENT predicate
      (2.4 is an alternative DOCUMENT routing when stakeholder context is present)
