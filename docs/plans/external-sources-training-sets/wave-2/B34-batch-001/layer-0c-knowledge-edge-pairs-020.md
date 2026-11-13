# Layer 0-C — Knowledge Edge Pairs (020)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-020-001 — Guidance + IMPLEMENT embedded: complete rule reference

IMPLEMENT fires when: build/implement/create + software noun embedded in the requester's action.
IMPLEMENT does NOT fire when: guidance-only vocabulary used or "teach/coach on how to build" pattern.

| Request | IMPLEMENT fires? | Reason |
|---------|-----------------|--------|
| "Coach through building the service" | yes | embedded "building service" → IMPLEMENT |
| "Mentor through implementing the pipeline" | yes | embedded "implementing pipeline" → IMPLEMENT |
| "Guide through refactoring the module" | yes | refactoring = IMPLEMENT sub-type |
| "Coach on how to build APIs" | no | "teach to X" pattern → gap |
| "Advise on building best practices" | no | advisory without embedded build action |
| "Help understand how to build" | no | learning intent → gap |
| "Facilitate the team building the service" | no | team builds = third-party action |

---

## KE-020-002 — "refactor" inside guidance frame → IMPLEMENT (refactoring sub-type)

"Guide through refactoring the auth module" → IMPLEMENT + TransitionType: refactoring
"Coach through cleaning up the legacy code" → IMPLEMENT + TransitionType: refactoring (if clean-up = refactoring)
"Mentor through extracting the repository layer" → IMPLEMENT (architectural refactoring)

---

## KE-020-003 — "coach on how to build" vs "coach through building"

"Coach on how to build X" → gap ("teach to X" pattern; infinitive = learning target)
"Coach through building X" → IMPLEMENT fires (participial form = active embedded action)
"Mentor on implementation patterns" → gap (advisory on patterns, no build verb)
"Mentor through implementing X" → IMPLEMENT fires (implementing = participial active form)

Key: participial "building/implementing" = active embedded action; infinitive "to build/to implement" = learning target (gap).
