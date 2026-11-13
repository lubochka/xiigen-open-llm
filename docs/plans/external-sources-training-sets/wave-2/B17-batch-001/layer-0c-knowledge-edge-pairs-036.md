# Layer 0-C — Knowledge-Edge Pairs (036)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Complete edge reference for connectors 025-036

---

## B17-0C-036-001 — Complete non-trigger terms for connectors 025-036

**Proper nouns — no predicate:**
Twilio, SendGrid, Plaid, DocuSign, Cloudinary, Algolia, Segment, Mixpanel, Zendesk, Salesforce, GitHub

**Domain nouns — no predicate:**
SMS, email, banking, e-signature, media, search, analytics, product metrics, support, CRM, source control,
ticket, agent, envelope, link, faceted, catalogue, write key, project, repository, commit, branch, push

**Acronyms — no predicate:**
SMS, CRM, ERP, SaaS, HMAC (the acronym itself; "HMAC validation" as phrase fires SECURITY), PR (pull request)

**Verbs not in IMPLEMENT trigger set:**
aggregate, sync, dispatch, coordinate, route, bridge, orchestrate, connect (alone, without build verb)

---

## B17-0C-036-002 — "authorization" English word: complete anti-trigger list

The following inputs do NOT fire AUTHORIZATION:
- "with proper authorization"
- "authorized access"
- "for authorization purposes"
- "the authorization service"
- "requires authorization"
- "authorization code" (the OAuth 2.0 grant type name, when "OAuth" absent)

AUTHORIZATION fires ONLY on: "OAuth", "OAuth 2.0", "OAuth token", "OAuth flow" — the word "OAuth" in any form.

---

## B17-0C-036-003 — SECURITY triggers: what fires vs what does not

**Fires SECURITY (Rule 6 or 8):**
- token (any compound: bearer token, access token, API token, project token, link token)
- authentication, authenticate
- validate signature, HMAC validation, HMAC-SHA256 validation
- secure (used as adjective for connection/channel)

**Does NOT fire SECURITY:**
- authorization (English word)
- Basic auth (not a SECURITY trigger in this corpus)
- signed URL
- API key (without "token")
- write key (without "token")
- admin key (without "token")
