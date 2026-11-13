# Layer 0-C — Knowledge Edge Pairs (021)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E021-001 — "dependency vulnerability" fires SECURITY; "outdated dependency" alone is gap

A-fires: "Audit the NuGet dependency graph for known security vulnerabilities — any package with a CRITICAL CVE must be updated within 24 hours."
  "audit" → SECURITY; "security vulnerabilities" → SECURITY; "CRITICAL CVE" → SECURITY
  CompiledPredicateIds: ["SECURITY"]

A-gap: "Upgrade all outdated dependencies before the release."
  "outdated dependencies" → gap; "upgrade" → gap
  CompiledPredicateIds: []

---

## E021-002 — "SCA" (Software Composition Analysis) fires SECURITY in audit context

A: "Run the SCA scan on the repository and review all HIGH and CRITICAL vulnerability findings."
  "SCA scan" → SECURITY (software composition analysis is a security audit tool); "vulnerability findings" → SECURITY
  CompiledPredicateIds: ["SECURITY"]

---

## E021-003 — "transitive dependency" is gap; "transitive CVE" fires SECURITY

A-gap: "The transitive dependency on Newtonsoft.Json causes version conflicts."
  "transitive dependency" → gap (version conflict, not security)
  CompiledPredicateIds: []

A-fires: "Scan the transitive dependency tree for CVE exposures — Newtonsoft.Json has a known deserialization CVE."
  "scan" → SECURITY; "CVE exposures" → SECURITY; "deserialization CVE" → SECURITY
  CompiledPredicateIds: ["SECURITY"]

---

## E021-004 — "supply chain attack" fires SECURITY; "supply chain" alone is gap

A-fires: "Audit the build pipeline for supply chain attack vectors — verify all package integrity checksums."
  "audit" → SECURITY; "supply chain attack" → SECURITY; "package integrity" → SECURITY (signing/hash context)
  CompiledPredicateIds: ["SECURITY"]

A-gap: "Optimize the supply chain for faster component delivery."
  "supply chain" → gap (business logistics, not security)
  CompiledPredicateIds: []

---

## E021-005 — "SBOM" (Software Bill of Materials) fires SECURITY via supply chain security context

A: "Generate and audit the SBOM for all production dependencies — flag any component with a known CVE."
  "audit" → SECURITY; "SBOM" → SECURITY (supply chain / software composition security); "known CVE" → SECURITY
  CompiledPredicateIds: ["SECURITY"]
  Teaching: SBOM in a vulnerability audit context fires SECURITY.
