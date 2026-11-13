# Layer 0-C — Knowledge-Edge Pairs (039)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-039-001 — GatedRelease context boundary

### GatedRelease terms: context only (no predicate)
| Term | Category | Notes |
|---|---|---|
| gate | Process noun | No predicate alone |
| gated release | Release type | No predicate |
| release gate | Gate noun | No predicate |
| gate condition | Condition noun | No predicate |
| gate threshold | Threshold noun | No predicate |
| pre-release gate | Gate type | No predicate |
| release blocker | Blocker noun | No predicate |

### GatedRelease inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Configure the gated release pipeline stages." | "Configure" not in lexicon → [] |
| "Set the gate threshold for the release." | "Set" not in lexicon → [] |
| "Open the release gate for production." | "Open" not in lexicon → [] |

---

## B38-0C-039-002 — RegulatorySubmission context boundary

### RegulatorySubmission terms: context only
| Term | Category | Notes |
|---|---|---|
| regulatory submission | Document noun | No predicate alone |
| FDA (Food and Drug Administration) | Regulator noun | No predicate |
| submission package | Package noun | No predicate |
| regulatory body | Authority noun | No predicate |
| dossier | Document noun | No predicate |
| filing | Action noun | No predicate |
| clearance | Status noun | No predicate |

### RegulatorySubmission inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Submit the regulatory package to the FDA." | "Submit" not in lexicon → [] |
| "File the regulatory dossier." | "File" not in lexicon → [] |
| "Apply for regulatory clearance." | "Apply" not in lexicon → [] |

---

## B38-0C-039-003 — "configure", "set", "open", "submit", "file" are gap verbs

All five are operations verbs not in any predicate lexicon for B38.

| Sentence | PredicateSet |
|---|---|
| "Configure the release gate." | [] → knowledge_edge |
| "Submit the regulatory submission." | [] → knowledge_edge |
| "File the FDA dossier." | [] → knowledge_edge |
| "Review the gated release conditions." | ["QA"] ("review" fires QA) |
| "Document the regulatory submission criteria." | ["DOCUMENT"] ("document" fires; "criteria" consolidates) |
