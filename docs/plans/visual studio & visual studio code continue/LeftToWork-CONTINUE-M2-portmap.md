# LeftToWork-CONTINUE — M2 PORT-MAP + ПЕРЕСЧЁТ КОНФЛИКТ-НАБОРА (Трек C)

- **Дата:** 31.07.2026
- **Временная метка исполнения (execution timestamp, UTC):** 2026-07-31T15:33:59Z
- **Репозиторий наблюдения (read-only):** `C:\Projects\llm_mvp_core-DpoImplementation` — только чтение объектной базы git (git object database); ни один файл кода не создан и не изменён. Разрешённый git: `ls-remote`, `rev-parse`, `log`, `show`, `diff` (между закоммиченными объектами), `merge-base`, `rev-list`, `patch-id`, `cat-file`, `merge-tree --write-tree`, `status --porcelain`, `worktree list`. Запрещённый git (НЕ выполнялся ни разу): `fetch`, `pull`, `checkout`, `switch`, `merge`, `cherry-pick`, `commit`, `add`, `stash`, `reset`, `clean`.
- **Природа документа:** ДОДАТОК (additive). **19 существующих файлов папки не тронуты.** Этот файл ничего существующего не редактирует; после записи двух новых файлов (`.md` + `.ndjson`) папка содержит 21 файл.
- **Цикл / задача:** Cycle M2 of Track-C CONTINUE / задача **M2-T1 — PORT-MAP**: классификация порт-листа из 41 коммита и пересчёт конфликт-набора evidence-grade (доказательно).
- **База математики (M2 math base):** локальная нога `codex/dpo-implementation` = `7258be865693cb63317713f6615c5478d5ef8fe3` (этот прогон). fetch НЕ выполнялся — база локальная, скачивание не требуется и запрещено.

> ⚠️ БАННЕР НЕ-ПЕРЕЗАЯВЛЕНИЕ (non-overclaim). Все факты ниже приколоты к вершинам (tips), наблюдаемым командами §1 в ЭТОТ прогон. Runtime (поведение при запуске) и качество кода НЕ проверялись — **запуск сборки/тестов в M2 ЗАПРЕЩЁН** и не выполнялся. «Заявлено» не равно «реализовано». Честное состояние ревью Трека C = **0/10 настоящих ревью-циклов** (дословно, не смягчается). Анализ «превзойдён / superseded» даёт только КАНДИДАТОВ по механическому критерию (§7); ВЕРДИКТЫ отложены к содержательному аудиту M4. Слово «fetch» появляется в этом документе ТОЛЬКО чтобы утверждать, что оно НЕ выполнялось.

---

## §0 Человеческое резюме (простыми словами, без командных блоков)

**Что такое порт-карта (port-map).** Груз Трека C — это **41 коммит** боковой ветки `orchestrator-extensions` (расширения для Visual Studio / VS Code), которых нет в главной линии. Их предстоит перенести (портировать) на будущую копию финализированного main. Порт-карта — это раскладка этих 41 коммита по типу, размеру и подсистеме плюс честный пересчёт того, сколько файлов при переносе столкнётся (конфликты слияния).

**История «41 → эффективно меньше».** Из 41 записи порт-листа одна — это мерж-коммит (merge commit, коммит-слияние) `1a283fd72`, который по содержимому ПУСТ: его дерево совпадает с деревом обоих родителей (content-null, см. §2a) — переносить в нём нечего. Ещё две пары коммитов — дубликаты по содержимому (identical patch-id, §2b): `{73a75b99, cd1dd9ab}` («latest») и `{4c1d95e8, faf2c289}` («Add net9 live-local test runner»). Если считать пустой мерж как ноль и каждую дубль-пару как один эффективный патч, «эффективно уникальных» получается **не более 38**. Это НЕ решение отбросить их — только факт кратности; что делать с мержем и дублями — вопрос Любы (luba_flags §9).

**Сколько файлов столкнётся.** Пересчёт слияния «на бумаге» (`git merge-tree --write-tree`, симуляция без касания рабочей копии) против ЖИВОЙ базы codex `7258be865` даёт **33 конфликтующих файла**, ВСЕ типа «CONFLICT (content)» (конфликт содержимого — обе стороны меняли одни и те же строки). Отдельно **50 файлов** слились автоматически, но правились С ОБЕИХ сторон (auto-merged both-sides) — их придётся ПРОВЕРИТЬ при переносе, хоть git и свёл их без ручного вмешательства. Историческое число «28» (из снимка 28 июля) считалось против МЁРТВОЙ старой вершины main и здесь заменяется свежими «33 / 50» (§6).

**Мега-коммит.** Один коммит порт-листа, `5b9ab429f` («latest changes»), — гигантский снимок (snapshot): **20409 файлов / 245071 вставка**. Из них ~19024 — папка `TestResults/` и ещё 722 — `artifacts/`, то есть ~19,7 тыс. записей это машинный мусор результатов тестов и артефактов, а не исходный код. Этот один коммит касается 24 из 33 конфликтующих файлов. Как его переносить (как есть / отфильтровав мусор / заменив чистым эквивалентом) — вопрос Любы (luba_flag §9).

**Что дальше.** M2 только РАЗМЕЧАЕТ. Настоящие вердикты (какой коммит нужен, какой уже впитан кодексом, как разрешать `.gitattributes` с учётом чувствительности к autocrlf) — задача содержательного аудита M4. Копия базы `C:\Projects\llm_mvp_core-orchestrator-extensions` по-прежнему НЕ существует и материализуется отдельным циклом M3 по GO Любы.

---

## §1 Пере-пин вершин (re-pin) — двойной пин server-leg / local-leg

Термины: `git ls-remote` — запрос серверу о вершинах веток, БЕЗ скачивания истории (даёт server-leg, серверную ногу); `git rev-parse <ref>` — локально разворачивает имя в 40-символьный хэш (local-leg, локальную ногу); хэш (sha) — идентификатор коммита.

Команды и вывод verbatim:

```
# UTC-метка прогона
2026-07-31T15:33:59Z

# git ls-remote origin main codex/dpo-implementation orchestrator-extensions DesignToCode-clean
469d7e85f876d19be909bd87c200b1c225c83349	refs/heads/DesignToCode-clean
7258be865693cb63317713f6615c5478d5ef8fe3	refs/heads/codex/dpo-implementation
afd26cbd292417967aa9cb90f8f56efac015fabb	refs/heads/main
b293045bd89b1eb96d3e32fedd341f1a4170adbc	refs/heads/orchestrator-extensions

# BASE=$(git rev-parse codex/dpo-implementation)  (локальная нога — база математики M2)
BASE=7258be865693cb63317713f6615c5478d5ef8fe3

# git cat-file -t b293045bd…            → commit
# git log -1 --format='%H %cI %s' $BASE
7258be865693cb63317713f6615c5478d5ef8fe3 2026-07-31T13:10:27+03:00 Add read-only capability product host
# git log -1 --format='%H %cI %s' $OE
b293045bd89b1eb96d3e32fedd341f1a4170adbc 2026-07-10T01:24:34+03:00 Bound DPO handoff trainer failures
```

| Ветка | Server-leg (`ls-remote`, этот прогон) | Local-leg (`rev-parse`) | Дата вершины | Движется / статична | Дрейф vs M1 |
|-------|----------------------------------------|--------------------------|--------------|---------------------|-------------|
| `main` (главная) | `afd26cbd292417967aa9cb90f8f56efac015fabb` | (объект в репо) | 2026-07-30T14:16:46+03:00 | статична vs ожидание | НЕТ (совпала с M1) |
| `codex/dpo-implementation` (Трек B, DPO) | `7258be865693cb63317713f6615c5478d5ef8fe3` | `7258be865693cb63317713f6615c5478d5ef8fe3` (== server) | 2026-07-31T13:10:27+03:00 | ДВИЖЕТСЯ почасово | НЕТ (== M1 == ожидание оркестратора) |
| `orchestrator-extensions` (Трек C, OE) | `b293045bd89b1eb96d3e32fedd341f1a4170adbc` | (remotes/origin) | 2026-07-10T01:24:34+03:00 | СТАТИЧНА с 10.07 | НЕТ (совпала с M1) |
| `DesignToCode-clean` (Трек A) | `469d7e85f876d19be909bd87c200b1c225c83349` | `f6172743881f19db33d18aac94dbd863e849c16d` (отстаёт) | — | ДВИЖЕТСЯ; local-leg информационный | НЕТ vs M1 (обе ноги совпали) |

**Дрейф относительно базы `7258be865`.** Оркестратор ожидал базу codex `7258be865693cb63317713f6615c5478d5ef8fe3`. Наблюдение этого прогона: server-leg == local-leg == `7258be865693cb63317713f6615c5478d5ef8fe3` — **дрейфа НЕТ, база стабильна между M1 (15:12Z) и M2 (15:33Z)**. Вся математика §2–§6 считается против локально присутствующего объекта `7258be865` без всякого скачивания. Если при повторном прогоне подвижная вершина codex уйдёт дальше — записать обе (M1-база и новая) и перепривязать таблицы; здесь расхождения нет.

---

## §2 Классификация 41 коммита (полная таблица)

Источник порт-листа: `git rev-list --right-only --cherry-pick 7258be865…...b293045bd…` (счёт строк = 41, подтверждён `wc -l`). Сортированное сравнение с порт-листом M1 §4: **IDENTICAL_TO_M1** (`diff` пуст) — набор из 41 не изменился.

Термины классификации: класс размера (size class) по числу изменённых файлов — **S** ≤ 5, **M** 6–30, **L** 31–500, **MEGA** > 500; бакет (bucket) — подсистема (top-level каталог); флаги (flags) — пометки-факты: `merge_null` (пустой мерж), `mega_snapshot` (гигантский снимок), `low_info_subject` (малоинформативный subject «latest»), `duplicate_twin:<group>` (дубль по patch-id).

| № | Полный sha (40-hex) | Дата | Subject | Размер | Бакет(ы) | Тема | Флаги |
|---|---------------------|------|---------|--------|----------|------|-------|
| 1 | `b293045bd89b1eb96d3e32fedd341f1a4170adbc` | 2026-07-10T01:24:34 | Bound DPO handoff trainer failures (= вершина OE) | S (2ф) | modelserver-25, tests | dpo-hardening | oe_tip |
| 2 | `b1cdc0a2236fbb76248d9964459351356425e71d` | 2026-07-10T01:18:05 | Bound DPO checkpoint bridge failures | S (2ф) | modelserver-25, tests | dpo-hardening | — |
| 3 | `c5faa9bdddf0ae42623f31daac879ed06269712a` | 2026-07-10T01:10:55 | Validate DPO dataset preference pairs | S (2ф) | dpo-11, tests | dpo-hardening | — |
| 4 | `590be3c72eedc5f286645af6e0135484bc34c219` | 2026-07-10T01:05:26 | Harden continue-training checkpoint path errors | S (2ф) | domain-44, tests | checkpoint-hardening | — |
| 5 | `f3dec253accc901dbe8d9c79741cf54bfb375f03` | 2026-07-10T00:58:41 | Clean checkpoint temp file on failed save | S (2ф) | domain-44, tests | checkpoint-hardening | — |
| 6 | `6a014c4f0d378fcf6f0940ccdd56cd9c73d01a88` | 2026-07-10T00:53:17 | Preserve checkpoint state on rejected imports | S (2ф) | domain-44, tests | checkpoint-hardening | — |
| 7 | `854638c1b1954e1ab6f090d5b826e3f75a57dd5c` | 2026-07-10T00:48:02 | Reject partial design conversion checkpoints | S (2ф) | domain-44, tests | checkpoint-hardening | — |
| 8 | `474bb4695357e4a580b809f5a04ad5db600520b4` | 2026-07-10T00:42:17 | Harden design conversion checkpoint paths | S (2ф) | domain-44, tests | checkpoint-hardening | — |
| 9 | `5bdfc7b28cd975c913f321c9c7bbaab3c5acb322` | 2026-07-10T00:35:37 | Redact target locator debug identities | S (2ф) | smartskills-43, tests | privacy-redaction | — |
| 10 | `7482ba30700fce4e3e0c7e412029427262775c42` | 2026-07-09T23:52:40 | Redact invalid Xiigen debug identities | S (2ф) | smartskills-43, tests | privacy-redaction | — |
| 11 | `2554c752a733ad439666a9bfae75e521c389ee31` | 2026-07-09T23:47:37 | Bound unsupported Xiigen service dispatch methods | S (2ф) | smartskills-43, tests | validation-hardening | — |
| 12 | `9dcf167872a87c23d870c43a84d2a4ade244ed23` | 2026-07-09T23:42:20 | Enforce exact Xiigen extension method tokens | S (2ф) | smartskills-43, tests | validation-hardening | — |
| 13 | `0dabee9d6e5973b7d948173611cbf5bced7b7dc8` | 2026-07-09T23:36:22 | Validate design conversion QA image refs | S (2ф) | domain-44, tests | qa-validation | — |
| 14 | `f72625807e03fc2fbe4d8ddf31647ee37cbed313` | 2026-07-09T23:30:55 | Validate design conversion QA identifiers | S (2ф) | domain-44, tests | qa-validation | — |
| 15 | `c70e1f7a5c20ce59c2b6ea935a67fc68e3c60243` | 2026-07-09T23:23:17 | Redact missing browser QA screenshot paths | S (2ф) | domain-44, tests | privacy-redaction | — |
| 16 | `7c532830d97c463c1da57a98c279fc949fa5d415` | 2026-07-09T21:58:36 | Reject unsafe Unicode identity characters | S (2ф) | smartskills-43, tests | validation-hardening | — |
| 17 | `105f344398f818f34acee4be033134dc89142010` | 2026-07-09T21:54:20 | Reject free-text eval runner identifiers | S (2ф) | smartskills-43, tests | validation-hardening | — |
| 18 | `b12675c56d115c7f5f5471c24cb521fce2a873d4` | 2026-07-09T21:48:31 | Reject duplicate browser QA viewport ids | S (2ф) | domain-44, tests | qa-validation | — |
| 19 | `82d46bb5bf7e012ad6850eb52726c92c9d8c37db` | 2026-07-09T21:44:08 | Guard xiigen extension request validation nulls | S (2ф) | smartskills-43, tests | validation-hardening | — |
| 20 | `e2360ac45d985632bba416b15f0ccfe0292b1fa7` | 2026-07-09T21:21:26 | Guard eval runner failure code payloads | S (2ф) | smartskills-43, tests | validation-hardening | — |
| 21 | `63f50866aa729ef384a56d50ea716ba3f5d9abc7` | 2026-07-09T21:15:18 | Reject path-like Xiigen extension identities | S (2ф) | smartskills-43, tests | validation-hardening | — |
| 22 | `9fd36447a8d60ab0f8f34f92310549061ca6f090` | 2026-07-09T21:10:34 | Validate browser QA manifest control characters | S (2ф) | domain-44, tests | qa-validation | — |
| 23 | `c0dfa1d788d41c4db686c459e03a71d8c650b47a` | 2026-07-09T21:02:37 | Validate browser QA manifest viewports | S (2ф) | domain-44, tests | qa-validation | — |
| 24 | `822d763c5e7f32e778f7afeef1d235e781b770a6` | 2026-07-09T20:27:58 | Validate Xiigen extension identity lengths | S (2ф) | smartskills-43, tests | validation-hardening | — |
| 25 | `ebce46c2f4d2d5e20c3efd353e74d6e64baf03d8` | 2026-07-09T20:08:57 | Harden eval-only encoded shared refs | S (2ф) | smartskills-43, tests | eval-hardening | — |
| 26 | `c613af83ff2d54bb392cfcdbea6c6a1c54f87870` | 2026-07-09T20:01:00 | Validate Xiigen extension identity controls | S (2ф) | smartskills-43, tests | validation-hardening | — |
| 27 | `316543c1f5ed9c8a6334cc98b9702fed9ef6c66b` | 2026-07-09T19:42:58 | Validate DesignConversion PNG chunk CRCs | S (2ф) | domain-44, tests | qa-validation | — |
| 28 | `e03bb05e5750030ae792831f7214f12f7eaef47e` | 2026-07-09T19:37:33 | Handle corrupt design conversion PNG evidence | S (2ф) | domain-44, tests | qa-validation | — |
| 29 | `d040856d19a071617bd6ef885d9890db85dfc558` | 2026-07-09T19:07:11 | Harden live-local render PNG proof | S (2ф) | tools, tests | qa-validation | — |
| 30 | `4d97bf29fa7adf05356e7b6ff11dce19850b3029` | 2026-07-09T18:57:14 | Harden eval-only shared evidence refs | S (2ф) | smartskills-43, tests | eval-hardening | — |
| 31 | `917a4a3e1f88ffaa493525f315aef012be77f555` | 2026-07-09T18:49:19 | Reject unsupported Xiigen extension methods | S (2ф) | smartskills-43, tests | validation-hardening | — |
| 32 | `1a283fd7244226cbc564025681d560c3c7cd3cf1` | 2026-07-09T16:35:20 | Merge remote-tracking branch 'origin/orchestrator-extensions'… | MERGE | (нет — пустой) | merge-null | **merge_null** |
| 33 | `e4db6e8883341980e7d7cb2c6ad8bc6d94711a11` | 2026-07-09T16:32:11 | Update orchestration extension state | S (1ф) | docs (STATE.json) | state-json | (1ф, +7487/−6957 строк) |
| 34 | `4c1d95e81f4d6895219129e81cf425086e301b91` | 2026-07-09T16:01:00 | Add net9 live-local test runner | S (1ф) | tools | tooling | **duplicate_twin:ea0b685** |
| 35 | `faf2c289276cc240e040f933454584dfb1ff2616` | 2026-07-09T15:47:55 | Add net9 live-local test runner | S (1ф) | tools | tooling | **duplicate_twin:ea0b685** |
| 36 | `73a75b994e5b0f73bfc5d74f444bb7e405fbca51` | 2026-07-09T13:11:50 | latest | M (7ф) | mixed (tests, tools, governance, domain-44, smartskills-43) | mixed-latest | **low_info_subject, duplicate_twin:97d2e67** |
| 37 | `cd1dd9abaec2230257635b227620ce65aa6ccf0b` | 2026-07-09T13:11:50 | latest | M (7ф) | mixed | mixed-latest | **low_info_subject, duplicate_twin:97d2e67** |
| 38 | `ed716810a01c3d3223365702a0f35cb60bc547d0` | 2026-07-09T07:23:59 | latest | M (8ф) | docs, governance-AGENTS-CLAUDE | docs-governance | **low_info_subject** |
| 39 | `5b9ab429f97527af7e659133478db3917b79fe79` | 2026-07-09T07:08:01 | latest changes | **MEGA (20409ф)** | junk-testresults, mixed | mega-snapshot | **mega_snapshot, low_info_subject** |
| 40 | `aa5d6f026ada9d775d1a915a3e34b62e19f8b854` | 2026-07-02T19:45:15 | governance: close ceremony-over-work loopholes… | M (7ф) | docs, governance-AGENTS-CLAUDE | governance | — |
| 41 | `e5a794819868a02dd51efd0bb6b605e18de41731` | 2026-07-01T19:14:32 | latest | M (9ф) | docs, governance-AGENTS, gitattributes | docs-governance | **low_info_subject** |

**Сводка размеров:** S = 35 (в т.ч. мерж-строка), M = 5 (№36,37,38,40,41), MEGA = 1 (№39). **Тематически** большинство (коммиты 1–31) — мелкие «hardening / validation / redaction» патчи по 2 файла (исходник + его тест). Оценка пригодности каждого к переносу и его реальной конфликтности — предмет M4, здесь НЕ выносится.

### §2a Мерж-экспансия — коммит `1a283fd72` (content-null, доказательство)

```
# git rev-parse 1a283fd72^{tree}  1a283fd72^1^{tree}  1a283fd72^2^{tree}
adae624ebddf623649fcf494fa2558a00732e03f      (собственное дерево)
adae624ebddf623649fcf494fa2558a00732e03f      (дерево родителя 1)
adae624ebddf623649fcf494fa2558a00732e03f      (дерево родителя 2)
```

Все три дерева РАВНЫ `adae624ebddf623649fcf494fa2558a00732e03f`. Дерево (tree) — снимок содержимого. Равенство «дерево коммита = дерево родителя-1 = дерево родителя-2» означает: этот мерж не внёс ни одного изменения содержимого — **content-null**. **Факт:** в карте файл→коммит (§5) этот мерж НЕ даёт ни одной строки, потому что `git show --name-only` для него пуст. Что с ним делать при переносе (пропустить / воссоздать структуру мержа) — luba_flag §9.

### §2b Дубль-пары по patch-id (все группы, оба sha каждая)

Термин: `git patch-id --stable` — хэш-отпечаток СОДЕРЖИМОГО диффа (одинаковые изменения → одинаковый patch-id, независимо от sha коммита). Проход по 40 не-мерж коммитам:

```
# группа patch-id 97d2e6733da157b43c1c6fd48ee3258cc29216d6
97d2e6733da157b43c1c6fd48ee3258cc29216d6 73a75b994e5b0f73bfc5d74f444bb7e405fbca51
97d2e6733da157b43c1c6fd48ee3258cc29216d6 cd1dd9abaec2230257635b227620ce65aa6ccf0b
# группа patch-id ea0b6851256f4d3f2a2db258698f7c19cb52ee5e
ea0b6851256f4d3f2a2db258698f7c19cb52ee5e 4c1d95e81f4d6895219129e81cf425086e301b91
ea0b6851256f4d3f2a2db258698f7c19cb52ee5e faf2c289276cc240e040f933454584dfb1ff2616
```

Две группы, по два коммита в каждой:
- **Группа 97d2e67** (subject «latest», 7 файлов, +491/−34): `73a75b994e5b0f73bfc5d74f444bb7e405fbca51` и `cd1dd9abaec2230257635b227620ce65aa6ccf0b` — совпали и по дате коммита (2026-07-09T13:11:50). Дубль-пара подтверждена и по patch-id, и по одинаковой статистике.
- **Группа ea0b685** (subject «Add net9 live-local test runner», 1 файл, +138): `4c1d95e81f4d6895219129e81cf425086e301b91` и `faf2c289276cc240e040f933454584dfb1ff2616`.

**Факт:** это 2 логических изменения, представленных 4 коммитами. Переносить обе копии или по одной из пары — luba_flag §9.

### §2c Мега-коммит `5b9ab429f` (shortstat + гистограмма top-dir verbatim)

```
# git show --shortstat --format= 5b9ab429f
 20409 files changed, 245071 insertions(+), 3576 deletions(-)
# git show --name-only --format= 5b9ab429f | cut -d/ -f1 | sort | uniq -c | sort -rn | head -10
  19024 TestResults
    722 artifacts
    286 docs
    210 experiments-v2
    106 Tests
     30 44.DesignConversionDomain
     16 25.ModelServer
      6 42.ProjectWikiDomain
      2 tools
      2 43.SmartSkillsDomain
```

**Факт:** 20409 файлов / 245071 вставка / 3576 удаления. Совпадает с эталоном оркестратора побайтно. Из них `TestResults/` (19024) + `artifacts/` (722) = **19746 записей** — машинный вывод результатов тестов и артефактов, а не исходники. Реального доменного кода в мега-коммите — десятки файлов (44.DesignConversionDomain 30, 25.ModelServer 16, Tests 106 и т.д.). Стратегия переноса мега-коммита — luba_flag §9.

### §2d Малоинформативные «latest»-коммиты (расширены по файловому следу)

Коммиты с subject «latest» / «latest changes» несут нулевую семантику в сообщении; их содержание восстанавливается только по файлам:
- `73a75b99` / `cd1dd9ab` — 7 файлов (3 теста + tools + AGENTS.md + domain-44 + smartskills-43), +491/−34 (дубль-пара, §2b).
- `ed716810` — 8 файлов (6 docs + CLAUDE.md + AGENTS.md), +237.
- `5b9ab429` — мега-снимок, §2c.
- `e5a79481` — 9 файлов (7 docs + AGENTS.md + `.gitattributes`), +1042.
Единственный из этой группы с осмысленным subject — `aa5d6f02` («governance: close ceremony-over-work loopholes»), 7 файлов, +1384/−1.

---

## §3 Пересчёт конфликт-набора (recompute) — команды и вывод verbatim

Термин: `git merge-tree --write-tree <base> <other>` — симуляция слияния «на бумаге»: git собирает результирующее дерево в объектной базе, НЕ трогая рабочую копию и НЕ создавая коммит; выход `exit 1` = обнаружены конфликты (это ОЖИДАЕМО, не ошибка). Первая строка вывода `--name-only` — OID (идентификатор) результирующего дерева; далее до пустой строки — список конфликтующих путей.

```
# git merge-tree --write-tree --name-only 7258be865… b293045bd…   ; echo exit=$?
exit=1
# head -1 (toplevel tree OID результата слияния):
33bc1aca32a5ecc800de252f6481a43e3a79a1c3
# список конфликтующих путей (до пустой строки) | wc -l:
33
# git merge-tree --write-tree 7258be865… b293045bd… ; grep '^CONFLICT' | тип | uniq -c:
     33 CONFLICT (content)
# Auto-merging строк всего: 83   →   clean-auto (83 − 33): 50
```

**Итоги пересчёта против ЖИВОЙ базы `7258be865`:** exit 1 (конфликты есть, ожидаемо); OID результирующего дерева `33bc1aca32a5ecc800de252f6481a43e3a79a1c3`; **33 конфликтующих файла, ВСЕ типа «CONFLICT (content)»**; **50 файлов auto-merged-both-sides-clean** (git свёл автоматически, но правились с обеих сторон). Совпадает с эталоном (33 / 50).

**Три яруса модели переноса (three-tier):**
- **Ярус 1 — 33 конфликта (ручное согласование при переносе).** Полный список — §4/§5. Тип каждого: `content`.
- **Ярус 2 — 50 auto-merged-both-sides (обязательство проверки).** git свёл сам, но т.к. правились обе стороны — при переносе ПРОВЕРИТЬ. Полный список ниже.
- **Ярус 3 — односторонние остальные.** Файлы, изменённые лишь одной стороной; сливаются без риска, отдельно не перечисляются.

**Полный список Яруса 2 — 50 файлов auto-merged-both-sides:**

```
 1 11.DPO/DpoDataset.cs
 2 43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs
 3 43.SmartSkillsDomain/XiigenExtensionApiService.cs
 4 44.DesignConversionDomain/DesignConversionCheckpointContracts.cs
 5 44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs
 6 44.DesignConversionDomain/DesignConversionDpoDisplay.cs
 7 44.DesignConversionDomain/DesignConversionDpoStorage.cs
 8 44.DesignConversionDomain/DesignConversionManifestPublisher.cs
 9 44.DesignConversionDomain/DesignConversionQaContracts.cs
10 44.DesignConversionDomain/DesignConversionQaProviders.cs
11 44.DesignConversionDomain/DesignConversionTrainedUnits.cs
12 44.DesignConversionDomain/DesignConversionTrainingRunner.cs
13 Tests/BackupExportImportSharePortabilityProbeTests.cs
14 Tests/DesignConversionDpoDatasetBuilderTests.cs
15 Tests/DesignConversionMeasurementGateTests.cs
16 Tests/DesignConversionModelServerApiTests.cs
17 Tests/DesignConversionQaProviderTests.cs
18 Tests/DesignConversionRuntimeFlowTests.cs
19 Tests/DesignConversionTrainingPipelineTests.cs
20 Tests/DpoDatasetTests.cs
21 Tests/ExtensionEvalOnlyQualityRunnerTests.cs
22 Tests/ExtensionQualityGapRepairTests.cs
23 Tests/ExtensionQualitySemanticGates10NoOverclaimTests.cs
24 Tests/ExtensionQualitySemanticGates11RubricTests.cs
25 Tests/ExtensionQualitySemanticGates12AdaptiveImprovementTests.cs
26 Tests/ExtensionQualitySemanticGates13AdaptiveDisplayTests.cs
27 Tests/ExtensionQualitySemanticGates14ClosureTests.cs
28 Tests/ExtensionQualitySemanticGates15PromotionReadinessTests.cs
29 Tests/ExtensionQualitySemanticGates16CandidateProbeTests.cs
30 Tests/ExtensionQualitySemanticGates17ProductRuntimeVerificationTests.cs
31 Tests/ExtensionQualitySemanticGates18LiveHostTargetRollbackClosureTests.cs
32 Tests/ExtensionQualitySemanticGates19TargetLocatorPlumbingTests.cs
33 Tests/ExtensionToken04QualityScoringArtifactsTests.cs
34 Tests/Learn05FreshLoadRunnerTests.cs
35 Tests/Learn06HeldOutAblationRunnerTests.cs
36 Tests/Learn07RollbackRunnerTests.cs
37 Tests/PromptToTaskP6TrainingCheckpointCandidateTests.cs
38 Tests/PromptToTaskP7FMissingCoverageRepairTests.cs
39 Tests/SmartSkillsSemanticQualityP25RuntimeHoldoutComparisonTests.cs
40 Tests/ThinClientApiContractTests.cs
41 Tests/XiigenExtensionApiContractsTests.cs
42 docs/SESSION-START-PROMPT.md
43 docs/skills/agent-output-format-SKILL.md
44 docs/skills/architect-behavior-classifier-SKILL.md
45 docs/skills/audit-protocol-SKILL.md
46 docs/skills/claim-verification-SKILL.md
47 docs/skills/phase-completion-packager-SKILL.md
48 docs/skills/plan-review-SKILL.md
49 docs/skills/planning-session-startup-SKILL.md
50 docs/skills/reference-inventory-and-source-map-SKILL.md
```

---

## §4 Бакеты конфликт-файлов (8 бакетов) — раскладка 33

```
# cut -d/ -f1 conf.txt | sort | uniq -c | sort -rn
     11 Tests
     11 44.DesignConversionDomain
      5 docs
      1 tools
      1 CLAUDE.md
      1 AGENTS.md
      1 43.SmartSkillsDomain
      1 25.ModelServer
      1 .gitattributes
```

| Бакет | Счёт | Содержание | Разрешение для M4 |
|-------|------|------------|-------------------|
| contract-43 (`43.SmartSkillsDomain`) | 1 | `XiigenExtensionApiContracts.cs` | ручное — контракт API, обе стороны правили |
| domain-44 (`44.DesignConversionDomain`) | 11 | Providers, Machine, Promotion, VisualJudge и др. | ручное — доменное ядро дизайн-конверсии |
| tests (`Tests/`) | 11 | DesignConversion*Tests.cs | ручное — тесты идут за доменом |
| governance (`AGENTS.md`, `CLAUDE.md`) | 2 | 2 файла управления | ручное — codex перевёл на англ. (i18n gpl) |
| docs (`docs/`) | 5 | HOW-TO-USE-SKILLS, шаблоны, SKILL.md, STATE.json | смешанно (см. §5: 2 из них НЕ superseded) |
| tools (`tools/`) | 1 | `DesignConversionLiveLocalServer/Program.cs` | ручное |
| gitattributes (`.gitattributes`) | 1 | правила EOL/бинарности | ручное — чувствительно к autocrlf (luba_flag §9) |
| modelserver-25 (`25.ModelServer`) | 1 | `DesignConversionDpoTrainingHandoff.cs` | ручное — НОВЫЙ вход vs историч. 28 (§6) |
| **ИТОГО** | **33** | | |

**Разбивка «ручное (Ярус 1) vs чистое-авто (Ярус 2)» для M4:** 33 файла требуют ручного согласования (Ярус 1); ещё 50 (§3) слились авто, но обе стороны правили — обязательны к проверке (Ярус 2). Итого зона внимания M4 = 33 + 50 = 83 файла.

---

## §5 Карта файл→коммиты (по конфликт-файлам) + кандидаты superseded

Термины: «касающие OE-коммиты» — какие из 41 порт-коммитов трогают этот файл; `codex_touch_total` = `git rev-list --count 6624161be..7258be865 -- <файл>` (сколько раз codex-сторона трогала файл после общей развилки); `codex_touch_after` — то же с `--since=2026-07-10`; `last_codex` — последний codex-коммит по файлу. **Механический критерий кандидата:** `CANDIDATE_SUPERSEDED := codex_touch_after ≥ 1`. Sha касающих коммитов даны в укороченной форме (первые 10), полные — в §2 и ndjson.

| # | Конфликт-файл | Касающие OE-коммиты (из 41) | codex total / after 07-10 | Последний codex-коммит | CAND. SUPERSEDED |
|---|---------------|------------------------------|---------------------------|------------------------|------------------|
| 1 | `.gitattributes` | e5a7948198 | 1 / 1 | `5586d08465` 07-28 quarantine sources | **yes** |
| 2 | `25.ModelServer/…/DesignConversionDpoTrainingHandoff.cs` | b293045bd8, b1cdc0a223, 5b9ab429f9 | 4 / 4 | `f0af554e40` 07-31 measurement+DPO provenance | **yes** |
| 3 | `43.SmartSkillsDomain/XiigenExtensionApiContracts.cs` | 9dcf167872, 7c532830d9, 82d46bb5bf, 63f50866aa, 822d763c5e, c613af83ff, 917a4a3e1f | 3 / 1 | `e6e838e21e` 07-23 AGPL license artifacts | **yes** |
| 4 | `44…/DesignConversionBrowserQaProviders.cs` | c70e1f7a5c, b12675c56d, 9fd36447a8, c0dfa1d788, 316543c1f5, e03bb05e57, 73a75b994e, cd1dd9abae, 5b9ab429f9 | 44 / 42 | `33a8cfd939` 07-30 merge DTC+DPO A-D | **yes** |
| 5 | `44…/DesignConversionDpoGenerator.cs` | 5b9ab429f9 | 2 / 2 | `3a0ff04126` 07-27 responsive viewport | **yes** |
| 6 | `44…/DesignConversionInternalService.cs` | 5b9ab429f9 | 26 / 26 | `33a8cfd939` 07-30 merge DTC+DPO A-D | **yes** |
| 7 | `44…/DesignConversionLocalContinueTrainingProbe.cs` | 590be3c72e, 5b9ab429f9 | 1 / 1 | `e6e838e21e` 07-23 AGPL license | **yes** |
| 8 | `44…/DesignConversionLocalFreshLoadProbe.cs` | 5b9ab429f9 | 2 / 2 | `e6e838e21e` 07-23 AGPL license | **yes** |
| 9 | `44…/DesignConversionMachine.cs` | f3dec253ac, 6a014c4f0d, 854638c1b1, 474bb46953, 5b9ab429f9 | 15 / 15 | `83245a2039` 07-31 merge DTC into DPO | **yes** |
| 10 | `44…/DesignConversionPromotion.cs` | 5b9ab429f9 | 6 / 6 | `83245a2039` 07-31 merge DTC into DPO | **yes** |
| 11 | `44…/DesignConversionThinClientJobRuntime.cs` | 5b9ab429f9 | 30 / 29 | `33a8cfd939` 07-30 merge DTC+DPO A-D | **yes** |
| 12 | `44…/DesignConversionTrainingPipeline.cs` | 5b9ab429f9 | 2 / 2 | `e6fe1c481f` 07-27 exports checkpoint-backed | **yes** |
| 13 | `44…/DesignConversionVisualJudge.cs` | 5b9ab429f9 | 27 / 26 | `33a8cfd939` 07-30 merge DTC+DPO A-D | **yes** |
| 14 | `44…/DesignConversionVisualQaBoundaryPolicy.cs` | 5b9ab429f9 | 23 / 22 | `33a8cfd939` 07-30 merge DTC+DPO A-D | **yes** |
| 15 | `AGENTS.md` | 73a75b994e, cd1dd9abae, ed716810a0, 5b9ab429f9, aa5d6f026a, e5a7948198 | 140 / 7 | `da6f9b8553` 07-23 i18n governance EN | **yes** |
| 16 | `CLAUDE.md` | ed716810a0, 5b9ab429f9, aa5d6f026a | 104 / 5 | `da6f9b8553` 07-23 i18n governance EN | **yes** |
| 17 | `Tests/DesignConversionBrowserQaProviderTests.cs` | c70e1f7a5c, b12675c56d, 9fd36447a8, c0dfa1d788, 316543c1f5, e03bb05e57, 73a75b994e, cd1dd9abae, 5b9ab429f9 | 41 / 39 | `33a8cfd939` 07-30 merge DTC+DPO A-D | **yes** |
| 18 | `Tests/DesignConversionCheckpointLifecycleTests.cs` | 590be3c72e, f3dec253ac, 6a014c4f0d, 854638c1b1, 474bb46953 | 12 / 12 | `f0af554e40` 07-31 measurement+DPO provenance | **yes** |
| 19 | `Tests/DesignConversionDpoGeneratorTests.cs` | 5b9ab429f9 | 2 / 2 | `3a0ff04126` 07-27 responsive viewport | **yes** |
| 20 | `Tests/DesignConversionDpoStorageTests.cs` | 5b9ab429f9 | 13 / 13 | `33a8cfd939` 07-30 merge DTC+DPO A-D | **yes** |
| 21 | `Tests/DesignConversionInternalServiceTests.cs` | 5b9ab429f9 | 20 / 20 | `d57a3630a8` 07-29 fail closed w/o visual judge | **yes** |
| 22 | `Tests/DesignConversionLiveLocalServerTests.cs` | d040856d19, 73a75b994e, cd1dd9abae, 5b9ab429f9 | 47 / 41 | `5ef090d56c` 07-31 live-local ready-file | **yes** |
| 23 | `Tests/DesignConversionMachineTests.cs` | 5b9ab429f9 | 5 / 5 | `522a1b83f7` 07-31 held-out gate | **yes** |
| 24 | `Tests/DesignConversionPromotionTests.cs` | 5b9ab429f9 | 6 / 6 | `83245a2039` 07-31 merge DTC into DPO | **yes** |
| 25 | `Tests/DesignConversionThinClientJobRuntimeTests.cs` | 5b9ab429f9 | 21 / 21 | `33a8cfd939` 07-30 merge DTC+DPO A-D | **yes** |
| 26 | `Tests/DesignConversionVisualJudgeTests.cs` | 5b9ab429f9 | 27 / 27 | `33a8cfd939` 07-30 merge DTC+DPO A-D | **yes** |
| 27 | `Tests/DesignConversionVisualQaBoundaryPolicyTests.cs` | 5b9ab429f9 | 15 / 14 | `33a8cfd939` 07-30 merge DTC+DPO A-D | **yes** |
| 28 | `docs/HOW-TO-USE-SKILLS.md` | 5b9ab429f9 | 1 / 1 | `da6f9b8553` 07-23 i18n governance EN | **yes** |
| 29 | `docs/RUSSIAN-STATUS-AND-EXECUTION-LOG-TEMPLATE.md` | 5b9ab429f9 | 1 / 1 | `da6f9b8553` 07-23 i18n governance EN | **yes** |
| 30 | `docs/plans/ORCHESTRATION-AND-PROMPT-TO-TASK-EXTENSIONS-STATE.json` | e4db6e8883 | 0 / 0 | `58b0e71846` 06-07 backfill host review authority | **no** |
| 31 | `docs/skills/output-readability-gate-SKILL.md` | 5b9ab429f9 | 1 / 0 | `8db88bf846` 07-09 latest | **no** |
| 32 | `docs/skills/verification-before-completion-SKILL.md` | ed716810a0, 5b9ab429f9, aa5d6f026a | 2 / 1 | `ca497462f6` 07-23 scrub workspace paths | **yes** |
| 33 | `tools/DesignConversionLiveLocalServer/Program.cs` | d040856d19, 73a75b994e, cd1dd9abae, 5b9ab429f9 | 38 / 34 | `5ef090d56c` 07-31 live-local ready-file | **yes** |

**Факт:** мерж `1a283fd72` не вносит НИ ОДНОЙ строки в эту карту (§2a, content-null) — он не касается ни одного из 33 файлов. Кандидатов superseded — **31 из 33** (yes); НЕ-кандидатов — **2** (`docs/plans/…STATE.json` — codex не трогал вовсе; `docs/skills/output-readability-gate-SKILL.md` — codex трогал 1 раз, но ДО 10 июля).

---

## §6 Дельта против исторической «28» (честное ограничение)

**Историческое ограничение.** Снимок `BRANCH-STATUS-CURRENT-2026-07-28.md` считал «28 конфликтов» против МЁРТВОЙ старой вершины main `df379c624` (`git merge-tree --write-tree df379c624 b293045bd`). Он **не перечислял 28 файлов пофайлово** — давал только КАТЕГОРИИ. Причём внутри самого документа две разные раскладки: тело события S3 писало «10 файлов домена, 12 тестов», а закрывающая коррекция (Цикл 5) — «domain-44 9 + tests 10 + contract 1 + AGENTS + CLAUDE + docs 5 + tools 1 = 28» и признала группировку S3 неточной. Обе исторические рамки приводятся; текущее число — 33.

| Категория | Историч. «28» (коррекция 07-28, база df379c624 МЁРТВА) | Текущее (33, база 7258be865 ЖИВА) | Дельта |
|-----------|--------------------------------------------------------|-----------------------------------|--------|
| contract-43 | 1 | 1 | 0 |
| domain-44 | 9 (тело S3 писало 10) | 11 | +2 (vs 9) |
| tests | 10 (тело S3 писало 12) | 11 | +1 (vs 10) |
| governance (AGENTS+CLAUDE) | 2 | 2 | 0 |
| docs | 5 | 5 | 0 |
| tools | 1 | 1 | 0 |
| **gitattributes** | 0 (отсутствовал) | 1 | **+1 новый вход** |
| **modelserver-25** | 0 (отсутствовал) | 1 | **+1 новый вход** |
| **ИТОГО** | **28** | **33** | **+5** |

**Новые входы (new entrants) vs историческая 28:** `.gitattributes` и `25.ModelServer/DesignConversion/DesignConversionDpoTrainingHandoff.cs` — двух этих категорий в раскладке 28 не было. **Старая база `df379c624` МЁРТВА** (главная ушла на `afd26cbd…`), поэтому историческая «28» здесь ЗАМЕНЯЕТСЯ на «33 / 50» против живой базы. Пофайловое сравнение с 28 невозможно — исторический снимок имён файлов не сохранил.

---

## §7 Кандидаты superseded — критерий, список, отложенный вердикт

**Механический критерий (только он, без оценки качества):** файл помечается `CANDIDATE_SUPERSEDED = yes`, если codex-сторона трогала его хотя бы раз ПОСЛЕ 2026-07-10 (`git rev-list --count --since=2026-07-10 6624161be..7258be865 -- <файл> ≥ 1`). Это индикатор «на живой базе файл, вероятно, ушёл вперёд относительно версии OE», НЕ доказательство ненужности OE-патча.

**Кандидаты (31 из 33):** все конфликт-файлы §5, кроме двух ниже.

**НЕ кандидаты (2 из 33):**
- `docs/plans/ORCHESTRATION-AND-PROMPT-TO-TASK-EXTENSIONS-STATE.json` — codex не трогал вообще (total 0); последнее касание — `58b0e71846` от 2026-06-07.
- `docs/skills/output-readability-gate-SKILL.md` — codex трогал 1 раз, но ДО отсечки (последнее `8db88bf846` от 2026-07-09).

**Отложенный вердикт (дословно):** данный список — только КАНДИДАТЫ по механическому критерию касания. **Вердикты о том, какой OE-патч действительно превзойдён кодексом и не нуждается в переносе, требуют содержательного аудита M4** (сравнение диффов по существу, а не по факту касания). В M2 такие вердикты НЕ выносятся.

---

## §8 Гигиена (footprint-zero)

```
# S0 ДО:  git status --porcelain | wc -l                                    → 38 (внешний churn)
#         git status --porcelain | grep -icE 'lefttowork|…|xiigen-planning|\.tmp' → 0
#         ls "…/visual studio & visual studio code continue" | wc -l        → 19
# S7 ПОСЛЕ: git status --porcelain | wc -l                                  → 38 (внешний churn)
#           git status --porcelain | grep -icE '…' (footprint)              → 0
#           git worktree list → одна запись, ветка не менялась
```

- **Porcelain 38 ДО и ПОСЛЕ.** Репозиторий наблюдения активно пишется ВНЕШНИМ .NET codex-процессом; счётчик `git status --porcelain` дрейфует сам по себе. Это НЕ наш след и НЕ провал — дельта-по-счётчику здесь не критерий.
- **Footprint (мой след) = 0 ДО и ПОСЛЕ.** Grep по моим подписям (`lefttowork`, `m2-portmap`, `xiigen-planning`, `.tmp`) в porcelain = 0 в обоих замерах. Ни один файл кода не создан/изменён; ни одна ветка/ссылка не двигалась (`worktree list` без изменений).
- **Папка планирования 19 → 21.** До: 19 файлов. После записи `LeftToWork-CONTINUE-M2-portmap.md` + `.ndjson`: 21. Ни один из 19 существующих файлов не тронут.

---

## §9 Что дальше + luba_flags

**Указатели на следующие циклы:**
- **M4** — содержательный аудит: пофайловое сравнение диффов OE-патчей против живой базы, конверсия кандидатов superseded (§7) в вердикты, план ручного согласования 33 конфликтов + проверки 50 auto-both-sides.
- **M3** — материализация копии `C:\Projects\llm_mvp_core-orchestrator-extensions` по GO Любы; спасательный аудит исторической дельты; доступ к трём хост-репозиториям.

**luba_flags (факты + опции; решает Luba, не исполнитель):**

- `luba_flag[mega_commit_port_strategy]` — стратегия переноса мега-коммита `5b9ab429f` (20409 файлов, из них ~19746 = `TestResults/` + `artifacts/`). Опции: (A) переносить как есть, включая ~19,7 тыс. записей `TestResults/`/`artifacts/`; (B) отфильтровать (исключить `TestResults/`, `artifacts/`), перенести только код/доки; (C) заменить эквивалентным чистым коммитом; (D) отложить к M4. РЕКОМЕНДАЦИИ НЕТ.
- `luba_flag[duplicate_twin_handling]` — обращение с дубль-парами по patch-id (`{73a75b99, cd1dd9ab}` группа 97d2e67; `{4c1d95e8, faf2c289}` группа ea0b685). Опции: (A) переносить обе копии каждой пары; (B) по одному коммиту из пары; (C) отложить к M4. РЕКОМЕНДАЦИИ НЕТ.
- `luba_flag[null_merge_commit]` — судьба content-null мержа `1a283fd72` (дерево == оба родителя, §2a). Опции: (A) пропустить пустой мерж; (B) воссоздать структуру мержа; (C) отложить. РЕКОМЕНДАЦИИ НЕТ.
- `luba_flag[gitattributes_resolution]` — разрешение конфликта `.gitattributes`. Факт (не рекомендация): `.gitattributes` управляет нормализацией EOL/autocrlf, а тест-база чувствительна к autocrlf (флейки по CRLF наблюдались). Опции: (A) сторона codex; (B) сторона OE; (C) ручное объединение (union); (D) отложить к M4. РЕКОМЕНДАЦИИ НЕТ.

---

## Журнал обновлений

- 2026-07-31 (UTC 2026-07-31T15:33:59Z) — файл создан циклом M2 (Track-C CONTINUE, задача M2-T1 PORT-MAP). База математики перепроверена исполнителем в момент записи: `codex/dpo-implementation` server-leg == local-leg == `7258be865693cb63317713f6615c5478d5ef8fe3` (дрейфа против M1 и ожидания оркестратора НЕТ). Вершины: `main afd26cbd292417967aa9cb90f8f56efac015fabb`, `orchestrator-extensions b293045bd89b1eb96d3e32fedd341f1a4170adbc`, `DesignToCode-clean` server-leg `469d7e85f876d19be909bd87c200b1c225c83349` / local-leg `f6172743881f19db33d18aac94dbd863e849c16d`. merge-base `6624161be8ea1836bf338d75662e19519310ae5a`. Пересчитанные числа: rev-list mb..OE = 2191; порт-лист = 41 (IDENTICAL_TO_M1); конфликтов = 33 (все content); auto-both-sides = 50; дубль-групп = 2; мерж content-null = 1; мега-коммит = 20409 файлов. Footprint в кодовых репозиториях = ноль (ДО и ПОСЛЕ). Папка планирования 19 → 21, 19 существующих файлов не тронуты.
