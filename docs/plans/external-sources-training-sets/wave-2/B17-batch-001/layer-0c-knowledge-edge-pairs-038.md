# Layer 0-C — Knowledge-Edge Pairs (038)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Slack connector

---

## B17-0C-038-001 — Slack: non-trigger terms

**Proper nouns — no predicate:** Slack
**Domain nouns — no predicate:** channel, workspace, message, bot, slash command, app, team, DM (Direct Message), mention, thread, reaction, modal
**Acronyms — no predicate:** DM (Direct Message), RTM (Real Time Messaging), API (alone)

---

## B17-0C-038-002 — Slack: "signing secret" does not fire SECURITY

"Signing secret" is Slack's webhook request verification mechanism. It is NOT in the SECURITY trigger list.
- "signing secret" → no predicate
- "signing secret token" → SECURITY fires (compound contains "token")
- "HMAC-SHA256 signature validation" → SECURITY fires (Rule 8 phrase)

Teaching: Domain terminology for credential management ("signing secret", "client secret") does not substitute for the trigger word "token" or the Rule 8 HMAC phrase.

---

## B17-0C-038-003 — Slack: "Events API" does not add a second API_CONTRACT

"Slack Events API" uses "API" — but API_CONTRACT fires only once (predicate set is deduplicated). "Events API" does not add a second API_CONTRACT occurrence.

---

## B17-0C-038-004 — Slack: "workspace" scope does not affect routing

"OAuth token with workspace scope" → AUTHORIZATION fires (OAuth present), SECURITY fires (token present). The scope specification ("workspace") is a domain modifier — it does not add or remove predicates.
