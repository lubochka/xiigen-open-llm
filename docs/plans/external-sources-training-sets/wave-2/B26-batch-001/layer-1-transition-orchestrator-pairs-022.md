# Layer 1 — Transition Orchestrator Pairs (022)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T022-001 — SECURITY+QA: feature_addition (RBAC + permission matrix QA)
Request: "Audit RBAC configuration for AdminPanel endpoints and QA the permission matrix."
  TransitionType: feature_addition | SECURITY → 6.3 | QA → 7.3

## T022-002 — SECURITY+AUTHORIZATION: feature_addition (privilege escalation review)
Request: "Review the AdminService for privilege escalation — can standard user elevate to admin?"
  TransitionType: feature_addition | SECURITY → 6.3 | AUTHORIZATION → 6.3

## T022-003 — SECURITY+AUTHORIZATION+DOCUMENT: documentation (access control matrix documented)
Request: "Audit access control enforcement across microservices and document unenforced authorization boundaries."
  TransitionType: documentation | SECURITY → 6.3 | AUTHORIZATION → 6.3 | DOCUMENT → 6.2

## T022-004 — Zero predicates: N/A (product feature permissions)
Request: "Allow users to manage their own team permissions from the settings page."
  TransitionType: N/A | GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

## T022-005 — SECURITY+AUTHORIZATION+QA: feature_addition (default-deny + coverage QA)
Request: "Audit AuthorizationMiddleware for default-deny posture and QA unauthenticated access paths."
  TransitionType: feature_addition | SECURITY → 6.3 | AUTHORIZATION → 6.3 | QA → 7.3
