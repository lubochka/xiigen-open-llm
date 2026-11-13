# Layer 2 — Nine-Node Flow Pairs (027): InjectionAudit — SearchService
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# SubPhase: 6.3 | Flow: SecurityComplianceReviewFlow63
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## N027 — SecurityComplianceReviewFlow63: SQL Injection Audit of SearchService

### Node 1 — DomainSplitter
A = SelectedLogicalTransition: SecurityComplianceReviewFlow63
Scope: SQL injection vulnerability review of all SearchService database query construction paths.
TrustState: deterministic_generic

### Node 2 — DomainTranslator
TranslatedAuditBoundaries:
  1. SearchService.SearchProducts(string query) — query construction
  2. SearchService.SearchUsers(string name) — admin user search
  3. SearchService.FilterByCategory(string category) — category filter query
  4. SearchService.AdvancedSearch(Dictionary params) — multi-field search
  5. Parameterized query compliance across all methods

### Node 3 — SchemePlanner
Standard: OWASP Top 10 A03 (Injection) + CWE-89
Severity levels: CRITICAL, HIGH, MEDIUM, LOW

### Node 4 — DomainProcessor
Finding 1: SearchProducts — string interpolation in WHERE clause: "SELECT * FROM products WHERE name LIKE '%" + query + "%'" — CRITICAL
  Remediation: Replace with parameterized query using SqlCommand.Parameters.
Finding 2: SearchUsers — string concatenation in admin query — CRITICAL
  Remediation: Use ORM parameterized query or SqlParameter.
Finding 3: FilterByCategory — parameterized query — PASS
Finding 4: AdvancedSearch — partial parameterization; sort_order field unparameterized — HIGH
  Remediation: Validate sort_order against allowlist; do not interpolate.
Finding 5: All other read queries — parameterized — PASS

### Node 5 — Applier
CRITICAL: 2 | HIGH: 1 | MEDIUM: 0 | LOW: 0
Remediation priority: 1. Fix SearchProducts interpolation. 2. Fix SearchUsers. 3. Fix AdvancedSearch sort.

### Node 6 — ResultTranslator
Format: SecurityAuditReport
File: security-audit-report-sqli-searchservice.md
Sections: 1. Query Construction Methods, 2. Parameterization Compliance, 3. Remediation Plan

### Node 7 — ArbiterSelector
Arbiter: SecuritySeverityArbiter (deterministic_generic)
Config: require_remediation_for: ["CRITICAL", "HIGH"]

### Node 8 — Arbiter
OverallVerdict: FAIL (2 CRITICAL SQL injection vulnerabilities)
Detail: Direct string interpolation in SearchProducts and SearchUsers enables full SQL injection.

### Node 9 — Formatter
status: "FAIL | 2 CRITICAL, 1 HIGH | IMMEDIATE ACTION REQUIRED"
output_status: "ready"
file: "security-audit-report-sqli-searchservice.md"
