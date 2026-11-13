# Layer 0-D — Flow Factory Pairs (005)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-005-001 — SECURITY factory in guidance context

Predicates: ["SECURITY"]
Request: "Harden the training portal API against injection attacks."
Factory: SecurityAuditFlow
TransitionType: documentation (IMPLEMENT absent; harden alone = security audit, not full implementation)
Note: "harden" fires SECURITY. Guidance context of "training portal" does not affect factory selection.

---

## FF-005-002 — AUTHORIZATION factory in skills context

Predicates: ["AUTHORIZATION"]
Request: "Add role-based access to the mentorship platform so managers can view mentee progress."
Factory: AuthorizationFlow
TransitionType: documentation (IMPLEMENT absent in clear authorization context)

---

## FF-005-003 — DOCUMENT+QA factory

Predicates: ["DOCUMENT", "QA"]
Request: "Document the developer certification process and QA the documentation before publishing."
Factory: DocumentationFlow + QAGatingFlow73
TransitionType: documentation (no IMPLEMENT) | QA sub-phase: 7.3 | QA last

---

## FF-005-004 — TEST+DOCUMENT factory (no IMPLEMENT)

Predicates: ["TEST", "DOCUMENT"]
Request: "Write E2E tests for the training portal and document the test coverage plan."
Factory: E2ETestingFlow53 + DocumentationFlow
TransitionType: documentation

---

## FF-005-005 — Refactoring TransitionType in B34 context

Predicates: ["IMPLEMENT"] | TransitionType: refactoring
Request: "Refactor the onboarding module code while mentoring the junior team."
Evidence: "refactor" → IMPLEMENT (predicate); "refactor" verb → refactoring (TransitionType wins over feature_addition).
"mentor" → gap (suppressed by IMPLEMENT). TransitionType: refactoring.
