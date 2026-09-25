export interface GroupResponse {
  id: string;
  name: string;
  isDefault: boolean;
  rules: string[];
  memberIds: string[];
}
