# 7. Aprobador de calidad {#role-qa}

## Su trabajo diario {#qa-daily}

Usted es el segundo par de ojos de su organización. Usted:

- aprueba los registros de PCC que han enviado sus colegas (PCC 0, PCC 2–10 y registros de almacenamiento);
- confirma las solicitudes de finalización, con lo que se sella una partida;
- en un mayorista o una tienda, aprueba los registros de almacenamiento del PCC 10.

Sus tareas aparecen en **Mis tareas**, en **Valores por aprobar** y **Solicitudes de finalización**. Nunca aprueba un registro en el que usted mismo haya registrado valores.

## Aprobar un registro de PCC {#qa-approve}

1. En **Mis tareas**, abra un elemento de **Valores por aprobar**, por ejemplo "PCC 0 · #20070".
2. Revise los valores. Cada valor muestra **Dentro del límite** o **Fuera del límite** respecto de su límite.
3. En la tarjeta **Aprobación (cuatro ojos)**, lea la nota: al aprobar, usted confirma que los valores están registrados correctamente, no que estén dentro de sus límites.
4. Haga clic en **Aprobar**.

<!-- SHOT: ccp-approve -->

El mensaje "Registro de PCC aprobado." lo confirma; la etiqueta de estado muestra **Aprobado** con su nombre y la hora.

**Valores fuera de sus límites.** Si algún valor está fuera del límite, un cuadro de diálogo **Valores fuera de sus límites** los enumera, cada uno con su **Límite:** (y "límite aún no verificado" para los límites que todavía deben verificarse). Haga clic en **Aprobar de todos modos** para aprobar, o en **Cancelar**. Los valores siguen marcados como fuera del límite; la aprobación solo confirma que se registraron correctamente.

<!-- SHOT: out-of-limit-dialog -->

**Registros no aplicables.** Un PCC marcado como "No aplicable" también debe aprobarse, y aparece en **Valores por aprobar** para los aprobadores que no lo marcaron. Lea el motivo antes de aprobarlo.

**Errores frecuentes**

| Mensaje | Significado |
|---|---|
| "Cuatro ojos: usted registró valores de este PCC, por lo que otra persona debe aprobarlo." | Usted ingresó valores en este registro. Debe aprobarlo otro aprobador. |
| "Cuatro ojos: usted marcó este PCC como "No aplicable", así que otra persona debe aprobarlo." | Usted marcó este registro como "No aplicable". Debe aprobarlo otro aprobador. |
| "Solo se puede aprobar un registro de PCC enviado (o "No aplicable")." | Todavía faltan valores obligatorios. |
| "Este registro de PCC ya está aprobado." | No hay nada que hacer. |
| "Su rol (…) no puede aprobar este punto de control; roles que aprueban: …" | Otro rol aprueba este PCC. |

Si su organización tiene la **Exención de la doble verificación (cuatro ojos)**, usted puede aprobar sus propios registros; el mensaje dice entonces "Aprobado con la exención de cuatro ojos (marcado en el registro de auditoría)." La exención nunca se aplica a los resultados de laboratorio.

## Confirmar la finalización (sellar una partida) {#qa-finalize}

1. En **Mis tareas**, abra el elemento de **Solicitudes de finalización**, o abra la partida en **Mi organización**.
2. Revise la partida: los **Puntos de control de la partida** y los **Lotes de origen** están todos aprobados, y "Falta antes de finalizar:" está vacío.
3. Haga clic en **Confirmar y finalizar**.

<!-- SHOT: finalize-confirm -->

La partida queda sellada: "Partida finalizada." Aparece una vez un sello con los enlaces **Abrir la Historia de la miel** y **Descargar QR**. A partir de ese momento:

- la partida y todos sus lotes quedan bloqueados;
- la Historia de la miel pública está disponible en su código QR;
- la partida puede enviarse.

No existe la opción de "deshacer la finalización". Los errores que se detecten después se corrigen mediante el proceso de corrección de los administradores.

Errores: "Cuatro ojos: la persona que solicitó la finalización no puede confirmarla.", "Primero se debe solicitar la finalización.", "La partida todavía no se puede finalizar: …".

<!-- SHOT: batch-sealed -->

## Aprobar registros de almacenamiento (mayorista o tienda) {#qa-storage}

1. En **Mis tareas**, abra el elemento marcado como **almacenamiento** ("PCC 10 · [batch]"), o vaya a **Envíos y existencias** → **Almacenamiento**.
2. Revise los valores y haga clic en **Aprobar** (si se le pide, confirme en el cuadro de diálogo los valores fuera del límite).

Se aplica la misma regla de doble verificación: el colega que registró los valores no puede aprobarlos.
