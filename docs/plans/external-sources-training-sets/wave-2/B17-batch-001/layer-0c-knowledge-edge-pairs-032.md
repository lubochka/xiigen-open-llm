# Layer 0-C — Knowledge-Edge Pairs (032)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Mixpanel product analytics connector edge cases

---

## B17-0C-032-001 — Mixpanel: "project token" — token fires SECURITY

"Build Mixpanel connector using project token authentication."
- "project token" contains "token" → SECURITY fires (Rule 6; unconditional)
- "authentication" → SECURITY (reinforces; fires once)
- "connector" → API_CONTRACT (integration vocabulary → 4.3)

"project" before "token" does not create an exception. Rule 6 is unconditional — no modifier suppresses it.

---

## B17-0C-032-002 — Mixpanel: "connect external service" without build verb

Input: "Connect the Mixpanel external service using project token authentication."
- "connect + external service" → API_CONTRACT (Rule 22)
- No build verb → IMPLEMENT absent
- "token" → SECURITY
- "authentication" → SECURITY (reinforces; fires once)

Predicates: [API_CONTRACT, SECURITY]
TransitionType: documentation (API_CONTRACT without IMPLEMENT = documentation)
IMPLEMENT is absent because "connect" alone is not a build verb.

---

## B17-0C-032-003 — Mixpanel: "event tracking" and "user journey" are domain phrases

Domain phrases that fire NO predicate:
- event tracking, user journey, funnel analysis, retention, cohort, product metrics
These are Mixpanel domain vocabulary — transparent to the compiler.

---

## B17-0C-032-004 — Mixpanel: ambiguous "build" position

Input: "Build and connect the Mixpanel external service with OAuth token."
- "Build" → IMPLEMENT (build verb present)
- "connect + external service" → API_CONTRACT (Rule 22)
- "OAuth" → AUTHORIZATION
- "token" → SECURITY
- Integration vocabulary from "external service" → IMPLEMENT routes to 4.3

Predicates: [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION]
DecisionCount: 4 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)
