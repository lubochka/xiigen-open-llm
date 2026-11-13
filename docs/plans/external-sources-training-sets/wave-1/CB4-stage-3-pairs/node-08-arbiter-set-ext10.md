# CB4 Stage 3 — Node 08 Arbiter Set Extension 10
# Sequences CB4-3.4-257 through CB4-3.4-264
# Stratum: A — new sub-domain baselines (redirect URI, session logout, hardcoded creds,
#           PKCE plain method, permission introspection, bcrypt cost, code review security,
#           algorithm allowlist enforcement)
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 528 instances

---

## Pair[CB4-3.4-257]

**A:** OAuth 2.0 authorization server review. Client registration allows redirect URIs matching the pattern `https://*.example.com/callback` (wildcard subdomain). The team states this is necessary to support multiple product subdomains without registering each individually.

**B:**
```
Arbiter: ASVS-V10-OAuth

V10.2.1 (redirect URI validation): REPROCESS
Redirect URIs MUST be exactly registered — wildcard patterns are not permitted.
A wildcard subdomain pattern https://*.example.com/callback can be satisfied by:
(1) any subdomain the team currently operates (intended behavior),
(2) any subdomain controlled by an attacker who acquires a subdomain via
    subdomain takeover, expired DNS, acquired domain, or similar,
(3) any subdomain discoverable through a path-traversal or subdomain-confusion
    vulnerability on the main domain.

An attacker matching *.example.com can register as a redirect target and receive
the authorization code in the Location header or request URL. Since the attacker
receives the authorization code, token exchange is possible without the legitimate
user completing the flow.

The operational convenience of wildcard matching does not override the
security requirement. Each redirect URI for each product subdomain must be
explicitly registered.

Evidence gap: redirect URI registered as wildcard pattern *.example.com/callback.
Overall verdict: REPROCESS
```

**Reason:** V10.2.1 requires exact-match redirect URI registration. Wildcard patterns create open redirect vectors via subdomain takeover or similar. The operational argument (multiple subdomains) does not modify the MUST requirement — the solution is to register each subdomain explicitly or use a single callback URI.

**Phrasing-B:** Our OAuth server lets clients register redirect URIs with wildcard subdomains like *.example.com/callback so our five product subdomains can all receive OAuth callbacks without listing each one. Is the wildcard registration a compliance issue?

---

## Pair[CB4-3.4-258]

**A:** Web application session security review. Sessions are implemented using a JWT issued at login, stored in an httpOnly Secure cookie. Cookie expiry is 7 days. On logout, the application deletes the cookie from the browser (Set-Cookie with Max-Age=0). The server maintains no revocation list or session store — tokens are stateless. A JWT captured before logout remains technically valid for its remaining expiry window.

**B:**
```
Arbiter: ASVS-V7-Session

V7.3.1 (server-side session invalidation on logout): REPROCESS
Session logout MUST result in server-side invalidation of the session token. Deleting
the client-side cookie satisfies the client-side logout UX but does not constitute
server-side invalidation.

A stateless JWT with no server-side revocation has no mechanism to honor logout
intent. If the token was captured before logout — via logs, network interception,
browser extension, XSS, or simply a copied cookie value — the captured token remains
valid for the full remaining expiry window (up to 7 days post-logout) with no
mechanism for the server to reject it.

Mitigation requires one of: (1) maintaining a server-side session store where tokens
can be explicitly invalidated at logout; (2) maintaining a revocation list checked on
each request; (3) using very short-lived tokens with server-side session refresh.

Evidence gap: no server-side session invalidation on logout; stateless JWT with
7-day expiry remains valid after cookie deletion.
Overall verdict: REPROCESS
```

**Reason:** V7.3.1 requires server-side invalidation, not client-side cookie deletion. A stateless JWT has no inherent revocation path — the server cannot reject a previously issued, unexpired token without additional infrastructure. The 7-day expiry window means a pre-logout-captured token provides a significant post-logout exploitation window.

**Phrasing-B:** We use JWT in httpOnly cookies for sessions. Logout deletes the cookie. The JWT is stateless so we don't maintain a session store — there's nothing server-side to invalidate. If someone copies the cookie before logout, they'd have access for up to 7 days. Is this a V7.3.1 issue given we're using stateless JWTs?

---

## Pair[CB4-3.4-259]

**A:** Application source code audit. Database connection is established with: `connection_string = "postgresql://appuser:K3yP@55word!@db.prod.internal:5432/appdb"`. This string is present in `config/database.py` and is committed to the repository's main branch history.

**B:**
```
Arbiter: SSDF-PW

PW.1 (secure coding practices — credential management): REPROCESS
Credentials MUST NOT be hardcoded in source code or committed to version control.
A production database password in application source code exposes the credential to:
(1) every developer with repository access (current and former),
(2) CI/CD systems that checkout the repository during builds,
(3) version control history — rotating the password removes it from the file but
    the credential remains retrievable from git history unless the commit is
    explicitly removed and history rewritten (which does not guarantee all
    forks/clones are updated),
(4) code hosting platform breaches or misconfigured repository access.

Remediation: remove the credential from source, store in an environment variable,
secrets manager (Vault, AWS Secrets Manager, etc.), or deployment-time injection.
The repository history must be audited and the credential must be rotated immediately
regardless of remediation status.

Evidence gap: production database password hardcoded in config/database.py,
committed to main branch.
Overall verdict: REPROCESS
```

**Reason:** PW.1 requires that credentials are not hardcoded in source code. Hardcoded production credentials in version-controlled source expose the credential to all repository access paths, including historically — rotation of the credential does not retroactively remove it from git history. Immediate rotation and secret manager migration are required.

**Phrasing-B:** We found a hardcoded DB password in config/database.py — `postgresql://appuser:K3yP@55word!@db.prod.internal/appdb` — which has been in the repo for 8 months. It works and the app uses it. We know it needs to be moved to env vars eventually. How urgent is this?

---

## Pair[CB4-3.4-260]

**A:** OAuth 2.0 server PKCE review. The server supports PKCE and all authorization requests include a code_challenge. However, code_challenge_method is set to `plain` on all requests — the code_challenge value is the raw code_verifier string without hashing.

**B:**
```
Arbiter: ASVS-V10-OAuth

V10.4.6 (PKCE code_challenge_method): REPROCESS
PKCE code_challenge_method MUST be S256. The plain method is not an acceptable
PKCE implementation for the following reason:

With method=plain, code_challenge == code_verifier. The code_challenge is
transmitted in the authorization request URL, which appears in: server logs,
browser history, HTTP Referer headers, and network traffic. If the authorization
request is observed, the code_verifier is immediately known — it is the same value.
An attacker who observes the authorization request can then exchange the
authorization code using the known code_verifier.

With method=S256, code_challenge = BASE64URL(SHA256(code_verifier)). Observing
the code_challenge does not reveal the code_verifier because SHA256 is one-way.
The attacker cannot reconstruct the verifier from the challenge.

Plain method provides no meaningful protection against the authorization code
interception attack that PKCE is designed to prevent. Presence of a
code_challenge with method=plain does not satisfy V10.4.6.

Evidence gap: code_challenge_method=plain on all authorization requests.
Overall verdict: REPROCESS
```

**Reason:** PKCE with method=plain transmits the verifier (as the challenge) in the authorization request, defeating the purpose of PKCE. V10.4.6 requires method=S256. The presence of a code_challenge field does not satisfy V10.4.6 when the method is plain — what matters is that the verifier cannot be derived from the challenge, which plain explicitly prevents.

**Phrasing-B:** We implemented PKCE — every authorization request has a code_challenge and code_verifier is validated at exchange. We used code_challenge_method=plain because it's simpler to implement. Does ASVS V10.4.6 require specifically S256, or is plain acceptable?

---

## Pair[CB4-3.4-261]

**A:** Authorization system review. The system implements resource-level RBAC (Role-Based Access Control) with fine-grained permissions. Users have no mechanism to query their own permissions — there is no `/me/permissions` endpoint, no `X-Permitted-Actions` response header, and no UI element showing access rights. Users discover what they can access by attempting actions and receiving 403 responses on unauthorized resources.

**B:**
```
Arbiter: ASVS-V8-AuthZ

V8.2.1 (authorization metadata accessible to users): CONCERN
V8.2.1 requires that users can access records of their own authorization attributes
so they can understand what they are and are not permitted to do. Discovery of
permissions through 403-error trial-and-error is a violation of this principle.

Finding level: CONCERN (not REPROCESS). Reasoning:
(1) The authorization controls themselves are correctly enforced — 403 responses
    are correct and unauthorized access is denied. The security function is present.
(2) V8.2.1 is an authorization transparency and user experience requirement, not
    a security control that prevents unauthorized access.
(3) The gap creates operational problems: users cannot verify their access level,
    administrators cannot confirm grants are correct, and the user experience
    degrades for any role-limited user navigating the product.

Remediation: implement a /me/permissions endpoint returning the caller's current
permission set, or equivalent mechanism (response headers, UI reflection).

Evidence gap: no mechanism for users to query their own authorization attributes.
Overall verdict: CONCERN
```

**Reason:** V8.2.1 is an authorization transparency requirement — users must be able to introspect their own permissions. This is CONCERN not REPROCESS because the security controls themselves (403 enforcement) are functioning correctly. The gap is discoverability and operational clarity, not access control failure.

**Phrasing-B:** Our RBAC system is working well — unauthorized access attempts get 403 responses. Users don't have a way to query their permissions; they find out by trying things. We haven't built a permissions endpoint. Is this a REPROCESS-level gap or just a nice-to-have?

---

## Pair[CB4-3.4-262]

**A:** Password storage security review. Passwords are hashed using bcrypt. Bcrypt cost factor (work parameter) is set to 4. The team's response: "We use bcrypt which is a memory-hard algorithm — this satisfies V6.2.5. The cost factor was set to 4 to keep authentication response times fast."

**B:**
```
Arbiter: ASVS-V6-Auth

V6.2.5 (memory-hard password hashing): CONCERN
bcrypt is an appropriate memory-hard algorithm for V6.2.5 — the algorithm class
is correct. However, bcrypt cost factor 4 is significantly below contemporary
recommendations.

bcrypt cost factor determines the number of iterations: 2^cost. At cost 4 (16
iterations), modern hardware can attempt hundreds of thousands of bcrypt hashes
per second in offline attack scenarios. OWASP and NIST recommendations converge
on bcrypt cost ≥ 10 as a minimum for current hardware; cost ≥ 12 is typical in
contemporary deployments. Cost 4 reduces the offline attack resistance to a
fraction of what an appropriate configuration would provide.

Finding level: CONCERN (not REPROCESS). Reasoning:
(1) bcrypt is a memory-hard algorithm — the requirement for algorithm class is met.
(2) The cost factor configuration is inappropriately low, substantially reducing the
    protection that the algorithm is designed to provide.
(3) This is a configuration gap in an otherwise correct choice, warranting CONCERN.
    If the algorithm were not memory-hard (e.g., MD5+salt), that would be REPROCESS.

Remediation: increase bcrypt cost factor to at least 10 (preferably 12+); test
authentication response time at higher cost factors with planned hardware.

Evidence gap: bcrypt cost factor 4 is insufficient for current hardware threat model.
Overall verdict: CONCERN
```

**Reason:** V6.2.5 requires memory-hard hashing (algorithm class) AND appropriate configuration. bcrypt cost 4 uses the correct algorithm family (CONCERN, not REPROCESS) but an inadequate cost parameter. The distinction is critical: wrong algorithm = REPROCESS; correct algorithm, wrong configuration = CONCERN. Authentication response time is a legitimate concern but does not override security configuration requirements.

**Phrasing-B:** We use bcrypt for passwords — that's memory-hard per V6.2.5. We set cost factor 4 to keep logins fast. The V6.2.5 requirement is for memory-hard functions, and we have one. Is cost factor 4 an issue?

---

## Pair[CB4-3.4-263]

**A:** Code review process assessment. All PRs require at least one peer reviewer before merge (enforced by branch protection). Reviewers are instructed to check functionality, readability, and test coverage. No security-specific checklist, training module, or review guidance exists for reviewers. The team operates on the assumption that SAST will catch security issues.

**B:**
```
Arbiter: SSDF-PW

PW.3.2 (security-focused code review): CONCERN
SSDF PW.3.2 requires that code reviews include security-focused examination.
Peer review for functionality, readability, and test coverage is PW.3.1-adjacent
practice but does not satisfy the security dimension of PW.3.

SAST (PW.4) does not substitute for human security judgment in code review:
(1) SAST detects pattern-level vulnerabilities; it does not evaluate authorization
    design, trust boundary decisions, or context-specific security logic.
(2) Logic-level vulnerabilities (IDOR, authorization flow gaps, business logic
    bypass) are typically beyond SAST detection capability.
(3) PW.3.2 specifically requires human review with security intent — the practice
    requires human judgment applied to security considerations, not a tool substitute.

Finding level: CONCERN (not REPROCESS). Reasoning:
(1) A code review practice is in place — the review process exists.
(2) The security component is absent from the existing practice.
(3) Security-focused review is a "should include" practice within PW.3 — the gap
    degrades security assurance but the overall practice category is present.

Remediation: add a security checklist to the PR review template; provide reviewer
training on common security review patterns; designate security review responsibility
per PR.

Evidence gap: code review exists but has no security-focused component; SAST
dependency does not satisfy PW.3.2.
Overall verdict: CONCERN
```

**Reason:** PW.3.2 requires security-focused human review, not just any peer review. A review process that explicitly focuses on functionality and style, with security delegated to SAST, does not satisfy PW.3.2. CONCERN because the review practice exists — the gap is the security dimension within the existing practice.

**Phrasing-B:** We require peer review on all PRs — at least one reviewer before merge. Our reviewers focus on functionality and code quality. We rely on SAST for security issues. We haven't given reviewers security-specific guidance because the tools handle that. Does this satisfy PW.3?

---

## Pair[CB4-3.4-264]

**A:** JWT validation service algorithm allowlist assessment. The service is configured with an allowlist: `acceptable_algorithms = ["RS256"]`. The implementation checks `if token_header.alg == "RS256"` — a string equality check. A token with `"alg":"RS384"` passes because the check does not verify membership in the allowlist set; it only verifies equality with "RS256". RS384 tokens from the same issuer are accepted.

**B:**
```
Arbiter: ASVS-V9-Token

V9.1.1 (algorithm allowlist enforcement): CONCERN
V9.1.1 requires that only algorithms in a configured allowlist are accepted.
The implementation flaw — checking equality with RS256 rather than set membership
— means RS384 tokens bypass the allowlist check.

RS384 is an RSA-SHA384 algorithm, a reasonable asymmetric signing algorithm.
The security concern is not that RS384 is inherently dangerous (it is not); the
concern is that the allowlist enforcement mechanism does not function as a
true allowlist.

Finding level: CONCERN (not REPROCESS). Reasoning:
(1) RS384 is not a dangerous or weak algorithm — this is not an algorithm confusion
    attack vector in the same class as HS256 with a public key.
(2) The allowlist intention is present; only the implementation is incorrect.
(3) The gap should be closed by fixing the comparison to set membership, but the
    immediate security risk is lower than an algorithm confusion attack.

Note: if the service accepted HS256 or alg=none through this same logic gap,
that would be REPROCESS CRITICAL. The specific bypassed algorithm (RS384) and its
security properties determine severity.

Evidence gap: allowlist check uses string equality rather than set membership;
RS384 accepted outside the configured allowlist.
Overall verdict: CONCERN
```

**Reason:** V9.1.1 requires allowlist enforcement — the mechanism must reject all algorithms not in the configured set. RS384 bypasses the check due to an implementation flaw. CONCERN (not REPROCESS) because RS384 is a secure asymmetric algorithm; the risk is the allowlist mechanism being ineffective, not the specific bypassed algorithm being dangerous. If the bypass admitted HS256 or alg=none, it would be REPROCESS CRITICAL.

**Phrasing-B:** Our JWT service has an allowlist configured as RS256 only. The check is `if alg == "RS256"` — equality comparison. We found RS384 tokens are accepted because the check passes RS384 through (it only rejects non-RS256 via the condition logic). RS384 is still RSA-based. Is this a blocker given RS384 is a secure algorithm?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-257 | V10.2.1 redirect URI wildcard *.example.com | A | ASVS-V10-OAuth | REPROCESS |
| CB4-3.4-258 | V7.3.1 stateless JWT no server-side revocation on logout | A | ASVS-V7-Session | REPROCESS |
| CB4-3.4-259 | SSDF PW.1 hardcoded DB password in source code | A | SSDF-PW | REPROCESS |
| CB4-3.4-260 | V10.4.6 PKCE method=plain (not S256) | A | ASVS-V10-OAuth | REPROCESS |
| CB4-3.4-261 | V8.2.1 no permission introspection for users | A | ASVS-V8-AuthZ | CONCERN |
| CB4-3.4-262 | V6.2.5 bcrypt cost factor 4 — algorithm correct, config insufficient | A | ASVS-V6-Auth | CONCERN |
| CB4-3.4-263 | SSDF PW.3.2 peer review exists, no security-focused component | A | SSDF-PW | CONCERN |
| CB4-3.4-264 | V9.1.1 allowlist check via string equality, RS384 bypasses | A | ASVS-V9-Token | CONCERN |
