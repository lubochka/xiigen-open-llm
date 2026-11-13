# CB4 Stage 3 — Node 08 Arbiter Set Extension 20
# Sequences CB4-3.4-517 through CB4-3.4-524
# Stratum: A/B/D — baselines, team context, adversarial rejection
# New patterns: OIDC hybrid flow, JWE (JWT encryption), token binding DPoP+aud, state entropy
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 1048 instances

---

## Pair[CB4-3.4-517]

**A:** OIDC hybrid flow review. Application uses OIDC hybrid flow with `response_type=code id_token`. The authorization response returns both an authorization code and an ID token directly in the redirect. Team: "Hybrid flow is defined in OIDC Core — it's a legitimate OIDC response type."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

V10.x (response_type policy) / OAuth 2.0 BCP (RFC 9700): REPROCESS
Team argument: "hybrid flow is defined in OIDC Core"

Hybrid flow response_type=code id_token analysis:
  (1) ID token returned in authorization response (front-channel redirect) ✗
      ID token contains user claims — delivered through browser redirect URL
      URL appears in browser history, Referer headers, server access logs ✗
  (2) Authorization code returned in front-channel (same concern as implicit token) ✗
  (3) ID token in front-channel not protected by PKCE — PKCE protects the code exchange,
      not the front-channel ID token delivery
  (4) OAuth 2.0 BCP (RFC 9700) §2.1.2: response types that return tokens or ID tokens
      directly from the authorization endpoint in the front channel are discouraged for
      the same reasons implicit flow is deprecated

V10.4.7 / response_type policy:
  ASVS V10: response_type=code (code-only) with PKCE is the required pattern
  Hybrid flow returns an ID token in the front channel → REPROCESS
  "Defined in OIDC Core" → spec defines it; BCP and ASVS restrict it for security reasons
  Same argument pattern as implicit flow: "RFC 6749 supports it" → REJECTED

Finding: hybrid flow response_type=code id_token returns ID token in front channel
Overall verdict: REPROCESS
```

**Reason:** OIDC hybrid flow (`response_type=code id_token`) returns an ID token directly in the authorization response (front-channel URL). This exposes the ID token to browser history and logs, same class of issue as implicit flow. OAuth 2.0 BCP (RFC 9700) discourages front-channel token delivery. "OIDC Core defines it" is rejected — the spec defines it; BCP and ASVS restrict it. REPROCESS.

**Phrasing-B:** OIDC hybrid flow with response_type=code id_token — ID token returned in authorization redirect. Team: "Hybrid flow is in OIDC Core spec." Is hybrid flow compliant?

---

## Pair[CB4-3.4-518]

**A:** Code-only OIDC flow correctly implemented. Application uses OIDC with `response_type=code` only. After code exchange at token endpoint, the application calls the UserInfo endpoint to retrieve user claims. ID token issued at token endpoint — never in the front-channel URL. PKCE S256 enforced. Team: "We use code-only flow — the ID token is never in the URL."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

T-oauth-oidc:
  response_type=code only ✓ — no front-channel token delivery ✓
  ID token issued at token endpoint (back-channel) ✓ — not in URL ✓
  UserInfo endpoint used for claims ✓ — additional back-channel call ✓
  PKCE S256 enforced ✓ — V10.4.6 satisfied ✓

Code-only OIDC flow: the correct pattern per ASVS V10 and OAuth 2.0 BCP
  Authorization response: only contains code (opaque) ✓
  ID token: delivered via back-channel token response ✓
  All sensitive data protected from URL exposure ✓

Overall verdict: PASS
```

**Reason:** Code-only OIDC flow with PKCE S256: authorization response contains only the code; ID token delivered via back-channel token endpoint response. No front-channel token exposure. Correct implementation per ASVS V10 and OAuth 2.0 BCP. PASS.

**Phrasing-B:** OIDC code-only flow. ID token issued only at token endpoint — never in authorization redirect URL. PKCE S256. Team: "ID token never in URL." Is this the correct pattern?

---

## Pair[CB4-3.4-519]

**A:** State parameter entropy review. OAuth authorization request includes a `state` parameter. State is generated as a 6-character alphanumeric string (e.g., `a7B3kQ`). State is validated at callback. Team: "State is present and validated — CSRF is prevented."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

V10.2.6 (state parameter) / RFC 6749 §4.1.1: CONCERN
Team: "state present and validated → CSRF prevented"

State entropy analysis:
  6-character alphanumeric: 62^6 = ~56 billion combinations
  Sounds large, but:
    (1) With high request volumes, brute-force guessing becomes feasible
    (2) No minimum entropy specified in RFC 6749 / ASVS for state exactly, but
        ASVS V10.2.6 requires state to be unguessable / cryptographically random
    (3) 6 characters from 62-char alphabet ≈ 35.7 bits of entropy
        Recommended minimum: 128 bits (similar to nonce, CSRF token guidance)
    (4) Common CSRF token standards: ≥ 128 bits cryptographically random
  6-character state: entropy below recommended threshold ✗

Team argument: "state present and validated → CSRF prevented"
  State validation is correct ✓; the concern is the entropy of the value being validated
  Low-entropy state: a guessed or brute-forced state can be injected into the callback
  → "Validation present = sufficient" REJECTED (entropy is the other half)

Finding level: CONCERN
  State is present and validated ✓ (mechanism correct)
  Entropy below recommended 128-bit threshold ✗ (value security gap)
Overall verdict: CONCERN
```

**Reason:** State parameter is present and validated (mechanism correct) but 6-character alphanumeric yields ~35.7 bits of entropy — well below the 128-bit minimum recommended for unpredictable state values. "State validated = CSRF prevented" is rejected as incomplete: validation correctness depends on the value being unguessable. CONCERN (not REPROCESS) because the mechanism exists; the entropy gap is the concern.

**Phrasing-B:** OAuth state parameter: 6-character alphanumeric, validated at callback. Team: "State is present and validated — CSRF is prevented." Is 6-character state sufficient?

---

## Pair[CB4-3.4-520]

**A:** State parameter correctly implemented. OAuth authorization request: state generated as 32 bytes from a cryptographically secure random number generator (CSPRNG), Base64url-encoded (256-bit entropy). State stored in server-side session before redirect. Validated at callback: state in response must match session-stored value. Single-use: state invalidated after use.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

V10.2.6 (state parameter): PASS
  32 bytes from CSPRNG: 256-bit entropy ✓ — far exceeds 128-bit minimum
  Base64url-encoded ✓ — URL-safe representation
  Server-side session storage before redirect ✓ — not exposed in URL as sole copy
  Callback validation: response state == session state ✓
  Single-use: invalidated after successful validation ✓

State security guarantees:
  CSRF binding: authorization response bound to the session that initiated the request ✓
  Guessing infeasible: 256-bit CSPRNG entropy ✓
  Replay protection: single-use enforcement ✓

Overall verdict: PASS
```

**Reason:** State with 256-bit CSPRNG entropy, server-side session storage, callback value match, and single-use enforcement satisfies V10.2.6 completely. PASS.

**Phrasing-B:** OAuth state: 32 bytes CSPRNG = 256-bit entropy, stored in session, validated at callback, single-use. Does this satisfy V10.2.6?

---

## Pair[CB4-3.4-521]

**A:** JWE (JSON Web Encryption) review — JWT payload not encrypted. Application uses JWTs as access tokens. JWTs are signed (RS256) but not encrypted. JWT payload contains: `sub`, `iss`, `aud`, `exp`, `iat`, `scope`. Team: "JWT payload is not sensitive — it contains only standard claims. Encryption would add unnecessary overhead."

**B:**
```
Arbiter: ASVS-V9-Token + RFC-JWT

T-asvs-token / RFC 7516 (JWE): PASS
JWT payload contents: sub, iss, aud, exp, iat, scope — standard claims
  sub: opaque identifier (not PII (Personal Identifiable Information)) ✓
  iss, aud, exp, iat: technical metadata ✓
  scope: authorization scope values ✓

Analysis:
  ASVS V9 / RFC 7519: JWT signing (JWS) is required ✓ (RS256 ✓)
  JWE (encryption) is specified in RFC 7516 but is NOT mandated for all JWT use cases
  JWE is appropriate when:
    - Payload contains PII or sensitive user data
    - The token will pass through intermediaries that should not read it
  Standard access token claims (sub, iss, aud, exp, scope): do not inherently require encryption
  Team argument: correct analysis for standard-claim access tokens ✓

Finding: no encryption gap for standard-claim JWTs
Note: if PII (name, email, address) were added to payload, JWE would become appropriate
Overall verdict: PASS
```

**Reason:** JWT encryption (JWE) is not universally required — it is appropriate when payload contains PII or passes through untrusted intermediaries. Access tokens with only standard claims (sub, iss, aud, exp, scope) do not require encryption. Signing (RS256) is the required protection. PASS. Note: adding PII to payload would change this assessment.

**Phrasing-B:** JWT access tokens signed (RS256) but not encrypted. Payload contains only sub, iss, aud, exp, iat, scope. Team: "Standard claims don't need encryption." Is unencrypted JWT acceptable for standard-claim access tokens?

---

## Pair[CB4-3.4-522]

**A:** JWE required for sensitive payload — absent. Application uses JWTs as access tokens. JWT payload contains: `sub`, `email`, `name`, `phone_number`, `address`, `iss`, `aud`, `exp`. JWTs are signed but not encrypted. JWTs are sent in Authorization headers and also stored in browser localStorage (accessible to JavaScript). Team: "Headers protect the JWT in transit — HTTPS is used."

**B:**
```
Arbiter: ASVS-V9-Token + RFC-JWT

T-asvs-token:
  JWT payload: email, name, phone_number, address — PII (Personal Identifiable Information) ✓
  JWT not encrypted (no JWE) ✗
    PII claims visible to: any JavaScript code, browser extensions, XSS attackers,
      any party that can read localStorage ✗
    PII exposure through localStorage: JavaScript-accessible storage ✗

  Team argument: "HTTPS protects in transit"
    HTTPS protects the JWT body during network transmission ✓
    HTTPS does NOT protect the JWT contents once it is stored in localStorage:
      - XSS: any injected script can read localStorage → full PII exposure ✗
      - Browser extensions with read access to localStorage ✗
      - User who shares device → localStorage visible ✗
    → "HTTPS in transit = content protected" REJECTED (storage is a separate attack surface)

  V9 / RFC 7516 JWE: when JWT contains PII and is stored in JavaScript-accessible storage,
    encryption (JWE) is appropriate to prevent PII disclosure from token content
    → REPROCESS for PII in unencrypted JWT stored in localStorage

T-asvs-token verdict: REPROCESS
Overall verdict: REPROCESS
```

**Reason:** JWT with PII claims (email, name, phone, address) stored unencrypted in localStorage exposes PII to any XSS attack or JavaScript reader. HTTPS protects transit only — it does not protect JWT content in client-side storage. JWE is appropriate when PII is in the payload and the token is accessible to JavaScript. REPROCESS.

**Phrasing-B:** JWT payload contains email, name, phone, address. Signed but not encrypted. Stored in localStorage. Team: "HTTPS protects it in transit." Is unencrypted PII-containing JWT in localStorage acceptable?

---

## Pair[CB4-3.4-523]

**A:** DPoP bound token plus aud validation. API: access tokens are DPoP-bound (RFC 9449). DPoP proof required on all API requests. Additionally: DPoP access tokens include `aud` claim specifying the resource server's URL. Resource server validates: DPoP proof (jti replay, htm, htu, iat); token signature; `aud` claim matches resource server URL. Team: "DPoP binds the token to the key; aud binds it to this resource server — together they prevent any token misuse."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT

T-oauth-oidc:
  DPoP implementation (RFC 9449):
    DPoP proof: jti replay detection ✓; htm (HTTP method) ✓; htu (HTTP URL) ✓; iat ✓
    Access token bound to DPoP public key via cnf/jkt ✓
    → PASS (DPoP correctly implemented)

  aud claim on resource server:
    aud = resource server URL ✓
    Resource server validates aud matches its own identifier ✓
    → Prevents token issued for one resource server from being used at another ✓

T-rfc-jwt:
  aud validated at resource server ✓ — PASS (RFC 7519 §4.1.3)
  Token signature validated ✓ — PASS

Security guarantees:
  DPoP: token cannot be used without possession of the corresponding private key ✓
  aud: token cannot be used at a different resource server ✓
  jti: DPoP proofs cannot be replayed ✓
  Together: sender-constrained by key (DPoP) + audience-constrained by aud ✓

All territories: PASS
Overall verdict: PASS
```

**Reason:** DPoP binding (sender-constraining via key pair) combined with aud validation (resource-server-constraining) provides two independent layers of token misuse prevention. Team characterization is correct. PASS.

**Phrasing-B:** DPoP-bound tokens with full proof validation. aud claim validated against resource server URL. Team: "DPoP constrains who; aud constrains where." Does this dual-binding produce a PASS?

---

## Pair[CB4-3.4-524]

**A:** Comprehensive review — all ext20 patterns correctly implemented. Application: OIDC code-only flow (no hybrid, no implicit); state with 256-bit CSPRNG entropy, session-stored, single-use validated; JWT access tokens with only standard claims (no PII) — signed RS256 2048-bit; DPoP enforced; aud validated; PAR required for all clients; nonce validated (value + single-use); all prior controls correct (PKCE S256, protected dynamic registration, TTL+pub/sub cache, minimum-scope M2M, etc.).

**B:**
```
Arbiter: Multiple

T-oauth-oidc:
  response_type=code only (no hybrid, no implicit) ✓ — PASS
  PAR mandatory: request_uri-only ✓ — PASS
  OIDC nonce: value match + single-use ✓ — PASS
  DPoP enforced: proof validation (jti, htm, htu, iat) ✓ — PASS
  Dynamic registration: initial access token + vetting ✓ — PASS

T-rfc-jwt:
  JWT payload: standard claims only (no PII) ✓ — no JWE required → PASS
  RS256 2048-bit signing ✓ — PASS
  aud validated at resource server ✓ — PASS
  DPoP cnf/jkt binding ✓ — PASS

T-rfc-oauth2:
  State: 256-bit CSPRNG entropy + session storage + single-use ✓ — PASS
  Introspection cache: TTL + pub/sub invalidation ✓ — PASS
  Revocation: client auth ✓ — PASS

T-asvs-authorization:
  M2M minimum-scope tokens ✓ — PASS

T-asvs-authentication:
  Argon2id ✓ — PASS

T-ssdf-process:
  Vault ✓; Security Champion with checklist ✓ — PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** All ext20 patterns correctly implemented: code-only OIDC (no hybrid), PAR mandatory, 256-bit state, standard-claim JWT (no encryption needed), DPoP with full proof validation, aud at resource server, plus all prior controls. Every territory returns PASS. Comprehensive multi-territory PASS.

**Phrasing-B:** Code-only OIDC, PAR mandatory, 256-bit state, standard-claim JWT, DPoP enforced, aud validated, plus all prior controls. Does this comprehensive implementation pass all territories?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-517 | OIDC hybrid flow response_type=code id_token — "OIDC Core defines it" rejected | B/D | V10-OAuth + RFC-OAuth2 | REPROCESS |
| CB4-3.4-518 | OIDC code-only flow — ID token only at token endpoint, PKCE S256 | A | V10-OAuth + RFC-OAuth2 | PASS |
| CB4-3.4-519 | State parameter 6-char alphanumeric → CONCERN (~35 bits entropy) | B | V10-OAuth + RFC-OAuth2 | CONCERN |
| CB4-3.4-520 | State parameter 256-bit CSPRNG, session-stored, single-use | A | V10-OAuth + RFC-OAuth2 | PASS |
| CB4-3.4-521 | JWT standard-claim payload unsigned-encrypted — JWE not required | A | V9-Token + RFC-JWT | PASS |
| CB4-3.4-522 | JWT with PII in payload, unencrypted, stored in localStorage — JWE required | B/D | V9-Token + RFC-JWT | REPROCESS |
| CB4-3.4-523 | DPoP binding + aud validation at resource server — dual-binding PASS | A | V10-OAuth + RFC-JWT | PASS |
| CB4-3.4-524 | All ext20 patterns correctly implemented — comprehensive multi-territory PASS | B | Multiple | PASS |
