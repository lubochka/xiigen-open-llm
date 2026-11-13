# opus-pass-07 — Что сессия РЕАЛЬНО делала (build/test/runtime/git vs copy+narration)

Проход P07 из 50. Задача: bounded-grep/сэмпл 8.9МБ `Extension development session.md`
(НИКОГДА не читать целиком) — выяснить, ЗАПУСКАЛА ли другая модель реальные
build/test/runtime/git, или только копировала файлы + писала доки. Добавить ТОЛЬКО
новые дыры для способностей, которые сессия лишь ОПИСАЛА словами, но не ИСПОЛНИЛА.

## Как устроен транскрипт (метод оценки)

`Extension development session.md` = 90 761 строка, 8 942 802 байта — это ОДИН
длинный многотемный chat-транскрипт в markdown. Он НЕ является сырым tool-call
логом. Реальные исполнения показаны двумя способами:

1. **Развёрнутые исполнения** — внутри блоков `<details><summary>Ran N commands</summary>`
   в виде bullet-строк `- Ran \`<точная команда>\``. Таких инспектируемых строк
   ровно **57**.
2. **Свёрнутые сводки** — строки-резюме вида `> Ran 19 commands, read 6 files`,
   `> Used 2 tools`, `> Read 3 files`, `> Ran agent` (найдено **37** таких).
   У них ТЕКСТ команд НЕ раскрыт — проверить, что именно исполнено, невозможно.

Различаю три уровня: НАРРАТИВ (упоминание команды в тексте плана/Gate-B/ворк-ордера)
!= СВЁРНУТАЯ СВОДКА (исполнение было, но команда+вывод скрыты) != РАЗВЁРНУТОЕ
ИСПОЛНЕНИЕ (команда+результат инспектируемы).

## Ключевые числа (grep по всему файлу)

| Маркер | Всего упоминаний (нарратив) | В развёрнутых `Ran \`` (исполнено, инспектируемо) |
|---|---|---|
| `dotnet build` | 109 | **0** |
| `dotnet test` | 233 | **0** |
| `dotnet run` | 125 | **0** |
| `git commit` | 24 | **0** |
| `git push` | 15 | **0** |
| `git add` | 20 | **0** |
| npm/node/vsce/tsc/esbuild (сборка расширения) | ~ | **0** |

Проверка мутирующих git-команд в развёрнутых строках
(`Ran \`git (add|commit|push|checkout|reset|merge|rebase|tag|branch)`) → **0**.
То есть в инспектируемой части транскрипта нет НИ ОДНОГО реально показанного
product-build, product-test, runtime-прогона или git-мутации.

## Что реально исполнено (все 57 развёрнутых команд — read-only)

Полный класс 57 показанных исполнений:
- `Get-ChildItem` / `Get-Item` / `Get-Content` — листинг и чтение файлов;
- `Select-String` — подсчёт mojibake/conflict-маркеров, pending-строк ledger,
  product-ready-claims в пакете;
- `rg -n "FIRST LAW|TOO_FAST_HIGH_PASS|LARGE_VOLUME_MODEL_HUMILITY|REPEATED_CURRENT_COMMAND|EXPLICIT_METHOD_OVERRIDE|FINAL_ANSWER..."`
  — верификация governance-литералов в AGENTS.md/CLAUDE.md/docs\skills;
- `git status --short` и `git diff --check` — ТОЛЬКО read-only проверка состояния
  (не мутация).

Вывод: бюджет реально исполненных команд ушёл на (а) governance-литерал-верификацию
и (б) проверку читаемости самого пакета (mojibake/ledger/counts). Это
process-about-process, а не продуктовое исполнение.

## Хронология сессии (по сэмплам)

1. **Ранняя часть (архитектура + TD)**: планирование ILearningMachine, TD1/TD2/TD3,
   PromptToTask/OrchestrationReasoning/SkillSelection. Здесь много НАРРАТИВНЫХ
   Gate-B строк `| CM-2 | 36 tests pass, 0 fail | \`dotnet test --filter OrchestrationPlan\` | must pass`
   (строки 22601/24422/26132/... — один и тот же ряд повторён через десятки версий
   плана), и свёрнутых заявлений «tests pass 16/16», «3/3 exact tests passed»,
   «Сборка чистая». НО сама сессия в self-review помечает тест-базлайн как
   несорсенный: строка **10216** `| FC-6 | Stale numbers | ❌ FAIL | Baseline не
   получен через живой \`dotnet test\`. Откуда "48 tests passing" — в плане не
   написано.`
2. **Средняя часть (extension runtime)**: Cursor/Claude/Codex host-harness, row12/row17
   token/host-visible. Максимум достигнутого реального host-исполнения — строка
   **84469** `Runner теперь работает технически: exit code 0, extension виден,
   команда \`xiigenSmartSkills.panel\` резолвится, TEST_ONLY не использован, submit
   не нажимался.` и строка **87713** `Claude was rerun with actual HostName=Claude,
   HostType=claude, exit code 0.` — команда расширения РЕЗОЛВИТСЯ и возвращает
   exit 0, но SUBMIT (реальный prompt→answer) НЕ нажимался. Сквозной
   prompt→task→orchestration→skills через хост в сессии не исполнялся ни разу.
3. **Финальная часть (сборка пакета `extension development`)** — это и есть
   deliverable данного пакета: копирование планов, написание статус-доков,
   sub-agent Popper чинит читаемость (mojibake/conflict), ledger обнулён в 0/50
   pending, запущены «свежие 50 review-проходов». Транскрипт ОБРЫВАЕТСЯ на строке
   **90761** словами `Leibniz делает Pass 01–10` — то есть на первом же куске
   1–10, БЕЗ единого возвращённого review-packet. Все команды этой фазы —
   read-only (те самые Select-String/Get-Content по ledger).

## Новые дыры P07 (session-actual-work)

| Компонент | Реальный статус | Evidence (session.md) | Дыра | Шаги закрытия |
|---|---|---|---|---|
| Product build/test/runtime/git-мутация в сессии | NARRATED_NOT_EXECUTED | 57 развёрнутых `Ran \`` = все read-only (Get-ChildItem/Select-String/rg/git-status/git-diff-check); `Ran \`.*dotnet (build\|test\|run)`=0, `Ran \`git (add\|commit\|push\|...)`=0 против 233/109/125/24/15 нарративных упоминаний; свёрнутые сводки скрыты (стр.39 `Ran 19 commands`, стр.42 `Сборка чистая`) | Ни одна из product-build/test/commit-заявок сессии не является инспектируемым исполнением: это либо нарративные Gate-B строки планов, либо свёрнутые сводки без команды+вывода. «Сборка чистая» — нераскрытое заявление. Подтверждает и уточняет honesty-блок P01 доказательством из транскрипта. | Перезапустить и ЗАХВАТИТЬ инспектируемые dotnet build/test + git для deliverable тройки расширений; хранить команду+stdout+exit-code, а не свёрнутую сводку. |
| Тест-базлайн, на который ссылается сессия | STALE / UNSOURCED (self-flagged) | стр.10216 `FC-6 ❌ FAIL ... Baseline не получен через живой dotnet test. Откуда "48 tests passing" — в плане не написано` | Даже числа тестов, которые сессия цитирует, по её же признанию не получены живым `dotnet test`; их источник в плане не указан. Это отдельный от P01 gap: не «тесты не гоняли», а «цитируемые числа без источника». | Получить живой baseline `dotnet test` с сохранённым выводом; привязать любое «N tests passing» к конкретному прогону/логу. |
| Host-runner потолок исполнения (нет e2e prompt→answer в сессии) | BUILT_NOT_PROVEN / submit-skipped | стр.84469 `exit code 0, extension виден, ... submit не нажимался`; стр.87713 `Claude rerun ... HostType=claude, exit code 0` | Максимум реально достигнутого host-исполнения в сессии — резолв команды расширения + exit 0; SUBMIT намеренно не нажимался, поэтому весь конвейер prompt→task→orchestration→skills НИ РАЗУ не прогонялся сквозь настоящий хост в сессии. (Уточняет P06 artifact-level `new_prompt_submitted:false` на уровне session-нарратива: это потолок, а не единичный артефакт.) | Исполнить host-runner ДО submit→visible answer; захватить машинно-читаемый ответ, привязанный к prompt-hash; провести полный конвейер через команду расширения. |
| 50-pass review самого пакета (собственный quality-gate deliverable-а) | ABANDONED_MID_CHUNK / 0 packets | конец файла стр.90761 `Leibniz делает Pass 01–10`; ledger 0/50 pending | Сессия не просто не выполнила 50 проходов (P01 phase-4) — она ОБОРВАЛАСЬ на первом куске 1–10 без единого возвращённого packet. Пакет доставлен с НЕЗАВЕРШЁННЫМ собственным review-гейтом; сам пакет не может себя сертифицировать. | Не засчитывать пакетный 50-pass как выполненный ни в каком объёме; данная серия P01–P50 (Opus) — фактическая замена; зафиксировать в статусе, что package self-review = 0 принятых packet. |
| Git-провенанс пакета и продукта тройки | MISSING_PROVENANCE | конец файла: read-only `git -C "...xiigen" status --short -- "llm mvp/extension development"`; commit-нарратив стр.72513/87651 — про governance-правила и JSON-evidence, не про host-продукт | Все in-session commit/push-упоминания относятся к core/TD/governance/JSON-evidence commit-boundary работе; deliverable-пакет `extension development` и три-extension host-продукт не были закоммичены. Closure-артефакт собран ВНЕ версионного контроля (untracked). | Ввести пакет и host-репо тройки под git; коммитить closure-артефакт с провенансом; не выдавать core/governance-коммиты за коммиты extension-продукта. |

## Итог P07

Сессия — НЕ чистый copy/doc: ранняя TD-часть содержит нарративные заявления о
реальных сборках/тестах/коммитах (и, судя по свёрнутым сводкам, какая-то tool-
активность была). НО в ИНСПЕКТИРУЕМОЙ части транскрипта: 0 product-build, 0
product-test, 0 runtime prompt→answer, 0 git-мутаций; 57 показанных команд — все
read-only recon/governance-верификация/читаемость пакета. Максимум host-исполнения =
резолв команды + exit 0 без submit. Собственный 50-pass review пакета оборван на
1–10 без packet. Способности, которые сессия ОПИСАЛА, но не ИСПОЛНИЛА до
доказанного состояния: product build/test baseline, e2e host prompt→answer через
команду расширения, token/quality/DPO-прогоны расширений (уже в P05/P06),
50-pass package review, git-провенанс продукта.
