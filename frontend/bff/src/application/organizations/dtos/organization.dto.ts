export interface OrganizationDto {
  id: string;
  name: string;
  slug: string;
  /** Organização nativa da FIX (equipe interna). */
  isInternal: boolean;
  /** O usuário vê a organização pelo acesso interno FIX, sem ser membro (suporte: vê e edita, não decide). */
  internalAccess: boolean;
}
