# Planning Document Domain Mapping and Arbiter Binding Experiment Plan v1.0

Date: 2026-05-04
Status: Awaiting approval before any executor session begins

---

## Round contract

Test baseline: 985/985 passing. Cannot run dotnet test in this session.
985 is the floor. No executor session may produce a result below 985.

Produced: this plan defines one experiment that does not yet have a number.
The number is assigned after the plan is approved.

Gap being closed: a planning document like the semantic compiler session plan
makes a set of claims: the critical rules are complete, the phase gate commands
correctly detect violations, the loading sequence establishes the right context
before execution begins, and the test names bind to the behaviors that actually
matter. No experiment has ever verified these claims systematically. The plan
may have gaps in its critical rule coverage, gate commands that pass when they
should fail, a loading sequence that omits a document whose absence causes
silent rule violations, or test names that do not cover the behaviors the
experiments proved were important. This experiment treats a planning document
as a testable artifact and produces evidence about whether its rules, gates,
and bindings are correctly mapped to the domain they govern.

---

## Purpose

The experiment must answer one question:

Given a planning document and the domain it governs, are the critical rules
complete and correctly stated, do the Gate B commands detect the violations
those rules are meant to prevent, does the loading sequence establish all the
context needed before execution, and do the test names cover the behaviors
the domain actually requires — or are there missing rules, gates that pass
violations silently, loading gaps that cause rule violations, and tests that
cover the wrong behaviors?

The semantic compiler session plan is the right first document to test because
it is concrete and evidence-backed. Every critical rule in the plan traces to
a named experiment with a measured result. Rule A traces to E20. Rule B traces
to E23. Rule C traces to E44. Rule D traces to decision D-04 in the locked
decisions document. This traceability makes it possible to verify each rule
against its source evidence and determine whether the rule correctly encodes
what the experiment actually found.

---

## Why the semantic compiler plan is the right first planning document

The semantic compiler plan has four properties that make it testable.

First, it has named critical rules with experiment provenance. Rule A states
the pipeline order and names E20 as its source. Rule B states the SUCCESS
equals RETURN mapping and names E23 as its source. Rule C states the dynamic
weight formula and names E44 as its source. Rule D states the canonical domain
names and names locked decision D-04 as its source. Each rule can be verified
against its experiment result.

Second, it has per-phase Gate B commands that are intended to detect specific
violations. Phase 2 Gate B checks VocabSize equals 22. Phase 3 Gate B checks
that StackDetector appears on a lower line number than SemanticNormalizer.
Phase 5 Gate B checks that the VALIDATE_OP mapping produces "return Validate"
and not a string containing "SUCCESS". These gate commands can be tested for
detection accuracy — do they actually detect the violations they are meant to
catch, and do they pass when the violation is absent?

Third, it has a loading sequence that declares which documents must be read
before execution begins. The sequence names CLAUDE.md, AGENTS.md, DECISIONS-
LOCKED.md, EXPERIMENT-RESULTS-MAP.md, and two skill files. Each document in
this sequence provides context that a rule or gate depends on. If any document
is missing from the sequence, the rule or gate that depends on it may fire
incorrectly.

Fourth, it has test names that are meant to bind to specific behaviors. The
test "AstTokenizer_PreservesCase_ForStackDetector" is meant to verify Rule A.
The test "TemplateBeautifier_ValidateOpSuccess_MapsToReturn" is meant to
verify Rule B. The test "LogicExtractor_VocabSize_IsExactly22" is meant to
verify the vocabulary constant. These bindings can be checked for completeness
and correctness.

---

## Experiment scenario

Scenario identifier: planning-mapping-scenario-001-semantic-compiler-domain

Planning document given to the flow: the semantic compiler session plan
version 1.0 as uploaded, governing the 29.SemanticCompiler project
implementation across five phases.

Domain governed: the semantic compiler pipeline — specifically the five-stage
compression path (AstTokenizer, StackDetector, RouteClassifier,
SemanticNormalizer, FamilyCollapser, LogicExtractor), the planning components
(Decomposer, StopSignalDetector, DependencyGraph), and the translation and
beautification layer (PatternClassifier, ContextualDictionary, PatternReorderer,
TemplateBeautifier).

Why this scenario is hard enough for a first test:

It requires checking four critical rules against four different source
authorities (two experiments, one experiment, one experiment, one locked
decision). It requires checking five Gate B command sets for detection
accuracy. It requires checking a six-document loading sequence for
completeness. It requires checking twenty-five test names for behavioral
coverage. Each of these checks can produce an independent finding.

---

## What the experiment must prove

Claim 1 — Critical rule completeness: the four named critical rules cover
all the domain-specific failure modes the experiments identified as
significant. No experiment result with a named failure mode is absent from
the critical rules section.

Claim 2 — Critical rule accuracy: each critical rule correctly encodes what
its source experiment actually found. Rule A correctly states the pipeline
order. Rule B correctly states that SUCCESS means RETURN, not the string
"SUCCESS". Rule C correctly states the dynamic weight formula. Rule D
correctly states that short forms appear in code and long forms in comments
only.

Claim 3 — Gate B detection accuracy: each Gate B command correctly detects
the violation it is meant to catch when that violation is present, and does
not fire a false positive when the violation is absent.

Claim 4 — Loading sequence completeness: every document in the loading
sequence is necessary, and no document that a critical rule or gate depends
on is absent from the sequence.

Claim 5 — Test name behavioral coverage: the named tests cover the behaviors
the experiments proved were important. No experiment with a significant
finding is unrepresented in the test set.

Claim 6 — Arbiter binding correctness: the arbiters configured for the
planning document domain bind to the decisions that actually matter. For the
semantic compiler domain, the required arbiters are the pipeline order arbiter
(Rule A must be verifiable from the code), the template mapping arbiter (Rule
B must produce the correct string), the vocabulary constant arbiter (VocabSize
must equal exactly 22), the domain name arbiter (short forms in code, long
forms in comments), and the loading sequence arbiter (all six documents
present before execution).

---

## Required documents before any experiment code

### Document 1 — Planning document analysis table

File: PLANNING-MAPPING-SEMANTIC-COMPILER-ANALYSIS-TABLE-v1.md

Content: a structured table that maps every element of the semantic compiler
plan to a verifiable claim. One row per element. Columns: element type (critical
rule, Gate B command, loading sequence entry, test name), element identifier,
what it claims to enforce, the source authority for that claim, the specific
failure mode it is meant to prevent, and whether the enforcement is currently
verifiable (yes, no, or partial).

Required rows for the four critical rules:

Rule A row: element type is critical rule. Claims to enforce pipeline order
with StackDetector before SemanticNormalizer. Source authority is E20. Failure
mode prevented is TypeScript markers destroyed by normalization before
detection. Verifiable by grep on line numbers in the pipeline file.

Rule B row: element type is critical rule. Claims to enforce that the
VALIDATE_OP SUCCESS mapping produces the string "return Validate({X});" and
not any string containing the word "SUCCESS". Source authority is E23. Failure
mode prevented is template match rate falling from 100 percent to 10.9 percent.
Verifiable by grep on the Templates dictionary in TemplateBeautifier.

Rule C row: element type is critical rule. Claims to enforce the dynamic
weight formula. Source authority is E44. Failure mode prevented is
catastrophic forgetting caused by constant weight multiplier. Verifiable by
grep on the weight calculation expression. Current status: verifiable only
after Phase 2 creates the file — the rule exists in the plan but no phase
creates a file where this formula lives. This is a gap.

Rule D row: element type is critical rule. Claims to enforce canonical domain
name format. Source authority is locked decision D-04. Failure mode prevented
is long form domain names appearing in code. Verifiable by grep on the Domain
enum and any switch statements over domain values.

Required rows for a sample of Gate B commands:

Phase 2 VocabSize gate row: claims to detect if VocabSize is not exactly 22.
Verifiable. Failure mode: experiments calibrated to 22 produce wrong results
if constant changes.

Phase 3 pipeline order gate row: claims to detect if StackDetector appears
after SemanticNormalizer in the pipeline file. Verifiable by comparing line
numbers. Gap: this gate only runs on Phase 5 when the pipeline file exists,
but Phase 3 creates StackDetector before the pipeline file exists. The gate
cannot enforce the order until Phase 5.

Phase 5 SUCCESS mapping gate row: claims to detect if the mapping produces
a string containing "SUCCESS" instead of "return Validate". Verifiable.
Gap: the grep command checks for "return Validate" but does not check that
no other mapping also contains "SUCCESS" as a string value. A future mapping
could introduce a regression this gate would not catch.

Gate B:
```bash
grep -c "Rule A\|Rule B\|Rule C\|Rule D\|VocabSize\|SUCCESS.*RETURN\|pipeline order" \
  PLANNING-MAPPING-SEMANTIC-COMPILER-ANALYSIS-TABLE-v1.md
# Expected: 7 or more — one hit per required row
```

### Document 2 — Gap registry

File: PLANNING-MAPPING-SEMANTIC-COMPILER-GAP-REGISTRY-v1.md

Content: every gap found in the planning document analysis table, with one
entry per gap. Each entry names the gap, the element where it was found, the
failure mode that could slip through, and the minimum addition to the plan
that would close it.

Required entries based on what was read in the plan:

Gap 1 — Rule C has no file to live in: the dynamic weight formula (Rule C,
source E44) is stated as a critical rule but no phase in the plan creates a
file where this formula would be implemented. The formula is relevant to
continuous learning infrastructure (project 14 and project 16), not to the
semantic compiler directly. The rule may belong in a different plan, or the
plan needs a note directing the executor to the correct project. As written,
an executor following this plan has no phase that creates or verifies the
Rule C implementation.

Gap 2 — Phase 3 pipeline order gate fires too late: the Gate B command for
pipeline order requires the pipeline file to exist (Phase 5), but Phase 3
introduces StackDetector. An executor could implement StackDetector in the
wrong position and not discover this until Phase 5 Gate B runs. The gap is
that no early-phase gate enforces the order constraint before the file that
would violate it exists.

Gap 3 — Rule B SUCCESS regression not fully covered: the Phase 5 Gate B
checks for "return Validate" in the specific VALIDATE_OP mapping but does not
check that no other entry in the Templates dictionary contains "SUCCESS" as a
literal string value. A future mapping addition could introduce the failure
mode without triggering the gate.

Gap 4 — Loading sequence missing experiment evidence: the loading sequence
names EXPERIMENT-RESULTS-MAP.md as the source for E01 through E27 validated
results, but does not name the specific experiment files for E20 and E23
whose results are encoded in Rules A and B. An executor who reads the map
but not the original experiment files may not understand why the rules exist
and may weaken them.

Gap 5 — Test coverage missing for Rule C: no test name in the plan covers
the dynamic weight formula. The test "LogicExtractor_VocabSize_IsExactly22"
covers the vocabulary constant. No equivalent test covers the weight
calculation. If Rule C were implemented in this project (which Gap 1 shows it
is not), there would be no behavioral test for its most critical property.

Gate B:
```bash
grep -c "Gap [0-9]\|Rule C.*no file\|pipeline order.*late\|SUCCESS.*regression\|loading sequence.*missing\|test coverage.*Rule C" \
  PLANNING-MAPPING-SEMANTIC-COMPILER-GAP-REGISTRY-v1.md
# Expected: 5 or more — one hit per required gap
```

### Document 3 — Arbiter binding specification

File: PLANNING-MAPPING-SEMANTIC-COMPILER-ARBITER-BINDING-v1.md

Content: the arbiter set for the semantic compiler planning domain, with one
entry per arbiter. Each entry names the arbiter, what decision it governs,
what input it receives, and what verdict it produces.

Required arbiters:

Pipeline order arbiter: governs the Rule A constraint. Input: the pipeline
file after Phase 5 and the set of line numbers for StackDetector and
SemanticNormalizer references. Verdict: pass if StackDetector line is less
than SemanticNormalizer line, block if not. This arbiter must not fire before
Phase 5 because the pipeline file does not exist earlier.

Template mapping arbiter: governs the Rule B constraint. Input: the Templates
dictionary content in TemplateBeautifier. Verdict: pass if VALIDATE_OP SUCCESS
maps to a string containing "return" and not containing "SUCCESS" as a
substring, block if either condition fails. This arbiter covers the regression
gap identified in Gap 3 by checking all entries, not just the named one.

Vocabulary constant arbiter: governs the VocabSize equals 22 constraint.
Input: the VocabSize constant value in LogicExtractor. Verdict: pass if value
is exactly 22, block if not. This arbiter has no exception path — no value
other than 22 is acceptable regardless of other changes.

Domain name arbiter: governs the Rule D constraint. Input: the Domain enum
definition and any switch or match statements over Domain values in the project.
Verdict: pass if all enum values use short form (CODE_Q, BUG, PLAN, ARCH,
REFACTOR), block if any long form appears in code. This arbiter must check
all files, not only Core.cs.

Loading sequence arbiter: governs the loading sequence completeness. Input:
the list of documents actually read before execution begins. Verdict: pass if
all six named documents were read, block if any is absent. This arbiter also
checks that the experiment source files for E20 and E23 are accessible to
the executor even if not explicitly loaded, addressing Gap 4.

Rule C placement arbiter: governs the Rule C constraint. Input: the plan text
and the set of files created across all five phases. Verdict: block if Rule C
is stated as a critical rule but no phase creates a file where the formula
lives and no note directs the executor to the correct project. This arbiter
catches Gap 1.

Gate B:
```bash
grep -c "pipeline order arbiter\|template mapping arbiter\|vocabulary constant arbiter\|domain name arbiter\|loading sequence arbiter\|Rule C placement arbiter" \
  PLANNING-MAPPING-SEMANTIC-COMPILER-ARBITER-BINDING-v1.md
# Expected: 6 or more
```

### Document 4 — Evidence schema

File: PLANNING-MAPPING-SEMANTIC-COMPILER-EVIDENCE-SCHEMA-v1.md

Content: defines exactly what the experiment must write as evidence. Fields
required in the evidence JSON:

- experiment identifier
- planning document identifier and version
- domain governed
- critical rule analysis: one entry per rule with source verified, rule text
  accurate, and file verifiable fields
- Gate B analysis: one entry per phase gate with detection present, false
  negative risk, and false positive risk fields
- loading sequence analysis: one entry per document with necessary field and
  gap field
- test name analysis: one entry per test with behavior covered and experiment
  provenance fields
- gap registry: one entry per gap with gap name, element, failure mode, and
  minimum fix fields
- arbiter verdicts: one entry per arbiter with verdict and evidence fields
- overall verdict: fully mapped, partially mapped with named gaps, or
  incorrectly mapped with named violations

Gate B:
```bash
grep -c "critical rule analysis\|Gate B analysis\|loading sequence analysis\|test name analysis\|gap registry\|arbiter verdicts\|overall verdict" \
  PLANNING-MAPPING-SEMANTIC-COMPILER-EVIDENCE-SCHEMA-v1.md
# Expected: 7 or more
```

### Document 5 — Human result checklist

File: PLANNING-MAPPING-SEMANTIC-COMPILER-HUMAN-RESULT-CHECKLIST-v1.md

Content: defines what a reviewer can determine from the evidence without
re-running the experiment.

A passing result must allow a reviewer to answer:

Are all four critical rules complete and accurately stated against their source
experiments?

Does each Gate B command correctly detect its target violation? Are there
violations any gate would miss?

Is the loading sequence sufficient to establish the context all rules and gates
depend on?

Do the test names cover all behaviors the experiments proved were important?

Which gaps were found, what failure mode does each gap allow through, and what
is the minimum change to the plan that would close each gap?

Which arbiters passed and which blocked, and for each that blocked, what
specific plan element caused the failure?

Gate B:
```bash
grep -c "critical rules complete\|Gate B command\|loading sequence sufficient\|test names cover\|gaps were found\|arbiters passed" \
  PLANNING-MAPPING-SEMANTIC-COMPILER-HUMAN-RESULT-CHECKLIST-v1.md
# Expected: 6 or more
```

---

## Orchestration abilities under test

### Ability 1 — Rule provenance verification

Each critical rule traces to a named source authority. The experiment verifies
that the source authority actually supports the rule as stated. A rule that
overstates its source, understates it, or misquotes it is a provenance failure.

Metric: provenance mismatch count equals zero.

### Ability 2 — Gate B detection accuracy

Each Gate B command is tested against a scenario where the violation is present
and a scenario where it is absent. The gate must fire in the first scenario and
not fire in the second.

Metric: false negative count (violation present, gate does not fire) equals
zero. False positive count (violation absent, gate fires) equals zero.

### Ability 3 — Loading sequence sufficiency

Every document that a critical rule or gate depends on is present in the
loading sequence. A rule that fires without its source document loaded is
operating from memory or inference rather than from verified context.

Metric: missing necessary document count equals zero.

### Ability 4 — Test name behavioral coverage

Every experiment with a significant finding (more than ten percent improvement
or a named failure mode) has at least one test name in the plan that covers
the behavior that finding proves matters.

Metric: uncovered significant finding count equals zero.

### Ability 5 — Gap detection completeness

The experiment finds all gaps in the plan — missing rules, late-firing gates,
partial regression coverage, loading omissions, and test coverage holes. The
gap registry is complete if no gap that would cause a silent failure in an
executor session is absent from the registry.

Metric: this metric is evaluated by a human reviewer against the gap registry.
A gap registry that names fewer than the gaps found in the analysis table is
incomplete.

### Ability 6 — Arbiter binding coverage

The arbiters collectively cover every critical constraint in the plan. No
constraint that could be violated without any arbiter detecting it is left
uncovered.

Metric: uncovered constraint count equals zero.

---

## Pass gates

| Gate | Required result |
|---|---|
| Rule provenance | All four critical rules trace accurately to their source experiments |
| Gate B detection | No false negatives and no false positives across all five phase gates |
| Loading sequence | All necessary documents present; no rule fires from a missing source |
| Test coverage | All experiments with significant findings have at least one binding test |
| Gap registry completeness | All silent-failure gaps named with minimum fix specified |
| Arbiter binding | All six arbiters produce verdicts; no critical constraint left uncovered |
| Evidence completeness | JSON and markdown evidence written with per-ability traces |
| Existing tests | Full suite remains at or above 985 passing |

---

## What this will tell us

If the experiment passes on all gates: the semantic compiler plan correctly
maps its critical rules to their source experiments, its Gate B commands
correctly detect their target violations, its loading sequence is sufficient,
its test names cover the behaviors that matter, and the arbiter set covers all
critical constraints.

If rule provenance fails: a critical rule misquotes or overstates its source
experiment. The specific rule and the specific discrepancy are named. The
experiment result that was misquoted is cited.

If Gate B detection fails on a false negative: a violation can exist in the
produced code and pass the gate command. The specific violation and the specific
gate that missed it are named. The minimum addition to the gate command that
would catch it is specified.

If Gate B detection fails on a false positive: a correctly implemented file
triggers the gate command. The gate condition is too strict or checks the
wrong text. The minimum correction to the gate command is specified.

If loading sequence fails: a critical rule or gate fires in an executor
session without the document that establishes the context for that rule. The
specific missing document and the specific rule that depends on it are named.

If test coverage fails: an experiment with a significant finding has no
behavioral test. The finding, the experiment, and the improvement magnitude
are named. The test name and assertion that would cover it are specified.

If gap registry is incomplete: a silent-failure gap was found during analysis
that was not in the registry. The gap and the minimum fix are named.

Each failure tells the plan author exactly which element needs to be
strengthened, corrected, or added.

---

## Relationship to the guidance and skills experiments

This experiment tests planning documents — the artifacts that govern executor
sessions. The guidance experiment tests the session start prompt that governs
how any session opens. The skills experiment tests the trigger rules that route
skills to task domains.

The dependency direction for executor sessions is: guidance fires first
(session start prompt), then skill routing (which skills to load), then plan
execution (which phases to run, which gates to check). This experiment tests
the third layer. Gaps found here affect any executor session that follows this
plan. Gaps in the guidance experiment affect every session type. Gaps in the
skills experiment affect skill routing for any domain.

All three experiments can produce their round A documents in parallel. Round B
for this experiment does not depend on the guidance or skills experiments
completing first, because it tests the plan document directly rather than the
machinery that loads it.

---

## Execution sequence

### Round A — Documents

Produce all five documents named above. Stop for review. No experiment code
until all five documents are reviewed and accepted.

Source authorities the executor must read before writing any document, in
this order:

- The semantic compiler session plan version 1.0 — the document under test,
  read completely before any analysis begins
- The experiment results map (docs/EXPERIMENT-RESULTS-MAP.md in the codebase)
  — to verify each critical rule against its source experiment result
- The locked decisions document (docs/DECISIONS-LOCKED.md) — to verify Rule D
  against locked decision D-04
- The behavioral corrections registry — for any documented failure mode that
  overlaps with the plan's critical rules

### Round B — Deterministic experiment

Implement the plan analysis checks as deterministic functions that apply
each rule and gate command from the plan, compare against the expected behavior
trace, run each Gate B command against a violation-present and violation-absent
scenario, and write the evidence files.

Commands to run after round B:
```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "PlanningMapping" 2>&1 | tail -5
# Expected: all planning mapping tests pass

dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Expected: 985 or more passing, 0 failing
```

Create round zip containing all five documents, experiment source file,
test file, evidence files, and round manifest.

Stop for review.

### Round C — Extend to a second planning document

Only after round B passes and the failure diagnosis is complete, select a
second planning document from the project plan set. The selection is driven
by the gap types found in round B. If round B finds primarily gate detection
gaps, the second document should be one with a similar gate structure. If
round B finds primarily loading sequence gaps, the second document should be
one with a different loading sequence to compare against.

Do not select the second document before round B evidence is reviewed.

---

## What not to do

Do not execute the semantic compiler plan as part of this experiment. This
experiment tests the plan document, not the code the plan would produce. The
semantic compiler implementation is separate executor work. This experiment
produces evidence about the plan's mapping quality, not about the compiler's
behavior.

Do not treat the four critical rules as exhaustive. The experiment checks
whether they are complete — it may find that additional rules are needed for
failure modes the plan does not currently name.

Do not accept a gap registry with zero entries without verifying the analysis
table produced no gaps. A plan of this complexity analyzed against four source
experiments and five phase gates will produce at least some gaps. A zero-entry
gap registry indicates the analysis was not complete.
