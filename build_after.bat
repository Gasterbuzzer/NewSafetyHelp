@echo off
setlocal

REM Define source and destination paths
set "source_file=%CD%\NewSafetyHelp.dll"
echo Starting copying of mod dll to game folder.
echo Current directory is: %CD%

REM Read the destination folder from the config file
set /p destination_folder=<..\..\config.txt

REM Check if the source file exists
if not exist "%source_file%" (
    echo Source file "%source_file%" not found.
    exit /b 1
)

REM Check if the destination folder exists
if not exist "%destination_folder%" (
    echo Destination folder "%destination_folder%" not found.
    pause
    exit /b 1
)

REM Copy the file
copy "%source_file%" "%destination_folder%"

REM Check for errors
if errorlevel 1 (
    echo An error occurred during the file copy operation.
) else (
    echo File copied successfully.
)

endlocal
pause