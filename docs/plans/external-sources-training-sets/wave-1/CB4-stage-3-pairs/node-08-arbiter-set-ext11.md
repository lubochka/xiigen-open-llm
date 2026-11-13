# CB4 Stage 3 — Node 08 Arbiter Set Extension 11
# Sequences CB4-3.4-283 through CB4-3.4-290
# Stratum: D — adversarial arguments for new sub-domains; includes acceptance cases
#           where team's approach is actually correct (important negative-of-negative training)
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 580 instances

---

## Pair[CB4-3.4-283]

**A:** OAuth authorization server redirect URI implementation. The server validates redirect URIs by checking `if provided_uri.startsWith(registered_prefix)`. Registered prefix for a client is `https://app.example.com/`. Redirect URIs beginning with `https://app.example.com/` are accepted — including `https://app.example.com/callback`, `https://app.example.com/auth/done`, and any other path under that prefix.

**B:**
```
Arbiter: ASVS-V10-OAuth

V10.2.1 (redirect URI exact match): REPROCESS
Prefix matching does not satisfy exact registration. V10.2.1 requires that
redirect URIs be exactly registered — meaning the provided URI must equal
an entry in the registered set, not merely begin with a registered prefix.

Prefix matching (startsWith) allows any URI with the matching prefix to be used
as a redirect target, not just the intended callback endpoint. An attacker who
can register or control any path under the prefix domain can receive the
authorization code. This vulnerability class is known as open redirect via
path confusion — the registered prefix https://app.example.com/ does not
restrict the redirect to the intended endpoint at https://app.example.com/callback.

Example exploit: if app.example.com has any open redirect, XSS, or path-serving
functionality, an attacker can redirect to https://app.example.com/redirect?url=attacker.com
and receive the authorization code in the Referer header or logs.

Exact match means: the provided URI must be byte-for-byte identical to one of the
explicitly registered URIs. startsWith is not exact match.

Evidence gap: redirect URI validated via prefix match, not exact character comparison.
Overall verdict: REPROCESS
```

**Reason:** Prefix matching is a commonly believed equivalent to exact matching but is not. V10.2.1 exact registration means the provided URI must match a registered entry exactly. Any form of pattern matching (prefix, suffix, domain-only, wildcard) that accepts URIs beyond the specifically registered values violates V10.2.1. The "startsWith our domain" argument is rejected.

**Phrasing-B:** Our redirect URI validation checks if the provided URI starts with the registered prefix (https://app.example.com/). Any URI path under our domain is accepted — this restricts redirects to our own domain. Is prefix matching sufficient for V10.2.1's exact registration requirement?

---

## Pair[CB4-3.4-284]

**A:** JWT session management review. Session JWTs have a 5-minute expiry. On logout, the client cookie is deleted but no server-side revocation mechanism exists — the JWT remains technically valid for up to 5 minutes post-logout. Team argument: "The 5-minute window is so short that revocation adds unnecessary infrastructure complexity. No attacker can meaningfully exploit a 5-minute window."

**B:**
```
Arbiter: ASVS-V7-Session

V7.3.1 (server-side session invalidation on logout): CONCERN
The 5-minute expiry is a significant mitigating factor that substantially reduces
the exploitation window compared to longer-lived tokens. This reduces the severity
assessment from REPROCESS to CONCERN.

Argument partially accepted: the 5-minute window substantially reduces exploitability.
A 5-minute post-logout valid token significantly limits the attack surface:
(1) the attacker must act within the 5-minute window,
(2) no long-lived token capture provides extended access.

Argument rejected: "no attacker can meaningfully exploit a 5-minute window."
(1) Automated attack tools operate in milliseconds; 5 minutes is ample time.
(2) Incident response to a discovered token capture takes longer than 5 minutes.
(3) V7.3.1 requires server-side invalidation as a control, not just a low
    exploitation probability.

Finding level: CONCERN (not REPROCESS). Reasoning:
(1) V7.3.1 server-side invalidation requirement is not fully satisfied.
(2) The short expiry is a meaningful compensating control that reduces risk.
(3) The architectural constraint (stateless JWT) is a known trade-off; the
    implementation makes a reasonable risk reduction decision.
(4) CONCERN is appropriate when a control is partially mitigated but not complete.

Overall verdict: CONCERN
```

**Reason:** Short expiry (5 minutes) is a genuine and significant mitigating factor — it substantially changes the severity of the V7.3.1 gap compared to a 7-day expiry. CONCERN (not REPROCESS) because the expiry provides meaningful risk reduction. However, V7.3.1's requirement for server-side invalidation still exists and the "5 minutes is too short to exploit" argument is rejected — automated tools and incident response timeframes do not support that claim.

**Phrasing-B:** JWT sessions with 5-minute expiry. Logout clears the cookie but the JWT could technically be used for up to 5 minutes. We feel server-side revocation is overkill for a 5-minute window. Is this V7.3.1 REPROCESS or something less severe given the short expiry?

---

## Pair[CB4-3.4-285]

**A:** Application credential management review. Application connects to a third-party API using a key retrieved at runtime: `api_key = os.environ.get('THIRD_PARTY_API_KEY')`. The key is not in source code. The environment variable is injected at container startup from AWS Secrets Manager via the deployment pipeline. The key is not present in any configuration file committed to version control.

**B:**
```
Arbiter: SSDF-PW

PW.1 (secure coding — credential management): PASS
The credential is not hardcoded in source code. The implementation follows the
recommended pattern for credential management:
(1) Source code references the credential via an environment variable name, not
    a hardcoded value.
(2) The credential value is stored in AWS Secrets Manager (a dedicated secrets
    management service).
(3) The credential is injected at container startup via the deployment pipeline.
(4) The credential is not present in version-controlled configuration files.

This approach satisfies PW.1: credentials are managed outside of source code,
stored in a purpose-built secrets management system, and injected at runtime.
PW.1 is satisfied.

Overall verdict: PASS
```

**Reason:** Environment variable retrieval from a proper secrets management system (AWS Secrets Manager, Vault, etc.) is the intended remediation for PW.1 hardcoded credential violations. The key is not in source code, not in version control, and is injected at runtime. This is a PASS case — training must include positive identification of correct patterns, not only rejection of incorrect ones.

**Phrasing-B:** API key managed via environment variable — `os.environ.get('THIRD_PARTY_API_KEY')`. The key value comes from AWS Secrets Manager injected at container startup. Not in source code, not in any committed config file. Does this satisfy SSDF PW.1?

---

## Pair[CB4-3.4-286]

**A:** OAuth PKCE implementation review. Authorization requests include `code_challenge` with method S256 — the challenge is correctly computed as BASE64URL(SHA256(code_verifier)). However, at the token exchange endpoint, the server does not validate the `code_verifier` parameter — it accepts token exchange requests with or without a valid code_verifier, as long as the authorization code is valid.

**B:**
```
Arbiter: ASVS-V10-OAuth

V10.4.6 (PKCE): REPROCESS
PKCE requires both components: (1) the code_challenge must be correctly computed
and included in the authorization request, AND (2) the code_verifier must be
validated at token exchange. Both are required for PKCE to provide its intended
protection.

Including a correct S256 code_challenge without validating code_verifier at
exchange means PKCE provides zero protection: the authorization code can be
exchanged by any party, with any (or no) code_verifier, bypassing the binding
between the code and the entity that initiated the authorization request.

The security property of PKCE is that only the entity that computed
code_verifier (and thus knows the pre-image of code_challenge) can exchange the
authorization code. Without verifier validation at exchange, this binding is
never enforced — any party with the authorization code can exchange it.

Evidence gap: code_verifier not validated at token exchange endpoint despite
S256 code_challenge included in authorization requests.
Overall verdict: REPROCESS
```

**Reason:** PKCE is only effective when both the challenge (authorization request) and verifier (token exchange) components are enforced. A server that includes a correct S256 challenge but skips verifier validation at exchange has PKCE in name only — the code can be exchanged by any party. V10.4.6 requires the complete PKCE flow.

**Phrasing-B:** PKCE is implemented — authorization requests include correct S256 code_challenge values. At the token exchange endpoint, we don't validate the code_verifier (it's optional in our implementation). The challenge is there on the way in. Is not validating the verifier on the way out a problem?

---

## Pair[CB4-3.4-287]

**A:** Password hashing implementation review. Passwords are hashed using Argon2id with parameters: memory cost m=65536 (64 MB), time cost t=3 iterations, parallelism p=4 threads. The team states this configuration was chosen based on OWASP password hashing recommendations.

**B:**
```
Arbiter: ASVS-V6-Auth

V6.2.5 (memory-hard password hashing): PASS
Argon2id is a memory-hard password hashing algorithm and the winner of the
Password Hashing Competition (PHC). It is a recommended algorithm for V6.2.5.

The configured parameters satisfy contemporary recommendations:
- m=65536 (64 MB memory): substantial memory requirement per hash, making
  GPU-parallel attacks expensive in terms of memory bandwidth.
- t=3 (3 iterations): sufficient time cost to slow computation without
  excessive authentication latency.
- p=4 (4 parallel threads): appropriate parallelism for server hardware.

OWASP password hashing cheat sheet recommends Argon2id with minimum
m=19456 (19 MB), t=2, p=1; this implementation exceeds those minimums.

V6.2.5 is satisfied: memory-hard algorithm with appropriate work factors.
Overall verdict: PASS
```

**Reason:** Argon2id with m=65536, t=3, p=4 is a correctly configured memory-hard function that satisfies V6.2.5. This is a PASS case — the model must recognize correct implementations and produce PASS verdicts, not only identify gaps. Training must not bias toward always finding problems when the implementation is genuinely correct.

**Phrasing-B:** We use Argon2id for password hashing: m=65536, t=3, p=4. Based on OWASP recommendations. Does this satisfy ASVS V6.2.5?

---

## Pair[CB4-3.4-288]

**A:** Authentication credential handling review. The payment service authenticates to an external API using: `api_key = os.environ["PAYMENT_SERVICE_KEY"]`. The key value is provided via the container orchestration platform's secret injection mechanism (Kubernetes Secret mounted as environment variable). The key is not present anywhere in the source code repository.

**B:**
```
Arbiter: CWE-Authn / SSDF-PW

CWE-798: NOT PRESENT — the code does not contain a hard-coded credential.
Reading a value from an environment variable is not a hard-coded credential —
the credential value is not embedded in the program's source code. The program
references a name (the environment variable key), not a value.

SSDF PW.1: PASS — credential not hardcoded in source code. Environment
variable retrieval with platform-managed injection via Kubernetes Secrets
is an appropriate credential management pattern.

Note: the security of this approach depends on how the Kubernetes Secret is
managed (base64 is encoding, not encryption; Secrets at rest should be
encrypted at the cluster level). However, the source code practice satisfies
PW.1 and does not exhibit CWE-798.

Overall verdict: PASS
```

**Reason:** Reading from os.environ is definitively not a hardcoded credential — the code contains a key name, not a key value. CWE-798 requires the credential value to be present in the source code. The correct pattern (env var from secrets management) produces PASS for both PW.1 and CWE-798. This is an important positive training case: models must correctly classify correct patterns as PASS.

**Phrasing-B:** Payment service uses `api_key = os.environ["PAYMENT_SERVICE_KEY"]`. The actual key value comes from a Kubernetes Secret mounted as an env var. No key value in source. Does this satisfy PW.1, or is even the env-var reference a PW.1 concern?

---

## Pair[CB4-3.4-289]

**A:** Authorization transparency review. The application does not expose a `/me/permissions` API endpoint. However, every API response for a user-facing resource includes an `X-User-Roles` response header listing the caller's current roles (e.g., `X-User-Roles: reader,commenter`). Users can also see their role display in the account settings UI which fetches the same roles from a `/me/profile` endpoint that returns `{"roles": ["reader", "commenter"]}` in the JSON response body.

**B:**
```
Arbiter: ASVS-V8-AuthZ

V8.2.1 (authorization metadata accessible to users): PASS
V8.2.1 requires that users can access records of their own authorization
attributes. It does not mandate a specific implementation mechanism — it
requires that the capability exist.

The application provides two mechanisms through which users can access their
authorization attributes:
(1) X-User-Roles response header on user-facing API responses — machine-readable,
    accessible to API clients and frontends.
(2) /me/profile endpoint returning role array in the response body — explicit
    programmatic access to authorization metadata.
(3) UI role display in account settings — user-facing visual access.

V8.2.1 is satisfied through these combined mechanisms. A dedicated
/me/permissions endpoint is one implementation option; it is not the only
valid implementation. Any mechanism that allows users to query their own
authorization attributes satisfies the requirement.

Overall verdict: PASS
```

**Reason:** V8.2.1 requires that users can access their authorization attributes — it does not specify the mechanism. Response headers, profile endpoints with role data, and UI displays all satisfy the requirement. This is a PASS case that teaches the model not to over-specify the requirement (demanding a specific /me/permissions endpoint when other equivalent mechanisms exist).

**Phrasing-B:** No dedicated /me/permissions endpoint, but every API response includes an X-User-Roles header and the /me/profile JSON includes a roles array. The UI also shows roles in account settings. Does V8.2.1 require a specific permissions endpoint, or do these mechanisms satisfy it?

---

## Pair[CB4-3.4-290]

**A:** Code review security assessment. The engineering team has a designated Security Champion role assigned to a senior engineer. The Security Champion reviews all PRs for security concerns using an OWASP Top 10-aligned checklist. The Security Champion has completed a 40-hour application security training program. Non-security-champion reviewers focus on functionality and code quality.

**B:**
```
Arbiter: SSDF-PW

PW.3.2 (security-focused code review): PASS
PW.3.2 requires that code reviews include security-focused examination.
The Security Champion model satisfies this requirement:

(1) Designated role with security responsibility — the Security Champion is
    explicitly assigned to perform security-focused review, not just peer review.
(2) Security-specific checklist (OWASP Top 10-aligned) — reviews are structured
    around security concerns, not just functionality.
(3) Trained reviewer — 40-hour application security training establishes
    security competence for the review role.
(4) All PRs covered — the Security Champion reviews all PRs, providing
    systematic coverage.

PW.3.2 is satisfied: security-focused code review is performed systematically
by a trained, designated reviewer using a security checklist.

Overall verdict: PASS
```

**Reason:** The Security Champion model with a security checklist and trained reviewer satisfies PW.3.2. The requirement is for security-focused code review — designation, training, and a structured security checklist together deliver this. This is a PASS case that recognizes a common organizational pattern as valid, preventing the model from requiring impractical implementations (e.g., a formal security team reviewing every PR at a startup).

**Phrasing-B:** We have a Security Champion who reviews all PRs for security issues using an OWASP Top 10 checklist and has done 40 hours of security training. Regular reviewers handle functionality. Does the Security Champion model satisfy PW.3.2?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-283 | V10.2.1 "startsWith registered prefix" = prefix not exact | D | ASVS-V10-OAuth | REPROCESS |
| CB4-3.4-284 | V7.3.1 5-minute JWT expiry — CONCERN (significant mitigation) | D | ASVS-V7-Session | CONCERN |
| CB4-3.4-285 | PW.1 env var from Vault at deploy time — PASS (correct pattern) | D | SSDF-PW | PASS |
| CB4-3.4-286 | V10.4.6 S256 challenge sent, code_verifier not validated at exchange | D | ASVS-V10-OAuth | REPROCESS |
| CB4-3.4-287 | V6.2.5 Argon2id m=65536,t=3,p=4 — PASS (appropriate parameters) | D | ASVS-V6-Auth | PASS |
| CB4-3.4-288 | CWE-798 / PW.1 os.environ read — PASS (env var ≠ hardcoded) | D | CWE-Authn / SSDF-PW | PASS |
| CB4-3.4-289 | V8.2.1 roles in X-header + /me/profile — PASS (mechanism equivalent) | D | ASVS-V8-AuthZ | PASS |
| CB4-3.4-290 | PW.3.2 Security Champion model with OWASP checklist — PASS | D | SSDF-PW | PASS |
