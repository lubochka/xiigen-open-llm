# Layer 3 — Teaching Unit Pairs (014)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: Mailgun email connector

---

## B17-TU-014-001 — Email connector: "EmailNotificationService" vs "Mailgun email connector"

**A:** Both "Build the EmailNotificationService" and "Build the Mailgun email connector" use the verb "Build" and fire IMPLEMENT. Why does one route to 4.1 and the other to 4.3?

**B:**
- "EmailNotificationService" → "Service" is internal vocabulary (4.1)
- "Mailgun email connector" → "connector" is integration vocabulary (4.3)

The routing within IMPLEMENT is determined by vocabulary co-occurrence. "connector" overrides "service" if both are present. In this case, only one is present: the first has "service", the second has "connector". Connector → 4.3; service alone → 4.1.

---

## B17-TU-014-002 — Mailgun HMAC webhook validation fires SECURITY in same way as payment webhooks

**A:** Mailgun uses "HMAC-SHA256" validation for webhooks, just like GitHub and Stripe. Does SECURITY fire in all three cases?

**B:** Yes. Rule 8 (validate signature fires SECURITY) is domain-independent. Whether it is Mailgun email, Stripe payment, or GitHub webhook — the phrase "validate signature", "HMAC validation", or "signature validation" fires SECURITY in all cases. The predicate system is vocabulary-based, not domain-based.

---

## B17-TU-014-003 — "write tests" + "build connector" = IMPLEMENT + API_CONTRACT + TEST

**A:** "Create the Mailgun webhook connector, write integration tests for delivery event handling." What predicates fire?

**B:**
- "Create" → IMPLEMENT (Rule 11)
- "connector" → API_CONTRACT (Rule 4)
- "webhook" → API_CONTRACT (Rule 5; reinforces)
- "write integration tests" → TEST (Rule 13)
- **CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "TEST"]
- **SubPhase:** 4.3, 3.3, 5.1

Three predicates, three routing decisions, three sub-phases.

---

## B17-TU-014-004 — HMAC validation also fires SECURITY (not just "token")

**A:** "Create the Mailgun webhook connector and validate the HMAC signature." Does SECURITY fire? There is no "token" in this request.

**B:** Yes. SECURITY fires from "validate signature" (Rule 8), not only from "token" (Rule 6). Rule 8 covers: token, authentication, authenticate, validate signature, secure, HMAC validation. Any of these triggers SECURITY independently.

**Result:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"] — SECURITY fires from "validate signature", not from "token" (which is absent).

**Key distinction:** Rule 6 (token → SECURITY) and Rule 8 (validate signature → SECURITY) are separate triggers for the same predicate. Either is sufficient.
