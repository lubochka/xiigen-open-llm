# CB4 Stage 3 — Node 08 Arbiter Set Extension 12
# Sequences CB4-3.4-309 through CB4-3.4-316
# Stratum: B — team context for new sub-domains; includes correct-scope scoping decisions
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 632 instances

---

## Pair[CB4-3.4-309]

**A:** OAuth redirect URI review. Redirect URIs registered as wildcard patterns (*.example.com). The team's response: "Our most recent penetration test covered the OAuth authorization flows and the report shows no critical findings on redirect URI handling. The pen tester didn't flag it."

**B:**
```
Arbiter: ASVS-V10-OAuth

V10.2.1: REPROCESS
Argument rejected: "penetration test cleared the wildcard redirect URI."

Penetration test clearance does not satisfy a normative requirement:
(1) A penetration test assesses exploitability within the scope and methodology
    chosen. A tester may not have found a specific exploit for the wildcard
    pattern during the engagement window — this does not mean the requirement
    is satisfied or that the vulnerability does not exist.
(2) V10.2.1 is a configuration requirement, not an exploitability threshold.
    The requirement is satisfied by exact registration, not by the absence of
    a demonstrated exploit.
(3) Subdomain takeover opportunities and path-confusion exploits may not be
    detectable in a point-in-time pen test that does not have access to
    the full subdomain inventory or ongoing monitoring.

A penetration test "no critical findings" result on a wildcard redirect URI
means the tester did not find a specific working exploit during the engagement.
It does not certify compliance with V10.2.1.

Evidence gap: wildcard redirect URI registered; exact registration required.
Overall verdict: REPROCESS
```

**Reason:** Penetration test clearance is evidence of exploitability within scope, not evidence of requirement satisfaction. V10.2.1 requires exact registration as a configuration control — the absence of a demonstrated exploit does not satisfy this. This is a common team argument pattern: "the pen test passed it" used to defer a configuration requirement.

**Phrasing-B:** Our pen test covered OAuth flows and the tester didn't find any issues with our wildcard redirect URIs. The report shows no critical findings on the authorization flow. Doesn't that mean the wildcard pattern is acceptable?

---

## Pair[CB4-3.4-310]

**A:** Session invalidation review. Application uses JWT sessions. A nightly batch job runs at 02:00 UTC that invalidates all session tokens issued more than 24 hours ago. The team argues: "We do perform server-side revocation — the batch job revokes all old sessions nightly. V7.3.1 requires server-side invalidation, and we have it."

**B:**
```
Arbiter: ASVS-V7-Session

V7.3.1 (server-side session invalidation on logout): REPROCESS
Argument rejected: "nightly batch revocation satisfies V7.3.1."

V7.3.1 requires server-side invalidation on logout — meaning that when a user
terminates their session by logging out, the server-side session is invalidated
at that moment. This is a per-logout action, not a scheduled maintenance task.

A nightly batch job that revokes sessions older than 24 hours:
(1) Does not revoke sessions at the time of logout — a user who logs out at
    10:00 PM has a session token that remains valid until the 02:00 AM batch
    runs (4 hours post-logout).
(2) Does not provide revocation for sessions younger than 24 hours — if a
    user logs in and logs out within the same day, the batch may not revoke
    the token at all if the token's creation timestamp is less than 24 hours old.
(3) Represents session expiry management, not logout-triggered invalidation.

V7.3.1 requires that logout triggers immediate server-side invalidation.
Batch-based expiry management is a separate operational practice that does not
satisfy the logout-triggered requirement.

Evidence gap: session tokens remain valid after logout until the next batch run.
Overall verdict: REPROCESS
```

**Reason:** V7.3.1 is a logout-triggered requirement — the server-side invalidation must occur as a consequence of the logout event. A nightly batch job is an expiry management mechanism, not logout-triggered invalidation. Between logout and the next batch execution (up to 24 hours), session tokens remain exploitable. The team's argument conflates batch expiry management with logout-triggered revocation.

**Phrasing-B:** We have a batch job at 02:00 AM that invalidates sessions older than 24 hours — that's server-side revocation. V7.3.1 says server-side invalidation, and we have it. Is nightly batch revocation sufficient for V7.3.1?

---

## Pair[CB4-3.4-311]

**A:** Credential management review. Application connects to external services using API keys stored as GitHub Actions secrets. Keys are referenced in workflow files as `${{ secrets.PAYMENT_API_KEY }}`. Key values are masked in CI/CD logs. Keys are not in any source code file or committed configuration. Key values exist only in the GitHub Actions secrets store.

**B:**
```
Arbiter: SSDF-PW

PW.1 (secure coding — credential management): PASS
Credentials stored as GitHub Actions secrets satisfy PW.1 for the following reasons:
(1) The credential values are not hardcoded in source code — workflow files
    reference them by name using the ${{ secrets.NAME }} template syntax.
(2) Credential values are stored in the CI/CD platform's secrets management
    facility, not in version-controlled configuration files.
(3) Values are masked in CI/CD logs, reducing accidental exposure risk.
(4) GitHub Actions secrets are encrypted at rest and access-controlled by
    repository permissions.

This pattern (CI/CD platform secrets with template injection) is an accepted
implementation of PW.1's intent: credentials managed outside of source code in
a purpose-built credential store.

Note: the security of this approach depends on proper GitHub repository access
controls. If the repository has inappropriately broad access, the credential
store's protections are limited to the platform's own controls.
Overall verdict: PASS
```

**Reason:** CI/CD platform secrets (GitHub Actions secrets, GitLab CI variables, etc.) that are not in source code and injected via template syntax satisfy PW.1. The credential is not hardcoded — the source code contains only the secret name reference. The key value lives in the platform's encrypted secrets store. This is a standard, accepted practice.

**Phrasing-B:** API keys in GitHub Actions secrets, referenced as `${{ secrets.KEY_NAME }}` in workflows. Not in source code, masked in logs, managed by GitHub. Does using CI/CD platform secrets satisfy SSDF PW.1?

---

## Pair[CB4-3.4-312]

**A:** PKCE coverage review. PKCE with S256 method is enforced on all authorization code flows. The application also supports OAuth 2.0 Device Authorization Grant (RFC 8628) for smart TV clients. PKCE is not implemented on the device flow — no code_challenge is included in device authorization requests. Team notes: "The device flow spec says PKCE is optional."

**B:**
```
Arbiter: ASVS-V10-OAuth

V10.4.6 (PKCE on authorization code flow): PASS — S256 PKCE correctly
implemented and enforced on all authorization code flow requests.

Device Authorization Grant (RFC 8628) PKCE: PASS — correct scoping.
V10.4.6 applies to authorization code flows that use code_challenge parameters.
RFC 8628 Device Authorization Grant does not use an authorization code in the
traditional redirect-based sense — the device flow uses a device_code and
user_code via a verification URI, which is a different grant type without the
same redirect interception attack surface that PKCE addresses.

PKCE was designed for public clients receiving authorization codes via redirect
URIs where the code can be intercepted in transit. The device flow's threat
model is different — the client polls for a token rather than receiving a code
via redirect. V10.4.6 does not impose PKCE on grant types for which PKCE's
protection mechanism is not applicable.

The team's statement that PKCE is optional for device flow aligns with the
applicable RFC and ASVS scoping.

Overall verdict: PASS
```

**Reason:** V10.4.6 PKCE requirement applies to authorization code flows — its security model addresses authorization code interception via redirect. Device Authorization Grant (RFC 8628) uses a fundamentally different flow (polling via device_code) that does not have the same interception attack surface. Correctly scoping PKCE to authorization code flows only is the right implementation; absence of PKCE on device flow is not a V10.4.6 violation.

**Phrasing-B:** PKCE S256 on all authorization code flows, but no PKCE on the device flow (smart TV). RFC 8628 doesn't require PKCE the same way. Is the absence of PKCE on device flow a V10.4.6 finding?

---

## Pair[CB4-3.4-313]

**A:** Password hashing assessment. Current implementation uses bcrypt cost 12 for all user passwords. The team is planning to migrate to Argon2id next quarter as part of a security modernization project. The migration plan includes lazy re-hashing on next login.

**B:**
```
Arbiter: ASVS-V6-Auth

V6.2.5 (memory-hard password hashing): PASS
bcrypt cost 12 is a correctly configured, approved memory-hard algorithm:
(1) bcrypt is a memory-hard password hashing function.
(2) Cost factor 12 (2^12 = 4096 iterations) meets current recommendations
    (OWASP recommends ≥10; cost 12 exceeds this minimum).

The planned migration to Argon2id is a security improvement but does not
affect the current compliance state. bcrypt cost 12 satisfies V6.2.5 today.
A planned upgrade to a stronger algorithm reflects good security practice and
indicates the team is tracking the evolving recommendations; it does not imply
that the current implementation is deficient.

V6.2.5 is currently satisfied.
Overall verdict: PASS
```

**Reason:** bcrypt cost 12 is a compliant V6.2.5 implementation. Planning to upgrade to Argon2id does not retroactively make bcrypt cost 12 non-compliant — the current state is assessed on its own merits. The planned migration is positive security hygiene, not remediation of a current gap. This is an important training case: proposed improvements do not create current compliance gaps.

**Phrasing-B:** Currently using bcrypt cost 12, planning to migrate to Argon2id next quarter. Does the fact that we're planning an upgrade imply that bcrypt cost 12 is currently non-compliant with V6.2.5?

---

## Pair[CB4-3.4-314]

**A:** Authorization transparency review. The application does not have a dedicated `/me/permissions` endpoint. Access tokens are JWTs containing role claims: `{"sub": "user123", "roles": ["editor", "commenter"]}`. Users receive this token on login. The frontend decodes the JWT (base64) to display the user's role in the UI. Users who use the API directly can decode their own JWT to see their role claims.

**B:**
```
Arbiter: ASVS-V8-AuthZ

V8.2.1 (authorization metadata accessible to users): PASS
V8.2.1 requires that users can access records of their own authorization
attributes. The mechanism does not need to be a dedicated API endpoint.

JWT access tokens with role claims satisfy V8.2.1 because:
(1) The JWT is issued to and held by the user — the user possesses the token
    that contains their authorization attributes.
(2) JWT tokens are base64url-encoded and trivially decodable by the user using
    standard tools or the frontend application.
(3) The payload claims (`roles`) directly represent the user's authorization
    attributes.
(4) The frontend already decodes and displays the role, providing the user
    with visible access to their own authorization state.

A user who holds a JWT containing their own role claims has access to their
own authorization attributes through the token itself.

V8.2.1 is satisfied through the JWT role claims mechanism.
Overall verdict: PASS
```

**Reason:** JWT tokens containing role claims are held by the user who was issued the token. The user can decode the base64url payload to read their own claims at any time. This satisfies V8.2.1's requirement for user access to their own authorization attributes — the mechanism is the token itself, not a separate endpoint. V8.2.1 does not mandate a specific endpoint type.

**Phrasing-B:** Access JWTs include role claims that the frontend decodes for the UI. Users can read their own JWT payload. No separate /me/permissions endpoint. Does user-held JWT with role claims satisfy V8.2.1 without a dedicated permissions endpoint?

---

## Pair[CB4-3.4-315]

**A:** Code review security assessment. All engineers have completed a 40-hour application security training course as part of onboarding. All PRs require at least one peer review before merge. However, there is no security-specific checklist, no designated Security Champion, and no structured process to ensure security concerns are explicitly addressed during review. The training is expected to inform reviewers' judgment.

**B:**
```
Arbiter: SSDF-PW

PW.3.2 (security-focused code review): CONCERN
Security training for all engineers is a positive foundation. However, security
training that informs general judgment does not by itself satisfy PW.3.2.

PW.3.2 requires security-focused examination during code review. The gap between
training and structured security review is significant:
(1) Security training provides knowledge; structured security review requires
    applying that knowledge systematically to every PR with security intent.
(2) Without a security checklist, coverage of security concerns depends on
    individual reviewer memory and focus during each PR — high variance, not
    systematic.
(3) Without a designated security reviewer (champion or equivalent), security
    review is implicit rather than explicit — any reviewer may prioritize
    functionality and assume another reviewer handled security.

Finding level: CONCERN (not REPROCESS). The review practice exists and the
engineering team has relevant training. The gap is that security examination
is not structured or designated — it may or may not occur on any given PR
depending on the reviewer's focus.

Remediation: add a security checklist to the PR template; or designate a
Security Champion role with explicit security review responsibility.
Overall verdict: CONCERN
```

**Reason:** Training alone does not satisfy PW.3.2. The distinction: training → knowledge exists; security checklist/designated role → security is systematically applied during review. Without structure, security review becomes implicit and inconsistent. CONCERN because the review practice exists and engineers have training — the gap is structural, not a complete absence of security awareness.

**Phrasing-B:** All engineers have 40 hours of security training. Peer review required on all PRs. No security checklist and no Security Champion role — we trust trained engineers to catch security issues naturally during review. Does the training satisfy PW.3.2?

---

## Pair[CB4-3.4-316]

**A:** JWT algorithm configuration review. Production environment: JWT service configured with allowlist `["RS256"]` only. Development environment: JWT service configured with allowlist `["RS256", "HS256"]` to simplify local testing with symmetric keys. The development service uses a different signing key and does not share infrastructure with production.

**B:**
```
Arbiter: ASVS-V9-Token

V9.1.1 (algorithm allowlist — production): PASS — production configuration
accepts only RS256. The production allowlist is correctly configured.

Development environment allowlist (HS256 included): CONCERN
The production system satisfies V9.1.1. The development environment concern
is architectural:
(1) HS256 in development with different keys is not a direct production
    vulnerability — the services are isolated.
(2) However, development configurations that are more permissive than production
    create risk of: (a) configuration drift where dev settings accidentally reach
    production, (b) developers writing code paths that expect HS256 acceptance
    which then create implicit assumptions in code deployed to production.
(3) Ideally, development environments should use the same algorithm configuration
    as production to avoid these risks.

Finding: CONCERN for the development environment configuration divergence.
Production V9.1.1 compliance: PASS.
Overall verdict: CONCERN (development environment configuration gap — production
assessment is PASS; overall finding elevated to CONCERN due to environment
divergence risk).
```

**Reason:** Production configuration satisfies V9.1.1 — RS256 only is the correct allowlist. The development environment permissiveness (adding HS256) is a CONCERN because environment divergence creates the risk of misconfiguration propagating to production and of code paths that implicitly depend on HS256 acceptance. This is CONCERN not REPROCESS because the current production state is correct — the risk is about configuration management discipline.

**Phrasing-B:** Production allows only RS256 in our JWT allowlist. Dev environment allows RS256 and HS256 for easier local testing, using different keys. Production is correctly configured. Is the dev environment permissiveness a V9.1.1 concern even if production is clean?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-309 | V10.2.1 "pen test cleared wildcard" — pen test ≠ requirement satisfaction | B | ASVS-V10-OAuth | REPROCESS |
| CB4-3.4-310 | V7.3.1 "nightly batch revocation" — batch ≠ logout-triggered | B | ASVS-V7-Session | REPROCESS |
| CB4-3.4-311 | PW.1 GitHub Actions secrets (${{ secrets.X }}) — PASS (platform secrets) | B | SSDF-PW | PASS |
| CB4-3.4-312 | V10.4.6 PKCE on code flow PASS; device flow (RFC 8628) excluded — PASS | B | ASVS-V10-OAuth | PASS |
| CB4-3.4-313 | V6.2.5 bcrypt cost 12, planning Argon2id upgrade — PASS (current compliant) | B | ASVS-V6-Auth | PASS |
| CB4-3.4-314 | V8.2.1 JWT role claims as authorization metadata — PASS (user holds token) | B | ASVS-V8-AuthZ | PASS |
| CB4-3.4-315 | PW.3.2 security training without checklist/champion — CONCERN | B | SSDF-PW | CONCERN |
| CB4-3.4-316 | V9.1.1 RS256 prod PASS + HS256 dev environment — CONCERN (divergence) | B | ASVS-V9-Token | CONCERN |
