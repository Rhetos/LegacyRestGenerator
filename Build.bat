SETLOCAL
SET Version=1.9.0
SET Prerelease=auto

CALL Tools\Build\FindVisualStudio.bat || GOTO Error0

REM Updating the build version.
PowerShell -ExecutionPolicy ByPass .\ChangeVersion.ps1 %Version% %Prerelease% || GOTO Error0

WHERE /Q NuGet.exe || ECHO ERROR: Please download the NuGet.exe command line tool. && GOTO Error0
NuGet restore -NonInteractive || GOTO Error0
MSBuild /target:rebuild /p:Configuration=Debug /verbosity:minimal /fileLogger || GOTO Error0
IF NOT EXIST Install md Install
NuGet pack -OutputDirectory Install || GOTO Error0

REM Updating the build version back to "dev" (internal development build), to avoid spamming git history with timestamped prerelease versions.
PowerShell -ExecutionPolicy ByPass .\ChangeVersion.ps1 %Version% dev || GOTO Error0

@REM ================================================

@ECHO.
@ECHO %~nx0 SUCCESSFULLY COMPLETED.
@EXIT /B 0

:Error0
@ECHO.
@ECHO %~nx0 FAILED.
@IF /I [%1] NEQ [/NOPAUSE] @PAUSE
@EXIT /B 1
