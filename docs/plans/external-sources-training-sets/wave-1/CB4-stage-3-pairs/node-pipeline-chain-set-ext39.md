# Node Pipeline Chain Set — Extension 39
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1175–1182 (16 instances — 8 traces × 2 phrasings)
# Covers: Full 9-node pipeline traces for ext45 patterns
# Running total after this file: 2364 instances (1182 base pairs)
# Formula check: 120 + (1182 − 60) × 2 = 120 + 2244 = 2364 ✓

---

## Pair[CB4-3.4-1175]/A

`prompt=none` in authorization request; user not authenticated. AS displays
login UI instead of returning `login_required` error.

## Pair[CB4-3.4-1175]/B

**Node 1 — Domain Splitter**
Input: `prompt=none` received; AS displays login page instead of returning `login_required`.
Domain signal: OIDC authorization request; prompt parameter handling; MUST NOT display UI.
Output → domain-token: `OIDC-PROMPT-PARAMETER`; sub-domain: `prompt-none-ui-displayed`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: OIDC Core §3.1.2.2.
Scope signal: MUST NOT display authentication UI when prompt=none; MUST return login_required
error when user not authenticated; UI display violates MUST NOT.
Output → sub-phase assignment: `3.4-OIDC-PROMPT-NONE-MUST-NOT-UI`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- OIDC Core §3.1.2.2: MUST NOT display UI when prompt=none; MUST return login_required
Scheme: verify AS returns error code, not UI, when prompt=none and user unauthenticated.
Output → scheme: `PROMPT-NONE-ERROR-REQUIRED-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: `prompt=none` received; user not authenticated; AS displayed login
UI instead of returning `login_required`; MUST NOT violated.
Output → violation-record: `PROMPT_NONE_UI_DISPLAYED_MUST_NOT_VIOLATED`.

**Node 5 — Applier**
Applies OIDC Core §3.1.2.2 MUST NOT: UI display prohibited when prompt=none; MUST
return login_required; UX argument rejected — RP relies on error code for flow control.
Output → application-result: `OIDC-CORE-S3.1.2.2-MUST-NOT-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: MUST NOT violation; RP cannot detect silent auth failure; prompt=none contract broken.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: HIGH.

**Node 7 — Arbiter Selector**
Selects OIDC prompt=none arbiter rule.
Rule: AS MUST NOT display UI when prompt=none; MUST return login_required error → REPROCESS.
Output → arbiter-rule: `OIDC-CORE-S3.1.2.2-PROMPT-NONE-REPROCESS`.

**Node 8 — Arbiter**
Final determination: OIDC Core §3.1.2.2 MUST NOT violated; AS displayed login UI when
prompt=none required silent behavior; RP cannot distinguish silent success from failure.
Output → verdict: `REPROCESS`; citation: `OIDC Core §3.1.2.2 MUST NOT display UI prompt=none`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — OIDC Core §3.1.2.2 MUST NOT: the AS must not display any UI when
`prompt=none` is received. When the user is not authenticated and silent auth is not
possible, the AS MUST return `login_required`. Displaying the login page violates the
MUST NOT unconditionally; the UX argument is rejected.

## Pair[CB4-3.4-1175]/Reason

OIDC Core §3.1.2.2 MUST NOT display UI when prompt=none; MUST return login_required;
UX argument rejected — RP relies on error code to manage flow.

## Pair[CB4-3.4-1175]/Phrasing-B

Verdict REPROCESS. Node 1 identifies UI display despite prompt=none. Node 3 locks
OIDC Core §3.1.2.2 (MUST NOT display UI; MUST return login_required). Node 5 applies:
prompt=none is a binding contract; UX argument rejected. Nodes 6–8 confirm REPROCESS:
AS violated MUST NOT; RP flow control depends on error code, not UI interaction.

---

## Pair[CB4-3.4-1176]/A

Client registered for `client_credentials` with scope `read:metrics write:metrics`.
Requests `admin:system` scope at token endpoint. AS issues token with all three
scopes including unregistered `admin:system`.

## Pair[CB4-3.4-1176]/B

**Node 1 — Domain Splitter**
Input: client_credentials token scope exceeds registered scope; admin:system not registered.
Domain signal: OAuth 2.0 scope authorization; CWE-269 privilege management.
Output → domain-token: `OAUTH2-SCOPE-AUTHORIZATION`; sub-domain: `scope-exceeds-registration`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: RFC 6749 §3.3 / CWE-269.
Scope signal: AT scope MUST NOT exceed client's registered scope; issuing unregistered
scope = privilege elevation.
Output → sub-phase assignment: `3.4-CLIENT-CREDENTIALS-SCOPE-CEILING`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 6749 §3.3: scope bounded by AS authorization for client; unregistered scope = invalid_scope
- CWE-269: improper privilege management
Scheme: verify issued scope within registered set; excess scope = REPROCESS.
Output → scheme: `CLIENT-CREDENTIALS-SCOPE-CEILING-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: `admin:system` not in registered scope; AS issued it anyway;
scope elevation beyond registration = CWE-269.
Output → violation-record: `CLIENT_CRED_SCOPE_ELEVATION_RFC6749_S3.3_CWE269`.

**Node 5 — Applier**
Applies RFC 6749 §3.3 + CWE-269: issued scope must be within registered scope;
client request does not expand entitlement; AS must reject with invalid_scope or narrow.
Output → application-result: `RFC6749-S3.3-SCOPE-ELEVATION-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: scope elevation beyond registration; CWE-269.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: HIGH.

**Node 7 — Arbiter Selector**
Selects scope ceiling arbiter rule for client_credentials grant.
Rule: AT scope MUST NOT exceed registered scope; excess scope → REPROCESS (CWE-269).
Output → arbiter-rule: `RFC6749-S3.3-SCOPE-ELEVATION-REPROCESS`.

**Node 8 — Arbiter**
Final determination: RFC 6749 §3.3 + CWE-269: scope elevation at client_credentials
token issuance; `admin:system` issued without registration → REPROCESS.
Output → verdict: `REPROCESS`; citation: `RFC 6749 §3.3 scope ceiling; CWE-269`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — RFC 6749 §3.3 and CWE-269: the AT scope MUST NOT exceed the
client's registered scope. Issuing `admin:system` to a client registered only for
`read:metrics write:metrics` is scope elevation; the AS MUST return `invalid_scope`
or honor only the registered scope intersection.

## Pair[CB4-3.4-1176]/Reason

RFC 6749 §3.3 scope ceiling + CWE-269: AT scope MUST NOT exceed registered scope;
admin:system not in registration → scope elevation → REPROCESS.

## Pair[CB4-3.4-1176]/Phrasing-B

Verdict REPROCESS. Node 1 flags scope exceeding registered set. Node 3 locks RFC 6749
§3.3 (scope ceiling) and CWE-269 (improper privilege management). Node 5 applies:
client request does not expand registration entitlement; AS MUST reject or narrow.
Nodes 6–8 confirm REPROCESS: scope elevation at issuance; REPROCESS.

---

## Pair[CB4-3.4-1177]/A

Token endpoint error responses include full Java exception stack traces in
`error_description` field. Operations team argues it helps diagnose
integration issues.

## Pair[CB4-3.4-1177]/B

**Node 1 — Domain Splitter**
Input: Token endpoint error response includes exception stack trace in error_description.
Domain signal: error response information disclosure; CWE-209.
Output → domain-token: `OAUTH2-ERROR-RESPONSE`; sub-domain: `stack-trace-in-error-response`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: CWE-209 / ASVS V14.3.2.
Scope signal: error responses SHOULD NOT reveal internal implementation details;
stack traces expose technology stack; guidance-level — SHOULD NOT.
Output → sub-phase assignment: `3.4-ERROR-RESPONSE-INFORMATION-DISCLOSURE`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- CWE-209: Information Exposure Through an Error Message
- ASVS V14.3.2: error responses should not reveal internal implementation details
Scheme: assess normative weight — SHOULD NOT guidance; no unconditional RFC MUST prohibits.
Output → scheme: `ERROR-RESPONSE-DISCLOSURE-SHOULD-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts gap: stack trace in error_description; reveals class names, file paths, line numbers;
ASVS V14.3.2 SHOULD NOT guidance not met; no unconditional MUST.
Output → gap-record: `STACK_TRACE_IN_ERROR_RESPONSE_SHOULD_GAP_CWE209`.

**Node 5 — Applier**
Applies CWE-209 + ASVS V14.3.2 SHOULD NOT: guidance not met; stack traces reveal implementation
details; no unconditional RFC MUST; SHOULD-level guidance gap → CONCERN.
Output → application-result: `CWE209-ASVS-V14.3.2-SHOULD-GAP`; confidence HIGH.

**Node 6 — Result Translator**
Translates: SHOULD NOT guidance gap; no MUST violation; CWE-209 risk identified.
Preliminary verdict: CONCERN.
Output → preliminary-verdict: `CONCERN`; severity: MEDIUM.

**Node 7 — Arbiter Selector**
Selects error response disclosure guidance arbiter rule.
Rule: SHOULD NOT include stack traces; CWE-209 risk; guidance gap → CONCERN.
Output → arbiter-rule: `CWE209-ERROR-DISCLOSURE-SHOULD-CONCERN`.

**Node 8 — Arbiter**
Final determination: CWE-209 + ASVS V14.3.2 SHOULD NOT guidance not met;
stack trace in error response; no unconditional MUST; guidance gap.
Output → verdict: `CONCERN`; citation: `CWE-209; ASVS V14.3.2 SHOULD NOT stack trace`.

**Node 9 — Formatter**
Formats final output.
Verdict: CONCERN — CWE-209 and ASVS V14.3.2 guidance: error responses SHOULD NOT include
stack traces or internal implementation details. Stack traces expose class names, file paths,
and code structure. No unconditional RFC MUST prohibits informative error detail in all contexts;
guidance gap → CONCERN. Stack traces should be logged server-side only.

## Pair[CB4-3.4-1177]/Reason

CWE-209 + ASVS V14.3.2 SHOULD NOT include stack traces in error responses; no unconditional
RFC MUST; guidance gap → CONCERN.

## Pair[CB4-3.4-1177]/Phrasing-B

Verdict CONCERN. Node 1 identifies stack trace in token endpoint error response. Node 3 locks
CWE-209 and ASVS V14.3.2 (SHOULD NOT expose implementation details). Node 5 applies: SHOULD
NOT guidance not met; no unconditional RFC MUST prohibits informative error detail. Nodes 6–8
confirm CONCERN: CWE-209 risk; guidance gap; CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1178]/A

JWT access tokens (RFC 9068 profile) issued without `nbf` claim. RS validates
`exp`, `iss`, `aud` only. AS team cites RFC 9068 `nbf` OPTIONAL classification.
Pre-issued tokens for scheduled batch jobs have unconstrained start window.

## Pair[CB4-3.4-1178]/B

**Node 1 — Domain Splitter**
Input: JWT AT issued without `nbf` claim; RS has no start-bound validation; OPTIONAL cited.
Domain signal: JWT access token temporal binding; nbf claim completeness.
Output → domain-token: `JWT-ACCESS-TOKEN-CLAIMS`; sub-domain: `nbf-claim-absent`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: RFC 9068 §2.2.
Scope signal: `nbf` OPTIONAL per RFC 9068; absence removes start-bound temporal constraint;
guidance-level gap for pre-issued token security.
Output → sub-phase assignment: `3.4-JWT-NBF-CLAIM-TEMPORAL-BINDING`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 9068 §2.2: `nbf` OPTIONAL; not SHOULD or MUST
Scheme: assess normative weight — OPTIONAL; no explicit SHOULD; guidance gap for
temporal window closure in pre-issued token scenarios.
Output → scheme: `JWT-NBF-OPTIONAL-GUIDANCE-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts gap: `nbf` absent; pre-issued tokens usable from issuance time, not intended
deployment time; temporal start bound missing; OPTIONAL status confirmed — guidance gap only.
Output → gap-record: `JWT_NBF_ABSENT_OPTIONAL_TEMPORAL_GAP`.

**Node 5 — Applier**
Applies RFC 9068 §2.2 OPTIONAL: nbf is syntactically optional; absence is permitted;
best-practice gap for temporal start-bound definition; no MUST or SHOULD violated.
Output → application-result: `RFC9068-S2.2-NBF-OPTIONAL-GUIDANCE-GAP`; confidence HIGH.

**Node 6 — Result Translator**
Translates: OPTIONAL claim absent; guidance gap for temporal binding; no MUST violation.
Preliminary verdict: CONCERN.
Output → preliminary-verdict: `CONCERN`; severity: LOW-MEDIUM.

**Node 7 — Arbiter Selector**
Selects JWT temporal binding guidance arbiter rule.
Rule: `nbf` OPTIONAL; absence = guidance gap for temporal start bound; CONCERN.
Output → arbiter-rule: `RFC9068-S2.2-NBF-OPTIONAL-CONCERN`.

**Node 8 — Arbiter**
Final determination: RFC 9068 §2.2 OPTIONAL for `nbf`; absence removes temporal start
bound; guidance gap; no MUST or SHOULD violated.
Output → verdict: `CONCERN`; citation: `RFC 9068 §2.2 nbf OPTIONAL; temporal binding gap`.

**Node 9 — Formatter**
Formats final output.
Verdict: CONCERN — RFC 9068 §2.2 classifies `nbf` as OPTIONAL; the AS's citation is correct.
Absence removes the temporal start-bound definition from the access token; pre-issued tokens
have an unconstrained start window. Guidance-level gap — CONCERN not REPROCESS.

## Pair[CB4-3.4-1178]/Reason

RFC 9068 §2.2 `nbf` OPTIONAL; absence is syntactically permitted; temporal start-bound
guidance gap only → CONCERN.

## Pair[CB4-3.4-1178]/Phrasing-B

Verdict CONCERN. Node 1 identifies `nbf` absent from JWT AT. Node 3 locks RFC 9068 §2.2
(OPTIONAL — not SHOULD or MUST). Node 5 applies: OPTIONAL status confirmed; absence is
permitted; guidance gap for temporal start binding. Nodes 6–8 confirm CONCERN: no MUST
violated; temporal start-bound gap documented; CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1179]/A

Token endpoint accepts HTTP GET requests in addition to POST. Client submits
`GET /token?grant_type=authorization_code&code=<code>&client_secret=<secret>`.
AS processes and issues token. Team argues GET improves integration flexibility.

## Pair[CB4-3.4-1179]/B

**Node 1 — Domain Splitter**
Input: Token endpoint accepts HTTP GET; client_secret and code transmitted in URL.
Domain signal: OAuth 2.0 token endpoint HTTP method; credential exposure in URL.
Output → domain-token: `OAUTH2-TOKEN-ENDPOINT`; sub-domain: `get-method-accepted`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: RFC 6749 §3.2.
Scope signal: MUST use POST; GET exposes credentials in URL (logs, proxy, history);
integration-flexibility argument rejected.
Output → sub-phase assignment: `3.4-TOKEN-ENDPOINT-MUST-POST`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 6749 §3.2: client MUST use HTTP POST method for access token requests
Scheme: verify token endpoint rejects GET; URL-based credential exposure risk documented.
Output → scheme: `TOKEN-ENDPOINT-POST-MUST-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: GET accepted at token endpoint; `client_secret` and `code` in URL query
string; logged in server access logs, proxy logs, browser history, Referer headers.
Output → violation-record: `TOKEN_ENDPOINT_GET_ACCEPTED_RFC6749_S3.2_MUST_VIOLATED`.

**Node 5 — Applier**
Applies RFC 6749 §3.2 MUST POST: GET acceptance violates MUST; URL credential exposure
compounds violation; flexibility argument rejected.
Output → application-result: `RFC6749-S3.2-MUST-POST-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: MUST violation; credential exposure in URL via GET method.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: HIGH.

**Node 7 — Arbiter Selector**
Selects token endpoint HTTP method MUST arbiter rule.
Rule: MUST POST; GET acceptance → REPROCESS; credential URL exposure.
Output → arbiter-rule: `RFC6749-S3.2-TOKEN-ENDPOINT-POST-MUST-REPROCESS`.

**Node 8 — Arbiter**
Final determination: RFC 6749 §3.2 MUST POST violated; GET acceptance exposes credentials
in URL; flexibility argument rejected.
Output → verdict: `REPROCESS`; citation: `RFC 6749 §3.2 MUST use HTTP POST`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — RFC 6749 §3.2 MUST: clients must use HTTP POST for access token
requests. GET transmits `client_secret`, `code`, and other credentials in the URL query
string, which is captured in server logs, proxy logs, and browser history. The flexibility
argument does not override the MUST; the token endpoint MUST reject GET requests.

## Pair[CB4-3.4-1179]/Reason

RFC 6749 §3.2 MUST use HTTP POST; GET accepted exposes credentials in URL; MUST
unconditional → REPROCESS.

## Pair[CB4-3.4-1179]/Phrasing-B

Verdict REPROCESS. Node 1 flags token endpoint accepting GET requests. Node 3 locks RFC 6749
§3.2 (MUST POST). Node 5 applies: GET acceptance violates MUST; credentials in URL are
captured in logs and history. Nodes 6–8 confirm REPROCESS: MUST unconditional; GET method
rejected regardless of flexibility argument.

---

## Pair[CB4-3.4-1180]/A

RP validates ID token `iss` claim using case-insensitive comparison:
`iss.toLowerCase() === "as.example.com"`. AS iss = `https://AS.Example.COM`.
Case-insensitive match succeeds. Team argues DNS hostnames are case-insensitive.

## Pair[CB4-3.4-1180]/B

**Node 1 — Domain Splitter**
Input: ID token `iss` validated case-insensitively; DNS case-insensitivity argument.
Domain signal: OIDC ID token iss validation; case-sensitive comparison requirement.
Output → domain-token: `OIDC-ID-TOKEN-VALIDATION`; sub-domain: `iss-case-insensitive-comparison`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: OIDC Core §3.1.3.7.1 / RFC 7519 §4.1.1.
Scope signal: `iss` MUST be case-sensitive exact string comparison; DNS argument rejected.
Output → sub-phase assignment: `3.4-ID-TOKEN-ISS-CASE-SENSITIVE-MUST`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 7519 §4.1.1: `iss` is a case-sensitive string
- OIDC Core §3.1.3.7.2: `iss` Claim Value MUST be identical to Issuer Identifier
Scheme: verify case-sensitive comparison; DNS argument mis-identifies iss as hostname.
Output → scheme: `ISS-CASE-SENSITIVE-COMPARISON-MUST-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: case-insensitive comparison; DNS argument incorrectly applied to
URI string identifier; `iss` comparison must be case-sensitive per RFC 7519 §4.1.1.
Output → violation-record: `ID_TOKEN_ISS_CASE_INSENSITIVE_MUST_VIOLATED`.

**Node 5 — Applier**
Applies RFC 7519 §4.1.1 + OIDC Core §3.1.3.7.2: `iss` is a case-sensitive string;
MUST exact match; DNS argument rejected — iss is a URI identifier, not a hostname lookup.
Output → application-result: `RFC7519-S4.1.1-ISS-CASE-SENSITIVE-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: MUST violation; case-insensitive comparison allows issuer confusion attacks.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: HIGH.

**Node 7 — Arbiter Selector**
Selects iss case-sensitive MUST arbiter rule.
Rule: `iss` MUST case-sensitive exact match; case-insensitive → REPROCESS.
Output → arbiter-rule: `RFC7519-S4.1.1-ISS-CASE-SENSITIVE-REPROCESS`.

**Node 8 — Arbiter**
Final determination: RFC 7519 §4.1.1 + OIDC Core §3.1.3.7.2: `iss` MUST case-sensitive;
case-insensitive comparison violates MUST; DNS argument mis-categorizes iss as hostname.
Output → verdict: `REPROCESS`; citation: `RFC 7519 §4.1.1 case-sensitive iss; OIDC Core §3.1.3.7.2`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — RFC 7519 §4.1.1 and OIDC Core §3.1.3.7.2: `iss` is a case-sensitive
string that MUST be compared by exact match. Case-insensitive comparison allows issuer
confusion; the DNS hostname case-insensitivity argument misidentifies `iss` as a hostname —
it is a URI string identifier with case-sensitive semantics defined by the JWT specification.

## Pair[CB4-3.4-1180]/Reason

RFC 7519 §4.1.1 `iss` case-sensitive string; OIDC Core §3.1.3.7.2 MUST exact match;
DNS argument rejected — iss is URI identifier, not hostname → REPROCESS.

## Pair[CB4-3.4-1180]/Phrasing-B

Verdict REPROCESS. Node 1 identifies case-insensitive iss comparison. Node 3 locks RFC 7519
§4.1.1 (case-sensitive string) and OIDC Core §3.1.3.7.2 (MUST exact match). Node 5 applies:
DNS case-insensitivity argument misidentifies iss — it is a URI string identifier, not a DNS
label; JWT spec defines case-sensitive semantics. Nodes 6–8 confirm REPROCESS: case-insensitive
comparison violates MUST.

---

## Pair[CB4-3.4-1181]/A

AS hashes nonce before embedding in ID token: received `nonce=abc123xyz`;
ID token contains `"nonce": "HASHED_abc123xyz"`. Privacy motivation cited.
RP match fails; rejects token.

## Pair[CB4-3.4-1181]/B

**Node 1 — Domain Splitter**
Input: AS transforms (hashes) nonce before including in ID token; RP match fails.
Domain signal: OIDC nonce passthrough; MUST unmodified.
Output → domain-token: `OIDC-NONCE-HANDLING`; sub-domain: `nonce-transformed-by-as`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: OIDC Core §3.1.2.2.
Scope signal: nonce MUST be passed through unmodified from request to ID token;
transformation violates unmodified passthrough MUST.
Output → sub-phase assignment: `3.4-OIDC-NONCE-UNMODIFIED-MUST`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- OIDC Core §3.1.2.2: nonce "passed through unmodified" from auth request to ID token
Scheme: verify nonce in ID token == nonce in auth request byte-for-byte; transformation = violation.
Output → scheme: `NONCE-UNMODIFIED-PASSTHROUGH-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: nonce hashed before inclusion; RP match impossible; replay binding broken;
"passed through unmodified" requirement violated.
Output → violation-record: `NONCE_TRANSFORMED_UNMODIFIED_MUST_VIOLATED`.

**Node 5 — Applier**
Applies OIDC Core §3.1.2.2 "passed through unmodified": any transformation (hash, encoding,
truncation) violates the requirement; privacy motivation does not override normative requirement.
Output → application-result: `OIDC-CORE-S3.1.2.2-NONCE-UNMODIFIED-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: MUST violation (unmodified passthrough); RP cannot validate nonce; replay detection broken.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: HIGH.

**Node 7 — Arbiter Selector**
Selects OIDC nonce unmodified passthrough arbiter rule.
Rule: nonce MUST be passed through unmodified; any transformation → REPROCESS.
Output → arbiter-rule: `OIDC-CORE-S3.1.2.2-NONCE-UNMODIFIED-REPROCESS`.

**Node 8 — Arbiter**
Final determination: OIDC Core §3.1.2.2 "passed through unmodified" violated; AS hashed
nonce; RP match impossible; privacy motivation does not override normative requirement.
Output → verdict: `REPROCESS`; citation: `OIDC Core §3.1.2.2 nonce passed through unmodified`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — OIDC Core §3.1.2.2: the nonce must be "passed through unmodified"
from the Authentication Request to the ID Token. Hashing the nonce before embedding it
violates the unmodified requirement; the RP cannot match the transformed value against the
original, breaking replay detection. Privacy motivation does not override the normative
requirement.

## Pair[CB4-3.4-1181]/Reason

OIDC Core §3.1.2.2 nonce "passed through unmodified"; AS hashing transforms the value;
RP match fails; replay binding broken → REPROCESS.

## Pair[CB4-3.4-1181]/Phrasing-B

Verdict REPROCESS. Node 1 identifies nonce hashed before ID token inclusion. Node 3 locks
OIDC Core §3.1.2.2 ("passed through unmodified"). Node 5 applies: hashing is a transformation;
"unmodified" means byte-for-byte identical to the received value; privacy motivation rejected.
Nodes 6–8 confirm REPROCESS: RP cannot validate nonce; replay detection broken.

---

## Pair[CB4-3.4-1182]/A

Multi-IdP RP uses only `sub` for user lookup: `SELECT * FROM users WHERE sub = ?`.
User from Google (sub=12345) and user from internal AS (sub=12345) map to same
account. OIDC Core §8.2 states sub is locally unique per issuer.

## Pair[CB4-3.4-1182]/B

**Node 1 — Domain Splitter**
Input: RP uses sub-only lookup in multi-issuer environment; OIDC Core §8.2 sub is per-issuer.
Domain signal: OIDC identity binding; (iss, sub) as unique identifier; CWE-284.
Output → domain-token: `OIDC-IDENTITY-BINDING`; sub-domain: `sub-only-lookup-multi-issuer`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: OIDC Core §8.2.
Scope signal: sub unique within issuer only; multi-issuer RP MUST use (iss, sub) pair;
sub-alone lookup = identity confusion CWE-284.
Output → sub-phase assignment: `3.4-OIDC-ISS-SUB-IDENTITY-BINDING`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- OIDC Core §8.2: sub is locally unique within issuer; (iss, sub) = unique identifier
- CWE-284: Improper Access Control (identity confusion)
Scheme: verify RP uses (iss, sub) pair; sub-alone in multi-issuer = identity confusion.
Output → scheme: `ISS-SUB-IDENTITY-PAIR-REQUIRED-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: sub-alone lookup; multiple issuers may share sub values; user from
Google sub=12345 maps to same record as user from internal AS sub=12345; identity confusion.
Output → violation-record: `SUB_ONLY_LOOKUP_MULTI_ISSUER_CWE284_OIDC_S8.2`.

**Node 5 — Applier**
Applies OIDC Core §8.2 + CWE-284: sub unique per issuer; (iss, sub) MUST be used as
identity key in multi-issuer context; sub-alone = identity confusion vulnerability.
Output → application-result: `OIDC-CORE-S8.2-ISS-SUB-MUST-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: identity confusion; wrong user authenticated; CWE-284 improper access control.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: CRITICAL.

**Node 7 — Arbiter Selector**
Selects OIDC (iss, sub) identity pair arbiter rule.
Rule: multi-issuer RP MUST use (iss, sub) pair; sub-alone = REPROCESS (CWE-284).
Output → arbiter-rule: `OIDC-CORE-S8.2-ISS-SUB-PAIR-REQUIRED-REPROCESS`.

**Node 8 — Arbiter**
Final determination: OIDC Core §8.2: sub locally unique per issuer; multi-issuer RP
sub-alone lookup = identity confusion; CWE-284 (Improper Access Control).
Output → verdict: `REPROCESS`; citation: `OIDC Core §8.2 sub per-issuer; CWE-284 identity confusion`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — OIDC Core §8.2: `sub` identifiers are locally unique within an issuer,
not globally. A multi-issuer RP MUST use the `(iss, sub)` pair as the identity key.
Sub-alone lookup allows users from different identity providers to collide on the same
account — a CWE-284 identity confusion vulnerability.

## Pair[CB4-3.4-1182]/Reason

OIDC Core §8.2 sub locally unique per issuer; multi-issuer RP sub-only lookup = identity
confusion; (iss, sub) pair MUST be identity key; CWE-284 → REPROCESS.

## Pair[CB4-3.4-1182]/Phrasing-B

Verdict REPROCESS. Node 1 flags sub-only user lookup in multi-issuer RP. Node 3 locks
OIDC Core §8.2 (sub per-issuer unique) and CWE-284. Node 5 applies: sub alone is
insufficient as identity key when multiple issuers are possible; (iss, sub) MUST be the
lookup key. Nodes 6–8 confirm REPROCESS: identity confusion vulnerability; wrong user
authenticated by sub collision across issuers.
