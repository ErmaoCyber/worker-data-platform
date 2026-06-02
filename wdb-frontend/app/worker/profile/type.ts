export interface WorkerProfileCategory {
  category: string;
  fields: WorkerProfileField[];
}

export interface WorkerProfileField {
  infoId?: string | null;
  fieldId?: string | null;
  label: string;
  type: 'text' | 'file' | string;
  value?: string | null;
  isPreset: boolean;
  hasValue: boolean;
}

export interface UpdatePresetFieldRequest {
  fieldId: string;
  value?: string | null;
}

export interface CreateCustomFieldRequest {
  label: string;
  type: 'text' | 'file';
  value?: string | null;
}

export interface UpdateCustomFieldRequest {
  label?: string | null;
  value?: string | null;
}

/**
 * Legacy type kept for older profile components that are no longer used by
 * the new profile page but are still type-checked during Next.js build.
 *
 * TODO: Remove this after BasicProfileCard/UserInfoCard are refactored or deleted.
 */
export interface WorkerInfoItem {
  id?: string;
  workerId: string;
  desc: string;
  value: string;
}
