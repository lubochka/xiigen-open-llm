# Layer 0-C — Knowledge-Edge Pairs (043)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-043-001 — QA lexicon completeness for 037-060 topics

All QA-triggering verbs confirmed active in 037-060 range:
| QA trigger | Confirmed in scenario |
|---|---|
| review | All scenarios |
| approve | All scenarios |
| verify | Provenance, signed artifact, compliance |
| validate | Migration, SBOM, gate |
| sign off | Deprecation, advisory, gate, regulatory |
| audit | Security advisory, CVE, compliance, auditable release |
| compliance | ComplianceRelease |
| inspect | Not featured in 037-060 but remains in lexicon |
| acceptance | BackwardCompatibilityNote, APIBreakingChange |

---

## B38-0C-043-002 — No new QA triggers in 037-060

No new QA-triggering vocabulary is introduced in the 037-060 range.
Scenario-specific nouns (SBOM, attestation, provenance, regulatory) are context only.
QA fires exclusively from the established QA lexicon.

| Sentence | PredicateSet | Reason |
|---|---|---|
| "Attest the release artifact." | [] (attest not in QA lexicon) |
| "Validate the release artifact." | ["QA"] (validate fires QA) |
| "Verify the attestation." | ["QA"] (verify fires QA) |
| "Submit the regulatory filing." | [] (submit not in QA lexicon) |
| "Approve the regulatory filing." | ["QA"] (approve fires QA) |

---

## B38-0C-043-003 — "attest" and "submit" are not QA triggers

"Attest" (action: to attest something) is not in the predicate lexicon — knowledge_edge.
"Submit" (action: to submit a filing) is not in the predicate lexicon — knowledge_edge.

| Sentence | PredicateSet |
|---|---|
| "Attest the signed artifact." | [] → knowledge_edge |
| "Submit the attestation to the registry." | [] → knowledge_edge |
| "Verify the attestation and approve." | ["QA"] (verify + approve fire QA) |
| "Review the regulatory submission and approve." | ["QA"] (review + approve fire QA) |
