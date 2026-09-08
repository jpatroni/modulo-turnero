const $ = id => document.getElementById(id);
let config = {}, token = '', dirty = false, uploads = 0, saving = false;
const definitions = [
 ['appearanceFields','tipografia','Tipografía','select',['Arial','Segoe UI','Verdana','Tahoma','Georgia','Trebuchet MS','Courier New']],
 ['appearanceFields','textoEspera','Texto de espera','text',80],
 ['appearanceFields','tamanoAviso','Tamaño máximo del llamado (% del ancho)','number',2,20],
 ['appearanceFields','tamanoHistorial','Tamaño de números del historial (px)','number',18,100],
 ['appearanceFields','segundosAviso','Duración del llamado (segundos)','number',1,60],
 ['appearanceFields','colorFondo','Color de fondo','color'],
 ['appearanceFields','colorTexto','Color de texto','color'],
 ['appearanceFields','colorAviso','Color del llamado','color'],
 ['bannerFields','tamanoMarquesina','Tamaño del banner (px)','number',14,80],
 ['bannerFields','velocidadMarquesina','Tiempo de recorrido (segundos)','number',5,120],
 ['bannerFields','colorMarquesina','Color del banner','color'],
 ['bannerFields','colorTextoMarquesina','Texto del banner','color']
];
function field(parent, id, title, type, min, max) {
 const label = document.createElement('label'); label.textContent = title;
 const input = document.createElement(type === 'select' ? 'select' : 'input');
 input.id = id;
 if (type === 'select') for (const text of min) { const option = document.createElement('option'); option.value = text; option.textContent = text; input.append(option); }
 else { input.type = type; if (type === 'number') { input.min = min; input.max = max; input.step = 1; } if (type === 'text') input.maxLength = min; }
 if (type === 'checkbox') label.className = 'toggle'; else input.required = true;
 label.append(input); parent.append(label); return input;
}
for (const [parent,...args] of definitions) field($(parent), ...args);
const media = [
 ['fondo','Imagen de fondo','imagen','.png,.jpg,.jpeg,.gif,.webp','5 MB · JPG, PNG, GIF o WebP'],
 ['logo','Logo del negocio','imagen','.png,.jpg,.jpeg,.gif,.webp','5 MB · JPG, PNG, GIF o WebP'],
 ['video','Video lateral','video','.mp4,.webm','100 MB · MP4 o WebM. Se reproduce sin audio, en bucle.'],
 ['sonido','Sonido personalizado','audio','.mp3,.wav,.ogg','5 MB · MP3, WAV u OGG']
];
field($('soundFields'), 'sonidoActivo', 'Activar sonido en los llamados', 'checkbox');
for (const [key,title,type,accept,help] of media) {
 const section = document.createElement('div'); section.className = 'media-block';
 $(key === 'sonido' ? 'soundFields' : 'mediaFields').append(section);
 field(section, key === 'sonido' ? 'sonidoPersonalizado' : key+'Activo', title, 'checkbox');
 const label = document.createElement('label'); label.textContent = 'Subir archivo';
 const input = document.createElement('input'); input.type = 'file'; input.accept = accept; input.id = key+'Archivo';
 const small = document.createElement('small'); small.textContent = help; label.append(input, small); section.append(label);
 const state = document.createElement('p'); state.className = 'file-state'; state.id = key+'Estado'; section.append(state);
 const remove = document.createElement('button'); remove.type = 'button'; remove.className = 'remove'; remove.textContent = 'Quitar archivo'; remove.id = key+'Quitar'; section.append(remove);
 remove.addEventListener('click', () => { config[key+'Url'] = ''; $(key === 'sonido' ? 'sonidoPersonalizado' : key+'Activo').checked = false; input.value = ''; changed(); showMedia(); });
 input.addEventListener('change', () => upload(key, type, input));
 if (key === 'fondo') field(section, 'fondoOpacidad','Opacidad del fondo (%)','number',0,100);
 if (key === 'logo') { const group = document.createElement('div'); group.className = 'fields'; section.append(group); field(group,'logoTamano','Ancho del logo (%)','number',1,40); field(group,'logoPosicionX','Posición horizontal (%)','number',0,90); field(group,'logoPosicionY','Posición vertical (%)','number',0,90); }
}
function status(text, error = false) { $('status').textContent = text; $('status').classList.toggle('error',error); }
function lockSave() { $('save').disabled = uploads > 0 || saving; }
async function api(path, options = {}) {
 const headers = { ...options.headers };
 if (options.method && options.method !== 'GET') headers['X-CSRF-TOKEN'] = token;
 const response = await fetch(path, { ...options, headers });
 const data = await response.json().catch(() => ({}));
 if (!response.ok) {
  if (response.status === 404 && path.startsWith("/api/datos")) throw Error("Reiniciá Turnero desde Visual Studio para activar el módulo de datos.");
  if (response.status === 404 && path === "/api/configuracion/sesion") throw Error("El servidor está desactualizado. Reiniciá Turnero para habilitar el administrador.");
  if (response.status === 401 && !path.endsWith('/login')) { showLogin(); throw Error('La sesión venció. Volvé a ingresar.'); }
  throw Error(data.error || (response.status === 429 ? 'Demasiados intentos. Esperá un minuto.' : response.status === 400 ? 'Revisá los campos y sus valores. Si la sesión venció, recargá la página.' : 'No se pudo completar la operación. Intentá nuevamente.'));
 }
 return data;
}
async function session() { const state = await api('/api/configuracion/sesion'); token = state.token; return state.autenticado; }
function showLogin() { $('app').hidden = true; $('login').hidden = false; }
async function loadPanel() {
 config = await api('/api/configuracion');
 for (const input of $('configForm').querySelectorAll('input:not([type=file]), select')) {
  if (input.type === 'checkbox') input.checked = !!config[input.id]; else input.value = config[input.id] ?? '';
 }
 $('mensajesMarquesina').value = (config.mensajesMarquesina || []).join('\n');
 $('login').hidden = true; $('app').hidden = false; dirty = false;
 showMedia(); preview(); status('Los cambios guardados se aplican en tiempo real.');
 window.dispatchEvent(new Event('sesion-iniciada'));
}
function collect() {
 for (const input of $('configForm').querySelectorAll('input:not([type=file]), select')) config[input.id] = input.type === 'checkbox' ? input.checked : input.type === 'number' ? Number(input.value) : input.value;
 config.mensajesMarquesina = $('mensajesMarquesina').value.split('\n').map(s => s.trim()).filter(Boolean);
 return config;
}
function preview() {
 const frame = $('preview');
 frame.style.transform = `scale(${frame.parentElement.clientWidth / 1280})`;
 if (config.textoEspera) frame.contentWindow.postMessage({ tipo: 'vista-previa', configuracion: config }, location.origin);
}
function changed() { collect(); dirty = true; status('Tenés cambios sin guardar.'); preview(); }
function showMedia() {
 for (const [key] of media) { const url = config[key+'Url']; const state = $(key+'Estado'); state.replaceChildren(); if (url) { const link = document.createElement('a'); link.href = url; link.target = '_blank'; link.rel = 'noopener'; link.textContent = 'Ver archivo cargado ↗'; state.append(link); } else state.textContent = 'Sin archivo cargado.'; $(key+'Quitar').hidden = !url; }
}
async function upload(key, tipo, input) {
 const file = input.files[0]; if (!file) return;
 if (file.size > (tipo === 'video' ? 100 : 5) * 1024 * 1024) { status('El archivo supera el tamaño permitido.', true); input.value = ''; return; }
 uploads++; lockSave(); input.disabled = true; $(key+'Quitar').disabled = true; $(key+'Estado').textContent = 'Subiendo archivo…';
 try {
  const data = new FormData(); data.append('archivo',file); data.append('tipo',tipo);
  const result = await api('/api/archivos/subir', { method:'POST', body:data });
  config[key+'Url'] = result.url;
  $(key === 'sonido' ? 'sonidoPersonalizado' : key+'Activo').checked = true;
  changed(); status('Archivo cargado. Guardá los cambios para mostrarlo en la pantalla.');
 } catch(error) { status(error.message,true); }
 finally { uploads--; lockSave(); input.disabled = false; $(key+'Quitar').disabled = false; input.value = ''; showMedia(); }
}
$('configForm').addEventListener('input', event => { if (event.target.type !== 'file') changed(); });
$('configForm').addEventListener('submit', async event => {
 event.preventDefault(); if (uploads || saving) return;
 collect();
 if (!config.mensajesMarquesina.length || config.mensajesMarquesina.length > 20) { status('Ingresá entre 1 y 20 mensajes para el banner.',true); $('mensajesMarquesina').focus(); return; }
 for (const [key] of media) if (config[key === 'sonido' ? 'sonidoPersonalizado' : key+'Activo'] && !config[key+'Url']) { status('Subí el archivo antes de activar esa opción de multimedia.',true); $(key+'Archivo').focus(); return; }
 saving = true; lockSave(); $('configForm').inert = true;
 try { await api('/api/configuracion', { method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify(config) }); dirty = false; status('Cambios guardados. Las pantallas se actualizaron.'); }
 catch(error) { status(error.message,true); }
 finally { saving = false; lockSave(); $('configForm').inert = false; }
});
$('loginForm').addEventListener('submit', async event => {
 event.preventDefault(); const button = event.submitter; button.disabled = true; $('loginError').textContent = '';
 try { await session(); const data = new FormData(event.target); await api('/api/configuracion/login', {method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(Object.fromEntries(data))}); event.target.elements.clave.value = ''; await session(); await loadPanel(); }
 catch(error) { $('loginError').textContent = error.message; }
 finally { button.disabled = false; }
});
$('logout').addEventListener('click', async () => {
 if (uploads || saving) return;
 if (dirty && !confirm('Tenés cambios sin guardar. ¿Querés salir y descartarlos?')) return;
 try { await api('/api/configuracion/logout', {method:'POST'}); dirty = false; config = {}; showLogin(); await session(); }
 catch(error) { status(error.message,true); }
});
$('preview').addEventListener('load', preview);
new ResizeObserver(preview).observe($('preview').parentElement);
window.addEventListener('beforeunload', event => { if (dirty || uploads) { event.preventDefault(); event.returnValue = ''; } });
(async () => { try { if (await session()) await loadPanel(); } catch(error) { $('loginError').textContent = error.message; } })();
window.addEventListener("message", event => { if (event.origin === location.origin && event.source === $("preview").contentWindow && event.data?.tipo === "pantalla-lista") preview(); });

// La pantalla siempre usa la configuración guardada.
for (const link of document.querySelectorAll('[data-mostrar-pantalla]')) {
 link.addEventListener('click', event => {
  if (dirty || uploads || saving) {
   event.preventDefault();
   status('Guardá los cambios antes de mostrar la pantalla.', true);
   $('save').scrollIntoView({ block: 'center' });
   $('save').focus();
  }
 });
}


