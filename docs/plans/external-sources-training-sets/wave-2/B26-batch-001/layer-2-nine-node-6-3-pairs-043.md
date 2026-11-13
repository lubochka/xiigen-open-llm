# Layer 2 — Nine-Node Flow Pairs (043): APTHunting — ServiceAccountAudit
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# SubPhase: 6.3 | Flow: SecurityComplianceReviewFlow63 + AuthorizationAuditFlow63
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## N043 — SecurityComplianceReviewFlow63 + AuthorizationAuditFlow63: APT Hunting — Service Account Audit

### Node 1 — DomainSplitter
A = SelectedLogicalTransition: SecurityComplianceReviewFlow63 (primary) + AuthorizationAuditFlow63 (concurrent)
Scope: APT (Advanced Persistent Threat) hunting focused on lateral movement via compromised service accounts and RBAC role injection.
Dual predicate: "privilege escalation" and "RBAC role injection" fire both SECURITY and AUTHORIZATION.
TrustState: deterministic_generic

### Node 2 — DomainTranslator
TranslatedAuditBoundaries:
  1. Service account authentication logs — lateral movement patterns
  2. RBAC role assignments — unauthorized role additions
  3. Credential harvesting indicators in auth logs
  4. Scheduled task and registry persistence
  5. Outbound data exfiltration patterns

### Node 3 — SchemePlanner
Standard: MITRE ATT&CK TA0008 (Lateral Movement) + TA0004 (Privilege Escalation)
Severity levels: CRITICAL, HIGH, MEDIUM, LOW

### Node 4 — DomainProcessor
Finding 1: svc_analytics service account authenticated from 3 different production servers in 10-minute window — anomalous lateral movement — CRITICAL
  Remediation: Disable svc_analytics; investigate source of credential compromise; rotate service account secrets.
Finding 2: RBAC role "analytics_reader" elevated to "platform_admin" for svc_analytics without change ticket — CRITICAL
  Remediation: Revert role change immediately; implement role change approval workflow with audit trail.
Finding 3: Scheduled task "WindowsUpdate_cleanup" created during anomaly window — persistence indicator — HIGH
  Remediation: Delete scheduled task; investigate task content for malicious payload.
Finding 4: No exfiltration observed in network logs during anomaly window — PASS
Finding 5: Credential harvesting indicator: 200 failed auth attempts from svc_analytics IP before lateral movement — HIGH
  Remediation: Implement service account lockout and anomaly detection.

### Node 5 — Applier
CRITICAL: 2 | HIGH: 2 | MEDIUM: 0 | LOW: 0
Remediation priority: 1. Disable svc_analytics. 2. Revert RBAC elevation. 3. Remove scheduled task. 4. Add service account lockout.

### Node 6 — ResultTranslator
Format: SecurityAuditReport
File: security-audit-report-apt-hunting-serviceaccount.md
Sections: 1. Lateral Movement Evidence, 2. RBAC Privilege Escalation, 3. Persistence Indicators, 4. Exfiltration Assessment

### Node 7 — ArbiterSelector
Arbiter: SecuritySeverityArbiter (deterministic_generic)
Config: require_remediation_for: ["CRITICAL", "HIGH"]

### Node 8 — Arbiter
OverallVerdict: FAIL (2 CRITICAL APT indicators confirmed)
Detail: Active lateral movement and unauthorized privilege escalation via compromised service account.

### Node 9 — Formatter
status: "FAIL | 2 CRITICAL, 2 HIGH | IMMEDIATE ACTION REQUIRED"
output_status: "ready"
file: "security-audit-report-apt-hunting-serviceaccount.md"
