# Layer 3 — Teaching Unit Pairs (020)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: Intercom messaging connector

---

## B17-TU-020-001 — "conversation routing logic" without connector vocabulary → 4.1

**A:** "Implement the conversation routing logic and message dispatch service for the support inbox." Does this route to 4.3?

**B:** No. "conversation routing logic" and "message dispatch service" are internal service patterns — no integration vocabulary is present (no connector, API, webhook, external service, gateway, SDK, endpoint). IMPLEMENT routes to 4.1.

**Contrast:**
- "Implement the conversation routing logic" → IMPLEMENT → 4.1 (no integration vocabulary)
- "Implement the Intercom conversation webhook connector" → IMPLEMENT + API_CONTRACT → 4.3

The support inbox and message dispatch are internal concerns. IMPLEMENT routes to 4.3 only when external connector vocabulary is present.

---

## B17-TU-020-002 — "shared secret" does not fire SECURITY; "signature validation" does

**A:** "Build the Intercom webhook connector with Intercom shared secret validation." Does SECURITY fire?

**B:** Yes — from "signature validation" (Rule 8), not from "shared secret". "Secret" is not a SECURITY trigger in isolation. The SECURITY predicate fires from the explicit trigger phrase "validate signature" or "signature validation" (Rule 8).

**Result:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

**Teaching point:** "Secret" alone does not fire SECURITY. "Shared secret" does not fire SECURITY. The trigger is "validate signature", "signature validation", "HMAC validation", "authenticate", "authentication", "token", or "secure". Proximity words like "secret" reinforce the security context but do not independently trigger SECURITY.

---

## B17-TU-020-003 — "bearer token" fires SECURITY in messaging context

**A:** "Write the Intercom REST connector and include bearer token authentication for all outbound API calls." What predicates fire?

**B:**
- "Write" → IMPLEMENT (Rule 11)
- "connector" → API_CONTRACT
- "token" → SECURITY (Rule 6: "bearer token" fires SECURITY)
- "authentication" → SECURITY (Rule 8: reinforces; fires once)
- "API" → API_CONTRACT (reinforces)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Two SECURITY triggers (token + authentication) → predicate fires once (Rule: predicates are sets).

---

## B17-TU-020-004 — "document" + "test" add routing without changing 4.3 routing

**A:** Adding "document the webhook schema" and "write unit tests" to the Intercom connector request — does this change the sub-phase routing for IMPLEMENT?

**B:** No. Adding DOCUMENT and TEST predicates adds routing decisions to sub-phases 2.1 and 5.1 respectively. They do not change the IMPLEMENT routing (still 4.3) or the API_CONTRACT routing (still 3.3).

**Before:** ["IMPLEMENT", "API_CONTRACT"] → 4.3, 3.3 (DecisionCount 2)
**After:** ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"] → 4.3, 3.3, 5.1, 2.1 (DecisionCount 4)

Additional predicates add routing decisions. They do not modify existing routing decisions.
