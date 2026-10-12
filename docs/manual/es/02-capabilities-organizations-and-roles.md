# 2. Capacidades, organizaciones y roles {#roles}

En TOP4HoneyChains nadie actúa por su cuenta: cada acción se realiza **en nombre de una organización y con un rol**. Tres elementos determinan lo que usted puede hacer:

1. las **capacidades** de su organización: lo que es capaz de hacer en la cadena de la miel;
2. su **rol** en la organización: quién es usted dentro de ella;
3. el **estado** de su organización: solo una organización **Verificada** puede ingresar datos. Las organizaciones las verifica un administrador de la plataforma (consulte [Estado de la organización](#org-status)).

Un rol se tiene a través de una **membresía**: una cuenta de usuario × una organización × un rol (consulte [Varias membresías](#memberships)). Además, algunas reglas forman parte de la plataforma y no se pueden desactivar (consulte [Reglas básicas](#fixed-rules)).

Este capítulo distingue dos cosas:

- las **actividades de la cadena de valor**: el trabajo que las personas hacen con la miel fuera del ordenador, como entregar, tomar muestras, analizar, procesar, almacenar y enviar ([Table](#tbl-activities));
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
| **Asociación central** | Capaz de verificar asociaciones de apicultores, cuando la plataforma está configurada para ello. | ninguno |
| **Autoridad pública** | Capaz de leer todos los registros, como ministerio u organismo de inspección; no ingresa nada. | ninguno (lee todo) |
| **Explotación apícola** | Capaz de producir miel. Los apicultores no tienen cuenta; su explotación está representada por un registro de apicultor. | ninguno |

Una planta de procesamiento que además llena y almacena su propia miel necesita **Procesamiento**, **Envasado** y **Comercio mayorista** (o **Comercio minorista**), porque los PCC 7–9 de la partida requieren Envasado y el PCC 10 requiere Comercio mayorista, Comercio minorista o Envasado.

**La plataforma.** *Administrador de la plataforma* no es una capacidad que se pueda asignar a una organización: es un rol global de una cuenta de usuario, para las personas que gestionan la plataforma. Los administradores de la plataforma gestionan organizaciones, membresías, datos de referencia y ajustes y leen todos los registros, pero no ingresan datos del flujo de trabajo (consulte el [capítulo 12](#admin)). La organización propia de la plataforma tiene la capacidad reservada **Administrador de la plataforma**; no se puede asignar a ninguna otra organización ni quitar.

## Actividades de la cadena de valor {#activities}

Las actividades de la cadena de valor las realizan personas, con la miel, fuera del ordenador. La plataforma registra el resultado de cada actividad como datos. La [Table](#tbl-activities) muestra, por capacidad, qué actividades realizan sus personas y qué se registra en la plataforma.

Table: Actividades de la cadena de valor (personas) y lo que registra la plataforma (ordenador) {#tbl-activities}

| Capacidad | Actividades de la cadena de valor (personas, fuera del ordenador) | Registrado en la plataforma (en el ordenador) |
|---|---|---|
| Explotación apícola | Criar abejas, cosechar la miel, entregarla a una planta | Registros de apicultor y de apiario, que mantienen los administradores y la asociación |
| Asociación | Vincular e informar a los apicultores miembros, leerles el texto de consentimiento, recoger las firmas | Registros de consentimiento; páginas públicas de apicultores |
| Procesamiento | Recibir entregas, controlar la miel en la recepción, precintar muestras, calentar, filtrar, mezclar y decantar la miel | Entrega (lote de miel cruda), muestra, datos de los PCC 0 y 2–6 |
| Envasado | Llenar, etiquetar y envasar la miel, almacenarla en la planta | Datos de los PCC 7–10 |
| Laboratorio | Recibir y controlar las muestras precintadas, analizar la miel, firmar el informe | Recepción de la muestra, informe de laboratorio (PDF) y resultados (PCC 1) |
| Comercio mayorista, Comercio minorista | Recibir la mercancía, almacenarla, venderla o reenviarla | Confirmación del envío, registro de almacenamiento (PCC 10), nuevos envíos |
| Asociación central | Controlar las asociaciones de apicultores | Verificación de asociaciones (si está configurada) |
| Autoridad pública | Inspeccionar | Lee los registros |

## Tipos de organización {#org-types}

La plataforma no tiene una lista aparte de tipos de organización: el tipo de una organización es el conjunto de capacidades que tiene. La [Table](#tbl-org-types) enumera organizaciones típicas.

Table: Organizaciones típicas y sus capacidades {#tbl-org-types}

| Organización | Capacidades | Roles típicos de sus miembros |
|---|---|---|
| Asociación de apicultores | **Asociación** | Administrador de la organización, Inspector de campo |
| Planta de procesamiento o envasador | **Procesamiento**, **Envasado** y **Comercio mayorista** o **Comercio minorista** para el almacenamiento en la planta | Administrador de la organización, Personal, Responsable de recepción de muestras, Responsable de aseguramiento de la calidad |
| Laboratorio de análisis de alimentos (acreditado o propio) | **Laboratorio** | Administrador de la organización, Analista, Responsable de laboratorio |
| Mayorista | **Comercio mayorista** | Administrador de la organización, Personal, Responsable de aseguramiento de la calidad |
| Minorista | **Comercio minorista** | Administrador de la organización, Personal, Responsable de aseguramiento de la calidad |
| Ministerio u organismo de inspección | **Autoridad pública** | Administrador de la organización (solo lectura) |

Una asociación que gestiona su propia planta de procesamiento tiene también **Procesamiento** (y **Envasado**) y, en ese caso, también los roles de una planta.

### Estado de la organización {#org-status}

Un administrador de la plataforma verifica, suspende y restablece las organizaciones. Si la plataforma está configurada para ello, el administrador de la organización de una **Asociación central** verificada también puede verificar, suspender y restablecer asociaciones de apicultores. Para verificar hace falta al menos un documento subido; para suspender hace falta un motivo. La [Table](#tbl-org-status) enumera los estados.

Table: Estado de la organización {#tbl-org-status}

| Estado | Significado |
|---|---|
| **Pendiente** | Organización nueva, a la espera de verificación. Los miembros pueden leer y el administrador de la organización puede subir documentos. Nadie puede ingresar datos. |
| **Verificada** | Revisada por un administrador de la plataforma (o, en el caso de las asociaciones, por la asociación central si está configurado). Los miembros pueden trabajar según sus roles. |
| **Suspendida** | Verificación retirada (con un motivo). Funciona como Pendiente hasta que se restablece. |

## Roles {#role-list}

La [Table](#tbl-roles) es la lista de referencia de los roles. Todas las demás tablas de este manual usan los mismos nombres de rol en el mismo orden.

Table: Roles {#tbl-roles}

| Rol (en este manual) | Código | Persona típica | Trabajo principal |
|---|---|---|---|
| **Administrador de la organización** | OrgAdmin | Responsable de la organización | Sube documentos, solicita la finalización, envía y confirma envíos, registra consentimientos de apicultores (asociaciones), lee entregas y muestras |
| **Personal** | Staff | Personal de producción, de llenado o de comercio minorista | Ingresa los datos de los PCC 1–10, solicita la finalización, envía y confirma envíos, registra el almacenamiento |
| **Inspector de campo** | FieldInspector | Técnico de campo de la asociación | Lee registros; ingresa el PCC 0 solo si un administrador asigna ese PCC a la asociación |
| **Responsable de recepción de muestras** | SampleAcceptanceOfficer | Encargado de recepción en la planta | Registra entregas, ingresa el PCC 0, toma y envía muestras |
| **Responsable de aseguramiento de la calidad** | QualityApprover | Responsable de calidad, ingeniero de alimentos | Aprueba registros de PCC y registros de almacenamiento, confirma la finalización |
| **Analista** | Analyst | Analista de laboratorio | Recibe muestras, ingresa resultados de laboratorio, envía informes (la primera persona en el laboratorio) |
| **Responsable de laboratorio** | LabManager | Director del laboratorio | Recibe muestras, aprueba o devuelve informes de laboratorio (la segunda persona en el laboratorio) |
| **Administrador de la plataforma** | (rol global) | Operador de la plataforma | Administración (capítulo 12); lee todos los datos del flujo de trabajo, pero no ingresa ni aprueba ninguno |

Un administrador de la plataforma no forma parte del flujo de trabajo de una organización de negocio: puede leer todos los registros, pero **no puede ingresar ni aprobar datos del flujo de trabajo** (entregas, datos de PCC, resultados de laboratorio, envíos, consentimientos). Cuando un dato es incorrecto después de su aprobación, un administrador solicita una *corrección*, que aprueba un segundo administrador (consulte [Correcciones](#admin-corrections)). Los administradores sí mantienen los datos maestros: apicultores, apiarios, lotes, partidas y su composición (consulte el [capítulo 12](#admin)).

## Derechos sobre los datos (CRUD) {#crud}

La [Table](#tbl-crud) muestra los derechos predeterminados de cada rol sobre cada tipo de registro, según el modelo CRUD: **C** crear (*create*), **R** leer (*read*), **U** modificar (*update*), **D** eliminar (*delete*) y **A** aprobar (un derecho aparte). "–" significa sin derecho. Los derechos de un miembro se aplican solo a los registros del trabajo de su propia organización (sus propios lotes y partidas, las partidas que tiene en su poder, los apicultores de su asociación) y, salvo la lectura, solo mientras la organización está verificada. Los derechos provienen del código y de la configuración predeterminada; la [Table](#tbl-admin-changeable) indica cuáles puede cambiar un administrador.

Table: Derechos predeterminados sobre los datos por rol (CRUD + Aprobar) {#tbl-crud}

| Registro | Administrador de la organización | Personal | Inspector de campo | Responsable de recepción de muestras | Responsable de aseguramiento de la calidad | Analista | Responsable de laboratorio | Administrador de la plataforma | Visitante público |
|---|---|---|---|---|---|---|---|---|---|
| Apicultor | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | C R U ² | R ³ |
| Apiario | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | C R U ² | R ³ |
| Lote de miel cruda | R | R | R | C R ⁴ | R | – | – | C R U D ⁵ | R ³ |
| Muestra | R | – | – | C R | – | R A ⁶ | R A ⁶ | R | – |
| Informe y resultados de laboratorio | R | R | R | R | R | C R U D ⁷ | R A ⁷ | R | R ⁸ |
| Registro de PCC (datos, "No aplicable") | R | C R U D ⁹ | R ⁹ | C R U D ⁹ | R A | R | R | R ¹⁰ | R ³ |
| Partida | R, solicitar la finalización | R, solicitar la finalización | R | R | R, confirmar la finalización (A) | R | R | C R U D ¹¹ | R ³ |
| Envío | C R A ¹² | C R A ¹² | R | R | R | R | R | R | – |
| Registro de almacenamiento en el receptor (PCC 10) | R | C R U D | R | R | R A | R | R | R | – |
| Consentimiento del apicultor | C R U ¹³ | R | R | R | R | R | R | R | R ¹⁴ |
| Organización (datos, documentos) | R; documentos C | R | R | R | R | R | R | C R U, estado A ¹⁵ | – |
| Membresía | R | R | R | R | R | R | R | C R U ¹⁶ | – |
| Entrada de auditoría | – | – | – | – | – | – | – | R ¹⁷ | – |

Notas:

1. Solo se leen a través de los registros con los que trabaja la organización (páginas de PCC, informe de retiro). Un poseedor posterior de una partida lee los datos sin los nombres de apicultores y apiarios. Los miembros de una asociación leen la lista de sus apicultores miembros.
2. Los apicultores y los apiarios nunca se eliminan; solo se desactivan. Las fotos de los apiarios se pueden añadir, cambiar y eliminar.
3. En las páginas públicas de las partidas finalizadas (Historia de la miel) y en las páginas de apicultores publicadas: nombres, provincia y datos aprobados; nunca coordenadas ni datos de contacto (consulte [Lo que puede leer el público](#trust-public)).
4. Un lote se crea al registrar una entrega, en una organización verificada con la capacidad **Procesamiento**.
5. Los administradores mantienen los datos maestros del lote (por ejemplo, la organización propietaria y la asignación de PCC). No se puede cambiar nada una vez que el lote está en una partida finalizada; un lote con entregas, muestras o datos de PCC en uso no se puede eliminar.
6. **A** = recibir la muestra con el precinto intacto, o rechazarla.
7. El analista crea, guarda, modifica y envía un informe mientras es un borrador; el responsable de laboratorio lo aprueba o lo devuelve. Un informe aprobado no se puede modificar.
8. La página **Verificar un informe de laboratorio**, por código de informe.
9. Ingresar datos (C), modificarlos o borrarlos (U, D) y marcar "No aplicable", hasta que el registro se aprueba o se bloquea. Roles de ingreso predeterminados: Responsable de recepción de muestras para los PCC 0–1, Personal para los PCC 1–10. El inspector de campo ingresa el PCC 0 solo si un administrador asigna el PCC 0 a la asociación.
10. Los administradores modifican datos aprobados o bloqueados solo mediante una corrección solicitada por un administrador y aprobada por un segundo.
11. Crear una partida, componerla a partir de lotes, asignar sus PCC y fijar su propietario. Una partida solo se puede eliminar antes de finalizarla. Los administradores no pueden finalizar.
12. **C** = enviar una partida finalizada que su organización tiene en su poder; **A** = confirmar o rechazar un envío que recibe su organización.
13. Registrar (C) y retirar (U) un consentimiento; solo asociaciones.
14. La página del apicultor publicada.
15. Verificar, solicitar documentos, suspender y restablecer (consulte [Estado de la organización](#org-status)). Las organizaciones nunca se eliminan.
16. Añadir una membresía y revocarla (U); las membresías nunca se eliminan.
17. Al registro de auditoría solo se le pueden añadir entradas; nadie puede modificar ni eliminar una entrada.

**Qué se puede eliminar.** Los miembros solo borran sus propios datos antes de que se aprueben (datos de PCC, resultados de laboratorio en borrador, datos de almacenamiento). Los administradores pueden eliminar lotes y partidas antes de la finalización, fotos de apiarios, datos de referencia y cuentas de usuario (nunca la propia). Los apicultores, apiarios, organizaciones, documentos, membresías, consentimientos, muestras, informes de laboratorio, envíos y entradas de auditoría nunca se eliminan.

## Matriz de permisos {#permission-matrix}

La [Table](#tbl-permissions) muestra los derechos predeterminados por **actividad en la plataforma** (una función como "registrar una entrega"), tal como están configurados hoy. Complementa los derechos sobre los datos de la [Table](#tbl-crud) y usa los mismos roles en el mismo orden. Leyenda: **R** leer, **C** crear o ingresar, **A** aprobar (o recibir), **F** finalizar (solicitar o confirmar), **–** sin derecho. La columna "Capacidad necesaria" indica qué capacidad necesita la organización.

Table: Derechos predeterminados por actividad en la plataforma y rol {#tbl-permissions}

| Actividad en la plataforma | Capacidad necesaria | Administrador de la organización | Personal | Inspector de campo | Responsable de recepción de muestras | Responsable de aseguramiento de la calidad | Analista | Responsable de laboratorio | Administrador de la plataforma |
|---|---|---|---|---|---|---|---|---|---|
| Registrar una entrega (recepción) | Procesamiento | R | – | – | C R | – | – | – | R |
| Tomar y enviar una muestra | Procesamiento (propietario del lote) | R | – | – | C R | – | R | R | R |
| Recibir una muestra (control del precinto) | Laboratorio | – | – | – | – | – | A | A | R |
| Ingresar resultados de laboratorio, enviar el informe | Laboratorio | R | R | R | R | R | C R | R | R |
| Aprobar o devolver un informe de laboratorio | Laboratorio | R | R | R | R | R | R | A R | R |
| Control de recepción PCC 0 (lote) | Procesamiento (propietario del lote) | R | R | R | C R | A R | R | R | R |
| Datos de laboratorio PCC 1 (lote) | se toman del informe de laboratorio aprobado | R | R | R | R | R | R | R | R |
| PCC 2–3 (lote) | Procesamiento (propietario del lote) | R | C R | R | R | A R | R | R | R |
| Crear una partida, componerla, asignar los PCC | – | – | – | – | – | – | – | – | C |
| PCC 4–6 (partida) | Procesamiento (propietario de la partida) | R | C R | R | R | A R | R | R | R |
| PCC 7–9 (partida) | Envasado (propietario de la partida) | R | C R | R | R | A R | R | R | R |
| PCC 10 almacenamiento en la planta (partida) | Comercio mayorista, Comercio minorista o Envasado (propietario de la partida) | R | C R | R | R | A R | R | R | R |
| Solicitar la finalización | propietario, verificada | F | F | – | – | – | – | – | – |
| Confirmar la finalización (sellar) | propietario, verificada | – | – | – | – | F | – | – | – |
| Crear un envío | poseedor, verificada | C R | C R | – | – | R | – | – | R |
| Confirmar o rechazar un envío recibido | receptor, verificada | A R | A R | – | – | R | – | – | R |
| Registro de almacenamiento PCC 10 en el receptor | Comercio mayorista, Comercio minorista o Envasado | R | C R | R | R | A R | R | R | R |
| Informe de retiro (ámbito propio) | – | R | R | R | R | R | R | R | R (todo) |
| Registrar o retirar el consentimiento de un apicultor | Asociación | C R | – | – | – | – | – | – | R |
| Subir documentos de la organización | organización propia | C | – | – | – | – | – | – | C |
| Verificar organizaciones, gestionar membresías y capacidades | – | – | – | – | – | – | – | – | C A |
| Verificar números de registro de apicultores | – | – | – | – | – | – | – | – | A |
| Cambiar permisos, PCC, límites, traducciones | – | – | – | – | – | – | – | – | C |
| Solicitar o decidir una corrección | – | – | – | – | – | – | – | – | C A (dos administradores distintos) |
| Registro de auditoría | – | – | – | – | – | – | – | – | R |

Notas:

- **Leer los PCC (R)**: cualquier rol puede abrir los PCC de los lotes y las partidas en los que participa su organización: como propietaria u organización responsable, como organización que tomó una muestra (en el caso de los lotes), como poseedora posterior de la partida o como autoridad pública verificada. Un poseedor lee los PCC de la partida y los de sus lotes **en modo de solo lectura**. Cualquier otra organización recibe la página "No puede abrir esta página" (403).
- El **PCC 1** se completa a partir del informe de laboratorio aprobado. Sus roles de ingreso configurados (Responsable de recepción de muestras, Personal) solo se usan para los parámetros del PCC 1 que el informe de laboratorio no cubre.
- **Los envíos y la finalización** comprueban el rol y el estado de la organización, no una capacidad: cualquier propietario verificado de una partida finalizada puede enviarla.
- El administrador puede asignar el **PCC 0** a la asociación del apicultor en lugar de a la planta (**Responsabilidades de PCC** → PCC 0 → **La asociación del apicultor del lote**, capacidad Asociación, rol Inspector de campo). No es la configuración predeterminada.

<!-- SHOT: admin-permissions-matrix -->
Figure: **Roles y permisos**: los derechos predeterminados del rol Personal.

### Qué puede cambiar un administrador {#admin-changeable}

La [Table](#tbl-admin-changeable) enumera todos los ajustes que puede cambiar un administrador de la plataforma. Todo lo demás es fijo: la lista de roles, la lista de capacidades y las [Reglas básicas](#fixed-rules).

Table: Lo que puede cambiar un administrador de la plataforma {#tbl-admin-changeable}

| Ajuste | Dónde | Qué se puede cambiar exactamente |
|---|---|---|
| Permisos de los roles | **Roles y permisos** → **Permisos por rol** | Por rol: **Introducir**, **Aprobar** y **Leer** para cada PCC; **Introducir** y **Leer** para las entregas (recepción); **Introducir**, **Aprobar** (recibir) y **Leer** para las muestras; **Introducir**, **Aprobar** y **Leer** para los resultados de laboratorio; **Introducir** (enviar), **Aprobar** (confirmar la recepción) y **Leer** para los envíos; **Introducir** y **Leer** para las páginas de apicultores; **Leer** para el informe de retiro; **Introducir** (solicitar) y **Aprobar** (confirmar) para la finalización. Cada derecho conserva al menos un rol. |
| Responsabilidades de PCC | **Responsabilidades de PCC** | Por PCC: la organización responsable (la propietaria del lote o de la partida o, en el caso de los lotes, la asociación del apicultor), las capacidades que necesita, los roles de ingreso, los roles de aprobación y si se requiere aprobación. |
| Capacidades | Página de la organización; **Capacidades de la organización** | Añadir o quitar capacidades de una organización (siempre queda al menos una); número de acreditación del laboratorio y su validez. |
| Exención de la doble verificación | Página de la organización | Por organización: permite a sus miembros aprobar sus propios registros de PCC y de almacenamiento y confirmar su propia solicitud de finalización; siempre queda marcado en el registro de auditoría. Nunca para los resultados de laboratorio. |
| Estado de la organización | Página de la organización | Verificar, solicitar documentos, suspender, restablecer. |
| Membresías y usuarios | Página de la organización; **Miembros**; **Gestión de Usuarios** | Añadir y revocar membresías; crear usuarios, establecer contraseñas, desbloquear, eliminar cuentas. |
| Puntos de control, parámetros, límites | **Puntos Críticos de Control** | Parámetros, tipos de datos, preguntas obligatorias, límites, "Límites verificados". |
| Datos maestros y de referencia | Menús de administración | Apicultores, apiarios, números de registro, lotes, partidas, tipos de miel, periodos, traducciones. |

## Reglas básicas {#fixed-rules}

Estas reglas forman parte de la plataforma; ningún ajuste las desactiva:

- **Solo las organizaciones verificadas ingresan datos.**
- **Cuatro ojos en los registros de PCC**: nadie aprueba un registro de PCC en el que haya registrado datos o que haya marcado como "No aplicable". Excepción: un administrador de la plataforma puede conceder a una organización la **Exención de la doble verificación (cuatro ojos)**; entonces sus miembros pueden aprobar sus propios registros de PCC, y cada una de esas aprobaciones queda marcada en el registro de auditoría.
- **Cuatro ojos en los resultados de laboratorio, siempre**: la persona que ingresó los resultados no puede aprobarlos. No hay exención.
- **Cuatro ojos en la finalización y en los registros de almacenamiento**: la persona que solicitó la finalización no puede confirmarla, y la persona que registró un registro de almacenamiento no puede aprobarlo (salvo que la organización tenga la exención).
- **Cualquier cambio anula la aprobación**: modificar datos borra la aprobación existente; los datos fuera de sus límites deben confirmarse expresamente al aprobarlos.
- **Los resultados de laboratorio siguen a la muestra**: los resultados solo se ingresan para una muestra recibida con el precinto intacto y solo después de que se apruebe el PCC 0 del lote; un informe aprobado no se puede modificar.
- **Los administradores de la plataforma tienen solo lectura** sobre los datos del flujo de trabajo y solo los corrigen mediante el proceso de corrección con dos administradores.
- **Finalizado significa bloqueado**: una partida solo se finaliza cuando todos los PCC de la partida y de sus lotes están completos; una partida finalizada y todos sus lotes ya no se pueden modificar, salvo mediante una corrección. No existe la "anulación de la finalización".
- **Envíos**: solo partidas finalizadas, solo a otra organización verificada, como máximo la cantidad que se tiene en poder.
- **Solo los administradores configuran las organizaciones**: capacidades, membresías y la exención.
- **Cada derecho conserva al menos un rol.**
- **Al registro de auditoría solo se le pueden añadir entradas**; nada de lo que contiene se modifica ni se elimina, y cada cambio en las páginas de permisos también queda registrado.
- **Las páginas de apicultores** solo son públicas con un consentimiento activo, un número de registro verificado y una asociación verificada.

## Varias membresías {#memberships}

Una **membresía** es una cuenta de usuario × una organización × un rol. Una persona puede tener varias membresías: en organizaciones distintas o con varios roles en la misma organización (por ejemplo, **Responsable de aseguramiento de la calidad** y **Administrador de la organización** en una asociación). Siempre se trabaja con una sola membresía a la vez, y los derechos no se combinan: tener dos roles nunca le permite aprobar datos que usted mismo ingresó.

Para cambiar:

1. Haga clic en la **etiqueta organización · rol** de la barra superior (consulte [La barra superior y su rol](#top-bar)). Una lista con el título **Sus roles** muestra todas las membresías activas; la actual aparece marcada.
2. Haga clic en otra membresía. La plataforma abre **Mi organización** con la nueva membresía.

El menú, las tareas de **Mis tareas**, los registros que puede leer y las acciones que puede realizar dependen de la membresía activa. La elección se mantiene hasta que cierre el navegador; después, vuelve a empezar con su primera membresía (por orden alfabético del nombre de la organización). Si tiene dos o más membresías, **Mi organización** también ofrece una lista **Actuar como** con un botón **Cambiar**.
