# Layer 0-C — Knowledge-Edge Pairs (025)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-025-001 — SemanticVersioning: predicate vs context boundary

### Terms in SemanticVersioning context
| Term | Category | Predicate fires? | Notes |
|---|---|---|---|
| semantic version | versioning concept | NO | Context noun |
| version bump | SemVer operation | NO | Context noun |
| major / minor / patch | Version increment | NO | Context labels |
| SEMVER | Abbreviation | NO | Versioning system name |
| pre-release | Release stage | NO | Release label |
| stable | Release maturity | NO | Release descriptor |
| review | QA action | YES (QA) | Core QA trigger |
| approve | QA action | YES (QA) | Core QA trigger |
| verify | QA action | YES (QA) | Core QA trigger |
| build | Code authoring | YES (IMPLEMENT) | Rule 6 — absolute |
| service | Code reference | YES (IMPLEMENT) | Rule 2 — absolute |
| criteria | Documentation | YES (DOCUMENT) | Rule 3 — absolute |

---

## B38-0C-025-002 — Knowledge edge inputs for versioning

### Pure versioning inputs that produce knowledge_edge
| Input | Why knowledge_edge | PredicateSet |
|---|---|---|
| "Finalize the semantic version for Q3." | "Finalize" not in lexicon; "semantic version" is context | [] |
| "Publish the SemVer specification." | "Publish" not in lexicon; "specification" not document trigger | [] |
| "Tag the 2.0.0 release." | "Tag" not in lexicon (verb); "release" is context | [] |
| "Bump the patch version to 1.0.1." | "Bump" not in lexicon; "patch version" is context | [] |
| "Assign the CalVer date stamp." | "Assign" not in lexicon; "CalVer" is context | [] |

All above: ResultState == knowledge_edge, PredicateSet == []

---

## B38-0C-025-003 — Gap vs predicate for versioning-adjacent terms

| Term | Gap or predicate? | Notes |
|---|---|---|
| rollout | GAP (gap-project-delivery-rollout-golive-001) | Even in "version rollout" |
| deployment | GAP (rule 4) | "Deploy the versioned artifact" → GAP |
| launch | GAP (gap-project-delivery-launch-001) | "Launch 2.0.0" → GAP |
| go-live | GAP (gap-project-delivery-go-live-001) | Even in "2.0.0 go-live" |
| pipeline | IMPLEMENT (code artifact) | "Build version pipeline" → IMPLEMENT |
| registry | Context (no predicate) | "Publish to registry" → context |
| artifact | Context (no predicate) | "Version artifact" → context |
