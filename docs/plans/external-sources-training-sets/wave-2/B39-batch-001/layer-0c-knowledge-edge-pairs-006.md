# Layer 0-C — Knowledge Edge Pairs (006)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-006-001 — UI fires on "icon" and "image" vocabulary

"icon" → UI (UI element vocabulary)
"image" + alt/src/render context → UI
"SVG icon" → UI
"loading spinner icon" → UI
Gap: "image upload to S3" → not UI (infrastructure context)
Rule: icon/image fires UI when in visual/rendering context; not in data/storage context.

---

## KE-006-002 — UI fires on "scroll" and "sticky" vocabulary

"scroll" vocabulary: "scroll position", "infinite scroll", "scroll to top", "scroll indicator" → UI
"sticky" vocabulary: "sticky header", "sticky nav", "sticky sidebar" → UI
Gap: "scroll through logs" / "scroll database records" → not UI (data context)
Rule: scroll/sticky fire UI when applied to UI elements; not in data browsing context.

---

## KE-006-003 — UI fires on "focus" and "blur" vocabulary

"focus" in UI context: "focus state", "focus ring", "input focus", "focus trap in modal" → UI
"blur" in UI context: "blur effect", "backdrop blur", "input blur event" → UI
Gap: "focus on the main task" (metaphorical) → not UI
Rule: focus/blur fire UI when applied to UI elements or states.

---

## KE-006-004 — TEST fires on "cover" / "coverage" in test context

"add test coverage for the modal" → TEST (test-writing intent)
"increase test coverage" → TEST (test-writing intent implied)
Gap: "review test coverage report" → observational gap (no test-writing verb)
Rule: "coverage" fires TEST when preceded by "add", "increase", "improve", "write" (writing intent). Not when followed by "report", "view", "check" (observational).
