# Cursor UI/session preflight recovery

Фаза 4 из 10: доказать живой runtime в реальных программах Claude/Cursor/Codex.

Текущий slice: Cursor UI/session preflight recovery.

Итоговый verdict: `BLOCKED_WINDOWS_LOCK_OR_PIN`.

Счетчик product readiness остается `0`. Эта проверка не доказывает готовность продукта.
Product readiness count remains 0.

## Кратко по шагу

Всего проверок в этом slice: 7.
Выполнено: 7 evidence/preflight checks записаны в эту папку.
В процессе: 0.
Осталось: 1 следующий bounded action: человек должен разблокировать Windows session без передачи PIN/password, после чего надо повторить preflight.

## Что доказано

- Найдено окно Cursor Extension Development Host.
- Title: `[Extension Development Host] cursor-xiigen-smart-skills - Cursor`.
- Process: `Cursor`.
- PID: `11344`.
- Handle: `0x2205FE`.
- Реальный desktop UI заблокирован Windows lock/PIN prompt; display name на screenshot замаскирован.
- Prompt не отправлялся, secrets/PIN/password не вводились, training не запускался.

## Что не доказано

- Cursor target не доказан focused/interactable.
- Cursor signed-in session не доказана; PrintWindow target показывает Cursor Log In / Sign Up без видимого email.
- Extension product readiness не доказана.
- Token/quality/learnability/savings/training claims не доказаны.

## Blocking facts

- Windows lock/PIN blocker: `screenshot-02-before-target-screenregion.png`.
- Cursor login required visible through target PrintWindow: `screenshot-01-before-target-printwindow.png`.
- Cursor Storage Error modal captured before action: `screenshot-03-before-storage-error-printwindow.png`.

No modal dismissal was attempted because Windows lock/PIN is the first blocker. No browser login was opened. No prompt was submitted.

## Evidence files

- `window-inventory.json`
- `window-inventory.txt`
- `selected-target.json`
- `screenshot-01-before-target-printwindow.png`
- `screenshot-02-before-target-screenregion.png`
- `screenshot-03-before-storage-error-printwindow.png`
- `screenshot-04-before-storage-error-screenregion.png`
- `redaction-log.json`
- `screenshot-manifest-before.json`
- `result.json`
- `verification.json`
- `verification.txt`

## Next bounded action

Human unlocks Windows session locally without sharing PIN/password, then rerun this same Cursor UI/session preflight before any Cursor prompt submission.
