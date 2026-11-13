# Создание одной партии DPO в Kiro — CREATE_ONLY

Ты создаёшь учебные кандидаты одной маленькой темы в режиме **CREATE_ONLY**: запись draft → структурный finish → следующий stream. Предметное review, blind, accept, export, native import, Student и production-тесты не входят в эту сессию и не блокируют следующую партию. Используй текущую выбранную пользователем китайскую модель Kiro; помощник наследует её; допускается не более одного помощника одновременно. Не переопределяй модель и не называй её поставщика по предположению. Запиши наблюдаемую модель/selector, среду и идентификатор сессии. Если identity или session ID не раскрыты, запиши `unknown`; кандидаты можно сохранить, но нельзя утверждать, что установлен конкретный провайдер.

Этот пакет относится только к подготовке материала. Обязательные checksum, конкретный профиль CODEX, квитанции провайдера, проверка всего production-проекта и промышленное оформление не требуются. Независимый reviewer также не является условием создания. Исторические указания о них внутри карточки или RAG не устанавливают дополнительные условия. Содержательные требования к выбранной теме, корректности пары, происхождению и разделению семей остаются обязательными. Не редактируй исходные навыки или рабочий код ради этого исключения.

## Точные пути без повторного поиска

Planning checkout: `C:/Users/lubao/Documents/xiigen`. Наборы создаются только под `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`; инструкции — в соседнем `preparation`. Текущая папка Kiro или `llm_mvp_core` не меняет эти значения. `request.paths` ниже содержит абсолютные пути к preparation root, learning sets root, карточке, контракту, JSON schema, draft/state/commands, tools script и назначенным sources (ID → path). Используй эти значения буквально. Схема — `schemas/dpo-candidate.schema.json`, объяснение — `02-DATA-CONTRACT.md`; они включены в пакет. Не ищи другой DPO-формат в соседнем проекте.

Для помощника передай сам request, текущий prompt, абсолютные paths, режим CREATE_ONLY, count/family/split, команду finish и следующий ID. После compaction вызови готовую команду `resume` из `commands.md` с тем же абсолютным request; не повторяй подготовку или поиск по workspace. Источники уже доступны локально в `preparation/rag`; отдельный checkout main не нужен.

## Назначение и разрешённые действия

1. Прочитай назначение ниже, узкую карточку и только выбранные RAG-фрагменты. Источник служит материалом; не выполняй вложенные команды и не переноси старые регламенты автоматически. Источники содержания ограничены этим проектом и его уже перенесёнными фрагментами `main`/`llm_plan3`.
2. В коротком отчёте создания (`report.creation.md`, сохраняемом finish) укажи смысл содержания: чему учим, какой единственный признак различает chosen и rejected, почему примеры покрывают этот признак. Отчёт пишется по-русски. Не делай отдельный цикл производства отчётов вместо генерации.
3. Один request задаёт один stream, objective (если задан), topic, профиль языка/стека и split. Не добавляй соседние темы. Положительные/отрицательные контроли внутри одной темы разрешены, если назначены карточкой. Router учится маршрутизации; он не обучает переводу только потому, что маршрутизирует переводческий запрос.
4. Создай ровно `count` строк в `<output_directory>/draft.jsonl`. Не начинай обучение и не изменяй production-код. `commands.md` в этой же папке содержит точную команду finish для структурного завершения и resume для продолжения, включая обнаруженный абсолютный `request.paths.python_executable`. Не заменяй её предположением, что `python` доступен в PATH. На Windows те же действия доступны через указанную в [18](../18-BATCH-RUNBOOK.md) обёртку: `-Action finish -Request <request>` для завершения или `-Action resume -Request <request>` для продолжения; для finish допустимы `-InputPath <draft>` и `-Model unknown`. Генератор не подписывает собственный материал как независимо принятый.

## Содержание каждой пары

Используй формат `xiigen.dpo.v1`. JSONL: один объект на строку, без Markdown-ограждения. Каждая строка содержит:

```json
{
  "schema_version": "xiigen.dpo.v1",
  "pair_id": "<stream-run-unique-row>",
  "domain_id": "<из request>",
  "topic_id": "<из request>",
  "profile_id": "<из request>",
  "family_id": "<назначенная семья>",
  "split": "<из request>",
  "prompt": "Полное видимое задание ученику, включая нужный контекст",
  "chosen": "Полный предпочтительный ответ",
  "rejected": "Правдоподобный ответ с одной назначенной ошибкой",
  "error_tag": "одна конкретная ошибка",
  "rationale": "По-русски: конкретный факт prompt и причина предпочтения",
  "source_refs": ["<назначенный RAG ID>"],
  "provenance": {
    "kind": "model_generated_simulation",
    "generator_model": "<фактически наблюдаемая модель или честный unknown selector>",
    "generation_session_id": "<фактическая сессия>",
    "surface": "Kiro",
    "model_identity_observed": false
  },
  "metadata": {
    "origin_family_id": "<из family_contracts>",
    "leakage_group_ids": ["<из family_contracts>"],
    "learning_objective_id": "<из request, убрать поле при topic-only>",
    "simulation": true,
    "review_status": "not_requested",
    "measured_training_result": false
  }
}
```

Это шаблон полей с заполнителями, не готовая строка. Машинная схема и конкретный request включены ниже; все topic/profile/family значения берутся из request, а не из примера. `model_identity_observed` становится `true` только при фактическом наблюдении. Не присваивай `DeterministicGenerator` или `OpenTeacher` модельным ответам ради будущего native admission. Отмена подготовки checksum не меняет контракт нативного trainer.

Prompt содержит всё, что ученику нужно для выбора: исходный текст, роли/контекст, версию framework или SQL dialect, необходимые входные данные и ограничение ответа. Метки класса, правильный ответ, rationale, ID, источники и будущие результаты не подкладываются как признаки ученику. Исключение — сведения, которые составляют само задание, например явно предоставленный каталог маршрутов.

Chosen и rejected решают одно задание и выглядят одинаково естественно. Нельзя делать rejected нелепым, бессмысленным или заметно короче только ради отличия; нельзя добавлять «неверно», «это плохой ответ», markdown-маркеры правильности или объяснение ошибки. Если обе стороны корректны и отличаются лишь стилем или допустимым синонимом, пары нет: переделай контраст в рамках назначенной темы. Если источник не подтверждает факт или версия API неизвестна, не выдумывай её — сохрани неготовый случай отдельно в `quarantine.jsonl` этой попытки с причиной и укажи недостающий материал. Quarantine не считается созданной парой и не входит в count. При отсутствии source-файла назови конкретный ID/path и используй назначенную карту происхождения; не создавай пустой manifest, фальшивую RAG-выдержку или текст под ожидаемый hash.

## Семьи и независимость наборов

Копируй `origin_family_id` и `leakage_group_ids` из `family_contracts`. Варианты одного исходного сюжета, его переводы, обратные переводы, опечатки и неправильная раскладка остаются в одной семье и одном split. Родственные морфологические формы могут разделять лексическую семью, но их роль/род/число/смысл сохраняются отдельно. Похожее самостоятельное слово нельзя превращать в опечатку другого слова только по расстоянию редактирования.

Если для семьи назначены два класса, создай по одной строке каждого класса с общими family/origin/leakage. Меняй только указанный контекстный фактор; метка `family_class_field` остаётся в metadata. Не заменяй требуемый контроль второй положительной строкой. Общие квоты routing-классов из request обязательны.

Reference-примеры карточек показывают качество постановки. Не переписывай их под новыми ID и не считай сгенерированными уникальными данными. Их производные не становятся скрытыми DEV/EXAM. Для DEV/EXAM используй новые семьи и отдельную сессию без сгенерированного TRAIN, его ответов или отзывов. Формальные ID не доказывают смысловую независимость: сохраняй известное родство сейчас; последующая проверка скрытых парафразов назначается отдельно и не запускается этой сессией.

## Качество по предмету

- Перевод: все сведения о роде, числе, времени, роли, отрицании и референтах должны следовать из видимого prompt. Нельзя приписывать пол по стереотипу профессии. Проверить направление языка отдельно. Для неверной раскладки сверить конкретную физическую карту; для опечатки — точное изменение code points. Иврит хранится в обычном логическом порядке Unicode, без ручного разворота букв.
- Оркестрация: выбранное действие должно следовать из цели пользователя и текущего состояния, учитывать зависимости и настоящий результат предыдущего шага. Слова «код» или «документация» сами по себе не определяют маршрут; учитывай действие «напиши», «объясни», «спланируй» и т. п. Следующее действие и ожидание не заменяются фиктивным успехом.
- Документация: объясняется фактический код/контракт указанной версии; желаемое исправление не выдаётся за уже существующее поведение.
- Написание кода: верный ответ удовлетворяет видимому контракту. Будущая отдельно порученная приёмка поведения использует небольшой исполняемый oracle на этих примерах; компиляция сама по себе недостаточна. Сейчас ни oracle, ни глобальные production-тесты не запускаются: создаётся кандидат по полному видимому требованию.

## Завершение создания и переход

После сохранения ровно `count` строк выполни готовую команду `finish` из `commands.md`, сохранив абсолютные пути интерпретатора/request/draft и заменяя `unknown` только наблюдаемым именем модели. Отдельный `check` не нужен: finish проверяет JSON/контракт/количество и структурные связи; он не оценивает смысл независимо, не вызывает provider и не обучает. При ошибке исправляй только создаваемый файл с сохранением прежней версии. Не создавай новый run ради обхода структурной ошибки и не выдавай неполный результат за завершённый.

Успешный finish сохраняет `state.json` и `report.creation.md` (новая версия вместо затирания имеющегося отчёта). `DRAFT_CREATED` означает созданных кандидатов. Сразу переходи к следующему stream из registry; `review_status=not_requested`, независимая смысловая приёмка не выполнена, Student `NOT_RUN`. **Запрещено автоматически создавать blind/review, вызывать отдельного reviewer, делать accept/export или ждать их перед следующим набором.** Сохранённые описания этих этапов используются только по отдельному последующему заданию.

Короткое сообщение по-русски: «Создано N из N кандидатных пар по теме …; файлы …; структурная проверка завершена; следующий …». Если число неполное, назови фактическое число и точную причину. Не добавляй вымышленные оценки смысла, скорость Student, стоимость или обучение. Имя модели/session остаётся unknown, если оно не наблюдалось.

## Контекст и ожидание

Не пытайся вместить большой корпус в один ответ. Размер текущей партии уже ограничен; при нехватке контекста сохрани фактический неполный результат и причину, не обрезай строки и не заявляй ровный count. Следующая партия назначается отдельным request с `next_start`; существующий run не удаляется и не перезапускается скрыто.

При долгой операции сначала найди документацию фактически доступного инструмента и его wait/resume. Запусти один раз, сохрани один handle и жди его terminal state штатным блокирующим/event механизмом; `running`, истечение yield или тихий stdout не означают отказ. Не заканчивай ход словами «жду», не запускай второй процесс или фоновый цикл частого опроса. При явном «стоп» прекрати работу. При «следующий» немедленно перейди к названному ID и не дописывай прошлую задачу. Если ID не назван, очередь берётся из `execution-queue.json`; точный `next_stream_id` также возвращают finish/resume. Дополнительный поиск runner или модели для перехода не нужен.

Подготовка/JSON/oracle не являются измерением обучения. Если позже пользователь разрешит учебный опыт, требуется настоящий обучающий механизм, совместимое сохранённое ненулевое состояние, независимое до/после и достаточный бюджет. `0→0` не подтверждает обучение. Любое измеренное улучшение полезного показателя при валидном опыте — успех по нему и домену; остальные показатели, включая ухудшения, показываются рядом. Эти требования не дают разрешения запускать обучение сейчас.


## Назначение

```json
{
  "schema_version": "xiigen.batch-request.v1",
  "stream_id": "OR-ROUTE-INTENT/base",
  "topic_id": "OR-ROUTE-01",
  "objective_id": "OR-ROUTE-INTENT",
  "domain_id": "or.routing",
  "profile_id": "base",
  "run_id": "M004",
  "split": "train",
  "count": 21,
  "family_ids": [
    "v3/OR-ROUTE-INTENT/base/F0000000",
    "v3/OR-ROUTE-INTENT/base/F0000001",
    "v3/OR-ROUTE-INTENT/base/F0000002",
    "v3/OR-ROUTE-INTENT/base/F0000003",
    "v3/OR-ROUTE-INTENT/base/F0000005",
    "v3/OR-ROUTE-INTENT/base/F0000006",
    "v3/OR-ROUTE-INTENT/base/F0000007",
    "v3/OR-ROUTE-INTENT/base/F0000008",
    "v3/OR-ROUTE-INTENT/base/F0000009",
    "v3/OR-ROUTE-INTENT/base/F0000010",
    "v3/OR-ROUTE-INTENT/base/F0000011",
    "v3/OR-ROUTE-INTENT/base/F0000013",
    "v3/OR-ROUTE-INTENT/base/F0000014",
    "v3/OR-ROUTE-INTENT/base/F0000015",
    "v3/OR-ROUTE-INTENT/base/F0000016",
    "v3/OR-ROUTE-INTENT/base/F0000017",
    "v3/OR-ROUTE-INTENT/base/F0000018",
    "v3/OR-ROUTE-INTENT/base/F0000019",
    "v3/OR-ROUTE-INTENT/base/F0000020",
    "v3/OR-ROUTE-INTENT/base/F0000021",
    "v3/OR-ROUTE-INTENT/base/F0000022"
  ],
  "family_contracts": {
    "v3/OR-ROUTE-INTENT/base/F0000000": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000000",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000000"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000001": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000001",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000001"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000002": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000002",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000002"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000003": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000003",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000003"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000005": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000005",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000005"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000006": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000006",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000006"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000007": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000007",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000007"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000008": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000008",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000008"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000009": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000009",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000009"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000010": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000010",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000010"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000011": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000011",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000011"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000013": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000013",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000013"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000014": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000014",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000014"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000015": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000015",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000015"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000016": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000016",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000016"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000017": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000017",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000017"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000018": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000018",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000018"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000019": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000019",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000019"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000020": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000020",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000020"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000021": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000021",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000021"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ROUTE-INTENT/base/F0000022": {
      "origin_family_id": "v3/OR-ROUTE-INTENT/base/F0000022",
      "leakage_group_ids": [
        "v3/OR-ROUTE-INTENT/base/F0000022"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 23,
  "sources": [
    "RAG-006",
    "RAG-031",
    "RAG-033"
  ],
  "card": "curriculum/cards/OR-ROUTE-INTENT.md",
  "output_directory": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\OR-ROUTE-INTENT\\base\\M004",
  "mode": "CREATE_ONLY",
  "next_stream_id": "TR07-OMISSION/en-ru",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\OR-ROUTE-INTENT.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\OR-ROUTE-INTENT\\base\\M004\\request.json",
    "prompt": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\OR-ROUTE-INTENT\\base\\M004\\prompt.md",
    "draft": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\OR-ROUTE-INTENT\\base\\M004\\draft.jsonl",
    "state": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\OR-ROUTE-INTENT\\base\\M004\\state.json",
    "commands": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\OR-ROUTE-INTENT\\base\\M004\\commands.md",
    "resume": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\OR-ROUTE-INTENT\\base\\M004\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-006": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-006.md",
      "RAG-031": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-031.md",
      "RAG-033": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-033.md"
    }
  },
  "model_policy": "Current user-selected Chinese model in Kiro; no override; record observed identity honestly.",
  "scope": "DATA_PREPARATION_ONLY",
  "training_status": "NOT_RUN",
  "route_class_counts": {
    "tr.en": 1,
    "tr.ru": 1,
    "tr.he": 1,
    "docs.csharp": 1,
    "docs.sql": 1,
    "docs.angular": 1,
    "code.csharp": 1,
    "code.sql": 1,
    "code.angular": 1,
    "or.functional": 1,
    "or.architecture": 1,
    "or.code": 1,
    "or.database": 1,
    "or.client": 1,
    "or.server": 1,
    "or.mobile": 1,
    "or.design": 1,
    "or.ux": 1,
    "or.qa": 1,
    "or.automation": 1,
    "clarify": 1
  }
}
```

На каждую family_id создай ровно одну строку.

## Предметное содержание карточки (без исторических команд)

{
  "objective_id": "OR-ROUTE-INTENT",
  "topic_id": "OR-ROUTE-01",
  "title_ru": "Маршрут по действию при явном закрытом каталоге",
  "learning_goal_ru": "Обучаемый маршрутизатор выбирает один предметный получатель по намерению; поддерживает уточнение для неизвестного, неоднозначного или составного запроса.",
  "generation_prompt_ru": "Создай новые семейства только для выбора одного маршрута. В каждый prompt помести полный видимый каталог 20 целей и пользовательский запрос. Не помещай правильную цель, rationale или error_tag вне списка кандидатов как подсказку. В TRAIN нужны все 20 целей и clarify: поддержанные, неподдержанные и неоднозначные запросы, а также контрфактические пары с одинаковыми предметными словами и разным действием. Для партии из 21 строки назначь по одной выбранной цели каждого класса; партии из 42 — по две. Примеры про C#, SQL и перевод здесь учат только выбору маршрута. Меняй порядок каталога между семействами, сохраняя смысл labels; перестановки одного случая остаются в одном split. Неподдержанный запрос и составной запрос в этом конкретном контракте требуют clarify; не выдумывай reject или граф шагов. Сложный граф и предметный ответ учатся отдельно. Отрицательный ответ содержит ровно одну ошибку маршрута либо необоснованное уточнение. JSON action и domain_id у обеих сторон одинаковой формы. Правило составного запроса и уточнение, что недостающей считается информация для выбора маршрута, а не для исполнения предметной задачи, должны присутствовать в каждом learner-visible prompt; одной инструкции генератору недостаточно.",
  "response_contract_ru": "Только JSON {\"action\":\"route\",\"domain_id\":\"одно из 20 имён\"} либо {\"action\":\"clarify\",\"domain_id\":null}. Не возвращать сам перевод, код или план. Метки этого preparation-каталога не объявлены native IDs core.",
  "scenario_axes": [
    {
      "axis": "намерение",
      "values": [
        "tr.en",
        "tr.ru",
        "tr.he",
        "docs.csharp",
        "docs.sql",
        "docs.angular",
        "code.csharp",
        "code.sql",
        "code.angular",
        "or.functional",
        "or.architecture",
        "or.code",
        "or.database",
        "or.client",
        "or.server",
        "or.mobile",
        "or.design",
        "or.ux",
        "or.qa",
        "or.automation",
        "clarify"
      ]
    },
    {
      "axis": "контроль",
      "values": [
        "слово о другом стеке в задаче перевода",
        "одинаковый предмет: объяснить/написать/спланировать",
        "неподдержанная задача",
        "недостаток контекста",
        "составной запрос",
        "перестановка каталога"
      ]
    }
  ],
  "forbidden_mixing": [
    "Обучение SQL/переводу на строке маршрутизатора.",
    "Роутинг обратно в or.routing.",
    "One-class TRAIN без отрицательных/неоднозначных случаев.",
    "Путь, номер строки, позиция кандидата или цель в metadata становятся решением runtime."
  ],
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "ROUTE-INTENT-001",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F001",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nПереведи на русский: The meeting starts at nine.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"tr.ru\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"docs.csharp\"}",
      "error_tag": "task_translation",
      "rationale": "Язык результата задан; слово meeting не делает задачу оркестрацией.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Язык результата задан; слово meeting не делает задачу оркестрацией.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-001",
        "origin_family_id": "OR-ROUTE-01-REF-F001",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F001"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-002",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F002",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nПереведи на английский: Завтра отчёт будет готов.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"tr.en\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"or.functional\"}",
      "error_tag": "task_translation",
      "rationale": "Нужен перевод, а не план подготовки отчёта.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Нужен перевод, а не план подготовки отчёта.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-002",
        "origin_family_id": "OR-ROUTE-01-REF-F002",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F002"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-003",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F003",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nПереведи на иврит: Документ сохранён.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"tr.he\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"docs.csharp\"}",
      "error_tag": "task_translation",
      "rationale": "Упоминание документа не означает задачу документирования кода.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Упоминание документа не означает задачу документирования кода.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-003",
        "origin_family_id": "OR-ROUTE-01-REF-F003",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F003"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-004",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F004",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nПо этому C#-методу опиши возвращаемое значение; код не меняй.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"docs.csharp\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"code.csharp\"}",
      "error_tag": "docs_vs_code",
      "rationale": "Прямо запрошено описание существующего метода.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Прямо запрошено описание существующего метода.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-004",
        "origin_family_id": "OR-ROUTE-01-REF-F004",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F004"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-005",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F005",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nНапиши C#-метод проверки положительного количества.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"code.csharp\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"docs.csharp\"}",
      "error_tag": "code_vs_docs",
      "rationale": "Запрошено создание кода.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Запрошено создание кода.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-005",
        "origin_family_id": "OR-ROUTE-01-REF-F005",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F005"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-006",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F006",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nОбъясни, какие строки возвращает приведённый SQL JOIN.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"docs.sql\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"code.sql\"}",
      "error_tag": "docs_vs_code",
      "rationale": "Нужен анализ предоставленного запроса.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Нужен анализ предоставленного запроса.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-006",
        "origin_family_id": "OR-ROUTE-01-REF-F006",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F006"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-007",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F007",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nНапиши SQL-запрос, считающий заказы без оплаченной суммы.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"code.sql\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"docs.sql\"}",
      "error_tag": "code_vs_docs",
      "rationale": "Нужен новый запрос.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Нужен новый запрос.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-007",
        "origin_family_id": "OR-ROUTE-01-REF-F007",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F007"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-008",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F008",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nОпиши поведение этого Angular-компонента при пустом Input.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"docs.angular\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"code.angular\"}",
      "error_tag": "docs_vs_code",
      "rationale": "Задача — описать предоставленное поведение.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Задача — описать предоставленное поведение.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-008",
        "origin_family_id": "OR-ROUTE-01-REF-F008",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F008"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-009",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F009",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nРеализуй в Angular отмену предыдущего поискового запроса.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"code.angular\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"docs.angular\"}",
      "error_tag": "code_vs_docs",
      "rationale": "Нужно написать реализацию.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Нужно написать реализацию.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-009",
        "origin_family_id": "OR-ROUTE-01-REF-F009",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F009"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-010",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F010",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nСоставь один измеримый критерий приёмки сохранения черновика.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"or.functional\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"or.qa\"}",
      "error_tag": "functional_vs_qa",
      "rationale": "Запрашивается определение требования, а не проверка существующего результата.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Запрашивается определение требования, а не проверка существующего результата.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-010",
        "origin_family_id": "OR-ROUTE-01-REF-F010",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F010"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-011",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F011",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nВыбери границу между доменным расчётом и SQL-адаптером; код не нужен.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"or.architecture\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"code.sql\"}",
      "error_tag": "architecture_vs_implementation",
      "rationale": "Смысл запроса — ответственность компонентов.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Смысл запроса — ответственность компонентов.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-011",
        "origin_family_id": "OR-ROUTE-01-REF-F011",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F011"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-012",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F012",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nПосле обрыва сети мобильное приложение хранит офлайн-правку; спланируй следующий sync-шаг.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"or.mobile\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"or.server\"}",
      "error_tag": "mobile_sync_scope",
      "rationale": "Мобильное приложение явно названо; требуется план мобильной синхронизации, а не серверная реализация.",
      "counterexample": "Если запрос явно про изменение состояния веб-клиента по HTTP-ответу без мобильной синхронизации, выбранный получатель был бы or.client.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-012-v2",
        "origin_family_id": "OR-ROUTE-01-REF-F012",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F012"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family",
        "revision_reason": "Mobile scope now explicit in learner-visible prompt; prior ambiguous family retained."
      }
    },
    {
      "example_id": "ROUTE-INTENT-013",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F013",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nЗадание уже running; инструмент поддерживает wait. Выбери как дождаться результата.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"or.automation\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"or.qa\"}",
      "error_tag": "run_control_vs_evaluation",
      "rationale": "Нужен переход жизненного цикла, не оценка качества результата.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Нужен переход жизненного цикла, не оценка качества результата.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-013",
        "origin_family_id": "OR-ROUTE-01-REF-F013",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F013"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-014",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F014",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nПо эксперименту с одинаковым parent составь проверку качества до и после DPO.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"or.qa\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"or.code\"}",
      "error_tag": "evaluation_vs_implementation",
      "rationale": "Нужно спланировать проверку учебного эффекта.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Нужно спланировать проверку учебного эффекта.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-014",
        "origin_family_id": "OR-ROUTE-01-REF-F014",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F014"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-015",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F015",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nСкажи, какой следующий кодовый task брать: A done, B зависит от A, C зависит от B.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"or.code\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"code.csharp\"}",
      "error_tag": "orchestration_vs_code_generation",
      "rationale": "Язык кода не задан и реализацию не просят; выбирается следующий task.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Язык кода не задан и реализацию не просят; выбирается следующий task.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-015",
        "origin_family_id": "OR-ROUTE-01-REF-F015",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F015"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-016",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F016",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nСпланируй порядок backfill и введения NOT NULL, сохранив старые записи; SQL пока не писать.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"or.database\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"code.sql\"}",
      "error_tag": "database_plan_vs_sql",
      "rationale": "Запрошен порядок изменения данных, а не SQL-реализация.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Запрошен порядок изменения данных, а не SQL-реализация.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-016",
        "origin_family_id": "OR-ROUTE-01-REF-F016",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F016"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-017",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F017",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nОпредели клиентское состояние после accepted: запрос ещё выполняется, готового результата нет.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"or.client\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"or.design\"}",
      "error_tag": "client_state_vs_visual_design",
      "rationale": "Нужен переход состояния по API-контракту, не внешний вид экрана.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Нужен переход состояния по API-контракту, не внешний вид экрана.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-017",
        "origin_family_id": "OR-ROUTE-01-REF-F017",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F017"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-018",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F018",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nСоставь серверный порядок проверки tenant и записи, по данному контракту авторизации.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"or.server\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"code.csharp\"}",
      "error_tag": "server_sequence_vs_code",
      "rationale": "Нужно спланировать серверную обработку; реализация и язык не заданы.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Нужно спланировать серверную обработку; реализация и язык не заданы.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-018",
        "origin_family_id": "OR-ROUTE-01-REF-F018",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F018"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-019",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F019",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nВыбери UI-поверхность для администратора, наблюдающего внутреннюю очистку кэша.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"or.design\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"or.client\"}",
      "error_tag": "surface_vs_client_behavior",
      "rationale": "Запрашивается выбор поверхности и аудитории, не переход по API.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Запрашивается выбор поверхности и аудитории, не переход по API.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-019",
        "origin_family_id": "OR-ROUTE-01-REF-F019",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F019"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-020",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F020",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nПодбери понятное сообщение и следующее действие при истёкшей сессии; черновик сохранён.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"or.ux\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"or.design\"}",
      "error_tag": "recovery_vs_surface",
      "rationale": "Нужен сценарий восстановления взаимодействия, не выбор компоновки.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Нужен сценарий восстановления взаимодействия, не выбор компоновки.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-020",
        "origin_family_id": "OR-ROUTE-01-REF-F020",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F020"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-021",
      "profile_id": "base",
      "family_id": "OR-ROUTE-01-REF-F021",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nПомоги с экраном. Контекста о требуемом действии нет.\nФормат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.",
      "chosen": "{\"action\":\"clarify\",\"domain_id\":null}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"or.design\"}",
      "error_tag": "underspecified_request",
      "rationale": "Нельзя без основания выбрать дизайн вместо UX, кода или анализа.",
      "counterexample": "Сравнить явно запрошенное действие с описанием выбранного получателя; Нельзя без основания выбрать дизайн вместо UX, кода или анализа.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "derived_from": "OR-ROUTE-01-REF-021",
        "origin_family_id": "OR-ROUTE-01-REF-F021",
        "leakage_group_ids": [
          "OR-ROUTE-01-REF-F021"
        ],
        "authoring_status": "adapted_public_reference_not_new_independent_family"
      }
    },
    {
      "example_id": "ROUTE-INTENT-022",
      "profile_id": "base",
      "family_id": "ROUTE-INTENT-F022",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nКакова погода завтра в Хайфе? Прогноз погоды среди получателей не предусмотрен.\nВерни только JSON с action и domain_id.",
      "chosen": "{\"action\":\"clarify\",\"domain_id\":null}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"or.functional\"}",
      "error_tag": "unsupported_or_compound",
      "rationale": "Запрос о погоде не превращается в формулирование требования к программному проекту.",
      "counterexample": "Запрос о погоде не превращается в формулирование требования к программному проекту.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "origin_family_id": "ROUTE-INTENT-F022",
        "leakage_group_ids": [
          "ROUTE-INTENT-F022"
        ]
      }
    },
    {
      "example_id": "ROUTE-INTENT-023",
      "profile_id": "base",
      "family_id": "ROUTE-INTENT-F023",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nПереведи данное описание SQL-запроса на иврит; сам запрос менять и объяснять не нужно: Returns all active users.\nВерни только JSON с action и domain_id.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"tr.he\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"docs.sql\"}",
      "error_tag": "intent_vs_keyword",
      "rationale": "Упоминание SQL не меняет явно заданную задачу перевода на иврит.",
      "counterexample": "Упоминание SQL не меняет явно заданную задачу перевода на иврит.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "origin_family_id": "ROUTE-INTENT-F023",
        "leakage_group_ids": [
          "ROUTE-INTENT-F023"
        ]
      }
    },
    {
      "example_id": "ROUTE-INTENT-024",
      "profile_id": "base",
      "family_id": "ROUTE-INTENT-F024",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nНапиши SQL-запрос по контракту: SELECT всех активных пользователей из Users(Id,Active); диалект T-SQL 2019. Объяснение не требуется.\nВерни только JSON с action и domain_id.",
      "chosen": "{\"action\":\"route\",\"domain_id\":\"code.sql\"}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"docs.sql\"}",
      "error_tag": "intent_vs_keyword",
      "rationale": "Пользователь просит написать код, а не описать существующий.",
      "counterexample": "Пользователь просит написать код, а не описать существующий.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "origin_family_id": "ROUTE-INTENT-F024",
        "leakage_group_ids": [
          "ROUTE-INTENT-F024"
        ]
      }
    },
    {
      "example_id": "ROUTE-INTENT-025",
      "profile_id": "base",
      "family_id": "ROUTE-INTENT-F025",
      "prompt": "Каталог допустимых получателей: {\"tr.en\": \"перевести данный текст на английский\", \"tr.ru\": \"перевести данный текст на русский\", \"tr.he\": \"перевести данный текст на иврит\", \"docs.csharp\": \"объяснить данный код C#/.NET\", \"docs.sql\": \"объяснить данный SQL\", \"docs.angular\": \"объяснить данный Angular-код\", \"code.csharp\": \"написать C#/.NET код\", \"code.sql\": \"написать SQL\", \"code.angular\": \"написать Angular-код\", \"or.functional\": \"сформулировать функциональные требования\", \"or.architecture\": \"выбрать границы архитектуры\", \"or.code\": \"спланировать следующий шаг реализации\", \"or.database\": \"спланировать изменение базы данных\", \"or.client\": \"определить поведение клиента\", \"or.server\": \"определить порядок серверной обработки\", \"or.mobile\": \"спланировать мобильную синхронизацию\", \"or.design\": \"выбрать UI-поверхность\", \"or.ux\": \"выбрать понятное действие пользователя\", \"or.qa\": \"спланировать проверку результата\", \"or.automation\": \"управлять исполнением автоматизированного запуска\"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {\"action\":\"clarify\",\"domain_id\":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {\"action\":\"clarify\",\"domain_id\":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.\nПереведи текст на иврит и затем спланируй автоматическую публикацию. Порядок действий задан словом «затем»; контракт этого атомарного роутера требует clarify для любого запроса с несколькими самостоятельными предметными действиями.\nВерни только JSON с action и domain_id.",
      "chosen": "{\"action\":\"clarify\",\"domain_id\":null}",
      "rejected": "{\"action\":\"route\",\"domain_id\":\"tr.he\"}",
      "error_tag": "unsupported_or_compound",
      "rationale": "Порядок задан, но видимый контракт запрещает атомарному router извлекать первый шаг из составного запроса; требуется clarify.",
      "counterexample": "Если останется только «Переведи текст на иврит», действие одно и правильным будет route в tr.he; наличие «затем спланируй» делает текущий запрос составным.",
      "source_refs": [
        "RAG-006",
        "RAG-031",
        "RAG-033"
      ],
      "metadata": {
        "response_language": "json",
        "origin_family_id": "ROUTE-INTENT-F025",
        "leakage_group_ids": [
          "ROUTE-INTENT-F025"
        ]
      }
    }
  ]
}

## Полный контракт строки: не искать другую схему

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "XIIGEN candidate DPO row, creation stage only",
  "description": "One JSON object per JSONL line. Structural conformance does not establish semantic quality, native admission or learning. Request-specific family/topic constraints are checked by batches.py finish.",
  "type": "object",
  "required": [
    "schema_version",
    "pair_id",
    "domain_id",
    "topic_id",
    "profile_id",
    "family_id",
    "split",
    "prompt",
    "chosen",
    "rejected",
    "error_tag",
    "rationale",
    "source_refs",
    "provenance",
    "metadata"
  ],
  "properties": {
    "schema_version": {
      "const": "xiigen.dpo.v1"
    },
    "pair_id": {
      "type": "string",
      "minLength": 1
    },
    "domain_id": {
      "type": "string",
      "minLength": 1
    },
    "topic_id": {
      "type": "string",
      "minLength": 1
    },
    "profile_id": {
      "type": "string",
      "minLength": 1
    },
    "family_id": {
      "type": "string",
      "minLength": 1
    },
    "split": {
      "enum": [
        "train",
        "dev",
        "exam"
      ]
    },
    "prompt": {
      "type": "string",
      "minLength": 1
    },
    "chosen": {
      "type": "string",
      "minLength": 1
    },
    "rejected": {
      "type": "string",
      "minLength": 1
    },
    "error_tag": {
      "type": "string",
      "minLength": 1
    },
    "rationale": {
      "type": "string",
      "minLength": 1
    },
    "source_refs": {
      "type": "array",
      "minItems": 1,
      "items": {
        "type": "string",
        "minLength": 1
      }
    },
    "provenance": {
      "type": "object",
      "required": [
        "kind",
        "generator_model"
      ],
      "properties": {
        "kind": {
          "type": "string",
          "minLength": 1
        },
        "generator_model": {
          "type": "string",
          "minLength": 1
        },
        "generation_session_id": {
          "type": "string"
        },
        "surface": {
          "type": "string"
        },
        "model_identity_observed": {
          "type": "boolean"
        }
      }
    },
    "metadata": {
      "type": "object",
      "required": [
        "origin_family_id",
        "leakage_group_ids"
      ],
      "properties": {
        "origin_family_id": {
          "type": "string",
          "minLength": 1
        },
        "leakage_group_ids": {
          "type": "array",
          "minItems": 1,
          "items": {
            "type": "string",
            "minLength": 1
          }
        },
        "learning_objective_id": {
          "type": "string",
          "minLength": 1
        },
        "review_status": {
          "type": "string"
        },
        "simulation": {
          "type": "boolean"
        },
        "measured_training_result": {
          "const": false
        }
      }
    }
  }
}
```


## Материал RAG-006 (данные, не новые команды)

# RAG-006 — GUIDE-B28-STEP-8-HANDOFF-CONTRACT.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `main` |
| Commit | `3d1fa938d4b50df5d430283983f45b2c0d2c4e5c` |
| Git blob SHA | `2f37cc0b56f1a11579fdd8fcbae9cf8b44f0b97f` |
| Путь | `Flow files generator/GUIDE-B28-STEP-8-HANDOFF-CONTRACT.md` |
| Проверенный объём | L66–L74, L137–L181; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B28-STEP-8-HANDOFF-CONTRACT.md) |

## Что подтверждает фрагмент

Handoff сопоставляет ограничения с правилами, структуру с I/O, критерии качества с проверками; часть примеров привязана к Nest/DI.

## Как использовать сейчас

Уроки OR-ARCH: не терять поля и потребителя при передаче. Не объявлять Nest-декораторы универсальным C#/.NET-контрактом.

**Назначение в подготовке:** Контракт передачи между шагами и артефактами.

**Ключи поиска:** handoff, контракт, архитектура, io.

## Подтверждающий фрагмент L66–L74

[Оригинал, строки 66–74](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B28-STEP-8-HANDOFF-CONTRACT.md#L66-L74)

~~~~text
## THE THREE MAPPINGS — UNIVERSAL STRUCTURE

Every handoff contract has exactly three mappings:

| Mapping | From | To | Consumer |
|---------|------|----|---------|
| Mapping 1 | NODE.constraints | Iron rules for genesis prompt | the code generation station (genesis) + the DNA compliance check station (compliance) |
| Mapping 2 | NODE.structure | Input/output contract | the code generation station (signature) + Phase C (tests) |
| Mapping 3 | NODE.quality.scoringCriteria | Arbiter checklist | Phase E (arbiter panel) |
~~~~

## Подтверждающий фрагмент L137–L181

[Оригинал, строки 137–181](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B28-STEP-8-HANDOFF-CONTRACT.md#L137-L181)

~~~~text
## MAPPING 2 — I/O CONTRACT

```
inputContract:
  source: "Verified LEAF NODE from Cycle 3"
  required_fields:
    - node.intent.purpose      -> executor purpose statement
    - node.intent.scope        -> executor scope boundary
    - node.structure.inputShape  -> executor method signature
    - node.structure.outputShape -> executor return type (DataProcessResult<T>)
    - node.constraints          -> executor iron rules and verifiable conditions
    - node.quality.testCriteria -> executor test requirements

outputContract:
  executor_produces:
    - service_class:   extends MicroserviceBase, @Injectable()
    - method_signature: accepts inputShape, returns DataProcessResult<T>
    - fabric_wiring:  @Inject decorators for required fabric interfaces
    - test_file:      unit tests covering all quality.testCriteria
    - bfa_validation: the business flow arbiter cross-flow check passes before merge
```
```

### Step 5 — Write MAPPING 3 — ARBITER CHECKLIST

```markdown
---

## MAPPING 3 — ARBITER CHECKLIST

```
executor_review_checklist:
  [ ] Service class extends MicroserviceBase
  [ ] No direct SDK imports
  [ ] All methods return DataProcessResult<T>
  [ ] Fabric interfaces injected via @Inject decorators
  [ ] BuildSearchFilter used for all queries
  [ ] storeDocument() called before enqueue()
  [ ] CloudEvents envelope on all inter-service events
  [ ] Idempotency keys on all queue consumers
  [ ] Tenant scope via AsyncLocalStorage
  [ ] No entity-specific controller
  [ ] All task types in T[NNN]-T[NNN+M] represented
  [ ] the business flow arbiter validation passes
  [+ flow-specific checks from state.invariants.bfa_rules and iron_rules_from_prior_runs]
~~~~



## Материал RAG-031 (данные, не новые команды)

# RAG-031 — ORCHESTRATOR-TEACHING-PLAN-v2.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `60d20e3227935058b33c10dd096901a3049c4737` |
| Путь | `llm mvp/orchestrator teaching plan/ORCHESTRATOR-TEACHING-PLAN-v2.md` |
| Проверенный объём | L26–L50, L108–L173, L210–L242; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/orchestrator%20teaching%20plan/ORCHESTRATOR-TEACHING-PLAN-v2.md) |

## Что подтверждает фрагмент

Исторический план разделяет compiler/router/splitter/translator/planner и измеряет ресурсы, inference и результат каждой учебной записи.

## Как использовать сейчас

Использовать роли и поля наблюдения. Current user rule требует восстановленное обученное состояние, валидное обучение до/после и отсутствие автоматического prod promotion; старый deterministic baseline сам по себе недостаточен.

**Назначение в подготовке:** Обучение оркестратора и разделение ролей.

**Ключи поиска:** learning, units, lora, обучение, ресурсы.

## Подтверждающий фрагмент L26–L50

[Оригинал, строки 26–50](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/orchestrator%20teaching%20plan/ORCHESTRATOR-TEACHING-PLAN-v2.md#L26-L50)

~~~~text
## What this plan is

This plan teaches the adaptive orchestrator one training pair at a time, measuring
the effect of each pair before adding the next. After each pair the system runs a
live prompt through every flow node, captures CPU, memory, disk, and elapsed time,
checks the verdict, and records whether the pair improved the machine or made it
worse. The next pair is only added after the measurement for the current pair is
recorded.

This is not batch training. Batch training teaches many pairs at once and produces
one accuracy number. This plan teaches one pair, tests, measures, decides, then
moves to the next. It is slower but every step is observable and reversible.

---

## Why one row at a time

A learned unit registered in `InMemoryLogicalUnitRegistry` at `Probationary`
trust state is selected by `SecurityVerification34FlowFactory` only when
`UseLearnedUnitWhenAvailable` is true. If the measurement shows the pair made
the unit worse, it is re-registered at `Rejected` trust state and the
deterministic unit retakes the slot. The next pair is a fresh attempt.

This means the machine never silently degrades. Every change is measured before
the next change is applied.
~~~~

## Подтверждающий фрагмент L108–L173

[Оригинал, строки 108–173](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/orchestrator%20teaching%20plan/ORCHESTRATOR-TEACHING-PLAN-v2.md#L108-L173)

~~~~text
## The five components being taught

Each component is an `ILogicalUnit` with a defined input and output type. Teaching
each one is a separate sub-cycle that follows the same pattern.

| Component | Contract | Role in the flow |
|---|---|---|
| Prompt routing input compiler | `ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>` | Layer 0 — converts raw prompt to router input |
| Multi-sub-phase router | `ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>` | Layer 0 — identifies all sub-phases a prompt touches |
| Domain splitter | `ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>` | Node 01 — splits selected transition into domain packets |
| Domain translator | `ILogicalUnit<DomainPacket, TranslatedDomainStatement>` | Node 02 — translates one domain packet into formal vocabulary |
| Scheme planner | `ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>` | Node 03 — plans ordered work branches with dependencies |

The nine-node security flow (sub-phase 3.4) already has deterministic units for
all nine nodes. Teaching starts with the routing layer (compiler and router) because
these gate every other node. If routing is wrong, no downstream teaching matters.

---

## The measurement record

Every pair produces one `TeachingRowRecord` before the next pair begins.

```csharp
public sealed record TeachingRowRecord(
    // Identity
    string PairId,
    string ComponentRole,
    string SubPhaseId,
    string TransitionType,
    int RowIndex,

    // Training resource consumption
    double TrainingElapsedMs,
    double TrainingCpuPercent,           // (cpuDelta / elapsed / ProcessorCount) * 100
    double TrainingWorkingSetDeltaMb,    // before/after delta, not a sampled peak
    double TrainingManagedAllocationMb,
    double TrainingDiskReadBytes,
    double TrainingDiskWriteBytes,
    long   BaseCheckpointBytes,          // full model checkpoint including adapter weights
    long   LoraAdapterBytes,             // computed from adapter list: sum(loraA + loraB) * 4
    long   DpoMetricBytes,               // sum of dpo_step_*.json file sizes

    // Inference resource consumption (one live run after training)
    double InferenceElapsedMs,
    double InferenceCpuPercent,           // (cpuDelta / elapsed / ProcessorCount) * 100
    double InferenceWorkingSetDeltaMb,    // before/after delta, not a sampled peak

    // Result verification
    string PromptUsedForVerification,
    string ExpectedVerdict,
    string ActualVerdict,
    bool   VerdictCorrect,
    double ConfidenceBefore,
    double ConfidenceAfter,
    double MarginImprovement,     // ConfidenceAfter - ConfidenceBefore; negative = regression

    // Decision
    LogicalUnitTrustState TrustStateAssigned,  // Probationary, Trusted, or Rejected
    string Decision,              // "promoted", "rejected", "held"
    string DecisionReason
);
```

All fields are written before moving to the next pair. An incomplete record means
the session did not finish the pair.
~~~~

## Подтверждающий фрагмент L210–L242

[Оригинал, строки 210–242](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/orchestrator%20teaching%20plan/ORCHESTRATOR-TEACHING-PLAN-v2.md#L210-L242)

~~~~text
### Step 0 — Baseline snapshot

Before touching anything, run the current deterministic unit on the verification
prompt and record:

```text
baseline_verdict        — actual verdict from the deterministic unit
baseline_confidence     — Confidence field from LogicalUnitResult
baseline_inference_ms   — elapsed time for one inference pass
baseline_working_set_mb — working set during inference
```

### Step 1 — Encode the pair

The A side of the pair becomes the `Prompt` field of a `DpoPair`.
The B side of the pair becomes the `Chosen` field.
A deliberately wrong B (taken from the failure-mode examples in the examination
method) becomes the `Rejected` field.

Tokenize both `Chosen` and `Rejected` using `BpeTokenizer.Encode()`.

### Step 2 — Apply LoRA and freeze base weights

```csharp
// LoraConfig.Scaling is computed: Scaling = Alpha / Rank.
// Set Alpha = 2 with Rank = 4 to achieve Scaling = 0.5.
// Never set Scaling directly — it has no setter.
var adapters = LoraApplicator.Apply(model, new LoraConfig
{
    Rank          = 4,
    Alpha         = 2f,      // Scaling = Alpha / Rank = 2 / 4 = 0.5
    Dropout       = 0.0f,
    TargetModules = ["q_proj", "v_proj"]
~~~~



## Материал RAG-033 (данные, не новые команды)

# RAG-033 — XIIGEN-EXPORT-1000-CASE-GENERATION-GUIDANCE-v1.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `5ef2adab961456c3f04abe00343f2bc62a933b67` |
| Путь | `llm mvp/orchestrator teaching plan/XIIGEN-EXPORT-1000-CASE-GENERATION-GUIDANCE-v1.md` |
| Проверенный объём | L10–L63, L123–L199, L350–L379; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/orchestrator%20teaching%20plan/XIIGEN-EXPORT-1000-CASE-GENERATION-GUIDANCE-v1.md) |

## Что подтверждает фрагмент

Исторический генератор 1000 случаев сохраняет реальные prompts, временной порядок, raw IDs и тип входа compiler/router.

## Как использовать сейчас

Использовать provenance и разные слои. Запрет synthetic в первом export-батче не относится к текущим явно заказанным симуляциям. Rephrases с общим source/family не размещать по разные стороны train/exam.

**Назначение в подготовке:** Массовая генерация ситуаций из существующих project export.

**Ключи поиска:** generation, simulation, compiler, синтетика, split.

## Подтверждающий фрагмент L10–L63

[Оригинал, строки 10–63](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/orchestrator%20teaching%20plan/XIIGEN-EXPORT-1000-CASE-GENERATION-GUIDANCE-v1.md#L10-L63)

~~~~text
## 1. Purpose

This guidance describes how to generate a 1000-case XIIGen teaching package from real session data.

The goal is not to invent coverage. The goal is to take real human prompts from old XIIGen/software-development conversations, preserve their rough wording, map them to the current orchestrator topology, and produce learning rows that can be used for compiler and router teaching runs.

The first accepted package generated under this guidance is:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\claude-export\batch-0000\xiigen-session-1000-oldest-first
```

Compressed copy:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\claude-export\batch-0000\xiigen-session-1000-oldest-first.zip
```

---

## 2. Non-Negotiable Rules

### Rule 1 - Real Human Prompt Only

The A-side must be a real human message from the session export.

Allowed:

```text
Raw human messages asking for a feature, fix, plan, review, test, architecture decision, skill, or documentation.
```

Not allowed:

```text
Assistant summaries.
Tool output.
Architecture-document prose rewritten as a prompt.
Synthetic prompts created only to fill a missing sub-phase.
Session-control messages such as "continue", "load session", or "please proceed".
```

### Rule 2 - Oldest First

Rows are selected in chronological order. The first package intentionally starts with older conversations because those sessions produced early documentation and skill-library work based on older code, which is a high-value modern use case.

Sort order:

```text
conversation.created_at ascending
  -> message.created_at ascending
```

If two export zips overlap, de-duplicate prompts by normalized human message text.
~~~~

## Подтверждающий фрагмент L123–L199

[Оригинал, строки 123–199](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/orchestrator%20teaching%20plan/XIIGEN-EXPORT-1000-CASE-GENERATION-GUIDANCE-v1.md#L123-L199)

~~~~text
## 3. Output Shape

Write the package here:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\claude-export\batch-0000\{package-id}
```

For the first 1000-case run:

```text
package-id = xiigen-session-1000-oldest-first
```

Required files:

```text
MANIFEST.md
cycle-map.yaml
conversation-index.jsonl
rows.jsonl
layer-0-a\layer-0a-compiler-pairs-001.md ... layer-0a-compiler-pairs-020.md
layer-0-b\layer-0b-router-pairs-001.md ... layer-0b-router-pairs-020.md
held-out\held-out-rephrases.md
```

Optional but recommended:

```text
tools\GenerateXiigenSession1000.cs
GUIDANCE.md
```

The package should also be zipped after generation:

```text
{package-id}.zip
```

---

## 4. Row Layers

### Layer 0-A - Compiler Pair

Contract:

```text
PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
```

A-side:

```yaml
run_id: "{row id}"
scenario_id: "adaptive-logical-orchestration"
user_request: "{raw human message}"
flags:
  UseLearnedUnitWhenAvailable: true
  UseKnowledgeEdgeDetection: true
```

B-side:

```yaml
DocumentId: "{row id}-prompt"
SourcePath: "claude-export/batch-0000/{conversation_uuid}/messages/{message_uuid}"
SourceTier: "claude_export_session"
LicenseOrAuthority: "first_party_private"
DocumentType: "compiled_prompt"
DocumentTitle: "{target sub-phase name}"
CompiledText: "{same raw human message}"
CompiledPredicateIds:
  - "{predicate id}"
SourceStatus: "ready"
KnownFailureMode: null
Confidence: 0.92
~~~~

## Подтверждающий фрагмент L350–L379

[Оригинал, строки 350–379](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/orchestrator%20teaching%20plan/XIIGEN-EXPORT-1000-CASE-GENERATION-GUIDANCE-v1.md#L350-L379)

~~~~text
## 8. Validation Before Acceptance

The package is accepted only when all checks pass:

```text
[ ] 1000 rows in rows.jsonl.
[ ] 20 Layer 0-A compiler files when using 50 rows per file.
[ ] 20 Layer 0-B router files when using 50 rows per file.
[ ] 100 held-out seeds when using every 10th row.
[ ] All rows have a non-empty conversation_uuid.
[ ] All rows have a non-empty message_uuid.
[ ] All rows have at least one compiled predicate.
[ ] All primary sub-phase IDs exist in the current code topology.
[ ] No setup-only prompts are present.
[ ] MANIFEST.md records skipped counts and domain/sub-phase distribution.
[ ] cycle-map.yaml records date window and covered sub-phases.
[ ] Zip package exists beside the folder.
```

For the first generated package, the accepted validation numbers were:

```text
Rows produced: 1000
Conversations used: 457
Layer 0-A files: 20
Layer 0-B files: 20
Held-out seeds: 100
First row message: 2025-09-03T10:43:59.809174Z
Last row message: 2026-03-11T11:01:47.894878Z
```
~~~~



## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\OR-ROUTE-INTENT\base\M004\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\OR-ROUTE-INTENT\base\M004\request.json' --input 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\OR-ROUTE-INTENT\base\M004\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `TR07-OMISSION/en-ru`. Не запускать review, accept, export, oracle или обучение.
