# CB4 Stage 3 — Action 3 Extended Pair Set
# Node: Node 8 (Arbiter Units) — ext31
# Sequences: CB4-3.4-803 through CB4-3.4-810
# Instances: 16 (8 base pairs × 2 phrasings each)
# Stratum: A/B/D (new domains: AS PKCE enforcement policy, HS256 multi-party token signing, token endpoint per-client rate limiting, OIDC End Session id_token_hint)
# Running total after this file: 1620 instances (810 base pairs)

---

## Pair[CB4-3.4-803]

### A
Authorization server does not require PKCE for public clients (RFC 9700 §2.1.1):

An authorization server processes authorization code requests from public clients (native mobile apps, SPAs) without requiring a code_challenge parameter. If a public client includes code_challenge, it is validated; if it does not, the authorization code is issued without PKCE. The AS accepts both PKCE and non-PKCE authorization code flows from public clients. Team: "PKCE is available for clients that want it. Requiring it would break backward compatibility with older client versions."

### B
REPROCESS. RFC 9700 §2.1.1 states: "Authorization servers MUST require PKCE for public clients." This is an unconditional MUST. Public clients that do not include a code_challenge must have their authorization requests rejected.

The backward-compatibility argument is rejected. RFC 9700's MUST requirement establishes the current normative standard for OAuth 2.0 security. Legacy clients that do not support PKCE must be updated — backward compatibility with non-PKCE public clients is not a recognized exception in RFC 9700. The security threat that PKCE addresses for public clients — authorization code interception — applies regardless of whether older clients know about PKCE.

The practical impact of accepting non-PKCE public clients: any authorization code intercepted from a public client (from browser navigation logs, phishing redirects, MITM on HTTP redirects, or authorization server logs) can be exchanged for tokens without the code_verifier constraint. PKCE binding is the primary protection for public clients against code theft; making it optional makes it ineffective as a security control.

RFC 9700 §2.1.1 MUST: violated. REPROCESS.

### Reason
RFC 9700 §2.1.1 MUST is unconditional for public clients. Backward compatibility with non-PKCE clients does not constitute an exception. Optional PKCE = no PKCE protection for clients that omit it.

### Phrasing-B
REPROCESS — RFC 9700 §2.1.1 MUST require PKCE for public clients violated. Backward-compatibility exception does not exist in RFC 9700.

AS accepts public client authorization code requests without code_challenge. RFC 9700 §2.1.1 MUST: authorization servers must require PKCE for public clients. Optional PKCE is equivalent to no PKCE for clients that skip it — code interception attacks remain viable for all non-PKCE authorization code flows. Backward-compatibility argument: updating older clients to support PKCE S256 is required by the MUST; compatibility timelines must accommodate this. REPROCESS.

---

## Pair[CB4-3.4-804]

### A
Authorization server enforces PKCE for all public client authorization code requests:

The authorization server rejects authorization code requests from public clients that do not include a code_challenge parameter with a valid code_challenge_method=S256. The error response returns 400 (error: invalid_request) with error_description indicating the required parameter is missing. All registered public clients have been updated to send code_challenge_method=S256 and generate a code_verifier per RFC 7636 §4.1 (43-128 chars, high-entropy CSPRNG).

### B
PASS. RFC 9700 §2.1.1 MUST require PKCE for public clients satisfied.

The AS enforces the PKCE requirement at the authorization endpoint — public client requests without code_challenge are rejected before an authorization code is issued. This ensures PKCE cannot be bypassed by any client, regardless of client library version or configuration. The 400 error response provides a clear signal to clients that need updating.

All registered public clients send code_challenge_method=S256. RFC 7636 §4.1 code_verifier format requirements satisfied (high-entropy, 43-128 chars). No non-PKCE authorization codes can be issued for public clients. Code interception attack vector eliminated at the AS enforcement level. No findings.

### Reason
RFC 9700 §2.1.1 MUST satisfied — AS rejects non-PKCE public client requests. Client-side S256 compliance confirmed. PASS.

### Phrasing-B
PASS — AS enforces PKCE S256 for all public client authorization code requests per RFC 9700 §2.1.1.

AS rejects public client requests without code_challenge (400 invalid_request). RFC 9700 §2.1.1 MUST: satisfied. All registered public clients send code_challenge_method=S256 with RFC 7636 §4.1-compliant code_verifier. Code interception attack requires possession of both the code AND the verifier — separate delivery paths; interception of code alone is not sufficient. PASS.

---

## Pair[CB4-3.4-805]

### A
JWT access tokens signed with HS256 in a multi-party token system:

An authorization server issues JWT access tokens signed with HS256. The same HMAC secret key used for signing must be distributed to all resource servers that validate the tokens. The system has four resource servers operated by different teams. All four resource servers are configured with the same shared HS256 secret. Team: "HS256 is simpler to configure than RSA key pairs — no key generation, no certificate management. It works well in our environment."

### B
REPROCESS. RFC 8725 §3.6 (JWT Best Practices) recommends asymmetric keys for multi-party token systems where the signer and verifier are different parties; V9.1.1 enforces correct algorithm use. The use of HS256 in a system where four separate teams hold the signing secret creates a catastrophic shared-secret attack surface.

The core problem: in HS256, the key used to sign tokens is identical to the key used to verify them. In a multi-party system, distributing the signing secret to all resource servers means that ANY party holding the shared secret can forge tokens. If any of the four resource server deployments is compromised — by an attacker who gains access to the server's configuration, secrets manager, environment variables, or code repository — the attacker obtains the HMAC secret and can produce arbitrary signed JWTs that are accepted by all four resource servers.

In contrast, RS256 uses an asymmetric key pair: the AS holds the private key (signs tokens) and distributes only the public key to resource servers (verification only). Compromise of a resource server yields only the public key — the attacker cannot forge tokens because they cannot produce RSA signatures without the private key.

"Simpler to configure" — accepted as accurate. Operational simplicity does not satisfy the security requirement when the simplified configuration creates a systemic forgery attack surface. RFC 8725 §3.6 addresses this exact scenario. REPROCESS.

### Reason
RFC 8725 §3.6 explicitly addresses multi-party token systems: asymmetric keys required when the signer and verifiers are separate parties. HS256 symmetric key distribution to four resource servers means compromise of any RS yields the ability to forge all tokens.

### Phrasing-B
REPROCESS — HS256 symmetric secret distributed to four resource server teams in a multi-party token system.

HS256: signing key = verification key. Distributing the signing secret to resource servers means each RS has token forgery capability. Four RSes × four opportunities for secret compromise = four attack surfaces for universal token forgery. Any RS compromise → all tokens forgeable across all RSes.

RS256 (asymmetric): AS holds private key (not distributed); RSes hold public key (verification only). RS compromise yields public key — cannot produce valid RS256 signatures. RFC 8725 §3.6 guidance for multi-party JWT systems: use asymmetric keys. V9.1.1 correct algorithm enforcement: RS256/PS256/ES256 for multi-party. REPROCESS.

---

## Pair[CB4-3.4-806]

### A
JWT access tokens signed with RS256 in a multi-party token system:

The same multi-party system is reconfigured: the authorization server signs JWT access tokens with an RSA private key (RS256). The public key is published at the AS JWKS endpoint (https://). All four resource servers are configured to fetch the JWKS from the AS and use only the public key for signature verification. The private key is stored in the AS's secrets manager; no resource server has access to it.

### B
PASS. RS256 asymmetric signing correctly applied to a multi-party token system. RFC 8725 §3.6 guidance satisfied.

The asymmetric key architecture correctly separates signing capability from verification capability. The private key is held exclusively by the AS — resource servers cannot produce signatures even if compromised. Each RS can verify tokens but cannot forge them. Compromise of any RS yields only the public key, which is already publicly available from the JWKS endpoint.

JWKS delivery over HTTPS ensures public key integrity is protected — resource servers receive authentic key material. RS algorithm allowlist (if configured per V9.1.1 to accept only RS256) prevents algorithm confusion attacks. No findings.

### Reason
RS256 separates signing (AS private key) from verification (RS public key). Compromise of any RS does not enable token forgery. RFC 8725 §3.6 multi-party guidance satisfied.

### Phrasing-B
PASS — RS256 with private key held exclusively by AS; RSes use public key for verification only.

AS: RSA private key in secrets manager; signs JWTs with RS256. RSes: fetch public key from HTTPS JWKS; verify only. No RS team has signing capability. RS compromise → public key obtained (already public) → cannot forge tokens. RFC 8725 §3.6 multi-party guidance satisfied. V9.1.1 RS256 algorithm allowlist: meets requirement. PASS.

---

## Pair[CB4-3.4-807]

### A
Token endpoint rate limiting based on source IP only — no per-client-id throttling:

An authorization server's token endpoint has IP-based rate limiting: each source IP is limited to 100 requests per minute. However, there is no per-client-id throttling. An attacker using a botnet (100 different IP addresses) can attempt 100 × 100 = 10,000 client_secret guesses per minute against a known client_id, each IP staying within the rate limit. Team: "IP rate limiting protects against simple attacks. We can't rate-limit by client_id because legitimate high-volume services may make many token requests."

### B
CONCERN. IP-based rate limiting provides meaningful protection against single-source volume attacks but does not address distributed credential stuffing or brute force against specific client credentials. Per-client-id throttling is the defense mechanism appropriate for client credential attacks.

The team's argument about high-volume legitimate services is partially valid but the concern is addressable: per-client-id throttling can be set with a higher limit for known high-volume clients (e.g., based on client registration). The argument demonstrates awareness of the design challenge but does not satisfy the need for client-level credential protection.

The gap: a botnet with a sufficient number of IPs can make effectively unlimited client_secret guesses against a specific client_id while staying within per-IP limits. The token endpoint requires authentication of clients — client_secret brute force is a credential attack that IP rate limiting alone does not prevent.

However, this is not a MUST violation — no RFC or ASVS section specifies per-client-id rate limiting as a MUST requirement for token endpoints (ASVS V6.2 specifies brute force protection requirements but without mandating the specific mechanism). The absence of per-client throttling is a defense-in-depth gap. CONCERN.

### Reason
IP-only rate limiting is a real defense gap against distributed credential attacks, but no applicable standard mandates per-client-id throttling at the token endpoint as a MUST requirement. CONCERN.

### Phrasing-B
CONCERN — IP-based rate limiting only; per-client-id throttling absent. Distributed brute-force viable.

IP rate limiting: protects against single-source volume attacks. Per-client-id throttling: absent. A botnet with N IPs can attempt N × (per-IP limit) client_secret guesses per minute per target client_id — effectively unlimited with sufficient IP diversity.

Team argument about high-volume legitimate services: valid design challenge; per-client throttling should use higher limits for known high-volume registered clients. No RFC or ASVS MUST requires a specific rate limiting mechanism for token endpoint credential attacks. This is a defense-in-depth gap. CONCERN.

---

## Pair[CB4-3.4-808]

### A
Token endpoint per-client-id rate limiting with graduated response:

The authorization server applies per-client-id rate limiting to token endpoint requests: each client_id is throttled to a maximum of 30 failed authentication attempts per 15-minute window (sliding window counter in Redis); on exceeding the threshold, subsequent requests return 429 with a Retry-After header; after 5 consecutive windows of threshold breaches, the client_secret is automatically flagged for rotation and the client owner is notified. IP rate limiting also applies as an additional layer.

### B
PASS. Per-client-id credential protection implemented at the appropriate granularity with graduated response.

Per-client throttling: 30 attempts per 15-minute window limits brute-force viability against any specific client_id regardless of the number of source IPs used. A botnet attack cannot bypass the per-client counter. The 429 + Retry-After response correctly signals to legitimate clients to back off.

Automated escalation: consecutive-window threshold breaches trigger secret rotation flagging — this prevents a successful brute force from remaining useful indefinitely even if the attacker persists across throttled windows. Notification provides the client owner visibility into credential attack attempts.

Combined IP + per-client rate limiting provides layered defense. No single-vector attack (single IP or distributed multi-IP) can bypass both layers simultaneously. No findings.

### Reason
Per-client-id rate limiting directly addresses the client credential brute-force attack vector. Graduated response (429 → rotation flag → notification) provides both immediate and sustained protection. PASS.

### Phrasing-B
PASS — per-client-id rate limiting with graduated response correctly protects against credential brute-force.

30-attempt/15-min per client_id in Redis. Consistent threshold behavior regardless of source IP count — botnet-distributed attacks cannot bypass. 429 + Retry-After on threshold breach. Consecutive-window escalation → secret rotation flag + owner notification. Layered with IP rate limiting. No single attack vector bypasses both layers. PASS.

---

## Pair[CB4-3.4-809]

### A
OIDC End Session endpoint accepts logout requests without id_token_hint validation:

An OIDC-enabled application's end session endpoint (per OIDC Core / RP-Initiated Logout specification) accepts requests with only post_logout_redirect_uri — no id_token_hint required. Any caller who knows a registered post_logout_redirect_uri can trigger session termination for any active user session. The team states: "Logout is not a security-sensitive action — terminating a session is harmless. We don't require id_token_hint because it complicates the client integration."

### B
CONCERN. The OIDC End Session (RP-Initiated Logout) specification lists id_token_hint as RECOMMENDED — not MUST. The absence of id_token_hint is a SHOULD/RECOMMENDED-level gap, not a MUST violation.

However, the team's characterization of logout as "not security-sensitive" is rejected. Session forcing attacks — where an attacker causes a logged-in user's session to be terminated unexpectedly — are a recognized threat. A user performing a critical operation (completing a purchase, submitting a form, conducting a banking transaction) whose session is forced to expire mid-operation may lose work or be presented with unexpected re-authentication demands. In higher-security contexts, repeated forced logout can be used as a denial-of-service mechanism against specific users or as a precursor to a phishing attack (force logout → present fake login page).

Without id_token_hint, the AS cannot verify that the logout request originated from the holder of a valid session token — any party that knows a registered redirect URI can trigger logout. The id_token_hint provides binding between the logout request and an actual current session.

RECOMMENDED level: CONCERN (not REPROCESS).

### Reason
OIDC RP-Initiated Logout id_token_hint is RECOMMENDED not MUST. Absent id_token_hint is a defense-in-depth gap. Session forcing is a real (if lower-severity) attack. CONCERN.

### Phrasing-B
CONCERN — OIDC End Session id_token_hint absent. RECOMMENDED not MUST; session forcing attack vector present.

OIDC RP-Initiated Logout: id_token_hint RECOMMENDED. Without it, any caller with a registered post_logout_redirect_uri can force session termination for any active user. "Harmless" characterization rejected — session forcing disrupts active operations, can function as DoS against specific users, and enables precursor attacks (force logout → fake login page). RECOMMENDED not MUST: CONCERN.

---

## Pair[CB4-3.4-810]

### A
OIDC End Session endpoint validates id_token_hint; binds logout to token holder:

The OIDC End Session endpoint requires id_token_hint. The AS validates: (1) id_token_hint is present; (2) the ID token's signature is valid; (3) the sub claim matches an active session; (4) the aud claim matches the requesting client_id; (5) the hint is not expired. If validation fails, the logout request is rejected with an appropriate error. After validation, only the session corresponding to the ID token hint's sub is terminated.

### B
PASS. OIDC RP-Initiated Logout with id_token_hint correctly required and validated.

id_token_hint validation binds the logout request to the holder of a valid ID token. The five-step validation chain ensures: the request came from a party who recently held a valid ID token for the correct client; the session being terminated belongs to the user identified in the token; expired tokens (from prior sessions) cannot be used to terminate current sessions. Session forcing attacks are blocked — an attacker without a current valid ID token cannot trigger logout for another user.

The RECOMMENDED guidance from OIDC RP-Initiated Logout is implemented at a stricter level than RECOMMENDED — the AS requires (not merely accepts) id_token_hint. Elevated implementation above the RECOMMENDED floor. No findings.

### Reason
id_token_hint required and fully validated with five-step chain. Session forcing attack blocked. Elevated above RECOMMENDED.

### Phrasing-B
PASS — OIDC End Session id_token_hint required and validated. Session forcing attack blocked.

End session requires id_token_hint. Validates: signature, sub → active session match, aud → requesting client_id, exp → not expired. Logout terminates only the session for the validated sub. Session forcing blocked — attacker without valid current ID token cannot trigger logout. Implemented above RECOMMENDED floor. PASS.
