# Layer 0-A — Compiler Pairs (049)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Auth0 connector

---

## B17-0A-049-001 — Auth0: predicate firing

**Input:** "Build the Auth0 identity connector with OAuth 2.0 token validation and PKCE (Proof Key for Code Exchange) flow."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| OAuth | AUTHORIZATION | 7 |
| token | SECURITY | 6 |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
TransitionType: feature_addition
Note: PKCE (Proof Key for Code Exchange) is a domain acronym — fires no predicate.
Integration vocabulary present: connector → IMPLEMENT routes to 4.3

---

## B17-0A-049-002 — Auth0: "integrate" alone

**Input:** "Integrate the Auth0 Management API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent — identity domain has no exception to Rule 1.

---

## B17-0A-049-003 — Auth0: OIDC does not fire AUTHORIZATION

**Input:** "Implement the Auth0 connector with OIDC (OpenID Connect) discovery endpoint."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |

Note: OIDC (OpenID Connect) does NOT fire AUTHORIZATION — only the word "OAuth" in any form fires AUTHORIZATION (Rule 7).
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
DecisionCount: 2 | UniqueSubPhases: 2

---

## B17-0A-049-004 — Auth0: access token fires SECURITY

**Input:** "Implement the Auth0 connector with access token introspection."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-049-005 — Auth0: full pipeline with QA

**Input:** "Build the Auth0 connector with OAuth token refresh, write tests, document the API, and run QA."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |
| write tests | TEST |
| document | DOCUMENT |
| QA | QA (Rule 21 — abbreviation only) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT", "QA"]
DecisionCount: 7 | UniqueSubPhases: 6
QAGatingFlow73 is last-absolute.
