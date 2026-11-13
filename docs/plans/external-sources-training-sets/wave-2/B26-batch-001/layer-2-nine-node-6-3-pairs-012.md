# Layer 2 — Nine-Node Flow Pairs (012)
# Batch: B26-batch-001 | Scenario: B26 — Security audit | SubPhase: 6.3
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## N012 — SecurityAuditFlow63: Dependency CVE Scan

Request: "Scan project dependencies for known CVE vulnerabilities."

### Node 1 — Classification
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

### Node 2 — Context Assembly
  Scope: All NuGet packages in the solution
  Tool: NuGet audit / OWASP dependency check

### Node 3 — Audit Plan
  3.1 Run dotnet list package --vulnerable
  3.2 Cross-reference with NVD (National Vulnerability Database) CVE list
  3.3 Prioritize CRITICAL and HIGH severity CVEs
  3.4 Identify transitive dependency CVEs

### Node 4 — Tool Resolution
  dotnet list package --vulnerable --include-transitive

### Node 5 — Audit Execution
  Direct dependencies: 2 CVEs (1 CRITICAL — Newtonsoft.Json prototype pollution; 1 HIGH — XmlSerializer XXE)
  Transitive: 3 CVEs (1 MEDIUM, 2 LOW)
  Total: 5 CVEs

### Node 6 — Verification
  1 CRITICAL (Newtonsoft.Json 12.0.1 → CVE-2024-21907) confirmed

### Node 7 — Quality Gate
  Status: FAIL | Critical: 1 | High: 1 | Medium: 1 | Low: 2

### Node 8 — Output Assembly
  OutputType: SecurityAuditReport
  Remediation: Upgrade Newtonsoft.Json to 13.0.3+; upgrade System.Xml.Serialization; update transitive dependencies via lock file

### Node 9 — Result
  Result: FAIL — 5 CVEs found (1 CRITICAL, 1 HIGH, 1 MEDIUM, 2 LOW)
