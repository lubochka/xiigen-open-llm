# Claude Path B: проверка ввода prompt в composer после helper-fallback

Финальный статус: `BLOCKED_COMPOSER_FOCUS_TARGET_BAD`.

Этот пакет закрывает только узкий шаг Path B: безопасный prompt из сценария должен быть найден в composer локального Claude UI. Отправка prompt была запрещена и не выполнялась: `prompt_sent=false`, `enter_sent=false`, `send_button_clicked=false`.

Что продвинулось:

- использован проверенный helper-паттерн: top-level `EnumWindows`, выбранный Claude HWND, `EnumChildWindows` только от выбранного окна и bounded handle-rooted TreeWalker;
- маршрут XIIGen/Smart Skills был выбран через `InvokePattern`;
- composer-кандидат был найден в bounded UIA-дереве выбранного Claude HWND;
- prompt был загружен только в память runner по hash/length, без записи raw prompt в evidence.

Где остановилось:

- найденный composer target оказался плохой целью для фокуса: `composer_focus_action.focus_ok=false`, `focus_error="Target element cannot receive focus."`;
- fallback paste через безопасную установку clipboard и `Ctrl+V` не подтвердился: `prompt_input_action.ok=false`;
- probes после попытки не нашли exact/prefix prompt match;
- `prompt_typed_or_pasted=false`, `prompt_confirmed_in_composer=false`, `prompt_sent=false`.

Что этим пакетом не заявляется:

- prompt submission;
- ответ Claude;
- core/adapter correlation after submission;
- token savings;
- semantic quality;
- learnability/training;
- checkpoint promotion;
- all-host readiness.

Следующий минимальный ремонт: только composer locator mapping. Нужно подобрать корректный focus/input target для composer и повторить proof без отправки prompt.
