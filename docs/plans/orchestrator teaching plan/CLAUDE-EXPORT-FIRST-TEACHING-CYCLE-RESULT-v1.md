# Claude Export First Teaching Cycle Result

Version: 1.0 - 2026-05-05

---

## Source Bundle

```text
bundle_id: legacy-docs-skills-from-old-code
source: Claude conversation export batch-0000
problem type: existing/old codebase -> documentation -> skills library -> tests/plans -> correction learning
```

Bundle files:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\claude-export\batch-0000\legacy-docs-skills-from-old-code
```

Ledger files:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\teaching-ledger\claude-export\batch-0000\legacy-docs-skills-from-old-code
```

Packaged artifact:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\claude-export\batch-0000\legacy-docs-skills-from-old-code-cycle.zip
```

---

## Bundle Extraction Result

```text
Conversations extracted: 2
Valid prompt rows produced: 74
Compiler rows: 74
Transition orchestrator rows: 74
Domain splitter rows: 74
Correction pairs: 25
Held-out corruption cases: 25
```

The correction count is lower than the first rough index estimate because the final extraction used strict H0/A0/H1/A1 correction mapping. Only corrections that could be linked to a previous real human prompt and rejected assistant answer were accepted.

---

## First Row Tested

Real human prompt:

```text
I need a documentation on this project and skills for claude code
```

Full topology loaded:

```text
Phases: 7
Sub-phases: 31
```

Compiler output:

```text
CompiledPredicateIds: DOCUMENT
Confidence: 0.92
```

Router output:

```text
2.1 Business Capability Definition - confidence 0.86
2.4 Edge Cases and Boundary Conditions - confidence 0.85
```

---

## Baseline Run

Before teaching, no executable flows existed for the routed sub-phases.

```text
Verdict: KNOWLEDGE_EDGE
Selected flow: knowledge-edge-missing-flow
Branches: 2
Knowledge edges: 2
Elapsed: 42.039 ms
CPU: 18.58%
Working-set delta: 0.328 MB
Managed allocation delta: 117,824 bytes
Disk delta: 4,096 bytes
```

Human meaning:

```text
The orchestrator understood this as a documentation/business-definition problem,
but it did not yet have executable capability for the routed sub-phases.
```

---

## Teaching And Retry

The capability trainer created probationary learned capabilities for both missing branches, registered retryable flows, and ran the same prompt again.

```text
Verdict after teaching: PASS
Selected flow: generated-capability-flow-2-1
Branches: 2
Knowledge edges after retry: 0
Elapsed: 2,561.7116 ms
CPU: 12.35%
Working-set delta: 19.398 MB
Managed allocation delta: 173,456,536 bytes
Disk delta: 3,870,720 bytes
```

Registered learned units:

```text
generated-capability-2-1-legacy-docs-skills-row-0001
  role: generated-capability
  domain/sub-phase: 2.1
  trust: Probationary
  learned: true

generated-capability-2-4-legacy-docs-skills-row-0001
  role: generated-capability
  domain/sub-phase: 2.4
  trust: Probationary
  learned: true
```

Registered flows:

```text
generated-capability-flow-2-1
  task: business-capability-definition

generated-capability-flow-2-4
  task: edge-cases-and-boundary-conditions
```

Human meaning:

```text
The machine detected missing expertise, taught a probationary capability for the
missing routed areas, registered flows for them, and retried successfully.
```

---

## Important Finding

The first prompt was routed only through the `DOCUMENT` predicate and landed in Phase 2 sub-phases.

This is a real signal:

```text
The current compiler still under-represents "documentation + skills library from code"
as a whole development-cycle task. It should probably also activate ARCHITECTURE,
IMPLEMENT, TEST, LEARNING, and possibly QA depending on the prompt.
```

This does not invalidate the first teaching row. It gives the next teaching target:

```text
teach the compiler/router that old-code documentation and skill-library generation
is not only documentation. It is a multi-phase orchestration request.
```

---

## Next Row Recommendation

Use the next real prompt from the same bundle that mentions an actual code issue:

```text
I have some issue with my figma code it uses figmaBase64 of just generated frame...
```

Expected improvement target:

```text
The compiler should activate DOCUMENT, IMPLEMENT, UI, TEST, and LEARNING rather
than only DOCUMENT.
```

---

# Last updated: 2026-05-05

---

## After Predicate Lexicon Expansion

The compiler was updated to recognize `LEARNING`, skills-library language, old/current codebase language, and implementation/test vocabulary in real exported prompts.

The same first prompt was rerun:

```text
I need a documentation on this project and skills for claude code
```

Compiler output changed from:

```text
DOCUMENT
```

to:

```text
ARCHITECTURE
DOCUMENT
IMPLEMENT
LEARNING
```

Router output changed from 2 branches:

```text
2.1 Business Capability Definition
2.4 Edge Cases and Boundary Conditions
```

to 9 branches:

```text
2.1 Business Capability Definition
2.4 Edge Cases and Boundary Conditions
3.1 Architecture and Component Topology
3.2 Data Model and Persistence Design
3.5 Observability and Operational Design
4.1 Backend Services
4.3 Infrastructure and Deployment Code
4.4 Refactoring and Code Quality
7.5 Learning Set and Model Feedback
```

Measured after-fix run:

```text
Baseline elapsed: 36.7699 ms
Baseline CPU: 10.62%
Baseline RAM delta: 0.465 MB
Baseline branches: 9
Baseline knowledge edges: 9

Teaching/retry elapsed: 14,044.5202 ms
Teaching/retry CPU: 11.43%
Teaching/retry RAM delta: 30.004 MB
Teaching/retry branches: 9
Teaching/retry knowledge edges: 0
Registered probationary learned units: 9
Registered generated flows: 9
```

Human interpretation:

```text
The original bottleneck is fixed: the prompt is no longer treated as pure DOCUMENT.
The new bottleneck is route precision. The router now understands this as multi-phase
work, but it fans out to too many same-predicate sub-phases from one row.
```

Next engineering/teaching target:

```text
teach or refine the router to rank task-specific sub-phases inside a predicate family.
For this prompt, 3.1, 4.1/4.4, 7.5, and 2.1 are useful; 3.2, 3.5, and 4.3 may be
over-routing unless the prompt names data persistence, observability, infrastructure,
or deployment explicitly.
```

---

## After Anchor Filtering

The router was updated to require prompt-level anchor terms inside broad predicate
families. This prevents one predicate such as `ARCHITECTURE` or `IMPLEMENT` from
selecting every sibling sub-phase unless the human prompt actually names that area.

The first prompt rerun produced:

```text
Predicates:
ARCHITECTURE
DOCUMENT
IMPLEMENT
LEARNING

Routed branches:
2.1 Business Capability Definition
3.1 Architecture and Component Topology
4.1 Backend Services
4.4 Refactoring and Code Quality
7.5 Learning Set and Model Feedback

Removed over-routes:
3.2 Data Model and Persistence Design
3.5 Observability and Operational Design
4.3 Infrastructure and Deployment Code
```

Measured run:

```text
Elapsed: 9,926.4751 ms
CPU: 12.08%
Working-set delta: 55.777 MB
Managed allocation delta: 521.320 MB
Checkpoint bytes: 627
Branches: 5
Knowledge edges after retry: 0
Verdict: PASS
```

Human interpretation:

```text
The original under-routing was fixed and the first over-routing issue was reduced.
The row now teaches exactly the meaningful old-code documentation/skills path:
business meaning, architecture extraction, implementation inspection/refactoring,
and learning-set feedback.
```

---

## Second Real Row Tested

Real human prompt:

```text
I have some issue with my figma code it uses figmaBase64 of just generated frame.
Since we use the dynamic document, the moment we need to use it, it is still null.
We are missing some async call which wasn't really finished and do not know where
a fix is required.
```

First measurement after anchor filtering still over-routed because `dynamic document`
was treated as documentation work:

```text
Predicates:
DOCUMENT
IMPLEMENT
TEST
UI

Routed branches:
2.1
4.1
4.2
4.4
5.1
5.4
7.4

Elapsed: 14,757.9384 ms
Branches: 7
```

The guidance resolution was applied: `document` alone is no longer a DOCUMENT
predicate. Documentation now requires terms such as `documentation`, `docs`,
`readme`, or explicit "write/create/generate/document this" phrasing. This keeps
runtime code entities named `document` from triggering Phase 2 or coverage/reporting.

After the fix:

```text
Predicates:
IMPLEMENT
TEST
UI

Routed branches:
4.1 Backend Services
4.2 Frontend and Client Integration
4.4 Refactoring and Code Quality
5.1 Unit Test Design
5.4 Regression and Compatibility Testing

Removed over-routes:
2.1 Business Capability Definition
7.4 Coverage and Reporting
```

Measured run:

```text
Elapsed: 9,582.4083 ms
CPU: 12.31%
Working-set delta: 71.012 MB
Managed allocation delta: 957.215 MB
Checkpoint bytes: 1,510
Branches: 5
Knowledge edges after retry: 0
Verdict: PASS
```

Human interpretation:

```text
The second real row confirms the same pattern generalizes: the orchestrator can now
distinguish documentation requests from code entities named document, split a real
Figma async/null bug into implementation, UI integration, refactoring, unit test,
and regression branches, teach missing capabilities, register generated flows, and
retry successfully within the 10-second row budget.
```
