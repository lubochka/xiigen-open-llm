# Layer 0-B — Router Pairs (038)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-038-001 — Gap: gradient descent standalone

A: [] | "Gradient descent converges slowly on this loss surface."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "gradient descent" = AI optimization algorithm. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-038-002 — Gap: backpropagation without code verb

A: [] | "Backpropagation computes gradients through the network layers."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "backpropagation" = AI training mechanism. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-038-003 — IMPLEMENT: implement gradient descent optimizer

A: ["IMPLEMENT"] | "Implement a gradient descent optimizer with momentum and weight decay."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "implement" fires IMPLEMENT. "gradient descent optimizer" = implementation artifact.

---

## C0B-038-004 — IMPLEMENT: write backpropagation routine

A: ["IMPLEMENT"] | "Write the backpropagation routine for the custom activation function."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "write" (code artifact context) fires IMPLEMENT.

---

## C0B-038-005 — All seven flows: training vocabulary + all code verbs

A: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"] | "Architect the gradient descent platform, integrate backpropagation API, implement optimizer, secure training endpoint, test gradient computation, document algorithm, QA."
B: ARCHITECTURE → 3.1; API_CONTRACT → APIContractFlow33; IMPLEMENT → 4.1; SECURITY → SecurityAuditFlow; TEST → 5.1; DOCUMENT → DocumentationFlow62; QA → QAGatingFlow73 (last). Seven flows.
