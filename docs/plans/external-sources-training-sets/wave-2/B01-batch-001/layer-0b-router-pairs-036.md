# Layer 0-B — Router Pairs (036)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-036-001 — Component library: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-036-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a complete UI component library covering hero, card, modal, and gallery components."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-036-002 — HeroSection redesign gap: knowledge-edge

```
source_id: xiigen-b01-0b-036-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Plan the hero section redesign journey and map the user engagement strategy."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"

---

## B01-0B-036-003 — Four-predicate (pagination+modal): UI+IMPLEMENT+DOCUMENT+ARCHITECTURE routes to 1.1 + 4.1 + 2.1 + 3.1

```
source_id: xiigen-b01-0b-036-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT", "ARCHITECTURE"]
   User request: "Build the pagination and modal components, document their accessibility requirements, and define the API service for loading paginated data."
B: DecisionCount: 4 | SubPhaseIds: ["1.1", "4.1", "2.1", "3.1"] | KnowledgeEdge: false (all)

v6-03 guard: DecisionCount (4) == len(CompiledPredicateIds) (4). Verified.

---

## B01-0B-036-004 — Round-2 routing summary

```
source_id: xiigen-b01-0b-036-r2-summary
source_license_state: first_party
pair_kind: router
```

Round-2 (files 025–036) routing patterns observed:
  UI alone: 1.1 only (DatePicker design, modal wireframe, tab prototype)
  UI+IMPLEMENT: 1.1 + 4.1 (most round-2 component build tasks)
  UI+DOCUMENT: 1.1 + 2.1 (widget spec, gallery requirements)
  UI+IMPLEMENT+DOCUMENT: 1.1 + 4.1 + 2.1 (build+spec tasks)
  UI+IMPLEMENT+ARCHITECTURE: 1.1 + 4.1 + 3.1 (build+schema tasks)
  UI+IMPLEMENT+DOCUMENT+ARCHITECTURE: 1.1 + 4.1 + 2.1 + 3.1 (full sweep)
  Knowledge-edge: null (redesign, enhance, journey, brand, improve — all gap)
