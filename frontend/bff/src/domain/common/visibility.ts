/** Private: só o dono vê. Public: todos os membros da organização veem. */
export const VISIBILITIES = ['Private', 'Public'] as const;

export type Visibility = (typeof VISIBILITIES)[number];
