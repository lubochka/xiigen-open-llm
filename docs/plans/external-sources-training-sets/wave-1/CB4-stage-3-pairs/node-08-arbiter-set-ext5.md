# Node 08 — Arbiter Units — Extension Set 5
# Stage 3 Action 3 — Coverage Extension (Stratum D mixed adversarial + Stratum C depth)
# Batch: CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Sequences: CB4-3.4-179 through CB4-3.4-188
# Instances: 20 (10 base pairs × 2 phrasings each)
# Running total after this file: 376 instances (sequences 001–188)

---

## Coverage targets for this file

Mixed adversarial scenarios where multiple failure modes interact or compound.
Also: Stratum C depth pairs for edge cases at the boundary of PASS and CONCERN.

Primary scenarios:
  - OAuth confusion: client_credentials grant submitted as if it satisfies V10 authorization code requirements
  - ASVS V9 with "short expiry" claim: 8-hour exp on API access token
  - CWE-862 vs V8 discrepancy: V8 PASS but CWE-Authz finds a gap in a different endpoint
  - SSDF PW.2 exact one-year boundary: dated exactly 12 months ago
  - RFC 7519 iss claim: team uses IP address as iss value (not URI)
  - ASVS V10 + SSDF PW combined: OAuth claims + design review artefact
  - MFA TOTP with 90-second window: extended window = CONCERN?
  - JWT with "nbf" only, no exp: valid without exp per RFC, but ASVS gap
  - CWE-287 with hardware token MFA: correct authentication = PASS
  - V7 session fixation: fixed session ID not regenerated after login

---

## Pair CB4-3.4-179

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — client_credentials grant submitted for user authorization flow)
**Source:** OWASP ASVS 5.0.0, IETF RFC 6749
**Verdict:** CONCERN

### A

```
arbiter: ASVS-V10-OAuth-Arbiter
territory: T-oauth-oidc
findings:
  - "OAuth grant type used: client_credentials"
  - "team claim: V10.2.1 satisfied — using authorization flow"
  - "V10.4.6 (PKCE): not applicable per team — client_credentials does not use authorization code"
  - "system: machine-to-machine API calls between microservices; no user interaction"
pipeline_run_id: PR-5089
```

### B

```
arbiter_result:
  arbiter: ASVS-V10-OAuth-Arbiter
  verdict: CONCERN
  requirements: [V10.2.1]
  gap: "client_credentials grant is used for machine-to-machine authentication. V10.2.1 targets user-delegated authorization flows (authorization code + PKCE). client_credentials satisfies a different use case — it cannot satisfy V10.2.1 because V10.2.1 requires user consent and authorization code exchange."
  action_recommended: "Clarify the use case scope. If this endpoint serves only machine-to-machine API calls with no user delegation, V10.2.1 may not apply — re-scope the assessment to the relevant V10 sections for service-to-service calls. If user-delegated authorization is also required, implement authorization code + PKCE separately."
  note: "CONCERN not REPROCESS: the client_credentials grant is a legitimate OAuth 2.0 grant type for machine-to-machine scenarios. The gap is the team's claim that it satisfies V10.2.1 — it does not. If M2M is the only use case, the correct V10 requirements differ."
```

### Reason

client_credentials grant is for machine-to-machine flows where no user is present. V10.2.1 governs user-delegated authorization (authorization code flow). The team incorrectly cited V10.2.1 as satisfied by client_credentials. CONCERN (not REPROCESS) because the grant type is legitimate for the stated use case; the issue is the incorrect V10.2.1 claim. The action_recommended guides the team to re-scope their ASVS assessment to the correct V10 requirements for M2M flows.

### Phrasing B

```
ASVS-V10-OAuth-Arbiter, run PR-5089.
Grant type: client_credentials. Team claims V10.2.1 satisfied.
PKCE not applicable per team (no auth code flow).
System: machine-to-machine microservice calls.
Verdict?
```

Expected: CONCERN. client_credentials does not satisfy V10.2.1 (user-delegated authorization code flow). M2M use case is legitimate; V10.2.1 claim is incorrect. Re-scope to relevant M2M OAuth requirements.

---

## Pair CB4-3.4-180

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — API access token with 8-hour expiry; is this ASVS V9.2.1 CONCERN or REPROCESS?)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
arbiter: ASVS-V9-Token-Arbiter
territory: T-token-validation
findings:
  - requirement: V9.2.1
  - "JWT access tokens: exp=8 hours from issuance"
  - "use case: long-running data processing jobs that run for up to 6 hours; tokens must remain valid for the duration of the job"
  - "RS256 signing: confirmed"
  - "no token refresh mechanism for long-running jobs"
pipeline_run_id: PR-5100
```

### B

```
arbiter_result:
  arbiter: ASVS-V9-Token-Arbiter
  verdict: CONCERN
  requirements: [V9.2.1]
  gap: "JWT access token expiry of 8 hours exceeds the typically recommended short expiry (≤15 minutes for API access tokens). V9.2.1 requires 'short expiry' — the definition of 'short' depends on the use case, but 8 hours is notably longer than standard API access token lifetimes."
  context_consideration: "Long-running job use case (6-hour jobs) is a legitimate reason for longer expiry if no token refresh mechanism is available. However, 8 hours is 2 hours beyond the stated maximum job duration — the overage creates an unnecessary exposure window."
  action_recommended:
    - "Implement a token refresh mechanism for long-running jobs (issue a new short-lived token when the job requests it)."
    - "If refresh is not feasible: scope the expiry to job_duration + buffer (e.g., 7 hours for a 6-hour job) rather than a fixed 8-hour default for all tokens."
    - "Consider per-job token issuance with a tight expiry matching the expected job duration."
  note: "CONCERN not REPROCESS: the use case provides legitimate context for longer expiry. The gap is the lack of a refresh mechanism and the 2-hour overage beyond the stated job duration."
```

### Reason

8-hour JWT access token expiry. V9.2.1 requires "short expiry." The use case (6-hour jobs) provides context. The verdict is CONCERN (not REPROCESS) because the use case is legitimate and the team has a documented reason for the longer expiry. However, 8 hours is 2 hours beyond the stated need, and the absence of a refresh mechanism means all tokens regardless of actual job duration get 8-hour validity. The action_recommended provides three paths to remediation.

### Phrasing B

```
V9.2.1. JWT access tokens exp=8h. Long-running jobs up to 6 hours. No refresh mechanism.
ASVS-V9-Token-Arbiter, run PR-5100.
Is 8-hour expiry REPROCESS or CONCERN?
```

Expected: CONCERN. Legitimate use case for longer expiry (6-hour jobs). But 8h > stated need + no refresh mechanism. Action_recommended: refresh mechanism or tighter expiry per job duration. Not REPROCESS (use case context acknowledged).

---

## Pair CB4-3.4-181

**Node:** 08 — Arbiter Units
**Stratum:** C (depth — SSDF PW.2 exactly at one-year boundary)
**Source:** NIST SP 800-218 SSDF v1.1
**Verdict:** PASS

### A

```
arbiter: SSDF-PW-Arbiter
territory: T-ssdf-pw2
findings:
  - practice: PW.2
  - "threat model artefact: threat-model-v5.pdf"
  - "last review date recorded in document: 2025-05-05"
  - "current date: 2026-05-05"
  - "gap from last review to today: exactly 12 months"
pipeline_run_id: PR-5111
```

### B

```
arbiter_result:
  arbiter: SSDF-PW-Arbiter
  verdict: PASS
  practice: PW.2
  satisfied: ["PW.2.1"]
  notes: "Threat model reviewed 2025-05-05 — exactly 12 months ago as of 2026-05-05. The recommended annual review cycle is satisfied. The artefact with dated review is present."
  recommendation: "Schedule the next review for 2027-05-05 (or earlier if significant architectural changes occur before that date)."
```

### Reason

At exactly 12 months since the last review, SSDF PW.2 is still PASS (the annual cycle is satisfied — "annual" means within 12 months, and 12 months exactly is at the boundary, still satisfied). The recommendation notes the next review date. This edge case establishes that the 12-month boundary is inclusive — PASS at exactly 12 months, CONCERN approaching it (e.g., 10-11 months), REPROCESS when exceeded.

### Phrasing B

```
SSDF-PW-Arbiter, PW.2, run PR-5111.
Threat model last reviewed 2025-05-05. Current date: 2026-05-05. Exactly 12 months.
Is 12 months exactly within the annual cycle, or is it already in the CONCERN zone?
```

Expected: PASS. 12 months exactly satisfies the annual cycle. Boundary is inclusive. Recommendation: schedule next review for 2027-05-05.

---

## Pair CB4-3.4-182

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — RFC 7519 iss claim with IP address instead of URI)
**Source:** IETF RFC 7519
**Verdict:** CONCERN

### A

```
arbiter: RFC-JWT-Arbiter
territory: T-rfc-jwt-normative
findings:
  - requirement: "RFC7519-§4.1.1 (iss claim)"
  - "iss claim value: '192.168.10.15' (internal IP address of the issuing service)"
  - "RFC 7519 §4.1.1: 'The iss (issuer) Claim identifies the principal that issued the JWT. The processing of this claim is generally application specific. The iss value is a case-sensitive string containing a StringOrURI value.'"
  - "team note: 'we use the internal IP as the issuer since the service has a static IP'"
pipeline_run_id: PR-5122
```

### B

```
arbiter_result:
  arbiter: RFC-JWT-Arbiter
  verdict: CONCERN
  requirements: ["RFC7519-§4.1.1"]
  gap: "iss claim contains an IP address (192.168.10.15). RFC 7519 §4.1.1 requires the iss value to be a 'StringOrURI' — if the value contains a colon ':', it must be a URI. An IP address is not a URI and does not contain a colon in standard IPv4 form, so it is technically a valid 'String' under RFC 7519 §4.1.1. However, IP addresses as issuer identifiers create practical problems: IP addresses can be reused (NAT, container restarts), making the issuer identifier unstable."
  action_recommended: "Use a stable URI as the iss value (e.g., 'https://auth.internal.example.com' or 'urn:auth-service:prod'). This provides a stable, environment-independent identifier that survives IP address changes."
  note: "CONCERN not REPROCESS: RFC 7519 §4.1.1 does not technically prohibit an IP-address iss value — it is a StringOrURI and an IP is a valid string. The concern is operational stability, not a MUST violation. However, many JWT validation libraries require iss to be a URI and may reject this token."
```

### Reason

RFC 7519 §4.1.1 requires iss to be a StringOrURI. An IP address is technically a valid String (it contains no colon, so it does not need to be a URI). The concern is operational (IP address instability) and library compatibility (many validators expect a URI). CONCERN not REPROCESS because the RFC is technically not violated. This is a nuanced case: technically valid, practically problematic.

### Phrasing B

```
RFC-JWT-Arbiter, run PR-5122.
iss claim: '192.168.10.15' (internal IP address).
RFC 7519 §4.1.1 requires StringOrURI. Is an IP address valid?
```

Expected: CONCERN. IP address is technically a valid String under RFC 7519 §4.1.1 (no colon = not required to be URI). Not REPROCESS (no MUST violation). Practical concern: IP instability, library compatibility. Recommend stable URI.

---

## Pair CB4-3.4-183

**Node:** 08 — Arbiter Units
**Stratum:** C (depth — JWT with nbf only and no exp: RFC PASS, ASVS REPROCESS)
**Source:** IETF RFC 7519, OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter_inputs:
  - arbiter: RFC-JWT-Arbiter
    findings:
      - "JWT has nbf (not-before) claim set to issuance time"
      - "JWT has no exp claim"
      - "RS256 signing"
      - "iss and aud present"
  - arbiter: ASVS-V9-Token-Arbiter
    findings:
      - "V9.1.1: RS256 — satisfied"
      - "V9.2.1: no exp claim — gap"
      - "nbf is present but exp is absent"
pipeline_run_id: PR-5133
```

### B

```
arbiter_results:
  - arbiter: RFC-JWT-Arbiter
    verdict: PASS
    satisfied: [RFC7519-§4.1.1, RFC7519-§4.1.3, RFC7519-§4.1.5, RFC7519-§4.1.6]
    note: "RFC 7519 §4.1.4 (exp) is OPTIONAL. Absence of exp does not violate RFC 7519. nbf present and iss, aud, RS256 satisfy MUST-level requirements. RFC-JWT-Arbiter: PASS."

  - arbiter: ASVS-V9-Token-Arbiter
    verdict: REPROCESS
    satisfied: [V9.1.1]
    requirements_violated: [V9.2.1]
    gap: "V9.2.1 requires a short expiry (exp claim). nbf claim establishes a not-before boundary but does not establish a not-after boundary — the token has no expiry and remains valid indefinitely after the nbf time."
    action_required: "Add exp claim to all issued JWTs. Set expiry to a short duration (≤15 minutes for API access tokens)."
```

### Reason

This is the nbf/exp variant of pair 118. nbf (not-before) establishes when the token becomes valid but not when it expires. RFC 7519 marks exp as OPTIONAL, so the RFC-JWT-Arbiter returns PASS. ASVS V9.2.1 requires short expiry — absent exp = REPROCESS. Same pattern as pair 118 but with nbf present (confirming that nbf does not satisfy exp).

### Phrasing B

```
JWT: nbf set, no exp. RS256. iss and aud present.
RFC-JWT-Arbiter and ASVS-V9-Token-Arbiter both run. PR-5133.
What does each return?
```

Expected: RFC-JWT PASS (exp optional per RFC). V9-Token REPROCESS (V9.2.1: nbf ≠ exp; token has no expiry boundary). Overall: REPROCESS.

---

## Pair CB4-3.4-184

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — CWE-287 with hardware MFA token: correct authentication PASS)
**Source:** CWE Top 25 2023
**Verdict:** PASS

### A

```
arbiter: CWE-Authn-Arbiter
territory: T-cwe-authn
findings:
  - "authentication mechanism: FIDO2 hardware security key (YubiKey)"
  - "login flow: username + FIDO2 WebAuthn assertion"
  - "no password in the flow — passwordless authentication"
  - "private key stored on hardware device; cannot be exported"
  - "server verifies WebAuthn assertion using registered public key"
pipeline_run_id: PR-5144
```

### B

```
arbiter_result:
  arbiter: CWE-Authn-Arbiter
  verdict: PASS
  finding: "No CWE-287 (Improper Authentication) pattern identified. FIDO2 hardware security key with WebAuthn assertion provides strong authentication: private key is hardware-bound (cannot be exported), server verifies the assertion using a registered public key, and there is no shared secret that could be phished or intercepted."
  note: "Passwordless FIDO2 authentication is resistant to phishing, credential stuffing, and replay attacks. CWE-Authn-Arbiter: PASS."
```

### Reason

FIDO2 hardware security key (WebAuthn) is a strong authentication mechanism. CWE-287 is Improper Authentication — the arbiter checks for weakness in the authentication mechanism. Hardware-bound private keys, server-side assertion verification, and phishing resistance mean there is no CWE-287 pattern. PASS because the authentication mechanism is robust.

### Phrasing B

```
CWE-Authn-Arbiter, run PR-5144.
Authentication: FIDO2 hardware key (YubiKey), WebAuthn assertion, passwordless.
Private key hardware-bound. Server verifies with registered public key.
Verdict?
```

Expected: PASS. No CWE-287 pattern. FIDO2 hardware key = strong authentication (hardware-bound, phishing-resistant, no shared secret).

---

## Pair CB4-3.4-185

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — V7 session fixation: session ID not regenerated after login)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter: ASVS-V7-Session-Arbiter
territory: T-asvs-session
findings:
  - requirement: V7.2.3
    status: "session ID remains the same before and after login — no session regeneration on authentication"
    impact: "Session fixation attack: attacker can set a known session ID on the victim's browser (e.g., via URL injection), the victim logs in, and the attacker uses the known session ID to take over the authenticated session"
pipeline_run_id: PR-5155
```

### B

```
arbiter_result:
  arbiter: ASVS-V7-Session-Arbiter
  verdict: REPROCESS
  requirements_violated: [V7.2.3]
  gap: "Session ID not regenerated after successful login. V7.2.3 requires session ID regeneration after authentication to prevent session fixation attacks."
  action_required: "Generate a new session ID upon successful authentication. The pre-login session ID must be invalidated and a new session ID issued as part of the login response. Implement server-side, not client-side, session ID regeneration."
  note: "Session fixation is a distinct attack from session replay (V7.3.1). Fixation requires the attacker to plant a known session ID before login; replay requires capturing a post-login session token. Both V7.2.3 and V7.3.1 must be addressed independently."
```

### Reason

Session fixation (V7.2.3) is a distinct attack from session replay (V7.3.1). Session fixation exploits the failure to regenerate the session ID at login. REPROCESS because V7.2.3 is a MUST-level requirement. The note explains the distinction between fixation and replay to prevent the team from conflating the two.

### Phrasing B

```
ASVS-V7-Session-Arbiter, V7.2.3, run PR-5155.
Session ID same before and after login — no regeneration on authentication.
Verdict?
```

Expected: REPROCESS. V7.2.3 violated. Session fixation risk. Action_required: generate new session ID on login, invalidate pre-login session ID. Note distinguishes fixation (V7.2.3) from replay (V7.3.1).

---

## Pair CB4-3.4-186

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — MFA TOTP with 90-second window: CONCERN or PASS?)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
arbiter: ASVS-V6-Auth-Arbiter
territory: T-asvs-authentication
findings:
  - requirement: V6.3.1
  - "TOTP implementation: 90-second window (3 × 30-second periods)"
  - "standard TOTP: 30-second periods with ±1 period tolerance (90-second valid window)"
  - "team note: 90-second window matches RFC 6238 recommendation of T0 ± 1 period"
  - "RS256 JWT for session — not relevant to TOTP evaluation"
pipeline_run_id: PR-5166
```

### B

```
arbiter_result:
  arbiter: ASVS-V6-Auth-Arbiter
  verdict: CONCERN
  requirements: [V6.3.1]
  gap: "TOTP with a 90-second acceptance window (±1 period tolerance). Standard TOTP allows ±1 period (T0-1, T0, T0+1) to account for clock skew. This is within RFC 6238 SHOULD guidance and is common practice. However, a 90-second window means a TOTP code is valid for a 90-second window, which may exceed minimal-exposure TOTP best practice for high-security environments."
  action_recommended: "For standard applications: ±1 period (90-second window) is acceptable and aligns with RFC 6238. For high-security environments: reduce to ±0 periods (30-second window with time synchronization) to minimize the attack window."
  note: "CONCERN not REPROCESS: ±1 period is within RFC 6238 SHOULD guidance and is industry standard. The concern is context-specific — acceptable for most applications, potentially too broad for high-value targets."
```

### Reason

TOTP ±1 period (90-second window) is the RFC 6238 SHOULD recommendation for handling clock skew. It is industry standard. The verdict is CONCERN (not REPROCESS) because this is within RFC 6238 guidance. The concern is context-specific — acceptable for most environments, potentially worth tightening for high-security targets. This is a nuanced case: technically compliant with RFC, concern is contextual.

### Phrasing B

```
TOTP: 90-second window (±1 period, RFC 6238 SHOULD recommendation). Is this PASS or CONCERN?
ASVS-V6-Auth-Arbiter, run PR-5166.
```

Expected: CONCERN (contextual). ±1 period is RFC 6238 SHOULD and industry standard. CONCERN because it may be too broad for high-security environments. Not REPROCESS (no MUST violation). Note: acceptable for most applications.

---

## Pair CB4-3.4-187

**Node:** 08 — Arbiter Units
**Stratum:** C (depth — V10 + SSDF PW combined: OAuth claims + design review artefact)
**Source:** OWASP ASVS 5.0.0, NIST SP 800-218 SSDF v1.1
**Verdict:** PASS

### A

```
arbiter_inputs:
  - arbiter: ASVS-V10-OAuth-Arbiter
    territory: T-oauth-oidc
    findings:
      - "V10.2.1: authorization code + PKCE S256 — confirmed"
      - "V10.2.3: redirect URI exact match enforced — confirmed"
      - "V10.4.6: PKCE S256 verifier confirmed"
  - arbiter: SSDF-PW-Arbiter
    territory: T-ssdf-pw2
    findings:
      - "PW.2.1: threat model reviewed 2026-02-15 — 2.5 months ago"
      - "threat model covers OAuth 2.0 authorization flows explicitly"
pipeline_run_id: PR-5177
```

### B

```
arbiter_results:
  - arbiter: ASVS-V10-OAuth-Arbiter
    verdict: PASS
    satisfied: [V10.2.1, V10.2.3, V10.4.6]
    summary: "Authorization code + PKCE S256 with exact redirect URI match satisfies V10 OAuth requirements."

  - arbiter: SSDF-PW-Arbiter
    verdict: PASS
    satisfied: [PW.2.1]
    summary: "Threat model reviewed 2026-02-15 (2.5 months ago); covers OAuth 2.0 flows explicitly. PW.2 satisfied."
```

### Reason

Both arbiters return PASS. The SSDF-PW.2 finding is notable: the threat model explicitly covers the OAuth 2.0 authorization flows that are being assessed in V10 — this is a good practice (threat model aligned with the implementation domain being assessed). Both arbiters evaluate independently.

### Phrasing B

```
Run PR-5177. ASVS-V10: auth code + PKCE S256 + redirect URI. SSDF-PW.2: threat model 2026-02-15, covers OAuth flows.
Two arbiters. Both return what?
```

Expected: ASVS-V10-OAuth-Arbiter PASS. SSDF-PW-Arbiter PASS. Both independent. Overall (Node 09): PASS.

---

## Pair CB4-3.4-188

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — V8.1.2 deny-by-default: default ALLOW with explicit deny list)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter: ASVS-V8-AuthZ-Arbiter
territory: T-asvs-authz
findings:
  - requirement: V8.1.2
    status: "authorization policy: default ALLOW for all resources; explicit DENY list maintained for sensitive endpoints (/admin/*, /api/reports/*)"
    team_note: "we use a deny-list approach; all new endpoints are accessible by default unless added to the deny list"
pipeline_run_id: PR-5188
```

### B

```
arbiter_result:
  arbiter: ASVS-V8-AuthZ-Arbiter
  verdict: REPROCESS
  requirements_violated: [V8.1.2]
  gap: "Default ALLOW with explicit DENY list does not satisfy V8.1.2 (deny by default). V8.1.2 requires that access be denied unless explicitly granted. A deny-list approach means any new endpoint is accessible to all authenticated users until someone remembers to add it to the deny list — new functionality is insecure by default."
  action_required: "Invert the authorization policy: default DENY for all resources; explicit ALLOW grants required for each endpoint. Sensitive endpoints and all other endpoints must require an explicit permission grant before returning data."
  note: "Deny-list approaches are architecturally insecure for authorization: when new endpoints are added, they are accessible by default. Allow-list approaches (default deny, explicit grant) ensure that authorization must be actively established for every resource."
```

### Reason

V8.1.2 requires deny-by-default: access must be denied unless explicitly permitted. The team's deny-list approach is the opposite: access is allowed by default unless explicitly denied. This is a REPROCESS because V8.1.2 is a MUST-level architectural requirement. New endpoints in a deny-list system are insecure until someone manually adds them to the deny list — human error creates authorization gaps.

### Phrasing B

```
ASVS-V8-AuthZ-Arbiter, V8.1.2, run PR-5188.
Authorization policy: default ALLOW; sensitive endpoints on deny list.
Team: "new endpoints default to accessible; we add them to the deny list when needed."
Does deny-list satisfy deny-by-default?
```

Expected: REPROCESS. Deny-list = default ALLOW, which violates V8.1.2 (deny-by-default). Must invert: default DENY + explicit ALLOW grants. Architectural policy change required.

---

## File summary

| Seq | Stratum | Source | Verdict | Key scenario |
|-----|---------|--------|---------|-------------|
| 179 | D | ASVS | CONCERN | client_credentials ≠ V10.2.1 (user-delegated auth code flow) |
| 180 | D | ASVS | CONCERN | 8-hour JWT exp: legitimate use case context = CONCERN not REPROCESS |
| 181 | C | SSDF | PASS | PW.2 at exactly 12 months: boundary is inclusive, PASS |
| 182 | D | IETF | CONCERN | RFC 7519 iss with IP address: technically valid String, operationally problematic |
| 183 | C | RFC+ASVS | REPROCESS | JWT with nbf only, no exp: RFC PASS, ASVS V9.2.1 REPROCESS |
| 184 | A | CWE | PASS | CWE-287 FIDO2 hardware key: strong authentication = PASS |
| 185 | A | ASVS | REPROCESS | V7.2.3 session fixation: session ID not regenerated after login |
| 186 | D | ASVS | CONCERN | TOTP 90-second window: RFC 6238 SHOULD compliant, context-specific concern |
| 187 | C | ASVS+SSDF | PASS | V10 + SSDF PW.2 combined: both PASS |
| 188 | D | ASVS | REPROCESS | V8.1.2 deny-by-default violated by deny-list approach |

**Running total: 376 instances (sequences 001–188)**
Stratum breakdown: A ~30%, B ~23%, C ~29%, D ~18%
