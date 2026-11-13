# Redaction note

The row12 workload is synthetic no-send text from row12-workload-spec.json and contains no private prompt material from Luba.

The harness marks prompt and answer fields as redacted in token traces. Raw baseline/adaptive response JSON files are present because the workload is synthetic and required for local validation; no external AI call was made by this measurement packet.

No secrets were captured intentionally. If later host-visible Cursor evidence is collected, it must repeat this redaction boundary and avoid capturing credentials, account UI, or private workspace content outside the synthetic marker scenario.
