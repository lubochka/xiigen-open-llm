# Layer 1 — Transition Orchestrator Pairs (024)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-024-001 — B34 orchestration priority reference (complete)

For guidance vocabulary scenarios in B34, orchestration order follows standard priority:
1. ARCHITECTURE first (ArchitectureFlow31 or 35)
2. API_CONTRACT (APIContractFlow33) — before IMPLEMENT
3. IMPLEMENT (ImplementationFlow41) — feature_addition or refactoring
4. SECURITY (SecurityAuditFlow) — post-validates IMPLEMENT
5. AUTHORIZATION (AuthorizationFlow) — after IMPLEMENT
6. TEST (UnitTestingFlow51 or E2ETestingFlow53) — on implemented code
7. DOCUMENT (DocumentationFlow) — parallel with TEST after IMPLEMENT
8. QA (QAGatingFlow73 or ReleaseGateFlow71) — absolute last

Guidance vocabulary = gap warnings appended to the execution result. Position in request does not affect order.

---

## TO-024-002 — GapHandler invocation conditions in B34

GapHandler invoked when: predicate set = [] after complete scan
GapHandler suppressed when: any code predicate fires (even one)
SCHEDULE_CAPABILITY_LEARNING: emitted once with GapHandler, never without it
Gap warnings: emitted alongside code predicates when guidance vocabulary present
DomainId: software-development (always, for all gap IDs in B34)

---

## TO-024-003 — "teach to X" orchestration exceptions

"Teach to integrate" → GapHandler (integrate blocked by teach-to pattern)
"Teach to build" → GapHandler (build blocked by teach-to pattern)
"Teach to write tests" → GapHandler (write tests blocked by teach-to pattern)
"Teach to QA" → GapHandler (QA blocked by teach-to pattern)

In all "teach to X" cases: GapHandler → gap-skills-guidance-train-001 + SCHEDULE_CAPABILITY_LEARNING.
The infinitive X is the learning content, not the requester's action.
