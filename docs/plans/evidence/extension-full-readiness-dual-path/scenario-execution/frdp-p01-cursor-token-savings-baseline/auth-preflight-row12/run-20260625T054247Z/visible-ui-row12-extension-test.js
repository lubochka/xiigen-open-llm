"use strict";

const fs = require("fs");
const path = require("path");

const runRoot = __dirname;

function writeJson(filePath, value) {
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function run() {
  writeJson(path.join(runRoot, "extension-host-evidence", "auth-preflight-noop-extension-test-started.json"), {
    schema_version: "frdp-p01-row12-auth-preflight-noop-extension-test/v1",
    generated_at_utc: new Date().toISOString(),
    marker: "FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1",
    scenario_id: "frdp-p01-cursor-token-savings-baseline",
    purpose: "Keep the visible Cursor extension host open for auth preflight only.",
    opens_xiigen_panel: false,
    types_or_submits_row12_prompt: false,
    external_ai_send: false,
    no_secrets_captured: true
  });

  await sleep(10 * 60 * 1000);
}

module.exports = { run };
