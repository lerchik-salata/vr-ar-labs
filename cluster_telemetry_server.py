#!/usr/bin/env python3
"""
Cluster Telemetry API: local mock server for Lab 5.

  GET  /api/nodes  -> node telemetry (cpu, mem, pods, status)
  POST /api/audit  -> accepts an operator audit report
  GET  /           -> health check

Run: python3 cluster_telemetry_server.py  (Ctrl+C to stop)
"""

import json
import random
import datetime
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

HOST = "127.0.0.1"
PORT = 8080

NODE_NAMES = ["node-1", "node-2", "node-3"]
STATUSES = ["Ready", "Ready", "Ready", "NotReady"]  # mostly Ready


def make_telemetry():
    """Build randomized cluster telemetry."""
    nodes = []
    for name in NODE_NAMES:
        nodes.append({
            "name": name,
            "cpu": round(random.uniform(15, 85), 1),
            "mem": round(random.uniform(30, 90), 1),
            "pods": random.randint(4, 18),
            "status": random.choice(STATUSES),
        })
    return {
        "cluster": "cloud-mesh",
        "timestamp": datetime.datetime.utcnow().isoformat() + "Z",
        "nodes": nodes,
    }


class Handler(BaseHTTPRequestHandler):
    def _send(self, code, payload):
        body = json.dumps(payload).encode("utf-8")
        self.send_response(code)
        self.send_header("Content-Type", "application/json")
        self.send_header("Access-Control-Allow-Origin", "*")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def do_GET(self):
        if self.path.startswith("/api/nodes"):
            self._send(200, make_telemetry())
        elif self.path == "/" or self.path.startswith("/health"):
            self._send(200, {"service": "cluster-telemetry-api", "status": "up"})
        else:
            self._send(404, {"error": "not found", "path": self.path})

    def do_POST(self):
        if self.path.startswith("/api/audit"):
            length = int(self.headers.get("Content-Length", 0))
            raw = self.rfile.read(length) if length else b""
            try:
                report = json.loads(raw.decode("utf-8")) if raw else {}
            except json.JSONDecodeError:
                self._send(400, {"error": "invalid json"})
                return
            print("\n=== AUDIT REPORT RECEIVED ===")
            print(json.dumps(report, ensure_ascii=False, indent=2))
            print("=============================\n")
            self._send(200, {
                "status": "accepted",
                "auditId": "audit-" + datetime.datetime.utcnow().strftime("%Y%m%d%H%M%S"),
                "message": "Report stored in cluster control plane",
            })
        else:
            self._send(404, {"error": "not found", "path": self.path})

    # Compact request log
    def log_message(self, fmt, *args):
        print("[%s] %s" % (self.log_date_time_string(), fmt % args))


if __name__ == "__main__":
    srv = ThreadingHTTPServer((HOST, PORT), Handler)
    print("Cluster Telemetry API running at http://%s:%d" % (HOST, PORT))
    print("  GET  http://localhost:%d/api/nodes" % PORT)
    print("  POST http://localhost:%d/api/audit" % PORT)
    print("Press Ctrl+C to stop.")
    try:
        srv.serve_forever()
    except KeyboardInterrupt:
        print("\nStopped.")
        srv.server_close()
