# 2. Capacidades, organizaciones y roles {#roles}

En TOP4HoneyChains nadie actúa por su cuenta: cada acción se realiza **en nombre de una organización y con un rol**. Tres elementos determinan lo que usted puede hacer:

1. las **capacidades** de su organización (qué tipo de trabajo realiza);
2. su **rol** en la organización (quién es usted dentro de ella);
3. el **estado** de su organización: solo una organización **Verificada** puede ingresar datos.

Además, algunas reglas están fijadas en el código y no se pueden desactivar (consulte [Reglas fijas](#fixed-rules)).

## Capacidades {#capabilities}

Una capacidad indica qué trabajo puede hacer una organización en la cadena de la miel. Qué miembro hace el trabajo lo fijan los roles (consulte [Roles](#role-list)). Una organización tiene una o varias capacidades; un administrador de la plataforma las asigna y las quita (consulte [Capacidades](#admin-org-capabilities)). La lista de capacidades en sí es fija.

| Capacidad (en pantalla) | Qué significa (descripción en pantalla) | Qué puede hacer la organización | PCC (predeterminado) |
|---|---|---|---|
| **Asociación** | Asociación de apicultores; vincula a sus apicultores miembros y registra su consentimiento para una página pública de apicultor | Vincula a sus apicultores miembros, registra y retira su consentimiento y publica sus páginas de apicultor (**Páginas de apicultores**); ve los lotes de sus apicultores miembros, por ejemplo en los informes de retirada | ninguno; el PCC 0 solo si un administrador lo asigna a la asociación |
| **Procesamiento** | Recepción y procesamiento: entregas, muestras, PCC 0–3 del lote y PCC 4–6 de la partida | Registra entregas, lo que crea los lotes de miel cruda de los que es propietaria; los controla en la recepción; toma muestras precintadas y las envía a un laboratorio; registra los PCC de procesamiento (**Entregas (recepción)**, **Muestras y laboratorio**) | PCC 0–3 del lote, PCC 4–6 de la partida |
| **Envasado** | Llenado, etiquetado y envasado, PCC 7-9 | Registra el llenado, el etiquetado y el envasado de sus partidas y el almacenamiento del PCC 10 en la planta | PCC 7–9 de la partida, PCC 10 |
| **Laboratorio** | Laboratorio (acreditado o propio) | Recibe muestras precintadas, ingresa los resultados de laboratorio y aprueba el informe; una segunda persona siempre lo aprueba. El número de acreditación y su fecha de validez se guardan en esta capacidad (**Muestras y laboratorio**) | ninguno; sus resultados aprobados completan el PCC 1 del lote |
| **Comercio mayorista** | Comercio mayorista; poseedor en el PCC 10 mientras tiene la mercancía | Tiene en su poder las partidas que ha recibido, registra el almacenamiento del PCC 10 de la cantidad que tiene y puede reenviarlas (**Envíos y existencias**) | PCC 10 |
| **Comercio minorista** | Comercio minorista; poseedor en el PCC 10 mientras tiene la mercancía | Recibe envíos para la venta, registra el almacenamiento del PCC 10 de la cantidad que tiene y puede reenviarlos (**Envíos y existencias**) | PCC 10 |

Una planta de procesamiento que además llena y almacena su propia miel necesita **Procesamiento**, **Envasado** y **Comercio mayorista** (o **Comercio minorista**), porque los PCC 7–9 de la partida requieren Envasado y el PCC 10 requiere Comercio mayorista, Comercio minorista o Envasado.

La lista incluye además tres capacidades para casos especiales:

- **Asociación central**: una asociación nacional de apicultores. Si la plataforma está configurada para que la asociación central verifique las asociaciones, el administrador de la organización de una asociación central verificada verifica las asociaciones y ve las asociaciones que verifica.
- **Autoridad pública**: un ministerio o un organismo de inspección. Una vez verificada, sus miembros consultan todos los registros; no ingresa nada.
- **Apicultura**: la explotación de un apicultor. Los apicultores no inician sesión; son registros que mantienen la plataforma y su asociación.

**La plataforma.** *Administrador de la plataforma* no es una capacidad que se pueda asignar a una organización: es un rol global de una cuenta de usuario, para las personas que gestionan la plataforma. Los administradores de la plataforma gestionan organizaciones, miembros, datos de referencia y ajustes y consultan todos los registros, pero no ingresan datos de negocio (consulte el [capítulo 12](#admin)). La organización propia de la plataforma tiene la capacidad reservada **Administrador de la plataforma**; no se puede asignar a ninguna otra organización ni quitar.

## Tipos de organización {#org-types}

La plataforma no tiene una lista aparte de tipos de organización: el tipo de una organización es el conjunto de capacidades que tiene. Organizaciones típicas:

| Organización | Capacidades | Roles típicos de sus miembros |
|---|---|---|
| Asociación de apicultores | **Asociación** | Administrador de la organización; Inspector de campo |
| Planta de procesamiento o envasador | **Procesamiento**, **Envasado** y **Comercio mayorista** o **Comercio minorista** para el almacenamiento en la planta | Administrador de la organización, Responsable de recepción de muestras, Personal, Aprobador de calidad |
| Laboratorio (acreditado o propio) | **Laboratorio** | Administrador de la organización, Analista, Responsable de laboratorio |
| Mayorista | **Comercio mayorista** | Administrador de la organización, Personal, Aprobador de calidad |
| Tienda o comercio | **Comercio minorista** | Administrador de la organización, Personal, Aprobador de calidad |
| Ministerio u organismo de inspección | **Autoridad pública** | Administrador de la organización (solo lectura) |

Una asociación que gestiona su propia planta de procesamiento tiene también **Procesamiento** (y **Envasado**) y, en ese caso, también los roles de una planta.

### Estado de la organización {#org-status}

| Estado | Significado |
|---|---|
| **Pendiente** | Organización nueva, a la espera de verificación. Los miembros pueden consultar y el administrador de la organización puede subir documentos. Nadie puede ingresar datos. |
| **Verificada** | Revisada por un administrador de la plataforma. Los miembros pueden trabajar según sus roles. |
| **Suspendida** | Verificación retirada (con un motivo). Funciona como Pendiente hasta que se restablece. |

## Roles {#role-list}

Un rol se tiene a través de una **membresía**: una persona, una organización, un rol. Las membresías las crean los administradores de la plataforma (consulte [Miembros](#admin-members)); una persona puede tener varias (consulte [Varias membresías](#memberships)).

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

## Varias membresías {#memberships}

Una persona puede tener varias membresías: en organizaciones distintas o con varios roles en la misma organización (por ejemplo, **Aprobador de calidad** y **Administrador de la organización** en una asociación). Siempre se trabaja con una sola membresía a la vez.

Para cambiar:

1. Haga clic en la **etiqueta organización · rol** de la barra superior (consulte [La barra superior y su rol](#top-bar)). Una lista con el título **Sus roles** muestra todas las membresías activas; la actual aparece marcada.
2. Haga clic en otra membresía. La plataforma abre **Mi organización** con la nueva membresía.

El menú, las tareas de **Mis tareas**, los registros que puede ver y las acciones que puede realizar dependen de la membresía activa. La elección se mantiene hasta que cierre el navegador; después, vuelve a empezar con su primera membresía (por orden alfabético del nombre de la organización). Si tiene dos o más membresías, **Mi organización** también ofrece una lista **Actuar como** con un botón **Cambiar**.
