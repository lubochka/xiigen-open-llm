# Layer 2 — Capability Gap Extended Pairs (012)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B34-gap-012 — QAGatingFlow73 nine-node: QA onboarding portal

Request: "QA the developer onboarding portal before the first cohort onboards next week."

### Flow: QAGatingFlow73 (QA predicate; sub-phase 7.3 — feature acceptance gate)

**Node 1**: Parse. "QA" abbreviation → QA predicate fires. "before cohort onboards" = feature acceptance gate → 7.3.
**Node 2**: Predicate set = ["QA"]. TransitionType: no contribution.
**Node 3**: QA scope: onboarding portal — registration, environment setup wizard, checklist completion, resource library access.
**Node 4**: QA gates: registration flow works, setup wizard completes all steps, checklist marks as complete, resources load.
**Node 5**: Arbiter: QAGatingArbiter73
**Node 6**: Gate 1 — Registration: new user registers successfully ✓
  Gate 2 — Setup wizard: 5 steps complete with progress saved ✓
  Gate 3 — Checklist: all items checkable; progress persists on reload ✓
  Gate 4 — Resources: all documents accessible; no broken links ✓
  Gate 5 — Mobile: portal usable on mobile viewport ✓
**Node 7**: All 5 gates pass. Portal ready for first cohort.
**Node 8**: QA report: onboarding-portal-QA.md | 5 gates | all green
**Node 9**: Verdict: QA_PASS — onboarding portal cleared for launch. TransitionType: no contribution.
