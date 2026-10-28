# 1. Sobre esta prueba {#about}

## Qué es TOP4HoneyChains {#what}

TOP4HoneyChains es una plataforma web que sigue la miel desde el apiario del apicultor hasta el estante del comercio y guarda un registro verificable de cada paso. Se desarrolla en el proyecto TOP4HoneyChains ("Trustable and Sustainable Open Platform for Smart Honey Value Chains"), financiado en la convocatoria ICT-AGRI-FOOD 2022.

Por qué es importante:

- **Confianza que se puede comprobar.** Cada registro lo ingresa una persona y lo aprueba otra (principio de los cuatro ojos). Cada cambio queda en un registro de auditoría que no se puede modificar.
- **Toda la cadena en un solo lugar.** El registro del apicultor, la entrega en envases precintados, una muestra de cada envase, el análisis de laboratorio, los puntos de control (PCC) en la planta, la partida sellada y el envío al comercio.
- **El fraude es más difícil.** Los precintos, las tapas de seguridad de los frascos de muestra y los códigos SMS vinculan cada registro con las personas que lo confirmaron. Un precinto roto o un frasco abierto se notan.
- **Una historia para el consumidor.** El código QR del frasco abre la Historia de la miel: de dónde viene la miel, los resultados de laboratorio aprobados con sus límites y los puntos de control.
- **Justa con el apicultor.** El apicultor ve sus entregas, sus resultados y sus partidas, indica un precio solicitado y decide qué se muestra sobre él.

Esta guía permite a su equipo probar todo esto en un entorno de demostración: una miel, desde el registro de su apicultor hasta el consumidor que escanea el código QR. Lleva unas dos o tres horas con varias personas; una sola persona también puede hacer todos los roles, uno después de otro.

## Cómo funciona la demostración {#demo}

- **Sitio de demostración:** {{demo.url}} (su coordinador confirma la dirección). Una franja oscura al pie de cada página dice **Entorno de demostración: no se envían SMS reales**. Todo lo que ve son datos de prueba marcados con **[DEMO]**.
- **Cuentas:** una cuenta por rol y organización de su país (capítulo 3). Las contraseñas están en la hoja aparte credentials.txt; no la comparta.
- **Idioma:** elija el idioma con la bandera, arriba a la derecha (en la página de inicio de sesión: TR EN ES PL LV).
- **SMS:** la demostración no envía SMS. Los códigos y los datos de acceso aparecen en **Bandeja de SMS de demostración** (enlace en la franja inferior; se abre en una pestaña nueva). Usted ve los mensajes de su organización activa y los que usted envió.
- **Primer inicio de sesión:** la plataforma le pide leer y confirmar el aviso de privacidad. Los roles que firman por su organización (administrador de la organización, responsable de registro de apicultores, Responsable de aseguramiento de la calidad, responsable de laboratorio, Responsable de cumplimiento) confirman con un código SMS: envíelo, léalo en la bandeja de SMS de demostración e ingréselo. Después aparece un breve recorrido; haga clic en **Saltar el recorrido** o sígalo.
- **Cuentas compartidas:** otros evaluadores de su país usan las mismas cuentas al mismo tiempo. Dé a sus registros un nombre reconocible (sus iniciales) y no modifique los registros de otros evaluadores.
- **Contraseña olvidada:** la demostración no envía SMS, así que no se puede restablecer la contraseña. Consulte a su coordinador.
- **Restablecimiento:** el equipo de la plataforma puede devolver la demostración a su estado inicial. Entonces se borra todo lo ingresado después del último restablecimiento.
- **Miel de ejemplo:** su país ya tiene un ejemplo terminado: {{example.batch}} de {{b1.name}}. Abra su Historia de la miel en {{example.story}}.

## Límites de la demostración para los países socios {#limits}

La plataforma se desarrolló primero para Türkiye. Para Argentina, Polonia y Letonia, la demostración usa las soluciones provisionales más pequeñas y seguras; léalas antes de evaluar un paso:

- **Número de registro.** La plataforma comprueba el número de registro de explotaciones de Türkiye (TR, código de provincia y 10 dígitos). En la demostración, un país socio usa un número de prueba **AR-DEMO-**, **PL-DEMO-** o **LV-DEMO-** y 6 dígitos. No se comprueba en ningún registro nacional.
- **Provincias y distritos.** Las listas contienen las 81 provincias de Türkiye. La demostración agrega dos regiones por país socio, marcadas con (AR), (PL) o (LV), con algunos distritos. Son ejemplos, no listas oficiales.
- **Mapa de origen.** La Historia de la miel, la página del apicultor y el panel dibujan los orígenes solo en un mapa de Türkiye. Para otros países, la página muestra el país y la región como texto ("no aparecen en el mapa"). Un mapa por país socio es una ampliación prevista (A8).
- **Números de explotación.** Los apicultores registrados en la demostración reciben números como AR-BEE-DEMO-000001.
- **Textos y límites.** El aviso de privacidad y los textos de consentimiento son borradores redactados para Türkiye (marcados con **TEXTO DE DEMOSTRACIÓN – no aprobado**). Los parámetros de laboratorio y sus límites siguen las normas turcas sobre la miel; su país puede usar otros límites. Indíquelo en sus comentarios.
- **Moneda.** El precio solicitado se puede ingresar en ARS, PLN, EUR, USD o TRY.

# 2. Los roles {#roles}

En TOP4HoneyChains una persona nunca actúa sola: cada acción se realiza para una organización, con un rol. A continuación hay una ficha por rol: quién es, qué hace, qué pantallas usa y qué cuenta de demostración lo representa. El manual de usuario explica cada rol en detalle (el capítulo figura en cada ficha).

{{ROLECARDS}}

# 3. Sus cuentas de demostración {#accounts}

{{COORDINATORNOTE}}

{{ACCOUNTS}}

# 4. Seguir una miel {#scenario}

El escenario sigue una miel a través de la plataforma. Cada paso indica el rol, la cuenta de demostración, qué hacer y qué debería ver. Trabaje en este orden: muchos pasos esperan al anterior. Los valores sugeridos para los puntos de control y el informe de laboratorio están en la [Tabla](#tbl-values).

Dos reglas que encontrará en todas partes: **un registro lo ingresa una persona y lo aprueba otra**, y **un lote o una partida solo avanza cuando todo lo anterior está aprobado**. Si un botón está desactivado, la página indica qué falta.

{{SCENARIO}}

# 5. Comentarios {#feedback}

Dé sus comentarios por paso y responda las preguntas generales. Puede escribir en esta página, usar la lista de control interactiva (el archivo HTML de la lista de control de este paquete, que puede exportar sus respuestas) o completar la plantilla de comentarios (CSV). Envíe el resultado a su coordinador.

{{FEEDBACK}}

# Anexo: valores sugeridos {#values}

Estos valores están dentro de los límites de la plataforma. Pruebe un valor fuera de su límite para ver cómo la plataforma avisa y pide una confirmación.

{{VALUES}}
