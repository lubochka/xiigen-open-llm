# Layer 0-A — Compiler Pairs (052)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Supabase connector

---

## B17-0A-052-001 — Supabase: predicate firing

**Input:** "Build the Supabase database connector with REST API access and row-level security."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Note: "row-level security" is a domain phrase — does not fire SECURITY (no token or authenticate trigger).
Integration vocabulary present: connector → IMPLEMENT routes to 4.3

---

## B17-0A-052-002 — Supabase: anon key does not fire SECURITY

**Input:** "Implement the Supabase connector using the anonymous API key."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |

Note: "API key" without "token" does NOT fire SECURITY.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
DecisionCount: 2 | UniqueSubPhases: 2

---

## B17-0A-052-003 — Supabase: JWT fires SECURITY via token compound

**Input:** "Build the Supabase connector with JWT (JSON Web Token) for row-level policy enforcement."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| Token (in JWT = JSON Web Token expansion) | SECURITY (Rule 6) |

Note: JWT (JSON Web Token) — the expansion "JSON Web Token" contains "Token" → SECURITY fires.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-052-004 — Supabase: "integrate" alone

**Input:** "Integrate the Supabase Realtime API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent.

---

## B17-0A-052-005 — Supabase: OAuth + token

**Input:** "Build the Supabase connector with OAuth 2.0 access token for third-party auth providers."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
DecisionCount: 4 | UniqueSubPhases: 3
