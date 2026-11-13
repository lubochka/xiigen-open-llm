# Layer 0-C — Knowledge-Edge Pairs (030)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-030-001 — ChangelogAutomation context boundary

### ChangelogAutomation terms: context only (no predicate)
| Term | Category | Notes |
|---|---|---|
| changelog | Document noun | No predicate |
| commit message | Source noun | No predicate |
| release history | History noun | No predicate |
| diff | Technical noun | No predicate |
| tag (noun) | Label noun | No predicate |
| entry | Content noun | No predicate |
| format | Format noun | No predicate |

### ChangelogAutomation inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Generate the changelog from commit messages." | "Generate" not in lexicon → [] |
| "Automate the changelog production." | "Automate" not in lexicon → [] |
| "Format the changelog for release." | "Format" not in lexicon → [] |

---

## B38-0C-030-002 — ReleaseNoteTemplate context boundary

### ReleaseNoteTemplate terms: context only
| Term | Category | Notes |
|---|---|---|
| release note | Document noun | No predicate |
| template | Document structure | No predicate |
| format | Template type | No predicate |
| section | Template part | No predicate |
| header | Template element | No predicate |
| placeholder | Template element | No predicate |

### ReleaseNoteTemplate inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Distribute the release note template to teams." | "Distribute" not in lexicon → [] + gap: distribute |
| "Format the release note template." | "Format" not in lexicon → [] |
| "Prepare the release note template for v3." | "Prepare" not in lexicon → [] |

---

## B38-0C-030-003 — Changelog and release note ambiguity: "create" vs. "generate"

"Create" fires DOCUMENT in document-production context.
"Generate" is gap — automation action not in lexicon.

| Sentence | PredicateSet |
|---|---|
| "Create the release note template." | ["DOCUMENT"] ("create" fires DOCUMENT) |
| "Generate the release note template." | [] ("generate" is gap → knowledge_edge) |
| "Build the changelog generation service." | ["IMPLEMENT"] ("build" fires IMPLEMENT — rule 6; "service" fires IMPLEMENT — rule 2) |
| "Review the generated changelog." | ["QA"] ("review" fires QA) |
