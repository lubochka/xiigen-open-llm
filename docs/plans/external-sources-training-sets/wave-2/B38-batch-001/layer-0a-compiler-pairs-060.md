# Layer 0-A — Compiler Pairs (060)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-060-001 — Complete factory reference: 049-060 provenance and compliance batch

This file is a complete reference for the 049-060 batch covering ArtifactProvenanceTracking, SBOMGeneration (Software Bill of Materials), SignedArtifact, AttestationPolicy, ProvenanceVerification, SupplyChainRelease, ComplianceRelease, AuditableRelease, GatedRelease, RegulatorySubmission, FinalReleaseReport.

### Predicate pattern summary for 049-060
| File | Primary topic | Key predicate patterns |
|---|---|---|
| 049 | ArtifactProvenanceTracking | ["DOCUMENT"] / ["IMPLEMENT"] / ["QA"] / ["QA","SECURITY"] / ["QA","IMPLEMENT","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT","SECURITY"] |
| 050 | SBOMGeneration | ["DOCUMENT"] / ["QA"] / ["QA","SECURITY"] / ["QA","IMPLEMENT","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT","SECURITY"] |
| 051 | SignedArtifact | ["DOCUMENT"] / ["QA"] / ["QA","SECURITY"] / ["QA","IMPLEMENT","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT","SECURITY"] |
| 052 | AttestationPolicy | ["DOCUMENT"] / ["QA"] / ["QA","DOCUMENT","SECURITY"] / ["QA","IMPLEMENT","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT","SECURITY"] |
| 053 | ProvenanceVerification | ["QA"] / ["DOCUMENT"] / ["QA","SECURITY"] / ["QA","IMPLEMENT","DOCUMENT"] |
| 054 | SupplyChainRelease | ["DOCUMENT"] / ["QA"] / ["QA","SECURITY"] / ["QA","IMPLEMENT","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT","SECURITY"] |
| 055 | ComplianceRelease | ["DOCUMENT"] / ["QA"] / ["QA","DOCUMENT","SECURITY"] / ["QA","IMPLEMENT","DOCUMENT","SECURITY"] |
| 056 | AuditableRelease | ["DOCUMENT"] / ["QA","SECURITY"] / ["QA","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT","SECURITY"] |
| 057 | GatedRelease | ["DOCUMENT"] / ["QA"] / ["QA","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT","SECURITY"] |
| 058 | RegulatorySubmission | ["DOCUMENT"] / ["QA"] / ["QA","DOCUMENT","SECURITY"] / ["QA","IMPLEMENT","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT","SECURITY"] |
| 059 | FinalReleaseReport | ["DOCUMENT"] / ["QA"] / ["QA","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT","SECURITY"] |
| 060 | Complete factory reference | Reference file |

---

## B38-0A-060-002 — 12 teaching points for 049-060

1. "SBOM", "attestation", "provenance", "supply chain", "auditable", "gated", "regulatory" are context nouns — no predicate fires from them alone.
2. "Audit" always co-fires SECURITY (rule 19) — never QA alone when "audit" is the trigger.
3. "Compliance" fires QA in verification/validation context; it does not fire DOCUMENT alone.
4. "Service" fires IMPLEMENT always (rule 2) — "attestation service", "provenance service", "gate service", "reporting service" all produce IMPLEMENT.
5. "Build" fires IMPLEMENT always (rule 6) — even in provenance/compliance contexts.
6. "Submit", "generate", "issue", "sign", "configure", "archive" are gap verbs — no predicate fires.
7. SECURITY fires from "security", "CVE" (Common Vulnerabilities and Exposures), "vulnerability" — these are security-domain terms.
8. "Criteria" fires DOCUMENT for all qualifiers: provenance, submission, auditability, gate, regulatory, report.
9. QA is absolutely last in all multi-predicate scenarios.
10. TransitionType = feature_addition if and only if IMPLEMENT is in the predicate set.
11. "Policy" as a document artifact fires DOCUMENT; "policy" alone in non-document context is gap.
12. Five-predicate maximum is achievable in 049-060: ["QA","IMPLEMENT","DOCUMENT","SECURITY","ARCHITECTURE"] if architecture review co-occurs.

---

## B38-0A-060-003 — Anti-corruption for 049-060

v6-01 (QA suppression): QA must fire from review/approve/verify/validate/sign-off regardless of SBOM, provenance, regulatory co-present nouns. These nouns are context — never suppressors.

v6-03 (predicate collapse): ["QA","IMPLEMENT","DOCUMENT","SECURITY"] MUST produce DecisionCount: 4. Any collapse to fewer decisions is v6-03 corruption.
