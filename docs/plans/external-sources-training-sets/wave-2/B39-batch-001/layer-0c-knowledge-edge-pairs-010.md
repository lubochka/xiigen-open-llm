# Layer 0-C — Knowledge Edge Pairs (010)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-010-001 — UI fires on "responsive" vocabulary comprehensively

Trigger → UI:
- responsive design / responsive layout / responsive component
- mobile-first / mobile breakpoint / tablet view / desktop view
- fluid grid / flexible layout
- viewport / viewport width / viewport height
- @media / media query (CSS context)
- retina / HiDPI / pixel density (display context)

---

## KE-010-002 — UI fires on "dark mode" and "theme switching" vocabulary

"dark mode" / "dark theme" / "light theme" → UI
"theme switching" / "theme toggle" → UI
"color scheme" / "color palette" / "brand colors in UI" → UI
"CSS custom properties" / "CSS variables for theming" → UI

---

## KE-010-003 — TEST fires on "cover" and "test coverage" in writing context

"add test coverage for the form validation" → TEST
"increase E2E test coverage" → TEST
"write tests to cover the edge cases" → TEST
Gap: "the test coverage is 80%" (observation) → gap
Gap: "review test coverage in the report" → gap

---

## KE-010-004 — DOCUMENT fires on "document" verb in UI context

"document the component API" → DOCUMENT+UI (if UI vocabulary present)
"write documentation for the design system" → DOCUMENT+UI
"add JSDoc to the button component" → DOCUMENT+UI
Gap: "write comments" alone → not DOCUMENT (inline code comments are not documentation artifact)
Rule: "document" as verb fires DOCUMENT. UI vocabulary co-present fires UI.

---

## KE-010-005 — SECURITY in UI context

"sanitize user input in the form" → SECURITY+UI
"prevent XSS in rendered HTML" → SECURITY+UI (if form/render vocabulary present)
"validate input to prevent injection" → SECURITY+TEST (validate in security test context)
Rule: Security vocabulary (sanitize, XSS, injection, CSP) fires SECURITY. UI vocabulary co-present fires UI.
