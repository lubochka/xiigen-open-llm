# Layer 0-B — Router Pairs (011)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: router

---

## Method note

File 011 covers the v6-03 corruption at router level: DOCUMENT routes to single
sub-phase when two sub-phase signals present. Three corruption examples.

---

## B06-0B-011-001 — v6-03 corruption: [DOCUMENT] routes to 2.1 only, misses 2.4

```
source_id: xiigen-b06-0b-011-001
source_title: XIIGen B06 router — v6-03 single routing when dual expected
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  DocumentId: "B06-0b-011-001-prompt"
  CompiledText: "Define the use cases and stakeholder requirements for the checkout flow, covering all user types and external partner needs."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"],
      SubPhaseSignals: ["use cases", "define"], KnowledgeEdge: false
    - PredictedSubPhaseId: "2.4", EvidencePredicates: ["DOCUMENT"],
      SubPhaseSignals: ["stakeholder", "external partner"], KnowledgeEdge: false
  DecisionCount: 2

B-corrupt (v6-03):
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  DecisionCount: 1

Corruption type: v6-03 — routing to single sub-phase.
Why harmful: "stakeholder requirements" and "external partner needs" are
  sub-phase 2.4 artifacts. When routing is collapsed to 2.1 only, the
  stakeholder requirements document is never authored.
Fix: Router must scan for all sub-phase signals and produce a decision
  for each sub-phase where signal density is sufficient.

---

## B06-0B-011-002 — v6-03 corruption: [DOCUMENT, IMPLEMENT] collapses to 2.1 only

```
source_id: xiigen-b06-0b-011-002
source_title: XIIGen B06 router — v6-03 IMPLEMENT routing collapsed into DOCUMENT
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  DocumentId: "B06-0b-011-002-prompt"
  CompiledText: "Write the user stories for the payment feature with acceptance criteria."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
    - PredictedSubPhaseId: "4.1", EvidencePredicates: ["IMPLEMENT"], KnowledgeEdge: false
  DecisionCount: 2

B-corrupt (v6-03 variant):
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT", "IMPLEMENT"],
      KnowledgeEdge: false
  DecisionCount: 1

Corruption type: v6-03 — multi-predicate routing collapsed to one sub-phase.
Note: This variant is the same corruption as COR-003 from the original batch —
  reproduced here as router-pair teaching.
Why harmful: IMPLEMENT predicate is absorbed into the 2.1 decision, which does not
  own IMPLEMENT. Sub-phase 4.1 is never selected.
Fix: Each predicate routes to its own sub-phase. Never merge two predicates
  into one routing decision.

---

## B06-0B-011-003 — v6-03 three corruption forms compared

```
source_id: xiigen-b06-0b-011-003
source_title: XIIGen B06 router — v6-03 three forms compared
source_license_state: first_party
pair_kind: router
```

v6-03 corruption occurs in three forms in B06:

  Form A — one predicate should route to two sub-phases but routes to one:
    Correct:  ["DOCUMENT"] → 2.1 + 2.4 (when stakeholder signals present)
    Corrupt:  ["DOCUMENT"] → 2.1 only

  Form B — two predicates should route to two sub-phases but routes to one:
    Correct:  ["DOCUMENT", "IMPLEMENT"] → 2.1 + 4.1
    Corrupt:  ["DOCUMENT", "IMPLEMENT"] → 2.1 only (IMPLEMENT swallowed into 2.1)

  Form C — two predicates route correctly but wrong sub-phase assigned:
    Correct:  ["DOCUMENT", "IMPLEMENT"] → 2.1 (DOCUMENT) + 4.1 (IMPLEMENT)
    Corrupt:  ["DOCUMENT", "IMPLEMENT"] → 2.1 (DOCUMENT) + 2.1 (IMPLEMENT merged)

  All three forms result in one or more sub-phases being missed.
  The model must produce one routing decision per predicate (Form B fix)
  and one additional decision per sub-phase signal when one predicate
  maps to multiple sub-phases (Form A fix).
