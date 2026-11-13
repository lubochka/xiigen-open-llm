# Pass P36 — Path B локальный десктоп: реальность (агрегат по всем хостам)

Дата: 2026-07-08. Ассессор: Opus 4.8 (read-only). Слайс: есть ли ХОТЬ КАКОЕ-ТО
реальное local-desktop host-доказательство Path B (скриншоты / видео / логи +
маркеры), или Path B по факту отсутствует. Смотрю АГРЕГАТ, а не по-сценарно
(по-сценарные механизмы Claude/Cursor/Codex уже разобраны в P32/P33/P34).

## Что проверено (bounded reads живого репо)

Корень доказательств: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\`.

1. Инвентарь медиа по всему дереву доказательств:
   - `find -iname *.png` = **288** PNG; `*.jpg/*.jpeg` = **0**;
     `*.mp4/*.webm/*.gif/*.mkv/*.avi` = **0** (ноль видео).
2. Каноническая папка автоматического Path B локального десктопа
   `phase-04-local-desktop-evidence/` — прочитана целиком по файлам:
   - `00-discovery-summary.txt`, `08-marker-smoke-policy.json`,
     `cursor/16-...rejected-private-surface.txt`, `cursor/18-...redaction-note.txt`,
     `markers/{claude,codex,cursor}-*.txt`, дерево `find -type f`.
   - PNG внутри этой папки: claude=0, codex=0, cursor=**1**
     (`cursor/18-cursor-marker-editor-crop-redaction-not-required.png`).
3. Схема контракта доказательств `phase-02-dual-path-evidence-schema.json`
   (`:561-562`, `:575`) — поля `visible_marker` и `screenshot_video_log_evidence`
   объявлены **required**.
4. Образец local-desktop-пакета `phase-02-sample-local-desktop-packet.json`
   (`local_desktop.status:"NOT_RUN"`, `evidence[0].status:"NOT_CAPTURED"`).
5. Распределение 288 PNG по папкам (`find | uniq -c`): концентрация в ad-hoc
   `scenario-execution/*` (106 в одном `frdp-p01-cursor-token-savings-baseline`,
   остальное — `cursor-path-b-*`/`codex-path-b-*`/`claude-path-b-*` прогоны,
   уже даунгрейднутые как stale/re-photo в P32-P34). В коммитнутом
   local-desktop deliverable — 1 PNG.

## Ключевые факты

- **Видео = 0.** Спецификация Dual-Path (CLAUDE.md: «screenshot/video/log
  evidence») и сама схема требуют `screenshot_video_log_evidence`, но во всём
  дереве нет ни одного видеофайла ни для одного хоста. Видео-нога Path B никогда
  не производилась.
- **Канонический автоматический local-desktop-слайс = только discovery.**
  `00-discovery-summary.txt`: `safe_marker_policy = "No screenshot or host prompt
  marker smoke was run unless the surface could be proven marker-only and
  private-data-free; none met that threshold in this automated slice."`
  Метрики этого слайса — `found_command_count:4`, `known_path_found_count:2`,
  `installed_app_match_count:1`, `appx_package_match_count:2`,
  `shortcut_match_count:4`, `process_match_count:17` — то есть найдены только
  локальные пути/процессы/ярлыки, НИ ОДНОГО захвата host-UI.
  `08-marker-smoke-policy.json`: для ВСЕХ ТРЁХ хостов (Claude/Cursor/Codex)
  `safe_marker_smoke_status: "NOT_RUN"` с причиной «GUI может раскрыть
  аккаунт/историю/воркспейс/внешний сервис».
- **Единственный принятый канонический local-desktop скриншот = кроп РЕДАКТОРА
  Cursor, не UI расширения.** `cursor/18-...redaction-note.txt`:
  `accepted_status = PASS_LOCAL_DESKTOP_MARKER_ONLY`; `inspection_result` —
  «marker text visible in Cursor editor crop»; `NOT THE TASK: no Cursor chat,
  no prompt/message send». То есть доказано лишь, что `Cursor.exe` установлен и
  показывает набранный текст в текстовом редакторе — НЕ что расширение XIIGen
  запускается/отвечает. Для Claude и Codex принятых канонических local-desktop
  скриншотов нет вовсе (0 PNG).
- **Инструмент захвата снял СОБСТВЕННЫЙ Codex-разговор ассессора вместо
  целевого хоста.** `cursor/16-...rejected-private-surface.txt`:
  `rejection_reason = "screen capture returned the current Codex conversation
  surface instead of a marker-only Cursor editor surface"`,
  `redaction_status: REJECTED_AND_DELETED_PRIVATE_SURFACE`,
  `accepted_screenshot_evidence: NONE`. Маркер Codex явно предупреждает:
  «do not use current Codex conversation as proof». Наведение на окно/фолбэк по
  мониторам (требование CLAUDE.md о monitor/window-overlap fallback) не
  управляется — Path B-скриншоты рискуют фотографировать чат самого агента.
- **Статические prep-маркеры не подтверждены на реальной поверхности хоста
  (Claude/Codex).** `markers/{claude,codex}-frdp-p04-manual-route-marker.txt` —
  просто текстовые файлы «manual-route prep marker source only … Screenshot:
  only after a marker-only surface is confirmed». Подтверждённого скриншота
  поверхности для Claude/Codex нет; маркер подготовлен, но никогда не показан и
  не снят на установленном хосте для 2 из 3 хостов.

## Вывод P36

Реальность Path B локального десктопа как ДОСТАВЛЕННОГО артефакта: видео — ноль;
каноническая автоматическая local-desktop-папка — только discovery
(path/process), скриншоты и marker-smoke `NOT_RUN` для всех трёх хостов;
единственный принятый канонический скриншот — текстовый кроп редактора Cursor
(доказывает установку приложения, не расширение); Claude/Codex — 0 принятых
канонических local-desktop скриншотов; инструмент захвата однажды снял
собственный Codex-чат ассессора (провенанс окна ненадёжен). Все 288 PNG живут в
ad-hoc `scenario-execution/*` прогонах, уже даунгрейднутых P32-P34 как
stale/re-photo/dev-host; в коммитнутый local-desktop deliverable попал 1 PNG.
Path B как продуктовое доказательство (установленный хост + видимый маркер +
скриншот/видео/лог) для Claude и Codex фактически отсутствует, для Cursor —
только marker-only кроп редактора.

Дедуп: НЕ повторяю P06 (package pixel/бумажные joins), P08 (host-репо phase40
stdout-smoke маркеры), P16 (Cursor QA headless), P31 (board-счётчики 0/28,
A↔B-поле), P32 (Claude нативный ответ/broken composer/re-photo — по-сценарно),
P33 (Cursor dev-host/stale/no-oracle — по-сценарно), P34 (Codex overclaim-reverse/
circular re-photo — по-сценарно). Здесь — АГРЕГАТ по каноническому
local-desktop deliverable + факт нулевого видео + факт capture-of-own-chat.
