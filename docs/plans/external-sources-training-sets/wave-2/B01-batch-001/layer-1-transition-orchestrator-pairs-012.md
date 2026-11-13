# Layer 1 — Transition Orchestrator Pairs (012)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B01-L1-012-001 — v6 ordering violations: examples with A/B format

```
source_id: xiigen-b01-l1-012-001
source_title: XIIGen B01 layer1-012 — v6 ordering violations A/B examples
source_license_state: first_party
pair_kind: transition_orchestrator
coverage_status: covered
```

v6 ordering violation example 1 — wrong SubPhaseId for UI:

A:
  CompiledPredicateIds: ["UI"]
  user_request: "Map all screens in the onboarding interface."

B-correct:
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"

B-corrupt (v6 ordering violation):
  TransitionType: "documentation"
  SubPhaseId: "2.1"

Explanation: UI alone → feature_addition + SubPhaseId 1.1. The corrupt output
assigns documentation TransitionType and routes to 2.1 — both wrong.

---

v6 ordering violation example 2 — wrong primary SubPhaseId for [UI, DOCUMENT]:

A:
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  user_request: "Document the checkout interface and produce a page specification."

B-correct:
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"  (UI is primary)

B-corrupt (v6 ordering violation):
  TransitionType: "documentation"  (wrong — UI present makes it feature_addition)
  SubPhaseId: "2.1"  (wrong — UI is primary, 1.1 should be selected)

Explanation: UI presence forces feature_addition TransitionType AND SubPhaseId: 1.1.
The corrupt output uses DOCUMENT as primary, producing wrong TransitionType and
wrong SubPhaseId.

---

## B01-L1-012-002 — TransitionType test: can you distinguish documentation vs feature_addition?

```
source_id: xiigen-b01-l1-012-002
source_title: XIIGen B01 layer1-012 — TransitionType discrimination test
source_license_state: first_party
pair_kind: transition_orchestrator
coverage_status: covered
```

TransitionType discrimination test:

  Test 1: "Design the architecture for the onboarding screen routing system."
  → ARCHITECTURE fires (design + architecture); UI fires (screen)
  → Predicates: ["UI", "ARCHITECTURE"]
  → IMPLEMENT absent → documentation
  → Answer: documentation

  Test 2: "Design and build the architecture for the onboarding screen routing system."
  → ARCHITECTURE fires; UI fires (screen); IMPLEMENT fires (build)
  → Predicates: ["UI", "ARCHITECTURE", "IMPLEMENT"]
  → IMPLEMENT present → feature_addition
  → Answer: feature_addition

  Test 3: "Describe the functional behavior of each page in the admin interface."
  → DOCUMENT fires (describe, functional, behavior); UI fires (page, interface)
  → Predicates: ["UI", "DOCUMENT"]
  → UI present → feature_addition
  → Answer: feature_addition

  Test 4: "Describe the functional behavior of the onboarding module."
  → DOCUMENT fires (describe, functional, behavior)
  → No UI terms present
  → Predicates: ["DOCUMENT"]
  → documentation
  → Answer: documentation

---

## B01-L1-012-003 — Complete B01 transition orchestrator rules summary

```
source_id: xiigen-b01-l1-012-003
source_title: XIIGen B01 layer1-012 — complete transition orchestrator rules summary
source_license_state: first_party
pair_kind: transition_orchestrator
coverage_status: covered
```

Complete B01 transition orchestrator rules:

  Rule 1: TransitionType
    IMPLEMENT present → feature_addition (always)
    UI present (no IMPLEMENT) → feature_addition
    ARCHITECTURE + UI (no IMPLEMENT) → documentation
    ARCHITECTURE alone → documentation
    DOCUMENT alone → documentation
    [] → unknown (knowledge-edge)

  Rule 2: SubPhaseId selection (primary)
    UI → 1.1
    UI absent, ARCHITECTURE → 3.1
    UI + ARCHITECTURE absent, IMPLEMENT → 4.1
    Only DOCUMENT → 2.1

  Rule 3: DomainId
    Always "software-development" in B01

  Rule 4: ContextCarryForward
    Always true in B01

  Rule 5: EM-9 guard
    Node 01 input always SelectedLogicalTransition; never raw string

  Rule 6: TransitionId format
    "transition-[batch]-[run_id]-[SubPhaseId]-[TransitionType]"

  Rule 7: Flow execution order
    1.1 → 3.1 → 2.1 → 4.1 (when multiple predicates)

  Rule 8: Metadata required fields
    transition_classification_basis, primary_intent, domain_signal
    flow_execution_order (when multiple predicates)
