# Layer 0-C — Knowledge-Edge Pairs (059)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-059-001 — DOCUMENT lexicon additions for 037-060

New DOCUMENT-triggering constructs confirmed in the 037-060 range:

| Construct | DOCUMENT trigger | Qualifier |
|---|---|---|
| Document the [X] | "document" verb | any document noun |
| Create the [X] | "create" in document context | templates, policies |
| [X] criteria | "criteria" (rule 3 absolute) | with document verb |
| [X] policy | "policy" as document artifact | with document verb |
| [X] requirements | "requirements" as document artifact | with document verb |
| [X] notice | "notice" as document artifact | with document verb |
| [X] advisory | "advisory" as document artifact | with document verb |
| [X] report | "report" as document artifact | with document verb |
| [X] guide | "guide" as document artifact | with document verb |

---

## B38-0C-059-002 — Document artifact nouns confirmed in 037-060

These nouns, when preceded by document verb, fire DOCUMENT:
changelog, release note, migration guide, upgrade guide, deprecation notice, API breaking change document, backward compatibility note, client communication plan, internal announcement, security advisory, CVE notification, provenance policy, SBOM criteria document, signing policy, attestation policy, verification criteria, supply chain criteria, compliance requirements, auditability criteria, gate criteria, regulatory submission, final release report

---

## B38-0C-059-003 — Document artifact nouns that do NOT independently fire DOCUMENT

The same nouns above are context only when no document verb is present.
DOCUMENT fires only when the document verb ("document", "create" in document context) is the action.

| Sentence | PredicateSet |
|---|---|
| "The compliance requirements are defined." | [] → knowledge_edge |
| "The security advisory was issued." | [] → knowledge_edge (issued = gap) |
| "The attestation policy is in place." | [] → knowledge_edge |
| "Document the compliance requirements." | ["DOCUMENT"] |
