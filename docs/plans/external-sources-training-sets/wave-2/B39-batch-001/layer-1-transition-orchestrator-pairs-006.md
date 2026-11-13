# Layer 1 — Transition Orchestrator Pairs (006)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-006-001 — IMPLEMENT+TEST: IMPLEMENT before TEST (dependency)

A: ["IMPLEMENT", "TEST"] | "Build the date picker and write tests"
Orchestration: ImplementationFlow41 → E2ETestingFlow53 (tests require the feature to exist)
Rule: IMPLEMENT executes before TEST. Tests verify implementation. Reversed order is invalid.

---

## TO-006-002 — TEST+UI: no dependency (can be parallel)

A: ["TEST", "UI"] | "Write tests for existing button states and fix button CSS"
Orchestration: E2ETestingFlow53 || UIInteractionFlow15 (can execute in parallel)
Rule: TEST and UI have no dependency on each other when no IMPLEMENT is present.

---

## TO-006-003 — IMPLEMENT+UI: may be parallel (UI work not always dependent on IMPLEMENT)

A: ["IMPLEMENT", "UI"] | "Build the file upload backend and fix the upload button styling"
Orchestration: ImplementationFlow41 and UIInteractionFlow15 may proceed in parallel (different concerns)
Note: When UI depends on implementation (e.g., styling a new component), IMPLEMENT first. When independent (e.g., fixing existing styling), parallel.

---

## TO-006-004 — DOCUMENT after IMPLEMENT+TEST

A: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"]
Orchestration: IMPLEMENT → TEST → DOCUMENT → QA
Rule: Documentation is typically after implementation and testing. QA last.

---

## TO-006-005 — TransitionType: documentation vs feature_addition in TEST-only requests

A: ["TEST"] | "Write unit tests for the color contrast checker" → documentation
A: ["IMPLEMENT", "TEST"] | "Build and test the color contrast checker" → feature_addition
Rule: TEST alone without IMPLEMENT → documentation. IMPLEMENT present → feature_addition.
