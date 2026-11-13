# 00 - Index

## Source Document

One source document was used for this package:

`C:\Users\lubao\Documents\xiigen\business flows\04-post-publishing.md`

Source facts recorded for this examination:

- Title: Post Publishing & Feed Distribution
- Flow ID stated by the document: the event attendance flow
- Version stated by the document: 1.0
- Last updated stated by the document: 2026-02-25
- File size: 21,621 bytes
- Line count: 202 lines
- SHA256: E0B599CEF44E5B7E9172293B9E40A9FED1AD1B4DEE9F7CE54B2FD405E6C6F5D9
- First meaningful line: `# Post Publishing & Feed Distribution`
- Last meaningful line: `* **Elasticsearch Integration**: Utilize existing datastore capabilities for high-performance audience matching and topic extraction.`

This document was chosen because it is the active business-flow source specification for post publishing and feed distribution. Related files were recorded, but only this document supplies the training pairs in this package.

## Related Versions Found

Current duplicates of this source:

- `C:\Users\lubao\Documents\xiigen\business-flows\business flows\04-post-publishing.md`
- `C:\Users\lubao\Documents\xiigen\old\04-post-publishing.md`

Older same-name versions found:

- `C:\Users\lubao\Documents\xiigen\.claude\worktrees\adoring-davinci-058d7f\business flows\04-post-publishing.md`
- `C:\Users\lubao\Documents\xiigen\.claude\worktrees\adoring-davinci-058d7f\business-flows\business flows\04-post-publishing.md`
- `C:\Users\lubao\Documents\xiigen\.claude\worktrees\adoring-davinci-058d7f\old\04-post-publishing.md`

Deep-search documents with the same subject were also found. They are related material, not sources for this package:

- `C:\Users\lubao\Documents\xiigen\business flows\04-post-publishing deep search.md`
- `C:\Users\lubao\Documents\xiigen\business-flows\business flows\04-post-publishing deep search.md`
- `C:\Users\lubao\Documents\xiigen\old\04-post-publishing deep search.md`
- worktree copies under `.claude\worktrees\adoring-davinci-058d7f`

Observed differences between the active file and the older same-name worktree file:

- The active file uses the current name "the event attendance flow" in the flow metadata and later architecture mapping.
- The active file includes the business-flow-arbiter wording used by the later platform architecture.
- The active file adds later integration language for ranking-service, feed-reorder execution, Redis queue throughput, and Elasticsearch audience matching.
- The older same-name file is shorter and should be treated as a prior version, not as part of this package's training source.

## Session and History Evidence

Search phrases used:

- `Post Publishing & Feed Distribution`
- `04-post-publishing`

Creation evidence was not confirmed. The strongest related evidence is the older worktree source file and its deep-search companion under `.claude\worktrees\adoring-davinci-058d7f`. Later-use evidence appears in architecture and catalog documents such as `12-erp systems ENGINE_ARCHITECTURE.md`, `TASK_TYPES_CATALOG.md`, `UNIFIED_SOURCE_INDEX.md`, and `V62_BFA_STRESS_TEST.md`, where Flow-04 is referenced after the source document already exists.

## Domains This Document Teaches

The source document defines these runtime domains:

- post creation and visibility
- content analysis and moderation
- business audience matching
- social-connection matching
- group membership matching
- recipient list compilation
- composite ranking
- feed tier placement
- feed diversity and reordering
- distribution analytics
- notification for mentioned users
- privacy and safety enforcement
- high-volume batching and recovery

## Package Files

- `01-UML-FRAME.md` records the dynamic-flow node and edge frame used for this package.
- `node-01-domain-splitter-set.md` trains raw user prompt -> domain labels.
- `node-02-domain-translator-set.md` trains domain-labeled prompt fragment -> domain wording.
- `node-03-scheme-planner-set.md` trains all domain wordings -> ordered and parallel work pieces.
- `node-04-domain-processor-set.md` trains one work piece -> complete processed result.
- `node-05-applier-set.md` trains one processed piece -> concrete action and outcome.
- `node-06-result-translator-set.md` trains one formal result -> user-readable result.
- `node-07-arbiter-selector-set.md` trains formal results -> selected review units.
- `node-08-arbiter-units-set.md` trains selected review responsibility -> pass, concern, or reprocess instruction.
- `node-09-formatter-set.md` trains approved translated fragments -> one finished answer.
- `10-SIMULATION-CHECK.md` tests whether the nine node sets can recreate the document's intended flow.

## Examination Boundary

The training pairs below do not use document headers, metadata, blank lines, or table formatting as runtime inputs. Those items are evidence for this package only. Runtime A values are written as the kind of human request or previous-node output the node would actually receive.
