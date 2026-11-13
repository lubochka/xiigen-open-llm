# Layer 0-D — Flow Factory Pairs (021)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F021-001 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73] (NuGet CVE + update workflow)
Input: "Audit all NuGet dependencies for known CVE vulnerabilities and QA the package update workflow."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]

## F021-002 — SECURITY → SecurityComplianceReviewFlow63 (transitive CVE + CVSS)
Input: "Scan the dependency tree for transitive CVE exposures — flag CRITICAL and HIGH CVSS scores."
  CompiledPredicateIds: ["SECURITY"]
  Factory: FlowFactory.Create("SECURITY") → SecurityComplianceReviewFlow63

## F021-003 — SECURITY+DOCUMENT → [SecurityComplianceReviewFlow63, DocumentationFlow62] (SCA audit)
Input: "Review the SCA report for vulnerable dependencies and document the upgrade path for each affected package."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"]
  Factory: FlowFactory.CreateMulti(["SECURITY","DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]

## F021-004 — Zero predicates → GapHandler (routine package update — no vulnerability vocabulary)
Input: "Update all outdated NuGet packages to their latest stable versions."
  CompiledPredicateIds: [] | GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

## F021-005 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73] (third-party library + CRITICAL CVE)
Input: "Audit all third-party libraries for security vulnerabilities and QA remediation tracking for CRITICAL CVEs."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]
