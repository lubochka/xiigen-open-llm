# Layer 0-A — Compiler Pairs (059) — SupplyChainSecurityAudit
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C059-001 — Supply chain vulnerability audit → SECURITY

A: "Audit all third-party dependencies for known CVEs (Common Vulnerabilities and Exposures) using SBOM (Software Bill of Materials) analysis."
  "audit" → SECURITY | "CVEs" → SECURITY (vulnerability vocabulary) | "SBOM" analysis = security-action context
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C059-002 — Dependency hash verification → SECURITY

A: "Review the package lock files to verify SHA (Secure Hash Algorithm) hash integrity of all installed dependencies."
  "review" → SECURITY | "SHA hash integrity" → SECURITY (cryptographic group)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C059-003 — "update npm packages" → gap (no audit vocabulary)

A: "Update all npm packages to the latest stable versions."
  "update" → gap | "npm packages" → gap (maintenance task, no vulnerability/audit vocabulary)
  CompiledPredicateIds: [] → knowledge_edge

---

## C059-004 — Typosquatting scan → SECURITY

A: "Scan the package manifest for typosquatted dependency names that mimic legitimate packages."
  "scan" → SECURITY | "typosquatted" → SECURITY (attack-vector vocabulary)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C059-005 — Supply chain + SRI (Subresource Integrity) + DOCUMENT → SECURITY+DOCUMENT

A: "Audit CDN (Content Delivery Network) dependencies for missing SRI (Subresource Integrity) hashes and document all findings."
  "audit" → SECURITY | "SRI" → SECURITY (integrity verification) | "document" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | TransitionType: documentation
