# Layer 1 — Transition Orchestrator Pairs (055)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B01-L1-055-001 — SubPhaseId selection rule: primary predicate hierarchy

```
source_id: xiigen-b01-l1-055-r3-rule
source_license_state: first_party
pair_kind: transition_orchestrator
```

SubPhaseId selection rule for B01-batch-001 (confirmed from files 001-054):

When multiple predicates fire, SubPhaseId is assigned as follows:
  UI present → SubPhaseId: "1.1" (UI is always primary for B01 screen inventory scenario)
  UI absent, IMPLEMENT present → SubPhaseId: "4.1"
  UI absent, IMPLEMENT absent, ARCHITECTURE present → SubPhaseId: "3.1"
  UI absent, IMPLEMENT absent, ARCHITECTURE absent, DOCUMENT present → SubPhaseId: "2.1"

This hierarchy: UI > IMPLEMENT > ARCHITECTURE > DOCUMENT.

FlowsSelected contains ALL selected flows regardless of primary SubPhaseId.
SubPhaseId indicates the primary/leading flow only.

---

## B01-L1-055-002 — TransitionType determination rule

```
source_id: xiigen-b01-l1-055-r3-type
source_license_state: first_party
pair_kind: transition_orchestrator
```

TransitionType determination rule (confirmed from files 001-054):
  IMPLEMENT in CompiledPredicateIds → TransitionType: "feature_addition"
  IMPLEMENT absent → TransitionType: "documentation"

This applies regardless of how many other predicates are present:
  [UI] → documentation
  [UI, DOCUMENT] → documentation
  [ARCHITECTURE] → documentation
  [UI, IMPLEMENT] → feature_addition
  [IMPLEMENT, ARCHITECTURE] → feature_addition
  [DOCUMENT, IMPLEMENT] → feature_addition (write user stories)
  [UI, DOCUMENT, IMPLEMENT, ARCHITECTURE] → feature_addition

IMPLEMENT is the sole determinant of TransitionType.
