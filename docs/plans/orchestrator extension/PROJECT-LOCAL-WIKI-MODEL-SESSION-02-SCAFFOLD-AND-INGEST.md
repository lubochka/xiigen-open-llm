# SESSION-02 - Project Wiki Scaffold And Accepted Documentation Ingest

Status: EXECUTION SESSION.

Do not start this session until SESSION-01 has passed its stop gate.

## Goal

Run one orchestrated documentation/wiki request for a selected project, create
the project code trace, and persist optional snapshots only after the graph is
accepted. Recommended first project: `C:\Projects\askapediaweb`.

## Required Reads

1. `PROJECT-LOCAL-WIKI-MODEL-PLAN-v1.md`
2. `PROJECT-LOCAL-WIKI-MODEL-STATE.json`
3. `PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md`
4. `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md`
5. SESSION-01 acceptance report
6. Latest documentation quality summary for the selected project
7. `C:\Projects\llm_mvp_core\12.SearchEngine` contracts for `SearchEngineFacade`
8. `C:\Projects\llm_mvp_core\42.ProjectWikiDomain\ProjectDocumentationPipelineFlowAdapter.cs`
9. `C:\Projects\llm_mvp_core\15.OrchestratorModel\Logical\LogicalOrchestratorAssistant.cs`
10. `EXECUTOR-STAGE-PROMPT-v2.md`
11. `EXECUTOR-DISCIPLINE-RULES-v1.md`
12. `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md`

## Required Entry Point

This session must start from:

```text
LogicalOrchestratorAssistant.RunAsync(
  "Document this .NET project, train its local wiki, and register it for project questions.",
  ProjectDocumentationPipelineFlowAdapter.ScenarioId,
  metadata: { project_root, wiki_domain_root, documentation_version, persist_artifacts, verify_coverage })
```

Directly creating `.xiigen/wiki-domain/` without this entry point does not pass
Stage 0.

## Outputs

The primary output is a project code trace plus an accepted documentation graph.
Persisted files are snapshots of that graph, not the product boundary.

Inside the selected project:

```text
.xiigen/wiki-domain/
  domain-manifest.json
  model-manifest.json
  documentation/
  evidence/evidence-index.jsonl
  relations/documentation-units.jsonl
  relations/call-edges.jsonl
  relations/value-flow-edges.jsonl
  eval/rejected-documentation-ledger.jsonl
```

Required in-memory result:

```text
ProjectCodeTrace
ProjectDocumentationKnowledgeGraph
```

Phase boundary rule:

- Do not create or update `question-bank/` in this session.
- The question database belongs to Phase 2 only.
- This session creates Phase 1 documentation evidence and relation graph rows.
- No question rows, question seeds, or Q/A training batches are produced here.

## Documentation Acceptance Gate

Only ingest documentation that passes:

```text
documentation_quality_score >= 0.95
stack_version_score == 1.00
source_evidence_score == 1.00
bottom_up_propagation_score == 1.00
workflow_use_case_score == 1.00 for workflow docs
feature_boundary_score == 1.00 for feature docs
system_boundary_score == 1.00 for system docs
hard_failure_count == 0
```

Accepted documentation must also be extractable into the Phase 1 relation
contract: symbol, meaning, location, accepted parameters, callers, returns, and
for caller/workflow docs, called functions with argument and return-value
usage.

Rejected documentation goes to `eval/rejected-documentation-ledger.jsonl` and
must not be used for wiki training.

## Mode 3 Evidence Index Contract

The `evidence-index.jsonl` scaffold is the source for Mode 3 deterministic
evidence lookup.

- Use `SearchEngineFacade` from `12.SearchEngine` for BM25 lexical lookup.
- Do not describe this as vector search.
- Do not require `EmbeddingModel` for Mode 3.
- The index rows must include enough fields for citation recovery:
  `evidence_id`, `project_id`, `documentation_version`, `doc_path`,
  `source_path`, `source_line`, `symbol_id`, `relation_symbol_id`,
  `target_framework`, and `csharp_language_version`.

## Source Consent Gate

Record `source_authority` in `domain-manifest.json` before ingest:

- `first_party_internal`
- `customer_approved`
- `local_validation_only`
- `blocked_no_consent`

If the value is `blocked_no_consent`, stop.

## Stop Gate

Stop after the project code trace and accepted documentation graph exist and
the optional scaffold/evidence snapshots are written.

Do not generate training preference rows or question rows in this session.


