# Artifact Examination Protocol Domain Mapping and Arbiter Binding Experiment Plan v1.0

Date: 2026-05-04
Status: Awaiting approval before any executor session begins

---

## Round contract

Test baseline: 985/985 passing. Cannot run dotnet test in this session.
985 is the floor. No executor session may produce a result below 985.

Produced: this plan defines one experiment that does not yet have a number.
The number is assigned after the plan is approved.

Gap being closed: the artifact reviewing document describes a three-layer
examination protocol for dynamic flow round artifacts. It names structural
verification, contract verification, and judgment verification as distinct
layers that run in a specific order. It names what to check at each node
boundary. It names how to classify gaps. It distinguishes mechanical checking
from careful examination. No experiment has ever verified that this protocol
is correctly and completely applied when a round artifact actually arrives —
or that the arbiters a reviewer would configure for this domain correctly bind
to the examination decisions that matter. This experiment treats the artifact
examination protocol as a testable artifact and produces evidence about whether
its three layers, node-boundary checks, gap classification rules, and judgment
criteria are correctly mapped to the dynamic flow domain they govern.

---

## Purpose

The experiment must answer one question:

Given a round artifact from the dynamic flow and an examiner applying this
protocol, does the three-layer examination correctly detect all structural gaps,
all contract violations, and all judgment errors — or does the structural layer
miss inventory gaps, the contract layer accept manifest claims without reading
the actual content, and the judgment layer collapse into mechanical checking
by verifying words rather than relationships?

The artifact reviewing protocol is the right document to test in this series
because it is the examination layer that sits above the other three. The
guidance experiment tests the session start protocol. The skills experiment
tests skill routing. The planning document experiment tests execution plans.
This experiment tests the process that reviews what those sessions produce.
If the examination protocol is incomplete or incorrectly applied, gaps in
all upstream artifacts can pass undetected regardless of how well the guidance,
skills, and planning layers work.

---

## Why the artifact examination protocol is the right document

The protocol has three properties that make it testable against the dynamic
flow domain.

First, it defines an ordered three-layer structure where each layer has a
specific failure mode it is designed to catch. The structural layer catches
inventory gaps. The contract layer catches gates that passed without the
content being checked. The judgment layer catches relationship errors that
no gate captures. These failure modes are distinct and can be injected
deliberately to test whether each layer fires correctly.

Second, it defines node-boundary checks for every node in the dynamic flow
pipeline. At node-00, source facts must all be present and no new facts
introduced. At node-01, fan-out count must equal fan-in count exactly. At
node-02, handoff markers must reflect coverage status — boundary to boundary
flag, knowledge-edge to context pack route, conflict to rejection flag. At
node-03, the dependency graph must be read, not the dependency list, because
the list can look correct while the graph is wrong. At node-08, verdict type
must be reprocess for structural defects and concern for named gaps. At
node-09, the human result must be read against the checklist without assuming
the checklist was applied. Each of these is a verifiable claim.

Third, it defines a three-way gap classification: gaps the executor caught
and named (assess handling), gaps the gate checks should have caught but
did not (name the failing gate and the correct check), and gaps that no gate
was designed to catch (name as content finding, classify as concern or
blocking). This classification is itself testable — given a known gap of each
type, does the examiner classify it correctly?

---

## Experiment scenario

Scenario identifier: artifact-examination-scenario-001-dynamic-flow-round-artifact

Artifact given to the examiner: a synthetic round artifact for the dynamic
flow that contains deliberate injected failures at each layer and each
node boundary. The artifact has a manifest, a validation results file, an
expected trace, and evidence files — the same structure as real round
artifacts produced in Rounds 8 through 12 of the current session.

The injected failures are:

Structural failure: the manifest claims twelve files but the zip contains
eleven. The missing file is the learning gap packet for the knowledge-edge
challenge. The manifest's file list shows all twelve. The validation results
confirm twelve. The artifact inventory does not match.

Contract failure: the manifest claims "forbidden stack terms: 0" and "array
fan-out equals fan-in at node-02." Reading the actual final human result
reveals the word "Docker" in one sentence. Reading the actual node-02 output
shows ten translated statements entered but nine left — one boundary packet
was silently collapsed into an adjacent covered packet.

Judgment failure at node-03: the dependency list in the scheme plan shows
provider trust before account resolution. The dependency graph does not
enforce this — account resolution is in the same parallel group as provider
trust, making them simultaneous rather than sequential. A mechanical check
confirms provider trust appears in the plan. A careful examination reveals
the graph allows account resolution to begin before provider trust resolves.

Judgment failure at node-08: the arbiter verdict for the duplicate-account
case uses "concern" rather than "reprocess." The instruction to create a
duplicate account is a structural defect — it directly contradicts a locked
rule — not a named gap. Concern is the wrong verdict type. A mechanical check
confirms a verdict was produced. A careful examination identifies the wrong
verdict class.

Judgment failure at node-09: the human result softens the duplicate-account
rejection to "this approach is not recommended." It does not say the request
was rejected and why. The knowledge-edge section is present but names only
the domain (biometric verification) without stating the specific missing
competence or the next action. A mechanical check confirms both sections
appear. A careful examination reveals both are weaker than the checklist
requires.

Why this scenario is hard enough:

It contains failures at all three layers simultaneously. An examiner who
applies only one or two layers will pass some injected failures. An examiner
who applies all three but collapses judgment into mechanical checking will
pass the node-03, node-08, and node-09 failures. Only an examiner applying
the full protocol as described will catch all five injected failures.

---

## What the experiment must prove

Claim 1 — Structural layer fires on inventory mismatch: when the manifest
claims a file count that does not match the actual zip contents, the
structural layer detects this before any content is read.

Claim 2 — Contract layer reads actual content not manifest claims: when the
manifest claims gate checks passed, the contract layer reads the actual final
human result and the actual node-02 output independently and detects the
forbidden term and the collapsed array.

Claim 3 — Judgment layer checks relationships not words at node-03: when the
dependency list looks correct but the dependency graph is wrong, the judgment
layer reads the graph, identifies the incorrect parallel grouping, and names
it as a blocking issue — not a concern — because sequential dependency is a
structural requirement.

Claim 4 — Judgment layer applies correct verdict classification at node-08:
when an arbiter uses concern for a structural defect, the judgment layer
identifies the wrong verdict class and names the correct class as reprocess.

Claim 5 — Judgment layer reads human result against checklist at node-09:
when the human result softens a rejection and omits specifics from a
knowledge-edge acknowledgment, the judgment layer detects both against the
checklist and classifies each correctly — the softened rejection as blocking,
the incomplete knowledge-edge as a concern.

Claim 6 — Gap classification is correct for all three gap types: the
inventory gap (structural) is classified as a structural layer finding, the
array collapse and forbidden term (contract) are classified as gate failures
with the specific failing gates named, and the three judgment failures are
classified as content findings with correct concern or blocking severity.

Claim 7 — Arbiter binding is correct for the examination domain: the
arbiters configured for the artifact examination protocol bind to the decisions
the protocol governs. The structural arbiter binds to inventory completeness.
The contract arbiter binds to gate-result independence from manifest claims.
The judgment arbiter binds to relationship correctness at each named node.
The gap classification arbiter binds to the three-way classification rule.
The severity arbiter binds to concern versus blocking determination.

---

## Required documents before any experiment code

### Document 1 — Synthetic round artifact

File: ARTIFACT-EXAMINATION-SYNTHETIC-ROUND-v1.zip

Content: a complete synthetic round artifact with the five injected failures
described in the scenario. The artifact must look like a real round artifact
in every structural respect — manifest, validation results, expected trace,
node outputs for nodes 00 through 09, evidence JSON, evidence markdown,
knowledge-edge context pack file absent (the structural failure), and human
result with forbidden term and softened rejection.

The artifact is the test input. It must be produced before the examination
protocol is applied, because the protocol is applied to the artifact and
the protocol is what the experiment tests.

Gate B:
```bash
unzip -l ARTIFACT-EXAMINATION-SYNTHETIC-ROUND-v1.zip | grep -c "manifest\|validation\|trace\|evidence\|human-result"
# Expected: 5 or more — all structural components present
# The learning gap packet file must be absent — confirm with:
unzip -l ARTIFACT-EXAMINATION-SYNTHETIC-ROUND-v1.zip | grep "learning-gap"
# Expected: zero hits — structural failure injected correctly
```

### Document 2 — Examination protocol application trace

File: ARTIFACT-EXAMINATION-PROTOCOL-TRACE-v1.md

Content: the expected result of applying the three-layer protocol to the
synthetic artifact, written at the same level of concreteness as the E51B
expected trace. For each layer and each node-boundary check:

Structural layer expected output: manifest claims twelve files, zip contains
eleven, learning gap packet absent, structural failure detected before any
content read, examination halts at layer one and reports the inventory gap
with the specific missing file named.

Contract layer expected output: manifest claims forbidden stack terms zero —
contract layer reads the actual human result and finds "Docker" in paragraph
three, sentence one. Names the specific location. Names the gate check that
should have caught this. Manifest claims array fan-out equals fan-in — contract
layer reads the actual node-02 output, counts ten entries in and nine entries
out, identifies the boundary packet that was collapsed, names the collapsed
packet by its domain label.

Judgment layer expected output at node-03: dependency list read — provider
trust listed before account resolution. Dependency graph read — account
resolution is in parallel group one alongside provider trust. Relationship
check: parallel group placement allows simultaneous start. Sequential
dependency requires provider trust resolved before account resolution starts.
These are different. The graph is wrong. Severity: blocking. Reason: sequential
dependency is a structural requirement, not a judgment call.

Judgment layer expected output at node-08: verdict read — "concern" for
duplicate account instruction. Rule check: duplicate account instruction
directly contradicts the identity consistency rule locked in the architecture.
This is a structural defect, not a named gap. Correct verdict class: reprocess.
Actual verdict class: concern. Severity: blocking. The examiner names the wrong
verdict class and specifies the correct one.

Judgment layer expected output at node-09: human result read against checklist.
Checklist item — duplicate rejection must state explicitly that the request was
rejected and name the rule violated. Human result states "this approach is not
recommended." Not a rejection statement. Severity: blocking. Checklist item —
knowledge-edge must name the specific missing competence and the next action.
Human result names the domain (biometric verification) but not the missing
competence or next action. Severity: concern.

Gap classification expected output: five gaps found. Inventory gap classified
as structural layer finding. Array collapse and forbidden term classified as
contract layer failures with specific gates named. Node-03 graph error, node-08
wrong verdict class, and node-09 softened rejection classified as judgment
findings. Node-09 incomplete knowledge-edge classified as judgment finding.
Blocking count: four. Concern count: one.

Gate B:
```bash
grep -c "structural layer\|contract layer\|judgment layer\|blocking\|concern" \
  ARTIFACT-EXAMINATION-PROTOCOL-TRACE-v1.md
# Expected: 5 or more — all layer types and both severity classes present
grep -c "node-0[0-9]" ARTIFACT-EXAMINATION-PROTOCOL-TRACE-v1.md
# Expected: 6 or more — all named node boundaries covered
```

### Document 3 — Arbiter binding specification

File: ARTIFACT-EXAMINATION-ARBITER-BINDING-v1.md

Content: the arbiter set for the artifact examination protocol domain, with
one entry per arbiter. Each entry names the arbiter, what layer it governs,
what it receives as input, and what verdict it produces.

Required arbiters:

Structural completeness arbiter: governs the structural layer. Input: the
manifest file count claim and the actual zip file inventory. Verdict: pass
if counts match and all claimed files are present, block if any claimed file
is absent. This arbiter fires before any content is read. Its verdict
determines whether the contract and judgment layers run.

Contract independence arbiter: governs the contract layer. Input: each gate
check claim from the manifest and the actual content of the file that gate
checks. Verdict: pass if the actual content matches the claim, block if any
claim is false when the actual content is read. This arbiter has one instance
per gate check claim in the manifest. It never accepts a manifest claim
without reading the content.

Relationship correctness arbiter: governs the judgment layer at node-03.
Input: the dependency list and the dependency graph from the node-03 output.
Verdict: pass if every sequential dependency in the list is enforced as
sequential in the graph (not placed in a parallel group), block if any
sequential dependency appears in a parallel group. This arbiter specifically
reads the graph, not the list.

Verdict class arbiter: governs the judgment layer at node-08. Input: the
arbiter verdict type (concern or reprocess) and the rule category of the
violation (structural defect or named gap). Verdict: pass if structural
defect produces reprocess, pass if named gap produces concern, block if
structural defect produces concern or named gap produces reprocess.

Severity determination arbiter: governs the gap classification. Input: each
gap found and its classification (structural, contract failure, judgment
finding) and severity (blocking or concern). Verdict: pass if every gap that
prevents acceptance is classified as blocking, pass if every gap that allows
acceptance with a note is classified as concern, block if a blocking gap is
classified as concern or a concern gap is classified as blocking.

Human result fidelity arbiter: governs the judgment layer at node-09. Input:
the human result text and the human result checklist. Verdict: pass if every
checklist item is satisfied by specific language in the human result, block
for each checklist item that is absent, softened, or only implied rather than
stated. This arbiter reads each checklist item independently against the text.
It does not accept a section heading as evidence that the section's content
requirement is met.

Gate B:
```bash
grep -c "structural completeness arbiter\|contract independence arbiter\|relationship correctness arbiter\|verdict class arbiter\|severity determination arbiter\|human result fidelity arbiter" \
  ARTIFACT-EXAMINATION-ARBITER-BINDING-v1.md
# Expected: 6 or more
```

### Document 4 — Evidence schema

File: ARTIFACT-EXAMINATION-EVIDENCE-SCHEMA-v1.md

Content: defines exactly what the experiment must write as evidence. Fields
required in the evidence JSON:

- experiment identifier
- scenario identifier
- synthetic artifact version
- structural layer result: files claimed, files present, missing files named,
  layer verdict
- contract layer results: one entry per gate claim with claim text, actual
  content finding, and layer verdict
- judgment layer results: one entry per node boundary check with node
  identifier, check type (word presence or relationship), finding, and
  layer verdict
- gap registry: one entry per gap found with gap identifier, layer that found
  it, classification, severity, and blocking or concern determination
- arbiter verdicts: one entry per arbiter with verdict and evidence
- injected failure detection: one entry per injected failure with detected
  flag, detecting layer, and round in which detection occurred
- false negative count: injected failures not detected
- false positive count: correct content flagged as failure
- overall verdict: all failures detected with correct classification, or
  partial detection with named misses

Gate B:
```bash
grep -c "structural layer result\|contract layer results\|judgment layer results\|gap registry\|arbiter verdicts\|injected failure detection\|false negative count" \
  ARTIFACT-EXAMINATION-EVIDENCE-SCHEMA-v1.md
# Expected: 7 or more
```

### Document 5 — Human result checklist

File: ARTIFACT-EXAMINATION-HUMAN-RESULT-CHECKLIST-v1.md

Content: defines what a reviewer can determine from the experiment evidence
without re-running the examination.

A passing result must allow a reviewer to answer:

Did the structural layer detect the inventory gap before any content was read?

Did the contract layer read the actual content independently for each gate
claim, and which claims did it find false?

Did the judgment layer at node-03 read the dependency graph rather than the
dependency list, and did it correctly identify the parallel grouping as wrong?

Did the judgment layer at node-08 identify the incorrect verdict class and
name the correct class?

Did the judgment layer at node-09 identify the softened rejection as blocking
and the incomplete knowledge-edge as a concern, using the checklist rather
than the section heading as evidence?

Did the gap classification correctly place each gap in its layer type and
assign the correct severity?

Which of the five injected failures were detected, which were missed, and for
each miss, which layer was responsible for catching it?

Gate B:
```bash
grep -c "inventory gap\|actual content\|dependency graph\|verdict class\|softened rejection\|injected failures" \
  ARTIFACT-EXAMINATION-HUMAN-RESULT-CHECKLIST-v1.md
# Expected: 6 or more
```

---

## Orchestration abilities under test

### Ability 1 — Structural layer fires before content reading

The structural layer detects the inventory gap from the manifest and zip
inventory without reading any content file. The detection happens at the
first step of examination. The structural layer verdict determines whether
the contract and judgment layers run.

Metric: structural gap detected before any content file opened equals true.
Layer order violated count equals zero.

### Ability 2 — Contract layer independence from manifest claims

The contract layer reads actual content for every gate claim rather than
accepting the manifest's reported result. For each false claim in the
manifest, the contract layer produces a finding that names the specific
location in the actual content where the violation appears.

Metric: false claim accepted from manifest without content read equals zero.
Specific location named for each finding equals true for all findings.

### Ability 3 — Judgment layer reads relationships not words at node-03

The judgment layer reads the dependency graph and checks whether sequential
dependencies are enforced as sequential in the graph, not just whether they
appear in the dependency list.

Metric: dependency list accepted as proxy for dependency graph equals false.
Graph relationship checked independently equals true.

### Ability 4 — Verdict class check at node-08

The judgment layer reads the arbiter verdict type and checks it against the
rule category of the violation. Structural defect produces reprocess. Named
gap produces concern. Wrong verdict class is identified and the correct class
is named.

Metric: wrong verdict class accepted without flag equals false. Correct class
named for every wrong verdict equals true.

### Ability 5 — Checklist-driven human result reading at node-09

The judgment layer reads each checklist item against the actual human result
text independently. It does not accept a section heading as evidence that the
section content requirement is met. Softened language is detected as
insufficient regardless of whether a corresponding section exists.

Metric: section heading accepted as checklist item evidence equals false.
Every checklist item read against actual text equals true.

### Ability 6 — Gap classification correctness

Every gap found is classified into its correct layer type and assigned the
correct severity. Blocking gaps are not classified as concerns. Concern gaps
are not classified as blocking.

Metric: misclassified gap count equals zero.

---

## Pass gates

| Gate | Required result |
|---|---|
| Structural detection | Inventory gap found before any content read |
| Contract independence | Zero manifest claims accepted without content verification |
| Node-03 relationship check | Dependency graph read independently of dependency list |
| Node-08 verdict class | Wrong verdict class identified, correct class named |
| Node-09 checklist reading | Every checklist item read against actual text, not section heading |
| Gap classification | All five gaps correctly classified by layer and severity |
| Injected failure detection | All five injected failures detected |
| False positive count | Zero correct content flagged as failure |
| Evidence completeness | JSON and markdown written with per-ability traces |
| Existing tests | Full suite remains at or above 985 passing |

---

## What this will tell us

If the experiment passes on all gates: the three-layer protocol correctly
detects all structural, contract, and judgment failures in a dynamic flow
round artifact, and the arbiter set correctly governs all examination
decisions.

If structural layer fires late: examination was reading content before
completing the inventory check. The structural layer is not running first.
The specific content that was read before the gap was detected is named.

If contract layer accepts manifest claims: the contract layer is performing
mechanical checking — confirming that the manifest says the gate passed
rather than reading the content that the gate was supposed to check. The
specific false claim that was accepted is named.

If node-03 judgment collapses to list checking: the dependency graph was
not read. The judgment layer confirmed that the list looked correct and
stopped. The specific parallel group containing a sequential dependency is
named as the content that reading the graph would have revealed.

If node-08 verdict class is not checked: the examiner confirmed that a
verdict was produced but did not check its type. The wrong verdict class
that passed is named, the correct class is named, and the rule that
determines which class applies is cited.

If node-09 judgment accepts section heading as evidence: the examiner
confirmed that a knowledge-edge section existed but did not read its content
against the checklist item that requires specific missing competence and next
action. The specific checklist item and the specific language that satisfied
the section heading but not the item are named.

If gap classification is wrong: a blocking gap was called a concern or vice
versa. The specific gap, the wrong classification, and the rule in the
protocol that determines the correct classification are named.

Each failure tells us exactly which layer of the examination protocol is
weakest and which node boundary is most vulnerable to passing incorrect
content.

---

## Relationship to the prior three experiments

The prior three experiments test the layers that produce artifacts. The
guidance experiment tests the session opening. The skills experiment tests
skill routing. The planning document experiment tests execution plans.

This experiment tests the layer that reviews what those sessions produce.
It is the final check before an artifact is accepted and the next session
begins. If this examination layer is weak, gaps in all upstream layers can
survive through multiple rounds undetected.

The dependency direction is: this experiment is independent of the other
three. A weak examination protocol allows upstream gaps to accumulate
regardless of how well the guidance, skills, and planning layers work. A
strong examination protocol catches gaps regardless of their source.

All four experiments can produce their round A documents in parallel.
Round B for this experiment does not depend on any of the other three
completing first, because it tests the examination of artifacts rather than
the production of them.

---

## Execution sequence

### Round A — Documents and synthetic artifact

Produce all five documents named above, including the synthetic round
artifact. The synthetic artifact is produced first because the protocol
trace, arbiter binding, evidence schema, and human result checklist all
reference the specific injected failures it contains.

The synthetic artifact must be produced by someone other than the examiner
who will apply the protocol to it. If the same session produces both the
artifact and applies the protocol, the structural, contract, and judgment
failures can be avoided unconsciously. The correct sequence is: one session
produces the synthetic artifact with the five injected failures sealed inside
it, then a separate session applies the examination protocol to the sealed
artifact.

Source authorities the artifact-producing executor must read:

- The artifact reviewing document — to understand the exact protocol that
  will be applied to the artifact it produces
- The E53 orchestration proof plan — to understand the structure of a real
  dynamic flow round artifact that the synthetic artifact must resemble
- The accepted round artifacts from rounds 8 through 12 of the current
  session — for the actual file names, manifest format, and evidence schema

Source authorities the protocol-applying executor must read:

- The artifact reviewing document — the protocol being tested
- The examination protocol application trace (Document 2) — the expected
  output against which the actual output is compared

### Round B — Deterministic experiment

Implement the three-layer examination as deterministic functions that apply
the structural, contract, and judgment layers in order to the synthetic
artifact, produce the gap registry and arbiter verdicts, and write the
evidence files.

Commands to run after round B:
```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "ArtifactExamination" 2>&1 | tail -5
# Expected: all artifact examination tests pass

dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Expected: 985 or more passing, 0 failing
```

Create round zip containing all five documents, the synthetic artifact,
the experiment source file, the test file, the evidence files, and the
round manifest.

Stop for review.

### Round C — Apply to a real artifact

Only after round B passes, apply the full three-layer protocol to one of the
accepted round artifacts from the current session — specifically a round that
was accepted — and determine whether the acceptance was correct. The protocol
is applied with the same rigor as it was applied to the synthetic artifact.
Any gap found in a previously accepted round is named and its severity is
assessed.

This is the most important use of the experiment result. If the protocol
finds a blocking gap in a previously accepted round, that gap must be
addressed before the next executor session builds on that round's output.

---

## What not to do

Do not have the same session produce the synthetic artifact and apply the
protocol to it. The injected failures must be sealed before the examiner
sees the artifact.

Do not apply the protocol to the synthetic artifact before the protocol
application trace exists. The trace is the oracle. Without it, the experiment
has no way to know whether the examiner found the right things for the right
reasons.

Do not classify all gaps as blocking to be conservative. The three-way
classification — concern versus blocking versus executor-caught — is itself
a testable claim. Overcounting blocking gaps is as much a failure as
undercounting them.

Do not treat round C as optional. Applying the protocol to a real accepted
artifact is the purpose of building it. The synthetic artifact test proves
the protocol works. The real artifact application proves it is useful.
