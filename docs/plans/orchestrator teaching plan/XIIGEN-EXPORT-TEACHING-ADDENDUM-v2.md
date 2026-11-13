# Addendum - Using Claude Session Export Data For XIIGen Teaching Sets

Extends: `XIIGEN-DRIVEN-SELF-TEACHING-GUIDANCE-v1.md`  
Supersedes: `XIIGEN-EXPORT-TEACHING-ADDENDUM-v1.md`  
Version: 2.0 - 2026-05-05  
Source batch: `C:\Users\lubao\Documents\xiigen\old\data-6308de18-3e10-4d75-9d43-35c6c315a9ad-1777178009-8b897c61-batch-0000.zip`

---

## 1. Purpose

The Claude export is a first-party XIIGen session source. It should be used to teach the orchestrator from real working conversations, not from polished architecture documents only.

It adds four teaching signals that normal XIIGen documents do not provide:

1. Raw developer prompt language.
2. Real correction events where the prior answer was rejected.
3. Temporal ordering across a long development conversation.
4. Cross-session carry-forward context.

This export must be processed as orchestration data first. The goal is to teach the machine to receive a human prompt, normalize it into a logical transition, split it across domains/sub-phases/tasks, run the correct flows, carry context forward, detect gaps, and schedule learning when expertise is missing.

For the first XIIGen self-teaching pass, real conversation prompts are the primary A-side authority. Synthetic prompts are not allowed unless they are explicitly marked as held-out rephrases of a real conversation prompt.

The executor must not treat each conversation as an isolated source. The export contains many conversations that are fragments of the same real development cycle. The correct teaching unit is a dated development-cycle bundle: all conversations, source files, corrections, and delivery artifacts that together solved one real XIIGen problem.

---

## 1.1 Primary Evidence Rule

The conversation export is not just another optional source. It is the source record for many of the documents already produced during the project. Therefore the executor must treat it as the evidence root:

```text
real human message -> assistant response -> correction/review -> derived document
```

The learning set row must preserve this chain. A derived document may be used as B-side evidence, explanation, or expected structure, but it must not replace the real human message as the A-side.

Every row produced from a derived XIIGen document must include:

```yaml
conversation_trace:
  export_batch: "batch-0000"
  conversation_uuid: "{uuid}"
  conversation_name: "{name}"
  human_message_uuid: "{uuid}"
  human_message_index: 0
  assistant_message_uuid: "{uuid-or-null}"
  derived_document_path: "{path-if-known}"
```

If the executor cannot trace a derived document back to a conversation message, the row is still allowed, but it must be marked:

```yaml
trace_status: "derived_document_without_conversation_anchor"
teaching_priority: "secondary"
```

Rows with real conversation anchors have priority over rows from derived documents without anchors.

---

## 1.2 No Imaginary Prompt Rule

The first teaching batches must not invent prompts to make coverage look complete.

Allowed A-sides:

```text
Raw human messages from the export.
Raw human messages from local XIIGen session files.
Short held-out rephrases of a real prompt, marked with source_prompt_id.
```

Disallowed A-sides:

```text
Synthetic prompts written only to cover a missing phase.
Architecture-document sentences rewritten as user prompts.
Assistant summaries used as prompts.
Idealized prompts that remove the user's real wording, typos, or uncertainty.
```

If a sub-phase has no real prompt evidence yet, mark it as a coverage gap. Do not fill it with an imaginary prompt.

---

## 1.3 Development-Cycle Bundle Rule

Before producing pair rows, split the export into development-cycle bundles.

A development-cycle bundle is:

```text
one real problem
  -> discussed across one or more conversations
  -> over one date window
  -> producing one or more delivery artifacts
  -> later reviewed, corrected, or extended
```

This bundle is the orchestration teaching source. It shows the whole machine cycle: user problem, transition choice, domain split, sub-phase routing, flow execution, arbiter correction, delivery output, and follow-up learning.

Do not process the export as:

```text
conversation 1 -> rows
conversation 2 -> rows
conversation 3 -> rows
```

Process it as:

```text
date/problem/deliverable bundle 1 -> rows + teaching cycle
date/problem/deliverable bundle 2 -> rows + teaching cycle
date/problem/deliverable bundle 3 -> rows + teaching cycle
```

The first pass should find bundles that produced real deliverables such as:

```text
implementation plan
canonical design pattern document
external source learning set method
orchestrator teaching plan
corpus expansion plan
round manifest
test evidence json
zip package
diagram
code change
```

Each bundle must produce a `cycle-map.yaml` before any pair rows are written.

```yaml
cycle_id: "xiigen-export-2026-05-05-orchestrator-teaching-cycle"
cycle_date_window:
  start: "2026-05-05T00:00:00"
  end: "2026-05-05T23:59:59"
real_problem: "{the development problem being solved}"
delivery_artifacts:
  - path: "{document/code/zip/evidence path}"
    artifact_type: "{plan | code | test-evidence | zip | diagram | manifest}"
    produced_from_conversation_uuid: "{uuid-or-unknown}"
source_conversations:
  - conversation_uuid: "{uuid}"
    conversation_name: "{name}"
    created_at: "{timestamp}"
    updated_at: "{timestamp}"
    role_in_cycle: "{problem_definition | design | implementation | review | correction | packaging}"
primary_prompts:
  - message_uuid: "{uuid}"
    message_index: 0
    prompt_role: "{problem_statement | correction | extension | verification_request}"
covered_phases:
  - "{phase id}"
covered_sub_phases:
  - "{sub-phase id}"
known_gaps:
  - "{gap discovered while processing the bundle}"
```

If a conversation has no delivery artifact and no connection to a real development problem, it is not used in the first teaching pass.

---

## 2. Export Inventory

The zip contains four JSON files:

```text
users.json         - account identity; do not use as teaching text
projects.json      - project metadata; use only for source grouping
memories.json      - memory/profile blobs; use only as vocabulary context
conversations.json - conversation messages; primary teaching source
```

Important size constraint:

```text
conversations.json uncompressed size is about 1.1 GB.
```

Do not read `conversations.json` with `json.loads(z.read(...))`, `File.ReadAllText`, or any other full-file load. The executor must stream it or extract one conversation at a time. This is mandatory because teaching must be able to run on ordinary PCs.

---

## 3. Source Classification

Use this source tier:

```yaml
source_family: "xiigen-session-export"
source_license_state: "first_party_private"
source_batch: "claude-export-batch-0000"
```

Do not mix this source with public external-source licensing rules. This is owned XIIGen session data.

Redaction rule:

```text
Do not copy account emails, user identity fields, or unrelated personal profile text into pair rows.
```

The teaching rows should contain only the development prompt, the orchestration result, the correction signal, and the relevant source references.

---

## 4. Valid Conversation Filter

Include a conversation when its title, summary, or first substantial human messages indicate XIIGen development work:

```text
xiigen
xii gen
flow
orchestrator
architect
planning
design review
testing and architecture
claude code
code review
learning set
dynamic flow
logical unit
logical flow
logical orchestrator
```

Exclude a conversation when:

```text
It is not about software development.
It is Hebrew/Russian language-learning or form-filling content unrelated to XIIGen.
It has fewer than four total messages.
It contains only session-loading commands and no domain content.
```

Session-loading messages may provide carry-forward context, but they are not valid A-sides by themselves.

---

## 5. Correct Pair Mapping

The canonical runtime path is:

```text
Human prompt
  -> LogicalTransitionOrchestrator
  -> SelectedLogicalTransition
  -> DomainSplitterUnit
  -> DomainPacket[]
  -> nine-node flow branches
```

Therefore the same raw human message can produce several pair layers, but each layer has a different A-side.

### Layer 0-A - Compiler

```text
PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
```

A-side:

```yaml
request_text: "{raw human message}"
```

B-side:

```yaml
compiled_text: "{raw human message}"
compiled_predicate_ids:
  - "{predicate id}"
confidence: 0.0-1.0
evidence_terms:
  - "{term that activated predicate}"
knowledge_edges:
  - "{missing vocabulary, if any}"
```

### Layer 0-B - Transition Orchestrator

```text
LogicalTransitionOrchestrator : ILogicalOrchestrator<LogicalOrchestratorRequest>
```

A-side:

```yaml
user_request: "{raw human message}"
context_packet:
  prior_decisions: []
  prior_constraints: []
```

B-side:

```yaml
candidate_transitions:
  - transition_type: "{feature_addition | repair | learning_cycle | topology_mutation | qa_release | external_source_integration}"
    A: "{current state}"
    B: "{target state}"
    X: "{transformation}"
    confidence: 0.0-1.0
    selected: true|false
    rejection_reason: "{required when selected=false}"
selected_transition:
  transition_type: "{selected type}"
  A: "{current state}"
  B: "{target state}"
  X: "{transformation}"
```

### Layer 0-C - Domain Splitter

```text
DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
```

A-side:

```yaml
selected_transition:
  transition_type: "{selected type}"
  A: "{current state}"
  B: "{target state}"
  X: "{transformation}"
```

B-side:

```yaml
domain_packets:
  - domain_id: "{phase/sub-phase/domain}"
    phase_id: "{phase}"
    sub_phase_id: "{sub-phase}"
    task_type: "{task type}"
    depends_on:
      - "{upstream packet id}"
    reason: "{why this packet is required}"
```

Do not use the raw human message as the DomainSplitterUnit A-side. The splitter receives the selected transition, not the original prompt directly.

---

## 6. Valid A-Side Rule

Use a human message as an A-side only when it contains real development intent.

Accept examples:

```text
"I would like projects 21-23 to use the internal LLM as much as possible..."
"Can the orchestrator split this request across phases and sub-phases?"
"Why does the topology registry not behave like a recursive orchestrator?"
```

Reject examples:

```text
"Please load the session."
"Continue."
"Read this."
"Created a file, ran a command..."
```

Do not paraphrase accepted A-sides. Keep typos, partial wording, and natural user phrasing. The compiler must learn real input, not polished documentation language.

---

## 7. Correction Event Extraction

Correction events are the highest-value teaching signal in the export. They become DPO and arbiter training rows.

A correction event has this message shape:

```text
H0: human asks original prompt
A0: assistant gives wrong/incomplete answer
H1: human rejects or corrects A0
A1: assistant produces corrected answer, or H1 itself contains the accepted correction
```

The DPO pair is:

```yaml
prompt: "{H0}"
rejected: "{A0}"
chosen: "{A1 if accepted, otherwise the corrective content in H1}"
correction_signal: "{phrase in H1 that indicates rejection}"
```

Correction signal phrases include:

```text
no
wrong
incorrect
not right
still not
missing
you missed
not aligned
that is not what I asked
why did you
why are you
explain to me why
fix this
do not stop
you are still far
```

Important: the correction belongs to the previous human prompt and previous assistant answer. Do not attach the correction to the correction message itself as if it were a new independent prompt.

Every correction event must produce two artifacts:

1. A `CorrectionLearningUnit` preference pair.
2. A held-out corruption case where `rejected` is treated as the wrong output.

---

## 8. Streaming Extraction Requirement

The executor must process the export in streaming mode.

Allowed extraction patterns:

```text
Open zip entry stream -> streaming JSON reader -> one conversation at a time.
Extract conversations.json to temp disk -> streaming JSON reader -> delete temp file after batch.
Use a batch index file that records conversation offsets or uuids.
```

Disallowed extraction patterns:

```text
json.loads(z.read("conversations.json"))
File.ReadAllText("conversations.json")
ConvertFrom-Json on the full conversations.json file
Loading all messages into one in-memory list
```

For each conversation, write its learning-set files before moving to the next conversation. The executor must not wait until all conversations are parsed before writing rows.

Correction to the previous sentence: the executor must write bundle files incrementally, not isolated conversation files. During streaming, it may write one lightweight conversation summary per conversation, but teaching rows are written only after the conversation has been assigned to a development-cycle bundle.

Required streaming sequence:

```text
Pass 1 - Metadata index
  Read conversations one at a time.
  Write conversation-index.jsonl with uuid, name, summary, created_at, updated_at,
  message counts, attachment names, file names, and candidate delivery phrases.

Pass 2 - Bundle grouping
  Group conversation index rows by date window, shared problem vocabulary,
  artifact references, and follow-up/correction links.
  Write one cycle-map.yaml per bundle.

Pass 3 - Bundle extraction
  Re-open only the conversations that belong to the selected bundle.
  Extract prompts, corrections, assistant outputs, and delivery references.
  Produce pair rows for that bundle.

Pass 4 - Teach and verify
  Teach one row at a time.
  Run the full orchestrator against the bundle's primary problem prompt.
  Measure result per flow, node, CPU, RAM, time, and disk.
```

The metadata index is allowed to cover the whole export. Full message text extraction is restricted to the current bundle.

---

## 8.1 Date And Delivery Grouping

Use dates as the first split because XIIGen work often happens in dense bursts. A single date may contain several conversations that all serve the same development delivery.

Initial grouping rule:

```text
Bucket conversations by local calendar date using created_at and updated_at.
```

Then merge or split buckets using delivery evidence:

```text
Merge adjacent dates when conversation N clearly continues the same delivery as conversation N-1.
Split one date into multiple bundles when it produced unrelated deliveries.
```

A conversation belongs to a bundle when at least one of these is true:

```text
It contains the primary user problem statement for the bundle.
It references files/documents/artifacts produced by the bundle.
It reviews or corrects an artifact produced by the bundle.
It continues a prompt from another conversation in the bundle.
It contains test/build/evidence output for the bundle.
```

Delivery phrases to detect in human or assistant text:

```text
created a file
wrote the plan
manifest
evidence
zip
test suite
build passed
committed
pushed
design document
implementation plan
canonical pattern
learning set
teaching plan
round
phase
```

The executor must prefer bundles with a clear delivery artifact because those bundles represent real development-world problems that reached an output.

---

## 9. Memory Blob Usage

`memories.json` contains conversation and project memory. This is context, not pair material.

Use memory for:

```text
Vocabulary calibration.
Confirming project domain terms.
Finding missing predicate terms.
Explaining why a prompt refers to "21-23", "dynamic flow", or "topology registry".
```

Do not use memory for:

```text
A-side rows.
B-side expected outputs.
DPO chosen/rejected answers.
Uncited carry-forward facts.
```

If a memory-derived fact is used as context, record it as:

```yaml
context_source: "claude-export-memory"
context_use: "vocabulary_calibration"
```

Do not silently insert memory facts into the expected answer.

---

## 10. Output Layout

Write export-derived rows here:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\
  claude-export\
    batch-0000\
      {conversation-slug}\
        session-map.yaml
        manifest.yaml
        layer-0-a\
        layer-0-b\
        layer-0-c\
        layer-1\
        layer-2\
        layer-3\
        correction-pairs\
        held-out\
```

Write teaching measurements here:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\teaching-ledger\
  claude-export\
    batch-0000\
      {conversation-slug}\
        teaching-ledger.jsonl
        cycle-summary.json
        checkpoints\
```

---

## 11. Conversation Manifest

Each processed conversation must produce:

```yaml
source: "claude-export"
source_batch: "batch-0000"
conversation_uuid: "{uuid}"
conversation_name: "{name}"
created_at: "{timestamp}"
updated_at: "{timestamp}"
total_messages: 0
human_messages: 0
valid_a_sides: 0
correction_events: 0
rows_produced:
  compiler: 0
  transition_orchestrator: 0
  domain_splitter: 0
  node_pairs: 0
  correction_pairs: 0
  held_out: 0
teaching_status:
  taught_rows: 0
  promoted_units: 0
  rejected_units: 0
resource_summary:
  total_training_ms: 0
  total_inference_ms: 0
  max_working_set_delta_mb: 0
  checkpoint_bytes: 0
```

---

## 12. Processing Order

Process high-signal XIIGen conversations first:

1. `Xiigen flow 01 testing and architecture review`
2. `Loading xiigen session and architecture skills`
3. `Preparing flow 01-24 for development with Claude code`
4. `planning session - xiigen first run`
5. `Co-architect collaboration and planning`
6. `Configuring architect model behavior protocols`
7. `Xiigen architect and product designer consultation`
8. Remaining relevant XIIGen conversations by descending valid A-side count.

Do not process small setup-only conversations first. They do not give enough domain signal and can distort the compiler toward session-control language.

---

## 13. Acceptance Gates

Before a conversation batch is accepted:

```text
[ ] conversations.json was processed in streaming mode.
[ ] No assistant messages were used as A-sides.
[ ] No account identity or unrelated personal fields were copied into rows.
[ ] Every accepted A-side contains real development intent.
[ ] DomainSplitterUnit rows use SelectedLogicalTransition as A-side.
[ ] Every correction event links H0/A0/H1/A1 correctly.
[ ] Every correction event produced both a DPO pair and a held-out corruption case.
[ ] Memory was used only as vocabulary/context calibration.
[ ] Each row records source conversation uuid and message index.
[ ] A manifest exists before moving to the next conversation.
[ ] Teaching metrics include elapsed time, CPU percent, working-set delta, disk delta, and checkpoint bytes.
```

If any gate fails, reject the conversation batch and do not teach from it.

---

## 14. First Batch Recommendation

Use the current bundle map as the first-pass ordering:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator teaching plan\
CLAUDE-EXPORT-DEVELOPMENT-CYCLE-BUNDLE-MAP-v1.md
```

The first bundle should be:

```text
legacy-docs-skills-from-old-code
```

This bundle teaches the broad real-world task:

```text
existing/old codebase -> documentation -> skills library -> tests/plans -> correction learning
```

This is more generally valuable than starting with the newest orchestrator-only sessions because many users will ask the system to understand an existing codebase and produce documentation, skills, plans, and reusable guidance from it.

After that, process:

```text
figma-to-code-skills-to-xiigen
self-learning-cycle-validation
flow-01-station-testing
session-prompt-and-learning-protocol
value-proposition-code-gaps
April orchestrator-machine-cycle bundles
May implementation/teaching bundles
```

Teach one row at a time:

```text
extract row -> teach unit -> run orchestrator -> measure -> write ledger -> decide promote/reject -> continue
```

Do not teach the entire export in one run. The first objective is to prove that session-derived rows improve compiler/router/orchestrator behavior without increasing CPU, RAM, or disk beyond acceptable limits.

---

# Last updated: 2026-05-05
