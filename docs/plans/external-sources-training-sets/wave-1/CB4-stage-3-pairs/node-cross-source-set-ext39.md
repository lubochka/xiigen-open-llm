# Node Cross-Source Set Extension 39
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1209–1218 (20 instances — 10 base pairs × 2 phrasings)
# Sources: RFC 7519 §4.1.7, OIDC Discovery §3, RFC 6749 §5.1,
#          CWE-79 / ASVS V5.3.3, ASVS V7.2, RFC 6749 §4.1.3,
#          RFC 8725 §2.1, OIDC Core §6, NIST SSDF PO.1
# Running total after this file: 2436 instances (1218 base pairs)
# Formula check: 120 + (1218 − 60) × 2 = 120 + 2316 = 2436 ✓

---

## Pair[CB4-3.4-1209]/A

An OIDC provider's discovery document omits the `claims_supported` field, and
the AS session cookies for the authorization flow are issued without an explicit
`SameSite` attribute (browser default `Lax` applies). A security audit flags
both observations. The team argues that neither issue individually constitutes
a MUST-level violation and asks whether the combination escalates to REPROCESS.

## Pair[CB4-3.4-1209]/B

CONCERN. Two independent guidance-level observations combine: (1) `claims_supported`
absent from the OIDC discovery document — OIDC Core Discovery §3 classifies it as
RECOMMENDED, not REQUIRED; no unconditional MUST applies; (2) session cookie issued
without explicit `SameSite` — ASVS V7.2 recommends `SameSite=Strict`; no
unconditional RFC MUST mandates a specific `SameSite` value; browser-default `Lax`
provides partial CSRF protection. The graduation rule applies: N CONCERNs do not
escalate to REPROCESS regardless of count. Both observations remain guidance-level.
CONCERN.

## Pair[CB4-3.4-1209]/Reason

Two RECOMMENDED/guidance gaps: claims_supported absent (OIDC Discovery §3 RECOMMENDED)
+ SameSite absent (ASVS V7.2 guidance); graduation rule: N CONCERNs ≠ REPROCESS;
both guidance-level → CONCERN.

## Pair[CB4-3.4-1209]/Phrasing-B

Verdict CONCERN. Neither gap is MUST-level: `claims_supported` is RECOMMENDED per
OIDC Core Discovery §3, and explicit `SameSite` is ASVS V7.2 guidance — not an
unconditional RFC MUST. Combining two guidance-level findings does not change their
classification; the graduation rule forecloses escalation. CONCERN.

---

## Pair[CB4-3.4-1210]/A

An AS token endpoint returns `"expires_in": 3600` while the actual JWT `exp`
lifetime is 900 seconds, causing clients to cache the token past its valid
window. Separately, the same AS discovery document omits the `claims_supported`
field. Both are flagged in an internal review. The team asks whether either
or both issues require a REPROCESS filing.

## Pair[CB4-3.4-1210]/B

CONCERN. Two guidance-level observations: (1) `expires_in` value of 3600 contradicts
the actual 900-second token lifetime — RFC 6749 §5.1 defines `expires_in` as the
actual token lifetime but does not contain an explicit MUST for accuracy when the
field is present; the practical impact (silent 401 failures) is real but the
normative anchor is definitional, not an unconditional prohibition; (2)
`claims_supported` absent — OIDC Core Discovery §3 RECOMMENDED, not REQUIRED; no
MUST applies for minimum conformance. Neither finding has an unconditional MUST
anchor. Graduation rule: N CONCERNs ≠ REPROCESS. CONCERN.

## Pair[CB4-3.4-1210]/Reason

expires_in mismatch (RFC 6749 §5.1 definitional gap, no explicit MUST for accuracy)
+ claims_supported absent (OIDC Discovery §3 RECOMMENDED); two guidance-level
findings; graduation rule → CONCERN.

## Pair[CB4-3.4-1210]/Phrasing-B

Verdict CONCERN. RFC 6749 §5.1 defines `expires_in` semantically but lacks an
explicit MUST requiring accuracy when the field is present — a definitional gap.
`claims_supported` absence is RECOMMENDED-level per OIDC Core Discovery §3. Two
guidance-level findings do not aggregate to REPROCESS. CONCERN.

---

## Pair[CB4-3.4-1211]/A

A security review of an OIDC authorization server surfaces three observations:
the discovery document omits `claims_supported`; the token endpoint response
includes `"expires_in": 3600` while the actual token lifetime is 900 seconds;
and AS session cookies are issued without an explicit `SameSite` attribute. The
auditor argues that three simultaneous guidance gaps, taken together, should
constitute a REPROCESS finding because their cumulative operational impact is
significant.

## Pair[CB4-3.4-1211]/B

CONCERN. All three observations are guidance-level: (1) `claims_supported` absent —
OIDC Core Discovery §3 RECOMMENDED, not REQUIRED; (2) `expires_in` mismatch —
RFC 6749 §5.1 definitional gap; no explicit MUST for accuracy when the field is
present; (3) `SameSite` absent — ASVS V7.2 / OWASP guidance-level; no unconditional
RFC MUST mandates a specific value. The auditor's aggregation argument is rejected:
the graduation rule is categorical — N CONCERNs do not escalate to REPROCESS
regardless of count or cumulative impact. Only a MUST-level violation can produce
REPROCESS. Three CONCERNs remain CONCERN.

## Pair[CB4-3.4-1211]/Reason

Three guidance-level gaps: claims_supported (OIDC Discovery §3 RECOMMENDED) +
expires_in mismatch (RFC 6749 §5.1 definitional) + SameSite absent (ASVS V7.2);
graduation rule: 3 CONCERNs ≠ REPROCESS; auditor aggregation argument rejected →
CONCERN.

## Pair[CB4-3.4-1211]/Phrasing-B

Verdict CONCERN. Three simultaneous guidance gaps do not aggregate to REPROCESS.
The graduation rule is unconditional: absent a MUST-level finding, no accumulation
of RECOMMENDED or guidance-level deficiencies changes the verdict. All three
observations — `claims_supported` RECOMMENDED, `expires_in` definitional gap,
`SameSite` guidance — are below MUST threshold. CONCERN.

---

## Pair[CB4-3.4-1212]/A

An OIDC provider has never formally documented security requirements for its
authorization server (NIST SSDF PO.1 gap). A compliance review additionally
confirms that the AS currently serves its UserInfo endpoint over plain HTTP —
`http://as.example.com/userinfo` — in production. The team classifies both
findings as low-severity operational notes, reasoning that PO.1 is a process
gap and that the UserInfo endpoint only exposes user profile data, not credentials.

## Pair[CB4-3.4-1212]/B

REPROCESS. PO.1 absence alone constitutes a CONCERN. However, the PO.1 escalation
rule applies: when PO.1 absence is traceable to an active production MUST violation,
it escalates to REPROCESS. OIDC Core §6 MUST HTTPS is unconditional and covers all
AS endpoints — including UserInfo; the specification does not limit the TLS
requirement to credential-handling endpoints. The UserInfo endpoint transmits user
profile PII (name, email, phone, address) over plaintext, enabling on-path
interception and Bearer token replay. The team's endpoint-scoping argument is
rejected: OIDC Core §6 MUST covers all AS endpoints. PO.1 gap escalates because
the HTTPS obligation was never documented or tracked. REPROCESS.

## Pair[CB4-3.4-1212]/Reason

PO.1 gap (CONCERN alone) + active production MUST violation (OIDC Core §6 MUST
HTTPS; UserInfo on plain HTTP) = PO.1 escalation rule → REPROCESS. Endpoint-scoping
argument rejected.

## Pair[CB4-3.4-1212]/Phrasing-B

Verdict REPROCESS. PO.1 escalation rule: PO.1 absence is CONCERN in isolation, but
escalates to REPROCESS when traceable to an active production MUST violation.
OIDC Core §6 unconditionally requires HTTPS for all AS endpoints; serving UserInfo
over HTTP violates this MUST while transmitting PII. The team's credential-endpoint
scoping argument is rejected; OIDC Core §6 draws no such distinction. REPROCESS.

---

## Pair[CB4-3.4-1213]/A

An AS issues session cookies for the authorization flow without an explicit
`SameSite` attribute (browser default `Lax`). The same authorization server uses
an 8-bit sequential counter for JWT `jti` generation — values cycle every 256
tokens within the 15-minute TTL window, producing `jti` collisions in the Redis
blocklist. A reviewer argues the cookie observation is low severity and questions
whether it moderates the jti finding.

## Pair[CB4-3.4-1213]/B

REPROCESS. The `SameSite` absence is CONCERN-level (ASVS V7.2 guidance; no
unconditional RFC MUST for a specific value; browser-default `Lax` provides
partial protection). The jti counter cycling within the TTL window is REPROCESS-
level: RFC 7519 §4.1.7 MUST requires jti to be unique when used for replay
prevention; an 8-bit counter that wraps within the token lifetime generates
colliding jti values in the blocklist, collapsing the revocation model. REPROCESS
overrides CONCERN — the concurrent guidance-level cookie finding does not moderate
the verdict. REPROCESS.

## Pair[CB4-3.4-1213]/Reason

SameSite absent (ASVS V7.2 guidance, CONCERN) + jti cycling within TTL (RFC 7519
§4.1.7 MUST unique jti → revocation collapse, REPROCESS); REPROCESS overrides
CONCERN → REPROCESS.

## Pair[CB4-3.4-1213]/Phrasing-B

Verdict REPROCESS. SameSite absence is a guidance gap (CONCERN). The jti counter
wrapping within the TTL window violates RFC 7519 §4.1.7 MUST — non-unique jti
values render the blocklist unable to distinguish between tokens, disabling replay
prevention. REPROCESS overrides all concurrent CONCERN findings; the cookie guidance
gap does not moderate the verdict. REPROCESS.

---

## Pair[CB4-3.4-1214]/A

An AS token endpoint returns `"expires_in": 3600` when the actual token lifetime
is 900 seconds, causing clients to cache tokens past their valid window and
producing silent 401 failures. The same AS reflects the `login_hint` query
parameter directly into the HTML value attribute of the username input field
without output encoding. The team asks whether the expires_in discrepancy
provides any compensating context for the login_hint handling.

## Pair[CB4-3.4-1214]/B

REPROCESS. The `expires_in` mismatch is CONCERN-level (RFC 6749 §5.1 definitional
gap; no explicit MUST for accuracy when the field is present; operational impact
is real but the normative anchor does not rise to MUST). The unencoded `login_hint`
rendered into HTML is REPROCESS-level: CWE-79 (Cross-Site Scripting) and ASVS
V5.3.3 unconditionally MUST HTML-encode all user-controlled input rendered in HTML
contexts; XSS at the AS origin enables session theft, token theft, and authorization
flow manipulation. A concurrent CONCERN cannot moderate or offset a MUST violation.
REPROCESS overrides CONCERN. REPROCESS.

## Pair[CB4-3.4-1214]/Reason

expires_in mismatch (RFC 6749 §5.1 definitional gap, CONCERN) + login_hint XSS
(CWE-79 / ASVS V5.3.3 MUST HTML-encode, REPROCESS); REPROCESS overrides CONCERN;
compensation argument rejected → REPROCESS.

## Pair[CB4-3.4-1214]/Phrasing-B

Verdict REPROCESS. The expires_in/lifetime mismatch is a definitional gap at
CONCERN level. XSS via unencoded `login_hint` violates ASVS V5.3.3 MUST — an
unconditional requirement to output-encode user-controlled input before HTML
rendering. REPROCESS overrides CONCERN; the guidance gap cannot moderate a MUST
violation. REPROCESS.

---

## Pair[CB4-3.4-1215]/A

An OIDC provider's discovery document omits `claims_supported`. The same provider
accepts JWT client assertions with `alg: none` in the JOSE header — unsigned
assertions — and issues access tokens to any caller that presents the correct
`client_id` in the assertion payload. The team argues the discovery omission is
mitigated because clients cannot verify the claim set anyway, and that `client_id`
presence in an unsigned assertion is sufficient to identify the client.

## Pair[CB4-3.4-1215]/B

REPROCESS. `claims_supported` absence is CONCERN-level (OIDC Core Discovery §3
RECOMMENDED, not REQUIRED). Accepting JWT client assertions with `alg: none` is
REPROCESS-level: RFC 8725 §2.1 MUST NOT applies universally to signed JWTs —
including JWT client assertions under RFC 7523. An unsigned assertion provides no
cryptographic proof of key possession; any party knowing the `client_id` can forge
and submit an unsigned assertion to impersonate a confidential client at the token
endpoint. The discovery-offset and client_id-sufficiency arguments are both rejected:
a RECOMMENDED-level gap cannot moderate an unconditional MUST NOT violation; the
purpose of `private_key_jwt` authentication is to prove key possession, which an
unsigned assertion cannot demonstrate. REPROCESS overrides CONCERN. REPROCESS.

## Pair[CB4-3.4-1215]/Reason

claims_supported absent (OIDC Discovery §3 RECOMMENDED, CONCERN) + alg:none client
assertion accepted (RFC 8725 §2.1 MUST NOT, REPROCESS); offset argument rejected;
REPROCESS overrides CONCERN → REPROCESS.

## Pair[CB4-3.4-1215]/Phrasing-B

Verdict REPROCESS. Discovery's `claims_supported` absence is RECOMMENDED-level
(CONCERN). Accepting `alg=none` JWT client assertions violates RFC 8725 §2.1
MUST NOT unconditionally — no signed JWT context is exempt. The team's discovery-
offset and client_id-sufficiency arguments are rejected: a guidance gap cannot
moderate a MUST NOT violation. REPROCESS overrides CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1216]/A

AS session cookies for the authorization flow are issued without an explicit
`SameSite` attribute (browser default `Lax`). The same AS token endpoint does
not verify that the authorization code presented was issued to the authenticated
client — client B can exchange a code intercepted from client A and receive an
access token. The team argues that browser-default `Lax` SameSite behavior limits
the attack surface sufficiently to compensate for the code-binding gap.

## Pair[CB4-3.4-1216]/B

REPROCESS. The absent explicit `SameSite` is CONCERN-level (ASVS V7.2 guidance;
no unconditional RFC MUST for a specific value). The authorization code–client
binding absence is REPROCESS-level: RFC 6749 §4.1.3 MUST requires the AS to
ensure the authorization code was issued to the authenticated client presenting
it at the token endpoint; issuing an access token to a client that did not receive
the code violates this MUST unconditionally. The compensation argument is rejected
on two grounds: (1) `SameSite` is a guidance-level attribute that cannot substitute
for the §4.1.3 binding check; (2) REPROCESS overrides CONCERN — concurrent
guidance gaps do not moderate MUST violations. REPROCESS.

## Pair[CB4-3.4-1216]/Reason

SameSite absent (ASVS V7.2 guidance, CONCERN) + auth code client binding absent
(RFC 6749 §4.1.3 MUST, REPROCESS); compensation argument rejected; REPROCESS
overrides CONCERN → REPROCESS.

## Pair[CB4-3.4-1216]/Phrasing-B

Verdict REPROCESS. SameSite absence is a guidance gap (CONCERN). Failure to bind
authorization codes to the requesting client violates RFC 6749 §4.1.3 MUST — the
binding check is unconditional; `SameSite` enforcement addresses CSRF, not
cross-client code exchange. REPROCESS overrides CONCERN; the guidance gap does not
moderate the verdict. REPROCESS.

---

## Pair[CB4-3.4-1217]/A

An OIDC authorization server is reviewed against all eight control points from
the ext46 batch: jti generation uses UUID v4 (globally unique per token);
`claims_supported` is fully populated in the discovery document; `expires_in`
in token responses matches the JWT `exp` lifetime (900 seconds); all user-
controlled query parameters including `login_hint` are HTML-encoded before page
rendering; all AS session cookies carry an explicit `SameSite=Strict` attribute;
the token endpoint verifies that authorization codes are only exchanged by the
client to which they were issued; JWT client assertions with `alg: none` are
rejected at the token endpoint; and all AS endpoints including UserInfo are
served exclusively over HTTPS. No anomalies are detected.

## Pair[CB4-3.4-1217]/B

PASS. All eight ext46 control points are satisfied: (1) UUID v4 jti satisfies
RFC 7519 §4.1.7 uniqueness MUST; (2) `claims_supported` present satisfies
OIDC Core Discovery §3 RECOMMENDED; (3) `expires_in` matches actual lifetime
per RFC 6749 §5.1; (4) output encoding of `login_hint` satisfies CWE-79 /
ASVS V5.3.3 MUST; (5) explicit `SameSite=Strict` satisfies ASVS V7.2 guidance;
(6) authorization code–client binding enforced per RFC 6749 §4.1.3 MUST;
(7) `alg: none` rejection satisfies RFC 8725 §2.1 MUST NOT; (8) HTTPS on all
endpoints including UserInfo satisfies OIDC Core §6 MUST. No violations or
guidance gaps found. PASS.

## Pair[CB4-3.4-1217]/Reason

All eight ext46 controls compliant: RFC 7519 §4.1.7 (jti), OIDC Discovery §3
(claims_supported), RFC 6749 §5.1 (expires_in accuracy), CWE-79 / ASVS V5.3.3
(output encoding), ASVS V7.2 (SameSite=Strict), RFC 6749 §4.1.3 (code binding),
RFC 8725 §2.1 (alg:none), OIDC Core §6 (HTTPS all endpoints) → PASS.

## Pair[CB4-3.4-1217]/Phrasing-B

Verdict PASS. The AS satisfies all eight ext46 control points: unique jti
(RFC 7519 §4.1.7), claims_supported present (OIDC Discovery §3), accurate
expires_in (RFC 6749 §5.1), HTML-encoded login_hint (ASVS V5.3.3), explicit
SameSite=Strict (ASVS V7.2), code-to-client binding enforced (RFC 6749 §4.1.3),
alg:none rejected (RFC 8725 §2.1), and HTTPS on all endpoints (OIDC Core §6).
PASS.

---

## Pair[CB4-3.4-1218]/A

An AS token endpoint returns `"expires_in": 3600` when the actual JWT `exp`
lifetime is 900 seconds, causing clients to cache tokens past their valid window.
The same AS issues session cookies without an explicit `SameSite` attribute.
During a code review, an engineer asserts that two guidance-level findings in
combination should trigger a mandatory REPROCESS escalation path due to their
compounded operational risk.

## Pair[CB4-3.4-1218]/B

CONCERN. Two guidance-level findings: (1) `expires_in` mismatch — RFC 6749 §5.1
defines `expires_in` as the actual token lifetime but provides no explicit MUST for
accuracy when the field is present; the normative gap is definitional; (2) no
explicit `SameSite` attribute — ASVS V7.2 / OWASP guidance recommends `SameSite=Strict`;
no unconditional RFC MUST mandates a specific value; browser-default `Lax` provides
partial CSRF protection. The engineer's mandatory escalation assertion is rejected:
the graduation rule is categorical and admits no exceptions based on finding count,
operational severity, or compounding risk. N CONCERNs = CONCERN. CONCERN.

## Pair[CB4-3.4-1218]/Reason

expires_in mismatch (RFC 6749 §5.1 definitional gap, CONCERN) + SameSite absent
(ASVS V7.2 guidance, CONCERN); graduation rule categorical: N CONCERNs ≠ REPROCESS
regardless of count or compounded risk; escalation argument rejected → CONCERN.

## Pair[CB4-3.4-1218]/Phrasing-B

Verdict CONCERN. Both findings are below MUST threshold: `expires_in` definitional
gap (RFC 6749 §5.1) and `SameSite` guidance gap (ASVS V7.2). The graduation rule
admits no count-based or risk-compounding exception — two guidance-level findings
remain CONCERN. The engineer's mandatory escalation argument is rejected. CONCERN.
