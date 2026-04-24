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
 * Then expose via cloudflared:
 *   cloudflared tunnel --url http://localhost:8080
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
const COMPRESSION_ENTRIES = Object.entries(COMPRESSION_MAP);

// e.g. "foo.framework.js.br" → { contentType: "application/javascript", contentEncoding: "br" }
function resolveFileInfo(filePath) {
  let p = filePath;
  let contentEncoding = null;
  for (const [ext, encoding] of COMPRESSION_ENTRIES) {
    if (p.endsWith(ext)) {
      p = p.slice(0, -ext.length);
      contentEncoding = encoding;
      break;
    }
  }
  const ext = path.extname(p).toLowerCase();
  return { contentType: MIME_TYPES[ext] || "application/octet-stream", contentEncoding };
}

// --- Server ---
const server = http.createServer((req, res) => {
  let urlPath;
  try {
    urlPath = new URL(req.url, "http://localhost").pathname;
  } catch {
    res.writeHead(400);
    res.end("Bad Request");
    return;
  }
  if (urlPath === "/") urlPath = "/index.html";

  const filePath = path.join(ROOT, urlPath);

  // Prevent directory traversal
  if (!filePath.startsWith(ROOT)) {
    res.writeHead(403);
    res.end("Forbidden");
    return;
  }

  let stat;
  try {
    stat = fs.statSync(filePath);
  } catch (err) {
    res.writeHead(err.code === "ENOENT" ? 404 : 500);
    res.end(err.code === "ENOENT" ? "Not Found: " + urlPath : "Internal Server Error");
    return;
  }

  if (!stat.isFile()) {
    res.writeHead(404);
    res.end("Not Found: " + urlPath);
    return;
  }

  const { contentType, contentEncoding } = resolveFileInfo(filePath);

  const headers = {
    "Content-Type": contentType,
    "Content-Length": stat.size,
    "Access-Control-Allow-Origin": "*",
    "Cross-Origin-Opener-Policy": "same-origin", // Required for SharedArrayBuffer
    "Cross-Origin-Embedder-Policy": "require-corp",
  };

  if (contentEncoding) {
    headers["Content-Encoding"] = contentEncoding;
  }

  res.writeHead(200, headers);
  const stream = fs.createReadStream(filePath);
  stream.on("error", () => res.destroy());
  stream.pipe(res);
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
  ║  cloudflared tunnel --url http://localhost:${String(PORT).padEnd(6)}║
  ╚══════════════════════════════════════════════════╝
  `);
});
