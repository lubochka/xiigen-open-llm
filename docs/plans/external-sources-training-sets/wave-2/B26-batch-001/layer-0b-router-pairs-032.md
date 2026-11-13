# Layer 0-B — Router Pairs (032) — SRIIntegrity Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R032-001 — SRI hash audit → SECURITY → 6.3

R032-001 test: "Audit all third-party script tags for missing SRI hashes."
  SECURITY → 6.3 (audit + SRI + hashes) | CompiledPredicateIds: ["SECURITY"]

---

## R032-002 — SRI + injection risk → SECURITY → 6.3

R032-002 test: "Review CDN asset loading for SRI violations and supply chain injection risks."
  SECURITY → 6.3 (review + SRI + injection) | CompiledPredicateIds: ["SECURITY"]

---

## R032-003 — Integrity hash mismatches → SECURITY → 6.3

R032-003 test: "Scan external JavaScript dependencies for integrity hash mismatches."
  SECURITY → 6.3 (scan + integrity hash) | CompiledPredicateIds: ["SECURITY"]

---

## R032-004 — Code signing certificates → SECURITY → 6.3

R032-004 test: "Verify all release artifacts have valid code signing certificates."
  SECURITY → 6.3 (signing + certificate) | CompiledPredicateIds: ["SECURITY"]

---

## R032-005 — "add integrity attributes" → IMPLEMENT → 4.1

R032-005 test: "Add integrity attributes and crossorigin=anonymous to all CDN script tags."
  IMPLEMENT → 4.1 (implementation instruction, no audit vocabulary)
  CompiledPredicateIds: ["IMPLEMENT"] | SubPhase: 4.1
