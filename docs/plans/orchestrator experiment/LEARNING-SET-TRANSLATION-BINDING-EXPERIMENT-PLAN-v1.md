# Learning Set Translation Binding Validation Experiment
# Orchestration proof applied to a translation plan:
# examining whether source material is correctly mapped to learning rows,
# and whether the bindings between source lines, diagram nodes, and
# teaching purposes are honest and complete
# Status: DESIGN — awaiting approval before any documents are produced

---

## What This Experiment Proves

A learning set translation plan makes claims about how source material maps
to teaching rows. It claims that every source line is assigned to the right
diagram node. It claims that every diagram edge is explained by at least one
row. It claims that version facts are preserved and not silently overwritten.
It claims that gaps are exposed rather than hidden.

None of those claims are verifiable by inspecting the plan's structure alone.
Verifying them requires an examiner that understands the translation domain,
can identify when a mapping claim is too broad or too narrow, can detect when
a gap is marked as a fact, and can determine whether the binding between a
source line and a diagram node is justified or assumed.

The dynamic flow orchestrator is that examiner. This experiment tests whether
the orchestrator can receive a translation plan as input, decompose it into
the domains it governs, translate each domain into formal examination
statements, plan an examination sequence, apply arbiters to the binding
claims, and produce a human-readable verdict that names what is correctly
mapped, what is incorrectly mapped, what is missing, and what would produce
wrong teaching if left uncorrected.

The input is the dynamic flow first-file learning set plan. It is a good
subject because it is a real plan with real complexity: it governs the
translation of 234 source lines, 7 batch families, 27 diagram nodes, 23
diagram edges, 6 simulation checks, and a known version split between a live
diagram and an archived diagram. The plan is internally well-structured. Its
failure modes are not in its structure but in its binding claims — some
mappings are too broad, some gaps are understated, and one version fact
carries a specific risk that the plan acknowledges but does not fully resolve.

---

## The Five Claim Categories in This Plan

### Claim category 1 — Source-line-to-node binding completeness

The plan claims that all 234 source lines are routed to diagram nodes or to
document-boundary teaching. The routing plan table assigns line ranges to
primary and secondary teaching targets. The claim is that this routing is
complete and correct — no line falls through without a teaching assignment,
and no line is assigned to a node it does not actually teach.

What the orchestrator must examine: are the routing assignments justified by
the content of the source lines, or are they assumed? A line range assigned
to "universal continuous learning" must actually contain content about
continuous learning, not just be adjacent to content about it. The routing
table assigns lines 13-21 to universal continuous learning with secondary
targets including dataset curator, arbiter pool, and all unit nodes. That
secondary target list is very broad — "all unit nodes" as a secondary target
means any line in this range could be cited as teaching any node, which
weakens the teaching specificity.

The specific risk: broad secondary target assignments allow a row to be
counted as teaching a node without actually teaching it. A simulation that
removes the row and does not visibly damage the node's reconstruction is
evidence that the binding was too broad.

### Claim category 2 — Diagram edge teaching coverage

The plan lists 23 edges and requires one row per edge. The claim is that
each edge row is supported by source material — either document lines or
session lines that explain the transition. The plan acknowledges that some
edges "may be weakly explained by the document" and instructs that these be
marked as missing teaching rows rather than silently filled with inferred
reasoning.

What the orchestrator must examine: which edges have explicit source support
and which are marked as gaps versus which are quietly assumed to be covered
by nearby rows. The plan names the self-edge on the Domain applier node as
specially important — it represents repeated application and reprocessing.
This edge has no explicit mention in the 23-edge list's source support column
(the plan says to list source document or session lines, but the edge
inventory does not list them). The orchestrator must determine whether source
support for this edge actually exists in the source line routing or whether
it is an inferred edge.

The specific risk: the self-edge (edge 12: Domain applier -> Domain applier)
is the only self-referential edge in the graph. The plan explicitly warns
not to remove it. But a warning not to remove an edge is different from
evidence that the edge has teaching row support. An edge that is warned
against removal but has no explicit source citation is a gap that the plan
has not exposed.

### Claim category 3 — Version fact preservation binding

The plan identifies a specific version split: the live diagram (27 nodes,
23 edges) and the archive diagram (24 nodes, 23 edges) differ by three
explanatory text nodes. It instructs that both versions be represented in
the learning set. It also identifies a test baseline conflict: the source
document mentions 877 tests, the current verified baseline is 938, and the
plan instructs that both values be preserved in separate rows without silent
overwriting.

What the orchestrator must examine: the plan correctly identifies both
version facts. But the instruction for handling the test baseline conflict
("a learning row must preserve the source value and a separate version row
must preserve the current verified value") describes two rows but does not
specify which family each row belongs to. Family 6 (version rows) is the
natural home for both. But Family 1 (source document line rows) would
encounter the 877 value at its source location and must handle it there.
The plan does not say what a Family 1 row must do when it encounters a value
that is known to be stale — preserve it with a note, or simply route it to
Family 6 without annotation in Family 1.

The specific risk: if Family 1 rows silently route the 877 value to Family 6
without annotating the staleness in the Family 1 row itself, a simulation
that uses only Family 1 rows will encounter 877 and have no signal that the
value is stale. The simulation plan (Simulation 1 uses Family 1 rows only)
would not expose this problem because Simulation 1's expected result does
not include a check on the test baseline value.

### Claim category 4 — Gap marking completeness

The plan lists five known gaps in the "Known Gaps To Preserve" section. It
instructs that these gaps be exposed rather than hidden. The claim is that
the five listed gaps are the complete list of known gaps — or at least the
most important ones.

What the orchestrator must examine: the five listed gaps are real and
correctly identified. But the plan's own routing table creates at least one
additional gap that is not listed: the lines 195-209 range is assigned to
"Open items" with teaching target "Human review rows, unresolved decision
rows." Open items by definition are items without a resolved answer. The plan
instructs that these lines get rows, but it does not instruct what a row for
an unresolved decision must say to prevent the learning set from treating the
unresolved item as a resolved one. This is a gap in the gap-handling
instructions, not in the source material itself.

A second unlisted gap: edge 19 (Domain result-translation layer -> Arbiter
selector) is the connection between the translation output and the arbiter
choice mechanism. This edge is architecturally significant — it is the point
where the flow determines whether the translated results are ready for
arbitration. The source line routing table does not explicitly cite which
source lines teach this edge. Lines 131-141 cover feedback paths, and lines
in that range may touch this edge indirectly, but the routing table's primary
target for those lines is "Arbiter pool, arbiter selector, reprocessing edge"
— which covers the feedback edge (edge 22: Arbiter pool -> Domain splitter)
but not necessarily edge 19.

### Claim category 5 — Simulation test sufficiency binding

The plan defines six simulations. Each simulation uses a subset of families
and states expected results and expected missing parts. The claim is that
the six simulations together are sufficient to detect any significant binding
failure in the learning set.

What the orchestrator must examine: Simulation 6 (edge removal test) is the
most diagnostic because it tests each edge's teaching strength by removing
its row and checking whether the simulated output degrades. The plan
correctly states that if removing an edge does not visibly damage the output,
the row is too weak and must be expanded. But the plan does not specify how
to run this test — what "visibly damages the simulated output" means in
measurable terms. A simulation that relies on qualitative judgment ("does
the explanation seem incomplete?") is weaker than one that relies on a
specific expected output that must be present or absent.

The specific risk: Simulation 6 is the experiment's quality gate but its
pass criterion is undefined. If the criterion is qualitative, the simulation
will produce different verdicts depending on the examiner's interpretation.
This is a sufficiency gap in the test design rather than a content gap in
the learning rows.

---

## The Three Scenarios

### Scenario L1 — Broad secondary target dilution

**The examination input:** The routing plan table, specifically the line
ranges with broad secondary target lists, presented to the orchestrator with
the question: are any secondary target assignments too broad to produce
specific teaching?

**What the orchestrator must find:** Lines 13-21 are assigned to
"universal continuous learning" with secondary targets including "all unit
nodes." This is a genuine assignment — the lines about universal continuous
learning do teach all unit nodes because the principle applies universally.
But "all unit nodes" as a formal secondary target in a learning row means
that a row from this range can be cited as teaching any node. This creates
a coverage illusion: the learning set appears to cover all nodes for this
principle, but the coverage is diffuse rather than node-specific.

The orchestrator must distinguish between a broad assignment that is
substantively correct (the principle genuinely applies to all nodes) and
a broad assignment that is a coverage shortcut (the line is too generic to
teach any specific node well). For lines 13-21, the former is true — but
the arbiter must still name the distinction so the executor understands what
the broad assignment means and does not treat it as specific node coverage.

**What the arbiters must do:** The teaching specificity arbiter fires with
a concern verdict — not a reprocess verdict. The assignment is not wrong,
but it carries a coverage risk that must be explicitly acknowledged in the
row structure. The concern names the specific risk: a simulation that removes
a "universal continuous learning" row and checks only one target node may
not detect the removal.

**Why this scenario is the right starting point:** It is the clearest case
where the plan's instruction is internally consistent but creates a risk
that the instruction does not name. Finding this type of problem requires
examining the relationship between the routing instruction and its downstream
effect on simulation quality — which is exactly what the orchestrator is
designed to do.

---

### Scenario L2 — Self-edge teaching gap

**The examination input:** Edge 12 (Domain applier -> Domain applier) from
the edge inventory, presented to the orchestrator with the question: what
source material supports the teaching row for this edge, and is that support
explicit or inferred?

**What the orchestrator must find:** The plan explicitly warns not to remove
the self-edge and explains its meaning (repeated application and
reprocessing). But in the source line routing table, the lines most likely
to support this edge are lines 79-111 (cast of unit roles, primary target
"all diagram node rows and missing-node rows") and the session lines from
Product session 5 lines 401-418 (diagram node origin). Neither range
specifically mentions the self-edge or the concept of repeated application.

The session line routing plan mentions the reprocessing label node
("We may send some of fbx1...fbxn back to reprocessing with apx1...abxn")
which is a diagram label node, not an edge. The reprocessing label teaches
the concept of reprocessing but does not explicitly teach the self-edge as
a structural graph connection.

The orchestrator must classify this as an unlisted gap: the self-edge is
present in the edge inventory and warned against removal, but its source
support is inferred from the reprocessing label node rather than explicitly
cited. The arbiter produces a concern verdict naming the specific source
search that would close the gap: look in Product session 5 lines 741-777
(the arbiter selector and mirror pattern section) for any language about
repeated application, partial results, or reprocessing passes.

**Why this scenario is the right second step:** It requires the orchestrator
to distinguish between a gap that is explicitly named in the plan and a gap
that exists but is not named. The plan lists five known gaps but this is a
sixth. Finding it requires reading the edge inventory against the routing
table and noticing the absence of a citation, not just checking whether
the plan's own gap list is internally consistent.

---

### Scenario L3 — Simulation pass criterion gap

**The examination input:** Simulation 6 (edge removal test) from the
simulation plan, presented to the orchestrator with the question: is the
pass criterion for this simulation measurable, and if not, what would make
it measurable?

**What the orchestrator must find:** Simulation 6 states that removing an
edge row and not visibly damaging the output means the row is too weak. This
is a correct design principle but an unmeasurable criterion. "Visibly
damages" depends on the examiner's interpretation of the simulated output,
which means two examiners could reach different verdicts on the same edge.

The orchestrator must identify the specific expected outputs that would make
Simulation 6 measurable. For the self-edge, a measurable criterion would be:
"after removing the self-edge row, a simulated reconstruction of the Domain
applier node's behavior must fail to mention repeated application, partial
results, or reprocessing passes — any of these three phrases absent in the
reconstruction is a pass for the removal test." For the arbiter feedback
edge (edge 22), the criterion would be: "after removing this row, a
simulated reconstruction of the arbiter pool's output must fail to mention
the domain splitter as a possible feedback destination."

The arbiter produces a reprocess verdict for Simulation 6's criterion
definition — not a concern verdict — because the simulation's pass criterion
is a structural requirement of the simulation design, not an optional
enhancement. A simulation that cannot be run with binary verdicts is not a
simulation; it is a qualitative review. The reprocess instruction names the
exact form the criterion must take for each of the six simulations.

**Why this scenario is the right third step:** It requires the orchestrator
to evaluate the quality of a testing mechanism rather than the quality of the
content being tested. This is the hardest judgment in the three scenarios
because it requires reasoning about what a good test looks like, not just
what a good learning row looks like.

---

## Document Design

Four documents are required before any training pairs are written.

### Document 1 — Translation Examination Domain Map

File: `LEARNING-SET-EXAM-DOMAIN-MAP.md`

This document maps the five claim categories to their examination domains
and specifies the formal vocabulary for each. The five domains are:
source-line-to-node binding, edge teaching coverage, version fact
preservation, gap marking completeness, and simulation test sufficiency.

For each domain, the document specifies:
- the kind of claim that belongs to this domain
- the vocabulary for a formal domain statement
- what a concern verdict looks like in this domain
- what a reprocess verdict looks like in this domain
- what a pass verdict looks like in this domain

The most important vocabulary distinction is between a gap that is named
(the plan knows about it and instructs how to handle it) and a gap that is
unnamed (the plan does not know about it or does not name the handling
instruction). Named gaps get a different arbiter treatment than unnamed ones.

### Document 2 — Binding Failure Taxonomy for Translation Plans

File: `LEARNING-SET-EXAM-BINDING-FAILURE-TAXONOMY.md`

This document defines the binding failure types specific to translation plan
examination. There are four:

**Coverage illusion:** a broad target assignment appears to cover many nodes
but provides no node-specific teaching. The plan knows the principle applies
broadly but the learning rows cannot distinguish which node the principle
most specifically teaches.

**Inferred edge support:** an edge is present in the edge inventory but its
teaching row is supported by inferred reasoning from adjacent material rather
than explicit source citation. The plan warns against removing the edge but
does not provide a source reference for it.

**Version handling ambiguity:** a version conflict (stale value, diagram
split, hash difference) is identified but the instruction for handling it
in each family is incomplete. A simulation that uses only one family will
encounter the conflict without a handling signal.

**Unmeasurable simulation criterion:** a simulation check defines an
expected result in qualitative terms rather than in specific expected phrases
or structures. The criterion cannot be applied consistently across examiners.

For each type, the taxonomy defines the detectable signal, the arbiter
verdict type (concern or reprocess), and the remediation form.

### Document 3 — Expected Examination Traces

File: `LEARNING-SET-EXAM-EXPECTED-TRACES.md`

One section per scenario. The most critical section is the node-03 scheme
planner output for Scenario L2 (self-edge teaching gap). The planner must
sequence the examination correctly: first establish which edges are in the
inventory (node-01 produces a packet per edge), then translate each edge
packet into a formal statement about its source support type (explicit or
inferred), then plan the examination in an order that checks the
explicitly-cited edges first and the inferred edges last (because inferred
edge support requires examining the source routing table, which is a
dependent step).

The Scenario L3 section must show the reprocess verdict as a node-08 output
that is different in structure from the concern verdicts in Scenarios L1
and L2. A reprocess verdict names a specific corrected version of the
deficient content. For Scenario L3, the corrected version is the measurable
criterion for each of the six simulations.

### Document 4 — Executor Contract

File: `LEARNING-SET-EXAM-EXECUTOR-CONTRACT.md`

Exact files to read first, files to create, test names to add, evidence
files to write, commands to run, pass gates, what not to change, and zip
requirement for each round.

---

## Learning Set Design

The learning sets for this experiment teach the orchestrator to reason about
translation plans as structured examination inputs. Three structural patterns
from prior training transfer directly.

The coverage illusion pattern (Scenario L1) maps to the policy acceptance
boundary pattern from the registration training set: in both cases, something
is present in the split output but marked as insufficient — the domain packet
exists but the coverage status is boundary rather than covered. The
translation equivalent is: the node assignment exists but the coverage type
is broad rather than specific.

The inferred edge support pattern (Scenario L2) maps to the knowledge-edge
pattern from the onboarding orchestration experiment: in both cases, the
orchestrator must name something that is missing from local coverage rather
than inventing a substitute. The orchestrator does not know which Product
session 5 lines teach the self-edge — it must name that as an open source
lookup rather than asserting that the reprocessing label provides sufficient
support.

The unmeasurable criterion pattern (Scenario L3) is new. It has no direct
equivalent in prior training. The pattern is: a testing mechanism is present
and structurally correct but its pass criterion cannot be applied
consistently. The reprocess verdict for this pattern must produce a
corrected version of the criterion — which means the orchestrator must
know what a measurable simulation criterion looks like. The node-08 pairs
for this scenario are the most important new pairs to write.

### New pairs needed

Node-01 (domain splitter): three pairs, one per scenario. The A inputs
are the relevant sections of the translation plan. The B outputs are domain
packets for the examination domain — source binding packets, edge coverage
packets, version fact packets, gap marking packets, simulation criterion
packets.

Node-02 (domain translator): six pairs, two per scenario. Each A input is
one domain packet. The B outputs are formal examination statements in the
vocabulary defined in the domain map — specifying coverage type, support
type, gap type, or criterion type rather than simply describing the content.

Node-03 (scheme planner): three pairs, one per scenario. The Scenario L2
pair must show the dependency-ordered examination plan where the source
routing table lookup is a dependent step that cannot run before the edge
inventory is established.

Node-08 (arbiter units): six pairs, two per scenario. The most important
pair is the reprocess verdict for Scenario L3 — it must produce a
measurable criterion for each simulation, which requires the arbiter to
reason about what binary pass conditions look like for qualitative
statements.

Node-09 (formatter): three pairs, one per scenario. The audience is the
plan author preparing to dispatch the translation to an executor. The
output must be actionable: not "Simulation 6 criterion may be vague" but
"Add this specific phrase to the self-edge simulation criterion: after
removing the self-edge row, a reconstruction of the Domain applier node
must fail to mention any of these three concepts: repeated application,
partial results, reprocessing passes."

Total new pairs: twenty-one across five node files.

---

## Pass Gates

| Gate | Required result |
|---|---|
| Coverage illusion detection | broad secondary target in lines 13-21 identified and named in Scenario L1 |
| Inferred edge detection | self-edge teaching gap identified as unlisted gap in Scenario L2 |
| Verdict type distinction | Scenario L1 and L2 produce concern verdicts, Scenario L3 produces reprocess verdict |
| Reprocess verdict specificity | Scenario L3 reprocess verdict names specific measurable criteria for at least two simulations |
| Gap classification | named gap vs unnamed gap distinction applied correctly across all three scenarios |
| Dependency ordering | node-03 plan shows source routing table lookup as dependent on edge inventory in Scenario L2 |
| Version fact handling | 877-vs-938 conflict handling gap identified as a Family 1 annotation gap |
| Human result actionability | every finding includes a specific corrective action the plan author can apply immediately |
| Evidence written | JSON, markdown, and Mermaid files produced for each scenario |
| Existing tests | full suite remains green |

---

## The Distinction That Matters Most in This Experiment

The continuous learning plan experiment was about enforcement gaps — things
the plan said it would check but did not. This experiment is about something
different: honesty gaps — the difference between a plan that says everything
is covered and a plan that correctly names what is uncertain, inferred, or
unmeasurable.

The translation plan is more honest than most plans. It explicitly lists
known gaps, warns about specific edges, and acknowledges the diagram version
split. The experiment does not test whether the plan is dishonest. It tests
whether the orchestrator can find the things the plan is honest about but
incomplete on — the gaps within the gap handling, the inferred support
behind the named edge, the unmeasurable criterion inside the otherwise
well-designed simulation.

This is why the arbiter vocabulary matters. A concern verdict for a named
but incompletely handled gap is different from a concern verdict for an
unnamed gap, and both are different from a reprocess verdict for a
structurally deficient testing mechanism. The binding failure taxonomy
document must make these distinctions clear enough that the node-08 pairs
can produce the right verdict type for each scenario without ambiguity.

---

## Failure Classification

If the experiment fails because the domain splitter does not distinguish
named gaps from unnamed gaps: the domain map needs to establish gap naming
status as a first-class domain property, and the node-01 pairs need explicit
examples of both gap types.

If the experiment fails because the scheme planner treats the source routing
table lookup as a parallel step rather than a dependent step: the node-03
pairs for Scenario L2 need negative examples showing why running the source
lookup before the edge inventory produces an incomplete examination.

If the experiment fails because the arbiter produces a concern verdict for
Scenario L3 instead of a reprocess verdict: the binding failure taxonomy
needs a clearer definition of when an unmeasurable criterion requires reprocess
rather than concern. The test: if the deficiency prevents the simulation from
being run at all (not just imperfectly), it is a reprocess; if the simulation
can still run but may produce inconsistent verdicts, it is a concern. For
Scenario L3, qualitative criteria prevent consistent verdicts — this is a
reprocess.

If the experiment fails because the formatter produces generic actionable
advice rather than plan-specific corrective text: the node-09 pairs need
examples where the corrective action names the specific phrase, the specific
simulation number, and the specific expected output that closes the gap.

---

## Execution Order

Phase 1: Produce the four design documents. Stop for review.

Phase 2: Produce the twenty-one training pairs in node order. Stop after
node-03 for a targeted review of whether the Scenario L2 scheme plan
correctly sequences the source routing table lookup as a dependent step.

Phase 3: Run the experiment in deterministic mode for Scenario L1 (broad
secondary target dilution — the clearest structural finding). Record the
full trace. Evaluate against the oracle.

Phase 4: Run Scenario L2 (self-edge teaching gap — the unlisted gap
detection test) and Scenario L3 (simulation criterion gap — the reprocess
verdict test).

Phase 5: Write the evidence files and experiment results document.

---

## What This Will Tell Us

If the experiment passes: the orchestrator can examine a translation plan,
find gaps that the plan itself does not list, distinguish concern from
reprocess at the correct boundary, and produce formatter output that gives
the plan author specific corrective text rather than general advice.

The practical consequence is the same as for the planning document binding
experiment: a translation plan could be run through the orchestrator before
it is dispatched to an executor. The orchestrator's verdict would give the
executor precise knowledge of which source line assignments are too broad,
which edge rows have inferred rather than explicit support, and which
simulation criteria need measurable definitions. The executor starts with a
validated plan.

If the experiment fails because the orchestrator invents source support for
the self-edge rather than naming the inferred support as a gap: the
knowledge-edge training for the translation domain needs explicit examples
of "source lookup required, not inferred" as a coverage status distinct from
"covered" and "boundary."

If the experiment fails because the orchestrator cannot produce the
specific measurable criterion text in the Scenario L3 formatter output: the
node-09 pairs need examples of what a measurable simulation criterion looks
like for a qualitative expected result, written in the vocabulary the plan
author can apply directly.

Either result advances the project. The orchestrator's ability to find
unlisted gaps in a plan that already lists its known gaps is the hardest
test of the system's examination capability — and the most useful one.
