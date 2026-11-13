# OWASP-ASVS — Expected Orchestration Trace
# Stage 1, Document 3 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source version: OWASP ASVS 5.0.0 (May 2025)
# Produced: 2026-05-05

---

## Purpose

This document shows the exact node-by-node trace that the orchestrator must
produce for each of the three scenarios. Every B output shown here is the
actual output — not a description of what the output should contain.

The executor compares actual traces against these expected traces verbatim.
Differences are classified using the Binding Failure Taxonomy.

---

## Scenario S1 — JWT authentication chain, all controls satisfied (positive case)

### S1 Input (to Node 1)

"Our backend authentication service has the following verified controls in
production:
- Password minimum length: 15 characters, enforced at write time. No maximum
  other than the 64-character floor required by ASVS 6.2.9.
- OTP expiry: 10 minutes for all out-of-band codes. TOTP window: 30 seconds.
- Session idle timeout: 20 minutes. Risk analysis document RA-AUTH-003 (filed
  2026-03-15) justifies this value relative to NIST SP 800-63B guidance.
- Re-authentication enforced before any change to username, email, password,
  or linked MFA factors.
- JWT validation: RS256 algorithm only. The 'none' algorithm is rejected by
  the validation middleware. Both exp and nbf claims are validated. The 'aud'
  claim is validated against the service identifier 'auth.internal.example.com'.
- RBAC enforced on all API endpoints. Per-resource ownership checks prevent
  cross-user data access.
Does this implementation satisfy OWASP ASVS 5.0.0 for sub-phase 3.4 (Security
and Authorization Model)?"

---

### S1 Node 1 — Domain Splitter output

Territory 1: Password strength and composition enforcement (ASVS V6.2)
Territory 2: OTP and MFA factor lifetime controls (ASVS V6.5)
Territory 3: Session lifetime and re-authentication gates (ASVS V7.3, V7.5)
Territory 4: Self-contained token validation (JWT) (ASVS V9.1, V9.2)
Territory 5: Function-level and data-specific access control (ASVS V8.2)

---

### S1 Node 2 — Domain Translator outputs (one per territory)

Territory 1 → ["G-security-verification", "G-auth-model", "B-password-policy"]
Territory 2 → ["G-security-verification", "G-auth-model", "B-otp-validation"]
Territory 3 → ["G-security-verification", "G-session-management"]
Territory 4 → ["G-security-verification", "G-token-security"]
Territory 5 → ["G-security-verification", "G-access-control"]

---

### S1 Node 3 — Scheme Planner output

Step 1 (parallel-safe with Steps 2, 3, 4, 5): Verify ASVS V6.2 password
controls. Check: minimum length reported is 15 characters — ASVS 6.2.1
requires 8 minimum (15 strongly recommended), satisfied. Check: 64-character
maximum floor acknowledged — ASVS 6.2.9 satisfied. Wait state: both checks
confirmed before Step 6 receives V6 result.

Step 2 (parallel-safe with Steps 1, 3, 4, 5): Verify ASVS V6.5 OTP controls.
Check: OTP expiry 10 minutes — ASVS 6.5.5 maximum is 10 minutes, boundary
exactly met, satisfied. Check: TOTP window 30 seconds — ASVS 6.5.5 maximum
is 30 seconds, boundary exactly met, satisfied. Recovery path: if OTP expiry
is stated without a unit, request clarification before recording result; do
not assume minutes. Wait state: both checks confirmed before Step 6 receives
V6.5 result.

Step 3 (parallel-safe with Steps 1, 2, 4, 5): Verify ASVS V7.3 and V7.5
session controls. Check: idle timeout 20 minutes — ASVS 7.3.1 requires
enforcement per documented risk analysis, RA-AUTH-003 cited. Risk analysis
document reference present, satisfied at L2. Check: re-authentication before
credential changes — ASVS 7.5.1 satisfied. Wait state: RA-AUTH-003 citation
confirmed before Step 6 receives V7 result.

Step 4 (parallel-safe with Steps 1, 2, 3, 5): Verify ASVS V9.1 and V9.2
JWT controls. Check: RS256 — approved algorithm, ASVS 9.1.2 allowlist
requirement satisfied. Check: 'none' algorithm rejected — ASVS 9.1.2 None
prohibition satisfied. Check: exp and nbf validated — ASVS 9.2.1 satisfied.
Check: 'aud' claim validated against 'auth.internal.example.com' — ASVS 9.2.3
satisfied. Wait state: all four checks confirmed before Step 6 receives V9
result.

Step 5 (parallel-safe with Steps 1, 2, 3, 4): Verify ASVS V8.2 access
controls. Check: RBAC on all endpoints — ASVS 8.2.1 function-level access
satisfied. Check: per-resource ownership checks — ASVS 8.2.2 data-specific
access (IDOR/BOLA prevention) satisfied. Wait state: both checks confirmed
before Step 6 receives V8 result.

Step 6 (depends on Steps 1–5): Collect all five domain results and pass to
Node 7 arbiter selector for final verdict.

---

### S1 Node 4 — Domain Processor outputs (one per territory)

Territory 1: ASVS 6.2.1 → implementation uses 15 chars, minimum is 8, strongly
recommended is 15. Implementation meets strongly recommended threshold. ASVS
6.2.9 → 64-char floor acknowledged. Status: COMPLIANT.

Territory 2: ASVS 6.5.5 → OTP 10 min (at maximum boundary), TOTP 30 sec (at
maximum boundary). Both values are at the exact maximum. Status: COMPLIANT AT
BOUNDARY.

Territory 3: ASVS 7.3.1 → 20-min idle timeout, RA-AUTH-003 filed 2026-03-15.
Documentation present and referenced by ID. ASVS 7.5.1 → re-auth before
credential changes confirmed. Status: COMPLIANT.

Territory 4: ASVS 9.1.1 → RS256 used (digital signature validation). ASVS
9.1.2 → 'none' algorithm rejected (approved algorithm allowlist enforced). ASVS
9.2.1 → exp and nbf validated (validity time span enforced). ASVS 9.2.3 → 'aud'
claim validated. Status: COMPLIANT.

Territory 5: ASVS 8.2.1 → RBAC on all endpoints (function-level access
restricted). ASVS 8.2.2 → per-resource ownership checks (data-specific access
restricted, IDOR/BOLA prevention present). Status: COMPLIANT.

---

### S1 Node 5 — Applier outputs (one per territory)

Territory 1: PASS — V6.2.1 satisfied (15 ≥ 8 minimum, 15 meets strongly
recommended). V6.2.9 satisfied (64-char floor acknowledged).

Territory 2: PASS — V6.5.5 satisfied (OTP 10 min = maximum, TOTP 30 sec =
maximum). Note: values at exact boundary; any increase above these values would
violate the requirement.

Territory 3: PASS — V7.3.1 satisfied (20-min timeout, RA-AUTH-003 documents
the risk analysis justification). V7.5.1 satisfied.

Territory 4: PASS — V9.1.1, V9.1.2, V9.2.1, V9.2.3 all satisfied.

Territory 5: PASS — V8.2.1 and V8.2.2 satisfied.

---

### S1 Node 6 — Result Translator outputs (one per territory)

Territory 1: "Password policy satisfies ASVS 6.2.1 (minimum 8 characters;
implementation uses 15, meeting the strongly recommended minimum). ASVS 6.2.9
satisfied (64-character maximum floor acknowledged)."

Territory 2: "OTP and TOTP lifetime controls satisfy ASVS 6.5.5 (OTP maximum
is 10 minutes; implementation uses exactly 10 minutes. TOTP maximum is 30
seconds; implementation uses exactly 30 seconds). Note: these values are at
the boundary — any increase would break compliance."

Territory 3: "Session management satisfies ASVS 7.3.1 (20-minute idle timeout
enforced; risk analysis RA-AUTH-003 on file documents the justification) and
ASVS 7.5.1 (re-authentication required before credential changes)."

Territory 4: "JWT token security satisfies ASVS 9.1.1 (RS256 signature
validation), 9.1.2 ('none' algorithm rejected from allowlist), 9.2.1 (exp
and nbf claims validated), and 9.2.3 (audience claim validated against
service identifier)."

Territory 5: "Access controls satisfy ASVS 8.2.1 (RBAC on all endpoints,
function-level access restricted) and ASVS 8.2.2 (per-resource ownership
checks prevent cross-user data access, IDOR and BOLA prevention present)."

---

### S1 Node 7 — Arbiter Selector output

Selected arbiters:
- ASVS-V6-Auth-Arbiter (triggers on G-auth-model, B-password-policy, B-otp-validation)
- ASVS-V7-Session-Arbiter (triggers on G-session-management)
- ASVS-V8-AuthZ-Arbiter (triggers on G-access-control)
- ASVS-V9-Token-Arbiter (triggers on G-token-security)

---

### S1 Node 8 — Arbiter Unit outputs

ASVS-V6-Auth-Arbiter: PASS. V6.2.1 confirmed: 15-char minimum exceeds 8-char
floor and meets strongly recommended threshold. V6.5.5 confirmed: OTP 10 min
and TOTP 30 sec are both at the maximum boundary; compliant. No concern raised.

ASVS-V7-Session-Arbiter: PASS. V7.3.1 confirmed: 20-min timeout with
RA-AUTH-003 on file satisfies the documentation requirement. V7.5.1 confirmed:
re-auth before credential changes present. No concern raised.

ASVS-V8-AuthZ-Arbiter: PASS. V8.2.1 confirmed: RBAC on all endpoints satisfies
function-level access restriction. V8.2.2 confirmed: ownership checks present,
IDOR/BOLA prevention satisfied. No concern raised.

ASVS-V9-Token-Arbiter: PASS. V9.1.1 (RS256), V9.1.2 (none rejected), V9.2.1
(exp/nbf), V9.2.3 (aud) all confirmed. No concern raised.

Combined S1 verdict: PASS. All four arbiters return PASS. No REPROCESS and no
CONCERN raised.

---

### S1 Node 9 — Formatter output

VERDICT: PASS

ASVS 5.0.0 requirements verified for sub-phase 3.4 (Security and Authorization
Model). All checked requirements satisfied:

- V6.2.1: Password minimum 15 chars (requirement: ≥ 8, strongly recommended ≥ 15) ✓
- V6.2.9: 64-char maximum floor acknowledged ✓
- V6.5.5: OTP expiry 10 min (requirement: ≤ 10 min), TOTP window 30 sec (requirement: ≤ 30 sec) ✓
- V7.3.1: Session idle timeout 20 min with risk analysis RA-AUTH-003 on file ✓
- V7.5.1: Re-authentication before credential changes ✓
- V8.2.1: RBAC on all endpoints, function-level access restricted ✓
- V8.2.2: Per-resource ownership checks, IDOR/BOLA prevention present ✓
- V9.1.1: RS256 signature validation ✓
- V9.1.2: 'none' algorithm rejected from allowlist ✓
- V9.2.1: exp and nbf claims validated ✓
- V9.2.3: 'aud' claim validated against 'auth.internal.example.com' ✓

Notes: OTP and TOTP values are at the maximum boundary allowed by V6.5.5. An
increase above these values would create a compliance gap. Recommend review
if lifetime values change.

---

---

## Scenario S2 — OAuth 2.0 public client using 'plain' PKCE method (worst negative)

### S2 Input (to Node 1)

"Our single-page application uses OAuth 2.0 authorization code flow. We
implemented PKCE to protect against authorization code interception. Due to
a compatibility issue with our authorization server (which was deployed before
RFC 7636 S256 support was added), we are using code_challenge_method=plain.
The authorization server accepts both S256 and plain. Our security review
team has marked this implementation as satisfying ASVS 10.4.6 compliance.
Please confirm whether this assessment is correct."

---

### S2 Node 1 — Domain Splitter output

Territory 1: OAuth 2.0 authorization code flow implementation (ASVS V10.2, V10.4)
Territory 2: PKCE compliance and code challenge method selection (ASVS V10.4.6)
Territory 3: Authorization server compatibility and protocol enforcement (ASVS V10.4.4)

---

### S2 Node 2 — Domain Translator outputs

Territory 1 → ["G-security-verification", "G-auth-model", "G-token-security"]
Territory 2 → ["G-security-verification", "G-auth-model", "B-threat-modeling"]
Territory 3 → ["G-security-verification", "G-access-control"]

---

### S2 Node 3 — Scheme Planner output

Step 1 (parallel-safe with Step 2): Verify the PKCE implementation against
ASVS 10.4.6. ASVS 10.4.6 requires: (1) PKCE present, (2) 'plain' method
rejected, (3) code_verifier validated. The input states plain method is used
and accepted. Check item (2): plain method must be rejected per ASVS 10.4.6.
Observable output: ASVS 10.4.6 compliance status for plain method usage.
Wait state: plain method check result recorded before Step 3 receives input.

Step 2 (parallel-safe with Step 1): Verify RFC 7636 compliance. RFC 7636
Section 4.2 states clients MUST use S256 when capable. The input states the
authorization server accepts S256 — therefore the client is capable of using
S256. Plain method usage when S256 is available violates RFC 7636 Section 4.2.
Observable output: RFC 7636 compliance status.

Step 3 (depends on Steps 1 and 2): Collect ASVS and RFC findings. If either
Step 1 or Step 2 finds a violation, the assessment that this implementation
satisfies ASVS 10.4.6 is incorrect. Pass to Node 7 for arbiter selection.

---

### S2 Node 4 — Domain Processor outputs

Territory 1: OAuth 2.0 authorization code flow — authorization server present,
grant type is code (not implicit or password, ASVS 10.4.4 satisfied). PKCE
attempted. Status: PARTIAL — PKCE present but method invalid.

Territory 2: PKCE method check — ASVS 10.4.6 requires the authorization server
to reject the 'plain' code_challenge_method. The input states the authorization
server accepts 'plain'. This means the authorization server is not conforming
to ASVS 10.4.6. The security review team's assessment is incorrect.
Status: NON_COMPLIANT. Violation: ASVS 10.4.6 — plain method accepted, must
be rejected. RFC 7636 Section 4.2 — client MUST use S256 when capable; server
accepts S256, therefore client is capable.

Territory 3: Authorization server enforcement — server accepts both S256 and
plain. ASVS 10.4.6 requires rejection of plain. Server is not enforcing ASVS
10.4.6 at the protocol level. Status: NON_COMPLIANT.

---

### S2 Node 5 — Applier outputs

Territory 1: CONCERN — OAuth 2.0 authorization code flow used (ASVS 10.4.4
satisfied), but PKCE implementation is non-compliant.

Territory 2: REPROCESS — ASVS 10.4.6 violation. Plain method used and accepted.
ASVS 10.4.6 requires rejection of plain method. Error code:
ASVS_10_4_6_PLAIN_METHOD_REJECTED.

Territory 3: REPROCESS — Authorization server accepts plain method in violation
of ASVS 10.4.6 server-side requirement. Error code:
ASVS_10_4_6_SERVER_NOT_REJECTING_PLAIN.

---

### S2 Node 6 — Result Translator outputs

Territory 1: "OAuth 2.0 authorization code flow present and implicit/password
grant types not used (ASVS 10.4.4 satisfied). However, the PKCE implementation
uses the 'plain' code_challenge_method, which is non-compliant."

Territory 2: "ASVS 10.4.6 compliance assessment is INCORRECT. ASVS 10.4.6
requires the authorization server to reject the 'plain' code_challenge_method.
The server currently accepts 'plain', violating this requirement. RFC 7636
Section 4.2 additionally requires clients to use S256 when capable; the server
supports S256, making the client capable but non-compliant."

Territory 3: "The authorization server must be reconfigured to reject the 'plain'
code_challenge_method and accept only 'S256'. Until this change is made, the
implementation does not satisfy ASVS 10.4.6."

---

### S2 Node 7 — Arbiter Selector output

Selected arbiters:
- ASVS-V10-OAuth-Arbiter (triggers on G-auth-model + B-threat-modeling for OAuth/PKCE input)
- ASVS-V9-Token-Arbiter (triggers on G-token-security for authorization code verification)

---

### S2 Node 8 — Arbiter Unit outputs

ASVS-V10-OAuth-Arbiter: REPROCESS. ASVS 10.4.6 requires the authorization
server to reject the 'plain' code_challenge_method. Current state: server
accepts 'plain'. This is a direct violation of ASVS 10.4.6. The security
review team's assessment of compliance is incorrect. Error code:
ASVS_10_4_6_PLAIN_METHOD_REJECTED. Remediation required: configure the
authorization server to reject 'plain' and accept only 'S256'. Client must
also be updated to use 'S256' per RFC 7636 Section 4.2.

ASVS-V9-Token-Arbiter: PASS on token validation shape (authorization code
flow, single-use codes). No token-specific violation in the provided input.

Combined S2 verdict: REPROCESS (one arbiter returns REPROCESS; REPROCESS
takes precedence over PASS in the combined verdict).

---

### S2 Node 9 — Formatter output

VERDICT: REPROCESS — ASVS 10.4.6 VIOLATION

The security review team's assessment that this implementation satisfies ASVS
10.4.6 is INCORRECT. Remediation is required before this implementation can
be considered compliant.

Violation: ASVS 10.4.6 — Proof Key for Code Exchange (PKCE)
- Requirement: The authorization server must reject the 'plain'
  code_challenge_method.
- Current state: Authorization server accepts 'plain'. The 'plain' method is
  in use and succeeds.
- Impact: Authorization code interception attacks are not mitigated. An attacker
  who intercepts the authorization code can exchange it without the
  code_verifier because the 'plain' method provides no meaningful security
  beyond the code itself.

RFC 7636 Section 4.2 additionally states: "If the client is capable of using
'S256', it MUST use 'S256'." The authorization server supports S256; therefore
the client MUST use S256. Plain method usage is not permitted under these
conditions.

Required actions:
1. Reconfigure the authorization server to reject code_challenge_method=plain
   and accept only code_challenge_method=S256.
2. Update the client to use code_challenge_method=S256.
3. Verify the code_verifier has at least 256 bits of entropy (RFC 7636 Section 4.1).
4. Re-run the security review after these changes.

This work item is returned to the development queue. The formatter does not
resolve this item.

---

---

## Scenario S3 — Session timeout satisfies ASVS but deviates from internal standard (borderline)

### S3 Input (to Node 1)

"Our session management implementation uses a 20-minute idle timeout with a
documented risk analysis (RA-AUTH-003) that justifies this value. However,
our enterprise security standard (ESS-SESSION-001) requires exactly 30 minutes
to align with the enterprise SSO token lifetime of our identity provider.
Our implementation uses 20 minutes, not 30 minutes. ASVS 7.3.1 says to
enforce re-authentication per documented risk analysis. Our risk analysis is
on file and recommends 20 minutes based on session hijacking risk modeling.
Is there a compliance issue with OWASP ASVS 5.0.0?"

---

### S3 Node 1 — Domain Splitter output

Territory 1: Session idle timeout and risk analysis compliance (ASVS V7.3.1)
Territory 2: Organizational security standard alignment (enterprise internal standard)
Territory 3: ASVS compliance scope boundary (what ASVS governs vs what internal standards govern)

---

### S3 Node 2 — Domain Translator outputs

Territory 1 → ["G-security-verification", "G-session-management"]
Territory 2 → ["G-security-verification", "B-threat-modeling"]
Territory 3 → ["G-security-verification"]

---

### S3 Node 3 — Scheme Planner output

Step 1: Verify ASVS 7.3.1 compliance independently of the internal standard.
ASVS 7.3.1 requires: inactivity timeout enforces re-authentication per documented
risk analysis. Check: does the implementation enforce re-authentication after
inactivity? Yes (20 minutes). Check: is there a documented risk analysis? Yes
(RA-AUTH-003). Observable output: ASVS 7.3.1 compliance status.

Step 2 (depends on Step 1): Assess the internal standard deviation. ESS-SESSION-001
requires 30 minutes. Implementation uses 20 minutes. 20 minutes is more restrictive
than 30 minutes. A more restrictive timeout is always more secure — it does not
decrease security relative to the 30-minute standard. Observable output: internal
standard deviation type (more restrictive vs less restrictive).

Step 3 (depends on Step 2): Determine whether the deviation has an ASVS implication.
ASVS 7.3.1 sets no specific value — it requires documentation. The 20-minute
value has documentation. The internal standard deviation does not create an ASVS
7.3.1 violation. Observable output: ASVS scope boundary determination.

Step 4 (depends on Step 3): Pass to Node 7 for arbiter selection.

---

### S3 Node 4 — Domain Processor outputs

Territory 1: ASVS 7.3.1 — 20-min idle timeout with RA-AUTH-003 on file.
ASVS 7.3.1 requires enforcement per documented risk analysis. Documentation
present, timeout enforced. Status: COMPLIANT.

Territory 2: ESS-SESSION-001 requires 30 minutes. Implementation uses 20 minutes.
Deviation: 10 minutes below the internal standard. Nature of deviation:
more restrictive (lower timeout = sooner re-authentication = higher security).
Status: INTERNAL_STANDARD_DEVIATION — more restrictive.

Territory 3: ASVS 7.3.1 does not specify a minimum or maximum timeout value.
The requirement is that the chosen value be documented and based on risk analysis.
The internal standard deviation is an organizational governance matter outside
the scope of ASVS 7.3.1. Status: ASVS scope boundary confirmed — deviation is
organizational, not ASVS.

---

### S3 Node 5 — Applier outputs

Territory 1: PASS — V7.3.1 satisfied (20-min timeout, RA-AUTH-003 documents
the risk analysis justification).

Territory 2: CONCERN — implementation deviates from ESS-SESSION-001 (requires
30 minutes). The deviation is more restrictive (20 min < 30 min). This is an
internal governance concern, not an ASVS violation. Teams should reconcile
the internal standard deviation through the standard change process.

Territory 3: NOTE — ASVS 7.3.1 compliance is not affected by this deviation.
ASVS governs that a risk analysis exists and re-authentication is enforced.
The specific value is an organizational decision.

---

### S3 Node 6 — Result Translator outputs

Territory 1: "Session idle timeout of 20 minutes with risk analysis RA-AUTH-003
on file satisfies ASVS 7.3.1 (inactivity timeout enforced per documented risk
analysis)."

Territory 2: "The implementation deviates from enterprise security standard
ESS-SESSION-001, which requires 30 minutes. The 20-minute timeout is more
restrictive (shorter = sooner re-authentication). This is a governance concern:
the internal standard may need updating to reflect the risk analysis findings,
or the implementation may need to align to the internal standard by increasing
the timeout to 30 minutes. Neither option affects ASVS 7.3.1 compliance."

Territory 3: "ASVS 7.3.1 compliance is confirmed. The internal standard
deviation is outside the scope of ASVS compliance verification."

---

### S3 Node 7 — Arbiter Selector output

Selected arbiters:
- ASVS-V7-Session-Arbiter (triggers on G-session-management)

Note: ASVS-V8-AuthZ-Arbiter and ASVS-V9-Token-Arbiter are not selected —
no input content triggers their domain labels for this scenario.

---

### S3 Node 8 — Arbiter Unit output

ASVS-V7-Session-Arbiter: CONCERN. ASVS 7.3.1 is satisfied — 20-minute
idle timeout is enforced with RA-AUTH-003 on file documenting the risk
analysis justification. No ASVS violation is present.

However, a concern is raised: the implementation deviates from organizational
security standard ESS-SESSION-001, which requires 30 minutes. The deviation
is more restrictive (20 min is more secure than 30 min in terms of session
exposure window) but creates a governance mismatch. The arbiter recommends
one of two resolutions: (a) update ESS-SESSION-001 to accept the 20-minute
value supported by RA-AUTH-003, or (b) increase the timeout to 30 minutes
and update RA-AUTH-003 accordingly.

This is not a REPROCESS. ASVS 7.3.1 compliance is confirmed. The concern is
organizational governance, not a security control failure.

Combined S3 verdict: CONCERN (one arbiter returns CONCERN; no REPROCESS).

---

### S3 Node 9 — Formatter output

VERDICT: CONCERN — INTERNAL STANDARD DEVIATION (ASVS 7.3.1 COMPLIANT)

ASVS 5.0.0 compliance for the session idle timeout:
- V7.3.1: SATISFIED. 20-minute idle timeout enforced. Risk analysis RA-AUTH-003
  on file documents the justification. ASVS 7.3.1 requires enforcement per
  documented risk analysis — both conditions met.

Internal standard concern:
- ESS-SESSION-001 requires a 30-minute session idle timeout.
- Current implementation: 20 minutes.
- This is a more restrictive timeout (shorter = higher security). No security
  regression relative to ESS-SESSION-001.
- Governance concern: the internal standard and the implemented value are
  misaligned. This should be resolved through the standard update process.

Recommended resolution paths:
(a) Update ESS-SESSION-001 to accept the 20-minute value, supported by
    RA-AUTH-003's risk analysis.
(b) Increase the implementation timeout to 30 minutes and update RA-AUTH-003
    to document the revised risk analysis.

This item does not require reprocessing. ASVS compliance is confirmed.
The governance concern is forwarded to the security standard owner.
