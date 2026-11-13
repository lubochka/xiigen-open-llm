# Node Pipeline Chain Set — Extension 40
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1201–1208 (16 instances — 8 traces × 2 phrasings)
# Covers: Full 9-node pipeline traces for ext46 patterns
# Running total after this file: 2416 instances (1208 base pairs)
# Formula check: 120 + (1208 − 60) × 2 = 120 + 2296 = 2416 ✓

---

## Pair[CB4-3.4-1201]/A

AS uses 8-bit counter for JWT jti; values cycle every 256 tokens within the
15-minute TTL. Two valid tokens can share the same jti simultaneously.

## Pair[CB4-3.4-1201]/B

**Node 1 — Domain Splitter**
Input: JWT jti values cycle (8-bit counter); collisions within TTL window.
Domain signal: JWT ID claim uniqueness; replay prevention; RFC 7519 §4.1.7.
Output → domain-token: `JWT-JTI-UNIQUENESS`; sub-domain: `jti-counter-cycles-within-ttl`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: RFC 7519 §4.1.7.
Scope signal: jti MUST be unique when used for replay prevention; counter cycling = collision.
Output → sub-phase assignment: `3.4-JWT-JTI-MUST-UNIQUE`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 7519 §4.1.7: jti MUST be unique per token when used for replay prevention
Scheme: verify jti uniqueness; 8-bit counter cycling within TTL → collisions → MUST violated.
Output → scheme: `JTI-UNIQUENESS-MUST-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: 8-bit counter → 256-token cycle; TTL 15 min → >256 tokens possible
in window; two valid tokens with same jti; blocklist cannot distinguish; MUST violated.
Output → violation-record: `JTI_COUNTER_CYCLE_UNIQUENESS_MUST_VIOLATED`.

**Node 5 — Applier**
Applies RFC 7519 §4.1.7 MUST unique: counter cycling within TTL creates collisions;
jti-based revocation model collapses; exp argument rejected — jti and exp serve different purposes.
Output → application-result: `RFC7519-S4.1.7-JTI-UNIQUE-MUST-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: MUST violation; jti collision within TTL; revocation and replay prevention broken.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: HIGH.

**Node 7 — Arbiter Selector**
Selects JWT jti uniqueness MUST arbiter rule.
Rule: jti MUST be unique for replay prevention; counter cycling → REPROCESS.
Output → arbiter-rule: `RFC7519-S4.1.7-JTI-UNIQUE-REPROCESS`.

**Node 8 — Arbiter**
Final determination: RFC 7519 §4.1.7 MUST violated; jti counter cycles within TTL;
two valid tokens share same jti; revocation model broken.
Output → verdict: `REPROCESS`; citation: `RFC 7519 §4.1.7 MUST unique jti`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — RFC 7519 §4.1.7 MUST: jti values must be unique per token
when used for replay prevention. An 8-bit counter that cycles within the 15-minute TTL
produces colliding jti values; the blocklist cannot distinguish tokens; the revocation
model is broken. Use UUID v4 or equivalent CSPRNG-based jti.

## Pair[CB4-3.4-1201]/Reason

RFC 7519 §4.1.7 MUST unique jti when used for replay prevention; 8-bit counter cycles
within TTL → jti collisions within window → REPROCESS.

## Pair[CB4-3.4-1201]/Phrasing-B

Verdict REPROCESS. Node 1 identifies jti cycling within TTL. Node 3 locks RFC 7519 §4.1.7
(MUST unique for replay prevention). Node 5 applies: counter cycling produces collisions
within 15-minute TTL; revocation model collapses. Nodes 6–8 confirm REPROCESS: MUST unique
violated; REPROCESS.

---

## Pair[CB4-3.4-1202]/A

OIDC Discovery document contains all REQUIRED fields but `claims_supported` is
absent. AS argues claims_supported is optional per Discovery §3.

## Pair[CB4-3.4-1202]/B

**Node 1 — Domain Splitter**
Input: OIDC Discovery document missing `claims_supported`; RECOMMENDED field.
Domain signal: OIDC Discovery metadata completeness; claims_supported guidance.
Output → domain-token: `OIDC-DISCOVERY`; sub-domain: `claims-supported-absent`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: OIDC Core Discovery §3.
Scope signal: `claims_supported` RECOMMENDED not REQUIRED; absence is permitted;
guidance gap for programmatic claim discovery.
Output → sub-phase assignment: `3.4-OIDC-DISCOVERY-CLAIMS-SUPPORTED`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- OIDC Core Discovery §3: `claims_supported` RECOMMENDED
Scheme: assess normative weight — RECOMMENDED; AS optionality argument is correct.
Output → scheme: `OIDC-DISCOVERY-RECOMMENDED-FIELD-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts gap: `claims_supported` absent; RECOMMENDED not met; clients cannot
determine available claims programmatically; no REQUIRED violation.
Output → gap-record: `CLAIMS_SUPPORTED_ABSENT_RECOMMENDED_GAP`.

**Node 5 — Applier**
Applies OIDC Discovery §3 RECOMMENDED: `claims_supported` is RECOMMENDED not REQUIRED;
absence is permitted for minimum conformance; guidance gap only.
Output → application-result: `OIDC-DISCOVERY-S3-RECOMMENDED-GAP`; confidence HIGH.

**Node 6 — Result Translator**
Translates: RECOMMENDED guidance gap; no REQUIRED/MUST violation.
Preliminary verdict: CONCERN.
Output → preliminary-verdict: `CONCERN`; severity: LOW.

**Node 7 — Arbiter Selector**
Selects OIDC Discovery RECOMMENDED field arbiter rule.
Rule: RECOMMENDED field absent = guidance gap; CONCERN; not REPROCESS.
Output → arbiter-rule: `OIDC-DISCOVERY-S3-RECOMMENDED-CONCERN`.

**Node 8 — Arbiter**
Final determination: OIDC Discovery §3 RECOMMENDED; claims_supported absent;
guidance gap only; AS optionality argument correct.
Output → verdict: `CONCERN`; citation: `OIDC Discovery §3 claims_supported RECOMMENDED`.

**Node 9 — Formatter**
Formats final output.
Verdict: CONCERN — OIDC Core Discovery §3 classifies `claims_supported` as RECOMMENDED,
not REQUIRED. The AS team's optionality argument is correct for minimum conformance. Absence
is a guidance gap — clients cannot determine available claims programmatically. CONCERN not
REPROCESS.

## Pair[CB4-3.4-1202]/Reason

OIDC Discovery §3 `claims_supported` RECOMMENDED (not REQUIRED); absence permitted
for minimum conformance; AS optionality argument correct → CONCERN.

## Pair[CB4-3.4-1202]/Phrasing-B

Verdict CONCERN. Node 1 identifies `claims_supported` absent from OIDC Discovery. Node 3
locks OIDC Discovery §3 (RECOMMENDED). Node 5 applies: RECOMMENDED not REQUIRED; absence
is permitted; guidance gap only. Nodes 6–8 confirm CONCERN: no MUST violated; RECOMMENDED
guidance gap → CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1203]/A

Token response includes `expires_in: 3600` but actual token exp = iat + 900.
Clients cache token for 3600 seconds; silent 401s after 900 seconds.

## Pair[CB4-3.4-1203]/B

**Node 1 — Domain Splitter**
Input: expires_in value (3600) mismatches actual token TTL (900); silent 401 failures.
Domain signal: OAuth 2.0 token response metadata; expires_in accuracy.
Output → domain-token: `OAUTH2-TOKEN-RESPONSE`; sub-domain: `expires-in-mismatch`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: RFC 6749 §5.1.
Scope signal: expires_in defined as "lifetime in seconds"; mismatch misrepresents lifetime;
no explicit MUST for accuracy when present.
Output → sub-phase assignment: `3.4-EXPIRES-IN-ACCURACY`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 6749 §5.1: expires_in = "the lifetime in seconds of the access token" (RECOMMENDED)
Scheme: assess normative weight — RECOMMENDED; definition implies accuracy; no explicit
MUST prohibiting misrepresentation; guidance gap.
Output → scheme: `EXPIRES-IN-ACCURACY-GUIDANCE-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts gap: expires_in 3600 vs actual TTL 900; clients cache token 4× longer than valid;
silent 401 failures; misleading metadata; no explicit MUST for accuracy.
Output → gap-record: `EXPIRES_IN_MISMATCH_GUIDANCE_GAP`.

**Node 5 — Applier**
Applies RFC 6749 §5.1: definition implies accuracy; no explicit MUST for correctness when
present; guidance gap; impact significant but normative anchor is definitional → CONCERN.
Output → application-result: `RFC6749-S5.1-EXPIRES-IN-GUIDANCE-GAP`; confidence HIGH.

**Node 6 — Result Translator**
Translates: guidance-level gap; RFC definition implies accuracy but no explicit MUST; CONCERN.
Preliminary verdict: CONCERN.
Output → preliminary-verdict: `CONCERN`; severity: MEDIUM.

**Node 7 — Arbiter Selector**
Selects expires_in accuracy guidance arbiter rule.
Rule: RFC 6749 §5.1 defines expires_in as actual TTL; mismatch = guidance gap; CONCERN.
Output → arbiter-rule: `RFC6749-S5.1-EXPIRES-IN-ACCURACY-CONCERN`.

**Node 8 — Arbiter**
Final determination: RFC 6749 §5.1 definitional gap; expires_in misrepresents token TTL;
no explicit MUST for accuracy when present; guidance gap only.
Output → verdict: `CONCERN`; citation: `RFC 6749 §5.1 expires_in should match actual TTL`.

**Node 9 — Formatter**
Formats final output.
Verdict: CONCERN — RFC 6749 §5.1 defines `expires_in` as the actual token lifetime.
A value of 3600 that misrepresents a 900-second TTL causes silent 401 failures and
incorrect client-side cache management. No explicit MUST for accuracy when present;
definitional gap → CONCERN not REPROCESS.

## Pair[CB4-3.4-1203]/Reason

RFC 6749 §5.1 defines expires_in as actual TTL; 3600/900 mismatch misrepresents
lifetime; no explicit MUST for accuracy when present → CONCERN.

## Pair[CB4-3.4-1203]/Phrasing-B

Verdict CONCERN. Node 1 identifies expires_in mismatch. Node 3 locks RFC 6749 §5.1
(definition: "lifetime in seconds of access token"; RECOMMENDED). Node 5 applies:
definition implies accuracy but no explicit MUST; significant operational impact but
normative anchor is definitional. Nodes 6–8 confirm CONCERN: guidance gap; CONCERN
not REPROCESS.

---

## Pair[CB4-3.4-1204]/A

AS login page echoes `login_hint` into HTML input value without encoding:
`<input value="{{ login_hint }}">`. Attacker crafts URL with
`login_hint="><script>...</script>` → XSS at AS origin.

## Pair[CB4-3.4-1204]/B

**Node 1 — Domain Splitter**
Input: login_hint unencoded in HTML → XSS; attacker executes script at AS origin.
Domain signal: output encoding in HTML; CWE-79; AS origin XSS severity.
Output → domain-token: `AS-LOGIN-PAGE-XSS`; sub-domain: `login-hint-unencoded-html`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: CWE-79 / ASVS V5.3.3.
Scope signal: MUST HTML-encode user-controlled input in HTML contexts; AS origin XSS
is critical — session, token, consent state accessible.
Output → sub-phase assignment: `3.4-AS-LOGIN-PAGE-OUTPUT-ENCODING`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- CWE-79: Cross-Site Scripting
- ASVS V5.3.3: MUST use output encoding in HTML contexts for user-controlled input
Scheme: verify HTML encoding of login_hint before rendering; unencoded = REPROCESS.
Output → scheme: `HTML-OUTPUT-ENCODING-MUST-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: login_hint value interpolated into HTML value attribute unencoded;
XSS at AS origin; session theft, token theft, flow manipulation possible.
Output → violation-record: `LOGIN_HINT_UNENCODED_CWE79_ASVS_V5.3.3_MUST_VIOLATED`.

**Node 5 — Applier**
Applies CWE-79 + ASVS V5.3.3 MUST: all user-controlled input MUST be output-encoded in
HTML contexts; XSS at AS origin is critical; MUST unconditional.
Output → application-result: `CWE79-ASVS-V5.3.3-MUST-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: MUST violation; XSS at AS origin; critical severity.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: CRITICAL.

**Node 7 — Arbiter Selector**
Selects CWE-79 / ASVS V5.3.3 output encoding arbiter rule.
Rule: user input MUST be HTML-encoded; unencoded → REPROCESS; AS origin XSS critical.
Output → arbiter-rule: `CWE79-ASVS-V5.3.3-OUTPUT-ENCODING-REPROCESS`.

**Node 8 — Arbiter**
Final determination: CWE-79 + ASVS V5.3.3 MUST violated; login_hint XSS at AS origin.
Output → verdict: `REPROCESS`; citation: `CWE-79; ASVS V5.3.3 MUST HTML output encoding`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — CWE-79 and ASVS V5.3.3 MUST: all user-controlled input rendered
in HTML contexts must be output-encoded. Echoing `login_hint` into an HTML value
attribute without encoding creates XSS at the AS origin — a critical finding given the
authentication sessions, consent state, and OAuth codes accessible at that origin.

## Pair[CB4-3.4-1204]/Reason

CWE-79 + ASVS V5.3.3 MUST HTML-encode user input; login_hint unencoded in HTML
attribute → XSS at AS origin; critical severity → REPROCESS.

## Pair[CB4-3.4-1204]/Phrasing-B

Verdict REPROCESS. Node 1 flags login_hint XSS at AS origin. Node 3 locks CWE-79 and
ASVS V5.3.3 (MUST HTML-encode user input). Node 5 applies: MUST is unconditional; XSS
at AS origin enables session and token theft. Nodes 6–8 confirm REPROCESS: MUST output
encoding violated; REPROCESS.

---

## Pair[CB4-3.4-1205]/A

AS session cookie has `Secure` and `HttpOnly` attributes but `SameSite` is absent.
Browser defaults to `Lax`. Team asks whether explicit SameSite=Strict is REQUIRED.

## Pair[CB4-3.4-1205]/B

**Node 1 — Domain Splitter**
Input: AS session cookie missing explicit SameSite attribute; browser defaults to Lax.
Domain signal: session cookie security attributes; CSRF mitigation; SameSite guidance.
Output → domain-token: `AS-SESSION-COOKIE`; sub-domain: `samesite-absent`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: ASVS V7.2 / OWASP session management guidance.
Scope signal: SameSite=Strict recommended for AS session cookies; no unconditional
RFC MUST; browser Lax default provides partial protection.
Output → sub-phase assignment: `3.4-SESSION-COOKIE-SAMESITE-GUIDANCE`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- ASVS V7.2: session cookie attribute guidance — SameSite recommended
- OWASP: SameSite=Strict for sensitive cookies
Scheme: assess normative weight — RECOMMENDED/guidance; no unconditional RFC MUST.
Output → scheme: `SESSION-COOKIE-SAMESITE-GUIDANCE-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts gap: explicit SameSite absent; browser Lax default provides partial CSRF
mitigation; no unconditional RFC MUST for SameSite attribute; guidance gap.
Output → gap-record: `SESSION_COOKIE_SAMESITE_ABSENT_GUIDANCE_GAP`.

**Node 5 — Applier**
Applies ASVS V7.2 guidance: SameSite=Strict recommended; browser Lax default
mitigates most CSRF vectors; no unconditional MUST; guidance gap → CONCERN.
Output → application-result: `ASVS-V7.2-SAMESITE-GUIDANCE-GAP`; confidence HIGH.

**Node 6 — Result Translator**
Translates: guidance-level gap; browser Lax default provides partial mitigation.
Preliminary verdict: CONCERN.
Output → preliminary-verdict: `CONCERN`; severity: LOW-MEDIUM.

**Node 7 — Arbiter Selector**
Selects session cookie SameSite guidance arbiter rule.
Rule: SameSite=Strict recommended; no unconditional RFC MUST; guidance gap → CONCERN.
Output → arbiter-rule: `ASVS-V7.2-SAMESITE-GUIDANCE-CONCERN`.

**Node 8 — Arbiter**
Final determination: ASVS V7.2 guidance gap; SameSite absent; browser Lax default
provides partial protection; no unconditional RFC MUST.
Output → verdict: `CONCERN`; citation: `ASVS V7.2 SameSite=Strict recommended; no RFC MUST`.

**Node 9 — Formatter**
Formats final output.
Verdict: CONCERN — ASVS V7.2 and OWASP guidance recommend explicit `SameSite=Strict`
for AS session cookies. No unconditional RFC MUST requires a specific SameSite value;
browser-default Lax provides partial CSRF mitigation. Guidance-level gap → CONCERN.
Remediation: set `SameSite=Strict` explicitly.

## Pair[CB4-3.4-1205]/Reason

ASVS V7.2 / OWASP guidance: SameSite=Strict recommended for session cookies; no
unconditional RFC MUST; browser Lax default mitigates main CSRF vectors → CONCERN.

## Pair[CB4-3.4-1205]/Phrasing-B

Verdict CONCERN. Node 1 flags missing SameSite on AS session cookie. Node 3 locks ASVS V7.2
(recommended) and OWASP guidance. Node 5 applies: RECOMMENDED guidance not met; no
unconditional RFC MUST; browser Lax default provides partial protection. Nodes 6–8 confirm
CONCERN: guidance gap; CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1206]/A

Client B intercepts Client A's authorization code and exchanges it at token
endpoint using its own valid credentials. AS authenticates Client B successfully
but does not check that the code was issued to Client A, not Client B.
Token issued to Client B.

## Pair[CB4-3.4-1206]/B

**Node 1 — Domain Splitter**
Input: Client B exchanges Client A's authorization code; AS doesn't verify client_id binding.
Domain signal: OAuth 2.0 authorization code binding; RFC 6749 §4.1.3 client_id MUST check.
Output → domain-token: `OAUTH2-AUTHZ-CODE-BINDING`; sub-domain: `client-id-binding-not-checked`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: RFC 6749 §4.1.3.
Scope signal: AS MUST ensure code was issued to the requesting client; client B with
client A's code violates MUST; authentication ≠ code binding.
Output → sub-phase assignment: `3.4-AUTHZ-CODE-CLIENT-BINDING-MUST`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 6749 §4.1.3: MUST ensure auth code was issued to the authenticated client
Scheme: verify client_id binding at code exchange; different client using intercepted code = MUST violated.
Output → scheme: `AUTHZ-CODE-CLIENT-BINDING-MUST-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: auth code issued to Client A; Client B presents code with own credentials;
AS validates Client B credentials but skips code-to-client binding check; MUST violated.
Output → violation-record: `AUTHZ_CODE_CLIENT_BINDING_SKIPPED_RFC6749_S4.1.3_MUST_VIOLATED`.

**Node 5 — Applier**
Applies RFC 6749 §4.1.3 MUST: code MUST be bound to requesting client; credential
validation and binding check are distinct requirements; both must pass.
Output → application-result: `RFC6749-S4.1.3-CLIENT-BINDING-MUST-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: MUST violation; cross-client code exploitation enabled.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: HIGH.

**Node 7 — Arbiter Selector**
Selects RFC 6749 §4.1.3 client binding arbiter rule.
Rule: auth code MUST be bound to issuing client; different client exchange → REPROCESS.
Output → arbiter-rule: `RFC6749-S4.1.3-CLIENT-BINDING-REPROCESS`.

**Node 8 — Arbiter**
Final determination: RFC 6749 §4.1.3 MUST violated; auth code not bound to requesting
client; Client B exploited Client A's intercepted code.
Output → verdict: `REPROCESS`; citation: `RFC 6749 §4.1.3 MUST verify code issued to requesting client`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — RFC 6749 §4.1.3 MUST: the AS must verify that the authorization
code was issued to the authenticated client making the exchange. Client credential
validation and client-code binding are independent requirements; both must pass.
Client B exchanging Client A's intercepted code violates the MUST.

## Pair[CB4-3.4-1206]/Reason

RFC 6749 §4.1.3 MUST verify auth code issued to requesting client; Client B using
Client A's intercepted code bypasses this MUST; credential validation ≠ code binding.

## Pair[CB4-3.4-1206]/Phrasing-B

Verdict REPROCESS. Node 1 flags Client B exchanging Client A's code. Node 3 locks
RFC 6749 §4.1.3 (MUST verify code-to-client binding). Node 5 applies: credential
validation and binding check are distinct requirements; both must pass simultaneously.
Nodes 6–8 confirm REPROCESS: MUST violated; cross-client code exploitation enabled.

---

## Pair[CB4-3.4-1207]/A

Confidential client uses `private_key_jwt` auth method. Client sends JWT assertion
with `alg: none` header. AS validates client_id and iss claims without cryptographic
verification; issues token because client_id is correct.

## Pair[CB4-3.4-1207]/B

**Node 1 — Domain Splitter**
Input: private_key_jwt client assertion with alg:none; AS accepts without sig verification.
Domain signal: JWT client assertion security; alg:none prohibition; RFC 8725 §2.1.
Output → domain-token: `OAUTH2-CLIENT-ASSERTION`; sub-domain: `private-key-jwt-alg-none`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: RFC 8725 §2.1 / RFC 7523.
Scope signal: MUST NOT use alg:none for signed JWTs; applies to client assertions;
unsigned assertion proves no key possession.
Output → sub-phase assignment: `3.4-CLIENT-ASSERTION-ALG-NONE-MUST-NOT`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- RFC 8725 §2.1: MUST NOT use alg:none for signed JWTs (universal; includes client assertions)
- RFC 7523: JWT client assertion authentication
Scheme: verify AS rejects alg:none in client assertions; client_id-alone argument rejected.
Output → scheme: `CLIENT-ASSERTION-ALG-NONE-MUST-NOT-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: alg:none client assertion accepted; no cryptographic key possession proof;
any caller knowing client_id can forge and submit; MUST NOT violated.
Output → violation-record: `CLIENT_ASSERTION_ALG_NONE_RFC8725_S2.1_MUST_NOT_VIOLATED`.

**Node 5 — Applier**
Applies RFC 8725 §2.1 MUST NOT: alg:none prohibited universally including client assertions;
private_key_jwt purpose is key possession proof; unsigned assertion provides none.
Output → application-result: `RFC8725-S2.1-MUST-NOT-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: MUST NOT violation; client impersonation possible by any caller knowing client_id.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: HIGH.

**Node 7 — Arbiter Selector**
Selects RFC 8725 §2.1 / client assertion alg:none arbiter rule.
Rule: alg:none MUST NOT in client assertions; unsigned assertion → REPROCESS.
Output → arbiter-rule: `RFC8725-S2.1-CLIENT-ASSERTION-ALG-NONE-REPROCESS`.

**Node 8 — Arbiter**
Final determination: RFC 8725 §2.1 MUST NOT violated; alg:none in client assertion;
client_id-alone argument rejected; AS provided unauthenticated tokens.
Output → verdict: `REPROCESS`; citation: `RFC 8725 §2.1 MUST NOT alg:none; RFC 7523`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — RFC 8725 §2.1 MUST NOT applies universally to all signed JWT contexts
including JWT client assertions (RFC 7523). An unsigned client assertion provides no proof
of key possession; accepting it based on `client_id` alone allows any caller to impersonate
any registered client. MUST NOT violated; REPROCESS.

## Pair[CB4-3.4-1207]/Reason

RFC 8725 §2.1 MUST NOT alg:none applies to all signed JWTs including private_key_jwt
client assertions; unsigned assertion proves no key possession; client_id-alone rejected.

## Pair[CB4-3.4-1207]/Phrasing-B

Verdict REPROCESS. Node 1 identifies alg:none in private_key_jwt assertion. Node 3 locks
RFC 8725 §2.1 (MUST NOT alg:none universally) and RFC 7523. Node 5 applies: MUST NOT is
universal; client assertions are signed JWTs; purpose is key possession proof. Nodes 6–8
confirm REPROCESS: unsigned assertion accepted; MUST NOT violated.

---

## Pair[CB4-3.4-1208]/A

OIDC Discovery document lists `userinfo_endpoint: http://as.example.com/userinfo`
(plain HTTP). AS argues only token endpoint and authorization endpoint require HTTPS;
UserInfo is "read-only" and lower priority.

## Pair[CB4-3.4-1208]/B

**Node 1 — Domain Splitter**
Input: userinfo_endpoint URL uses HTTP; AS argues scoped HTTPS requirement.
Domain signal: OIDC UserInfo endpoint transport security; OIDC Core §6.
Output → domain-token: `OIDC-USERINFO-ENDPOINT-TRANSPORT`; sub-domain: `userinfo-http`.

**Node 2 — Domain Translator**
Maps to sub-phase 3.4 security-verification flow.
Standard family: OIDC Core §6.
Scope signal: MUST use HTTPS for all AS endpoints; OIDC Core §6 is not limited to
credential-handling endpoints; UserInfo transmits PII.
Output → sub-phase assignment: `3.4-USERINFO-ENDPOINT-MUST-HTTPS`.

**Node 3 — Scheme Planner**
Applicable normative sources:
- OIDC Core §6: AS MUST use HTTPS for all endpoints
Scheme: verify HTTPS on userinfo_endpoint URL; credential-endpoints-only argument rejected.
Output → scheme: `USERINFO-HTTPS-MUST-CHECK`; citation set locked.

**Node 4 — Domain Processor**
Extracts violation: userinfo_endpoint on HTTP; PII transmitted in plaintext; Bearer token
used to access it also exposed on HTTP; OIDC Core §6 MUST HTTPS for all endpoints violated.
Output → violation-record: `USERINFO_ENDPOINT_HTTP_OIDC_CORE_S6_MUST_VIOLATED`.

**Node 5 — Applier**
Applies OIDC Core §6 MUST HTTPS: all AS endpoints require HTTPS; read-only / scoped argument
rejected; UserInfo transmits PII and Bearer token in its interaction.
Output → application-result: `OIDC-CORE-S6-MUST-HTTPS-VIOLATED`; confidence HIGH.

**Node 6 — Result Translator**
Translates: MUST violation; UserInfo endpoint on HTTP transmits PII unencrypted.
Preliminary verdict: REPROCESS.
Output → preliminary-verdict: `REPROCESS`; severity: HIGH.

**Node 7 — Arbiter Selector**
Selects OIDC Core §6 all-endpoints HTTPS MUST arbiter rule.
Rule: all AS endpoints MUST HTTPS; userinfo_endpoint on HTTP → REPROCESS.
Output → arbiter-rule: `OIDC-CORE-S6-ALL-ENDPOINTS-HTTPS-REPROCESS`.

**Node 8 — Arbiter**
Final determination: OIDC Core §6 MUST HTTPS all endpoints; userinfo_endpoint HTTP;
scoped-HTTPS argument rejected.
Output → verdict: `REPROCESS`; citation: `OIDC Core §6 MUST HTTPS all endpoints`.

**Node 9 — Formatter**
Formats final output.
Verdict: REPROCESS — OIDC Core §6 MUST: the authorization server must use HTTPS for
all endpoints. The MUST is not limited to credential-handling endpoints; it covers all
AS endpoints including UserInfo. UserInfo transmits user PII and is accessed via Bearer
token — both require transport security. The scoped-HTTPS argument is rejected.

## Pair[CB4-3.4-1208]/Reason

OIDC Core §6 MUST HTTPS for all AS endpoints; userinfo_endpoint HTTP violates MUST;
scoped-HTTPS argument rejected; UserInfo transmits PII and Bearer token interaction.

## Pair[CB4-3.4-1208]/Phrasing-B

Verdict REPROCESS. Node 1 identifies userinfo_endpoint on HTTP. Node 3 locks OIDC Core §6
(MUST HTTPS all endpoints). Node 5 applies: MUST is unconditional; not scoped to
credential-handling endpoints; UserInfo transmits PII; scoped-HTTPS argument rejected.
Nodes 6–8 confirm REPROCESS: MUST HTTPS violated.
