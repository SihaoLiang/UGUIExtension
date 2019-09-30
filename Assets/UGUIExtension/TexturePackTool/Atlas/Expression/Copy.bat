set sprite_from=..\..\ui\B±Ì«È\
set sprite_to=.\

cd %~dp0

xcopy %sprite_from%*.png %sprite_to% /e/h

pause