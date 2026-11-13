# Layer 1 — Transition Orchestrator Pairs (010)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-010-001 — Deduplication in guidance context

Request: "Build the skill tracker, implement the badge system, create the leaderboard."
Predicates: ["IMPLEMENT"] (three IMPLEMENT triggers → one predicate by deduplication)
Orchestration: ImplementationFlow41 (single flow)
TransitionType: feature_addition

---

## TO-010-002 — DOCUMENT deduplication

Request: "Write the onboarding guide, document the coding standards, spec out the API conventions."
Predicates: ["DOCUMENT"] (three DOCUMENT triggers → one predicate)
Orchestration: DocumentationFlow (single flow)
TransitionType: documentation

---

## TO-010-003 — TEST routing: 5.1 vs 5.3 in guidance context

"Write unit tests for the scoring algorithm" → TEST → 5.1 (unit + algorithm = pure function)
"Write E2E tests for the portal registration" → TEST → 5.3 (E2E + user flow)
"Write integration tests for the LMS API" → TEST → 5.3 (integration)

Rule unchanged in B34 context: unit + pure function → 5.1; E2E/integration/user flow → 5.3.

---

## TO-010-004 — Gap: "knowledge sharing" → gap

Request: "Organize knowledge sharing sessions for the engineering guild."
Predicates: [] | GapHandler → gap-skills-guidance-mentor-001

---

## TO-010-005 — IMPLEMENT+SECURITY+QA: security before QA

Request: "Build the training portal login system, secure the session management, and QA the auth flow."
Predicates: ["IMPLEMENT", "SECURITY", "QA"]
Orchestration: ImplementationFlow41 → SecurityAuditFlow → QAGatingFlow73
TransitionType: feature_addition | SECURITY audits IMPLEMENT output; QA validates both.
