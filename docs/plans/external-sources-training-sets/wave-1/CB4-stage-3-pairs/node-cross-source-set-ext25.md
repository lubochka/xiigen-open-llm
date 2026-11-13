# CB4 Stage 3 — Action 3 Cross-Source Combination Set
# Node: Multi-source (Nodes 1–9, cross-source arbitration) — ext25
# Sequences: CB4-3.4-845 through CB4-3.4-854
# Instances: 20 (10 base pairs × 2 phrasings each)
# Stratum: B/C (cross-source combinations from ext32 patterns)
# Running total after this file: 1708 instances (854 base pairs)

---

## Pair[CB4-3.4-845]

### A
PAR endpoint over plain HTTP combined with JWT access tokens lacking sub claim:

An authorization server exposes its PAR endpoint at http://as.example.com/par (plain HTTP). The same AS issues JWT access tokens for all user-initiated flows without a sub claim. The team argues the PAR endpoint is an internal server-to-server call and the token design omits sub for user privacy protection.

### B
REPROCESS. Two independent MUST-level violations are present.

Violation 1 — PAR endpoint over HTTP: RFC 9126 §3 MUST TLS on PAR endpoint. HTTP violates this unconditional MUST. The "internal server-to-server" argument is rejected (same category as JWKS HTTP, ext25; AS discovery HTTP, ext28): internal networks are not immune to on-path attacks. An on-path attacker intercepting the PAR request body before registration can substitute the code_challenge parameter, enabling code exchange with a known code_verifier while the legitimate client's verifier fails.

Violation 2 — JWT access tokens without sub: RFC 9068 §2.2 MUST sub present in user-initiated flow tokens. The privacy argument is rejected as a rationale for MUST omission. Opaque PPID (UUID v4) satisfies both privacy and the MUST simultaneously. Sub omission is not a recognized privacy control; it is a MUST violation that disables per-user RS authorization policies and user-level revocation.

Combined impact: an attacker who intercepts a PAR request can substitute code_challenge. After successfully exchanging the authorization code, the attacker receives JWT access tokens that have no sub claim — the RS cannot determine which user's authorization was used, cannot apply per-user restrictions, and cannot revoke the attacker's specific tokens at the user level. The two violations compound: parameter injection at PAR removes PKCE protection; sub absence removes per-user token accountability.

Verdict: REPROCESS on both violations independently. REPROCESS.

### Reason
Two independent RFC MUST violations: RFC 9126 §3 (PAR TLS) and RFC 9068 §2.2 (JWT sub). Both are REPROCESS independently. Combined impact: PAR parameter injection removes PKCE binding; sub absence removes per-user token accountability. REPROCESS.

### Phrasing-B
REPROCESS — PAR HTTP (RFC 9126 §3 MUST TLS) + JWT sub absent (RFC 9068 §2.2 MUST). Two independent MUST violations; compound impact.

Violation 1: PAR HTTP. RFC 9126 §3 MUST TLS. Internal-network argument rejected. On-path attacker can substitute code_challenge before registration — code exchange with known code_verifier succeeds while legitimate client's verifier fails.

Violation 2: sub absent. RFC 9068 §2.2 MUST sub. Privacy argument rejected — opaque UUID PPID satisfies both. Without sub: RS cannot apply per-user policies; user-level revocation non-functional.

Combined: PAR parameter injection removes PKCE binding; sub absence removes per-user token accountability. Attacker who exploits both controls has unconstrained token access that cannot be revoked at user level. REPROCESS on both.

---

## Pair[CB4-3.4-846]

### A
JWT access tokens without jti combined with IP-only rate limiting at the token endpoint:

An authorization server issues JWT access tokens without a jti claim. The same AS applies only IP-based rate limiting to token endpoint requests — no per-client-id throttling. The team argues 15-minute TTL addresses token tracking needs and IP rate limiting protects against brute force.

### B
CONCERN. Two SHOULD/defense-in-depth gaps are present. They do not escalate to REPROCESS.

Gap 1 — jti absent: RFC 7519 §4.1.7 SHOULD jti. The TTL-based argument is partially accepted: 15-minute TTL limits the post-compromise window. However, precision revocation of individual tokens before TTL, RS-side concurrent-use anomaly detection, and per-token audit correlation all require jti. SHOULD-level gap: CONCERN.

Gap 2 — IP-only rate limiting: No per-client-id throttling. A botnet with sufficient IP diversity can attempt effectively unlimited client_secret guesses against a specific client_id while each source IP remains within per-IP limits. No RFC or ASVS MUST mandates a specific rate-limiting mechanism at the token endpoint; this is a defense-in-depth gap. CONCERN.

Graduation rule: two CONCERN-level gaps do not escalate to REPROCESS. Neither gap constitutes a MUST-level violation. No RFC MUST or ASVS MUST is violated by either jti absence or IP-only rate limiting. The combination is more severe than either gap alone, but the correct verdict remains CONCERN — escalation to REPROCESS requires an identified MUST violation, not a count of CONCERNs.

Both remediations are recommended: add UUID v4 jti to all issued tokens with Redis TTL-keyed blocklist; add per-client-id rate limiting to the token endpoint (30 failed attempts per 15-minute sliding window). CONCERN.

### Reason
Two SHOULD/defense-in-depth gaps: RFC 7519 §4.1.7 SHOULD jti, and no per-client-id throttling at token endpoint. Graduation rule: two CONCERNs do not escalate to REPROCESS. No MUST violation identified. CONCERN.

### Phrasing-B
CONCERN — jti absent (RFC 7519 §4.1.7 SHOULD) + IP-only rate limiting (no MUST for specific mechanism). Two CONCERNs ≠ REPROCESS.

jti absent: SHOULD gap; precision revocation + replay detection + audit correlation absent; 15-min TTL is partial mitigation. CONCERN.

IP-only rate limiting: botnet bypass viable; no per-client-id throttling; no RFC/ASVS MUST specifies the mechanism. Defense-in-depth gap. CONCERN.

Graduation rule: REPROCESS requires an identified MUST violation. Two CONCERN-level gaps + no MUST violation = CONCERN. Neither gap individually nor in combination constitutes a MUST. Both remediations recommended independently. CONCERN.

---

## Pair[CB4-3.4-847]

### A
Authorization codes with 24-hour lifetime combined with token endpoint logging client_secret in production:

An authorization server issues authorization codes with a 24-hour expiry (single-use enforced). The same AS's token endpoint logs full POST body — including client_secret — in its production ELK stack with access-controlled but centralized log aggregation.

### B
REPROCESS. Two independent MUST-level violations are present.

Violation 1 — Authorization code 24-hour lifetime: RFC 6749 §4.1.2 MUST expire shortly. 24 hours is not "shortly" — universally interpreted as minutes. Single-use enforcement addresses replay attacks but not interception-and-use attacks. An attacker who intercepts an authorization code has a full day to construct and execute the token exchange request. Both RFC 6749 §4.1.2 conditions are independently MUST — short lifetime and single-use are separate requirements.

Violation 2 — Credential logging: ASVS V8.2.2 MUST NOT log credentials in production. client_secret is a credential. The "access-controlled log store" argument is rejected: the MUST NOT is unconditional; ELK pipeline replication (logstash → elasticsearch → kibana) multiplies the number of systems that hold the credential; access controls are a secondary defense, not a substitute for non-logging of credentials.

Combined impact: a credential logging violation exposes client secrets, which combined with the 24-hour auth code window creates a cascading attack path. If client_secret is leaked from logs, the attacker holds client credentials. If an auth code is also intercepted (e.g., from the 24-hour window), the attacker has both the auth code and the client credential needed to exchange it. Neither single-use enforcement nor access-controlled logs prevent this combination.

Both violations are independently REPROCESS. REPROCESS.

### Reason
Two independent MUST-level violations: RFC 6749 §4.1.2 MUST short lifetime (24h ≠ "shortly") and ASVS V8.2.2 MUST NOT log credentials. Combined: credential leak + long auth code window = cascading attack path. REPROCESS.

### Phrasing-B
REPROCESS — 24-hour auth code (RFC 6749 §4.1.2 MUST expire shortly) + client_secret logging (ASVS V8.2.2 MUST NOT). Two independent MUST violations.

Violation 1: 24h auth code ≠ "shortly." Single-use addresses replay, not interception-and-use. Full day for attacker to execute exchange. RFC 6749 §4.1.2 MUST: violated.

Violation 2: client_secret logged in production ELK. ASVS V8.2.2 MUST NOT. Access-controlled log store rejected: ELK pipeline multiplies exposure surfaces; MUST NOT is unconditional.

Combined: leaked client_secret + 24-hour auth code window = attacker holds both credential and authorization artifact needed for exchange. Cascading impact across both violations. REPROCESS on both independently.

---

## Pair[CB4-3.4-848]

### A
SSDF PO.1 security training absent combined with 24-hour authorization code lifetime in production:

An organization has no formal security training program for developers (SSDF PO.1 gap). The authorization server they built issues authorization codes with a 24-hour lifetime. A code review surfaces the 24-hour code TTL; a developer responds: "The spec says codes should expire shortly — I thought 24 hours counted." The system is deployed in production.

### B
REPROCESS. PO.1 escalation applies.

PO.1 gap alone: CONCERN. The absence of formal security training is a SSDF PO.1 (Prepare the Organization — training and awareness) gap. Developer awareness of OAuth authorization code lifetime requirements — specifically, that "shortly" means minutes not hours — is a fundamental OAuth security requirement. Without PO.1, developers make this category of well-documented error.

Production MUST violation: RFC 6749 §4.1.2 MUST expire shortly is violated. The developer's response ("I thought 24 hours counted") provides direct traceability from the PO.1 gap to the production MUST violation. This is not a coincidental co-occurrence — the training gap is the cause of the MUST violation in production.

PO.1 escalation rule: PO.1 absence alone = CONCERN. PO.1 absence + traceable production MUST violation = REPROCESS. The traceability is present: the RFC 6749 §4.1.2 MUST is violated in production, and the developer comment provides causal evidence that the lack of OAuth security training caused the misconfiguration.

The escalated verdict is REPROCESS because the system has a deployable MUST violation caused by an organizational training gap. Remediations: (1) immediate — reduce authorization code lifetime to ≤10 minutes; (2) systemic — implement SSDF PO.1 security training that includes OAuth normative requirements. REPROCESS.

### Reason
PO.1 escalation rule: PO.1 gap (CONCERN) + traceable production MUST violation (RFC 6749 §4.1.2 MUST expire shortly) = REPROCESS. Developer comment provides direct traceability from training gap to MUST violation.

### Phrasing-B
REPROCESS — SSDF PO.1 absent + RFC 6749 §4.1.2 MUST expire shortly violated in production. PO.1 escalation rule applies.

PO.1 alone: CONCERN. Developer training on OAuth normative requirements absent. "Shortly" means minutes — a well-documented, widely-cited requirement. Without PO.1 training, this category of error is foreseeable.

Production MUST violation: 24-hour auth code. Developer comment ("I thought 24 hours counted") = direct traceability from PO.1 gap to MUST violation deployed in production. Not coincidence — gap caused misconfiguration.

PO.1 escalation rule: CONCERN → REPROCESS when PO.1 gap + traceable production MUST violation confirmed. Remediation: (1) reduce code TTL to minutes; (2) implement OAuth normative security training per SSDF PO.1. REPROCESS.

---

## Pair[CB4-3.4-849]

### A
JWT sub claim absent combined with ROPC grant type enabled:

An authorization server issues JWT access tokens for user-initiated flows without a sub claim (privacy argument). The same AS has ROPC (Resource Owner Password Credentials) grant type enabled — users can authenticate by directly posting username and password to the token endpoint. Team: "ROPC is simpler for our native app users and we protect privacy by not including user identifiers in tokens."

### B
REPROCESS. Two independent MUST-level violations with compounding impact.

Violation 1 — JWT sub absent: RFC 9068 §2.2 MUST sub in user-initiated flow JWT access tokens. Privacy argument rejected — opaque PPID (UUID v4) satisfies both privacy and MUST. Sub omission is not a valid privacy control; it is a MUST violation. Without sub: RS cannot apply per-user policies, user-level revocation non-functional. REPROCESS.

Violation 2 — ROPC enabled: RFC 9700 §2.4 (BCP 240) MUST NOT use ROPC. RFC 9700 supersedes RFC 6749's definition of ROPC. "Simpler for native apps" is rejected — RFC 8252 defines the correct native app flow (authorization code with PKCE and claimed HTTPS redirect URIs). ROPC eliminates the AS trust boundary: the application directly receives user credentials. MFA enforcement becomes impossible. Applications indistinguishable from phishing pages that collect credentials. REPROCESS.

Combined impact: ROPC enables the application to receive user credentials directly. With sub absent from tokens, the RS cannot identify which user's credentials were collected or which user's session is active. An attacker who intercepts ROPC credentials gains long-term credential access; without sub in subsequent tokens, the RS cannot detect that all requests are from the same compromised user account. Both the authentication boundary (ROPC) and the authorization accountability (sub absent) are broken.

Two independent MUST violations. REPROCESS.

### Reason
Two independent MUST violations: RFC 9068 §2.2 MUST sub (user-initiated flows) and RFC 9700 §2.4 MUST NOT ROPC. Combined: credential collection by application (ROPC) + no user identity in tokens (sub absent) = authentication boundary broken + authorization accountability broken. REPROCESS.

### Phrasing-B
REPROCESS — JWT sub absent (RFC 9068 §2.2 MUST) + ROPC enabled (RFC 9700 §2.4 MUST NOT). Two independent MUST violations; compounding breach.

Violation 1: sub absent in user-initiated flows. RFC 9068 §2.2 MUST. Privacy argument → opaque PPID is correct remediation. Without sub: per-user RS controls non-functional. REPROCESS.

Violation 2: ROPC enabled. RFC 9700 §2.4 MUST NOT. "Simpler for native apps" rejected — RFC 8252 defines correct native app flow. ROPC: AS trust boundary eliminated; application collects credentials; MFA impossible; phishing-indistinguishable. REPROCESS.

Combined: ROPC compromises authentication boundary; sub absence removes per-user token accountability. Attacker with ROPC-intercepted credentials operates through tokens that have no user identity at RS. REPROCESS on both independently.

---

## Pair[CB4-3.4-850]

### A
JWT access tokens without jti combined with OIDC End Session endpoint accepting logout without id_token_hint:

An authorization server issues JWT access tokens without a jti claim. The same AS's OIDC end session endpoint accepts logout requests with only a registered post_logout_redirect_uri — no id_token_hint required or validated. Team: "Token tracking adds complexity we don't need. Logout is a harmless operation."

### B
CONCERN. Two SHOULD/RECOMMENDED-level gaps are present. They do not escalate to REPROCESS.

Gap 1 — jti absent: RFC 7519 §4.1.7 SHOULD jti. Precision revocation, concurrent-use anomaly detection, and per-token audit correlation absent. TTL-based management partially mitigates. SHOULD-level: CONCERN.

Gap 2 — OIDC End Session without id_token_hint: OIDC RP-Initiated Logout specification lists id_token_hint as RECOMMENDED. Without id_token_hint, any party who knows a registered post_logout_redirect_uri can force session termination for any active user. "Logout is harmless" characterization rejected: session forcing disrupts active operations, can function as denial-of-service against specific users, and enables precursor attacks (force logout → present fake login page). RECOMMENDED level: CONCERN.

Graduation rule: two CONCERN-level gaps do not escalate to REPROCESS. Neither involves a MUST violation. The first gap is SHOULD-level (RFC 7519 §4.1.7) and the second is RECOMMENDED-level (OIDC RP-Initiated Logout). Combined presence increases severity within CONCERN, but does not cross into REPROCESS territory without an identified MUST violation.

Both remediations recommended: add UUID v4 jti to tokens with Redis blocklist; require id_token_hint at end session endpoint with five-step validation chain. CONCERN.

### Reason
Two gaps at SHOULD/RECOMMENDED level: RFC 7519 §4.1.7 SHOULD jti, and OIDC RP-Initiated Logout RECOMMENDED id_token_hint. No MUST violations. Graduation rule: two CONCERNs remain CONCERN — count of gaps does not determine verdict severity. CONCERN.

### Phrasing-B
CONCERN — jti absent (SHOULD) + End Session id_token_hint absent (RECOMMENDED). Graduation rule: two CONCERNs ≠ REPROCESS.

jti absent: RFC 7519 §4.1.7 SHOULD. Precision revocation + anomaly detection + audit absent. CONCERN.

End Session without id_token_hint: OIDC RP-Initiated Logout RECOMMENDED. Session forcing viable for any party with registered redirect URI. "Logout is harmless" rejected. CONCERN.

Graduation rule: verdict severity determined by the highest applicable normative level (SHOULD/RECOMMENDED), not the count of gaps. Two SHOULD/RECOMMENDED gaps = CONCERN. No MUST identified in either gap. Both remediations recommended independently. CONCERN.

---

## Pair[CB4-3.4-851]

### A
PAR endpoint over HTTP, JWT sub claim absent, and authorization code 24-hour lifetime simultaneously:

An authorization server has three concurrent gaps: (1) PAR endpoint at http:// (plain HTTP), (2) JWT access tokens issued without sub claim in user-initiated flows, (3) authorization codes with 24-hour expiry. The team is aware of all three and has deferred remediation for the next sprint cycle.

### B
REPROCESS. Three independent MUST-level violations present. The correct verdict does not change with additional MUST violations — the ceiling is REPROCESS.

Violation 1 — PAR HTTP: RFC 9126 §3 MUST TLS on PAR endpoint. Plain HTTP is a MUST violation. On-path code_challenge parameter injection attack enabled. REPROCESS.

Violation 2 — JWT sub absent: RFC 9068 §2.2 MUST sub in user-initiated flow tokens. Privacy argument not stated here, but MUST is unconditional regardless of rationale. Without sub: per-user RS controls non-functional; user-level revocation non-functional. REPROCESS.

Violation 3 — Authorization code 24-hour lifetime: RFC 6749 §4.1.2 MUST expire shortly. 24 hours is not "shortly." 24-hour window for interception-and-use attack. REPROCESS.

Combined impact: (a) An on-path attacker intercepts the PAR request and substitutes the code_challenge. (b) The authorization code is valid for 24 hours — ample time for the attacker to execute the substituted-verifier exchange. (c) The resulting JWT access tokens have no sub claim — the RS cannot identify which user's authorization was exploited. The three violations form a complete attack chain: parameter injection at PAR → 24-hour window to exploit → untraceable token at RS.

Sprint deferral does not affect the verdict — REPROCESS reflects the current state of the deployed system. REPROCESS.

### Reason
Three independent RFC MUST violations: RFC 9126 §3 (PAR TLS), RFC 9068 §2.2 (JWT sub), RFC 6749 §4.1.2 (auth code short lifetime). Verdict ceiling is REPROCESS regardless of number of MUST violations. Combined: complete attack chain from PAR injection through 24-hour code window to untraceable token. REPROCESS.

### Phrasing-B
REPROCESS — three concurrent MUST violations: PAR HTTP, sub absent, auth code 24h. Verdict ceiling: REPROCESS; count of violations does not change verdict.

Violation 1: PAR HTTP. RFC 9126 §3 MUST TLS. Code_challenge injection attack viable.
Violation 2: sub absent. RFC 9068 §2.2 MUST. Per-user RS controls non-functional; tokens untraceable to user.
Violation 3: 24h auth code. RFC 6749 §4.1.2 MUST expire shortly. Interception-and-use window 24 hours.

Attack chain: parameter injection at PAR → 24-hour code window for exploitation → untraceable token at RS. Three violations form cascading attack path. Sprint deferral = current state is REPROCESS. REPROCESS.

---

## Pair[CB4-3.4-852]

### A
JWT access tokens without jti combined with AS not enforcing PKCE for public clients:

An authorization server issues JWT access tokens without jti claims. The same AS accepts public client authorization code requests with or without a code_challenge parameter. Team: "Our tokens are short-lived (15 minutes) and PKCE is available for clients that want it."

### B
REPROCESS. One MUST-level violation overrides the CONCERN-level gap.

Gap 1 — jti absent: RFC 7519 §4.1.7 SHOULD jti. TTL-based argument partially accepted. Precision revocation, replay detection, and audit correlation absent. SHOULD-level gap: CONCERN.

Violation 2 — AS does not enforce PKCE for public clients: RFC 9700 §2.1.1 MUST. Authorization servers MUST require PKCE for public clients. This is an unconditional MUST. "Available for clients that want it" = optional PKCE = no PKCE protection for clients that omit it. Backward-incompatibility argument not stated here but would be rejected. Authorization code interception attacks remain viable for all non-PKCE authorization code flows. REPROCESS.

Graduation rule: one CONCERN (jti absent) + one REPROCESS (PKCE enforcement absent) → the REPROCESS verdict overrides. The graduation rule states: REPROCESS overrides CONCERN. The verdict is determined by the highest severity finding present. One MUST violation is sufficient for REPROCESS regardless of how many CONCERN-level gaps also exist.

Combined impact: public clients can request authorization codes without PKCE. Those codes can be intercepted and exchanged for JWT access tokens. The exchanged tokens lack jti — the RS cannot detect the concurrent use of tokens from the legitimate user and the attacker simultaneously. The PKCE enforcement gap enables code theft; the jti absence reduces the RS's ability to detect the resulting token misuse. REPROCESS.

### Reason
Graduation rule application: jti absent = CONCERN (SHOULD); PKCE enforcement absent = REPROCESS (RFC 9700 §2.1.1 MUST). REPROCESS overrides CONCERN — verdict is REPROCESS. One MUST violation is sufficient.

### Phrasing-B
REPROCESS — jti absent (CONCERN) + AS not enforcing PKCE for public clients (RFC 9700 §2.1.1 MUST). Graduation rule: REPROCESS overrides CONCERN.

jti absent: SHOULD-level gap. CONCERN. TTL partially mitigates.
PKCE enforcement absent: RFC 9700 §2.1.1 MUST. Optional PKCE = no PKCE for clients that omit it. Authorization code interception viable. REPROCESS.

Graduation rule: the final verdict is the highest severity finding. CONCERN + REPROCESS = REPROCESS. The CONCERN does not dilute the REPROCESS; the REPROCESS does not require all findings to be REPROCESS. One MUST violation = REPROCESS verdict regardless of concurrent CONCERN-level gaps. REPROCESS.

---

## Pair[CB4-3.4-853]

### A
Authorization code 24-hour lifetime combined with JWT sub absent — cascading authorization accountability failure:

An authorization server issues authorization codes with a 24-hour expiry (single-use enforced). The same AS issues JWT access tokens for user-initiated flows without a sub claim. An attacker intercepts an authorization code through a phishing redirect and plans their attack over several hours before execution.

### B
REPROCESS. Two independent MUST violations with compound cascading impact.

Violation 1 — Authorization code 24-hour lifetime: RFC 6749 §4.1.2 MUST expire shortly. Single-use does not substitute for short lifetime — it addresses replay, not interception-and-use. A 24-hour window gives the attacker time to plan their attack, wait for the right moment, and execute the token exchange before the legitimate user returns. REPROCESS.

Violation 2 — JWT sub absent: RFC 9068 §2.2 MUST sub in user-initiated flow tokens. Without sub, the tokens issued after the attacker's code exchange cannot be attributed to any user at the RS. Per-user rate limiting, per-user data restrictions, and user-level revocation are all non-functional. REPROCESS.

Cascading compound impact: the 24-hour code window enables the attacker to execute a deliberate, planned interception-and-use attack (rather than a frantic 60-second window attack). After successful exchange, the attacker receives JWT access tokens with no sub claim — the RS cannot identify who these tokens belong to, cannot apply per-user restrictions, and cannot revoke them at the user level when the breach is discovered. The victim may discover the breach (e.g., from account activity notifications), but without sub in the tokens, user-level revocation cannot directly invalidate the attacker's tokens. Both the acquisition path (long auth code window) and the post-acquisition accountability (no sub) are broken.

Both violations are independently REPROCESS. REPROCESS.

### Reason
Two independent RFC MUST violations: RFC 6749 §4.1.2 (auth code 24h) and RFC 9068 §2.2 (sub absent). Cascading impact: 24-hour window enables planned interception; sub absence prevents user-level revocation after discovery. Both REPROCESS independently.

### Phrasing-B
REPROCESS — 24-hour auth code (RFC 6749 §4.1.2 MUST) + sub absent (RFC 9068 §2.2 MUST). Cascading: long acquisition window + no post-acquisition accountability.

Violation 1: 24h auth code. MUST expire shortly violated. 24-hour window enables planned interception-and-use attack — attacker waits for optimal execution moment. Single-use addresses replay, not this threat.

Violation 2: sub absent. RFC 9068 §2.2 MUST. Post-exchange: attacker's tokens untraceable to any user at RS. User-level revocation non-functional even when breach detected.

Cascade: long code window enables planned breach; sub absence prevents recovery after breach. Two MUST violations, both independently REPROCESS, with compounding impact on the complete attack lifecycle. REPROCESS.

---

## Pair[CB4-3.4-854]

### A
Complete ext32 compliance baseline — jti, 60-second auth codes, opaque sub, PAR HTTPS, and all controls:

An authorization server implements all ext32 controls correctly: (1) JWT access tokens include UUID v4 jti with Redis JTI blocklist (TTL = remaining token lifetime). (2) Authorization codes expire after 60 seconds; single-use enforced; replay triggers session-wide revocation. (3) JWT access tokens include sub = "usr-{UUID v4 PPID}" — no PII. (4) PAR endpoint at https://as.example.com/par with TLS 1.3 and valid CA; client authentication required; request_uri single-use and client_id-bound. (5) Public clients required to use PKCE S256; non-PKCE requests rejected with 400.

### B
PASS. All ext32 security controls correctly implemented.

Control 1 — jti: UUID v4 per token; globally unique; RFC 7519 §4.1.7 SHOULD uniqueness requirement satisfied. Redis blocklist keyed by jti with TTL-matched entries (no unbounded growth). RS validates jti against blocklist on every request. Precision revocation: specific tokens revocable before TTL. Concurrent-use anomaly detection: enabled. Per-token audit correlation: enabled. PASS.

Control 2 — Authorization code lifetime: 60 seconds. RFC 6749 §4.1.2 MUST expire shortly: satisfied. Single-use enforcement with replay-triggered session-wide revocation: §4.1.2 second condition satisfied. Both RFC 6749 §4.1.2 requirements met. 60-second window not viable for manual interception-and-use; PKCE code_verifier adds second factor for automated attacks. PASS.

Control 3 — JWT sub (opaque PPID): UUID v4-based PPID "usr-{UUID}". RFC 9068 §2.2 MUST sub: satisfied. No PII in sub value. RS validates sub format (UUID pattern). Per-user rate limiting at RS: functional. User-level revocation (all tokens for a sub added to blocklist): functional. Audit correlation per request via sub: functional. Privacy + MUST compliance simultaneously achieved. PASS.

Control 4 — PAR HTTPS: HTTPS TLS 1.3 with valid CA. RFC 9126 §3 MUST TLS: satisfied. Authorization parameters protected in transit — code_challenge injection not viable. Client authentication required at PAR endpoint: RFC 9126 §2 satisfied. request_uri single-use + client_id binding: RFC 9126 §4.1 MUST satisfied. All three RFC 9126 requirement areas correct. PASS.

Control 5 — PKCE for public clients: RFC 9700 §2.1.1 MUST. AS rejects public client authorization code requests without code_challenge_method=S256 (400 invalid_request). All registered public clients use S256. Code interception requires possession of both code and verifier. PASS.

No findings across all ext32 security domains. PASS.

### Reason
All five ext32 control areas correctly implemented: jti (RFC 7519 §4.1.7 SHOULD), auth code lifetime (RFC 6749 §4.1.2 MUST), JWT sub PPID (RFC 9068 §2.2 MUST), PAR HTTPS (RFC 9126 §3 MUST), PKCE enforcement (RFC 9700 §2.1.1 MUST). PASS.

### Phrasing-B
PASS — all ext32 controls correct: jti + 60s auth code + opaque sub + PAR HTTPS + PKCE enforcement.

Control 1: UUID v4 jti + Redis TTL-keyed blocklist. RFC 7519 §4.1.7 SHOULD. Precision revocation + anomaly detection + audit all functional. PASS.

Control 2: 60-second auth code + single-use + replay-triggered revocation. RFC 6749 §4.1.2 both conditions satisfied. PASS.

Control 3: sub = UUID v4 PPID. RFC 9068 §2.2 MUST. No PII. Per-user rate limiting + user-level revocation + audit functional. Privacy + compliance simultaneously achieved. PASS.

Control 4: PAR HTTPS TLS 1.3 valid CA. RFC 9126 §3 MUST TLS satisfied. Client auth at PAR (§2). request_uri single-use + client_id binding (§4.1 MUST). All three RFC 9126 areas. PASS.

Control 5: PKCE S256 enforced for all public clients. RFC 9700 §2.1.1 MUST. Non-PKCE requests rejected. Code interception requires code + verifier. PASS.

All ext32 security domains compliant. No findings. PASS.
