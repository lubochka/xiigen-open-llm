# Layer 0-C — Knowledge Edge Pairs
# Batch: B39-batch-001
# Scenario: B39 — QA UI automation, sub-phases 5.3/7.3/1.5
# Contract: KnowledgeEdgeHandlerUnit : ILogicalUnit<SubPhaseRouterResult, CapabilityGapSignal>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: knowledge_edge

---

## Pair B39B1-WDA-edge-001 — "automate" alone does not fire TEST

```
source_id: xiigen-b39-edge-001
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: knowledge_edge
```

A:
  user_request: "Automate the user journey through the checkout process."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  PhaseId: null
  SubPhaseId: null
  SourceStatus: "knowledge_edge"
  Confidence: 0.70

  CapabilityGapSignal:
    gap_id: "gap-testing-automation-001"
    gap_vocabulary: ["automate", "user journey", "checkout process"]
    DomainId: "software-development"
    SCHEDULE_CAPABILITY_LEARNING: true

  Predicate evidence:
    "automate" — not in any predicate lexicon (automate ≠ IMPLEMENT ≠ TEST)
    "user journey" — gap (UX research vocabulary)
    "checkout process" — gap (product feature term)
    Result: zero predicates → knowledge_edge

  Critical boundary: "automate" is a frequent gap term in testing contexts. TEST requires
  the literal words: test, verify, assert, validate, spec, test suite. "Automate" without
  a test noun produces zero predicates.

---

## Pair B39B1-WDA-edge-002 — "dashboard" in UI testing context does not fire UI

```
source_id: xiigen-b39-edge-002
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: knowledge_edge
```

A:
  user_request: "Test the admin dashboard for responsive behavior across devices."

B:
  CompiledPredicateIds: ["TEST"]
  TransitionType: feature_addition
  PhaseId: 5
  SubPhaseId: 5.3
  SourceStatus: "ready"
  Confidence: 0.92

  Predicate evidence:
    "test" → TEST fires
    "admin dashboard" — UI does NOT fire on "dashboard" (not in UI lexicon, as established in B32)
    "responsive behavior" — "responsive" fires UI? Checking: UI lexicon includes "responsive"
      as a term (responsive design/layout). Yes: "responsive" → UI fires.
    "devices" — not in UI predicate

  Correction:
    CompiledPredicateIds: ["TEST", "UI"] — "responsive" fires UI.
    CapabilityGapWarnings:
      - "gap-ui-dashboard-001: 'dashboard' is not in UI lexicon"

  Teaching: "dashboard" is confirmed gap (established in B32). But "responsive" is in the
  UI lexicon — it fires UI even though "dashboard" does not. TEST fires via "test".

---

## Pair B39B1-WDA-edge-003 — Cypress/Selenium framework names are gap vocabulary

```
source_id: xiigen-b39-edge-003
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: knowledge_edge
```

A:
  user_request: "Set up Cypress to run automated browser tests on the product catalog page."

B:
  CompiledPredicateIds: ["TEST", "UI"]
  TransitionType: feature_addition
  PhaseId: 5
  SubPhaseId: 5.3
  SourceStatus: "ready"
  Confidence: 0.92

  Predicate evidence:
    "Cypress" — gap (framework name; not in any predicate lexicon)
    "run" — not in any predicate lexicon
    "automated" — gap
    "browser tests" — TEST fires on "tests"
    "product catalog page" — UI fires on "page"
    "set up" — IMPLEMENT? "set up" is not in the IMPLEMENT lexicon explicitly.
      "set up" is gap — it does not fire IMPLEMENT. IMPLEMENT requires: build, create,
      write, implement, code, add, develop, generate, scaffold, extend.
    Result: ["TEST", "UI"]
    CapabilityGapWarnings:
      - "gap-testing-automation-001: 'Cypress', 'automated', 'set up' are gap vocabulary"

  Teaching: Framework names (Cypress, Playwright, Selenium) are gap vocabulary. The
  predicate fires on generic test vocabulary ("browser tests" → TEST, "page" → UI),
  not on tool names.
