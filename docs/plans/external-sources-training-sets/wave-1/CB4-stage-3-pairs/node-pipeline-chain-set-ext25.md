# CB4 Stage 3 — Action 3 Extended Pair Set
# Node: Multi-node pipeline chains (ext25 — ext31 pipeline traces)
# Sequences: CB4-3.4-811 through CB4-3.4-818
# Instances: 16 (8 base pairs × 2 phrasings each)
# Stratum: C (full 9-node pipeline traces)
# Running total after this file: 1636 instances (818 base pairs)

---

## Pair[CB4-3.4-811]

### A
Full pipeline trace — AS does not require PKCE for public clients:

Input: AS accepts authorization code requests from public clients without code_challenge. PKCE optional. Team: "Backward compatibility with older client versions."

### B
Node 1 — Domain Splitter: Domain T-rfc-oauth2 (RFC 9700 §2.1.1 PKCE enforcement policy). Sub-domain: AS-side PKCE requirement for public clients. Distinct from client-side PKCE implementation — this evaluates the AS's enforcement policy.

Node 2 — Domain Translator: Subject: AS accepts non-PKCE authorization code requests from public clients. Reference: RFC 9700 §2.1.1 MUST require PKCE for public clients. Argument: backward compatibility.

Node 3 — Scheme Planner: Plan: (1) RFC 9700 §2.1.1 MUST compliance check; (2) evaluate backward-compatibility argument; (3) assess impact of optional PKCE policy; (4) determine verdict.

Node 4 — Domain Processor:
  AS behavior: public client requests without code_challenge accepted and processed.
  RFC 9700 §2.1.1: "Authorization servers MUST require PKCE for public clients." MUST violated.
  Backward-compatibility argument: RFC 9700 MUST has no backward-compatibility clause. The MUST establishes the current security requirement; legacy clients must be updated to comply. A legacy exception would effectively permanently exempt all pre-existing clients from the PKCE requirement.
  Security impact of optional PKCE: any public client authorization code flow without PKCE is vulnerable to code interception attacks. If the AS permits non-PKCE flows, the effectiveness of the PKCE requirement depends entirely on individual client compliance — clients that omit PKCE are unprotected, and the AS cannot compensate for this.

Node 5 — Applier: RFC 9700 §2.1.1 MUST violated. Backward-compatibility argument rejected. REPROCESS trigger: 1.

Node 6 — Result Translator: REPROCESS.

Node 7 — Arbiter Selector: T-rfc-oauth2 arbiter for RFC 9700 §2.1.1 public client PKCE enforcement.

Node 8 — Arbiter Units: REPROCESS. RFC 9700 §2.1.1 MUST violated — AS accepts public client authorization code requests without PKCE. Optional PKCE = PKCE provides no protection for non-compliant clients. Backward-compatibility argument rejected.

Node 9 — Formatter: Verdict: REPROCESS. RFC 9700 §2.1.1. AS must reject public client authorization code requests without a valid code_challenge parameter (return 400 invalid_request).

### Reason
RFC 9700 §2.1.1 MUST is unconditional for public clients. Backward compatibility exception absent. Optional PKCE = no PKCE protection for omitting clients.

### Phrasing-B
Node 1: T-rfc-oauth2. RFC 9700 §2.1.1 AS PKCE enforcement.
Node 2: Non-PKCE public clients accepted. §2.1.1 MUST. Backward-compatibility argument.
Node 3: (1) §2.1.1 MUST; (2) argument; (3) optional PKCE security impact.
Node 4: Non-PKCE requests accepted. §2.1.1 MUST violated. No compatibility exception in RFC 9700. Optional PKCE: unprotected clients exist. REPROCESS trigger.
Node 5: §2.1.1 MUST violated. Argument rejected. REPROCESS.
Node 6: REPROCESS.
Node 7: T-rfc-oauth2.
Node 8: REPROCESS. RFC 9700 §2.1.1 — AS must require PKCE for public clients.
Node 9: REPROCESS. §2.1.1. Reject non-PKCE public client requests (400 invalid_request).

---

## Pair[CB4-3.4-812]

### A
Full pipeline trace — AS enforces PKCE S256 for all public client requests:

Input: AS rejects public client authorization code requests without code_challenge (400 invalid_request). All registered public clients send code_challenge_method=S256.

### B
Node 1 — Domain Splitter: T-rfc-oauth2. RFC 9700 §2.1.1 public client PKCE enforcement.

Node 2 — Domain Translator: AS rejects non-PKCE requests. RFC 9700 §2.1.1 MUST.

Node 3 — Scheme Planner: (1) §2.1.1 enforcement confirmed; (2) client-side S256 compliance.

Node 4 — Domain Processor: AS rejects missing code_challenge with 400. RFC 9700 §2.1.1 MUST: satisfied. All public clients send S256. RFC 7636 §4.2 MUST (S256 for capable clients): satisfied. No non-PKCE codes issued.

Node 5 — Applier: No violations.

Node 6 — Result Translator: PASS.

Node 7 — Arbiter Selector: No activation.

Node 8 — Arbiter Units: PASS. RFC 9700 §2.1.1 MUST satisfied. AS-level enforcement ensures PKCE cannot be bypassed.

Node 9 — Formatter: PASS. RFC 9700 §2.1.1 satisfied.

### Reason
AS enforces PKCE at the server level. Clients send S256. §2.1.1 and §4.2 both satisfied. PASS.

### Phrasing-B
Node 1: T-rfc-oauth2. §2.1.1.
Node 2: AS rejects non-PKCE. §2.1.1 MUST.
Node 3: Enforcement confirmed; S256 compliance.
Node 4: 400 on missing code_challenge. §2.1.1 satisfied. All clients S256. §4.2 satisfied.
Node 5: No violations.
Node 6: PASS.
Node 7: No activation.
Node 8: PASS. §2.1.1 satisfied.
Node 9: PASS.

---

## Pair[CB4-3.4-813]

### A
Full pipeline trace — JWT access tokens signed with HS256 in a multi-party system:

Input: AS signs tokens with HS256. Same HMAC secret distributed to 4 resource server teams. "HS256 is simpler to configure."

### B
Node 1 — Domain Splitter: Domain T-rfc-jwt (RFC 8725 §3.6 multi-party signing) + T-asvs-token (V9.1.1 algorithm enforcement). Sub-domain: symmetric vs asymmetric signing in multi-party token systems.

Node 2 — Domain Translator: Subject: HS256 symmetric key distributed to 4 RS teams. References: RFC 8725 §3.6 (asymmetric keys for multi-party); V9.1.1 (algorithm configuration). Argument: configuration simplicity.

Node 3 — Scheme Planner: Plan: (1) RFC 8725 §3.6 multi-party signing assessment; (2) V9.1.1 algorithm compliance; (3) evaluate simplicity argument; (4) assess multi-party signing key distribution risk; (5) determine verdict.

Node 4 — Domain Processor:
  HS256 symmetric key: same secret used at AS (sign) and all 4 RSes (verify).
  RFC 8725 §3.6: "If symmetric MACs are used, use a different secret for each subject" and the general guidance for multi-party: use asymmetric keys when the signer and verifiers are different parties.
  Security analysis: HS256 key = signing secret = verification secret. Distributing to 4 RSes creates 4 independent attack surfaces for signing key exposure. Compromise of any RS config, secrets manager, or code repo provides the HMAC secret → attacker can forge JWT claims with valid HS256 signatures accepted by all 4 RSes.
  Comparison: RS256 private key held by AS only; RSes use public key (not secret). RS compromise → public key only → cannot forge.
  Simplicity argument: operationally accurate. Does not satisfy the security requirement for multi-party systems.
  V9.1.1: algorithm configuration must be secure; HS256 for multi-party token systems creates the exact forgery risk V9.1.1 is designed to prevent.

Node 5 — Applier: RFC 8725 §3.6 multi-party signing guidance violated; V9.1.1 algorithm security requirement not met. REPROCESS trigger.

Node 6 — Result Translator: REPROCESS.

Node 7 — Arbiter Selector: T-rfc-jwt + T-asvs-token arbiters.

Node 8 — Arbiter Units: REPROCESS. RFC 8725 §3.6 — HS256 with shared secret distributed to 4 resource server teams. Any RS compromise yields the signing key, enabling token forgery across all RSes. Use RS256 (AS: private key; RSes: HTTPS JWKS public key). V9.1.1 algorithm security: use asymmetric signing for multi-party systems.

Node 9 — Formatter: Verdict: REPROCESS. RFC 8725 §3.6; V9.1.1. Migrate to RS256: AS holds private key (not distributed); RSes use HTTPS JWKS for public key verification only.

### Reason
RFC 8725 §3.6 explicitly addresses multi-party JWT signing: asymmetric keys required when different parties sign and verify. HS256 distributed to 4 RSes = 4 key-forgery attack surfaces. Simplicity argument rejected.

### Phrasing-B
Node 1: T-rfc-jwt + T-asvs-token. Multi-party JWT signing algorithm.
Node 2: HS256 shared secret to 4 RSes. RFC 8725 §3.6; V9.1.1. Simplicity argument.
Node 3: (1) §3.6 multi-party; (2) V9.1.1; (3) simplicity argument; (4) key distribution risk.
Node 4: HS256: sign = verify key. 4 RSes hold signing secret. Any RS compromise → universal token forgery. RS256: private key in AS only. RSes use public key (already public). Simplicity rejected — security requirement not optional.
Node 5: RFC 8725 §3.6 + V9.1.1 violated. REPROCESS.
Node 6: REPROCESS.
Node 7: T-rfc-jwt + T-asvs-token.
Node 8: REPROCESS. RFC 8725 §3.6 + V9.1.1 — HS256 multi-party key distribution. Migrate to RS256.
Node 9: REPROCESS. RFC 8725 §3.6. RS256 with AS private key + HTTPS JWKS public key for RSes.

---

## Pair[CB4-3.4-814]

### A
Full pipeline trace — RS256 asymmetric signing in multi-party system:

Input: AS signs tokens with RSA private key (RS256). 4 RSes fetch public key from HTTPS JWKS. Private key in AS secrets manager only; no RS holds it.

### B
Node 1 — Domain Splitter: T-rfc-jwt + T-asvs-token. Multi-party asymmetric JWT signing.

Node 2 — Domain Translator: RS256, AS private key, RSes use HTTPS JWKS. RFC 8725 §3.6; V9.1.1.

Node 3 — Scheme Planner: (1) §3.6 multi-party compliance; (2) private key exclusivity; (3) JWKS delivery security.

Node 4 — Domain Processor: RS256: AS signs with private key (secrets manager); RSes verify with public key from HTTPS JWKS. No RS holds private key. RS compromise → public key obtained (already public) → cannot forge. HTTPS JWKS delivery (from ext28 pattern): public key integrity protected. RFC 8725 §3.6: satisfied. V9.1.1: RS256 asymmetric algorithm correct for multi-party.

Node 5 — Applier: No violations.

Node 6 — Result Translator: PASS.

Node 7 — Arbiter Selector: No activation.

Node 8 — Arbiter Units: PASS. RFC 8725 §3.6 multi-party signing satisfied. Private key exclusivity maintained. RS compromise cannot enable token forgery.

Node 9 — Formatter: PASS. RFC 8725 §3.6 + V9.1.1 satisfied.

### Reason
RS256 separates signing capability from verification. RS compromise yields only public key. §3.6 satisfied.

### Phrasing-B
Node 1: T-rfc-jwt + T-asvs-token. RS256 multi-party.
Node 2: RS256 + HTTPS JWKS. RFC 8725 §3.6; V9.1.1.
Node 3: §3.6 compliance; private key exclusivity; JWKS security.
Node 4: Private key in AS secrets manager; no RS access. RS compromise → public key only → no forgery. §3.6 satisfied. V9.1.1 satisfied.
Node 5: No violations.
Node 6: PASS.
Node 7: No activation.
Node 8: PASS. §3.6 + V9.1.1 satisfied.
Node 9: PASS.

---

## Pair[CB4-3.4-815]

### A
Full pipeline trace — Token endpoint IP-rate-limiting only; no per-client-id throttling:

Input: AS limits token endpoint to 100 req/min per source IP. No per-client-id counter. Botnet with 100 IPs could attempt 10,000 client_secret guesses/min per target client_id. Team: "High-volume legitimate services can't be per-client throttled."

### B
Node 1 — Domain Splitter: Domain T-asvs-authentication (ASVS V6.2 brute force protection). Sub-domain: token endpoint credential rate limiting granularity.

Node 2 — Domain Translator: Subject: IP-only rate limiting on token endpoint. No per-client-id throttle. Distributed credential attack viable. Argument: high-volume services.

Node 3 — Scheme Planner: Plan: (1) ASVS V6.2 brute force protection compliance check; (2) assess whether IP-only limits provide sufficient protection; (3) evaluate high-volume services argument; (4) determine MUST vs CONCERN level.

Node 4 — Domain Processor:
  IP rate limiting: provides protection against single-source volume attacks.
  Per-client-id throttling: absent. Distributed attack (100 IPs × 100 req/min = 10,000 guesses/min per client_id) bypasses IP limits.
  ASVS V6.2 brute force protection: requires mechanisms to protect against automated credential attacks. IP-only limiting is a partial implementation — it addresses single-source volume but not distributed credential stuffing.
  High-volume services argument: partially valid design challenge. Per-client throttling should use higher limits for registered high-volume clients (configuration per registration). Not a fundamental barrier.
  MUST level determination: no RFC or ASVS section specifies per-client-id rate limiting as a MUST mechanism for token endpoints. ASVS V6.2 specifies protection requirement without mandating the specific mechanism. IP-only = defense-in-depth gap, not MUST violation.

Node 5 — Applier: Defense-in-depth gap. No MUST violated. CONCERN-level finding.

Node 6 — Result Translator: CONCERN.

Node 7 — Arbiter Selector: T-asvs-authentication arbiter for V6.2 brute force protection.

Node 8 — Arbiter Units: CONCERN. IP-only rate limiting provides partial protection. Distributed credential brute-force against specific client_id viable via botnet. Per-client-id throttling is the appropriate mechanism for client credential attacks. SHOULD implement; no MUST specifying the mechanism. CONCERN.

Node 9 — Formatter: Verdict: CONCERN. ASVS V6.2 brute force protection. Recommendation: add per-client-id throttling with configurable limits (higher limits for registered high-volume clients) to complement IP rate limiting.

### Reason
ASVS V6.2 brute force protection: IP-only rate limiting is incomplete for distributed attacks but no specific per-client-id mechanism is MUST-mandated. CONCERN.

### Phrasing-B
Node 1: T-asvs-authentication. ASVS V6.2 brute force.
Node 2: IP-only rate limiting. No per-client-id. Distributed attack viable. High-volume services argument.
Node 3: (1) V6.2 compliance; (2) distributed attack protection; (3) high-volume services argument; (4) MUST vs CONCERN.
Node 4: IP limiting partial. Botnet bypass: 100 IPs × 100/min = 10,000/min per client_id. High-volume services argument: addressable via per-registration configuration. No MUST specifying per-client mechanism.
Node 5: Defense-in-depth gap. No MUST violated. CONCERN.
Node 6: CONCERN.
Node 7: T-asvs-authentication.
Node 8: CONCERN. Distributed credential attack viable. Per-client-id throttling recommended. No MUST for specific mechanism.
Node 9: CONCERN. V6.2. Add per-client-id rate limiting.

---

## Pair[CB4-3.4-816]

### A
Full pipeline trace — Per-client-id rate limiting with graduated response on token endpoint:

Input: 30 failed auth attempts per 15-min window per client_id in Redis. 429 + Retry-After on breach. Consecutive-window breaches → secret rotation flag + owner notification. Also IP rate limiting.

### B
Node 1 — Domain Splitter: T-asvs-authentication. ASVS V6.2 brute force protection.

Node 2 — Domain Translator: Per-client-id counter (30/15min). 429 response. Escalation on repeated breach. V6.2.

Node 3 — Scheme Planner: (1) Per-client-id protection adequacy; (2) graduated response coverage; (3) layered with IP limiting.

Node 4 — Domain Processor: Per-client-id counter: limits guesses regardless of source IP count — botnet cannot bypass. 30/15min: reasonable limit for legitimate clients (most use case patterns well below this). 429 + Retry-After: correct HTTP signaling. Escalation to rotation flag + notification: sustained attack → credential lifecycle response; owner alerted. IP rate limiting as additional layer. Defense-in-depth: multiple layers, neither bypassable alone. V6.2 brute force protection: satisfied.

Node 5 — Applier: No violations.

Node 6 — Result Translator: PASS.

Node 7 — Arbiter Selector: No activation.

Node 8 — Arbiter Units: PASS. Per-client-id rate limiting directly addresses credential brute-force regardless of IP distribution. Graduated response provides both immediate and sustained protection. ASVS V6.2 satisfied.

Node 9 — Formatter: PASS. ASVS V6.2 satisfied.

### Reason
Per-client throttling defeats distributed credential attacks. Graduated response handles sustained attempts. Layered with IP limiting. PASS.

### Phrasing-B
Node 1: T-asvs-authentication. V6.2.
Node 2: 30/15min per client_id. 429 + Retry-After. Escalation. V6.2.
Node 3: Per-client adequacy; graduated response; layering.
Node 4: Per-client counter: botnet-bypassing-proof. 30/15min: usable limit. Escalation: sustained attack response. Layered with IP. V6.2 satisfied.
Node 5: No violations.
Node 6: PASS.
Node 7: No activation.
Node 8: PASS. V6.2 satisfied. Per-client-id protection correct.
Node 9: PASS.

---

## Pair[CB4-3.4-817]

### A
Full pipeline trace — OIDC End Session without id_token_hint requirement:

Input: End session endpoint accepts post_logout_redirect_uri only; no id_token_hint required. Any caller can force logout. Team: "Logout is not security-sensitive."

### B
Node 1 — Domain Splitter: Domain T-rfc-oidc (OIDC RP-Initiated Logout). Sub-domain: id_token_hint requirement and session forcing attack surface.

Node 2 — Domain Translator: Subject: End session endpoint without id_token_hint requirement. Reference: OIDC RP-Initiated Logout 1.0 id_token_hint RECOMMENDED. Argument: logout is harmless.

Node 3 — Scheme Planner: Plan: (1) RECOMMENDED vs MUST level for id_token_hint; (2) evaluate "harmless logout" argument; (3) identify session forcing threat; (4) determine verdict level.

Node 4 — Domain Processor:
  id_token_hint: absent requirement.
  OIDC RP-Initiated Logout: id_token_hint is RECOMMENDED, not MUST.
  "Harmless logout" argument: rejected. Session forcing (forcing a user's session to be terminated by an attacker) has real security impact: (a) disrupts active operations (form submission, financial transactions, workflow); (b) can serve as a DoS mechanism against specific users; (c) can be a phishing precursor (force logout → present fake login page to steal credentials).
  Without id_token_hint: any party with knowledge of a registered redirect URI can trigger logout for any active user.
  RECOMMENDED level: the gap is CONCERN, not REPROCESS.

Node 5 — Applier: OIDC RP-Initiated Logout RECOMMENDED gap. Harmless argument rejected. One CONCERN-level finding.

Node 6 — Result Translator: CONCERN.

Node 7 — Arbiter Selector: T-rfc-oidc arbiter for OIDC RP-Initiated Logout.

Node 8 — Arbiter Units: CONCERN. OIDC RP-Initiated Logout id_token_hint RECOMMENDED not satisfied. Session forcing attack viable — any caller can terminate any session without holding a valid token. "Harmless logout" argument rejected: session forcing has operational and security impact. RECOMMENDED not MUST: CONCERN.

Node 9 — Formatter: Verdict: CONCERN. OIDC RP-Initiated Logout. Require and validate id_token_hint to bind logout requests to session token holders.

### Reason
RECOMMENDED not MUST. Session forcing attack viable and non-trivial. Harmless argument rejected. CONCERN.

### Phrasing-B
Node 1: T-rfc-oidc. OIDC RP-Initiated Logout id_token_hint.
Node 2: No id_token_hint requirement. OIDC RP-Initiated Logout RECOMMENDED. Harmless-logout argument.
Node 3: (1) RECOMMENDED level; (2) harmless argument; (3) session forcing threat.
Node 4: No id_token_hint required. RECOMMENDED not satisfied. Harmless argument rejected — session forcing disrupts operations, DoS tool, phishing precursor. RECOMMENDED: CONCERN level.
Node 5: RECOMMENDED gap. Harmless argument rejected. CONCERN.
Node 6: CONCERN.
Node 7: T-rfc-oidc.
Node 8: CONCERN. OIDC RP-Initiated Logout RECOMMENDED — id_token_hint absent. Session forcing viable.
Node 9: CONCERN. RP-Initiated Logout. Require id_token_hint with 5-step validation.

---

## Pair[CB4-3.4-818]

### A
Full pipeline trace — OIDC End Session with id_token_hint required and validated:

Input: End session requires id_token_hint. AS validates: signature, sub → active session, aud → requesting client, exp → not expired. Only the matching session terminated.

### B
Node 1 — Domain Splitter: T-rfc-oidc. OIDC RP-Initiated Logout.

Node 2 — Domain Translator: id_token_hint required and validated (5-step). OIDC RP-Initiated Logout RECOMMENDED.

Node 3 — Scheme Planner: (1) id_token_hint present and validated; (2) session binding; (3) session forcing protection.

Node 4 — Domain Processor: id_token_hint required. 5-step validation: signature valid; sub matches active session; aud matches requesting client_id; exp not expired; only matching session terminated. RECOMMENDED: satisfied (implemented above RECOMMENDED floor — required, not optional). Session forcing blocked: attacker without valid current ID token cannot trigger logout. OIDC RP-Initiated Logout RECOMMENDED met.

Node 5 — Applier: No violations.

Node 6 — Result Translator: PASS.

Node 7 — Arbiter Selector: No activation.

Node 8 — Arbiter Units: PASS. OIDC RP-Initiated Logout RECOMMENDED satisfied. id_token_hint required, fully validated. Session forcing blocked.

Node 9 — Formatter: PASS. RP-Initiated Logout RECOMMENDED satisfied.

### Reason
id_token_hint required and fully validated. RECOMMENDED satisfied at elevated level. Session forcing blocked. PASS.

### Phrasing-B
Node 1: T-rfc-oidc. OIDC RP-Initiated Logout.
Node 2: id_token_hint required + 5-step validation. RECOMMENDED.
Node 3: id_token_hint presence; session binding; forcing protection.
Node 4: Required id_token_hint. 5-step validates signature, sub, aud, exp. Only matched session terminated. RECOMMENDED satisfied above floor. Session forcing blocked.
Node 5: No violations.
Node 6: PASS.
Node 7: No activation.
Node 8: PASS. RP-Initiated Logout RECOMMENDED satisfied.
Node 9: PASS.
