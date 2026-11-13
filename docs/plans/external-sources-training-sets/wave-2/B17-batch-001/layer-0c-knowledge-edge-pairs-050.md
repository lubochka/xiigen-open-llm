# Layer 0-C — Knowledge-Edge Pairs (050)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: AWS Cognito connector

---

## B17-0C-050-001 — Cognito: non-trigger terms

**Proper nouns — no predicate:** Cognito, AWS, Amazon
**Domain nouns — no predicate:** user pool, identity pool, federated identity, hosted UI, user group, attribute, trigger (Lambda), challenge, MFA (Multi-Factor Authentication)
**Acronyms — no predicate:** SRP (Secure Remote Password), SAML (Security Assertion Markup Language), OIDC (OpenID Connect)

---

## B17-0C-050-002 — Cognito: SAML does not fire AUTHORIZATION

"Cognito with SAML federation" → AUTHORIZATION does NOT fire. SAML (Security Assertion Markup Language) is not "OAuth".
"Cognito with OAuth 2.0" → AUTHORIZATION fires.
Teaching: Cognito supports both SAML and OAuth. The predicate fires only when "OAuth" is present.

---

## B17-0C-050-003 — Cognito: "ID token" and "access token" fire SECURITY

Cognito issues three token types: ID token, access token, refresh token.
All three contain "token" → SECURITY fires (Rule 6) for any of them.

---

## B17-0C-050-004 — Cognito: Lambda trigger is not IMPLEMENT trigger

"Cognito Lambda trigger for pre-authentication" — "trigger" here is a Cognito UI/feature concept, not the IMPLEMENT verb trigger. IMPLEMENT does not fire from "trigger" used as a domain noun. "pre-authentication" → SECURITY fires (Rule 8: "authentication" fires SECURITY). If "connector" is also present → API_CONTRACT + IMPLEMENT also fire.
