# CREATE_ONLY: запись кандидатов и следующий поток

Команды работают из любого текущего каталога. Открой prompt.md в текущей Kiro-сессии и создай реальные пары в draft.jsonl. prepare сам не генерирует данные.

После записи выполни один структурный finish; unknown меняй только на наблюдаемое имя модели:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --input 'C:\Projects\llm_mvp_core\learning-sets\TR06-PLACEHOLDER\ru-he\T333\draft.jsonl' --request 'C:\Projects\llm_mvp_core\learning-sets\TR06-PLACEHOLDER\ru-he\T333\request.json' --model 'unknown'
```

После DRAFT_CREATED переходи к указанному в результате следующему ID. Содержательная оценка остаётся NOT_ESTABLISHED. Не запускать blind/review/oracle/accept/export/обучение.

После compaction или при уже существующей папке:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' resume --request 'C:\Projects\llm_mvp_core\learning-sets\TR06-PLACEHOLDER\ru-he\T333\request.json'
```

resume читает фактические файлы, не запускает генерацию. Частичный draft дописывай с сохранением уже созданных строк и назначенных family IDs; не удаляй папку и не создавай дубль. Если пользователь сказал «следующий», переходи немедленно без завершения прошлой работы. При «стоп» прекрати действия.
