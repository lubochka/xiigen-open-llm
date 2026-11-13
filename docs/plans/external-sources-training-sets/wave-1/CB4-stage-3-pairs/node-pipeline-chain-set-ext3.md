# CB4 Stage 3 — Pipeline Chain Set Extension 3
# Sequences CB4-3.4-249 through CB4-3.4-256
# Stratum: C — full 9-node pipeline traces, newest security domains
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 512 instances

---

## Pair[CB4-3.4-249]

**A:** OAuth 2.0 authorization server review for a fintech SaaS. PKCE with S256 is implemented on all authorization requests. State parameter is absent from all authorization requests — the implementation never included it. The team is asking for a compliance assessment.

**B:**
```
Node 01 — Domain Splitter:
Input signals: OAuth 2.0 authorization code flow, PKCE present, state absent.
Territories: [T-oauth-oidc]
Single territory. No authentication mechanism, session, or process evidence signals.

Node 02 — Domain Translator:
T-oauth-oidc → label: oauth-oidc-normative
Source family: ASVS V10 (OAuth and OIDC).

Node 03 — Scheme Planner:
Plan type: linear (single territory).
Branch: oauth-oidc → domain-processor → applier → result-translator → arbiter-selector
→ arbiter-unit → formatter.

Node 04 — Domain Processor:
Territory: oauth-oidc
Requirements extracted:
  V10.4.6 (PKCE): code_challenge with S256 method, code_verifier at exchange.
  V10.2.6 (state parameter): must be present on all authorization requests.
Evidence mapped:
  V10.4.6: SATISFIED — S256 code_challenge present, code_verifier validated.
  V10.2.6: GAP — state parameter absent from all authorization requests.

Node 05 — Applier:
Context: fintech SaaS authorization server. OAuth CSRF risks are elevated in
financial contexts — CSRF against the authorization response could allow account
linking to an attacker-controlled session. V10.2.6 REPROCESS is appropriate.
No mitigating context changes the requirement status.

Node 06 — Result Translator:
Vocabulary tags for oauth-oidc territory: "state parameter", "CSRF", "authorization
code flow", "V10.2.6", "V10.4.6", "PKCE", "code_challenge", "S256".

Node 07 — Arbiter Selector:
Territory: oauth-oidc → ASVS-V10-OAuth arbiter selected.

Node 08 — Arbiter Unit (ASVS-V10-OAuth):
V10.4.6: PASS — S256 code_challenge present, code_verifier validated at exchange.
V10.2.6: REPROCESS — state parameter MUST be present. PKCE satisfies V10.4.6 only;
it does not satisfy V10.2.6. Independent requirements, different attack vectors.
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
        {"requirement": "V10.2.6", "verdict": "REPROCESS",
         "gap": "state parameter absent from all authorization requests"}
      ],
      "territory_verdict": "REPROCESS"
    }
  ],
  "overall_verdict": "REPROCESS",
  "source_attribution": "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
}
```

**Reason:** Single oauth-oidc territory. Node 01 identifies OAuth signals only — no authentication, session, or process evidence present. Node 04 extracts both V10.4.6 and V10.2.6 as applicable sub-requirements and maps evidence to each independently. Node 08 produces PASS on V10.4.6 and REPROCESS on V10.2.6. PKCE satisfaction does not carry to state. Fintech context at Node 05 reinforces severity without changing verdict type.

**Phrasing-B:** Fintech identity provider compliance check. PKCE S256 implemented correctly on all authorization requests with code_verifier validation at exchange. State parameter was never added to the implementation. What does the pipeline assessment produce?

---

## Pair[CB4-3.4-250]

**A:** Internal platform security review. All authenticated routes are protected. The `/internal/admin/config` endpoint was added by the infrastructure team and exists outside the application's authentication middleware configuration — it returns environment configuration including database connection strings without any authentication or authorization check.

**B:**
```
Node 01 — Domain Splitter:
Input signals: unauthenticated endpoint, authentication bypass via alternate path,
sensitive data (database credentials) exposed, missing authentication check.
Territories: [T-cwe-authz]
CWE authorization weakness signals: alternate-path bypass, missing auth check.

Node 02 — Domain Translator:
T-cwe-authz → label: cwe-authz-normative
Source family: CWE Top 25 (authorization weaknesses).

Node 03 — Scheme Planner:
Plan type: linear (single territory).
Branch: cwe-authz → domain-processor → applier → result-translator → arbiter-selector
→ arbiter-unit → formatter.

Node 04 — Domain Processor:
Territory: cwe-authz
CWE patterns identified:
  CWE-288 (Authentication bypass via alternate path): /internal/admin/config bypasses
    authentication middleware via an alternate endpoint path. Sensitive data exposed.
  CWE-862 (Missing authorization check): even setting aside the bypass, no
    authorization check verifies the caller is permitted to access config data.
Evidence mapped:
  CWE-288: PRESENT — endpoint outside auth middleware, exposes DB credentials.
  CWE-862: PRESENT — no role or identity-based authorization check.
Severity indicator: CRITICAL — database credentials exposed without authentication.

Node 05 — Applier:
Context: internal platform. Even "internal-only" endpoints with database credentials
exposed without authentication constitute a CRITICAL finding — internal network
presence does not prevent exploitation by insider threat, compromised systems, or
SSRF from other services. CRITICAL classification confirmed.

Node 06 — Result Translator:
Vocabulary tags for cwe-authz territory: "authentication bypass", "alternate path",
"missing authorization check", "CWE-288", "CWE-862", "unauthenticated endpoint",
"database credentials".

Node 07 — Arbiter Selector:
Territory: cwe-authz → CWE-Authz arbiter selected.

Node 08 — Arbiter Unit (CWE-Authz):
CWE-288: REPROCESS — CRITICAL. /internal/admin/config bypasses authentication
middleware entirely via an alternate path not covered by middleware configuration.
Database credential exposure without authentication. Both weaknesses independently
present; both must be reported.
CWE-862: REPROCESS. No identity or role authorization check present.
Severity: CRITICAL.
Territory verdict: REPROCESS — CRITICAL.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["cwe-authz"],
  "findings": [
    {
      "territory": "cwe-authz",
      "arbiter": "CWE-Authz",
      "sub_findings": [
        {"cwe": "CWE-288", "verdict": "REPROCESS", "severity": "CRITICAL",
         "gap": "authentication bypass via alternate path; endpoint outside middleware"},
        {"cwe": "CWE-862", "verdict": "REPROCESS",
         "gap": "no authorization check on config endpoint"}
      ],
      "territory_verdict": "REPROCESS — CRITICAL"
    }
  ],
  "overall_verdict": "REPROCESS — CRITICAL",
  "source_attribution": "CWE Top 25 2023 — public domain (MITRE)"
}
```

**Reason:** Single cwe-authz territory. Node 01 identifies CWE-288 (bypass via alternate path) and CWE-862 (missing authz check) signals. Node 04 maps both CWE patterns independently. Node 08 reports both patterns with separate CWE IDs — the arbiter evaluates all CWE patterns present, not just the most severe. Database credential exposure without authentication warrants CRITICAL classification.

**Phrasing-B:** Platform review finding: infrastructure team added /internal/admin/config outside the normal deployment process; it's not in the auth middleware configuration. It returns DB connection strings and environment variables without any auth check. How does the pipeline classify this?

---

## Pair[CB4-3.4-251]

**A:** API gateway security review for a healthcare platform. The gateway logs decoded JWT payloads at INFO level including sub, email, and roles. SAST runs on every PR (PW.4 satisfied). No system-level threat model exists — security is tracked through per-sprint security tickets in Jira.

**B:**
```
Node 01 — Domain Splitter:
Input signals: JWT claims in application logs (ASVS authorization signal), SAST
running (SSDF process evidence), no threat model (SSDF process gap), healthcare
context.
Territories: [T-asvs-authorization, T-ssdf-process]
Two territories: authorization control and process evidence.

Node 02 — Domain Translator:
T-asvs-authorization → label: asvs-authorization-normative
T-ssdf-process → label: ssdf-process-normative
Source families: ASVS V8 (Authorization), SSDF PW (produce secure software).

Node 03 — Scheme Planner:
Plan type: parallel (two independent territories, no inter-territory dependency).
Branch A: asvs-authorization → domain-processor-A → arbiter-unit-A
Branch B: ssdf-process → domain-processor-B → arbiter-unit-B
Sync at: result-translator → formatter.

Node 04 — Domain Processor (parallel):
Territory A (asvs-authorization):
  V8.2.2: authentication state data and PII MUST NOT appear in logs.
  Evidence: JWT claims (sub, email, roles) logged at INFO level. GAP: V8.2.2 violated.

Territory B (ssdf-process):
  PW.4: SAST runs on every PR, configured for auth/authz patterns. SATISFIED.
  PW.2: system-level threat model artefact required.
  Evidence: Jira security tickets per sprint. NOT A THREAT MODEL — Jira sprint tickets
  are implementation-level issue tracking (PW.8 domain). GAP: PW.2 not satisfied.

Node 05 — Applier:
Healthcare context: V8.2.2 REPROCESS severity is elevated — healthcare PII in logs
has HIPAA implications in addition to ASVS requirements. PW.2 REPROCESS: lack of
threat model in a healthcare system with PHI (Protected Health Information) is
particularly high risk. Context does not change verdict type; it informs remediation
priority.

Node 06 — Result Translator:
Territory A tags: "JWT claims", "application logs", "INFO level", "V8.2.2", "PII",
  "authentication state", "sub", "email", "roles".
Territory B tags: "threat model", "SAST", "PW.4", "PW.2", "sprint tickets",
  "system-level", "artefact".

Node 07 — Arbiter Selector:
Territory A: asvs-authorization → ASVS-V8-AuthZ arbiter.
Territory B: ssdf-process → SSDF-PW arbiter.

Node 08 — Arbiter Units (parallel):
ASVS-V8-AuthZ: V8.2.2 REPROCESS. JWT claims logged at INFO level. PII and auth
state in logs. SAST clearance does not certify log content compliance.
SSDF-PW: PW.4 PASS (SAST running). PW.2 REPROCESS (Jira sprint tickets ≠ threat
model artefact; PW.8 domain activity does not satisfy PW.2).

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["asvs-authorization", "ssdf-process"],
  "findings": [
    {
      "territory": "asvs-authorization",
      "arbiter": "ASVS-V8-AuthZ",
      "sub_findings": [
        {"requirement": "V8.2.2", "verdict": "REPROCESS",
         "gap": "JWT claims (sub, email, roles) logged at INFO level"}
      ],
      "territory_verdict": "REPROCESS"
    },
    {
      "territory": "ssdf-process",
      "arbiter": "SSDF-PW",
      "sub_findings": [
        {"practice": "PW.4", "verdict": "PASS"},
        {"practice": "PW.2", "verdict": "REPROCESS",
         "gap": "no threat model artefact; Jira sprint tickets are PW.8 not PW.2"}
      ],
      "territory_verdict": "REPROCESS"
    }
  ],
  "overall_verdict": "REPROCESS",
  "source_attribution": [
    "OWASP ASVS 5.0.0 — CC BY-SA 4.0",
    "NIST SP 800-218 SSDF v1.1 — public domain"
  ]
}
```

**Reason:** Two territories processed in parallel. PW.4 PASS and PW.2 REPROCESS are simultaneously valid in the ssdf-process territory — SAST running certifies the practice; it does not certify threat model existence. V8.2.2 is a separate requirement from SAST. Healthcare context at Node 05 amplifies remediation priority but does not change verdict classification. PW.4 PASS does not offset PW.2 or V8.2.2 REPROCESS in the overall verdict.

**Phrasing-B:** Healthcare API gateway review: SAST runs on every PR and shows clean, JWT payloads are logged at INFO for audit purposes, and we track security work through sprint Jira tickets rather than a formal threat model. Show me the full pipeline trace.

---

## Pair[CB4-3.4-252]

**A:** JWT verification service deep assessment. The service accepts RS256 and HS256 signed tokens. RS256 public key is published at `/.well-known/jwks.json`. The validation library accepts the RS256 public key as an HMAC-SHA256 secret when alg=HS256 — no algorithm-key binding constraint is configured.

**B:**
```
Node 01 — Domain Splitter:
Input signals: JWT token verification, RS256 and HS256 accepted, public key at JWKS,
no algorithm constraint, algorithm confusion attack vector present.
Territories: [T-rfc-jwt, T-asvs-token]
RFC normative signal (algorithm validation MUST per RFC 8725) and ASVS token
verification signal (V9.2.1) both present.

Node 02 — Domain Translator:
T-rfc-jwt → label: rfc-jwt-normative
T-asvs-token → label: asvs-token-normative
Source families: IETF RFC 7519/8725 (JWT), ASVS V9 (Self-contained Tokens).

Node 03 — Scheme Planner:
Plan type: parallel (two independent territories, same vulnerability, different
standards).
Branch A: rfc-jwt → domain-processor-A → arbiter-unit-A
Branch B: asvs-token → domain-processor-B → arbiter-unit-B
Sync at: result-translator → formatter.

Node 04 — Domain Processor (parallel):
Territory A (rfc-jwt):
  RFC 8725 Section 3.1: MUST validate alg header against configured acceptable set.
  RFC 8725 Section 2.1: alg confusion is an explicit attack category requiring
    mitigation.
  Evidence: library accepts public key as HMAC secret for HS256-header tokens.
  JWKS key is publicly accessible. GAP: algorithm confusion exploitable via public key.

Territory B (asvs-token):
  V9.2.1: token verification must validate all required claims including algorithm;
    tokens failing verification must be rejected.
  Evidence: HS256 token with public key as HMAC secret passes verification. GAP:
    verification bypass possible for any party with access to the public key.

Node 05 — Applier:
Context: JWT verification service. Algorithm confusion allowing token forgery via a
published public key means complete authentication bypass for any service relying on
this verifier. Both territories: CRITICAL severity confirmed.

Node 06 — Result Translator:
Territory A tags: "algorithm confusion", "HS256", "RS256", "public key as HMAC secret",
  "RFC 8725", "alg header", "algorithm validation", "JWKS".
Territory B tags: "V9.2.1", "token verification", "algorithm", "signature bypass",
  "alg confusion", "HMAC", "RSA public key".

Node 07 — Arbiter Selector:
Territory A: rfc-jwt → RFC-JWT arbiter.
Territory B: asvs-token → ASVS-V9-Token arbiter.

Node 08 — Arbiter Units (parallel):
RFC-JWT: RFC 8725 Section 3.1 REPROCESS — CRITICAL. Algorithm validation MUST
prevent accepting an RSA public key as HMAC secret under HS256 header. Public JWKS
key makes this exploitable by any external party.

ASVS-V9-Token: V9.2.1 REPROCESS — CRITICAL. Token verification accepts forged
tokens (HS256 signed with public key). Tokens failing proper algorithm-key binding
verification must be rejected.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["rfc-jwt", "asvs-token"],
  "findings": [
    {
      "territory": "rfc-jwt",
      "arbiter": "RFC-JWT",
      "sub_findings": [
        {"standard": "RFC 8725 §3.1", "verdict": "REPROCESS", "severity": "CRITICAL",
         "gap": "no algorithm-key binding constraint; HS256 accepts RSA public key as HMAC secret"}
      ],
      "territory_verdict": "REPROCESS — CRITICAL"
    },
    {
      "territory": "asvs-token",
      "arbiter": "ASVS-V9-Token",
      "sub_findings": [
        {"requirement": "V9.2.1", "verdict": "REPROCESS", "severity": "CRITICAL",
         "gap": "verification bypass via algorithm confusion using published public key"}
      ],
      "territory_verdict": "REPROCESS — CRITICAL"
    }
  ],
  "overall_verdict": "REPROCESS — CRITICAL",
  "source_attribution": [
    "IETF RFC 7519, RFC 8725 — IETF Trust provisions",
    "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
  ]
}
```

**Reason:** Algorithm confusion is an attack vector covered independently by RFC 8725 (algorithm validation MUST) and ASVS V9.2.1 (verification must reject improperly bound tokens). The same vulnerability produces REPROCESS CRITICAL in two separate territories from two separate standards. The published JWKS key makes the attack practical for any external party. Both findings must be reported; neither cancels the other.

**Phrasing-B:** JWT service accepting RS256 and HS256. Public key at JWKS. Library accepts the RSA public key as an HS256 HMAC secret without blocking that combination. Full pipeline trace showing both RFC 7519/8725 and ASVS V9 assessment.

---

## Pair[CB4-3.4-253]

**A:** OAuth 2.0 server assessment with team argument. State parameter is absent. Team response to the gap: "RFC 6749 Section 10.12 says state is RECOMMENDED, not required. Since the canonical OAuth specification uses RECOMMENDED language, we should not be penalized for an optional feature. Our security review accepted this."

**B:**
```
Node 01 — Domain Splitter:
Input signals: OAuth 2.0 flow, state parameter absent, RFC 6749 RECOMMENDED language
argument, ASVS V10 territory signal.
Territories: [T-oauth-oidc]
Single territory. Argument about RFC normative language does not create a separate
rfc-jwt territory — this is an ASVS V10 compliance assessment.

Node 02 — Domain Translator:
T-oauth-oidc → label: oauth-oidc-normative
Source family: ASVS V10 (OAuth and OIDC).

Node 03 — Scheme Planner:
Plan type: linear (single territory).
Note: RFC 6749 normative language argument is an arbiter-unit matter, not a
territory or plan modification. The argument is passed to Node 08 for evaluation.

Node 04 — Domain Processor:
Territory: oauth-oidc
V10.2.6: state parameter MUST be present. ASVS 5.0.0 normative language.
Evidence: state absent. GAP confirmed.
Argument noted: team cites RFC 6749 Section 10.12 RECOMMENDED language.
Domain processor passes argument as context to arbiter — does not adjudicate it.

Node 05 — Applier:
Context: standard OAuth server. The RFC vs. ASVS normative language conflict is
a standards comparison question — resolved at Node 08, not adjusted here.

Node 06 — Result Translator:
Tags: "state parameter", "V10.2.6", "RFC 6749", "RECOMMENDED", "MUST", "CSRF",
"authorization code flow", "oauth-oidc".

Node 07 — Arbiter Selector:
Territory: oauth-oidc → ASVS-V10-OAuth arbiter selected.

Node 08 — Arbiter Unit (ASVS-V10-OAuth):
V10.2.6: REPROCESS.
Argument rejected: "RFC 6749 RECOMMENDED language overrides ASVS MUST."

RFC 6749 and ASVS 5.0.0 are independent standards. Each has its own normative
requirements. ASVS V10.2.6 uses MUST language for the state parameter. RFC 6749
Section 10.12 using RECOMMENDED language means RFC 6749 recommends the practice;
it does not mean ASVS V10.2.6 adopts that normative level. Compliance with ASVS
V10.2.6 requires MUST-level satisfaction regardless of RFC 6749's language.

Argument rejected: "security review accepted the risk."
Risk acceptance does not modify a binary MUST requirement in ASVS.

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
        {"requirement": "V10.2.6", "verdict": "REPROCESS",
         "gap": "state parameter absent",
         "argument_rejected": "RFC 6749 RECOMMENDED language does not modify ASVS V10.2.6 MUST requirement"}
      ],
      "territory_verdict": "REPROCESS"
    }
  ],
  "overall_verdict": "REPROCESS",
  "source_attribution": "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
}
```

**Reason:** The RFC vs. ASVS normative language argument does not change the territory split at Node 01 or the plan structure at Node 03 — it is an arbiter-unit matter passed through as context. Node 08 receives the argument and rejects it explicitly: independent standards have independent normative requirements. ASVS V10.2.6 MUST is not reduced by RFC 6749 RECOMMENDED. Risk acceptance does not modify binary ASVS requirements.

**Phrasing-B:** OAuth server review where the team argues RFC 6749 makes state optional. They cite Section 10.12 and their security review sign-off. Show the full pipeline trace including how the argument is handled at each node.

---

## Pair[CB4-3.4-254]

**A:** Internal web application authorization review. Finding 1: `/internal/metrics` endpoint returns user session data and service configuration without any authentication check — outside auth middleware. Finding 2: admin route `/admin/config` is covered by auth middleware confirming session, but the authorization middleware config was not updated to include this route in the deny-by-default allow list.

**B:**
```
Node 01 — Domain Splitter:
Input signals: unauthenticated endpoint (CWE-288 signal), missing authorization
check on admin route (CWE-862 / V8.1.2 signals), deny-by-default omission.
Territories: [T-cwe-authz, T-asvs-authorization]

Node 02 — Domain Translator:
T-cwe-authz → cwe-authz-normative
T-asvs-authorization → asvs-authorization-normative
Source families: CWE Top 25, ASVS V8.

Node 03 — Scheme Planner:
Plan type: parallel (two territories).
Branch A: cwe-authz
Branch B: asvs-authorization
Sync at formatter.

Node 04 — Domain Processor (parallel):
Territory A (cwe-authz):
  CWE-288: /internal/metrics — authentication bypass via alternate path.
    Sensitive data (session IDs, service config) without auth.
  CWE-862: /admin/config — authentication present, but no authorization role check
    (not in authorization allow list).
  Both patterns present.

Territory B (asvs-authorization):
  V8.1.2 (deny-by-default): /admin/config not in authorization middleware allow list.
    Omissions from deny-by-default configuration are violations.
  Note: /internal/metrics also violates V8.1.2 but is more severe under CWE-288.
  Evidence: /admin/config accessible without role verification. GAP confirmed.

Node 05 — Applier:
Context: internal application. /internal/metrics CRITICAL due to session ID exposure.
/admin/config REPROCESS — any authenticated user can access admin configuration.
Context confirms CRITICAL for auth bypass finding.

Node 06 — Result Translator:
Territory A tags: "authentication bypass", "CWE-288", "CWE-862", "alternate path",
  "missing authorization check", "session IDs", "unauthenticated".
Territory B tags: "V8.1.2", "deny-by-default", "authorization middleware",
  "allow list", "omission", "admin config".

Node 07 — Arbiter Selector:
Territory A → CWE-Authz arbiter.
Territory B → ASVS-V8-AuthZ arbiter.

Node 08 — Arbiter Units (parallel):
CWE-Authz:
  CWE-288: REPROCESS — CRITICAL. /internal/metrics auth bypass, session ID exposure.
  CWE-862: REPROCESS. /admin/config — auth present, authorization absent.
  Territory verdict: REPROCESS — CRITICAL.

ASVS-V8-AuthZ:
  V8.1.2: REPROCESS. /admin/config omitted from deny-by-default allow list.
    Deny-by-default requires that no route is accessible unless explicitly permitted.
    Omissions are violations.
  Territory verdict: REPROCESS.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["cwe-authz", "asvs-authorization"],
  "findings": [
    {
      "territory": "cwe-authz",
      "arbiter": "CWE-Authz",
      "sub_findings": [
        {"cwe": "CWE-288", "verdict": "REPROCESS", "severity": "CRITICAL",
         "endpoint": "/internal/metrics"},
        {"cwe": "CWE-862", "verdict": "REPROCESS",
         "endpoint": "/admin/config"}
      ],
      "territory_verdict": "REPROCESS — CRITICAL"
    },
    {
      "territory": "asvs-authorization",
      "arbiter": "ASVS-V8-AuthZ",
      "sub_findings": [
        {"requirement": "V8.1.2", "verdict": "REPROCESS",
         "gap": "/admin/config not in deny-by-default allow list"}
      ],
      "territory_verdict": "REPROCESS"
    }
  ],
  "overall_verdict": "REPROCESS — CRITICAL",
  "source_attribution": [
    "CWE Top 25 2023 — public domain (MITRE)",
    "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
  ]
}
```

**Reason:** CWE-288 and CWE-862 go to the same cwe-authz arbiter because they are both CWE authorization weakness patterns — the arbiter evaluates all present patterns. V8.1.2 is a separate ASVS requirement in a separate territory. The same /admin/config endpoint produces CWE-862 (no authz check) and V8.1.2 (deny-by-default omission) as independent findings in separate territories. CRITICAL classification comes from CWE-288 on the metrics endpoint.

**Phrasing-B:** Two auth findings in internal app: /internal/metrics has no auth at all (outside middleware), and /admin/config has authentication but wasn't added to the authorization middleware allow list. Full pipeline trace for both findings.

---

## Pair[CB4-3.4-255]

**A:** Identity platform compliance review. Two separate findings: (1) OAuth authorization server has no state parameter in any authorization request. (2) The system-level threat model was produced 22 months ago; the team states "security is stable, no material changes to the architecture since then."

**B:**
```
Node 01 — Domain Splitter:
Input signals: OAuth flow (state absent) → T-oauth-oidc; threat model age (22 months,
no update) → T-ssdf-process.
Territories: [T-oauth-oidc, T-ssdf-process]
Two territories: OAuth normative requirements and SSDF process evidence.

Node 02 — Domain Translator:
T-oauth-oidc → oauth-oidc-normative
T-ssdf-process → ssdf-process-normative

Node 03 — Scheme Planner:
Plan type: parallel (two independent territories).

Node 04 — Domain Processor (parallel):
Territory oauth-oidc: V10.2.6 requires state parameter. Evidence: absent. GAP.
Territory ssdf-process: PW.2 requires current threat model.
  22 months > 12-month threshold. "No material changes" argument — passed to arbiter.
  GAP: threat model not current under PW.2 time threshold.

Node 05 — Applier:
Context: identity platform. OAuth CSRF via missing state is high risk for an identity
provider — authorization response hijacking could compromise account sessions. PW.2
gap on an identity platform means authentication architecture and key management are
not analyzed in a current threat model. Both severity assessments confirm REPROCESS.

Node 06 — Result Translator:
oauth-oidc tags: "state parameter", "V10.2.6", "CSRF", "authorization code flow".
ssdf-process tags: "threat model", "PW.2", "22 months", "current", "artefact".

Node 07 — Arbiter Selector:
oauth-oidc → ASVS-V10-OAuth.
ssdf-process → SSDF-PW.

Node 08 — Arbiter Units (parallel):
ASVS-V10-OAuth: V10.2.6 REPROCESS. State parameter absent. Standard MUST.

SSDF-PW: PW.2 REPROCESS.
22 months exceeds the 12-month threshold for threat model currency.
Argument rejected: "no material changes to architecture."
(1) PW.2 does not provide an exception for unchanged architectures — threat actor
    capabilities, vulnerability landscape, and operational configuration evolve even
    when internal architecture does not change.
(2) "No material changes" is a self-assessment without a defined standard — PW.2
    requires a time-bounded artefact currency.
(3) Even if architecture is unchanged, the threat model must be reviewed periodically
    to confirm it remains accurate; that review and confirmation should be documented.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["oauth-oidc", "ssdf-process"],
  "findings": [
    {
      "territory": "oauth-oidc",
      "arbiter": "ASVS-V10-OAuth",
      "sub_findings": [
        {"requirement": "V10.2.6", "verdict": "REPROCESS",
         "gap": "state parameter absent from all authorization requests"}
      ],
      "territory_verdict": "REPROCESS"
    },
    {
      "territory": "ssdf-process",
      "arbiter": "SSDF-PW",
      "sub_findings": [
        {"practice": "PW.2", "verdict": "REPROCESS",
         "gap": "threat model 22 months old; argument 'no material changes' rejected"}
      ],
      "territory_verdict": "REPROCESS"
    }
  ],
  "overall_verdict": "REPROCESS",
  "source_attribution": [
    "OWASP ASVS 5.0.0 — CC BY-SA 4.0",
    "NIST SP 800-218 SSDF v1.1 — public domain"
  ]
}
```

**Reason:** Parallel two-territory trace. The "no material changes" argument for PW.2 is rejected at Node 08 because: (1) PW.2 does not have an exception for stable architectures, (2) threat actor capabilities and vulnerability landscape evolve independently, and (3) a periodic review confirming currency should itself be documented as a PW.2 activity. Both territories produce REPROCESS independently.

**Phrasing-B:** Identity platform with two issues: state parameter not implemented in OAuth flow, and threat model is 22 months old (team says architecture hasn't changed). Show the full 9-node trace and how the "no changes" argument is handled.

---

## Pair[CB4-3.4-256]

**A:** Comprehensive security review for a multi-service SaaS platform. Evidence provided covers all new security domains: OAuth authorization requests include state parameter (random nonce) and PKCE S256. No alternate unauthenticated endpoints exist — all routes are covered by authentication middleware. No PII or JWT claims appear in application logs. Container images are signed and provenance-verified at deployment. Threat model produced 8 months ago, covers all current services including OAuth server and payment module. SAST runs on every PR. No CWE-862, CWE-288, or CWE-352 patterns identified in endpoint authorization reviews.

**B:**
```
Node 01 — Domain Splitter:
Input signals: OAuth flow (state+PKCE present), no auth bypass endpoints (CWE-288
N/A), logs clean (V8.2.2 signal), signed artifacts (PW.5 signal), current threat
model (PW.2 signal), SAST present (PW.4 signal), no CWE authorization weaknesses.
Territories: [T-oauth-oidc, T-cwe-authz, T-asvs-authorization, T-ssdf-process,
              T-asvs-token]
All active security domains covered.

Node 02 — Domain Translator:
T-oauth-oidc → oauth-oidc-normative
T-cwe-authz → cwe-authz-normative
T-asvs-authorization → asvs-authorization-normative
T-ssdf-process → ssdf-process-normative
T-asvs-token → asvs-token-normative

Node 03 — Scheme Planner:
Plan type: parallel (five independent territories, no inter-territory dependencies).
All five branches converge at formatter.

Node 04 — Domain Processor (parallel):
oauth-oidc: V10.2.6 state present (random nonce). V10.4.6 PKCE S256 present.
  No gaps identified.
cwe-authz: No CWE-288 patterns (all routes covered by middleware). No CWE-862
  patterns (authorization checks present on all protected endpoints). No CWE-352
  patterns noted. No gaps identified.
asvs-authorization: V8.2.2 — no PII or JWT claims in logs confirmed. V8.1.2 —
  deny-by-default covers all routes (audit complete). No gaps identified.
ssdf-process: PW.2 — threat model 8 months old, covers all current services.
  PW.4 — SAST on every PR. PW.5 — container images signed, provenance verified.
  No gaps identified.
asvs-token: No URL token delivery noted. Token transport not flagged. No gaps.

Node 05 — Applier:
Context: multi-service SaaS. Comprehensive evidence across all new domains.
No context factor changes any verdict from expected PASS.

Node 06 — Result Translator:
All territories: positive compliance vocabulary per territory. No gap vocabulary
to translate.

Node 07 — Arbiter Selector:
oauth-oidc → ASVS-V10-OAuth
cwe-authz → CWE-Authz
asvs-authorization → ASVS-V8-AuthZ
ssdf-process → SSDF-PW
asvs-token → ASVS-V9-Token

Node 08 — Arbiter Units (all parallel):
ASVS-V10-OAuth: V10.2.6 PASS (state present). V10.4.6 PASS (PKCE S256). PASS.
CWE-Authz: No CWE-288 patterns. No CWE-862 patterns. No CWE-352 patterns. PASS.
ASVS-V8-AuthZ: V8.2.2 PASS (no PII/auth state in logs). V8.1.2 PASS (deny-by-default
  covers all routes). PASS.
SSDF-PW: PW.2 PASS (threat model current, 8 months, all services covered). PW.4 PASS
  (SAST on every PR). PW.5 PASS (signed artifacts, provenance verified). PASS.
ASVS-V9-Token: No token transport violations noted. PASS.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["oauth-oidc", "cwe-authz", "asvs-authorization",
                  "ssdf-process", "asvs-token"],
  "findings": [
    {"territory": "oauth-oidc", "arbiter": "ASVS-V10-OAuth",
     "territory_verdict": "PASS"},
    {"territory": "cwe-authz", "arbiter": "CWE-Authz",
     "territory_verdict": "PASS"},
    {"territory": "asvs-authorization", "arbiter": "ASVS-V8-AuthZ",
     "territory_verdict": "PASS"},
    {"territory": "ssdf-process", "arbiter": "SSDF-PW",
     "territory_verdict": "PASS"},
    {"territory": "asvs-token", "arbiter": "ASVS-V9-Token",
     "territory_verdict": "PASS"}
  ],
  "overall_verdict": "PASS",
  "source_attribution": [
    "OWASP ASVS 5.0.0 — CC BY-SA 4.0",
    "CWE Top 25 2023 — public domain (MITRE)",
    "NIST SP 800-218 SSDF v1.1 — public domain",
    "IETF RFC 7519 — IETF Trust provisions"
  ]
}
```

**Reason:** Five-territory comprehensive PASS baseline for all new security domains introduced in Action 3. Establishes the positive evidence baseline: state+PKCE present (V10.2.6+V10.4.6), no authorization bypass patterns (CWE-288/862/352), no PII in logs and deny-by-default confirmed (V8.2.2+V8.1.2), current threat model with coverage, SAST and signed artifacts (PW.2+PW.4+PW.5). All five territories PASS → overall PASS.

**Phrasing-B:** Multi-service SaaS platform with comprehensive new-domain controls: state and PKCE on OAuth, all routes behind auth/authz middleware, clean logs, signed container images, 8-month threat model covering all services, SAST on every PR, no CWE authorization patterns in audit. Show the full 9-node trace and overall verdict.

---

## File Summary

| Pair ID | Description | Stratum | Territories | Verdict |
|---------|-------------|---------|-------------|---------|
| CB4-3.4-249 | V10.2.6 absent state — full trace, PKCE PASS shown | C | oauth-oidc | REPROCESS |
| CB4-3.4-250 | CWE-288 auth bypass — full trace, DB creds exposed | C | cwe-authz | REPROCESS CRITICAL |
| CB4-3.4-251 | V8.2.2 JWT logs + PW.2 absent + PW.4 PASS — full trace | C | asvs-authorization, ssdf-process | REPROCESS |
| CB4-3.4-252 | Algorithm confusion HS256+pubkey — RFC-JWT + V9.2.1 full trace | C | rfc-jwt, asvs-token | REPROCESS CRITICAL |
| CB4-3.4-253 | "RFC says RECOMMENDED" adversarial chain — argument rejected at N08 | C | oauth-oidc | REPROCESS |
| CB4-3.4-254 | CWE-288+CWE-862 unauth endpoint + V8.1.2 omission — full trace | C | cwe-authz, asvs-authorization | REPROCESS CRITICAL |
| CB4-3.4-255 | V10.2.6 absent + PW.2 22-month "no changes" argument — full trace | C | oauth-oidc, ssdf-process | REPROCESS |
| CB4-3.4-256 | All-new-domain 5-territory comprehensive PASS baseline | C | oauth-oidc, cwe-authz, asvs-authorization, ssdf-process, asvs-token | PASS |
