# Layer 0-A — Compiler Pairs (058)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-058-001 — "show how to" never fires topic predicate (extended reference)

"Show how to build" → gap (not IMPLEMENT)
"Show how to test" → gap (not TEST)
"Show how to integrate" → gap (not API_CONTRACT)
"Show how to architect" → gap (not ARCHITECTURE)
"Show how to secure" → gap (not SECURITY)
"Show how to document" → gap (not DOCUMENT)
Rule: "show how to [code verb]" = learning target infinitive → gap in all cases. No exceptions.

---

## B34-compiler-058-002 — "demonstrate how to" mirrors "show how to" rule

"Demonstrate how to build" → gap (not IMPLEMENT)
"Demonstrate how to test" → gap (not TEST)
"Demonstrate how to integrate" → gap (not API_CONTRACT)
"Demonstrate how to secure" → gap (not SECURITY)
Rule: "demonstrate how to [code verb]" = learning target infinitive → gap in all cases. No exceptions.

---

## B34-compiler-058-003 — "show how to" vs "show [output]" boundary

"Show how to configure the gateway" → gap (teaching frame)
"Show the gateway configuration in the dashboard" → no gap (display action, not guidance)
"Show the team the test results" → no gap (presentation of output, not teaching)
"Show the team how to read the test results" → gap (teaching frame)
Rule: "show how to" = gap; "show [noun]" = display action = no gap unless other guidance vocabulary present.

---

## B34-compiler-058-004 — "introduce [feature] into" disambiguation (complete reference)

"Introduce [person] to [topic]" → gap (onboard/train vocabulary)
"Introduce [feature] into [system]" → IMPLEMENT (feature addition)
"Introduce [concept] to [person]" → gap (teaching vocabulary)
"Introduce [module] into [codebase]" → IMPLEMENT (addition vocabulary)
Rule: subject determines routing. Person/team = gap. Feature/module/component = IMPLEMENT.

---

## B34-compiler-058-005 — "brief" scope: brief on [topic] vs brief [document]

"Brief the team on [topic]" → gap (guidance vocabulary)
"Write a brief for the stakeholders" → DOCUMENT (brief as noun = document type)
"Create a technical brief" → DOCUMENT (brief as noun)
Rule: "brief [person] on" = guidance verb → gap; "brief" as noun/document type = DOCUMENT.
