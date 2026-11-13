# Layer 1 — Transition Orchestrator Pairs (021)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T021-001 — SECURITY+QA: feature_addition (NuGet CVE + update workflow QA)
Request: "Audit all NuGet dependencies for CVE vulnerabilities and QA the package update workflow."
  TransitionType: feature_addition | SECURITY → 6.3 | QA → 7.3

## T021-002 — SECURITY: feature_addition (transitive CVE scan + CVSS)
Request: "Scan the dependency tree for transitive CVE exposures — flag CRITICAL and HIGH CVSS scores."
  TransitionType: feature_addition | SECURITY → 6.3

## T021-003 — SECURITY+DOCUMENT: documentation (SCA audit + upgrade paths documented)
Request: "Review the SCA report for vulnerable dependencies and document the upgrade path for each package."
  TransitionType: documentation | SECURITY → 6.3 | DOCUMENT → 6.2

## T021-004 — Zero predicates: N/A (routine package update)
Request: "Update all outdated NuGet packages to their latest stable versions."
  TransitionType: N/A | GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

## T021-005 — SECURITY+QA: feature_addition (third-party libraries + CRITICAL CVE QA)
Request: "Audit all third-party libraries for security vulnerabilities and QA remediation for CRITICAL CVEs."
  TransitionType: feature_addition | SECURITY → 6.3 | QA → 7.3
