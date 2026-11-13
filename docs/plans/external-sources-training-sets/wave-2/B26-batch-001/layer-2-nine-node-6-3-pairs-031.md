# Layer 2 — Nine-Node Flow Pairs (031): TimingAttackAudit — PasswordService
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# SubPhase: 6.3 | Flow: SecurityComplianceReviewFlow63
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## N031 — SecurityComplianceReviewFlow63: Timing Attack Audit of PasswordService

### Node 1 — DomainSplitter
A = SelectedLogicalTransition: SecurityComplianceReviewFlow63
Scope: Timing attack vulnerability review of all credential comparison and HMAC (Hash-based Message Authentication Code) verification in PasswordService.
TrustState: deterministic_generic

### Node 2 — DomainTranslator
TranslatedAuditBoundaries:
  1. PasswordService.VerifyPassword(string input, string hash) — comparison method
  2. HmacService.VerifySignature(byte[] input, byte[] expected) — HMAC comparison
  3. ApiKeyService.ValidateKey(string provided) — API key comparison
  4. User enumeration via response timing differentials
  5. Login endpoint latency consistency across valid/invalid accounts

### Node 3 — SchemePlanner
Standard: OWASP ASVS V2.1 + CWE-208 (Observable Timing Discrepancy)
Severity levels: CRITICAL, HIGH, MEDIUM, LOW

### Node 4 — DomainProcessor
Finding 1: PasswordService.VerifyPassword — uses string.Equals() which exits early on first mismatch — HIGH
  Remediation: Replace with CryptographicOperations.FixedTimeEquals() or equivalent constant-time comparison.
Finding 2: HmacService.VerifySignature — uses SequenceEqual which is not constant-time — HIGH
  Remediation: Use CryptographicOperations.FixedTimeEquals(computed, expected).
Finding 3: ApiKeyService.ValidateKey — uses == operator on string — HIGH
  Remediation: Use constant-time comparison for all API key validation.
Finding 4: User enumeration via timing — login returns 401 faster for non-existent users (no bcrypt computation) — MEDIUM
  Remediation: Always compute bcrypt hash even for non-existent usernames; use dummy hash comparison.
Finding 5: Login endpoint timing consistency — bcrypt rounds consistent for valid accounts — PASS

### Node 5 — Applier
CRITICAL: 0 | HIGH: 3 | MEDIUM: 1 | LOW: 0
Remediation priority: 1. Fix VerifyPassword comparison. 2. Fix HMAC verification. 3. Fix API key comparison. 4. Fix user enumeration timing.

### Node 6 — ResultTranslator
Format: SecurityAuditReport
File: security-audit-report-timing-attack-passwordservice.md
Sections: 1. Credential Comparison Methods, 2. HMAC Verification, 3. User Enumeration Risk

### Node 7 — ArbiterSelector
Arbiter: SecuritySeverityArbiter (deterministic_generic)
Config: require_remediation_for: ["CRITICAL", "HIGH"]

### Node 8 — Arbiter
OverallVerdict: PASS_WITH_WARNINGS (0 CRITICAL, 3 HIGH)
Detail: No CRITICAL timing oracle; three HIGH-severity non-constant-time comparisons require remediation.

### Node 9 — Formatter
status: "PASS_WITH_WARNINGS | 0 CRITICAL, 3 HIGH"
output_status: "ready"
file: "security-audit-report-timing-attack-passwordservice.md"
