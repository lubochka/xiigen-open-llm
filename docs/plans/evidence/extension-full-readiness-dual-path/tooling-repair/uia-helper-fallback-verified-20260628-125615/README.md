# Проверенный fallback для UI helper

Пакет проверяет только безопасный fallback-паттерн для helper-скрипта. Claude UI здесь не открывался и не использовался.

## Что исправлено

Новый паттерн не начинает обход с корня рабочего стола. Вместо этого он:

1. перечисляет видимые top-level окна через `EnumWindows` с лимитом;
2. выбирает один harmless target по process/HWND;
3. обходит только дочерние HWND выбранного окна через `EnumChildWindows` с лимитом;
4. сохраняет только class/category/visible и длину подписи, а сам текст подписи редактирует;
5. для следующего runner допускает более глубокий UIA-обход только от заранее выбранного HWND и только с явными `MaxDepth`/`MaxNodes`.

## Как применить к следующему Claude runner

1. Сначала выполнить preflight: окно Claude найдено, пользовательская сессия уже открыта, секреты не читаются и не логируются.
2. Найти один top-level HWND Claude по process/window target, а не сканировать весь desktop.
3. Сначала снять bounded HWND-сводку: class/category/counts/redacted label lengths.
4. Если HWND-сводки мало, запускать handle-rooted `TreeWalker` только от выбранного HWND, с глубиной и счетчиком узлов.
5. Любой лог должен оставаться redacted: без raw label text, без clipboard, без секретов.

## Файлы

- `run-uia-helper-fallback.ps1` - минимальный runner.
- `run.log` - журнал фактического запуска.
- `window-summary.json` - redacted сводка найденных окон.
- `result.json` - machine-readable статус.
- `action-log.md` - ручной журнал действий и проверок.
