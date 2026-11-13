# Claude Path B route preflight

## Человеческий итог

Проверялся 1 Claude scenario из prep-пакета `claude-full-extension-runtime-20260627-223159`.

Path A: accepted. Уже существующий пакет `claude-current-state-or-path-a-20260628-005038` принят как direct Claude API/core boundary fallback для этого сценария.

Path B: `BLOCKED_ROUTE_NOT_PROVEN`. Локальное окно Claude найдено, и UI Automation видит один focusable `Document` control с именем `Claude`, но маршрут к нужному thread/project/extension surface не доказан: нет видимого сигнала `XIIGen`, `Smart Skills` или extension route.

Я не вводил scenario prompt и не нажимал submit, потому что route не был однозначным и безопасным для запуска в этом preflight.

Этот пакет полезен только как blocked-route evidence. Он не является accepted Path B и не доказывает Claude UI prompt submission или Claude UI response.

## Что видно сейчас

- Найдено 9 процессов `claude`.
- Найдено 1 главное окно Claude: process `claude`, pid `15384`, title `Claude`, handle `0x103B8`.
- Bounds главного окна: left `689`, top `160`, width `1216`, height `677`.
- UIA controls scanned: `29`.
- UIA input state: `CLAUDE_INPUT_VISIBLE_BUT_EXTENSION_ROUTE_NOT_PROVEN`.
- Auth/login/password/PIN signals: none found by this redacted UIA scan.
- Route signals: none found by this redacted UIA scan.

## Скриншоты и visual evidence

`claude-window-redacted-geometry.png` is a redacted geometry image only. It does not persist raw Claude content and is not route-proof screenshot evidence.

Raw Claude route screenshot не сохранялся, потому что текущий route не доказан и raw chat content мог быть private.

## Оставшееся действие

Следующее безопасное действие: human или later authorized UI preflight должен вывести Claude на правильный XIIGen/Smart Skills thread/project/extension surface, снять privacy-safe route crop и только после этого решать, можно ли вводить и отправлять тот же scenario prompt.

Parent commit/push recommendation: review this evidence folder only as blocked-route evidence. If parent accepts that packet boundary, commit only this folder. Do not push without separate approval.
