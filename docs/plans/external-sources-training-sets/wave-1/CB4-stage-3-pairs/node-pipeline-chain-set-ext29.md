# CB4 Stage 3 — Action 3 Pipeline Chain Set
# Node: Multi-node (full 9-node traces) — ext29
# Sequences: CB4-3.4-915 through CB4-3.4-922
# Instances: 16 (8 base pairs × 2 phrasings each)
# Stratum: C (full pipeline traces for ext35 patterns)
# Running total after this file: 1844 instances (922 base pairs)

---

## Pair[CB4-3.4-915]

### A
Full pipeline trace — hardcoded client_secret in mobile app binary (ASVS V9.2.1 / CWE-798):

A native mobile OAuth client has client_secret hardcoded as a string constant in the app source code, compiled into the distributed binary. The team applies code obfuscation and argues extraction requires significant effort.

### B
Node 1 — Domain Splitter: Input domain: T-asvs-authentication / T-cwe-authn. Sub-domain: mobile OAuth client credential management — hardcoded client_secret in distributed binary. CWE mapping: CWE-798 (Use of Hardcoded Credentials). Pipeline route: Node 8 ASVS arbiter with V9.2.1 MUST analysis.

Node 2 — Domain Translator: ASVS V9.2.1: "Verify that the application does not contain hardcoded credentials." MUST-level. CWE-798: hardcoded credentials are a recognized critical vulnerability class. RFC 8252 (OAuth 2.0 for Native Apps): native apps should be public clients — a client_secret is architecturally inappropriate for distributed native apps. Obfuscation context: not a security control per OWASP guidelines.

Node 3 — Scheme Planner: Verify client_secret is hardcoded (compiled into binary). Evaluate obfuscation as a security control. Assess attack path for extracting hardcoded secrets from distributed binaries. Evaluate team's effort argument. Expected: REPROCESS if MUST violated.

Node 4 — Domain Processor: client_secret is a string constant in source code → compiled into binary → distributed to all users. The same binary (and thus the same client_secret) is on every device. Reverse-engineering tools: jadx/Hopper/IDA Pro can decompile or disassemble mobile binaries with varying effort. Obfuscation raises cost of extraction — it does not make extraction impossible. ASVS V9.2.1 MUST: "does not contain hardcoded credentials." The client_secret is a credential. The binary contains it. MUST violated. "Significant effort" argument: MUST requirements cannot be satisfied by making violation harder — they require the condition to not exist. RFC 8252: native app should be registered as public client with PKCE; no secret needed.

Node 5 — Applier: ASVS V9.2.1 MUST hardcoded credentials violated. CWE-798. Obfuscation rejected as security substitute. REPROCESS.

Node 6 — Result Translator: REPROCESS. client_secret in mobile binary is hardcoded. Distributed to all users. Extractable regardless of obfuscation. ASVS V9.2.1 MUST violated.

Node 7 — Arbiter Selector: Route to Node 8 ASVS arbiter (T-asvs-authentication) + T-cwe-authn (CWE-798). Both confirm REPROCESS. No PO.1 escalation needed (already REPROCESS).

Node 8 — Arbiter Units: REPROCESS. ASVS V9.2.1 MUST no hardcoded credentials: violated. CWE-798. Mobile binary distributed to all users — same secret on every device. Obfuscation rejected: deterrent not security control. RFC 8252: native app = public client; use PKCE. REPROCESS.

Node 9 — Formatter: REPROCESS. ASVS V9.2.1 MUST: hardcoded client_secret in mobile binary. CWE-798. Any user receiving the binary can extract the secret with reverse-engineering tools; obfuscation raises cost but does not provide security. Correct architecture: register as public client (no client_secret); use PKCE S256 per RFC 8252 and RFC 9700 §2.1.1.

### Reason
9-node trace: ASVS V9.2.1 MUST hardcoded credentials violated; mobile binary distributed to all users; obfuscation rejected as security control; RFC 8252 identifies native apps as public clients. REPROCESS.

### Phrasing-B
REPROCESS — 9-node trace: hardcoded client_secret in mobile binary, ASVS V9.2.1 MUST, CWE-798.

N1: T-asvs-authentication + CWE-798 / hardcoded credential in distributed binary.
N2: ASVS V9.2.1 MUST; CWE-798; RFC 8252 native apps = public clients.
N3: Verify hardcoded credential; evaluate obfuscation argument; assess attack path.
N4: client_secret in binary distributed to all users; reverse-engineering tools available; obfuscation ≠ security; MUST violated.
N5: MUST violated; CWE-798. REPROCESS.
N6: REPROCESS — hardcoded secret in distributed binary.
N7: ASVS + T-cwe-authn CWE-798; both REPROCESS.
N8: REPROCESS — V9.2.1 MUST; obfuscation rejected; RFC 8252.
N9: REPROCESS. Register as public client with PKCE S256.

---

## Pair[CB4-3.4-916]

### A
Full pipeline trace — mobile app as public client with PKCE S256; no client_secret:

A native mobile OAuth client is registered as a public client. No client_secret exists. Each authorization code flow uses PKCE S256 with a CSPRNG code_verifier.

### B
Node 1 — Domain Splitter: Input domain: T-asvs-authentication / T-rfc-oauth2. Sub-domain: mobile OAuth client architecture — public client with PKCE. Pipeline route: Node 8 ASVS/RFC arbiter with positive compliance confirmation.

Node 2 — Domain Translator: ASVS V9.2.1: no hardcoded credentials — satisfied by design (no credentials to hardcode). RFC 8252 §8.4: native apps as public clients. RFC 9700 §2.1.1 MUST PKCE for public clients. PKCE S256: code exchange protection without distributable secrets.

Node 3 — Scheme Planner: Verify no client_secret registered. Verify PKCE S256. Verify CSPRNG verifier. Expected: PASS if all controls confirmed.

Node 4 — Domain Processor: No client_secret registered — V9.2.1 and CWE-798 risks eliminated by design. PKCE S256: CSPRNG verifier per request. RFC 9700 §2.1.1 MUST satisfied. Code interception requires both code and verifier. No credentials in binary. All controls confirmed.

Node 5 — Applier: ASVS V9.2.1 satisfied by design. RFC 8252 + RFC 9700 §2.1.1 satisfied. PASS.

Node 6 — Result Translator: PASS. Public client with PKCE S256. No credential to hardcode. CWE-798 not applicable.

Node 7 — Arbiter Selector: Route to Node 8 ASVS/RFC arbiter. Positive compliance.

Node 8 — Arbiter Units: PASS. V9.2.1 satisfied by design (no credentials). RFC 8252 native app public client architecture correct. RFC 9700 §2.1.1 PKCE satisfied. No findings.

Node 9 — Formatter: PASS. Native mobile app as public client. No client_secret — ASVS V9.2.1 and CWE-798 risks eliminated by design. PKCE S256 with CSPRNG verifier provides code exchange protection. RFC 8252 and RFC 9700 §2.1.1 satisfied.

### Reason
9-node trace confirms public client architecture eliminates hardcoded credential risk by design. PKCE S256 provides code protection. RFC 8252 + RFC 9700 §2.1.1 satisfied. PASS.

### Phrasing-B
PASS — 9-node trace: mobile app as public client with PKCE S256; no client_secret; RFC 8252 + RFC 9700 §2.1.1.

N1: T-asvs-authentication / public client PKCE architecture.
N2: V9.2.1 satisfied by design; RFC 8252 §8.4; RFC 9700 §2.1.1 MUST PKCE.
N3: Verify no secret registered; PKCE S256; CSPRNG verifier.
N4: No secret → V9.2.1/CWE-798 eliminated; PKCE S256 confirmed; all controls.
N5: All requirements met. PASS.
N6: PASS — no credentials; PKCE S256.
N7: ASVS/RFC arbiter; positive.
N8: PASS — V9.2.1 by design; RFC 8252; RFC 9700.
N9: PASS. Public client + PKCE S256; no credential risk.

---

## Pair[CB4-3.4-917]

### A
Full pipeline trace — token endpoint successful response missing token_type field:

An authorization server's token endpoint returns access_token and expires_in but omits token_type from the successful response. Team: "Documentation specifies Bearer — clients know what to use."

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oauth2. Sub-domain: token endpoint response format — required fields per RFC 6749 §5.1. Pipeline route: Node 8 RFC arbiter with §5.1 MUST analysis.

Node 2 — Domain Translator: RFC 6749 §5.1: "The authorization server MUST include the HTTP response parameters: access_token, token_type." Both are MUST fields. token_type identifies how the token is to be used: "Bearer" (RFC 6750), "DPoP" (RFC 9449), etc. Documentation cannot substitute for required response parameters.

Node 3 — Scheme Planner: Verify token_type present in response. Evaluate "documentation substitutes" argument. Assess interoperability impact. Expected: REPROCESS if §5.1 MUST violated.

Node 4 — Domain Processor: token_type: absent from response. RFC 6749 §5.1 MUST: violated. "Documentation" argument: RFC 6749 §5.1 requires the response to be self-describing. Out-of-band documentation is not equivalent to a required response field. Standard-conformant OAuth clients check token_type from the response, not from documentation. Without token_type, clients may fail to construct correct Authorization headers, use incorrect header schemes, or fail silently on standards-conformant library implementations that validate response completeness.

Node 5 — Applier: RFC 6749 §5.1 MUST token_type violated. Documentation argument rejected. REPROCESS.

Node 6 — Result Translator: REPROCESS. RFC 6749 §5.1 MUST token_type absent. Non-conformant response. Documentation cannot substitute for required fields.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-oauth2). MUST violation confirmed.

Node 8 — Arbiter Units: REPROCESS. RFC 6749 §5.1 MUST: token_type is a required field in successful token responses. Documentation argument rejected: §5.1 requires self-describing response; out-of-band documentation is not equivalent. Interoperability impact: generic OAuth libraries expect token_type. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 6749 §5.1 MUST violated — token_type absent from token response. Remediation: include token_type=Bearer (RFC 6750) in all successful token responses. Response must be self-describing per §5.1.

### Reason
9-node trace confirms RFC 6749 §5.1 MUST token_type requirement, rejects documentation-as-substitute argument, and produces REPROCESS. Response self-description is the RFC requirement.

### Phrasing-B
REPROCESS — 9-node trace: token_type absent from token endpoint response, RFC 6749 §5.1 MUST violated.

N1: T-rfc-oauth2 / token endpoint response format; §5.1 required fields.
N2: RFC 6749 §5.1 MUST access_token + token_type; documentation not a substitute.
N3: Verify token_type present; evaluate documentation argument; assess interoperability.
N4: token_type absent; §5.1 MUST violated; documentation argument rejected.
N5: MUST violated. REPROCESS.
N6: REPROCESS — §5.1 MUST; self-describing response required.
N7: RFC arbiter T-rfc-oauth2; MUST confirmed.
N8: REPROCESS — §5.1 MUST; token_type required.
N9: REPROCESS. Add token_type=Bearer to all successful token responses.

---

## Pair[CB4-3.4-918]

### A
Full pipeline trace — token endpoint response includes token_type=Bearer and all §5.1 fields:

The authorization server's token endpoint returns token_type=Bearer, access_token, expires_in, refresh_token (where applicable), and scope (when narrowed). All RFC 6749 §5.1 MUST fields are present.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oauth2. Sub-domain: token endpoint response completeness. Pipeline route: Node 8 RFC arbiter with positive compliance confirmation.

Node 2 — Domain Translator: RFC 6749 §5.1: MUST access_token + token_type. §3.3 SHOULD scope when narrowed. token_type=Bearer per RFC 6750. Self-describing response.

Node 3 — Scheme Planner: Verify token_type=Bearer present. Verify access_token present. Verify expires_in present. Verify scope included when narrowed. Expected: PASS.

Node 4 — Domain Processor: token_type=Bearer: correct RFC 6750 value. access_token: present. expires_in: present. refresh_token: present where applicable. scope: present when AS narrowed scope from request. All §5.1 MUST fields satisfied. Self-describing response — clients need no out-of-band documentation to correctly use the token.

Node 5 — Applier: RFC 6749 §5.1 MUST satisfied. PASS.

Node 6 — Result Translator: PASS. All §5.1 MUST fields present. Self-describing response. token_type=Bearer correctly identifies usage pattern.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-oauth2). Positive compliance.

Node 8 — Arbiter Units: PASS. RFC 6749 §5.1 satisfied. token_type=Bearer, access_token, expires_in all present. Scope included when narrowed. No findings.

Node 9 — Formatter: PASS. RFC 6749 §5.1 satisfied. token_type=Bearer: correct. All required fields present. Self-describing response enables correct client behavior without documentation dependency.

### Reason
9-node trace confirms all RFC 6749 §5.1 MUST fields present: token_type=Bearer, access_token, expires_in, refresh_token where applicable, scope when narrowed. PASS.

### Phrasing-B
PASS — 9-node trace: token endpoint response with all §5.1 MUST fields; token_type=Bearer confirmed.

N1: T-rfc-oauth2 / token response completeness.
N2: RFC 6749 §5.1 MUST; token_type=Bearer per RFC 6750; self-describing response.
N3: Verify all MUST fields present.
N4: All §5.1 fields confirmed; self-describing response.
N5: §5.1 satisfied. PASS.
N6: PASS — all required fields; token_type=Bearer.
N7: RFC arbiter T-rfc-oauth2; positive.
N8: PASS — §5.1 satisfied; no findings.
N9: PASS. All fields present; self-describing.

---

## Pair[CB4-3.4-919]

### A
Full pipeline trace — JWT access token contains user email in custom claim, unencrypted:

An authorization server issues JWT access tokens with a custom "email" claim containing the user's email address. The JWT is signed (RS256) but not encrypted. The RS logs these access tokens. Team: "Email helps the RS identify the user without a DB call. Tokens are only transmitted over HTTPS."

### B
Node 1 — Domain Splitter: Input domain: T-asvs-token / T-rfc-jwt. Sub-domain: JWT payload content — PII inclusion in unencrypted access token payload. Secondary concern: RS logging risk. Pipeline route: Node 8 ASVS arbiter with V9.3.1 PII-in-token analysis.

Node 2 — Domain Translator: ASVS V9.3.1: token transport and storage security guidance — PII in unencrypted JWT creates transparency risk. ASVS V8.2.2: MUST NOT log credentials/PII in production logs. JWT structure: BASE64URL-encoded payload is readable by any party holding the token (signed ≠ encrypted). ext20 precedent: "HTTPS protects in transit" is insufficient — token payload is exposed in contexts beyond transit. No unconditional MUST mandating JWE for access tokens containing PII — ASVS V9.3.1 is SHOULD-level guidance.

Node 3 — Scheme Planner: Assess JWT payload transparency (signed vs encrypted). Evaluate HTTPS-protects-transit argument (ext20 pattern). Assess RS logging risk against V8.2.2. Determine normative level. Expected: CONCERN if SHOULD-level without escalating MUST violation.

Node 4 — Domain Processor: JWT signed (RS256) but not encrypted. Payload is BASE64URL-encoded — any party holding the token can decode and read the email. RS receives and logs the token → email enters log pipeline → V8.2.2 PII-in-logs concern raised. HTTPS protects transit only (ext20 established). Email in access token payload is exposed in: RS logs, log aggregators, monitoring systems, error reporting, browser developer tools. "Email helps RS" argument: use opaque sub + UserInfo call instead — same functional result without PII in token payload. No MUST mandating JWE for all access tokens containing PII → SHOULD-level guidance. V8.2.2 logging concern is secondary (depends on whether RS actually logs the full token — which is described as occurring here).

Node 5 — Applier: ASVS V9.3.1 PII-in-token guidance violated. Secondary V8.2.2 logging concern triggered. No unconditional MUST identified. CONCERN.

Node 6 — Result Translator: CONCERN. JWT payload transparency creates PII exposure surface. RS logging amplifies risk. SHOULD-level: CONCERN not REPROCESS.

Node 7 — Arbiter Selector: Route to Node 8 ASVS arbiter (T-asvs-token). SHOULD-level gap. No PO.1 escalation triggered (no MUST).

Node 8 — Arbiter Units: CONCERN. ASVS V9.3.1 PII-in-token guidance. JWT payload transparent to token bearers. RS logging introduces V8.2.2 secondary concern. "HTTPS protects in transit" rejected (ext20). No MUST for JWE on access tokens: CONCERN. Remediation: use opaque sub + UserInfo call; or use JWE; and stop logging full token payloads.

Node 9 — Formatter: CONCERN. JWT access token contains email in unencrypted payload. JWT payload transparent to all token bearers — any party with the token can decode the email address. RS logging of the token creates V8.2.2 secondary concern (PII in logs). HTTPS only protects transit. No unconditional MUST for JWE: CONCERN not REPROCESS. Remediation: remove PII from token payload (use opaque sub + UserInfo); or encrypt payload (JWE); stop logging full tokens in production.

### Reason
9-node trace identifies ASVS V9.3.1 PII-in-token guidance as SHOULD-level, documents JWT payload transparency, raises V8.2.2 secondary concern from RS logging, and produces CONCERN. No unconditional MUST identified.

### Phrasing-B
CONCERN — 9-node trace: JWT with email in unencrypted payload, ASVS V9.3.1 SHOULD, V8.2.2 secondary.

N1: T-asvs-token / PII in JWT payload; logging secondary concern.
N2: V9.3.1 SHOULD guidance; V8.2.2 MUST NOT log PII (secondary); ext20 "HTTPS-transit-only" precedent.
N3: Assess JWT transparency; HTTPS argument; logging risk; normative level.
N4: JWT signed not encrypted; email readable by any bearer; RS logs → email in logs; no MUST for JWE.
N5: SHOULD guidance violated; secondary V8.2.2. CONCERN.
N6: CONCERN — PII in transparent JWT; logging amplifies.
N7: ASVS arbiter T-asvs-token; SHOULD-level; no PO.1.
N8: CONCERN — V9.3.1 guidance; no unconditional MUST.
N9: CONCERN. Remove PII from token; or use JWE; stop logging full tokens.

---

## Pair[CB4-3.4-920]

### A
Full pipeline trace — JWT access tokens contain only non-PII standard claims:

JWT access tokens contain sub (UUID v4 PPID), iss, aud, exp, iat, jti, and scope — no PII. RS calls UserInfo for profile attributes when needed.

### B
Node 1 — Domain Splitter: Input domain: T-asvs-token / T-rfc-jwt. Sub-domain: JWT payload composition — non-PII claims only. Pipeline route: Node 8 ASVS/RFC arbiter with positive compliance confirmation.

Node 2 — Domain Translator: ASVS V9.3.1: token transparency risk eliminated when payload contains no PII. V8.2.2: token logging creates no PII event when payload is non-PII. opaque sub (UUID v4 PPID): user identity without exposing PII. UserInfo separation: RS fetches profile data separately per OIDC Core §5.3.

Node 3 — Scheme Planner: Verify no PII in token payload. Verify sub is opaque PPID. Verify all claims are non-PII. Expected: PASS.

Node 4 — Domain Processor: sub = UUID v4 PPID — no PII. iss, aud, exp, iat, jti, scope — operational non-PII claims. No email, name, phone, or other PII. Payload transparency not a privacy risk. V8.2.2 logging: no PII exposure event. JWE not needed. RS-profile-data pattern: RS calls UserInfo for email/name when needed — separation maintained. All confirmed.

Node 5 — Applier: ASVS V9.3.1 guidance satisfied. V8.2.2 logging risk eliminated. PASS.

Node 6 — Result Translator: PASS. Non-PII JWT payload. Logging risk eliminated. Privacy maintained through separation pattern.

Node 7 — Arbiter Selector: Route to Node 8 ASVS/RFC arbiter. Positive compliance.

Node 8 — Arbiter Units: PASS. V9.3.1 guidance satisfied — non-PII token payload. V8.2.2 logging risk not present. sub = opaque PPID. RS → UserInfo for profile. No findings.

Node 9 — Formatter: PASS. JWT access token with non-PII claims only. ASVS V9.3.1 satisfied. V8.2.2 logging risk eliminated. Privacy maintained: user identity via opaque sub PPID; profile data via UserInfo only when needed.

### Reason
9-node trace confirms non-PII token payload satisfies V9.3.1, eliminates V8.2.2 logging risk, and maintains privacy through opaque sub PPID and UserInfo separation. PASS.

### Phrasing-B
PASS — 9-node trace: JWT with non-PII claims only (sub PPID, iss, aud, exp, iat, jti, scope). ASVS V9.3.1 satisfied.

N1: T-asvs-token / non-PII JWT composition.
N2: V9.3.1 risk eliminated without PII; V8.2.2 logging safe; sub PPID; UserInfo separation.
N3: Verify no PII; sub opaque; all non-PII claims.
N4: All confirmed; no PII; transparent payload not a privacy risk.
N5: V9.3.1 satisfied; V8.2.2 safe. PASS.
N6: PASS — non-PII JWT; privacy maintained.
N7: ASVS/RFC arbiter; positive.
N8: PASS — no PII; no findings.
N9: PASS. Non-PII claims; sub PPID; UserInfo separation.

---

## Pair[CB4-3.4-921]

### A
Full pipeline trace — confidential client authorization code flow without PKCE:

A confidential server-side web application uses authorization code flow without code_challenge. The client authenticates at token endpoint with client_secret_post. AS accepts both PKCE and non-PKCE authorization code requests from confidential clients.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oauth2 / T-asvs-authentication. Sub-domain: PKCE for confidential clients — authorization code flow without code_challenge. Pipeline route: Node 8 RFC arbiter with RFC 9700 §2.1.1 SHOULD analysis.

Node 2 — Domain Translator: RFC 9700 §2.1.1: "Authorization servers SHOULD require PKCE for confidential clients as well." SHOULD-level. RFC 9700 §2.1.1 MUST applies to public clients; SHOULD applies to confidential clients. client_secret provides token endpoint authentication but does not prevent authorization code interception attacks. PKCE adds code-level binding independent of client secret.

Node 3 — Scheme Planner: Verify whether RFC 9700 §2.1.1 applies at MUST or SHOULD level for confidential clients. Evaluate client_secret partial mitigation. Determine normative level. Expected: CONCERN if SHOULD-level for confidential clients.

Node 4 — Domain Processor: Confidential client: client_secret_post authentication at token endpoint — real protection. client_secret prevents unauthorized parties from completing token exchange (without the secret, token exchange fails). However: authorization code interception attack path: attacker intercepts code from referrer header/browser history/server-side redirect log → attempts exchange. With client_secret, attacker needs both code AND client_secret. This is harder than for public clients. But PKCE would add an additional layer: attacker also needs code_verifier. RFC 9700 §2.1.1 SHOULD: not MUST for confidential clients. Gap is real but at SHOULD level. CONCERN.

Node 5 — Applier: RFC 9700 §2.1.1 SHOULD gap for confidential clients. client_secret provides partial mitigation. SHOULD-level: CONCERN.

Node 6 — Result Translator: CONCERN. SHOULD-level gap: RFC 9700 §2.1.1 recommends PKCE for confidential clients too. client_secret partially mitigates code interception risk but PKCE provides additional binding.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-oauth2). SHOULD-level gap. No PO.1 escalation (no MUST violation traceable).

Node 8 — Arbiter Units: CONCERN. RFC 9700 §2.1.1 SHOULD PKCE for confidential clients: gap present. client_secret authenticates at token endpoint (partial mitigation). PKCE would add independent code binding. SHOULD not MUST: CONCERN.

Node 9 — Formatter: CONCERN. Confidential client authorization code flow without PKCE. RFC 9700 §2.1.1 SHOULD recommends PKCE for confidential clients as well. client_secret provides meaningful but partial protection against code interception (attacker needs both code + secret). PKCE adds additional code binding independent of credential theft. SHOULD-level: CONCERN not REPROCESS. Remediation: add PKCE S256 to all confidential client flows; configure AS to require for both public and confidential clients.

### Reason
9-node trace correctly identifies RFC 9700 §2.1.1 SHOULD (not MUST) for confidential clients, evaluates client_secret as partial mitigation, and produces CONCERN. MUST applies only to public clients.

### Phrasing-B
CONCERN — 9-node trace: confidential client without PKCE, RFC 9700 §2.1.1 SHOULD, client_secret partially mitigates.

N1: T-rfc-oauth2 / PKCE for confidential clients; SHOULD level determination.
N2: RFC 9700 §2.1.1 MUST for public clients; SHOULD for confidential; client_secret partial mitigation.
N3: Verify normative level for confidential clients; evaluate client_secret mitigation.
N4: Confidential client with secret: code interception requires both code + secret. PKCE would add code binding. §2.1.1 SHOULD not MUST.
N5: SHOULD gap; partial mitigation present. CONCERN.
N6: CONCERN — SHOULD-level gap.
N7: RFC arbiter T-rfc-oauth2; SHOULD-level; no PO.1.
N8: CONCERN — RFC 9700 §2.1.1 SHOULD; client_secret partial mitigation.
N9: CONCERN. Add PKCE S256 to confidential client flows; layered protection.

---

## Pair[CB4-3.4-922]

### A
Full pipeline trace — confidential client uses authorization code with PKCE S256 and client_secret:

A confidential server-side web application uses authorization code flow with PKCE S256. At the token endpoint, the client authenticates with client_secret_post AND presents the code_verifier. Both controls are layered.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oauth2 / T-asvs-authentication. Sub-domain: confidential client authorization code flow with PKCE and client_secret layered. Pipeline route: Node 8 RFC arbiter with positive compliance confirmation.

Node 2 — Domain Translator: RFC 9700 §2.1.1 SHOULD PKCE for confidential clients: satisfied. RFC 7636 §4.1 PKCE S256 requirements: satisfied. RFC 6749 §2.3: client authentication at token endpoint: satisfied. Layered protection: code_verifier proves request binding; client_secret proves client identity.

Node 3 — Scheme Planner: Verify PKCE S256 included. Verify client_secret authentication at token endpoint. Verify both controls present. Expected: PASS.

Node 4 — Domain Processor: PKCE S256: code_challenge sent in authorization request; code_verifier sent in token request. Code interception: attacker must possess both code AND code_verifier. client_secret: client authenticates at token endpoint — code interception requires both code AND secret in addition to verifier. Three independent factors required for a successful attack: code (intercepted), verifier (held by legitimate client only), secret (held by legitimate client only). RFC 9700 §2.1.1 SHOULD satisfied above-SHOULD level. All controls confirmed.

Node 5 — Applier: RFC 9700 §2.1.1 SHOULD satisfied. RFC 7636 §4.1 satisfied. Client authentication satisfied. PASS.

Node 6 — Result Translator: PASS. Confidential client with PKCE S256 + client_secret. Layered defense. §2.1.1 SHOULD above SHOULD floor.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-oauth2). Positive compliance.

Node 8 — Arbiter Units: PASS. RFC 9700 §2.1.1 SHOULD satisfied. PKCE S256 + client_secret layered. Code interception requires three factors. No findings.

Node 9 — Formatter: PASS. Confidential client with authorization code + PKCE S256 + client_secret. RFC 9700 §2.1.1 SHOULD implemented above SHOULD floor. Layered: PKCE S256 binds code to requesting client; client_secret authenticates client identity at token endpoint. Maximum defense for confidential client authorization code flow.

### Reason
9-node trace confirms RFC 9700 §2.1.1 SHOULD above-SHOULD level with PKCE S256 + client_secret layered. PASS.

### Phrasing-B
PASS — 9-node trace: confidential client authorization code + PKCE S256 + client_secret. RFC 9700 §2.1.1 SHOULD above-floor; layered protection.

N1: T-rfc-oauth2 / confidential client full compliance.
N2: RFC 9700 §2.1.1 SHOULD; RFC 7636 §4.1; RFC 6749 §2.3 client auth.
N3: Verify PKCE S256 + client_secret layered.
N4: PKCE + secret = three-factor attack requirement (code + verifier + secret); all confirmed.
N5: All requirements met. PASS.
N6: PASS — PKCE + client_secret layered; maximum defense.
N7: RFC arbiter; positive.
N8: PASS — §2.1.1 SHOULD above floor; no findings.
N9: PASS. PKCE S256 + client_secret; layered protection.
