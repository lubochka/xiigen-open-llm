# Возобновление одной партии — CREATE_ONLY

Поток: `TR09-IDENTIFIER/en-ru`. Следующий: `TR09-IDENTIFIER/ru-he`.

1. Прочитай `C:\Projects\llm_mvp_core\learning-sets\TR09-IDENTIFIER\en-ru\T338\state.json` и `C:\Projects\llm_mvp_core\learning-sets\TR09-IDENTIFIER\en-ru\T338\request.json`.
2. Если DRAFT_CREATED, переходи к следующему потоку; не открывай review.
3. Иначе прочитай `C:\Projects\llm_mvp_core\learning-sets\TR09-IDENTIFIER\en-ru\T338\prompt.md` и продолжи запись `C:\Projects\llm_mvp_core\learning-sets\TR09-IDENTIFIER\en-ru\T338\draft.jsonl`.
4. При живом процессе продолжи один сохранённый handle штатным wait/resume; отсутствие вывода не означает отказ.
5. После записи ровно назначенного числа пар выполни finish из commands.md.

Модель: текущая Kiro, без override. Не более одного помощника. Источники и схема уже включены в prompt; абсолютные пути находятся в request.paths. Не ищи их в другом проекте. Старые эксперименты, файлы и версии сохраняются. Прямые NEXT и STOP имеют приоритет.
