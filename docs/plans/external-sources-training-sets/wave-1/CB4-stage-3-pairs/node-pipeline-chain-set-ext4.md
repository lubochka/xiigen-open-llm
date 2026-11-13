# CB4 Stage 3 — Pipeline Chain Set Extension 4
# Sequences CB4-3.4-275 through CB4-3.4-282
# Stratum: C — full 9-node pipeline traces, new sub-domains
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 564 instances

---

## Pair[CB4-3.4-275]

**A:** OAuth server compliance review. Redirect URIs registered as `https://*.app.example.com/oauth/callback` (wildcard subdomain). PKCE with S256 implemented correctly. No other findings.

**B:**
```
Node 01 — Domain Splitter:
Input signals: OAuth authorization code flow, wildcard redirect URI pattern,
PKCE S256 present.
Territories: [T-oauth-oidc]
Single territory. Both redirect URI and PKCE signals resolve to the same
oauth-oidc territory — the same arbiter evaluates both sub-requirements.

Node 02 — Domain Translator:
T-oauth-oidc → label: oauth-oidc-normative
Source family: ASVS V10 (OAuth and OIDC).

Node 03 — Scheme Planner:
Plan type: linear (single territory, two sub-requirements).
Branch: oauth-oidc → domain-processor → applier → result-translator
→ arbiter-selector → arbiter-unit → formatter.

Node 04 — Domain Processor:
Territory: oauth-oidc
V10.2.1 (redirect URI exact match): required. Evidence: wildcard pattern
*.app.example.com registered. GAP — wildcard not exact registration.
V10.4.6 (PKCE S256): required. Evidence: S256 implemented, code_verifier
validated. SATISFIED.

Node 05 — Applier:
Context: standard OAuth deployment. Wildcard subdomain allows code interception
via any matching subdomain. PKCE PASS does not offset redirect URI gap. Both
sub-requirements independent.

Node 06 — Result Translator:
Tags: "redirect URI", "wildcard", "V10.2.1", "exact registration",
"subdomain takeover", "PKCE", "V10.4.6", "S256", "oauth-oidc".

Node 07 — Arbiter Selector:
Territory: oauth-oidc → ASVS-V10-OAuth arbiter.

Node 08 — Arbiter Unit (ASVS-V10-OAuth):
V10.4.6: PASS — S256 PKCE correctly implemented.
V10.2.1: REPROCESS — wildcard redirect URI not exact registration.
Territory verdict: REPROCESS.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["oauth-oidc"],
  "findings": [
    {
      "territory": "oauth-oidc",
      "arbiter": "ASVS-V10-OAuth",
      "sub_findings": [
        {"requirement": "V10.4.6", "verdict": "PASS"},
        {"requirement": "V10.2.1", "verdict": "REPROCESS",
         "gap": "redirect URI registered as wildcard *.app.example.com; exact registration required"}
      ],
      "territory_verdict": "REPROCESS"
    }
  ],
  "overall_verdict": "REPROCESS",
  "source_attribution": "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
}
```

**Reason:** Single territory with two sub-requirements evaluated independently by the same arbiter. V10.4.6 PASS and V10.2.1 REPROCESS coexist within the territory. Territory verdict is REPROCESS because any REPROCESS sub-finding sets the territory outcome. Node 04 correctly extracts both sub-requirements and maps evidence to each independently before passing to Node 08.

**Phrasing-B:** OAuth server with wildcard redirect URIs (*.app.example.com) and correct S256 PKCE. Full trace — show how one sub-requirement PASS and one REPROCESS are handled within the same territory.

---

## Pair[CB4-3.4-276]

**A:** Session security review. JWT in httpOnly cookie, 7-day expiry. On logout, cookie deleted client-side only — no server-side session store or revocation list. Pre-logout-captured tokens remain valid.

**B:**
```
Node 01 — Domain Splitter:
Input signals: JWT session cookie (session management context), logout without
server-side invalidation, 7-day expiry, stateless token.
Territories: [T-asvs-session]
Session management territory. No OAuth flow signals (JWT used as session
credential, not OAuth access token). No CWE signal distinct from the session
management gap.

Node 02 — Domain Translator:
T-asvs-session → label: asvs-session-normative
Source family: ASVS V7 (Session Management).

Node 03 — Scheme Planner:
Plan type: linear (single territory).
Branch: asvs-session → domain-processor → applier → result-translator
→ arbiter-selector → arbiter-unit → formatter.

Node 04 — Domain Processor:
Territory: asvs-session
V7.3.1 (server-side session invalidation on logout): required. Evidence:
logout deletes client-side cookie only; no server-side revocation mechanism.
GAP — server-side invalidation absent.
V7.2.4 (idle timeout): not in scope for this submission (not provided).

Node 05 — Applier:
Context: 7-day session expiry. Post-logout token validity window is up to
7 days (minus time elapsed in session). Captured token exploitation window
is significant. REPROCESS severity confirmed.

Node 06 — Result Translator:
Tags: "server-side invalidation", "logout", "V7.3.1", "JWT session",
"revocation", "stateless", "cookie", "expiry window".

Node 07 — Arbiter Selector:
Territory: asvs-session → ASVS-V7-Session arbiter.

Node 08 — Arbiter Unit (ASVS-V7-Session):
V7.3.1: REPROCESS — server-side session invalidation MUST occur on logout.
Client-side cookie deletion does not constitute server-side invalidation.
7-day expiry means captured tokens remain exploitable for up to 7 days
post-logout. Stateless JWT without revocation cannot honor logout intent.
Territory verdict: REPROCESS.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["asvs-session"],
  "findings": [
    {
      "territory": "asvs-session",
      "arbiter": "ASVS-V7-Session",
      "sub_findings": [
        {"requirement": "V7.3.1", "verdict": "REPROCESS",
         "gap": "no server-side session revocation on logout; stateless JWT valid for up to 7 days post-logout"}
      ],
      "territory_verdict": "REPROCESS"
    }
  ],
  "overall_verdict": "REPROCESS",
  "source_attribution": "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
}
```

**Reason:** Single asvs-session territory. Node 01 correctly identifies this as a session management gap, not an OAuth token issue (JWT is used as a session credential, not in an OAuth flow). Node 04 extracts V7.3.1 as the applicable requirement. Node 08 rejects the implicit "stateless JWT can't be revoked" argument by stating that the system architecture must support revocation (session store, blacklist, or short expiry) — the architecture limitation does not exempt the requirement.

**Phrasing-B:** Session logout with httpOnly JWT cookie: logout clears the cookie client-side, but the token stays cryptographically valid for its full 7-day window. No session store. Show the full pipeline trace for V7.3.1.

---

## Pair[CB4-3.4-277]

**A:** PKCE implementation audit. Authorization requests use `code_challenge_method=plain`. The code_challenge equals the raw code_verifier. Team references RFC 7636 defining plain as a valid method value.

**B:**
```
Node 01 — Domain Splitter:
Input signals: PKCE implementation, code_challenge_method=plain, RFC 7636
citation, ASVS V10 signal.
Territories: [T-oauth-oidc, T-rfc-oauth2]
Both ASVS V10 and RFC 7636 normative requirements apply to PKCE method
selection. Two territories.

Node 02 — Domain Translator:
T-oauth-oidc → oauth-oidc-normative (ASVS V10)
T-rfc-oauth2 → rfc-oauth2-normative (RFC 7636)

Node 03 — Scheme Planner:
Plan type: parallel (two territories, RFC and ASVS independently evaluate
PKCE method requirement).
Branch A: oauth-oidc
Branch B: rfc-oauth2
Sync at formatter.

Node 04 — Domain Processor (parallel):
oauth-oidc: V10.4.6 requires S256. Evidence: plain method. GAP.
rfc-oauth2: RFC 7636 Section 4.2 — "If the client is capable of using
  'S256', it MUST use 'S256'". Evidence: plain used on a standard platform
  capable of SHA256. GAP — plain used when S256 is available.
RFC argument ("plain is defined in RFC 7636"): noted; passed to arbiter.

Node 05 — Applier:
Context: standard web/mobile OAuth client — SHA256 hashing available.
S256 availability confirmed. Plain method provides no protection against
code interception. Both territories confirm REPROCESS.

Node 06 — Result Translator:
oauth-oidc tags: "PKCE", "code_challenge_method", "plain", "S256", "V10.4.6".
rfc-oauth2 tags: "RFC 7636", "Section 4.2", "S256 MUST", "plain fallback",
  "code interception", "rfc-oauth2-normative".

Node 07 — Arbiter Selector:
oauth-oidc → ASVS-V10-OAuth arbiter.
rfc-oauth2 → RFC-OAuth2 arbiter.

Node 08 — Arbiter Units (parallel):
ASVS-V10-OAuth: V10.4.6 REPROCESS — plain method does not satisfy V10.4.6.

RFC-OAuth2: RFC 7636 Section 4.2 REPROCESS.
Argument rejected: "RFC 7636 defines plain as a valid method."
RFC 7636 Section 4.2 states: "If the client is capable of using 'S256', it
MUST use 'S256'." Plain is defined as a fallback for environments that cannot
support SHA256 — not a coequal alternative. A standard platform capable of
hashing MUST use S256.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["oauth-oidc", "rfc-oauth2"],
  "findings": [
    {
      "territory": "oauth-oidc",
      "arbiter": "ASVS-V10-OAuth",
      "sub_findings": [
        {"requirement": "V10.4.6", "verdict": "REPROCESS",
         "gap": "code_challenge_method=plain; S256 required"}
      ],
      "territory_verdict": "REPROCESS"
    },
    {
      "territory": "rfc-oauth2",
      "arbiter": "RFC-OAuth2",
      "sub_findings": [
        {"standard": "RFC 7636 §4.2", "verdict": "REPROCESS",
         "gap": "S256 capable client MUST use S256; plain is a constrained-environment fallback only",
         "argument_rejected": "RFC 7636 defines both plain and S256 — misreads §4.2 conditional"}
      ],
      "territory_verdict": "REPROCESS"
    }
  ],
  "overall_verdict": "REPROCESS",
  "source_attribution": [
    "OWASP ASVS 5.0.0 — CC BY-SA 4.0",
    "IETF RFC 7636 — IETF Trust provisions"
  ]
}
```

**Reason:** Two territories because RFC 7636 and ASVS V10 independently impose requirements on PKCE method selection. Node 04 correctly notes the RFC argument and passes it to Node 08 for adjudication (not Node 04's responsibility). Node 08 evaluates the RFC argument against the actual RFC 7636 Section 4.2 text and rejects it. Both territories REPROCESS.

**Phrasing-B:** PKCE with plain method, team arguing RFC 7636 allows it since plain is a defined method value. Full 9-node trace showing the RFC argument path through the pipeline.

---

## Pair[CB4-3.4-278]

**A:** Application security audit finding: `STRIPE_API_KEY = "sk_live_REDACTED_SECRET_REMOVED..."` hardcoded in `billing/stripe_client.py`, committed to the main repository branch for 6 months. The key is used in payment processing flows that authenticate to the Stripe API.

**B:**
```
Node 01 — Domain Splitter:
Input signals: hardcoded credential in source code (SSDF PW.1 signal),
credential used in authentication-adjacent component (payment API auth),
version control exposure (CWE-798 signal).
Territories: [T-ssdf-process, T-cwe-authn]
Process evidence gap (PW.1) and authentication weakness (CWE-798 — hardcoded
credential in authentication-adjacent code).

Node 02 — Domain Translator:
T-ssdf-process → ssdf-process-normative
T-cwe-authn → cwe-authn-normative

Node 03 — Scheme Planner:
Plan type: parallel (two independent territories).
Branch A: ssdf-process
Branch B: cwe-authn
Sync at formatter.

Node 04 — Domain Processor (parallel):
ssdf-process: PW.1 — credentials MUST NOT be hardcoded. Evidence: Stripe
  live API key in source code, version-controlled 6 months. GAP confirmed.
cwe-authn: CWE-798 (Use of Hard-coded Credentials) — hardcoded credential
  used to authenticate to an external payment service. The credential grants
  complete Stripe API access (charges, refunds, customer data). GAP confirmed.

Node 05 — Applier:
Context: payment processing. Stripe live key exposure means: any repository
reader can initiate charges, issue refunds, or access customer payment data.
CRITICAL severity for cwe-authn territory confirmed. ssdf-process: REPROCESS.

Node 06 — Result Translator:
ssdf-process tags: "hardcoded credential", "PW.1", "version control",
  "secrets manager", "rotation required".
cwe-authn tags: "CWE-798", "hardcoded credentials", "Stripe API key",
  "payment authentication", "authentication bypass".

Node 07 — Arbiter Selector:
ssdf-process → SSDF-PW arbiter.
cwe-authn → CWE-Authn arbiter.

Node 08 — Arbiter Units (parallel):
SSDF-PW: PW.1 REPROCESS — live Stripe API key hardcoded in source code,
version-controlled 6 months. Credential exposed to all repository access
paths and historical forks.

CWE-Authn: CWE-798 REPROCESS — CRITICAL. Hardcoded credential granting
full payment API authentication capability. Any party with repository access
can authenticate as the application to Stripe's production API, enabling
charges, refunds, and customer data access without interacting with the
application's authentication layer.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["ssdf-process", "cwe-authn"],
  "findings": [
    {
      "territory": "ssdf-process",
      "arbiter": "SSDF-PW",
      "sub_findings": [
        {"practice": "PW.1", "verdict": "REPROCESS",
         "gap": "Stripe live API key hardcoded in billing/stripe_client.py, 6 months in version control"}
      ],
      "territory_verdict": "REPROCESS"
    },
    {
      "territory": "cwe-authn",
      "arbiter": "CWE-Authn",
      "sub_findings": [
        {"cwe": "CWE-798", "verdict": "REPROCESS", "severity": "CRITICAL",
         "gap": "hardcoded Stripe live API key enables full payment API authentication by any repository reader"}
      ],
      "territory_verdict": "REPROCESS — CRITICAL"
    }
  ],
  "overall_verdict": "REPROCESS — CRITICAL",
  "source_attribution": [
    "NIST SP 800-218 SSDF v1.1 — public domain",
    "CWE Top 25 2023 — public domain (MITRE)"
  ]
}
```

**Reason:** Two territories for hardcoded credentials — SSDF PW.1 (practice gap) and CWE-798 (weakness pattern in authentication-adjacent code). The Stripe live key grants complete payment API authentication capability, making this CRITICAL. Version control exposure means the credential is accessible to all repository readers regardless of whether they interact with the running application.

**Phrasing-B:** Live Stripe API key hardcoded in billing/stripe_client.py, 6 months in git. Used for payment processing authentication. Full pipeline trace with both SSDF and CWE perspectives.

---

## Pair[CB4-3.4-279]

**A:** Password storage review. Passwords hashed with bcrypt, cost factor 4. Team argument: "bcrypt IS a memory-hard function — V6.2.5 requires memory-hard functions and we have one. The cost parameter is a performance tuning choice, not a security choice. Our authentication endpoints are fast and reliable."

**B:**
```
Node 01 — Domain Splitter:
Input signals: password storage, bcrypt, cost factor 4, V6.2.5 argument,
memory-hard algorithm class claim.
Territories: [T-asvs-authentication]
Single territory. Authentication credential storage.

Node 02 — Domain Translator:
T-asvs-authentication → asvs-authentication-normative
Source family: ASVS V6 (Authentication).

Node 03 — Scheme Planner:
Plan type: linear (single territory).
Note: cost factor argument passed through as context for Node 08 adjudication.

Node 04 — Domain Processor:
Territory: asvs-authentication
V6.2.5: approved memory-hard function with appropriate work factor required.
Evidence: bcrypt cost 4. Algorithm: correct class (memory-hard). Configuration:
below recommended threshold (≥10). Partial satisfaction.
Argument noted: "cost parameter is performance tuning, not security." Passed
to arbiter.

Node 05 — Applier:
Context: standard web application. Authentication endpoint performance at
cost 10 is typically 200-400ms — acceptable for interactive login. Cost 4
provides insufficient offline attack resistance on current hardware. Context
confirms CONCERN (not REPROCESS: algorithm class correct).

Node 06 — Result Translator:
Tags: "bcrypt", "cost factor", "V6.2.5", "memory-hard", "work factor",
"offline attack resistance", "configuration".

Node 07 — Arbiter Selector:
Territory: asvs-authentication → ASVS-V6-Auth arbiter.

Node 08 — Arbiter Unit (ASVS-V6-Auth):
V6.2.5: CONCERN.
Argument partially rejected: "cost parameter is a performance choice."

bcrypt cost factor determines the computational effort required to hash one
password. This is not purely a performance tuning choice — it is a security
parameter that directly controls resistance to offline brute-force and dictionary
attacks. A cost factor that enables hundreds of thousands of hashes per second
per GPU does not fulfill V6.2.5's intent of slowing offline attack campaigns.

However: bcrypt is a memory-hard algorithm. The algorithm class is correct
for V6.2.5. The gap is configuration, not algorithm selection. This is
CONCERN (not REPROCESS): correct algorithm class, insufficient work factor.
The distinction: MD5+salt = REPROCESS (wrong algorithm); bcrypt cost 4 = CONCERN
(right algorithm, wrong configuration).

Territory verdict: CONCERN.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["asvs-authentication"],
  "findings": [
    {
      "territory": "asvs-authentication",
      "arbiter": "ASVS-V6-Auth",
      "sub_findings": [
        {"requirement": "V6.2.5", "verdict": "CONCERN",
         "note": "bcrypt algorithm class correct; cost factor 4 insufficient — argument 'cost is performance not security' rejected",
         "gap": "bcrypt cost 4 provides insufficient offline attack resistance; cost ≥10 recommended"}
      ],
      "territory_verdict": "CONCERN"
    }
  ],
  "overall_verdict": "CONCERN",
  "source_attribution": "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
}
```

**Reason:** The cost-as-performance argument is rejected at Node 08 — bcrypt cost directly controls security, not just performance. The CONCERN (not REPROCESS) determination is explicitly traced: correct algorithm class → CONCERN; if the algorithm were MD5+salt → REPROCESS. The chain from Node 04's evidence mapping through Node 08's adjudication shows this classification reasoning explicitly.

**Phrasing-B:** bcrypt cost 4 — team says "we use bcrypt which is memory-hard, V6.2.5 is satisfied; the cost factor is just a performance parameter." Full trace showing how the argument is handled and why the verdict is CONCERN not REPROCESS.

---

## Pair[CB4-3.4-280]

**A:** Full session and token lifecycle review. On logout: session cookie deleted client-side (JWT session token not server-side revoked). OAuth refresh tokens for the session not revoked at logout. Both the session layer and the OAuth token layer fail to honor logout intent.

**B:**
```
Node 01 — Domain Splitter:
Input signals: JWT session cookie logout gap (V7.3.1 signal), OAuth refresh
token not revoked on logout (V10.3 signal), two separate lifecycle gaps.
Territories: [T-asvs-session, T-oauth-oidc]
Session management territory (V7) and OAuth token territory (V10) — different
arbiters, different sub-requirements.

Node 02 — Domain Translator:
T-asvs-session → asvs-session-normative
T-oauth-oidc → oauth-oidc-normative

Node 03 — Scheme Planner:
Plan type: parallel (two territories, independent gaps in different lifecycle
layers — session and OAuth token).

Node 04 — Domain Processor (parallel):
asvs-session: V7.3.1 — server-side session invalidation on logout. Evidence:
  cookie deleted only, no server-side revocation. GAP confirmed.
oauth-oidc: V10.3.x — refresh token revocation. Evidence: refresh tokens not
  invalidated at logout. GAP confirmed.

Node 05 — Applier:
Context: application with both session tokens and OAuth refresh tokens.
Two independent lifecycle failure points on logout. Both territories REPROCESS.

Node 06 — Result Translator:
asvs-session tags: "V7.3.1", "server-side invalidation", "logout",
  "session revocation", "JWT session cookie".
oauth-oidc tags: "V10.3", "refresh token revocation", "logout",
  "token lifecycle", "token invalidation".

Node 07 — Arbiter Selector:
asvs-session → ASVS-V7-Session arbiter.
oauth-oidc → ASVS-V10-OAuth arbiter.

Node 08 — Arbiter Units (parallel):
ASVS-V7-Session: V7.3.1 REPROCESS — server-side session invalidation not
performed on logout. Client-side cookie deletion insufficient.

ASVS-V10-OAuth: V10.3.x REPROCESS — OAuth refresh tokens must be revoked
when the user terminates the session. Refresh tokens captured before logout
can be used to obtain new access tokens indefinitely after the user has
terminated the session.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["asvs-session", "oauth-oidc"],
  "findings": [
    {
      "territory": "asvs-session",
      "arbiter": "ASVS-V7-Session",
      "sub_findings": [
        {"requirement": "V7.3.1", "verdict": "REPROCESS",
         "gap": "no server-side session revocation on logout"}
      ],
      "territory_verdict": "REPROCESS"
    },
    {
      "territory": "oauth-oidc",
      "arbiter": "ASVS-V10-OAuth",
      "sub_findings": [
        {"requirement": "V10.3", "verdict": "REPROCESS",
         "gap": "OAuth refresh tokens not revoked on user logout"}
      ],
      "territory_verdict": "REPROCESS"
    }
  ],
  "overall_verdict": "REPROCESS",
  "source_attribution": "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
}
```

**Reason:** Two territories because V7 (session management) and V10 (OAuth tokens) are different ASVS chapters with different arbiters. Both gaps exist at the same logout event but are independent requirements: session token server-side invalidation (V7.3.1) and refresh token revocation (V10.3). The full trace shows parallel evaluation of both and how logout touches multiple security layers.

**Phrasing-B:** Session logout review: JWT session cookie not server-side revoked, AND OAuth refresh tokens not revoked on logout. Two separate logout lifecycle failures. Full pipeline trace for both V7.3.1 and V10.3.

---

## Pair[CB4-3.4-281]

**A:** OAuth server assessment with adversarial argument. Redirect URIs use wildcard pattern `https://*.service.example.com/auth/callback`. Team argument: "All subdomains matching *.service.example.com are owned and operated by our organization. We don't have externally contributed subdomains. There is no subdomain takeover risk because we control all DNS entries for service.example.com."

**B:**
```
Node 01 — Domain Splitter:
Input signals: OAuth flow, wildcard redirect URI, "we control all subdomains"
argument. T-oauth-oidc territory.

Node 02 — Domain Translator:
T-oauth-oidc → oauth-oidc-normative

Node 03 — Scheme Planner:
Linear. Argument passed to Node 08.

Node 04 — Domain Processor:
V10.2.1: exact redirect URI registration required. Evidence: wildcard pattern.
Argument noted: "all subdomains owned by us." Passed to arbiter for adjudication.

Node 05 — Applier:
Context: organization claiming full subdomain control. Argument has surface
plausibility; requires arbiter-level evaluation of whether ownership eliminates
the risk V10.2.1 addresses.

Node 06 — Result Translator:
Tags: "V10.2.1", "redirect URI", "wildcard", "subdomain", "exact registration".

Node 07 — Arbiter Selector:
oauth-oidc → ASVS-V10-OAuth.

Node 08 — Arbiter Unit (ASVS-V10-OAuth):
V10.2.1: REPROCESS.
Argument partially rejected: "we own all subdomains, no takeover risk."

(1) V10.2.1 requires exact registration as a practice — the standard does not
    provide exceptions for "claimed complete ownership." The exact-registration
    requirement exists because ownership claims cannot be continuously verified
    at runtime and are subject to change.

(2) Subdomain control is not permanently stable: (a) DNS configuration errors
    can temporarily expose a subdomain, (b) internal services on subdomains
    may be compromised (an attacker on *.service.example.com can receive OAuth
    codes even without DNS-level takeover), (c) organizational changes (divested
    subdomains, expired internal services) can create unexpected exposure.

(3) The risk model under V10.2.1 extends beyond external subdomain takeover:
    any service at any matching subdomain can be the redirect target, including
    internal services that have XSS vulnerabilities or that log all parameters.

(4) Exact registration is operationally straightforward: list each subdomain
    that legitimately needs OAuth callback. The operational cost of exact
    registration is low; the security benefit is meaningful.

Evidence gap: redirect URI registered as wildcard; exact registration required
regardless of organizational subdomain ownership claims.
Overall verdict: REPROCESS

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["oauth-oidc"],
  "findings": [
    {
      "territory": "oauth-oidc",
      "arbiter": "ASVS-V10-OAuth",
      "sub_findings": [
        {"requirement": "V10.2.1", "verdict": "REPROCESS",
         "argument_rejected": "'we own all subdomains' — ownership claim does not satisfy exact-registration requirement; internal service compromise and operational DNS risk remain",
         "gap": "wildcard redirect URI; exact registration required"}
      ],
      "territory_verdict": "REPROCESS"
    }
  ],
  "overall_verdict": "REPROCESS",
  "source_attribution": "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
}
```

**Reason:** The "we own all subdomains" argument is the adversarial form of the wildcard redirect URI scenario. Node 08 rejects it on three grounds: (1) the requirement does not have an ownership exception, (2) subdomain ownership is not permanently stable, (3) internal services at matching subdomains can be compromised even under organizational ownership. The argument rejection is documented in the formatter output.

**Phrasing-B:** Wildcard redirect URIs for our OAuth server — *.service.example.com. We own all those subdomains, no third parties, full DNS control. The wildcard risk is subdomain takeover, which can't happen here. Full trace including how the ownership argument is evaluated.

---

## Pair[CB4-3.4-282]

**A:** Comprehensive security review of a well-secured platform. All new sub-domain controls in place: exact OAuth redirect URIs registered, server-side session revocation on logout via Redis session store, S256 PKCE, bcrypt cost 12, user permissions API at `/me/permissions`, no hardcoded credentials (secrets manager deployed), security-focused code review checklist in place.

**B:**
```
Node 01 — Domain Splitter:
Input signals: exact OAuth redirect URIs (V10.2.1 satisfied), server-side
session revocation (V7.3.1 satisfied), S256 PKCE (V10.4.6 satisfied), bcrypt
cost 12 (V6.2.5 satisfied), /me/permissions (V8.2.1 satisfied), no hardcoded
creds (PW.1 satisfied), security review checklist (PW.3.2 satisfied).
Territories: [T-oauth-oidc, T-asvs-session, T-asvs-authentication,
              T-asvs-authorization, T-ssdf-process]

Node 02 — Domain Translator:
All five territories → respective normative labels.

Node 03 — Scheme Planner:
Plan type: parallel (five independent territories, all evidence satisfactory).

Node 04 — Domain Processor (parallel):
oauth-oidc: V10.2.1 exact URIs SATISFIED. V10.4.6 S256 SATISFIED.
asvs-session: V7.3.1 Redis session store revocation SATISFIED.
asvs-authentication: V6.2.5 bcrypt cost 12 SATISFIED.
asvs-authorization: V8.2.1 /me/permissions endpoint SATISFIED.
ssdf-process: PW.1 no hardcoded creds SATISFIED. PW.3.2 security checklist SATISFIED.

Node 05 — Applier:
Context: well-secured platform. No context factor elevates any finding.

Node 06 — Result Translator:
All territories: positive compliance vocabulary per territory.

Node 07 — Arbiter Selector:
oauth-oidc → ASVS-V10-OAuth
asvs-session → ASVS-V7-Session
asvs-authentication → ASVS-V6-Auth
asvs-authorization → ASVS-V8-AuthZ
ssdf-process → SSDF-PW

Node 08 — Arbiter Units (all parallel):
ASVS-V10-OAuth: V10.2.1 PASS. V10.4.6 PASS. Territory PASS.
ASVS-V7-Session: V7.3.1 PASS. Territory PASS.
ASVS-V6-Auth: V6.2.5 PASS. Territory PASS.
ASVS-V8-AuthZ: V8.2.1 PASS. Territory PASS.
SSDF-PW: PW.1 PASS. PW.3.2 PASS. Territory PASS.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["oauth-oidc", "asvs-session", "asvs-authentication",
                  "asvs-authorization", "ssdf-process"],
  "findings": [
    {"territory": "oauth-oidc", "arbiter": "ASVS-V10-OAuth",
     "territory_verdict": "PASS"},
    {"territory": "asvs-session", "arbiter": "ASVS-V7-Session",
     "territory_verdict": "PASS"},
    {"territory": "asvs-authentication", "arbiter": "ASVS-V6-Auth",
     "territory_verdict": "PASS"},
    {"territory": "asvs-authorization", "arbiter": "ASVS-V8-AuthZ",
     "territory_verdict": "PASS"},
    {"territory": "ssdf-process", "arbiter": "SSDF-PW",
     "territory_verdict": "PASS"}
  ],
  "overall_verdict": "PASS",
  "source_attribution": [
    "OWASP ASVS 5.0.0 — CC BY-SA 4.0",
    "NIST SP 800-218 SSDF v1.1 — public domain"
  ]
}
```

**Reason:** Five-territory all-PASS baseline for the new sub-domains introduced in ext10. Establishes the positive evidence standard: exact redirect URIs, server-side session revocation, S256 PKCE, appropriate bcrypt cost, permission introspection, no hardcoded credentials, security-focused code review. Full pipeline trace confirms each node processes positive evidence correctly and produces PASS at each territory.

**Phrasing-B:** Platform with all new sub-domain controls: exact OAuth redirect URIs, Redis-backed session revocation on logout, S256 PKCE, bcrypt cost 12, /me/permissions, no hardcoded creds, security review checklist. Show the 5-territory all-PASS pipeline trace.

---

## File Summary

| Pair ID | Description | Stratum | Territories | Verdict |
|---------|-------------|---------|-------------|---------|
| CB4-3.4-275 | V10.2.1 wildcard + V10.4.6 PASS — single territory sub-req split | C | oauth-oidc | REPROCESS |
| CB4-3.4-276 | V7.3.1 stateless JWT no revocation on logout — full trace | C | asvs-session | REPROCESS |
| CB4-3.4-277 | V10.4.6 plain + RFC 7636 "both valid" argument rejected | C | oauth-oidc, rfc-oauth2 | REPROCESS |
| CB4-3.4-278 | SSDF PW.1 + CWE-798 Stripe live key hardcoded — full trace | C | ssdf-process, cwe-authn | REPROCESS CRITICAL |
| CB4-3.4-279 | V6.2.5 bcrypt cost 4 "cost=performance" argument — CONCERN trace | C | asvs-authentication | CONCERN |
| CB4-3.4-280 | V7.3.1 + V10.3 logout revocation both fail — two territory trace | C | asvs-session, oauth-oidc | REPROCESS |
| CB4-3.4-281 | V10.2.1 wildcard "we own all subdomains" adversarial chain | C | oauth-oidc | REPROCESS |
| CB4-3.4-282 | All new sub-domain 5-territory PASS — comprehensive baseline | C | oauth-oidc, asvs-session, asvs-authentication, asvs-authorization, ssdf-process | PASS |
