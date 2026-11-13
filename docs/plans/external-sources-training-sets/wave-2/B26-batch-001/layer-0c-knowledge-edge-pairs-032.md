# Layer 0-C — Knowledge Edge Pairs (032) — SRIIntegrity Knowledge Edge
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E032-001 — SRI and integrity vocabulary that fires SECURITY

"SRI" (Subresource Integrity) → SECURITY (integrity/signing vocabulary)
"integrity hash" → SECURITY
"code signing" → SECURITY (signing fires)
"signed bundles" / "unsigned bundles" → SECURITY
"integrity mismatch" → SECURITY

---

## E032-002 — Supply chain injection vocabulary

"supply chain injection" → SECURITY (injection fires)
"CDN asset integrity" → SECURITY
"third-party script without hash" → SECURITY (hash fires)
"tampered dependency" → SECURITY

---

## E032-003 — "add integrity attribute" vs "audit for missing integrity"

"Audit scripts for missing SRI hashes" → SECURITY (audit + SRI + hashes)
"Add integrity attribute to script tags" → IMPLEMENT (implementation task, no audit)
Teaching: Audit/review/scan of SRI → SECURITY. Adding SRI attributes → IMPLEMENT.

---

## E032-004 — Code signing certificate distinction

"signing certificates" → SECURITY (certificate fires via signing)
"TLS certificate" → SECURITY (TLS fires)
"business registration certificate" → gap (not a cryptographic/security certificate)
Teaching: Context determines if "certificate" fires SECURITY — cryptographic/signing context only.

---

## E032-005 — Trust state for SRI/integrity audits

"Audit all CDN scripts for missing SRI hashes and supply chain injection risks" → deterministic_generic
"Check integrity hashes" → probationary_accepted (check = weak verb)
"Add integrity attributes" → knowledge_edge (IMPLEMENT path, not SECURITY 6.3)
