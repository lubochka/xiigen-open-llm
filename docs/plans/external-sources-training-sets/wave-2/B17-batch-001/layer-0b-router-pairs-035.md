# Layer 0-B — Router Pairs (035)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: GitHub source control connector routing

---

## B17-0B-035-001 — GitHubWebhookConnector routing table

| Input pattern | Predicates | SubPhase(s) | TransitionType | DecisionCount |
|---|---|---|---|---|
| Build GitHub webhook connector with HMAC and token | [IMPL, API, SEC] | 4.3, 3.3, 3.4 | feature_addition | 3 |
| Integrate GitHub source control API | [API] | 3.3 | documentation | 1 |
| Build GitHub connector with OAuth app token | [IMPL, API, SEC, AUTH] | 4.3, 3.3, 3.4 | feature_addition | 4 |
| Wire up GitHub webhook endpoint, add tests | [API, TEST] | 3.3, 5.1 | documentation | 2 |
| Build GitHub connector, document REST API contract | [IMPL, API, DOC] | 4.3, 3.3, 2.1 | feature_addition | 3 |

---

## B17-0B-035-002 — GitHub: "wire up" routing (no build verb)

Input: "Wire up the GitHub webhook endpoint and add tests for push and PR events."
- "Wire up" → API_CONTRACT (Rule 22) — like "integrate", never fires IMPLEMENT
- "webhook" → API_CONTRACT (reinforces; fires once)
- "endpoint" → API_CONTRACT (reinforces; fires once)
- "add tests" → TEST (Rule 13)
- No build verb → IMPLEMENT absent

DecisionCount: 2 | UniqueSubPhases: 2 (3.3, 5.1) | TransitionType: documentation

---

## B17-0B-035-003 — GitHub HMAC: SECURITY trigger from "validate signature"

Input: "Build GitHub webhook connector with HMAC-SHA256 signature validation."
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT (integration vocabulary → 4.3)
- "webhook" → API_CONTRACT (reinforces; fires once)
- "validate signature" / "HMAC validation" → SECURITY (Rule 8)

SECURITY fires on "validate signature" alone — "token" is not required.
DecisionCount: 3 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)
