# CB4 Stage 3 — Node Pipeline Chain Set Extension 17
# Sequences CB4-3.4-603 through CB4-3.4-610
# Stratum: C — full 9-node pipeline traces for ext23 patterns
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 1220 instances

---

## Pair[CB4-3.4-603]

**A:** Full pipeline trace — JWKS rollover window too short. Authorization server rotates keys monthly. Rollover window: 10 minutes (new key added, old key available for 10 minutes, then removed). Access tokens: 30-minute TTL. Team: "Monthly rotation is more frequent than most implementations — our key hygiene is excellent."

**B:**
```
N1 · Domain Splitter
  Input: JWKS 10-minute rollover window, access tokens 30-minute TTL, "frequent rotation = good"
  Domain signals: JWKS rollover timing, outstanding token validity, operational reliability
  Territories opened: T-asvs-token (primary), T-rfc-jwt
  Split: [rollover-window-calculation | outstanding-token-coverage | operational-impact]

N2 · Domain Translator
  rollover-window-calculation → window (10 min) vs max token TTL (30 min) → mismatch ✗
  outstanding-token-coverage → tokens issued before rotation: valid up to 30 min → not covered by 10 min
  operational-impact → valid tokens rejected 10 minutes after key rotation

N3 · Scheme Planner
  Primary scheme: JWKS-rollover-window
  Conflict check: rotation frequency is excellent; window duration is the gap
  Plan: compute mismatch; assess operational impact; classify CONCERN vs REPROCESS

N4 · Domain Processor
  Rollover window: 10 minutes
  Max access token TTL: 30 minutes
  Gap: token issued 1 minute before rotation → still valid for 29 more minutes ✗
    Old key removed after 10 minutes → RS tries to verify → key not in JWKS ✗ → 401 ✗
    Window must be ≥ max token TTL for complete coverage
  Security impact: no security attack enabled (short window doesn't help attackers)
  Operational impact: valid tokens may be rejected during rotation window → CONCERN
  Team argument: "frequent rotation = excellent key hygiene"
    Rotation frequency is independent from window duration ✓ and ✗
    More rotation = more key hygiene ✓; but also more events where window mismatch causes disruption ✗
    → Frequent rotation with inadequate window = frequent valid-token rejections ✗

N5 · Applier
  Apply JWKS-rollover-window: CONCERN — window (10 min) < max TTL (30 min); valid tokens rejected
  Verdict: CONCERN (operational gap; no security attack vector)

N6 · Result Translator
  Finding: rollover window shorter than max token TTL; tokens issued before rotation rejected after key removal
  Severity: CONCERN

N7 · Arbiter Selector: ASVS-V9-Token, RFC-JWT

N8 · Arbiter Units
  ASVS-V9-Token: rollover window < TTL ✗ → CONCERN (operational reliability gap)
  RFC-JWT: JWKS rollover window not aligned with token lifetime ✗ → CONCERN
  T-asvs-token verdict: CONCERN
  T-rfc-jwt verdict: CONCERN

N9 · Formatter
  Graduation: 0 REPROCESS, CONCERN → Overall CONCERN
  Remediation: set rollover window ≥ max access token TTL (30 min min; recommend 2× TTL for safety margin)
Overall verdict: CONCERN
```

**Reason:** 10-minute rollover window with 30-minute access tokens leaves 20 minutes of valid tokens that will fail verification after key removal. Rollover window must be at least as long as the maximum token TTL. CONCERN (operational gap, no security attack). Team's praise for rotation frequency is valid but doesn't address window duration.

**Phrasing-B:** Pipeline trace for JWKS rollover with 10-minute window and 30-minute access tokens. Team: "Monthly rotation is excellent key hygiene." Walk through all 9 nodes.

---

## Pair[CB4-3.4-604]

**A:** Full pipeline trace — OIDC back-channel logout correctly implemented. Authorization server: back-channel logout. Logout token: signed JWT, contains `sub`, `sid`, `iss`, `iat`, `events`, `jti`. RP validation: signature ✓; iss ✓; aud ✓; iat (not in future, within 5 minutes) ✓; jti uniqueness (Redis-backed) ✓; events claim verified ✓. Session terminated for `sub`/`sid`.

**B:**
```
N1 · Domain Splitter
  Input: back-channel logout with full logout token validation, session termination
  Domain signals: OIDC logout propagation, logout token security, server-side session invalidation
  Territories opened: T-oauth-oidc (primary), T-rfc-jwt
  Split: [logout-token-validation | jti-replay-prevention | session-termination]

N2 · Domain Translator
  logout-token-validation → OIDC Back-Channel Logout spec: all required claims validated ✓
  jti-replay-prevention → jti tracked in Redis ✓
  session-termination → sub/sid session invalidated ✓

N3 · Scheme Planner → all PASS expected

N4 · Domain Processor
  Logout token: signed JWT ✓
  sub ✓; sid ✓; iss ✓; iat ✓; events ✓; jti ✓
  Validation:
    Signature ✓; iss ✓; aud ✓
    iat: not in future ✓; within 5 minutes ✓ (recent issuance confirmed)
    jti: Redis lookup → new ✓; stored ✓ (replay prevention)
    events: correct value ✓
  Session termination: sub/sid → user session invalidated ✓
  Server-to-server propagation: reliable; not subject to browser restrictions ✓
  No gaps

N5 · Applier → all PASS

N7-N8: all territories PASS

N9 · Overall PASS
Overall verdict: PASS
```

**Reason:** Back-channel logout with complete logout token validation (sig, iss, aud, iat, jti uniqueness, events) and session termination. Server-to-server reliability. PASS.

**Phrasing-B:** Pipeline trace for OIDC back-channel logout with complete logout token validation and jti replay prevention. Walk through all 9 nodes to confirm PASS.

---

## Pair[CB4-3.4-605]

**A:** Full pipeline trace — token exchange scope widening. Service A has access token with `scope=read:data`. Service A calls token exchange endpoint requesting `scope=admin:manage`. Authorization server issues new token with `scope=admin:manage`. Team: "Token exchange exists so services can get the permissions they need."

**B:**
```
N1 · Domain Splitter
  Input: token exchange for scope widening, "services get needed permissions" argument
  Domain signals: RFC 8693 token exchange, privilege escalation, scope security
  Territories opened: T-oauth-oidc (primary), T-rfc-oauth2 (RFC 8693)
  Split: [token-exchange-scope | privilege-escalation | adversarial-argument]

N2 · Domain Translator
  token-exchange-scope → RFC 8693: subject_token scope vs requested scope
  privilege-escalation → requested scope broader than subject_token → CWE-269 via token exchange
  adversarial-argument → "get needed permissions" → flag for N5 rejection

N3 · Scheme Planner
  Primary scheme: RFC8693-scope-security
  Conflict check: token exchange is valid; scope widening is not
  Plan: verify RFC 8693 §7.1; construct privilege escalation scenario; reject argument

N4 · Domain Processor
  subject_token: scope=read:data (read-only authorization)
  requested scope: admin:manage (administrative operations)
  Scope comparison: admin:manage > read:data → scope widening ✗

  RFC 8693 §7.1 security considerations:
    "The authorization server MUST NOT issue a token with permissions exceeding those
     represented in the subject_token unless additional authorization is explicitly granted"
  Authorization server issuing admin:manage from read:data subject_token:
    No additional authorization explicitly granted ✗
    Privilege escalation via token exchange ✗

  Team argument: "services get permissions they need via exchange"
    Token exchange purpose: narrow scope for delegation, change audience for target resource
    Token exchange is NOT a mechanism to acquire new permissions beyond subject_token ✗
    To get admin:manage: service needs to initiate a new authorization flow with appropriate scope
    → REJECTED

N5 · Applier
  Adversarial rejection: "token exchange = get needed permissions" → REJECTED
  Apply RFC8693-§7.1: REPROCESS — scope widening via exchange violates RFC 8693
  Verdict: REPROCESS

N6 · Result Translator
  Finding: token exchange used for privilege escalation; scope wider than subject_token
  Severity: REPROCESS

N7-N8: all territories REPROCESS

N9 · Overall REPROCESS
Overall verdict: REPROCESS
```

**Reason:** Token exchange issuing `admin:manage` from `read:data` subject token violates RFC 8693 §7.1 — no additional authorization was granted. "Services get permissions via exchange" is rejected: token exchange narrows/transfers scope, it does not manufacture new permissions. REPROCESS.

**Phrasing-B:** Pipeline trace for token exchange: read:data subject token, admin:manage requested. Team: "Token exchange lets services get needed permissions." Walk through all 9 nodes.

---

## Pair[CB4-3.4-606]

**A:** Full pipeline trace — correct token exchange scope narrowing plus DPoP. Service A has access token with `scope=read:data write:data admin:config` for the authorization server. Service A calls token exchange to delegate read access to Service B. Exchange: `subject_token` = Service A's token; `resource` = Service B's URL; `scope` = `read:data`. Output: new token with `scope=read:data`, `aud=["service-b.example.com"]`, DPoP-bound.

**B:**
```
N1 · Domain Splitter
  Input: token exchange with scope narrowing + audience restriction + DPoP binding
  Domain signals: least-privilege delegation, audience restriction, DPoP sender-constraining
  Territories opened: T-oauth-oidc, T-rfc-oauth2 (RFC 8693 + RFC 9449)
  Split: [scope-narrowing | audience-restriction | dpop-binding]

N2 · Domain Translator
  scope-narrowing → read:data subset of original (read:data write:data admin:config) ✓
  audience-restriction → aud=["service-b.example.com"] ✓
  dpop-binding → new token DPoP-bound ✓ (sender-constraining for downstream call)

N3 · Scheme Planner → all PASS expected

N4 · Domain Processor
  Scope: read:data ⊂ {read:data write:data admin:config} ✓ (narrower than subject_token)
  Audience: service-b.example.com ✓ (restricted to delegation target)
  DPoP: exchanged token DPoP-bound ✓ (Service B must prove key possession)
  RFC 8693 §7.1: issued scope ≤ subject_token scope ✓ — compliant
  Result: Service B gets exactly read:data; cannot write data or access admin config ✓
    Cannot present token at other services (aud-restricted) ✓
    Cannot be used without DPoP key (sender-constrained) ✓

N5 · Applier → all PASS

N7-N8: all territories PASS

N9 · Overall PASS
Overall verdict: PASS
```

**Reason:** Token exchange for delegation: scope narrowed to `read:data` (subset of original), audience restricted to Service B, DPoP-bound. RFC 8693 §7.1 satisfied (issued scope ≤ subject_token scope). Comprehensive least-privilege delegation. PASS.

**Phrasing-B:** Pipeline trace for token exchange: scope narrowed from broad to read:data, aud restricted to service-b, DPoP-bound exchanged token. Walk through all 9 nodes to confirm PASS.

---

## Pair[CB4-3.4-607]

**A:** Full pipeline trace — front-channel logout plus CWE-306 management endpoint. OIDC logout: front-channel logout via iframes. Additionally: management API endpoint `/admin/audit-logs` returns audit logs without requiring any authentication. Team: "Audit logs are read-only internal operations — front-channel handles logout and audit logs are operational data."

**B:**
```
N1 · Domain Splitter
  Input: front-channel logout + unauthenticated admin audit log endpoint
  Territories opened: T-oauth-oidc, T-cwe-authn
  Split: [front-channel-logout | admin-endpoint-auth | operational-data-sensitivity]

N2 · Domain Translator
  front-channel-logout → OIDC spec defined; browser reliability concern → CONCERN
  admin-endpoint-auth → CWE-306: missing authentication on admin endpoint
  operational-data-sensitivity → audit logs: security-sensitive; reveal user actions, access patterns

N3 · Scheme Planner
  front-channel: CONCERN; admin endpoint: REPROCESS (CWE-306)
  Graduation: REPROCESS + CONCERN → REPROCESS

N4 · Domain Processor
  Front-channel logout: browser reliability concern ✗ → CONCERN (same as ext23 pair 597)
  Admin audit logs endpoint: no authentication ✗
    Audit logs: security-sensitive; reveal who accessed what, when, from where ✗
    Information disclosure to unauthenticated callers ✗
    CWE-306: missing authentication on critical function ✗
    "Read-only internal" → CWE-306 applies to reads of sensitive data ✗ → REPROCESS

N5 · Applier
  front-channel-logout: CONCERN
  admin-audit-logs: REPROCESS (CWE-306)
  Graduation: REPROCESS + CONCERN → REPROCESS

N7-N8: T-oauth-oidc=CONCERN; T-cwe-authn=REPROCESS

N9 · Overall REPROCESS
Overall verdict: REPROCESS
```

**Reason:** Front-channel logout (CONCERN) + unauthenticated admin audit log endpoint (REPROCESS, CWE-306). REPROCESS overrides CONCERN. Audit logs are sensitive operational data even as read-only.

**Phrasing-B:** Pipeline trace for front-channel logout CONCERN plus unauthenticated admin audit log endpoint REPROCESS. What is the overall verdict?

---

## Pair[CB4-3.4-608]

**A:** Full pipeline trace — prompt=none without id_token_hint. Application uses OIDC `prompt=none` for session renewals. Authorization request includes: `client_id`, `redirect_uri`, `scope=openid`, `response_type=code`, `code_challenge`, `state`. No `id_token_hint` included. Team: "prompt=none checks if any user is authenticated — we don't need to specify which user."

**B:**
```
N1 · Domain Splitter
  Input: prompt=none without id_token_hint, "any authenticated user" argument
  Domain signals: OIDC session check, user binding, session mix-up risk
  Territories opened: T-oauth-oidc, T-asvs-session
  Split: [prompt-none-mechanism | id-token-hint-purpose | session-mixup-risk]

N2 · Domain Translator
  prompt-none-mechanism → OIDC Core: silent check ✓; PKCE present ✓
  id-token-hint-purpose → binds silent check to expected user; prevents session mix-up
  session-mixup-risk → without id_token_hint: any active user's session may be returned

N3 · Scheme Planner
  Primary scheme: OIDC-prompt-none-binding
  Conflict check: mechanism correct; user binding absent
  Plan: assess session mix-up scenario; classify

N4 · Domain Processor
  prompt=none behavior without id_token_hint:
    Authorization server: returns authorization code for whoever is currently logged in ✓
    Multi-user device scenario:
      User A logged into authorization server; User B opens the application
      User B's SPA sends prompt=none → authorization server returns code for User A ✗
      User B's session gets User A's identity → session mix-up ✗
    id_token_hint: specifies expected subject; authorization server should return login_required
      if the hint subject is not the currently authenticated user ✓

  CONCERN level:
    Session mix-up risk on shared devices ✗
    Not REPROCESS: prompt=none is still functional; the gap is defensive binding
    CONCERN: safety gap in multi-user device scenarios

N5 · Applier
  Apply OIDC-prompt-none-binding: CONCERN — id_token_hint absent; session mix-up possible on shared devices
  Team argument: "any authenticated user is fine" → REJECTED (application may have an expected user)
  Verdict: CONCERN

N7-N8: T-oauth-oidc=CONCERN

N9 · Overall CONCERN
Overall verdict: CONCERN
```

**Reason:** `prompt=none` without `id_token_hint` risks session mix-up on shared devices — the authorization server returns a code for whoever is currently authenticated, which may not be the expected user. CONCERN because the risk is limited to shared-device scenarios and `prompt=none` itself is functional.

**Phrasing-B:** Pipeline trace for OIDC prompt=none without id_token_hint. Team: "We don't need to specify which user — prompt=none checks for any authenticated session." Walk through all 9 nodes.

---

## Pair[CB4-3.4-609]

**A:** Full pipeline trace — JWKS correct rollover with 2× TTL safety margin. Authorization server: signing key rotation quarterly. Access tokens: 60-minute TTL. Rollover window: 180 minutes (3 hours = 3× TTL). New key added to JWKS 30 minutes before rotation begins. Old key retained for 180 minutes after rotation. All outstanding tokens verifiable during rollover.

**B:**
```
N1 · Domain Splitter
  Input: JWKS rollover with 180-minute window, 60-minute TTL, pre-publication, safety margin
  Territories opened: T-asvs-token, T-rfc-jwt
  Split: [window-coverage | pre-publication | safety-margin]

N2 · Domain Translator
  window-coverage → 180 min window ≥ 60 min max TTL ✓ (3× safety margin)
  pre-publication → new key in JWKS 30 min before use ✓ (clients can cache before rotation)
  safety-margin → 3× TTL: accounts for any token issuance at the last moment of old key use ✓

N3 · Scheme Planner → all PASS expected

N4 · Domain Processor
  Worst case: token issued at the last second before rotation → still valid for 60 minutes ✓
  Old key in JWKS for 180 minutes → covers all outstanding valid tokens ✓
  Pre-publication (30 min): clients that cache JWKS have time to update ✓
  Safety margin (3×): accommodates clock skew, caching delays ✓
  No valid token will be rejected during rollover ✓

N5 · Applier → PASS

N7-N8: all territories PASS

N9 · Overall PASS
Overall verdict: PASS
```

**Reason:** 180-minute rollover window (3× the 60-minute max TTL) with 30-minute pre-publication. All outstanding valid tokens are covered. No valid tokens rejected during rotation. PASS.

**Phrasing-B:** Pipeline trace for JWKS rollover with 180-minute window (3× 60-minute TTL) and 30-minute pre-publication. Walk through all 9 nodes to confirm PASS.

---

## Pair[CB4-3.4-610]

**A:** Full pipeline trace — all ext23 patterns correct. JWKS: quarterly rotation, 3-hour window (3× max TTL), pre-publication; back-channel logout with jti validation; token exchange scope-narrowing only; prompt=none with id_token_hint + PKCE; JWT client assertions with jti; all prior controls correct.

**B:**
```
N1 · Domain Splitter
  Input: comprehensive ext23 correct implementation
  Territories: T-oauth-oidc, T-rfc-jwt, T-ssdf-process, T-asvs-token
  Split: all ext23 sub-schemes

N4 · Domain Processor
  JWKS rollover: 3-hour window ≥ max TTL ✓; pre-publication ✓ — PASS
  Back-channel logout: full validation + jti ✓ — PASS
  Token exchange: scope narrowing only ✓ (RFC 8693 §7.1) — PASS
  prompt=none: id_token_hint ✓; PKCE S256 ✓ — PASS
  JWT assertions: jti uniqueness ✓ — PASS
  Prior controls: all correct ✓

N5 · All PASS

N8 · All territories PASS

N9 · Overall PASS
Overall verdict: PASS
```

**Reason:** All ext23 patterns correctly implemented. Comprehensive multi-territory PASS.

**Phrasing-B:** Pipeline trace for JWKS 3× TTL window, back-channel logout with jti, scope-narrowing token exchange, prompt=none with id_token_hint, JWT assertions. Walk through all 9 nodes for comprehensive PASS.

---

## File Summary

| Pair ID | Description | Stratum | Pipeline Focus | Verdict |
|---------|-------------|---------|----------------|---------|
| CB4-3.4-603 | JWKS 10-min window with 30-min TTL — valid tokens rejected after rotation | C | T-asvs-token + T-rfc-jwt | CONCERN |
| CB4-3.4-604 | Back-channel logout complete validation pipeline trace | C | T-oauth-oidc + T-rfc-jwt | PASS |
| CB4-3.4-605 | Token exchange scope widening pipeline trace — "get needed permissions" rejected | C/D | T-oauth-oidc + T-rfc-oauth2 | REPROCESS |
| CB4-3.4-606 | Token exchange scope narrowing + audience restriction + DPoP | C | T-oauth-oidc + T-rfc-oauth2 | PASS |
| CB4-3.4-607 | Front-channel logout CONCERN + unauthenticated admin endpoint REPROCESS | C | T-oauth-oidc + T-cwe-authn | REPROCESS |
| CB4-3.4-608 | prompt=none without id_token_hint — session mix-up risk | C | T-oauth-oidc + T-asvs-session | CONCERN |
| CB4-3.4-609 | JWKS 3× TTL rollover window with 30-min pre-publication | C | T-asvs-token + T-rfc-jwt | PASS |
| CB4-3.4-610 | All ext23 patterns correct — comprehensive pipeline PASS | C | Multiple | PASS |
