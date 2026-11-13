# Layer 0-A — Compiler Pairs (003)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## B34-compiler-003-001 — "walkthrough" alone → gap

A: "Run a walkthrough of the deployment process for the team."
B: Predicates: [] | Gap: gap-skills-guidance-onboard-001
Evidence: "walkthrough" = guidance vocabulary; "deployment" alone ≠ predicate without QA/IMPLEMENT action verb.

---

## B34-compiler-003-002 — "tutorial" alone → gap

A: "Create a tutorial on how to use the API."
Analysis: "create" could fire IMPLEMENT, but "a tutorial" is content-type noun → DOCUMENT (not IMPLEMENT).
B: Predicates: ["DOCUMENT"] | TransitionType: documentation
Evidence: "create a tutorial" → DOCUMENT (tutorial = documentation artifact type); create + documentation target → DOCUMENT.
Note: "create a [tutorial/guide/doc]" → DOCUMENT. "create a [module/service/system]" → IMPLEMENT. Noun type distinguishes.

---

## B34-compiler-003-003 — "learning path" → gap

A: "Build a learning path for junior developers."
Analysis: "build" could fire IMPLEMENT, but "learning path" is curriculum vocabulary → gap or DOCUMENT?
B: Predicates: ["DOCUMENT"] (learning path = documentation artifact if build = create content)
OR Predicates: ["IMPLEMENT"] (if build = software implementation of an LMS module)
Resolution: Context-dependent; default to IMPLEMENT when "build" + noun is ambiguous ("build a learning path system" → IMPLEMENT).
Training pair: "build a learning path" → DOCUMENT (content creation default); "build a learning path tracker" → IMPLEMENT.

---

## B34-compiler-003-004 — "mentor" + "document" → DOCUMENT fires

A: "Mentor the team on best practices and document the coding standards."
B: Predicates: ["DOCUMENT"] | TransitionType: documentation
Evidence: "mentor" → gap; "document the coding standards" → DOCUMENT. DOCUMENT fires; GapHandler NOT invoked.

---

## B34-compiler-003-005 — "guide through" embedded predicate

A: "Guide the team through writing the API documentation."
Analysis: "guide through" is a teaching frame; BUT "writing the API documentation" contains DOCUMENT trigger.
B: Predicates: ["DOCUMENT"] | TransitionType: documentation
Evidence: "writing" (documentation target) → DOCUMENT; "guide through" = teaching frame that wraps a DOCUMENT action.
Key rule: Embedded action verbs inside guidance frames DO fire their predicates.
