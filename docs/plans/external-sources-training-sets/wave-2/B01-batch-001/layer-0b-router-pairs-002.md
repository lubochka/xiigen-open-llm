# Layer 0-B — Router Pairs (002)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-002-001 — [UI] → 1.1: admin panel pages routing

```
source_id: xiigen-b01-0b-002-001
source_title: XIIGen B01 router-002 — [UI] routes to 1.1 for admin panel inventory
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-002-001]
  DocumentId: "B01-0a-002-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-002-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Admin interface page inventory"
  CompiledText: "Create an inventory of all pages in the admin interface and list each page's purpose and navigation."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-002-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  UI predicate (page, interface) routes to sub-phase 1.1 (Screen Flow and UX
  Definition). "Inventory" is a strong signal for the screen inventory sub-phase.
  No competing predicates; single decision. "navigation" did not fire any predicate
  and therefore contributes no routing signal. NearestWrongSubPhaseScore 0.38
  reflects sub-phases 1.2-1.5 as plausible UI candidates, but 1.1 is the
  designated owner of page and screen inventory work.

---

## B01-0B-002-002 — [UI] → 1.1: admin panel with "document" in title but UI fires

```
source_id: xiigen-b01-0b-002-002
source_title: XIIGen B01 router-002 — [UI] single-branch for admin panel full inventory
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-002-002]
  DocumentId: "B01-0a-002-002-prompt"
  CompiledText: "Map every page in the admin panel and document each page's interaction model, entry points, and screen layout."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-002-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-002-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.35
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  Two predicates → two independent routing decisions. UI → 1.1 for the screen
  mapping work; DOCUMENT → 2.1 for the interaction model documentation artifact.
  Each RoutingDecision carries only its own EvidencePredicates. The router must
  never merge ["UI", "DOCUMENT"] into a single decision.

---

## B01-0B-002-003 — [UI] → 1.1: navigation boundary confirmed at router stage

```
source_id: xiigen-b01-0b-002-003
source_title: XIIGen B01 router-002 — [UI] routing when "navigation" is present but inert
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-002-003]
  DocumentId: "B01-0a-002-003-prompt"
  CompiledText: "List the navigation structure of the admin interface so we can verify every page is reachable."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-002-003-prompt"
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
  The compiler correctly omitted "navigation" from the predicate evidence.
  The router receives ["UI"] based on "interface" and "page" only. This single
  predicate routes to 1.1 with no ambiguity. The presence of "navigation" in
  the raw text does not affect routing because no compiler predicate was fired
  by that term.
