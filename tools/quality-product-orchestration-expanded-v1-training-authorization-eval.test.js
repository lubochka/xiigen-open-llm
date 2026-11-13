"use strict";

const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const { spawnSync } = require("node:child_process");
const test = require("node:test");

const repoRoot = path.resolve(__dirname, "..");
const runner = path.join(
  repoRoot,
  "tools",
  "quality-product-orchestration-expanded-v1-training-authorization-eval.js"
);
const outputRoot = path.join(
  repoRoot,
  "artifacts",
  "extension-runtime-token-quality-learnability",
  "quality-product-orchestration-expanded-v1-training-authorization-eval"
);

function snapshot(root) {
  if (!fs.existsSync(root)) {
    return [];
  }
  return fs.readdirSync(root).sort();
}

test("expanded-v1 runner rejects before filesystem writes or dotnet spawn", () => {
  const before = snapshot(outputRoot);
  const result = spawnSync(process.execPath, [runner], {
    cwd: repoRoot,
    encoding: "utf8"
  });
  const after = snapshot(outputRoot);

  assert.equal(result.status, 3);
  assert.equal(result.stdout, "");
  const failure = JSON.parse(result.stderr.trim());
  assert.equal(
    failure.error_code,
    "ORCHESTRATION_REASONING_SOURCE_ADMISSION_REJECTED"
  );
  assert.equal(
    failure.error_message,
    "Expanded-v1 retained material is pending evidence-bound R17 source admission; training remains blocked."
  );
  assert.equal(failure.filesystem_writes, 0);
  assert.equal(failure.dotnet_spawned, false);
  assert.equal(failure.training_started, false);
  assert.deepEqual(after, before);
});
