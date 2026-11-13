# Возобновление одной партии — CREATE_ONLY

Поток: `CODE-CS-AWAIT/base`. Следующий: `DOC-SQL-LEFT/base`.

1. Прочитай `C:\Projects\llm_mvp_core\learning-sets\CODE-CS-AWAIT\base\T489\state.json` и `C:\Projects\llm_mvp_core\learning-sets\CODE-CS-AWAIT\base\T489\request.json`.
2. Если DRAFT_CREATED, переходи к следующему потоку; не открывай review.
3. Иначе прочитай `C:\Projects\llm_mvp_core\learning-sets\CODE-CS-AWAIT\base\T489\prompt.md` и продолжи запись `C:\Projects\llm_mvp_core\learning-sets\CODE-CS-AWAIT\base\T489\draft.jsonl`.
4. При живом процессе продолжи один сохранённый handle штатным wait/resume; отсутствие вывода не означает отказ.
5. После записи ровно назначенного числа пар выполни finish из commands.md.

Модель: текущая Kiro, без override. Не более одного помощника. Источники и схема уже включены в prompt; абсолютные пути находятся в request.paths. Не ищи их в другом проекте. Старые эксперименты, файлы и версии сохраняются. Прямые NEXT и STOP имеют приоритет.
