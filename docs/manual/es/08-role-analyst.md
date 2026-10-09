# 8. Analista {#role-analyst}

## Su trabajo diario {#analyst-daily}

Usted trabaja en un laboratorio. Recibe las muestras precintadas que le envían las plantas de procesamiento, controla el precinto, analiza la miel, ingresa los resultados con el PDF del informe y envía el informe a su responsable de laboratorio para que lo apruebe.

Sus tareas aparecen en **Mis tareas**, en **Muestras por recibir** y **Resultados de laboratorio por ingresar**. El menú muestra **Muestras y laboratorio**.

## Recibir una muestra {#analyst-receive}

1. En **Mis tareas**, abra **Muestras por recibir**, o vaya a **Muestras y laboratorio** → **Muestras enviadas a su laboratorio**.
2. Compare el número de precinto de la muestra con el que aparece en pantalla.
3. Si el precinto está intacto, haga clic en **Recibida, precinto intacto**: "Muestra recibida; el precinto está intacto."
4. Si el precinto está roto, escriba el **motivo (si el precinto está roto)** y haga clic en **Precinto roto: rechazar**: "Muestra rechazada: el precinto está roto." La planta debe tomar una muestra nueva.

<!-- SHOT: sample-receive -->

## Ingresar los resultados de laboratorio {#analyst-results}

1. En **Mis tareas**, abra **Resultados de laboratorio por ingresar**. Se abre la página **Informe de laboratorio** de la muestra.
2. Compruebe que la página no diga "El PCC 0 (auditoría primaria) de este lote aún no está completo; …". Mientras el PCC 0 del lote no esté aprobado, el formulario es de solo lectura. Espere a que el aprobador de calidad de la planta apruebe el PCC 0.
3. Ingrese el **Número del informe** y la **Fecha de emisión** (no puede ser futura).
4. Elija el **PDF del informe** (un PDF de hasta 10 MB).
5. Ingrese el valor de cada parámetro. Los parámetros obligatorios están marcados con *. Los números admiten coma o punto.
6. Haga clic en **Guardar**: "Resultados de laboratorio guardados."

<!-- SHOT: lab-results-form -->

Después de guardar, la plataforma:

- almacena el PDF con su huella SHA-256 (que se muestra con un botón para copiarla);
- genera el **Código del informe** de 12 caracteres, que cualquier persona puede comprobar en la página pública de verificación;
- compara cada valor con su límite (**Dentro del límite** / **Fuera del límite**).

Puede guardar tantas veces como quiera mientras el informe sea un borrador.

## Enviar el informe {#analyst-submit}

1. La página muestra "Valores obligatorios introducidos: … de …". **Enviar para aprobación** solo se activa cuando están todos los valores obligatorios, el PDF, el número del informe y la fecha.
2. Haga clic en **Enviar para aprobación**: "Informe enviado; un responsable de laboratorio (otra persona) debe aprobarlo."

<!-- SHOT: lab-submit -->

Una vez enviado, el informe ya no se puede editar, salvo que el responsable de laboratorio lo devuelva. Un informe devuelto muestra el motivo; corríjalo y vuelva a enviarlo.

**Errores frecuentes**

| Mensaje | Qué hacer |
|---|---|
| "Los resultados solo se ingresan para una muestra recibida con el precinto intacto." | Reciba primero la muestra. |
| "Suba el PDF del informe antes de enviar." | Elija el PDF y guarde. |
| "Introduzca todos los valores obligatorios antes de enviar: … de … introducidos." | Complete los valores que aparecen en "Faltan:". |
| "El informe de laboratorio debe ser un PDF." | Para el informe solo se aceptan archivos PDF. |
| "El informe se envió; el responsable de laboratorio lo aprueba o lo devuelve para modificaciones." | Espere al responsable de laboratorio. |
| "Un informe de laboratorio aprobado no se puede modificar; solicite una corrección en su lugar." | Pida una corrección a un administrador de la plataforma. |
| "El lote está bloqueado en una partida finalizada; los valores de laboratorio solo se pueden modificar mediante una corrección." | El lote quedó sellado en una partida; ya no se puede guardar ni enviar un borrador. Consulte a un administrador de la plataforma. |
