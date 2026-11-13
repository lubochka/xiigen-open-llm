# Layer 0-A — Compiler Pairs (058)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-058-001 — CommentThread: "build a comment thread component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-058-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a comment thread component with nested replies, avatar display, and reaction buttons."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "component", "button"
  IMPLEMENT fires on: "build"
  "comment" NOT in UI lexicon; "thread" NOT in any lexicon — fires via "component" and "button".
  TransitionType: feature_addition.

---

## B01-0A-058-002 — CommentThread: "create threaded comment widget with rich text input" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-058-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create a threaded comment widget with rich text input, mention support, and attachment upload."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.93

Predicate evidence:
  UI fires on: "widget", "input"
  IMPLEMENT fires on: "create"
  TransitionType: feature_addition.

---

## B01-0A-058-003 — CommentThread: "document comment moderation use cases" fires DOCUMENT only

```
source_id: xiigen-b01-0a-058-003
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Document comment thread moderation use cases including flagging, hiding, and escalation workflows."
  CompiledPredicateIds: ["DOCUMENT"]
  Confidence: 0.88

Predicate evidence:
  DOCUMENT fires on: "document", "use cases"
  No UI lexicon terms present.
  No IMPLEMENT verb present.
  Single predicate → single routing decision.
  TransitionType: documentation.

---

## B01-0A-058-004 — CommentThread: five-predicate sweep

```
source_id: xiigen-b01-0a-058-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build the comment thread component, implement the moderation API, document use cases, define the notification schema, and write integration tests."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "ARCHITECTURE", "DOCUMENT", "TEST"]
  Confidence: 0.94

Predicate evidence:
  UI fires on: "component"
  IMPLEMENT fires on: "build", "implement", "write"
  ARCHITECTURE fires on: "API", "schema", "notification" (infrastructure)
  DOCUMENT fires on: "document", "use cases"
  TEST fires on: "tests" (via "integration tests")
  Five predicates → five independent routing decisions.
