# Layer 0-C — Knowledge Edge Pairs (007)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-007-001 — UI fires on "drag and drop" interaction vocabulary

"drag and drop" → UI (interaction pattern vocabulary)
"draggable" / "droppable" / "drag handle" → UI
"file upload via drag and drop" → UI + IMPLEMENT (if building it)
Gap: "drag to reorder database records" → context-dependent; UI fires only if UI-element reordering is described.

---

## KE-007-002 — UI fires on "accordion" and "collapsible" vocabulary

"accordion" / "accordion panel" / "expand/collapse" in UI context → UI
"collapsible sidebar" / "collapsible section" → UI
Gap: "collapse the data tree" (abstract data structure) → not UI without explicit UI context.

---

## KE-007-003 — UI fires on "notification toast" vocabulary

"toast notification" / "notification toast" / "snackbar" / "banner" → UI
"success toast" / "error toast" / "toast message" → UI
"alert banner" / "info banner" → UI

---

## KE-007-004 — UI fires on "progress bar" and "stepper" vocabulary

"progress bar" / "progress indicator" / "step indicator" / "stepper" → UI
"upload progress" / "form step progress" → UI
"progress tracker" → UI

---

## KE-007-005 — Gap vocabulary confirmed for B39

Confirmed gap (never fire any predicate alone):
- automate / automation (without QA abbreviation or test-writing verb)
- framework names: Playwright, Cypress, Selenium, TestCafe, Puppeteer
- configure / set up / install / run (operational verbs)
- screenshot / compare screenshots (observational)
- performance profiling / load testing / browser compatibility (without test-writing verb)
- dashboard (UI confirmed gap)
- check / review / look at / inspect (observational verbs without QA abbreviation)
