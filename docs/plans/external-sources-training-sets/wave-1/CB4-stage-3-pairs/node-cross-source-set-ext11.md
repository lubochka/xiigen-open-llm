# CB4 Stage 3 — Node Cross-Source Set Extension 11
# Sequences CB4-3.4-481 through CB4-3.4-490
# Stratum: B/C/D — multi-source combinations for ext18 patterns
# Instances: 20 (10 base pairs × 2 phrasings)
# Running total after this file: 980 instances

---

## Pair[CB4-3.4-481]

**A:** Open dynamic registration plus PKCE downgrade. Authorization server: dynamic client registration unauthenticated. Registered clients include a public client with `redirect_uri=https://app.example.com/callback`. PKCE is enforced — S256 required, plain rejected. Auth code flow: PKCE S256 + code-only response type. Team: "The PKCE enforcement makes up for open registration — codes are useless without the verifier."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

T-oauth-oidc:
  PKCE S256 enforced ✓ (V10.4.6 — no plain) — PASS
  response_type=code only ✓ — PASS
  Open dynamic registration: any party registers clients with arbitrary redirect_uris ✗
    V10.2.1 exact-URI registration model undermined ✗
    Attacker registers redirect_uri=https://attacker.com/callback ✗
    → REPROCESS (registration authentication absent)

Team argument: "PKCE makes up for open registration"
  PKCE protects against code interception (authorization code cannot be exchanged without verifier)
  PKCE does NOT protect against redirect_uri hijacking:
    Attacker registers malicious redirect_uri ✓ (allowed by open registration)
    User authorizes — code sent to attacker's registered URI ✓
    Attacker possesses the code; attacker created the PKCE challenge → attacker has the verifier too
    PKCE does not prevent attacker who controls both the client and the code from exchanging the code
  → "PKCE compensates for open registration" REJECTED — independent security boundaries

T-oauth-oidc verdict: REPROCESS (open registration)
Overall verdict: REPROCESS
```

**Reason:** PKCE correctness (PASS) does not compensate for open dynamic registration (REPROCESS). An attacker who registers their own client controls both the redirect_uri and the PKCE challenge — PKCE only protects against interception by a third party, not against the attacker who owns the client. Two independent security requirements; team argument rejected.

**Phrasing-B:** PKCE S256 enforced, code-only flow, but dynamic registration unauthenticated. Team: "PKCE makes codes useless without the verifier — open registration is safe." Is this argument valid?

---

## Pair[CB4-3.4-482]

**A:** Redirect_uri comparison plus Argon2id. Token endpoint: redirect_uri compared with full-URI case-insensitive comparison (`stored.lower() == submitted.lower()`). Password hashing: Argon2id with recommended parameters. No other security gaps identified. Team: "Our password security is strong and redirect_uri verification exists — the overall security posture is good."

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V6-Auth

T-oauth-oidc:
  RFC 6749 §4.1.3 redirect_uri comparison: full-URI case-insensitive ✗
    Path is case-sensitive per RFC 3986; full-URI lower() is overly permissive
    → CONCERN (path case variant passes comparison; narrow exploitation window)

T-asvs-authentication:
  Argon2id with recommended parameters ✓ — PASS

Multi-source analysis:
  Two independent territories: authentication (PASS) and OAuth URI comparison (CONCERN)
  PASS in authentication does not compensate for CONCERN in OAuth comparison
  Team argument: "overall security posture is good" does not affect per-territory verdicts
  Graduation: 0 REPROCESS, 1 CONCERN → CONCERN

T-oauth-oidc verdict: CONCERN
T-asvs-authentication verdict: PASS
Overall verdict: CONCERN
```

**Reason:** Argon2id is correct (PASS for authentication). redirect_uri full-URI case-insensitive comparison is CONCERN (path is case-sensitive per RFC 3986). Strong authentication does not compensate for a separate OAuth comparison gap. Overall CONCERN from the URI comparison finding.

**Phrasing-B:** redirect_uri full-URI case-insensitive comparison. Argon2id correct. Team says overall security posture is good. Does a PASS in authentication compensate for the URI comparison CONCERN?

---

## Pair[CB4-3.4-483]

**A:** Introspection open plus Vault. API gateway: introspection endpoint accessible without authentication — any caller with network access to the gateway can introspect any token. Secret management: all secrets stored in HashiCorp Vault with audit logging. Team: "Our secrets are secure in Vault — the real attack surface is credential compromise, which we've addressed."

**B:**
```
Arbiter: ASVS-V10-OAuth + SSDF-PW

T-rfc-oauth2:
  RFC 7662 §2.1: introspection endpoint must require authentication ✗
    Any caller can query active status, scopes, subject of any token ✗
    Token metadata disclosure to unauthenticated callers ✗
    → REPROCESS (required authentication absent on introspection endpoint)

T-ssdf-process:
  Vault for secret management ✓ — PASS

Multi-source analysis:
  Vault PASS does not compensate for introspection authentication REPROCESS
  Secrets being secure does not prevent token metadata disclosure via unauthenticated introspection
  Team argument: "real attack surface is credential compromise" → REJECTED
    Introspection disclosure is its own attack surface independent of credential storage
  Graduation: REPROCESS present → Overall REPROCESS

T-rfc-oauth2 verdict: REPROCESS
T-ssdf-process verdict: PASS
Overall verdict: REPROCESS
```

**Reason:** Vault is correct (PASS). Open introspection (REPROCESS) is an independent finding — unauthenticated callers can enumerate token metadata. Secure secret storage does not protect against token metadata disclosure via an unprotected introspection endpoint. REPROCESS overrides PASS.

**Phrasing-B:** Introspection endpoint open (no auth required). Vault used for all secrets. Team: "Credential compromise is the real risk and we've handled it." Does Vault security resolve the introspection finding?

---

## Pair[CB4-3.4-484]

**A:** Protected dynamic registration plus introspection cache CONCERN. Authorization server: dynamic registration protected by initial access token. Introspection cache: 8-minute TTL, no active cache invalidation on revocation. Revocation endpoint: client authentication required. All other controls correct.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

T-oauth-oidc:
  Dynamic registration: initial access token required ✓ — PASS
  redirect_uris stored exactly ✓ — PASS

T-rfc-oauth2:
  RFC 7009 §2.1: revocation with client auth ✓ — PASS (mechanism)
  Introspection cache: 8-minute TTL, no active invalidation
    8-minute post-revocation window: bounded; revocation mechanism exists
    Same analysis as 5-minute and 10-minute cases — CONCERN category
    8 minutes: between 5 and 10 min; CONCERN (bounded window)
  RFC 7662: introspection correctly protected — focus: cache invalidation gap

Multi-source analysis:
  Dynamic registration PASS + revocation mechanism PASS + cache CONCERN
  Graduation: 0 REPROCESS, 1 CONCERN → CONCERN

T-oauth-oidc verdict: PASS
T-rfc-oauth2 verdict: CONCERN
Overall verdict: CONCERN
```

**Reason:** Protected dynamic registration is correct (PASS). 8-minute introspection cache without active invalidation creates a bounded post-revocation window — CONCERN (same category as 5-minute and 10-minute cases). Overall CONCERN from the cache finding; dynamic registration correctness does not change this.

**Phrasing-B:** Dynamic registration correctly protected by initial access token. Introspection cache 8-minute TTL with no active invalidation. Revocation with client auth. What is the overall verdict?

---

## Pair[CB4-3.4-485]

**A:** scope=admin internal service plus Security Champion. Internal M2M service uses scope=admin for all API calls. Scope enforcement is correct at resource server. A designated Security Champion reviews security changes. No security checklist used — review is ad-hoc based on champion's judgment. Team: "The Security Champion is reviewing everything — we have oversight."

**B:**
```
Arbiter: ASVS-V8-AuthZ + SSDF-PW

T-asvs-authorization:
  V8.1.2: scope enforcement at resource server ✓ — PASS (enforcement mechanism)
  V8.1.2: M2M service scope=admin for all operations ✗
    Violates least-privilege: token carries more privilege than any individual operation requires
    → CONCERN (design gap; enforcement exists)

T-ssdf-process:
  Security Champion designated ✓ — PASS (role exists)
  Ad-hoc review without checklist ✗
    SSDF PW.1.2: structured review process expected
    Champion without checklist → oversight quality unpredictable
    → CONCERN (champion exists; structured process absent)

Multi-source analysis:
  Scope design CONCERN + Security Champion process CONCERN
  Two CONCERNs do NOT escalate to REPROCESS — graduation rule
  Graduation: 0 REPROCESS, 2 CONCERN → CONCERN

T-asvs-authorization verdict: CONCERN
T-ssdf-process verdict: CONCERN
Overall verdict: CONCERN
```

**Reason:** Two independent CONCERNs: scope=admin for M2M (least-privilege gap) and Security Champion without structured checklist (process quality gap). Two CONCERNs do not escalate to REPROCESS per the graduation rule — 0 REPROCESS, 1+ CONCERN = CONCERN.

**Phrasing-B:** M2M service uses scope=admin. Security Champion does ad-hoc reviews without a checklist. Two separate CONCERNs — does that escalate to REPROCESS?

---

## Pair[CB4-3.4-486]

**A:** redirect_uri not verified at exchange plus open dynamic registration. Token endpoint: no redirect_uri parameter checked at code exchange (RFC 6749 §4.1.3 requirement absent). Dynamic client registration: unauthenticated. Team: "Registration happens before auth — checking redirect_uri again at exchange is redundant."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

T-oauth-oidc:
  RFC 6749 §4.1.3: redirect_uri MUST be included in token request and verified ✗
    No redirect_uri verification at code exchange ✗
    → REPROCESS (required verification step absent)

  Open dynamic registration: no initial access token ✗
    Arbitrary redirect_uris registerable ✗
    V10.2.1 undermined ✗
    → REPROCESS (registration control absent)

Team argument: "checking redirect_uri at exchange is redundant"
  RFC 6749 §4.1.3: verification at exchange is a separate, independent MUST
  Registration creates the allowed set; verification at exchange confirms the
    specific code was issued for the specific URI being presented
  Not redundant — defense in depth; different attack surfaces protected
  → REJECTED

T-oauth-oidc verdict: REPROCESS (two REPROCESS findings; graduation: REPROCESS)
T-rfc-oauth2 verdict: REPROCESS
Overall verdict: REPROCESS
```

**Reason:** Two independent REPROCESS findings: (1) no redirect_uri verification at code exchange (RFC 6749 §4.1.3 MUST), (2) unauthenticated dynamic registration (V10.2.1). Team argument that exchange verification is redundant is rejected — it is an independent MUST in the spec. Two REPROCESSes do not change the graduation (still REPROCESS).

**Phrasing-B:** Token endpoint doesn't verify redirect_uri at code exchange. Registration is also unauthenticated. Team: "Registration check is sufficient; exchange verification is redundant." Verdict?

---

## Pair[CB4-3.4-487]

**A:** All ext18 patterns correct plus CWE-306 authentication bypass review. All ext18 controls correct (protected dynamic registration, RFC 3986-correct URI comparison, TTL+pub/sub introspection cache, minimum-scope M2M tokens). Additionally: authentication bypass audit — all management endpoints require a valid access token with admin scope. No unauthenticated paths to privileged operations found.

**B:**
```
Arbiter: ASVS-V10-OAuth + CWE-Authn

T-oauth-oidc:
  Dynamic registration: initial access token ✓ — PASS
  redirect_uri comparison: per-component RFC 3986 ✓ — PASS
  Introspection cache: TTL + pub/sub ✓ — PASS
  M2M scope: minimum per operation ✓ — PASS

T-cwe-authn:
  CWE-306 (missing authentication): all management endpoints require valid token ✓
  No unauthenticated paths to privileged operations ✓
  Admin scope required for management APIs ✓
  → PASS

Multi-source analysis:
  All territories: PASS
  No findings in any territory

T-oauth-oidc verdict: PASS
T-cwe-authn verdict: PASS
Overall verdict: PASS
```

**Reason:** All ext18 patterns correct (PASS) plus authentication audit finding no CWE-306 gaps (PASS). All tokens required for management endpoints; no unauthenticated privileged paths. Comprehensive multi-source PASS.

**Phrasing-B:** All ext18 controls correct. Management endpoints all require valid access tokens with admin scope — no unauthenticated paths to privileged operations. Does this produce a multi-source PASS?

---

## Pair[CB4-3.4-488]

**A:** Introspection cache no-invalidation plus JWT in production logs. API gateway: 6-minute introspection cache, no active invalidation. Application: access tokens (JWTs) logged at INFO level in production, including Authorization header contents. Token revocation implemented with client auth.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2 + RFC-JWT

T-rfc-oauth2:
  RFC 7009 §2.1: revocation with client auth ✓ — PASS (mechanism)
  Introspection cache 6-min TTL, no active invalidation:
    6-minute post-revocation window — CONCERN (bounded; same category as 5/8/10-min cases)
    → CONCERN

T-rfc-jwt:
  JWT access tokens logged at INFO level in production ✗
    Production logs contain valid access tokens (signature included)
    Any log access → token theft possible
    RFC 7519 / ASVS V9: tokens must not appear in logs ✗
    → REPROCESS (active security violation in production)

Multi-source analysis:
  Revocation mechanism PASS + introspection cache CONCERN + JWT in prod logs REPROCESS
  Graduation: REPROCESS present → Overall REPROCESS

T-rfc-oauth2 verdict: CONCERN (cache) / PASS (revocation mechanism)
T-rfc-jwt verdict: REPROCESS
Overall verdict: REPROCESS
```

**Reason:** JWT production logging is REPROCESS (active token exposure in logs). Introspection cache is CONCERN (bounded post-revocation window). REPROCESS overrides CONCERN per graduation rule. Revocation mechanism being correct does not compensate for either finding.

**Phrasing-B:** Introspection cache 6-minute TTL no invalidation (CONCERN). JWT access tokens logged at INFO in production (REPROCESS). What is the overall verdict when one finding is CONCERN and one is REPROCESS?

---

## Pair[CB4-3.4-489]

**A:** Three-CONCERN scenario — dynamic registration, cache, and scope. Authorization server: (1) dynamic registration requires initial access token but tokens are issued without any vetting (automated approval). (2) Introspection cache: 7-minute TTL, no active invalidation. (3) Internal M2M service: scope=admin for all calls. Resource server enforces scope correctly.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2 + ASVS-V8-AuthZ

T-oauth-oidc:
  Dynamic registration: initial access token required ✓ (mechanism present)
  Token issuance: automated approval without vetting ✗
    Any party can obtain an initial access token automatically
    Undermines purpose of initial access token (controlled onboarding)
    CONCERN: control mechanism exists; vetting process absent
    → CONCERN (not REPROCESS — initial access token mechanism is present, just not enforced
      through manual vetting; open to uncontrolled automated registration)

T-rfc-oauth2:
  Introspection cache 7-min TTL, no active invalidation:
    7-minute post-revocation window — CONCERN (same category; bounded window)
    → CONCERN

T-asvs-authorization:
  V8.1.2: scope enforcement correct ✓ — PASS (mechanism)
  M2M scope=admin: least-privilege violation ✗ → CONCERN

Multi-source analysis:
  Three CONCERNs (registration vetting, cache, scope design) + one PASS (enforcement)
  Three CONCERNs do NOT escalate to REPROCESS — graduation rule (same principle as CB4-3.4-464)
  Graduation: 0 REPROCESS, 3 CONCERN → CONCERN

T-oauth-oidc verdict: CONCERN
T-rfc-oauth2 verdict: CONCERN
T-asvs-authorization verdict: CONCERN
Overall verdict: CONCERN (three CONCERNs)
```

**Reason:** Three independent CONCERNs: (1) dynamic registration token issued without vetting (mechanism present, process gap), (2) 7-minute introspection cache without invalidation, (3) scope=admin for M2M. Three CONCERNs do not escalate to REPROCESS — the graduation rule is based on presence/absence of REPROCESS findings, not on count of CONCERNs.

**Phrasing-B:** Three separate CONCERNs: automated initial access token issuance (no vetting), 7-minute introspection cache, scope=admin for M2M. Does three CONCERNs become REPROCESS?

---

## Pair[CB4-3.4-490]

**A:** Comprehensive post-remediation — all ext18 gaps closed. Previous audit found: (1) open dynamic registration → REPROCESS [remediated: initial access token + manual vetting added]. (2) Full-URI case-insensitive redirect_uri comparison → CONCERN [remediated: scheme/host normalized, path exact]. (3) 12-minute introspection cache no invalidation → CONCERN [remediated: TTL=token-remaining + pub/sub invalidation]. (4) M2M scope=admin → CONCERN [remediated: separate tokens per operation]. All prior controls already correct.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2 + ASVS-V8-AuthZ

T-oauth-oidc:
  Dynamic registration: initial access token + manual vetting ✓ (REPROCESS remediated) — PASS
  redirect_uri comparison: scheme/host normalized to lowercase; path exact ✓ (CONCERN remediated) — PASS
  V10.2.1: exact registration + correct comparison ✓ — PASS

T-rfc-oauth2:
  Introspection cache: TTL=token-remaining + pub/sub invalidation ≤100ms ✓
    (CONCERN remediated; 12-minute window eliminated) — PASS
  V10.3: revocation effective — 100ms propagation ✓ — PASS

T-asvs-authorization:
  M2M scope: separate tokens per operation, minimum scope each ✓
    (CONCERN remediated; least-privilege satisfied) — PASS

All prior controls:
  PKCE S256 no-plain ✓; code-only ✓; CORS restricted ✓; Argon2id ✓; Vault ✓ — PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** All four previously identified gaps (1 REPROCESS + 3 CONCERN) remediated with correct implementations. Post-remediation state: every territory returns PASS. The remediated implementation satisfies all applicable requirements. Comprehensive PASS confirms remediation is complete and correct.

**Phrasing-B:** Previous audit found 1 REPROCESS (open registration) and 3 CONCERNs (URI comparison, cache, scope). All remediated with correct controls. Does the post-remediation state produce a comprehensive PASS?

---

## File Summary

| Pair ID | Description | Stratum | Sources | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-481 | Open dynamic registration + PKCE S256 — "PKCE compensates" argument rejected | B/D | V10-OAuth + RFC 7591 | REPROCESS |
| CB4-3.4-482 | redirect_uri case-insensitive CONCERN + Argon2id PASS — no cross-compensation | B | V10-OAuth + V6-Auth | CONCERN |
| CB4-3.4-483 | Open introspection REPROCESS + Vault PASS — Vault doesn't cover token metadata | B/D | RFC 7662 + SSDF-PW | REPROCESS |
| CB4-3.4-484 | Protected dynamic registration PASS + 8-min cache no-invalidation CONCERN | B | V10-OAuth + RFC-OAuth2 | CONCERN |
| CB4-3.4-485 | scope=admin CONCERN + Security Champion no-checklist CONCERN → two CONCERNs | B | V8-AuthZ + SSDF-PW | CONCERN |
| CB4-3.4-486 | No redirect_uri at exchange REPROCESS + open registration REPROCESS | B/D | V10-OAuth + RFC 6749 §4.1.3 | REPROCESS |
| CB4-3.4-487 | All ext18 correct + no CWE-306 gaps — comprehensive multi-source PASS | A | V10-OAuth + CWE-Authn | PASS |
| CB4-3.4-488 | 6-min cache CONCERN + JWT prod logs REPROCESS → REPROCESS overrides | B | RFC-OAuth2 + RFC-JWT | REPROCESS |
| CB4-3.4-489 | Three CONCERNs (registration vetting, cache, scope) → CONCERN not REPROCESS | B | V10-OAuth + RFC-OAuth2 + V8-AuthZ | CONCERN |
| CB4-3.4-490 | Post-remediation: 1 REPROCESS + 3 CONCERNs all closed → comprehensive PASS | A | V10-OAuth + RFC-OAuth2 + V8-AuthZ | PASS |
