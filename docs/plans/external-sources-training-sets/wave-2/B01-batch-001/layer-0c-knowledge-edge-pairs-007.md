# Layer 0-C — Knowledge-Edge Pairs (007)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-007-001 — "write" fires IMPLEMENT even in documentation context

```
source_id: xiigen-b01-0c-007-001
source_title: XIIGen B01 knowledge-edge-007 — "write" always fires IMPLEMENT
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
pair_kind: knowledge_edge
coverage_status: covered
```

A:
  DocumentId: "B01-0c-007-001-prompt"
  CompiledText: "Write the accessibility specification for the onboarding screens."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate analysis:
  "write" → IMPLEMENT fires (IMPLEMENT lexicon includes "write" unconditionally)
  "accessibility" → UI fires
  "screen" (via "screens") → UI fires
  "specification" → DOCUMENT fires
  Result: ["UI", "DOCUMENT", "IMPLEMENT"]

The "write" rule — detailed explanation:
  "write" is in the IMPLEMENT lexicon regardless of what is being written.
  "Write the specification" → IMPLEMENT fires on "write" + DOCUMENT fires on
    "specification". Both fire simultaneously. Neither cancels the other.
  "Write the user stories" → IMPLEMENT + DOCUMENT (story)
  "Write the test cases" → IMPLEMENT (no DOCUMENT because "test" and "cases" are not
    in DOCUMENT lexicon — "criteria" would be needed)
  "Write code for the component" → IMPLEMENT (write, code, component→IMPLEMENT if
    contextual; component is UI so UI also fires)

This rule is unconditional. "Write" always contributes an IMPLEMENT fire.

B:
  FlowId: "universal-nine-node-development-flow-1.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    matched_predicate_count: "3"
    routing_summary: "UI → 1.1; DOCUMENT → 2.1; IMPLEMENT → 4.1"
    recommended_action: "EXECUTE_FLOW"
  Warnings: []

---

## B01-0C-007-002 — "write" in a DOCUMENT-only sentence still fires IMPLEMENT

```
source_id: xiigen-b01-0c-007-002
source_title: XIIGen B01 knowledge-edge-007 — "write" IMPLEMENT fire cannot be suppressed
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

A:
  DocumentId: "B01-0c-007-002-prompt"
  CompiledText: "Write the user journey narrative for the onboarding flow."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate analysis:
  "write" → IMPLEMENT fires
  "narrative" → DOCUMENT fires (DOCUMENT lexicon)
  "user journey" → "journey" not in any lexicon; "user" not in any lexicon
  "onboarding" → not in any lexicon
  "flow" → not in any lexicon
  Result: ["DOCUMENT", "IMPLEMENT"]
  Note: This might seem like a pure documentation request to a human reader,
    but the lexical rule is strict: "write" always fires IMPLEMENT.
    The IMPLEMENT fire causes TransitionType: feature_addition even though the
    primary output is a narrative document.

B:
  FlowId: "universal-nine-node-development-flow-2.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    matched_predicate_count: "2"
    routing_summary: "DOCUMENT → 2.1; IMPLEMENT → 4.1"
    recommended_action: "EXECUTE_FLOW"
  Warnings: []

---

## B01-0C-007-003 — Contrast: "document" does NOT fire IMPLEMENT

```
source_id: xiigen-b01-0c-007-003
source_title: XIIGen B01 knowledge-edge-007 — "document" is DOCUMENT lexicon only
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

Contrast to pair 001/002:

  "Document the screen flows" → DOCUMENT fires on "document"; UI fires on "screen"
    → ["UI", "DOCUMENT"] — IMPLEMENT does NOT fire

  "Write the screen flow documentation" → IMPLEMENT fires on "write"; UI fires on
    "screen"; DOCUMENT fires on "documentation" (treated as "document" root)
    → ["UI", "DOCUMENT", "IMPLEMENT"]

  The difference: "document" (as a verb) fires only DOCUMENT.
    "write" (as a verb) fires only IMPLEMENT.
    Both can be in the same sentence with different DOCUMENT context words firing.

  Summary table:
    Verb | Predicate fired
    -----|----------------
    document | DOCUMENT
    write    | IMPLEMENT
    describe | DOCUMENT
    define   | DOCUMENT
    create   | IMPLEMENT
    implement | IMPLEMENT
    build    | IMPLEMENT
    spec     | DOCUMENT
