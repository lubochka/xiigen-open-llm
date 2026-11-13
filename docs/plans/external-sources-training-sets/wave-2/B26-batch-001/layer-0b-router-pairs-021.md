# Layer 0-B — Router Pairs (021)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: router

---

## R021-001 — SECURITY+QA → [6.3, 7.3]: NuGet dependency CVE audit + update workflow QA
Request: "Audit all NuGet dependencies for known CVE vulnerabilities and QA the package update workflow."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]

## R021-002 — SECURITY → 6.3: transitive CVE scan
Request: "Scan the dependency tree for transitive CVE exposures — flag CRITICAL and HIGH CVSS scores."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## R021-003 — SECURITY+DOCUMENT → [6.3, 6.2]: SCA audit documented
Request: "Review the SCA report for vulnerable dependencies and document the upgrade path for each affected package."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhases: [6.3, 6.2]

## R021-004 — Zero predicates → GapHandler: routine package update
Request: "Update all outdated NuGet packages to their latest stable versions."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Gap: "update packages" without vulnerability vocabulary is routine maintenance — no predicates.

## R021-005 — SECURITY+QA → [6.3, 7.3]: third-party library security audit + CRITICAL CVE QA
Request: "Audit all third-party libraries for security vulnerabilities and QA the remediation tracking for all CRITICAL CVEs."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]
