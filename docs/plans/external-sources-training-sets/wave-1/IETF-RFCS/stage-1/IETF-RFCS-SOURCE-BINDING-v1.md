# IETF-RFCS — Source Binding
# Stage 1, Document 1 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source: RFC 6749 (OAuth 2.0), RFC 7519 (JWT), RFC 7636 (PKCE), RFC 8446 (TLS 1.3)
# License: IETF Trust Legal Provisions — IETF_TRUST_PROVISIONS
# Attribution required: RFC number + "IETF Trust"
# Produced: 2026-05-05

---

## What this document is

IETF-RFCS provides the protocol-level authority for authorization decisions
in batch CB4. While OWASP-ASVS references RFC requirements (e.g., ASVS 10.4.6
references RFC 7636), this source tests whether the orchestrator can apply
RFC-level protocol conformance checks directly, without requiring the ASVS
intermediary. It also tests RFC-to-ASVS cross-reference: when an ASVS
requirement cites an RFC, the orchestrator must verify compliance at the RFC
level, not just the ASVS label level.

---

## RFC binding per scenario

### Scenario S1 — Complete OAuth 2.0 + JWT flow (positive)

The input describes a full OAuth 2.0 authorization code flow with all protocol
conformance requirements met.

| RFC | Section | Requirement | Input evidence |
|-----|---------|-------------|----------------|
| RFC 6749 | §4.1 | Authorization code flow, not implicit | Uses authorization code flow |
| RFC 6749 | §4.1.2 | Authorization codes expire; 10-minute max recommended | Authorization codes expire in 5 minutes |
| RFC 6749 | §10.12 | CSRF prevention via state parameter | State parameter present and validated |
| RFC 7636 | §4.2 | Client MUST use S256 when capable | S256 used, server supports it |
| RFC 7636 | §4.3 | Server verifies code_verifier | Verification confirmed |
| RFC 7519 | §4.1.4 | exp claim: token invalid after expiry | exp validated, tokens expire in 1 hour |
| RFC 7519 | §4.1.3 | aud claim: token intended for this service | aud claim validated against service |
| RFC 7519 | §7.2 | JWT validation algorithm verified | RS256 signature validated |

---

### Scenario S2 — JWT HS256 shared key (worst negative)

**S2 binding:** A JWT access token signed with HMAC-SHA256 (HS256) where the
same secret key is shared between the authorization server (which issues tokens)
and multiple resource servers (which validate tokens). RFC 7519 Section 8
(Security Considerations) describes the trust implication: any party that can
validate a JWT can also forge one if they hold the signing key.

| RFC | Section | Requirement | Conflict in S2 |
|-----|---------|-------------|----------------|
| RFC 7519 | §8 (Security) | Private keys must be known only to issuer | HS256 shared key: any resource server can forge tokens for any other resource server |
| RFC 7517 | §4.4 | Algorithm restrictions | HS256 with shared key creates implicit trust between services |

**Why worst negative:** The input describes a configuration that is syntactically
valid (HS256 is a valid JWT algorithm; shared keys work technically) but creates
a trust boundary violation. Any resource server that holds the shared key can
issue tokens impersonating the authorization server. In a microservices architecture
with multiple resource servers, this means a single compromised resource server
can forge tokens for all other resource servers.

The orchestrator must identify this as a REPROCESS because the configuration
violates RFC 7519 §8 trust model requirements for multi-service deployments.

---

### Scenario S3 — JWT exp validation with clock skew (borderline)

**S3 binding:** A resource server validates JWT exp claims but uses a 10-minute
clock skew tolerance to account for network delays. RFC 7519 §4.1.4 says
processors "MAY provide for some small leeway, usually no more than a few
minutes, to account for clock skew." 10 minutes exceeds "a few minutes."

| RFC | Section | Requirement | Question |
|-----|---------|-------------|---------|
| RFC 7519 | §4.1.4 | "A few minutes" leeway | Is 10 minutes "a few minutes"? |

**Why borderline:** RFC 7519 does not define "a few minutes" numerically. Industry
convention is 60–300 seconds (1–5 minutes). 10 minutes (600 seconds) is at the
upper edge of what can be argued as "a few minutes." The practical impact: a
token that expired 9 minutes ago would still be accepted. This extends the
window during which a stolen JWT can be used after expiry.

The correct verdict is CONCERN — the implementation technically follows the
RFC's non-binding guidance ("MAY provide for some leeway"), but the leeway
value exceeds the convention and creates a meaningful security gap. REPROCESS
would be wrong because the RFC does not specify a numeric maximum.

---

## Arbiter role names

| Arbiter role | RFCs enforced | Triggered by |
|-------------|--------------|-------------|
| `RFC-OAuth2-Arbiter` | RFC 6749, RFC 7636 | G-auth-model + G-token-security |
| `RFC-JWT-Arbiter` | RFC 7519 | G-token-security |

---

## Acceptance criteria

**AC-1:** For S1, Node 8 must emit PASS. Must name at least three RFC references
(e.g., RFC 7636 §4.2, RFC 7519 §4.1.4) in the verdict. "RFC requirements met"
without specific section references fails.

**AC-2:** For S2, Node 8 must emit REPROCESS. Must identify HS256 shared key
as the trust boundary violation. Must name RFC 7519 §8. PASS or CONCERN fails.

**AC-3:** For S3, Node 8 must emit CONCERN. Must acknowledge that 10 minutes
exceeds typical leeway convention while acknowledging the RFC does not specify
a numeric maximum. REPROCESS fails — the RFC is non-prescriptive on the value.
PASS fails — the 10-minute gap is a real security concern.

**AC-4:** For all scenarios, Node 7 must select RFC-JWT-Arbiter when the input
describes JWT token handling. Absence of RFC-JWT-Arbiter when JWT is the topic
is FM-4 (arbiter selection gap).
