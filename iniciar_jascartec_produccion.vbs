' ============================================================
' iniciar_jascartec.vbs — Lanzador del acceso directo de escritorio
' (versión de INSTALACIÓN REAL, para la laptop del negocio)
' ------------------------------------------------------------
' 1) Si el backend (Jascartec.Api.exe) no está corriendo en el
'    puerto 5080, lo levanta oculto (sin consola negra) y espera
'    a que responda. No necesita el SDK de .NET instalado: es un
'    .exe autocontenido, ya trae todo adentro.
' 2) Abre el sistema en una ventana de Chrome en modo app.
' Requiere que PostgreSQL ya esté instalado y corriendo (queda
' como servicio de Windows, arranca solo con la laptop) y que
' Google Chrome esté instalado.
' ============================================================

Dim proyectoDir, apiExe, indexUrl, chromePath
proyectoDir = "C:\Jascartec"
apiExe = proyectoDir & "\backend\Jascartec.Api.exe"
indexUrl = "file:///C:/Jascartec/app/index.html"
chromePath = "C:\Program Files\Google\Chrome\Application\chrome.exe"

Dim shell
Set shell = CreateObject("WScript.Shell")

If Not PuertoResponde("127.0.0.1", 5080) Then
    shell.CurrentDirectory = proyectoDir & "\backend"
    shell.Run "cmd /c """ & apiExe & """ --urls http://localhost:5080", 0, False

    Dim segundosEsperados, maxSegundos
    segundosEsperados = 0
    maxSegundos = 40
    Do While (Not PuertoResponde("127.0.0.1", 5080)) And segundosEsperados < maxSegundos
        WScript.Sleep 1000
        segundosEsperados = segundosEsperados + 1
    Loop
End If

' --kiosk-printing: al imprimir el ticket de venta, sale directo a la impresora
' predeterminada de Windows sin mostrar la ventanita de "elegir impresora".
shell.Run """" & chromePath & """ --app=""" & indexUrl & """ --window-size=1366,768 --kiosk-printing"

' ---------- Funciones ----------
Function PuertoResponde(host, puerto)
    On Error Resume Next
    Dim http
    Set http = CreateObject("WinHttp.WinHttpRequest.5.1")
    http.SetTimeouts 800, 800, 800, 800
    http.Open "GET", "http://" & host & ":" & puerto & "/", False
    http.Send
    PuertoResponde = (Err.Number = 0)
    On Error Goto 0
End Function
