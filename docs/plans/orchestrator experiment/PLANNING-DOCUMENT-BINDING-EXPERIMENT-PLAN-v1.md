# Planning Document Binding Validation Experiment
# Orchestration proof applied to a real planning artifact:
# examining whether a plan is correctly mapped to its task, domain, and binding contracts
# Status: DESIGN — awaiting approval before any documents are produced

---

## What This Experiment Proves

A planning document makes claims. It claims to govern a specific task. It
claims to bind specific components to specific phases. It claims that its
principles are sufficient to prevent known failure modes. It claims that its
gate checks are measurable and binary. It claims that its test count
arithmetic is consistent.

None of those claims are verified by the document itself. Verifying them
requires an examiner that understands the domain, knows the failure modes,
and can determine whether each binding is correct — not just whether the
document is internally consistent.

The dynamic flow orchestrator is that examiner. This experiment tests whether
the orchestrator can receive a real planning document as input, decompose it
into the domains it touches, translate each domain into formal statements,
plan an examination sequence, process each claim, apply the correct arbiters,
and produce a human-readable verdict that names what is correctly bound,
what is incorrectly bound, what is missing, and what would fail in practice.

The input is the continuous learning session plan. It is a good subject
because it is a real document with real structure, real domain dependencies,
real binding contracts between components, and real failure modes that are
known from prior sessions. The experiment does not need to invent a scenario —
the document provides all the material needed.

---

## What The Plan Claims and What Must Be Examined

The continuous learning plan makes five categories of claim. Each category
is a domain that the orchestrator must identify, translate, and examine.

### Claim category 1 — Component-to-domain binding

The plan binds eight components to a specific C# namespace (ContinuousLearning)
and declares that each component belongs exclusively to one phase. The claim
is that this assignment is correct and complete — no component is in the
wrong phase and no required component is missing.

What the orchestrator must check: does each component belong to the phase
it is assigned to, based on the dependency order of the components? A
component that is used by a later component but assigned to a later phase
than the one that uses it is a binding violation. A component that is
referenced in the interfaces but absent from the component map is a coverage
gap.

The specific risk here: the plan assigns ContinuousLearningPipeline to Phase 5
alongside DomainModelRegistry. But ContinuousLearningPipeline depends on all
five of the interface types declared in Phase 1. That dependency is correctly
ordered. What the arbiter must check is whether the IContinuousLearningPipeline
interface — declared in Phase 5 inside the pipeline file — is too late. The
plan says DomainModelRegistry.Register is called in step 5 of the pipeline's
RunCycleAsync. If something outside this plan needs to call RegisterAsync on
the pipeline before Phase 5 exists, that is a sequencing violation.

### Claim category 2 — Principle-to-enforcement binding

The plan declares five principles. Each principle has a stated enforcement
mechanism: some are enforced by Gate B bash checks, some are enforced by
test assertions, and one (Principle 4, ISemanticCompiler injected not copied)
is enforced by both. The claim is that every principle has an enforcement
mechanism and that the mechanism is sufficient to catch a violation.

What the orchestrator must check: for each principle, is there a Gate B
command or test assertion that would actually fail if the principle were
violated? Principle 1 (OperationResult everywhere) has a Gate B grep in
Phase 5 that checks for public methods returning non-OperationResult types.
But the grep pattern is complex and may produce false negatives — it excludes
records, interfaces, and abstract classes, but static factory methods on
records could return raw values without triggering the pattern.

Principle 2 (BaseService) has no Gate B check. The plan does not include
a grep that verifies every service class extends BaseService. This is a
coverage gap in the principle enforcement.

### Claim category 3 — Test count arithmetic binding

The plan accumulates tests across five phases: 877 baseline, then plus 3,
plus 6, plus 5, plus 4, plus 4, reaching 899. The claim is that this
arithmetic is correct and that the tests in each phase actually test the
components declared in that phase.

What the orchestrator must check: does the arithmetic hold (it does: 877
plus 22 equals 899), and does each phase's test list actually test the
components assigned to that phase? Phase 4 adds four tests for
IncrementalTrainer but one of them (the gradient reimplementation check)
is a bash grep command, not a dotnet test assertion. The plan lists it as
a test in the test count comments, but the Gate B section calls it as a
bash command. There is an ambiguity about whether this counts toward the
test total.

### Claim category 4 — External dependency binding

The plan references four external dependencies: 10.LoRA (LoraApplicator),
11.DPO (DpoTrainer), 21.AgentInfrastructure (BaseService, ICodeReader,
ICodeWriter, ISessionStorage), and 29.SemanticCompiler (ISemanticCompiler).
The claim is that each external dependency is accessed through the correct
interface and that no dependency is accessed by copying its implementation.

What the orchestrator must check: each reference to an external type must
trace to an injected interface, not to a concrete instantiation. The plan's
Principle 3 (Fabric First) is specifically about this. The Phase 2 Gate B
check verifies that StackDetector and SemanticNormalizer are not reimplemented
in the 14 namespace. But it does not check whether the concrete types from
21.AgentInfrastructure (ICodeReader, ICodeWriter) are correctly passed through
interfaces rather than instantiated directly inside the service classes.

A specific risk: the ProjectIndexer constructor contains this line:

```
base(reader as ISessionStorage ?? throw new ArgumentException(...))
```

This casts ICodeReader to ISessionStorage inside the constructor body. If
ICodeReader does not implement ISessionStorage in the actual codebase, this
throws at construction time. The plan does not verify that ICodeReader
implements ISessionStorage before writing this pattern.

### Claim category 5 — Gate B sufficiency binding

The plan has five Gate B sections. Each Gate B is a set of bash commands
that must pass before the phase is committed. The claim is that each Gate B
is sufficient to detect a violation of the phase's requirements.

What the orchestrator must check: for each Gate B command, is there a
violation of the phase's requirements that the command would not catch? This
is the hardest domain to examine because it requires knowing what violations
are possible, not just what the command checks.

The Phase 3 Gate B checks that the max_weight formula is present using a
grep for the pattern `totalExamples.*numDomains`. But this grep would pass
if the formula is present in a comment but not in the actual computation.
A comment that says `// maxWeight = totalExamples / (numDomains × rareExamples)`
followed by actual code that hardcodes `double maxWeight = 10.0` would pass
the grep but violate Principle 5.

---

## The Three Scenarios

### Scenario D1 — Principle enforcement gap

**The examination input:** The complete continuous learning plan, presented
to the orchestrator with the question: are all five principles enforced by
the gate checks and tests?

**What the orchestrator must find:** Principle 2 (BaseService) has no
enforcement mechanism. No Gate B grep checks whether every service class
extends BaseService. No test assertion verifies it at compile time. A class
that omits the BaseService extension would pass all gate checks and all tests.

This is a coverage gap in the enforcement binding. The orchestrator must
identify it as a gap, name the specific principle that is unprotected, name
what kind of check would close the gap (a grep for class declarations that
do not contain `: BaseService`), and mark it as a finding in the examination
result.

**What the arbiters must do:** The principle coverage arbiter fires on
Principle 2 and produces a concern verdict — not a reprocess verdict, because
the principle statement itself is correct. The gap is in the enforcement, not
in the principle. The arbiter names the specific grep that would close the gap.

**Why this scenario is the right starting point:** It is the most clearly
defined gap and requires the least interpretation. The plan has five
principles and five Gate B sections. A mechanical check of which principles
appear in Gate B commands reveals the gap without ambiguity.

---

### Scenario D2 — Binding sequencing violation risk

**The examination input:** The component map and the phase assignments,
presented to the orchestrator with the question: is the phase ordering
correct given the dependency relationships between components?

**What the orchestrator must find:** The ProjectIndexer constructor contains
a cast from ICodeReader to ISessionStorage. This cast is either always valid
(ICodeReader implements ISessionStorage in the real codebase) or always
invalid (it throws ArgumentException at construction time). The plan does
not verify which is true before writing the constructor.

This is a binding sequencing risk — the plan writes code that depends on a
property of an external interface without first verifying that property
exists. The orchestrator must identify this as a conditional binding risk,
name the verification that must happen before the phase is executed (check
whether ICodeReader implements ISessionStorage in 21.AgentInfrastructure),
and classify it as a pre-condition gap rather than a principle violation.

The distinction matters for the formatter: a principle violation is a
definite problem that should block the plan. A pre-condition gap is a risk
that should be verified before execution, not after. The arbiter must produce
a concern verdict that specifies when the verification must happen (before
Phase 2, not as a Phase 2 gate check).

**Why this scenario is the right second step:** It requires the orchestrator
to distinguish between a definite violation and a conditional risk. That
distinction is the core judgment that separates an orchestrator from a
mechanical checker.

---

### Scenario D3 — Gate B sufficiency gap

**The examination input:** The Phase 3 Gate B section, presented to the
orchestrator with the question: would this gate catch a formula violation
where the correct formula appears in a comment but the incorrect constant
appears in the computation?

**What the orchestrator must find:** The Gate B grep
`grep -n "totalExamples.*numDomains"` would match a comment that contains
those words even if the actual computation uses a hardcoded constant. A
false pass is possible if the developer writes the formula as a comment
above a different implementation.

The correct Gate B for this principle is not a grep for the formula pattern
but a test assertion that actually calls ComputeMaxWeight with known inputs
and verifies the result. The plan already includes such a test (
FeedbackRouter_ComputeMaxWeight_Formula_1000_5_50_Returns4) in Phase 3's test
list. The Gate B grep is redundant and weaker than the test. The orchestrator
must recognize that the test is the correct enforcement mechanism and the
grep is a false sense of security.

This scenario requires the orchestrator to compare two enforcement mechanisms
for the same principle and determine which is stronger. It is the hardest
domain judgment in the three scenarios.

---

## Document Design

Four documents are required before any training pairs are written.

### Document 1 — Examination Domain Map

File: `PLANNING-EXAM-DOMAIN-MAP.md`

This document maps the five claim categories to their examination domains,
names the vocabulary each domain uses, and specifies what a formal domain
statement looks like for each category. It is the reference that the domain
translator node uses when producing formal statements from planning document
packets.

For example, the principle enforcement domain uses this vocabulary: principle
statement, enforcement mechanism type (gate check, test assertion, or both),
enforcement sufficiency (sufficient or insufficient), gap description, and
gap remediation. A formal domain statement in this vocabulary looks like:
"Principle 2 governs BaseService extension. Enforcement mechanism: none
present. Enforcement sufficiency: insufficient. Gap: no grep or assertion
checks class declarations for BaseService extension. Remediation: add
`grep -rn 'class.*:.*[^B]ase' 14.ContinuousLearning --include='*.cs'` to
Phase 1 Gate B."

### Document 2 — Binding Failure Taxonomy for Planning Documents

File: `PLANNING-EXAM-BINDING-FAILURE-TAXONOMY.md`

This document defines the binding failure types specific to planning document
examination: enforcement gap (a principle has no mechanism), pre-condition
gap (a step depends on an unverified external property), sequencing violation
(a component is used before it is defined), and gate weakness (an enforcement
mechanism passes when it should fail). For each type, it defines the detectable
signal, the failure vocabulary, the correct arbiter verdict, and the
distinction from the other types.

This document is the ground truth for evaluating whether the arbiters in the
three scenarios produce the correct verdict type — concern vs reprocess vs
pass — and whether the verdict uses the right vocabulary.

### Document 3 — Expected Examination Traces

File: `PLANNING-EXAM-EXPECTED-TRACES.md`

One section per scenario. For each scenario, the expected output at every
node boundary — the oracle the gate checks run against.

The most critical section is the node-03 scheme planner output for Scenario
D1. The planner must sequence the examination correctly: check which
principles exist first (node-01 domain split produces a packet for each
principle), then check which have enforcement mechanisms (node-02 translates
each principle packet into a formal statement with enforcement type), then
identify gaps (the planner's dependency graph shows that principle coverage
checking depends on both the principle list and the enforcement mechanism
inventory). The planner must not produce a flat list of checks — it must show
that the enforcement inventory step depends on the principle list step.

### Document 4 — Executor Contract

File: `PLANNING-EXAM-EXECUTOR-CONTRACT.md`

Exact files to read first, files to create, test names to add, evidence
files to write, commands to run, pass gates, what not to change, and zip
requirement for each round.

---

## Learning Set Design

The learning sets for this experiment teach the orchestrator to reason about
planning documents as structured examination inputs. The domain vocabulary
is different from both the registration domain and the skill binding domain,
but the structural patterns transfer directly.

The knowledge-edge pattern from the onboarding orchestration experiment maps
to the pre-condition gap pattern here: in both cases, the orchestrator must
identify something that cannot be resolved locally and name it explicitly
rather than inventing an answer. The orchestrator cannot determine whether
ICodeReader implements ISessionStorage by examining the planning document —
it must name this as a verification requirement rather than a binding
violation.

The conflict pattern from the registration training set maps to the gate
weakness pattern in Scenario D3: in both cases, the orchestrator identifies
a claim that contradicts a stronger requirement. The duplicate-account
instruction contradicted the identity consistency rule; the Phase 3 grep
contradicts the stronger test assertion. The arbiter handles both with a
concern verdict that names the contradiction explicitly.

### New pairs needed

Node-01 (domain splitter): three pairs, one per scenario. Each A input is
the relevant section of the planning document. Each B output is the set of
domain packets for that section — principle packets, component packets,
enforcement mechanism packets, external dependency packets.

Node-02 (domain translator): six pairs, two per scenario. Each A input is
one domain packet. Each B output is the formal domain statement in the
examination domain vocabulary defined in Document 1.

Node-03 (scheme planner): three pairs, one per scenario. The Scenario D1
pair is the most important — it must show a dependency-ordered examination
plan where the enforcement inventory step depends on the principle list step.
The Scenario D2 pair must show the pre-condition verification as a step that
is sequenced before Phase 2, not after.

Node-08 (arbiter units): six pairs, two per scenario. One concern verdict
and one pass verdict per scenario. The concern verdict for Scenario D1
names the specific grep that would close the Principle 2 enforcement gap.
The concern verdict for Scenario D2 names the specific interface property
that must be verified before Phase 2 executes.

Node-09 (formatter): three pairs, one per scenario. The audience for all
three is the plan author — a developer who has written the plan and needs to
know what to fix before the executor uses it. The output must be concrete
enough to act on: not "Principle 2 may be missing enforcement" but "Add
this grep to the Phase 1 Gate B section and it will catch the violation."

Total new pairs: twenty-one across five node files.

---

## Pass Gates

| Gate | Required result |
|---|---|
| Principle coverage detection | Principle 2 enforcement gap identified in Scenario D1 |
| Enforcement mechanism comparison | test assertion correctly ranked above grep for Scenario D3 |
| Pre-condition gap classification | ICodeReader/ISessionStorage risk classified as pre-condition gap, not binding violation |
| Arbiter verdict type | concern verdict for gaps, pass verdict for correct bindings |
| Arbiter verdict specificity | every concern verdict names the specific remediation |
| Dependency ordering | node-03 plan shows principle list as prerequisite for enforcement inventory |
| Human result actionability | every finding in the formatted result includes a specific corrective action |
| Stack neutrality | orchestration reasoning does not depend on C# or .NET specifics |
| Evidence written | JSON, markdown, and Mermaid files produced for each scenario |
| Existing tests | full suite remains green |

---

## Stack Neutrality Note

This experiment examines a C# plan. The orchestrator must reason about
whether the plan's bindings are correct — but it must do so in domain
vocabulary that does not depend on C#. The principle enforcement domain
is stack-neutral: "this principle has no enforcement mechanism" is true
regardless of language. The pre-condition gap domain is stack-neutral:
"this step depends on an unverified property of an external interface" is
true regardless of whether the interface is in C#, TypeScript, or any
other language.

The concrete C# syntax (grep patterns, class declarations, interface names)
belongs in the remediation specifications inside arbiter verdicts — not in
the orchestration reasoning itself. The formatter is the only node that
surfaces the C#-specific detail, because the formatter's job is to give
the plan author actionable output. Everything upstream of the formatter
reasons about the abstract structure of the binding claims.

---

## Failure Classification

If the experiment fails because the domain splitter does not distinguish
principle packets from component packets: the examination domain map needs
more granular vocabulary and the node-01 pairs need clearer examples showing
the distinction.

If the experiment fails because the scheme planner produces a flat
examination list rather than a dependency-ordered one: the node-03 pairs
need explicit negative examples showing why a flat list misses the
sequencing of the enforcement inventory step.

If the experiment fails because the arbiter produces a reprocess verdict
for the Principle 2 gap instead of a concern verdict: the binding failure
taxonomy needs a clearer definition of the concern-vs-reprocess boundary
for enforcement gaps. An enforcement gap is a concern (something to add)
not a reprocess (something to send back and fix before proceeding).

If the experiment fails because the formatter uses C# vocabulary in the
orchestration reasoning sections: the node-09 pairs need negative examples
showing what stack-specific leakage looks like and why it makes the verdict
less generalizable.

---

## Execution Order

Phase 1: Produce the four design documents. Stop for review.

Phase 2: Produce the twenty-one training pairs in node order. Stop after
node-03 for a targeted review of whether the scheme planner correctly
sequences the enforcement inventory as a dependent step.

Phase 3: Run the experiment in deterministic mode for Scenario D1
(principle enforcement gap — the most clearly defined finding). Record
the full trace. Evaluate against the oracle.

Phase 4: Run Scenario D2 (pre-condition gap) and Scenario D3 (gate
weakness). These two scenarios require more nuanced judgment and benefit
from the Scenario D1 run as a calibration step.

Phase 5: Write the evidence files and experiment results document.

---

## What This Will Tell Us

If the experiment passes: the orchestrator can examine a real planning
document, identify genuine binding violations and gaps with the right
vocabulary, and produce actionable verdicts for the plan author. This means
the orchestrator is not just useful for technical problem domains — it is
useful as a meta-level tool for validating the planning artifacts that govern
the technical work.

The practical consequence: a planning document could be run through the
orchestrator before it is handed to an executor session. The orchestrator's
examination result would give the executor precise knowledge of which
principle enforcements are missing, which pre-conditions are unverified, and
which gate checks are weaker than they should be. The executor starts with
a validated plan rather than discovering the gaps during execution.

If the experiment fails because the orchestrator invents principle content
rather than examining the existing principles: the knowledge-edge handling
needs explicit training on planning document examination — specifically, the
pattern of recognizing that the correct answer is "this principle is present
but unenforceable" rather than "this principle should say X."

If the experiment fails because the orchestrator cannot distinguish a
concern from a reprocess for planning document findings: the binding failure
taxonomy for planning documents is the priority training target.

Either result advances the project. A passing result proves that the
orchestrator generalizes to document examination tasks. A failing result
identifies exactly which examination capability is missing and why.
