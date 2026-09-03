/* ============================================================
   dniApi.js — Consulta de DNI (RENIEC) vía Decolecta
   ------------------------------------------------------------
   Plan gratuito: 100 consultas/mes (ver config.js).
   Cuando se agota el límite del mes, el sistema NO muestra
   errores: recuerda ese estado en el navegador y deja que el
   resto de ventas del mes se registren con el formulario de
   cliente 100% manual. Al iniciar un mes nuevo, vuelve a
   intentar la API automáticamente sin que nadie toque nada.

   La llamada real va a NUESTRO proxy en Cloudflare Workers
   (carpeta /worker), no directo a Decolecta: Decolecta bloquea
   llamadas desde el navegador (CORS) y su token no debe quedar
   expuesto en el frontend. El proxy guarda ese token a salvo y
   reenvía la respuesta tal cual (mismo status/body), así que la
   detección del límite (429) sigue funcionando igual.
   ============================================================ */

const DNI_ESTADO_KEY = 'jascartec_dni_api_estado';

function mesActualISO() {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
}

function leerEstadoDniApi() {
    try {
        const raw = localStorage.getItem(DNI_ESTADO_KEY);
        if (!raw) return { mes: mesActualISO(), agotado: false };
        const estado = JSON.parse(raw);
        if (estado.mes !== mesActualISO()) return { mes: mesActualISO(), agotado: false }; // mes nuevo => se reinicia solo
        return estado;
    } catch {
        return { mes: mesActualISO(), agotado: false };
    }
}

function guardarEstadoDniApi(estado) {
    try { localStorage.setItem(DNI_ESTADO_KEY, JSON.stringify(estado)); } catch { /* localStorage no disponible: seguimos sin persistir */ }
}

function marcarDniApiAgotada() {
    guardarEstadoDniApi({ mes: mesActualISO(), agotado: true });
}

function dniApiDisponible() {
    return !leerEstadoDniApi().agotado;
}

/**
 * Consulta un DNI en RENIEC.
 * Devuelve:
 *   { ok: true, data: { nombres, apellidoPaterno, apellidoMaterno, nombreCompleto, dni } }
 *   { ok: false, motivo: 'agotado' | 'no_encontrado' | 'invalido' | 'error' }
 */
async function consultarDNI(numero) {
    if (!/^\d{8}$/.test(numero)) {
        return { ok: false, motivo: 'invalido' };
    }
    if (!dniApiDisponible()) {
        return { ok: false, motivo: 'agotado' };
    }

    try {
        const resp = await fetch(`${DNI_API_CONFIG.endpoint}?numero=${numero}`, {
            headers: {
                'X-App-Secret': DNI_API_CONFIG.appSecret,
                'Content-Type': 'application/json'
            }
        });

        if (resp.status === 429) {
            marcarDniApiAgotada();
            return { ok: false, motivo: 'agotado' };
        }

        if (!resp.ok) {
            let cuerpo = null;
            try { cuerpo = await resp.json(); } catch { /* respuesta sin cuerpo JSON */ }
            const msg = ((cuerpo && (cuerpo.message || cuerpo.error)) || '').toString().toLowerCase();
            if (msg.includes('limit') || msg.includes('quota') || msg.includes('límite') || msg.includes('plan')) {
                marcarDniApiAgotada();
                return { ok: false, motivo: 'agotado' };
            }
            if (resp.status === 404) return { ok: false, motivo: 'no_encontrado' };
            return { ok: false, motivo: 'error' };
        }

        const data = await resp.json();
        return {
            ok: true,
            data: {
                nombres: data.first_name || '',
                apellidoPaterno: data.first_last_name || '',
                apellidoMaterno: data.second_last_name || '',
                nombreCompleto: data.full_name || `${data.first_last_name || ''} ${data.second_last_name || ''} ${data.first_name || ''}`.trim(),
                dni: data.document_number || numero
            }
        };
    } catch {
        return { ok: false, motivo: 'error' }; // sin conexión u otro problema de red
    }
}
