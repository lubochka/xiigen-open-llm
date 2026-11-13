# Internal Inventory Evidence Handling - 2026-05-11

This note covers the raw JSON inventory and uncovered-type files in this folder.

These files are evidence snapshots:

- `XIIGEN-REGISTRY-UNCOVERED-DOCUMENT-TYPES-ADDENDUM-2026-05-08.json`
- `XIIGEN-RECURSIVE-ZIP-INVENTORY-DEPTHFIRST-2026-05-08.json`
- `XIIGEN-NESTED-ZIP-ENTRIES-2026-05-08.json`
- `XIIGEN-FULL-FILES-AND-ZIP-INVENTORY-2026-05-08.json`
- `XIIGEN-REGISTRY-UNCOVERED-DOCUMENT-TYPES-2026-05-08.json`
- `uncovered-document-types.json`

Do not rewrite these snapshots as if they were current registry truth. They are
inputs to model-led adjudication.

## Correct Use

Use the JSON files to answer mechanical questions:

- which files existed,
- which ZIP members existed,
- which preliminary uncovered buckets were observed,
- which examples were attached to a candidate family,
- which scan method produced the evidence.

Do not use them alone to decide:

- whether a document is train-ready,
- whether a user request is low-authority,
- whether a file is new type vs new version,
- whether a source is canonical or stale,
- whether a row should enter DPO training.

Those decisions require the registry guide, examination method, and model-led
reading of the document content and surrounding session/ZIP context.

## 2026-05-11 Run Fix

Today's run showed a concrete failure mode: deterministic scripts and inventory
surfaces can demote root prompt/user-intent documents because they do not look
like ordinary canonical documentation. That is wrong for XIIGen.

When an inventory row points to a user request, `basic_prompt.txt`, session start
prompt, flow initiation description, or genesis prompt/response document:

1. Preserve it as a possible prompt-authority source.
2. Route it to `model-led-authority-candidates.jsonl`.
3. Use `XIIGEN-DPO-REGISTRY-GUIDE-v1.md` to decide its family and lifecycle.
4. Build pairs only after the model has identified producer, consumer,
   canonicality, and DPO behavior.

## Training Gates

Any pair derived from these inventories must still pass:

```text
chosen != rejected
rejected contains [WRONG: ...]
no mojibake in prompt/a_side/chosen/rejected
source span recorded
canonicality/version state explicit
ledger/status/audit/inventory rows excluded from DPO rows
16-64 row standalone ablation before combo promotion
```

Raw inventory JSON is evidence. It is not a replacement for examination.
