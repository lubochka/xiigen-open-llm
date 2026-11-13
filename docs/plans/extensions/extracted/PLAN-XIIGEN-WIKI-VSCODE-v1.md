# Plan — XIIGen Smart Code Wiki: VS Code Extension
# Version: 1.0 — 2026-05-15
# Status: REFERENCE - DO NOT EXECUTE directly.
# Execution: use SESSION files once approved.
#
# This plan is fully standalone. It does not require any other plugin plan.
# It covers VS Code, Cursor, and Windsurf (same .vsix, different configs).

---

## Goal

Ship a VS Code extension that injects wiki context into every AI prompt,
shows a code relevance map in the sidebar, and works before the model is
trained (Stage A) and after (Stage B) with zero code changes between stages.

---

## Solution Structure

```
xiigen-vscode-extension/
  package.json                    ← extension manifest, activation events
  tsconfig.json
  src/
    extension.ts                  ← activation entry point
    hookRunner.ts                 ← calls wiki-context-hook.exe via child_process
    wikiPanel.ts                  ← WebviewViewProvider (sidebar panel)
    copilotParticipant.ts         ← vscode.chat participant for context injection
    statusBar.ts                  ← status bar item
    types.ts                      ← shared types matching WikiAnswer contract
  webview/
    panel.html                    ← sidebar panel UI
    panel.js                      ← panel logic (search, render results)
  bin/
    wiki-context-hook.exe         ← bundled or downloaded on first activation
  .vscodeignore
```

---

## Stage A — Mock (Start Today, No Training Required)

### What Stage A requires

The only prerequisite is that `evidence-index.jsonl` exists:
```
.xiigen/wiki-domain/evidence/evidence-index.jsonl
```
This is created by SESSION-02. It does not require SESSION-04 or PROMOTED status.

Mode 3 (`--map`) uses BM25 against this file — fully functional in Stage A.
Mode 2 (prompt injection) uses `MockWikiOrchestrator` — returns fixture answers.

### Step A.1 — hookRunner.ts

All communication with the hook binary goes through one function.
No other file imports `child_process`.

```typescript
// src/hookRunner.ts
import { spawn } from 'child_process'
import * as path from 'path'
import * as fs from 'fs'
import * as vscode from 'vscode'

export interface WikiHookResult {
  systemPromptAddition: string
  wikiContext?: {
    projectId: string
    confidence: number
    evidenceIds: string[]
    formattedBlock: string
    status: string        // "PROMOTED" | "MOCK" | "TRAINING"
  }
}

export interface WikiMapResult {
  entries: Array<{ name: string; summary: string; filePath: string; score: number }>
  dataFlows: string[]
}

export interface WikiReadinessResult {
  isReady: boolean
  status: string
  documentationVersion: string
  entryCount: number
  nextAction: string
}

function getHookBinaryPath(): string {
  // 1. Bundled with extension
  const bundled = path.join(__dirname, '..', 'bin', 'wiki-context-hook.exe')
  if (fs.existsSync(bundled)) return bundled

  // 2. User-configured path
  const config = vscode.workspace.getConfiguration('xiigen')
  const configured = config.get<string>('hookPath')
  if (configured && fs.existsSync(configured)) return configured

  // 3. System PATH fallback
  return 'wiki-context-hook'
}

function getProjectRoot(): string {
  const folders = vscode.workspace.workspaceFolders
  if (!folders?.length) return process.cwd()
  // Use first workspace folder; multi-root handled by iterating all
  return folders[0].uri.fsPath
}

async function runHook(
  args: string[],
  env: Record<string, string>,
  timeoutMs: number
): Promise<string> {
  return new Promise((resolve) => {
    const proc = spawn(getHookBinaryPath(), args, {
      env: { ...process.env, ...env },
      stdio: 'pipe'
    })
    let out = ''
    proc.stdout.on('data', (d: Buffer) => out += d.toString())
    proc.on('close', () => resolve(out.trim()))
    proc.on('error', () => resolve(''))
    setTimeout(() => { proc.kill(); resolve('') }, timeoutMs)
  })
}

export async function queryWiki(prompt: string): Promise<WikiHookResult | null> {
  const raw = await runHook([], {
    HOOK_PROMPT: prompt,
    XIIGEN_PROJECT_ROOT: getProjectRoot()
  }, 500)
  if (!raw) return null
  try { return JSON.parse(raw) } catch { return null }
}

export async function getMap(query: string): Promise<WikiMapResult | null> {
  const raw = await runHook(['--map', query], {
    XIIGEN_PROJECT_ROOT: getProjectRoot()
  }, 2000)
  if (!raw) return null
  try { return JSON.parse(raw) } catch { return null }
}

export async function checkReadiness(): Promise<WikiReadinessResult | null> {
  const raw = await runHook(['--ensure-ready', '--json'], {
    XIIGEN_PROJECT_ROOT: getProjectRoot()
  }, 5000)
  if (!raw) return null
  try { return JSON.parse(raw) } catch { return null }
}
```

**Gate B A.1:**
```bash
# Unit test: hookRunner returns null on missing binary, not exception
# npm test -- --testPathPattern hookRunner
```

### Step A.2 — Copilot context injection

VS Code 1.90+ provides `vscode.chat.createChatParticipant`. This is the
injection point. Do not intercept `onDidChangeChatModels` — that fires when
models change, not when prompts are submitted.

```typescript
// src/copilotParticipant.ts
import * as vscode from 'vscode'
import { queryWiki } from './hookRunner'

export function registerCopilotParticipant(
  context: vscode.ExtensionContext
): vscode.Disposable {
  // Register as implicit context provider for all chat requests
  return vscode.chat.createChatParticipant(
    'xiigen.wikiContext',
    async (request, _chatContext, stream, _token) => {
      const result = await queryWiki(request.prompt)
      if (result?.systemPromptAddition) {
        // Prepend wiki context as a reference
        stream.reference(
          vscode.Uri.parse('xiigen://wiki-context'),
          result.systemPromptAddition
        )
      }
      // Participant does not respond itself — it only injects context
    }
  )
}
```

For VS Code versions < 1.90 that do not have `createChatParticipant`:
fall back to writing `hook-state.json` and letting the user paste
the wiki context block manually via the "Copy Wiki Context" command.
Do not block or error.

**Gate B A.2:**
```typescript
// In extension.test.ts
// Activate extension, open test workspace
// Run command: "xiigen.copyWikiContext"
// Clipboard should contain "[MOCK] Found N relevant components"
// or real wiki block if PROMOTED
```

### Step A.3 — Sidebar panel (WebviewViewProvider)

```typescript
// src/wikiPanel.ts
import * as vscode from 'vscode'
import { getMap, checkReadiness } from './hookRunner'

export class WikiPanelProvider implements vscode.WebviewViewProvider {
  public static readonly viewType = 'xiigen.wikiPanel'

  resolveWebviewView(webviewView: vscode.WebviewView): void {
    webviewView.webview.options = { enableScripts: true }
    webviewView.webview.html = this.getHtmlContent()

    // Receive search queries from the panel
    webviewView.webview.onDidReceiveMessage(async (message) => {
      if (message.type === 'search') {
        const result = await getMap(message.query)
        webviewView.webview.postMessage({ type: 'results', data: result })
      }
      if (message.type === 'checkStatus') {
        const status = await checkReadiness()
        webviewView.webview.postMessage({ type: 'status', data: status })
      }
    })
  }

  private getHtmlContent(): string {
    // Read from webview/panel.html (bundled with extension)
    // Template has: search input, results list, status indicator
    return `<!DOCTYPE html><html><body>
      <input id="q" type="text" placeholder="Search code..." />
      <button onclick="search()">Find</button>
      <div id="status"></div>
      <ul id="results"></ul>
      <script>
        const vscode = acquireVsCodeApi();
        function search() {
          vscode.postMessage({ type: 'search', query: document.getElementById('q').value });
        }
        window.addEventListener('message', e => {
          const { type, data } = e.data;
          if (type === 'results' && data?.entries) {
            document.getElementById('results').innerHTML =
              data.entries.map(e =>
                '<li><b>' + e.name + '</b> — ' + e.summary +
                '<br><small>' + e.filePath + '</small></li>'
              ).join('');
          }
          if (type === 'status') {
            document.getElementById('status').textContent =
              'XIIGen Wiki: ' + (data?.status || 'unknown') +
              ' · ' + (data?.entryCount || 0) + ' entries';
          }
        });
        vscode.postMessage({ type: 'checkStatus' });
      </script>
    </body></html>`
  }
}
```

**Gate B A.3:**
```
1. Open VS Code with extension installed
2. Activity Bar → XIIGen icon → panel opens
3. Status shows "XIIGen Wiki: MOCK · N entries" (or PROMOTED if available)
4. Type "authorization" in search → results appear with file paths
5. Click result → file opens at correct line
```

### Step A.4 — Status bar item

```typescript
// src/statusBar.ts
import * as vscode from 'vscode'
import { checkReadiness } from './hookRunner'

export function createStatusBarItem(
  context: vscode.ExtensionContext
): vscode.StatusBarItem {
  const item = vscode.window.createStatusBarItem(
    vscode.StatusBarAlignment.Right, 100)
  item.command = 'xiigen.openWikiPanel'

  async function refresh() {
    const status = await checkReadiness()
    if (!status) {
      item.text = '$(database) XIIGen: Not ready'
      item.tooltip = 'XIIGen Wiki not initialized'
    } else {
      item.text = `$(database) XIIGen: ${status.status} · ${status.entryCount}`
      item.tooltip = `Wiki status: ${status.status}\n${status.nextAction}`
    }
    item.show()
  }

  refresh()
  // Refresh every 60s and on file save
  const timer = setInterval(refresh, 60_000)
  vscode.workspace.onDidSaveTextDocument(refresh, undefined, context.subscriptions)
  context.subscriptions.push({ dispose: () => clearInterval(timer) })
  return item
}
```

### Step A.5 — extension.ts wiring

```typescript
// src/extension.ts
import * as vscode from 'vscode'
import { WikiPanelProvider } from './wikiPanel'
import { registerCopilotParticipant } from './copilotParticipant'
import { createStatusBarItem } from './statusBar'
import { queryWiki, getMap } from './hookRunner'

export function activate(context: vscode.ExtensionContext) {
  // Sidebar panel
  context.subscriptions.push(
    vscode.window.registerWebviewViewProvider(
      WikiPanelProvider.viewType,
      new WikiPanelProvider()))

  // Copilot context injection
  if (typeof vscode.chat?.createChatParticipant === 'function') {
    context.subscriptions.push(registerCopilotParticipant(context))
  }

  // Status bar
  context.subscriptions.push(createStatusBarItem(context))

  // Commands
  context.subscriptions.push(
    vscode.commands.registerCommand('xiigen.openWikiPanel', () =>
      vscode.commands.executeCommand('workbench.view.extension.xiigen-panel')),

    vscode.commands.registerCommand('xiigen.copyWikiContext', async () => {
      const editor = vscode.window.activeTextEditor
      const prompt = editor?.document.getText(editor.selection) || ''
      const result = await queryWiki(prompt || 'current file context')
      if (result?.systemPromptAddition) {
        await vscode.env.clipboard.writeText(result.systemPromptAddition)
        vscode.window.showInformationMessage('XIIGen: Wiki context copied to clipboard')
      }
    }),

    vscode.commands.registerCommand('xiigen.showMap', async () => {
      const query = await vscode.window.showInputBox({ prompt: 'Search code...' })
      if (!query) return
      const result = await getMap(query)
      if (result?.entries.length) {
        const items = result.entries.map(e => ({
          label: e.name,
          description: e.summary,
          detail: e.filePath
        }))
        const pick = await vscode.window.showQuickPick(items)
        if (pick?.detail)
          vscode.workspace.openTextDocument(pick.detail)
            .then(d => vscode.window.showTextDocument(d))
      }
    })
  )
}

export function deactivate() {}
```

### Step A.6 — package.json (essential fields only)

```json
{
  "name": "xiigen-wiki",
  "displayName": "XIIGen Smart Wiki",
  "version": "0.1.0",
  "engines": { "vscode": "^1.85.0" },
  "activationEvents": ["onStartupFinished"],
  "contributes": {
    "views": {
      "xiigen-panel": [{
        "type": "webview",
        "id": "xiigen.wikiPanel",
        "name": "XIIGen Wiki"
      }]
    },
    "viewsContainers": {
      "activitybar": [{
        "id": "xiigen-panel",
        "title": "XIIGen",
        "icon": "$(database)"
      }]
    },
    "commands": [
      { "command": "xiigen.openWikiPanel",   "title": "XIIGen: Open Wiki Panel" },
      { "command": "xiigen.copyWikiContext", "title": "XIIGen: Copy Wiki Context" },
      { "command": "xiigen.showMap",         "title": "XIIGen: Show Code Map" }
    ],
    "configuration": {
      "properties": {
        "xiigen.hookPath": {
          "type": "string",
          "description": "Path to wiki-context-hook.exe if not bundled"
        }
      }
    }
  },
  "main": "./out/extension.js"
}
```

---

## Stage A: Full Gate B Checklist

Run before calling Stage A complete:

```bash
# 1. Build succeeds
cd xiigen-vscode-extension && npm run compile
# Expected: no TypeScript errors

# 2. Package builds
npx @vscode/vsce package
# Expected: xiigen-wiki-0.1.0.vsix created

# 3. Install locally
code --install-extension ./xiigen-wiki-0.1.0.vsix

# 4. Open test project (SESSION-02 complete = evidence-index exists)
code C:\Projects\askapediaweb

# 5. Status bar check
# Expected: "XIIGen Wiki: MOCK · N" (N > 0 if evidence-index exists)

# 6. Sidebar Mode 3 check
# Open panel → type "authorization" → results appear
# Expected: ≥ 1 result with file path, ≤ 200ms

# 7. Hook invocation check (terminal)
HOOK_PROMPT="tenant isolation" XIIGEN_PROJECT_ROOT=C:\Projects\askapediaweb \
  node -e "
const {spawn} = require('child_process');
const p = spawn('./bin/wiki-context-hook.exe', [],
  {env:{...process.env,HOOK_PROMPT:'tenant isolation',
    XIIGEN_PROJECT_ROOT:'C:/Projects/askapediaweb'}, stdio:'pipe'});
let o=''; p.stdout.on('data',d=>o+=d); p.on('close',()=>console.log(JSON.parse(o)));
"
# Expected: { systemPromptAddition: '...[MOCK]...' }

# 8. No-project resilience
HOOK_PROMPT="test" XIIGEN_PROJECT_ROOT=/tmp/noproject \
  ./bin/wiki-context-hook.exe ; echo "exit: $?"
# Expected: exit 0, empty stdout
```

---

## Stage B — Real Model (After SESSION-04 PROMOTED)

`WikiOrchestratorFactory.Create()` in `42.ProjectWikiDomain` detects PROMOTED
and returns the real orchestrator. The extension code does not change.

### Stage B Gate B Checklist

```bash
# 1. Verify PROMOTED
cat C:\Projects\askapediaweb\.xiigen\wiki-domain\model-manifest.json | jq '.status'
# Expected: "PROMOTED"

# 2. Status bar now shows PROMOTED
# "XIIGen Wiki: PROMOTED · 247"

# 3. Hook returns real answer (no [MOCK] prefix)
HOOK_PROMPT="tenant isolation" XIIGEN_PROJECT_ROOT=C:\Projects\askapediaweb \
  ./bin/wiki-context-hook.exe | jq '.wikiContext.confidence'
# Expected: > 0.65

# 4. Copilot injection verification
# Open GitHub Copilot Chat in VS Code
# Ask any question about the project
# In Output → XIIGen Wiki channel:
# Should show: "Wiki context injected: confidence=0.87, evidence=2 ids"

# 5. Regression test: all Stage A fixtures pass with real model
XIIGEN_PROJECT_ROOT=C:\Projects\askapediaweb \
  dotnet test --filter "WikiRegression"
# Expected: all pass, no [MOCK] in answers

# 6. Publish version 1.0 to Open VSX
npx ovsx publish xiigen-wiki-1.0.0.vsix \
  --pat $OPEN_VSX_TOKEN
```

---

## Cursor and Windsurf (Free Ride)

Both Cursor and Windsurf install from Open VSX. Publishing to Open VSX once
covers all three.

Cursor-specific `.cursor/hooks.json` (for teams using cursor hook API):
```json
{
  "version": 1,
  "hooks": {
    "sessionStart": [
      { "command": "wiki-context-hook.exe --ensure-ready --json" }
    ],
    "beforeSubmitPrompt": [
      { "command": "wiki-context-hook.exe", "timeout": 500 }
    ]
  }
}
```

Windsurf uses the extension API identically to VS Code. No additional config.

---

## Development Loop Summary

```
Day 1–3 (Stage A setup):
  □ Scaffold extension with npm create (vscode extension generator)
  □ hookRunner.ts + unit test (mock binary path)
  □ Sidebar panel with hardcoded "loading..." placeholder
  □ Status bar item showing "XIIGen: initializing"

Day 3–5 (Stage A wiring):
  □ hookRunner calls real wiki-context-hook.exe
  □ Mode 3 --map results appear in sidebar (real BM25)
  □ Status bar shows real status from --ensure-ready

Day 5–7 (Stage A copilot):
  □ Copilot participant registration
  □ Context injection log in output channel
  □ xiigen.copyWikiContext command

Day 7 (Stage A Gate B):
  □ All 8 Gate B checks pass
  □ Extension works with mock on any project

When SESSION-04 PROMOTED (Stage B):
  □ Run regression tests
  □ Verify status bar shows PROMOTED
  □ Publish to Open VSX
```
