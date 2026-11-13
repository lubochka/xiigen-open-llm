# Два корня: инструкции и создаваемые наборы

**Текущее уточнение пользователя #7 имеет приоритет над прежними путями и журналами.**

| Назначение | Точный путь Windows |
|---|---|
| **Создаваемые наборы** | `C:/Projects/llm_mvp_core/learning-sets` |
| Инструкции, карточки, исходные материалы | `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation` |
| Основной скрипт | `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1` |
| Состояние новой кампании | `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/state.json` |
| Сохранённые смысловые семейства | `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/family-material.jsonl` |


Неправильно: вывести output из родительской папки `preparation`. Правильно: preparation находится в planning checkout, а output берётся из явно назначенного data root. `paths.json` и `campaign/registry.json` содержат одинаковое назначение. На чужой платформе нужен явный `--work-root`, чтобы строка `C:/...` не превратилась в относительный каталог.

## Новая кампания

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1" -Action next -WorkRoot "C:/Projects/llm_mvp_core/learning-sets"
```

## Состояние

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1" -Action status -WorkRoot "C:/Projects/llm_mvp_core/learning-sets"
```

`next` сам выбирает A-ID, номер партии и точные пути, возобновляет pending и выдаёт literal commands. Эти команды уже содержат Python executable; не нужно заново искать runtime или активный checkout. Обёртка читает скрипт относительно `$PSScriptRoot`, использует literal paths и UTF-8.

`finish` принимает только точные сохранённые пути `draft` и `family_material`. При завершении записываются размер и `mtime_ns` draft. Повторные `finish/resume` проверяют наличие этого файла и ту же пару размер/время; это лёгкая проверка сохранности, без checksum и без предметного review. Если завершённый draft исчез или изменился, результат — `BLOCKED`, а прежний progress сохраняется. Не обнулять state и не выдавать изменённый файл за прежний успешный результат; новую редакцию назначают явно. Эта проверка не доказывает неизменность содержимого при намеренном сохранении размера и времени.

## Уже начатая попытка

Используй **точный существующий путь request**, выданный прежним запуском, и его `commands.md`. Команда нового runner `resume --request <существующий абсолютный request>` распознаёт старый batch-контракт и возвращает точную команду старого `batches.py resume`; она не копирует данные и не начинает другую генерацию.

Старые записи не удаляются, не переносятся и не засчитываются в новую кампанию автоматически. Если пользователь явно выбрал прежний request, новая кампания не стартует вместо него. У новой кампании отдельный namespace; её отчёт честно сообщает, сколько кандидатов относится именно к ней.

## Ожидание и чтение

Подготовка request должна завершиться до чтения prompt. Используй документацию реально доступного tool: синхронное завершение либо один возвращённый handle и штатный wait/resume. Пустой файл, молчание или timeout транспорта не доказывают завершение или падение. Не стартуй дубль и не оставляй живое ожидание финальным сообщением вместо доступного wait. Не добавляй циклы частого опроса.

При повреждённом выводе читать исходный файл как UTF-8. Не перекодировать исправный файл по одному нечитаемому diff. При бессвязном повторе сохранить trace и вернуться к текущему request/state с коротким входом; причина сбоя по одному UI-журналу не устанавливается.

Модель наследуется из Kiro. Reason internally in English; do not output private reasoning. CREATE_ONLY не выполняет review, обучение и production-проверки. NEXT немедленно прекращает работу над предыдущим ID; STOP прекращает все действия. После успешной партии продолжать до квоты1000, без вопроса «продолжать?».

Граница проверки: Windows-пути назначены пользователем. Эта облачная подготовка не открывала диск C: на компьютере пользователя; проверки инструментов выполняются с временным явно заданным root.
