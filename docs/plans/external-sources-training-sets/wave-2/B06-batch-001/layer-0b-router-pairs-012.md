# Layer 0-B — Router Pairs (012)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: router

---

## Method note

File 012 is the complete B06 routing decision table summary.

---

## B06-0B-012-001 — Complete B06 routing decision table

```
source_id: xiigen-b06-0b-012-001
source_title: XIIGen B06 router — complete routing decision table
source_license_state: first_party
pair_kind: router
```

B06 Routing Decision Table (complete):

  Predicate set                       | Sub-phase decisions    | DecisionCount | TransitionType
  ------------------------------------|------------------------|---------------|----------------
  ["DOCUMENT"]                        | 2.1                    | 1             | documentation
  ["DOCUMENT"] + stakeholder sig      | 2.1 + 2.4              | 2             | documentation
  ["DOCUMENT", "IMPLEMENT"]           | 2.1 + 4.1              | 2             | feature_addition
  ["DOCUMENT", "UI"]                  | 2.1 + 1.1              | 2             | feature_addition
  ["ARCHITECTURE", "DOCUMENT"]        | 3.1 + 2.1              | 2             | documentation
  ["DOCUMENT", "IMPLEMENT", "UI"]     | 2.1 + 4.1 + 1.1        | 3             | feature_addition
  ["ARCHITECTURE", "DOCUMENT", "IMP"] | 3.1 + 2.1 + 4.1        | 3             | feature_addition
  ["ARCH", "DOC", "IMP", "UI"]        | 3.1 + 2.1 + 4.1 + 1.1  | 4             | feature_addition
  []                                  | none (knowledge-edge)  | 0             | null

  TransitionType rule:
    feature_addition if IMPLEMENT or UI is present
    documentation if only DOCUMENT and/or ARCHITECTURE present

---

## B06-0B-012-002 — EvidencePredicates field in routing decisions

```
source_id: xiigen-b06-0b-012-002
source_title: XIIGen B06 router — EvidencePredicates field
source_license_state: first_party
pair_kind: router
```

Teaching note — EvidencePredicates field:

  Each routing decision carries EvidencePredicates: the predicate that produced it.

  DOCUMENT routing decision → EvidencePredicates: ["DOCUMENT"]
  IMPLEMENT routing decision → EvidencePredicates: ["IMPLEMENT"]
  ARCHITECTURE routing decision → EvidencePredicates: ["ARCHITECTURE"]
  UI routing decision → EvidencePredicates: ["UI"]

  EvidencePredicates must NEVER contain more than one predicate per routing decision.
  One predicate → one sub-phase → one routing decision (with exceptions for DOCUMENT
  mapping to multiple sub-phases, where each decision still cites ["DOCUMENT"] as its
  single evidence predicate).

  Wrong pattern to avoid:
    EvidencePredicates: ["DOCUMENT", "IMPLEMENT"]  ← wrong; this merges two predicates
    into one decision. Each should be a separate decision with its own predicate.

---

## B06-0B-012-003 — KnowledgeEdge field in routing decisions

```
source_id: xiigen-b06-0b-012-003
source_title: XIIGen B06 router — KnowledgeEdge field
source_license_state: first_party
pair_kind: router
```

Teaching note — KnowledgeEdge field in routing decisions:

  KnowledgeEdge: false → normal routing; a sub-phase was selected
  KnowledgeEdge: true  → no sub-phase selected; request goes to knowledge-edge path

  KnowledgeEdge is set to true when:
    - CompiledPredicateIds is empty (no predicate matched)
    - SourceStatus contains "knowledge_edge" from the compiler

  When KnowledgeEdge is true:
    PredictedPhaseId: null
    PredictedSubPhaseId: null
    FailureClassification: "no_predicate_match"

  Example (from B06B1-WDA-kedge-001):
    Input: "Our checkout needs to be better"
    CompiledPredicateIds: []
    RoutingDecision: KnowledgeEdge: true, PredictedSubPhaseId: null

  The KnowledgeEdge result is not a routing decision — it is a routing failure
  that triggers the capability-gap learning pipeline.
