# CB4 Stage 3 — Cross-Source Pair Set Extension 4
# Sequences CB4-3.4-291 through CB4-3.4-300
# Stratum: A/B/D — multi-source combinations for new sub-domains;
#           includes positive-acceptance cases across territories
# Instances: 20 (10 base pairs × 2 phrasings)
# Running total after this file: 600 instances

---

## Pair[CB4-3.4-291]

**A:** OAuth server assessment. Redirect URI validation uses prefix matching (`startsWith registered value`) — not exact match. The threat model is 14 months old and was created before the OAuth server was deployed. PKCE S256 is correctly implemented.

**B:**
```
Territory 1: oauth-oidc
Arbiter: ASVS-V10-OAuth

V10.2.1: REPROCESS — prefix matching does not satisfy exact registration.
Redirect URIs with any path under the registered prefix are accepted; exact
character comparison is required.

V10.4.6: PASS — S256 PKCE correctly implemented.

Territory verdict: REPROCESS.

Territory 2: ssdf-process
Arbiter: SSDF-PW

PW.2: REPROCESS — threat model 14 months old, predates OAuth server deployment.
OAuth trust boundaries, authorization code flows, and redirect URI attack surface
are not modeled.

Overall verdict: REPROCESS
Reason: Two independent MUST-level violations. V10.4.6 PASS within oauth-oidc
territory is reported but does not offset V10.2.1 REPROCESS.
```

**Reason:** Prefix matching violates V10.2.1 exact registration. PW.2 is REPROCESS because the threat model predates the entire OAuth deployment. Two REPROCESS findings across two territories. V10.4.6 PASS is noted within the territory but does not change the territory verdict.

**Phrasing-B:** OAuth review: prefix-matched redirect URIs, correct S256 PKCE, and 14-month threat model from before OAuth was deployed. How does the combined assessment read?

---

## Pair[CB4-3.4-292]

**A:** Session and token lifecycle review. JWT sessions have a 5-minute expiry — logout deletes the cookie, no server-side revocation. OAuth refresh tokens associated with sessions are not revoked at logout. Refresh tokens have a 30-day expiry.

**B:**
```
Territory 1: asvs-session
Arbiter: ASVS-V7-Session

V7.3.1: CONCERN — 5-minute session expiry is a significant mitigating factor
for the absence of server-side revocation. The post-logout exploitation window
is substantially limited. CONCERN (not REPROCESS) due to the short expiry
providing meaningful risk reduction. Server-side invalidation is still the
recommended remediation.

Territory 2: oauth-oidc
Arbiter: ASVS-V10-OAuth

V10.3 (refresh token revocation on logout): REPROCESS — refresh tokens with
30-day expiry are not revoked at logout. A captured refresh token allows
obtaining new access tokens for 30 days after the user's explicit logout.
The 5-minute session expiry does not mitigate the refresh token revocation
gap — these are independent lifecycle mechanisms.
Evidence gap: 30-day refresh tokens not invalidated at user logout.

Overall verdict: REPROCESS
Reason: V10.3 REPROCESS overrides V7.3.1 CONCERN. The session expiry
mitigation applies to session tokens only; it does not apply to the separate
OAuth refresh token lifecycle. REPROCESS precedence applies.
```

**Reason:** Two lifecycle layers with independent findings. The 5-minute session expiry makes V7.3.1 a CONCERN rather than REPROCESS. But the 30-day refresh tokens not revoked on logout is independently REPROCESS — the session mitigation argument does not carry over to the OAuth token layer. REPROCESS overrides CONCERN in the overall verdict.

**Phrasing-B:** 5-minute JWT sessions (logout clears cookie, no revocation) and 30-day OAuth refresh tokens (not revoked at logout). The short session window mitigates one concern. How does the combined V7.3.1 and V10.3 assessment come out?

---

## Pair[CB4-3.4-293]

**A:** PKCE and RFC compliance assessment. Authorization requests include correct S256 code_challenge. At the token exchange endpoint, the server accepts exchange requests without validating the submitted code_verifier — any value or absent value is accepted as long as the authorization code is valid.

**B:**
```
Territory 1: oauth-oidc
Arbiter: ASVS-V10-OAuth

V10.4.6: REPROCESS — PKCE requires both challenge (authorization request) and
verifier validation (token exchange). Accepting exchange requests without
code_verifier validation means the authorization code can be exchanged by any
party. The code-holder binding that PKCE provides is never enforced.

Territory 2: rfc-oauth2
Arbiter: RFC-OAuth2

RFC 7636 (PKCE complete flow): REPROCESS — RFC 7636 Section 4.6 requires that
the authorization server MUST verify the code_verifier by computing
BASE64URL(SHA256(code_verifier)) and comparing it to the stored code_challenge.
Accepting exchange requests without this verification violates RFC 7636 Section 4.6.
The challenge is computed and transmitted correctly; the server-side verification
at exchange is absent.

Overall verdict: REPROCESS
Reason: Both standards independently require code_verifier validation at exchange.
S256 challenge inclusion is a necessary but not sufficient PKCE condition.
```

**Reason:** RFC 7636 Section 4.6 explicitly requires server-side code_verifier validation at token exchange. ASVS V10.4.6 requires the complete PKCE flow. Sending a correct S256 challenge without enforcing the verifier at exchange satisfies neither standard. Two territories, two independent REPROCESS findings.

**Phrasing-B:** PKCE S256 challenges sent on all authorization requests, but the token exchange endpoint accepts any code_verifier (or none at all). Both ASVS V10.4.6 and RFC 7636 apply. What's the combined finding?

---

## Pair[CB4-3.4-294]

**A:** Authentication security review for a fintech application. Passwords are hashed with Argon2id (m=65536, t=3, p=4). Login endpoints have rate limiting: 5 attempts per 15 minutes per IP, with exponential backoff after 3 consecutive failures. CAPTCHA required after 5 failures.

**B:**
```
Territory 1: asvs-authentication
Arbiter: ASVS-V6-Auth

V6.2.5 (memory-hard hashing): PASS — Argon2id with m=65536, t=3, p=4.
Appropriate memory-hard algorithm with parameters exceeding minimum
recommendations.

Territory 2: cwe-authn
Arbiter: CWE-Authn

CWE-307 (improper restriction of authentication attempts): PASS
Rate limiting: 5 attempts per 15 minutes per IP. Exponential backoff after
3 consecutive failures. CAPTCHA after 5 failures. These controls collectively
restrict brute force attempts. The layered approach (rate limit + backoff +
CAPTCHA) satisfies CWE-307 brute force protection requirement.

Overall verdict: PASS
Reason: Both territories PASS. Memory-hard password storage (V6.2.5) and
brute force protection (CWE-307) are both correctly implemented.
```

**Reason:** Both territories produce PASS. Argon2id with appropriate parameters satisfies V6.2.5. Layered brute force protection (rate limiting + backoff + CAPTCHA) satisfies CWE-307. Multi-territory all-PASS baseline for authentication security combining ASVS and CWE perspectives.

**Phrasing-B:** Authentication review: Argon2id m=65536/t=3/p=4, login rate-limited 5 per 15 minutes with exponential backoff and CAPTCHA at 5 failures. V6.2.5 and CWE-307 assessment.

---

## Pair[CB4-3.4-295]

**A:** Infrastructure security review. Application secrets are retrieved at runtime from HashiCorp Vault — no credentials are in source code. JWT access tokens are passed to internal microservices via URL query parameters (legacy integration, migration scheduled for next release). SSDF PW.4 SAST is running. No threat model exists.

**B:**
```
Territory 1: ssdf-process
Arbiter: SSDF-PW

PW.1 (hardcoded credentials): PASS — credentials retrieved from HashiCorp
Vault at runtime. No credentials in source code. Correct pattern.

PW.4 (SAST): PASS — automated static analysis in pipeline.

PW.2 (threat model): REPROCESS — no threat model artefact exists.
Absence of a threat model is not compensated by correct credential management
or SAST practices. PW.2 is independently required.

Territory 2: asvs-token
Arbiter: ASVS-V9-Token

V9.3.1 (token transport): REPROCESS — JWT tokens transmitted via URL query
parameters on internal microservice routes. URL parameters appear in server
access logs, Referer headers, and log aggregation platforms regardless of
whether transmission is internal-only. Migration timeline does not suspend
the MUST requirement.

Overall verdict: REPROCESS
Reason: PW.2 REPROCESS and V9.3.1 REPROCESS. PW.1 PASS and PW.4 PASS
are noted but do not offset the two REPROCESS findings.
```

**Reason:** PW.1 PASS (Vault), PW.4 PASS (SAST), PW.2 REPROCESS (no threat model), V9.3.1 REPROCESS (URL tokens). Shows that doing some things correctly (credential management, SAST) does not offset independently required practices and controls. REPROCESS findings are independent of PASS findings.

**Phrasing-B:** Vault for secrets (nothing hardcoded), SAST running, but no threat model and JWT tokens in URL params on internal routes. How do PW.1/PW.4 PASSes interact with PW.2 REPROCESS and V9.3.1 REPROCESS?

---

## Pair[CB4-3.4-296]

**A:** OAuth and session lifecycle security review. All OAuth redirect URIs exactly registered. PKCE S256 implemented correctly. OAuth refresh tokens are revoked at logout via a revocation endpoint call. However, session JWT cookies are deleted client-side on logout without server-side revocation — JWT session expiry is 2 hours.

**B:**
```
Territory 1: oauth-oidc
Arbiter: ASVS-V10-OAuth

V10.2.1 (redirect URI): PASS — all URIs exactly registered.
V10.4.6 (PKCE S256): PASS — correct implementation.
V10.3 (token revocation at logout): PASS — refresh tokens revoked at logout
via revocation endpoint.
Territory verdict: PASS.

Territory 2: asvs-session
Arbiter: ASVS-V7-Session

V7.3.1 (server-side session invalidation): CONCERN — session cookie deleted
client-side. No server-side session revocation. 2-hour expiry creates a
meaningful post-logout exploitation window. CONCERN (not REPROCESS) noted
because 2 hours is substantially shorter than multi-day session tokens,
but not as mitigating as a 5-minute expiry.

Note: V10.3 refresh token revocation at logout does not satisfy V7.3.1 —
these are separate requirements for separate token types.

Territory verdict: CONCERN.

Overall verdict: CONCERN
Reason: oauth-oidc territory PASS. asvs-session territory CONCERN.
CONCERN overrides PASS in overall verdict per precedence rules.
```

**Reason:** OAuth controls are all correctly implemented (three PASS sub-requirements). Session token revocation on logout is a CONCERN due to 2-hour expiry without server-side revocation — more significant than 5 minutes but not as severe as multi-day. OAuth revocation does not satisfy V7.3.1. Overall CONCERN because CONCERN overrides PASS.

**Phrasing-B:** OAuth with exact redirect URIs, S256 PKCE, refresh token revocation at logout — all good. But session JWTs use client-side cookie deletion only, 2-hour expiry. We revoke refresh tokens at logout — does that satisfy V7.3.1 for the session token too?

---

## Pair[CB4-3.4-297]

**A:** Comprehensive positive security baseline review. Application credential management: all credentials in environment variables injected from Vault. Code review: Security Champion with OWASP Top 10 checklist reviews all PRs. OAuth redirect URIs: all exactly registered. PKCE S256: correctly implemented with code_verifier validated at exchange. Argon2id m=65536, t=3, p=4 for password hashing. Session tokens revoked server-side on logout via Redis session store. Users can access their permissions at `/me/permissions`.

**B:**
```
Territory 1: ssdf-process
Arbiter: SSDF-PW
PW.1: PASS (Vault + env vars). PW.3.2: PASS (Security Champion + checklist).
Territory verdict: PASS.

Territory 2: oauth-oidc
Arbiter: ASVS-V10-OAuth
V10.2.1: PASS (exact registration). V10.4.6: PASS (S256 + verifier validation).
Territory verdict: PASS.

Territory 3: asvs-authentication
Arbiter: ASVS-V6-Auth
V6.2.5: PASS (Argon2id m=65536, t=3, p=4).
Territory verdict: PASS.

Territory 4: asvs-session
Arbiter: ASVS-V7-Session
V7.3.1: PASS (Redis session store, server-side revocation on logout).
Territory verdict: PASS.

Territory 5: asvs-authorization
Arbiter: ASVS-V8-AuthZ
V8.2.1: PASS (/me/permissions endpoint present).
Territory verdict: PASS.

Overall verdict: PASS
Reason: All five territories PASS. All new sub-domain controls correctly
implemented across credential management, OAuth, authentication, session
management, and authorization transparency.
```

**Reason:** Five-territory all-PASS baseline covering all the positive implementations introduced in ext11 (Vault credentials, Security Champion, exact OAuth redirect URIs, complete S256 PKCE, Argon2id, server-side session revocation, permission introspection). Establishes the reference standard against which gap findings are measured.

**Phrasing-B:** Mature platform with Vault credential management, Security Champion code review, exact OAuth redirect URIs, complete S256 PKCE, Argon2id, Redis session revocation, and /me/permissions. Five-territory PASS assessment.

---

## Pair[CB4-3.4-298]

**A:** Security review covering both process and authentication controls. Passwords hashed with Argon2id (m=65536, t=3, p=4). The security code review process uses a Security Champion with an OWASP Top 10 checklist. SAST runs on every PR. No threat model artefact exists (PW.2 gap). Login has rate limiting but bcrypt at cost 4 is used for API token hashing (not user passwords — user passwords use Argon2id).

**B:**
```
Territory 1: asvs-authentication
Arbiter: ASVS-V6-Auth

V6.2.5 (user password hashing): PASS — user passwords use Argon2id m=65536,
t=3, p=4. Appropriate memory-hard algorithm.

V6.2.5 (API token hashing with bcrypt cost 4): CONCERN — API tokens hashed
with bcrypt cost 4 are subject to the same cost-factor concern as user
passwords. bcrypt cost 4 is insufficient for current hardware. Separate
CONCERN for the API token hashing configuration.

Territory 2: ssdf-process
Arbiter: SSDF-PW

PW.3.2: PASS — Security Champion with OWASP checklist satisfies security-
focused code review.
PW.4: PASS — SAST on every PR.
PW.2: REPROCESS — no threat model artefact. PW.3.2 PASS and PW.4 PASS do not
substitute for PW.2.

Overall verdict: REPROCESS
Reason: PW.2 REPROCESS. asvs-authentication CONCERN (API token bcrypt cost 4)
does not change the overall verdict — REPROCESS from PW.2 overrides CONCERN
from the authentication territory.
```

**Reason:** Within the asvs-authentication territory, two V6.2.5 assessments coexist for different credential types — user passwords (PASS, Argon2id) and API tokens (CONCERN, bcrypt cost 4). The same requirement applies to different credential types in the same submission. PW.2 REPROCESS overrides all PASS/CONCERN findings in the overall verdict.

**Phrasing-B:** User passwords use Argon2id correctly, but API tokens use bcrypt cost 4. Security Champion + OWASP checklist + SAST running, but no threat model. V6.2.5 applies to both credential types. What's the combined assessment?

---

## Pair[CB4-3.4-299]

**A:** OAuth security review where team has corrected all previously identified gaps. Redirect URIs exactly registered for all clients (migration from wildcard complete). S256 PKCE with code_verifier validation at exchange. State parameter present on all authorization requests. Refresh tokens revoked at logout. The team asks for a clean bill of health across V10.

**B:**
```
Territory: oauth-oidc
Arbiter: ASVS-V10-OAuth

V10.2.1 (redirect URI exact registration): PASS — all client redirect URIs
exactly registered; wildcard migration complete.

V10.2.6 (state parameter): PASS — state present on all authorization requests.

V10.4.6 (PKCE S256): PASS — S256 method, code_verifier validated at exchange.

V10.3 (refresh token revocation): PASS — refresh tokens revoked at logout.

Territory verdict: PASS.

Overall verdict: PASS
Reason: All evaluated V10 sub-requirements satisfied. This is the positive
remediation baseline — all four V10 gaps that have appeared in prior CB4
pairs have been closed.
```

**Reason:** Full V10 positive baseline: V10.2.1, V10.2.6, V10.4.6, V10.3 all PASS. Establishes that when all identified V10 sub-requirements are addressed, the territory and overall verdict are PASS. This is the "after remediation" case that complements the individual REPROCESS pairs.

**Phrasing-B:** We've remediated all the V10 OAuth gaps: exact redirect URIs (wildcard migration done), state parameter on all flows, S256 PKCE with verifier validation, and refresh token revocation at logout. Can we get PASS on V10?

---

## Pair[CB4-3.4-300]

**A:** Comprehensive security sign-off review for a newly certified application. Evidence covers all new sub-domains: exact OAuth redirect URIs, server-side session revocation on logout (Redis), S256 PKCE with verifier validation, Argon2id m=65536/t=3/p=4 for passwords, /me/permissions endpoint, no hardcoded credentials (Vault), security-focused code review (Security Champion + OWASP checklist), state parameter on all OAuth requests, SAST on every PR, current threat model (7 months), all authorization checks server-side with deny-by-default.

**B:**
```
Territory 1: oauth-oidc
ASVS-V10-OAuth: V10.2.1 PASS, V10.2.6 PASS, V10.4.6 PASS, V10.3 PASS.
Territory: PASS.

Territory 2: asvs-session
ASVS-V7-Session: V7.3.1 PASS (Redis server-side revocation).
Territory: PASS.

Territory 3: asvs-authentication
ASVS-V6-Auth: V6.2.5 PASS (Argon2id m=65536, t=3, p=4).
Territory: PASS.

Territory 4: asvs-authorization
ASVS-V8-AuthZ: V8.2.1 PASS (/me/permissions). V8.1.2 PASS (deny-by-default
server-side checks).
Territory: PASS.

Territory 5: ssdf-process
SSDF-PW: PW.1 PASS (Vault). PW.3.2 PASS (Security Champion + checklist).
PW.4 PASS (SAST). PW.2 PASS (threat model 7 months, current).
Territory: PASS.

Overall verdict: PASS
Reason: All five territories PASS. Complete positive baseline across all
new sub-domains and all core security controls assessed in CB4.
```

**Reason:** This is the comprehensive all-clear baseline covering every new sub-domain category introduced in ext10-ext11 plus core controls. Five territories, all PASS. Establishes the ideal reference state against which gaps are measured across the entire CB4 dataset.

**Phrasing-B:** Full sign-off review: all V10 OAuth sub-requirements met, Redis session revocation, Argon2id, permissions endpoint, deny-by-default, Vault credentials, Security Champion review, SAST, current threat model. What does the comprehensive 5-territory assessment show?

---

## File Summary

| Pair ID | Description | Stratum | Territories | Verdict |
|---------|-------------|---------|-------------|---------|
| CB4-3.4-291 | V10.2.1 prefix match REPROCESS + PW.2 REPROCESS; V10.4.6 PASS | B | oauth-oidc, ssdf-process | REPROCESS |
| CB4-3.4-292 | V7.3.1 5-min CONCERN + V10.3 30-day refresh REPROCESS | A | asvs-session, oauth-oidc | REPROCESS |
| CB4-3.4-293 | V10.4.6 no verifier validation + RFC 7636 §4.6 | A | oauth-oidc, rfc-oauth2 | REPROCESS |
| CB4-3.4-294 | V6.2.5 Argon2id PASS + CWE-307 rate limit PASS | A | asvs-authentication, cwe-authn | PASS |
| CB4-3.4-295 | PW.1 Vault PASS + PW.4 PASS + PW.2 REPROCESS + V9.3.1 REPROCESS | B | ssdf-process, asvs-token | REPROCESS |
| CB4-3.4-296 | oauth-oidc all PASS + asvs-session CONCERN (2-hour, no server revocation) | B | oauth-oidc, asvs-session | CONCERN |
| CB4-3.4-297 | All new sub-domain 5-territory PASS: Vault + Security Champion + OAuth + Argon2id + session | A | ssdf-process, oauth-oidc, asvs-authentication, asvs-session, asvs-authorization | PASS |
| CB4-3.4-298 | User Argon2id PASS + API token bcrypt4 CONCERN + PW.2 REPROCESS | B | asvs-authentication, ssdf-process | REPROCESS |
| CB4-3.4-299 | All V10 sub-requirements satisfied after remediation — full V10 PASS | A | oauth-oidc | PASS |
| CB4-3.4-300 | Comprehensive 5-territory complete PASS baseline | A | oauth-oidc, asvs-session, asvs-authentication, asvs-authorization, ssdf-process | PASS |
