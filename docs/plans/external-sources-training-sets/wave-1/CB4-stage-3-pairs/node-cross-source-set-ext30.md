# CB4 Stage 3 — Cross-Source Pair Set ext30
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-975 through CB4-3.4-984 (10 base pairs × 2 phrasings = 20 instances)
# Source domains: ext37 patterns (client_credentials-refresh_token, OIDC-max_age, nbf-RS, wildcard-aud)
#   combined with each other and with earlier patterns
# Running total after this file: 120 + (984 − 60) × 2 = 1968 instances (984 base pairs)
# Graduation rule: REPROCESS overrides CONCERN; N CONCERNs alone ≠ REPROCESS
# PO.1 escalation: PO.1 absence + traceable production MUST violation = REPROCESS

---

## Pair[CB4-3.4-975-A]

**Territory:** T-rfc-oauth2 · T-rfc-oidc
**Domains combined:** client_credentials-refresh_token (ext37) + OIDC-max_age-ignored (ext37)
**Expected verdict:** REPROCESS

**A:**
An OAuth/OIDC deployment has two gaps. First: the token endpoint includes `refresh_token` in client credentials grant responses, violating RFC 6749 §4.4.3 MUST NOT. Second: the authorization server ignores the `max_age` parameter in OIDC authorization requests — a client requesting `max_age=300` receives an authorization code from a 4-hour-old session without re-authentication, violating OIDC Core §3.1.2.2 MUST.

**B:**
REPROCESS. Two independent MUST-level violations are present. First: RFC 6749 §4.4.3 MUST NOT — refresh tokens must not be issued with client credentials grant responses; the prohibition is unconditional, reflecting the design decision that client re-authentication is the correct renewal mechanism. Second: OIDC Core §3.1.2.2 MUST — the AS must re-authenticate the user when the elapsed time since last authentication exceeds max_age; treating max_age as advisory violates this unconditionally and removes the RP's ability to enforce authentication freshness. Both violations are independent MUST-level failures. Each independently mandates REPROCESS.

**Reason:** RFC 6749 §4.4.3 MUST NOT (client_credentials+refresh_token) + OIDC Core §3.1.2.2 MUST (max_age ignored). Two independent MUST violations. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 6749 §4.4.3 MUST NOT: refresh_token in client_credentials response is unconditionally prohibited. OIDC Core §3.1.2.2 MUST: max_age is a binding RP security requirement, not advisory; stale session must not be used when elapsed > max_age. Two independent MUSTs. REPROCESS.

---

## Pair[CB4-3.4-976-A]

**Territory:** T-rfc-jwt · T-rfc-oauth2
**Domains combined:** RS-skips-nbf (ext37, REPROCESS) + wildcard-aud (ext37, CONCERN)
**Expected verdict:** REPROCESS (graduation: REPROCESS overrides CONCERN)

**A:**
A resource server skips `nbf` claim validation — tokens are accepted before their not-before time (RFC 9068 §4 MUST violated). The same RS also accepts JWT access tokens with `aud="*.example.com"` as a wildcard pattern, accepting any RS on the domain (RFC 8725 §3.9 SHOULD gap — CONCERN level).

**B:**
REPROCESS. Graduation rule applies: REPROCESS overrides CONCERN. The nbf validation absence violates RFC 9068 §4 MUST unconditionally — JWT access tokens must be rejected when nbf > current time; skipping this check allows pre-issued tokens to be used before their intended validity window opens; this is an unconditional MUST = REPROCESS. The wildcard aud is a RFC 8725 §3.9 SHOULD gap — per-RS audience isolation is defeated but no explicit MUST prohibits wildcards in normative text = CONCERN. Graduation rule: the REPROCESS verdict on the nbf violation controls. Both gaps should be remediated: add nbf validation with 5-second clock skew tolerance; adopt per-RS aud values via RFC 8707 resource indicators.

**Reason:** RFC 9068 §4 MUST (nbf skipped = REPROCESS) + RFC 8725 §3.9 SHOULD (wildcard aud = CONCERN). Graduation rule: REPROCESS overrides CONCERN. REPROCESS.

**Phrasing-B:**
REPROCESS. Graduation: REPROCESS + CONCERN = REPROCESS. RFC 9068 §4 MUST nbf check absent = REPROCESS. RFC 8725 §3.9 SHOULD wildcard aud = CONCERN. REPROCESS controls. Fix both: add nbf check (5s skew tolerance); move to per-RS specific aud values.

---

## Pair[CB4-3.4-977-A]

**Territory:** T-rfc-oidc · T-asvs-token
**Domains combined:** OIDC-max_age-ignored (ext37, REPROCESS) + JWT-PII-email (ext35, CONCERN)
**Expected verdict:** REPROCESS (graduation: REPROCESS overrides CONCERN)

**A:**
The authorization server ignores the OIDC `max_age` parameter, allowing stale sessions to satisfy fresh-auth requirements (OIDC Core §3.1.2.2 MUST). JWT access tokens contain a `custom_email` claim in an unencrypted payload (ASVS V9.3.1 SHOULD guidance — CONCERN level).

**B:**
REPROCESS. Graduation rule applies. The max_age violation (OIDC Core §3.1.2.2 MUST) mandates REPROCESS: the AS must re-authenticate when elapsed time > max_age; treating the parameter as advisory fails this unconditional MUST. The JWT PII email gap (ASVS V9.3.1 SHOULD) is CONCERN-level alone. Graduation rule: REPROCESS overrides CONCERN. The compound effect is noteworthy: an attacker who accesses a user's stale browser session (max_age bypass) completes a sensitive operation and receives a PII-bearing access token (email visible to all bearers and in logs). The MUST violation on max_age controls the verdict regardless.

**Reason:** OIDC Core §3.1.2.2 MUST (max_age = REPROCESS) + ASVS V9.3.1 SHOULD (JWT PII = CONCERN). Graduation: REPROCESS controls. REPROCESS.

**Phrasing-B:**
REPROCESS. OIDC Core §3.1.2.2 MUST violation on max_age is REPROCESS-grade. ASVS V9.3.1 SHOULD gap on JWT email is CONCERN-grade. Graduation: REPROCESS overrides. Both should be fixed: enforce max_age; remove email from JWT payload. REPROCESS.

---

## Pair[CB4-3.4-978-A]

**Territory:** T-ssdf-process · T-rfc-oidc
**Domains combined:** PO.1-absent + OIDC-max_age-ignored (ext37) — PO.1 escalation rule
**Expected verdict:** REPROCESS (PO.1 escalation: PO.1 gap + traceable production MUST violation)

**A:**
The development team has no formal SSDF PO.1 security training program — they learn OIDC from tutorial blog posts and informal channels. A security review reveals that the authorization server ignores the OIDC `max_age` parameter. The lead developer, when asked, responds: "I've seen `max_age` in OIDC docs but I thought it was a preference the client was expressing to the UI — like a hint to show a login page if you think authentication is old. I didn't know the server was obligated to enforce it."

**B:**
REPROCESS. PO.1 escalation rule applies: PO.1 absence alone is CONCERN; PO.1 absence combined with a traceable production MUST violation is REPROCESS. The developer's statement directly traces the OIDC Core §3.1.2.2 MUST violation to a knowledge gap: misunderstanding MUST as a UI hint rather than a binding server obligation. A PO.1-compliant training program covering OIDC normative requirements would address the distinction between MUST and advisory text in protocol specifications. The MUST violation itself (max_age ignored) is independently REPROCESS-eligible. PO.1 absence is the systemic root cause; the §3.1.2.2 violation is the production evidence with a direct causal link. Together: REPROCESS per the escalation rule.

**Reason:** PO.1 escalation: no formal security training + OIDC Core §3.1.2.2 MUST violated + direct causal link (developer explicitly traces the max_age misunderstanding to not having received training on OIDC normative obligations). REPROCESS.

**Phrasing-B:**
REPROCESS. PO.1 escalation triggered: no formal security training (PO.1 absent — CONCERN baseline) + OIDC Core §3.1.2.2 MUST violated (max_age not enforced) + developer's explanation provides explicit causal link (misread MUST as UI hint due to informal training source). PO.1 + traceable production MUST violation = REPROCESS per escalation rule.

---

## Pair[CB4-3.4-979-A]

**Territory:** T-rfc-oauth2 · T-rfc-oidc
**Domains combined:** client_credentials-refresh_token (ext37) + offline_access-no-scope (ext36)
**Expected verdict:** REPROCESS

**A:**
A token endpoint issues refresh tokens in two scenarios: (1) client credentials grant responses (RFC 6749 §4.4.3 MUST NOT), and (2) authorization code flow responses regardless of offline_access scope (OIDC Core §11 MUST NOT). Both MUST NOT clauses are violated simultaneously.

**B:**
REPROCESS. Two independent MUST NOT violations are present. First: RFC 6749 §4.4.3 MUST NOT — refresh tokens must not be included in client credentials grant responses. Second: OIDC Core §11 MUST NOT — refresh tokens must not be issued unless offline_access was explicitly in the authorization request scope. Both prohibitions are unconditional and reflect distinct design principles: the §4.4.3 MUST NOT addresses the architectural inappropriateness of refresh tokens in client-to-AS two-party flows; the §11 MUST NOT addresses user consent for offline access. The system violates both simultaneously, issuing refresh tokens in contexts where they are doubly inappropriate. Each independently mandates REPROCESS.

**Reason:** RFC 6749 §4.4.3 MUST NOT + OIDC Core §11 MUST NOT. Two independent refresh-token prohibition violations. REPROCESS.

**Phrasing-B:**
REPROCESS. Two MUST NOT violations on refresh token issuance: RFC 6749 §4.4.3 MUST NOT (client credentials grant) + OIDC Core §11 MUST NOT (no offline_access scope). The system issues refresh tokens in both contexts where they are explicitly prohibited. REPROCESS.

---

## Pair[CB4-3.4-980-A]

**Territory:** T-rfc-jwt · T-rfc-oidc
**Domains combined:** wildcard-aud (ext37, CONCERN) + acr-not-validated (ext34, CONCERN)
**Expected verdict:** CONCERN (graduation: two CONCERNs do not escalate to REPROCESS)

**A:**
JWT access tokens use wildcard `aud="*.example.com"` (RFC 8725 §3.9 SHOULD — CONCERN). Additionally, the client does not validate the `acr` claim in returned ID tokens despite requesting `acr_values=2` (OIDC Core §3.1.3.7.9 SHOULD — CONCERN).

**B:**
CONCERN. Two SHOULD-level gaps are present. First: RFC 8725 §3.9 — wildcard aud defeats per-RS audience isolation; specific RS identifiers should be used; no explicit MUST prohibition found. CONCERN. Second: OIDC Core §3.1.3.7.9 — acr claim should be validated against the requested level; silent acr downgrade (level 2 → level 1) not detected. CONCERN. Graduation rule: two CONCERNs do not escalate to REPROCESS. The combined security posture is notable: audience-unrestricted tokens (wildcard aud) issued after potentially downgraded authentication (acr not validated) — any RS accepts any token regardless of the client's authorized RS scope, and MFA may have been silently bypassed. However, no MUST violation elevates this to REPROCESS. Verdict: CONCERN with dual gap description.

**Reason:** RFC 8725 §3.9 SHOULD (wildcard aud) + OIDC Core §3.1.3.7.9 SHOULD (acr not validated). Both SHOULD-level. Graduation rule: N CONCERNs = CONCERN. CONCERN.

**Phrasing-B:**
CONCERN. RFC 8725 §3.9 SHOULD (wildcard aud = CONCERN) + OIDC Core §3.1.3.7.9 SHOULD (acr not validated = CONCERN). Graduation rule: two CONCERNs = CONCERN; they do not combine to REPROCESS. Both should be remediated: adopt per-RS aud; validate acr ≥ requested level. Verdict: CONCERN.

---

## Pair[CB4-3.4-981-A]

**Territory:** T-rfc-jwt
**Domains combined:** RS-skips-nbf (ext37) + JWT-typ-absent (ext36)
**Expected verdict:** REPROCESS

**A:**
A resource server validates JWT access tokens for signature, `iss`, `aud`, `exp`, and `scope` but skips both `nbf` and `typ` checks. Tokens with nbf in the future are accepted immediately (RFC 9068 §4 MUST violated). Tokens lack `typ: at+JWT` in their JOSE headers — the RS would accept ID tokens presented as access tokens (RFC 9068 §2.1 MUST violated).

**B:**
REPROCESS. Two independent MUST-level violations of RFC 9068 at the resource server. First: RFC 9068 §4 MUST — RS must reject JWT access tokens when nbf > current time; skipping nbf allows pre-issued tokens to be used before their intended validity window. Second: RFC 9068 §2.1 MUST — JWT access tokens must carry `typ: at+JWT` in the JOSE header; the absent typ enables token confusion attacks (ID token from same issuer accepted as access token). Both violations are from RFC 9068 — the JWT access token profile specification — and both represent mandatory RS-side validation that the implementation omits. The combination is particularly exploitable: an attacker can present an ID token (no typ check) with a future nbf (no nbf check) and both bypass mechanisms fail simultaneously.

**Reason:** RFC 9068 §4 MUST (nbf skipped) + RFC 9068 §2.1 MUST (typ absent). Two RFC 9068 MUST violations at RS. REPROCESS.

**Phrasing-B:**
REPROCESS. Two RFC 9068 MUST violations at the RS: §4 MUST (nbf not checked — tokens accepted before validity window) + §2.1 MUST (typ absent — token type confusion undetected). Both missing validation steps from the same JWT access token profile. REPROCESS.

---

## Pair[CB4-3.4-982-A]

**Territory:** T-rfc-oauth2 · T-cwe-authz
**Domains combined:** client_credentials-refresh_token (ext37) + state-not-validated (ext34)
**Expected verdict:** REPROCESS

**A:**
A deployment has two OAuth implementation gaps. First: client credentials grant responses include refresh_token (RFC 6749 §4.4.3 MUST NOT). Second: the OAuth callback handler logs but does not validate the `state` parameter — CSRF protection is absent from the authorization code flow (RFC 6749 §10.12 MUST).

**B:**
REPROCESS. Two independent MUST-level violations are present. First: RFC 6749 §4.4.3 MUST NOT — refresh tokens must not be issued with client credentials grant responses; this is unconditional. Second: RFC 6749 §10.12 MUST — CSRF protection requires state validation at the callback, not just logging; an attacker can CSRF-inject their authorization code into a victim's callback session; without state comparison, the victim is bound to the attacker's authorization grant. Both violations are from RFC 6749 — the core OAuth 2.0 specification — and both represent mandatory implementation requirements that the deployment omits. Each independently mandates REPROCESS.

**Reason:** RFC 6749 §4.4.3 MUST NOT (client_credentials+refresh_token) + RFC 6749 §10.12 MUST (state not validated). Two RFC 6749 MUST violations. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 6749 §4.4.3 MUST NOT: client_credentials refresh token prohibition violated. RFC 6749 §10.12 MUST: state validation absent — CSRF protection defeated; "logging = compliance" is rejected. Two RFC 6749 violations from opposite ends of the flow (token issuance + callback handling). REPROCESS.

---

## Pair[CB4-3.4-983-A]

**Territory:** T-rfc-jwt · T-rfc-oauth2
**Domains combined:** wildcard-aud (ext37, CONCERN) + confidential-no-PKCE (ext35, CONCERN)
**Expected verdict:** CONCERN (graduation: two CONCERNs do not escalate to REPROCESS)

**A:**
JWT access tokens use wildcard `aud="*.example.com"` (RFC 8725 §3.9 SHOULD gap — CONCERN). A confidential client uses authorization code flow without PKCE, relying on `client_secret_basic` only (RFC 9700 §2.1.1 SHOULD gap — CONCERN).

**B:**
CONCERN. Two SHOULD-level gaps are present. First: RFC 8725 §3.9 — wildcard aud should be replaced with specific RS identifiers for per-RS audience isolation; SHOULD gap = CONCERN. Second: RFC 9700 §2.1.1 — PKCE is recommended for confidential clients despite client_secret providing partial mitigation; PKCE would address authorization code injection attacks not mitigated by client_secret; SHOULD gap = CONCERN. Graduation rule: two CONCERNs do not escalate to REPROCESS. No MUST violations are present in this combination. The combined security posture note: audience-unrestricted tokens (any RS accepts them) combined with a code flow missing injection protection (PKCE absent) — but neither gap reaches MUST-violation territory. Verdict: CONCERN.

**Reason:** RFC 8725 §3.9 SHOULD (wildcard aud) + RFC 9700 §2.1.1 SHOULD (confidential client no PKCE). Both SHOULD-level. Graduation rule: N CONCERNs = CONCERN. CONCERN.

**Phrasing-B:**
CONCERN. Two SHOULD-level deficiencies: RFC 8725 §3.9 SHOULD (wildcard aud = CONCERN) + RFC 9700 §2.1.1 SHOULD (confidential client without PKCE = CONCERN). Graduation rule: two CONCERNs = CONCERN. No MUST violations. Both should be remediated: per-RS aud + PKCE S256 added to confidential client flow. Verdict: CONCERN.

---

## Pair[CB4-3.4-984-A]

**Territory:** T-rfc-oauth2 · T-rfc-oidc · T-rfc-jwt
**Domains combined:** comprehensive PASS — client_credentials correct + max_age enforced + nbf validated + per-RS aud
**Expected verdict:** PASS

**A:**
A deployment satisfies all four ext37 PASS conditions simultaneously. (1) Client credentials grant responses contain `access_token`, `token_type=Bearer`, `expires_in=3600`, `scope` — no `refresh_token`. Services re-authenticate before token expiry. (2) The OIDC authorization server evaluates `max_age` on every request: when elapsed time since `auth_time` exceeds `max_age`, re-authentication is triggered, `auth_time` is updated, and the ID token reflects the re-authentication time. (3) The resource server validates `nbf` as part of its pipeline: tokens with `nbf > current_time + 5s` are rejected with 401. (4) JWT access tokens carry specific RS URI values in `aud` (e.g., `"aud": "https://api.example.com"`), selected via RFC 8707 resource indicators; each RS validates its own URI in the aud claim and rejects cross-RS tokens.

**B:**
PASS. No arbiter violations found. RFC 6749 §4.4.3 MUST NOT satisfied: no refresh_token in client credentials grant response. OIDC Core §3.1.2.2 MUST satisfied: max_age evaluated, re-authentication triggered when elapsed > max_age, auth_time updated accurately. RFC 9068 §4 MUST satisfied: nbf validated at RS with 5-second clock skew tolerance. RFC 8725 §3.9 SHOULD satisfied: per-RS specific aud values via RFC 8707 resource indicators; audience isolation fully operational. All four ext37 patterns simultaneously satisfied.

**Reason:** RFC 6749 §4.4.3, OIDC Core §3.1.2.2, RFC 9068 §4, RFC 8725 §3.9 — all satisfied. No MUST NOT violations, no MUST violations, no SHOULD gaps. Correct integrated deployment. PASS.

**Phrasing-B:**
PASS. Four simultaneous PASS conditions: (1) no refresh_token in client_credentials per RFC 6749 §4.4.3 MUST NOT; (2) max_age enforced per OIDC Core §3.1.2.2 MUST; (3) nbf validated at RS with 5-second skew tolerance per RFC 9068 §4 MUST; (4) per-RS aud via RFC 8707 resource indicators satisfying RFC 8725 §3.9 SHOULD. No violations at any layer. PASS.
