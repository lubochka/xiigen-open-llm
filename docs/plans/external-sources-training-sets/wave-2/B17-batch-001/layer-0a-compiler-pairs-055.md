# Layer 0-A — Compiler Pairs (055)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Cloudflare connector

---

## B17-0A-055-001 — Cloudflare: predicate firing

**Input:** "Build the Cloudflare Workers connector with webhook event routing and edge request handling."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| webhook | API_CONTRACT | 5 (reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Integration vocabulary present: connector, webhook → IMPLEMENT routes to 4.3

---

## B17-0A-055-002 — Cloudflare: API token fires SECURITY

**Input:** "Implement the Cloudflare connector with an API token scoped to DNS (Domain Name System) zone management."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6) |

Note: DNS (Domain Name System) is a domain acronym — fires no predicate.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-055-003 — Cloudflare: "integrate" alone

**Input:** "Integrate the Cloudflare API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent.

---

## B17-0A-055-004 — Cloudflare: HMAC-SHA256 validation fires SECURITY

**Input:** "Build the Cloudflare Webhook connector and validate the HMAC-SHA256 signature on each incoming event."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| webhook | API_CONTRACT (reinforces; fires once) |
| validate HMAC-SHA256 signature | SECURITY (Rule 8) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-055-005 — Cloudflare: OAuth + token + document

**Input:** "Build the Cloudflare connector with OAuth access token and document the edge API contract."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |
| document | DOCUMENT |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "DOCUMENT"]
DecisionCount: 5 | UniqueSubPhases: 4 (4.3, 3.3, 3.4, 2.1)
