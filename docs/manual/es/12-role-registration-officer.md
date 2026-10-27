# 12. Responsable de registro de apicultores {#role-ro}

## Su trabajo diario {#ro-daily}

Usted trabaja en una dirección distrital de agricultura, una organización con la capacidad **Registro de apicultores**. Registra a los apicultores de su zona y sus apiarios, para que las plantas puedan registrar sus entregas. Para cada apicultor, usted:

1. comprueba el número de registro de la explotación;
2. registra al apicultor y su primer apiario;
3. deja que el apicultor lea el aviso informativo y registra sus elecciones de consentimiento;
4. confirma el registro con el código que el apicultor recibe por SMS, o con un formulario firmado y fotografiado;
5. envía al apicultor los datos de acceso al portal del apicultor, cuando el portal está abierto.

El menú muestra **Registro de apicultores**. Solo lee y encuentra los registros de su propia dirección. Un administrador de la plataforma puede registrar apicultores de la misma manera (para la organización propia de la plataforma).

## Buscar o registrar un apicultor {#ro-find}

1. Abra **Registro de apicultores**. La lista muestra los apicultores registrados por su organización, con la **Fecha**, el **Número de registro**, cómo se confirmó el registro (**Confirmado por**: código SMS o foto firmada) y el estado de la **Prueba**.
2. En **Número de registro (AKS) o número de explotación**, ingrese el número y haga clic en **Buscar o registrar**. "Un apicultor conocido se verifica de nuevo; un número de registro nuevo inicia un registro nuevo."

Si el apicultor pertenece a otra dirección: "Este apicultor está registrado por otra dirección."

<!-- SHOT: registration-list -->
Figure: **Registro de apicultores**: los registros de una dirección.

## Registrar un apicultor {#ro-register}

El formulario **Registrar un apicultor** (o **Verificar de nuevo a un apicultor** para uno ya conocido) tiene cuatro partes ([Figure](#fig-registration-form)).

1. **Número de registro**: `TR`, el código de matrícula de la provincia y 10 dígitos, por ejemplo `TR281234500004` ("TR, el código de provincia y 10 dígitos. Se compara con la provincia de la dirección."). Si el número pertenece a otra provincia, escriba el motivo en **Si el número es de otra provincia: el motivo**.
2. **El apicultor**: **Nombre**, **Apellido**, **Fecha de nacimiento**, **Teléfono móvil**, el **Idioma del SMS** y, de forma opcional, el nivel de educación, el propósito y el tipo de apicultura. Marque **El apicultor no tiene móvil (foto firmada, sin cuenta)** si no lo tiene.
3. **Colmenar**: el lugar del primer apiario (provincia, distrito y localidad, que también dan al apiario su título), su posición y el número de colmenas. Más adelante se pueden añadir otros apiarios en la página del registro.
4. **Aviso informativo y consentimiento**: "Deje que el apicultor lea este aviso o léalo en voz alta". Después, registre las elecciones del apicultor:
   - **El apicultor acepta que su nombre aparezca en las páginas públicas (opcional; si no, se muestra un código neutro)**;
   - **El apicultor acepta avisos por SMS sobre entregas y resultados de laboratorio (opcional)**.
   **Textos completos de los consentimientos** abre los textos.
5. **Su declaración**: marque **He visto el documento de identidad del apicultor y el registro.** Marque **Enviar también al apicultor los datos de acceso por SMS** si el apicultor debe recibir ya una cuenta del portal (solo aparece mientras el portal está abierto).

<!-- SHOT: registration-form -->
Figure: Registro de un apicultor: número de registro, identidad, apiario, aviso y consentimientos.

### Confirmar con el código SMS {#ro-sms}

1. En **Confirmación**, haga clic en **Enviar código**. "El apicultor recibe un SMS con su organización, el enlace al aviso, la elección del nombre y un código. Cambiar un campo después exige un código nuevo."
2. Ingrese el **Código del SMS (6 dígitos)** que el apicultor lee en voz alta y haga clic en **Confirmar el registro**.

"El apicultor está registrado y verificado." El registro queda guardado con el contenido confirmado, los consentimientos surten efecto y el apicultor puede entregar de inmediato. Las reglas del código son las mismas que en una entrega: válido 5 minutos, 3 intentos, un código nuevo al cabo de un minuto.

### Sin SMS posible: foto firmada {#ro-photo}

Si el apicultor no tiene teléfono móvil, o el SMS no se puede enviar:

1. Abra **Sin SMS posible: foto firmada** y haga clic en **Imprimir el formulario**. La página impresa recoge las elecciones del apicultor y la línea "He leído el aviso informativo; mis elecciones arriba son correctas. Firma del apicultor y fecha:".
2. Deje que el apicultor la firme, tome una foto, elíjala en **Foto del formulario firmado** y escriba **Por qué no hay código SMS**.
3. Haga clic en **Guardar con la foto firmada**: "El registro se guardó con la foto firmada. Espera la revisión del Responsable de cumplimiento."

Hasta que el Responsable de cumplimiento de su dirección acepte la foto, el apicultor no puede entregar; una foto rechazada también retira los consentimientos del registro. Usted no puede revisar un registro que haya hecho usted mismo. Un apicultor sin teléfono móvil no recibe ninguna cuenta del portal.

## La página del registro {#ro-details}

Haga clic en un registro de la lista para abrirlo. Muestra el registro, el **Número de registro** (con "otra provincia" cuando difiere), el **Teléfono móvil**, los **Consentimientos** (con "retirado" cuando se ha retirado alguno), la **Prueba** y su revisión, y los **Colmenares** con sus colmenas.

- **Añadir un colmenar** registra otro apiario del apicultor.
- **Enviar datos de acceso por SMS** envía al apicultor un nombre de usuario y una contraseña temporal para el portal del apicultor (solo mientras el portal está abierto, y solo para un registro confirmado con el código SMS: "El acceso requiere un registro confirmado con el código SMS (sin móvil no hay cuenta)."). El apicultor elige su propia contraseña en el primer inicio de sesión.

<!-- SHOT: registration-details -->
Figure: Un registro confirmado con sus consentimientos y sus apiarios.

Los datos atestiguados (identidad, número de registro, teléfono) son de solo lectura, también en la página del apicultor de los administradores; solo cambian con un nuevo registro del mismo apicultor (nueva verificación), confirmado de nuevo.

**Errores frecuentes**

Table: Errores frecuentes al registrar un apicultor {#tbl-registration-errors}

| Mensaje | Qué hacer |
|---|---|
| "Introduzca el número de registro (o el número de explotación de un apicultor registrado)." | Ingrese un número antes de **Buscar o registrar**. |
| "El número debe tener TR, un código de provincia 01-81 y 10 dígitos (p. ej. TR061234567890)." | Corrija el número de registro. |
| "Este apicultor está registrado por otra dirección." | Consulte a esa dirección. |
| "Código incorrecto. Quedan … intentos." / "El código ha caducado. Envíe un código nuevo." | Pida al apicultor que vuelva a leer el código, o envíe uno nuevo. |
| "Los datos cambiaron después de enviar el código. Envíe un código nuevo." | Usted cambió un campo después de enviar el código. |
| "Demasiados códigos a este número de teléfono. Inténtelo más tarde." | A un mismo número de teléfono se envían como máximo 5 códigos por hora y 10 por día. |
