# CB4 Stage 3 — Multi-Node Pipeline Chain Set ext20
# Sequences CB4-3.4-681 through CB4-3.4-688
# Stratum C — full 9-node pipeline traces for ext26 patterns
# Running total after this file: 1376 instances (688 base pairs)
# Format: Pair[ID] / A / B / Reason / Phrasing-B

---

## Pair[CB4-3.4-681]

**A**
```
Incoming pipeline input:
"Native iOS and Android app uses myapp://oauth/callback as the redirect URI.
Custom URI scheme registered at OS level. Developer cites RFC 8252 as justification
for custom URI scheme use."
```

**B**
```
Pipeline trace — CB4-3.4-681

Node 1 — Domain Splitter:
  Input: native app redirect URI method
  Domain: T-rfc-oauth2 (RFC 8252 OAuth 2.0 for Native Apps)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 8252 §7.1: private-use URI schemes — defined, with hijacking risk noted
  RFC 8252 §7.2: claimed HTTPS redirect URIs — RECOMMENDED
  RFC 8252 §8.1: PKCE S256 REQUIRED for native apps

Node 3 — Scheme Planner:
  Check 1: Is a custom URI scheme used? → YES (myapp://)
  Check 2: Is PKCE mentioned? → NO (not stated in input)
  Check 3: Is there a stronger alternative per RFC 8252? → YES (§7.2)
  Plan: §7.1 defined but §7.2 recommended; assess hijacking risk; note PKCE gap

Node 4 — Domain Processor:
  myapp:// scheme: OS routes to any app registered for that scheme
    Another app with same scheme registered → intercepts authorization response → auth code stolen
  RFC 8252 §7.1 permits but warns; §7.2 recommends claimed HTTPS (stronger binding)
  PKCE S256 not mentioned: RFC 8252 §8.1 REQUIRED for native apps → gap
    If PKCE absent: intercepted auth code directly exchangeable → CONCERN escalates
    Note: even if PKCE present, custom scheme remains a CONCERN per §7.2

Node 5 — Applier:
  CONCERN (custom URI scheme; §7.2 recommended alternative; PKCE gap noted)

Node 6 — Result Translator:
  CONCERN — spec-permitted but not recommended; hijacking risk; PKCE status unclear

Node 7 — Arbiter Selector:
  T-rfc-oauth2 — native app redirect URI

Node 8 — Arbiter:
  CONCERN — Custom URI scheme (myapp://) is spec-defined in RFC 8252 §7.1 but carries
  URI hijacking risk. RFC 8252 §7.2 recommends claimed HTTPS redirect URIs (Universal
  Links / App Links) with OS-level verification. Additionally, PKCE S256 is REQUIRED
  per RFC 8252 §8.1 for native apps; its absence is not confirmed but not stated.
  Implement PKCE S256; consider migrating to claimed HTTPS redirect URIs.

Node 9 — Formatter:
  CONCERN
  Custom URI scheme: hijacking risk; §7.2 alternative recommended
  PKCE S256: required per RFC 8252 §8.1; status not confirmed
```

**Reason**
Full 9-node trace for native app custom URI scheme. Node 3 checks PKCE status (not mentioned in input) and flags it as a gap alongside the URI scheme concern. Node 4 evaluates both dimensions. The pipeline trace shows how the absence of a stated control affects the verdict.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-681 (phrasing B)

Node 1: T-rfc-oauth2 (RFC 8252)
Node 2: §7.1 permitted; §7.2 RECOMMENDED; §8.1 PKCE REQUIRED
Node 4: myapp:// hijacking risk; §7.2 superior; PKCE not stated
Node 8 — Arbiter: CONCERN — Custom URI scheme + PKCE status unclear.
  Implement PKCE S256; consider Universal Links / App Links per §7.2.
Node 9: CONCERN; §7.2 recommended; §8.1 PKCE required
```

---

## Pair[CB4-3.4-682]

**A**
```
Incoming pipeline input:
"Native app: redirect URI is https://app.example.com/oauth/callback (claimed HTTPS).
iOS: Universal Link with AASA file. Android: App Link with assetlinks.json.
PKCE: S256, code_verifier in memory (module variable), cleared after exchange."
```

**B**
```
Pipeline trace — CB4-3.4-682

Node 1 — Domain Splitter:
  Input: native app redirect URI and PKCE review
  Domain: T-rfc-oauth2 (RFC 8252)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 8252 §7.2: claimed HTTPS redirect URIs — RECOMMENDED (OS-verified)
  RFC 8252 §8.1: PKCE S256 REQUIRED for native apps

Node 3 — Scheme Planner:
  Check 1: Claimed HTTPS? → YES (AASA + assetlinks.json)
  Check 2: OS-level verification? → YES
  Check 3: PKCE S256? → YES (in-memory verifier)
  Plan: all RFC 8252 controls present; PASS expected

Node 4 — Domain Processor:
  Claimed HTTPS + AASA/assetlinks.json → OS-verified binding → scheme hijacking eliminated → PASS
  PKCE S256 + in-memory module variable + cleared post-exchange → PASS
  Both iOS and Android: same registered URI → consistent → PASS

Node 5 — Applier:
  PASS (RFC 8252 §7.2 + §8.1 both satisfied)

Node 6 — Result Translator:
  Native app redirect correctly implemented per RFC 8252

Node 7 — Arbiter Selector:
  T-rfc-oauth2 — native app redirect PASS

Node 8 — Arbiter:
  PASS — Claimed HTTPS redirect URIs with OS-level verification (Universal Links / App Links)
  eliminate URI scheme hijacking. PKCE S256 with in-memory verifier satisfies RFC 8252 §8.1.
  No findings.

Node 9 — Formatter:
  PASS
  RFC 8252 §7.2 confirmed (claimed HTTPS + OS verification)
  RFC 8252 §8.1 confirmed (PKCE S256 + in-memory storage)
```

**Reason**
Full pipeline trace for correct native app redirect. Node 3 plans three checks; Node 4 verifies each. The OS-level verification via AASA/assetlinks.json is the key element that upgrades from CONCERN to PASS compared to custom URI schemes.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-682 (phrasing B)

Node 1: T-rfc-oauth2 (RFC 8252)
Node 3: claimed HTTPS + OS verification + PKCE S256 → PASS all three
Node 4: AASA/assetlinks.json → OS-verified → no hijacking; PKCE S256 in-memory → PASS
Node 8 — Arbiter: PASS — RFC 8252 §7.2 + §8.1 both satisfied. No findings.
Node 9: PASS; §7.2 + §8.1 confirmed
```

---

## Pair[CB4-3.4-683]

**A**
```
Incoming pipeline input:
"Authorization codes not marked as used after exchange. Lifetime: 10 minutes.
Developer argues: PKCE and client_secret make unauthorized re-use impractical."
```

**B**
```
Pipeline trace — CB4-3.4-683

Node 1 — Domain Splitter:
  Input: authorization code lifecycle — single-use enforcement
  Domain: T-rfc-oauth2 (RFC 6749 §4.1.2)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 6749 §4.1.2: authorization code MUST be single-use
    Additionally: if code presented after prior use, AS MUST revoke all associated tokens

Node 3 — Scheme Planner:
  Check 1: Codes marked used after exchange? → NO
  Check 2: Does lifetime (10 min) substitute for single-use? → MUST is unconditional
  Check 3: Do PKCE + client_secret satisfy single-use? → independent requirements
  Plan: §4.1.2 MUST assessment; developer argument evaluation

Node 4 — Domain Processor:
  Single-use not enforced: code remains valid for 10 min post-exchange → MUST violation
  Developer argument: "PKCE + client_secret prevent unauthorized re-use"
    Authentication requirements ≠ code lifecycle requirements
    Attacker with code+verifier (or code+secret) can exchange a non-invalidated code
    RFC 6749 §4.1.2 is not conditional on credential theft difficulty
  Replay revocation: impossible without single-use tracking
    Cannot detect reuse → cannot revoke tokens issued for a replayed code
    Two MUST violations: (1) non-invalidation; (2) inability to revoke on replay

Node 5 — Applier:
  REPROCESS (RFC 6749 §4.1.2 MUST single-use; developer argument rejected)

Node 6 — Result Translator:
  REPROCESS — single-use not enforced; replay revocation also impossible

Node 7 — Arbiter Selector:
  T-rfc-oauth2 — authorization code single-use

Node 8 — Arbiter:
  REPROCESS — Authorization codes not marked as used violate RFC 6749 §4.1.2 MUST.
  PKCE and client_secret do not substitute for code invalidation — they are independent
  requirements. Additionally, without single-use tracking, the server cannot fulfill
  the §4.1.2 requirement to revoke all tokens when a used code is re-presented.
  Invalidate codes on first exchange; revoke all associated tokens on replay.

Node 9 — Formatter:
  REPROCESS
  RFC 6749 §4.1.2 MUST: single-use not enforced
  §4.1.2 MUST: replay revocation also impossible without tracking
  Developer argument rejected: credential requirements ≠ lifecycle requirements
```

**Reason**
Full pipeline trace for authorization code not invalidated after use. Node 4 identifies two independent MUST violations (single-use not enforced + replay revocation impossible). The developer argument is traced through the pipeline and rejected with the established credential-vs-lifecycle distinction.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-683 (phrasing B)

Node 1: T-rfc-oauth2 (RFC 6749 §4.1.2)
Node 2: MUST single-use + MUST revoke on replay
Node 4: code not invalidated → MUST (1) violated; replay revocation impossible → MUST (2) violated
  "PKCE + secret prevent re-use" → authentication ≠ lifecycle; independent requirements
Node 8 — Arbiter: REPROCESS — Two MUST violations. Mark codes used; revoke tokens on replay.
Node 9: REPROCESS; §4.1.2 two conditions cited
```

---

## Pair[CB4-3.4-684]

**A**
```
Incoming pipeline input:
"Authorization code: single-use enforced (DB flag), replay → 400, replay detection →
all tokens revoked. Lifetime: 45 seconds from issuance."
```

**B**
```
Pipeline trace — CB4-3.4-684

Node 1 — Domain Splitter:
  Input: authorization code lifecycle review
  Domain: T-rfc-oauth2 (RFC 6749 §4.1.2)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 6749 §4.1.2: MUST single-use; MUST revoke all tokens on replay

Node 3 — Scheme Planner:
  Check 1: Single-use (DB flag)? → YES
  Check 2: Replay → 400? → YES
  Check 3: Replay detection → token revocation? → YES
  Check 4: Lifetime short? → YES (45 seconds)
  Plan: all §4.1.2 requirements present; PASS

Node 4 — Domain Processor:
  DB flag on first exchange → code invalidated → PASS (MUST 1)
  Replay → 400 invalid_grant → correct rejection → PASS
  Replay detection → all tokens revoked → PASS (MUST 2)
  45-second lifetime → short, reduces interception window → PASS

Node 5 — Applier:
  PASS (RFC 6749 §4.1.2 both MUST conditions satisfied)

Node 6 — Result Translator:
  Authorization code lifecycle correct

Node 7 — Arbiter Selector:
  T-rfc-oauth2 — authorization code PASS

Node 8 — Arbiter:
  PASS — Authorization code lifecycle correctly implemented. Codes marked used on first
  exchange (DB flag); replay returns 400. On replay detection, all associated tokens
  revoked per RFC 6749 §4.1.2. 45-second lifetime minimizes interception window. No findings.

Node 9 — Formatter:
  PASS
  §4.1.2 MUST (1): single-use enforced ✓
  §4.1.2 MUST (2): token revocation on replay ✓
  Lifetime: 45 seconds ✓
```

**Reason**
Full pipeline trace for correct authorization code lifecycle. Node 3 plans all four checks and Node 4 verifies each. Both RFC 6749 §4.1.2 MUST conditions (single-use AND token revocation on replay) are explicitly confirmed in the trace.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-684 (phrasing B)

Node 1: T-rfc-oauth2 (RFC 6749 §4.1.2)
Node 3: four checks — single-use flag, 400 on replay, token revocation, short lifetime
Node 4: all four PASS; both MUST conditions satisfied
Node 8 — Arbiter: PASS — §4.1.2 both MUST conditions met. 45s lifetime. No findings.
Node 9: PASS; §4.1.2 confirmed
```

---

## Pair[CB4-3.4-685]

**A**
```
Incoming pipeline input:
"Token endpoint does not validate grant_type against client registration.
Client 'analytics-svc' (registered for authorization_code only) successfully
uses grant_type=client_credentials. Developer: authentication success = authorization."
```

**B**
```
Pipeline trace — CB4-3.4-685

Node 1 — Domain Splitter:
  Input: grant type enforcement at token endpoint
  Domains: T-rfc-oauth2 (RFC 6749 §3.1.1) + T-asvs-authorization
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 6749 §3.1.1: authorization server MUST restrict grant types per client
  CWE-269: privilege escalation via unauthorized grant type

Node 3 — Scheme Planner:
  Check 1: Grant type validated per client registration? → NO
  Check 2: Does "authentication success" = grant type permission? → NO
  Check 3: Is this CWE-269 privilege escalation? → YES
  Plan: §3.1.1 MUST violation; developer argument evaluation

Node 4 — Domain Processor:
  grant_type not checked against registered list → client_credentials accepted for auth_code-only client
  authorization_code flow requires user consent; client_credentials bypasses user consent
    Using client_credentials → service obtains token without user involvement → consent bypass
  "Authentication success = authorization for any grant" → rejected
    Authentication: who you are (client_id + client_secret verified)
    Authorization: what you are permitted to do (grant_types registered)
    Passing authentication ≠ permission to use unregistered grant types
    This conflation is CWE-269 privilege escalation

Node 5 — Applier:
  REPROCESS (RFC 6749 §3.1.1 MUST; CWE-269)

Node 6 — Result Translator:
  REPROCESS — grant type enforcement absent; privilege escalation via grant bypass

Node 7 — Arbiter Selector:
  T-rfc-oauth2 + T-asvs-authorization — grant type bypass

Node 8 — Arbiter:
  REPROCESS — Grant type not validated against client registration violates RFC 6749 §3.1.1.
  analytics-svc bypasses user consent by using client_credentials instead of the registered
  authorization_code flow. CWE-269 privilege escalation. Enforce registered grant_types
  on every token request; reject unregistered grant types with 400 unauthorized_client.

Node 9 — Formatter:
  REPROCESS
  RFC 6749 §3.1.1 MUST — grant type enforcement absent
  CWE-269 — privilege escalation via grant bypass
  Developer argument rejected: authentication ≠ capability authorization
```

**Reason**
Full pipeline trace for grant type enforcement absent. Node 4 traces the consent bypass attack (client_credentials → no user consent) and explicitly rejects the authentication-equals-authorization argument. CWE-269 is cited as the applicable weakness class.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-685 (phrasing B)

Node 1: T-rfc-oauth2 / T-asvs-authorization
Node 2: RFC 6749 §3.1.1 MUST; CWE-269
Node 4: grant type not checked; consent bypass via client_credentials; "auth = authorization" rejected
Node 8 — Arbiter: REPROCESS — §3.1.1 MUST. CWE-269 grant bypass.
  Enforce registered grant_types; reject unregistered.
Node 9: REPROCESS; §3.1.1 + CWE-269
```

---

## Pair[CB4-3.4-686]

**A**
```
Incoming pipeline input:
"Token endpoint validates grant_type against client registration on every request.
'api-gateway' (client_credentials only): authorization_code request → 400.
'user-spa' (authorization_code + refresh_token): client_credentials → 400.
Error: 400 unauthorized_client per RFC 6749 §5.2."
```

**B**
```
Pipeline trace — CB4-3.4-686

Node 1 — Domain Splitter:
  Input: grant type enforcement at token endpoint
  Domain: T-rfc-oauth2 (RFC 6749 §3.1.1)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 6749 §3.1.1: grant type MUST be validated per client registration
  RFC 6749 §5.2: unauthorized_client error for disallowed grant type

Node 3 — Scheme Planner:
  Check 1: Grant type validated per registration? → YES
  Check 2: Machine clients blocked from user-delegation grants? → YES
  Check 3: User clients blocked from machine grants? → YES
  Plan: bidirectional enforcement confirmed; PASS

Node 4 — Domain Processor:
  api-gateway (client_credentials) → authorization_code rejected → PASS
  user-spa (authorization_code + refresh_token) → client_credentials rejected → PASS
  400 unauthorized_client → RFC 6749 §5.2 correct error code → PASS
  Bidirectional: neither machine→user nor user→machine grant bypass possible

Node 5 — Applier:
  PASS (RFC 6749 §3.1.1 + §5.2 satisfied)

Node 6 — Result Translator:
  Grant type enforcement correct; bidirectional restriction verified

Node 7 — Arbiter Selector:
  T-rfc-oauth2 — grant type enforcement PASS

Node 8 — Arbiter:
  PASS — Grant type enforcement correctly implemented in both directions. Machine clients
  cannot use user-delegation grants; user clients cannot use machine grants. Correct error
  code (400 unauthorized_client) returned. RFC 6749 §3.1.1 + §5.2 satisfied. No findings.

Node 9 — Formatter:
  PASS
  §3.1.1: bidirectional grant type enforcement ✓
  §5.2: 400 unauthorized_client ✓
```

**Reason**
Full pipeline trace for correct grant type enforcement. Node 3 plans bidirectional checks; Node 4 verifies both directions. The error code (§5.2 unauthorized_client) is also confirmed, completing the trace across the full applicable spec requirement.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-686 (phrasing B)

Node 1: T-rfc-oauth2 (RFC 6749 §3.1.1 + §5.2)
Node 3: bidirectional — machine→user blocked; user→machine blocked
Node 4: both directions confirmed PASS; 400 unauthorized_client correct
Node 8 — Arbiter: PASS — §3.1.1 bidirectional enforcement + §5.2 error code. No findings.
Node 9: PASS; §3.1.1 + §5.2 confirmed
```

---

## Pair[CB4-3.4-687]

**A**
```
Incoming pipeline input:
"Short auth code lifetime (30 seconds) used as substitute for single-use tracking.
Developer: 'In 30 seconds, replay is impractical. PKCE adds additional protection.'"
```

**B**
```
Pipeline trace — CB4-3.4-687

Node 1 — Domain Splitter:
  Input: authorization code single-use with short-lifetime argument
  Domain: T-rfc-oauth2 (RFC 6749 §4.1.2)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 6749 §4.1.2: MUST single-use — unconditional; no lifetime exception
    If code re-presented after use, AS MUST revoke all associated tokens

Node 3 — Scheme Planner:
  Check 1: Single-use tracking present? → NO
  Check 2: Does 30-second lifetime satisfy §4.1.2 MUST? → MUST unconditional
  Check 3: Is "impractical replay" a valid risk-based exception? → MUST not conditional
  Plan: §4.1.2 MUST assessment; short-lifetime and PKCE arguments

Node 4 — Domain Processor:
  RFC 6749 §4.1.2 MUST: single-use — no exception for short lifetime
  "30 seconds = impractical" → REJECTED:
    (1) MUST is unconditional (established pattern)
    (2) Scripted attacker with intercepted code (e.g., from referrer/proxy log) can
        submit immediately — 30 seconds is sufficient
    (3) PKCE: if attacker has code+verifier, 30-second window is adequate for exchange
  Replay revocation: impossible without tracking → second MUST violation
  Same normative + practical double rejection as PAR and auth code cases

Node 5 — Applier:
  REPROCESS (RFC 6749 §4.1.2 MUST; both arguments rejected)

Node 6 — Result Translator:
  REPROCESS — short lifetime does not satisfy MUST; replay revocation also impossible

Node 7 — Arbiter Selector:
  T-rfc-oauth2 — auth code single-use

Node 8 — Arbiter:
  REPROCESS — RFC 6749 §4.1.2 MUST single-use is unconditional. 30-second lifetime
  reduces replay window but does not satisfy the MUST. A scripted attacker with an
  intercepted code and verifier can complete exchange within seconds. Additionally,
  without tracking, the AS cannot fulfill the §4.1.2 requirement to revoke all tokens
  on replay detection. Implement single-use tracking; invalidate codes on first exchange.

Node 9 — Formatter:
  REPROCESS
  RFC 6749 §4.1.2 MUST: unconditional; no lifetime exception
  Short-lifetime argument rejected: normative + practical grounds
  PKCE argument rejected: independent requirement
  Replay revocation also impossible without tracking
```

**Reason**
Full pipeline trace for the short-lifetime-substitutes-for-single-use argument. Node 4 applies the established double-rejection pattern (normative: MUST unconditional; practical: scripted attacker within 30 seconds). The replay revocation impossibility adds a second independent REPROCESS justification.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-687 (phrasing B)

Node 1: T-rfc-oauth2 (RFC 6749 §4.1.2)
Node 2: MUST single-use (unconditional); MUST revoke on replay
Node 4: 30-second argument rejected; replay revocation impossible without tracking
Node 8 — Arbiter: REPROCESS — §4.1.2 MUST unconditional. Short lifetime doesn't satisfy.
  Replay revocation also impossible. Add single-use tracking.
Node 9: REPROCESS; §4.1.2 two MUST conditions; arguments rejected
```

---

## Pair[CB4-3.4-688]

**A**
```
Incoming pipeline input — full ext26 PASS scenario:
"Native app: claimed HTTPS redirect (Universal Links + AASA; App Links + assetlinks.json),
PKCE S256 in-memory. Auth code: single-use (DB flag), 400 on replay, all tokens revoked
on replay detection, 45-second lifetime. Grant type: validated per registration on every
token request; 400 unauthorized_client for unregistered grant types."
```

**B**
```
Pipeline trace — CB4-3.4-688

Node 1 — Domain Splitter:
  Input: comprehensive ext26 pattern review
  Domains: T-rfc-oauth2 × 3 (RFC 8252, RFC 6749 §4.1.2, RFC 6749 §3.1.1)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 8252 §7.2 + §8.1: claimed HTTPS + PKCE S256 for native apps
  RFC 6749 §4.1.2: single-use + replay revocation for auth codes
  RFC 6749 §3.1.1 + §5.2: grant type per registration + correct error code

Node 3 — Scheme Planner:
  Three patterns; each with sub-requirements:
  Pattern A: claimed HTTPS + OS verification + PKCE S256
  Pattern B: single-use + replay 400 + replay revocation + short lifetime
  Pattern C: grant type validated + bidirectional + correct error code

Node 4 — Domain Processor:
  A: Universal Links + AASA + App Links + assetlinks.json → OS-verified → PASS
     PKCE S256 in-memory → RFC 8252 §8.1 REQUIRED → PASS
  B: DB flag single-use → PASS; replay 400 → PASS; all tokens revoked → PASS
     45-second lifetime → PASS; both §4.1.2 MUST conditions met → PASS
  C: grant type per registration → §3.1.1 PASS
     Bidirectional: confirmed by 400 unauthorized_client for unregistered grants → PASS
     §5.2 error code → PASS

Node 5 — Applier:
  PASS × 3 patterns

Node 6 — Result Translator:
  All ext26 patterns correct

Node 7 — Arbiter Selector:
  Comprehensive multi-RFC PASS

Node 8 — Arbiter:
  PASS — All three ext26 patterns correctly implemented. Native app redirect uses
  claimed HTTPS with OS-level verification and PKCE S256 (RFC 8252 §7.2 + §8.1).
  Authorization codes are single-use with replay detection and token revocation
  (RFC 6749 §4.1.2 both MUST conditions). Grant types validated per client registration
  with correct error response (RFC 6749 §3.1.1 + §5.2). No findings.

Node 9 — Formatter:
  PASS
  RFC 8252 §7.2 + §8.1: native app redirect ✓
  RFC 6749 §4.1.2 (both MUST): auth code lifecycle ✓
  RFC 6749 §3.1.1 + §5.2: grant type enforcement ✓
```

**Reason**
Comprehensive PASS pipeline trace for all three ext26 patterns. Node 3 explicitly lists all sub-requirements before Node 4 verifies them. The trace confirms both RFC 6749 §4.1.2 MUST conditions and the bidirectional grant type enforcement from §3.1.1.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-688 (phrasing B)

Node 1: T-rfc-oauth2 × 3 (RFC 8252, §4.1.2, §3.1.1)
Node 3: three patterns; nine sub-requirements total
Node 4:
  A: OS-verified claimed HTTPS + PKCE S256 → RFC 8252 → PASS
  B: single-use + 400 + revocation + 45s → §4.1.2 → PASS
  C: per-registration + bidirectional + 400 unauthorized_client → §3.1.1 → PASS
Node 8 — Arbiter: PASS — All ext26 patterns correct. No findings.
Node 9: PASS; three patterns confirmed
```
