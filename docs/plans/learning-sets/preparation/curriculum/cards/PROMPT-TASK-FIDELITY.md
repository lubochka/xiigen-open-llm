<!-- CURRENT-PARALLEL-CONTRACT-20261026 -->
## Текущий порядок исполнения — 26.10.2026

Эта карточка используется в CREATE_ONLY-кампании по 1000 кандидатных пар на домен; её микротемы распределяют квоту, не добавляют ещё 1000 каждая. Старые профили/примеры ниже сохранены; текущие количества и IDs задаёт [реестр](../../campaign/registry.json).

**Не завершать работу после одной партии:** сохранить реальные строки, выполнить выданный `finish`, затем `next`; продолжать до квоты назначенной группы. Прямой STOP прекращает действия, прямой NEXT немедленно меняет задание. Не дописывать старый отчёт после переключения. Реальный внешний блок назвать точно, не объявлять DONE.

Общий статус: `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/status.json`; состояние-владелец: соседний `state.json`. Перед записью получить назначение с уникальным `agent-id`; работать только со своим owner/token. После сохранения полных строк выполнить команду `progress`; после партии — `finish`. Вручную эти JSON не редактировать. Старое время отметки не разрешает захват чужой работы. Сам файл статуса не синхронизирует разные компьютеры/копии папки.

Точные пути к текущему prompt, commands и material берутся из ответа координатора. Для текущего владельца это `coordination-prompt.md`, `coordination-commands.md`, `coordination-material.required.json`; старый request/draft сохраняется. Новые наборы — `C:/Projects/llm_mvp_core/learning-sets`; инструкции — `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`.

Reason internally in English; do not output private reasoning. Прогресс по-русски, учебный текст по назначенному профилю, текущая модель Kiro без override. Нужные предметные примеры и локальные RAG-фрагменты включены в назначение. Только проектные источники либо конкретно подтверждённые MIT/Apache-2.0; чужую лицензию не выводить из названия репозитория. Не добавлять checksum, independent review, обучение, export/import и production CI к созданию.

Работающий tool: сохранить один handle, пользоваться его документированным wait/resume до terminal. Не перезапускать по пустому stdout, не подменять ожидание быстрым повтором запросов и не завершать раунд вместо доступного ожидания.

| Назначение | Домен / профиль | Пар в этой доле | Следующее в группе |
|---|---|---:|---|
| `A195` — `PROMPT-TASK-FIDELITY` | `prompt.task-compilation` / `base` | 700 | `A196` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# Составить короткое самодостаточное задание, сохранив предмет, все значимые ограничения и входы.

Домен обучения: `prompt.task-compilation`; группа: `orchestration`; цель домена: 1000 кандидатных TRAIN-пар.

## PROMPT-TASK-FIDELITY/base

Составить короткое самодостаточное задание, сохранив предмет, все значимые ограничения и входы.

Ученик получает полный исходный пользовательский запрос, выбранный фрагмент, явно назначенный domain_id и доступные входы/результаты предыдущих действий. Все решающие данные для этой микроцели присутствуют. Chosen убирает только разговорную избыточность: сохраняет действие, отрицания, знак сравнения, числа, единицы, имена, язык и формат ответа, версии когда они заданы, требование не менять данные/код и общий контекст полного запроса. Нельзя выполнить задачу вместо её формулирования или сменить назначенный домен. Rejected меняет ровно одно решающее условие. Не требуй отсутствующий вход, если по показанному контракту он не нужен. Компактность не измеряется произвольным потолком символов и не оправдывает потерю контекста.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"domain_id":"назначенный ID","task":"короткое полное задание","missing":[]}. Task — инструкция исполнителю, не код/перевод/ответ документации.

### Простая самопроверка

- Make an action-input-constraint-output list from the full source, then compare it to task. Every decisive item must remain.
- Use one boundary witness: =1 vs =0, < vs <=, null vs empty, or source vs required target language.
- Remove the original prompt mentally: the task must remain executable from its explicit inputs; do not drop a shared condition attached elsewhere.

### SW26.prompt.task-compilation.01

**Prompt**

````text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "Пожалуйста, нужен один SELECT для SQL Server 2019. Таблица Users(Id int, Active bit). Верни только Id активных строк (Active=1), без удаления или изменения данных и без пояснений.",
  "selected_segment": {
    "id": "s1",
    "text": "Пожалуйста, нужен один SELECT для SQL Server 2019. Таблица Users(Id int, Active bit). Верни только Id активных строк (Active=1), без удаления или изменения данных и без пояснений.",
    "domain_id": "code.sql",
    "depends_on": []
  },
  "assigned_domain_id": "code.sql",
  "available_prior_results": {}
}
````

**Chosen**

````text
{"domain_id":"code.sql","task":"SQL Server 2019: напиши один SELECT Id из Users(Id int, Active bit) для Active=1. Данные не менять; ответ только SQL.","missing":[]}
````

**Rejected**

````text
{"domain_id":"code.sql","task":"SQL Server 2019: напиши один SELECT Id из Users(Id int, Active bit) для Active=0. Данные не менять; ответ только SQL.","missing":[]}
````

**Проверка:** Compare the compiled predicate with the original: Active=1 must remain 1, not 0.

### SW26.prompt.task-compilation.02

**Prompt**

````text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "Сделай только C# 10 метод string Label(string? s), nullable enabled. Только null заменяется на \"(нет)\"; пустую и пробельную строку оставь буквально, без Trim.",
  "selected_segment": {
    "id": "s1",
    "text": "Сделай только C# 10 метод string Label(string? s), nullable enabled. Только null заменяется на \"(нет)\"; пустую и пробельную строку оставь буквально, без Trim.",
    "domain_id": "code.csharp",
    "depends_on": []
  },
  "assigned_domain_id": "code.csharp",
  "available_prior_results": {}
}
````

**Chosen**

````text
{"domain_id":"code.csharp","task":"C# 10, nullable enabled: напиши только string Label(string? s). Null→\"(нет)\"; все другие строки вернуть без изменений и Trim.","missing":[]}
````

**Rejected**

````text
{"domain_id":"code.csharp","task":"C# 10, nullable enabled: напиши только string Label(string? s). Null и пустую строку→\"(нет)\"; остальные строки вернуть без Trim.","missing":[]}
````

**Проверка:** Use the empty string as a witness. The source requires preserving it; the bad task changes it.

Источники проекта: [RAG-015](../../rag/sources/RAG-015.md), [RAG-PROMPT-004](../../rag/sources/RAG-PROMPT-004.md), [RAG-PROMPT-001](../../rag/sources/RAG-PROMPT-001.md). Это источники метода, не сертификат качества пары.

Открытые примеры в этой карточке не засчитываются в 1000 и не являются закрытым EXAM.
