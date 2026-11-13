# Layer 3 — Teaching Unit Pairs (021)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU021-001 — SCA (Software Composition Analysis) fires SECURITY in vulnerability audit context
A: "Run the SCA scan and review all HIGH and CRITICAL findings." Does SECURITY fire?
B: Yes. "SCA scan" → SECURITY (software composition analysis is a security audit tool). "vulnerability findings" → SECURITY.
   "review" (security context) → SECURITY. CompiledPredicateIds: ["SECURITY"]. SubPhase: 6.3.

## TU021-002 — "update outdated packages" is gap; "audit for CVEs" fires SECURITY
A: "Update all outdated NuGet packages to their latest stable versions." vs "Audit NuGet packages for CVE vulnerabilities."
B: "update outdated packages" → gap (routine maintenance). CompiledPredicateIds: [].
   "audit" + "CVE vulnerabilities" → SECURITY. CompiledPredicateIds: ["SECURITY"].
   Rule: Routine upgrades are gap. CVE vulnerability audits fire SECURITY.

## TU021-003 — "supply chain attack" fires SECURITY; "supply chain" as business logistics is gap
A: "Audit the build pipeline for supply chain attack vectors." vs "Optimize the supply chain for faster delivery."
B: "supply chain attack" → SECURITY. CompiledPredicateIds: ["SECURITY"].
   "supply chain for faster delivery" → gap. CompiledPredicateIds: [].

## TU021-004 — "SBOM" fires SECURITY in vulnerability context
A: "Audit the SBOM (Software Bill of Materials) for components with known CVEs." Does SECURITY fire?
B: Yes. "audit" → SECURITY. "SBOM" in vulnerability context → SECURITY. "known CVEs" → SECURITY.
   CompiledPredicateIds: ["SECURITY"]. Teaching: SBOM in a CVE audit context fires SECURITY.

## TU021-005 — "transitive dependency" is gap; "transitive CVE" fires SECURITY
A: "The transitive dependency on Newtonsoft.Json causes version conflicts." Does SECURITY fire?
B: No. "transitive dependency" + "version conflicts" → gap (version management). CompiledPredicateIds: [].
   Contrast: "Scan transitive dependencies for CVE exposures" → "CVE" fires SECURITY.
