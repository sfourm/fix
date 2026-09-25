/** Quem executa a ação e em qual organização (enviado ao core em todo request gRPC). */
export interface RequestContext {
  userId: string;
  organizationId?: string;
}
