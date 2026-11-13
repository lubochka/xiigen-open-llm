# Document Examination To Training Sets Plan v1

## Superseded Pair-Writing Rule

This plan is preserved as a useful historical bridge from the failed ledger-only
attempt to the later training-set method. For actual pair writing, it is
superseded by `EXAMINATION-METHOD-FINAL.md`.

The final method takes precedence wherever this document still says or implies
that `B expected` may be a description, category, label, or planning hint. B
must be the exact node output written in full. This document's ledger-style
language is useful for provenance, but it must not be copied directly into node
training pairs.

The current architecture also requires a compiler pre-stage before the original
node list. New output folders must include `node-00-logical-structure-compiler-set.md`
before the domain-splitter file.

## Purpose

This plan explains how to examine one gathered document and turn it into
proper per-node learning sets for the dynamic-flow system.

The goal is not to summarize a document.

The goal is to teach each node LLM what it receives as input, what it must
produce as output, and how its output moves to the next node in the flow.

The method below follows the successful Flow-01 retry:

- one source document
- all source lines preserved
- versions recorded
- session or history evidence recorded when found
- UML nodes and edges preserved
- per-node training files created
- simulation used to expose missing teaching material

## Required Inputs

Every examination round must start with these inputs:

1. One source document.
2. The dynamic-flow UML file.
3. The corrected visual guide, if one exists.
4. The root folder where older versions and session files may live.

For the Flow-01 example:

- Source document:
  `C:\Users\lubao\Documents\xiigen\business flows\01-user-registration.md`
- UML:
  `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\improve algo.drawio.xml`
- Corrected visual:
  `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\llm_pipeline_corrected.svg`
- Search root:
  `C:\Users\lubao\Documents\xiigen`

## Non-Negotiable Rules

1. Examine one document at a time.
2. Do not replace the source document with a summary.
3. Preserve every source line.
4. Preserve blank lines and separators.
5. Preserve every UML node.
6. Preserve every UML edge.
7. Keep array stages as arrays:
   - domain processor
   - applier
   - result translator
8. Keep the arbiter selector separate from the result translator.
9. Place arbiters before final formatting.
10. Mark uncertain history as uncertain.
11. Mark derived rejected examples as derived, not as source text.
12. Do not let later documents contaminate the one source document.

## The Dynamic Flow Shape

The examination uses this flow:

1. User prompt
2. Domain splitter
3. Domain translator array
4. Scheme planner
5. Domain processor array
6. Applier array
7. Result translator array
8. Arbiter selector
9. Arbiter units
10. Formatter

The important corrections are:

- `db1...dbn` means multiple processors, not one processor.
- `fbi(bi)->Rqi` means multiple appliers, not one applier.
- `Rb1..n -> user language` means multiple result translators.
- the arbiter selector sees the result collection before formatting.
- arbiter units may send selected pieces back for reprocessing.
- the formatter receives only approved translated results.

## Round Structure

Each document examination round has seven phases.

### Phase 1 - Identify The One Source Document

Pick one document and name it exactly.

Do not gather five related documents and call them one document.

Record:

- full path
- file name
- line count
- byte size
- first line
- last line
- why this document was chosen

If there are related documents, list them only as related material.
They do not become source content for this round.

### Phase 2 - Find Versions

Search the root folder for exact filename matches and likely duplicates.

Look in:

- current folders
- `old`
- `Older`
- `history`
- `session_files`
- `.claude\worktrees`
- archive files when needed

For each version, record:

- path
- size
- line count
- last modified time
- content hash or other identity check

Then compare versions and list exact changed lines.

If two files are duplicates, say they are duplicates.
If two files differ, preserve both as versions.

### Phase 3 - Find Session Or History Evidence

Search for:

- the exact document name
- the document title
- unique phrases from the document
- the flow name
- related user request language

Classify found material as one of:

- creation evidence
- candidate history evidence
- later use
- unrelated mention

Only call something creation evidence if it clearly shows the document being
requested or produced.

If no creation transcript is found, say so directly.

Do not invent a source session.

### Phase 4 - Read The UML As The Mapping Frame

Extract the UML nodes and edges.

Record every node.

Record every edge.

Do not collapse visual support nodes unless the output clearly says they are
support nodes.

The edge ledger is required because training sets must teach transitions, not
only node meanings.

Each edge row should answer:

- what moves from the source node
- what the target node receives
- what the target node must produce
- what would break if this edge is missing

### Phase 5 - Build The Source-Line Ledger

Create one ledger row for every source document line.

Each row must include:

- row id
- source file
- source line number
- original source text
- document section
- node being taught
- edge being preserved
- input this node receives
- output this node must produce
- reason the line belongs there

The original source text must remain visible.

Blank lines and separators get rows because they teach boundaries.

### Phase 6 - Build Per-Node Training Sets

Create a folder for this document's sets.

Inside it, create one file per node:

- `00-INDEX.md`
- `node-00-logical-structure-compiler-set.md`
- `node-01-domain-splitter-set.md`
- `node-02-domain-translator-set.md`
- `node-03-scheme-planner-set.md`
- `node-04-domain-processor-set.md`
- `node-05-applier-set.md`
- `node-06-result-translator-set.md`
- `node-07-arbiter-selector-set.md`
- `node-08-arbiter-units-set.md`
- `node-09-formatter-set.md`
- `10-SIMULATION-CHECK.md`

Each node file must contain rows shaped like this:

```text
ROW ID
Source
Source section
Original source text
A received by this node
B this node must produce
Edge this row prepares
Why this row belongs here
```

The row is allowed to add explanation.

The row is not allowed to shorten away the source text.

### Phase 7 - Simulate The Trained Flow

After the sets are created, simulate the flow as if the node LLMs were trained
only on those sets.

The simulation must answer:

1. What domains would the splitter emit?
2. What domain-language prompts would the translator array create?
3. What planned pieces would the scheme planner create?
4. Which pieces run as parallel processor units?
5. What results would the applier array produce?
6. What human-readable fragments would the result translator array produce?
7. Which arbiters would the arbiter selector choose?
8. What would arbiters pass or send back?
9. What final answer would the formatter produce?
10. What is missing?

The missing-parts section is required.

Missing teaching material is not a failure. Hidden missing teaching material is
a failure.

## Per-Node Teaching Guidance

### User Prompt

This node learns the subject.

It receives raw human intent or raw document identity.

It produces the basic subject the flow is about.

### Domain Splitter

This node learns which territories appear in the source.

It receives source text.

It produces an array of domain labels.

One source line may produce several labels.

### Domain Translator Array

Each translator receives:

- one selected domain
- the source text belonging to that domain

It produces clear domain-language work.

This is not a summary. It is a translation into the kind of language that later
nodes can use.

### Scheme Planner

This node receives the domain-language material.

It produces planned pieces:

- ordered pieces
- parallel pieces
- wait pieces
- recovery pieces
- review pieces

The planner must not hide waits, branches, or recovery work.

### Domain Processor Array

Each processor receives one planned piece.

It produces one processed requirement.

This is an array. Do not turn it into one large processor.

### Applier Array

Each applier receives one processed requirement and the transformation needed
for that requirement.

It produces one domain result.

This is also an array.

### Result Translator Array

Each translator receives one domain result, while seeing the full result
collection.

It produces one human-readable result fragment.

This is also an array.

### Arbiter Selector

The selector receives all results.

It chooses which reviewers are needed.

It is separate from the result translator.

### Arbiter Units

Each arbiter receives:

- one review responsibility
- one result or result group

It produces:

- pass
- concern
- reprocess instruction

Rejected examples can be derived from source rules, but they must be marked as
derived.

### Formatter

The formatter receives approved translated result fragments.

It produces one user-readable final answer.

It must not receive unchecked raw results.

## Version And History Rules

Versions and history are not optional.

Each examination must say:

- what versions were found
- what changed between versions
- what session or history evidence was found
- whether creation evidence was found
- whether history evidence is uncertain

If the creation session is not found, the correct statement is:

`Exact creation transcript was not found.`

Do not fill the gap by guessing.

## Validation Checklist

A completed set batch passes only if:

- every source line appears exactly once as a primary source row
- no source line is missing
- no source line appears twice as primary source
- history rows are marked as history
- derived rows are marked as derived
- every node has its own file
- every file states the edge it preserves
- array nodes remain arrays
- result translator and arbiter selector are separate
- arbiters happen before final formatter
- formatter receives approved translated fragments only
- simulation exists
- missing-parts section exists

## Output Naming

Use this pattern:

```text
<document-name>-training-sets-v1/
  00-INDEX.md
  node-00-logical-structure-compiler-set.md
  node-01-domain-splitter-set.md
  node-02-domain-translator-set.md
  node-03-scheme-planner-set.md
  node-04-domain-processor-set.md
  node-05-applier-set.md
  node-06-result-translator-set.md
  node-07-arbiter-selector-set.md
  node-08-arbiter-units-set.md
  node-09-formatter-set.md
  10-SIMULATION-CHECK.md
```

If the source document is Flow 01 user registration, the folder is:

```text
flow-01-training-sets-v1/
```

## What To Do After One Document

Stop after one document.

Review the produced sets.

Only after review, choose the next source document.

The point is to build a trustworthy method, not to rush through the corpus.
