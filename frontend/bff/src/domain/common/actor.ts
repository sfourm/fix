/** Quem está agindo sobre um recurso salvo, já com a membership na organização verificada. */
export interface Actor {
  userId: string;
  organizationId: string;
  /** Pode gerenciar recursos públicos da organização mesmo sem ser o dono (role edit_organization). */
  canManageShared: boolean;
}
