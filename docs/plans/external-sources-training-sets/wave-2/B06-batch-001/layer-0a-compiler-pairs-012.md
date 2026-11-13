# Layer 0-A — Compiler Pairs (012)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 012 is the B06 compiler pattern summary: complete predicate rule table.
This file consolidates all B06 compiler teaching into a reference table format.

---

## B06-0A-012-001 — Complete DOCUMENT predicate rule table for B06

```
source_id: xiigen-b06-0a-012-001
source_title: XIIGen B06 compiler — complete predicate rule table
source_license_state: first_party
pair_kind: compiler
```

B06 Compiler Predicate Rule Table:

  DOCUMENT lexicon (17 terms):
    document, spec, specification, requirement, story, criteria,
    describe, define, functional, behavior, use-case, narrative,
    feature-brief, capability, epics, business, stakeholder

  IMPLEMENT lexicon (relevant terms for B06):
    implement, code, build, develop, write, create, function,
    class, logic, library, handler, backend, feature, method, refactor

  UI lexicon (relevant terms for B06):
    ui, interface, ux, screen, page, form, component, layout

  ARCHITECTURE lexicon (relevant terms for B06):
    design, architecture, diagram, component (shared with UI), structure,
    pattern, system, model

---

## B06-0A-012-002 — B06 predicate firing summary table

```
source_id: xiigen-b06-0a-012-002
source_title: XIIGen B06 compiler — predicate firing summary
source_license_state: first_party
pair_kind: compiler
```

B06 Request Pattern → Predicate Result Table:

  Request pattern                                  | Result
  ------------------------------------------------|-------------------------
  "define the requirements"                        | ["DOCUMENT"]
  "describe the use cases"                         | ["DOCUMENT"]
  "document the business rules"                    | ["DOCUMENT"]
  "specify the behavior"                           | ["DOCUMENT"]
  "write the requirements"                         | ["DOCUMENT", "IMPLEMENT"]
  "write the user stories"                         | ["DOCUMENT", "IMPLEMENT"]
  "write the specification"                        | ["DOCUMENT", "IMPLEMENT"]
  "create the feature brief"                       | ["DOCUMENT", "IMPLEMENT"]
  "design the architecture and document the spec"  | ["ARCHITECTURE", "DOCUMENT"]
  "design the architecture and write the spec"     | ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT"]
  "document the form requirements"                 | ["DOCUMENT", "UI"]
  "write the form requirements"                    | ["DOCUMENT", "IMPLEMENT", "UI"]
  "design, implement, document, UI flow"           | ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT", "UI"]
  "describe the behavior of checkout"              | ["DOCUMENT"]
  "define the epics and capabilities"              | ["DOCUMENT"]
  "our checkout needs to be better"               | [] (knowledge edge)

---

## B06-0A-012-003 — B06 dual-predicate canonical cases

```
source_id: xiigen-b06-0a-012-003
source_title: XIIGen B06 compiler — dual predicate canonical cases
source_license_state: first_party
pair_kind: compiler
```

Canonical dual-predicate cases for B06:

  Case 1 — "write" + requirements noun:
    "write the user stories"           → DOCUMENT (story) + IMPLEMENT (write)
    "write the specification"          → DOCUMENT (specification) + IMPLEMENT (write)
    "write the requirements"           → DOCUMENT (requirement) + IMPLEMENT (write)
    "write the feature brief"          → DOCUMENT (feature-brief) + IMPLEMENT (write)
    "write the business requirements"  → DOCUMENT (business, requirement) + IMPLEMENT (write)

  Case 2 — "create" + requirements noun:
    "create the feature brief"         → DOCUMENT (feature-brief) + IMPLEMENT (create)
    "create the requirements document" → DOCUMENT (requirement, document) + IMPLEMENT (create)

  Case 3 — "build" + requirements noun (rare but valid):
    "build the requirements document"  → DOCUMENT (requirement, document) + IMPLEMENT (build)

  Anti-pattern to avoid:
    "describe the user stories" → DOCUMENT only (describe is NOT in IMPLEMENT lexicon)
    "define the requirements"   → DOCUMENT only (define is NOT in IMPLEMENT lexicon)

---

## B06-0A-012-004 — B06 v6 corruption types at compiler level

```
source_id: xiigen-b06-0a-012-004
source_title: XIIGen B06 compiler — v6 corruption types summary
source_license_state: first_party
pair_kind: compiler
```

B06 v6 Corruption Types (from B06 Core Rule 20):

  v6-01 — DOCUMENT missed on requirements request:
    Symptom: CompiledPredicateIds: [] for a request with "define", "requirements",
      "use cases", or other clear DOCUMENT terms.
    Cause: Failure to match lexicon terms.
    Fix: DOCUMENT fires on any of the 17 DOCUMENT lexicon terms.

  v6-02 — IMPLEMENT suppressed when "write" fires alongside DOCUMENT:
    Symptom: CompiledPredicateIds: ["DOCUMENT"] for "write the user stories".
    Cause: Intent-based suppression of "write" in documentation context.
    Fix: Compiler is lexicon-based. "write" always fires IMPLEMENT.
      CompiledPredicateIds must be ["DOCUMENT", "IMPLEMENT"].

  v6-03 — Routing collapsed to single sub-phase:
    Symptom: Router produces one routing decision (2.1) when both 2.1 and 2.4
      signals are present in a DOCUMENT-predicate request with "stakeholder".
    Cause: Router fails to check all sub-phase signals; stops at first match.
    Fix: Router must produce one decision per sub-phase where signal density
      exceeds threshold. One predicate → multiple routing decisions is valid.

  All three corruptions are covered by held-out and corruption pairs in file
  held-out-and-corruption-pairs-011.md.
