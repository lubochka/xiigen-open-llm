// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// SPDX-License-Identifier: AGPL-3.0-only
//
// SMALL SHADOW PROXY (not native C#, not BPE320, not KNN training).
// Reproduces only BpeTokenizer CountPairs + first MaxBy selection over two
// pre-tokenized ASCII word chunks (RegexSplitter would split those identically).
// Run: node 21-MICRO-BPE-FIRST-MERGE-PROXY.mjs
function firstMerge(chunks) {
  const counts = new Map();
  for (const chunk of chunks) {
    const ids = Array.from(Buffer.from(chunk, 'utf8'));
    for (let i = 0; i < ids.length - 1; i++) {
      const key = String(ids[i]) + ',' + String(ids[i + 1]);
      counts.set(key, (counts.get(key) || 0) + 1);
    }
  }
  let pair, max = -1;
  for (const [key, count] of counts) {
    if (count > max) {
      pair = key;
      max = count;
    }
  }
  return { tokenId: 256, chosenBytePair: pair.split(',').map(Number), frequency: max };
}
const abFirst = firstMerge(['abab', 'cdcd']);
const cdFirst = firstMerge(['cdcd', 'abab']);
if (abFirst.chosenBytePair.join(',') === cdFirst.chosenBytePair.join(',')) {
  throw new Error('Tie contrast unexpectedly identical');
}
if (abFirst.frequency !== 2 || cdFirst.frequency !== 2) {
  throw new Error('Frequency precondition violated');
}
console.log(JSON.stringify({
  status: 'PASS_PROXY_ONLY',
  abFirst,
  cdFirst,
  scope: 'one CountPairs+MaxBy merge, pre-tokenized ASCII; no C# native, no BPE320, no retrieval or answer quality'
}, null, 2));
