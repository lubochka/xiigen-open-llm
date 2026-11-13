# Возобновление одной партии — CREATE_ONLY

Поток: `TR09-IDENTIFIER/ru-en`. Следующий: `TR09-IDENTIFIER/en-ru`.

1. Прочитай `C:\Projects\llm_mvp_core\learning-sets\TR09-IDENTIFIER\ru-en\T385\state.json` и `C:\Projects\llm_mvp_core\learning-sets\TR09-IDENTIFIER\ru-en\T385\request.json`.
2. Если DRAFT_CREATED, переходи к следующему потоку; не открывай review.
3. Иначе прочитай `C:\Projects\llm_mvp_core\learning-sets\TR09-IDENTIFIER\ru-en\T385\prompt.md` и продолжи запись `C:\Projects\llm_mvp_core\learning-sets\TR09-IDENTIFIER\ru-en\T385\draft.jsonl`.
4. При живом процессе продолжи один сохранённый handle штатным wait/resume; отсутствие вывода не означает отказ.
5. После записи ровно назначенного числа пар выполни finish из commands.md.

Модель: текущая Kiro, без override. Не более одного помощника. Источники и схема уже включены в prompt; абсолютные пути находятся в request.paths. Не ищи их в другом проекте. Старые эксперименты, файлы и версии сохраняются. Прямые NEXT и STOP имеют приоритет.
