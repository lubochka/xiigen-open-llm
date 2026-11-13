# Redaction log

## Prompt

The scenario prompt is the synthetic Russian scenario prompt from the Claude prep JSON. It was used for the corrected direct core API rerun. No secret prompt was used.

## Window inventory

Raw window titles were not persisted. The inventory stores only process name, process id, host-keyword match boolean, and a redacted title marker.

## Screenshots

No screenshots were created in this run. The local route was not proven by safe preflight, so I did not capture the visible Claude window.

## Files

`request.json`, `response.json`, and `trace.json` are the corrected Claude-host direct core API artifacts. `trace.json` reports `raw_private_prompt_persisted=false`, `contains_secrets=false`, `external_ai_used=false`, and `training_started=false`.
