# 5. Responsable de recepción de muestras {#role-sao}

## Su trabajo diario {#sao-daily}

Usted trabaja en la recepción de una planta de procesamiento. Cuando un apicultor entrega miel, usted:

1. registra la entrega envase por envase, con los precintos, y llena un frasco de muestra y un frasco de contramuestra de cada envase;
2. deja que el apicultor confirme la entrega con el código que recibe por SMS;
3. imprime las etiquetas y envía los frascos de muestra al laboratorio;
4. ingresa el PCC 0, la auditoría primaria del lote, para que un responsable de aseguramiento de la calidad pueda aprobarlo.

Sus tareas aparecen en **Mis tareas**, en **Valores por ingresar**. El menú muestra **Entregas (recepción)** y **Muestras y laboratorio**.

## Registrar una entrega {#sao-intake}

"Registre cada envase de la entrega con su precinto, tome una muestra de cada envase y luego el apicultor lo confirma todo con un código SMS."

1. Abra **Entregas (recepción)** y haga clic en **Registrar una entrega**.
2. En **Número de explotación o de registro del apicultor**, escriba exactamente el número de explotación o el número de registro verificado del apicultor, y haga clic en **Buscar**. Si no se encuentra nada: "No hay ningún apicultor activo con este número." Revise el número. A los apicultores los registra el responsable de registro de apicultores; un apicultor registrado con una foto firmada solo puede entregar después de que el Responsable de cumplimiento haya aceptado la foto. "sin móvil: solo foto firmada" significa que el apicultor no puede recibir un código SMS.
3. **La entrega**: elija el **Apiario**, el **Tipo de miel** y el **Período de producción** (el actual aparece preseleccionado), el **País de origen** y la **Fecha de entrega** (por defecto, la de hoy; no puede ser una fecha futura).
4. **Envases y muestras**: una fila por envase ([Figure](#fig-intake-form)).
   - **Tipo** de envase, **Kg netos** y, opcionalmente, **Kg brutos** (se acepta la coma o el punto como separador decimal).
   - **A la llegada**: marque **llegó precintado** e ingrese su **Número de precinto**, marcando **precinto intacto** cuando lo esté; para un envase sin precinto, ingrese el número del **Precinto de planta aplicado ahora**, delante del apicultor.
   - Abra el envase y llene el frasco de muestra y el frasco de contramuestra. Ingrese el **Precinto del frasco** y el **Precinto de contramuestra**.
   - Marque **Tapa intacta** solo cuando el botón de la tapa esté abajo. "Tapa de seguridad: marque solo si el botón está abajo. Una tapa saltada indica que el frasco se abrió: deséchelo y llene uno nuevo."
   - Ingrese el **Precinto nuevo tras el muestreo** que vuelve a cerrar el envase.
   - **Añadir un envase** añade una fila; **Quitar la fila** quita una. **Total neto** suma los envases.
5. **Laboratorio y contramuestras**: elija el **Laboratorio** (solo aparecen los laboratorios verificados; uno acreditado muestra su número de acreditación), **Dónde se guardan las contramuestras** y **Conservar hasta** (por defecto, 12 meses más tarde).
6. **El apicultor confirma**: haga clic en **Enviar código**. El apicultor recibe un SMS con su organización, la fecha, los envases, los kilos, el tipo de miel, el número de muestras y un código. Ingrese el **Código del SMS (6 dígitos)** que el apicultor lee en voz alta y haga clic en **Confirmar la entrega**. "Si el apicultor no está presente, llámele: leerá el código de su teléfono."

<!-- SHOT: intake-form -->
Figure: Registro de una entrega en un teléfono: envases, precintos y frascos de muestra.

El mensaje "Entrega confirmada por el apicultor y registrada como lote #… ." confirma el nuevo lote. La plataforma asigna los PCC 0–3 al lote, registra cada precinto como un evento de precinto y envía un frasco de muestra por envase al laboratorio elegido.

**El código.** El código es válido durante 5 minutos y se puede probar 3 veces; **Enviar un código nuevo** envía otro al cabo de un minuto. Si cambia algo después de enviar el código, este queda anulado ("Los datos cambiaron después de enviar el código. Envíe un código nuevo."). Tras 10 códigos erróneos en una hora debe esperar 15 minutos, y un mismo responsable envía como máximo 30 códigos por hora.

**Sin SMS posible: foto firmada.** Si el apicultor no tiene teléfono móvil, o el SMS no se puede enviar, abra **Sin SMS posible: foto firmada**, imprima el formulario de entrega, deje que el apicultor lo firme, elija la **Foto del formulario de entrega firmado** (cámara o archivo: PDF de hasta 10 MB, o JPG, PNG o WEBP de hasta 8 MB), escriba **Por qué no hay código SMS** y haga clic en **Registrar con la foto firmada**: "Entrega registrada como lote #… con la foto firmada; espera al Responsable de cumplimiento." Se puede trabajar con el lote, pero no se puede finalizar hasta que el Responsable de cumplimiento de la planta acepte la foto.

**Errores frecuentes**

Table: Errores frecuentes al registrar una entrega {#tbl-intake-errors}

| Mensaje | Qué hacer |
|---|---|
| "Registre al menos un envase." | Añada una fila por cada envase. |
| "Envase …: introduzca el peso neto (más de 0 kg)." / "…: el peso bruto no puede ser menor que el neto." | Corrija los pesos de ese envase. |
| "Envase …: introduzca el número del precinto con el que llegó." / "…: aplique un precinto de planta delante del apicultor e introduzca su número." | Cada envase necesita su precinto de llegada o un precinto de la planta. |
| "Envase …: introduzca los precintos del frasco de muestra y del de contramuestra." | Los dos frascos de cada envase van precintados. |
| "Envase …: la tapa del frasco debe estar intacta (botón abajo) al precintar. Tapa saltada: deseche el frasco y llene uno nuevo." | Llene un frasco nuevo con la tapa intacta. |
| "Envase …: introduzca el nuevo precinto de planta aplicado tras el muestreo." | Cierre el envase con un precinto nuevo. |
| "Cada número de precinto de la entrega debe ser distinto." | Un número de precinto se usa dos veces; revise los números. |
| "Elija uno de los apiarios del apicultor." / "Introduzca la fecha de entrega (no puede ser futura)." | Corrija los campos de la entrega. |
| "Este apiario ya tiene un registro de producción para el período y el tipo de miel seleccionados." | Un apiario entrega una sola vez por período y tipo de miel. Revise el período y el tipo de miel. |
| "Código incorrecto. Quedan … intentos." / "El código ha caducado. Envíe un código nuevo." | Pida al apicultor que vuelva a leer el código, o envíe uno nuevo. |
| "Indique por qué no se puede usar el código SMS." / "Adjunte la foto del formulario de entrega firmado." | El camino de la foto firmada necesita ambas cosas. |
| "Solo una organización de procesamiento registra entregas." / "Su rol (…) no puede registrar entregas; roles: …" | Su organización o su rol no pueden registrar entregas. Consulte al administrador de su organización. |

La lista **Entregas (recepción)** muestra cada entrega con su número de lote, fecha, apiario, tipo de miel, cantidad, origen y cómo se confirmó. Desde cada fila puede abrir la página de la entrega, **Muestra** y **Retiro**.

## La página de la entrega y las etiquetas {#sao-labels}

La página de la entrega muestra el lote, los envases con su **Historial de precintos**, los **Frascos de muestra** y su estado en el laboratorio, cómo se confirmó la entrega (**Confirmado por**: código SMS o foto firmada) y la comprobación del **Contenido confirmado**: "sin cambios desde la confirmación" o, después de una corrección, el contenido corregido.

**Etiquetas (hoja A4)** imprime todas las etiquetas de la entrega en hojas A4 (2 × 5); **Etiquetas (100 × 50 mm)** imprime una etiqueta por página para una impresora de etiquetas. Hay una etiqueta por envase (`L{lot}-C{n}`), por frasco de muestra (`S-{lot}-C{n}`) y por contramuestra (`S-{lot}-C{n}-K`), cada una con un código QR y el número de precinto; la etiqueta de la contramuestra muestra además **conservar hasta**.

<!-- SHOT: intake-labels -->
Figure: Etiquetas de una entrega: envase, frasco de muestra y contramuestra.

## Tomar un frasco nuevo cuando el laboratorio rechaza uno {#sao-resample}

Si el laboratorio rechaza un frasco (número de precinto, etiqueta o tapa no conformes), la página de la entrega muestra en ese frasco "El laboratorio rechazó el frasco: tome uno nuevo". Vuelva a abrir el envase, llene un frasco de muestra y un frasco de contramuestra nuevos, ingrese sus precintos, marque **Tapa intacta** e ingrese el **Precinto nuevo tras el muestreo**, y haga clic en **Abrir, muestrear y precintar de nuevo**. El frasco nuevo (`S-{lot}-C{n}-2`, …) va al mismo laboratorio; el historial de precintos muestra que el envase se abrió y se volvió a precintar.

## Tomar una muestra de una entrega anterior {#sao-sample}

Las entregas registradas antes del procedimiento con envases ("anterior (envases no registrados)") tienen una sola muestra, que se toma con la página **Tomar una muestra**:

1. Abra **Tomar una muestra** desde **Muestras y laboratorio** o desde la fila de la entrega.
2. Elija el **Laboratorio**: solo aparecen los laboratorios verificados.
3. Ingrese el **Número de precinto** de la muestra y el **Número de precinto de la contramuestra**. Deben ser distintos.
4. Ingrese **Dónde se conserva la contramuestra** y **Conservar hasta** (por defecto, 12 meses más tarde).
5. Haga clic en **Enviar al laboratorio**, o en **Más tarde** para enviarla en otro momento.

<!-- SHOT: sample-form -->
Figure: Toma de una muestra de una entrega anterior: laboratorio y números de precinto.

La muestra recibe un código como `S-20070-1` y el estado **Enviada**. Si el laboratorio encuentra el precinto roto, la muestra se rechaza; tome una muestra nueva del mismo lote.

Errores: "Elija un laboratorio verificado.", "La muestra y la contramuestra necesitan precintos diferentes.", "Indique dónde se conserva la contramuestra (máximo 200 caracteres).", "Indique hasta cuándo se conserva la contramuestra (una fecha futura).", "Solo la organización que tiene el lote en su poder toma sus muestras.", "Solo una organización de procesamiento toma muestras."

## Trabajar en una página de PCC {#ccp-page}

Todos los roles que ingresan o aprueban datos de PCC usan la misma página ([Figure](#fig-ccp-page-lot)). Ábrala desde **Mis tareas**, desde las etiquetas de PCC de **Mi organización** o desde la página **Flujo del proceso** de un lote o de una partida.

<!-- SHOT: ccp-page-lot -->
Figure: Una página de PCC de un lote: una respuesta **Sí** abre una pregunta adicional obligatoria.

La página muestra:

- el lote (apicultor, apiario, tipo de miel, fecha de producción) o la partida (partida, cantidad total, mayorista, composición de origen);
- la etiqueta de **Estado** y quién ingresó y aprobó el registro;
- la clase de miel que se usa para los límites ("Límites para la clase de miel: …");
- los parámetros, agrupados, cada uno con su campo, su **Límite crítico:** y, después de guardar, **Dentro del límite** o **Fuera del límite**;
- un aviso **Solo lectura** con el motivo cuando usted no puede ingresar datos.

Tipos de campo:

- **Número**: coma o punto como separador decimal.
- **Sí / No**: elija en la lista.
- **Texto**: como máximo 100 caracteres.
- **Archivo**: suba un documento (PDF de hasta 10 MB, o una imagen de hasta 8 MB). Marque **Quitar** para eliminarlo.
- **Notas**: una observación opcional, de 500 caracteres como máximo.

Algunos parámetros solo son obligatorios cuando otra pregunta se responde de una forma determinada, por ejemplo "Obligatorio solo si «…» se responde Sí."

**Para ingresar datos**

1. Complete los parámetros.
2. Haga clic en **Guardar registros de monitoreo**. El mensaje "PCC … valores guardados." confirma que se guardaron, y usted vuelve a **Mi organización**.

No hay un botón de envío aparte. Cuando todos los parámetros obligatorios tienen valor, el registro pasa a **Enviado** y aparece en **Mis tareas** de los aprobadores. Con solo algunos valores, queda **En curso**.

**Si el PCC no corresponde** (por ejemplo, la miel no se calienta):

1. Mientras el registro no tenga valores, abra "Este PCC no se aplica aquí (marcar como "No aplicable")".
2. Escriba el **Motivo**, por ejemplo "La miel cruda no se calienta ni se pasteuriza".
3. Haga clic en **Marcar como "No aplicable"**. El motivo, su nombre de usuario y la hora quedan registrados, y en la Historia de la miel pública aparece "No aplicable".

Para deshacerlo, haga clic en **Quitar "No aplicable"**. Un PCC no aplicable también necesita aprobación, por una persona distinta de la que lo marcó.

**Errores frecuentes**

Table: Errores frecuentes en una página de PCC {#tbl-ccp-errors}

| Mensaje | Significado |
|---|---|
| "…: introduzca un número." / "…: elija Sí o No." | Un valor tiene un tipo incorrecto. |
| "Este PCC está marcado como "No aplicable". Quite primero esa marca para registrar valores." | Quite la marca antes de ingresar valores. |
| "Este punto de control es responsabilidad de otra organización; solo puede consultarlo." | Puede leer este PCC, pero no modificarlo. |
| "Su rol (…) no puede ingresar datos en este punto de control; roles que ingresan: …" | Otro rol ingresa este PCC. |
| "Su organización está en estado Pendiente; solo las organizaciones verificadas pueden ingresar datos." | Espere la verificación. |
| "Este lote de miel cruda se utiliza en la partida finalizada … y está bloqueado. …" | El lote está sellado; solo una corrección puede modificarlo. |

## Ingresar el PCC 0 (auditoría primaria) {#sao-ccp0}

1. En **Mis tareas**, en **Valores por ingresar**, abra el elemento "PCC 0 · #[lot]".
2. Ingrese los controles de recepción (los parámetros del PCC 0). Algunas preguntas, si se responden **Sí**, abren otras preguntas obligatorias.
3. Haga clic en **Guardar registros de monitoreo**.

Cuando están todos los datos obligatorios, el PCC 0 pasa a **Enviado** ([Figure](#fig-ccp0-submitted)) y el responsable de aseguramiento de la calidad de su organización lo encuentra en **Valores por aprobar**. Usted no puede aprobar su propio PCC 0 ("Cuatro ojos: usted registró valores de este PCC, por lo que otra persona debe aprobarlo."). El laboratorio solo puede ingresar resultados después de que se apruebe el PCC 0.

<!-- SHOT: ccp0-submitted -->
Figure: PCC 0 enviado y a la espera de la aprobación de otra persona.
