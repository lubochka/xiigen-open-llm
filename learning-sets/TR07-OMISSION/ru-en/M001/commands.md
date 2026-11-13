# Команды из каталога preparation

Открыть prompt.md в выбранной Kiro-сессии. Генератор пишет draft.jsonl; он не придумывает результаты review и не запускает обучение.

```powershell
python tools/batches.py check --input "C:\Projects\llm_mvp_core\learning-sets\TR07-OMISSION\ru-en\M001\draft.jsonl" --request "C:\Projects\llm_mvp_core\learning-sets\TR07-OMISSION\ru-en\M001\request.json"
python tools/batches.py blind --input "C:\Projects\llm_mvp_core\learning-sets\TR07-OMISSION\ru-en\M001\draft.jsonl" --request "C:\Projects\llm_mvp_core\learning-sets\TR07-OMISSION\ru-en\M001\request.json" --out "C:\Projects\llm_mvp_core\learning-sets\TR07-OMISSION\ru-en\M001\blind.jsonl"
```

Отдельный проверяющий читает blind.review.prompt.md и blind.jsonl; не передавать ему draft, key, rationale или готовые метки. Он заполняет blind.review.template.json и сохраняет review.json. Не выдавать шаблон за завершённый review.

```powershell
python tools/batches.py accept --input "C:\Projects\llm_mvp_core\learning-sets\TR07-OMISSION\ru-en\M001\draft.jsonl" --request "C:\Projects\llm_mvp_core\learning-sets\TR07-OMISSION\ru-en\M001\request.json" --blind "C:\Projects\llm_mvp_core\learning-sets\TR07-OMISSION\ru-en\M001\blind.jsonl" --key "C:\Projects\llm_mvp_core\learning-sets\TR07-OMISSION\ru-en\M001\blind.key.json" --review "C:\Projects\llm_mvp_core\learning-sets\TR07-OMISSION\ru-en\M001\review.json" --out "C:\Projects\llm_mvp_core\learning-sets\TR07-OMISSION\ru-en\M001\accepted.jsonl"
python tools/batches.py export --inputs "C:\Projects\llm_mvp_core\learning-sets\TR07-OMISSION\ru-en\M001\accepted.jsonl" --out "C:\Projects\llm_mvp_core\learning-sets\TR07-OMISSION\ru-en\M001\export"
```

Если уже есть другие партии, к check/accept/export добавить --against <пути остальных accepted JSONL>. Проверка связности ограничена переданными файлами; скрытое смысловое родство требует review. Для исправления сохранять draft.v2.jsonl и новый blind/review рядом. При «следующий» сразу перейти к названному ID, не продолжать этот отчёт. При «стоп» прекратить работу.
