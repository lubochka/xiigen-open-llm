# Layer 0-C — Knowledge-Edge Pairs (020)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Intercom messaging connector edges

---

## B17-0C-020-001 — Edge: messaging domain uses same connector vocabulary

**Assertion:** Customer messaging connectors (Intercom, Zendesk, etc.) use the same predicate vocabulary as payment and email connectors. No messaging-specific predicate triggers exist.

**Messaging terms with NO predicate:**
- conversation, message, ticket, chat, customer support, lifecycle event, inbox — domain nouns, no predicate

**Messaging terms that DO fire predicates (via integration vocabulary):**
| Token | Predicate |
|---|---|
| connector | API_CONTRACT (Rule 4) |
| webhook | API_CONTRACT (Rule 5) |
| API, endpoint | API_CONTRACT (reinforcers) |
| build, implement, write, create | IMPLEMENT |

---

## B17-0C-020-002 — Edge: "conversation routing logic" without connector vocabulary → 4.1

**Input:** "Implement the conversation routing logic and message dispatch service for the support inbox."

**Edge assertions:**
- "Implement" → IMPLEMENT
- "routing logic", "dispatch service" — internal service vocabulary (no integration tokens)
- No connector, no webhook, no API, no external service

**Result:** ["IMPLEMENT"] → 4.1

**Contrast with 4.3:** "Implement the Intercom conversation webhook connector" → IMPLEMENT → 4.3 (webhook + connector = integration vocabulary).

---

## B17-0C-020-003 — Edge: "bearer token" in Intercom context fires SECURITY

**Input:** "Write the Intercom REST connector and include bearer token authentication for all outbound API calls."

**Edge assertions:**
- "Write" → IMPLEMENT
- "connector" → API_CONTRACT
- "REST" — not a predicate trigger
- "token" → SECURITY (Rule 6: "bearer token" fires SECURITY)
- "authentication" → SECURITY (Rule 8: reinforces; fires once)
- "API" → API_CONTRACT (reinforces)

**Result:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

**Note:** Two SECURITY triggers ("token" and "authentication") — predicate fires once. Predicate is a set member, not a counter.

---

## B17-0C-020-004 — Edge: "shared secret" near SECURITY but not a direct trigger

**Input:** "Build the Intercom webhook connector and implement signature validation using the Intercom shared secret."

**Edge assertions:**
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT
- "webhook" → API_CONTRACT (reinforces)
- "signature validation" → SECURITY (Rule 8)
- "shared secret" — "secret" is not a direct SECURITY trigger; SECURITY fires from "signature validation"

**Result:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

**Teaching point:** "secret" alone does not fire SECURITY. The SECURITY predicate fires from the explicit trigger words: token, authentication, secure, validate signature, HMAC validation. "Secret" in "shared secret" is a proximity word that reinforces the security context but does not independently trigger SECURITY.
