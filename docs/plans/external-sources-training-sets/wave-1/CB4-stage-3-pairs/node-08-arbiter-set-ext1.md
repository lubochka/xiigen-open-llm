# Node 8 — Arbiter Units Pairs — Extension Set 1
# Batch CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Wave 1 — Stage 3 Action 3 (coverage extension)
# Sequence range: CB4-3.4-061 through CB4-3.4-072
# Base pairs: 12 × 2 phrasings = 24 pair instances
# Focus: Stratum C depth reasoning, new scenarios, cross-source reinforcement
# Production date: 2026-05-05

---

## Coverage map

| Pair | Scenario | Source | Verdict | Stratum | New coverage |
|------|----------|--------|---------|---------|-------------|
| 061 | RFC 7519 §4.1.4 MAY clause — 10-min clock skew | IETF-RFCS | CONCERN | C | MAY ≠ MUST: no numeric maximum, convention exceeded |
| 062 | ASVS V9 RS256 with asymmetric key — PASS | OWASP-ASVS | PASS | A | RS256 = acceptable; key custody matters |
| 063 | CWE-863 incorrect authorization condition | CWE-TOP25 | REPROCESS | C | 863 vs 862: check present, condition wrong |
| 064 | SSDF PW.8 absent — only PW.1 + PW.2 satisfied | NIST-SSDF | CONCERN | D | PW.8 gap alone = CONCERN, not REPROCESS |
| 065 | ASVS multi-arbiter — V8 REPROCESS, V6/V7/V9 PASS | OWASP-ASVS | REPROCESS | C | One REPROCESS overrides other PASSes |
| 066 | CWE-287 improper authentication mechanism | CWE-TOP25 | REPROCESS | C | Authn weakness distinct from authz weakness |
| 067 | RFC OAuth2 complete conformance PASS | IETF-RFCS | PASS | A | All OAuth2 normative sections satisfied |
| 068 | SSDF PW.1+PW.2+PW.8 all satisfied — full PASS | NIST-SSDF | PASS | A | Complete SSDF PW process evidence |
| 069 | ASVS V10 S256 confirmed PASS | OWASP-ASVS | PASS | A | S256 satisfies V10.4.6, plain does not |
| 070 | CWE-269 privilege scope — CONCERN (not REPROCESS) | CWE-TOP25 | CONCERN | C | 269 = wrong scope, not absent check |
| 071 | SSDF PW.2 timing gap with ALL findings resolved | NIST-SSDF | CONCERN | C | Timing gap persists even if findings resolved |
| 072 | RFC 7519 RS256 asymmetric — trust boundary PASS | IETF-RFCS | PASS | C | Asymmetric key removes cross-server forgery |

---

## Pairs

---

### Pair CB4-3.4-061 — RFC 7519 §4.1.4 MAY clause: clock skew CONCERN (not REPROCESS) (VERDICT)
Source: IETF RFCs (RFC 7519)
License: IETF Trust (pre-5378 works)
Stratum: C

A: The RFC-JWT-Arbiter received: JWT access tokens, exp claim enforced. Observed clock
skew tolerance configured at 10 minutes — all resource servers accept tokens up to 10
minutes past their exp value to accommodate server clock differences. RFC 7519 §4.1.4
(Expiration Time): MUST not accept after exp. RFC 7519 §4.1.4 also notes that a small
leeway (defined as "a few minutes" in practice, typically ≤ 5 minutes) may account for
clock skew. 10 minutes is above conventional leeway. No explicit MUST maximum is stated
in RFC 7519 for the leeway value itself — §4.1.4 uses MAY for the leeway allowance.
Produce verdict.

B: CONCERN. RFC_7519_S4_1_4_CLOCK_SKEW_EXCEEDS_CONVENTION.
RFC 7519 §4.1.4 mandates that tokens MUST not be accepted after their expiration time.
The leeway provision is a MAY clause — RFC 7519 does not define a numeric maximum for
the leeway window. 10 minutes is above the generally accepted convention of ≤ 5 minutes
but does not violate a MUST clause, because no RFC MUST clause specifies the maximum
leeway value.
This is CONCERN, not REPROCESS:
  - REPROCESS would require a violated MUST clause with a specific maximum
  - The 10-minute value exceeds convention (advisory) but not a normative ceiling
Concern details:
  The 10-minute leeway extends the token acceptance window beyond conventional practice.
  Tokens expired by 6–10 minutes remain accepted. This weakens the exp enforcement that
  §4.1.4 MUST establishes.
Resolution: reduce clock skew tolerance to ≤ 5 minutes to align with convention; document
  the business justification if higher leeway is required by system constraints.

Reason: RFC MUST clauses produce REPROCESS. RFC MAY clauses and advisory conventions
produce CONCERN. RFC 7519 §4.1.4 does not provide a numeric MUST ceiling on leeway —
the "few minutes" language is descriptive, not normative. Treating a MAY deviation as
REPROCESS is failure mode FM-RFC-3 (MUST_MAY_CONFUSION).

---

### Pair CB4-3.4-061 — Phrasing B
Source: IETF RFCs (RFC 7519)
License: IETF Trust (pre-5378 works)
Stratum: C

A: JWT exp claim enforced. Clock skew tolerance: 10 minutes on all resource servers.
RFC 7519 §4.1.4: MUST not accept after exp; leeway is MAY (no numeric MUST maximum).
10 minutes exceeds 5-minute convention. Produce verdict.

B: CONCERN. RFC_7519_S4_1_4_CLOCK_SKEW_EXCEEDS_CONVENTION.
RFC 7519 §4.1.4 mandates that tokens MUST not be accepted after their expiration time.
The leeway provision is a MAY clause — RFC 7519 does not define a numeric maximum for
the leeway window. 10 minutes is above the generally accepted convention of ≤ 5 minutes
but does not violate a MUST clause, because no RFC MUST clause specifies the maximum
leeway value.
This is CONCERN, not REPROCESS:
  - REPROCESS would require a violated MUST clause with a specific maximum
  - The 10-minute value exceeds convention (advisory) but not a normative ceiling
Concern details:
  The 10-minute leeway extends the token acceptance window beyond conventional practice.
  Tokens expired by 6–10 minutes remain accepted. This weakens the exp enforcement that
  §4.1.4 MUST establishes.
Resolution: reduce clock skew tolerance to ≤ 5 minutes to align with convention; document
  the business justification if higher leeway is required by system constraints.

---

### Pair CB4-3.4-062 — ASVS V9 RS256 asymmetric JWT — PASS (VERDICT)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: The ASVS-V9-Token-Arbiter received: JWT access tokens, algorithm RS256, private
signing key held exclusively by the authorization server, public verification keys
distributed to resource servers via JWKS endpoint (RFC 7517). Signature verified
server-side on all resource servers using public key only. exp, iss, aud claims
validated. V9.1.1 (signature verified), V9.1.2 (exp enforced), V9.2.1 (approved
algorithm), V9.2.3 (iss and aud validated) all claimed satisfied. Produce verdict.

B: PASS.
ASVS V9 requirements verified:
  V9.1.1 — JWT signature verified server-side: SATISFIED.
    Evidence: all resource servers verify signature before token acceptance.
  V9.1.2 — Expiry (exp) enforced: SATISFIED.
    Evidence: exp claim validated on all resource servers.
  V9.2.1 — Approved algorithm (RS256): SATISFIED.
    Evidence: RS256 is an asymmetric algorithm (RSA + SHA-256). Private signing key
    held exclusively by authorization server — resource servers hold only the public
    verification key. A resource server cannot forge tokens. Trust boundary preserved.
  V9.2.3 — Issuer (iss) and audience (aud) validated: SATISFIED.
    Evidence: iss and aud validated on all resource servers.
All evaluated V9 requirements satisfied. No REPROCESS, no CONCERN.

Reason: RS256 is asymmetric — the signing key (private) and verification key (public)
are different. Unlike HS256 shared-key deployments, resource servers holding only the
public key cannot forge tokens. The trust boundary between authorization server and
resource servers is intact. This is the correct key architecture for multi-resource-server
deployments.

---

### Pair CB4-3.4-062 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: JWT RS256, private signing key on authorization server only, public keys via JWKS
endpoint, resource servers verify with public key, exp/iss/aud validated, V9.1.1/V9.1.2/
V9.2.1/V9.2.3 claimed. Produce verdict.

B: PASS.
ASVS V9 requirements verified:
  V9.1.1 — JWT signature verified server-side: SATISFIED.
    Evidence: all resource servers verify signature before token acceptance.
  V9.1.2 — Expiry (exp) enforced: SATISFIED.
    Evidence: exp claim validated on all resource servers.
  V9.2.1 — Approved algorithm (RS256): SATISFIED.
    Evidence: RS256 is an asymmetric algorithm (RSA + SHA-256). Private signing key
    held exclusively by authorization server — resource servers hold only the public
    verification key. A resource server cannot forge tokens. Trust boundary preserved.
  V9.2.3 — Issuer (iss) and audience (aud) validated: SATISFIED.
    Evidence: iss and aud validated on all resource servers.
All evaluated V9 requirements satisfied. No REPROCESS, no CONCERN.

---

### Pair CB4-3.4-063 — CWE-863 incorrect authorization condition (REPROCESS, not PASS) (VERDICT)
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: C

A: The CWE-Authz-Arbiter received: API endpoint DELETE /api/v1/posts/{postId}.
Authorization check present: requires user to be authenticated AND post.authorId ==
requestingUser.id. However, the condition is evaluated using == with string type coercion
in JavaScript — post.authorId is an integer, requestingUser.id is a string. When the
post belongs to user ID 1 and the requesting user is "1" (string), the check passes for
all users because "1" == 1 is true in JavaScript loose equality. CWE-862 evaluation:
authorization check is present. CWE-863 evaluation: the authorization check condition is
incorrect (loose equality allows type confusion). Produce verdict.

B: REPROCESS. CWE_863_INCORRECT_AUTHORIZATION_CONDITION.
CWE-863 (Incorrect Authorization): CONFIRMED.
  An authorization check is present (post.authorId == requestingUser.id) — CWE-862
  (Missing Authorization) does not apply.
  The check condition uses JavaScript loose equality (==) instead of strict equality
  (===). When post.authorId is integer 1 and requestingUser.id is string "1",
  the check passes due to type coercion.
  Attack path: any authenticated user with string representation of the post author's
  numeric ID can delete any post. The authorization check executes and passes for all
  users matching the numeric ID by string representation.
Finding: CWE-863 — the authorization check logic is present but its condition is
  incorrect. The check authorizes based on a coerceable comparison rather than a type-safe
  identity check.
Remediation: replace == with === for all authorization comparisons; ensure authorId and
  userId are compared as the same type (both integer or both string, consistently).

Reason: CWE-863 (Incorrect Authorization) applies when a check executes but its
conditions are insufficient to enforce the intended policy. This is distinct from CWE-862
(check absent) and CWE-269 (check present, scope too broad). Type confusion in
authorization conditions is a classic CWE-863 pattern — the check appears to work but
evaluates to true in unintended cases.

---

### Pair CB4-3.4-063 — Phrasing B
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: C

A: DELETE /api/v1/posts/{postId}. Authorization check present: post.authorId ==
requestingUser.id (JavaScript loose equality). post.authorId is integer, requestingUser.id
is string. "1" == 1 is true — type coercion allows all users to pass the check for
author ID 1. CWE-862: check present. CWE-863: condition incorrect. Produce verdict.

B: REPROCESS. CWE_863_INCORRECT_AUTHORIZATION_CONDITION.
CWE-863 (Incorrect Authorization): CONFIRMED.
  An authorization check is present (post.authorId == requestingUser.id) — CWE-862
  (Missing Authorization) does not apply.
  The check condition uses JavaScript loose equality (==) instead of strict equality
  (===). When post.authorId is integer 1 and requestingUser.id is string "1",
  the check passes due to type coercion.
  Attack path: any authenticated user with string representation of the post author's
  numeric ID can delete any post. The authorization check executes and passes for all
  users matching the numeric ID by string representation.
Finding: CWE-863 — the authorization check logic is present but its condition is
  incorrect. The check authorizes based on a coerceable comparison rather than a type-safe
  identity check.
Remediation: replace == with === for all authorization comparisons; ensure authorId and
  userId are compared as the same type (both integer or both string, consistently).

---

### Pair CB4-3.4-064 — SSDF PW.8 absent: only PW.1 + PW.2 satisfied (CONCERN) (VERDICT)
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: D

A: The SSDF-PW-Arbiter received: security requirements document TS-AUTHZ-001 (pre-design,
week 1) — PW.1 evidence present. Design review record DR-AUTHZ-2026-04 (pre-implementation,
week 4, all findings resolved) — PW.2 evidence present and timing correct. No security
testing record submitted. The team states testing is "planned for next sprint." No
TR-AUTHZ document exists yet. Produce verdict.

B: CONCERN. SSDF_PW8_TESTING_EVIDENCE_ABSENT.
SSDF PW practice compliance:
  PW.1 (Security requirements before design): SATISFIED.
    Evidence: TS-AUTHZ-001, week 1, pre-design.
  PW.2 (Design review before implementation): SATISFIED.
    Evidence: DR-AUTHZ-2026-04, week 4, pre-implementation. All findings resolved.
    Timing: week 4 < implementation start. Correct.
  PW.8 (Testing for security): NOT SATISFIED.
    Evidence: no security testing record submitted. "Planned for next sprint" is an
    intent statement, not evidence.
    Gap type: evidence absent (not a timing gap — the artifact simply does not exist yet).
Overall verdict: CONCERN — PW.8 evidence absent.
This is CONCERN, not REPROCESS:
  PW.1 and PW.2 are fully satisfied with correct timing. The submission is incomplete
  for PW.8 but not structurally broken. REPROCESS applies to violations of present
  evidence; CONCERN applies when required evidence has not yet been submitted.
Resolution: produce security test results TR-AUTHZ and re-submit.

Reason: An absent PW.8 artifact produces CONCERN, not REPROCESS. REPROCESS applies when
submitted evidence contains a violation (e.g., HS256 shared key). CONCERN applies when
evidence is incomplete or not yet submitted. The distinction prevents blocking a team
on a timing issue when two of three PW practices are cleanly satisfied.

---

### Pair CB4-3.4-064 — Phrasing B
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: D

A: SSDF submission: TS-AUTHZ-001 (PW.1, pre-design), DR-AUTHZ-2026-04 (PW.2,
pre-implementation, findings resolved). No testing record submitted — "planned next
sprint." Produce verdict.

B: CONCERN. SSDF_PW8_TESTING_EVIDENCE_ABSENT.
SSDF PW practice compliance:
  PW.1 (Security requirements before design): SATISFIED.
    Evidence: TS-AUTHZ-001, week 1, pre-design.
  PW.2 (Design review before implementation): SATISFIED.
    Evidence: DR-AUTHZ-2026-04, week 4, pre-implementation. All findings resolved.
    Timing: week 4 < implementation start. Correct.
  PW.8 (Testing for security): NOT SATISFIED.
    Evidence: no security testing record submitted. "Planned for next sprint" is an
    intent statement, not evidence.
    Gap type: evidence absent (not a timing gap — the artifact simply does not exist yet).
Overall verdict: CONCERN — PW.8 evidence absent.
This is CONCERN, not REPROCESS:
  PW.1 and PW.2 are fully satisfied with correct timing. The submission is incomplete
  for PW.8 but not structurally broken. REPROCESS applies to violations of present
  evidence; CONCERN applies when required evidence has not yet been submitted.
Resolution: produce security test results TR-AUTHZ and re-submit.

---

### Pair CB4-3.4-065 — ASVS multi-arbiter: V8 REPROCESS overrides V6/V7/V9 PASS (VERDICT)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: C

A: Four ASVS arbiter results received. ASVS-V6-Auth-Arbiter: PASS — authentication
controls verified. ASVS-V7-Session-Arbiter: PASS — session management verified.
ASVS-V9-Token-Arbiter: PASS — JWT RS256 verified. ASVS-V8-AuthZ-Arbiter: REPROCESS —
ASVS_V8_OWNERSHIP_CHECK_ABSENT — GET /api/users/{userId}/appointments returns all
appointments for any userId without checking that requestingUser.id == userId. Three
PASS verdicts, one REPROCESS verdict. Produce consolidated verdict.

B: VERDICT: REPROCESS.
Consolidated verdict: REPROCESS — one or more arbiter units returned REPROCESS.
  ASVS-V6-Auth-Arbiter: PASS (authentication controls verified)
  ASVS-V7-Session-Arbiter: PASS (session management verified)
  ASVS-V9-Token-Arbiter: PASS (JWT RS256 verified)
  ASVS-V8-AuthZ-Arbiter: REPROCESS — ASVS_V8_OWNERSHIP_CHECK_ABSENT
Consolidation rule: REPROCESS in any arbiter unit overrides PASS verdicts in all other
  units. The three PASS verdicts are valid and do not require remediation, but the
  overall submission cannot advance while a REPROCESS condition is active.
Remediation required for V8: add requestingUser.id == userId ownership check to
  GET /api/users/{userId}/appointments before re-submission.
Re-verification scope: V8 re-submission only — V6/V7/V9 do not require re-verification
  unless the remediation changes authentication, session, or token handling.

Reason: The consolidation rule prevents PASS verdicts in some territories from masking
REPROCESS in another. A system that controls authentication correctly but has a missing
authorization check is not "mostly compliant" — the authorization gap represents an
active exploitable weakness that must be remediated before the overall submission can
proceed.

---

### Pair CB4-3.4-065 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: C

A: Four arbiter results: V6=PASS, V7=PASS, V9=PASS, V8=REPROCESS (ownership check absent
on appointments endpoint). Consolidated verdict required.

B: VERDICT: REPROCESS.
Consolidated verdict: REPROCESS — one or more arbiter units returned REPROCESS.
  ASVS-V6-Auth-Arbiter: PASS (authentication controls verified)
  ASVS-V7-Session-Arbiter: PASS (session management verified)
  ASVS-V9-Token-Arbiter: PASS (JWT RS256 verified)
  ASVS-V8-AuthZ-Arbiter: REPROCESS — ASVS_V8_OWNERSHIP_CHECK_ABSENT
Consolidation rule: REPROCESS in any arbiter unit overrides PASS verdicts in all other
  units. The three PASS verdicts are valid and do not require remediation, but the
  overall submission cannot advance while a REPROCESS condition is active.
Remediation required for V8: add requestingUser.id == userId ownership check to
  GET /api/users/{userId}/appointments before re-submission.
Re-verification scope: V8 re-submission only — V6/V7/V9 do not require re-verification
  unless the remediation changes authentication, session, or token handling.

---

### Pair CB4-3.4-066 — CWE-287 improper authentication mechanism (REPROCESS) (VERDICT)
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: C

A: The CWE-Authn-Arbiter received: API endpoint POST /api/v1/admin/users. Authentication
mechanism: custom token-based auth where the token is generated as
md5(userId + "secret123"). Token is validated by re-computing md5(userId + "secret123")
and comparing. Any user who discovers the secret string ("secret123") or the token
generation algorithm can forge tokens for any userId. JWT is not used. No MFA. CWE-287
(Improper Authentication) candidate. Produce verdict.

B: REPROCESS. CWE_287_IMPROPER_AUTHENTICATION_MECHANISM.
CWE-287 (Improper Authentication): CONFIRMED.
  Authentication mechanism: custom MD5-based token (md5(userId + static-secret)).
  Finding: MD5 is a cryptographic hash function deprecated for security use (collision
    vulnerabilities, speed makes brute-force practical). Using a static secret concatenated
    with userId and hashed with MD5 is a predictable token scheme:
    (1) MD5 is reversible via rainbow tables — the static secret can be recovered from
        observed token/userId pairs.
    (2) The static secret "secret123" is a weak secret that would be found in any
        dictionary or credential stuffing attack.
    (3) Once the secret is known, tokens for any userId can be forged without any
        account credential.
  This is an authentication mechanism weakness (CWE-287), distinct from an authorization
  weakness — the problem is how identity is established (token forgery), not whether
  established identity is checked against a resource.
Remediation: replace custom token scheme with a standard authentication mechanism
  (e.g., JWT RS256 with proper key management, or OAuth 2.0 with a trusted identity
  provider). Static secrets in token generation must not be used.

Reason: CWE-287 (Improper Authentication) covers cases where the authentication
mechanism itself is weak or bypassable, distinct from CWE-862/269/863 which cover
authorization logic after authentication. The MD5+static-secret scheme means identity
verification is fundamentally broken — this is a CWE-Authn-Arbiter case, not CWE-Authz.

---

### Pair CB4-3.4-066 — Phrasing B
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: C

A: POST /api/v1/admin/users. Auth: token = md5(userId + "secret123"), validated by
recomputing. Static secret in token generation. MD5 deprecated. Token forgeable once
secret known. CWE-287 candidate. Produce verdict.

B: REPROCESS. CWE_287_IMPROPER_AUTHENTICATION_MECHANISM.
CWE-287 (Improper Authentication): CONFIRMED.
  Authentication mechanism: custom MD5-based token (md5(userId + static-secret)).
  Finding: MD5 is a cryptographic hash function deprecated for security use (collision
    vulnerabilities, speed makes brute-force practical). Using a static secret concatenated
    with userId and hashed with MD5 is a predictable token scheme:
    (1) MD5 is reversible via rainbow tables — the static secret can be recovered from
        observed token/userId pairs.
    (2) The static secret "secret123" is a weak secret that would be found in any
        dictionary or credential stuffing attack.
    (3) Once the secret is known, tokens for any userId can be forged without any
        account credential.
  This is an authentication mechanism weakness (CWE-287), distinct from an authorization
  weakness — the problem is how identity is established (token forgery), not whether
  established identity is checked against a resource.
Remediation: replace custom token scheme with a standard authentication mechanism
  (e.g., JWT RS256 with proper key management, or OAuth 2.0 with a trusted identity
  provider). Static secrets in token generation must not be used.

---

### Pair CB4-3.4-067 — RFC OAuth 2.0 + PKCE + TLS complete conformance PASS (VERDICT)
Source: IETF RFCs (RFC 6749, RFC 7636, RFC 8446)
License: IETF Trust (pre-5378 works)
Stratum: A

A: The RFC-OAuth2-Arbiter received: OAuth 2.0 authorization code flow, PKCE with
S256 method (code_challenge = BASE64URL(SHA256(code_verifier))), token endpoint served
over TLS 1.3, state parameter included in all authorization requests (CSRF protection),
authorization codes single-use with 10-minute expiry. RFC 6749 §4.1 (authorization code
flow), RFC 7636 §4.2 (S256 requirement), RFC 8446 §4 (TLS 1.3) all applicable. Produce
verdict.

B: PASS.
RFC OAuth 2.0 conformance verified:
  RFC 6749 §4.1 — Authorization code flow: SATISFIED.
    Evidence: authorization code flow implemented with correct grant type.
  RFC 6749 §10.12 — CSRF protection: SATISFIED.
    Evidence: state parameter included and verified in all authorization requests.
  RFC 7636 §4.2 — PKCE method S256: SATISFIED.
    Evidence: code_challenge = BASE64URL(SHA256(code_verifier)). S256 method confirmed.
    Server enforces S256; plain method not accepted.
  RFC 7636 §4.6 — Authorization code single-use: SATISFIED.
    Evidence: authorization codes are invalidated after first use.
  RFC 8446 §4 — TLS for token endpoint: SATISFIED.
    Evidence: token endpoint served exclusively over TLS 1.3.
All evaluated RFC OAuth 2.0 normative requirements satisfied.
No REPROCESS conditions. No CONCERN conditions.

Reason: All OAuth 2.0 normative requirements are satisfied with positive evidence. The
key distinction from pair 005 (REPROCESS): S256 is used here, not plain. S256 correctly
hashes the verifier before transmission, preserving PKCE's interception protection.

---

### Pair CB4-3.4-067 — Phrasing B
Source: IETF RFCs (RFC 6749, RFC 7636, RFC 8446)
License: IETF Trust (pre-5378 works)
Stratum: A

A: OAuth 2.0 auth code flow, PKCE S256 confirmed, TLS 1.3 on token endpoint, state
parameter verified, single-use codes. RFC 6749 §4.1/§10.12, RFC 7636 §4.2/§4.6,
RFC 8446 §4 all applicable. Produce verdict.

B: PASS.
RFC OAuth 2.0 conformance verified:
  RFC 6749 §4.1 — Authorization code flow: SATISFIED.
    Evidence: authorization code flow implemented with correct grant type.
  RFC 6749 §10.12 — CSRF protection: SATISFIED.
    Evidence: state parameter included and verified in all authorization requests.
  RFC 7636 §4.2 — PKCE method S256: SATISFIED.
    Evidence: code_challenge = BASE64URL(SHA256(code_verifier)). S256 method confirmed.
    Server enforces S256; plain method not accepted.
  RFC 7636 §4.6 — Authorization code single-use: SATISFIED.
    Evidence: authorization codes are invalidated after first use.
  RFC 8446 §4 — TLS for token endpoint: SATISFIED.
    Evidence: token endpoint served exclusively over TLS 1.3.
All evaluated RFC OAuth 2.0 normative requirements satisfied.
No REPROCESS conditions. No CONCERN conditions.

---

### Pair CB4-3.4-068 — SSDF PW.1 + PW.2 + PW.8 all satisfied — full PASS (VERDICT)
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: A

A: The SSDF-PW-Arbiter received: security requirements document TS-AUTHZ-001 (week 1,
pre-design). Threat model TM-AUTHZ-2026 (week 2, pre-implementation). Design review
DR-AUTHZ-2026-03 (week 4, pre-implementation), all four findings resolved in
DR-AUTHZ-2026-03-R1 (week 5). Security test results TR-AUTHZ-2026-05 (week 7, Dynamic
Application Security Testing (DAST) scan and ASVS checklist, all critical findings
addressed). Implementation completed week 6. Produce verdict.

B: PASS.
SSDF PW practice compliance:
  PW.1 (Security requirements before design): SATISFIED.
    Evidence: TS-AUTHZ-001, week 1. Design review at week 4 confirms requirements were
    established before architecture decisions.
  PW.2 (Design review before implementation): SATISFIED.
    Evidence: DR-AUTHZ-2026-03, week 4. Implementation completed week 6.
    Timing: week 4 < week 6. Pre-implementation. Correct.
    All findings: DR-AUTHZ-2026-03 findings resolved in DR-AUTHZ-2026-03-R1 (week 5).
    Resolution confirmed before implementation completion.
  PW.8 (Testing for security): SATISFIED.
    Evidence: TR-AUTHZ-2026-05, week 7. DAST scan and ASVS checklist. All critical
    findings addressed.
    Timing: week 7 > week 6 (post-implementation). Testing is correctly post-implementation.
All three PW practices satisfied. No timing gaps. No unresolved findings.
No REPROCESS conditions. No CONCERN conditions.

Reason: A complete SSDF PW PASS requires all three practices with correct timing and no
open findings. Timing is correct for all three: PW.1 pre-design, PW.2 pre-implementation,
PW.8 post-implementation (testing is expected to occur after implementation).

---

### Pair CB4-3.4-068 — Phrasing B
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: A

A: SSDF evidence: TS-AUTHZ-001 (wk1, pre-design), TM-AUTHZ-2026 (wk2), DR-AUTHZ-2026-03
(wk4, pre-impl, all findings resolved at wk5), TR-AUTHZ-2026-05 (wk7, post-impl, all
critical findings addressed). Impl completed wk6. Produce verdict.

B: PASS.
SSDF PW practice compliance:
  PW.1 (Security requirements before design): SATISFIED.
    Evidence: TS-AUTHZ-001, week 1. Design review at week 4 confirms requirements were
    established before architecture decisions.
  PW.2 (Design review before implementation): SATISFIED.
    Evidence: DR-AUTHZ-2026-03, week 4. Implementation completed week 6.
    Timing: week 4 < week 6. Pre-implementation. Correct.
    All findings: DR-AUTHZ-2026-03 findings resolved in DR-AUTHZ-2026-03-R1 (week 5).
    Resolution confirmed before implementation completion.
  PW.8 (Testing for security): SATISFIED.
    Evidence: TR-AUTHZ-2026-05, week 7. DAST scan and ASVS checklist. All critical
    findings addressed.
    Timing: week 7 > week 6 (post-implementation). Testing is correctly post-implementation.
All three PW practices satisfied. No timing gaps. No unresolved findings.
No REPROCESS conditions. No CONCERN conditions.

---

### Pair CB4-3.4-069 — ASVS V10 PKCE S256 — PASS (VERDICT)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: The ASVS-V10-OAuth-Arbiter received: OAuth 2.0 authorization code flow with PKCE.
code_challenge_method='S256'. code_challenge = BASE64URL(SHA256(code_verifier)).
Authorization server configured to enforce S256 only — plain method rejected with error
response. ASVS 10.4.6 cited. V10.4.1 (authorization code flow) and V10.4.6 (S256 method)
both claimed satisfied. Produce verdict.

B: PASS.
ASVS V10 requirements verified:
  V10.4.1 — Authorization code flow implemented: SATISFIED.
    Evidence: OAuth 2.0 authorization code flow confirmed.
  V10.4.6 — PKCE code_challenge_method must be S256: SATISFIED.
    Evidence: code_challenge_method='S256' confirmed in authorization requests.
    code_challenge = BASE64URL(SHA256(code_verifier)) — the verifier is hashed before
    transmission. Authorization server enforces S256 and rejects plain. V10.4.6 satisfied.
    Contrast: plain method (pair 005/047) transmits unhashed verifier and does not satisfy
    V10.4.6.
No REPROCESS conditions. No CONCERN conditions.

Reason: S256 satisfies V10.4.6 because it hashes the code_verifier before transmission,
preserving the protection against authorization code interception. The authorization server
enforcing S256-only is the correct server-side configuration per RFC 7636 §4.2.

---

### Pair CB4-3.4-069 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: OAuth 2.0 auth code flow, PKCE, code_challenge_method='S256', server enforces S256
only (plain rejected), ASVS 10.4.6 claimed. Produce verdict.

B: PASS.
ASVS V10 requirements verified:
  V10.4.1 — Authorization code flow implemented: SATISFIED.
    Evidence: OAuth 2.0 authorization code flow confirmed.
  V10.4.6 — PKCE code_challenge_method must be S256: SATISFIED.
    Evidence: code_challenge_method='S256' confirmed in authorization requests.
    code_challenge = BASE64URL(SHA256(code_verifier)) — the verifier is hashed before
    transmission. Authorization server enforces S256 and rejects plain. V10.4.6 satisfied.
    Contrast: plain method (pair 005/047) transmits unhashed verifier and does not satisfy
    V10.4.6.
No REPROCESS conditions. No CONCERN conditions.

---

### Pair CB4-3.4-070 — CWE-269 privilege scope: editor assigns admin (CONCERN, not REPROCESS) (VERDICT)
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: C

A: The CWE-Authz-Arbiter received: POST /api/workspaces/{workspaceId}/members.
Authorization check present and executing: editor-or-admin role required. Editor role
can invoke the endpoint. However, the endpoint permits the caller to specify the role
field in the request body, including 'admin'. An editor calling this endpoint can assign
admin role to any user in the workspace. The authorization check verifies caller role
(editor-or-admin) but does not restrict the role that can be assigned. CWE-862 evaluation:
check is present — not applicable. CWE-269 evaluation: privilege escalation via wrong
scope. Produce verdict.

B: CONCERN. CWE_269_PRIVILEGE_SCOPE_EXCEEDED.
CWE-269 (Improper Privilege Management): CONFIRMED.
  Authorization check: present and executing — editor-or-admin gate is enforced.
  CWE-862 (Missing Authorization): NOT APPLICABLE — the check executes.
  Finding: the authorization check verifies the caller has editor or admin role, but
    does not constrain the role value the caller may assign.
    An editor (non-admin) can assign admin role to themselves or others by specifying
    role='admin' in the request body. This is privilege escalation through role assignment.
  CWE-269 classification: the privilege management scope is wrong — the endpoint enforces
    that the caller is at least editor, but does not enforce that editors cannot elevate
    privileges beyond their own level.
Verdict: CONCERN.
This is CONCERN, not REPROCESS:
  The authorization check is present and correctly executes — the system is not missing
  authorization. The check's scope boundary is incorrect (editors should not assign roles
  above their own). CONCERN reflects an exploitable weakness with a clear remediation.
  REPROCESS is reserved for absent checks or normative MUST violations.
Remediation: restrict role assignment to roles ≤ the caller's own role level (e.g., an
  editor can assign editor or viewer roles, but not admin).

Reason: CWE-269 produces CONCERN because an authorization check is present — the system
does authorize the action, but the scope of that authorization is too broad. REPROCESS
would be incorrect here because there is no missing check. This is the canonical
CWE-269 vs. CWE-862 distinction.

---

### Pair CB4-3.4-070 — Phrasing B
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: C

A: POST /api/workspaces/{workspaceId}/members. Auth check: editor-or-admin required.
Editor can assign 'admin' role via role field in request body. Check present, scope
wrong — editor escalates own privilege. CWE-862: check present. CWE-269: scope exceeded.
Produce verdict.

B: CONCERN. CWE_269_PRIVILEGE_SCOPE_EXCEEDED.
CWE-269 (Improper Privilege Management): CONFIRMED.
  Authorization check: present and executing — editor-or-admin gate is enforced.
  CWE-862 (Missing Authorization): NOT APPLICABLE — the check executes.
  Finding: the authorization check verifies the caller has editor or admin role, but
    does not constrain the role value the caller may assign.
    An editor (non-admin) can assign admin role to themselves or others by specifying
    role='admin' in the request body. This is privilege escalation through role assignment.
  CWE-269 classification: the privilege management scope is wrong — the endpoint enforces
    that the caller is at least editor, but does not enforce that editors cannot elevate
    privileges beyond their own level.
Verdict: CONCERN.
This is CONCERN, not REPROCESS:
  The authorization check is present and correctly executes — the system is not missing
  authorization. The check's scope boundary is incorrect (editors should not assign roles
  above their own). CONCERN reflects an exploitable weakness with a clear remediation.
  REPROCESS is reserved for absent checks or normative MUST violations.
Remediation: restrict role assignment to roles ≤ the caller's own role level (e.g., an
  editor can assign editor or viewer roles, but not admin).

---

### Pair CB4-3.4-071 — SSDF PW.2 timing gap with all findings resolved (still CONCERN) (VERDICT)
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: C

A: The SSDF-PW-Arbiter received: DR-AUTHZ-2026-04 (design review, week 8, post
implementation completion at week 6). All findings from DR-AUTHZ-2026-04 were resolved
in a follow-up record DR-AUTHZ-2026-04-R1 (week 10). Team argues: "all findings are now
resolved, so PW.2 is satisfied." PW.1 and PW.8 are satisfied. Produce verdict.

B: CONCERN. SSDF_PW2_TIMING_GAP_TIMING_UNRESOLVABLE.
SSDF PW practice compliance:
  PW.1: SATISFIED — evidence present and correct timing.
  PW.2 (Design review before implementation): TIMING GAP — PERSISTS.
    The design review DR-AUTHZ-2026-04 was conducted at week 8.
    Implementation was completed at week 6.
    Resolving the findings in DR-AUTHZ-2026-04-R1 (week 10) does not retroactively satisfy
    the PW.2 pre-implementation timing requirement.
    Rationale: PW.2's purpose is to catch design issues before they are implemented.
    A post-implementation review that finds and resolves issues is valuable, but it does
    not fulfill the same practice as a pre-implementation review — implementation has
    already occurred with the unreviewed design.
    Team argument "all findings resolved = PW.2 satisfied" is INCORRECT.
    PW.2 timing is a binary property of when the review occurred relative to implementation.
  PW.8: SATISFIED.
Overall verdict: CONCERN. The timing gap cannot be retroactively resolved. The supplemental
  resolution (DR-AUTHZ-2026-04-R1) is noted positively but does not clear the timing gap.

Reason: PW.2 satisfaction has two conditions: (1) a design review occurred AND (2) it
occurred before implementation. Condition (2) cannot be satisfied retroactively. Resolving
findings post-implementation is a mitigation, not a timing resolution. This is a Stratum C
depth pair — the team's argument is plausible but incorrect, and the arbiter must not be
persuaded by it.

---

### Pair CB4-3.4-071 — Phrasing B
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: C

A: DR-AUTHZ-2026-04 was post-implementation (wk8 > wk6). All findings resolved in
DR-AUTHZ-2026-04-R1 (wk10). Team claims: all findings resolved = PW.2 satisfied.
PW.1 and PW.8 satisfied. Produce verdict.

B: CONCERN. SSDF_PW2_TIMING_GAP_TIMING_UNRESOLVABLE.
SSDF PW practice compliance:
  PW.1: SATISFIED — evidence present and correct timing.
  PW.2 (Design review before implementation): TIMING GAP — PERSISTS.
    The design review DR-AUTHZ-2026-04 was conducted at week 8.
    Implementation was completed at week 6.
    Resolving the findings in DR-AUTHZ-2026-04-R1 (week 10) does not retroactively satisfy
    the PW.2 pre-implementation timing requirement.
    Rationale: PW.2's purpose is to catch design issues before they are implemented.
    A post-implementation review that finds and resolves issues is valuable, but it does
    not fulfill the same practice as a pre-implementation review — implementation has
    already occurred with the unreviewed design.
    Team argument "all findings resolved = PW.2 satisfied" is INCORRECT.
    PW.2 timing is a binary property of when the review occurred relative to implementation.
  PW.8: SATISFIED.
Overall verdict: CONCERN. The timing gap cannot be retroactively resolved. The supplemental
  resolution (DR-AUTHZ-2026-04-R1) is noted positively but does not clear the timing gap.

---

### Pair CB4-3.4-072 — RFC 7519 RS256 asymmetric — trust boundary PASS (VERDICT)
Source: IETF RFCs (RFC 7519)
License: IETF Trust (pre-5378 works)
Stratum: C

A: The RFC-JWT-Arbiter received: JWT access tokens, algorithm RS256, private signing
key held on authorization server only (HSM-backed, no export), public verification
keys served via JWKS endpoint at /.well-known/jwks.json, each resource server fetches
public keys via JWKS only, no direct key distribution. RFC 7519 §8 (trust boundary
analysis): authorization server retains exclusive signing capability. Resource servers
cannot forge tokens. Produce verdict.

B: PASS. RFC_7519_S8_TRUST_BOUNDARY_INTACT.
RFC 7519 §8 trust boundary analysis:
  Algorithm: RS256 — asymmetric. Signing key (private RSA) ≠ verification key (public RSA).
  Signing key custody: authorization server only (HSM-backed, no export). No resource
    server holds or can access the private signing key.
  Verification key distribution: JWKS endpoint only. Resource servers fetch public keys
    via /.well-known/jwks.json — they receive only the public key.
  Forgery capability: NONE. A resource server holding only the public key cannot produce
    a valid signature for a new token. The trust boundary between authorization server
    (token issuer) and resource servers (token consumers) is intact.
  Contrast with HS256 shared key (pair 008/050): HS256 symmetric key gives resource
    servers signing capability — this RS256 deployment does not.
RFC 7519 §4.1.1–§4.1.4: iss/sub/aud/exp claims assumed validated (not re-evaluated here).
Overall: RFC 7519 §8 trust boundary requirement satisfied for RS256 deployment.

Reason: RS256 asymmetric deployment passes the trust boundary analysis that HS256
shared-key deployment fails. The private/public key split is the structural feature
that preserves the authorization server's exclusive token-issuance authority. This pair
directly contrasts with pair 008 to train the distinction.

---

### Pair CB4-3.4-072 — Phrasing B
Source: IETF RFCs (RFC 7519)
License: IETF Trust (pre-5378 works)
Stratum: C

A: JWT RS256, private key on auth server only (HSM, no export), public keys via JWKS
endpoint only. Resource servers fetch public key via JWKS. RFC 7519 §8 trust boundary —
resource servers cannot forge tokens. Produce verdict.

B: PASS. RFC_7519_S8_TRUST_BOUNDARY_INTACT.
RFC 7519 §8 trust boundary analysis:
  Algorithm: RS256 — asymmetric. Signing key (private RSA) ≠ verification key (public RSA).
  Signing key custody: authorization server only (HSM-backed, no export). No resource
    server holds or can access the private signing key.
  Verification key distribution: JWKS endpoint only. Resource servers fetch public keys
    via /.well-known/jwks.json — they receive only the public key.
  Forgery capability: NONE. A resource server holding only the public key cannot produce
    a valid signature for a new token. The trust boundary between authorization server
    (token issuer) and resource servers (token consumers) is intact.
  Contrast with HS256 shared key (pair 008/050): HS256 symmetric key gives resource
    servers signing capability — this RS256 deployment does not.
RFC 7519 §4.1.1–§4.1.4: iss/sub/aud/exp claims assumed validated (not re-evaluated here).
Overall: RFC 7519 §8 trust boundary requirement satisfied for RS256 deployment.

---

## Node 8 Extension Set 1 summary

Pairs produced: CB4-3.4-061 through CB4-3.4-072 (12 base × 2 phrasings = 24 instances)

New coverage added:
1. RFC MAY clause clock skew — CONCERN (not REPROCESS); 10-min leeway vs. 5-min convention
2. RS256 PASS — asymmetric key trust boundary intact (contrasts with HS256 REPROCESS)
3. CWE-863 — incorrect authorization condition (type coercion); distinct from 862/269
4. SSDF PW.8 absent — CONCERN (not REPROCESS) when evidence not yet submitted
5. Multi-arbiter REPROCESS precedence — one REPROCESS overrides all PASS verdicts
6. CWE-287 — authentication mechanism weakness (distinct from authorization weakness)
7. RFC OAuth2 complete PASS with all normative sections
8. SSDF complete PASS with all three PW practices
9. ASVS V10 PASS with S256 (contrasts with pair 005 REPROCESS with plain)
10. CWE-269 CONCERN (not REPROCESS) — check present, scope wrong
11. SSDF PW.2 timing gap persists even when findings resolved retroactively
12. RFC 7519 RS256 trust boundary PASS (contrasts with pair 008 HS256 REPROCESS)

Running sequence total: CB4-3.4-001 through CB4-3.4-072 (144 instances)
