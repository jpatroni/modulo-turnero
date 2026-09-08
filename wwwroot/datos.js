(() => {
 const el = id => document.getElementById(id);
 let cargando = false;
 function fila(destino, valores) {
  const tr = document.createElement('tr');
  for (const valor of valores) { const td = document.createElement('td'); td.textContent = valor; tr.append(td); }
  destino.append(tr);
 }
 function barras(destino, series) {
  destino.replaceChildren();
  const maximo = Math.max(1, ...series.map(s => s.llamados));
  for (const serie of series) {
   const row = document.createElement('div'); row.className = 'data-bar-row';
   const label = document.createElement('span'); label.textContent = serie.etiqueta;
   const track = document.createElement('div'); track.className = 'data-bar-track'; track.setAttribute('aria-hidden','true');
   const bar = document.createElement('div'); bar.style.width = `${serie.llamados / maximo * 100}%`; track.append(bar);
   const count = document.createElement('strong'); count.textContent = serie.llamados;
   row.append(label,track,count); destino.append(row);
  }
 }
 async function cargarDatos() {
  if (cargando || el('app').hidden) return;
  cargando = true; el('datosActualizar').disabled = true;
  const dias = el('datosPeriodo').value;
  el('datosEstado').textContent = 'Consultando registros…';
  try {
   const resumen = await api(`/api/datos?dias=${dias}`);
   if (dias !== el('datosPeriodo').value) return;
   el('datosTotal').textContent = resumen.total;
   el('datosCajas').textContent = resumen.cajas;
   const maximo = Math.max(0, ...resumen.porHora.map(h => h.llamados));
   const picos = resumen.porHora.filter(h => h.llamados === maximo);
   el('datosPico').textContent = !maximo ? '—' : picos.length > 1 ? 'Varias horas' : `${String(picos[0].hora).padStart(2,'0')}:00`;
   el('datosVacio').hidden = resumen.total !== 0;
   el('datosPorCaja').replaceChildren();
   for (const caja of resumen.porCaja) fila(el('datosPorCaja'), [`Caja ${caja.caja}`,caja.llamados,caja.intervaloMedioSegundos == null ? 'Sin pares de llamados' : `${(caja.intervaloMedioSegundos / 60).toLocaleString('es-AR',{maximumFractionDigits:1})} min (${caja.intervalos} intervalos)`]);
   barras(el('datosPorHora'),resumen.porHora.map(h => ({...h,etiqueta:`${String(h.hora).padStart(2,'0')}:00`})));
   const diasSemana = ['Dom','Lun','Mar','Mié','Jue','Vie','Sáb'];
   barras(el('datosPorDia'),resumen.porDiaSemana.map(d => ({...d,etiqueta:diasSemana[d.dia]})));
   el('datosRecientes').replaceChildren();
   for (const registro of resumen.recientes) fila(el('datosRecientes'), [new Date(registro.fechaUtc).toLocaleString('es-AR',{timeZone:'America/Argentina/Buenos_Aires'}), `Caja ${registro.caja}`,registro.cajaOrigen]);
   el('datosEstado').textContent = `Actualizado a las ${new Date().toLocaleTimeString('es-AR')}. Se actualiza cada 30 segundos mientras mirás esta sección.`;
   el('datosEstado').classList.remove('error');
  } catch(error) { el('datosEstado').textContent = error.message; el('datosEstado').classList.add('error'); }
  finally { cargando = false; el('datosActualizar').disabled = false; if (dias !== el('datosPeriodo').value) cargarDatos(); }
 }
 el('datosActualizar').addEventListener('click',cargarDatos);
 el('datosPeriodo').addEventListener('change', () => { el('datosExportar').href = `/api/datos/registros?dias=${el('datosPeriodo').value}`; cargarDatos(); });
 window.addEventListener('sesion-iniciada',cargarDatos);
 document.querySelector('a[href="#datos"]').addEventListener('click',cargarDatos);
 setInterval(() => { const rect = el('datos').getBoundingClientRect(); if (!document.hidden && rect.top < innerHeight && rect.bottom > 0) cargarDatos(); },30000);
})();
