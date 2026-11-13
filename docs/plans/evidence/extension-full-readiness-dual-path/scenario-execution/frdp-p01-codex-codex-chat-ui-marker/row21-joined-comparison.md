# Row21 joined comparison

## Result

Row21 is accepted only as scoped blocker evidence for
`frdp-p01-codex-codex-chat-ui-marker`. It is not a formal joined pass and does
not make a product readiness claim.

## Path A

Path A provides supporting entrypoint/adaptor/core-boundary evidence for the
Row21 marker. It is supporting proof only. It is not Codex UI proof and does not
prove host readiness, product readiness, token savings, semantic quality,
learnability/training, checkpoint promotion, or a locator update.

## Path B

Path B stopped before input with the exact blocker
`UI_INPUT_CONTROL_NOT_FOUND`. The Codex Desktop window candidate was found, but
UIA control view and raw UIA view did not expose a confirmed composer/input
control.

The broad RootWebArea was rejected as unsafe because it was whole-window sized,
not a confirmed input control.

Safety result:

- no blind input
- prompt not typed, pasted, or sent
- clipboard not used

## Claim Boundary

This joined comparison supports only:

- Row21 Path A supporting entrypoint/adaptor/core-boundary evidence
- Row21 Path B exact blocker `UI_INPUT_CONTROL_NOT_FOUND`

It does not prove:

- Codex UI route pass
- visible marker in Codex UI
- product readiness
- full Codex UI readiness
- token savings
- semantic quality
- learnability/training
- checkpoint promotion
- locator update

## Next Action

Focus on a clean marker-only Codex Desktop/UI input surface or a confirmed
composer/input locator before any paste/send. If that cannot be established
safely, preserve the exact blocker.

## Final Screenshot Redaction

The final retained preflight screenshot is opaquely redacted after visual
validation. An initial semi-transparent overlay was found insufficient during
validation and was overwritten before final artifact validation. No input was
performed.
