# Node Cross-Source Set Extension 47
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1417–1426 (20 instances — 10 base pairs × 2 phrasings)
# Sources: RFC 8414 §3, RFC 6749 §5.1,
#          RFC 9126 §2.2, OIDC Core §5.3.2,
#          OIDC Discovery §3, RFC 9101 §10.3,
#          OIDC Core §3.1.3.7.4, RFC 9700 §4.3.1,
#          NIST SSDF PO.1, OIDC RP-Initiated Logout §3
# Running total after this file: 2852 instances (1426 base pairs)
# Formula check: 120 + (1426 − 60) × 2 = 120 + 2732 = 2852 ✓

---

## Pair[CB4-3.4-1417]/A

An OIDC authorization server's discovery document omits the `userinfo_endpoint`
URL even though a UserInfo endpoint exists. The same AS's JAR processing accepts
request objects that do not include the `nbf` claim. Both gaps are noted in a
protocol review. A reviewer argues that two discoverability and request-object
binding gaps together indicate insufficient implementation completeness, warranting
REPROCESS.

## Pair[CB4-3.4-1417]/B

CONCERN. Two guidance-level observations: (1) userinfo_endpoint absent from
discovery — OIDC Discovery §3 RECOMMENDED; no unconditional MUST; hardcode URL
is a valid workaround; (2) JAR nbf absent — RFC 9101 §10.3 guidance SHOULD;
no unconditional MUST requires nbf in request objects; exp provides the forward
boundary. The graduation rule applies: N CONCERNs = CONCERN regardless of count,
implementation-completeness framing, or discoverability arguments. Neither finding
has a MUST-level anchor. CONCERN.

## Pair[CB4-3.4-1417]/Reason

Two guidance-level gaps: userinfo_endpoint absent (OIDC Discovery §3 RECOMMENDED)
+ JAR nbf absent (RFC 9101 §10.3 guidance SHOULD); graduation rule → CONCERN.

## Pair[CB4-3.4-1417]/Phrasing-B

Verdict CONCERN. Both findings are guidance-level. The graduation rule forecloses
completeness-based escalation. CONCERN.

---

## Pair[CB4-3.4-1418]/A

An OIDC relying party omits `azp` validation when processing multi-audience ID
tokens. The same AS does not enforce PKCE for public clients. Both gaps are
identified in a security assessment. The assessor argues that two authentication
context and authorization code protection gaps constitute a compounded security
weakness requiring REPROCESS.

## Pair[CB4-3.4-1418]/B

CONCERN. Two guidance-level observations: (1) azp not validated for multi-audience
tokens — OIDC Core §3.1.3.7.4 SHOULD; no unconditional MUST; aud membership
verification provides the MUST-level binding; (2) PKCE not required for public
clients — RFC 9700 §4.3.2 SHOULD; no unconditional MUST mandates PKCE enforcement;
redirect URI matching and short code TTL are acknowledged compensating controls.
The graduation rule is categorical: N CONCERNs = CONCERN regardless of security
impact framing, weakness-compounding language, or count. CONCERN.

## Pair[CB4-3.4-1418]/Reason

Two SHOULD-level gaps: azp not validated (OIDC Core §3.1.3.7.4 SHOULD) + PKCE
not required for public clients (RFC 9700 §4.3.2 SHOULD); graduation rule →
CONCERN.

## Pair[CB4-3.4-1418]/Phrasing-B

Verdict CONCERN. Both findings are SHOULD-level. The graduation rule forecloses
compounded-weakness escalation. CONCERN.

---

## Pair[CB4-3.4-1419]/A

An AS's JAR processing accepts request objects without the `nbf` claim. The same
AS's RP-Initiated Logout endpoint does not validate `id_token_hint` against the
active session. Both gaps are noted in an audit. The auditor argues that two time-
bounding and session-binding control gaps together require REPROCESS.

## Pair[CB4-3.4-1419]/B

CONCERN. Two guidance-level observations: (1) JAR nbf absent — RFC 9101 §10.3
guidance SHOULD; no MUST requires nbf in request objects; (2) id_token_hint
session binding absent — OIDC RP-Initiated Logout §3 SHOULD; no unconditional
MUST for strict session binding. The graduation rule is categorical: N CONCERNs
= CONCERN regardless of thematic overlap (both involve time/session binding),
auditor framing, or count. No MUST-level violation is identified. CONCERN.

## Pair[CB4-3.4-1419]/Reason

Two SHOULD-level gaps: JAR nbf absent (RFC 9101 §10.3 guidance) + id_token_hint
session binding absent (OIDC RP-Initiated Logout §3 SHOULD); graduation rule →
CONCERN.

## Pair[CB4-3.4-1419]/Phrasing-B

Verdict CONCERN. Both findings are SHOULD-level guidance gaps. The graduation rule
forecloses time/session-binding escalation. CONCERN.

---

## Pair[CB4-3.4-1420]/A

An OIDC provider has no formal security requirements documentation (NIST SSDF
PO.1 gap). A concurrent test confirms that the AS's UserInfo endpoint returns
responses with `Content-Type: text/html` — not `application/json`. Active
client integrations using strict HTTP content-type validation are failing in
production. The team classifies PO.1 as an administrative debt item and the
UserInfo Content-Type as a format issue for the next sprint.

## Pair[CB4-3.4-1420]/B

REPROCESS. PO.1 absence alone is CONCERN. The PO.1 escalation rule applies: PO.1
absence combined with a traceable production MUST violation escalates to REPROCESS.
OIDC Core §5.3.2 MUST: the UserInfo endpoint must return Content-Type:
application/json. The UserInfo endpoint is actively returning text/html in
production — a live MUST violation causing client failures. The PO.1 gap means
the UserInfo response format requirement was never formally tracked as a required
control. The sprint-backlog framing is rejected: the PO.1 escalation rule mandates
REPROCESS when PO.1 absence is traceable to an active production MUST violation.
REPROCESS.

## Pair[CB4-3.4-1420]/Reason

PO.1 gap (CONCERN alone) + active MUST violation (OIDC Core §5.3.2 MUST: UserInfo
must return Content-Type application/json; returns text/html in production) = PO.1
escalation → REPROCESS.

## Pair[CB4-3.4-1420]/Phrasing-B

Verdict REPROCESS. PO.1 escalation: PO.1 absence + traceable active MUST violation
= REPROCESS. OIDC Core §5.3.2 MUST is violated: UserInfo returns wrong Content-
Type in production. REPROCESS.

---

## Pair[CB4-3.4-1421]/A

An OIDC provider's discovery document omits `userinfo_endpoint`. The same AS's
discovery document has an `issuer` value that doesn't match the well-known URL
prefix from which the discovery document was served. Both gaps are noted in a
review. The team asks which gap requires more immediate attention.

## Pair[CB4-3.4-1421]/B

REPROCESS. The userinfo_endpoint absence is CONCERN-level (OIDC Discovery §3
RECOMMENDED; no MUST; hardcode workaround valid). The issuer mismatch is REPROCESS-
level: RFC 8414 §3 MUST — the issuer value must exactly match the well-known URL
prefix; a mismatch destroys the trust binding that allows relying parties to verify
that the entity serving the discovery document is the same entity that issued
the tokens, enabling discovery spoofing attacks. REPROCESS overrides CONCERN —
the team's question is answered: the issuer mismatch requires immediate attention;
the userinfo_endpoint absence is guidance-level. REPROCESS.

## Pair[CB4-3.4-1421]/Reason

userinfo_endpoint absent (OIDC Discovery §3 RECOMMENDED, CONCERN) + issuer mismatch
(RFC 8414 §3 MUST, REPROCESS); REPROCESS overrides CONCERN → REPROCESS.

## Pair[CB4-3.4-1421]/Phrasing-B

Verdict REPROCESS. userinfo_endpoint absence is guidance-level (CONCERN). Issuer
mismatch violates RFC 8414 §3 MUST. REPROCESS overrides CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1422]/A

An AS's JAR processing accepts request objects without the `nbf` claim. The same
AS's token endpoint responses omit the `token_type` field. Both gaps are noted.
A developer asks whether the request object gap and the token response gap are
the same severity level.

## Pair[CB4-3.4-1422]/B

REPROCESS. The JAR nbf gap is CONCERN-level (RFC 9101 §10.3 guidance SHOULD; no
MUST requires nbf). The missing token_type is REPROCESS-level: RFC 6749 §5.1
REQUIRED — token_type must be present in every access token response; its absence
prevents clients from determining the correct token transmission scheme; clients
following the specification correctly cannot process the response. REPROCESS
overrides CONCERN — the developer's question is answered: the token_type absence
is REQUIRED (MUST); the nbf gap is guidance-level. REPROCESS.

## Pair[CB4-3.4-1422]/Reason

JAR nbf absent (RFC 9101 §10.3 guidance SHOULD, CONCERN) + token_type absent
(RFC 6749 §5.1 REQUIRED, REPROCESS); REPROCESS overrides CONCERN → REPROCESS.

## Pair[CB4-3.4-1422]/Phrasing-B

Verdict REPROCESS. JAR nbf gap is guidance-level (CONCERN). Absent token_type
violates RFC 6749 §5.1 REQUIRED. REPROCESS overrides CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1423]/A

An OIDC relying party omits `azp` validation when aud is multi-valued. The same
AS's PAR responses are missing the `request_uri` field — only `expires_in` is
returned. Both gaps are identified in a protocol compliance review. The reviewer
asks whether these are the same compliance tier.

## Pair[CB4-3.4-1423]/B

REPROCESS. The azp validation gap is CONCERN-level (OIDC Core §3.1.3.7.4 SHOULD;
no MUST; aud membership is the MUST-level check). The missing request_uri is
REPROCESS-level: RFC 9126 §2.2 REQUIRED — PAR responses must include request_uri;
without it, clients cannot initiate the authorization flow; the PAR mechanism
is non-functional. REPROCESS overrides CONCERN — the reviewer's question is
answered: the PAR gap is REQUIRED (MUST); the azp gap is guidance-level.
REPROCESS.

## Pair[CB4-3.4-1423]/Reason

azp validation absent (OIDC Core §3.1.3.7.4 SHOULD, CONCERN) + PAR response
missing request_uri (RFC 9126 §2.2 REQUIRED, REPROCESS); REPROCESS overrides
CONCERN → REPROCESS.

## Pair[CB4-3.4-1423]/Phrasing-B

Verdict REPROCESS. azp validation is guidance-level (CONCERN). PAR response
missing request_uri violates RFC 9126 §2.2 REQUIRED. REPROCESS overrides CONCERN.
REPROCESS.

---

## Pair[CB4-3.4-1424]/A

An authorization server does not enforce PKCE for public clients. The same AS's
UserInfo endpoint returns responses with `Content-Type: text/plain` instead of
`application/json`. Both gaps are noted. The team argues that the UserInfo
Content-Type is lower impact because the body content is correct JSON.

## Pair[CB4-3.4-1424]/B

REPROCESS. The missing PKCE requirement is CONCERN-level (RFC 9700 §4.3.2 SHOULD;
no unconditional MUST). The UserInfo Content-Type gap is REPROCESS-level: OIDC
Core §5.3.2 MUST — the UserInfo endpoint must return Content-Type: application/json;
strict clients reject text/plain responses before parsing; the MUST is structural
and unconditional regardless of body correctness. REPROCESS overrides CONCERN —
the team's lower-impact argument for the Content-Type is rejected: body correctness
does not satisfy the Content-Type MUST. REPROCESS.

## Pair[CB4-3.4-1424]/Reason

PKCE not required for public clients (RFC 9700 §4.3.2 SHOULD, CONCERN) + UserInfo
Content-Type wrong (OIDC Core §5.3.2 MUST, REPROCESS); REPROCESS overrides CONCERN;
body-correctness argument rejected → REPROCESS.

## Pair[CB4-3.4-1424]/Phrasing-B

Verdict REPROCESS. PKCE enforcement gap is guidance-level (CONCERN). UserInfo
wrong Content-Type violates OIDC Core §5.3.2 MUST. Body correctness does not
satisfy the Content-Type MUST. REPROCESS overrides CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1425]/A

An OIDC deployment is reviewed against all eight ext54 control points: the AS
discovery issuer matches the well-known URL prefix per RFC 8414 §3; token responses
include `token_type` per RFC 6749 §5.1; PAR responses include `request_uri` per
RFC 9126 §2.2; the UserInfo endpoint returns `Content-Type: application/json`
per OIDC Core §5.3.2; `userinfo_endpoint` is published in the discovery document;
JAR request objects include `nbf` alongside `exp`; the RP validates `azp` against
its own `client_id` when `aud` contains multiple values; and the AS requires PKCE
for all public clients. No anomalies are detected.

## Pair[CB4-3.4-1425]/B

PASS. All eight ext54 control points are satisfied: (1) issuer matches well-known
URL per RFC 8414 §3 MUST; (2) token_type present in all token responses per RFC
6749 §5.1 REQUIRED; (3) request_uri in PAR responses per RFC 9126 §2.2 REQUIRED;
(4) UserInfo Content-Type: application/json per OIDC Core §5.3.2 MUST; (5)
userinfo_endpoint in discovery per OIDC Discovery §3 RECOMMENDED (exceeds
RECOMMENDED floor); (6) JAR nbf present per RFC 9101 §10.3 guidance (exceeds
guidance floor); (7) azp validated for multi-audience tokens per OIDC Core
§3.1.3.7.4 SHOULD (exceeds SHOULD floor); (8) PKCE required for public clients
per RFC 9700 §4.3.2 SHOULD (exceeds SHOULD floor). No violations or guidance gaps
found. PASS.

## Pair[CB4-3.4-1425]/Reason

All eight ext54 controls compliant: RFC 8414 §3 (issuer match), RFC 6749 §5.1
(token_type), RFC 9126 §2.2 (PAR request_uri), OIDC Core §5.3.2 (Content-Type),
OIDC Discovery §3 (userinfo_endpoint), RFC 9101 §10.3 (JAR nbf), OIDC Core
§3.1.3.7.4 (azp), RFC 9700 §4.3.2 (PKCE) → PASS.

## Pair[CB4-3.4-1425]/Phrasing-B

Verdict PASS. All eight ext54 control points satisfied: issuer match (RFC 8414 §3),
token_type (RFC 6749 §5.1), PAR request_uri (RFC 9126 §2.2), UserInfo Content-Type
(OIDC Core §5.3.2), userinfo_endpoint discovery, JAR nbf, azp validation,
PKCE for public clients. PASS.

---

## Pair[CB4-3.4-1426]/A

An OIDC authorization server has four concurrent guidance-level gaps: the discovery
document omits `userinfo_endpoint`; JAR request objects do not include `nbf`;
OIDC relying parties omit `azp` validation for multi-audience tokens; and the AS
does not require PKCE for public clients. A security committee reviews all four
findings and asserts that these gaps collectively establish an authorization
infrastructure that falls below acceptable operational thresholds, warranting
escalation to REPROCESS.

## Pair[CB4-3.4-1426]/B

CONCERN. Four guidance-level observations: (1) userinfo_endpoint absent from
discovery — OIDC Discovery §3 RECOMMENDED; no MUST; (2) JAR nbf absent — RFC 9101
§10.3 guidance SHOULD; no MUST; (3) azp not validated for multi-audience — OIDC
Core §3.1.3.7.4 SHOULD; no MUST; (4) PKCE not required for public clients — RFC
9700 §4.3.2 SHOULD; no MUST. The graduation rule is categorical and count-
independent: N CONCERNs = CONCERN regardless of count, thematic breadth, threshold
framing, committee authority, or operational-quality language. Four SHOULD-level
gaps equal one CONCERN. Only a MUST-level violation triggers REPROCESS. The
committee's escalation conclusion is rejected. CONCERN.

## Pair[CB4-3.4-1426]/Reason

Four SHOULD/RECOMMENDED-level gaps: userinfo_endpoint + JAR nbf + azp validation
+ PKCE for public clients; graduation rule: N CONCERNs ≠ REPROCESS; threshold
argument rejected → CONCERN.

## Pair[CB4-3.4-1426]/Phrasing-B

Verdict CONCERN. All four findings are guidance-level. The graduation rule is
count-independent — four CONCERNs equal one CONCERN. Threshold and operational-
quality escalation arguments are foreclosed. CONCERN.
