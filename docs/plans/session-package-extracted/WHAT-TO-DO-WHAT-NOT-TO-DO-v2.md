# What To Do and What Not To Do v2
## Derived from all session files â€” LlmFromScratch / Dynamic Flow project

This document is extracted from the last code session, the last product
session, and all eleven archived session files. Every rule below has a
direct source in observed session behavior: either something that worked
and should be repeated, or something that failed and caused correction.

## v2 Corrections

This version updates v1 after the projects 21-23 / E52 round and the
dynamic-flow document-adaptation round.

- D-15 is now locked, not pending.
- The approval-gate language now separates PermissionGate approval from
  state-machine single-use guard behavior.
- The plain-language rule now distinguishes natural-language training content
  from formal logical-structure outputs.
- The dynamic-flow architecture plan warning is scoped to gathered-document
  training-set tasks.
- The round-zip packaging rule is included as a required session habit.

---

## DOCUMENT EXAMINATION AND TRANSLATION TO LEARNING SETS

### What to do

**Read the whole source document before deriving anything from it.**
A line near the beginning sets context that a line near the end depends
on. Pairs written before reading the whole document miss that dependency.
Product session confirms: reading must precede any assignment of lines
to nodes.

**Take one document at a time, name it exactly before starting.**
Record full path, line count, byte size, first line, last line. Do not
begin with several related documents grouped under one label. The
document examination method requires this as a formal pre-step, and
the sessions showed that skipping it causes source contamination.

**Find all versions of the document before writing any training pairs.**
Search current folders, `old`, `Older`, `history`, `session_files`,
`.claude\worktrees`, and archive zips. Record path, size, line count,
and modification time for each version. Compare exact changed lines
between versions. If two files are duplicates, say so. If they differ,
preserve both.

**Find the creation session for the document.**
Search session files for the document name, its title, and distinctive
phrases. Classify findings as creation evidence, candidate history,
later use, or unrelated mention. If no creation session is found, say
`Exact creation transcript was not found.` Do not invent a source
session.

**Extract all UML nodes and edges before assigning any line to a node.**
Use the diagram as the node authority. The edge ledger is required â€”
training sets must teach transitions, not only node meanings. Each edge
record must answer four questions: what moves from the source node, what
the target node receives, what the target node must produce, and what
breaks if this edge is missing.

**Create one record per source line including blank lines and separators.**
Blank lines and separator lines teach boundaries. Without them, the
trained flow may merge sections that the document kept apart. A row for
a blank line is not waste â€” it preserves the document's intended rhythm.

**Write B as the actual output, not a description of the output.**
The sessions identified this as the primary failure mode across all
previous training sets. If B says "a clear work statement in the
language of that domain," the node learns to produce that description,
not the work statement. B must be the text the node should actually
produce, written out in full.

**Write A as the actual runtime input the node receives.**
A must be what a real user would type or what the previous node would
actually pass forward. A source document line is never A â€” no node
receives document lines at runtime. A is a user prompt, or a domain
fragment, or a translated description, or a processed result â€” whichever
actually arrives at that node during a live run.

**Apply the rephrasing rule to every pair.**
Every training pair must appear at least twice â€” once in its original
phrasing and once rephrased. The same situation from a different angle,
at the same depth. Rephrasing is not summarising. A shorter rephrasing
has lost content.

**Keep array nodes as arrays.**
Domain translator, domain processor, applier, and result translator are
each an array. Each pair in these node files covers one unit receiving
one element. Do not write one large pair where the unit receives all
elements together.

**Keep the arbiter selector separate from the result translator.**
Both complete before the formatter runs. They are separate node files
with separate training pairs. The sessions showed that collapsing them
into one produces a unit that conflates translation and quality review.

**Simulate the trained flow after writing the sets.**
After the node files are complete, simulate the full flow as if the
nodes were trained only on those sets. The simulation must answer what
would be missing. A missing-parts section is required and must be honest.

**Assign every source line to all nodes it touches, not only the primary one.**
A line about "every unit learns" belongs primarily to the learning node
but also touches the registry, arbiter behavior, and flow modification.
All links must appear in the batch row. This is the main anti-edge-cut
rule from the last code session correction.

**Create parallel learning streams for each source line.**
The same source line feeds multiple streams with different jobs:
architecture stream (what system shape should exist), session-memory
stream (why this line exists and what correction produced it), UML
stream (which visible node or arrow receives it), learning stream (what
future model behavior this line teaches), arbiter stream (what would
be wrong if the line were misclassified), version stream (whether this
line is original, derived, corrected, open, or missing).

**For Node 8 (arbiter units), include at least one pass case.**
The sessions showed that a previous training set had zero pass cases
in the arbiter units file. Every arbiter-unit pair that flags something
must be balanced by pairs that demonstrate correct passing behavior.

**Stop after one document and review before choosing the next.**
One document examined carefully and completely is worth more than ten
examined superficially. The plan confirms: produce the sets, review
them, run the simulation, then choose the next document.

---

### What not to do

**Do not start with a plan to scan "relevant" skill files and narrow intake
immediately.**
The last code session was corrected for exactly this: declaring "the
file landscape confirms there is already a dynamic-flow document family"
and then choosing only "relevant" skills. That is edge cutting. The
full examination must precede any narrowing.

**Do not treat the dynamic-flow architecture plan as the source document
to translate when the task is to translate gathered business or system
documents into learning sets.**
The last code session made this mistake twice. The dynamic-flow plan is
the TARGET FLOW â€” the UML that training sets feed into. The source
documents are the gathered business or architecture documents under the
`xiigen` folder. These are different things. In a separate architecture
maintenance task, the dynamic-flow architecture plan may itself be the
source document under examination, but that must be named explicitly.

**Do not produce a plan document and call it the batch.**
The last code session produced `DYNAMIC-FLOW-FIRST-FILE-LEARNING-SET-PLAN-v1.md`
and stopped there. A plan is not the actual 234-row batch. The plan
must be followed by actual batch files. Stopping at the plan stage is
incomplete work.

**Do not let B describe the output instead of showing it.**
Across all sessions, this was the most frequently observed failure mode.
Examples of wrong B entries: "a clear work statement in the language of
that domain," "a planned piece with its place in the provider path,"
"a section, sentence, or boundary in the final answer." None of these
are outputs â€” they are descriptions of outputs. The node learns to
produce the description, not the result.

**Do not merge document lines because an idea was already covered.**
Every source line gets its own row. Repeated ideas stay repeated. The
fact that a concept appeared earlier in the document does not make its
later appearance redundant for training purposes.

**Do not collapse parallel array stages into one unit.**
Domain processor, applier, result translator â€” these are arrays. Writing
one large pair that handles all elements together breaks the architectural
contract. The sessions showed this causes the trained units to not
generalise across different domain pieces.

**Do not add a "relevant" filter before reading the full document.**
The first correction in the last code session was about this. Reading
only "relevant" parts is a form of edge cutting that produces incomplete
coverage. Read the whole document first, then assign.

**Do not use technical or code terms in natural-language training pair
content unless the node's actual runtime output is formal structure.**
User-facing A/B content should be plain human language. Technical identifiers
are allowed for canonical file names, flow names, predicates, arbiter names,
feature-vector fields, and logical-structure outputs when those are the actual
runtime content the node must produce. The language around the formal content
must still be clear to a human who will examine the flow.

**Do not produce plans and batches in the same turn.**
Produce the plan, stop, get approval, then produce the batch. The
sessions showed that combining them in one turn makes the plan
unreviewed and errors propagate into the batch.

---

## EXPERIMENT DESIGN AND EVIDENCE

### What to do

**Write tests before writing training examples.**
The wrong-domain retrieval guard test was written before any new
training examples were added in E51A Phase 2. This order prevents the
new data from accidentally satisfying the test through coincidence
rather than through mechanism.

**Make every failing case a named failure, not a metric summary.**
When E51A had three thin cases, each was named: V05-predicate_overlap_below_gate,
V06-domain_mismatch, V08-predicate_overlap_below_gate. A metric
percentage alone does not tell you which records failed and why.

**Assert the exact set of failures, not just their presence.**
The failure set should be an exact match, not a contains check. If the
expected failures are V05, V06, V08, the test should assert
`new[] { "V05...", "V06...", "V08..." }` exactly â€” ordered. This catches
the case where a new failure appears while a previous one is fixed.

**Add a positive assertion for every fixed failure.**
When V06 was corrected, the fix created `E51A_CompoundDiagnosticRepairPrompt_RoutesToBugFix`
as a permanent guard. The thin case is now documented as expected
behavior, not just "no longer failing."

**Verify code contexts produce the expected predicates before adding
them to training sets.**
For E51A Phase 2, the team checked Level3Tokens on each proposed
context before committing. A code context that is too minimal or
assignment-heavy will produce thin Level 3 output regardless of
the domain it is supposed to represent.

**Use domain-scoped KNN: scope the candidate pool before ranking.**
E51A Phase 1 showed that scoping KNN to the predicted domain pool
before similarity ranking gives cleaner separation. V07's architecture
case went from a single-point attractor to a genuine two-neighbor
cluster once the pool was constrained.

**Add the wrong-domain retrieval guard before expanding training sets.**
If domain classification misroutes and the pool is wrong, KNN will
search the wrong pool. The guard must assert that all neighbor scores
are below 0.50 in the wrong domain pool. This guard was the first
thing added in Phase 2, before any new examples.

**Promote only after coverage verification, not after metric passage.**
D-13 is the locked rule: tier promotion requires coverage verification.
E51A Tier 1 passed its metric gates (overlap 0.867, domain accuracy 90%)
while still exposing V07's single-point attractor, V03/V04/V09's shared
loop signature, and V10's wrong context. Metrics passing is necessary
but not sufficient.

**Verify coverage checks are concrete and JSON-auditable.**
The coverage checks for D-13 must produce a pass/fail from the evidence
JSON without manual interpretation. V07 check: count of neighbors
with score >= 0.60 must be >= 2. V03/V04/V09 check: the top-3 neighbor
ID sequences for all three must not be identical. V10 check:
predicate_overlap == 1.0 and produced predicates contain RETURN or
DECLARE.

**State what failed and why in the evidence, not just the count.**
Each failed test example must produce one sentence explaining which
stage failed and why. "Domain misrouted," "KNN too distant," "forbidden
predicate appeared," "fidelity gate not reached" â€” these are the kinds
of explanations that make the evidence actionable.

**Check teaching/runtime symmetry explicitly.**
E51B added an assertion that TR-01 and TE-01 compile through the same
feature path, produce the same vector width (60), use the same
22-predicate vocabulary, and their predicate profiles overlap above 0.50.
Without this, teaching and runtime can diverge subtly without any metric
catching it.

**Link evidence files across experiment tracks.**
E51B's evidence JSON includes a `compiler_validation_reference` field
pointing to E51A Tier 2. This makes the relationship between tracks
explicit and auditable. Without it, two passing experiments provide no
evidence that they are using the same underlying mechanism.

**Assert neighbor scores for knowledge-edge cases, not only their presence.**
CH-01's context pack existence is not enough. The test must assert that
nearest neighbors were found (present) but all scored below 0.50 (not
confident). Low-confidence retrieval and zero-neighbor retrieval are
different failure modes. The designed behavior is low-confidence with
named reason.

**Assert arbiter rejection overrides high KNN similarity.**
CH-02 must assert two things in one test: nearest-neighbor score above
0.70 (KNN found a good match), and arbiter verdict is rejected anyway.
A test that only checks the rejection without verifying the similarity
was high does not prove the architectural rule.

---

### What not to do

**Do not declare a dataset "ready" when metrics pass but neighborhoods
are thin.**
E51A Tier 1 passed all metric gates with V07 having only one strong
architecture neighbor. The system routed correctly only because T06 was
the sole ARCHITECTURE training example and it dominated. At Tier 2 with
more examples, this would have collapsed. The sessions caught this only
because of the explicit coverage check.

**Do not add new training examples before fixing existing thin contexts.**
The sequencing is: fix existing contexts, verify fixed evidence still
passes 100%, then expand. Adding new examples before the existing ones
are clean risks masking V10 and V03/V04/V09 problems behind new data.

**Do not use raw keyword scoring as the only routing signal.**
The last product session identified that compound prompts like "why and
how to fix it" score both CODE_QUESTION (why, how) and BUG (fix, crash)
keywords. Without the compound-diagnostic rule, these route incorrectly.
Pure keyword counting without predicate profile input was the weakness
E51A exposed and the gateway classifier inherited.

**Do not treat tests as structural if they should be behavioral.**
The session identified four structural tests: two "AllExamples produce
NonEmpty predicates" tests (structural), two "WriteEvidence creates
Json file" tests (structural). These do not catch routing regressions
or arbiter failures. Behavioral tests are required alongside structural
ones.

**Do not let a high KNN similarity score implicitly approve an arbiter
result.**
The arbiter-KNN authority rule is locked: KNN uses arbiter history as
memory, arbiters are the hard gate. No KNN confidence, priority, budget
pressure, or style preference can override an arbiter rejection. This
rule must be explicitly enforced in tests, not assumed from architecture
documentation.

---

## INTERNAL-FIRST AI DESIGN (PROJECTS 21-23)

### What to do

**Attempt the internal model first for every request.**
`ExternalCallCount` on `InternalLlmGateway` must always return 0.
The internal gateway is not a fallback â€” it is the primary and only
path. External AI is an escape valve, not a default.

**Gate external AI calls through explicit single-use user approval.**
`call_external_ai` belongs in the `RequiresApproval` set in
`PermissionGate`, not in `Allowed` or `Denied`. Approval is
single-use â€” it does not persist across loop iterations. The sessions
confirmed that `ExternalAiStateGuards.ShouldRequestExternalAiApproval`
must return false when `ApprovalRequestedKey` is already set to true,
even if confidence remains low.

**Name the missing competence, not just report low confidence.**
The external AI approval package must carry a named missing-competence
statement ("the internal model is missing coverage for X") rather than
a generic low-confidence signal. `MissingCompetence` being empty must
prevent the approval gate from firing, even with low confidence.
The sessions showed this as a hard rule: low confidence alone is not
enough.

**Store all examined results as training signals.**
Results above the confidence threshold go to `FeedbackRouter` as
Accepted. Results below go as Edited with the examiner's corrected
version as UserCorrection. External AI results go through
`ExternalAiResultAdapter` and produce a DPO pair. Nothing is discarded.

**Run learning events at idle time only, gated by IdleDetector.**
`LearningEventScheduler.DrainIfIdle()` must return no events when
`isIdle()` returns false. The queue accumulates and drains only when
the system is idle. A new adapter installs only if after-accuracy
exceeds before-accuracy.

**Enforce arbiter authority over external AI results.**
`ExternalAiResultAdapter.ContradictsLockedDuplicateRule` is the
production equivalent of E51B's CH-02. External AI answers that
contradict locked arbiter rules must be rejected, not silently
discarded and not accepted. The production test must assert both that
similarity was high and that the arbiter still rejected.

**Verify code routing behavior in production, not only in experiments.**
V06 (compound diagnostic+repair routes to BUG) was validated in E51A
and must be enforced as a production test. The sessions transferred
this finding to `InternalLlmGateway` and added a production test
asserting that "why and how to fix" routes to BUG and loads `bug.lora`.

**Harden the `ExternalAiResultAdapter` towards general arbiter lookups.**
The current implementation hardcodes the identity_consistency check as
a private method. The right shape is looking up the constraint by name
from the `ArbiterConstraints` array. This should be generalized before
more arbiter constraints are added, to avoid a proliferation of private
methods.

---

### What not to do

**Do not auto-approve `call_external_ai`.**
Any test that asserts `Allowed` for this tool name is wrong by design.
The tool exists in `RequiresApproval` because it crosses a fundamental
boundary in the architecture.

**Do not let external-AI approval persist across loop iterations.**
Two mechanisms are involved and they must not be confused.
`PermissionGate.Check("call_external_ai")` returns `RequiresApproval`
whenever a proposed external call is evaluated. The state-machine guard
`ExternalAiStateGuards.ShouldRequestExternalAiApproval(...)` returns false
after `ApprovalRequestedKey` is already true, so the same state does not
request approval twice. A previous approval never becomes a permanent
permission for a later request.

**Do not trigger learning events from Accepted signals alone.**
Only DPO pairs trigger learning events. Accepted feedback events produce
positive training examples but not DPO pairs. The threshold count
(`minimumExamples`) applies to DPO pairs specifically. Routing only
Accepted signals through the feedback router must leave `QueuedCount`
at zero.

**Do not install a new adapter when after-accuracy is lower than
before-accuracy.**
The rollback gate is hard. A new adapter that regresses on the current
domain must not be installed. The test for this must use actual
`AutonomousLearningPipeline` wiring, not a structural stub.

---

## LOCKED DECISIONS â€” CARRY THESE INTO EVERY SESSION

D-01: OperationResult<T> â€” no exceptions.
D-04: Domain split: CODE_Q / BUG / PLAN / ARCH / REFACTOR â€” these are
  the canonical names. No others.
D-05: LoRA adapters per domain, 2MB each.
D-06: 70/30 replay buffer â€” prevents catastrophic forgetting.
D-07: StackDetector runs before normalization â€” markers are destroyed by
  E15 if normalization runs first.
D-09: max_weight = total / (classes Ã— rare) â€” constant Ã—10 caused
  catastrophic forgetting in E44.
D-11: No external ML frameworks â€” TensorCore (project 00) is the only
  tensor primitive.
D-13: Tier promotion requires coverage verification, not only metric
  passage. Check subject density, distinct predicate profiles, and named
  edge risks before scaling.
D-14: Internal LLM always first; external AI requires explicit single-use
  approval with named missing-competence; all results examined and stored
  as training signals; training at idle time only; new adapter installs
  only if after-accuracy exceeds before-accuracy.
D-15: Priority decides what to attempt.
  KNN uses arbiter history as memory. Arbiters decide what is accepted.
  Nothing overrides arbiter rejection except explicit user correction.

---

## GENERAL SESSION BEHAVIOR

### What to do

**Record a clean test baseline before any work and verify it has not
changed after each phase.**
The code sessions showed consistent Round Contract structure: baseline
at start, current at end, produced, gate, gap. Every code change must
keep the full suite green. No phase is complete until the suite passes.

**Follow the round contract format: baseline, current, produced, gate, gap.**
The gap field is especially important â€” it prevents sessions from ending
without a clear statement of what is not yet done.

**Name the next concrete action at the end of every turn.**
The product session ended with "ready for the next slice when you are."
That is correct â€” it names the state clearly without overpromising.

**When corrected, acknowledge the correction and state what changes.**
The code session showed the correct pattern: "You were right. I corrected
the examination instead of trying to patch over it." Then immediately
state what changes. Do not defend the wrong approach.

**When a design decision is confirmed across multiple sessions, add it
to DECISIONS-LOCKED.md.**
D-13 was added after the E51A Tier 1 experience confirmed the rule.
D-14 was added after E51B confirmed the internal-first design. These
decisions protect future sessions from re-litigating settled questions.

**Create a round zip at the end of every completed work round.**
The zip must contain every file produced or changed in that round, a
`ROUND-MANIFEST.md`, the verification result, and
`SESSION-START-PROMPT-ROUND-ARTIFACT-ZIPPING-v1.md`. Do not include
build outputs, `bin`, `obj`, temporary extraction folders, IDE state,
or unrelated dirty files. A future session should be able to open the
zip and understand what changed, why it changed, and how to continue.

---

### What not to do

**Do not declare "I have found the relevant files" and immediately narrow.**
This is edge cutting. The last code session was corrected for exactly
this: announcing that the file landscape confirmed something and then
proceeding to narrow intake based on that announcement. Full examination
must precede any narrowing.

**Do not produce a plan and call it the output when the task was to
produce batch files.**
The last code session's second failure: `DYNAMIC-FLOW-FIRST-FILE-LEARNING-SET-PLAN-v1.md`
was produced as if it were the deliverable. The deliverable was the
actual 234-row batch. Plans are intermediate outputs, not final ones.

**Do not treat silence or lack of pushback as approval.**
The sessions showed this explicitly as anti-pattern 6: treating Luba's
silence as confirmation that the goal is being met. The goal must be
actively verified, not assumed from the absence of correction. A
correction may arrive several turns later when contamination has
compounded.

**Do not stop unnecessarily on already-resolved decisions.**
Anti-pattern from the no-product-decisions skill: if a decision is in
DECISIONS-LOCKED.md, it is closed. Stopping to re-present it or ask
for confirmation wastes turns and signals that the locked decisions
file was not read.

**Do not produce the next phase before the current phase is reviewed.**
The document examination method is explicit: produce one document's
sets, stop, review, then choose the next document. The pattern applies
equally to experiment phases: Phase 1 fixes are verified before Phase 2
expansion begins.

**Do not use code terms or technical identifiers in natural-language learning
set content unless the runtime output itself is formal.**
Plain user-facing pairs should be written in human language. Formal compiler
outputs, predicates, arbiter names, evidence fields, and canonical file or
flow names may use their exact technical names because those names are part
of the real runtime output. The explanation around them must remain readable
to a human who will examine the flow.
