# Layer 0-B — Router Pairs (002)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: router

---

## Method note

File 002 covers [DOCUMENT] → 2.1 routing for acceptance criteria.
Single predicate, single routing decision.

---

## B06-0B-002-001 — [DOCUMENT] → 2.1 for acceptance criteria

```
source_id: xiigen-b06-0b-002-001
source_title: XIIGen B06 router — DOCUMENT to 2.1 acceptance criteria
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: documentation
phase: 2
sub_phase: 2.1
task_type: requirements-and-business-rules
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B06-0a-002-001]
  DocumentId: "B06-0a-002-001-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/002-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Payment processing acceptance criteria and use cases"
  CompiledText: "Define the acceptance criteria and use cases for the payment processing feature, including the business rules for failed payments."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B06-0a-002-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.39
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  "define", "criteria", "use cases", "business" all fire DOCUMENT.
  Strong 2.1 signal: "acceptance criteria" and "use cases" are primary 2.1 signals.
  "business rules" partially matches 2.1 (business rules) over 2.4 (stakeholder).
  NearestWrongSubPhaseScore 0.39 reflects 2.4 proximity due to "business" term.

---

## B06-0B-002-002 — [DOCUMENT] → 2.1 for behavior specification

```
source_id: xiigen-b06-0b-002-002
source_title: XIIGen B06 router — DOCUMENT to 2.1 behavior specification
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: documentation
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B06-0a-008-001]
  DocumentId: "B06-0a-008-001-prompt"
  CompiledText: "Specify the behavior of the order confirmation system and describe the notification use cases."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B06-0a-008-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.32
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  "specify", "behavior", "describe", "use cases" all fire DOCUMENT.
  Primary signals for 2.1: "behavior" and "use cases".
  No stakeholder, technical, or non-functional signals → only 2.1 applies.
  NearestWrongSubPhaseScore 0.32 (low) — no competing sub-phase signal.

---

## B06-0B-002-003 — Single routing decision structure

```
source_id: xiigen-b06-0b-002-003
source_title: XIIGen B06 router — single routing decision structure
source_license_state: first_party
pair_kind: router
```

Teaching note — single routing decision:

  When [DOCUMENT] is the only compiled predicate and only 2.1 signals are
  present (requirements, use-case, behavior, criteria, story), the router
  produces exactly one routing decision:

  RoutingDecisions:
    - PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
  DecisionCount: 1

  DecisionCount == 1 is the normal result for a DOCUMENT-only request
  with no stakeholder or other sub-phase cross-signals.

  DecisionCount > 1 occurs when:
    - Multiple predicates are compiled (DOCUMENT + IMPLEMENT → 2 decisions)
    - One predicate maps to multiple sub-phases (DOCUMENT with stakeholder
      signals → 2.1 + 2.4 = 2 decisions)
