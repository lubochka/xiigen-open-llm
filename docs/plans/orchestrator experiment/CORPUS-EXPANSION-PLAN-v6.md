# Corpus Expansion Plan
# Orchestration-first teaching corpus plan
# Version: 6.0 - 2026-05-05
# Supersedes: CORPUS-EXPANSION-PLAN-v5.md

---

## Why v6 Exists

Version 5 completed the build-ready corpus. It proved that every development
sub-phase and every adjacent lifecycle transition has at least one structured
learning path from node-01 through node-09.

That was necessary, but it is not the final learning goal.

The next corpus must teach orchestration, not isolated node behavior. The user
prompt is the real input. The correct output is not a code explanation and not a
single sub-phase label. The correct output is a managed development topology:

```text
prompt -> domains -> phases -> sub-phases -> transitions -> flows -> arbiters -> sub-tasks -> human result
```

The machine must learn to split a real software-development request into the
right work areas, preserve cross-phase constraints, detect contradictions, call
the right arbiters, and produce useful sub-tasks. Code implementation remains a
downstream output format. It is not the current training target.

---

## Active Completion Target

The active target is now an orchestration teaching corpus.

The corpus is complete for this stage when these things exist:

```text
all five real orchestration scenarios have prompt-level teaching packets;
each scenario has an expected phase/sub-phase split graph;
each scenario has transition carry-forward examples;
each scenario has conflict and knowledge-edge examples;
each scenario has arbiter-selection examples;
each scenario has final human-result examples;
each scenario has held-out prompt variants;
every teaching cycle records teaching time, inference time, CPU, RAM, and disk allocation;
the learned-unit plan starts with orchestration units, not node-08 security only.
```

The old build-ready corpus remains valid as the base material. Version 6 adds
the orchestration layer on top of it.

---

## What Is Already Complete

```text
31 / 31 development sub-phases represented
6 / 6 adjacent phase transitions represented
144 training document packets
36 sub-phase held-out document packets
6 transition held-out packets
558 original learning pair rows
558 Phrasing B companions
WDA lifecycle checks passing
full test suite passing at last code verification
```

This means the machine has the minimum vocabulary to start learning.

It does not mean the machine has learned to orchestrate a full development
request. That is what v6 starts.

---

## Teaching Priority Order

The first learned unit is not the security arbiter.

The first learned unit is the orchestration prompt splitter.

The teaching order is:

| Order | Unit role | What it learns |
|---:|---|---|
| 1 | orchestration-prompt-splitter | Split a user request into phases, sub-phases, domains, transitions, and risk items. |
| 2 | phase-sequencing-unit | Order the selected phase/sub-phase work so dependencies are preserved. |
| 3 | context-carry-forward-unit | Carry decisions, constraints, knowledge edges, and rejected items between phases. |
| 4 | transition-verifier-unit | Detect when downstream work ignores or contradicts upstream context. |
| 5 | arbiter-selection-unit | Select the right arbiters for each branch of the orchestration. |
| 6 | sub-task-planner-unit | Produce actionable sub-tasks from the accepted split and sequence. |
| 7 | human-result-formatter-unit | Explain the orchestration result without hiding blocked, rejected, or unknown items. |

Node-level units remain important, but they are trained under the orchestration
unit above them. A node that is locally accurate but causes a bad lifecycle
split has not solved the problem.

---

## Source Families

The orchestration corpus uses two real source families.

### Family A - XIIGen internal sources

These sources teach the machine how this project actually works:

```text
orchestrator experiment plans
development phase guides
corpus expansion plans
logical unit implementation plans
inspector design documents
artifact reviewing protocol
session state ledgers
project code in C:\Projects\llm_mvp_core
```

### Family B - license-checked external sources

Wave 1 contributes deep security and authorization material:

```text
OWASP ASVS 5.0.0
NIST SSDF
CWE Top 25
IETF OAuth, OIDC, JWT, TLS, mTLS, DPoP, PAR, and related RFCs
```

Wave 1 is excellent for sub-phase 3.4 and useful for 6.3, 5.2, 7.3, and
security transitions. It does not cover the whole development lifecycle by
itself.

---

## Real Orchestration Scenario Set

The corpus now centers on five real scenarios.

| Scenario | Source basis | Primary orchestration challenge |
|---|---|---|
| O1 multi-tenant authentication addition | XIIGen auth/security + Wave 1 security corpus | Carry security design into implementation, tests, QA, and automation. |
| O2 wave-gated flow registration | XIIGen flow registry and corpus phases | Sequence registration, implementation, tests, and release gates. |
| O3 performance regression investigation | XIIGen inspector performance issue | Route investigation across testing, QA, technical design, implementation, and automation. |
| O4 external-source integration under license constraint | Wave 1 source pipeline | Preserve license authority, source gaps, security review, and result formatting. |
| O5 skill advisor full lifecycle | XIIGen planned feature | Cover all seven phases and all thirty-one sub-phases in one lifecycle request. |

Each scenario gets original prompts, Phrasing B prompts, held-out prompts, and
deliberately corrupted prompts.

---

## Learning Row Shape

Every orchestration teaching row has this shape.

```text
row_id:
source_family:
scenario_id:
prompt_variant:
user_prompt:
available_context:
expected_domains:
expected_phase_sub_phase_graph:
expected_parallel_groups:
expected_transitions:
expected_context_carry_forward:
expected_conflicts:
expected_knowledge_edges:
expected_arbiters:
expected_sub_tasks:
expected_human_result:
resource_measurement_required:
reason:
```

The B side must contain the actual split graph and actual sub-task plan. It must
not describe what a good split should contain.

---

## Held-Out Prompt Shape

Each scenario needs held-out prompt variants that change wording but preserve
the same logical orchestration challenge.

Held-out prompts must include:

```text
one compact executive-style request;
one long developer-session request;
one ambiguous request missing at least one needed source;
one request with a planted contradiction;
one request with a planted safety or compliance boundary.
```

The held-out output is measured against the expected orchestration graph, not
against exact words.

---

## Resource Measurement Requirement

Every teaching cycle and every prompt run records:

```text
teaching_elapsed_ms
feature_extraction_elapsed_ms
model_build_elapsed_ms
model_load_elapsed_ms
prompt_process_elapsed_ms
cpu_total_ms
cpu_delta_ms
working_set_mb_start
working_set_mb_peak
private_memory_mb_peak
managed_allocated_mb
disk_read_mb
disk_write_mb
model_file_bytes
training_corpus_bytes
```

The resource budget is intentionally simple:

```text
the cycle must complete on this PC;
no leftover process is allowed;
resource growth must be explained by corpus size or model file size;
the inspector remains lightweight and is not part of the learning hot path.
```

---

## Teaching Cycle

Each orchestration unit follows the same cycle:

```text
prepare training rows from real scenarios;
extract predicate vectors and graph labels;
train the unit offline;
record teaching metrics;
run original prompts;
run held-out prompts;
record process metrics;
compare against deterministic fixture and prior learned version;
convert misses into new rows;
retrain;
confirm the original prompts did not regress.
```

This is real learning only if the second trained version improves on a prompt it
previously missed without losing the original scenarios.

---

## Corpus States

Rows keep one of these source states:

| State | Meaning |
|---|---|
| source_backed | The row is supported by a local XIIGen source. |
| external_license_checked | The row is supported by license-checked external source material. |
| source_gap | The correct behavior is to ask for missing source authority. |
| corruption_case | The row is deliberately wrong so the machine learns to reject it. |
| held_out | The row is used only for measuring generalization. |

No `source_gap` row may be rewritten as source-backed by invention.

---

## Wave 1 Placement

Wave 1 is not a general lifecycle corpus. It is a security-authority corpus.

Wave 1 is used in v6 for:

```text
3.4 security and authorization model;
6.3 security and compliance review;
5.2 integration tests when security isolation is the tested behavior;
7.3 CI/CD gates when security evidence blocks promotion;
3.4 -> 4.1 security implementation carry-forward;
4.1 -> 5.2 security test carry-forward;
5.2 -> 6.3 security review carry-forward;
6.3 -> 7.3 security automation carry-forward.
```

Wave 1 is not used as proof that UI/UX, functional specification, frontend
rendering, QA automation, or documentation are mature.

---

## Active Machine Action

Build the orchestration teaching set for O1 first.

O1 is selected first because it uses the strongest external source family while
also requiring real cross-phase orchestration:

```text
technical design -> implementation -> integration tests -> QA security review -> CI/CD gate
```

O1 must produce:

```text
orchestration teaching rows;
held-out prompt rows;
corruption rows;
resource metric template;
first training-cycle evidence template;
zip.
```

After O1 is complete, continue O2 through O5 without user-facing stop reports.

---

## Last Updated

2026-05-05. Version 6 changes the active target from build-ready corpus
completion to measured orchestration teaching.
