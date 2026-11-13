# Layer 0-C — Knowledge-Edge Pairs (050)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-050-001 — Compliance scenarios: "inspect" fires QA

"Inspect" is in the QA lexicon. It fires QA even in compliance/regulatory contexts.

| Sentence | PredicateSet |
|---|---|
| "Inspect the regulatory submission for completeness." | ["QA"] (inspect fires QA) |
| "Inspect the SBOM for missing components." | ["QA"] (inspect fires QA) |
| "Inspect the signed artifact signature." | ["QA"] (inspect fires QA) |

---

## B38-0C-050-002 — "quality" fires QA in release scenarios

"Quality" is in the QA lexicon. It fires QA in all contexts.

| Sentence | PredicateSet |
|---|---|
| "Verify the quality of the release artifact." | ["QA"] (verify + quality both fire QA) |
| "Quality review of the supply chain release." | ["QA"] (quality fires QA) |
| "Quality gate for the gated release." | ["QA"] (quality fires QA) |
| "Assess the quality of the SBOM." | ["QA"] (quality fires QA; "assess" might or might not be in lexicon) |

Note: "Assess" is not in QA lexicon — only "quality" fires QA in last example.

---

## B38-0C-050-003 — Complete QA lexicon confirmation for B38

Confirmed QA-triggering terms active in all B38 scenarios including 037-060:
review, approve, verify, validate, sign-off (sign off), audit, quality, compliance, inspect, acceptance

No new QA triggers are introduced in the 037-060 range.
All SBOM/provenance/attestation/regulatory/gate nouns are context only — they fire QA only when co-present with one of the above QA-lexicon terms.
