@echo off

if [%1] == [] goto :eof

:enteroutput
set /p output=Name of the TPPK archive to create: 

if [%output%] == [] (
    echo Name cannot be blank.
    goto :enteroutput
) else (
    "%~dp0TppkTool.exe" create "%output%" %*
)
