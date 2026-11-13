# Layer 0-C — Knowledge-Edge Pairs (060)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Complete edge reference for connectors 049-060

---

## B17-0C-060-001 — Complete non-trigger terms for connectors 049-060

**Proper nouns — no predicate:**
Auth0, Okta, Cognito, AWS, Amazon, Firebase, Google Firebase, Supabase, Vercel, Netlify, Cloudflare, Azure, Microsoft, GCS, Google Cloud, HashiCorp Vault

**Domain nouns — no predicate:**
tenant, user pool, identity pool, bucket, object, blob, container, zone, record, worker, function, site, deployment, secret, lease, path, engine, tier, namespace

**Acronyms — no predicate:**
PKCE (Proof Key for Code Exchange), MFA (Multi-Factor Authentication), M2M (Machine-to-Machine), SRP (Secure Remote Password), SAML (Security Assertion Markup Language), OIDC (OpenID Connect), RLS (Row-Level Security), IAM (Identity and Access Management), CDN (Content Delivery Network), WAF (Web Application Firewall), DNS (Domain Name System), SAS (Shared Access Signature — noun form), AAD (Azure Active Directory), RBAC (Role-Based Access Control), PKI (Public Key Infrastructure), FCM (Firebase Cloud Messaging), S3 (Simple Storage Service)

---

## B17-0C-060-002 — "authorization" English word: does NOT fire AUTHORIZATION (049-060)

The following inputs do NOT fire AUTHORIZATION:
- "with proper authorization"
- "requires authorization to access"
- "authorization header" (HTTP header)
- "authorized access"
- "authorization code" (OAuth grant type name, when "OAuth" absent)
AUTHORIZATION fires ONLY on: "OAuth", "OAuth 2.0", "OAuth token", "OAuth flow", "OAuth role" — the word "OAuth" in any form.

---

## B17-0C-060-003 — SECURITY triggers that apply to all connectors 049-060

**Fires SECURITY:**
- token (any compound: access token, bearer token, ID token, refresh token, Vault token, SAS token, service account token, JWT JSON Web Token)
- authenticate / authentication (any form)
- HMAC-SHA256 validate/validation phrase

**Does NOT fire SECURITY:**
- SAML (Cognito, Supabase)
- OIDC (Auth0, Cognito, Firebase, Netlify)
- PKCE (Auth0)
- signed URL (GCS, S3)
- API key (any)
- connection string (AzureBlob)
- secret (Vault — without "token")
- certificate / x.509

---

## B17-0C-060-004 — JWT abbreviation vs expansion: canonical teaching

| Form | SECURITY fires? |
|---|---|
| "JWT" alone | No |
| "JWT (JSON Web Token)" | Yes — "Token" in expansion |
| "JSON Web Token" written out | Yes — "Token" present |
| "access token" | Yes — "token" present |
| "JWT token" (pleonastic) | Yes — "token" present |
