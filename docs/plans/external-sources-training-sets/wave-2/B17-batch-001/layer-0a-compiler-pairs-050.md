# Layer 0-A — Compiler Pairs (050)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: AWS Cognito connector

---

## B17-0A-050-001 — Cognito: predicate firing

**Input:** "Build the AWS Cognito identity connector with OAuth 2.0 token exchange and user pool integration."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| OAuth | AUTHORIZATION | 7 |
| token | SECURITY | 6 |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
TransitionType: feature_addition
Note: user pool is a domain noun — fires no predicate.
Integration vocabulary present: connector → IMPLEMENT routes to 4.3

---

## B17-0A-050-002 — Cognito: "integrate" alone

**Input:** "Integrate the AWS Cognito User Pools API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent — cloud identity domain has no exception to Rule 1.

---

## B17-0A-050-003 — Cognito: SAML does not fire AUTHORIZATION

**Input:** "Implement the Cognito connector with SAML (Security Assertion Markup Language) federation."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |

Note: SAML (Security Assertion Markup Language) does NOT fire AUTHORIZATION. Only "OAuth" fires AUTHORIZATION.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
DecisionCount: 2 | UniqueSubPhases: 2

---

## B17-0A-050-004 — Cognito: ID token compound

**Input:** "Build the Cognito connector and validate the ID token for each authenticated request."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6) |

Note: "ID token" contains "token" — SECURITY fires unconditionally.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-050-005 — Cognito: "authentication" also fires SECURITY

**Input:** "Implement the Cognito connector with multi-factor authentication."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| authentication | SECURITY (Rule 8) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3
