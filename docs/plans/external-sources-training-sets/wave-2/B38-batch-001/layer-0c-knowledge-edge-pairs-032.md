# Layer 0-C — Knowledge-Edge Pairs (032)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-032-001 — DeprecationNotice context boundary

### DeprecationNotice terms: context only (no predicate)
| Term | Category | Notes |
|---|---|---|
| deprecation | Process noun | No predicate |
| deprecation notice | Notice noun | No predicate alone |
| sunset | Process noun | No predicate |
| end-of-life | Status noun | No predicate |
| legacy | Status adjective | No predicate |
| removal | Action noun | No predicate |
| API sunset | Event noun | No predicate |

### DeprecationNotice inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Send the deprecation notice to all API consumers." | "Send" not in lexicon → [] |
| "Schedule the API deprecation for Q3." | "Schedule" not in lexicon → [] |
| "Plan the sunset of the v1 API." | "Plan" not in lexicon → [] |

---

## B38-0C-032-002 — APIBreakingChange context boundary

### APIBreakingChange terms: context only
| Term | Category | Notes |
|---|---|---|
| breaking change | Change noun | No predicate |
| API breaking change | Change descriptor | No predicate |
| backward incompatible | Compatibility adjective | No predicate |
| interface change | Change type | No predicate |
| contract change | Change type | No predicate |
| v3 breaking | Version context | No predicate |

### APIBreakingChange inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Introduce the breaking change in v3." | "Introduce" not in lexicon → [] |
| "Release the API breaking change." | "Release" (as action verb) not in predicate lexicon → [] |
| "Announce the API breaking change." | "Announce" not in lexicon → [] |

---

## B38-0C-032-003 — Deprecation and breaking change: "release" ambiguity

"Release" as a noun (the release, a release) is context — no predicate.
"Release" as an action verb (release the software) is not in the predicate lexicon — no predicate.
QA fires from: review, approve, verify, validate, sign-off, audit, compliance, inspect — not from "release" verb.

| Sentence | PredicateSet |
|---|---|
| "Release the deprecation notice." | [] ("release" verb not in lexicon) |
| "Approve the deprecation release." | ["QA"] ("approve" fires QA) |
| "Document the breaking change." | ["DOCUMENT"] ("document" fires DOCUMENT) |
| "Review the breaking change impact." | ["QA"] ("review" fires QA) |
