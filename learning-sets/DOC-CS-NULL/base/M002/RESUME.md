# Возобновление одной партии — CREATE_ONLY

Поток: `DOC-CS-NULL/base`. Следующий: `CODE-CS-NULL/base`.

1. Прочитай `C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\DOC-CS-NULL\base\M002\state.json` и `C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\DOC-CS-NULL\base\M002\request.json`.
2. Если DRAFT_CREATED, переходи к следующему потоку; не открывай review.
3. Иначе прочитай `C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\DOC-CS-NULL\base\M002\prompt.md` и продолжи запись `C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\DOC-CS-NULL\base\M002\draft.jsonl`.
4. При живом процессе продолжи один сохранённый handle штатным wait/resume; отсутствие вывода не означает отказ.
5. После записи ровно назначенного числа пар выполни finish из commands.md.

Модель: текущая Kiro, без override. Не более одного помощника. Источники и схема уже включены в prompt; абсолютные пути находятся в request.paths. Не ищи их в другом проекте. Старые эксперименты, файлы и версии сохраняются. Прямые NEXT и STOP имеют приоритет.
