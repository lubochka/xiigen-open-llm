# CB4 Stage 3 — Cross-Source Combination Set ext19
# Sequences CB4-3.4-689 through CB4-3.4-698
# Stratum B/C/D — ext26 multi-source combinations
# Running total after this file: 1396 instances (698 base pairs)
# Format: Pair[ID] / A / B / Reason / Phrasing-B

---

## Pair[CB4-3.4-689]

**A**
```
Security audit of a mobile OAuth 2.0 flow:

Finding 1 — Redirect URI: native iOS/Android app uses myapp://oauth/callback
  (custom URI scheme). PKCE S256 implemented. PAR implemented with single-use
  tracking and client_id binding.
Finding 2 — Authorization code: single-use enforced (DB flag). Replay → 400 error.
  On replay detection, all tokens associated with the code are revoked. 30-second lifetime.
Finding 3 — Grant type: token endpoint validates grant_type per registration.
  Mobile app registered for authorization_code + refresh_token only.
```

**B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 (RFC 8252) / T-rfc-oauth2 (RFC 6749 §4.1.2) / T-rfc-oauth2 (RFC 6749 §3.1.1)
Node 2 — Translation:
  F1: native app redirect URI method + PAR
  F2: authorization code lifecycle
  F3: grant type enforcement
Node 3 — Plan: F1=RFC-8252-scheme; F2=§4.1.2-check; F3=§3.1.1-check
Node 4 — Processing:
  F1: myapp:// + PKCE S256 + PAR
    Custom URI scheme: hijacking risk (RFC 8252 §7.1); §7.2 recommends claimed HTTPS
    PKCE S256 present: mitigates auth code theft → reduces hijacking impact
    PAR single-use + binding: additional protection
    Assessment: scheme is a CONCERN despite mitigations; §7.2 preferred alternative
  F2: single-use + replay 400 + all-tokens revocation + 30-second lifetime → RFC 6749 §4.1.2 PASS
  F3: grant type per registration; auth_code + refresh_token only → §3.1.1 PASS
  Result: CONCERN (F1) + PASS (F2) + PASS (F3) = CONCERN
Node 5 — Application: CONCERN (one finding; two PASS)
Node 6 — Translation: CONCERN — custom URI scheme gap; auth code + grant type correct
Node 7 — Arbiter: T-rfc-oauth2 — native app redirect URI
Node 8 — Arbiter verdict:
  CONCERN — Custom URI scheme (myapp://) carries URI hijacking risk (RFC 8252 §7.1).
  PKCE S256 and PAR provide meaningful mitigations (code interception without verifier
  is not exchangeable), but RFC 8252 §7.2 recommends claimed HTTPS redirect URIs
  for stronger OS-level binding. Authorization code lifecycle (F2) and grant type
  enforcement (F3) are correctly implemented. Recommend migrating to Universal Links /
  App Links for the redirect URI.
Node 9 — Format: CONCERN; F1 custom scheme; F2/F3 PASS; §7.2 migration recommended
```

**Reason**
One CONCERN (custom URI scheme despite PKCE + PAR mitigations) + two PASS (auth code lifecycle, grant type). The PKCE + PAR combination reduces the hijacking impact but does not eliminate the underlying scheme weakness — §7.2 remains the preferred approach. CONCERN verdict by graduation rule.

**Phrasing-B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 × 3
Node 4:
  F1: custom URI scheme → hijacking risk; PKCE+PAR mitigate but don't eliminate → CONCERN
  F2: single-use + revocation + 30s lifetime → §4.1.2 PASS
  F3: grant type per registration → §3.1.1 PASS
  CONCERN + PASS + PASS → CONCERN
Node 8 — Arbiter: CONCERN — F1 custom scheme; F2/F3 correct. Migrate to claimed HTTPS (§7.2).
Node 9 — Format: CONCERN; F1 primary; F2/F3 PASS noted
```

---

## Pair[CB4-3.4-690]

**A**
```
Three REPROCESS findings combined:

Finding 1 — Authorization code: not marked as used after token exchange. Lifetime: 10 minutes.
Finding 2 — Grant type: token endpoint does not validate grant_type per registration.
  Service accounts registered for client_credentials can use authorization_code flow.
Finding 3 — JWKS: https://auth.example.com/.well-known/jwks.json (HTTPS, correct config).
  Private key parameters (d, p, q, dp, dq, qi) included in JWKS response.
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 6749 §4.1.2) / T-rfc-oauth2 (RFC 6749 §3.1.1) / T-rfc-jwt (RFC 7517)
Node 2 — Translation: auth code reuse / grant type bypass / private key in JWKS
Node 3 — Plan: F1=§4.1.2-single-use; F2=§3.1.1-grant-type; F3=RFC-7517-private-key
Node 4 — Processing:
  F1: code not invalidated after use → §4.1.2 MUST single-use violated → REPROCESS
      Replay revocation also impossible → second MUST violation
  F2: grant type not enforced → §3.1.1 MUST violated; service accounts access user-delegation flows → REPROCESS
  F3: private RSA key (d, p, q, etc.) in JWKS over HTTPS → RFC 7517 fundamental violation
      Any caller with HTTPS access obtains private signing key → forge arbitrary JWTs → REPROCESS
      Note: HTTPS is correct for the endpoint; private key content is the violation
Node 5 — Application: REPROCESS × 3
Node 6 — Translation: three independent REPROCESS findings; overall REPROCESS
Node 7 — Arbiter: F3 (private key in JWKS) is the most severe; all three REPROCESS
Node 8 — Arbiter verdict:
  REPROCESS — Three critical violations:
  F1: Authorization codes not invalidated after use (RFC 6749 §4.1.2 MUST). Replay revocation
    also impossible without tracking. Mark codes used on first exchange.
  F2: Grant type enforcement absent (RFC 6749 §3.1.1). Service accounts can use user-delegation
    flows. Validate grant_type per client registration.
  F3: Private RSA key parameters in JWKS endpoint — any caller can forge JWTs (RFC 7517).
    This is the most critical finding: complete JWT authentication bypass. Remove private key
    parameters from JWKS immediately; rotate signing keys; revoke all outstanding tokens.
Node 9 — Format: REPROCESS; F3 most severe (JWT forgery); F1/F2 also REPROCESS; priorities noted
```

**Reason**
Three independent REPROCESS violations demonstrate that the overall verdict is always REPROCESS regardless of severity ordering. F3 (private key in JWKS) is the most severe — complete JWT forgery capability. F1 and F2 are standard REPROCESS. The manifest shows how priorities should be ordered in remediation guidance.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 × 2 / T-rfc-jwt
Node 4:
  F1: code not invalidated → §4.1.2 MUST × 2 → REPROCESS
  F2: grant type not enforced → §3.1.1 MUST → REPROCESS
  F3: private RSA key params in JWKS → RFC 7517 → complete JWT forgery → REPROCESS
Node 8 — Arbiter: REPROCESS — Three violations. F3 most critical (private key exposure;
  forge any JWT). F1 auth code reuse. F2 grant type bypass. Remediate in priority order.
Node 9 — Format: REPROCESS; three violations; F3 most critical
```

---

## Pair[CB4-3.4-691]

**A**
```
Post-remediation review — prior three-violation scenario:

All three findings from the prior audit addressed:
1. Authorization codes now marked used on first exchange. Replay → 400.
   All tokens revoked on replay detection. Lifetime reduced to 45 seconds.
2. Token endpoint now validates grant_type against registered list. Unregistered
   grant types → 400 unauthorized_client. Service accounts restricted to
   client_credentials only.
3. JWKS endpoint updated: private key parameters removed. Public keys only
   (n, e, kid, use, alg). Signing keys rotated; all prior tokens revoked.
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-oauth2 / T-rfc-jwt
Node 2 — Translation: post-remediation for three prior REPROCESS findings
Node 3 — Plan: verify each remediation against original finding criterion
Node 4 — Processing:
  F1: DB flag + replay 400 + all tokens revoked + 45s lifetime → §4.1.2 both MUST → PASS
  F2: grant_type per registration + 400 unauthorized_client → §3.1.1 + §5.2 → PASS
      Service accounts restricted to client_credentials → consent bypass prevented → PASS
  F3: private keys removed; public keys only; signing keys rotated; prior tokens revoked → PASS
      RFC 7517 JWKS exposure resolved; no private key in endpoint
Node 5 — Application: PASS × 3
Node 6 — Translation: all three prior REPROCESS findings resolved
Node 7 — Arbiter: post-remediation PASS
Node 8 — Arbiter verdict:
  PASS — All three critical findings remediated. Authorization code lifecycle: single-use
  enforced with replay revocation (RFC 6749 §4.1.2). Grant type enforcement: per-client
  validation with correct error response (RFC 6749 §3.1.1). JWKS: private key parameters
  removed; public keys only; signing keys rotated; all prior tokens revoked (RFC 7517).
  No findings.
Node 9 — Format: PASS; three prior violations resolved; no remaining findings
```

**Reason**
Post-remediation PASS for all three ext26+ext25 critical violations. The F3 remediation includes the necessary steps: remove private key params + rotate signing keys + revoke prior tokens (all tokens signed with the compromised key are potentially forged and must be invalidated).

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-oauth2 / T-rfc-jwt
Node 4:
  F1: single-use + revocation + 45s → §4.1.2 PASS
  F2: per-registration + 400 unauthorized_client → §3.1.1 PASS
  F3: private key removed + rotation + prior tokens revoked → RFC 7517 PASS
Node 8 — Arbiter: PASS — All three findings resolved. Key rotation + token revocation
  (F3) complete. No remaining violations.
Node 9 — Format: PASS; three findings confirmed resolved
```

---

## Pair[CB4-3.4-692]

**A**
```
Developer argument about grant type flexibility:

"We need some clients to be able to switch between authorization_code and client_credentials
based on context. When a user is logged in, the service uses authorization_code. When
running in batch mode overnight, the same client_id uses client_credentials. We registered
both grant types for this client: grant_types=["authorization_code", "client_credentials"].
Is this configuration safe?"
```

**B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 (RFC 6749 §3.1.1) + T-asvs-authorization
Node 2 — Translation: dual grant type registration for same client — sub-phase 3.4
Node 3 — Plan: check whether dual registration is spec-permitted; assess security implications
Node 4 — Processing:
  RFC 6749 §3.1.1: grant type restriction per client — but restriction to MULTIPLE registered
    grant types is not prohibited; a client can register for multiple grant types
  Is authorization_code + client_credentials for the same client_id safe?
    These two grant types have different security properties:
    authorization_code: scoped to user consent; user can revoke; tokens tied to user context
    client_credentials: machine-to-machine; no user context; broader machine scope
    Sharing a client_id: the same client credentials (client_id + client_secret) can be used
    for EITHER flow → if client secret is compromised, attacker can use client_credentials
    to get tokens without any user interaction
    "Authorization_code flow has user consent" → true for one flow; bypass possible via the other
  Is this a REPROCESS? Registration of both is not a spec violation per §3.1.1
    The server correctly enforces: only the two registered grant types accepted
  Is this a CONCERN? YES: dual registration conflates user-delegated and machine scope
    under the same client identity. Security boundary erosion is a CONCERN:
    client credentials compromise → machine access to resources without user consent
  Recommendation: separate client IDs for user-delegated and machine flows
    (one client_id for authorization_code, a different one for client_credentials)
    This is the defense-in-depth approach: separate credentials, separate audit trails
Node 5 — Application: CONCERN (dual grant type registration; security boundary erosion)
Node 6 — Translation: CONCERN — spec-permitted but security boundary design gap
Node 7 — Arbiter: T-rfc-oauth2 — grant type configuration
Node 8 — Arbiter verdict:
  CONCERN — Registering both authorization_code and client_credentials for the same client_id
  is spec-permitted but erodes the security boundary between user-delegated and machine access.
  A compromised client secret enables machine-mode access (client_credentials) without user
  involvement, bypassing the consent requirement of the authorization_code flow. Recommend
  separate client IDs for user-delegated and batch/machine use: different credentials,
  separate audit trails, and independent authorization scope.
Node 9 — Format: CONCERN; dual grant type security boundary erosion; separate client IDs recommended
```

**Reason**
Dual grant type registration is spec-permitted (§3.1.1 allows registering multiple grant types), so not REPROCESS. The CONCERN is that the same client credentials can be used for both user-delegated and machine flows — a compromised secret bypasses user consent. This is the Stratum B design-gap-vs-spec-violation distinction.

**Phrasing-B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 / T-asvs-authorization
Node 4:
  Dual registration: spec-permitted (§3.1.1 allows multiple grant types) → not REPROCESS
  Security boundary: same client_id for user-delegated + machine → credential compromise
    enables machine access without user consent → security boundary erosion → CONCERN
  Recommendation: separate client IDs for user-delegated vs. machine flows
Node 8 — Arbiter: CONCERN — Dual grant type registration is permitted but erodes security
  boundary. Use separate client IDs for authorization_code and client_credentials.
Node 9 — Format: CONCERN; boundary erosion; separate IDs recommended
```

---

## Pair[CB4-3.4-693]

**A**
```
Multi-source review: authorization code + SSDF:

Finding 1 — Authorization code: single-use enforced. Replay → 400. Token revocation
  on replay. 60-second lifetime.
Finding 2 — SSDF PW.4 (Use of approved tools): the team uses an open-source OAuth 2.0
  library (oauth2-server v3.2). No process for monitoring the library for CVEs.
  Last review: 6 months ago. The library has 2 known CVEs in the current version
  (CVSSv3 6.1 and 7.4) that have not been remediated.
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 6749 §4.1.2) / T-ssdf-process (PW.4 + PO.3)
Node 2 — Translation:
  F1: authorization code lifecycle
  F2: known CVEs in OAuth library; SSDF PW.4 / PO.3
Node 3 — Plan: F1=§4.1.2-check; F2=known-CVE-assessment
Node 4 — Processing:
  F1: single-use + replay 400 + revocation + 60s → §4.1.2 PASS
  F2: 2 known CVEs (CVSS 6.1 and 7.4) not remediated → SSDF PW.4 + PO.3 gap
      CVSSv3 7.4 = HIGH severity; known, unmitigated vulnerability in production
      "No monitoring process" → CVE discovered 6 months into a potential window
      SSDF PO.3.2: SBOM + CVE monitoring required; absent → CONCERN normally
      But: active known HIGH CVE (7.4) in production = known exploitable vulnerability
      PW.4: use of approved tools requires validation that tools are free of known critical issues
      Known HIGH CVE in production security library = REPROCESS (not just CONCERN)
      Compare: no SBOM = CONCERN; known HIGH CVE unmitigated = REPROCESS
      The distinction: CONCERN = process gap; REPROCESS = known active vulnerability present
Node 5 — Application: PASS (F1) + REPROCESS (F2)
Node 6 — Translation: F2 REPROCESS (known HIGH CVE unmitigated in production security library)
Node 7 — Arbiter: T-ssdf-process — known CVE in OAuth library; F1 PASS
Node 8 — Arbiter verdict:
  REPROCESS — Known HIGH severity CVE (CVSSv3 7.4) unmitigated in production OAuth 2.0
  library. This escalates beyond a process CONCERN: a known active vulnerability in the
  security infrastructure (the OAuth library itself) is a production risk requiring
  immediate remediation. SSDF PW.4 requires approved tools free of known critical issues.
  Authorization code lifecycle (F1) is correctly implemented. Remediate: update oauth2-server
  to a version without the HIGH CVE; implement CVE monitoring for all security-relevant libraries.
Node 9 — Format: REPROCESS; F2 known HIGH CVE primary; F1 PASS noted
```

**Reason**
Known HIGH CVE unmitigated in the OAuth library escalates from CONCERN (no process) to REPROCESS (known active vulnerability). The SSDF distinction: PO.3 gap (no SBOM) = CONCERN; but knowing about a HIGH CVE and not remediating = active production risk = REPROCESS. This introduces the known-active-CVE escalation pattern.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 / T-ssdf-process
Node 4:
  F1: §4.1.2 all controls → PASS
  F2: CVSS 7.4 HIGH CVE known + unmitigated in production OAuth library → REPROCESS
    "No monitoring" alone → CONCERN; known HIGH CVE + no action → REPROCESS escalation
Node 8 — Arbiter: REPROCESS — F2 controls. Known HIGH CVE in production security library
  requires immediate remediation. F1 auth code correct. Update library; add CVE monitoring.
Node 9 — Format: REPROCESS; F2 primary; F1 PASS
```

---

## Pair[CB4-3.4-694]

**A**
```
Three CONCERNs — no REPROCESS:

Finding 1 — Native app: custom URI scheme (myapp://) with PKCE S256 and PAR.
Finding 2 — PKCE verifier: stored in sessionStorage (43-char CSPRNG, correct format).
Finding 3 — JWKS caching: max-age=7200 (2 hours). Key rollover window: 3 hours (>2 hours).
  Access token TTL: 15 minutes. Rollover window exceeds cache TTL.
```

**B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 (RFC 8252) / T-rfc-oauth2 (RFC 7636) / T-rfc-jwt (RFC 7517)
Node 2 — Translation: native app redirect / PKCE storage / JWKS caching
Node 3 — Plan: F1=URI-scheme; F2=sessionStorage; F3=cache-rollover-alignment
Node 4 — Processing:
  F1: custom URI scheme + PKCE S256 + PAR → mitigations present; RFC 8252 §7.2 preferred → CONCERN
  F2: sessionStorage + 43-char CSPRNG → correct format; XSS-accessible → CONCERN
  F3: max-age=7200 (2h) with rollover window 3h; access token TTL 15 min
      Rollover window (3h) > cache TTL (2h) → resource servers re-fetch before rollover ends → PASS
      Access token TTL (15 min) << rollover window (3h) → valid tokens not rejected after rotation → PASS
      F3 actually PASSES: rollover window exceeds cache TTL; no valid-token-rejection risk
      (Compare: rollover window < cache TTL = CONCERN; here rollover > cache = PASS)
  Three findings: CONCERN + CONCERN + PASS = CONCERN
  No REPROCESS present → graduation rule: CONCERN
Node 5 — Application: CONCERN (two CONCERNs; one PASS)
Node 6 — Translation: CONCERN — two gaps; no REPROCESS
Node 7 — Arbiter: T-rfc-oauth2 + T-rfc-jwt — combined CONCERN
Node 8 — Arbiter verdict:
  CONCERN — Two findings at CONCERN level:
  F1: Custom URI scheme (myapp://) carries hijacking risk; PKCE S256 + PAR mitigate;
    claimed HTTPS (RFC 8252 §7.2) preferred.
  F2: PKCE code_verifier in sessionStorage is XSS-accessible; use module-level variable.
  F3: JWKS caching (max-age=2h) with rollover window (3h) correctly ensures resource servers
    refresh before key retirement — no valid token rejection. PASS.
Node 9 — Format: CONCERN; F1+F2 CONCERNs; F3 PASS; graduation rule applied
```

**Reason**
F3 resolves to PASS after calculation: rollover window (3h) > cache TTL (2h), so resource servers re-fetch before old key is retired. Tokens issued before rotation are still valid during rollover. F1 and F2 are each CONCERN. Two CONCERNs + one PASS = CONCERN. This trains the model to correctly evaluate rollover-window-vs-cache-TTL arithmetic.

**Phrasing-B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-oauth2 / T-rfc-jwt
Node 4:
  F1: custom scheme + PKCE + PAR → mitigated CONCERN
  F2: sessionStorage XSS-accessible → CONCERN
  F3: rollover 3h > cache 2h → RS re-fetches before key retirement → PASS
  Two CONCERNs + one PASS → no REPROCESS → CONCERN
Node 8 — Arbiter: CONCERN — F1 custom scheme; F2 sessionStorage. F3 JWKS caching/rollover correct.
Node 9 — Format: CONCERN; F1+F2 CONCERNs; F3 PASS
```

---

## Pair[CB4-3.4-695]

**A**
```
Review of authorization code flow with full cross-source check:

1. Authorization code: single-use (DB flag). Replay → 400. All tokens revoked on replay.
   30-second lifetime.
2. PKCE: S256, code_verifier stored server-side (DB associated with session).
3. Grant type: token endpoint validates per registration; auth_code-only client.
4. Native app redirect: claimed HTTPS (Universal Links + App Links). PKCE as above.
5. SSDF PW.5 (signed artifacts): all OAuth library and dependency artifacts verified
   against checksums and GPG signatures before build integration.
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 × 4 + T-ssdf-process
Node 2 — Translation: comprehensive auth code flow + SSDF signed artifacts
Node 3 — Plan: five components; verify each
Node 4 — Processing:
  F1: §4.1.2 both MUST (single-use + revocation on replay) + 30s lifetime → PASS
  F2: server-side verifier storage → not browser-accessible → XSS-safe → RFC 7636 → PASS
  F3: §3.1.1 grant type enforced; auth_code only registered → PASS
  F4: claimed HTTPS + Universal Links + App Links → RFC 8252 §7.2 → PASS
      PKCE S256 server-side → RFC 8252 §8.1 → PASS
  F5: SSDF PW.5: checksums + GPG signatures on OAuth artifacts before integration → PASS
      PW.5 requires signed artifact verification; procedure is implemented → PASS
Node 5 — Application: PASS × 5
Node 6 — Translation: all five components correct
Node 7 — Arbiter: comprehensive five-component PASS
Node 8 — Arbiter verdict:
  PASS — All five components correctly implemented. Authorization code lifecycle meets
  RFC 6749 §4.1.2 (single-use + replay revocation + 30s TTL). PKCE code_verifier is
  server-side (XSS-safe). Grant type enforced per registration (§3.1.1). Native app
  redirect uses claimed HTTPS with OS verification (RFC 8252 §7.2 + §8.1). SSDF PW.5
  signed artifact verification implemented. No findings.
Node 9 — Format: PASS; five components confirmed
```

**Reason**
Five-component comprehensive PASS spanning RFC 6749, RFC 7636, RFC 8252, and SSDF PW.5. Each component is individually verified in Node 4. PW.5 (signed artifact verification) is a new positive confirmation: checksums + GPG on OAuth dependencies = SSDF PW.5 satisfied.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 × 4 + T-ssdf-process
Node 4:
  F1: §4.1.2 both MUST → PASS
  F2: server-side verifier → XSS-safe → PASS
  F3: grant type per registration → §3.1.1 → PASS
  F4: claimed HTTPS + OS verification → RFC 8252 §7.2 → PASS
  F5: checksums + GPG signatures → SSDF PW.5 → PASS
Node 8 — Arbiter: PASS — Five components all correct. No findings.
Node 9 — Format: PASS; five components confirmed
```

---

## Pair[CB4-3.4-696]

**A**
```
Security architecture dispute about mobile app redirect URIs:

"We're migrating from myapp:// to https://app.example.com/callback (Universal Links).
During transition, both schemes are registered. The old myapp:// will be removed in
the next release (6 weeks from now). The AASA file is live and verified. PKCE S256
is implemented. Developer asks: is the transitional dual-registration acceptable?"
```

**B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 (RFC 8252)
Node 2 — Translation: transitional dual redirect URI registration — sub-phase 3.4
Node 3 — Plan: assess dual registration during migration; evaluate hijacking risk during transition
Node 4 — Processing:
  Both URIs registered: both are valid during transition
    Claimed HTTPS (Universal Links): OS-verified; hijacking-resistant → secure
    Custom scheme (myapp://): registered alongside claimed HTTPS → still accessible to scheme hijacking
  Authorization server processes: requests with either redirect_uri are accepted
    If an attacker registers the same scheme "myapp" during the 6-week window:
      - An old version of the app (or a user who hasn't updated) might still send myapp:// redirect
      - Attacker's app intercepts it
    PKCE S256 present: intercepted code unusable without verifier → meaningful mitigation
    But: scheme hijacking risk still exists for the 6-week window for clients using the old URI
  The dual registration is a transitional CONCERN, not a REPROCESS:
    No MUST violation (dual registration is spec-permitted)
    The migration direction is correct (toward claimed HTTPS)
    The transition plan has a defined end date (6 weeks)
    PKCE provides meaningful mitigation during transition
  CONCERN: 6-week window; old URI still active; scheme hijacking possible for legacy clients
Node 5 — Application: CONCERN (transitional hijacking window; defined end date)
Node 6 — Translation: CONCERN — migration in progress; defined remediation timeline
Node 7 — Arbiter: T-rfc-oauth2 — transition period redirect URI
Node 8 — Arbiter verdict:
  CONCERN — Dual redirect URI registration during migration creates a 6-week window where
  the old custom URI scheme (myapp://) remains active and hijackable. PKCE S256 provides
  meaningful mitigation (intercepted code unusable without verifier). The migration direction
  is correct and has a defined timeline. Ensure: (1) force-update users to the new app version
  before removing the old URI; (2) monitor for scheme registration attempts on "myapp";
  (3) remove the custom scheme registration from the AS as soon as migration completes.
Node 9 — Format: CONCERN; transitional window; defined timeline; PKCE mitigation noted
```

**Reason**
Transitional state: both URIs registered during migration. The migration direction is correct, the end date is defined, and PKCE provides meaningful mitigation. CONCERN not REPROCESS because no MUST is violated and the situation is temporary with a plan. This tests whether the model can calibrate transitional security states correctly.

**Phrasing-B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 (RFC 8252)
Node 4:
  Claimed HTTPS live (AASA verified) → target state correct
  myapp:// still registered for 6 weeks → hijacking risk during transition window
  PKCE S256 → intercepted code unusable without verifier → meaningful mitigation
  Defined end date; migration in progress → not a permanent violation → CONCERN
Node 8 — Arbiter: CONCERN — 6-week dual-registration window; custom scheme still hijackable.
  PKCE mitigates. Accelerate migration; remove custom URI on completion.
Node 9 — Format: CONCERN; transitional window; defined timeline
```

---

## Pair[CB4-3.4-697]

**A**
```
Combined cross-source review — four findings:

1. Authorization code: single-use + replay revocation + 45-second lifetime → PASS
2. Grant type: validated per registration → PASS
3. Native app redirect: myapp:// with PKCE S256 + PAR → CONCERN
4. SSDF PO.1: no security training documented. One junior engineer introduced a scope
   validation bug 3 months ago that allowed any authenticated user to obtain admin:all
   scope via a crafted authorization request. The bug was traced to unfamiliarity with
   RFC 6749 scope semantics and OAuth 2.0 authorization model.
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-oauth2 / T-rfc-oauth2 (RFC 8252) / T-ssdf-process (PO.1)
Node 2 — Translation: four findings including PO.1 with traceable production vulnerability
Node 3 — Plan: F1/F2 PASS-checks; F3=RFC-8252-URI; F4=PO.1-escalation-check
Node 4 — Processing:
  F1: auth code correct → PASS
  F2: grant type correct → PASS
  F3: custom URI scheme + PKCE S256 + PAR → CONCERN (mitigated but not ideal)
  F4: SSDF PO.1 no training documented
    Previously established rule (ext21): PO.1 absence = CONCERN normally;
      escalates to REPROCESS when production vulnerability is traceable to knowledge gap
    Scope validation bug → CWE-269 privilege escalation in production
    Traceable to unfamiliarity with RFC 6749 scope semantics
    This is the escalation trigger: known production vulnerability + traceable knowledge gap
    → PO.1 escalates to REPROCESS
Node 5 — Application: PASS (F1) + PASS (F2) + CONCERN (F3) + REPROCESS (F4)
Node 6 — Translation: REPROCESS present → overall REPROCESS
Node 7 — Arbiter: F4 (PO.1 escalated) is controlling finding; F3 secondary CONCERN
Node 8 — Arbiter verdict:
  REPROCESS — SSDF PO.1 escalated: the absence of documented security training resulted
  in a production CWE-269 scope validation vulnerability traceable to an engineer's
  unfamiliarity with RFC 6749 scope semantics. PO.1 absence without a traceable production
  vulnerability is CONCERN; with a traceable production vulnerability, it escalates to
  REPROCESS. F3 (custom URI scheme) is a secondary CONCERN. F1 and F2 are correctly implemented.
  Primary remediation: implement mandatory security training covering OAuth 2.0 authorization
  model, RFC 6749, and scope semantics.
Node 9 — Format: REPROCESS; F4 PO.1 escalated (traceable production vulnerability); F3 secondary CONCERN
```

**Reason**
PO.1 escalation rule (ext21) triggered: no training + production vulnerability traceable to knowledge gap → REPROCESS. The production vulnerability (scope validation bug allowing admin:all for any authenticated user) is directly traceable to unfamiliarity with RFC 6749 scope semantics. F1/F2 PASS, F3 CONCERN, F4 REPROCESS → REPROCESS governs.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 × 3 / T-ssdf-process (PO.1)
Node 4:
  F1: auth code → PASS; F2: grant type → PASS
  F3: custom URI + PKCE + PAR → CONCERN
  F4: PO.1 no training + CWE-269 prod vulnerability traceable to knowledge gap → REPROCESS
      (ext21 escalation rule: CONCERN → REPROCESS when production vulnerability traceable)
Node 8 — Arbiter: REPROCESS — F4 PO.1 escalated (production vulnerability traceable to training gap).
  F3 secondary CONCERN. F1/F2 correct. Implement mandatory OAuth security training.
Node 9 — Format: REPROCESS; F4 controlling; F3 secondary; F1/F2 PASS
```

---

## Pair[CB4-3.4-698]

**A**
```
Comprehensive post-remediation PASS — all ext26 cross-source patterns:

1. Native app redirect: claimed HTTPS (Universal Links + App Links), PKCE S256 server-side.
2. Authorization code: single-use (DB flag), replay → 400, all tokens revoked on replay, 45s TTL.
3. Grant type: validated per registration; separate client IDs for user-delegated vs. machine flows.
4. OAuth library: updated to version without known CVEs. Weekly automated CVE scan (CycloneDX).
5. Security training: quarterly mandatory training covering OAuth 2.0, RFC 6749, scope semantics.
   New engineer security onboarding added to checklist.
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-oauth2 / T-rfc-oauth2 / T-ssdf-process / T-ssdf-process
Node 2 — Translation: all five ext26 cross-source patterns post-remediation
Node 3 — Plan: verify each component against applicable standard
Node 4 — Processing:
  F1: claimed HTTPS + OS verification (AASA/assetlinks.json) + PKCE S256 server-side
      → RFC 8252 §7.2 + §8.1 → PASS
  F2: single-use + 400 + all-tokens revocation + 45s TTL
      → RFC 6749 §4.1.2 both MUST conditions → PASS
  F3: grant type per registration + separate client IDs for user-delegated vs. machine
      → §3.1.1 + defense-in-depth (separate client IDs = independent credentials) → PASS
  F4: OAuth library CVE-free + weekly automated CycloneDX scan → SSDF PO.3 + PW.4 → PASS
  F5: quarterly mandatory training + OAuth/RFC curriculum + new engineer onboarding
      → SSDF PO.1 documented and implemented → PASS
Node 5 — Application: PASS × 5
Node 6 — Translation: all five patterns correctly addressed
Node 7 — Arbiter: comprehensive ext26 cross-source PASS
Node 8 — Arbiter verdict:
  PASS — All five ext26 cross-source patterns correctly implemented. Native app redirect:
  claimed HTTPS + PKCE S256 server-side (RFC 8252). Authorization code: single-use with
  replay revocation (RFC 6749 §4.1.2). Grant types: per-registration enforcement with
  separate client IDs for different grant contexts (§3.1.1). OAuth library: CVE-free with
  weekly automated scanning (SSDF PO.3/PW.4). Security training: mandatory quarterly +
  onboarding (SSDF PO.1). No findings.
Node 9 — Format: PASS; five components confirmed; no findings
```

**Reason**
Comprehensive post-remediation PASS for all ext26 cross-source patterns. F3 shows the defense-in-depth upgrade: separate client IDs (not just dual grant type restriction on one client). F5 confirms PO.1 escalation is resolved with documented quarterly training + new engineer onboarding. This closes the ext26 cross-source set.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 × 3 / T-ssdf-process × 2
Node 4:
  F1: claimed HTTPS + server-side PKCE → RFC 8252 → PASS
  F2: single-use + revocation + 45s → §4.1.2 → PASS
  F3: per-registration + separate client IDs → §3.1.1 + defense-in-depth → PASS
  F4: CVE-free library + weekly CycloneDX → PO.3/PW.4 → PASS
  F5: quarterly training + onboarding → PO.1 → PASS
Node 8 — Arbiter: PASS — All five patterns correct. No findings.
Node 9 — Format: PASS; five components confirmed
```
