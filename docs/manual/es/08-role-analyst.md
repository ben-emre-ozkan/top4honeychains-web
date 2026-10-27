# 8. Analista {#role-analyst}

## Su trabajo diario {#analyst-daily}

Usted trabaja en un laboratorio de análisis de alimentos y es la primera de sus dos personas: usted ingresa, el responsable de laboratorio aprueba. Recibe los frascos de muestra precintados que le envían las plantas de procesamiento (uno de cada envase de una entrega), comprueba el precinto, la etiqueta y la tapa, prepara la muestra compuesta, analiza la miel, ingresa los resultados con el PDF del informe y envía el informe a su responsable de laboratorio para que lo apruebe.

Sus tareas aparecen en **Mis tareas**, en **Muestras por recibir** y **Resultados de laboratorio por ingresar**. El menú muestra **Muestras y laboratorio**.

## Recibir una muestra {#analyst-receive}

1. En **Mis tareas**, abra **Muestras por recibir**, o vaya a **Muestras y laboratorio** → **Muestras enviadas a su laboratorio**. Cada frasco muestra su código (`S-{lot}-C{n}`), su envase y su número de precinto.
2. Responda las tres comprobaciones del frasco: **El número de precinto coincide con el registro**, **La etiqueta está intacta** y **La tapa está intacta (botón abajo)**.
3. Si las tres son **Sí**, haga clic en **Registrar la recepción**: el frasco queda recibido.
4. Si alguna comprobación es **No**, escriba el **Motivo si falla una comprobación** y haga clic en **Registrar la recepción**: el frasco queda rechazado. La planta toma un frasco nuevo del mismo envase y se lo envía.

<!-- SHOT: sample-receive -->
Figure: Recepción de frascos de muestra en el laboratorio: precinto, etiqueta y tapa.

Una muestra de una entrega anterior (una muestra por lote) tiene el control único del precinto: **Recibida, precinto intacto**, o el **motivo (si el precinto está roto)** y **Precinto roto: rechazar**.

## Hacer la muestra compuesta {#analyst-composite}

Con el análisis compuesto (el predeterminado), el laboratorio analiza una muestra compuesta por lote (consulte [Muestra compuesta o por envase](#flow-analysis)). Cuando el frasco de cada envase de un lote ha llegado en buen estado, el lote aparece en **Lotes listos para la muestra compuesta**: "El frasco de cada envase de estos lotes llegó en buen estado. Mezcle partes iguales de los frascos en una muestra compuesta y analícela; los frascos se conservan para análisis individuales."

1. Mezcle partes iguales de los frascos.
2. Haga clic en **Hacer la muestra compuesta**. Se crea y se recibe la muestra compuesta `S-{lot}-COMP`; su informe se ingresa como cualquier otro.

Si la muestra compuesta no cumple, o a petición, también puede ingresar un informe para un solo frasco; se queda con ese frasco y no cambia los valores del PCC 1 del lote. Con el análisis por envase no hay muestra compuesta: cada frasco recibe su propio informe, y el PCC 1 del lote toma el peor valor de cada parámetro (consulte [La regla del peor valor](#flow-worst)).

## Ingresar los resultados de laboratorio {#analyst-results}

1. En **Mis tareas**, abra **Resultados de laboratorio por ingresar**. Se abre la página **Informe de laboratorio** de la muestra.
2. Compruebe que la página no diga "El PCC 0 (auditoría primaria) de este lote aún no está completo; …". Mientras el PCC 0 del lote no esté aprobado, el formulario es de solo lectura. Espere a que el responsable de aseguramiento de la calidad de la planta apruebe el PCC 0.
3. Ingrese el **Número del informe** y la **Fecha de emisión** (no puede ser futura).
4. Elija el **PDF del informe** (un PDF de hasta 10 MB).
5. Ingrese el valor de cada parámetro. Los parámetros obligatorios están marcados con *. Los números admiten coma o punto.
6. Haga clic en **Guardar**: "Resultados de laboratorio guardados."

<!-- SHOT: lab-results-form -->
Figure: Ingreso de los resultados de laboratorio con el PDF del informe.

Después de guardar, la plataforma:

- almacena el PDF con su huella SHA-256 (que se muestra con un botón para copiarla);
- genera el **Código del informe** de 12 caracteres, que cualquier persona puede comprobar en la página pública de verificación;
- compara cada valor con su límite (**Dentro del límite** / **Fuera del límite**).

Puede guardar tantas veces como quiera mientras el informe sea un borrador.

## Enviar el informe {#analyst-submit}

1. La página muestra "Valores obligatorios introducidos: … de …"; el recuento sigue lo que usted escribe, y "Faltan:" nombra los valores que siguen vacíos. **Enviar para aprobación** solo se activa cuando todos los valores obligatorios, el PDF, el número del informe y la fecha están **guardados**: cuando todo está escrito pero aún no se ha guardado, la página dice "Guarde los valores antes de enviar."
2. Haga clic en **Enviar para aprobación**: "Informe enviado; un responsable de laboratorio (otra persona) debe aprobarlo."

<!-- SHOT: lab-submit -->
Figure: Todos los datos obligatorios ingresados: el informe se puede enviar.

Una vez enviado, el informe ya no se puede editar, salvo que el responsable de laboratorio lo devuelva. Un informe devuelto muestra el motivo; corríjalo y vuelva a enviarlo.

**Errores frecuentes**

Table: Errores frecuentes al ingresar los resultados de laboratorio {#tbl-lab-errors}

| Mensaje | Qué hacer |
|---|---|
| "Los resultados solo se ingresan para una muestra recibida con el precinto intacto." | Reciba primero el frasco; un frasco rechazado no recibe resultados. |
| "Suba el PDF del informe antes de enviar." | Elija el PDF y guarde. |
| "Introduzca todos los valores obligatorios antes de enviar: … de … introducidos." | Complete los valores que aparecen en "Faltan:". |
| "El informe de laboratorio debe ser un PDF." | Para el informe solo se aceptan archivos PDF. |
| "El informe se envió; el responsable de laboratorio lo aprueba o lo devuelve para modificaciones." | Espere al responsable de laboratorio. |
| "Un informe de laboratorio aprobado no se puede modificar; solicite una corrección en su lugar." | Pida una corrección a un administrador de la plataforma. |
| "El lote está bloqueado en una partida finalizada; los valores de laboratorio solo se pueden modificar mediante una corrección." | El lote quedó sellado en una partida; ya no se puede guardar ni enviar un borrador. Consulte a un administrador de la plataforma. |
