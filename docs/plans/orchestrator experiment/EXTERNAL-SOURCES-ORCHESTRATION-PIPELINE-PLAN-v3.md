# External Sources Orchestration Pipeline Plan
# Complete orchestration learning edition
# Version: 3.0 - 2026-05-05
# Supersedes: EXTERNAL-SOURCES-ORCHESTRATION-PIPELINE-PLAN-v2.md

---

## Purpose

This plan describes how license-checked external sources and XIIGen first-party
sources become complete orchestration learning sets.

The output is not only node pairs. The output is a teaching cycle that can train
and measure orchestration units.

---

## Core Change From v2

Version 2 deferred cross-phase context carry-forward to a later wave.

That no longer matches the active goal.

The active goal is to teach the orchestrator to manage the whole development
area. Cross-phase context is therefore part of the first orchestration teaching
cycle, starting with O1.

---

## Pipeline Overview

```text
source registry
-> prompt extraction
-> formal authority extraction
-> lifecycle graph oracle
-> orchestration teaching rows
-> held-out and corruption rows
-> offline training
-> measured prompt run
-> one-row correction
-> retraining
-> evidence zip
```

Each stage repairs its own local issues and continues. User-facing reports are
written only at completion or if a source cannot legally or logically be used.

---

## Stage 0 - Source Authority Registration

Record:

```text
source_id;
source_state;
license_class;
attribution_required;
allowed_use_note;
primary_orchestration_units;
primary_scenarios;
primary_phases;
primary_sub_phases;
```

Internal check:

```text
source is first-party, license-checked, source_gap, corruption_case, or held_out;
VERIFY_BEFORE_USE sources do not enter training rows.
```

---

## Stage 1 - Prompt Extraction

Extract real prompt language.

Priority order:

```text
XIIGen user prompts;
engineering proposal motivation/user-story language;
developer question titles and bodies with attribution;
review correction statements;
source-gap statements.
```

Do not use formal standard clauses as prompt-splitter A sides. Formal clauses
belong to authority extraction.

---

## Stage 2 - Formal Authority Extraction

Extract what the source proves or constrains.

Examples:

```text
OWASP ASVS requirement;
RFC MUST/SHOULD clause;
XIIGen locked architecture rule;
reviewer correction;
accepted oracle trace;
source-gap declaration.
```

This material supports B-side correctness, arbiter selection, and transition
verification.

---

## Stage 3 - Lifecycle Graph Oracle

For each real scenario, write the expected orchestration graph.

The oracle includes:

```text
domains;
phases;
sub-phases;
parallel groups;
dependencies;
transition points;
context carry-forward;
conflicts;
knowledge edges;
arbiters;
sub-tasks;
human result;
resource measurement requirement.
```

This is the reference object for training and comparison.

---

## Stage 4 - Teaching Row Production

Produce five row families:

```text
prompt split rows;
phase sequencing rows;
context carry-forward rows;
transition verification rows;
arbiter and sub-task rows.
```

Each row has original and Phrasing B forms.

Each scenario also has held-out and corruption rows.

---

## Stage 5 - Offline Training

Train one orchestration unit at a time.

First unit:

```text
orchestration-prompt-splitter
```

Model:

```text
small inspectable KNN over predicate and graph features;
JSON model file;
explicit offline command;
no startup training;
no hidden external AI call.
```

---

## Stage 6 - Measured Prompt Run

Run:

```text
deterministic fixture;
learned v1;
held-out prompt variants;
corruption prompt variants.
```

Record both quality and resources:

```text
split recall;
split precision;
transition recall;
dependency violations;
context loss;
missing arbiters;
knowledge-edge invention;
reopened rejected items;
teaching time;
prompt process time;
CPU;
RAM;
managed allocation;
drive allocation;
model size.
```

---

## Stage 7 - One-Row Correction And Retraining

Find one held-out miss.

Add one teaching row that fixes it.

Retrain.

Re-run original, Phrasing B, held-out, and corruption prompts.

The learning cycle is meaningful only if:

```text
the missed case improves;
the original cases do not regress;
resource growth is visible and explainable.
```

---

## Stage 8 - Promotion To Corpus

Write:

```text
cycle summary markdown;
training metrics JSON;
prompt-run metrics JSON;
held-out result JSON;
model summary;
source delta;
corpus ledger update;
zip.
```

Promote only rows that preserve source state and attribution.

---

## Scenario Order

### O1 first

O1 uses both source families immediately:

```text
XIIGen topology and implementation sources;
Wave 1 security authority sources.
```

It tests:

```text
security design;
backend implementation carry-forward;
integration test obligations;
security QA review;
release-blocking automation.
```

### O2 through O5 next

Continue:

```text
O2 wave-gated flow registration;
O3 performance regression investigation;
O4 external source integration under license constraint;
O5 skill advisor full lifecycle.
```

O5 is the full lifecycle run across all thirty-one sub-phases.

---

## Relationship To Existing Wave 1

Wave 1 remains valid and important.

It supplies:

```text
security authority;
arbiter verdict vocabulary;
transition examples for security carry-forward;
release blocking examples.
```

It does not supply all orchestration behavior. The v3 pipeline uses Wave 1 as
source authority inside O1 and O4 rather than treating it as the whole external
learning set.

---

## Last Updated

2026-05-05. Version 3 aligns external-source learning with complete
orchestration teaching cycles and measured learned-unit replacement.
