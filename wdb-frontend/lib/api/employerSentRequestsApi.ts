const BASE_URL = `${process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5258'}/api/Employer`;

export interface EmployerSentRequestItem {
  permissionId: string;
  categoryName: string;
  label: string;
  // 0 Pending, 1 Approved, 2 Rejected, 3 Revoked
  status: number;
  isCustom: boolean;
}

export interface EmployerSentRequest {
  requestId: string;
  workerId: string;
  workerName: string;
  workerEmail: string;
  reason: string;
  expiryDate: string;
  createdAt: string;
  lastUpdatedAt: string;
  customRequest: string | null;
  customRequestStatus: 'pending' | 'approved' | 'rejected' | null;
  items: EmployerSentRequestItem[];
}

export async function fetchSentRequests(token: string): Promise<EmployerSentRequest[]> {
  const res = await fetch(`${BASE_URL}/sent-requests`, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });
  if (!res.ok) {
    throw new Error(`Failed to load sent requests (${res.status})`);
  }
  return res.json();
}
