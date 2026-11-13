# Источники и навыки исполнителя

Применение навыков в текущем дополнении: источники → конкретное задание → полные примеры → точные пути и очередь. [Распределение ролей](10-CODEX-AND-CHINESE-ROLES.md) сохраняет CODEX с Astra 6 ultracode для выбора и оценки; CREATE_ONLY не запускает эти отдельные действия. [Предметная карта RAG](rag/SUBJECT-PACK-MAP.md) связывает новые цели с прочитанными фрагментами. Это применение скилов авторинга, не выполненный review DPO.

Действующий этап — подготовка учебного материала. [Контракт 17](17-CURRENT-PREPARATION-CONTRACT.md) и [команды 18](18-BATCH-RUNBOOK.md) применяют прямые условия Любы: без обязательных checksum, фиксированного имени модели, подписанных receipts и полной production-цепочки. Режим CREATE_ONLY заканчивается записью кандидатов и структурным finish. Содержательная проверка и действительный native-контракт сохранены для отдельно порученного будущего этапа.

## Разрешённый материал

Предметные источники — только `lubochka/xiigen-planning` в ветках `main` и `llm_plan3`. Core используется как источник навыков и интерфейсных контрактов, а не как дополнительный предметный корпус. Внешние ссылки внутри оригинала не расширяют этот список.

В [RAG manifest](rag/manifest.json) и [source lock](sources.lock.json) согласованы 58 ID. Сохранены прежние 57 выдержок; добавлен [RAG-DATA-001](rag/sources/RAG-DATA-001.md). Все 28 выбранных полных main-оригиналов находятся в [rag/originals](rag/originals/INDEX.md); [карта переноса](rag/transfer-map.json) сохраняет путь и commit каждого файла. Для генератора не нужен отдельный checkout main. Hash-поля сохранены для происхождения, но не являются предварительным гейтом подготовки.

Не утверждается полное прочтение всего репозитория. У каждой выдержки есть ограниченный объём, назначение и оговорки. Исходный `PASS`, `train_ready`, готовый score либо regex-проверка не являются доказательством правильной DPO-разметки или работающего алгоритма.

## Какие навыки и зачем читать

Ниже закреплены действительные имена файлов core на проверенном ref `b6c0fff7d57dc9ad3e87e27d884476994fde7e16`. Перед выполнением своей роли прочитать соответствующий навык адресно. Название документа не является заявлением, что проведены все перечисленные в нём production-процедуры или многократные ревью.

| Работа | Навык | Применение в этом пакете |
|---|---|---|
| Запуск задачи | [planning-session-startup](https://github.com/lubochka/llm_mvp_core/blob/b6c0fff7d57dc9ad3e87e27d884476994fde7e16/docs/skills/planning-session-startup-SKILL.md) | Зафиксировать актуальный scope CREATE_ONLY, готовый request, абсолютные пути и продолжение после сокращения контекста; не запускать старые production-процедуры |
| Построение плана | [intent-to-plan](https://github.com/lubochka/llm_mvp_core/blob/b6c0fff7d57dc9ad3e87e27d884476994fde7e16/docs/skills/intent-to-plan-SKILL.md) | Отделить текущее поручение о данных от прежнего аудита и production-исправлений |
| Архитектура | [human-readable-architecture-audit](https://github.com/lubochka/llm_mvp_core/blob/b6c0fff7d57dc9ad3e87e27d884476994fde7e16/docs/skills/human-readable-architecture-audit-SKILL.md) | Показать фактический путь prompt → набор → будущий learner; различать router и предметную Unit |
| Архитектурная классификация | [architect-behavior-classifier](https://github.com/lubochka/llm_mvp_core/blob/b6c0fff7d57dc9ad3e87e27d884476994fde7e16/docs/skills/architect-behavior-classifier-SKILL.md) | Различить учебный механизм, контракт и служебное ограничение |
| Этапы | [phase-planning](https://github.com/lubochka/llm_mvp_core/blob/b6c0fff7d57dc9ad3e87e27d884476994fde7e16/docs/skills/phase-planning-SKILL.md) | Генерация, review, native import и обучение имеют самостоятельные результаты |
| Проверка плана | [plan-self-validation](https://github.com/lubochka/llm_mvp_core/blob/b6c0fff7d57dc9ad3e87e27d884476994fde7e16/docs/skills/plan-self-validation-SKILL.md), [plan-review](https://github.com/lubochka/llm_mvp_core/blob/b6c0fff7d57dc9ad3e87e27d884476994fde7e16/docs/skills/plan-review-SKILL.md) | Один порядок, реальные пути, сохранённые попытки, нет круговых блокировок |
| Проверка утилит | [code-examination](https://github.com/lubochka/llm_mvp_core/blob/b6c0fff7d57dc9ad3e87e27d884476994fde7e16/docs/skills/code-examination-SKILL.md) | Проверить конкретные входы/выходы и регрессии подготовки; не выдавать smoke за обучение |
| Разбор сгенерированного кода и контрактов | [generated-code-review](https://github.com/lubochka/llm_mvp_core/blob/b6c0fff7d57dc9ad3e87e27d884476994fde7e16/docs/skills/generated-code-review-SKILL.md) | Для автора утилит: реальные input/output и потребители; для автора примеров: точный видимый контракт. Навык не назначает review кандидатных пар, oracle или production-тесты в CREATE_ONLY |
| Промпты | [prompt-patch-authoring](https://github.com/lubochka/llm_mvp_core/blob/b6c0fff7d57dc9ad3e87e27d884476994fde7e16/docs/skills/prompt-patch-authoring-SKILL.md) | Узкая цель, полный видимый контекст, правдоподобное одно различие, примеры |
| DPO для планирования | [planning-dpo-authoring](https://github.com/lubochka/llm_mvp_core/blob/b6c0fff7d57dc9ad3e87e27d884476994fde7e16/docs/skills/planning-dpo-authoring-SKILL.md) | Один сценарий решения, допустимые и недопустимые действия, контроли одной темы |
| Формат DPO | [dpo-triple-format](https://github.com/lubochka/llm_mvp_core/blob/b6c0fff7d57dc9ad3e87e27d884476994fde7e16/docs/skills/dpo-triple-format-SKILL.md) | Prompt/chosen/rejected, источники и обоснование предпочтения без подмены ответа critique |
| Перевод | [translation-domain-mindset-reset](https://github.com/lubochka/llm_mvp_core/blob/b6c0fff7d57dc9ad3e87e27d884476994fde7e16/docs/skills/translation-domain-mindset-reset-SKILL.md) | Семантика, порядок, морфология, раскладка и их доменные ограничения |
| Коммуникация | [human-communication-GLOSSARY](https://github.com/lubochka/llm_mvp_core/blob/b6c0fff7d57dc9ad3e87e27d884476994fde7e16/docs/skills/human-communication-GLOSSARY.md) | Отчёт по-русски; не смешивать язык, профиль, домен, тему, получателя и метрику |

При подготовке этого пакета адресно читались методики и прикладные контракты. Проверки утилит/индекса не являются ревью сгенерированного корпуса и не поручаются текущему генератору. Полные многопроходные production-аудиты не заявлены. Старое требование навыка о другом профиле, двух генераторах, фиксированном score или сборке всего продукта не отменяет прямых условий нынешней задачи. Не изменять сами навыки ради исключения.

## Минимальный комплект для роли

Генератор читает MASTER, созданный prompt/request, назначенную карточку и включённые RAG-фрагменты; навык промптов и DPO уточняет методику, но не добавляет посторонние темы. Только в отдельно порученной будущей стадии reviewer читает BATCH-REVIEW, blind-пакет, предметную рубрику и источники без ключа/разметки генератора. Автор будущего учебного пилота отдельно читает 05 и native boundary, проверяет настоящий B1 и фактического потребителя данных.

Ссылки: [MASTER](prompts/MASTER-PROMPT-RU.md), [BATCH-GENERATE](prompts/BATCH-GENERATE-PROMPT-RU.md), [BATCH-REVIEW](prompts/BATCH-REVIEW-PROMPT-RU.md), [русский отчёт](08-REPORT-TEMPLATE.md). При живом процессе исполнитель сначала находит документированный инструмент wait/resume этой среды и сохраняет handle; sleep/polling не заменяет ожидание. Точное NEXT пользователя прекращает работу над прежней задачей.

Предыдущая версия страницы со старыми source pins и публикационными заявлениями сохранена [в истории](history/before-batches-2026-10-25/06-SOURCES-AND-SKILLS.md).


## Навыки текущего дополнения

Для поставки25 прочитаны проектные инструкции из core commit `f86cb4ad2f85214be193fb8bf3640072d947b37c`:

- [human-readable-architecture-audit-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/f86cb4ad2f85214be193fb8bf3640072d947b37c/docs/skills/human-readable-architecture-audit-SKILL.md).
- [phase-planning-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/f86cb4ad2f85214be193fb8bf3640072d947b37c/docs/skills/phase-planning-SKILL.md).
- [plan-review-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/f86cb4ad2f85214be193fb8bf3640072d947b37c/docs/skills/plan-review-SKILL.md).
- [code-examination-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/f86cb4ad2f85214be193fb8bf3640072d947b37c/docs/skills/code-examination-SKILL.md).
- [prompt-patch-authoring-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/f86cb4ad2f85214be193fb8bf3640072d947b37c/docs/skills/prompt-patch-authoring-SKILL.md).
- [human-communication-GLOSSARY.md](https://github.com/lubochka/llm_mvp_core/blob/f86cb4ad2f85214be193fb8bf3640072d947b37c/docs/skills/human-communication-GLOSSARY.md).

Применены ясное описание предмета, конкретные good/bad примеры, реальные пути, единый порядок и честная граница измерений. Прямое поручение CREATE_ONLY выше production-процедур из навыков: checksum, CI, дополнительные review и фиксированный model alias не становятся условиями создания. Сами skills этим пакетом не менялись.
