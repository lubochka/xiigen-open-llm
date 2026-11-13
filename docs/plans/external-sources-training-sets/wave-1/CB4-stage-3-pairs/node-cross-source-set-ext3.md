# CB4 Stage 3 — Cross-Source Pair Set Extension 3
# Sequences CB4-3.4-265 through CB4-3.4-274
# Stratum: A/B/C — multi-source new sub-domain combinations
# Instances: 20 (10 base pairs × 2 phrasings)
# Running total after this file: 548 instances

---

## Pair[CB4-3.4-265]

**A:** Identity provider OAuth server review. Redirect URIs are registered as wildcard patterns (`https://*.example.com/callback`) for operational convenience across subdomains. The organization's threat model is 16 months old and was produced before the OAuth server was deployed. Separately, PKCE S256 is correctly implemented on all flows.

**B:**
```
Territory 1: oauth-oidc
Arbiter: ASVS-V10-OAuth

V10.2.1 (redirect URI): REPROCESS — wildcard redirect URI registration
permits authorization code interception via any matching subdomain including
those obtained through subdomain takeover. Exact registration required.

V10.4.6 (PKCE): PASS — S256 method correctly implemented.

Territory verdict: REPROCESS (V10.2.1 violation overrides V10.4.6 PASS).

Territory 2: ssdf-process
Arbiter: SSDF-PW

PW.2 (threat model): REPROCESS — 16 months exceeds the currency threshold.
The threat model was produced before the OAuth server existed; the OAuth
attack surface (authorization code flows, redirect URIs, token issuance,
client credential exposure) is not modeled. Threat model does not reflect
the current system.

Overall verdict: REPROCESS
Reason: Two territories, two REPROCESS findings. V10.4.6 PASS within the
oauth-oidc territory does not offset V10.2.1 REPROCESS.
```

**Reason:** Within the oauth-oidc territory, V10.4.6 PASS and V10.2.1 REPROCESS coexist — territory verdict is REPROCESS because any REPROCESS in a territory overrides PASS. PW.2 is independently REPROCESS (16 months, OAuth server not modeled). Two independent violations; PKCE satisfaction is reported but does not change the overall outcome.

**Phrasing-B:** OAuth server review: wildcard redirect URIs for all our subdomains, PKCE S256 correctly implemented, and threat model from 16 months ago (before OAuth was deployed). Two separate compliance questions here — what does the full assessment show?

---

## Pair[CB4-3.4-266]

**A:** User authentication flow review for a SaaS platform. On logout: (1) the application deletes the session cookie (client-side), but there is no server-side session revocation list, so the JWT session token remains valid for its remaining expiry window. (2) The user's OAuth refresh tokens are not revoked at logout — the authorization server has no logout endpoint that triggers refresh token invalidation.

**B:**
```
Territory 1: asvs-session
Arbiter: ASVS-V7-Session

V7.3.1 (server-side session invalidation on logout): REPROCESS — session cookie
deleted client-side but JWT session token not server-side revoked. Token
valid for remaining expiry after logout. Server-side invalidation requires
a revocation mechanism (session store, token blacklist, or similar).
Evidence gap: no server-side session invalidation on logout.

Territory 2: oauth-oidc
Arbiter: ASVS-V10-OAuth

V10.3.x (token revocation): REPROCESS — refresh tokens are not invalidated
when the user logs out of the application. An OAuth refresh token captured
before logout (via log exposure, interception, or storage compromise) can
be used to obtain new access tokens indefinitely after the user's logout.
Logout MUST trigger revocation of all active refresh tokens associated with
the session.
Evidence gap: OAuth refresh tokens not revoked on user logout.

Overall verdict: REPROCESS
Reason: Two independent session and token lifecycle violations. The user's
logout intent is not honored by either the session layer or the OAuth
token layer.
```

**Reason:** V7.3.1 (session invalidation) and V10.3 (token revocation) address different parts of the authentication lifecycle — session tokens and OAuth refresh tokens. Both must be invalidated on logout. The failures are in different territories (asvs-session and oauth-oidc) with different arbiters but the same underlying cause: missing server-side revocation on logout.

**Phrasing-B:** On logout we delete the session cookie, but the JWT stays valid since we don't have a revocation list. Also, we don't have an OAuth logout endpoint that revokes refresh tokens. These are two separate issues on the same logout path. How does the assessment classify this?

---

## Pair[CB4-3.4-267]

**A:** Application security audit. Source code review reveals: `DB_PASSWORD = "Pr0d_DB_S3cr3t#2024"` hardcoded in `app/config.py`, committed to the main branch. The same credential is used in the authentication module to connect to the user accounts database. The credential has been in version control for 11 months.

**B:**
```
Territory 1: ssdf-process
Arbiter: SSDF-PW

PW.1 (secure coding — credential management): REPROCESS — production database
password hardcoded in application source code and committed to version control.
Credential exposed to all repository access paths and retrievable from 11 months
of version control history regardless of future rotation. Credential must be
rotated immediately; repository history must be audited.

Territory 2: cwe-authn
Arbiter: CWE-Authn

CWE-798 (Use of Hard-coded Credentials): REPROCESS — CRITICAL
The hardcoded credential is used in the authentication module for database access.
CWE-798 applies when a software product contains hard-coded credentials for a
critical resource (authentication, database access, or equivalent). An attacker
with repository read access can extract the credential and authenticate directly
to the production database, bypassing application-layer controls entirely.
Severity: CRITICAL — direct authentication to production user accounts database
using an exposed credential represents complete data access compromise.

Overall verdict: REPROCESS — CRITICAL
Reason: SSDF PW.1 and CWE-798 independently flag the same hardcoded credential
from different analytical frameworks. CWE-798 CRITICAL classification reflects
the direct database authentication risk.
```

**Reason:** SSDF PW.1 flags the practice gap (credentials must not be in source). CWE-798 flags the weakness pattern (hardcoded credential in an authentication-adjacent component). Same evidence, different frameworks, same conclusion. CWE-798 warrants CRITICAL because the hardcoded credential grants direct database authentication — not just an indirect information disclosure.

**Phrasing-B:** We found `DB_PASSWORD = "Pr0d_DB_S3cr3t#2024"` hardcoded in app/config.py for the user auth database. It's been there for 11 months. Both SSDF and CWE perspectives needed. How bad is this?

---

## Pair[CB4-3.4-268]

**A:** PKCE implementation review. Authorization requests use PKCE with `code_challenge_method=plain`. The code_challenge value equals the raw code_verifier. The team's argument: "RFC 7636 defines both plain and S256 as valid methods. Since the RFC allows plain, our implementation is RFC-compliant and ASVS-compliant."

**B:**
```
Territory 1: oauth-oidc
Arbiter: ASVS-V10-OAuth

V10.4.6: REPROCESS — PKCE code_challenge_method MUST be S256. The plain
method does not provide meaningful protection: code_challenge == code_verifier,
so observing the authorization request reveals the code_verifier immediately.
PKCE's protection depends on the verifier being unrecoverable from the
challenge; plain defeats this property.

Territory 2: rfc-oauth2
Arbiter: RFC-OAuth2

RFC 7636 (PKCE): REPROCESS
Argument rejected: "RFC 7636 defines both plain and S256 as valid methods."
RFC 7636 Section 4.2 states: "If the client is capable of using 'S256', it MUST
use 'S256'". The plain method is a fallback for clients that cannot support S256,
not a coequal alternative. A client capable of constructing a SHA256 hash (any
modern platform) MUST use S256; plain is only for legacy constrained environments
that cannot perform hashing. The argument that RFC 7636 "allows" plain misreads
the conditional: plain is permitted only when S256 is not available.

Overall verdict: REPROCESS
Reason: Both ASVS V10.4.6 and RFC 7636 require S256 when available. The
"both are valid" argument misreads RFC 7636 Section 4.2's conditional.
```

**Reason:** RFC 7636 Section 4.2's MUST-use-S256-if-capable language is often misread as making plain and S256 coequal options. The RFC establishes a priority: S256 is required when available (which it is for any standard platform); plain is a fallback for constrained environments. Both ASVS and RFC produce REPROCESS, and the RFC argument is explicitly rejected at Node 08.

**Phrasing-B:** Our PKCE implementation uses code_challenge_method=plain. RFC 7636 documents both plain and S256 — it defines both as method values. Since the RFC includes plain as a defined method, we're RFC-compliant. Does ASVS require S256 over what RFC 7636 allows?

---

## Pair[CB4-3.4-269]

**A:** Password security review. Passwords are hashed with bcrypt, cost factor 4. Security testing (PW.7) was performed last quarter — a penetration test assessed the authentication endpoints. The penetration test report shows no critical findings on the authentication module. The bcrypt cost factor was not included in the penetration test scope.

**B:**
```
Territory 1: asvs-authentication
Arbiter: ASVS-V6-Auth

V6.2.5 (memory-hard hashing): CONCERN — bcrypt is an appropriate memory-hard
algorithm. Cost factor 4 is significantly below current recommendations
(≥10 recommended, ≥12 typical). Offline attack resistance is substantially
weaker than an appropriately configured deployment. Algorithm class correct;
configuration inappropriate.
Territory verdict: CONCERN.

Territory 2: ssdf-process
Arbiter: SSDF-PW

PW.7 (security testing): CONCERN — security testing (penetration test) was
performed. However, the bcrypt cost factor configuration was not within scope.
Security testing that does not assess password storage configuration leaves a
gap in the security assurance provided by PW.7. The penetration test clearance
on authentication endpoints does not validate the password hashing configuration.
Note: a penetration test clearance on authentication endpoints is not equivalent
to a finding of "no password storage vulnerability." Scope limitations define
what was assessed.
Territory verdict: CONCERN.

Overall verdict: CONCERN
Reason: Both territories CONCERN. Two security assurance gaps: algorithm
misconfigured (V6.2.5 CONCERN) and security testing did not cover the gap
(PW.7 CONCERN). Neither rises to REPROCESS level individually; combined they
represent a meaningful assurance gap requiring attention.
```

**Reason:** Two CONCERN findings from different territories. V6.2.5 CONCERN for bcrypt cost 4 (correct algorithm, wrong configuration). PW.7 CONCERN because the penetration test's scope excluded the password hashing configuration — the pen test clearance covers only what was in scope. Both are CONCERN rather than REPROCESS; overall verdict CONCERN (no REPROCESS to override).

**Phrasing-B:** bcrypt cost 4 for passwords, pen test done last quarter on auth endpoints with no critical findings. The pen test didn't specifically check the bcrypt cost parameter. ASVS V6.2.5 and SSDF PW.7 both relevant here — what's the combined assessment?

---

## Pair[CB4-3.4-270]

**A:** Authorization and training review. Users have no way to query their own permissions — no permissions API, no UI role display. Separately, the operations team manages RBAC assignments through an admin console but has received no training on the authorization model, role definitions, or grant management procedures.

**B:**
```
Territory 1: asvs-authorization
Arbiter: ASVS-V8-AuthZ

V8.2.1 (authorization metadata accessible to users): CONCERN — no mechanism
exists for users to introspect their own permission set. Users discover access
through trial-and-error 403 responses. Authorization controls enforce correctly
(403 responses are correct); the transparency gap is the concern.
Territory verdict: CONCERN.

Territory 2: ssdf-process
Arbiter: SSDF-PW / SSDF-PO

SSDF PO.1 (security training — operations role): CONCERN — operations team
members who perform RBAC grant management have not received role-specific
training on the authorization model, role definitions, or grant procedures.
Incorrect grants (over-privileged assignments) are a direct risk when operators
manage access without understanding the model. PO.1 requires that personnel
with security-relevant responsibilities receive appropriate training.
Territory verdict: CONCERN.

Overall verdict: CONCERN
Reason: Two CONCERN findings — authorization transparency gap (V8.2.1) and
operator training gap for RBAC management (PO.1). Neither finding constitutes
a MUST-level control failure; both represent meaningful authorization assurance
gaps requiring remediation plans.
```

**Reason:** V8.2.1 is an authorization transparency requirement producing CONCERN (security controls enforce correctly; discoverability is the gap). SSDF PO.1 for ops personnel performing security-relevant RBAC management is also CONCERN — operators with grant authority who don't understand the model are a risk. Two CONCERN findings, overall CONCERN.

**Phrasing-B:** Two authorization questions: users can't see their own permissions (they get 403s when they try unauthorized things), and the ops team managing access assignments hasn't been trained on our RBAC model. Both V8.2.1 and PO.1 apply. What's the combined assessment?

---

## Pair[CB4-3.4-271]

**A:** Session termination security review. Application uses JWT stored in httpOnly cookies as session tokens. On logout, the cookie is deleted client-side. No server-side revocation list. If a session token is captured before logout, it remains valid. Security team argues: "This isn't really improper authentication — the token was legitimately issued. It's just that we don't have logout revocation."

**B:**
```
Territory 1: asvs-session
Arbiter: ASVS-V7-Session

V7.3.1: REPROCESS — server-side session invalidation on logout is required.
Client-side cookie deletion is not server-side invalidation.
Evidence gap: no server-side revocation on logout.

Territory 2: cwe-authn
Arbiter: CWE-Authn

CWE-287 (Improper Authentication): REPROCESS
Argument partially rejected: "token was legitimately issued, not improper auth."
Authentication establishes and maintains identity over a session. CWE-287
applies when an authentication mechanism can be circumvented, even if the
initial authentication was legitimate. A session token that remains valid after
explicit logout intent allows continued access that the user has explicitly
revoked. The authentication mechanism fails to honor termination — this is
the CWE-287 weakness: the system authenticates the identity using a token
that the user has signaled should no longer be accepted. The weakness is in
the authentication lifecycle management, not in the initial issuance.
Evidence gap: authentication via a session token continues to succeed after
the user's declared logout.

Overall verdict: REPROCESS
Reason: V7.3.1 REPROCESS and CWE-287 REPROCESS. The "legitimately issued"
argument addresses issuance only; authentication lifecycle management
(including logout) is within both standards' scope.
```

**Reason:** CWE-287 covers the full authentication lifecycle, not just the initial authentication event. A session that persists after explicit logout represents an authentication management failure — the system continues to authenticate access using a credential the user has declared invalid. The "legitimately issued" argument addresses only the initial auth event, not the post-logout lifecycle.

**Phrasing-B:** After logout we delete the cookie but the JWT stays valid — no revocation. Security team says this isn't really a CWE-287 issue since the token was legitimately obtained. V7.3.1 and CWE-287 assessment please.

---

## Pair[CB4-3.4-272]

**A:** OAuth authorization server compliance audit. Within the same server: PKCE is correctly implemented with S256 method and code_verifier validation. Redirect URIs are registered as wildcard patterns (`https://app-*.example.com/callback`) covering all app subdomains.

**B:**
```
Territory: oauth-oidc
Arbiter: ASVS-V10-OAuth

V10.4.6 (PKCE): PASS — code_challenge_method=S256, code_verifier validated
at token exchange. PKCE requirement satisfied.

V10.2.1 (redirect URI): REPROCESS — wildcard redirect URI pattern
https://app-*.example.com/callback is not exact registration. Wildcard
patterns permit authorization code interception via any subdomain matching
the pattern, including those not operated by the application owner. Exact
registration of each individual redirect URI is required.

Territory verdict: REPROCESS
Reason: V10.2.1 REPROCESS overrides V10.4.6 PASS within the same territory.
Sub-requirements in a single territory are evaluated independently. A PASS
on one sub-requirement does not compensate for a REPROCESS on another.

Overall verdict: REPROCESS
```

**Reason:** Both V10.4.6 and V10.2.1 are sub-requirements within the oauth-oidc territory evaluated by the same arbiter. PKCE passing does not compensate for redirect URI wildcard. Territory verdict is REPROCESS because any REPROCESS finding within a territory sets the territory verdict. This establishes the sub-requirement independence rule within a single territory.

**Phrasing-B:** Same OAuth server — PKCE is S256 and correctly validated (V10.4.6 PASS), but redirect URIs use wildcards (app-*.example.com). Does the PKCE PASS partially offset the wildcard redirect finding, or does one REPROCESS set the territory outcome regardless?

---

## Pair[CB4-3.4-273]

**A:** Source code security review. A hardcoded API key is found in `integrations/payment.py`: `PAYMENT_API_KEY = "sk_live_REDACTED_SECRET_REMOVED..."`. SAST runs on every PR (PW.4 satisfied). The SAST tool is configured with hardcoded-credential detection rules. The latest SAST scan did not flag this key — the tool's pattern matching did not match the specific payment provider's key format.

**B:**
```
Territory 1: ssdf-process
Arbiter: SSDF-PW

PW.1 (secure coding — credential management): REPROCESS — production payment
API key hardcoded in source code. The key is present in version-controlled
source and exposed to all repository access paths. That SAST did not detect
the key does not change the presence of the hardcoded credential.

PW.4 (SAST): PASS — automated static analysis with hardcoded-credential
detection rules runs on every PR. The practice is in place. That the tool
did not detect this particular key indicates a tool coverage gap (a PW.4
quality concern) but does not change the PW.4 practice assessment.

Note: PW.1 REPROCESS and PW.4 PASS are simultaneously valid. PW.4 evaluates
whether automated analysis is practiced; it does not certify that all
hardcoded credentials are detected. Tool detection coverage and practice
performance are different dimensions.

Overall verdict: REPROCESS
Reason: PW.1 REPROCESS overrides PW.4 PASS. SAST not detecting the key
is a tool coverage limitation, not a practice that satisfies PW.1. The
credential must be rotated and moved to a secrets manager.
```

**Reason:** PW.4 PASS and PW.1 REPROCESS coexist exactly as PW.4 PASS and PW.2 REPROCESS do — the practice was performed (PASS) and a specific violation exists (REPROCESS). PW.4 certifies that SAST is run; it does not certify absence of hardcoded credentials. Tool detection limitations are a quality concern within PW.4 but do not cause PW.4 to fail or PW.1 to pass.

**Phrasing-B:** SAST runs on every PR with credential detection rules — PASS. But it missed a hardcoded payment API key in integrations/payment.py. If SAST was configured and running and didn't find it, does that affect the PW.1 or PW.4 assessment?

---

## Pair[CB4-3.4-274]

**A:** Security review of a mature SaaS platform with comprehensive sub-domain controls implemented. OAuth redirect URIs are all exactly registered (no wildcards). Session tokens are properly revoked server-side on logout via a session store. PKCE uses S256 method. Passwords use bcrypt cost 12. Users have a `/me/permissions` endpoint to query their access. Source code contains no hardcoded credentials (verified by automated scan and manual review). Code review includes a security checklist.

**B:**
```
Territory 1: oauth-oidc
Arbiter: ASVS-V10-OAuth
V10.2.1 (redirect URI): PASS — all URIs exactly registered, no wildcards.
V10.4.6 (PKCE): PASS — S256 method, code_verifier validated.
Territory verdict: PASS.

Territory 2: asvs-session
Arbiter: ASVS-V7-Session
V7.3.1 (server-side invalidation on logout): PASS — session store revocation
on logout confirmed.
Territory verdict: PASS.

Territory 3: asvs-authentication
Arbiter: ASVS-V6-Auth
V6.2.5 (memory-hard hashing): PASS — bcrypt cost 12, appropriate configuration.
Territory verdict: PASS.

Territory 4: asvs-authorization
Arbiter: ASVS-V8-AuthZ
V8.2.1 (permission introspection): PASS — /me/permissions endpoint present.
Territory verdict: PASS.

Territory 5: ssdf-process
Arbiter: SSDF-PW
PW.1 (no hardcoded credentials): PASS — verified by automated scan and manual
review.
PW.3.2 (security-focused code review): PASS — security checklist in review
process.
Territory verdict: PASS.

Overall verdict: PASS
```

**Reason:** All new sub-domains introduced in ext10 are covered with positive baseline evidence. Each territory PASS is based on specific control verification. This establishes the baseline against which the individual REPROCESS/CONCERN findings in other pairs can be compared.

**Phrasing-B:** Mature SaaS with comprehensive controls: exact OAuth redirect URIs, server-side session revocation on logout, S256 PKCE, bcrypt cost 12, permissions API, no hardcoded credentials confirmed, security code review checklist. Show the multi-territory PASS assessment.

---

## File Summary

| Pair ID | Description | Stratum | Territories | Verdict |
|---------|-------------|---------|-------------|---------|
| CB4-3.4-265 | V10.2.1 wildcard + PW.2 pre-OAuth threat model; V10.4.6 PASS shown | B | oauth-oidc, ssdf-process | REPROCESS |
| CB4-3.4-266 | V7.3.1 logout + V10.3 refresh token revocation | A | asvs-session, oauth-oidc | REPROCESS |
| CB4-3.4-267 | SSDF PW.1 hardcoded cred + CWE-798 (hardcoded auth credential) | A | ssdf-process, cwe-authn | REPROCESS CRITICAL |
| CB4-3.4-268 | V10.4.6 plain PKCE + RFC 7636 S256-MUST argument rejected | B | oauth-oidc, rfc-oauth2 | REPROCESS |
| CB4-3.4-269 | V6.2.5 bcrypt cost 4 CONCERN + PW.7 pen test scope gap CONCERN | B | asvs-authentication, ssdf-process | CONCERN |
| CB4-3.4-270 | V8.2.1 no permission introspection + PO.1 ops RBAC training | B | asvs-authorization, ssdf-process | CONCERN |
| CB4-3.4-271 | V7.3.1 logout REPROCESS + CWE-287 "legitimately issued" rejected | B | asvs-session, cwe-authn | REPROCESS |
| CB4-3.4-272 | V10.4.6 PKCE PASS + V10.2.1 wildcard REPROCESS (same territory) | A | oauth-oidc | REPROCESS |
| CB4-3.4-273 | SSDF PW.1 hardcoded key REPROCESS + PW.4 PASS (SAST missed it) | B | ssdf-process | REPROCESS |
| CB4-3.4-274 | All new sub-domain 5-territory PASS baseline | A | oauth-oidc, asvs-session, asvs-authentication, asvs-authorization, ssdf-process | PASS |
