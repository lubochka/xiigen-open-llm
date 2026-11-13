# Layer 0-B — Router Pairs (032)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: Mixpanel product analytics connector routing

---

## B17-0B-032-001 — MixpanelConnector routing table

| Input pattern | Predicates | SubPhase(s) | TransitionType | DecisionCount |
|---|---|---|---|---|
| Build Mixpanel connector with event endpoint | [IMPL, API] | 4.3, 3.3 | feature_addition | 2 |
| Integrate Mixpanel event tracking | [API] | 3.3 | documentation | 1 |
| Connect Mixpanel external service with token | [API, SEC] | 3.3, 3.4 | documentation | 2 |
| Build Mixpanel connector, document event schema | [IMPL, API, DOC] | 4.3, 3.3, 2.1 | feature_addition | 3 |
| Build Mixpanel connector with OAuth token | [IMPL, API, SEC, AUTH] | 4.3, 3.3, 3.4 | feature_addition | 4 |

---

## B17-0B-032-002 — "connect external service" routing (no build verb)

Input: "Connect the Mixpanel external service using project token authentication."
- "connect + external service" → API_CONTRACT (Rule 22)
- No build verb → IMPLEMENT absent
- "token" → SECURITY (Rule 6)
- "authentication" → SECURITY (reinforces; fires once)

DecisionCount: 2 | UniqueSubPhases: 2 (3.3, 3.4) | TransitionType: documentation
"connect" without a build verb: API_CONTRACT only. IMPLEMENT requires a build verb.

---

## B17-0B-032-003 — Mixpanel nearest wrong answers

| Input | Nearest wrong | Correct |
|---|---|---|
| "Integrate Mixpanel event tracking" | IMPLEMENT invented | [API_CONTRACT] → 3.3 |
| "Connect Mixpanel service with token" | IMPLEMENT invented (from "connect") | [API, SEC] → 3.3 + 3.4 (no build verb) |
| "Build Mixpanel analytics pipeline" (no connector) | IMPLEMENT → 4.3 | IMPLEMENT → 4.1 (no integration vocab) |
