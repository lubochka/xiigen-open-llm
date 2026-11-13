# Layer 1 — Transition Orchestrator Pairs (031)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Segment analytics connector orchestration

---

## B17-L1-031-001 — SegmentAnalyticsConnector orchestration: [IMPL, API]

```
Step 1: IntegrationConnectorFlow43 (4.3) — build SegmentAnalyticsConnector [parallel with Step 2]
Step 2: APIContractFlow33 (3.3) — finalize Segment event API contract [parallel with Step 1]
TransitionType: feature_addition
```

---

## B17-L1-031-002 — Segment orchestration: refactor [IMPL, API, TEST]

```
Step 1: IntegrationConnectorFlow43 (4.3) — refactor SegmentAnalyticsConnector
Step 2: APIContractFlow33 (3.3) — update Segment API contract [after Step 1]
Step 3: TestingFlow51 (5.1) — unit tests for track and identify methods [after Step 1; hard dependency]
TransitionType: refactoring
```

TransitionType=refactoring because "refactor" present.
TestingFlow51 still has hard sequential dependency on IntegrationConnectorFlow43 even for refactoring.

---

## B17-L1-031-003 — Segment: DocumentationFlow21 parallelism vs TestingFlow51 dependency

DocumentationFlow21: spec-driven → can run in parallel with IntegrationConnectorFlow43 (Step 1)
TestingFlow51: code-dependent → must run AFTER IntegrationConnectorFlow43 (hard sequential)

This is the fundamental orchestration asymmetry in B17:
- Document: parallel-safe (spec exists before code)
- Test: sequential-required (code must exist before tests)
