set sprite_from=..\..\..\ui\UI��\Zս��\HUD\Num\
set sprite_to=.\

cd %~dp0
del %sprite_to%*.png
copy %sprite_from%*.png %sprite_to%

pause