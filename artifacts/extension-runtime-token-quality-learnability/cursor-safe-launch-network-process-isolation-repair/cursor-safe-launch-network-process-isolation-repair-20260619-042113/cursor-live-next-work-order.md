# CURSOR-LIVE-EVIDENCE-03

## Authority / Trace

- Requested artifact type: Cursor live evidence or safe defer packet after isolation preflight.
- Current source authority: Luba current work order `CURSOR-SAFE-LAUNCH-NETWORK-PROCESS-ISOLATION-REPAIR-03`.
- Prior packet: `cursor-safe-launch-preflight-or-defer-20260619-041343`.
- Isolation repair packet: `cursor-safe-launch-network-process-isolation-repair-20260619-042113`.
- Helper: `tools/cursor-safe-launch-isolation-preflight.ps1`.
- Git authority: forbidden for artifact/code/content sub-agent; parent performs commit/push after review.

## Goal

Run the next bounded Cursor phase only if the isolation gate passes in the same run. The phase may produce a live Cursor launch evidence packet, but it must not send prompts to Cursor/Copilot/AI/model surfaces. If isolation cannot be proven before launch, it must immediately produce fallback work order `CURSOR-TEST-ONLY-CHAT-AUTOMATION-ADAPTER-03` and not launch Cursor.

## Required First Gate

1. Run helper dry-run with the exact paths from `cursor-isolation-contract.json`.
2. Run helper with `-EnableFirewallProbe` and `-LaunchCursor:$false`.
3. PASS only if temporary firewall rule is created, verified, removed, and verified removed in the same run.
4. Start process/network monitor before any Cursor launch.
5. Confirm monitor cmdlets are available: `Get-CimInstance` and `Get-NetTCPConnection`.
6. Confirm no existing unrelated Cursor process tree is mixed into the proof.

## Launch Permission

Cursor launch is allowed only after the first gate passes. Launch must use the exact command line from `cursor-isolation-contract.json`, isolated user-data/extensions dirs, temporary network deny rule, same-run process tree capture, same-run network snapshots and cleanup. No prompt/chat/model/Copilot action is allowed in this work order.

## PASS Conditions

- Active temporary deny rule exists before launch and is removed after cleanup.
- Same-run process tree packet captures root and children from launch through cleanup.
- Same-run network monitor shows zero successful outbound connections from the proof process tree.
- stdout/stderr/log scan has zero unapproved backend/model metadata risk hits.
- Any blocked backend/model metadata attempt is quarantined and does not become readiness proof unless explicitly accepted by a later work order.
- Cleanup touches only timestamped isolated dirs and the proof process tree.

## Auto-Fallback

If admin is missing, firewall probe fails, monitor is unavailable, process tree capture is incomplete, or Cursor attempts successful outbound networking, do not send prompts and do not continue live proof. Produce `CURSOR-TEST-ONLY-CHAT-AUTOMATION-ADAPTER-03` with:

- exact failure evidence;
- safe test-only adapter/core-boundary scope;
- no Cursor UI/chat readiness claim;
- no token/quality/learnability/final readiness claim;
- next needed isolation/sandbox repair.

## No-Overclaim Rules

This work order cannot claim token savings, quality improvement, learnability, DPO/lifecycle readiness, target promotion, locator update, final readiness or product readiness. A passed launch-only packet can only say that one network-isolated Cursor launch boundary was observed under the named command and run id.
