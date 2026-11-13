# Layer 0-B — Router Pairs (011)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-011-001 — v6 corruption: UI wrongly routes to 2.1 instead of 1.1

```
source_id: xiigen-b01-0b-011-v603
source_title: XIIGen B01 router-011 — Corruption-v6-03: UI routed to 2.1 instead of 1.1
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
corruption_type: Corruption-v6-03-ui-wrong-sub-phase
```

A:
  DocumentId: "B01-0b-011-v603-prompt"
  CompiledText: "Map all screen flows for the SaaS onboarding UI."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

B-correct:
  RoutingDecisions:
    - DocumentId: "B01-0b-011-v603-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

B-corrupt (v6-03):
  RoutingDecisions:
    - DocumentId: "B01-0b-011-v603-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

Corruption explanation:
  v6-03 (wrong sub-phase for UI): the corrupted router sends UI to 2.1 (Business
  Requirements and Use Cases), which is owned by DOCUMENT. The UI predicate always
  routes to 1.1 in Phase 1. Sub-phase 2.1 must only appear in routing decisions
  when DOCUMENT is in EvidencePredicates. Routing UI → 2.1 is always wrong.

Impact: ScreenInventoryFlow11 is never invoked. BusinessRequirementsFlow21 runs
instead, producing a requirements artifact for a request that asked for a screen map.

---

## B01-0B-011-002 — v6 corruption: UI predicate dropped at routing stage

```
source_id: xiigen-b01-0b-011-v601-router
source_title: XIIGen B01 router-011 — Corruption-v6-01: UI predicate dropped from routing
source_license_state: first_party
pair_kind: router
corruption_type: Corruption-v6-01-ui-dropped-at-routing
```

A:
  DocumentId: "B01-0b-011-v601-prompt"
  CompiledText: "Audit the accessibility of all onboarding screens and document the WCAG compliance requirements."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"]
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"]
  DecisionCount: 2

B-corrupt (v6-01 at router stage):
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"]
  DecisionCount: 1

Corruption explanation:
  The compiler correctly produced ["UI", "DOCUMENT"]. The corrupted router then
  drops the UI predicate and only routes DOCUMENT → 2.1. This is a v6-01 variant
  occurring at the routing stage rather than the compilation stage. The 1.1 routing
  for screen accessibility work is suppressed. The ScreenInventoryFlow11 never runs.

---

## B01-0B-011-003 — v6 corruption: DOCUMENT suppressed at routing stage

```
source_id: xiigen-b01-0b-011-v602-router
source_title: XIIGen B01 router-011 — Corruption-v6-02: DOCUMENT suppressed in routing
source_license_state: first_party
pair_kind: router
corruption_type: Corruption-v6-02-document-suppressed-at-routing
```

A:
  DocumentId: "B01-0b-011-v602-prompt"
  CompiledText: "Document the complete user interface flows for the checkout process and produce a page specification."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"]
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"]
  DecisionCount: 2

B-corrupt (v6-02 at router stage):
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI", "DOCUMENT"]
  DecisionCount: 1

Corruption explanation:
  The router collapses both predicates into a single 1.1 decision and assigns
  both EvidencePredicates to it. This is Corruption-v6-02: DOCUMENT is suppressed
  as a routing signal. The page specification artifact (2.1) is never produced.
  EvidencePredicates in a RoutingDecision must contain only the predicates that
  drive that specific branch — never a combined set.
