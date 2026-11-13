# CB4 Stage 3 — Node Pipeline Chain Set Extension 16
# Sequences CB4-3.4-577 through CB4-3.4-584
# Stratum: C — full 9-node pipeline traces for ext22 patterns
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 1168 instances

---

## Pair[CB4-3.4-577]

**A:** Full pipeline trace — single token for all resource servers. Multi-RS deployment: `api.example.com`, `reports.example.com`, `admin.example.com`. Access tokens issued with `aud=["api.example.com", "reports.example.com", "admin.example.com"]` — all three RSes accept the same token. Team: "A single aud covering all our services means one token flow for users."

**B:**
```
N1 · Domain Splitter
  Input: multi-RS deployment, single token with broad aud covering all three RSes
  Domain signals: audience scope, token-confusion risk, resource server isolation
  Territories opened: T-oauth-oidc (primary), T-rfc-jwt (aud validation)
  Split: [audience-scope-breadth | rs-isolation | least-privilege-token]

N2 · Domain Translator
  audience-scope-breadth → aud includes all three RSes → token accepted at any RS
  rs-isolation → each RS should only accept tokens intended for it
  least-privilege-token → token should carry minimum necessary audience

N3 · Scheme Planner
  Primary scheme: RFC7519-aud-scope + RFC8707-resource-indicators
  Conflict check: aud is present and validated at each RS ✓; aud is too broad ✗
  Plan: assess aud scope; determine CONCERN vs REPROCESS; compare to per-RS pattern

N4 · Domain Processor
  Aud scope analysis:
    aud=["api.example.com", "reports.example.com", "admin.example.com"] → all three in aud
    Token validated at any of these RSes: aud check passes ✓
    Security implication:
      Token stolen from a low-privilege flow (reports.example.com) can be presented
        at admin.example.com — aud validation passes (admin is in the aud list) ✗
      Token audience does not restrict which resource server the token is valid at ✗
      RFC 8707 resource indicators: intended to enable per-RS aud restriction ✓ (not used here)
  Team argument: "one token flow = single aud"
    Aud present and validated ✓ (mechanism correct); aud too broad ✗ (scope design gap)
    → CONCERN (not REPROCESS — aud validation exists; scope of aud is the issue)

N5 · Applier
  Apply RFC7519-aud: CONCERN — aud validated but includes all RSes; no per-RS restriction
  Verdict: CONCERN

N6 · Result Translator
  Finding: aud validated but covers all RSes; token valid at any RS including admin
  Severity: CONCERN

N7 · Arbiter Selector: ASVS-V10-OAuth, RFC-JWT

N8 · Arbiter Units
  ASVS-V10-OAuth: aud present and validated ✓; aud includes all RSes ✗ — CONCERN
  RFC-JWT: RFC 7519 §4.1.3 aud validated ✓; scope too broad ✗ — CONCERN
  T-oauth-oidc verdict: CONCERN
  T-rfc-jwt verdict: CONCERN

N9 · Formatter
  Graduation: 0 REPROCESS, 1+ CONCERN → Overall CONCERN
  Remediation: use resource indicators (RFC 8707); issue per-RS tokens with aud restricted to target RS
Overall verdict: CONCERN
```

**Reason:** aud claim is present and validated at each RS (mechanism correct) but the aud list covers all three RSes — a token valid at `reports.example.com` is also valid at `admin.example.com`. CONCERN because validation exists; the scope of the aud is the design gap. Per-RS resource indicators would restrict each token to its intended RS.

**Phrasing-B:** Pipeline trace for single token with aud covering all three resource servers. Team: "One aud covering all services simplifies the token flow." Walk through all 9 nodes.

---

## Pair[CB4-3.4-578]

**A:** Full pipeline trace — PKCE code_verifier too short (41 characters). Client generates a `code_verifier` of 41 characters. RFC 7636 §4.1 minimum is 43 characters. Client uses S256 method. Team: "41 characters provides adequate entropy — the minimum of 43 is arbitrary."

**B:**
```
N1 · Domain Splitter
  Input: 41-char code_verifier, S256 method, "minimum is arbitrary" argument
  Domain signals: PKCE length requirement, entropy, RFC 7636 compliance
  Territories opened: T-oauth-oidc, T-rfc-oauth2
  Split: [verifier-length | entropy-assessment | minimum-requirement]

N2 · Domain Translator
  verifier-length → RFC 7636 §4.1: 43–128 characters required ✗ (41 < 43)
  entropy-assessment → 41 chars from unreserved set: adequate entropy but below spec minimum
  minimum-requirement → MUST requirement in RFC 7636 §4.1

N3 · Scheme Planner
  Primary scheme: RFC7636-verifier-format
  Conflict check: entropy is adequate; format requirement violated
  Plan: determine whether length violation is CONCERN vs REPROCESS

N4 · Domain Processor
  RFC 7636 §4.1:
    "code_verifier = 43*128unreserved" (43 to 128 characters inclusive)
    41 characters: below minimum by 2 characters ✗
  Entropy analysis:
    41 characters from 66-char unreserved set: ~247 bits → adequate entropy ✓
    The entropy gap argument: minimum 43 chars provides ~256 bits from same set ✓
    2 characters below minimum: entropy still adequate ✓ (entropy is not the issue)
  Compliance: RFC 7636 §4.1 minimum is a normative requirement ✗
    Some server implementations reject verifiers < 43 characters ✗
    Interoperability issue: may fail at some authorization servers ✗
  Team argument: "arbitrary minimum"
    RFC minimums are not arbitrary — set for interoperability and minimum security properties
    REJECTED

N5 · Applier
  Adversarial rejection: "minimum is arbitrary" → REJECTED
  Apply RFC7636-verifier-length: CONCERN
    Entropy adequate ✓; length below spec minimum ✗; potential interoperability failure ✗
    Same pattern as UUID verifier (36 chars): entropy adequate; length non-compliant → CONCERN
  Verdict: CONCERN

N6 · Result Translator
  Finding: code_verifier 41 chars — below RFC 7636 §4.1 minimum of 43; entropy adequate but
    non-compliant format; potential interoperability issues
  Severity: CONCERN (entropy not the issue; format compliance is)

N7 · Arbiter Selector: ASVS-V10-OAuth, RFC-OAuth2

N8 · Arbiter Units
  ASVS-V10-OAuth: PKCE S256 ✓; code_verifier length non-compliant ✗ — CONCERN
  RFC-OAuth2 (RFC 7636 §4.1): length 41 < 43 minimum ✗ — CONCERN
  T-oauth-oidc verdict: CONCERN
  T-rfc-oauth2 verdict: CONCERN

N9 · Formatter
  Graduation: 0 REPROCESS, 1+ CONCERN → Overall CONCERN
  Remediation: generate code_verifier with 43–128 unreserved characters using CSPRNG
Overall verdict: CONCERN
```

**Reason:** 41-character code_verifier is below the RFC 7636 §4.1 minimum of 43 characters. Entropy is adequate but the format is non-compliant — same category as the UUID verifier pattern (entropy OK, length not). "Minimum is arbitrary" is rejected. CONCERN.

**Phrasing-B:** Pipeline trace for PKCE code_verifier of 41 characters — 2 below RFC 7636 §4.1 minimum. Team: "The minimum is arbitrary — 41 chars has adequate entropy." Walk through all 9 nodes.

---

## Pair[CB4-3.4-579]

**A:** Full pipeline trace — JWT client assertion with jti reuse. Token endpoint: JWT client assertions (RFC 7523). Client sends JWT assertions for authentication. Server validates: signature, iss, aud, exp. jti: NOT checked for uniqueness — same jti can be reused across multiple token requests. Team: "JWT assertions are signed — reusing a jti is just an implementation optimization."

**B:**
```
N1 · Domain Splitter
  Input: JWT client assertions, jti not verified for uniqueness, "signed = replay safe" argument
  Domain signals: client assertion replay, jti uniqueness, RFC 7523 requirements
  Territories opened: T-oauth-oidc, T-rfc-jwt
  Split: [assertion-validation | jti-uniqueness | replay-scenario]

N2 · Domain Translator
  assertion-validation → RFC 7523 §3: signature + iss + aud + exp validated ✓
  jti-uniqueness → RFC 7523 §3: jti MUST be unique per assertion to prevent replay ✗
  replay-scenario → if attacker captures JWT assertion: reusable without jti check ✗

N3 · Scheme Planner
  Primary scheme: RFC7523-jti-uniqueness
  Conflict check: other validation steps correct; jti uniqueness absent
  Plan: verify jti requirement in RFC 7523; assess replay attack; verdict

N4 · Domain Processor
  RFC 7523 §3 requirements:
    Signature validated ✓; iss == client_id ✓; aud == token endpoint ✓; exp validated ✓
    jti: MUST be unique per assertion; used assertions MUST be tracked to prevent replay ✗
  Replay attack scenario:
    Attacker intercepts a valid JWT client assertion ✓
    Asserted JWT: still within exp window (e.g., 5-minute validity) ✓
    Attacker replays assertion to token endpoint ✓
    Server: signature ✓; iss ✓; aud ✓; exp ✓ (still valid); jti not checked ✗ → accepted ✓
    Attacker obtains access token in the client's name ✗
  Team argument: "signed = replay safe"
    Signature proves issuer; does NOT prevent replay within the validity window ✗
    jti is specifically for replay prevention; signature does not substitute ✗
    → REJECTED

N5 · Applier
  Adversarial rejection: "signed = replay safe" → REJECTED
  Apply RFC7523-jti-uniqueness: REPROCESS (MUST requirement; replay attack possible)
  Verdict: REPROCESS

N6 · Result Translator
  Finding: JWT client assertion jti not tracked for uniqueness; replay attack within exp window possible
  Severity: REPROCESS (MUST requirement absent)

N7 · Arbiter Selector: ASVS-V10-OAuth, RFC-JWT

N8 · Arbiter Units
  ASVS-V10-OAuth: client assertion jti uniqueness absent ✗ → REPROCESS
  RFC-JWT (RFC 7523 §3): jti MUST be unique + tracked ✗ → REPROCESS
  T-oauth-oidc verdict: REPROCESS
  T-rfc-jwt verdict: REPROCESS

N9 · Formatter
  Graduation: REPROCESS → Overall REPROCESS
  Remediation: track jti values seen within expiry window; reject jti that has been used before
Overall verdict: REPROCESS
```

**Reason:** RFC 7523 §3 requires jti to be unique and tracked to prevent replay. Without jti uniqueness checking, a captured client assertion can be replayed within its validity window. "Signed = replay safe" is rejected — signature proves origin but does not prevent replay; jti is the replay prevention mechanism. REPROCESS.

**Phrasing-B:** Pipeline trace for JWT client assertions where jti is not tracked for uniqueness. Team: "Assertions are signed — reusing jti is just an optimization." Walk through all 9 nodes.

---

## Pair[CB4-3.4-580]

**A:** Full pipeline trace — correct JWT client assertions. Token endpoint: JWT client assertions (RFC 7523). Validation: signature against registered JWKS; `iss` == `client_id`; `aud` == token endpoint URL; `exp` not expired; `jti` not previously seen within a 5-minute window (tracked in Redis with TTL = assertion exp). Short assertion validity: 3 minutes.

**B:**
```
N1 · Domain Splitter
  Input: JWT client assertions with complete validation including jti uniqueness (Redis-backed)
  Domain signals: client authentication, assertion replay protection, RFC 7523
  Territories opened: T-oauth-oidc, T-rfc-jwt, T-rfc-oauth2
  Split: [assertion-validation | jti-uniqueness-tracking | assertion-lifecycle]

N2 · Domain Translator
  assertion-validation → sig + iss + aud + exp all validated ✓
  jti-uniqueness-tracking → Redis stores seen jti values with TTL = exp time ✓
  assertion-lifecycle → 3-minute validity: short window reduces replay risk ✓

N3 · Scheme Planner → all PASS expected

N4 · Domain Processor
  Signature: validated against registered JWKS ✓
  iss == client_id ✓; aud == token endpoint URL ✓; exp validated ✓
  jti: Redis lookup → if seen: reject ✓; if new: store with TTL=exp ✓
  3-minute exp: short validity window limits replay opportunity ✓
  After exp: jti naturally evicted from Redis (TTL); no persistent storage needed ✓
  No gaps

N5 · Applier → PASS

N6 · Result Translator → PASS

N7 · Arbiter Selector: ASVS-V10-OAuth, RFC-JWT

N8 · Arbiter Units
  ASVS-V10-OAuth: JWT assertions with complete validation + jti ✓ — PASS
  RFC-JWT (RFC 7523 §3): all requirements met ✓ — PASS
  All territories: PASS

N9 · Formatter → Overall PASS
Overall verdict: PASS
```

**Reason:** JWT client assertions with complete validation (sig, iss, aud, exp) plus jti uniqueness tracking via Redis with TTL matching assertion expiry. Short 3-minute validity window. All RFC 7523 §3 requirements satisfied. PASS.

**Phrasing-B:** Pipeline trace for JWT client assertions with Redis-backed jti uniqueness tracking (TTL=exp) and 3-minute validity. Walk through all 9 nodes to confirm PASS.

---

## Pair[CB4-3.4-581]

**A:** Full pipeline trace — SBOM query delay on critical vulnerability. Organization: SBOM maintained in CycloneDX format. When a critical CVE is disclosed: manual process — security lead receives notification, manually queries SBOM tooling, reports findings. Assessment time: typically 48–72 hours for critical vulnerabilities. Team: "48 hours is a fast response compared to industry average — we have a SBOM."

**B:**
```
N1 · Domain Splitter
  Input: SBOM exists (CycloneDX), manual CVE assessment process, 48-72 hour assessment time
  Domain signals: SSDF PO.3.2, vulnerability response time, manual vs automated process
  Territories opened: T-ssdf-process
  Split: [sbom-existence | assessment-process | response-time]

N2 · Domain Translator
  sbom-existence → CycloneDX SBOM ✓ (PO.3.2 partially satisfied)
  assessment-process → manual lookup ✗ (not automated)
  response-time → 48-72 hours for critical CVE ✗ (should be hours, not days)

N3 · Scheme Planner
  Primary scheme: SSDF-PO3-2-rapid-assessment
  Conflict check: SBOM exists ✓; assessment is not rapid ✗
  Plan: assess whether SBOM + slow manual process satisfies PO.3.2 intent

N4 · Domain Processor
  SBOM: CycloneDX maintained ✓ — PASS for inventory existence
  Assessment process:
    PO.3.2 intent: enable RAPID identification of affected components ✓
    "Rapid" requires automation for critical vulnerabilities ✗
    48-72 hours: during this window the organization may be running vulnerable software
      in production without knowing it ✗
    Critical CVEs (CVSS ≥ 9.0): 48-72 hours is unacceptable for critical severity ✗
  Team argument: "48 hours is fast compared to industry average"
    "Industry average" does not define the security requirement ✗
    SSDF PO.3.2: requires rapid; 48-72 hours for critical severity is not rapid ✗
    → REJECTED

Finding level: CONCERN
  SBOM exists ✓ (not REPROCESS); assessment process is slow for critical CVEs ✗
  → CONCERN (SBOM present; operational gap in assessment speed)

N5 · Applier
  Apply SSDF-PO3-2: CONCERN — SBOM exists; rapid-assessment capability inadequate
  Verdict: CONCERN

N6 · Result Translator → CONCERN

N7 · Arbiter Selector: SSDF-PW

N8 · Arbiter Units
  SSDF-PW: PO.3.2 — SBOM present ✓; rapid assessment not implemented for critical CVEs ✗ — CONCERN
  T-ssdf-process verdict: CONCERN

N9 · Formatter
  Graduation: 0 REPROCESS, CONCERN → Overall CONCERN
  Remediation: automate CVE feed integration with SBOM; alert within 1-2 hours for CVSS ≥ 7
Overall verdict: CONCERN
```

**Reason:** SBOM exists (CycloneDX) which satisfies the inventory requirement. The gap is the manual assessment process taking 48-72 hours for critical CVEs. CONCERN — SBOM present (not REPROCESS); PO.3.2 rapid-assessment capability is inadequate. "48 hours is industry average" rejected as a benchmark for security requirements.

**Phrasing-B:** Pipeline trace for CycloneDX SBOM with 48-72 hour manual CVE assessment process. Team: "48 hours is fast compared to industry average." Walk through all 9 nodes.

---

## Pair[CB4-3.4-582]

**A:** Full pipeline trace — resource indicators plus scope minimization. Application: RFC 8707 resource indicators used. Client requests tokens with resource-specific scope. For `api.example.com`: token issued with `scope=read:data`, `aud=["api.example.com"]`. For `admin.example.com`: separate token with `scope=admin:users`, `aud=["admin.example.com"]`. M2M services also use per-operation minimum scope tokens. Resource servers enforce scope.

**B:**
```
N1 · Domain Splitter
  Input: resource indicators per-RS, per-RS aud, per-operation scope, M2M minimum scope
  Domain signals: audience isolation, scope minimization, least-privilege
  Territories opened: T-oauth-oidc, T-rfc-jwt, T-asvs-authorization
  Split: [resource-audience-binding | scope-minimization | rs-scope-enforcement]

N2 · Domain Translator
  resource-audience-binding → RFC 8707 + RFC 7519 aud: per-RS token ✓
  scope-minimization → V8.1.2: minimum scope per operation ✓
  rs-scope-enforcement → V8.1.2: RS enforces scope per endpoint ✓

N3 · Scheme Planner → all PASS expected

N4 · Domain Processor
  Resource indicators: per-RS aud binding ✓ — token for api.example.com not valid at admin ✓
  Scope: read:data for api; admin:users for admin — minimum necessary ✓
  M2M: per-operation minimum scope ✓
  RS scope enforcement: per endpoint ✓
  No gaps

N5 · Applier → all PASS

N7-N8 · All territories PASS

N9 · Overall PASS
Overall verdict: PASS
```

**Reason:** Resource indicators with per-RS aud binding, minimum scope per resource type, M2M per-operation minimum scope, RS scope enforcement. Comprehensive audience and scope isolation. PASS.

**Phrasing-B:** Pipeline trace for resource indicators (per-RS aud + per-RS minimum scope) + M2M per-operation scope + RS enforcement. Walk through all 9 nodes to confirm PASS.

---

## Pair[CB4-3.4-583]

**A:** Full pipeline trace — combined: broad aud CONCERN plus PKCE code_verifier too long (200 chars) CONCERN. Application: tokens with aud covering all RSes. PKCE verifier: 200-character string (exceeds 128 maximum). All other controls correct. Team: "We have PKCE and aud — both security controls are in place."

**B:**
```
N1 · Domain Splitter
  Input: broad aud (all RSes), PKCE verifier 200 chars (exceeds max), "controls in place" argument
  Territories opened: T-oauth-oidc, T-rfc-jwt, T-rfc-oauth2
  Split: [aud-breadth | pkce-length | graduation]

N2 · Domain Translator
  aud-breadth → aud covers all RSes → CONCERN (not per-RS)
  pkce-length → 200 chars > 128 maximum → CONCERN (RFC 7636 §4.1)

N3 · Scheme Planner
  Two CONCERN candidates; graduation: 0 REPROCESS, N CONCERN → CONCERN

N4 · Domain Processor
  Broad aud: all RSes in aud → token valid at any RS ✗ → CONCERN
  PKCE verifier 200 chars: exceeds RFC 7636 §4.1 maximum 128 ✗ → CONCERN
    (server may reject or truncate; interoperability risk)
  Team argument: "controls in place" → controls exist; both have correctness gaps → CONCERN

N5 · Applier
  Both CONCERN
  Graduation: 0 REPROCESS, 2 CONCERN → CONCERN

N7-N9 · Overall CONCERN
Overall verdict: CONCERN
```

**Reason:** Broad aud (CONCERN) + PKCE verifier too long (CONCERN). Two CONCERNs = CONCERN overall. "Controls are in place" is correct that they exist; the gaps are in how they are configured. No REPROCESS.

**Phrasing-B:** Pipeline trace for broad aud covering all RSes plus PKCE verifier 200 chars (both CONCERN). What is the overall verdict?

---

## Pair[CB4-3.4-584]

**A:** Full pipeline trace — comprehensive ext22 PASS. All ext22 patterns correct: risk-tiered TTL (5/15/60 min); RFC 8707 resource indicators with per-RS aud; PKCE code_verifier within 43–128 char limits; CycloneDX SBOM auto-updated with 1-hour CVE query; JWT client assertions with full validation including jti; all prior controls correct.

**B:**
```
N1 · Domain Splitter
  Input: all ext22 patterns correct
  Territories opened: T-oauth-oidc, T-rfc-jwt, T-rfc-oauth2, T-ssdf-process, T-asvs-authentication
  Split: all sub-schemes

N4 · Domain Processor
  Risk-tiered TTL: 5/15/60 min bounded ✓; exp set correctly ✓
  Resource indicators: per-RS aud ✓; each RS validates own identifier ✓
  PKCE verifier: 43–128 chars ✓; S256 ✓
  JWT assertions: sig + iss + aud + exp + jti (unique, Redis-backed) ✓
  SBOM: CycloneDX + auto-updated + transitive deps + 1-hour CVE query ✓
  Prior controls: all correct ✓

N5 · All PASS

N8 · All territories PASS

N9 · Formatter → Overall PASS
Overall verdict: PASS
```

**Reason:** All ext22 patterns correctly implemented. Every territory PASS. Comprehensive multi-territory PASS.

**Phrasing-B:** Pipeline trace for risk-tiered TTL, per-RS resource indicators, RFC 7636-compliant PKCE, CycloneDX SBOM with auto-CVE, JWT assertions with jti, all prior controls. Walk through all 9 nodes for comprehensive PASS.

---

## File Summary

| Pair ID | Description | Stratum | Pipeline Focus | Verdict |
|---------|-------------|---------|----------------|---------|
| CB4-3.4-577 | Broad aud covering all RSes pipeline trace | C | T-oauth-oidc + T-rfc-jwt | CONCERN |
| CB4-3.4-578 | PKCE verifier 41 chars (below 43 minimum) pipeline trace | C | T-oauth-oidc + T-rfc-oauth2 | CONCERN |
| CB4-3.4-579 | JWT client assertion jti not tracked — "signed = replay safe" rejected | C/D | T-oauth-oidc + T-rfc-jwt | REPROCESS |
| CB4-3.4-580 | JWT client assertions with Redis-backed jti uniqueness tracking | C | T-oauth-oidc + T-rfc-jwt | PASS |
| CB4-3.4-581 | SBOM exists + 48-72h manual CVE assessment — "industry average" rejected | C/D | T-ssdf-process | CONCERN |
| CB4-3.4-582 | RFC 8707 resource indicators + per-RS aud + minimum scope | C | T-oauth-oidc + T-rfc-jwt + T-asvs-authorization | PASS |
| CB4-3.4-583 | Broad aud CONCERN + PKCE verifier 200-char CONCERN → CONCERN graduation | C | T-oauth-oidc + T-rfc-oauth2 | CONCERN |
| CB4-3.4-584 | All ext22 patterns correct — comprehensive pipeline PASS | C | Multiple | PASS |
