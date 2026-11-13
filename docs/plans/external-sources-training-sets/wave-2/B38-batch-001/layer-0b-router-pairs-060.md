# Layer 0-B — Router Pairs (060)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-060-001 — Complete routing reference: 049-060 provenance and compliance batch

This file is a complete routing reference for files 049-060.

### Routing summary for 049-060
| Scenario | PredicateSet | Phases | DecisionCount | TransitionType |
|---|---|---|---|---|
| ArtifactProvenanceTracking — IMPLEMENT | ["IMPLEMENT"] | 4.1 | 1 | feature_addition |
| ArtifactProvenanceTracking — QA+IMPLEMENT+DOCUMENT+SECURITY | ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | 4.1, 2.1, 3.4, 7.1 | 4 | feature_addition |
| SBOMGeneration — knowledge_edge | [] | none | 0 | N/A |
| SBOMGeneration — QA+IMPLEMENT+DOCUMENT+SECURITY | ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | 4.1, 2.1, 3.4, 7.1 | 4 | feature_addition |
| SignedArtifact — knowledge_edge | [] | none | 0 | N/A |
| SignedArtifact — QA+IMPLEMENT+DOCUMENT+SECURITY | ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | 4.1, 2.1, 3.4, 7.1 | 4 | feature_addition |
| AttestationPolicy — QA+DOCUMENT+SECURITY | ["QA","DOCUMENT","SECURITY"] | 2.1, 3.4, 7.1 | 3 | no_contribution |
| ProvenanceVerification — knowledge_edge | [] | none | 0 | N/A |
| SupplyChainRelease — knowledge_edge | [] | none | 0 | N/A |
| ComplianceRelease — QA+DOCUMENT+SECURITY | ["QA","DOCUMENT","SECURITY"] | 2.1, 3.4, 7.1 | 3 | no_contribution |
| AuditableRelease — QA+IMPLEMENT+DOCUMENT+SECURITY | ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | 4.1, 2.1, 3.4, 7.1 | 4 | feature_addition |
| GatedRelease — QA+IMPLEMENT+DOCUMENT+SECURITY | ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | 4.1, 2.1, 3.4, 7.1 | 4 | feature_addition |
| RegulatorySubmission — QA+IMPLEMENT+DOCUMENT+SECURITY | ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | 4.1, 2.1, 3.4, 7.1 | 4 | feature_addition |
| FinalReleaseReport — QA+IMPLEMENT+DOCUMENT+SECURITY | ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | 4.1, 2.1, 3.4, 7.1 | 4 | feature_addition |

---

## B38-0B-060-002 — Key routing rules confirmed for 049-060

1. Four-predicate ["QA","IMPLEMENT","DOCUMENT","SECURITY"] is the canonical maximum pattern for all provenance/compliance scenarios.
2. Three-predicate ["QA","DOCUMENT","SECURITY"] is the no-IMPLEMENT variant — no_contribution always.
3. IMPLEMENT alone (single-predicate) is valid — routes only to 4.1 with feature_addition.
4. Knowledge_edge paths are confirmed for: SBOMGeneration (generate), SignedArtifact (sign), ProvenanceVerification (check), SupplyChainRelease (coordinate).
5. QA is always last when present — absolute for all 49-060 scenarios.

---

## B38-0B-060-003 — Anti-corruption for 049-060

Corrupt: AttestationPolicy ["QA","DOCUMENT","SECURITY"] → DecisionCount: 1 (only 7.1)
Correct: DecisionCount: 3 (2.1 + 3.4 + 7.1) — DOCUMENT and SECURITY must each get their own decision.

Corrupt: SBOMGeneration "Generate the SBOM" → ["QA"] (QA misfire)
Correct: PredicateSet: [] — "generate" is not in any predicate lexicon → knowledge_edge.
