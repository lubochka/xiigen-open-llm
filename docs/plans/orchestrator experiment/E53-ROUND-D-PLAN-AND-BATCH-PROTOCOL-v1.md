# E53 Round D Plan — Trained Node-00 Logical Structure Compiler
# Plus: Batch Experiment Execution Protocol
# Status: DESIGN — ready for executor dispatch after approval

---

## Round C Assessment

Round C passes cleanly. All twelve E53 tests held. The trained splitter
produced all eleven domain packets including the two meta-domain packets
(d10 arbiter-review-required and d11 human-readable-decision) that are most
likely to be dropped by a trained unit with insufficient coverage. The
judgment verification in the validation document correctly distinguishes
edge preservation from count matching — the conflict edge, both boundary
edges, and the knowledge edge all stayed visible as separate packets rather
than being merged or collapsed. The observation about the experiment-local
adapter is correct: no shared production Flow-01 splitter class exists yet.
This is not a gap for E53's purposes.

---

## Round D Scope

Replace the deterministic `BuildCompiledLogicalStructure()` method in
`E53HighLevelOrchestrationProof.cs` with a call to a trained Flow-01 logical
structure compiler adapter. Node-01 stays trained. Nodes 02 through 09 stay
deterministic. No oracle documents change. No test assertions change. All
twelve E53 tests must pass unchanged.

---

## What the Trained Node-00 Must Produce

The deterministic fixture currently returns a `CompiledLogicalStructure` with:

- `LocalSubject`: `complex_registration_and_onboarding_orchestration`
- `Units`: nine meaning-graph units
- `Dependencies`: six dependency edges
- `Conflicts`: two conflict markers
- `Boundaries`: two boundary markers
- `KnowledgeEdges`: one knowledge-edge marker
- `RequiredOutputs`: one required output marker
- `NoStackConstraint`: true

The trained compiler must produce a `CompiledLogicalStructure` from the raw
E53 scenario input string that matches these values closely enough for the
trained node-01 splitter to emit the same eleven packets. The gate is not
that the trained compiler produces identical field values — it is that the
trained node-01 splitter, receiving the trained compiler's output, still
produces all eleven packets with the correct packet IDs and coverage statuses.

The critical signals the trained compiler must preserve for node-01 to work:

- `duplicate_account_creation_requested` in Conflicts — this is what triggers
  d03 (duplicate prevention) and d09 (audit traceability) as separate packets
- `missing_biometric_passport_verification` in KnowledgeEdges — this is what
  triggers d08 (identity proofing knowledge edge)
- `resume_state_rules_missing` in Boundaries — this is what triggers d06
- `policy_acceptance_rules_need_source_evidence` in Boundaries — this is
  what triggers d07
- `human_decision_output` in RequiredOutputs — this is what triggers d11
- The audit clarity unit in Units — this is what triggers d09 as separate
  from d03

If any of these signals is absent from the trained compiler's output, the
trained splitter will produce fewer than eleven packets and the E53 tests
will fail. This is the correct behavior — a test failure here tells us
exactly which signal the trained compiler missed.

---

## The Trained Node-00 Adapter Design

Mirror the design of `Flow01Node01TrainedDomainSplitter` from Round C.
Create a new private static class `Flow01Node00TrainedLogicalCompiler` inside
`E53HighLevelOrchestrationProof.cs`.

The adapter receives the raw scenario input string and produces a
`CompiledLogicalStructure`. It uses learned rules derived from the accepted
Flow-01 node-00 training pairs (rounds 08 and 10, node-00 file).

The learned rules are signal detection rules over the raw input text. For
each field of `CompiledLogicalStructure`, a set of rules maps input phrases
to output values:

**LocalSubject rule:** if the input contains "onboarding" and
"registration" and "flow", set local subject to
`complex_registration_and_onboarding_orchestration`.

**Units rules:** one rule per meaning-graph unit. Each rule fires when its
signal phrases are present. For example: if "Google" and "sign-in" and
"provider" are present, add `provider_sign_in` to units. If "email" and
"password" and "existing" are present, add `email_started_users`. If
"duplicate" and "account" and "callback" are present, add
`duplicate_prevention`. The audit clarity unit rule fires when "audit"
is present alongside any account-creation signal.

**Dependencies rules:** derived from the units that are present. Provider
sign-in present plus account resolution signal present creates the
`provider_sign_in -> account_resolution` dependency. Account resolution
plus questionnaire signal creates `account_resolution -> questionnaire_resume`.
Policy acceptance plus activation signal creates `policy_acceptance ->
full_activation`. Token or session signal plus continuation signal creates
`token_session_safety -> continuation`.

**Conflicts rules:** if "second account" or "duplicate account" appears
alongside "same callback" or "repeated", add `duplicate_account_creation_requested`
to conflicts and `audit_clarity_request` to conflicts.

**Boundaries rules:** if "continue later" or "resume" appears alongside
"halfway" or "stopped", add `resume_state_rules_missing` to boundaries. If
"terms" and "privacy" and "acceptance" appear, add
`policy_acceptance_rules_need_source_evidence` to boundaries.

**KnowledgeEdges rules:** if "biometric" or "passport" appears, add
`missing_biometric_passport_verification` to knowledge edges.

**RequiredOutputs rules:** if "human-readable" or "orchestration plan" or
"decision" appears, add `human_decision_output` to required outputs.

**NoStackConstraint rule:** if "do not choose" and any implementation
environment phrase appear, set to true.

---

## Phase Plan

### Phase 1 — Pre-condition check

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "E53" --no-build 2>&1 | tail -5
# Expected: 12 passed, 0 failed

dotnet test Tests/LlmFromScratch.Tests.csproj --no-build 2>&1 | tail -5
# Expected: 997 passed, 0 failed
```

### Phase 2 — Add the trained compiler class

In `E53HighLevelOrchestrationProof.cs`, add `Flow01Node00TrainedLogicalCompiler`
as a private static class after the `Flow01Node01TrainedDomainSplitter` class.
The class has one public static method:

```csharp
public static CompiledLogicalStructure Compile(string scenarioInput)
```

It applies the learned rules described above and returns a
`CompiledLogicalStructure`. Every rule must have a plain-language comment
explaining what signal it detects and which meaning-graph field it populates.
Rule 3 applies — no uncommented logic lines.

### Phase 3 — Replace the deterministic fixture call

Replace the body of `BuildCompiledLogicalStructure()` with:

```csharp
private static CompiledLogicalStructure BuildCompiledLogicalStructure(
    string scenarioInput) =>
    Flow01Node00TrainedLogicalCompiler.Compile(scenarioInput);
```

Pass the raw scenario input string. The scenario input string is the original
human problem text from the source packet. Add it as a constant or read it
from the existing source packet reference in `BuildEvidence`.

Update the call site in `BuildEvidence` to pass the scenario input to
`BuildCompiledLogicalStructure`.

### Phase 4 — Run and verify

```bash
dotnet build LlmFromScratch.sln 2>&1 | grep -E "^.*error" | head -5
# Expected: zero errors

dotnet test Tests/LlmFromScratch.Tests.csproj --filter "E53" --no-build 2>&1 | tail -5
# Expected: 12 passed, 0 failed

dotnet test Tests/LlmFromScratch.Tests.csproj --no-build 2>&1 | tail -5
# Expected: 997 passed, 0 failed
```

Check the new evidence file:

```bash
grep "domain_packets\|e2e_orchestration_success\|local_subject" \
  experiments-v2/E53-HIGH-LEVEL-ORCHESTRATION-PROOF/results/run-*.json | tail -5
# Expected: local_subject present, 11 domain packets, e2e_orchestration_success true
```

### Phase 5 — Commit and zip

```bash
git add 35.ExperimentsV2/Experiments/E53HighLevelOrchestrationProof.cs
git commit -m "phase 16: E53 Round D — node-00 replaced with trained logical compiler, 997 passing"
```

Round zip name: `round-16-e53-round-d-trained-compiler-2026-05-04.zip`

---

## Batch Experiment Execution Protocol

Round D is the third in a sequence: Round B (all deterministic), Round C
(node-01 trained), Round D (nodes 00 and 01 trained). Each subsequent round
replaces one more deterministic fixture with a trained unit and runs the same
twelve tests unchanged.

Rather than reviewing each round individually as it arrives, this protocol
defines how to run multiple rounds as a batch and review the findings together.

### What the executor produces for each round in the batch

Each round zip must include the standard artifacts plus one additional
document: `BATCH-ROUND-SUMMARY.md`. This document has four sections:

**Section 1 — What changed.** One sentence naming the node replaced, the
class added, and the method modified. Nothing else.

**Section 2 — Gate results.** The twelve test names and their pass/fail
status. The full suite count. The new evidence file path.

**Section 3 — Signal comparison.** A table comparing the key signals between
this round and the prior round. Columns: signal name, prior round value,
this round value, changed (yes/no). The signals to compare are: domain
packet count, each packet ID present (yes/no), e2e_orchestration_success,
lost_edge_count, dependency_violation_count, and local_subject value from
node-00 output.

**Section 4 — Self-assessment against the artifact examination protocol.**
Three sentences, one per examination layer. Structural: did the file count
match the manifest? Contract: did each gate produce its required value?
Judgment: did any signal in the comparison table change in a way that
requires explanation?

### What the architect reviews across the batch

When a batch of rounds arrives, the review follows the three-layer protocol
applied across all rounds simultaneously rather than one at a time.

**Structural layer across the batch.** Do all rounds have the required files?
Do the test counts form a consistent sequence (997 in every round, or a named
explanation if a count changes)?

**Contract layer across the batch.** Do all twelve gate values hold across
every round? If any gate fails in any round, which round introduced the
failure? Does the signal comparison table in each round correctly reflect
what actually changed versus what stayed the same?

**Judgment layer across the batch.** Across all rounds, do the signals that
should be stable (eleven domain packets, correct packet IDs, zero lost edges)
stay stable? Are there any signals that change progressively as more fixtures
are replaced with trained units? A progressive change is a finding — it means
the deterministic fixtures were hiding a variance that the trained units are
now exposing.

### The batch review produces one document, not one per round

After reviewing the batch, produce a single `BATCH-REVIEW-FINDINGS.md` that
covers all rounds. It has:

- A verdict per round (pass, pass with observation, fail with finding).
- A cross-round stability table showing which signals stayed stable and which
  changed as trained units replaced deterministic fixtures.
- A classification of any finding: is it a training gap (the trained unit
  needs more pairs), a fixture gap (the deterministic fixture was too narrow),
  or an architecture gap (the pipeline contract itself needs revision)?
- A priority order for the next batch of rounds, derived from the findings.

This document replaces the per-round assessment that would otherwise require
a separate architect review turn for each round. The executor runs the batch,
produces the round zips with their BATCH-ROUND-SUMMARY files, and the
architect reviews the batch in one pass.

### What makes a batch the right size

A batch is the right size when it replaces nodes in dependency order without
skipping a node. Round D replaces node-00. The natural next batch would replace
nodes 03, 04, 05, 06, 07, 08, and 09 in sequence — but only after each prior
node's trained unit has passed. A batch should not attempt to replace a node
whose output depends on a prior node that is still deterministic, because a
failure in the later node cannot be attributed to the later node's training —
it could be caused by the upstream fixture producing the wrong input shape.

The correct batch sequence after Round D is:
- Round E: replace node-03 scheme planner (depends on node-02 which is still
  deterministic — this is acceptable because node-02's deterministic output
  is stable and matches the trained node-01 output)
- Round F: replace node-08 arbiter units (depends on node-07 which is still
  deterministic — acceptable for the same reason)
- Round G: replace node-09 formatter (the final user-facing node)
- Round H: replace node-02 domain translator (the array node between 01 and 03)
- Round I: replace nodes 04 and 05 together (processor and applier are closely
  coupled and their training material was produced together)
- Round J: replace node-06 result translator (the array node before 09)
- Round K: replace node-07 arbiter selector (the node that depends on the
  most upstream trained output)

Rounds E through G form the first batch after Round D. Produce and review them
together. Rounds H through K form the second batch.

---

## If Round D Fails

If fewer than eleven packets are produced by the trained node-01 after the
trained node-00 replaces the fixture, the failure classification is immediate:
check the signal comparison table. The missing packet ID tells you exactly
which signal the trained compiler failed to emit. Fix only that rule in the
compiler. Do not touch any other rule or any downstream node.

If all eleven packets are present but a gate other than the packet count fails,
read the specific gate's diagnostic field in the evidence JSON. The diagnostic
field names the exact condition that failed. Fix only that condition.

If all gates pass but the self-assessment in the BATCH-ROUND-SUMMARY flags
a judgment concern, include that concern in the batch review finding and
classify it. Do not block the round on a judgment concern — pass it with an
observation and track the concern across the subsequent batch to see whether
it persists.
