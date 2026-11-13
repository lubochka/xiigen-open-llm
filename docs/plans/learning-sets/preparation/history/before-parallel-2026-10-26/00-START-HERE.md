# Начать создание DPO

**Цель: 1000 кандидатных пар на каждый из 21 домена; всего 21 000. CREATE_ONLY, без review.**

1. Если уже назначен существующий request, возобнови его по сохранённому абсолютному пути. Не начинай новую кампанию вместо него. Прямой NEXT/новый ID немедленно заменяет предыдущую задачу; её отчёт больше не дописывается. STOP прекращает действия.
2. Для новой кампании выполни команду ниже. Инструмент назначит микротему, точное число строк и семей, источники и пути. Профили и микротемы входят в доменную квоту.
3. Прочитай только выданные `prompt.md`, `request.json`, `material.required.json` и `commands.md`. Предметные примеры и RAG уже включены. Полные skills, исторические аудиты и весь репозиторий перечитывать не нужно.
4. Создай реальные DPO-пары и материал отсутствующих семейств. Сохраняй одно намерение, один формат двух ответов, одну содержательную ошибку rejected. Новые EN/RU/HE варианты одного сюжета используют общий сохранённый материал.
5. Выполни точный `finish`, затем `next` из `commands.md`. Продолжай, пока все доменные квоты не достигнуты. Не спрашивай разрешения на каждую следующую партию. Конкретный внешний блок или прямой STOP нельзя скрывать фиктивным DONE.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1" -Action next -WorkRoot "C:/Projects/llm_mvp_core/learning-sets"
```

| Назначение | Точный путь Windows |
|---|---|
| **Создаваемые наборы** | `C:/Projects/llm_mvp_core/learning-sets` |
| Инструкции, карточки, исходные материалы | `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation` |
| Основной скрипт | `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1` |
| Состояние новой кампании | `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/state.json` |
| Сохранённые смысловые семейства | `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/family-material.jsonl` |


Обёртка использует свой каталог для поиска `campaign.py`, а назначенный `-WorkRoot` — для данных. Первый вызов использует `py -3`; если известный Python установлен иначе, параметр `-PythonExecutable` задаёт его точный путь. Дальнейшие команды уже содержат фактический `sys.executable`. Они работают из любой текущей папки.

**Language rule:** Reason internally in English. Do not output private reasoning. Russian progress; learner text follows the assigned profile. Модель — текущая выбранная китайская в Kiro, без override.

Не запускай независимую проверку данных, blind, acceptance, export/import, обучение, production tests или checksum-гейты. Самопроверка правильности составляемой строки остаётся частью создания; `finish` не доказывает смысловую правильность и обучение.

Если работающий инструмент вернул handle, используй его документированный wait/resume до terminal. Зависимое чтение выполняется после завершения записи. Пустой stdout/transport timeout не доказывает падение. Не запускай дубль и не заканчивай раунд вместо доступного ожидания. Имена и параметры Kiro tools берутся из реально доступной документации, а не из вымышленного примера.

[Очередь](EXECUTION-QUEUE.md) · [контракт](17-CURRENT-PREPARATION-CONTRACT.md) · [точные пути и resume](20-CREATE-ONLY-AND-PATHS.md) · [справочник автора](campaign/AUTHORING-PROTOCOL-EN.md).
