#!/usr/bin/env node

/**
 * Unity WebGL Brotli/Gzip Server
 *
 * Usage:
 *   node serve-unity-webgl.js [build-output-folder] [port]
 *
 * Examples:
 *   node serve-unity-webgl.js ./Build 8080
 *   node serve-unity-webgl.js .        3000
 *
 * Then expose via ngrok:
 *   ngrok http 8080
 */

const http = require("http");
const fs = require("fs");
const path = require("path");

// --- Config ---
const ROOT = path.resolve(process.argv[2] || ".");
const PORT = parseInt(process.argv[3], 10) || 8080;

// --- MIME Types ---
const MIME_TYPES = {
  ".html": "text/html; charset=utf-8",
  ".js":   "application/javascript",
  ".mjs":  "application/javascript",
  ".css":  "text/css",
  ".json": "application/json",
  ".wasm": "application/wasm",
  ".data": "application/octet-stream",
  ".png":  "image/png",
  ".jpg":  "image/jpeg",
  ".svg":  "image/svg+xml",
  ".ico":  "image/x-icon",
  ".gif":  "image/gif",
  ".webp": "image/webp",
  ".mp3":  "audio/mpeg",
  ".ogg":  "audio/ogg",
  ".wav":  "audio/wav",
  ".mp4":  "video/mp4",
  ".webm": "video/webm",
  ".ttf":  "font/ttf",
  ".woff": "font/woff",
  ".woff2":"font/woff2",
};

// --- Compression Mapping ---
// Unity outputs files with .br (Brotli) or .gz (Gzip) extensions.
// The Content-Encoding header must be set so the browser can decompress them.
const COMPRESSION_MAP = {
  ".br": "br",
  ".gz": "gzip",
};

function getContentType(filePath) {
  // .framework.js.br → return MIME for .js
  // .wasm.br         → return MIME for .wasm
  let p = filePath;
  for (const compExt of Object.keys(COMPRESSION_MAP)) {
    if (p.endsWith(compExt)) {
      p = p.slice(0, -compExt.length);
      break;
    }
  }
  const ext = path.extname(p).toLowerCase();
  return MIME_TYPES[ext] || "application/octet-stream";
}

function getContentEncoding(filePath) {
  for (const [ext, encoding] of Object.entries(COMPRESSION_MAP)) {
    if (filePath.endsWith(ext)) return encoding;
  }
  return null;
}

// --- Server ---
const server = http.createServer((req, res) => {
  // Extract path from URL (strip query string)
  let urlPath = decodeURIComponent(req.url.split("?")[0]);
  if (urlPath === "/") urlPath = "/index.html";

  const filePath = path.join(ROOT, urlPath);

  // Prevent directory traversal
  if (!filePath.startsWith(ROOT)) {
    res.writeHead(403);
    res.end("Forbidden");
    return;
  }

  // Check if file exists
  if (!fs.existsSync(filePath)) {
    res.writeHead(404);
    res.end("Not Found: " + urlPath);
    return;
  }

  const contentType = getContentType(filePath);
  const contentEncoding = getContentEncoding(filePath);

  const headers = {
    "Content-Type": contentType,
    "Access-Control-Allow-Origin": "*",          // CORS
    "Cross-Origin-Opener-Policy": "same-origin", // Required for SharedArrayBuffer
    "Cross-Origin-Embedder-Policy": "require-corp",
  };

  if (contentEncoding) {
    headers["Content-Encoding"] = contentEncoding;
  }

  const stat = fs.statSync(filePath);
  headers["Content-Length"] = stat.size;

  res.writeHead(200, headers);
  fs.createReadStream(filePath).pipe(res);
});

server.listen(PORT, () => {
  console.log(`
  ╔══════════════════════════════════════════════════╗
  ║  Unity WebGL Server                              ║
  ╠══════════════════════════════════════════════════╣
  ║  Root:  ${ROOT.padEnd(40)}║
  ║  URL:   http://localhost:${String(PORT).padEnd(24)}║
  ╠══════════════════════════════════════════════════╣
  ║  Brotli (.br)  → Content-Encoding: br           ║
  ║  Gzip   (.gz)  → Content-Encoding: gzip         ║
  ╠══════════════════════════════════════════════════╣
  ║  Next step:                                      ║
  ║  ngrok http ${String(PORT).padEnd(37)}║
  ╚══════════════════════════════════════════════════╝
  `);
});
