@echo off

if [%1] == [] goto :eof
if not exist [%1] (
	echo '%1' does not exist.
	goto :eof
)

set /p output=Name of the folder to extract to, or leave blank to extract to the current folder: 

if [%output%] == [] (
    TppkTool.exe extract "%~1"
) else (
    TppkTool.exe extract --output "%output%" "%~1"
)