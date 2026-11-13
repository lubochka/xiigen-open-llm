import { createHash } from 'node:crypto';
import { createReadStream, createWriteStream } from 'node:fs';
import { access, mkdir, rm, stat, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { once } from 'node:events';
import readline from 'node:readline';

const DEFAULT_SOURCE_PATH = 'C:\\Users\\lubao\\Documents\\xiigen\\sonnet local - analyzes\\xiigen-file-analysis-state.json';
const DEFAULT_OUT_DIR = 'artifacts/smart-skills-quality-uplift/intake';
const PROCESSED_VERSION = 'q1-01-source-ledger-v1';
const AUDIT_METADATA_NOTE = 'path and file_name are audit metadata for traceability only; sha256 is a source-row metadata/version hash, not referenced file content.';
const CONTENT_VERSION_NOTE = 'source_version_id is based on stable source-row content fields and excludes row state plus volatile mtime fields, so todo->done and mtime-only changes do not create a new content version.';
const VOLATILE_OR_STATE_FIELDS = new Set(['state', 'mtime', 'modified', 'last_modified', 'lastModified', 'last_write_time', 'lastWriteTime', 'source_mtime', 'sourceMtime']);

const args = parseArgs(process.argv.slice(2));
const SOURCE_PATH = args.sourcePath;
const OUT_DIR = path.resolve(args.outDir);
const LEDGER_PATH = path.join(OUT_DIR, 'source-ledger.jsonl');
const SCHEMA_PATH = path.join(OUT_DIR, 'source-ledger-schema.json');
const SUMMARY_PATH = path.join(OUT_DIR, 'source-ledger-summary.json');
const REPORT_PATH = path.join(OUT_DIR, 'source-ledger-build-report.json');
const PREVIOUS_LEDGER_PATH = args.previousLedgerPath === null ? null : path.resolve(args.previousLedgerPath);
const startedAt = new Date();

function parseArgs(argv) {
  const options = {
    sourcePath: DEFAULT_SOURCE_PATH,
    outDir: DEFAULT_OUT_DIR,
    previousLedgerPath: null
  };
  const positional = [];

  for (let index = 0; index < argv.length; index += 1) {
    const arg = argv[index];

    if (arg === '--source') {
      options.sourcePath = argv[index + 1];
      index += 1;
      continue;
    }

    if (arg === '--out-dir') {
      options.outDir = argv[index + 1];
      index += 1;
      continue;
    }

    if (arg === '--previous-ledger') {
      options.previousLedgerPath = argv[index + 1];
      index += 1;
      continue;
    }

    positional.push(arg);
  }

  if (positional.length > 0) {
    options.sourcePath = positional[0];
  }

  return options;
}

function sha256Text(value) {
  return createHash('sha256').update(value).digest('hex');
}

function stableJson(value) {
  if (Array.isArray(value)) {
    return `[${value.map((item) => stableJson(item)).join(',')}]`;
  }

  if (value !== null && typeof value === 'object') {
    return `{${Object.keys(value).sort().map((key) => `${JSON.stringify(key)}:${stableJson(value[key])}`).join(',')}}`;
  }

  return JSON.stringify(value);
}

function normalizeState(value) {
  return typeof value === 'string' && value.trim().length > 0 ? value.trim().toLowerCase() : 'unknown';
}

function processingDecision(state) {
  if (state === 'done') {
    return 'process_candidate_done';
  }

  if (state === 'todo') {
    return 'backlog_not_processed';
  }

  return `evidence_only_state_${state}`;
}

function contentProcessingDecision(state, previousRow) {
  if (state !== 'done') {
    return processingDecision(state);
  }

  if (previousRow?.processing_decision === 'process_candidate_done' || previousRow?.processing_decision === 'already_processed_done') {
    return 'already_processed_done';
  }

  return 'process_candidate_done';
}

async function writeJson(filePath, value) {
  await writeFile(filePath, `${JSON.stringify(value, null, 2)}\n`, 'utf8');
}

async function writeJsonl(stream, row) {
  if (!stream.write(`${JSON.stringify(row)}\n`)) {
    await once(stream, 'drain');
  }
}

function buildSchema() {
  return {
    $schema: 'https://json-schema.org/draft/2020-12/schema',
    title: 'Smart skills Q1 source ledger row',
    type: 'object',
    additionalProperties: false,
    audit_metadata_note: AUDIT_METADATA_NOTE,
    content_version_note: CONTENT_VERSION_NOTE,
    required: [
      'source_row_id',
      'source_version_id',
      'path',
      'file_name',
      'first_seen_state',
      'last_seen_state',
      'sha256',
      'mtime',
      'processed_version',
      'processed_at',
      'processing_decision'
    ],
    properties: {
      source_row_id: { type: 'string', minLength: 1 },
      source_version_id: { type: 'string', minLength: 1 },
      path: { type: 'string', description: 'Audit metadata only; not model-learning content.' },
      file_name: { type: 'string', description: 'Audit metadata only; not model-learning content.' },
      first_seen_state: { type: 'string', minLength: 1 },
      last_seen_state: { type: 'string', minLength: 1 },
      sha256: { type: 'string', pattern: '^[a-f0-9]{64}$', description: 'Stable source-row content hash; excludes state and volatile mtime fields.' },
      mtime: { type: 'string', minLength: 1, description: 'Source state file mtime, recorded as audit metadata only.' },
      processed_version: { type: 'string', minLength: 1 },
      processed_at: { type: 'string', format: 'date-time' },
      processing_decision: { type: 'string', minLength: 1 }
    }
  };
}

async function removePartialOutputs() {
  for (const outputPath of [LEDGER_PATH, SCHEMA_PATH, SUMMARY_PATH, REPORT_PATH]) {
    await rm(outputPath, { force: true });
  }
}

async function fileExists(filePath) {
  try {
    await access(filePath);
    return true;
  } catch {
    return false;
  }
}

async function readPreviousLedger(previousLedgerPath) {
  if (previousLedgerPath === null || !(await fileExists(previousLedgerPath))) {
    return {
      rowsByVersionId: new Map(),
      versionsBySourceRowId: new Map(),
      rowCount: 0
    };
  }

  const rowsByVersionId = new Map();
  const versionsBySourceRowId = new Map();
  const previousStream = createReadStream(previousLedgerPath, { encoding: 'utf8' });
  const previousLines = readline.createInterface({
    input: previousStream,
    crlfDelay: Infinity
  });
  let rowCount = 0;

  for await (const line of previousLines) {
    const trimmed = line.trim();

    if (trimmed === '') {
      continue;
    }

    const row = JSON.parse(trimmed);
    rowsByVersionId.set(row.source_version_id, row);

    if (!versionsBySourceRowId.has(row.source_row_id)) {
      versionsBySourceRowId.set(row.source_row_id, new Set());
    }

    versionsBySourceRowId.get(row.source_row_id).add(row.source_version_id);
    rowCount += 1;
  }

  previousLines.close();

  return {
    rowsByVersionId,
    versionsBySourceRowId,
    rowCount
  };
}

function buildContentMetadata(source, sourcePath, fileName) {
  const metadata = {
    path: sourcePath,
    file_name: fileName
  };

  if (source !== null && typeof source === 'object' && !Array.isArray(source)) {
    for (const key of Object.keys(source).sort()) {
      if (key === 'path' || key === 'file_name' || VOLATILE_OR_STATE_FIELDS.has(key)) {
        continue;
      }

      metadata[key] = source[key];
    }
  }

  return metadata;
}

function buildRow(source, sourceStateIdentity, processedAt, previousRow) {
  const sourcePath = typeof source?.path === 'string' ? source.path : '';
  const fileName = typeof source?.file_name === 'string' ? source.file_name : path.basename(sourcePath);
  const state = normalizeState(source?.state);
  const rowMetadata = { path: sourcePath, file_name: fileName };
  const sourceRowId = `source-row-${sha256Text(stableJson(rowMetadata)).slice(0, 32)}`;
  const rowVersionMetadata = buildContentMetadata(source, sourcePath, fileName);
  const rowHash = sha256Text(stableJson(rowVersionMetadata));
  const sourceVersionId = `source-version-${sha256Text(stableJson({
    source_row_id: sourceRowId,
    source_row_sha256: rowHash,
    processed_version: PROCESSED_VERSION
  })).slice(0, 32)}`;

  return {
    source_row_id: sourceRowId,
    source_version_id: sourceVersionId,
    path: sourcePath,
    file_name: fileName,
    first_seen_state: previousRow?.first_seen_state ?? state,
    last_seen_state: state,
    sha256: rowHash,
    mtime: sourceStateIdentity.mtime,
    processed_version: PROCESSED_VERSION,
    processed_at: processedAt,
    processing_decision: contentProcessingDecision(state, previousRow)
  };
}

function recordDuplicateProbe(groupMap, id, sourceFingerprint, row, physicalLine) {
  if (!groupMap.has(id)) {
    groupMap.set(id, {
      id,
      count: 0,
      fingerprints: new Set(),
      versionIds: new Set(),
      examples: []
    });
  }

  const group = groupMap.get(id);
  group.count += 1;
  group.fingerprints.add(sourceFingerprint);
  group.versionIds.add(row.source_version_id);

  if (group.examples.length < 2) {
    group.examples.push({
      physical_line: physicalLine,
      source_row_id: row.source_row_id,
      source_version_id: row.source_version_id,
      path: row.path,
      file_name: row.file_name,
      first_seen_state: row.first_seen_state,
      last_seen_state: row.last_seen_state,
      sha256: row.sha256,
      processing_decision: row.processing_decision
    });
  }
}

function summarizeDuplicateGroups(groupMap) {
  const duplicateGroups = [...groupMap.values()].filter((group) => group.count > 1);
  const groups = duplicateGroups.map((group) => {
    let classification = 'exact_duplicate_rows';

    if (group.fingerprints.size > 1 && group.versionIds.size === 1) {
      classification = 'id_collision_or_non_exact_duplicate';
    } else if (group.versionIds.size > 1) {
      classification = 'same_source_row_multiple_versions';
    }

    return {
      id: group.id,
      row_count: group.count,
      extra_duplicate_rows: group.count - 1,
      unique_source_fingerprint_count: group.fingerprints.size,
      unique_source_version_id_count: group.versionIds.size,
      classification,
      examples: group.examples
    };
  });
  const collisionLikeGroups = groups.filter((group) => group.classification === 'id_collision_or_non_exact_duplicate');

  return {
    duplicate_group_count: groups.length,
    duplicate_extra_row_count: groups.reduce((sum, group) => sum + group.extra_duplicate_rows, 0),
    duplicate_rows_in_groups: groups.reduce((sum, group) => sum + group.row_count, 0),
    exact_duplicate_group_count: groups.filter((group) => group.classification === 'exact_duplicate_rows').length,
    collision_like_group_count: collisionLikeGroups.length,
    groups
  };
}

async function* streamTopLevelArrayObjects(filePath) {
  const stream = createReadStream(filePath, { encoding: 'utf8' });
  let objectText = '';
  let depth = 0;
  let inString = false;
  let escaped = false;
  let arrayOpenSeen = false;
  let arrayCloseSeen = false;
  let physicalLineCount = 1;
  let objectStartLine = 1;

  for await (const chunk of stream) {
    for (let index = 0; index < chunk.length; index += 1) {
      const char = chunk[index];

      if (char === '\n') {
        physicalLineCount += 1;
      }

      if (!arrayOpenSeen) {
        if (char.charCodeAt(0) === 0xfeff || /\s/.test(char)) {
          continue;
        }

        if (char !== '[') {
          throw new Error(`Expected top-level JSON array at physical line ${physicalLineCount}`);
        }

        arrayOpenSeen = true;
        continue;
      }

      if (depth === 0) {
        if (/\s/.test(char) || char === ',') {
          continue;
        }

        if (char === ']') {
          arrayCloseSeen = true;
          continue;
        }

        if (char !== '{') {
          throw new Error(`Expected object start at physical line ${physicalLineCount}`);
        }

        objectText = '{';
        depth = 1;
        inString = false;
        escaped = false;
        objectStartLine = physicalLineCount;
        continue;
      }

      objectText += char;

      if (escaped) {
        escaped = false;
        continue;
      }

      if (char === '\\' && inString) {
        escaped = true;
        continue;
      }

      if (char === '"') {
        inString = !inString;
        continue;
      }

      if (inString) {
        continue;
      }

      if (char === '{') {
        depth += 1;
      } else if (char === '}') {
        depth -= 1;

        if (depth === 0) {
          yield {
            objectText,
            physicalLine: objectStartLine,
            currentPhysicalLine: physicalLineCount
          };
          objectText = '';
        }
      }
    }
  }

  if (!arrayOpenSeen) {
    throw new Error('Top-level JSON array was not opened.');
  }

  if (depth !== 0) {
    throw new Error(`Unclosed JSON object that started at physical line ${objectStartLine}`);
  }

  if (!arrayCloseSeen) {
    throw new Error('Top-level JSON array was not closed.');
  }

  return {
    physicalLineCount,
    sourceArrayOpenSeen: arrayOpenSeen,
    sourceArrayCloseSeen: arrayCloseSeen
  };
}

async function main() {
  await mkdir(OUT_DIR, { recursive: true });
  const previousLedger = await readPreviousLedger(PREVIOUS_LEDGER_PATH);
  await removePartialOutputs();

  const sourceStat = await stat(SOURCE_PATH);
  const readStarted = new Date();
  const sourceStateIdentity = {
    path: SOURCE_PATH,
    size_bytes: sourceStat.size,
    mtime: sourceStat.mtime.toISOString()
  };

  const processedAt = new Date().toISOString();
  const stateCounts = {};
  const decisionCounts = {};
  const uniqueSourceRowIds = new Set();
  const uniqueSourceVersionIds = new Set();
  const outputRowsByVersionId = new Map(previousLedger.rowsByVersionId);
  const versionsBySourceRowId = new Map();
  const inputGroupsBySourceRowId = new Map();
  const inputGroupsByVersionId = new Map();
  const requiredFields = buildSchema().required;
  let requiredFieldFailures = 0;
  let invalidShapeRows = 0;
  let currentSourceRowCount = 0;
  let newProcessedRowCount = 0;
  let alreadyProcessedDoneCount = 0;
  let newContentVersionCount = 0;
  let retainedPreviousVersionCount = previousLedger.rowsByVersionId.size;
  let lineCount = 0;
  let physicalLineCount = 0;
  let parsedObjectLineCount = 0;
  let sourceArrayOpenSeen = false;
  let sourceArrayCloseSeen = false;

  const objectStream = streamTopLevelArrayObjects(SOURCE_PATH);

  try {
    while (true) {
      const nextObject = await objectStream.next();

      if (nextObject.done) {
        physicalLineCount = nextObject.value?.physicalLineCount ?? physicalLineCount;
        sourceArrayOpenSeen = nextObject.value?.sourceArrayOpenSeen ?? sourceArrayOpenSeen;
        sourceArrayCloseSeen = nextObject.value?.sourceArrayCloseSeen ?? sourceArrayCloseSeen;
        break;
      }

      const { objectText, physicalLine, currentPhysicalLine } = nextObject.value;
      physicalLineCount = currentPhysicalLine;
      let source;

      try {
        source = JSON.parse(objectText);
        parsedObjectLineCount += 1;
      } catch (error) {
        invalidShapeRows += 1;
        throw new Error(`Failed to parse source-state row at physical line ${physicalLine}: ${error instanceof Error ? error.message : String(error)}`);
      }

      if (source === null || typeof source !== 'object' || Array.isArray(source)) {
        invalidShapeRows += 1;
      }

      const probeRow = buildRow(source, sourceStateIdentity, processedAt, null);
      const previousRow = previousLedger.rowsByVersionId.get(probeRow.source_version_id);
      const row = buildRow(source, sourceStateIdentity, processedAt, previousRow);
      const sourceFingerprint = stableJson(source);

      recordDuplicateProbe(inputGroupsBySourceRowId, row.source_row_id, sourceFingerprint, row, physicalLine);
      recordDuplicateProbe(inputGroupsByVersionId, row.source_version_id, sourceFingerprint, row, physicalLine);

      if (!previousLedger.rowsByVersionId.has(row.source_version_id)) {
        newContentVersionCount += 1;
      } else {
        retainedPreviousVersionCount -= 1;
      }

      if (row.processing_decision === 'process_candidate_done' && previousRow?.processing_decision !== 'process_candidate_done') {
        newProcessedRowCount += 1;
      }

      if (row.processing_decision === 'already_processed_done') {
        alreadyProcessedDoneCount += 1;
      }

      for (const field of requiredFields) {
        if (!Object.hasOwn(row, field) || row[field] === null || row[field] === '') {
          requiredFieldFailures += 1;
        }
      }

      outputRowsByVersionId.set(row.source_version_id, row);
      currentSourceRowCount += 1;
    }
  } finally {
    await objectStream.return?.();
  }

  const sortedRows = [...outputRowsByVersionId.values()].sort((left, right) => {
    const sourceCompare = left.source_row_id.localeCompare(right.source_row_id);
    return sourceCompare === 0 ? left.source_version_id.localeCompare(right.source_version_id) : sourceCompare;
  });
  const ledger = createWriteStream(LEDGER_PATH, { encoding: 'utf8' });

  try {
    for (const row of sortedRows) {
      stateCounts[row.last_seen_state] = (stateCounts[row.last_seen_state] ?? 0) + 1;
      decisionCounts[row.processing_decision] = (decisionCounts[row.processing_decision] ?? 0) + 1;
      uniqueSourceRowIds.add(row.source_row_id);
      uniqueSourceVersionIds.add(row.source_version_id);

      if (!versionsBySourceRowId.has(row.source_row_id)) {
        versionsBySourceRowId.set(row.source_row_id, new Set());
      }

      versionsBySourceRowId.get(row.source_row_id).add(row.source_version_id);
      await writeJsonl(ledger, row);
      lineCount += 1;
    }
  } finally {
    ledger.end();
    await once(ledger, 'finish');
  }

  const sourceRowsWithMultipleVersions = [...versionsBySourceRowId.values()].filter((versions) => versions.size > 1).length;
  const duplicateInputReview = {
    source_row_id_duplicates: summarizeDuplicateGroups(inputGroupsBySourceRowId),
    source_version_id_duplicates: summarizeDuplicateGroups(inputGroupsByVersionId)
  };
  duplicateInputReview.collision_like_group_count =
    duplicateInputReview.source_row_id_duplicates.collision_like_group_count +
    duplicateInputReview.source_version_id_duplicates.collision_like_group_count;
  duplicateInputReview.verdict = duplicateInputReview.collision_like_group_count === 0 ? 'PASS_EXACT_DUPLICATES_OR_DISTINCT_VERSIONS' : 'FAIL_COLLISION_LIKE_DUPLICATES';
  const finishedAt = new Date();
  const status = requiredFieldFailures === 0 && invalidShapeRows === 0 && sourceArrayOpenSeen && sourceArrayCloseSeen && duplicateInputReview.collision_like_group_count === 0 ? 'PASS' : 'FAIL_PARTIAL';
  const summary = {
    artifact: 'source-ledger-summary',
    row_count: lineCount,
    current_source_row_count: currentSourceRowCount,
    unique_source_row_id_count: uniqueSourceRowIds.size,
    unique_source_version_id_count: uniqueSourceVersionIds.size,
    state_counts: stateCounts,
    processing_decision_counts: decisionCounts,
    previous_ledger_path: PREVIOUS_LEDGER_PATH,
    previous_ledger_row_count: previousLedger.rowCount,
    new_processed_row_count: newProcessedRowCount,
    already_processed_done_count: alreadyProcessedDoneCount,
    new_content_version_count: newContentVersionCount,
    retained_previous_version_count: retainedPreviousVersionCount,
    source_rows_with_multiple_versions: sourceRowsWithMultipleVersions,
    duplicate_input_review: duplicateInputReview,
    referenced_file_content_hashed: false,
    source_state_row_hash: true,
    sha256_meaning: 'stable hash of source-row content metadata; excludes state and volatile mtime fields',
    mtime_meaning: 'source state file mtime recorded as audit metadata only; not part of source_version_id',
    content_version_note: CONTENT_VERSION_NOTE,
    required_field_failures: requiredFieldFailures,
    invalid_shape_rows: invalidShapeRows,
    processed_version: PROCESSED_VERSION,
    processed_at: processedAt,
    source_path: SOURCE_PATH,
    source_size_bytes: sourceStat.size,
    audit_metadata_note: AUDIT_METADATA_NOTE
  };
  const report = {
    artifact: 'source-ledger-build-report',
    status,
    source_parse: {
      status: 'PASS',
      top_level_shape: 'array',
      parser: 'createReadStream_top_level_array_object_scanner',
      source_size_bytes: sourceStat.size,
      source_state_mtime: sourceStateIdentity.mtime,
      read_started_at: readStarted.toISOString(),
      full_source_read_in_memory: false,
      physical_line_count: physicalLineCount,
      parsed_object_line_count: parsedObjectLineCount,
      source_array_open_seen: sourceArrayOpenSeen,
      source_array_close_seen: sourceArrayCloseSeen,
      current_source_row_count: currentSourceRowCount,
      output_row_count: lineCount
    },
    output_write: {
      status: 'PASS',
      jsonl_path: LEDGER_PATH,
      schema_path: SCHEMA_PATH,
      summary_path: SUMMARY_PATH,
      report_path: REPORT_PATH,
      previous_ledger_path: PREVIOUS_LEDGER_PATH,
      previous_ledger_row_count: previousLedger.rowCount,
      jsonl_lines_written: lineCount,
      full_jsonl_string_built_in_memory: false
    },
    hashing_scope: {
      referenced_file_content_hashed: false,
      source_state_row_hash: true,
      sha256_meaning: 'stable hash of source-row content metadata; excludes state and volatile mtime fields',
      mtime_meaning: 'source state file mtime recorded as audit metadata only; not part of source_version_id',
      audit_metadata_note: AUDIT_METADATA_NOTE,
      content_version_note: CONTENT_VERSION_NOTE
    },
    required_fields: {
      status: requiredFieldFailures === 0 ? 'PASS' : 'FAIL_PARTIAL',
      required_field_failures: requiredFieldFailures,
      required_fields: requiredFields
    },
    unique_ids: {
      unique_source_row_id_count: uniqueSourceRowIds.size,
      unique_source_version_id_count: uniqueSourceVersionIds.size,
      source_rows_with_multiple_versions: sourceRowsWithMultipleVersions
    },
    duplicate_input_review: duplicateInputReview,
    rerun_semantics: {
      status: 'PASS',
      previous_ledger_path: PREVIOUS_LEDGER_PATH,
      previous_ledger_row_count: previousLedger.rowCount,
      current_source_row_count: currentSourceRowCount,
      new_processed_row_count: newProcessedRowCount,
      already_processed_done_count: alreadyProcessedDoneCount,
      new_content_version_count: newContentVersionCount,
      retained_previous_version_count: retainedPreviousVersionCount,
      source_rows_with_multiple_versions: sourceRowsWithMultipleVersions,
      content_version_note: CONTENT_VERSION_NOTE
    },
    state_counts: stateCounts,
    processing_decision_counts: decisionCounts,
    audit_metadata_note: AUDIT_METADATA_NOTE,
    started_at_utc: startedAt.toISOString(),
    finished_at_utc: finishedAt.toISOString(),
    elapsed_ms: finishedAt.getTime() - startedAt.getTime()
  };

  await writeJson(SCHEMA_PATH, buildSchema());
  await writeJson(SUMMARY_PATH, summary);
  await writeJson(REPORT_PATH, report);

  console.log(JSON.stringify({
    status: report.status,
    row_count: summary.row_count,
    current_source_row_count: summary.current_source_row_count,
    unique_source_row_id_count: summary.unique_source_row_id_count,
    unique_source_version_id_count: summary.unique_source_version_id_count,
    state_counts: summary.state_counts,
    new_processed_row_count: summary.new_processed_row_count,
    new_content_version_count: summary.new_content_version_count,
    retained_previous_version_count: summary.retained_previous_version_count,
    referenced_file_content_hashed: summary.referenced_file_content_hashed,
    source_state_row_hash: summary.source_state_row_hash,
    required_field_failures: summary.required_field_failures,
    invalid_shape_rows: summary.invalid_shape_rows,
    elapsed_ms: report.elapsed_ms
  }, null, 2));
}

main().catch((error) => {
  console.error(JSON.stringify({
    status: 'FAIL_PARTIAL',
    reason: error instanceof Error ? error.message : String(error),
    source_path: SOURCE_PATH,
    audit_metadata_note: AUDIT_METADATA_NOTE
  }, null, 2));
  process.exitCode = 1;
});
