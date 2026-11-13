# Уточнение задания уже работающей Kiro-сессии: создать DPO для обучения

Роль человека при выборе учебного материала выполняет **CODEX с Astra 6 ultracode**. Оценку качества DPO также выполняет **CODEX с Astra 6 ultracode**. Внешняя открытая китайская модель через Kiro создаёт `chosen/rejected`. Сейчас выполняется CREATE_ONLY по уже назначенным карточкам: новые вызовы выбора/оценки не запускаются и не задерживают создание следующей партии. Назначение роли не является доказательством выполненного модельного вызова.

Этот промпт передаётся исполнителю, который работал по старым инструкциям. Он уточняет цель и порядок текущей подготовки, сохраняет созданные файлы и не назначает повторную генерацию уже сделанной партии. Основание: [разбор ce76](../22-DPO-LEARNING-TASK-AND-CE76.md).

Новые задания включают примеры через BATCH-GENERATE; старые prompt.md не изменяются автоматически. Этот reset передаёт короткую авторскую подсказку начатой сессии. Обновление текста само не доказывает улучшения качества: новый измеренный результат здесь не заявляется.

```text
CONTINUE THE CAMPAIGN: 192 streams, at least 192,000 real unique candidate
pairs. Completing M008 or another wave does not end this assignment.
After finish, immediately take the next unfinished stream; after END,
start another pass for streams below target. Do not end the working turn
with questions about continuation, commit/push, or 10 versus 25–50 pairs.
Use the existing request.count / queue candidate_count. Follow an existing
publication order; otherwise save locally and continue. Do not invent work.
Within the same scenario plan use a new run and -Previous. For a new assigned
scenario file use -FamilyPlan and -Start 0 without -Previous. Preserve groups:
router 1008, other streams 1000, minimum total 192008 for the 192000 goal.
User STOP/NEXT still applies. Report a genuine blocker, not a preference.
Keep visible operational explanations in English; do not switch to Chinese.
Reports for Luba and rationale remain Russian; learner data follow the profile.
Incoherent repeated output is a failed response, never data or progress.
Continue from the saved request/state after recovery, not from zero.
C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/27-CAMPAIGN-CONTINUATION-CONTRACT.md

ТЫ СОЗДАЁШЬ УЧЕБНЫЕ ДАННЫЕ ДЛЯ МОДЕЛЕЙ/UNIT XIIGEN.
Ты — автор данных, будущая модель XIIGEN — ученик. Для назначенной темы
создай реальные DPO-тройки: prompt, chosen, rejected.

prompt — полная предметная задача ученику; chosen — полный желаемый ответ;
rejected — полный правдоподобный ответ на тот же вход с одной заданной ошибкой.
Пара задаёт предпочтение, которое позже использует обучающий механизм.
Задание «создай DPO», схема JSON, перечень тем, отчёт, названия файлов и
пустые заполнители не являются учебной парой. Не помещай их вместо
предметной задачи и ответов ученика.

Прочитай учебную цель и response contract назначенной карточки.
В переводе ученик должен переводить и сохранять смысл, в документации —
объяснять фактическое поведение кода, в коде — реализовывать видимый контракт,
в оркестрации — выдавать нужный план/действие/маршрут по входному состоянию.
Это разные назначенные потоки; не смешивай их в одной партии.
Используй предметные reference-примеры как образец постановки, не копируй
их в новый корпус под свежими ID.

Для одной опечатки сначала сделай простое действие на слове из предложения:
письмо(6) -> писмо(5) — удаление; пильмо(6) — не удаление.
Верни удалённую букву на её место: исходное слово и регистр должны совпасть.
market -> maket: index2, не3. В «Возьми ручку» правильно ручку -> рукчу;
ручка -> рукча меняет другую форму и не подходит этому предложению.
Обратная операция должна вернуть всё предложение, не только слово:
She saw a cat. -> She saw a cap. сохраняет остальной текст;
She saw the cap near the door. меняет также артикль и место.
VALIDWORD учит сохранять уже правильное слово: She mentioned a sword.
-> Она упомянула меч.; rejected Она упомянула слово. естественен, но неверен.
Не добавляй пять строк с восстановлением: контрпример не назначает квоту 5+5.
Сохраняй все прочие факты: שולחנו -> его стол, не наш; school entrance
-> вход школы, без «главный». Источник форм иврита приведён в шпаргалке.
Разобранные good/bad-примеры без поиска:
C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/prompts/TYPO-AUTHORING-EXAMPLES-RU.md
Это помощь автору в текущей задаче, не отдельный review или новая стадия.
Старые файлы не переписывай из-за подсказки; продолжай назначенный CREATE_ONLY.

CREATE_ONLY: сейчас создать данные для обучения. Обучение сейчас не запускать.
Кандидатный статус означает стадию приёмки и не отменяет качества содержания.
Для каждой строки задание должно определять предпочтение по видимым фактам.
Rationale, error_tag и служебные family ID не являются ответом ученика.
В опечатках и раскладках реальный повреждённый ввод находится в prompt;
семейства сохраняются в metadata. Metadata сама не обучает kNN/токенизатор.

ИСТОЧНИК И ТОЧНЫЕ ПУТИ:
xiigen-planning, ветка llm_plan3, checkout C:/Users/lubao/Documents/xiigen
preparation: C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation
Все новые наборы: C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets
Рабочая папка llm_mvp_core не меняет корень наборов.

Прочитай актуальные prompts/MASTER-PROMPT-RU.md и
prompts/BATCH-GENERATE-PROMPT-RU.md в этом preparation.
В начале batch теперь есть раздел «Что ты создаёшь для обучения XIIGEN».
Если локальная копия старее, получи эти конкретные файлы и текущий контракт
из llm_plan3 доступным GitHub-инструментом. Main целиком не скачивай;
не создавай заглушки источников, не вводи checksum-процедуру.

Прямой stream ID пользователя имеет приоритет. Для новой партии бери точную
команду из execution-queue.json и существующую create_sets.ps1.
Она только собирает prompt/request: после prepare ты обязан сам создать
назначенные реальные строки draft.jsonl. Count относится к одной партии.
10 строк не означают завершения всего корпуса или всех доменов.

Если текущая попытка существует, сохраняй её файлы и исходные count/family/split.
Сохранённый старый prompt.md автоматически не обновляется: применяй это
уточнение цели и актуальный batch как инструкции, сохраняя предметное
назначение request. Не переписывай прошлый draft только ради нового статуса.
Старые команды blind/review/accept/export не назначают их запуск сейчас.
Данные старой M001 в llm_mvp_core не удаляй и не пересоздавай автоматически.
Их наличие отдельно от нового корня не доказывает native импорт или завершение
очереди. Перенос и исправление старой партии не подменяют названный Любой
следующий stream. Если она разрешила продолжать после M001, следующий ID
по очереди — DOC-CS-NULL/base; его точную команду возьми из registry.

Для текущей новой партии запиши ровно request.count полных строк xiigen.dpo.v1
по назначенным families и split в request.paths.draft. Используй фактически
выбранную китайскую модель Kiro; не меняй её и не выдумывай model identity.
Один генератор вправе создать обе стороны. Не переименовывай модельное
авторство в DeterministicGenerator/OpenTeacher ради native admission.

После записи выполни один finish из актуального commands.md этой новой
попытки и переходи дальше по поручению/очереди. Ни независимое review,
ни обучение, ни изменение production-кода сейчас не выполняются.
Finish подтверждает структуру и количество; он не доказывает обучение.

В коротком сообщении покажи: чему учится модель на этих строках, сколько
реальных строк создано, фактический путь, текущий следующий ID. Приведи одну
реальную записанную тройку, если это нужно для пояснения; не выдумывай
пример якобы из draft и не заменяй создание отдельным циклом отчётов.
Фактический прогресс кампании и запланированную общую квоту называй отдельно.

При живом процессе продолжай один handle штатным wait/resume до terminal.
Не запускай дубль, не заканчивай ход вместо ожидания и не делай частый polling.
При NEXT сразу перейди к указанному ID, не дописывая прежний отчёт.
При STOP прекрати действия. Старые данные и эксперименты сохраняй.
```

## Точная команда следующего потока после M001

Использовать, когда текущее поручение — продолжение после первой партии и попытка M002 ещё не существует:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "DOC-CS-NULL/base" -Run "M002" -Count 10 -Split train -Start 0
```

Если M002 уже существует, продолжить её по фактическому request:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Action resume -Request "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/DOC-CS-NULL/base/M002/request.json"
```

Эти команды не выполнялись в документной сессии уточнения. Старый M001 остаётся исторической попыткой; его действительные строки не прочитаны через Git LFS и не объявлены проверенными.

