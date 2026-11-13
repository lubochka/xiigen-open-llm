# Layer 0-C — Knowledge-Edge Pairs (042)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-042-001 — "criteria" triggers: all qualifiers confirmed for 037-060

"Criteria" fires DOCUMENT (rule 3) for all qualifiers in the 037-060 range.

| Qualifier + criteria | DOCUMENT trigger | Notes |
|---|---|---|
| changelog entry criteria | Yes | file 037 |
| release note entry criteria | Yes | file 038 |
| migration guide criteria | Yes | file 039 |
| upgrade readiness criteria | Yes | file 040 |
| deprecation acceptance criteria | Yes | file 041 |
| breaking change acceptance criteria | Yes | file 042 |
| backward compatibility acceptance criteria | Yes | file 043 |
| communication release criteria | Yes | file 044 |
| announcement template criteria | Yes | file 045 |
| patch release criteria | Yes | file 046 |
| vulnerability criteria | Yes | file 047 |
| provenance criteria | Yes | file 049 |
| SBOM criteria | Yes | file 050 |
| signing policy criteria | Yes | file 051 |
| attestation criteria | Yes | file 052 |
| verification criteria | Yes | file 053 |
| supply chain criteria | Yes | file 054 |
| compliance criteria | Yes | file 055 |
| auditability criteria | Yes | file 056 |
| gate exit criteria | Yes | file 057 |
| regulatory criteria | Yes | file 058 |
| report criteria | Yes | file 059 |

Gate B assertion: For any of the above qualifiers, "criteria" in input → DOCUMENT in PredicateSet.

---

## B38-0C-042-002 — No qualifier suppresses "criteria" → DOCUMENT (rule 3 absolute)

Rule 3 is absolute: no qualifier (changelog, SBOM, regulatory, provenance, gate, compliance, etc.) can suppress the DOCUMENT predicate when "criteria" is present with a DOCUMENT-producing verb.

Anti-corruption: if the model returns PredicateSet: [] for "Document the regulatory submission criteria", that is v6-01 style DOCUMENT suppression — incorrect.

---

## B38-0C-042-003 — "criteria" without "document" verb

"Criteria" alone (without a DOCUMENT-triggering verb) does NOT fire DOCUMENT independently.
The verb "document" (or "create" in document-production context) must be present.

| Sentence | PredicateSet |
|---|---|
| "Review the gate criteria and approve." | ["QA"] (criteria without document verb → no DOCUMENT; review+approve → QA) |
| "Document the gate criteria." | ["DOCUMENT"] (document verb fires; criteria consolidates) |
| "The gate exit criteria are defined." | [] → knowledge_edge (no action verb in predicate lexicon) |
