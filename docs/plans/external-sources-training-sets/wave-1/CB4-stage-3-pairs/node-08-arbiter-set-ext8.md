# Node 08 — Arbiter Units — Extension Set 8
# Stage 3 Action 3 — Coverage Extension (remaining domain gaps + Stratum D)
# Batch: CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Sequences: CB4-3.4-223 through CB4-3.4-230
# Instances: 16 (8 base pairs × 2 phrasings each)
# Running total after this file: 460 instances (sequences 001–230)

---

## Coverage targets for this file

Remaining domain gaps (Stratum A) + adversarial edge cases (Stratum D).
New domains:
  - ASVS V10.2.6: state parameter for CSRF protection in OAuth authorization code flow
  - CWE-288: authentication bypass using an alternate path or channel
  - V8.2.2: sensitive data in server logs (data exposure)
  - SSDF PW.5: build process security (signed artifacts, trusted build environment)

Stratum D adversarial:
  - Algorithm confusion attack: team submits HS256-signed JWT using the public key as HMAC secret
  - JWT kid (key ID) injection: arbitrary kid value in JWT header
  - Authorization header stripping: team argues "our load balancer strips auth headers, so we can't use JWT"
  - Circular SSDF argument: "our code review IS our threat model review"

---

## Pair CB4-3.4-223

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — ASVS V10.2.6: state parameter for CSRF in OAuth)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter: ASVS-V10-OAuth-Arbiter
territory: T-oauth-oidc
findings:
  - requirement: V10.2.6
  - "OAuth authorization code flow implemented"
  - "state parameter: not included in authorization request"
  - "redirect_uri: exact match enforced — confirmed"
  - "PKCE S256: confirmed"
pipeline_run_id: PR-5501
```

### B

```
arbiter_result:
  arbiter: ASVS-V10-OAuth-Arbiter
  verdict: REPROCESS
  requirements_violated: [V10.2.6]
  gap: "State parameter absent from authorization request. V10.2.6 requires a cryptographically random state parameter to prevent CSRF attacks on the OAuth callback endpoint. PKCE protects the authorization code exchange; state protects the authorization request initiation."
  action_required: "Generate a cryptographically random state value per authorization request, store it in the user's session, include it in the authorization request, and verify the state value on callback. Reject callbacks where the state does not match the session-stored value."
  note: "PKCE does not substitute for state. PKCE (V10.4.6) protects the code exchange step (authorization code → token). State (V10.2.6) protects the authorization request initiation step against CSRF. Both are required independently."
```

### Reason

V10.2.6 requires the state parameter. The team has PKCE (V10.4.6) but not state (V10.2.6). These are independent: PKCE protects code exchange, state protects against authorization request CSRF. The note makes this explicit because teams frequently conflate the two protections. REPROCESS because state is a MUST-level V10 requirement.

### Phrasing B

```
ASVS-V10-OAuth-Arbiter, V10.2.6, run PR-5501.
Auth code flow with PKCE S256. State parameter: not included.
Does PKCE substitute for state?
```

Expected: REPROCESS. State is absent. PKCE ≠ state (different attacks, different steps). Both required. Action_required: generate random state per request, verify on callback.

---

## Pair CB4-3.4-224

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — CWE-288: authentication bypass via direct object access)
**Source:** CWE Top 25 2023
**Verdict:** REPROCESS

### A

```
arbiter: CWE-Authn-Arbiter
territory: T-cwe-authn
findings:
  - "authenticated endpoints: POST /api/auth/login → JWT issued → /api/protected/* requires JWT"
  - "bypass path discovered: GET /api/export/data.csv — returns all user data; no authentication required"
  - "endpoint is not in the route authentication middleware chain"
  - "CWE-288: authentication bypass using an alternate path"
pipeline_run_id: PR-5512
```

### B

```
arbiter_result:
  arbiter: CWE-Authn-Arbiter
  verdict: REPROCESS
  cwe_id: CWE-288
  severity: CRITICAL
  gap: "/api/export/data.csv bypasses authentication entirely. The endpoint is not covered by the route authentication middleware. CWE-288 (Authentication Bypass Using an Alternate Path or Channel): the alternate path (/api/export/data.csv) circumvents the authentication enforcement applied to /api/protected/*."
  action_required: "Add authentication enforcement to /api/export/data.csv. Audit all endpoints to confirm that authentication middleware applies universally — do not rely on route pattern matching that may miss specific paths."
  note: "CRITICAL: unauthenticated export of all user data. This is higher severity than a missing ownership check (CWE-862) because no authentication is required at all — any internet user can access the endpoint."
```

### Reason

CWE-288 is Authentication Bypass Using an Alternate Path. The team has authentication on /api/protected/* but missed /api/export/data.csv, which is outside the middleware chain. CRITICAL severity because no authentication is required to access all user data. REPROCESS because the bypass is complete.

### Phrasing B

```
CWE-Authn-Arbiter, run PR-5512.
/api/protected/* requires JWT. But /api/export/data.csv: no auth middleware, returns all user data.
Verdict?
```

Expected: REPROCESS. CWE-288 CRITICAL. Authentication bypass via alternate path. Action_required: add auth middleware to export endpoint; audit all endpoints for middleware coverage gaps.

---

## Pair CB4-3.4-225

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — V8.2.2: sensitive data in server logs)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter: ASVS-V8-AuthZ-Arbiter
territory: T-asvs-authz
findings:
  - requirement: V8.2.2
  - "server-side logging: all request parameters logged for debugging"
  - "logged parameters include: Authorization header value (JWT token), password field in POST body, user PII (email, SSN)"
  - "log storage: centralized logging service accessible by engineering team"
pipeline_run_id: PR-5523
```

### B

```
arbiter_result:
  arbiter: ASVS-V8-AuthZ-Arbiter
  verdict: REPROCESS
  requirements_violated: [V8.2.2]
  gap: "Server logs contain: Authorization header (full JWT tokens), password field, email, and SSN. V8.2.2 prohibits logging sensitive data — logging JWT tokens exposes active credentials; logging passwords exposes plaintext credentials; logging SSN/email exposes PII."
  action_required:
    - "Exclude Authorization headers from request logging."
    - "Exclude password fields from request body logging (mask or omit)."
    - "Apply PII minimization: log only what is needed for debugging; exclude SSN, email addresses, or mask them."
    - "Audit existing log retention and access: purge or redact sensitive data from current log store."
  note: "REPROCESS: logging JWT tokens means any log-access incident exposes active session credentials. Logging passwords means any log-access incident exposes user credentials. Both are MUST-level violations of V8.2.2."
```

### Reason

V8.2.2 prohibits logging sensitive data. The logs contain JWT tokens (active credentials), passwords (plaintext credential exposure), and PII. REPROCESS because logging active session tokens is equivalent to storing credentials in plaintext logs. Multiple MUST-level violations.

### Phrasing B

```
ASVS-V8-AuthZ-Arbiter, V8.2.2, run PR-5523.
All request params logged including Authorization header (JWT), password field, SSN, email.
Verdict?
```

Expected: REPROCESS. V8.2.2 violated. JWT in logs = credential exposure. Password in logs = plaintext credential. Action_required: exclude auth headers, passwords, and PII from logs.

---

## Pair CB4-3.4-226

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — SSDF PW.5: unsigned build artifacts)
**Source:** NIST SP 800-218 SSDF v1.1
**Verdict:** CONCERN

### A

```
arbiter: SSDF-PW-Arbiter
territory: T-ssdf-pw5
findings:
  - practice: PW.5
  - "build pipeline: GitHub Actions — confirmed"
  - "artifact signing: not implemented; Docker images published without signature"
  - "base image: pulled from Docker Hub without digest pinning (using :latest tag)"
  - "existing control: Docker Hub official images only"
pipeline_run_id: PR-5534
```

### B

```
arbiter_result:
  arbiter: SSDF-PW-Arbiter
  verdict: CONCERN
  practice: PW.5
  gap: "Build artifacts (Docker images) not signed. Base image pulled by :latest tag without digest pinning — no verification that the base image is what was originally used."
  action_recommended:
    - "Sign Docker images using Docker Content Trust (DCT) or Sigstore/cosign."
    - "Pin base images to digest (e.g., FROM node:20@sha256:... instead of FROM node:20-latest) to ensure reproducible builds."
    - "Verify base image digest in CI before building."
  note: "CONCERN not REPROCESS: the build pipeline is using official images (partial trust) and GitHub Actions (managed build environment). The gap is artifact integrity verification — unsigned artifacts cannot be verified at deployment time."
```

### Reason

SSDF PW.5 concerns build process security — ensuring artifacts are built from trusted sources and can be verified. Unsigned Docker images and :latest tag base images are CONCERN (not REPROCESS) because the team is using official Docker Hub images in a managed CI environment — the complete absence of a secure build environment would be REPROCESS. The gap is the absence of integrity verification mechanisms (signatures, digest pinning).

### Phrasing B

```
SSDF-PW-Arbiter, PW.5, run PR-5534.
GitHub Actions CI. Docker images unsigned. Base image: node:latest (no digest pin).
Verdict?
```

Expected: CONCERN. PW.5 gap: no artifact signing, no digest pinning. Official images + managed CI = partial trust. Action_recommended: cosign/DCT for image signing, digest pinning for base images.

---

## Pair CB4-3.4-227

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — algorithm confusion: HS256 JWT signed with public key as HMAC secret)
**Source:** IETF RFC 7519, OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter_inputs:
  - arbiter: RFC-JWT-Arbiter
    territory: T-rfc-jwt-normative
    findings:
      - "JWT header: alg=HS256"
      - "JWT signature: HMAC-SHA256 using the RS256 public key as the HMAC secret"
      - "server: validates signature using the public key (passed to HS256 validation as the key)"
      - "team note: 'HS256 is technically valid per RFC 7519; the key material is from our RSA public key'"
  - arbiter: ASVS-V9-Token-Arbiter
    territory: T-token-validation
    findings:
      - "V9.1.1: asymmetric signing claimed"
      - "actual algorithm: HS256 (symmetric HMAC) using public key bytes as HMAC secret"
pipeline_run_id: PR-5545
```

### B

```
arbiter_results:
  RFC-JWT-Arbiter:
    verdict: REPROCESS
    requirements_violated: ["RFC7519-§8-MUST-alg-security", "RFC7519-§4.1.6"]
    gap: "Algorithm confusion attack pattern: server accepts HS256-signed JWTs using the RSA public key as the HMAC secret. The attacker who knows the public key (which is by definition public) can forge JWTs by computing HMAC-SHA256 using the public key bytes. RFC 7519 §8 requires implementations to validate the alg header and reject tokens using algorithms that present security risks in context."
    action_required: "Reject all HS256-signed tokens at the server. Whitelist RS256 (or other asymmetric algorithms) only. The alg header must not be trusted from the token — the server must enforce the expected algorithm."

  ASVS-V9-Token-Arbiter:
    verdict: REPROCESS
    requirements_violated: [V9.1.1]
    gap: "V9.1.1 requires asymmetric signing. HS256 is symmetric regardless of what key material is used. Using a public key as an HMAC secret does not make HS256 asymmetric — it makes the HMAC secret public (defeating HMAC's security guarantee)."
    action_required: "Sign tokens with RS256 (actual asymmetric algorithm). Reject HS256 at the server."
```

### Reason

This is the classic JWT algorithm confusion attack: the attacker changes alg from RS256 to HS256 in the header and signs with the public key (which they have). The server, if it trusts the alg header and uses the same key for HS256 verification, accepts the forged token. Both arbiters return REPROCESS. The key training point: HS256 with public key bytes as HMAC secret is not asymmetric — the HMAC secret is public, defeating HMAC's security entirely.

### Phrasing B

```
RFC-JWT and ASVS-V9 arbiters, run PR-5545.
JWT: alg=HS256, signed with RSA public key bytes as HMAC secret. Server validates via HS256 with public key.
Is this "asymmetric" per V9.1.1?
```

Expected: Both REPROCESS. HS256 with public key = algorithm confusion attack. Public key as HMAC secret = HMAC secret is public = forging is trivial. V9.1.1 violated (not asymmetric). RFC-JWT REPROCESS (alg security violation). Both action_required: whitelist RS256 only, reject HS256.

---

## Pair CB4-3.4-228

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — JWT kid (key ID) injection: arbitrary kid in header)
**Source:** IETF RFC 7519
**Verdict:** REPROCESS

### A

```
arbiter: RFC-JWT-Arbiter
territory: T-rfc-jwt-normative
findings:
  - "JWT header: kid claim present — value is user-controlled; not validated server-side"
  - "server key lookup: uses kid value as a filesystem path to load the signing key"
  - "attack: attacker sets kid=../../etc/passwd; server attempts to open that file as a key"
  - "or: attacker sets kid=https://attacker.example.com/key.json; server fetches external key"
pipeline_run_id: PR-5556
```

### B

```
arbiter_result:
  arbiter: RFC-JWT-Arbiter
  verdict: REPROCESS
  severity: CRITICAL
  gap: "kid (Key ID) claim in JWT header used as filesystem path or URL for key lookup without validation. This creates two attack vectors: (1) Path traversal: kid=../../etc/passwd attempts to read a system file as a signing key; (2) Remote key injection: if kid is a URL, an attacker can point to a self-controlled key and forge JWTs that the server will accept."
  action_required:
    - "Validate kid values against an allowlist of known key IDs before key lookup."
    - "Do not use kid as a filesystem path — use it as a lookup key into a pre-loaded key registry."
    - "If kid is a URL (JWK Set URL), validate the URL is in an allowlist of trusted JWKS endpoints."
  note: "CRITICAL: key injection via kid allows complete JWT forgery. The attacker who controls key lookup controls the signing key, making token verification meaningless."
```

### Reason

kid injection is a critical JWT attack vector. Using kid as a filesystem path or URL without validation allows path traversal or remote key injection. REPROCESS CRITICAL because this completely breaks JWT signature verification.

### Phrasing B

```
RFC-JWT-Arbiter, run PR-5556.
JWT header: kid field used as filesystem path for server-side key lookup. No validation of kid value.
Verdict?
```

Expected: REPROCESS CRITICAL. kid injection = path traversal or remote key injection. Complete JWT forgery possible. Action_required: allowlist of valid key IDs; key registry lookup, not filesystem path.

---

## Pair CB4-3.4-229

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — "load balancer strips auth headers; JWT in URL is the only option")
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter: ASVS-V9-Token-Arbiter
territory: T-token-validation
findings:
  - requirement: V9.3.1
  - "JWT delivery: URL query parameter (?token=...)"
  - "team justification: 'our load balancer strips Authorization headers before passing to the application; URL parameter is the only way to pass JWT'"
pipeline_run_id: PR-5567
```

### B

```
arbiter_result:
  arbiter: ASVS-V9-Token-Arbiter
  verdict: REPROCESS
  requirements_violated: [V9.3.1]
  gap: "JWT delivered via URL query parameter. Load balancer configuration that strips Authorization headers is a deployment infrastructure issue, not a reason to use URL parameter token delivery."
  action_required:
    - "Configure the load balancer to pass Authorization headers to the application without stripping them."
    - "Alternatively, use httpOnly cookies for JWT delivery (also satisfies V9.3.1 — cookies are an acceptable transport mechanism)."
    - "URL parameter delivery must be remediated regardless of load balancer configuration."
  note: "REPROCESS: the load balancer behavior is within the team's control to change. URL parameter token delivery creates credential exposure (logs, browser history, Referer headers) that the team's load balancer justification does not address. Infrastructure configuration is not an exemption from security requirements."
```

### Reason

The load balancer stripping Authorization headers is a fixable infrastructure configuration issue, not a fundamental architectural constraint that exempts the team from V9.3.1. Two alternatives exist: (1) fix the load balancer configuration, (2) use httpOnly cookies. URL parameter token delivery is REPROCESS regardless of the justification.

### Phrasing B

```
V9.3.1. JWT in URL parameter. Team: load balancer strips Authorization headers.
ASVS-V9-Token-Arbiter, run PR-5567.
Does load balancer limitation exempt URL parameter delivery from V9.3.1?
```

Expected: REPROCESS. Load balancer configuration is within team's control to fix. Alternative: httpOnly cookies also satisfy V9.3.1. URL parameter delivery is REPROCESS regardless of infrastructure justification.

---

## Pair CB4-3.4-230

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — "our code review IS our threat model review" SSDF circular argument)
**Source:** NIST SP 800-218 SSDF v1.1
**Verdict:** REPROCESS

### A

```
arbiter: SSDF-PW-Arbiter
territory: T-ssdf-pw2
findings:
  - practice: PW.2
  - "threat model artefact: not maintained as a separate document"
  - "team argument: 'during every code review, our engineers identify security threats in the proposed changes. This continuous, embedded threat modeling during code reviews satisfies PW.2'"
  - "code review records: PR comments showing security discussion on 48 PRs over 12 months"
pipeline_run_id: PR-5578
```

### B

```
arbiter_result:
  arbiter: SSDF-PW-Arbiter
  verdict: REPROCESS
  practice: PW.2
  gap: "PR-level code review security comments do not constitute a threat model. PW.2 requires a threat model that covers the system architecture: trust boundaries, asset inventory, threat actors, and mitigations at the system level. Code review discussions address implementation-level concerns on individual changes — they do not produce a system-level threat model artefact."
  action_required: "Produce a threat model document covering the system architecture: (1) identify assets and trust boundaries; (2) enumerate threat actors and attack vectors against the system; (3) document mitigations in place for each threat. Code reviews remain valuable as a complementary practice (PW.8) but do not substitute for a system-level threat model (PW.2)."
  note: "REPROCESS: the circular argument conflates PW.2 (system threat model) with PW.8 (code review). PR comments on individual code changes cannot accumulate to form a system-level threat model because they lack system-level scope, asset enumeration, and trust boundary analysis."
```

### Reason

Code review security discussions (PW.8) and system threat modeling (PW.2) are different practices. A PR comment saying "this looks like it could be an IDOR" is a code-level security observation, not a system threat model. PW.2 requires a system-level artefact with assets, trust boundaries, and threat actor enumeration. The circular argument is rejected. REPROCESS because no PW.2 artefact exists.

### Phrasing B

```
SSDF-PW-Arbiter, PW.2, run PR-5578.
No threat model document. Team: code review security discussions on 48 PRs = continuous threat modeling.
Does PR-level security review satisfy PW.2?
```

Expected: REPROCESS. Code review = PW.8, not PW.2. PR comments address implementation-level concerns, not system-level threat model (assets, trust boundaries, threat actors). System-level threat model artefact required.

---

## File summary

| Seq | Stratum | Source | Verdict | Domain / adversarial |
|-----|---------|--------|---------|---------------------|
| 223 | A | ASVS | REPROCESS | V10.2.6: state parameter absent; PKCE ≠ state (different protections) |
| 224 | A | CWE | REPROCESS | CWE-288: auth bypass via alternate path (unauthenticated export endpoint) |
| 225 | A | ASVS | REPROCESS | V8.2.2: JWT, passwords, PII in server logs |
| 226 | A | SSDF | CONCERN | SSDF PW.5: unsigned Docker images, :latest base image |
| 227 | D | RFC+ASVS | REPROCESS | Algorithm confusion: HS256 with public key as HMAC secret |
| 228 | D | IETF | REPROCESS | JWT kid injection: filesystem path or remote URL as key source |
| 229 | D | ASVS | REPROCESS | Load balancer argument: infrastructure config is not V9.3.1 exemption |
| 230 | D | SSDF | REPROCESS | Code review circular argument: PW.8 ≠ PW.2 (system threat model) |

**Running total: 460 instances (sequences 001–230)**
Stratum breakdown: A ~31%, B ~24%, C ~29%, D ~16%
