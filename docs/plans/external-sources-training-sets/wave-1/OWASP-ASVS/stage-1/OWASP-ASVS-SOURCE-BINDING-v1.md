# OWASP-ASVS — Source Binding
# Stage 1, Document 1 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source version: OWASP ASVS 5.0.0 (May 2025)
# License: CC BY-SA 4.0 — Attribution required: "OWASP ASVS Project, Version 5.0.0"
# Produced: 2026-05-05

---

## What this document is

This document binds the three OWASP-ASVS experiment scenarios (S1, S2, S3) to the
specific ASVS chapters, sections, and requirement IDs they exercise. Every oracle
claim in documents 3 and 4 traces back to a binding entry in this document.

---

## Source summary

OWASP ASVS 5.0.0 defines 17 verification chapters (V1–V17). The following four
chapters govern sub-phase 3.4 (Security and Authorization Model):

| Chapter | Title | Scenario relevance |
|---------|-------|--------------------|
| V6 | Authentication | S1 (password, OTP controls), S2 (identity provider integration) |
| V7 | Session Management | S1 (timeout, re-authentication), S3 (borderline timeout) |
| V8 | Authorization | S1 (function-level, data-specific access) |
| V9 | Self-Contained Tokens | S1 (JWT validation) |
| V10 | OAuth and OIDC | S2 (PKCE, authorization code flow, public clients) |

Chapter V11 (Cryptography), V12 (Secure Communication), and V13 (Configuration)
are out of scope for these three scenarios. They apply to sub-phase 3.4 but are
addressed in separate experiment batches.

---

## Requirement ID binding per scenario

### Scenario S1 — JWT authentication chain, all controls satisfied (positive case)

| ASVS ID | Section | Requirement text (condensed) | Role in S1 |
|---------|---------|-------------------------------|------------|
| V6.2.1 | Password strength | Password minimum 8 chars; 15 strongly recommended | S1 claims 15-char minimum — exceeds minimum, satisfies L1 |
| V6.5.5 | OTP lifetime | OTP max 10 min; TOTP max 30 sec | S1 claims OTP expiry 10 min — at maximum boundary, satisfies L2 |
| V7.3.1 | Session inactivity timeout | Enforce re-authentication per documented risk analysis | S1 provides documented 20-min timeout with risk analysis on file — satisfies L2 |
| V7.5.1 | Re-auth for sensitive changes | Require re-authentication before modification of authentication credentials | S1 states re-auth enforced before credential changes — satisfies L2 |
| V8.2.1 | Function-level access | Ensure function-level access restricted to consumers with explicit permissions | S1 states RBAC enforced on all endpoints — satisfies L1 |
| V8.2.2 | Data-specific access (IDOR/BOLA) | Ensure data-specific access restricted to consumers with explicit permissions | S1 states per-resource ownership checks enforced — satisfies L1 |
| V9.1.1 | Token signature validation | Validate self-contained tokens using digital signature or MAC | S1 uses RS256 — approved algorithm, satisfies L1 |
| V9.1.2 | None algorithm prohibition | Approved algorithms only; 'None' must not be in allowlist | S1 states 'none' algorithm rejected — satisfies L1 |
| V9.2.1 | Token validity time span | Accept tokens only within nbf–exp validity window | S1 validates both exp and nbf claims — satisfies L1 |
| V9.2.3 | Audience restriction | Service only accepts tokens intended for that service via 'aud' claim | S1 validates 'aud' claim against service identifier — satisfies L2 |

**S1 binding rationale:**
All ten requirements above are directly stated as satisfied in the S1 input.
The arbiter must verify each ASVS ID individually — not by categorical label
match but by checking that the specific requirement's condition is met. A
coverage illusion failure (anti-pattern) occurs when the arbiter assigns PASS
because the domain labels match without checking each of these ten IDs.

---

### Scenario S2 — OAuth 2.0 public client using plain PKCE method (worst negative)

| ASVS ID | Section | Requirement text (condensed) | Role in S2 |
|---------|---------|-------------------------------|------------|
| V10.4.6 | PKCE requirement | Mitigate authorization code interception by requiring PKCE; reject 'plain' method; validate code_verifier | S2 uses plain method — directly violates the "reject plain method" clause |
| V10.4.4 | Prohibited grant types | Implicit and Password flows prohibited | S2 uses authorization code flow — this requirement is satisfied, not violated |
| V10.2.1 | CSRF protection for authorization code flow | Require PKCE or state parameter validation | S2 uses PKCE (with plain method) — partially satisfies this requirement; plain method creates downgrade risk |
| V10.1.2 | Client-generated secret quality | PKCE code_verifier must be non-guessable and transaction-specific | S2 does not specify code_verifier entropy — potential V10.1.2 gap, secondary finding |

**RFC binding for S2:**
| RFC | Section | Binding |
|-----|---------|---------|
| RFC 7636 | Section 4.2 | "Clients MUST use S256" when capable; plain permitted only when S256 not supported |
| RFC 7636 | Section 4.1 | code_verifier minimum 256 bits entropy |
| RFC 7636 | Section 7.2 | S256 prevents plain method downgrade attacks |
| RFC 6749 | Section 10.12 | CSRF prevention via state parameter |

**S2 binding rationale:**
ASVS 10.4.6 explicitly requires the authorization server to reject the 'plain'
code_challenge_method. The S2 input claims ASVS 10.4.6 compliance while using
'plain'. This is not a borderline case — the requirement text contains the word
"rejects" for the plain method. The correct Node 8 verdict is REPROCESS with
error code `ASVS_10_4_6_PLAIN_METHOD_REJECTED`. A PASS verdict here is a
binding failure of type CLAIM_BOUNDARY_FAILURE.

---

### Scenario S3 — Session timeout satisfies ASVS but deviates from internal standard (borderline)

| ASVS ID | Section | Requirement text (condensed) | Role in S3 |
|---------|---------|-------------------------------|------------|
| V7.3.1 | Inactivity timeout | Enforce re-authentication per documented risk analysis | S3 uses 20-min timeout with documented risk analysis — satisfies L2 |
| V7.3.2 | Absolute maximum lifetime | Enforce absolute session lifetime per documented risk analysis | S3 does not specify absolute maximum — potential gap, acknowledged in borderline verdict |
| V7.1.1 | Timeout documentation | Document inactivity timeout with justification for deviations from NIST standards | S3 has documentation but does not address NIST alignment — secondary concern |

**Organizational standard (non-ASVS) context for S3:**
The organization mandates exactly 30-minute session timeouts to align with the
enterprise SSO provider's token lifetime. The implementation uses 20 minutes.
This is an internal standard deviation, not an ASVS violation — a more
restrictive timeout is always ASVS-compliant.

**S3 binding rationale:**
ASVS 7.3.1 requires re-authentication to be enforced per documented risk
analysis. It does not specify a minimum or maximum value for the timeout —
it requires that the value chosen be documented and justified. A 20-minute
timeout enforces re-authentication. The deviation from the internal 30-minute
standard is an organizational governance concern, not an ASVS compliance
failure. The correct Node 8 verdict is CONCERN (not REPROCESS). A REPROCESS
verdict here is a binding failure of type CLAIM_BOUNDARY_FAILURE — escalating
a concern to a block because the orchestrator cannot distinguish ASVS
compliance from internal-standard conformance.

---

## Domain label to ASVS chapter mapping

| Domain label | ASVS chapters | Notes |
|-------------|---------------|-------|
| `G-security-verification` | V6, V7, V8, V9, V10 | General verification; present in all three scenarios |
| `G-auth-model` | V6, V9, V10 | Authentication and token security |
| `G-access-control` | V8 | Function-level and data-specific access |
| `G-session-management` | V7 | Session lifetime and termination |
| `G-token-security` | V9, V10 | JWT and OAuth token validation |
| `B-password-policy` | V6.2 | Password strength and composition |
| `B-otp-validation` | V6.5, V6.6 | OTP and MFA factor validation |
| `B-threat-modeling` | V8.1.3, V8.1.4, V10.2 | Contextual and environmental threat modeling |

---

## Arbiter role names for this experiment

| Arbiter role name | ASVS chapters it enforces | Triggered by scenario |
|-------------------|---------------------------|----------------------|
| `ASVS-V6-Auth-Arbiter` | V6.2, V6.5, V6.6, V6.8 | S1 |
| `ASVS-V7-Session-Arbiter` | V7.3, V7.5 | S1, S3 |
| `ASVS-V8-AuthZ-Arbiter` | V8.2, V8.3 | S1 |
| `ASVS-V9-Token-Arbiter` | V9.1, V9.2 | S1 |
| `ASVS-V10-OAuth-Arbiter` | V10.2, V10.4 | S2 |

---

## Acceptance criteria (Stage 1, Step 6)

These criteria are measurable without manual interpretation.

**AC-1:** For S1, Node 8 must emit a PASS verdict. The result must name at least
eight of the ten bound ASVS requirement IDs (V6.2.1, V6.5.5, V7.3.1, V7.5.1,
V8.2.1, V8.2.2, V9.1.1, V9.1.2, V9.2.1, V9.2.3) explicitly. Naming only the
chapter (e.g., "V6 satisfied") without the section and item number does not
count — that is a coverage illusion.

**AC-2:** For S2, Node 8 must emit a REPROCESS verdict. The verdict must name
ASVS 10.4.6 and the specific violation: plain method used where plain method
is rejected. A PASS or CONCERN verdict for S2 is a test failure regardless
of what other output is produced.

**AC-3:** For S3, Node 8 must emit a CONCERN verdict. The concern must distinguish
between ASVS compliance (confirmed) and internal standard deviation (flagged).
A REPROCESS verdict for S3 is a test failure — it indicates the orchestrator
cannot make the ASVS/internal-standard distinction.

**AC-4:** For all three scenarios, Node 9 must produce a result that names the
primary ASVS requirement IDs relevant to its verdict. "Security requirements
satisfied" without ASVS IDs is not a passing formatter output.

**AC-5:** For all three scenarios, the domain labels produced by Node 2 must
include `G-security-verification`. A result that omits this label has
misclassified the domain.
