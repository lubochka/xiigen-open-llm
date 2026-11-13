# CB4 Stage 3 — Node Pipeline Chain Set Extension 12
# Sequences CB4-3.4-473 through CB4-3.4-480
# Stratum: C — full 9-node pipeline traces for ext18 patterns
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 960 instances

---

## Pair[CB4-3.4-473]

**A:** Full pipeline trace — open dynamic client registration. Authorization server exposes `POST /oauth/register` with no authentication. Any caller can register a client with arbitrary redirect URIs. Team: "RFC 7591 supports open dynamic registration — we are spec-compliant."

**B:**
```
N1 · Domain Splitter
  Input: open dynamic registration, unauthenticated /oauth/register, RFC 7591 claim
  Domain signals: OAuth client lifecycle, redirect_uri trust model, registration security
  Territories opened: T-oauth-oidc (primary), T-rfc-oauth2 (RFC 7591)
  Split: [oauth-client-registration | redirect-uri-trust | spec-compliance-argument]

N2 · Domain Translator
  oauth-client-registration → RFC 7591 §3.1 (initial access token mechanism)
  redirect-uri-trust → V10.2.1 (exact redirect URI registration)
  spec-compliance-argument → "spec supports it" adversarial pattern → flag for N5 rejection

N3 · Scheme Planner
  Primary scheme: RFC7591-registration-security
  Secondary scheme: V10-2-1-redirect-uri-trust
  Conflict check: "spec supports it" argument contradicts ASVS requirement for controlled registration
  Plan: verify RFC 7591 §3.1 options; assess redirect_uri trust impact; reject spec-flexibility argument

N4 · Domain Processor
  RFC 7591 §3.1: endpoint MAY be protected with initial access token — both open and protected are
    syntactically valid options in the RFC
  Security impact of open registration:
    (1) Any party registers clients with arbitrary redirect URIs
    (2) Registered URIs receive authorization codes — open to attacker-controlled endpoints
    (3) Undermines exact-URI-registration security model (V10.2.1)
    (4) RFC 7591 §2 explicitly anticipates initial access token for controlled deployment
  "The spec supports it" argument: RFC supports both modes; choosing the insecure mode is not
    compliance with the security intent of ASVS — RFC flexibility ≠ secure deployment choice
  Evidence gap: unauthenticated dynamic client registration active

N5 · Applier
  Adversarial rejection: "spec supports it" → REJECTED
    RFC 7591 supports both open and protected; ASVS V10 requires controlled registration
    RFC flexibility does not make insecure deployment choice ASVS-compliant
  Apply RFC7591-registration-security: FAIL — no initial access token
  Apply V10-2-1: undermined — any party can register new redirect URIs
  Verdict: REPROCESS

N6 · Result Translator
  Finding: unauthenticated dynamic client registration enabled; redirect_uri trust model undermined
  Adversarial claim rejected: RFC 7591 flexibility does not substitute for security control
  Severity: REPROCESS — absent security control (registration authentication)

N7 · Arbiter Selector
  Primary: ASVS-V10-OAuth (V10.2.1 redirect URI trust)
  Secondary: RFC-OAuth2 (RFC 7591 §3.1 initial access token)

N8 · Arbiter Units
  ASVS-V10-OAuth:
    V10.2.1: exact redirect URI registration undermined by open registration ✗
    Dynamic registration without authentication → REPROCESS
  RFC-OAuth2:
    RFC 7591 §3.1: initial access token absent ✗
    "Spec supports it" argument: RFC supports both modes; open mode is not recommended for
      production deployments where redirect_uri trust is a security boundary — REJECTED
  T-oauth-oidc verdict: REPROCESS
  T-rfc-oauth2 verdict: REPROCESS

N9 · Formatter
  Territory results: T-oauth-oidc=REPROCESS, T-rfc-oauth2=REPROCESS
  Graduation: REPROCESS present → Overall REPROCESS
  Remediation: require initial access token on /oauth/register; issue tokens only after vetting
Overall verdict: REPROCESS
```

**Reason:** RFC 7591 open dynamic registration allows anyone to register clients with arbitrary redirect URIs, undermining the exact-URI-registration security model. "The spec supports it" is rejected — RFC 7591 supports both open and protected registration; choosing the open (insecure) option is not ASVS-compliant. REPROCESS because the registration authentication control is absent.

**Phrasing-B:** Pipeline trace for unauthenticated dynamic client registration. RFC 7591 claim that open registration is spec-compliant. Walk through all 9 nodes showing the verdict.

---

## Pair[CB4-3.4-474]

**A:** Full pipeline trace — protected dynamic client registration. Authorization server: `POST /oauth/register` requires a Bearer initial access token. Initial access tokens issued only to vetted parties after manual approval. Registered `redirect_uris` stored exactly; matched exactly at authorization time. Confidential clients receive `client_secret`; public clients registered without secret.

**B:**
```
N1 · Domain Splitter
  Input: protected dynamic registration, initial access token, exact URI storage/match
  Domain signals: OAuth client lifecycle, registration security, redirect_uri trust
  Territories opened: T-oauth-oidc (primary), T-rfc-oauth2 (RFC 7591)
  Split: [registration-authentication | redirect-uri-exactness | client-credential-issuance]

N2 · Domain Translator
  registration-authentication → RFC 7591 §3.1 (initial access token requirement)
  redirect-uri-exactness → V10.2.1 (exact match requirement)
  client-credential-issuance → confidential vs public client handling

N3 · Scheme Planner
  Primary scheme: RFC7591-protected-registration
  Secondary scheme: V10-2-1-redirect-uri-exact
  Conflict check: none — controls align with requirements
  Plan: verify initial access token mechanism; confirm exact URI storage; verify match at auth

N4 · Domain Processor
  RFC 7591 §3.1: initial access token present ✓ — protected configuration used
  Manual vetting for initial access tokens ✓ — controlled onboarding
  redirect_uris stored exactly (no normalization beyond spec-required) ✓
  Exact match at authorization time ✓ (V10.2.1 satisfied)
  Confidential clients receive client_secret ✓; public clients without secret ✓
  No evidence gaps found

N5 · Applier
  Apply RFC7591-protected-registration: PASS — initial access token required and issued after vetting
  Apply V10-2-1: PASS — exact storage and exact match confirmed
  Apply client-credential-issuance: PASS — correct credential model per client type
  Verdict: PASS

N6 · Result Translator
  Finding: dynamic client registration correctly implemented in protected configuration
  No gaps identified
  Severity: PASS

N7 · Arbiter Selector
  Primary: ASVS-V10-OAuth (V10.2.1 redirect URI trust, dynamic registration)
  Secondary: RFC-OAuth2 (RFC 7591 §3.1)

N8 · Arbiter Units
  ASVS-V10-OAuth:
    V10.2.1: exact redirect URI registration ✓ — PASS
    Dynamic registration controlled (initial access token + vetting) ✓ — PASS
  RFC-OAuth2:
    RFC 7591 §3.1: initial access token used — secure protected configuration ✓ — PASS
  T-oauth-oidc verdict: PASS
  T-rfc-oauth2 verdict: PASS

N9 · Formatter
  Territory results: T-oauth-oidc=PASS, T-rfc-oauth2=PASS
  Graduation: all PASS → Overall PASS
Overall verdict: PASS
```

**Reason:** Initial access token required for registration satisfies RFC 7591 §3.1 protected configuration. Manual vetting ensures only authorized parties receive registration access. Exact URI storage and exact match satisfies V10.2.1. This is the correct, secure deployment of dynamic client registration. PASS.

**Phrasing-B:** Pipeline trace for dynamic registration protected by initial access token with manual vetting. Registered URIs stored and matched exactly. Walk through all 9 nodes to the verdict.

---

## Pair[CB4-3.4-475]

**A:** Full pipeline trace — introspection cache without active invalidation. API gateway caches introspection results for 10 minutes using the token's hash as cache key. Cache entries expire after 10 minutes but are never actively invalidated when a token is revoked. Revocation endpoint is implemented with client authentication.

**B:**
```
N1 · Domain Splitter
  Input: introspection cache 10-min TTL, no active invalidation, revocation endpoint exists
  Domain signals: token revocation effectiveness, introspection caching, post-revocation window
  Territories opened: T-oauth-oidc (primary), T-rfc-oauth2 (RFC 7009 + RFC 7662)
  Split: [revocation-mechanism | introspection-cache-behavior | post-revocation-window]

N2 · Domain Translator
  revocation-mechanism → RFC 7009 §2.1 (revocation endpoint with client auth)
  introspection-cache-behavior → RFC 7662 §2.1 + V10.3 (token revocation effectiveness)
  post-revocation-window → 10 minutes of continued token acceptance after revocation

N3 · Scheme Planner
  Primary scheme: V10-3-revocation-effectiveness
  Secondary scheme: RFC7009-revocation-mechanism
  Conflict check: revocation mechanism exists but cache undermines effectiveness
  Plan: assess revocation mechanism compliance; measure post-revocation window; classify window severity

N4 · Domain Processor
  Revocation endpoint: client auth required ✓ (RFC 7009 §2.1 satisfied)
  Cache behavior: 10-min TTL, no active invalidation
    Revocation event: token marked revoked at authorization server ✓
    Gateway cache: still holds active=true for up to 10 minutes ✗
    Revoked token presented to gateway within 10 minutes of last introspection → accepted
    Effective post-revocation window: 0–10 minutes depending on cache age
  10-minute window classification:
    Revocation infrastructure exists ✓ — not an absence of revocation
    Window is bounded (10 minutes) — CONCERN territory
    10 minutes: longer than 5-minute case, same category
    CONCERN (not REPROCESS) — mechanism exists; window is operational gap

N5 · Applier
  Apply RFC7009-revocation-mechanism: PASS (client auth on revocation endpoint)
  Apply V10-3-cache-effectiveness: CONCERN (10-min post-revocation window; bounded; mechanism exists)
  Verdict: CONCERN (1 CONCERN, 0 REPROCESS)

N6 · Result Translator
  Finding: revocation mechanism correct; 10-minute cache creates bounded post-revocation window
  Severity: CONCERN — revocation exists; cache design creates bounded exposure

N7 · Arbiter Selector
  Primary: ASVS-V10-OAuth (V10.3 revocation effectiveness)
  Secondary: RFC-OAuth2 (RFC 7009 §2.1 + RFC 7662 introspection)

N8 · Arbiter Units
  ASVS-V10-OAuth:
    V10.3: revocation endpoint exists with client auth ✓; cache creates 10-min window ✗
    → CONCERN (revocation not fully effective due to cache; bounded window)
  RFC-OAuth2:
    RFC 7009 §2.1: client auth on revocation ✓ — PASS (mechanism)
    RFC 7662: introspection cache with no active invalidation — CONCERN (same as 5-min analysis)
  T-oauth-oidc verdict: CONCERN
  T-rfc-oauth2 verdict: CONCERN

N9 · Formatter
  Territory results: T-oauth-oidc=CONCERN, T-rfc-oauth2=CONCERN
  Graduation: 0 REPROCESS, 1+ CONCERN → Overall CONCERN
  Remediation: add active cache invalidation on revocation; or use Redis pub/sub to propagate events
Overall verdict: CONCERN
```

**Reason:** Revocation with client auth is correct (PASS for mechanism). 10-minute introspection cache without active invalidation creates a bounded post-revocation window — CONCERN because the revocation infrastructure exists and the window is bounded. REPROCESS would apply only if revocation mechanism were absent entirely.

**Phrasing-B:** Pipeline trace for introspection cache with 10-minute TTL and no active cache invalidation on revocation. Revocation endpoint exists with client auth. Walk through all 9 nodes.

---

## Pair[CB4-3.4-476]

**A:** Full pipeline trace — introspection cache with active invalidation. API gateway: introspection results cached with TTL = `token_exp - current_time`. On revocation, authorization server publishes `revoke:{token_hash}` to Redis pub/sub channel. Gateway subscribes and deletes the cache entry within 100ms. Revoked tokens are not served from cache after invalidation.

**B:**
```
N1 · Domain Splitter
  Input: TTL-based introspection cache, Redis pub/sub invalidation on revocation, 100ms propagation
  Domain signals: token revocation effectiveness, cache management, introspection security
  Territories opened: T-oauth-oidc, T-rfc-oauth2
  Split: [cache-ttl-design | active-invalidation-mechanism | revocation-effectiveness]

N2 · Domain Translator
  cache-ttl-design → exp-based TTL: cache naturally expires with token
  active-invalidation-mechanism → pub/sub propagation within 100ms
  revocation-effectiveness → V10.3: revoked tokens cease to be effective within propagation delay

N3 · Scheme Planner
  Primary scheme: V10-3-revocation-effectiveness
  Secondary scheme: RFC7662-introspection-security
  Conflict check: none — TTL + active invalidation fully cover revocation gap
  Plan: verify TTL design; verify invalidation path; measure effective revocation latency

N4 · Domain Processor
  TTL = token_exp - now: cache never outlives token ✓
  Revocation path: authorization server → Redis pub/sub → gateway cache deletion within 100ms ✓
  100ms propagation: negligible for practical security purposes ✓
  Revoked token after invalidation: not served from cache ✓
  Effective revocation window: ~100ms (propagation delay only) ✓
  No post-revocation acceptance window beyond 100ms ✓

N5 · Applier
  Apply cache-ttl-design: PASS — exp-aligned TTL eliminates stale-token acceptance
  Apply active-invalidation: PASS — pub/sub ensures revocation propagates within 100ms
  Apply V10-3: PASS — revoked tokens cease to be effective within propagation delay
  Verdict: PASS

N6 · Result Translator
  Finding: introspection cache design fully satisfies V10.3; no post-revocation window beyond 100ms
  Severity: PASS

N7 · Arbiter Selector
  Primary: ASVS-V10-OAuth (V10.3)
  Secondary: RFC-OAuth2 (RFC 7662)

N8 · Arbiter Units
  ASVS-V10-OAuth:
    V10.3: revocation effective — 100ms propagation delay negligible ✓ — PASS
  RFC-OAuth2:
    RFC 7662: introspection cache with TTL + active invalidation ✓ — PASS
  T-oauth-oidc verdict: PASS
  T-rfc-oauth2 verdict: PASS

N9 · Formatter
  Territory results: T-oauth-oidc=PASS, T-rfc-oauth2=PASS
  Graduation: all PASS → Overall PASS
Overall verdict: PASS
```

**Reason:** TTL aligned to token expiry prevents stale-token cache hits. Active invalidation via Redis pub/sub ensures revocation propagates within 100ms — negligible for security purposes. This is the correct architecture for introspection caching that fully satisfies V10.3. PASS.

**Phrasing-B:** Pipeline trace for introspection cache with TTL=token-remaining and Redis pub/sub invalidation within 100ms on revocation. Walk through all 9 nodes to confirm PASS.

---

## Pair[CB4-3.4-477]

**A:** Full pipeline trace — redirect_uri case-insensitive comparison. Token endpoint performs `stored_uri.lower() == submitted_uri.lower()` for redirect_uri verification at code exchange. Team: "URIs are case-insensitive — this is correct normalization per RFC 3986."

**B:**
```
N1 · Domain Splitter
  Input: full-URI case-insensitive comparison, RFC 3986 normalization argument, token exchange
  Domain signals: redirect_uri verification, URI normalization, OAuth code exchange security
  Territories opened: T-oauth-oidc (primary), T-rfc-oauth2 (RFC 6749 §4.1.3)
  Split: [redirect-uri-comparison-method | rfc3986-normalization-rules | adversarial-argument]

N2 · Domain Translator
  redirect-uri-comparison-method → RFC 6749 §4.1.3 (exact match requirement)
  rfc3986-normalization-rules → RFC 3986 §2.1: scheme+host case-insensitive; path case-SENSITIVE
  adversarial-argument → "URIs are case-insensitive" → flag for N5 rejection (overgeneralization)

N3 · Scheme Planner
  Primary scheme: RFC6749-redirect-uri-exact-match
  Secondary scheme: RFC3986-normalization-per-component
  Conflict check: full-URI lowercasing violates path case-sensitivity rule
  Plan: apply RFC 3986 component-level analysis; reject overgeneralization; determine verdict

N4 · Domain Processor
  RFC 3986 normalization rules (per-component analysis):
    Scheme (https://) → case-insensitive ✓ — lowercasing scheme is correct
    Host (example.com) → case-insensitive ✓ — lowercasing host is correct
    Path (/oauth/callback) → case-SENSITIVE ✗ — lowercasing path is INCORRECT
  Full-URI lower() impact:
    /oauth/callback == /OAuth/Callback == /OAUTH/CALLBACK (after lowercasing) → false equality
    An attacker registering /OAuth/Callback could submit /oauth/callback and pass comparison
    (or vice versa — depends on which case-variant was registered)
  RFC 6749 §4.1.3: redirect_uri at exchange must match registered URI
  Team argument: "URIs are case-insensitive" → overgeneralization; only scheme and host are
  Practical exploitation: narrow (attacker needs a registered case-variant path)
  → CONCERN (not REPROCESS) — comparison logic incorrect but exploitation is limited

N5 · Applier
  Adversarial rejection: "URIs are case-insensitive" → REJECTED
    RFC 3986: scheme+host are case-insensitive; path is case-SENSITIVE — overgeneralization
  Apply RFC6749-redirect-uri-exact-match: CONCERN — full-URI lowercasing is overly permissive
  Apply RFC3986-normalization: CONCERN — path normalization violated
  Verdict: CONCERN

N6 · Result Translator
  Finding: redirect_uri comparison applies case-insensitivity to full URI including path; path is
    case-sensitive per RFC 3986; comparison logic is incorrect but exploitation requires registered
    case-variant path (narrow window)
  Severity: CONCERN

N7 · Arbiter Selector
  Primary: ASVS-V10-OAuth (V10.2.1 + RFC 6749 §4.1.3)
  Secondary: RFC-OAuth2 (RFC 6749 §4.1.3)

N8 · Arbiter Units
  ASVS-V10-OAuth:
    V10.2.1 / RFC 6749 §4.1.3: exact-match principle violated for path component ✗
    Team argument rejected: RFC 3986 path is case-sensitive — CONCERN
  RFC-OAuth2:
    RFC 6749 §4.1.3: redirect_uri comparison incorrect for path — CONCERN
  T-oauth-oidc verdict: CONCERN
  T-rfc-oauth2 verdict: CONCERN

N9 · Formatter
  Territory results: T-oauth-oidc=CONCERN, T-rfc-oauth2=CONCERN
  Graduation: 0 REPROCESS, 1+ CONCERN → Overall CONCERN
  Remediation: normalize scheme+host to lowercase; compare path case-sensitively; exact-match path
Overall verdict: CONCERN
```

**Reason:** Full-URI case-insensitive comparison is overly permissive — RFC 3986 makes only scheme and host case-insensitive; the path component is case-sensitive. CONCERN (not REPROCESS) because exploitation requires a registered case-variant path, making the practical window narrow. Team argument rejected as overgeneralization.

**Phrasing-B:** Pipeline trace for redirect_uri comparison using full-URI lowercase. Team argues URIs are case-insensitive per RFC 3986. Walk through all 9 nodes showing where the argument fails and the correct verdict.

---

## Pair[CB4-3.4-478]

**A:** Full pipeline trace — internal service scope=admin. Authorization server issues tokens for machine-to-machine (M2M) calls. Internal admin service requests `scope=admin` for all its API calls because it performs different operations (read, write, management) at different times. Resource server enforces scope correctly per endpoint. Team: "The service needs broad scope because its operations vary."

**B:**
```
N1 · Domain Splitter
  Input: internal M2M service, scope=admin for all operations, correct resource-server enforcement
  Domain signals: OAuth scope design, least privilege, M2M token security
  Territories opened: T-oauth-oidc (primary), T-asvs-authorization (V8 least privilege)
  Split: [scope-enforcement-correctness | scope-design-principle | least-privilege-gap]

N2 · Domain Translator
  scope-enforcement-correctness → V8.1.2 (resource server enforces per endpoint) — likely PASS
  scope-design-principle → V8.1.2 (minimum necessary privilege)
  least-privilege-gap → M2M requesting admin scope "because it varies" → design gap

N3 · Scheme Planner
  Primary scheme: V8-1-2-least-privilege
  Secondary scheme: OAuth-scope-minimization
  Conflict check: enforcement is correct; the gap is scope design (what is requested)
  Plan: confirm enforcement PASS; assess least-privilege gap in scope design; apply separate verdict

N4 · Domain Processor
  Resource server scope enforcement: per-endpoint check ✓ (V8 enforcement side: PASS)
  Internal service scope design: scope=admin requested for all calls
    Individual operations: read (scope=read sufficient), write (scope=write sufficient),
      management (scope=admin required for this specific operation only)
    Actual request: scope=admin for everything ✗ — carries more privilege than any single operation
    Least-privilege principle: token should carry minimum scope for each operation
    Compromise impact: stolen admin-scope M2M token → full admin access to all endpoints
    Mitigated by: correct enforcement at resource server ✓
    Gap: design choice of scope breadth, not enforcement mechanism
  → CONCERN (enforcement PASS; scope design gap = CONCERN not REPROCESS)

N5 · Applier
  Apply V8-1-2-enforcement: PASS — per-endpoint scope enforcement correct
  Apply V8-1-2-least-privilege-design: CONCERN — scope=admin for all operations violates minimum
  Team argument "operations vary": does not justify requesting admin scope at all times;
    solution is separate token per operation type, or narrow scopes per-call
  Verdict: CONCERN

N6 · Result Translator
  Finding: scope enforcement correct; scope design for internal service violates least-privilege
  Severity: CONCERN (enforcement PASS; design gap CONCERN; no REPROCESS)

N7 · Arbiter Selector
  Primary: ASVS-V8-AuthZ (V8.1.2 least privilege)
  Secondary: ASVS-V10-OAuth (scope design)

N8 · Arbiter Units
  ASVS-V8-AuthZ:
    V8.1.2: scope enforcement correct ✓; scope requested violates least-privilege ✗
    CONCERN — design gap, not enforcement failure
  ASVS-V10-OAuth:
    Scope minimization: admin scope for all M2M operations too broad ✗ — CONCERN
  T-asvs-authorization verdict: CONCERN
  T-oauth-oidc verdict: CONCERN

N9 · Formatter
  Territory results: T-asvs-authorization=CONCERN, T-oauth-oidc=CONCERN
  Graduation: 0 REPROCESS, 1+ CONCERN → Overall CONCERN
  Remediation: issue separate tokens per operation type; or request minimum scope per-call
Overall verdict: CONCERN
```

**Reason:** Scope enforcement at the resource server is correct (PASS for mechanism). The gap is the M2M service requesting `scope=admin` for all calls when individual operations require narrower scopes. CONCERN because enforcement works correctly — the least-privilege gap is in scope design, not enforcement. Team argument rejected: varying operations justify per-operation tokens, not permanent admin scope.

**Phrasing-B:** Pipeline trace for M2M internal service using scope=admin for all calls. Resource server enforces scope correctly per endpoint. Team argues broad scope is needed because operations vary. Walk through all 9 nodes.

---

## Pair[CB4-3.4-479]

**A:** Combined pipeline trace — open dynamic registration plus scope=admin service. Authorization server: dynamic registration unauthenticated (no initial access token). Internal M2M service uses scope=admin for all calls. Resource server enforces scope per endpoint. Team: "Registration is open because RFC 7591 allows it; admin scope for M2M is convenient."

**B:**
```
N1 · Domain Splitter
  Input: open dynamic registration, M2M scope=admin, correct scope enforcement, dual-argument
  Domain signals: registration security, scope design, least privilege, OAuth security posture
  Territories opened: T-oauth-oidc (primary), T-rfc-oauth2 (RFC 7591), T-asvs-authorization (V8)
  Split: [dynamic-registration-auth | scope-admin-design | enforcement-correctness]

N2 · Domain Translator
  dynamic-registration-auth → RFC 7591 §3.1 + V10.2.1 → registration authentication required
  scope-admin-design → V8.1.2 least privilege → scope design gap
  enforcement-correctness → scope enforcement at resource server → likely PASS

N3 · Scheme Planner
  Primary scheme: RFC7591-registration-security (REPROCESS candidate)
  Secondary scheme: V8-1-2-least-privilege (CONCERN candidate)
  Conflict check: one REPROCESS + one CONCERN → graduation rule applies
  Plan: assess each independently; apply graduation (REPROCESS overrides CONCERN)

N4 · Domain Processor
  Dynamic registration: no initial access token ✗
    Attacker can register clients with arbitrary redirect URIs ✗
    Undermines V10.2.1 exact-URI-registration model ✗
    → REPROCESS (absent required control)
  M2M scope=admin: scope enforcement correct ✓; scope design violates least-privilege ✗
    → CONCERN (design gap; enforcement exists)
  Team arguments:
    "RFC 7591 allows open registration" → REJECTED (RFC supports both; open is insecure choice)
    "Admin scope for M2M is convenient" → REJECTED (convenience does not override least-privilege)

N5 · Applier
  Apply RFC7591-registration: REPROCESS — unauthenticated registration active
  Apply V8-1-2-scope-design: CONCERN — scope=admin for all M2M calls
  Team arguments rejected (both)
  Verdict: REPROCESS (REPROCESS + CONCERN → graduation rule → REPROCESS)

N6 · Result Translator
  Finding: open dynamic registration is REPROCESS (absent control); scope design is CONCERN
  Graduation: REPROCESS overrides CONCERN → Overall REPROCESS
  Severity: REPROCESS

N7 · Arbiter Selector
  Primary: ASVS-V10-OAuth
  Secondary: RFC-OAuth2, ASVS-V8-AuthZ

N8 · Arbiter Units
  ASVS-V10-OAuth:
    V10.2.1: open registration undermines exact-URI model → REPROCESS
    Scope enforcement correct ✓ — PASS (mechanism)
  RFC-OAuth2:
    RFC 7591 §3.1: no initial access token → REPROCESS
  ASVS-V8-AuthZ:
    V8.1.2: scope=admin for M2M → CONCERN
  T-oauth-oidc verdict: REPROCESS
  T-rfc-oauth2 verdict: REPROCESS
  T-asvs-authorization verdict: CONCERN

N9 · Formatter
  Territory results: T-oauth-oidc=REPROCESS, T-rfc-oauth2=REPROCESS, T-asvs-authorization=CONCERN
  Graduation: REPROCESS present → Overall REPROCESS
  Remediation: (1) add initial access token to registration endpoint; (2) narrow M2M scope per op
Overall verdict: REPROCESS
```

**Reason:** Open dynamic registration (REPROCESS) + scope=admin design gap (CONCERN). Graduation rule: 1+ REPROCESS anywhere → Overall REPROCESS. CONCERN for scope design does not compensate for or reduce the REPROCESS for open registration. Both team arguments rejected.

**Phrasing-B:** Pipeline trace for two combined issues: open dynamic registration (no auth) and M2M service using scope=admin. Resource server enforcement is correct. What is the overall verdict when one issue is REPROCESS and one is CONCERN?

---

## Pair[CB4-3.4-480]

**A:** Full pipeline trace — all ext18 patterns correctly implemented. Authorization server: dynamic registration protected by initial access token (manual vetting). redirect_uri comparison: scheme+host lowercased, path compared exactly. Introspection cache: TTL = token remaining + Redis pub/sub invalidation within 100ms. Internal M2M services: separate tokens per operation type with minimum required scope. All prior controls correct (PKCE S256, code-only, CORS restricted, Argon2id, Vault).

**B:**
```
N1 · Domain Splitter
  Input: comprehensive ext18 implementation — all patterns correct
  Domain signals: dynamic registration, URI comparison, introspection cache, scope design,
    plus prior ext controls
  Territories opened: T-oauth-oidc, T-rfc-oauth2, T-asvs-authorization, T-asvs-authentication,
    T-ssdf-process
  Split: [dynamic-registration | redirect-uri-comparison | introspection-cache |
          scope-design | prior-controls]

N2 · Domain Translator
  dynamic-registration → RFC 7591 §3.1 initial access token → protected registration
  redirect-uri-comparison → RFC 3986 per-component + RFC 6749 §4.1.3 → scheme/host lower; path exact
  introspection-cache → RFC 7662 + V10.3 → TTL-aligned + pub/sub invalidation
  scope-design → V8.1.2 → minimum scope per operation
  prior-controls → PKCE S256, code-only, CORS, Argon2id, Vault

N3 · Scheme Planner
  Multi-territory scheme: all territories covered by separate sub-schemes
  Conflict check: none — each control addresses its requirement
  Plan: verify each control independently; aggregate verdicts; apply graduation

N4 · Domain Processor
  Dynamic registration: initial access token required + manual vetting ✓
    redirect_uris stored exactly ✓; matched exactly ✓
  redirect_uri comparison: scheme+host → lowercase ✓ (RFC 3986 case-insensitive components)
    path → exact comparison ✓ (RFC 3986 case-sensitive component)
    → RFC 3986-correct per-component normalization ✓
  Introspection cache: TTL = exp - now ✓; pub/sub invalidation ≤100ms ✓
    V10.3: revoked tokens cease to be effective within 100ms ✓
  M2M scope: separate token per operation type ✓; each token carries minimum required scope ✓
    V8.1.2 least-privilege satisfied ✓
  PKCE: S256 only, no plain ✓; response_type=code only ✓
  CORS: restricted to registered origins ✓
  Argon2id: password hashing ✓
  Vault: secret management ✓
  No evidence gaps found

N5 · Applier
  All controls: PASS
  Verdict: PASS (all territories)

N6 · Result Translator
  Finding: all ext18 patterns implemented correctly; all prior controls correct
  Severity: PASS

N7 · Arbiter Selector
  T-oauth-oidc: ASVS-V10-OAuth
  T-rfc-oauth2: RFC-OAuth2
  T-asvs-authorization: ASVS-V8-AuthZ
  T-asvs-authentication: ASVS-V6-Auth
  T-ssdf-process: SSDF-PW

N8 · Arbiter Units
  ASVS-V10-OAuth:
    V10.2.1: exact redirect URIs + RFC 3986-correct comparison ✓ — PASS
    Dynamic registration: initial access token + vetting ✓ — PASS
    V10.4.6: PKCE S256 no-plain ✓ — PASS
    V10.3: revocation + 100ms cache invalidation ✓ — PASS
  RFC-OAuth2:
    RFC 7591 §3.1: initial access token ✓ — PASS
    RFC 7662: introspection TTL + pub/sub invalidation ✓ — PASS
    RFC 7009 §2.1: revocation with client auth ✓ — PASS
  ASVS-V8-AuthZ:
    V8.1.2: minimum-scope tokens per operation for M2M ✓ — PASS
  ASVS-V6-Auth: Argon2id ✓ — PASS
  SSDF-PW: Vault ✓ — PASS
  All territories: PASS

N9 · Formatter
  Territory results: all PASS
  Graduation: 0 findings → Overall PASS
Overall verdict: PASS
```

**Reason:** All ext18 patterns correctly implemented: RFC 3986-correct per-component URI comparison, protected dynamic registration, TTL+pub/sub introspection cache, minimum-scope M2M tokens, plus all prior controls correct. Every territory returns PASS. Comprehensive multi-territory PASS.

**Phrasing-B:** Pipeline trace for implementation with protected dynamic registration, RFC 3986-correct redirect_uri comparison, TTL+pub/sub introspection cache, minimum-scope M2M tokens, and all prior controls. Walk through all 9 nodes to confirm comprehensive PASS.

---

## File Summary

| Pair ID | Description | Stratum | Pipeline Focus | Verdict |
|---------|-------------|---------|----------------|---------|
| CB4-3.4-473 | Open dynamic registration pipeline trace — "spec supports it" rejected | C/D | T-oauth-oidc + T-rfc-oauth2 | REPROCESS |
| CB4-3.4-474 | Protected dynamic registration (initial access token + vetting) pipeline trace | C | T-oauth-oidc + T-rfc-oauth2 | PASS |
| CB4-3.4-475 | Introspection cache 10-min TTL no active invalidation pipeline trace | C | T-oauth-oidc + T-rfc-oauth2 | CONCERN |
| CB4-3.4-476 | Introspection cache TTL + Redis pub/sub invalidation pipeline trace | C | T-oauth-oidc + T-rfc-oauth2 | PASS |
| CB4-3.4-477 | redirect_uri full-URI case-insensitive comparison pipeline trace — RFC 3986 argument rejected | C/D | T-oauth-oidc + T-rfc-oauth2 | CONCERN |
| CB4-3.4-478 | M2M scope=admin for all calls pipeline trace | C | T-asvs-authorization + T-oauth-oidc | CONCERN |
| CB4-3.4-479 | Open dynamic registration (REPROCESS) + scope=admin (CONCERN) combined trace | C | T-oauth-oidc + T-rfc-oauth2 + T-asvs-authorization | REPROCESS |
| CB4-3.4-480 | All ext18 patterns correctly implemented — comprehensive pipeline PASS | C | Multiple | PASS |
