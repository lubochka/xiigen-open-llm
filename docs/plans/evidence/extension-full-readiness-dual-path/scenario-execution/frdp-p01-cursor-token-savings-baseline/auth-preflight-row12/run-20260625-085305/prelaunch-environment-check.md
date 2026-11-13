# Row12 Auth Preflight Step 1 - Prelaunch Environment Check

- status: PRELAUNCH_DIRTY_NEEDS_CLEANUP
- checked_at: 2026-06-25T08:53:56.7427335+03:00
- run_folder: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\auth-preflight-row12\run-20260625-085305
- process_count: 11
- visible_cursor_window_count: 1
- visible_error_window_count: 0

## Human Explanation

Prelaunch environment is dirty: one or more Cursor processes, Cursor windows, or visible Error/#32770 windows were detected. This split only reports evidence and does not close anything.

## Next Smallest Action

Review the evidence and authorize the smallest cleanup or recovery step before launching Cursor or opening browser/login.

## Actions Performed

- created_run_folder
- checked_running_cursor_processes
- checked_visible_cursor_windows
- checked_visible_top_level_error_32770_windows
- wrote_json_and_markdown_evidence

## Forbidden Actions Confirmed Not Performed

- did_not_launch_cursor
- did_not_open_browser
- did_not_login
- did_not_execute_row12_panel_prompt
- did_not_stage_commit_push_or_run_git
- did_not_edit_product_source
- did_not_close_processes_or_windows

## Cursor Processes

```json
[
    {
        "process_id":  4660,
        "process_name":  "Cursor",
        "main_window_title":  "",
        "start_time":  "2026-06-25T08:47:11.9018608+03:00",
        "path":  "C:\\Users\\lubao\\AppData\\Local\\Programs\\cursor\\Cursor.exe"
    },
    {
        "process_id":  17360,
        "process_name":  "Cursor",
        "main_window_title":  "",
        "start_time":  "2026-06-25T08:47:35.1016493+03:00",
        "path":  "C:\\Users\\lubao\\AppData\\Local\\Programs\\cursor\\Cursor.exe"
    },
    {
        "process_id":  18792,
        "process_name":  "Cursor",
        "main_window_title":  "",
        "start_time":  "2026-06-25T08:47:30.9344398+03:00",
        "path":  "C:\\Users\\lubao\\AppData\\Local\\Programs\\cursor\\Cursor.exe"
    },
    {
        "process_id":  22352,
        "process_name":  "Cursor",
        "main_window_title":  "",
        "start_time":  "2026-06-25T08:47:13.0488730+03:00",
        "path":  "C:\\Users\\lubao\\AppData\\Local\\Programs\\cursor\\Cursor.exe"
    },
    {
        "process_id":  33228,
        "process_name":  "Cursor",
        "main_window_title":  "",
        "start_time":  "2026-06-25T08:47:29.6496060+03:00",
        "path":  "C:\\Users\\lubao\\AppData\\Local\\Programs\\cursor\\Cursor.exe"
    },
    {
        "process_id":  39008,
        "process_name":  "Cursor",
        "main_window_title":  "",
        "start_time":  "2026-06-25T08:49:20.0861905+03:00",
        "path":  "C:\\Users\\lubao\\AppData\\Local\\Programs\\cursor\\Cursor.exe"
    },
    {
        "process_id":  64776,
        "process_name":  "Cursor",
        "main_window_title":  "",
        "start_time":  "2026-06-25T08:47:12.9323217+03:00",
        "path":  "C:\\Users\\lubao\\AppData\\Local\\Programs\\cursor\\Cursor.exe"
    },
    {
        "process_id":  68276,
        "process_name":  "Cursor",
        "main_window_title":  "",
        "start_time":  "2026-06-25T08:47:30.0356477+03:00",
        "path":  "C:\\Users\\lubao\\AppData\\Local\\Programs\\cursor\\Cursor.exe"
    },
    {
        "process_id":  81276,
        "process_name":  "Cursor",
        "main_window_title":  "",
        "start_time":  "2026-06-25T08:50:08.0117302+03:00",
        "path":  "C:\\Users\\lubao\\AppData\\Local\\Programs\\cursor\\Cursor.exe"
    },
    {
        "process_id":  82892,
        "process_name":  "Cursor",
        "main_window_title":  "",
        "start_time":  "2026-06-25T08:47:16.5869970+03:00",
        "path":  "C:\\Users\\lubao\\AppData\\Local\\Programs\\cursor\\Cursor.exe"
    },
    {
        "process_id":  85432,
        "process_name":  "Cursor",
        "main_window_title":  "[Extension Development Host] cursor-xiigen-smart-skills - Cursor",
        "start_time":  "2026-06-25T08:47:06.7537348+03:00",
        "path":  "C:\\Users\\lubao\\AppData\\Local\\Programs\\cursor\\Cursor.exe"
    }
]
```

## Visible Cursor Windows

```json
{
    "hwnd":  "0x34C1368",
    "process_id":  85432,
    "title":  "[Extension Development Host] cursor-xiigen-smart-skills - Cursor",
    "class_name":  "Chrome_WidgetWin_1"
}
```

## Visible Error/#32770 Windows

```json

```
