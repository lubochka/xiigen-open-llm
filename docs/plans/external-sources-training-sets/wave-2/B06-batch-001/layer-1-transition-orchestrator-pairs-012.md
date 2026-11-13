# Layer 1 — Transition Orchestrator Pairs (012)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 012 covers v6 ordering violations for B06 at the transition orchestrator level.

---

## B06-T1-012-001 — v6 ordering violation: implementation before requirements

```
source_id: xiigen-b06-t1-012-001
source_title: XIIGen B06 T1 — v6 ordering violation impl before requirements
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"
    - PredictedSubPhaseId: "4.1"

B-correct:
  Transitions:
    [ordered: 2.1 first, 4.1 second]
    - SubPhaseId: "2.1", TransitionType: "feature_addition"
    - SubPhaseId: "4.1", TransitionType: "feature_addition"

B-corrupt (ordering violation):
  Transitions:
    [reversed: 4.1 first, 2.1 second]
    - SubPhaseId: "4.1", TransitionType: "feature_addition"
    - SubPhaseId: "2.1", TransitionType: "feature_addition"

Corruption: Ordering violation — implementation transition produced before requirements.
Why harmful: The ImplementationFlow41 executes before RequirementsAuthoringFlow21.
  The checkout service is implemented before requirements are authored. The
  implementation has no requirements to validate against. The arbiter review
  in N08 cannot check against acceptance criteria that haven't been written.
Fix: Requirements (2.1) must always precede implementation (4.1) in transition
  ordering when both are present in the same orchestration run.

---

## B06-T1-012-002 — v6 ordering violation: UI before architecture

```
source_id: xiigen-b06-t1-012-002
source_title: XIIGen B06 T1 — v6 ordering violation UI before architecture
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "UI"]
  RoutingDecisions:
    - PredictedSubPhaseId: "3.1"
    - PredictedSubPhaseId: "1.1"

B-correct transition order:
  1. SubPhaseId: "3.1" (Architecture first)
  2. SubPhaseId: "1.1" (UI after architecture)

B-corrupt order:
  1. SubPhaseId: "1.1" (UI before architecture — wrong)
  2. SubPhaseId: "3.1"

Why harmful: UI design without architecture produces screens disconnected from
  service boundaries and API capabilities. The architecture should define what
  the UI can display before the UI is designed.

---

## B06-T1-012-003 — B06 canonical transition ordering

```
source_id: xiigen-b06-t1-012-003
source_title: XIIGen B06 T1 — canonical transition ordering
source_license_state: first_party
pair_kind: transition_orchestrator
```

Teaching note — canonical transition ordering for B06:

  When multiple transitions are produced, the canonical order is:
    1. 2.1 (Business Requirements) — requirements first
    2. 3.1 (Architecture) — design follows requirements
    3. 2.4 (Stakeholder Requirements) — stakeholder after business requirements
    4. 1.1 (Screen/UX) — UI follows architecture
    5. 4.1 (Implementation) — implementation last

  Rationale:
    Requirements → Architecture → UI → Implementation

  This ordering is the canonical software development lifecycle order:
    1. Define what you need (requirements)
    2. Design how to build it (architecture)
    3. Design how it looks (UI)
    4. Build it (implementation)

  In B06, the primary transition is always 2.1. If 4.1 also fires, it follows 2.1.
  If 1.1 also fires, it follows 3.1 (or 2.1 if no 3.1).
