# Layer 0-B — Router Pairs (028)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: DocuSign e-signature connector routing

---

## B17-0B-028-001 — DocuSignConnector routing table

| Input pattern | Predicates | SubPhase(s) | TransitionType | DecisionCount |
|---|---|---|---|---|
| Build DocuSign connector with Connect webhook | [IMPL, API] | 4.3, 3.3 | feature_addition | 2 |
| Integrate DocuSign envelope creation | [API] | 3.3 | documentation | 1 |
| Build DocuSign connector, OAuth token flow | [IMPL, API, SEC, AUTH] | 4.3, 3.3, 3.4 | feature_addition | 4 |
| Refactor DocuSign connector to v2.1 REST endpoint | [IMPL, API] | 4.3, 3.3 | refactoring | 2 |
| Build DocuSign connector, write tests, document | [IMPL, API, TEST, DOC] | 4.3, 3.3, 5.1, 2.1 | feature_addition | 4 |

---

## B17-0B-028-002 — DocuSign refactor routing

Input: "Refactor the DocuSign connector to use the v2.1 eSignature REST API endpoint."
- "Refactor" → IMPLEMENT (Rule 14; TransitionType=refactoring)
- "connector" → API_CONTRACT
- "endpoint" → API_CONTRACT (reinforces; fires once)
- Integration vocabulary: connector → IMPLEMENT routes to 4.3

DecisionCount: 2 | UniqueSubPhases: 2 (4.3, 3.3) | TransitionType: refactoring
TransitionType is refactoring (not feature_addition) because "refactor" is present.

---

## B17-0B-028-003 — DocuSign 4-predicate auth routing

Input: "Build DocuSign connector with OAuth authorization code flow and access token refresh."
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT
- "OAuth" → AUTHORIZATION (Rule 7)
- "token" → SECURITY (Rule 6; fires on "access token refresh")
- Integration vocabulary: connector → IMPLEMENT routes to 4.3

DecisionCount: 4 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)
SECURITY and AUTHORIZATION both route to 3.4 — this reduces UniqueSubPhases by 1.
