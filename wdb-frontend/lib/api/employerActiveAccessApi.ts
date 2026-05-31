const BASE_URL = `${process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5258'}/api/Employer/active-access`;

export interface ActiveAccessItem {
  permissionId: string;
  label: string;
  type: 'text' | 'file';
  isCustom: boolean;
}

export interface ActiveAccessCategory {
  name: string;
  items: ActiveAccessItem[];
}

export interface EmployerActiveAccess {
  requestId: string;
  workerId: string;
  workerName: string;
  workerEmail: string;
  reason: string;
  grantedAt: string;
  expiryDate: string;
  categories: ActiveAccessCategory[];
}

export interface AccessViewResult {
  type: 'text' | 'file';
  value: string | null;
  url: string | null;
  urlExpiresAt: string | null;
}

// Carries the HTTP status code so the modal can show a tailored message.
export class ViewAccessError extends Error {
  status: number;
  detail: string;
  constructor(status: number, detail: string) {
    super(`View failed (${status})${detail ? `: ${detail}` : ''}`);
    this.status = status;
    this.detail = detail;
  }
}

export async function fetchActiveAccess(token: string): Promise<EmployerActiveAccess[]> {
  const res = await fetch(BASE_URL, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });
  if (!res.ok) {
    throw new Error(`Failed to load active access (${res.status})`);
  }
  return res.json();
}

export async function viewAccessItem(
  token: string,
  permissionId: string,
): Promise<AccessViewResult> {
  const res = await fetch(`${BASE_URL}/${permissionId}/view`, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });
  if (!res.ok) {
    let detail = '';
    try {
      const body = await res.json();
      detail = body?.message ?? body?.error ?? '';
    } catch {
      // ignore: error response had no JSON body
    }
    throw new ViewAccessError(res.status, detail);
  }
  return res.json();
}
