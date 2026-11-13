# Layer 0-B — Router Pairs (010)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: router

---

## Method note

File 010 covers [DOCUMENT] → 2.1 for feature-brief routing.

---

## B06-0B-010-001 — [DOCUMENT] → 2.1 for feature-brief

```
source_id: xiigen-b06-0b-010-001
source_title: XIIGen B06 router — DOCUMENT to 2.1 feature-brief
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: documentation
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B06-0a-010-001]
  DocumentId: "B06-0a-010-001-prompt"
  CompiledText: "Author the feature brief for the one-click checkout capability and define the success criteria."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B06-0a-010-001-prompt"
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
  "feature brief"→feature-brief, "capability", "define", "criteria" → 2.1 signals.
  No competing sub-phase signals.
  DecisionCount: 1.

---

## B06-0B-010-002 — feature-brief routes to 2.1 not a special sub-phase

```
source_id: xiigen-b06-0b-010-002
source_title: XIIGen B06 router — feature-brief routes to 2.1
source_license_state: first_party
pair_kind: router
```

Teaching note — feature-brief routing:

  "feature-brief" fires DOCUMENT (Rule 14). DOCUMENT routes to 2.1.
  There is no separate "feature-brief sub-phase" — it routes to 2.1
  (Business Requirements and Use Cases) because a feature brief is a
  type of requirements artifact.

  Feature briefs authored in sub-phase 2.1 context:
    - Feature brief = structured requirements document for a specific feature
    - Includes: capability description, use cases, acceptance criteria, scope
    - Lives in the 2.1 requirements artifact set

  No special routing logic for feature-brief beyond DOCUMENT → 2.1.

---

## B06-0B-010-003 — Complete B06 routing decision table

```
source_id: xiigen-b06-0b-010-003
source_title: XIIGen B06 router — complete routing decision table
source_license_state: first_party
pair_kind: router
```

B06 Routing Decision Table:

  Compiled predicates             | Routing decisions produced        | DecisionCount
  --------------------------------|-----------------------------------|---------------
  ["DOCUMENT"]                    | 2.1                               | 1
  ["DOCUMENT"] + stakeholder sig  | 2.1 + 2.4                         | 2
  ["DOCUMENT", "IMPLEMENT"]       | 2.1 + 4.1                         | 2
  ["DOCUMENT", "UI"]              | 2.1 + 1.1                         | 2
  ["ARCHITECTURE", "DOCUMENT"]    | 3.1 + 2.1                         | 2
  ["DOCUMENT", "IMPLEMENT", "UI"] | 2.1 + 4.1 + 1.1                   | 3
  ["ARCH", "DOCUMENT", "IMPL"]    | 3.1 + 2.1 + 4.1                   | 3
  ["ARCH", "DOC", "IMPL", "UI"]   | 3.1 + 2.1 + 4.1 + 1.1            | 4
  []                              | knowledge-edge (no routing)       | 0

  Note: "stakeholder sig" means the compiled text contains "stakeholder" or
  "external partner" or "compliance" — sub-phase signals for 2.4.
