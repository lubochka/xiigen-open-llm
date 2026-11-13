# CB4 Stage 3 — Action 3 Extended Pair Set
# Node: Node 8 (Arbiter Units) — ext28
# Sequences: CB4-3.4-725 through CB4-3.4-732
# Instances: 16 (8 base pairs × 2 phrasings each)
# Stratum: A/B/D (new domains: AS discovery integrity, OIDC issuer binding, kid header, client auth enforcement, RS aud validation)
# Running total after this file: 1464 instances (732 base pairs)

---

## Pair[CB4-3.4-725]

### A
Authorization server metadata discovery endpoint served over HTTP (RFC 8414 §3 MUST HTTPS):

An authorization server exposes its discovery document at http://as.example.com/.well-known/oauth-authorization-server (plain HTTP). The document lists the token endpoint, authorization endpoint, JWKS URI, and supported grant types. The server team argues: "The discovery document is entirely public — it contains no credentials, no user data, and nothing confidential. Using HTTPS would add unnecessary TLS overhead for a document that anyone can read anyway. All the actual sensitive endpoints (token, introspection) use HTTPS."

### B
REPROCESS. RFC 8414 §3 states: "The authorization server MUST use the HTTPS scheme." The team's argument — that the document's public nature removes the HTTPS requirement — is rejected. The requirement is not conditional on whether the document contains credentials.

The attack surface is metadata integrity, not confidentiality. A network adversary who can intercept HTTP traffic can replace the discovery document in transit and substitute attacker-controlled endpoint URLs: the token endpoint URL replaced with https://attacker.example.com/token; the authorization endpoint replaced with a phishing authorization server; the JWKS URI replaced with an attacker-controlled key set. Clients that bootstrap their OAuth configuration from the discovery document will be configured to send authorization codes, tokens, and credentials to the attacker-controlled endpoints. The document's contents are public; its integrity is not.

The HTTPS-on-sensitive-endpoints-only argument is rejected. Clients fetch the discovery document first to learn where those sensitive endpoints are. If the discovery document is replaceable, all subsequent HTTPS connections to the correct endpoints are irrelevant — the client never reaches them.

RFC 8414 §3 MUST requirement: violated. REPROCESS.

### Reason
RFC 8414 §3 is an unconditional MUST on HTTPS for the metadata endpoint. The MITM-replacement attack on endpoint URLs is the specific threat model — the document's public readability does not eliminate the integrity requirement.

### Phrasing-B
REPROCESS — RFC 8414 §3 MUST HTTPS violated. The public-content argument does not address the integrity requirement.

Authorization server metadata served over HTTP allows any on-path adversary to replace the discovery document. The replacement does not need to expose credentials — it only needs to substitute the token endpoint, authorization endpoint, and JWKS URI values. A client reading the tampered discovery document will use attacker-controlled endpoints for all subsequent OAuth operations. The HTTPS requirement protects the integrity of the bootstrapping configuration, not the confidentiality of the document content.

The argument that sensitive endpoints use HTTPS is incomplete — those endpoints are unreachable by a client that has already been misdirected by a tampered discovery document. RFC 8414 §3 MUST violated. REPROCESS.

---

## Pair[CB4-3.4-726]

### A
Authorization server discovery endpoint correctly served over HTTPS with matching issuer:

An authorization server publishes its discovery document at https://as.example.com/.well-known/oauth-authorization-server. The document contains: issuer: "https://as.example.com"; token_endpoint: "https://as.example.com/token"; authorization_endpoint: "https://as.example.com/authorize"; jwks_uri: "https://as.example.com/.well-known/jwks.json". All listed endpoint URLs use HTTPS. The discovery URL base (https://as.example.com) matches the issuer field exactly. TLS certificate is valid and issued by a public CA.

### B
PASS. RFC 8414 §3 HTTPS requirement satisfied. All endpoint URLs in the discovery document use HTTPS. The issuer field matches the discovery URL base — the document's binding to the claimed authorization server is verifiable.

The HTTPS transport protects metadata integrity: clients receiving the discovery document over TLS can rely on the endpoint URLs not having been substituted in transit. Endpoint substitution attacks are not viable against HTTPS-delivered discovery documents with valid certificates.

The issuer field "https://as.example.com" matches the URL prefix used to fetch the document (https://as.example.com/.well-known/oauth-authorization-server). Clients that validate issuer-discovery-URL binding can confirm they received a discovery document from the intended authorization server. No findings.

### Reason
RFC 8414 §3 MUST HTTPS satisfied. All document endpoint URLs use HTTPS. Issuer field matches discovery base URL, enabling client-side binding validation.

### Phrasing-B
PASS — discovery document correctly served over HTTPS with all endpoint URLs and issuer matching.

HTTPS delivery from https://as.example.com/.well-known/oauth-authorization-server with a valid CA-issued certificate. All endpoint entries (token, authorization, JWKS) use https:// scheme. The issuer field "https://as.example.com" matches the base URL from which the document was fetched — clients can validate this binding and reject documents where the issuer does not match the fetch URL. RFC 8414 §3 requirements satisfied.

---

## Pair[CB4-3.4-727]

### A
OIDC Discovery issuer field does not match the discovery document URL base (OpenID Connect Discovery §4.3):

A client fetches the OIDC discovery document from https://idp.example.com/.well-known/openid-configuration. The returned document contains: issuer: "https://auth.internal.example.com". The team explains: "We have two hostnames — the external-facing hostname is idp.example.com and the internal infrastructure name is auth.internal.example.com. The issuer is set to the internal name because that is what the IdP uses in token iss claims. Both resolve to the same server." The client library performs OpenID Connect Discovery §4.3 validation: it compares the issuer field in the discovery document against the URL used to fetch the document and finds a mismatch.

### B
REPROCESS. OpenID Connect Discovery §4.3 requires: "The issuer value returned MUST be identical to the Issuer URL that was directly used to retrieve the configuration information." The team's explanation of dual hostnames does not satisfy this requirement.

The issuer field in the discovery document is the binding that proves to clients that the document they received originates from the intended provider. If the issuer in the document does not match the URL used to fetch it, a client cannot verify that the document was served by the provider it intended to reach. An attacker who can perform a DNS substitution or SSRF to redirect the discovery request to a different server could serve a discovery document for an attacker-controlled provider — with any issuer value — and the client would have no way to distinguish this from a legitimate mismatch caused by dual-hostname architecture.

The "both resolve to the same server" argument is rejected. DNS resolution at the time of discovery is not a persistent binding — the issuer field in the issued tokens (iss=https://auth.internal.example.com) would not match the discovery URL (https://idp.example.com) used by external clients, creating downstream token validation failures even without an attack. Correct remediation: configure the IdP issuer to https://idp.example.com, matching the externally-reachable discovery URL.

OpenID Connect Discovery §4.3 MUST requirement: violated. REPROCESS.

### Reason
OpenID Connect Discovery §4.3 is an unconditional MUST. The dual-hostname architecture explanation does not satisfy the requirement — it creates both a compliance violation and a downstream token validation inconsistency.

### Phrasing-B
REPROCESS — OIDC Discovery §4.3 issuer mismatch. The dual-hostname explanation is rejected.

The discovery document at https://idp.example.com/.well-known/openid-configuration returns issuer: "https://auth.internal.example.com". OpenID Connect Discovery §4.3 requires the issuer field to be identical to the URL used to retrieve the document. The mismatch means: (1) client libraries that perform §4.3 validation will reject the discovery document; (2) tokens issued with iss=https://auth.internal.example.com will fail iss validation for clients who expect iss to match the external discovery URL; (3) the binding between discovery response and claimed provider identity is broken, enabling SSRF or DNS-substitution attacks to substitute discovery documents undetected.

The argument that both hostnames resolve to the same server addresses DNS correctness, not the §4.3 issuer binding requirement. REPROCESS.

---

## Pair[CB4-3.4-728]

### A
OIDC Discovery issuer correctly matches discovery URL:

A client fetches the OIDC discovery document from https://idp.example.com/.well-known/openid-configuration. The document contains: issuer: "https://idp.example.com"; authorization_endpoint: "https://idp.example.com/oauth/authorize"; token_endpoint: "https://idp.example.com/oauth/token"; jwks_uri: "https://idp.example.com/.well-known/jwks.json". The client library confirms: the issuer field "https://idp.example.com" matches the base URL "https://idp.example.com" used to construct the discovery URL. All tokens issued by this provider carry iss: "https://idp.example.com", which clients validate against the known issuer after discovery.

### B
PASS. OpenID Connect Discovery §4.3 issuer binding requirement satisfied.

The issuer field in the discovery document is identical to the base URL used to fetch the document. Client libraries that implement §4.3 validation will confirm the binding and proceed. Tokens issued with iss="https://idp.example.com" will match the issuer validated during discovery — no downstream iss mismatch at token validation. The substitution attack vector (serving a discovery document for a different provider) is blocked by TLS integrity plus §4.3 issuer validation.

No findings.

### Reason
OpenID Connect Discovery §4.3 satisfied: issuer field exactly matches discovery URL base. Token iss claims will be consistent with the validated issuer value. No binding gaps.

### Phrasing-B
PASS — OIDC Discovery issuer correctly matches the discovery fetch URL.

Discovery at https://idp.example.com/.well-known/openid-configuration returns issuer: "https://idp.example.com". The base URL matches. Client validates the binding per §4.3. All issued tokens carry iss: "https://idp.example.com", consistent with the validated issuer. No issuer mismatch at any step in the client's token validation chain. OpenID Connect Discovery §4.3 satisfied.

---

## Pair[CB4-3.4-729]

### A
Access tokens issued without kid header during active key rollover with two JWKS keys:

An authorization server is performing a key rollover. The JWKS endpoint currently publishes two public keys: key ID "key-2024" (the previous signing key, still valid for token verification) and key ID "key-2025" (the new signing key, now used for issuance). During the rollover window, the AS issues access tokens using the new key but does not include a kid header in the JWT. Resource servers receiving tokens without a kid claim must iterate through all public keys in the JWKS until one validates the signature. Team states: "The validation always succeeds eventually — it just tries both keys."

### B
CONCERN. The absence of a kid header during multi-key operation creates an operational risk below the level of a MUST violation.

RFC 7515 §4.1.4 SHOULD: "The kid (key ID) Header Parameter is a hint indicating which key was used to secure the JWS. When used with a JWK, the kid value is used to match a JWK kid parameter value." The SHOULD-level guidance exists precisely for the multi-key scenario. Absent the kid hint, resource servers must perform sequential or parallel key trials — O(n) attempts for n active keys.

The operational risks: (1) Performance degradation proportional to JWKS size — during aggressive rollover periods with three or more simultaneous active keys (current + previous + rotation-ahead), validation latency increases; (2) Error masking — if the JWKS is misconfigured and contains a key the AS did not use for signing, the RS may not identify the mismatch until all keys have been tried; (3) During rollover, if the RS's cached JWKS has not yet refreshed to include the new key, all tokens issued with the new key will produce sequential failures before the RS refreshes its cache — the kid hint allows the RS to know when a refresh is needed before attempting (kid not found in current cache = refresh JWKS immediately); (4) RFC 7515 §4.1.4 SHOULD not met.

This is not a MUST violation — token validation eventually succeeds during normal operation. CONCERN.

### Reason
RFC 7515 §4.1.4 is SHOULD not MUST. Absent kid during multi-key rollover is an operational risk and a spec deviation, but not a MUST violation. The RS can still validate correctly by trying all keys. CONCERN.

### Phrasing-B
CONCERN — kid header absent during multi-key JWKS rollover. RFC 7515 §4.1.4 SHOULD not satisfied.

Two active JWKS keys (key-2024 and key-2025) with no kid header in issued JWTs. Resource servers must iterate all active keys on every validation attempt. Beyond performance cost, the absence of kid prevents the RS from knowing whether a validation failure indicates a bad token or a stale JWKS cache — the kid hint serves as a cache invalidation signal: if kid is present in the token but not in the cached JWKS, the RS knows to re-fetch before concluding the token is invalid. Without kid, the RS must re-fetch the JWKS on every failure regardless of whether the cause is a stale cache or a genuinely invalid token. RFC 7515 §4.1.4 SHOULD violation. CONCERN.

---

## Pair[CB4-3.4-730]

### A
Access tokens issued with kid header matching the active signing key:

The same authorization server with two active JWKS keys (key-2024 and key-2025) is now configured to include the kid header in all issued JWTs. Each access token carries kid: "key-2025" in its JOSE header, matching the key ID of the current signing key in the JWKS. Resource servers receive a token, read the kid value "key-2025", look up that specific key in their JWKS cache, and validate the signature with that key only. If the kid is not present in the cached JWKS, the RS re-fetches the JWKS endpoint once and retries before failing.

### B
PASS. RFC 7515 §4.1.4 SHOULD satisfied. kid-directed key lookup implemented correctly.

The kid header in each JWT directly identifies the signing key. Resource servers perform a single targeted key lookup (O(1) instead of O(n)) and validate with the identified key. During rollover, if a token arrives with kid="key-2025" and the RS cache still only contains key-2024, the kid mismatch triggers a targeted JWKS re-fetch — the RS fetches the updated JWKS, finds key-2025, and validates. This eliminates the ambiguity between stale-cache failures and invalid tokens.

The SHOULD guidance in RFC 7515 §4.1.4 is satisfied. Key selection is deterministic, efficient, and cache-refresh-aware. No findings.

### Reason
kid header in every JWT satisfies RFC 7515 §4.1.4 SHOULD. Single-key lookup is both efficient and semantically unambiguous. Cache-refresh trigger on kid mismatch eliminates a class of operational failures.

### Phrasing-B
PASS — kid header present in all issued tokens; RS performs targeted key lookup per RFC 7515 §4.1.4.

Each JWT header contains kid matching the active signing key in the JWKS. RS reads kid, performs a direct lookup in its JWKS cache, validates with the matched key. No sequential key iteration required. kid-not-found in cache triggers one JWKS re-fetch — deterministic response to rollover, not an ambiguous signature failure. RFC 7515 §4.1.4 SHOULD satisfied. PASS.

---

## Pair[CB4-3.4-731]

### A
Token endpoint accepts unauthenticated requests from registered confidential clients (RFC 6749 §2.3 MUST authenticate):

An authorization server has registered clients as confidential — each client was issued a client_secret during registration. The token endpoint is configured to accept requests with or without client authentication: if a request includes client_id and client_secret in the POST body, the credentials are validated; if a request includes only client_id and no client_secret, the request proceeds and a token is issued without authentication. The server documentation describes this as "flexible authentication — clients that know their secret provide it; clients that don't can still use the endpoint." A penetration tester demonstrates that using only a known client_id (obtainable from any intercepted authorization code flow) yields a valid access token.

### B
REPROCESS. RFC 6749 §2.3 states: "Confidential clients or other clients issued client credentials MUST authenticate with the authorization server." The token endpoint's acceptance of unauthenticated requests for registered confidential clients violates this MUST requirement unconditionally.

The "flexible authentication" description inverts the security model. A confidential client is classified as confidential precisely because it is capable of maintaining credential secrecy. The client_secret is the authorization server's mechanism for verifying that a token request originates from the registered client, not from an unauthorized party who has learned the client_id through observation of authorization code flows, logs, or public documentation.

The penetration tester's demonstration confirms the practical impact: any party who knows a client_id can obtain tokens on behalf of that client without possessing the client_secret. This enables: impersonation of any registered confidential client; token issuance in client flows that have access to privileged scopes (service accounts, admin APIs); bypass of client-level authorization controls.

RFC 6749 §2.3 MUST requirement: violated. REPROCESS.

### Reason
RFC 6749 §2.3 is an unconditional MUST for confidential clients. Accepting unauthenticated requests — even as a fallback — removes the client identity assurance that the token endpoint is required to enforce. The penetration test outcome confirms exploitability.

### Phrasing-B
REPROCESS — RFC 6749 §2.3 MUST authenticate violated. Confidential clients must not be permitted to skip authentication.

The token endpoint accepts POST requests with client_id only, issuing tokens without client_secret validation. RFC 6749 §2.3 requires that confidential clients — those issued credentials at registration — MUST authenticate with the authorization server at the token endpoint. The conditional model ("clients that know their secret provide it; others proceed without") nullifies the confidential client classification entirely. A client_id is not a secret — it appears in redirect URIs, authorization request logs, and is observable in the OAuth authorization response. Any party who observes a client_id can impersonate the registered client and obtain tokens. MUST violated. REPROCESS.

---

## Pair[CB4-3.4-732]

### A
Resource server accepts any valid access token without validating the aud claim against its own registered identifier:

An authorization system has three resource servers: api.example.com (issues tokens with aud=["api.example.com"]), reports.example.com (aud=["reports.example.com"]), and admin.example.com (aud=["admin.example.com"]). Each resource server validates the JWT signature and expiry time on incoming Bearer tokens but does not check the aud claim. A user who has a valid access token for api.example.com presents it to admin.example.com — the token passes signature and expiry validation, and admin.example.com processes the request as authenticated. The team argues: "All three RSs trust the same issuer. A valid token from our own AS proves the user is authenticated, regardless of which RS it was meant for."

### B
REPROCESS. The absence of aud validation at the resource server allows tokens issued for one resource server to be accepted by any other resource server in the system. RFC 9068 §4 (JWT Profile for OAuth 2.0 Access Tokens) requires the RS to validate the aud claim; RFC 8725 §3.9 requires audience validation as a core JWT validation step.

The team's argument conflates authentication (verifying who issued the token and who the subject is) with authorization (verifying that this token was issued for this resource server). A valid token from the AS proves: the AS issued it, the user authenticated, and the access was granted with specific scopes. It does not prove: this token was intended for this resource server. The aud claim is the binding between the token and its intended recipient(s).

The practical attack: a user with access to api.example.com (low-privilege RS) presents their token to admin.example.com (high-privilege RS). Without aud validation, admin.example.com cannot distinguish this token from a token legitimately issued for admin.example.com. The attacker does not need to forge a token — they use a legitimately issued token at an unintended recipient. This is an audience confusion attack, a form of token misuse that aud validation is specifically designed to prevent.

The "same issuer = trusted" argument does not address the per-RS scope of authorization. The issuer establishes that the AS signed the token; the aud establishes that this RS is an intended recipient. Both validations are required. REPROCESS.

### Reason
RFC 9068 §4 requires aud validation at the RS. RFC 8725 §3.9 requires it as a core JWT step. The audience confusion attack — using a legitimately-issued token at an unintended RS — is the specific threat model aud validation addresses. The "same issuer = trusted" argument conflates token authenticity with token authorization scope.

### Phrasing-B
REPROCESS — RS aud validation absent. Audience confusion attack viable.

Three resource servers share a common issuer but each has a distinct registered audience identifier. Without aud validation, any valid access token from the AS — regardless of which RS it was issued for — passes validation at any RS. A token with aud=["api.example.com"] is accepted by admin.example.com because the signature is valid and the expiry is current. The aud claim goes unread.

RFC 9068 §4 requires the RS to verify that its own identifier appears in the aud claim before processing the request. RFC 8725 §3.9 lists audience validation as a mandatory JWT validation step. The issuer validates token authenticity (the AS signed it). The aud validates authorization scope (this RS is an intended recipient). Both are required. Without aud validation, the authorization boundary between resource servers is non-functional — tokens issued with limited scope for one RS can be used at any other RS. REPROCESS.
