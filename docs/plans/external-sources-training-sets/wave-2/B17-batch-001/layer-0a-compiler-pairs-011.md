# Layer 0-A — Compiler Pairs (011)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: compiler | focus: v6 corruptions

---

## B17-0A-011-001 — v6-01: "integrate" misclassified as IMPLEMENT

**Input:**
"Integrate the Slack notification API for the team."

**CORRUPTED output (v6-01):**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
(Error: "integrate" was treated as a build verb, firing IMPLEMENT)

**CORRECT output:**
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 3.3
TransitionType: documentation

**Explanation:**
v6-01 is predicate invention — the model invents IMPLEMENT from "integrate". Rule 1 is absolute: "integrate" → API_CONTRACT ONLY. "integrate" is NOT a build verb. IMPLEMENT requires an explicit build verb: build, implement, write, create, code, develop.

**Gate B test:** Input containing ONLY "integrate" + API/connector vocabulary must produce ["API_CONTRACT"] only. Any IMPLEMENT in the output is a v6-01 error.

---

## B17-0A-011-002 — v6-02: "token" SECURITY suppressed

**Input:**
"Build the Auth0 OAuth connector and implement the token refresh flow."

**CORRUPTED output (v6-02):**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"]
(Error: "token" was present but SECURITY was suppressed/not fired)

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
SubPhase: 4.3, 3.3, 3.4, 3.4
TransitionType: feature_addition

**Explanation:**
v6-02 is gap suppression — the model detects "token" but fails to fire SECURITY. Rule 6: "token" always fires SECURITY. Rule 7: "oauth" always fires AUTHORIZATION. Both must fire independently when both vocabulary items are present.

**Gate B test:** Any input containing "token" must produce SECURITY in CompiledPredicateIds.

---

## B17-0A-011-003 — v6-03: IMPLEMENT routes to 4.1 instead of 4.3

**Input:**
"Implement the Stripe payment connector for the checkout service."

**CORRUPTED output (v6-03):**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.1 (IMPLEMENT), 3.3 (API_CONTRACT)
(Error: IMPLEMENT was routed to 4.1 instead of 4.3)

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3 (IMPLEMENT), 3.3 (API_CONTRACT)
TransitionType: feature_addition

**Explanation:**
v6-03 is wrong sub-phase assignment. When IMPLEMENT fires AND integration-context vocabulary is present (connector, integration, webhook, API, external service, third-party), IMPLEMENT routes to 4.3 — not 4.1. Rule 3 is explicit: IMPLEMENT routes to 4.3 when integration vocabulary co-occurs.

**Gate B test:** IMPLEMENT + "connector" vocabulary must produce SubPhase 4.3, never 4.1.
