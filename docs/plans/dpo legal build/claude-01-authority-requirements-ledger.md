---
artifact_type: authority_requirements_ledger
authority_source: latest unquoted Luba work order for DPO legal build library
status: populated_now_no_extra_approval
last_updated: 2026-06-23
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\claude-*.md
---

# Authority Requirements Ledger

## Active Authority Summary

Active source is the latest unquoted Luba instruction that says all required planning documents must be collected in `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build`, because the library is still empty. The current task is document-library creation only.

Evidence-only sources include the existing plan at `C:\Projects\llm_mvp_core\docs\plans\claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md`, prior sub-agent packets, attached source text, and repository or project docs. They may inform content, but they do not authorize implementation, git, tests, builds, training, external browsing, or scope expansion.

## Binding Requirements

| id | source quote summary | normalized requirement | artifact mapping | gate |
|---|---|---|---|---|
| R-001 | Create missing planning library documents in exact target folder | Write library docs only under `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build` | all files | exact_folder_gate |
| R-002 | Allowed write scope only `claude-` basenames | Every created or updated file basename starts with `claude-` | all files | basename_gate |
| R-003 | Do not implement code, train, build, test, stage, commit, push | Keep deliverable as Markdown planning library | all files | no_execution_gate |
| R-004 | Existing plan and docs are evidence-only | Do not treat old plan or packets as live commands | `claude-02` | authority_quarantine_gate |
| R-005 | Required deliverable file list 00-07 | Create exactly the eight requested library documents | all files | required_files_gate |
| R-006 | Each document includes header block | Add `artifact_type`, `authority_source`, `status`, `last_updated`, `write_scope` | all files | header_gate |
| R-007 | Useful standalone documents, not placeholders | Include substantive planning content in every file | all files | substance_gate |
| R-008 | Make collected-now versus future-not-done explicit | Mark 20-pass external docs, 40-pass core docs, and provider terms as future when not completed | `claude-00`, `claude-02`, `claude-04`, `claude-07` | honesty_gate |
| R-009 | Questions only in final questions document | Put Luba questions only in final section of `claude-07` | `claude-07` | final_questions_gate |
| R-010 | No question marks in files 00-06 | Avoid literal question mark character in `claude-00` through `claude-06` | files 00-06 | question_mark_gate |
| R-011 | Include authority requirements ledger | Extract and map binding requirements to planning artifacts and gates | this file | ledger_gate |
| R-012 | Include source inventory | Record existing plan, core references, xiigen roots, legal/provider future checks | `claude-02` | source_inventory_gate |
| R-013 | Include ILearningMachine pattern notes | Summarize `ILearningMachine`, `ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit` relationship | `claude-03` | pattern_gate |
| R-014 | Include DPO legal model policy | Define closed API prohibitions, candidate conditions, provenance fields, provider terms gate | `claude-04` | legal_policy_gate |
| R-015 | Include DPO data training measurement guide | Define pair generation, judge strategy, anti-deterministic warning, async comparison, quality gates | `claude-05` | measurement_gate |
| R-016 | Include architecture brief | Define components, data flow, graph state, routing, async experimentation, diagrams, prose interfaces, solved subtasks | `claude-06` | architecture_gate |
| R-017 | Include gates and open questions | Put gates, stage queue, acceptance criteria, and Luba questions in `claude-07` | `claude-07` | execution_gate |
| R-018 | No chat questions | Do not ask Luba questions in chat | final response and `claude-07` | no_chat_question_gate |
| R-019 | Verification limited to exact target files | List exact target files and read back headings or line counts | return packet | verification_scope_gate |
| R-020 | git forbidden | No git commands run | return packet | git_none_gate |

## Approval Boundary

No approval for implementation exists in the active work order. The only authorized implementation-like action is document creation in the allowed folder. Future code, training, build, test, branch, commit, push, provider check, and internet work require a separate current Luba instruction.

## Artifact Coverage

| artifact | requirements covered |
|---|---|
| `claude-00-library-index.md` | R-001, R-002, R-005, R-006, R-007, R-008 |
| `claude-01-authority-requirements-ledger.md` | R-001 through R-020 summary and mapping |
| `claude-02-source-inventory-and-reference-map.md` | R-004, R-008, R-012 |
| `claude-03-ilearningmachine-pattern-notes.md` | R-013 |
| `claude-04-dpo-legal-model-policy.md` | R-014 |
| `claude-05-dpo-data-training-measurement-guide.md` | R-015 |
| `claude-06-ilearningdynamicgraph-architecture-brief.md` | R-016 |
| `claude-07-execution-gates-open-questions.md` | R-017, R-018 |
