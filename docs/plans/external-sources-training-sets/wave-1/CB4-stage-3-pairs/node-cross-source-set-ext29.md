# CB4 Stage 3 — Cross-Source Pair Set ext29
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-949 through CB4-3.4-958 (10 base pairs × 2 phrasings = 20 instances)
# Source domains: ext36 patterns (PAR enforcement, offline_access scope, JWT typ, DPoP-bound-as-Bearer)
#   combined with each other and with earlier patterns
# Running total after this file: 120 + (958 − 60) × 2 = 1916 instances (958 base pairs)
# Graduation rule: REPROCESS overrides CONCERN; N CONCERNs alone ≠ REPROCESS
# PO.1 escalation: PO.1 absence + traceable production MUST violation = REPROCESS

---

## Pair[CB4-3.4-949-A]

**Territory:** T-rfc-oauth2 · T-rfc-oidc · T-oauth-oidc
**Domains combined:** PAR-enforcement-bypass (ext36) + offline_access-refresh-without-scope (ext36)
**Expected verdict:** REPROCESS

**A:**
An authorization server advertises `require_pushed_authorization_requests=true` but its authorization endpoint accepts direct authorization requests as a fallback. Separately, the same server issues refresh tokens with every authorization code flow response regardless of whether `offline_access` was requested — a representative flow carries `scope=openid email` with no `offline_access`.

**B:**
REPROCESS. Two independent MUST-level violations are present. First: RFC 9126 §4 MUST requires the authorization endpoint to reject non-PAR requests when `require_pushed_authorization_requests=true` is advertised; the fallback path violates this unconditionally, nullifying all PAR security properties. Second: OIDC Core §11 MUST NOT requires that refresh tokens not be issued unless `offline_access` was in the authorization request scope; issuing refresh tokens on every flow violates user consent scope — the user approved an online session, not offline access. Both violations are unconditional normative constraints with no exception clauses. Each independently mandates REPROCESS.

**Reason:** RFC 9126 §4 MUST (PAR enforcement) + OIDC Core §11 MUST NOT (offline_access scope). Two independent MUST-level violations. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9126 §4 MUST: PAR enforcement must be actual, not merely advertised — authorization endpoint that accepts non-PAR requests contradicts the metadata declaration. OIDC Core §11 MUST NOT: refresh token must not be issued without offline_access scope — issuing by default bypasses explicit user consent for offline access. Both MUST violations present independently. REPROCESS.

---

## Pair[CB4-3.4-950-A]

**Territory:** T-rfc-jwt · T-rfc-oauth2
**Domains combined:** JWT-typ-absent (ext36) + DPoP-bound-token-as-Bearer (ext36)
**Expected verdict:** REPROCESS

**A:**
A resource server receives JWT access tokens. The tokens' JOSE headers contain only `alg` and `kid` — `typ` is absent, violating RFC 9068 §2.1. The same RS also fails to validate DPoP proofs: some access tokens carry `cnf/jkt` claims (DPoP-bound tokens) but the RS processes all tokens through a single Bearer validation pipeline that ignores both the `typ` header and the `cnf/jkt` claim.

**B:**
REPROCESS. Two independent MUST-level violations are present at the resource server. First: RFC 9068 §2.1 MUST requires `typ: at+JWT` in JWT access token headers; the absent typ field means the RS cannot discriminate access tokens from ID tokens or other JWT types — token confusion attack vector open. Second: RFC 9449 §7.2 MUST (twice) requires the RS to check for and validate the DPoP proof when cnf/jkt is present in the access token; ignoring cnf/jkt converts sender-constrained tokens to unconstrained Bearer tokens, allowing stolen access tokens to be used without the DPoP private key. The RS's single Bearer pipeline fails both requirements simultaneously: it accepts tokens it should type-check and fails to enforce the sender-constraint the AS embedded via cnf/jkt.

**Reason:** RFC 9068 §2.1 MUST (typ: at+JWT absent) + RFC 9449 §7.2 MUST ×2 (DPoP proof not validated). Two independent MUST violations at the RS. REPROCESS.

**Phrasing-B:**
REPROCESS. The RS's undifferentiated Bearer validation pipeline fails two distinct RFC requirements. RFC 9068 §2.1: typ header absent — token confusion vector (ID token presentable as access token). RFC 9449 §7.2: cnf/jkt ignored — sender-constraint stripped from DPoP-bound tokens, making stolen tokens usable without DPoP key. Each failure independently mandates REPROCESS; both present in the same validation path.

---

## Pair[CB4-3.4-951-A]

**Territory:** T-rfc-oidc · T-asvs-token
**Domains combined:** offline_access-refresh-without-scope (ext36, REPROCESS) + JWT-PII-email (ext35, CONCERN)
**Expected verdict:** REPROCESS (graduation: REPROCESS overrides CONCERN)

**A:**
The authorization server issues refresh tokens with every flow regardless of whether `offline_access` was requested (OIDC Core §11 MUST NOT violation). Separately, JWT access tokens include a `custom_email` claim carrying the user's email address in an unencrypted payload (ASVS V9.3.1 SHOULD guidance — CONCERN level).

**B:**
REPROCESS. Graduation rule applies: the system contains both a REPROCESS-level violation and a CONCERN-level gap; REPROCESS overrides CONCERN. The offline_access violation (OIDC Core §11 MUST NOT) mandates REPROCESS independently — refresh tokens issued without explicit user consent for offline access is an unconditional MUST NOT. The JWT PII email gap (ASVS V9.3.1 SHOULD) is CONCERN-level standing alone. The interaction between them is noteworthy: the offline_access violation produces refresh tokens that, if stolen, grant long-term access; those refresh tokens exchange for PII-bearing access tokens that expose email to all bearers — the MUST NOT violation extends the PII exposure window. Nevertheless, the verdict is REPROCESS on the strength of the OIDC Core §11 violation alone; the CONCERN description is subsumed.

**Reason:** OIDC Core §11 MUST NOT (refresh without offline_access = REPROCESS) + ASVS V9.3.1 SHOULD (JWT PII = CONCERN). Graduation rule: REPROCESS overrides CONCERN. REPROCESS.

**Phrasing-B:**
REPROCESS. Graduation rule: one REPROCESS + any number of CONCERNs = REPROCESS. Offline_access violation — OIDC Core §11 MUST NOT — controls the verdict. JWT email claim is CONCERN-level (V9.3.1 SHOULD) and does not affect the primary verdict. Both gaps should be remediated: gate refresh tokens on offline_access scope; remove email from JWT payload. Verdict: REPROCESS.

---

## Pair[CB4-3.4-952-A]

**Territory:** T-ssdf-process · T-rfc-oauth2
**Domains combined:** PO.1-absent + PAR-enforcement-bypass (ext36) — PO.1 escalation rule
**Expected verdict:** REPROCESS (PO.1 escalation: PO.1 gap + traceable production MUST violation)

**A:**
The development team has no formal SSDF PO.1 security training program — developers learn OAuth and OIDC informally. An authorization server they built advertises `require_pushed_authorization_requests=true` in its discovery metadata but maintains a legacy fallback in the authorization endpoint that accepts direct authorization requests. When asked about the fallback, the developer explains: "I set the metadata flag because clients told me they support PAR. I didn't realise the flag commits the server to rejecting non-PAR requests — I thought it was just an informational field."

**B:**
REPROCESS. PO.1 escalation rule applies: PO.1 absence alone is CONCERN; PO.1 absence combined with a traceable production MUST violation is REPROCESS. The developer's explanation directly traces the RFC 9126 §4 MUST violation to the knowledge gap that a PO.1-compliant training program would have addressed: understanding that `require_pushed_authorization_requests` is a binding server-side enforcement declaration, not a capability advertisement. RFC 9126 §4 MUST is explicit that the server must reject non-PAR requests when the flag is true. A developer with access to RFC 9126 and the security training to read normative requirements would not interpret MUST as informational. The MUST violation itself (PAR enforcement bypass) is independently REPROCESS-eligible. The PO.1 absence establishes the systemic root cause; the §4 violation is the production evidence of that root cause. Combined: REPROCESS per escalation rule.

**Reason:** PO.1 escalation: PO.1 absent (no formal training) + RFC 9126 §4 MUST violation + direct causal link (developer explicitly traces the §4 misunderstanding to not having received training on normative RFC requirements). REPROCESS.

**Phrasing-B:**
REPROCESS. PO.1 escalation: no formal security training (SSDF PO.1 absent — standing CONCERN) + RFC 9126 §4 MUST violated + causal link (developer misread metadata flag as informational due to lack of training on MUST vs informational RFC distinctions). The escalation condition is fully satisfied: the gap that PO.1 training would have closed is the direct root cause of the MUST violation in production. REPROCESS on both grounds.

---

## Pair[CB4-3.4-953-A]

**Territory:** T-rfc-jwt · T-rfc-oauth2
**Domains combined:** JWT-typ-absent (ext36, REPROCESS) + confidential-no-PKCE (ext35, CONCERN)
**Expected verdict:** REPROCESS (graduation: REPROCESS overrides CONCERN)

**A:**
JWT access tokens have no `typ` header field, violating RFC 9068 §2.1 MUST. Separately, a confidential client web application uses the authorization code flow without PKCE — the client authenticates at the token endpoint with `client_secret_basic` only. The developer notes: "I know RFC 9700 §2.1.1 recommends PKCE for confidential clients too, but our client_secret is sufficient for our threat model."

**B:**
REPROCESS. Graduation rule applies: the system contains both a REPROCESS-level violation and a CONCERN-level gap; REPROCESS overrides CONCERN. The JWT typ absence violates RFC 9068 §2.1 MUST unconditionally — `typ: at+JWT` is required in all JWT access token JOSE headers; its absence enables token confusion attacks. This mandates REPROCESS. The confidential client's PKCE absence is RFC 9700 §2.1.1 SHOULD — `client_secret` provides partial mitigation and the gap is CONCERN-level, not REPROCESS. The graduation rule means the REPROCESS verdict on the typ violation controls the overall assessment. Both gaps require remediation: add `typ: at+JWT` to access tokens; add PKCE S256 to the confidential client flow as a layered control.

**Reason:** RFC 9068 §2.1 MUST (typ absent = REPROCESS) + RFC 9700 §2.1.1 SHOULD (no PKCE for confidential client = CONCERN). Graduation rule: REPROCESS overrides CONCERN. REPROCESS.

**Phrasing-B:**
REPROCESS. Graduation: REPROCESS + CONCERN = REPROCESS. JWT typ absent = RFC 9068 §2.1 MUST violation = REPROCESS. Confidential client without PKCE = RFC 9700 §2.1.1 SHOULD gap = CONCERN. The typ REPROCESS controls. The "client_secret is sufficient" argument is noted for the PKCE gap but is irrelevant to the typ MUST violation. Both should be fixed. Verdict: REPROCESS.

---

## Pair[CB4-3.4-954-A]

**Territory:** T-rfc-oauth2
**Domains combined:** PAR-enforcement-bypass (ext36) + token-type-absent (ext35)
**Expected verdict:** REPROCESS

**A:**
An authorization server both (1) advertises `require_pushed_authorization_requests=true` while maintaining a fallback path that accepts direct authorization requests, and (2) returns token endpoint successful responses without the `token_type` field, containing only `access_token` and `expires_in`. Both gaps exist in the same deployment.

**B:**
REPROCESS. Two independent RFC 6749-family MUST violations are present. First: RFC 9126 §4 MUST — authorization endpoint must reject non-PAR requests when require_pushed_authorization_requests=true is advertised; the fallback path violates this unconditionally. Second: RFC 6749 §5.1 MUST — token endpoint successful responses must include `token_type`; the field enables programmatic determination of the Authorization header scheme; "access_token presence implies Bearer" is not a valid substitute for the self-describing response the MUST requires. Both violations independently mandate REPROCESS. The combination affects both the authorization phase (PAR bypass) and the token phase (incomplete response), creating a system where PAR's security properties are void and clients may misbehave due to missing token_type.

**Reason:** RFC 9126 §4 MUST (PAR bypass) + RFC 6749 §5.1 MUST (token_type absent). Two independent MUST violations affecting different phases of the OAuth flow. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9126 §4 MUST: PAR enforcement must match metadata advertisement — fallback path is a MUST violation. RFC 6749 §5.1 MUST: token_type must be in successful token responses — self-describing response requirement, not implied by access_token presence. Two independent MUSTs, both unconditional. REPROCESS.

---

## Pair[CB4-3.4-955-A]

**Territory:** T-rfc-oidc · T-oauth-oidc
**Domains combined:** offline_access-refresh-without-scope (ext36, REPROCESS) + acr-not-validated (ext34, CONCERN)
**Expected verdict:** REPROCESS (graduation: REPROCESS overrides CONCERN)

**A:**
The AS issues refresh tokens unconditionally (OIDC Core §11 MUST NOT violated). The client application also does not validate the `acr` claim returned in the ID token despite requesting `acr_values=2` — it accepts `acr=1` silently (OIDC Core §3.1.3.7.9 SHOULD — CONCERN level).

**B:**
REPROCESS. Graduation rule applies. The offline_access violation (OIDC Core §11 MUST NOT) mandates REPROCESS: refresh tokens must not be issued without explicit offline_access scope; user consent for offline access has been bypassed. This MUST NOT is unconditional. The acr validation gap (OIDC Core §3.1.3.7.9 SHOULD) is CONCERN-level: the AS may silently downgrade authentication assurance from level 2 to level 1; the RP should detect this but SHOULD is not MUST. Graduation rule: REPROCESS overrides CONCERN. The verdict is REPROCESS on the offline_access violation alone. The combined security posture is notable: an attacker who defeats level-1 authentication (if level-2 MFA is silently bypassed via acr downgrade) receives not just an access token but also a refresh token that grants ongoing access — the combination of both gaps is more harmful than either alone, but the verdict remains REPROCESS regardless of the CONCERN addition.

**Reason:** OIDC Core §11 MUST NOT (offline_access bypass = REPROCESS) + OIDC Core §3.1.3.7.9 SHOULD (acr not validated = CONCERN). Graduation rule: REPROCESS controls. REPROCESS.

**Phrasing-B:**
REPROCESS. Graduation applies: OIDC Core §11 MUST NOT violation (refresh without offline_access) = REPROCESS; OIDC Core §3.1.3.7.9 SHOULD gap (acr not validated) = CONCERN. REPROCESS overrides CONCERN. Both should be fixed: gate refresh tokens on offline_access; validate acr ≥ requested level at ID token receipt. Verdict: REPROCESS.

---

## Pair[CB4-3.4-956-A]

**Territory:** T-rfc-jwt · T-rfc-oauth2
**Domains combined:** DPoP-bound-token-as-Bearer (ext36) + JWT-iss-substring-match (ext33)
**Expected verdict:** REPROCESS

**A:**
A resource server accepts DPoP-bound access tokens as plain Bearer tokens without requiring DPoP proof (RFC 9449 §7.2 MUST violated). Separately, the same RS validates the JWT `iss` claim using a substring/contains check: `if (token.Issuer.Contains("auth.example.com"))` — a check that would pass for an attacker-controlled issuer value of `evil-auth.example.com` (RFC 7519 §4.1.1 requires exact case-sensitive comparison).

**B:**
REPROCESS. Two independent MUST-level violations at the resource server. First: RFC 9449 §7.2 MUST — RS must validate DPoP proof when cnf/jkt is present; accepting DPoP-bound tokens as plain Bearer converts sender-constrained tokens to unconstrained ones, allowing token theft to succeed without key possession. Second: RFC 7519 §4.1.1 — the `iss` claim must be validated as an exact case-sensitive string comparison; a substring/contains check allows `evil-auth.example.com` to pass iss validation; in JWKS-discovery mode, the RS fetches keys from an attacker-controlled discovery URL derived from the forged issuer value, enabling full JWT signature forgery. Both violations independently mandate REPROCESS. The combination is particularly dangerous: the iss substring check enables forged tokens to pass; DPoP-proof non-enforcement means legitimate DPoP-bound tokens can be stolen and replayed — the RS provides neither correct issuer binding nor sender-constraint enforcement.

**Reason:** RFC 9449 §7.2 MUST (DPoP not enforced) + RFC 7519 §4.1.1 (iss substring check instead of exact match). Two independent MUST violations at the RS. REPROCESS.

**Phrasing-B:**
REPROCESS. Two RS-side MUST failures. RFC 9449 §7.2: cnf/jkt present but DPoP proof not validated — sender-constraint stripped, stolen tokens freely usable. RFC 7519 §4.1.1: iss validated by substring match instead of exact case-sensitive comparison — evil-auth.example.com passes; JWKS discovery from attacker-controlled URL enables full token forgery. Each independently = REPROCESS. Combined RS posture: forged tokens accepted (iss bypass) AND stolen tokens accepted (DPoP not enforced).

---

## Pair[CB4-3.4-957-A]

**Territory:** T-rfc-jwt · T-rfc-oauth2 · T-cwe-authz
**Domains combined:** JWT-typ-absent (ext36) + state-not-validated (ext34)
**Expected verdict:** REPROCESS

**A:**
JWT access tokens are issued without the `typ: at+JWT` header (RFC 9068 §2.1 MUST). Separately, the OAuth callback handler receives the `state` parameter from the authorization server and logs it but never compares it against the session-stored value before processing the authorization code (RFC 6749 §10.12 MUST — CSRF protection).

**B:**
REPROCESS. Two independent MUST-level violations are present. First: RFC 9068 §2.1 MUST — JWT access tokens must carry `typ: at+JWT`; absent typ enables token confusion attacks (ID token presented as access token at RS). Second: RFC 6749 §10.12 MUST — CSRF protection for the authorization callback requires state validation; receiving and logging state without comparing it to the session-stored value provides zero CSRF protection; an attacker can CSRF-inject their authorization code into a victim's callback, binding the victim to the attacker's authorization grant. Both MUSTs are unconditional. The attack chain combining both gaps: an attacker CSRF-injects a code (state not validated); the victim exchanges the code for tokens; those tokens lack typ (if the attacker later has RS access, they could also attempt token confusion). The CSRF gap is the more immediately dangerous: it allows attacker-controlled token binding to victims. Both violations mandate REPROCESS independently.

**Reason:** RFC 9068 §2.1 MUST (typ absent) + RFC 6749 §10.12 MUST (CSRF/state not validated). Two independent MUST violations. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9068 §2.1 MUST: typ: at+JWT absent — token confusion attack vector. RFC 6749 §10.12 MUST: state received and logged but not validated — CSRF protection absent; attacker can inject authorization code into victim callback; "logging = compliance" is rejected, validation is the protective mechanism. Both are unconditional MUSTs. REPROCESS.

---

## Pair[CB4-3.4-958-A]

**Territory:** T-rfc-oauth2 · T-rfc-oidc · T-rfc-jwt
**Domains combined:** comprehensive PASS — PAR enforced + offline_access gated + typ=at+JWT + DPoP fully validated
**Expected verdict:** PASS

**A:**
An authorization server advertises `require_pushed_authorization_requests=true` and enforces it: the authorization endpoint returns 400 error=invalid_request for any request lacking a valid `request_uri`. Refresh tokens are only issued when `offline_access` is explicitly included in the authorization request scope; a consent screen explicitly names the offline access capability. JWT access tokens are issued with `{"typ":"at+JWT","alg":"RS256","kid":"key-2024-01"}` in the JOSE header. When DPoP-bound tokens are issued (cnf/jkt present), the resource server validates the full DPoP proof chain per RFC 9449 §4.3: public key JWK thumbprint match against cnf/jkt; htm and htu match current request; ath = BASE64URL(SHA256(access_token)); iat within ±60s; jti not previously seen.

**B:**
PASS. No arbiter violations found. RFC 9126 §4 MUST satisfied: PAR enforcement is actual, not merely advertised — authorization endpoint rejects all non-PAR requests unconditionally. OIDC Core §11 MUST NOT satisfied: refresh token issuance is gated on offline_access scope + user consent; no refresh tokens issued for flows without offline_access. RFC 9068 §2.1 MUST satisfied: `typ: at+JWT` present in all JWT access token JOSE headers — token confusion attack vector eliminated. RFC 9449 §7.2 MUST satisfied: RS validates full DPoP proof chain when cnf/jkt present (key match, method/URI binding, access token hash, freshness, replay prevention) — sender-constraint property fully enforced. All four ext36 patterns simultaneously satisfied in a single integrated deployment.

**Reason:** RFC 9126 §4, OIDC Core §11, RFC 9068 §2.1, RFC 9449 §7.2 — all satisfied. No MUST violations, no SHOULD gaps identified. Correct implementation of PAR enforcement, offline_access consent, JWT typ, and DPoP sender-constraint. PASS.

**Phrasing-B:**
PASS. Four simultaneous PASS conditions: (1) PAR enforcement consistent with metadata — authorization endpoint rejects non-PAR per RFC 9126 §4; (2) refresh token gated on offline_access scope + user consent per OIDC Core §11 MUST NOT; (3) JWT access tokens carry `typ: at+JWT` per RFC 9068 §2.1 MUST; (4) RS validates complete DPoP chain per RFC 9449 §7.2/§4.3 when cnf/jkt present. No violations at any layer. PASS.
