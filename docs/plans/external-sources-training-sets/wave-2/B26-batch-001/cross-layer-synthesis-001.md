# Cross-Layer Synthesis (001)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## S001 — End-to-end trace: "Audit the AdminService for SQL injection and QA sign-off"

### Layer 0-A
Input: "Audit the AdminService for SQL injection vulnerabilities and QA the input validation controls."
  "audit" → SECURITY | "SQL injection vulnerabilities" → SECURITY | "QA" → QA
  CompiledPredicateIds: ["SECURITY", "QA"]

### Layer 0-B
  SECURITY → SubPhase 6.3 | QA → SubPhase 7.3
  DecisionSet: [SecurityAuditDecision(6.3), QAGatingDecision(7.3)]

### Layer 0-D
  FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityAuditFlow63, QAGatingFlow73]

### Layer 1
  TransitionType: feature_addition | Sequential: SECURITY audit → QA sign-off

### Layer 2 (SecurityAuditFlow63)
  Audit findings: SQL injection in searchAdminUsers (CRITICAL) | Node 7: FAIL

### Layer 3
  Teaching: "audit" verb fires SECURITY. "QA" abbreviation fires QA. Sub-phases are independent flows.

### Cross-layer invariant
  SECURITY and QA are independent predicates routing to different sub-phases. Neither suppresses the other.
