# Layer 3 — Teaching Unit Pairs (011)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: teaching-unit

---

## B17-TU-011-001 — "wire up" = "connect" = "integrate" = API_CONTRACT vocabulary (complete list)

**All three are API_CONTRACT signals. None fire IMPLEMENT.**

| Phrase | API_CONTRACT? | IMPLEMENT? | Notes |
|---|---|---|---|
| integrate | Yes | No | Rule 1 — direct |
| wire up | Yes | No | Rule 22 — direct |
| connect (+ external) | Yes | No | Rule 22 — conditional |
| hook up | Yes (by analogy) | No | Similar to wire up |
| plug in | Yes (by analogy) | No | Similar to connect |
| link to | Yes (by analogy) | No | Similar to connect |

**All five are equivalent for API_CONTRACT purposes.** "wire up" is the clearest canonical form in B17.

---

## B17-TU-011-002 — "wire up" examples across B17 connectors

| Input | Predicates |
|---|---|
| "Wire up the Stripe webhook endpoint." | ["API_CONTRACT"] |
| "Build and wire up the PayPal capture API." | ["IMPLEMENT", "API_CONTRACT"] |
| "Wire up the Twilio SMS connector to the notification pipeline." | ["API_CONTRACT"] |
| "Implement and wire up the SendGrid bounce handler." | ["IMPLEMENT", "API_CONTRACT"] |
| "Wire up the Auth0 OAuth endpoints." | ["API_CONTRACT", "AUTHORIZATION"] (OAuth also fires) |

---

## B17-TU-011-003 — "connect" examples: conditional on external service context

**"connect" + external service → API_CONTRACT:**
- "Connect the Stripe API to the checkout flow." → API_CONTRACT (external API)
- "Connect the PayPal gateway to the payment processor." → API_CONTRACT (external gateway)
- "Connect the Auth0 service for authentication." → API_CONTRACT (external service)

**"connect" + internal context → ambiguous (may not fire API_CONTRACT):**
- "Connect the payment service to the database." → ambiguous (internal connection)
- "Connect module A to module B." → ambiguous (internal modules)

**Rule:** For B17 training, use "connect" in external service contexts where API_CONTRACT firing is clear.

---

## B17-TU-011-004 — Why none of these fire IMPLEMENT

**"integrate", "wire up", "connect", "hook up", "link to"** all describe the relationship between systems — not the act of writing code to build one of those systems.

**You can wire up an existing component (no new code needed).**
**You can integrate by configuring (no new code needed).**
**IMPLEMENT is only triggered when new code is committed to be written.**

The build verbs (build/implement/write/create/code/develop) are the exclusive IMPLEMENT triggers because they unambiguously commit to writing new code. API_CONTRACT vocabulary commits only to establishing a connection — which may or may not involve new code.
