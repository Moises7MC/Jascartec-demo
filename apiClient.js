/* ============================================================
   apiClient.js — Cliente HTTP hacia el backend Jascartec.Api
   ------------------------------------------------------------
   Sin secretos: la URL del backend local no es sensible (a
   diferencia de config.js del buscador de DNI), así que este
   archivo sí se sube al repositorio con normalidad.

   El token JWT se guarda solo en memoria (variable de módulo),
   no en localStorage — igual que antes la sesión (currentUser)
   tampoco sobrevivía a un refresco de página.
   ============================================================ */

const API_BASE_URL = 'http://localhost:5080/api';

let authToken = null;

class ApiError extends Error {}

function setAuthToken(token) { authToken = token; }
function clearAuthToken() { authToken = null; }

async function apiFetch(path, { method = 'GET', body } = {}) {
    const headers = { 'Content-Type': 'application/json' };
    if (authToken) headers['Authorization'] = `Bearer ${authToken}`;

    let resp;
    try {
        resp = await fetch(`${API_BASE_URL}${path}`, {
            method,
            headers,
            body: body !== undefined ? JSON.stringify(body) : undefined
        });
    } catch {
        throw new ApiError('No se pudo conectar con el servidor. ¿Está corriendo el backend (Jascartec.Api)?');
    }

    if (resp.status === 204) return null;

    let data = null;
    try { data = await resp.json(); } catch { /* respuesta sin cuerpo JSON */ }

    if (!resp.ok) {
        if (resp.status === 401) clearAuthToken();
        const mensaje = (data && (data.detail || data.title)) || `Error ${resp.status}`;
        throw new ApiError(mensaje);
    }
    return data;
}

const api = {
    get: (path) => apiFetch(path),
    post: (path, body) => apiFetch(path, { method: 'POST', body }),
    put: (path, body) => apiFetch(path, { method: 'PUT', body }),
    patch: (path, body) => apiFetch(path, { method: 'PATCH', body }),
    del: (path) => apiFetch(path, { method: 'DELETE' })
};
