# Layer 0-B — Router Pairs (030)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: Algolia search connector routing

---

## B17-0B-030-001 — AlgoliaConnector routing table

| Input pattern | Predicates | SubPhase(s) | TransitionType | DecisionCount |
|---|---|---|---|---|
| Build Algolia connector with index sync webhook | [IMPL, API] | 4.3, 3.3 | feature_addition | 2 |
| Integrate Algolia search API | [API] | 3.3 | documentation | 1 |
| Build Algolia connector with admin token | [IMPL, API, SEC] | 4.3, 3.3, 3.4 | feature_addition | 3 |
| Implement Algolia faceted search (no connector) | [IMPL] | 4.1 | feature_addition | 1 |
| Build Algolia connector, write tests, document | [IMPL, API, TEST, DOC] | 4.3, 3.3, 5.1, 2.1 | feature_addition | 4 |

---

## B17-0B-030-002 — Algolia "implement" without integration vocab

Input: "Implement Algolia faceted search for the product catalogue."
- "Implement" → IMPLEMENT
- No integration vocabulary: no connector, webhook, API, endpoint, external service, SDK, gateway
- "Algolia" is a proper noun — transparent, no integration vocabulary
- "faceted search" and "product catalogue" are domain phrases

IMPLEMENT routes to 4.1 (BackendServiceFlow41)
DecisionCount: 1 | UniqueSubPhases: 1 (4.1)

---

## B17-0B-030-003 — Algolia nearest wrong answers

| Input | Nearest wrong | Correct |
|---|---|---|
| "Integrate Algolia search API" | IMPLEMENT invented | [API_CONTRACT] only → 3.3 |
| "Implement Algolia faceted search" | IMPLEMENT → 4.3 | IMPLEMENT → 4.1 (no integration vocab) |
| "Build Algolia connector with admin token" | SECURITY suppressed | SECURITY fires on "token" unconditionally |
| "Build Algolia search connector" | IMPLEMENT → 4.1 | IMPLEMENT → 4.3 ("connector" present) |
