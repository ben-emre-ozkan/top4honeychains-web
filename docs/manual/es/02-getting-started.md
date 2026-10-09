# 2. Primeros pasos {#getting-started}

## Inicio de sesión {#sign-in}

1. Abra la dirección de la plataforma y haga clic en **Iniciar sesión** en la página de inicio, o vaya directamente a `/Account/Login`.
2. Ingrese su **Nombre de usuario** (su nombre de usuario, no su dirección de correo electrónico) y su **Contraseña**.
3. Marque **Recuérdame** si desea seguir con la sesión iniciada después de cerrar el navegador. Déjelo sin marcar en un equipo compartido.
4. Haga clic en **Iniciar sesión**.

<!-- SHOT: sign-in -->

Después de iniciar sesión llega a **Mis tareas**, la lista de tareas que esperan su intervención.

**Quién puede iniciar sesión.** Necesita el rol de administrador de la plataforma o al menos una membresía activa en una organización. Un administrador de la plataforma le asigna la membresía (consulte [Miembros](#admin-members)).

**Mensajes frecuentes**

- "Nombre de usuario o contraseña no válidos. Tras demasiados intentos fallidos, la cuenta se bloquea durante un tiempo." El nombre de usuario o la contraseña son incorrectos, o la cuenta está bloqueada. **Tras 5 intentos fallidos, la cuenta se bloquea durante 15 minutos**. Espere y vuelva a intentarlo, o use **¿Olvidaste tu contraseña?**.
- "Esta cuenta todavía no tiene acceso al panel: no es miembro de ninguna organización. …" Su cuenta no tiene ninguna membresía activa. Pida a un administrador de la plataforma que lo agregue a su organización.

Para cerrar la sesión, abra el menú de su avatar (arriba a la derecha) y elija **Cerrar Sesión**.

## Olvidó su contraseña {#password-reset}

1. En la página de inicio de sesión, haga clic en **¿Olvidaste tu contraseña?**.
2. Ingrese el **Correo electrónico** de su cuenta y haga clic en **Restablecer**.
3. La página responde siempre "Si existe una cuenta con esta dirección de correo electrónico, se le han enviado las instrucciones para restablecer la contraseña." (No revela qué direcciones tienen cuenta).
4. Abra el correo "Restablezca su contraseña de TOP4HoneyChains" y haga clic en el enlace. El enlace es válido durante un día.
5. Ingrese la **Nueva contraseña** dos veces y confirme. La plataforma lo lleva de vuelta a la página de inicio de sesión.

Las contraseñas deben tener al menos 6 caracteres; si no se cumple una regla, la página lo indica en su idioma, y si el enlace es demasiado antiguo muestra "El enlace no es válido o ha caducado. Solicite un nuevo restablecimiento de contraseña." Si el correo no llega, revise la carpeta de correo no deseado y, después, consulte a un administrador de la plataforma, que puede asignarle una contraseña nueva (consulte [Usuarios](#admin-users)).

<!-- SHOT: forgot-password -->

## Su perfil {#profile}

Abra el menú del avatar y elija **Perfil**.

- **Nombre de usuario** y **Correo electrónico** se muestran, pero no se pueden cambiar aquí. Consulte a un administrador de la plataforma.
- **Número de teléfono** se puede cambiar. Haga clic en **Guardar**.
- **Cambiar contraseña** abre un cuadro de diálogo: ingrese la **Contraseña actual**, la **Nueva contraseña** y **Confirme la contraseña**, y haga clic en **Actualizar contraseña**.

Para dejar un equipo compartido, use **Cerrar Sesión**; no hay pantalla de bloqueo.

<!-- SHOT: profile -->

## Elegir el idioma {#language}

La plataforma está disponible en turco, inglés, español, polaco y letón. El turco es el idioma predeterminado.

- En el panel, use el botón de la bandera en la barra superior. (En la pantalla estrecha de un teléfono, el botón de la bandera está oculto; cambie el idioma en la página de inicio de sesión).
- En las páginas de inicio de sesión, use los enlaces **TR EN ES PL LV** arriba a la derecha.
- En las páginas públicas, use los enlaces de idioma de la barra superior.

El navegador recuerda la elección durante un año. Los nombres que provienen de los datos de referencia (tipos de miel, puntos de control, roles) siguen el mismo idioma; cuando falta una traducción, se muestra el texto en inglés.

## La barra superior y su rol {#top-bar}

La barra superior muestra, de izquierda a derecha:

- el botón de menú (en teléfonos) y el logotipo de TOP4HoneyChains, que abre la página de inicio pública;
- la **etiqueta organización · rol**: la organización y el rol con los que está trabajando ahora;
- el botón de idioma;
- un botón de pantalla completa;
- su avatar, con **Perfil** y **Cerrar Sesión**.

<!-- SHOT: top-bar-role-chip -->

La etiqueta muestra una marca de verificación cuando su organización está verificada, un signo de exclamación mientras espera la verificación y un signo de alto sobre fondo rojo cuando está suspendida. Mientras una organización no está verificada, sus miembros pueden consultar sus datos y su administrador de la organización puede subir documentos, pero **nadie puede ingresar datos**. Un aviso lo indica: "Su organización aún no está verificada. …", o, para una organización suspendida, "Su organización está suspendida. Puede consultar sus datos, pero nadie puede ingresar datos hasta que un administrador de la plataforma la reactive."

### Varias membresías {#memberships}

Una persona puede tener varias membresías: en organizaciones distintas o con varios roles en la misma organización (por ejemplo, **Aprobador de calidad** y **Administrador de la organización** en una asociación). Siempre se trabaja con una sola membresía a la vez.

Para cambiar:

1. Haga clic en la etiqueta. Una lista con el título **Sus roles** muestra todas las membresías activas; la actual aparece marcada.
2. Haga clic en otra membresía. La plataforma abre **Mi organización** con la nueva membresía.

El menú, las tareas de **Mis tareas**, los registros que puede ver y las acciones que puede realizar dependen de la membresía activa. La elección se mantiene hasta que cierre el navegador; después, vuelve a empezar con su primera membresía (por orden alfabético del nombre de la organización). Si tiene dos o más membresías, **Mi organización** también ofrece una lista **Actuar como** con un botón **Cambiar**.

<!-- SHOT: role-switch -->

## Mis tareas {#my-work}

**Mis tareas** es su página de inicio. Muestra todo lo que espera su intervención en la membresía activa, agrupado por tipo y empezando por lo más antiguo. Cada grupo tiene un mosaico en la parte superior; haga clic en él para saltar al grupo.

<!-- SHOT: my-work -->

| Grupo | Qué significa | Quién lo recibe |
|---|---|---|
| **Valores por ingresar** | Un punto de control (PCC) de un lote o de una partida necesita valores | Los roles que ingresan ese PCC |
| **Valores por aprobar** | Se envió un registro de PCC o un registro de almacenamiento del PCC 10, o se completó o marcó como "No aplicable" sin una aprobación, y necesita una segunda persona. Nunca ve los registros que usted mismo ingresó o marcó. | Los roles que aprueban ese PCC |
| **Solicitudes de finalización** | Alguien solicitó finalizar una partida | Aprobador de calidad del propietario de la partida |
| **Muestras por recibir** | Se envió una muestra a su laboratorio | Analista, Responsable de laboratorio |
| **Resultados de laboratorio por ingresar** | Una muestra recibida aún no tiene informe, o el informe fue devuelto | Analista |
| **Informes de laboratorio por aprobar** | Se envió un informe | Responsable de laboratorio |
| **Envíos por confirmar** | Un envío dirigido a su organización está pendiente | Personal, Administrador de la organización del receptor |
| **Organizaciones por verificar** | Una organización subió documentos y espera la verificación | Administrador de la plataforma |
| **Números de registro por verificar** | El número de registro de la explotación de un apicultor espera la verificación | Administrador de la plataforma |
| **Correcciones por decidir** | Otro administrador solicitó una corrección | Administrador de la plataforma |

Cada elemento muestra un título (por ejemplo "PCC 0 · #20070" y la palabra **lote**, **partida** o **almacenamiento**), cuánto tiempo lleva esperando (los elementos con más de tres días reciben una marca ámbar) y una etiqueta **Nuevo** si aún no lo ha visto. Un elemento de laboratorio también puede indicar por qué debe esperar, por ejemplo: "El PCC 0 (auditoría primaria) de este lote aún no está completo; los resultados de laboratorio se registran y aprueban solo después."

El enlace **Mis tareas** del menú lleva un indicador con el número de elementos pendientes. Mientras la página está abierta, busca tareas nuevas cada minuto; cuando llega algo nuevo, el enlace se ilumina una vez y un pequeño aviso enumera los grupos que crecieron. Cuando no hay nada pendiente, la página dice "No hay nada pendiente para usted." y ofrece enlaces a las páginas que puede abrir.

## El menú {#menu}

Los miembros de una organización ven un menú breve. Cada entrada aparece solo si su organización tiene la capacidad correspondiente y su rol tiene el derecho necesario:

| Entrada del menú | Abre | Se muestra cuando |
|---|---|---|
| **Mis tareas** | Sus tareas | Siempre |
| **Mi organización** | Los lotes y las partidas de su organización | Siempre |
| **Entregas (recepción)** | Las entregas registradas | Organización de procesamiento; roles con derecho de lectura de la recepción |
| **Muestras y laboratorio** | Muestras enviadas y recibidas, informes de laboratorio | Organización de procesamiento o laboratorio; roles con derecho de lectura de muestras |
| **Envíos y existencias** | Partidas en su poder, envíos entrantes y salientes, registros de almacenamiento | Organización de procesamiento, envasado, comercio mayorista o minorista; roles con derecho de lectura de envíos |
| **Páginas de apicultores** | Consentimientos y páginas públicas de apicultores | Asociación; roles con derecho de lectura de las páginas de apicultores |

Los administradores de la plataforma ven el menú de administración completo (consulte el [capítulo 12](#admin)).

## Mensajes, carga y páginas de error {#messages}

- **Mensajes**: aparecen en la esquina superior. Los mensajes de éxito y de información se cierran tras unos segundos; los mensajes de error permanecen hasta que usted los cierra.
- El **indicador de carga** (una abeja animada) aparece cuando una página o una operación de guardado tarda más de un instante.
- **Páginas de error**: "No puede abrir esta página" (403) significa que su rol no permite abrir la página; consulte al administrador de su organización. "Esta página no existe" (404) significa que la dirección no existe o que el registro no es visible para usted. Una página 500 muestra una referencia breve; indíquela cuando informe del problema. Cada página de error tiene un botón para volver a **Mis tareas** y un botón **Volver**.
