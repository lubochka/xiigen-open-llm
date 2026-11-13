# Phase 04.05 - Claude/Codex Route Establishment Fix-Loop

CURRENT TASK:
Phase 04.05 fix-loop for Claude/Codex route-establishment. Reviewer found Claude route candidate overclaimed safety: `--name` help proves display-name separate from positional prompt, but does not prove no external-service send on future launch. Fix the artifacts by either safely proving that claim without running a real probe or by downgrading Claude to blocker/uncertain.

active_task_sentence_source: Luba current unquoted instruction: "CURRENT TASK: Phase 04.05 fix-loop for Claude/Codex route-establishment. Reviewer found Claude route candidate overclaimed safety..."

NOT THE TASK:
Do not run real Claude/Codex marker probes. Do not send prompts/messages. Do not click/type into external-service chat. Do not launch a command that may send a prompt or connect as a real service action. Do not capture private UI screenshots. Do not mutate model/training/checkpoint/locator. Do not claim readiness, `PASS_LOCAL_DESKTOP`, `PASS_JOINED`, token savings, semantic quality, or learnability. Do not stage/commit/push/switch branches.

EVIDENCE-ONLY:
Existing Phase 04.05 artifacts, prior local `--help` evidence, Appx/package metadata, and the reviewer finding were used only to repair the route-establishment claim. Prior `claude --help` output proves only option semantics: `--name` is separate from the positional prompt. It does not prove that a future non-help Claude launch is local-only or no-send.

## Human-Readable Status

Фаза 5 из 17: Local desktop proof. Смысл фазы: проверить реальные локальные host-сценарии на этом компьютере без подмены доказательств документацией или dashboard и записать точный blocker, когда безопасный локальный путь не доказан.

Шаг 04.05: Claude/Codex route-establishment fix-loop. Смысл шага: убрать overclaim вокруг Claude `--name`, сохранить только доказанные факты и оставить Claude/Codex без real marker probe до отдельного безопасного route proof.

Статус: DONE. Fix-loop завершен: Claude финально понижен до `BLOCKED_WITH_REASON`, потому что нет безопасного статического доказательства, что будущий `claude --name` launch будет local-only/no-send; Codex остается `BLOCKED_WITH_REASON`.

Сделано:
- Claude statuses updated: 1 final `BLOCKED_WITH_REASON`, 0 `ROUTE_CANDIDATE_REQUIRES_REVIEW`.
- Codex statuses updated: 1 final `BLOCKED_WITH_REASON`.
- Removed unsupported Claude no-send design claim from route packet and rolling local-desktop packet.
- Reframed prior `--help` evidence as display-name/positional-prompt separation only, not as future launch safety proof.
- Recorded the reviewer finding and this fix-loop in state/log.

В процессе:
- Nothing is running for Claude/Codex route establishment, screenshots, prompts/messages, external-service input, readiness probes, model/training/checkpoint/locator mutation, git staging, git commit, git push, or branch switching.

Осталось:
- A future safe sandbox/manual review may establish a Claude route only if it proves marker display without external-service send and without private UI exposure.
- Codex needs a separate safe route decision before any marker probe.
- No real marker probe may start from this fix-loop.

Review нашел:
- Claude route candidate overclaimed safety. The existing `--name` help evidence proves only that the display-name option is separate from the positional prompt; it does not prove that a future `claude --name` launch is local-only/no-send.

Что исправлено:
- Claude no longer uses `ROUTE_CANDIDATE_REQUIRES_REVIEW` as the final 04.05 status.
- Claude is now `BLOCKED_WITH_REASON` with exact reason: no proof that `claude --name` launch is local-only/no-send.
- Codex remains `BLOCKED_WITH_REASON`; no new safe static evidence was found or claimed.
- Unsupported strong boolean/no-send-by-design wording is removed from current packets.

Следующее действие:
- Review this 04.05 fix-loop packet. Future work must first establish a safe sandbox/manual route; do not run Claude/Codex marker probes from the current evidence.

## Host Route Decisions

| Host | Final 04.05 route status | Safe local marker-only unsent route proven? | Exact reason | Next action |
|---|---|---:|---|---|
| Claude | `BLOCKED_WITH_REASON` | No | No proof that `claude --name` launch is local-only/no-send. Prior help proves display-name is separate from positional prompt only. | Keep blocked until a future bounded safe sandbox/manual route proves no prompt/message send and no private UI capture. |
| Codex | `BLOCKED_WITH_REASON` | No | Packaged CLI was Access denied in prior evidence; `codex:` protocol and spreadsheet file associations do not prove a clean marker-only surface separate from the active conversation/workspace. | Keep blocked unless a reviewer/operator establishes a clean marker-only unsent Codex route. |

## Claude Blocker

Current route status: `BLOCKED_WITH_REASON`.

Exact marker source:

```text
FRDP-P04-MANUAL-ROUTE-Claude
```

Exact blocker:
- Prior local Claude Code `--help` evidence says `--name <name>` sets display metadata and separates that metadata from the positional prompt.
- That help evidence does not prove that a future non-help `claude --name` launch will avoid external-service connection or prompt/message send.
- The current fix-loop was not allowed to run a real probe, send a prompt/message, click/type into chat, or launch a command that may connect as a real service action.
- Therefore the safe local marker-only unsent Claude route is not proven.

Downgrade rule:
- Keep Claude as `BLOCKED_WITH_REASON` unless a future bounded work order can prove a safe sandbox/manual route without prompt/message send, external-service chat action, or private UI capture.

## Codex Blocker

Current route status: `BLOCKED_WITH_REASON`.

Exact marker source:

```text
FRDP-P04-MANUAL-ROUTE-Codex
```

Exact blockers:
- Prior evidence recorded Codex packaged CLI as Access denied before native process start.
- Codex Appx protocol/file associations prove only launch/file-association metadata.
- They do not prove marker display, separation from the active Codex conversation/workspace, or private-content-safe capture.
- No new safe static Codex evidence was found or claimed in this fix-loop.

## Commands Launched In This Fix-Loop

None.

This fix-loop used only file reads, file edits, JSON parsing, textual searches, and `git diff --check`. It did not launch Claude, Codex, or any command that may send a prompt or connect as a real service action.

## Packet Status Vocabulary

Current final route-establishment statuses used in this fix-loop:
- `BLOCKED_WITH_REASON`: 2 hosts, Claude and Codex.
- `ROUTE_CANDIDATE_REQUIRES_REVIEW`: 0 hosts.
- `ROUTE_UNCERTAIN_NEEDS_SAFE_SANDBOX`: not used because the current packet vocabulary already supports `BLOCKED_WITH_REASON`.

Forbidden current readiness/joined claims:
- `PASS_LOCAL_DESKTOP`
- `PASS_JOINED`
- `joined_claim_proven=true`

## Explicit Non-Claims

- I did not run a Claude marker probe.
- I did not run a Codex marker probe.
- I did not launch Claude or Codex in this fix-loop.
- I did not stage a marker in Claude/Codex chat.
- I did not send a prompt/message to Claude, Codex, Cursor, or any external service.
- I did not click/type into Claude or Codex chat UI.
- I did not capture screenshots or videos.
- I did not mutate models, training, checkpoints, or locators.
- I did not claim `PASS_LOCAL_DESKTOP`.
- I did not claim `PASS_JOINED`.
- I did not claim product readiness, full extension readiness, Codex chat UI readiness, token savings, semantic quality, learnability/training, checkpoint promotion, locator update, or host runtime pass.

## Verification Results

Verification is recorded after the packet files are written:
- JSON parse of `phase-04-claude-codex-route-establishment-packets.json`: PASS.
- JSON parse of `phase-04-local-desktop-packets.json`: PASS.
- Targeted unsupported-strong-phrase search for removed strong no-send wording: PASS.
- Targeted forbidden-claim search for `PASS_LOCAL_DESKTOP`, `PASS_JOINED`, and `joined_claim_proven=true`: PASS; hits are non-claims, forbidden-claim text, false fields, or historical context, not unsupported current claims.
- Targeted final status search for Claude `BLOCKED_WITH_REASON` and Codex `BLOCKED_WITH_REASON`: PASS.
- `git diff --check` over changed files plus no-index whitespace check for untracked changed files: PASS.
