# Layer 0-A — Compiler Pairs (052)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-052-001 — FileUploadWidget: "build a file upload widget" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-052-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a file upload widget with drag-and-drop zone, progress indicator, and multi-file support."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "widget"
  IMPLEMENT fires on: "build"
  "drag-and-drop" NOT in any predicate lexicon (gap term).
  "progress" NOT in any predicate lexicon.
  Fires via "widget" + "build" only.
  TransitionType: feature_addition.

---

## B01-0A-052-002 — FileUploadWidget: "create upload form with validation" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-052-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create a file upload form with client-side type validation and error feedback."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "form"
  IMPLEMENT fires on: "create"
  TransitionType: feature_addition.

---

## B01-0A-052-003 — FileUploadWidget: "define file upload service API" fires ARCHITECTURE only

```
source_id: xiigen-b01-0a-052-003
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Define the file upload service API schema including endpoint, file size limits, and supported MIME types."
  CompiledPredicateIds: ["ARCHITECTURE"]
  Confidence: 0.89

Predicate evidence:
  ARCHITECTURE fires on: "API", "service", "schema"
  "define" NOT in IMPLEMENT lexicon (gap verb).
  No UI lexicon terms present.
  Single predicate → single routing decision.
  TransitionType: documentation.

---

## B01-0A-052-004 — FileUploadWidget: UI+IMPLEMENT+SECURITY+DOCUMENT four-predicate

```
source_id: xiigen-b01-0a-052-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build the file upload widget, implement server-side virus scanning, document the upload requirements, and define the storage API."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "SECURITY", "DOCUMENT", "ARCHITECTURE"]
  Confidence: 0.94

Predicate evidence:
  UI fires on: "widget"
  IMPLEMENT fires on: "build", "implement"
  SECURITY fires on: "virus scanning", "server-side" (security control)
  DOCUMENT fires on: "document", "requirements"
  ARCHITECTURE fires on: "API", "storage"
  Five predicates → five independent routing decisions.
