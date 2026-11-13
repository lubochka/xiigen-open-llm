# Pass P40 — Git / branch / mergeability: реальное состояние (только read-only)

Ассессор: Opus 4.8. Дата: 2026-07-08. Область: закоммичен ли extension-код и на какой ветке;
мержабельность рабочей ветки в original base + main; git-состояние 3 host-репо.
Метод: только read-only git (`rev-parse`, `branch -a`, `remote -v`, `status --porcelain`,
`diff --shortstat`, `rev-list --left-right --count`, `merge-tree`, `worktree list`, `log`).
Репозитории: `C:\Projects\llm_mvp_core` и `C:\Projects\{claude,cursor,codex}-xiigen-smart-skills`.

Дедуп с прошлыми проходами: P07 (сессия исполнила 0 git-мутаций; пакет + host-продукт untracked;
in-session commit-narrative про governance/JSON, не host-продукт); P08 (board мис-локализует
deliverable; host-репо last-commit даты + working-tree дрейф M README/M package.json);
P09 (2 расходящиеся `.git`-копии Claude-плагина). P40 — про git-ТОПОЛОГИЮ ЯДРА и структуру
core↔host на уровне репозиториев, чего прошлые проходы не измеряли.

---

## 0. Честный положительный факт (не переоцениваю в минус)

ЗАКОММИЧЕННОЕ дерево `DesignToCode-clean` мержится ЧИСТО в обе базы:

- в original base `codex/chat-gpt-5.5`: `git merge-tree --write-tree HEAD codex/chat-gpt-5.5`
  → **exit 0**, вывод = только tree-hash `0b97e0385c6cab0b16e5abc6839e4fbe45c7947f`, **0 строк CONFLICT**.
  Старый формат `git merge-tree <base> HEAD codex/chat-gpt-5.5` показывает 7× «changed in both»
  (обе стороны редактировали общие файлы с момента merge-base), НО ни одной `<<<<<<<`/`CONFLICT` —
  git авто-мержит без конфликта.
- в `main`: HEAD на 2893 коммита впереди, 0 позади ⇒ merge = **fast-forward**, конфликтов нет.

Вывод: инвариант CLAUDE.md «clean-mergeable в original И main» на уровне КОММИТОВ держится.
Я НЕ заявляю merge-конфликт. Дыра — в другом (см. §1).

---

## 1. Центральная дыра: extension-продукт ЯДРА не закоммичен

`git -C C:\Projects\llm_mvp_core diff --shortstat`:

```
331 files changed, 18574 insertions(+), 3576 deletions(-)
```

`git status --porcelain | wc -l` = **876** записей; `^\?\?` (untracked) = **545**.

В незакоммиченную дельту входят ключевые extension-исходники:

- `43.SmartSkillsDomain/XiigenExtensionApiService.cs` (core-API тонкого адаптера — сердце всех 3 расширений)
- `43.SmartSkillsDomain/PromptToTaskMachine.cs`
- весь `44.DesignConversionDomain/*` — `git diff --stat` по нему = **22 файла, +4 717 / −245**
  (VisualJudge +355, TrainedUnits +282, VisualQaBoundaryPolicy +239, ThinClientJobRuntime +194, …)
- `42.ProjectWikiDomain/*` (6 training-pipeline файлов), `11.DPO/DpoTrainer.cs`,
  `15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs`, `25.ModelServer/ModelServer.csproj`,
  десятки `Tests/*` (Extension*Gates10..19, ExtensionToken04…, DesignConversion*).

Последние ФАКТИЧЕСКИЕ extension-коммиты в истории:

```
543990631 2026-07-01 17:19:46 latest
406256aa3 2026-06-28 16:36:28 Add DesignConversion learning domain
6d3da7a14 2026-06-26 23:49:11 docs: record extension readiness evidence boundary
29e7d4c3d 2026-06-23 21:35:17 phase 12: produce universal orchestration evidence
```

Значит текущее рабочее дерево ушло далеко за последний коммит (18,5 тыс. строк сверху),
и настоящий продукт (core-API сервис, prompt-to-task машина, весь DesignConversion) существует
ТОЛЬКО как грязное рабочее дерево — не сохранён, не версионирован, не мержабелен никуда.

Почему это делает §0-мержабельность бессодержательной: `merge-tree` считает по committed-дереву,
т.е. БЕЗ этих 18,5 тыс. строк. «Чистый merge» вычислен по состоянию, которого нет в реальном продукте.

**Closure:** закоммитить extension-дельту ядра фазовыми коммитами (Rule 6: имя фазы + список файлов),
прогнать `git diff --check` + тесты как pre-commit gate; только ПОСЛЕ этого перепроверить merge-tree.

---

## 2. Верхушка ветки не запушена; коммиты «latest» без provenance

`git rev-list --left-right --count origin/DesignToCode-clean...HEAD` = `0  2` ⇒ **2 непушнутых** коммита.
`git log origin/DesignToCode-clean..HEAD`:

```
0d4a87929 2026-07-02 19:45:15 governance: close ceremony-over-work loopholes (codex "dumbest session" analysis)
c25687041 2026-07-01 19:14:32 latest
```

HEAD-коммит — **governance**, не продукт. В recent-истории `543990631 latest`, `507fe4616 latest` —
три+ подряд коммита «latest». Это нарушает Rule 6 («Commit message names the phase and lists every file
changed»): provenance extension-работы в истории отсутствует.

**Closure:** запушить принятые коммиты после review-boundary; дать коммитам фазовые имена + список файлов;
запретить «latest» для extension-фаз.

---

## 3. Инвариант мержабельности не применён/не зафиксирован для deliverable

grep по пакету `extension development` на `merge-tree|mergeab|clean into.*(main|original|codex/chat)`
дал хиты ТОЛЬКО в скопированном тексте правил:
`supporting-files/project-CLAUDE.md`, `project-AGENTS.md`, `skills/audit-protocol-SKILL.md`,
`skills/claim-verification-SKILL.md`, `HOW-TO-USE-SKILLS.md`, `SESSION-START-PROMPT.md`.
Ни одной ФАКТИЧЕСКОЙ evidence-строки вида
`mergeability: clean into codex/chat-gpt-5.5 AND main (git merge-tree, 0 conflicts)`
для extension-плана или коммита нет.

Т.е. правило (`clean-mergeable-into-original-and-main invariant`, требующее такую строку на каждый
коммит/план) только СКОПИРОВАНО в пакет, но никогда не ПРИМЕНЕНО к extension-работе.
Плюс: любая такая строка сейчас была бы бессодержательна, пока продукт не закоммичен (§1).

**Closure:** после коммита дельты вводить per-phase строку мержабельности с именем original-ветки,
хранить exit-code + tree-hash; не считать мержабельность доказанной, пока продукт uncommitted.

---

## 4. Ветка не интегрирована: 32 позади original base, 2893 впереди main

```
git rev-list --left-right --count codex/chat-gpt-5.5...HEAD  =  32  1653
git rev-list --left-right --count main...HEAD                =   0  2893
```

- Original base `codex/chat-gpt-5.5` на **32 коммита ВПЕРЕДИ**, которых нет в deliverable-ветке
  (эти 32 base-коммита не интегрированы — риск расхождения/потери), а deliverable на 1653 впереди базы.
- `main` на **2893 позади**, 0 впереди ⇒ merge deliverable→main = чистый fast-forward, но он НИКОГДА не сделан.
  Весь extension-код живёт только на `DesignToCode-clean`; в `main` его 0.

**Closure:** втянуть 32 отставших base-коммита (rebase/merge с coordination-rule для общих файлов);
после review выполнить forward-merge/FF `DesignToCode-clean`→`main`; зафиксировать оба merge-tree=0-conflict.

---

## 5. Пролиферация worktree/checkout'ов ядра

`git worktree list` (9 worktree на один core-репо):

```
C:/Projects/llm_mvp_core                                                 0d4a87929 [DesignToCode-clean]
C:/Projects/_uus-worktrees/llm-mvp-core                                  cb70046dc [UpdateUniversalSkills]
C:/Projects/codex-mergeability-check-20260705-f0f21c73/wt-origin-main    ded0a6d89 (detached HEAD) prunable
C:/Projects/codex-wt-mergeability-chat-gpt-55-20260705-225510-31920      5084f355e (detached HEAD)
C:/Projects/codex-wt-mergeability-main-20260705-225510-31920             ded0a6d89 (detached HEAD)
C:/Projects/llm_mvp_core/.claude/worktrees/confident-jennings-4d87d9     a783f9d82 (detached HEAD)
C:/Projects/llm_mvp_core/.claude/worktrees/infallible-kowalevski-a243c0  fdb6b21e4 (detached HEAD)
C:/Projects/llm_mvp_core_codex_sync_20260628                             489c4bbad [codex/chat-gpt-5.5]
C:/Projects/llm_mvp_core_DesignToCode_worktree                           8f20ceb8a [DesignToCode]
```

Параллельные `DesignToCode` vs `DesignToCode-clean` vs `codex_sync_20260628` + три временных
`codex-*-mergeability-*` (один `prunable`). Неясно, какой checkout несёт канонический extension-deliverable;
extension-правки могут дрейфовать между копиями. Расширяет P09 (2 копии Claude-плагина) на уровень ЯДРА.

**Closure:** определить один канонический extension-checkout/ветку; `git worktree prune`;
архивировать `*_sync_*` / `*-mergeability-*`; задокументировать источник истины.

---

## 6. Три host-расширения — в 3 отдельных несвязанных GitHub-репо

| host | remote | branch | HEAD | dirty | unpushed |
|---|---|---|---|---|---|
| claude | github.com/lubochka/claude-xiigen-smart-skills | main | `e4eaac7 2026-06-23 phase 09: normalize claude proof evidence` | 2 | 0 |
| cursor | github.com/lubochka/cursor-xiigen-smart-skills | main | `46c7282 2026-06-28 test: preserve row12 check set references` | 7 | 0 |
| codex | github.com/lubochka/codex-xiigen-smart-skills | main | `6f96dfa 2026-06-18 phase RUNTIME-04: mirror Codex runtime QA evidence` | 0 | 0 |

Каждое расширение — самостоятельный `.git` (не submodule/не worktree ядра), на своём `main`,
отсоединённый от git-истории `llm_mvp_core`. Следствия:

- core-сторона расширения (незакоммичена на DesignToCode-clean, §1) и host-сторона (отдельный repo main)
  НЕ могут быть закоммичены / смержены / проверены на мержабельность как ОДНО изменение;
- инвариант «clean into original AND main» неприменим через границу репо (разные истории/базы);
- host-репо запушены (unpushed=0), но устаревшие (Jun18–28 vs board 2026-07-08) и с uncommitted-дрейфом
  (claude 2, cursor 7 — согласуется с P08).

**Closure:** согласованная схема версионирования core+host (submodule/monorepo или синхронные release-теги
с записью core-commit-hash в каждом host-релизе); либо явный документ раздельных репо + правило синхронизации;
закоммитить host working-tree дрейф; синхронизировать даты host-релизов с board.

---

## Итог P40 (6 новых дыр)

1. Extension-исходники ЯДРА (core-API, prompt-to-task, весь DesignConversion) — незакоммиченное грязное
   дерево: 331 файл / +18 574 / −3 576 + 545 untracked; «чистая мержабельность» посчитана без них.
2. Верхушка deliverable-ветки не запушена (2 коммита); история extension-работы в коммитах «latest» — нарушение Rule 6.
3. Инвариант мержабельности только скопирован в пакет, но не применён и не зафиксирован для extension-deliverable
   (0 фактических evidence-строк); при этом committed-дерево вживую проверено как clean-mergeable в обе базы.
4. DesignToCode-clean не интегрирована: 32 base-коммита не втянуты, 2893 extension-коммита не влиты в `main`
   (чистый FF доступен, никогда не сделан).
5. 9 worktree/checkout'ов одного ядра (DesignToCode vs DesignToCode-clean vs codex_sync + временные
   mergeability-worktree) — нет единого канонического extension-checkout.
6. 3 host-расширения — отдельные несвязанные GitHub-репо на своих `main`; нет атомарного версионного юнита
   core+host; host-репо устаревшие и частью dirty.

Ничего не объявляю «готовым». Все статусы привязаны к конкретному git-выводу выше или к явному «grep→0».
