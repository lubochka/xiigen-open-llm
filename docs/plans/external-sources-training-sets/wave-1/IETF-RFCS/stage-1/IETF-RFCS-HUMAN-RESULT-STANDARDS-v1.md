# IETF-RFCS — Human Result Standards
# Stage 1, Document 4 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source: RFC 6749, RFC 7519, RFC 7636 — IETF_TRUST_PROVISIONS
# Produced: 2026-05-05

---

## Scenario S1 — Complete OAuth 2.0 + JWT flow

**S1-M01** (mandatory): Node 8 verdict must be PASS.

**S1-M02** (mandatory): Node 8 must name at least three RFC section references
in format RFC NNNN §X.X (e.g., RFC 7636 §4.2). RFC number alone without section
fails. Failure mode: FM-RFC-2.

**S1-M03** (mandatory): Node 7 must have selected both RFC-OAuth2-Arbiter and
RFC-JWT-Arbiter (or equivalents covering OAuth 2.0 and JWT).
Failure mode: FM-4 (arbiter selection gap).

**S1 passes if:** All three pass.

---

## Scenario S2 — HS256 shared key trust boundary

**S2-M01** (mandatory): Node 8 final verdict must be REPROCESS. PASS or CONCERN
fails. Failure mode for CONCERN: FM-RFC-4 (trust boundary missed — framed as
"weaker algorithm" rather than cross-service forgery capability).

**S2-M02** (mandatory): Node 8 must name RFC 7519 §8 (Security Considerations)
explicitly. Naming RFC 7519 without the section fails.
Failure mode: FM-RFC-2.

**S2-M03** (mandatory): Node 8 must identify the specific threat: any resource
server holding the shared key can forge tokens accepted by all other resource
servers. "HS256 is less secure than RS256" is not this statement. The output
must name the cross-service forgery capability.
Failure mode: FM-RFC-4 (trust boundary missed).

**S2-M04** (mandatory): Node 8 must include the recommended remediation path:
migrate to RS256/ES256 with JWKS endpoint for public key distribution.
Failure mode if absent: structural gap.

**S2-M05** (mandatory): Node 9 output must contain REPROCESS, not CONCERN.
FM-RFC-3 if CONCERN — MUST clause severity downgraded.

**S2 passes if:** All five pass.

---

## Scenario S3 — JWT exp clock skew

**S3-M01** (mandatory): Node 8 final verdict must be CONCERN.
REPROCESS fails — FM-RFC-3 (MAY clause treated as MUST).
PASS fails — security concern not identified.

**S3-M02** (mandatory): Node 8 must cite RFC 7519 §4.1.4 and state that it
uses the non-normative "usually no more than a few minutes" language without
a numeric maximum. This citation is what distinguishes CONCERN from REPROCESS —
the RFC is non-prescriptive, so the violation is of convention, not requirement.
Failure mode if absent: structural gap.

**S3-M03** (mandatory): Node 8 must state the practical security impact:
the 10-minute leeway extends effective token validity beyond the stated expiry.
"Leeway too high" without the impact statement fails.
Failure mode if absent: FM-RFC-5 (incomplete).

**S3-M04** (mandatory): Node 9 output must not contain REPROCESS. CONCERN
with the advisory recommendation must be propagated.

**S3 passes if:** All four pass.

---

## Aggregate scoring

| Scenario | Mandatory checks | Passes if |
|----------|-----------------|-----------|
| S1 | M01–M03 | All 3 pass |
| S2 | M01–M05 | All 5 pass |
| S3 | M01–M04 | All 4 pass |

Most critical checks: S2-M03 (cross-service forgery capability named) and
S3-M01 (CONCERN, not REPROCESS — MAY clause distinction). Together these
train the RFC MUST/MAY severity calibration, which is the primary training
contribution of IETF-RFCS to batch CB4.
