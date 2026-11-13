# CB4 Stage 3 — Node Pipeline Chain Set Extension 7
# Sequences CB4-3.4-343 through CB4-3.4-350
# Stratum: C — full 9-node pipeline traces with intermediate outputs
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 700 instances

---

## Pair[CB4-3.4-343]

**A:** OAuth redirect URI review. Application registers exact redirect URIs for its five main client applications. A sixth entry in the redirect URI list is `https://*.docs.example.com/oauth/callback` — a wildcard for the documentation subdomain cluster. The team states: "Five out of six redirect URI registrations are exact. Only the docs cluster uses a wildcard because the docs subdomain changes with each release version (v1.docs.example.com, v2.docs.example.com, etc.). The other five clients are fully exact-registered."

**B:**
```
Node 1 — Domain Splitter
Input: OAuth redirect URI registration; five exact URIs + one wildcard for docs subdomain cluster.
Domains detected: [oauth-oidc]
Primary domain: oauth-oidc
Routing: single-domain

Node 2 — Domain Translator
Domain: oauth-oidc
Normative mapping:
  Redirect URI registration: ASVS V10.2.1 (exact redirect URI registration required)
  PKCE enforcement: V10.4.6
  State parameter: V10.2.6
Relevant territory: T-oauth-oidc

Node 3 — Scheme Planner
Territories scheduled: [T-oauth-oidc]
Arbiter assigned: ASVS-V10-OAuth
Sub-requirements to evaluate:
  V10.2.1: redirect URI exact match — primary finding
  V10.4.6, V10.2.6: not the focus of this submission; evidence not provided
Plan: evaluate V10.2.1 against all registered redirect URIs

Node 4 — Domain Processor
T-oauth-oidc analysis:
  Five exact redirect URIs: satisfy V10.2.1 for those clients
  One wildcard: https://*.docs.example.com/oauth/callback
    - This URI does not satisfy V10.2.1 regardless of rationale
    - V10.2.1 requires exact, character-by-character match for every registered URI
    - The registration list is evaluated as a whole; one non-compliant entry makes
      the overall registration non-compliant
  Team argument ("only one wildcard, five are exact"): rejected
    - V10.2.1 has no "mostly exact" exception
    - Partial compliance does not satisfy a character-exact configuration requirement
  Versioned subdomains (v1.docs, v2.docs, etc.): the correct resolution is to
    register each versioned subdomain URI explicitly and prune old versions

Node 5 — Applier
V10.2.1 → REPROCESS
  Evidence: wildcard redirect URI https://*.docs.example.com/oauth/callback registered
  Finding: wildcard does not satisfy exact registration requirement
  Mitigation offered (five-of-six exact): does not change verdict — one non-compliant URI is a violation
V10.4.6, V10.2.6: insufficient evidence provided; not evaluated

Node 6 — Result Translator
T-oauth-oidc: REPROCESS
  Blocking finding: V10.2.1 wildcard redirect URI
  Non-blocking note: versioned subdomain management via wildcard is a deployment
    concern; correct resolution is versioned exact URIs with lifecycle management

Node 7 — Arbiter Selector
Territory REPROCESS verdict → ASVS-V10-OAuth

Node 8 — Arbiter Unit (ASVS-V10-OAuth)
V10.2.1: REPROCESS
Argument rejected: "five of six redirect URIs are exact."

V10.2.1 requires exact registration for all redirect URIs in the registration
set. The requirement does not tolerate exceptions based on operational convenience
or the ratio of compliant to non-compliant entries.

One wildcard URI in a registration set that also contains five exact URIs:
(1) The wildcard entry itself remains exploitable — an attacker who can control
    any subdomain matching the pattern can receive authorization codes intended
    for the docs cluster clients.
(2) The correct approach for versioned subdomains is to register each version's
    URI explicitly (https://v1.docs.example.com/oauth/callback, etc.) and
    implement a URI lifecycle process to prune deprecated versions.

Evidence gap: https://*.docs.example.com/oauth/callback wildcard registered.
Overall verdict: REPROCESS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-343",
  "overall_verdict": "REPROCESS",
  "territories": [
    {
      "territory": "T-oauth-oidc",
      "verdict": "REPROCESS",
      "arbiter": "ASVS-V10-OAuth",
      "findings": [
        {
          "requirement": "V10.2.1",
          "verdict": "REPROCESS",
          "evidence": "Wildcard redirect URI https://*.docs.example.com/oauth/callback registered",
          "argument_rejected": "Five of six URIs exact does not satisfy requirement for all URIs exact"
        }
      ]
    }
  ]
}
```

**Reason:** V10.2.1 applies to every registered redirect URI, not to the majority of them. One wildcard in a registration set makes the set non-compliant regardless of how many exact entries exist alongside it. The operational rationale (versioned subdomains) is understood but does not create an exception — the correct resolution is per-version exact registration with URI lifecycle management.

**Phrasing-B:** We have six redirect URIs registered. Five are exact. The sixth is a wildcard for our docs cluster because the docs subdomain changes with each release. Most of our registrations are exact — does having one wildcard in a mostly-exact set violate V10.2.1?

---

## Pair[CB4-3.4-344]

**A:** Session invalidation review for SSO environment. Application is one of seven services in an enterprise SSO federation. User logout from the application: (1) clears the application-level session cookie; (2) calls the application's own /api/logout endpoint which invalidates the server-side session record; (3) does not call the SSO provider's logout endpoint (no back-channel or front-channel logout initiated). Users who log out from this application remain authenticated at the SSO provider and can access the other six federated services without re-authentication.

**B:**
```
Node 1 — Domain Splitter
Input: SSO environment session invalidation; application logout does not terminate SSO session.
Domains detected: [asvs-session]
Primary domain: asvs-session
Routing: single-domain

Node 2 — Domain Translator
Domain: asvs-session
Normative mapping:
  Session invalidation on logout: ASVS V7.3.1
  SSO context: V7.3.1 applies to the complete session scope, including SSO
Territory: T-asvs-session

Node 3 — Scheme Planner
Territories scheduled: [T-asvs-session]
Arbiter assigned: ASVS-V7-Session
Sub-requirements to evaluate:
  V7.3.1: server-side session invalidation — does application-only logout satisfy
    when SSO session remains active?

Node 4 — Domain Processor
T-asvs-session analysis:
  Application-layer logout: session cookie cleared + server-side record invalidated ✓
  SSO session: remains active after application logout ✗
  V7.3.1 interpretation in SSO context:
    - The user's "session" in an SSO federation is not limited to the application-layer
      session record. The SSO session token held by the IdP (identity provider) grants
      the user continued access across the federation.
    - V7.3.1 requires that logout invalidates the server-side session. In an SSO
      context, the "server-side session" encompasses both the application session
      and the SSO session — the user remains authentically "logged in" to the
      federation after the application-layer logout.
    - Back-channel logout (RFC 9110 / OIDC back-channel logout) or front-channel
      logout is the mechanism to terminate the SSO session.
  Result: application-only logout does not satisfy V7.3.1 in an SSO context

Node 5 — Applier
V7.3.1 → REPROCESS
  Evidence: SSO session not terminated on application logout
  Finding: user remains authenticated to SSO provider post-logout; access to
    all six other federated services continues without re-authentication

Node 6 — Result Translator
T-asvs-session: REPROCESS
  Blocking finding: V7.3.1 — SSO session not invalidated on logout
  Remediation: implement back-channel logout call to SSO provider on application
    logout event, or implement front-channel logout redirect

Node 7 — Arbiter Selector
REPROCESS → ASVS-V7-Session

Node 8 — Arbiter Unit (ASVS-V7-Session)
V7.3.1 (server-side session invalidation on logout): REPROCESS
Application-layer logout does not satisfy V7.3.1 in an SSO context.

V7.3.1 requires that logout invalidates the server-side session. In a federated
SSO architecture:
(1) The application-layer session is one component of the user's authenticated
    state. Invalidating only the application session while leaving the SSO session
    active leaves the user in an authenticated state accessible across the
    federation.
(2) An attacker who acquires the SSO session token (e.g., via cookie theft on
    a shared device) retains access to all federated services after the user
    believes they have logged out.
(3) The correct implementation requires calling the SSO provider's logout endpoint
    (back-channel or front-channel OIDC logout) as part of the application's
    logout flow.

Evidence gap: SSO session remains active after application logout.
Overall verdict: REPROCESS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-344",
  "overall_verdict": "REPROCESS",
  "territories": [
    {
      "territory": "T-asvs-session",
      "verdict": "REPROCESS",
      "arbiter": "ASVS-V7-Session",
      "findings": [
        {
          "requirement": "V7.3.1",
          "verdict": "REPROCESS",
          "evidence": "Application logout clears application session but does not call SSO provider logout endpoint; SSO session persists",
          "remediation": "Implement OIDC back-channel or front-channel logout call on application logout"
        }
      ]
    }
  ]
}
```

**Reason:** V7.3.1 requires complete session invalidation on logout. In an SSO environment, the user's authenticated state spans both the application session and the SSO session — invalidating only the application layer while leaving the SSO token active means the user is not actually logged out. The SSO session must be terminated via back-channel or front-channel logout.

**Phrasing-B:** Our application clears the session cookie and invalidates our server-side session record on logout. But we don't call the SSO provider's logout endpoint — users stay logged in at the IdP level. Does V7.3.1 require terminating the SSO session, not just our own application session?

---

## Pair[CB4-3.4-345]

**A:** Credential management review. Application running in Kubernetes. Database password stored as a Kubernetes Secret (not a ConfigMap). The Secret is base64-encoded in the Kubernetes API and etcd. The Secret is mounted as an environment variable into the application pod: `DB_PASSWORD=$(kubectl secret value)`. The team states: "We use Kubernetes Secrets, not ConfigMaps. Kubernetes Secrets are the designated credential storage mechanism for Kubernetes workloads."

**B:**
```
Node 1 — Domain Splitter
Input: Kubernetes Secret for database password; mounted as environment variable.
Domains detected: [ssdf-process]
Primary domain: ssdf-process
Routing: single-domain

Node 2 — Domain Translator
Domain: ssdf-process
Normative mapping:
  Credential management: SSDF PW.1
Territory: T-ssdf-process

Node 3 — Scheme Planner
Territories scheduled: [T-ssdf-process]
Arbiter assigned: SSDF-PW
Sub-requirements to evaluate:
  PW.1: credential management — Kubernetes Secret vs ConfigMap distinction

Node 4 — Domain Processor
T-ssdf-process analysis:
  Kubernetes Secret vs ConfigMap distinction:
    - Kubernetes Secrets: base64-encoded (not encrypted) by default in etcd
    - Kubernetes Secrets are NOT encrypted at rest unless etcd encryption is
      explicitly configured (EncryptionConfiguration resource)
    - base64 is an encoding, not encryption — trivially reversible
    - ConfigMap: plaintext in etcd — clearly inadequate
    - Kubernetes Secret without etcd encryption: base64-encoded in etcd — functionally
      similar security posture to ConfigMap; base64 provides no confidentiality
  Team argument ("Secrets not ConfigMaps"): partially valid as a distinction
    between recommended and not-recommended K8s practices, but does not establish
    that the credential is protected at rest
  PW.1 evaluation:
    - PW.1 requires credentials managed in a protected credential store
    - Base64-in-etcd without encryption configuration is not a protected store
    - The credential is readable by anyone with etcd access or sufficient K8s RBAC
  Note: if etcd encryption IS configured, PW.1 would be satisfied by Kubernetes
    Secrets. The submission does not indicate etcd encryption is configured.

Node 5 — Applier
PW.1 → CONCERN
  Evidence: Kubernetes Secret used (correct K8s practice vs ConfigMap); no
    indication of etcd encryption configuration
  Finding: Kubernetes Secret without etcd encryption is base64-encoded, not
    encrypted at rest; does not meet PW.1 protected storage requirement
  Note: CONCERN rather than REPROCESS because:
    (a) Kubernetes Secrets are the designated K8s mechanism — team is using correct
        K8s resource type
    (b) The gap is configuration (etcd encryption not enabled) rather than
        fundamental architecture (vs. ConfigMap which is always plaintext)
    (c) Enabling etcd encryption is a configuration change, not an architectural
        redesign

Node 6 — Result Translator
T-ssdf-process: CONCERN
  Finding: PW.1 — Kubernetes Secret without etcd encryption does not provide
    credential protection at rest
  Remediation: configure etcd EncryptionConfiguration for Secrets, or migrate
    to a dedicated secret management solution (HashiCorp Vault, AWS Secrets
    Manager, etc.)

Node 7 — Arbiter Selector
CONCERN → SSDF-PW

Node 8 — Arbiter Unit (SSDF-PW)
PW.1 (credential management — protected storage): CONCERN
Team argument partially accepted: Kubernetes Secret is the correct K8s resource
type vs. ConfigMap. The team has chosen the appropriate K8s primitive.

However, the PW.1 gap is real:
(1) Kubernetes Secrets are base64-encoded in etcd by default. base64 is not
    encryption — any principal with etcd access or a broad RBAC role that includes
    Secret read access can retrieve the credential in plaintext.
(2) PW.1 requires a protected credential store. "Protected" requires confidentiality
    controls — encryption at rest, not just encoding.
(3) The gap is remediable: configure etcd EncryptionConfiguration (the K8s-native
    path) or adopt a dedicated secrets management system (Vault, cloud provider
    secrets manager).

Finding level: CONCERN. The team is using the appropriate K8s resource type.
The gap is that the default configuration does not provide encryption at rest.
Overall verdict: CONCERN

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-345",
  "overall_verdict": "CONCERN",
  "territories": [
    {
      "territory": "T-ssdf-process",
      "verdict": "CONCERN",
      "arbiter": "SSDF-PW",
      "findings": [
        {
          "requirement": "PW.1",
          "verdict": "CONCERN",
          "evidence": "Kubernetes Secret used (correct K8s type); etcd encryption not configured; base64 encoding is not encryption at rest",
          "remediation": "Configure etcd EncryptionConfiguration or migrate to dedicated secrets manager"
        }
      ]
    }
  ]
}
```

**Reason:** Kubernetes Secrets without etcd encryption configured are base64-encoded in etcd, which is encoding not encryption. The team correctly uses Kubernetes Secrets (not ConfigMaps), which earns partial credit — but base64 does not satisfy PW.1's protected storage requirement. CONCERN rather than REPROCESS because the team uses the correct K8s resource type and the gap is a configuration matter, not a fundamental architecture failure.

**Phrasing-B:** We store our database password in a Kubernetes Secret, not a ConfigMap. Kubernetes Secrets are the proper K8s mechanism for credentials. Does using Kubernetes Secrets satisfy SSDF PW.1, or is there still a concern?

---

## Pair[CB4-3.4-346]

**A:** JWT algorithm configuration review. JWT library configured with an algorithm blocklist: `blocked_algorithms = ["none", "HS256", "HS384", "HS512"]`. All HMAC algorithms and the `none` algorithm are blocked. The library will accept any algorithm not on this list. RS256, RS384, RS512, ES256, ES384, ES512, PS256, PS384, PS512 are all accepted by default. The team states: "We block all dangerous algorithms. Nothing on our blocklist can be used to forge a token."

**B:**
```
Node 1 — Domain Splitter
Input: JWT algorithm blocklist configuration blocking none + all HMAC variants.
Domains detected: [asvs-token, rfc-jwt]
Primary domain: asvs-token
Secondary: rfc-jwt
Routing: multi-domain

Node 2 — Domain Translator
Domains: asvs-token, rfc-jwt
Normative mapping:
  Algorithm configuration: ASVS V9.1.1 (algorithm allowlist)
  JWT security: RFC 7515 §4.1.1 (alg header — accepted algorithms)
Territories: T-asvs-token, T-rfc-jwt

Node 3 — Scheme Planner
Territories scheduled: [T-asvs-token, T-rfc-jwt]
Arbiters: ASVS-V9-Token (primary), RFC-JWT
Sub-requirements:
  V9.1.1: allowlist vs blocklist — key architectural question
  RFC 7515: algorithm handling

Node 4 — Domain Processor
T-asvs-token analysis:
  Blocklist approach:
    - Blocks: none, HS256, HS384, HS512 — all dangerous algorithms blocked
    - Accepts: any algorithm not on the blocklist
    - Risk: new algorithms added to the JWT ecosystem will be automatically
      accepted unless explicitly added to the blocklist (open-world assumption)
    - Risk: algorithm confusion with accepted but weak/unusual algorithms
      (e.g., RS256 with very short key, EC algorithms with unusual parameters)
    - V9.1.1 requires an allowlist — explicitly enumerate accepted algorithms
    - Allowlist (closed-world): unknown/new algorithms are rejected by default
    - Blocklist (open-world): unknown/new algorithms are accepted by default
  T-rfc-jwt analysis:
    RFC 7515 does not mandate allowlist vs blocklist architecture; RFC 7515
    recommends validating the alg header against expected values. The blocklist
    approach partially satisfies RFC intent — dangerous algorithms are excluded.

Node 5 — Applier
V9.1.1 → CONCERN
  Evidence: blocklist blocks none + all HMAC; all RSA/EC/PSS variants accepted
  Finding: blocklist approach is architecturally weaker than allowlist; dangerous
    algorithms currently blocked; architectural risk is open-world assumption
  Level: CONCERN — dangerous algorithms are blocked (meaningful mitigation);
    gap is architectural discipline, not current exploitability
RFC 7515: PASS
  Dangerous algorithms excluded; the specific RFC concerns (none, weak HMAC) addressed

Node 6 — Result Translator
T-asvs-token: CONCERN (V9.1.1 blocklist vs allowlist)
T-rfc-jwt: PASS
Overall verdict: CONCERN (CONCERN overrides PASS)

Node 7 — Arbiter Selector
Multi-territory: ASVS-V9-Token + RFC-JWT
Primary finding: ASVS-V9-Token CONCERN

Node 8 — Arbiter Unit (ASVS-V9-Token)
V9.1.1 (algorithm allowlist): CONCERN
Blocking of dangerous algorithms acknowledged: none, HS256, HS384, HS512 blocked.

V9.1.1 requires an allowlist, not a blocklist:
(1) A blocklist accepts any algorithm not explicitly blocked. As new algorithms
    emerge or unusual algorithm identifiers are introduced, they are accepted by
    default unless added to the blocklist — this is the open-world assumption.
(2) An allowlist accepts only algorithms explicitly enumerated. Unknown/new
    algorithms are rejected by default — this is the closed-world assumption
    required by V9.1.1.
(3) The team's current blocklist does meaningfully reduce risk by removing the
    most dangerous options (none, HMAC algorithms susceptible to secret confusion).
    This is why the finding is CONCERN not REPROCESS.
(4) Remediation: replace the blocklist with an allowlist of exactly the algorithms
    the application intends to accept (e.g., ["RS256"] or ["RS256", "ES256"]).

Arbiter Unit (RFC-JWT)
RFC 7515: PASS — none algorithm and HMAC variants excluded; primary RFC attack
surfaces addressed by the blocklist.

Overall verdict: CONCERN

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-346",
  "overall_verdict": "CONCERN",
  "territories": [
    {
      "territory": "T-asvs-token",
      "verdict": "CONCERN",
      "arbiter": "ASVS-V9-Token",
      "findings": [
        {
          "requirement": "V9.1.1",
          "verdict": "CONCERN",
          "evidence": "Blocklist approach: blocks none + HMAC; accepts all others by default",
          "finding": "Blocklist is open-world (new algs accepted by default); allowlist required"
        }
      ]
    },
    {
      "territory": "T-rfc-jwt",
      "verdict": "PASS",
      "arbiter": "RFC-JWT",
      "findings": [
        {
          "requirement": "RFC7515-alg",
          "verdict": "PASS",
          "note": "Dangerous algorithms (none, HMAC) excluded by blocklist"
        }
      ]
    }
  ]
}
```

**Reason:** A blocklist that blocks dangerous algorithms provides real risk reduction — hence CONCERN not REPROCESS. But V9.1.1 requires an allowlist because a blocklist has an open-world assumption: any algorithm not on the list is accepted by default. An allowlist inverts this: only explicitly enumerated algorithms are accepted. RFC 7515 is PASS because the primary JWT attack surfaces (none, HMAC confusion) are addressed.

**Phrasing-B:** Our JWT library uses a blocklist: none, HS256, HS384, HS512 are all blocked. All RSA and EC algorithms are accepted. We're blocking everything dangerous. Is blocking dangerous algorithms equivalent to V9.1.1's allowlist requirement?

---

## Pair[CB4-3.4-347]

**A:** PKCE implementation review. The authorization server validates the code_verifier at token exchange by: (1) confirming the code_verifier is present in the request; (2) confirming the code_verifier length is between 43 and 128 characters; (3) confirming the code_verifier contains only unreserved characters (A-Z, a-z, 0-9, `-._~`). The authorization server does NOT compute SHA256(code_verifier) and compare to the stored code_challenge. The team states: "We verify the code_verifier is present and correctly formatted per RFC 7636 character requirements. The format check validates the verifier."

**B:**
```
Node 1 — Domain Splitter
Input: PKCE code_verifier validation — presence + length + character set only; no SHA256 comparison.
Domains detected: [oauth-oidc, rfc-oauth2]
Primary domain: oauth-oidc
Secondary: rfc-oauth2
Routing: multi-domain

Node 2 — Domain Translator
Domains: oauth-oidc, rfc-oauth2
Normative mapping:
  PKCE verification: ASVS V10.4.6 (PKCE with S256 enforced)
  RFC 7636: §4.6 — code_verifier validation procedure
Territories: T-oauth-oidc, T-rfc-oauth2

Node 3 — Scheme Planner
Territories: [T-oauth-oidc, T-rfc-oauth2]
Arbiters: ASVS-V10-OAuth, RFC-OAuth2
Sub-requirements:
  V10.4.6: full PKCE validation — challenge stored, verifier hashed and compared
  RFC 7636 §4.6: verification procedure — compute BASE64URL(SHA256(verifier)),
    compare to stored code_challenge

Node 4 — Domain Processor
T-oauth-oidc + T-rfc-oauth2 analysis:
  PKCE S256 verification procedure (RFC 7636 §4.6):
    Step 1: receive code_verifier at token exchange
    Step 2: compute BASE64URL(SHA256(ASCII(code_verifier)))
    Step 3: compare result to stored code_challenge from authorization request
    Step 4: if equal, PKCE verified; if not equal, reject
  Team's actual procedure:
    - Checks presence: ✓ (necessary but not sufficient)
    - Checks length (43-128): ✓ (RFC 7636 §4.1 character requirement)
    - Checks character set: ✓ (RFC 7636 §4.1 character requirement)
    - Computes SHA256 and compares to code_challenge: ✗ (the actual verification step)
  Impact of missing SHA256 comparison:
    - Any code_verifier of valid length and character set will pass validation
    - An attacker who intercepts the authorization code can use ANY valid-format
      string as the code_verifier — the authorization server cannot distinguish
      the legitimate client from an attacker
    - PKCE provides zero authorization code interception protection without the
      comparison step

Node 5 — Applier
V10.4.6 → REPROCESS
  Evidence: SHA256(code_verifier) not computed; no comparison to stored code_challenge
  Finding: PKCE verification absent; format checks alone provide no PKCE security
RFC 7636 §4.6 → REPROCESS
  Evidence: verification procedure not followed; §4.6 requires hash comparison

Node 6 — Result Translator
T-oauth-oidc: REPROCESS
T-rfc-oauth2: REPROCESS
Overall: REPROCESS

Node 7 — Arbiter Selector
Both territories REPROCESS → ASVS-V10-OAuth + RFC-OAuth2

Node 8 — Arbiter Unit (ASVS-V10-OAuth)
V10.4.6 (PKCE with S256): REPROCESS
Argument rejected: "format validation satisfies PKCE verification."

PKCE verification requires the authorization server to compute
BASE64URL(SHA256(code_verifier)) and compare it to the stored code_challenge.
Format validation (presence, length, character set) is a pre-condition for
receiving the verifier, not the verification itself.

Without the hash comparison:
(1) Any valid-format string will pass the token exchange.
(2) An attacker who intercepted the authorization code can submit any
    valid-format code_verifier and obtain a token.
(3) PKCE provides no protection — the security property of PKCE (proving
    possession of the original random secret) requires the hash comparison.

Evidence gap: SHA256(code_verifier) not computed; stored code_challenge not verified.
Overall verdict: REPROCESS

Arbiter Unit (RFC-OAuth2)
RFC 7636 §4.6: REPROCESS — verification procedure not implemented.

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-347",
  "overall_verdict": "REPROCESS",
  "territories": [
    {
      "territory": "T-oauth-oidc",
      "verdict": "REPROCESS",
      "arbiter": "ASVS-V10-OAuth",
      "findings": [
        {
          "requirement": "V10.4.6",
          "verdict": "REPROCESS",
          "evidence": "code_verifier format validated (presence, length, chars) but SHA256 not computed; no comparison to stored code_challenge",
          "argument_rejected": "Format validation ≠ PKCE cryptographic verification"
        }
      ]
    },
    {
      "territory": "T-rfc-oauth2",
      "verdict": "REPROCESS",
      "arbiter": "RFC-OAuth2",
      "findings": [
        {
          "requirement": "RFC7636-§4.6",
          "verdict": "REPROCESS",
          "evidence": "Verification procedure §4.6 not implemented; hash comparison absent"
        }
      ]
    }
  ]
}
```

**Reason:** Format validation (length and character checks) satisfies RFC 7636 §4.1 pre-conditions for the code_verifier value itself, but it is not the PKCE verification step. The verification step (§4.6) requires computing BASE64URL(SHA256(code_verifier)) and comparing to the stored code_challenge. Without this comparison, any valid-format string passes — PKCE provides zero protection.

**Phrasing-B:** At token exchange, we check that the code_verifier is present, is 43-128 characters, and contains only RFC 7636 allowed characters. We validate the format per the spec. Is format validation of the code_verifier sufficient for V10.4.6, or is there a missing verification step?

---

## Pair[CB4-3.4-348]

**A:** Password hashing assessment. User passwords are hashed using scrypt with parameters N=1024, r=8, p=1. The team states: "We use scrypt, which is a memory-hard algorithm. scrypt is on the OWASP recommended list for password hashing. Our configuration matches our production server's available memory and we optimize for response time."

**B:**
```
Node 1 — Domain Splitter
Input: scrypt N=1024, r=8, p=1 for password hashing.
Domains detected: [asvs-authentication]
Primary domain: asvs-authentication
Routing: single-domain

Node 2 — Domain Translator
Domain: asvs-authentication
Normative mapping:
  Password hashing: ASVS V6.2.5 (memory-hard algorithm with correct configuration)
Territory: T-asvs-authentication

Node 3 — Scheme Planner
Territories: [T-asvs-authentication]
Arbiter: ASVS-V6-Auth
Sub-requirements:
  V6.2.5: scrypt N=1024 vs recommended minimum N=32768

Node 4 — Domain Processor
T-asvs-authentication analysis:
  scrypt parameter analysis:
    N (CPU/memory cost): 1024
    OWASP minimum recommendation: N=32768 (2^15)
    Current value: 1024 (2^10)
    Ratio: 1024 / 32768 = 1/32 — 32× below minimum recommendation
  Memory cost comparison:
    Memory per hash = 128 × N × r bytes
    N=1024, r=8: 128 × 1024 × 8 = 1,048,576 bytes = 1 MB
    N=32768, r=8: 128 × 32768 × 8 = 33,554,432 bytes = 32 MB
    Current configuration uses 1/32 of the recommended minimum memory
  Security impact:
    Memory-hardness at N=1024 is near-negligible for modern hardware
    GPU/ASIC parallel attacks are not meaningfully impeded at N=1024
    The algorithm is scrypt (correct type) but the configuration defeats the
    purpose of the memory-hard property
  Team argument ("production server memory constraints"):
    - scrypt at N=1024 is not a memory-constrained optimization — it is an
      order-of-magnitude misconfiguration
    - Server memory constraints affect N ceiling, not whether minimum N is met
    - If the server cannot support N=32768 with acceptable latency, the correct
      path is to use bcrypt (lower memory requirement) properly configured
      rather than scrypt misconfigured to near-zero memory hardness

Node 5 — Applier
V6.2.5 → REPROCESS
  Evidence: scrypt N=1024; 32× below OWASP minimum N=32768
  Finding: configuration defeats memory-hard security property; not CONCERN
    because the delta is an order of magnitude, not a marginal shortfall
  Note: bcrypt cost=4 is CONCERN (correct algorithm, insufficient iterations,
    marginal shortfall); scrypt N=1024 is REPROCESS (correct algorithm type,
    but configuration is order-of-magnitude below minimum, defeating the
    memory-hard property entirely)

Node 6 — Result Translator
T-asvs-authentication: REPROCESS
  Finding: V6.2.5 — scrypt N=1024 is not compliant configuration
  Remediation: increase N to ≥32768 (if server supports); or migrate to bcrypt
    cost ≥10 if scrypt memory requirements are genuinely prohibitive

Node 7 — Arbiter Selector
REPROCESS → ASVS-V6-Auth

Node 8 — Arbiter Unit (ASVS-V6-Auth)
V6.2.5 (memory-hard password hashing): REPROCESS
Team argument partially acknowledged: scrypt is a memory-hard algorithm and is
on the OWASP recommended list. The algorithm choice is correct in type.

However, the configuration N=1024 does not satisfy V6.2.5:
(1) OWASP minimum for scrypt: N=32768. The current N=1024 is 32× below this
    minimum — an order-of-magnitude shortfall.
(2) At N=1024, the memory requirement per hash is ~1 MB. At N=32768 it is ~32 MB.
    The memory-hard property at N=1024 provides near-negligible resistance to
    parallel attacks on modern GPU hardware.
(3) "Production server memory constraints" do not justify operating at N=1024.
    If the server cannot support N=32768, the correct response is to use a
    different algorithm (bcrypt, Argon2id) that operates within memory constraints
    while maintaining security, not to configure scrypt at a near-zero N value.

Distinction from bcrypt cost=4 (CONCERN): bcrypt cost=4 is a marginal shortfall
on a correctly-scoped algorithm. scrypt N=1024 is an order-of-magnitude departure
from the minimum — the memory-hard property is effectively nullified.

Evidence gap: N=1024 is 32× below OWASP minimum for scrypt.
Overall verdict: REPROCESS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-348",
  "overall_verdict": "REPROCESS",
  "territories": [
    {
      "territory": "T-asvs-authentication",
      "verdict": "REPROCESS",
      "arbiter": "ASVS-V6-Auth",
      "findings": [
        {
          "requirement": "V6.2.5",
          "verdict": "REPROCESS",
          "evidence": "scrypt N=1024; OWASP minimum N=32768; 32× below minimum; memory-hard property near-nullified",
          "argument_rejected": "Algorithm type correct; configuration defeats memory-hardness entirely"
        }
      ]
    }
  ]
}
```

**Reason:** scrypt N=1024 is an order-of-magnitude misconfiguration, not a marginal shortfall. The OWASP minimum is N=32768; N=1024 is 32× below this, providing ~1 MB per hash versus the recommended ~32 MB. At N=1024, memory-hard protection against GPU parallel attacks is near-negligible. This is REPROCESS unlike bcrypt cost=4 (CONCERN) because the delta from minimum is so large that the memory-hard property — the entire purpose of scrypt — is effectively defeated.

**Phrasing-B:** We use scrypt (a memory-hard algorithm on the OWASP list) with N=1024, r=8, p=1. Server constraints led to this configuration. Does scrypt N=1024 satisfy V6.2.5, or is there a problem with this parameter value?

---

## Pair[CB4-3.4-349]

**A:** Authorization disclosure review. The application has no `/me/permissions` endpoint. Users learn about permissions by attempting operations and observing results. Successful operations reveal capabilities; failed operations return `403 Forbidden` with the message: "You do not have permission to perform this action: {action_name}". The `{action_name}` field includes the name of the attempted action. No permission list is proactively provided to users.

**B:**
```
Node 1 — Domain Splitter
Input: permissions disclosed only via 403 error messages with action names; no proactive permission list.
Domains detected: [asvs-authorization]
Primary domain: asvs-authorization
Routing: single-domain

Node 2 — Domain Translator
Domain: asvs-authorization
Normative mapping:
  Authorization metadata: ASVS V8.2.1 (users can access their own authorization attributes)
Territory: T-asvs-authorization

Node 3 — Scheme Planner
Territories: [T-asvs-authorization]
Arbiter: ASVS-V8-AuthZ
Sub-requirements:
  V8.2.1: does reactive 403 disclosure satisfy "access to own authorization attributes"?

Node 4 — Domain Processor
T-asvs-authorization analysis:
  V8.2.1 requirement: users can access records of their own authorization attributes
  Current mechanism: permission discovery via 403 trial-and-error
  Analysis:
    - 403 response with action name tells the user that they lack a specific
      permission after they attempt to use it — reactive disclosure
    - V8.2.1 requires that users CAN ACCESS their authorization attributes;
      this implies some mechanism for proactive or on-demand disclosure, not
      purely reactive discovery through failure
    - Reactive 403 discovery provides partial information:
      (a) A user learns what they cannot do only after attempting it
      (b) A user has no way to know all of their permissions without exhaustively
          attempting every possible action
      (c) A user cannot distinguish "this action doesn't exist" from
          "you don't have this permission" in some API designs
    - CONCERN rationale: 403 with action_name does provide some information
      about the user's authorization state (they lack the named permission);
      it is not a complete absence of authorization metadata access

Node 5 — Applier
V8.2.1 → CONCERN
  Evidence: no proactive permission disclosure; 403 responses with action names
    provide reactive partial information only
  Finding: reactive-only discovery via error does not satisfy V8.2.1's intent
    for user access to own authorization attributes
  Level: CONCERN — some information is provided (which permissions are denied
    when attempted); gap is proactive vs reactive disclosure

Node 6 — Result Translator
T-asvs-authorization: CONCERN
  Finding: V8.2.1 partially satisfied by 403 disclosure; proactive mechanism absent
  Remediation: implement /me/permissions endpoint, JWT role claims, or equivalent
    proactive authorization attribute access

Node 7 — Arbiter Selector
CONCERN → ASVS-V8-AuthZ

Node 8 — Arbiter Unit (ASVS-V8-AuthZ)
V8.2.1 (user access to own authorization attributes): CONCERN
403 error messages with action names provide reactive partial information. This
does not satisfy V8.2.1.

V8.2.1 requires that users can access records of their own authorization attributes.
This means a user should be able to query or retrieve their permissions without
having to discover them by attempting operations and observing failures.

403 discovery:
(1) Reveals denied permissions only when attempted — users cannot learn their
    permissions proactively.
(2) Does not reveal all granted permissions — only the specific denied action
    that was attempted.
(3) Requires exhaustive trial-and-error to map the full permission set.
(4) Does not allow a user to verify their permissions before attempting a
    sensitive operation.

Finding level: CONCERN. The 403 messages do convey some authorization state
information (the specific action name) — not a complete absence. The gap is
the absence of a proactive access mechanism.

Remediation: /me/permissions endpoint returning the full permission set; or
JWT claims containing the user's roles/permissions; or any on-demand mechanism
that returns the full permission set without requiring trial-and-error.
Overall verdict: CONCERN

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-349",
  "overall_verdict": "CONCERN",
  "territories": [
    {
      "territory": "T-asvs-authorization",
      "verdict": "CONCERN",
      "arbiter": "ASVS-V8-AuthZ",
      "findings": [
        {
          "requirement": "V8.2.1",
          "verdict": "CONCERN",
          "evidence": "Permission disclosure only via 403 Forbidden with action_name; no proactive mechanism",
          "finding": "Reactive trial-and-error discovery does not satisfy proactive user access to own authorization attributes"
        }
      ]
    }
  ]
}
```

**Reason:** V8.2.1 requires that users can access their own authorization attributes — meaning they should be able to query their permissions, not only discover them by attempting operations and failing. 403 responses with action names are reactive: they reveal denied permissions only when attempted and do not allow users to see their full permission set proactively. CONCERN because 403 with action_name does convey some authorization information; not a complete gap.

**Phrasing-B:** Users find out about their permissions by trying things. If they lack permission, they get a 403 with the name of the action they couldn't perform. No /me/permissions endpoint. Does learning permissions reactively through 403 errors satisfy V8.2.1?

---

## Pair[CB4-3.4-350]

**A:** Five-territory comprehensive review. Application implements:
- OAuth: exact redirect URIs for all clients, PKCE S256 enforced, state parameter validated on all flows
- Session: server-side session invalidation on logout (logout terminates app session + calls OIDC back-channel logout)
- Authentication: Argon2id m=65536, t=3, p=4 for all user passwords; bcrypt cost 11 for API key hashing
- Authorization: /me/permissions endpoint returning full role and permission set; enforce deny-by-default with explicit grants
- Token: RS256 allowlist only; aud claim verified against expected service identifier
- Credential management: HashiCorp Vault for all secrets; no hardcoded credentials anywhere
- Code review: Security Champion reviews all PRs with OWASP Security Code Review checklist

**B:**
```
Node 1 — Domain Splitter
Input: comprehensive five-territory review; all security controls fully described.
Domains detected: [oauth-oidc, asvs-session, asvs-authentication, asvs-authorization,
  asvs-token, rfc-jwt, rfc-oauth2, ssdf-process, cwe-authn, cwe-authz]
Primary domain: multi-domain
Routing: multi-territory

Node 2 — Domain Translator
Full normative mapping:
  OAuth controls: V10.2.1, V10.4.6, V10.2.6 → T-oauth-oidc
  Session: V7.3.1 (back-channel logout) → T-asvs-session
  Authentication: V6.2.5 (Argon2id + bcrypt cost 11) → T-asvs-authentication
  Authorization: V8.2.1 (/me/permissions), V8.1.2 (deny-by-default) → T-asvs-authorization
  Token: V9.1.1 (RS256 allowlist), V10.5.1 (aud claim) → T-asvs-token, T-rfc-jwt
  Credential management: PW.1 (Vault) → T-ssdf-process
  Code review: PW.3.2 (Security Champion + OWASP checklist) → T-ssdf-process

Node 3 — Scheme Planner
Territories: [T-oauth-oidc, T-asvs-session, T-asvs-authentication, T-asvs-authorization,
  T-asvs-token, T-rfc-jwt, T-rfc-oauth2, T-ssdf-process]
All sub-requirements appear to have evidence; plan: verify each

Node 4 — Domain Processor
T-oauth-oidc:
  V10.2.1: exact redirect URIs for all clients ✓
  V10.4.6: PKCE S256 enforced ✓
  V10.2.6: state parameter validated ✓
  → PASS

T-asvs-session:
  V7.3.1: logout terminates app session + calls OIDC back-channel logout ✓
  → PASS

T-asvs-authentication:
  V6.2.5: Argon2id m=65536/t=3/p=4 — meets OWASP minimum (m=47104) ✓
  V6.2.5: bcrypt cost 11 for API keys — exceeds OWASP minimum (cost 10) ✓
  → PASS

T-asvs-authorization:
  V8.2.1: /me/permissions endpoint with full role and permission set ✓
  V8.1.2: deny-by-default with explicit grants ✓
  → PASS

T-asvs-token / T-rfc-jwt:
  V9.1.1: RS256 allowlist only ✓
  V10.5.1: aud claim verified against expected service identifier ✓
  → PASS

T-ssdf-process:
  PW.1: HashiCorp Vault; no hardcoded credentials ✓
  PW.3.2: Security Champion + OWASP Security Code Review checklist ✓
  → PASS

Node 5 — Applier
All territories: PASS

Node 6 — Result Translator
All territories PASS → overall PASS

Node 7 — Arbiter Selector
All PASS: no REPROCESS or CONCERN arbiters required; confirming PASS verdicts

Node 8 — Arbiter Units (confirmatory)
ASVS-V10-OAuth: V10.2.1 PASS, V10.4.6 PASS, V10.2.6 PASS
ASVS-V7-Session: V7.3.1 PASS (back-channel logout correctly implemented)
ASVS-V6-Auth: V6.2.5 PASS (both Argon2id and bcrypt cost 11 correctly configured)
ASVS-V8-AuthZ: V8.2.1 PASS, V8.1.2 PASS
ASVS-V9-Token: V9.1.1 PASS (RS256 allowlist)
RFC-JWT: V10.5.1 PASS (aud claim verified)
SSDF-PW: PW.1 PASS (Vault), PW.3.2 PASS (Security Champion + checklist)

Overall verdict: PASS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-350",
  "overall_verdict": "PASS",
  "territories": [
    {"territory": "T-oauth-oidc", "verdict": "PASS", "arbiter": "ASVS-V10-OAuth"},
    {"territory": "T-asvs-session", "verdict": "PASS", "arbiter": "ASVS-V7-Session"},
    {"territory": "T-asvs-authentication", "verdict": "PASS", "arbiter": "ASVS-V6-Auth"},
    {"territory": "T-asvs-authorization", "verdict": "PASS", "arbiter": "ASVS-V8-AuthZ"},
    {"territory": "T-asvs-token", "verdict": "PASS", "arbiter": "ASVS-V9-Token"},
    {"territory": "T-rfc-jwt", "verdict": "PASS", "arbiter": "RFC-JWT"},
    {"territory": "T-ssdf-process", "verdict": "PASS", "arbiter": "SSDF-PW"}
  ]
}
```

**Reason:** All controls correctly implemented across all territories. Key positive recognitions: (a) back-channel OIDC logout satisfies V7.3.1 in SSO context; (b) Argon2id m=65536 and bcrypt cost 11 both satisfy V6.2.5; (c) /me/permissions endpoint satisfies V8.2.1; (d) RS256 allowlist + aud claim verification satisfy token territory; (e) HashiCorp Vault satisfies PW.1; (f) Security Champion + OWASP checklist satisfies PW.3.2.

**Phrasing-B:** Exact redirect URIs, PKCE S256, state validation, back-channel SSO logout, Argon2id m=65536, bcrypt cost 11 for API keys, /me/permissions endpoint, deny-by-default, RS256 allowlist, aud verified, Vault for secrets, Security Champion reviewing PRs with OWASP checklist. Does this full implementation pass all applicable territories?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-343 | V10.2.1 partial exact + wildcard docs cluster — pipeline trace REPROCESS | C | ASVS-V10-OAuth | REPROCESS |
| CB4-3.4-344 | V7.3.1 SSO back-channel logout — pipeline trace REPROCESS | C | ASVS-V7-Session | REPROCESS |
| CB4-3.4-345 | PW.1 Kubernetes Secret (not ConfigMap) without etcd encryption — CONCERN trace | C | SSDF-PW | CONCERN |
| CB4-3.4-346 | V9.1.1 blocklist (none+HMAC blocked) — CONCERN trace + RFC-JWT PASS | C | ASVS-V9-Token + RFC-JWT | CONCERN |
| CB4-3.4-347 | V10.4.6 length-only verifier validation — pipeline trace REPROCESS | C | ASVS-V10-OAuth + RFC-OAuth2 | REPROCESS |
| CB4-3.4-348 | V6.2.5 scrypt N=1024 — pipeline trace REPROCESS (not CONCERN) | C | ASVS-V6-Auth | REPROCESS |
| CB4-3.4-349 | V8.2.1 403-only permission disclosure — pipeline trace CONCERN | C | ASVS-V8-AuthZ | CONCERN |
| CB4-3.4-350 | All ext13 patterns correctly implemented — 7-territory PASS | C | Multiple | PASS |
