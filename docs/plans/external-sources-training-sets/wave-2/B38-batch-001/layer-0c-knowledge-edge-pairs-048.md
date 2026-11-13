# Layer 0-C — Knowledge-Edge Pairs (048)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-048-001 — Consolidated knowledge-edge inputs for 037-048 batch

Complete list of knowledge_edge inputs confirmed in the 037-048 topics:

| Input | Gap verb | Notes |
|---|---|---|
| "Generate the changelog from commit messages." | generate | |
| "Automate the changelog production." | automate | |
| "Distribute the release note template to teams." | distribute | |
| "Prepare the release note template for v3." | prepare | |
| "Prepare the migration guide for v2." | prepare | |
| "Schedule the migration window." | schedule | |
| "Distribute the upgrade guide to customers." | distribute | |
| "Send the deprecation notice to subscribers." | send | |
| "Schedule the API deprecation for Q3." | schedule | |
| "Introduce the API breaking change in v3." | introduce | |
| "Announce the API breaking change." | announce | |
| "Ensure backward compatibility is maintained." | ensure | |
| "Send the client communication email." | send | |
| "Post the internal announcement." | post | |
| "Register the CVE with the national database." | register | (has ["SECURITY"] — not knowledge_edge) |

Note: "Register the CVE" returns ["SECURITY"] — not knowledge_edge, since CVE fires SECURITY.

---

## B38-0C-048-002 — Consolidated knowledge-edge inputs for 049-060 batch

| Input | Gap verb | Notes |
|---|---|---|
| "Track artifact provenance." | track | |
| "Generate the SBOM for the release artifact." | generate | |
| "Sign the release artifact." | sign (action) | |
| "Issue the attestation for the release artifact." | issue | |
| "Check the provenance chain." | check | |
| "Coordinate the supply chain release." | coordinate | |
| "Ensure the release meets all compliance requirements." | ensure | |
| "Configure the release pipeline for full auditability." | configure | |
| "Configure the gated release pipeline stages." | configure | |
| "Submit the regulatory package to the FDA." | submit | |
| "Archive the final release report." | archive | |

---

## B38-0C-048-003 — Boundary: these inputs are NOT knowledge_edge

| Input | PredicateSet | Reason |
|---|---|---|
| "Publish the security advisory." | ["SECURITY"] | "security" fires SECURITY; publish is gap |
| "Register the CVE." | ["SECURITY"] | CVE fires SECURITY; register is gap |
| "Document the compliance requirements." | ["DOCUMENT"] | document fires DOCUMENT |
| "Review the supply chain integrity." | ["QA"] | review fires QA |
