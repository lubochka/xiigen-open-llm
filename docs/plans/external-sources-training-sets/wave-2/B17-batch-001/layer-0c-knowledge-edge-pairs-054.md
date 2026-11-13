# Layer 0-C — Knowledge-Edge Pairs (054)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Netlify connector

---

## B17-0C-054-001 — Netlify: non-trigger terms

**Proper nouns — no predicate:** Netlify
**Domain nouns — no predicate:** site, build, deploy, form submission, identity, function, redirect, header, plugin, branch deploy, split test
**Acronyms — no predicate:** CDN (Content Delivery Network), JAMstack (JavaScript API Markup stack)

---

## B17-0C-054-002 — Netlify: "publish" and "configure" are not IMPLEMENT

"Publish the static site to Netlify" → no predicate.
"Configure Netlify redirect rules" → no predicate.
"Build the Netlify Functions connector" → IMPLEMENT fires from "Build".

---

## B17-0C-054-003 — Netlify: personal access token fires SECURITY; site ID does not

"Netlify personal access token" → SECURITY fires (compound contains "token").
"Netlify site ID" → no predicate ("ID" alone is not a token compound).
"Netlify deploy key" → no SECURITY (no "token").

---

## B17-0C-054-004 — Netlify: identity service uses JWT

"Build the Netlify Identity connector with JWT (JSON Web Token) user validation" → SECURITY fires (expansion "JSON Web Token" contains "Token").
"OAuth" present → AUTHORIZATION fires.
This is the same pattern as other identity connectors: JWT expansion fires SECURITY, OAuth fires AUTHORIZATION.
