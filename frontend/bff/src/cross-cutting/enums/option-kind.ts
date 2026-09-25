export const OPTION_KINDS = ['Call', 'Put'] as const;

export type OptionKind = (typeof OPTION_KINDS)[number];
