# Layer 0-B — Router Pairs (042)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-042-001 — EmptyState: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-042-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Create an empty state component with illustration, heading, and call-to-action button."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-042-002 — EmptyState: UI only routes to 1.1

```
source_id: xiigen-b01-0b-042-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI"]
   User request: "Design empty state screens for the dashboard, search results, and notification center."
B: DecisionCount: 1 | SubPhaseId: "1.1" | KnowledgeEdge: false

---

## B01-0B-042-003 — EmptyState: DOCUMENT+IMPLEMENT (write user stories) routes to 2.1 + 4.1

```
source_id: xiigen-b01-0b-042-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
   User request: "Write user stories for the empty state behavior across all product list and search result views."
B: DecisionCount: 2 | SubPhaseIds: ["2.1", "4.1"] | KnowledgeEdge: false (both)

Note: "write user stories" fires both DOCUMENT and IMPLEMENT per special rule. UI does NOT fire — no UI lexicon terms present.

---

## B01-0B-042-004 — EmptyState: knowledge-edge (enhance gap)

```
source_id: xiigen-b01-0b-042-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Enhance empty state messaging to feel more friendly and encourage user action."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"
