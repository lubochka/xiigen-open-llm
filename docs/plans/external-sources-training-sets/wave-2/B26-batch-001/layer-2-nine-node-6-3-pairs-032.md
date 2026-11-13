# Layer 2 — Nine-Node Flow Pairs (032): InsecureDeserializationAudit — ApiGateway
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# SubPhase: 6.3 | Flow: SecurityComplianceReviewFlow63
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## N032 — SecurityComplianceReviewFlow63: Insecure Deserialization Audit of ApiGateway

### Node 1 — DomainSplitter
A = SelectedLogicalTransition: SecurityComplianceReviewFlow63
Scope: Insecure deserialization vulnerability audit of all JSON and binary deserialization endpoints in ApiGateway.
TrustState: deterministic_generic

### Node 2 — DomainTranslator
TranslatedAuditBoundaries:
  1. JsonSerializer configuration — TypeNameHandling setting
  2. Binary deserialization endpoints — BinaryFormatter usage
  3. Deserialization from untrusted sources — request body, headers
  4. Type allowlist enforcement
  5. Gadget chain exposure via known .NET deserialization sinks

### Node 3 — SchemePlanner
Standard: OWASP Top 10 A08 (Software and Data Integrity Failures) + CWE-502
Severity levels: CRITICAL, HIGH, MEDIUM, LOW

### Node 4 — DomainProcessor
Finding 1: JsonSerializer configured with TypeNameHandling.All in LegacyApiController — CRITICAL
  Remediation: Set TypeNameHandling.None; use explicit type mappings.
Finding 2: BinaryFormatter usage in CacheSerializer.Deserialize — CRITICAL
  Remediation: Replace BinaryFormatter with MessagePack or System.Text.Json; BinaryFormatter is banned in .NET 7+.
Finding 3: Request body deserialization — uses System.Text.Json without TypeNameHandling — PASS
Finding 4: Type allowlist — absent for LegacyApiController — HIGH
  Remediation: Implement ISerializationBinder with allowlist of permitted types.
Finding 5: Gadget chain exposure — ysoserial.net payloads applicable to BinaryFormatter — CRITICAL
  Remediation: Remove all BinaryFormatter usage; critical RCE (Remote Code Execution) risk.

### Node 5 — Applier
CRITICAL: 3 | HIGH: 1 | MEDIUM: 0 | LOW: 0
Remediation priority: 1. Remove BinaryFormatter. 2. Fix TypeNameHandling.All. 3. Add type allowlist.

### Node 6 — ResultTranslator
Format: SecurityAuditReport
File: security-audit-report-deserialization-apigateway.md
Sections: 1. Serializer Configuration, 2. Binary Deserialization, 3. Type Allowlist, 4. Gadget Chain Risk

### Node 7 — ArbiterSelector
Arbiter: SecuritySeverityArbiter (deterministic_generic)
Config: require_remediation_for: ["CRITICAL", "HIGH"]

### Node 8 — Arbiter
OverallVerdict: FAIL (3 CRITICAL deserialization vulnerabilities)
Detail: TypeNameHandling.All and BinaryFormatter enable remote code execution via gadget chains.

### Node 9 — Formatter
status: "FAIL | 3 CRITICAL, 1 HIGH | IMMEDIATE ACTION REQUIRED"
output_status: "ready"
file: "security-audit-report-deserialization-apigateway.md"
