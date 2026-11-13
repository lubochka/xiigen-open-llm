# Layer 0-C — Knowledge-Edge Pairs (056)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-056-001 — Provenance and attestation: full disambiguation table

This file provides the authoritative disambiguation table for all provenance/attestation/signing scenarios.

| Input | PredicateSet | Rule |
|---|---|---|
| "Generate the SBOM." | [] | generate gap |
| "Track artifact provenance." | [] | track gap |
| "Sign the artifact." | [] | sign (action) gap |
| "Issue the attestation." | [] | issue gap |
| "Check the provenance chain." | [] | check gap |
| "Verify the SBOM completeness." | ["QA"] | verify fires QA |
| "Review the provenance records." | ["QA"] | review fires QA |
| "Inspect the signed artifact." | ["QA"] | inspect fires QA |
| "Approve the attestation policy." | ["QA"] | approve fires QA |
| "Document the SBOM criteria." | ["DOCUMENT"] | document + criteria |
| "Document the attestation policy." | ["DOCUMENT"] | document + policy |
| "Build the SBOM generation service." | ["IMPLEMENT"] | build + service |
| "Build the attestation service." | ["IMPLEMENT"] | build + service |
| "Audit the SBOM components." | ["QA","SECURITY"] | audit rule 19 |
| "Audit the attestation security." | ["QA","SECURITY"] | audit rule 19 |

---

## B38-0C-056-002 — Anti-corruption: "sign" is never IMPLEMENT

"Sign" (action: to sign an artifact/document) is not in the IMPLEMENT lexicon.
Only "build" and "service" fire IMPLEMENT.

Corrupt: "Sign the release artifact." → ["IMPLEMENT"] (wrong)
Correct: "Sign the release artifact." → [] → knowledge_edge

---

## B38-0C-056-003 — Anti-corruption: "audit" always fires QA+SECURITY (rule 19)

Based on the established rule 19: "audit" co-fires QA and SECURITY in all B38 contexts.

Corrupt: "Audit the release process." → ["QA"] alone (wrong per rule 19)
Correct: "Audit the release process." → ["QA","SECURITY"]

This is the strict interpretation per rule 19 and file 054 section 003.
