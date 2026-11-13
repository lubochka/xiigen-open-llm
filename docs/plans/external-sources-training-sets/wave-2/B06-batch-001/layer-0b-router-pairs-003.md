# Layer 0-B — Router Pairs (003)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: router

---

## Method note

File 003 covers [DOCUMENT, IMPLEMENT] → 2.1 + 4.1 dual routing from "write user stories".
Two predicates, two routing decisions, DecisionCount == 2.

---

## B06-0B-003-001 — [DOCUMENT, IMPLEMENT] → 2.1 + 4.1 for user story writing

```
source_id: xiigen-b06-0b-003-001
source_title: XIIGen B06 router — DOCUMENT+IMPLEMENT to 2.1+4.1 user stories
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
task_type: requirements-and-business-rules
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B06-0a-003-001]
  DocumentId: "B06-0a-003-001-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/003-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Order management user stories with acceptance criteria"
  CompiledText: "Write the user stories for the order management feature with acceptance criteria for each scenario."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B06-0a-003-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B06-0a-003-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  DOCUMENT predicate ("story", "criteria") routes to 2.1 (Business Requirements).
  IMPLEMENT predicate ("write") routes to 4.1 (Backend Services and Business Logic).
  Each predicate produces one independent routing decision.
  The 4.1 routing is context-lower-confidence because "write" appears in a
  documentation context, but the compiler has already determined IMPLEMENT fires —
  the router honors that determination without re-evaluating context.
  DecisionCount: 2 is correct.

---

## B06-0B-003-002 — [DOCUMENT, IMPLEMENT] → 2.1 + 4.1 for specification writing

```
source_id: xiigen-b06-0b-003-002
source_title: XIIGen B06 router — DOCUMENT+IMPLEMENT to 2.1+4.1 specification
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B06-0a-003-002]
  DocumentId: "B06-0a-003-002-prompt"
  CompiledText: "Write the specification for the order cancellation use case including the business rules."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B06-0a-003-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B06-0a-003-002-prompt"
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
  DOCUMENT ("specification", "use case", "business") → 2.1.
  IMPLEMENT ("write") → 4.1.
  DecisionCount: 2.

---

## B06-0B-003-003 — Router rule: each predicate routes independently

```
source_id: xiigen-b06-0b-003-003
source_title: XIIGen B06 router — predicate independent routing rule
source_license_state: first_party
pair_kind: router
```

Teaching note — routing independence:

  Each compiled predicate produces exactly one routing decision.
  The router does NOT merge predicates into a single decision.
  The router does NOT suppress the lower-confidence routing.

  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  → Two routing decisions: one for DOCUMENT (2.1), one for IMPLEMENT (4.1)
  → DecisionCount: 2

  CompiledPredicateIds: ["DOCUMENT"]
  → One routing decision: for DOCUMENT (2.1)
  → DecisionCount: 1

  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT", "ARCHITECTURE", "UI"]
  → Four routing decisions: 2.1, 4.1, 3.1, 1.1
  → DecisionCount: 4

  Exception: DOCUMENT with multiple sub-phase signals (2.1 + 2.4) → one
  predicate produces two routing decisions. In this case DecisionCount can be
  greater than the number of compiled predicates. See router pairs file 005
  for the 2.1 + 2.4 dual-routing case.
