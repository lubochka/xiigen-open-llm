# Layer 0-B — Router Pairs (051)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-051-001 — RichTextEditor: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-051-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Implement a rich text editor component with formatting toolbar and markdown export."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-051-002 — RichTextEditor: UI+IMPLEMENT (toolbar widget) routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-051-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Create a toolbar widget for text formatting with bold, italic, underline, and link buttons."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-051-003 — RichTextEditor: DOCUMENT+IMPLEMENT (write specification) routes to 2.1 + 4.1

```
source_id: xiigen-b01-0b-051-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
   User request: "Write the feature specification for the rich text editor."
B: DecisionCount: 2 | SubPhaseIds: ["2.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-051-004 — RichTextEditor: "fix the UX" — UI borderline case routes to 1.1

```
source_id: xiigen-b01-0b-051-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI"]
   User request: "The rich text editor feels clunky — figure out why typing is laggy and fix the UX."
B: DecisionCount: 1 | SubPhaseId: "1.1" | KnowledgeEdge: false

Note: "ux" IS in UI lexicon — fires UI even in casual phrasing. This is NOT a knowledge-edge case.
TransitionType: documentation (UI present, no IMPLEMENT verb).
