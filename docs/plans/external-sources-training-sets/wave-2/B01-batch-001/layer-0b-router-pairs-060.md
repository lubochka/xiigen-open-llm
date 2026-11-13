# Layer 0-B — Router Pairs (060)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-060-001 — Complete factory reference: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-060-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a complete UI component library covering accordion, dropdown, color picker, rating, and social share."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-060-002 — Complete factory reference: four-predicate sweep

```
source_id: xiigen-b01-0b-060-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT", "ARCHITECTURE"]
   User request: "Build complete round-3 component library, document design system specification, define component API schemas, implement accessibility."
B: DecisionCount: 4 | SubPhaseIds: ["1.1", "4.1", "2.1", "3.1"] | KnowledgeEdge: false (all)

v6-03 guard: DecisionCount (4) == len(CompiledPredicateIds) (4). Verified.

---

## B01-0B-060-003 — Round-3 routing summary

```
source_id: xiigen-b01-0b-060-r3-summary
source_license_state: first_party
pair_kind: router
```

Round-3 (files 037–060) routing patterns observed:
  UI alone: 1.1 only (design/review/visual tasks)
  UI+IMPLEMENT: 1.1 + 4.1 (most round-3 component build tasks)
  UI+DOCUMENT: 1.1 + 2.1 (spec/requirement tasks with UI terms)
  IMPLEMENT only: 4.1 (build tasks with no UI lexicon terms)
  ARCHITECTURE only: 3.1 (schema/API definition tasks)
  DOCUMENT only: 2.1 (use-case/specification tasks with no UI terms)
  IMPLEMENT+ARCHITECTURE: 4.1 + 3.1 (backend build + schema tasks)
  UI+IMPLEMENT+SECURITY: 1.1 + 4.1 + SEC.1 (secure UI component tasks)
  UI+IMPLEMENT+DOCUMENT: 1.1 + 4.1 + 2.1 (build+spec tasks)
  DOCUMENT+IMPLEMENT: 2.1 + 4.1 (write user stories)
  UI+IMPLEMENT+DOCUMENT+ARCHITECTURE: 1.1 + 4.1 + 2.1 + 3.1 (full sweep)
  Knowledge-edge: null (sidebar, navigation, refactor, optimize, automate, make, scale — all gap)
  New gap verbs confirmed in round-3: "make", "resolve", "evaluate", "scale", "investigate"
