# Layer 0-C — Knowledge-Edge Pairs (053)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Vercel connector

---

## B17-0C-053-001 — Vercel: non-trigger terms

**Proper nouns — no predicate:** Vercel
**Domain nouns — no predicate:** deployment, preview, production, branch, environment variable, serverless function, edge function, domain, CDN (Content Delivery Network)
**Acronyms — no predicate:** CDN (Content Delivery Network), CI/CD (Continuous Integration/Continuous Deployment)

---

## B17-0C-053-002 — Vercel: "deploy" is not IMPLEMENT

"Deploy the application to Vercel" — "deploy" is not in the IMPLEMENT trigger list.
"Build the Vercel deployment connector" — "Build" fires IMPLEMENT, "deployment" is a modifier; "connector" fires API_CONTRACT.

---

## B17-0C-053-003 — Vercel: "configure environment variables" does not fire

"Configure environment variables in Vercel" → no predicates. "Configure" is not an IMPLEMENT trigger; "environment variables" is a domain noun.

---

## B17-0C-053-004 — Vercel: webhook + HMAC-SHA256 specific pattern

Vercel sends webhook events for deployment lifecycle.
"Validate the HMAC-SHA256 signature on Vercel webhook events" → SECURITY fires (Rule 8 complete phrase).
"Verify Vercel webhook events" alone → SECURITY does NOT fire (no HMAC phrase or token).
The SECURITY trigger requires the complete Rule 8 phrase including "HMAC-SHA256" and "validate/validate signature".
