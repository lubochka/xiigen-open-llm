# CB4 Stage 3 — Cross-Source Combination Set ext16
# Sequences CB4-3.4-611 through CB4-3.4-620
# Stratum B/C/D — ext23 multi-source combinations
# Running total after this file: 1240 instances (620 base pairs)
# Format: Pair[ID] / A / B / Reason / Phrasing-B

---

## Pair[CB4-3.4-611]

**A**
```
Security review findings for OAuth 2.0 / OIDC deployment:

Finding 1 — JWKS rollover: JWKS endpoint refreshed quarterly. New signing key published.
  Dual-key window: 8 hours. Maximum access token TTL: 24 hours.
Finding 2 — Token logging: JWT access tokens logged at INFO level in production API
  gateway for debugging. Log entries include full token string.
Finding 3 — Logout: OIDC back-channel logout implemented. Logout token validated:
  signature, iss, sub, jti (uniqueness checked via Redis set), event claim present.
```

**B**
```
REPROCESS

Node 1 — Domain split: IETF-RFCS / OWASP-ASVS / IETF-RFCS
  JWKS rollover → T-rfc-oauth2 (RFC 8414 / JWKS lifecycle)
  Token logging → T-asvs-token (V9.3.1 token transport / exposure)
  Back-channel logout → T-rfc-oidc (OIDC Core §17 / back-channel spec)
Node 2 — Translation: all three findings mapped to correct sub-phase 3.4 scope
Node 3 — Plan: F1=rollover-window-TTL-gap; F2=token-exposure-production; F3=logout-completeness
Node 4 — Processing:
  F1: 8h window < 24h max TTL → tokens issued near rotation boundary rejected
      Violation: operational gap (valid tokens rejected); MUST NOT break valid tokens
  F2: INFO/prod logging of full JWT → REPROCESS
      V9.3.1 MUST NOT log credential-equivalent tokens; INFO = reachable in prod aggregators
  F3: sig + iss + sub + jti-unique + event claim → compliant back-channel logout; PASS
Node 5 — Application: CONCERN (F1) + REPROCESS (F2) + PASS (F3)
Node 6 — Translation: verdict elevation rule applied: REPROCESS present → overall REPROCESS
Node 7 — Arbiter: F2 (V9.3.1 production token logging) is controlling REPROCESS finding
Node 8 — Arbiter verdict:
  REPROCESS — JWT access tokens logged at INFO level in production violates V9.3.1.
  Full token strings in production logs are credential-equivalent. INFO level is not
  disabled in production environments. Additionally, JWKS rollover window (8h) is
  shorter than max access token TTL (24h), creating a window where valid tokens are
  rejected. Correct: remove token logging from INFO level; extend rollover window to
  ≥ 24h (e.g., 72h for safety margin). Back-channel logout implementation is correct.
Node 9 — Format: REPROCESS with finding enumeration; F2 primary; F1 secondary; F3 passing
```

**Reason**
Two independent violations: F2 is REPROCESS (V9.3.1 production token logging, INFO level not disabled), F1 is CONCERN (rollover window < TTL). REPROCESS overrides CONCERN. F3 is the one correct element demonstrating correct back-channel logout. The multi-source nature (ASVS + RFC) validates cross-standard enforcement.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split:
  Finding 1 (JWKS rollover window): T-rfc-oauth2
  Finding 2 (token INFO logging): T-asvs-token (V9.3.1)
  Finding 3 (back-channel logout): T-rfc-oidc
Node 4 — Processing:
  F1: rollover window 8h < max access token TTL 24h → valid tokens at boundary rejected → CONCERN
  F2: JWT full strings at INFO/production → credential exposure in prod log aggregators → REPROCESS
  F3: back-channel logout token fully validated (sig/iss/sub/jti-unique/event) → PASS
Node 8 — Arbiter verdict:
  REPROCESS — production-level JWT logging (F2) violates V9.3.1 at INFO severity.
  INFO-level logs are active in production and route to aggregation systems.
  F1 rollover window gap is a secondary CONCERN. F3 back-channel logout is correct.
  Remediate: purge token logging from INFO; extend JWKS dual-key window to ≥ TTL.
Node 9 — Format: REPROCESS; primary F2; secondary F1 CONCERN; F3 passing
```

---

## Pair[CB4-3.4-612]

**A**
```
Security review — OAuth 2.0 service-to-service architecture:

Component A: Internal API gateway authenticates downstream services using JWT client
  assertions (RFC 7523). Validation: signature verified with JWKS, iss claim matches
  registered client_id, aud claim exact-match to gateway's issuer URI, exp checked,
  jti stored in cache for 1× assertion lifetime to prevent replay.
Component B: Password hashing for user accounts: Argon2id with m=65536 (64 MiB),
  t=3 iterations, p=4 parallelism. Salt: 16-byte CSPRNG per user. Hash stored in
  separate credential table, not co-located with profile data.
Component C: JWKS key rollover: 4096-bit RSA keys, rotated every 6 months.
  New key pre-published 30 days before old key retirement. Dual-key window: 30 days.
  Max access token TTL: 15 minutes.
```

**B**
```
PASS

Node 1 — Domain split: IETF-RFCS / OWASP-ASVS / IETF-RFCS
  Client assertions → T-rfc-oauth2 (RFC 7523)
  Password hashing → T-asvs-authentication (V6.2.5)
  JWKS rollover → T-rfc-oauth2 / T-asvs-token (V9.2)
Node 2 — Translation: all three components map to sub-phase 3.4
Node 3 — Plan: A=assertion-validation-completeness; B=password-hash-compliance; C=rollover-window
Node 4 — Processing:
  A: sig + iss=client_id + aud=exact + exp + jti-cached → RFC 7523 §3 complete validation → PASS
  B: Argon2id m=65536/t=3/p=4 → exceeds OWASP minimum m=47104/t=1/p=1 → PASS
     Separate credential table → V6.2.5 compliant storage → PASS
  C: 4096-bit RSA → RFC 7518 §3.3 PASS; dual-key window 30 days >> 15-min TTL → PASS
     Pre-published 30 days → operational overlap adequate → PASS
Node 5 — Application: PASS × 3
Node 6 — Translation: three independent PASS; no violations detected
Node 7 — Arbiter: comprehensive PASS — all RFC and ASVS requirements met
Node 8 — Arbiter verdict:
  PASS — All three components are compliant. JWT client assertion validation is complete
  per RFC 7523: signature, exact iss match, exact aud match, exp, and jti replay prevention
  with cache sized to assertion lifetime. Password hashing uses Argon2id at compliant
  parameters. JWKS rollover has a 30-day dual-key window against a 15-minute max TTL —
  a ratio exceeding 2880×. No findings.
Node 9 — Format: PASS; all three components enumerated as conformant
```

**Reason**
All three controls are correct: RFC 7523 full assertion validation, Argon2id with compliant parameters, and JWKS rollover with window far exceeding TTL. Multi-source cross-check (RFC 7523 × ASVS V6.2.5 × JWKS lifecycle) demonstrates comprehensive PASS baseline.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 / T-asvs-authentication / T-rfc-oauth2
Node 4 — Processing:
  A: RFC 7523 full validation chain satisfied (sig/iss/aud-exact/exp/jti-cached) → PASS
  B: Argon2id m=65536/t=3/p=4 → compliant; separate table → PASS
  C: 30-day window vs 15-min TTL; pre-publication 30 days prior → PASS
Node 8 — Arbiter verdict:
  PASS — Client assertions (RFC 7523), password hashing (V6.2.5 Argon2id), and JWKS
  rollover all meet or exceed applicable requirements. No remediation needed.
Node 9 — Format: PASS; all components listed as conformant
```

---

## Pair[CB4-3.4-613]

**A**
```
Developer argues during code review:

"Token exchange is the right tool here. Our internal analytics service needs read:events
scope AND admin:config scope to do its job — it processes configuration events for
reporting. The analytics service starts with a user token (scope: read:events), then
calls token exchange to get admin:config added. RFC 8693 supports scope specification
in the exchange request, and since analytics is an internal trusted service, this
is appropriate."
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 8693 token exchange)
Node 2 — Translation: token exchange scope widening — sub-phase 3.4 in scope
Node 3 — Plan: check RFC 8693 §7.1 scope widening rule; check privilege-escalation
Node 4 — Processing:
  RFC 8693 §7.1: if scope parameter specifies any scope not in subject_token → MUST reject
  Subject token scope: read:events
  Requested scope: read:events + admin:config
  admin:config is not in subject_token → this is prohibited scope widening
  Developer claim: "RFC 8693 supports scope specification" — TRUE but partial
    RFC 8693 allows scope restriction and translation; §7.1 prohibits widening
  Developer claim: "internal trusted service" — not a recognized exception in RFC 8693
    trust boundary of the exchange server does not grant new permissions
  CWE-269: improper privilege management — granting admin:config via exchange
    without independent authorization for that scope
Node 5 — Application: REPROCESS
Node 6 — Translation: RFC 8693 §7.1 MUST violation; CWE-269 privilege escalation
Node 7 — Arbiter: T-rfc-oauth2 + T-cwe-authz — joint finding
Node 8 — Arbiter verdict:
  REPROCESS — Token exchange to add admin:config to a subject_token that only carries
  read:events is prohibited scope widening under RFC 8693 §7.1. The exchange server
  MUST reject any exchange that would grant scopes not present in the subject token.
  "Internal trusted service" is not an exception; trust boundary does not manufacture
  new authorization. The analytics service must obtain admin:config independently —
  either via its own client credentials grant or via explicit authorization grant at
  registration time.
Node 9 — Format: REPROCESS; RFC 8693 §7.1 cited; CWE-269 noted; remediation specified
```

**Reason**
Scope widening via token exchange is an explicit REPROCESS per RFC 8693 §7.1. The developer argument ("RFC supports scope specification" + "internal trusted") is a partial-truth adversarial pattern: RFC 8693 does allow scope in exchange requests — for narrowing and translation, not widening. Internal trust does not grant new permissions.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 / T-cwe-authz
Node 4 — Processing:
  Subject token carries read:events only
  Exchange request adds admin:config → not present in subject token → §7.1 violation
  "RFC 8693 allows scope" → partial claim; §7.1 explicitly prohibits widening
  "Trusted service" → no RFC 8693 exception for internal services
  CWE-269: privilege escalation pathway created
Node 8 — Arbiter verdict:
  REPROCESS — Requesting admin:config via token exchange when the subject token
  only authorizes read:events constitutes RFC 8693 §7.1 prohibited scope widening.
  Exchange cannot manufacture new authorization. Analytics must obtain admin:config
  through a separate, explicit authorization mechanism.
Node 9 — Format: REPROCESS; §7.1 primary; CWE-269 secondary
```

---

## Pair[CB4-3.4-614]

**A**
```
OIDC configuration for a customer-facing web portal:

1. Logout: OIDC front-channel logout implemented via pixel iframes to registered
   logout_uri endpoints for each session participant. Browser fires on window.onunload.
2. SBOM: Generated monthly from package-lock.json using npm audit. CVE review
   performed manually at SBOM generation time. Next generation: +30 days.
3. Scope control: Authorization server issues scopes as requested with no server-side
   policy. Scopes are: read:profile, write:profile, admin:all. Any authenticated user
   can request admin:all.
```

**B**
```
REPROCESS

Node 1 — Domain split: IETF-RFCS / NIST-SSDF / OWASP-ASVS
  Logout → T-rfc-oidc (front-channel logout spec)
  SBOM → T-ssdf-process (PO.3)
  Scope control → T-asvs-authorization (V8 / CWE-269)
Node 2 — Translation: all three in sub-phase 3.4
Node 3 — Plan: F1=logout-reliability; F2=SBOM-process; F3=scope-policy
Node 4 — Processing:
  F1: front-channel logout via iframe → CONCERN
      Browser-initiated: subject to SameSite, ITP, ad-blockers, tab close timing
      Not a MUST violation (front-channel logout is spec-defined); reliability gap → CONCERN
  F2: monthly generation + manual review at generation time → CONCERN
      30-day window between SBOM refresh cycles; CVE disclosed day after generation
      not discovered for 29 days → meaningful exposure window; PO.3.2 gap → CONCERN
  F3: any authenticated user can request admin:all → REPROCESS
      No server-side policy enforcement — scope == claim, not authorization
      CWE-269: privilege escalation; V8 authorization requirement: server MUST enforce
      Authorization server issuing admin:all to any user violates core authorization model
Node 5 — Application: CONCERN + CONCERN + REPROCESS
Node 6 — Translation: REPROCESS present → overall REPROCESS
Node 7 — Arbiter: F3 is controlling REPROCESS finding; F1/F2 secondary CONCERNs
Node 8 — Arbiter verdict:
  REPROCESS — Scope control is absent: any authenticated user can obtain admin:all
  scope by including it in the authorization request. This is a fundamental CWE-269
  privilege escalation — the authorization server issues scopes as requested without
  policy enforcement. Additionally: front-channel logout via iframes carries browser
  reliability concerns (CONCERN, not REPROCESS). Monthly SBOM with 30-day CVE window
  is a PO.3.2 gap (CONCERN). Primary remediation: implement scope policy enforcement
  on the authorization server; admin:all must require explicit elevated authorization.
Node 9 — Format: REPROCESS; F3 primary; F1/F2 secondary CONCERNs enumerated
```

**Reason**
F3 (any user can obtain admin:all) is a clear REPROCESS — no server-side scope enforcement is CWE-269 privilege escalation, not merely a gap. F1 (front-channel logout) and F2 (monthly SBOM) are each CONCERN individually. Three violations: REPROCESS governs the overall verdict.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oidc / T-ssdf-process / T-asvs-authorization + T-cwe-authz
Node 4 — Processing:
  F1: front-channel logout (iframe) → reliability concerns; not MUST violation → CONCERN
  F2: monthly SBOM / manual CVE review → 29-day blind window possible → CONCERN (PO.3.2)
  F3: authorization server issues admin:all to any requester → no enforcement → REPROCESS
      CWE-269: privilege escalation by scope request alone
Node 8 — Arbiter verdict:
  REPROCESS — No server-side scope policy allowing any user to claim admin:all is a
  CWE-269 privilege escalation and ASVS V8 violation. F1 logout and F2 SBOM are
  secondary CONCERNs. Implement authorization server scope policies immediately.
Node 9 — Format: REPROCESS; F3 primary; CONCERNs secondary
```

---

## Pair[CB4-3.4-615]

**A**
```
Developer proposes silent authentication flow for a single-page application:

"We want seamless re-authentication when the user's session might have expired.
Plan:
1. Make a hidden iframe request to /authorize?response_type=code&prompt=none
   &client_id=spa-client&redirect_uri=https://app.example.com/silent-callback
   &code_challenge=...&code_challenge_method=S256
2. If the user has an active session, we get the code in the callback
3. If not, we get an error and show the login prompt
There's no id_token_hint because we don't have the user's previous ID token cached.
We accept any authenticated user session the IdP returns."
```

**B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 / T-oauth-oidc (OIDC prompt=none)
Node 2 — Translation: prompt=none silent authentication — sub-phase 3.4 in scope
Node 3 — Plan: check prompt=none requirements: PKCE, redirect_uri, id_token_hint
Node 4 — Processing:
  PKCE with S256 present → PASS
  Exact redirect_uri registered and used → PASS
  response_type=code (not implicit/hybrid) → PASS
  id_token_hint absent → CONCERN
    Without id_token_hint, IdP cannot confirm it is authenticating the same user
    as the previous session. On shared devices or SSO environments, prompt=none
    could silently return a session for a different user. OIDC Core recommends
    id_token_hint to prevent session mix-up in silent flows.
  "We accept any authenticated user session" → confirms the risk: no binding to
    expected user identity → session mix-up on shared devices undetected
  Not a REPROCESS: no MUST violation in OIDC Core for id_token_hint;
    the absence is a best-practice gap, not a normative requirement
Node 5 — Application: CONCERN (id_token_hint absent; session mix-up risk)
Node 6 — Translation: CONCERN — recommended control absent, not prohibited behavior
Node 7 — Arbiter: T-oauth-oidc — prompt=none / id_token_hint
Node 8 — Arbiter verdict:
  CONCERN — prompt=none implementation correctly uses PKCE (S256), exact redirect_uri,
  and authorization-code flow. The gap is the absence of id_token_hint: without it,
  the IdP cannot bind the silent authentication to the expected user, creating a
  session mix-up risk on shared devices or when multiple accounts share the same
  IdP session. OIDC Core recommends id_token_hint for silent re-authentication.
  Remediate: cache the user's ID token (or its sub claim) and include id_token_hint
  in prompt=none requests to bind the silent authentication to the expected identity.
Node 9 — Format: CONCERN; id_token_hint gap identified; PKCE/redirect_uri noted as correct
```

**Reason**
PKCE + S256 + exact redirect_uri + code flow are all correct; id_token_hint is absent. OIDC Core recommends but does not MUST-require id_token_hint, so this is CONCERN not REPROCESS. The developer's "accept any authenticated user" statement confirms the session mix-up risk is unmitigated. This is the prompt=none-without-id_token_hint pattern established in ext23.

**Phrasing-B**
```
CONCERN

Node 1 — Domain split: T-oauth-oidc / T-rfc-oauth2
Node 4 — Processing:
  PKCE S256 + exact redirect_uri + code flow → correct; PASS on those controls
  id_token_hint absent → session mix-up risk on shared devices
  "Accept any authenticated user" → unmitigated session binding gap
  Not a MUST violation → CONCERN not REPROCESS
Node 8 — Arbiter verdict:
  CONCERN — Silent re-authentication (prompt=none) is structurally correct (PKCE/S256,
  exact redirect_uri, code flow). However, id_token_hint is absent; without it, the
  IdP may silently authenticate a different user on a shared device. Cache the ID token
  and include id_token_hint to bind silent authentication to the expected identity.
Node 9 — Format: CONCERN; id_token_hint as recommended but absent
```

---

## Pair[CB4-3.4-616]

**A**
```
Token exchange implementation for a microservices mesh:

Service A (scope: read:orders) calls token exchange to obtain a token for Service B
(scope: read:orders). The exchange request includes:
  grant_type: urn:ietf:params:oauth:grant-type:token-exchange
  subject_token: <Service A's access token>
  requested_token_type: urn:ietf:params:oauth:token-type:access_token
  audience: service-b
  scope: read:orders  (same scope as subject token)

Service B validates: signature, iss, exp, aud=service-b, scope=read:orders.
Resource indicators (RFC 8707): Service A's token was issued with
  resource=https://orders.internal, Service B validates aud matches resource URI.
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 8693 + RFC 8707)
Node 2 — Translation: token exchange with resource indicators — sub-phase 3.4 in scope
Node 3 — Plan: check scope (not widening), audience binding, resource indicator enforcement
Node 4 — Processing:
  Scope: read:orders requested == read:orders in subject_token → no widening → PASS
  Audience: explicit audience=service-b; Service B validates aud → PASS
  RFC 8707 resource indicators: subject_token issued with resource=https://orders.internal;
    Service B validates aud matches resource URI → certificate-binding equivalent → PASS
  Exchange semantics: delegation chain is transparent (service A → exchange → service B)
  No privilege escalation detected; least-privilege preserved throughout chain
Node 5 — Application: PASS (scope preserved, audience explicit, resource indicator enforced)
Node 6 — Translation: RFC 8693 compliant; RFC 8707 enforced at resource server
Node 7 — Arbiter: T-rfc-oauth2 — token exchange + resource indicators
Node 8 — Arbiter verdict:
  PASS — Token exchange correctly preserves scope (no widening), specifies explicit
  audience, and Service B validates both aud and the resource indicator. RFC 8693 §7.1
  is satisfied (scope read:orders is present in subject_token). RFC 8707 resource
  indicator enforcement at Service B ensures the token is only honored for the
  intended resource server. This is the correct microservices delegation pattern.
Node 9 — Format: PASS; RFC 8693 + RFC 8707 compliance stated
```

**Reason**
Token exchange with scope preservation (no widening), explicit audience, and RFC 8707 resource indicator enforcement at the consuming service is the fully correct pattern. All three requirements are met: §7.1 no-widening, aud explicit, aud validated by RS.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 8693 / RFC 8707)
Node 4 — Processing:
  Scope: read:orders in subject_token and in exchange request → no widening → PASS
  Audience: service-b explicit; validated by Service B → PASS
  Resource indicator: resource URI enforced at Service B → PASS
  Delegation chain preserved without privilege escalation
Node 8 — Arbiter verdict:
  PASS — Scope-preserving token exchange with explicit audience and RFC 8707 resource
  indicator enforcement. All exchange conditions met; no scope widening or aud gaps.
Node 9 — Format: PASS; delegation chain noted as correct
```

---

## Pair[CB4-3.4-617]

**A**
```
Review of an OIDC deployment's logout and back-channel session management:

The system implements OIDC back-channel logout. When the IdP sends a logout token:
  1. Signature verified with IdP's JWKS (fetched fresh, not cached)
  2. iss verified against registered issuer
  3. aud verified against client_id
  4. iat checked: token not older than 5 minutes
  5. event claim: urn:ietf:params:oauth:event:backchannel-logout present
  6. sub or sid identifies which session to terminate

However: jti claim is not tracked. The system processes the same logout token
multiple times if it arrives more than once (e.g., network retry by the IdP).
Developer comment: "Replayed logout tokens just terminate an already-terminated
session — harmless. We don't need a jti cache."
```

**B**
```
CONCERN

Node 1 — Domain split: T-rfc-oidc (OIDC back-channel logout spec)
Node 2 — Translation: back-channel logout token validation — sub-phase 3.4 in scope
Node 3 — Plan: validate all required logout token claims; check jti tracking requirement
Node 4 — Processing:
  sig + iss + aud + iat (5-min) + event claim + sub/sid → correct per spec → PASS
  jti not tracked → re-processing of replay possible
  Developer claim: "replayed logout = harmless" — partially correct in the common case
    Re-terminating an already-terminated session is low-risk
    However: spec recommends jti uniqueness to prevent unintended session manipulation
    in edge cases (e.g., logout token replay to deny service, race conditions with
    re-login between logout + replay)
  OIDC Back-Channel Logout spec §2.4: implementation SHOULD track jti to prevent replay
    SHOULD = recommendation, not MUST → CONCERN not REPROCESS
  The gap is real but not a MUST-level violation; developer rationale is partially valid
Node 5 — Application: CONCERN (jti not tracked; spec recommendation not followed)
Node 6 — Translation: CONCERN — SHOULD-level gap; no MUST violation
Node 7 — Arbiter: T-rfc-oidc — logout token jti tracking
Node 8 — Arbiter verdict:
  CONCERN — Back-channel logout validation is largely correct: signature, iss, aud, iat
  freshness (5 minutes), event claim, and sub/sid identification are all implemented.
  The gap is jti tracking: the spec recommends (SHOULD) tracking jti to prevent replay.
  The developer's rationale ("harmless replay") is partially valid for the common case,
  but jti tracking eliminates edge-case risks (DoS via repeated logout, race conditions).
  Implement a jti cache sized to the logout token lifetime (5 minutes) to close this gap.
Node 9 — Format: CONCERN; jti gap identified; other controls noted as correct
```

**Reason**
OIDC Back-Channel Logout spec uses SHOULD (not MUST) for jti tracking → CONCERN not REPROCESS. The developer's "harmless replay" argument is partially correct but ignores edge cases. Most of the validation chain is correct; jti is the one missing recommended control. Compare with ext23's PASS baseline (which included jti uniqueness): this is the Stratum B variant with one recommended control missing.

**Phrasing-B**
```
CONCERN

Node 1 — Domain split: T-rfc-oidc
Node 4 — Processing:
  sig/iss/aud/iat/event/sub-sid → correct → PASS
  jti not tracked → replay possible
  SHOULD-level recommendation → not a MUST violation → CONCERN
  "Harmless replay" argument → partially valid, not a complete dismissal
Node 8 — Arbiter verdict:
  CONCERN — Back-channel logout implementation is mostly correct. The jti tracking gap
  (SHOULD, not MUST) is the only finding. Add a short-lived jti cache to prevent
  replay in edge cases. The developer's claim that replays are harmless is mostly true
  but not universally so.
Node 9 — Format: CONCERN; SHOULD-level gap; jti cache recommended
```

---

## Pair[CB4-3.4-618]

**A**
```
Architecture review — multiple concurrent violations:

1. JWKS rollover: signing keys rotated every 3 months. Dual-key window: 2 hours.
   Access token TTL: 1 hour. Refresh token TTL: 30 days.
2. Token exchange policy: when a service needs additional permissions not in its
   current token, it requests token exchange. The exchange server is configured to
   "honor reasonable service requests" for internal services (no explicit scope list).
3. OIDC front-channel logout: iframes to logout URIs on logout, with a 3-second
   timeout. Back-channel logout: not implemented.
```

**B**
```
REPROCESS

Node 1 — Domain split: IETF-RFCS / IETF-RFCS / IETF-RFCS
  JWKS rollover → T-rfc-oauth2 (JWKS lifecycle)
  Token exchange → T-rfc-oauth2 (RFC 8693)
  Logout → T-rfc-oidc (front-channel/back-channel)
Node 2 — Translation: all three in sub-phase 3.4
Node 3 — Plan: F1=rollover-window-vs-TTL; F2=exchange-scope-policy; F3=logout-method
Node 4 — Processing:
  F1: dual-key window 2h ≥ access token TTL 1h → access tokens OK
      refresh token TTL 30 days >> dual-key window 2h → gap
      refresh tokens issued before rollover valid for 30 days, but old key retired after 2h
      services using refresh tokens would fail after 2h; but note: refresh tokens are
      typically opaque, not JWTs — if refresh tokens are opaque, JWKS window applies to
      access tokens only → 2h ≥ 1h → PASS for that element
      If refresh tokens ARE JWTs signed by the same key: 2h << 30 days → CONCERN
      Interpretation: ambiguous; flag as CONCERN (depends on refresh token format)
  F2: "honor reasonable service requests" = no explicit scope policy → REPROCESS
      Exchange server grants what services request without defined scope list
      This is equivalent to unbounded scope widening by informal assessment
      RFC 8693 §7.1 + CWE-269: privilege escalation by exchange without explicit policy
  F3: front-channel logout only; 3-second timeout → CONCERN
      Not a MUST violation; reliability gap; no REPROCESS trigger
Node 5 — Application: CONCERN (F1) + REPROCESS (F2) + CONCERN (F3)
Node 6 — Translation: REPROCESS present → overall REPROCESS
Node 7 — Arbiter: F2 is controlling REPROCESS; F1/F3 secondary CONCERNs
Node 8 — Arbiter verdict:
  REPROCESS — Token exchange policy "honor reasonable service requests" for internal
  services with no explicit scope list is a REPROCESS-level violation. RFC 8693 §7.1
  requires explicit scope governance; informal "reasonable" assessment is not a scope
  policy. This enables unbounded privilege escalation via exchange. Additionally:
  JWKS rollover window (2h) may be shorter than refresh token JWT lifetime (30 days)
  if refresh tokens are signed with the same key (CONCERN, format-dependent). Front-
  channel-only logout carries reliability concerns (CONCERN). Remediate: define explicit
  scope policies in the exchange server; verify refresh token format and extend rollover
  window if applicable; consider adding back-channel logout.
Node 9 — Format: REPROCESS; F2 primary; F1 conditional CONCERN; F3 CONCERN
```

**Reason**
F2 "honor reasonable service requests" with no explicit scope list is REPROCESS — informal policy cannot substitute for explicit RFC 8693 scope governance. F1 is a conditional CONCERN (depends on refresh token format). F3 is CONCERN. REPROCESS governs. Multi-source finding (RFC 8693 + JWKS lifecycle + logout).

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-oauth2 / T-rfc-oidc
Node 4 — Processing:
  F1: 2h window ≥ 1h access TTL → OK for access tokens; if refresh tokens are JWTs:
      2h << 30 days → CONCERN (format-dependent)
  F2: no explicit scope policy, "reasonable requests" granted → REPROCESS (RFC 8693 §7.1)
  F3: front-channel only, 3-sec timeout → reliability CONCERN
Node 8 — Arbiter verdict:
  REPROCESS — Exchange server without an explicit scope policy is the controlling
  violation (RFC 8693 §7.1 + CWE-269). Define explicit permitted-scope lists per client.
  F1 JWKS window may also be inadequate if refresh tokens are JWTs (verify format).
  F3 front-channel logout is a secondary CONCERN.
Node 9 — Format: REPROCESS; F2 primary; secondary findings listed
```

---

## Pair[CB4-3.4-619]

**A**
```
Post-remediation review for a previously flagged OAuth 2.0 implementation.

Prior findings (all addressed):
  - JWKS rollover window was 8 hours vs 24-hour TTL → extended to 72 hours (3× TTL)
  - Back-channel logout was absent → implemented; logout token validated: sig, iss,
    aud, iat (5-min freshness), event claim, sub, jti (Redis set, 10-min TTL)
  - Token exchange scope widening was enabled → policy now restricts exchange to
    scope narrowing only; widening rejected with error=invalid_scope
  - prompt=none lacked id_token_hint → id_token_hint now required; client caches
    sub from previous ID token, passes it as hint on all prompt=none requests

No new architectural changes. Same authorization server, same resource servers.
```

**B**
```
PASS

Node 1 — Domain split: IETF-RFCS / IETF-RFCS / IETF-RFCS (all OIDC/RFC patterns)
Node 2 — Translation: post-remediation review — all findings mapped to prior ext23 patterns
Node 3 — Plan: verify each remediation satisfies the original finding criterion
Node 4 — Processing:
  JWKS rollover: 72h window vs 24h max TTL → 3× TTL safety margin → PASS
    Matches ext23 correct baseline (quarterly rotation, 3× TTL window)
  Back-channel logout: sig + iss + aud + iat-5min + event + sub + jti-Redis-10min → PASS
    jti now tracked with cache sized to logout token freshness window
    Full validation chain per OIDC Back-Channel Logout spec
  Token exchange scope: widening explicitly rejected → RFC 8693 §7.1 satisfied → PASS
    Narrowing permitted → correct least-privilege delegation pattern
  prompt=none + id_token_hint: sub-based hint on all silent requests → PASS
    Session mix-up risk eliminated; binding to expected user established
Node 5 — Application: PASS × 4 remediations verified
Node 6 — Translation: all prior findings resolved; no new violations introduced
Node 7 — Arbiter: comprehensive post-remediation PASS
Node 8 — Arbiter verdict:
  PASS — All four prior findings have been remediated correctly. JWKS rollover window
  (72h) provides 3× the max token TTL safety margin. Back-channel logout now includes
  jti tracking via Redis, closing the replay gap. Token exchange scope policy explicitly
  rejects widening requests. prompt=none flows include id_token_hint binding the silent
  authentication to the expected user. No new issues introduced. No findings.
Node 9 — Format: PASS; each remediated item confirmed; no new findings
```

**Reason**
All four ext23 findings have been correctly remediated: rollover window is now 3× TTL, back-channel logout adds jti tracking, token exchange blocks widening, and prompt=none adds id_token_hint. Post-remediation PASS demonstrates the model can confirm correct state after fix, not just flag violations.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-oidc / T-rfc-oauth2 / T-oauth-oidc
Node 4 — Processing:
  JWKS: 72h ≥ 3× 24h TTL → PASS
  Back-channel logout: all claims + jti-Redis → PASS
  Token exchange: scope widening rejected → PASS
  prompt=none: id_token_hint required and enforced → PASS
Node 8 — Arbiter verdict:
  PASS — All four previously flagged findings resolved. No new violations. Each
  remediation satisfies the applicable RFC and ASVS requirement.
Node 9 — Format: PASS; four items verified; no remaining findings
```

---

## Pair[CB4-3.4-620]

**A**
```
Developer argument during architecture review:

"OIDC front-channel logout is actually fine for our use case. Here's why:
1. All our users use modern browsers (Chrome/Firefox) — SameSite issues don't apply
   to our setup because we use SameSite=None with Secure for cross-site cookies
2. ITP (Intelligent Tracking Prevention) is a Safari thing, and we don't support Safari
3. Ad-blockers won't block logout requests to our own domain
4. We have 99.9% success rate in our testing environment with front-channel logout

Our system handles session termination well enough. The OIDC spec defines front-channel
logout precisely to support our use case. We don't need back-channel logout."
```

**B**
```
CONCERN

Node 1 — Domain split: T-rfc-oidc (OIDC front-channel vs back-channel logout)
Node 2 — Translation: logout method selection — sub-phase 3.4 in scope
Node 3 — Plan: evaluate each developer argument; check whether front-channel gap is MUST or SHOULD
Node 4 — Processing:
  Argument 1: "SameSite=None with Secure works in modern browsers"
    Partially true: SameSite=None allows cross-site cookies in Chrome/Firefox
    Does not address: iframe load failures due to browser security changes, content blockers,
    or enterprise proxy filtering of tracking-shaped requests → argument incomplete
  Argument 2: "ITP is a Safari thing, we don't support Safari"
    Ignores: Firefox Enhanced Tracking Protection (ETP); Chrome Privacy Sandbox; future browser changes
    Unsupported browser claim is a support-scope argument, not a security guarantee
  Argument 3: "Ad-blockers won't block our own domain"
    True for same-domain: first-party requests are typically not blocked
    However: some aggressive content blockers do block iframe requests by URL pattern
    Not a definitive guarantee
  Argument 4: "99.9% success rate in testing"
    Testing environment ≠ production: no enterprise proxies, no aggressive security extensions,
    controlled browser versions; 0.1% = 1 in 1000 users fail to have session terminated
    At scale (100k users): 100 failed logouts per session termination event
  "OIDC spec defines front-channel logout" → TRUE: it is a defined mechanism
    But spec also defines back-channel logout; spec existence ≠ best practice for all deployments
  Overall: developer arguments partially rebut some concerns but not all
  Front-channel logout is not a MUST violation; it is a less-reliable mechanism
  CONCERN remains — reliability gap is real, not fully addressed by arguments
  Not REPROCESS: no MUST-level requirement violated; spec permits front-channel
Node 5 — Application: CONCERN (partially-rebutted but real reliability gap)
Node 6 — Translation: CONCERN — spec-permitted but recommended-to-upgrade
Node 7 — Arbiter: T-rfc-oidc — logout reliability analysis
Node 8 — Arbiter verdict:
  CONCERN — Front-channel logout is a spec-defined mechanism, but carries browser
  reliability risks that the developer's arguments do not fully eliminate. "SameSite=None"
  does not address iframe load failures from enterprise proxies or content security
  extensions. ITP is not exclusively a Safari issue (Firefox ETP exists). Testing
  success does not reflect enterprise production environments. At 100k+ user scale,
  a 0.1% logout failure rate means hundreds of users with active sessions post-logout.
  Front-channel logout is not a MUST violation, but back-channel logout provides
  server-to-server reliability that eliminates these risks entirely. Recommend
  adding back-channel logout as the primary mechanism, front-channel as fallback.
Node 9 — Format: CONCERN; each developer argument evaluated; back-channel recommended
```

**Reason**
Front-channel logout is spec-permitted so not REPROCESS, but the developer arguments do not fully close the reliability gap. Each claim is evaluated individually: SameSite=None / ITP / ad-blockers / testing coverage — each is partially addressed but not definitively resolved. CONCERN is calibrated: spec-permitted but with real gaps. The Stratum D element is evaluating adversarial rebuttals one by one rather than accepting or rejecting wholesale.

**Phrasing-B**
```
CONCERN

Node 1 — Domain split: T-rfc-oidc
Node 4 — Processing:
  "SameSite=None/modern browsers" → incomplete (enterprise proxies, future browser changes)
  "No Safari support" → ignores Firefox ETP and future Chrome Privacy Sandbox changes
  "Own domain ad-blockers" → mostly true but not absolute guarantee
  "99.9% testing" → testing ≠ enterprise production; 0.1% failure is meaningful at scale
  "Spec defines front-channel" → true; but spec also defines back-channel; existence ≠ sufficiency
  Not a MUST violation → CONCERN not REPROCESS
Node 8 — Arbiter verdict:
  CONCERN — Developer arguments partially valid but do not fully address logout
  reliability risks in enterprise/large-scale deployments. Front-channel is spec-permitted,
  not prohibited. Implement back-channel logout as the primary mechanism to eliminate
  browser-dependent reliability gaps.
Node 9 — Format: CONCERN; each argument addressed; back-channel recommended
```
