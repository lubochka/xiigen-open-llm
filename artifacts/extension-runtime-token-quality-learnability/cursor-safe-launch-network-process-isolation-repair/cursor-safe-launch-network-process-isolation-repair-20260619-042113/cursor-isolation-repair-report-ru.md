# CURSOR-SAFE-LAUNCH-NETWORK-PROCESS-ISOLATION-REPAIR-03

## Решение

Cursor не запускался. Prompt/chat/model surface не трогались. Эта фаза чинит не runtime-доказательство, а безопасный контракт запуска: точная команда, изолированные каталоги, временный firewall probe/deny метод, команды same-run process/network monitor, cleanup и no-overclaim границы.

Статус ремонта: `CURSOR_ISOLATION_CONTRACT_READY_NO_LIVE_PROOF`.

## Что было слабым

В предыдущем packet был найден `Cursor.exe` версии `3.8.11` и extension path candidate `C:\Projects\cursor-xiigen-smart-skills`, но решение осталось `defer_no_safe_launch`. Не хватало активного pre-launch network deny правила, same-run process tree monitor, same-run network monitor, cleanup доказательства и строгой трактовки старых backend/model metadata risk strings.

## Что теперь repaired

- Создан dry-run helper: `tools/cursor-safe-launch-isolation-preflight.ps1`.
- Helper по умолчанию не запускает Cursor, не создаёт firewall rule и не вызывает внешние AI/model surfaces.
- Helper валидирует `Cursor.exe`, extension development path и `package.json`.
- Helper строит exact command line с isolated `--user-data-dir` и `--extensions-dir`.
- Helper создаёт/валидирует только изолированные каталоги внутри artifact run scope.
- Helper содержит выключенную по умолчанию temporary firewall probe ветку: при отсутствии admin она возвращает `admin_required=true` и ничего не меняет; при admin создаёт temporary outbound block rule for Cursor.exe, verifies present, removes it, verifies removed.
- Helper содержит future launch guard: launch запрещён, если нет admin, firewall probe PASS, monitor cmdlets и cleanup.
- Contract записывает exact future monitor commands for process tree and network snapshots.

## Что остаётся

Это не live UI/chat proof. Active firewall rule сейчас не оставлен и same-run monitor не выполнялся, потому что Cursor не запускался. Следующий live work order должен сначала включить temporary firewall probe/deny, стартовать monitor до запуска, затем запускать Cursor без prompt/chat/model actions. Если firewall/admin/monitor gate не проходит, live work order обязан перейти в test-only fallback без overclaim.

## No-overclaim

Этот packet доказывает только готовность safe-launch contract/helper к следующему bounded live-preflight шагу. Он не доказывает Cursor runtime readiness, full UI/chat readiness, token savings, quality improvement, learnability, DPO/lifecycle readiness, target promotion, locator update, final readiness или product readiness.
