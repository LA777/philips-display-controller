@echo off
rem Set ALL displays brightness to 10%.
set "PDC=%~dp0pdc\bin\Release\net10.0-windows\win-x64\publish\pdc.exe"
if not exist "%PDC%" set "PDC=pdc.exe"
"%PDC%" --brightness all:10
