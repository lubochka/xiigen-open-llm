# CB4 Stage 3 — Node 08 Arbiter Set Extension 21
# Sequences CB4-3.4-543 through CB4-3.4-550
# Stratum: A/B/D — baselines, team context, adversarial rejection
# New patterns: client_credentials flow, mTLS (RFC 8705), UserInfo endpoint validation,
#   SSDF PO.1 organizational security training, refresh token family detection
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 1100 instances

---

## Pair[CB4-3.4-543]

**A:** client_credentials flow security review. Authorization server: client_credentials grant for M2M (machine-to-machine) services. Client authentication: `client_id` + `client_secret` in request body (`POST /oauth/token` with `grant_type=client_credentials`). Client secrets: 16-character random alphanumeric strings generated at registration. Secrets stored hashed (bcrypt cost=12) in the authorization server database. Team: "client_credentials is the correct grant type for M2M — we authenticate clients with a secret."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

V10.x (client_credentials grant) / RFC 6749 §4.4: PASS
  grant_type=client_credentials: correct grant for M2M ✓
  Client authentication: client_id + client_secret in POST body ✓
    RFC 6749 §2.3.1: client credentials MAY be sent in request body ✓
      (HTTP Basic auth is preferred but POST body is allowed)
  Client secret strength:
    16-character alphanumeric: 62^16 ≈ 4.7 × 10^28 combinations ≈ 95 bits entropy ✓
    Meets minimum secret length requirements ✓
  Secrets hashed with bcrypt cost=12 ✓ — appropriate storage for client secrets ✓
    (bcrypt at authorization server protects against database breach)

Note: mTLS (mutual TLS — RFC 8705) would provide stronger client authentication
  than shared secrets — but client secret authentication is a valid, compliant option
  per RFC 6749. Absence of mTLS is not a CONCERN for client_credentials flow using
  strong secrets.

Overall verdict: PASS
```

**Reason:** client_credentials grant with strong client secrets (95 bits entropy) and bcrypt cost=12 storage is correct and compliant. RFC 6749 allows client credentials in POST body. mTLS would be stronger but is not required for compliance. PASS.

**Phrasing-B:** client_credentials grant with 16-char random alphanumeric secret, bcrypt cost=12 at auth server. Is this a compliant M2M authentication approach?

---

## Pair[CB4-3.4-544]

**A:** mTLS client authentication review. Authorization server supports mutual TLS (mTLS — RFC 8705) for client authentication on the token endpoint. Clients authenticate by presenting a client certificate. Certificate binding: access tokens include `cnf/x5t#S256` claim (SHA-256 thumbprint of client certificate). Resource server validates `cnf/x5t#S256` against the certificate presented on the mTLS connection. Team: "mTLS provides the strongest client authentication — certificate binding prevents token misuse."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

RFC 8705 (mTLS client authentication + certificate-bound tokens): PASS
  Token endpoint mTLS: client presents certificate ✓ — client authentication via certificate ✓
  Certificate-bound access tokens:
    cnf/x5t#S256 claim: SHA-256 thumbprint of client certificate in token ✓ (RFC 8705 §3)
    Resource server validates: certificate presented on mTLS connection matches cnf/x5t#S256 ✓
    Token cannot be used by a party without the corresponding client certificate ✓

mTLS vs client_secret comparison:
  mTLS: certificate-based; proof of possession via TLS handshake ✓
    Stronger than client_secret: certificate cannot be guessed; requires key possession ✓
    Certificate-bound tokens: additional layer binding token to certificate ✓
  Both are compliant options; mTLS is the stronger configuration ✓

Overall verdict: PASS
```

**Reason:** mTLS client authentication with certificate-bound tokens (`cnf/x5t#S256`) correctly implements RFC 8705. Resource server validates certificate binding on each request. This is the strongest form of client authentication. PASS.

**Phrasing-B:** mTLS client authentication with cnf/x5t#S256 certificate binding in access tokens. Resource server validates certificate match. Is this the correct RFC 8705 implementation?

---

## Pair[CB4-3.4-545]

**A:** UserInfo endpoint validation review. OIDC login. After receiving the ID token (code-only flow, PKCE S256, nonce validated), the application calls the UserInfo endpoint to retrieve user claims (`email`, `name`, `sub`). UserInfo response: JSON with claims. Application uses claims directly without validation. Team: "UserInfo claims come from the same IdP that issued the ID token — they are already trusted."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT

V10.x / OIDC Core §5.3 (UserInfo response): CONCERN
Team argument: "same IdP = already trusted"

UserInfo response validation requirements (OIDC Core §5.3.4):
  If UserInfo response is a JWT: signature MUST be validated ✓ (if returned as signed JWT)
  If UserInfo response is plain JSON: the sub claim MUST match the sub in the ID token ✗

Application behavior: uses JSON claims without sub-claim cross-check
  Risk: if UserInfo endpoint returns a different sub than the ID token, the application
    would bind claims from a different user to the authenticated session
  Attack scenario: if network interception or confused-deputy can substitute a different
    UserInfo response, claims from another user can be injected

  "Same IdP = trusted" argument:
    The connection is HTTPS ✓ — network interception mitigated for well-configured TLS
    However: application should still validate sub cross-check as a correctness guarantee
    → CONCERN (not REPROCESS — JSON UserInfo sub cross-check is a defensive practice;
      HTTPS mitigates the primary network threat; but the validation step is absent)

Finding level: CONCERN — sub cross-check absent; HTTPS mitigates primary risk
Overall verdict: CONCERN
```

**Reason:** OIDC UserInfo response (plain JSON) should include a `sub` cross-check against the ID token's `sub` claim. The application doesn't perform this check. CONCERN (not REPROCESS) because HTTPS mitigates the primary substitution attack, but the `sub` cross-check is a recommended defensive step per OIDC Core. "Same IdP = trusted" is insufficient reasoning for skipping validation.

**Phrasing-B:** OIDC UserInfo endpoint returns JSON claims used without validating sub against the ID token. Team: "Claims come from the same IdP — they're trusted." Is sub cross-check required?

---

## Pair[CB4-3.4-546]

**A:** UserInfo correctly validated. OIDC flow: code-only, PKCE S256, nonce validated. UserInfo endpoint called after token exchange. Application validation: (1) UserInfo response `sub` claim compared to ID token `sub` — rejected if different; (2) If UserInfo returns a JWT, signature verified against JWKS. Claims accepted only after validation.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT

V10.x / OIDC Core §5.3.4 (UserInfo response validation): PASS
  sub cross-check: UserInfo sub == ID token sub ✓ — binding confirmed ✓
  JWT UserInfo response: signature validated against JWKS ✓ (when applicable)
  Claims rejected if sub mismatch ✓ — confused-deputy protection ✓

OIDC Core §5.3.4 requirements:
  (1) sub claim in UserInfo MUST equal sub in ID token ✓ (verified)
  (2) If UserInfo is JWT: validate signature ✓

Overall verdict: PASS
```

**Reason:** UserInfo response validated: `sub` cross-check with ID token (confused-deputy protection) and JWT signature validation when applicable. Satisfies OIDC Core §5.3.4. PASS.

**Phrasing-B:** OIDC UserInfo: sub cross-checked against ID token sub, JWT signature validated. Does this satisfy OIDC Core UserInfo validation requirements?

---

## Pair[CB4-3.4-547]

**A:** SSDF PO.1 organizational security training review. Development organization: no formal security training program for developers. Security knowledge is ad-hoc — developers consult documentation when they encounter security issues. Team: "We hire experienced developers who know security — formal training is redundant."

**B:**
```
Arbiter: SSDF-PW

SSDF PO.1 (organizational security training): CONCERN
Team argument: "experienced developers know security — formal training redundant"

PO.1.1 (SSDF v1.1): organizations should establish and conduct role-based security training
  for all staff involved in software development
  Key principle: knowledge that "experienced developers should know" ≠ verified, current,
    consistent security knowledge across the team

Analysis:
  Ad-hoc security knowledge: varies by developer background ✗
  No shared vocabulary for security requirements ✗
  No structured coverage of OWASP Top 10, SANS CWE, etc. ✗
  No tracking of who has what security knowledge ✗
  Team composition turnover: new hires may have gaps uncaught without training ✗

Team argument analysis:
  "Experienced" is unverified without training assessment ✗
  Experience alone does not ensure coverage of current threat landscape ✗
  → "Experienced = trained" REJECTED

Finding level: CONCERN (not REPROCESS) — no training program is a process hygiene gap;
  training absence is typically CONCERN rather than REPROCESS unless tied to a specific
  active vulnerability traceable to knowledge gap
Overall verdict: CONCERN
```

**Reason:** No formal security training program violates SSDF PO.1 organizational intent. "Experienced developers don't need training" is rejected — experience varies by individual, doesn't cover current threats, and creates no consistent baseline. CONCERN (not REPROCESS) because training absence is a process hygiene gap rather than a specific absent control causing an active vulnerability.

**Phrasing-B:** No formal security training for developers. Team: "We hire experienced developers — formal training is redundant." Is absent formal training a CONCERN or REPROCESS?

---

## Pair[CB4-3.4-548]

**A:** Refresh token family detection review. Refresh token rotation implemented (single-use rotation). Family detection: if a refresh token is presented that has already been used (detected as a replay of a rotated-away token), the authorization server immediately revokes the entire token family (all current refresh tokens in that family). Team: "Replay detection plus family revocation — if a token is stolen and used before the legitimate client, we detect the theft and invalidate everything."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

V10.3 / Refresh token rotation + family detection: PASS
  Single-use rotation: each refresh produces a new refresh token ✓
  Family detection:
    Used (rotated-away) token presented → theft/replay detected ✓
    Family revocation: all tokens in family immediately revoked ✓
    (Prevents attacker who obtained an old token from continuing to use the family)

Security model:
  Token rotation: limits window from rotation to next rotation ✓
  Family detection: detects that rotation chain was breached (two parties have a token) ✓
  Family revocation: forces re-authentication → theft cannot persist silently ✓

This is the RECOMMENDED pattern per OAuth 2.0 Security BCP §4.14.1:
  "If replay is detected, all refresh tokens in the family SHOULD be revoked"
  Implementation matches BCP recommendation ✓

Overall verdict: PASS
```

**Reason:** Refresh token rotation with family detection and family revocation on replay detection is the recommended pattern per OAuth 2.0 Security BCP §4.14.1. Replay detection identifies that the rotation chain was breached; family revocation forces re-authentication to close the attack window. PASS.

**Phrasing-B:** Refresh token single-use rotation. Replay of rotated token triggers revocation of entire token family. Does this satisfy refresh token security requirements?

---

## Pair[CB4-3.4-549]

**A:** Refresh token rotation WITHOUT family detection. Single-use refresh token rotation implemented. When a rotated-away token is presented (replay detected), the server returns an error but does NOT revoke the current active refresh token for that family. Team: "Replay detection is in place — the old token is rejected, so the attacker can't use it."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

V10.3 / Refresh token rotation: CONCERN
Team argument: "replay detected = old token rejected = attacker blocked"

Analysis:
  Token rotation: single-use ✓
  Replay detection: old token rejected ✓
  Family revocation on replay: NOT implemented ✗

Race condition on token theft:
  Scenario A: attacker uses stolen token BEFORE legitimate client
    Attacker: rotated-away token → error (replay detected) ✓
    But wait: if attacker used the token BEFORE the legitimate client, the attacker now holds
      the CURRENT token (not the old one) ✓ — attacker's use was the successful rotation
    In this scenario the LEGITIMATE CLIENT presents the "old" token (their copy) → replay detected
    Without family revocation: current token (held by attacker) remains valid ✗
    → Attacker continues to have access; legitimate client is locked out ✗

  Family revocation closes this gap:
    On replay detection → revoke all → both parties forced to re-authenticate
    Forces the theft to surface (user must re-authenticate)

Finding: single-use rotation without family revocation fails to handle the stolen-token-used-first
  race condition
Level: CONCERN — rotation provides partial protection; family revocation adds necessary completeness
Overall verdict: CONCERN
```

**Reason:** Single-use rotation without family revocation fails when the attacker uses the stolen token first — the attacker holds the current token after successful rotation, and the legitimate client is the one triggering "replay detected" with the old copy. Without family revocation, the attacker's access persists. CONCERN (rotation provides partial protection; the gap is significant but not a complete absence of controls).

**Phrasing-B:** Refresh token single-use rotation. Replay detected but no family revocation. Team: "Old token is rejected — attacker is blocked." Is this argument correct?

---

## Pair[CB4-3.4-550]

**A:** Comprehensive review — all ext21 patterns correctly implemented. Application: client_credentials with strong secrets (96-bit entropy, bcrypt stored); mTLS available for high-security M2M services with certificate-bound tokens; OIDC UserInfo sub cross-check + JWT signature validation; formal developer security training program (annual, role-based, OWASP-based curriculum); refresh token single-use rotation with family detection and family revocation on replay; all prior controls correct.

**B:**
```
Arbiter: Multiple

T-oauth-oidc:
  client_credentials: strong secrets ✓ + mTLS option ✓ — PASS
  OIDC UserInfo: sub cross-check ✓; JWT sig validation ✓ — PASS
  Refresh rotation: single-use ✓; family detection + revocation on replay ✓ — PASS

T-rfc-oauth2:
  RFC 8705 mTLS + cnf/x5t#S256 certificate binding ✓ — PASS
  Refresh token BCP §4.14.1 family revocation ✓ — PASS

T-asvs-authentication:
  Argon2id ✓ — PASS

T-ssdf-process:
  Vault ✓; Security Champion with checklist ✓;
  SSDF PO.1: annual role-based security training ✓ — PASS

All prior controls:
  PKCE S256 ✓; code-only ✓; PAR mandatory ✓; OIDC nonce+aud ✓; DPoP ✓;
  state 256-bit ✓; no-PII JWT ✓; httpOnly cookie ✓; protected dynamic reg ✓;
  introspection TTL+pub/sub ✓; minimum-scope M2M ✓ — all PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** All ext21 patterns correctly implemented: strong client_credentials, mTLS with certificate binding, UserInfo validation, formal PO.1 training, refresh rotation with family revocation, plus all prior controls. Every territory returns PASS. Comprehensive multi-territory PASS.

**Phrasing-B:** Strong client_credentials, mTLS certificate binding, UserInfo sub cross-check, annual security training, refresh rotation with family revocation, all prior controls. Does this pass all territories?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-543 | client_credentials with strong secret + bcrypt storage | A | V10-OAuth + RFC-OAuth2 | PASS |
| CB4-3.4-544 | mTLS client auth + cnf/x5t#S256 certificate-bound tokens | A | V10-OAuth + RFC 8705 | PASS |
| CB4-3.4-545 | UserInfo sub cross-check absent — "same IdP = trusted" | B | V10-OAuth + RFC-JWT | CONCERN |
| CB4-3.4-546 | UserInfo sub cross-check + JWT sig validation | A | V10-OAuth + RFC-JWT | PASS |
| CB4-3.4-547 | No formal security training — "experienced developers know security" | B/D | SSDF-PW (PO.1) | CONCERN |
| CB4-3.4-548 | Refresh rotation + family detection + family revocation on replay | A | V10-OAuth + RFC-OAuth2 | PASS |
| CB4-3.4-549 | Refresh rotation without family revocation — attacker-first race condition | B | V10-OAuth + RFC-OAuth2 | CONCERN |
| CB4-3.4-550 | All ext21 patterns correctly implemented — comprehensive multi-territory PASS | B | Multiple | PASS |
