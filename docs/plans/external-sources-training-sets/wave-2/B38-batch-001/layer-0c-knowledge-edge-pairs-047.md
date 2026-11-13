# Layer 0-C — Knowledge-Edge Pairs (047)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-047-001 — Multi-predicate knowledge-edge analysis: when QA is missing

These inputs contain IMPLEMENT or DOCUMENT or SECURITY but no QA trigger.
They are not knowledge_edge — they are valid multi-predicate sets without QA.

| Sentence | PredicateSet | Notes |
|---|---|---|
| "Build the SBOM service and document the SBOM criteria." | ["IMPLEMENT","DOCUMENT"] | No QA trigger |
| "Document the security advisory." | ["DOCUMENT","SECURITY"] | No QA trigger |
| "Build the signing service." | ["IMPLEMENT"] | No QA trigger |
| "Document the compliance requirements." | ["DOCUMENT"] | No QA trigger |

These are NOT knowledge_edge — they have predicates. Knowledge_edge is only PredicateSet: [].

---

## B38-0C-047-002 — True knowledge_edge vs. non-QA predicate sets

| Condition | Classification |
|---|---|
| PredicateSet: [] | knowledge_edge |
| PredicateSet: ["IMPLEMENT"] | FLOW_EXECUTING (one flow) |
| PredicateSet: ["DOCUMENT"] | FLOW_EXECUTING (one flow) |
| PredicateSet: ["SECURITY"] | FLOW_EXECUTING (one flow) |
| PredicateSet: ["DOCUMENT","SECURITY"] | FLOW_EXECUTING (two flows) |
| PredicateSet: ["IMPLEMENT","DOCUMENT"] | FLOW_EXECUTING (two flows) |

None of these are knowledge_edge — knowledge_edge means zero predicates only.

---

## B38-0C-047-003 — Gap vocabulary in 037-060 batch: summary table

| Gap verb | Scenario |
|---|---|
| generate | changelog, SBOM, signing key |
| automate | changelog |
| distribute | upgrade, client comm |
| send | deprecation, client comm |
| post | announcement |
| publish | security advisory, release notes |
| announce | deprecation |
| issue | attestation, certificate |
| register | CVE |
| submit | regulatory |
| file | regulatory, final report |
| archive | final report |
| configure | gate, compliance |
| coordinate | supply chain |
| track | provenance |
| store | provenance, report |
| introduce | API change |
| ensure | backward compat, compliance |
| maintain | backward compat |
| align | supply chain |
| integrate | supply chain |
| report (verb) | final report |
