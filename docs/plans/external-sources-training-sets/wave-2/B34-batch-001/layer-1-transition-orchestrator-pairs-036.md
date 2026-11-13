# Layer 1 — Transition Orchestrator Pairs (036)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-036-001 — orchestration for "show how to" stacked with all code verbs

A: "Show how the system works, then implement the service, integrate the dependency, architect the topology, and write tests."
Orchestration: CapabilityGapWarning: gap-skills-guidance-train-001 → ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 → UnitTestingFlow51
TransitionType: feature_addition. "Show how" = gap warning; four code predicates fire in priority order.

---

## TO-036-002 — orchestration for advisory vocabulary + full delivery cycle

A: "Consult, second opinion, rubber duck — implement the solution, test it, document it, and QA the release."
Orchestration: CapabilityGapWarning: gap-skills-guidance-mentor-001 → ImplementationFlow41 → UnitTestingFlow51 ‖ DocumentationFlow → QAGatingFlow73
TransitionType: feature_addition. Three advisory verbs → single gap warning. Four code flows execute in order.

---

## TO-036-003 — orchestration for guidance purpose clause + full delivery cycle

A: "To help the team understand our patterns, implement the reference service, write tests, document the API (Application Programming Interface), and present the results."
Orchestration: CapabilityGapWarning: gap-skills-guidance-train-001 (from "help team understand" + "present") → ImplementationFlow41 → UnitTestingFlow51 ‖ DocumentationFlow
TransitionType: feature_addition. Guidance verbs at start and end = single consolidated gap warning. Present at end = additional gap warning token (merged into single warning).

---

## TO-036-004 — orchestration for "explain + build" with security context

A: "Explain the threat model to the team, implement the hardened authentication layer, and secure the session storage."
Orchestration: CapabilityGapWarning: gap-skills-guidance-train-001 → ImplementationFlow41 → SecurityAuditFlow
TransitionType: feature_addition. Explain = gap warning; IMPLEMENT + SECURITY both fire; SECURITY post-validates IMPLEMENT.

---

## TO-036-005 — B34 complete orchestration summary (all rounds)

B34 orchestration invariants (final):
1. GapHandler ↔ code flows: mutually exclusive. Never both.
2. Guidance vocabulary = gap warning OR gap (never code predicate).
3. Gap warning = single emission regardless of guidance verb count.
4. SCHEDULE_CAPABILITY_LEARNING = single emission on full gap (predicate set = []) only.
5. TransitionType = from code predicates only. Gap presence = no contribution.
6. DomainId = software-development. Always. Not derived from topic.
7. Ordering = ARCHITECTURE → API_CONTRACT → IMPLEMENT → SECURITY → AUTHORIZATION → TEST → DOCUMENT → QA.
8. Guidance vocabulary position (start/middle/end of request) = no effect on orchestration outcome.
