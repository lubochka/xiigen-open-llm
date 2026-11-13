# Layer 0-C — Knowledge-Edge Pairs (004)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: knowledge-edge

---

## B17-0C-004-001 — "webhook" always fires API_CONTRACT

**Rule 5:** "webhook" fires API_CONTRACT in all compound and standalone forms.

**Examples:**
- "webhook listener" → API_CONTRACT
- "webhook handler" → API_CONTRACT
- "webhook endpoint" → API_CONTRACT
- "inbound webhook" → API_CONTRACT
- "bounce-webhook" → API_CONTRACT (hyphenated compound)
- "GitHubWebhookConnector" → API_CONTRACT (contained in compound class name)
- "implement webhook validation" → IMPLEMENT + API_CONTRACT

**"webhook" is always an external-facing API surface.** By definition, a webhook is an API endpoint exposed to a third party for event delivery. This makes "webhook" a strong API_CONTRACT signal.

---

## B17-0C-004-002 — Webhook security pattern: webhook → API_CONTRACT; validate signature → SECURITY

**Pattern:** "Build the webhook endpoint and validate the signature."

**Compound firing:**
- "webhook" → API_CONTRACT
- "validate" + "signature" → SECURITY (signature validation is the standard security pattern for webhooks)
- "Build" → IMPLEMENT

**Result:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

**The webhook security pattern is the most common three-predicate combination in B17:**
Any webhook task that includes signature verification → API_CONTRACT + SECURITY (both fire).

---

## B17-0C-004-003 — Webhook security vocabulary table

| Webhook security phrase | Predicate fired |
|---|---|
| validate signature | SECURITY |
| verify signature | SECURITY |
| HMAC validation | SECURITY (HMAC: Hash-based Message Authentication Code) |
| signature check | SECURITY |
| webhook authentication | SECURITY |
| secure webhook | SECURITY |
| webhook secret | SECURITY (secret → SECURITY context) |

**Note:** "webhook authentication" fires SECURITY (from "authentication"), not AUTHORIZATION (OAuth is not involved in webhook signature validation).

---

## B17-0C-004-004 — Webhook compound forms: all fire API_CONTRACT

**Common webhook compound forms in B17:**

| Form | API_CONTRACT? |
|---|---|
| webhook | Yes |
| Webhook | Yes (case-insensitive) |
| WEBHOOK | Yes |
| bounce-webhook | Yes (hyphenated) |
| webhooks (plural) | Yes |
| webhook_handler | Yes (snake_case) |
| WebhookHandler | Yes (PascalCase class name) |
| webhook_endpoint | Yes |

**The token "webhook" is recognized regardless of case, hyphenation, or surrounding punctuation.**
