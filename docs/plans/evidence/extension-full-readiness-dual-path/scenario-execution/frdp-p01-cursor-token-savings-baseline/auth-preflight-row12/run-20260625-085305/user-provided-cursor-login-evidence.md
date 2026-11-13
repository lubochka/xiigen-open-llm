# Row12 User-Provided Cursor Login Evidence

status: `SESSION_SIGNED_IN_READY`

scenario_id: `frdp-p01-cursor-token-savings-baseline`

marker: `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`

## Source

Luba provided two screenshots in the current conversation.

Raw screenshots were not copied into the repo. The second screenshot contains an account identity, so this packet records a privacy-safe summary only.

Privacy handling: account email visible to Luba, redacted in repo evidence. No passwords, tokens, cookies, or secrets are recorded.

## Screenshot Findings

1. First screenshot: evidence of the old scenario defect. The account command/input was not reliable; `Accounts` landed in the wrong UI/text area. This screenshot is not auth/session evidence.
2. Second screenshot: user-provided visual evidence shows Cursor Settings > Cursor Account with a signed-in account and Pro Plan. The account email is redacted here.

## Determination

The second screenshot is sufficient to record row12 auth/session preflight as `SESSION_SIGNED_IN_READY`.

This evidence supports only the auth/session state. It is not product readiness, not full Cursor readiness, not row12 UI proof, not token savings, not semantic quality, not training, not checkpoint promotion, and not locator update.

## Next Smallest Action

Panel input/submit discovery for row12, not login recovery.
