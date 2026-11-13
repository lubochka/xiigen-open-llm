# Layer 2 — Nine-Node Flow Pairs (048): TimingAttackAudit — HmacService PASS_WITH_WARNINGS
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# SubPhase: 6.3 | Flow: SecurityComplianceReviewFlow63
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## N048 — SecurityComplianceReviewFlow63: Timing Attack Audit — HmacService (PASS_WITH_WARNINGS)

### Node 1 — DomainSplitter
A = SelectedLogicalTransition: SecurityComplianceReviewFlow63
Scope: Timing attack and user enumeration audit of HmacService and login endpoint.
TrustState: deterministic_generic

### Node 2 — DomainTranslator
TranslatedAuditBoundaries:
  1. HmacService.Verify — HMAC comparison method
  2. Login endpoint timing consistency
  3. Password comparison method
  4. API key validation comparison
  5. User enumeration via timing differential

### Node 3 — SchemePlanner
Standard: OWASP ASVS V2.1 + CWE-208
Severity levels: CRITICAL, HIGH, MEDIUM, LOW

### Node 4 — DomainProcessor
Finding 1: HmacService.Verify — uses CryptographicOperations.FixedTimeEquals — PASS
Finding 2: Login endpoint — bcrypt computation applied for both valid and invalid usernames — PASS (user enumeration mitigated)
Finding 3: PasswordService.VerifyPassword — uses CryptographicOperations.FixedTimeEquals — PASS
Finding 4: ApiKeyService.ValidateKey — uses string == operator (not constant-time) — HIGH
  Remediation: Replace with CryptographicOperations.FixedTimeEquals for API key comparison.
Finding 5: Login timing consistency — measured 5ms maximum differential — acceptable — PASS

### Node 5 — Applier
CRITICAL: 0 | HIGH: 1 | MEDIUM: 0 | LOW: 0
Remediation priority: Fix ApiKeyService.ValidateKey comparison to use constant-time algorithm.

### Node 6 — ResultTranslator
Format: SecurityAuditReport
File: security-audit-report-timing-hmacservice.md
Sections: 1. HMAC Comparison, 2. Login Timing, 3. Password Comparison, 4. API Key Validation

### Node 7 — ArbiterSelector
Arbiter: SecuritySeverityArbiter (deterministic_generic)
Config: require_remediation_for: ["CRITICAL", "HIGH"]

### Node 8 — Arbiter
OverallVerdict: PASS_WITH_WARNINGS (0 CRITICAL, 1 HIGH)
Detail: HmacService and login endpoint correctly implemented; ApiKeyService requires constant-time fix.

### Node 9 — Formatter
status: "PASS_WITH_WARNINGS | 0 CRITICAL, 1 HIGH"
output_status: "ready"
file: "security-audit-report-timing-hmacservice.md"
