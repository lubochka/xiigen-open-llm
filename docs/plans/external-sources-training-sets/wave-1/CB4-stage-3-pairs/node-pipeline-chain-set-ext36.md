# CB4 Stage 3 — Node Pipeline Chain Set ext36
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-1097 through CB4-3.4-1104 (8 traces × 2 phrasings = 16 instances)
# Covers: full 9-node pipeline traces for all 8 ext42 arbiter patterns
# Running total after this file: 120 + (1104 − 60) × 2 = 2208 instances (1104 base pairs)
# Sources: RFC 9449 §6, OIDC Core §2, ASVS V9.2, RFC 7009 §2.2, RFC 6749 §4.1.3, RFC 7517 §4.2, RFC 7662 §2.2, ASVS V6.1/CWE-204

---

## Trace[CB4-3.4-1097]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** DPoP proof presented at token endpoint but issued AT contains no cnf/jkt claim
**Expected verdict:** REPROCESS

**A:**
A client presents a valid DPoP proof at the token endpoint. The AS validates it and issues an access token — but the AT does not contain a `cnf/jkt` claim. The RS accepts it as a plain bearer token. AS rationale: "DPoP at the token endpoint authenticates the client; the AT doesn't need to carry binding forward."

**B:**
Node 1 · Domain Splitter: Input classified as DPoP access token issuance at authorization server. Territory: T-rfc-jwt (DPoP sender-constrained token), T-asvs-token. Sub-domain: cnf/jkt embedding at token issuance. Routed to DPoP issuance binding branch.

Node 2 · Source Classifier: Primary source: RFC 9449 §6 (AS must bind AT to DPoP public key via cnf/jkt when DPoP proof presented). This is a MUST requirement at the AS issuance stage.

Node 3 · Claim Extractor: Extracted — (1) DPoP proof presented at token endpoint: confirmed; (2) DPoP proof valid (sig, htm, htu, iat, jti): confirmed; (3) cnf/jkt claim in issued AT: NOT present; (4) RS treats AT as plain bearer: confirmed; (5) Rationale: "DPoP at issuance = binding; no cnf in AT needed."

Node 4 · Standard Matcher: RFC 9449 §6 MUST: "the access token issued MUST be bound to the public key in the DPoP proof, by including a `cnf` claim in the access token that has a `jkt` claim." Matched claim: DPoP proof presented; cnf/jkt absent from issued AT. Applicability: direct — MUST violated.

Node 5 · Violation Detector: Violation confirmed. RFC 9449 §6 MUST: when DPoP proof presented at token endpoint, AT must contain cnf/jkt. Absent cnf/jkt: RS has no information to enforce DPoP binding — token has plain bearer semantics. Stolen AT usable by any party without the corresponding private key. DPoP sender-constraint nullified.

Node 6 · Severity Ranker: RFC 9449 §6 MUST (unconditional). "DPoP at token endpoint authenticates issuance" argument rejected: issuance authentication ≠ RS-side binding enforcement; cnf/jkt is the mechanism. Severity: REPROCESS.

Node 7 · Evidence Assembler: Evidence — (1) RFC 9449 §6 MUST: cnf/jkt required when DPoP proof presented; (2) DPoP proof validated: yes; (3) cnf/jkt in AT: absent; (4) RS behavior: plain bearer; (5) Attack: stolen AT usable without private key; DPoP protection nullified.

Node 8 · Arbiter: REPROCESS. RFC 9449 §6 MUST violated: DPoP proof presented at token endpoint but issued AT does not contain cnf/jkt. Token degrades to plain bearer — DPoP sender-constraint is end-to-end; cnf/jkt in AT enables RS-side enforcement. REPROCESS.

Node 9 · Formatter: Verdict: REPROCESS. Primary citation: RFC 9449 §6 MUST. Violation: cnf/jkt absent from AT. Remedy: extract DPoP public key from proof JOSE header; compute JWK thumbprint (RFC 7638); embed cnf={"jkt":"<thumbprint>"} in issued JWT AT.

**Reason:** RFC 9449 §6 MUST: AT must contain cnf/jkt when DPoP proof presented. Absent cnf/jkt = plain bearer. Pipeline detects at Node 5; Node 8 issues REPROCESS.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-jwt, T-asvs-token. DPoP AT issuance binding. cnf/jkt embedding branch.
Node 2 · Source Classifier: RFC 9449 §6 MUST primary. cnf/jkt required when DPoP proof presented at token endpoint.
Node 3 · Claim Extractor: DPoP proof valid: yes. cnf/jkt in AT: absent. RS: plain bearer. Rationale: "authentication at issuance = binding."
Node 4 · Standard Matcher: RFC 9449 §6 MUST: cnf/jkt required. Matched: absent.
Node 5 · Violation Detector: MUST violated. No cnf/jkt. DPoP sender-constraint nullified. Plain bearer semantics.
Node 6 · Severity Ranker: RFC 9449 §6 MUST. Issuance auth ≠ RS-side binding. REPROCESS.
Node 7 · Evidence Assembler: RFC 9449 §6 MUST; cnf/jkt absent; stolen AT usable without key; DPoP nullified.
Node 8 · Arbiter: REPROCESS. RFC 9449 §6 MUST: cnf/jkt must be in AT when DPoP proof presented. Absent. REPROCESS.
Node 9 · Formatter: REPROCESS. RFC 9449 §6 MUST. Add cnf/jkt: JWK thumbprint of DPoP public key in issued AT.

---

## Trace[CB4-3.4-1098]

**Territory:** T-oauth-oidc · T-rfc-jwt
**Domain:** OIDC id_token iss claim uses HTTP URL — violates https scheme requirement
**Expected verdict:** REPROCESS

**A:**
An OIDC AS issues id_tokens with `iss=http://auth.example.com`. The server uses HTTPS for connections. Rationale: "Both URLs resolve to the same server. The iss value is just a string identifier — HTTP vs HTTPS in the string doesn't affect security."

**B:**
Node 1 · Domain Splitter: Input classified as OIDC id_token claim compliance. Territory: T-oauth-oidc (OIDC id_token claims), T-rfc-jwt. Sub-domain: iss claim scheme requirement. Routed to OIDC id_token iss claim validation branch.

Node 2 · Source Classifier: Primary source: OIDC Core §2 (iss claim definition — MUST use https scheme). Secondary source: OIDC Discovery §3 (issuer MUST be HTTPS URL). Both impose MUST or normative HTTPS requirements for OIDC issuer identifiers.

Node 3 · Claim Extractor: Extracted — (1) id_token iss value: http://auth.example.com (HTTP); (2) Server actually serves HTTPS: confirmed; (3) Discovery document URL: https://auth.example.com; (4) Rationale: "resolves to same server; iss is just a string identifier."

Node 4 · Standard Matcher: OIDC Core §2 MUST: "iss value is a case sensitive URL using the https scheme that contains scheme, host, and optionally, port number and path components." Matched claim: iss uses http scheme. Applicability: direct — https scheme required; http scheme violates MUST.

Node 5 · Violation Detector: Violation confirmed. OIDC Core §2 MUST: iss must use https scheme. Current iss uses http. Downstream consequences: (1) RP configured with https://auth.example.com as expected issuer → id_token iss validation fails (case-sensitive comparison); (2) HTTP discovery document is susceptible to on-path MITM substitution.

Node 6 · Severity Ranker: OIDC Core §2 MUST (unconditional). "Both resolve to same server" argument rejected — iss is a normative identifier bound to HTTPS; "just a string" argument rejected — OIDC Core §2 defines its semantics. RP issuer validation breaks. Severity: REPROCESS.

Node 7 · Evidence Assembler: Evidence — (1) OIDC Core §2 MUST: iss must use https scheme; (2) iss=http://...: confirmed; (3) RP comparison against https://... fails; (4) HTTP discovery susceptible to MITM; (5) "Just a string" rejected — OIDC Core §2 defines it normatively.

Node 8 · Arbiter: REPROCESS. OIDC Core §2 MUST violated: iss claim uses http scheme instead of required https. RP issuer validation fails; discovery integrity not protected. "Both URLs resolve to same server" rejected. REPROCESS.

Node 9 · Formatter: Verdict: REPROCESS. Primary citation: OIDC Core §2 MUST. Violation: iss=http://auth.example.com. Remedy: set iss=https://auth.example.com in all issued id_tokens; update AS configuration to use HTTPS issuer identifier.

**Reason:** OIDC Core §2 MUST: iss must use https scheme. HTTP iss breaks RP validation and discovery integrity. Pipeline detects at Node 5; Node 8 issues REPROCESS.

**Phrasing-B:**
Node 1 · Domain Splitter: T-oauth-oidc, T-rfc-jwt. OIDC id_token iss claim scheme. MUST-https branch.
Node 2 · Source Classifier: OIDC Core §2 MUST primary. iss must be HTTPS URL.
Node 3 · Claim Extractor: iss=http://...: confirmed. Server uses HTTPS: yes. Rationale: "just a string; both resolve."
Node 4 · Standard Matcher: OIDC Core §2 MUST: iss uses https scheme. Matched: iss uses http.
Node 5 · Violation Detector: MUST violated. iss=http. RP validation fails; discovery MITM-susceptible.
Node 6 · Severity Ranker: OIDC Core §2 MUST. "Same server" and "just a string" rejected. REPROCESS.
Node 7 · Evidence Assembler: OIDC Core §2 MUST; iss=http; RP comparison fails; http discovery unprotected.
Node 8 · Arbiter: REPROCESS. OIDC Core §2 MUST: iss must use https scheme. Uses http. REPROCESS.
Node 9 · Formatter: REPROCESS. OIDC Core §2 MUST. Fix: iss=https://auth.example.com in all id_tokens.

---

## Trace[CB4-3.4-1099]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** JWKS rollover window 30 minutes < 1-hour max AT TTL — valid tokens rejected after rotation
**Expected verdict:** CONCERN

**A:**
AS rotates JWKS signing keys quarterly. Old key removed from JWKS after 30-minute overlap. Max AT TTL: 1 hour. A token issued 50 minutes before rotation, signed with old key, is presented 10 minutes post-rotation — the old key has been removed; RS rejects the still-valid token.

**B:**
Node 1 · Domain Splitter: Input classified as JWKS key rotation operational configuration. Territory: T-rfc-jwt (JWKS key lifecycle), T-asvs-token. Sub-domain: key overlap duration vs token TTL. Routed to key rotation adequacy branch.

Node 2 · Source Classifier: Primary source: ASVS V9.2 guidance ("signing keys are rotated with sufficient overlap to prevent disruption to active tokens"). No RFC specifies a minimum numeric JWKS overlap window — this is guidance-level only.

Node 3 · Claim Extractor: Extracted — (1) JWKS rotation frequency: quarterly; (2) Old key overlap window: 30 minutes; (3) Maximum AT TTL: 1 hour; (4) Scenario: token issued 50 min before rotation, presented 10 min post-rotation; (5) Old key available at presentation: NO (removed after 30 min).

Node 4 · Standard Matcher: ASVS V9.2 guidance: overlap window should be ≥ max token TTL. RFC 7517: no numeric MUST for key retention duration. 30-minute window < 1-hour max TTL: gap confirmed. Applicability: guidance-level; no RFC MUST.

Node 5 · Violation Detector: Operational gap identified. ASVS V9.2 guidance not met: 30-min overlap window < 1-hour max AT TTL. Legitimate valid tokens rejected post-rotation. No security failure (retaining old key longer doesn't weaken security). Availability/correctness concern only. No RFC MUST violated.

Node 6 · Severity Ranker: No RFC MUST with numeric bound. ASVS V9.2 guidance not met (availability gap). Security implication: none — retaining old key longer is safe. Operational implication: valid token rejections. Severity: CONCERN.

Node 7 · Evidence Assembler: Evidence — (1) ASVS V9.2 guidance: sufficient overlap required; (2) Overlap window (30 min) < max TTL (1 hour): confirmed; (3) Token rejection scenario: confirmed; (4) No RFC MUST for specific overlap duration; (5) Guidance gap = CONCERN.

Node 8 · Arbiter: CONCERN. ASVS V9.2 guidance gap: JWKS overlap window (30 min) is shorter than maximum AT TTL (1 hour), causing valid token rejections post-rotation. No RFC MUST for specific overlap duration. Operational availability concern, not security failure. CONCERN. Overlap window should be ≥ max AT TTL.

Node 9 · Formatter: Verdict: CONCERN. Primary citation: ASVS V9.2 guidance. Gap: overlap window (30 min) < max TTL (1 hour). Recommendation: extend key overlap window to ≥ max AT TTL; review overlap whenever TTL changes.

**Reason:** ASVS V9.2 guidance: overlap window should cover active token lifetime. 30-min window < 1-hour TTL = operational gap. No RFC MUST → CONCERN.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-jwt, T-asvs-token. JWKS rollover configuration. Overlap window adequacy branch.
Node 2 · Source Classifier: ASVS V9.2 guidance primary. No RFC MUST for numeric overlap window.
Node 3 · Claim Extractor: Overlap window: 30 min. Max AT TTL: 1 hour. Valid token rejected post-rotation: yes.
Node 4 · Standard Matcher: ASVS V9.2: sufficient overlap. 30 min < 1 hour = guidance gap.
Node 5 · Violation Detector: ASVS V9.2 guidance not met. No security failure. Valid token rejections confirmed.
Node 6 · Severity Ranker: ASVS V9.2 guidance. No RFC MUST. CONCERN.
Node 7 · Evidence Assembler: ASVS V9.2 guidance gap; 30-min < 1-hour; valid token rejection confirmed; no RFC MUST.
Node 8 · Arbiter: CONCERN. ASVS V9.2: overlap window insufficient. No RFC MUST for specific duration. CONCERN.
Node 9 · Formatter: CONCERN. ASVS V9.2 guidance. Extend overlap to ≥ max AT TTL.

---

## Trace[CB4-3.4-1100]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Token revocation endpoint returns 400 for unrecognized or expired tokens
**Expected verdict:** REPROCESS

**A:**
A token revocation endpoint returns `HTTP 400 Bad Request` with `error=invalid_token` when the presented token is not recognized, already expired, or already revoked. Rationale: "Returning 200 for invalid tokens is misleading. 400 tells the client something was wrong."

**B:**
Node 1 · Domain Splitter: Input classified as OAuth 2.0 token revocation endpoint response behavior. Territory: T-rfc-oauth2 (RFC 7009 revocation), T-asvs-token. Sub-domain: HTTP response code for unrecognized tokens. Routed to token revocation response branch.

Node 2 · Source Classifier: Primary source: RFC 7009 §2.2 (revocation endpoint response codes). RFC 7009 §2.2 specifies MUST return 200 for unrecognized tokens — intentional design to prevent token validity enumeration.

Node 3 · Claim Extractor: Extracted — (1) Revocation endpoint: confirmed; (2) Response for unrecognized token: 400 with error=invalid_token; (3) Response for recognized token: 200; (4) Rationale: "200 is misleading for invalid tokens."

Node 4 · Standard Matcher: RFC 7009 §2.2 MUST: "The authorization server responds with HTTP status code 200 if the token has been revoked successfully or if the client submitted an invalid token." Matched claim: AS returns 400 for unrecognized tokens instead of 200. Applicability: direct — MUST violated.

Node 5 · Violation Detector: Violation confirmed. RFC 7009 §2.2 MUST: 200 required for unrecognized tokens. AS returns 400. Consequence: clients can probe token validity by calling the revocation endpoint — different response for "found and revoked" (200) vs "not found" (400) enables token validity enumeration.

Node 6 · Severity Ranker: RFC 7009 §2.2 MUST (unconditional). RFC 7009's 200-for-invalid design is intentional enumeration prevention; "200 is misleading" rejects the security rationale. Severity: REPROCESS.

Node 7 · Evidence Assembler: Evidence — (1) RFC 7009 §2.2 MUST: 200 for invalid/unrecognized tokens; (2) AS returns 400 for unrecognized tokens: confirmed; (3) Attack: token validity enumeration via revocation endpoint (200=valid, 400=invalid); (4) "200 is misleading" rejected — RFC 7009 defines this behavior intentionally.

Node 8 · Arbiter: REPROCESS. RFC 7009 §2.2 MUST violated: revocation endpoint returns 400 for unrecognized tokens; must return 200 per RFC 7009. 400 enables token validity enumeration. "200 is misleading" rejected. REPROCESS.

Node 9 · Formatter: Verdict: REPROCESS. Primary citation: RFC 7009 §2.2 MUST. Violation: 400 for unrecognized tokens. Remedy: return 200 for all revocation outcomes (successfully revoked, unrecognized, expired, already-revoked) when client authentication succeeds.

**Reason:** RFC 7009 §2.2 MUST: 200 for all token revocation outcomes including unrecognized tokens. 400 enables token validity enumeration. Pipeline detects at Node 5; Node 8 issues REPROCESS.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-oauth2, T-asvs-token. Token revocation response code. RFC 7009 §2.2 branch.
Node 2 · Source Classifier: RFC 7009 §2.2 MUST primary. 200 for invalid tokens is intentional.
Node 3 · Claim Extractor: Unrecognized token response: 400. Recognized token response: 200. Rationale: "200 misleading."
Node 4 · Standard Matcher: RFC 7009 §2.2 MUST: 200 for invalid tokens. Matched: 400 returned.
Node 5 · Violation Detector: MUST violated. 400 enables token validity enumeration.
Node 6 · Severity Ranker: RFC 7009 §2.2 MUST. "Misleading" argument rejects RFC's intentional design. REPROCESS.
Node 7 · Evidence Assembler: RFC 7009 §2.2 MUST; 400 for invalid; enumeration via 200/400 discrimination; "misleading" rejected.
Node 8 · Arbiter: REPROCESS. RFC 7009 §2.2 MUST: 200 for unrecognized tokens required. 400 enables enumeration. REPROCESS.
Node 9 · Formatter: REPROCESS. RFC 7009 §2.2 MUST. Return 200 for all revocation outcomes.

---

## Trace[CB4-3.4-1101]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** redirect_uri not stored at authorization request — any registered redirect_uri accepted at token exchange
**Expected verdict:** REPROCESS

**A:**
AS validates redirect_uri at authorization request time but does not store the specific URI with the authorization code. At token exchange, any of the client's registered redirect_uris is accepted. Rationale: "Validated at authorization time; both are registered URIs — second check seems redundant."

**B:**
Node 1 · Domain Splitter: Input classified as OAuth 2.0 authorization code redirect_uri binding at token exchange. Territory: T-rfc-oauth2 (code exchange parameters), T-asvs-token. Sub-domain: redirect_uri verification at token endpoint. Routed to redirect_uri binding branch.

Node 2 · Source Classifier: Primary source: RFC 6749 §4.1.3 (token endpoint requirements — redirect_uri must match original authorization request value). This is a MUST/REQUIRED requirement.

Node 3 · Claim Extractor: Extracted — (1) redirect_uri validated at authorization request: confirmed; (2) redirect_uri stored with authorization code: NOT done; (3) Token exchange: any registered redirect_uri accepted; (4) Specific redirect_uri from original request not verified at exchange: confirmed; (5) Rationale: "validated once; both registered."

Node 4 · Standard Matcher: RFC 6749 §4.1.3 MUST: "redirect_uri: REQUIRED, if the redirect_uri parameter was included in the authorization request as described in Section 4.1.1, and their values MUST be identical." Matched claim: redirect_uri not stored; values not compared at token exchange. Applicability: direct — MUST requirement unmet.

Node 5 · Violation Detector: Violation confirmed. RFC 6749 §4.1.3 MUST: redirect_uri at token exchange must match the specific value from the authorization request. Without storing the original redirect_uri: any client holding a stolen authorization code can exchange it with a different registered redirect_uri — code injection via redirect_uri substitution.

Node 6 · Severity Ranker: RFC 6749 §4.1.3 MUST (unconditional). "Registered membership substitutes for exact match" rejected: the MUST requires the specific value, not membership in the registration set. Severity: REPROCESS.

Node 7 · Evidence Assembler: Evidence — (1) RFC 6749 §4.1.3 MUST: redirect_uri values must be identical; (2) Specific redirect_uri not stored with code: confirmed; (3) Any registered URI accepted: confirmed; (4) Attack: stolen code exchangeable with different registered redirect_uri; (5) "Both registered" rejected — specific value required.

Node 8 · Arbiter: REPROCESS. RFC 6749 §4.1.3 MUST violated: redirect_uri from original authorization request not stored; any registered redirect_uri accepted at token exchange. Code injection via redirect_uri substitution is viable. "Both registered" does not satisfy the specific-value MUST. REPROCESS.

Node 9 · Formatter: Verdict: REPROCESS. Primary citation: RFC 6749 §4.1.3 MUST. Violation: redirect_uri not stored with code; exact match not enforced. Remedy: store redirect_uri from authorization request with authorization code; compare exact value at token exchange; reject with invalid_grant on mismatch.

**Reason:** RFC 6749 §4.1.3 MUST: redirect_uri at token exchange must match authorization request redirect_uri. Absent storage/comparison enables code injection via redirect_uri substitution. Pipeline detects at Node 5; Node 8 issues REPROCESS.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-oauth2, T-asvs-token. redirect_uri binding at token exchange. RFC 6749 §4.1.3 branch.
Node 2 · Source Classifier: RFC 6749 §4.1.3 MUST primary. Exact match of redirect_uri required.
Node 3 · Claim Extractor: redirect_uri stored with code: no. Any registered URI accepted: yes. Rationale: "validated at auth time; both registered."
Node 4 · Standard Matcher: RFC 6749 §4.1.3 MUST: redirect_uri values must be identical. Matched: not stored; not compared.
Node 5 · Violation Detector: MUST violated. Specific redirect_uri not verified. Code injection via substitution viable.
Node 6 · Severity Ranker: RFC 6749 §4.1.3 MUST. "Registered membership" rejected. REPROCESS.
Node 7 · Evidence Assembler: RFC 6749 §4.1.3 MUST; redirect_uri not stored; any registered accepted; code injection viable.
Node 8 · Arbiter: REPROCESS. RFC 6749 §4.1.3 MUST: redirect_uri must match original request value. Not stored; not verified. REPROCESS.
Node 9 · Formatter: REPROCESS. RFC 6749 §4.1.3 MUST. Store redirect_uri with code; verify exact match at token exchange.

---

## Trace[CB4-3.4-1102]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** JWKS keys published without `use` parameter — key purpose not indicated
**Expected verdict:** CONCERN

**A:**
AS publishes JWKS with RSA signing key and RSA encryption key, neither with `use` parameter. RS tries all keys for signature verification. AS rationale: "RS tries each key in order — the right key will succeed."

**B:**
Node 1 · Domain Splitter: Input classified as JWKS key metadata completeness. Territory: T-rfc-jwt (JWKS structure), T-asvs-token. Sub-domain: JWKS use parameter presence. Routed to JWKS metadata completeness branch.

Node 2 · Source Classifier: Primary source: RFC 7517 §4.2 ("use" parameter — SHOULD be present; not MUST). No MUST for use parameter presence — guidance-level only.

Node 3 · Claim Extractor: Extracted — (1) JWKS published: confirmed; (2) Signing key present: confirmed; (3) Encryption key present: confirmed; (4) use parameter on any key: NOT present; (5) RS behavior: tries all keys for signature verification; (6) Rationale: "RS tries keys in order; correct one succeeds."

Node 4 · Standard Matcher: RFC 7517 §4.2 SHOULD: "use parameter SHOULD be present." No MUST for use presence. Matched claim: use parameter absent from all JWKS keys. Applicability: guidance-level.

Node 5 · Violation Detector: Guidance gap identified. RFC 7517 §4.2 SHOULD not met: use parameter absent. RS cannot determine signing key without trial-and-error. No security failure per se — the correct key will eventually succeed. Operational overhead and key-type confusion risk. No RFC MUST violated.

Node 6 · Severity Ranker: RFC 7517 §4.2 SHOULD (guidance). No security attack enabled by absent use. Operational clarity gap. Severity: CONCERN.

Node 7 · Evidence Assembler: Evidence — (1) RFC 7517 §4.2 SHOULD: use should be present; (2) use absent from all keys: confirmed; (3) RS trial-and-error: confirmed; (4) No security failure: signing succeeds; (5) No RFC MUST → CONCERN.

Node 8 · Arbiter: CONCERN. RFC 7517 §4.2 SHOULD not met: use parameter absent from JWKS keys. RS cannot distinguish signing from encryption keys without trial-and-error. No RFC MUST for use parameter; no security attack enabled. Guidance gap = CONCERN.

Node 9 · Formatter: Verdict: CONCERN. Primary citation: RFC 7517 §4.2 SHOULD. Gap: use parameter absent. Recommendation: add use=sig to signing keys; use=enc to encryption keys.

**Reason:** RFC 7517 §4.2 SHOULD: use should indicate key purpose. Absent use causes key-type confusion and RS overhead. No MUST → CONCERN.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-jwt, T-asvs-token. JWKS key metadata. use parameter branch.
Node 2 · Source Classifier: RFC 7517 §4.2 SHOULD primary. Not MUST.
Node 3 · Claim Extractor: Signing key: yes. Encryption key: yes. use parameter: absent. RS: trial-and-error.
Node 4 · Standard Matcher: RFC 7517 §4.2 SHOULD: use present. Matched: absent. Guidance only.
Node 5 · Violation Detector: SHOULD not met. No security failure. Operational clarity gap.
Node 6 · Severity Ranker: RFC 7517 §4.2 SHOULD. No MUST. CONCERN.
Node 7 · Evidence Assembler: RFC 7517 §4.2 SHOULD gap; use absent; RS overhead; no security attack.
Node 8 · Arbiter: CONCERN. RFC 7517 §4.2 SHOULD: use absent from JWKS keys. Guidance gap. CONCERN.
Node 9 · Formatter: CONCERN. RFC 7517 §4.2 SHOULD. Add use=sig (signing keys); use=enc (encryption keys).

---

## Trace[CB4-3.4-1103]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Introspection response includes user PII beyond token scope
**Expected verdict:** CONCERN

**A:**
Token introspection for `scope=read:orders` returns email, phone, address, birthdate in the response. Rationale: "RS needs user identity for authorization decisions. Including user attributes in introspection is convenient."

**B:**
Node 1 · Domain Splitter: Input classified as OAuth 2.0 token introspection response content. Territory: T-rfc-oauth2 (RFC 7662 introspection), T-asvs-token. Sub-domain: introspection response scope filtering. Routed to introspection response content branch.

Node 2 · Source Classifier: Primary source: RFC 7662 §2.2 (introspection response — SHOULD NOT include resource owner info beyond token scope). Secondary source: ASVS V8.2.2 (PII in logs/responses). SHOULD NOT is guidance-level.

Node 3 · Claim Extractor: Extracted — (1) Token scope: read:orders; (2) Introspection response includes: email, phone, address, birthdate; (3) PII beyond scope: confirmed; (4) Rationale: "RS needs user attributes; convenient."

Node 4 · Standard Matcher: RFC 7662 §2.2 SHOULD NOT: "Authorization servers SHOULD NOT include any additional information about the resource owner beyond what was granted in the access token." ASVS V8.2.2: sensitive data not in logs/responses. Matched: PII included beyond scope. Applicability: guidance-level; no MUST NOT.

Node 5 · Violation Detector: Guidance gap identified. RFC 7662 §2.2 SHOULD NOT not met: PII included beyond read:orders scope authorization. ASVS V8.2.2 concern: introspection responses may be logged by RS. No RFC MUST NOT → guidance gap only.

Node 6 · Severity Ranker: RFC 7662 §2.2 SHOULD NOT (guidance). ASVS V8.2.2 logging concern. No MUST NOT → CONCERN.

Node 7 · Evidence Assembler: Evidence — (1) RFC 7662 §2.2 SHOULD NOT: PII beyond scope; (2) email/phone/address/birthdate in response: confirmed; (3) Scope=read:orders: no PII authorization; (4) ASVS V8.2.2 logging concern; (5) No MUST NOT → CONCERN.

Node 8 · Arbiter: CONCERN. RFC 7662 §2.2 SHOULD NOT not met: PII included in introspection response beyond token scope. ASVS V8.2.2 PII logging concern in RS-facing responses. No RFC MUST NOT → CONCERN.

Node 9 · Formatter: Verdict: CONCERN. Primary citations: RFC 7662 §2.2 SHOULD NOT + ASVS V8.2.2. Gap: PII beyond scope in introspection response. Recommendation: scope-filter introspection response to claims authorized by presented token scope.

**Reason:** RFC 7662 §2.2 SHOULD NOT: introspection should not include resource owner info beyond token scope. ASVS V8.2.2 PII logging concern. No MUST NOT → CONCERN.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-oauth2, T-asvs-token. Introspection response content. Scope filtering branch.
Node 2 · Source Classifier: RFC 7662 §2.2 SHOULD NOT primary. ASVS V8.2.2 secondary. Guidance-level.
Node 3 · Claim Extractor: Token scope: read:orders. PII in response: email/phone/address/birthdate. Beyond scope: yes.
Node 4 · Standard Matcher: RFC 7662 §2.2 SHOULD NOT: no extra resource owner info. Guidance. Matched: PII present.
Node 5 · Violation Detector: SHOULD NOT not met. PII beyond scope. ASVS V8.2.2 logging risk. No MUST NOT.
Node 6 · Severity Ranker: RFC 7662 §2.2 SHOULD NOT. No MUST NOT. CONCERN.
Node 7 · Evidence Assembler: RFC 7662 §2.2 SHOULD NOT gap; PII in response; ASVS V8.2.2 concern; no MUST NOT.
Node 8 · Arbiter: CONCERN. RFC 7662 §2.2 SHOULD NOT + ASVS V8.2.2. PII beyond scope. Guidance gap. CONCERN.
Node 9 · Formatter: CONCERN. RFC 7662 §2.2 SHOULD NOT + ASVS V8.2.2. Scope-filter introspection response.

---

## Trace[CB4-3.4-1104]

**Territory:** T-oauth-oidc · T-cwe-authn
**Domain:** User enumeration via login_hint — different responses for registered vs unregistered users
**Expected verdict:** CONCERN

**A:**
OIDC AS returns `error_description="User not found in directory"` for unregistered login_hint values; normal authentication challenge for registered ones. Unauthenticated parties can enumerate registered email addresses.

**B:**
Node 1 · Domain Splitter: Input classified as OIDC authorization endpoint login_hint handling and user enumeration risk. Territory: T-oauth-oidc (OIDC login_hint parameter), T-cwe-authn. Sub-domain: user enumeration via observable response discrepancy. Routed to enumeration prevention branch.

Node 2 · Source Classifier: Primary source: CWE-204 (Observable Response Discrepancy) — distinct responses enable enumeration. Secondary source: ASVS V6.1.3 guidance ("verify that authentication functions display generic messages not revealing account existence"). No RFC MUST NOT for login_hint response differentiation.

Node 3 · Claim Extractor: Extracted — (1) login_hint with registered user: normal auth challenge; (2) login_hint with unregistered user: "User not found in directory" error; (3) Behavioral difference: observable; (4) Unauthenticated enumeration: confirmed possible; (5) RFC MUST NOT for this behavior: none identified.

Node 4 · Standard Matcher: CWE-204: observable response discrepancy enables enumeration. ASVS V6.1.3 guidance: generic messages; no account existence disclosure. No RFC MUST NOT for login_hint response differentiation. Applicability: guidance-level; CWE advisory.

Node 5 · Violation Detector: CWE-204 pattern identified: distinct responses enable registered-user enumeration via login_hint. ASVS V6.1.3 guidance not met. No RFC MUST NOT for this specific behavior. Guidance gap only.

Node 6 · Severity Ranker: CWE-204 (advisory). ASVS V6.1.3 guidance level. No RFC MUST NOT for login_hint response behavior. Severity: CONCERN.

Node 7 · Evidence Assembler: Evidence — (1) CWE-204: observable discrepancy; (2) Registered vs unregistered response difference: confirmed; (3) ASVS V6.1.3 guidance gap; (4) No RFC MUST NOT → CONCERN not REPROCESS.

Node 8 · Arbiter: CONCERN. CWE-204: distinct responses for registered vs unregistered login_hint enable email enumeration by unauthenticated parties. ASVS V6.1.3 guidance not met. No RFC MUST NOT for this behavior → CONCERN.

Node 9 · Formatter: Verdict: CONCERN. Primary citations: CWE-204 + ASVS V6.1.3 guidance. Gap: observable discrepancy enables user enumeration. Recommendation: return uniform login_required regardless of whether login_hint identifies registered user.

**Reason:** CWE-204 enumeration via login_hint observable response discrepancy. ASVS V6.1.3 guidance gap. No RFC MUST NOT → CONCERN.

**Phrasing-B:**
Node 1 · Domain Splitter: T-oauth-oidc, T-cwe-authn. login_hint enumeration. Observable discrepancy branch.
Node 2 · Source Classifier: CWE-204 primary. ASVS V6.1.3 guidance secondary. No RFC MUST NOT.
Node 3 · Claim Extractor: Registered user response: auth challenge. Unregistered: "not found." Observable difference: yes.
Node 4 · Standard Matcher: CWE-204: observable discrepancy. ASVS V6.1.3: generic messages. Guidance-level.
Node 5 · Violation Detector: CWE-204 pattern. ASVS V6.1.3 guidance not met. No RFC MUST NOT.
Node 6 · Severity Ranker: CWE-204 advisory. ASVS guidance. No MUST. CONCERN.
Node 7 · Evidence Assembler: CWE-204; discrepancy confirmed; ASVS V6.1.3 guidance gap; no RFC MUST NOT.
Node 8 · Arbiter: CONCERN. CWE-204 + ASVS V6.1.3 guidance gap. No RFC MUST NOT. CONCERN.
Node 9 · Formatter: CONCERN. CWE-204 + ASVS V6.1.3. Return uniform login_required regardless of user existence.
