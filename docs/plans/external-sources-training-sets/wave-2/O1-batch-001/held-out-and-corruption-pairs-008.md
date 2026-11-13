# Held-Out and Corruption Pairs (008)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: held_out_and_corruption

---

## Held-out pair 008-H001 — add MFA to admin user workflow

A: "Add MFA (Multi-Factor Authentication) to the admin user provisioning workflow with TOTP (Time-Based One-Time Password) enforcement."
Expected B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
(not in training set — held out for evaluation)

Lexicon evidence (for evaluator reference only):
  IMPLEMENT: "add"
  SECURITY: "mfa", "authentication"
  AUTHORIZATION: "admin"

---

## Held-out pair 008-H002 — enforce MFA on privilege escalation

A: "Enforce MFA (Multi-Factor Authentication) re-authentication when any user attempts a privilege escalation."
Expected B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
(not in training set — held out for evaluation)

---

## Corruption 008-C001 — v6-01 invents MULTI_FACTOR predicate

v6 corrupt: "Add MFA enforcement for admin roles."
  → Predicates: ["MULTI_FACTOR", "AUTHORIZATION"] | TransitionType: feature_addition
Correct: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
Error type: v6-01 (predicate invention)
MULTI_FACTOR is not a valid predicate — "mfa" fires the canonical SECURITY predicate
The model invented a new predicate instead of using SECURITY

---

## Corruption 008-C002 — v6-01 invents MFA_ENFORCEMENT predicate

v6 corrupt: "Enforce MFA re-authentication for privilege escalation."
  → Predicates: ["MFA_ENFORCEMENT", "PRIVILEGE_ESCALATION_GUARD"] | TransitionType: feature_addition
Correct: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
Error type: v6-01 (predicate invention)
MFA_ENFORCEMENT and PRIVILEGE_ESCALATION_GUARD are invented — the correct predicates are
SECURITY (mfa, authentication) and AUTHORIZATION (privilege)
IMPLEMENT fires on "enforce" as a structural action verb
