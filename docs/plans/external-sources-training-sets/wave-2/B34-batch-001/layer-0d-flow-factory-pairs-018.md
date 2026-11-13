# Layer 0-D — Flow Factory Pairs (018)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-018-001 — "Facilitate" vocabulary: factory map

| Request | Predicates | Factory | TransitionType |
|---------|-----------|---------|----------------|
| "Facilitate knowledge transfer" | [] | GapHandler | none |
| "Facilitate a retrospective" | [] | GapHandler | none |
| "Facilitate integration of A with B" | [API_CONTRACT] | APIContractFlow33 | documentation |
| "Facilitate and then implement the feature" | [IMPLEMENT] | ImplementationFlow41 | feature_addition |
| "Facilitate session where team writes tests" | [] | GapHandler | none |
| "Facilitate + personally write the API docs" | [DOCUMENT] | DocumentationFlow | documentation |

---

## FF-018-002 — "Facilitate integration" — API_CONTRACT factory trigger (special rule)

"Facilitate the integration of the billing system with the payment processor" → APIContractFlow33
"Facilitate connecting the legacy system to the new platform" → APIContractFlow33 ("connecting" = "integrate" equivalent)
"Facilitate the API connection between services A and B" → APIContractFlow33

Rule: When "facilitate" wraps a structural coupling action (integrate/connect), the integration action fires API_CONTRACT. This is the integrate-always-fires rule operating inside a facilitation frame.

---

## FF-018-003 — Facilitate: full gap vs gap warning decision tree

Step 1: Is there any integration/connect action? → API_CONTRACT fires → gap warning for facilitate
Step 2: Is there any direct requester code action (build/implement/write tests/design/secure/etc.)? → corresponding predicate fires → gap warning for facilitate
Step 3: Only facilitation vocabulary present, no direct code actions, no integration? → GapHandler → gap-skills-guidance-mentor-001
