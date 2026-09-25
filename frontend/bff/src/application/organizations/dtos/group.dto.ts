export interface GroupDto {
  id: string;
  name: string;
  isDefault: boolean;
  rules: string[];
  memberIds: string[];
}
