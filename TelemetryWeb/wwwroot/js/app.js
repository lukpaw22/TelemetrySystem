// ============================================================
// app.js – logika klienta SignalR
// ============================================================

const HUB_URL = "/alertHub";

let counts = { total: 0, crit: 0, warn: 0, ok: 0 };

// ── Połączenie z hubem ───────────────────────────────────────
const connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL)
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

// ── Obsługa statusu połączenia ───────────────────────────────
function setStatus(state) {
    const el = document.getElementById("status");
    el.className = "status " + state;
    el.textContent =
        state === "connected"    ? "● Połączono"  :
        state === "disconnected" ? "● Rozłączono" : "● Łączenie...";
}

connection.onreconnecting(() => setStatus("connecting"));
connection.onreconnected(() => setStatus("connected"));
connection.onclose(() => setStatus("disconnected"));

// ── Nasłuchiwanie na alerty z serwera ────────────────────────
connection.on("ReceiveAlert", (alert) => {
    console.log("[SignalR] Odebrano alert:", alert);
    addAlert(alert);
});

// ── Start połączenia ─────────────────────────────────────────
async function startConnection() {
    setStatus("connecting");
    try {
        await connection.start();
        setStatus("connected");
        console.log("[SignalR] Połączono z hubem.");
    } catch (err) {
        console.error("[SignalR] Błąd połączenia:", err);
        setStatus("disconnected");
        setTimeout(startConnection, 5000); // ponów po 5s
    }
}

// ── Dodaj kartę alertu do UI ─────────────────────────────────
function addAlert(alert) {
    const list = document.getElementById("alertList");

    // Usuń komunikat "brak alertów"
    const empty = list.querySelector(".empty-msg");
    if (empty) empty.remove();

    const level = (alert.level || "info").toLowerCase();
    const icon  = level === "crit" ? "" :
                  level === "warn" ? "" :
                  level === "ok"   ? "" : "";

    const time = alert.time
        ? new Date(alert.time).toLocaleString("pl-PL")
        : new Date().toLocaleString("pl-PL");

    const card = document.createElement("div");
    card.className = `alert-card ${level}`;
    card.innerHTML = `
        <div class="alert-icon">${icon}</div>
        <div class="alert-body">
            <div class="alert-header">
                <span class="alert-name">${escapeHtml(alert.checkName || "Alert")}</span>
                <span class="alert-badge badge-${level}">${level}</span>
            </div>
            <div class="alert-message">${escapeHtml(alert.message || "")}</div>
            <div class="alert-meta">
                📊 ${escapeHtml(alert.measurement || "-")} &nbsp;|&nbsp; 🕐 ${time}
            </div>
        </div>
    `;

    // Nowe alerty na górze
    list.insertBefore(card, list.firstChild);

    // Aktualizuj liczniki
    counts.total++;
    if (level === "crit") counts.crit++;
    else if (level === "warn") counts.warn++;
    else if (level === "ok")   counts.ok++;
    updateCounts();
}

function updateCounts() {
    document.getElementById("countTotal").textContent = counts.total;
    document.getElementById("countCrit").textContent  = counts.crit;
    document.getElementById("countWarn").textContent  = counts.warn;
    document.getElementById("countOk").textContent    = counts.ok;
}

// ── Wyczyść listę alertów ────────────────────────────────────
function clearAlerts() {
    const list = document.getElementById("alertList");
    list.innerHTML = '<p class="empty-msg">Brak alertów. Oczekiwanie na dane...</p>';
    counts = { total: 0, crit: 0, warn: 0, ok: 0 };
    updateCounts();
}

// ── Testowy alert (wywołuje endpoint /api/webhook/test) ──────
async function sendTestAlert() {
    const levels = ["ok", "warn", "crit", "info"];
    const level  = levels[Math.floor(Math.random() * levels.length)];

    try {
        await fetch("/api/webhook/test", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                message: `Testowy alert – temperatura przekroczyła próg (poziom: ${level})`,
                level: level
            })
        });
    } catch (err) {
        console.error("[TEST] Błąd wysyłania testowego alertu:", err);
    }
}

// ── Helper XSS ───────────────────────────────────────────────
function escapeHtml(str) {
    return String(str)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;");
}

// ── Start ────────────────────────────────────────────────────
startConnection();
