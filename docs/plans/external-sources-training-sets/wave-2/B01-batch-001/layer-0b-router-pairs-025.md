# Layer 0-B — Router Pairs (025)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-025-001 — HeroSection: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-025-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-025-002 — ProductCard: UI-only routes to 1.1

```
source_id: xiigen-b01-0b-025-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI"]
B: DecisionCount: 1 | SubPhaseId: "1.1" | KnowledgeEdge: false

---

## B01-0B-025-003 — BreadcrumbNav gap: empty predicates route to knowledge-edge

```
source_id: xiigen-b01-0b-025-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Add breadcrumb navigation to every page in the product catalog."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"

Routing rule: "navigation" and "breadcrumb" are both gap vocabulary.
No lexicon match → no sub-phase assigned → knowledge-edge path only.

---

## B01-0B-025-004 — ReviewWidget + DOCUMENT: UI+DOCUMENT routes to 1.1 + 2.1

```
source_id: xiigen-b01-0b-025-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "DOCUMENT"]
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "2.1"] | KnowledgeEdge: false (both)
