# Layer 0-C — Knowledge-Edge Pairs (006)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B38-0C-006-001 — Complete QA lexicon in release context

The following terms fire the QA predicate in B38 release-gate scenarios:

| Term | Notes |
|---|---|
| review | Core QA term; always fires |
| approve / approved / approval | Core QA terms; always fire |
| verify / verification | Core QA terms (rules 20, 21) |
| validate / validation | Core QA term |
| accept / acceptance | QA in quality/gate context |
| quality | QA; fires on "quality gate", "quality check" etc. |
| audit | QA in release context; QA+SECURITY in security context (rule 19) |
| sign-off / sign off | Core QA term (rule 23); fires as noun or verb |
| compliance | QA; also SECURITY context support |
| inspect / inspection | QA; inspection is QA-equivalent in release context |

---

## B38-0C-006-002 — QA terms that might be confused with other predicates

"audit" confusion risk:
- In release context: fires QA
- In security context ("security audit"): fires QA AND SECURITY
- Never fires IMPLEMENT or DOCUMENT

"compliance" confusion risk:
- In QA context: fires QA
- In security context: supports SECURITY
- Never fires IMPLEMENT or DOCUMENT

"validation" confusion risk:
- Fires QA always in B38
- Some other domains use "validation" for DOCUMENT
- In B38 specifically: validation is a QA action

---

## B38-0C-006-003 — QA terms that do NOT fire from near-synonyms

Terms that do NOT fire QA despite sounding similar:
- "check" (borderline — context-dependent; in quality-gate context fires QA, in checklist context does not)
- "test" → does not fire QA (TEST is a separate predicate; QA is not TEST)
- "monitor" → does not fire QA (rule 7)
- "confirm" → borderline; not a core QA term in B38
- "assess" → borderline; not in core QA lexicon
- "evaluate" → borderline; not in core QA lexicon

The safe set that always fires QA: review, approve, verify, validate, audit, sign-off, compliance, inspect, quality.

---

## B38-0C-006-004 — QA in B38 vs QA in other B-series scenarios

B38 (Project delivery): QA fires on release-gate vocabulary — approve, sign-off, audit, verify
B37 (Testing): QA fires on test-quality vocabulary — test coverage, defect review, QA sign-off
B33 (Documentation): QA fires on documentation review — review, approve, accept

In B38, QA is specifically oriented toward release gates and approval workflows. The same word "review" fires QA in all B-series, but the output routing is B38-specific: always to Phase 7.1 in this scenario.
