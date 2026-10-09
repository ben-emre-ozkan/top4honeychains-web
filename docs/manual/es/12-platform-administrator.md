# 12. Administrador de la plataforma {#admin}

## Su trabajo diario {#admin-daily}

Los administradores de la plataforma la gestionan. Verifican organizaciones, gestionan miembros y permisos, mantienen los datos de referencia y las definiciones de los puntos de control, registran a los apicultores, crean partidas (por ahora), deciden sobre las correcciones y supervisan el registro de auditoría.

Los administradores pueden consultar todos los registros, pero **no ingresan ni aprueban datos de negocio**. Al iniciar sesión llegan a **Mis tareas**, que muestra **Organizaciones por verificar**, **Números de registro por verificar** y **Correcciones por decidir**. El **Panel** ofrece una visión general (cifras, la cadena, un mapa de apicultores y apiarios por provincia, la actividad reciente y las aprobaciones pendientes).

<!-- SHOT: admin-dashboard -->

El menú de administración:

| Menú | Entradas |
|---|---|
| **Panel** | Visión general |
| **Apicultores** | Lista de apicultores, apiarios, niveles de educación, propósitos de la apicultura, tipos de apicultura |
| **Producción de Miel Cruda** | Lotes de miel cruda, tipos de miel, períodos de producción |
| **Mayoristas** | Lista de mayoristas, agregar un mayorista |
| **Producciones Finales de Miel** | Partidas |
| **Puntos Críticos de Control** | PCC, categorías de parámetros, parámetros, traducciones de datos de referencia |
| **Gestión de Usuarios** | Usuarios, roles, **Registro de auditoría** |
| **Organizaciones** | Mis tareas, organizaciones, capacidades, registro, roles y permisos, responsabilidades de PCC, correcciones, envíos, muestras, páginas de apicultores |

## Organizaciones {#admin-organizations}

### Crear una organización {#admin-org-create}

1. **Organizaciones** → **Nueva organización**.
2. Ingrese el nombre, la razón social, el número de identificación fiscal, la provincia y el distrito.
3. Si la organización es un mayorista que ya existe en la plataforma, elíjalo en el campo del mayorista existente. **Una planta de procesamiento debe estar vinculada a su registro de mayorista**: los lotes que recibe se archivan bajo ese mayorista, y una partida solo puede usar lotes de su propio mayorista.
4. Marque al menos una capacidad y haga clic en **Crear**.

La organización empieza en estado **Pendiente**: "Organización creada. Permanece en estado Pendiente hasta que sea verificada."

<!-- SHOT: admin-org-create -->

### Verificar, solicitar documentos, suspender {#admin-org-verify}

En la página de la organización:

- **Verificar** (desde Pendiente) requiere al menos un documento subido.
- **Solicitar documentos** (mientras está Pendiente) requiere un motivo, por ejemplo qué documento falta.
- **Suspender** (mientras está Verificada) requiere un motivo. **Restablecer** vuelve a verificar una organización suspendida.

Cada cambio aparece en el historial de verificación y en el registro de auditoría.

### Capacidades {#admin-org-capabilities}

**Agregar capacidad** agrega una capacidad. Para **Laboratorio**, marque acreditado e ingrese el número de acreditación y su fecha de validez; un laboratorio acreditado necesita un número. **Quitar** elimina una capacidad; una organización conserva siempre al menos una. La página **Capacidades** muestra, por capacidad, qué PCC la requieren y qué organizaciones la tienen. La pestaña **Capacidades de la organización** de **Roles y permisos** permite editar todas las organizaciones a la vez.

### Miembros {#admin-members}

1. En la página de la organización, en la sección de miembros, elija el rol.
2. Ingrese un nombre de usuario existente, o un nombre de usuario, correo electrónico y contraseña nuevos para crear el usuario al mismo tiempo.
3. Haga clic en **Agregar miembro**: "… agregado como …".

Un usuario puede tener varias membresías. **Revocar** termina una membresía y cierra la sesión del usuario en todos los dispositivos. La pestaña **Miembros** de **Roles y permisos** muestra todas las membresías de todas las organizaciones.

<!-- SHOT: admin-org-members -->

### Exención de la doble verificación {#admin-four-eyes}

Las organizaciones pequeñas con una sola persona de calidad pueden recibir la **Exención de la doble verificación (cuatro ojos)**: sus miembros pueden entonces aprobar sus propios registros de PCC y confirmar sus propias solicitudes de finalización. Cada una de esas aprobaciones queda marcada en el registro de auditoría. La exención nunca se aplica a los resultados de laboratorio.

## Usuarios {#admin-users}

**Gestión de Usuarios** → **Usuarios** muestra las cuentas.

- **Agregar nuevo usuario** crea una cuenta. Con el modelo de organizaciones, deje vacío el **Rol global** para los miembros de organizaciones y asígneles una membresía en la página de la organización; elija **Administrador de la plataforma** solo para los administradores.
- **Restablecer contraseña** asigna una contraseña nueva a un usuario.
- **Eliminar** borra una cuenta (úselo con cuidado; no se puede deshacer).

Las contraseñas deben tener al menos 6 caracteres. Tras 5 intentos fallidos de inicio de sesión, la cuenta se bloquea durante 15 minutos. No hay un botón de desbloqueo; el bloqueo termina por sí solo.

## Roles y permisos {#admin-permissions}

**Roles y permisos** tiene tres pestañas:

- **Miembros**: todas las membresías activas; agregar y revocar.
- **Permisos por rol**: elija un rol y marque, para cada función, **Introducir**, **Aprobar** y **Leer**. Las funciones son los PCC (una fila cada uno) y las áreas de recepción, muestras, resultados de laboratorio, envíos, páginas de apicultores, retiro y finalización. Cada derecho debe conservar al menos un rol. Los cambios se aplican de inmediato.
- **Capacidades de la organización**: las capacidades de cada organización.

El recuadro "Reglas fijas (no se pueden desactivar):" enumera las reglas que ninguna configuración modifica (consulte [Reglas fijas](#fixed-rules)).

<!-- SHOT: admin-role-permissions -->

## Responsabilidades de PCC {#admin-ccp-responsibilities}

**Responsabilidades de PCC** define, para cada PCC, qué organización es responsable (el propietario del lote o de la partida o, en el caso de los lotes, la asociación del apicultor), qué capacidad necesita, qué roles ingresan los valores y cuáles los aprueban, y si se requiere aprobación. Ejemplo: asigne el PCC 0 a la asociación eligiendo "La asociación del apicultor del lote", **Asociación** e **Inspector de campo**.

## Puntos de control, parámetros y límites {#admin-ccps}

- **Puntos Críticos de Control**: código, título, etapa del proceso, justificación de la criticidad y **Se aplica a** (lote de miel cruda o partida final). La opción de mostrar valores de texto libre en la Historia de la miel pública debe permanecer desactivada cuando las respuestas puedan contener datos personales.
- **Parámetros**: nombre, unidad, categoría, tipo de dato (**Número**, **Sí / No**, **Texto**, **Archivo**), mínimo y máximo, obligatorio u opcional, y "obligatorio solo cuando" otra pregunta de sí/no tiene una respuesta determinada.
- **Límites**: por parámetro, límites predeterminados para todos los tipos de miel y límites específicos por clase de miel (de flores, de mielada, de pino) o por tipo de miel, opcionalmente con una condición, una suma o una proporción. Los resultados almacenados no se recalculan cuando cambian los límites.

**Límites verificados.** Cada parámetro tiene la casilla **Límites verificados (se muestran públicamente)**. Solo los límites verificados producen un veredicto público dentro/fuera del límite en la Historia de la miel y en la página del apicultor. En el panel, los límites no verificados aparecen marcados como "por verificar", y las páginas públicas indican que los límites todavía se están revisando.

<!-- SHOT: admin-parameter-limits -->

## Datos de referencia y traducciones {#admin-reference}

Los tipos de miel (con su clase de miel para los límites), los períodos de producción, los niveles de educación, los tipos y propósitos de la apicultura, y los mayoristas se mantienen en sus menús. Un registro que está en uso no se puede eliminar.

**Traducciones de datos de referencia** contiene los nombres que se muestran en listas, tablas y páginas públicas en los cinco idiomas: tipos de miel, períodos, estados, capacidades, roles, PCC, parámetros, categorías de parámetros, notas de límites, explicaciones de parámetros y países. Un campo vacío recurre al inglés y, después, al texto base.

## Apicultores, apiarios y el registro {#admin-registry}

- **Apicultores**: agregue un apicultor (nombre, fecha de nacimiento, experiencia, teléfono, educación, tipo y propósito de la apicultura, foto opcional). El número de explotación se genera automáticamente. Los apicultores no inician sesión. Nunca se eliminan; solo se desactivan.
- **Apiarios**: título, número de colmenas y ubicación (latitud y longitud). La ubicación nunca se muestra públicamente; las páginas públicas muestran solo la provincia.
- **Registro de apicultores y asociaciones**: para cada apicultor, vincule la asociación e ingrese el número de registro de la explotación (formato `TR` + código de provincia + 10 dígitos, por ejemplo `TR061234567890`) con un documento probatorio. Después, haga clic en **Verificar** o **Rechazar** (para rechazar se requiere un motivo).

<!-- SHOT: admin-registry -->

## Partidas (por ahora, solo administradores) {#admin-batches}

1. **Mayoristas** → el mayorista → **Producciones finales de miel (partidas)** → **Nueva partida**.
2. Ingrese el **Título de la partida** y la **Fecha de producción**, y haga clic en **Crear partida**. Se genera el código QR.
3. En **Gestionar partida**, ingrese la cantidad que se usará de cada lote elegible (lotes de este mayorista con todos sus PCC aprobados) y haga clic en **Guardar composición de la partida**. Las cantidades superiores a la disponible se ignoran.
4. Use **Editar** en la partida para definir la organización propietaria (la planta).
5. **Gestionar PCC**: marque del PCC 4 al PCC 10 y haga clic en **Guardar asignaciones**.

Después, la planta ingresa los PCC de la partida y la finaliza. Los administradores no pueden finalizar: "Con el modelo de organizaciones, la organización propietaria finaliza la partida (solicitud y aprobación por dos personas)."

<!-- SHOT: admin-manage-batch -->

## Correcciones {#admin-corrections}

Los datos aprobados o bloqueados solo se modifican mediante una corrección:

1. Abra la página de PCC del registro. El recuadro **Correcciones** le permite elegir el parámetro, ingresar el valor nuevo y un **motivo**, y hacer clic en **Solicitar**: "Corrección solicitada; otro administrador de la plataforma debe aprobarla."
2. Un **segundo** administrador abre **Correcciones**, lee el valor anterior, el nuevo y el motivo, y hace clic en **Aprobar y aplicar** o en **Rechazar** (para rechazar se requiere una nota).

Quien solicitó una corrección no puede decidir sobre ella. Las correcciones también se aplican a registros bloqueados, y ambos pasos quedan en el registro de auditoría.

## Páginas de apicultores, envíos y muestras {#admin-overviews}

Los administradores pueden abrir **Páginas de apicultores**, **Envíos y existencias** y **Muestras y laboratorio** en modo de solo lectura, para seguir el trabajo de todas las organizaciones.

## Retiro {#admin-recall}

El **Informe de retiro** de una partida o de un lote muestra:

- **aguas arriba**: los lotes, los apiarios, los apicultores y los formularios de entrega;
- las partidas;
- **aguas abajo**: los envíos y los poseedores actuales.

Los miembros ven la parte que su organización puede ver (propietario, poseedor o asociación); las filas ocultas se cuentan. **Imprimir** y **CSV** exportan el informe.

<!-- SHOT: recall-report -->

## Registro de auditoría {#admin-audit}

Consulte [El registro de auditoría](#trust-audit).
