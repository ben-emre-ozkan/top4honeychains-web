# 10. Administrador de la organización {#role-org-admin}

## Su trabajo diario {#orgadmin-daily}

El administrador de la organización es el responsable de una organización en la plataforma. Usted:

- mantiene al día los documentos de la organización (licencia, acreditación, certificado fiscal, comprobante de registro);
- en una planta: lee entregas y muestras, lee el precio solicitado por los apicultores para los lotes que posee su organización, solicita la finalización, envía y confirma envíos;
- en un mayorista o un minorista: confirma envíos;
- en una asociación: registra y retira el consentimiento de los apicultores para una página pública del apicultor.

Las membresías y las capacidades de su organización los gestionan los administradores de la plataforma. Si un colega necesita acceso o un rol distinto, consúltelos. Cada organización necesita un **Responsable de cumplimiento**; en una organización pequeña puede ser usted, con una segunda membresía (consulte el [capítulo 13](#role-co)).

## La página de su organización {#orgadmin-org-page}

1. Abra **Mi organización** y haga clic en **Página de la organización**.
2. La página muestra el estado, las capacidades, los documentos, los datos del **Responsable del tratamiento**, el historial de verificación y los miembros de su organización.

Mientras la organización no tiene Responsable de cumplimiento, la página dice "Esta organización aún no tiene Responsable de cumplimiento. Cada organización nombra al menos uno (puede ser el administrador de la organización con una segunda membresía)." con la fecha en la que debe asignarse uno.

**Responsable del tratamiento.** El aviso de privacidad de la organización la nombra como responsable del tratamiento: su **Razón social**, **Contacto (correo o teléfono)** y **Dirección**. "falta" significa que los datos aún no se han introducido. Los mantienen al día el Responsable de cumplimiento o un administrador de la plataforma.

<!-- SHOT: org-details -->
Figure: Documentos de la organización con sus huellas SHA-256.

### Subir un documento {#orgadmin-documents}

1. En la sección de documentos, elija el tipo: **Licencia**, **Acreditación**, **Certificado fiscal**, **Comprobante de registro** u **Otro**.
2. Elija el archivo: PDF de hasta 10 MB, o JPG, PNG o WEBP de hasta 8 MB.
3. Haga clic en **Subir**: "Documento subido."

La plataforma almacena el archivo con su huella SHA-256. Los documentos no se pueden eliminar. Una organización nueva necesita al menos un documento para que un administrador de la plataforma pueda verificarla.

## Finalización y envíos {#orgadmin-plant}

En una planta, usted puede usar **Solicitar la finalización** y **Enviar** con una partida, igual que el Personal: consulte [Solicitar la finalización](#staff-request-finalize) y [Enviar una partida](#staff-ship). En un receptor, puede **Confirmar recepción** o **Rechazar**: consulte [Confirmar o rechazar un envío](#staff-receive).

## El precio solicitado de sus lotes {#orgadmin-price}

Los apicultores indican en su portal un precio solicitado por kg para cada entrega (consulte [Sus entregas y su precio](#portal-price)). En la página de la entrega de un lote que posee su organización, el recuadro **Precio solicitado** muestra el precio actual y su historial: "El precio por kg solicitado por el apicultor y su historial. No se muestra a otros roles, poseedores posteriores ni páginas públicas." Lo leen el administrador de la organización, el Responsable de cumplimiento y los administradores de la plataforma; nadie más. Una asociación solo lee los precios de sus miembros cuando el apicultor lo ha aceptado.

## Páginas de apicultores (asociaciones) {#orgadmin-storefronts}

Una asociación puede publicar una **Página del apicultor** pública para cada apicultor miembro que lo acepte por escrito. La página muestra solo datos verificados: el nombre del apicultor (solo con consentimiento; si no, un código neutro como `BK-…`), la provincia y el distrito, los tipos de miel, los lotes verificados y los resultados de laboratorio aprobados, y las partidas en las que se usó la miel. Nunca muestra un número de teléfono, una foto de la persona, la fecha de nacimiento ni la ubicación exacta de un apiario. Los visitantes se ponen en contacto con el apicultor a través de la asociación.

La página es pública solo cuando se cumplen **todas** estas condiciones:

- el apicultor está activo;
- un administrador de la plataforma ha verificado el número de registro de la explotación del apicultor;
- su asociación está verificada;
- su asociación (la asociación actual del apicultor) registró el consentimiento y este no se ha retirado.

### Registrar un consentimiento {#orgadmin-consent}

1. Lea al apicultor el texto del consentimiento y haga firmar el formulario.
2. Abra **Páginas de apicultores** y haga clic en **Registrar consentimiento** junto al apicultor.
3. Marque **El texto de consentimiento se leyó al apicultor y fue firmado.**
4. Marque **El apicultor acepta que se muestre su nombre (de lo contrario, se muestra un código neutro)** solo si el apicultor lo aceptó.
5. Ingrese la **Fecha del consentimiento** (no puede ser futura) y adjunte el **Documento de consentimiento firmado** (foto o PDF).
6. Guarde.

<!-- SHOT: consent-form -->
Figure: Registro del consentimiento de un apicultor en un teléfono.

El mensaje es "Consentimiento registrado; la página pública está publicada." o, si el número de registro aún no está verificado, "Consentimiento registrado. La página pública se publicará cuando se verifique el número de registro."

**Tarjeta imprimible.** **Tarjeta imprimible** imprime una tarjeta con el código QR de la página del apicultor, para el puesto de venta del apicultor.

### Retirar un consentimiento {#orgadmin-withdraw}

1. En **Páginas de apicultores**, ingrese si lo desea un **motivo (opcional)** y haga clic en **Retirar consentimiento**.
2. La página se retira de inmediato: "Consentimiento retirado; la página pública ya no es visible."

Errores: "Este apicultor ya tiene un consentimiento activo; retírelo primero para registrar uno nuevo.", "Confirme que se leyó el texto de consentimiento al apicultor.", "Adjunte el documento de consentimiento firmado."
