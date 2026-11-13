# Layer 0-B — Router Pairs (027)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: Plaid banking connector routing

---

## B17-0B-027-001 — PlaidBankingConnector routing table

| Input pattern | Predicates | SubPhase(s) | TransitionType | DecisionCount |
|---|---|---|---|---|
| Build Plaid connector with OAuth token exchange | [IMPL, API, SEC, AUTH] | 4.3, 3.3, 3.4 | feature_addition | 4 |
| Integrate Plaid into payment flow | [API] | 3.3 | documentation | 1 |
| Implement Plaid link token creation (no connector) | [IMPL, SEC] | 4.1, 3.4 | feature_addition | 2 |
| Build Plaid API connector, document integration | [IMPL, API, DOC] | 4.3, 3.3, 2.1 | feature_addition | 3 |
| Connect Plaid external service with token | [API, SEC] | 3.3, 3.4 | documentation | 2 |

---

## B17-0B-027-002 — Plaid 4-predicate routing: DecisionCount vs UniqueSubPhases

Input: "Build Plaid banking connector with OAuth token exchange."
- IMPLEMENT → 4.3
- API_CONTRACT → 3.3
- SECURITY → 3.4 (fires on "token")
- AUTHORIZATION → 3.4 (fires on "OAuth")

DecisionCount: 4
UniqueSubPhases: 3 (4.3, 3.3, 3.4 — SECURITY and AUTHORIZATION share 3.4)
This is the canonical 4-predicate, 3-sub-phase pattern for identity/auth connectors.

---

## B17-0B-027-003 — Plaid "implement" without integration vocab routing

Input: "Implement Plaid link token creation."
- "Implement" fires IMPLEMENT
- No connector, webhook, API, endpoint, external service, gateway, SDK in input
- "link token creation" is a domain phrase; no integration vocabulary
- IMPLEMENT → 4.1 (BackendServiceFlow41)
- "token" → SECURITY → 3.4

DecisionCount: 2 | UniqueSubPhases: 2 (4.1, 3.4)
Key: "Plaid" proper noun does NOT provide integration vocabulary.
