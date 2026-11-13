# Node Pipeline Chain Set — Extension 38
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1149–1156 (16 instances — 8 traces × 2 phrasings)
# Covers: Full 9-node pipeline traces for ext44 patterns
# Running total after this file: 2312 instances (1156 base pairs)
# Formula check: 120 + (1156 − 60) × 2 = 120 + 2192 = 2312 ✓

---

## Pair[CB4-3.4-1149]/A

Client sends `max_age=300` in authorization request. AS re-authenticates user.
Returned ID token does not contain `auth_time` claim. AS argues optionality.

## Pair[CB4-3.4-1149]/B

**Node 1 — Domain Splitter**
Input: `max_age` in auth request; ID token missing `auth_time` claim.
Domain signal: OIDC ID token validation; `max_age` / `auth_time` binding requirement.
Output → domain-token: `OIDC-ID-TOKEN`; sub-domain: `auth-time-absent-max-age-present`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: OIDC Core §3.1.3.7.11.
Scope signal: `auth_time` MUST be present when `max_age` was in the request; AS optionality
argument applies only when `max_age` is absent.
Output → sub-phase assignment: `3.4-OIDC-AUTH-TIME-MAX-AGE-MUST`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- OIDC Core §3.1.3.7.11: MUST include `auth_time` when `max_age` request made
Scheme: verify ID token contains `auth_time` when `max_age` in request; absence = MUST violated.
Output → scheme: `OIDC-AUTH-TIME-MUST-PRESENCE-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: `max_age=300` in request; ID token returned without `auth_time`; RP cannot
verify authentication recency; `max_age` security guarantee silently nullified.
Output → violation-record: `AUTH_TIME_ABSENT_MAX_AGE_PRESENT_MUST_VIOLATED`.

**Node 5 — Applier**
Applies OIDC Core §3.1.3.7.11 MUST: `auth_time` required when `max_age` in request; optionality
argument rejected — optionality applies only in absence of `max_age`.
Output → application-result: `OIDC-CORE-S3.1.3.7.11-MUST-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: MUST violation; RP validation of `max_age` constraint impossible without `auth_time`.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: HIGH.

**Node 7 — Arbiter Selector**
Selects OIDC `auth_time` / `max_age` arbiter rule.
Rule: `auth_time` MUST be present when `max_age` requested → REPROCESS if absent.
Output → arbiter-rule: `OIDC-CORE-S3.1.3.7.11-AUTH-TIME-REPROCESS`.

**Node 8 — Arbiter**
Final determination: OIDC Core §3.1.3.7.11 MUST violated; `auth_time` absent when `max_age`
was in request; RP-side max_age validation impossible.
Output → verdict: `REPROCESS`; citation: `OIDC Core §3.1.3.7.11 MUST auth_time when max_age`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — OIDC Core §3.1.3.7.11 MUST: the ID token MUST contain `auth_time`
when a `max_age` request was made. The AS's optionality argument applies only when
`max_age` is absent; the MUST is unconditional once `max_age` is in the request.

## Pair[CB4-3.4-1149]/Reason

OIDC Core §3.1.3.7.11 MUST: `auth_time` required when `max_age` present in authorization
request; optionality applies only to requests without `max_age`.

## Pair[CB4-3.4-1149]/Phrasing-B

Verdict REPROCESS. Node 1 identifies `auth_time` absent with `max_age=300` in request.
Node 3 locks OIDC Core §3.1.3.7.11 (MUST). Node 5 applies: the optionality argument is
incorrect — once `max_age` is present, `auth_time` is MUST-level. Nodes 6–8 confirm
REPROCESS: without `auth_time`, the RP cannot verify the `max_age` constraint; the
security purpose is silently defeated.

---

## Pair[CB4-3.4-1150]/A

Confidential server-side client registered with
`token_endpoint_auth_method=none`. AS issues tokens without client
authentication — client presents only `client_id`.

## Pair[CB4-3.4-1150]/B

**Node 1 — Domain Splitter**
Input: Confidential client registered with `none` auth method; no client authentication at token endpoint.
Domain signal: OAuth 2.0 client authentication; confidential client MUST requirement.
Output → domain-token: `OAUTH2-CLIENT-AUTH`; sub-domain: `confidential-client-none-auth-method`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: RFC 6749 §3.2.1 (confidential client authentication MUST).
Scope signal: MUST authenticate confidential clients; `none` permitted only for public clients.
Output → sub-phase assignment: `3.4-CONFIDENTIAL-CLIENT-AUTH-REQUIRED`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 6749 §3.2.1: confidential clients MUST authenticate at token endpoint
- RFC 6749 §2.1: client type determines authentication capability
Scheme: verify confidential client authenticates; `none` method valid only for public clients.
Output → scheme: `CONFIDENTIAL-CLIENT-AUTH-MUST-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: confidential server-side client; `token_endpoint_auth_method=none`; AS
accepts unauthenticated token requests; §3.2.1 MUST violated.
Output → violation-record: `CONFIDENTIAL_CLIENT_AUTH_NONE_MUST_VIOLATED`.

**Node 5 — Applier**
Applies RFC 6749 §3.2.1 MUST: confidential clients MUST authenticate; `none` is not a valid
method for confidential clients; syntactic validity in registration spec does not override §3.2.1.
Output → application-result: `RFC6749-S3.2.1-MUST-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: MUST violation; confidential client unauthenticated at token endpoint.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: HIGH.

**Node 7 — Arbiter Selector**
Selects confidential client authentication MUST arbiter rule.
Rule: `none` auth method → allowed for public clients only; confidential client with `none` → REPROCESS.
Output → arbiter-rule: `RFC6749-S3.2.1-CONFIDENTIAL-CLIENT-AUTH-REPROCESS`.

**Node 8 — Arbiter**
Final determination: RFC 6749 §3.2.1 MUST violated; confidential client issued tokens without
authentication; `none` method registration does not override the §3.2.1 MUST.
Output → verdict: `REPROCESS`; citation: `RFC 6749 §3.2.1 MUST confidential client auth`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — RFC 6749 §3.2.1 MUST: confidential clients MUST authenticate at the
token endpoint. `token_endpoint_auth_method=none` is valid only for public clients; applying
it to a confidential client and issuing unauthenticated tokens violates the MUST.

## Pair[CB4-3.4-1150]/Reason

RFC 6749 §3.2.1 MUST authenticate confidential clients; `none` valid only for public clients;
confidential client with `none` and unauthenticated token issuance → REPROCESS.

## Pair[CB4-3.4-1150]/Phrasing-B

Verdict REPROCESS. Node 1 flags confidential client with `token_endpoint_auth_method=none`.
Node 3 locks RFC 6749 §3.2.1 (MUST authenticate confidential clients). Node 5 applies:
`none` is defined for public clients only; syntactic validity in Dynamic Registration spec
does not override §3.2.1; confidential client unauthenticated at token endpoint = MUST
violation. Nodes 6–8 confirm REPROCESS.

---

## Pair[CB4-3.4-1151]/A

Refresh tokens issued for authorization_code flows. Each exchange returns a
new AT but the same unchanged refresh token. Absolute 90-day lifetime and
family revocation present. Team says per-use rotation is unnecessary.

## Pair[CB4-3.4-1151]/B

**Node 1 — Domain Splitter**
Input: Refresh token not rotated on each use; 90-day absolute lifetime; family revocation present.
Domain signal: OAuth 2.0 refresh token lifecycle security; per-use rotation guidance.
Output → domain-token: `OAUTH2-REFRESH-TOKEN`; sub-domain: `per-use-rotation-absent`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: RFC 9700 §4.14.1.
Scope signal: SHOULD rotate refresh tokens on each use; SHOULD-level normative weight.
Output → sub-phase assignment: `3.4-REFRESH-TOKEN-PER-USE-ROTATION`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 9700 §4.14.1: authorization servers SHOULD rotate refresh tokens on each use
Scheme: assess normative weight — SHOULD; compensating controls noted but do not substitute.
Output → scheme: `REFRESH-TOKEN-ROTATION-SHOULD-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts gap: no per-use rotation; stolen refresh token exploitable for full 90-day lifetime;
SHOULD guidance not met; absolute lifetime and family revocation are compensating controls
but do not fulfill rotation SHOULD.
Output → gap-record: `REFRESH_TOKEN_ROTATION_ABSENT_SHOULD_VIOLATED`.

**Node 5 — Applier**
Applies RFC 9700 §4.14.1 SHOULD: per-use rotation not implemented; SHOULD gap; no MUST anchor.
Compensating controls reduce risk but do not satisfy the SHOULD; guidance gap documented.
Output → application-result: `RFC9700-S4.14.1-SHOULD-GAP`; confidence HIGH.

**Node 6 — Result Translator**
Translates: SHOULD-level gap; no MUST violation.
Preliminary verdict: CONCERN.
Output → preliminary-verdict: `CONCERN`; severity: MEDIUM.

**Node 7 — Arbiter Selector**
Selects refresh token lifecycle SHOULD arbiter rule.
Rule: per-use rotation SHOULD; absent = CONCERN; compensating controls noted.
Output → arbiter-rule: `RFC9700-S4.14.1-ROTATION-SHOULD-CONCERN`.

**Node 8 — Arbiter**
Final determination: RFC 9700 §4.14.1 SHOULD rotation not met; no unconditional MUST.
Compensating controls insufficient to fulfill the SHOULD; guidance gap only.
Output → verdict: `CONCERN`; citation: `RFC 9700 §4.14.1 SHOULD rotate refresh tokens`.

**Node 9 — Formatter**
Formats final output.
Verdict: CONCERN — RFC 9700 §4.14.1 SHOULD: refresh tokens SHOULD be rotated on each use.
Per-use rotation limits exploitation of stolen tokens to the period before next legitimate use.
No unconditional MUST applies; compensating controls noted; guidance gap → CONCERN.

## Pair[CB4-3.4-1151]/Reason

RFC 9700 §4.14.1 SHOULD rotate refresh tokens on each use; no unconditional MUST;
compensating controls present but do not fulfill rotation SHOULD → CONCERN.

## Pair[CB4-3.4-1151]/Phrasing-B

Verdict CONCERN. Node 1 identifies missing per-use refresh token rotation. Node 3 locks
RFC 9700 §4.14.1 (SHOULD rotate on each use). Node 5 applies: SHOULD-level; absolute
lifetime and family revocation are compensating controls but do not substitute for the
SHOULD guidance; stolen token remains exploitable for its full lifetime. Nodes 6–8 confirm
CONCERN: guidance gap; no MUST violated; CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1152]/A

OIDC Discovery document at `/.well-known/openid-configuration` contains all
endpoint fields except `jwks_uri`, which is absent. AS team argues clients
can obtain JWKS URL out of band.

## Pair[CB4-3.4-1152]/B

**Node 1 — Domain Splitter**
Input: OIDC Discovery document missing `jwks_uri` field; out-of-band key distribution argument.
Domain signal: OIDC Discovery metadata; REQUIRED field absence.
Output → domain-token: `OIDC-DISCOVERY`; sub-domain: `jwks-uri-absent-required`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: OIDC Core Discovery §3.
Scope signal: `jwks_uri` REQUIRED in discovery document; absence breaks programmatic key location.
Output → sub-phase assignment: `3.4-OIDC-DISCOVERY-JWKS-URI-REQUIRED`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- OIDC Core Discovery §3: `jwks_uri` REQUIRED
Scheme: verify REQUIRED field present in discovery document; out-of-band argument rejected.
Output → scheme: `OIDC-DISCOVERY-REQUIRED-FIELD-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: `jwks_uri` absent; REQUIRED field missing from discovery document;
discovery-conformant clients have no standard programmatic JWKS endpoint reference.
Output → violation-record: `JWKS_URI_ABSENT_REQUIRED_OIDC_DISCOVERY`.

**Node 5 — Applier**
Applies OIDC Discovery §3 REQUIRED: `jwks_uri` MUST be present; out-of-band argument
defeats discovery purpose; REQUIRED status is unconditional.
Output → application-result: `OIDC-DISCOVERY-S3-REQUIRED-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: REQUIRED field absent; discovery non-conformant; ID token signature validation
impossible from discovery alone.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: HIGH.

**Node 7 — Arbiter Selector**
Selects OIDC Discovery REQUIRED field arbiter rule.
Rule: REQUIRED field absent in discovery = REPROCESS; out-of-band distribution rejected.
Output → arbiter-rule: `OIDC-DISCOVERY-S3-REQUIRED-FIELD-REPROCESS`.

**Node 8 — Arbiter**
Final determination: OIDC Discovery §3 REQUIRED field (`jwks_uri`) absent; discovery
document non-conformant; clients cannot locate JWKS from discovery alone.
Output → verdict: `REPROCESS`; citation: `OIDC Discovery §3 REQUIRED jwks_uri`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — OIDC Core Discovery §3 classifies `jwks_uri` as REQUIRED.
The discovery document is non-conformant; out-of-band key distribution defeats the
purpose of machine-readable discovery and does not substitute for a REQUIRED field.

## Pair[CB4-3.4-1152]/Reason

OIDC Core Discovery §3 REQUIRED: `jwks_uri` must be present; out-of-band key
distribution argument rejected; REQUIRED absence → REPROCESS.

## Pair[CB4-3.4-1152]/Phrasing-B

Verdict REPROCESS. Node 1 identifies `jwks_uri` absent from OIDC Discovery document.
Node 3 locks OIDC Discovery §3 (REQUIRED). Node 5 applies: REQUIRED is unconditional;
the out-of-band argument negates the purpose of discovery. Nodes 6–8 confirm REPROCESS:
`jwks_uri` REQUIRED; discovery document missing a REQUIRED field is non-conformant.

---

## Pair[CB4-3.4-1153]/A

OIDC end-session endpoint `/end_session` redirects browser to
`post_logout_redirect_uri` without checking whether the value is pre-registered
for the RP. Attacker crafts logout link to attacker-controlled domain.

## Pair[CB4-3.4-1153]/B

**Node 1 — Domain Splitter**
Input: `post_logout_redirect_uri` not validated against pre-registered URIs; open redirect risk.
Domain signal: OIDC RP-Initiated Logout; end-session endpoint URI validation.
Output → domain-token: `OIDC-END-SESSION`; sub-domain: `post-logout-redirect-uri-unvalidated`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: OIDC RP-Initiated Logout §4.
Scope signal: MUST validate `post_logout_redirect_uri` against registered URIs; open redirect vulnerability.
Output → sub-phase assignment: `3.4-OIDC-POST-LOGOUT-REDIRECT-URI-VALIDATION`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- OIDC RP-Initiated Logout §4: OP MUST validate `post_logout_redirect_uri` is registered for RP
Scheme: verify end-session validates redirect URI; unvalidated = open redirect at AS origin.
Output → scheme: `OIDC-POST-LOGOUT-REDIRECT-VALIDATION-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: `post_logout_redirect_uri` not validated; attacker can craft logout URL
redirecting to attacker-controlled domain; open redirect vulnerability at AS origin.
Output → violation-record: `POST_LOGOUT_REDIRECT_UNVALIDATED_MUST_VIOLATED`.

**Node 5 — Applier**
Applies OIDC RP-Initiated Logout §4 MUST: validate `post_logout_redirect_uri` against
registered URIs; no-token-issued argument rejected (open redirect threat independent of tokens).
Output → application-result: `OIDC-RP-LOGOUT-S4-MUST-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: MUST violation; open redirect at AS origin; phishing / session attack enabler.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: HIGH.

**Node 7 — Arbiter Selector**
Selects OIDC RP-Initiated Logout URI validation arbiter rule.
Rule: MUST validate `post_logout_redirect_uri`; unvalidated = REPROCESS; open redirect risk.
Output → arbiter-rule: `OIDC-RP-LOGOUT-S4-REDIRECT-VALIDATION-REPROCESS`.

**Node 8 — Arbiter**
Final determination: OIDC RP-Initiated Logout §4 MUST violated; open redirect at AS origin;
no-token argument rejected — open redirect threat is independent of token issuance.
Output → verdict: `REPROCESS`; citation: `OIDC RP-Initiated Logout §4 MUST validate post_logout_redirect_uri`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — OIDC RP-Initiated Logout §4 MUST: `post_logout_redirect_uri` MUST
be validated against the RP's pre-registered URIs. Unvalidated redirects create an open
redirect vulnerability at the AS origin, enabling phishing and social engineering attacks.

## Pair[CB4-3.4-1153]/Reason

OIDC RP-Initiated Logout §4 MUST validate `post_logout_redirect_uri`; open redirect at AS;
no-token argument rejected — open redirect risk is token-issuance-independent.

## Pair[CB4-3.4-1153]/Phrasing-B

Verdict REPROCESS. Node 1 identifies unvalidated `post_logout_redirect_uri`. Node 3 locks
OIDC RP-Initiated Logout §4 (MUST validate). Node 5 applies: the MUST is unconditional;
no-token argument rejected — open redirect at an AS origin is a meaningful attack surface.
Nodes 6–8 confirm REPROCESS: attacker can craft logout URL redirecting to attacker-controlled
page after legitimate logout interaction.

---

## Pair[CB4-3.4-1154]/A

RS exception handler logs full HTTP request headers on 500-class errors —
including `Authorization: Bearer <token>`. Access-controlled log. Team argues
log access is restricted to ops team only.

## Pair[CB4-3.4-1154]/B

**Node 1 — Domain Splitter**
Input: RS logs full Bearer token in exception handler; access-controlled log argument.
Domain signal: credential logging; MUST NOT log authentication tokens.
Output → domain-token: `RS-ERROR-LOGGING`; sub-domain: `bearer-token-in-exception-log`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: ASVS V8.2.2 / V9.3.1.
Scope signal: MUST NOT log authentication credentials; Bearer tokens are credentials;
access-controlled log argument does not satisfy MUST NOT.
Output → sub-phase assignment: `3.4-BEARER-TOKEN-LOGGING-MUST-NOT`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- ASVS V8.2.2: MUST NOT log authentication credentials and sensitive tokens
- ASVS V9.3.1: tokens MUST be protected throughout lifecycle
Scheme: verify Bearer tokens not logged in any log; access-control argument rejected (MUST NOT unconditional).
Output → scheme: `CREDENTIAL-LOGGING-MUST-NOT-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: full Bearer token in exception log; access-controlled but MUST NOT is
unconditional; log aggregators, backups, forwarding multiply exposure surface.
Output → violation-record: `BEARER_TOKEN_LOGGED_ASVS_V8.2.2_MUST_NOT_VIOLATED`.

**Node 5 — Applier**
Applies ASVS V8.2.2 MUST NOT: authentication credentials must not be logged;
access-controlled log argument rejected (same rejection as client_secret logging pattern).
Output → application-result: `ASVS-V8.2.2-MUST-NOT-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: MUST NOT violation; Bearer token logged in exception traces.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: HIGH.

**Node 7 — Arbiter Selector**
Selects credential logging MUST NOT arbiter rule.
Rule: authentication credentials (including Bearer tokens) MUST NOT appear in logs;
access-controlled log does not satisfy MUST NOT → REPROCESS.
Output → arbiter-rule: `ASVS-V8.2.2-CREDENTIAL-LOGGING-REPROCESS`.

**Node 8 — Arbiter**
Final determination: ASVS V8.2.2 MUST NOT violated; Bearer token logged unconditionally
prohibited; access-control argument rejected.
Output → verdict: `REPROCESS`; citation: `ASVS V8.2.2 MUST NOT log credentials; V9.3.1`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — ASVS V8.2.2 MUST NOT: authentication credentials including Bearer
tokens MUST NOT be logged. The access-controlled log argument is rejected; MUST NOT is
unconditional. Log aggregation, forwarding pipelines, and backup retention extend the
exposure surface beyond the intended audience.

## Pair[CB4-3.4-1154]/Reason

ASVS V8.2.2 MUST NOT log authentication credentials; Bearer tokens are credentials;
access-controlled log argument rejected — MUST NOT unconditional.

## Pair[CB4-3.4-1154]/Phrasing-B

Verdict REPROCESS. Node 1 flags Bearer token in RS exception log. Node 3 locks ASVS V8.2.2
(MUST NOT log credentials) and V9.3.1 (token lifecycle protection). Node 5 applies: Bearer
tokens are authentication credentials; MUST NOT is unconditional regardless of log access
controls; log infrastructure multiplies exposure. Nodes 6–8 confirm REPROCESS.

---

## Pair[CB4-3.4-1155]/A

JWT access tokens issued with `sub` = user's email address (`alice@example.com`).
Team argues email is stable, globally unique, and avoids extra lookups.
Tokens transmitted over HTTPS, stored server-side only.

## Pair[CB4-3.4-1155]/B

**Node 1 — Domain Splitter**
Input: JWT `sub` claim contains email address in access tokens; PII-in-sub concern.
Domain signal: JWT access token claim composition; PII in token payload.
Output → domain-token: `JWT-ACCESS-TOKEN-CLAIMS`; sub-domain: `sub-email-pii`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: RFC 9068 §2.2 / ASVS V9.3.1.
Scope signal: email is syntactically valid as sub; no MUST for opaque sub in AT;
ASVS guidance recommends opaque PPID; guidance-level gap.
Output → sub-phase assignment: `3.4-JWT-SUB-PII-GUIDANCE`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 9068 §2.2: `sub` REQUIRED; no unconditional MUST for opaque value in AT
- ASVS V9.3.1: guidance: tokens should not contain unnecessary PII
Scheme: assess normative weight — no MUST prohibiting email as sub; ASVS guidance gap.
Output → scheme: `JWT-SUB-PII-GUIDANCE-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts gap: email as `sub` in AT is PII; visible to RS, log systems, intermediaries;
HTTPS/server-side argument partially mitigates transit but not log exposure; no MUST violated.
Output → gap-record: `JWT_SUB_EMAIL_PII_SHOULD_GAP`.

**Node 5 — Applier**
Applies RFC 9068 §2.2 / ASVS V9.3.1: email satisfies structural MUST for sub;
guidance recommends opaque PPID; guidance gap only; no unconditional MUST prohibits.
Output → application-result: `RFC9068-S2.2-ASVS-V9.3.1-GUIDANCE-GAP`; confidence HIGH.

**Node 6 — Result Translator**
Translates: guidance-level gap; no MUST violation; log and intermediary PII exposure risk noted.
Preliminary verdict: CONCERN.
Output → preliminary-verdict: `CONCERN`; severity: MEDIUM.

**Node 7 — Arbiter Selector**
Selects JWT PII-in-sub guidance arbiter rule.
Rule: email as sub is structurally valid; guidance recommends opaque PPID; guidance gap → CONCERN.
Output → arbiter-rule: `RFC9068-S2.2-SUB-PII-GUIDANCE-CONCERN`.

**Node 8 — Arbiter**
Final determination: RFC 9068 §2.2 MUST for sub satisfied (email is a valid sub value);
ASVS V9.3.1 guidance gap; no unconditional MUST prohibiting non-opaque sub in AT.
Output → verdict: `CONCERN`; citation: `RFC 9068 §2.2 / ASVS V9.3.1 guidance; no MUST`.

**Node 9 — Formatter**
Formats final output.
Verdict: CONCERN — RFC 9068 §2.2 requires `sub` and is satisfied structurally. ASVS V9.3.1
guidance recommends opaque PPID sub values in access tokens to minimize PII exposure in
logs and intermediaries. No unconditional MUST prohibits email as `sub`; guidance gap.

## Pair[CB4-3.4-1155]/Reason

RFC 9068 §2.2 `sub` REQUIRED satisfied; ASVS V9.3.1 guidance: opaque PPID preferred
to avoid PII in token payload; no unconditional MUST → CONCERN.

## Pair[CB4-3.4-1155]/Phrasing-B

Verdict CONCERN. Node 1 identifies email as `sub` in JWT AT. Node 3 locks RFC 9068 §2.2
(REQUIRED satisfied by email) and ASVS V9.3.1 (guidance for opaque PPID). Node 5 applies:
email is structurally valid; no MUST prohibits non-opaque sub; guidance gap on PII exposure
in logs and intermediaries. Nodes 6–8 confirm CONCERN: guidance-level gap; CONCERN not
REPROCESS.

---

## Pair[CB4-3.4-1156]/A

JWT access tokens (RFC 9068 profile) issued without `scope` claim. Scope tracked
server-side only (introspection). Team cites RFC 9068 optionality. RS cannot enforce
scope from self-contained JWT without introspection call.

## Pair[CB4-3.4-1156]/B

**Node 1 — Domain Splitter**
Input: JWT AT missing `scope` claim; RS cannot enforce scope from self-contained JWT.
Domain signal: JWT access token claim completeness; RS scope enforcement capability.
Output → domain-token: `JWT-ACCESS-TOKEN-CLAIMS`; sub-domain: `scope-claim-absent`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: RFC 9068 §2.2.3.1.
Scope signal: SHOULD include `scope` in JWT AT; optionality acknowledged; SHOULD guidance.
Output → sub-phase assignment: `3.4-JWT-SCOPE-CLAIM-SHOULD`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 9068 §2.2.3.1: authorization server SHOULD include `scope` in AT
Scheme: assess normative weight — SHOULD; team's optionality citation acknowledged;
SHOULD guidance gap impacts RS-side stateless scope enforcement.
Output → scheme: `JWT-SCOPE-CLAIM-SHOULD-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts gap: `scope` absent from JWT AT; RS cannot perform scope-based authorization
without introspection; self-contained JWT value diminished; SHOULD guidance not met.
Output → gap-record: `JWT_SCOPE_ABSENT_SHOULD_GAP`.

**Node 5 — Applier**
Applies RFC 9068 §2.2.3.1 SHOULD: include `scope`; optionality is syntactic; SHOULD guidance
not met; RS stateless scope enforcement impossible without scope claim; guidance gap.
Output → application-result: `RFC9068-S2.2.3.1-SHOULD-GAP`; confidence HIGH.

**Node 6 — Result Translator**
Translates: SHOULD-level gap; no MUST violation; RS scope enforcement impaired.
Preliminary verdict: CONCERN.
Output → preliminary-verdict: `CONCERN`; severity: MEDIUM.

**Node 7 — Arbiter Selector**
Selects JWT scope claim SHOULD arbiter rule.
Rule: `scope` SHOULD in JWT AT; absent = guidance gap; stateless RS enforcement impaired → CONCERN.
Output → arbiter-rule: `RFC9068-S2.2.3.1-SCOPE-SHOULD-CONCERN`.

**Node 8 — Arbiter**
Final determination: RFC 9068 §2.2.3.1 SHOULD guidance not met; `scope` absent from JWT AT;
RS stateless authorization impaired; no unconditional MUST.
Output → verdict: `CONCERN`; citation: `RFC 9068 §2.2.3.1 SHOULD include scope in AT`.

**Node 9 — Formatter**
Formats final output.
Verdict: CONCERN — RFC 9068 §2.2.3.1 SHOULD: access tokens SHOULD include a `scope` claim.
Team's optionality citation is accurate for structural conformance; the SHOULD guidance exists
to enable RS-side stateless scope enforcement. Absent `scope`, any RS relying on the
self-contained JWT cannot perform scope-based authorization without an introspection round
trip. CONCERN not REPROCESS.

## Pair[CB4-3.4-1156]/Reason

RFC 9068 §2.2.3.1 SHOULD include `scope` in JWT AT; no unconditional MUST; absent `scope`
impairs RS stateless scope enforcement → CONCERN.

## Pair[CB4-3.4-1156]/Phrasing-B

Verdict CONCERN. Node 1 identifies `scope` absent from JWT AT. Node 3 locks RFC 9068
§2.2.3.1 (SHOULD include scope). Node 5 applies: SHOULD guidance not met; the optionality
is syntactic; absent scope claim removes the RS's ability to perform stateless authorization.
Nodes 6–8 confirm CONCERN: guidance-level gap; no unconditional MUST; CONCERN not REPROCESS.
