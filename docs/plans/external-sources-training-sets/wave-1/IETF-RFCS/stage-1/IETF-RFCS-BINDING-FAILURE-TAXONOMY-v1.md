# IETF-RFCS — Binding Failure Taxonomy
# Stage 1, Document 2 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source: RFC 6749, RFC 7519, RFC 7636 — IETF_TRUST_PROVISIONS
# Produced: 2026-05-05

---

## Five named failure modes

### FM-RFC-1: RFC_SURFACE_CLAIM_ACCEPTED

**Description:** The orchestrator accepts "we use OAuth 2.0" or "we use JWT"
as evidence of RFC compliance without checking protocol conformance details.
Using a technology name is not the same as conforming to the RFC that defines it.

**Detection:** Node 8 returns PASS for an OAuth 2.0 or JWT input without
checking which grant type is used, whether PKCE is in use, whether exp/aud
are validated, or whether the algorithm is appropriate for the deployment model.

**Classification:** Judgment gap in Node 8.

**Training correction:** Pairs where "we use OAuth 2.0" inputs are fed to Node 8
and Node 8 returns CONCERN requesting specific protocol conformance details
(grant type, PKCE, state parameter, scope).

---

### FM-RFC-2: RFC_SECTION_ABSENT_FROM_VERDICT

**Description:** Node 8 verdict references an RFC number without naming the
specific section (e.g., "RFC 7519 violation" without "RFC 7519 §8"). RFC
verdicts require section references because different sections have different
normative weight (MUST vs MAY vs SHOULD).

**Detection:** Search Node 8 output for "§" or "Section" or "Sec." following
an RFC number. If the RFC is named but no section is cited, this failure applies.

**Classification:** Structural gap in Node 8.

**Training correction:** Pairs where Node 8 output always includes RFC number
plus section number for each compliance claim.

---

### FM-RFC-3: MUST_MAY_CONFUSION

**Description:** The orchestrator treats RFC SHOULD or MAY clauses as MUST
requirements, or treats MUST clauses as SHOULD recommendations. This affects
the severity of the verdict.

**Critical instance (S3):** RFC 7519 §4.1.4 says processors MAY provide for
some small leeway. A 10-minute leeway is a concern (exceeds convention), not
a REPROCESS (the RFC does not mandate a specific value). Treating MAY as MUST
produces a REPROCESS for S3 when CONCERN is correct.

**Critical instance for PKCE:** RFC 7636 §4.2 says clients MUST use S256
when capable. Treating this as a SHOULD (optional preference) produces a
CONCERN for an S256 violation when REPROCESS is correct.

**Classification:** Judgment gap in Node 8.

**Training correction:** Pairs that show RFC normative keyword level alongside
the specific requirement, and Node 8 calibrating verdict severity accordingly:
- MUST violation → REPROCESS
- SHOULD violation → CONCERN
- MAY deviation → CONCERN only if the deviation creates a security risk;
  otherwise NOTE

---

### FM-RFC-4: TRUST_BOUNDARY_MISSED

**Description:** The orchestrator does not identify trust boundary violations
in cryptographic key sharing scenarios. Specifically: the HS256 shared key
pattern creates an implicit trust relationship between all parties holding the
key — any key holder can forge tokens. This is a structural trust boundary
violation, not just a "weaker algorithm" concern.

**Detection:** Node 8 returns CONCERN (instead of REPROCESS) for S2, framing
HS256 as "less secure than RS256" without identifying the cross-service token
forgery capability.

**Classification:** Judgment gap in Node 8.

**Training correction:** Pairs that explicitly show the trust boundary
implication: in a multi-service deployment where multiple resource servers
hold the same HS256 signing key, any compromised resource server can forge
tokens accepted by all other resource servers. This is the specific threat,
not just "HS256 is symmetric."

---

### FM-RFC-5: PROTOCOL_VERSION_UNSPECIFIED

**Description:** The orchestrator accepts "OAuth 2.0" without noting that
RFC 9700 (OAuth 2.0 Security Best Current Practice, 2024) deprecated the
implicit grant type and the resource owner password credentials grant type.
ASVS 10.4.4 reflects this by prohibiting both flows.

**Detection:** Node 8 returns PASS for an input describing OAuth 2.0 without
checking that neither the implicit grant nor the password grant type is in use.

**Classification:** Judgment gap in Node 8.

**Training correction:** Pairs where Node 8 checks grant type presence as
a baseline OAuth 2.0 security requirement for all OAuth inputs.

---

## Severity ordering

1. FM-RFC-4 first (trust boundary missed = structural vulnerability undetected)
2. FM-RFC-3 second (MUST/MAY confusion = wrong verdict severity)
3. FM-RFC-1 third (surface claim accepted = entire protocol check skipped)
4. FM-RFC-2 fourth (section absent = traceability lost)
5. FM-RFC-5 last (protocol version gap = important but less immediate)
