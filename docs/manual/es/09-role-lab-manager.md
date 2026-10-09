# 9. Responsable de laboratorio {#role-lab-manager}

## Su trabajo diario {#labmanager-daily}

Usted aprueba o devuelve los informes de laboratorio que envían sus analistas. También puede recibir muestras. Nunca aprueba un informe cuyos resultados haya ingresado usted mismo; para los resultados de laboratorio no hay exención de esta regla.

Sus tareas aparecen en **Mis tareas**, en **Informes de laboratorio por aprobar** (y en **Muestras por recibir**).

## Aprobar o devolver un informe {#labmanager-decide}

1. En **Mis tareas**, abra **Informes de laboratorio por aprobar**.
2. Revise el informe: el PDF, el número del informe, la fecha de emisión y cada valor. Los valores fuera del límite aparecen marcados.
3. Para aprobarlo, haga clic en **Aprobar**. Si hay valores fuera del límite, el cuadro de diálogo **Valores fuera de sus límites** le pide confirmación; haga clic en **Aprobar de todos modos**.
4. Para devolver el informe, escriba el **motivo (para devolver)** y haga clic en **Devolver**: "Informe devuelto al analista."

<!-- SHOT: lab-approve -->

Al aprobar, usted confirma que el informe está registrado correctamente, no que la miel cumpla los requisitos; los valores fuera de sus límites siguen marcados en el lote.

**Qué hace la aprobación** ("Informe de laboratorio aprobado; los resultados son ahora los valores del PCC 1 del lote."):

- El informe pasa a **Aprobado** y se puede consultar en la página pública de verificación.
- La plataforma registra si el laboratorio estaba **acreditado** en ese momento (con su número de acreditación) y si es un laboratorio **propio** del propietario del lote.
- Los resultados pasan a ser los valores del **PCC 1** del lote. Ya no se pueden editar; solo los administradores pueden corregirlos. Si el laboratorio cubrió todos los parámetros del PCC 1, el PCC 1 queda completado y aprobado de inmediato; si no, pasa al aprobador de calidad de la planta.

**Errores frecuentes**

| Mensaje | Significado |
|---|---|
| "Cuatro ojos: la persona que ingresó los resultados no puede aprobarlos (no hay exención para los resultados de laboratorio)." | Debe aprobarlo otro responsable de laboratorio. |
| "Se requiere un motivo para devolver un informe." | Escriba el motivo. |
| "El archivo del informe almacenado no coincide con su hash SHA-256; vuelva a subirlo." | El PDF almacenado cambió; el analista debe volver a subirlo. |
| "El PCC 0 (auditoría primaria) de este lote aún no está completo; …" | La aprobación espera al PCC 0. Aun así, puede devolver el informe. |

## Verificar un informe públicamente {#labmanager-verify}

Cualquier persona puede comprobar un informe en **Verificar un informe de laboratorio** (`/verify-report`): ingrese el **Código del informe** y haga clic en **Consultar**. La página muestra si el informe está aprobado, el laboratorio, su acreditación, el número del informe, la fecha y la huella SHA-256. **Comparar un PDF con esta huella (el archivo permanece en su dispositivo)** comprueba un PDF en el propio dispositivo del visitante (el archivo no se sube). No se muestran datos del lote, del apicultor ni de usuarios.

<!-- SHOT: verify-report -->
