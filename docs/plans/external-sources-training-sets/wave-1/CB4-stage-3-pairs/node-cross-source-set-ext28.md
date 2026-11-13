# CB4 Stage 3 — Cross-Source Pair Set ext28
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-923 through CB4-3.4-932 (10 base pairs × 2 phrasings = 20 instances)
# Source domains: ext35 patterns (mobile-client-secret, token-type, JWT-PII, confidential-PKCE)
#   combined with each other and with earlier patterns
# Running total after this file: 120 + (932 − 60) × 2 = 1864 instances (932 base pairs)
# Graduation rule: REPROCESS overrides CONCERN; N CONCERNs alone ≠ REPROCESS
# PO.1 escalation: PO.1 absence + traceable production MUST violation = REPROCESS

---

## Pair[CB4-3.4-923-A]

**Territory:** T-asvs-token · T-oauth-oidc · T-cwe-authn
**Domains combined:** mobile-hardcoded-client-secret (ext35) + UserInfo-scope-violation (ext34)
**Expected verdict:** REPROCESS

**A:**
A mobile OAuth client embeds its client_secret directly in the application binary (APK and IPA). The secret is compiled into a constants file alongside the bundle identifier and redirect URI. Separately, the OIDC UserInfo endpoint is implemented to return all claims stored for the authenticated user — name, email, phone, address, and custom profile fields — regardless of which scopes the access token carries. The implementation comment reads: "if the user authenticated with this token, they are entitled to their own data." The access token scope in the test run carries only `openid`.

**B:**
REPROCESS. Two independent MUST-level violations are present. First: ASVS V9.2.1 prohibits hardcoded credentials in any application component; embedding client_secret in a binary violates this unconditionally regardless of obfuscation or extraction difficulty. CWE-798 applies. RFC 8252 §8.4 additionally classifies native apps as public clients for which no client_secret is appropriate or expected — the correct remediation is to register as a public client with PKCE S256, removing the credential entirely rather than better-hiding it. Second: OIDC Core §5.3.3 MUST requires the UserInfo endpoint to return only claims authorized by the presented access token's scope; §5.4 defines the scope-to-claim mapping: `openid` authorizes `sub` only. Returning email, phone, and address on a token carrying `openid` scope violates both the MUST and the user's consent boundary — the user consented to identity assertion, not profile disclosure. Either MUST violation alone mandates REPROCESS; both present = REPROCESS without qualification.

**Reason:** Two distinct MUST violations from separate standards (ASVS V9.2.1 + OIDC Core §5.3.3) affecting different system layers (client credential storage + server-side claim filtering). The "authenticated = entitled to own data" argument conflates token validity with scope entitlement and is rejected: scope is the authorization grant for specific claims, independent of authentication.

**Phrasing-B:**
REPROCESS. The system contains two unambiguous MUST failures. ASVS V9.2.1: client_secret compiled into an application binary is a hardcoded credential regardless of how deeply it is embedded; CWE-798; RFC 8252 confirms native apps must operate as public clients without secrets. OIDC Core §5.3.3: UserInfo MUST scope-filter claims; `openid` scope grants access to `sub` alone (§5.4); returning email, phone, and address beyond the authorized scope boundary violates both the RFC MUST and the user's consent scope. Two MUST violations = REPROCESS. The "own data" rationale does not override the scope constraint: scope controls which claims the RP is authorized to receive, not which data the user owns.

---

## Pair[CB4-3.4-924-A]

**Territory:** T-rfc-oauth2 · T-asvs-authentication · T-cwe-authz
**Domains combined:** state-not-validated (ext34) + token-type-absent (ext35)
**Expected verdict:** REPROCESS

**A:**
An OAuth 2.0 authorization code flow implementation includes the `state` parameter in the authorization request and receives it back at the callback endpoint, where it is logged for debugging purposes but never compared against the session-stored value. The developer notes: "We send state so we comply with the spec; the logged value helps us trace flow issues." Additionally, the token endpoint returns successful responses containing only `access_token` and `expires_in` fields; `token_type` is absent from all token responses. The client application infers the token type from its own configuration rather than from the response.

**B:**
REPROCESS. Two independent MUST-level violations are present. First: RFC 6749 §10.12 MUST requires CSRF protection for the authorization callback; the `state` mechanism satisfies this requirement only through validation — transmission without comparison provides zero protection. An attacker can inject their own authorization code into a victim's callback session; without state comparison, the victim is silently bound to the attacker's authorization grant. "Sending for logging" is rejected as a substitute for validation. Second: RFC 6749 §5.1 MUST requires `token_type` in every successful token endpoint response; `token_type` is a self-describing field that allows clients to programmatically determine the Authorization header scheme. Client-side inference from configuration is not a valid substitute — the MUST mandates the server-side response include the field. The two violations are independent and each alone requires REPROCESS.

**Reason:** RFC 6749 §10.12 (CSRF MUST via state validation) + RFC 6749 §5.1 (token_type MUST in response). The interaction between these gaps compounds risk: an attacker who CSRF-injects a code can also exploit a client that may misapply tokens if it cannot reliably determine their type from the response. Both MUSTs are unconditional.

**Phrasing-B:**
REPROCESS. Two RFC 6749 MUST violations in the same implementation. §10.12: state must be validated at the callback — the CSRF protection property is delivered by the comparison against the session-stored value, not by the transmission alone; logging without validating provides no attack-blocking capability; an attacker with network position can CSRF an authorization code injection. §5.1: token_type must appear in successful token responses; clients must not infer type from external configuration; the field provides the programmatic basis for constructing Authorization headers correctly. Both violations are unconditional MUSTs with no exception clauses. REPROCESS on each.

---

## Pair[CB4-3.4-925-A]

**Territory:** T-asvs-token
**Domains combined:** JWT-PII-email (ext35) + token-storage-localStorage (ext34)
**Expected verdict:** CONCERN (graduation: two CONCERNs do not escalate to REPROCESS)

**A:**
An SPA (Single-Page Application) stores access tokens in browser `localStorage`. Each access token is a JWT whose payload contains a `custom_email` claim carrying the user's email address, alongside standard claims (`sub`, `iss`, `aud`, `exp`, `iat`, `scope`). The JWT payload is not encrypted. The developer reasoning: "Both storage in localStorage and PII in tokens are common patterns; our HTTPS deployment and short 15-minute TTL mitigate any risk."

**B:**
CONCERN. Two SHOULD-level gaps are present. First: ASVS V9.3.1 guidance recommends against placing PII in JWT payloads that will be transmitted to and stored at relying parties; the email claim is readable by all bearers, logged at resource servers, and persisted in `localStorage` storage; "HTTPS protects in transit" does not address the at-rest and in-log exposure. Second: V9.3.1 also addresses token storage; `localStorage` is JavaScript-accessible, meaning XSS vulnerabilities can continuously re-exfiltrate tokens, defeating the TTL mitigation argument. Neither gap involves a normative MUST violation — both derive from SHOULD-level guidance. Graduation rule: N CONCERNs do not escalate to REPROCESS; the combined effect is a compound CONCERN with heightened urgency. The two gaps interact: PII in a JWT stored in XSS-accessible storage means exfiltration of the token also directly exfiltrates the user's email. The correct posture — standard non-PII claims in module-level memory storage — would clear both concerns simultaneously.

**Reason:** ASVS V9.3.1 governs both JWT claim content and token storage guidance. Both gaps are SHOULD-level. Graduation rule applies: two CONCERNs = CONCERN, not REPROCESS. The "common patterns" argument is noted but does not convert SHOULD violations to acceptable practice. The compounding effect (PII + XSS-accessible storage) elevates the CONCERN's severity description but does not change the verdict category.

**Phrasing-B:**
CONCERN. Two SHOULD-level deficiencies under ASVS V9.3.1: (1) email claim in unencrypted JWT payload exposes PII to all token bearers, RS logs, and storage layers — "HTTPS in transit" does not address post-transit exposure; (2) localStorage token storage is readable by any JavaScript in the page context, allowing XSS-driven continuous re-exfiltration that the 15-minute TTL does not eliminate. Graduation rule: multiple CONCERNs remain a CONCERN — they do not combine to REPROCESS. The interaction between these two gaps is noteworthy: the same XSS that exfiltrates the token also directly exfiltrates the email address embedded in it. Recommend: standard non-PII JWT claims + module-level in-memory storage.

---

## Pair[CB4-3.4-926-A]

**Territory:** T-ssdf-process · T-rfc-oauth2
**Domains combined:** PO.1-absent (from prior sessions) + token-type-absent (ext35) — PO.1 escalation rule
**Expected verdict:** REPROCESS (PO.1 escalation: PO.1 gap + traceable production MUST violation)

**A:**
The organization has no formal security training program for its development team. Security knowledge is acquired informally — developers learn through blog posts and peer recommendations. There is no SSDF PO.1 evidence: no training records, no designated learning resources, no onboarding security module. During a security review, the team's token endpoint implementation is found to omit the `token_type` field from all successful responses, in violation of RFC 6749 §5.1 MUST. When asked, the lead developer states: "I wasn't aware `token_type` was mandatory; the spec is long and we focused on the `access_token` field."

**B:**
REPROCESS. PO.1 escalation rule applies: PO.1 absence alone is CONCERN; PO.1 absence combined with a traceable production MUST violation is REPROCESS. The developer's statement directly traces the RFC 6749 §5.1 MUST violation to a knowledge gap that a PO.1-compliant training program would address — awareness of the RFC 6749 §5.1 token response mandatory fields is foundational OAuth knowledge. The MUST violation itself (token_type absent) is independently REPROCESS-eligible: RFC 6749 §5.1 unconditionally requires `token_type` in all successful token endpoint responses; "the spec is long" is not an exception clause. The PO.1 absence is the systemic root cause; the §5.1 violation is the production evidence of that root cause. Together they satisfy the full escalation condition. The appropriate remediation addresses both: immediate fix of token response format + establishment of PO.1-compliant training program.

**Reason:** PO.1 escalation: PO.1 gap (no formal security training, no records) + RFC 6749 §5.1 MUST violation (token_type absent) + direct causal link between gap and violation (developer explicitly traces the §5.1 miss to not knowing the requirement). Escalation from CONCERN to REPROCESS per established rule.

**Phrasing-B:**
REPROCESS. PO.1 (SSDF organizational training requirement) is absent — no training program, no records, informal knowledge only. Standing alone this is CONCERN. However, the developer's explanation — "I wasn't aware token_type was mandatory" — directly attributes the RFC 6749 §5.1 MUST violation to the absence of the training that PO.1 requires. This causal link triggers the PO.1 escalation rule: PO.1 absence + production MUST violation traceable to that absence = REPROCESS. The §5.1 violation (token_type missing) is itself unconditional REPROCESS material. Combined: REPROCESS on dual grounds.

---

## Pair[CB4-3.4-927-A]

**Territory:** T-rfc-oauth2 · T-rfc-oidc · T-oauth-oidc
**Domains combined:** confidential-no-PKCE (ext35) + acr-not-validated (ext34)
**Expected verdict:** CONCERN (graduation: two CONCERNs do not escalate to REPROCESS)

**A:**
A confidential client web application implements the authorization code flow. The client authenticates at the token endpoint with `client_secret_basic`. PKCE is not used — the developer reasoning is: "We are a confidential client with a properly secured secret; PKCE is for public clients." Additionally, the application requests `acr_values=2` (two-factor authentication required) in the authorization request but does not validate the `acr` claim in the returned ID token. The developer comment: "We trust our identity provider to enforce the acr_values we request."

**B:**
CONCERN. Two SHOULD-level gaps are present. First: RFC 9700 §2.1.1 uses SHOULD (not MUST) when recommending PKCE for confidential clients; the client_secret does partially mitigate the code interception threat at the token exchange step; however, PKCE would also protect against authorization code injection attacks that client_secret does not address. SHOULD violation = CONCERN. Second: OIDC Core §3.1.3.7.9 uses SHOULD when specifying that RPs validate the `acr` claim against the requested level; the AS may silently return `acr=1` when `acr=2` was requested (downgrade without notification); without validation, the RP cannot detect MFA bypass. SHOULD violation = CONCERN. Graduation rule: two CONCERNs do not escalate to REPROCESS. The combined effect represents a system where both an active defense layer (PKCE injection protection) and a passive assurance check (acr validation) are absent, but neither gap reaches the MUST-violation threshold for REPROCESS.

**Reason:** RFC 9700 §2.1.1 SHOULD (confidential client PKCE) + OIDC Core §3.1.3.7.9 SHOULD (acr validation). Both are SHOULD-level. Graduation rule: N CONCERNs = CONCERN. The "trust the IdP" argument for acr is noted but rejected — trust does not substitute for verification when the RP has a defined SHOULD obligation to check.

**Phrasing-B:**
CONCERN. RFC 9700 §2.1.1 SHOULD for confidential client PKCE: client_secret mitigates token exchange attacks but not authorization code injection; absence of PKCE leaves a SHOULD gap — CONCERN. OIDC Core §3.1.3.7.9 SHOULD for acr validation: the RP must validate the returned acr value against the requested level; an AS can silently downgrade; "trusting the IdP" does not satisfy the RP-side verification SHOULD — CONCERN. Graduation rule: two independent CONCERNs = CONCERN verdict. No MUST violations present; REPROCESS is not warranted.

---

## Pair[CB4-3.4-928-A]

**Territory:** T-asvs-token · T-rfc-oauth2 · T-cwe-authn
**Domains combined:** mobile-hardcoded-client-secret (ext35) + auth-code-24h-lifetime (ext32)
**Expected verdict:** REPROCESS

**A:**
A mobile application embeds its OAuth `client_secret` in the compiled binary. The application was designed before RFC 8252 was broadly adopted, and the developer intended the secret to serve as a second factor preventing unauthorized token exchange. Additionally, the authorization server issues authorization codes with a 24-hour expiry, configured to accommodate users who may start an authorization flow and complete it the following day. The AS developer notes: "Single-use enforcement is implemented; replay is therefore impossible regardless of code lifetime."

**B:**
REPROCESS. Two independent MUST-level violations are present. First: ASVS V9.2.1 MUST prohibits hardcoded credentials regardless of intent; a client_secret compiled into a binary is extractable through static analysis, disassembly, or memory inspection — obfuscation provides deterrence, not security. CWE-798. RFC 8252 §8.4 confirms native apps are public clients for which client_secret is inappropriate by design; the correct architecture eliminates the credential rather than hardening its storage. Second: RFC 6749 §4.1.2 MUST requires authorization codes to expire shortly after issuance; "shortly" is universally interpreted as minutes (RFC 6749 references 10 minutes as a calibration point); 24 hours is not "shortly" by any reasonable interpretation. The "single-use + replay impossible" argument is rejected: single-use addresses code replay after one successful exchange; a 24-hour window gives an attacker who intercepts the code (via network MITM, server log, or browser history) a full day to initiate the token exchange before the user does. These are different threat categories; both MUST conditions must be independently satisfied.

**Reason:** ASVS V9.2.1 (hardcoded credentials MUST) + RFC 6749 §4.1.2 (short code lifetime MUST). Two independent MUST violations. The "second factor" framing for client_secret in a mobile app does not survive RFC 8252 analysis. The "single-use substitutes for short lifetime" argument addresses code replay, not code interception — rejected per established ext32 pattern.

**Phrasing-B:**
REPROCESS. ASVS V9.2.1 MUST: embedding client_secret in a binary is a hardcoded credential; CWE-798; RFC 8252: native apps are public clients requiring no secret — the proper fix is architectural (public client registration + PKCE), not better obfuscation. RFC 6749 §4.1.2 MUST: authorization codes must expire shortly; 24 hours is not "shortly"; the single-use enforcement prevents replay of an already-used code but cannot prevent first use by an attacker who intercepts the code during the 24-hour window — these are distinct threat vectors requiring distinct controls. Both MUSTs independently require REPROCESS.

---

## Pair[CB4-3.4-929-A]

**Territory:** T-asvs-token · T-rfc-jwt
**Domains combined:** JWT-PII-email (ext35) + jti-absent (ext32)
**Expected verdict:** CONCERN (graduation: two CONCERNs do not escalate to REPROCESS)

**A:**
JWT access tokens in the system contain a `custom_email` claim (user's email address) in the unencrypted payload, alongside standard claims. The JWT payload also lacks a `jti` claim — no per-token unique identifier is issued. The system does implement a 15-minute TTL via `exp`. The argument: "JWT payloads are not secrets; users know their own email. And without a token store, jti is operationally unnecessary for our stateless architecture."

**B:**
CONCERN. Two SHOULD-level gaps are present. First: ASVS V9.3.1 guidance addresses PII in JWT payloads; email in an unencrypted token is visible to all bearers, logged at resource servers, and exposed in any storage or debug context — the "user knows their email" argument does not address the exposure of the email to non-user parties who process the token; "HTTPS in transit" does not address at-rest and in-log exposure. SHOULD-level gap = CONCERN. Second: RFC 7519 §4.1.7 uses SHOULD for the `jti` claim; `jti` enables precision per-token revocation before TTL expiry and per-token audit correlation; without `jti`, the system cannot revoke a specific token without revoking all tokens for the user, and cannot correlate audit events to individual token instances. The "stateless architecture" argument is noted as a valid design consideration but does not eliminate the SHOULD gap. SHOULD-level gap = CONCERN. Graduation rule: two CONCERNs do not escalate to REPROCESS. The combined effect is notable: PII tokens without jti cannot be individually tracked or revoked — if the email-bearing token is exfiltrated, there is no targeted revocation path; only TTL provides any bound on the exposure window.

**Reason:** ASVS V9.3.1 (PII in JWT SHOULD) + RFC 7519 §4.1.7 (jti SHOULD). Both SHOULD-level. Graduation rule: N CONCERNs = CONCERN. The interaction between these gaps is meaningful (PII token without targeted revocation) but does not create a new MUST violation — the combined gap description belongs in CONCERN with heightened urgency.

**Phrasing-B:**
CONCERN. ASVS V9.3.1: email in unencrypted JWT payload exposes PII to RS logs, storage, and all bearers beyond the user themselves — "user knows their email" addresses user awareness, not third-party exposure; SHOULD gap = CONCERN. RFC 7519 §4.1.7: jti SHOULD; absent jti means no per-token precision revocation, no audit correlation at the token level, no concurrent-use anomaly detection; TTL alone is the only bound; SHOULD gap = CONCERN. Graduation rule applies: two CONCERNs = CONCERN. The interaction — PII-bearing token without targeted revocation — is a compound risk that elevates the CONCERN description but does not change the verdict category.

---

## Pair[CB4-3.4-930-A]

**Territory:** T-rfc-oauth2 · T-rfc-jwt
**Domains combined:** token-type-absent (ext35) + sub-absent (ext32/RFC 9068)
**Expected verdict:** REPROCESS

**A:**
A token endpoint returns successful responses with `access_token` and `expires_in` but omits `token_type`. The access tokens themselves are JWTs (RFC 9068 profile) that contain `iss`, `aud`, `exp`, `iat`, and `scope` claims but no `sub` claim. The developer explanation: "The `token_type` is obvious — every token is a Bearer token in this system. And we don't need `sub` because we have a `client_id` claim identifying the application; user identity is determined by the application session, not the token."

**B:**
REPROCESS. Two independent MUST-level violations are present. First: RFC 6749 §5.1 MUST requires `token_type` in every successful token endpoint response; the server's knowledge that all tokens are Bearer tokens does not satisfy the requirement — the MUST is specifically designed so that clients can determine the type programmatically from the response rather than relying on implementation-specific conventions. Second: RFC 9068 §2.2 MUST requires the `sub` claim in JWT access tokens; `sub` identifies the resource owner on whose behalf the token was issued, enabling per-user authorization policies, user-level revocation, and user-attributable audit logs at the resource server. `client_id` identifies the application but is not a substitute for the resource owner subject — machine-to-machine client credential flows are distinguished from delegated user flows precisely through `sub`. "User identity determined by application session" is an application-layer concern; the RS cannot access the application session — it relies on the token's `sub` claim for user-scoped policy decisions. Both MUSTs independently require REPROCESS.

**Reason:** RFC 6749 §5.1 (token_type MUST in response) + RFC 9068 §2.2 (sub MUST in JWT access tokens). Two independent MUST violations. The "obvious" and "client_id substitutes" arguments are both rejected: each MUST is self-standing and non-delegable to implementation convention or alternate claims.

**Phrasing-B:**
REPROCESS. RFC 6749 §5.1: token_type must be present in the response; "everyone knows it's Bearer" does not satisfy the MUST — the field enables programmatic determination, not human recall; the requirement is unconditional. RFC 9068 §2.2: sub must be present in JWT access tokens; client_id identifies the application, not the resource owner; the RS applies per-user policies using sub; user-level revocation is keyed on sub; audit logs at the RS require a user identifier in the token; application session state is not accessible to the RS. Two independent MUST violations = REPROCESS.

---

## Pair[CB4-3.4-931-A]

**Territory:** T-rfc-oauth2 · T-rfc-jwt
**Domains combined:** confidential-no-PKCE (ext35, CONCERN) + PKCE-code-verifier-from-timestamp (ext33, REPROCESS)
**Expected verdict:** REPROCESS (graduation: REPROCESS overrides CONCERN)

**A:**
A system has two distinct OAuth clients. The first is a confidential web application client that uses the authorization code flow without PKCE, relying solely on its `client_secret` at the token endpoint — the developer's reasoning: "Confidential clients with secrets don't need PKCE per the BCP." The second is a public mobile client that implements PKCE but generates the `code_verifier` from the Unix timestamp in milliseconds, encoded as a decimal string (13 characters). The developer reasoning: "Every request is at a unique millisecond; the verifier is therefore unique per authorization request."

**B:**
REPROCESS. Graduation rule applies: the system contains both a CONCERN-level gap and a REPROCESS-level violation; REPROCESS overrides CONCERN. The confidential client's PKCE absence: RFC 9700 §2.1.1 uses SHOULD for confidential clients; `client_secret` provides partial mitigation; this is a CONCERN not a REPROCESS standing alone. The mobile public client's timestamp-based verifier: RFC 7636 §4.1 MUST requires the `code_verifier` to be generated using a cryptographically secure random number generator (CSPRNG); a Unix timestamp in milliseconds yields approximately 10 bits of entropy per second of real time; with a ±5-second brute-force window, an attacker has approximately 10,000 candidates to test within the authorization code's validity window — enumerable in seconds. Additionally, a 13-character decimal string is below the RFC 7636 §4.1 minimum of 43 unreserved characters. "Unique per millisecond" is rejected: uniqueness ≠ unpredictability; PKCE requires cryptographic unguessability, not merely per-request uniqueness. The REPROCESS verdict for the timestamp verifier violation subsumes the CONCERN for the confidential client PKCE absence; the overall system verdict is REPROCESS.

**Reason:** PKCE timestamp verifier = REPROCESS (RFC 7636 §4.1 MUST CSPRNG + length violation). Confidential-no-PKCE = CONCERN (RFC 9700 §2.1.1 SHOULD). Graduation rule: REPROCESS overrides CONCERN. The system verdict is REPROCESS regardless of how many CONCERNs accompany the MUST violation.

**Phrasing-B:**
REPROCESS. Graduation rule: one REPROCESS + one CONCERN = REPROCESS; CONCERNs do not lower a REPROCESS verdict. The public client code_verifier from Unix timestamp violates RFC 7636 §4.1 MUST (CSPRNG required; ~10 bits/second entropy; 13-char string below 43-char minimum; "unique ≠ unpredictable" argument rejected) — REPROCESS. The confidential client without PKCE violates RFC 9700 §2.1.1 SHOULD only; client_secret provides partial mitigation; CONCERN. The REPROCESS verdict for the timestamp violation controls. Fix both: timestamp verifier → CSPRNG 64-char unreserved string; confidential client → add PKCE S256 as layered defense.

---

## Pair[CB4-3.4-932-A]

**Territory:** T-rfc-oauth2 · T-rfc-jwt · T-asvs-token · T-oauth-oidc
**Domains combined:** comprehensive PASS — public client PKCE + token_type + non-PII JWT + state validation
**Expected verdict:** PASS

**A:**
A mobile application is registered as a public OAuth client with no `client_secret`. The authorization code flow uses PKCE with S256: the `code_verifier` is a 64-character string generated by a CSPRNG using the OS's secure entropy source; the `code_challenge` is computed as `BASE64URL(SHA256(ASCII(code_verifier)))` and transmitted as `code_challenge_method=S256`. The token endpoint returns successful responses with: `access_token`, `token_type=Bearer`, `expires_in=900`, `refresh_token`, and `scope`. JWT access tokens contain only: `sub` (opaque UUID v4 PPID — no PII), `iss`, `aud` (RS identifier), `exp`, `iat`, `jti` (UUID v4), `scope`. The authorization request includes a 256-bit CSPRNG-generated `state` value, stored in the session; the callback handler validates the returned `state` against the session value before proceeding; state values are single-use and absent state is rejected.

**B:**
PASS. No arbiter violations found. Public client registration (RFC 8252 §8.4): correct architecture — no `client_secret` hardcoded or otherwise. PKCE S256 (RFC 7636 §4.1): 64-character CSPRNG verifier satisfies the high-entropy MUST; S256 method satisfies §4.2 MUST for S256-capable clients; challenge computation is correct. Token response (RFC 6749 §5.1): all MUST fields present — `access_token`, `token_type`, `expires_in`; token_type=Bearer is correct. JWT claims (RFC 9068 §2.2): `sub` present (opaque PPID — satisfies MUST without PII); `iss`, `aud`, `exp`, `iat` present; `jti` present (enables precision revocation). No PII in JWT payload (ASVS V9.3.1): email absent; sub is opaque PPID; V9.3.1 guidance satisfied. State CSRF protection (RFC 6749 §10.12): 256-bit CSPRNG generates unpredictable state; exact match validated at callback; single-use; absent rejected — CSRF protection fully operational. All ext35 and ext34 PASS conditions met simultaneously.

**Reason:** All four ext35 PASS conditions satisfied: public client (no hardcoded secret), token_type=Bearer present, non-PII JWT claims, plus RFC 6749 §10.12 state validation from ext34 PASS pattern. No MUST or SHOULD violations across any checked standard. Correct implementation of layered mobile OAuth security in a single integrated flow.

**Phrasing-B:**
PASS. The implementation satisfies all checked requirements. RFC 8252: public client without secret is correct architecture for mobile. RFC 7636 §4.1/§4.2: CSPRNG 64-char verifier + S256 method — MUST satisfied. RFC 6749 §5.1: token_type=Bearer + all MUST response fields present. RFC 9068 §2.2: sub (opaque PPID), aud, exp, iat all present. RFC 7519 §4.1.7: jti present — precision revocation enabled. ASVS V9.3.1: JWT carries no PII; opaque sub satisfies user identity without exposing personal data. RFC 6749 §10.12: 256-bit CSPRNG state, validated at callback, single-use, absent rejected — CSRF protection complete. No violations found. PASS.
