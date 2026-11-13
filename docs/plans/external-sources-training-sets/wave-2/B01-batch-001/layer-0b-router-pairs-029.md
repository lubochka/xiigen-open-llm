# Layer 0-B — Router Pairs (029)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-029-001 — HeroSection responsive: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-029-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a hero section with a responsive layout for mobile and desktop viewports."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-029-002 — DatePicker with color themes: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-029-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a date picker page with support for custom color themes and RTL layout."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-029-003 — UI+DOCUMENT routes to 1.1 + 2.1 (pagination spec)

```
source_id: xiigen-b01-0b-029-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "DOCUMENT"]
   User request: "Specify the pagination UI behavior and document the button state transitions."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "2.1"] | KnowledgeEdge: false (both)

---

## B01-0B-029-004 — "enhance card experience + improve journey" → knowledge-edge

```
source_id: xiigen-b01-0b-029-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Enhance the product card experience and improve the overall checkout journey."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"

Gap vocabulary: "enhance", "experience", "improve", "journey" — none in any predicate lexicon.
