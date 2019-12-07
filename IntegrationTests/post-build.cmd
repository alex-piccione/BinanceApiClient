:: Copy (overwrite) the secret API key and settings (not part of the repository) into the build folder
set ProjectDir=%~1
set TargetDir=%~2

::copy "%ProjectDir%settings.secret.yaml" "%TargetDir%"
IF EXIST "%ProjectDir%settings.secret.yaml" (copy /Y "%ProjectDir%settings.secret.yaml" "%TargetDir%settings.yaml")