# Layer 2 — Nine-Node Flow Pairs (028): XXEAudit — DocumentProcessingService
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# SubPhase: 6.3 | Flow: SecurityComplianceReviewFlow63
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## N028 — SecurityComplianceReviewFlow63: XXE Audit of DocumentProcessingService

### Node 1 — DomainSplitter
A = SelectedLogicalTransition: SecurityComplianceReviewFlow63
Scope: XXE (XML External Entity) vulnerability audit of all XML parsers in DocumentProcessingService.
TrustState: deterministic_generic

### Node 2 — DomainTranslator
TranslatedAuditBoundaries:
  1. InvoiceParser.Parse(XmlDocument) — DTD processing status
  2. ReportGenerator.LoadTemplate(string xmlPath) — external entity status
  3. ConfigLoader.ParseXml(Stream) — DTD and external entity settings
  4. ThirdPartyXmlClient.Process(string xml) — external entity resolution
  5. XXE-to-SSRF chaining potential via file:// and http:// entities

### Node 3 — SchemePlanner
Standard: OWASP Top 10 A05 + CWE-611 (Improper Restriction of XML External Entity Reference)
Severity levels: CRITICAL, HIGH, MEDIUM, LOW

### Node 4 — DomainProcessor
Finding 1: InvoiceParser — XmlDocument with DTD processing enabled, no external entity restriction — CRITICAL
  Remediation: Set XmlReaderSettings.DtdProcessing = DtdProcessing.Prohibit.
Finding 2: ReportGenerator.LoadTemplate — XmlDocument default settings, external entity resolution active — CRITICAL
  Remediation: Use XmlReader with prohibited DTD processing.
Finding 3: ConfigLoader.ParseXml — XmlReader with DtdProcessing.Prohibit — PASS
Finding 4: ThirdPartyXmlClient — external entity resolution enabled; SSRF-to-file chaining possible — HIGH
  Remediation: Disable external entity resolution; validate input schema before parsing.
Finding 5: SSRF chain risk via XXE — PASS (internal network not reachable from parsing host)

### Node 5 — Applier
CRITICAL: 2 | HIGH: 1 | MEDIUM: 0 | LOW: 0
Remediation priority: 1. Fix InvoiceParser DTD settings. 2. Fix ReportGenerator. 3. Restrict ThirdPartyXmlClient.

### Node 6 — ResultTranslator
Format: SecurityAuditReport
File: security-audit-report-xxe-documentprocessing.md
Sections: 1. Parser DTD Configuration, 2. External Entity Settings, 3. SSRF Chain Assessment

### Node 7 — ArbiterSelector
Arbiter: SecuritySeverityArbiter (deterministic_generic)
Config: require_remediation_for: ["CRITICAL", "HIGH"]

### Node 8 — Arbiter
OverallVerdict: FAIL (2 CRITICAL XXE exposures)
Detail: InvoiceParser and ReportGenerator allow external entity processing; attacker can read local files.

### Node 9 — Formatter
status: "FAIL | 2 CRITICAL, 1 HIGH | IMMEDIATE ACTION REQUIRED"
output_status: "ready"
file: "security-audit-report-xxe-documentprocessing.md"
