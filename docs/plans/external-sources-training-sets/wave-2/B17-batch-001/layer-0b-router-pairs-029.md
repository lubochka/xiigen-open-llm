# Layer 0-B — Router Pairs (029)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: Cloudinary media connector routing

---

## B17-0B-029-001 — CloudinaryConnector routing table

| Input pattern | Predicates | SubPhase(s) | TransitionType | DecisionCount |
|---|---|---|---|---|
| Build Cloudinary connector with webhook callbacks | [IMPL, API] | 4.3, 3.3 | feature_addition | 2 |
| Integrate Cloudinary into image pipeline | [API] | 3.3 | documentation | 1 |
| Build Cloudinary SDK wrapper with token | [IMPL, API, SEC] | 4.3, 3.3, 3.4 | feature_addition | 3 |
| Implement Cloudinary image resizing service | [IMPL] | 4.1 | feature_addition | 1 |
| Build Cloudinary connector, write tests, QA pipeline | [IMPL, API, TEST, QA] | 4.3, 3.3, 5.1, 7.x | feature_addition | 4 |

---

## B17-0B-029-002 — Cloudinary SDK routing

Input: "Build Cloudinary SDK wrapper with API secret token validation."
- "Build" → IMPLEMENT
- "SDK" → integration vocabulary (reinforcer) → IMPLEMENT routes to 4.3
- "SDK" also → API_CONTRACT (reinforcer for Rule 4 family; integration vocab)
- "token" → SECURITY (Rule 6)

Note: SDK is in the integration vocabulary list. It shifts IMPLEMENT from 4.1 to 4.3.
DecisionCount: 3 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)

---

## B17-0B-029-003 — Cloudinary QA routing

Input: "Build Cloudinary connector, write upload tests, QA the delivery pipeline."
- Predicates: [IMPLEMENT, API_CONTRACT, TEST, QA]
- Routing: 4.3, 3.3, 5.1, 7.x
- QAGatingFlow73: position = last-absolute

DecisionCount: 4 | UniqueSubPhases: 4 (4.3, 3.3, 5.1, 7.x — all distinct)
QA must run after all other flows complete. It cannot run in parallel.
