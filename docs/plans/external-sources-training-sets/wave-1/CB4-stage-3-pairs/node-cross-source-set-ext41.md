# Node Cross-Source Set Extension 41
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1261–1270 (20 instances — 10 base pairs × 2 phrasings)
# Sources: RFC 6749 §4.1.3, OIDC Dynamic Registration §3.2,
#          CWE-601 / ASVS V13.4.1, RFC 7009 §1,
#          OIDC Core §5.3.4, RFC 6749 §4.1.2,
#          ASVS V6.3.2, RFC 9101 §10.3, NIST SSDF PO.1
# Running total after this file: 2540 instances (1270 base pairs)
# Formula check: 120 + (1270 − 60) × 2 = 120 + 2420 = 2540 ✓

---

## Pair[CB4-3.4-1261]/A

An OIDC provider's revocation endpoint returns `error=unsupported_token_type`
for access tokens — only refresh token revocation is supported. The same
provider accepts JAR request objects with `exp` values set 30 days into the
future. A security review flags both observations and asks whether their
combination requires a REPROCESS finding.

## Pair[CB4-3.4-1261]/B

CONCERN. Two guidance-level observations: (1) access token revocation absent —
RFC 7009 §1 SHOULD support access token revocation; a 15-minute TTL provides
partial compensating control; no unconditional MUST violated; (2) JAR exp 30 days
in the future — RFC 9101 §10.3 provides SHOULD-level guidance against excessive
expiry without a numeric MUST ceiling. Both findings are guidance-level. The
graduation rule applies: N CONCERNs do not escalate to REPROCESS regardless of
count. CONCERN.

## Pair[CB4-3.4-1261]/Reason

Two guidance-level gaps: access token revocation absent (RFC 7009 §1 SHOULD) +
JAR exp 30 days (RFC 9101 §10.3 SHOULD); graduation rule: N CONCERNs ≠ REPROCESS
→ CONCERN.

## Pair[CB4-3.4-1261]/Phrasing-B

Verdict CONCERN. RFC 7009 §1 SHOULD and RFC 9101 §10.3 qualitative guidance are
both below MUST threshold. Two guidance-level findings do not aggregate to REPROCESS.
CONCERN.

---

## Pair[CB4-3.4-1262]/A

An OIDC provider's TOTP MFA verification endpoint applies no server-side rate
limiting on failed attempts. The same AS's revocation endpoint returns
`error=unsupported_token_type` for access tokens, supporting only refresh token
revocation. Both are noted in a security review. The reviewer asserts that two
operational security gaps together justify escalation.

## Pair[CB4-3.4-1262]/B

CONCERN. Two guidance-level observations: (1) TOTP no rate limiting — ASVS V6.3.2
SHOULD rate-limit authenticator code entry; no unconditional MUST for specific
mechanism; password prerequisite partially limits attacker scope; (2) access token
revocation absent — RFC 7009 §1 SHOULD; no MUST. The reviewer's escalation argument
is rejected: the graduation rule is categorical. N CONCERNs do not escalate to
REPROCESS regardless of operational significance or count. Both findings are guidance-
level. CONCERN.

## Pair[CB4-3.4-1262]/Reason

TOTP no rate limiting (ASVS V6.3.2 SHOULD) + access token revocation absent
(RFC 7009 §1 SHOULD); graduation rule: N CONCERNs ≠ REPROCESS; escalation argument
rejected → CONCERN.

## Pair[CB4-3.4-1262]/Phrasing-B

Verdict CONCERN. Both observations are SHOULD-level guidance gaps. The graduation
rule forecloses escalation regardless of operational significance. CONCERN.

---

## Pair[CB4-3.4-1263]/A

A security audit of an OIDC AS surfaces three guidance-level observations: the
revocation endpoint lacks access token revocation support; the TOTP MFA endpoint
has no rate limiting; and JAR request objects with 30-day `exp` values are accepted.
The auditor argues that three concurrent gaps affecting authentication, session
security, and protocol integrity together constitute a systemic deficiency that
should be classified REPROCESS.

## Pair[CB4-3.4-1263]/B

CONCERN. All three observations are guidance-level: (1) access token revocation
absent — RFC 7009 §1 SHOULD; no MUST; (2) TOTP no rate limiting — ASVS V6.3.2
SHOULD; no MUST for specific mechanism; (3) JAR exp 30-day — RFC 9101 §10.3
qualitative SHOULD; no numeric MUST ceiling. The auditor's systemic-deficiency
escalation argument is rejected: the graduation rule is unconditional and categorical.
N CONCERNs = CONCERN regardless of count, domain diversity, or systemic framing.
Only a MUST-level violation triggers REPROCESS. CONCERN.

## Pair[CB4-3.4-1263]/Reason

Three guidance-level gaps: access token revocation (RFC 7009 §1 SHOULD) + TOTP
rate limit (ASVS V6.3.2 SHOULD) + JAR exp (RFC 9101 §10.3 SHOULD); graduation
rule: 3 CONCERNs ≠ REPROCESS; systemic-deficiency argument rejected → CONCERN.

## Pair[CB4-3.4-1263]/Phrasing-B

Verdict CONCERN. Three SHOULD-level gaps do not aggregate to REPROCESS. The
graduation rule is unconditional; systemic framing does not change the classification.
CONCERN.

---

## Pair[CB4-3.4-1264]/A

An OIDC provider has no formal security requirements documentation for its
authorization server (NIST SSDF PO.1 gap). A follow-up technical review confirms
that the OIDC provider's ID tokens use a UUID-based `sub` claim while the same
provider's UserInfo endpoint returns the authenticated user's email address as
`sub` — a mismatch that causes all RP sub cross-checks to fail. The team proposes
filing both findings as separate low-priority items.

## Pair[CB4-3.4-1264]/B

REPROCESS. PO.1 absence alone is CONCERN. The PO.1 escalation rule applies: PO.1
absence combined with a traceable production MUST violation escalates to REPROCESS.
OIDC Core §5.3.4 MUST: the RP must verify that the `sub` claim in the UserInfo
response is identical to the `sub` claim in the ID token; the AS must ensure
sub consistency across both responses. The sub mismatch is an active production
MUST violation — every RP sub cross-check fails. The PO.1 gap means the sub
consistency requirement was never formally tracked as an obligation. The team's
separate low-priority framing is rejected: the PO.1 escalation rule and the OIDC
Core §5.3.4 MUST together mandate REPROCESS. REPROCESS.

## Pair[CB4-3.4-1264]/Reason

PO.1 gap (CONCERN alone) + active MUST violation (OIDC Core §5.3.4 MUST: UserInfo
sub must match ID token sub; mismatch in production) = PO.1 escalation → REPROCESS.

## Pair[CB4-3.4-1264]/Phrasing-B

Verdict REPROCESS. PO.1 escalation rule: PO.1 + traceable MUST violation =
REPROCESS. OIDC Core §5.3.4 MUST requires consistent sub across ID token and
UserInfo; mismatch is an active production MUST violation. REPROCESS.

---

## Pair[CB4-3.4-1265]/A

A JAR (request object) with `exp` set 30 days in the future is accepted by the
AS. The same AS's authorization code exchange uses a non-atomic check-and-set
operation for single-use enforcement — under load, two simultaneous exchange
requests for the same code both succeed and receive valid access tokens. The team
asks whether the JAR expiry observation is relevant to the severity assessment of
the race condition.

## Pair[CB4-3.4-1265]/B

REPROCESS. The JAR exp 30-day finding is CONCERN-level (RFC 9101 §10.3 qualitative
SHOULD; no numeric MUST ceiling). The authorization code race condition is REPROCESS-
level: RFC 6749 §4.1.2 MUST requires authorization codes to be single-use; two
successful exchanges for the same code violate this MUST unconditionally regardless
of whether the root cause is a concurrency issue. REPROCESS overrides CONCERN —
the guidance-level JAR finding is irrelevant to the severity of the MUST violation
and cannot moderate it. REPROCESS.

## Pair[CB4-3.4-1265]/Reason

JAR exp 30 days (RFC 9101 §10.3 SHOULD, CONCERN) + auth code race condition
(RFC 6749 §4.1.2 MUST single-use violated, REPROCESS); REPROCESS overrides CONCERN
→ REPROCESS.

## Pair[CB4-3.4-1265]/Phrasing-B

Verdict REPROCESS. JAR exp guidance gap is CONCERN-level. Auth code non-atomic
check enabling two successful exchanges violates RFC 6749 §4.1.2 MUST. REPROCESS
overrides CONCERN; JAR finding irrelevant to severity of MUST violation. REPROCESS.

---

## Pair[CB4-3.4-1266]/A

An AS revocation endpoint lacks access token revocation support (refresh tokens
only). The same AS's token endpoint does not require the client to provide
`redirect_uri` at the exchange step despite the authorization request having
included it — the AS accepts the exchange without the parameter and without
comparison. The team argues the revocation gap partially offsets the redirect_uri
omission by limiting long-term token risk.

## Pair[CB4-3.4-1266]/B

REPROCESS. The access token revocation gap is CONCERN-level (RFC 7009 §1 SHOULD;
no MUST). The absent redirect_uri comparison at token exchange is REPROCESS-level:
RFC 6749 §4.1.3 MUST requires the AS to verify that the redirect_uri at the token
endpoint is identical to the value from the authorization request when redirect_uri
was included in the authorization request. No REQUIRED parameter omission exception
exists for single-registered-URI clients. The team's offset argument is rejected:
REPROCESS overrides CONCERN — a guidance gap cannot offset a MUST violation. The
revocation gap and the redirect_uri MUST are independent requirements. REPROCESS.

## Pair[CB4-3.4-1266]/Reason

Access token revocation absent (RFC 7009 §1 SHOULD, CONCERN) + redirect_uri absent
at token exchange (RFC 6749 §4.1.3 MUST, REPROCESS); offset argument rejected;
REPROCESS overrides CONCERN → REPROCESS.

## Pair[CB4-3.4-1266]/Phrasing-B

Verdict REPROCESS. Access token revocation is a SHOULD-level guidance gap. Absent
redirect_uri comparison at token exchange violates RFC 6749 §4.1.3 MUST. Offset
argument rejected. REPROCESS overrides CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1267]/A

An OIDC AS's TOTP MFA verification has no rate limiting on failed attempts. The
same AS's login page redirects to a user-supplied `return_url` query parameter
after successful authentication without validating it against any registered URI
list. The team argues the TOTP gap is low severity and should be evaluated
separately from the redirect behavior.

## Pair[CB4-3.4-1267]/B

REPROCESS. The TOTP rate-limiting absence is CONCERN-level (ASVS V6.3.2 SHOULD;
no unconditional MUST for specific mechanism). The unvalidated `return_url` redirect
is REPROCESS-level: CWE-601 and ASVS V13.4.1 MUST require all redirect targets to
be validated against a configured allowlist; AS origin open redirects are particularly
dangerous due to the trusted domain context and active OAuth flows. The two findings
are independent; the team's separate-evaluation request does not change the REPROCESS
threshold being met by the redirect finding. REPROCESS overrides CONCERN. REPROCESS.

## Pair[CB4-3.4-1267]/Reason

TOTP no rate limit (ASVS V6.3.2 SHOULD, CONCERN) + unvalidated return_url redirect
(CWE-601 / ASVS V13.4.1 MUST, REPROCESS); REPROCESS overrides CONCERN; separate-
evaluation request does not change verdict → REPROCESS.

## Pair[CB4-3.4-1267]/Phrasing-B

Verdict REPROCESS. TOTP rate limiting is SHOULD-level guidance. Unvalidated
return_url redirect violates ASVS V13.4.1 MUST. REPROCESS overrides CONCERN;
separate evaluation of the guidance gap does not moderate the MUST violation.
REPROCESS.

---

## Pair[CB4-3.4-1268]/A

An OIDC provider's TOTP MFA has no server-side rate limiting on failed attempts.
The same provider's dynamic client registration returns a registration response
for confidential clients with `token_endpoint_auth_method=client_secret_basic`
that omits the `client_secret` field. The team argues the absent rate limiting is
unrelated to and does not affect the registration protocol issue.

## Pair[CB4-3.4-1268]/B

REPROCESS. The TOTP rate-limiting absence is CONCERN-level (ASVS V6.3.2 SHOULD;
no MUST). The absent `client_secret` in the dynamic registration response is
REPROCESS-level: OIDC Dynamic Client Registration §3.2 MUST include `client_secret`
in the registration response when the auth method requires it; the only transmission
window is the registration response itself, since hashed storage precludes later
retrieval. The team is correct that the two findings are technically independent —
but independence does not prevent REPROCESS overriding CONCERN when a MUST-level
finding is present. REPROCESS overrides CONCERN. REPROCESS.

## Pair[CB4-3.4-1268]/Reason

TOTP no rate limit (ASVS V6.3.2 SHOULD, CONCERN) + client_secret absent from
registration response (OIDC Dynamic Registration §3.2 MUST, REPROCESS); independence
argument acknowledged but REPROCESS overrides CONCERN → REPROCESS.

## Pair[CB4-3.4-1268]/Phrasing-B

Verdict REPROCESS. TOTP rate limiting is guidance-level (CONCERN). Absent
client_secret in the registration response violates OIDC Dynamic Registration
§3.2 MUST. Technical independence between findings does not prevent REPROCESS
overriding CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1269]/A

An OIDC authorization server is reviewed against all eight ext48 control points:
the token endpoint requires `redirect_uri` when it was present in the authorization
request and performs byte-for-byte comparison; dynamic client registration responses
include `client_secret` for all auth methods that require it; all login-page
redirect targets are validated against a registered allowlist before any redirect
is issued; the revocation endpoint supports both access token and refresh token
revocation; ID token `sub` and UserInfo `sub` are identical for every user;
authorization code single-use is enforced via database-level atomic constraint;
TOTP MFA endpoints apply per-user rate limiting with lockout after five failures;
and JAR request objects with exp exceeding 300 seconds from submission are rejected
with `invalid_request_object`. No anomalies are detected.

## Pair[CB4-3.4-1269]/B

PASS. All eight ext48 control points are satisfied: (1) redirect_uri comparison at
token endpoint per RFC 6749 §4.1.3 MUST; (2) client_secret in registration response
per OIDC Dynamic Registration §3.2 MUST; (3) redirect target allowlist validation
per CWE-601 / ASVS V13.4.1 MUST; (4) access token revocation supported per RFC
7009 §1 SHOULD (exceeds SHOULD floor); (5) sub consistency across ID token and
UserInfo per OIDC Core §5.3.4 MUST; (6) atomic single-use enforcement per RFC 6749
§4.1.2 MUST; (7) TOTP rate limiting per ASVS V6.3.2 SHOULD (exceeds SHOULD floor);
(8) JAR exp bounded at 300 seconds per RFC 9101 §10.3 guidance. No violations or
guidance gaps found. PASS.

## Pair[CB4-3.4-1269]/Reason

All eight ext48 controls compliant: RFC 6749 §4.1.3 (redirect_uri), Dynamic
Registration §3.2 (client_secret), CWE-601/ASVS V13.4.1 (redirect allowlist),
RFC 7009 §1 (AT revocation), OIDC Core §5.3.4 (sub consistency), RFC 6749 §4.1.2
(atomic single-use), ASVS V6.3.2 (TOTP rate limit), RFC 9101 §10.3 (JAR exp) →
PASS.

## Pair[CB4-3.4-1269]/Phrasing-B

Verdict PASS. All eight ext48 control points satisfied: redirect_uri comparison
(RFC 6749 §4.1.3), client_secret in registration (OIDC Dynamic Registration §3.2),
redirect allowlist (ASVS V13.4.1), AT revocation (RFC 7009 §1), sub consistency
(OIDC Core §5.3.4), atomic single-use (RFC 6749 §4.1.2), TOTP rate limiting (ASVS
V6.3.2), bounded JAR exp (RFC 9101 §10.3). PASS.

---

## Pair[CB4-3.4-1270]/A

An OIDC AS's TOTP MFA verification endpoint applies no server-side rate limiting
on failed authentication attempts. JAR request objects submitted to the same AS
with `exp` values set 30 days in the future are also accepted without rejection.
An engineer argues that two independent guidance gaps, both involving failure to
enforce temporal or attempt-count boundaries, constitute a pattern of negligence
that should be escalated to REPROCESS.

## Pair[CB4-3.4-1270]/B

CONCERN. Two guidance-level findings: (1) TOTP no rate limiting — ASVS V6.3.2
SHOULD; no unconditional MUST for specific mechanism; password prerequisite partially
limits scope; (2) JAR exp 30 days — RFC 9101 §10.3 qualitative SHOULD guidance;
no numeric MUST ceiling. The engineer's pattern-of-negligence escalation argument
is rejected: the graduation rule is categorical and admits no pattern-based
exception — N CONCERNs = CONCERN regardless of thematic overlap, operational
significance, or perceived systemic negligence. Only a MUST-level violation triggers
REPROCESS. CONCERN.

## Pair[CB4-3.4-1270]/Reason

TOTP no rate limit (ASVS V6.3.2 SHOULD, CONCERN) + JAR exp 30 days (RFC 9101 §10.3
SHOULD, CONCERN); graduation rule: N CONCERNs ≠ REPROCESS; pattern-of-negligence
argument rejected → CONCERN.

## Pair[CB4-3.4-1270]/Phrasing-B

Verdict CONCERN. Both findings are SHOULD-level guidance gaps. The graduation rule
forecloses pattern-based escalation. N CONCERNs = CONCERN. CONCERN.
