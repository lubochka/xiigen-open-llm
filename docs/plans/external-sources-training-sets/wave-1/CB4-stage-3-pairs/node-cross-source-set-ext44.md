# Node Cross-Source Set Extension 44
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1339–1348 (20 instances — 10 base pairs × 2 phrasings)
# Sources: RFC 7662, OIDC Core §5.5.1, RFC 8414 §2,
#          RFC 6750 §7, RFC 9101 §4,
#          RFC 7521 §4.8, RFC 6749 §5.1,
#          RFC 9126 §2.3, NIST SSDF PO.1, RFC 6750 §3.1
# Running total after this file: 2696 instances (1348 base pairs)
# Formula check: 120 + (1348 − 60) × 2 = 120 + 2576 = 2696 ✓

---

## Pair[CB4-3.4-1339]/A

A resource server caches token introspection responses for 24 hours regardless
of token expiry, creating a long revocation window. The same OIDC authorization
server ignores the `claims` request parameter in authorization requests, always
returning the default claim set. Both gaps are noted in an audit. A reviewer
argues that two operational deficiencies that reduce the effectiveness of the
AS's security controls constitute a combined REPROCESS-level finding.

## Pair[CB4-3.4-1339]/B

CONCERN. Two guidance-level observations: (1) introspection response cache TTL
ignoring exp — RFC 7662 contains no unconditional MUST specifying a maximum cache
duration; caching beyond exp is a guidance-level recommendation to limit the
revocation window; (2) claims parameter ignored — OIDC Core §5.5.1 SHOULD; no
unconditional MUST requires the AS to honor the claims request parameter. The
graduation rule applies: N CONCERNs = CONCERN regardless of count, operational
severity framing, or combined-deficiency arguments. Neither finding has a MUST-
level anchor. CONCERN.

## Pair[CB4-3.4-1339]/Reason

Two SHOULD/guidance-level gaps: introspection cache beyond exp (RFC 7662) +
claims parameter ignored (OIDC Core §5.5.1 SHOULD); graduation rule: N CONCERNs
≠ REPROCESS → CONCERN.

## Pair[CB4-3.4-1339]/Phrasing-B

Verdict CONCERN. Both findings are guidance-level — introspection cache TTL
(RFC 7662) and claims parameter support (OIDC Core §5.5.1). The graduation rule
forecloses combined-deficiency escalation. CONCERN.

---

## Pair[CB4-3.4-1340]/A

An OIDC authorization server ignores the `claims` request parameter in all
authorization requests. The same AS's discovery metadata omits the
`grant_types_supported` field. Both gaps are identified in a developer integration
audit. The developer argues that two metadata and claims-handling gaps together
signal an insufficiently complete OIDC implementation, warranting REPROCESS.

## Pair[CB4-3.4-1340]/B

CONCERN. Two guidance-level observations: (1) claims parameter ignored — OIDC
Core §5.5.1 SHOULD; no unconditional MUST; (2) grant_types_supported absent —
RFC 8414 §2 RECOMMENDED / SHOULD; absent field has a defined RFC fallback
assumption; no MUST mandates this field. The graduation rule is categorical:
N CONCERNs = CONCERN regardless of domain category, implementation-completeness
framing, or count. Neither observation has a MUST-level anchor. CONCERN.

## Pair[CB4-3.4-1340]/Reason

Two SHOULD-level gaps: claims parameter ignored (OIDC Core §5.5.1 SHOULD) +
grant_types_supported absent (RFC 8414 §2 SHOULD); graduation rule categorical →
CONCERN.

## Pair[CB4-3.4-1340]/Phrasing-B

Verdict CONCERN. Both findings are SHOULD-level. The graduation rule forecloses
implementation-completeness escalation arguments. CONCERN.

---

## Pair[CB4-3.4-1341]/A

An authorization server's discovery metadata omits the `grant_types_supported`
field. The same AS's resource servers return `WWW-Authenticate` Bearer challenges
that do not include the `realm` attribute. Both gaps are flagged in a protocol
conformance review. The reviewer argues that two presentation and discoverability
deficiencies together constitute a conformance-level finding requiring REPROCESS.

## Pair[CB4-3.4-1341]/B

CONCERN. Two guidance-level observations: (1) grant_types_supported absent —
RFC 8414 §2 RECOMMENDED / SHOULD; defined fallback behavior exists; no MUST;
(2) realm absent from Bearer challenges — RFC 6750 §3.1 SHOULD; challenge remains
parseable and processable without realm; no MUST. The graduation rule applies
categorically: N CONCERNs = CONCERN regardless of thematic clustering (both
involve protocol presentation completeness), reviewer framing, or count. CONCERN.

## Pair[CB4-3.4-1341]/Reason

Two SHOULD-level gaps: grant_types_supported absent (RFC 8414 §2 SHOULD) + realm
absent (RFC 6750 §3.1 SHOULD); graduation rule: N CONCERNs ≠ REPROCESS → CONCERN.

## Pair[CB4-3.4-1341]/Phrasing-B

Verdict CONCERN. Both findings are SHOULD-level guidance gaps. The graduation rule
forecloses conformance-level escalation regardless of thematic overlap. CONCERN.

---

## Pair[CB4-3.4-1342]/A

An OIDC provider has no formal security requirements documentation (NIST SSDF
PO.1 gap). A concurrent code review confirms that the AS's JAR processing pipeline
accepts request objects that do not include the `iss` claim — the AS validates
the signature but does not check that `iss` equals the `client_id`. Active clients
are submitting JAR request objects without `iss`, and the AS processes them
successfully in production. The team classifies PO.1 as an administrative backlog
item and the JAR issue as a future milestone.

## Pair[CB4-3.4-1342]/B

REPROCESS. PO.1 absence alone is CONCERN. The PO.1 escalation rule applies: PO.1
absence combined with a traceable production MUST violation escalates to REPROCESS.
RFC 9101 §4 MUST: request objects must contain the `iss` claim set to client_id;
the AS must reject request objects where iss is absent or does not match client_id.
Active production clients are submitting iss-absent JAR objects that the AS
accepts — a live MUST violation. The PO.1 gap means the iss obligation was never
formally captured as a required control. The backlog-and-milestone framing is
rejected: the PO.1 escalation rule mandates REPROCESS when PO.1 absence is
traceable to an active production MUST violation. REPROCESS.

## Pair[CB4-3.4-1342]/Reason

PO.1 gap (CONCERN alone) + active MUST violation (RFC 9101 §4 MUST: JAR request
object must include iss=client_id; AS accepts iss-absent JARs in production)
= PO.1 escalation → REPROCESS.

## Pair[CB4-3.4-1342]/Phrasing-B

Verdict REPROCESS. PO.1 escalation: PO.1 absence + traceable active MUST violation
= REPROCESS. RFC 9101 §4 MUST is violated: JAR request objects without iss are
accepted in production. REPROCESS.

---

## Pair[CB4-3.4-1343]/A

A resource server caches token introspection responses for 24 hours, creating a
post-revocation acceptance window. The same RS accepts HTTP requests that present
Bearer tokens via both the `Authorization` header and the request body form
parameter simultaneously, using the header value when both are present. A security
auditor notes both gaps. The auditor asks whether the caching gap mitigates or
is separate from the multi-method acceptance issue.

## Pair[CB4-3.4-1343]/B

REPROCESS. The introspection cache TTL gap is CONCERN-level (RFC 7662 no MUST
for cache duration; caching beyond exp is guidance-level). The multiple-methods
acceptance is REPROCESS-level: RFC 6750 §7 MUST requires the RS to reject requests
containing Bearer tokens via multiple transmission methods with invalid_request —
the RS must reject before examining or using the token value, regardless of which
method is "primary." REPROCESS overrides CONCERN — the cache TTL guidance gap
does not moderate the MUST violation in multi-method token acceptance. The two
findings are independent. REPROCESS.

## Pair[CB4-3.4-1343]/Reason

Introspection cache beyond exp (RFC 7662 guidance, CONCERN) + multiple Bearer
methods accepted (RFC 6750 §7 MUST, REPROCESS); REPROCESS overrides CONCERN →
REPROCESS.

## Pair[CB4-3.4-1343]/Phrasing-B

Verdict REPROCESS. Introspection cache TTL is guidance-level (CONCERN). Accepting
requests with multiple Bearer token methods violates RFC 6750 §7 MUST. REPROCESS
overrides CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1344]/A

An OIDC authorization server ignores the `claims` request parameter — UserInfo
returns the default claim set. The same AS does not check `jti` uniqueness for
`private_key_jwt` client assertions — replayed assertions within their validity
window succeed. Both findings are noted in a penetration test report. The team
argues that the claims parameter gap is non-security and should be treated
separately from the assertion replay issue.

## Pair[CB4-3.4-1344]/B

REPROCESS. The claims parameter gap is CONCERN-level (OIDC Core §5.5.1 SHOULD;
no MUST for claims parameter support). The client assertion JTI replay finding
is REPROCESS-level: RFC 7521 §4.8 MUST — each assertion must contain a unique
jti; the AS must verify jti uniqueness within the assertion's validity window;
not tracking jti allows unlimited client impersonation for the assertion's lifetime.
REPROCESS overrides CONCERN — the team is correct that the claims gap is non-
security in the MUST sense; however, REPROCESS overrides the non-security guidance
gap for the overall verdict. REPROCESS.

## Pair[CB4-3.4-1344]/Reason

claims parameter ignored (OIDC Core §5.5.1 SHOULD, CONCERN) + client assertion
JTI replay not checked (RFC 7521 §4.8 MUST, REPROCESS); REPROCESS overrides
CONCERN → REPROCESS.

## Pair[CB4-3.4-1344]/Phrasing-B

Verdict REPROCESS. Claims parameter gap is guidance-level (CONCERN). Absent JTI
replay prevention violates RFC 7521 §4.8 MUST. REPROCESS overrides CONCERN.
REPROCESS.

---

## Pair[CB4-3.4-1345]/A

An authorization server's discovery metadata omits the `grant_types_supported`
field. The same AS accepts PAR `request_uri` values that are 35 seconds past
their stated expiry. Both gaps are noted. A developer asks whether the metadata
gap and the PAR enforcement gap are both specification violations or only one.

## Pair[CB4-3.4-1345]/B

REPROCESS. The grant_types_supported gap is CONCERN-level (RFC 8414 §2
RECOMMENDED / SHOULD; defined fallback exists; no MUST). The expired request_uri
acceptance is REPROCESS-level: RFC 9126 §2.3 MUST — the AS must reject
authorization requests using a request_uri beyond its expiry time with
invalid_request_uri; no grace-period provision exists; accepting expired
request_uri values allows replay of captured PAR references. REPROCESS overrides
CONCERN — the metadata guidance gap does not moderate the MUST violation in PAR
expiry enforcement. The developer's question: only the PAR gap is a MUST-level
violation; the metadata gap is SHOULD-level. Overall verdict: REPROCESS.

## Pair[CB4-3.4-1345]/Reason

grant_types_supported absent (RFC 8414 §2 SHOULD, CONCERN) + expired request_uri
accepted (RFC 9126 §2.3 MUST, REPROCESS); REPROCESS overrides CONCERN → REPROCESS.

## Pair[CB4-3.4-1345]/Phrasing-B

Verdict REPROCESS. grant_types_supported absence is guidance-level (CONCERN).
Accepting expired PAR request_uri violates RFC 9126 §2.3 MUST. REPROCESS overrides
CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1346]/A

A resource server caches token introspection responses indefinitely beyond the
token's `exp`. The same AS issues token responses that omit the `scope` parameter
when the granted scope is narrower than the requested scope. Both gaps are
discovered in an OAuth compliance audit. The compliance team asks whether one
finding escalates the other or whether they are evaluated independently.

## Pair[CB4-3.4-1346]/B

REPROCESS. The introspection cache TTL gap is CONCERN-level (RFC 7662 no MUST
for maximum cache duration; guidance-level recommendation to align cache with exp).
The missing scope in the token response is REPROCESS-level: RFC 6749 §5.1 MUST —
when the AS issues a token with scope different from the scope requested, the AS
MUST include the `scope` parameter in the token response; clients cannot determine
the granted scope without it, especially for opaque tokens. REPROCESS overrides
CONCERN — the findings are independent and do not escalate each other; however,
the MUST violation in the token response produces the REPROCESS verdict. REPROCESS.

## Pair[CB4-3.4-1346]/Reason

Introspection cache beyond exp (RFC 7662 guidance, CONCERN) + token response
scope absent when narrowed (RFC 6749 §5.1 MUST, REPROCESS); REPROCESS overrides
CONCERN → REPROCESS.

## Pair[CB4-3.4-1346]/Phrasing-B

Verdict REPROCESS. Introspection cache TTL is guidance-level (CONCERN). Omitting
scope when narrowed violates RFC 6749 §5.1 MUST. REPROCESS overrides CONCERN.
REPROCESS.

---

## Pair[CB4-3.4-1347]/A

An OAuth/OIDC deployment is reviewed against all eight ext51 control points:
the resource server rejects requests with multiple Bearer token transmission methods
per RFC 6750 §7; JAR request objects include `iss` set to client_id per RFC 9101
§4; the AS checks `jti` uniqueness for client assertions and rejects replays per
RFC 7521 §4.8; token responses include `scope` when the issued scope differs from
requested scope per RFC 6749 §5.1; the AS rejects expired PAR `request_uri` values
with `invalid_request_uri` per RFC 9126 §2.3; introspection response cache TTL
is aligned with token remaining lifetime; AS discovery metadata includes
`grant_types_supported`; and the AS honors the `claims` request parameter for
UserInfo claim selection. No anomalies are detected.

## Pair[CB4-3.4-1347]/B

PASS. All eight ext51 control points are satisfied: (1) multiple Bearer method
rejection per RFC 6750 §7 MUST; (2) JAR iss claim present and equals client_id
per RFC 9101 §4 MUST; (3) client assertion jti uniqueness tracked and replay
rejected per RFC 7521 §4.8 MUST; (4) scope included in token response when
different from requested per RFC 6749 §5.1 MUST; (5) expired PAR request_uri
rejected per RFC 9126 §2.3 MUST; (6) introspection cache TTL aligned with token
exp per RFC 7662 guidance (exceeds guidance floor); (7) grant_types_supported
in discovery metadata per RFC 8414 §2 SHOULD (exceeds SHOULD floor); (8) claims
parameter honored per OIDC Core §5.5.1 SHOULD (exceeds SHOULD floor). No
violations or guidance gaps found. PASS.

## Pair[CB4-3.4-1347]/Reason

All eight ext51 controls compliant: RFC 6750 §7 (multiple Bearer methods), RFC 9101
§4 (JAR iss), RFC 7521 §4.8 (assertion jti), RFC 6749 §5.1 (scope when narrowed),
RFC 9126 §2.3 (PAR expiry), RFC 7662 (introspection cache), RFC 8414 §2
(grant_types_supported), OIDC Core §5.5.1 (claims parameter) → PASS.

## Pair[CB4-3.4-1347]/Phrasing-B

Verdict PASS. All eight ext51 control points satisfied: multiple Bearer rejection
(RFC 6750 §7), JAR iss (RFC 9101 §4), assertion jti (RFC 7521 §4.8), scope when
narrowed (RFC 6749 §5.1), PAR expiry (RFC 9126 §2.3), introspection cache alignment
(RFC 7662), grant_types_supported (RFC 8414 §2), claims parameter (OIDC Core §5.5.1).
PASS.

---

## Pair[CB4-3.4-1348]/A

An OAuth/OIDC deployment has three concurrent guidance-level gaps: introspection
responses are cached for 24 hours regardless of token expiry; the OIDC provider
ignores the `claims` request parameter; and the AS discovery metadata omits
`grant_types_supported`. A principal security architect reviews all three gaps
and argues that the combined operational impact — extended revocation windows,
degraded claim selection, and reduced discoverability — collectively constitutes
a systemic conformance deficiency requiring REPROCESS escalation to enforce
remediation.

## Pair[CB4-3.4-1348]/B

CONCERN. Three guidance-level observations: (1) introspection cache beyond exp —
RFC 7662 no MUST for cache TTL; (2) claims parameter ignored — OIDC Core §5.5.1
SHOULD; no unconditional MUST; (3) grant_types_supported absent — RFC 8414 §2
RECOMMENDED / SHOULD; defined fallback behavior. The graduation rule is categorical
and count-independent: N CONCERNs = CONCERN regardless of count, combined-impact
framing, systemic-deficiency arguments, or remediation-enforcement intent. The
security architect's argument is rejected: no MUST-level violation is identified;
three SHOULD-level gaps equal one CONCERN classification. Only a MUST-level
violation triggers REPROCESS. CONCERN.

## Pair[CB4-3.4-1348]/Reason

Three guidance-level SHOULD gaps: introspection cache (RFC 7662) + claims
parameter (OIDC Core §5.5.1) + grant_types_supported (RFC 8414 §2); graduation
rule: N CONCERNs ≠ REPROCESS; systemic-deficiency argument rejected → CONCERN.

## Pair[CB4-3.4-1348]/Phrasing-B

Verdict CONCERN. All three findings are SHOULD-level guidance gaps. The graduation
rule is categorical — three CONCERNs equal one CONCERN. Systemic-deficiency
escalation arguments do not override the graduation rule. CONCERN.
