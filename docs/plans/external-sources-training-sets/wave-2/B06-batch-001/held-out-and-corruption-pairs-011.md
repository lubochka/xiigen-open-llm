# Held-Out and Corruption Pairs (011)
# Batch: B06-batch-001
# Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: held_out | corruption

---

## Method note

File 011 covers HO-022 (arbiter review FAIL triggers N09 FAIL), COR-013 (arbiter PASS when it should FAIL, v6-02).

---

## Held-Out Pair B06B1-WDA-HO-022 — arbiter FAIL propagates to N09 FAIL verdict

```
source_id: xiigen-b06-held-out-022
pair_kind: held_out
```

A:
  run_id: "B06-ho-022"
  scenario: "ArbiterFailPropagation"
  sub_phase_id: "2.1"
  node: "N08-N09"

N08 arbiter input:
  ReviewType: "use_case_completeness"
  UseCases:
    - UseCaseId: "UC-001"; MainFlow: present; AlternativeFlows: []; Postconditions: present
    - UseCaseId: "UC-002"; MainFlow: present; AlternativeFlows: present; Postconditions: present
  RequiredAlternativeFlows: "At least one AF per use case that was requested with edge cases"

Expected N08 arbiter output:
  ReviewResult: "FAIL"
  FailedUseCases: ["UC-001"]
  FailureReason: "UC-001 has empty AlternativeFlows; request specified edge cases"
  Recommendation: "Populate AlternativeFlows for UC-001 with all specified failure scenarios"

Expected N09 FormatterUnit output:
  OverallVerdict: "FAIL"
  FinalHumanResult: |
    FAIL — Use case completeness arbiter failed.
    UC-001: AlternativeFlows missing despite requested edge cases.
    Action required: re-run N04/N05 for UC-001 with AF population.
  Warnings:
    - "arbiter_fail: UC-001 AlternativeFlows empty"
    - "review_required: N04/N05 re-run for UC-001"

Generalization test:
  N08 FAIL must propagate to N09 as OverallVerdict: FAIL.
  N09 must not suppress N08 failures. The output must be actionable, not optimistic.

---

## Held-Out Pair B06B1-WDA-HO-023 — knowledge edge: no development signal

```
source_id: xiigen-b06-held-out-023
pair_kind: held_out
```

A:
  run_id: "B06-ho-023"
  user_request: "Can you help me think through the checkout flow?"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: []
  Confidence: 0.70
  Warnings: ["no_predicate_matched: knowledge_edge_result"]

Expected routing decisions:
  - PredictedSubPhaseId: null; KnowledgeEdge: true; FailureClassification: "no_predicate_match"

Generalization test:
  "help me think through" — no lexicon terms.
  "checkout flow" — no lexicon terms ("checkout", "flow" not in any predicate lexicon).
  Conversational intent without development action verbs → knowledge-edge.
  The model must produce no predicate and route to knowledge edge.

---

## Corruption Pair B06B1-WDA-COR-013 — v6-02: arbiter FAIL suppressed, N09 reports PASS

```
source_id: xiigen-b06-corruption-013
pair_kind: corruption
corruption_type: Corruption-v6-02-arbiter-fail-suppressed
```

A:
  DocumentId: "B06-ho-022-N08-output"
  N08_ArbiterReview:
    ReviewResult: "FAIL"
    FailedUseCases: ["UC-001"]
    FailureReason: "AlternativeFlows empty"

B-correct (N09 output):
  OverallVerdict: "FAIL"
  FinalHumanResult: "FAIL — UC-001 AlternativeFlows empty. Re-run required."
  Warnings: ["arbiter_fail: UC-001 AlternativeFlows empty"]

B-corrupt (N09 output):
  OverallVerdict: "PASS"
  FinalHumanResult: |
    Guest checkout use cases complete.
    UC-001: Main flow documented. UC-002: Complete.
    Acceptance criteria: 6 assertions.
  Warnings: []

Corruption type: v6-02 — arbiter FAIL suppressed; N09 FormatterUnit ignores N08 FAIL verdict
and produces an optimistic PASS output.
Why harmful: The incomplete use case (UC-001 missing AlternativeFlows) is marked as complete
and shipped. Downstream teams (QA (Quality Assurance), implementation) work from an incomplete
requirements document without knowing it is incomplete. The payment failure and session timeout
recovery paths are undocumented. The arbiter system's entire purpose is to catch exactly this
gap — suppressing it defeats the mechanism entirely.
The model must propagate N08 FAIL to N09 OverallVerdict: FAIL without exception.
