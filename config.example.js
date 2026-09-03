// Plantilla de configuración. Copia este archivo como "config.js".
// "endpoint" es la URL de TU proxy en Cloudflare Workers (carpeta /worker),
// no la de Decolecta directamente (Decolecta bloquea llamadas del navegador).
// "appSecret" debe coincidir con el secreto APP_SECRET configurado en el Worker.
const DNI_API_CONFIG = {
  endpoint: "https://TU-WORKER.TU-SUBDOMINIO.workers.dev/dni",
  appSecret: "TU_APP_SECRET_AQUI",
  limiteMensual: 100
};
