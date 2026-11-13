# Redaction Note

- No external AI/web call was used for this packet.
- No prompt was sent to Cursor chat or external AI.
- The intended full row12 prompt exists inside `row12-host-visible-command-runner.test.js` and the generated runtimeProof request JSON. This is expected by request-payload contract.
- The full prompt was not observed in the generated core-response JSON.
- The required host trace/redacted trace files were not produced, so a complete redaction pass over the full host-visible trace packet could not be claimed.
- The marker and prompt hash are recorded because they are required evidence keys:
  - marker: `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`
  - prompt hash: `sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374`
- No token savings, semantic quality, readiness, training, DPO, or checkpoint mutation claim is made.
