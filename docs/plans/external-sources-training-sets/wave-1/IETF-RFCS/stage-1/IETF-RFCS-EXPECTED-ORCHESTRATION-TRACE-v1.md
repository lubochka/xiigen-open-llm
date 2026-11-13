# IETF-RFCS — Expected Orchestration Trace
# Stage 1, Document 3 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source: RFC 6749, RFC 7519, RFC 7636 — IETF_TRUST_PROVISIONS
# Produced: 2026-05-05

---

## Scenario S1 — Complete OAuth 2.0 + JWT flow (positive)

### S1 Input (to Node 1)

"Our authentication service implements OAuth 2.0 with the following specifics:
- Grant type: authorization code flow only. Implicit and password grants
  are disabled at the authorization server configuration level.
- PKCE: S256 method. The authorization server rejects plain method.
  code_verifier is generated with 256-bit entropy per RFC 7636 §4.1.
- State parameter: generated per-transaction, validated on redirect.
- JWT access tokens: signed with RS256. Expiry: 1 hour. All tokens validated
  for signature, exp, nbf, and aud claims. aud claim validated against the
  specific service identifier for each resource server.
- Authorization codes: expire in 5 minutes, single-use only.
Please verify RFC 6749, RFC 7636, and RFC 7519 conformance."

### S1 Expected Node 8 output

PASS. RFC-OAuth2-Arbiter and RFC-JWT-Arbiter results:

RFC 6749 conformance:
- §4.1 Authorization code flow used — not implicit, not password grant. Compliant.
- §4.1.2 Authorization codes expire in 5 minutes — within 10-minute RFC recommendation. Compliant.
- §10.12 State parameter present and validated — CSRF prevention satisfied. Compliant.

RFC 7636 conformance:
- §4.2 S256 used; plain method rejected by server — MUST requirement satisfied. Compliant.
- §4.1 256-bit code_verifier entropy — minimum entropy satisfied. Compliant.

RFC 7519 conformance:
- §7.2 RS256 signature validation — algorithm verified. Compliant.
- §4.1.4 exp claim validated, 1-hour expiry. Compliant.
- §4.1.3 aud claim validated against per-service identifier. Compliant.

All three RFCs: conformance confirmed.

### S1 Expected Node 9 output

VERDICT: PASS

RFC protocol conformance verified:
- RFC 6749 §4.1: Authorization code flow ✓
- RFC 6749 §10.12: State parameter, CSRF prevention ✓
- RFC 7636 §4.2: PKCE S256 method, plain rejected ✓
- RFC 7519 §7.2: RS256 signature validation ✓
- RFC 7519 §4.1.3/4.1.4: aud and exp validation ✓

Implementation conforms to RFC 6749, RFC 7636, and RFC 7519 requirements for
the described configuration.

---

## Scenario S2 — JWT HS256 shared key (worst negative)

### S2 Input (to Node 1)

"Our microservices platform uses JWT access tokens signed with HMAC-SHA256
(HS256). The signing key is a 256-bit symmetric key stored in a secrets manager.
Each of our six resource servers has access to this signing key for JWT
validation. The authorization server uses the same key to sign tokens. This
was implemented to keep the token validation fast and simple — no JWKS endpoint
needed. Is there a security issue with this JWT configuration?"

### S2 Expected Node 8 output

REPROCESS. RFC-JWT-Arbiter result:

RFC 7519 §8 (Security Considerations) — trust boundary violation confirmed.

The HS256 shared key configuration creates a trust boundary violation in a
multi-service deployment. Explanation:

With HS256, the same key is used to sign tokens (authorization server) and to
validate tokens (resource servers). RFC 7519 §8 notes that "the keys used to
sign and validate JWTs must be treated as sensitive." More critically: a party
that holds the signing key can create valid JWTs indistinguishable from those
issued by the authorization server.

Impact in this deployment: any of the six resource servers that hold the shared
key can forge JWT access tokens that will be accepted by all other resource
servers. A single compromised resource server compromises the entire token trust
chain. This is not merely a "weaker algorithm" concern — it is a structural
trust boundary that HS256 symmetric key sharing eliminates across the service mesh.

RFC 7519 §8 requirement: key material must be protected. In a shared-key model
across six services, "protected" is diluted — the key's blast radius is
proportional to the number of services that hold it.

Recommended remediation: Migrate to RS256 (or ES256). The authorization server
holds the private signing key; resource servers access the public key via JWKS
endpoint. No resource server can forge tokens because they hold only the
verification key, not the signing key. This re-establishes the trust boundary.

Error code: RFC_7519_TRUST_BOUNDARY_VIOLATION.

### S2 Expected Node 9 output

VERDICT: REPROCESS — RFC 7519 TRUST BOUNDARY VIOLATION

Security configuration issue: HS256 shared signing key across multiple resource servers.

Risk: Any of the 6 resource servers that hold the shared HS256 key can forge
JWT tokens valid for all other resource servers. A single compromised service
breaks the entire authorization trust chain.

RFC 7519 §8 basis: JWT signing keys must be treated as sensitive material.
In a shared-key multi-service model, the effective protection of that key is
bounded by the least-secure service holding it.

Required remediation:
1. Migrate JWT signing to an asymmetric algorithm (RS256 or ES256).
2. Authorization server holds the private signing key only.
3. Resource servers access the public key via JWKS endpoint (per RFC 7517).
4. No resource server holds signing capability — only verification capability.

This migration eliminates the cross-service token forgery risk while preserving
validation performance (JWKS public keys can be cached). This item is returned
to the development queue.

---

## Scenario S3 — JWT exp clock skew (borderline)

### S3 Input (to Node 1)

"Our resource server validates JWT exp claims for all incoming access tokens.
Due to network delays between our authorization server and resource servers,
we configured a 10-minute clock skew tolerance in the JWT validation library.
Tokens that expire within 10 minutes in the past are still accepted. RFC 7519
§4.1.4 says implementers MAY provide for some small leeway to account for
clock skew. Is our 10-minute leeway within RFC guidelines?"

### S3 Expected Node 8 output

CONCERN. RFC-JWT-Arbiter result:

RFC 7519 §4.1.4 — clock skew assessment:

RFC 7519 §4.1.4 states: "Implementers MAY provide for some small leeway,
usually no more than a few minutes, to account for clock skew."

Assessment: 10 minutes (600 seconds) exceeds the "a few minutes" guidance.
Industry convention for clock skew leeway is 60–300 seconds (1–5 minutes),
established by widespread JWT library defaults (e.g., 5-minute leeway in
major libraries). 10 minutes is at the upper edge of what can be defended as
"a few minutes."

RFC verdict: The RFC does not specify a numeric maximum for clock skew — it
uses the non-normative phrase "usually no more than a few minutes." The 10-minute
value is technically within the non-prescriptive RFC guidance. This is not a
MUST violation. REPROCESS is not warranted.

Security concern: 10 minutes extends the window during which a stolen JWT can
be used after expiry. An attacker who intercepts an access token has a 10-minute
post-expiry window to use it, in addition to the token's original lifetime.
For 1-hour access tokens (the most common configuration), this represents a 17%
extension of the effective token validity window.

Recommendation: Reduce clock skew tolerance to ≤ 5 minutes. If the 10-minute
tolerance was implemented to mask network delays, investigate the actual network
latency and address that root cause rather than compensating with a generous
skew window.

### S3 Expected Node 9 output

VERDICT: CONCERN — JWT CLOCK SKEW TOLERANCE EXCEEDS CONVENTION

RFC 7519 §4.1.4 compliance: TECHNICALLY WITHIN GUIDELINES (MAY clause —
no numeric maximum defined)

Security concern: 10-minute clock skew tolerance extends effective token validity
beyond the stated expiry. Industry convention for clock skew leeway is 1–5
minutes. 10 minutes is above this range.

Practical impact: a JWT that expired 9 minutes ago will still be accepted. For
1-hour access tokens, this creates a 70-minute effective validity window instead
of 60 minutes.

Recommended action: Reduce clock skew tolerance to ≤ 300 seconds (5 minutes).
Investigate the root cause of the reported network delays — genuine network
latency should be addressed at the infrastructure level, not compensated with
a permissive JWT validation policy.

This item does not require reprocessing — the RFC is non-prescriptive on the
clock skew value. The concern is forwarded to the team as an advisory security
recommendation.
