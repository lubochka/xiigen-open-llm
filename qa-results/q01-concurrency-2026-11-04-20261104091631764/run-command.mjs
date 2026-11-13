import fs from 'node:fs';
import { spawn } from 'node:child_process';
import crypto from 'node:crypto';
const [label, seconds, ...args] = process.argv.slice(2);
const root = new URL('.', import.meta.url);
const file = name => new URL(label + name, root);
const stdout = fs.createWriteStream(file('.stdout.log'), { flags: 'wx' });
const stderr = fs.createWriteStream(file('.stderr.log'), { flags: 'wx' });
const started = new Date().toISOString();
const start = performance.now();
const child = spawn('dotnet', args, { cwd: process.cwd(), windowsHide: true, env: process.env });
let timedOut = false;
const timer = setTimeout(() => { timedOut = true; spawn('taskkill', ['/PID', String(child.pid), '/T', '/F'], { windowsHide: true }); }, Number(seconds) * 1000);
child.stdout.pipe(stdout); child.stderr.pipe(stderr);
child.on('error', error => stderr.write(String(error)));
child.on('close', (exit, signal) => {
 clearTimeout(timer); stdout.end(); stderr.end();
 const receipt = { command: 'dotnet', args, cwd: process.cwd(), pid: child.pid, start_utc: started,
 end_utc: new Date().toISOString(), wall_ms: performance.now() - start, exit, signal, timeout: timedOut };
 fs.writeFileSync(file('.process.json'), JSON.stringify(receipt, null, 2) + '\n');
 console.log(JSON.stringify(receipt)); process.exitCode = timedOut ? 124 : exit ?? 1;
});
