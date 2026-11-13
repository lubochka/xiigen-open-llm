# Layer 0-A — Compiler Pairs (037)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-037-001 — AccordionPanel: "build an accordion panel" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-037-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build an accordion panel component with expand and collapse animation for FAQ sections."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "component", "panel" (via "layout" family)
  IMPLEMENT fires on: "build"
  TransitionType: feature_addition (IMPLEMENT present).
  Routing: 1.1 (UI) + 4.1 (IMPLEMENT).

---

## B01-0A-037-002 — AccordionPanel: "design the accordion interaction" fires UI only

```
source_id: xiigen-b01-0a-037-002
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Design the accordion interaction model with expand, collapse, and keyboard navigation states."
  CompiledPredicateIds: ["UI"]
  Confidence: 0.85

Predicate evidence:
  UI fires on: "interaction" (UI lexicon term)
  "design" alone does NOT fire IMPLEMENT.
  "accordion" NOT in UI lexicon; fires only via co-occurring "interaction".
  TransitionType: documentation (UI present, IMPLEMENT absent).

---

## B01-0A-037-003 — AccordionPanel: "improve accordion animation speed" fires nothing

```
source_id: xiigen-b01-0a-037-003
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Improve the accordion animation speed and transition timing for smoother user experience."
  CompiledPredicateIds: []
  Confidence: 0.45

Predicate evidence:
  "improve" NOT in IMPLEMENT lexicon (gap verb).
  "animation" NOT in any predicate lexicon (confirmed gap from round-2).
  "transition timing" NOT in any predicate lexicon.
  "smoother" NOT in any predicate lexicon.
  CompiledPredicateIds: [] → knowledge-edge path.

---

## B01-0A-037-004 — AccordionPanel: four-predicate sweep

```
source_id: xiigen-b01-0a-037-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build the accordion component, document its accessibility requirements, define the API schema, and write tests."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT", "TEST"]
  Confidence: 0.93

Predicate evidence:
  UI fires on: "component", "accessibility"
  IMPLEMENT fires on: "build", "write"
  DOCUMENT fires on: "document", "requirements", "accessibility" (secondary)
  TEST fires on: "tests"
  Four predicates → four independent routing decisions.
