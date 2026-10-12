# 6. Personal {#role-staff}

## Su trabajo diario {#staff-daily}

**En una planta de procesamiento o de envasado**, usted ingresa los datos de los puntos de control de producción: los PCC 2–3 de los lotes de miel cruda y los PCC 4–10 de las partidas. Cuando una partida está completa, solicita su finalización y, una vez sellada, la envía.

**En un mayorista o un minorista**, usted confirma o rechaza los envíos entrantes y registra las condiciones de almacenamiento (PCC 10) de las partidas que tiene en su poder.

Sus tareas aparecen en **Mis tareas**, en **Valores por ingresar** y **Envíos por confirmar**. El menú muestra **Mi organización** y **Envíos y existencias**.

## Mi organización {#staff-my-organization}

**Mi organización** muestra los lotes de miel cruda y las partidas de su organización. Cada lote y cada partida muestran sus PCC como pequeñas etiquetas. El color indica el estado:

- gris: abierto (sin valores o incompleto);
- amarillo: enviado, a la espera de aprobación;
- contorno verde: completado, pero aún no aprobado;
- verde con ✓: aprobado;
- contorno oscuro: no aplicable.

Haga clic en una etiqueta para abrir la página del PCC. El enlace **Flujo del proceso** muestra la cronología de un lote o de una partida.

<!-- SHOT: my-organization -->
Figure: **Mi organización**: lotes y partidas con sus etiquetas de PCC.

## Ingresar los PCC 2–3 del lote {#staff-lot-ccps}

1. En **Mis tareas**, abra "PCC 2 · #[lot]" (o haga clic en la etiqueta en **Mi organización**).
2. Ingrese los valores como se describe en [Trabajar en una página de PCC](#ccp-page) y haga clic en **Guardar registros de monitoreo**.
3. Si el paso no corresponde a esta miel, márquelo como **No aplicable** con un motivo.
4. Repita lo mismo con el PCC 3.

Después, el responsable de aseguramiento de la calidad los aprueba. Un lote solo puede incluirse en una partida cuando todos sus PCC están completados o son no aplicables y están aprobados.

## Ingresar los PCC 4–10 de la partida {#staff-batch-ccps}

Un administrador de la plataforma crea la partida, agrega los lotes y asigna los PCC 4–10. Entonces la partida aparece en **Mi organización** y sus PCC, en sus **Mis tareas**.

1. Abra cada PCC de la partida (del PCC 4 al PCC 10) e ingrese sus valores.
2. El PCC 10 de la partida corresponde al **almacenamiento en su planta**. Debe ingresarse y aprobarse antes de que la partida pueda sellarse.
3. Haga clic en **Guardar registros de monitoreo** cada vez.

<!-- SHOT: ccp-page-batch -->
Figure: Una página de PCC de una partida: composición de origen y parámetros del PCC 10.

## Solicitar la finalización {#staff-request-finalize}

1. En **Mi organización**, abra la partida.
2. La tarjeta **Finalización** enumera todo lo que aún falta en "Falta antes de finalizar:", por ejemplo "Partida: estado de PCC 10 (Monitoreo de temperatura y humedad): Enviado, aún no aprobado". Mientras falte algo, **Solicitar la finalización** está desactivado.
3. Cuando la lista esté vacía, haga clic en **Solicitar la finalización**.

<!-- SHOT: finalize-request -->
Figure: La tarjeta **Finalización** con la lista de lo que aún falta.

El mensaje "Finalización solicitada; un responsable de aseguramiento de la calidad (otra persona) debe confirmarla." confirma la solicitud. Un responsable de aseguramiento de la calidad de su organización la encuentra en **Solicitudes de finalización**. Usted no puede confirmar su propia solicitud.

## Enviar una partida {#staff-ship}

Solo se puede enviar una partida finalizada.

1. Abra **Envíos y existencias**. **Partidas en su poder** muestra, por partida, la cantidad que tiene, la reservada por envíos pendientes y la disponible.
2. Haga clic en **Enviar** junto a la partida.
3. Elija la organización receptora en **Destinatario**: cualquier otra organización verificada. Ingrese la **Cantidad (kg)** y, si lo desea, una **Nota**.
4. Confirme. La cantidad queda reservada de inmediato: "Envío creado; la custodia se transfiere cuando el receptor lo confirma."

<!-- SHOT: shipment-create -->
Figure: Envío de una partida a otra organización.

Errores: "Solo se puede enviar una partida finalizada.", "Puede enviar como máximo … kg de esta partida.", "Elija otra organización verificada como receptora."

## Confirmar o rechazar un envío (receptor) {#staff-receive}

1. En **Mis tareas**, abra **Envíos por confirmar**, o vaya a **Envíos y existencias** → **Envíos entrantes**.
2. Revise la mercancía. Haga clic en **Confirmar recepción**: "Recepción confirmada; ahora usted tiene esta cantidad en su poder."
3. O bien escriba un **motivo (para rechazar)** y haga clic en **Rechazar**: "Envío rechazado; la cantidad permanece en poder del remitente." El motivo es obligatorio.

<!-- SHOT: shipment-confirm -->
Figure: Un envío entrante en un teléfono: **Confirmar recepción** o **Rechazar**.

## Registrar el almacenamiento (PCC 10 en el receptor) {#staff-storage}

Después de recibir una partida:

1. En **Envíos y existencias**, en la columna **Almacenamiento** de la partida, haga clic en **PCC 10** (página "PCC 10 – Almacenamiento").
2. Ingrese de una vez todos los valores de almacenamiento obligatorios (por ejemplo, temperatura y humedad).
3. Haga clic en **Guardar**: "Valores de almacenamiento guardados." El registro pasa a **Enviado** y su responsable de aseguramiento de la calidad lo encuentra en **Valores por aprobar**.

<!-- SHOT: storage-record -->
Figure: Registro del almacenamiento del PCC 10 en el receptor.

## Leer los registros del productor {#staff-read-only}

Cuando su organización tiene en su poder o ha recibido una partida, usted puede abrir **en modo de solo lectura** los puntos de control del productor de esa partida y de sus lotes: puede leer los datos y quién los aprobó, pero no hay botones para guardar ni aprobar. Los nombres del apicultor y del apiario aparecen como "no se muestra", igual que en el informe de retiro. La página muestra **Solo lectura** con "Este punto de control es responsabilidad de otra organización; solo puede consultarlo." Para una partida que su organización nunca recibió, la plataforma muestra "No puede abrir esta página".

Para abrirlos, vaya a **Envíos y existencias**, haga clic en **Flujo del proceso** junto a la partida que tiene en su poder y haga clic en una etapa del flujo.

## Informe de retiro {#staff-recall}

Desde una partida o un lote puede abrir el **Informe de retiro**. Muestra los lotes aguas arriba y los envíos aguas abajo que su organización puede leer, con los botones **Imprimir** y **CSV**. Consulte [Retiro](#admin-recall).
