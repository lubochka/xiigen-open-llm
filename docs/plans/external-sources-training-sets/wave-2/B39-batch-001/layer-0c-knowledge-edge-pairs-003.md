# Layer 0-C — Knowledge Edge Pairs (003)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-003-001 — QA trigger edges in UI automation context

Trigger vocabulary → QA predicate (abbreviation gate):
- QA sign-off / QA gate / QA review / QA approval / QA check
- QA team / QA engineer / QA lead / QA tester / QA analyst
- QA environment / QA testing / QA harness / QA report / QA criteria
- "QA this" / "QA the build" / "QA it" (imperative verb)
- "QA is required" / "QA must approve" (subject position)
- "(QA)" parenthetical

Gap (do NOT fire QA):
- "quality assurance" spelled out → gap
- testing alone → TEST (not QA)
- approve alone / sign-off alone / clearance alone → gap

---

## KE-003-002 — Sub-phase disambiguation: TEST 5.1 vs 5.3

Routes to 5.1 (UnitTestingFlow51):
- "unit tests" keyword present
- pure function / utility / helper / calculator / service method (no UI/browser)
- Jest, Mocha unit context (but framework names alone are gap — verbs required)

Routes to 5.3 (E2ETestingFlow53):
- "E2E" / "end-to-end" keyword present
- "smoke tests" / "browser tests" / "integration tests" with UI/system context
- UI component vocabulary co-present (button, modal, form, etc.)
- "cross-browser" / "viewport" / "responsive" context
- "interaction tests" / "visual regression tests"

Default when ambiguous: 5.3 (E2E) when UI vocabulary co-present

---

## KE-003-003 — Sub-phase disambiguation: QA 7.1 vs 7.3

Routes to 7.1 (ReleaseGateFlow71):
- "release" + "pipeline" / "deployment" / "branch cut" / "production" context
- "staging deployment" / "release candidate" vocabulary
- "release gate" / "release cut"

Routes to 7.3 (QAGatingFlow73):
- "sign-off on feature/change/PR" vocabulary
- "QA gate" without release/deploy context
- "QA this" / "QA the feature" (bare feature acceptance)
- Default when no release vocabulary present
