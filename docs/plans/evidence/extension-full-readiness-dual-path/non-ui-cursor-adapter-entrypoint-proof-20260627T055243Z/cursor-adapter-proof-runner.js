"use strict";

const fs = require("fs");
const path = require("path");
const Module = require("module");

const evidenceRoot = process.env.XIIGEN_CURSOR_ADAPTER_EVIDENCE_ROOT;
if (!evidenceRoot) {
  throw new Error("XIIGEN_CURSOR_ADAPTER_EVIDENCE_ROOT is required.");
}
fs.mkdirSync(evidenceRoot, { recursive: true });
process.env.PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR = evidenceRoot;

const registeredCommands = new Map();
const registeredChatParticipants = new Map();
const fakeContext = {
  subscriptions: [],
  globalStorageUri: { fsPath: path.join(evidenceRoot, "fake-global-storage") }
};

function disposable() {
  return { dispose() {} };
}

const configurationValues = new Map([
  ["coreRoot", "C:\\Projects\\llm_mvp_core"],
  ["coreApiScript", "C:\\Projects\\llm_mvp_core\\tools\\xiigen-extension-core-api.ps1"],
  ["debug", false],
  ["targetLocatorPath", ""],
  ["targetLocatorJson", ""],
  ["userAdaptiveAssetRoot", ""],
  ["rollbackAssetRoot", ""],
  ["locatorMode", "target"]
]);

const fakeVscode = {
  commands: {
    registerCommand(name, callback) {
      registeredCommands.set(name, callback);
      return disposable();
    }
  },
  window: {
    async showInputBox() {
      throw new Error("showInputBox should not be used in non-UI adapter proof.");
    },
    async showInformationMessage() {
      return undefined;
    },
    createWebviewPanel() {
      return { webview: { html: "" }, dispose() {} };
    }
  },
  workspace: {
    getConfiguration(section) {
      if (section !== "xiigenSmartSkills") {
        throw new Error(`Unexpected configuration section: ${section}`);
      }
      return {
        get(name, defaultValue) {
          return configurationValues.has(name) ? configurationValues.get(name) : defaultValue;
        },
        inspect() {
          return {};
        }
      };
    }
  },
  chat: {
    createChatParticipant(id, handler) {
      registeredChatParticipants.set(id, handler);
      return disposable();
    }
  },
  ViewColumn: { Active: 1 }
};

const originalLoad = Module._load;
Module._load = function patchedLoad(request, parent, isMain) {
  if (request === "vscode") {
    return fakeVscode;
  }
  return originalLoad.call(this, request, parent, isMain);
};

function writeJson(fileName, value) {
  fs.writeFileSync(path.join(evidenceRoot, fileName), `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function readJsonIfExists(filePath) {
  if (!filePath || !fs.existsSync(filePath)) {
    return null;
  }
  return JSON.parse(fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/, ""));
}

(async () => {
  const extensionPath = "C:\\Projects\\cursor-xiigen-smart-skills\\dist\\extension.js";
  const extension = require(extensionPath);
  if (typeof extension.activate !== "function") {
    throw new Error("Cursor extension dist does not export activate().");
  }

  extension.activate(fakeContext);

  const commandNames = [...registeredCommands.keys()].sort();
  const chatNames = [...registeredChatParticipants.keys()].sort();
  const command = registeredCommands.get("xiigen.smartSkills.select");
  if (!command) {
    writeJson("proof-output.json", {
      passed: false,
      error: "xiigen.smartSkills.select command was not registered",
      commandNames,
      chatNames,
      productReadinessClaimed: false
    });
    process.exitCode = 2;
    return;
  }

  const prompt = "<redacted-cursor-adapter-proof-prompt:sha256:cursor-adapter-boundary-20260627>";
  const result = await command({
    prompt,
    method: "skill-selection",
    requestedSkills: ["xiigen-smart-skills"],
    availableSkills: ["xiigen-smart-skills", "qa-automation", "planning"]
  });

  const coreRequest = readJsonIfExists(result && result.coreRequestRef);
  const coreResponse = readJsonIfExists(result && result.coreResponseRef);
  const hostTrace = readJsonIfExists(result && result.hostTraceRef);
  const entrypointResponse = readJsonIfExists(result && result.entrypointResponseRef);

  const proof = {
    schemaVersion: 1,
    proofKind: "non-ui-cursor-adapter-entrypoint-to-core-boundary",
    passed: Boolean(result && result.passed === true && result.adapterFunctionalityPassed === true),
    verdictCandidate: Boolean(result && result.passed === true && result.adapterFunctionalityPassed === true)
      ? "PASS_CURSOR_ADAPTER_TO_CORE_BOUNDARY_ONLY"
      : "PRECHECK_INCONCLUSIVE",
    productReadinessClaimed: false,
    fullCursorUiEvidenceClaimed: false,
    tokenSavingsClaimed: false,
    semanticQualityClaimed: false,
    learnabilityClaimed: false,
    trainingStarted: false,
    checkpointMutationStarted: false,
    externalAiUsed: false,
    extensionPath,
    commandNames,
    chatParticipantNames: chatNames,
    invokedCommand: "xiigen.smartSkills.select",
    requestedSkillsPassedThrough: coreRequest && Array.isArray(coreRequest.RequestedSkills)
      ? coreRequest.RequestedSkills
      : [],
    availableSkillsPassedThrough: coreRequest && Array.isArray(coreRequest.AvailableSkills)
      ? coreRequest.AvailableSkills
      : [],
    hostFacts: coreRequest ? coreRequest.HostFacts : null,
    resultSummary: result ? {
      passed: result.passed,
      adapterFunctionalityPassed: result.adapterFunctionalityPassed,
      evidenceScope: result.evidenceScope,
      coreRequestRef: result.coreRequestRef,
      coreResponseRef: result.coreResponseRef,
      coreTraceRef: result.coreTraceRef,
      hostTraceRef: result.hostTraceRef,
      entrypointResponseRef: result.entrypointResponseRef,
      runtimeEvidence: result.runtimeEvidence,
      trueHostRuntimeEvidence: result.trueHostRuntimeEvidence,
      fullCursorExtensionHostUiProof: result.fullCursorExtensionHostUiProof,
      semanticQualityClaimed: result.semanticQualityClaimed,
      externalAiOrUnrelatedToolUsed: result.externalAiOrUnrelatedToolUsed
    } : null,
    coreResponseSummary: coreResponse ? {
      passed: coreResponse.passed,
      success: coreResponse.Success,
      status: coreResponse.Status,
      evidenceScope: coreResponse.evidenceScope,
      runtimeEvidence: coreResponse.runtimeEvidence,
      semanticQualityClaimed: coreResponse.semanticQualityClaimed || coreResponse.SemanticQualityClaimed || false,
      externalAiOrUnrelatedToolUsed: coreResponse.externalAiOrUnrelatedToolUsed || false
    } : null,
    hostTraceSummary: hostTrace ? {
      adapterFunctionalityPassed: hostTrace.adapterFunctionalityPassed,
      requestedSkillsCount: hostTrace.skillBinding && hostTrace.skillBinding.requestedSkillsCount,
      availableSkillsCount: hostTrace.skillBinding && hostTrace.skillBinding.availableSkillsCount,
      hostSelectorOrScorerAdded: hostTrace.skillBinding && hostTrace.skillBinding.hostSelectorOrScorerAdded,
      noOverclaimReview: hostTrace.noOverclaimReview
    } : null,
    entrypointResponseSummary: entrypointResponse ? {
      passed: entrypointResponse.passed,
      adapterFunctionalityPassed: entrypointResponse.adapterFunctionalityPassed,
      evidenceScope: entrypointResponse.evidenceScope,
      fullCursorExtensionHostUiProof: entrypointResponse.fullCursorExtensionHostUiProof,
      semanticQualityClaimed: entrypointResponse.semanticQualityClaimed
    } : null
  };

  writeJson("proof-output.json", proof);
  process.stdout.write(JSON.stringify({
    passed: proof.passed,
    verdictCandidate: proof.verdictCandidate,
    evidenceRoot,
    commandNames,
    chatNames
  }));
})().catch((error) => {
  writeJson("proof-output.json", {
    schemaVersion: 1,
    proofKind: "non-ui-cursor-adapter-entrypoint-to-core-boundary",
    passed: false,
    verdictCandidate: "PRECHECK_INCONCLUSIVE",
    error: error && error.stack ? error.stack : String(error),
    productReadinessClaimed: false
  });
  console.error(error && error.stack ? error.stack : String(error));
  process.exit(1);
});
