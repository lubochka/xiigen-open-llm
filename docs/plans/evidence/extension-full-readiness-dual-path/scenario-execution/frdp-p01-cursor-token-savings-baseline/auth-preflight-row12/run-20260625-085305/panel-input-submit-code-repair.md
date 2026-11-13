# Cursor Panel Input Submit Code Repair

## Scope

This evidence covers exactly two Cursor panel input+submit code review findings: the webview metadata trust boundary and the CSP nonce randomness source.

It does not run row12 UI proof, does not open or login to Cursor, does not use external AI, and does not claim product readiness, token savings, semantic quality, training, or full host UI proof.

## Findings Fixed

- P1 webview metadata trust boundary: panel submit no longer spreads or trusts `message.metadata`. The extension host rebuilds submit input with `buildTrustedPanelSubmitInput(panelInput, submittedPrompt)`, which whitelists only trusted extension-side `scenario`, trusted extension-side `correlationId`, and the submitted prompt. Webview-supplied `evidenceRoot` and path-like fields are ignored.
- P2 cryptographic CSP nonce: `createNonce()` now uses Node `crypto.randomBytes(24)` and base64url-compatible character replacement. `Math.random()` is no longer used for nonce generation.

## Changed Files

- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\src\extension.ts`
- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\dist\extension.js`
- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\auth-preflight-row12\run-20260625-085305\panel-input-submit-code-repair.md`
- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\auth-preflight-row12\run-20260625-085305\panel-input-submit-code-repair.json`

## Commands Run

```text
node --check "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\dist\extension.js"
rg -n "Math\.random|createNonce|crypto\.randomBytes|message\.metadata|evidenceRoot|buildTrustedPanelSubmitInput|\.\.\.\(message\.metadata" "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\src\extension.ts" "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\dist\extension.js"
$files = @("C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\src\extension.ts","C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\dist\extension.js"); $matches = rg -n "message\.metadata|Math\.random|\.\.\.\s*\(?message\.metadata" @files; if ($LASTEXITCODE -eq 0) { $matches; exit 1 } elseif ($LASTEXITCODE -eq 1) { "no message.metadata spread/trust or Math.random matches" } else { exit $LASTEXITCODE }
Select-String -Path "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\src\extension.ts","C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\dist\extension.js" -Pattern "function buildTrustedPanelSubmitInput|scenario: panelInput\.scenario|correlationId: panelInput\.correlationId|prompt: submittedPrompt|crypto\.randomBytes" -Context 0,3
npm run install-smoke
node -e "JSON.parse(require('fs').readFileSync('C:\\Projects\\llm_mvp_core\\docs\\plans\\evidence\\extension-full-readiness-dual-path\\scenario-execution\\frdp-p01-cursor-token-savings-baseline\\auth-preflight-row12\\run-20260625-085305\\panel-input-submit-code-repair.json','utf8')); console.log('json parse ok')"
```

## Verification Results

- `node --check` passed for `dist\extension.js`.
- `npm run install-smoke` passed.
- Targeted grep found `buildTrustedPanelSubmitInput` at the panel submit call and helper in both source and dist.
- Targeted grep found `crypto.randomBytes(24)` in both source and dist.
- Targeted no-match grep found no `message.metadata` spread/trust and no `Math.random()` in source/dist.
- Evidence JSON parse passed after the JSON packet was written.

## Remaining Risks

- Shared trusted command/runtime input still supports `evidenceRoot`; this repair intentionally changed only the webview panel submit boundary.
- This is code, dist syntax, smoke, targeted text, and evidence-parse verification only. Row12 interactive Cursor UI proof was not run by instruction.

## Next Smallest Action

Parent review should inspect this bounded two-finding repair and evidence packet. Row12 UI proof remains a separate explicitly authorized slice.
