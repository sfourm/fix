/** Tipos de upload: usuários, políticas, mandatos e boletas são processados linha a linha (só criação); documentos só armazenados. */
export const FILE_KINDS = ['Users', 'Policies', 'Mandates', 'Orders', 'Documents'] as const;

export type FileKind = (typeof FILE_KINDS)[number];
