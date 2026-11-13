# Layer 0-A — Compiler Pairs (054)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Netlify connector

---

## B17-0A-054-001 — Netlify: predicate firing

**Input:** "Build the Netlify Functions connector with webhook trigger handling for form submissions."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| webhook | API_CONTRACT | 5 (reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Integration vocabulary present: connector, webhook → IMPLEMENT routes to 4.3

---

## B17-0A-054-002 — Netlify: access token fires SECURITY

**Input:** "Implement the Netlify connector with personal access token for site management."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-054-003 — Netlify: "integrate" alone

**Input:** "Integrate the Netlify Build API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent.

---

## B17-0A-054-004 — Netlify: OAuth + token

**Input:** "Build the Netlify connector with OAuth 2.0 access token for team-level operations."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
DecisionCount: 4 | UniqueSubPhases: 3

---

## B17-0A-054-005 — Netlify: "publish" is not IMPLEMENT

**Input:** "Publish the static site to Netlify and configure the CDN edge rules."

Triggers: none ("publish" and "configure" are not IMPLEMENT triggers)
CompiledPredicateIds: []
TransitionType: none
