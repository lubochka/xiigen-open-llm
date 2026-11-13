# Layer 0-A — Compiler Pairs (030)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-030-001 — Complete QA reference for semantic versioning context

This file is a complete QA predicate reference for SemanticVersioning (SemVer) and CalendarVersioning (CalVer) release contexts.

### QA triggers in version release context
| Input term | Fires QA? | Notes |
|---|---|---|
| review | YES | Core QA lexicon |
| approve / approval | YES | Core QA lexicon |
| verify / verification | YES | Rule 20/21 |
| validate / validation | YES | Core QA lexicon |
| sign-off / sign off | YES | Rule 23 — absolute |
| quality | YES | Core QA lexicon |
| compliance | YES | Core QA lexicon |
| inspect | YES | Core QA lexicon |
| accept / acceptance | YES | Quality/gate context |
| audit | YES | + SECURITY if "security" co-occurs (rule 19) |
| version | NO | Versioning context word |
| tag | NO | Release labeling noun |
| bump | NO | SemVer operation noun |
| patch / minor / major | NO | Version increment labels |
| nightly / weekly / monthly | NO | Cadence context words |

---

## B38-0A-030-002 — Version-specific context vs predicate boundary

### Terms confirmed as context-only in version release sentences
| Term | Category | Predicate fires? |
|---|---|---|
| semantic version | versioning concept | NO |
| calendar version | versioning model | NO |
| release candidate (RC) | release artifact | NO |
| pre-release tag | release label | NO |
| stable release | release maturity | NO |
| hotfix tag | fix label | NO |
| patch release | maintenance release | NO |
| major release | version increment | NO |
| version number | identifier | NO |
| release series | cadence group | NO |

---

## B38-0A-030-003 — IMPLEMENT triggers in version/build contexts

| Input term | Fires IMPLEMENT? | Rule | Notes |
|---|---|---|---|
| service | YES | Rule 2 — ABSOLUTE | Even in "release the service" sentences |
| build | YES | Rule 6 — ABSOLUTE | "Build version", "build artifact", "build tag" all fire |
| pipeline | YES | Code artifact context | CI/CD version pipeline |
| implementation | YES | Code-authoring context | When writing the versioning logic |
| component | YES | Code-authoring context | Only when writing code, not reviewing |
| version bump script | YES | via "build" or "service" co-occurrence | Script itself does not fire; "service" or "build" must be present |

---

## B38-0A-030-004 — TransitionType for versioning scenarios

| PredicateSet | TransitionType | Notes |
|---|---|---|
| ["QA"] | no_contribution | Reviewing/approving version only |
| ["DOCUMENT"] | no_contribution | Documenting version criteria |
| ["QA","DOCUMENT"] | no_contribution | No IMPLEMENT |
| ["QA","IMPLEMENT"] | feature_addition | Build or service present |
| ["QA","IMPLEMENT","DOCUMENT"] | feature_addition | Build + document criteria |
| ["QA","IMPLEMENT","SECURITY"] | feature_addition | Build + security audit |
| [] | N/A (knowledge_edge) | Planning/scheduling only |
