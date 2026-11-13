# Layer 0-B — Router Pairs (027)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-027-001 — DatePicker: UI-only routes to 1.1

```
source_id: xiigen-b01-0b-027-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI"]
   User request: "Design a date picker component with month/year navigation and day selection."
B: DecisionCount: 1 | SubPhaseId: "1.1" | KnowledgeEdge: false

---

## B01-0B-027-002 — ImageGallery: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-027-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Create an image gallery component with thumbnail grid and lightbox view."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-027-003 — TabNavigation: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-027-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a tab navigation component with animated indicator and keyboard accessibility."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

Note: "navigation" in user request does NOT fire UI; routing is driven by "component" and "accessibility" alone.

---

## B01-0B-027-004 — Three-predicate UI+IMPLEMENT+DOCUMENT routes to 1.1 + 4.1 + 2.1

```
source_id: xiigen-b01-0b-027-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"]
   User request: "Build the hero section component and write the specification for its visual behavior."
B: DecisionCount: 3 | SubPhaseIds: ["1.1", "4.1", "2.1"] | KnowledgeEdge: false (all)

v6-03 guard: DecisionCount (3) == len(CompiledPredicateIds) (3). Verified.
Each predicate owns one independent RoutingDecision entry.
