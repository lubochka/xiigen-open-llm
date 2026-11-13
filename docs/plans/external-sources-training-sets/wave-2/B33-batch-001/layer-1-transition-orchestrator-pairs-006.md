# Layer 1 — Transition Orchestrator Pairs (006)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-006-001 — QA+IMPLEMENT ordering constraint

Request: "Build the agent intent classifier and QA the feature before release."
Predicates: ["IMPLEMENT", "QA"]
Orchestration: ImplementationFlow41 → QAGatingFlow73 (QA always last)
TransitionType: feature_addition (IMPLEMENT present)
QA sub-phase: 7.3 (feature acceptance gate — "before release" of a feature, not staging deployment)

---

## TO-006-002 — IMPLEMENT+TEST+QA three-flow sequence

Request: "Build the agent memory store, write integration tests, and QA sign-off."
Predicates: ["IMPLEMENT", "TEST", "QA"]
Orchestration: ImplementationFlow41 → TestingFlow51 → QAGatingFlow73
TransitionType: feature_addition (IMPLEMENT present)
Note: TEST waits on IMPLEMENT; QA waits on both IMPLEMENT and TEST. Three-stage sequential pipeline.

---

## TO-006-003 — Gap inside multi-verb request

Request: "Adapt the agent for finance domain and document the integration contract."
Analysis: "adapt" → gap; "document" → DOCUMENT; "integration contract" — no "integrate" verb, so API_CONTRACT does NOT fire.
Predicates: ["DOCUMENT"]
Orchestration: DocumentationFlow (single flow; GapHandler NOT invoked — at least one predicate fired)
TransitionType: documentation
Key rule: A single firing predicate suppresses GapHandler entirely.

---

## TO-006-004 — SECURITY routing sub-phase in AI context

Request: "Audit the agent's data access controls and add role-based authorization."
Predicates: ["SECURITY", "AUTHORIZATION"]
Orchestration: SecurityAuditFlow → AuthorizationFlow (sequential; AUTHORIZATION post-validates access model)
TransitionType: documentation (no IMPLEMENT; "add" in security/authorization context without implementation intent)
Note: Context-dependent; if "add" means software implementation → IMPLEMENT present → feature_addition.

---

## TO-006-005 — API_CONTRACT+IMPLEMENT combined flow

Request: "Integrate the agent with Slack and build the message handler."
Predicates: ["API_CONTRACT", "IMPLEMENT"]
Orchestration: APIContractFlow33 ‖ ImplementationFlow41 (parallel; contract spec and implementation may proceed concurrently)
TransitionType: feature_addition (IMPLEMENT present)
Note: "integrate" fires API_CONTRACT; "build" fires IMPLEMENT. No dependency between flows at planning stage.
