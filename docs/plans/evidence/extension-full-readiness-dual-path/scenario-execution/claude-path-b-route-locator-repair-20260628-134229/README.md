# Claude Path B: route locator repair

Статус: `BLOCKED_ROUTE_TARGET_NOT_FOUND`.

Этот пакет проверяет только локатор маршрута Claude для XIIGen/Smart Skills. Prompt не вводился, не вставлялся и не отправлялся. Composer не картировался.

Что выполнено:

- выбран реальный локальный top-level window `Claude`;
- окно поднято на передний план безопасным действием restore/foreground;
- UIA обход выполнен только от выбранного Claude HWND через bounded `TreeWalker.RawViewWalker`;
- whole-tree `RootElement.FindAll` по desktop tree не использовался;
- начальный bounded scan увидел 50 UIA nodes;
- видимый XIIGen/Smart Skills route target не найден;
- безопасных неиспользованных навигационных recovery-кандидатов runner не нашел, поэтому дополнительных UI actions не выполнял;
- screenshot, приватный текст, prompt text и clipboard text не сохранялись.

Итог:

- `route_visible=false`;
- `candidate_count=0`;
- `selected_route_target_summary=null`;
- `prompt_typed_or_pasted=false`;
- `prompt_sent=false`;
- `deprecated_product_readiness_used=false`.

Блокер: видимый XIIGen/Smart Skills route target не найден после bounded selected-HWND scan. В текущем UI tree не было безопасного навигационного recovery-кандидата для раскрытия маршрута.

Следующий шаг: вручную вывести Claude в состояние, где XIIGen/Smart Skills entrypoint виден, либо добавить более точный safe navigation locator, затем повторить только route-locator repair. Не вводить prompt, не картировать composer и не отправлять сообщение в рамках этого пакета.

Что этим пакетом не заявляется:

- prompt typing/paste/submission;
- composer mapping;
- Claude response;
- core/adapter correlation after submission;
- token savings;
- semantic quality;
- learnability/training;
- checkpoint promotion;
- cross-host completion.
