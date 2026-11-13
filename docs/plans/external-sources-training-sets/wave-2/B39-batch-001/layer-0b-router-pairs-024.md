# Layer 0-B — Router Pairs (024)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-024-001 — Complete routing reference: TEST sub-phase disambiguation

A: How does TEST route to 5.1 vs 5.3?
B: TEST → 5.3 when: e2e, end-to-end, browser, UI test, visual regression, accessibility test, cross-browser, snapshot, interaction test, acceptance test, Playwright/Cypress context (even though framework names are gap, they provide disambiguation context).
TEST → 5.1 when: unit test, integration test, pure function, service, repository, logic vocabulary without UI component terms.
Default when ambiguous: 5.3.

---

## C0B-024-002 — Complete routing reference: QA sub-phase disambiguation

A: How does QA route to 7.1 vs 7.3?
B: QA → 7.3 when: feature acceptance, gate before merge, sign-off, review, QA the results, QA the suite (default behavior for all B39 scenarios).
QA → 7.1 when: release cut, staging deployment, pre-release, pipeline, production deployment vocabulary is present.
Rule: 7.3 is the default QA sub-phase in B39. 7.1 requires explicit release/staging vocabulary.

---

## C0B-024-003 — Complete routing reference: UI always routes to 1.5

A: Does UI ever route to a sub-phase other than 1.5?
B: No. UI predicate always routes to sub-phase 1.5 (UI/UX Prototyping and Component Design) regardless of surrounding context — testing, implementation, documentation, or QA context. This is a hard deterministic rule. No exception.

---

## C0B-024-004 — Complete routing reference: TransitionType by predicate combination

A: What TransitionType results from TEST+UI+QA (no IMPLEMENT)?
B: TransitionType: documentation. Rule: TEST+UI without IMPLEMENT → documentation (testing produces documentation artifacts, not feature additions). QA does not change TransitionType. IMPLEMENT present → overrides to feature_addition. TEST alone → documentation. UI alone → documentation. QA alone → no contribution.

---

## C0B-024-005 — Complete routing reference: PrimarySubPhaseId determination

A: When TEST+UI+QA all fire, what is the PrimarySubPhaseId?
B: PrimarySubPhaseId is the sub-phase of the first-firing or highest-priority predicate. Ordering: IMPLEMENT (4.1) > QA (7.3) > TEST (5.3) > UI (1.5). For TEST+UI+QA without IMPLEMENT: QA fires last (QA always executes last), so PrimarySubPhaseId follows the primary action predicate — TEST → PrimarySubPhaseId: 5.3. AllSubPhaseIds: [5.3, 1.5, 7.3].
