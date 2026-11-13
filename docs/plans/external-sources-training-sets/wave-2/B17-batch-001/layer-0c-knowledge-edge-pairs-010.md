# Layer 0-C — Knowledge-Edge Pairs (010)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: knowledge-edge

---

## B17-0C-010-001 — "wire up" = API_CONTRACT; complete API_CONTRACT vocabulary for B17

**Rule 22:** "wire up" fires API_CONTRACT.

**Full API_CONTRACT vocabulary in B17:**

| Token/phrase | Rule | Notes |
|---|---|---|
| integrate | Rule 1 | Primary signal; never IMPLEMENT |
| connector | Rule 4 | Any compound |
| webhook | Rule 5 | Any compound |
| wire up | Rule 22 | Two-word phrase |
| connect (+ external) | Rule 22 | Conditional on external service context |
| API | Reinforcer | Strengthens API_CONTRACT context |
| APIs | Reinforcer | Plural |
| API endpoint | Reinforcer | Specific |
| integration | Reinforcer | Context word |
| external service | Context | Shifts IMPLEMENT to 4.3 |
| third-party | Context | Integration context |
| gateway | Context | API gateway context |
| SDK | Context | Third-party SDK |

---

## B17-0C-010-002 — "wire up" vs "connect": same predicate, different conditions

**"wire up":** Fires API_CONTRACT unconditionally. Rule 22 has no condition.
- "Wire up the endpoint" → API_CONTRACT
- "Wire up the service" → API_CONTRACT

**"connect":** Fires API_CONTRACT when combined with external service context. Rule 22 is conditional for "connect".
- "Connect the Stripe API to the payment flow" → API_CONTRACT (external service present)
- "Connect the two internal services" → ambiguous; may not fire API_CONTRACT (internal services)

**Recommendation:** "wire up" is the cleaner API_CONTRACT signal. "connect" requires external service context to reliably fire.

---

## B17-0C-010-003 — API_CONTRACT vocabulary interaction with IMPLEMENT

**Interaction rule:** API_CONTRACT vocabulary in the input does two things:
1. Fires the API_CONTRACT predicate
2. Acts as integration-context vocabulary that shifts IMPLEMENT from 4.1 to 4.3

**Example:** "Build the payment gateway connector."
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT (fires API_CONTRACT) + shifts IMPLEMENT to 4.3 (integration context)

**So connector vocabulary does double duty: predicate firing AND routing context shifting.**

---

## B17-0C-010-004 — "wire up" never fires IMPLEMENT

**Test:**
- "Wire up the payment connector." → ["API_CONTRACT"] only. IMPLEMENT does NOT fire (no build verb).
- "Build and wire up the payment connector." → ["IMPLEMENT", "API_CONTRACT"] (build fires IMPLEMENT; wire up fires API_CONTRACT)

**Rule:** "wire up" is an API_CONTRACT signal exclusively. Like "integrate" and "connect", it does not fire IMPLEMENT. A separate build verb is required.

**Three-way equivalence:** "integrate" ≡ "wire up" ≡ "connect" (+ external) for API_CONTRACT firing purposes. None fire IMPLEMENT.
