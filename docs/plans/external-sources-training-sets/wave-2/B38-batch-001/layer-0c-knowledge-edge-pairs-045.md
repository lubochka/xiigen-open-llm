# Layer 0-C — Knowledge-Edge Pairs (045)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-045-001 — Supply chain and compliance: "integrate" and "align" as gaps

"Integrate" and "align" are process verbs not in any predicate lexicon.

| Sentence | PredicateSet |
|---|---|
| "Integrate third-party components for the release." | [] → knowledge_edge |
| "Align vendor releases with the supply chain." | [] → knowledge_edge |
| "Review the supply chain release integrity." | ["QA"] (review fires QA) |
| "Build the supply chain integrity service." | ["IMPLEMENT"] (build + service fire) |

---

## B38-0C-045-002 — "report" vs. "document": noun vs. verb

"Report" as a noun is a document artifact — context only, no predicate.
"Report" as an action verb is not in the predicate lexicon — gap.
"Document" as an action verb fires DOCUMENT.

| Sentence | PredicateSet |
|---|---|
| "Report the release status." | [] → knowledge_edge ("report" verb not in lexicon) |
| "Document the final release report." | ["DOCUMENT"] ("document" verb fires; "report" = artifact) |
| "Review the final release report." | ["QA"] ("review" fires QA) |
| "Approve the final release report." | ["QA"] ("approve" fires QA) |

---

## B38-0C-045-003 — "archival" and "retention": gap vocabulary

"Archive", "store", and "retain" are gap verbs in B38 — not in any predicate lexicon.

| Sentence | PredicateSet |
|---|---|
| "Archive the signed artifacts." | [] → knowledge_edge |
| "Store the SBOM for 7 years." | [] → knowledge_edge |
| "Retain the audit trail." | [] → knowledge_edge |
| "Review the archived artifacts." | ["QA"] (review fires QA) |
| "Document the archival policy criteria." | ["DOCUMENT"] (document fires; criteria consolidates) |
