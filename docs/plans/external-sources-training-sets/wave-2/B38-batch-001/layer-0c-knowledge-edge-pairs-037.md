# Layer 0-C — Knowledge-Edge Pairs (037)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-037-001 — ProvenanceVerification context boundary

### ProvenanceVerification terms: context only (no predicate)
| Term | Category | Notes |
|---|---|---|
| provenance verification | Process noun | No predicate alone |
| integrity check | Check noun | No predicate |
| hash verification | Verification type | No predicate |
| checksum | Integrity noun | No predicate |
| digest | Integrity noun | No predicate |
| provenance chain | Chain noun | No predicate |

### ProvenanceVerification inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Check the provenance chain." | "Check" not in lexicon → [] |
| "Validate the artifact hash." | Wait — "validate" fires QA. So: ["QA"] |
| "Compare artifact checksums." | "Compare" not in lexicon → [] |

Note: "validate" fires QA even in hash/provenance context. "check" alone does not.

---

## B38-0C-037-002 — SupplyChainRelease context boundary

### SupplyChainRelease terms: context only
| Term | Category | Notes |
|---|---|---|
| supply chain | Logistics noun | No predicate |
| vendor | Stakeholder noun | No predicate |
| third-party | Stakeholder adjective | No predicate |
| dependency chain | Chain noun | No predicate |
| integration | Process noun | No predicate |
| coordination | Process noun | No predicate |

### SupplyChainRelease inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Coordinate the supply chain release." | "Coordinate" not in lexicon → [] |
| "Align vendor releases for the Q3 release." | "Align" not in lexicon → [] |
| "Integrate third-party components for the release." | "Integrate" not in lexicon → [] |

---

## B38-0C-037-003 — "check" vs. "verify": only "verify" fires QA

"Check" is not in the predicate lexicon for B38.
"Verify" is in the QA lexicon and always fires QA.

| Sentence | PredicateSet |
|---|---|
| "Check the artifact provenance." | [] → knowledge_edge |
| "Verify the artifact provenance." | ["QA"] ("verify" fires QA) |
| "Verify the supply chain integrity and sign off." | ["QA"] (verify + sign-off both fire QA; consolidate) |
| "Check the supply chain integrity." | [] → knowledge_edge |
