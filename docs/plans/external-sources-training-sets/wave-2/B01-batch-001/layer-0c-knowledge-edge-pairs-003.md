# Layer 0-C — Knowledge-Edge Pairs (003)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-003-001 — "navigation" not in UI lexicon; what IS in UI for navigation-adjacent concepts

```
source_id: xiigen-b01-0c-003-001
source_title: XIIGen B01 knowledge-edge-003 — "navigation" gap; UI alternatives
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
pair_kind: knowledge_edge
coverage_status: covered
```

A:
  DocumentId: "B01-0c-003-001-prompt"
  CompiledText: "Map the navigation structure of our SaaS application."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match"

Predicate analysis:
  "navigation" — checked against all predicate lexicons:
    UI lexicon: absent
    DOCUMENT lexicon: absent
    IMPLEMENT lexicon: absent
    ARCHITECTURE lexicon: absent
  "structure" — checked:
    UI: absent; ARCHITECTURE: borderline but not listed
  "SaaS", "application" — no predicate terms
  Result: CompiledPredicateIds: [] → knowledge-edge path

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics:
    knowledge_edge_reason: "no_predicate_match"
    matched_predicate_count: "0"
    capability_gap_id: "gap-navigation-no-predicate"
    capability_gap_description: "Request uses 'navigation' without any UI lexicon term co-present. 'navigation' is not in the UI lexicon."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  Warnings: ["knowledge_edge: no predicate matched"]

---

## B01-0C-003-002 — Navigation-adjacent requests that DO fire UI

```
source_id: xiigen-b01-0c-003-002
source_title: XIIGen B01 knowledge-edge-003 — navigation with co-present UI terms
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

A:
  DocumentId: "B01-0c-003-002-prompt"
  CompiledText: "Map the page navigation and document which screens link to each other."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate analysis:
  "page" → UI fires (UI lexicon)
  "navigation" → NO fire (not in UI lexicon)
  "screen" (via "screens") → UI fires
  "document" → DOCUMENT fires
  Result: ["UI", "DOCUMENT"] — UI fires via "page" and "screen", not via "navigation"

B:
  FlowId: "universal-nine-node-development-flow-1.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    knowledge_edge_reason: "null"
    matched_predicate_count: "2"
    recommended_action: "EXECUTE_FLOW"
  Warnings: []

Key learning: The word "navigation" in a request is always inert from a predicate
perspective. UI fires only when one of the 22 UI lexicon terms is present.
Co-presence of a UI term alongside "navigation" routes the request normally.
The "navigation" word itself carries no routing weight.

---

## B01-0C-003-003 — navigation alone → knowledge-edge; navigation + "page" → UI fires

```
source_id: xiigen-b01-0c-003-003
source_title: XIIGen B01 knowledge-edge-003 — navigation lexicon contrast pair
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

Contrast pair demonstrating the navigation boundary:

  Request A: "Fix the navigation so users can find the checkout screen faster."
    → "screen" fires UI; "Fix" is borderline (not in IMPLEMENT: "fix" is NOT listed;
      "refactor" is but "fix" is not). Actually "fix" is commonly used but let's
      check: IMPLEMENT lexicon: implement, code, build, develop, write, create,
      function, class, logic, library, handler, middleware, controller, backend,
      feature, method, refactor. "fix" is absent.
    → Predicate result: ["UI"] (from "screen" only)
    → Routes to: 1.1

  Request B: "Fix the navigation so users can find things faster."
    → "navigation" → NO fire; "fix" → NO fire (not in IMPLEMENT)
    → Predicate result: []
    → knowledge-edge

  Contrast conclusion: identical intent ("fix navigation") but different predicate
  outcomes depending solely on the presence of the word "screen" in Request A.
  The predicate system is lexical, not semantic. Adding or removing one UI lexicon
  term changes routing entirely. This is why precise vocabulary matters in B01.
