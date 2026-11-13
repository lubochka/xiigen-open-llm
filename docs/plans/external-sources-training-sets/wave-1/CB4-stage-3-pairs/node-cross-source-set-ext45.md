# Node Cross-Source Set Extension 45
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1365–1374 (20 instances — 10 base pairs × 2 phrasings)
# Sources: OIDC Back-Channel Logout §2.4, RFC 9126 §2.3,
#          OIDC Core §2, RFC 9700 §4.3.2,
#          RFC 6749, OIDC Core §5.3,
#          RFC 9126 §2.2, OIDC RP-Initiated Logout §3,
#          RFC 8414 §2, NIST SSDF PO.1
# Running total after this file: 2748 instances (1374 base pairs)
# Formula check: 120 + (1374 − 60) × 2 = 120 + 2628 = 2748 ✓

---

## Pair[CB4-3.4-1365]/A

An authorization server issues refresh tokens without including
`refresh_token_expires_in` in token responses. The same AS's UserInfo endpoint
does not support CORS headers, requiring SPA clients to use a backend proxy.
Both gaps are noted in a developer integration review. The reviewer argues that
two developer-experience deficiencies together constitute a protocol conformance
failure requiring REPROCESS.

## Pair[CB4-3.4-1365]/B

CONCERN. Two guidance-level observations: (1) refresh_token_expires_in absent —
RFC 6749 §5.1 does not include this field as a required or recommended response
field; no unconditional MUST mandates it; (2) UserInfo endpoint CORS absent —
OIDC Core §5.3 guidance SHOULD; BFF proxy is a valid architectural alternative;
no unconditional MUST. The graduation rule applies: N CONCERNs = CONCERN regardless
of count, developer-experience framing, or conformance-failure arguments. Neither
finding has a MUST-level anchor. CONCERN.

## Pair[CB4-3.4-1365]/Reason

Two guidance-level gaps: refresh_token_expires_in absent (RFC 6749 §5.1 not
required) + UserInfo CORS absent (OIDC Core §5.3 SHOULD); graduation rule →
CONCERN.

## Pair[CB4-3.4-1365]/Phrasing-B

Verdict CONCERN. Both findings are guidance-level. The graduation rule forecloses
developer-experience escalation arguments. CONCERN.

---

## Pair[CB4-3.4-1366]/A

An authorization server's PAR `request_uri` values have a 3600-second expiry.
The same AS's RP-Initiated Logout endpoint accepts `id_token_hint` values from
previously terminated sessions without checking session binding. Both gaps are
identified in a security review. The reviewer argues that two lifecycle control
deficiencies in push-based and logout-based flows constitute a systematic weakness
requiring REPROCESS.

## Pair[CB4-3.4-1366]/B

CONCERN. Two guidance-level observations: (1) PAR expires_in=3600 — RFC 9126
§2.2 SHOULD short; no numeric MUST ceiling; AS team's usability justification
accepted as an operational trade-off; (2) id_token_hint session binding absent —
OIDC RP-Initiated Logout §3 SHOULD; no unconditional MUST mandates strict session
binding. The graduation rule is categorical: N CONCERNs = CONCERN regardless of
thematic clustering, systematic-weakness framing, or count. No MUST-level violation
is identified. CONCERN.

## Pair[CB4-3.4-1366]/Reason

Two SHOULD-level gaps: PAR expiry too long (RFC 9126 §2.2 SHOULD) + id_token_hint
session binding absent (OIDC RP-Initiated Logout §3 SHOULD); graduation rule →
CONCERN.

## Pair[CB4-3.4-1366]/Phrasing-B

Verdict CONCERN. Both findings are SHOULD-level guidance gaps. The graduation rule
forecloses systematic-weakness escalation. CONCERN.

---

## Pair[CB4-3.4-1367]/A

An OIDC authorization server's RP-Initiated Logout endpoint does not validate
`id_token_hint` against the current session. The same AS's discovery metadata
omits the `grant_types_supported` field. Both gaps are noted in a protocol
conformance audit. The auditor argues that two governance and lifecycle control
gaps — one in logout validation and one in metadata completeness — together
constitute a conformance-level deficiency requiring REPROCESS.

## Pair[CB4-3.4-1367]/B

CONCERN. Two guidance-level observations: (1) id_token_hint session binding absent
— OIDC RP-Initiated Logout §3 SHOULD; no unconditional MUST; logout CSRF risk at
guidance level; (2) grant_types_supported absent — RFC 8414 §2 RECOMMENDED / SHOULD;
defined RFC fallback assumption when absent; no MUST. The graduation rule is
categorical: N CONCERNs = CONCERN regardless of domain category, governance framing,
conformance-level language, or count. Neither finding has a MUST-level anchor.
CONCERN.

## Pair[CB4-3.4-1367]/Reason

Two SHOULD-level gaps: id_token_hint session binding (OIDC RP-Initiated Logout §3
SHOULD) + grant_types_supported absent (RFC 8414 §2 SHOULD); graduation rule:
N CONCERNs ≠ REPROCESS → CONCERN.

## Pair[CB4-3.4-1367]/Phrasing-B

Verdict CONCERN. Both findings are SHOULD-level. The graduation rule forecloses
conformance-level escalation. CONCERN.

---

## Pair[CB4-3.4-1368]/A

An OIDC provider has no formal security requirements documentation (NIST SSDF
PO.1 gap). A concurrent security test confirms that the AS's authorization
endpoint accepts `response_type=token` from web application clients, issuing
access tokens directly in the authorization response fragment — the implicit
grant flow is active in production. The team classifies PO.1 as an administrative
gap and the implicit flow as a legacy-compatibility item on the product roadmap.

## Pair[CB4-3.4-1368]/B

REPROCESS. PO.1 absence alone is CONCERN. The PO.1 escalation rule applies:
PO.1 absence combined with a traceable production MUST NOT violation escalates
to REPROCESS. RFC 9700 §4.3.2 MUST NOT: authorization servers must not issue
access tokens via the implicit grant. The AS actively issues access tokens via
implicit flow in production — a live MUST NOT violation. The PO.1 gap means the
implicit flow prohibition was never formally tracked as a required control.
The roadmap argument is rejected: the PO.1 escalation rule mandates REPROCESS
when PO.1 absence is traceable to an active production MUST NOT violation.
REPROCESS.

## Pair[CB4-3.4-1368]/Reason

PO.1 gap (CONCERN alone) + active MUST NOT violation (RFC 9700 §4.3.2 MUST NOT:
implicit flow prohibited; AS issues AT via implicit flow in production) = PO.1
escalation → REPROCESS.

## Pair[CB4-3.4-1368]/Phrasing-B

Verdict REPROCESS. PO.1 escalation: PO.1 absence + traceable active MUST NOT
violation = REPROCESS. RFC 9700 §4.3.2 MUST NOT is violated: implicit flow active
in production. REPROCESS.

---

## Pair[CB4-3.4-1369]/A

An authorization server issues refresh tokens without including
`refresh_token_expires_in` in token responses. The same AS's back-channel logout
tokens are missing the `events` claim. Both gaps are noted in a review. An engineer
asks whether the missing events claim constitutes the same severity as the missing
refresh token expiry field.

## Pair[CB4-3.4-1369]/B

REPROCESS. The refresh_token_expires_in gap is CONCERN-level (RFC 6749 §5.1 does
not require this field; no MUST). The missing events claim is REPROCESS-level:
OIDC Back-Channel Logout §2.4 MUST — logout tokens must include the events claim
with the backchannel-logout URI member; its absence means logout tokens are
structurally non-conformant and can be confused with other token types. REPROCESS
overrides CONCERN — the engineer's question is answered: the events claim absence
is a MUST violation; the refresh token expiry field absence is guidance-level.
Overall verdict: REPROCESS. REPROCESS.

## Pair[CB4-3.4-1369]/Reason

refresh_token_expires_in absent (guidance-level, CONCERN) + logout token events
claim absent (OIDC Back-Channel Logout §2.4 MUST, REPROCESS); REPROCESS overrides
CONCERN → REPROCESS.

## Pair[CB4-3.4-1369]/Phrasing-B

Verdict REPROCESS. Absent refresh_token_expires_in is guidance-level (CONCERN).
Absent events claim in logout token violates OIDC Back-Channel Logout §2.4 MUST.
REPROCESS overrides CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1370]/A

An OIDC provider's UserInfo endpoint does not support CORS headers. The same AS
does not check `client_id` binding when a PAR `request_uri` is used at the
authorization endpoint — any registered client can use any valid request_uri.
Both gaps are noted. The team argues that CORS is a browser concern and the PAR
binding is an optional security layer.

## Pair[CB4-3.4-1370]/B

REPROCESS. The UserInfo CORS gap is CONCERN-level (OIDC Core §5.3 guidance SHOULD;
BFF proxy is a valid alternative; no MUST). The PAR client binding gap is REPROCESS-
level: RFC 9126 §2.3 MUST — the authorization endpoint must reject if the client_id
in the authorization request does not match the client that submitted the PAR push;
allowing any client to use any request_uri enables client substitution attacks.
REPROCESS overrides CONCERN — the team's "optional security layer" framing for PAR
client binding is rejected: it is a MUST. REPROCESS.

## Pair[CB4-3.4-1370]/Reason

UserInfo CORS absent (OIDC Core §5.3 SHOULD, CONCERN) + PAR client binding absent
(RFC 9126 §2.3 MUST, REPROCESS); REPROCESS overrides CONCERN → REPROCESS.

## Pair[CB4-3.4-1370]/Phrasing-B

Verdict REPROCESS. UserInfo CORS gap is guidance-level (CONCERN). PAR request_uri
client binding absence violates RFC 9126 §2.3 MUST. REPROCESS overrides CONCERN.
REPROCESS.

---

## Pair[CB4-3.4-1371]/A

An authorization server's PAR `request_uri` values have a 3600-second expiry
window. The same AS issues ID tokens where the `azp` claim is set to a third-
party analytics service identifier rather than the requesting client's `client_id`.
Both gaps are identified in a protocol review. The team asks whether the expiry
guidance gap and the azp setting combine into a single verdict.

## Pair[CB4-3.4-1371]/B

REPROCESS. The PAR expiry gap is CONCERN-level (RFC 9126 §2.2 SHOULD short;
no numeric MUST ceiling; operational trade-off acknowledged). The azp mismatch
is REPROCESS-level: OIDC Core §2 MUST — when the azp claim is present, it must
contain the OAuth 2.0 Client ID of the party to whom the ID token was issued
(the requesting client); setting azp to a third-party service misrepresents the
authorized party and causes azp validation to fail at the RP. REPROCESS overrides
CONCERN. The team is correct that they combine into a single verdict: REPROCESS
overrides the guidance-level PAR gap. REPROCESS.

## Pair[CB4-3.4-1371]/Reason

PAR expiry too long (RFC 9126 §2.2 SHOULD, CONCERN) + azp mismatch in ID token
(OIDC Core §2 MUST, REPROCESS); REPROCESS overrides CONCERN → REPROCESS.

## Pair[CB4-3.4-1371]/Phrasing-B

Verdict REPROCESS. PAR expiry length is guidance-level (CONCERN). azp mismatch
violates OIDC Core §2 MUST. REPROCESS overrides CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1372]/A

An OIDC provider's RP-Initiated Logout endpoint does not validate that the
`id_token_hint` corresponds to the currently active session. The same AS's
authorization endpoint continues to support `response_type=token` for web
application clients, issuing access tokens via the implicit grant. A compliance
officer asks whether one finding should be prioritized over the other for
immediate remediation.

## Pair[CB4-3.4-1372]/B

REPROCESS. The id_token_hint session binding gap is CONCERN-level (OIDC
RP-Initiated Logout §3 SHOULD; no unconditional MUST for strict session binding).
The implicit flow finding is REPROCESS-level: RFC 9700 §4.3.2 MUST NOT —
authorization servers must not issue access tokens via the implicit grant;
continuing to support response_type=token for web clients violates this MUST NOT.
REPROCESS overrides CONCERN — the compliance officer's question is answered: the
implicit flow violation requires immediate remediation; the id_token_hint binding
gap is guidance-level. Overall verdict: REPROCESS. REPROCESS.

## Pair[CB4-3.4-1372]/Reason

id_token_hint session binding absent (OIDC RP-Initiated Logout §3 SHOULD, CONCERN)
+ implicit flow enabled (RFC 9700 §4.3.2 MUST NOT, REPROCESS); REPROCESS overrides
CONCERN → REPROCESS.

## Pair[CB4-3.4-1372]/Phrasing-B

Verdict REPROCESS. id_token_hint session binding is guidance-level (CONCERN).
Implicit grant for web clients violates RFC 9700 §4.3.2 MUST NOT. REPROCESS
overrides CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1373]/A

An OIDC deployment is reviewed against all eight ext52 control points: back-channel
logout tokens include the `events` claim with the backchannel-logout URI per OIDC
Back-Channel Logout §2.4; the PAR authorization endpoint verifies that the
`client_id` in the authorization request matches the pushing client per RFC 9126
§2.3; the `azp` claim when present equals the requesting client's `client_id` per
OIDC Core §2; the authorization endpoint does not accept `response_type=token`
per RFC 9700 §4.3.2; token responses include `refresh_token_expires_in` as a
non-standard extension; the UserInfo endpoint supports CORS headers; PAR
`request_uri` values expire after 90 seconds; and `id_token_hint` at logout is
validated against the active session. No anomalies are detected.

## Pair[CB4-3.4-1373]/B

PASS. All eight ext52 control points are satisfied: (1) back-channel logout token
events claim per OIDC Back-Channel Logout §2.4 MUST; (2) PAR client_id binding
check per RFC 9126 §2.3 MUST; (3) azp equals requesting client_id per OIDC Core
§2 MUST (when azp is present); (4) implicit grant disabled per RFC 9700 §4.3.2
MUST NOT; (5) refresh_token_expires_in included as extension (exceeds RFC 6749
§5.1 guidance floor); (6) UserInfo CORS support per OIDC Core §5.3 SHOULD
(exceeds SHOULD floor); (7) PAR expires_in=90s per RFC 9126 §2.2 SHOULD short
(exceeds guidance floor); (8) id_token_hint session binding validated per OIDC
RP-Initiated Logout §3 SHOULD (exceeds SHOULD floor). No violations or guidance
gaps found. PASS.

## Pair[CB4-3.4-1373]/Reason

All eight ext52 controls compliant: OIDC BCL §2.4 (events claim), RFC 9126 §2.3
(PAR client binding), OIDC Core §2 (azp), RFC 9700 §4.3.2 (no implicit), RFC 6749
§5.1 extension (RT expiry), OIDC Core §5.3 (CORS), RFC 9126 §2.2 (PAR short
expiry), OIDC RP-Initiated Logout §3 (hint binding) → PASS.

## Pair[CB4-3.4-1373]/Phrasing-B

Verdict PASS. All eight ext52 control points satisfied: logout events claim (OIDC
BCL §2.4), PAR client binding (RFC 9126 §2.3), azp correctness (OIDC Core §2),
no implicit flow (RFC 9700 §4.3.2), RT expiry disclosure, UserInfo CORS, PAR short
expiry, id_token_hint session binding. PASS.

---

## Pair[CB4-3.4-1374]/A

An OIDC authorization server has four concurrent guidance-level gaps: token
responses omit `refresh_token_expires_in`; the UserInfo endpoint does not support
CORS headers; PAR `request_uri` values have a 3600-second expiry; and the
RP-Initiated Logout endpoint does not validate `id_token_hint` against the active
session. A security board reviews all four gaps and concludes that the combination
of lifecycle disclosure, cross-origin access, reference lifetime, and logout
integrity deficiencies establishes a systemic governance failure that escalates
to REPROCESS.

## Pair[CB4-3.4-1374]/B

CONCERN. Four guidance-level observations: (1) refresh_token_expires_in absent —
RFC 6749 §5.1 not required; (2) UserInfo CORS absent — OIDC Core §5.3 SHOULD;
BFF proxy valid; no MUST; (3) PAR expiry=3600 — RFC 9126 §2.2 SHOULD short; no
numeric MUST ceiling; (4) id_token_hint session binding absent — OIDC RP-Initiated
Logout §3 SHOULD; no MUST. The graduation rule is categorical and count-independent:
N CONCERNs = CONCERN regardless of count, domain clustering, governance framing,
or security-board authority. Four SHOULD-level gaps equal one CONCERN. Only a
MUST-level violation triggers REPROCESS. The security board's escalation conclusion
is rejected by the graduation rule. CONCERN.

## Pair[CB4-3.4-1374]/Reason

Four SHOULD/guidance-level gaps: refresh_token_expires_in + UserInfo CORS +
PAR expiry + id_token_hint binding; graduation rule: N CONCERNs ≠ REPROCESS;
governance-failure argument rejected → CONCERN.

## Pair[CB4-3.4-1374]/Phrasing-B

Verdict CONCERN. All four findings are SHOULD-level guidance gaps. The graduation
rule is count-independent — four CONCERNs equal one CONCERN. Governance-failure
escalation arguments do not override the graduation rule. CONCERN.
