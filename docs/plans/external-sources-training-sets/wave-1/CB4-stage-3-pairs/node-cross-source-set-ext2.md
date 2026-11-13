# CB4 Stage 3 — Cross-Source Pair Set Extension 2
# Sequences CB4-3.4-231 through CB4-3.4-240
# Stratum: B/C — multi-source new domain combinations
# Instances: 20 (10 base pairs × 2 phrasings)
# Running total after this file: 480 instances

---

## Pair[CB4-3.4-231]

**A:** Authorization code flow review for an identity provider. PKCE is implemented correctly — code_challenge with S256 method and code_verifier validated at token exchange. However, the state parameter is not included in any authorization request. Separately, the team's most recent formal threat model document was produced before the OAuth server was added to the architecture; it is now 36 months old.

**B:**
```
Territory 1: oauth-oidc
Arbiter: ASVS-V10-OAuth

V10.4.6 (PKCE): PASS — code_challenge with S256 method present on all authorization
requests; code_verifier validated at token exchange. PKCE requirement satisfied.

V10.2.6 (state parameter): REPROCESS — state parameter MUST be present in all
authorization requests. State provides CSRF protection for the authorization response
binding, which is a separate concern from the code interception protection that PKCE
provides. PKCE satisfying V10.4.6 does not satisfy V10.2.6. Both sub-requirements
are independently mandatory.
Evidence gap: no state parameter included in authorization requests.

Territory 2: ssdf-process
Arbiter: SSDF-PW

PW.2 (threat model): REPROCESS — the current threat model is 36 months old and was
produced before the OAuth authorization server was introduced. The OAuth server
introduces new trust boundaries (authorization endpoint, token endpoint, client
registrations), new threat actors (authorization code interception, token theft,
client impersonation), and new attack surfaces not present in the original threat
model. A threat model that does not include these components does not reflect the
current system.
Evidence gap: no current threat model; existing artefact predates the OAuth server
and does not cover OAuth-specific threat surface.

Overall verdict: REPROCESS
Reason: Two independent MUST-level violations across separate standards. PKCE
presence satisfies V10.4.6 only. Implementation quality does not compensate for
absent process artefact.
```

**Reason:** PKCE (V10.4.6) and state (V10.2.6) are independent sub-requirements targeting different attack vectors — code interception vs. authorization response CSRF. A 36-month threat model predating the OAuth server fails PW.2's currency requirement because the entire OAuth attack surface is unmodeled. Neither finding is mitigated by the other.

**Phrasing-B:** We've implemented PKCE for our OAuth flow — S256 code challenges and verified code_verifier on every authorization request. The state parameter isn't included because PKCE already covers CSRF-style replay. Our threat model was written before we stood up the OAuth server; we haven't had time to update it since.

---

## Pair[CB4-3.4-232]

**A:** Internal web application review. All main application routes require authentication and role-based access control. The `/internal/metrics` endpoint returns active user list with session IDs, user counts by role, and internal service configuration. This endpoint has no authentication or authorization check — it was added by the monitoring team for operational visibility and was not included in the authorization middleware configuration. The application uses deny-by-default: all routes are blocked by default, with explicit allow entries per role. The metrics endpoint was omitted.

**B:**
```
Territory 1: cwe-authz
Arbiter: CWE-Authz

CWE-288 (Authentication bypass via alternate path): REPROCESS — CRITICAL
The /internal/metrics endpoint provides access to sensitive data (active user list
with session IDs, role distribution, internal configuration) without any
authentication check. The endpoint bypasses the application's authentication layer
entirely via a path not covered by auth middleware. Authentication on all other
routes does not protect alternate paths that circumvent the authentication layer.

CWE-862 (Missing authorization check): REPROCESS
Even if authentication were added to this endpoint, no authorization check verifies
that the requesting identity is permitted to access operational metrics. Both
the authentication absence (CWE-288) and authorization absence (CWE-862) are
independently present on the same endpoint.

Severity: CRITICAL — unauthenticated access to user session identifiers and internal
service configuration via an alternate path.

Territory 2: asvs-authorization
Arbiter: ASVS-V8-AuthZ

V8.1.2 (deny-by-default): REPROCESS — the /internal/metrics endpoint is not present
in the authorization middleware configuration. An endpoint not in the deny-by-default
allow list that is nonetheless reachable violates the deny-by-default principle.
The intention to use deny-by-default does not produce a deny-by-default outcome
when routes are omitted from the configuration. Omissions are violations.
Evidence gap: /internal/metrics not covered by authorization middleware; reachable
without authorization check.

Overall verdict: REPROCESS — CRITICAL
Reason: CWE-288 (auth bypass) and CWE-862 (missing authz check) present on the
same endpoint. ASVS V8.1.2 violated by the same omission. CRITICAL due to
unauthenticated access to session identifiers and configuration data.
```

**Reason:** CWE-288 and CWE-862 are both cwe-authz authorization weaknesses evaluated by the same arbiter — the arbiter must report all CWE patterns present, not just one. V8.1.2 deny-by-default is a separate ASVS requirement in a separate territory; the same omission that causes CWE-288/862 also violates V8.1.2. CRITICAL severity is appropriate because session identifiers are exposed without authentication.

**Phrasing-B:** Our authorization system uses deny-by-default — every route needs to be on the explicit allow list per role. The /internal/metrics endpoint was set up outside the normal process by the ops team and wasn't added to the allow list. It's only accessible from internal IPs. Does the IP restriction compensate for the missing allow-list entry?

---

## Pair[CB4-3.4-233]

**A:** API gateway security review. The team runs automated SAST (static analysis) on every PR merge and the latest scan shows zero high-severity findings. Application gateway logs include JWT access token payloads decoded at ingress for routing purposes — sub, email, roles, and org_id are written at DEBUG level. The security program tracks security concerns through Jira tickets per feature team; there is no system-level threat model document.

**B:**
```
Territory 1: asvs-authorization
Arbiter: ASVS-V8-AuthZ

V8.2.2: REPROCESS — JWT access token claims including sub, email, roles, and org_id
constitute authentication and authorization state data. These MUST NOT appear in
application logs at any log level. Writing decoded JWT claims at DEBUG level violates
V8.2.2 regardless of whether DEBUG logs are filtered in production. The requirement
governs what data is written to logs, not how log output is filtered post-write.
Evidence gap: JWT claims (sub, email, roles, org_id) logged at DEBUG level by API
gateway middleware.
Note: SAST scan clearing with zero findings does not satisfy V8.2.2. Static analysis
tools detect code pattern vulnerabilities; they do not evaluate log output content
for sensitive data classification compliance.

Territory 2: ssdf-process
Arbiter: SSDF-PW

PW.2 (threat model): REPROCESS — Jira tickets per feature team are
implementation-level security issue tracking. This is PW.8 domain activity
(security review of individual features). PW.2 requires a system-level threat
model artefact: assets enumerated, trust boundaries drawn, threat actors
identified, and attack surface analyzed at the system level. Per-feature security
Jira tickets do not constitute a PW.2-compliant threat model.
Evidence gap: no system-level threat model artefact; feature-level Jira security
tickets are not a substitute.

PW.4 (SAST): PASS — automated static analysis runs on every PR merge, integrated
into CI/CD pipeline. Process requirement satisfied.

Overall verdict: REPROCESS
Reason: V8.2.2 REPROCESS for JWT claims in logs. PW.2 REPROCESS for absent threat
model. PW.4 PASS (SAST running). SAST clearance does not satisfy V8.2.2 or PW.2 —
these are different requirement types with different evidence standards.
```

**Reason:** SAST passing (PW.4) certifies that static analysis was performed, not that all runtime-observable violations are absent. V8.2.2 is a runtime content requirement that SAST typically cannot evaluate. Per-feature Jira tickets are PW.8 activity (feature-level security review), not PW.2 (system-level threat model). Three independent findings: two REPROCESS, one PASS.

**Phrasing-B:** SAST runs on every PR and it's clean — no high-severity findings. We log JWT claims at DEBUG level for routing, but that level is stripped in production. We don't have a formal threat model document; security concerns are tracked in feature-level Jira tickets by each team. The SAST pass should cover the security baseline, right?

---

## Pair[CB4-3.4-234]

**A:** JWT verification service review. The service accepts tokens signed with RS256 or HS256. The RS256 public key is published at `/.well-known/jwks.json`. The JWT validation library accepts the configured RS256 public key as a valid HMAC-SHA256 secret when the token header specifies `"alg":"HS256"` — no runtime algorithm constraint prevents this combination. An attacker with knowledge of the public key can construct an HS256-signed token using the public key as the HMAC secret, and the library will accept it.

**B:**
```
Territory 1: rfc-jwt
Arbiter: RFC-JWT

RFC 7519 / RFC 8725 Section 3.1: REPROCESS — CRITICAL
Implementations MUST validate the alg header parameter against a configured set of
acceptable algorithms. RFC 8725 Section 2.1 prohibits accepting algorithm values
not explicitly expected; Section 3.1 requires that key-algorithm combinations be
validated before processing. Accepting an RS256 RSA public key as an HMAC-SHA256
secret when alg=HS256 is specified in the header violates the algorithm-key binding
requirement. The public key is publicly accessible at the JWKS endpoint — any party
can retrieve it and forge arbitrary tokens using this exploit path.
Finding: REPROCESS — CRITICAL. Signature bypass via algorithm confusion using a
publicly accessible key constitutes complete token integrity failure.

Territory 2: asvs-token
Arbiter: ASVS-V9-Token

V9.2.1: REPROCESS — CRITICAL
Token verification must validate all required claims including the algorithm and
verify the signature using a key appropriate for that algorithm. Accepting a public
RSA key as an HMAC secret when alg=HS256 is specified means that token verification
can be bypassed by any party with knowledge of the public key. V9.2.1 requires
that tokens which fail proper verification be rejected. The RS256 public key at
/.well-known/jwks.json is publicly accessible; this is not a theoretical attack path.
Finding: REPROCESS — CRITICAL. Algorithm-key mismatch allows signature bypass.

Overall verdict: REPROCESS — CRITICAL
Reason: Both RFC 7519/8725 and ASVS V9.2.1 independently require algorithm
validation. Algorithm confusion allowing token forgery via a publicly accessible
key is CRITICAL severity — complete authentication bypass for any service accepting
these tokens.
```

**Reason:** Algorithm confusion (HS256 with RSA public key as HMAC secret) is independently flagged by both RFC 8725 Section 3.1 (algorithm validation MUST) and ASVS V9.2.1 (verification must reject mismatched algorithm-key pairs). The JWKS endpoint makes the public key accessible to any party, making this a practical rather than theoretical attack. Both standards produce REPROCESS CRITICAL.

**Phrasing-B:** We support both RS256 for external clients and HS256 for internal services. The RS256 public key is at our JWKS endpoint. Our validation library accepts whatever key is configured as the HS256 secret — if someone configured the RS256 public key in the HS256 slot, the library would accept it. We haven't added a guard against that configuration. Is this an issue?

---

## Pair[CB4-3.4-235]

**A:** E-commerce platform authorization audit. Finding 1: `/admin/reports` verifies the requesting user has an active session (authenticated) but does not check admin role — any authenticated user can access admin financial reports. Finding 2: `/api/v1/checkout/confirm` was added via a database-routing code path that bypasses the main application authentication middleware; it processes payment confirmations using a `transaction_id` body parameter without requiring authentication.

**B:**
```
Territory: cwe-authz
Arbiter: CWE-Authz

CWE-862 (Missing authorization check): REPROCESS
/admin/reports — authentication is present (session verified) but authorization is
absent. The endpoint confirms the caller is logged in but does not verify that the
caller holds the admin role required to access financial report data. Authentication
and authorization are separate controls; a present authentication check does not
satisfy the authorization requirement. Any authenticated user can access admin
financial reports.

CWE-288 (Authentication bypass via alternate path): REPROCESS — CRITICAL
/api/v1/checkout/confirm — the checkout confirmation endpoint bypasses the main
application authentication middleware via an alternate database-routing code path.
Payment confirmations are processed using only a caller-supplied transaction_id,
with no authentication of the calling party. Any network-accessible party can submit
payment confirmation requests and alter financial transaction state without
authenticating.

Severity: CRITICAL — unauthenticated modification of financial transaction state
(CWE-288) and unauthorized access to financial reports (CWE-862) both present.
CWE-288 on a financial processing endpoint warrants CRITICAL classification
independently.

Overall verdict: REPROCESS — CRITICAL
```

**Reason:** CWE-288 and CWE-862 are both cwe-authz weaknesses; the single cwe-authz arbiter evaluates all CWE authorization weakness patterns present in the submission and must report each finding with its CWE ID. CWE-288 and CWE-862 are distinct weaknesses with distinct requirements — authentication bypass via alternate path vs. missing role check on an authenticated path. CRITICAL is appropriate because unauthenticated financial transaction processing is directly exploitable.

**Phrasing-B:** We found two auth issues in our audit. First, /admin/reports only checks that the user is logged in, not that they're an admin — so any user can reach it. Second, the checkout confirmation endpoint was wired through a DB trigger path that never went through the auth middleware setup. No auth check at all on that endpoint. How bad are these?

---

## Pair[CB4-3.4-236]

**A:** OAuth 2.0 authorization server review. PKCE is implemented with S256 method on all authorization requests and code_verifier is correctly validated at token exchange. The state parameter is absent from all authorization requests. The team's implementation notes claim the system is "fully protected against OAuth-related CSRF" because PKCE is present, and that state is redundant when PKCE is used.

**B:**
```
Territory: oauth-oidc
Arbiter: ASVS-V10-OAuth

V10.4.6 (PKCE): PASS — code_challenge with S256 method present on all authorization
requests; code_verifier validated at token exchange. PKCE sub-requirement satisfied.

V10.2.6 (state parameter): REPROCESS — state parameter MUST be present in
authorization requests. ASVS 5.0.0 V10.2.6 is an independent MUST-level requirement.
Argument rejected: "PKCE provides equivalent CSRF protection."
PKCE (V10.4.6) addresses authorization code interception and binding the code to the
client that initiated the request. State (V10.2.6) addresses cross-site request
forgery of the authorization response — an attacker triggering the authorization
flow from a different site and capturing the resulting code. These are different
attack vectors at different stages of the flow. RFC 6749 Section 10.12 uses
RECOMMENDED language for state; ASVS V10.2.6 uses MUST. Both requirements must
be independently satisfied. V10.4.6 PASS does not carry over to V10.2.6.
Evidence gap: state parameter absent from all authorization requests.

Overall verdict: REPROCESS
Reason: V10.2.6 violation. V10.4.6 PASS does not satisfy V10.2.6. Two sub-requirements
within the same territory evaluated independently.
```

**Reason:** Within the oauth-oidc territory, V10.4.6 (PKCE) and V10.2.6 (state) are independent sub-requirements with independent evidence requirements. Satisfying one does not satisfy the other. The "PKCE covers CSRF" argument conflates the attack vectors — PKCE addresses code interception, state addresses authorization response CSRF. RFC 6749 using RECOMMENDED language does not override ASVS V10.2.6 using MUST language.

**Phrasing-B:** Our OAuth implementation is PKCE-compliant — S256 challenges on every request, verified at exchange. We didn't implement state because our security review concluded that PKCE already provides CSRF-level protection by binding the code to the initiating client. Is state still mandatory when full PKCE is in place?

---

## Pair[CB4-3.4-237]

**A:** Microservices infrastructure review with two separate findings. First: three legacy service-to-service endpoints transmit JWT tokens as URL query parameters; these endpoints cannot be refactored until a planned migration next quarter. Second: the CI/CD pipeline builds and deploys container images without signing or provenance verification — images are pulled from an internal registry without signature checks at deployment.

**B:**
```
Territory 1: asvs-token
Arbiter: ASVS-V9-Token

V9.3.1: REPROCESS — tokens MUST NOT be transmitted via URL parameters. URL query
parameters appear in server access logs, browser history, HTTP Referer headers, and
load balancer logs, creating multiple channels through which token values can be
unintentionally disclosed to parties who should not have access. Migration timeline
does not suspend a MUST-level requirement. The planned migration does not alter the
current state of non-compliance.
Evidence gap: JWT tokens transmitted in URL query parameters on three service-to-service
endpoints.

Territory 2: ssdf-process
Arbiter: SSDF-PW

PW.5 (signed artifacts): CONCERN — the CI/CD pipeline builds container images without
applying artifact signatures or verifying image provenance before deployment. Unsigned
container images do not provide protection against supply chain substitution between
build and deployment. SSDF PW.5 uses normative "should" language for artifact signing
practices; this is a significant operational gap but does not constitute a MUST-level
process requirement violation.
Evidence gap: container images deployed without signing or provenance verification
at deployment.

Overall verdict: REPROCESS
Reason: V9.3.1 REPROCESS overrides PW.5 CONCERN. REPROCESS precedence applies
to the overall verdict. The CONCERN finding for PW.5 is still reported and requires
a remediation plan.
```

**Reason:** V9.3.1 is a MUST-level token transport requirement that cannot be deferred by migration plans. PW.5 uses "should" normative language in SSDF and therefore produces CONCERN rather than REPROCESS — it is a significant gap but not a MUST-level violation. REPROCESS takes precedence over CONCERN in the overall verdict.

**Phrasing-B:** Two infrastructure issues came up. The legacy internal APIs pass JWT tokens in query strings — we know this is bad but the migration is scheduled for next quarter. Also, we haven't set up image signing yet; the plan is to add it in Q3. Both are on the roadmap. Is the URL token issue a hard block versus the signing issue?

---

## Pair[CB4-3.4-238]

**A:** API platform review. JWT access tokens issued by the platform include OIDC standard profile claims in the payload: email, phone_number, address, given_name, family_name. The platform's JWT implementation has been assessed as RFC 7519 compliant. API gateway middleware decodes and logs all JWT payload fields at INFO level for audit trail purposes, including all profile claim values.

**B:**
```
Territory 1: asvs-authorization
Arbiter: ASVS-V8-AuthZ

V8.2.2: REPROCESS — personal identifiable information MUST NOT appear in application
logs. JWT payload claims email, phone_number, address, given_name, and family_name
are personal identifiable information. These MUST NOT be written to application logs
at any log level. Logging for audit trail purposes does not exempt PII from V8.2.2.
An audit trail of authentication events can be maintained without including the PII
values themselves (e.g., logging a user identifier or a pseudonymous reference).
Evidence gap: PII profile claims logged at INFO level by API gateway middleware.

Territory 2: rfc-jwt
Arbiter: RFC-JWT

RFC 7519 conformance: PASS — RFC 7519 Section 4.1 defines registered claim names
as optional. Section 4.3 defines public claim names that may include profile claims
from the IANA registry. Including email, phone_number, address, given_name, and
family_name in the JWT payload is permitted under RFC 7519; there is no MUST-level
prohibition on including PII claims. The scope of RFC 7519 is token structure and
processing — it does not govern how receiving services handle token contents in
application logs.
RFC conformance assessment: PASS.

Overall verdict: REPROCESS
Reason: V8.2.2 REPROCESS for PII in application logs. RFC-JWT PASS for token
structure. REPROCESS overrides PASS in overall verdict. RFC 7519 conformance on
token structure does not satisfy ASVS V8.2.2 on log content — different standards
with different scopes.
```

**Reason:** RFC 7519 does not prohibit including OIDC profile claims in JWT payloads — this is within spec, and RFC-JWT produces PASS. V8.2.2 is a separate ASVS requirement governing what data must not appear in application logs, producing REPROCESS. The standards have different scopes: RFC 7519 governs token structure; ASVS V8.2.2 governs log hygiene. A PASS in one territory does not cancel a REPROCESS in another.

**Phrasing-B:** We include standard OIDC profile claims (email, phone_number, given_name, family_name, address) in our JWT access tokens — this is RFC 7519 / OIDC compliant. The API gateway logs decoded JWT payloads at INFO level for the audit trail, including those profile fields. Our RFC conformance audit passed. Is there still a finding given the RFC compliance?

---

## Pair[CB4-3.4-239]

**A:** SaaS platform security review. Static analysis (SSDF PW.4) runs on every PR configured to detect authorization issues, with zero high-severity findings on the latest scan. The mobile backend has a `/sync/bulk` endpoint that reads from a shared cache using the key pattern `user:{user_id}:sync_queue`, where `user_id` is taken from the request body parameter. The authenticated user's identity from the JWT sub claim is used to verify login state but is not used to bound the cache key — a logged-in user can retrieve any other user's sync queue by supplying a different `user_id` value.

**B:**
```
Territory 1: cwe-authz
Arbiter: CWE-Authz

CWE-862 (Missing authorization check): REPROCESS
The /sync/bulk endpoint authenticates the caller (JWT verified) but does not
authorize the data access — the cache key user:{user_id}:sync_queue is derived from
a caller-supplied request body parameter, not from the authenticated user's identity.
Any authenticated user can access any other user's sync queue by modifying the
user_id body parameter. This is a horizontal privilege escalation via IDOR (Insecure
Direct Object Reference) pattern: the authorization check that the requested user_id
matches the authenticated sub claim is absent.
Severity: HIGH — cross-user data access via IDOR on a data synchronization endpoint.
Note: SAST clearance does not certify absence of CWE-862. SAST detects code pattern
vulnerabilities; IDOR authorization logic gaps require semantic analysis of
authorization intent that static analysis tools do not perform.

Territory 2: ssdf-process
Arbiter: SSDF-PW

PW.4 (SAST): PASS — automated static analysis integrated into CI/CD, runs on every
PR, configured for authentication and authorization pattern detection. The process
requirement is satisfied: automated analysis is performed.
Note: PW.4 evaluates whether static analysis is practiced, not whether all
vulnerabilities are detected. PW.4 PASS and CWE-862 REPROCESS are simultaneously
valid findings.

Overall verdict: REPROCESS
Reason: CWE-862 REPROCESS for IDOR on sync endpoint. SSDF PW.4 PASS. REPROCESS
overrides PASS. SAST passing does not certify authorization logic correctness.
```

**Reason:** SAST satisfaction (PW.4) is a process requirement — the practice was followed, but the tool's PASS does not certify absence of all weaknesses. CWE-862 IDOR is a logic-level authorization gap that SAST tools typically cannot detect because it requires understanding the relationship between the authenticated identity and the resource identifier. Both findings are valid simultaneously.

**Phrasing-B:** We have SAST running on every PR, configured for auth/authz patterns — zero high-severity findings. The bulk sync endpoint uses user_id from the request body as the cache key. We check the JWT to confirm the user is logged in, but the actual data fetch uses the body parameter. SAST didn't flag it. Is this a concern, given the SAST clearance?

---

## Pair[CB4-3.4-240]

**A:** Payment processing platform audit with three findings. (1) Server logs write full JWT access token strings at ERROR level when token validation fails — "helps with debugging validation issues." (2) A `/webhook/payment-notify` endpoint processes payment status callbacks; it validates a shared secret transmitted in the request body against a constant defined in application configuration — no authentication middleware is applied. (3) The threat model document is 14 months old and was written before the payment processing module was added to the system.

**B:**
```
Territory 1: asvs-authorization
Arbiter: ASVS-V8-AuthZ

V8.2.2: REPROCESS — full JWT access token strings constitute authentication state
data and MUST NOT appear in application logs. Error-level logging does not exempt
token data from V8.2.2. Debugging value of token strings in logs does not override
the requirement. An opaque correlation ID should be logged instead of the token
itself.
Evidence gap: full JWT token strings written to ERROR-level logs on validation failure.

Territory 2: cwe-authz
Arbiter: CWE-Authz

CWE-288 (Authentication bypass via alternate path): REPROCESS — CRITICAL
The /webhook/payment-notify endpoint processes financial transaction state updates
without authentication middleware. A shared secret in the request body validated
against a configuration constant is not an authentication mechanism — it is an
obscurity control providing no protection once the constant value is known via
source code access, traffic interception, or brute force. The endpoint bypasses
the application's authentication framework via an alternate processing path.
Processing payment state changes without authenticated caller identity creates
direct financial transaction manipulation risk.
Severity: CRITICAL — unauthenticated financial transaction state modification.

Territory 3: ssdf-process
Arbiter: SSDF-PW

PW.2 (threat model): REPROCESS — the threat model is 14 months old and was
produced before the payment processing module was added. The payment processing
feature introduces new assets (financial transaction records, payment processor
integrations), new trust boundaries (incoming payment callbacks from external
payment processors), and new threat actors (attackers targeting financial data
and transaction state). A threat model that does not cover these components does
not reflect the current system.
Evidence gap: threat model 14 months old, does not include payment processing
module or payment callback trust boundary.

Overall verdict: REPROCESS — CRITICAL
Reason: Three independent violations across three territories. CWE-288 on a payment
processing endpoint warrants CRITICAL classification. V8.2.2 and PW.2 are additional
REPROCESS findings. Overall severity CRITICAL due to unauthenticated financial
transaction processing.
```

**Reason:** Three distinct territories, three independent MUST-level violations. CWE-288 on a payment webhook without authentication is CRITICAL because unauthenticated financial transaction state modification has direct business impact. A configuration constant is not an authentication mechanism — it is obscurity, not identity verification. V8.2.2 applies to full token strings regardless of log level. PW.2 requires the threat model to cover the current system including new features with external trust boundaries.

**Phrasing-B:** Payment platform items: validation errors log the full JWT at ERROR level for debugging; the payment webhook uses a config constant for a shared secret check in the request body (not hardcoded, just in config); and the threat model is 14 months old but the core architecture hasn't changed, just added the payment module. How critical are these findings?

---

## File Summary

| Pair ID | Description | Stratum | Territories | Verdict |
|---------|-------------|---------|-------------|---------|
| CB4-3.4-231 | V10.2.6 absent state + PW.2 pre-OAuth threat model | C | oauth-oidc, ssdf-process | REPROCESS×2 |
| CB4-3.4-232 | CWE-288+CWE-862 unauth metrics endpoint + V8.1.2 omission | B | cwe-authz, asvs-authorization | REPROCESS CRITICAL |
| CB4-3.4-233 | V8.2.2 JWT claims in logs + PW.2 Jira≠threat model + PW.4 PASS | B | asvs-authorization, ssdf-process | REPROCESS×2, PW.4 PASS |
| CB4-3.4-234 | Algorithm confusion (HS256+RSA pubkey) × RFC-JWT + V9.2.1 | D | rfc-jwt, asvs-token | REPROCESS CRITICAL×2 |
| CB4-3.4-235 | CWE-862 admin reports + CWE-288 payment path bypass | A | cwe-authz (both patterns) | REPROCESS CRITICAL |
| CB4-3.4-236 | V10.4.6 PKCE PASS + V10.2.6 state REPROCESS (same territory) | B | oauth-oidc | REPROCESS |
| CB4-3.4-237 | V9.3.1 URL tokens REPROCESS + PW.5 unsigned images CONCERN | A | asvs-token, ssdf-process | REPROCESS |
| CB4-3.4-238 | V8.2.2 PII in logs REPROCESS + RFC-JWT profile claims PASS | B | asvs-authorization, rfc-jwt | REPROCESS |
| CB4-3.4-239 | CWE-862 IDOR sync endpoint + SSDF PW.4 PASS (SAST≠absence) | B | cwe-authz, ssdf-process | REPROCESS |
| CB4-3.4-240 | V8.2.2 JWT in logs + CWE-288 payment webhook + PW.2 stale | C | asvs-authorization, cwe-authz, ssdf-process | REPROCESS CRITICAL |
