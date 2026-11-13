# Node Cross-Source Set Extension 40
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1235–1244 (20 instances — 10 base pairs × 2 phrasings)
# Sources: RFC 6749 §4.1.1, OIDC Core §3.1.3.7.3, RFC 9700 §4.1.3,
#          ASVS V9.2.2, RFC 6749 §3.3, OIDC Core §2,
#          RFC 6750 §2.3, RFC 8414 §2, NIST SSDF PO.1
# Running total after this file: 2488 instances (1244 base pairs)
# Formula check: 120 + (1244 − 60) × 2 = 120 + 2368 = 2488 ✓

---

## Pair[CB4-3.4-1235]/A

An OIDC provider issues ID tokens with a multi-valued `aud` array but omits the
`azp` claim. Separately, a service using the `client_credentials` grant omits the
`scope` parameter from its token request, and the AS issues a token with all five
registered scopes as the default. A combined security review flags both observations
and asks whether together they constitute a REPROCESS finding.

## Pair[CB4-3.4-1235]/B

CONCERN. Two guidance-level observations: (1) `azp` absent when `aud` is multi-
valued — OIDC Core §2 SHOULD be present in the multi-audience case; no unconditional
MUST; authorized-party ambiguity is a guidance gap; (2) `client_credentials` token
issued with all registered scopes when scope is absent from the request — RFC 6749
§3.3 explicitly permits AS to default to pre-established registered scopes when scope
is omitted; least-privilege guidance recommends explicit minimal scope but no MUST
is violated. Both findings are guidance-level. The graduation rule applies: N
CONCERNs do not escalate to REPROCESS regardless of count. CONCERN.

## Pair[CB4-3.4-1235]/Reason

Two guidance-level gaps: azp absent with multi-valued aud (OIDC Core §2 SHOULD)
+ scope absent defaults to max registered (RFC 6749 §3.3 permitted); graduation
rule: N CONCERNs ≠ REPROCESS → CONCERN.

## Pair[CB4-3.4-1235]/Phrasing-B

Verdict CONCERN. `azp` absence is OIDC Core §2 SHOULD-level. Default scope
issuance on absent scope request is explicitly permitted by RFC 6749 §3.3. Two
guidance-level observations do not aggregate to REPROCESS. CONCERN.

---

## Pair[CB4-3.4-1236]/A

An API client submits access tokens to the OIDC UserInfo endpoint via URL query
parameter rather than the Authorization header. In the same deployment, the
`client_credentials` service omits `scope` from its token requests and the AS
issues maximum registered scope by default. Both patterns are identified in a code
review. The reviewer argues the two gaps reinforce each other and should together
constitute a REPROCESS.

## Pair[CB4-3.4-1236]/B

CONCERN. Two guidance-level observations: (1) UserInfo query-parameter token
submission — RFC 6750 §2.3 SHOULD NOT; last resort only; token appears in server
logs; no unconditional MUST NOT; (2) `client_credentials` scope default — RFC 6749
§3.3 explicitly permits default scope issuance; least-privilege guidance gap; no
MUST violated. The reviewer's reinforcement argument is rejected: the graduation
rule is categorical. N CONCERNs do not escalate to REPROCESS regardless of how the
findings interact operationally. Both observations remain guidance-level. CONCERN.

## Pair[CB4-3.4-1236]/Reason

UserInfo query-parameter submission (RFC 6750 §2.3 SHOULD NOT) + scope absent
defaults to max (RFC 6749 §3.3 permitted); both guidance-level; graduation rule
categorical: N CONCERNs ≠ REPROCESS → CONCERN.

## Pair[CB4-3.4-1236]/Phrasing-B

Verdict CONCERN. RFC 6750 §2.3 SHOULD NOT applies to query-parameter submission.
RFC 6749 §3.3 permits scope defaulting. Neither has an unconditional MUST anchor.
Reinforcement argument rejected; graduation rule applies. CONCERN.

---

## Pair[CB4-3.4-1237]/A

A comprehensive OIDC deployment review surfaces three guidance-level observations
simultaneously: ID tokens with multi-valued aud omit the `azp` claim; the UserInfo
endpoint is accessed via URL query parameter in one integration; and `client_credentials`
token requests omit the `scope` parameter, receiving full registered scope by default.
An auditor argues that three concurrent guidance gaps in a production OIDC deployment
should trigger a REPROCESS escalation for systemic security posture deficiency.

## Pair[CB4-3.4-1237]/B

CONCERN. All three observations are guidance-level: (1) `azp` absent in multi-
audience ID tokens — OIDC Core §2 SHOULD; no unconditional MUST; (2) UserInfo
query-parameter submission — RFC 6750 §2.3 SHOULD NOT; no MUST NOT; (3) scope-
absent `client_credentials` request defaulting to maximum registered — RFC 6749
§3.3 explicitly permits this behavior. The auditor's systemic-posture escalation
argument is rejected: the graduation rule is unconditional and categorical — N
CONCERNs = CONCERN regardless of count, operational overlap, or systemic framing.
Only a MUST-level violation can produce REPROCESS. CONCERN.

## Pair[CB4-3.4-1237]/Reason

Three guidance-level gaps: azp absent (OIDC Core §2 SHOULD) + UserInfo query
param (RFC 6750 §2.3 SHOULD NOT) + scope absent defaults (RFC 6749 §3.3 permitted);
graduation rule: 3 CONCERNs ≠ REPROCESS; systemic-posture argument rejected →
CONCERN.

## Pair[CB4-3.4-1237]/Phrasing-B

Verdict CONCERN. Three guidance-level gaps do not aggregate to REPROCESS. The
graduation rule admits no count-based, systemic-posture, or operational-overlap
exception. All three findings are below MUST threshold. CONCERN.

---

## Pair[CB4-3.4-1238]/A

An OIDC deployment has no formal documentation of security requirements for its
authorization server (NIST SSDF PO.1 gap). A concurrent investigation finds that
the AS accepts authorization requests that omit the `response_type` parameter,
inferring `response_type=code` rather than returning `error=invalid_request`. The
team treats both findings as isolated observations — PO.1 as a process gap and the
response_type handling as a developer convenience feature.

## Pair[CB4-3.4-1238]/B

REPROCESS. PO.1 absence alone is CONCERN. However, the PO.1 escalation rule applies:
PO.1 absence combined with a traceable production MUST violation escalates to
REPROCESS. RFC 6749 §4.1.1 designates `response_type` as a REQUIRED parameter;
RFC 6749 §4.1.2.1 MUST: the AS must return `error=invalid_request` when a REQUIRED
parameter is absent. The AS's inference behavior is an active MUST violation in
production. The PO.1 gap means the REQUIRED parameter enforcement obligation was
never formally documented or tracked. The team's framing of the response_type
handling as a "developer convenience feature" is rejected — REQUIRED parameter
enforcement is not optional. REPROCESS.

## Pair[CB4-3.4-1238]/Reason

PO.1 gap (CONCERN alone) + active MUST violation (RFC 6749 §4.1.1 + §4.1.2.1:
response_type REQUIRED; AS must return error=invalid_request, not infer) =
PO.1 escalation → REPROCESS.

## Pair[CB4-3.4-1238]/Phrasing-B

Verdict REPROCESS. PO.1 escalation rule: PO.1 absence escalates to REPROCESS when
traceable to an active production MUST violation. RFC 6749 §4.1.1 + §4.1.2.1 MUST:
absent response_type = error=invalid_request; inference is a MUST violation. The
convenience-feature framing is rejected. REPROCESS.

---

## Pair[CB4-3.4-1239]/A

An AS issues `client_credentials` tokens with all registered scopes when the scope
parameter is absent from the token request. The same AS stores all registered client
secrets as plaintext strings in its database. The team argues the scope behavior is
standards-compliant and focuses only on the secrets storage as a concern.

## Pair[CB4-3.4-1239]/B

REPROCESS. The `scope` behavior is CONCERN-level (RFC 6749 §3.3 explicitly permits
default scope issuance; guidance gap only). However, the plaintext client secret
storage is REPROCESS-level: ASVS V9.2.2 MUST requires client secrets to be stored
hashed using an approved algorithm; plaintext storage means any database read access
yields all secrets immediately. REPROCESS overrides CONCERN — the concurrent
guidance-level scope finding does not moderate the MUST violation. The team's
correct identification of the secrets storage as a concern does not change the
overall verdict: when any MUST-level finding is present, the verdict is REPROCESS.
REPROCESS.

## Pair[CB4-3.4-1239]/Reason

scope absent defaults to max (RFC 6749 §3.3 permitted, CONCERN) + client secret
plaintext storage (ASVS V9.2.2 MUST hash, REPROCESS); REPROCESS overrides CONCERN
→ REPROCESS.

## Pair[CB4-3.4-1239]/Phrasing-B

Verdict REPROCESS. Scope defaulting is CONCERN-level (RFC 6749 §3.3). Plaintext
client secret storage violates ASVS V9.2.2 MUST unconditionally. REPROCESS overrides
CONCERN; the guidance gap does not moderate the MUST violation. REPROCESS.

---

## Pair[CB4-3.4-1240]/A

An OIDC provider issues ID tokens with multi-valued aud arrays but omits the azp
claim. The same provider has registered a confidential web application client with
a redirect_uri using the plain `http://` scheme rather than `https://`. The team
notes that the `azp` observation is the "lower severity" finding and asks whether
it somehow offsets the redirect_uri concern.

## Pair[CB4-3.4-1240]/B

REPROCESS. The `azp` absence is CONCERN-level (OIDC Core §2 SHOULD; no unconditional
MUST). The `http://` redirect_uri registration for a non-native web client is
REPROCESS-level: RFC 9700 §4.1.3 MUST requires redirect URIs for non-native clients
to use the HTTPS scheme; the registered `http://` URI violates this unconditionally.
The team's offset argument is rejected: CONCERN and REPROCESS findings are
independent; a lower-severity guidance gap cannot offset or moderate a MUST
violation. REPROCESS overrides CONCERN. REPROCESS.

## Pair[CB4-3.4-1240]/Reason

azp absent with multi-valued aud (OIDC Core §2 SHOULD, CONCERN) + web client
http:// redirect_uri (RFC 9700 §4.1.3 MUST HTTPS, REPROCESS); offset argument
rejected; REPROCESS overrides CONCERN → REPROCESS.

## Pair[CB4-3.4-1240]/Phrasing-B

Verdict REPROCESS. azp absence is SHOULD-level CONCERN. Web client http:// redirect_uri
violates RFC 9700 §4.1.3 MUST. Offset argument rejected. REPROCESS overrides CONCERN.
REPROCESS.

---

## Pair[CB4-3.4-1241]/A

An API client submits access tokens to the UserInfo endpoint via URL query parameter.
The same OIDC provider issues ID tokens that omit the `aud` claim entirely. The team
asks whether the query-parameter handling partially compensates for the absent `aud`
by limiting the scope of tokens that can be replayed.

## Pair[CB4-3.4-1241]/B

REPROCESS. The UserInfo query-parameter submission is CONCERN-level (RFC 6750 §2.3
SHOULD NOT; last resort; no unconditional MUST NOT). The absent `aud` in ID tokens
is REPROCESS-level: OIDC Core §3.1.3.7.3 MUST requires the `aud` claim to contain
the requesting client's `client_id` in every ID token; absent `aud` enables cross-
client token confusion at the same issuer. The compensation argument is rejected:
the two findings concern different attack surfaces (token transmission and token
content respectively); a guidance gap in one cannot limit the impact of a MUST
violation in the other. REPROCESS overrides CONCERN. REPROCESS.

## Pair[CB4-3.4-1241]/Reason

UserInfo query-parameter submission (RFC 6750 §2.3 SHOULD NOT, CONCERN) + ID token
aud absent (OIDC Core §3.1.3.7.3 MUST, REPROCESS); compensation argument rejected;
REPROCESS overrides CONCERN → REPROCESS.

## Pair[CB4-3.4-1241]/Phrasing-B

Verdict REPROCESS. Query-parameter UserInfo submission is a guidance gap (CONCERN).
Absent `aud` in ID tokens violates OIDC Core §3.1.3.7.3 MUST. Compensation argument
rejected. REPROCESS overrides CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1242]/A

A `client_credentials` service omits `scope` from its token requests (AS defaults
to maximum registered scope). The same AS publishes a metadata document at
`/.well-known/oauth-authorization-server` that omits the `issuer` field. The team
classifies the scope behavior as permitted and argues the missing `issuer` field
is obvious from the document URL, treating both as guidance-level notes.

## Pair[CB4-3.4-1242]/B

REPROCESS. The scope-absent default is CONCERN-level (RFC 6749 §3.3: default scope
issuance explicitly permitted; least-privilege guidance gap). The absent `issuer`
field is REPROCESS-level: RFC 8414 §2 designates `issuer` as a REQUIRED field in
the AS metadata document; REQUIRED means MUST present; absent issuer makes the
document non-conformant. RFC 8414 §3 further specifies that the issuer value MUST
exactly match the well-known URL prefix; the inference argument is rejected because
RFC 8414 §3 requires the field to be explicitly present and verifiable by RPs.
REPROCESS overrides CONCERN. REPROCESS.

## Pair[CB4-3.4-1242]/Reason

scope absent defaults to max (RFC 6749 §3.3 permitted, CONCERN) + AS metadata
issuer absent (RFC 8414 §2 REQUIRED + §3 MUST, REPROCESS); inference argument
rejected; REPROCESS overrides CONCERN → REPROCESS.

## Pair[CB4-3.4-1242]/Phrasing-B

Verdict REPROCESS. Scope defaulting is permitted by RFC 6749 §3.3 (CONCERN). Absent
`issuer` violates RFC 8414 §2 REQUIRED + §3 MUST. The inference argument is rejected.
REPROCESS overrides CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1243]/A

An OIDC authorization server is reviewed against all eight ext47 control points:
authorization requests missing `response_type` are rejected with `error=invalid_request`;
all ID tokens include an `aud` claim containing the requesting client's `client_id`;
all non-native client redirect URIs are registered with `https://` schemes; client
secrets are stored hashed using Argon2id; `client_credentials` clients explicitly
specify minimal scope in their requests; ID tokens issued with multi-valued aud
arrays include an `azp` claim; all UserInfo requests use the `Authorization: Bearer`
header; and the AS metadata document at `/.well-known/oauth-authorization-server`
includes a correctly formatted `issuer` field. No anomalies are detected.

## Pair[CB4-3.4-1243]/B

PASS. All eight ext47 control points are satisfied: (1) absent `response_type`
triggers `error=invalid_request` per RFC 6749 §4.1.1 + §4.1.2.1; (2) ID token
`aud` contains `client_id` per OIDC Core §3.1.3.7.3 MUST; (3) non-native redirect
URIs use HTTPS per RFC 9700 §4.1.3 MUST; (4) client secrets hashed with Argon2id
per ASVS V9.2.2 MUST; (5) explicit minimal scope in `client_credentials` requests
satisfies RFC 9700 least-privilege guidance; (6) `azp` present when aud is multi-
valued satisfies OIDC Core §2 SHOULD; (7) `Authorization: Bearer` header at UserInfo
satisfies RFC 6750 §2.1 primary method and OIDC Core §5.3.1; (8) `issuer` field
present in AS metadata satisfies RFC 8414 §2 REQUIRED + §3 MUST. No violations or
guidance gaps found. PASS.

## Pair[CB4-3.4-1243]/Reason

All eight ext47 controls compliant: RFC 6749 §4.1.1+§4.1.2.1 (response_type),
OIDC Core §3.1.3.7.3 (aud), RFC 9700 §4.1.3 (redirect_uri HTTPS), ASVS V9.2.2
(client secret hashing), RFC 6749 §3.3 (explicit scope), OIDC Core §2 (azp),
RFC 6750 §2.1 (Bearer header), RFC 8414 §2+§3 (issuer) → PASS.

## Pair[CB4-3.4-1243]/Phrasing-B

Verdict PASS. All eight ext47 control points satisfied: response_type validation
(RFC 6749 §4.1.1), ID token aud (OIDC Core §3.1.3.7.3), HTTPS redirect URIs
(RFC 9700 §4.1.3), hashed client secrets (ASVS V9.2.2), explicit minimal scope,
azp with multi-valued aud (OIDC Core §2), Bearer header at UserInfo (RFC 6750 §2.1),
and issuer in metadata (RFC 8414 §2 + §3). PASS.

---

## Pair[CB4-3.4-1244]/A

An OIDC provider issues ID tokens with multi-valued aud arrays but omits the `azp`
claim. The same provider's API integration documentation encourages using URL query
parameters to submit access tokens to the UserInfo endpoint for "simplicity."
A developer following the documentation submits tokens via query parameter; these
tokens appear in full in server-side logs. An auditor argues that two documented
guidance deficiencies — one in the token issuance and one in the integration guide —
together justify a mandatory REPROCESS escalation.

## Pair[CB4-3.4-1244]/B

CONCERN. Two guidance-level findings: (1) `azp` absent when aud is multi-valued —
OIDC Core §2 SHOULD; no unconditional MUST; authorized-party ambiguity is a guidance
gap; (2) access token via URL query parameter — RFC 6750 §2.3 SHOULD NOT (last
resort); token in server logs is a meaningful risk but no unconditional MUST NOT
prohibition applies. The auditor's mandatory-escalation argument is rejected: the
graduation rule is unconditional regardless of whether the gaps appear in
implementation or documentation. N CONCERNs = CONCERN. CONCERN.

## Pair[CB4-3.4-1244]/Reason

azp absent with multi-valued aud (OIDC Core §2 SHOULD, CONCERN) + UserInfo query-
parameter submission (RFC 6750 §2.3 SHOULD NOT, CONCERN); graduation rule
unconditional; documentation-gap framing does not change classification → CONCERN.

## Pair[CB4-3.4-1244]/Phrasing-B

Verdict CONCERN. Both findings are below MUST threshold: `azp` absence is OIDC
Core §2 SHOULD-level, and query-parameter token submission is RFC 6750 §2.3
SHOULD NOT. The graduation rule admits no documentation-gap escalation exception.
N CONCERNs = CONCERN. CONCERN.
