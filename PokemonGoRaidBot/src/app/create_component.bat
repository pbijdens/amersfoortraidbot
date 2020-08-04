rem Usage: create_component <module>\<component>
rem Please prefix the components with the module-name, so create_component users\users-dashboard
%~dp0\..\..\node_modules\.bin\ng generate component %1
