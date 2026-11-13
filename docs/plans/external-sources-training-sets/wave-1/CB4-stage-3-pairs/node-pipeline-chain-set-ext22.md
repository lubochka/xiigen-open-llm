# CB4 Stage 3 — Action 3 Extended Pair Set
# Node: Multi-node pipeline chains (ext22 — ext28 pipeline traces)
# Sequences: CB4-3.4-733 through CB4-3.4-740
# Instances: 16 (8 base pairs × 2 phrasings each)
# Stratum: C (full 9-node pipeline traces)
# Running total after this file: 1480 instances (740 base pairs)

---

## Pair[CB4-3.4-733]

### A
Full pipeline trace — AS discovery metadata served over HTTP:

Input: Authorization server publishes its discovery document at http://as.example.com/.well-known/oauth-authorization-server (plain HTTP). The document lists all OAuth endpoints. Team argues: "It's a public document — no confidential data is exposed."

### B
Node 1 — Domain Splitter: Input domain identified as T-rfc-oauth2 (RFC 8414 authorization server metadata). Sub-domain: AS metadata endpoint transport security. Discovery endpoint transport is a distinct evaluation surface from the endpoints listed within the document. Routed to T-rfc-oauth2 for RFC 8414 §3 transport evaluation.

Node 2 — Domain Translator: Translated to evaluation schema. Subject: AS metadata endpoint protocol. Control under evaluation: use of HTTP vs HTTPS for .well-known/oauth-authorization-server. Applicable normative reference: RFC 8414 §3 — "The authorization server MUST use the HTTPS scheme." Argument to evaluate: public content eliminates HTTPS requirement.

Node 3 — Scheme Planner: Plan: (1) evaluate RFC 8414 §3 MUST against observed HTTP transport; (2) assess whether public-content argument has normative weight; (3) identify specific threat model addressed by HTTPS requirement; (4) determine verdict level. Two-step evaluation: transport compliance first; argument rebuttal second.

Node 4 — Domain Processor:
  Observation: http:// scheme confirmed at discovery URL.
  RFC 8414 §3 check: MUST HTTPS — violated. The requirement is not conditional on whether the document contains credentials.
  Threat model: HTTPS protects document integrity, not confidentiality. A network adversary can intercept HTTP responses and substitute attacker-controlled endpoint URLs (token endpoint, authorization endpoint, JWKS URI) in the discovery document. Clients bootstrapping from the tampered document send credentials to attacker-controlled endpoints.
  Argument assessment: "Public document" addresses confidentiality concern only. The HTTPS MUST requirement addresses integrity. The argument is category-mismatched — it rejects the wrong threat model.

Node 5 — Applier: RFC 8414 §3 MUST applied. HTTP transport = MUST violation. Argument rejection confirmed: public-content reasoning does not modify MUST obligations. REPROCESS trigger: 1.

Node 6 — Result Translator: Single MUST violation (RFC 8414 §3 transport). One REPROCESS trigger. Verdict: REPROCESS.

Node 7 — Arbiter Selector: Activated: T-rfc-oauth2 arbiter for RFC 8414 MUST enforcement. No additional arbiter units required.

Node 8 — Arbiter Units: REPROCESS. RFC 8414 §3 MUST HTTPS violated. HTTP discovery endpoint allows MITM substitution of all listed endpoint URLs. The public-content argument is rejected — HTTPS at the metadata endpoint protects configuration integrity, not credential confidentiality.

Node 9 — Formatter: Verdict: REPROCESS. Primary citation: RFC 8414 §3. Argument rejected: "public document needs no HTTPS" (integrity requirement, not confidentiality). Remediation: serve discovery document at https:// scheme with valid TLS certificate.

### Reason
RFC 8414 §3 is an unconditional MUST. The integrity-not-confidentiality distinction is the key rebuttal. Full pipeline produces REPROCESS on single clear violation.

### Phrasing-B
Node 1: T-rfc-oauth2. Discovery endpoint transport. RFC 8414 §3 domain.
Node 2: Subject: http:// scheme at .well-known endpoint. Normative reference: RFC 8414 §3 MUST HTTPS.
Node 3: Plan: (1) RFC 8414 §3 compliance check; (2) public-content argument evaluation.
Node 4: HTTP confirmed. RFC 8414 §3 violated. Threat: MITM replaces endpoint URLs in document; clients directed to attacker infrastructure. Argument category mismatch: "public" addresses confidentiality; RFC 8414 §3 addresses integrity.
Node 5: MUST violation. Argument rejected. REPROCESS trigger: 1.
Node 6: REPROCESS.
Node 7: T-rfc-oauth2 arbiter.
Node 8: REPROCESS. RFC 8414 §3 MUST HTTPS violated. Endpoint URL substitution attack viable via HTTP transport. Public content does not eliminate integrity requirement.
Node 9: REPROCESS. RFC 8414 §3. Serve discovery over HTTPS.

---

## Pair[CB4-3.4-734]

### A
Full pipeline trace — AS discovery correctly served over HTTPS with matching issuer:

Input: Authorization server discovery at https://as.example.com/.well-known/oauth-authorization-server. Returned document: issuer "https://as.example.com"; all endpoint URLs use https://. Valid CA certificate. Issuer field matches discovery base URL.

### B
Node 1 — Domain Splitter: Domain T-rfc-oauth2 (RFC 8414 AS metadata). Sub-domain: discovery endpoint transport and issuer binding. Two evaluation points: (1) HTTPS transport compliance; (2) issuer field consistency with discovery URL.

Node 2 — Domain Translator: Subject: HTTPS discovery with issuer=https://as.example.com. Applicable references: RFC 8414 §3 (MUST HTTPS); RFC 8414 §3.3 (issuer field must identify the AS).

Node 3 — Scheme Planner: Plan: (1) confirm HTTPS scheme; (2) verify all endpoint URLs in document use HTTPS; (3) verify issuer field matches discovery base URL.

Node 4 — Domain Processor:
  HTTPS confirmed at discovery URL with valid CA certificate.
  All endpoint URLs in document: https:// scheme verified.
  Issuer field "https://as.example.com" matches base URL "https://as.example.com" used to construct discovery URL.
  No gaps identified.

Node 5 — Applier: RFC 8414 §3 satisfied. Issuer binding consistent. No violations.

Node 6 — Result Translator: Zero violations. Verdict: PASS.

Node 7 — Arbiter Selector: No arbiter activation required for PASS path. Confirming evaluation complete.

Node 8 — Arbiter Units: PASS. HTTPS delivery confirmed. All endpoints HTTPS. Issuer matches discovery base URL — clients can validate §4.3-style binding. No configuration integrity gaps.

Node 9 — Formatter: Verdict: PASS. RFC 8414 §3 satisfied. Issuer binding consistent.

### Reason
All RFC 8414 §3 requirements met: HTTPS transport, HTTPS endpoint URLs, issuer field consistency. Clean PASS.

### Phrasing-B
Node 1: T-rfc-oauth2. HTTPS discovery + issuer binding.
Node 2: HTTPS at discovery URL. Issuer = "https://as.example.com". RFC 8414 §3.
Node 3: (1) HTTPS check; (2) endpoint URL scheme check; (3) issuer match.
Node 4: HTTPS confirmed. All endpoints https://. Issuer matches. No gaps.
Node 5: No violations.
Node 6: PASS.
Node 7: No activation.
Node 8: PASS. RFC 8414 §3 satisfied. Issuer binding intact.
Node 9: PASS.

---

## Pair[CB4-3.4-735]

### A
Full pipeline trace — OIDC Discovery issuer field does not match discovery URL base:

Input: Client fetches https://idp.example.com/.well-known/openid-configuration. Returned document: issuer "https://auth.internal.example.com". Team: "Both hostnames are the same server — internal vs external name."

### B
Node 1 — Domain Splitter: Domain T-rfc-oidc (OIDC Discovery). Sub-domain: issuer field binding. OpenID Connect Discovery §4.3 defines the issuer-URL matching requirement. Routed to T-rfc-oidc for §4.3 evaluation.

Node 2 — Domain Translator: Subject: issuer field in discovery document. Discovery fetch URL: https://idp.example.com. Issuer in document: https://auth.internal.example.com. Applicable reference: OpenID Connect Discovery §4.3 — "The issuer value returned MUST be identical to the Issuer URL that was directly used to retrieve the configuration information." Argument: dual-hostname architecture (same server, different names).

Node 3 — Scheme Planner: Plan: (1) compare issuer field to discovery base URL; (2) evaluate dual-hostname argument; (3) identify downstream consequences of mismatch; (4) determine verdict.

Node 4 — Domain Processor:
  Issuer field: "https://auth.internal.example.com"
  Discovery base URL: "https://idp.example.com"
  Comparison: not identical. §4.3 MUST requirement: violated.
  Dual-hostname argument: DNS resolution at discovery time is not a persistent cryptographic binding. An attacker who can redirect the discovery request (SSRF, DNS manipulation) to a different server could serve any issuer's discovery document; without issuer-URL matching validation, the client cannot distinguish the authentic document from a substituted one.
  Downstream consequence: tokens issued with iss="https://auth.internal.example.com" will fail iss validation for clients who configured their expected issuer from the external discovery URL ("https://idp.example.com"). Mismatch causes both security gap and operational token validation failure.

Node 5 — Applier: OpenID Connect Discovery §4.3 MUST violated. Argument rejected. REPROCESS trigger: 1.

Node 6 — Result Translator: REPROCESS.

Node 7 — Arbiter Selector: T-rfc-oidc arbiter for OIDC Discovery §4.3.

Node 8 — Arbiter Units: REPROCESS. OpenID Connect Discovery §4.3 MUST violated — issuer field does not match the URL used to fetch the discovery document. The dual-hostname argument addresses DNS resolution, not the §4.3 binding requirement. Downstream consequence: token iss validation will also fail for external clients.

Node 9 — Formatter: Verdict: REPROCESS. Primary citation: OpenID Connect Discovery §4.3. Argument rejected: "same server with different hostname" (DNS resolution ≠ §4.3 binding). Remediation: configure IdP issuer to https://idp.example.com (the externally-reachable discovery URL).

### Reason
OpenID Connect Discovery §4.3 MUST is unconditional — exact string match required. Dual-hostname explanation addresses DNS, not the §4.3 trust binding. Downstream token validation failures confirm the gap is operational as well as security-relevant.

### Phrasing-B
Node 1: T-rfc-oidc. OIDC Discovery §4.3 domain.
Node 2: Discovery URL: https://idp.example.com. Issuer in document: https://auth.internal.example.com. OpenID Connect Discovery §4.3 MUST identical.
Node 3: (1) Exact string comparison; (2) dual-hostname argument evaluation; (3) downstream consequences.
Node 4: Mismatch confirmed. §4.3 violated. Dual-hostname does not satisfy MUST identical. Downstream: tokens with iss=internal fail validation for external clients.
Node 5: MUST violated. Argument rejected. REPROCESS trigger: 1.
Node 6: REPROCESS.
Node 7: T-rfc-oidc.
Node 8: REPROCESS. §4.3 MUST violated. Configure issuer to match external discovery URL.
Node 9: REPROCESS. OpenID Connect Discovery §4.3. Configure IdP issuer to https://idp.example.com.

---

## Pair[CB4-3.4-736]

### A
Full pipeline trace — OIDC Discovery issuer correctly matches discovery URL:

Input: Discovery fetch from https://idp.example.com/.well-known/openid-configuration. Returned document: issuer "https://idp.example.com". All tokens carry iss: "https://idp.example.com". Client validates issuer field matches fetch URL per §4.3.

### B
Node 1 — Domain Splitter: T-rfc-oidc. OIDC Discovery §4.3 domain.

Node 2 — Domain Translator: Issuer field "https://idp.example.com" vs discovery base URL "https://idp.example.com". OpenID Connect Discovery §4.3.

Node 3 — Scheme Planner: (1) String comparison of issuer to discovery base URL.

Node 4 — Domain Processor: Issuer "https://idp.example.com" = discovery base "https://idp.example.com". Exact match. Downstream: tokens with iss="https://idp.example.com" match client's configured expected issuer. No mismatch.

Node 5 — Applier: §4.3 satisfied. No violations.

Node 6 — Result Translator: PASS.

Node 7 — Arbiter Selector: No activation.

Node 8 — Arbiter Units: PASS. OpenID Connect Discovery §4.3 satisfied. Issuer binding intact.

Node 9 — Formatter: Verdict: PASS. §4.3 issuer binding confirmed.

### Reason
Exact issuer match satisfies §4.3 MUST. Token iss values consistent with validated discovery issuer. PASS.

### Phrasing-B
Node 1: T-rfc-oidc. §4.3 evaluation.
Node 2: Issuer = "https://idp.example.com". Discovery base = "https://idp.example.com". OpenID Connect Discovery §4.3.
Node 3: String comparison.
Node 4: Match confirmed. No downstream inconsistency.
Node 5: No violations.
Node 6: PASS.
Node 7: No activation.
Node 8: PASS. §4.3 satisfied.
Node 9: PASS.

---

## Pair[CB4-3.4-737]

### A
Full pipeline trace — JWT tokens issued without kid header during multi-key JWKS rollover:

Input: JWKS has two active keys (key-2024, key-2025) during rollover. Access tokens issued without kid header. RS must try both keys per token. Team: "Validation always succeeds eventually — both keys are valid."

### B
Node 1 — Domain Splitter: Domain T-rfc-jwt (JWT header parameters). Sub-domain: kid hint during multi-key operation. RFC 7515 §4.1.4 applies. Separate from algorithm allowlist (separate sub-domain).

Node 2 — Domain Translator: Subject: kid absence in JWT header with 2 active JWKS keys. Applicable reference: RFC 7515 §4.1.4 SHOULD use kid when multiple keys are present. Team argument: eventual-success argument.

Node 3 — Scheme Planner: Plan: (1) RFC 7515 §4.1.4 compliance check; (2) assess operational risks beyond the eventually-succeeds claim; (3) evaluate whether SHOULD violation reaches REPROCESS or remains CONCERN.

Node 4 — Domain Processor:
  kid header: absent.
  JWKS active keys: 2 (key-2024, key-2025).
  RFC 7515 §4.1.4 SHOULD: "kid is a hint indicating which key was used to secure the JWS ... used to match a JWK kid parameter value." SHOULD-level, not MUST.
  Operational risks:
    (a) Performance: O(n) key attempts per token request.
    (b) Cache invalidation signal absent: if kid is present in a token but absent from the RS's cached JWKS, the RS knows to re-fetch. Without kid, the RS cannot distinguish stale-cache failure from genuinely invalid token — must re-fetch speculatively.
    (c) During rollover with 3+ simultaneous keys: validation ambiguity increases proportionally.
  Eventual-success argument: accurate for normal operation but ignores the operational failure modes above.
  Verdict level: SHOULD violation; token validation succeeds; no MUST requirement violated. CONCERN level.

Node 5 — Applier: RFC 7515 §4.1.4 SHOULD not satisfied. One CONCERN-level gap. No REPROCESS trigger.

Node 6 — Result Translator: CONCERN.

Node 7 — Arbiter Selector: T-rfc-jwt arbiter for SHOULD-level kid header guidance.

Node 8 — Arbiter Units: CONCERN. RFC 7515 §4.1.4 SHOULD violation — kid header absent during multi-key JWKS rollover. Token validation eventually succeeds but the absence removes the cache-refresh signal and increases per-request key attempt count. SHOULD-level gap; not a MUST violation. CONCERN.

Node 9 — Formatter: Verdict: CONCERN. RFC 7515 §4.1.4 SHOULD. Recommendation: include kid header in all issued JWTs, matching the key ID of the signing key used. Enables O(1) key lookup and targeted JWKS cache refresh.

### Reason
RFC 7515 §4.1.4 is SHOULD not MUST. Validation succeeds without kid. Operational risks (performance, cache signal) establish CONCERN. The eventual-success argument is accepted as accurate but incomplete.

### Phrasing-B
Node 1: T-rfc-jwt. kid header presence during multi-key JWKS.
Node 2: kid absent. 2 active JWKS keys. RFC 7515 §4.1.4 SHOULD.
Node 3: (1) §4.1.4 compliance; (2) operational risk analysis; (3) SHOULD vs MUST determination.
Node 4: kid absent confirmed. §4.1.4 SHOULD not met. Operational risks: O(n) attempts; no cache-refresh signal; rollover ambiguity. Eventual-success accurate but incomplete. SHOULD-level gap.
Node 5: CONCERN-level gap. No MUST violation.
Node 6: CONCERN.
Node 7: T-rfc-jwt.
Node 8: CONCERN. RFC 7515 §4.1.4 SHOULD — kid absent during multi-key rollover introduces operational risk without causing MUST violation.
Node 9: CONCERN. RFC 7515 §4.1.4. Include kid in JWT headers matching signing key ID.

---

## Pair[CB4-3.4-738]

### A
Full pipeline trace — JWT tokens with kid header, correct JWKS key lookup:

Input: JWKS has two active keys (key-2024, key-2025). Each JWT carries kid: "key-2025" in JOSE header. RS reads kid from header, looks up that specific key in cached JWKS, validates. If kid not found in cache, RS re-fetches JWKS once then retries.

### B
Node 1 — Domain Splitter: T-rfc-jwt. kid header presence and JWKS key matching.

Node 2 — Domain Translator: kid="key-2025" in header. JWKS contains key-2025. RFC 7515 §4.1.4 SHOULD.

Node 3 — Scheme Planner: (1) Verify kid matches an active JWKS key ID; (2) Verify RS lookup behavior; (3) Evaluate cache-refresh-on-miss pattern.

Node 4 — Domain Processor:
  kid="key-2025" matches key ID in JWKS. O(1) lookup: RS selects key-2025, validates signature directly.
  Cache-refresh-on-miss: if kid not in cache, RS re-fetches before failing — targeted, not speculative.
  RFC 7515 §4.1.4 SHOULD: satisfied.

Node 5 — Applier: §4.1.4 satisfied. No violations.

Node 6 — Result Translator: PASS.

Node 7 — Arbiter Selector: No activation.

Node 8 — Arbiter Units: PASS. kid header present in all JWTs matching active signing key. O(1) key selection. Cache-refresh logic triggered on kid-not-found rather than on validation failure. RFC 7515 §4.1.4 SHOULD satisfied.

Node 9 — Formatter: Verdict: PASS. RFC 7515 §4.1.4 satisfied.

### Reason
kid header present and matching. O(1) key lookup. Cache-refresh-on-miss pattern correct. All §4.1.4 SHOULD guidance implemented.

### Phrasing-B
Node 1: T-rfc-jwt. kid lookup.
Node 2: kid="key-2025". JWKS has key-2025. RFC 7515 §4.1.4.
Node 3: kid match check; RS lookup behavior check.
Node 4: Match confirmed. O(1) lookup. Cache-refresh on kid-not-found. No gaps.
Node 5: No violations.
Node 6: PASS.
Node 7: No activation.
Node 8: PASS. §4.1.4 SHOULD satisfied.
Node 9: PASS.

---

## Pair[CB4-3.4-739]

### A
Full pipeline trace — Token endpoint accepts unauthenticated requests from registered confidential clients:

Input: Clients are registered as confidential with client_secret. Token endpoint accepts requests with client_id only (no client_secret), issuing tokens without authentication. Team: "Flexible auth — clients that know their secret provide it." Penetration test confirms: client_id alone yields valid access token.

### B
Node 1 — Domain Splitter: Domain T-rfc-oauth2 (RFC 6749 §2.3 client authentication). Sub-domain: confidential client authentication enforcement at token endpoint. Separate from grant type validation (RFC 6749 §3.1.1) — this is about authentication, not grant type scope.

Node 2 — Domain Translator: Subject: token endpoint authentication requirements for confidential clients. Applicable reference: RFC 6749 §2.3 — "Confidential clients or other clients issued client credentials MUST authenticate with the authorization server." Argument: flexible authentication model.

Node 3 — Scheme Planner: Plan: (1) RFC 6749 §2.3 MUST compliance check; (2) evaluate whether "flexible auth" argument modifies MUST obligation; (3) assess penetration test outcome; (4) determine verdict.

Node 4 — Domain Processor:
  Client classification: confidential (client_secret issued at registration).
  Token endpoint behavior: accepts requests with client_id and no client_secret.
  RFC 6749 §2.3 MUST: confidential clients MUST authenticate. No exceptions defined for "flexible" models.
  Argument assessment: "flexible auth" inverts the security model. A confidential client is classified as such because it can maintain credential secrecy; the classification carries the MUST authentication obligation unconditionally. The server's acceptance of no-auth requests effectively reclassifies all confidential clients as public clients — without the architectural justification for public client status.
  Penetration test outcome: client_id alone yields valid token. The client_id is observable in any intercepted authorization code flow, in browser history, and in publicly-documented OAuth flows. Any party with a known client_id can impersonate the registered confidential client.
  Practical impact: confidential clients with privileged scopes (service accounts, admin APIs, reporting access) can be impersonated by any caller who knows the client_id.

Node 5 — Applier: RFC 6749 §2.3 MUST violated. Argument rejected — MUST has no flexibility clause. Penetration test confirms exploitation. REPROCESS trigger: 1.

Node 6 — Result Translator: REPROCESS.

Node 7 — Arbiter Selector: T-rfc-oauth2 arbiter for RFC 6749 §2.3 client authentication MUST.

Node 8 — Arbiter Units: REPROCESS. RFC 6749 §2.3 MUST violated — confidential clients can authenticate with client_id alone, without providing the registered client_secret. The flexible-auth model eliminates the security guarantee that the confidential client classification is supposed to provide. Penetration test confirms exploitation: any party knowing the client_id can obtain tokens.

Node 9 — Formatter: Verdict: REPROCESS. Primary citation: RFC 6749 §2.3. Argument rejected: "flexible auth" (MUST unconditional; no flexibility clause for confidential client authentication). Remediation: reject all token requests from confidential clients that do not include valid client authentication credentials; return 401 with WWW-Authenticate header.

### Reason
RFC 6749 §2.3 MUST is unconditional for confidential clients. The "flexible" framing does not constitute a recognized exception. Penetration test confirms the MUST violation is exploitable. REPROCESS.

### Phrasing-B
Node 1: T-rfc-oauth2. RFC 6749 §2.3 confidential client authentication domain.
Node 2: Token endpoint accepts client_id only. RFC 6749 §2.3 MUST authenticate. Flexible-auth argument.
Node 3: (1) §2.3 compliance; (2) argument evaluation; (3) impact assessment.
Node 4: Confidential client with client_secret registered. Token endpoint issues tokens without credential check. §2.3 MUST violated — no exception for "flexible" model. client_id is observable in any OAuth flow; any caller can impersonate. Pentest confirms.
Node 5: MUST violated. Argument rejected. REPROCESS trigger.
Node 6: REPROCESS.
Node 7: T-rfc-oauth2.
Node 8: REPROCESS. RFC 6749 §2.3 MUST — confidential clients MUST authenticate. Token endpoint accepts no-credential requests. Pentest-confirmed exploitation.
Node 9: REPROCESS. RFC 6749 §2.3. Enforce client credential validation for all confidential client token requests.

---

## Pair[CB4-3.4-740]

### A
Full pipeline trace — Resource server accepts access tokens without validating the aud claim:

Input: System has 3 resource servers (api.example.com, reports.example.com, admin.example.com) each issuing tokens with different aud values. All RSs validate signature and expiry only — aud claim is not checked. A user with an api.example.com token presents it to admin.example.com; admin.example.com processes the request. Team: "Same issuer = trusted token."

### B
Node 1 — Domain Splitter: Domain T-rfc-jwt (JWT claim validation) + T-asvs-token (token scope validation). Sub-domain: audience claim validation at resource server. Multiple RS scenario — token intended for one RS used at another. Audience confusion attack pattern.

Node 2 — Domain Translator: Subject: RS aud claim validation. Three resource servers with distinct aud values. Token with aud=["api.example.com"] accepted at admin.example.com. Applicable references: RFC 9068 §4 (JWT Profile for OAuth 2.0 Access Tokens — aud validation required at RS); RFC 8725 §3.9 (Validate the aud claim); ASVS V9.1.1 adjacent (token claim validation). Argument: same-issuer trust.

Node 3 — Scheme Planner: Plan: (1) RFC 9068 §4 aud validation requirement at RS; (2) RFC 8725 §3.9 compliance; (3) evaluate same-issuer argument; (4) assess attack vector; (5) determine verdict.

Node 4 — Domain Processor:
  Observation: aud claim present in access tokens (aud=["api.example.com"]) but not validated by admin.example.com.
  RFC 9068 §4: the RS MUST verify that its own identifier appears in the aud claim. The RS's failure to check aud means it processes tokens not intended for it.
  RFC 8725 §3.9: "Validate the aud claim." Listed as a core JWT validation step applicable to all JWT consumers.
  Same-issuer argument assessment: the issuer claim proves the token was signed by the trusted AS. It does not establish that this token was issued for this RS. The aud claim is precisely the mechanism for per-RS scope of authorization — it binds the token to one or more specific recipients. A token with valid issuer and signature but aud pointing to a different RS is not authorized for the requesting RS, regardless of whether both RSs share the same trusted issuer.
  Attack vector: audience confusion attack. A user (or attacker) with a valid token for a low-privilege RS (api.example.com) presents it at a high-privilege RS (admin.example.com). Without aud validation, the high-privilege RS processes it. No token forgery required — a legitimately-issued token is misused at an unintended recipient. This is the canonical threat model that the aud claim and RFC 9068 §4 aud validation requirement exist to prevent.
  Scope consequence: admin.example.com may return sensitive data or execute privileged operations based on a token that was never authorized for those operations.

Node 5 — Applier: RFC 9068 §4 aud validation requirement at RS: violated. RFC 8725 §3.9: violated. Same-issuer argument rejected — issuer ≠ intended recipient. Audience confusion attack vector confirmed. REPROCESS trigger: 1.

Node 6 — Result Translator: REPROCESS.

Node 7 — Arbiter Selector: T-rfc-jwt arbiter for JWT claim validation (RFC 8725 §3.9); T-asvs-token arbiter for access token scope validation. Both activated.

Node 8 — Arbiter Units: REPROCESS. RFC 9068 §4 aud validation absent at resource server. RFC 8725 §3.9 aud validation requirement not met. Audience confusion attack is viable: valid access token issued for api.example.com is accepted by admin.example.com, granting access to administrative operations outside the token's authorized scope. The same-issuer-equals-trust argument is rejected — the issuer establishes token authenticity; the aud claim establishes the intended recipient(s). Both validations are independently required.

Node 9 — Formatter: Verdict: REPROCESS. Primary citations: RFC 9068 §4; RFC 8725 §3.9. Argument rejected: "same issuer = trusted" (authenticity ≠ authorization scope; aud establishes intended recipient). Remediation: each RS must validate that its own registered identifier appears in the token's aud claim before processing any request; reject with 401 if aud does not include the RS identifier.

### Reason
RFC 9068 §4 and RFC 8725 §3.9 both require RS-side aud validation. The audience confusion attack — using a legitimate token at an unintended RS — is the exact threat model both requirements address. Same-issuer trust does not substitute for per-recipient aud validation.

### Phrasing-B
Node 1: T-rfc-jwt + T-asvs-token. RS aud claim validation. Audience confusion attack domain.
Node 2: 3 RSs, distinct aud values. api.example.com token used at admin.example.com. RFC 9068 §4; RFC 8725 §3.9. Same-issuer argument.
Node 3: (1) RFC 9068 §4 check; (2) RFC 8725 §3.9 check; (3) argument evaluation; (4) attack vector.
Node 4: aud not validated at admin.example.com. RFC 9068 §4 RS MUST verify aud contains its identifier — violated. Same-issuer argument conflates token authenticity (issuer) with authorization scope (aud). Audience confusion: valid api.example.com token grants admin access without forgery.
Node 5: RFC 9068 §4 violated. RFC 8725 §3.9 violated. Argument rejected. REPROCESS trigger.
Node 6: REPROCESS.
Node 7: T-rfc-jwt + T-asvs-token.
Node 8: REPROCESS. RFC 9068 §4 + RFC 8725 §3.9 — RS aud validation absent. Audience confusion attack: legitimate token for low-privilege RS accepted at high-privilege RS. MUST validate aud before processing.
Node 9: REPROCESS. RFC 9068 §4; RFC 8725 §3.9. Each RS validate its identifier in token aud before processing.
