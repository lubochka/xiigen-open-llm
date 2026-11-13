# Cross-Layer Synthesis (006)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## CLS-006 — Embedded TEST fires: "Guide through writing unit tests"

**Request**: "Guide the junior developers through writing unit tests for the order management module."

### Layer 0-A (Compiler)
Tokens: guide → guidance vocabulary (gap candidate); through → preposition (no predicate); writing → embedded action verb; unit tests → test noun; order management module → software module.
Embedded verb analysis: "writing unit tests" = write + unit + module → TEST fires.
Predicate set = ["TEST"]
CapabilityGapWarning: gap-skills-guidance-mentor-001 (guide = guidance vocabulary)

### Layer 0-B (Router)
Predicates = ["TEST"] → UnitTestingFlow51
Gap warning appended; GapHandler NOT invoked.

### Layer 0-C (Knowledge Edge)
Embedded verb rule: guidance frame ("guide through") does not suppress embedded code action verb ("writing tests"). The write/create/assert + test noun combination fires TEST regardless of outer guidance framing.
Critical distinction: "mentor on testing" → gap (no write verb); "guide through writing tests" → TEST fires (embedded write verb).

### Layer 0-D (Flow Factory)
Predicate ["TEST"] → UnitTestingFlow51 selected.
TransitionType: documentation.
CapabilityGapWarning: gap-skills-guidance-mentor-001.

### Layer 1 (Transition Orchestrator)
UnitTestingFlow51 executes.
Gap warning appended to orchestration result.
SCHEDULE_CAPABILITY_LEARNING NOT emitted (predicate set non-empty).

### Layer 2 (Flow Execution)
UnitTestingFlow51: order management module scoped — write unit tests for order creation, update, cancellation logic.
Verdict: TEST_PASS

### Layer 3 (Teaching)
Key rule: Embedded verb rule — guidance frames do NOT suppress embedded code-action verbs. "guide through writing tests" embeds "writing tests" which fires TEST. The guidance vocabulary ("guide") becomes a CapabilityGapWarning only; GapHandler is suppressed because TEST fires.

**TransitionType**: documentation | **Gap warning**: gap-skills-guidance-mentor-001 | **SCHEDULE_CAPABILITY_LEARNING**: NOT emitted
