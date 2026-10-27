# 15. Administrador de la plataforma {#admin}

## Su trabajo diario {#admin-daily}

Los administradores de la plataforma la gestionan. Verifican organizaciones, gestionan membresías y permisos, envían los datos de acceso, publican los textos de los avisos, mantienen los datos de referencia y las definiciones de los puntos de control, registran a los apicultores, crean partidas (por ahora), deciden sobre las correcciones y supervisan el registro de auditoría.

Los administradores pueden leer todos los registros, pero **no ingresan ni aprueban datos de negocio**. Al iniciar sesión llegan a **Mis tareas**, que muestra **Organizaciones por verificar**, **Números de registro por verificar** y **Correcciones por decidir**. El **Panel** ofrece una visión general (cifras, la cadena, un mapa de apicultores y apiarios por provincia, la actividad reciente y las aprobaciones pendientes).

<!-- SHOT: admin-dashboard -->
Figure: El panel del administrador de la plataforma.

El menú de administración ([Table](#tbl-admin-menu)):

Table: El menú de administración {#tbl-admin-menu}

| Menú | Entradas |
|---|---|
| **Panel** | Visión general |
| **Apicultores** | Lista de apicultores, apiarios, niveles de educación, propósitos de la apicultura, tipos de apicultura |
| **Producción de Miel Cruda** | Lotes de miel cruda, tipos de miel, períodos de producción |
| **Mayoristas** | Lista de mayoristas, agregar un mayorista |
| **Producciones Finales de Miel** | Partidas |
| **Puntos Críticos de Control** | PCC, categorías de parámetros, parámetros, traducciones de datos de referencia |
| **Gestión de Usuarios** | Usuarios, roles, **Registro de auditoría**, **Conservación de datos**, **Avisos y textos de consentimiento**, **Bandeja de salida de SMS (proveedor de prueba)** (solo con el proveedor de SMS de prueba) |
| **Organizaciones** | Mis tareas, organizaciones, capacidades, registro, registro de apicultores, roles y permisos, responsabilidades de PCC, correcciones, envíos, muestras, páginas de apicultores |

## Organizaciones {#admin-organizations}

### Crear una organización {#admin-org-create}

1. **Organizaciones** → **Nueva organización**.
2. Ingrese el nombre, la razón social, el número de identificación fiscal, la provincia y el distrito.
3. Si la organización es un mayorista que ya existe en la plataforma, elíjalo en el campo del mayorista existente. **Una planta de procesamiento debe estar vinculada a su registro de mayorista**: los lotes que recibe se archivan bajo ese mayorista, y una partida solo puede usar lotes de su propio mayorista. El vínculo también se puede definir más tarde en la página **Editar** de la organización. Los lotes que una planta recibió antes de quedar vinculada pasan a las partidas del mayorista al que está vinculada.
4. Marque al menos una capacidad y haga clic en **Crear**.

La organización empieza en estado **Pendiente**: "Organización creada. Permanece en estado Pendiente hasta que sea verificada."

<!-- SHOT: admin-org-create -->
Figure: Creación de una organización: vínculo con el mayorista y capacidades.

### Verificar, solicitar documentos, suspender {#admin-org-verify}

En la página de la organización (un administrador de la plataforma; para las asociaciones de apicultores, en su lugar, el administrador de la organización de una asociación central verificada, si la plataforma está configurada para ello: entonces los administradores reciben "Acceso denegado" en las asociaciones):

- **Verificar** (desde Pendiente) requiere al menos un documento subido y, para una organización que se verifica por primera vez, un Responsable de cumplimiento activo.
- **Solicitar documentos** (mientras está Pendiente) requiere un motivo, por ejemplo qué documento falta.
- **Suspender** (mientras está Verificada) requiere un motivo. **Restablecer** vuelve a verificar una organización suspendida.

Cada cambio aparece en el historial de verificación y en el registro de auditoría.

### Capacidades {#admin-org-capabilities}

**Agregar capacidad** agrega una capacidad. Para **Laboratorio**, marque acreditado e ingrese el número de acreditación y su fecha de validez; un laboratorio acreditado necesita un número. **Quitar** elimina una capacidad; una organización conserva siempre al menos una. La página **Capacidades** muestra, por capacidad, qué PCC la requieren y qué organizaciones la tienen. La pestaña **Capacidades de la organización** de **Roles y permisos** permite editar todas las organizaciones a la vez.

### Miembros {#admin-members}

1. En la página de la organización, en la sección de miembros, elija el rol. El rol **Responsable de registro de apicultores** solo está disponible en una organización con la capacidad Registro de apicultores.
2. Ingrese un nombre de usuario existente, o un nombre de usuario, un correo electrónico y un **Teléfono móvil** nuevos para crear el usuario al mismo tiempo. Elija cómo recibe el usuario sus datos de acceso: **Enviar el usuario y una contraseña temporal por SMS (recomendado)** o **Fijar aquí una contraseña inicial (el usuario la cambia al primer acceso)**.
3. Haga clic en **Agregar miembro**: "… agregado como …".

Una membresía es una cuenta de usuario × una organización × un rol; un usuario puede tener varias. **Revocar** termina una membresía y cierra la sesión del usuario en todos los dispositivos. La pestaña **Miembros** de **Roles y permisos** muestra todas las membresías de todas las organizaciones.

<!-- SHOT: admin-org-members -->
Figure: Membresías de una organización y el formulario **Agregar miembro**.

### Exención de la doble verificación {#admin-four-eyes}

Las organizaciones pequeñas con una sola persona de calidad pueden recibir la **Exención de la doble verificación (cuatro ojos)**: sus miembros pueden entonces aprobar sus propios registros de PCC y confirmar sus propias solicitudes de finalización. Cada una de esas aprobaciones queda marcada en el registro de auditoría, y el Responsable de cumplimiento de la organización la ve en **Mecanismo de aprobación**. La exención nunca se aplica a los resultados de laboratorio ni a la revisión de las fotos firmadas.

### Responsable del tratamiento {#admin-data-controller}

Cada organización es la responsable del tratamiento de los datos personales que registra. En la página de la organización, **Responsable del tratamiento** contiene su **Razón social**, **Contacto (correo o teléfono)** y **Dirección**; figuran en el aviso de privacidad de la organización. Los mantienen al día el Responsable de cumplimiento de la organización o un administrador de la plataforma.

## Usuarios {#admin-users}

**Gestión de Usuarios** → **Usuarios** muestra las cuentas.

- **Agregar nuevo usuario** crea una cuenta. Con el modelo de organizaciones, deje vacío el **Rol global** para los miembros de organizaciones y asígneles una membresía en la página de la organización; elija **Administrador de la plataforma** solo para los administradores. Ingrese el **Teléfono móvil** y elija **Enviar el usuario y una contraseña temporal por SMS (recomendado)** o **Fijar aquí una contraseña inicial (el usuario la cambia al primer acceso)**.
- **Enviar acceso por SMS** envía nuevos datos de acceso (una nueva contraseña temporal, válida 72 horas) a un usuario que los ha perdido. Nadie, tampoco el administrador, ve la contraseña temporal.
- **Restablecer contraseña** asigna una contraseña nueva a un usuario; también cuenta como contraseña inicial, que el usuario cambia en el siguiente inicio de sesión.
- **Desbloquear** aparece junto a una cuenta con el estado **Bloqueado** y termina el bloqueo de inmediato ("Cuenta desbloqueada."). Sin él, el bloqueo termina por sí solo a los 15 minutos.
- **Eliminar** borra una cuenta (úselo con cuidado; no se puede deshacer). No puede eliminar su propia cuenta. Cada eliminación y cada desbloqueo queda registrado en el registro de auditoría.

Las contraseñas nuevas deben tener al menos 10 caracteres y no pueden ser el nombre de usuario ni una contraseña común; las contraseñas existentes siguen siendo válidas. Tras 5 intentos fallidos de inicio de sesión, la cuenta se bloquea durante 15 minutos.

**Gestión de Usuarios** → **Roles** muestra los roles globales. Los roles de los que depende la plataforma (**Administrador** y **Apicultor**) están marcados como **Protegido** y no se pueden renombrar ni eliminar.

## Roles y permisos {#admin-permissions}

**Roles y permisos** tiene tres pestañas:

- **Miembros**: todas las membresías activas; agregar y revocar.
- **Permisos por rol**: elija un rol y marque, para cada función, **Introducir**, **Aprobar** y **Leer**. Las funciones son los PCC (una fila cada uno, con la abreviatura de PCC del idioma de la página) y las áreas de recepción, muestras, resultados de laboratorio, envíos, páginas de apicultores, retiro, finalización, registro de apicultores, pruebas, precio y las entradas de auditoría de la organización. Cada derecho debe conservar al menos un rol. Los cambios se aplican de inmediato.
- **Capacidades de la organización**: las capacidades de cada organización.

El recuadro "Reglas básicas (no se pueden desactivar):" enumera las reglas que ninguna configuración modifica (consulte [Reglas básicas](#fixed-rules)). La [Table](#tbl-admin-changeable) indica exactamente lo que puede cambiar un administrador.

<!-- SHOT: admin-role-permissions -->
Figure: Permisos del rol Responsable de aseguramiento de la calidad.

## Responsabilidades de PCC {#admin-ccp-responsibilities}

**Responsabilidades de PCC** define, para cada PCC, qué organización es responsable (el propietario del lote o de la partida o, en el caso de los lotes, la asociación del apicultor), qué capacidad necesita, qué roles ingresan los valores y cuáles los aprueban, y si se requiere aprobación. Ejemplo: asigne el PCC 0 a la asociación eligiendo "La asociación del apicultor del lote", **Asociación** e **Inspector de campo**.

## Puntos de control, parámetros y límites {#admin-ccps}

- **Puntos Críticos de Control**: código, título, etapa del proceso, justificación de la criticidad y **Se aplica a** (lote de miel cruda o partida final). La opción de mostrar valores de texto libre en la Historia de la miel pública debe permanecer desactivada cuando las respuestas puedan contener datos personales.
- **Parámetros**: nombre, unidad, categoría, tipo de dato (**Número**, **Sí / No**, **Texto**, **Archivo**), mínimo y máximo, obligatorio u opcional, y "obligatorio solo cuando" otra pregunta de sí/no tiene una respuesta determinada.
- **Límites**: por parámetro, límites predeterminados para todos los tipos de miel y límites específicos por clase de miel (de flores, de mielada, de pino) o por tipo de miel, opcionalmente con una condición, una suma o una proporción. Los resultados almacenados no se recalculan cuando cambian los límites.

**Límites verificados.** Cada parámetro tiene la casilla **Límites verificados (se muestran públicamente)**. Solo los límites verificados producen un veredicto público dentro/fuera del límite en la Historia de la miel y en la página del apicultor. En el panel, los límites no verificados aparecen marcados como "por verificar", y las páginas públicas indican que los límites todavía se están revisando.

<!-- SHOT: admin-parameter-limits -->
Figure: Límites de un parámetro (PCC 1, humedad).

## Datos de referencia y traducciones {#admin-reference}

Los tipos de miel (con su clase de miel para los límites), los períodos de producción, los niveles de educación, los tipos y propósitos de la apicultura, y los mayoristas se mantienen en sus menús. Un registro que está en uso no se puede eliminar.

**Traducciones de datos de referencia** contiene los nombres que se muestran en listas, tablas y páginas públicas en los cinco idiomas: tipos de miel, períodos, estados, capacidades, roles, PCC, parámetros, categorías de parámetros, notas de límites, explicaciones de parámetros y países. Un campo vacío recurre al inglés y, después, al texto base.

## Apicultores, apiarios y el registro {#admin-registry}

- **Registro de apicultores**: a los apicultores los registran los responsables de registro de apicultores de las direcciones distritales, con un código SMS o una foto firmada (consulte el [capítulo 12](#role-ro)). Un administrador de la plataforma puede registrar apicultores de la misma manera; esos registros se guardan para la organización propia de la plataforma, y sus fotos firmadas las revisa otro administrador de la plataforma.
- **Apicultores**: la lista y la página del apicultor (nombre, fecha de nacimiento, experiencia, teléfono, educación, tipo y propósito de la apicultura, foto opcional). El número de explotación se genera automáticamente. Los datos atestiguados en el registro (identidad, número de registro, teléfono) son aquí de solo lectura; solo cambian con un nuevo registro. Los apicultores nunca se eliminan; solo se desactivan. Los apicultores antiguos, registrados antes del procedimiento de registro, se verifican de nuevo con el mismo formulario; sus antiguos accesos siguen bloqueados.
- **Apiarios**: título, número de colmenas y ubicación (latitud y longitud). La ubicación nunca se muestra públicamente; las páginas públicas muestran solo la provincia.
- **Registro de apicultores y asociaciones**: para cada apicultor, vincule la asociación e ingrese el número de registro de la explotación (formato `TR` + código de provincia + 10 dígitos, por ejemplo `TR061234567890`) con un documento probatorio. Después, haga clic en **Verificar** o **Rechazar** (para rechazar se requiere un motivo).

<!-- SHOT: admin-registry -->
Figure: Registro de apicultores y asociación.

## Partidas (por ahora, solo administradores) {#admin-batches}

1. **Mayoristas** → el mayorista → **Producciones finales de miel (partidas)** → **Nueva partida**.
2. Ingrese el **Título de la partida** y la **Fecha de producción**, y haga clic en **Crear partida**. Se genera el código QR.
3. En **Gestionar partida**, ingrese la cantidad que se usará de cada lote elegible (lotes de este mayorista con todos sus PCC aprobados) y haga clic en **Guardar composición de la partida**: "Composición de la partida guardada: … kg en total." Si una cantidad es demasiado grande o un lote no se puede usar, la página enumera cada problema (por ejemplo, "Lote #…: hay como máximo … kg disponibles.") y no guarda nada. Si una planta tiene lotes que ninguna partida puede tomar porque no está vinculada a un registro de mayorista, la página la nombra con un enlace a su página **Editar**.
4. Use **Editar** en la partida para definir la organización propietaria (la planta).
5. **Gestionar PCC**: marque del PCC 4 al PCC 10 y haga clic en **Guardar asignaciones**: "Asignaciones de PCC guardadas: … añadidas, … quitadas."

Después, la planta ingresa los PCC de la partida y la finaliza. Los administradores no pueden finalizar: "Con el modelo de organizaciones, la organización propietaria finaliza la partida (solicitud y aprobación por dos personas)."

<!-- SHOT: admin-manage-batch -->
Figure: Composición de una partida a partir de los lotes que cumplen los requisitos.

## Correcciones {#admin-corrections}

Los datos aprobados o bloqueados solo se modifican mediante una corrección:

1. Abra la página de PCC del registro. El recuadro **Correcciones** le permite elegir el parámetro, ingresar el valor nuevo y un **motivo**, y hacer clic en **Solicitar**: "Corrección solicitada; otro administrador de la plataforma debe aprobarla."
2. Un **segundo** administrador abre **Correcciones**, lee el valor anterior, el nuevo y el motivo, y hace clic en **Aprobar y aplicar** o en **Rechazar** (para rechazar se requiere una nota).

Quien solicitó una corrección no puede decidir sobre ella. Las correcciones también se aplican a registros bloqueados, y ambos pasos quedan en el registro de auditoría.

**Datos de la entrega.** Los datos de una entrega confirmada (fecha, tipo y peso de los envases, números de precinto) se corrigen en la página de la entrega, en **Correcciones de esta entrega** ("Solo para un error en el registro. Otro administrador de la plataforma debe confirmar la corrección; el original sigue legible aquí y se informa al apicultor por SMS."):

1. Haga clic en **Pedir una corrección**, cambie los campos, escriba el **Motivo** y haga clic en **Enviar la solicitud**: "Corrección solicitada; otro administrador de la plataforma debe confirmarla."
2. Un segundo administrador compara **Original** y **Corregido** y hace clic en **Confirmar y aplicar**, o escribe una **Nota (necesaria para rechazar)** y hace clic en **Rechazar**. "Corrección aplicada; se informó al apicultor por SMS."

Una corrección nunca añade ni quita envases. La entrega conserva ambos contenidos con sus huellas; el registro de auditoría recoge cada fila modificada.

<!-- SHOT: admin-delivery-correction -->
Figure: Una corrección de una entrega a la espera del segundo administrador.

## Páginas de apicultores, envíos y muestras {#admin-overviews}

Los administradores pueden abrir **Páginas de apicultores**, **Envíos y existencias** y **Muestras y laboratorio** en modo de solo lectura, para seguir el trabajo de todas las organizaciones.

## Retiro {#admin-recall}

El **Informe de retiro** de una partida o de un lote muestra:

- **aguas arriba**: los lotes, los apiarios, los apicultores y los formularios de entrega y, para las entregas con envases, cada envase con su historial de precintos, sus frascos de muestra, los valores de laboratorio por envase y si el contenido confirmado no ha cambiado;
- las partidas;
- **aguas abajo**: los envíos y los poseedores actuales.

Los miembros leen la parte que su organización puede leer (propietario, poseedor o asociación); las filas ocultas se cuentan. Un miembro solo ve su propia existencia, en **Su existencia actual**; los administradores ven a todos los poseedores en **Poseedores actuales**. Los precios nunca aparecen en el informe de retiro. **Imprimir** y **CSV** exportan el informe.

<!-- SHOT: recall-report -->
Figure: El informe de retiro de una partida.

## Avisos y textos de consentimiento {#admin-notices}

**Avisos y textos de consentimiento** enumera los avisos de privacidad (para usuarios y para apicultores, de la plataforma y de cada organización), los textos de consentimiento y la declaración del registro, por idioma, con su **Versión**, la fecha de publicación (**Publicado**) y cuántas personas los han aceptado (**Aceptado**). "Los textos tienen versiones y no se cambian tras publicarse: publique una versión nueva."

1. Haga clic en **Publicar una versión nueva** (o en **Nueva versión** junto a un texto).
2. Elija el **Tipo**, para quién es (**Para**), la **Organización** (vacía para la plataforma), la **Finalidad del consentimiento** para un texto de consentimiento, la **Versión** y el **Idioma**, y escriba el **Texto**.
3. Marque **Borrador (aún no aprobado por la oficina de protección de datos)** mientras el texto no esté aprobado; entonces se muestra con la marca **BORRADOR**.
4. Haga clic en **Publicar**. "Publicar retira el texto vigente del mismo tipo, público, organización, finalidad e idioma. A los usuarios que aceptaron una versión anterior se les pide de nuevo."

La página pública **Privacidad y consentimiento** (`/privacy`) muestra los avisos vigentes para los apicultores y para los usuarios de la plataforma.

## SMS {#admin-sms}

La plataforma envía SMS para los códigos de registro, los datos de acceso, las confirmaciones de entrega, las confirmaciones de avisos, los restablecimientos de contraseña, las comprobaciones de teléfono y los mensajes informativos. Qué proveedor los envía es un ajuste del servidor (consulte [Ajustes en la configuración del servidor](#admin-settings)). La plataforma guarda un registro de cada mensaje con el número enmascarado; los códigos y las contraseñas nunca se guardan en claro. Un límite diario protege el presupuesto de SMS; los restablecimientos de contraseña y las confirmaciones de entrega se siguen enviando después de alcanzarlo.

Con el **proveedor de prueba** (solo para desarrollo y demostraciones) no se envía nada: **Bandeja de salida de SMS (proveedor de prueba)** muestra en su lugar cada mensaje con su texto y su código, con "Segmentos hoy / límite diario".

<!-- SHOT: admin-sms-outbox -->
Figure: La bandeja de salida de SMS del proveedor de prueba.

## Ajustes en la configuración del servidor {#admin-settings}

Algunos interruptores se fijan en la configuración del servidor, no en el panel, y cambiarlos requiere al operador del servidor. Cada uno se activa por separado, cuando lo que necesita está listo:

Table: Interruptores en la configuración del servidor {#tbl-settings}

| Ajuste | Desactivado (hoy) | Activado | Se activa cuando |
|---|---|---|---|
| Proveedor de SMS | Los mensajes solo se escriben en el registro | Los mensajes los envía el proveedor contratado | Se ha comprado el paquete de SMS y se ha registrado el nombre del remitente |
| Entregas con envases | Las plantas registran una entrega con un formulario firmado y una muestra (anterior) | El procedimiento con envases del [capítulo 5](#sao-intake) | Las plantas tienen los frascos de muestra con tapa de seguridad, los precintos y las etiquetas |
| Confirmación obligatoria del aviso | Un aviso en la parte superior pide a los usuarios que confirmen el aviso | El panel permanece cerrado hasta que se confirma el aviso | La oficina de protección de datos ha aprobado los textos de los avisos |
| Portal del apicultor | Los apicultores no reciben datos de acceso | Los responsables de registro envían los datos de acceso; se abre el portal del [capítulo 14](#portal) | El proyecto da el visto bueno al portal |
| Nombres públicos de los apicultores antiguos | – | La Historia de la miel muestra el código neutro hasta que exista un consentimiento de nombre (predeterminado) | – (la alternativa conserva los nombres hasta que el apicultor se registre de nuevo) |
| Ejecución de la conservación de datos | La página **Conservación de datos** solo cuenta | La ejecución es posible (ver más abajo) | El propietario aprueba la primera ejecución y el formato de los seudónimos |

## Conservación de datos {#admin-retention}

**Conservación de datos** muestra cuánto tiempo conserva la plataforma cada tipo de dato y qué ocurre después ([Table](#tbl-retention)). La página es una **simulación**: "cuenta las filas que superan su plazo y no cambia nada". **Informe de simulación (CSV)** descarga los recuentos.

Table: Plazos de conservación {#tbl-retention}

| Datos | Se conserva | Después |
|---|---|---|
| Códigos de un solo uso | 30 días | se borran (solo se guardaron hashes); un código que prueba un registro se queda con él |
| Mensajes SMS | 1 año | se eliminan el número y el texto; se conservan los recuentos |
| Precios | 5 años | se borran |
| Lotes, entradas, envases, muestras, informes de laboratorio, partidas | 5 años después del último consumo preferente de una partida (fecha de la partida + 2 años) o después de la entrega | el vínculo con el apicultor se seudonimiza; se conservan los datos del producto |
| Identidad del apicultor (apicultores inactivos) | mientras esté activo y después el plazo de sus lotes | se seudonimiza |
| Fotos de prueba (formularios de entrega firmados) | lo mismo que el registro que prueban | se borran; se conserva el hash |
| Consentimientos y aceptaciones de avisos | mientras existan los datos relacionados + 3 años (prueba) | se borran |
| Cuentas de antiguos miembros | 2 años después del fin de la última membresía | se anonimizan; la auditoría conserva solo el id del usuario |
| Registro de auditoría | 10 años | no se modifica (solo se añade) |

La ejecución de la conservación está **desactivada** ("Desactivada: la primera ejecución la decide el propietario"). Cuando el propietario la ha aprobado y el operador de la plataforma la ha activado, aparece **Ejecutar la conservación ahora**; requiere la confirmación "El propietario aprobó esta ejecución (borra y vacía datos y no se puede deshacer)." La ejecución borra los códigos de un solo uso antiguos, vacía el número y el texto de los mensajes SMS antiguos y borra los precios antiguos; queda escrita en el registro de auditoría. Las demás reglas solo se cuentan ("solo cuenta (decisión O-14)") hasta que se decida el formato de los seudónimos. Una ejecución mensual solo se puede activar después de una primera ejecución manual.

**Fecha de consumo preferente.** Las partidas no tienen fecha de consumo preferente; la sustituye la fecha de la partida más 2 años (un ajuste). El productor confirma la vida útil real de su miel; el plazo se fija entonces en consecuencia.

## Registro de auditoría {#admin-audit}

Consulte [El registro de auditoría](#trust-audit).
