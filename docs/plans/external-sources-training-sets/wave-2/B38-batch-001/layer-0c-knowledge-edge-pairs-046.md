# Layer 0-C — Knowledge-Edge Pairs (046)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-046-001 — Regulatory and gated release: "clearance" and "authorization" are gaps

"Clearance" and "authorization" in regulatory context are status nouns — no predicate.
"Approval" fires QA (approve is in QA lexicon); "clearance" does not.

| Sentence | PredicateSet |
|---|---|
| "Receive regulatory clearance for the release." | [] → knowledge_edge ("receive" not in lexicon) |
| "Obtain FDA authorization." | [] → knowledge_edge ("obtain" not in lexicon) |
| "Approve the FDA submission." | ["QA"] ("approve" fires QA) |
| "Verify regulatory compliance and approve." | ["QA"] (verify + approve fire QA) |

---

## B38-0C-046-002 — "policy" as DOCUMENT artifact: full range

"Policy" fires DOCUMENT only when a DOCUMENT-producing verb is present.
"Policy" alone is a document artifact noun — context only.

| Sentence | PredicateSet |
|---|---|
| "Document the attestation policy." | ["DOCUMENT"] |
| "Create the signing policy." | ["DOCUMENT"] (create in document context fires) |
| "Review the attestation policy." | ["QA"] |
| "Configure the attestation policy." | [] → knowledge_edge (configure is gap) |
| "The attestation policy is in place." | [] → knowledge_edge (declarative, no action verb) |

---

## B38-0C-046-003 — "release" action verb is not a predicate trigger

"Release" used as an action verb ("release the software", "release the artifact") is not in any predicate lexicon.
"Release" as a noun ("the release", "a release", "release criteria") is context.

| Sentence | PredicateSet |
|---|---|
| "Release the signed artifact." | [] → knowledge_edge ("release" verb not in lexicon) |
| "Release the regulatory submission." | [] → knowledge_edge |
| "Approve the release." | ["QA"] ("approve" fires QA; "release" noun is context) |
| "Sign off on the release." | ["QA"] ("sign off" fires QA) |
