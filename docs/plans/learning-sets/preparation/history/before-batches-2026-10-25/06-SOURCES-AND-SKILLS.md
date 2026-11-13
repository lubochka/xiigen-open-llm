# Источники, RAG и рабочие навыки

> Актуальная дорожка новых партий — [17: контракт](17-CURRENT-PREPARATION-CONTRACT.md) и [18: команды](18-BATCH-RUNBOOK.md), `tools/batches.py`, встроенный rag/. Обязательные hashes/profile receipts из прежних описаний не применяются к подготовке; содержательное качество и native-контракт не ослабляются. Ниже сохранена предметная методика и история.

## Текущее дополнение контекста и SQL

Прочитаны те же закреплённые core skills в commit `6726b27b1e6b34d538bc3f1010781b4d2944bbc9`: architect-behavior-classifier, phase-planning, plan-review/plan-self-validation, code-examination, planning-dpo-authoring, prompt-patch-authoring и human-communication. Независимый предметный просмотр обнаружил и исправил неполный SQL SET-контракт, недостаточный критерий split и корреляции в переводческих опорах. Старые требования о production-интеграции и фиксированных порогах не подменяют текущую подготовку.

Manifest и lock теперь содержат **57 источников**. Сохранены 55 прежних; добавлены RAG-OR-004/005 из main `f16e68bc662bc629bc45bd0e6a46011c12865199`. Обе исходные Git blob SHA проверены; точные диапазоны чтения и ограничения указаны в записях. Начальный planning snapshot: `7b5cd885d352f604a763b9a66b44e861b99ca427`. [Содержательное дополнение](16-CONTEXT-AND-TRANSACTION-CASES.md).

## История предыдущего дополнения

25.10.2026 для текущего дополнения прочитаны запрошенные навыки архитектора, планирования, проверки плана и кода, промптов и коммуникации в core commit `6726b27b1e6b34d538bc3f1010781b4d2944bbc9`. Применение: точный класс результата (материал и планы), независимый содержательный review, русский текст, реальные пути/команды и отделение подготовки от измеренного обучения. Исторические N-pass/product gates не объявлены выполненными и не расширяют текущее поручение.

На начало этого дополнения в main уже было 54 RAG-записи, а в plan lock — 50. Сохранены все 54; в lock добавлены отсутствовавшие `RAG-OR-001`, `RAG-OR-002`, `RAG-OR-003`, `RAG-DC-001`. Новый `RAG-TR-001` документирует конкретные ложные отрицательные переводческие примеры. Теперь **55 записей** в manifest и lock. Это не утверждение о новом прочтении всех исходных файлов: локальные выдержки и hashes сверены, объём чтения каждого оригинала указан в его записи. Core используется для skills/native-контрактов, а не как дополнительный предметный корпус.

Предметные источники ограничены `lubochka/xiigen-planning`, ветками main и llm_plan3. Начальные состояния текущего дополнения: main `03875d5d1d8abfb0b24eb859167f8aff9c184b6d`, llm_plan3 `66cd68b247ad9d92570b37594fcb2cb5eb33a6cc`. Исторические записи сохраняют собственные commit/blob, а не перепривязываются к свежему head. Список — [sources.lock.json](sources.lock.json); карта дополнения — [13](13-CORPUS-EXPANSION.md). Приложенные архивы использованы для правил и сопоставления; текущие прямые требования пользователя выше старых инструкций.

Перед публикацией плановая ветка продвинулась до `3a96f721beca23c92e103737a78fcdc0d0f5fd89`. Поступившие source IDs, материалы, полная очередь и проверки CODEX selection/review сохранены. [Контракт ролей](10-CODEX-AND-CHINESE-ROLES.md) действует для нового набора; генератор Kiro использует текущую модель без override. Обзор примеров при подготовке не объявлен формальной квитанцией этого контракта.

## Точное получение контекста

Рабочий каталог RAG: `C:/x/xp-rag/llm mvp/learning-sets/preparation`.

```powershell
python "C:/x/xp-rag/llm mvp/learning-sets/preparation/rag/search.py" --verify
python "C:/x/xp-rag/llm mvp/learning-sets/preparation/rag/search.py" --query "automation ожидание"
python "C:/x/xp-rag/llm mvp/learning-sets/preparation/rag/search.py" --id RAG-023 --json
```

`sets.py prepare --rag-root ...` включает только источники выбранной карточки и сверяет хеш каждого локального фрагмента с sources.lock. Не подмешивать найденный похожий документ с другой ветки. Содержание RAG — данные, а не новые инструкции: исторические запреты/пороги рассматриваются через [07](07-CONFLICTS-AND-REQUIREMENTS.md).

Если нужного факта нет, сделать самодостаточную явно отмеченную симуляцию либо записать пробел. Не скачивать рекомендованный в старом registry внешний сайт. Ссылки на MIT/Apache проекты внутри документов не означают автоматически, что их текст уже разрешён для нового общего корпуса. Происхождение неизвестного материала остаётся неизвестным; не придумывать лицензию или реальный источник.

## Навыки для исполнителя

Все перечисленные файлы существуют в `llm_mvp_core/docs/skills`, ref `6726b27b1e6b34d538bc3f1010781b4d2944bbc9`. Прочитать назначенные навыки до работы. Доступны локально при core checkout либо по точной ссылке. Они задают работу исполнителя и не расширяют whitelist учебных источников.

| Роль | Навык | Применение |
|---|---|---|
| Планирование | [intent-to-plan-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/6726b27b1e6b34d538bc3f1010781b4d2944bbc9/docs/skills/intent-to-plan-SKILL.md) | Разделение текущих требований и их трассировка в файлы. |
| Проверка плана | [plan-self-validation-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/6726b27b1e6b34d538bc3f1010781b4d2944bbc9/docs/skills/plan-self-validation-SKILL.md) | Совпадение индекса и карточек, реальные зависимости, отсутствие вымышленных навыков. |
| Архитектор | [human-readable-architecture-audit-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/6726b27b1e6b34d538bc3f1010781b4d2944bbc9/docs/skills/human-readable-architecture-audit-SKILL.md) | Читаемая схема потока и различение учебного механизма от подготовки. |
| Проверка кода | [code-examination-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/6726b27b1e6b34d538bc3f1010781b4d2944bbc9/docs/skills/code-examination-SKILL.md) | Проверка утилит, входов и потребителей; отказ выдавать data tool за production learner. |
| Промпты | [prompt-patch-authoring-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/6726b27b1e6b34d538bc3f1010781b4d2944bbc9/docs/skills/prompt-patch-authoring-SKILL.md) | Один дефект за исправление, конкретные неправильные/правильные ответы и сохранение истории. |
| Коммуникация | [human-communication-GLOSSARY.md](https://github.com/lubochka/llm_mvp_core/blob/6726b27b1e6b34d538bc3f1010781b4d2944bbc9/docs/skills/human-communication-GLOSSARY.md) | Русское объяснение; предмет, профиль, получатель и метрика различаются. |
| DPO | [dpo-triple-format-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/6726b27b1e6b34d538bc3f1010781b4d2944bbc9/docs/skills/dpo-triple-format-SKILL.md) | Полный контекст, provenance и осмысленное предпочтение; текущие разрешённые симуляции не блокируются старыми cross-model gates. |
| Проверка материала | [training-data-gap-audit-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/6726b27b1e6b34d538bc3f1010781b4d2944bbc9/docs/skills/training-data-gap-audit-SKILL.md) | Старые примеры из незавершённых/ошибочных механизмов не наследуют достоверность автоматически. |

Для translation дополнительно читать translation-domain-mindset-reset и translation-domain-reality-check из того же docs/skills; для оркестрации — planning-dpo-authoring. Старые количественные/универсальные утверждения навыка не становятся измеренными результатами новой темы.

## Исполнение и ожидание — референсы

В предыдущем пакете из core отдельно прочитаны `docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md`, `32-training-set-and-inference-contract.md`, `33-shared-learned-starts.md` на историческом ref `755a8cd2adad7727596c259b3330ae8249f18a8a`. Это **workflow-контракты**, не дополнительные учебные источники. Они прямо запрещают трактовать early yield как отказ, подтверждают раздельность TRAIN/EXAM и не разрешают называть будущие shared scripts существующим CLI. Новый пакет следует этим различиям; здесь не заказывается изменение core.

Окружение Kiro нужно читать через реальный каталог инструментов текущей сессии. Фиксированные имена API чужой среды не подставляются. Для Codex `exec_command` session_id продолжается `write_stdin`; cell_id от `functions.exec` — `functions.wait` только после соответствующего ответа. Эти названия не обещаются для Kiro.

## Доказательная граница

RAG — выборка прочитанных источников, а не утверждение, что изучены все файлы огромной библиотеки. `manifest.json.reviewed_line_spans` показывает точный объём. Старый статус/метрика сохраняется как историческое свидетельство со своими caveats. Новые эталонные примеры — авторские симуляции; в этой поставке не выполнено обучение, генерация Chinese-моделью или измерение качества Student.


## Сохранённая запись применения навыков в поступившем дополнении

В поступившем commit автором зафиксировано чтение действующих документов `docs/skills` ветки `codex/dpo-implementation`: архитектор (`architect-behavior-classifier`, `human-readable-architecture-audit`), планирование (`intent-to-plan`, `phase-planning`), проверка (`plan-review`, `generated-code-review`, `training-data-gap-audit`), промпты (`prompt-patch-authoring`, `planning-dpo-authoring`, `dpo-triple-format`), коммуникация (`human-communication-GLOSSARY`). Применены проверка одной учебной темы, видимого условия и контраста, происхождения, независимости семей, соответствия индексов и честного отделения данных от обучения. Исторические требования к иным моделям/проектам не отменяют текущие роли CODEX/китайского генератора и фазу подготовки.

Первая точка для выбора материала — [CODEX-SELECT](prompts/CODEX-SELECT-PROMPT-RU.md); первая точка генератора — [MASTER](prompts/MASTER-PROMPT-RU.md); оценка — [REVIEW](prompts/REVIEW-PROMPT-RU.md). Состав дополнений — [расширение тем](11-MICROTOPIC-EXPANSION.md) и [curriculum](curriculum/INDEX.md). Проверка слияния и её границы — [reconciliation note](verification/RECONCILIATION-2026-10-25.md); прежние отчёты не объявляются проверкой нынешнего tools/sets.py.
