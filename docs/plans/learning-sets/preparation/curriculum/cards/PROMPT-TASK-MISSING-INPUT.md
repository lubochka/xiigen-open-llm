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
| `A196` — `PROMPT-TASK-MISSING-INPUT` | `prompt.task-compilation` / `base` | 300 | `POOL_END; проверить оставшиеся квоты` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# Явно назвать отсутствующие решающие данные, не сочиняя исполняемое задание на выдуманных входах.

Домен обучения: `prompt.task-compilation`; группа: `orchestration`; цель домена: 1000 кандидатных TRAIN-пар.

## PROMPT-TASK-MISSING-INPUT/base

Явно назвать отсутствующие решающие данные, не сочиняя исполняемое задание на выдуманных входах.

Назначенный предметный домен известен; полного решающего входа нет. Примеры: перевести отсутствующий исходный текст при известном целевом домене, объяснить метод без метода, запрос к неизвестной схеме, потребитель ещё не полученного результата. Chosen сохраняет domain_id, task=null и называет ровно конкретные недостающие поля. Rejected выбирает придуманный текст/схему/результат либо необоснованное значение по умолчанию. Не объявлять версию обязательной, если задача от неё не зависит. Не смешивать эту микроцель с complete-input задачами; это отдельно выделенная квота. В compiler не выполняется ожидание runtime: отсутствие результата описывается как входная зависимость, а не как вымышленный факт.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"domain_id":"назначенный ID","task":null,"missing":["конкретный недостающий вход"]}. Нет фиктивного задания с подставленными данными.

### Простая самопроверка

- Show two different valid inputs consistent with the prompt that would require different answers; this witnesses why clarification is necessary.
- Each missing item is necessary for this exact task, not a generic checklist.
- No fabricated schema, file content, prior result or authorization may appear.

### SW26.prompt.task-compilation.05

**Prompt**

````text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "Объясни результат SQL-запроса на моих данных.",
  "selected_segment": {
    "id": "s1",
    "text": "Объясни результат SQL-запроса на моих данных.",
    "domain_id": "docs.sql",
    "depends_on": []
  },
  "assigned_domain_id": "docs.sql",
  "available_prior_results": {}
}
````

**Chosen**

````text
{"domain_id":"docs.sql","task":null,"missing":["SQL-запрос","схема и данные для результата"]}
````

**Rejected**

````text
{"domain_id":"docs.sql","task":"Объясни, что SELECT * FROM Users возвращает всех пользователей.","missing":[]}
````

**Проверка:** The requested concrete explanation needs the query and relevant input rows; neither is provided. Report those missing inputs.

### SW26.prompt.task-compilation.11

**Prompt**

````text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "Объясни, почему мой метод возвращает null для проблемного вызова.",
  "selected_segment": {
    "id": "s1",
    "text": "Объясни, почему мой метод возвращает null для проблемного вызова.",
    "domain_id": "docs.csharp",
    "depends_on": []
  },
  "assigned_domain_id": "docs.csharp",
  "available_prior_results": {}
}
````

**Chosen**

````text
{"domain_id":"docs.csharp","task":null,"missing":["код метода","входные данные проблемного вызова"]}
````

**Rejected**

````text
{"domain_id":"docs.csharp","task":"Объясни, что метод FindUser возвращает null, потому что пользователь отсутствует в базе.","missing":[]}
````

**Проверка:** A concrete explanation of an unseen method needs its code and the problematic input. A plausible method name or cause is still invented.

Источники проекта: [RAG-015](../../rag/sources/RAG-015.md), [RAG-PROMPT-004](../../rag/sources/RAG-PROMPT-004.md), [RAG-PROMPT-001](../../rag/sources/RAG-PROMPT-001.md). Это источники метода, не сертификат качества пары.

Открытые примеры в этой карточке не засчитываются в 1000 и не являются закрытым EXAM.
