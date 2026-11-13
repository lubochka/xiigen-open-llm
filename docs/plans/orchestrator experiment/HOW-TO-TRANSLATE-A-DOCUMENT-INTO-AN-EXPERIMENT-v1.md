# How To Translate a Plan, Skill, or Guidance Document Into an Experiment
# A method derived from all seven experiment designs produced in this project
# Version: 1.0 — 2026-05-04

---

## What This Document Is

Every experiment designed in this project followed the same underlying method,
even though each addressed a different kind of source document. This guidance
extracts that method into a repeatable process. Anyone with a plan, a skill
file, or a guidance document can follow this process to produce a well-formed
experiment that tests whether the source document does what it claims to do.

The method has six steps. They must run in order. Skipping a step produces an
experiment that is structurally present but intellectually hollow — it runs,
produces evidence, and tells you nothing useful.

---

## The Central Question

Before anything else, state the central question the experiment must answer.

Every experiment in this project had exactly one central question. Not a list
of things to check — one question. The question takes this form:

> Given [source document] and [the domain it governs], does the document
> correctly [claim A], [claim B], and [claim C] — or does it [failure mode 1],
> [failure mode 2], and [failure mode 3]?

The positive half names what the document claims to do correctly. The negative
half names the specific ways it could fail without the failure being obvious.
Both halves must be present. A question without the negative half is not a
question — it is an outline of the document's own purpose statement.

Examples from the seven experiments:

The orchestration proof asked: given a complex human problem, does the
nine-node pipeline correctly decompose it, plan it with real dependencies,
detect constraint violations, handle knowledge edges without inventing answers,
and produce a human-readable result that is honest about what was resolved
and what was not — or does it lose edges, violate dependency order, invent
missing competence, or produce a result that looks complete but hides blocked
work?

The planning document binding experiment asked: given a planning document and
the domain it governs, are all five principles enforced by the gate checks
and tests, are component phases correctly sequenced, and do the Gate B
commands catch what they claim to catch — or are some principles unprotected,
some bindings sequentially invalid, and some gates weaker than the tests that
sit next to them?

The skill domain mapping experiment asked: given a planning task and a skill
library, does the orchestrator correctly identify which skills belong, which
are misbound, and which are missing — or does it produce a coverage illusion
from broad assignments, miss unlisted gaps, or fail to distinguish a concern
from a reprocess?

If you cannot state the central question in this form, the source document is
not ready to become an experiment. Read the document again and find what it
claims to prevent.

---

## Step 1 — Read the Source Document as a Set of Claims

Every plan, skill file, and guidance document makes claims. The first step is
to find all the claims and classify them.

There are five claim types:

**Enforcement claims.** The document says it prevents a specific failure mode.
The claim is that some mechanism — a rule, a gate check, a test assertion, a
load order — will catch a violation. These are the most important claims to
examine because enforcement failures are silent. The document looks complete
and the violation passes undetected.

**Binding claims.** The document assigns something to a category, a phase, a
domain, or a role. The claim is that the assignment is correct and complete.
Binding failures produce correct-looking output that belongs to the wrong
context.

**Coverage claims.** The document says it handles a specific situation or
applies to a specific scope. The claim is that nothing in that scope falls
through without a handler. Coverage failures look like gaps but are announced
as completeness.

**Sequencing claims.** The document says steps happen in a specific order and
that the order matters. The claim is that earlier steps establish context that
later steps depend on. Sequencing failures are the most architecturally
dangerous because they allow later steps to operate on missing or incorrect
context from earlier steps.

**Honesty claims.** The document says it acknowledges its own limits — names
its known gaps, marks its boundaries, flags its unknowns. The claim is that
what the document does not cover is explicitly named rather than silently
omitted. Honesty failures are the hardest to find because the document appears
complete.

Write down every claim you find, classified by type. This list is the raw
material for Step 2.

---

## Step 2 — Find the Real Failure Modes

A claim is not interesting on its own. What makes a claim interesting for an
experiment is its failure mode — the specific way the claim could be wrong
without the failure being visible from the surface of the document.

For each claim you found in Step 1, ask: what would have to be true for this
claim to appear satisfied while actually failing?

This question has a specific form for each claim type:

For an enforcement claim: what violation would pass the stated mechanism?
The planning document binding experiment found that the Phase 3 Gate B grep
for the max_weight formula would pass if the formula appeared in a comment
above a hardcoded constant. The claim was satisfied on the surface. The
enforcement failed in substance.

For a binding claim: what assignment looks correct but belongs to the wrong
category? The skill domain mapping experiment found that loading the temporal
behavior design skill for a bid processing task looks like correct planning-
layer skill loading, but the temporal skill governs state machines rather than
stream processing. The assignment looked right by layer but was wrong by domain.

For a coverage claim: what situation in the stated scope has no handler? The
learning set translation experiment found that the self-referential edge on
the Domain applier node was in the edge inventory and warned against removal,
but had no explicit source citation. The coverage claim was satisfied by the
warning. The actual coverage was inferred from adjacent material.

For a sequencing claim: what later step depends on a context that an earlier
step has not established? The guidance domain mapping experiment found that
sessions produce a governance output wall before the absorption paraphrase,
because the load order in steps one and two fires before the absorption
directive in the session opening. The sequencing claim said governance-first.
The actual sequence was governance-before-absorption.

For a honesty claim: what is absent from the document's named gap list that
a careful reading reveals? The learning set translation experiment found that
the translation plan listed five known gaps but the simulation test criterion
was unmeasurable — a sixth gap the plan did not name. The honesty claim was
satisfied by the five listed gaps. The sixth was invisible.

For each failure mode you find, write one sentence: if this failure were
present, what observable evidence would exist? If you cannot answer this
question, the failure mode is not real enough to test.

---

## Step 3 — Select Three Scenarios

Three scenarios is the right number for a first experiment. Not one — one
scenario cannot distinguish a design failure from a coincidence. Not five —
five scenarios produces so much evidence that the failure classification
becomes work rather than insight.

Each scenario must put pressure on a different failure mode. The three failure
modes you select should be the three that have the highest risk of being
silent — failures that would pass all stated checks while violating the
document's core purpose.

Each scenario must be hard enough to require real judgment. A scenario that
any mechanical check could handle is not testing the orchestrator — it is
testing grep. The scenarios that are worth running are the ones where the
answer depends on understanding the relationship between claims, not just
whether a word appears in a file.

Each scenario must have a known correct answer before the experiment runs.
This is the most important constraint. If you do not know what the correct
output looks like at each node boundary, you cannot tell whether the
experiment passed or failed. Writing the expected outputs is the work of
Step 4. Do not select scenarios you cannot specify in advance.

For each scenario, write:

- Its name and a one-sentence description of what it tests.
- The specific failure mode it exercises.
- Why this scenario is hard enough — what makes it require judgment rather
  than mechanical checking.
- The A input: exactly what the scenario gives to the orchestrator.
- The expected finding: what the orchestrator must identify.
- The expected arbiter verdict: concern, reprocess, or pass, and why.

The three verdict types have specific meanings. A concern verdict means the
finding is real but does not prevent the document from being used — it is
something to add or correct. A reprocess verdict means the finding is a
structural defect that prevents the document from functioning correctly —
it must be corrected before the document can be used. A pass verdict means
the mechanism being tested is correctly implemented and would catch the
failure it claims to catch.

Most first-scenario findings produce concern verdicts. Reprocess verdicts
are appropriate when the defect is in a testing mechanism or a dependency
structure, not just in a content gap.

---

## Step 4 — Write the Oracle Documents

The oracle is the complete expected answer before the experiment runs. It
is not produced by running the experiment — it is produced by the architect
working through the source document and reasoning about what correct
orchestration looks like at each step.

Five oracle documents are required for every experiment. They must be produced
in this order because each depends on the one before.

**Document 1 — Examination Domain Map.** This document maps the claim types
from Step 1 to their examination domains. For each domain, it defines the
vocabulary of a formal domain statement — what a covered finding looks like,
what a boundary finding looks like, what a knowledge-edge finding looks like,
and what a conflict finding looks like. This vocabulary is what the domain
translator node uses when it converts domain packets into formal statements.
Without this document, the translator produces statements in general language
rather than domain-specific language, which means the planner and arbiters
receive ambiguous input.

**Document 2 — Binding Failure Taxonomy.** This document defines the failure
types from Step 2 with enough precision that an arbiter can distinguish
between them. Each failure type needs: its name, the observable signal that
reveals it, the correct arbiter verdict type for it, and why it is distinct
from the other failure types. The taxonomy is what prevents the arbiter from
producing a concern verdict when the correct verdict is reprocess, or
classifying an enforcement gap as a sequencing violation.

**Document 3 — Expected Orchestration Trace.** This is the oracle for the
full pipeline run. One section per scenario. For each scenario, the expected
output at every node boundary — what the domain splitter produces, what the
domain translator produces for each packet, what the scheme planner produces
including the dependency graph, what the arbiter verdicts are, and what the
human-readable result says. Write this document as if you are the orchestrator
and you are producing correct output. Every failure mode from Step 2 must
appear explicitly in this trace — either as a detected finding or as a
verified pass.

The most critical section of the expected trace is always the scheme planner
output. The planner output is where dependency ordering becomes visible. If
the planner treats steps as independent when they are dependent, the entire
downstream processing operates on a wrong assumption. Check the dependency
graph of your expected planner output before finalizing it: does each step
that depends on an earlier step actually have the earlier step as a listed
dependency? Can the stated parallel groups actually run in parallel given the
dependencies?

**Document 4 — Human Result Standards.** This document defines what a passing
human-readable result looks like for each scenario. It names the required
sections, the required statements within each section, the forbidden
statements, and the forbidden omissions. It is the standard that the formatter
output is measured against — not by the formatter itself, but by a human
reviewer reading the final output. Every finding the experiment is designed
to detect must appear in the human result as a concrete, actionable statement.
A finding that is present in the machine trace but absent from the human result
is an orchestration failure even if all machine metrics pass.

**Document 5 — Executor Contract.** This document tells the executor exactly
what to do in Round B. Files to read first. Files to create. Test names to
add. Evidence files to write. Commands to run. Pass gates. What not to change.
Zip requirement. The executor contract must be specific enough that an
executor can run Round B without making any design decisions. Every decision
that the executor would need to make should already be made in the contract.

The oracle documents are the most valuable output of the experiment design
process. If the experiment never runs, the oracle documents are still worth
producing because the act of writing them forces you to find the gaps in the
source document that you would otherwise discover only during execution — at
which point finding them costs more.

---

## Step 5 — Identify the Training Gap

Before the experiment runs, identify which nodes need new training pairs and
which can use human proxy for the first pass.

The rule is: use human proxy where the node's structural role is clear and
its content is deterministic. Use training pairs where the node's output
requires judgment that the experiment is specifically testing.

For most first experiments, nodes 00 and 01 can use human proxy because the
compilation step and the domain split are mechanical enough to be done by
hand. Node 02 (the domain translator) usually needs one or two new pairs
because the vocabulary of the new domain is different from prior training.
Node 03 (the scheme planner) always needs new pairs because the dependency
ordering is the core of what the experiment is testing — you cannot proxy
the most important output and call it an orchestration proof. Nodes 04 through
06 can usually use human proxy for the first pass. Node 07 (the arbiter
selector) needs a pass case pair and a fire case pair for each new failure
type. Node 08 (the arbiter units) needs pairs for the specific verdict types
the experiment is exercising — if you are testing a reprocess verdict for
the first time, you need a pair that teaches that verdict type. Node 09
(the formatter) needs a pair for the new audience type if the experiment
introduces one.

Write a gap map: one row per node, stating whether it uses an existing pair,
a new pair, or human proxy, and why.

---

## Step 6 — Define the Pass Gates and Failure Classification

The pass gates are the binary conditions that determine whether the experiment
passed. Each gate maps to one claim from Step 1. Each gate must be checkable
without interpretation — either the condition holds or it does not.

The failure classification is the most important product of the experiment.
Before the experiment runs, define what it means if each gate fails:

What does it tell you about the source document if gate X fails? What is the
priority fix? Is it a content fix (the document needs a new rule or a better
gate check), a binding fix (the assignment is wrong and must be corrected), or
a taxonomy fix (the failure type definitions are not precise enough for the
arbiter to distinguish them)?

Write the failure classification table before running the experiment. If you
cannot fill in the table before the run, the experiment is not ready.

---

## The Pattern That Appears in Every Experiment

Reading all seven experiments, the same pattern appears in every one:

The source document has a stated purpose and a set of mechanisms. The
mechanisms look sufficient on the surface. The experiment finds at least one
mechanism that passes when it should fail, at least one gap the document does
not acknowledge, and at least one finding where the correct arbiter verdict
is not the obvious one.

The planning document binding experiment: Principle 2 had no enforcement
mechanism. The ICodeReader-to-ISessionStorage cast was a conditional risk
the plan did not verify. The Phase 3 grep was weaker than the test it sat
next to.

The learning set translation experiment: the broad secondary target assignment
created a coverage illusion. The self-edge had inferred rather than explicit
source support. The simulation criterion was unmeasurable.

The skill domain mapping experiment: the wrong skill could be loaded for the
right layer and look correct. A dependency of a plan review check could be
missing without the check signaling incompleteness. A trigger condition could
be present in the session opening but not processed as a trigger event.

The guidance document experiment: absorption fired after governance output
rather than before. Session type was inferred from document content rather
than declared by keyword. The load order produced visible initialization
before the session knew it should not.

The pattern is: the surface looks correct. The failure is in the relationship
between elements, not in any single element examined alone. The experiment is
the tool for finding failures in relationships.

---

## What the Experiment Is Not

An experiment of this kind is not a code review. It does not find bugs in
implementation. It finds structural failures in how a document's claims
relate to each other and to the domain they govern.

An experiment is not a coverage audit. It does not check whether all items
in a list are present. It checks whether the items that are present are
correctly bound to the domain, correctly ordered relative to each other,
and correctly enforced by the stated mechanisms.

An experiment is not a simulation of running the document's instructions.
It is a test of whether the orchestrator that would run those instructions
correctly understands the domain, correctly identifies the failure modes,
and correctly classifies the findings.

An experiment without a known expected answer is not an experiment. It is
an exploration. Explorations are valuable, but they are not experiments —
they do not produce pass/fail evidence against a prior specification. Write
the oracle before running the experiment. If you cannot write the oracle,
the source document is not ready to be tested.

---

## Quick Reference — The Six Steps

```
Step 1 — Read the source document as a set of claims
         Classify: enforcement, binding, coverage, sequencing, honesty
         
Step 2 — Find the real failure modes
         For each claim: what violation would appear satisfied while actually failing?
         
Step 3 — Select three scenarios
         One per failure mode. Hard enough to require judgment. Known answer in advance.
         
Step 4 — Write five oracle documents in order:
         1. Examination domain map (vocabulary per domain)
         2. Binding failure taxonomy (failure types and arbiter verdicts)
         3. Expected orchestration trace (the oracle)
         4. Human result standards (what the formatter must produce)
         5. Executor contract (what Round B must do exactly)
         
Step 5 — Identify the training gap
         Per node: existing pair, new pair, or human proxy — and why
         
Step 6 — Define pass gates and failure classification
         Per gate: what does failure tell you? What is the fix?
```

---

## The Document Types and What Makes Each Testable

Different source document types have different testable properties. Use this
table to find the claim types most likely to be productive for the kind of
document you are examining.

| Document type | Highest-value claim types | Most common failure mode | Typical first scenario |
|---|---|---|---|
| Implementation plan | Enforcement (principle gates), Sequencing (phase order) | A principle with no enforcement mechanism | Find the principle whose Gate B would pass a violation |
| Skill file | Binding (task-to-skill assignment), Coverage (activation trigger completeness) | Wrong skill loaded for the right layer | Find the task where the skill's domain does not match |
| Guidance document | Sequencing (load order), Enforcement (behavioral rule detection) | A step that fires before its prerequisite is established | Find the rule that appears satisfied while the prerequisite is missing |
| Learning set translation plan | Coverage (source line routing), Honesty (named vs unnamed gaps) | An edge with inferred rather than explicit source support | Find the gap the plan does not list |
| Training set pair file | Binding (A-to-node assignment), Coverage (coverage status accuracy) | A B output that describes rather than shows | Find the pair where B is a description of output rather than the output itself |

---

## The One Rule That Must Not Be Broken

Write the expected answers before running the experiment.

If you write the expected trace after seeing the experiment output, you are
not testing the document — you are documenting the experiment. The oracle must
precede the run. The oracle is the definition of what correct orchestration
looks like for this source document. The experiment measures the distance
between the oracle and the actual run output. Without the oracle, there is
nothing to measure against.

This rule applies even when the oracle is hard to write. Especially when the
oracle is hard to write. The difficulty of writing the expected trace is the
signal that the experiment is testing something real. If writing the oracle
is easy, the experiment is not hard enough.
