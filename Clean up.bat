@echo off
rmdir /S /Q Launcher\Debug
rmdir /S /Q Launcher\Release
rmdir /S /Q Network_Manager\obj
rmdir /S /Q UpdateVersion\obj
rmdir /S /Q Lib\obj
rmdir /S /Q Debug
rmdir /S /Q Release

echo Done.
pause>nul