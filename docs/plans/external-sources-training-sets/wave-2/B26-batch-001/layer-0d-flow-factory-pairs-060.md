# Layer 0-D — Flow Factory Pairs (060) — Complete Factory Reference Summary
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F060-001 — FlowFactory complete predicate-to-flow matrix (all B26 topics)

SECURITY (audit) → SecurityComplianceReviewFlow63
SECURITY (test-writing) → SecurityTestingFlow52
AUTHORIZATION → AuthorizationAuditFlow63
IMPLEMENT → FeatureImplementationFlow41
DOCUMENT → DocumentationFlow62
QA → QATestingFlow73
ARCHITECTURE → ArchitectureDesignFlow23
API_CONTRACT → ContractDefinitionFlow31
Zero predicates → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

---

## F060-002 — TransitionType priority matrix

IMPLEMENT present → feature_addition (highest priority)
DOCUMENT present, IMPLEMENT absent → documentation
SECURITY/QA alone (no DOCUMENT, no IMPLEMENT) → feature_addition
Zero predicates → N/A (GapHandler)

---

## F060-003 — Dual-predicate flow invocation reference

privilege escalation → CreateMulti(["SECURITY","AUTHORIZATION"])
IDOR → CreateMulti(["SECURITY","AUTHORIZATION"])
wildcard permission → CreateMulti(["SECURITY","AUTHORIZATION"])
RBAC role injection → CreateMulti(["SECURITY","AUTHORIZATION"])
least-privilege access + audit → CreateMulti(["SECURITY","AUTHORIZATION"])

---

## F060-004 — Gap boundary: six gap action patterns

"configure X" → gap | "schedule X" → gap | "run X" → gap (exercise execution)
"read X" → gap | "monitor X" → gap | "update X" → gap (without CVE/vulnerability vocabulary)

---

## F060-005 — Sub-phase 5.2 vs 6.3 factory disambiguation

Security audit (review/scan/audit/harden/protect) → SecurityComplianceReviewFlow63 (6.3)
Security testing (write tests/implement tests/assert) → SecurityTestingFlow52 (5.2)
Both can appear in one request → CreateMulti(["SECURITY(5.2)", "SECURITY(6.3)"])
