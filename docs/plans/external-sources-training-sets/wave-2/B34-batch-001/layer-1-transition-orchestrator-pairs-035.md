# Layer 1 — Transition Orchestrator Pairs (035)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-035-001 — "consult + implement + test + document" full chain orchestration

A: "Consult with the engineering guild, implement the rate limiting service, write integration tests, and document the configuration."
Orchestration: CapabilityGapWarning: gap-skills-guidance-mentor-001 → ImplementationFlow41 → UnitTestingFlow51 ‖ DocumentationFlow
TransitionType: feature_addition. All four flows execute; consult = gap warning only.

---

## TO-035-002 — "sanity check + integrate + test" orchestration

A: "Sanity check the webhook design and integrate the event dispatch endpoint and write the webhook contract tests."
Orchestration: CapabilityGapWarning: gap-skills-guidance-mentor-001 → APIContractFlow33 → UnitTestingFlow51
TransitionType: documentation. Sanity check = gap warning; integrate → API_CONTRACT; write tests → TEST.

---

## TO-035-003 — "rubber duck + architect + implement + QA" orchestration

A: "Rubber duck the design, architect the processing pipeline, implement the pipeline stages, and QA the release."
Orchestration: CapabilityGapWarning: gap-skills-guidance-mentor-001 → ArchitectureFlow31 → ImplementationFlow41 → QAGatingFlow73
TransitionType: feature_addition. Rubber duck = gap warning; QA appended last.

---

## TO-035-004 — "present + integrate + implement + secure" orchestration

A: "Present the plan, integrate the identity provider, implement the profile service, and harden the token storage."
Orchestration: CapabilityGapWarning: gap-skills-guidance-train-001 → APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow
TransitionType: feature_addition. Present = gap warning; API first, then IMPLEMENT, SECURITY post-validates.

---

## TO-035-005 — complete B34 orchestration reference (Round 3 final)

Orchestration algorithm steps:
1. Record all guidance vocabulary as CapabilityGapWarning candidates.
2. Execute code flows in priority order (ARCHITECTURE → API_CONTRACT → IMPLEMENT → SECURITY → AUTHORIZATION → TEST → DOCUMENT → QA).
3. After step 1–2: if predicate set = [] → GapHandler only, emit SCHEDULE_CAPABILITY_LEARNING.
4. If predicate set ≠ [] → execute code flows, append single CapabilityGapWarning to first result.
5. Return TransitionType from code predicates only (guidance vocabulary = no contribution).
