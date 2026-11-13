# Layer 0-C — Knowledge-Edge Pairs (008)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B38-0C-008-001 — TransitionType for QA-only scenarios: no_contribution

When PredicateSet = ["QA"] only:
TransitionType = no_contribution

Meaning: The quality gate check (approval, sign-off, review) does not constitute a new code contribution. The QA predicate represents verification of existing work, not authoring of new work.

N09 output for no_contribution QA:
- sign_off_register.md (record of approvals)
- release_gate_report.md (gate decision)
- No code files generated

---

## B38-0C-008-002 — TransitionType for QA+IMPLEMENT scenarios: feature_addition

When PredicateSet includes "IMPLEMENT":
TransitionType = feature_addition

Meaning: Code is being authored or reviewed at the implementation level. The IMPLEMENT predicate confirms a code artifact (service, build, pipeline) is part of the contribution.

N09 output for feature_addition QA+IMPLEMENT:
- service_review.md or build_audit.md (IMPLEMENT phase output)
- release_gate_report.md (QA phase output)
- release_approval.md (gate decision)
- 3 files minimum

---

## B38-0C-008-003 — TransitionType for QA+DOCUMENT scenarios: no_contribution (not documentation_addition)

When PredicateSet = ["QA", "DOCUMENT"]:
TransitionType = no_contribution

Note: Even though DOCUMENT fires and documentation is produced, TransitionType remains no_contribution. The TransitionType tracks code feature contributions, not documentation contributions. DOCUMENT routing to Phase 2.1 produces documentation, but this does not qualify as feature_addition.

This is a non-obvious rule. Learners sometimes expect DOCUMENT to trigger a "documentation_addition" TransitionType. The valid TransitionType values in B38 are:
- no_contribution (QA alone, or QA+DOCUMENT, or QA+SECURITY)
- feature_addition (any combination with IMPLEMENT)
- N/A (knowledge_edge — no predicates)

---

## B38-0C-008-004 — TransitionType summary table for B38

| PredicateSet | TransitionType | Reasoning |
|---|---|---|
| [] | N/A | knowledge_edge; no transition |
| ["QA"] | no_contribution | QA = review only |
| ["IMPLEMENT"] | feature_addition | Code authored |
| ["QA","DOCUMENT"] | no_contribution | No code authored |
| ["QA","IMPLEMENT"] | feature_addition | Code in scope |
| ["QA","SECURITY"] | no_contribution | No code authored |
| ["QA","IMPLEMENT","DOCUMENT"] | feature_addition | Code in scope |
| ["QA","IMPLEMENT","SECURITY"] | feature_addition | Code in scope |
| ["QA","DOCUMENT","SECURITY"] | no_contribution | No code authored |
| ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | feature_addition | Code in scope |

Rule: feature_addition iff IMPLEMENT in PredicateSet.
