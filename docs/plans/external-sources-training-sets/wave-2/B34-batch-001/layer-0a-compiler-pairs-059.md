# Layer 0-A — Compiler Pairs (059)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-059-001 — complete SCHEDULE_CAPABILITY_LEARNING emission rule

SCHEDULE_CAPABILITY_LEARNING emitted: once, when predicate set = [] and GapHandler invoked.
NOT emitted when: any code predicate fires (IMPLEMENT, TEST, DOCUMENT, SECURITY, ARCHITECTURE, API_CONTRACT, QA, AUTHORIZATION).
NOT emitted: multiple times for multiple guidance verbs in one request.
NOT emitted: when GapHandler is suppressed.
Payload: {gap_id: "[most specific match]", domainId: "software-development", request: "[original request]"}.

---

## B34-compiler-059-002 — GapWarning vs GapHandler distinction

GapHandler invoked: predicate set = [] → gap logged, SCHEDULE_CAPABILITY_LEARNING emitted.
GapWarning emitted: predicate set ≠ [] AND guidance vocabulary present → warning appended to code flow result.
NEVER both: GapHandler and code predicates in same request. GapHandler invoked ONLY when predicate set = [].

---

## B34-compiler-059-003 — "train on" model vs "train [human]" disambiguation

"Train the team on Kubernetes (K8s)" → gap-skills-guidance-train-001 (human training → gap)
"Train the ML (Machine Learning) model on the dataset" → not in B34 scope (model training = ML operation)
"Retrain the recommendation model" → not in B34 scope
Rule: B34 governs human skill/guidance only. Model training operations are not guidance vocabulary.

---

## B34-compiler-059-004 — "run through" vs "run [test/deployment]" boundary

"Run through the checklist with the team" → gap (explanation vocabulary)
"Run the test suite" → TEST (direct test action)
"Run through the deployment with the team" → gap (walk-through vocabulary)
"Run the deployment pipeline" → IMPLEMENT or no predicate (direct deployment action, context-dependent)
Rule: "run through [X] with [person]" = guidance → gap; "run [X]" = direct action = potentially code predicate.

---

## B34-compiler-059-005 — complete B34 predicate set = [] conditions

Predicate set = [] when ALL of these are true:
1. No requester direct action verbs in {implement, build, deploy, configure, integrate, write [code/tests/docs], design, architect, secure, harden, audit, document, QA}.
2. Only guidance vocabulary present (any combination from Round 1+2+3).
3. No "teach to [code verb]" with embedded code action pattern (those remain as gap).
4. No third-party perform "integrate" construction (those trigger API_CONTRACT).
When predicate set = [] → GapHandler → SCHEDULE_CAPABILITY_LEARNING emitted once.
