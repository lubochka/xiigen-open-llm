# IETF-RFCS — Executor Contract
# Stage 1, Document 5 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source: RFC 6749, RFC 7519, RFC 7636 — IETF_TRUST_PROVISIONS
# Produced: 2026-05-05

---

## Section 1 — Pre-run checklist

Same as OWASP-ASVS Executor Contract Section 1. Baseline ≥ 1153 required.

---

## Section 3 — Scenario inputs (verbatim)

### S1 Input

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

### S2 Input

"Our microservices platform uses JWT access tokens signed with HMAC-SHA256
(HS256). The signing key is a 256-bit symmetric key stored in a secrets manager.
Each of our six resource servers has access to this signing key for JWT
validation. The authorization server uses the same key to sign tokens. This
was implemented to keep the token validation fast and simple — no JWKS endpoint
needed. Is there a security issue with this JWT configuration?"

### S3 Input

"Our resource server validates JWT exp claims for all incoming access tokens.
Due to network delays between our authorization server and resource servers,
we configured a 10-minute clock skew tolerance in the JWT validation library.
Tokens that expire within 10 minutes in the past are still accepted. RFC 7519
§4.1.4 says implementers MAY provide for some small leeway to account for
clock skew. Is our 10-minute leeway within RFC guidelines?"

---

## Section 4 — What the executor must not do

**Must not conflate "weaker algorithm" with "trust boundary violation."**
If the pipeline returns CONCERN for S2 on the basis that HS256 is less secure
than RS256, record this as FM-RFC-4. The specific threat is cross-service token
forgery — not just algorithm strength.

**Must not over-prescribe the RFC MAY clause.** If the pipeline returns REPROCESS
for S3, record this as FM-RFC-3. RFC 7519 §4.1.4 is non-normative on the
leeway value — the concern is conventional, not MUST-level.

---

## Section 5 — Evidence file location

`wave-1/IETF-RFCS/stage-2/IETF-RFCS-EXPERIMENT-EVIDENCE-v1.md`

---

## Section 6 — Source authority

| RFC | Title | Key sections for this experiment |
|-----|-------|----------------------------------|
| RFC 6749 | The OAuth 2.0 Authorization Framework | §4.1, §4.1.2, §10.12 |
| RFC 7519 | JSON Web Token (JWT) | §4.1.3, §4.1.4, §7.2, §8 |
| RFC 7636 | PKCE for OAuth Public Clients | §4.1, §4.2, §7.2 |

License: IETF Trust Legal Provisions — reproduction permitted with attribution.
Attribution text: "RFC NNNN, [author(s)], IETF Trust"

---

## Section 7 — Pair production for Stage 3

**Pair ID format:** CB4-3.4-[SEQ] (continuing from CWE-TOP25 pairs)
**Primary training goal:** Node 8 RFC-JWT-Arbiter ability to distinguish MUST
violations (REPROCESS) from MAY deviations (CONCERN) using RFC normative
keyword calibration, and to identify trust boundary violations in symmetric
key sharing.

**Node coverage priorities:**
1. Node 8 — RFC-JWT-Arbiter: S2 REPROCESS with RFC 7519 §8 trust boundary named
2. Node 8 — RFC-JWT-Arbiter: S3 CONCERN with RFC MAY clause distinction named
3. Node 8 — RFC-OAuth2-Arbiter: S1 PASS with RFC section references
4. Node 7 — arbiter selector: RFC-JWT-Arbiter selected when JWT vocabulary present

Node 8 pass case (S1) must precede CONCERN and REPROCESS cases per batch
CB4 Node 8 pass-case-first rule.

**Cross-source note for Stage 3 pair writing:**
IETF-RFCS pairs in batch CB4 should reference the corresponding ASVS
requirement where one exists:
- RFC 7636 §4.2 (PKCE S256 MUST) → ASVS 10.4.6
- RFC 7519 §7.2 (JWT validation) → ASVS 9.1.1
- RFC 6749 §4.1 (authorization code flow) → ASVS 10.4.4

This cross-reference trains the orchestrator to apply both RFC and ASVS
authority simultaneously, not as alternatives.
