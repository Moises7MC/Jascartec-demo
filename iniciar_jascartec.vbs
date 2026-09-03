' ============================================================
' iniciar_jascartec.vbs — Lanzador del acceso directo de escritorio
' ------------------------------------------------------------
' 1) Si el backend (Jascartec.Api) no está corriendo en el puerto
'    5080, lo levanta oculto (sin consola negra) y espera a que
'    responda.
' 2) Abre el sistema en una ventana de Chrome en modo app, igual
'    que antes.
' Requiere que PostgreSQL ya esté corriendo (servicio de Windows,
' normalmente siempre activo) y el SDK de .NET instalado.
' ============================================================

Dim proyectoDir, apiDir, indexUrl, chromePath
proyectoDir = "C:\Users\Moise\OneDrive\Documentos\Moche\Jascartec-demo"
apiDir = proyectoDir & "\backend\src\Jascartec.Api"
indexUrl = "file:///C:/Users/Moise/OneDrive/Documentos/Moche/Jascartec-demo/index.html"
chromePath = "C:\Program Files\Google\Chrome\Application\chrome.exe"

Dim shell
Set shell = CreateObject("WScript.Shell")

If Not PuertoResponde("127.0.0.1", 5080) Then
    shell.CurrentDirectory = apiDir
    shell.Environment("Process")("ASPNETCORE_ENVIRONMENT") = "Development"
    shell.Run "cmd /c dotnet run --urls http://localhost:5080", 0, False

    Dim segundosEsperados, maxSegundos
    segundosEsperados = 0
    maxSegundos = 40
    Do While (Not PuertoResponde("127.0.0.1", 5080)) And segundosEsperados < maxSegundos
        WScript.Sleep 1000
        segundosEsperados = segundosEsperados + 1
    Loop
End If

shell.Run """" & chromePath & """ --app=""" & indexUrl & """ --window-size=1366,768"

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
