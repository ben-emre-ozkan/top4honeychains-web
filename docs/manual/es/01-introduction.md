# 1. Acerca de TOP4HoneyChains {#about}

> **Borrador, para revisión de los socios.**

TOP4HoneyChains es una plataforma web que sigue la miel desde el apiario del apicultor hasta el estante del minorista y conserva un registro verificable de cada paso. Se desarrolló en el proyecto TOP4HoneyChains ("Plataforma abierta confiable y sostenible para cadenas de valor inteligentes de la miel"), financiado en la convocatoria ICT-AGRI-FOOD 2022 por las agencias nacionales de financiación de los países socios.

Este manual describe la plataforma tal como funciona hoy, con el modelo de organizaciones activado. Está dirigido a todas las personas que inician sesión en el panel y a los administradores de la plataforma. Las páginas públicas, que no requieren cuenta, se describen donde corresponde.

## Cómo leer este manual {#how-to-read}

- **Capítulos 1–4**: explican la plataforma, las capacidades, las organizaciones y los roles, el inicio de sesión y el recorrido completo de una partida de miel. Léalos primero.
- **Capítulos 5–11**: un capítulo por rol. Cada uno empieza con "su trabajo diario" y luego explica cada tarea paso a paso. Lea el capítulo de su propio rol.
- **Capítulo 12**: para los administradores de la plataforma.
- **Capítulo 13**: explica el registro de auditoría y lo que puede leer el público.
- **Capítulo 14**: contiene un glosario y respuestas a problemas frecuentes.

Los nombres de botones, menús y campos aparecen en **negrita**, tal como se ven en pantalla. Los mensajes que muestra el sistema aparecen entre comillas. Las figuras y las tablas están numeradas; los índices de figuras y de tablas siguen al índice general. Los ejemplos usan registros de prueba cuyos nombres empiezan por **[DEMO]**; no son personas ni empresas reales.

> **Etiquetas que cambian en la próxima versión.** Algunas etiquetas de la pantalla cambiarán de nombre en la próxima versión de la plataforma. Este manual ya usa los nombres nuevos, por ejemplo el rol **Responsable de aseguramiento de la calidad** (hoy: "Aprobador de calidad") y las descripciones de capacidades que empiezan por "Capaz de …". Las capturas de pantalla muestran las etiquetas actuales. La [Table](#tbl-renamed-labels) del glosario enumera todos los nombres antiguos y nuevos.

> En español, la plataforma usa la abreviatura **PCC** (punto crítico de control) en lugar del término internacional CCP. Las demás versiones usan su propia abreviatura, tal como aparece en pantalla: CCP (inglés), KKN (turco), KPK (polaco), KKP (letón).

## La cadena de valor de la miel {#value-chain}

La miel pasa por cinco estaciones. La plataforma registra cada una:

1. **Apiario**: el apicultor produce la miel. Los apicultores no inician sesión y no tienen cuenta; cada apicultor está representado en la plataforma por un **registro de apicultor**, que mantienen al día los administradores de la plataforma y la asociación del apicultor.
2. **Entrega**: el apicultor entrega la miel a una planta de procesamiento. La planta registra la entrega, lo que crea un *lote de miel cruda*, y lo controla en la recepción (PCC 0).
3. **Laboratorio**: la planta precinta una muestra y la envía a un laboratorio de análisis de alimentos (normalmente acreditado). En el laboratorio, un analista ingresa los resultados del análisis y una segunda persona, el responsable de laboratorio, aprueba el informe.
4. **Procesamiento**: la planta procesa la miel (PCC 2–3 en el lote), combina lotes en una *partida* y registra los controles de procesamiento, llenado, etiquetado, envasado y almacenamiento (PCC 4–10 en la partida). Después, dos personas *finalizan* (sellan) la partida.
5. **Estantería**: la partida se envía a un mayorista o a un minorista. El receptor confirma la recepción y registra sus propias condiciones de almacenamiento.

En este manual, **datos** significa lo que las personas registran en la plataforma: valores medidos (como la humedad en %) y respuestas a preguntas (como Sí / No).

**El principio de los cuatro ojos.** Cada dato lo ingresa una persona y lo aprueba una segunda persona distinta; nadie puede aprobar lo que ha ingresado él mismo. Los dos "pares de ojos" hacen mucho menos probables los errores y las manipulaciones. Las reglas exactas figuran en [Reglas básicas](#fixed-rules). Además, cada cambio queda escrito en un registro de auditoría a prueba de manipulaciones.

<!-- SHOT: intro-chain -->
Figure: Las cinco estaciones de la cadena de valor de la miel, tal como aparecen en la página de inicio pública.

## Trazabilidad {#traceability}

Trazabilidad significa que, para cualquier tarro de miel, se pueden responder tres preguntas:

- **¿De dónde viene?** La partida conoce sus lotes de miel cruda, y cada lote conoce su apiario, su apicultor, su entrega y su informe de laboratorio.
- **¿Se controló?** Cada punto crítico de control del lote y de la partida tiene datos registrados, y consta quién los ingresó, quién los aprobó y cuándo.
- **¿Adónde fue?** Los envíos registran qué organización tiene en su poder qué cantidad de la partida.

Si surge un problema, el **Informe de retiro** muestra todos los lotes aguas arriba y todos los envíos aguas abajo de una partida o de un lote (consulte [Retiro](#admin-recall)).

## Para quién es la plataforma {#audience}

La [Table](#tbl-audience) enumera a todos los que usan la plataforma. Las organizaciones y sus roles se definen en el [capítulo 2](#roles).

Table: Para quién es la plataforma {#tbl-audience}

| Quién | Cuenta | Qué hacen en la plataforma |
|---|---|---|
| Apicultores | ninguna; representados por un **registro de apicultor** | Entregan la miel; su asociación registra su consentimiento para una página pública de apicultor |
| Asociaciones de apicultores | miembros de una organización | Vinculan a sus apicultores miembros y registran su consentimiento para una página pública de apicultor |
| Plantas de procesamiento y de envasado | miembros de una organización | Registran entregas, toman muestras, registran los puntos de control, solicitan y confirman la finalización, envían |
| Laboratorios de análisis de alimentos | miembros de una organización | Reciben muestras, ingresan resultados, aprueban informes |
| Mayoristas | miembros de una organización | Confirman envíos, registran las condiciones de almacenamiento, leen los registros del productor |
| Minoristas | miembros de una organización | Confirman envíos, registran las condiciones de almacenamiento, leen los registros del productor |
| Autoridades públicas | miembros de una organización | Leen los registros (solo lectura) |
| Administradores de la plataforma | rol global de una cuenta de usuario | Verifican organizaciones, gestionan membresías, definen puntos de control y límites, crean partidas, leen el registro de auditoría |
| Consumidores y otros visitantes públicos | ninguna | Leen las páginas públicas: la Historia de la miel desde el código QR del tarro, las páginas de apicultores, la verificación de informes |

## La Historia de la miel pública y los códigos QR {#honey-story}

Cuando se finaliza una partida, la plataforma crea su página pública **Historia de la miel** y un código QR que apunta a ella. El código QR puede imprimirse en la etiqueta. Cualquier visitante puede escanearlo sin cuenta.

La Historia de la miel muestra, en el idioma que el visitante elige en la página:

- el nombre de la partida, la cantidad y la fecha de producción;
- el recorrido de la miel: cosecha, entrega, laboratorio, procesamiento y estantería;
- el origen de la miel en un mapa: por ahora por provincia de Türkiye; están previstos los mapas de los demás países socios;
- los apicultores que aportaron miel, con un enlace a su página pública de apicultor cuando la han aceptado;
- los resultados de laboratorio, con un veredicto dentro/fuera del límite solo para los límites que ya están verificados;
- los puntos de control del procesamiento y del envasado;
- un enlace para verificar el informe de laboratorio.

La Historia de la miel nunca muestra el teléfono de un apicultor, la foto de una persona, la ubicación exacta de un apiario ni ninguna nota de texto libre que pudiera contener datos personales. Consulte [Lo que puede leer el público](#trust-public).

<!-- SHOT: public-honey-story -->
Figure: La Historia de la miel pública de una partida finalizada, en un teléfono.

Otras páginas públicas:

- **Página del apicultor** (`/storefront/<code>`): la página pública de un apicultor; se publica solo con el consentimiento firmado del apicultor y un número de registro verificado.
- **Verificar un informe de laboratorio** (`/verify-report`): cualquier persona puede ingresar un código de informe y leer si el informe existe y fue aprobado, y comparar un PDF con su huella.
- **Página de inicio** (`/`): la página principal del proyecto, con el botón para iniciar sesión.
