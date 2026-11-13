# Layer 0-C — Knowledge Edge Pairs
# Batch: B20-batch-001
# Scenario: B20 — Unit testing, sub-phase 5.1
# Contract: KnowledgeEdgeHandlerUnit : ILogicalUnit<SubPhaseRouterResult, CapabilityGapSignal>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: knowledge_edge

---

## Pair B20B1-WDA-edge-001 — "check that it works" does not fire TEST

```
source_id: xiigen-b20-edge-001
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: knowledge_edge
```

A:
  user_request: "Check that the login works correctly before the release."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  PhaseId: null
  SubPhaseId: null
  SourceStatus: "knowledge_edge"
  Confidence: 0.70

  CapabilityGapSignal:
    gap_id: "gap-testing-informal-check-001"
    gap_vocabulary: ["check that", "works correctly"]
    DomainId: "software-development"
    SCHEDULE_CAPABILITY_LEARNING: true

  Predicate evidence:
    "check" — not in TEST predicate lexicon. TEST fires on: test, verify, assert,
      validate, spec, mock, unit test, integration test, write tests.
    "works correctly" — informal QA phrase; not in any predicate lexicon
    "before the release" — temporal qualifier; no predicate
    Result: zero predicates → knowledge_edge

  Critical boundary: "check" ≠ TEST. Informal correctness verification language ("check
  that it works", "make sure it works", "confirm it's correct") has no predicate.
  The predicate compiler requires the literal terms of the TEST lexicon.

---

## Pair B20B1-WDA-edge-002 — "QA the flow" fires QA not TEST

```
source_id: xiigen-b20-edge-002
transition_type: feature_addition
phase: 7
sub_phase: 7.3
coverage_status: ready
pair_kind: knowledge_edge
```

A:
  user_request: "QA the payment checkout flow before launch."

B:
  CompiledPredicateIds: ["QA"]
  TransitionType: feature_addition
  PhaseId: 7
  SubPhaseId: 7.3
  SourceStatus: "ready"
  Confidence: 0.92

  Predicate evidence:
    "QA" — QA predicate fires on the literal "QA" abbreviation
    "payment checkout flow" — no predicate (product feature term)
    "before launch" — temporal qualifier; no predicate
    "test" does NOT appear — TEST does not fire
    Result: ["QA"] only; TEST does not fire because "test" is not in the sentence

  Teaching: QA and TEST are separate predicates. "QA" fires the QA predicate.
  "test" fires the TEST predicate. The two are not interchangeable. "QA the flow"
  routes to QA (7.3), not to TEST (5.1).

---

## Pair B20B1-WDA-edge-003 — "mock" alone does not fire TEST

```
source_id: xiigen-b20-edge-003
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: knowledge_edge
```

A:
  user_request: "Mock the database layer for the repository."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  PhaseId: null
  SubPhaseId: null
  SourceStatus: "knowledge_edge"
  Confidence: 0.70

  CapabilityGapSignal:
    gap_id: "gap-testing-mock-setup-001"
    gap_vocabulary: ["mock", "database layer", "repository"]
    DomainId: "software-development"
    SCHEDULE_CAPABILITY_LEARNING: true

  Predicate evidence:
    "mock" — not independently in TEST predicate lexicon. "mock" appears in testing
      contexts but the TEST predicate fires on "test"/"verify"/"assert". "Mock" alone
      is test infrastructure vocabulary without a test action verb.
    "database layer" — not in any predicate
    "repository" — not in any predicate (IMPLEMENT might fire in "implement a repository"
      but "the repository" is a noun phrase here, not an action)
    Result: zero predicates → knowledge_edge

  Boundary note: "mock" is gap vocabulary in isolation. TEST fires when "mock" co-occurs
  with "test" (e.g., "write tests that mock the database"). "Mock" alone — zero predicates.
