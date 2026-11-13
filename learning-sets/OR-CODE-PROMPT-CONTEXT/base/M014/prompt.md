# Создание одной партии DPO в Kiro — CREATE_ONLY

## Прямое поручение: продолжать до 192 000 пар

Текущая партия — часть кампании **192 потоков / минимум 192 000 реально созданных пар**, а не отдельная цель с новым запросом разрешения после finish. После `DRAFT_CREATED` немедленно выполняй следующее назначение; после `END` начни следующий обход недовыполненных потоков. Короткий промежуточный отчёт не завершает рабочий ход. STOP/NEXT пользователя имеют приоритет, реальный технический блокер сообщается точно.

**Не задавай вопросы «продолжать?», «коммитить или копить?», «10 или 25–50?»** Размер уже задан request/`candidate_count`; не увеличивай его ради скорости. Если публикация явно назначена — соблюдай её без переспроса; иначе сохраняй локально и продолжай. Не придумывай дополнительные отчёты, скрипты или мониторинг. Для новой партии того же stream используй новый run и `-Previous` от последнего завершённого request; сохраняй `next_start` и полные группы. Подробности, счёт без дублей и округление router 1000→1008: [контракт продолжения](../27-CAMPAIGN-CONTINUATION-CONTRACT.md). Старая accepted-квота 132 000 не является ограничением новой кампании кандидатов.

Уточнение для конечного файла сценариев: `-Previous` продолжает тот же family-plan. Когда подготовлен новый общий chunk, использовать его настоящий путь через `-FamilyPlan`, новый run и `-Start 0` **без `-Previous`**. Если материал ещё недоступен текущей роли, продолжать другие доступные потоки; не обнулять cursor старого файла и не выдавать повторные сюжеты за новые пары. Порядок подготовки следующих chunk сохранён в контракте продолжения.

Visible operational explanations and brief action rationales must remain in English. Do not switch them to Chinese. Reports for Luba and metadata rationale remain in Russian; learner examples follow their assigned language profile. Do not produce long thought monologues. Repetitive or incoherent output is a failed response, not progress or training data; recover from the existing request/state without restarting the campaign. This governs visible output, not hidden reasoning.

Роль человека при выборе учебного материала выполняет **CODEX с Astra 6 ultracode**. Оценку качества DPO также выполняет **CODEX с Astra 6 ultracode**. Внешняя открытая китайская модель через Kiro создаёт `chosen/rejected`. Сейчас выполняется CREATE_ONLY по уже назначенным карточкам: новые вызовы выбора/оценки не запускаются и не задерживают создание следующей партии. Назначение роли не является доказательством выполненного модельного вызова.

## Что ты создаёшь для обучения XIIGEN

Ты — автор учебных данных. Получатель данных — будущая обучаемая модель или Unit XIIGEN по назначенной предметной теме. Твой результат — реальные DPO-тройки `prompt/chosen/rejected` в `draft.jsonl`, пригодные по содержанию для последующего обучения предпочтению. Кандидатный статус обозначает стадию приёмки; он не разрешает заменять учебные примеры отчётом, схемой, списком тем или заглушками.

- `prompt` — полная задача, которую ученик должен решать при использовании модели: перевести текст, объяснить код, написать код либо выбрать действие по состоянию проекта.
- `chosen` — полный желаемый ответ ученика на этот prompt.
- `rejected` — полный правдоподобный ответ на тот же prompt с одной назначенной предметной ошибкой.
- Пара задаёт, какому ответу на тот же вход следует отдавать предпочтение. Фактическое изменение весов/LoRA или памяти выполняет соответствующий обучающий механизм позднее; генерация файла сама его не запускает.

Не помещай «создай DPO-набор», команды подготовки, пути Kiro, отчёт исполнителя или объяснение предпочтения в learner prompt/answers, если сама назначенная предметная задача явно этого не требует. При обычном использовании ученик получает prompt и выдаёт ответ; метки chosen/rejected не подсказываются ему как ответ. Обе альтернативы доступны обучающему механизму для задания предпочтения.

Сначала прочитай конкретные учебную цель, response contract и предметные reference-примеры назначенной карточки. До написания каждой строки должно быть ясно: «После будущего обучения на подобных примерах ученик должен уметь ___; в этой строке chosen выполняет ___, а rejected нарушает ___». Это рабочая постановка при авторинге, не отдельный отчёт или новый review-этап.

### Простая самопроверка на примерах

Для задачи удаления: `письмо` —6 букв, `писмо` —5: удалён `ь`, остальные буквы прежние. `пильмо` —6: это замена `с` на `л`, не удаление. Сначала проверь «длина стала меньше на1», затем «оставшиеся буквы в прежнем порядке и регистре». `Окно→кно` подходит для удаления первой буквы; `Окно→Кно` дополнительно меняет регистр. У `market` индексы `m0 a1 r2 k3 e4 t5`: `maket` получается удалением индекса2, не3. Даже правильная длина не доказывает правильный индекс.

Для перестановки бери слово прямо из предложения: в `Возьми ручку` это `ручку`. Обмен `ч/к` даёт `рукчу`,5→5 букв. `рукча` не подходит: изменилось ещё окончание. Обратная операция должна точно восстановить слово из предложения. Эти действия — часть составления строки, не отдельный audit/отчёт/запуск инструментов.

Для `TR07-VALIDWORD` исходное слово уже правильное: `She mentioned a sword.` → `Она упомянула меч.`; rejected `Она упомянула слово.`. Не добавляй строки «восстанови повреждение»: «Контрпример / граница» карточки не назначает квоту 5+5. Сравнение с конкурентом `sword→word` — удаление первой буквы (5→4), а learner source остаётся `sword`. Проверь и другие факты: `שולחנו` → `его стол`, не `наш стол`; `school entrance` → `вход школы`, без добавления «главный». Подробные разборы и источник форм иврита — в шпаргалке ниже.

При задаче с одной опечаткой обратная замена должна восстановить **всё предложение**, а не только токен: `She saw a cat.` → `She saw a cap.` сохраняет остальной текст; `She saw the cap near the door.` дополнительно меняет артикль и место.

Проверка пары: для `Она отправила писмо бабушке` chosen `She sent a letter to her grandmother.` и rejected `She sent a parcel to her grandmother.` различаются предметом. Rejected `She mailed a letter to her grandmother.` не создаёт такого контраста: это другой допустимый перевод. Rationale не может сделать правильный перевод ошибочным.

Полные хорошие/плохие разборы: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/prompts/TYPO-AUTHORING-EXAMPLES-RU.md`. Это публичная подсказка автору, не новые строки корпуса. Совпадающий F-номер у двух профилей не означает один сюжет. Автор плана подготовки вкладывает общий исходный сценарий или исходник для общей семьи; используй его, когда он дан. У старого request без такого сюжета сохраняй назначенные family/origin/leakage/split и кандидатный статус: родство не подтверждено, его нельзя объявить установленным по номеру. Не ищи чужие сессии, не выдумывай подтверждение origin и не переименовывай семьи. Это открытая часть задания для автора подготовки, а не новый блокер генерации: продолжай CREATE_ONLY с обычным finish и следующим ID, без отдельного review-процесса.

**CREATE_ONLY означает: сейчас создать данные для обучения, само обучение сейчас не запускать.** Он не отменяет предметной правильности, полного текста ответов, различающего смысла и требуемого объёма. `count` задаёт размер текущей партии, а не всей кампании; десять строк не означают, что большие наборы всех доменов готовы.

### Как выглядит учебное содержание

Публичный reference из TR07-OMISSION/ru-en (иллюстрация трёх текстовых полей; не копировать в новый TRAIN/DEV/EXAM):

```json
{
  "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод. Контекст: документ для проезда в автобусе. В одном обычном слове случайно пропущена ровно одна буква; восстанови его по контексту.\nИсходный текст: Блет лежит на столе.",
  "chosen": "The ticket is on the table.",
  "rejected": "The receipt is on the table."
}
```

Учебное содержание здесь — восстановление значения по контексту и его перевод. Ответы сохраняют одну и ту же сцену; меняется перевод восстанавливаемого предмета. Это не урок о том, как оформлять JSON. Для текущей другой темы используй её собственный response contract и примеры: документация объясняет фактическое поведение, код реализует видимое требование, оркестрация выдаёт требуемое действие/план/маршрут.

`canonical_token`, `raw_token`, `lexical_family_ids`, origin/split и rationale служат разметкой и связностью. Чтобы учить переводу повреждённого слова, реальная повреждённая форма должна присутствовать в learner prompt, а правильный смысл — в выбранном ответе. Одна запись связи в metadata не обучает токенизатор, kNN или Unit и не доказывает, что native consumer использует это поле. Не раскрывай скрытый canonical-ответ ученику ради упрощения задачи.

Ты создаёшь учебные кандидаты одной маленькой темы в режиме **CREATE_ONLY**: запись draft → структурный finish → следующий stream. Предметное review, blind, accept, export, native import, Student и production-тесты не входят в эту сессию и не блокируют следующую партию. Используй текущую выбранную пользователем китайскую модель Kiro; помощник наследует её; допускается не более одного помощника одновременно. Не переопределяй модель и не называй её поставщика по предположению. Запиши наблюдаемую модель/selector, среду и идентификатор сессии. Если identity или session ID не раскрыты, запиши `unknown`; кандидаты можно сохранить, но нельзя утверждать, что установлен конкретный провайдер.

Этот пакет относится только к подготовке материала. Обязательные checksum, конкретный профиль CODEX, квитанции провайдера, проверка всего production-проекта и промышленное оформление не требуются. Независимый reviewer также не является условием создания. Исторические указания о них внутри карточки или RAG не устанавливают дополнительные условия. Содержательные требования к выбранной теме, корректности пары, происхождению и разделению семей остаются обязательными. Не редактируй исходные навыки или рабочий код ради этого исключения.

## Точные пути без повторного поиска

Planning checkout: `C:/Users/lubao/Documents/xiigen`. Наборы создаются только под `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`; инструкции — в соседнем `preparation`. Текущая папка Kiro или `llm_mvp_core` не меняет эти значения. `request.paths` ниже содержит абсолютные пути к preparation root, learning sets root, карточке, контракту, JSON schema, draft/state/commands, tools script и назначенным sources (ID → path). Используй эти значения буквально. Схема — `schemas/dpo-candidate.schema.json`, объяснение — `02-DATA-CONTRACT.md`; они включены в пакет. Не ищи другой DPO-формат в соседнем проекте.

Если продолжаешь уже сохранённый request с другим `output_directory` (например, существующий M007 под `C:/Projects/llm_mvp_core/learning-sets`), эту попытку продолжай целиком по её request/commands. Не переноси часть файлов, не создавай копию партии и не меняй request задним числом. Для следующего ещё не подготовленного запуска используй команду действующей очереди и библиотечный корень выше, без унаследованного старого `--work-root`. Пример M007 объясняет существующее расхождение, а не назначает новый запуск. [Правило двух ситуаций](../20-CREATE-ONLY-AND-PATHS.md).

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
  "stream_id": "OR-CODE-PROMPT-CONTEXT/base",
  "topic_id": "OR-CODE-01",
  "objective_id": "OR-CODE-PROMPT-CONTEXT",
  "domain_id": "or.code",
  "profile_id": "base",
  "run_id": "M014",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000000",
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000001",
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000002",
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000003",
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000005",
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000006",
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000007",
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000008",
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000011",
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000012"
  ],
  "family_contracts": {
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000000": {
      "origin_family_id": "v3/OR-CODE-PROMPT-CONTEXT/base/F0000000",
      "leakage_group_ids": [
        "v3/OR-CODE-PROMPT-CONTEXT/base/F0000000"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000001": {
      "origin_family_id": "v3/OR-CODE-PROMPT-CONTEXT/base/F0000001",
      "leakage_group_ids": [
        "v3/OR-CODE-PROMPT-CONTEXT/base/F0000001"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000002": {
      "origin_family_id": "v3/OR-CODE-PROMPT-CONTEXT/base/F0000002",
      "leakage_group_ids": [
        "v3/OR-CODE-PROMPT-CONTEXT/base/F0000002"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000003": {
      "origin_family_id": "v3/OR-CODE-PROMPT-CONTEXT/base/F0000003",
      "leakage_group_ids": [
        "v3/OR-CODE-PROMPT-CONTEXT/base/F0000003"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000005": {
      "origin_family_id": "v3/OR-CODE-PROMPT-CONTEXT/base/F0000005",
      "leakage_group_ids": [
        "v3/OR-CODE-PROMPT-CONTEXT/base/F0000005"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000006": {
      "origin_family_id": "v3/OR-CODE-PROMPT-CONTEXT/base/F0000006",
      "leakage_group_ids": [
        "v3/OR-CODE-PROMPT-CONTEXT/base/F0000006"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000007": {
      "origin_family_id": "v3/OR-CODE-PROMPT-CONTEXT/base/F0000007",
      "leakage_group_ids": [
        "v3/OR-CODE-PROMPT-CONTEXT/base/F0000007"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000008": {
      "origin_family_id": "v3/OR-CODE-PROMPT-CONTEXT/base/F0000008",
      "leakage_group_ids": [
        "v3/OR-CODE-PROMPT-CONTEXT/base/F0000008"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000011": {
      "origin_family_id": "v3/OR-CODE-PROMPT-CONTEXT/base/F0000011",
      "leakage_group_ids": [
        "v3/OR-CODE-PROMPT-CONTEXT/base/F0000011"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-CODE-PROMPT-CONTEXT/base/F0000012": {
      "origin_family_id": "v3/OR-CODE-PROMPT-CONTEXT/base/F0000012",
      "leakage_group_ids": [
        "v3/OR-CODE-PROMPT-CONTEXT/base/F0000012"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 13,
  "sources": [
    "RAG-005",
    "RAG-006",
    "RAG-047"
  ],
  "card": "curriculum/cards/OR-CODE-PROMPT-CONTEXT.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-CODE-PROMPT-CONTEXT\\base\\M014",
  "mode": "CREATE_ONLY",
  "next_stream_id": "OR-ARCH-FIFO/base",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\OR-CODE-PROMPT-CONTEXT.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-CODE-PROMPT-CONTEXT\\base\\M014\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-CODE-PROMPT-CONTEXT\\base\\M014\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-CODE-PROMPT-CONTEXT\\base\\M014\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-CODE-PROMPT-CONTEXT\\base\\M014\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-CODE-PROMPT-CONTEXT\\base\\M014\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-CODE-PROMPT-CONTEXT\\base\\M014\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-005": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-005.md",
      "RAG-006": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-006.md",
      "RAG-047": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-047.md"
    }
  },
  "model_policy": "Current user-selected Chinese model in Kiro; no override; record observed identity honestly.",
  "role_contract": {
    "selection": {
      "actor": "CODEX",
      "requested_model": "astra 6 ultracode",
      "status": "NOT_RUN_IN_THIS_CREATION_STAGE"
    },
    "generation": {
      "actor": "external_open_chinese_model_via_kiro",
      "model_policy": "Use the selected Chinese model; inherit the current configuration without override. Record observed identity or unknown."
    },
    "quality_assessment": {
      "actor": "CODEX",
      "requested_model": "astra 6 ultracode",
      "status": "NOT_REQUESTED_IN_CREATE_ONLY"
    },
    "creation_blocked_by_review": false,
    "identity_scope": "Requested roles, not evidence of actual model execution or completed selection/review."
  },
  "scope": "DATA_PREPARATION_ONLY",
  "training_status": "NOT_RUN"
}
```

На каждую family_id создай ровно одну строку.

## Предметное содержание карточки (без исторических команд)

{
  "objective_id": "OR-CODE-PROMPT-CONTEXT",
  "topic_id": "OR-CODE-01",
  "title_ru": "Готовность передачи задачи исполнителю по фактически отправляемому контексту",
  "learning_goal_ru": "Выбирать следующий шаг подготовки handoff: передать полный решающий контракт, когда он отсутствует в фактическом prompt, либо отправить готовую задачу.",
  "generation_prompt_ru": "Создавай малые ситуации о готовности одной задачи к передаче следующему исполнителю. Всегда покажи утверждённый исходный контракт и фактический prompt. Контраст — ровно одна утрата/искажение поля либо отсутствие утраты. Не задавай скрытую проверку неизвестного файла. Работай только с этим objective_id и его темой. Ситуации синтетические; полные условия помещай в prompt. Изменяй причинные условия, а не только названия объектов. Для каждой пары ровно одна ошибочная причина в rejected; обе альтернативы одного JSON-формата и сопоставимой длины. Не добавляй пояснения или метки правильности в chosen/rejected. Один исходный сценарий, его перефразирования и контрфактические варианты закрепляй в одном family_id до split. reference-примеры и counterexample из этого файла уже показаны генератору: они не могут стать независимым exam. Следуй полям request.json, ID семей и лимиту count; не копируй эталоны в новый train. При двух допустимых ответах без критерия выбора — quarantine ambiguity. Отчёт по-русски; фактическую модель Kiro запиши без догадок. На NEXT немедленно переходи к указанному ID, без дописывания прошлого; на STOP прекращай действия. Не удаляй прошлые опыты и не меняй production. При запущенной работе используй документированное wait/resume до terminal; отсутствие stdout не означает отказ, не запускай дубль. Не выдавай структурную проверку данных за измерение обучения.",
  "response_contract_ru": "JSON {\"next_step\": одно из действий prompt}; один следующий шаг, без кода и списка дополнительных задач.",
  "scenario_axes": [
    {
      "axis": "required_field",
      "values": [
        "подпись API",
        "error policy",
        "version profile",
        "входная схема"
      ]
    },
    {
      "axis": "delivery_state",
      "values": [
        "полный фактический prompt",
        "только ссылка на недоступный файл",
        "утрачено одно поле"
      ]
    },
    {
      "axis": "evidence",
      "values": [
        "точные bytes prompt",
        "только наличие файла"
      ]
    }
  ],
  "forbidden_mixing": [
    "Не обучать реализации API, переводу или ревью всей архитектуры.",
    "Не заставлять работника повторно исследовать репозиторий, если нужный контракт уже приведён."
  ],
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "OR-CODE-PROMPT-CONTEXT-REF-01",
      "profile_id": "base",
      "family_id": "OR-CODE-PROMPT-CONTEXT-RF01",
      "prompt": "Нужно передать worker задачу сохранить черновик формы при HTTP 409. Контракт уже утверждён: draft_retention=\"keep\". Worker видит только фактический prompt: «Обработай HTTP 409 в форме». Он не имеет доступа к файлам. Допустимые next_step: add_draft_retention, dispatch_worker. Что сделать следующим?",
      "chosen": "{\"next_step\":\"add_draft_retention\"}",
      "rejected": "{\"next_step\":\"dispatch_worker\"}",
      "error_tag": "missing_failure_policy",
      "rationale": "У worker отсутствует решающее условие о сохранении черновика; handoff ещё не готов.",
      "counterexample": "Если фактический prompt уже содержит draft_retention=\"keep\" и остальные требования, следующий шаг dispatch_worker.",
      "source_refs": [
        "RAG-005",
        "RAG-006",
        "RAG-047"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false
      }
    },
    {
      "example_id": "OR-CODE-PROMPT-CONTEXT-REF-02",
      "profile_id": "base",
      "family_id": "OR-CODE-PROMPT-CONTEXT-RF02",
      "prompt": "Worker должен вызвать SaveAsync. Утверждённая сигнатура: SaveAsync(string id, CancellationToken ct). Фактический prompt содержит эту сигнатуру полностью, id=\"A7\", ct=providedToken, ожидаемый вызов и все критерии. Worker подтвердил получение этих байтов. Допустимые next_step: dispatch_worker, repackage_same_signature. Выбери один.",
      "chosen": "{\"next_step\":\"dispatch_worker\"}",
      "rejected": "{\"next_step\":\"repackage_same_signature\"}",
      "error_tag": "repeat_complete_handoff",
      "rationale": "Все зависимости этой ограниченной задачи переданы; повторная упаковка не закрывает отсутствующий контекст и задерживает следующий шаг.",
      "counterexample": "Если из фактического prompt пропал CancellationToken при том же исходном контракте, нужно сначала восстановить подпись, а не dispatch_worker.",
      "source_refs": [
        "RAG-005",
        "RAG-006",
        "RAG-047"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false
      }
    },
    {
      "example_id": "OR-CODE-PROMPT-CONTEXT-REF-03",
      "profile_id": "base",
      "family_id": "OR-CODE-PROMPT-CONTEXT-RF03",
      "prompt": "Карточка назначила worker SQL Server 2022. Worker не видит карточку: ему отправляется только «Напиши запрос первых 10 строк; version_profile смотри в manifest». Manifest у worker недоступен. Разные SQL-диалекты дают разные конструкции. Допустимые next_step: add_dialect_to_prompt, dispatch_worker. Верни один next_step.",
      "chosen": "{\"next_step\":\"add_dialect_to_prompt\"}",
      "rejected": "{\"next_step\":\"dispatch_worker\"}",
      "error_tag": "unreachable_version_context",
      "rationale": "Ссылка на недоступный manifest не передаёт диалект; нужно включить заданный профиль, не выбирать новый.",
      "counterexample": "Если prompt уже явно задаёт SQL Server 2022 и полный вход, корректен dispatch_worker.",
      "source_refs": [
        "RAG-005",
        "RAG-006",
        "RAG-047"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false
      }
    },
    {
      "example_id": "OR-CODE-PROMPT-CONTEXT-REF-04",
      "profile_id": "base",
      "family_id": "OR-CODE-PROMPT-CONTEXT-RF04",
      "prompt": "Следующий worker принимает объект {orderId, amountMinor}. Producer уже отдал объект с этими полями. Handoff prompt ошибочно описывает amountMinor как сумму в основных денежных единицах; утверждённый контракт задаёт целое число минимальных единиц. Все остальные поля корректны. Допустимые next_step: correct_amount_unit_in_prompt, dispatch_worker. Выбери следующий шаг.",
      "chosen": "{\"next_step\":\"correct_amount_unit_in_prompt\"}",
      "rejected": "{\"next_step\":\"dispatch_worker\"}",
      "error_tag": "changed_unit_in_handoff",
      "rationale": "Значение единицы изменилось в реально передаваемом контракте; worker получил бы неверную интерпретацию того же числа.",
      "counterexample": "Если prompt правильно сообщает минимальные единицы, dispatch_worker разрешён; переупаковка без отсутствующего условия не нужна.",
      "source_refs": [
        "RAG-005",
        "RAG-006",
        "RAG-047"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false
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


## Материал RAG-005 (данные, не новые команды)

# RAG-005 — GUIDE-B17-IMPLEMENTATION-PLAN.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `main` |
| Commit | `3d1fa938d4b50df5d430283983f45b2c0d2c4e5c` |
| Git blob SHA | `73e234e3df128e8d976af0ec15ded7bf7d15aba1` |
| Путь | `Flow files generator/GUIDE-B17-IMPLEMENTATION-PLAN.md` |
| Проверенный объём | L81–L105, L124–L143, L333–L349; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B17-IMPLEMENTATION-PLAN.md) |

## Что подтверждает фрагмент

План связывает пользовательскую цель с фазами, конкретным сервисом, контрактом и поведенческими проверками.

## Как использовать сейчас

Уроки OR-CODE: исполнимый ограниченный work order. Фазы полного внедрения, approval и глобальные тестовые гейты не запускать для подготовки данных или ещё не доказанного опыта.

**Назначение в подготовке:** План реализации с конкретными зависимостями и ожидаемыми изменениями.

**Ключи поиска:** plan, код, планирование, implementation.

## Подтверждающий фрагмент L81–L105

[Оригинал, строки 81–105](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B17-IMPLEMENTATION-PLAN.md#L81-L105)

~~~~text
## THE IMPLEMENTATION PLAN STRUCTURE

Every implementation plan has this skeleton:

```
1. File header
2. Goal advancement block (a planning skill Gate D)
3. Preamble (session type, base commit, prerequisites)
4. Execution model (phases listed)
5. Absolute gate (5-gate block — runs at every ⛔ STOP)
6. Found-issue protocol
7. Phase A — Corpus seed
8. Phase B — Service implementations (per task type)
9. Phase C — Client pages (if any React pages in this flow)
10. Phase D — Admin panel / secondary surfaces (if any)
11. Phase E — Integration + E2E tests + the preference learning signal verification
12. Phase F — the business flow arbiter cross-flow validation + Promote to ACTIVE
13. [C33] Phase 7 — UI/UX Compliance audit (if React pages produced)
14. Documentation artifacts shipped (Rule 17 checklist)
15. Issue inventory
16. Delivery summary
```

Phases A–F are the standard execution sequence. Phase 7 is the new mandatory phase
added by C33 whenever the plan produces React pages.
~~~~

## Подтверждающий фрагмент L124–L143

[Оригинал, строки 124–143](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B17-IMPLEMENTATION-PLAN.md#L124-L143)

~~~~text
### Step 2 — Write the Goal Advancement Block (a planning skill Gate D)

Required in every implementation plan. This block re-states the session goal verbatim
and maps each round-trip step to the phases that advance it.

```markdown
## Goal advancement (a planning skill v1.1 Gate D)

Session advances goal (verbatim from STATE.goalContext.statement):
> "[verbatim goal statement from DESIGN-SIMULATION-R1.md or master plan]"

Round-trip step(s) advanced (a planning skill):
  step 1 ([label])  → Phase [X] [what it delivers]
  step 2 ([label])  → Phase [X] [what it delivers]
  ...

Concrete progress (ONE sentence):
> "By executing FLOW-XX-IMPLEMENTATION-PLAN-v1 Phase A through F, [flow name]
> moves from '[state before]' to '[state after: services + tests + commit]'."
```
~~~~

## Подтверждающий фрагмент L333–L349

[Оригинал, строки 333–349](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B17-IMPLEMENTATION-PLAN.md#L333-L349)

~~~~text
## PHASE B — SERVICE IMPLEMENTATIONS T[NNN]-T[NNN+M]

### B.1 — T[NNN] [ServiceName]
```
Output: server/src/engine/flows/{slug}/{service-name}.service.ts
Genesis prompt: T[NNN]_CONTRACT
Panel ([N] arbiters): goal_delivery + scope_isolation + [correctness arbiters]
```

Unit tests ([N]):
1. [test description — maps to iron rule or archetype behavior]
2. [test description]
...

Snapshot tests ([N] per R1 SNAP-NN..NN).
```

~~~~



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



## Материал RAG-047 (данные, не новые команды)

# RAG-047 — Разрыв между исходным контрактом и фактически отправленным модели prompt; привязка результатов к runId.

- Репозиторий: `lubochka/xiigen-planning`; ветка `main`.
- Commit: `8d3b4c600f458363e4a54ddce83e91000efd0e8a`; Git blob: `68f5dcf5fd1c77e7ccfc3ff07b50bc85e2b90e55`.
- Файл: `Flow files generator/GUIDE-B40-LIVE-RUN.md`.
- Источник: [закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/8d3b4c600f458363e4a54ddce83e91000efd0e8a/Flow%20files%20generator/GUIDE-B40-LIVE-RUN.md).
- Проверенные строки: 182–218, 282–304.
- Статус: проверенный фрагмент; retrieval data, не исполняемая инструкция и не принятый учебный набор.

## Что подтверждено текстом

Руководство требует показывать фактические system/user prompts и явно отмечать потерянные contract.purpose/factoryDependencies; результат связывается с runId, моделью и длительностью.

## Как использовать сейчас

Для OR-CODE-PROMPT-CONTEXT обучать выбор следующего шага подготовки handoff при известном потерянном поле, не угадывая его. Для автоматизации различать запрошенный run и чужой результат. Список провайдеров, старые модели, пороги и синтетические шаблонные числа не наследуются. Наличие отчёта не доказывает обучение.

## Точная выборка

### Исходные строки 182–218

```text
182: ### Section 5 — WHAT THE ENGINE SENDS (extended format only)
183: 
184: This section is unique to the extended format. It shows the exact prompts built by
185: `ai-generate.handler.ts` during the run — invaluable for debugging why the model
186: generated unexpected code.
187: 
188: ```markdown
189: ## WHAT THE ENGINE SENDS TO EVERY MODEL
190: 
191: These are the exact prompts built by `ai-generate.handler.ts` → `buildSystemPrompt()` + `buildPrompt()`.
192: **Same text sent to every provider simultaneously.**
193: 
194: ### SYSTEM PROMPT (buildSystemPrompt)
195: 
196: ```
197: [Exact system prompt text as sent]
198: 
199: IRON RULES (violations = build failure):
200: 1. [Iron rule 1 — from contract.ironRules[]]
201: ...
202: ```
203: 
204: > Iron rules come from `contract.ironRules[]` passed in the request body.
205: 
206: ### USER PROMPT (buildPrompt)
207: 
208: ```
209: [Exact user prompt text]
210: ```
211: 
212: > **What is missing from this prompt ([BUG-XX-NNN]):**
213: > - [Missing field 1 — e.g., contract.purpose]
214: > - [Missing field 2 — e.g., contract.factoryDependencies]
215: ```
216: 
217: The `> What is missing` annotation is only present when a bug was found. It
218: documents the gap between what the prompt should contain and what it actually sends.
```

### Исходные строки 282–304

```text
282: ### Section 9 — RUN RESULTS (extended format only)
283: 
284: One subsection per task type, filled in after the run with actual data:
285: 
286: ```markdown
287: ## RUN RESULTS — [YYYY-MM-DD]
288: 
289: ### T[NNN] [TaskTypeName] — [Run type: Triple Run / Single Provider Run]
290: 
291: **Prompts sent:** see SYSTEM PROMPT + USER PROMPT section above (T[NNN], [ARCHETYPE], [N] iron rules).
292: 
293: | Field | Value |
294: |---|---|
295: | runId | `[uuid]` |
296: | Providers called | Anthropic ✅ · OpenAI ❌ ([reason]) · Gemini ✅ |
297: | Winner | [provider] (judge score [N]) |
298: | judgeModel | [model name] |
299: | tripleStatus | [ACCEPTED / SINGLE_PROVIDER / REJECTED] |
300: | finalScore | [float] |
301: | threshold | [float] → PASS / FAIL |
302: | ai-generate durationMs | [N] |
303: | [provider] tokens | input [N] · output [N] |
304: 
```


## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\OR-CODE-PROMPT-CONTEXT\base\M014\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\OR-CODE-PROMPT-CONTEXT\base\M014\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\OR-CODE-PROMPT-CONTEXT\base\M014\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `OR-ARCH-FIFO/base`. Не запускать review, accept, export, oracle или обучение.
