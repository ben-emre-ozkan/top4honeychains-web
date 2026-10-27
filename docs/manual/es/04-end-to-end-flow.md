# 4. El flujo completo {#flow}

Este capítulo sigue una partida de miel a través de la plataforma: quién realiza cada paso y qué debe estar terminado antes de que pueda empezar el siguiente ([Figure](#fig-flow-overview)). Los capítulos de cada rol (5–14) explican cada pantalla en detalle.

<!-- SHOT: flow-overview -->
Figure: El **Flujo del proceso** de una partida finalizada: cada etapa con su estado y su organización responsable.

## Los puntos de control (PCC) {#ccp-list}

Un **punto crítico de control (PCC)** es un paso en el que se mide o se comprueba algo. Cada PCC tiene parámetros (por ejemplo, temperatura o humedad), límites y un estado. Los PCC 0–3 pertenecen al **lote de miel cruda**; los PCC 4–10, a la **partida** ([Table](#tbl-ccps)).

Table: Los puntos críticos de control (PCC) y sus roles predeterminados {#tbl-ccps}

| PCC | Pertenece a | Qué se controla | Lo ingresa (predeterminado) | Lo aprueba (predeterminado) |
|---|---|---|---|---|
| PCC 0 | Lote | Auditoría primaria en la recepción, **antes** del análisis de laboratorio | Responsable de recepción de muestras de la planta | Responsable de aseguramiento de la calidad de la planta |
| PCC 1 | Lote | Muestreo y resultados de laboratorio | Se toma del informe de laboratorio aprobado | Responsable de laboratorio (o el Responsable de aseguramiento de la calidad de la planta, ver más abajo) |
| PCC 2 | Lote | Monitoreo de la temperatura (calentamiento) | Personal de la planta | Responsable de aseguramiento de la calidad |
| PCC 3 | Lote | Control del estado del filtro | Personal | Responsable de aseguramiento de la calidad |
| PCC 4 | Partida | Control de la homogeneidad (mezcla) | Personal del propietario de la partida | Responsable de aseguramiento de la calidad |
| PCC 5 | Partida | Control de tiempo y temperatura (pasteurización) | Personal | Responsable de aseguramiento de la calidad |
| PCC 6 | Partida | Eliminación de espuma y sedimentos | Personal | Responsable de aseguramiento de la calidad |
| PCC 7 | Partida | Control de la cantidad (llenado) | Personal (Envasado) | Responsable de aseguramiento de la calidad |
| PCC 8 | Partida | Verificación de la exactitud de la etiqueta | Personal (Envasado) | Responsable de aseguramiento de la calidad |
| PCC 9 | Partida | Control de la integridad del envase | Personal (Envasado) | Responsable de aseguramiento de la calidad |
| PCC 10 | Partida | Monitoreo de temperatura y humedad en el almacenamiento | Personal (Comercio mayorista, Comercio minorista o Envasado) | Responsable de aseguramiento de la calidad |

El PCC 10 aparece dos veces en la vida de una partida: primero **en la partida, en la planta** (debe aprobarse antes de que la partida pueda sellarse) y después como **registro de almacenamiento en cada receptor** tras un envío.

Un registro de PCC pasa por estos estados: **Asignado** (aún sin datos) → **En curso** (algunos datos) → **Enviado** (todos los datos obligatorios ingresados; a la espera de aprobación) → **Completado** con una aprobación. Un PCC que no corresponde puede marcarse como **No aplicable** con un motivo; aun así debe aprobarse. Cambiar cualquier dato anula la aprobación existente.

## Paso a paso {#flow-steps}

La [Table](#tbl-flow-steps) enumera cada paso, quién lo realiza y qué bloquea.

Table: El flujo completo, paso a paso {#tbl-flow-steps}

| # | Paso | Quién | Dónde | Bloquea el paso siguiente hasta que… |
|---|---|---|---|---|
| 1 | Se registran el apicultor y su primer apiario: número de registro, identidad, aviso y consentimientos, confirmados con un código SMS (o una foto firmada) | Responsable de registro de apicultores (dirección distrital) o administrador de la plataforma | **Registro de apicultores** | Un apicultor confirmado con una foto firmada solo se encuentra en la recepción después de que el Responsable de cumplimiento acepte la foto |
| 2 | Se verifica el número de registro y se vincula la asociación | Administrador de la plataforma | **Registro de apicultores y asociaciones** | (Necesario para la página pública del apicultor, no para las entregas) |
| 3 | Se registra la entrega: cada envase con su peso y su precinto, un frasco de muestra y un frasco de contramuestra de cada envase; el apicultor confirma con el código SMS; se crea un lote de miel cruda | Responsable de recepción de muestras (planta) | **Entregas (recepción)** → **Registrar una entrega** | – (los PCC 0–3 se asignan al lote automáticamente; los frascos van al laboratorio elegido) |
| 4 | Se imprimen las etiquetas y se pegan en los envases y los frascos | Responsable de recepción de muestras | Página de la entrega → **Etiquetas** | – |
| 5 | Se ingresa el PCC 0 | Responsable de recepción de muestras | **Mis tareas** → PCC 0 | Queda enviado |
| 6 | Se aprueba el PCC 0 | Responsable de aseguramiento de la calidad (otra persona) | **Mis tareas** → PCC 0 | **Los resultados de laboratorio solo pueden ingresarse después de aprobar el PCC 0** |
| 7 | Se recibe cada frasco: se comprueban el número de precinto, la etiqueta y la tapa | Analista o Responsable de laboratorio | **Mis tareas** → **Muestras por recibir** | Una comprobación fallida rechaza el frasco; la planta toma un frasco nuevo del mismo envase |
| 8 | Se prepara la muestra compuesta a partir de los frascos recibidos (análisis compuesto) | Analista o Responsable de laboratorio | **Muestras y laboratorio** → **Hacer la muestra compuesta** | El frasco de cada envase debe haberse recibido en buen estado |
| 9 | Se ingresan los resultados de laboratorio, se sube el PDF y se envía el informe | Analista | **Mis tareas** → **Resultados de laboratorio por ingresar** | Están todos los valores obligatorios, el PDF, el número del informe y la fecha |
| 10 | Se aprueba (o se devuelve) el informe de laboratorio | Responsable de laboratorio (otra persona) | **Mis tareas** → **Informes de laboratorio por aprobar** | La aprobación escribe los resultados en el PCC 1 del lote |
| 11 | Se ingresan y aprueban los PCC 2–3 | Personal y, después, Responsable de aseguramiento de la calidad | **Mis tareas** | Un lote solo puede entrar en una partida cuando **todos** sus PCC están completados o son no aplicables, **y están aprobados** |
| 12 | Se crea la partida, se agregan los lotes, se define el propietario y se asignan los PCC 4–10 | **Administrador de la plataforma** (por ahora) | **Producciones Finales de Miel** | La partida tiene lotes y sus PCC |
| 13 | Se ingresan y aprueban los PCC 4–10, incluido el almacenamiento del PCC 10 en la planta | Personal y, después, Responsable de aseguramiento de la calidad | **Mis tareas** | Todos los PCC de la partida deben estar aprobados |
| 14 | Se solicita la finalización | Personal o Administrador de la organización del propietario | **Mi organización** → partida → **Solicitar la finalización** | El botón está desactivado mientras falte algo (también una foto de entrega que el Responsable de cumplimiento no ha aceptado) |
| 15 | Se confirma la finalización: la partida queda sellada | Responsable de aseguramiento de la calidad (otra persona) | **Mis tareas** → **Solicitudes de finalización** | – La Historia de la miel y el código QR se publican; la partida y sus lotes quedan bloqueados; el precio del apicultor queda bloqueado |
| 16 | Se crea el envío | Personal o Administrador de la organización del poseedor | **Envíos y existencias** → **Enviar** | El receptor confirma |
| 17 | Se confirma (o se rechaza) la recepción | Personal o Administrador de la organización del receptor | **Mis tareas** → **Envíos por confirmar** | La custodia se transfiere solo con la confirmación |
| 18 | Se registra y aprueba el almacenamiento (PCC 10) en el receptor | Personal del receptor y, después, su Responsable de aseguramiento de la calidad | **Envíos y existencias** → **Almacenamiento** | – |
| 19 | El receptor lee los registros del productor | Receptor (cualquier rol con derecho de lectura de PCC) | Páginas de PCC, solo lectura | – |
| 20 | El consumidor escanea el código QR | Cualquier visitante público (sin cuenta) | Historia de la miel | – |

Paralelamente al flujo, el apicultor sigue sus entregas, muestras y partidas en el portal del apicultor e indica su precio solicitado (consulte el [capítulo 14](#portal)), y el Responsable de cumplimiento de cada organización revisa las fotos firmadas, las muestras rechazadas y los precintos rotos (consulte el [capítulo 13](#role-co)).

### Registro con un código SMS o una foto firmada {#flow-registration}

Un apicultor se registra una sola vez, por el responsable de registro de apicultores de la dirección distrital de agricultura (o por un administrador de la plataforma). El responsable comprueba el número de registro de la explotación (`TR`, el código de matrícula de la provincia y 10 dígitos) con la provincia de la dirección, registra al apicultor y su primer apiario, deja que el apicultor lea el aviso informativo y registra sus elecciones: el consentimiento opcional para mostrar su nombre en las páginas públicas y el consentimiento opcional para recibir avisos por SMS. El apicultor lo confirma todo con un **código de 6 dígitos** que le llega por SMS y que lee en voz alta; el código está vinculado exactamente a este contenido.

Un apicultor sin teléfono móvil firma en su lugar el formulario impreso; el responsable sube una foto con el motivo. Un registro así espera al **Responsable de cumplimiento** de la dirección; hasta que se acepte la foto, el apicultor no puede entregar ("No hay ningún apicultor activo con este número." en la recepción), y una foto rechazada retira los consentimientos. Un apicultor sin teléfono móvil no recibe ninguna cuenta del portal. Consulte el [capítulo 12](#role-ro).

### Envases, precintos y frascos de muestra {#flow-containers}

La miel llega en **envases** (latas, bidones, barriles). En la recepción, el responsable de recepción de muestras registra una fila por envase:

1. **El precinto a la llegada**: un envase precintado se comprueba (número de precinto y **precinto intacto**); uno sin precinto recibe un precinto de la planta delante del apicultor (**Precinto de planta aplicado ahora**).
2. **El peso**: kilogramos netos (y, opcionalmente, brutos); la cantidad del lote es la suma de los envases.
3. **Los frascos de muestra**: se abre el envase y se llenan de él un **frasco de muestra** y un **frasco de contramuestra**, cada uno con su propio número de precinto y una **tapa de seguridad**. La tapa debe estar intacta (el botón abajo) cuando se precinta el frasco; una tapa saltada indica que el frasco se abrió, así que se desecha y se llena un frasco nuevo.
4. **Un precinto nuevo** cierra el envase después del muestreo.

Cada paso es un **evento de precinto** al que solo se pueden añadir entradas, nunca cambiarlas. El laboratorio recibe los frascos de muestra; las contramuestras se quedan en la planta durante el tiempo fijado en **Conservar hasta**.

**La confirmación del apicultor.** El apicultor recibe un SMS con la organización, la fecha, los envases, los kilos, el tipo de miel, el número de muestras y un código. Cuando se introduce el código, la entrega queda confirmada y se guarda su **huella del contenido**. Cambiar cualquier cosa después de enviar el código lo anula, y hace falta un código nuevo. Si el apicultor ya se ha ido, el responsable lo llama y el apicultor lee el código en su teléfono. Sin SMS (sin teléfono, sin cobertura), el responsable sube el **formulario de entrega firmado** con un motivo: se puede trabajar con el lote, pero solo se puede finalizar después de que el Responsable de cumplimiento de la planta acepte la foto.

<!-- SHOT: intake-containers -->
Figure: Registro de una entrega: una fila por envase con sus precintos y sus frascos de muestra.

**Etiquetas.** La página de la entrega imprime las etiquetas con códigos QR: una por envase (`L{lot}-C{n}`), una por frasco de muestra (`S-{lot}-C{n}`) y una por contramuestra (`…-K`), en una hoja A4 (2 × 5) o una etiqueta de 100 × 50 mm por página.

### Recepción en el laboratorio {#flow-reception}

Para cada frasco, el laboratorio registra tres comprobaciones: **El número de precinto coincide con el registro**, **La etiqueta está intacta** y **La tapa está intacta (botón abajo)**. Si alguna respuesta es **No**, el frasco se rechaza con un motivo. La planta toma entonces un **frasco nuevo del mismo envase** (**Abrir, muestrear y precintar de nuevo**), lo que rompe de nuevo el precinto del envase y aplica uno nuevo; ambos se ven en el historial de precintos. El contenido confirmado de la entrega conserva el primer frasco.

### Muestra compuesta o por envase {#flow-analysis}

La plataforma admite dos formas de analizar un lote; el ajuste de la plataforma decide cuál se usa:

- **Compuesta (predeterminada).** Cuando se ha recibido en buen estado el frasco de cada envase, el laboratorio prepara **una muestra compuesta** con partes iguales de los frascos (**Hacer la muestra compuesta**, código `S-{lot}-COMP`) y la analiza. Su informe aprobado completa el PCC 1 del lote. Los frascos individuales se conservan; si la muestra compuesta no cumple, o a petición, se analiza un solo frasco por separado. Un informe de un solo frasco así se queda con su frasco y no sustituye los valores del PCC 1 del lote.
- **Por envase.** Cada frasco se analiza por separado, y cada informe aprobado de un frasco cuenta para el lote.

### La regla del peor valor {#flow-worst}

Cuando los frascos se analizan uno por uno, el valor del PCC 1 del lote para cada parámetro es el **peor** valor de los envases:

- primero, un valor fuera de sus límites, el que más se aleje;
- si no, el valor más cercano al límite, en la dirección que importa (el más alto donde hay un máximo, el más bajo donde hay un mínimo);
- sin límite, el valor más alto;
- las respuestas de texto se conservan todas, cada una con su envase.

El informe de laboratorio y el informe de retiro muestran los valores de cada envase ("Valores de laboratorio por envase"), con el valor del lote en negrita.

### Correcciones de los datos de una entrega {#flow-corrections}

Una vez confirmada una entrega, sus datos (fecha, tipo y peso de los envases, números de precinto) quedan bloqueados, también para los administradores. Un error lo corrigen **dos administradores de la plataforma**: uno solicita la corrección con un motivo y un segundo la confirma (quien la solicita nunca decide). El contenido original y el corregido siguen legibles en la página de la entrega, con sus huellas, y el apicultor recibe un SMS informativo. Una corrección nunca añade ni quita envases.

### Por qué el PCC 0 va antes del análisis de laboratorio {#why-ccp0-first}

El PCC 0 es la auditoría primaria en la recepción: ¿esta miel es aceptable? Por eso el laboratorio no puede guardar, enviar ni aprobar resultados hasta que el PCC 0 del lote esté completado **y aprobado**. Hasta entonces, la tarea de laboratorio muestra "El PCC 0 (auditoría primaria) de este lote aún no está completo; los resultados de laboratorio se registran y aprueban solo después." Mientras tanto, sí se puede devolver un informe.

### Por qué se necesita el PCC 10 de la partida antes de sellarla {#why-ccp10}

La finalización sella la partida solo cuando todos los PCC asignados a ella están aprobados. El PCC 10 de la partida registra las condiciones de almacenamiento en la propia planta, de modo que el registro de almacenamiento de la planta debe existir antes del sellado. Cada poseedor posterior lleva su propio registro de almacenamiento PCC 10 para sus existencias.

### Por ahora, solo el administrador crea partidas {#batch-admin-only}

Un administrador de la plataforma crea la partida, elige sus lotes y cantidades, define la organización propietaria y asigna los PCC de la partida. Después, el personal de la planta ingresa los PCC de la partida. El proyecto decidirá si el personal de la planta podrá crear partidas por sí mismo.

### Entregas registradas antes de los envases {#flow-legacy}

Las entregas registradas antes del procedimiento con envases (o mientras una planta aún trabaja sin él) se marcan como **anterior (envases no registrados)**: tienen un formulario de entrega firmado y una sola muestra con un solo control de precinto. Siguen siendo válidas y se finalizan como antes.

## Qué bloquea qué: resumen {#flow-blocks}

- **No hay entrega sin** una organización de Procesamiento verificada, un rol con el derecho de recepción y un apicultor registrado (un registro con foto, solo después de que el Responsable de cumplimiento la haya aceptado).
- **No hay entrega confirmada sin** el código SMS del apicultor para exactamente este contenido, o una foto firmada con un motivo.
- **No hay resultados de laboratorio sin** un frasco recibido que haya superado las comprobaciones de precinto, etiqueta y tapa, y un PCC 0 aprobado.
- **No puede aprobar un informe** la persona que ingresó los resultados.
- **Ningún lote entra en una partida** hasta que todos sus PCC estén completados o sean no aplicables y estén aprobados, y el lote pertenezca al mayorista de la partida.
- **No hay finalización** mientras la partida no tenga miel, falte un PCC de la partida o no esté aprobado, haya un PCC de lote abierto o una foto de entrega espere al Responsable de cumplimiento; la lista "Falta antes de finalizar:" indica cada problema.
- **No puede confirmar la finalización** la persona que la solicitó.
- **No hay envío** de una partida que no esté finalizada, ni de una cantidad mayor que la que usted tiene en su poder.
- **No hay cambios** en una partida finalizada ni en sus lotes, ni en los datos de una entrega confirmada, salvo mediante una corrección hecha por dos administradores.
