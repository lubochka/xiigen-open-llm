# Layer 0-A — Compiler Pairs (032) — SRIIntegrity
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C032-001 — SRI (Subresource Integrity) audit → SECURITY

A: "Audit all third-party script tags for missing SRI (Subresource Integrity) hashes."
  "audit" → SECURITY | "SRI" → SECURITY (integrity/signing vocabulary) | "hashes" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C032-002 — SRI + supply chain vulnerability

A: "Review the CDN (Content Delivery Network) asset loading for SRI violations and supply chain injection risks."
  "Review" + "SRI violations" → SECURITY | "injection risks" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C032-003 — SRI + integrity vocabulary

A: "Scan all external JavaScript dependencies for integrity hash mismatches or unsigned bundles."
  "Scan" → SECURITY | "integrity hash mismatches" → SECURITY | "unsigned bundles" → SECURITY (signing vocabulary)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C032-004 — "signing" fires SECURITY independently

A: "Verify all release artifacts have valid code signing certificates before deployment."
  "signing certificates" → SECURITY | "certificate" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C032-005 — SRI + add attribute = IMPLEMENT not SECURITY alone

A: "Add integrity attributes and crossorigin=anonymous to all CDN script tags."
  "Add" → IMPLEMENT (implementation action, not audit)
  No audit/review/scan verb; purely a code change instruction
  CompiledPredicateIds: ["IMPLEMENT"] | SubPhase: 4.1
  Teaching: "add attribute" without audit/review vocabulary → IMPLEMENT, not SECURITY.
