# 2. Capacidades, organizaciones y roles {#roles}

En TOP4HoneyChains nadie actúa por su cuenta: cada acción se realiza **en nombre de una organización y con un rol**. Tres elementos determinan lo que usted puede hacer:

1. las **capacidades** de su organización: lo que es capaz de hacer en la cadena de la miel;
2. su **rol** en la organización: quién es usted dentro de ella;
3. el **estado** de su organización: solo una organización **Verificada** puede ingresar datos. Las organizaciones las verifica un administrador de la plataforma (consulte [Estado de la organización](#org-status)).

Un rol se tiene a través de una **membresía**: una cuenta de usuario × una organización × un rol (consulte [Varias membresías](#memberships)). Los apicultores son la única excepción: un apicultor puede tener una cuenta personal para el portal del apicultor, vinculada a su registro de apicultor y sin ninguna membresía (consulte el [capítulo 14](#portal)). Además, algunas reglas forman parte de la plataforma y no se pueden desactivar (consulte [Reglas básicas](#fixed-rules)).

Este capítulo distingue dos cosas:

- las **actividades de la cadena de valor**: el trabajo que las personas hacen con la miel fuera del ordenador, como registrar, entregar, tomar muestras, analizar, procesar, almacenar y enviar ([Table](#tbl-activities));
- los **derechos sobre los datos**: lo que cada rol puede hacer con los registros de la plataforma: crear, leer, modificar, eliminar y aprobar ([Table](#tbl-crud)).

## Capacidades {#capabilities}

Una capacidad es una aptitud de una organización: indica lo que la organización es **capaz de hacer** en la cadena de la miel. Qué miembro hace el trabajo lo fijan los roles (consulte [Roles](#role-list)). Una organización tiene una o varias capacidades; un administrador de la plataforma las asigna y las quita (consulte [Capacidades](#admin-org-capabilities)). La lista de capacidades en sí es fija. La [Table](#tbl-capabilities) las enumera con su nombre en pantalla.

Table: Capacidades: lo que una organización es capaz de hacer {#tbl-capabilities}

| Capacidad (en pantalla) | Aptitud | PCC (predeterminado) |
|---|---|---|
| **Asociación** | Capaz de vincular a sus apicultores miembros y registrar su consentimiento para una página pública de apicultor. | ninguno; el PCC 0 solo si un administrador lo asigna a la asociación |
| **Procesamiento** | Capaz de recibir y procesar miel: entregas, muestras, PCC 0–3 del lote y PCC 4–6 de la partida. | PCC 0–3 del lote, PCC 4–6 de la partida |
| **Envasado** | Capaz de llenar, etiquetar y envasar miel: PCC 7–9 de la partida y el almacenamiento del PCC 10 en la planta. | PCC 7–9 de la partida, PCC 10 |
| **Laboratorio** | Capaz de analizar muestras de miel y emitir informes de laboratorio, como laboratorio de análisis de alimentos acreditado o propio. El número de acreditación y su fecha de validez se guardan en esta capacidad. | ninguno; sus resultados aprobados completan el PCC 1 del lote |
| **Comercio mayorista** | Capaz de tener partidas en el comercio mayorista, registrar su almacenamiento (PCC 10) y reenviarlas. | PCC 10 |
| **Comercio minorista** | Capaz de tener partidas para la venta a los consumidores, registrar su almacenamiento (PCC 10) y reenviarlas. | PCC 10 |
| **Registro de apicultores** | Capaz de registrar a los apicultores de su zona y sus apiarios, dar fe de su identidad y de su número de registro, registrar su consentimiento y emitir sus datos de acceso. | ninguno |
| **Asociación central** | Capaz de verificar asociaciones de apicultores, cuando la plataforma está configurada para ello. | ninguno |
| **Autoridad pública** | Capaz de leer todos los registros, como ministerio u organismo de inspección; no ingresa nada. | ninguno (lee todo) |
| **Explotación apícola** | Capaz de producir miel. Los apicultores no tienen membresías; su explotación está representada por un registro de apicultor. | ninguno |

Una planta de procesamiento que además llena y almacena su propia miel necesita **Procesamiento**, **Envasado** y **Comercio mayorista** (o **Comercio minorista**), porque los PCC 7–9 de la partida requieren Envasado y el PCC 10 requiere Comercio mayorista, Comercio minorista o Envasado. Las propias acciones también comprueban la capacidad: tomar una muestra requiere **Procesamiento**; enviar y recibir requieren **Procesamiento**, **Envasado**, **Comercio mayorista** o **Comercio minorista**; solicitar y confirmar la finalización requieren **Procesamiento** o **Envasado**.

**La plataforma.** *Administrador de la plataforma* no es una capacidad que se pueda asignar a una organización: es un rol global de una cuenta de usuario, para las personas que gestionan la plataforma. Los administradores de la plataforma gestionan organizaciones, membresías, datos de referencia y ajustes y leen todos los registros, pero no ingresan datos del flujo de trabajo (consulte el [capítulo 15](#admin)). La organización propia de la plataforma tiene la capacidad reservada **Administrador de la plataforma**; no se puede asignar a ninguna otra organización ni quitar.

## Actividades de la cadena de valor {#activities}

Las actividades de la cadena de valor las realizan personas, con la miel, fuera del ordenador. La plataforma registra el resultado de cada actividad como datos. La [Table](#tbl-activities) muestra, por capacidad, qué actividades realizan sus personas y qué se registra en la plataforma.

Table: Actividades de la cadena de valor (personas) y lo que registra la plataforma (ordenador) {#tbl-activities}

| Capacidad | Actividades de la cadena de valor (personas, fuera del ordenador) | Registrado en la plataforma (en el ordenador) |
|---|---|---|
| Explotación apícola | Criar abejas, cosechar la miel, entregarla en envases a una planta, leer el código SMS a la planta, fijar un precio solicitado | Registros de apicultor y de apiario; la confirmación de la entrega (código SMS); en el portal: el precio solicitado y los consentimientos del propio apicultor |
| Registro de apicultores | Reunirse con el apicultor, comprobar el documento de identidad y el registro, leerle el aviso informativo | Registros de apicultor y de apiario, el registro (código SMS o foto firmada), los consentimientos dados en el registro, los datos de acceso del apicultor |
| Asociación | Vincular e informar a los apicultores miembros, leerles el texto de consentimiento, recoger las firmas | Registros de consentimiento; páginas públicas de apicultores |
| Procesamiento | Recibir entregas en envases, comprobar o aplicar precintos, llenar un frasco de muestra y un frasco de contramuestra de cada envase, calentar, filtrar, mezclar y decantar la miel | Entrega con sus envases y su historial de precintos, los frascos de muestra, datos de los PCC 0 y 2–6 |
| Envasado | Llenar, etiquetar y envasar la miel, almacenarla en la planta | Datos de los PCC 7–10 |
| Laboratorio | Recibir los frascos precintados y comprobar precinto, etiqueta y tapa; preparar la muestra compuesta; analizar la miel; firmar el informe | Controles de recepción, muestra compuesta, informe de laboratorio (PDF) y resultados (PCC 1) |
| Comercio mayorista, Comercio minorista | Recibir la mercancía, almacenarla, venderla o reenviarla | Confirmación del envío, registro de almacenamiento (PCC 10), nuevos envíos |
| Asociación central | Controlar las asociaciones de apicultores | Verificación de asociaciones (si está configurada) |
| Autoridad pública | Inspeccionar | Lee los registros |

Cada organización, tenga las capacidades que tenga, cuenta además con un **Responsable de cumplimiento**: la persona que supervisa las aprobaciones, las pruebas y la protección de datos (consulte el [capítulo 13](#role-co)).

## Tipos de organización {#org-types}

La plataforma no tiene una lista aparte de tipos de organización: el tipo de una organización es el conjunto de capacidades que tiene. La [Table](#tbl-org-types) enumera organizaciones típicas.

Table: Organizaciones típicas y sus capacidades {#tbl-org-types}

| Organización | Capacidades | Roles típicos de sus miembros |
|---|---|---|
| Dirección distrital de agricultura | **Registro de apicultores** | Administrador de la organización, Responsable de registro de apicultores, Responsable de cumplimiento |
| Asociación de apicultores | **Asociación** | Administrador de la organización, Inspector de campo, Responsable de cumplimiento |
| Planta de procesamiento o envasador | **Procesamiento**, **Envasado** y **Comercio mayorista** o **Comercio minorista** para el almacenamiento en la planta | Administrador de la organización, Personal, Responsable de recepción de muestras, Responsable de aseguramiento de la calidad, Responsable de cumplimiento |
| Laboratorio de análisis de alimentos (acreditado o propio) | **Laboratorio** | Administrador de la organización, Analista, Responsable de laboratorio, Responsable de cumplimiento |
| Mayorista | **Comercio mayorista** | Administrador de la organización, Personal, Responsable de aseguramiento de la calidad, Responsable de cumplimiento |
| Minorista | **Comercio minorista** | Administrador de la organización, Personal, Responsable de aseguramiento de la calidad, Responsable de cumplimiento |
| Ministerio u organismo de inspección | **Autoridad pública** | Administrador de la organización (solo lectura) |

Una asociación que gestiona su propia planta de procesamiento tiene también **Procesamiento** (y **Envasado**) y, en ese caso, también los roles de una planta. En una organización pequeña, el Responsable de cumplimiento puede ser el administrador de la organización con una segunda membresía; las reglas de los cuatro ojos se siguen aplicando.

### Estado de la organización {#org-status}

Un administrador de la plataforma verifica, suspende y restablece las organizaciones. Para verificar hace falta al menos un documento subido; para suspender hace falta un motivo. Una organización que se verifica por primera vez necesita además un **Responsable de cumplimiento** activo; una organización verificada antes sin él muestra un aviso con la fecha en la que debe nombrarlo, y no se bloquea. La [Table](#tbl-org-status) enumera los estados.

**Asociación central.** Si la plataforma está configurada para ello, el administrador de la organización de una **Asociación central** verificada verifica, suspende y restablece las asociaciones de apicultores, y entonces es el **único** que lo hace: los administradores de la plataforma ya no verifican asociaciones, y las asociaciones salen de su lista de espera.

Table: Estado de la organización {#tbl-org-status}

| Estado | Significado |
|---|---|
| **Pendiente** | Organización nueva, a la espera de verificación. Los miembros pueden leer y el administrador de la organización puede subir documentos. Nadie puede ingresar datos. |
| **Verificada** | Revisada por un administrador de la plataforma (o, en el caso de las asociaciones, por la asociación central si está configurado). Los miembros pueden trabajar según sus roles. |
| **Suspendida** | Verificación retirada (con un motivo). Funciona como Pendiente hasta que se restablece. |

## Roles {#role-list}

La [Table](#tbl-roles) es la lista de referencia de los roles. Todas las demás tablas de este manual usan los mismos roles en el mismo orden. Las tablas de derechos sobre los datos usan las abreviaturas de la segunda columna.

Table: Roles {#tbl-roles}

| Rol (en pantalla) | Abrev. | Código | Persona típica | Trabajo principal |
|---|---|---|---|---|
| **Administrador de la organización** | AO | OrgAdmin | Responsable de la organización | Sube documentos, solicita la finalización, envía y confirma envíos, registra consentimientos de apicultores (asociaciones), lee entregas, muestras y el precio solicitado de los lotes que posee |
| **Personal** | Pe | Staff | Personal de producción, de llenado o de comercio minorista | Ingresa los datos de los PCC 1–10, solicita la finalización, envía y confirma envíos, registra el almacenamiento |
| **Inspector de campo** | IC | FieldInspector | Técnico de campo de la asociación | Lee registros; ingresa el PCC 0 solo si un administrador asigna ese PCC a la asociación |
| **Responsable de recepción de muestras** | RRM | SampleAcceptanceOfficer | Encargado de recepción en la planta | Registra entregas con sus envases precintados, envía el código SMS del apicultor y lo introduce, toma una muestra de cada envase, ingresa el PCC 0 |
| **Responsable de aseguramiento de la calidad** | RAC | QualityApprover | Responsable de calidad, ingeniero de alimentos | Aprueba registros de PCC y registros de almacenamiento, confirma la finalización |
| **Analista** | An | Analyst | Analista de laboratorio | Recibe muestras (control de precinto, etiqueta y tapa), ingresa resultados de laboratorio, envía informes (la primera persona en el laboratorio) |
| **Responsable de laboratorio** | RL | LabManager | Director del laboratorio | Recibe muestras (control de precinto, etiqueta y tapa), aprueba o devuelve informes de laboratorio (la segunda persona en el laboratorio) |
| **Responsable de registro de apicultores** | RRA | RegistrationOfficer | Funcionario de una dirección distrital de agricultura | Registra a los apicultores y sus apiarios, comprueba el número de registro, envía el código SMS del apicultor, da fe de la identidad, registra el aviso y los consentimientos, emite los datos de acceso del apicultor |
| **Responsable de cumplimiento** | RC | ComplianceOfficer | Responsable de calidad o de protección de datos de cualquier organización | Revisa las pruebas con foto firmada, la calidad del muestreo y las aprobaciones, mantiene los datos del responsable del tratamiento, atiende las solicitudes de los interesados y las retiradas de consentimiento, lee las entradas de auditoría de la organización |
| **Administrador de la plataforma** | AP | (rol global) | Operador de la plataforma | Administración ([capítulo 15](#admin)); lee todos los datos del flujo de trabajo, pero no ingresa ni aprueba ninguno; también puede registrar apicultores |
| **Apicultor** (cuenta del portal) | Api | (sin membresía) | El apicultor | Lee sus propios apiarios, entregas, muestras, resultados de laboratorio, partidas y páginas de la Historia de la miel; indica el precio solicitado de su miel; otorga y retira sus propios consentimientos ([capítulo 14](#portal)) |

Un administrador de la plataforma no forma parte del flujo de trabajo de una organización de negocio: puede leer todos los registros, pero **no puede ingresar ni aprobar datos del flujo de trabajo** (entregas, datos de PCC, resultados de laboratorio, envíos, consentimientos). Cuando un dato es incorrecto después de su aprobación, un administrador solicita una *corrección*, que aprueba un segundo administrador (consulte [Correcciones](#admin-corrections)). Los administradores sí mantienen los datos maestros: apicultores, apiarios, lotes, partidas y su composición (consulte el [capítulo 15](#admin)).

## Derechos sobre los datos (CRUD) {#crud}

La [Table](#tbl-crud) muestra los derechos predeterminados de cada rol sobre cada tipo de registro, según el modelo CRUD: **C** crear (*create*), **R** leer (*read*), **U** modificar (*update*), **D** eliminar (*delete*) y **A** aprobar (un derecho aparte). "–" significa sin derecho. Api es la cuenta del portal del apicultor y Púb, un visitante público sin cuenta. Los derechos de un miembro se aplican solo a los registros del trabajo de su propia organización (sus propios lotes y partidas, las partidas que tiene en su poder, los apicultores de su asociación, los registros de su dirección) y, salvo la lectura, solo mientras la organización está verificada. Los derechos provienen del código y de la configuración predeterminada; la [Table](#tbl-admin-changeable) indica cuáles puede cambiar un administrador.

Table: Derechos predeterminados sobre los datos por rol (CRUD + Aprobar) {#tbl-crud}

| Registro | AO | Pe | IC | RRM | RAC | An | RL | RRA | RC | AP | Api | Púb |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Apicultor | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | C R U ¹⁸ | R ¹ | C R U ² | R (propio) | R ³ |
| Apiario | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | C R U ¹⁸ | R ¹ | C R U ² | R (propios) | R ³ |
| Registro del apicultor | – | – | – | – | – | – | – | C R ¹⁸ | A ¹⁹ | C R | – | – |
| Datos de acceso (credenciales) | – | – | – | – | – | – | – | C ²⁰ | – | C ²⁰ | – | – |
| Lote de miel cruda | R | R | R | C R ⁴ | R | – | – | – | R | C R U D ⁵ | R (propios) | R ³ |
| Confirmación de la entrega (código SMS o foto firmada) | R | – | – | C R | – | – | – | – | R A ¹⁹ | R | A ²¹ | – |
| Envase y sus precintos | R | R ²⁹ | R ²⁹ | C R | R ²⁹ | R | R | – | R | R | R (propios) | – |
| Muestra | R | – | – | C R | – | R A ⁶ | R A ⁶ | – | R | R | R (propias) | – |
| Informe y resultados de laboratorio | R | R | R | R | R | C R U D ⁷ | R A ⁷ | – | R | R | R (aprobados, propios) | R ⁸ |
| Registro de PCC (datos, "No aplicable") | R | C R U D ⁹ | R ⁹ | C R U D ⁹ | R A | R | R | – | R | R ¹⁰ | – | R ³ |
| Partida | R, solicitar la finalización | R, solicitar la finalización | R | R | R, confirmar la finalización (A) | R | R | – | R | C R U D ¹¹ | R ²² | R ³ |
| Envío | C R A ¹² | C R A ¹² | R ²⁹ | R ²⁹ | R | R ²⁹ | R ²⁹ | – | R | R | – | – |
| Registro de almacenamiento en el receptor (PCC 10) | R | C R U D | R | R | R A | R | R | – | R | R | – | – |
| Precio solicitado de un lote | R ²³ | – | – | – | – | – | – | – | R ²³ | R | C R ²³ | – ²⁴ |
| Consentimiento del apicultor | C R U ¹³ | R | R | R | R | R | R | C R ²⁵ | R U ²⁵ | R | R U ²⁵ | R ¹⁴ |
| Aviso informativo (texto, versión) | R | R | R | R | R | R | R | R | R | C R ²⁶ | R | R |
| Aceptación del aviso | R (propia) | R (propia) | R (propia) | R (propia) | R (propia) | R (propia) | R (propia) | R (propia) | R (propia) | R | R (propia) | – |
| Datos del responsable del tratamiento | R | R | R | R | R | R | R | R | C R U | C R U | R | R (en el aviso) |
| Revisión de la calidad del muestreo | – | – | – | – | – | – | – | – | C R ³⁰ | – | – | – |
| Solicitud de un interesado | – | – | – | – | – | – | – | – | C R U ³⁰ | – | – | – |
| Mensaje SMS | – | – | – | – | – | – | – | – | – | R ²⁷ | – | – |
| Organización (datos, documentos) | R; documentos C | R | R | R | R | R | R | R | R; documentos C | C R U, estado A ¹⁵ | – | – |
| Membresía | R | R | R | R | R | R | R | R | R | C R U ¹⁶ | – | – |
| Entrada de auditoría | – | – | – | – | – | – | – | – | R ²⁸ | R ¹⁷ | – | – |

Notas:

1. Solo se leen a través de los registros con los que trabaja la organización (páginas de PCC, informe de retiro). Un poseedor posterior de una partida lee los datos sin los nombres de apicultores y apiarios. Los miembros de una asociación leen la lista de sus apicultores miembros.
2. Los apicultores y los apiarios nunca se eliminan; solo se desactivan. Las fotos de los apiarios se pueden añadir, cambiar y eliminar.
3. En las páginas públicas de las partidas finalizadas (Historia de la miel) y en las páginas de apicultores publicadas: el nombre solo con consentimiento (si no, un código neutro), la provincia y los datos aprobados; nunca coordenadas ni datos de contacto (consulte [Lo que puede leer el público](#trust-public)).
4. Un lote se crea al registrar una entrega, en una organización verificada con la capacidad **Procesamiento**.
5. Los administradores mantienen los datos maestros del lote (por ejemplo, la organización propietaria y la asignación de PCC). Los datos de la entrega de un lote registrado con envases (fecha, envases, pesos, números de precinto) no se pueden editar, tampoco por los administradores; solo los cambia una corrección de la entrega hecha por dos administradores (consulte [Correcciones](#admin-corrections)). No se puede cambiar nada una vez que el lote está en una partida finalizada; un lote con entregas, muestras o datos de PCC en uso no se puede eliminar.
6. **A** = recibir la muestra tras comprobar el precinto, la etiqueta y la tapa, o rechazarla.
7. El analista crea, guarda, modifica y envía un informe mientras es un borrador; el responsable de laboratorio lo aprueba o lo devuelve. Un informe aprobado no se puede modificar.
8. La página **Verificar un informe de laboratorio**, por código de informe; solo informes aprobados.
9. Ingresar datos (C), modificarlos o borrarlos (U, D) y marcar "No aplicable", hasta que el registro se aprueba o se bloquea. Roles de ingreso predeterminados: Responsable de recepción de muestras para los PCC 0–1, Personal para los PCC 1–10. El inspector de campo ingresa el PCC 0 solo si un administrador asigna el PCC 0 a la asociación.
10. Los administradores modifican datos aprobados o bloqueados solo mediante una corrección solicitada por un administrador y aprobada por un segundo.
11. Crear una partida, componerla a partir de lotes, asignar sus PCC y fijar su propietario. Una partida solo se puede eliminar antes de finalizarla. Los administradores no pueden finalizar.
12. **C** = enviar una partida finalizada que su organización tiene en su poder; **A** = confirmar o rechazar un envío que recibe su organización.
13. Registrar (C) y retirar (U) un consentimiento para una página pública de apicultor; solo asociaciones.
14. La página del apicultor publicada.
15. Verificar, solicitar documentos, suspender y restablecer (consulte [Estado de la organización](#org-status)). Las organizaciones nunca se eliminan.
16. Añadir una membresía y revocarla (U); las membresías nunca se eliminan.
17. Al registro de auditoría solo se le pueden añadir entradas; nadie puede modificar ni eliminar una entrada.
18. Un responsable de registro de apicultores registra a los apicultores y sus apiarios para su propia dirección y solo lee los registros de esta. Los datos atestiguados (identidad, número de registro, teléfono) solo cambian con un nuevo registro del mismo apicultor (nueva verificación), confirmado de nuevo.
19. **A** = aceptar o rechazar una prueba registrada con una foto firmada en lugar de un código SMS: una foto de registro o una foto de entrega. Decide el Responsable de cumplimiento de la organización que la registró, nunca la persona que la registró. Para los registros de la propia plataforma decide un administrador de la plataforma.
20. Los datos de acceso del apicultor los envía por SMS el responsable de registro de apicultores (cuando el portal del apicultor está abierto); los datos de acceso de los miembros los envían los administradores de la plataforma. Nadie ve una contraseña después de enviarla.
21. La **A** del apicultor es el código SMS que lee en voz alta al responsable de recepción de muestras; confirma la entrega con sus envases, pesos y precintos.
22. Partidas que contienen miel del propio apicultor: nombre, la parte del apicultor, estado y enlace a la Historia de la miel. Nunca nombres ni precios de otros apicultores.
23. El apicultor indica un precio solicitado por cada lote entregado y puede cambiarlo (una nueva entrada; el historial se conserva) hasta que el lote esté en una partida finalizada. Lo leen el AO y el RC de la organización propietaria del lote y los administradores de la plataforma; la asociación, solo si el apicultor lo ha aceptado. El personal, los laboratorios y los poseedores posteriores, nunca.
24. Las páginas públicas y el informe de retiro no muestran ningún precio.
25. El responsable de registro de apicultores registra los consentimientos dados en el registro; el apicultor otorga o retira un consentimiento opcional en el portal; el Responsable de cumplimiento registra una retirada recibida en papel o por teléfono. Una retirada surte efecto de inmediato.
26. Los textos de los avisos tienen versiones y nunca se editan: publicar una versión nueva retira la anterior.
27. Solo la bandeja de salida del proveedor de prueba (desarrollo y demostraciones); un proveedor real no guarda códigos ni contraseñas.
28. Las entradas de auditoría de la propia organización del Responsable de cumplimiento.
29. Solo en el informe de retiro: la página **Envíos y existencias** requiere el derecho de lectura de envíos.
30. La sección propia del Responsable de cumplimiento, **Cumplimiento** (consulte el [capítulo 13](#role-co)).

**Quién descarga los archivos de prueba.** Los formularios de entrega firmados, las fotos de registro, los documentos de consentimiento y los documentos de la organización solo los descargan el rol que los registra (responsable de recepción de muestras, responsable de registro de apicultores o, para los consentimientos, el administrador de la organización de la asociación), el Responsable de cumplimiento y el administrador de la organización, y los administradores de la plataforma. Otro rol de la organización recibe "No puede abrir esta página" (403); otra organización recibe "Esta página no existe" (404). Cada descarga queda escrita en el registro de auditoría.

**Qué se puede eliminar.** Los miembros solo borran sus propios datos antes de que se aprueben (datos de PCC, resultados de laboratorio en borrador, datos de almacenamiento). Los administradores pueden eliminar lotes y partidas antes de la finalización, fotos de apiarios, datos de referencia y cuentas de usuario (nunca la propia). Los apicultores, apiarios, organizaciones, documentos, membresías, consentimientos, registros de apicultores, envases, eventos de precinto, muestras, informes de laboratorio, envíos, entradas de precio, mensajes SMS, avisos, aceptaciones y entradas de auditoría nunca se eliminan.

## Matriz de permisos {#permission-matrix}

La [Table](#tbl-permissions) muestra los derechos predeterminados por **actividad en la plataforma** (una función como "registrar una entrega"), tal como están configurados hoy. Complementa los derechos sobre los datos de la [Table](#tbl-crud) y usa los mismos roles en el mismo orden. Leyenda: **R** leer, **C** crear o ingresar, **A** aprobar (o recibir, o aceptar pruebas), **F** finalizar (solicitar o confirmar), **–** sin derecho. La columna "Capacidad necesaria" indica qué capacidad necesita la organización.

Table: Derechos predeterminados por actividad en la plataforma y rol {#tbl-permissions}

| Actividad en la plataforma | Capacidad necesaria | AO | Pe | IC | RRM | RAC | An | RL | RRA | RC | AP |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Registrar un apicultor y sus apiarios | Registro de apicultores | – | – | – | – | – | – | – | C R | – | C R |
| Enviar por SMS los datos de acceso de un apicultor | Registro de apicultores | – | – | – | – | – | – | – | C | – | C |
| Registrar una entrega con envases, enviar el código SMS | Procesamiento | R | – | – | C R | – | – | – | – | R | R |
| Tomar muestras (una de cada envase) | Procesamiento (propietario del lote) | R | – | – | C R | – | R | R | – | R | R |
| Aceptar o rechazar una foto firmada (registro, entrega) | organización propia | – | – | – | – | – | – | – | – | A | A ¹ |
| Recibir una muestra (precinto, etiqueta, tapa) | Laboratorio | – | – | – | – | – | A | A | – | R | R |
| Ingresar resultados de laboratorio, enviar el informe | Laboratorio | R | R | R | R | R | C R | R | – | R | R |
| Aprobar o devolver un informe de laboratorio | Laboratorio | R | R | R | R | R | R | A R | – | R | R |
| Control de recepción PCC 0 (lote) | Procesamiento (propietario del lote) | R | R | R | C R | A R | R | R | – | R | R |
| Datos de laboratorio PCC 1 (lote) | se toman del informe de laboratorio aprobado | R | R | R | R | R | R | R | – | R | R |
| PCC 2–3 (lote) | Procesamiento (propietario del lote) | R | C R | R | R | A R | R | R | – | R | R |
| Crear una partida, componerla, asignar los PCC | – | – | – | – | – | – | – | – | – | – | C |
| PCC 4–6 (partida) | Procesamiento (propietario de la partida) | R | C R | R | R | A R | R | R | – | R | R |
| PCC 7–9 (partida) | Envasado (propietario de la partida) | R | C R | R | R | A R | R | R | – | R | R |
| PCC 10 almacenamiento en la planta (partida) | Comercio mayorista, Comercio minorista o Envasado (propietario de la partida) | R | C R | R | R | A R | R | R | – | R | R |
| Solicitar la finalización | Procesamiento o Envasado (propietario), verificada | F | F | – | – | – | – | – | – | – | – |
| Confirmar la finalización (sellar) | Procesamiento o Envasado (propietario), verificada | – | – | – | – | F | – | – | – | – | – |
| Crear un envío | Procesamiento, Envasado, Comercio mayorista o Comercio minorista (poseedor), verificada | C R | C R | – | – | R | – | – | – | R | R |
| Confirmar o rechazar un envío recibido | Procesamiento, Envasado, Comercio mayorista o Comercio minorista (receptor), verificada | A R | A R | – | – | R | – | – | – | R | R |
| Registro de almacenamiento PCC 10 en el receptor | Comercio mayorista, Comercio minorista o Envasado | R | C R | R | R | A R | R | R | – | R | R |
| Informe de retiro (ámbito propio) | – | R | R | R | R | R | R | R | – | R | R (todo) |
| Leer el precio solicitado de los lotes propios | Procesamiento (propietario del lote) | R | – | – | – | – | – | – | – | R | R |
| Registrar o retirar el consentimiento de un apicultor (página de apicultor) | Asociación | C R | – | – | – | – | – | – | – | – | R |
| Cumplimiento: revisiones, solicitudes de los interesados, controles de aprobación | organización propia | – | – | – | – | – | – | – | – | C R | – |
| Mantener los datos del responsable del tratamiento | organización propia | R | – | – | – | – | – | – | – | C R | C R |
| Leer las entradas de auditoría de la organización | organización propia | – | – | – | – | – | – | – | – | R | R (todo) |
| Subir documentos de la organización | organización propia | C | – | – | – | – | – | – | – | C | C |
| Publicar los textos de los avisos | – | – | – | – | – | – | – | – | – | – | C |
| Verificar organizaciones, gestionar membresías y capacidades | – | – | – | – | – | – | – | – | – | – | C A ² |
| Verificar números de registro de apicultores | – | – | – | – | – | – | – | – | – | – | A |
| Cambiar permisos, PCC, límites, traducciones | – | – | – | – | – | – | – | – | – | – | C |
| Solicitar o decidir una corrección (datos de PCC, datos de la entrega) | – | – | – | – | – | – | – | – | – | – | C A (dos administradores distintos) |
| Registro de auditoría, informe de conservación de datos | – | – | – | – | – | – | – | – | – | – | R |

1. Para los registros hechos por administradores de la plataforma.
2. Si la plataforma está configurada para ello, las asociaciones las verifica en su lugar la asociación central.

Notas:

- **Leer los PCC (R)**: cualquier rol puede abrir los PCC de los lotes y las partidas en los que participa su organización: como propietaria u organización responsable, como organización que tomó una muestra (en el caso de los lotes), como poseedora posterior de la partida o como autoridad pública verificada. Un poseedor lee los PCC de la partida y los de sus lotes **en modo de solo lectura**. Cualquier otra organización recibe la página "No puede abrir esta página" (403).
- El **PCC 1** se completa a partir del informe de laboratorio aprobado. Sus roles de ingreso configurados (Responsable de recepción de muestras, Personal) solo se usan para los parámetros del PCC 1 que el informe de laboratorio no cubre.
- **Las páginas comprueban el derecho de lectura, no solo el menú.** Un rol sin el derecho de lectura de una página (por ejemplo, **Envíos y existencias** o **Páginas de apicultores**) vuelve a **Mis tareas** con un mensaje que nombra los roles que pueden abrirla.
- El administrador puede asignar el **PCC 0** a la asociación del apicultor en lugar de a la planta (**Responsabilidades de PCC** → PCC 0 → **La asociación del apicultor del lote**, capacidad Asociación, rol Inspector de campo). No es la configuración predeterminada.

<!-- SHOT: admin-permissions-matrix -->
Figure: **Roles y permisos**: los derechos predeterminados del rol Personal.

### Qué puede cambiar un administrador {#admin-changeable}

La [Table](#tbl-admin-changeable) enumera todos los ajustes que puede cambiar un administrador de la plataforma en el panel. Todo lo demás es fijo: la lista de roles, la lista de capacidades y las [Reglas básicas](#fixed-rules). Algunos interruptores se fijan, en cambio, en la configuración del servidor (consulte [Ajustes en la configuración del servidor](#admin-settings)).

Table: Lo que puede cambiar un administrador de la plataforma {#tbl-admin-changeable}

| Ajuste | Dónde | Qué se puede cambiar exactamente |
|---|---|---|
| Permisos de los roles | **Roles y permisos** → **Permisos por rol** | Por rol: **Introducir**, **Aprobar** y **Leer** para cada PCC; **Introducir** y **Leer** para las entregas (recepción); **Introducir**, **Aprobar** (recibir) y **Leer** para las muestras; **Introducir**, **Aprobar** y **Leer** para los resultados de laboratorio; **Introducir** (enviar), **Aprobar** (confirmar la recepción) y **Leer** para los envíos; **Introducir** y **Leer** para las páginas de apicultores; **Leer** para el informe de retiro; **Introducir** (solicitar) y **Aprobar** (confirmar) para la finalización; **Introducir** y **Leer** para el registro de apicultores; **Aprobar** para las pruebas; **Leer** para los precios y para las entradas de auditoría de la organización. Cada derecho conserva al menos un rol. |
| Responsabilidades de PCC | **Responsabilidades de PCC** | Por PCC: la organización responsable (la propietaria del lote o de la partida o, en el caso de los lotes, la asociación del apicultor), las capacidades que necesita, los roles de ingreso, los roles de aprobación y si se requiere aprobación. |
| Capacidades | Página de la organización; **Capacidades de la organización** | Añadir o quitar capacidades de una organización (siempre queda al menos una); número de acreditación del laboratorio y su validez. |
| Exención de la doble verificación | Página de la organización | Por organización: permite a sus miembros aprobar sus propios registros de PCC y de almacenamiento y confirmar su propia solicitud de finalización; siempre queda marcado en el registro de auditoría. Nunca para los resultados de laboratorio ni para las pruebas. |
| Estado de la organización | Página de la organización | Verificar, solicitar documentos, suspender, restablecer. |
| Datos del responsable del tratamiento | Página de la organización | Razón social, contacto y dirección de la organización como responsable del tratamiento (también los mantiene su Responsable de cumplimiento). |
| Membresías y usuarios | Página de la organización; **Miembros**; **Gestión de Usuarios** | Añadir y revocar membresías; crear usuarios con datos de acceso por SMS o con una contraseña inicial; enviar nuevos datos de acceso; desbloquear y eliminar cuentas. |
| Avisos y textos de consentimiento | **Avisos y textos de consentimiento** | Publicar una versión nueva de un aviso o de un texto de consentimiento, con la marca de borrador hasta que la oficina de protección de datos lo apruebe. |
| Puntos de control, parámetros, límites | **Puntos Críticos de Control** | Parámetros, tipos de datos, preguntas obligatorias, límites, "Límites verificados". |
| Datos maestros y de referencia | Menús de administración | Apicultores, apiarios, números de registro, lotes, partidas, tipos de miel, periodos, traducciones. |

## Reglas básicas {#fixed-rules}

Estas reglas forman parte de la plataforma; ningún ajuste las desactiva:

- **Solo las organizaciones verificadas ingresan datos.** Una organización que se verifica por primera vez necesita un Responsable de cumplimiento.
- **Cuatro ojos en los registros de PCC**: nadie aprueba un registro de PCC en el que haya registrado datos o que haya marcado como "No aplicable". Excepción: un administrador de la plataforma puede conceder a una organización la **Exención de la doble verificación (cuatro ojos)**; entonces sus miembros pueden aprobar sus propios registros de PCC, y cada una de esas aprobaciones queda marcada en el registro de auditoría.
- **Cuatro ojos en los resultados de laboratorio, siempre**: la persona que ingresó los resultados no puede aprobarlos. No hay exención.
- **Cuatro ojos en las pruebas, siempre**: una foto firmada que sustituye a un código SMS (registro o entrega) la acepta o la rechaza el Responsable de cumplimiento de la organización, nunca la persona que la registró.
- **Cuatro ojos en la finalización y en los registros de almacenamiento**: la persona que solicitó la finalización no puede confirmarla, y la persona que registró un registro de almacenamiento no puede aprobarlo (salvo que la organización tenga la exención).
- **Un código confirma exactamente aquello para lo que se envió**: el código SMS del apicultor vincula el registro o la entrega (envases, pesos, tipo de miel, muestras, cada número de precinto); cualquier cambio después del envío anula el código.
- **Los datos de la entrega no cambian**: a los envases y a los eventos de precinto solo se les pueden añadir entradas; los datos de una entrega confirmada solo se cambian mediante una corrección de dos administradores, y se informa al apicultor por SMS.
- **Una muestra de cada envase**: cada envase recibe su propio frasco de muestra precintado y su contramuestra con una tapa de seguridad; el laboratorio comprueba el precinto, la etiqueta y la tapa, y rechaza un frasco si falla cualquier comprobación.
- **Cualquier cambio anula la aprobación**: modificar datos borra la aprobación existente; los datos fuera de sus límites deben confirmarse expresamente al aprobarlos.
- **Los resultados de laboratorio siguen a la muestra**: los resultados solo se ingresan para una muestra recibida cuyas comprobaciones fueron correctas y solo después de que se apruebe el PCC 0 del lote; un informe aprobado no se puede modificar.
- **Los administradores de la plataforma tienen solo lectura** sobre los datos del flujo de trabajo y solo los corrigen mediante el proceso de corrección con dos administradores.
- **Finalizado significa bloqueado**: una partida solo se finaliza cuando todos los PCC de la partida y de sus lotes están completos; una partida finalizada y todos sus lotes ya no se pueden modificar, salvo mediante una corrección. No existe la "anulación de la finalización".
- **Envíos**: solo partidas finalizadas, solo a otra organización verificada, como máximo la cantidad que se tiene en poder.
- **Solo los administradores configuran las organizaciones**: capacidades, membresías y la exención.
- **Cada derecho conserva al menos un rol.**
- **Al registro de auditoría solo se le pueden añadir entradas**; nada de lo que contiene se modifica ni se elimina, y cada cambio en las páginas de permisos también queda registrado.
- **Los precios son del apicultor**: solo el apicultor indica el precio solicitado; nunca se muestra en las páginas públicas.
- **Una cuenta de apicultor solo ve los datos de su propio apicultor** y solo puede abrir el portal del apicultor.
- **Los nombres públicos requieren consentimiento**: las páginas públicas solo nombran a un apicultor con un consentimiento de nombre activo; si no, muestran un código neutro.
- **Las páginas de apicultores** solo son públicas con un consentimiento activo, un número de registro verificado y una asociación verificada.

## Varias membresías {#memberships}

Una **membresía** es una cuenta de usuario × una organización × un rol. Una persona puede tener varias membresías: en organizaciones distintas o con varios roles en la misma organización (por ejemplo, **Responsable de aseguramiento de la calidad** y **Administrador de la organización** en una asociación, o **Administrador de la organización** y **Responsable de cumplimiento** en una organización pequeña). Siempre se trabaja con una sola membresía a la vez, y los derechos no se combinan: tener dos roles nunca le permite aprobar datos que usted mismo ingresó.

Para cambiar:

1. Haga clic en la **etiqueta organización · rol** de la barra superior (consulte [La barra superior y su rol](#top-bar)). Una lista con el título **Sus roles** muestra todas las membresías activas; la actual aparece marcada.
2. Haga clic en otra membresía. La plataforma abre **Mi organización** con la nueva membresía.

El menú, las tareas de **Mis tareas**, los registros que puede leer y las acciones que puede realizar dependen de la membresía activa. La elección se mantiene hasta que cierre el navegador; después, vuelve a empezar con su primera membresía (por orden alfabético del nombre de la organización). Si tiene dos o más membresías, **Mi organización** también ofrece una lista **Actuar como** con un botón **Cambiar**.

La cuenta del portal de un apicultor no tiene membresía ni etiqueta; solo abre el portal del apicultor.
