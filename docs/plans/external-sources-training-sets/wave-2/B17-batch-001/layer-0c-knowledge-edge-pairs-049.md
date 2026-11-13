# Layer 0-C — Knowledge-Edge Pairs (049)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Auth0 connector

---

## B17-0C-049-001 — Auth0: non-trigger terms

**Proper nouns — no predicate:** Auth0, Okta (parent company)
**Domain nouns — no predicate:** tenant, user pool, rule, action, pipeline, universal login, social connection, organization, machine-to-machine, audience
**Acronyms — no predicate:** M2M (Machine-to-Machine), MFA (Multi-Factor Authentication), PKCE (Proof Key for Code Exchange)

---

## B17-0C-049-002 — Auth0: OIDC and PKCE do not fire AUTHORIZATION

"OIDC" (OpenID Connect) → no predicate.
"PKCE" (Proof Key for Code Exchange) → no predicate.
"OpenID Connect discovery" → no predicate.
Only "OAuth" fires AUTHORIZATION (Rule 7).

---

## B17-0C-049-003 — Auth0: "access token", "ID token", "refresh token" all fire SECURITY

All three Auth0 token types contain the word "token":
- access token → SECURITY
- ID token → SECURITY
- refresh token → SECURITY

Each fires Rule 6. The token type doesn't matter — only the presence of "token".

---

## B17-0C-049-004 — Auth0: "tenant" does not add routing

"Auth0 tenant configuration" — "tenant" is a domain noun. It adds no predicate, no routing decision, and no flow selection. Auth0 tenant scope does not multiply predicates.
