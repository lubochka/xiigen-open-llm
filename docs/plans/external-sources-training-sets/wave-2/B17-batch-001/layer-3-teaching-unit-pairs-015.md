# Layer 3 — Teaching Unit Pairs (015)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: FedEx/UPS shipping connector

---

## B17-TU-015-001 — Two connectors named in one request — predicates fire once

**A:** "Build the FedEx and UPS shipping connector adapters." How many times does API_CONTRACT fire?

**B:** Once. Predicates are sets. Even though "connector" appears twice (conceptually once for FedEx, once for UPS), API_CONTRACT fires once. The compiled predicate IDs are ["IMPLEMENT", "API_CONTRACT"] — not ["IMPLEMENT", "API_CONTRACT", "API_CONTRACT"].

**Rule:** CompiledPredicateIds is always a deduplicated ordered list. Multiple occurrences of the same trigger yield one predicate.

---

## B17-TU-015-002 — "FedEx" proper noun does not supply integration vocabulary

**A:** "Implement FedEx tracking." Does IMPLEMENT route to 4.3 because FedEx is an external service?

**B:** No. "FedEx" is a proper noun. It is not in the integration vocabulary list (connector, webhook, API, endpoint, external service, third-party, gateway, SDK). IMPLEMENT routes to 4.3 only when integration vocabulary tokens are present.

"Implement FedEx tracking" → IMPLEMENT only → 4.1 (no integration vocabulary)
"Implement the FedEx tracking API" → IMPLEMENT + API_CONTRACT → 4.3 (API is integration vocabulary)
"Implement the FedEx connector" → IMPLEMENT + API_CONTRACT → 4.3 (connector is integration vocabulary)

---

## B17-TU-015-003 — "OAuth" in shipping connector fires AUTHORIZATION (same as payment connectors)

**A:** "Create the FedEx OAuth connector." Does AUTHORIZATION fire?

**B:** Yes. Rule 7 (OAuth → AUTHORIZATION) is domain-independent. It fires in shipping, payment, email, analytics, and all other connector domains. The word "OAuth" (any case) fires AUTHORIZATION.

**Result:** "Create the FedEx OAuth connector, implement the API access token refresh logic."
- "Create" → IMPLEMENT
- "connector" → API_CONTRACT
- "OAuth" → AUTHORIZATION
- "token" → SECURITY
- **CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

---

## B17-TU-015-004 — "refactoring" for shipping connector: TransitionType changes, routing unchanged

**A:** "Refactor the FedEx connector to support FedEx Ship v1 API schema changes." What changes vs "Build the FedEx connector"?

**B:** Only TransitionType changes.

| Property | "Build the FedEx connector" | "Refactor the FedEx connector" |
|---|---|---|
| CompiledPredicateIds | ["IMPLEMENT", "API_CONTRACT"] | ["IMPLEMENT", "API_CONTRACT"] |
| SubPhase | 4.3, 3.3 | 4.3, 3.3 |
| TransitionType | feature_addition | refactoring |
| Nine-node flow | IntegrationConnectorFlow43 | IntegrationConnectorFlow43 |
| N08 additional check | None | Backward compatibility |

Same predicate set, same routing, same flow. TransitionType is the only change.
