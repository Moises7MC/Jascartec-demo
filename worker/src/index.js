/* ============================================================
   Proxy Cloudflare Worker — consulta de DNI (RENIEC) vía Decolecta
   ------------------------------------------------------------
   Motivo de este proxy: la API de Decolecta bloquea llamadas
   directas desde el navegador (CORS) y su token no debe quedar
   expuesto en el frontend. Este Worker:
     1) Recibe la petición del sistema Jascartec (con CORS abierto).
     2) Verifica un secreto simple compartido (X-App-Secret) para
        que no cualquiera en internet gaste la cuota gratuita.
     3) Llama a Decolecta servidor-a-servidor con el token real
        guardado como secreto de Cloudflare (nunca visible al
        navegador).
     4) Devuelve la respuesta tal cual (mismo status/body), para
        que el frontend siga detectando el límite mensual (429)
        igual que antes.
   ============================================================ */

export default {
  async fetch(request, env) {
    const corsHeaders = {
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Methods': 'GET, OPTIONS',
      'Access-Control-Allow-Headers': 'Content-Type, X-App-Secret',
    };

    if (request.method === 'OPTIONS') {
      return new Response(null, { headers: corsHeaders });
    }

    const url = new URL(request.url);

    if (url.pathname !== '/dni' || request.method !== 'GET') {
      return new Response(JSON.stringify({ error: 'not_found' }), {
        status: 404,
        headers: { 'Content-Type': 'application/json', ...corsHeaders },
      });
    }

    const appSecret = request.headers.get('X-App-Secret');
    if (!env.APP_SECRET || appSecret !== env.APP_SECRET) {
      return new Response(JSON.stringify({ error: 'unauthorized' }), {
        status: 401,
        headers: { 'Content-Type': 'application/json', ...corsHeaders },
      });
    }

    const numero = url.searchParams.get('numero') || '';
    if (!/^\d{8}$/.test(numero)) {
      return new Response(JSON.stringify({ error: 'dni_invalido' }), {
        status: 400,
        headers: { 'Content-Type': 'application/json', ...corsHeaders },
      });
    }

    const apiResp = await fetch(`https://api.decolecta.com/v1/reniec/dni?numero=${numero}`, {
      headers: {
        'Authorization': `Bearer ${env.DECOLECTA_TOKEN}`,
        'Content-Type': 'application/json',
      },
    });

    const body = await apiResp.text();
    return new Response(body, {
      status: apiResp.status,
      headers: { 'Content-Type': 'application/json', ...corsHeaders },
    });
  },
};
