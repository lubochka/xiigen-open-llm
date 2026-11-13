# Возобновление одной партии — CREATE_ONLY

Поток: `TR05-CONTEXT-SENSE/ru-he`. Следующий: `TR05-CONTEXT-SENSE/he-ru`.

1. Прочитай `C:\Projects\llm_mvp_core\learning-sets\TR05-CONTEXT-SENSE\ru-he\T327\state.json` и `C:\Projects\llm_mvp_core\learning-sets\TR05-CONTEXT-SENSE\ru-he\T327\request.json`.
2. Если DRAFT_CREATED, переходи к следующему потоку; не открывай review.
3. Иначе прочитай `C:\Projects\llm_mvp_core\learning-sets\TR05-CONTEXT-SENSE\ru-he\T327\prompt.md` и продолжи запись `C:\Projects\llm_mvp_core\learning-sets\TR05-CONTEXT-SENSE\ru-he\T327\draft.jsonl`.
4. При живом процессе продолжи один сохранённый handle штатным wait/resume; отсутствие вывода не означает отказ.
5. После записи ровно назначенного числа пар выполни finish из commands.md.

Модель: текущая Kiro, без override. Не более одного помощника. Источники и схема уже включены в prompt; абсолютные пути находятся в request.paths. Не ищи их в другом проекте. Старые эксперименты, файлы и версии сохраняются. Прямые NEXT и STOP имеют приоритет.
