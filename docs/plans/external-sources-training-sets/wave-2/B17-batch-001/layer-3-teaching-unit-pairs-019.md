# Layer 3 — Teaching Unit Pairs (019)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: Okta identity connector

---

## B17-TU-019-001 — "OIDC" does NOT fire AUTHORIZATION; only "OAuth" fires AUTHORIZATION

**A:** "Create the Okta OIDC connector." Does AUTHORIZATION fire?

**B:** No. Rule 7 fires AUTHORIZATION on the literal word "OAuth" (case-insensitive). "OIDC" is an acronym for OpenID Connect — it is an OAuth-based protocol, but the word "OIDC" is not "OAuth". AUTHORIZATION does NOT fire on "OIDC", "OpenID Connect", "SAML", "PKCE", or any other identity protocol acronym.

**Contrast:**
- "Build the Okta OAuth connector" → AUTHORIZATION fires (literal "OAuth")
- "Build the Okta OIDC connector" → AUTHORIZATION does NOT fire ("OIDC" ≠ "OAuth")
- "Build the Okta OpenID Connect connector" → AUTHORIZATION does NOT fire (written out ≠ "OAuth")

---

## B17-TU-019-002 — "authentication" fires SECURITY; "authorization" (English) does NOT fire AUTHORIZATION

**A:** "Build the authorization connector for Okta access control." Does AUTHORIZATION fire from the English word "authorization"?

**B:** No. AUTHORIZATION fires from "OAuth" only (Rule 7). The English word "authorization" does not fire the AUTHORIZATION predicate. "authorization" in "authorization connector" or "authorization code flow" is an English word describing an authorization concept — it is not the trigger.

**Contrast:**
- "Build the OAuth authorization connector" → AUTHORIZATION fires (from "OAuth", not from "authorization")
- "Build the authorization connector" (no "OAuth") → NO AUTHORIZATION

**The trigger is "OAuth" the word, not "authorization" the concept.**

---

## B17-TU-019-003 — "SAML connector" → no AUTHORIZATION, no SECURITY (unless explicit trigger present)

**A:** "Develop the Okta SAML connector for enterprise SSO." What predicates fire?

**B:**
- "Develop" → IMPLEMENT (Rule 11)
- "connector" → API_CONTRACT (Rule 4)
- "SAML" — no predicate
- "SSO" (Single Sign-On) — no predicate
- No "token" → SECURITY does NOT fire
- No "OAuth" → AUTHORIZATION does NOT fire
- No "authentication" → SECURITY does NOT fire

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]

**Teaching point:** SECURITY and AUTHORIZATION do not fire automatically on identity connector requests. They fire only when their specific trigger words are present. A SAML connector without "token", "OAuth", or "authenticate" yields only IMPLEMENT + API_CONTRACT.

---

## B17-TU-019-004 — Okta multi-predicate path: IMPLEMENT + API_CONTRACT + SECURITY + AUTHORIZATION

**A:** "Build the Okta connector, implement the OAuth 2.0 PKCE flow, and validate the access token." What predicates fire?

**B:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT
- "OAuth" → AUTHORIZATION (Rule 7)
- "token" → SECURITY (Rule 6: "access token" fires SECURITY)
- "validate" + token context → SECURITY (Rule 8: reinforces)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
**DecisionCount:** 4
**UniqueSubPhases:** 3 (4.3, 3.3, 3.4)

**"PKCE" alone does not fire any predicate.** AUTHORIZATION fires from "OAuth". SECURITY fires from "token". The PKCE method (S256) is a domain detail handled in the nine-node flow, not a predicate trigger.
