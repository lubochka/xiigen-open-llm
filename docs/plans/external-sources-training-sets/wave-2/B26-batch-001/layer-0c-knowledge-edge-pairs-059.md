# Layer 0-C — Knowledge Edge Pairs (059) — SupplyChainSecurityAudit Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E059-001 — Supply chain security vocabulary

"CVE" (Common Vulnerability and Exposure) → SECURITY (vulnerability vocabulary)
"SBOM" (Software Bill of Materials) analysis → SECURITY (vulnerability audit context)
"typosquatted dependency" → SECURITY (attack vector vocabulary)
"update npm packages" → gap (maintenance task without vulnerability vocabulary)

---

## E059-002 — SHA hash integrity boundary

"SHA hash integrity of dependencies" → SECURITY (SHA = crypto group, integrity verification)
"SHA-256 hash of the file" → SECURITY (SHA = cryptographic vocabulary)
"file checksum" → gap (integrity check without explicit cryptographic algorithm)
Teaching: SHA/MD5/SHA-256 fire SECURITY (crypto group). "checksum" alone does not.

---

## E059-003 — SRI vocabulary

"SRI (Subresource Integrity) hash" → SECURITY (integrity hash in browser security context)
"CDN resource loading" → gap (delivery infrastructure without integrity vocabulary)
"audit CDN dependencies for missing SRI hashes" → SECURITY (audit + SRI)

---

## E059-004 — Dependency vulnerability vs maintenance

"scan for CVEs in dependencies" → SECURITY (scan + CVEs)
"update outdated dependencies" → gap (maintenance task)
"upgrade to patch a critical CVE" → SECURITY (CVE = vulnerability vocabulary)
Teaching: Updating dependencies is gap. Patching a CVE fires SECURITY.

---

## E059-005 — Trust state calibration

CVE scan + SBOM + typosquatting → deterministic_generic (three triggers)
"update npm packages" alone → gap (zero predicates)
SRI audit + SHA hash review → deterministic_generic (crypto group triggers)
