# CB4 Stage 3 — Node 08 Arbiter Set Extension 15
# Sequences CB4-3.4-387 through CB4-3.4-394
# Stratum: A/B/D — baselines, team context, adversarial rejection, edge cases
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 788 instances

---

## Pair[CB4-3.4-387]

**A:** PKCE enforcement review. Authorization server is configured with PKCE required for all public clients. Confidential clients (those with a registered client_secret) are exempt from PKCE — PKCE is optional for them. For confidential clients, no code_challenge is required in the authorization request and no code_verifier is checked at token exchange. The team states: "RFC 7636 was designed for public clients. Confidential clients have a client_secret for authentication — that's equivalent protection. ASVS V10.4.6 targets public clients."

**B:**
```
Arbiter: ASVS-V10-OAuth

V10.4.6 (PKCE S256): CONCERN
Team argument partially acknowledged: RFC 7636 was originally motivated primarily
by the needs of public clients (native apps, SPAs) where client secrets cannot
be securely stored. Confidential clients can authenticate via client_secret.

However, ASVS V10.4.6 does not exclude confidential clients, and the team's
argument is incomplete:
(1) PKCE protects against authorization code interception, which is an attack
    on the authorization code in transit — not on the client authentication step.
    A confidential client still receives an authorization code via redirect URI.
    If that code is intercepted before the client exchanges it, the attacker can
    attempt the exchange (with a stolen or guessed verifier).
(2) However, for confidential clients, the token exchange requires the
    client_secret — an intercepted code alone is insufficient without the secret.
    This provides meaningful mitigation compared to public clients.
(3) The residual risk is lower for confidential clients, but not zero.

Finding level: CONCERN — confidential client authentication (client_secret)
provides meaningful mitigation for code interception; PKCE absence is not as
critical as for public clients, but V10.4.6 does not carve out an explicit
exemption for confidential clients in ASVS 5.0.0.

Recommendation: implement PKCE for all clients regardless of confidentiality;
or document the risk acceptance for confidential client exemption.
Overall verdict: CONCERN
```

**Reason:** RFC 7636 was motivated by public clients, and confidential client authentication with client_secret provides meaningful mitigation for authorization code interception (attacker needs both code and secret). However, V10.4.6 does not explicitly exclude confidential clients, and the protection is not equivalent — it reduces risk rather than eliminating it. CONCERN rather than REPROCESS given the meaningful mitigation.

**Phrasing-B:** PKCE required for public clients; optional for confidential clients (they have client_secret). RFC 7636 targets public clients; confidential clients have secret-based auth. Does exempting confidential clients from PKCE satisfy V10.4.6, or is this a CONCERN?

---

## Pair[CB4-3.4-388]

**A:** Token lifetime review. Access tokens have a 15-minute expiry. Refresh tokens have a 24-hour expiry. On logout: access token revoked immediately; refresh token revoked immediately. No token rotation on refresh — the same refresh token is reused for all token refreshes within the 24-hour window. The team states: "Refresh token rotation (issuing a new refresh token on each use) is not implemented. We revoke on logout, so rotation isn't necessary."

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V7-Session

V10.3 (token revocation on logout): PASS
  Both access and refresh tokens revoked on logout ✓
  Logout-triggered revocation is correctly implemented.

Refresh token rotation: CONCERN
  V10.3 and V7.3.1 require revocation on logout — satisfied.
  Refresh token rotation is a defense-in-depth control against refresh token
  theft between login and logout:
  (1) Without rotation, a stolen refresh token remains valid for the full 24-hour
      window — the attacker can use it until the user explicitly logs out.
  (2) With rotation, each use of the refresh token invalidates the old token
      and issues a new one — a stolen token used by the attacker is detectable
      (the legitimate client's next refresh will fail with the invalidated token).
  (3) The 24-hour window for a stolen refresh token is meaningful; refresh token
      theft is a real attack vector.

Team argument ("revocation covers it"): partially valid — logout revocation is
correctly implemented. The gap is the absence of theft-detection capability
between session start and logout.

Finding level: CONCERN (not REPROCESS). Logout revocation is implemented; the
gap is the absence of rotation as a theft-detection mechanism, not a missing
core requirement.
Overall verdict: CONCERN
```

**Reason:** V10.3 logout revocation is correctly implemented (PASS). Refresh token rotation is a security improvement but not a MUST-level requirement in ASVS V10.3 — it is defense-in-depth. Without rotation, a stolen refresh token is usable for the full 24-hour window without detection. CONCERN because the core requirement (logout revocation) is met; rotation is an additional mitigation.

**Phrasing-B:** Access tokens 15 min, refresh tokens 24 hours, both revoked on logout. No refresh token rotation — same token reused until logout. Logout revocation is implemented. Is the absence of rotation a CONCERN or a REPROCESS given that logout revocation works?

---

## Pair[CB4-3.4-389]

**A:** MFA (Multi-Factor Authentication) review. Application supports TOTP (Time-based One-Time Password) as a second factor. MFA enrollment is optional — users can choose to enable or disable TOTP for their account. Users with MFA disabled authenticate with username + password only. The team states: "We support MFA — users who want extra security can enable it. V6.3 says MFA must be available, and it is."

**B:**
```
Arbiter: ASVS-V6-Auth

V6.3.x (MFA — second factor): CONCERN
Team argument partially accepted: MFA is implemented and available.

ASVS V6.3 requirements depend on the application's classification (Level 1/2/3).
For general-purpose applications at Level 2:
(1) MFA being available (optional enrollment) provides the mechanism.
(2) However, for high-value operations or sensitive user data, ASVS Level 2
    typically requires that MFA be enforced for specific operations, not merely
    available as an option.
(3) The specific sub-requirement depends on the application's ASVS level and
    what operations the application performs.

Assessment framework:
  - If the application is assessed at Level 1: optional MFA availability may
    satisfy the requirement.
  - If the application is assessed at Level 2 with sensitive data or financial
    operations: enforcement for high-risk operations is required, not just availability.
  - The team's statement "must be available" reflects a Level 1 reading.

Finding: CONCERN — MFA is available (positive); whether optionality satisfies
the applicable ASVS level depends on the application classification. For any
application handling sensitive user data, optional MFA is insufficient.

Note: if the application is Level 1, this may be PASS. Insufficient context
provided to determine ASVS level. Finding elevated to CONCERN pending level
clarification.
Overall verdict: CONCERN (pending level determination)
```

**Reason:** Optional MFA enrollment satisfies "MFA must be available" at Level 1. At Level 2 (which applies to applications handling sensitive data), ASVS requires MFA enforcement for at least high-risk operations — not just availability. CONCERN pending the application's ASVS level: if Level 1, this is likely PASS; if Level 2, optional-only is insufficient for sensitive operations.

**Phrasing-B:** TOTP MFA available but optional — users choose whether to enable it. V6.3 says MFA must be available and it is. Is optional MFA enrollment a PASS or a CONCERN? Does it depend on the application's ASVS level?

---

## Pair[CB4-3.4-390]

**A:** Redirect URI review with additional argument. Application registers five exact redirect URIs. The team submits documentation showing that all five URIs were reviewed and approved in a security design review six months ago. The security design review was conducted by the security team and the approval is documented. Team: "The redirect URIs were security-reviewed and approved. The approval document confirms the URIs are acceptable."

**B:**
```
Arbiter: ASVS-V10-OAuth

V10.2.1 (exact redirect URI registration): PASS
The redirect URI registration satisfies V10.2.1:
(1) Five exact redirect URIs are registered — character-exact match configured.
(2) Exact match is the requirement; it is satisfied.

Security design review documentation: noted positively.
V10.2.1 does not require a security design review of redirect URIs — exact
registration is the requirement, and it is met. The documentation represents
additional security governance above the compliance requirement.

Note on argument structure: the team argues "approved by security review" as
evidence of V10.2.1 compliance. This argument structure is similar to "pen test
cleared it" — approval/clearance does not substitute for requirement satisfaction.
However, in this case, the URIs are also exactly registered — so the
argument is redundant to the actual compliance evidence, not a substitute for it.

V10.2.1 is satisfied by the exact registration itself.
Overall verdict: PASS
```

**Reason:** Five exact redirect URIs — V10.2.1 is satisfied on its own merits. The security design review documentation is noted positively but not required by V10.2.1. The PASS is based on the exact registration, not on the approval document. This pair distinguishes the correct acceptance pattern: the security review argument is noted but the verdict rests on the actual technical evidence.

**Phrasing-B:** Five exact redirect URIs registered. Security team reviewed and approved them six months ago with documentation. Does the security design review approval add to the V10.2.1 compliance evidence, or is the exact registration itself what matters?

---

## Pair[CB4-3.4-391]

**A:** JWT `nbf` (not-before) claim review. The application issues JWTs with an `nbf` claim set 5 seconds in the past (`nbf = iat - 5`). This is intentional — the 5-second clock skew allowance prevents token rejection due to slight clock differences between the token issuer and verifiers. The JWT validation library checks `exp`, `iat`, and `nbf`. The team states: "The 5-second skew is a standard practice for handling distributed system clock drift."

**B:**
```
Arbiter: RFC-JWT + ASVS-V9-Token

RFC 7519 §4.1.5 (nbf claim): PASS
RFC 7519 defines `nbf` as the time before which the JWT must not be accepted.
Setting nbf = iat - 5:
(1) Creates a 5-second window before issuance time when the token would
    technically be valid — this is backward from the typical use.
(2) In practice: the token is issued at time T, nbf = T-5, so the token is
    valid from T-5 seconds. This is a 5-second clock skew allowance.
(3) RFC 7519 does not specify exact nbf value relative to iat; a small
    backward offset for clock skew accommodation is a recognized practice.
(4) The validation library checks nbf — nbf is not being ignored.

A 5-second backward nbf offset for distributed system clock skew:
- Is a minimal, bounded clock skew accommodation ✓
- Does not create a meaningful exploitation window ✓
- Is a standard pattern for clock-synchronized JWT ecosystems ✓

V9.1 (token validation): PASS — exp, iat, and nbf all validated

Finding: PASS — 5-second clock skew accommodation via nbf offset is acceptable.
Note: a larger backward offset (e.g., 30 minutes) would be a finding; 5 seconds
is within acceptable clock skew tolerance.
Overall verdict: PASS
```

**Reason:** A 5-second backward `nbf` offset for clock skew accommodation is a standard and acceptable JWT practice. RFC 7519 does not prohibit it, and the validation library actively checks `nbf` — the claim is not ignored. The small window (5 seconds) does not create an exploitable vulnerability. PASS.

**Phrasing-B:** JWTs issued with nbf = iat - 5 seconds to handle clock drift in distributed systems. Validation library checks exp, iat, and nbf. Is the 5-second backward nbf offset a concern, or is it an accepted clock skew accommodation?

---

## Pair[CB4-3.4-392]

**A:** Introspection endpoint review. OAuth authorization server exposes a token introspection endpoint (`POST /oauth/introspect`) as per RFC 7662. The endpoint is publicly accessible — no authentication is required to call it. Any caller can submit a token and receive the introspection response including `active`, `sub`, `scope`, `exp`, and `client_id`. The team states: "RFC 7662 defines the introspection endpoint. We implement the spec."

**B:**
```
Arbiter: RFC-OAuth2 + ASVS-V10-OAuth

RFC 7662 §2.1 (introspection endpoint security): REPROCESS
Team argument rejected: "we implement the spec."

RFC 7662 §2.1 explicitly requires that the introspection endpoint be protected:
"To prevent token scanning attacks, the endpoint MUST require some form of
authorization to access this endpoint, such as client authentication as
described in OAuth 2.0 [RFC6749] or a separate OAuth 2.0 access token."

A publicly accessible introspection endpoint:
(1) Violates RFC 7662 §2.1 — the MUST requirement for caller authentication
    is not met.
(2) Enables token scanning: any party can submit arbitrary token values to
    determine whether they are active and learn associated metadata (sub, scope,
    client_id, exp).
(3) Metadata disclosure: active tokens reveal the subject identifier, scopes,
    and client — privacy and security impact.

The team "implementing the spec" without the §2.1 authentication requirement
is not implementing the spec — it is implementing the response format while
omitting a MUST security control.

V10 / RFC-OAuth2 verdict: REPROCESS
Evidence gap: no caller authentication on introspection endpoint.
Overall verdict: REPROCESS
```

**Reason:** RFC 7662 §2.1 explicitly requires that the introspection endpoint require "some form of authorization" — this is a MUST. A publicly accessible introspection endpoint violates this requirement and enables token scanning (probing arbitrary values to find active tokens) and metadata disclosure. "We implement the spec" is rejected because the authentication requirement is part of the spec.

**Phrasing-B:** Token introspection endpoint at POST /oauth/introspect, no authentication required — anyone can query it. RFC 7662 defines this endpoint and we implement it. Is an open introspection endpoint compliant with RFC 7662?

---

## Pair[CB4-3.4-393]

**A:** JWT key rotation review. The application uses RS256 for JWT signing. The RSA signing key pair is generated once at application deployment and never rotated. The public key is served at `/.well-known/jwks.json`. Key ID (kid) is set in all issued JWTs. The team states: "Key rotation is complex. Our key hasn't been compromised — we have no reason to rotate. RFC 7517 doesn't mandate rotation frequency."

**B:**
```
Arbiter: ASVS-V9-Token + RFC-JWT

V9.2.x (signing key management): CONCERN
Team argument partially accepted: RFC 7517 (JWK Sets) does not specify a
mandatory rotation frequency. There is no hard MUST in RFC 7517 for rotation
cadence.

However, a CONCERN is warranted:
(1) A key that has never been rotated since deployment has an ever-increasing
    exposure window. If the key is compromised at any point after deployment,
    all tokens issued from deployment to discovery are at risk.
(2) ASVS V9 recommends key rotation as part of secure token key management.
    The absence of any rotation plan is a security hygiene gap.
(3) "No reason to rotate" is not equivalent to "not compromised" — key
    compromise may not be detectable immediately.
(4) Key rotation provides defense-in-depth: even if a key is compromised,
    rotation limits the damage window.

Finding level: CONCERN — no MUST violation; gap is security hygiene and
key lifecycle management. The absence of rotation is not an immediate
compliance failure but is a risk-increasing practice.

Recommendation: establish a key rotation schedule (e.g., annually or on
security events); implement graceful rotation with kid-based key versioning
(already present via kid claim).
Overall verdict: CONCERN
```

**Reason:** No RFC 7517 MUST requires a specific rotation frequency. However, never rotating a signing key since deployment creates an ever-growing exposure window and represents poor key lifecycle hygiene. CONCERN because there is no hard requirement violation, but the absence of any rotation is a meaningful security gap. The `kid` infrastructure is already present, making rotation straightforward.

**Phrasing-B:** RS256 signing key generated at deployment, never rotated. Public key at /.well-known/jwks.json. kid in all JWTs. No known compromise. RFC 7517 doesn't mandate rotation frequency. Is never rotating the signing key a CONCERN, or is there no requirement to rotate?

---

## Pair[CB4-3.4-394]

**A:** Comprehensive review with two new patterns — introspection endpoint and key rotation. Application implements: all prior controls (exact URIs; PKCE S256 CSPRNG; state; token revocation; back-channel SSO logout; Argon2id; /me/permissions; RS256 allowlist; Vault; Security Champion + OWASP checklist; bounded privilege grants; aud claim validation; audit log for privilege grants; no token logging). Additionally: introspection endpoint requires client authentication (client_id + client_secret); signing key is rotated quarterly with kid-based versioning at JWKS endpoint.

**B:**
```
Arbiter: Multiple — all territories

All prior controls: PASS (verified per prior assessments)

Introspection endpoint (RFC 7662 §2.1):
  Caller authentication: client_id + client_secret required ✓
  RFC 7662 §2.1 MUST satisfied: authorization to access endpoint is required ✓
  T-rfc-oauth2: PASS

JWT signing key rotation:
  Quarterly rotation schedule ✓
  kid-based versioning at JWKS endpoint ✓ — validators can retrieve the
    correct key for any token by looking up the kid
  ASVS V9.2 key management: PASS — rotation lifecycle managed; key versioning
    infrastructure present
  T-asvs-token: PASS (already PASS for allowlist + aud; rotation reinforces)

All territories: PASS
Overall verdict: PASS
```

**Reason:** All prior controls plus two new patterns correctly implemented: introspection endpoint with client authentication (satisfies RFC 7662 §2.1 MUST); quarterly signing key rotation with kid versioning (satisfies ASVS V9 key management best practice). Comprehensive PASS.

**Phrasing-B:** All prior controls correct. Introspection endpoint requires client_id + client_secret authentication. RS256 signing key rotated quarterly with kid versioning at JWKS. Does this comprehensive implementation including the introspection auth and key rotation yield an all-PASS?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-387 | V10.4.6 PKCE for confidential clients — CONCERN (client_secret mitigates; not explicit exemption) | B/D | ASVS-V10-OAuth | CONCERN |
| CB4-3.4-388 | V10.3 logout revocation PASS + no refresh token rotation — CONCERN (rotation = defense-in-depth) | B | ASVS-V10-OAuth + V7-Session | CONCERN |
| CB4-3.4-389 | V6.3 optional MFA — CONCERN (Level 1 may PASS; Level 2 requires enforcement) | A/B | ASVS-V6-Auth | CONCERN |
| CB4-3.4-390 | V10.2.1 exact URIs + security design review approval — PASS (exact registration is the evidence) | B/D | ASVS-V10-OAuth | PASS |
| CB4-3.4-391 | RFC 7519 nbf = iat - 5 clock skew — PASS (5-second backward offset accepted) | A/D | RFC-JWT + ASVS-V9-Token | PASS |
| CB4-3.4-392 | RFC 7662 §2.1 open introspection endpoint — REPROCESS (MUST auth requirement violated) | A | RFC-OAuth2 + ASVS-V10-OAuth | REPROCESS |
| CB4-3.4-393 | JWT signing key never rotated — CONCERN (no MUST violation; key lifecycle hygiene gap) | B/D | ASVS-V9-Token + RFC-JWT | CONCERN |
| CB4-3.4-394 | All prior + introspection auth + quarterly key rotation — comprehensive PASS | B | Multiple | PASS |
