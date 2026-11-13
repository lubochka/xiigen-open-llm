# Layer 2 — Nine-Node Flow Pairs (008)
# Batch: B26-batch-001 | Scenario: B26 — Security audit | SubPhase: 6.3
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## N008 — SecurityAuditFlow63: SessionService Broken Authentication Audit

Request: "Audit the SessionService for broken authentication — verify session expiry and logout invalidation."

### Node 1 — Classification
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

### Node 2 — Context Assembly
  Service: SessionService | Attack Vector: Session fixation, session reuse post-logout
  Target: createSession(userId), validateSession(token), invalidateSession(token)

### Node 3 — Audit Plan
  3.1 Confirm session token regenerated after successful login (prevents session fixation)
  3.2 Verify session expires after 30 minutes of inactivity
  3.3 Confirm session is invalidated on logout (server-side invalidation, not just client cookie delete)
  3.4 Check session token entropy ≥128-bit

### Node 4 — Tool Resolution
  Manual test: capture session token, logout, attempt reuse

### Node 5 — Audit Execution
  Token regeneration: YES (SAFE) | Expiry 30min: YES (SAFE) | Logout invalidation: NO — server stores no session state (HIGH)
  Token entropy: 256-bit (SAFE)

### Node 6 — Verification
  1 HIGH: Logout doesn't invalidate server-side — stateless tokens allow reuse until natural expiry

### Node 7 — Quality Gate
  Status: PASS_WITH_WARNINGS | Critical: 0 | High: 1 (logout invalidation gap)

### Node 8 — Output Assembly
  OutputType: SecurityAuditReport
  Remediation: Implement server-side token revocation list or switch to short-lived tokens (5min) with refresh

### Node 9 — Result
  Result: PASS_WITH_WARNINGS — 1 HIGH finding
