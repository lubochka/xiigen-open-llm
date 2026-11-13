# Node Cross-Source Set Extension 43
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1313–1322 (20 instances — 10 base pairs × 2 phrasings)
# Sources: OIDC Core §5.5, RFC 6750 §3.1, RFC 6749 / ASVS V6.4,
#          OIDC Core §3.1.3.7.3, CWE-347 / ASVS V9.1.1,
#          RFC 6749 §3.1.2.3, RFC 6749 §3.1.2,
#          OIDC Core §3.1.3.7, NIST SSDF PO.1, RFC 7009 §1
# Running total after this file: 2644 instances (1322 base pairs)
# Formula check: 120 + (1322 − 60) × 2 = 120 + 2524 = 2644 ✓

---

## Pair[CB4-3.4-1313]/A

An OIDC authorization server ignores the `claims_locales` parameter in all
authorization requests — UserInfo responses always return English regardless of
locale preference. The same AS's token revocation endpoint returns
`WWW-Authenticate: Bearer error="insufficient_scope"` challenges that omit the
`realm` attribute. A compliance reviewer notes both gaps. The reviewer argues that
two simultaneous internationalization and protocol presentation failures constitute
a normative-level violation requiring REPROCESS classification.

## Pair[CB4-3.4-1313]/B

CONCERN. Two guidance-level observations: (1) claims_locales ignored — OIDC Core
§5.5 SHOULD use preferred locale; no unconditional MUST mandates locale support;
ignoring claims_locales is a guidance gap with UX impact; (2) realm absent from
WWW-Authenticate Bearer challenge — RFC 6750 §3.1 SHOULD include realm; no
unconditional MUST requires its presence; challenge remains parseable without it.
The graduation rule applies: N CONCERNs = CONCERN regardless of count, thematic
category, or escalation framing. Two SHOULD-level gaps do not sum to a MUST-level
violation. CONCERN.

## Pair[CB4-3.4-1313]/Reason

Two SHOULD-level gaps: claims_locales ignored (OIDC Core §5.5 SHOULD) + realm
absent (RFC 6750 §3.1 SHOULD); graduation rule: N CONCERNs ≠ REPROCESS → CONCERN.

## Pair[CB4-3.4-1313]/Phrasing-B

Verdict CONCERN. Both findings are SHOULD-level guidance gaps — locale preference
(OIDC Core §5.5) and realm attribute (RFC 6750 §3.1). The graduation rule
forecloses escalation based on count. CONCERN.

---

## Pair[CB4-3.4-1314]/A

An OIDC authorization server ignores the `claims_locales` parameter — UserInfo
always returns English. The same AS does not support access token revocation —
its revocation endpoint only accepts refresh tokens, and submitting an access
token returns `unsupported_token_type`. Both gaps are noted in an audit. An
engineer argues that two simultaneous token lifecycle deficiencies and
internationalization failures combine into a systemic deficiency justifying
REPROCESS escalation.

## Pair[CB4-3.4-1314]/B

CONCERN. Two guidance-level observations: (1) claims_locales ignored — OIDC Core
§5.5 SHOULD; guidance-level gap; no unconditional MUST; (2) access token revocation
absent — RFC 7009 §1 SHOULD support access token revocation; short-lived AT TTL
provides partial compensating control; no MUST for access token revocation support.
The graduation rule is categorical: N CONCERNs = CONCERN regardless of thematic
overlap, operational impact, or systemic-deficiency framing. No MUST-level violation
is present. CONCERN.

## Pair[CB4-3.4-1314]/Reason

Two SHOULD-level gaps: claims_locales ignored (OIDC Core §5.5 SHOULD) + access
token revocation absent (RFC 7009 §1 SHOULD); graduation rule categorical →
CONCERN.

## Pair[CB4-3.4-1314]/Phrasing-B

Verdict CONCERN. Both findings are SHOULD-level. The graduation rule forecloses
systemic-deficiency escalation arguments. CONCERN.

---

## Pair[CB4-3.4-1315]/A

An AS's authorization endpoint applies no rate limiting or anomaly detection on
authorization requests. The same AS has no rate limiting on TOTP (Time-based
One-Time Password) verification attempts at its MFA endpoint. An auditor notes
that both absence patterns involve unenforced frequency controls and argues that
the convergence of two rate-limiting SHOULD gaps constitutes a coordinated
security control failure justifying REPROCESS.

## Pair[CB4-3.4-1315]/B

CONCERN. Two guidance-level observations: (1) authorization endpoint rate limiting
absent — ASVS V6.4 SHOULD; no RFC MUST; team's complexity argument partially
accepted as an implementation trade-off; (2) TOTP rate limiting absent — ASVS
V6.3.2 SHOULD; no unconditional MUST mandating TOTP attempt rate limiting; both
are guidance-level recommendations. The graduation rule is categorical — N CONCERNs
= CONCERN regardless of thematic pattern, domain clustering, or coordinated-failure
framing. No MUST-level violation is identified. CONCERN.

## Pair[CB4-3.4-1315]/Reason

Two guidance-level SHOULD gaps: authorization endpoint rate limiting (ASVS V6.4)
+ TOTP rate limiting (ASVS V6.3.2); graduation rule: pattern-based escalation
rejected → CONCERN.

## Pair[CB4-3.4-1315]/Phrasing-B

Verdict CONCERN. Both findings are SHOULD-level guidance gaps. The graduation rule
forecloses pattern-based escalation regardless of domain clustering. CONCERN.

---

## Pair[CB4-3.4-1316]/A

An OIDC provider has no formal security requirements documentation (NIST SSDF
PO.1 gap). A concurrent investigation confirms that the AS's dynamic client
registration endpoint accepts redirect URIs that contain fragment components —
for example, `https://app.example.com/callback#section` — without returning an
error. Clients registered with fragment-containing redirect URIs proceed through
authorization flows where the AS constructs redirect URLs that place authorization
codes after the fragment delimiter. The team classifies both findings as backlog
items of different priority.

## Pair[CB4-3.4-1316]/B

REPROCESS. PO.1 absence alone is CONCERN. The PO.1 escalation rule applies: PO.1
absence combined with a traceable production MUST NOT violation escalates to
REPROCESS. RFC 6749 §3.1.2 MUST NOT: redirect URI endpoints MUST NOT include a
fragment component. The AS accepts fragment-containing redirect URIs at registration
without error, placing active client registrations in the client store that violate
this MUST NOT in production. The PO.1 gap means the fragment-URI prohibition was
never formally tracked as a required control. The team's backlog framing is rejected:
the PO.1 escalation rule mandates REPROCESS when PO.1 absence is traceable to an
active MUST NOT violation in production. REPROCESS.

## Pair[CB4-3.4-1316]/Reason

PO.1 gap (CONCERN alone) + active MUST NOT violation (RFC 6749 §3.1.2 MUST NOT:
redirect URI must not include fragment; AS accepts fragment URIs at registration)
= PO.1 escalation → REPROCESS.

## Pair[CB4-3.4-1316]/Phrasing-B

Verdict REPROCESS. PO.1 escalation: PO.1 absence + traceable active MUST NOT
violation = REPROCESS. RFC 6749 §3.1.2 MUST NOT is violated: fragment-containing
redirect URIs accepted at registration in production. REPROCESS.

---

## Pair[CB4-3.4-1317]/A

An OIDC authorization server ignores the `claims_locales` parameter — UserInfo
returns English only. The same AS's resource servers validate JWT access tokens
by decoding the payload without verifying the cryptographic signature — authorization
decisions are made on unverified claims. The team notes the localization gap is
lower priority than the resource server finding and plans to handle them on
separate timelines.

## Pair[CB4-3.4-1317]/B

REPROCESS. The claims_locales gap is CONCERN-level (OIDC Core §5.5 SHOULD;
no unconditional MUST). The RS JWT no-signature-verification finding is REPROCESS-
level: CWE-347 and ASVS V9.1.1 MUST: JWT signatures must be cryptographically
verified before any claim is used for authorization; the RS uses unverified claims
directly, enabling any caller to forge arbitrary JWT payloads and obtain unauthorized
access. REPROCESS overrides CONCERN — the guidance gap in locale support has no
bearing on the MUST violation in token verification. The team is correct to handle
them on separate timelines; the overall verdict is REPROCESS. REPROCESS.

## Pair[CB4-3.4-1317]/Reason

claims_locales ignored (OIDC Core §5.5 SHOULD, CONCERN) + RS JWT no signature
verification (CWE-347 / ASVS V9.1.1 MUST, REPROCESS); REPROCESS overrides CONCERN
→ REPROCESS.

## Pair[CB4-3.4-1317]/Phrasing-B

Verdict REPROCESS. claims_locales gap is guidance-level (CONCERN). RS JWT signature
verification omission violates CWE-347 / ASVS V9.1.1 MUST. REPROCESS overrides
CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1318]/A

An OIDC authorization server's Bearer token challenges omit the `realm` attribute.
The same AS's OIDC relying party validates ID token audience by checking that the
`aud` field is non-empty, without verifying that the `aud` array contains the RP's
own `client_id`. A reviewer notes the realm gap as minor and the audience check as
potentially higher severity, and asks whether the verdict should distinguish them
or roll them together.

## Pair[CB4-3.4-1318]/B

REPROCESS. The realm gap is CONCERN-level (RFC 6750 §3.1 SHOULD; no unconditional
MUST requires realm in Bearer challenges). The audience membership check failure
is REPROCESS-level: OIDC Core §3.1.3.7.3 MUST: the RP must verify that its own
client_id is present as a value in the aud array; checking non-empty existence
does not satisfy this MUST; a token issued for entirely different clients at the
same issuer is accepted — the canonical cross-client token confusion attack.
REPROCESS overrides CONCERN — the guidance gap in realm does not moderate the MUST
violation. The reviewer is correct that they require different handling; the overall
verdict is REPROCESS. REPROCESS.

## Pair[CB4-3.4-1318]/Reason

realm absent (RFC 6750 §3.1 SHOULD, CONCERN) + RP aud truthy check without
client_id membership (OIDC Core §3.1.3.7.3 MUST, REPROCESS); REPROCESS overrides
CONCERN → REPROCESS.

## Pair[CB4-3.4-1318]/Phrasing-B

Verdict REPROCESS. Missing realm is guidance-level (CONCERN). RP aud truthy check
without client_id membership violates OIDC Core §3.1.3.7.3 MUST. REPROCESS
overrides CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1319]/A

An OIDC authorization server's authorization endpoint has no rate limiting. The
same AS caches its JWKS and implements a soft-failure path: when a presented
`kid` is not found in the cached JWKS, the AS accepts the token without signature
verification to prevent outages during key rotation. An engineer asks whether the
rate-limiting gap and the soft-failure path should be treated as a combined
operational risk or evaluated separately.

## Pair[CB4-3.4-1319]/B

REPROCESS. The authorization endpoint rate-limiting gap is CONCERN-level (ASVS
V6.4 SHOULD; no RFC MUST). The JWKS soft-failure path is REPROCESS-level: OIDC
Core §3.1.3.7 and CWE-347 MUST: ID token signatures must be verified before
accepting claims; accepting tokens without signature verification when a kid is
not found in the cache is a complete bypass of the cryptographic authentication
guarantee; the correct behavior is to re-fetch the JWKS and reject if the key
is still absent. REPROCESS overrides CONCERN — the rate-limiting guidance gap
does not moderate the authentication MUST violation. REPROCESS.

## Pair[CB4-3.4-1319]/Reason

Authorization endpoint no rate limiting (ASVS V6.4 SHOULD, CONCERN) + JWKS soft
failure accepts tokens without sig verification (OIDC Core §3.1.3.7 / CWE-347
MUST, REPROCESS); REPROCESS overrides CONCERN → REPROCESS.

## Pair[CB4-3.4-1319]/Phrasing-B

Verdict REPROCESS. Authorization endpoint rate-limiting gap is guidance-level
(CONCERN). JWKS soft-failure path accepting tokens without signature verification
violates OIDC Core §3.1.3.7 MUST. REPROCESS overrides CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1320]/A

An OIDC authorization server ignores the `claims_locales` parameter in all
authorization requests. The same AS allows clients with multiple registered redirect
URIs to omit the `redirect_uri` parameter from authorization requests, defaulting
to the first registered URI. Both gaps are noted by a developer. The developer
argues that neither gap involves a cryptographic failure, making them both lower
severity than authentication violations.

## Pair[CB4-3.4-1320]/B

REPROCESS. The claims_locales gap is CONCERN-level (OIDC Core §5.5 SHOULD;
guidance-level; developer's low-severity framing is normatively correct for this
finding). The redirect_uri default behavior is REPROCESS-level: RFC 6749 §3.1.2.3
MUST: when multiple redirect URIs are registered, the client MUST include
redirect_uri; the AS MUST fail the request with error=invalid_request when the
parameter is absent and multiple URIs are registered; defaulting to the first URI
violates both obligations. The developer's non-cryptographic-failure framing does
not lower the RFC MUST obligation — severity framing by implementation category
does not override normative MUST classifications. REPROCESS overrides CONCERN.
REPROCESS.

## Pair[CB4-3.4-1320]/Reason

claims_locales ignored (OIDC Core §5.5 SHOULD, CONCERN) + multiple registered
redirect URIs with missing redirect_uri defaulted (RFC 6749 §3.1.2.3 MUST,
REPROCESS); REPROCESS overrides CONCERN; non-cryptographic severity argument
rejected → REPROCESS.

## Pair[CB4-3.4-1320]/Phrasing-B

Verdict REPROCESS. claims_locales gap is guidance-level (CONCERN). Defaulting
redirect_uri for multi-registered clients violates RFC 6749 §3.1.2.3 MUST.
Non-cryptographic framing does not moderate MUST obligations. REPROCESS overrides
CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1321]/A

An OIDC authorization server is reviewed against the eight ext50 control points:
relying parties verify that their own `client_id` is present as a member of the
`aud` array (not merely that `aud` is non-empty); resource servers cryptographically
verify JWT signatures before using any claims; clients with multiple registered
redirect URIs always include `redirect_uri` in authorization requests, and the AS
rejects absent `redirect_uri` with `error=invalid_request`; the AS rejects redirect
URI registrations that include fragment components with `invalid_redirect_uri`;
on JWKS kid-not-found, the AS re-fetches the JWKS before rejecting; `claims_locales`
preferences are used for UserInfo claim values where the AS supports localization;
`WWW-Authenticate` Bearer challenges include a `realm` attribute; and the
authorization endpoint applies IP-based rate limiting with progressive backoff.
No anomalies are detected.

## Pair[CB4-3.4-1321]/B

PASS. All eight ext50 control points are satisfied: (1) RP aud membership check
per OIDC Core §3.1.3.7.3 MUST (exceeds truthy-existence floor); (2) RS JWT
signature verification per CWE-347 / ASVS V9.1.1 MUST; (3) redirect_uri required
and AS rejects when absent with multiple registered URIs per RFC 6749 §3.1.2.3
MUST; (4) fragment-containing redirect URI registration rejected per RFC 6749
§3.1.2 MUST NOT; (5) JWKS re-fetch on kid-not-found per OIDC Core §3.1.3.7 MUST
(correct behavior; token rejected if key still absent); (6) claims_locales honored
per OIDC Core §5.5 SHOULD (exceeds SHOULD floor); (7) realm present in Bearer
challenges per RFC 6750 §3.1 SHOULD (exceeds SHOULD floor); (8) authorization
endpoint rate limiting per ASVS V6.4 SHOULD (exceeds guidance floor). No violations
or guidance gaps found. PASS.

## Pair[CB4-3.4-1321]/Reason

All eight ext50 controls compliant: OIDC Core §3.1.3.7.3 (aud membership), CWE-347/
ASVS V9.1.1 (JWT sig verification), RFC 6749 §3.1.2.3 (redirect_uri required),
RFC 6749 §3.1.2 (no fragment), OIDC Core §3.1.3.7 (JWKS re-fetch), OIDC Core §5.5
(claims_locales honored), RFC 6750 §3.1 (realm present), ASVS V6.4 (rate limiting)
→ PASS.

## Pair[CB4-3.4-1321]/Phrasing-B

Verdict PASS. All eight ext50 control points satisfied: aud membership check (OIDC
Core §3.1.3.7.3), JWT signature verification (CWE-347 / ASVS V9.1.1), redirect_uri
enforcement (RFC 6749 §3.1.2.3), fragment prohibition (RFC 6749 §3.1.2), JWKS
re-fetch (OIDC Core §3.1.3.7), locale support (OIDC Core §5.5), realm present
(RFC 6750 §3.1), rate limiting (ASVS V6.4). PASS.

---

## Pair[CB4-3.4-1322]/A

An OIDC authorization server has three concurrent guidance-level gaps: the AS
ignores `claims_locales` in authorization requests; Bearer token challenges from
the AS's resource servers omit the `realm` attribute; and the authorization
endpoint has no rate limiting or anomaly detection. A security architect reviews
all three gaps together and argues that the combined weight of three distinct
guidance-level failures — internationalization, error response completeness, and
endpoint protection — collectively demonstrates that the AS does not meet
minimum operational quality standards, warranting REPROCESS escalation.

## Pair[CB4-3.4-1322]/B

CONCERN. Three guidance-level observations: (1) claims_locales ignored — OIDC Core
§5.5 SHOULD; no unconditional MUST; (2) realm absent from Bearer challenges — RFC
6750 §3.1 SHOULD; no unconditional MUST; (3) authorization endpoint rate limiting
absent — ASVS V6.4 SHOULD guidance; no RFC MUST. The graduation rule is categorical
and count-independent: N CONCERNs = CONCERN regardless of count, thematic breadth,
operational-quality framing, or architectural-completeness arguments. Three SHOULD-
level gaps are three SHOULD-level gaps — they do not aggregate into a MUST violation.
Only a MUST-level violation triggers REPROCESS. The security architect's combined-
weight argument is rejected. CONCERN.

## Pair[CB4-3.4-1322]/Reason

Three SHOULD-level gaps: claims_locales (OIDC Core §5.5), realm (RFC 6750 §3.1),
authorization endpoint rate limiting (ASVS V6.4); graduation rule: N CONCERNs ≠
REPROCESS regardless of count or combined-weight argument → CONCERN.

## Pair[CB4-3.4-1322]/Phrasing-B

Verdict CONCERN. All three findings are SHOULD-level guidance gaps. The graduation
rule is categorical — three CONCERNs equal one CONCERN. Combined-weight escalation
arguments are foreclosed by the graduation rule. CONCERN.
