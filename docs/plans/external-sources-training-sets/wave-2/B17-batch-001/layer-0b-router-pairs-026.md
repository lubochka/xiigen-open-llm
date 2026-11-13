# Layer 0-B — Router Pairs (026)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: SendGrid email connector routing

---

## B17-0B-026-001 — SendGridEmailConnector routing table

| Input pattern | Predicates | SubPhase(s) | TransitionType | DecisionCount |
|---|---|---|---|---|
| Build SendGrid connector with webhook | [IMPL, API] | 4.3, 3.3 | feature_addition | 2 |
| Integrate SendGrid email delivery API | [API] | 3.3 | documentation | 1 |
| Build SendGrid connector with token auth | [IMPL, API, SEC] | 4.3, 3.3, 3.4 | feature_addition | 3 |
| Build SendGrid connector, write tests, document | [IMPL, API, TEST, DOC] | 4.3, 3.3, 5.1, 2.1 | feature_addition | 4 |
| Refactor SendGrid connector to v3 API endpoint | [IMPL, API] | 4.3, 3.3 | refactoring | 2 |

---

## B17-0B-026-002 — SendGrid: routing decision flow

```
Input received
  ↓
"Integrate"? → API_CONTRACT only → 3.3 → STOP
  ↓
Build/create/implement/develop present?
  ↓ Yes
Integration vocabulary present (connector, webhook, endpoint, API)?
  ↓ Yes → IMPLEMENT → 4.3
  ↓ No  → IMPLEMENT → 4.1
  ↓
"token" or "authenticate"? → SECURITY → 3.4
"OAuth"? → AUTHORIZATION → 3.4
"write tests" / "add tests"? → TEST → 5.1
"document"? → DOCUMENT → 2.1
"QA"? → QA → 7.x (last-absolute)
```

---

## B17-0B-026-003 — UniqueSubPhases calculation for SendGrid 4-predicate case

Input: "Build SendGrid connector, write tests, document the email API contract."
Predicates: [IMPLEMENT, API_CONTRACT, TEST, DOCUMENT]
- IMPLEMENT → 4.3
- API_CONTRACT → 3.3
- TEST → 5.1
- DOCUMENT → 2.1

DecisionCount: 4 | UniqueSubPhases: 4 (4.3, 3.3, 5.1, 2.1 — all distinct)
