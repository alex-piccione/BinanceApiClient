SET ProjectDir=%~1
SET TargetDir=%~2

IF EXIST "%ProjectDir%settings.secret.yaml" (copy "%ProjectDir%settings.secret.yaml" "%TargetDir%settings.yaml")