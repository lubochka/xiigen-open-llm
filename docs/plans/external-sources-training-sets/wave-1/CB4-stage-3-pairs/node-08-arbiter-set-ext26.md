# CB4 Stage 3 — Node 8 Arbiter Baseline Set ext26
# Sequences CB4-3.4-673 through CB4-3.4-680
# Stratum A/B/D — native-app-redirect / auth-code-reuse / grant-type-enforcement
# Running total after this file: 1360 instances (680 base pairs)
# Format: Pair[ID] / A / B / Reason / Phrasing-B

---

## Pair[CB4-3.4-673]

**A**
```
Native mobile application OAuth 2.0 redirect URI review:

Application type: iOS and Android native apps.
Redirect URI registered: myapp://oauth/callback
Implementation: app registers the custom URI scheme "myapp" at OS level.
  On authorization response, the OS routes the redirect to the app.
Developer: "RFC 8252 defines native app redirect URIs. Custom URI schemes are
a standard mechanism for native apps per the RFC."
```

**B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 (RFC 8252 OAuth for Native Apps)
Node 2 — Translation: native app redirect URI method — sub-phase 3.4 in scope
Node 3 — Plan: check RFC 8252 guidance on custom URI schemes vs. alternatives
Node 4 — Processing:
  RFC 8252 §7: native app redirect URI options:
    §7.1: private-use URI schemes (myapp://) — defined but with caveats
    §7.2: claimed HTTPS redirect URIs (Universal Links / App Links) — RECOMMENDED
    §7.3: loopback interface (127.0.0.1) — for desktop/local apps
  RFC 8252 §7.1 caution: custom URI schemes can be claimed by any app on the device
    If two apps register "myapp" scheme, the OS picks one (behavior varies by platform)
    Malicious app can register the same scheme to intercept the authorization response
    "URI scheme hijacking" attack: attacker app intercepts OAuth redirect → auth code stolen
  PKCE mitigates auth code theft: attacker without code_verifier cannot exchange code
    RFC 8252 §8.1: PKCE is REQUIRED for native apps precisely because of redirect interception
    But: PKCE requirement is separate from the URI scheme recommendation
  Developer claim: "RFC 8252 defines custom URI schemes" → TRUE: defined as acceptable
    But: RFC 8252 §7.2 RECOMMENDS claimed HTTPS redirect URIs (Universal Links / App Links)
    as superior to custom schemes precisely because they require app-specific verification
    by the OS (AASA / assetlinks.json) → only the registered app can handle them
  PKCE presence in input: not stated → assume absent for maximum concern assessment
  Verdict: CONCERN — custom URI scheme is spec-defined but not the recommended approach;
    the more secure claimed HTTPS URI approach is preferred per RFC 8252 §7.2
Node 5 — Application: CONCERN (custom URI scheme; recommended alternative available)
Node 6 — Translation: CONCERN — RFC 8252 permits but §7.2 recommends stronger alternative
Node 7 — Arbiter: T-rfc-oauth2 — native app redirect URI
Node 8 — Arbiter verdict:
  CONCERN — Custom URI scheme (myapp://) is defined in RFC 8252 §7.1 but carries
  scheme-hijacking risk: any app on the device can register the same URI scheme and
  intercept the authorization response. RFC 8252 §7.2 recommends "claimed HTTPS
  redirect URIs" (Universal Links on iOS, App Links on Android) which require
  cryptographic verification by the OS that only the registered app can handle them.
  Ensure PKCE (S256) is implemented per RFC 8252 §8.1 REQUIRED for native apps
  to mitigate auth code interception. Consider migrating to claimed HTTPS redirect URIs.
Node 9 — Format: CONCERN; custom scheme hijacking risk; claimed HTTPS recommended
```

**Reason**
RFC 8252 §7.1 defines custom URI schemes but §7.2 recommends claimed HTTPS redirect URIs as more secure (only the registered app can handle them via OS verification). Custom scheme hijacking is a real attack vector mitigated by PKCE but not eliminated. This is CONCERN not REPROCESS because custom URI schemes are spec-defined and PKCE provides meaningful mitigation.

**Phrasing-B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 (RFC 8252)
Node 4 — Processing:
  myapp:// registered: any app can register same scheme → hijacking risk
  RFC 8252 §7.1 permits custom schemes; §7.2 RECOMMENDS claimed HTTPS (stronger)
  PKCE required (§8.1) for native apps; mitigates code theft but not the scheme gap
  Not REPROCESS: custom scheme is spec-defined → CONCERN
Node 8 — Arbiter verdict:
  CONCERN — Custom URI scheme hijacking risk; RFC 8252 §7.2 recommends claimed HTTPS
  redirect URIs. Ensure PKCE S256 per §8.1. Consider Universal Links / App Links.
Node 9 — Format: CONCERN; §7.2 recommendation noted
```

---

## Pair[CB4-3.4-674]

**A**
```
Native mobile application OAuth 2.0 — correct redirect URI implementation:

iOS app redirect URI: https://app.example.com/oauth/callback
  Implemented as: Universal Link (AASA file at https://app.example.com/.well-known/apple-app-site-association)
  AASA associates the callback path with the app's bundle ID.
Android app redirect URI: https://app.example.com/oauth/callback
  Implemented as: App Link (assetlinks.json at https://app.example.com/.well-known/assetlinks.json)
  assetlinks.json associates the callback path with the app's SHA-256 certificate fingerprint.
PKCE: S256 method, code_verifier stored in memory (not storage API), cleared post-exchange.
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 8252)
Node 2 — Translation: native app redirect URI with claimed HTTPS — sub-phase 3.4 in scope
Node 3 — Plan: verify claimed HTTPS redirect URI, OS-level verification, PKCE
Node 4 — Processing:
  Claimed HTTPS redirect URI (Universal Links / App Links):
    AASA + assetlinks.json: only the app whose bundle ID / certificate fingerprint is listed
    can handle the redirect → OS-level cryptographic verification → no scheme hijacking
    RFC 8252 §7.2: this is the RECOMMENDED approach → PASS
  PKCE S256 + memory storage + cleared post-exchange → RFC 8252 §8.1 REQUIRED → PASS
  Redirect URI is exact HTTPS URL registered with the authorization server → PASS
  Both iOS and Android use the same registered redirect URI (consistent) → PASS
Node 5 — Application: PASS (RFC 8252 §7.2 + §8.1 satisfied)
Node 6 — Translation: native app redirect correctly implemented per RFC 8252 recommendations
Node 7 — Arbiter: T-rfc-oauth2 — native app redirect PASS
Node 8 — Arbiter verdict:
  PASS — Native app redirect URI uses claimed HTTPS with OS-level verification (Universal
  Links / App Links), eliminating scheme-hijacking risk. PKCE S256 with in-memory verifier
  storage satisfies RFC 8252 §8.1. Both iOS and Android use the same registered HTTPS
  redirect URI. No findings.
Node 9 — Format: PASS; RFC 8252 §7.2 + §8.1 confirmed
```

**Reason**
Correct native app redirect URI: claimed HTTPS with Universal Links (iOS) and App Links (Android) provides OS-level cryptographic binding — only the registered app can handle the redirect. PKCE S256 with in-memory storage. Pairs with CB4-3.4-673 as the PASS baseline.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 8252)
Node 4 — Processing:
  Universal Links + AASA: OS-verified; only registered app handles redirect → PASS
  App Links + assetlinks.json: certificate fingerprint binding → PASS
  PKCE S256 + in-memory → RFC 8252 §8.1 → PASS
Node 8 — Arbiter verdict:
  PASS — Claimed HTTPS redirect URIs with OS-level verification + PKCE S256.
  RFC 8252 §7.2 + §8.1 both satisfied. No findings.
Node 9 — Format: PASS; §7.2 + §8.1 confirmed
```

---

## Pair[CB4-3.4-675]

**A**
```
Authorization code lifecycle review:

The authorization server issues authorization codes (RFC 6749 §4.1).
Authorization code lifetime: 10 minutes.
Implementation finding: the authorization code is not invalidated after successful
use. After the first token exchange, the code remains valid for the rest of its
10-minute lifetime.
Developer: "We don't need to invalidate codes after use — the client_secret or
PKCE verifier required for the exchange prevents unauthorized re-use."
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 6749 §4.1.2)
Node 2 — Translation: authorization code single-use enforcement — sub-phase 3.4 in scope
Node 3 — Plan: RFC 6749 §4.1.2 single-use requirement; developer argument evaluation
Node 4 — Processing:
  RFC 6749 §4.1.2 (and BCP §2.1.1): authorization codes MUST be single-use
    After successful exchange → code MUST be invalidated
    Implementation: code remains valid for 10 minutes after first use → REPROCESS
  Developer argument: "client_secret or PKCE prevents unauthorized re-use"
    PKCE: verifier is known to the legitimate client; attacker without verifier cannot exchange
    But: if attacker has both code AND code_verifier (e.g., via XSS, logging, proxy)
      → attacker can exchange the code a second time if it remains valid
    "client_secret prevents unauthorized re-use" → IF client uses PKCE, there is no shared secret
    Even with client_secret: if attacker steals both code and secret → second exchange possible
    The argument assumes attacker has only the code; RFC 6749 §4.1.2 applies regardless
    of what other credentials are required — single-use is an independent MUST
  Additional RFC 6749 §4.1.2 requirement: if code is presented again (after first use),
    the AS MUST revoke all previously issued tokens for that code → the server must detect reuse
Node 5 — Application: REPROCESS (RFC 6749 §4.1.2 MUST single-use violated)
Node 6 — Translation: authorization code not invalidated after first use → MUST violation
Node 7 — Arbiter: T-rfc-oauth2 — authorization code single-use
Node 8 — Arbiter verdict:
  REPROCESS — Authorization codes MUST be single-use per RFC 6749 §4.1.2. After a successful
  token exchange, the code must be invalidated immediately. The developer argument that
  PKCE or client_secret prevents re-use conflates authentication requirements with code
  lifecycle requirements — they are independent. If an attacker obtains all required
  credentials (code + verifier or code + secret), a valid code remains exchangeable.
  Additionally, RFC 6749 §4.1.2 requires: if a code is presented after prior use, the AS
  MUST revoke all tokens previously issued for that code. Invalidate codes on first use.
Node 9 — Format: REPROCESS; RFC 6749 §4.1.2 MUST; developer argument rejected
```

**Reason**
RFC 6749 §4.1.2 explicitly MUST require single-use authorization codes. The developer argument (PKCE or client_secret prevents unauthorized re-use) conflates two independent requirements: authentication credentials required for exchange vs. code lifecycle. If an attacker has all required credentials, a non-invalidated code can be exchanged again.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 6749 §4.1.2)
Node 4 — Processing:
  Code remains valid 10 minutes after successful exchange → MUST single-use violated
  "PKCE prevents re-use" → independent requirements; if attacker has code+verifier, second exchange possible
  "client_secret prevents re-use" → same argument; both are authentication, not code lifecycle
  RFC 6749 §4.1.2 MUST: invalidate on first use + revoke all tokens if code re-presented
Node 8 — Arbiter verdict:
  REPROCESS — Authorization code not invalidated after first use violates RFC 6749 §4.1.2 MUST.
  PKCE/client_secret are independent requirements. Invalidate codes on first use.
Node 9 — Format: REPROCESS; §4.1.2 MUST; argument rejected
```

---

## Pair[CB4-3.4-676]

**A**
```
Authorization code lifecycle — correct implementation:

Authorization server behavior:
  1. On successful token exchange: authorization code immediately marked as "used" in DB.
  2. Any subsequent presentation of the same code → 400 invalid_grant.
  3. If a code that was already used is presented: ALL tokens previously issued for that
     code (access token, refresh token) are immediately revoked.
  4. Authorization code lifetime: 60 seconds from issuance.
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 6749 §4.1.2)
Node 2 — Translation: authorization code single-use and replay revocation — sub-phase 3.4
Node 3 — Plan: verify single-use marking, replay detection, token revocation on replay
Node 4 — Processing:
  Single-use: marked "used" on first exchange → PASS (RFC 6749 §4.1.2 MUST)
  Replay → 400 invalid_grant → PASS (rejection of reuse)
  Token revocation on replay: ALL tokens for the code revoked → PASS
    RFC 6749 §4.1.2: if code presented again after use, MUST revoke all issued tokens
    Implementation satisfies both the rejection AND the revocation requirements
  60-second lifetime: short; reduces interception window → PASS
    RFC 6749 §4.1.2: code MUST expire shortly; no specific maximum given; 60s is appropriate
Node 5 — Application: PASS (RFC 6749 §4.1.2 all requirements satisfied)
Node 6 — Translation: authorization code lifecycle correctly implemented
Node 7 — Arbiter: T-rfc-oauth2 — authorization code PASS
Node 8 — Arbiter verdict:
  PASS — Authorization code lifecycle correctly implemented. Codes are single-use (marked
  used on first exchange; subsequent use → 400). On replay detection, ALL associated tokens
  are revoked per RFC 6749 §4.1.2. 60-second lifetime minimizes interception window. No findings.
Node 9 — Format: PASS; §4.1.2 all requirements satisfied; replay revocation noted
```

**Reason**
Correct authorization code lifecycle: single-use marking, replay rejection with 400, and the critical RFC 6749 §4.1.2 requirement that replay → ALL associated tokens revoked. This pairs with CB4-3.4-675 and also establishes the replay-revocation pattern alongside the single-use requirement.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 6749 §4.1.2)
Node 4 — Processing:
  Single-use: marked used on first exchange → PASS
  Replay → 400 → PASS
  Replay → all tokens revoked → RFC 6749 §4.1.2 → PASS
  60-second lifetime → PASS
Node 8 — Arbiter verdict:
  PASS — Authorization code single-use enforced; replay detection triggers full token
  revocation per RFC 6749 §4.1.2. No findings.
Node 9 — Format: PASS; §4.1.2 fully satisfied
```

---

## Pair[CB4-3.4-677]

**A**
```
OAuth 2.0 grant type enforcement review:

Client registration: client "analytics-svc" registered with grant_types=["authorization_code"].
Observation: the token endpoint does not validate the grant_type against the client's
registered grant types. When "analytics-svc" sends a client_credentials grant request:
  POST /token
  grant_type=client_credentials&client_id=analytics-svc&client_secret=...
The token endpoint issues an access token with the client's registered scopes.

Developer: "The client authenticates successfully, so it should be able to use any grant."
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 6749 §3.1.1) + T-asvs-authorization
Node 2 — Translation: grant type enforcement per client registration — sub-phase 3.4
Node 3 — Plan: RFC 6749 grant type restriction; developer argument; privilege escalation
Node 4 — Processing:
  RFC 6749 §3.1.1 and OAuth 2.1 §2.1: authorization server MUST restrict grant types per client
    client_credentials grant = machine-to-machine, without user context
    authorization_code grant = user-delegated, with user context and consent
    These are distinct authorization models with different security properties
  "analytics-svc" registered for authorization_code only:
    The client is designed for user-delegated authorization
    Using client_credentials → the service obtains tokens WITHOUT user consent or context
    Scopes granted may include user-delegated scopes that should require user consent
  Developer argument: "client authenticates successfully = can use any grant"
    Authentication (client_id + client_secret) = identity verification
    Authorization (grant_type restriction) = what the client is permitted to do
    Passing authentication does not grant permission to use unregistered grant types
    CWE-269: privilege escalation via grant type bypass
  RFC 6749: the token endpoint MUST enforce registered grant types per client
  Without enforcement: any registered client can bypass authorization_code flow
    (which requires user involvement) and use client_credentials (machine-only)
Node 5 — Application: REPROCESS (grant type enforcement absent; CWE-269)
Node 6 — Translation: RFC 6749 §3.1.1 grant type restriction not enforced → REPROCESS
Node 7 — Arbiter: T-rfc-oauth2 + T-asvs-authorization — grant type bypass
Node 8 — Arbiter verdict:
  REPROCESS — Grant type is not validated against client registration. "analytics-svc"
  registered only for authorization_code can use client_credentials to obtain tokens
  without user consent or context. This is CWE-269 privilege escalation: bypassing user
  consent by switching grant types. Authentication (passing client_id + client_secret)
  is not equivalent to authorization to use any grant type. Remediate: enforce registered
  grant_types per client on every token request.
Node 9 — Format: REPROCESS; RFC 6749 §3.1.1; CWE-269 privilege escalation via grant bypass
```

**Reason**
Not enforcing grant type per client registration is REPROCESS: it allows a client to bypass user consent (authorization_code flow) by using client_credentials instead. The developer argument ("authenticates = can use any grant") conflates identity verification with capability authorization. CWE-269 applies.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 / T-asvs-authorization
Node 4 — Processing:
  analytics-svc registered for authorization_code only
  Token endpoint accepts client_credentials without checking registration → REPROCESS
  "Authenticates = can use any grant" → identity ≠ capability; CWE-269 bypass
  RFC 6749 §3.1.1: grant types MUST be enforced per client registration
Node 8 — Arbiter verdict:
  REPROCESS — Grant type bypass (CWE-269). analytics-svc uses client_credentials
  to avoid user consent. Enforce registered grant_types on every token request.
Node 9 — Format: REPROCESS; §3.1.1 + CWE-269
```

---

## Pair[CB4-3.4-678]

**A**
```
Grant type enforcement review — correct implementation:

Token endpoint validation:
  1. Client authenticates (client_id + client_secret).
  2. Token endpoint reads the requested grant_type from the request.
  3. Token endpoint queries client registration: fetches registered grant_types[] for client.
  4. If requested grant_type is not in registered list → 400 unauthorized_client error.
  5. Client "api-gateway" has grant_types=["client_credentials"].
     Attempting authorization_code with api-gateway → 400 unauthorized_client.
  6. Client "user-spa" has grant_types=["authorization_code", "refresh_token"].
     client_credentials attempt → 400 unauthorized_client.
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 6749 §3.1.1)
Node 2 — Translation: grant type enforcement per client — sub-phase 3.4 in scope
Node 3 — Plan: verify enforcement logic; check bidirectional restriction
Node 4 — Processing:
  Step 1-4: client auth → grant_type from request → check against registered list → reject if absent
    This is the correct enforcement flow → PASS (RFC 6749 §3.1.1)
  api-gateway: client_credentials only; authorization_code rejected → correct restriction → PASS
  user-spa: authorization_code + refresh_token; client_credentials rejected → correct restriction → PASS
  Bidirectional enforcement: machine-only clients cannot use user-delegation grants;
    user-delegation clients cannot use machine-only grants → PASS
  Error code: 400 unauthorized_client → correct per RFC 6749 §5.2 → PASS
Node 5 — Application: PASS (grant type enforcement correctly implemented)
Node 6 — Translation: RFC 6749 §3.1.1 enforced; both directions tested
Node 7 — Arbiter: T-rfc-oauth2 — grant type enforcement PASS
Node 8 — Arbiter verdict:
  PASS — Grant type enforcement correctly implemented. Each token request validates the
  requested grant_type against the client's registered grant_types. Machine clients
  (client_credentials) cannot use user-delegation flows and vice versa. Correct error
  code (400 unauthorized_client) returned on violation. No findings.
Node 9 — Format: PASS; §3.1.1 enforcement confirmed; bidirectional restriction verified
```

**Reason**
Correct grant type enforcement: each token request validated against registered grant_types; both machine→user-delegation and user-delegation→machine restrictions enforced. Pairs with CB4-3.4-677.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 6749 §3.1.1)
Node 4 — Processing:
  Requested grant_type checked against registered list; reject if absent → PASS
  api-gateway: client_credentials only; authorization_code rejected → PASS
  user-spa: authorization_code + refresh_token; client_credentials rejected → PASS
  400 unauthorized_client → RFC 6749 §5.2 → PASS
Node 8 — Arbiter verdict:
  PASS — Grant type enforcement correct in both directions. No findings.
Node 9 — Format: PASS; §3.1.1 bidirectional enforcement confirmed
```

---

## Pair[CB4-3.4-679]

**A**
```
Developer argument about authorization code single-use:

"Our authorization codes expire in 30 seconds. We don't mark them as 'used' after
the first exchange — we rely on the short lifetime to prevent replay. In 30 seconds,
an attacker would need to both intercept the code and complete a second exchange
before the code expires. The PKCE verifier adds an additional barrier."
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 6749 §4.1.2)
Node 2 — Translation: authorization code single-use enforcement — sub-phase 3.4
Node 3 — Plan: RFC 6749 §4.1.2 MUST single-use; evaluate short-lifetime argument
Node 4 — Processing:
  RFC 6749 §4.1.2: authorization code MUST be single-use; no exception for short lifetime
  "30 seconds + short lifetime = replay impractical" → same pattern as PAR one-time-use argument
    (1) MUST requirements are unconditional — not conditional on developer risk assessment
    (2) 30 seconds is not impractical for a scripted attack:
        If code is intercepted (e.g., in referrer header, proxy, compromised redirect URI),
        attacker can immediately submit to /token with the code_verifier (if known)
    (3) PKCE adds a barrier for attackers without code_verifier — but:
        Authorized attacker (XSS, malicious app) may have access to both code AND verifier
        PKCE is independent of single-use; both are required simultaneously
  RFC 6749 §4.1.2 additional requirement: on code reuse detection, MUST revoke all tokens
    Without single-use tracking: the server cannot detect code reuse → cannot revoke
    This means a stolen code that has been used cannot trigger protective revocation
  "Short lifetime + PKCE" reduces risk but does not satisfy the MUST → REPROCESS
Node 5 — Application: REPROCESS (RFC 6749 §4.1.2 MUST single-use; risk argument rejected)
Node 6 — Translation: REPROCESS — MUST requirement unconditional; risk-based argument rejected
Node 7 — Arbiter: T-rfc-oauth2 — authorization code single-use
Node 8 — Arbiter verdict:
  REPROCESS — RFC 6749 §4.1.2 MUST requires authorization code single-use regardless of
  code lifetime. Short lifetime and PKCE reduce risk but cannot substitute for the MUST
  requirement. Additionally, without single-use tracking, the server cannot detect code
  reuse and cannot fulfill the RFC 6749 §4.1.2 requirement to revoke all tokens issued
  for a replayed code. Mark codes as used on first exchange; reject subsequent uses with
  400 invalid_grant; revoke all tokens if a used code is re-presented.
Node 9 — Format: REPROCESS; §4.1.2 MUST; short-lifetime argument rejected
```

**Reason**
Same pattern as PAR one-time-use short-expiry argument: MUST is unconditional. The additional dimension here is that without single-use tracking, the server cannot fulfill the RFC 6749 §4.1.2 replay-detection-triggers-revocation requirement — the violation is doubly justified.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 6749 §4.1.2)
Node 4 — Processing:
  MUST single-use: no exception for short lifetime
  "30 seconds + PKCE = impractical" → REJECTED:
    (1) MUST is unconditional
    (2) Short lifetime doesn't stop scripted attacker
    (3) Without tracking, reuse cannot be detected → RFC §4.1.2 token revocation on replay impossible
Node 8 — Arbiter verdict:
  REPROCESS — §4.1.2 MUST single-use. Short lifetime and PKCE reduce risk; do not satisfy MUST.
  Without tracking, replay revocation (also MUST) is also impossible.
Node 9 — Format: REPROCESS; §4.1.2 MUST; two separate justifications
```

---

## Pair[CB4-3.4-680]

**A**
```
Comprehensive review — all ext26 patterns correct:

1. Native app redirect: claimed HTTPS redirect URIs (Universal Links / App Links)
   with AASA and assetlinks.json. PKCE S256. Registered redirect URI exact-match enforced.
2. Authorization code: single-use (marked used on first exchange). Subsequent use → 400.
   Re-presented used code → ALL associated tokens revoked. Lifetime: 45 seconds.
3. Grant type: token endpoint validates requested grant_type against registered list.
   client_credentials clients cannot use authorization_code and vice versa.
4. PAR (bonus): Redis consumed flag; client_id binding; authenticated /par endpoint.
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 8252 + RFC 6749 + RFC 9126)
Node 2 — Translation: all four ext26 patterns in sub-phase 3.4 scope
Node 3 — Plan: verify each against applicable RFC
Node 4 — Processing:
  F1: Claimed HTTPS + AASA/assetlinks.json → RFC 8252 §7.2 RECOMMENDED approach → PASS
      PKCE S256 → RFC 8252 §8.1 REQUIRED → PASS
      Exact redirect URI registration → PASS
  F2: Single-use (marked on first exchange) + 400 on reuse + token revocation on replay
      → RFC 6749 §4.1.2 MUST all three conditions satisfied → PASS
      45-second lifetime → short; appropriate → PASS
  F3: Grant type validated per registration → RFC 6749 §3.1.1 → PASS
      Bidirectional restriction → no grant type bypass possible → PASS
  F4: PAR — Redis consumed flag + client_id binding + authenticated endpoint → RFC 9126 §4.1 → PASS
Node 5 — Application: PASS × 4
Node 6 — Translation: all four patterns correctly implemented
Node 7 — Arbiter: comprehensive ext26 multi-RFC PASS
Node 8 — Arbiter verdict:
  PASS — All four ext26 patterns correctly implemented. Native app redirect uses claimed
  HTTPS with OS-level verification and PKCE S256 (RFC 8252 §7.2 + §8.1). Authorization
  codes are single-use with replay detection and token revocation (RFC 6749 §4.1.2). Grant
  types are enforced per client registration (RFC 6749 §3.1.1). PAR satisfies RFC 9126 §4.1.
  No findings.
Node 9 — Format: PASS; four patterns confirmed; RFC citations for each
```

**Reason**
Comprehensive PASS for all ext26 patterns: RFC 8252 (native app redirect), RFC 6749 §4.1.2 (authorization code single-use with replay revocation), RFC 6749 §3.1.1 (grant type enforcement), and RFC 9126 (PAR from ext25, reinforced here). All requirements met across four patterns.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 × 4
Node 4 — Processing:
  F1: claimed HTTPS + PKCE S256 → RFC 8252 §7.2 + §8.1 → PASS
  F2: single-use + 400 + token revocation → RFC 6749 §4.1.2 → PASS
  F3: grant type per registration → RFC 6749 §3.1.1 → PASS
  F4: PAR Redis + binding → RFC 9126 §4.1 → PASS
Node 8 — Arbiter verdict:
  PASS — All four ext26 patterns correct. RFC 8252, §4.1.2, §3.1.1, RFC 9126 all satisfied.
Node 9 — Format: PASS; four patterns confirmed
```
