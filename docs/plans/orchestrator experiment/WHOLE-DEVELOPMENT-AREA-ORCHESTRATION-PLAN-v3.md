# Whole Development Area Orchestration Plan
# Version: 3.0 - 2026-05-05
# Supersedes: WHOLE-DEVELOPMENT-AREA-ORCHESTRATION-PLAN-v2.md

---

## Purpose

This plan defines how to teach and test the dynamic flow as a whole-development
orchestrator.

Version 3 adds the missing first step:

```text
human prompt -> logical transition object
```

The orchestrator does not split raw wording immediately. It first converts the
human prompt into a compact source-to-solution transition:

```text
A -> B with transformation X
```

Then it splits that transition between development expertise areas and specific
tasks.

---

## Correct Runtime Shape

```text
human prompt
-> logical transition normalizer
-> expertise and task splitter
-> prompt simplifier
-> domain-language translator
-> scheme planner
-> parallel ILogicalUnits
-> appliers
-> result translator
-> arbiter selector
-> arbiter units
-> reprocess loop when needed
-> final human formatter
```

The final result returns to human language only after logical results have been
produced and reviewed.

---

## Functional Principle

Each ILogicalUnit works on one functional principle.

The unit does not solve the whole user prompt.

The unit receives:

```text
Ai = compact source/problem state for this branch
```

The unit returns:

```text
Bi = branch result
Xi = transformation evidence or action used to reach Bi
```

The whole orchestration is:

```text
A -> B
A1 -> B1 + X1
A2 -> B2 + X2
...
An -> Bn + Xn
```

---

## Required Orchestration Units

| Order | Unit role | Responsibility |
|---:|---|---|
| 1 | logical-transition-normalizer | Convert human prompt into A, B, X, constraints, unknowns, rejected intents, and allowed scope. |
| 2 | orchestration-prompt-splitter | Split the normalized transition into development expertise areas and task branches. |
| 3 | prompt-simplifier | Convert each branch into compact Ai, Bi, Xi source for a specific logical unit. |
| 4 | domain-language-translator | Translate compact branch source into the domain vocabulary expected by the unit. |
| 5 | phase-sequencing-unit | Order branches into dependencies, parallel groups, waits, and reprocess points. |
| 6 | context-carry-forward-unit | Preserve decisions, constraints, knowledge edges, and rejected items across phase boundaries. |
| 7 | transition-verifier-unit | Detect downstream work that ignores or contradicts upstream context. |
| 8 | arbiter-selection-unit | Select the review units with authority over each branch result. |
| 9 | sub-task-planner-unit | Produce actionable sub-tasks from accepted branch transitions. |
| 10 | human-result-formatter-unit | Translate reviewed logical results back into human language. |

This is the learning order. Node-level optimization follows after these roles
are measurable.

---

## Scenario O1 Example

### Human Prompt

```text
Add multi-tenant authentication to the API, preserve internal-first execution,
avoid duplicate identity creation, enforce token and tenant isolation, test it,
review it for security, and add automation so an unsafe release cannot pass.
```

### Transition Object

```yaml
A:
  - API authentication lifecycle does not yet have the requested multi-tenant
    orchestration evidence.
B:
  - API authentication lifecycle enforces tenant and token isolation through
    design, implementation, tests, QA review, and release automation.
X:
  - design identity and security boundaries
  - implement backend/provider enforcement
  - test tenant and token isolation
  - review security evidence
  - automate release blocking
constraints:
  - internal-first execution
  - duplicate identity creation rejected
  - missing tenant source remains a knowledge edge
allowed_scope:
  - orchestration and sub-task production before code implementation
```

### Expert Branches

```text
3.1 architecture and component topology
3.3 API contract definition
3.4 security and authorization model
4.1 backend services
4.3 integration and provider connectors
5.1 unit and contract tests
5.2 integration tests
6.3 security and compliance review
7.3 CI/CD gate configuration
7.4 coverage and reporting
```

### Branch Principle Example

```yaml
A1:
  "Security rules for tenant and token isolation are not yet formalized."
B1:
  "Security model defines tenant isolation, token validation, duplicate identity
  rejection, source authority, and evidence required for release blocking."
X1:
  "Translate Wave 1 security authority and XIIGen internal rules into a 3.4
  security branch."
```

---

## Real Scenario Suite

| Scenario | Real source basis | Primary orchestration pressure |
|---|---|---|
| O1 multi-tenant authentication addition | XIIGen + Wave 1 security corpus | Transition normalization, security carry-forward, release blocking. |
| O2 wave-gated flow registration | XIIGen corpus/flow registry docs | Methodology topology, flow registration, conformance evidence. |
| O3 performance regression investigation | Inspector performance history | Resource-aware routing, performance testing, UI simplification. |
| O4 external-source integration under license constraint | External-source pipeline and Wave 1 archive | Source authority, license preservation, gap handling. |
| O5 skill advisor full lifecycle | XIIGen skill/orchestration plans | All seven phases and all thirty-one sub-phases. |

---

## Teaching Data Per Scenario

Each scenario produces:

```text
human prompt row;
transition-normalizer row;
expert-split row;
prompt-simplifier rows;
domain-language rows;
phase-sequencing row;
context-carry-forward rows;
transition-verifier rows;
arbiter-selection rows;
sub-task-planner rows;
human-result row;
held-out variants;
corruption variants;
resource metrics template.
```

The B side always contains the actual logical object, never a description of the
object.

---

## Measurement

Every teaching and prompt run records:

```text
teaching time;
feature extraction time;
model build time;
model load time;
prompt process time;
CPU time;
peak RAM;
managed allocation;
disk read/write allocation;
model size;
corpus size;
split accuracy;
transition accuracy;
context-loss count;
arbiter-missing count;
knowledge-edge invention count;
reopened rejected item count.
```

The architecture must remain usable on this PC.

---

## Success Definition

The plan is successful when:

```text
the normalizer produces correct A/B/X transition objects;
the splitter assigns the transition to correct expertise areas;
the simplifier creates compact Ai/Bi/Xi branch sources;
logical units solve only their branch principle;
arbiters can reprocess a branch without restarting the whole prompt;
the final result translates reviewed logical results back to human language;
resource metrics prove the cycle is practical.
```

---

## First Execution Path

Start O1 with:

```text
logical-transition-normalizer
```

Then train:

```text
orchestration-prompt-splitter
prompt-simplifier
phase-sequencing-unit
context-carry-forward-unit
transition-verifier-unit
arbiter-selection-unit
sub-task-planner-unit
human-result-formatter-unit
```

---

## Last Updated

2026-05-05. Version 3 aligns the plan with the A -> B and A1 -> B1 + X1 dynamic
flow model.
