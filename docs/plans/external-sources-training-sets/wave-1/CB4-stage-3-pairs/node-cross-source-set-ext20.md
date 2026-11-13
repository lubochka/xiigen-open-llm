# CB4 Stage 3 — Action 3 Extended Pair Set
# Node: Cross-source combinations (ext20 — ext27 cross-source)
# Sequences: CB4-3.4-715 through CB4-3.4-724
# Instances: 20 (10 base pairs × 2 phrasings each)
# Stratum: B/C (cross-source pattern combinations)
# Running total after this file: 1448 instances (724 base pairs)

---

## Pair[CB4-3.4-715]

### A
Cross-source input — ext27 pattern combination (ID token API auth + RS algorithm allowlist absent):

An OIDC-enabled system is under review. The resource server accepts Bearer tokens at protected API endpoints. Inspection reveals: (1) the client presents the OIDC ID token (typ=JWT, aud=client_id) as the Bearer credential in API requests; the resource server validates the signature and expiry but does not check the aud claim against its own registered identifier; (2) the resource server's algorithm validation code accepts any algorithm field in the JWT header — RS256, HS256, and algorithm="none" all pass through to signature verification without rejection. Development team states: "The ID token is signed by our trusted IdP, and we verify the signature — both problems you describe are caught by the same validation layer."

### B
REPROCESS. Two independent MUST-level violations identified; the development team's consolidation argument is rejected.

Finding 1 — V10.5.3 MUST NOT (OIDC Core §2 token role violation): The ID token is used as an API authorization credential at the resource server. The ID token carries aud=client_id, not the resource server's registered identifier. It contains no scope claims. It cannot be revoked via RFC 7009 — the logout endpoint does not invalidate ID tokens held by the RS. The ID token is an authentication assertion for the client; it is not an authorization credential for the resource server. Signature validation by the RS does not transform the token's role. MUST NOT violated.

Finding 2 — V9.1.1 MUST (RFC 8725 §3.1 algorithm allowlist): The resource server performs no algorithm allowlist check before passing the token to the signature verification routine. Two attacks are viable: (a) HS256 confusion — an attacker obtains the IdP's RSA public key (available from the public JWKS endpoint), crafts a token signed with HS256 using the public key bytes as the HMAC secret; the RS treats the public key as an HMAC shared secret and the signature validates; (b) algorithm=none — an attacker strips the signature and sets alg=none; the RS accepts the unsigned token with valid claims as authentic. The team's argument that signature validation catches both — invalid. Validation is the attack surface, not the defense, when algorithm enforcement is absent. MUST violated.

The team's consolidation argument — that both gaps are "caught by the same validation layer" — is rejected. Signature validation is the compromised layer in both attacks, not the safeguard.

### Reason
Two independent REPROCESS triggers (V10.5.3 MUST NOT; V9.1.1 MUST) with no mitigating controls for either. The team's argument attempts to collapse two distinct MUST requirements into one code path, which does not reduce the finding count.

### Phrasing-B
REPROCESS — two MUST violations confirmed. The argument that signature validation covers both findings is rejected.

First violation: V10.5.3 MUST NOT — the ID token (aud=client_id, no scope claims, no RFC 7009 pathway) is presented as a Bearer API credential at the resource server. The resource server is not the intended audience of the ID token and cannot enforce scope or revocation through it.

Second violation: V9.1.1 MUST — algorithm allowlist absent. HS256 confusion attack vector: the IdP's RSA public key is publicly available; an attacker uses it as an HMAC secret to produce a validating HS256 signature. Algorithm-none attack vector: signature stripped, alg=none set, RS accepts unsigned claims. Both vectors bypass the signature verification routine entirely because that routine is invoked without first rejecting the algorithm.

The consolidation argument — "same validation layer" — fails because the validation layer itself is the exploitable surface in both attacks.

---

## Pair[CB4-3.4-716]

### A
Cross-source input — ext27 pattern combination (correct access token + RS allowlist correct):

An OIDC-enabled API system presents the following controls for review: (1) after login, the SPA receives an access token (typ=at+JWT, aud=api.example.com) and an ID token separately; the SPA uses the access token as the Bearer credential for all API calls; the resource server validates aud against its registered identifier "api.example.com"; (2) the resource server's JWT validation code checks the algorithm allowlist as its first step — before any claim or signature evaluation — and rejects any token whose alg header is not in ["RS256", "PS256"]; HS256 and algorithm="none" both result in immediate 401 responses. Review team confirms: scope claims present in access token; RS enforces per-endpoint scope checks.

### B
PASS. Both token role separation and algorithm allowlist controls are correctly implemented.

Access token role: typ=at+JWT signals authorization credential; aud=api.example.com matches the RS registered identifier; scope claims present and enforced per endpoint. The ID token is separate and not used for API authorization. OIDC Core §2 role boundary maintained. V10.5.3 satisfied.

RS algorithm allowlist: the allowlist check executes first in the validation chain, before signature verification and before claim evaluation. Tokens with alg=RS256 or alg=PS256 proceed to verification; all other algorithm values produce immediate rejection. HS256 confusion attack vector eliminated — the public key is not tested as an HMAC secret because the alg=HS256 header is rejected before that step. Algorithm-none attack eliminated — alg=none rejected before the verification routine is invoked. V9.1.1 and RFC 8725 §3.1 satisfied.

No findings.

### Reason
Both controls address distinct MUST requirements and are implemented in the correct order. Algorithm allowlist first-in-chain ordering is the critical structural element that makes PASS valid.

### Phrasing-B
PASS — correct token architecture and algorithm enforcement confirmed.

The access token carries aud=api.example.com (matching the RS registered identifier) and scope claims. The ID token is consumed by the client for display only. Resource server never receives the ID token as a credential. V10.5.3 boundary maintained.

Algorithm validation: allowlist ["RS256", "PS256"] is the first check executed; no other code path runs before it. Tokens presenting any other algorithm value — including HS256, PS256-MD5, none — are rejected with 401 before signature evaluation begins. The ordering eliminates both known bypass vectors (confusion attack; unsigned-token attack). V9.1.1 MUST satisfied.

---

## Pair[CB4-3.4-717]

### A
Cross-source input — ext27 pattern combination (refresh without absolute max + revocation no cascade):

A token lifecycle review covers two areas: (1) refresh tokens use a sliding window via per-use rotation and family detection — if a replay is detected (token already used), all tokens in the family are revoked; there is no configured absolute maximum lifetime; a refresh token in active use can theoretically remain valid indefinitely through continuous rotation; (2) when a refresh token is revoked (manually or via family revocation), the authorization server marks the refresh token invalid but does not invalidate outstanding access tokens; access tokens have a 45-minute TTL and will continue to pass resource server validation until they expire naturally. Team argues: "Active rotation proves ongoing legitimate access, and access tokens expire on their own within the hour."

### B
CONCERN. Two SHOULD/MAY-level gaps identified. No REPROCESS trigger present. N CONCERNs do not escalate to REPROCESS — graduation rule applies.

Finding 1 — BCP §4.14 SHOULD (refresh absolute maximum lifetime): Refresh token rotation + family revocation is the correct foundational control. However, BCP §4.14 SHOULD impose an absolute maximum lifetime independent of rotation activity. The team's argument — "active rotation proves ongoing legitimate consent" — is rejected. An attacker who obtains a refresh token before the legitimate user is next active will rotate the token first; they now hold the current token and continue rotating indefinitely while the legitimate user's subsequent attempt triggers family revocation (which may not help if the attacker acts faster on the next cycle). Absolute max lifetime caps the attacker's window regardless of rotation behavior. SHOULD-level gap: CONCERN.

Finding 2 — RFC 7009 §2 MAY (revocation cascade to access tokens): RFC 7009 §2 defines cascade revocation as a MAY, not a MUST. The absence of cascade is not a MUST violation. The 45-minute post-revocation window is a real security gap — tokens remain valid for up to 45 minutes after the refresh token (and its associated family) is revoked — but the RFC does not mandate immediate cascade. CONCERN.

Combined verdict: CONCERN. Two CONCERN findings; graduation rule (N CONCERNs ≠ REPROCESS) applies.

### Reason
Both gaps are SHOULD/MAY-level — neither triggers REPROCESS. The graduation rule explicitly states that accumulating CONCERNs does not produce a REPROCESS. The team's "active rotation = consent" argument is rejected but the rejection produces CONCERN, not escalation.

### Phrasing-B
CONCERN — two SHOULD/MAY-level gaps. Graduation rule applied: N CONCERNs do not escalate to REPROCESS.

Gap 1: BCP §4.14 SHOULD absolute maximum lifetime — absent. Refresh rotation and family revocation are correct defensive mechanisms but they are not substitutes for an absolute cap. A stolen refresh token used by an attacker first allows indefinite access through continued rotation; the absolute max cap is the only control that terminates the window without requiring detection of the theft. CONCERN.

Gap 2: RFC 7009 §2 MAY cascade revocation — not implemented. Revoked refresh token leaves up to 45 minutes of valid access token lifetime. The RFC permits this; it is a hygiene gap, not a MUST violation. CONCERN.

No REPROCESS triggers. Overall verdict: CONCERN.

---

## Pair[CB4-3.4-718]

### A
Cross-source input — ext27 pattern combination (full refresh lifecycle + access token blocklist):

Token management controls submitted for review: (1) refresh tokens carry a 90-day absolute maximum lifetime field (abs_exp), regardless of rotation activity; additionally, a 30-day idle timeout revokes the token if no exchange occurs within 30 days; per-use rotation is applied on every exchange; family detection flags any re-use of a superseded token and triggers revocation of all tokens in that family; access token TTL is 15 minutes; (2) the authorization server maintains an in-memory blocklist (Redis, TTL-scoped to the remaining token lifetime) of revoked access token JTIs; when a refresh token is revoked — whether by idle timeout, absolute expiry, family revocation, or manual action — the authorization server immediately adds all associated outstanding access token JTIs to the blocklist; the resource server consults the blocklist on every request.

### B
PASS. All refresh token lifecycle and revocation cascade controls are correctly implemented.

Refresh lifecycle: 90-day absolute max eliminates indefinite-rotation attack window. 30-day idle timeout terminates abandoned credential paths. Per-use rotation + family revocation on replay correctly handles stolen-token-used-first race condition. 15-minute access token TTL minimizes standalone access token exposure. BCP §4.14 controls satisfied.

Revocation cascade: blocklist implementation with TTL-scoped entries (entries expire when the original token would have expired, preventing unbounded growth) provides immediate cascade on revocation events. The resource server checks the JTI blocklist on every request — revoked access tokens are rejected within milliseconds of the revocation event. RFC 7009 §2 MAY satisfied (implemented as stronger than MAY requires). The 15-minute TTL means blocklist entries age out quickly regardless.

No findings.

### Reason
All four BCP §4.14 SHOULD controls present (absolute max, idle timeout, rotation, family revocation). Cascade revocation implemented beyond RFC 7009 §2 MAY level. Access token TTL minimizes the value of any residual window.

### Phrasing-B
PASS — complete refresh token lifecycle and revocation cascade confirmed.

Refresh token controls: absolute 90-day cap (abs_exp claim, enforced server-side regardless of rotation), 30-day idle revocation, per-use rotation, family-level revocation on replay detection. BCP §4.14 multi-layer defense implemented.

Access token cascade: Redis JTI blocklist, entries TTL-matched to remaining access token lifetime. All revocation events (idle expiry, absolute expiry, manual, family revocation) immediately populate the blocklist. RS blocklist check on every request. The combination of 15-minute TTL and JTI blocklist means the maximum post-revocation window is effectively zero for detected revocation events. RFC 7009 §2 cascade satisfied at implementation depth beyond the MAY requirement.

---

## Pair[CB4-3.4-719]

### A
Cross-source input — ext27 + PO.1 escalation (ID token API auth traceable to training gap):

A post-incident review covers an authorization flaw discovered in production. The resource server was accepting OIDC ID tokens (aud=client_id) as API authorization credentials; attackers forged ID tokens by obtaining a client_id and using a leaked historical IdP signing key. Incident review includes: (1) the resource server code was written by the lead backend engineer, who was designated the OIDC integration owner; (2) a SSDF PO.1 audit reveals the organization has no formal security training program — engineers are expected to learn security requirements "on the job through documentation reading"; (3) the lead engineer states during review: "I did not know that ID tokens were not supposed to be used for API authorization — I thought any signed JWT from the IdP could serve as a Bearer credential." The engineer confirms they had not read OIDC Core §2 or V10.5.3 prior to the implementation. The training gap is directly traceable to the production MUST violation.

### B
REPROCESS. V10.5.3 MUST NOT violated in production, with the violation directly traceable to an absent SSDF PO.1 security training program.

Primary finding — V10.5.3 MUST NOT: ID token used as API authorization credential at resource server. The specific failure path: ID token aud=client_id does not match the RS identifier; no scopes present; no RFC 7009 revocation pathway for the RS. This is a MUST NOT violation with confirmed production exploitation.

PO.1 escalation: SSDF PO.1 requires the organization to establish and maintain a security training program for all personnel with security-relevant responsibilities. The OIDC integration owner was assigned security-critical responsibilities (OAuth/OIDC implementation) without receiving training on the authorization model. The engineer's own statement confirms the knowledge gap directly caused the V10.5.3 violation. This is the escalation condition: a PO.1 gap that is not merely a process concern but is the proximate cause of a production MUST violation. Escalates from CONCERN (PO.1 alone) to REPROCESS.

REPROCESS issued on two grounds: (1) V10.5.3 MUST NOT directly violated in production; (2) PO.1 absence traceable to production MUST violation — escalation rule applies.

### Reason
The escalation rule: PO.1 absence alone = CONCERN; PO.1 absence + traceable production MUST violation = REPROCESS. The engineer's confirmation statement establishes the causal chain explicitly.

### Phrasing-B
REPROCESS — V10.5.3 MUST NOT violated in production; SSDF PO.1 absence confirmed as proximate cause.

V10.5.3: ID token (aud=client_id, no scopes, no RFC 7009 revocation pathway) used as API authorization credential at the resource server. The RS accepted it as a valid Bearer token. Production exploitation confirmed.

PO.1 escalation: the engineer responsible for OIDC implementation was not provided security training on OIDC authorization model. The engineer explicitly stated they were unaware that ID tokens are not API authorization credentials. The training gap directly caused the MUST NOT violation. SSDF PO.1 compliance requires formal training for personnel with security-critical responsibilities — "learn from documentation on your own" does not satisfy PO.1. The causal chain (no training → engineer unaware of V10.5.3 → V10.5.3 violated in production) triggers the escalation from CONCERN to REPROCESS.

---

## Pair[CB4-3.4-720]

### A
Cross-source input — ext27 + ext26 combination (RS algorithm allowlist absent + grant type not bounded):

A joint security review of two authorization server components finds: (1) the resource server's JWT verification accepts any algorithm value in the header — RS256, HS256, PS256, ES256, and algorithm="none" are all passed to the verification step without an allowlist check; the IdP's RSA public key is published at a well-known JWKS endpoint; (2) the authorization server does not validate the grant_type in token requests against the client's registered grant types — a client registered for authorization_code flow only can successfully submit a client_credentials grant request; the authorization server issues a client_credentials token without checking whether that grant is registered for the requesting client_id. Team states: "Both issues are theoretical — we have not seen either exploited in production."

### B
REPROCESS. Two independent MUST-level violations confirmed. The "not seen in production" argument does not modify MUST obligations — absence of observed exploitation is not a compliance standard.

Finding 1 — V9.1.1 MUST (RFC 8725 §3.1 algorithm allowlist absent): The resource server does not perform an allowlist check before invoking the signature verification routine. HS256 confusion attack: the IdP's RSA public key is publicly available from the JWKS endpoint; an attacker constructs a JWT with alg=HS256 header, signs it using the RSA public key bytes as the HMAC-SHA256 secret; the RS library, expecting RS256, treats the public key as the HMAC secret, and the HMAC verification succeeds. Algorithm-none attack: attacker sets alg=none, removes signature; the RS passes the header through to the verification step which may accept it if the library does not explicitly reject unsigned tokens. MUST violated. REPROCESS trigger.

Finding 2 — RFC 6749 §3.1.1 + CWE-269 (grant type not bounded per client): The authorization server issues tokens without verifying the requested grant_type against the client's registered capabilities. A client registered solely for authorization_code (user-consented delegated access) can invoke client_credentials (machine-to-machine; no user consent required, no authorization code flow). This allows a client to bypass the user consent requirement entirely — obtaining tokens with the same scopes that would require user authorization in the code flow. Privilege escalation via grant type confusion. REPROCESS trigger.

Neither REPROCESS trigger is conditional on production exploitation history. Both are structural MUST violations.

### Reason
Two independent REPROCESS triggers from two distinct RFC/ASVS MUST requirements. The "not exploited yet" argument does not constitute a mitigating control under any of the applicable standards.

### Phrasing-B
REPROCESS — two MUST violations. No mitigation from lack of observed exploitation.

First MUST violation: V9.1.1 algorithm allowlist absent. The RS accepts alg=HS256 — the IdP's RSA public key is publicly known; an attacker using it as an HMAC secret produces a validating signature. The RS also accepts alg=none — an unsigned token with valid claims passes verification. Both attacks are viable because the algorithm field is not screened before the verification code runs. The "theoretical only" characterization is rejected — these are documented, widely replicated attack patterns.

Second MUST violation: RFC 6749 §3.1.1 grant type enforcement absent. The authorization server issues a client_credentials token to a client registered for authorization_code only. The client bypasses user consent and the entire authorization code flow by directly requesting machine credentials. This is a capability authorization failure — client authentication (confirming who the client is) does not confer grant type capabilities (what flows the client is permitted to use). REPROCESS.

---

## Pair[CB4-3.4-721]

### A
Cross-source input — ext26 + ext27 combination (native app custom URI + refresh without absolute max):

A native mobile application and its authorization server are reviewed: (1) the native app uses a custom URI scheme redirect (myapp://callback) registered in the OS; PKCE S256 is enforced for all authorization code requests; no alternative redirect URI registered; (2) refresh tokens rotate on each use and include family detection — replay of a superseded refresh token triggers revocation of all tokens in the family; there is no configured absolute maximum lifetime for refresh tokens; the server documentation describes tokens as "active as long as the user continues to use the app." Team notes that both design choices are intentional and have been in production for two years without incident.

### B
CONCERN. Two CONCERN-level findings identified. No REPROCESS triggers present.

Finding 1 — RFC 8252 §7.1/§7.2 (native app redirect URI): Custom URI scheme (myapp://callback) is the registered redirect target. RFC 8252 §7.1 acknowledges custom URI schemes as an option for native apps but §7.2 RECOMMENDS claimed HTTPS redirect URIs (Universal Links on iOS, App Links on Android) because the OS enforces that only the registered app can handle the HTTPS URI, eliminating the URI hijacking risk that exists with custom schemes. PKCE S256 mitigates the impact of interception (code alone is unusable without the verifier), but the redirect URI itself remains hijackable by a malicious app claiming the same custom scheme. The RECOMMENDS status makes this a CONCERN rather than a REPROCESS. CONCERN.

Finding 2 — BCP §4.14 SHOULD (refresh absolute max lifetime): Rotation + family revocation is the correct foundational defense. The absence of an absolute maximum lifetime means a refresh token in continuous active use has no bounded validity period. An attacker who obtains a current refresh token (through any exfiltration path not covered by family detection) can maintain access indefinitely by rotating before the legitimate user. BCP §4.14 SHOULD absolute max: absent. CONCERN.

Production history of no incident does not satisfy SHOULD or RECOMMENDED obligations. Combined: CONCERN.

### Reason
Both RFC 8252 §7.2 RECOMMENDS and BCP §4.14 SHOULD are below MUST/MUST NOT level. Two CONCERNs remain CONCERN per graduation rule. Two years without incident is not a compliance argument.

### Phrasing-B
CONCERN — two SHOULD/RECOMMENDED-level gaps. No escalation to REPROCESS.

Native app redirect: custom URI scheme (myapp://callback) presents URI hijacking risk — any app on the device can register the same scheme and intercept the authorization code. RFC 8252 §7.2 RECOMMENDS claimed HTTPS redirect URIs (OS-verified App Links/Universal Links) as the stronger pattern. PKCE S256 limits the exploitability of an intercepted code but does not eliminate the interception vector. CONCERN.

Refresh absolute lifetime: active rotation without an absolute cap allows indefinitely valid refresh tokens for active users. An attacker with a stolen refresh token who activates it first may continue rotating indefinitely while the legitimate user triggers family revocation on the next superseded attempt — but if the attacker stays ahead of the user's rotation cadence, they remain current. Absolute max cap (e.g., 90 days) terminates this regardless. BCP §4.14 SHOULD: absent. CONCERN.

N CONCERNs ≠ REPROCESS. Verdict: CONCERN.

---

## Pair[CB4-3.4-722]

### A
Cross-source input — ext26 + ext27 combination (correct native app redirect + correct refresh lifecycle):

A native mobile application's security controls are reviewed: (1) the app uses a claimed HTTPS redirect URI (https://app.example.com/callback) with both Apple App Site Association (AASA) file and Android assetlinks.json configured, verified by the OS before the deep link is honored; PKCE S256 is enforced for all authorization code requests; no custom URI scheme registered; (2) refresh tokens carry: abs_exp = issued_at + 90 days (absolute maximum, enforced server-side regardless of rotation activity); idle_exp = last_used + 30 days; per-use rotation applied on every exchange; family detection active — replay of any superseded token in a family triggers immediate revocation of all family members; access token TTL: 15 minutes.

### B
PASS. Correct native app redirect URI pattern and comprehensive refresh token lifecycle controls confirmed.

Native app redirect: claimed HTTPS redirect URI with OS-verified app association files (AASA + assetlinks.json) ensures only the registered app can receive the redirect. The URI hijacking vector present with custom schemes is eliminated — the OS enforces app-binding at the OS layer before the deep link is processed. PKCE S256 adds a second layer (code alone is unusable without the verifier). RFC 8252 §7.2 RECOMMENDS pattern implemented. §8.1 PKCE S256 REQUIRED for native apps: satisfied.

Refresh token lifecycle: 90-day absolute max caps the validity window unconditionally. 30-day idle timeout terminates abandoned credential paths. Per-use rotation prevents replay of superseded tokens. Family revocation on replay handles the attacker-first race condition. 15-minute access token TTL limits standalone token exposure. BCP §4.14 multi-layer defense complete.

No findings.

### Reason
Claimed HTTPS redirect (OS-verified) satisfies RFC 8252 §7.2 RECOMMENDS. PKCE S256 satisfies §8.1 REQUIRED. Four-layer refresh token lifecycle satisfies BCP §4.14 SHOULD controls completely.

### Phrasing-B
PASS — native app redirect and refresh lifecycle both correctly implemented.

Redirect URI: https://app.example.com/callback with AASA (iOS) and assetlinks.json (Android). OS verifies app association before honoring the deep link — no app other than the registered one receives the authorization code. Custom URI scheme hijacking vector absent. PKCE S256 enforced independently. RFC 8252 §7.2 pattern satisfied.

Refresh controls: absolute 90-day max (abs_exp, server-enforced), 30-day idle timeout (idle_exp), per-use rotation, family revocation on replay detection. The 15-minute access token TTL further reduces exposure from any residual window. All BCP §4.14 SHOULD controls present and active. PASS.

---

## Pair[CB4-3.4-723]

### A
Cross-source input — ext27 + ext26 combination (correct ID token role + correct auth code single-use):

A review covers two aspects of an OIDC + OAuth 2.0 system: (1) the system issues both an ID token and an access token at the end of the authorization code flow; the client application uses the ID token exclusively for display purposes — the user's name and email are extracted from the ID token claims for the UI; the id_token_hint parameter is passed to the logout endpoint; all API calls to the resource server use the access token (typ=at+JWT, aud=api.example.com) as the Bearer credential; (2) the authorization server enforces single-use authorization codes — on the first POST to /token, the code is marked as consumed in Redis (with TTL equal to the original code expiry); any subsequent attempt to exchange the same code returns HTTP 400 (error: invalid_grant); additionally, if a consumed code is presented again, all access tokens and refresh tokens previously issued for that authorization session are immediately revoked.

### B
PASS. Correct OIDC token role separation and correct authorization code lifecycle both confirmed.

Token role separation: the ID token is consumed exclusively for its intended purposes — UI display and id_token_hint. The resource server never receives the ID token as a credential. All API authorization flows through the access token with aud=api.example.com, allowing the RS to validate the intended audience. OIDC Core §2 role boundary maintained. V10.5.3 MUST NOT satisfied (ID token not used as API authorization credential).

Authorization code single-use: Redis-backed consumed flag enforces one-exchange-only semantics. The 400 response on re-presentation is the correct RFC 6749 error. The revocation-on-replay trigger satisfies the second MUST condition of RFC 6749 §4.1.2 — replay detection must result in revocation of all tokens issued for that authorization session. Both MUST conditions of §4.1.2 satisfied: (1) code invalidated on first exchange; (2) all associated tokens revoked on replay detection.

No findings.

### Reason
Both controls address separate MUST requirements. ID token role boundary (V10.5.3) and authorization code single-use + revocation-on-replay (RFC 6749 §4.1.2 dual MUST) are independently satisfied.

### Phrasing-B
PASS — ID token role scoped correctly; authorization code single-use with replay revocation confirmed.

ID token: used for display (name, email in UI) and id_token_hint at logout. Not presented to resource server. Not used for API access decisions. All API authorization uses access token with aud=api.example.com. OIDC Core §2 and V10.5.3 boundary maintained.

Auth code: consumed flag set in Redis on first exchange; TTL = original code expiry; any re-presentation returns 400 invalid_grant and triggers immediate revocation of all tokens in the associated session. RFC 6749 §4.1.2 first MUST (single-use) satisfied. §4.1.2 second MUST (revoke all on replay) satisfied. No window exists where a replayed code would produce a second valid token set.

---

## Pair[CB4-3.4-724]

### A
Cross-source input — ext27 comprehensive PASS (all patterns correctly implemented):

A complete authorization system review covers all ext27 security domains: (1) Token roles: access token (typ=at+JWT, aud=api.example.com, exp=+15min, scope claims present) used for all API authorization; ID token used for display and id_token_hint only; resource server validates aud claim against registered identifier on every request. (2) RS algorithm enforcement: allowlist ["RS256", "PS256"] checked as first step in token processing pipeline, before any claim or signature evaluation; tokens with any other alg value rejected with 401 before verification is invoked. (3) Refresh token lifecycle: 90-day absolute max (abs_exp); 30-day idle timeout; per-use rotation; family revocation on replay; access token TTL 15 minutes. (4) Revocation cascade: Redis JTI blocklist populated immediately on any refresh token revocation event; RS consults blocklist on every request; blocklist entries TTL-matched to remaining access token lifetime. (5) Auth code lifecycle: single-use enforced; replay triggers revocation of all associated tokens.

### B
PASS. All ext27 security controls comprehensively implemented. No findings.

Token role separation: access token carries aud=api.example.com (RS-specific), scope claims, and typ=at+JWT. ID token is never presented to the RS. RS validates aud on every request. V10.5.3 MUST NOT satisfied.

Algorithm allowlist: ["RS256", "PS256"] as first pipeline step. No other algorithm value reaches the verification routine. HS256 confusion attack vector eliminated (wrong alg rejected before verification). Algorithm-none vector eliminated. V9.1.1 MUST and RFC 8725 §3.1 satisfied.

Refresh lifecycle: 90-day abs_exp caps unconditional validity. 30-day idle terminates abandoned tokens. Per-use rotation + family revocation handles replay. 15-minute access token TTL. BCP §4.14 SHOULD controls complete.

Revocation cascade: JTI blocklist with TTL-matched entries provides immediate cascade from refresh revocation to access token invalidation. RS blocklist check on every request. RFC 7009 §2 implemented beyond MAY level.

Auth code: single-use + revocation on replay. RFC 6749 §4.1.2 dual MUST satisfied.

### Reason
All five domain control areas cover their respective MUST/SHOULD requirements completely. No partial implementations, no missing MUST conditions, no substitutions for unconditional requirements.

### Phrasing-B
PASS — comprehensive correct implementation across all ext27 security domains.

Summary of verified controls: (1) access token with aud=api.example.com + RS aud validation; ID token scoped to display/id_token_hint — V10.5.3 satisfied; (2) allowlist ["RS256", "PS256"] first-in-pipeline — V9.1.1 + RFC 8725 satisfied; (3) 90-day abs_exp + 30-day idle + rotation + family revocation + 15-min ATT — BCP §4.14 satisfied; (4) JTI blocklist with TTL-matched entries + RS blocklist check per request — RFC 7009 §2 cascade satisfied; (5) auth code single-use + replay-triggered revocation — RFC 6749 §4.1.2 dual MUST satisfied.

No REPROCESS triggers. No CONCERN findings. All controls meet or exceed the applicable normative requirements.
