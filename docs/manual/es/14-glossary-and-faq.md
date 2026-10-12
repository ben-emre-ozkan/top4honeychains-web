# 14. Glosario y preguntas frecuentes {#glossary-faq}

## Glosario {#glossary}

La [Table](#tbl-glossary) define los términos que se usan en este manual; la [Table](#tbl-renamed-labels) enumera las etiquetas que cambian en la próxima versión.

Table: Glosario {#tbl-glossary}

| Término | Significado |
|---|---|
| **HACCP** | Análisis de Peligros y Puntos Críticos de Control (en España también APPCC): el método de seguridad alimentaria del que provienen los puntos de control. |
| **PCC (punto crítico de control)** | Un paso en el que un peligro se controla midiendo o comprobando algo. CCP en inglés, KKN en turco, KPK en polaco, KKP en letón. |
| **Datos** | Lo que las personas registran en la plataforma: valores medidos (por ejemplo, la humedad en %) y respuestas (por ejemplo, Sí / No). |
| **Parámetro** | Un dato que se registra en un PCC, por ejemplo la humedad o la temperatura. |
| **Límite crítico** | El mínimo o el máximo (o ambos) permitidos para un parámetro. |
| **Límite verificado** | Un límite confirmado por el proyecto; solo los límites verificados dan un veredicto público dentro/fuera del límite. |
| **Dentro del límite / Fuera del límite** | La comparación de un valor con su límite. Los valores fuera del límite siguen marcados después de la aprobación. |
| **No aplicable** | Un PCC que no corresponde a esta miel, registrado con un motivo; aun así necesita aprobación. |
| **Lote de miel cruda** | La miel de una entrega de un apiario (un período, un tipo de miel). Lleva los PCC 0–3. |
| **Partida** | Miel elaborada a partir de uno o varios lotes, procesados y envasados juntos. Lleva los PCC 4–10 y el código QR. |
| **Finalizar / sellar** | El paso, realizado por dos personas, que bloquea una partida y sus lotes y publica la Historia de la miel. |
| **Principio de los cuatro ojos** | En cada registro intervienen dos personas distintas: una ingresa los datos y una segunda persona los revisa y los aprueba. Nadie aprueba lo que ha ingresado él mismo. Consulte [Reglas básicas](#fixed-rules). |
| **Exención de la doble verificación (cuatro ojos)** | Una configuración para organizaciones pequeñas que permite autoaprobar registros de PCC, siempre con una marca. Nunca para los resultados de laboratorio. |
| **Muestra / contramuestra** | La muestra precintada que se envía al laboratorio y su gemela, que se conserva en la planta. |
| **Número de precinto** | El número del precinto de una muestra; el laboratorio lo comprueba al recibirla. |
| **Informe de laboratorio / código del informe** | El informe del laboratorio (PDF y valores) y su código público de 12 caracteres. |
| **Laboratorio de análisis de alimentos** | El laboratorio que analiza las muestras de miel, normalmente acreditado. Su analista ingresa los resultados; su responsable de laboratorio los aprueba. |
| **Laboratorio acreditado** | Un laboratorio con una acreditación vigente, que se registra cuando se aprueba el informe. |
| **Laboratorio propio** | Un laboratorio que pertenece al propietario del lote. |
| **Poseedor** | Una organización que tiene en su poder una cantidad de una partida tras un envío confirmado. |
| **Custodia** | Quién tiene la mercancía; se transfiere cuando el receptor confirma un envío. |
| **Informe de retiro** | La lista de todo lo que hay aguas arriba y aguas abajo de una partida o de un lote. |
| **Historia de la miel** | La página pública de una partida finalizada, que se abre desde su código QR. |
| **Página del apicultor** | La página pública de un apicultor, publicada con su consentimiento. |
| **Número de registro (AKS)** | El número de registro de la explotación del apicultor: `TR` + código de provincia + 10 dígitos. |
| **Membresía** | Una cuenta de usuario × una organización × un rol. Una persona puede tener varias membresías, también varias en la misma organización. |
| **Apicultor (registro)** | Los apicultores no tienen cuenta. Cada uno está representado en la plataforma por un registro de apicultor. |
| **Leer** | El derecho a abrir y leer registros, sin modificarlos. |
| **Capacidad** | Una aptitud de una organización: lo que es capaz de hacer en la cadena de la miel (por ejemplo, "Capaz de recibir y procesar miel"). Consulte la [Table](#tbl-capabilities). |
| **Reglas básicas** | Las reglas que forman parte de la plataforma y que ningún ajuste puede desactivar. Consulte [Reglas básicas](#fixed-rules). |
| **Registro de auditoría** | El registro de solo adición de todos los cambios, protegido por una cadena de hash. |
| **Huella SHA-256** | Un código calculado a partir del contenido de un archivo; cualquier cambio en el archivo lo modifica. |

Table: Etiquetas que cambian de nombre en la próxima versión (las capturas de pantalla muestran las etiquetas antiguas) {#tbl-renamed-labels}

| Hoy en pantalla | Nombre nuevo (usado en este manual) |
|---|---|
| Aprobador de calidad | Responsable de aseguramiento de la calidad |
| Reglas fijas (no se pueden desactivar) | Reglas básicas (no se pueden desactivar) |
| Descripciones de las capacidades (por ejemplo, "Recepción y procesamiento: …") | Textos de aptitud (por ejemplo, "Capaz de recibir y procesar miel: …"); consulte la [Table](#tbl-capabilities) |
| Apicultura (capacidad) | Explotación apícola |

## Preguntas frecuentes y solución de problemas {#faq}

**No puedo iniciar sesión: "Nombre de usuario o contraseña no válidos. …"**
Revise el nombre de usuario (no el correo electrónico) y la contraseña. Tras 5 intentos fallidos, la cuenta se bloquea durante 15 minutos; espere o pida a un administrador de la plataforma que la desbloquee con **Desbloquear** y, si es necesario, use **¿Olvidaste tu contraseña?**.

**"Esta cuenta todavía no tiene acceso al panel: no es miembro de ninguna organización."**
Su cuenta no tiene ninguna membresía activa. Pida a un administrador de la plataforma que lo agregue a su organización con el rol adecuado.

**Veo "No puede abrir esta página" (403).**
Su rol o su organización no pueden abrir esta página o este registro. Con un PCC de otra organización es lo esperado, salvo que su organización tenga esa partida en su poder. Si necesita acceso, consulte al administrador de su organización.

**Veo "Esta página no existe" (404).**
La dirección es incorrecta, el registro no existe o (en el caso de la Historia de la miel) la partida aún no está finalizada.

**No puedo ingresar nada; la página dice "Solo lectura".**
Lea el motivo que aparece debajo. Motivos habituales: su organización aún no está verificada; otra organización u otro rol es responsable de este PCC; el registro está bloqueado por la finalización.

**Mi organización no está verificada.**
El administrador de la organización sube un documento (licencia, acreditación, comprobante de registro) en la página de la organización; después, un administrador de la plataforma la verifica.

**El laboratorio no puede ingresar resultados: "El PCC 0 (auditoría primaria) de este lote aún no está completo; …"**
Primero, la planta debe ingresar el PCC 0 y su responsable de aseguramiento de la calidad debe aprobarlo.

**No puedo aprobar: "Cuatro ojos: usted registró valores de este PCC, por lo que otra persona debe aprobarlo."**
Debe hacerlo otro aprobador. Si su organización tiene una sola persona de calidad, consulte a un administrador de la plataforma sobre la exención de la doble verificación (no disponible para los resultados de laboratorio).

**"Solicitar la finalización" está desactivado.**
Lea la lista "Falta antes de finalizar:". Todos los PCC de la partida, incluido el PCC 10 en la planta, y todos los PCC de los lotes deben estar aprobados.

**Cambié un valor y la aprobación desapareció.**
Es lo previsto: cualquier cambio requiere una nueva aprobación.

**El lote no aparece en "Gestionar partida".**
Un lote solo aparece cuando todos sus PCC están completados o son no aplicables y están aprobados, y solo para el mayorista de la partida. Una planta de procesamiento debe estar vinculada a su registro de mayorista (organización → **Editar**); la página nombra las plantas que no lo están.

**Un valor está aprobado, pero es incorrecto.**
Pida una corrección a un administrador de la plataforma; un segundo administrador la aprueba.

**La página del apicultor no es pública.**
Compruebe que el consentimiento esté registrado, que el número de registro esté verificado, que la asociación esté verificada y que el apicultor esté activo.

**Trabajo para dos organizaciones.**
Cambie de una a otra con la etiqueta organización · rol de la barra superior. La elección se mantiene hasta que cierre el navegador.

**No recibo el correo para restablecer la contraseña.**
Revise la carpeta de correo no deseado. Un administrador de la plataforma puede asignarle una contraseña nueva.
