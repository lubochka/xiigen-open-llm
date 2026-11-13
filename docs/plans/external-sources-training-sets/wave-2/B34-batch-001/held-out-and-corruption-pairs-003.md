# Held-Out and Corruption Pairs (003)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-003-A — Held-out: "explain the auth flow"

A: "Explain the authentication flow to the new team members."
B: Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Reason: "explain" = teaching vocabulary → gap. "authentication" without security-implementing action → no SECURITY.
Phrasing-B: Gap — explaining security concepts is teaching, not implementing security controls.

---

## HC-003-B — Held-out: "secure the auth layer"

A: "Secure the authentication layer against session fixation attacks."
B: Predicates: ["SECURITY"] | SecurityAuditFlow | TransitionType: documentation
Reason: "secure" + "attacks" = security-implementing action → SECURITY fires.
Phrasing-B: SECURITY — securing authentication = implementing security controls (not explaining them).

---

## HC-003-C — Corruption v6-02: "explain auth" fires SECURITY

v6 corrupt: "Explain the authentication vulnerability to the team" → Predicates: ["SECURITY"]
Correct: Predicates: [] → GapHandler
Error type: v6-02 (gap suppression). Explaining a vulnerability ≠ implementing security. "explain" = teaching → gap.
