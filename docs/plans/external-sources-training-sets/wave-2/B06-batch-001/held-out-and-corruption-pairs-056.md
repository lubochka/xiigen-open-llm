# Held-Out and Corruption Pairs (056)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: held_out | corruption

---

## Held-Out Pair B06B1-WDA-HO-112 — PriceMatchPolicy: DOCUMENT predicate fires

```
source_id: xiigen-b06-held-out-112
pair_kind: held_out
```

A:
  run_id: "B06-ho-112"
  scenario: "DocumentPredicateCheck"
  user_request: "Specify the requirements for the pricematchpolicy system."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  SubPhaseId: "2.1"
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TransitionType: "documentation"
  DomainId: "software-development"

Generalization test: "specify" fires DOCUMENT; DomainId is software-development not pricematchpolicy.

---

## Held-Out Pair B06B1-WDA-HO-113 — PriceMatchPolicy: knowledge-edge gap verb

```
source_id: xiigen-b06-held-out-113
pair_kind: held_out
```

A:
  run_id: "B06-ho-113"
  scenario: "KnowledgeEdgeCheck"
  user_request: "Analyze the pricematchpolicy system."

Expected B:
  CompiledPredicateIds: []
  RoutingDecision: "KNOWLEDGE_EDGE"
  Note: "analyze" is not in DOCUMENT lexicon (17 terms) and not an IMPLEMENT trigger.

Teaching: zero lexicon terms in request → KNOWLEDGE_EDGE. "analyze" is a gap verb.

---

## Corruption Pair B06B1-WDA-COR-58 — v6-01: DOCUMENT missed for PriceMatchPolicy

```
source_id: xiigen-b06-corruption-58
pair_kind: corruption
corruption_type: Corruption-v6-01-document-missed
```

A:
  user_request: "Write the specification for the pricematchpolicy feature."

B-correct:
  CompiledPredicateIds: ["DOCUMENT"]
  Note: "specification" is a DOCUMENT lexicon noun → DOCUMENT must fire.

B-corrupt:
  CompiledPredicateIds: ["IMPLEMENT"]
  Note: IMPLEMENT incorrectly fired; DOCUMENT missed.

Corruption: "specification" is DOCUMENT lexicon. IMPLEMENT has no trigger here.
Fix: Scan for DOCUMENT noun terms. "specification" → DOCUMENT. No IMPLEMENT without write/build/create/develop/implement verbs.

---

## Corruption Pair B06B1-WDA-COR-58b — v6-03: routing collapsed for PriceMatchPolicy

```
source_id: xiigen-b06-corruption-58b
pair_kind: corruption
corruption_type: Corruption-v6-03-routing-collapsed
```

A:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]
  Context: "PriceMatchPolicy: write requirements, design architecture, build service"

B-correct:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "3.1", SubPhaseId: "4.1"]
  TransitionCount: 3
  TransitionType: "feature_addition"

B-corrupt:
  RoutingDecisions: [SubPhaseId: "2.1"]
  TransitionCount: 1
  Note: 3.1 and 4.1 collapsed — only 2.1 emitted.

Corruption: routing collapsed from 3 SubPhases to 1. Each predicate generates its own routing decision.
Fix: DOCUMENT → 2.1; ARCHITECTURE → 3.1; IMPLEMENT → 4.1 — always emit all three when all three predicates present.
