# Layer 0-B — Router Pairs (021)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: router

---

## Method note (021)

Knowledge-edge routing cases: "navigation" and "menu" terms that do NOT fire UI.
When CompiledPredicateIds is empty, the router must return KnowledgeEdge: true,
PredictedSubPhaseId: null, and FailureClassification: "no_predicate_match".

---

## B01-0B-021-001 — knowledge_edge: "navigation menu" — no predicate match

```
source_id: xiigen-b01-0b-021-001
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-kedge-nav-001-prompt"
  CompiledText: "Redesign the main navigation menu to reduce cognitive load and improve discoverability."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no predicate matched"
  KnownFailureMode: "no_predicate_match"

B:
  RoutingDecisions:
    - DocumentId: "B01-kedge-nav-001-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match"

  DecisionCount: 1

Knowledge-edge rationale:
  "navigation" — NOT in UI lexicon. "menu" — NOT in UI lexicon.
  "cognitive load", "discoverability" — not in any predicate lexicon.
  "redesign" — NOT in IMPLEMENT lexicon.
  Zero predicates fire → KnowledgeEdge: true.

---

## B01-0B-021-002 — knowledge_edge: "sidebar menu" — no predicate match

```
source_id: xiigen-b01-0b-021-002
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-kedge-menu-001-prompt"
  CompiledText: "Update the sidebar menu to show only items relevant to the current user's role."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no predicate matched"
  KnownFailureMode: "no_predicate_match"

B:
  RoutingDecisions:
    - DocumentId: "B01-kedge-menu-001-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match"

  DecisionCount: 1

Knowledge-edge rationale:
  "sidebar" — NOT in any predicate lexicon.
  "menu" — NOT in UI lexicon.
  "update" — NOT in IMPLEMENT lexicon (requires: implement, build, create, write, develop, etc.)
  Zero predicates → KnowledgeEdge: true.

---

## B01-0B-021-003 — knowledge_edge: "nav bar" — no predicate match

```
source_id: xiigen-b01-0b-021-003
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-kedge-navbar-001-prompt"
  CompiledText: "Rethink the nav bar structure to better support multi-tenant accounts."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no predicate matched"
  KnownFailureMode: "no_predicate_match"

B:
  RoutingDecisions:
    - DocumentId: "B01-kedge-navbar-001-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match"

  DecisionCount: 1

Knowledge-edge rationale:
  "nav bar" — NOT in UI lexicon (only "navigation" disqualified, "nav bar" also not present).
  "rethink" — NOT in any predicate lexicon.
  "structure" — NOT in ARCHITECTURE lexicon (requires: architecture, component[system], topology, service, module, system, pattern).
  Zero predicates → KnowledgeEdge: true.

---

## B01-0B-021-004 — contrast: "button" in sentence with "navigation" — UI fires

```
source_id: xiigen-b01-0b-021-004
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-router-nav-button-contrast-prompt"
  CompiledText: "Map the button interactions in the navigation header, including hover, active, and focus states."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-router-nav-button-contrast-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Contrast rationale:
  "navigation" does NOT fire UI.
  "button" DOES fire UI — it is in the UI lexicon.
  "interaction" also fires UI.
  Even though "navigation" is present and is a gap term, the presence of "button" and
  "interaction" ensures the UI predicate fires. Result: 1.1 routing, NOT knowledge-edge.
