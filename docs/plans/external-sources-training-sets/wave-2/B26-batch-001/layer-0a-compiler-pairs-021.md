# Layer 0-A — Compiler Pairs (021)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: compiler

---

## C021-001 — SECURITY+QA: dependency vulnerability audit

A: "Audit all NuGet (package manager for .NET) dependencies for known CVE vulnerabilities and QA the package update workflow."
  "audit" → SECURITY; "CVE vulnerabilities" → SECURITY; "QA" → QA
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3

---

## C021-002 — SECURITY: transitive dependency vulnerability scan

A: "Scan the dependency tree for transitive CVE exposures — flag any package with a CRITICAL or HIGH severity CVSS (Common Vulnerability Scoring System) score."
  "scan" → SECURITY; "CVE exposures" → SECURITY; "CRITICAL" → severity context; "CVSS" → SECURITY (vulnerability scoring)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C021-003 — SECURITY+DOCUMENT: SCA (Software Composition Analysis) audit documented

A: "Review the SCA report for all vulnerable dependencies and document the upgrade path for each affected package."
  "review" (security context) → SECURITY; "SCA report" → SECURITY (vulnerability context); "vulnerable dependencies" → SECURITY; "document" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhase: 6.3/6.2

---

## C021-004 — Gap: "dependency update" as routine maintenance is gap without vulnerability vocabulary

A: "Update all outdated NuGet packages to their latest stable versions."
  "update" → borderline IMPLEMENT? No: "update packages" without vulnerability/security vocabulary is a maintenance task, not a security audit.
  CompiledPredicateIds: [] — routine package maintenance does not fire SECURITY.
  Gap: SECURITY fires on "audit/scan for vulnerabilities" not on "update packages."

---

## C021-005 — SECURITY+QA: license and vulnerability dual audit + QA

A: "Audit all third-party libraries for security vulnerabilities and QA the remediation tracking for all CRITICAL CVEs."
  "audit" → SECURITY; "security vulnerabilities" → SECURITY; "QA" → QA; "CRITICAL CVEs" → SECURITY
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3
