# 3. Organizaciones, roles y permisos {#roles}

En TOP4HoneyChains nadie actúa por su cuenta: cada acción se realiza **en nombre de una organización y con un rol**. Tres elementos determinan lo que usted puede hacer:

1. las **capacidades** de su organización (qué tipo de trabajo realiza);
2. su **rol** en la organización (quién es usted dentro de ella);
3. el **estado** de su organización: solo una organización **Verificada** puede ingresar datos.

Además, algunas reglas están fijadas en el código y no se pueden desactivar (consulte [Reglas fijas](#fixed-rules)).

## Tipos de organización (capacidades) {#capabilities}

Una organización tiene una o varias capacidades. Las asigna un administrador de la plataforma.

| Capacidad | Organización típica | Qué permite |
|---|---|---|
| **Asociación** | Asociación de apicultores | Vincula a los apicultores miembros; registra el consentimiento de los apicultores para una página pública del apicultor |
| **Asociación central** | Asociación nacional | Opcional: puede verificar asociaciones, si la plataforma está configurada para ello |
| **Procesamiento** | Planta de procesamiento de miel | Registra entregas, toma muestras, registra los PCC 0–3 del lote y los PCC 4–6 de la partida |
| **Envasado** | Planta de llenado y envasado | Registra los PCC 7–9 de la partida (llenado, etiquetado, envasado) y el PCC 10 |
| **Laboratorio** | Laboratorio de análisis (acreditado o propio) | Recibe muestras, ingresa y aprueba informes de laboratorio |
| **Comercio mayorista** | Mayorista | Tiene partidas en su poder; registra el almacenamiento del PCC 10 |
| **Comercio minorista** | Tienda o comercio | Tiene partidas en su poder; registra el almacenamiento del PCC 10 |
| **Autoridad pública** | Ministerio, organismo de inspección | Consulta los registros (solo lectura) |
| **Apicultura** | Explotación apícola | Reservada para las explotaciones apícolas; los apicultores no inician sesión |
| **Administrador de la plataforma** | El operador de la plataforma | Reservada para la organización de la plataforma |

Una planta de procesamiento que además llena y almacena su propia miel necesita **Procesamiento**, **Envasado** y **Comercio mayorista** (o **Comercio minorista**), porque los PCC 7–9 de la partida requieren Envasado y el PCC 10 requiere Comercio mayorista, Comercio minorista o Envasado.

### Estado de la organización {#org-status}

| Estado | Significado |
|---|---|
| **Pendiente** | Organización nueva, a la espera de verificación. Los miembros pueden consultar y el administrador de la organización puede subir documentos. Nadie puede ingresar datos. |
| **Verificada** | Revisada por un administrador de la plataforma. Los miembros pueden trabajar según sus roles. |
| **Suspendida** | Verificación retirada (con un motivo). Funciona como Pendiente hasta que se restablece. |

## Roles {#role-list}

| Rol (en pantalla) | Código | Persona típica | Trabajo principal |
|---|---|---|---|
| **Administrador de la organización** | OrgAdmin | Responsable de la organización | Sube documentos, solicita la finalización, envía y confirma envíos, registra consentimientos de apicultores (asociaciones), consulta recepciones y muestras |
| **Personal** | Staff | Personal de producción, de llenado o de tienda | Ingresa los valores de los PCC 2–10, solicita la finalización, envía y confirma envíos, registra el almacenamiento |
| **Inspector de campo** | FieldInspector | Técnico de campo de la asociación | Consulta registros; ingresa el PCC 0 solo si un administrador asigna ese PCC a la asociación |
| **Responsable de recepción de muestras** | SampleAcceptanceOfficer | Encargado de recepción en la planta | Registra entregas, ingresa el PCC 0, toma y envía muestras |
| **Aprobador de calidad** | QualityApprover | Responsable de calidad, ingeniero de alimentos | Aprueba registros de PCC, confirma la finalización, aprueba registros de almacenamiento |
| **Analista** | Analyst | Analista de laboratorio | Recibe muestras, ingresa resultados de laboratorio, envía informes |
| **Responsable de laboratorio** | LabManager | Director del laboratorio | Recibe muestras, aprueba o devuelve informes de laboratorio |
| **Administrador de la plataforma** | (rol global) | Operador de la plataforma | Administración (capítulo 12); solo lectura sobre los datos de negocio |

Un administrador de la plataforma no forma parte del flujo de trabajo de una organización de negocio: puede consultar todos los registros, pero **no puede ingresar ni aprobar datos de negocio** (entregas, valores de PCC, resultados de laboratorio, envíos, consentimientos). Cuando un dato aprobado es incorrecto, un administrador solicita una *corrección*, que aprueba un segundo administrador (consulte [Correcciones](#admin-corrections)).

## Matriz de permisos {#permission-matrix}

La matriz siguiente muestra los derechos **predeterminados** tal como están configurados hoy. Un administrador de la plataforma puede cambiar la mayoría en **Roles y permisos** y **Responsabilidades de PCC**; las reglas fijas que se indican más abajo no se pueden cambiar.

Leyenda: **V** ver, **C** crear o ingresar, **A** aprobar (o recibir), **F** finalizar (solicitar o confirmar), **–** sin derecho. La columna de capacidad indica qué capacidad necesita la organización.

| Acción | Capacidad necesaria | Administrador de la organización | Personal | Inspector de campo | Responsable de recepción de muestras | Aprobador de calidad | Analista | Responsable de laboratorio | Administrador de la plataforma |
|---|---|---|---|---|---|---|---|---|---|
| Registrar una entrega (recepción) | Procesamiento | V | – | – | C V | – | – | – | V |
| Tomar y enviar una muestra | Procesamiento (propietario del lote) | V | – | – | C V | – | V | V | V |
| Recibir una muestra (control del precinto) | Laboratorio | – | – | – | – | – | A | A | V |
| Ingresar resultados de laboratorio, enviar el informe | Laboratorio | V | V | V | V | V | C V | V | V |
| Aprobar o devolver un informe de laboratorio | Laboratorio | V | V | V | V | V | V | A V | V |
| Control de recepción PCC 0 (lote) | Procesamiento (propietario del lote) | V | V | V | C V | A V | V | V | V |
| Valores de laboratorio PCC 1 (lote) | se toman del informe de laboratorio aprobado | V | V | V | V | V | V | V | V |
| PCC 2–3 (lote) | Procesamiento (propietario del lote) | V | C V | V | V | A V | V | V | V |
| Crear una partida, componerla, asignar los PCC | – | – | – | – | – | – | – | – | C |
| PCC 4–6 (partida) | Procesamiento (propietario de la partida) | V | C V | V | V | A V | V | V | V |
| PCC 7–9 (partida) | Envasado (propietario de la partida) | V | C V | V | V | A V | V | V | V |
| PCC 10 almacenamiento en la planta (partida) | Comercio mayorista, Comercio minorista o Envasado (propietario de la partida) | V | C V | V | V | A V | V | V | V |
| Solicitar la finalización | propietario, verificada | F | F | – | – | – | – | – | – |
| Confirmar la finalización (sellar) | propietario, verificada | – | – | – | – | F | – | – | – |
| Crear un envío | poseedor, verificada | C V | C V | – | – | V | – | – | V |
| Confirmar o rechazar un envío recibido | receptor, verificada | A V | A V | – | – | V | – | – | V |
| Registro de almacenamiento PCC 10 en el receptor | Comercio mayorista, Comercio minorista o Envasado | V | C V | V | V | A V | V | V | V |
| Informe de retiro (ámbito propio) | – | V | V | V | V | V | V | V | V (todo) |
| Registrar o retirar el consentimiento de un apicultor | Asociación | C V | – | – | – | – | – | – | V |
| Subir documentos de la organización | organización propia | C | – | – | – | – | – | – | C |
| Verificar organizaciones, gestionar miembros y capacidades | – | – | – | – | – | – | – | – | C A |
| Verificar números de registro de apicultores | – | – | – | – | – | – | – | – | A |
| Cambiar permisos, PCC, límites, traducciones | – | – | – | – | – | – | – | – | C |
| Solicitar o decidir una corrección | – | – | – | – | – | – | – | – | C A (dos administradores distintos) |
| Registro de auditoría | – | – | – | – | – | – | – | – | V |

Notas:

- **Ver los PCC (V)**: cualquier rol puede abrir los PCC de los lotes y las partidas en los que participa su organización: como propietaria u organización responsable, como organización que tomó una muestra (en el caso de los lotes), como poseedora posterior de la partida o como autoridad pública verificada. Un poseedor ve los PCC de la partida y los de sus lotes **en modo de solo lectura**. Cualquier otra organización recibe la página "No puede abrir esta página" (403).
- El **PCC 1** se completa a partir del informe de laboratorio aprobado. Sus roles de ingreso configurados (Responsable de recepción de muestras, Personal) solo se usan para los parámetros del PCC 1 que el informe de laboratorio no cubre.
- **Los envíos y la finalización** comprueban el rol y el estado de la organización, no una capacidad: cualquier propietario verificado de una partida finalizada puede enviarla.
- El administrador puede asignar el **PCC 0** a la asociación del apicultor en lugar de a la planta (**Responsabilidades de PCC** → PCC 0 → "La asociación del apicultor del lote", capacidad Asociación, rol Inspector de campo). No es la configuración predeterminada.

<!-- SHOT: admin-permissions-matrix -->

## Reglas fijas {#fixed-rules}

Estas reglas forman parte de la plataforma:

- **Doble verificación en los registros de PCC**: nadie aprueba un registro de PCC en el que haya registrado valores o que haya marcado como "No aplicable". Excepción: un administrador de la plataforma puede conceder a una organización la **Exención de la doble verificación (cuatro ojos)**; entonces sus miembros pueden aprobar sus propios registros de PCC, y cada una de esas aprobaciones queda marcada en el registro de auditoría.
- **Doble verificación en los resultados de laboratorio, siempre**: la persona que ingresó los resultados no puede aprobarlos. No hay exención.
- **Doble verificación en la finalización**: la persona que solicitó la finalización no puede confirmarla (salvo que la organización tenga la exención).
- **Solo las organizaciones verificadas ingresan datos.**
- **Los administradores de la plataforma tienen solo lectura** sobre los datos de negocio y solo los corrigen mediante el proceso de corrección con dos administradores.
- **Finalizado significa bloqueado**: una partida finalizada y todos sus lotes ya no se pueden modificar, salvo mediante una corrección.
- **Al registro de auditoría solo se le pueden añadir entradas**; nada de lo que contiene se modifica ni se elimina, y cada cambio en las páginas de permisos también queda registrado.
