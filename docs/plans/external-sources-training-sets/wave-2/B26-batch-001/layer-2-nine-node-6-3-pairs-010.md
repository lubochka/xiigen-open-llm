# Layer 2 — Nine-Node Flow Pairs (010)
# Batch: B26-batch-001 | Scenario: B26 — Security audit | SubPhase: 6.3
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## N010 — SecurityAuditFlow63: FileUploadService Path Traversal Audit

Request: "Scan the FileUploadService for directory traversal and path injection vulnerabilities."

### Node 1 — Classification
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

### Node 2 — Context Assembly
  Service: FileUploadService | Attack Vector: Directory traversal, path injection
  Target: uploadFile(name, content), getFile(filename)

### Node 3 — Audit Plan
  3.1 Inject traversal payload in filename: ../../etc/passwd
  3.2 URL-encoded variant: %2e%2e%2fetc%2fpasswd
  3.3 Verify file extension allowlist (no .exe, .php, .sh)
  3.4 Confirm storage path is outside web root

### Node 4 — Tool Resolution
  Payload injection via HTTP test request

### Node 5 — Audit Execution
  Path traversal ../: Blocked (SAFE) | URL-encoded: Not blocked (CRITICAL) | Extension allowlist: Present (SAFE)
  Storage location: Outside web root (SAFE)

### Node 6 — Verification
  1 CRITICAL: URL-encoded traversal bypasses path normalization

### Node 7 — Quality Gate
  Status: FAIL | Critical: 1 | High: 0

### Node 8 — Output Assembly
  OutputType: SecurityAuditReport
  Remediation: Normalize path before validation; use Path.GetFullPath and verify canonical path starts within upload directory

### Node 9 — Result
  Result: FAIL — 1 CRITICAL path traversal via URL encoding
