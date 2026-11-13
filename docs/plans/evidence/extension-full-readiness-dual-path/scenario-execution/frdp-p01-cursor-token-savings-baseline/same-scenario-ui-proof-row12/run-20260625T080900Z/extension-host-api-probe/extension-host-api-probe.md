# Cursor Extension Host API Probe

status: API_PROBE_EXTENSION_AND_COMMAND_PRESENT
run_folder: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z
extension_id_checked: xiigen.cursor-xiigen-smart-skills

## API Probe Result

- extension_present: True
- activation_attempted: True
- activation_succeeded: True
- target_command: xiigenSmartSkills.panel
- target_present_before_activation: False
- target_present_after_activation: True

## Diagnosis

Cursor/VS Code extension-host API visibility is positive: vscode.extensions.getExtension('xiigen.cursor-xiigen-smart-skills') is present, activation succeeded, and vscode.commands.getCommands(true) contains xiigenSmartSkills.panel after activation. In this same probe, the command was not present before explicit activation, so the previous command-palette miss is not explained by package id absence or registration absence after activation.

## What Was Proven

vscode.extensions.getExtension('xiigen.cursor-xiigen-smart-skills') was present; activation attempted=True, succeeded=True; getCommands(true) target before activation=False, after activation=True.

## What Is Not Proven

This does not prove command-palette visibility, manual UI panel opening, row12 prompt submit, browser/login state, token savings, semantic quality, or product readiness.

## Human Explanation

The development extension is visible to the host API and the panel command is present in the command registry after activation. This narrows the remaining blocker away from manifest/source/dist visibility and toward command-palette/UI discovery timing/filtering or needing an activation-triggered UI proof split.

## Next Smallest Action

Run a narrow UI proof split that first triggers safe activation without executing xiigenSmartSkills.panel, then rechecks command-palette visibility for XIIGen: Open Smart Skills Panel. Do not run row12 submit.
