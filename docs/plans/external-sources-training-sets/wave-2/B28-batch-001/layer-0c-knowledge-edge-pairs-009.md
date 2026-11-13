# Layer 0-C — Knowledge Edge Pairs (009)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E009-001 — "QA pass/fail" fires QA

A: "The SearchService must achieve a QA pass before the production release." → ["QA"]
  Teaching: "QA pass", "QA fail", "QA result" all fire QA via compound noun.

---

## E009-002 — "critical defects" alone is gap

A: "Fix the critical defects before the release." → []
  Teaching: "critical defects", "bugs", "issues" → gap. SECURITY is for security vulnerabilities. TEST fires on "test/verify/assert/validate". "fix" does not fire IMPLEMENT.

---

## E009-003 — "hotfix" alone is gap

A: "Apply the hotfix to the production environment." → []
  Teaching: "hotfix", "patch", "bugfix" are gap operational vocabulary. They do not fire IMPLEMENT or QA.

---

## E009-004 — "QA report" fires QA+DOCUMENT

A: "Write the QA report for the v2.0 release." → ["QA", "DOCUMENT"]
  "QA report" → QA | "write" → DOCUMENT
  Teaching: "QA report" fires QA. "write" fires DOCUMENT.

---

## E009-005 — "QA" inside parentheses still fires QA

A: "Get quality assurance (QA) sign-off before the release." → ["QA"]
  "(QA)" → QA abbreviation in parenthetical still fires QA | "quality assurance" → gap (spelled out)
  Teaching: "(QA)" inside parentheses still fires QA. The abbreviation itself is the trigger, regardless of position.

