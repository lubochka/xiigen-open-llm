# CB4 Stage 3 — Node Pipeline Chain Set ext35
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-1071 through CB4-3.4-1078 (8 traces × 2 phrasings = 16 instances)
# Covers: full 9-node pipeline traces for all 8 ext41 arbiter patterns
# Running total after this file: 120 + (1078 − 60) × 2 = 2156 instances (1078 base pairs)
# Sources: RFC 9449 §4.2/§7.2, RFC 9126 §5, OIDC Core §3.3.2.11, RFC 9449 §11.1, RFC 6749 §3.3, RFC 9325 §4, OIDC Core §3.3.2.11, RFC 6749 §5.1

---

## Trace[CB4-3.4-1071]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** DPoP proof htu/htm not validated at RS — endpoint and method binding absent
**Expected verdict:** REPROCESS

**A:**
A resource server validates DPoP proofs but does not check the `htu` (HTTP Target URI) or `htm` (HTTP Method) claims. A DPoP proof signed for `POST https://api.example.com/payments` is accepted at `DELETE https://api.example.com/accounts/123`. Rationale: "We validate key binding via jkt — uri/method are redundant since we check the token's audience."

**B:**
Node 1 · Domain Splitter: Input classified as DPoP (Demonstrating Proof of Possession) proof validation at resource server. Territory: T-rfc-jwt (DPoP proof claims), T-asvs-token. Sub-domain: DPoP proof binding claims validation. Routed to DPoP proof validation branch.

Node 2 · Source Classifier: Primary source: RFC 9449 §7.2 (RS DPoP proof verification requirements). Secondary source: RFC 9449 §4.2 (DPoP proof structure and claim requirements). Both htm and htu are MUST-level claims in DPoP proof validation.

Node 3 · Claim Extractor: Extracted — (1) DPoP proof signature validated: confirmed; (2) jkt binding against cnf/jkt: confirmed; (3) iat freshness checked: confirmed; (4) htu (HTTP Target URI) validated against current request URI: NOT performed; (5) htm (HTTP Method) validated against current request method: NOT performed; (6) Rationale: "aud and jkt cover binding; htu/htm are redundant."

Node 4 · Standard Matcher: RFC 9449 §7.2 MUST: "verify that... the htm claim matches the HTTP method of the current request; the htu claim matches the HTTP target URI." Matched claims: neither htu nor htm validated. Applicability: direct — two MUST requirements unmet simultaneously.

Node 5 · Violation Detector: Violation confirmed. RFC 9449 §7.2 MUST (×2): htu and htm must be validated against current request. Without htu/htm validation: a DPoP proof captured from a low-privilege read endpoint can be replayed at a high-privilege write/delete endpoint. aud validates the RS, not the specific endpoint or method within it. Endpoint-and-method binding is the unique purpose of htu/htm.

Node 6 · Severity Ranker: RFC 9449 §7.2 MUST (×2). "aud covers binding" rejected: aud is per-RS, not per-endpoint. Two unconditional MUSTs unmet. Captured proof replay across endpoints fully viable. Severity: REPROCESS.

Node 7 · Evidence Assembler: Evidence — (1) RFC 9449 §7.2 MUST ×2: htu match; htm match; (2) htu validation: absent; (3) htm validation: absent; (4) Attack surface: proof from GET /user/profile replayable at DELETE /admin/accounts; (5) "aud covers binding" rejected — aud is RS-level, not endpoint-level.

Node 8 · Arbiter: REPROCESS. RFC 9449 §7.2 MUST violated (×2): htu not verified against current request URI; htm not verified against current HTTP method. Captured DPoP proofs are replayable across endpoints and methods. Endpoint-level binding is nullified. REPROCESS.

Node 9 · Formatter: Verdict: REPROCESS. Primary citation: RFC 9449 §7.2 MUST (×2). Violation: htu and htm not validated. Remedy: validate htm (exact HTTP method match) and htu (URI without query/fragment) on every DPoP-protected request; reject with 401/invalid_dpop_proof on mismatch.

**Reason:** RFC 9449 §7.2 MUST ×2: htu and htm must be validated. Absent validation enables cross-endpoint proof replay. Pipeline detects at Node 5; Node 8 issues REPROCESS.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-jwt, T-asvs-token. DPoP proof validation at RS. htu/htm binding branch.
Node 2 · Source Classifier: RFC 9449 §7.2 MUST primary. htm and htu are RS validation requirements.
Node 3 · Claim Extractor: htu validated: no. htm validated: no. jkt validated: yes. Rationale: "aud and jkt sufficient."
Node 4 · Standard Matcher: RFC 9449 §7.2 MUST ×2: htm matches method; htu matches URI. Matched: both absent.
Node 5 · Violation Detector: MUST ×2 violated. No endpoint/method binding. Proof replay across endpoints viable.
Node 6 · Severity Ranker: RFC 9449 §7.2 MUST ×2. Endpoint-level binding nullified. REPROCESS.
Node 7 · Evidence Assembler: RFC 9449 §7.2 MUST ×2; htu absent; htm absent; proof replayable at any endpoint/method.
Node 8 · Arbiter: REPROCESS. RFC 9449 §7.2 MUST: htu and htm must be validated against current request. Neither checked. REPROCESS.
Node 9 · Formatter: REPROCESS. RFC 9449 §7.2 MUST ×2. Add: htm exact-match; htu URI-without-query match on every request.

---

## Trace[CB4-3.4-1072]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** PAR request_uri used at authorization endpoint — client_id binding not verified
**Expected verdict:** REPROCESS

**A:**
An Authorization Server accepts PAR (Pushed Authorization Requests, RFC 9126). At the authorization endpoint, it processes `request_uri` values without verifying that the `client_id` in the authorization request matches the `client_id` that originally pushed the request. Rationale: "request_uri is short-lived (30s) and opaque — guessing is infeasible. Client binding is an edge case."

**B:**
Node 1 · Domain Splitter: Input classified as PAR (Pushed Authorization Requests) authorization endpoint handling. Territory: T-rfc-oauth2 (PAR request_uri processing), T-asvs-token. Sub-domain: PAR request_uri client binding validation. Routed to PAR request_uri handling branch.

Node 2 · Source Classifier: Primary source: RFC 9126 §5 (PAR authorization endpoint requirements). Secondary source: RFC 9126 §4 (PAR endpoint response, request_uri lifetime). RFC 9126 §5 imposes MUST for client_id binding verification.

Node 3 · Claim Extractor: Extracted — (1) AS accepts request_uri at authorization endpoint: confirmed; (2) AS validates request_uri is not expired: confirmed (30s); (3) AS verifies client_id in auth request matches client_id that pushed request: NOT performed; (4) Rationale: "opaque + short-lived makes binding unnecessary."

Node 4 · Standard Matcher: RFC 9126 §5 MUST: "the authorization server MUST check that the client_id provided in the authorization request matches the client_id attribute of the authorization request object." Matched claim: client_id binding not verified. Applicability: direct — MUST requirement unmet.

Node 5 · Violation Detector: Violation confirmed. RFC 9126 §5 MUST: client_id at authorization endpoint must match PAR-associated client_id. Cross-client substitution attack viable: Client A pushes PAR request; Client B uses the request_uri at authorization endpoint with its own credentials → AS initiates Client B's flow using Client A's request parameters.

Node 6 · Severity Ranker: RFC 9126 §5 MUST (unconditional). "Opaque + short-lived" argument rejected: MUST applies regardless of opaqueness; binding is a normative requirement not a risk-based countermeasure. Severity: REPROCESS.

Node 7 · Evidence Assembler: Evidence — (1) RFC 9126 §5 MUST: client_id binding required; (2) Binding check: absent; (3) Attack: any party obtaining request_uri can use it with different client credentials; (4) "Opaque makes guessing infeasible" rejected — MUST is not conditional on attack feasibility.

Node 8 · Arbiter: REPROCESS. RFC 9126 §5 MUST violated: client_id at authorization endpoint not verified against PAR-associated client_id. Cross-client request_uri substitution viable. Opaqueness and short lifetime do not satisfy the normative binding requirement. REPROCESS.

Node 9 · Formatter: Verdict: REPROCESS. Primary citation: RFC 9126 §5 MUST. Violation: client_id binding not checked. Remedy: store client_id with pushed request; compare against client_id at authorization endpoint; reject with invalid_request on mismatch.

**Reason:** RFC 9126 §5 MUST: client_id binding must be verified at authorization endpoint for PAR request_uri. Absent check enables cross-client substitution. Pipeline detects at Node 5; Node 8 issues REPROCESS.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-oauth2, T-asvs-token. PAR request_uri processing at AS authorization endpoint. Client binding branch.
Node 2 · Source Classifier: RFC 9126 §5 MUST primary. Client_id binding at authorization endpoint.
Node 3 · Claim Extractor: request_uri accepted: yes. Expiry checked: yes. client_id binding check: no. Rationale: "opaque + short-lived."
Node 4 · Standard Matcher: RFC 9126 §5 MUST: client_id must match PAR-associated client_id. Matched: binding absent.
Node 5 · Violation Detector: MUST violated. No binding check. Cross-client substitution viable.
Node 6 · Severity Ranker: RFC 9126 §5 MUST. Opaqueness irrelevant to normative requirement. REPROCESS.
Node 7 · Evidence Assembler: RFC 9126 §5 MUST; binding absent; cross-client request_uri use undetected; "opaque" defense rejected.
Node 8 · Arbiter: REPROCESS. RFC 9126 §5 MUST: client_id binding at authorization endpoint not checked. Cross-client substitution enabled. REPROCESS.
Node 9 · Formatter: REPROCESS. RFC 9126 §5 MUST. Fix: store and verify client_id binding; reject mismatch with invalid_request.

---

## Trace[CB4-3.4-1073]

**Territory:** T-oauth-oidc · T-rfc-jwt
**Domain:** OIDC hybrid flow — id_token missing c_hash binding to authorization code
**Expected verdict:** REPROCESS

**A:**
An OIDC AS hybrid flow (`response_type=code id_token`) returns both authorization code and id_token in the fragment. The id_token includes standard claims but no `c_hash`. Rationale: "c_hash is an optimization; we bind the code to the session server-side — that's equivalent."

**B:**
Node 1 · Domain Splitter: Input classified as OIDC hybrid flow id_token claim validation. Territory: T-oauth-oidc (OIDC hybrid flow), T-rfc-jwt (id_token claims). Sub-domain: c_hash claim requirement in hybrid flow. Routed to OIDC hybrid flow id_token validation branch.

Node 2 · Source Classifier: Primary source: OIDC Core §3.3.2.11 (hybrid flow id_token requirements). Secondary source: OIDC Core §2 (id_token hash claims definition). OIDC Core §3.3.2.11 imposes MUST for c_hash when code is returned from authorization endpoint.

Node 3 · Claim Extractor: Extracted — (1) response_type=code id_token: confirmed; (2) Authorization code returned from auth endpoint: confirmed; (3) id_token returned from auth endpoint: confirmed; (4) id_token includes c_hash: NOT present; (5) Rationale: "server-side session binding is equivalent."

Node 4 · Standard Matcher: OIDC Core §3.3.2.11 MUST: "If the ID Token is issued with a code in response to an Authorization Request, this Claim [c_hash] MUST be present in the ID Token response to the Authorization Request." Matched claim: code returned at auth endpoint; c_hash absent from id_token. Applicability: direct — MUST condition met and requirement violated.

Node 5 · Violation Detector: Violation confirmed. OIDC Core §3.3.2.11 MUST: c_hash required in id_token when code returned from authorization endpoint. Without c_hash: RP has no cryptographic mechanism to verify the code returned in the fragment matches the code the AS intended — code substitution in front channel is undetectable. Server-side session binding does not satisfy the RP-side cryptographic requirement.

Node 6 · Severity Ranker: OIDC Core §3.3.2.11 MUST (unconditional). "Server-side equivalent" rejected: the RP-verifiable c_hash is the normative mechanism; server state is not visible to the RP. Severity: REPROCESS.

Node 7 · Evidence Assembler: Evidence — (1) OIDC Core §3.3.2.11 MUST: c_hash required when code returned; (2) code returned at auth endpoint: yes; (3) c_hash in id_token: absent; (4) Attack: on-path attacker substitutes different code in fragment; RP accepts without detecting mismatch; (5) "Server-side binding" rejected — not RP-verifiable.

Node 8 · Arbiter: REPROCESS. OIDC Core §3.3.2.11 MUST violated: c_hash absent from id_token in hybrid flow where code is returned from authorization endpoint. RP cannot verify code-to-id_token binding. Code substitution attack in front channel undetectable. REPROCESS.

Node 9 · Formatter: Verdict: REPROCESS. Primary citation: OIDC Core §3.3.2.11 MUST. Violation: c_hash absent. Remedy: compute c_hash as BASE64URL(LEFT_HALF(SHA256(ASCII(code)))); include in id_token; RP validates c_hash against received code.

**Reason:** OIDC Core §3.3.2.11 MUST: c_hash required in hybrid flow id_token when code returned. Absent c_hash enables code substitution. Pipeline detects at Node 5; Node 8 issues REPROCESS.

**Phrasing-B:**
Node 1 · Domain Splitter: T-oauth-oidc, T-rfc-jwt. OIDC hybrid flow. id_token c_hash requirement branch.
Node 2 · Source Classifier: OIDC Core §3.3.2.11 MUST primary. c_hash requirement in hybrid flow.
Node 3 · Claim Extractor: response_type=code id_token: yes. Code returned: yes. c_hash in id_token: no. Rationale: "server-side equivalent."
Node 4 · Standard Matcher: OIDC Core §3.3.2.11 MUST: c_hash required when code returned. Matched: absent.
Node 5 · Violation Detector: MUST violated. c_hash absent. Front-channel code substitution undetectable by RP.
Node 6 · Severity Ranker: OIDC Core §3.3.2.11 MUST. Server-side binding not RP-verifiable. REPROCESS.
Node 7 · Evidence Assembler: OIDC Core §3.3.2.11 MUST; c_hash absent; code substitution attack viable; "server binding" rejected.
Node 8 · Arbiter: REPROCESS. OIDC Core §3.3.2.11 MUST: c_hash must be present when code returned at auth endpoint. Code-to-id_token binding absent. REPROCESS.
Node 9 · Formatter: REPROCESS. OIDC Core §3.3.2.11 MUST. Add c_hash: BASE64URL(LEFT_HALF(hash(code))); RP validates.

---

## Trace[CB4-3.4-1074]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** DPoP jti not tracked server-side — DPoP proof replay within freshness window
**Expected verdict:** REPROCESS

**A:**
A resource server validates DPoP proofs including signature, jkt, iat (60-second freshness), htu, and htm, but does not track `jti` values server-side. The same DPoP proof can be submitted multiple times within the iat window. Rationale: "iat freshness window limits replay to 60 seconds — acceptable. jti tracking requires distributed state."

**B:**
Node 1 · Domain Splitter: Input classified as DPoP proof replay prevention at resource server. Territory: T-rfc-jwt (DPoP jti uniqueness), T-asvs-token. Sub-domain: DPoP proof jti cache. Routed to DPoP replay prevention branch.

Node 2 · Source Classifier: Primary source: RFC 9449 §7.2 (RS DPoP proof verification — jti uniqueness). Secondary source: RFC 9449 §11.1 (DPoP security considerations, replay prevention). jti uniqueness checking is a MUST per §7.2.

Node 3 · Claim Extractor: Extracted — (1) DPoP proof signature: validated; (2) jkt binding: validated; (3) iat freshness (60s): validated; (4) htu: validated; (5) htm: validated; (6) jti tracked in cache: NOT done; (7) Same DPoP proof submittable multiple times within 60s: confirmed; (8) Rationale: "iat limits window; jti tracking needs distributed state."

Node 4 · Standard Matcher: RFC 9449 §7.2 MUST: RS must "check that the jti claim value is unique within a reasonable time window." RFC 9449 §11.1: "To prevent DPoP proof replay... the DPoP proof jti claim has to be validated by the server to check that the same DPoP proof JWT has not been used before." Matched claim: jti uniqueness not checked. Applicability: direct — MUST requirement unmet.

Node 5 · Violation Detector: Violation confirmed. RFC 9449 §7.2 MUST: jti uniqueness must be enforced. Without jti tracking, a network attacker who captures a valid DPoP proof can replay it repeatedly within the 60-second iat window — the iat window defines the outer boundary; jti closes the window per-proof. The "distributed state" argument does not override a MUST.

Node 6 · Severity Ranker: RFC 9449 §7.2 MUST (unconditional). iat window does not substitute for jti uniqueness — the two serve different purposes. Distributed Redis cache with TTL = standard solution. Severity: REPROCESS.

Node 7 · Evidence Assembler: Evidence — (1) RFC 9449 §7.2 MUST: jti uniqueness required; (2) jti cache: absent; (3) Attack: captured DPoP proof replayed N times within 60-second window; (4) "iat limits window" rejected — iat defines boundary; jti closes it per-proof; (5) Redis cache (TTL=freshness window) is standard mitigation.

Node 8 · Arbiter: REPROCESS. RFC 9449 §7.2 MUST violated: jti not tracked; same DPoP proof replayable within iat freshness window. iat boundary ≠ jti uniqueness enforcement. Distributed state requirement is implementation detail, not an exception to the MUST. REPROCESS.

Node 9 · Formatter: Verdict: REPROCESS. Primary citation: RFC 9449 §7.2 MUST. Violation: jti not tracked. Remedy: maintain per-proof jti cache (Redis + TTL=freshness window + clock skew); reject jti previously seen within window; evict after TTL.

**Reason:** RFC 9449 §7.2 MUST: jti uniqueness must be checked. Absent cache allows proof replay within freshness window. Pipeline detects at Node 5; Node 8 issues REPROCESS.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-jwt, T-asvs-token. DPoP jti replay prevention. Proof uniqueness tracking branch.
Node 2 · Source Classifier: RFC 9449 §7.2 MUST primary. §11.1 replay prevention guidance secondary.
Node 3 · Claim Extractor: jti in proof: yes. jti tracked server-side: no. Same proof replayable within 60s: yes.
Node 4 · Standard Matcher: RFC 9449 §7.2 MUST: jti uniqueness checked within reasonable window. Matched: not checked.
Node 5 · Violation Detector: MUST violated. No jti cache. Proof replayable N times within iat window.
Node 6 · Severity Ranker: RFC 9449 §7.2 MUST. iat ≠ jti. Distributed state is standard implementation. REPROCESS.
Node 7 · Evidence Assembler: RFC 9449 §7.2 MUST; jti cache absent; proof replay confirmed; "iat substitutes" rejected.
Node 8 · Arbiter: REPROCESS. RFC 9449 §7.2 MUST: jti must be tracked. DPoP proof replay within 60-second window undetected. REPROCESS.
Node 9 · Formatter: REPROCESS. RFC 9449 §7.2 MUST. Add: Redis jti cache; TTL=freshness window; reject seen jti.

---

## Trace[CB4-3.4-1075]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Scope downgrade — token issued with narrowed scope; scope parameter absent from token response
**Expected verdict:** REPROCESS

**A:**
An AS issues a token with scope `read:profile` when client requested `read:profile write:data`. The token response contains `access_token` and `token_type` but no `scope` parameter. Rationale: "The client will get a 403 from the RS. scope is optional per RFC 6749."

**B:**
Node 1 · Domain Splitter: Input classified as OAuth 2.0 token response scope disclosure. Territory: T-rfc-oauth2 (scope parameter in token response), T-asvs-token. Sub-domain: scope downgrade notification. Routed to token response scope branch.

Node 2 · Source Classifier: Primary source: RFC 6749 §3.3 (scope parameter in token response). MUST language applies specifically when issued scope differs from requested scope.

Node 3 · Claim Extractor: Extracted — (1) Requested scope: read:profile write:data; (2) Issued scope: read:profile (narrowed); (3) scope parameter in token response: NOT present; (4) Rationale: "client discovers via 403; scope is optional."

Node 4 · Standard Matcher: RFC 6749 §3.3 MUST: "If the issued access token scope is different from the one requested by the client, the authorization server MUST include the `scope` response parameter to inform the client of the actual scope granted." Matched claim: issued scope ≠ requested scope; scope parameter absent. Applicability: direct — MUST triggered and violated.

Node 5 · Violation Detector: Violation confirmed. RFC 6749 §3.3 MUST: scope must be included when issued scope differs from requested scope. Client has no programmatic mechanism to discover effective scope — forced into 403-driven reactive discovery. "scope optional" accurately describes the optional-when-matching case; when scope differs, it is MUST.

Node 6 · Severity Ranker: RFC 6749 §3.3 MUST (unconditional when scope differs). "Scope is optional" argument conflates the general case with the conditional MUST. 403-driven discovery is operationally broken. Severity: REPROCESS.

Node 7 · Evidence Assembler: Evidence — (1) RFC 6749 §3.3 MUST: scope when different; (2) Issued scope ≠ requested scope: confirmed; (3) scope absent from response: confirmed; (4) Attack surface: client unable to detect downgrade; makes calls expecting write:data permissions, receives unexpected 403s; (5) "Optional" applies when scope matches; MUST applies when scope differs.

Node 8 · Arbiter: REPROCESS. RFC 6749 §3.3 MUST violated: scope downgrade occurred; scope parameter absent from token response. Client cannot programmatically detect the effective scope. 403-driven discovery is not a specification-compliant substitute. REPROCESS.

Node 9 · Formatter: Verdict: REPROCESS. Primary citation: RFC 6749 §3.3 MUST. Violation: scope absent when issued scope ≠ requested scope. Remedy: include scope in token response whenever issued scope differs from requested scope.

**Reason:** RFC 6749 §3.3 MUST: scope parameter required in token response when issued scope ≠ requested scope. Absence prevents client from detecting downgrade. Pipeline detects at Node 5; Node 8 issues REPROCESS.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-oauth2, T-asvs-token. Token response scope disclosure. Scope downgrade notification branch.
Node 2 · Source Classifier: RFC 6749 §3.3 MUST primary. Conditional on scope mismatch.
Node 3 · Claim Extractor: Requested scope: read:profile write:data. Issued scope: read:profile. scope in response: absent.
Node 4 · Standard Matcher: RFC 6749 §3.3 MUST: scope required when issued ≠ requested. Matched: condition met; scope absent.
Node 5 · Violation Detector: MUST violated. Scope downgrade unannounced. Client discovers via 403 only.
Node 6 · Severity Ranker: RFC 6749 §3.3 MUST. "Optional" applies to matching scope; MUST for different scope. REPROCESS.
Node 7 · Evidence Assembler: RFC 6749 §3.3 MUST; scope mismatch confirmed; scope absent; 403-based discovery rejected as substitute.
Node 8 · Arbiter: REPROCESS. RFC 6749 §3.3 MUST: scope must be returned when issued scope ≠ requested. Not done. REPROCESS.
Node 9 · Formatter: REPROCESS. RFC 6749 §3.3 MUST. Fix: include scope=read:profile in token response; client validates against required scopes.

---

## Trace[CB4-3.4-1076]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Token endpoint accepts TLS 1.0 and TLS 1.1 — deprecated protocol versions permitted
**Expected verdict:** REPROCESS

**A:**
An AS token endpoint accepts TLS 1.0, 1.1, 1.2, and 1.3. TLS 1.0 and 1.1 are not disabled. Rationale: "Backward compatibility with legacy clients. No attacks observed. TLS 1.2 negotiated first — lower versions are fallback."

**B:**
Node 1 · Domain Splitter: Input classified as TLS version enforcement at OAuth token endpoint. Territory: T-rfc-oauth2 (transport security requirements), T-asvs-token. Sub-domain: deprecated TLS version acceptance. Routed to TLS protocol version enforcement branch.

Node 2 · Source Classifier: Primary source: RFC 9325 §4 (BCP 195, TLS Best Practices — MUST NOT negotiate TLS 1.0/1.1). Secondary source: RFC 8996 (deprecation of TLS 1.0 and TLS 1.1). Both impose MUST NOT language.

Node 3 · Claim Extractor: Extracted — (1) Token endpoint accepts TLS 1.0: confirmed; (2) Token endpoint accepts TLS 1.1: confirmed; (3) TLS 1.2/1.3 also accepted: confirmed; (4) TLS 1.0/1.1 explicitly disabled: NOT done; (5) Rationale: "backward compatibility; fallback only; no observed attacks."

Node 4 · Standard Matcher: RFC 9325 §4 MUST NOT: "Implementations MUST NOT negotiate TLS 1.0 and TLS 1.1." RFC 8996: TLS 1.0 and TLS 1.1 deprecated. Matched claims: TLS 1.0 and TLS 1.1 not disabled. Applicability: direct — MUST NOT violated.

Node 5 · Violation Detector: Violation confirmed. RFC 9325 §4 MUST NOT: TLS 1.0 and TLS 1.1 must not be negotiated. OAuth token endpoint transmits client secrets and bearer tokens — deprecated TLS version acceptance exposes credential transmission to downgrade attacks (BEAST, POODLE). A TLS downgrade attack forces negotiation of lower versions that a server accepts; "TLS 1.2 negotiated first" is not a defense against forced downgrade.

Node 6 · Severity Ranker: RFC 9325 §4 MUST NOT (unconditional). "No observed attacks" and "backward compatibility" are explicitly rejected as reasons in BCP 195. Token endpoint is a credential-issuing endpoint — TLS downgrade vulnerability is high-severity. Severity: REPROCESS.

Node 7 · Evidence Assembler: Evidence — (1) RFC 9325 §4 MUST NOT: no TLS 1.0/1.1; (2) TLS 1.0 accepted: confirmed; (3) TLS 1.1 accepted: confirmed; (4) Attack: downgrade to TLS 1.0 enables BEAST/POODLE against credential transmission; (5) "No observed attacks" rejected — MUST NOT is unconditional; (6) "Fallback only" rejected — downgrade attacks force fallback intentionally.

Node 8 · Arbiter: REPROCESS. RFC 9325 §4 MUST NOT violated: TLS 1.0 and TLS 1.1 accepted at token endpoint. Credential transmission (client secrets, bearer tokens) exposed to TLS downgrade attacks. Backward compatibility and observed-attack arguments rejected. REPROCESS.

Node 9 · Formatter: Verdict: REPROCESS. Primary citation: RFC 9325 §4 MUST NOT. Violation: TLS 1.0 and TLS 1.1 not disabled. Remedy: set MinProtocol=TLSv1.2; configure TLS 1.3 preferred; restrict to PFS cipher suites.

**Reason:** RFC 9325 §4 MUST NOT: TLS 1.0 and TLS 1.1 must not be negotiated. Token endpoint with deprecated TLS = credential transmission downgrade vulnerability. Pipeline detects at Node 5; Node 8 issues REPROCESS.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-oauth2, T-asvs-token. Token endpoint TLS version enforcement. Deprecated protocol branch.
Node 2 · Source Classifier: RFC 9325 §4 MUST NOT primary. RFC 8996 (deprecation) secondary.
Node 3 · Claim Extractor: TLS 1.0 accepted: yes. TLS 1.1 accepted: yes. TLS 1.0/1.1 disabled: no.
Node 4 · Standard Matcher: RFC 9325 §4 MUST NOT: TLS 1.0 and 1.1 must not be negotiated. Matched: both accepted.
Node 5 · Violation Detector: MUST NOT violated ×2. Credential transmission exposed to TLS downgrade.
Node 6 · Severity Ranker: RFC 9325 §4 MUST NOT. Backward compat and no-observed-attacks rejected. REPROCESS.
Node 7 · Evidence Assembler: RFC 9325 §4 MUST NOT; TLS 1.0 and 1.1 accepted; downgrade viable; "fallback first" rejected.
Node 8 · Arbiter: REPROCESS. RFC 9325 §4 MUST NOT: TLS 1.0/1.1 must not be negotiated. Both accepted. Token endpoint credentials exposed to downgrade. REPROCESS.
Node 9 · Formatter: REPROCESS. RFC 9325 §4 MUST NOT. Fix: MinProtocol=TLSv1.2; prefer TLS 1.3; PFS suites only.

---

## Trace[CB4-3.4-1077]

**Territory:** T-oauth-oidc · T-rfc-jwt
**Domain:** OIDC hybrid flow — id_token missing at_hash when access token returned from authorization endpoint
**Expected verdict:** REPROCESS

**A:**
An OIDC AS hybrid flow (`response_type=code token id_token`) returns code, access token, and id_token from the authorization endpoint. The id_token includes c_hash but not `at_hash`. Rationale: "c_hash covers the code; access token is short-lived (5 min); at_hash is optional per OIDC Core."

**B:**
Node 1 · Domain Splitter: Input classified as OIDC hybrid flow id_token at_hash claim requirement. Territory: T-oauth-oidc (OIDC hybrid flow), T-rfc-jwt (id_token claims). Sub-domain: at_hash claim in hybrid flow with access token. Routed to OIDC hybrid flow id_token at_hash validation branch.

Node 2 · Source Classifier: Primary source: OIDC Core §3.3.2.11 (hybrid flow at_hash requirement). OIDC Core §3.3.2.11 imposes MUST for at_hash when access token is returned from authorization endpoint.

Node 3 · Claim Extractor: Extracted — (1) response_type=code token id_token: confirmed; (2) Access token returned from auth endpoint: confirmed; (3) id_token returned from auth endpoint: confirmed; (4) at_hash in id_token: NOT present; (5) c_hash in id_token: present; (6) Rationale: "at_hash optional per OIDC Core; AT short-lived."

Node 4 · Standard Matcher: OIDC Core §3.3.2.11 MUST: "at_hash: Access Token hash value... If the ID Token is issued with an access_token in response to an Authorization Request, this Claim MUST be present in the ID Token response to the Authorization Request." Matched claim: access token returned at auth endpoint; at_hash absent. Applicability: direct — MUST condition triggered and violated.

Node 5 · Violation Detector: Violation confirmed. OIDC Core §3.3.2.11 MUST: at_hash required when access token returned from authorization endpoint in hybrid flow. Without at_hash: RP cannot cryptographically verify that the access token in the fragment was produced by the same AS operation as the id_token — access token substitution in the front channel is undetectable. Short TTL does not substitute for front-channel binding.

Node 6 · Severity Ranker: OIDC Core §3.3.2.11 MUST (unconditional when AT returned at auth endpoint). "at_hash optional per OIDC Core" correctly describes the optional-when-no-AT-at-auth-endpoint case; when AT is returned at auth endpoint, it is MUST. Short TTL rejected — TTL is post-issuance protection; at_hash is issuance-time binding. Severity: REPROCESS.

Node 7 · Evidence Assembler: Evidence — (1) OIDC Core §3.3.2.11 MUST: at_hash required when AT returned; (2) AT returned at auth endpoint: confirmed; (3) at_hash absent: confirmed; (4) Attack: on-path attacker substitutes different AT in fragment; RP cannot detect via id_token; (5) "Optional" applies to code-only hybrid; MUST applies when AT returned.

Node 8 · Arbiter: REPROCESS. OIDC Core §3.3.2.11 MUST violated: at_hash absent from id_token in hybrid flow where access token is returned from authorization endpoint. Front-channel access token substitution undetectable by RP. "Optional" and "short-lived AT" arguments rejected. REPROCESS.

Node 9 · Formatter: Verdict: REPROCESS. Primary citation: OIDC Core §3.3.2.11 MUST. Violation: at_hash absent. Remedy: compute at_hash as BASE64URL(LEFT_HALF(SHA256(ASCII(access_token)))); include in id_token; RP validates at_hash against received access token.

**Reason:** OIDC Core §3.3.2.11 MUST: at_hash required when access token returned at hybrid flow auth endpoint. Absent at_hash enables front-channel AT substitution. Pipeline detects at Node 5; Node 8 issues REPROCESS.

**Phrasing-B:**
Node 1 · Domain Splitter: T-oauth-oidc, T-rfc-jwt. OIDC hybrid flow. id_token at_hash requirement branch.
Node 2 · Source Classifier: OIDC Core §3.3.2.11 MUST primary. at_hash when AT returned at auth endpoint.
Node 3 · Claim Extractor: AT returned at auth endpoint: yes. at_hash in id_token: no. Rationale: "optional per OIDC Core."
Node 4 · Standard Matcher: OIDC Core §3.3.2.11 MUST: at_hash required when AT issued at auth endpoint. Matched: absent.
Node 5 · Violation Detector: MUST violated. at_hash absent. Front-channel AT substitution undetectable.
Node 6 · Severity Ranker: OIDC Core §3.3.2.11 MUST. "Optional" applies to non-AT flows. Short TTL not a substitute for binding. REPROCESS.
Node 7 · Evidence Assembler: OIDC Core §3.3.2.11 MUST; AT returned; at_hash absent; AT substitution attack viable; "optional" defense rejected.
Node 8 · Arbiter: REPROCESS. OIDC Core §3.3.2.11 MUST: at_hash required when AT returned at authorization endpoint. Absent. REPROCESS.
Node 9 · Formatter: REPROCESS. OIDC Core §3.3.2.11 MUST. Add at_hash: BASE64URL(LEFT_HALF(hash(access_token))); RP validates.

---

## Trace[CB4-3.4-1078]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** `expires_in` absent from token response — clients cannot determine token expiry programmatically
**Expected verdict:** CONCERN

**A:**
An AS issues 30-minute access tokens but omits `expires_in` from token responses. Rationale: "Clients use the token until they get a 401, then refresh. expires_in is optional per RFC 6749 §5.1. TTL is documented in the developer portal."

**B:**
Node 1 · Domain Splitter: Input classified as OAuth 2.0 token response completeness. Territory: T-rfc-oauth2 (token response parameters), T-asvs-token. Sub-domain: expires_in field presence. Routed to token response parameter completeness branch.

Node 2 · Source Classifier: Primary source: RFC 6749 §5.1 (token response parameters — expires_in: RECOMMENDED). RECOMMENDED = RFC 2119 guidance-level term; not MUST. Secondary source: RFC 6749 §5.1 SHOULD for providing expiry by other means.

Node 3 · Claim Extractor: Extracted — (1) Token response includes access_token: confirmed; (2) Token response includes token_type: confirmed; (3) expires_in in response: NOT present; (4) 30-minute AT TTL: confirmed; (5) TTL documented in developer portal: confirmed; (6) Rationale: "clients discover via 401; expires_in is optional."

Node 4 · Standard Matcher: RFC 6749 §5.1 RECOMMENDED: "expires_in: The lifetime in seconds of the access token. If omitted, the authorization server SHOULD provide the expiration time via other means or document the default value." Matched claim: expires_in absent; TTL documented in developer portal (partial SHOULD satisfaction). Applicability: guidance-level — no MUST violated.

Node 5 · Violation Detector: No RFC MUST violated. RFC 6749 §5.1 RECOMMENDED not met (expires_in absent). SHOULD partially satisfied (developer portal documents TTL). No programmatic client discovery possible. Clients forced into reactive 401-driven refresh pattern. Guidance-level gap only.

Node 6 · Severity Ranker: RFC 6749 §5.1 RECOMMENDED (not MUST). SHOULD for "other means" partially satisfied by documentation but not programmatically discoverable. No unconditional MUST for expires_in presence. Severity: CONCERN.

Node 7 · Evidence Assembler: Evidence — (1) RFC 6749 §5.1 RECOMMENDED: expires_in should be present; (2) expires_in absent: confirmed; (3) Developer portal documents TTL: partially satisfies SHOULD; (4) Programmatic discovery: not possible; (5) Client behavior forced to reactive pattern; (6) No MUST for specific field → guidance gap.

Node 8 · Arbiter: CONCERN. RFC 6749 §5.1 RECOMMENDED not met: expires_in absent from token response. SHOULD for "other means" partially satisfied by developer documentation but not programmatically discoverable by clients. Reactive 401-driven refresh is an operational concern but no RFC MUST is violated. CONCERN. Recommend: include expires_in in all token responses.

Node 9 · Formatter: Verdict: CONCERN. Primary citation: RFC 6749 §5.1 RECOMMENDED. Gap: expires_in absent; clients cannot proactively manage token expiry. Recommendation: include expires_in (seconds) in all token responses to enable proactive refresh.

**Reason:** RFC 6749 §5.1 RECOMMENDED: expires_in should be present. No MUST violated; documentation partially satisfies SHOULD. Guidance gap = CONCERN. Pipeline classifies at Node 6.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-oauth2, T-asvs-token. Token response parameter completeness. expires_in presence branch.
Node 2 · Source Classifier: RFC 6749 §5.1 RECOMMENDED primary. Not MUST.
Node 3 · Claim Extractor: expires_in: absent. TTL documented in portal: yes. Rationale: "optional; clients discover via 401."
Node 4 · Standard Matcher: RFC 6749 §5.1 RECOMMENDED: expires_in. SHOULD if omitted: provide by other means. Matched: absent; partial SHOULD via docs.
Node 5 · Violation Detector: No RFC MUST. RECOMMENDED not met. SHOULD partially met. Guidance gap only.
Node 6 · Severity Ranker: RFC 6749 §5.1 RECOMMENDED. No MUST. CONCERN.
Node 7 · Evidence Assembler: RFC 6749 §5.1 RECOMMENDED not met; expires_in absent; portal docs partial SHOULD; no programmatic discovery; reactive 401 pattern.
Node 8 · Arbiter: CONCERN. RFC 6749 §5.1 RECOMMENDED not met. No MUST violated. Guidance gap. CONCERN.
Node 9 · Formatter: CONCERN. RFC 6749 §5.1 RECOMMENDED. Recommend: add expires_in to all token responses for proactive client token management.
