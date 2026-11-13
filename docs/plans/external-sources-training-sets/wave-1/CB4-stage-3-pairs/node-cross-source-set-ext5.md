# CB4 Stage 3 — Cross-Source Pair Set Extension 5
# Sequences CB4-3.4-325 through CB4-3.4-334
# Stratum: A/B — multi-source combinations, new sub-domain patterns
#           mixed PASS/REPROCESS/CONCERN across territories
# Instances: 20 (10 base pairs × 2 phrasings)
# Running total after this file: 668 instances

---

## Pair[CB4-3.4-325]

**A:** Multi-control security review. OAuth redirect URIs: all exactly registered (wildcard migration complete). Session management: JWT sessions with 8-hour expiry — logout clears cookie, nightly 03:00 AM batch revokes sessions older than 8 hours. No logout-triggered server revocation. Password hashing: bcrypt cost 12.

**B:**
```
Territory 1: oauth-oidc
Arbiter: ASVS-V10-OAuth
V10.2.1: PASS — all redirect URIs exactly registered. Migration complete.
Territory verdict: PASS.

Territory 2: asvs-session
Arbiter: ASVS-V7-Session
V7.3.1: REPROCESS — nightly batch revocation is not logout-triggered server-side
invalidation. 8-hour post-logout exploitation window (until the 03:00 AM batch).
Nightly batch manages expiry; it does not satisfy the logout event requirement.
Territory verdict: REPROCESS.

Territory 3: asvs-authentication
Arbiter: ASVS-V6-Auth
V6.2.5: PASS — bcrypt cost 12. Appropriate memory-hard configuration.
Territory verdict: PASS.

Overall verdict: REPROCESS
Reason: V7.3.1 REPROCESS in asvs-session territory overrides PASS in oauth-oidc
and asvs-authentication territories. Two of three controls are correctly
implemented; the session revocation gap produces the overall REPROCESS.
```

**Reason:** Three territories: two PASS (exact redirect URIs, bcrypt 12), one REPROCESS (nightly batch ≠ logout-triggered revocation). REPROCESS overrides PASS per precedence. This combination teaches that partial correctness — even 2-out-of-3 correct — does not produce an overall PASS when a REPROCESS finding exists.

**Phrasing-B:** Redirect URIs migrated to exact registration (PASS), session logout using nightly batch revocation with 8-hour expiry (gap), bcrypt cost 12 (PASS). How does the overall verdict resolve across these three territories?

---

## Pair[CB4-3.4-326]

**A:** OAuth PKCE review with Security Champion involvement. Security Champion reviewed the authorization code flow implementation and found no issues. The implementation includes S256 code_challenge on all authorization requests. Token exchange endpoint accepts any code_verifier (or absent) as long as the authorization code is valid — verifier is not validated. PW.3.2 satisfied via Security Champion + OWASP checklist.

**B:**
```
Territory 1: oauth-oidc
Arbiter: ASVS-V10-OAuth
V10.4.6: REPROCESS — code_verifier not validated at token exchange. PKCE
challenge included but verifier validation absent. Authorization code
exchangeable by any code holder regardless of code_verifier value.
Territory verdict: REPROCESS.

Territory 2: ssdf-process
Arbiter: SSDF-PW
PW.3.2: PASS — Security Champion with OWASP checklist performed review.
Territory verdict: PASS.

Overall verdict: REPROCESS
Reason: V10.4.6 REPROCESS overrides PW.3.2 PASS.
Note: PW.3.2 certifies that security-focused review was performed — it does
not certify that all security vulnerabilities were found. The Security Champion's
review missing the PKCE verifier gap is consistent with PW.3.2 PASS:
the practice was performed; human reviewers, like SAST, can have blind spots.
PW.3.2 PASS and V10.4.6 REPROCESS are simultaneously valid findings.
```

**Reason:** PW.3.2 certifies that security-focused code review was performed. It does not guarantee that every security issue was detected. A Security Champion review that missed the PKCE verifier gap is analogous to SAST that misses an IDOR — the practice was followed; the gap exists. The two findings are independently valid and simultaneous.

**Phrasing-B:** Security Champion reviewed the OAuth implementation with an OWASP checklist and found no issues (PW.3.2 PASS). But code_verifier isn't validated at exchange. Can PW.3.2 PASS offset V10.4.6 REPROCESS given the review covered this area?

---

## Pair[CB4-3.4-327]

**A:** JWT service configuration review. Production: RS256 only in allowlist. Development: RS256 and HS256, isolated infrastructure. Separately, the threat model is 19 months old and was last updated before a major API redesign. SAST runs on every PR.

**B:**
```
Territory 1: asvs-token
Arbiter: ASVS-V9-Token
V9.1.1 (production): PASS — RS256 only allowlist in production.
V9.1.1 (dev divergence): CONCERN — development environment includes HS256,
  creating environment parity risk.
Territory verdict: CONCERN.

Territory 2: ssdf-process
Arbiter: SSDF-PW
PW.2: REPROCESS — threat model 19 months old, predates major API redesign.
  New API endpoints, changed trust boundaries, and new data flows are not
  modeled. Threat model does not reflect current system.
PW.4: PASS — SAST on every PR.
Territory verdict: REPROCESS.

Overall verdict: REPROCESS
Reason: PW.2 REPROCESS overrides V9.1.1 CONCERN and PW.4 PASS.
asvs-token territory CONCERN noted; ssdf-process REPROCESS sets overall verdict.
```

**Reason:** Two territories: asvs-token CONCERN (dev environment divergence, production correct), ssdf-process REPROCESS (stale threat model, SAST PASS). REPROCESS overrides CONCERN in the overall verdict. The stale threat model predating a major API redesign is a significant gap regardless of the JWT configuration quality.

**Phrasing-B:** Production JWT allowlist RS256-only (good), dev allows HS256 (divergence concern), 19-month threat model from before the API redesign, SAST running. How do V9.1.1 CONCERN and PW.2 REPROCESS combine?

---

## Pair[CB4-3.4-328]

**A:** Authentication security review. OAuth redirect URIs: wildcard pattern (*.app.example.com). PKCE: S256 correctly implemented with verifier validation at exchange. Password hashing: Argon2id m=65536, t=3, p=4. State parameter on all OAuth flows. All other OAuth sub-requirements satisfied.

**B:**
```
Territory 1: oauth-oidc
Arbiter: ASVS-V10-OAuth
V10.2.1: REPROCESS — wildcard redirect URI. Exact registration required.
V10.4.6: PASS — S256 PKCE with verifier validation.
V10.2.6: PASS — state present.
Territory verdict: REPROCESS.

Territory 2: asvs-authentication
Arbiter: ASVS-V6-Auth
V6.2.5: PASS — Argon2id m=65536, t=3, p=4.
Territory verdict: PASS.

Overall verdict: REPROCESS
Reason: V10.2.1 REPROCESS in oauth-oidc territory. Three out of four assessed
controls are correctly implemented. One wildcard URI configuration produces
the REPROCESS outcome across both territories.
```

**Reason:** Three of four controls are correct (PKCE, state, Argon2id), and one is a REPROCESS (wildcard URI). The single REPROCESS in the oauth-oidc territory sets both the territory verdict and the overall verdict, despite three other correct implementations. This reinforces the REPROCESS precedence rule.

**Phrasing-B:** Wildcard OAuth redirect URIs, correct S256 PKCE + verifier validation, state parameter, Argon2id m=65536/t=3/p=4. Three controls correct, one wildcard URI. How does the one gap affect the overall verdict?

---

## Pair[CB4-3.4-329]

**A:** Credential and token configuration review. Credentials: stored as GitHub Actions secrets and injected at pipeline time (PW.1 satisfied). JWT token allowlist: RS256 only in production. Development environment: RS256 and HS256 allowed (isolated). State parameter present on all authorization flows. No other gaps identified.

**B:**
```
Territory 1: ssdf-process
Arbiter: SSDF-PW
PW.1: PASS — GitHub Actions secrets, not in source code.
Territory verdict: PASS.

Territory 2: asvs-token
Arbiter: ASVS-V9-Token
V9.1.1 (production): PASS — RS256 only.
V9.1.1 (dev divergence): CONCERN — HS256 in dev environment.
Territory verdict: CONCERN.

Territory 3: oauth-oidc
Arbiter: ASVS-V10-OAuth
V10.2.6: PASS — state present on all flows.
Territory verdict: PASS.

Overall verdict: CONCERN
Reason: asvs-token territory CONCERN overrides all PASS territories.
PW.1 PASS (ssdf-process), V10.2.6 PASS (oauth-oidc), and V9.1.1 production
PASS do not offset the dev environment divergence CONCERN in asvs-token.
```

**Reason:** Three territories: two PASS, one CONCERN. CONCERN overrides PASS in the overall verdict. The dev environment divergence concern in the asvs-token territory sets the overall outcome even though production configuration and credential management are correctly implemented.

**Phrasing-B:** GitHub Actions secrets (correct), RS256-only production allowlist (correct), HS256 in dev environment (concern), state parameter on all OAuth flows (correct). Three controls PASS, one dev environment CONCERN. What's the overall verdict?

---

## Pair[CB4-3.4-330]

**A:** Session and token lifecycle review. Session JWT expiry: 5 minutes. Logout clears cookie; no server-side revocation. OAuth refresh tokens: revoked at logout via revocation endpoint. State parameter: present on all authorization code flows.

**B:**
```
Territory 1: asvs-session
Arbiter: ASVS-V7-Session
V7.3.1: CONCERN — 5-minute expiry substantially mitigates the post-logout
window. Server-side revocation absent. Significant mitigation (CONCERN, not
REPROCESS).
Territory verdict: CONCERN.

Territory 2: oauth-oidc
Arbiter: ASVS-V10-OAuth
V10.3 (refresh token revocation): PASS — refresh tokens revoked at logout.
V10.2.6 (state): PASS — state present on all flows.
Territory verdict: PASS.

Overall verdict: CONCERN
Reason: asvs-session CONCERN overrides oauth-oidc PASS. The 5-minute session
expiry reduces V7.3.1 severity to CONCERN but does not achieve PASS. CONCERN
overrides PASS per verdict precedence.
```

**Reason:** Correct OAuth refresh token revocation and state parameter produce PASS in oauth-oidc territory. The 5-minute session expiry produces CONCERN (not REPROCESS) in asvs-session territory. CONCERN overrides PASS overall. The OAuth controls being correct does not offset the session management CONCERN.

**Phrasing-B:** 5-minute JWT sessions with client-side logout only (CONCERN), refresh tokens revoked at logout (PASS), state parameter present (PASS). Overall verdict given CONCERN + two PASSes?

---

## Pair[CB4-3.4-331]

**A:** Authorization and operations review. JWT access tokens include role claims (`roles: ["admin", "editor"]`) that users can read from their token payload — V8.2.1 satisfied. Operations team manages RBAC assignments for 500 staff users via an admin console. Operations personnel have not received training on the RBAC model, role definitions, or grant procedures.

**B:**
```
Territory 1: asvs-authorization
Arbiter: ASVS-V8-AuthZ
V8.2.1: PASS — JWT role claims accessible to users. Users can decode their
token to read their own authorization attributes. Mechanism satisfies V8.2.1.
Territory verdict: PASS.

Territory 2: ssdf-process
Arbiter: SSDF-PW (SSDF-PO)
PO.1 (operations role training): CONCERN — operations staff performing RBAC
grant management for 500 users have not received training on the authorization
model. Incorrect grants (over-privileged or under-privileged) from untrained
operators represent a significant risk at this user scale.
Territory verdict: CONCERN.

Overall verdict: CONCERN
Reason: asvs-authorization PASS, ssdf-process CONCERN. CONCERN overrides PASS.
The technical authorization transparency control is correct; the operational
risk from untrained grant managers is the remaining concern.
```

**Reason:** V8.2.1 PASS (JWT role claims accessible) and PO.1 CONCERN (operators not trained on RBAC they're managing). CONCERN overrides PASS. This combination teaches the distinction: technical controls can be correct while operational/organizational practices have gaps, and the gap affects the overall verdict.

**Phrasing-B:** JWT role claims in tokens users can read (V8.2.1 PASS). But the ops team managing RBAC assignments for 500 users hasn't been trained on our authorization model. How does PO.1 CONCERN interact with V8.2.1 PASS?

---

## Pair[CB4-3.4-332]

**A:** PKCE implementation with a device flow exception. S256 PKCE on all authorization code flows with verifier validation. Device Authorization Grant without PKCE (RFC 8628, constrained devices). OAuth redirect URIs: validated by checking if the provided URI begins with a registered domain prefix (prefix matching, not exact).

**B:**
```
Territory: oauth-oidc
Arbiter: ASVS-V10-OAuth

V10.4.6 (PKCE on code flows): PASS — S256 with verifier validation on
authorization code flows. Device flow excluded: correct scoping per RFC 8628.

V10.2.1 (redirect URI): REPROCESS — prefix matching is not exact registration.
"Begins with registered domain" accepts any path under the domain, not just
the registered endpoint.

Territory verdict: REPROCESS
Reason: V10.2.1 REPROCESS overrides V10.4.6 PASS. Both are sub-requirements
of the same oauth-oidc territory; one REPROCESS sets the territory verdict.
Note: device flow PKCE scoping is correct — this does not create a finding.
```

**Reason:** Within the same oauth-oidc territory: V10.4.6 PASS (PKCE correct, device flow correctly scoped), V10.2.1 REPROCESS (prefix matching ≠ exact registration). The device flow PKCE scoping being correct does not offset the redirect URI gap. One REPROCESS in a territory produces REPROCESS for that territory.

**Phrasing-B:** S256 PKCE with verifier validation on code flows, device flow correctly excluded, but redirect URIs use prefix matching. V10.4.6 PASS and V10.2.1 REPROCESS in the same territory — how does the territory verdict resolve?

---

## Pair[CB4-3.4-333]

**A:** Comprehensive positive review covering new sub-domain patterns. Credentials: GitHub Actions secrets (PW.1 PASS). Passwords: bcrypt cost 12 (V6.2.5 PASS). Session revocation: Redis server-side revocation on logout (V7.3.1 PASS). OAuth redirect URIs: all exactly registered (V10.2.1 PASS). PKCE S256 with verifier validation (V10.4.6 PASS). JWT role claims in tokens (V8.2.1 PASS).

**B:**
```
Territory 1: ssdf-process
SSDF-PW: PW.1 PASS.
Territory verdict: PASS.

Territory 2: asvs-authentication
ASVS-V6-Auth: V6.2.5 bcrypt cost 12 PASS.
Territory verdict: PASS.

Territory 3: asvs-session
ASVS-V7-Session: V7.3.1 Redis revocation PASS.
Territory verdict: PASS.

Territory 4: oauth-oidc
ASVS-V10-OAuth: V10.2.1 PASS. V10.4.6 PASS.
Territory verdict: PASS.

Territory 5: asvs-authorization
ASVS-V8-AuthZ: V8.2.1 JWT role claims PASS.
Territory verdict: PASS.

Overall verdict: PASS
```

**Reason:** All five territories PASS. This is the comprehensive positive baseline for the new sub-domain patterns: GitHub secrets, bcrypt 12, Redis session revocation, exact OAuth redirect URIs, complete PKCE, JWT role claims. All of these are correct implementations that must produce PASS.

**Phrasing-B:** All new sub-domain patterns correctly implemented: GitHub secrets, bcrypt 12, Redis session revocation, exact OAuth URIs, S256 PKCE + verifier, JWT role claims. 5-territory PASS — reference baseline.

---

## Pair[CB4-3.4-334]

**A:** Security review with two session failures and one authentication control passing. OAuth redirect URIs: wildcard (*. service.example.com) — not exact registration. Session JWT with 12-hour expiry — nightly batch revocation, no logout-triggered revocation. Password hashing: bcrypt cost 12.

**B:**
```
Territory 1: oauth-oidc
ASVS-V10-OAuth: V10.2.1 REPROCESS — wildcard redirect URI.
Territory verdict: REPROCESS.

Territory 2: asvs-session
ASVS-V7-Session: V7.3.1 REPROCESS — nightly batch ≠ logout-triggered
revocation; 12-hour post-logout window is significant.
Territory verdict: REPROCESS.

Territory 3: asvs-authentication
ASVS-V6-Auth: V6.2.5 PASS — bcrypt cost 12 appropriate.
Territory verdict: PASS.

Overall verdict: REPROCESS
Reason: Two REPROCESS findings across two territories. bcrypt PASS noted but
does not offset oauth-oidc REPROCESS or asvs-session REPROCESS. Multiple
independent REPROCESS violations reported independently.
```

**Reason:** Two REPROCESS territories and one PASS territory. REPROCESS overrides PASS. Both the wildcard redirect URI (V10.2.1) and the nightly batch session revocation (V7.3.1) are independent REPROCESS findings — neither depends on the other. The correct bcrypt configuration is reported as PASS but does not change the overall verdict.

**Phrasing-B:** Wildcard OAuth redirect URIs (REPROCESS), 12-hour JWT sessions with nightly batch revocation (REPROCESS), bcrypt cost 12 (PASS). Two failures and one pass — what's the combined verdict?

---

## File Summary

| Pair ID | Description | Stratum | Territories | Verdict |
|---------|-------------|---------|-------------|---------|
| CB4-3.4-325 | Exact URIs PASS + nightly batch REPROCESS + bcrypt 12 PASS — 3-territory | A | oauth-oidc, asvs-session, asvs-authentication | REPROCESS |
| CB4-3.4-326 | V10.4.6 verifier-absent REPROCESS + PW.3.2 Security Champion PASS | B | oauth-oidc, ssdf-process | REPROCESS |
| CB4-3.4-327 | V9.1.1 dev CONCERN + PW.2 REPROCESS + PW.4 PASS | B | asvs-token, ssdf-process | REPROCESS |
| CB4-3.4-328 | V10.2.1 wildcard REPROCESS + V10.4.6+V10.2.6 PASS + Argon2id PASS | A | oauth-oidc, asvs-authentication | REPROCESS |
| CB4-3.4-329 | PW.1 PASS + V9.1.1 prod PASS + dev CONCERN + V10.2.6 PASS | B | ssdf-process, asvs-token, oauth-oidc | CONCERN |
| CB4-3.4-330 | V7.3.1 5-min CONCERN + V10.3 revocation PASS + V10.2.6 PASS | A | asvs-session, oauth-oidc | CONCERN |
| CB4-3.4-331 | V8.2.1 JWT role claims PASS + PO.1 ops RBAC training CONCERN | B | asvs-authorization, ssdf-process | CONCERN |
| CB4-3.4-332 | V10.4.6 device-flow-scoped PASS + V10.2.1 prefix REPROCESS (same territory) | A | oauth-oidc | REPROCESS |
| CB4-3.4-333 | All new-pattern 5-territory PASS baseline | A | ssdf-process, asvs-authentication, asvs-session, oauth-oidc, asvs-authorization | PASS |
| CB4-3.4-334 | V10.2.1 wildcard REPROCESS + V7.3.1 nightly-batch REPROCESS + bcrypt12 PASS | A | oauth-oidc, asvs-session, asvs-authentication | REPROCESS |
