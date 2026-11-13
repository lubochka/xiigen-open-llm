# Node 08 — Arbiter Units — Extension Set 2
# Stage 3 Action 3 — Coverage Extension (Stratum B team context)
# Batch: CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Sequences: CB4-3.4-107 through CB4-3.4-116
# Instances: 20 (10 base pairs × 2 phrasings each)
# Running total after this file: 232 instances (sequences 001–116)

---

## Coverage targets for this file

Stratum B: realistic team and product contexts that arise in practice.
Primary scenarios:
  - Legacy system migration: team is transitioning from old algorithm to new; partial migration
  - Regulated industry: team argues that a different compliance standard (PCI-DSS, HIPAA) satisfies the requirement
  - Prior audit argument: team argues "we passed the last audit so this is fine"
  - Third-party library: team cites library documentation instead of implementation evidence
  - "Not in scope" argument: team argues the endpoint is internal-only, therefore exempt
  - Sprint boundary argument: team argues the fix is "planned for next sprint"
  - Compensating control argument: team argues one control compensates for another's absence
  - Staging environment: team submits evidence from staging, not production
  - Multiple deployments: team argues that one deployment's compliance covers all deployments
  - "Risk accepted" argument: team states risk has been formally accepted by management

---

## Pair CB4-3.4-107

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — partial algorithm migration: old JWT library still active on legacy endpoints)
**Source:** OWASP ASVS 5.0.0, IETF RFC 7519
**Verdict:** REPROCESS

### A

```
arbiter: RFC-JWT-Arbiter
territory: T-rfc-jwt-normative
findings:
  - "New endpoints (/api/v2/*): RS256 signing confirmed, alg:none rejected"
  - "Legacy endpoints (/api/v1/*): HS256 signing still in use; migration scheduled Q3 2026"
  - "JWT validation library updated on v2 path; v1 path uses old library version 2.3.1"
  - "Team note: migration is in progress; v1 endpoints receive <5% of traffic"
pipeline_run_id: PR-4648
```

### B

```
arbiter_result:
  arbiter: RFC-JWT-Arbiter
  verdict: REPROCESS
  requirements_violated: [RFC7519-§4.1.6, "RFC7519-§8-MUST-symmetric-key-risk"]
  gap: "Legacy endpoints (/api/v1/*) continue to use HS256 shared-key signing. HS256 shared-key tokens can be forged by any service holding the shared key. The migration timeline does not exempt the current deployment from the requirement."
  action_required: "Complete algorithm migration to RS256 on all endpoints including /api/v1/* before re-submission. Disable HS256 signing on the legacy library path. Traffic volume (<5%) does not reduce the severity of a symmetric-key forgery risk."
  note: "Partial compliance is not compliance. A system with one non-compliant endpoint remains non-compliant for the endpoint-scope claim."
```

### Reason

The team's migration is incomplete. RFC 7519 requirements apply to all active endpoints, not only new ones. The <5% traffic argument does not change the algorithm's cryptographic properties — HS256 with a shared key allows cross-service forgery regardless of traffic volume. The arbiter must return REPROCESS (not CONCERN) because this is a MUST-level violation (symmetric key trust boundary), not a deviation from a recommended practice. The note explicitly addresses the partial-compliance argument: partial migration = not migrated for the remaining endpoints.

### Phrasing B

```
JWT library migration in progress for PR-4648.
V2 endpoints: RS256, alg:none rejected — compliant.
V1 endpoints: HS256 still active, migration scheduled Q3 2026, <5% traffic.
What verdict does the RFC-JWT-Arbiter return, and does the migration schedule or traffic percentage change it?
```

Expected: REPROCESS. HS256 shared-key on any active endpoint is a MUST violation. Migration schedule is not evidence of compliance; it is evidence of future intent. Traffic percentage is irrelevant to cryptographic risk.

---

## Pair CB4-3.4-108

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — team cites PCI-DSS compliance as satisfying ASVS requirement)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
arbiter: ASVS-V6-Auth-Arbiter
territory: T-asvs-authentication
findings:
  - "Credential transmission: TLS enforced — confirmed"
  - "Password hashing: SHA-256 with application-level salt — team claims this satisfies V6.2.5"
  - "Team note: 'We are PCI-DSS v4.0 compliant; Requirement 8.3.6 covers password requirements. This should satisfy ASVS V6.2.5.'"
pipeline_run_id: PR-4659
```

### B

```
arbiter_result:
  arbiter: ASVS-V6-Auth-Arbiter
  verdict: CONCERN
  practice: V6.2.5
  gap: "SHA-256 with application-level salt does not satisfy V6.2.5. V6.2.5 requires a memory-hard or computationally expensive adaptive hashing function (bcrypt, scrypt, Argon2, or PBKDF2). SHA-256, regardless of salt, is a fast cryptographic hash and does not provide the same brute-force resistance."
  action_recommended: "Migrate password storage to bcrypt (cost ≥ 10), scrypt, Argon2id, or PBKDF2 (≥ 600,000 iterations with SHA-256). PCI-DSS Requirement 8.3.6 specifies minimum password complexity, not hashing algorithm — PCI-DSS compliance does not satisfy ASVS V6.2.5."
  note: "CONCERN not REPROCESS: the team has implemented a salted hash (partial control). The gap is the algorithm choice, not the complete absence of hashing. Recommend remediation before next security review cycle."
```

### Reason

PCI-DSS and ASVS are different standards with different requirements for the same domain. PCI-DSS 8.3.6 covers password complexity rules; ASVS V6.2.5 covers hashing algorithm strength. Complying with PCI-DSS does not automatically satisfy ASVS requirements. The arbiter must evaluate ASVS V6.2.5 independently — SHA-256+salt fails V6.2.5 because it is a fast hash. The verdict is CONCERN (not REPROCESS) because the team has implemented salted hashing — partial control is present. The gap is the algorithm, not the complete absence of protection. The note explains the PCI-DSS/ASVS distinction to prevent recurrence.

### Phrasing B

```
Team claims SHA-256 with application-level salt satisfies ASVS V6.2.5 because they are PCI-DSS v4.0 compliant.
Arbiter: ASVS-V6-Auth-Arbiter. Run PR-4659.
What verdict, and does PCI-DSS compliance satisfy ASVS V6.2.5?
```

Expected: CONCERN (partial control present — salted hash exists, but algorithm is fast hash not memory-hard). PCI-DSS compliance does not satisfy ASVS V6.2.5 — different standard, different requirements for hashing algorithm. Action_recommended: migrate to bcrypt/Argon2/scrypt/PBKDF2.

---

## Pair CB4-3.4-109

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — "we passed the last audit" argument; prior audit does not cover current deployment)
**Source:** NIST SP 800-218 SSDF v1.1
**Verdict:** REPROCESS

### A

```
arbiter: SSDF-PW-Arbiter
territory: T-ssdf-process
findings:
  - practice: PW.2
    evidence_provided: "external security audit report, dated 2024-03-15, concluded threat model was adequate"
    team_note: "We passed a SOC 2 audit in March 2024 which reviewed our threat model. The auditor confirmed it was acceptable."
  - current_date: "2026-05-05"
  - threat_model_artifact: "no updated threat model artefact provided; team references audit report only"
pipeline_run_id: PR-4671
```

### B

```
arbiter_result:
  arbiter: SSDF-PW-Arbiter
  verdict: REPROCESS
  practice: PW.2
  gap: "The 2024-03-15 SOC 2 audit report is evidence that a threat model existed as of that date. It is not evidence of a current threat model. The gap between the last confirmed threat model review (2024-03-15) and the current date (2026-05-05) is 25 months — exceeding the recommended annual review cycle by 13 months."
  action_required: "Provide a threat model artefact (not an audit report referencing a prior threat model) with a review or update date within the last 12 months. The SOC 2 audit report confirms a past state; it cannot certify the current state of the threat model."
```

### Reason

The SOC 2 audit confirmed the threat model was adequate in March 2024 — over 25 months ago. An audit report references the state of evidence at audit time. It cannot certify the current threat model. The team is conflating "we passed an audit" with "we currently satisfy the requirement." SSDF PW.2 requires current, maintained design artefacts. The verdict is REPROCESS (not CONCERN) because the gap is 25 months (more than twice the recommended cycle) and no dated current artefact is provided — this is FM-SSDF-3 (TIMING_GAP_IGNORED) combined with FM-SSDF-5 (ARTIFACT_NAME_REQUIRED).

### Phrasing B

```
Team provides: SOC 2 audit report from 2024-03-15 confirming threat model was adequate.
Today: 2026-05-05. No updated threat model artefact provided.
SSDF-PW-Arbiter, PW.2. Run PR-4671.
Does the 2024 audit report satisfy PW.2 today?
```

Expected: REPROCESS. Audit report confirms past state (2024-03-15), not current state. Gap = 25 months. Current dated threat model artefact required. Team argument (we passed the audit) does not satisfy PW.2 in 2026.

---

## Pair CB4-3.4-110

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — team cites third-party library documentation as implementation evidence)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
arbiter: ASVS-V9-Token-Arbiter
territory: T-token-validation
findings:
  - requirement: V9.1.1
    evidence: "team links to jsonwebtoken npm library documentation confirming RS256 support"
    team_note: "we use jsonwebtoken v9.0 which supports RS256 per its documentation"
  - requirement: V9.2.1
    evidence: "team links to jsonwebtoken documentation showing exp claim is validated by default"
  - implementation_evidence: "no configuration file, no code snippet, no deployment log showing the library is configured to use RS256 in this deployment"
pipeline_run_id: PR-4682
```

### B

```
arbiter_result:
  arbiter: ASVS-V9-Token-Arbiter
  verdict: CONCERN
  requirements: [V9.1.1, V9.2.1]
  gap: "Library documentation confirms the library supports RS256 and validates exp — it does not confirm that this specific deployment uses RS256 or that exp validation is not disabled. A library's capability documentation is not evidence of deployment configuration."
  action_recommended: "Provide one or more of: (1) JWT signing configuration file showing algorithm=RS256; (2) code snippet showing token issuance with RS256; (3) deployment log or environment variable confirming RS256 key loaded. Library documentation is acceptable supporting material, not standalone evidence."
  note: "CONCERN not REPROCESS: the library does support RS256, reducing the likelihood of incorrect configuration. However, configuration evidence is required to confirm the capability is exercised in this deployment."
```

### Reason

Library documentation proves capability; it does not prove deployment configuration. The team must demonstrate that the library is configured to use RS256 in this deployment. The verdict is CONCERN (not REPROCESS) because the library genuinely supports RS256 (partial evidence) — the gap is configuration evidence, not capability. If the team had cited a library that does not support RS256 at all, it would be REPROCESS. The action_recommended lists three acceptable alternative evidence types.

### Phrasing B

```
ASVS-V9-Token-Arbiter, run PR-4682.
Team provides jsonwebtoken v9.0 library documentation showing RS256 support and exp validation.
No configuration file, no code, no deployment logs provided.
Verdict?
```

Expected: CONCERN. Library documentation confirms capability but not deployment configuration. Three acceptable alternative evidence types listed in action_recommended. Not REPROCESS because the library genuinely supports RS256; not PASS because deployment configuration is unconfirmed.

---

## Pair CB4-3.4-111

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — "internal-only endpoint" exemption claim)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter: ASVS-V8-AuthZ-Arbiter
territory: T-asvs-authz
findings:
  - requirement: V8.1.1
    endpoint: "/internal/admin/user-export"
    status: "no authorization check; any service in the internal network can call this endpoint"
    team_note: "this is an internal microservice endpoint, only accessible within our VPC — external users cannot reach it, so authorization is not needed"
pipeline_run_id: PR-4694
```

### B

```
arbiter_result:
  arbiter: ASVS-V8-AuthZ-Arbiter
  verdict: REPROCESS
  requirements_violated: [V8.1.1]
  gap: "No authorization check on /internal/admin/user-export. The internal network boundary does not satisfy V8.1.1. ASVS V8 requires per-resource access control checks regardless of network segment. An internal endpoint is still a protected resource — any compromised internal service can call this endpoint without restriction."
  action_required: "Implement service-to-service authorization on /internal/admin/user-export. Verify caller identity (service account or mTLS certificate) and confirm caller has explicit permission to invoke this endpoint before returning data."
  note: "Network perimeter is not a substitute for application-level authorization. Internal endpoints are attack surfaces for lateral movement after any internal service compromise."
```

### Reason

Network perimeter (VPC, internal network) does not satisfy ASVS V8.1.1, which requires application-level per-resource authorization checks. The "internal-only" argument is a classic authorization bypass attempt. An internal endpoint is reachable by any compromised internal service or container in the VPC. ASVS V8 does not include a network-segment exemption. The verdict is REPROCESS (not CONCERN) because the authorization check is completely absent — this is CWE-862 pattern at the ASVS level. The note explains why the exemption claim is architecturally unsound (lateral movement risk).

### Phrasing B

```
Team argument: "/internal/admin/user-export is VPC-internal only; external users can't reach it; no auth check needed."
ASVS-V8-AuthZ-Arbiter, V8.1.1, run PR-4694.
Does the internal network boundary satisfy the authorization requirement?
```

Expected: REPROCESS. V8.1.1 requires application-level authorization regardless of network segment. Internal-only argument is not an exemption under ASVS. Action_required: service-to-service authorization on the endpoint.

---

## Pair CB4-3.4-112

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — "fix is planned for next sprint" argument)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter: ASVS-V7-Session-Arbiter
territory: T-asvs-session
findings:
  - requirement: V7.2.1
    status: "session tokens transmitted over HTTP on the password reset flow"
    team_note: "we know about this — it is in our backlog (JIRA ticket SEC-447); fix is planned for next sprint (Sprint 84, starting 2026-05-12)"
pipeline_run_id: PR-4707
```

### B

```
arbiter_result:
  arbiter: ASVS-V7-Session-Arbiter
  verdict: REPROCESS
  requirements_violated: [V7.2.1]
  gap: "Session tokens transmitted over HTTP on the password reset flow. V7.2.1 MUST-level requirement violated."
  action_required: "Enforce TLS on the password reset flow for session token transmission before re-submission. A planned fix (JIRA SEC-447, Sprint 84) is not a current control — the requirement is not satisfied until the fix is deployed."
  note: "Sprint schedule and backlog entries document intent, not implementation. Resubmit after Sprint 84 fix is deployed to confirm compliance."
```

### Reason

A ticket in a backlog and a sprint plan do not satisfy a MUST-level requirement. ASVS V7.2.1 requires session tokens to be transmitted over TLS — right now, in the current deployment, they are not. The arbiter must return REPROCESS based on current state. The sprint plan is noted (it is useful context for the team) but it does not change the verdict. The note directs the team to resubmit after deployment, not before.

### Phrasing B

```
ASVS-V7-Session-Arbiter, run PR-4707.
Session tokens over HTTP on password reset flow.
Team says: JIRA ticket SEC-447 is planned for Sprint 84 starting 2026-05-12.
Verdict?
```

Expected: REPROCESS. V7.2.1 MUST violation. Backlog ticket and sprint plan are future intent, not current compliance. Resubmit after fix deployed.

---

## Pair CB4-3.4-113

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — compensating control argument: WAF blocks the attack vector, so V8.1.1 is not required)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter: ASVS-V8-AuthZ-Arbiter
territory: T-asvs-authz
findings:
  - requirement: V8.1.1
    endpoint: "/api/reports/{report_id}"
    status: "no per-report ownership check in application code"
    team_note: "our WAF (Web Application Firewall) rate-limits requests to 100/minute per IP and blocks known attack patterns including sequential enumeration. We believe the WAF provides sufficient compensating control for the absence of an application-level ownership check"
pipeline_run_id: PR-4718
```

### B

```
arbiter_result:
  arbiter: ASVS-V8-AuthZ-Arbiter
  verdict: REPROCESS
  requirements_violated: [V8.1.1]
  gap: "No per-report ownership check in application code. The WAF rate limit and pattern detection are network-layer controls — they do not implement per-resource authorization. A WAF does not verify that the requesting user is authorized to access a specific report ID."
  action_required: "Implement application-level ownership check: before returning report data for /api/reports/{report_id}, verify that the authenticated user is the owner of or has explicit permission to access that report_id. WAF controls remain valuable as defence-in-depth but cannot substitute for application-level authorization."
  note: "ASVS V8.1.1 requires authorization to be enforced at the application layer, not only at the network perimeter. A sophisticated attacker who bypasses or is not blocked by the WAF pattern (e.g., low-rate manual enumeration, compromised internal session) will still reach the unprotected endpoint."
```

### Reason

WAF rate-limiting and pattern detection are network-layer controls that reduce attack surface but do not provide per-resource authorization. ASVS V8.1.1 requires an application-level check. A WAF does not know whether user A is authorized to see report 12345 — it only knows about request rates and known attack signatures. The compensating control argument is invalid for ASVS requirements that specify application-level implementation. REPROCESS (not CONCERN) because the authorization check is completely absent in the application.

### Phrasing B

```
Team claims WAF rate-limiting compensates for absent application-level ownership check on /api/reports/{id}.
ASVS-V8-AuthZ-Arbiter, V8.1.1, run PR-4718.
Does WAF rate-limiting satisfy the ASVS V8.1.1 requirement?
```

Expected: REPROCESS. WAF is a network-layer control; V8.1.1 requires application-layer ownership check. Compensating control argument invalid. Action_required: add application-level authorization before returning report data.

---

## Pair CB4-3.4-114

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — evidence from staging environment, not production)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
arbiter: ASVS-V6-Auth-Arbiter
territory: T-asvs-authentication
findings:
  - requirement: V6.2.5
    evidence: "bcrypt cost=12 confirmed in staging environment — staging-app-config.yaml shows BCRYPT_ROUNDS=12"
    team_note: "our staging environment mirrors production; what is true in staging is true in production"
  - requirement: V6.2.3
    evidence: "TLS confirmed in staging via staging.example-internal.com certificate"
  - production_evidence: "no production configuration artefact provided"
pipeline_run_id: PR-4729
```

### B

```
arbiter_result:
  arbiter: ASVS-V6-Auth-Arbiter
  verdict: CONCERN
  requirements: [V6.2.3, V6.2.5]
  gap: "Evidence provided is from the staging environment. The claim that staging mirrors production is asserted but not verified. Staging and production environments frequently diverge on configuration values."
  action_recommended: "Provide production configuration artefacts confirming BCRYPT_ROUNDS=12 and TLS enforcement in the production deployment. Acceptable evidence: production application configuration export, deployment manifest, or infrastructure-as-code for the production environment."
  note: "CONCERN not REPROCESS: staging evidence suggests the controls are likely present in production. The gap is evidence coverage, not control absence. Provide production-specific evidence to resolve to PASS."
```

### Reason

Staging evidence does not prove production configuration. Teams routinely have configuration drift between environments. The verdict is CONCERN (not REPROCESS) because the staging configuration suggests the controls likely exist in production — the team is not claiming the controls are absent, they are providing evidence from the wrong environment. The action_recommended specifies what acceptable production evidence looks like (configuration export, deployment manifest, IaC).

### Phrasing B

```
Team provides staging configuration (staging-app-config.yaml) showing bcrypt rounds=12 and TLS.
No production config provided. Team says "staging mirrors production."
ASVS-V6-Auth-Arbiter, run PR-4729.
```

Expected: CONCERN (staging evidence only). Controls likely present but unconfirmed in production. Action_recommended: production configuration artefact. Not REPROCESS because controls are likely present; not PASS because production evidence is absent.

---

## Pair CB4-3.4-115

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — "risk accepted by management" argument for CWE-862 gap)
**Source:** CWE Top 25 2023
**Verdict:** REPROCESS

### A

```
arbiter: CWE-Authz-Arbiter
territory: T-cwe-authz
findings:
  - cwe_id: CWE-862
    endpoint: "/api/invoices/{invoice_id}"
    status: "no ownership check; any authenticated user can retrieve any invoice"
    severity: HIGH
    team_note: "our CISO has accepted this risk in writing (risk acceptance #RA-2024-117). This risk is formally accepted and documented."
pipeline_run_id: PR-4741
```

### B

```
arbiter_result:
  arbiter: CWE-Authz-Arbiter
  verdict: REPROCESS
  cwe_id: CWE-862
  severity: HIGH
  gap: "No ownership check on /api/invoices/{invoice_id}. Any authenticated user can retrieve any invoice by ID."
  action_required: "Add authorization check verifying the requesting user is the owner of or has explicit permission to access the requested invoice_id before returning data."
  note: "Management risk acceptance (#RA-2024-117) is an organizational governance record — it does not change the technical finding. This pipeline evaluates technical controls; risk acceptance decisions are recorded separately and do not satisfy a CWE-862 gap. The finding stands; the acceptance record should be noted in the deployment risk register."
```

### Reason

Management risk acceptance is an organizational governance decision. It does not change the technical finding that an authorization check is absent. The arbiter evaluates technical controls; it does not have authority to dismiss a finding based on a risk acceptance record. REPROCESS stands. The note acknowledges the acceptance record exists and suggests it belongs in the deployment risk register — but it does not modify the verdict. This is a critical training point: risk acceptance documents have no effect on arbiter verdicts.

### Phrasing B

```
CWE-Authz-Arbiter, run PR-4741.
Gap: /api/invoices/{id} has no ownership check — any authenticated user can access any invoice.
Team produces: risk acceptance record RA-2024-117 signed by CISO.
Does management risk acceptance satisfy the CWE-862 gap?
```

Expected: REPROCESS. Risk acceptance is an organizational record; it does not satisfy a technical control requirement. CWE-862 finding stands. Note acknowledges the record but clarifies it does not affect the arbiter verdict.

---

## Pair CB4-3.4-116

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — one deployment satisfies; second deployment not assessed; team claims both covered)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
arbiter: ASVS-V9-Token-Arbiter
territory: T-token-validation
findings:
  - requirement: V9.1.1
    evidence:
      - "production-eu.example.com: RS256 confirmed via configuration export"
      - "production-us.example.com: no configuration provided"
    team_note: "our deployments are identical — the EU deployment configuration covers both regions"
pipeline_run_id: PR-4753
```

### B

```
arbiter_result:
  arbiter: ASVS-V9-Token-Arbiter
  verdict: CONCERN
  requirements: [V9.1.1]
  gap: "Evidence provided for production-eu.example.com only. production-us.example.com has no configuration artefact. Regional deployments are separate systems; configuration evidence from one region does not certify another region's configuration."
  action_recommended: "Provide V9.1.1 configuration evidence for production-us.example.com (configuration export, deployment manifest, or infrastructure-as-code confirming RS256 in the US deployment). If deployments share the same Helm chart or IaC module, provide the IaC source as joint evidence."
  note: "CONCERN not REPROCESS: EU deployment evidence confirms RS256 is supported and configured. The gap is evidence coverage for the US deployment. If IaC manages both deployments identically, a single IaC reference suffices for both."
```

### Reason

Two distinct production deployments are separate systems under ASVS assessment. Evidence from one deployment does not automatically cover another. The team's "identical deployments" assertion is unverified. The verdict is CONCERN (not REPROCESS) because the EU evidence confirms RS256 is in use and correctly configured — the US deployment is likely the same, but unconfirmed. The note provides the efficient resolution path: if both deployments are managed by the same IaC module, one IaC reference covers both.

### Phrasing B

```
ASVS-V9-Token-Arbiter, run PR-4753.
EU deployment: RS256 confirmed via config export.
US deployment: no config provided. Team claims both are identical.
Verdict?
```

Expected: CONCERN. EU deployment satisfies V9.1.1; US deployment unconfirmed. IaC joint evidence acceptable if deployments share the same module. Not REPROCESS because EU evidence confirms capability in this system family; not PASS because US deployment is unconfirmed.

---

## File summary

| Seq | Stratum | Source | Verdict | Key team argument trained against |
|-----|---------|--------|---------|----------------------------------|
| 107 | B | ASVS+RFC | REPROCESS | Partial migration: v1 endpoints still HS256; <5% traffic irrelevant |
| 108 | B | ASVS | CONCERN | Cross-standard conflation: PCI-DSS 8.3.6 ≠ ASVS V6.2.5 |
| 109 | B | SSDF | REPROCESS | Prior audit: 2024 SOC 2 report does not certify 2026 threat model state |
| 110 | B | ASVS | CONCERN | Library docs: capability ≠ deployment configuration evidence |
| 111 | B | ASVS | REPROCESS | Internal-only exemption: VPC perimeter ≠ application-level authorization |
| 112 | B | ASVS | REPROCESS | Backlog ticket + sprint plan = future intent, not current compliance |
| 113 | B | ASVS | REPROCESS | WAF compensating control: network-layer ≠ application-layer authorization |
| 114 | B | ASVS | CONCERN | Staging evidence: controls likely present but production config required |
| 115 | B | CWE | REPROCESS | Risk acceptance: governance record does not satisfy technical control gap |
| 116 | B | ASVS | CONCERN | Multi-deployment: one region's evidence does not cover other region |

**Running total: 232 instances (sequences 001–116)**
Stratum breakdown: A ~27%, B ~30%, C ~28%, D ~15%
