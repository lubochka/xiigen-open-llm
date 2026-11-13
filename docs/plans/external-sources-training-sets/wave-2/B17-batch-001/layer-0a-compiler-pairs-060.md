# Layer 0-A — Compiler Pairs (060)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Complete connector reference for connectors 049-060

---

## B17-0A-060-001 — Connector predicate summary (049-060)

| Connector | Key trigger(s) | Typical predicate set |
|---|---|---|
| Auth0Connector | connector, OAuth, token | [IMPL, API, SEC, AUTH] |
| CognitoConnector | connector, OAuth, token | [IMPL, API, SEC, AUTH] |
| FirebaseConnector | connector, webhook, token | [IMPL, API, SEC] |
| SupabaseConnector | connector, token (or JWT) | [IMPL, API, SEC] |
| VercelConnector | connector, webhook, token | [IMPL, API, SEC] |
| NetlifyConnector | connector, webhook, token | [IMPL, API, SEC] |
| CloudflareConnector | connector, webhook, token | [IMPL, API, SEC] |
| AWSS3Connector | connector, token | [IMPL, API, SEC] |
| GCSConnector | connector, OAuth, token | [IMPL, API, SEC, AUTH] |
| AzureBlobConnector | connector, token, OAuth | [IMPL, API, SEC, AUTH] |
| HashiCorpVaultConnector | connector, token, authenticate | [IMPL, API, SEC] |

---

## B17-0A-060-002 — "integrate" rule: applies equally to all connectors 049-060

For any connector 049-060, "integrate [ConnectorName]" or "integrate [ConnectorName] API":
- Fires API_CONTRACT only
- Does NOT fire IMPLEMENT
- TransitionType: documentation
- No connector code is generated
This is Rule 1 (absolute) and has NO exception based on connector domain.

---

## B17-0A-060-003 — Non-IMPLEMENT verbs for connectors 049-060

The following verbs appear in connector 049-060 inputs but do NOT fire IMPLEMENT:
- deploy (Vercel, Netlify, Cloudflare)
- publish (Netlify)
- upload (S3, GCS, AzureBlob)
- store (S3, GCS, AzureBlob)
- stream (AzureBlob)
- retrieve (HashiCorpVault)
- inject (HashiCorpVault)
- rotate (HashiCorpVault)
- push (Firebase)
- sync (Firebase, Supabase)
- configure (Vercel, Netlify, Cloudflare)

IMPLEMENT trigger list for 049-060: build, implement, write, create, code, develop, refactor, restructure, rewrite.

---

## B17-0A-060-004 — OIDC and SAML do not fire AUTHORIZATION (049-060)

For all connectors 049-060:
- "OIDC" alone → no predicate
- "SAML" alone → no predicate
- "OpenID Connect" → no predicate
- "OAuth" (any form, any case) → AUTHORIZATION (Rule 7; unconditional)
- "OAuth 2.0", "OAuth token", "OAuth flow", "OAuth role" → all fire AUTHORIZATION

---

## B17-0A-060-005 — JWT expansion teaches token rule

JWT (JSON Web Token) expansion contains "Token" → SECURITY fires when expansion is present.
"JWT" alone (abbreviation without expansion) does NOT fire SECURITY.
Best practice: treat JWT with its expansion — the expansion "JSON Web Token" supplies the "token" trigger.
This rule applies in: Supabase, Firebase, Auth0, Cognito, and any connector using JWT-based access.
