# Claude Path B: mapping composer input target

Ð¤Ð¸Ð½Ð°Ð»ÑŒÐ½Ñ‹Ð¹ ÑÑ‚Ð°Ñ‚ÑƒÑ: $Status.

Ð­Ñ‚Ð¾Ñ‚ Ð¿Ð°ÐºÐµÑ‚ Ð·Ð°ÐºÑ€Ñ‹Ð²Ð°ÐµÑ‚ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ ÑƒÐ·ÐºÐ¸Ð¹ discovery-ÑˆÐ°Ð³: Ð½Ð°Ð¹Ñ‚Ð¸ Ð±ÐµÐ·Ð¾Ð¿Ð°ÑÐ½Ñ‹Ð¹ target Ð´Ð»Ñ Ð±ÑƒÐ´ÑƒÑ‰ÐµÐ³Ð¾ Ð²Ð²Ð¾Ð´Ð° Ð² composer Ð»Ð¾ÐºÐ°Ð»ÑŒÐ½Ð¾Ð³Ð¾ Claude UI Ð¿Ð¾ÑÐ»Ðµ Ð²Ñ‹Ð±Ð¾Ñ€Ð° XIIGen/Smart Skills route. Prompt Ð½Ðµ Ð²Ð²Ð¾Ð´Ð¸Ð»ÑÑ, Ð½Ðµ Ð²ÑÑ‚Ð°Ð²Ð»ÑÐ»ÑÑ Ð¸ Ð½Ðµ Ð¾Ñ‚Ð¿Ñ€Ð°Ð²Ð»ÑÐ»ÑÑ.

Ð§Ñ‚Ð¾ Ð²Ñ‹Ð¿Ð¾Ð»Ð½ÐµÐ½Ð¾:

- Ð²Ñ‹Ð±Ñ€Ð°Ð½ Ñ€ÐµÐ°Ð»ÑŒÐ½Ñ‹Ð¹ Ð»Ð¾ÐºÐ°Ð»ÑŒÐ½Ñ‹Ð¹ Claude window Ñ‡ÐµÑ€ÐµÐ· top-level EnumWindows;
- Ð¸ÑÐ¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ð½ bounded helper pattern: selected Claude HWND, EnumChildWindows Ñ‚Ð¾Ð»ÑŒÐºÐ¾ Ð¾Ñ‚ Ð²Ñ‹Ð±Ñ€Ð°Ð½Ð½Ð¾Ð³Ð¾ Ð¾ÐºÐ½Ð°, handle-rooted TreeWalker.RawViewWalker;
- whole-tree RootElement.FindAll Ð½Ðµ Ð¸ÑÐ¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ð»ÑÑ;
- XIIGen/Smart Skills route Ð±Ñ‹Ð» Ð½Ð°Ð¹Ð´ÐµÐ½ Ð² Ð¿Ð°Ð¼ÑÑ‚Ð¸ Ð¿Ð¾ Ð±ÐµÐ·Ð¾Ð¿Ð°ÑÐ½Ð¾Ð¼Ñƒ route-term Ð¸ Ð²Ñ‹Ð±Ñ€Ð°Ð½/ÑÑ„Ð¾ÐºÑƒÑÐ¸Ñ€Ð¾Ð²Ð°Ð½ Ð±ÐµÐ· prompt input;
- Ð¿Ð¾ÑÐ»Ðµ route selection ÑÐ¾Ð±Ñ€Ð°Ð½Ñ‹ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ redacted candidate attributes: control type, bounds, focusable/enabled flags, available patterns, relative position, category and score;
- private UI text, raw prompt, screenshots, clipboard text and message content Ð½Ðµ ÑÐ¾Ñ…Ñ€Ð°Ð½ÑÐ»Ð¸ÑÑŒ.

Ð˜Ñ‚Ð¾Ð³ mapping:

- route_selected: $RouteSelected;
- candidate_count: $CandidateCount;
- Ð‘ÐµÐ·Ð¾Ð¿Ð°ÑÐ½Ñ‹Ð¹ composer target Ð½Ðµ Ð²Ñ‹Ð±Ñ€Ð°Ð½.

Ð‘Ð»Ð¾ÐºÐµÑ€: No visible XIIGen/Smart Skills route candidate was found in the bounded selected-HWND UIA tree.

Ð¡Ð»ÐµÐ´ÑƒÑŽÑ‰Ð¸Ð¹ ÑˆÐ°Ð³: Bring Claude to the XIIGen/Smart Skills route manually or repair route locator before composer mapping.

Ð§Ñ‚Ð¾ ÑÑ‚Ð¸Ð¼ Ð¿Ð°ÐºÐµÑ‚Ð¾Ð¼ Ð½Ðµ Ð·Ð°ÑÐ²Ð»ÑÐµÑ‚ÑÑ:

- prompt Ð±Ñ‹Ð» typed/pasted/sent;
- Claude response;
- core/adapter correlation after submission;
- token savings;
- semantic quality;
- learnability/training;
- checkpoint promotion;
- all-host readiness or product readiness.
