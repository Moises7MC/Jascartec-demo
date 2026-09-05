@echo off
setlocal enabledelayedexpansion
title Instalacion de Jascartec
color 0B

:: ============================================================
:: INSTALAR.bat - Instalador de Jascartec para la laptop del negocio
:: Ejecutar como Administrador (clic derecho, "Ejecutar como administrador")
:: ============================================================

net session >nul 2>&1
if %errorLevel% neq 0 (
    echo.
    echo  ERROR: Este instalador necesita permisos de Administrador.
    echo  Cierra esta ventana, haz clic DERECHO sobre INSTALAR.bat
    echo  y elegi "Ejecutar como administrador".
    echo.
    pause
    exit /b 1
)

echo ============================================
echo   Instalando Jascartec - Sistema de Ventas
echo ============================================
echo.

set "USB=%~dp0"
set "DESTINO=C:\Jascartec"
set "PGPASS=Jascartec2026Trujillo"
set "PGBIN=C:\Program Files\PostgreSQL\18\bin"

echo [1/6] Copiando los archivos del sistema a %DESTINO% ...
if not exist "%DESTINO%" mkdir "%DESTINO%"
xcopy "%USB%app" "%DESTINO%\app\" /E /I /Y >nul
xcopy "%USB%backend" "%DESTINO%\backend\" /E /I /Y >nul
copy "%USB%iniciar_jascartec.vbs" "%DESTINO%\iniciar_jascartec.vbs" /Y >nul
copy "%USB%jascartec.ico" "%DESTINO%\jascartec.ico" /Y >nul
copy "%USB%jascartec.ico" "%DESTINO%\app\jascartec.ico" /Y >nul
echo       Listo.
echo.

echo [2/6] Verificando PostgreSQL (base de datos)...
if exist "%PGBIN%\psql.exe" (
    echo       Ya esta instalado, se omite este paso.
) else (
    echo       Instalando PostgreSQL 18. Esto puede tardar varios minutos.
    echo       NO CIERRES esta ventana ni la del instalador hasta que termine solo.
    "%USB%instaladores\PostgreSQL-18-instalador.exe" --mode unattended --unattendedmodeui minimal --superpassword "%PGPASS%" --serverport 5432 --disable-components stackbuilder
    if not exist "%PGBIN%\psql.exe" (
        echo.
        echo       ATENCION: no se detecto PostgreSQL instalado correctamente.
        echo       Segui el PASO 2 de LEEME_INSTALACION.txt para instalarlo a mano.
        echo.
        pause
    ) else (
        echo       PostgreSQL instalado correctamente.
    )
)
echo.

echo [3/6] Esperando a que PostgreSQL este listo para recibir conexiones...
if exist "%PGBIN%\pg_isready.exe" (
    set "pg_ok=0"
    for /l %%i in (1,1,20) do (
        if "!pg_ok!"=="0" (
            "%PGBIN%\pg_isready.exe" -h 127.0.0.1 -p 5432 >nul 2>&1
            if !errorLevel! equ 0 (
                set "pg_ok=1"
            ) else (
                timeout /t 2 /nobreak >nul
            )
        )
    )
    if "!pg_ok!"=="1" (
        echo       Listo, el servidor esta respondiendo.
    ) else (
        echo       ATENCION: el servidor tardo mas de lo normal en responder.
        echo       Se va a intentar crear la base de datos igual a continuacion.
    )
) else (
    echo       Se omite: PostgreSQL todavia no esta instalado ^(ver paso anterior^).
)
echo.

echo [4/6] Creando la base de datos "jascartec"...
if not exist "%PGBIN%\psql.exe" (
    echo       Se omite: PostgreSQL todavia no esta instalado.
) else (
    set "PGPASSWORD=%PGPASS%"
    "%PGBIN%\psql.exe" -h 127.0.0.1 -U postgres -tc "SELECT 1 FROM pg_database WHERE datname = 'jascartec'" 2>nul | findstr /C:"1" >nul
    if !errorLevel! equ 0 (
        echo       La base de datos ya existia, se omite.
    ) else (
        "%PGBIN%\createdb.exe" -h 127.0.0.1 -U postgres jascartec
        if !errorLevel! equ 0 (
            echo       Base de datos "jascartec" creada.
        ) else (
            echo       ATENCION: no se pudo crear la base de datos automaticamente.
            echo       Segui el PASO 3 de LEEME_INSTALACION.txt para crearla a mano,
            echo       y despues volve a ejecutar este instalador.
            pause
        )
    )
)
echo.

echo [5/7] Verificando Google Chrome...
if exist "C:\Program Files\Google\Chrome\Application\chrome.exe" (
    echo       Ya esta instalado, se omite este paso.
) else (
    echo       Instalando Google Chrome...
    "%USB%instaladores\Chrome-instalador.exe" /silent /install
    timeout /t 20 /nobreak >nul
    if exist "C:\Program Files\Google\Chrome\Application\chrome.exe" (
        echo       Chrome instalado correctamente.
    ) else (
        echo       ATENCION: no se detecto Chrome instalado.
        echo       Segui el PASO 4 de LEEME_INSTALACION.txt para instalarlo a mano.
        pause
    )
)
echo.

echo [6/7] Creando el acceso directo en el Escritorio...
powershell -NoProfile -ExecutionPolicy Bypass -File "%USB%crear_acceso_directo.ps1"
echo.

echo [7/7] Arrancando el sistema por primera vez (crea las tablas y el usuario admin)...
cd /d "%DESTINO%\backend"
start "Jascartec" /min "Jascartec.Api.exe" --urls http://localhost:5080
echo       Esperando que arranque...
timeout /t 15 /nobreak >nul
curl -s -o nul -w "" http://localhost:5080/api/categorias >nul 2>&1
taskkill /IM Jascartec.Api.exe /F >nul 2>&1
echo       Listo.
echo.

echo ============================================
echo   INSTALACION TERMINADA
echo ============================================
echo.
echo  Ahora podes cerrar esta ventana y abrir el acceso
echo  directo "Jascartec" que quedo en el Escritorio.
echo.
echo  Usuario:    admin
echo  Contrasena: admin123
echo  (recomendale al dueno cambiarla apenas entre, desde "Usuarios")
echo.
echo  Si algun paso de arriba dijo "ATENCION", revisa
echo  LEEME_INSTALACION.txt para completarlo a mano, y despues
echo  volve a ejecutar INSTALAR.bat: los pasos ya hechos se saltan
echo  solos y sigue desde donde quedo pendiente.
echo.
pause
