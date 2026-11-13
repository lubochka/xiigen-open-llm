# Layer 0-A — Compiler Pairs (040)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-040-001 — ProgressBar: "build a progress bar component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-040-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a progress bar component that shows task completion percentage with animated fill."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.91

Predicate evidence:
  UI fires on: "component"
  IMPLEMENT fires on: "build"
  "progress bar" NOT directly in UI lexicon; fires via co-occurring "component".
  TransitionType: feature_addition.

---

## B01-0A-040-002 — ProgressBar: "design a progress indicator" fires UI only

```
source_id: xiigen-b01-0a-040-002
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Design a progress indicator with visual states for loading, partial, and complete."
  CompiledPredicateIds: ["UI"]
  Confidence: 0.86

Predicate evidence:
  UI fires on: "visual" (UI lexicon term), "screen" context implied but "indicator" not in lexicon
  "design" does NOT fire IMPLEMENT.
  TransitionType: documentation (UI only, no IMPLEMENT).

---

## B01-0A-040-003 — ProgressBar: "write test suite for progress bar" fires UI+IMPLEMENT+TEST

```
source_id: xiigen-b01-0a-040-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Write a test suite for the progress bar component covering all state transitions."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "TEST"]
  Confidence: 0.90

Predicate evidence:
  UI fires on: "component"
  IMPLEMENT fires on: "write" (code artifact)
  TEST fires on: "test" (via "test suite")
  Three predicates → three independent routing decisions.
  TransitionType: feature_addition (IMPLEMENT present via "write").

---

## B01-0A-040-004 — ProgressBar: "track progress across steps" fires nothing

```
source_id: xiigen-b01-0a-040-004
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Track progress across wizard steps and show percentage completion to the user."
  CompiledPredicateIds: []
  Confidence: 0.43

Predicate evidence:
  "track" NOT in any predicate lexicon.
  "progress" NOT in any predicate lexicon (domain noun).
  "wizard steps" NOT in any predicate lexicon.
  "percentage" NOT in any predicate lexicon.
  CompiledPredicateIds: [] → knowledge-edge path.
