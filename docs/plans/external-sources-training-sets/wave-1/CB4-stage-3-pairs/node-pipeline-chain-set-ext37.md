# Node Pipeline Chain Set — Extension 37
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1123–1130 (16 instances — 8 traces × 2 phrasings)
# Covers: Full 9-node pipeline traces for ext43 patterns
# Running total after this file: 2260 instances (1130 base pairs)
# Formula check: 120 + (1130 − 60) × 2 = 120 + 2140 = 2260 ✓

---

## Pair[CB4-3.4-1123]/A

Token endpoint receives a POST with `code`, `redirect_uri`, `client_id`, and
`client_secret` — `grant_type` parameter absent. AS defaults to
authorization_code and issues an access token.

## Pair[CB4-3.4-1123]/B

**Node 1 — Domain Splitter**
Input: Token endpoint request missing `grant_type`; AS infers grant and issues token.
Domain signal: OAuth 2.0 token endpoint; REQUIRED parameter handling.
Output → domain-token: `OAUTH2-TOKEN-ENDPOINT`; sub-domain: `grant-type-required-param`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: RFC 6749 (OAuth 2.0 Authorization Framework).
Scope signal: REQUIRED parameter processing; error handling obligation.
Output → sub-phase assignment: `3.4-OAUTH2-GRANT-TYPE-REQUIRED`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 6749 §4.1.3: `grant_type` REQUIRED in token request
- RFC 6749 §5.2: error=invalid_request for missing required parameter
Scheme: validate that AS rejects missing REQUIRED parameter; defaulting violates REQUIRED status.
Output → scheme: `REQUIRED-PARAM-REJECTION-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: `grant_type` absent; AS infers authorization_code from presence of `code`
parameter; MUST NOT substitute inference for explicit REQUIRED parameter.
Output → violation-record: `GRANT_TYPE_ABSENT_AS_INFERRED_UNAUTHORIZED`.

**Node 5 — Applier**
Applies RFC 6749 §4.1.3: REQUIRED parameter absent → AS MUST return error=invalid_request per §5.2.
Inference from other parameters does not satisfy REQUIRED status.
Output → application-result: `RFC6749-S4.1.3-MUST-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates application result: MUST violation (inference substitutes for REQUIRED parameter).
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: HIGH.

**Node 7 — Arbiter Selector**
Selects RFC 6749 REQUIRED-parameter arbiter rule.
Rule: `grant_type` REQUIRED → absence → error=invalid_request; no inference permitted.
Output → arbiter-rule: `RFC6749-S4.1.3-REQUIRED-PARAM-REJECT`.

**Node 8 — Arbiter**
Final determination: RFC 6749 §4.1.3 REQUIRED parameter absent; AS defaulted to
authorization_code in violation of the REQUIRED status; §5.2 mandates error=invalid_request.
Output → verdict: `REPROCESS`; citation: `RFC 6749 §4.1.3 REQUIRED / §5.2 invalid_request`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — RFC 6749 §4.1.3: `grant_type` is a REQUIRED parameter in token
requests. The authorization server MUST return `error=invalid_request` when it is absent
(§5.2); inferring the grant type from other parameters violates the REQUIRED status
unconditionally.

## Pair[CB4-3.4-1123]/Reason

RFC 6749 §4.1.3 REQUIRED; §5.2 error=invalid_request for missing required parameters;
AS MUST NOT infer grant type from co-present parameters.

## Pair[CB4-3.4-1123]/Phrasing-B

Verdict REPROCESS. Node 1 flags the missing `grant_type` as a REQUIRED-parameter gap.
Node 3 locks RFC 6749 §4.1.3 (REQUIRED) and §5.2 (error=invalid_request). Node 5 applies:
inference from the presence of `code` does not satisfy REQUIRED status — the parameter must
be explicit. Nodes 6–8 confirm REPROCESS: AS MUST reject with error=invalid_request; the
inference behavior substitutes AS judgment for explicit parameter semantics.

---

## Pair[CB4-3.4-1124]/A

Client sends authorization request with `request=` containing a JAR JWT
(RFC 9101) with `alg: none` header — unsigned. AS accepts the request
object and processes the authorization flow from the unsigned JWT claims.

## Pair[CB4-3.4-1124]/B

**Node 1 — Domain Splitter**
Input: JAR request object with alg:none accepted by AS.
Domain signal: OIDC/OAuth JAR; request object integrity; signed request mandate.
Output → domain-token: `OAUTH2-OIDC-JAR`; sub-domain: `request-object-alg-none-rejected`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: RFC 9101 (JAR — JWT-Secured Authorization Requests).
Scope signal: signed request object MUST NOT use alg=none; integrity protection mandate.
Output → sub-phase assignment: `3.4-JAR-ALG-NONE-PROHIBITION`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 9101 §10.2: MUST NOT use alg=none for signed request objects
Scheme: verify that AS rejects unsigned (alg:none) request objects per explicit MUST NOT.
Output → scheme: `JAR-ALG-NONE-REJECTION-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: `alg: none` in JAR JWT; AS accepted unsigned request object;
JAR purpose (integrity protection of authorization parameters) nullified by unsigned object.
Output → violation-record: `JAR_ALG_NONE_ACCEPTED_MUST_NOT_VIOLATED`.

**Node 5 — Applier**
Applies RFC 9101 §10.2 MUST NOT: alg=none prohibited for request objects unconditionally.
Accepting unsigned request object removes JAR integrity guarantee; MUST NOT is explicit.
Output → application-result: `RFC9101-S10.2-MUST-NOT-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: MUST NOT violation; explicit prohibition; AS accepted what it must reject.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: HIGH.

**Node 7 — Arbiter Selector**
Selects RFC 9101 JAR arbiter rule.
Rule: `alg=none` in request object → MUST NOT → AS must reject with `invalid_request_object`.
Output → arbiter-rule: `RFC9101-S10.2-ALG-NONE-REJECT`.

**Node 8 — Arbiter**
Final determination: RFC 9101 §10.2 MUST NOT alg=none for signed request objects.
Unsigned JAR provides no integrity protection; equivalent to plain query parameters.
Output → verdict: `REPROCESS`; citation: `RFC 9101 §10.2 MUST NOT alg=none`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — RFC 9101 §10.2 explicitly prohibits `alg=none` for signed JAR
request objects. An unsigned request object nullifies JAR's integrity guarantee; the
authorization server MUST reject it with `invalid_request_object` or `invalid_request`.

## Pair[CB4-3.4-1124]/Reason

RFC 9101 §10.2 MUST NOT alg=none for request objects; unsigned JAR removes the
integrity protection JAR exists to provide.

## Pair[CB4-3.4-1124]/Phrasing-B

Verdict REPROCESS. Node 1 identifies JAR request object with alg:none. Node 3 locks
RFC 9101 §10.2 (MUST NOT alg=none for signed request objects). Node 5 applies: the
MUST NOT is unconditional — alg:none removes JAR's integrity guarantee entirely. Nodes 6–8
confirm REPROCESS: AS must reject the unsigned request object; accepting it treats a JAR
request no more securely than a plain query-parameter request.

---

## Pair[CB4-3.4-1125]/A

AS issues refresh tokens for authorization_code flows but records no
`client_id` binding with the refresh token — only the token hash, user ID,
and scope. Any authenticated confidential client can redeem the token.

## Pair[CB4-3.4-1125]/B

**Node 1 — Domain Splitter**
Input: Refresh token issued without client_id binding; any confidential client can use it.
Domain signal: OAuth 2.0 refresh token lifecycle; client binding security.
Output → domain-token: `OAUTH2-REFRESH-TOKEN`; sub-domain: `client-id-binding-absent`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: RFC 6749 §10.4 / RFC 9700 §4.13.2.
Scope signal: SHOULD bind refresh token to issuing client; SHOULD-level normative weight.
Output → sub-phase assignment: `3.4-REFRESH-TOKEN-CLIENT-BINDING`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 6749 §10.4: AS SHOULD bind refresh token to client
- RFC 9700 §4.13.2: refresh tokens SHOULD be bound to client instance
Scheme: assess normative weight (SHOULD); no unconditional MUST for client_id binding.
Output → scheme: `REFRESH-TOKEN-CLIENT-BINDING-SHOULD-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts gap: refresh token not bound to client_id; cross-client use possible if one client
is compromised; SHOULD-level obligation not met.
Output → gap-record: `REFRESH_TOKEN_CLIENT_BINDING_ABSENT_SHOULD_VIOLATED`.

**Node 5 — Applier**
Applies RFC 6749 §10.4 / RFC 9700 §4.13.2: SHOULD binding — guidance gap, not MUST.
Cross-client risk is real but normative weight is SHOULD; graduation rule: SHOULD gap → CONCERN.
Output → application-result: `RFC6749-S10.4-SHOULD-GAP`; confidence HIGH.

**Node 6 — Result Translator**
Translates: SHOULD-level gap; no MUST violation identified.
Preliminary verdict: CONCERN.
Output → preliminary-verdict: `CONCERN`; severity: MEDIUM.

**Node 7 — Arbiter Selector**
Selects refresh-token lifecycle SHOULD-level arbiter rule.
Rule: SHOULD binding not met; no escalating MUST violation → CONCERN.
Output → arbiter-rule: `RFC6749-S10.4-SHOULD-CONCERN`.

**Node 8 — Arbiter**
Final determination: RFC 6749 §10.4 / RFC 9700 §4.13.2 SHOULD binding not met.
No unconditional MUST prohibits unbound refresh tokens; guidance gap only.
Output → verdict: `CONCERN`; citation: `RFC 6749 §10.4 / RFC 9700 §4.13.2 SHOULD`.

**Node 9 — Formatter**
Formats final output.
Verdict: CONCERN — RFC 6749 §10.4 and RFC 9700 §4.13.2 both recommend binding refresh
tokens to the issuing client (SHOULD). No unconditional MUST applies; the gap allows
cross-client token use in a compromised-client scenario but is guidance-level only.

## Pair[CB4-3.4-1125]/Reason

RFC 6749 §10.4 / RFC 9700 §4.13.2 SHOULD bind refresh token to client; SHOULD-level
gap only — no unconditional MUST anchor → CONCERN.

## Pair[CB4-3.4-1125]/Phrasing-B

Verdict CONCERN. Node 1 flags missing client_id binding on refresh token. Node 3 locks
RFC 6749 §10.4 and RFC 9700 §4.13.2 — both use SHOULD. Node 5 applies the normative
weight: SHOULD gap is guidance-level; no MUST is violated. Nodes 6–8 confirm CONCERN:
the cross-client risk is meaningful but both standards use SHOULD; CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1126]/A

RS receives JWT access token with valid `iss`, `sub`, `aud`, `exp`, `scope`,
and `jti` claims but no `iat` claim. RS validates `exp`, `iss`, and `aud`
only — no recency check performed because `iat` is absent.

## Pair[CB4-3.4-1126]/B

**Node 1 — Domain Splitter**
Input: JWT at RS without `iat` claim; no recency validation performed.
Domain signal: JWT access token validation at RS; claim completeness.
Output → domain-token: `JWT-ACCESS-TOKEN-RS-VALIDATION`; sub-domain: `iat-claim-absent`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: RFC 9068 §4 (JWT Profile for OAuth 2.0 Access Tokens).
Scope signal: SHOULD include `iat`; SHOULD validate recency — SHOULD-level normative weight.
Output → sub-phase assignment: `3.4-JWT-IAT-RECENCY-CHECK`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 9068 §4: AS SHOULD include `iat`; RS SHOULD validate recency
Scheme: assess normative weight; all MUST validations satisfied (typ, iss, aud, exp).
Output → scheme: `JWT-IAT-SHOULD-RECENCY-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts gap: `iat` absent from token; RS performs no recency check; all MUST claims present.
Output → gap-record: `JWT_IAT_ABSENT_SHOULD_RECENCY_UNFULFILLED`.

**Node 5 — Applier**
Applies RFC 9068 §4 SHOULD: `iat` and recency check are SHOULD-level; all MUST validations
satisfied; missing `iat` is a defense-in-depth gap, not a MUST violation.
Output → application-result: `RFC9068-S4-SHOULD-GAP`; confidence HIGH.

**Node 6 — Result Translator**
Translates: SHOULD-level gap; MUST validation set complete.
Preliminary verdict: CONCERN.
Output → preliminary-verdict: `CONCERN`; severity: LOW-MEDIUM.

**Node 7 — Arbiter Selector**
Selects JWT claim completeness SHOULD-level arbiter rule.
Rule: `iat` SHOULD present / validated; absence is guidance gap; CONCERN.
Output → arbiter-rule: `RFC9068-S4-IAT-SHOULD-CONCERN`.

**Node 8 — Arbiter**
Final determination: RFC 9068 §4 SHOULD for `iat` and recency check; all MUST claims valid.
SHOULD gap only; defense-in-depth loss, not authorization bypass.
Output → verdict: `CONCERN`; citation: `RFC 9068 §4 SHOULD iat recency check`.

**Node 9 — Formatter**
Formats final output.
Verdict: CONCERN — RFC 9068 §4 recommends that access tokens include `iat` and that RS
validate issuance recency (SHOULD). All MUST-level validations are satisfied; the missing
`iat` removes a defense-in-depth control but is not a MUST violation.

## Pair[CB4-3.4-1126]/Reason

RFC 9068 §4 SHOULD include `iat`; SHOULD validate recency; SHOULD-level guidance gap —
all MUST validations satisfied → CONCERN.

## Pair[CB4-3.4-1126]/Phrasing-B

Verdict CONCERN. Node 1 flags `iat` absent from JWT. Node 3 locks RFC 9068 §4 — both
`iat` presence and recency validation are SHOULD. Node 5 confirms all MUST validations
are satisfied (`typ`, `iss`, `aud`, `exp`). Nodes 6–8 confirm CONCERN: the absence of
`iat` removes a useful anomaly-detection signal but is guidance-level; CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1127]/A

OIDC provider's `/userinfo` endpoint accepts requests without
`Authorization: Bearer` header. Callers supply `sub` as a query parameter
and receive full user profile claims — no token validation performed.

## Pair[CB4-3.4-1127]/B

**Node 1 — Domain Splitter**
Input: UserInfo endpoint returns claims without Bearer token; `sub` query param routing.
Domain signal: OIDC UserInfo; authorization enforcement; consent model.
Output → domain-token: `OIDC-USERINFO-ENDPOINT`; sub-domain: `unauthenticated-access`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: OIDC Core §5.3.1; RFC 6750 (Bearer Token).
Scope signal: UserInfo MUST require Bearer token; unauthenticated access bypasses consent.
Output → sub-phase assignment: `3.4-USERINFO-BEARER-REQUIRED`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- OIDC Core §5.3.1: UserInfo endpoint MUST be protected with Bearer access token
- CWE-862: Missing Authorization
Scheme: verify Bearer token requirement at UserInfo; sub-parameter routing bypasses MUST.
Output → scheme: `USERINFO-BEARER-ENFORCEMENT-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: UserInfo returns PII without Bearer token; any caller with a `sub`
value retrieves claims without consent or access token; OIDC Core §5.3.1 MUST violated.
Output → violation-record: `USERINFO_UNAUTHENTICATED_MUST_VIOLATED_CWE862`.

**Node 5 — Applier**
Applies OIDC Core §5.3.1 MUST: UserInfo endpoint MUST accept OAuth 2.0 Bearer token
per RFC 6750; sub-query routing is not a valid substitute; consent model bypassed.
Output → application-result: `OIDC-CORE-S5.3.1-MUST-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: MUST violation; CWE-862; PII exposed without authorization.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: CRITICAL.

**Node 7 — Arbiter Selector**
Selects OIDC UserInfo authorization arbiter rule.
Rule: Bearer token MUST be required at UserInfo; unauthenticated access → REPROCESS.
Output → arbiter-rule: `OIDC-CORE-S5.3.1-BEARER-REQUIRED-REPROCESS`.

**Node 8 — Arbiter**
Final determination: OIDC Core §5.3.1 MUST violated; Bearer token absent; PII exposed
to unauthenticated callers; CWE-862 (Missing Authorization).
Output → verdict: `REPROCESS`; citation: `OIDC Core §5.3.1 MUST Bearer token; CWE-862`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — OIDC Core §5.3.1 MUST: the UserInfo endpoint must be protected by
a valid OAuth 2.0 Bearer access token. Accepting `sub` as a query parameter without token
validation bypasses the MUST requirement and exposes PII without user consent. CWE-862.

## Pair[CB4-3.4-1127]/Reason

OIDC Core §5.3.1 MUST Bearer token at UserInfo; sub-query routing bypasses authorization
check; CWE-862 (Missing Authorization).

## Pair[CB4-3.4-1127]/Phrasing-B

Verdict REPROCESS. Node 1 identifies unauthenticated UserInfo access by `sub` parameter.
Node 3 locks OIDC Core §5.3.1 (MUST Bearer token) and CWE-862. Node 5 applies: `sub`
routing is not a substitute for Bearer token; consent and scope bound by the token are
bypassed entirely. Nodes 6–8 confirm REPROCESS: the MUST is unconditional; any caller
knowing a subject identifier can retrieve PII without authorization.

---

## Pair[CB4-3.4-1128]/A

Client registered with `token_endpoint_auth_method=client_secret_post`.
At the token endpoint, client sends credentials in the HTTP Basic Authorization
header (`client_secret_basic`). AS verifies the credential and ignores the
method mismatch against registration record — issues access token.

## Pair[CB4-3.4-1128]/B

**Node 1 — Domain Splitter**
Input: Client uses wrong `token_endpoint_auth_method` at token endpoint; AS ignores mismatch.
Domain signal: OAuth 2.0 client authentication; registration method binding.
Output → domain-token: `OAUTH2-CLIENT-AUTH`; sub-domain: `token-endpoint-auth-method-mismatch`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: RFC 7591 §2; OpenID Connect Dynamic Registration §3.2.
Scope signal: `token_endpoint_auth_method` SHOULD be enforced; SHOULD-level normative weight.
Output → sub-phase assignment: `3.4-TOKEN-ENDPOINT-AUTH-METHOD-ENFORCEMENT`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 7591 §2: `token_endpoint_auth_method` — negotiated at registration; SHOULD enforce
- OIDC Core Dynamic Registration §3.2: registered method SHOULD be honored
Scheme: assess normative weight — SHOULD; no unconditional MUST for method enforcement.
Output → scheme: `AUTH-METHOD-BINDING-SHOULD-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts gap: `client_secret_post` registered; `client_secret_basic` used; mismatch ignored.
SHOULD obligation not met; weakens registration contract.
Output → gap-record: `TOKEN_ENDPOINT_AUTH_METHOD_MISMATCH_SHOULD_VIOLATED`.

**Node 5 — Applier**
Applies RFC 7591 §2 / OIDC Registration §3.2 SHOULD: binding SHOULD be enforced; no
unconditional MUST; SHOULD gap is guidance-level; graduation rule: SHOULD → CONCERN.
Output → application-result: `RFC7591-S2-SHOULD-GAP`; confidence HIGH.

**Node 6 — Result Translator**
Translates: SHOULD-level gap; no MUST violation.
Preliminary verdict: CONCERN.
Output → preliminary-verdict: `CONCERN`; severity: MEDIUM.

**Node 7 — Arbiter Selector**
Selects client auth method binding SHOULD-level arbiter rule.
Rule: SHOULD enforce registered `token_endpoint_auth_method`; mismatch = CONCERN.
Output → arbiter-rule: `RFC7591-S2-AUTH-METHOD-SHOULD-CONCERN`.

**Node 8 — Arbiter**
Final determination: RFC 7591 §2 / OIDC Registration §3.2 SHOULD; SHOULD-level gap only.
No MUST prohibits accepting a substitute valid authentication method.
Output → verdict: `CONCERN`; citation: `RFC 7591 §2 / OIDC Registration §3.2 SHOULD`.

**Node 9 — Formatter**
Formats final output.
Verdict: CONCERN — RFC 7591 §2 and OIDC Dynamic Registration §3.2 treat
`token_endpoint_auth_method` as a registration binding that SHOULD be enforced. No
unconditional MUST prohibits accepting a substitute valid method; CONCERN not REPROCESS.

## Pair[CB4-3.4-1128]/Reason

RFC 7591 §2 / OIDC Registration §3.2 SHOULD enforce registered `token_endpoint_auth_method`;
SHOULD-level binding gap → CONCERN.

## Pair[CB4-3.4-1128]/Phrasing-B

Verdict CONCERN. Node 1 identifies `token_endpoint_auth_method` mismatch. Node 3 locks
RFC 7591 §2 and OIDC Registration §3.2 — both use SHOULD for method enforcement. Node 5
applies: SHOULD gap is guidance-level; no unconditional MUST prohibits accepting a valid
but non-registered method. Nodes 6–8 confirm CONCERN: registration binding weakened but
no MUST violation; CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1129]/A

RS validates JWT signature, `iss` (trusted AS), `aud` (matches RS), and `exp`
(not expired). JWT `scope` claim is `read:profile`. RS grants access to
`DELETE /admin/data` without checking the `scope` claim in the token.

## Pair[CB4-3.4-1129]/B

**Node 1 — Domain Splitter**
Input: RS authorizes admin operation without checking scope claim; token scope is read:profile.
Domain signal: Resource server authorization enforcement; scope validation; CWE-862.
Output → domain-token: `RS-AUTHORIZATION-ENFORCEMENT`; sub-domain: `scope-claim-not-checked`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: CWE-862 (Missing Authorization); ASVS V8.1.1.
Scope signal: RS performed authentication (token integrity) but skipped authorization (scope check).
Output → sub-phase assignment: `3.4-RS-SCOPE-ENFORCEMENT-REQUIRED`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- CWE-862: Missing Authorization check
- ASVS V8.1.1: MUST use single well-vetted access control mechanism; scope is that mechanism
Scheme: verify RS enforces scope claim as authorization check; absent check = CWE-862.
Output → scheme: `RS-SCOPE-ENFORCEMENT-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: `scope=read:profile`; RS grants `DELETE /admin/data` without scope check;
authorization check absent; any valid AT authorizes any endpoint.
Output → violation-record: `RS_SCOPE_NOT_VALIDATED_CWE862_ASVS_V8.1.1`.

**Node 5 — Applier**
Applies CWE-862 + ASVS V8.1.1: scope claim is the authorization mechanism in OAuth JWT model;
ignoring it means authorization check is absent; any valid token grants any operation.
Output → application-result: `CWE862-ASVS-V8.1.1-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: authorization check absent; CWE-862; `read:profile` token authorizes admin delete.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: CRITICAL.

**Node 7 — Arbiter Selector**
Selects CWE-862 / ASVS V8.1.1 authorization enforcement arbiter rule.
Rule: RS MUST check scope claim as authorization step; absent check → REPROCESS.
Output → arbiter-rule: `CWE862-RS-SCOPE-ENFORCEMENT-REPROCESS`.

**Node 8 — Arbiter**
Final determination: CWE-862 (Missing Authorization); ASVS V8.1.1 MUST; scope claim is
the authorization mechanism — skipping it nullifies the authorization model.
Output → verdict: `REPROCESS`; citation: `CWE-862; ASVS V8.1.1 MUST scope enforcement`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — CWE-862 (Missing Authorization). The RS validates token integrity and
provenance but skips the authorization step. ASVS V8.1.1 requires enforcement of a well-vetted
access control mechanism; in the OAuth JWT model the `scope` claim is that mechanism. A
`read:profile`-scoped token authorizing a destructive admin endpoint is a direct authorization
bypass.

## Pair[CB4-3.4-1129]/Reason

CWE-862 missing authorization; ASVS V8.1.1 MUST enforce access control; RS scope claim is
the authorization mechanism — skipping it nullifies the model.

## Pair[CB4-3.4-1129]/Phrasing-B

Verdict REPROCESS. Node 1 flags RS authorization without scope check. Node 3 locks CWE-862
and ASVS V8.1.1. Node 5 applies: the RS performs authentication (token validation) but
skips authorization (scope enforcement) — a textbook CWE-862 scenario. Nodes 6–8 confirm
REPROCESS: any valid access token authorizes any endpoint; the authorization check is absent.

---

## Pair[CB4-3.4-1130]/A

AS authorization endpoint served at `http://as.example.com/authorize` (plain
HTTP). Ops team argues TLS is terminated at the upstream load balancer and
internal HTTP traverses an isolated internal VLAN.

## Pair[CB4-3.4-1130]/B

**Node 1 — Domain Splitter**
Input: Authorization endpoint on plain HTTP; TLS-at-load-balancer argument.
Domain signal: OAuth 2.0 authorization endpoint transport security; RFC 6749 §3.1.
Output → domain-token: `OAUTH2-AUTHZ-ENDPOINT-TRANSPORT`; sub-domain: `http-no-tls`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: RFC 6749 §3.1 (Authorization Endpoint TLS requirement).
Scope signal: MUST use TLS at authorization endpoint; load-balancer argument rejected.
Output → sub-phase assignment: `3.4-AUTHZ-ENDPOINT-TLS-REQUIRED`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 6749 §3.1: AS MUST require TLS for authorization endpoint
Scheme: verify TLS at authorization endpoint URI; load-balancer-terminates argument invalid
(same pattern as §3.2 token endpoint rejection).
Output → scheme: `AUTHZ-ENDPOINT-TLS-ENFORCEMENT-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: authorization endpoint on HTTP; TLS-at-load-balancer argument is
equivalent to rejected pattern for token endpoint; authorization codes and state traverse
this endpoint unprotected.
Output → violation-record: `AUTHZ_ENDPOINT_HTTP_RFC6749_S3.1_MUST_VIOLATED`.

**Node 5 — Applier**
Applies RFC 6749 §3.1 MUST: authorization endpoint MUST use TLS; internal VLAN isolation
does not satisfy the endpoint-level MUST; load-balancer pattern rejected (same as §3.2).
Output → application-result: `RFC6749-S3.1-MUST-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: MUST violation; authorization codes and state parameters exposed on HTTP path.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: HIGH.

**Node 7 — Arbiter Selector**
Selects RFC 6749 §3.1 authorization endpoint TLS arbiter rule.
Rule: MUST TLS at authorization endpoint; load-balancer argument rejected → REPROCESS.
Output → arbiter-rule: `RFC6749-S3.1-AUTHZ-TLS-REQUIRED-REPROCESS`.

**Node 8 — Arbiter**
Final determination: RFC 6749 §3.1 MUST TLS; HTTP endpoint violates the requirement;
TLS-termination-at-load-balancer argument rejected — MUST applies to endpoint URI itself.
Output → verdict: `REPROCESS`; citation: `RFC 6749 §3.1 MUST TLS authorization endpoint`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — RFC 6749 §3.1 MUST: the authorization endpoint must use TLS.
The load-balancer-terminates-TLS argument is the same rejected pattern as the token
endpoint: the MUST applies to the endpoint itself, not to the network boundary. Authorization
codes, state parameters, and PKCE challenges traverse this endpoint and require TLS
at the point of handling.

## Pair[CB4-3.4-1130]/Reason

RFC 6749 §3.1 MUST TLS for authorization endpoint; load-balancer-terminates argument
rejected — same pattern as §3.2 token endpoint.

## Pair[CB4-3.4-1130]/Phrasing-B

Verdict REPROCESS. Node 1 flags authorization endpoint on plain HTTP. Node 3 locks
RFC 6749 §3.1 (MUST TLS). Node 5 applies: the load-balancer argument is the same rejected
pattern documented for the token endpoint — the MUST applies to the endpoint URI itself,
not to the external network boundary. Nodes 6–8 confirm REPROCESS: authorization codes
and state parameters require endpoint-level TLS; perimeter termination does not satisfy
the §3.1 requirement.
