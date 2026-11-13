# LeftToWork-CONTINUE — M3 МАТЕРИАЛИЗАЦИЯ + СПАСАТЕЛЬНЫЙ АУДИТ (Трек C)

- **Дата:** 02.08.2026
- **Временная метка исполнения (execution timestamp, UTC):** 2026-08-02T10:26:42Z
- **Репозиторий наблюдения (read-only):** `C:\Projects\llm_mvp_core-DpoImplementation` — только чтение объектной базы git; ни один файл кода не создан и не изменён.
- **Разрешённый git в M3 (только чтение):** `ls-remote`, `rev-parse`, `log`, `show`, `diff` (по закоммиченным объектам), `merge-base`, `rev-list`, `cat-file`, `ls-tree`, `merge-tree --write-tree`, `reflog`, `stash list`, `worktree list`, `status --porcelain`, `remote -v`, `config --get`, плюс два расширения ТОЛЬКО для M3: `fsck --unreachable --no-reflogs --no-progress` и `count-objects -v`.
- **Запрещённый git (в M3 не исполнялся ни разу):** `fetch`, `pull`, `checkout`, `switch`, `merge`, `cherry-pick`, `commit`, `add`, `stash push`, `reset`, `clean`, `clone`, создание/удаление ветки, создание tag, `push`, а также `fsck --lost-found` (ЯВНЫЙ ЗАПРЕТ). Эти слова встречаются ниже ТОЛЬКО как цитируемый текст внутри НЕисполненной программы §2.
- **Природа документа:** ДОДАТОК (additive). 21 ранее лежащий файл папки не тронут; единственное изменение существующего файла — аддитивный дозапис одного датированного блока в конец `PLAN-REVIEW-FINDINGS-LEDGER.md` (шаг W4). Папка после M3 = 23 файла.
- **Цикл:** Cycle M3 of Track-C CONTINUE (кампания сходимости расширений Visual Studio / VS Code). Задача M3-T1 — RECOVERY + MATERIALIZATION.

> ⚠️ БАННЕР НЕ-ПЕРЕЗАЯВЛЕНИЕ (non-overclaim): все факты ниже приколоты к вершинам, наблюдаемым командами §1 и §4 в этот прогон. Сборки и тесты НЕ запускались. Клон базы Трека C НЕ выполнялся — программа §2 НАПИСАНА, но НЕ ИСПОЛНЕНА. Честное состояние ревью Трека C = **0/10 настоящих независимых ревью-циклов** (дословно, не смягчается); мои собственные проверки этого прогона — контроль качества написания (authoring-QC), а НЕ ревью-циклы. «Заявлено» не равно «реализовано». Слово «clone» (и любой запрещённый git-глагол) появляется в этом документе ТОЛЬКО внутри НЕисполненной программы §2 как цитата.

---

## §0 Человеческое резюме (простыми словами, без командных блоков)

**Что такое «программа материализации».** Материализация — это создание рабочей копии (свежего клона репозитория), в которую потом, в следующих циклах, переносится груз Трека C (41 коммит ветки `orchestrator-extensions`, из них эффективно не более 38). В этом цикле M3 я такую копию НЕ создаю. Я только ПИШУ пошаговую программу того, как её создать, чтобы позже её можно было выполнить по команде. Ни одна строка этой программы в M3 не запущена.

**Почему копия сейчас НЕ делается.** Для создания копии нужны две твои отмашки, и обе — твои, не мои: (1) отмашка, что две готовые линии — дизайн-в-код и DPO — уже влиты в главную ветку (иначе копировать не с чего — копия берётся с уже финализированной главной ветки); (2) отдельная отмашка на само создание копии. Пока ты не сказала «да» на обе — программа остаётся написанной, но не выполненной. Это git-действия, руками в коде я ничего не меняю, кнопку жмёшь ты.

**Что показал спасательный аудит (двумя фразами).** По «331»: следы найдены в главной ветке — два «зелёных среза» кода (`84bc6e916`, `a8a2d9da6`) и их файлы уже лежат внутри главной ветки, влиты через PR #38. По «545»: восстановить по git-уликам нечего и нечего проверять — само число 545 нигде в плане не определено (это зафиксировано в леджере), а следов удалённого рабочего дерева ни в одном из двух репозиториев нет; вердикт честный — восстановление из git-улик невозможно.

**Смена имени копии на v2.** В M1 копия планировалась по пути `...-orchestrator-extensions`. Начиная с этого цикла путь и имя ветки копии — `orchestrator-extensions-v2` (`C:\Projects\llm_mvp_core-orchestrator-extensions-v2`). Имя на сервере сейчас свободно (проверено §1), путь на диске отсутствует (проверено §1 и §7). Это не переоткрытие решения — это фиксация имени будущей копии, чтобы копия не столкнулась со старой веткой `orchestrator-extensions`, которая остаётся на месте как источник 41 коммита.

---

## §1 Пере-пин вершин (re-pin) — двойной пин server-leg / local-leg

Термины: `git ls-remote` — запрос серверу GitHub о вершинах веток БЕЗ скачивания истории (server-leg, серверная нога); `git rev-parse` — разворачивает локальную remote-tracking ссылку в её хэш (local-leg, локальная нога). Fetch НЕ выполнялся — совпадение ног означает, что локальные ссылки уже синхронны с сервером на момент прогона.

Команды и вывод verbatim (этот прогон, 2026-08-02T10:26:42Z):

```
# git ls-remote origin main codex/dpo-implementation orchestrator-extensions DesignToCode-clean DesignToCode-mainline-recovery-20260719
9e1de90f8adfae6bbbb10e2681012dbbf7af4a12	refs/heads/DesignToCode-clean
3b1ec63467176b0fdd31874a1d9c3ab1a34db9f9	refs/heads/DesignToCode-mainline-recovery-20260719
c834c88090d0396dc14053823c834288add5dfba	refs/heads/codex/dpo-implementation
afd26cbd292417967aa9cb90f8f56efac015fabb	refs/heads/main
b293045bd89b1eb96d3e32fedd341f1a4170adbc	refs/heads/orchestrator-extensions

# git ls-remote origin refs/heads/orchestrator-extensions-v2   (ДОЛЖНО быть пусто)
(вывод пуст; строки ref нет; V2_REMOTE_EXIT=0)

# git rev-parse codex/dpo-implementation origin/main origin/orchestrator-extensions origin/DesignToCode-clean origin/DesignToCode-mainline-recovery-20260719
c834c88090d0396dc14053823c834288add5dfba
afd26cbd292417967aa9cb90f8f56efac015fabb
b293045bd89b1eb96d3e32fedd341f1a4170adbc
9e1de90f8adfae6bbbb10e2681012dbbf7af4a12
3b1ec63467176b0fdd31874a1d9c3ab1a34db9f9

# git log -1 --format='%H %cI %s' codex/dpo-implementation
c834c88090d0396dc14053823c834288add5dfba 2026-08-02T13:12:36+03:00 Reject non-finite legacy checkpoints
```

| Ветка | server-leg (ls-remote) | local-leg (rev-parse) | Совпали? | Комментарий |
|-------|------------------------|-----------------------|----------|-------------|
| `main` (главная) | `afd26cbd292417967aa9cb90f8f56efac015fabb` | `afd26cbd292417967aa9cb90f8f56efac015fabb` | ДА | **НЕ сдвинулась** vs ожидание оркестратора (`afd26cbd2`) — финализации A+B ЕЩЁ НЕ произошло |
| `codex/dpo-implementation` (Трек B) | `c834c88090d0396dc14053823c834288add5dfba` | `c834c88090d0396dc14053823c834288add5dfba` | ДА | ДВИЖУЩАЯСЯ; текущая нога = `c834c8809` |
| `orchestrator-extensions` (Трек C, источник 41 коммита) | `b293045bd89b1eb96d3e32fedd341f1a4170adbc` | `b293045bd89b1eb96d3e32fedd341f1a4170adbc` | ДА | СТАТИЧНА с 10.07 |
| `DesignToCode-clean` (Трек A) | `9e1de90f8adfae6bbbb10e2681012dbbf7af4a12` | `9e1de90f8adfae6bbbb10e2681012dbbf7af4a12` | ДА | этот прогон ноги СОВПАЛИ (в M1 отличались: сервер `469d7e85f` / локально `f6172743`); значение = ожидаемый local-leg `9e1de90f8` |
| `DesignToCode-mainline-recovery-20260719` (recovery H2) | `3b1ec63467176b0fdd31874a1d9c3ab1a34db9f9` | `3b1ec63467176b0fdd31874a1d9c3ab1a34db9f9` | ДА | предок main (см. §4.2) |

**Дрейф `codex` относительно трёх контрольных ног.** Оркестратор фиксировал почасовые ноги codex: `7258be865` / `f53dc87e2` / `c834c8809`. Этот прогон: вершина codex = `c834c88090d0396…` = короткий `c834c8809`, то есть РАВНА третьей контрольной ноге. Reflog подтверждает раскладку: `c834c8809` = HEAD@{0} (текущая), `f53dc87e2` = HEAD@{2}, `7258be865` = HEAD@{28}. **Нового дрейфа сверх зафиксированного набора нет** — ветка стоит на последней известной ноге.

**Главная ветка НЕ сдвинулась (notable-проверка).** Ожидание = `afd26cbd2`; наблюдение = `afd26cbd2`. Значит A+B в main ещё НЕ финализированы; вся ancestry §4 пере-считана против ЭТОЙ (неизменной) main. Если бы main сдвинулась, это было бы признаком возможной финализации A+B — здесь этого нет.

**Свобода имени `orchestrator-extensions-v2`.** `git ls-remote origin refs/heads/orchestrator-extensions-v2` вернул пустой вывод (ни одной ref-строки; код возврата 0). Имя на сервере СВОБОДНО — конфликта при будущем `switch -c orchestrator-extensions-v2` нет.

**Машина: путь копии и хост-репо.**
```
# ls -d /c/Projects/llm_mvp_core-orchestrator-extensions-v2   → V2_PATH_ABSENT
# ls -d /c/Projects/claude-xiigen-smart-skills /c/Projects/cursor-xiigen-smart-skills /c/Projects/codex-xiigen-smart-skills → HOST_REPOS_ABSENT
# git -C /c/Projects/llm_mvp_core remote -v
origin	https://github.com/lubochka/xiigen-open-llm.git (fetch)
origin	https://github.com/lubochka/xiigen-open-llm.git (push)
```
- Клон будущей базы `C:\Projects\llm_mvp_core-orchestrator-extensions-v2` на диске ОТСУТСТВУЕТ (V2_PATH_ABSENT) — как и должно быть в конце M3.
- Три хост-репозитория ОТСУТСТВУЮТ (HOST_REPOS_ABSENT).
- **Путь `C:\Projects\llm_mvp_core` перепрофилирован:** теперь это клон `xiigen-open-llm` (не `llm_mvp_core`), на ветке `llm_plan3`. Факт зафиксирован; это НЕ дефект и НЕ чинится здесь. Репозиторий наблюдения Трека C — по-прежнему `C:\Projects\llm_mvp_core-DpoImplementation` (origin `llm_mvp_core`).

---

## §2 ПРОГРАММА (a) МАТЕРИАЛИЗАЦИЯ — НАПИСАНА, НЕ ИСПОЛНЕНА

> ⛔ НИ ОДНА КОМАНДА ЭТОЙ СЕКЦИИ НЕ ВЫПОЛНЯЛАСЬ В M3. Каждый блок `COMMAND` ниже — цитата будущей программы. Клон, switch, publish, tag — всё это только текст. Проверка §7 доказывает: путь `...-orchestrator-extensions-v2` в конце M3 ОТСУТСТВУЕТ, ни одной ветки/tag не создано.

**Шелл исполнения программы §2: Git Bash (POSIX sh) на Windows.** Все цитаты команд §2 рассчитаны на Git Bash; пути — в кавычках с прямыми слэшами (`/c/Projects/...`), а не с обратными.

### Три гейта (человеческим языком + тех.тег)

- **GATE-M3-A** (тех.тег `ab_finalization_go`): твоя отмашка, что линии A (`DesignToCode-clean`) и B (`codex/dpo-implementation`) ВЛИТЫ в главную ветку, И ты называешь её финальную вершину — переменная `$PIN_FINAL_MAIN`. Без названной вершины материализация не стартует: копия берётся именно с финализированной main, а не с движущейся codex.
- **GATE-M3-B** (тех.тег `materialize_go`): твоя отмашка на само создание копии (клон + ветка). Отдельно от GATE-M3-A: сказать «A+B влиты» и «делай копию сейчас» — два разных «да».
- **GATE-M3-C** (тех.тег `publish_go`): отдельная отмашка на публикацию ветки копии на сервер (push). Локальная копия и публикация — тоже два разных «да».
- **GATE-M3-D** (метод-гейт, конъюнкция): материализация MAT-02 и далее НЕ начинается, пока НЕ пройдено ≥10 настоящих независимых ревью-циклов (§ см. M5 §10 / M4 §6). GO-гейты A/B/C и метод-гейт связаны «И»; наличие названного пина при <10 циклах исполнение НЕ открывает.

### Шаги MAT-00 … MAT-12 (каждый в 7 полях)

**MAT-00 — Пере-пин в момент исполнения.**
- PRECONDITION: программа запускается; вершины могли уйти с 02.08.
- COMMAND (цитата): `git -C "$OBS" ls-remote origin main orchestrator-extensions; git -C "$OBS" rev-parse origin/main`
- EXPECTED: свежие server-leg/local-leg; сверка с `$PIN_FINAL_MAIN`, названным в GATE-M3-A.
- ROLLBACK: не применимо (read-only шаг).
- GATE: после GATE-M3-A.
- PIN-переменные: `$PIN_FINAL_MAIN`, `$PIN_OE=b293045bd…`.
- FOOTPRINT-класс: **read-only** (нулевой след).

**MAT-01 — Preflight диска и отсутствия копии.**
- PRECONDITION: копии ещё нет; на диске есть место.
- COMMAND (цитата): `ls -d /c/Projects/llm_mvp_core-orchestrator-extensions-v2 || echo ABSENT; git -C "$OBS" count-objects -v; df -h /c`
- EXPECTED: `ABSENT`; свободного места ≥ 3× `size-pack`. Этот прогон: `size-pack=394084 KB`, 3× ≈ **1154 MB**, свободно **95G** → запас огромный.
- ROLLBACK: если путь НЕ `ABSENT` — STOP, не перезаписывать существующее.
- GATE: после GATE-M3-B.
- PIN-переменные: `$SIZE_PACK_KB=394084`.
- FOOTPRINT-класс: **read-only**.

**MAT-02 — Свежий standalone-клон копии.**
- PRECONDITION: MAT-01 прошёл; путь `ABSENT`.
- COMMAND (цитата): `git clone https://github.com/lubochka/llm_mvp_core.git "/c/Projects/llm_mvp_core-orchestrator-extensions-v2"` (Git Bash: путь в кавычках с прямыми слэшами; в cmd/PowerShell — `C:\Projects\...`, но канон исполнения — Git Bash)
- EXPECTED: новый независимый клон по правилу «один клон = одна ветка», имя папки = `<проект>-<ветка>` = `llm_mvp_core-orchestrator-extensions-v2`.
- ROLLBACK: удалить свежесозданную папку целиком (отдельный GO), если клон прервался.
- GATE: после GATE-M3-B.
- PIN-переменные: `$V2=C:\Projects\llm_mvp_core-orchestrator-extensions-v2`.
- FOOTPRINT-класс: **new-clone** (создаёт новый репозиторий на диске; НЕ трогает репо наблюдения).
- ⚠️ ЯВНЫЙ ЗАПРЕТ: НИКОГДА не делать `git worktree add` от `llm_mvp_core-DpoImplementation` для получения копии. Копия — только отдельный `git clone`. Worktree связал бы копию с объектной базой репо наблюдения и нарушил бы «один клон = одна ветка».

**MAT-03 — Присутствие объекта `$PIN_FINAL_MAIN` в клоне (без равенства HEAD).**
- PRECONDITION: клон MAT-02 создан.
- COMMAND (цитата): `git -C "$V2" cat-file -e $PIN_FINAL_MAIN` (объект присутствует; БЕЗ проверки `HEAD == $PIN_FINAL_MAIN`) плюс `git -C "$V2" merge-base --is-ancestor $PIN_FINAL_MAIN origin/main` (exit 0: названный пин действительно на линии main — защита от опечатки пина, ветвящей копию не от той базы).
- EXPECTED: MAT-05 создаёт ветку копии ОТ `$PIN_FINAL_MAIN` явно (`switch -c orchestrator-extensions-v2 $PIN_FINAL_MAIN`), поэтому равенство HEAD клона с `$PIN_FINAL_MAIN` НЕ требуется и НЕ проверяется (default-ветка клона движется почасово — равенство дедлочило бы). Достаточно: объект `$PIN_FINAL_MAIN` присутствует в клоне (`git -C "$V2" cat-file -e $PIN_FINAL_MAIN`), от него и ветвимся.
- ROLLBACK: если объект `$PIN_FINAL_MAIN` ОТСУТСТВУЕТ в клоне (`cat-file -e` даёт ненулевой код) ИЛИ пин не предок `origin/main` (`is-ancestor` даёт ненулевой код) — STOP, вернуться к GATE-M3-A и пере-подтвердить вершину, НЕ продолжать.
- GATE: контрольный, после MAT-02.
- PIN-переменные: `$PIN_FINAL_MAIN`.
- FOOTPRINT-класс: **read-only**.

**MAT-04 — Фиксация autocrlf + `.gitattributes`.**
- PRECONDITION: клон на месте; репо наблюдения имеет `core.autocrlf=true` (этот прогон).
- COMMAND (цитата): `git -C "$V2" config --get core.autocrlf; git -C "$V2" check-attr text -- <образцы>`
- EXPECTED: осознанное решение по концу строк: сторона базы (main) авторитетна; аудитить hunk'и OE на CRLF/LF-шум до переноса, чтобы дубли по patch-id не ломались.
- ROLLBACK: не применимо (диагностический шаг).
- GATE: после MAT-03.
- PIN-переменные: `$AUTOCRLF=true`.
- FOOTPRINT-класс: **read-only** (только чтение конфигурации; правило `.gitattributes` — заготовка для M4/M5).

**MAT-05 — Создание ветки копии.**
- PRECONDITION: MAT-03 подтвердил вершину.
- COMMAND (цитата): `git -C "$V2" switch -c orchestrator-extensions-v2 $PIN_FINAL_MAIN`
- EXPECTED: локальная ветка `orchestrator-extensions-v2` от финализированной main; имя на сервере свободно (§1).
- ROLLBACK: удалить локальную ветку (`switch main` + `branch -d`), если создана ошибочно (отдельный GO).
- GATE: после GATE-M3-B.
- PIN-переменные: `$PIN_FINAL_MAIN`.
- FOOTPRINT-класс: **local-branch** (в клоне копии; репо наблюдения не тронут).

**MAT-06 — Изоляция от внешнего процесса.**
- PRECONDITION: ветка создана; параллельно машину пишет внешний codex-процесс.
- COMMAND (цитата): `git -C "$V2" status --porcelain | wc -l` (дважды, с интервалом ≥10 мин); `git -C "$V2" worktree list; git -C "$OBS" worktree list`
- EXPECTED: `porcelain`==0 в обоих замерах (копия чистая); списки worktree обоих репо — без перекрёстных регистраций (копия не зарегистрирована как worktree наблюдения и наоборот).
- ROLLBACK: если появился внешний мусор — не коммитить его; дождаться чистого окна.
- GATE: после MAT-05.
- PIN-переменные: `$V2`, `$OBS`.
- FOOTPRINT-класс: **read-only**.

Известные мелкие остатки (не блокеры, задокументированы для исполнения): (1) в базе есть 6 правил
`filter=lfs` — при клоне нужен установленный git-lfs (ни один из 34 конфликт-файлов LFS не затронут);
(2) MAT-06 «чистое окно» может ложно срабатывать на EOL-фантоме в свежем клоне (autocrlf=true) — при
пустом содержательном diff считать окно чистым; (3) check-1 гейта в no-fetch $V2 отражает снимок
клона на момент клонирования.

**MAT-07 — Заготовка порта: мега-коммит (только для M4/M5, НЕ исполняется).**
- PRECONDITION: ветка копии готова; мега-коммит `5b9ab429f` = 20409 файлов, из них ~19746 = `TestResults/` + `artifacts/` (карта M2 §2c).
- COMMAND (цитата, заготовка): `# M4/M5: перенос чистого эквивалента 5b9ab429f БЕЗ TestResults/ и artifacts/`
- EXPECTED: провизорный порт-дефолт = **чистый эквивалент без `TestResults/artifacts`** (соответствует опции C `luba_flag[mega_commit_port_strategy]` из M2 §9). Это заготовка; финальный выбор стратегии остаётся за Любой через тот luba_flag.
- ROLLBACK: не применимо (в M3 не исполняется).
- GATE: M4/M5 (не M3).
- PIN-переменные: `$MEGA=5b9ab429f`.
- FOOTPRINT-класс: **stub** (нулевой след в M3).

**MAT-08 — Заготовка порта: дубль-пары (только для M4/M5, НЕ исполняется).**
- PRECONDITION: две пары дублей по patch-id: `{73a75b99, cd1dd9ab}` (группа 97d2e67) и `{4c1d95e8, faf2c289}` (группа ea0b685) (карта M2 §2b).
- COMMAND (цитата, заготовка): `# M4/M5: из каждой дубль-пары перенести ОДИН эффективный патч`
- EXPECTED: провизорный порт-дефолт = **один на пару** (соответствует опции B `luba_flag[duplicate_twin_handling]` из M2 §9). Заготовка; финал — за Любой.
- ROLLBACK: не применимо (в M3 не исполняется).
- GATE: M4/M5.
- PIN-переменные: группы `97d2e67`, `ea0b685`.
- FOOTPRINT-класс: **stub**.

**MAT-09 — Заготовка порта: пустой мерж + счёт эффективных (только для M4/M5, НЕ исполняется).**
- PRECONDITION: мерж-коммит `1a283fd72` content-null (дерево == дерево обоих родителей, M2 §2a).
- COMMAND (цитата, заготовка): `# M4/M5: пустой мерж 1a283fd72 → skip (переносить нечего)`
- EXPECTED: провизорный порт-дефолт = **skip пустого мержа**; итог кратности: 41 − 1 пустой мерж − 2 дубля = **не более 38 эффективных коммитов**. Заготовка; финальное решение по мержу/дублям — за Любой (те же luba_flags M2 §9).
- ROLLBACK: не применимо (в M3 не исполняется).
- GATE: M4/M5.
- PIN-переменные: `$NULLMERGE=1a283fd72`, `$EFFECTIVE≤38`.
- FOOTPRINT-класс: **stub**.

**MAT-10 — Публикация ветки (GATE-M3-C).**
- PRECONDITION: перенос M4/M5 завершён; получена отдельная отмашка publish.
- COMMAND (цитата): `git -C "$V2" push -u origin orchestrator-extensions-v2`
- EXPECTED: ветка `orchestrator-extensions-v2` появляется на сервере (имя было свободно, §1).
- ROLLBACK: удаление удалённой ветки — отдельный GO (не автоматическое).
- GATE: **GATE-M3-C** (отдельная отмашка publish).
- PIN-переменные: `$V2`.
- FOOTPRINT-класс: **remote-publish** (по отдельному GO).

**MAT-11 — Опциональная архивная tag-программа старой ветки (НЕ исполняется, отдельный GO).**
- PRECONDITION: перенос 41 коммита на копию подтверждён.
- COMMAND (цитата): `git -C "$V2" tag archive/orchestrator-extensions-20260710 b293045bd…` + `git -C "$V2" push origin archive/orchestrator-extensions-20260710` (пример; НЕ исполнять без GO).
- EXPECTED: старая ветка `orchestrator-extensions` замораживается tag'ом как архив. Тег ДОЛЖЕН быть запушен на origin (иначе заморозка невидима на сервере и MAT-12 rollback его потеряет). Удаление старой ветки — ТОЖЕ отдельный GO, не входит в этот шаг.
- ROLLBACK: удалить tag (отдельный GO).
- GATE: отдельный GO (не GATE-M3-A/B/C).
- PIN-переменные: `$PIN_OE=b293045bd…`.
- FOOTPRINT-класс: **tag** (по отдельному GO; в M3 не исполняется).

**MAT-12 — Карта отката (rollback map).**
- PRECONDITION: любой шаг MAT-02..MAT-10 требует отмены.
- COMMAND (цитата): сводка обратных действий по каждому FOOTPRINT-классу (new-clone → удалить папку; local-branch → `branch -d`; remote-publish → удалить удалённую ветку; tag → удалить tag) — каждое ТОЛЬКО по явному GO.
- ПОРЯДОК (RC3-F07): rollback удаления клона (new-clone → удалить папку) выполнять ТОЛЬКО после того, как архив-тег MAT-11 запушен на origin (иначе локальный тег теряется вместе с папкой).
- EXPECTED: единая таблица «что откатывает что», чтобы ни один шаг не оставлял грязного состояния.
- ROLLBACK: сам себе (это и есть карта отката).
- GATE: справочный.
- PIN-переменные: все выше.
- FOOTPRINT-класс: **doc** (только эта запись).

---

## §3 ХОСТ-РЕПОЗИТОРИИ — существуют на гите (исправлено 02.08.2026 по уточнению Любы)

**Исправление к первичной формулировке.** Три хост-репозитория расширений СУЩЕСТВУЮТ на GitHub под `lubochka/` — имена были в документах, на машине они просто не склонированы. Проверено `git ls-remote` 02.08.2026:

```
https://github.com/lubochka/claude-xiigen-smart-skills.git  HEAD e4eaac7e1aa5d78765e299ce53a28cfcef8f13dc
https://github.com/lubochka/cursor-xiigen-smart-skills.git  HEAD 46c7282fa85f171075df1f79da8b184b5961ee7f
https://github.com/lubochka/codex-xiigen-smart-skills.git   HEAD 6f96dfa56f08e50c7ce850135f48a153511238d9
```

По записи `PLAN-REVIEW-FINDINGS-LEDGER:196` у каждого из них есть своя линия `orchestrator-extensions` — то есть конвергенция Трека C охватывает 6 репозиториев (ядро `llm_mvp_core` + 3 smart-skills-хоста + `xiigen-platform` + `xiigen-flows`), а не только ядро. Наличие ветки `orchestrator-extensions` в каждом хост-репо перепроверяется пофакторно в M4 через `ls-remote` (это НЕ вопрос «существуют ли репо» — они известны и живы).

Последовательность (рабочее решение, не блокер): сначала сводится ядро `llm_mvp_core` (M4), затем те же шаги программы материализации §2 повторяются для каждого хост-репо по правилу расположения — клон в `C:\Projects\<repo>-orchestrator-extensions-v2`, порт линии `orchestrator-extensions` этого репо на копию его финализированной `main`. Единственное, что остаётся Любе по хостам — момент подключения (сейчас / после ядра), а не «существуют ли».

---

## §4 СПАСАТЕЛЬНЫЙ АУДИТ (исполнен СЕЙЧАС, read-only)

Термины: `merge-base --is-ancestor A B` — проверка «коммит A лежит в истории B» (exit 0 = да, exit 1 = нет); `cat-file -t` — тип объекта; `fsck --unreachable` — перечисление недостижимых объектов объектной базы; «недостижимый» ≠ «потерянный контент» без аудита содержимого.

### §4.1 H1 — «зелёные срезы» дошли до main (Блок D verbatim)

```
# git cat-file -t 84bc6e916 ; git cat-file -t a8a2d9da6
commit
commit
# git merge-base --is-ancestor 84bc6e916 origin/main ; echo h1a_exit=$?
h1a_exit=0
# git merge-base --is-ancestor a8a2d9da6 origin/main ; echo h1b_exit=$?
h1b_exit=0
# git log --oneline --grep='#38' origin/main | head -3
38ef88ec0 Merge pull request #38 from lubochka/DesignToCode-clean
# git show --name-only --format='%H %cI %s' 84bc6e916
84bc6e916673799c4fc319e768d77718689cfcf5 2026-07-12T11:25:22+03:00 Preserve measured PNG dimensions at trust boundary
44.DesignConversionDomain/DesignConversionRenderEvidenceTrustBoundary.cs
Tests/DesignConversionRenderEvidenceTrustBoundaryTests.cs
# git show --name-only --format='%H %cI %s' a8a2d9da6
a8a2d9da62e6ff64e038dcb3d5468ffd7eb09236 2026-07-12T11:56:34+03:00 Authorize browser evidence with typed receipts
44.DesignConversionDomain/DesignConversionBrowserQaProviders.cs
44.DesignConversionDomain/DesignConversionQaContracts.cs
44.DesignConversionDomain/DesignConversionRenderEvidenceTrustBoundary.cs
Tests/DesignConversionRenderEvidenceTrustBoundaryTests.cs
# for p in <файлы a8a2d9da6>: rev-parse origin/main:$p
PRESENT_IN_MAIN_TREE 44.DesignConversionDomain/DesignConversionBrowserQaProviders.cs
PRESENT_IN_MAIN_TREE 44.DesignConversionDomain/DesignConversionQaContracts.cs
PRESENT_IN_MAIN_TREE 44.DesignConversionDomain/DesignConversionRenderEvidenceTrustBoundary.cs
PRESENT_IN_MAIN_TREE Tests/DesignConversionRenderEvidenceTrustBoundaryTests.cs
# git ls-tree --name-only origin/main 44.DesignConversionDomain/ | wc -l
51
```
Оба «зелёных среза» — коммиты; оба являются предками `origin/main` (exit 0); PR #38 (`38ef88ec0`) — на main; все 4 файла `a8a2d9da6` присутствуют в дереве main; каталог `44.DesignConversionDomain/` в main содержит 51 запись.

### §4.2 H2 — recovery-ветка (Блок E verbatim + логика коллапса H2→H1)

```
# git log -1 --format='%H %cI %s' 3b1ec6346
3b1ec63467176b0fdd31874a1d9c3ab1a34db9f9 2026-07-19T05:57:24+03:00 Merge origin/main after DesignToCode history rewrite
# git merge-base --is-ancestor 3b1ec6346 origin/main ; echo h2_exit=$?
h2_exit=0
# git merge-base origin/main 3b1ec6346
3b1ec63467176b0fdd31874a1d9c3ab1a34db9f9
# git diff --name-status origin/main 3b1ec6346 | wc -l
1265
```
**Логика коллапса H2→H1.** `h2_exit=0` → recovery `3b1ec6346` является предком main. Общий предок main и recovery == сам `3b1ec6346` → recovery целиком содержится в main. Значит в recovery-ветке НЕТ ничего, что не лежало бы уже в main → гипотеза H2 («что-то выживает только в recovery») СХЛОПЫВАЕТСЯ в H1. Число `1265` (diff main..recovery) — это forward-эволюция main ПОСЛЕ точки recovery (main ушла вперёд), а НЕ потери; информационно, не вычитается из «спасаемого».

### §4.3 H3 — 545 неотслеживаемых, удалённое рабочее дерево (Блок F verbatim)

```
# git -C OBS stash list | wc -l                 → 0
# git -C OBS reflog | wc -l                      → 36
# reflog head: HEAD@{35} = afd26cbd2 clone: from https://github.com/lubochka/llm_mvp_core.git
#              HEAD@{0}  = c834c8809 commit: Reject non-finite legacy checkpoints
# ls OBS/.git/worktrees | wc -l                  → 0
# git -C OBS worktree list
C:/Projects/llm_mvp_core-DpoImplementation c834c8809 [codex/dpo-implementation]
# ls -d /c/Projects/*orchestrator-extensions*    → NO_DIR_REMNANTS
# git -C /c/Projects/llm_mvp_core stash list | wc -l   → 0
# git -C /c/Projects/llm_mvp_core reflog | wc -l        → 3
# ls /c/Projects/llm_mvp_core/.git/worktrees | wc -l    → 0
# git -C OBS fsck --unreachable --no-reflogs --no-progress | awk | sort | uniq -c
    421 unreachable blob
     23 unreachable commit
    302 unreachable tree
```
**Перепрофилирование старого пути.** Путь `C:\Projects\llm_mvp_core` больше НЕ хранит `llm_mvp_core` — это клон `xiigen-open-llm` на ветке `llm_plan3` (§1). Поэтому «удалённое рабочее дерево orchestrator-extensions» на этой машине не имеет остатков (`NO_DIR_REMNANTS`), а reflog обоих репозиториев не содержит следа удаления worktree: reflog наблюдения (36 записей) прослеживается прямо до клона с `afd26cbd2`; reflog `xiigen-open-llm` (3 записи) короткий.

**Цитаты леджера (PLAN-REVIEW-FINDINGS-LEDGER.md, read-only):**
- **R2-01(б):** worktree `llm_mvp_core-orchestrator-extensions` был ЧИСТ (0 porcelain) и уже нёс реализационные коммиты (`b293045bd`…); «331 незакоммиченный файл» live в этой форме НЕ существует.
- **:206 (R2-01(в)):** «число 545 НЕ определено нигде в плане» — единственное вхождение как счётчик = `:1406` плана; все прочие «545» — это line-ref `:545–:584` (ScenarioUnit). Исполнитель не может проверить «0 из 331+545».

**Недостижимые объекты (fsck).** 421 недостижимый blob / 23 коммита / 302 tree. Сообщается ТОЛЬКО количество: недостижимые объекты НЕАТРИБУТИРУЕМЫ без аудита содержимого, а массовый `cat-file` blob'ов запрещён (и не выполнялся). Их наличие НЕ доказывает, что это «545» — это норма для packed-репозитория (перепакованные/осиротевшие объекты).

### §4.4 ВЕРДИКТ-331 (независимый)

Словарь вердиктов: **RECOVERED-IN-MAIN** (перечислимая улика лежит в main) / **PARTIALLY-EVIDENCED** (часть перечислима и найдена, часть неперечислима) / **UNRECOVERABLE-FROM-GIT-EVIDENCE** (по git-уликам восстановить/проверить нельзя).

**ВЕРДИКТ-331 = PARTIALLY-EVIDENCED.**
- Перечислимая часть (два «зелёных среза» `84bc6e916` + `a8a2d9da6` и их 2+4 файла, PR #38 `38ef88ec0`, 51 запись `44.DesignConversionDomain/`) = **RECOVERED-IN-MAIN** (§4.1, exit 0 + PRESENT_IN_MAIN_TREE).
- Остаток номинальной «331» = **UNRECOVERABLE-FROM-GIT-EVIDENCE**: канонического списка «331 файл» не существует нигде (леджер R2-01(б)), проверять «331 как множество» не против чего.

**Границы знания (331).** Вердикт вынесен ТОЛЬКО по перечислимым уликам, наблюдаемым read-only-командами §4.1. Полного списка 331 не существует ни в плане, ни в git; поэтому «полностью восстановлено» утверждать НЕЛЬЗЯ — доказано лишь, что найденные перечислимые срезы уже в main.

### §4.5 ВЕРДИКТ-545 (независимый)

**ВЕРДИКТ-545 = UNRECOVERABLE-FROM-GIT-EVIDENCE.**
- Число «545» не определено нигде в плане (леджер :206) — восстанавливать/проверять нечего и не против чего.
- Следов удаления рабочего дерева нет ни в одном из двух репозиториев: stash 0/0, worktrees-каталог 0/0, `worktree list` наблюдения = 1 запись, `NO_DIR_REMNANTS`, reflog прослеживается до клона (§4.3).
- 421 недостижимый blob существует, но НЕАТРИБУТИРУЕМ без аудита содержимого (запрещён) — не может быть приписан «545».

**Границы знания (545).** Восстановить множество, которое никогда не было определено, невозможно; недостижимые объекты есть, но атрибуция из одних git-метаданных невозможна. Это честный отрицательный вердикт, а не пропуск проверки.

**Два вердикта НЕЗАВИСИМЫ:** 331 и 545 оценены раздельно, на разных уликах; они не выводятся один из другого.

---

## §5 ОТКРЫТЫЕ РЕШЕНИЯ ЛЮБЫ — по сути одно (исправлено 02.08.2026 по уточнению Любы)

Из прежних «трёх» два оказались уже решёнными/задокументированными — я ошибочно вынес их в «открытые», закрываю их фактами ниже. Реально остаётся одно, и оно не продуктовое. Ничто из этого не блокирует M3–M6.

1. **Когда слить две готовые линии (дизайн-в-код и DPO) в главную ветку.** Само вливание — git-действие; я в коде руками ничего не делаю, поэтому кнопку «слить» жмёшь ты. От тебя нужно одно слово: сейчас или позже. Всплыло только потому, что Трек C стартует с копии главной ветки уже ПОСЛЕ этого слияния (GATE-M3-A требует, чтобы ты назвала вершину финализированной main). Продуктового выбора здесь нет — только момент и твоё «go». Рекомендации не даю — это твой выбор; не блокирует M3–M6, можно решить позже. (тех.тег: `ab_finalization_go`)

**ЗАКРЫТО (было ошибочно вынесено в «открытые» — исправлено фактами):**

- **Хост-репозитории существуют** (§3): три живых репо под `lubochka/` с подтверждёнными вершинами (`ls-remote` 02.08.2026), у каждого — своя линия `orchestrator-extensions` (конвергенция = 6 репо). Не «существуют ли» — существуют; известны URL и вершины. Остаётся лишь момент подключения (сейчас / после ядра) — не продуктовое решение. (бывший тех.тег: `host_repos_access` — переформулирован в «когда подключать»)

- **Лицензия — AGPL-3.0 only, решено давно.** Файл `LICENSE` есть и в `main`, и в ветке `PrepareToGPL`; коммит `e6e838e21e` (23.07) добавил AGPL-артефакты. Это не открытый выбор. Правило порта: файл и заголовки лицензии AGPL-3.0 сохраняются и переносятся как есть, не ослабляются. (бывший тех.тег: `license_gpl_agpl` — снят)

> Отдельно (не входит в тройку, но по тому же правилу — options-only, без рекомендаций): судьба старой ветки `orchestrator-extensions` после переноса (архив как есть / tag-заморозка MAT-11 / удаление — каждое отдельным GO) и три технические порт-заготовки MAT-07..09 (мега-коммит, дубль-пары, пустой мерж) остаются под соответствующими luba_flags M2 §9 — их финал за тобой.

---

## §6 МЕТОД-ГЕЙТ — протокол authoring-QC

- **Что аппендится:** ровно один датированный блок в конец `PLAN-REVIEW-FINDINGS-LEDGER.md` (шаг W4) — записка authoring-QC цикла M3 с именами и sha256 двух новых файлов и честными находками перечитки против чек-листа.
- **Почему это НЕ ревью-цикл:** authoring-QC — это контроль качества написания записки (присутствуют ли секции, верны ли числа, человеческий ли язык флагов). Настоящий независимый ревью-цикл — это отдельный проверяющий, перечитывающий план против 12 классов отказов с возвратным пакетом. Одна записка ≠ один цикл.
- **Счётчик независимых ревью-циклов НЕ изменён: 0/10** (историческое — на момент M3; актуальное — в леджере). M3 не добавил ни одного независимого цикла; мои проверки — authoring-QC.
- **Механика аппенда:** ТОЛЬКО `bash cat "fragment" >> "target"` (append-only). Доказательство префикса: `head -c PRE_SIZE ledger | sha256sum` == `PRE_SHA` (первые `PRE_SIZE` байт леджера побайтно неизменны).

---

## §7 ГИГИЕНА (footprint-zero)

- **Репо наблюдения — read-only.** `git status --porcelain` = 3 записи И ДО, И на выходе — это ВНЕШНИЙ codex-churn, НЕ мой след. Grep моей подписи (`lefttowork|m3-material|xiigen-planning|\.tmp`) в porcelain = **0** (S0 и S-END).
- **Папка планирования 21 → 23.** Было 21; создано ровно 2 новых файла (`LeftToWork-CONTINUE-M3-materialization.md`, `LeftToWork-CONTINUE-M3-materialization.ndjson`); изменён аддитивно ровно 1 существующий (леджер). Итог = 23.
- **Леджер append-only.** `PRE_SIZE=302585`, `PRE_SHA=6b465bbc2ca26fb025295ad071dc3d8a7a711027c87fc33ca25f8f19d32acb6a`. После аппенда: `head -c PRE_SIZE | sha256sum` == `PRE_SHA` (проверено в W4).
- **Путь копии `...-orchestrator-extensions-v2` — ОТСУТСТВУЕТ** на выходе (V2_PATH_ABSENT, §1 и W5). Клон НЕ выполнялся.
- **worktree list репо наблюдения не изменился** — одна запись `codex/dpo-implementation`. Ни одной ветки/tag не создано ни в одном репозитории.
- **NDJSON-файл: 33 записи** (см. §7-счёт): meta ×1, tip_pin ×5, mat_step ×13, gate ×3, host_repo_stub ×1, rescue_probe ×3, rescue_verdict ×2, luba_flag ×3, ledger_append ×1, hygiene ×1 = **33**.

---

## §8 Журнал обновлений

- 2026-08-02 (UTC 2026-08-02T10:26:42Z) — файл создан циклом M3 (Track-C CONTINUE, задача M3-T1: RECOVERY + MATERIALIZATION). Вершины перепроверены исполнителем в момент записи: `main afd26cbd292417967aa9cb90f8f56efac015fabb` (НЕ сдвинулась), `codex/dpo-implementation c834c88090d0396dc14053823c834288add5dfba` (= нога `c834c8809`), `orchestrator-extensions b293045bd89b1eb96d3e32fedd341f1a4170adbc` (статична), `DesignToCode-clean 9e1de90f8adfae6bbbb10e2681012dbbf7af4a12` (ноги совпали), `DesignToCode-mainline-recovery-20260719 3b1ec63467176b0fdd31874a1d9c3ab1a34db9f9`. Спасательный аудит исполнен read-only: ВЕРДИКТ-331 = PARTIALLY-EVIDENCED, ВЕРДИКТ-545 = UNRECOVERABLE-FROM-GIT-EVIDENCE. Программа §2 (материализация) НАПИСАНА, НЕ ИСПОЛНЕНА. Footprint в кодовых репозиториях = ноль; путь копии `...-orchestrator-extensions-v2` = ABSENT. Счётчик независимых ревью-циклов не изменён: 0/10.
