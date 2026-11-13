# CB4 Stage 3 — Action 3 Pipeline Chain Set
# Node: Multi-node (full 9-node traces) — ext26
# Sequences: CB4-3.4-837 through CB4-3.4-844
# Instances: 16 (8 base pairs × 2 phrasings each)
# Stratum: C (full pipeline traces for ext32 patterns)
# Running total after this file: 1688 instances (844 base pairs)

---

## Pair[CB4-3.4-837]

### A
Full pipeline trace — JWT access tokens without jti claim (RFC 7519 §4.1.7 SHOULD):

An authorization server issues JWT access tokens containing iss, sub, aud, exp, iat, and scope but no jti (JWT ID) claim. The team argues 15-minute TTL provides sufficient revocation granularity and per-token tracking is unnecessary.

### B
Node 1 — Domain Splitter: Input domain identified as T-rfc-jwt / T-oauth-oidc. Sub-domain: JWT access token claim completeness, specifically jti (JWT ID) claim presence. Secondary domain: token revocation granularity. Pipeline route: Node 8 arbiter with RFC 7519 normative keyword calibration.

Node 2 — Domain Translator: Source binding: RFC 7519 §4.1.7 — "The jti (JWT ID) Claim ... SHOULD be assigned." Normative keyword: SHOULD (not MUST). Secondary source: RFC 9068 §2.2 — defines required claims for JWT access tokens (sub, iss, exp, aud, iat, jti optional at RFC 9068 level). ASVS V9.3.1 covers token transport; no MUST for jti in ASVS. Translation: jti is SHOULD-level; absence is a gap but not a MUST violation.

Node 3 — Scheme Planner: Verification scheme: (1) Check normative level of jti requirement — RFC 7519 §4.1.7 SHOULD. (2) Evaluate team's TTL mitigation argument — is it a valid complete substitute? (3) Identify remaining gaps where jti serves purposes TTL cannot replicate. (4) Assign verdict commensurate with SHOULD-level gap. Plan: expect CONCERN if TTL is partial mitigation only; PASS if TTL fully addresses jti use cases; REPROCESS only if a MUST-level violation is also present.

Node 4 — Domain Processor: Processing scenario against RFC 7519 §4.1.7 SHOULD: jti absent — SHOULD not satisfied. Evaluating TTL argument: (a) 15-minute TTL limits the post-compromise window for token validity — this is a real mitigation. (b) TTL does not enable precision revocation of a specific token before its TTL expires — requires jti. (c) TTL does not enable RS-side detection of concurrent use of the same token from different origins — requires jti in a per-presentation blocklist. (d) TTL does not provide per-token audit correlation — requires jti. Conclusion: TTL is real but partial mitigation. Three jti use cases remain unaddressed. No MUST violation identified in RFC 7519 §4.1.7 or RFC 9068 §2.2.

Node 5 — Applier: SHOULD gap confirmed. TTL argument partially accepted, partially rejected. No MUST-level violation identified. Verdict: CONCERN. Reasoning: SHOULD-level requirement not satisfied; three distinct functions of jti remain absent; 15-minute TTL is meaningful mitigation that reduces severity from what it would be with a longer TTL, but does not eliminate the use cases for jti.

Node 6 — Result Translator: CONCERN verdict. Narrative: RFC 7519 §4.1.7 SHOULD jti absent. The team's TTL argument addresses token validity window but not per-token revocation, replay detection, or audit correlation. Three jti capabilities are absent. SHOULD-level: CONCERN not REPROCESS.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-jwt). Also relevant: T-asvs-token (token lifecycle). Primary authority: RFC 7519 §4.1.7. No CWE mapping required for SHOULD-level gap. No SSDF PO.1 escalation path triggered (no MUST violation traceable to training gap).

Node 8 — Arbiter Units: CONCERN. RFC 7519 §4.1.7 SHOULD not satisfied. TTL mitigation is partial: precision revocation, replay detection, and audit correlation all absent. No MUST violated — graduation rule: SHOULD gap + partial mitigation = CONCERN, not REPROCESS.

Node 9 — Formatter: CONCERN. RFC 7519 §4.1.7 SHOULD jti claim absent. Precision revocation of individual tokens before TTL requires out-of-band tracking not anchored to the token. RS-side concurrent-use anomaly detection requires per-jti presentation tracking. Audit correlation across requests requires jti. 15-minute TTL partially mitigates but does not replace jti functionality. Remediation: include UUID v4 jti in all issued JWTs; maintain short-TTL blocklist keyed by jti for active revocation and replay tracking.

### Reason
9-node trace correctly routes jti absence to RFC 7519 §4.1.7 SHOULD, evaluates team's TTL argument as partial mitigation, identifies three residual jti use cases, and produces CONCERN (not REPROCESS). SHOULD-level gap confirmed.

### Phrasing-B
CONCERN — 9-node pipeline trace: JWT jti absent, RFC 7519 §4.1.7 SHOULD, TTL argument partially accepted.

N1: T-rfc-jwt / JWT claim completeness; jti sub-domain.
N2: RFC 7519 §4.1.7 SHOULD jti; RFC 9068 §2.2 does not add MUST for jti beyond required claims.
N3: Verify normative level; evaluate TTL as substitute; identify residual use cases.
N4: SHOULD not met; TTL = partial mitigation; precision revocation + replay detection + audit correlation all absent without jti.
N5: SHOULD gap + partial mitigation = CONCERN.
N6: CONCERN narrative: three jti functions not replaceable by TTL alone.
N7: RFC arbiter (T-rfc-jwt); no PO.1 escalation (no MUST trace).
N8: CONCERN — RFC 7519 §4.1.7 SHOULD gap; 15-min TTL accepted but incomplete.
N9: CONCERN. Add UUID v4 jti + Redis TTL-keyed blocklist.

---

## Pair[CB4-3.4-838]

### A
Full pipeline trace — JWT access tokens with jti (UUID v4) and Redis JTI blocklist:

JWT access tokens include jti (UUID v4 per token). Authorization server maintains a Redis blocklist keyed by jti with TTL equal to the token's remaining lifetime. Resource servers validate jti against the blocklist on every request. Explicit revocation events add jti to blocklist. Single-token precision revocation is functional.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-jwt / T-asvs-token. Sub-domain: JWT jti claim implementation with server-side revocation infrastructure. Pipeline route: Node 8 RFC arbiter with positive compliance confirmation.

Node 2 — Domain Translator: RFC 7519 §4.1.7 SHOULD jti uniqueness requirement. UUID v4 collision probability negligible — uniqueness condition met. RFC 9068 §2.2: required claims all present. Redis blocklist provides server-side token state management beyond TTL. ASVS V10.3: token revocation mechanism present.

Node 3 — Scheme Planner: Verify jti present and unique. Verify blocklist keyed correctly. Verify TTL-matched entries prevent unbounded growth. Verify RS validates against blocklist on every request. Verify precision revocation functional. Expected outcome: PASS if all controls in place.

Node 4 — Domain Processor: jti: UUID v4 — globally unique per token. RFC 7519 §4.1.7 SHOULD uniqueness satisfied. No two tokens share jti. Blocklist: Redis, keyed by jti string. TTL = token's remaining lifetime — entries expire automatically with the token, no unbounded growth. RS: validates jti against blocklist on every request — concurrent-use anomaly detection enabled. Precision revocation: specific jti added on revocation events; only that token revoked. Session continuation unaffected for other tokens. No gaps identified.

Node 5 — Applier: All jti controls verified. RFC 7519 §4.1.7 SHOULD satisfied. Precision revocation functional. Replay detection functional. Audit correlation functional. PASS.

Node 6 — Result Translator: PASS narrative. UUID v4 jti satisfies §4.1.7 SHOULD uniqueness. Redis blocklist with TTL-matched entries correctly implements precision revocation without unbounded growth. RS-side blocklist check enables anomaly detection.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-jwt) + T-asvs-token (V10.3 revocation). Both confirm positive compliance.

Node 8 — Arbiter Units: PASS. RFC 7519 §4.1.7 SHOULD jti satisfied with UUID v4. Precision revocation via Redis JTI blocklist, TTL-matched. RS validates on every request. No findings.

Node 9 — Formatter: PASS. RFC 7519 §4.1.7 SHOULD jti satisfied. UUID v4 per token — uniqueness requirement met. Redis blocklist with TTL-matched entries — precision revocation without growth. RS blocklist check on every request — anomaly detection enabled. V10.3 revocation supported at per-token granularity.

### Reason
9-node trace confirms all jti requirements satisfied: UUID v4 uniqueness, TTL-keyed Redis blocklist, RS validation on every request, and precision revocation. PASS.

### Phrasing-B
PASS — 9-node pipeline trace: JWT jti (UUID v4) + Redis JTI blocklist, all controls verified.

N1: T-rfc-jwt / jti implementation with revocation infrastructure.
N2: RFC 7519 §4.1.7 SHOULD; UUID v4 uniqueness; RFC 9068 §2.2 claims; ASVS V10.3.
N3: Verify jti unique; blocklist keyed+TTL-matched; RS validates; precision revocation functional.
N4: UUID v4 — negligible collision; Redis blocklist TTL=remaining token lifetime; RS validates per-request; precision revocation confirmed.
N5: All controls verified. PASS.
N6: PASS — RFC 7519 §4.1.7 SHOULD satisfied; blocklist correct; anomaly detection enabled.
N7: RFC arbiter + T-asvs-token V10.3.
N8: PASS — all jti requirements met.
N9: PASS. UUID v4 jti + TTL-keyed Redis blocklist + per-request RS validation.

---

## Pair[CB4-3.4-839]

### A
Full pipeline trace — Authorization codes with 24-hour lifetime (RFC 6749 §4.1.2):

An authorization server issues authorization codes with a 24-hour expiry. Single-use enforcement is present (codes marked consumed on first exchange). Team argues single-use enforcement makes the lifetime irrelevant for security, since only one exchange is allowed regardless of the validity window.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oauth2 / T-asvs-authentication. Sub-domain: authorization code lifetime requirement. The scenario presents two controls (short lifetime + single-use) where one is absent. Pipeline route: Node 8 RFC arbiter with RFC 6749 §4.1.2 dual-requirement check.

Node 2 — Domain Translator: RFC 6749 §4.1.2: "The authorization code MUST expire shortly after it is issued to mitigate the risk of leaks." MUST-level requirement. "Shortly" interpretation: RFC references 10 minutes as a reference point; ext16 established 10-minute single-use code as boundary of PASS. RFC 6749 §4.1.2 also requires single-use with token revocation on replay. Translation: two independent MUST conditions — (1) short lifetime; (2) single-use with revocation on replay. Both must be met. One does not substitute for the other.

Node 3 — Scheme Planner: Verify (1) lifetime satisfies "shortly" — 24 hours vs minutes. (2) Whether single-use satisfies the lifetime requirement (substitution analysis). (3) Threat model for each requirement: short lifetime = limits interception-and-use window; single-use = prevents replay. (4) Evaluate team's single-use substitution argument. Expected: REPROCESS if lifetime MUST violated even with single-use present.

Node 4 — Domain Processor: Lifetime check: 24 hours is not "shortly." RFC interpretation across all implementations: "shortly" = minutes. RFC 8252 §8.1 examples use 10-minute codes. Ext16 confirmed 10-minute = boundary; 60-second = well within boundary. 24 hours × 60 minutes = 1,440 minutes — not "shortly" by any reasonable interpretation. MUST violated.

Substitution analysis: single-use addresses replay attacks (attacker using the code a second time). Short lifetime addresses interception-and-use attacks (attacker using the code for the first time after interception, before the legitimate user). These are distinct threats. If an attacker intercepts a 24-hour code and uses it first, single-use enforcement fails the legitimate user — not the attacker. Single-use does not eliminate the value of the 24-hour window for an attacker; it only constrains post-first-use replay.

User flexibility argument: returning the next day to complete an authorization flow is a UX preference. OAuth 2.0 flows are designed for completion within a single session. A new authorization request should be initiated for a new session. UX preference does not satisfy a security MUST.

Node 5 — Applier: RFC 6749 §4.1.2 MUST short lifetime violated. 24 hours ≠ "shortly." Single-use argument rejected as substitute for lifetime requirement. Verdict: REPROCESS.

Node 6 — Result Translator: REPROCESS. RFC 6749 §4.1.2 MUST short lifetime violated. 24 hours provides an attacker who intercepts the code a full day to execute their attack. Single-use addresses replay, not interception-and-use. Both controls are required independently.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-oauth2). MUST-level violation. No SSDF PO.1 escalation needed (violation is already REPROCESS).

Node 8 — Arbiter Units: REPROCESS. RFC 6749 §4.1.2 MUST expire shortly: violated. 24 hours is not "shortly." Single-use enforcement addresses a different threat and does not satisfy the lifetime requirement. "User flexibility" UX argument rejected. Short lifetime and single-use are independent requirements — both must be met. Remediation: reduce authorization code lifetime to minutes (≤10 minutes; 60 seconds is PASS).

Node 9 — Formatter: REPROCESS. RFC 6749 §4.1.2 MUST "expire shortly" violated. 24-hour authorization code lifetime is not "shortly" — universally interpreted as minutes. Short lifetime limits interception-and-use window; single-use limits replay. Single-use enforcement does not substitute for short lifetime — they address separate threats. If attacker uses code first, single-use fails the legitimate user rather than blocking the attacker; short lifetime limits the window for this to occur. Remediation: 60-second or 10-minute authorization code lifetime with single-use + replay-triggered revocation.

### Reason
9-node trace correctly identifies two independent RFC 6749 §4.1.2 MUST conditions, rejects the single-use substitution argument (different threat model), and produces REPROCESS. Substitution of one control for the other is the key adversarial pattern addressed.

### Phrasing-B
REPROCESS — 9-node pipeline trace: 24-hour auth code, RFC 6749 §4.1.2 MUST short lifetime violated.

N1: T-rfc-oauth2 / authorization code lifetime; single-use substitution argument present.
N2: RFC 6749 §4.1.2 MUST expire shortly; two independent conditions: lifetime + single-use with revocation.
N3: Verify lifetime against "shortly"; analyze substitution argument; threat model separation.
N4: 24 hours ≠ "shortly"; single-use addresses replay, not interception-and-use; user flexibility = UX preference, not security requirement.
N5: MUST lifetime violated; substitution rejected. REPROCESS.
N6: REPROCESS — 24-hour window enables interception-and-use attack regardless of single-use.
N7: RFC arbiter T-rfc-oauth2; MUST violation confirmed.
N8: REPROCESS — RFC 6749 §4.1.2 MUST violated; both conditions independent; neither substitutes for the other.
N9: REPROCESS. Reduce to ≤10 minutes; maintain single-use + replay revocation as independent layer.

---

## Pair[CB4-3.4-840]

### A
Full pipeline trace — Authorization codes with 60-second lifetime, single-use, replay-triggered revocation:

Authorization codes expire after 60 seconds. Single-use enforcement marks codes as consumed on first exchange; replay attempts return 400 invalid_grant. Replay detection also triggers revocation of all tokens in the associated authorization session.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oauth2 / T-asvs-authentication. Sub-domain: authorization code lifetime and single-use enforcement, both conditions present. Pipeline route: Node 8 RFC arbiter with positive compliance confirmation.

Node 2 — Domain Translator: RFC 6749 §4.1.2: (1) MUST expire shortly; (2) single-use with token revocation on replay detection. Both conditions mapped. 60 seconds: well within "shortly" — minutes-scale. Single-use: consume-on-first-exchange pattern. Replay revocation: all session tokens revoked on replay detection — fulfills §4.1.2 revocation requirement.

Node 3 — Scheme Planner: Verify 60-second lifetime satisfies "shortly." Verify single-use enforcement correct. Verify replay-triggered revocation complete (covers all session tokens). Verify 400 response on replay is correct (invalid_grant). Expected: PASS if both RFC 6749 §4.1.2 conditions satisfied.

Node 4 — Domain Processor: 60-second lifetime: clearly within "shortly." Interception-and-use window: attacker who intercepts code has 60 seconds to construct and send exchange request (requires redirect_uri, client_id, PKCE code_verifier). In practice, this window is not viable for manual attacks; automated attacks blocked by PKCE code_verifier (known only to legitimate client). Single-use: consume-on-first-exchange, 400 on repeat. Replay revocation: ALL tokens associated with the session revoked on replay detection — even if attacker exchanged first, legitimate user's subsequent attempt revokes attacker's tokens. Both §4.1.2 conditions satisfied.

Node 5 — Applier: Both RFC 6749 §4.1.2 MUST conditions satisfied. Short lifetime: 60 seconds. Single-use with revocation: confirmed. PASS.

Node 6 — Result Translator: PASS narrative. 60-second codes satisfy "shortly." Single-use with replay-triggered session revocation satisfies second condition. Both independent requirements met simultaneously.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-oauth2). Positive compliance confirmation. No SSDF PO.1 check required.

Node 8 — Arbiter Units: PASS. RFC 6749 §4.1.2 both MUST conditions satisfied. 60-second lifetime (well within "shortly"). Single-use enforcement with 400 + replay-triggered revocation. No findings.

Node 9 — Formatter: PASS. RFC 6749 §4.1.2 satisfied. 60-second lifetime: interception-and-use window not viable (PKCE adds code_verifier requirement for any exchange). Single-use + replay-triggered revocation: both §4.1.2 conditions met. All tokens for the authorization session revoked on replay detection — attacker's tokens invalidated even if attacker exchanged first.

### Reason
9-node trace confirms both RFC 6749 §4.1.2 MUST conditions satisfied: 60-second lifetime and single-use with replay-triggered revocation. PASS. Note the revocation-on-replay detail: even if attacker exchanges first, legitimate user's attempt revokes all attacker tokens.

### Phrasing-B
PASS — 9-node pipeline trace: 60s auth code + single-use + replay-triggered revocation, both §4.1.2 conditions met.

N1: T-rfc-oauth2 / auth code controls; both conditions present.
N2: RFC 6749 §4.1.2: (1) MUST expire shortly → 60s; (2) single-use + revocation on replay.
N3: Verify 60s ≤ "shortly"; verify single-use; verify replay revocation covers all session tokens.
N4: 60s confirmed within "shortly"; PKCE makes interception-and-use non-viable; single-use enforced; replay revokes all session tokens.
N5: Both MUST conditions met. PASS.
N6: PASS — 60-second lifetime + single-use + revocation on replay.
N7: RFC arbiter T-rfc-oauth2; both conditions positive.
N8: PASS — RFC 6749 §4.1.2 fully satisfied.
N9: PASS. 60-second lifetime; single-use; replay-triggered revocation for all session tokens.

---

## Pair[CB4-3.4-841]

### A
Full pipeline trace — JWT access tokens without sub claim (RFC 9068 §2.2 MUST):

An authorization server issues JWT access tokens for user-initiated flows without a sub claim. Tokens contain iss, aud, exp, iat, jti, and scope but no sub. Team argues sub is a user identifier that should not be exposed in tokens for privacy reasons.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-jwt / T-rfc-oauth2 / T-asvs-authorization. Sub-domain: JWT access token required claims, specifically sub claim absence. Secondary: privacy-driven claim omission argument. Pipeline route: Node 8 RFC arbiter with RFC 9068 §2.2 MUST analysis.

Node 2 — Domain Translator: RFC 9068 §2.2 (JWT Profile for OAuth 2.0 Access Tokens): "The JWT access token MUST contain a sub claim." MUST-level — unconditional for JWT access tokens in user-initiated flows. Sub identifies the principal on whose behalf access was granted. RFC 9068 §2.2 distinction: machine-to-machine (client_credentials) flows may omit sub; user-initiated flows must include sub. Privacy consideration: sub does not need to be a real user identifier — RFC 9068 and OIDC support PPID (Pairwise Pseudonymous Identifier) and opaque sub values.

Node 3 — Scheme Planner: Verify flow type (user-initiated → sub MUST present). Evaluate privacy argument: (a) is the privacy concern valid? (b) is sub omission the correct remediation? (c) does an alternative (opaque sub) satisfy both privacy and MUST? Evaluate RS impacts without sub. Expected: REPROCESS if MUST violated; CONCERN if only SHOULD; PASS only if all claims present.

Node 4 — Domain Processor: Flow type: user-initiated — RFC 9068 §2.2 MUST sub applies. Sub: absent — MUST violated. Privacy argument evaluation: (a) Privacy concern is valid — exposing a human-readable username or email in tokens is a legitimate concern. (b) Remediation is wrong — RFC 9068 and OIDC Core §5.7 support PPID and opaque sub values (e.g., UUID v4) that contain no PII while satisfying the MUST requirement. (c) Sub omission is not a recognized privacy control — it is a MUST violation. Without sub: RS cannot apply per-user rate limiting, per-user data access restrictions, or per-user feature gating. User-level revocation requires sub to identify which tokens belong to a compromised user account. Audit logs cannot answer "which user accessed this resource."

Node 5 — Applier: RFC 9068 §2.2 MUST sub violated in user-initiated flow. Privacy argument rejected as a rationale for MUST omission — opaque PPID addresses privacy without violating MUST. Verdict: REPROCESS.

Node 6 — Result Translator: REPROCESS. RFC 9068 §2.2 MUST sub absent from user-initiated flow tokens. Privacy concern is valid but the correct remediation is an opaque sub (PPID, UUID), not sub omission. Without sub, per-user RS controls and user-level revocation are non-functional.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-jwt) + T-asvs-authorization (per-user authorization controls). MUST-level violation. No PO.1 escalation needed (already REPROCESS).

Node 8 — Arbiter Units: REPROCESS. RFC 9068 §2.2 MUST sub violated. Privacy concern valid but wrong remediation — opaque PPID (UUID v4) satisfies privacy requirements without omitting the MUST claim. Without sub: per-user policies at RS non-functional; user-level revocation non-functional; audit logs user-blind. Remediation: use opaque sub (UUID v4 PPID).

Node 9 — Formatter: REPROCESS. RFC 9068 §2.2 MUST sub absent. Sub MUST be present in JWT access tokens for user-initiated flows. Privacy argument addressed by opaque sub value (UUID v4 PPID): no PII in token, MUST satisfied simultaneously. Without sub: RS cannot enforce per-user authorization policies, cannot support user-level revocation, cannot produce user-correlated audit logs. Remediation: issue JWT access tokens with sub: UUID-v4-based PPID; validate format at RS.

### Reason
9-node trace correctly identifies RFC 9068 §2.2 MUST sub as unconditional for user-initiated flows, rejects the privacy argument (wrong remediation: use opaque PPID, not omission), and documents RS impact. REPROCESS.

### Phrasing-B
REPROCESS — 9-node pipeline trace: JWT sub absent, RFC 9068 §2.2 MUST, privacy argument = wrong remediation.

N1: T-rfc-jwt / JWT required claims; user-initiated flow; privacy argument present.
N2: RFC 9068 §2.2 MUST sub; user-initiated flow confirmed; opaque PPID satisfies privacy + MUST.
N3: Verify flow type; evaluate privacy argument validity and correct remediation; assess RS impact.
N4: User-initiated flow → MUST applies; sub absent → MUST violated; privacy valid but opaque PPID is correct remediation; RS per-user controls and user-level revocation require sub.
N5: MUST violated; privacy argument wrong remediation. REPROCESS.
N6: REPROCESS — RFC 9068 §2.2 MUST; opaque sub resolves privacy concern without omission.
N7: RFC arbiter T-rfc-jwt + T-asvs-authorization; MUST violation confirmed.
N8: REPROCESS — sub absent; RFC 9068 §2.2 MUST; opaque UUID-based PPID is compliant approach.
N9: REPROCESS. Add sub = UUID v4 PPID; no PII; satisfies both privacy and RFC 9068 §2.2 MUST.

---

## Pair[CB4-3.4-842]

### A
Full pipeline trace — JWT access tokens with opaque UUID v4 PPID sub claim:

JWT access tokens include sub: "usr-9f3a7b2c-d411-4e8c-8902-3a5d6e7f8b1c" — a UUID v4-based opaque identifier with no PII. RS validates sub claim format (UUID pattern). Per-user rate limiting applied at RS by sub value. User-level revocation supported (all tokens for a specific sub added to blocklist).

### B
Node 1 — Domain Splitter: Input domain: T-rfc-jwt / T-asvs-authorization. Sub-domain: JWT sub claim implementation with privacy-compliant opaque value. Pipeline route: Node 8 RFC arbiter with positive compliance confirmation.

Node 2 — Domain Translator: RFC 9068 §2.2 MUST sub: present as UUID v4-based PPID. OIDC Core §5.7: PPID support confirmed — sub need not expose PII. Privacy requirement: UUID v4 contains no username, email, real name, or other PII. ASVS V8.1.2: per-user authorization controls at RS functional via sub.

Node 3 — Scheme Planner: Verify sub present and correct format. Verify no PII in sub value. Verify RS uses sub for per-user controls. Verify user-level revocation uses sub. Expected: PASS if RFC 9068 §2.2 satisfied + RS controls functional.

Node 4 — Domain Processor: Sub present: "usr-9f3a7b2c-d411-4e8c-8902-3a5d6e7f8b1c" — UUID v4-based PPID, no PII. RFC 9068 §2.2 MUST satisfied. Privacy: no username, email, real name, or other user-identifiable PII in the sub value — opaque identifier only. RS sub validation: UUID pattern validation at RS. Per-user rate limiting: functional by sub. User-level revocation: all tokens for a specific sub added to blocklist — functional. Audit log correlation: RS logs sub per request — per-user audit trail. All controls functional without PII exposure.

Node 5 — Applier: RFC 9068 §2.2 MUST satisfied. Privacy maintained through opaque sub. RS per-user controls functional. PASS.

Node 6 — Result Translator: PASS narrative. Opaque sub (UUID v4 PPID) satisfies both RFC 9068 §2.2 MUST and privacy requirements simultaneously. Per-user RS controls functional.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-jwt) + T-asvs-authorization. Both confirm positive compliance.

Node 8 — Arbiter Units: PASS. RFC 9068 §2.2 MUST sub satisfied via UUID v4 PPID. No PII in sub. Per-user rate limiting, user-level revocation, and audit correlation all functional. Privacy + compliance simultaneously achieved. No findings.

Node 9 — Formatter: PASS. RFC 9068 §2.2 MUST sub satisfied. UUID v4 PPID: no PII exposed; RFC 9068 and OIDC PPID support confirmed. RS validates sub format. Per-user rate limiting and user-level revocation functional. Audit correlation per request via sub. Privacy and MUST compliance achieved without conflict through opaque identifier pattern.

### Reason
9-node trace confirms opaque UUID v4 PPID sub satisfies RFC 9068 §2.2 MUST while maintaining privacy — the correct resolution of the privacy-vs-MUST tension. All RS per-user controls functional. PASS.

### Phrasing-B
PASS — 9-node pipeline trace: JWT sub = UUID v4 PPID, RFC 9068 §2.2 MUST satisfied, privacy maintained.

N1: T-rfc-jwt / JWT sub implementation; opaque PPID pattern.
N2: RFC 9068 §2.2 MUST; UUID v4 PPID has no PII; ASVS V8.1.2 per-user controls via sub.
N3: Verify sub present; no PII; RS uses sub for per-user controls; revocation functional.
N4: UUID v4 PPID confirmed; no PII; RS validates format; per-user rate limiting + revocation + audit all functional.
N5: All requirements met. PASS.
N6: PASS — opaque sub satisfies MUST + privacy simultaneously.
N7: RFC arbiter T-rfc-jwt + T-asvs-authorization; positive confirmation.
N8: PASS — RFC 9068 §2.2 MUST satisfied; privacy maintained; all RS controls functional.
N9: PASS. UUID v4 PPID sub; no PII; per-user rate limiting, revocation, audit all functional.

---

## Pair[CB4-3.4-843]

### A
Full pipeline trace — PAR endpoint served over plain HTTP (RFC 9126 §3 MUST TLS):

The Pushed Authorization Request (PAR) endpoint is at http://as.example.com/par — plain HTTP. Clients POST authorization parameters (client_id, response_type, redirect_uri, scope, code_challenge, state) and receive a request_uri. Team argues the PAR endpoint is a server-to-server internal call and TLS is present on the authorization endpoint itself.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oauth2 / T-oauth-oidc. Sub-domain: PAR (Pushed Authorization Request) endpoint transport security. The scenario presents an HTTP PAR endpoint with two defensive arguments: (1) internal network; (2) authorization endpoint has TLS. Pipeline route: Node 8 RFC arbiter with RFC 9126 §3 MUST analysis.

Node 2 — Domain Translator: RFC 9126 §3: "The authorization server MUST require the use of TLS when sending requests to the pushed authorization request endpoint." MUST — unconditional and endpoint-specific. This is not a recommendation derived from general TLS guidance; it is a specific MUST for the PAR endpoint. RFC 9126 §3 applies regardless of whether the call is server-to-server or client-to-AS, and regardless of TLS on other endpoints.

Node 3 — Scheme Planner: Verify PAR endpoint transport. Evaluate "internal network" argument (same analysis as JWKS HTTP, ext25). Evaluate "authorization endpoint TLS covers PAR" argument. Assess on-path attack risk specific to PAR parameters (code_challenge, redirect_uri). Expected: REPROCESS if RFC 9126 §3 MUST violated.

Node 4 — Domain Processor: Transport: plain HTTP on PAR endpoint — RFC 9126 §3 MUST TLS violated.

Internal network argument: rejected. Same category as JWKS over HTTP (ext25) and AS discovery over HTTP (ext28). Internal networks are not immune to on-path attacks: VLAN hopping, compromised internal hosts, misconfigured network segments, insider threats. An adversary on the internal network intercepts the full PAR request body before registration. PAR request body contains: code_challenge, redirect_uri, scope, state, client_id — all parameters that govern token issuance and delivery. An on-path attacker can substitute code_challenge with a known value (known code_verifier) — when the legitimate client presents its code to the authorization endpoint, it presents the real code_verifier which does not match the attacker's substituted code_challenge. The attacker's known verifier matches, and they complete the exchange.

"Authorization endpoint TLS covers PAR" argument: rejected. RFC 9126 §3 is a separate MUST applying to the PAR endpoint specifically. TLS on one endpoint does not satisfy TLS requirements on a different endpoint. Each endpoint's transport security requirement is independent.

Node 5 — Applier: RFC 9126 §3 MUST TLS on PAR endpoint violated. Both defensive arguments rejected. Verdict: REPROCESS.

Node 6 — Result Translator: REPROCESS. RFC 9126 §3 MUST TLS violated. Plain HTTP on PAR endpoint enables parameter injection (code_challenge substitution) by on-path attacker. Internal network argument rejected (same pattern as JWKS HTTP). Per-endpoint TLS is required — authorization endpoint TLS does not satisfy PAR endpoint requirement.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-oauth2). MUST-level violation. No PO.1 escalation needed (already REPROCESS).

Node 8 — Arbiter Units: REPROCESS. RFC 9126 §3 MUST TLS violated on PAR endpoint. "Internal network" rejected — on-path attacks viable within internal networks (same category as JWKS HTTP, ext25). Code_challenge substitution attack enabled by plaintext PAR body in transit. "Authorization endpoint TLS covers PAR" rejected — endpoint-specific MUST requires TLS on that endpoint. Remediation: serve PAR endpoint over HTTPS with TLS ≥ 1.2 and valid CA certificate.

Node 9 — Formatter: REPROCESS. RFC 9126 §3 MUST TLS violated. PAR endpoint at http:// — plaintext transmission of code_challenge, redirect_uri, scope to AS before registration. On-path attacker can intercept and substitute code_challenge (parameter injection attack). Internal network argument rejected: same threat model as JWKS HTTP on internal LAN. TLS on authorization endpoint does not satisfy PAR endpoint's independent MUST TLS requirement. Remediation: move PAR endpoint to HTTPS; maintain client authentication at PAR endpoint per RFC 9126 §2.

### Reason
9-node trace applies the same "internal network rejected" logic as JWKS HTTP (ext25) and AS discovery HTTP (ext28) to PAR HTTP, and additionally identifies the code_challenge parameter injection attack vector specific to PAR. RFC 9126 §3 MUST TLS is endpoint-specific. REPROCESS.

### Phrasing-B
REPROCESS — 9-node pipeline trace: PAR endpoint HTTP, RFC 9126 §3 MUST TLS violated, internal-network argument rejected.

N1: T-rfc-oauth2 / PAR endpoint transport; two defensive arguments present.
N2: RFC 9126 §3 MUST TLS on PAR endpoint; unconditional; endpoint-specific; no internal-network exception.
N3: Verify PAR transport; reject internal-network argument; reject per-endpoint-coverage argument; assess code_challenge injection risk.
N4: Plain HTTP → MUST violated; internal network rejected (same category as JWKS/discovery HTTP); code_challenge substitution attack enabled; authorization endpoint TLS does not extend to PAR endpoint.
N5: MUST TLS violated; both arguments rejected. REPROCESS.
N6: REPROCESS — RFC 9126 §3 MUST; on-path code_challenge injection attack; per-endpoint requirement not transferable.
N7: RFC arbiter T-rfc-oauth2; MUST violation confirmed.
N8: REPROCESS — RFC 9126 §3 MUST TLS violated; all arguments rejected; code_challenge injection viable.
N9: REPROCESS. Move PAR endpoint to HTTPS; maintain client authentication per RFC 9126 §2.

---

## Pair[CB4-3.4-844]

### A
Full pipeline trace — PAR endpoint over HTTPS with client authentication and lifecycle controls:

The PAR endpoint is at https://as.example.com/par — HTTPS with TLS 1.3 and valid CA certificate. All PAR requests require client authentication (confidential clients: client_secret_post or registered method). The endpoint returns request_uri and expires_in. The request_uri is single-use and bound to the client_id.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oauth2 / T-oauth-oidc. Sub-domain: PAR endpoint transport, authentication, and request_uri lifecycle. Pipeline route: Node 8 RFC arbiter with positive compliance confirmation across three RFC 9126 requirement areas.

Node 2 — Domain Translator: RFC 9126 §3: MUST TLS on PAR endpoint. RFC 9126 §2: PAR endpoint must enforce the same client authentication required by the token endpoint. RFC 9126 §4.1: MUST one-time-use for request_uri; client_id binding required. ASVS V10 (PAR flow): request_uri validated against registered client at authorization endpoint. Translation: three independent requirement areas, all present.

Node 3 — Scheme Planner: Verify HTTPS with TLS ≥ 1.2 and valid CA. Verify client authentication enforced at PAR endpoint (matches token endpoint policy). Verify request_uri single-use. Verify client_id binding. Expected: PASS if all three RFC 9126 requirement areas satisfied.

Node 4 — Domain Processor: Transport: HTTPS TLS 1.3 with valid CA certificate — RFC 9126 §3 MUST TLS satisfied. TLS 1.3: meets TLS ≥ 1.2 minimum. Valid CA: prevents MITM by invalid certificate. Client authentication: confidential clients authenticate at PAR endpoint (client_secret_post or registered method) — RFC 9126 §2 satisfied, same as token endpoint. Unauthenticated parties cannot register authorization requests. request_uri controls: single-use enforced (Redis consumed flag per ext25 pattern) — RFC 9126 §4.1 MUST one-time-use satisfied. client_id binding: request_uri can only be used by the client_id that pushed it — binding confirmed. No gaps identified across all three requirement areas.

Node 5 — Applier: RFC 9126 §3 MUST TLS satisfied. RFC 9126 §2 client authentication satisfied. RFC 9126 §4.1 request_uri lifecycle satisfied. PASS.

Node 6 — Result Translator: PASS narrative. All three RFC 9126 requirement areas met: transport security, client authentication, and request_uri lifecycle controls.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-oauth2). All three requirement areas confirm positive compliance.

Node 8 — Arbiter Units: PASS. RFC 9126 §3 MUST TLS: satisfied (HTTPS TLS 1.3, valid CA). RFC 9126 §2 client authentication: satisfied at PAR endpoint. RFC 9126 §4.1 request_uri one-time-use + client_id binding: satisfied. No findings across transport, authentication, and lifecycle requirements.

Node 9 — Formatter: PASS. RFC 9126 §3 MUST TLS satisfied — HTTPS TLS 1.3, valid CA certificate; authorization parameters protected in transit; on-path code_challenge injection not viable. RFC 9126 §2 satisfied — client authentication enforced at PAR endpoint (same policy as token endpoint). RFC 9126 §4.1 satisfied — request_uri single-use and client_id-bound. All PAR transport, authentication, and lifecycle requirements met.

### Reason
9-node trace confirms all three RFC 9126 requirement areas satisfied (transport, client auth, request_uri lifecycle). This is the complete positive baseline for PAR implementation. PASS.

### Phrasing-B
PASS — 9-node pipeline trace: PAR endpoint HTTPS + client authentication + request_uri single-use + client_id binding.

N1: T-rfc-oauth2 / PAR implementation; all three RFC 9126 requirement areas present.
N2: RFC 9126 §3 MUST TLS; §2 client auth = token endpoint policy; §4.1 MUST one-time-use + client_id binding.
N3: Verify HTTPS TLS ≥ 1.2 + valid CA; verify client auth at PAR; verify request_uri single-use + binding.
N4: HTTPS TLS 1.3 valid CA confirmed; client auth at PAR confirmed; request_uri single-use + client_id binding confirmed.
N5: All three RFC 9126 areas satisfied. PASS.
N6: PASS — transport, authentication, and lifecycle all correct.
N7: RFC arbiter T-rfc-oauth2; all three areas positive.
N8: PASS — RFC 9126 §3, §2, §4.1 all satisfied.
N9: PASS. HTTPS TLS 1.3; client auth enforced; request_uri single-use and client_id-bound.
