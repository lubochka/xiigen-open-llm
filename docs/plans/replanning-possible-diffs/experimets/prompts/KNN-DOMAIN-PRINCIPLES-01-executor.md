# Будущий исполнитель KNN-DOMAIN-PRINCIPLES-01

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход перед новым аудитом или заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

До действий прочитать выбранную карточку целиком и её локальную секцию120, source/raw и фактический маршрут producer→ordinary caller→learner→consumer; local HEAD, published SHA и dirty bytes различаются. Списки навыков ниже — дополнительные, не освобождение от полного свежего стека120. Исторические Tier3/vocab/split/UI/Git/model правила не расширяют нынешний паспорт и авторизацию.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.

**Сохранение постановки:** exact recorded result не повторять, old NOT_RUN не выше текущей Q-строки. Полный outer/nested бюджет и независимый gold остаются у карточки; пересчёт raw/структурный guard сTrain0 не заменяет обязательный учебный цикл и не получает фиктивный Train. Команды, API и фильтры не додумывать исполнителю. Вход обязателен также в передаваемом промпте выбранного ID.
<!-- EXECUTOR-CONTEXT-120:END -->


**Сейчас это документ задания, не разрешение запускать опыт.** Исполнение начинается только по прямому поручению Любы на реализацию/запуск этого ID или соответствующую ограниченную программу. Модель исполнителя — модель текущей сессии по умолчанию; не выбирать дорогую исследовательскую модель для runtime.

Прочитай [карточку](../cards/KNN-DOMAIN-PRINCIPLES-01.md) и [точные наборы](../cards/KNN-DOMAIN-PRINCIPLES-01-fixtures.md). Рабочий корень `C:/Projects/llm_mvp_core`, экспериментальный host `experiments-v2/RP01-MICRO/KDP01`, Attempt01; точная будущая команда и outputs в §8 карточки. Production код, старые experiments и learning-sets не менять. Не придумывать новый host, profile, TRAIN, вес функции, модель student или result path.

Применить адресно repo skills из `C:/Projects/llm_mvp_core/docs/skills/`: architect-behavior-classifier, planning-session-startup, plan-review, generated-code-review, prompt-patch-authoring, output-readability-gate и human-communication-GLOSSARY. Читать соответствующие `*-SKILL.md` по точным именам; glossary — `human-communication-GLOSSARY.md`. Использовать [контракт31](../31-tool-discovery-and-wait.md) для обнаружения реального launch/wait, [контракт32](../32-training-set-and-inference-contract.md) для учебной интерпретации. Полную загрузку не заявлять при чтении отдельных фрагментов.

1. В разрешённой фазе подготовь ровно экспериментальный overload и данные из Markdown. Внутри scorer оставь native ответ из памяти; применяй массив только к ранжированию.
2. Повторно используй совместимые S0/S1; если их нет и producer входит в прямое поручение, создай по одному на профиль. Иначе назови отсутствующий prerequisite, не обучай молча заново. Restore не считать новым учебным событием.
3. Выполни назначенные 21 плечо и контроли в бюджете. Сохрани failures, before/after и regressions всех профилей, включая неприменимые функции. Ненулевой learned signal обязателен; 0→0 нельзя продавать как успех по margin.
4. Любое измеренное полезное улучшение считается успехом по метрике/профилю; рост accuracy не обязателен. Рядом все ухудшения. При ухудшении контрольной строки не предлагай общий default.
5. Дай русский `report.md`: какие уроки вошли, что модель действительно выучила, A0/A1/B0/B1, какие ответы исправились/сломались, время/память, состояние и границы вывода. Не выводи приватное рассуждение; не ограничивайся status JSON.

**NAMED_CHECK KDP-ANSWER-FROM-MEMORY. WRONG:** `if async then return "await"` выдаёт ответ при пустой памяти. **CORRECT:** функция меняет score существующего learned candidate, EMPTY возвращает отсутствие ответа.

**NAMED_CHECK KDP-CAUSAL-PREFIX. WRONG:** Roslyn получает файл с правильным продолжением и создаёт query features по готовому дереву. **CORRECT:** parser видит только prefix до курсора, extractor не читает expected или искусственный суффикс.

**NAMED_CHECK KDP-PHRASE-CONTEXT. WRONG:** частое `hearing` подменяет правильное `hear` после `would like to`. **CORRECT:** обе строки остаются в EXAM и в отчёте, функция оценивается по данному контексту; EXAM не пополняет frequency state.

Entry выдаётся один раз. Ожидание через настоящий wait/continuation того же handle ограниченными интервалами≤60с до terminal; yield не означает падение. Не завершай ход словами «жду», не делай polling-loop, повторный старт или новую работу ради ожидания. При STOP прекрати действия; при NEXT сразу перейди к назначенному ID, не дописывая прежний опыт. Следующая карточка KDR01 не запускается без входящего в поручение разрешения.
