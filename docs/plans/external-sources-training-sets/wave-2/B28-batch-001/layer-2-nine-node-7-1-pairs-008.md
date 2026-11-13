# Layer 2 — Nine-Node Flow Pairs (008)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: nine_node_flow (QAGatingFlow73)

---

## B28-nine-008 — AdminService RBAC update QA gate — PASS_WITH_WARNINGS

Request: "QA the RBAC (Role-Based Access Control) update for the AdminService before production."

### Flow: QAGatingFlow73

**Node 1**: Target: AdminService RBAC update | Change: new "account_manager" role with limited admin permissions
**Node 2**: Gates: permission boundaries, privilege escalation check, audit log, regression
**Node 3**: Gate 1 — account_manager permissions: read-only on /admin/users, write on /admin/accounts ✓
  Gate 2 — Privilege escalation: account_manager cannot access /admin/permissions ✓
  Gate 3 — Audit log: account_manager actions logged with userId and timestamp ✓
  Gate 4 — Regression: existing admin and super-admin roles unaffected ✓
  WARNING: account_manager can view users' email addresses — potential PII (Personally Identifiable Information) exposure not in spec
**Node 4**: No critical failures. WARNING: PII access not explicitly authorized in spec — requires product owner review
**Node 5**: Arbiter: ReleaseGateArbiter
**Node 6**: Coverage: all permission paths tested; PII concern flagged
**Node 7**: PASS_WITH_WARNINGS — PII email visibility flagged for product review; not a blocker
**Node 8**: QA report: AdminService-RBAC-QA.md | WARNING: PII-email-access filed as MEDIUM ticket
**Node 9**: Verdict: PASS_WITH_WARNINGS — cleared with PII visibility ticket required before next sprint

