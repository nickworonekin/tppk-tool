@echo off

if "%~1" == "" goto :eof

:enteroutput
set /p output=Name of the TPPK archive to create, including extension, or name of the NARC archive to update: 

if "%output%" == "" (
    echo Name cannot be blank.
    goto :enteroutput
) else (
    "%~dp0TppkTool.exe" create "%output%" %*
)

if %errorlevel% neq 0 goto :error

goto :eof

:error
pause
