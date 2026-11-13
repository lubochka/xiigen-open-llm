# CB4 Stage 3 — Node 08 Arbiter Set Extension 13
# Sequences CB4-3.4-335 through CB4-3.4-342
# Stratum: B/D — remaining team-context and adversarial patterns for new sub-domains
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 684 instances

---

## Pair[CB4-3.4-335]

**A:** OAuth redirect URI review. The team has registered exact URIs for the 5 most-used callback endpoints. For a long tail of less-common endpoints, a wildcard pattern (`https://app.example.com/oauth/*`) is registered to avoid maintaining a large list. Team: "The important paths are exactly registered. The wildcard only covers less-used internal callback paths."

**B:**
```
Arbiter: ASVS-V10-OAuth

V10.2.1: REPROCESS
A redirect URI registration set that contains a wildcard pattern does not
satisfy exact registration requirements, regardless of how many exact
registrations are also present.

The wildcard pattern https://app.example.com/oauth/* accepts any path under
/oauth/ on app.example.com. If any such path has open redirect behavior, logs
parameters, has XSS vulnerabilities, or can be reached by a third party,
the authorization code can be delivered to an unintended endpoint.

The "only covers less-used paths" argument does not mitigate the vulnerability:
(1) Less-used paths may not receive the same security attention as the primary
    paths (e.g., legacy paths, developer testing endpoints, undocumented handlers).
(2) An attacker targets the weakest endpoint in the pattern — the usage frequency
    of the legitimate callback endpoint is irrelevant to the attacker's choice.
(3) V10.2.1 requires all registered redirect URIs to be exact — partial exact
    registration with a wildcard remainder does not satisfy the requirement.

Evidence gap: wildcard pattern registered alongside exact URIs.
Overall verdict: REPROCESS
```

**Reason:** Partial exact registration (some exact, some wildcard) does not satisfy V10.2.1. The wildcard entries remain attack vectors regardless of how few legitimate uses they cover. The "less-used = lower risk" argument is rejected because attackers target the weakest point in the pattern, not the most-used one.

**Phrasing-B:** We registered exact URIs for our 5 primary callback paths, and a wildcard https://app.example.com/oauth/* for less-used internal paths so we don't need to maintain a long list. Most traffic uses the exact URIs. Does partial exact registration with a wildcard remainder satisfy V10.2.1?

---

## Pair[CB4-3.4-336]

**A:** Session invalidation review covering SSO. Application uses a central SSO service. On logout from the main application: the main application's session token is cleared (client-side cookie deletion). The SSO session (managed by the SSO service) is NOT terminated — users remain authenticated in the SSO service and can access linked applications without re-authentication. Team: "We clear the main app session. SSO is a separate system."

**B:**
```
Arbiter: ASVS-V7-Session

V7.3.1 (server-side session invalidation on logout): REPROCESS
Argument rejected: "SSO is a separate system, main app session cleared."

V7.3.1 requires that logout results in server-side session invalidation.
In an SSO architecture, a complete logout must include termination of the SSO
session, not only the application session. Clearing the application-level
cookie while leaving the SSO session active means:
(1) The user remains authenticated in the SSO service.
(2) Accessing any linked application automatically re-establishes the user's
    session via the SSO service without requiring re-authentication.
(3) An attacker who has access to the SSO context (e.g., on a shared device
    or browser) can access all linked applications after the user believes
    they have logged out.

V7.3.1's requirement for server-side session invalidation on logout encompasses
the full authentication state of the user in the context of the application's
authentication architecture. In an SSO architecture, this includes triggering
SSO session termination.

Evidence gap: main application session cleared client-side; SSO session not
terminated on logout.
Overall verdict: REPROCESS
```

**Reason:** V7.3.1 in an SSO context requires that the SSO session be terminated on logout, not just the application-level token. Clearing the application cookie while leaving the SSO session active means the user's logout is incomplete — they remain authenticated via the SSO service. The "SSO is separate" argument is rejected because the SSO session is part of the user's authentication state for the application.

**Phrasing-B:** Our main app logout clears the local session cookie. We don't call the SSO service's logout endpoint — that's a separate system we don't control fully. Users are still logged into SSO after our app logout. Is that a V7.3.1 concern?

---

## Pair[CB4-3.4-337]

**A:** Credential management review. Application uses Kubernetes for container orchestration. Database credentials are stored as a Kubernetes ConfigMap: `kubectl create configmap db-config --from-literal=password=S3cr3t!`. The ConfigMap is referenced in pod environment variables. Team: "It's not in source code — it's in the Kubernetes cluster configuration."

**B:**
```
Arbiter: SSDF-PW

PW.1 (credential management): REPROCESS
Argument rejected: "not in source code — in Kubernetes cluster configuration."

Kubernetes ConfigMaps are designed for non-sensitive configuration data, not
credentials. ConfigMaps are NOT encrypted:
(1) ConfigMap data is stored in etcd in plaintext (base64 encoding is not
    encryption — it is reversible without a key).
(2) ConfigMaps are readable by any process in the same namespace with
    appropriate RBAC, and RBAC for ConfigMaps is often broader than for Secrets.
(3) kubectl or API access to the cluster can read ConfigMap values trivially.

Kubernetes provides Secrets (not ConfigMaps) for sensitive data. Secrets can be
encrypted at rest when properly configured. The recommended approach for
production credentials is Kubernetes Secrets with etcd encryption, or an external
secrets management solution (Vault, AWS Secrets Manager, etc.).

The "not in source code" argument is correct but insufficient — PW.1 requires
that credentials are not in source code AND are stored in a credential management
system that provides appropriate protection. A ConfigMap provides neither access
control appropriate for credentials nor encryption.

Evidence gap: database password stored in Kubernetes ConfigMap (plaintext in etcd).
Overall verdict: REPROCESS
```

**Reason:** Kubernetes ConfigMaps are not encrypted and are designed for non-sensitive configuration. Storing credentials in a ConfigMap instead of source code is a partial improvement but does not satisfy PW.1's intent — credentials must be in a protected credential management system, not merely outside source code. The "not in source" argument addresses the first half of the requirement but not the protection requirement.

**Phrasing-B:** Database password in a Kubernetes ConfigMap — it's not in our source code, it's in the cluster config managed by our platform team. We treat cluster config as separate from application code. Does ConfigMap storage satisfy PW.1?

---

## Pair[CB4-3.4-338]

**A:** JWT algorithm configuration review. The JWT service uses a blocklist approach: alg=none is explicitly rejected, and HS256 is explicitly blocked. Any other algorithm is accepted. The service currently only issues RS256 tokens, but the blocklist approach means RS384, RS512, ES256, ES384, PS256, etc., would also be accepted without configuration.

**B:**
```
Arbiter: ASVS-V9-Token

V9.1.1 (algorithm allowlist): CONCERN
V9.1.1 requires that only algorithms in a configured allowlist are accepted.
A blocklist approach (reject specific algorithms, accept everything else) does
not satisfy an allowlist requirement.

Blocking alg=none and HS256 eliminates the two most commonly exploited algorithm
confusion attack vectors. However, blocklist-based approaches are inherently
incomplete:
(1) New algorithm values added to the JWT specification or introduced by libraries
    would be automatically accepted without configuration review.
(2) Unusual algorithm values (e.g., non-standard identifiers) might bypass the blocklist.
(3) The allowlist approach is explicitly safer: enumerate exactly what is accepted;
    reject everything else.

Finding level: CONCERN (not REPROCESS). The blocked algorithms are the highest-risk
ones (alg=none for signature bypass, HS256 for key confusion). The remaining
accepted algorithms (RS*, ES*, PS*) are asymmetric/secure. The risk from non-
allowlisted algorithms being accepted is lower than if HS256 were permitted.
However, the architectural approach (blocklist vs. allowlist) is incorrect and
should be remediated by switching to an explicit allowlist of accepted algorithms.

Evidence gap: algorithm selection uses blocklist, not allowlist.
Overall verdict: CONCERN
```

**Reason:** A blocklist of known-bad algorithms does not satisfy V9.1.1's allowlist requirement. CONCERN (not REPROCESS) because the specific blocked algorithms are the dangerous ones — the remaining accepted algorithms are all secure asymmetric options. The architectural flaw (blocklist instead of allowlist) is a meaningful gap that warrants CONCERN and remediation.

**Phrasing-B:** JWT service blocks alg=none and HS256 explicitly. Everything else is accepted — currently we only use RS256 but the blocklist doesn't restrict to that. Does blocking the dangerous algorithms satisfy V9.1.1, or is an allowlist specifically required?

---

## Pair[CB4-3.4-339]

**A:** PKCE validation review — partial implementation. Token exchange endpoint validates the code_verifier parameter only for length: verifier must be between 43 and 128 characters per RFC 7636 Section 4.1. The server does not compute SHA256(code_verifier) and compare it to the stored code_challenge. Any 43-128 character string is accepted as a valid verifier.

**B:**
```
Arbiter: ASVS-V10-OAuth

V10.4.6: REPROCESS
Length validation of code_verifier does not satisfy PKCE verifier validation.
RFC 7636 Section 4.6 requires the server to compute BASE64URL(SHA256(code_verifier))
and compare the result to the stored code_challenge. This is the cryptographic
binding between the authorization request and the token exchange.

Validating only the length of the code_verifier means:
(1) An attacker who obtains the authorization code can exchange it by submitting
    any 43-128 character string as the code_verifier — no knowledge of the actual
    verifier is required.
(2) The SHA256 hash relationship between code_verifier and code_challenge is never
    verified — the cryptographic proof is not computed.
(3) PKCE provides zero protection against code interception in this configuration.

Length validation confirms the code_verifier meets the format specification. It
does not perform the security verification. The security verification is the
hash comparison — without it, PKCE is not implemented.

Evidence gap: code_verifier validated for length only; SHA256 hash comparison
against stored code_challenge not performed.
Overall verdict: REPROCESS
```

**Reason:** Length validation of code_verifier checks only the format requirement (RFC 7636 Section 4.1). The security requirement (RFC 7636 Section 4.6) is the hash comparison. A length-only check allows any correctly-formatted verifier to succeed — the cryptographic binding is never enforced. This is a subtle partial implementation that satisfies the format check while missing the security function.

**Phrasing-B:** We validate that the code_verifier is between 43 and 128 characters per the RFC spec. We don't compute the SHA256 hash comparison against the code_challenge — is the length validation enough to satisfy V10.4.6?

---

## Pair[CB4-3.4-340]

**A:** V8.2.1 authorization transparency review. Permissions are managed via a permission service. Users cannot query their permissions directly. However, the application surfaces permission denial reasons in error responses: when a user gets a 403, the response includes `{"error": "insufficient_permission", "required": "admin:reports:read", "user_has": ["user:read", "user:write"]}`. Users learn their permissions through permission-denial error messages.

**B:**
```
Arbiter: ASVS-V8-AuthZ

V8.2.1 (authorization metadata accessible to users): CONCERN
Error message disclosure of missing permissions partially satisfies V8.2.1's
intent — users do learn something about their authorization state when they
encounter access denials.

However, V8.2.1 requires that users can access records of their own authorization
attributes. Error-message-based permission discovery has significant limitations:
(1) Users only learn about permissions they lack when they attempt unauthorized
    actions — they cannot proactively query what they are allowed to do.
(2) Users cannot get a comprehensive view of their full permission set without
    attempting every possible action.
(3) The mechanism requires users to experience access denials to discover their
    authorization state — not a designed access channel.

Finding level: CONCERN (not REPROCESS). The error messages provide meaningful
authorization state information at the point of denial. A full permission query
endpoint would better satisfy V8.2.1's intent. The current implementation
partially addresses the transparency requirement.

Overall verdict: CONCERN
```

**Reason:** Error message permission disclosure is a partial implementation of V8.2.1 — users learn about their authorization state through access denials but cannot proactively query their complete permission set. CONCERN because the mechanism provides real information (partial satisfaction) but doesn't deliver the full accessibility that V8.2.1 intends (access to their own authorization attributes on demand).

**Phrasing-B:** Our 403 responses include what permission is required and what the user currently has — e.g., `required: admin:reports:read, user_has: [user:read]`. Users can learn their permissions this way. Does error-message permission disclosure satisfy V8.2.1?

---

## Pair[CB4-3.4-341]

**A:** Code review security assessment. All PRs require review before merge. Security review is performed by an AI code review tool integrated into the PR pipeline — it analyzes code for OWASP Top 10 patterns and flags potential issues. Human reviewers see the AI tool's annotations but are not required to perform an independent security assessment. No human Security Champion or security checklist exists.

**B:**
```
Arbiter: SSDF-PW

PW.3.2 (security-focused code review): CONCERN
Automated AI code review tools can supplement human security review but do not
substitute for it. PW.3.2 requires security-focused code review — this implies
human judgment applied to security considerations.

AI code review tools (including OWASP Top 10 pattern detection) have the same
limitations as SAST tools: they detect patterns, not intent; they cannot evaluate
authorization logic design, trust boundary decisions, business logic security, or
context-specific security implications. They excel at syntactic/semantic pattern
matching.

The specific gap: human reviewers see AI annotations but are not required to
perform independent security assessment. This means security review coverage
depends on: (1) what the AI tool detects, and (2) whether human reviewers act
on AI annotations. There is no systematic human security assessment.

Finding level: CONCERN (not REPROCESS). The AI tool provides security signal —
this is better than purely functional peer review. However, the security review
component lacks the human judgment dimension that PW.3.2 intends.

Remediation: designate a human reviewer (Security Champion or equivalent) with
explicit responsibility for security-focused review, using the AI tool as a
supplement, not a replacement.

Overall verdict: CONCERN
```

**Reason:** Automated AI tools for security review are similar to SAST — they detect patterns but cannot substitute for human security judgment. PW.3.2 requires security-focused code review with human judgment. CONCERN (not REPROCESS) because the AI tool provides real security signal; the gap is the absence of required human security assessment and the dependence on AI coverage alone.

**Phrasing-B:** AI code review tool on all PRs analyzing for OWASP Top 10 patterns, annotations visible to reviewers. No human Security Champion. Does AI-driven security review satisfy PW.3.2, or is human security review specifically required?

---

## Pair[CB4-3.4-342]

**A:** scrypt password hashing assessment. Passwords hashed using scrypt with parameters: N=1024, r=8, p=1. The team chose scrypt as a "modern alternative to bcrypt" and N=1024 for performance. Team: "scrypt is a memory-hard function per V6.2.5."

**B:**
```
Arbiter: ASVS-V6-Auth

V6.2.5 (memory-hard password hashing): REPROCESS
scrypt is a memory-hard password hashing algorithm — the algorithm class is
correct. However, N=1024 is critically inadequate:

scrypt N parameter controls memory and CPU cost: memory requirement scales as
N × r × 128 bytes. With N=1024, r=8: memory = 1024 × 8 × 128 = 1 MB per hash.
Modern GPU memory allows thousands of simultaneous 1 MB hashing contexts per GPU.

OWASP recommendations for scrypt: N=32768 (minimum), r=8, p=1. This gives 32 MB
per hash, making GPU-parallel attacks significantly more expensive. N=1024 is
approximately 32× less expensive than the recommended minimum — providing
substantially weaker protection than the V6.2.5 requirement intends.

Unlike bcrypt cost 4 (wrong configuration, CONCERN), scrypt N=1024 is a severe
underconfiguration that provides near-negligible resistance to modern offline
attack hardware. Classification: REPROCESS (not CONCERN) because N=1024 provides
protection that is at least an order of magnitude below the recommended minimum,
rendering the memory-hard property largely ineffective.

The distinction from bcrypt cost 4: bcrypt cost 4 is insufficient but provides
some meaningful resistance. scrypt N=1024 is a configuration so far below
recommendations that it fails to provide the memory-hard protection the algorithm
is designed to deliver.

Evidence gap: scrypt N=1024 provides insufficient memory-hard protection.
Overall verdict: REPROCESS
```

**Reason:** scrypt N=1024 is a REPROCESS case (unlike bcrypt cost 4 which is CONCERN). The severity distinction: bcrypt cost 4 provides some resistance; scrypt N=1024 is an order of magnitude below the recommended minimum and provides near-negligible memory-hard protection in practice. The algorithm is correct (scrypt is memory-hard); the configuration is so far below recommendations that the memory-hard property is effectively absent.

**Phrasing-B:** We use scrypt with N=1024, r=8, p=1. scrypt is a modern memory-hard function — V6.2.5 compliance. We chose N=1024 for authentication performance. Is scrypt N=1024 sufficient for V6.2.5?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-335 | V10.2.1 partial exact + wildcard remainder "less-used paths" | B | ASVS-V10-OAuth | REPROCESS |
| CB4-3.4-336 | V7.3.1 SSO session not terminated on app logout | B | ASVS-V7-Session | REPROCESS |
| CB4-3.4-337 | PW.1 Kubernetes ConfigMap (plaintext in etcd) ≠ secrets manager | B | SSDF-PW | REPROCESS |
| CB4-3.4-338 | V9.1.1 blocklist (alg=none + HS256 blocked) — CONCERN not REPROCESS | D | ASVS-V9-Token | CONCERN |
| CB4-3.4-339 | V10.4.6 length-only verifier validation — SHA256 comparison absent | D | ASVS-V10-OAuth | REPROCESS |
| CB4-3.4-340 | V8.2.1 permission disclosure via 403 error messages — CONCERN (partial) | D | ASVS-V8-AuthZ | CONCERN |
| CB4-3.4-341 | PW.3.2 AI code review tool (no human security review) — CONCERN | B | SSDF-PW | CONCERN |
| CB4-3.4-342 | V6.2.5 scrypt N=1024 — REPROCESS (not CONCERN; order of magnitude below minimum) | D | ASVS-V6-Auth | REPROCESS |
