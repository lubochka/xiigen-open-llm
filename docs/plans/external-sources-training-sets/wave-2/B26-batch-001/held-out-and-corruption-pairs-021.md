# Held-Out and Corruption Pairs (021)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## H021-001 — Held-out: "SCA scan" fires SECURITY
Request: "Run the SCA scan on all repositories and review all CRITICAL and HIGH vulnerability findings."
  Correct: ["SECURITY"]
  Evidence: "SCA scan" → SECURITY (software composition analysis = security audit); "vulnerability findings" → SECURITY

## H021-002 — Corruption v6-01: "update outdated packages" fires SECURITY (invention)
Corrupt: "Update all NuGet packages to the latest versions." → ["SECURITY"]
Correct: []
  Fix: Package update without vulnerability vocabulary is routine maintenance — gap. Zero predicates.

## H021-003 — Held-out: "supply chain attack" fires SECURITY
Request: "Audit the build pipeline for supply chain attack vectors — verify all package checksums."
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "supply chain attack" → SECURITY; "package checksums" → SECURITY (integrity verification)

## H021-004 — Corruption v6-03: DOCUMENT dropped from SCA audit
Corrupt: "Review the SCA report for vulnerable dependencies and document upgrade paths." → ["SECURITY"]
Correct: ["SECURITY", "DOCUMENT"]
  Fix: "document" fires DOCUMENT. Multi-predicate — both must be retained.

## H021-005 — Held-out: "Newtonsoft.Json CVE" fires SECURITY
Request: "Audit Newtonsoft.Json for CVE-2024-21907 exposure — is the version >= 13.0.3?"
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "CVE-2024-21907" → SECURITY (named CVE = vulnerability identifier)
