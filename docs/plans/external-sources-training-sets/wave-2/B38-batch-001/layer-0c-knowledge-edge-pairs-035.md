# Layer 0-C — Knowledge-Edge Pairs (035)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-035-001 — ArtifactProvenanceTracking context boundary

### ArtifactProvenanceTracking terms: context only (no predicate)
| Term | Category | Notes |
|---|---|---|
| provenance | Origin noun | No predicate alone |
| artifact | Release noun | No predicate |
| artifact chain | Chain noun | No predicate |
| provenance record | Record noun | No predicate |
| origin | Source noun | No predicate |
| lineage | History noun | No predicate |
| build metadata | Metadata noun | No predicate (metadata is context) |

### ArtifactProvenanceTracking inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Track artifact provenance." | "Track" not in lexicon → [] |
| "Record the artifact lineage." | "Record" not in lexicon → [] |
| "Store provenance metadata." | "Store" not in lexicon → [] |

Note: "Track artifact provenance throughout the build pipeline" fires IMPLEMENT because "build" fires (rule 6).

---

## B38-0C-035-002 — SBOM (Software Bill of Materials) context boundary

### SBOM terms: context only
| Term | Category | Notes |
|---|---|---|
| SBOM | Inventory noun | No predicate alone |
| software bill of materials | Inventory noun | No predicate |
| component | Inventory item | No predicate |
| dependency | Technical noun | No predicate |
| license | Compliance noun | No predicate |
| package | Software unit | No predicate |

### SBOM inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Generate the SBOM for the release." | "Generate" not in lexicon → [] |
| "List all SBOM components." | "List" not in lexicon → [] |
| "Export the SBOM to JSON." | "Export" not in lexicon → [] |

---

## B38-0C-035-003 — "track", "record", "store", "generate" are gap verbs

All four are operations verbs not in any predicate lexicon for B38.
Surrounding SBOM/provenance nouns do not elevate them.

| Sentence | PredicateSet |
|---|---|
| "Track the artifact provenance chain." | [] → knowledge_edge |
| "Generate the SBOM." | [] → knowledge_edge |
| "Review the artifact provenance records." | ["QA"] ("review" fires) |
| "Build the provenance tracking service." | ["IMPLEMENT"] ("build" + "service" fire) |
| "Document the SBOM criteria." | ["DOCUMENT"] ("document" fires; "criteria" consolidates) |
