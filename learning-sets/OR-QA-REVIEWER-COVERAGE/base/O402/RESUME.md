# Возобновление одной партии — CREATE_ONLY

Поток: `OR-QA-REVIEWER-COVERAGE/base`. Следующий: `DOC-SQL-SAVEPOINT/base`.

1. Прочитай `C:\Projects\llm_mvp_core\learning-sets\OR-QA-REVIEWER-COVERAGE\base\O402\state.json` и `C:\Projects\llm_mvp_core\learning-sets\OR-QA-REVIEWER-COVERAGE\base\O402\request.json`.
2. Если DRAFT_CREATED, переходи к следующему потоку; не открывай review.
3. Иначе прочитай `C:\Projects\llm_mvp_core\learning-sets\OR-QA-REVIEWER-COVERAGE\base\O402\prompt.md` и продолжи запись `C:\Projects\llm_mvp_core\learning-sets\OR-QA-REVIEWER-COVERAGE\base\O402\draft.jsonl`.
4. При живом процессе продолжи один сохранённый handle штатным wait/resume; отсутствие вывода не означает отказ.
5. После записи ровно назначенного числа пар выполни finish из commands.md.

Модель: текущая Kiro, без override. Не более одного помощника. Источники и схема уже включены в prompt; абсолютные пути находятся в request.paths. Не ищи их в другом проекте. Старые эксперименты, файлы и версии сохраняются. Прямые NEXT и STOP имеют приоритет.
