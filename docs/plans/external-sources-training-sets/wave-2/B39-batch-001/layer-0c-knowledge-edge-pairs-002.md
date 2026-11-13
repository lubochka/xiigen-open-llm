# Layer 0-C — Knowledge Edge Pairs (002)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-002-001 — TEST trigger edges: test-writing verbs

Trigger vocabulary → TEST predicate:
- write tests / write test cases / write unit tests / write E2E tests / write smoke tests / write integration tests
- create tests / create test cases / create test scenarios
- add tests / add assertions / add test coverage
- assert (in test context — "assert that X renders") / validate (in test assertion context)
- implement tests (not "implement the feature")

Gap vocabulary (do NOT fire TEST):
- run tests / execute tests / check tests / review tests (observational/operational)
- automate (alone — gap verb)
- framework names alone: Playwright, Cypress, Selenium, WebDriver, TestCafe
- set up testing / configure test suite (operational)

---

## KE-002-002 — UI trigger edges: component vocabulary

Trigger vocabulary → UI predicate:
- button / button click / button hover / button state / button variant
- form / form input / form field / form control / form validation (UI context)
- modal / dialog / overlay / popup / drawer
- dropdown / select / combobox / listbox
- checkbox / radio button / toggle / switch
- tab / tab panel / tab navigation
- carousel / slider / image gallery

Gap vocabulary (do NOT fire UI):
- dashboard (confirmed gap across B32, B33, B39)
- API response / backend service / database
- infrastructure / deployment / configuration

---

## KE-002-003 — UI trigger edges: layout and styling vocabulary

Trigger vocabulary → UI predicate:
- layout / grid / flexbox / column / row
- responsive / breakpoint / viewport / mobile / tablet / desktop
- CSS / style / stylesheet / theme / color / typography / font
- z-index / position / overflow / scroll / sticky
- animation / transition / keyframe / hover state / focus state / active state
- skeleton loader / loading spinner / progress bar
- sidebar / header / footer / nav / navigation / breadcrumb / pagination
- tooltip / popover
