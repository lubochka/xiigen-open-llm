# Layer 0-A — Compiler Pairs (060)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-060-001 — Complete factory reference: Round 3 topics predicate summary

A: Summarize the TEST trigger pattern confirmed across Round 3 (AdminPanel through WASM) topics.
B: TEST fires consistently on: write E2E tests/scenarios/cases/coverage, create test assertions/scenarios, add test coverage, assert [UI component], validate [metric/state], verify [behavior/component]. No new TEST triggers were introduced in Round 3. The gap verb set remains unchanged: run, execute, automate, scan, configure, monitor, track, describe, measure, load, register, subscribe, schedule.

---

## C0A-060-002 — Complete factory reference: Round 3 UI vocabulary additions

A: What UI vocabulary tokens appeared for the first time in Round 3 (037–059)?
B: Tokens confirmed in Round 3 that may not have appeared in 025–036: sidebar (037), navigation/nav (037, 038), banner (047, 055, 057, 058), skeleton (049, 054). These join the already-established: button, form, modal, page, input, screen, badge, toggle, panel, checkbox, dropdown, component, accordion, toast. All are in the UI lexicon.

---

## C0A-060-003 — Complete factory reference: technology names that are gap

A: Which technology/API/framework names appeared in Round 3 and are confirmed gap?
B: All gap: Google (OAuth provider), Stripe (payment provider), Playwright (test framework), Cypress (test framework), IntersectionObserver (browser API), IndexedDB (browser API), WASM/WebAssembly (technology), PWA (Progressive Web App — tech descriptor), service worker (browser technology). None fire any predicate.

---

## C0A-060-004 — Complete factory reference: IMPLEMENT triggers in Round 3

A: What IMPLEMENT trigger patterns appeared in Round 3?
B: All follow the "build/implement/create/add [feature noun]" template: "build the admin panel component", "implement Google OAuth integration", "implement the service worker", "implement push notification support", "implement WASM module integration", "implement lazy loading", "implement IndexedDB caching", "implement PWA install capability". All confirm: the feature noun (component, integration, service, support, capability, feature) is the deciding factor.

---

## C0A-060-005 — Complete factory reference: deduplication rule final statement

A: State the deduplication rule as a complete algorithm.
B: For each predicate type (TEST, UI, QA, IMPLEMENT, DOCUMENT, ARCHITECTURE, REFACTOR): scan the entire request for any vocabulary trigger for that predicate. If one or more triggers found → predicate fires exactly once. If zero triggers found → predicate does not fire. Apply independently for each predicate. No predicate can fire more than once per request regardless of trigger count. This rule applies to all 60 topic files across all three rounds of B39.
