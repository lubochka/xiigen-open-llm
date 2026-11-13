# Layer 0-C — Knowledge Edge Pairs (024)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E024-001 — Complete gap vocabulary for TEST in B39

A: What vocabulary NEVER fires TEST in B39, even in test-adjacent context?
B: Gap vocabulary for TEST: automate, automated, run, execute, trigger, schedule, configure, deploy, check, review, monitor, observe, profile, scan, report, CI (Continuous Integration), pipeline, Cypress (framework name), Playwright (framework name), Selenium (framework name), TestCafe (framework name), axe-core (tool name), baseline (alone), snapshot (alone without test verb).
Teaching: TEST requires a test-writing verb + optionally a test noun. None of the above are test-writing verbs or test nouns by themselves.

---

## E024-002 — Complete gap vocabulary for UI in B39

A: What vocabulary looks like it should fire UI but does NOT?
B: Gap vocabulary for UI: dashboard, API, endpoint, service, controller, repository, database, model, schema, module, package, pipeline, workflow, process, CI/CD, microservice, Docker, Kubernetes, configuration, environment variable.
Teaching: These are backend/infrastructure terms. UI fires on front-end component and interaction vocabulary only.

---

## E024-003 — Complete gap vocabulary for QA in B39

A: What vocabulary looks like QA but does not fire the QA predicate?
B: Gap vocabulary for QA: quality, assurance, quality assurance (spelled out), quality check, quality review, review (standalone), acceptance (standalone), criteria, sign off (without "QA" abbreviation present), validate (standalone), compliance (standalone).
Teaching: QA fires ONLY when the two-letter abbreviation "QA" is literally present. No synonym, expansion, or near-synonym fires QA.

---

## E024-004 — Complete boundary: IMPLEMENT lexicon in B39

A: What exact verbs fire the IMPLEMENT predicate?
B: IMPLEMENT fires on: build, create, write, implement, add, develop, generate, scaffold, extend, code.
IMPLEMENT does NOT fire on: update, modify, change, refactor, configure, set up, deploy, run, install, setup.
Teaching: "write" fires both IMPLEMENT and TEST — context determines which is primary. In "write tests", TEST is primary. In "write a service", IMPLEMENT is primary. Both fire in "write tests" + "implement" contexts.

---

## E024-005 — Synthesis: deduplication rule for B39

A: If "button", "input", "form", "component", and "modal" all appear in one sentence, how many UI predicates fire?
B: One. Deduplication rule: Multiple vocabulary words from the same predicate lexicon → one predicate fires (deduplicated). "button + input + form + component + modal" → five UI triggers → one UI predicate. Similarly: "write tests", "assert", "validate", "verify" in one sentence → four TEST triggers → one TEST predicate.
Teaching: Predicates are counted by distinct predicate IDs, not by trigger occurrences.
