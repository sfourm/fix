export interface GroupResponse {
  id: string;
  name: string;
  isDefault: boolean;
  rules: string[];
  memberIds: string[];
  /** Grupo imediatamente acima no organograma; null só na raiz. */
  parentGroupId: string | null;
  /** Nível no organograma (0 = raiz). */
  depth: number;
}
