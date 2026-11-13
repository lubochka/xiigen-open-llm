# Layer 2 — Nine-Node Flow Pairs (004)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: nine_node_flow (QAGatingFlow73)

---

## B28-nine-004 — UserService v3.1 QA gate — FAIL

Request: "QA the UserService v3.1 login redesign — validate SSO (Single Sign-On), password reset, and session management."

### Flow: QAGatingFlow73

**Node 1**: Target: UserService v3.1 | Features: SSO login, password reset, session management
**Node 2**: Gates: SSO flow, password reset flow, session lifecycle, regression suite
**Node 3**: Gate 1 — SSO: OAuth2 callback → user logged in ✓
  Gate 2 — Password reset: reset email → token → new password → login ✓
  Gate 3 — Session: logout → token invalidated → re-login required ✗ FAIL: session token reusable for 5 minutes post-logout
  Gate 4 — Regression: 3 failures in legacy username/password flow ✗
**Node 4**: CRITICAL: session not invalidated on logout (security regression from JIRA (project tracking tool) ticket SEC-891)
  HIGH: legacy login failures caused by email normalization change
**Node 5**: Arbiter: ReleaseGateArbiter
**Node 6**: Coverage: SSO 100%, password reset 100%, session lifecycle: FAIL
**Node 7**: FAIL — session management regression violates security requirement; legacy login failures unacceptable
**Node 8**: QA report: UserService-v3.1-QA-FAIL.md | Defect list: 2 blockers filed
**Node 9**: Verdict: FAIL — release blocked; fix required for session invalidation and legacy login

