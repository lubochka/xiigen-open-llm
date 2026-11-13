# Cursor clean host preflight

Scope: one Cursor Path B clean-host preflight for scenario coverage only. No prompt was entered or submitted.

Scenario: cursor-full-extension-runtime-after-unlock-20260627-090159.
Exact prompt source: docs/plans/evidence/extension-full-readiness-dual-path/scenario-prep/cursor-full-extension-runtime-after-unlock-20260627-090159/scenario.json.
Exact prompt SHA-256: 83E2BF43EE362C5EC44E42CD99717BE03BB9D053E9E50E7FA7922FAAC2421990.

Outcome: launched clean Cursor Extension Development Host with fresh test-only storage under this packet folder.
Sanitization: generated runtime storage directories were removed before commit because they may contain cache, cookies, local storage, machine ids, sqlite state, and logs. See runtime-storage-manifest.json.

Storage Error: absent in corrected top-level window inventory.
Target title: [Extension Development Host] cursor-xiigen-smart-skills - Cursor
Input route: not visible. The target screenshot shows Cursor login/signup gate, so there was no unambiguous XIIGen Smart Skills prompt route.
Path B: blocked_login_required_after_clean_launch. This is partial preflight evidence, not accepted Path B evidence.

Files:
- launch-command.json
- launch-stdout.log
- launch-stderr.log
- process-inventory.json
- runtime-storage-manifest.json
- window-inventory.json
- target-window.png
- ui-summary.md
- redaction-log.md
- result.json
- verification.json

Parent recommendation: review this evidence folder and commit it if accepted. This sub-agent did not commit or push.
