@echo off
setlocal

set "PROC=Home Safety Hotline.exe"

set "run_game=false"

REM Check if the process is running
tasklist /FI "IMAGENAME eq %PROC%" | find /I "%PROC%" >nul
if errorlevel 1 (
    echo %PROC% is not running.
) else (
    echo Killing %PROC%...
    taskkill /F /IM "%PROC%" >nul

    REM Wait loop: keep checking if the process still exists
    :wait_loop
    ping -n 2 127.0.0.1 >nul
    tasklist /FI "IMAGENAME eq %PROC%" | find /I "%PROC%" >nul
    if not errorlevel 1 (
        echo Waiting for %PROC% to terminate...
        goto wait_loop
    )
    echo %PROC% has been terminated.
)


REM Define source and destination paths
set "source_file=%CD%\NewSafetyHelp.dll"
echo Starting copying of mod dll to game folder.
echo Current directory is: %CD%

REM Read the destination folder from the config file
set /p root_destination_folder=<..\..\config.txt

REM Check if the source file exists
if not exist "%source_file%" (
    echo Source file "%source_file%" not found.
    exit /b 1
)

REM Check if the destination folder exists
if not exist "%root_destination_folder%" (
    echo Destination folder "%root_destination_folder%" not found.
    pause
    exit /b 1
)

REM Create Mods Folder
set "mod_folder=\Mods\"
set "destination_folder=%root_destination_folder%%mod_folder%"

REM Check if the mods folder exists
if not exist "%destination_folder%" (
    echo Mods folder "%destination_folder%" not found.
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
     
     REM Run the game if set to true.
     if "%run_game%" == "true" (
         REM --- Starting Game ---
         start "" "%root_destination_folder%\Home Safety Hotline.exe"
     ) else (
        echo Not starting game.
     )
)

echo Finished building.

endlocal
pause