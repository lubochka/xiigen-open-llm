# CB4 Stage 3 — Node Pipeline Chain Set ext32
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-993 through CB4-3.4-1000 (8 base pairs × 2 phrasings = 16 instances)
# Full 9-node pipeline traces for all 8 ext38 arbiter patterns
# Running total after this file: 120 + (1000 − 60) × 2 = 2000 instances (1000 base pairs)
# Milestone: 2000 total CB4 instances (1000 base pairs)
# Domains: introspection-active-expired, scope-enlargement-code-exchange, iss-absent-RS, access-token-TTL-long

---

## Pair[CB4-3.4-993-A]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Introspection returns `active: true` for expired token
**Pipeline verdict:** REPROCESS

**A:**
[Node 1 — Domain Splitter] Input classified: OAuth 2.0 token introspection endpoint response behaviour. Security sub-domain: introspection active field semantics, RFC 7662 §2.2. Routed to token status validation branch.

[Node 2 — Domain Translator] Standard terminology: RFC 7662 defines a token introspection protocol by which resource servers query the authorization server for the current status of a token. The `active` field is the central status indicator. RFC 7662 §2.2 defines conditions under which `active` must be true and by implication must be false.

[Node 3 — Scheme Planner] Validation plan: (1) Identify RFC 7662 §2.2 semantics for the active field. (2) Assess "RS checks exp themselves" argument. (3) Determine verdict severity.

[Node 4 — Domain Processor] RFC 7662 §2.2: "Specifically, introspection endpoints MUST include in their responses the `active` claim set to Boolean true if the presented token is currently active ... A token is currently active if ... the token has not expired." The contrapositive: if the token has expired, active MUST NOT be true. Returning active=true for an expired token violates the normative semantics of the active field — the introspection endpoint is designed as the authoritative status oracle.

[Node 5 — Applier] Input: introspection returns active=true for token with exp in the past. Direct RFC 7662 §2.2 violation. "RS can check exp themselves" argument evaluated: this is exactly what introspection is designed to replace. If the RS must independently check exp, introspection provides no security benefit over JWT self-validation. The AS must be the reliable source of truth for active status.

[Node 6 — Result Translator] Finding: RFC 7662 §2.2 semantics violated. active=true for expired token. "RS double-checks" argument rejected. Verdict: REPROCESS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. RFC 7662 §2.2. MUST. Verdict: REPROCESS.

[Node 8 — Arbiter] REPROCESS. RFC 7662 §2.2: introspection active field must accurately reflect token status including expiry. Returning active=true for an expired token violates the status oracle contract. "RS can check exp" is rejected — introspection's purpose is precisely to serve as the authoritative status source.

[Node 9 — Formatter] Verdict: **REPROCESS** | Standard: RFC 7662 §2.2 | Normative level: MUST | Gap: active=true returned for expired token | Remediation: Evaluate exp before responding; return active=false for expired tokens.

**B:**
REPROCESS. RFC 7662 §2.2: the `active` field must accurately reflect the token's current status including expiry; active=true for an expired token violates the normative active-field semantics and makes the introspection endpoint an unreliable status oracle. RSes that trust introspection will accept expired tokens. "RS checks exp themselves" is rejected — introspection is the authoritative source of truth.

**Reason:** RFC 7662 §2.2 MUST violated. active=true for expired token. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 7662 §2.2 active field must be false for expired tokens. Returning active=true is materially incorrect and undermines introspection's role as authoritative token status oracle. REPROCESS.

---

## Pair[CB4-3.4-994-A]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Introspection correctly returns `active: false` for expired token
**Pipeline verdict:** PASS

**A:**
[Node 1 — Domain Splitter] Input classified: Token introspection endpoint correct response for expired token.

[Node 2 — Domain Translator] Standard terminology: introspection evaluates current_time < exp before responding. Expired tokens: active=false. Valid tokens: active=true with claims.

[Node 3 — Scheme Planner] Validation plan: (1) Verify exp evaluated server-side. (2) Verify active=false for expired. (3) Render verdict.

[Node 4 — Domain Processor] RFC 7662 §2.2: active field accurately reflects status. Exp evaluated before response. Expired → active=false. All conditions correct.

[Node 5 — Applier] All requirements satisfied. No violations.

[Node 6 — Result Translator] No violations. PASS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. RFC 7662 §2.2. No violations.

[Node 8 — Arbiter] PASS. RFC 7662 §2.2 satisfied: active correctly reflects token status; active=false for expired tokens.

[Node 9 — Formatter] Verdict: **PASS** | Standard: RFC 7662 §2.2 | active accurately reflects expiry status | No violations.

**B:**
PASS. RFC 7662 §2.2 satisfied: introspection evaluates expiry before responding; expired tokens receive active=false; valid tokens receive active=true with claims. Introspection oracle is trustworthy. No violations found.

**Reason:** RFC 7662 §2.2 satisfied. active accurately reflects token status. PASS.

**Phrasing-B:**
PASS. Introspection endpoint evaluates exp server-side; active=false for expired tokens; active=true only for currently valid, non-revoked tokens. RFC 7662 §2.2 satisfied. PASS.

---

## Pair[CB4-3.4-995-A]

**Territory:** T-rfc-oauth2
**Domain:** Scope enlargement at authorization code exchange
**Pipeline verdict:** REPROCESS

**A:**
[Node 1 — Domain Splitter] Input classified: Authorization code token exchange scope validation. Security sub-domain: scope ceiling enforcement at token exchange, RFC 6749 §4.1.3. Routed to scope validation branch.

[Node 2 — Domain Translator] Standard terminology: the scope parameter in an authorization code token exchange request is optional; if present, it specifies the desired access token scope. RFC 6749 §4.1.3 places an explicit ceiling: the requested scope must not exceed the original authorization grant scope. Input: authorization phase scope=read:profile; token request scope=read:profile write:profile admin:settings.

[Node 3 — Scheme Planner] Validation plan: (1) Identify RFC 6749 §4.1.3 MUST NOT. (2) Evaluate "client knows its needs" argument. (3) Assess impact. (4) Determine verdict severity.

[Node 4 — Domain Processor] RFC 6749 §4.1.3: "scope — OPTIONAL ... The requested scope MUST NOT include any scope not originally granted by the resource owner." This is an unconditional MUST NOT. The authorization grant (read:profile) represents the resource owner's informed consent; the token endpoint has no authority to expand that consent on the basis of the client's post-hoc scope assertion.

[Node 5 — Applier] Input: token request scope includes write:profile + admin:settings not in authorization grant. These are granted in the access token. Direct RFC 6749 §4.1.3 MUST NOT violation. "Client self-reports" argument rejected — client assertions at token exchange cannot override resource owner consent.

[Node 6 — Result Translator] Finding: RFC 6749 §4.1.3 MUST NOT violated. Scope enlargement beyond authorization grant. Consent boundary bypassed. Verdict: REPROCESS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. RFC 6749 §4.1.3. MUST NOT. Verdict: REPROCESS.

[Node 8 — Arbiter] REPROCESS. RFC 6749 §4.1.3 MUST NOT: token exchange scope must not exceed the authorization grant. Granting write:profile + admin:settings when only read:profile was authorized violates user consent boundary unconditionally.

[Node 9 — Formatter] Verdict: **REPROCESS** | Standard: RFC 6749 §4.1.3 | Normative level: MUST NOT | Gap: scope enlargement beyond authorization grant | Remediation: Validate token request scope against authorization code record; return error=invalid_scope for any scope not in original grant.

**B:**
REPROCESS. RFC 6749 §4.1.3 MUST NOT: scope at token exchange must not exceed the original authorization grant. The token endpoint granting write:profile + admin:settings when the resource owner only consented to read:profile violates the consent boundary unconditionally. "Client knows what it needs" does not override MUST NOT. Remediation: validate scope against authorization code record; reject excess scope with error=invalid_scope.

**Reason:** RFC 6749 §4.1.3 MUST NOT violated. Scope enlargement beyond resource owner consent. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 6749 §4.1.3: token request scope MUST NOT exceed original authorization grant. The user consented to read:profile; issuing write:profile + admin:settings bypasses that consent decision. No exception for client self-reporting. REPROCESS.

---

## Pair[CB4-3.4-996-A]

**Territory:** T-rfc-oauth2
**Domain:** Token exchange scope correctly bounded to authorization grant
**Pipeline verdict:** PASS

**A:**
[Node 1 — Domain Splitter] Input classified: authorization code token exchange with scope validation.

[Node 2 — Domain Translator] Standard terminology: token endpoint validates requested scope against stored authorization grant scope. Expansion rejected; subset accepted; omitted scope defaults to full grant scope. error=invalid_scope returned for any excess scope.

[Node 3 — Scheme Planner] Validation plan: (1) Verify scope ceiling enforced. (2) Verify correct error returned. (3) Render verdict.

[Node 4 — Domain Processor] RFC 6749 §4.1.3 MUST NOT: scope validation against grant. Expansion → error=invalid_scope. Downscoping → access token with narrower scope. Omission → full grant scope. All correct.

[Node 5 — Applier] All requirements satisfied. No violations.

[Node 6 — Result Translator] No violations. PASS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. RFC 6749 §4.1.3. No violations.

[Node 8 — Arbiter] PASS. RFC 6749 §4.1.3 MUST NOT satisfied: scope validated against authorization grant; expansion rejected with error=invalid_scope; subset accepted; resource owner consent honoured.

[Node 9 — Formatter] Verdict: **PASS** | Standard: RFC 6749 §4.1.3 | Scope ceiling enforced | No violations.

**B:**
PASS. RFC 6749 §4.1.3 MUST NOT satisfied: token endpoint validates scope against authorization grant; excess scope rejected (error=invalid_scope); downscoping accepted; omission defaults to full grant scope. Resource owner consent boundary upheld. No violations found.

**Reason:** RFC 6749 §4.1.3 MUST NOT satisfied. Scope correctly bounded. PASS.

**Phrasing-B:**
PASS. Token endpoint enforces scope ceiling: any scope not in the original authorization grant is rejected with error=invalid_scope. RFC 6749 §4.1.3 MUST NOT satisfied. Resource owner consent upheld. PASS.

---

## Pair[CB4-3.4-997-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** RS does not validate `iss` claim — any issuer's JWT access token accepted
**Pipeline verdict:** REPROCESS

**A:**
[Node 1 — Domain Splitter] Input classified: Resource server JWT validation — issuer claim handling. Security sub-domain: iss validation at RS, RFC 9068 §4. Routed to JWT claim validation branch.

[Node 2 — Domain Translator] Standard terminology: the `iss` claim identifies the authorization server that issued the token. RFC 9068 §4 requires the RS to validate iss against the expected AS identifier. Input: RS validates sig, aud, exp, typ, scope but skips iss check.

[Node 3 — Scheme Planner] Validation plan: (1) Identify RFC 9068 §4 MUST for iss validation. (2) Evaluate "signature proves source" argument. (3) Determine verdict severity.

[Node 4 — Domain Processor] RFC 9068 §4: the RS MUST validate the iss claim as a required step in JWT access token validation. Two distinct security properties: (A) signature verification: proves the token was not tampered with since issuance; (B) iss validation: proves the token was issued by the expected AS. A JWKS cache poisoning attack or an attacker with access to a different legitimate AS's signing keys can produce a validly signed token with an unauthorized iss. Without iss validation, the RS cannot detect that the token comes from an untrusted source.

[Node 5 — Applier] Input: iss not validated. Any issuer's JWT accepted if signature is valid. Direct RFC 9068 §4 MUST violation. "Signature proves source" rejected: the two checks are complementary, not redundant.

[Node 6 — Result Translator] Finding: RFC 9068 §4 MUST violated. iss not validated. Token-provenance check absent. Verdict: REPROCESS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. RFC 9068 §4. MUST. Verdict: REPROCESS.

[Node 8 — Arbiter] REPROCESS. RFC 9068 §4 MUST: RS must validate iss against expected AS identifier. Signature verification and iss validation serve complementary security properties; omitting iss allows tokens from unauthorized issuers to be accepted. REPROCESS.

[Node 9 — Formatter] Verdict: **REPROCESS** | Standard: RFC 9068 §4 | Normative level: MUST | Gap: iss not validated; any issuer's token accepted | Remediation: Add iss validation against configured AS identifier allowlist as step before key lookup.

**B:**
REPROCESS. RFC 9068 §4 MUST: RS must validate iss against the expected AS identifier. The "signature proves source" argument is rejected: signature verification proves integrity; iss validation proves provenance — both are required. Omitting iss allows unauthorized-issuer tokens to be accepted if their signing keys are valid at the JWKS URL. Remediation: add iss allowlist check before key lookup.

**Reason:** RFC 9068 §4 MUST violated. iss absent from RS validation. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9068 §4 requires iss validation. Signature verification (integrity) ≠ iss validation (provenance); these are complementary, not redundant. RS accepting tokens from any issuer with a valid signature violates §4 MUST. REPROCESS — add iss allowlist check.

---

## Pair[CB4-3.4-998-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** RS validates `iss` against exact allowlist — no unauthorized issuer accepted
**Pipeline verdict:** PASS

**A:**
[Node 1 — Domain Splitter] Input classified: RS iss validation — exact allowlist approach.

[Node 2 — Domain Translator] Standard terminology: RS maintains allowlist ["https://auth.example.com"]. Validation pipeline step 2: assert iss ∈ allowlist (exact string match). Non-matching or absent iss → 401 before key lookup. Allowlist is deployment-configured, not modifiable at runtime.

[Node 3 — Scheme Planner] Validation plan: (1) Verify iss check present and exact-match. (2) Verify correct pipeline ordering. (3) Render verdict.

[Node 4 — Domain Processor] RFC 9068 §4 MUST: iss validated. Exact string match prevents subdomain variants (ext33 pattern). Ordering before key lookup is correct — unknown issuers rejected before JWKS fetch. All requirements satisfied.

[Node 5 — Applier] No violations. PASS.

[Node 6 — Result Translator] No violations. PASS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. RFC 9068 §4. No violations.

[Node 8 — Arbiter] PASS. RFC 9068 §4 MUST satisfied: iss validated against exact allowlist; unauthorized issuers rejected before key lookup.

[Node 9 — Formatter] Verdict: **PASS** | Standard: RFC 9068 §4 | iss exact allowlist check; correct pipeline position | No violations.

**B:**
PASS. RFC 9068 §4 MUST satisfied: RS validates iss claim as exact string match against configured allowlist; non-matching iss rejected with 401 before key lookup; allowlist is deployment-configured. Token provenance verification complete. No violations found.

**Reason:** RFC 9068 §4 MUST satisfied. iss validated with exact-match allowlist. PASS.

**Phrasing-B:**
PASS. RS iss validation: exact string match against allowlist ["https://auth.example.com"]; check positioned before JWKS key lookup; non-matching or absent iss → 401 rejection. RFC 9068 §4 MUST satisfied. PASS.

---

## Pair[CB4-3.4-999-A]

**Territory:** T-asvs-token · T-rfc-oauth2
**Domain:** 24-hour access token TTL — exceeds ASVS V9.3.1 guidance range
**Pipeline verdict:** CONCERN

**A:**
[Node 1 — Domain Splitter] Input classified: access token lifetime configuration for user-facing authorization code flow. Security sub-domain: token TTL guidance, ASVS V9.3.1. Routed to token lifetime validation branch.

[Node 2 — Domain Translator] Standard terminology: access token TTL = 86400 seconds (24 hours). ASVS V9.3.1 provides guidance on expected lifetime ranges. No RFC specifies a maximum TTL in MUST-level terms for access tokens.

[Node 3 — Scheme Planner] Validation plan: (1) Identify ASVS V9.3.1 guidance. (2) Assess severity relative to guidance language. (3) Determine verdict.

[Node 4 — Domain Processor] ASVS V9.3.1: "Verify that access tokens have a limited lifetime (usually 15 minutes to 1 hour)." The guidance qualifier is "usually" — this is indicative language, not MUST. 24 hours is 16–96× above the guidance ceiling. The practical risk is a 24-hour exploitation window for exfiltrated tokens. No RFC establishes a MUST-level maximum TTL. Gap is SHOULD/guidance-level.

[Node 5 — Applier] Input: access token TTL = 86400s. ASVS V9.3.1 guidance not satisfied (guidance says "usually 15 min to 1 hour"). No MUST violation found. SHOULD/guidance gap. Verdict: CONCERN.

[Node 6 — Result Translator] Finding: ASVS V9.3.1 guidance not met. "Usually 15 min to 1 hour" guidance qualifier = SHOULD-level. Verdict: CONCERN.

[Node 7 — Arbiter Selector] OWASP-ASVS arbiter. ASVS V9.3.1. Guidance-level. Verdict: CONCERN.

[Node 8 — Arbiter] CONCERN. ASVS V9.3.1 guidance: access tokens usually 15 minutes to 1 hour. 24-hour TTL is substantially outside this range, creating a prolonged exploitation window for exfiltrated tokens. "Usually" = guidance language, not unconditional MUST. No RFC MUST violation. CONCERN.

[Node 9 — Formatter] Verdict: **CONCERN** | Standard: ASVS V9.3.1 | Normative level: Guidance ("usually") | Gap: 24-hour TTL substantially exceeds 15 min–1 hour guidance range | Recommendation: Reduce TTL to 15 minutes; use refresh token with offline_access + rotation for session continuity.

**B:**
CONCERN. ASVS V9.3.1 guidance: access token TTL "usually 15 minutes to 1 hour." A 24-hour TTL substantially exceeds this range and extends the exploitation window for exfiltrated tokens. The "usually" qualifier makes this guidance-level rather than MUST; no RFC provides a maximum TTL in MUST-level terms. Recommendation: 15-minute access token TTL with refresh token for session continuity.

**Reason:** ASVS V9.3.1 guidance not met. "Usually" = guidance-level language. 24-hour TTL is substantially excessive. CONCERN, not REPROCESS.

**Phrasing-B:**
CONCERN. ASVS V9.3.1 "usually 15 minutes to 1 hour" guidance not satisfied by 24-hour TTL. Guidance language ("usually") = CONCERN-level gap, not MUST violation. 24-hour exploitation window for stolen tokens is meaningful but does not cross MUST threshold. CONCERN.

---

## Pair[CB4-3.4-1000-A]

**Territory:** T-asvs-token · T-rfc-oauth2
**Domain:** Access token TTL within guidance range; refresh token provides session continuity
**Pipeline verdict:** PASS

**A:**
[Node 1 — Domain Splitter] Input classified: access token lifetime + refresh token architecture for user-facing flows.

[Node 2 — Domain Translator] Standard terminology: access token TTL = 900 seconds (15 minutes). Refresh token issued on offline_access scope with 30-day absolute max + rotation per BCP §4.14.1. Client pre-expiry renewal at 60-second buffer.

[Node 3 — Scheme Planner] Validation plan: (1) Verify TTL within ASVS V9.3.1 guidance. (2) Verify refresh token architecture correct. (3) Render verdict.

[Node 4 — Domain Processor] ASVS V9.3.1: 15 minutes is within "usually 15 min to 1 hour." RFC 6749: refresh token requires offline_access. BCP §4.14.1: rotation implemented. 30-day absolute max present. All guidance and requirements satisfied.

[Node 5 — Applier] No violations. No guidance gaps. PASS.

[Node 6 — Result Translator] No violations. PASS.

[Node 7 — Arbiter Selector] OWASP-ASVS arbiter. ASVS V9.3.1. No violations.

[Node 8 — Arbiter] PASS. ASVS V9.3.1 guidance satisfied: 15-minute TTL within guidance range. Refresh token architecture correct: offline_access scope, rotation, 30-day absolute max. No violations.

[Node 9 — Formatter] Verdict: **PASS** | Standard: ASVS V9.3.1 | 15-minute TTL satisfies guidance range | Refresh token architecture correct | No violations.

**B:**
PASS. ASVS V9.3.1 guidance satisfied: 15-minute access token TTL is within the "usually 15 minutes to 1 hour" guidance range. Refresh token provides session continuity per correct architecture: offline_access consent + BCP §4.14.1 rotation + 30-day absolute maximum. Short-lived access tokens + refresh tokens = correct two-token design pattern. No violations found.

**Reason:** ASVS V9.3.1 guidance satisfied. Correct two-token architecture. No violations. PASS.

**Phrasing-B:**
PASS. 15-minute access token TTL satisfies ASVS V9.3.1 guidance. Refresh token (offline_access + rotation + 30-day max) provides correct session continuity. Two-token architecture correct. PASS.
