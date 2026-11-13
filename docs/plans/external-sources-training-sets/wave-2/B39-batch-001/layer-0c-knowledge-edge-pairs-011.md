# Layer 0-C — Knowledge Edge Pairs (011)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-011-001 — "component" disambiguation

"button component" → UI (UI component vocabulary)
"service component" / "backend component" → not UI (architectural vocabulary → ARCHITECTURE)
"React component" / "Vue component" → UI (frontend component)
"microservice component" → ARCHITECTURE (not UI)
Rule: "component" fires UI when prefixed with UI element names or frontend framework context.

---

## KE-011-002 — "render" disambiguation

"render the button correctly" → UI (UI rendering vocabulary)
"write tests to verify render output" → TEST+UI
"render performance" → UI (rendering is UI context)
"render a PDF" → not UI (document generation, not UI rendering)
"server-side render" → UI (SSR is UI architecture pattern)
Rule: "render" fires UI when in browser/component display context.

---

## KE-011-003 — "click" and "hover" as TEST trigger edges

"write tests for the click event" → TEST+UI ("click" is UI vocabulary; "write tests" is test-writing verb)
"click on the button" alone (user action description) → may fire UI if component vocabulary present
"test click behavior" → TEST+UI ("test" as verb/noun in test-writing context)
Rule: "click" / "hover" fire UI via interaction vocabulary. TEST fires only when test-writing verb is present.

---

## KE-011-004 — "interaction" vocabulary

"interaction test" / "interaction testing" (in write-tests context) → TEST+UI
"interaction state" / "hover interaction" / "click interaction" (UI description) → UI
"user interaction" (behavioral description) → UI (if UI vocabulary co-present)
Rule: "interaction" alone → UI if UI vocabulary present. TEST requires test-writing verb.

---

## KE-011-005 — "layout" fires UI, "system layout" fires ARCHITECTURE

"page layout" / "grid layout" / "flex layout" → UI
"microservices layout" / "system architecture layout" → ARCHITECTURE
"layout component" / "layout wrapper" → UI
Rule: "layout" fires UI for visual/CSS layout; fires ARCHITECTURE for system topology descriptions.
