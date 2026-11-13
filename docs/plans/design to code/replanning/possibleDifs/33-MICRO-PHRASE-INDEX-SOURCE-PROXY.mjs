#!/usr/bin/env node
'use strict';
// Source-logic proxy: deliberately NOT a native .NET or model-learning run.
function runN33Proxy() {
  const normalizeMapKey = (value) => (value ?? "").trim().toLowerCase()
    .replace(/[^\p{L}\p{N}_$\s]/gu, "")
    .split(/\s+/u).filter(Boolean).join(" ");
  const phraseMap = new Map([["проверь", "check"]]);
  const wordMap = new Map([["проверь", "check"], ["файл", "file"]]);
  const cases = [
    { id: "no_punctuation_control", originalTokens: ["проверь", "файл"], gold: "check file" },
    { id: "leading_symbol", originalTokens: ["—", "проверь", "файл"], gold: "check file" },
    { id: "interposed_symbol", originalTokens: ["проверь", "—", "файл"], gold: "check file" },
    { id: "trailing_symbol_control", originalTokens: ["проверь", "файл", "—"], gold: "check file" },
  ];
  function compose(originalTokens, preserveOriginalIndex) {
    const filtered = originalTokens.map((word, originalIndex) => ({
      word: normalizeMapKey(word), originalIndex
    })).filter(entry => entry.word.length > 0);
    const parts = []; let matchedTokens = 0; let matchedPhraseSpans = 0;
    for (let i = 0; i < filtered.length; i++) {
      const phrase = phraseMap.get(filtered[i].word);
      if (phrase) { parts.push(phrase); matchedTokens++; matchedPhraseSpans++; continue; }
      const indexedWord = originalTokens[preserveOriginalIndex ? filtered[i].originalIndex : i];
      const mapped = wordMap.get(normalizeMapKey(indexedWord));
      if (mapped) { parts.push(mapped); matchedTokens++; }
    }
    const coverage = filtered.length ? matchedTokens / filtered.length : 0;
    const accepted = matchedPhraseSpans > 0 && coverage >= 0.85 && parts.length > 0;
    return {
      composedText: parts.join(" "),
      phraseAccepted: accepted,
      coverage,
      originalIndexesOfFilteredTokens: filtered.map(entry => entry.originalIndex)
    };
  }
  const rows = cases.map(item => ({
    caseId: item.id,
    source: item.originalTokens.join(" "),
    expectedWhenDashIsNonSemantic: item.gold,
    baseline: compose(item.originalTokens, false),
    indexPreserved: compose(item.originalTokens, true)
  }));
  const score = key => ({
    accepted: rows.filter(r => r[key].phraseAccepted).length,
    correctAccepted: rows.filter(r => r[key].phraseAccepted && r[key].composedText === r.expectedWhenDashIsNonSemantic).length,
    wrongAccepted: rows.filter(r => r[key].phraseAccepted && r[key].composedText !== r.expectedWhenDashIsNonSemantic).length,
  });
  return {
    study: "N33 filtered/original token index drift",
    status: "SOURCE_LOGIC_PROXY_ONLY__NOT_NATIVE_DOTNET__NOT_TRAINED_MODEL__NOT_SEMANTIC_QUALITY_RESULT",
    sourceCode: "27.TranslationDomain/TranslationTrainedUnits.cs L798-L859, L1008-L1023",
    setup: "Simulated admitted RU_EN PhraseMap and WordMap with no exact-source memory",
    rows,
    score: { baseline: score("baseline"), indexPreserved: score("indexPreserved") },
    qualification: "The gold assumes the standalone em dash is punctuation. This proxy does not implement native tokenizer, verifier, admission, checkpoint or model."
  };
}
process.stdout.write(JSON.stringify(runN33Proxy(), null, 2) + '\n');
