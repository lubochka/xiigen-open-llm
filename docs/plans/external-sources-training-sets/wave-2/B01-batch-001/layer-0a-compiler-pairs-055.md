# Layer 0-A — Compiler Pairs (055)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-055-001 — CodeEditor: "implement an embedded code editor component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-055-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Implement an embedded code editor component with syntax highlighting, line numbers, and auto-complete."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "component"
  IMPLEMENT fires on: "implement"
  "code editor" NOT in UI lexicon; fires via "component".
  TransitionType: feature_addition.

---

## B01-0A-055-002 — CodeEditor: "create a theme-switchable editor with design-system colors" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-055-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create a theme-switchable code editor that applies design-system color tokens for syntax highlighting."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.94

Predicate evidence:
  UI fires on: "theme", "color", "design-system"
  IMPLEMENT fires on: "create"
  Three UI lexicon hits; high confidence.
  TransitionType: feature_addition.

---

## B01-0A-055-003 — CodeEditor: "document editor accessibility requirements" fires UI+DOCUMENT

```
source_id: xiigen-b01-0a-055-003
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Document the code editor accessibility requirements for keyboard navigation and screen reader compatibility."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "accessibility", "screen" (via "screen reader")
  DOCUMENT fires on: "document", "requirements"
  TransitionType: documentation (no IMPLEMENT).

---

## B01-0A-055-004 — CodeEditor: "editor is too slow to type in" fires nothing

```
source_id: xiigen-b01-0a-055-004
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "The editor is too slow to type in — profile the keystroke handler and reduce latency."
  CompiledPredicateIds: []
  Confidence: 0.39

Predicate evidence:
  "too slow" NOT in any predicate lexicon.
  "profile" NOT in any predicate lexicon.
  "keystroke" NOT in any predicate lexicon.
  "latency" NOT in any predicate lexicon.
  CompiledPredicateIds: [] → knowledge-edge path.
