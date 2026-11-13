# CB4 Stage 3 — Action 3 Extended Pair Set
# Node: Cross-source combinations (ext24 — ext31 cross-source)
# Sequences: CB4-3.4-819 through CB4-3.4-828
# Instances: 20 (10 base pairs × 2 phrasings each)
# Stratum: B/C (cross-source pattern combinations)
# Running total after this file: 1656 instances (828 base pairs)

---

## Pair[CB4-3.4-819]

### A
Cross-source input — ext31 combination (AS no PKCE enforcement for public clients + HS256 multi-party):

An authorization system review finds: (1) the AS accepts authorization code requests from public clients (SPAs, native apps) without code_challenge — PKCE is optional; (2) JWTs are signed with HS256; the same HMAC secret is distributed to three resource server teams. Team: "Both are practical implementation choices — PKCE adoption is gradual and HS256 is operationally simpler."

### B
REPROCESS. Two independent MUST-level violations. Neither "gradual adoption" nor "operational simplicity" modifies MUST requirements.

Finding 1 — RFC 9700 §2.1.1 MUST (PKCE not enforced for public clients): AS accepts non-PKCE authorization code flows. Public clients without PKCE are vulnerable to code interception. "Gradual adoption" implies an ongoing period of unprotected code flows — there is no normative grace period in RFC 9700 §2.1.1 for implementation timelines. REPROCESS trigger.

Finding 2 — RFC 8725 §3.6 + V9.1.1 (HS256 symmetric key distributed to 3 RSes): In a multi-party system, distributing the signing secret to RS teams creates 3 independent forgery attack surfaces. Any RS configuration compromise yields the HMAC secret, enabling universal token forgery across all RSes and all public clients. REPROCESS trigger.

The two violations compound: non-PKCE public client flows produce authorization codes that can be intercepted; HS256 multi-party signing means a compromised RS can generate forged tokens. Both attack surface openings are independent and simultaneous.

### Reason
RFC 9700 §2.1.1 MUST and RFC 8725 §3.6 both violated independently. "Gradual" and "simpler" arguments acknowledge real implementation factors but do not constitute MUST exceptions.

### Phrasing-B
REPROCESS — AS no PKCE enforcement and HS256 multi-party: two MUST violations.

RFC 9700 §2.1.1: AS accepts non-PKCE public client requests. Gradual adoption ≠ normative exception. Code interception viable for all non-PKCE flows. REPROCESS.

RFC 8725 §3.6: HS256 secret to 3 RS teams. Operational simplicity does not satisfy multi-party signing security requirement. Any RS compromise → universal token forgery. REPROCESS.

---

## Pair[CB4-3.4-820]

### A
Cross-source input — ext31 combination (AS enforces PKCE + RS256):

An authorization system implements: (1) AS requires code_challenge_method=S256 for all public client authorization code requests (400 on missing challenge); (2) tokens signed with RS256; AS holds private key in secrets manager; all RSes use HTTPS JWKS public key for verification.

### B
PASS. Both PKCE enforcement and RS256 asymmetric signing correctly implemented.

RFC 9700 §2.1.1 MUST: AS enforces PKCE for all public client flows. Non-PKCE requests rejected at the authorization endpoint. Code interception attack vector eliminated.

RFC 8725 §3.6: RS256 asymmetric signing. AS private key never distributed. RSes use public key (verification only). RS compromise yields public key (already public via JWKS) — cannot enable token forgery. Multi-party signing security requirement satisfied.

No findings.

### Reason
RFC 9700 §2.1.1 and RFC 8725 §3.6 both satisfied. PKCE server-enforced; asymmetric signing correctly deployed.

### Phrasing-B
PASS — AS-enforced PKCE S256 and RS256 multi-party signing both confirmed correct.

§2.1.1: 400 on missing code_challenge for public clients. Code interception: not viable.
§3.6: RS256, AS private key exclusive; RSes use HTTPS JWKS public key. RS compromise cannot enable forgery. PASS.

---

## Pair[CB4-3.4-821]

### A
Cross-source input — ext31 combination (IP-only rate limiting + OIDC End Session without id_token_hint):

Two SHOULD-level gaps found in an OIDC system: (1) token endpoint rate limiting is IP-based only — no per-client-id throttling; distributed credential attacks viable; (2) OIDC End Session accepts logout requests without id_token_hint — any caller with a registered redirect URI can force session termination. Team: "Both are acceptable gaps given our deployment."

### B
CONCERN. Two SHOULD/RECOMMENDED-level gaps identified. No REPROCESS triggers present. N CONCERNs do not escalate to REPROCESS — graduation rule applies.

Finding 1 — Token endpoint IP-only rate limiting: per-client-id throttling absent; distributed credential brute-force viable. ASVS V6.2 brute force protection partially implemented. CONCERN.

Finding 2 — OIDC End Session without id_token_hint: OIDC RP-Initiated Logout RECOMMENDED not satisfied; session forcing attack viable. CONCERN.

Neither finding reaches MUST/MUST NOT level. "Acceptable gaps" characterization is factually not an argument against compliance — it is an operational risk acceptance statement that does not modify the CONCERN findings. The team should document the risk acceptance for both gaps with their threat model justification.

Combined verdict: CONCERN. N CONCERNs = CONCERN (graduation rule).

### Reason
Both findings are SHOULD/RECOMMENDED level. Graduation rule: N CONCERNs do not escalate to REPROCESS. Team's risk acceptance does not eliminate the findings — CONCERN remains.

### Phrasing-B
CONCERN — two RECOMMENDED/SHOULD-level gaps. Graduation rule: CONCERN + CONCERN = CONCERN.

IP-only rate limiting: per-client-id throttling absent. Distributed brute-force viable. CONCERN.
OIDC End Session: id_token_hint absent. Session forcing viable. CONCERN.
Team risk acceptance: noted but does not change the compliance finding. Both gaps present. N CONCERNs ≠ REPROCESS. Verdict: CONCERN.

---

## Pair[CB4-3.4-822]

### A
Cross-source input — ext31 combination (per-client rate limiting + OIDC End Session with id_token_hint):

Two controls confirmed correct: (1) token endpoint per-client-id rate limiting: 30 failed attempts per 15-minute window per client_id; 429 + Retry-After on breach; distributed attacks cannot bypass per-client counter; (2) OIDC End Session requires id_token_hint with full 5-step validation (signature, sub match, aud match, exp, session-only termination).

### B
PASS. Both brute-force protection and session termination security correctly implemented.

Token endpoint rate limiting: per-client-id counter correctly addresses the distributed credential attack vector (botnet cannot bypass a per-client-id counter regardless of IP count). 429 + Retry-After provides correct HTTP signaling. ASVS V6.2 brute force protection satisfied.

OIDC End Session: id_token_hint required and fully validated. Session forcing blocked — only the holder of a valid, unexpired ID token for the specific client can trigger logout. OIDC RP-Initiated Logout RECOMMENDED satisfied at required level.

No findings.

### Reason
Per-client-id rate limiting and id_token_hint both implemented at levels above their respective SHOULD/RECOMMENDED thresholds. PASS.

### Phrasing-B
PASS — per-client-id token endpoint rate limiting and id_token_hint End Session both confirmed correct.

Per-client-id: 30/15min counter; 429 on breach; botnet-proof. ASVS V6.2 satisfied.
End Session: id_token_hint required; 5-step validation; session forcing blocked. RP-Initiated Logout RECOMMENDED satisfied. PASS.

---

## Pair[CB4-3.4-823]

### A
Cross-source input — ext31 + ext30 combination (HS256 multi-party + ROPC enabled):

An authorization system review finds two grant/signing security failures: (1) JWTs signed with HS256; same HMAC secret distributed to all 4 resource server teams; any RS compromise yields universal token forgery capability; (2) ROPC (grant_type=password) is enabled — users post credentials directly to /token; application collects username and password before sending to AS. Team: "Both are legacy choices that work reliably in our system."

### B
REPROCESS. Two independent MUST-level violations. Legacy status does not constitute a normative exception to MUST requirements.

Finding 1 — RFC 8725 §3.6 + V9.1.1 (HS256 multi-party): HMAC secret distributed to 4 RS teams = 4 token-forgery attack surfaces. RFC 8725 §3.6 requires asymmetric keys for multi-party token systems. REPROCESS trigger.

Finding 2 — RFC 9700 §2.4 MUST NOT ROPC: Application collects user credentials — OAuth trust boundary eliminated. AS cannot enforce MFA in ROPC flow. Phishing applications indistinguishable. RFC 9700 §2.4 MUST NOT: violated. REPROCESS trigger.

The compound risk: HS256 multi-party allows forged tokens to be accepted by all RSes; ROPC allows attackers to harvest user credentials directly. Both attacks independently provide full system access.

### Reason
RFC 8725 §3.6 and RFC 9700 §2.4 independently violated. "Legacy and reliable" is not an exemption from either MUST requirement.

### Phrasing-B
REPROCESS — HS256 multi-party forgery risk and ROPC credential exposure: two independent MUST violations.

RFC 8725 §3.6: HS256 shared to 4 RSes. Legacy reliability does not address multi-party signing security. REPROCESS.
RFC 9700 §2.4 MUST NOT: ROPC active. Application collects credentials. OAuth trust boundary absent. MFA unenforceable. Legacy status not a MUST NOT exception. REPROCESS.

---

## Pair[CB4-3.4-824]

### A
Cross-source input — ext31 + ext30 combination (RS256 + ROPC disabled):

Correct implementation: (1) tokens signed with RS256; AS holds private key exclusively; RSes use HTTPS JWKS public key; no RS has token forgery capability; (2) ROPC disabled; all user auth through authorization code + PKCE S256; AS controls authentication UI; MFA enforceable.

### B
PASS. Both multi-party signing security and ROPC replacement correctly implemented.

RFC 8725 §3.6: RS256 asymmetric signing with AS-exclusive private key. RS compromise cannot enable token forgery. Multi-party signing security satisfied.

RFC 9700 §2.4 MUST NOT: ROPC disabled. Authorization code + PKCE S256 replaces it. Application never collects user credentials. AS trust boundary maintained. MFA enforceable at AS UI.

No findings.

### Reason
RFC 8725 §3.6 and RFC 9700 §2.4 both satisfied. Defense-in-depth: no token forgery path AND no credential harvesting by application.

### Phrasing-B
PASS — RS256 asymmetric signing and ROPC replaced by authorization code + PKCE S256.

RS256: private key in AS; RSes use HTTPS JWKS public key; RS compromise cannot forge tokens. §3.6 satisfied.
ROPC: disabled. Auth code + PKCE S256. Application never receives credentials. MFA enforceable. §2.4 MUST NOT satisfied. PASS.

---

## Pair[CB4-3.4-825]

### A
Cross-source input — ext31 + PO.1 escalation (AS no PKCE enforcement traceable to training gap):

A security audit of an authorization server finds: (1) the AS does not require PKCE for public clients — a React SPA and a native mobile app both operate without code_challenge; (2) SSDF PO.1 audit: the engineer who configured the AS was not provided training on RFC 9700 (BCP 240); the engineer states: "I configured OAuth based on RFC 6749. I was not aware that RFC 9700 changed the requirements for PKCE — I thought PKCE was an optional client-side feature." The configuration decision is directly traceable to the knowledge gap.

### B
REPROCESS. RFC 9700 §2.1.1 MUST violated in production, with the violation directly traceable to a SSDF PO.1 training gap.

Primary finding — RFC 9700 §2.1.1 MUST: AS accepts public client authorization code requests without PKCE. Two public clients operate without code interception protection. REPROCESS.

PO.1 escalation: SSDF PO.1 requires organizations to maintain security training for personnel with security-critical responsibilities. Configuring an OAuth authorization server — specifically its PKCE enforcement policy — is a security-critical responsibility. The engineer's statement confirms they were trained on RFC 6749 but not on RFC 9700, which superseded RFC 6749's optional treatment of PKCE with a MUST requirement. The absence of BCP-current training is the proximate cause of the §2.1.1 MUST violation. PO.1 gap + traceable production MUST violation = REPROCESS (escalation from CONCERN).

REPROCESS on two independent grounds.

### Reason
RFC 9700 §2.1.1 MUST is independently REPROCESS. PO.1 escalation: engineer's explicit statement of unawareness of RFC 9700 establishes the causal chain from training gap to production MUST violation.

### Phrasing-B
REPROCESS — RFC 9700 §2.1.1 MUST not enforced; PO.1 training gap confirmed proximate cause.

§2.1.1: AS accepts non-PKCE public clients. React SPA and native app operate without PKCE. Production MUST violation confirmed. REPROCESS.

PO.1 escalation: engineer trained on RFC 6749 but not on RFC 9700. Unaware that RFC 9700 made PKCE mandatory for public clients. SSDF PO.1: personnel with security-critical responsibilities must receive current training. AS PKCE enforcement policy is security-critical; RFC 9700 BCP updates should be part of security training. Knowledge gap → MUST violation. PO.1 absence + traceable production MUST = REPROCESS. REPROCESS.

---

## Pair[CB4-3.4-826]

### A
Cross-source input — ext31 + ext29 combination (HS256 multi-party + access tokens without exp):

An authorization system has two compounding token security failures: (1) JWT access tokens are signed with HS256; same HMAC secret distributed to 3 resource server teams; (2) access tokens have no exp claim — tokens are non-expiring at the resource server. Team: "HS256 is simple and we don't need exp because we manage revocation through our backend API."

### B
REPROCESS. Two independent MUST-level violations with compounding risk.

Finding 1 — RFC 8725 §3.6 + V9.1.1 (HS256 multi-party): Shared HMAC secret with 3 RS teams creates 3 token-forgery attack surfaces. Any RS compromise → universal token forgery across all RSes. REPROCESS trigger.

Finding 2 — RFC 9068 §2.2 MUST exp absent: Non-expiring access tokens at the RS. Revocation API requires active intervention for every revocation event; without exp, blocklist entries must be maintained indefinitely with no natural TTL-based cleanup. REPROCESS trigger.

Compounding risk: if an attacker obtains the HS256 HMAC secret (via RS compromise), they can forge non-expiring access tokens — tokens that have no time boundary at the RS. The revocation API cannot revoke tokens that haven't been issued through normal channels; the attacker can generate fresh forged tokens indefinitely. The combination of forgeability (HS256 multi-party) and non-expiry (no exp claim) creates a particularly severe compound vulnerability.

### Reason
RFC 8725 §3.6 and RFC 9068 §2.2 independently violated. The compound risk — forgeable AND non-expiring tokens — makes this a particularly severe combination.

### Phrasing-B
REPROCESS — HS256 forgeable tokens AND non-expiring tokens: compounding severity.

RFC 8725 §3.6: HMAC secret to 3 RSes. RS compromise → universal token forgery. REPROCESS.

RFC 9068 §2.2: no exp claim. Non-expiring at RS. Revocation API manages normal-channel revocations; cannot revoke forged tokens. If attacker obtains HS256 secret: they can forge tokens that the RS accepts indefinitely (no exp → no time boundary, no natural expiry). Compound: forgeable + non-expiring. REPROCESS.

---

## Pair[CB4-3.4-827]

### A
Cross-source input — ext31 combination (IP-only rate limiting + AS no PKCE enforcement):

Two security gaps identified: (1) token endpoint rate limiting is IP-based only — per-client-id throttling absent; distributed credential brute-force viable for known client_ids; (2) the AS does not enforce PKCE for public clients — authorization code requests without code_challenge are accepted; React SPA operates without PKCE. Team: "The rate limiting covers most attack scenarios, and PKCE will be required in a future release."

### B
REPROCESS. The REPROCESS determination is driven by Finding 2 (MUST violation); Finding 1 produces a CONCERN that is absorbed.

Finding 1 — Token endpoint IP-only rate limiting: per-client-id throttling absent; distributed brute-force viable. ASVS V6.2 partial implementation. No MUST specifying per-client mechanism. CONCERN. Absorbed by REPROCESS.

Finding 2 — RFC 9700 §2.1.1 MUST (PKCE not enforced for public clients): AS accepts non-PKCE public client authorization code requests. React SPA operates without code interception protection. "Future release" deferral does not satisfy a current MUST. RFC 9700 §2.1.1 MUST: violated. REPROCESS trigger.

Overall verdict: REPROCESS (from Finding 2). Graduation rule: REPROCESS overrides CONCERN. The CONCERN finding from IP-only rate limiting is absorbed.

### Reason
RFC 9700 §2.1.1 MUST violation determines verdict. IP-only rate limiting CONCERN is absorbed. REPROCESS overrides CONCERN per graduation rule. "Future release" is not a current-MUST-deferral argument.

### Phrasing-B
REPROCESS — RFC 9700 §2.1.1 MUST drives verdict. IP-only rate limiting CONCERN absorbed.

IP-only rate limiting: CONCERN (per-client-id absent; distributed brute-force viable). Absorbed by REPROCESS determination.

RFC 9700 §2.1.1: AS accepts non-PKCE public client requests. React SPA unprotected against code interception. "Future release" argument: MUST applies to the current system; deferral to a future release does not satisfy a current requirement. REPROCESS.

Graduation rule: REPROCESS overrides CONCERN. Verdict: REPROCESS.

---

## Pair[CB4-3.4-828]

### A
Cross-source input — ext31 comprehensive PASS (all patterns correctly implemented):

A complete authorization system review covers all ext31 domains: (1) AS enforces PKCE S256 for all public client authorization code requests; non-PKCE requests return 400 (RFC 9700 §2.1.1); (2) JWT access tokens signed with RS256; AS holds private key exclusively; RSes use HTTPS JWKS public key; no RS has signing capability (RFC 8725 §3.6); (3) token endpoint per-client-id rate limiting: 30 attempts/15min per client_id with graduated response; layered with IP rate limiting (ASVS V6.2); (4) OIDC End Session requires id_token_hint with 5-step validation; session forcing blocked (OIDC RP-Initiated Logout RECOMMENDED).

### B
PASS. All four ext31 security control domains correctly implemented. No findings.

RFC 9700 §2.1.1 MUST: AS enforces PKCE for public clients; non-PKCE requests rejected. Code interception attack vector eliminated at AS enforcement level.

RFC 8725 §3.6: RS256 asymmetric signing; private key exclusively in AS secrets manager; RSes use public key from HTTPS JWKS for verification only. RS compromise cannot enable token forgery.

ASVS V6.2: per-client-id rate limiting directly addresses distributed credential brute-force; graduated response (429 + rotation flag) provides immediate and sustained protection. Layered with IP rate limiting for defense-in-depth.

OIDC RP-Initiated Logout RECOMMENDED: id_token_hint required and fully validated; only session token holders can trigger logout; session forcing attack blocked.

No REPROCESS triggers. No CONCERN findings. All controls meet or exceed their normative requirements.

### Reason
All four ext31 domains — §2.1.1 MUST, §3.6 multi-party signing, V6.2 brute force, RP-Initiated Logout — satisfied at or above normative threshold.

### Phrasing-B
PASS — all ext31 domains correctly implemented.

(1) RFC 9700 §2.1.1: AS enforces PKCE for public clients; non-PKCE → 400. Code interception eliminated.
(2) RFC 8725 §3.6: RS256, AS private key exclusive, RSes use HTTPS JWKS public key; RS compromise ≠ forgery.
(3) ASVS V6.2: per-client-id 30/15min + 429 + rotation flag; botnet-proof; layered with IP.
(4) RP-Initiated Logout RECOMMENDED: id_token_hint required + 5-step validation; session forcing blocked.

All requirements met. PASS.
