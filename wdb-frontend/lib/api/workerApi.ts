import type {
  CreateCustomFieldRequest,
  UpdateCustomFieldRequest,
  UpdatePresetFieldRequest,
  WorkerProfileCategory,
  WorkerProfileField,
} from '@/app/worker/profile/type';

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ??
  process.env.NEXT_PUBLIC_API_URL ??
  'http://localhost:5258';

const PROFILE_URL = `${API_BASE_URL}/api/worker/profile`;

async function readErrorMessage(response: Response, fallback: string) {
  try {
    const data = await response.json();
    return data?.message ?? fallback;
  } catch {
    return fallback;
  }
}

function authHeaders(token: string) {
  return {
    Authorization: `Bearer ${token}`,
    'Content-Type': 'application/json',
  };
}

export async function getWorkerProfile(
  token: string,
): Promise<WorkerProfileCategory[]> {
  const response = await fetch(PROFILE_URL, {
    method: 'GET',
    headers: authHeaders(token),
  });

  if (!response.ok) {
    throw new Error(
      await readErrorMessage(response, 'Failed to fetch worker profile.'),
    );
  }

  return response.json();
}

export async function updatePresetField(
  token: string,
  request: UpdatePresetFieldRequest,
): Promise<WorkerProfileField> {
  const response = await fetch(`${PROFILE_URL}/preset`, {
    method: 'PUT',
    headers: authHeaders(token),
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    throw new Error(
      await readErrorMessage(response, 'Failed to update preset field.'),
    );
  }

  return response.json();
}

export async function createCustomField(
  token: string,
  request: CreateCustomFieldRequest,
): Promise<WorkerProfileField> {
  const response = await fetch(`${PROFILE_URL}/custom`, {
    method: 'POST',
    headers: authHeaders(token),
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    throw new Error(
      await readErrorMessage(response, 'Failed to create custom field.'),
    );
  }

  return response.json();
}

export async function updateCustomField(
  token: string,
  infoId: string,
  request: UpdateCustomFieldRequest,
): Promise<WorkerProfileField> {
  const response = await fetch(`${PROFILE_URL}/custom/${infoId}`, {
    method: 'PUT',
    headers: authHeaders(token),
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    throw new Error(
      await readErrorMessage(response, 'Failed to update custom field.'),
    );
  }

  return response.json();
}

export async function deleteCustomField(
  token: string,
  infoId: string,
): Promise<void> {
  const response = await fetch(`${PROFILE_URL}/custom/${infoId}`, {
    method: 'DELETE',
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    throw new Error(
      await readErrorMessage(response, 'Failed to delete custom field.'),
    );
  }
}

/**
 * Legacy compatibility wrapper.
 * Some older components still import addWorkerProfile(desc, value, category).
 * For now, we map that call to the new custom field API.
 *
 * TODO: Remove this after old components are refactored or deleted.
 */
export async function addWorkerProfile(
  token: string,
  desc: string,
  value: string,
  category?: string,
): Promise<WorkerProfileField> {
  return createCustomField(token, {
    label: desc,
    type: 'text',
    value,
  });
}

/**
 * Legacy compatibility wrapper.
 * The old profile page used updateWorkerProfile(desc, value, category).
 * New code should use updatePresetField or updateCustomField instead.
 */
export async function updateWorkerProfile(
  token: string,
  desc: string,
  value: string,
  category?: string,
): Promise<WorkerProfileField> {
  return createCustomField(token, {
    label: desc,
    type: 'text',
    value,
  });
}
