# Layer 3 — Teaching Unit Pairs
# Batch: B34-batch-001
# Scenario: B34 — Skills and guidance gap
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: teaching_unit

---

## Pair B34B1-WDA-teach-001 — "mentor" and "coach" have no predicate in any context

A: "Mentor the team on API design best practices." Does API_CONTRACT fire?
B: No. "mentor" is gap vocabulary. "API design" contains "API" but the action is
"mentor" (teach/guide). API_CONTRACT fires on action verbs: integrate, connect,
contract, interface (as action), define an API. The subject of the request is
a mentoring action, not an API design action. Zero predicates → knowledge_edge.

---

## Pair B34B1-WDA-teach-002 — embedded action verbs fire even inside guidance frames

A: "Guide the team through writing the unit tests for the payment service." What fires?
B: TEST fires on "tests" (unit tests) and IMPLEMENT may fire if "writing" is parsed
as an IMPLEMENT-adjacent action. The embedded action "writing unit tests" contains
"test/tests" which fires TEST. "service" fires IMPLEMENT. "Guide" is gap but does
not suppress the inner predicates. CompiledPredicateIds: ["TEST", "IMPLEMENT"].
Two independent routing decisions: TEST → 5.1, IMPLEMENT → 4.1.

---

## Pair B34B1-WDA-teach-003 — "tutorial" alone does not fire DOCUMENT

A: "Create a tutorial for the new authentication module." What fires?
B: IMPLEMENT fires on "create". "tutorial" is gap — it does not fire DOCUMENT.
DOCUMENT fires on: document, write up, specification, readme, changelog, notes,
wiki. "Tutorial" is a content type in the guidance domain — it has no predicate
unless paired with "document" or "write". CompiledPredicateIds: ["IMPLEMENT"] via
"create" + "module". CapabilityGapWarnings: tutorial, authentication module (tutorial sense).

---

## Pair B34B1-WDA-teach-004 — "explain" is always gap, even for documented topics

A: "Explain the architecture of the payment system." Does ARCHITECTURE fire?
B: No. "explain" has no predicate. ARCHITECTURE fires on: design, architect, structure,
layer (as action), define the architecture, plan the system. "Explain the architecture"
is a knowledge-sharing request, not an architecture construction request. Zero predicates
→ knowledge_edge. Gap ID: gap-skills-guidance-mentor-001.

---

## Pair B34B1-WDA-teach-005 — guidance DomainId is "software-development"

A: What DomainId does the orchestrator emit when mentoring/coaching vocabulary fires
zero predicates?
B: DomainId: "software-development". Mentoring and coaching in a software engineering
context are software-development domain activities. The gap is in human-learning
vocabulary (teach/mentor/coach) not in domain. This is distinct from B36/B37 which
emit "project-management" because those requests are about project planning activities
(sprint, roadmap, standup) that belong to a different domain.
