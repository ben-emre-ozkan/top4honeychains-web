# 5. Responsable de recepción de muestras {#role-sao}

## Su trabajo diario {#sao-daily}

Usted trabaja en la recepción de una planta de procesamiento. Cuando un apicultor entrega miel, usted:

1. registra la entrega (esto crea el lote de miel cruda);
2. precinta una muestra y una contramuestra, y envía la muestra a un laboratorio;
3. ingresa el PCC 0, la auditoría primaria del lote, para que un aprobador de calidad pueda aprobarlo.

Sus tareas aparecen en **Mis tareas**, en **Valores por ingresar**. El menú muestra **Entregas (recepción)** y **Muestras y laboratorio**.

## Registrar una entrega {#sao-intake}

1. Abra **Entregas (recepción)** y haga clic en **Registrar una entrega**.
2. En **Número de explotación o de registro del apicultor**, escriba exactamente el número de explotación o el número de registro verificado del apicultor, y haga clic en **Buscar**. Si no se encuentra nada: "No hay ningún apicultor activo con este número." Revise el número; los apicultores los registra un administrador de la plataforma.
3. Complete el formulario:
   - **Apiario**: uno de los apiarios del apicultor.
   - **Tipo de miel**.
   - **Período de producción**: el período actual aparece preseleccionado.
   - **País de origen**.
   - **Fecha de entrega**: por defecto, la de hoy; no puede ser una fecha futura.
   - **Cantidad (kg)**: se acepta la coma o el punto como separador decimal.
   - **Provincia** y **Distrito** de origen: cuando es posible, la provincia se completa a partir del número de registro.
   - **Foto del formulario de entrega firmado**: tome una foto con el botón de la cámara o elija un archivo (PDF de hasta 10 MB, o JPG, PNG o WEBP de hasta 8 MB).
   - **Enviar un SMS al apicultor** (opcional; en la versión actual el mensaje se registra, pero no se envía).
4. Haga clic en **Registrar entrega**.

<!-- SHOT: intake-form -->

El mensaje "Entrega registrada como lote n.º … ." confirma el nuevo lote. La plataforma asigna los PCC 0–3 al lote y abre **Tomar una muestra** para él.

**Errores frecuentes**

| Mensaje | Qué hacer |
|---|---|
| "Elija uno de los apiarios del apicultor." | Seleccione un apiario de la lista. |
| "Introduzca la fecha de entrega (no puede ser futura)." | Corrija la fecha. |
| "La cantidad debe ser mayor que 0." | Ingrese la cantidad en kilogramos. |
| "Elija un distrito de la provincia seleccionada." | Elija primero la provincia y después un distrito de su lista. |
| "Adjunte la foto del formulario de entrega firmado." | El formulario firmado es obligatorio. |
| "Este apiario ya tiene un registro de producción para el período y el tipo de miel seleccionados." | Un apiario entrega una sola vez por período y tipo de miel. Revise el período y el tipo de miel. |
| "Solo una organización de procesamiento registra entregas." / "Su rol (…) no puede registrar entregas; roles: …" | Su organización o su rol no pueden registrar entregas. Consulte al administrador de su organización. |

La lista **Entregas (recepción)** muestra cada entrega con su número de lote, fecha, apiario, tipo de miel, cantidad, origen y el formulario firmado (con los primeros caracteres de su huella SHA-256). Desde cada fila puede abrir **Muestra** y **Retiro**.

## Tomar y enviar una muestra {#sao-sample}

1. Después de registrar la entrega se abre la página **Tomar una muestra**. También puede abrirla desde **Muestras y laboratorio** o desde la fila de la entrega.
2. Elija el **Laboratorio**: solo aparecen los laboratorios verificados; un laboratorio acreditado muestra su número de acreditación.
3. Ingrese el **Número de precinto** de la muestra y el **Número de precinto de la contramuestra**. Deben ser distintos.
4. Ingrese **Dónde se conserva la contramuestra** y **Conservar hasta** (por defecto, 12 meses más tarde).
5. Haga clic en **Enviar al laboratorio**, o en **Más tarde** para enviarla en otro momento.

<!-- SHOT: sample-form -->

La muestra recibe un código como `S-20070-1` y el estado **Enviada**. El laboratorio la ve en **Muestras por recibir**. Si el laboratorio encuentra el precinto roto, la muestra se rechaza; tome una muestra nueva del mismo lote.

Errores: "Elija un laboratorio verificado.", "La muestra y la contramuestra necesitan precintos diferentes.", "Indique dónde se conserva la contramuestra (máximo 200 caracteres).", "Indique hasta cuándo se conserva la contramuestra (una fecha futura).", "Solo la organización que tiene el lote en su poder toma sus muestras."

## Trabajar en una página de PCC {#ccp-page}

Todos los roles que ingresan o aprueban valores de PCC usan la misma página. Ábrala desde **Mis tareas**, desde las etiquetas de PCC de **Mi organización** o desde la página **Flujo del proceso** de un lote o de una partida.

<!-- SHOT: ccp-page-lot -->

La página muestra:

- el lote (apicultor, apiario, tipo de miel, fecha de producción) o la partida (partida, cantidad total, mayorista, composición de origen);
- la etiqueta de **Estado** y quién ingresó y aprobó el registro;
- la clase de miel que se usa para los límites ("Límites para la clase de miel: …");
- los parámetros, agrupados, cada uno con su campo, su **Límite crítico:** y, después de guardar, **Dentro del límite** o **Fuera del límite**;
- un aviso **Solo lectura** con el motivo cuando usted no puede ingresar valores.

Tipos de campo:

- **Número**: coma o punto como separador decimal.
- **Sí / No**: elija en la lista.
- **Texto**: como máximo 100 caracteres.
- **Archivo**: suba un documento (PDF de hasta 10 MB, o una imagen de hasta 8 MB). Marque **Quitar** para eliminarlo.
- **Notas**: una observación opcional, de 500 caracteres como máximo.

Algunos parámetros solo son obligatorios cuando otra pregunta se responde de una forma determinada, por ejemplo "Obligatorio solo si «…» se responde Sí."

**Para ingresar valores**

1. Complete los parámetros.
2. Haga clic en **Guardar registros de monitoreo**. El mensaje "PCC … valores guardados." confirma que se guardaron, y usted vuelve a **Mi organización**.

No hay un botón de envío aparte. Cuando todos los parámetros obligatorios tienen valor, el registro pasa a **Enviado** y aparece en **Mis tareas** de los aprobadores. Con solo algunos valores, queda **En curso**.

**Si el PCC no corresponde** (por ejemplo, la miel no se calienta):

1. Mientras el registro no tenga valores, abra "Este PCC no se aplica aquí (marcar como "No aplicable")".
2. Escriba el **Motivo**, por ejemplo "La miel cruda no se calienta ni se pasteuriza".
3. Haga clic en **Marcar como "No aplicable"**. El motivo, su nombre de usuario y la hora quedan registrados, y en la Historia de la miel pública aparece "No aplicable".

Para deshacerlo, haga clic en **Quitar "No aplicable"**. Un PCC no aplicable también necesita aprobación.

**Errores frecuentes**

| Mensaje | Significado |
|---|---|
| "…: introduzca un número." / "…: elija Sí o No." | Un valor tiene un tipo incorrecto. |
| "This CCP is marked "Not applicable". Remove that mark first to record values." (Este PCC está marcado como "No aplicable". Quite primero esa marca para registrar valores). | Quite la marca antes de ingresar valores. |
| "Este punto de control es responsabilidad de otra organización; solo puede consultarlo." | Usted ve este PCC en modo de solo lectura. |
| "Su rol (…) no puede ingresar datos en este punto de control; roles que ingresan: …" | Otro rol ingresa este PCC. |
| "Su organización está en estado Pendiente; solo las organizaciones verificadas pueden ingresar datos." | Espere la verificación. |
| "Este lote de miel cruda se utiliza en la partida finalizada … y está bloqueado. …" | El lote está sellado; solo una corrección puede modificarlo. |

## Ingresar el PCC 0 (auditoría primaria) {#sao-ccp0}

1. En **Mis tareas**, en **Valores por ingresar**, abra el elemento "PCC 0 · #[lot]".
2. Ingrese los controles de recepción (los parámetros del PCC 0). Algunas preguntas, si se responden **Sí**, abren otras preguntas obligatorias.
3. Haga clic en **Guardar registros de monitoreo**.

Cuando están todos los valores obligatorios, el PCC 0 pasa a **Enviado** y el aprobador de calidad de su organización lo ve en **Valores por aprobar**. Usted no puede aprobar su propio PCC 0 ("Cuatro ojos: usted registró valores de este PCC, por lo que otra persona debe aprobarlo."). El laboratorio solo puede ingresar resultados después de que se apruebe el PCC 0.

<!-- SHOT: ccp0-submitted -->
