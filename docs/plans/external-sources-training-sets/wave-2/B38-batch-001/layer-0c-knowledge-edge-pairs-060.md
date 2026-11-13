# Layer 0-C — Knowledge-Edge Pairs (060)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-060-001 — Complete knowledge-edge reference: 030-060 summary

This file is a complete knowledge-edge reference for the 030-060 extension batch.

### Knowledge-edge confirmed scenarios by topic
| Topic | Representative knowledge_edge input |
|---|---|
| ChangelogAutomation | "Generate the changelog." |
| ReleaseNoteTemplate | "Distribute the release note template." |
| MigrationGuide | "Prepare the migration guide." |
| UpgradeGuide | "Distribute the upgrade guide." |
| DeprecationNotice | "Send the deprecation notice." |
| APIBreakingChange | "Introduce the API breaking change." |
| BackwardCompatibilityNote | "Ensure backward compatibility." |
| ClientCommunication | "Send the client communication." |
| InternalAnnouncement | "Post the internal announcement." |
| SecurityAdvisory | (no pure knowledge_edge — SECURITY always fires from "security") |
| CVENotification | (no pure knowledge_edge — SECURITY always fires from CVE) |
| ArtifactProvenanceTracking | "Track artifact provenance." |
| SBOMGeneration | "Generate the SBOM." |
| SignedArtifact | "Sign the release artifact." |
| AttestationPolicy | "Issue the attestation." |
| ProvenanceVerification | "Check the provenance chain." |
| SupplyChainRelease | "Coordinate the supply chain release." |
| ComplianceRelease | "Ensure compliance requirements are met." |
| AuditableRelease | "Configure the release for auditability." |
| GatedRelease | "Configure the gated release pipeline." |
| RegulatorySubmission | "Submit the regulatory package." |
| FinalReleaseReport | "Archive the final release report." |

---

## B38-0C-060-002 — Anti-corruption: SECURITY-domain scenarios have no knowledge_edge path

SecurityAdvisory and CVENotification scenarios always produce at least ["SECURITY"] because:
- "security" as a standalone noun fires SECURITY
- "CVE" as a domain identifier fires SECURITY
- "vulnerability" fires SECURITY

There is no input containing only "security advisory" or "CVE" vocabulary that yields PredicateSet: [].

---

## B38-0C-060-003 — 12 teaching points for knowledge-edge in 030-060

1. Gap verbs (generate, automate, distribute, send, post, publish, track, store, coordinate, submit, archive, configure, ensure, introduce) produce knowledge_edge when they are the only action.
2. "Service" fires IMPLEMENT regardless of the service type — changelog service, attestation service, gate service, reporting service.
3. "Build" fires IMPLEMENT regardless of the surrounding domain — provenance, compliance, regulatory.
4. "Criteria" fires DOCUMENT only when a document verb is present — never standalone.
5. "Security" fires SECURITY standalone — SecurityAdvisory and CVE scenarios always yield at least ["SECURITY"].
6. "Audit" fires QA+SECURITY (rule 19) in all contexts.
7. "Policy", "requirements", "notice", "advisory", "guide" are document artifact nouns — context only without document verb.
8. "Review", "approve", "verify", "validate", "sign-off", "compliance", "inspect", "quality" fire QA in all scenarios.
9. "Publish", "distribute", "send", "post", "announce" are gap verbs in all B38 contexts.
10. "SBOM", "attestation", "provenance", "supply chain", "gated", "regulatory" are context nouns — no predicate fires from them alone.
11. TransitionType = feature_addition if and only if IMPLEMENT is in PredicateSet.
12. QA is always last in multi-predicate routing.
