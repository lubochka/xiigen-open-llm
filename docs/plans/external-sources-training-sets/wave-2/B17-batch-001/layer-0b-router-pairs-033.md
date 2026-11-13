# Layer 0-B — Router Pairs (033)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: Zendesk support connector routing

---

## B17-0B-033-001 — ZendeskConnector routing table

| Input pattern | Predicates | SubPhase(s) | TransitionType | DecisionCount |
|---|---|---|---|---|
| Build Zendesk connector with ticket webhook and token | [IMPL, API, SEC] | 4.3, 3.3, 3.4 | feature_addition | 3 |
| Integrate Zendesk support ticketing API | [API] | 3.3 | documentation | 1 |
| Build Zendesk connector with OAuth 2.0 token | [IMPL, API, SEC, AUTH] | 4.3, 3.3, 3.4 | feature_addition | 4 |
| Build Zendesk connector with "authorization" (English) | [IMPL, API] | 4.3, 3.3 | feature_addition | 2 |
| Build Zendesk connector, write tests | [IMPL, API, TEST] | 4.3, 3.3, 5.1 | feature_addition | 3 |

---

## B17-0B-033-002 — Zendesk "authorization" English word routing

Input: "Build Zendesk connector with proper authorization for support agents."
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT (integration vocabulary → 4.3)
- "authorization" = English word — NO PREDICATE (Rule: only "OAuth" fires AUTHORIZATION)

DecisionCount: 2 | UniqueSubPhases: 2 (4.3, 3.3)
English word "authorization" is the most common v6-01 source for AUTHORIZATION over-invention.

---

## B17-0B-033-003 — Zendesk: SECURITY fires unconditionally on token

Input: "Build Zendesk connector with OAuth 2.0 token for agent authentication."
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT → 4.3
- "OAuth" → AUTHORIZATION → 3.4
- "token" → SECURITY → 3.4 (fires even though "OAuth" also present)
- "authentication" → SECURITY (reinforces; fires once)

SECURITY and AUTHORIZATION are independent. SECURITY fires on "token" regardless of whether OAuth is also present.
DecisionCount: 4 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)
