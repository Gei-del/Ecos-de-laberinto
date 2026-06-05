/* ==========================================================
   MAZE RUNNER v4.0 - Cronofragmentos (COOP)
   Motor: Canvas 2D / 60fps / sin dependencias externas
   Evolución de la v3.0 (single-player) a experiencia
   cooperativa local de 1 a 4 jugadores. Se conservan TODOS
   los sistemas originales (laberinto, IA de fantasmas, audio
   generativo, misiones, logros, persistencia) y se añaden:
     - Jugadores múltiples (players[]) con controles propios
     - Mecánica de "caído / revivir" cooperativa
     - Salida cooperativa (todos deben escapar)
     - Audio espacial (paneo estéreo) + tensión adaptativa
     - Modo invitado y modo cuenta (Google / correo local)
     - Cartera persistente, recompensa diaria y ranking local

   MODULOS:
   00 CONFIG/CONTROLS  - Constantes, controles por jugador
   01 PROFILE          - Cuenta, cartera, ranking (localStorage)
   02 CHARACTERS       - 6 personajes con stats y bonus
   03 GHOST AI         - 4 tipos: hunter, whisper, guardian, chaos
   04 AUDIO            - Web Audio API generativa + paneo espacial
   05 STATE            - Variables de juego globales + players[]
   06 DOM              - Referencias a elementos HTML
   07 RESIZE           - Adaptacion responsive
   08 MAZE GEN         - Backtracker recursivo + rutas alternativas
   09 EXIT LOGIC       - Salida cooperativa del laberinto
   10 PLACEMENT        - Colocacion de dots, pups, ghosts
   11 GAME CTRL        - Init, start, nextLevel, caído, revivir, wipe
   12 MOVEMENT         - Movimiento fluido por jugador
   13 GHOST TICK       - Loop de IA (objetivo: jugador más cercano)
   14 COLLISION        - Colisiones por jugador
   15 MAGNET           - Power-up iman por jugador
   16 PARTICLES        - Sistema de particulas con gravedad
   17 DRAW             - Render: bg, maze, dots, pups, ghosts, players
   18 MINIMAP          - Mapa pequeño con todos los jugadores
   19 MAIN LOOP        - requestAnimationFrame con delta time
   20 INPUT            - Teclado multi-jugador, D-pad, swipe
   21 MISSIONS         - Misiones diarias persistentes
   22 ACHIEVEMENTS     - Logros con verificacion periodica
   23 PERSISTENCE      - localStorage: record, chars, logros, cartera
   24 UI HELPERS       - Toast, banners, score pops, HUD, ranking
   25 BOOT             - Inicializacion, menu, cuenta
   ========================================================== */
(function(){
'use strict';

/* ===================== 00 CONFIG / CONTROLS ===================== */
var CFG = {
  PU_DUR:      8,     // segundos de power-up base
  COMBO_T:     2.8,   // segundos antes de resetear combo
  BASE_LIVES:  3,     // vidas base del equipo
  MAX_LVL:     16,    // niveles totales
  DOT_PTS:     10,    // puntos por moneda normal
  BIG_PTS:     25,    // puntos por moneda grande
  CRYS_PTS:    50,    // puntos por cristal
  GHOST_PTS:   500,   // puntos por fantasma comido
  PUP_PTS:     300,   // puntos por power-up
  LVL_BONUS:   1000,  // bonus al completar nivel
  ACH_BONUS:   150,   // bonus por logro
  REVIVE_T:    0.9,   // segundos junto a un compañero para revivirlo
  DAILY_COINS: 100,   // recompensa diaria
};

// Direcciones (arriba, derecha, abajo, izquierda)
var DX = [0, 1, 0, -1];
var DY = [-1, 0, 1, 0];
var PU_TYPES  = ['speed','ghost','magnet','freeze'];
var PU_COLORS = {speed:'#0af', ghost:'#f0f', magnet:'#fd0', freeze:'#0ff'};
var PU_ICONS  = {speed:'⚡', ghost:'👻', magnet:'🧲', freeze:'❄'};

// Esquema de control por jugador (teclado). El J1 también responde a D-pad/swipe.
var CONTROLS = [
  { name:'WASD / Flechas', up:['ArrowUp','w','W'],    right:['ArrowRight','d','D'], down:['ArrowDown','s','S'], left:['ArrowLeft','a','A'] },
  { name:'I J K L',        up:['i','I'],               right:['l','L'],             down:['k','K'],             left:['j','J'] },
  { name:'T F G H',        up:['t','T'],               right:['h','H'],             down:['g','G'],             left:['f','F'] },
  { name:'Numpad ↑←↓→',    up:['8','Up'],              right:['6','Right'],         down:['5','2','Down'],      left:['4','Left'] },
];
// Lookup key -> [playerIndex, dir]
var KEYS = {};
CONTROLS.forEach(function(c, pi){
  ['up','right','down','left'].forEach(function(d, di){
    c[d].forEach(function(k){ KEYS[k] = [pi, di]; });
  });
});

// Google client id: se puede definir sin tocar código vía
// localStorage.setItem('mzr_gcid','<TU_CLIENT_ID>') o window.MAZE_GOOGLE_CLIENT_ID
var GOOGLE_CLIENT_ID = (function(){
  try { return window.MAZE_GOOGLE_CLIENT_ID || localStorage.getItem('mzr_gcid') || ''; }
  catch(e){ return window.MAZE_GOOGLE_CLIENT_ID || ''; }
})();

/* ===================== 02 CHARACTERS ===================== */
var CHARS = [
  { id:'dog',     name:'Max',    emoji:'🐶', trait:'Velocidad x1.25',   color:'#ff9944', bonus:{speed:1.25},       cost:0,   unlocked:true,  trail:'rgba(255,153,68,' },
  { id:'cat',     name:'Luna',   emoji:'🐱', trait:'Iman x1.5',         color:'#cc88ff', bonus:{magnetRange:1.5},  cost:0,   unlocked:true,  trail:'rgba(200,136,255,' },
  { id:'monkey',  name:'Kiko',   emoji:'🐵', trait:'Power-ups +50%',    color:'#ffcc44', bonus:{pupDur:1.5},       cost:100, unlocked:false, trail:'rgba(255,204,68,' },
  { id:'dragon',  name:'Ember',  emoji:'🐲', trait:'Invenc. x2',        color:'#ff5533', bonus:{invDur:2},         cost:200, unlocked:false, trail:'rgba(255,85,51,' },
  { id:'parrot',  name:'Loro',   emoji:'🦜', trait:'Combo +60%',        color:'#44ff88', bonus:{comboT:1.6},       cost:350, unlocked:false, trail:'rgba(68,255,136,' },
  { id:'raccoon', name:'Bandit', emoji:'🦝', trait:'Puntos x1.5',       color:'#88aacc', bonus:{dotMult:1.5},      cost:500, unlocked:false, trail:'rgba(136,170,204,' },
];
function charById(id){ for (var i=0;i<CHARS.length;i++) if (CHARS[i].id===id) return CHARS[i]; return CHARS[0]; }

/* ===================== 03 GHOST AI ===================== */
function aiChase(g, tx, ty, noise) {
  var back = (g.dir + 2) % 4;
  var bestDir = -1, bestScore = -Infinity;
  var order = [0,1,2,3].sort(function(){ return Math.random() - 0.5; });
  for (var i = 0; i < 4; i++) {
    var d = order[i];
    var nx = g.gx + DX[d], ny = g.gy + DY[d];
    if (isWall(nx, ny)) continue;
    var sc = -Math.hypot(nx - tx, ny - ty) + Math.random() * noise;
    if (d === back) sc -= 4;
    if (sc > bestScore) { bestScore = sc; bestDir = d; }
  }
  return bestDir;
}
function aiWander(g) {
  var back = (g.dir + 2) % 4;
  var dirs = [0,1,2,3].filter(function(d) {
    return !isWall(g.gx + DX[d], g.gy + DY[d]) && d !== back;
  });
  return dirs.length ? dirs[Math.floor(Math.random() * dirs.length)] : back;
}

var GHOST_DEFS = {
  // Cazador: persigue directo, recuerda la última posición vista
  hunter: {
    name:'Cazador', color:'#ff4455', speed:0.75,
    ai: function(g, tp) {
      if (g.lastSeen && Math.random() < 0.3) {
        var d = aiChase(g, g.lastSeen[0], g.lastSeen[1], 0.5);
        if (d >= 0) return d;
      }
      g.lastSeen = [tp.gx, tp.gy];
      return aiChase(g, tp.gx, tp.gy, 1.0);
    }
  },
  // Susurrador: intenta colocarse a espaldas del jugador objetivo
  whisper: {
    name:'Susurrador', color:'#cc44ff', speed:0.60,
    ai: function(g, tp) {
      var bx = Math.max(0, Math.min(COLS-1, tp.gx - DX[tp.dir] * 4));
      var by = Math.max(0, Math.min(ROWS-1, tp.gy - DY[tp.dir] * 4));
      if (Math.random() < 0.02) SFX.whisperAt(panOfCell(g.gx));
      return aiChase(g, bx, by, 0.8);
    }
  },
  // Guardian: patrulla su zona, ataca si el jugador se acerca
  guardian: {
    name:'Guardian', color:'#ff8844', speed:0.55,
    ai: function(g, tp) {
      if (!g.ox) { g.ox = g.gx; g.oy = g.gy; }
      var dOrigin = Math.hypot(g.gx - g.ox, g.gy - g.oy);
      if (dOrigin > 7) return aiChase(g, g.ox, g.oy, 0.9);
      var dPlayer = Math.hypot(tp.gx - g.gx, tp.gy - g.gy);
      if (dPlayer < 5) return aiChase(g, tp.gx, tp.gy, 0.8);
      return aiWander(g);
    }
  },
  // Caotico: objetivo aleatorio que cambia seguido
  chaos: {
    name:'Caotico', color:'#44ff88', speed:0.90,
    ai: function(g, tp) {
      if (!g.ct || Math.random() < 0.07) {
        g.ct = [1 + Math.floor(Math.random()*(COLS-2)), 1 + Math.floor(Math.random()*(ROWS-2))];
      }
      return aiChase(g, g.ct[0], g.ct[1], 1.4);
    }
  },
};

/* ===================== 04 AUDIO (con paneo espacial) ===================== */
var SFX = (function() {
  var AC = null, MG = null, ok = true;
  var tensionGain = null, breath = null, breathGain = null;

  function init() {
    if (AC) return;
    try {
      AC = new (window.AudioContext || window.webkitAudioContext)();
      MG = AC.createGain(); MG.gain.value = 0.3; MG.connect(AC.destination);
      tensionGain = AC.createGain(); tensionGain.gain.value = 0; tensionGain.connect(MG);
      // respiración de tensión (ruido filtrado pulsante)
      breathGain = AC.createGain(); breathGain.gain.value = 0; breathGain.connect(MG);
    } catch(e) { ok = false; }
  }
  function resume() { if (AC && AC.state === 'suspended') AC.resume(); }

  // construye una cadena gain->(panner)->MG según pan [-1..1]
  function out(pan) {
    var node = MG;
    if (pan != null && AC.createStereoPanner) {
      var pn = AC.createStereoPanner();
      pn.pan.value = Math.max(-1, Math.min(1, pan));
      pn.connect(MG); node = pn;
    }
    return node;
  }

  function tone(freq, dur, type, vol, delay, vibrato, pan) {
    if (!ok || !AC) return;
    try {
      resume();
      var dest = out(pan);
      var t = AC.currentTime + (delay || 0);
      var osc = AC.createOscillator();
      var gain = AC.createGain();
      osc.type = type || 'sine';
      osc.frequency.setValueAtTime(freq, t);
      if (vibrato) {
        var lfo = AC.createOscillator();
        var lfog = AC.createGain();
        lfo.frequency.value = vibrato;
        lfog.gain.value = freq * 0.02;
        lfo.connect(lfog); lfog.connect(osc.frequency);
        lfo.start(t); lfo.stop(t + dur + 0.01);
      }
      osc.frequency.exponentialRampToValueAtTime(Math.max(20, freq * 0.5), t + dur);
      gain.gain.setValueAtTime(vol || 0.14, t);
      gain.gain.exponentialRampToValueAtTime(0.0001, t + dur);
      osc.connect(gain); gain.connect(dest);
      osc.start(t); osc.stop(t + dur + 0.01);
    } catch(e) {}
  }

  function noise(dur, vol, delay, pan) {
    if (!ok || !AC) return;
    try {
      resume();
      var dest = out(pan);
      var t = AC.currentTime + (delay || 0);
      var bufSize = Math.max(1, Math.floor(AC.sampleRate * dur));
      var buf = AC.createBuffer(1, bufSize, AC.sampleRate);
      var data = buf.getChannelData(0);
      for (var i = 0; i < bufSize; i++) data[i] = (Math.random() * 2 - 1) * 0.3;
      var src = AC.createBufferSource();
      var gain = AC.createGain();
      var filt = AC.createBiquadFilter();
      filt.type = 'bandpass'; filt.frequency.value = 800;
      src.buffer = buf;
      gain.gain.setValueAtTime(vol || 0.05, t);
      gain.gain.exponentialRampToValueAtTime(0.0001, t + dur);
      src.connect(filt); filt.connect(gain); gain.connect(dest);
      src.start(t); src.stop(t + dur + 0.01);
    } catch(e) {}
  }

  // tensión adaptativa: cuanto más cerca el fantasma, más intensa
  function setTension(dist) {
    if (!tensionGain || !AC) return;
    var v = dist < 2 ? 0.4 : dist < 4 ? 0.2 : dist < 6 ? 0.08 : 0;
    tensionGain.gain.setTargetAtTime(v, AC.currentTime, 0.3);
    // respiración: sube de volumen y ritmo con la cercanía
    if (breathGain) breathGain.gain.setTargetAtTime(dist < 3 ? 0.05 : dist < 5 ? 0.02 : 0, AC.currentTime, 0.4);
    if (dist < 3 && Math.random() < 0.02) noise(0.35, 0.05, 0, (Math.random()*2-1));
  }

  return {
    init: init, resume: resume, setTension: setTension,
    coin:     function(pan) { tone(880,0.08,'sine',0.1,0,0,pan); tone(1320,0.06,'sine',0.08,0.05,0,pan); },
    bigCoin:  function(pan) { [440,550,660,880].forEach(function(f,i){tone(f,0.1,'sine',0.12,i*0.05,0,pan);}); },
    pup:      function(pan) { [500,600,800,1000].forEach(function(f,i){tone(f,0.12,'sine',0.12,i*0.06,0,pan);}); },
    eatGhost: function(pan) { [440,330,220].forEach(function(f,i){tone(f,0.15,'sawtooth',0.15,i*0.08,0,pan);}); },
    die:      function() { [330,220,110,55].forEach(function(f,i){tone(f,0.18,'square',0.2,i*0.1);}); },
    revive:   function(pan) { [330,440,660,880].forEach(function(f,i){tone(f,0.14,'sine',0.14,i*0.06,0,pan);}); },
    win:      function() { [523,659,784,1047].forEach(function(f,i){tone(f,0.22,'sine',0.18,i*0.12);}); },
    combo:    function(n) { tone(440+n*80,0.1,'triangle',0.12,0,6); },
    step:     function() { if(Math.random()<0.25) tone(180+Math.random()*40,0.04,'sine',0.02); },
    whisperAt:function(pan) { noise(0.5, 0.04, 0, pan); },
    exit:     function() { [784,1047,1318].forEach(function(f,i){tone(f,0.2,'sine',0.15,i*0.1);}); },
  };
})();

/* ===================== 05 STATE ===================== */
var gstate = 'menu'; // menu | playing | paused | gameover | win
var score = 0, lives = 3, level = 1, coins = 0, bestScore = 0;
var frameN = 0, lastTS = 0, dt = 0;
var screenShake = 0, flashTimer = 0;
var ghostScared = 0;   // temporizador global de "fantasmas asustados"
var globalFreeze = 0;  // temporizador global de congelado

// Grid del laberinto
var COLS = 21, ROWS = 21, CS = 24, OX = 0, OY = 50;
var maze = [];   // 0=suelo, 1=pared

// Jugadores (1..4)
var players = [];
var numPlayers = 1;
var SPAWNS = [[1,1],[3,1],[1,3],[3,3]];

// Entidades
var dots = [], pups = [], ghosts = [], particles = [];

// Salida del laberinto
var exitX = -1, exitY = -1;

// Estadisticas de sesion
var SS = { deaths:0, ghostsEaten:0, pupsGot:0, maxCombo:1, revives:0, startTime:0 };

// Misiones y logros
var mission = null, missionProg = 0;
var achievements = [];

// Perfil / cuenta
var profile = null;

/* ===================== 01 PROFILE / WALLET / RANKING ===================== */
function defaultProfile(){
  return { type:'guest', name:'Invitado', email:'', picture:'', avatar:'dog',
           wallet:0, unlocked:['dog','cat'], achievements:[], lastDaily:'' };
}
function loadProfile(){
  try { profile = JSON.parse(localStorage.getItem('mzr_profile')||'null'); } catch(e){ profile=null; }
  if (!profile) profile = defaultProfile();
  // aplicar desbloqueos guardados a los CHARS
  CHARS.forEach(function(c){ c.unlocked = profile.unlocked.indexOf(c.id) >= 0; });
  achievements = profile.achievements || [];
}
function saveProfile(){
  profile.unlocked = CHARS.filter(function(c){return c.unlocked;}).map(function(c){return c.id;});
  profile.achievements = achievements;
  try { localStorage.setItem('mzr_profile', JSON.stringify(profile)); } catch(e) {}
}
function claimDaily(){
  var today = new Date().toDateString();
  if (profile.lastDaily !== today) {
    profile.lastDaily = today;
    profile.wallet += CFG.DAILY_COINS;
    saveProfile();
    setTimeout(function(){ showToast('🎁 Recompensa diaria: +'+CFG.DAILY_COINS+' 🪙'); }, 600);
  }
}
function loadLeaderboard(){
  try { return JSON.parse(localStorage.getItem('mzr_lb')||'[]'); } catch(e){ return []; }
}
function pushLeaderboard(name, sc){
  if (sc <= 0) return;
  var lb = loadLeaderboard();
  lb.push({ name:name||'Anon', score:sc, date:Date.now() });
  lb.sort(function(a,b){ return b.score - a.score; });
  lb = lb.slice(0, 8);
  try { localStorage.setItem('mzr_lb', JSON.stringify(lb)); } catch(e) {}
}

/* ===================== 06 DOM ===================== */
var canvas  = document.getElementById('gc');
var ctx     = canvas.getContext('2d');
var mmCv    = document.getElementById('mm');
var mctx    = mmCv.getContext('2d');
var rootEl  = document.getElementById('root');
var screenEl= document.getElementById('screen');
var hScore  = document.getElementById('hScore');
var hLevel  = document.getElementById('hLevel');
var hCombo  = document.getElementById('hCombo');
var hLives  = document.getElementById('hLives');
var hCoins  = document.getElementById('hCoins');
var hBest   = document.getElementById('hBest');
var hEmoji  = document.getElementById('hud-emoji');
var hName   = document.getElementById('hud-name');
var flashEl = document.getElementById('flash');
var toastEl = document.getElementById('toast');
var popsEl  = document.getElementById('pops');
var puEls   = {speed:document.getElementById('puS'),ghost:document.getElementById('puG'),magnet:document.getElementById('puM'),freeze:document.getElementById('puF')};
var lvlBnr  = document.getElementById('lvl-banner');
var comboPop= document.getElementById('combo-pop');
var pauseEl = document.getElementById('pause');
var dangerEl= document.getElementById('dp');
var missionEl=document.getElementById('mpop');
var exitMark= document.getElementById('exit-marker');
var pchipsEl= document.getElementById('pchips');

/* ===================== 07 RESIZE ===================== */
function resize() {
  var W = rootEl.clientWidth || window.innerWidth;
  var H = rootEl.clientHeight || window.innerHeight;
  canvas.width = W; canvas.height = H;
  var avW = W - 4, avH = H - 55;
  CS = Math.max(16, Math.min(32, Math.floor(Math.min(avW/23, avH/19))));
  COLS = Math.floor(avW / CS); ROWS = Math.floor(avH / CS);
  if (COLS % 2 === 0) COLS--; if (ROWS % 2 === 0) ROWS--;
  COLS = Math.max(13, Math.min(35, COLS)); ROWS = Math.max(11, Math.min(25, ROWS));
  OX = Math.floor((W - COLS * CS) / 2);
  OY = 50 + Math.floor((H - 50 - ROWS * CS) / 2);
  var mmS = Math.max(60, Math.min(100, Math.floor(Math.min(W,H) * 0.13)));
  mmCv.width = mmS; mmCv.height = Math.round(mmS * ROWS / COLS);
  mmCv.style.width = mmCv.width + 'px'; mmCv.style.height = mmCv.height + 'px';
}

/* ===================== 08 MAZE GENERATION ===================== */
function genMaze() {
  maze = [];
  for (var y = 0; y < ROWS; y++) maze[y] = new Uint8Array(COLS).fill(1);
  // Recursive backtracker iterativo (evita stack overflow en mazes grandes)
  var stack = [[1,1]]; maze[1][1] = 0;
  while (stack.length) {
    var cur = stack[stack.length-1], x = cur[0], y = cur[1];
    var dirs = [[0,-2],[2,0],[0,2],[-2,0]].sort(function(){return Math.random()-0.5;});
    var moved = false;
    for (var i = 0; i < dirs.length; i++) {
      var nx = x + dirs[i][0], ny = y + dirs[i][1];
      if (nx > 0 && nx < COLS-1 && ny > 0 && ny < ROWS-1 && maze[ny][nx] === 1) {
        maze[y + dirs[i][1]/2][x + dirs[i][0]/2] = 0;
        maze[ny][nx] = 0; stack.push([nx, ny]); moved = true; break;
      }
    }
    if (!moved) stack.pop();
  }
  for (var y2 = 0; y2 < ROWS; y2++) { maze[y2][0] = 1; maze[y2][COLS-1] = 1; }
  for (var x2 = 0; x2 < COLS; x2++) { maze[0][x2] = 1; maze[ROWS-1][x2] = 1; }
  // Rutas alternativas (más en niveles altos)
  var extra = Math.floor((COLS * ROWS) * (0.01 + level * 0.003));
  for (var i2 = 0; i2 < extra; i2++) {
    var ex = 1 + Math.floor(Math.random() * (COLS-2));
    var ey = 1 + Math.floor(Math.random() * (ROWS-2));
    if (ex > 0 && ex < COLS-1 && ey > 0 && ey < ROWS-1) maze[ey][ex] = 0;
  }
}
function isWall(x, y) { return x < 0 || y < 0 || x >= COLS || y >= ROWS || maze[y][x] === 1; }
function cellAt(c){ return (c + 0.5) * CS; }                 // centro de celda (local)
function panOfCell(cx){ return canvas.width ? ((OX + cellAt(cx)) / canvas.width * 2 - 1) : 0; }

/* ===================== 09 EXIT LOGIC ===================== */
function placeExit() {
  exitX = COLS - 2; exitY = ROWS - 2;
  maze[exitY][exitX] = 0;
  exitMark.style.display = 'block';
  updateExitMarker();
}
function updateExitMarker() {
  if (exitX < 0 || gstate !== 'playing') { exitMark.style.display = 'none'; return; }
  exitMark.style.left = (OX + exitX * CS + CS/2) + 'px';
  exitMark.style.top  = (OY + exitY * CS + CS/2) + 'px';
}
// % de dots recogidos por el equipo
function dotsPct(){
  if (!dots.length) return 1;
  var got = 0; for (var i=0;i<dots.length;i++) if (dots[i].collected) got++;
  return got / dots.length;
}

/* ===================== 10 PLACEMENT ===================== */
function isPlayerCell(x, y){
  for (var i=0;i<players.length;i++) if (players[i].gx===x && players[i].gy===y) return true;
  return false;
}
function placeDots() {
  dots = [];
  for (var y = 1; y < ROWS-1; y++) {
    for (var x = 1; x < COLS-1; x++) {
      if (maze[y][x] === 0 && !isPlayerCell(x,y) && !(x === exitX && y === exitY)) {
        var r = Math.random();
        var type = r < 0.05 ? 'crystal' : r < 0.20 ? 'big' : 'normal';
        dots.push({ x:x, y:y, type:type, collected:false, anim:Math.random()*Math.PI*2 });
      }
    }
  }
}
function placePups() {
  pups = [];
  var count = Math.min(3 + Math.floor(level * 0.7), 10);
  var placed = 0, tries = 0;
  while (placed < count && tries < 1000) {
    tries++;
    var x = 1 + Math.floor(Math.random() * (COLS-2));
    var y = 1 + Math.floor(Math.random() * (ROWS-2));
    var ok = maze[y][x] === 0 && Math.hypot(x-1,y-1) > 5 &&
             !pups.some(function(p){ return p.x===x && p.y===y; });
    if (ok) {
      var type = PU_TYPES[Math.floor(Math.random() * PU_TYPES.length)];
      pups.push({ x:x, y:y, type:type, col:PU_COLORS[type], anim:Math.random()*Math.PI*2, alive:true });
      placed++;
    }
  }
}
function spawnGhosts() {
  ghosts = [];
  var count = Math.min(2 + level, 8);
  var typeKeys = Object.keys(GHOST_DEFS);
  var cands = [
    [COLS-2,1],[COLS-2,ROWS-4],[1,ROWS-2],
    [Math.floor(COLS/2),ROWS-2],[COLS-2,Math.floor(ROWS/2)],
    [Math.floor(COLS/2),1],[1,Math.floor(ROWS/2)],
    [Math.floor(COLS*3/4),Math.floor(ROWS/2)]
  ];
  for (var i = 0; i < count; i++) {
    var gx = cands[i % cands.length][0];
    var gy = cands[i % cands.length][1];
    if (isWall(gx,gy)) {
      for (var dy2=-1;dy2<=1;dy2++) for (var dx2=-1;dx2<=1;dx2++)
        if (!isWall(gx+dx2,gy+dy2)) { gx+=dx2; gy+=dy2; dy2=2; break; }
    }
    maze[gy][gx] = 0;
    var tk = typeKeys[i % typeKeys.length];
    var gt = GHOST_DEFS[tk];
    ghosts.push({
      gx:gx, gy:gy, wx: OX+(gx+0.5)*CS, wy: OY+(gy+0.5)*CS,
      dir:2, mTimer:0, typeKey:tk, name:gt.name, color:gt.color, ai:gt.ai,
      scared:false, speed: gt.speed + level * 0.06,
      id:i, lastSeen:null, ct:null, ox:0, oy:0
    });
  }
}

/* ===================== 11 GAME CTRL ===================== */
function mkPlayer(i) {
  var ch = i === 0 ? charById(profile.avatar) : autoChar(i);
  var nm = i === 0 ? (profile.name || 'Explorer') : ('J'+(i+1));
  var sp = SPAWNS[i] || [1,1];
  return {
    idx:i, char:ch, name:nm,
    gx:sp[0], gy:sp[1], wx:cellAt(sp[0]), wy:cellAt(sp[1]), tx:cellAt(sp[0]), ty:cellAt(sp[1]),
    dir:1, moving:false, inv:0, trail:[], moveQ:-1, moveRepeat:0,
    combo:1, comboTimer:0, puSpeed:0, puGhost:0, puMagnet:0, puFreeze:0,
    state:'alive', reviveT:0, spawnX:sp[0], spawnY:sp[1]
  };
}
// personaje automático para J2-J4 (distinto al del J1 cuando es posible)
function autoChar(i){
  var pool = CHARS.slice();
  var pick = pool[i % pool.length];
  return pick;
}
function buildPlayers() {
  players = [];
  for (var i = 0; i < numPlayers; i++) players.push(mkPlayer(i));
}
function resyncPlayerVisuals(){
  players.forEach(function(p){
    var sp = SPAWNS[p.idx] || [1,1];
    p.gx = sp[0]; p.gy = sp[1];
    p.wx = cellAt(p.gx); p.wy = cellAt(p.gy);
    p.tx = p.wx; p.ty = p.wy; p.dir = 1; p.moving = false;
    p.inv = 0; p.trail = []; p.moveQ = -1; p.moveRepeat = 0;
    p.combo = 1; p.comboTimer = 0;
    p.puSpeed = p.puGhost = p.puMagnet = p.puFreeze = 0;
    p.state = 'alive'; p.reviveT = 0; p.spawnX = sp[0]; p.spawnY = sp[1];
  });
}

function initLevel() {
  genMaze();
  // garantizar suelo en spawns
  SPAWNS.forEach(function(s){ if (maze[s[1]] && maze[s[1]][s[0]]!==undefined) maze[s[1]][s[0]] = 0; });
  placeExit();
  resyncPlayerVisuals();
  particles = [];
  ghostScared = 0; globalFreeze = 0;
  spawnGhosts(); placeDots(); placePups();
  initMission();
  updateExitMarker();
}

function startGame() {
  var ni = document.getElementById('playerName');
  profile.name = (ni && ni.value.trim()) || profile.name || 'Explorer';
  saveProfile();
  buildPlayers();
  // HUD con personaje del J1
  hEmoji.textContent = players[0].char.emoji;
  hName.textContent  = players[0].name;
  document.getElementById('pChar').textContent = players[0].char.emoji;
  document.getElementById('pName').textContent  = players[0].name;
  score = 0; level = 1; coins = 0;
  lives = CFG.BASE_LIVES + (numPlayers - 1);
  SS = { deaths:0, ghostsEaten:0, pupsGot:0, maxCombo:1, revives:0, startTime:Date.now() };
  bestScore = Math.max(bestScore, 0);
  SFX.init(); SFX.resume();
  resize(); initLevel();
  gstate = 'playing';
  hideScreen();
  showBanner(numPlayers>1 ? '¡Coop x'+numPlayers+'!' : '¡Explorar!', 2000);
}

function nextLevel() {
  level++;
  var bonus = CFG.LVL_BONUS * level;
  score += bonus;
  popScore(OX+cellAt(players[0].gx), OY+cellAt(players[0].gy), '+'+bonus.toLocaleString(), '#00ff88', 28);
  SFX.win();
  exitMark.style.display = 'none';
  if (level > CFG.MAX_LVL) { gstate = 'win'; saveRecord(); showEnd('win'); return; }
  resize(); initLevel();
  showBanner('NIVEL ' + level, 1800);
}

// Un jugador es atrapado: pasa a "caído". El wipe de equipo se decide en el loop.
function onCaught(p) {
  if (p.state !== 'alive' || p.inv > 0) return;
  p.state = 'downed'; p.reviveT = 0; p.moveQ = -1; p.moving = false;
  emit(OX+p.wx, OY+p.wy, '#ff3355', 22, 110, 4.5);
  setFlash(255, 30, 50, 0.35); screenShake = 16; SFX.die();
  if (numPlayers > 1) showToast(p.name + ' cayó — ¡revívelo!');
  updateHUD();
}

// Revivir: si un compañero vivo está junto a un caído, carga la barra
function processRevives() {
  if (numPlayers < 2) return;
  players.forEach(function(p){
    if (p.state !== 'downed') return;
    var helper = players.some(function(o){
      return o.state==='alive' && Math.hypot(o.gx-p.gx, o.gy-p.gy) < 1.3;
    });
    if (helper) {
      p.reviveT += dt;
      if (p.reviveT >= CFG.REVIVE_T) {
        p.state = 'alive'; p.reviveT = 0; p.inv = 2.0;
        SS.revives++; SFX.revive(panOfCell(p.gx));
        emit(OX+p.wx, OY+p.wy, '#00ff88', 18, 90, 4);
        showToast('💚 ' + p.name + ' revivido!');
      }
    } else { p.reviveT = Math.max(0, p.reviveT - dt*0.5); }
  });
}

// Wipe de equipo: nadie vivo y no todos escaparon -> pierde vida, respawn de caídos
function teamWipe() {
  lives--; SS.deaths++;
  setFlash(255, 20, 40, 0.5); screenShake = 22; SFX.die();
  players.forEach(function(p){
    if (p.state === 'downed') {
      p.state = 'alive'; p.gx = p.spawnX; p.gy = p.spawnY;
      p.wx = cellAt(p.gx); p.wy = cellAt(p.gy); p.tx = p.wx; p.ty = p.wy;
      p.inv = 3.0 * (p.char.bonus.invDur || 1); p.reviveT = 0; p.combo = 1;
    }
  });
  updateHUD();
  if (lives <= 0) setTimeout(function(){ gstate='gameover'; saveRecord(); showEnd('gameover'); }, 700);
}

/* ===================== 12 MOVEMENT ===================== */
function tryMove(p, dir) {
  if (dir < 0 || p.state !== 'alive') return false;
  var nx = p.gx + DX[dir], ny = p.gy + DY[dir];
  if (isWall(nx, ny)) return false;
  p.gx = nx; p.gy = ny;
  p.tx = cellAt(nx); p.ty = cellAt(ny);
  p.dir = dir; p.moving = true;
  SFX.step();
  return true;
}

/* ===================== 13 GHOST TICK ===================== */
function nearestPlayer(g) {
  var best = null, bd = Infinity;
  players.forEach(function(p){
    if (p.state !== 'alive') return;
    var d = Math.hypot(p.gx-g.gx, p.gy-g.gy);
    if (d < bd) { bd = d; best = p; }
  });
  return best;
}
function ghostTick(g, dtg) {
  var tx = OX + cellAt(g.gx), ty = OY + cellAt(g.gy);
  var ls = globalFreeze > 0 ? 0.8 : Math.min(1, dtg * (g.scared ? 4 : 7));
  g.wx += (tx - g.wx) * ls;
  g.wy += (ty - g.wy) * ls;
  if (globalFreeze > 0) return;

  g.mTimer -= dtg;
  if (g.mTimer > 0) return;
  var spd = g.scared ? 0.32 : g.speed;
  g.mTimer = 1 / spd;

  var tp = nearestPlayer(g);
  var bestDir;
  if (g.scared && tp) {
    var bd = aiChase(g, tp.gx, tp.gy, 2.5);
    var aw = (bd + 2) % 4;
    var anx = g.gx + DX[aw], any = g.gy + DY[aw];
    bestDir = !isWall(anx, any) ? aw : bd;
  } else if (tp) {
    bestDir = g.ai ? g.ai(g, tp) : aiChase(g, tp.gx, tp.gy, 1.2);
  } else {
    bestDir = aiWander(g);
  }
  if (bestDir >= 0) {
    var mx = g.gx + DX[bestDir], my = g.gy + DY[bestDir];
    if (!isWall(mx, my)) { g.gx = mx; g.gy = my; g.dir = bestDir; }
  }
}

/* ===================== 14 COLLISION ===================== */
function checkCollisions() {
  players.forEach(function(p){
    if (p.state !== 'alive') return;
    // Monedas
    for (var i = 0; i < dots.length; i++) {
      var d = dots[i];
      if (!d.collected && Math.abs(d.x - p.gx) < 0.85 && Math.abs(d.y - p.gy) < 0.85) collectDot(p, d);
    }
    // Power-ups
    for (var j = 0; j < pups.length; j++) {
      var pu = pups[j];
      if (pu.alive && pu.x === p.gx && pu.y === p.gy) collectPup(p, pu);
    }
    // Salida
    if (p.gx === exitX && p.gy === exitY) {
      if (dotsPct() >= 0.5) {
        p.state = 'escaped'; SFX.exit();
        emit(OX+p.wx, OY+p.wy, '#ffd700', 18, 90, 4);
        showToast(p.name + ' escapó! ' + escapedCount()+'/'+players.length);
      } else if (frameN % 40 === 0) {
        showToast('Recojan más monedas antes de salir!');
      }
    }
    // Fantasmas
    if (p.inv > 0) return;
    for (var k = 0; k < ghosts.length; k++) {
      var g = ghosts[k];
      if (Math.hypot(g.gx - p.gx, g.gy - p.gy) < 1.0) {
        if (g.scared || p.puGhost > 0) eatGhost(p, g);
        else { respawnGhost(g); onCaught(p); break; }
      }
    }
  });
}
function escapedCount(){ var n=0; players.forEach(function(p){ if(p.state==='escaped') n++; }); return n; }

function collectDot(p, d) {
  d.collected = true;
  p.combo++;
  p.comboTimer = CFG.COMBO_T * (p.char.bonus.comboT || 1);
  SS.maxCombo = Math.max(SS.maxCombo, p.combo);
  var mult = p.char.bonus.dotMult || 1;
  var base = d.type==='crystal' ? CFG.CRYS_PTS : d.type==='big' ? CFG.BIG_PTS : CFG.DOT_PTS;
  var pts = Math.round(base * p.combo * mult);
  score += pts; coins++;
  var pan = panOfCell(d.x);
  var col = d.type==='crystal' ? '#0ff' : d.type==='big' ? '#ffd700' : '#ffaa00';
  emit(OX+cellAt(d.x), OY+cellAt(d.y), col, d.type==='crystal'?8:4, 50, d.type==='crystal'?3:2);
  if (d.type === 'big') SFX.bigCoin(pan); else SFX.coin(pan);
  if (p.combo >= 3) {
    showComboPop(p.combo);
    popScore(OX+cellAt(d.x), OY+cellAt(d.y)-10, '+'+pts, col, 14);
    SFX.combo(p.combo);
  }
  updateMission('coin', 1);
}

function collectPup(p, pu) {
  pu.alive = false; SS.pupsGot++;
  var pan = panOfCell(pu.x);
  emit(OX+cellAt(pu.x), OY+cellAt(pu.y), pu.col, 18, 90, 4);
  screenShake = 10;
  var pts = Math.round(CFG.PUP_PTS * p.combo);
  score += pts;
  popScore(OX+cellAt(pu.x), OY+cellAt(pu.y)-10, '+'+pts, pu.col, 18);
  var lbs = {speed:'⚡ VELOCIDAD',ghost:'👻 MODO FANTASMA',magnet:'🧲 IMAN',freeze:'❄ CONGELADO'};
  showToast(p.name + ': ' + (lbs[pu.type] || pu.type));
  var dm = p.char.bonus.pupDur || 1;
  var dur = (CFG.PU_DUR + level * 0.4) * dm;
  if (pu.type==='speed') p.puSpeed = dur;
  if (pu.type==='magnet') p.puMagnet = dur;
  if (pu.type==='ghost') { p.puGhost = dur; ghostScared = Math.max(ghostScared, dur); ghosts.forEach(function(g){g.scared=true;}); }
  if (pu.type==='freeze') { p.puFreeze = dur; globalFreeze = Math.max(globalFreeze, dur); }
  SFX.pup(pan);
  updateMission('pup', 1);
}

function eatGhost(p, g) {
  SS.ghostsEaten++;
  var pts = Math.round(CFG.GHOST_PTS * p.combo);
  score += pts; p.combo++; p.comboTimer = 3;
  popScore(g.wx, g.wy, '+'+pts, g.color, 22);
  emit(g.wx, g.wy, g.color, 18, 100, 5);
  emit(g.wx, g.wy, '#fff', 8, 55, 3);
  screenShake = 8; setFlash(150, 0, 200, 0.22);
  SFX.eatGhost(panOfCell(g.gx)); respawnGhost(g); showComboPop(p.combo);
  updateMission('ghost', 1);
}

function respawnGhost(g) {
  g.gx = COLS-2; g.gy = 1;
  g.wx = OX+cellAt(COLS-2); g.wy = OY+cellAt(1);
  g.scared = false; g.lastSeen = null;
}

/* ===================== 15 MAGNET ===================== */
function applyMagnet() {
  players.forEach(function(p){
    if (p.puMagnet <= 0 || p.state !== 'alive') return;
    var range = 6 * (p.char.bonus.magnetRange || 1);
    for (var i = 0; i < dots.length; i++) {
      var d = dots[i]; if (d.collected) continue;
      var mx = d.x-p.gx, my = d.y-p.gy, dist = Math.hypot(mx, my);
      if (dist < range && dist > 0.4) {
        d.x -= mx * 0.12 * dt; d.y -= my * 0.12 * dt;
        if (dist < 0.6) collectDot(p, d);
      }
    }
  });
}

/* ===================== 16 PARTICLES ===================== */
function emit(x, y, col, n, spd, sz) {
  for (var i = 0; i < n; i++) {
    var a = Math.random() * Math.PI * 2;
    var s = (0.4 + Math.random() * 0.6) * spd;
    particles.push({ x:x, y:y, vx:Math.cos(a)*s, vy:Math.sin(a)*s, col:col, life:1, sz:sz||3 });
  }
}
function setFlash(r, g2, b, a) {
  flashEl.style.background = 'rgba('+r+','+g2+','+b+','+a+')';
  flashEl.style.opacity = '1'; flashTimer = 0.35;
}

/* ===================== 17 DRAW ===================== */
function drawBg() { ctx.fillStyle = '#05080e'; ctx.fillRect(0, 0, canvas.width, canvas.height); }
function wallTheme() {
  var t = [
    {wall:'#0c1929', accent:'rgba(0,160,255,'},
    {wall:'#1a0d2e', accent:'rgba(180,0,255,'},
    {wall:'#1a1400', accent:'rgba(255,160,0,'},
    {wall:'#0a1a0a', accent:'rgba(0,200,80,'},
    {wall:'#1a0808', accent:'rgba(255,50,50,'},
  ];
  return t[Math.min(Math.floor((level-1)/3), t.length-1)];
}
function drawMaze() {
  var pulse = 0.08 + 0.03 * Math.sin(frameN * 0.012);
  var th = wallTheme();
  for (var y = 0; y < ROWS; y++) {
    for (var x = 0; x < COLS; x++) {
      var bx = OX+x*CS, by = OY+y*CS;
      if (maze[y][x] === 1) {
        ctx.fillStyle = th.wall; ctx.fillRect(bx, by, CS, CS);
        var nb = [[0,-1],[1,0],[0,1],[-1,0]];
        for (var n = 0; n < 4; n++) {
          var nx = x+nb[n][0], ny = y+nb[n][1];
          if (!isWall(nx, ny)) {
            ctx.fillStyle = th.accent + pulse + ')';
            var ex2 = bx + (nb[n][0]===1?CS-2:0);
            var ey2 = by + (nb[n][1]===1?CS-2:0);
            ctx.fillRect(ex2, ey2, nb[n][0]!==0?2:CS, nb[n][1]!==0?2:CS);
          }
        }
        ctx.strokeStyle = 'rgba(20,50,90,0.5)'; ctx.lineWidth = 0.5;
        ctx.strokeRect(bx+0.5, by+0.5, CS-1, CS-1);
      } else if (x === exitX && y === exitY) {
        var ep = 0.6 + 0.4 * Math.sin(frameN * 0.06);
        ctx.fillStyle = 'rgba(30,20,0,1)'; ctx.fillRect(bx, by, CS, CS);
        ctx.fillStyle = 'rgba(255,215,0,' + (ep * 0.3) + ')'; ctx.fillRect(bx, by, CS, CS);
        ctx.strokeStyle = 'rgba(255,215,0,' + ep + ')'; ctx.lineWidth = 2; ctx.strokeRect(bx+1, by+1, CS-2, CS-2);
      } else {
        ctx.fillStyle = '#060c14'; ctx.fillRect(bx, by, CS, CS);
        if (CS >= 20) {
          ctx.fillStyle = 'rgba(0,40,70,0.18)';
          ctx.fillRect(bx, by, CS, 1); ctx.fillRect(bx, by, 1, CS);
        }
      }
    }
  }
}
function drawDots() {
  for (var i = 0; i < dots.length; i++) {
    var d = dots[i]; if (d.collected) continue;
    d.anim += dt * 1.6;
    var dx2 = OX+cellAt(d.x), dy2 = OY+cellAt(d.y);
    var p2 = 0.85 + 0.15 * Math.sin(d.anim);
    var r = CS * (d.type==='crystal'?0.18:d.type==='big'?0.14:0.11) * p2;
    var col = d.type==='crystal'?'#0ff':d.type==='big'?'#ffd700':'#ffaa00';
    ctx.beginPath(); ctx.arc(dx2, dy2, r*2.4, 0, Math.PI*2);
    ctx.fillStyle = d.type==='crystal'?'rgba(0,255,255,'+(0.15*p2)+')':
                    d.type==='big'?'rgba(255,215,0,'+(0.1*p2)+')':
                    'rgba(255,170,0,'+(0.08*p2)+')';
    ctx.fill();
    ctx.beginPath(); ctx.arc(dx2, dy2, r, 0, Math.PI*2); ctx.fillStyle = col; ctx.fill();
    if (d.type === 'crystal') {
      ctx.beginPath(); ctx.arc(dx2-r*0.3, dy2-r*0.3, r*0.3, 0, Math.PI*2);
      ctx.fillStyle = 'rgba(255,255,255,0.6)'; ctx.fill();
    }
  }
}
function drawPups() {
  for (var i = 0; i < pups.length; i++) {
    var p = pups[i]; if (!p.alive) continue;
    p.anim += dt * 2.2;
    var ppx = OX+cellAt(p.x), ppy = OY+cellAt(p.y);
    var sc = 0.88 + 0.14*Math.sin(p.anim), r = CS*0.32*sc;
    ctx.beginPath(); ctx.arc(ppx, ppy, r*1.8, 0, Math.PI*2); ctx.fillStyle = p.col+'18'; ctx.fill();
    ctx.strokeStyle = p.col+'44'; ctx.lineWidth=1; ctx.stroke();
    ctx.save(); ctx.translate(ppx, ppy); ctx.rotate(p.anim*0.3);
    ctx.beginPath();
    for (var k = 0; k < 8; k++) {
      var a = k*Math.PI/4, rv = k%2===0?r:r*0.5;
      k===0 ? ctx.moveTo(Math.cos(a)*rv, Math.sin(a)*rv) : ctx.lineTo(Math.cos(a)*rv, Math.sin(a)*rv);
    }
    ctx.closePath();
    ctx.fillStyle = p.col+'55'; ctx.fill(); ctx.strokeStyle = p.col; ctx.lineWidth=1.5; ctx.stroke();
    ctx.restore();
    if (CS >= 20) {
      ctx.font = Math.round(CS*0.42) + 'px serif';
      ctx.textAlign = 'center'; ctx.textBaseline = 'middle';
      ctx.fillText(PU_ICONS[p.type]||'?', ppx, ppy);
    }
  }
}
function drawGhosts() {
  for (var i = 0; i < ghosts.length; i++) {
    var g = ghosts[i];
    var scared = g.scared && ghostScared > 0;
    var flick = scared && ghostScared < 2 && Math.floor(frameN/5)%2===0;
    var gcol = flick ? '#8888ff' : scared ? '#4444cc' : g.color;
    var bob = Math.sin(frameN*0.08 + g.id*1.3) * 2.5, r = CS*0.43;
    ctx.save(); ctx.translate(g.wx, g.wy+bob);
    ctx.beginPath(); ctx.arc(0, -r*0.3, r, Math.PI, 0);
    var ws = 3, wSt = (r*2)/ws;
    for (var k = 0; k <= ws; k++) {
      var wx2 = -r+k*wSt, wy2 = r*0.68+(k%2===0?r*0.28:-r*0.28);
      if (k===0) ctx.lineTo(wx2, wy2);
      else ctx.quadraticCurveTo(wx2-wSt*0.5, wy2+(k%2===0?-r*0.25:r*0.25), wx2, wy2);
    }
    ctx.closePath(); ctx.fillStyle = gcol+'cc'; ctx.fill();
    ctx.strokeStyle = gcol; ctx.lineWidth=1.2; ctx.stroke();
    ctx.beginPath(); ctx.arc(0, 0, r*1.35, 0, Math.PI*2); ctx.fillStyle = gcol+'16'; ctx.fill();
    if (!scared) {
      var eo = r*0.24, ey3 = -r*0.2;
      var lx = DX[g.dir]*r*0.08, ly = DY[g.dir]*r*0.08;
      [-eo, eo].forEach(function(ex3) {
        ctx.fillStyle='#fff'; ctx.beginPath(); ctx.ellipse(ex3,ey3,r*0.18,r*0.23,0,0,Math.PI*2); ctx.fill();
        ctx.fillStyle='#001'; ctx.beginPath(); ctx.arc(ex3+lx,ey3+ly,r*0.1,0,Math.PI*2); ctx.fill();
      });
    } else {
      ctx.fillStyle='#aaccff'; ctx.font=Math.round(r*0.82)+'px Arial';
      ctx.textAlign='center'; ctx.textBaseline='middle'; ctx.fillText('?',0,-r*0.15);
    }
    ctx.restore();
  }
}
function drawPlayer(p) {
  if (p.state === 'escaped') return; // ya salió del laberinto
  var ppx = OX+p.wx, ppy = OY+p.wy;
  if (p.state === 'downed') {
    // jugador caído: marcador translúcido + barra de revivir
    var pr = CS*0.4;
    ctx.globalAlpha = 0.5 + 0.2*Math.sin(frameN*0.18);
    ctx.font = Math.round(pr*1.6)+'px serif'; ctx.textAlign='center'; ctx.textBaseline='middle';
    ctx.fillText('💫', ppx, ppy); ctx.globalAlpha = 1;
    if (p.reviveT > 0) {
      ctx.beginPath(); ctx.arc(ppx, ppy, pr*1.2, -Math.PI/2, -Math.PI/2 + Math.PI*2*(p.reviveT/CFG.REVIVE_T));
      ctx.strokeStyle = '#00ff88'; ctx.lineWidth = 3; ctx.stroke();
    }
    return;
  }
  // Rastro
  p.trail.push({x:ppx,y:ppy,l:1});
  if (p.trail.length > 18) p.trail.shift();
  var tc = p.puGhost>0 ? 'rgba(255,0,255,' : p.char.trail;
  for (var i = 0; i < p.trail.length; i++) {
    var t = p.trail[i]; t.l -= dt*3.2; if (t.l<=0) continue;
    var al = t.l*(i/p.trail.length)*0.5, tr = CS*(0.07+0.09*(i/p.trail.length));
    ctx.beginPath(); ctx.arc(t.x, t.y, tr, 0, Math.PI*2); ctx.fillStyle = tc+al+')'; ctx.fill();
  }
  if (p.inv > 0 && Math.floor(frameN/5)%2===1) return;
  var r = CS*0.41, pcol = p.puGhost>0 ? '#ff00ff' : p.char.color;
  ctx.beginPath(); ctx.arc(ppx,ppy,r*1.65,0,Math.PI*2); ctx.fillStyle=pcol+'14'; ctx.fill();
  ctx.beginPath(); ctx.arc(ppx,ppy,r*1.28,0,Math.PI*2); ctx.fillStyle=pcol+'26'; ctx.fill();
  ctx.beginPath(); ctx.arc(ppx,ppy,r,0,Math.PI*2); ctx.fillStyle=pcol+'ee'; ctx.fill();
  ctx.strokeStyle=pcol; ctx.lineWidth=2; ctx.stroke();
  if (CS >= 24) {
    ctx.font = Math.round(r*1.5)+'px serif'; ctx.textAlign='center'; ctx.textBaseline='middle';
    ctx.fillText(p.char.emoji, ppx, ppy);
  } else {
    var er=r*0.2, eo2=r*0.22, ef=r*0.22; ctx.fillStyle='#001';
    ctx.beginPath(); ctx.arc(ppx+DX[p.dir]*ef-DY[p.dir]*eo2,ppy+DY[p.dir]*ef+DX[p.dir]*eo2,er,0,Math.PI*2); ctx.fill();
    ctx.beginPath(); ctx.arc(ppx+DX[p.dir]*ef+DY[p.dir]*eo2,ppy+DY[p.dir]*ef-DX[p.dir]*eo2,er,0,Math.PI*2); ctx.fill();
  }
  // etiqueta de número de jugador en coop
  if (numPlayers > 1) {
    ctx.font = '700 '+Math.round(CS*0.3)+'px '+'Orbitron, monospace';
    ctx.fillStyle = '#fff'; ctx.textAlign='center'; ctx.textBaseline='middle';
    ctx.fillText('P'+(p.idx+1), ppx, ppy - r - CS*0.25);
  }
  if (p.puSpeed > 0) {
    for (var j = 0; j < 4; j++) {
      var a = frameN*0.15 + j*Math.PI/2;
      var alpha2 = Math.floor((0.5+0.5*Math.sin(frameN*0.2+j))*255).toString(16).padStart(2,'0');
      ctx.strokeStyle = '#0af'+alpha2; ctx.lineWidth=2;
      ctx.beginPath();
      ctx.moveTo(ppx+Math.cos(a)*(r+3), ppy+Math.sin(a)*(r+3));
      ctx.lineTo(ppx+Math.cos(a)*(r+9), ppy+Math.sin(a)*(r+9));
      ctx.stroke();
    }
  }
}
function drawParticles() {
  for (var i = particles.length-1; i >= 0; i--) {
    var p = particles[i];
    p.x += p.vx*dt; p.y += p.vy*dt; p.vy += 110*dt; p.life -= dt*1.7;
    if (p.life <= 0) { particles.splice(i,1); continue; }
    var alpha3 = Math.floor(p.life*255).toString(16).padStart(2,'0');
    ctx.beginPath(); ctx.arc(p.x,p.y,p.sz*p.life,0,Math.PI*2); ctx.fillStyle = p.col+alpha3; ctx.fill();
  }
}

/* ===================== 18 MINIMAP ===================== */
function drawMinimap() {
  if (!mmCv.width || !mmCv.height) return;
  var mW=mmCv.width, mH=mmCv.height, cw=mW/COLS, ch=mH/ROWS;
  mctx.fillStyle='#060c14'; mctx.fillRect(0,0,mW,mH);
  for (var y=0;y<ROWS;y++) for (var x=0;x<COLS;x++)
    if (maze[y][x]===1) { mctx.fillStyle='#0d1b2e'; mctx.fillRect(x*cw,y*ch,cw,ch); }
  if (exitX >= 0) { mctx.fillStyle='rgba(255,215,0,.7)'; mctx.fillRect(exitX*cw-1,exitY*ch-1,cw+2,ch+2); }
  mctx.fillStyle='#ffd70066';
  dots.forEach(function(d){if(!d.collected) mctx.fillRect(d.x*cw,d.y*ch,cw*0.6,ch*0.6);});
  pups.forEach(function(p){if(p.alive){mctx.fillStyle=p.col+'bb';mctx.fillRect(p.x*cw-1,p.y*ch-1,cw*0.8+2,ch*0.8+2);}});
  ghosts.forEach(function(g){mctx.fillStyle=g.scared?'#4444ccbb':g.color+'dd';mctx.fillRect(g.gx*cw,g.gy*ch,cw,ch);});
  if (Math.floor(frameN/8)%2===0) {
    players.forEach(function(p){
      if (p.state==='escaped') return;
      mctx.fillStyle = p.state==='downed' ? '#ff3355' : p.char.color;
      mctx.fillRect(p.gx*cw-1,p.gy*ch-1,cw+2,ch+2);
    });
  }
}

/* ===================== 19 MAIN LOOP ===================== */
function loop(ts) {
  requestAnimationFrame(loop);
  dt = Math.min((ts - lastTS) / 1000, 0.05);
  lastTS = ts; frameN++;
  if (gstate !== 'playing') return;

  // -- UPDATE players --
  players.forEach(function(p){
    // interpolación visual
    var csm = p.char.bonus.speed || 1;
    var ms = (p.puSpeed>0?13:9) * csm;
    var lt = Math.min(1, dt*ms);
    p.wx += (p.tx-p.wx)*lt; p.wy += (p.ty-p.wy)*lt;
    if (Math.hypot(p.tx-p.wx, p.ty-p.wy) < 0.4) { p.wx=p.tx; p.wy=p.ty; p.moving=false; }
    // auto-repetición
    p.moveRepeat += dt;
    var rd = (p.puSpeed>0?0.055:0.085) / csm;
    if (p.state==='alive') {
      if (!p.moving && p.moveRepeat >= rd) { p.moveRepeat=0; tryMove(p, p.moveQ); }
      else if (p.moving && p.moveQ >= 0) { tryMove(p, p.moveQ); }
    }
    // timers por jugador
    p.inv=Math.max(0,p.inv-dt); p.puSpeed=Math.max(0,p.puSpeed-dt);
    p.puGhost=Math.max(0,p.puGhost-dt); p.puMagnet=Math.max(0,p.puMagnet-dt);
    p.puFreeze=Math.max(0,p.puFreeze-dt);
    if (p.comboTimer>0){ p.comboTimer-=dt; } else if (p.combo>1) { p.combo=1; }
  });

  // timers globales
  ghostScared=Math.max(0,ghostScared-dt); if (ghostScared<=0) ghosts.forEach(function(g){g.scared=false;});
  globalFreeze=Math.max(0,globalFreeze-dt);
  if (flashTimer>0){ flashTimer-=dt; if(flashTimer<=0) flashEl.style.opacity='0'; }

  // IA fantasmas
  ghosts.forEach(function(g){ ghostTick(g,dt); });

  // distancia mínima a fantasma (peligro + tensión/paneo)
  var nearDist = Infinity, nearGhost = null;
  ghosts.forEach(function(g){
    if (g.scared) return;
    players.forEach(function(p){
      if (p.state!=='alive') return;
      var dd = Math.hypot(g.gx-p.gx, g.gy-p.gy);
      if (dd < nearDist) { nearDist = dd; nearGhost = g; }
    });
  });
  if (nearDist<3.5) dangerEl.classList.add('near'); else dangerEl.classList.remove('near');
  if (frameN%30===0) SFX.setTension(nearDist);

  applyMagnet(); checkCollisions(); processRevives();

  // resolución cooperativa de fin de nivel / wipe
  var aliveN=0, escapedN=0;
  players.forEach(function(p){ if(p.state==='alive') aliveN++; else if(p.state==='escaped') escapedN++; });
  if (escapedN === players.length) { nextLevel(); }
  else if (aliveN === 0) { teamWipe(); }

  updateExitMarker();

  // screen shake
  var sx=0, sy=0;
  if (screenShake>0){ sx=(Math.random()-.5)*screenShake; sy=(Math.random()-.5)*screenShake; screenShake*=0.8; if(screenShake<0.4) screenShake=0; }

  // -- DRAW --
  ctx.save();
  if (sx||sy) ctx.translate(sx, sy);
  drawBg(); drawMaze(); drawDots(); drawPups(); drawGhosts();
  players.forEach(drawPlayer);
  drawParticles();
  ctx.restore();
  drawMinimap(); updateHUD();
}

/* ===================== 20 INPUT ===================== */
function isTypingTarget(e) {
  var t = e.target;
  if (!t) return false;
  var tag = (t.tagName || '').toLowerCase();
  return tag === 'input' || tag === 'textarea' || tag === 'select' || t.isContentEditable;
}
document.addEventListener('keydown', function(e) {
  // No capturar teclas mientras se escribe en campos de texto (nombre, correo)
  if (isTypingTarget(e)) return;
  var m = KEYS[e.key];
  if (m && gstate==='playing') {
    var p = players[m[0]];
    if (p) { p.moveQ = m[1]; p.moveRepeat = 99; }
    e.preventDefault();
  }
  if (e.key==='Enter'||e.key===' ') { if(gstate==='menu') document.getElementById('btnPlay').click(); }
  if (e.key==='Escape'||e.key==='p'||e.key==='P') { togglePause(); e.preventDefault(); }
});
document.addEventListener('keyup', function(e) {
  if (isTypingTarget(e)) return;
  var m = KEYS[e.key];
  if (m) { var p = players[m[0]]; if (p && p.moveQ===m[1]) p.moveQ=-1; }
});

// D-Pad táctil -> Jugador 1
function setupDpad() {
  var map = {dU:0,dR:1,dD:2,dL:3};
  Object.keys(map).forEach(function(id) {
    var el = document.getElementById(id); if (!el) return;
    var dir = map[id];
    el.addEventListener('pointerdown',function(e){e.preventDefault(); if(players[0]){players[0].moveQ=dir;players[0].moveRepeat=99;} el.classList.add('dn');});
    el.addEventListener('pointerup',function(e){e.preventDefault(); if(players[0]&&players[0].moveQ===dir)players[0].moveQ=-1; el.classList.remove('dn');});
    el.addEventListener('pointerleave',function(){ if(players[0]&&players[0].moveQ===dir)players[0].moveQ=-1; el.classList.remove('dn');});
  });
}
setupDpad();

// Swipe táctil -> Jugador 1
var touchSt = null;
document.addEventListener('touchstart',function(e){if(gstate!=='playing')return;touchSt={x:e.touches[0].clientX,y:e.touches[0].clientY};},{passive:true});
document.addEventListener('touchend',function(e){
  if(!touchSt||gstate!=='playing'||!players[0]) return;
  var dx=e.changedTouches[0].clientX-touchSt.x, dy=e.changedTouches[0].clientY-touchSt.y;
  if(Math.abs(dx)<14&&Math.abs(dy)<14){touchSt=null;return;}
  if(Math.abs(dx)>Math.abs(dy)) players[0].moveQ=dx>0?1:3; else players[0].moveQ=dy>0?2:0;
  players[0].moveRepeat=99; touchSt=null;
},{passive:true});

function togglePause() {
  if(gstate==='playing'){gstate='paused';pauseEl.classList.add('on');}
  else if(gstate==='paused'){gstate='playing';pauseEl.classList.remove('on');}
}

var mmVisible = true;
document.getElementById('actM').addEventListener('click',function(){
  mmVisible=!mmVisible; mmCv.style.display=mmVisible?'block':'none';
  showToast(mmVisible?'Mapa activado':'Mapa oculto');
});
document.getElementById('actP').addEventListener('click',togglePause);
document.getElementById('actH').addEventListener('click',function(){gstate='menu';pauseEl.classList.remove('on');saveRecord();showMenu();});
document.getElementById('btnR2').addEventListener('click',function(){gstate='playing';pauseEl.classList.remove('on');});
document.getElementById('btnRst').addEventListener('click',function(){pauseEl.classList.remove('on');resetAndPlay('¡De nuevo!');});
document.getElementById('btnPM').addEventListener('click',function(){gstate='menu';pauseEl.classList.remove('on');saveRecord();showMenu();});
document.getElementById('btnPlay').addEventListener('click',startGame);
document.getElementById('btnRetry').addEventListener('click',function(){resetAndPlay('¡Otra vez!');});
document.getElementById('btnGoM').addEventListener('click',showMenu);
document.getElementById('btnAgain').addEventListener('click',function(){resetAndPlay('¡A explorar!');});
document.getElementById('btnWinM').addEventListener('click',showMenu);
window.addEventListener('resize',function(){
  resize();
  if(gstate==='playing'||gstate==='paused'){
    players.forEach(function(p){ p.wx=cellAt(p.gx); p.wy=cellAt(p.gy); p.tx=p.wx; p.ty=p.wy; });
    ghosts.forEach(function(g){g.wx=OX+cellAt(g.gx);g.wy=OY+cellAt(g.gy);});
    updateExitMarker();
  }
});

function resetAndPlay(msg) {
  score=0; level=1; coins=0;
  lives = CFG.BASE_LIVES + (numPlayers - 1);
  SS={deaths:0,ghostsEaten:0,pupsGot:0,maxCombo:1,revives:0,startTime:Date.now()};
  buildPlayers();
  resize(); initLevel(); gstate='playing'; hideScreen();
  showBanner(msg, 1600);
}

/* ===================== 21 MISSIONS ===================== */
var MISSION_DEFS = [
  {id:'c50',  label:'Recoge 50 monedas',      type:'coin',  target:50,  reward:200},
  {id:'g3',   label:'Come 3 fantasmas',        type:'ghost', target:3,   reward:300},
  {id:'p5',   label:'Recoge 5 power-ups',      type:'pup',   target:5,   reward:250},
  {id:'lvl2', label:'Alcanza el nivel 3',       type:'level', target:3,   reward:400},
  {id:'c100', label:'Recoge 100 monedas hoy',  type:'coin',  target:100, reward:500},
];
function initMission() {
  try {
    var saved = JSON.parse(localStorage.getItem('mzr_mission')||'null');
    var today = new Date().toDateString();
    if (saved && saved.date===today) {
      mission = MISSION_DEFS.find(function(m){return m.id===saved.id;}) || MISSION_DEFS[0];
      missionProg = saved.progress || 0;
      mission.completed = missionProg >= mission.target;
    } else {
      mission = MISSION_DEFS[Math.floor(Math.random()*MISSION_DEFS.length)];
      missionProg = 0; mission.completed=false; saveMission();
    }
  } catch(e) { mission=MISSION_DEFS[0]; missionProg=0; }
  updateMissionUI();
  missionEl.classList.add('on');
  setTimeout(function(){ missionEl.classList.remove('on'); }, 3500);
}
function updateMission(type, amount) {
  if (!mission || mission.completed) return;
  if (type==='level') { if (level < mission.target) return; missionProg = mission.target; }
  else if (mission.type===type) { missionProg = Math.min(missionProg+amount, mission.target); }
  else return;
  saveMission(); updateMissionUI();
  if (missionProg >= mission.target) {
    mission.completed = true; score += mission.reward;
    showToast('MISION COMPLETADA! +'+mission.reward+'pts');
    emit(OX+cellAt(players[0].gx), OY+cellAt(players[0].gy), '#ffd700', 20, 80, 5);
  }
}
function updateMissionUI() {
  if (!mission) return;
  document.getElementById('mDesc').textContent = mission.label;
  document.getElementById('mFill').style.width = Math.round((missionProg/mission.target)*100)+'%';
}
function saveMission() {
  try { localStorage.setItem('mzr_mission',JSON.stringify({id:mission.id,date:new Date().toDateString(),progress:missionProg})); } catch(e) {}
}

/* ===================== 22 ACHIEVEMENTS ===================== */
var ACH_DEFS = [
  {id:'coin1',    label:'Primera moneda',        emoji:'🥇', check:function(){return coins>=1;}},
  {id:'combo5',   label:'Combo x5',              emoji:'🔥', check:function(){return SS.maxCombo>=5;}},
  {id:'combo10',  label:'Combo x10',             emoji:'⚡', check:function(){return SS.maxCombo>=10;}},
  {id:'ghost1',   label:'Primer fantasma comido',emoji:'👻', check:function(){return SS.ghostsEaten>=1;}},
  {id:'ghost5',   label:'5 fantasmas comidos',   emoji:'💀', check:function(){return SS.ghostsEaten>=5;}},
  {id:'level5',   label:'Nivel 5 alcanzado',     emoji:'🚀', check:function(){return level>=5;}},
  {id:'level10',  label:'Nivel 10 alcanzado',    emoji:'🏆', check:function(){return level>=10;}},
  {id:'nodeath',  label:'Nivel sin morir',        emoji:'⭐', check:function(){return SS.deaths===0&&level>1;}},
  {id:'rich',     label:'5000 puntos',            emoji:'💰', check:function(){return score>=5000;}},
  {id:'rescue',   label:'Revive a un compañero',  emoji:'🤝', check:function(){return SS.revives>=1;}},
  {id:'squad',    label:'Escapa en equipo (3+)',  emoji:'👫', check:function(){return numPlayers>=3 && level>1;}},
];
function checkAchievements() {
  ACH_DEFS.forEach(function(a) {
    if (achievements.indexOf(a.id)<0 && a.check()) {
      achievements.push(a.id);
      showToast(a.emoji+' LOGRO: '+a.label);
      score += CFG.ACH_BONUS;
      saveProfile();
    }
  });
}

/* ===================== 23 PERSISTENCE ===================== */
function loadRecord() {
  try { bestScore = parseInt(localStorage.getItem('mzr_best'))||0; } catch(e) {}
}
function saveRecord() {
  bestScore = Math.max(bestScore, score);
  try { localStorage.setItem('mzr_best', bestScore); } catch(e) {}
  // cartera persistente: las monedas de la partida se suman al perfil
  if (coins > 0) { profile.wallet += coins; coins = 0; }
  pushLeaderboard(profile.name, score);
  saveProfile();
  updateProfileChip();
}

/* ===================== 24 UI HELPERS ===================== */
var toastT = null;
function showToast(msg) {
  toastEl.textContent = msg; toastEl.classList.add('on');
  clearTimeout(toastT); toastT = setTimeout(function(){toastEl.classList.remove('on');}, 2500);
}
function showComboPop(c) {
  comboPop.textContent = 'COMBO x'+c+'!'; comboPop.style.opacity = '1';
  clearTimeout(comboPop._t); comboPop._t = setTimeout(function(){comboPop.style.opacity='0';}, 900);
}
function popScore(x, y, text, col, size) {
  var el = document.createElement('div');
  el.className = 'spop';
  el.style.cssText = 'left:'+Math.round(x)+'px;top:'+Math.round(y)+'px;color:'+col+';font-size:'+(size||14)+'px';
  el.textContent = text; popsEl.appendChild(el);
  setTimeout(function(){el.remove();}, 1350);
}
function showBanner(text, dur) {
  lvlBnr.textContent = text; lvlBnr.style.opacity='1';
  clearTimeout(lvlBnr._t); lvlBnr._t = setTimeout(function(){lvlBnr.style.opacity='0';}, dur||1700);
}
function teamMaxCombo(){ var m=1; players.forEach(function(p){ if(p.combo>m) m=p.combo; }); return m; }
function updateHUD() {
  bestScore = Math.max(bestScore, score);
  hScore.textContent = score.toLocaleString();
  hLevel.textContent = level;
  hCombo.textContent = 'x'+teamMaxCombo();
  hLives.textContent = '♥'.repeat(Math.max(0,lives)) + '♡'.repeat(Math.max(0,(CFG.BASE_LIVES+(numPlayers-1))-lives));
  hCoins.textContent = coins;
  hBest.textContent  = bestScore.toLocaleString();
  // power-up tags (del jugador 1)
  var p0 = players[0] || {puSpeed:0,puGhost:0,puMagnet:0,puFreeze:0};
  var pv = {speed:p0.puSpeed, ghost:p0.puGhost, magnet:p0.puMagnet, freeze:p0.puFreeze};
  Object.keys(pv).forEach(function(k){
    var el=puEls[k], v=pv[k];
    if(v>0){ el.style.display='block'; el.textContent=PU_ICONS[k]+' '+Math.ceil(v)+'s'; }
    else el.style.display='none';
  });
  // chips de jugadores (coop)
  if (numPlayers > 1) renderPlayerChips(); else pchipsEl.innerHTML='';
  if (frameN%120===0) checkAchievements();
}
function renderPlayerChips() {
  var html = '';
  players.forEach(function(p){
    var cls = 'pchip' + (p.state==='downed'?' down':p.state==='escaped'?' escaped':'');
    var st = p.state==='downed'?'CAÍDO':p.state==='escaped'?'✓':'x'+p.combo;
    html += '<div class="'+cls+'"><span class="pe">'+p.char.emoji+'</span><span class="pn">'+p.name+'</span><span class="pc">'+st+'</span></div>';
  });
  pchipsEl.innerHTML = html;
}
function hideScreen() { screenEl.classList.add('off'); exitMark.style.display='none'; }

function showMenu() {
  gstate = 'menu';
  screenEl.classList.remove('off'); showView('v-menu');
  document.getElementById('sRec').textContent = bestScore>0 ? 'Record: '+bestScore.toLocaleString() : '';
  buildCharGrid(); renderLeaderboard('lbWrap'); updateProfileChip();
  // precargar nombre
  var ni = document.getElementById('playerName');
  if (ni && !ni.value) ni.value = (profile.type!=='guest' ? profile.name : '');
}

function showEnd(type) {
  screenEl.classList.remove('off');
  var elapsed = Math.round((Date.now()-SS.startTime)/1000);
  var stars = SS.deaths===0?3:SS.deaths<=2?2:1;
  var statGrid = [['Puntos',score.toLocaleString()],['Monedas',coins],['Muertes',SS.deaths],['Combo','x'+SS.maxCombo],['Fantasmas',SS.ghostsEaten],['Tiempo',elapsed+'s']];
  if (numPlayers>1) statGrid[2] = ['Revividos', SS.revives];
  if (type==='gameover') {
    showView('v-go');
    document.getElementById('goT').textContent = 'GAME OVER';
    document.getElementById('goS').textContent = teamLabel()+' llegó al nivel '+level;
    document.getElementById('goR').textContent = 'Record: '+bestScore.toLocaleString();
    document.getElementById('goG').innerHTML = mkGrid(statGrid);
    document.getElementById('goSt').innerHTML = mkStars(stars);
  } else {
    showView('v-win');
    document.getElementById('winS').textContent = teamLabel()+' completó '+(level-1)+' niveles!';
    document.getElementById('winG').innerHTML = mkGrid(statGrid);
    document.getElementById('winSt').innerHTML = mkStars(3);
    document.getElementById('winB').innerHTML = achievements.map(function(id){
      var a=ACH_DEFS.find(function(ac){return ac.id===id;});
      return a ? '<span class="badge">'+a.emoji+' '+a.label+'</span>' : '';
    }).join('');
  }
  exitMark.style.display='none';
}
function teamLabel(){ return numPlayers>1 ? ('El equipo ('+numPlayers+')') : (players[0]?players[0].name:profile.name); }
function showView(id) {
  ['v-menu','v-go','v-win'].forEach(function(v){ document.getElementById(v).classList.toggle('off', v!==id); });
}
function mkGrid(items) {
  return items.map(function(it){ return '<div class="ri"><div class="rv">'+it[1]+'</div><div class="rl">'+it[0]+'</div></div>'; }).join('');
}
function mkStars(n) {
  return Array.from({length:3},function(_,i){ return '<span class="'+(i<n?'lit':'')+'">'+( i<n?'★':'☆')+'</span>'; }).join('');
}
function renderLeaderboard(targetId) {
  var el = document.getElementById(targetId); if (!el) return;
  var lb = loadLeaderboard();
  if (!lb.length) { el.innerHTML=''; return; }
  var rows = lb.slice(0,5).map(function(r,i){
    return '<div class="lrow"><span>'+(i+1)+'. '+escapeHtml(r.name)+'</span><span class="lp">'+r.score.toLocaleString()+'</span></div>';
  }).join('');
  el.innerHTML = '<div class="sl" style="text-align:center">🏅 Ranking local</div><div class="lb">'+rows+'</div>';
}
function escapeHtml(s){ return String(s).replace(/[&<>"]/g,function(c){return{'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;'}[c];}); }

/* ===================== 25 BOOT ===================== */
function updateProfileChip() {
  document.getElementById('pcEmoji').textContent = charById(profile.avatar).emoji;
  document.getElementById('pcName').textContent = profile.type==='guest' ? 'Invitado' : (profile.name || 'Cuenta');
  document.getElementById('pcWallet').textContent = profile.wallet + '🪙';
}
function buildCharGrid() {
  var grid = document.getElementById('charGrid'); if(!grid) return;
  grid.innerHTML = '';
  var curId = profile.avatar;
  CHARS.forEach(function(c) {
    var div = document.createElement('div');
    div.className = 'cc' + (c.id===curId?' sel':'') + (c.unlocked?'':' lk');
    div.innerHTML = '<span class="ce">'+c.emoji+'</span><div class="cn">'+c.name+'</div><div class="ct">'+c.trait+'</div><div class="ck">'+(c.unlocked?'✓ Listo':c.cost+'🪙')+'</div>';
    div.addEventListener('click', function() {
      if (c.unlocked) {
        profile.avatar = c.id; saveProfile();
        document.querySelectorAll('.cc').forEach(function(el){el.classList.remove('sel');});
        div.classList.add('sel');
        hEmoji.textContent=c.emoji; hName.textContent=c.name; updateProfileChip();
        showToast(c.emoji+' '+c.name+' seleccionado!');
      } else if (profile.wallet >= c.cost) {
        profile.wallet -= c.cost; c.unlocked=true; profile.avatar=c.id;
        saveProfile(); buildCharGrid(); updateProfileChip();
        showToast(c.emoji+' '+c.name+' desbloqueado!');
      } else {
        showToast('Necesitas '+c.cost+'🪙 para '+c.name+' (tienes '+profile.wallet+')');
      }
    });
    grid.appendChild(div);
  });
}

// Selector de jugadores
document.getElementById('pcountSeg').addEventListener('click', function(e){
  var b = e.target.closest('.so'); if (!b) return;
  numPlayers = parseInt(b.getAttribute('data-pc'))||1;
  document.querySelectorAll('#pcountSeg .so').forEach(function(el){ el.classList.toggle('sel', el===b); });
  var hints = ['P1: WASD/Flechas/D-Pad · ESC pausa',
               'P1: WASD/Flechas · P2: IJKL · ESC pausa',
               'P1: WASD · P2: IJKL · P3: TFGH · ESC pausa',
               'P1: WASD · P2: IJKL · P3: TFGH · P4: Numpad'];
  document.getElementById('ctrlHint').textContent = hints[numPlayers-1];
});

/* ---- Cuenta (modal) ---- */
var acctModal = document.getElementById('acctModal');
document.getElementById('profile-chip').addEventListener('click', openAccount);
document.getElementById('acctClose').addEventListener('click', function(){ acctModal.classList.remove('on'); });
document.getElementById('btnGuest').addEventListener('click', function(){
  profile.type='guest'; profile.name='Invitado'; saveProfile(); updateProfileChip(); acctModal.classList.remove('on');
});
document.getElementById('btnEmailLogin').addEventListener('click', function(){
  var em = document.getElementById('acctEmail').value.trim();
  if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(em)) { showToast('Correo no válido'); return; }
  profile.type='email'; profile.email=em; profile.name = em.split('@')[0]; saveProfile();
  updateProfileChip(); showMenu(); acctModal.classList.remove('on'); showToast('Cuenta guardada (local)');
});
function openAccount(){
  acctModal.classList.add('on');
  var note = document.getElementById('googleNote');
  var gb = document.getElementById('btnGoogle');
  if (!GOOGLE_CLIENT_ID) {
    gb.disabled = true;
    note.innerHTML = 'Google Login no configurado. Para activarlo: define tu OAuth Client ID con<br>localStorage.setItem("mzr_gcid","TU_CLIENT_ID") y recarga. Mientras tanto, usa correo o invitado.';
  } else { gb.disabled = false; note.textContent=''; }
}
function jwtPayload(t){ try { return JSON.parse(decodeURIComponent(atob(t.split('.')[1].replace(/-/g,'+').replace(/_/g,'/')).split('').map(function(c){return '%'+('00'+c.charCodeAt(0).toString(16)).slice(-2);}).join(''))); } catch(e){ return null; } }
function onGoogleCredential(resp){
  var p = jwtPayload(resp.credential); if (!p) { showToast('No se pudo leer la sesión de Google'); return; }
  profile.type='google'; profile.name = p.name || (p.email||'').split('@')[0] || 'Jugador';
  profile.email = p.email||''; profile.picture = p.picture||'';
  saveProfile(); updateProfileChip(); showMenu(); acctModal.classList.remove('on');
  showToast('¡Hola, '+profile.name+'!');
}
document.getElementById('btnGoogle').addEventListener('click', function(){
  if (!GOOGLE_CLIENT_ID) return;
  if (!window.google || !google.accounts || !google.accounts.id) { showToast('Cargando Google…'); initGoogle(true); return; }
  google.accounts.id.prompt();
});
function initGoogle(thenPrompt){
  if (!GOOGLE_CLIENT_ID) return;
  function setup(){
    try {
      google.accounts.id.initialize({ client_id: GOOGLE_CLIENT_ID, callback: onGoogleCredential });
      if (thenPrompt) google.accounts.id.prompt();
    } catch(e){}
  }
  if (window.google && google.accounts && google.accounts.id) { setup(); return; }
  var s = document.createElement('script');
  s.src = 'https://accounts.google.com/gsi/client'; s.async = true; s.defer = true;
  s.onload = setup; document.head.appendChild(s);
}

// Inicialización
loadProfile(); loadRecord(); claimDaily();
resize(); buildCharGrid(); updateProfileChip(); showMenu();
if (GOOGLE_CLIENT_ID) initGoogle(false);
requestAnimationFrame(loop);

// API de depuración (consola)
window.MAZE = { get state(){return gstate;}, get players(){return players;}, get score(){return score;}, profile:function(){return profile;}, start:startGame };

})();
