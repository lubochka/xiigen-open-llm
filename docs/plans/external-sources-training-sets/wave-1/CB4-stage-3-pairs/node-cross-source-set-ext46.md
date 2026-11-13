# Node Cross-Source Set Extension 46
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1391–1400 (20 instances — 10 base pairs × 2 phrasings)
# Sources: RFC 8693 §2.1, RFC 9101 §4 / RFC 9126,
#          OIDC Back-Channel Logout §2.6, RFC 6749 §4.1.1,
#          OIDC Core §3.1.3.7.6, RFC 7591 §2,
#          RFC 8628 §3.5, OIDC Core §3.1.3.7.13,
#          NIST SSDF PO.1, RFC 6749 §5.1
# Running total after this file: 2800 instances (1400 base pairs)
# Formula check: 120 + (1400 − 60) × 2 = 120 + 2680 = 2800 ✓

---

## Pair[CB4-3.4-1391]/A

An OIDC relying party omits iat-staleness validation from its ID token verification
pipeline. The same AS's dynamic client registration endpoint omits the
`token_endpoint_auth_method` field from registration responses. Both gaps are
noted in an integration review. The reviewer argues that two ID token and
registration lifecycle gaps together constitute a combined conformance deficiency
requiring REPROCESS.

## Pair[CB4-3.4-1391]/B

CONCERN. Two guidance-level observations: (1) iat staleness not checked — OIDC
Core §3.1.3.7.6 provides descriptive guidance; no MUST mandates a maximum iat age;
exp is the normative time boundary; (2) token_endpoint_auth_method absent from
registration response — RFC 7591 §2 SHOULD; defined default (client_secret_basic)
applies when absent; no MUST. The graduation rule applies: N CONCERNs = CONCERN
regardless of count, lifecycle framing, or conformance-deficiency arguments.
Neither finding has a MUST-level anchor. CONCERN.

## Pair[CB4-3.4-1391]/Reason

Two guidance-level gaps: iat staleness not checked (OIDC Core §3.1.3.7.6 guidance)
+ token_endpoint_auth_method absent (RFC 7591 §2 SHOULD); graduation rule →
CONCERN.

## Pair[CB4-3.4-1391]/Phrasing-B

Verdict CONCERN. Both findings are guidance-level. The graduation rule forecloses
combined conformance-deficiency escalation. CONCERN.

---

## Pair[CB4-3.4-1392]/A

An authorization server's device flow response omits the `interval` field. The
same AS's OIDC RPs do not validate returned `acr` values against the `acr_values`
requested in authorization requests. Both gaps are identified in a security review.
A reviewer argues that two validation omissions — polling control and authentication
context — constitute a pattern of insufficient validation requiring REPROCESS.

## Pair[CB4-3.4-1392]/B

CONCERN. Two guidance-level observations: (1) device flow interval absent — RFC
8628 §3.5 OPTIONAL; no MUST; 5-second default applies for client implementations;
(2) acr not validated against acr_values — OIDC Core §3.1.3.7.13 SHOULD; no
unconditional MUST; access control implications real but guidance-level. The
graduation rule is categorical: N CONCERNs = CONCERN regardless of validation-
pattern framing, thematic overlap, or count. No MUST-level violation is identified.
CONCERN.

## Pair[CB4-3.4-1392]/Reason

Two SHOULD/guidance-level gaps: device flow interval absent (RFC 8628 §3.5
OPTIONAL) + acr not validated (OIDC Core §3.1.3.7.13 SHOULD); graduation rule →
CONCERN.

## Pair[CB4-3.4-1392]/Phrasing-B

Verdict CONCERN. Both findings are guidance-level. The graduation rule forecloses
validation-pattern escalation. CONCERN.

---

## Pair[CB4-3.4-1393]/A

An OIDC relying party omits iat-staleness checks for ID token validation. The
same AS does not include `refresh_token_expires_in` in token responses. Both gaps
are identified in a compliance scan. The compliance team argues that two token
freshness and lifecycle disclosure gaps together constitute a REPROCESS-level
combined finding.

## Pair[CB4-3.4-1393]/B

CONCERN. Two guidance-level observations: (1) iat staleness not checked — OIDC
Core §3.1.3.7.6 guidance; no MUST for maximum iat age; exp is the normative
boundary; (2) refresh_token_expires_in absent — RFC 6749 §5.1 does not list this
field as required or recommended; no MUST mandates it. The graduation rule is
categorical: N CONCERNs = CONCERN regardless of thematic clustering (both concern
token freshness/disclosure), compliance framing, or count. CONCERN.

## Pair[CB4-3.4-1393]/Reason

Two guidance-level gaps: iat staleness not checked (OIDC Core guidance) +
refresh_token_expires_in absent (RFC 6749 §5.1 not required); graduation rule
categorical → CONCERN.

## Pair[CB4-3.4-1393]/Phrasing-B

Verdict CONCERN. Both findings are guidance-level. The graduation rule forecloses
thematic-clustering escalation. CONCERN.

---

## Pair[CB4-3.4-1394]/A

An OIDC provider has no formal security requirements documentation (NIST SSDF
PO.1 gap). A concurrent penetration test confirms that the AS's authorization
endpoint accepts authorization requests that omit the `client_id` parameter —
the AS infers the client from the redirect_uri lookup and proceeds with the
flow. This behavior is active in production. The team classifies PO.1 as a
documentation backlog item and the client_id gap as a convenience feature.

## Pair[CB4-3.4-1394]/B

REPROCESS. PO.1 absence alone is CONCERN. The PO.1 escalation rule applies: PO.1
absence combined with a traceable production MUST violation escalates to REPROCESS.
RFC 6749 §4.1.1 REQUIRED: the client_id parameter is REQUIRED in authorization
requests; the AS must return error=invalid_request when it is absent. The AS
actively accepts client_id-absent requests in production via redirect_uri
inference — a live REQUIRED (MUST) violation. The PO.1 gap means the client_id
requirement was never formally tracked as a mandatory control. The documentation-
backlog and convenience-feature framings are rejected: PO.1 escalation applies.
REPROCESS.

## Pair[CB4-3.4-1394]/Reason

PO.1 gap (CONCERN alone) + active MUST violation (RFC 6749 §4.1.1 REQUIRED:
client_id must be in authorization request; AS accepts requests without client_id
in production) = PO.1 escalation → REPROCESS.

## Pair[CB4-3.4-1394]/Phrasing-B

Verdict REPROCESS. PO.1 escalation: PO.1 absence + traceable active MUST violation
= REPROCESS. RFC 6749 §4.1.1 REQUIRED is violated: client_id-absent authorization
requests accepted in production. REPROCESS.

---

## Pair[CB4-3.4-1395]/A

An OIDC relying party does not perform iat-staleness checks on ID tokens. The same
deployment's token exchange endpoint accepts token exchange requests that omit the
`subject_token_type` parameter. Both gaps are noted in a review. A developer asks
whether the iat gap and the token exchange gap are the same severity.

## Pair[CB4-3.4-1395]/B

REPROCESS. The iat-staleness gap is CONCERN-level (OIDC Core §3.1.3.7.6 guidance;
no MUST; exp is the normative boundary). The subject_token_type absence is REPROCESS-
level: RFC 8693 §2.1 REQUIRED — subject_token_type is a REQUIRED parameter in
token exchange requests; the AS must return error=invalid_request when absent; the
AS accepted the exchange request instead. REPROCESS overrides CONCERN — the
developer's question is answered: the token exchange gap is MUST-level; the iat
gap is guidance-level. Overall verdict: REPROCESS. REPROCESS.

## Pair[CB4-3.4-1395]/Reason

iat staleness not checked (OIDC Core guidance, CONCERN) + subject_token_type
absent (RFC 8693 §2.1 REQUIRED, REPROCESS); REPROCESS overrides CONCERN →
REPROCESS.

## Pair[CB4-3.4-1395]/Phrasing-B

Verdict REPROCESS. iat staleness gap is guidance-level (CONCERN). Absent
subject_token_type violates RFC 8693 §2.1 REQUIRED. REPROCESS overrides CONCERN.
REPROCESS.

---

## Pair[CB4-3.4-1396]/A

An AS's dynamic client registration response omits `token_endpoint_auth_method`.
The same AS accepts JAR request objects pushed via PAR without verifying the
JWT signatures. Both gaps are noted. The team asks whether the registration
response gap mitigates or is separate from the JAR signature verification issue.

## Pair[CB4-3.4-1396]/B

REPROCESS. The token_endpoint_auth_method absence is CONCERN-level (RFC 7591 §2
SHOULD; defined default applies; no MUST). The JAR signature non-verification is
REPROCESS-level: RFC 9101 §4 MUST — request object signatures must be verified by
the AS; PAR client authentication at the transport level does not substitute for
JWT payload signature verification. REPROCESS overrides CONCERN — the registration
response guidance gap has no bearing on the MUST violation in JAR processing.
The team is correct that they are independent findings; the JAR gap is REPROCESS.
REPROCESS.

## Pair[CB4-3.4-1396]/Reason

token_endpoint_auth_method absent (RFC 7591 §2 SHOULD, CONCERN) + JAR sig not
verified (RFC 9101 §4 MUST, REPROCESS); REPROCESS overrides CONCERN → REPROCESS.

## Pair[CB4-3.4-1396]/Phrasing-B

Verdict REPROCESS. Registration response guidance gap is CONCERN-level. JAR
signature non-verification violates RFC 9101 §4 MUST. REPROCESS overrides CONCERN.
REPROCESS.

---

## Pair[CB4-3.4-1397]/A

An AS's device flow response omits the `interval` field — clients poll at
whatever rate they choose. The same AS issues back-channel logout tokens that
include a `nonce` claim copied from the original authentication request. Both
gaps are identified in a protocol review. The team asks which gap is higher
severity.

## Pair[CB4-3.4-1397]/B

REPROCESS. The device flow interval absence is CONCERN-level (RFC 8628 §3.5
OPTIONAL; 5-second default applies; no MUST for the field). The nonce in the
logout token is REPROCESS-level: OIDC Back-Channel Logout §2.6 MUST NOT — logout
tokens must not contain a nonce claim; its presence enables token type confusion
between logout tokens and ID tokens; the MUST NOT is unconditional regardless of
intent. REPROCESS overrides CONCERN — the device flow gap is guidance-level; the
logout token nonce is a MUST NOT violation. REPROCESS.

## Pair[CB4-3.4-1397]/Reason

Device flow interval absent (RFC 8628 §3.5 OPTIONAL, CONCERN) + nonce in logout
token (OIDC BCL §2.6 MUST NOT, REPROCESS); REPROCESS overrides CONCERN →
REPROCESS.

## Pair[CB4-3.4-1397]/Phrasing-B

Verdict REPROCESS. Device flow interval absence is guidance-level (CONCERN). Nonce
in logout token violates OIDC BCL §2.6 MUST NOT. REPROCESS overrides CONCERN.
REPROCESS.

---

## Pair[CB4-3.4-1398]/A

An OIDC relying party does not validate returned `acr` values against requested
`acr_values`. The same AS's authorization endpoint accepts requests missing the
`client_id` parameter, inferring the client from the redirect_uri. Both gaps are
noted in a compliance audit. The team argues that the acr gap is higher severity
because it affects authentication assurance levels.

## Pair[CB4-3.4-1398]/B

REPROCESS. The acr validation gap is CONCERN-level (OIDC Core §3.1.3.7.13 SHOULD;
no MUST; real security impact at guidance level). The missing client_id acceptance
is REPROCESS-level: RFC 6749 §4.1.1 REQUIRED — client_id must be present in
authorization requests; the AS must return error=invalid_request when absent.
REPROCESS overrides CONCERN — the team's severity framing is partly inverted from
a normative perspective: the acr gap, while impactful, is SHOULD-level; the
client_id gap is REQUIRED (MUST). Overall verdict: REPROCESS. REPROCESS.

## Pair[CB4-3.4-1398]/Reason

acr not validated (OIDC Core §3.1.3.7.13 SHOULD, CONCERN) + client_id absent
accepted (RFC 6749 §4.1.1 REQUIRED, REPROCESS); REPROCESS overrides CONCERN;
severity framing partly corrected → REPROCESS.

## Pair[CB4-3.4-1398]/Phrasing-B

Verdict REPROCESS. acr validation is guidance-level (CONCERN). Accepting
client_id-absent requests violates RFC 6749 §4.1.1 REQUIRED. REPROCESS overrides
CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1399]/A

An OAuth/OIDC deployment is reviewed against all eight ext53 control points:
token exchange requests include `subject_token_type` per RFC 8693 §2.1; JAR
request objects pushed via PAR have their signatures verified per RFC 9101 §4;
back-channel logout tokens do not include a `nonce` claim per OIDC BCL §2.6;
authorization requests include `client_id` per RFC 6749 §4.1.1; the RP performs
iat-staleness checks within a 5-minute window as an additional defense layer;
dynamic client registration responses include `token_endpoint_auth_method`; device
flow responses include `interval=5`; and RPs validate returned `acr` against
requested `acr_values`. No anomalies are detected.

## Pair[CB4-3.4-1399]/B

PASS. All eight ext53 control points are satisfied: (1) subject_token_type present
per RFC 8693 §2.1 REQUIRED; (2) JAR signature verified per RFC 9101 §4 MUST;
(3) nonce absent from logout tokens per OIDC BCL §2.6 MUST NOT (satisfied); (4)
client_id present in authorization requests per RFC 6749 §4.1.1 REQUIRED; (5)
iat staleness check per OIDC Core §3.1.3.7.6 guidance (exceeds guidance floor);
(6) token_endpoint_auth_method in registration response per RFC 7591 §2 SHOULD
(exceeds SHOULD floor); (7) device flow interval present per RFC 8628 §3.5
guidance (exceeds guidance floor); (8) acr validated against acr_values per OIDC
Core §3.1.3.7.13 SHOULD (exceeds SHOULD floor). No violations or guidance gaps
found. PASS.

## Pair[CB4-3.4-1399]/Reason

All eight ext53 controls compliant: RFC 8693 §2.1 (subject_token_type), RFC 9101
§4 (JAR sig), OIDC BCL §2.6 (no nonce), RFC 6749 §4.1.1 (client_id), OIDC Core
§3.1.3.7.6 (iat), RFC 7591 §2 (auth method), RFC 8628 §3.5 (interval), OIDC Core
§3.1.3.7.13 (acr) → PASS.

## Pair[CB4-3.4-1399]/Phrasing-B

Verdict PASS. All eight ext53 control points satisfied: subject_token_type
(RFC 8693 §2.1), JAR signature (RFC 9101 §4), no nonce in logout (OIDC BCL §2.6),
client_id present (RFC 6749 §4.1.1), iat staleness, token auth method, device
interval, acr validation. PASS.

---

## Pair[CB4-3.4-1400]/A

An OIDC deployment has four concurrent guidance-level gaps: the RP omits
iat-staleness checks for ID tokens; dynamic client registration responses omit
`token_endpoint_auth_method`; device flow responses omit the `interval` field;
and OIDC relying parties do not validate returned `acr` against requested
`acr_values`. A chief security officer reviews all four gaps and argues that the
density of validation omissions across token freshness, registration, device
flows, and authentication context collectively requires REPROCESS escalation.

## Pair[CB4-3.4-1400]/B

CONCERN. Four guidance-level observations: (1) iat staleness not checked — OIDC
Core §3.1.3.7.6 guidance; (2) token_endpoint_auth_method absent — RFC 7591 §2
SHOULD; (3) device flow interval absent — RFC 8628 §3.5 OPTIONAL; (4) acr not
validated — OIDC Core §3.1.3.7.13 SHOULD. The graduation rule is categorical and
count-independent: N CONCERNs = CONCERN regardless of count, density framing,
cross-domain clustering, or seniority of the escalating authority. Four guidance-
level gaps are one CONCERN classification. Only a MUST-level violation triggers
REPROCESS. The CSO's escalation is rejected by the graduation rule. CONCERN.

## Pair[CB4-3.4-1400]/Reason

Four SHOULD/guidance-level gaps: iat staleness + auth method + device interval +
acr validation; graduation rule: N CONCERNs ≠ REPROCESS; density argument rejected
→ CONCERN.

## Pair[CB4-3.4-1400]/Phrasing-B

Verdict CONCERN. All four findings are guidance-level. The graduation rule is
count-independent — four CONCERNs equal one CONCERN. Cross-domain density
arguments are foreclosed by the graduation rule. CONCERN.
